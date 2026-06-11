--------------------------------------------------------------------------------
-- SCRIPT:isResourcesResolvedCalendar.sql
-- DESCR: Creates stored procedures and View used to create Resources Resolved Calendar 
-- This SCRIPT file is a utility funtionality for Camstar Applications
-- Module: OEE
-- Date: 3 August 2018 
-- By: Development\IS5 (Genoa-Dev)
-- © 2025 Siemens Product Lifecycle Management Software Inc.

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


IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isOEEGetStartTimeEndTime')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isOEEGetStartTimeEndTime
GO 

--Remarks:
-- On Camstar Designer 
-- CREATE FUNCTION isGetOEEValues(@pisResourceFamilyId char(16),@pisResourceId char(16) = '', @pTimeSpanType int = 1 , @pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE)

CREATE FUNCTION isOEEGetStartTimeEndTime (@ResourceId char(16), @TimeSpanType int, @pStartTimeDate As DateTime, @pEndTimeDate As DateTime, @pRefDateForShiftType As DateTime)
returns 
	@t TABLE(FromStartTime	DATETIME, ToEndTime DATETIME)
AS
Begin
	Declare @FromStartTime	DATETIME, @ToEndTime DATETIME

	Declare @ReferenceDate Datetime
	Set @ReferenceDate = @pRefDateForShiftType
	if (@ReferenceDate is NULL) 
	begin
		set @ReferenceDate = GetDate()
	end

	--Remarks: @TimeSpanType  is like isRealTimeOEEDuration in isOEESettings  (Real Time OEE Duration)
	--			Shift = 1	Day = 2 Week = 3

	if (@TimeSpanType is null OR @TimeSpanType <= 0 OR @TimeSpanType > 3)
	begin
		--TODO  manage the TimeSpan taking into account possible holes in the Calendar 
		-- PlannedProductionTime as the sum of shifts present between  @pStartTimeDate and @pEndTimeDate
		set @FromStartTime	= @pStartTimeDate
		set @ToEndTime		= @pEndTimeDate
	end
	else
	if (@TimeSpanType = 3)  --Week
	begin
		--TODO  manage Monday instead of Sunday
		--TODO  manage the StartTime of the first shift on Monday instead of midnight

		Declare @Sunday DateTime = (DateAdd(d, - (DatePart(dw,@ReferenceDate) -1), @ReferenceDate))
		--set @FromStartTime	= @Sunday
		set @FromStartTime	= DATEADD(d,0,DATEDIFF(d,0,@Sunday)) --Date midnight time
		set @ToEndTime		= @ReferenceDate
	end
	else
	if (@TimeSpanType = 2)  --Day
	begin
		--TODO  manage the StartTime of the first shift  instead of midnight
		set @FromStartTime	= DATEADD(d,0,DATEDIFF(d,0,@ReferenceDate)) --Date now midnight time
		set @ToEndTime		= @ReferenceDate
	end
	else
	if (@TimeSpanType = 1)  --Shift
	begin
		DECLARE @MfgCalendarId char(16) = 
			(SELECT MfgCalendarId FROM Factory 
				where FactoryId = (SELECT FactoryID from ResourceDef where ResourceId = @ResourceId and FactoryID is not null UNION SELECT  FactoryId from ResourceDef where ResourceId = (SELECT ParentResourceId from ResourceDef where ResourceId = (select ParentResourceId from ResourceDef where ResourceId = @ResourceId and FactoryLevel =3)) and FactoryId IS NOT NULL))
		--Declare @ShiftName VARCHAR(30) = ''
					
		if (@MfgCalendarId is NOT NULL)
		begin
			SELECT top(1)
				--@CalendarShiftId = cs.CalendarShiftId 
				@FromStartTime = cs.ShiftStart, @ToEndTime = cs.ShiftEnd
				--, @NonScheduledTime = cs.isNonScheduledTime --@NonScheduledTimeInMinutes =  ROUND(cs.isNonScheduledTime*1440, 0) 
				--, @ShiftName = s.ShiftName
			FROM CalendarShift cs INNER JOIN [Shift] s ON s.shiftid = cs.shiftid
			Where	MfgCalendarId = @MfgCalendarId and 
					@ReferenceDate BETWEEN cs.ShiftStart AND cs.ShiftEnd
		end
	end

	if (@FromStartTime is NULL)
	begin 
		set @FromStartTime = @ReferenceDate
	end
	if (@ToEndTime is NULL)
	begin 
		set @ToEndTime = @ReferenceDate
	end
	if (@ToEndTime > @ReferenceDate)
	begin
		set @ToEndTime = @ReferenceDate			
	end

    INSERT INTO @t Values(@FromStartTime, @ToEndTime)
    RETURN 

end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetResolvedDowntimeSchdTimespan')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetResolvedDowntimeSchdTimespan
GO 

CREATE FUNCTION isGetResolvedDowntimeSchdTimespan (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS int 
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	DECLARE @tot int = 0;
	DECLARE @StartTime DATETIME, @EndTime DATETIME; --@CalDate DATETIME, 

	DECLARE db_cursor CURSOR LOCAL FOR 
		select isStartTime,isEndTime from isResolvedDowntimeSchd --isCalDate,isDuration,isShiftId,isResolvedDowntimeSchdId
		where 
			ResourceId = @ResourceId
			AND (
					(isStartTime <= @FromStartTime and isEndTime > @FromStartTime) OR
					(isStartTime BETWEEN @FromStartTime AND @ToEndTime) OR
					(isEndTime	 BETWEEN @FromStartTime AND @ToEndTime)
				)

	OPEN db_cursor  
	FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime
	
	DECLARE @currTimeSpan int = 0;
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		if (@StartTime <= @FromStartTime) 
		begin
			set @StartTime = @FromStartTime
		end

		if (@EndTime > @ToEndTime)
		begin
			set @EndTime = @ToEndTime
		end

		set @currTimeSpan = DATEDIFF(second, @StartTime, @EndTime)

		set @tot = @tot + @currTimeSpan;

		--Print	'ResourceId ' + @ResourceId + ' [ dt ' + CONVERT(VARCHAR,@currTimeSpan) + 'min]' + 
		--		' [StartTime ' + LEFT(CONVERT(VARCHAR, @StartTime, 121), 23) + ']' + '[EndTime ' + LEFT(CONVERT(VARCHAR, @EndTime, 121), 23) + ']'
		--		+ ' [ tot dt ' + CONVERT(VARCHAR,@tot) + 'min]' ;

		FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime
	END 

	CLOSE db_cursor  
	DEALLOCATE db_cursor 

    RETURN @tot;
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEEResourceStatusHistoryVirtualRecord]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEResourceStatusHistoryVirtualRecord
GO 
CREATE FUNCTION isGetOEEResourceStatusHistoryVirtualRecord(
		@ResourceId		char(16),
		@FromStartTime	DATETIME,
		@ToEndTime		DATETIME,
		@pRunDate		DATETIME,
		@GetIfOnlyFailure	int,
		@GetResourceStatusReason int
	)
RETURNS @t TABLE(
						ResourceId char(16), ResourceName nvarchar(36),
						FromStatusChangedate datetime, ToStatusChangedate datetime,
						isOEELossCategory int,
						DurHours float,DurMin float, DurSec int,
						FromStatus nvarchar(36), ToStatus nvarchar(36),
						FromReason nvarchar(36), ToReason nvarchar(36)
						)
AS
Begin
	-- -----------------------------------------------------------------------------
	-- Last Record management ------------------------------------------------------
	-- -----------------------------------------------------------------------------
	 --REMARKS: 
	 --			We want to find the last record BOTH if the user set the machine in failure (down) OR RUNNING
	 --			either coming from a running or another failure status 
	 --			therefore the check is ON isOEELossCategory and NOT on isOldOEELossCategory 
	 --			(due to the fact that this is the last record this is also the current status,
	 --			 the last record is closed as the previous ones but we have virtually a record that starts from the last record and has no ending)
	declare @LastResourceId				varchar(36), @LastResourceName			varchar(36)
	declare @LastFromResourceStatus		varchar(36), @LastToResourceStatus		varchar(36)
	declare @LastFromStatusChangedate	datetime,	 @LastToStatusChangedate	datetime
	declare @isOldOEELossCategory		int,		 @isOEELossCategory			int
	declare @LastFromReason				varchar(36), @LastToReason				varchar(36)
	SELECT TOP(1)
		@LastResourceId				= ResourceId,						@LastResourceName		= ResourceName, 
		@LastFromStatusChangedate	= rsh.oldlaststatuschangedate,		@LastToStatusChangedate = rsh.laststatuschangedate,
		@LastFromResourceStatus		= oldrsc.ResourceStatusCodeName,	@LastToResourceStatus	= newrsc.ResourceStatusCodeName,

		@LastFromReason				= oldrsr.ResourceStatusReasonName,	@LastToReason			= newrsr.ResourceStatusReasonName,

		@isOldOEELossCategory		= rsh.isOldOEELossCategory, --necessary to know if we are coming from a running or another failure
		@isOEELossCategory          = rsh.isOEELossCategory		--necessary to know if we are running or not
	FROM 
		ResourceDef R	INNER JOIN		ResourceStatusHistory RSH	ON R.ResourceId = RSH.HistoryId
						LEFT OUTER JOIN ResourceStatusCode	 oldrsc ON oldrsc.ResourceStatusCodeId	= rsh.OldResourceStatusCodeId
						LEFT OUTER JOIN ResourceStatusCode	 newrsc ON newrsc.ResourceStatusCodeId	= rsh.ResourceStatusCodeId
						LEFT OUTER JOIN ResourceStatusReason oldrsr ON oldrsr.ResourceStatusReasonId = rsh.OldResourceStatusReasonCodeId
						LEFT OUTER JOIN ResourceStatusReason newrsr ON newrsr.ResourceStatusReasonId = rsh.ResourceStatusReasonCodeId

		WHERE 
			RSH.HistoryId = @ResourceId
			and  (R.isIncludeInOEE = 1)
			and (	newrsc.ResourceStatusCodeName is not NULL OR oldrsc.ResourceStatusCodeName is not NULL)
			and (@GetResourceStatusReason = 0 OR 
					(
						@GetResourceStatusReason = 1 AND
						(	newrsr.ResourceStatusReasonName is not NULL OR oldrsr.ResourceStatusReasonName is not NULL)
					)
				)
			order by rsh.laststatuschangedate DESC

	--This record is important ONLY if after the end of the closed timespan the machine is in failure
	-- If the machine was running, the closed timespan is not interesting for us
	-- If the machine was already in failure, the closed timespan has already been managed by the previous general query
	--	----------------------------------------||Running-----------------------Failure||--------------------
	--	----------------------------------------||Failure-----------------------Failure||--------------------
	if (
		(
			(@GetIfOnlyFailure = 1 and @isOEELossCategory = 1) OR @GetIfOnlyFailure = 0
		)
		AND 
			(@LastToStatusChangedate is not null AND @LastToResourceStatus is not null and @LastToResourceStatus <> '')
		AND 
		(
			(@GetResourceStatusReason = 1 AND @LastToReason is not null and @LastToReason <> '') OR @GetResourceStatusReason = 0
		)
	  )
	begin
		DECLARE @dtFrom DATETIME = @FromStartTime, @dtTo DATETIME = @ToEndTime
		declare @ManageLastRecord int = 1 
		
		--@offset is useful to manage the difference of time between the camstar server and the database server 
		--but is still to be fixed 
		--if (@pRunDate is not null) 
		--begin
		--	--declare @offset int = DateDiff(second, @pRunDate, GetDate())
		--	--if (@offset <> 0) 
		--	--begin				
		--	--	select  @dtTo = DATEADD(second,@offset,@dtTo)
		--	--end
		--end
		
		-- LAST RECORD = FIRST RECORD
		--  @LastFromStatusChangedate NULL								  @LastToStatusChangedate NOT NULL
		--	---------------------------------------------------------------Failure||-------------------------------------------------
		--	---------------------------------------------------------------Running||-------------------------------------------------
		--	USE CASE 1 --------------||dtFrom-------------dtTo||------------------------------------------------------------------------
		--	USE CASE 2 ------------------------------------------  ||dtFrom-------------dtTo||----------------------------------------
		--	USE CASE 3 ----------------------------------------------------------------------  ||dtFrom-------------dtTo||-----------
		--The last record is also the first record and the only one for this resource
		if (@LastFromStatusChangedate is null)
		begin
			--Ignore running machine (before LastToStatusChangedate it is running by default)
			if (@isOEELossCategory is NULL OR  (@isOEELossCategory is NOT NULL and @isOEELossCategory <> 1))
			begin	
				set @ManageLastRecord = 0
			end
			else
				 if (@dtTo   <= @LastToStatusChangedate)	 set @ManageLastRecord = 0
			else if (@dtFrom <= @LastToStatusChangedate and @dtTo >= @LastToStatusChangedate )  set @dtFrom  = @LastToStatusChangedate; 
			-- Machine in failure
			--else if (@dtFrom >  @LastToStatusChangedate) 							
		end
		else
		--===================The last record is NOT also the first record =======================================
		--	----------------------------------------||Running-----------------------Failure||--------------------
		--	----------------------------------------||Failure-----------------------Failure||--------------------
		--	-------||dtFrom-------------dtTo||-------------------------------------------------------------------
		if (@dtTo <= @LastFromStatusChangedate OR 
		--	----------------------------------------||Running-----------------------Failure||--------------------
		--	----------------------------------------||Failure-----------------------Failure||--------------------
		--	---------------------------------------------------||dtFrom------dtTo||------------------------------
			(
				(@dtFrom between @LastFromStatusChangedate and @LastToStatusChangedate) AND @dtTo <= @LastToStatusChangedate)	--the request is in the middle of the last record (From side)
			)
		--	----------------------------------------||Running-----------------------Failure||--------------------
		--	----------------------------------------||Failure-----------------------Failure||--------------------
		--	----------------------------||dtFrom-------------------dtTo||------------------------------
			OR 
			(
				@dtFrom < @LastFromStatusChangedate and (@dtTo between @LastFromStatusChangedate and @LastToStatusChangedate)  --the request is in the middle of the last record (To side)
			)
			begin
				set @ManageLastRecord = 0	
			end
		else
		--	----------------------------------------||Running-----------------------Failure||--------------------
		--	----------------------------------------||Failure-----------------------Failure||--------------------
		--	----------------------------||dtFrom--------------------------------------------------dtTo||---------
		if (@dtFrom <= @LastFromStatusChangedate and @dtTo >= @LastToStatusChangedate) --the request encompasses the last record
		begin
			set @dtFrom = @LastToStatusChangedate;
		end
		else
		--	----------------------------------------||Running-----------------------Failure||--------------------
		--	----------------------------------------||Failure-----------------------Failure||--------------------
		--	-------------------------------------------------------------------------------------||dtFrom-------------------dtTo||---------
		if (@dtFrom >= @LastToStatusChangedate) --the request is after last record (we are always in failure mode)
		begin
			--all already set, but I set the next variable because of the Sql compiler
			set @ManageLastRecord = 1 
		end

		if (@dtFrom = @dtTo)
		begin
			set @ManageLastRecord = 0
		end

		if (@ManageLastRecord = 1 )
		begin
			declare @LastDurHours float, @LastDurMin float, @LastDurSec int, @LastDurSecMaxPrec Bigint

			set @LastDurHours =  convert(decimal(10,2), datediff(second, @dtFrom, max(@dtTo)) / 3600.0)
			set @LastDurMin   = convert(decimal(10,2), datediff(second, @dtFrom, max(@dtTo)) / 60.0)
			set @LastDurSec   = datediff(second, @dtFrom, max(@dtTo))
			set @LastDurSecMaxPrec = Convert(Bigint,Datediff(second, @dtFrom, max(@dtTo)))*1000 

			insert into @t	(	ResourceId, ResourceName,
								FromStatusChangedate, ToStatusChangedate,
								isOEELossCategory,
								DurHours, DurMin, DurSec, 
								--DurSecMaxPrec,
								FromStatus, ToStatus,
								FromReason,	ToReason
							)
					values		(@ResourceId, @LastResourceName,
								@dtFrom, @dtTo,		--it is a new record closed on user "To" selection
								@isOEELossCategory,
								@LastDurHours, @LastDurMin, @LastDurSec, 
								@LastToResourceStatus,	@LastToResourceStatus,
								@LastFromReason,		@LastToReason
								)  
		end
	end

	RETURN
end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEELossCategoryLastRecordTimespan')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEELossCategoryLastRecordTimespan
GO 

