/*
-- SCRIPT:isResourcesResolvedCalendar.sql
-- DESCR: Creates stored procedures and View used to create Resources Resolved Calendar 
-- This SCRIPT file is a utility funtionality for Camstar Applications
-- Date: 3 August 2018 
-- © 2020 Siemens Product Lifecycle Management Software Inc.
*/
--#delimiter
BEGIN
EXECUTE IMMEDIATE
'CREATE OR REPLACE VIEW isResourcesDowntimeSchd 
AS 
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
	 WHERE isEndTime	>= TRUNC(SYSDATE) -30 	--Avoid Schedule Expired from 30 days
), ResFam AS (-- 
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
	 WHERE  isEndTime	>= TRUNC(SYSDATE) -30 	--Avoid Schedule Expired from 30 days
),  Res_ResFam_Overlapped AS( 
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
	  WHERE  (rf.isStartTime, rf.isEndTime ) NOT IN (SELECT isStartTime_F, isEndTime_F FROM Res_ResFam_Overlapped WHERE RESOURCEID = rf.RESOURCEID) 
)	SELECT * FROM FinalMerge';
END;
--#delimiter
BEGIN
EXECUTE IMMEDIATE
'CREATE  OR REPLACE VIEW  isResourcesDwntSchd_Calendar 
AS
WITH ResourcesCalendar AS (
SELECT f.FactoryName, mc.MfgCalendarName, cs.CalendarShiftId, cs.CalendarDate, cs.ShiftId, cs.ShiftStart, cs.ShiftEnd, r.ResourceId, r.ResourceName, rf.ResourceFamilyName, fm.isStartTime, fm.isEndTime, fm.CDOTypeId 
	FROM Factory f 
	INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
	INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
	INNER JOIN ResourceDef    r ON r.FactoryId	= f.FactoryId
	LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
	INNER JOIN isResourcesDowntimeSchd fm ON fm.ResourceId = r.ResourceId
WHERE  TRUNC(cs.ShiftStart) = cs.CalendarDate  -- Only for Downtimes Schedule
  AND fm.isStartTime >= ShiftStart AND  fm.isStartTime < ShiftEnd
) SELECT * FROM ResourcesCalendar';
END;
--#delimiter
CREATE OR REPLACE PACKAGE isResourcesResolvedCalendar AUTHID CURRENT_USER
AS
	PROCEDURE isResourceDntInsert( 
		pResourceId CHAR DEFAULT NULL,
		pCDODefId NUMBER DEFAULT 4841599);

	PROCEDURE isResourcesDntInsertByFamily (
		pResourceFamilyId CHAR DEFAULT NULL, 
		pCDOTypeId NUMBER DEFAULT 4841599);
											  
