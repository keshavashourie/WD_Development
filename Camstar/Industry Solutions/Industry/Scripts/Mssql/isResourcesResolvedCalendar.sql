--------------------------------------------------------------------------------
-- SCRIPT:isResourcesResolvedCalendar.sql
-- DESCR: Creates stored procedures and View used to create Resources Resolved Calendar 
-- This SCRIPT file is a utility funtionality for Camstar Applications
-- Module: OEE
-- Date: 3 August 2018 
-- By: Development\IS5 (Genoa-Dev)
-- © 2019 Siemens Product Lifecycle Management Software Inc.

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isResourcesDowntimeSchd' 
	   AND 	  type = 'V')
    DROP  VIEW isResourcesDowntimeSchd
GO


CREATE VIEW [isResourcesDowntimeSchd] AS 
------------------------------------------------------------------------------------------------------
-- This VIEW produces all Resources Downtime Schedule extended with Resource Family Downtime Schedule, 
-- beside it excludes any Family Downtime Schedule in overlapped. It is also used into procedures and 
-- other views for the ResourcesResolvedCalendar functionality. 
-------------------------------------------------------------------------------------------------------	
WITH Res AS (-- prepare Resource Downtime
    SELECT  rd.RESOURCENAME
            ,rf.RESOURCEFAMILYNAME 
            ,rd.RESOURCEID
	        ,rd.RESOURCEFAMILYID
            ,dt.isStartTime
            ,dt.isEndTime
            ,dt.isDowntimeScheduleID
            ,dt.CDOTypeId
     FROM ResourceDef rd
       LEFT JOIN ResourceFamily rf ON rd.RESOURCEFAMILYID = rf.RESOURCEFAMILYID
       INNER JOIN isDowntimeSchedule dt ON dt.PARENTID = rd.RESOURCEID 
	 WHERE isEndTime >= CONVERT(DATETIME,FLOOR(CONVERT(FLOAT,GETDATE()))) -30 --Avoid Schedule Expired from 30 days
	 --AND rd.isIncludeInOEE = 1  
), ResFam AS (-- extend Resource Family Downtime to Resource 
      SELECT rd.RESOURCENAME
            ,rf.RESOURCEFAMILYNAME 
            ,rd.RESOURCEID
	        ,rd.RESOURCEFAMILYID
            ,dt.isStartTime 
            ,dt.isEndTime
            ,dt.isDowntimeScheduleID
	        ,dt.CDOTypeId
       FROM ResourceDef rd
         INNER JOIN ResourceFamily rf ON rd.RESOURCEFAMILYID = rf.RESOURCEFAMILYID
         INNER JOIN isDowntimeSchedule dt ON dt.PARENTID = rd.RESOURCEFAMILYID 
		WHERE isEndTime >= CONVERT(DATETIME,FLOOR(CONVERT(FLOAT,GETDATE()))) -30 --Avoid Schedule Expired from 30 days
		--AND rd.isIncludeInOEE = 1 
),  Res_ResFam_Overlapped AS( -- Prepare Overlapped
	SELECT  r.RESOURCEID
           ,r.RESOURCENAME
           ,r.isStartTime 
           ,r.isEndTime   
           ,r.RESOURCEFAMILYNAME
           ,rf.isStartTime isStartTime_F
           ,rf.isEndTime   isEndTime_F
	       ,r.CDOTypeId
	       ,rf.CDOTypeId CDOTypeId_F
    	FROM Res r JOIN ResFam rf ON r.RESOURCEFAMILYID=rf.RESOURCEFAMILYID 
         AND  ((
               (r.isStartTime >= rf.isStartTime AND r.isStartTime < rf.isEndTime ) 
            OR (rf.isStartTime >= r.isStartTime AND  rf.isStartTime < r.isEndTime ) 
			  )
         OR NOT (rf.isStartTime >= r.isEndTime OR rf.isEndTime <= r.isStartTime ))
), FinalMerge AS (
    SELECT ResourceName,ResourceFamilyName,ResourceID,ResourceFamilyID,isStartTime,isEndTime,isDowntimeScheduleID,CDOTypeId
	 FROM Res
    UNION
    SELECT rf.ResourceName, rf.ResourceFamilyName, rf.ResourceID, rf.ResourceFamilyID, rf.isStartTime, rf.isEndTime, rf.isDowntimeScheduleID, rf.CDOTypeId 
	 FROM ResFam rf LEFT jOIN Res r ON r.ResourceID= rf.ResourceID
	  WHERE  rf.isStartTime NOT IN (SELECT isStartTime_F FROM Res_ResFam_Overlapped WHERE RESOURCEID = rf.RESOURCEID) --Exclude Overlapped
	    AND  rf.isEndTime   NOT IN (SELECT isEndTime_F   FROM Res_ResFam_Overlapped WHERE RESOURCEID = rf.RESOURCEID)  
)	SELECT * FROM FinalMerge 
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isGetShift' 
	   AND 	  type = 'P')
    DROP  PROCEDURE [isGetShift]