CREATE FUNCTION isGetOEELossCategoryLastRecordTimespan (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS int 
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	declare  @LastNotAvailabileTimespan int = 0
	Declare @maxlaststatuschangedate Datetime = null 
	Declare @oldlaststatuschangedate Datetime = null 
	
		Declare @isOEELossCategoryLastRecord int = null 
		Declare @maxlaststatus Datetime = null 	
		SELECT top(1) @maxlaststatus = laststatuschangedate, @isOEELossCategoryLastRecord = isOEELossCategory
			FROM ResourceStatusHistory rsh 
			INNER JOIN  ResourceDef R ON R.ResourceId = rsh.HistoryId
			WHERE 
			r.ResourceId = @ResourceId
			and r.isIncludeInOEE = 1 
			and laststatuschangedate < @ToEndTime
			order by laststatuschangedate DESC
		
		-- I am in running after laststatuschangedate 
		if (@isOEELossCategoryLastRecord IS NULL)  
		begin
			return 0
		end

		SELECT @maxlaststatuschangedate = max(laststatuschangedate), @oldlaststatuschangedate = max(oldlaststatuschangedate)
			FROM ResourceStatusHistory rsh 
			INNER JOIN  ResourceDef R ON R.ResourceId = rsh.HistoryId
			WHERE 
			r.ResourceId = @ResourceId    --FILTER BY EQUIPMENT -==========================================================
			and r.isIncludeInOEE = 1 
			and RSH.isOldOEELossCategory IS NULL 
			and RSH.isOEELossCategory = 1
			--and @ToEndTime > @maxlaststatuschangedate

		--	If last status change was to isOEELossCategory = 1 - use that date
		IF (@maxlaststatuschangedate is NULL and @isOEELossCategoryLastRecord = 1)
		BEGIN
			SET @maxlaststatuschangedate = @maxlaststatus
		END

		--if (@oldlaststatuschangedate is NULL) 
		-- means that there is only one record 
		if (@maxlaststatuschangedate is not NULL) 
		begin
			--print 'maxlaststatuschangedate  ' + CONVERT(VARCHAR, @maxlaststatuschangedate, 121) --2018-11-14 18:00:00.000

			if (@ToEndTime > @maxlaststatuschangedate) 
			begin				
				--declare  @NrRecs int = 0
				--SELECT @NrRecs = count(*) FROM ResourceStatusHistory rsh 
				--WHERE 
				--rsh.HistoryId = @ResourceId --FILTER BY EQUIPMENT -==========================================================
				--and RSH.isOldOEELossCategory IS NULL 
				--and RSH.isOEELossCategory = 1

				--if (@oldlaststatuschangedate is NOT NULL)
				--begin
					set @LastNotAvailabileTimespan = DATEDIFF(second, @maxlaststatuschangedate, @ToEndTime) 
				--end
				--print 'LastNotAvailabileTimespan ' + convert(varchar, @LastNotAvailabileTimespan)
			end
		end


    RETURN @LastNotAvailabileTimespan;
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEELossCategoryAvailabilityTimespan')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEELossCategoryAvailabilityTimespan
GO 

CREATE FUNCTION isGetOEELossCategoryAvailabilityTimespan (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS int 
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	DECLARE @totMin int = 0, @totSec int = 0
	DECLARE @oldlaststatuschangedate DATETIME, @laststatuschangedate DATETIME
	DECLARE @isOldOEELossCategory int, @isOEELossCategory int 
	DECLARE @DurationMin int , @DurationSec int 

	DECLARE db_cursor CURSOR LOCAL FOR 
		SELECT
			rsh.oldlaststatuschangedate,
			rsh.laststatuschangedate,
			rsh.isOldOEELossCategory,
			rsh.isOEELossCategory,
			CASE 
			  WHEN rsh.oldlaststatuschangedate IS NULL THEN 0
			  ELSE DATEDIFF(minute, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 
			END as DurationMin
			,CASE 
			  WHEN rsh.oldlaststatuschangedate IS NULL THEN 0
			  ELSE DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 
			END as DurationSec

        FROM ResourceDef R
        INNER JOIN ResourceStatusHistory rsh ON R.ResourceId = rsh.HistoryId
        WHERE 
		--==============================================================================================================
		r.ResourceId = @ResourceId    --FILTER BY EQUIPMENT -==========================================================
		and r.isIncludeInOEE = 1 
		and (
				(RSH.oldlaststatuschangedate is NULL and RSH.isOEELossCategory = 1) OR
				RSH.isOldOEELossCategory = 1
			)

		-- USE isOldOEELossCategory INSTEAD OF isOEELossCategory BECAUSE THE DURATION of the Failure depends on the interval between OLD AND NEW STATUS 
		-- (From Failure Status to Running Status contains the duration of Failure Status)
		--and RSH.isOldOEELossCategory = 1   --FILTER BY OEE Loss Category Availability -====================================
		--AND (
		--		(ISNULL(rsh.oldlaststatuschangedate, @FromStartTime) <= @FromStartTime and rsh.laststatuschangedate > @FromStartTime) OR
		--		(ISNULL(rsh.oldlaststatuschangedate, @FromStartTime) >= @FromStartTime and rsh.laststatuschangedate <= @ToEndTime)
		--	)
		order by rsh.oldlaststatuschangedate asc

	OPEN db_cursor  
	FETCH NEXT FROM db_cursor INTO @oldlaststatuschangedate, @laststatuschangedate, @isOldOEELossCategory, @isOEELossCategory, @DurationMin, @DurationSec
	
	WHILE @@FETCH_STATUS = 0
	BEGIN  
		--Print '@ResourceId ' + @ResourceId;
		--Print '@ResourceId ' + @ResourceId + ' @oldlaststatuschangedate ' + LEFT(CONVERT(VARCHAR, @oldlaststatuschangedate, 121), 23) + ' @laststatuschangedate ' + LEFT(CONVERT(VARCHAR, @laststatuschangedate, 121), 23) ; 
		
		DECLARE @dtFrom DATETIME = null, @dtTo DATETIME = null

		--CASE OF FIRST ResourceSetup RECORD TO BE MANAGED ONLY IF THE STATUS OF ARRIVAL IS AvailabilityLOSS and the request's startime is before laststatuschangedate
		-- (IT IS THE CASE IN WHICH THE FIRST ResourceSetup makes a transition of AvailabilityLOSS )
		if (@oldlaststatuschangedate is NULL)
		BEGIN
			if (@isOEELossCategory = 1 AND @FromStartTime < @laststatuschangedate) 
			begin
				set @dtFrom = @laststatuschangedate --The first record the machine is considered running --@FromStartTime --Remarks: @oldlaststatuschangedate is NULL in this case
				set @dtTo	= (SELECT CASE WHEN @laststatuschangedate < @ToEndTime  THEN @laststatuschangedate ELSE @ToEndTime  END)
			end
		END
		else
		begin
			if (@isOldOEELossCategory = 1)  --REMARKS: the check is ON isOldOEELossCategory and NOT in isOEELossCategory (the first record has @isOldOEELossCategory equal to NULL)
			begin
				--The request starts before the start of the OEELossCategory timespan
				if (@FromStartTime < @oldlaststatuschangedate and @ToEndTime > @oldlaststatuschangedate)
				begin 
					set @dtFrom = @oldlaststatuschangedate
					set @dtTo	= (SELECT CASE WHEN @laststatuschangedate < @ToEndTime  THEN @laststatuschangedate ELSE @ToEndTime  END)  --i.e. min(@laststatuschangedate, @ToEndTime)
				end
				else
				begin
					--The request starts after the start of the OEELossCategory timespan
					if (@FromStartTime >= @oldlaststatuschangedate and @FromStartTime < @laststatuschangedate)
					begin 
						set @dtFrom = @FromStartTime
						set @dtTo	= (SELECT CASE WHEN @laststatuschangedate < @ToEndTime  THEN @laststatuschangedate ELSE @ToEndTime  END)  --i.e. min(@laststatuschangedate, @ToEndTime)
					end
				end
			end
		end

		if (@dtFrom is NOT null and @dtTo is NOT null)
		begin
			if (@dtFrom < @dtTo)
			begin
				Set @totSec = @totSec + DATEDIFF(second, @dtFrom, @dtTo) 
				set @totMin = @totMin + DATEDIFF(MINUTE, @dtFrom, @dtTo) 
			end
		end

		FETCH NEXT FROM db_cursor INTO @oldlaststatuschangedate, @laststatuschangedate, @isOldOEELossCategory, @isOEELossCategory, @DurationMin, @DurationSec
	END 
	
	DECLARE @OEELossCategoryLastRecordTimespan int
	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @OEELossCategoryLastRecordTimespan = isGetOEELossCategoryLastRecordTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @OEELossCategoryLastRecordTimespan = (select ischema460.isGetOEELossCategoryLastRecordTimespan( @ResourceId, @FromStartTime, @ToEndTime ));

	--print 'OEELossCategoryLastRecordTimespan = ' + CONVERT(VARCHAR, @OEELossCategoryLastRecordTimespan);
	set @totSec = @totSec + @OEELossCategoryLastRecordTimespan

	--Print '@totSec ' +  CONVERT(varchar(10), @totSec)
	--Print '@totMin ' +  CONVERT(varchar(10), @totMin)

	CLOSE db_cursor  
	DEALLOCATE db_cursor 

    RETURN @totSec;
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEENonScheduledTimespan')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEENonScheduledTimespan
GO 

CREATE FUNCTION isGetOEENonScheduledTimespan(@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS int 
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	DECLARE @MfgCalendarId char(16) = (SELECT MfgCalendarId FROM Factory where FactoryId = (SELECT FactoryID from ResourceDef where ResourceId = @ResourceId and FactoryID is not null UNION SELECT  FactoryId from ResourceDef where ResourceId = (SELECT ParentResourceId from ResourceDef where ResourceId = (select ParentResourceId from ResourceDef where ResourceId = @ResourceId and FactoryLevel =3)) and FactoryId IS NOT NULL))
		
	DECLARE @tot int = 0;
	if (@MfgCalendarId is NOT NULL)
	begin
		DECLARE @NonScheduledTime float = 0.0  --in days 
		DECLARE @NonScheduledTimeInSeconds float = 0.0, @NonScheduledTimeInMinutes float = 0.0
		DECLARE @StartTime DATETIME, @EndTime DATETIME;

		DECLARE db_cursor CURSOR LOCAL FOR 
		SELECT 
			cs.ShiftStart, cs.ShiftEnd, cs.isNonScheduledTime
		FROM CalendarShift cs 
		Where	MfgCalendarId = @MfgCalendarId
				AND isNonScheduledTime is not null and isNonScheduledTime > 0.0 
				AND (
						(cs.ShiftStart <= @FromStartTime and cs.ShiftEnd > @FromStartTime) OR
						(cs.ShiftStart	BETWEEN @FromStartTime AND @ToEndTime) OR
						(cs.ShiftEnd	BETWEEN @FromStartTime AND @ToEndTime)
					)
		OPEN db_cursor  
		FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime, @NonScheduledTime
	
		WHILE @@FETCH_STATUS = 0  
		BEGIN  
			if (@StartTime <= @FromStartTime) 
			begin
				set @StartTime = @FromStartTime
			end

			if (@EndTime > @ToEndTime)
			begin
				set @EndTime = @ToEndTime
			end

			declare @currSeconds int = DATEDIFF(second, @StartTime, @EndTime)
			--@NonScheduledTimeInSeconds	= ROUND(cs.isNonScheduledTime*1440*60,	0)
			--set @NonScheduledTimeInMinutes	= ROUND(@NonScheduledTime*1440,		0)
			set @NonScheduledTimeInSeconds	= ROUND(@NonScheduledTime*1440*60,	0)
			set @tot = @tot + @NonScheduledTimeInSeconds;

			FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime, @NonScheduledTime
		END 

		CLOSE db_cursor  
		DEALLOCATE db_cursor 
	end
	RETURN @tot;
end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEPlannedProductionTimePerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEPlannedProductionTimePerResourceId
GO 

CREATE FUNCTION isGetOEEPlannedProductionTimePerResourceId (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)
RETURNS @t TABLE(
		PlannedProductionTimeInSeconds float NULL,
		totTimespanInSeconds			float NULL,
		NonScheduledTimeInSeconds		float NULL,
		FromStartTimeUpdated			DateTime, 
		ToEndTimeUpdated				DateTime
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		--Remarks: Timespan could be simply the difference between StartTime and EndTime
		--		   but we should take into account some holes in the calendar (non scheduled times)
		--DECLARE @Timespan int = 0;
		--SET @Timespan = DateDiff(second, @FromStartTime, @ToEndTime)
	
		DECLARE @totTimespanInSeconds    float = 0,		 @totTimespanInMinutes			float = 0;
		DECLARE @totNonScheduledTimeInSeconds float = 0, @totNonScheduledTimeInMinutes	float = 0;
		DECLARE @FromStartTimeUpdated As DateTime = @FromStartTime, @ToEndTimeUpdated As DateTime = @ToEndTime
		set @totTimespanInSeconds = DATEDIFF(second, @FromStartTime, @ToEndTime)

		DECLARE @MfgCalendarId char(16) = (SELECT MfgCalendarId FROM Factory where FactoryId = (SELECT FactoryID from ResourceDef where ResourceId = @ResourceId and FactoryID is not null UNION SELECT  FactoryId from ResourceDef where ResourceId = (SELECT ParentResourceId from ResourceDef where ResourceId = (select ParentResourceId from ResourceDef where ResourceId = @ResourceId and FactoryLevel =3)) and FactoryId IS NOT NULL))
		if (@MfgCalendarId is NOT NULL)
		begin
			set @totTimespanInSeconds = 0
			DECLARE @NonScheduledTime float = 0.0  --in days 
			DECLARE @NonScheduledTimeInSeconds float = 0.0, @NonScheduledTimeInMinutes float = 0.0
			DECLARE @StartTime DATETIME, @EndTime DATETIME
			DECLARE @counter int = 0
			DECLARE @bFromStartTimeUpdatedAlreadySet int = 0

			DECLARE db_cursor CURSOR LOCAL FOR 
			SELECT	cs.ShiftStart, cs.ShiftEnd, cs.isNonScheduledTime
			FROM	CalendarShift cs 
			Where	MfgCalendarId = @MfgCalendarId
					AND (
							(cs.ShiftStart <= @FromStartTime and cs.ShiftEnd > @FromStartTime) OR
							(cs.ShiftStart	BETWEEN @FromStartTime AND @ToEndTime) OR
							(cs.ShiftEnd	BETWEEN @FromStartTime AND @ToEndTime)
						)
						order by cs.ShiftStart
			OPEN db_cursor  
			FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime, @NonScheduledTime
	
			WHILE @@FETCH_STATUS = 0  
			BEGIN  
				if (@StartTime >= @FromStartTime) 
				begin
					--the request of the user MUST BE IN an available shift
					if (@bFromStartTimeUpdatedAlreadySet = 0) 
					begin
						set @FromStartTimeUpdated = @StartTime
						set @bFromStartTimeUpdatedAlreadySet = 1
					end
				end					

				if (@StartTime <= @FromStartTime) 
				begin
					set @StartTime = @FromStartTime
				end

				if (@EndTime > @ToEndTime)
				begin
					set @EndTime = @ToEndTime
					set @ToEndTimeUpdated = @ToEndTime
				end

				declare @currSeconds int = DATEDIFF(second, @StartTime, @EndTime)
				--declare @currMinutes int = DATEDIFF(minute, @StartTime, @EndTime)
				set @totTimespanInSeconds = @totTimespanInSeconds + @currSeconds;
				if (@NonScheduledTime is NOT NULL  AND @NonScheduledTime >= 0.0)
				begin
					--set @NonScheduledTimeInMinutes	= ROUND(@NonScheduledTime*1440,		0)
					set @NonScheduledTimeInSeconds		= ROUND(@NonScheduledTime*1440*60,	0)
					set @totNonScheduledTimeInSeconds = @totNonScheduledTimeInSeconds + @NonScheduledTimeInSeconds;
				end

				set @counter = @counter + 1
				FETCH NEXT FROM db_cursor INTO @StartTime, @EndTime, @NonScheduledTime
			END 

			CLOSE db_cursor  
			DEALLOCATE db_cursor 
		end

		--Remarks: NonScheduledTimeInSeconds or NonScheduledTimeInMinutes  should not be taken into account for availability
		--if (@NonScheduledTimeInMinutes is NOT NULL and @NonScheduledTimeInMinutes > 0.0)
		--begin
		--	SET @PlannedProductionTime = @Timespan - @NonScheduledTimeInMinutes
		--end

		--PRINT 'Timespan = ' + convert(varchar, @Timespan) + ' seconds';
		--PRINT 'PlannedProductionTime = ' + convert(varchar, @PlannedProductionTime) + ' seconds';

		DECLARE @PlannedProductionTimeInSeconds float = @totTimespanInSeconds
		if (@totTimespanInSeconds <= 0)
			set @PlannedProductionTimeInSeconds = 0.0

			--Remarks: it could happen if the user set a too high NonScheduledTime
		if (@NonScheduledTimeInSeconds > 0.0 AND @totTimespanInSeconds >= @NonScheduledTimeInSeconds)
		begin
			set @PlannedProductionTimeInSeconds = @totTimespanInSeconds - @NonScheduledTimeInSeconds
		end
		--  Uncomment lines below to have NonScheduledTime apply immediately at start of shift
		--if (@NonScheduledTimeInSeconds > 0.0 AND @totTimespanInSeconds < @NonScheduledTimeInSeconds)
		--begin
		--	set @PlannedProductionTimeInSeconds = 0.0
		--end

		set @totTimespanInMinutes			= @totTimespanInSeconds / 60.0;
		set @totNonScheduledTimeInMinutes	= @totNonScheduledTimeInSeconds / 60.0;

		insert into @t  
			(
				PlannedProductionTimeInSeconds,
				totTimespanInSeconds,
				NonScheduledTimeInSeconds,
				FromStartTimeUpdated,
				ToEndTimeUpdated
			)
			Values(
				@PlannedProductionTimeInSeconds,
				@totTimespanInSeconds,
				@NonScheduledTimeInSeconds,
				@FromStartTimeUpdated,
				@ToEndTimeUpdated
			)
	end
	RETURN
END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEResourceProductIdealCycleTime')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEResourceProductIdealCycleTime
GO 

CREATE FUNCTION isGetOEEResourceProductIdealCycleTime (@ResourceId char(16), @ProductId char(16))
RETURNS @t TABLE(
		IdealCycleTime float NULL,
		descri nvarchar(20) NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	declare @fIdealCycleTime float = 0.0
	declare @sIdealCycleTime nvarchar(20) = ''

	declare  @IdealCycleTime float = null

	if (@ResourceId IS NOT NULL and @ProductId IS NOT NULL)
	begin
		select @IdealCycleTime = idt.IdealCycleTime from isIdealCycleTimes idt  where idt.ParentId = @ResourceId and ProductId = @ProductId
		if (@IdealCycleTime is NULL)
		begin
			declare  @ProductFamilyId char(16) = null
			select @ProductFamilyId = p.ProductFamilyId from Product p where p.ProductId = @ProductId
			if (@ProductFamilyId is not NULL)
				select @IdealCycleTime = idt.IdealCycleTime from isIdealCycleTimes idt  where idt.ParentId = @ResourceId and ProductFamilyId = @ProductFamilyId
		end	

		if (@IdealCycleTime is not null)  
		begin
			set @fIdealCycleTime = @IdealCycleTime
			declare @nIdealCycleTime int = CAST( (@IdealCycleTime+0.00000000000000000100) * 86400 AS INT)
			
			if (@nIdealCycleTime > 0)
				set @sIdealCycleTime = 
								CONVERT(VARCHAR(12), @nIdealCycleTime / 60 / 60 / 24)  +'d'
						+ ':' + CONVERT(VARCHAR(12), @nIdealCycleTime / 60 / 60 % 24) +'h'
						+ ':' + CONVERT(VARCHAR(2), @nIdealCycleTime  / 60 % 60)  +'m'
						+ ':' + CONVERT(VARCHAR(2), @nIdealCycleTime  % 60)  +'s'
		end
	end

	insert into @t (IdealCycleTime,descri) Values(@fIdealCycleTime, @sIdealCycleTime)

	RETURN
END
GO


SELECT IdealCycleTime FROM isIdealCycleTimes
WHERE (ParentId = '0005d28000000030' AND ProductId = '00062c8000000002' )
OR (ParentId = '0005d28000000030' AND ProductFamilyId = '' )
OR (ParentId = '001edc8000000005' AND ProductId = '00062c8000000002' )
OR (ParentId = '001edc8000000005' AND ProductFamilyId = '' )
ORDER BY CASE
    WHEN ParentId = '0005d28000000030' AND ProductId = '00062c8000000002' THEN 1
    WHEN ParentId = '0005d28000000030' AND ProductFamilyId = '' THEN 2
    WHEN ParentId = '001edc8000000005' AND ProductId = '00062c8000000002' THEN 3
    WHEN ParentId = '001edc8000000005' AND ProductFamilyId = '' THEN 4
    ELSE 5
END

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEAvailability')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEAvailability
GO 

CREATE FUNCTION isGetOEEAvailability (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS float
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NULL)
		return 0.0

	DECLARE @PlannedProductionTimeInSeconds float = 0.0
	DECLARE @totTimespanInSeconds			float = 0.0
	DECLARE @NonScheduledTimeInSeconds		float = 0.0
	
	DECLARE @FromStartTimeUpdated As DateTime, @ToEndTimeUpdated As DateTime

	--Takes into account possible gaps between consecutive shifts 
	select	@PlannedProductionTimeInSeconds = PlannedProductionTimeInSeconds,
			@totTimespanInSeconds			= totTimespanInSeconds,
			@NonScheduledTimeInSeconds		= NonScheduledTimeInSeconds,
			@FromStartTimeUpdated			= FromStartTimeUpdated,
			@ToEndTimeUpdated				= ToEndTimeUpdated

		from isGetOEEPlannedProductionTimePerResourceId( @ResourceId, @FromStartTime, @ToEndTime );

	--Remarks: the StartTime selected by the user could be in a moment in which a shift is not present ( a gap between shifts)
	set @FromStartTime  = @FromStartTimeUpdated
	set @ToEndTime		= @ToEndTimeUpdated
	
	DECLARE @PlannedProductionTimeInMinutes float = round(@PlannedProductionTimeInSeconds/60.0, 1)
	DECLARE @PlannedProductionTimeInHours float = round(@PlannedProductionTimeInSeconds/3600.0, 1)
	
	--Remarks: NonScheduledTimeInSeconds or NonScheduledTimeInMinutes  should not be taken into account for availability
	--if (@NonScheduledTimeInMinutes is NOT NULL and @NonScheduledTimeInMinutes > 0.0)
	--begin
	--	SET @PlannedProductionTime = @Timespan - @NonScheduledTimeInMinutes
	--end

	--PRINT 'Timespan = ' + convert(varchar, @Timespan) + ' seconds';
	--PRINT 'PlannedProductionTime = ' + convert(varchar, @PlannedProductionTime) + ' seconds';

	DECLARE @vResolvedDowntimeSch int;

	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @vResolvedDowntimeSch = isGetResolvedDowntimeSchdTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @vResolvedDowntimeSch = (select isGetResolvedDowntimeSchdTimespan( @ResourceId, @FromStartTime, @ToEndTime ));	

	if (@vResolvedDowntimeSch is NOT NULL and @vResolvedDowntimeSch > 0)
	begin
		--PRINT '@vResolvedDowntimeSch = '; PRINT @vResolvedDowntimeSch;-- PRINT ' minutes';
		SET @PlannedProductionTimeInSeconds = @PlannedProductionTimeInSeconds - @vResolvedDowntimeSch
		--PRINT '@PlannedProductionTimeInSeconds = '; PRINT @vResolvedDowntimeSch;
	end

	DECLARE @OEELossCategoryAvailabilityTimespan int = 0;

	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @OEELossCategoryAvailabilityTimespan = isGetOEELossCategoryAvailabilityTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @OEELossCategoryAvailabilityTimespan = (select isGetOEELossCategoryAvailabilityTimespan( @ResourceId, @FromStartTime, @ToEndTime ));

	--PRINT 'OEELossCategoryAvailabilityTimespan = ' + CONVERT(VARCHAR,@OEELossCategoryAvailabilityTimespan);

	DECLARE @OperatingTime int;  --Remarks: Runtime in literature
	Set @OperatingTime = (@PlannedProductionTimeInSeconds - @OEELossCategoryAvailabilityTimespan)

	DECLARE @Availability float = 0.0; 
	if (@PlannedProductionTimeInSeconds > 0) 
	begin
		Set @Availability = cast(@OperatingTime as float) / cast(@PlannedProductionTimeInSeconds as float)
	end
	--PRINT 'Availability= ' + CONVERT(VARCHAR, @Availability);

    RETURN @Availability;
END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEAvailabilityPerResourceFamilyId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEAvailabilityPerResourceFamilyId
GO 

CREATE FUNCTION isGetOEEAvailabilityPerResourceFamilyId (@ResourceFamilyId char(16), @TimeSpanType int = 1, 
					@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
					@pRunDate DATETIME )
RETURNS @t TABLE(
		isResourceId char(16) NULL,
		isResourceName nvarchar(30) NULL,
		isResourceFamilyId char(16) NULL,
		isOEE float NULL,
		isAvailability float NULL,
		isPerformance float NULL,
		isQuality float NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceFamilyId IS NOT NULL)
	begin

		Declare @ResourceId char(16)
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit

		DECLARE db_cursor CURSOR LOCAL FOR 
			SELECT ResourceId,ResourceName,isIncludeInOEE
			FROM ResourceDef r
			INNER JOIN ResourceFamily rf ON r.ResourceFamilyId = rf.ResourceFamilyId
			WHERE 
			--==============================================================================================================
			r.ResourceFamilyId = @ResourceFamilyId    --FILTER BY EQUIPMENT -==========================================================
			and r.isIncludeInOEE = 1 
			order by r.ResourceName asc

		OPEN db_cursor  
		FETCH NEXT FROM db_cursor INTO @ResourceId, @ResourceName, @isIncludeInOEE
	
		WHILE @@FETCH_STATUS = 0
		BEGIN  
			--Print '@ResourceId ' + @ResourceId + ' ResourceName ' + @ResourceName 
		
			DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
			DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
			select top(1) 
			@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
				isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

			declare @isOEE float = 0.0, @isAvailability float = 0.0, @isPerformance float = 0.0, @isQuality float = 0.0


			--Remarks: 
			-- a scalar valued function can be called directly only using the schema as a prefix
			-- but we cannot hardcode the schema name
			-- otherwise it is necessary to use EXEC to call it
			EXEC @isAvailability = isGetOEEAvailability @ResourceId, @FromStartTime, @ToEndTime
			--Same as: (but with schema name)
			--set @isAvailability = (select isGetOEEAvailability( @ResourceId, @FromStartTime, @ToEndTime ));
			set @isPerformance	= 0.8
			set @isQuality		= 0.8

			declare @tmpisOEE float = (@isAvailability * @isPerformance * @isQuality) --/ (100*100*100)

			declare @isOEEPerc float = 0.0, @isAvailabilityPerc float = 0.0, @isPerformancePerc float = 0.0, @isQualityPerc float = 0.0
			set @isAvailabilityPerc = round(@isAvailability,4) * 100
			set @isPerformancePerc = round(@isPerformance,4) * 100
			set @isQualityPerc = round(@isQuality,4) * 100
			set @isOEEPerc = round(@tmpisOEE,4) * 100

			--print 'Availability = ' + CONVERT(VARCHAR, @isAvailability);

			insert into @t  
						(isResourceId,	isResourceName, isResourceFamilyId, isOEE,	isAvailability,		isPerformance, isQuality ) 
				Values	(@ResourceId,	@ResourceName,	@ResourceFamilyId,	@isOEEPerc, @isAvailabilityPerc,	@isPerformancePerc, @isQualityPerc)

			FETCH NEXT FROM db_cursor INTO @ResourceId, @ResourceName, @isIncludeInOEE
		END 
	
		--Print '@totSec ' +  CONVERT(varchar(10), @totSec)
		--Print '@totMin ' +  CONVERT(varchar(10), @totMin)

		CLOSE db_cursor  
		DEALLOCATE db_cursor 
	end

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEERawDetailsIdealCycleTime')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEERawDetailsIdealCycleTime
GO 

CREATE FUNCTION isGetOEERawDetailsIdealCycleTime (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS float 
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	DECLARE @TotIdealCycleTimeMin float
	
	select @TotIdealCycleTimeMin = sum(IdealCycleTime) from isOEERawDetails where ResourceId = @ResourceId and TxnType IN (2180, 2880, 3390, 6660) AND TxnDate between @FromStartTime and @ToEndTime 

	--DECLARE @ParentId char(16)
	--DECLARE @IdealCycleTimeMin float , @IdealCycleTimeSec float
	
	--DECLARE @TxnDate Datetime
	--DECLARE @TotIdealCycleTimeMin float , @TotIdealCycleTimeSec float

	--DECLARE db_cursor CURSOR LOCAL FOR 
	--	SELECT
	--		round(rd.IdealCycleTime, 3)			IdealCycleTimeMin --minutes
	--		,round(rd.IdealCycleTime*60, 3)		IdealCycleTimeSec --seconds
	--		,TxnDate							TxnDate
 --       FROM isOEERawDetails rd
 --       WHERE rd.ResourceId = @ResourceId
	--			and TxnDate between @FromStartTime and @ToEndTime
	--	order by rd.TxnDate asc 

	--OPEN db_cursor  
	--FETCH NEXT FROM db_cursor INTO @ParentId, @IdealCycleTimeMin, @IdealCycleTimeSec, @TxnDate
	
	--WHILE @@FETCH_STATUS = 0
	--BEGIN  		
	--	DECLARE @dtFrom DATETIME = null, @dtTo DATETIME = null

	--	set @TotIdealCycleTimeMin = @TotIdealCycleTimeMin + @IdealCycleTimeMin
	--	set @TotIdealCycleTimeSec = @TotIdealCycleTimeSec + @IdealCycleTimeSec

	--	FETCH NEXT FROM db_cursor INTO @ParentId, @IdealCycleTimeMin, @IdealCycleTimeSec, @TxnDate
	--END 
	
	--CLOSE db_cursor  
	--DEALLOCATE db_cursor 

    RETURN @TotIdealCycleTimeMin;
END
GO


IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEERawDetailsQuantities')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEERawDetailsQuantities
GO 

CREATE FUNCTION isGetOEERawDetailsQuantities (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)
RETURNS @t TABLE(
		GoodQty			float NULL,
		TotalQty		float NULL
		)
AS 
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
BEGIN 
	DECLARE @TotGoodQty		float = 0.0
	DECLARE @TotTotalQty	float = 0.0

	select	@TotGoodQty		= CASE WHEN sum(ISNULL(GoodQty,0)) > 0 THEN sum(ISNULL(GoodQty,0)) ELSE sum(ISNULL(GoodQty2,0)) END ,
			@TotTotalQty	= CASE WHEN sum(ISNULL(TotalQty,0)) > 0 THEN sum(ISNULL(TotalQty,0)) ELSE sum(ISNULL(TotalQty2,0)) END
			from isOEERawDetails where ResourceId = @ResourceId and TxnType IN (2180, 2880, 3390, 6660) AND TxnDate between @FromStartTime and @ToEndTime 

	insert into @t  (GoodQty,TotalQty) Values(@TotGoodQty, @TotTotalQty)

    RETURN;
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEPerformance')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEPerformance
GO 

CREATE FUNCTION isGetOEEPerformance (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS float
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NULL)
		return 0.0

	DECLARE @PlannedProductionTimeInSeconds float = 0.0
	DECLARE @totTimespanInSeconds			float = 0.0
	DECLARE @NonScheduledTimeInSeconds		float = 0.0
	
	DECLARE @FromStartTimeUpdated As DateTime, @ToEndTimeUpdated As DateTime

	--Takes into account possible gaps between consecutive shifts 
	select	@PlannedProductionTimeInSeconds = PlannedProductionTimeInSeconds,
			@totTimespanInSeconds			= totTimespanInSeconds,
			@NonScheduledTimeInSeconds		= NonScheduledTimeInSeconds,
			@FromStartTimeUpdated			= FromStartTimeUpdated,
			@ToEndTimeUpdated				= ToEndTimeUpdated

		from isGetOEEPlannedProductionTimePerResourceId( @ResourceId, @FromStartTime, @ToEndTime );

	--Remarks: the StartTime selected by the user could be in a moment in which a shift is not present ( a gap between shifts)
	set @FromStartTime  = @FromStartTimeUpdated
	set @ToEndTime		= @ToEndTimeUpdated
	
	DECLARE @PlannedProductionTimeInMinutes float = round(@PlannedProductionTimeInSeconds/60.0, 1)
	DECLARE @PlannedProductionTimeInHours float = round(@PlannedProductionTimeInSeconds/3600.0, 1)
	
	--Remarks: NonScheduledTimeInSeconds or NonScheduledTimeInMinutes  should not be taken into account for availability
	--if (@NonScheduledTimeInMinutes is NOT NULL and @NonScheduledTimeInMinutes > 0.0)
	--begin
	--	SET @PlannedProductionTime = @Timespan - @NonScheduledTimeInMinutes
	--end

	--PRINT 'Timespan = ' + convert(varchar, @Timespan) + ' seconds';
	--PRINT 'PlannedProductionTime = ' + convert(varchar, @PlannedProductionTime) + ' seconds';

	DECLARE @vResolvedDowntimeSch int;

	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @vResolvedDowntimeSch = isGetResolvedDowntimeSchdTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @vResolvedDowntimeSch = (select ischema460.isGetResolvedDowntimeSchdTimespan( @ResourceId, @FromStartTime, @ToEndTime ));

	if (@vResolvedDowntimeSch is NOT NULL and @vResolvedDowntimeSch > 0)
	begin
		--PRINT '@vResolvedDowntimeSch = '; PRINT @vResolvedDowntimeSch;-- PRINT ' minutes';
		SET @PlannedProductionTimeInSeconds = @PlannedProductionTimeInSeconds - @vResolvedDowntimeSch
		--PRINT '@PlannedProductionTimeInSeconds = '; PRINT @vResolvedDowntimeSch;
	end

	DECLARE @OEELossCategoryAvailabilityTimespan int = 0;
	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @OEELossCategoryAvailabilityTimespan = isGetOEELossCategoryAvailabilityTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @OEELossCategoryAvailabilityTimespan = (select ischema460.isGetOEELossCategoryAvailabilityTimespan( @ResourceId, @FromStartTime, @ToEndTime ));
	

	--PRINT 'OEELossCategoryAvailabilityTimespan = ' + CONVERT(VARCHAR,@OEELossCategoryAvailabilityTimespan);

	DECLARE @OperatingTime int;  --Remarks: Runtime in literature
	Set @OperatingTime = (@PlannedProductionTimeInSeconds - @OEELossCategoryAvailabilityTimespan)

	DECLARE @totIdealCycleTimeMin float = 0.0
	DECLARE @totIdealCycleTimeSec float = 0.0

	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @totIdealCycleTimeMin = isGetOEERawDetailsIdealCycleTime @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @totIdealCycleTimeMin = ischema460.isGetOEERawDetailsIdealCycleTime( @ResourceId, @FromStartTime, @ToEndTime );

	
	Set @totIdealCycleTimeSec = @totIdealCycleTimeMin * 60.0

	DECLARE @Performance float = 0.0; 
	if (@PlannedProductionTimeInSeconds > 0 AND @OperatingTime > 0) 
	begin
		Set @Performance = @totIdealCycleTimeSec / cast(@OperatingTime as float)
	end
	--PRINT 'Performance= ' + CONVERT(VARCHAR, @Performance);

    RETURN @Performance;
END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEQuality')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEQuality
GO 

CREATE FUNCTION isGetOEEQuality (@ResourceId char(16), @FromStartTime As DateTime, @ToEndTime As DateTime)  RETURNS float
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NULL)
		return 0.0

	DECLARE @PlannedProductionTimeInSeconds float = 0.0
	DECLARE @totTimespanInSeconds			float = 0.0
	DECLARE @NonScheduledTimeInSeconds		float = 0.0
	
	DECLARE @FromStartTimeUpdated As DateTime, @ToEndTimeUpdated As DateTime

	--Takes into account possible gaps between consecutive shifts 
	select	@PlannedProductionTimeInSeconds = PlannedProductionTimeInSeconds,
			@totTimespanInSeconds			= totTimespanInSeconds,
			@NonScheduledTimeInSeconds		= NonScheduledTimeInSeconds,
			@FromStartTimeUpdated			= FromStartTimeUpdated,
			@ToEndTimeUpdated				= ToEndTimeUpdated

		from isGetOEEPlannedProductionTimePerResourceId( @ResourceId, @FromStartTime, @ToEndTime );

	--Remarks: the StartTime selected by the user could be in a moment in which a shift is not present ( a gap between shifts)
	set @FromStartTime  = @FromStartTimeUpdated
	set @ToEndTime		= @ToEndTimeUpdated
	
	DECLARE @PlannedProductionTimeInMinutes float = round(@PlannedProductionTimeInSeconds/60.0, 1)
	DECLARE @PlannedProductionTimeInHours float = round(@PlannedProductionTimeInSeconds/3600.0, 1)
	
	--Remarks: NonScheduledTimeInSeconds or NonScheduledTimeInMinutes  should not be taken into account for availability
	--if (@NonScheduledTimeInMinutes is NOT NULL and @NonScheduledTimeInMinutes > 0.0)
	--begin
	--	SET @PlannedProductionTime = @Timespan - @NonScheduledTimeInMinutes
	--end

	--PRINT 'Timespan = ' + convert(varchar, @Timespan) + ' seconds';
	--PRINT 'PlannedProductionTime = ' + convert(varchar, @PlannedProductionTime) + ' seconds';

	DECLARE @vResolvedDowntimeSch int;

	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @vResolvedDowntimeSch = isGetResolvedDowntimeSchdTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @vResolvedDowntimeSch = (select ischema460.isGetResolvedDowntimeSchdTimespan( @ResourceId, @FromStartTime, @ToEndTime ));


	if (@vResolvedDowntimeSch is NOT NULL and @vResolvedDowntimeSch > 0)
	begin
		--PRINT '@vResolvedDowntimeSch = '; PRINT @vResolvedDowntimeSch;-- PRINT ' minutes';
		SET @PlannedProductionTimeInSeconds = @PlannedProductionTimeInSeconds - @vResolvedDowntimeSch
		--PRINT '@PlannedProductionTimeInSeconds = '; PRINT @vResolvedDowntimeSch;
	end

	DECLARE @OEELossCategoryAvailabilityTimespan int = 0;
	--Remarks: 
	-- a scalar valued function can be called directly only using the schema as a prefix
	-- but we cannot hardcode the schema name
	-- otherwise it is necessary to use EXEC to call it
	EXEC @OEELossCategoryAvailabilityTimespan = isGetOEELossCategoryAvailabilityTimespan @ResourceId, @FromStartTime, @ToEndTime
	--Same as: (but with schema name)
	--Set @OEELossCategoryAvailabilityTimespan = (select ischema460.isGetOEELossCategoryAvailabilityTimespan( @ResourceId, @FromStartTime, @ToEndTime ));


	--PRINT 'OEELossCategoryAvailabilityTimespan = ' + CONVERT(VARCHAR,@OEELossCategoryAvailabilityTimespan);

	DECLARE @OperatingTime int;  --Remarks: Runtime in literature
	Set @OperatingTime = (@PlannedProductionTimeInSeconds - @OEELossCategoryAvailabilityTimespan)


	DECLARE @GoodQty As float = 0.0, @TotalQty As float = 0.0
	--do not prefix with schema (ischema460.) a table-valued function
	select	@GoodQty = GoodQty, @TotalQty = TotalQty 
			from
			isGetOEERawDetailsQuantities( @ResourceId, @FromStartTime, @ToEndTime );

	DECLARE @Quality float = 0.0; 
	if (@PlannedProductionTimeInSeconds > 0 AND @OperatingTime > 0 AND @TotalQty > 0.0) 
	begin
		Set @Quality = @GoodQty / @TotalQty
	end
	--PRINT 'Performance= ' + CONVERT(VARCHAR, @Performance);

    RETURN @Quality;
END

GO


IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEValuesPerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEValuesPerResourceId
GO 

CREATE FUNCTION isGetOEEValuesPerResourceId (
						@ResourceId char(16), 
						@TimeSpanType int = 1, 
						@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
						@pRunDate As DateTime)
RETURNS @t TABLE(
		isResourceId char(16) NULL,
		isResourceName nvarchar(30) NULL,
		isResourceFamilyId char(16) NULL,
		isOEE float NULL,
		isAvailability float NULL,
		isPerformance float NULL,
		isQuality float NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit
		Declare @ResourceFamilyId char(16) = ''
		
		--select CDODefId from CDODefinition where CDOName  = 'Resource' 1490 = 0x5d2 --0005d28000000001 0005d2
		--if (LEFT(@ResourceId,6) = '0005d2')
		--begin
		--	SELECT  @ResourceName = ResourceName,
		--			@ResourceFamilyId = ResourceFamilyId
		--	FROM	ResourceDef r
		--	WHERE	r.ResourceId = @ResourceId
		--		and r.isIncludeInOEE = 1
		--end
		--else
		--begin
			--QUERY By Name to let automatic tests work easily
			Declare @ActualResourceId nvarchar(30) = ''
			SELECT  @ActualResourceId = ResourceId,
					@ResourceName = ResourceName,
					@ResourceFamilyId = ResourceFamilyId
			FROM	ResourceDef r
			WHERE	r.ResourceId = @ResourceId
				and r.isIncludeInOEE = 1			
			
			set @ResourceId = @ActualResourceId
		--end

		DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
		DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
		--Remarks: do not prefix the schema name in table-valued function
		select top(1)
		@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
			isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

		declare @isOEE float = 0.0, @isAvailability float = 0.0, @isPerformance float = 0.0, @isQuality float = 0.0

		--Remarks: 
		-- a scalar valued function can be called directly only using the schema as a prefix
		-- but we cannot hardcode the schema name
		-- otherwise it is necessary to use EXEC to call it
		EXEC @isAvailability = isGetOEEAvailability @ResourceId, @FromStartTime, @ToEndTime
		--set @isAvailability = (select ischema460.isGetOEEAvailability( @ResourceId, @FromStartTime, @ToEndTime ));
		if (@isAvailability is NULL)
			set @isAvailability = 0.0

		EXEC @isPerformance = isGetOEEPerformance @ResourceId, @FromStartTime, @ToEndTime
		if (@isPerformance is NULL)
			set @isPerformance = 0.0
		--set @isPerformance= ischema460.isGetOEEPerformance( @ResourceId, @FromStartTime, @ToEndTime );
		
		EXEC @isQuality		= isGetOEEQuality @ResourceId, @FromStartTime, @ToEndTime
		--set @isQuality	= ischema460.isGetOEEQuality( @ResourceId, @FromStartTime, @ToEndTime );
		if (@isQuality is NULL)
			set @isQuality = 0.0

		declare @tmpisOEE float = (@isAvailability * @isPerformance * @isQuality) --/ (100*100*100)

		declare @isOEEPerc float = 0.0, @isAvailabilityPerc float = 0.0, @isPerformancePerc float = 0.0, @isQualityPerc float = 0.0
		set @isAvailabilityPerc = round(@isAvailability,4) * 100
		set @isPerformancePerc = round(@isPerformance,4) * 100
		set @isQualityPerc = round(@isQuality,4) * 100
		set @isOEEPerc = round(@tmpisOEE,4) * 100

		--print 'Availability = ' + CONVERT(VARCHAR, @isAvailability);

			insert into @t  
						(isResourceId,	isResourceName, isResourceFamilyId, isOEE,	isAvailability,		isPerformance, isQuality ) 
				Values	(@ResourceId,	@ResourceName,	@ResourceFamilyId,	@isOEEPerc, @isAvailabilityPerc,	@isPerformancePerc, @isQualityPerc)
		END 

    RETURN;

END

GO


IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEEValuesPerResourceFamilyId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEValuesPerResourceFamilyId
GO 

CREATE FUNCTION isGetOEEValuesPerResourceFamilyId (
							@ResourceFamilyId char(16), 
							@TimeSpanType int = 1, 
							@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
							@pRunDate As DateTime)
RETURNS @t TABLE(
		isResourceId char(16) NULL,
		isResourceName nvarchar(30) NULL,
		isResourceFamilyId char(16) NULL,
		isOEE float NULL,
		isAvailability float NULL,
		isPerformance float NULL,
		isQuality float NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceFamilyId IS NOT NULL)
	begin

		Declare @ResourceId char(16)
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit

		--select CDODefId from CDODefinition where CDOName  = 'ResourceFamily' 7900 = 0x1edc --001edc8000000001 001edc
		--if (LEFT(@ResourceFamilyId,6) <> '001edc')
		--begin
			--QUERY By Name to let automatic tests work easily
			--Declare @ActualResourceFamilyId nvarchar(30) = ''
			--SELECT  @ActualResourceFamilyId = ResourceFamilyId
			--FROM	ResourceFamily rf
			--WHERE	rf.ResourceFamilyName = @ResourceFamilyId
			
			--set @ResourceFamilyId= @ActualResourceFamilyId
		--end

		DECLARE db_cursor CURSOR LOCAL FOR 
			SELECT ResourceId,ResourceName,isIncludeInOEE
			FROM ResourceDef r
			INNER JOIN ResourceFamily rf ON r.ResourceFamilyId = rf.ResourceFamilyId
			WHERE 
			--==============================================================================================================
			r.ResourceFamilyId = @ResourceFamilyId    --FILTER BY EQUIPMENT -==========================================================
			and r.isIncludeInOEE = 1 
			order by r.ResourceName asc

		OPEN db_cursor  
		FETCH NEXT FROM db_cursor INTO @ResourceId, @ResourceName, @isIncludeInOEE
	
		WHILE @@FETCH_STATUS = 0
		BEGIN  
			--Print '@ResourceId ' + @ResourceId + ' ResourceName ' + @ResourceName 
		
			DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
			DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
			declare @isOEE float = 0.0, @isAvailability float = 0.0, @isPerformance float = 0.0, @isQuality float = 0.0

			SELECT  @ResourceName	= isResourceName,
					@isAvailability = isAvailability,
					@isPerformance	= isPerformance, 
					@isQuality		= isQuality , 
					@isOEE			= isOEE  
			FROM isGetOEEValuesPerResourceId (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType)

			insert into @t  
						(isResourceId,	isResourceName, isResourceFamilyId, isOEE,	isAvailability,		isPerformance, isQuality ) 
				Values	(@ResourceId,	@ResourceName,	@ResourceFamilyId,	@isOEE, @isAvailability,	@isPerformance, @isQuality)

			FETCH NEXT FROM db_cursor INTO @ResourceId, @ResourceName, @isIncludeInOEE
		END 
	
		--Print '@totSec ' +  CONVERT(varchar(10), @totSec)
		--Print '@totMin ' +  CONVERT(varchar(10), @totMin)

		CLOSE db_cursor  
		DEALLOCATE db_cursor 
	end

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEEValues]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isGetOEEValues]
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isGetOEEValues(@pisResourceFamilyId char(16),@pisResourceId char(16) = '', @pTimeSpanType int = 1 , @pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE, @pRunDate DATETIME = GETDATE)


RETURNS @t TABLE(
		isResourceId char(16) NULL,
		isResourceName nvarchar(30) NULL,
		isResourceFamilyId char(16) NULL,
		isOEE float NULL,
		isAvailability float NULL,
		isPerformance float NULL,
		isQuality float NULL
		)
AS
Begin
	if (@pTimeSpanType is null OR @pTimeSpanType <= 0 OR @pTimeSpanType > 3)
	begin
		set @pTimeSpanType = 0
	end
	
	--declare @emptyDate DATETIME = '1/1/1900'
	declare @emptyDate DateTime = 0 --select @defaultDateTime --1900-01-01 00:00:00.0000000

	if (@pStartTimeDate is null OR @pStartTimeDate = @emptyDate)
	begin
		Set @pStartTimeDate = @pRunDate --GetDate()
	end
	if (@pEndTimeDate is null OR @pEndTimeDate = @emptyDate)
	begin
		Set @pEndTimeDate = @pRunDate --GetDate() 
	end
	
	if (@pisResourceId='' Or @pisResourceId is null)
	begin
		insert into @t  select isResourceId, isResourceName, isResourceFamilyId, isOEE, isAvailability, isPerformance, isQuality 
			from isGetOEEValuesPerResourceFamilyId 
			(
			@pisResourceFamilyId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate
			)
	end
	else
	begin
		insert into @t  select isResourceId, isResourceName, isResourceFamilyId, isOEE, isAvailability, isPerformance, isQuality 
			from isGetOEEValuesPerResourceId 
			(
			@pisResourceId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate
			)
	end	
	RETURN
end


GO

 IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isOEEGetRandomDistinguishableColorVw')
                    --AND type IN (N'U', N'IF', N'TF') 
					)
	DROP VIEW isOEEGetRandomDistinguishableColorVw;
GO 

CREATE VIEW isOEEGetRandomDistinguishableColorVw
AS
	SELECT TOP 1 ColorName, ColorValue 
	FROM 
	(	values
			('aqua',  '#00ffff'),
			('azure',  '#f0ffff'),
			('beige',  '#f5f5dc'),
			('black',  '#000000'),
			('blue',  '#0000ff'),
			('brown',  '#a52a2a'),
			('cyan',  '#00ffff'),
			('darkblue',  '#00008b'),
			('darkcyan',  '#008b8b'),
			('darkgrey',  '#a9a9a9'),
			('darkgreen',  '#006400'),
			('darkkhaki',  '#bdb76b'),
			('darkmagenta',  '#8b008b'),
			('darkolivegreen',  '#556b2f'),
			('darkorange',  '#ff8c00'),
			('darkorchid',  '#9932cc'),
			('darkred',  '#8b0000'),
			('darksalmon',  '#e9967a'),
			('darkviolet',  '#9400d3'),
			('fuchsia',  '#ff00ff'),
			('gold',  '#ffd700'),
			('green',  '#008000'),
			('indigo',  '#4b0082'),
			('khaki',  '#f0e68c'),
			('lightblue',  '#add8e6'),
			('lightcyan',  '#e0ffff'),
			('lightgreen',  '#90ee90'),
			('lightgrey',  '#d3d3d3'),
			('lightpink',  '#ffb6c1'),
			('lightyellow',  '#ffffe0'),
			('lime',  '#00ff00'),
			('magenta',  '#ff00ff'),
			('maroon',  '#800000'),
			('navy',  '#000080'),
			('olive',  '#808000'),
			('orange',  '#ffa500'),
			('pink',  '#ffc0cb'),
			('purple',  '#800080'),
			('violet',  '#800080'),
			('red',  '#ff0000'),
			('silver',  '#c0c0c0'),
			('white',  '#ffffff'),
			('yellow', '#ffff00')
		)
		as isColorsTemp (ColorName , ColorValue) 
		ORDER BY NEWID()

GO


 IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEENewID')
                    --AND type IN (N'U', N'IF', N'TF') 
					)
	DROP VIEW isGetOEENewID;
GO 

CREATE VIEW isGetOEENewID  as select newid() as new_id

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetTopResourceDowntimesByResourceStatusReasonName]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetTopResourceDowntimesByResourceStatusReasonName
GO 
CREATE FUNCTION isGetTopResourceDowntimesByResourceStatusReasonName(
		@ResourceId char(16),
		@NrTop int,
		@FromStartTime	DATETIME,
		@ToEndTime		DATETIME,
		@pRunDate		DATETIME
	)
RETURNS @t TABLE(
						ResourceId char(16), ResourceName nvarchar(36),
						FromReason nvarchar(36),
						DurHours float, DurMin float, DurSec int, --, DurSecMaxPrec Bigint,
						isFormattedDuration nvarchar(30)
						)
AS
Begin

DECLARE @isTemp TABLE (	ResourceStatusHistoryId char(16),
						ResourceId char(16), ResourceName nvarchar(36),
						FromStatusChangedate datetime, ToStatusChangedate datetime,
						DurHours float, DurMin float, DurSec int, DurSecMaxPrec Bigint,
						FromOEELossCat int, ToOEELossCat int,
						FromStatus nvarchar(36),ToStatus nvarchar(36),
						FromReason nvarchar(36),ToReason nvarchar(36),
						FromResourceState nvarchar(36), ToResourceState nvarchar(36),
						FromResourceStateDesc nvarchar(36),ToResourceStateDesc nvarchar(36),
						FromAvailability int, ToAvailability int
						)

;with cte_ResourceStatusHistory (	ResourceStatusHistoryId,
									ResourceId, ResourceName, 
									FromStatusChangedate, ToStatusChangedate,
									DurSec, --DurHours, DurMin,  DurMillisec,
									FromOEELossCat, ToOEELossCat,
									FromStatus, ToStatus, --oldrsc.ResourceStatusCodeName --newrsc.ResourceStatusCodeName
									FromReason,  ToReason,--oldrsr.ResourceStatusReasonName --newrsr.ResourceStatusReasonName
			                        FromResourceState, ToResourceState, --RSH.OldResourceState RSH.ResourceState
									FromResourceStateDesc, ToResourceStateDesc,
									FromAvailability, ToAvailability		--,RSH.OldAvailability rsh.availability
									)
as
( 
        SELECT TOP(10000)
            RSH.ResourceStatusHistoryId								ResourceStatusHistoryId
			,R.ResourceId                                           ResourceId
			,R.ResourceName                                         ResourceName
			--,rsh.oldlaststatuschangedate							oldlaststatuschangedate
			--Remarks ,ISNULL(rsh.oldlaststatuschangedate, CS.LastChangeDate) FromStatusChangedate IS WRONG BECAUSE CS.LastChangeDate COULD BE GREATER THAN  laststatuschangedate if you edit and save a Resource
			,rsh.oldlaststatuschangedate						   FromStatusChangedate
		    ,rsh.laststatuschangedate                              ToStatusChangedate
			
			--DurHours  --convert(decimal(10,2), datediff(second, rsh.oldlaststatuschangedate, @ToEndTime) / 3600.0)
			 --			DATEDIFF(hour, rsh.oldlaststatuschangedate, rsh.laststatuschangedate)  truncates to zero decimals therefore to have hours use minutes divided by 60.0
			 --DurMin   -- convert(decimal(10,2), datediff(second, rsh.oldlaststatuschangedate, @ToEndTime) / 60.0)
			  --		DATEDIFF(minute, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) truncates to zero decimals therefore to have minutes use seconds divided by 60.0
			,CASE 
			  WHEN rsh.oldlaststatuschangedate IS NULL THEN 0

					--	----------------------------------------||Failure-----------------------Running/Failure||--------------------
					--	---------------------------------------------------||dtFrom------------------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate <= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom---------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, @ToEndTime) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom------------------------------------------------------------dtTo||----------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate < @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------------------------------||dtFrom-----dtTo||-----------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, @ToEndTime) 
			  ELSE	DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 
			END as DurSec

			--	Convert(Bigint,Datediff(second, rsh.oldlaststatuschangedate, @ToEndTime))*1000 -- To Milliseconds

			--,DATEDIFF(minute, COALESCE(rsh.oldlaststatuschangedate, hml.TxnDate), rsh.laststatuschangedate) as DurationMin
            ,RSH.isOldOEELossCategory                              FromOEELossCat
            ,RSH.isOEELossCategory                                 ToOEELossCat
            --Remarks: ResourceStatusCodeName  is the "Resource Status Code" in the "Resource Status Code" modeling form 
			,oldrsc.ResourceStatusCodeName                         FromStatus
            ,newrsc.ResourceStatusCodeName                         ToStatus 
            ,oldrsr.ResourceStatusReasonName					   FromReason
            ,newrsr.ResourceStatusReasonName                       ToReason
            --Remarks:  ResourceState is not visibile in Resource Setup Transition BUT ONLY in Modeling | "Resource Status Code" form 
			--			It is an enum (e.g. Nonscheduling Time, Productive Time, Engineering Time, Scheduled Downtime, Standby Time
			,RSH.OldResourceState                                  FromResourceState
            ,RSH.ResourceState                                     ToResourceState
			,CASE RSH.OldResourceState
			  WHEN 1 THEN 'Nonscheduled Time'	WHEN 2 THEN 'Nonscheduled Downtime'	WHEN 3 THEN 'Scheduled Downtime'
			  WHEN 4 THEN 'Engineering Time'	WHEN 5 THEN 'Productive Time'		WHEN 6 THEN 'Standby Time'
			  --it can be null
			  else  ''
			END														FromResourceStateDesc
			,CASE RSH.ResourceState
			  WHEN 1 THEN 'Nonscheduled Time'	WHEN 2 THEN 'Nonscheduled Downtime'	WHEN 3 THEN 'Scheduled Downtime'
			  WHEN 4 THEN 'Engineering Time'	WHEN 5 THEN 'Productive Time'		WHEN 6 THEN 'Standby Time'
			  else  'Please check'
			END													   ToResourceStateDesc
			--,rsh.OldLastActivityDate								OldLastActivityDate
            ,RSH.OldAvailability                                   FromAvailability	--Defines the old (prior to this transaction) availability status for a Resource.
		    ,rsh.availability                                      ToAvailability
			--,oldlaststatuschangedateGMT							   FromStatusChangedateGMT
			--,rsh.laststatuschangedateGMT						   ToStatusChangedateGMT
			--,rsh.OldLastActivityDateGMT							   OldLastActivityDateGMT
			--,RSH.HistoryMainlineId								   HistoryMainlineId
			--,RSH.TxnId											   TxnId
			--,hml.TxnDateGMT											TxnDateGMT
			--,hml.SystemDate											SystemDate
			--,hml.SystemDateGMT									   SystemDateGMT
			--,hml.TxnType										   TxnType		--Transaction type.  Returns "MyMove" as opposed to "Move".
			--,hml.MfgDate											MfgDate
			--,hml.TxnDate											TxnDate
			--,hml.Status											   Status      --Determines the current status (Active, In-Active) of this instance. 1 = Active 2 = Inactive
			--,mfgcal.MfgCalendarName								   MfgCalendarName
			--,sh.ShiftName										   ShiftName
			--,calshift.CalendarDate								   CalendarDate
			--,calshift.ShiftStart								   ShiftStart
			--,calshift.ShiftEnd									   ShiftEnd
			--,hml.ProductId										   ProductId
			--,ps.LastActivityDate									ProductionStatus_LastActivityDate
			--,ps.LastStatusChangeDate								ProductionStatus_LastStatusChangeDate
            --REMARKS: STE CS.LastChangeDate  is always present since when I create ChangeStatus record along with ResourceDef during data population 
			--,CS.LastChangeDate									    ChangeStatus_LastChangeDate
        FROM ResourceDef R
			INNER JOIN ChangeStatus CS ON R.ResourceId = CS.ParentId
			INNER JOIN ResourceStatusHistory RSH ON R.ResourceId = RSH.HistoryId
			LEFT OUTER JOIN ProductionStatus ps  ON R.ProductionStatusId = ps.ProductionStatusId
			-- Old
			LEFT OUTER JOIN ResourceStatusCode oldrsc ON oldrsc.ResourceStatusCodeId = rsh.OldResourceStatusCodeId
			LEFT OUTER JOIN ResourceStatusReason oldrsr ON oldrsr.ResourceStatusReasonId = rsh.OldResourceStatusReasonCodeId
			-- New
			LEFT OUTER JOIN ResourceStatusCode newrsc ON newrsc.ResourceStatusCodeId = rsh.ResourceStatusCodeId
			LEFT OUTER JOIN ResourceStatusReason newrsr ON newrsr.ResourceStatusReasonId = rsh.ResourceStatusReasonCodeId
			--LEFT OUTER JOIN HistoryMainline hml ON hml.HistoryMainlineId = rsh.HistoryMainlineId
			--LEFT OUTER JOIN CalendarShift calshift ON calshift.CalendarShiftId = hml.CalendarShiftId
			--LEFT OUTER JOIN Shift sh ON calshift.ShiftId = sh.ShiftId
			--LEFT OUTER JOIN MfgCalendar mfgcal ON calshift.MfgCalendarId = mfgcal.MfgCalendarId
        WHERE 
		--====================================================================================================================
			 r.ResourceId = @ResourceId
			and  r.isIncludeInOEE = 1 
			and (@FromStartTime <= @ToEndTime)   --to avoid user's mistakes
			and		(
						@FromStartTime is null OR  
						@FromStartTime <= rsh.laststatuschangedate
					)
			and @ToEndTime >= ISNULL(rsh.oldlaststatuschangedate, rsh.laststatuschangedate)
			and (RSH.isOldOEELossCategory = 1)					--ONLY FAILURES
			and (oldrsr.ResourceStatusReasonName is not NULL)	--ONLY records having ResourceStatusReasonName
		order by FromStatusChangedate ASC
)
	--SELECT 
	--	ResourceStatusHistoryId,
	--	ResourceId, ResourceName, 
	--	FromStatusChangedate, ToStatusChangedate, --DATENAME(weekday, FromStatusChangedate) AS FromStatusChangedateWD, DATENAME(weekday, ToStatusChangedate) AS ToStatusChangedateWD,
	--	Hours, DurMin, DurSec, DurMillisec,
	--	FromOEELossCat, ToOEELossCat,
	--	FromStatus, ToStatus, 
	--	FromReason,  ToReason,
	--	FromResourceState, ToResourceState, 
	--	FromResourceStateDesc, ToResourceStateDesc,
	--	FromAvailability, ToAvailability

	--	from cte_ResourceStatusHistory
	--	order by FromStatusChangedate

	INSERT into @isTemp(ResourceId, ResourceName, DurSec, FromReason)  --,DurHours, DurMin, DurSecMaxPrec
	select
		max(ResourceId), max(ResourceName),
		--sum(DurHours) as DurHours, sum(DurMin) as DurMin, 
		sum(DurSec) as DurSec, 
		--sum(DurMillisec)/1000 as DurSecMaxPrec,
		FromReason
		from cte_ResourceStatusHistory
		group by FromReason

	declare @isTempCount int = 0
	select @isTempCount  = count(*) from @isTemp

	DECLARE @GetIfOnlyFailure int
	set @GetIfOnlyFailure	= 1

	DECLARE @GetResourceStatusReason int
	set @GetResourceStatusReason	= 1

		--Add the virtually created record starting from ToStatusChangedate of the last record to the "To" date selected by user
	insert into @isTemp 
			(
			ResourceId, ResourceName, 
			FromStatusChangedate, ToStatusChangedate,
			DurHours, DurMin, DurSec, 
			--isOEELossCategory,
			FromStatus, ToStatus,
			FromReason, ToReason
			)
		select 
			ResourceId, ResourceName, 
			FromStatusChangedate, ToStatusChangedate,
			DurHours, DurMin, DurSec, 
			--isOEELossCategory,
			FromStatus, ToStatus,
			--FromReason, ToReason FromReason is NULL if first record therefore I must get ToReason also for FromReason because this is a virtual record
			ToReason, ToReason
		from isGetOEEResourceStatusHistoryVirtualRecord(@ResourceId, @FromStartTime, @ToEndTime, @pRunDate, @GetIfOnlyFailure, @GetResourceStatusReason)

	--Begin Function
	insert into @t  
			select TOP(@NrTop) 
				ResourceId, ResourceName,
				FromReason,
				round(DurSec / 3600.0, 2) as DurHours,
				round(DurSec / 60.0, 2) as DurMin,
				DurSec,  --,DurHours, DurMin, DurSecMaxPrec
				cast(cast((DurSec)/3600 as int) as varchar(3)) 
				+':'+ right('0'+ cast(cast(((floor(DurSec))%3600)/60 as int) as varchar(2)),2) 
				+':'+ right('0'+ cast(((floor(DurSec))%3600)%60 as varchar(2)),2) 
				as isFormattedDuration			
		from @isTemp
		order by  DurSec  DESC --DurSec DESC  DurSecMaxPrec
	RETURN
	--End Function
end
 
 GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEETopDowntimeReasonsPerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEETopDowntimeReasonsPerResourceId
GO 

CREATE FUNCTION isGetOEETopDowntimeReasonsPerResourceId (
						@ResourceId char(16), 
						@TimeSpanType int = 1, 
						@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
						@pRunDate As DateTime,
						@RequestedCount int
						)
RETURNS @t TABLE(
		isResourceName			nvarchar(30)	NULL,
		isReasonName			nvarchar(30)	NULL,
		DurHours float, DurMin float, isReasonDuration float, --, DurSecMaxPrec Bigint,
		isFormattedDuration		nvarchar(30)	NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit
		Declare @ResourceFamilyId char(16) = ''
		
		--select CDODefId from CDODefinition where CDOName  = 'Resource' 1490 = 0x5d2 --0005d28000000001 0005d2
		--if (LEFT(@ResourceId,6) = '0005d2')
		--begin
		--	SELECT  @ResourceName = ResourceName,
		--			@ResourceFamilyId = ResourceFamilyId
		--	FROM	ResourceDef r
		--	WHERE	r.ResourceId = @ResourceId
		--		and r.isIncludeInOEE = 1
		--end
		--else
		--begin
			--QUERY By Name to let automatic tests work easily
			Declare @ActualResourceId nvarchar(30) = ''
			SELECT  @ActualResourceId = ResourceId,
					@ResourceName = ResourceName,
					@ResourceFamilyId = ResourceFamilyId
			FROM	ResourceDef r
			WHERE	r.ResourceId = @ResourceId
				and r.isIncludeInOEE = 1			
			
			set @ResourceId = @ActualResourceId
		--end

		DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
		DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
		--Remarks: do not prefix the schema name in table-valued function
		select top(1)
		@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
			isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

		insert into @t  
					(isResourceName, isReasonName, DurHours, DurMin, isReasonDuration, isFormattedDuration) 
			select ResourceName as isResourceName,FromReason as isReasonName, 
						DurHours, DurMin, DurSec as isReasonDuration, 
						isFormattedDuration 
				 from  isGetTopResourceDowntimesByResourceStatusReasonName (@ResourceId, @RequestedCount, @FromStartTime, @ToEndTime, @pRunDate) order by DurSec DESC


		END 

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEETopDowntimeReasons ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isGetOEETopDowntimeReasons ]
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isGetOEETopDowntimeReasons (  
         @pisResourceFamilyId char(16) = '',   
         @pisResourceId char(16) = '',   
         @pTimeSpanType int = 1 ,   
         @pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE,   
         @pRunDate  DATETIME = GETDATE,  
         @RequestedCount int  
         )  
RETURNS @t TABLE(  
   isResourceName  nvarchar(30) NULL,  
   isReasonName  nvarchar(30) NULL,  
   DurHours   float   NULL,  
   DurMin    float   NULL,  
   isReasonDuration float   NULL,  
   isFormattedDuration nvarchar(30) NULL  
  )  
AS  
Begin  
 if (@pTimeSpanType is null OR @pTimeSpanType <= 0 OR @pTimeSpanType > 3)  
 begin  
  set @pTimeSpanType = 0  
 end  
   
 --declare @emptyDate DATETIME = '1/1/1900'  
 declare @emptyDate DateTime = 0 --select @defaultDateTime --1900-01-01 00:00:00.0000000  
  
 if (@pStartTimeDate is null OR @pStartTimeDate = @emptyDate)  
 begin  
  Set @pStartTimeDate = @pRunDate --GetDate()  
 end  
 if (@pEndTimeDate is null OR @pEndTimeDate = @emptyDate)  
 begin  
  Set @pEndTimeDate = @pRunDate --GetDate()   
 end  
   
 if (@pisResourceId is not null and @pisResourceId<>'')  
 begin  
	  insert into @t  select isResourceName, isReasonName,   
	   DurHours,  
	   DurMin,  
	   isReasonDuration,  
	   isFormattedDuration  
	   from isGetOEETopDowntimeReasonsPerResourceId   
	   (  
	   @pisResourceId,  
	   @pTimeSpanType,  
	   @pStartTimeDate, @pEndTimeDate, @pRunDate,  
	   @RequestedCount  
	   )  
 end   
 ELSE IF (@pisResourceFamilyId is not null and @pisResourceFamilyId <> '')
 BEGIN
	DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
		SELECT ResourceId FROM ResourceDef WHERE ResourceFamilyId = @pisResourceFamilyId

	OPEN csr
	
	FETCH NEXT FROM csr INTO @pisResourceId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		insert into @t  select isResourceName, isReasonName,   
			DurHours,  
			DurMin,  
			isReasonDuration,  
			isFormattedDuration  
			from isGetOEETopDowntimeReasonsPerResourceId   
			(  
			@pisResourceId,  
			@pTimeSpanType,  
			@pStartTimeDate, @pEndTimeDate, @pRunDate,  
			@RequestedCount  
			)  
		FETCH NEXT FROM csr INTO @pisResourceId
	END
	CLOSE csr
	DEALLOCATE csr
	
 END

 RETURN  
end  
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEETopScrapAndReworkByReasonName]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEETopScrapAndReworkByReasonName
GO 
CREATE FUNCTION isGetOEETopScrapAndReworkByReasonName(
	@ResourceFamilyId char(16),
	@ResourceId char(16),
	@NrTop int,
    @FromStartTime DATETIME,
    @ToEndTime  DATETIME
	)