END;
--#delimiter
CREATE OR REPLACE PACKAGE BODY isResourcesResolvedCalendar
AS
	PROCEDURE isGetShift (
		CalendarDate IN OUT DATE, 
		CalendarShiftId IN OUT CHAR,  
		ShiftId IN OUT CHAR, 
		ShiftName IN OUT VARCHAR2 , 
		ShiftStart IN OUT DATE, 
		ShiftEnd IN OUT DATE, 
		isStartTime IN OUT DATE, 
		isEndTime IN OUT DATE,
		pResourceId CHAR 
		)
	AS
		RowPos NUMBER:=0;    
	BEGIN
		SELECT iCalendarDate,
			   iCalendarShiftId,
			   iShiftId,
			   iShiftStart,
			   iShiftEnd,
			   iShiftName,
			   Rn
			INTO CalendarDate,
				 CalendarShiftId,
				 ShiftId,
				 ShiftStart,
				 ShiftEnd,
				 ShiftName,
				 RowPos
		FROM( SELECT cs.CalendarDate iCalendarDate, 
					 cs.CalendarShiftId iCalendarShiftId, 
					 cs.ShiftId iShiftId, 
					 cs.ShiftStart iShiftStart, 
					 cs.ShiftEnd iShiftEnd, 
					 s.ShiftName iShiftName, 
					 ROW_NUMBER() OVER (ORDER BY ShiftEnd ASC) Rn
			   FROM Factory f 
				 INNER JOIN MfgCalendar 	mc 	ON mc.MfgCalendarId = f.MfgCalendarId
				 INNER JOIN CalendarShift 	cs 	ON mc.MfgCalendarId = cs.MfgCalendarId
				 INNER JOIN Shift		  	s 	ON s.ShiftId		= cs.ShiftId
				 INNER JOIN ResourceDef    	r 	ON r.FactoryId		= f.FactoryId
				 LEFT  JOIN ResourceFamily 	rf 	ON rf.ResourceFamilyId = r.ResourceFamilyId 
				WHERE ShiftEnd > isStartTime
				  AND r.ResourceId = pResourceId
				)
		WHERE Rn=1;
	END;	
	
	PROCEDURE isSplitShiftByDay (CalendarShiftId IN OUT CHAR, 
								 ShiftStart IN OUT DATE, 
								 ShiftEnd IN OUT DATE , 
								 ShiftName IN OUT VARCHAR2,
								 CDOTypeId NUMBER, 
								 CalendarShiftDate IN OUT DATE, 
								 isShiftId IN OUT CHAR, 
								 isStartTime IN OUT DATE,
								 isEndTime IN OUT DATE, 
								 pResourceId CHAR)
	AS
		isResolvedDowntimeSchdId CHAR(16);
		init_CalendarShiftDate DATE := CalendarShiftDate;
		init_isEndTime DATE ;
	 
		Changed INTEGER :=	0;
		this_day DATE 	:= 	CalendarShiftDate;
		cnt_day  DATE 	:= 	TRUNC(isEndTime); --set last day
		current_day DATE := this_day;
		MaxShiftEndTime DATE;
	BEGIN
		--the dowuntime schdedule can not be calculated farther than the maximum calendar date.
		SELECT  MAX(cs.ShiftEnd) INTO MaxShiftEndTime
		FROM Factory f 
			INNER JOIN MfgCalendar mc 	ON mc.MfgCalendarId = f.MfgCalendarId
			INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
			INNER JOIN Shift		  s ON s.ShiftId		= cs.ShiftId
			INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
			LEFT JOIN ResourceFamily rf ON rf.ResourceFamilyId = r.ResourceFamilyId 
		WHERE r.ResourceId =pResourceId;

		IF MaxShiftEndTime < isEndTime THEN
			isEndTime:= MaxShiftEndTime;
		END IF;

		init_isEndTime := isEndTime;

--		--While there is no change of the day, it work for any shift.
--		--{...split Days...}
		WHILE this_day<=cnt_day LOOP
		BEGIN

			IF ((isEndTime = init_isEndTime) AND Changed = 1 ) 
			THEN 
				EXIT; 
			END IF;-- it's over

			current_day := this_day;
			--{...split Shifts...}
			WHILE this_day = current_day LOOP
			BEGIN
				
				csiPRDGetNextInstanceId (CDOTypeId, isResolvedDowntimeSchdId);
				
--				--it occurs if a downtime has a set endtime that falls between shifts
				IF ShiftEnd < init_isEndTime THEN isEndTime :=ShiftEnd; ELSE isEndTime :=init_isEndTime; END IF;
				
				IF (0 <= (isEndTime - isStartTime ) ) 
				THEN -- it occurs if a downtime has a set end time that falls into an unplanned interval time (duration can not be negative)
					INSERT INTO isResolvedDowntimeSchd (CDOTypeId, ChangeCount, isCalDate, isDuration, isStartTime, isEndTime, IsFrozen, isResolvedDowntimeSchdId, isResolvedDowntimeSchdName,isShiftId,ResourceId)
						VALUES ( CDOTypeId
								,1 
								,CalendarShiftDate
								,(isEndTime - isStartTime)
								,isStartTime
								,isEndTime
								,0
								,isResolvedDowntimeSchdId
								,SYS_GUID
								,isShiftId
								,pResourceId);
				END iF;
				
				IF isEndTime >= init_isEndTime --finish
				THEN
				BEGIN
					
					this_day := cnt_day;

					EXIT;
				END;
				END IF;
				
				isStartTime:=isEndTime; --Get Shift by starttime