GO

CREATE PROCEDURE [isGetShift] ( @CalendarDate DATETIME OUT, 
								@CalendarShiftId CHAR(16) OUT,  
								@ShiftId char(16) OUT, 
								@ShiftName nvarchar(30) OUT, 
								@ShiftStart DATETIME OUT, 
								@ShiftEnd DATETIME OUT, 
								@isStartTime DATETIME OUT, 
								@isEndTime DATETIME OUT,
								@p_ResourceId CHAR(16) )
AS
BEGIN

---------------------------------------------------------------------------------------
-- This Procedure obtain Shift Calendar information starting from starttime date.
-- It is used into isSplitShiftByDay procedure.
---------------------------------------------------------------------------------------
--
--CASE  manageded for the Shifts Calendar.

--|-------------|
--|   SHIF 1    |							
--|-------------|	current ShiftEnd
--              |---------------|
--              |     SHIF 2    |
--              |---------------|
--                              |               |----------|
--                              | hole interval |  SHIF 3  |
--                              |               |----------|
--
-- Set the next StartTime to current ShfitEnd , so the first record of the 
-- ordered list by ShiftEnd of calendar it will be the next Shift. So are 
-- managed also holes between shift.					

	DECLARE @RowPos INT
	
	SELECT  @CalendarDate	=subqury.CalendarDate, 
			@CalendarShiftId=subqury.CalendarShiftId, 
			@ShiftId		=subqury.ShiftId, 
			@ShiftStart		=subqury.ShiftStart, 
			@ShiftEnd		=subqury.ShiftEnd, 
			@ShiftName		=subqury.ShiftName,
            @RowPos			=subqury.Rn
	FROM( SELECT cs.CalendarDate 	CalendarDate, 
				 cs.CalendarShiftId CalendarShiftId, 
				 cs.ShiftId 		ShiftId, 
				 cs.ShiftStart 		ShiftStart, 
				 cs.ShiftEnd 		ShiftEnd, 
				 s.ShiftName 		ShiftName, 
				 ROW_NUMBER() OVER (ORDER BY ShiftEnd ASC) Rn
           FROM Factory f 
			 INNER JOIN MfgCalendar 	mc 	ON mc.MfgCalendarId = f.MfgCalendarId
			 INNER JOIN CalendarShift 	cs 	ON mc.MfgCalendarId = cs.MfgCalendarId
			 INNER JOIN [Shift]		  	s 	ON s.ShiftId		= cs.ShiftId
			 INNER JOIN ResourceDef    	r 	ON r.FactoryId		= f.FactoryId
			 LEFT  JOIN ResourceFamily 	rf 	ON rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE ShiftEnd > @isStartTime
			  AND r.ResourceId = @p_ResourceId
			) subqury
	WHERE subqury.Rn=1;

END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isResourcesDwntSchd_Calendar' 
	   AND 	  type = 'V')
    DROP  VIEW isResourcesDwntSchd_Calendar
GO

CREATE  VIEW  [isResourcesDwntSchd_Calendar] AS
------------------------------------------------------------------------------------------------------------------------------------
-- This VIEW produces all Resources Downtime Schedule from the isResourcesDowntimeSchd View mapped into own calendar Shift.
-- It is also used into isResourceDntInsert procedure.
------------------------------------------------------------------------------------------------------------------------------------
WITH ResourcesCalendar AS (
SELECT f.FactoryName, mc.MfgCalendarName, cs.CalendarShiftId, cs.CalendarDate, cs.ShiftId, cs.ShiftStart, cs.ShiftEnd, r.ResourceId, r.ResourceName, rf.ResourceFamilyName, fm.isStartTime, fm.isEndTime, fm.CDOTypeId 
	FROM Factory f 
	INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
	INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
	INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
	LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
	INNER JOIN [isResourcesDowntimeSchd] fm ON fm.ResourceId = r.ResourceId
WHERE  FLOOR(CAST(cs.ShiftStart AS FLOAT)) = CAST (cs.CalendarDate AS FLOAT) -- Only for Downtimes Schedule
  AND fm.isStartTime >= ShiftStart AND  fm.isStartTime < ShiftEnd
) SELECT * FROM ResourcesCalendar
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isSplitShiftByDay' 
	   AND 	  type = 'P')
    DROP  PROCEDURE isSplitShiftByDay