RETURNS @t TABLE(
						ResourceId char(16), ResourceName nvarchar(36),
						ScrapReworkType nvarchar(36),
						ReasonName nvarchar(36),
						Qty float
						)
AS
Begin
	--ScrapRwType  -->  Scrap    (LossReason and QtyAdjustReason (with negative qty))  
	--					Rework
	--Reason	--> coming from decoding of LossReason OR QtyAdjustReason OR ReworkReason
	--Qty		-->
DECLARE @isTemp TABLE (
						TxnDate datetime,
						ResourceId char(16), ResourceName nvarchar(36),	--ChangeQtyType int, ChangeQtyTypeDesc varchar(36),
						Qty float,
						--ReasonCodeId char(16),
						ReasonName nvarchar(36), --	LossReasonName varchar(36), QtyAdjustReasonName varchar(36), 
						ScrapReworkType nvarchar(36)
					)
DECLARE @isReasonTemp TABLE (
						ResourceId char(16), ResourceName nvarchar(36),
						ScrapReworkType nvarchar(36),
						ReasonName nvarchar(36),
						Qty float
					)
;with cte_LossAndQtyAdjustDetails (
						TxnDate,
						ResourceId, ResourceName, 
						--ChangeQtyType, ChangeQtyTypeDesc,
						Qty,
						--ReasonCodeId,
						--LossReasonName, --QtyAdjustReasonName
						ReasonName
						)