--				--Get calendar information for resource downtime schedule. 
				isGetShift 	(CalendarShiftDate, 
							CalendarShiftId, 
							isShiftId,
							ShiftName, 
							ShiftStart, 
							ShiftEnd, 
							isStartTime, 
							isEndTime,
							pResourceId );

				current_day := COALESCE(CalendarShiftDate, this_day +1 );

				isStartTime:=ShiftStart;

				Changed :=1;
			END;
			END LOOP;
				   
			this_day := this_day + 1;
		END;
		END LOOP;
		
	END;

	PROCEDURE isResourceDntInsert( pResourceId CHAR DEFAULT NULL,  
								   pCDODefId NUMBER DEFAULT 4841599)
	AS

		FactoryName VARCHAR2(30);
		MfgCalendarName VARCHAR2(30);
		CalendarShiftId CHAR(16);
		CalendarDate DATE;
		ShiftId CHAR(16);
		ShiftName VARCHAR2(30);
		ShiftStart DATE;
		ShiftEnd DATE;
		ResourceName VARCHAR2(30);
		ResourceFamilyName VARCHAR2(30);
		isStartTime DATE;
		isEndTime DATE;

		CURSOR cResDwtSchdCal(pRes CHAR)
		IS
			SELECT CalendarDate, CalendarShiftId , ShiftId, ShiftStart, ShiftEnd, isStartTime, isEndTime 
			  FROM isResourcesDwntSchd_Calendar 
			WHERE ResourceId= pRes;	

		v_code NUMBER;
		v_errm VARCHAR2(64); 
		checkcurrentshift NUMBER;
		ErrorMessage VARCHAR2(4000):='';
	BEGIN  
			
		DELETE isResolvedDowntimeSchd WHERE ResourceId = pResourceId;

		BEGIN
			OPEN cResDwtSchdCal(pResourceId);
			LOOP
				FETCH cResDwtSchdCal INTO CalendarDate, CalendarShiftId, ShiftId, ShiftStart, ShiftEnd, isStartTime, isEndTime;
				EXIT WHEN cResDwtSchdCal%NOTFOUND;
				isSplitShiftByDay ( CalendarShiftId, 
									ShiftStart, 
									ShiftEnd, 
									ShiftName,
									pCDODefId, 
									CalendarDate,  
									ShiftId,  
									isStartTime,
									isEndTime, 
									pResourceId );
				
			END LOOP;
			CLOSE cResDwtSchdCal;
			COMMIT;
			RETURN;
		EXCEPTION
			WHEN OTHERS THEN
			v_code := SQLCODE;
			v_errm := SUBSTR(SQLERRM, 1 , 64);
			ErrorMessage := v_code || '-' ||v_errm;
			DBMS_OUTPUT.PUT_LINE('isSplitShiftByDay ResourceId:' || pResourceId ||  ' Error:' || ErrorMessage);
			ROLLBACK;
		END;

	END; 
	
	PROCEDURE isResourcesDntInsertByFamily (pResourceFamilyId CHAR DEFAULT NULL, 
											pCDOTypeId NUMBER DEFAULT 4841599)
	AS
		ResourceId CHAR(16);
		
		CURSOR cResDwtSchdCalFamily (pResFamId CHAR) 
			IS SELECT ResourceId 
				FROM ResourceDef 
			WHERE ResourceFamilyId= pResFamId;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		checkcurrentshift NUMBER;
		ErrorMessage VARCHAR2(4000):='';
		
	BEGIN  

		OPEN cResDwtSchdCalFamily (pResourceFamilyId);
		LOOP
			FETCH cResDwtSchdCalFamily INTO ResourceId;
			EXIT WHEN cResDwtSchdCalFamily%NOTFOUND;
			BEGIN 
				isResourceDntInsert (ResourceId, pCDOTypeId);
			EXCEPTION
				WHEN OTHERS THEN 
				v_code := SQLCODE;
				v_errm := SUBSTR(SQLERRM, 1 , 64);
				ErrorMessage := v_code || '-' ||v_errm;
				DBMS_OUTPUT.PUT_LINE('isResourcesDntInsertByFamily pResourceFamilyId: ' || pResourceFamilyId || ' pResourceId: '|| ResourceId ||  ' Error:' || ErrorMessage);
			END;
		END LOOP;
		CLOSE cResDwtSchdCalFamily;
		
	END;
	
END;
--#delimiter