GO

CREATE PROCEDURE [isSplitShiftByDay] (	@CalendarShiftId CHAR(16) OUT, 
										@ShiftStart DATETIME OUTPUT, 
										@ShiftEnd DATETIME OUTPUT, 
										@ShiftName NVARCHAR(30) OUTPUT,
										@CDOTypeId INT, 
										@CalendarShiftDate DATETIME OUT, 
										@isShiftId CHAR(16) OUT, 
										@isStartTime DATETIME OUT, 
										@isEndTime DATETIME OUT, 
										@p_ResourceId CHAR(16))
AS
BEGIN
-------------------------------------------------------------------------------------------------------------
-- This Procedure execute control for manage dowuntime schedule spanned per day, shift, startime and endtime.
-- It then produces the Resources Resolved Calendar into isResolvedDowntimeSchd TABLE
-- It is used from isResourceDntInsert procedure.
-------------------------------------------------------------------------------------------------------------
	DECLARE @isResolvedDowntimeSchdId CHAR(16)
	
	DECLARE @Changed 	INT 	= 0;
	DECLARE @this_day 	FLOAT 	= CAST(@CalendarShiftDate	AS FLOAT)
	DECLARE @cnt_day  	FLOAT 	= FLOOR(CAST(@isEndTime		AS FLOAT)) --set last day
	
	DECLARE  @MaxShiftEndTime DATETIME
	
	--the dowuntime schdedule can not be calculated farther than the maximum calendar date.
	SELECT   @MaxShiftEndTime=MAX(cs.ShiftEnd) 
	FROM Factory f 
		INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
		INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
		INNER JOIN [Shift]		  s ON s.ShiftId		= cs.ShiftId
		INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
		LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
	WHERE r.ResourceId =@p_ResourceId

	IF @MaxShiftEndTime < @isEndTime
		SET @isEndTime= @MaxShiftEndTime 

	DECLARE @init_isEndTime DATETIME= @isEndTime

	--While there is no change of the day, it work for any shift.
	--{...split Days...}
	WHILE (@this_day <= @cnt_day)
	BEGIN

		IF (@isEndTime = @init_isEndTime AND @Changed =1 ) BREAK -- it's over

		DECLARE @current_day FLOAT = @this_day
		--{...split Shifts...}
		WHILE @this_day = @current_day 
		BEGIN
			
			EXEC csiPRDGetNextInstanceId @CDOTypeId, @isResolvedDowntimeSchdId OUTPUT
			
			--it occurs if a downtime has a set endtime that falls between shifts
			SET @isEndTime =  CASE WHEN @ShiftEnd < @init_isEndTime THEN @ShiftEnd ELSE @init_isEndTime END

			IF (0<=CAST(@isEndTime - @isStartTime AS FLOAT))  -- it occurs if a downtime has a set end time that falls into an unplanned interval time (duration can not be negative)
				INSERT INTO	[isResolvedDowntimeSchd] ([CDOTypeId], [ChangeCount], [isCalDate], [isDuration], [isStartTime], [isEndTime], [IsFrozen], [isResolvedDowntimeSchdId], [isResolvedDowntimeSchdName],[isShiftId],[ResourceId]) 
					VALUES (@CDOTypeId
							, 1 
							, @CalendarShiftDate
							, CAST(@isEndTime - @isStartTime AS FLOAT)
							, @isStartTime
							, @isEndTime
							, 0
							, @isResolvedDowntimeSchdId
							, NEWID()
							, @isShiftId
							, @p_ResourceId)
				
			IF @isEndTime >= @init_isEndTime --finish
			BEGIN
				
				SET @this_day = @cnt_day

				BREAK
			END

			SET @isStartTime=@isEndTime --Get Shift by starttime

			--Get calendar information for resource downtime schedule. 
			EXEC isGetShift @CalendarShiftDate OUT, 
							@CalendarShiftId OUT, 
							@isShiftId OUT,
							@ShiftName OUT, 
							@ShiftStart OUT, 
						    @ShiftEnd OUT, 
							@isStartTime OUT, 
							@isEndTime OUT,
							@p_ResourceId   

			SET @current_day = COALESCE(FLOOR(CAST(@CalendarShiftDate AS FLOAT)), @this_day +1) 

			SET @isStartTime=@ShiftStart

			SET @Changed =1
		END
			   
		SET @this_day = @this_day + 1

	END

	RETURN @Changed
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isResourceDntInsert' 
	   AND 	  type = 'P')
    DROP  PROCEDURE isResourceDntInsert