as
( 
	  SELECT 
		  --qh.QtyHistoryId		QtyHistoryId, --qhd.QtyHistoryId -- link between master-detail tables --useless to show
		  hml.TxnDate			--hml.TxnType , hml.Status
		  ,qh.ResourceId		--In ChangeQty Transition --> Show Additional Fields --> Resource
		  ,r.ResourceName
			--,qhd.ChangeQtyType
			-- ,case ChangeQtyType	when 2 then 'LossReason' when 4 then 'QtyAdjustReason'
			--end as ChangeQtyTypeDesc
		  ,case 
				when lr.LossReasonName is NULL		and qar.QtyAdjustReasonName is not null then abs(qhd.Qty)	-- the user must set it negative in QtyAdjustReason
				when lr.LossReasonName is not NULL	and qar.QtyAdjustReasonName is null		then abs(qhd.Qty)	-- the user must set it positive in LossReason
			end as Qty
		  --,qhd.ReasonCodeId	--LossReasonId		in table LossReason 
								--QtyAdjustReasonId in table QtyAdjustReason
		  --,lr.LossReasonName, qar.QtyAdjustReasonName	
		  ,case 
				when lr.LossReasonName is NULL		and qar.QtyAdjustReasonName is not null then qar.QtyAdjustReasonName
				when lr.LossReasonName is not NULL and qar.QtyAdjustReasonName is null then lr.LossReasonName
			end as ReasonName
	 FROM 
			QtyHistory qh
					join ResourceDef		r   on r.ResourceId			= qh.ResourceId
			        join HistoryMainline	hml on hml.HistoryMainlineId= qh.HistoryMainlineId
					join QtyHistoryDetails	qhd on qhd.QtyHistoryId		= qh.QtyHistoryId
		left outer	join LossReason			lr  on lr.LossReasonId		= qhd.ReasonCodeId
		left outer	join QtyAdjustReason	qar on qar.QtyAdjustReasonId= qhd.ReasonCodeId
     WHERE 
			(r.ResourceId = @ResourceId OR (r.ResourceFamilyId = @ResourceFamilyId AND @ResourceFamilyId IS NOT NULL))
			and  r.isIncludeInOEE = 1 
			and (@FromStartTime <= @ToEndTime)   --to avoid user's mistakes
			and		(
						@FromStartTime is null OR  
						@FromStartTime <= hml.TxnDate
					)
			and @ToEndTime >= hml.TxnDate
			and (
					(qhd.ChangeQtyType = 2 and (lr.LossReasonName is not NULL))			-- records having LossReasonName 
					OR	
					(qhd.ChangeQtyType = 4 and (qar.QtyAdjustReasonName is not NULL)  and qhd.Qty < 0.0)
				) 
)
	INSERT into @isTemp(
						TxnDate,
						ResourceId, ResourceName,	--ChangeQtyType, ChangeQtyTypeDesc,
						Qty,
						--ReasonCodeId,				--LossReasonName, QtyAdjustReasonName, 
						ReasonName,
						ScrapReworkType
					)
	select
						TxnDate,
						ResourceId, ResourceName,	--ChangeQtyType, ChangeQtyTypeDesc,
						Qty,
						--ReasonCodeId,				--LossReasonName, QtyAdjustReasonName,
						ReasonName,
						'Scrap'
		from cte_LossAndQtyAdjustDetails

;with cte_ReworkDetails (
						TxnDate,
						ResourceId, ResourceName, 
						--ChangeQtyType, ChangeQtyTypeDesc,
						Qty,
						--ReasonCodeId,
						--LossReasonName, --QtyAdjustReasonName
						ReasonName
						)
as
( 
	  SELECT
		  --rwh.MoveHistoryId,	--001a2c8000000003 Rework            000b2c8000000004 MoveHistory
		  --rwh.HistoryId,  --ContainerId
		  hml.TxnDate	--hml.TxnType,			-- 6660  = Rework			6640 MoveHistory
		  ,hml.ResourceId
		  ,r.ResourceName
		  ,rwh.Qty										--,cnt.ContainerName,rwh.ProductId, pdb.ProductName
		  --,rwh.ReworkReasonId			ReasonCodeId	--ReworkReasonId in ReworkReason
		  ,rr.ReworkReasonName			ReasonName
		  --,rwh.TxnId
		  --,rwh.HistoryMainlineId
		FROM MoveHistory rwh
		left outer join HistoryMainline hml ON hml.HistoryMainlineId = rwh.HistoryMainlineId
		left outer join ResourceDef r on r.ResourceId = hml.ResourceId
		left outer join ReworkReason rr on rr.ReworkReasonId = rwh.ReworkReasonId

        WHERE 
			(r.ResourceId = @ResourceId OR (r.ResourceFamilyId = @ResourceFamilyId AND @ResourceFamilyId IS NOT NULL))
			and r.isIncludeInOEE = 1 
			and TxnType = 6660			-- Rework
			and (@FromStartTime <= @ToEndTime)   --to avoid user's mistakes
			and		(
						@FromStartTime is null OR  
						@FromStartTime <= hml.TxnDate
					)
			and @ToEndTime >= hml.TxnDate
	)
	INSERT into @isTemp(
						TxnDate,
						ResourceId, ResourceName,
						Qty,
						--ReasonCodeId,				
						ReasonName,
						ScrapReworkType
					)
	select
						TxnDate,
						ResourceId, ResourceName,
						Qty,
						--ReasonCodeId,			
						ReasonName,
						'Rework'
		from cte_ReworkDetails

	--=========================================================================================================
	--===== GROUP BY ReasonName ===============================================================================
	--=========================================================================================================
	INSERT into @isReasonTemp(ResourceId, ResourceName, ScrapReworkType, ReasonName, Qty)
	select
			max(ResourceId) as ResourceId, max(ResourceName) as ResourceName,
			max(ScrapReworkType) as ScrapReworkType,
			ReasonName,
			sum(Qty) as Qty
	from	@isTemp
	group by ReasonName

	if (@NrTop is null or (@NrTop is not null and @NrTop <= 0))
		set @NrTop = 10

	insert into @t  
		select TOP(@NrTop) ResourceId, ResourceName, ScrapReworkType, ReasonName,Qty
		from @isReasonTemp
		order by  Qty  desc
	RETURN