GO

CREATE  PROCEDURE [isResourceDntInsert]( @p_ResourceId CHAR(16) = NULL,  
										 @p_CDODefId INT = 4841599)
AS
BEGIN  
------------------------------------------------------------------------------------------------------
-- This is entry point procedure. It work for produces Resource Resolved calendar for single RESOURCE.
------------------------------------------------------------------------------------------------------
	SET NOCOUNT ON

	DECLARE @FactoryName NVARCHAR(30)
	DECLARE @MfgCalendarName NVARCHAR(30)
	DECLARE @CalendarShiftId CHAR(16)
	DECLARE @CalendarDate DATETIME
	DECLARE @ShiftId CHAR(16)
	DECLARE @ShiftName NVARCHAR(30)
	DECLARE @ShiftStart DATETIME
	DECLARE @ShiftEnd DATETIME
	DECLARE @ResourceName NVARCHAR(30)
	DECLARE @ResourceFamilyName NVARCHAR(30)
	DECLARE @isStartTime DATETIME
	DECLARE @isEndTime DATETIME

	DECLARE cResDwtSchdCal CURSOR LOCAL FAST_FORWARD  FOR  
		SELECT CalendarDate, CalendarShiftId , ShiftId, ShiftStart, ShiftEnd, isStartTime, isEndTime 
		 FROM [isResourcesDwntSchd_Calendar] 
		WHERE [ResourceId]= @p_ResourceId 

	OPEN cResDwtSchdCal

	FETCH NEXT FROM cResDwtSchdCal
		INTO @CalendarDate, @CalendarShiftId, @ShiftId, @ShiftStart, @ShiftEnd, @isStartTime, @isEndTime 

	BEGIN TRY  
		BEGIN TRAN
	
		DELETE [isResolvedDowntimeSchd] WHERE ResourceId = @p_ResourceId

		WHILE @@FETCH_STATUS = 0
		BEGIN

			EXEC isSplitShiftByDay 	@CalendarShiftId OUT, 
									@ShiftStart OUTPUT, 
									@ShiftEnd OUTPUT, 
									@ShiftName OUTPUT,
									@p_CDODefId, 
									@CalendarDate OUT,  
									@ShiftId OUT,  
									@isStartTime OUT,
									@isEndTime OUT, 
									@p_ResourceId
		
			FETCH NEXT FROM cResDwtSchdCal
				INTO @CalendarDate, @CalendarShiftId, @ShiftId, @ShiftStart, @ShiftEnd, @isStartTime, @isEndTime  
	
		END 

		COMMIT TRAN

	END TRY  
	BEGIN CATCH  

		ROLLBACK TRAN

	END CATCH;   

	CLOSE cResDwtSchdCal

	DEALLOCATE cResDwtSchdCal

	RETURN	

END --1
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isResourcesDntInsertByFamily' 
	   AND 	  type = 'P')
    DROP  PROCEDURE isResourcesDntInsertByFamily
GO


CREATE PROCEDURE [isResourcesDntInsertByFamily] ( @p_ResourceFamilyId CHAR(16) =NULL, 
												  @p_CDOTypeId INT =4841599)
AS
BEGIN 
------------------------------------------------------------------------------------------------------------------------
-- This is entry point procedure. It work for produces Resource Resolved calendar for any RESOURCES of Resource Family.
------------------------------------------------------------------------------------------------------------------------
 	SET NOCOUNT ON
	DECLARE @ResourceId CHAR(16) 

	DECLARE cResDwtSchdCalFamily CURSOR LOCAL FAST_FORWARD  FOR  
		SELECT ResourceId 
		  FROM [ResourceDef]  
		WHERE [ResourceFamilyId]= @p_ResourceFamilyId 

	OPEN cResDwtSchdCalFamily 

	FETCH NEXT FROM cResDwtSchdCalFamily
		INTO @ResourceId

	WHILE @@FETCH_STATUS = 0
	BEGIN

		EXEC isResourceDntInsert @ResourceId, @p_CDOTypeId

		FETCH NEXT FROM cResDwtSchdCalFamily
			INTO @ResourceId

	END

END
GO