end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEETopScrapAndReworkPerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEETopScrapAndReworkPerResourceId
GO 

CREATE FUNCTION isGetOEETopScrapAndReworkPerResourceId (
						@ResourceId char(16), 
						@TimeSpanType int = 1, 
						@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
						@pRunDate As DateTime,
						@RequestedTopScrapRwCount int
						)
RETURNS @t TABLE(
		isResourceName nvarchar(30) NULL,
		isScrapReworkType nvarchar(30) NULL,
		isReasonName nvarchar(30) NULL,
		isScrapRwQty float NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit
		Declare @ResourceFamilyId char(16) = ''
		
		--select CDODefId from CDODefinition where CDOName  = 'Resource' 1490 = 0x5d2 --0005d28000000001 0005d2
		--if (LEFT(@ResourceId,6) = '0005d2')
		--begin
		--	SELECT  @ResourceName = ResourceName,
		--			@ResourceFamilyId = ResourceFamilyId
		--	FROM	ResourceDef r
		--	WHERE	r.ResourceId = @ResourceId
		--		and r.isIncludeInOEE = 1
		--end
		--else
		--begin
			--QUERY By Name to let automatic tests work easily
			Declare @ActualResourceId nvarchar(30) = ''
			SELECT  @ActualResourceId = ResourceId,
					@ResourceName = ResourceName,
					@ResourceFamilyId = ResourceFamilyId
			FROM	ResourceDef r
			WHERE	r.ResourceId = @ResourceId
				and r.isIncludeInOEE = 1			
			
			set @ResourceId = @ActualResourceId
		--end

		DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
		DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
		--Remarks: do not prefix the schema name in table-valued function
		select top(1)
		@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
			isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

		insert into @t  
					(isResourceName, isScrapReworkType, isReasonName, isScrapRwQty) 
			select ResourceName as isResourceName, ScrapReworkType as isScrapReworkType, ReasonName as isReasonName, Qty as isScrapRwQty  
				 from  isGetOEETopScrapAndReworkByReasonName ('',@ResourceId, @RequestedTopScrapRwCount, @FromStartTime, @ToEndTime) --order by Qty DESC


		END 

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEETopScrapAndReworkReasons ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isGetOEETopScrapAndReworkReasons ]
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isGetOEETopScrapAndReworkReasons (
									@pisResourceFamilyId char(16) = '',
									@pisResourceId char(16) = '', 
									@pTimeSpanType int = 1 , 
									@pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE, 
									@pRunDate DATETIME = GETDATE,
									@RequestedTopScrapRwCount int
									)
RETURNS @t TABLE(
		isResourceName nvarchar(30) NULL,
		isScrapReworkType nvarchar(30) NULL,
		isReasonName nvarchar(30) NULL,
		isScrapRwQty float NULL
		)
AS
Begin
	if (@pTimeSpanType is null OR @pTimeSpanType <= 0 OR @pTimeSpanType > 3)
	begin
		set @pTimeSpanType = 0
	end
	
	--declare @emptyDate DATETIME = '1/1/1900'
	declare @emptyDate DateTime = 0 --select @defaultDateTime --1900-01-01 00:00:00.0000000

	if (@pStartTimeDate is null OR @pStartTimeDate = @emptyDate)
	begin
		Set @pStartTimeDate = @pRunDate --GetDate()
	end
	if (@pEndTimeDate is null OR @pEndTimeDate = @emptyDate)
	begin
		Set @pEndTimeDate = @pRunDate --GetDate() 
	end
	
	if (@pisResourceId is not null and @pisResourceId<>'')
	begin
		insert into @t  select isResourceName, isScrapReworkType, isReasonName, isScrapRwQty
			from isGetOEETopScrapAndReworkPerResourceId
			(
			@pisResourceId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate,
			@RequestedTopScrapRwCount
			)
	end
	ELSE IF (@pisResourceFamilyId is not null and @pisResourceFamilyId <> '')
	BEGIN
		DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
			SELECT ResourceId FROM ResourceDef WHERE ResourceFamilyId = @pisResourceFamilyId

		OPEN csr
	
		FETCH NEXT FROM csr INTO @pisResourceId
		WHILE @@FETCH_STATUS = 0
		BEGIN
		insert into @t  select isResourceName, isScrapReworkType, isReasonName, isScrapRwQty
			from isGetOEETopScrapAndReworkPerResourceId
			(
			@pisResourceId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate,
			@RequestedTopScrapRwCount
			)
			FETCH NEXT FROM csr INTO @pisResourceId
		END
		CLOSE csr
		DEALLOCATE csr
	END	
 
	RETURN
end
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isOEEGetRandomNumber ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isOEEGetRandomNumber ]
GO 

CREATE FUNCTION isOEEGetRandomNumber(@lowerLimit BIGINT, @upperLimit BIGINT, @GuidValue UNIQUEIDENTIFIER)
RETURNS BIGINT
AS
BEGIN
    RETURN
    (
    SELECT ABS(CAST(CAST(@GuidValue AS VARBINARY(8)) AS BIGINT)) % (@upperLimit-@lowerLimit)+@lowerLimit
    )
END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isOEEGetRandomColour ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isOEEGetRandomColour ]
GO 

/*
* Purpose:      Returns the HTML colour code for a random colour
* Inputs:       @intStyle - 0: does not filter the colour range
*                           1: avoid dark colours
*                           2: avoid light colours
*/
CREATE FUNCTION isOEEGetRandomColour (
									@intStyle int = 0,
									@GuidValue1 UNIQUEIDENTIFIER,
									@GuidValue2 UNIQUEIDENTIFIER,
									@GuidValue3 UNIQUEIDENTIFIER
									)
RETURNS varchar(7)
AS
Begin
	DECLARE @c1 char(2), @c2 char(2), @c3 char(2)
	DECLARE @i1 int, @i2 int, @i3 int
	DECLARE @strResult As varchar(255)
	DECLARE @intLow int = 0
	DECLARE @intHigh int = 255

	IF @intStyle = 1
		SET @intLow = 80

	IF @intStyle = 2
		SET @intHigh = 140
	
	DECLARE @myRandNumber1 BIGINT, @myRandNumber2 BIGINT, @myRandNumber3 BIGINT
	SELECT @myRandNumber1 = ischema460.isOEEGetRandomNumber(-300,700,@GuidValue1) --<-- Here I pass in the non-deterministic part
	SELECT @myRandNumber2 = ischema460.isOEEGetRandomNumber(-300,700,@GuidValue2) --<-- Here I pass in the non-deterministic part
	SELECT @myRandNumber3 = ischema460.isOEEGetRandomNumber(-300,700,@GuidValue3) --<-- Here I pass in the non-deterministic part
	
	--Generate random numbers
	SELECT @i1 = CAST(ROUND((@intHigh-@intLow) * @myRandNumber1 + @intLow,0) as int) 
	SELECT @i2 = CAST(ROUND((@intHigh-@intLow) * @myRandNumber2 + @intLow,0) as int) 
	SELECT @i3 = CAST(ROUND((@intHigh-@intLow) * @myRandNumber3 + @intLow,0) as int) 

	--Convert them to hex format
	SELECT @c1 = FORMAT(@i1, 'X')
	SELECT @c2 = FORMAT(@i2, 'X')
	SELECT @c3 = FORMAT(@i3, 'X')

	--Pad them to two characters
	SELECT @strResult = '#' 
		+ REPLICATE('0', 2-LEN(@c1)) + @c1
		+ REPLICATE('0', 2-LEN(@c2)) + @c2
		+ REPLICATE('0', 2-LEN(@c3)) + @c3

	RETURN @strResult
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isOEEGetRandomDistinguishableColor ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isOEEGetRandomDistinguishableColor ]
GO 

CREATE FUNCTION isOEEGetRandomDistinguishableColor()
RETURNS varchar(7)
AS
Begin
	--https://stackoverflow.com/questions/10014271/generate-random-color-distinguishable-to-humans
	DECLARE @isColorsTemp TABLE (
						ColorName varchar(36),
						ColorValue varchar(36)
					)
	INSERT into @isColorsTemp(ColorName, ColorValue)
	values
    ('aqua',  '#00ffff'),
    ('azure',  '#f0ffff'),
    ('beige',  '#f5f5dc'),
    ('black',  '#000000'),
    ('blue',  '#0000ff'),
    ('brown',  '#a52a2a'),
    ('cyan',  '#00ffff'),
    ('darkblue',  '#00008b'),
    ('darkcyan',  '#008b8b'),
    ('darkgrey',  '#a9a9a9'),
    --('darkgreen',  '#006400'),
    ('darkkhaki',  '#bdb76b'),
    ('darkmagenta',  '#8b008b'),
    --('darkolivegreen',  '#556b2f'),
    ('darkorange',  '#ff8c00'),
    ('darkorchid',  '#9932cc'),
    ('darkred',  '#8b0000'),
    ('darksalmon',  '#e9967a'),
    ('darkviolet',  '#9400d3'),
    ('fuchsia',  '#ff00ff'),
    ('gold',  '#ffd700'),
    --('green',  '#008000'),
    ('indigo',  '#4b0082'),
    ('khaki',  '#f0e68c'),
    ('lightblue',  '#add8e6'),
    ('lightcyan',  '#e0ffff'),
    --('lightgreen',  '#90ee90'),
    ('lightgrey',  '#d3d3d3'),
    ('lightpink',  '#ffb6c1'),
    ('lightyellow',  '#ffffe0'),
    ('lime',  '#00ff00'),
    ('magenta',  '#ff00ff'),
    ('maroon',  '#800000'),
    ('navy',  '#000080'),
    --('olive',  '#808000'),
    ('orange',  '#ffa500'),
    ('pink',  '#ffc0cb'),
    ('purple',  '#800080'),
    ('violet',  '#800080'),
    ('red',  '#ff0000'),
    ('silver',  '#c0c0c0'),
    ('white',  '#ffffff'),
    ('yellow', '#ffff00')
	;

	--declare @GuidValue1 UNIQUEIDENTIFIER
	--select  @GuidValue1 = new_id  from  isGetOEENewID 

	--Remarks:
	-- ORDER BY NEWID() is forbidden because: "Invalid use of a side-effecting operator 'newid' within a function."
	-- ORDER BY @GuidValue1 is forbidden because: The SELECT item identified by the ORDER BY number 1 contains a variable as part of the expression identifying a column position. Variables are only allowed when ordering by an expression referencing a column name.

	DECLARE @strResult As varchar(255)
	SELECT TOP 1 @strResult = ColorValue 
	FROM @isColorsTemp
	ORDER BY (select  new_id  from  isGetOEENewID)
	
	

	RETURN @strResult
END
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEEDowntimeCatDistribution]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEDowntimeCatDistribution
GO 
CREATE FUNCTION isGetOEEDowntimeCatDistribution(
		@ResourceId		char(16),
		@FromStartTime	DATETIME,
		@ToEndTime		DATETIME,
		@pRunDate		DATETIME,
		@CalcColorCodes	int
	)
RETURNS @t TABLE(
						ResourceId char(16), ResourceName nvarchar(30) NULL,
						isCategoryName nvarchar(30) NULL,
						isCategoryDuration float NULL,
						isOldOEELossCategory int NULL,
						isCategoryCode nvarchar(30) NULL,
						isFormattedDuration nvarchar(30) null,
						FromStatusChangedate DateTime NULL,
						ToStatusChangedate DateTime NULL
						)
AS
Begin
DECLARE @isTemp TABLE (
						ResourceId char(16), ResourceName nvarchar(36),
						FromStatusChangedate datetime, ToStatusChangedate datetime,
						isOldOEELossCategory int,
						DurHours float,DurMin float, DurSec int,
						FromStatus nvarchar(36), ToStatus nvarchar(36)
					)
;with cte_ResourceStatusHistory (
						ResourceId, ResourceName, 
						FromStatusChangedate, ToStatusChangedate,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus,
						isOldOEELossCategory
						)
as
( 
        SELECT TOP(10000)
			 r.ResourceId                                          ResourceId
			,r.ResourceName                                        ResourceName
			,rsh.oldlaststatuschangedate						   FromStatusChangedate
		    ,rsh.laststatuschangedate                              ToStatusChangedate
			,CASE 
			  WHEN rsh.oldlaststatuschangedate IS NULL THEN 0

					--	----------------------------------------||Failure-----------------------Running/Failure||--------------------
					--	---------------------------------------------------||dtFrom------------------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate <= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom---------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, @ToEndTime) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom------------------------------------------------------------dtTo||----------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate < @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------------------------------||dtFrom-----dtTo||-----------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, @ToEndTime) 
			  ELSE	DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 
			END as DurSec
			,oldrsc.ResourceStatusCodeName                         FromStatus
            ,newrsc.ResourceStatusCodeName                         ToStatus,
			rsh.isOldOEELossCategory							   isOldOEELossCategory

        FROM ResourceDef r
        INNER JOIN ResourceStatusHistory RSH	ON R.ResourceId					= RSH.HistoryId
		INNER JOIN ResourceStatusCode	 oldrsc ON oldrsc.ResourceStatusCodeId	= rsh.OldResourceStatusCodeId
		INNER JOIN ResourceStatusCode	 newrsc ON newrsc.ResourceStatusCodeId	= rsh.ResourceStatusCodeId
        WHERE 
			r.ResourceId = @ResourceId  
			and  r.isIncludeInOEE = 1 
			and rsh.oldlaststatuschangedate is not null --Remarks: the first record has no ResourceStatusCodeName therefore must be skipped

			and (@FromStartTime <= @ToEndTime)   --to avoid user's mistakes
			and		(
						@FromStartTime is null OR  
						@FromStartTime <= rsh.laststatuschangedate
					)
			and @ToEndTime >= ISNULL(rsh.oldlaststatuschangedate, rsh.laststatuschangedate)

			--and (RSH.isOldOEELossCategory = 1)  --The filter is not set BOTH RUNNING AND FAILURE CODES

			and newrsc.ResourceStatusCodeName is not null and newrsc.ResourceStatusCodeName <> ''
			and oldrsc.ResourceStatusCodeName is not null and oldrsc.ResourceStatusCodeName <> ''
		order by FromStatusChangedate ASC
)
	INSERT into @isTemp(
						ResourceId, ResourceName,
						FromStatusChangedate, ToStatusChangedate,
						isOldOEELossCategory,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus
					)
		select
						ResourceId, ResourceName, FromStatusChangedate, ToStatusChangedate,
						isOldOEELossCategory,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus
		from cte_ResourceStatusHistory

		DECLARE @GetIfOnlyFailure int
		set @GetIfOnlyFailure	= 0

		DECLARE @GetResourceStatusReason int
		set @GetResourceStatusReason	= 0

		--Add the virtually created record starting from ToStatusChangedate of the last record to the "To" date selected by user
		INSERT into @isTemp(
						ResourceId, ResourceName,
						FromStatusChangedate, ToStatusChangedate,
						isOldOEELossCategory,
						DurHours,DurMin,DurSec,
						FromStatus, ToStatus
					)
			select 
				ResourceId, ResourceName, FromStatusChangedate, ToStatusChangedate,
				isOEELossCategory,
				DurHours, DurMin, DurSec, FromStatus, ToStatus
				--,FromReason, ToReason
			from isGetOEEResourceStatusHistoryVirtualRecord (@ResourceId, @FromStartTime, @ToEndTime, @pRunDate, @GetIfOnlyFailure, @GetResourceStatusReason)
	
	if (@CalcColorCodes = 1)
	Begin
		DECLARE @isCategoryTemp TABLE (
							RowNumber int,
							ResourceId char(16), ResourceName nvarchar(36),
							RefStatus nvarchar(36), DurSec float,
							isCategoryCode nvarchar(36)
						)
		INSERT into @isCategoryTemp(RowNumber, ResourceId, ResourceName, RefStatus, DurSec)
		select
			RowNumber = ROW_NUMBER() OVER (ORDER BY FromStatus),
			max(ResourceId), max(ResourceName),
			FromStatus as RefStatus,
			sum(DurSec) as DurSec
			from @isTemp
			group by FromStatus

		DECLARE @Counter INT = 1
		DECLARE @InstanceId	VARCHAR(16)
		WHILE @Counter <= (SELECT MAX(RowNumber) FROM @isCategoryTemp)
		BEGIN

			declare @myRandNumber1  varchar(36)
			EXEC @myRandNumber1 = isOEEGetRandomDistinguishableColor 

			--declare @intStyle  int = 1
			--declare @GuidValue1 UNIQUEIDENTIFIER
			--declare @GuidValue2 UNIQUEIDENTIFIER
			--declare @GuidValue3 UNIQUEIDENTIFIER
			--select  @GuidValue1 = new_id  from  isGetOEENewID 
			--select  @GuidValue2 = new_id  from  isGetOEENewID 
			--select  @GuidValue3 = new_id  from  isGetOEENewID 
			--EXEC @myRandNumber1 = isOEEGetRandomColour @intStyle, @GuidValue1, @GuidValue2, @GuidValue3

			Update @isCategoryTemp Set isCategoryCode = @myRandNumber1 WHERE RowNumber = @Counter

			SET @Counter = @Counter + 1
		END
	end

	insert into @t  
		select ResourceId, 
					ResourceName as ResourceName, 
					FromStatus	as isCategoryName, 
					DurSec		as isCategoryDuration,
					isOldOEELossCategory as isOldOEELossCategory,
	
					CASE			--see https://www.color-hex.com/color/00ff00
						WHEN @CalcColorCodes = 1 AND isOldOEELossCategory IS NULL THEN '#00b200' -- #00ff00' is too shining --GREEN  [RGB value is (0,255,0)]
						--WHEN isOldOEELossCategory = 1 THEN 'Availability'
						--WHEN isOldOEELossCategory = 2 THEN 'Performance'
						--WHEN isOldOEELossCategory = 3 THEN 'Schedule'
						WHEN @CalcColorCodes = 1 then (select isCategoryCode from @isCategoryTemp where RefStatus = it.FromStatus) 
						else null
						END as isCategoryCode

				,cast(cast((DurSec)/3600 as int) as varchar(3)) 
				+':'+ right('0'+ cast(cast(((floor(DurSec))%3600)/60 as int) as varchar(2)),2) 
				+':'+ right('0'+ cast(((floor(DurSec))%3600)%60 as varchar(2)),2) 
				as isFormattedDuration
		,FromStatusChangedate as FromStatusChangedate
		,ToStatusChangedate as ToStatusChangedate
		
		from @isTemp it
		order by  ToStatusChangedate  ASC
	RETURN
end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEECategoryDistributionPerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEECategoryDistributionPerResourceId
GO 

CREATE FUNCTION isGetOEECategoryDistributionPerResourceId (
						@ResourceId char(16), 
						@TimeSpanType int = 1, 
						@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
						@pRunDate As DateTime,
						@CalcColorCodes	int = 1
						)
RETURNS @t TABLE(
			ResourceName nvarchar(30) NULL, 
			isCategoryName nvarchar(30) NULL, isCategoryDuration float NULL, isOldOEELossCategory int, isCategoryCode nvarchar(30) NULL,
			isFormattedDuration nvarchar(30) NULL, 
			FromStatusChangedate DateTime NULL,
			ToStatusChangedate DateTime NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit
		Declare @ResourceFamilyId char(16) = ''
		
		--select CDODefId from CDODefinition where CDOName  = 'Resource' 1490 = 0x5d2 --0005d28000000001 0005d2
		--if (LEFT(@ResourceId,6) = '0005d2')
		--begin
		--	SELECT  @ResourceName = ResourceName,
		--			@ResourceFamilyId = ResourceFamilyId
		--	FROM	ResourceDef r
		--	WHERE	r.ResourceId = @ResourceId
		--		and r.isIncludeInOEE = 1
		--end
		--else
		--begin
			--QUERY By Name to let automatic tests work easily
			Declare @ActualResourceId nvarchar(30) = ''
			SELECT  @ActualResourceId = ResourceId,
					@ResourceName = ResourceName,
					@ResourceFamilyId = ResourceFamilyId
			FROM	ResourceDef r
			WHERE	r.ResourceId = @ResourceId
				and r.isIncludeInOEE = 1			
			
			set @ResourceId = @ActualResourceId
		--end

		DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
		DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
		--Remarks: do not prefix the schema name in table-valued function
		select top(1)
		@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
			isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

		insert into @t  
					(
					ResourceName, isCategoryName, isCategoryDuration, isOldOEELossCategory, isCategoryCode,
					isFormattedDuration, FromStatusChangedate, ToStatusChangedate
					) 
			select	--ResourceId,
					ResourceName, isCategoryName, isCategoryDuration, isOldOEELossCategory, isCategoryCode,
					isFormattedDuration, FromStatusChangedate, ToStatusChangedate

				 from  isGetOEEDowntimeCatDistribution (@ResourceId, @FromStartTime, @ToEndTime, @pRefDateForShiftType, @CalcColorCodes)


		END 

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEECategoryDistributionInternal')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEECategoryDistributionInternal
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isGetOEECategoryDistributionInternal (
									@pisResourceId char(16) = '', 
									@pTimeSpanType int = 1 , 
									@pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE, 
									@pRunDate DATETIME = GETDATE,
									@CalcColorCodes	int = 1
									)
RETURNS @t TABLE(
		ResourceName nvarchar(30) NULL, 
		isCategoryName nvarchar(30) NULL, isCategoryDuration float NULL, isCategoryType int, isCategoryCode nvarchar(30) NULL,
		isFormattedDuration nvarchar(30) NULL,  FromStatusChangedate DateTime null, ToStatusChangedate DateTime null
		)
AS
Begin
	if (@pTimeSpanType is null OR @pTimeSpanType <= 0 OR @pTimeSpanType > 3)
	begin
		set @pTimeSpanType = 0
	end
	
	--declare @emptyDate DATETIME = '1/1/1900'
	declare @emptyDate DateTime = 0 --select @defaultDateTime --1900-01-01 00:00:00.0000000

	if (@pStartTimeDate is null OR @pStartTimeDate = @emptyDate)
	begin
		Set @pStartTimeDate = @pRunDate --GetDate()
	end
	if (@pEndTimeDate is null OR @pEndTimeDate = @emptyDate)
	begin
		Set @pEndTimeDate = @pRunDate --GetDate() 
	end
	
	if (@pisResourceId is not null and @pisResourceId<>'')
	begin
		insert into @t  select ResourceName, isCategoryName, isCategoryDuration, isOldOEELossCategory as isCategoryType, isCategoryCode, 
				isFormattedDuration, FromStatusChangedate, ToStatusChangedate
			from isGetOEECategoryDistributionPerResourceId
			(
			@pisResourceId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate, @CalcColorCodes
			)
	end
	RETURN
end
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEECategoryDistribution')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEECategoryDistribution
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isGetOEECategoryDistribution (  
         @pisResourceFamilyId char(16) = '',   
         @pisResourceId char(16) = '',   
         @pTimeSpanType int = 1 ,   
         @pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE,   
         @pRunDate DATETIME = GETDATE  
         )  
RETURNS @t TABLE(  
  ResourceName nvarchar(30) NULL,   
  isCategoryName nvarchar(30) NULL, isCategoryDuration float NULL, isCategoryType int, isCategoryCode nvarchar(30) NULL,  
  isFormattedDuration nvarchar(30) NULL,  FromStatusChangedate DateTime null, ToStatusChangedate DateTime null  
  )  
AS  
Begin  
 --only to manage t-sql-function with defaultparameters  
 if (@pisResourceId is not null and @pisResourceId<>'')  
 begin  
	 insert into @t  select ResourceName, isCategoryName, isCategoryDuration, isCategoryType, isCategoryCode,   
	   isFormattedDuration, FromStatusChangedate, ToStatusChangedate  
	  from isGetOEECategoryDistributionInternal  
	  (  
	  @pisResourceId,  
	  @pTimeSpanType,  
	  @pStartTimeDate, @pEndTimeDate, @pRunDate,   
	  DEFAULT --@CalcColorCodes  
	  )  
 END
 ELSE IF (@pisResourceFamilyId is not null and @pisResourceFamilyId <> '')
 BEGIN
	DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
		SELECT ResourceId FROM ResourceDef WHERE ResourceFamilyId = @pisResourceFamilyId

	OPEN csr
	
	FETCH NEXT FROM csr INTO @pisResourceId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		insert into @t  select ResourceName, isCategoryName, isCategoryDuration, isCategoryType, isCategoryCode,   
		isFormattedDuration, FromStatusChangedate, ToStatusChangedate  
		from isGetOEECategoryDistributionInternal  
		(  
		@pisResourceId,  
		@pTimeSpanType,  
		@pStartTimeDate, @pEndTimeDate, @pRunDate,   
		DEFAULT --@CalcColorCodes  
		)  
		
		FETCH NEXT FROM csr INTO @pisResourceId
	END
	CLOSE csr
	DEALLOCATE csr
 END

 RETURN  
end  
go

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isGetOEEDowntimeCatDistributionCat]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEEDowntimeCatDistributionCat
GO 
CREATE FUNCTION isGetOEEDowntimeCatDistributionCat(
		@ResourceId		char(16),
		@FromStartTime	DATETIME,
		@ToEndTime		DATETIME,
		@pRunDate		DATETIME,
		@CalcColorCodes	int
	)
RETURNS @t TABLE(
						ResourceId char(16), ResourceName nvarchar(30) NULL,
						isCategoryName nvarchar(30) NULL,
						isCategoryDuration float NULL,
						isCategoryCode nvarchar(30) NULL,
						isFormattedDuration nvarchar(30) null
						)
AS
Begin
DECLARE @isTemp TABLE (
						ResourceId char(16), ResourceName nvarchar(36),
						FromStatusChangedate datetime, ToStatusChangedate datetime,
						isOldOEELossCategory int,
						DurHours float,DurMin float, DurSec int,
						FromStatus nvarchar(36), ToStatus nvarchar(36)
					)
;with cte_ResourceStatusHistory (
						ResourceId, ResourceName, 
						FromStatusChangedate, ToStatusChangedate,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus,
						isOldOEELossCategory
						)
as
( 
        SELECT TOP(10000)
			 r.ResourceId                                          ResourceId
			,r.ResourceName                                        ResourceName
			,rsh.oldlaststatuschangedate						   FromStatusChangedate
		    ,rsh.laststatuschangedate                              ToStatusChangedate
			,CASE 
			  WHEN rsh.oldlaststatuschangedate IS NULL THEN 0

					--	----------------------------------------||Failure-----------------------Running/Failure||--------------------
					--	---------------------------------------------------||dtFrom------------------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate <= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom---------------------------dtTo||-------------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, @ToEndTime) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------||dtFrom------------------------------------------------------------dtTo||----------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime <  rsh.oldlaststatuschangedate and rsh.laststatuschangedate < @ToEndTime  THEN DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 

					--	----------------------------------------||Failure-----------------------Running/Failure||-----------------------------------------------
					--	----------------------------------------------------||dtFrom-----dtTo||-----------------------------------------------------------------
			  WHEN rsh.oldlaststatuschangedate is not null and @FromStartTime >= rsh.oldlaststatuschangedate and rsh.laststatuschangedate >= @ToEndTime  THEN DATEDIFF(second, @FromStartTime, @ToEndTime) 
			  ELSE	DATEDIFF(second, rsh.oldlaststatuschangedate, rsh.laststatuschangedate) 
			END as DurSec
			,oldrsc.ResourceStatusCodeName                         FromStatus
            ,newrsc.ResourceStatusCodeName                         ToStatus,
			rsh.isOldOEELossCategory							   isOldOEELossCategory

        FROM ResourceDef r
        INNER JOIN ResourceStatusHistory RSH	ON R.ResourceId					= RSH.HistoryId
		INNER JOIN ResourceStatusCode	 oldrsc ON oldrsc.ResourceStatusCodeId	= rsh.OldResourceStatusCodeId
		INNER JOIN ResourceStatusCode	 newrsc ON newrsc.ResourceStatusCodeId	= rsh.ResourceStatusCodeId
        WHERE 
			r.ResourceId = @ResourceId  
			and  r.isIncludeInOEE = 1 
			and rsh.oldlaststatuschangedate is not null --Remarks: the first record has no ResourceStatusCodeName therefore must be skipped

			and (@FromStartTime <= @ToEndTime)   --to avoid user's mistakes
			and		(
						@FromStartTime is null OR  
						@FromStartTime <= rsh.laststatuschangedate
					)
			and @ToEndTime >= ISNULL(rsh.oldlaststatuschangedate, rsh.laststatuschangedate)

			--and (RSH.isOldOEELossCategory = 1)  --The filter is not set BOTH RUNNING AND FAILURE CODES

			and newrsc.ResourceStatusCodeName is not null and newrsc.ResourceStatusCodeName <> ''
			and oldrsc.ResourceStatusCodeName is not null and oldrsc.ResourceStatusCodeName <> ''
		order by FromStatusChangedate ASC
)
	INSERT into @isTemp(
						ResourceId, ResourceName,
						FromStatusChangedate, ToStatusChangedate,
						isOldOEELossCategory,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus
					)
		select
						ResourceId, ResourceName, FromStatusChangedate, ToStatusChangedate,
						isOldOEELossCategory,
						--DurHours,DurMin, 
						DurSec,
						FromStatus, ToStatus
		from cte_ResourceStatusHistory

		DECLARE @GetIfOnlyFailure int
		set @GetIfOnlyFailure	= 1

		DECLARE @GetResourceStatusReason int
		set @GetResourceStatusReason	= 1

		--Add the virtually created record starting from ToStatusChangedate of the last record to the "To" date selected by user
		insert into @isTemp 
			(
			ResourceId, ResourceName, 
			FromStatusChangedate, ToStatusChangedate,
			DurHours, DurMin, DurSec, 
			isOldOEELossCategory,
			FromStatus, ToStatus
			--,FromReason, ToReason
			)
			select 
				ResourceId, ResourceName, 
				FromStatusChangedate, ToStatusChangedate,
				DurHours, DurMin, DurSec, 
				isOEELossCategory,
				FromStatus, ToStatus
				--FromReason, ToReason FromReason is NULL if first record therefore I must get ToReason also for FromReason because this is a virtual record
				--,ToReason, ToReason
			from isGetOEEResourceStatusHistoryVirtualRecord(@ResourceId, @FromStartTime, @ToEndTime, @pRunDate, @GetIfOnlyFailure, @GetResourceStatusReason)
	
		DECLARE @isCategoryTemp TABLE (
							RowNumber int,
							ResourceId char(16), ResourceName nvarchar(36),
							RefStatus nvarchar(36), DurSec int,
							isCategoryCode nvarchar(36)
						)
		INSERT into @isCategoryTemp(RowNumber, ResourceId, ResourceName, RefStatus, DurSec)
		select
			RowNumber = ROW_NUMBER() OVER (ORDER BY FromStatus),
			max(ResourceId), max(ResourceName),
			FromStatus as RefStatus,
			sum(DurSec) as DurSec
			from @isTemp
			group by FromStatus

		if (@CalcColorCodes = 1)
		Begin
			DECLARE @Counter INT = 1
			DECLARE @InstanceId	VARCHAR(16)
			WHILE @Counter <= (SELECT MAX(RowNumber) FROM @isCategoryTemp)
			BEGIN

				declare @myRandNumber1  varchar(36)
				EXEC @myRandNumber1 = isOEEGetRandomDistinguishableColor 

				--declare @intStyle  int = 1
				--declare @GuidValue1 UNIQUEIDENTIFIER
				--declare @GuidValue2 UNIQUEIDENTIFIER
				--declare @GuidValue3 UNIQUEIDENTIFIER
				--select  @GuidValue1 = new_id  from  isGetOEENewID 
				--select  @GuidValue2 = new_id  from  isGetOEENewID 
				--select  @GuidValue3 = new_id  from  isGetOEENewID 
				--EXEC @myRandNumber1 = isOEEGetRandomColour @intStyle, @GuidValue1, @GuidValue2, @GuidValue3

				Update @isCategoryTemp Set isCategoryCode = @myRandNumber1 WHERE RowNumber = @Counter

				SET @Counter = @Counter + 1
			END
		end


		insert into @t  
		  	select  ResourceId, 
					ResourceName as ResourceName, 
					RefStatus	as isCategoryName, 
					DurSec		as isCategoryDuration,
					isCategoryCode as isCategoryCode,
					cast(cast((DurSec)/3600 as int) as varchar(3)) 
					+':'+ right('0'+ cast(cast(((floor(DurSec))%3600)/60 as int) as varchar(2)),2) 
					+':'+ right('0'+ cast(((floor(DurSec))%3600)%60 as varchar(2)),2) 
					as isFormattedDuration
		
		from @isCategoryTemp it
		order by  DurSec  DESC
	RETURN
end

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'isGetOEECatDistributionCatPerResourceId')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION isGetOEECatDistributionCatPerResourceId
GO 

CREATE FUNCTION isGetOEECatDistributionCatPerResourceId (
						@ResourceId char(16), 
						@TimeSpanType int = 1, 
						@pStartTimeDate As DateTime, @pEndTimeDate As DateTime,
						@pRunDate As DateTime,
						@CalcColorCodes	int = 1
						)
RETURNS @t TABLE(
			ResourceName nvarchar(30) NULL, 
			isCategoryName nvarchar(30) NULL, isCategoryDuration int NULL, isCategoryCode nvarchar(30) NULL,
			isFormattedDuration nvarchar(30) NULL
		)
AS 
Begin
--
--  2018 Siemens Product Lifecycle Management Software Inc.
--
	if (@ResourceId IS NOT NULL)
	begin
		Declare @ResourceName nvarchar(30)
		Declare @isIncludeInOEE bit
		Declare @ResourceFamilyId char(16) = ''
		
		--select CDODefId from CDODefinition where CDOName  = 'Resource' 1490 = 0x5d2 --0005d28000000001 0005d2
		--if (LEFT(@ResourceId,6) = '0005d2')
		--begin
		--	SELECT  @ResourceName = ResourceName,
		--			@ResourceFamilyId = ResourceFamilyId
		--	FROM	ResourceDef r
		--	WHERE	r.ResourceId = @ResourceId
		--		and r.isIncludeInOEE = 1
		--end
		--else
		--begin
			--QUERY By Name to let automatic tests work easily
			Declare @ActualResourceId nvarchar(30) = ''
			SELECT  @ActualResourceId = ResourceId,
					@ResourceName = ResourceName,
					@ResourceFamilyId = ResourceFamilyId
			FROM	ResourceDef r
			WHERE	r.ResourceId = @ResourceId
				and r.isIncludeInOEE = 1			
			
			set @ResourceId = @ActualResourceId
		--end

		DECLARE @FromStartTime As DateTime, @ToEndTime As DateTime
		DECLARE @pRefDateForShiftType DateTime = @pRunDate;
			
		--Remarks: do not prefix the schema name in table-valued function
		select top(1)
		@FromStartTime = FromStartTime, @ToEndTime = ToEndTime from
			isOEEGetStartTimeEndTime (@ResourceId, @TimeSpanType, @pStartTimeDate, @pEndTimeDate, @pRefDateForShiftType);

		insert into @t  (ResourceName, isCategoryName, isCategoryDuration, isCategoryCode, isFormattedDuration) 
			select	--ResourceId,
					ResourceName, isCategoryName, isCategoryDuration, isCategoryCode, isFormattedDuration 

				 from  isGetOEEDowntimeCatDistributionCat (@ResourceId, @FromStartTime, @ToEndTime, @pRefDateForShiftType, @CalcColorCodes)


		END 

    RETURN;

END

GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[isOEECategoryDistributionCategories ]')
                    AND type IN (N'FN', N'IF', N'TF') )
	DROP FUNCTION [isOEECategoryDistributionCategories ]
GO 

--Object:  UserDefinedFunction
CREATE FUNCTION isOEECategoryDistributionCategories (
									@pisResourceId char(16) = '', 
									@pTimeSpanType int = 1 , 
									@pStartTimeDate DATETIME = GETDATE, @pEndTimeDate DATETIME = GETDATE, 
									@pRunDate DATETIME = GETDATE
									)
RETURNS @t TABLE(
		ResourceName nvarchar(30) NULL, 
		isCategoryName nvarchar(30) NULL, isCategoryDuration float NULL, isCategoryCode nvarchar(30) NULL,
		isFormattedDuration nvarchar(30) NULL
		)
AS
Begin
	if (@pTimeSpanType is null OR @pTimeSpanType <= 0 OR @pTimeSpanType > 3)
	begin
		set @pTimeSpanType = 0
	end
	
	--declare @emptyDate DATETIME = '1/1/1900'
	declare @emptyDate DateTime = 0 --select @defaultDateTime --1900-01-01 00:00:00.0000000

	if (@pStartTimeDate is null OR @pStartTimeDate = @emptyDate)
	begin
		Set @pStartTimeDate = @pRunDate --GetDate()
	end
	if (@pEndTimeDate is null OR @pEndTimeDate = @emptyDate)
	begin
		Set @pEndTimeDate = @pRunDate --GetDate() 
	end

	DECLARE @CalcColorCodes	int
	set @CalcColorCodes	= 1
	
	if (@pisResourceId is not null and @pisResourceId<>'')
	begin
		insert into @t  select ResourceName, isCategoryName, isCategoryDuration, isCategoryCode, 
				isFormattedDuration
			from isGetOEECatDistributionCatPerResourceId
			(
			@pisResourceId,
			@pTimeSpanType,
			@pStartTimeDate, @pEndTimeDate, @pRunDate, @CalcColorCodes
			)
	end
	RETURN
end
GO

