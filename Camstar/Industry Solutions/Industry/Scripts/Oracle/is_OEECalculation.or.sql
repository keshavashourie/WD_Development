/*
-- SCRIPT:isOEECalculation.or.sql
-- DESCR: Creates stored procedures and Functions used for OEE Metrics.
-- This PACKAGE is a utility funtionality for Camstar Applications
-- Module: OEE
--
-- Modification History:
--  Name                 Date             Action
--  -----------          ---------------  --------------------------
--  amelchionna          30/09/2018       First release	
--
--  amelchionna          05/11/2018       1.Select [FromDate,ToDate] managemet(for free selection ). 
--                                        2.Manage timespan week 
--                                              
--  amelchionna          14/11/2018       1.Implemented the new GetResourceAvailabilityLoss funtionality.
--  
--  amelchionna          04/12/2018       1.Implemented the GetOEETopDowntimeReasons funtionality.
--                                              
--  amelchionna          10/12/2018       1.Implemented the GetOEETopScrapReason funtionality.
--                                     
--  amelchionna	         17/12/2018       1.modified TimeSpanCustom constant for parameters functions and others
--   
--  amelchionna	         20/12/2018       1.Implemented the GetOEEDowntimeDistribution funtionality.
--
-- © 2020 Siemens Product Lifecycle Management Software Inc.
*/
--#delimiter
/*
-- SCRIPT:isResourcesResolvedCalendar.sql
-- DESCR: Creates stored procedures and View used to create Resources Resolved Calendar 
-- This SCRIPT file is a utility funtionality for Camstar Applications
-- Date: 3 August 2018 
-- © 2020 Siemens Product Lifecycle Management Software Inc.
*/
--#delimiter
BEGIN
csiIncreaseStringColMaxLength('ISOEERESOURCEDETAILSBYSHIFT','RESOURCEFAMILY',40);
END;
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
CREATE OR REPLACE PACKAGE isOEECalculation AUTHID CURRENT_USER 
AS

    TimeSpanShift  CONSTANT NUMBER := 1;
    TimeSpanDay    CONSTANT NUMBER := 2;
    TimeSpanWeek   CONSTANT NUMBER := 3;
    TimeSpanCustom CONSTANT NUMBER := -1;

    ScrapLoss CONSTANT  VARCHAR2(30):= 'Scrap'; 	--'LossQty';
    ScrapAdjust CONSTANT  VARCHAR2(30):= 'Scrap'; 	--'AdjustQty'; --Negative only
    ScrapRework CONSTANT  VARCHAR2(30):= 'Rework';
		
    TYPE TYP_Day IS RECORD  ( Day DATE );
	
    TYPE TAB_Days IS TABLE OF TYP_Day;
    
    FUNCTION GetShifsWeekCalendar(
	         pRunDate IN DATE
	) RETURN TAB_Days PIPELINED;

    PROCEDURE GetTimeSpanWeek (pRunDate IN DATE, 
	                           pResourceId IN CHAR, 
	                           pFromDate IN OUT DATE, 
                               pToDate IN OUT DATE);
							   
    TYPE TYP_TimeSpanCustom IS RECORD (
        vFrom DATE   := NULL,
        vTo   DATE   := NULL,
        vSet  BOOLEAN:= FALSE,
        vDuration NUMBER :=0
    );

	TYPE TYP_OEEDwntDistributionValues IS RECORD (
		ResourceName ResourceDef.ResourceName%TYPE := '', 	
		CategoryName  VARCHAR2(30) := '', 
		Duration NUMBER(20,2):=0,
		isOEELossCategory   NUMBER(2,0),
		CategoryCode VARCHAR2(7)            -- e.g. '#FFFF66' 
		);
	
	TYPE TAB_OEEDwntDistributionValues IS TABLE OF TYP_OEEDwntDistributionValues; 		

    TYPE TYP_OEEScrapReasonValues IS RECORD (
	    ResourceName ResourceDef.ResourceName%TYPE := '',   -- for Automatic Test
	    Scrap  VARCHAR2(30),                                -- Scrap = QtyLoss + negative QtyAdjust.
	    Reason  LossReason.LOSSREASONNAME%TYPE := '', 
	    Qty NUMBER(12,6):=0,							
	    ReasonRank NUMBER :=0);
	
    TYPE TAB_OEEScrapReasonValues IS TABLE OF TYP_OEEScrapReasonValues; 		

    TYPE TYP_OEEResourceDowntimeValues IS RECORD (
	    ResourceName ResourceDef.ResourceName%TYPE := '', 			      -- for Automatic Test
	    ReasonName   ResourceStatusCode.RESOURCESTATUSCODENAME%TYPE := 'UNASSIGNED',  -- Resource Status Reason 
	    ReasonDuration NUMBER(20,2) := 0, 					      				  -- Time State Duration 
	    ReasonRank NUMBER :=0);                                                   -- Rank position

    TYPE TAB_OEEResourceDowntimeValues IS TABLE OF TYP_OEEResourceDowntimeValues; 
		
    TYPE TYP_isOEERunTimeValues IS RECORD (
        ResourceGroup VARCHAR2(30),
        "Resource"    VARCHAR2(30),
        Availability  NUMBER(6,2),
        Performance   NUMBER(6,2),
        Quality NUMBER(6,2),
        OEE     NUMBER(6,2)
    );
	
    TYPE TAB_isOEERunTimeValues IS TABLE OF TYP_isOEERunTimeValues;
	
	FUNCTION GetOEEDowntimeDistribution(pResourceFamilyId IN VARCHAR2,
										pResourceId   IN VARCHAR2,
                                        pTimeSpanType IN NUMBER,
                                        pFromDate IN DATE DEFAULT NULL,
                                        pToDate   IN DATE DEFAULT NULL,
                                        pRunDate  IN DATE DEFAULT SYSDATE) 
	RETURN TAB_OEEDwntDistributionValues PIPELINED;
	
    FUNCTION GetOEETopScrapReason ( 
		 pResourceFamilyId IN VARCHAR2,
         pResourceId   IN VARCHAR2,
         pTimeSpanType IN NUMBER,
         pFromDate IN DATE DEFAULT NULL,
         pToDate   IN DATE DEFAULT NULL,
         pRunDate  IN DATE DEFAULT SYSDATE) 
    RETURN TAB_OEEScrapReasonValues PIPELINED;

    FUNCTION GetOEETopDowntimeReasons ( 
		 pResourceFamilyId IN VARCHAR2,
         pResourceId   IN VARCHAR2,
         pTimeSpanType IN NUMBER,
         pFromDate IN DATE DEFAULT NULL, 
         pToDate   IN DATE DEFAULT NULL,
         pRunDate  IN DATE DEFAULT SYSDATE) 
    RETURN TAB_OEEResourceDowntimeValues PIPELINED;
	
    FUNCTION Quality (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    FUNCTION Performance (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    FUNCTION Availability (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    FUNCTION OEE (
        pResourceFamilyId IN VARCHAR2,
        pResourceId       IN VARCHAR2,
        pTimeSpanType     IN NUMBER,
        pFromDate IN DATE DEFAULT NULL, 
        pToDate  IN DATE DEFAULT NULL,
        pRunDate IN DATE DEFAULT SYSDATE
    ) RETURN TAB_isOEERunTimeValues PIPELINED;

--    --For testting functionality are espose the public
    FUNCTION GetResourceAvailabilityLoss (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    FUNCTION PlannedProductionTime (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    FUNCTION GetPlannedDownTimeResolvedDwt (
        pTimeSpanType         IN NUMBER,
        pRunDate              IN DATE,
        pResourceId           IN VARCHAR2,
        pRemovePartialShift   IN INTEGER DEFAULT 1
    ) RETURN NUMBER;

    FUNCTION GetTimeSpan( 
	pTimeSpanType IN NUMBER, 
        pRunDate IN DATE, 
        pResourceId IN VARCHAR2,
        pRemovePartialShift IN INTEGER DEFAULT 1
	) RETURN NUMBER;
	
    FUNCTION OperatingTime (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2
    ) RETURN NUMBER;

    PROCEDURE GetCalendarShiftBy (
        pTimeSpanType   IN NUMBER,
        pRunDate        IN DATE,
        pResourceId     IN VARCHAR2,
        pShiftStart IN OUT DATE,
        pShiftEnd IN OUT DATE,
        pRecursive BOOLEAN DEFAULT FALSE
    );

    FUNCTION GetPlannedDownTimeCalendar(
        pTimeSpanType IN NUMBER,
        pRunDate IN DATE,
        pResourceId IN VARCHAR2,
        pRecursive BOOLEAN DEFAULT FALSE
	) RETURN NUMBER;
	
   PROCEDURE SetTimeSpanCustom(
        pRunDate IN DATE,
        pFromDate IN DATE DEFAULT NULL, 
        pToDate IN DATE DEFAULT NULL);

   PROCEDURE ReSetTimeSpanCustom;
		
   PROCEDURE GetTimeSpanCustom(
        pFromDate IN OUT DATE, 
        pToDate IN OUT DATE,
        pSet IN OUT VARCHAR2 );
	
   PROCEDURE isSplitShiftByDay (
	ShiftStart IN OUT DATE, 
	ShiftEnd IN OUT DATE,
	isStartTime IN OUT DATE,
	isEndTime IN OUT DATE, 
	pResourceId CHAR);
								 
END isOEECalculation;
--#delimiter
CREATE OR REPLACE PACKAGE BODY isOEECalculation
AS
--  --Global structure for Time Span Custom
    gTimeSpanCustomSetting TYP_TimeSpanCustom;
	
--  --Manage Assert for DEBUG.
    DEBUG_MODE BOOLEAN := FALSE;

    PROCEDURE assert(assertion VARCHAR2, truth BOOLEAN, Info BOOLEAN DEFAULT TRUE)
    IS
        AssertionInfo VARCHAR2(10) := CASE WHEN Info THEN 'Info:' ELSE 'Assertion:' END;
    BEGIN
	  IF truth AND DEBUG_MODE THEN
		dbms_output.put_line(AssertionInfo || assertion );
	  END IF;
    END;
		
	FUNCTION GetShifsWeekCalendar(pRunDate IN DATE) 
        RETURN TAB_Days PIPELINED
    AS
        --Return the list of the Day from Monday to Now
        Today VARCHAR2(15);  
        vRunDate DATE; i NUMBER:=0;
        tabDays TAB_Days := TAB_Days();
        recDay TYP_Day;
    BEGIN
        FOR item IN 0..6
        LOOP
            vRunDate := pRunDate - TO_NUMBER(item);
            Today := TRIM(TO_CHAR(vRunDate,'Day'));
			IF item > 0 THEN 		-- if pRunDate is Sunday then not exit, and continue.
              EXIT WHEN Today='Sunday';
			END IF;
            recDay.Day := TRUNC(vRunDate);
            PIPE ROW(recDay);
        END LOOP;
    END GetShifsWeekCalendar;
	
--	--Procedure Not used..
	PROCEDURE GetCalendarShiftTimeLimits (
	    pMinShiftStart IN OUT DATE, 
		pMaxShiftEnd IN OUT DATE, 
		pResourceId CHAR)
	AS
	BEGIN
--		--Shifts time in OEE must not be calculated farther than the limits calendar date for the resource.
		SELECT  MIN(cs.ShiftStart), MAX(cs.ShiftEnd)  INTO pMinShiftStart, pMaxShiftEnd
		FROM Factory f 
			INNER JOIN MfgCalendar mc 	ON mc.MfgCalendarId = f.MfgCalendarId
			INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
			INNER JOIN Shift		  s ON s.ShiftId		= cs.ShiftId
			INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
			LEFT JOIN ResourceFamily rf ON rf.ResourceFamilyId = r.ResourceFamilyId 
		WHERE r.ResourceId = pResourceId;
	EXCEPTION	
        WHEN NO_DATA_FOUND THEN
        NULL;
    END;
	
    PROCEDURE GetTimeSpanWeek (pRunDate IN DATE, 
                               pResourceId IN CHAR,
                               pFromDate IN OUT DATE,
                               pToDate IN OUT DATE) 
    AS
	--Return list of DATE of the Shifts for TimeSpanWeek from Monday to Now in CalendarShift.
	BEGIN
	  IF pRunDate IS NULL THEN pToDate := SYSDATE; ELSE pToDate:= pRunDate; END IF;
     
	  SELECT MIN(cs.ShiftStart) INTO pFromDate
	    FROM Factory f 
		   INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
		   INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
		   INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
		   LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
		  WHERE TO_CHAR(cs.CalendarDate) IN (SELECT DAY FROM TABLE(GetShifsWeekCalendar(pRunDate) ))
		    AND r.ResourceId = pResourceId;
    EXCEPTION	
        WHEN NO_DATA_FOUND THEN
        pFromDate := SYSDATE;
        pToDate   := SYSDATE;
    END;
	
	
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
		
           v_code NUMBER;
           v_errm VARCHAR2(64); 
           ErrorMessage VARCHAR2(4000):='';
		
	BEGIN
        -- This Procedure obtain Shift Calendar information starting from starttime date.
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
	EXCEPTION
	WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm;
		DBMS_OUTPUT.PUT_LINE('isGetShift ResourceId:' || pResourceId ||  ' ' || ErrorMessage);	
		RAISE; 
		
	END;	
	
    PROCEDURE isSplitShiftByDay (ShiftStart IN OUT DATE, 
                                 ShiftEnd IN OUT DATE,
                                 isStartTime IN OUT DATE,
                                 isEndTime IN OUT DATE, 
                                 pResourceId CHAR)
	AS

        -- This Procedure manage spanned per day, shift, startime and endtime as accumulate any 
        -- time duration that it meets in any shifts.
        -- It is used from GetTimeSpan function.
        CalendarDate DATE; 
		CalendarShiftDate DATE := TRUNC(isStartTime);
		isShiftId CHAR(16);
        CalendarShiftId CHAR(16); 
        ShiftId  CHAR(16);
        ShiftName  VARCHAR2(30);
        
		init_CalendarShiftDate DATE := TRUNC(isStartTime);
		init_isEndTime DATE := isEndTime;
	 
		Changed INTEGER :=	0;

		this_day DATE 	 := 	TRUNC(isStartTime); -- Is CalendarShiftDate;
		cnt_day  DATE 	 := 	TRUNC(isEndTime);   -- set last day
		current_day DATE := this_day;
		MaxShiftEndTime DATE;
   
        DateDiff INTERVAL DAY(9) TO SECOND;
   
        v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';		
	BEGIN
--		--It can not be calculated farther than the maximum calendar date.
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
		
		--Clear Duration.
		gTimeSpanCustomSetting.vDuration:= 0;

--		--While there is no change of the day, it work for any shift.
--		--{...split Days...}
		WHILE this_day<=cnt_day LOOP
		BEGIN

			IF ((isEndTime = init_isEndTime) AND Changed = 1 ) THEN 
				EXIT; 
			END IF;-- it's over

			current_day := this_day;
--			--{...split Shifts...}
			WHILE this_day = current_day LOOP
			BEGIN
--				--it occurs if a set endtime falls between shifts.
				IF ShiftEnd < init_isEndTime THEN isEndTime :=ShiftEnd; ELSE isEndTime :=init_isEndTime; END IF;
                
				IF (0 <= isEndTime - isStartTime  ) --(can not be negative)
				THEN 
--					--Accumulate any piece in the shifts time
					gTimeSpanCustomSetting.vDuration := gTimeSpanCustomSetting.vDuration + (isEndTime - isStartTime);
					
				END iF;

				DateDiff:=(isEndTime - init_isEndTime) DAY(9) TO SECOND;

                                IF DateDiff >='+00000000 00:00:00.000000' THEN --finish
				BEGIN
					this_day := cnt_day;

					EXIT;
				END;
				END IF;
				
				isStartTime:=isEndTime; --Get Shift by starttime

--				--Get calendar shift for resource. 
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
	EXCEPTION	
        WHEN OTHERS THEN
          v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm; 
          DBMS_OUTPUT.PUT_LINE('isSplitShiftByDay ' || ErrorMessage);	
        END;

	PROCEDURE SetTimeSpanCustom(pRunDate IN DATE,
                                    pFromDate IN DATE DEFAULT NULL, 
                                    pToDate IN DATE DEFAULT NULL)
    
	AS
	BEGIN
		IF pFromDate IS NOT NULL AND pToDate IS NOT NULL THEN
		BEGIN
                    gTimeSpanCustomSetting.vFrom :=  pFromDate; 
                    gTimeSpanCustomSetting.vTo := CASE WHEN pRunDate <= pToDate THEN pRunDate ELSE pToDate END;
                    gTimeSpanCustomSetting.vSet := TRUE;
                    gTimeSpanCustomSetting.vDuration:=0;
		END;
		ELSE
		    gTimeSpanCustomSetting.vFrom :=  NULL; 
		    gTimeSpanCustomSetting.vTo := NULL;
		    gTimeSpanCustomSetting.vSet := FALSE;
		    gTimeSpanCustomSetting.vDuration:=0;
		END IF;
	END;
	
        PROCEDURE ReSetTimeSpanCustom
	AS
	BEGIN
		gTimeSpanCustomSetting.vFrom :=  NULL; 
		gTimeSpanCustomSetting.vTo := NULL;
		gTimeSpanCustomSetting.vSet := FALSE;
		gTimeSpanCustomSetting.vDuration:=0;
		assert('Reset TimeSpan ', TRUE);	
	END;

    PROCEDURE GetTimeSpanCustom(
        pFromDate IN OUT DATE, 
        pToDate IN OUT DATE,
		pSet IN OUT VARCHAR2 )
	AS
	BEGIN
		pFromDate:= gTimeSpanCustomSetting.vFrom;
		pToDate:=  gTimeSpanCustomSetting.vTo;
		pSet:= CASE WHEN (gTimeSpanCustomSetting.vSet = TRUE) THEN 'true' ELSE 'false' END;

		assert('vFrom = ' || gTimeSpanCustomSetting.vFrom, TRUE);	
		assert('vTO = ' || gTimeSpanCustomSetting.vTo, TRUE);	
		assert('vSet = ' || CASE WHEN gTimeSpanCustomSetting.vSet THEN 'true' ELSE 'false' END, TRUE);	

	END;
	

	PROCEDURE GetCalendarShiftBy (  
                pTimeSpanType IN NUMBER, 
				pRunDate IN DATE, 
				pResourceId IN VARCHAR2,
				pShiftStart IN OUT DATE, 
				pShiftEnd IN OUT DATE,
				pRecursive BOOLEAN DEFAULT FALSE)
	IS 
--
-- The procedure set the next TimeSpan for initial RunDate/From/To when it is into hole between shifts.
-- Avoid this behavior by setting the three TimeSpan parameters into shifts, even if they pass through a hole.
--
--          ShiftStart         ShiftEnd
--              |++++++++++++++++++|              |
--              |     Time Span    | <-- Hole --> |
--              |++++++++++++++++++|              |
--                                             ShiftStart         ShiftEnd
--                                                |++++++++++++++++++|
--                                                |     Time Span    |
--                                                |++++++++++++++++++|
--  e.g:             |                              |           |
--                   |                              |           |
--                   To                             From        |
--                                                              Run  
-- 
        vSrtd date; 
		vEndd date;
        
        v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';   
        
	BEGIN

		IF pTimeSpanType= TimeSpanShift THEN
		BEGIN 
		
			SELECT cs.ShiftStart, cs.ShiftEnd INTO pShiftStart, pShiftEnd
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE pRunDate >= cs.ShiftStart AND pRunDate < cs.ShiftEnd  --Case 1:[pRunDate >= Start] and [pRunDate < Start]
			  AND r.ResourceId = pResourceId;

		EXCEPTION
			WHEN NO_DATA_FOUND THEN
			BEGIN
   			    IF NOT pRecursive THEN
				  SELECT outer.ShiftStart, outer.ShiftEnd INTO pShiftStart, pShiftEnd
					FROM ( SELECT * 
							 FROM Factory f 
								INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
								INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
								INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
								LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
							 ORDER BY cs.ShiftEnd DESC  --Case 2 :[pRunDate > ShiftEnd DESC]      
							) outer
					WHERE ROWNUM = 1 
					AND pRunDate > outer.ShiftEnd
					AND outer.ResourceId = pResourceId;
			    ELSE
				  SELECT outer.ShiftStart, outer.ShiftEnd INTO pShiftStart, pShiftEnd
					FROM ( SELECT * 
							 FROM Factory f 
								INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
								INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
								INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
								LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
							 ORDER BY cs.ShiftEnd ASC  --Case 2: (recursive)[pRunDate < ShiftEnd ASC]      
							) outer
					WHERE ROWNUM = 1 
					AND pRunDate < outer.ShiftEnd
					AND outer.ResourceId = pResourceId;
			    END IF;
                        --DBMS_OUTPUT.put_line('GetCalendarShiftBy: Warning -> NO_DATA_FOUND pRunDate < outer.ShiftEnd');
			EXCEPTION 
				WHEN NO_DATA_FOUND THEN
				BEGIN  
				
					SELECT outer.ShiftStart, outer.ShiftEnd INTO pShiftStart, pShiftEnd
						FROM ( SELECT * 
								 FROM Factory f 
									INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
									INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
									INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
									LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
								 ORDER BY cs.ShiftStart ASC --Case 3:[pRunDate < Start]
								) outer
						WHERE ROWNUM = 1 
						AND pRunDate < outer.ShiftStart
						AND outer.ResourceId = pResourceId;
                                          --DBMS_OUTPUT.put_line('GetCalendarShiftBy: Warning -> NO_DATA_FOUND pRunDate < outer.ShiftStart');
				END;		
			END;
		END;
		ELSIF pTimeSpanType = TimeSpanDay THEN
		BEGIN
--			-- for day out of the range the following call also solves the pRunDate.
			GetCalendarShiftBy (TimeSpanShift, pRunDate, pResourceId, vSrtd, vEndd); -- Recall it self for shift Span (pTimeSpanType=1)

			SELECT MIN(cs.ShiftStart), MAX(cs.ShiftEnd) INTO pShiftStart, pShiftEnd
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE TRUNC(vSrtd) = cs.CalendarDate --shift start has the same CalendarDate.
			  AND r.ResourceId = pResourceId;

		END;
		END IF;
		assert('TimeSpan Week GetCalendarShiftBy', (pTimeSpanType = TimeSpanWeek), FALSE);	
		
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
    END GetCalendarShiftBy;
		
	FUNCTION GetResourceAvailabilityLoss( pTimeSpanType IN NUMBER, 
                                          pRunDate IN DATE, 
                                          pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
	
	-- Preconditions for run the OEE calculation to work well AvailabilityLoss:
	--	1. Date of the database server and Camstar server must be synchronized.
	--	2. Resource Status Code configuration must be set with Loss Category = Availability for Availability Down and OEE Loss Category empty for Availability Up.
	--	3. The TimeSpan must be sufficiently large to contain the  non-scheduling downtime and or Resolved Downtime. 

		vFrom DATE;
		vTo   DATE;
		CountChangeState NUMBER:=0;
		
		vRunDate DATE;
		
		AvailabilityLoss NUMBER :=0;
		LastAvailabilityLoss NUMBER :=0;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):=''; 
		
	BEGIN
	
		vFrom:= gTimeSpanCustomSetting.vFrom;
		vTo := gTimeSpanCustomSetting.vTo;
  
		IF (NOT gTimeSpanCustomSetting.vSet) THEN
			GetCalendarShiftBy (pTimeSpanType, pRunDate , pResourceId, vFrom,  vTo);
		END IF;

		--Normalize vTo at pRunDate.
		IF pRunDate < vTo THEN vTo := pRunDate; END IF;
		
		vRunDate:=pRunDate;

		BEGIN
			SELECT COUNT(*) INTO CountChangeState FROM ResourceStatusHistory WHERE OldLastStatusChangeDate IS NOT NULL AND HistoryId  = pResourceId;
			
			IF CountChangeState  > 0 THEN
			BEGIN
				-- The logic is to Do sum of the intervals that intercepts as for as Overlapping of the 
				-- Records for resource in Down Status for all change status closed.
				
				-- Query for status changed after the first time
				WITH RSH AS (
					SELECT OldLastStatusChangeDate olds, LastStatusChangeDate lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY FROM  ResourceStatusHistory 
				), CustomDate AS (
					SELECT vFrom, vTo FROM DUAL
					
				) SELECT 
						SUM(CASE WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory IS NULL --Up to Up
							  THEN 0
							  WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN 0
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN 0 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN 0  
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN 0 
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory IS NULL   
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
											ELSE 0 END)
							  ELSE 0              
						   END) AS STATUSCHANGEDURATION  INTO AvailabilityLoss 
					FROM RSH rsh JOIN CustomDate cd ON 
					  (( (cd.vFrom >= rsh.olds AND cd.vFrom < rsh.lasts ) OR (rsh.olds >= cd.vFrom AND  rsh.olds < cd.vTo ) )
					  OR NOT (rsh.olds >= cd.vTo OR rsh.lasts <= cd.vFrom ))
					  AND rsh.olds IS NOT NULL
					  AND rsh.lasts < (SELECT MAX(LASTSTATUSCHANGEDATE) FROM ResourceStatusHistory WHERE HISTORYID = pResourceId)
					  AND rsh.HistoryId = pResourceId 
					GROUP BY olds, lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY;	 
			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				AvailabilityLoss := 0; -- The Resource it is not in the status down .
			END;
			BEGIN
				--Query for the last Status Change.
				WITH RSH AS ( 
					SELECT OldLastStatusChangeDate olds, LastStatusChangeDate lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY FROM  ResourceStatusHistory 
				), CustomDate AS (
					SELECT vFrom, vTo FROM DUAL
				) SELECT CASE WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory IS NULL --Up to Up
							  THEN 0
							  WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN 0 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN 0  
											WHEN  cd.vFrom >= rsh.olds AND cd.vFrom <= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts 
											WHEN  cd.vFrom >= rsh.olds AND cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - cd.vFrom
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN cd.vTo - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds                         THEN cd.vTo - cd.vFrom   
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory IS NULL   
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											WHEN  cd.vFrom >= rsh.olds AND cd.vFrom <= rsh.lasts AND cd.vTo >= rsh.lasts THEN rsh.lasts- cd.vFrom
											WHEN  cd.vFrom >= rsh.olds AND cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN 0
											ELSE 0 END)
							  ELSE 0              
						   END AS STATUSCHANGEDURATION  INTO LastAvailabilityLoss  
					  FROM RSH rsh JOIN CustomDate cd ON cd.vTo > rsh.olds
					   AND rsh.lasts = (SELECT MAX(LASTSTATUSCHANGEDATE) FROM ResourceStatusHistory WHERE HISTORYID = pResourceId)
					   AND rsh.HistoryId = pResourceId               
					GROUP BY olds, lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY;

			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				LastAvailabilityLoss := 0;-- The Resource it is not in the status down .
			END;		 
			ELSE
			BEGIN
--				-- Query for status changed when have an only record
				SELECT CASE  WHEN ISOEELOSSCATEGORY IS NULL THEN 0 ---Resource Up
							 WHEN ISOEELOSSCATEGORY = 1 THEN 
                                             CASE  --Resource Down
												WHEN vFrom <  LASTSTATUSCHANGEDATE AND vTo <  LASTSTATUSCHANGEDATE THEN 0
												WHEN vFrom <  LASTSTATUSCHANGEDATE AND vTo >  LASTSTATUSCHANGEDATE THEN vTo - LASTSTATUSCHANGEDATE
												WHEN vFrom >  LASTSTATUSCHANGEDATE AND vTo >  LASTSTATUSCHANGEDATE THEN vTo - vFrom 
												WHEN vFrom >  LASTSTATUSCHANGEDATE AND vTo <  LASTSTATUSCHANGEDATE THEN 0 --Input Error 
											 END 
			           END STATUSCHANGEDURATION INTO AvailabilityLoss
				  FROM ResourceStatusHistory
				  WHERE OLDLASTSTATUSCHANGEDATE IS NULL
				   AND HISTORYID = pResourceId; 
         
			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				AvailabilityLoss := 0; 
			END;
			END IF;
			
			AvailabilityLoss:=  COALESCE(AvailabilityLoss,0) +  COALESCE(LastAvailabilityLoss,0);
        
		END;
		
        IF AvailabilityLoss < 0 THEN AvailabilityLoss := 0; END IF;   -- RunDate could be is prior to the first Change status.
		assert( 'The run date value could be prior to the first change status, or from date > to date', (AvailabilityLoss < 0),FALSE);

		RETURN COALESCE(AvailabilityLoss,0);
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		RETURN 0;    
	END GetResourceAvailabilityLoss;
    

	FUNCTION GetPlannedDownTimeResolvedDwt( pTimeSpanType IN NUMBER, 
						pRunDate IN DATE, 
						pResourceId IN VARCHAR2,
						pRemovePartialShift IN INTEGER DEFAULT 1) 
	RETURN NUMBER
	AS
                CurrentDuration NUMBER:=0;
        
                vSrtd    DATE;
                vEndd 	 DATE;
                vRundate DATE;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';
	BEGIN
    
		IF (NOT gTimeSpanCustomSetting.vSet) THEN
			GetCalendarShiftBy (pTimeSpanType, pRunDate, pResourceId, vSrtd, vEndd); 
		END IF;

	    IF pTimeSpanType = TimeSpanDay AND NOT gTimeSpanCustomSetting.vSet THEN
	    BEGIN
--            -- If pRemovePartialShift true default then:
--            -- Remove Partial time only to last shift at the total durations
--            -- In the last shift of the day remove the part time to downtime overlap on pRunDate
--            -- Remove partial times to all downtime after pRunDate
           
            IF pRemovePartialShift = 0 THEN --TimeSpan Day is absolute value
            BEGIN  
                SELECT SUM(rd.isDuration) "Absolute Duration Day" INTO CurrentDuration
                    FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId=cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE pRunDate BETWEEN cs.ShiftStart  AND cs.ShiftEnd
                  AND pRunDate BETWEEN vSrtd AND vEndd
                  AND cs.CalendarDate= rd.IsCalDate
                  --AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId; 
            END;
            ELSE --TimeSpan Day is Relative value at RunDate 
            BEGIN
                SELECT SUM(CASE WHEN pRunDate BETWEEN rd.isStartTime AND rd.isEndTime  
                                THEN pRunDate - rd.isStartTime 
                                ELSE rd.isDuration
                            END )  "Relative Duration Day"  INTO CurrentDuration 
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId=cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE pRunDate BETWEEN cs.ShiftStart AND cs.ShiftEnd --Potrebbe essere rimosso per evitare che il reporting nei buchi non riponda.
                  AND pRunDate >= rd.isStartTime
                  AND pRunDate BETWEEN vSrtd AND vEndd
                  AND cs.CalendarDate= rd.IsCalDate
                  --AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId; 
            END;
            END IF;
	    
	    END;  
	    ELSIF pTimeSpanType = TimeSpanShift AND NOT gTimeSpanCustomSetting.vSet THEN
	    BEGIN
            IF pRemovePartialShift = 0 THEN --TimeSpan Shift is absolute 
            BEGIN  
                SELECT SUM(rd.isDuration) AS "Absolute Duration Shift" INTO CurrentDuration --per Shift
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId = cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE pRunDate BETWEEN cs.ShiftStart  AND cs.ShiftEnd
                  AND pRunDate >= rd.isStartTime
                  AND pRunDate BETWEEN vSrtd  AND vEndd
                  AND cs.CalendarDate= rd.IsCalDate
                  --AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId;
            END;
            ELSE  --TimeSpan Shift is Relative value at RunDate
            BEGIN
                SELECT SUM(CASE WHEN pRunDate BETWEEN rd.isStartTime AND rd.isEndTime  
                                THEN pRunDate - rd.isStartTime 
                                ELSE rd.isDuration
                           END ) AS "Relative Duration Shift" INTO CurrentDuration --per Shift
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId = cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE pRunDate BETWEEN cs.ShiftStart  AND cs.ShiftEnd
                  AND pRunDate >= rd.isStartTime
                  AND pRunDate BETWEEN vSrtd  AND vEndd
                  AND cs.CalendarDate= rd.IsCalDate
                 --AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId;
				 
            END;
			END IF;
        END;
        ELSIF pTimeSpanType = TimeSpanWeek AND NOT gTimeSpanCustomSetting.vSet THEN
          NULL;
          assert( 'TimeSpanWeek for GetPlannedDownTimeResolvedDwt has not been set to a custom time span', (pTimeSpanType = TimeSpanWeek),FALSE);
	ELSIF (gTimeSpanCustomSetting.vSet = TRUE) THEN
        BEGIN
		
            vSrtd := gTimeSpanCustomSetting.vFrom;
            vEndd := gTimeSpanCustomSetting.vTo;
            vRundate := vEndd -1/24/60/60/60; --force RunDate near EndDate; 
			
            IF pRemovePartialShift = 0 THEN --TimeSpan Shift is absolute 
            BEGIN  
                SELECT SUM(rd.isDuration) AS "Absolute Duration Custom" INTO CurrentDuration --per Custom
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId	    = f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId = cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE vRundate BETWEEN cs.ShiftStart  AND cs.ShiftEnd
                  AND vRundate >= rd.isStartTime
                  AND vRundate BETWEEN vSrtd  AND vEndd
                  AND cs.CalendarDate= rd.IsCalDate
                  AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId;
            END;
            ELSE  --TimeSpan Custom is Relative value at RunDate because it is forced near EndDate;
            BEGIN
                SELECT SUM(CASE 
							 WHEN vSrtd >= rd.isStartTime AND vEndd <= rd.isEndTime --Case 1
							 THEN vEndd - vSrtd
							 WHEN vSrtd <= rd.isStartTime AND vEndd <= rd.isEndTime --Case 2
							 THEN vEndd - rd.isStartTime
							 WHEN vSrtd >= rd.isStartTime AND vEndd >= rd.isEndTime --Case 3
							 THEN rd.isEndTime - vSrtd 
							 WHEN vSrtd <= rd.isStartTime AND vEndd >= rd.isEndTime --Case 4
							 THEN rd.isEndTime - rd.isStartTime  
							 ELSE rd.isDuration
                           END ) AS "Relative Duration Custom" INTO CurrentDuration --per Custom
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                    INNER JOIN Shift s ON s.ShiftId = cs.ShiftId
                    INNER JOIN isResolvedDowntimeSchd rd ON rd.ResourceId = r.ResourceId
                WHERE vRundate >= rd.isStartTime
                  AND vSrtd <= rd.isEndTime
                  AND cs.CalendarDate= rd.IsCalDate
                  AND s.shiftId = rd.isShiftId
                  AND r.ResourceId = pResourceId;
            END;
            END IF;
 		END;
		END IF;
		
        RETURN COALESCE(CurrentDuration,0);
        
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm;
		DBMS_OUTPUT.PUT_LINE('GetPlannedDownTimeResolvedDwt ResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN 0;
	END GetPlannedDownTimeResolvedDwt;
    
        FUNCTION GetPlannedDownTimeCalendar(pTimeSpanType IN NUMBER, 
                                        pRunDate IN DATE, 
                                        pResourceId IN VARCHAR2, 
                                        pRecursive BOOLEAN DEFAULT FALSE) 
	RETURN NUMBER
	AS
           CurrentDuration NUMBER :=0;
           PartialDuration NUMBER :=0;
	
           vShiftStart DATE; 
           vShiftEnd   DATE;
           vRunDate    DATE;
           v_code NUMBER;
           v_errm VARCHAR2(64); 
           ErrorMessage VARCHAR2(4000):='';   
		
	BEGIN

        GetCalendarShiftBy (TimeSpanDay, pRunDate , pResourceId , vShiftStart,  vShiftEnd);
	
		IF (pTimeSpanType = TimeSpanDay AND NOT gTimeSpanCustomSetting.vSet) OR --if [From, To] intervall date are not setting or this is into re-called function
		   (pTimeSpanType = TimeSpanDay AND pRecursive) THEN
		BEGIN
            
			SELECT SUM(NVL(isNonScheduledTime,0)) INTO CurrentDuration
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE TRUNC(vShiftStart) = cs.CalendarDate
			  AND r.ResourceId = pResourceId;
              
                       --Distribution of the Non-ScheduleTime in the timespan proportionally. 
                       CurrentDuration:=  (pRunDate-vShiftStart)/(vShiftEnd -vShiftStart) * CurrentDuration;
		END;  
		ELSIF pTimeSpanType = TimeSpanShift AND NOT gTimeSpanCustomSetting.vSet  OR 
		     (pTimeSpanType = TimeSpanShift AND pRecursive) THEN --if [From, To] intervall date are not setting or this is into re-called function
		BEGIN
		
			SELECT SUM(NVL(isNonScheduledTime,0)) INTO CurrentDuration
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE pRunDate >= cs.ShiftStart AND pRunDate < cs.ShiftEnd
			AND r.ResourceId = pResourceId;
            
            IF NOT pRecursive THEN           
               --Proportional distribution  of the Non-scheduled Time in the interval. 
                CurrentDuration:=  (pRunDate-vShiftStart)/(vShiftEnd -vShiftStart) * CurrentDuration;
            ELSE
                assert( 'PlannedDownTimeCalendar not applied the distribution of the Non-scheduled Time of the current shift.', TRUE, FALSE);
            END IF;
	
		END;
		ELSIF (pTimeSpanType = TimeSpanWeek AND NOT gTimeSpanCustomSetting.vSet) THEN 
			assert( 'TimeSpanWeek for GetPlannedDownTimeCalendar has not been set to a custom time span', (pTimeSpanType = TimeSpanWeek),FALSE);
		ELSIF (gTimeSpanCustomSetting.vSet ) THEN -- This is checked for future timeSpan
		BEGIN
            vShiftStart := gTimeSpanCustomSetting.vFrom; 
            vShiftEnd   := gTimeSpanCustomSetting.vTo;
            vRunDate    := vShiftStart + 1/24/60/60;

			WHILE (vShiftStart < gTimeSpanCustomSetting.vTO  ) LOOP
            
				GetCalendarShiftBy (TimeSpanShift, vRunDate , pResourceId , vShiftStart,  vShiftEnd, TRUE); --Call iteratively for any shift between vFrom and vTo
				
				IF (vShiftEnd - vShiftStart <= 0) THEN
                                   assert( vShiftEnd || ' - ' || gTimeSpanCustomSetting.vTO || ' <=0 then Return Accumulated Duration.', TRUE,FALSE);
				   
				   RETURN COALESCE(CurrentDuration,0);
				END IF;
				
				PartialDuration:=  GetPlannedDownTimeCalendar(TimeSpanShift, vShiftStart + 1/24/60/60 , pResourceId, TRUE); --Call iteratively AND Recursivly for any shift between vFrom and vTo
				
				IF vShiftEnd >= gTimeSpanCustomSetting.vTO THEN -- This last SHIFT must be distributed from RunDate
				  PartialDuration:= (gTimeSpanCustomSetting.vTo- vShiftStart)/ (vShiftEnd -vShiftStart) * PartialDuration;
				 
				  assert( 'Duration in this SHIFT: ' || '(' || gTimeSpanCustomSetting.vTo || '-' || vShiftStart || ')/(' || vShiftEnd || '-' || vShiftStart || ') * ' || PartialDuration, TRUE);
				  
				END IF;
				
                CurrentDuration := 
				     CurrentDuration + PartialDuration;

				vShiftStart := vShiftEnd;
                vRunDate    := vShiftStart; 
				
			END LOOP;
            
--            --If the TimeSpan custom is smaller then CurrentDuration could be negative.
            CurrentDuration :=  CASE 
                                    WHEN CurrentDuration < 0 -- Assert! avoid setting ToDate in the hole between the shifts (e.g eeek end)
                                    THEN 0 
									WHEN (gTimeSpanCustomSetting.vTo - gTimeSpanCustomSetting.vFrom) < CurrentDuration 
									THEN (gTimeSpanCustomSetting.vTo - gTimeSpanCustomSetting.vFrom) 
                                    ELSE CurrentDuration 
                                END;
								
	        assert('CurrentDuration < 0! avoid setting ToDate in the hole between the shifts (e.g eeek end)', CurrentDuration < 0,FALSE);
			assert('avoid setting ToDate < FromDate.', (gTimeSpanCustomSetting.vTo - gTimeSpanCustomSetting.vFrom) < CurrentDuration, FALSE);
								
		END;
		END IF;

		RETURN COALESCE(CurrentDuration,0);
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm;
		DBMS_OUTPUT.PUT_LINE('GetPlannedDownTimeCalendar ResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN CurrentDuration;    --Return the accumulated values
	END GetPlannedDownTimeCalendar;

	FUNCTION GetTimeSpan( pTimeSpanType IN NUMBER, 
                          pRunDate IN DATE, 
                          pResourceId IN VARCHAR2,
                          pRemovePartialShift IN INTEGER DEFAULT 1) 
	RETURN NUMBER
	AS
		CurrentDuration  NUMBER:=0;

        vSrtd DATE;
        vEndd DATE;

		vShiftStart DATE; 
		vShiftEnd 	DATE;
		vRunDate 	DATE; 

		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';
	BEGIN
	
		IF (NOT gTimeSpanCustomSetting.vSet) THEN
			GetCalendarShiftBy (pTimeSpanType, pRunDate , pResourceId , vSrtd,  vEndd );
		END IF;	
	
		IF (pTimeSpanType = TimeSpanDay AND NOT gTimeSpanCustomSetting.vSet ) THEN 
		BEGIN
            
            GetCalendarShiftBy (TimeSpanDay, pRunDate, pResourceId, vSrtd, vEndd);
            
            IF pRemovePartialShift = 0 THEN --TimeSpan is absolute 
            BEGIN
                SELECT SUM(cs.ShiftEnd - cs.ShiftStart) INTO CurrentDuration
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                WHERE TRUNC(vSrtd) = cs.CalendarDate
                  AND r.ResourceId = pResourceId;
            END;
            ELSE --TimeSpan is Relative at RunDate
            BEGIN
                -- The sum by grouping of ShiftStart determines the differences of pRunDate with ShiftStart of the day, therefore
                -- when pRunDate becomes smaller than ShiftStart, the value becomes negative which means in this case that the shift 
                -- does not correspond to the one within which pRunDate falls 
                SELECT MAX(SUM(pRunDate - cs.ShiftStart)) INTO CurrentDuration
                FROM Factory f 
                    INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
                    INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
                    INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
                    LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
                WHERE TRUNC(vSrtd) = cs.CalendarDate
                  AND r.ResourceId = pResourceId
                GROUP BY cs.ShiftStart;
            END;
            END IF;

		END;  
		ELSIF (pTimeSpanType = TimeSpanShift AND NOT gTimeSpanCustomSetting.vSet) THEN 
		BEGIN
			SELECT (CASE pRemovePartialShift WHEN 0 THEN cs.ShiftEnd ELSE pRunDate END - cs.ShiftStart) INTO CurrentDuration
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE  pRunDate >= cs.ShiftStart AND pRunDate < cs.ShiftEnd
			AND r.ResourceId = pResourceId;
			
		END;
		ELSIF (pTimeSpanType = TimeSpanWeek AND NOT gTimeSpanCustomSetting.vSet) THEN 
			assert( 'TimeSpanWeek for GetTimeSpan has not been set to a custom time span', (pTimeSpanType = TimeSpanWeek),FALSE);
		ELSIF (gTimeSpanCustomSetting.vSet) THEN 
		BEGIN
		    --From
		    vSrtd :=gTimeSpanCustomSetting.vFrom ; 
		    --To
			vEndd := gTimeSpanCustomSetting.vTo	;
		    --Run
		    vRunDate :=	vSrtd;

		    --Use the Manage isSplitShiftByDay 
		    GetCalendarShiftBy (TimeSpanShift, vRunDate , pResourceId , vShiftStart,  vShiftEnd,TRUE);
            isSplitShiftByDay(vShiftStart, vShiftEnd, vSrtd, vEndd, pResourceId);
		    CurrentDuration := gTimeSpanCustomSetting.vDuration;
		END;
		END IF;

		RETURN COALESCE(CurrentDuration,0);
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm;
		DBMS_OUTPUT.PUT_LINE('GetTimeSpan ResourceId:' || pResourceId ||  ' pTimeSpanType:' || pTimeSpanType || ' ' || ErrorMessage);
		RETURN 0;    
	END GetTimeSpan;
	
	FUNCTION PlannedProductionTime( 
                    pTimeSpanType IN NUMBER, 
                    pRunDate IN DATE, 
                    pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
		vPlannedProductionTime NUMBER:=0;
        vRemovePartialShift INTEGER:=1; 
		
		vGetTimeSpan NUMBER:=GetTimeSpan(pTimeSpanType, pRunDate, pResourceId); 
		vGetPlannedDownTimeCalendar NUMBER := GetPlannedDownTimeCalendar(pTimeSpanType, pRunDate, pResourceId); 
		vGetPlannedDownTimeResolvedDwt NUMBER:=GetPlannedDownTimeResolvedDwt(pTimeSpanType, pRunDate, pResourceId);
	BEGIN
		vPlannedProductionTime:= vGetTimeSpan - (vGetPlannedDownTimeCalendar + vGetPlannedDownTimeResolvedDwt);
		
		assert( '***' , TRUE);
		assert( 'PlannedProductionTime:' || vPlannedProductionTime, TRUE);
		assert( '  GetTimeSpan:' || vGetTimeSpan, TRUE);
		assert( '  GetPlannedDownTimeCalendar:' || vGetPlannedDownTimeCalendar, TRUE);
		assert( '  GetPlannedDownTimeResolvedDwt:' || vGetPlannedDownTimeResolvedDwt, TRUE);

		RETURN (vPlannedProductionTime); 
				  
	END PlannedProductionTime;

	FUNCTION OperatingTime( pTimeSpanType IN NUMBER, 
                                pRunDate IN DATE, 
                                pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
		-- Warning:
		-- If the resource has been downed for a significant period of time od the timespan then PlannedProductionTime 
		-- may be negative. This happens when Planned Downtime Calendar or Planned Resolved Downtime between 
		-- TimeSpan period exists. In this case the Avalilability values is negative.
	
		vOperatingTime 	NUMBER:= 0;
		vGetResourceAvailabilityLoss NUMBER :=GetResourceAvailabilityLoss(pTimeSpanType,pRunDate,pResourceId);
		vPlannedProductionTime NUMBER:= PlannedProductionTime(pTimeSpanType,pRunDate,pResourceId) ;
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';
	BEGIN
		vOperatingTime:= vPlannedProductionTime - vGetResourceAvailabilityLoss;
		
		assert( 'OperatingTime is negative!', (vOperatingTime<0) , TRUE);

		assert( '***' , TRUE);
		assert( 'OperatingTime:' || TO_CHAR(vOperatingTime) , TRUE);
		assert( '  PlannedProductionTime:' || TO_CHAR(vPlannedProductionTime), TRUE);
		assert( '  GetResourceAvailabilityLoss:' || TO_CHAR(vGetResourceAvailabilityLoss) , TRUE);
		
		vOperatingTime:= CASE WHEN (vOperatingTime < 0) THEN 0 ELSE vOperatingTime END;
		
		RETURN vOperatingTime;
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		DBMS_OUTPUT.PUT_LINE('OperatingTime ResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN 0;   
	END OperatingTime;
     
        FUNCTION GetCDONameById(pRes_ResFamilyId VARCHAR2)
	RETURN NUMBER
	IS 
		pCDODefId NUMBER;
		vCDORes NUMBER:=-1;
	BEGIN

		SELECT TO_NUMBER( SUBSTR(pRes_ResFamilyId,1,6) , 'xxxxxx' ) INTO pCDODefId FROM DUAL;

		SELECT CASE  WHEN CDONAME IN('Resource','Carrier','WaferCarrier','AssemblyEquipment','BackGrindEquipment','Mask','ss_Feeder','ss_FeederBank','TestEquipment','Tool','WaferEquipment','WaferSortEquipment','Part','Scale','isDevice') THEN 0 
		             WHEN CDONAME IN('ResourceFamily','CarrierFamily','WaferCarrierFamily','AssemblyEquipmentFamily','BackGrindEquipmentFamily','MaskFamily','ss_FeederBankFamily','ss_FeederFamily','TestEquipmentFamily','ToolFamily','WaferEquipmentFamily','WaferSortEquipmentFamily','isDeviceFamily','PartFamily','ScaleFamily') THEN 1 
				    WHEN CDONAME IN('ResourceGroup','CarrierGroup','WaferCarrierGroup','AssemblyEquipmentGroup','BackGrindEquipmentGroup','MaskGroup','ss_FeederBankGroup','ss_FeederGroup','TestEquipmentGroup','ToolGroup','WaferEquipmentGroup','WaferSortEquipmentGroup','isDeviceGroup','ScaleGroup')  THEN 2 
			            ELSE -1 
		       END INTO vCDORes 
		  FROM CdoDefinition 
		WHERE CDODEFID=pCDODefId;

		RETURN vCDORes;

	EXCEPTION
		WHEN OTHERS THEN
		RETURN -1;    
	END GetCDONameById;

	FUNCTION OEE (pResourceFamilyId IN VARCHAR2,
                  pResourceId       IN VARCHAR2,
		          pTimeSpanType     IN NUMBER,
                  pFromDate IN DATE DEFAULT NULL, 
                  pToDate   IN DATE DEFAULT NULL,
                  pRunDate  IN DATE DEFAULT SYSDATE
    ) RETURN TAB_isOEERunTimeValues PIPELINED
	AS 
		--
		--OEE = Availability * Performance* Quality
		--
		i NUMBER:=0;
		CDOResource  NUMBER := 0;
		CDOResourceFamily NUMBER := 1;
		CDOResourceGroup  NUMBER := 2;

		vCDO  NUMBER;
		vCalendarShiftId VARCHAR2(16);
		vCurrResourceId VARCHAR2(16);
		vCurrResourceName VARCHAR2(30);
		vCurrResourceFamilyName VARCHAR2(30);
		vTimeSpanType NUMBER := pTimeSpanType;
		vResourceFamilyId  VARCHAR2(16):=pResourceFamilyId;
		
		vFromDate DATE := pFromDate;
		vToDate   DATE := pToDate;
		
		vSrtd DATE;
		vEndd DATE;
		
		c_ResourceFamily SYS_REFCURSOR;

		recOEE_RtCalc TYP_isOEERunTimeValues;
		tabOEE_RtCalc TAB_isOEERunTimeValues;

        v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';	
		
    BEGIN
        tabOEE_RtCalc :=  TAB_isOEERunTimeValues();

		IF pResourceId IS NOT NULL THEN
			vResourceFamilyId := pResourceId;
		END IF;

		vCDO := GetCDONameById(vResourceFamilyId);
		
		-- The gTimeSpanCustomSetting structure is initialized when pFromDate and pToDate are not null.
		SetTimeSpanCustom(pRunDate, pFromDate, pToDate );
		
		IF ((gTimeSpanCustomSetting.vSet AND pRunDate > pFromDate) 
			OR NOT gTimeSpanCustomSetting.vSet ) THEN 
		
			IF (vCDO=CDOResourceFamily) THEN
				OPEN c_ResourceFamily FOR 
					    SELECT rd.ResourceId, rd.ResourceName, rf.ResourceFamilyName
                          FROM ResourceDef rd LEFT JOIN ResourceFamily rf ON rf.ResourceFamilyId = rd.ResourceFamilyId
                        WHERE rf.ResourceFamilyId = vResourceFamilyId;
										   
				LOOP 

					FETCH c_ResourceFamily INTO  vCurrResourceId, vCurrResourceName, vCurrResourceFamilyName;
					EXIT WHEN c_ResourceFamily%NOTFOUND;

                    IF vTimeSpanType=TimeSpanWeek  THEN -- TimeSpaWeek use the time span Custom case
					  --Search vFromDate and vToDate for Week.
                      GetTimeSpanWeek (pRunDate, vCurrResourceId, vFromDate, vToDate); 
					  --Initialize gTimeSpanCustomSetting Custom structure.
					  SetTimeSpanCustom(pRunDate, vFromDate, vToDate );
                    END IF;

					i := i+1;
					recOEE_RtCalc.ResourceGroup :=  vCurrResourceFamilyName;
					recOEE_RtCalc."Resource"    :=  vCurrResourceName;
					recOEE_RtCalc.Availability  :=  Availability ( vTimeSpanType, pRunDate, vCurrResourceId  ) * 100;
					recOEE_RtCalc.Performance   :=  Performance( vTimeSpanType, pRunDate, vCurrResourceId  ) * 100;
					recOEE_RtCalc.Quality       :=  Quality( vTimeSpanType, pRunDate, vCurrResourceId  ) * 100;
					recOEE_RtCalc.OEE           :=  (recOEE_RtCalc.Availability * recOEE_RtCalc.Performance * recOEE_RtCalc.Quality)/10000 ;
					tabOEE_RtCalc.extend;
					tabOEE_RtCalc(i):=recOEE_RtCalc;
					PIPE ROW(tabOEE_RtCalc(i));

					END LOOP;
				CLOSE c_ResourceFamily;

			ELSIF (vCDO=CDOResourceGroup) THEN
				NULL;
				assert( 'CDO ResourceGroup non implemented', TRUE,FALSE);
            ELSIF (vCDO=CDOResource OR vCDO = -1) THEN -- vCDO = -1 is implemeted to support behavior polimofe for ResourceName.

			    IF vCDO = CDOResource THEN
				   SELECT rd.ResourceName INTO vCurrResourceName FROM ResourceDef rd WHERE rd.ResourceId  =  vResourceFamilyId;
				   vCurrResourceId := vResourceFamilyId;
                ELSE --vCDO = -1
				   --This case vResourceFamilyId contain the resource name
				   SELECT rd.ResourceId   INTO vCurrResourceId   FROM ResourceDef rd WHERE rd.ResourceName = vResourceFamilyId;
                   vCurrResourceName := vResourceFamilyId;
                END IF;
				

				assert( 'vCurrResourceId: ' || vCurrResourceId || ' vCurrResourceName := ' || vCurrResourceName , TRUE);

                IF vTimeSpanType=TimeSpanWeek  THEN -- TimeSpaWeek use the time span Custom case
				   --Search vFromDate and vToDate for Week.
                   GetTimeSpanWeek (pRunDate, vCurrResourceId, vFromDate, vToDate); 
				   --Initialize gTimeSpanCustomSetting Custom structure.
				   SetTimeSpanCustom(pRunDate, vFromDate, vToDate );
                END IF;				
				recOEE_RtCalc.ResourceGroup :='';
				recOEE_RtCalc."Resource"    :=  vCurrResourceName;
				recOEE_RtCalc.Availability  :=  Availability ( vTimeSpanType, pRunDate, vCurrResourceId  ) * 100;
				recOEE_RtCalc.Performance   :=  Performance( vTimeSpanType, pRunDate, vCurrResourceId  )  * 100;
				recOEE_RtCalc.Quality       :=  Quality( vTimeSpanType, pRunDate, vCurrResourceId  )  * 100;
				recOEE_RtCalc.OEE           :=  (recOEE_RtCalc.Availability * recOEE_RtCalc.Performance * recOEE_RtCalc.Quality)/10000 ;
				tabOEE_RtCalc.extend;
				tabOEE_RtCalc(1):=recOEE_RtCalc;
				PIPE ROW(tabOEE_RtCalc(1));
				assert( 'vCurrResourceId: '  || vCurrResourceId  || ' pRunDate:' || pRunDate|| ' vFromDate:' || vFromDate || ' vToDate:' || vToDate , TRUE);
			ELSE 
				NULL;
			END IF;
		ELSE
			assert( 'The selected timespan may not be available in the future', TRUE);
			PIPE ROW (NULL);
		END IF;
		
    EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		DBMS_OUTPUT.PUT_LINE('OEE vResourceFamilyId:' || vResourceFamilyId || '  '  || vCDO ||  ' ' || ErrorMessage);
  		PIPE ROW (null);    	
    END OEE;

    FUNCTION Availability (pTimeSpanType IN NUMBER, 
                           pRunDate IN DATE, 
                           pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
		pAvailability NUMBER  :=0;
        vOperatingTime NUMBER := OperatingTime(pTimeSpanType,pRunDate,pResourceId);
		vPlannedProductionTime NUMBER := PlannedProductionTime(pTimeSpanType,pRunDate,pResourceId);
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';
		
	BEGIN
		pAvailability := vOperatingTime /vPlannedProductionTime;
		
		assert( '***' , TRUE);
		assert('Availability:' || pAvailability , TRUE);
		assert('  OperatingTime:' || vOperatingTime, TRUE);
		assert('  PlannedProductionTime:' || vPlannedProductionTime , TRUE);

		RETURN COALESCE(pAvailability,0);
	EXCEPTION
	
		WHEN ZERO_DIVIDE THEN
		RETURN 0; -- The Production time is not planned;
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		DBMS_OUTPUT.PUT_LINE('Availability ResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN 0;    
	END Availability;
    
	FUNCTION Quality( pTimeSpanType IN NUMBER, 
					  pRunDate IN DATE, 
					  pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
		pQuality NUMBER:=0;
		vCalendarShiftId VARCHAR2(16);
        vShiftStart DATE;
		vShiftEnd DATE;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';	
	BEGIN
		IF pTimeSpanType= TimeSpanShift AND NOT gTimeSpanCustomSetting.vSet THEN
		BEGIN 
			SELECT cs.CalendarShiftId INTO vCalendarShiftId
			FROM Factory f 
			  INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
			  INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
			  INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
			  LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE pRunDate >= cs.ShiftStart AND pRunDate < cs.ShiftEnd
			  AND r.ResourceId = pResourceId;

			SELECT CASE WHEN SUM(NVL(GOODQTY,0)) > 0 THEN SUM(NVL(GOODQTY,0))/SUM(NVL(TOTALQTY,0)) ELSE SUM(NVL(GOODQTY2,0))/SUM(NVL(TOTALQTY2,0)) END INTO pQuality 
			   FROM isOEEThruputDetails 
			 WHERE CalendarShiftId = vCalendarShiftId 
			   AND ResourceId = pResourceId;
		END;
		ELSIF pTimeSpanType = TimeSpanDay AND NOT gTimeSpanCustomSetting.vSet THEN
		BEGIN
		    --it is necessary to Get the ShiftStart date and to sicure CalendarDate for the WHERE clausule because these always coincide
            GetCalendarShiftBy (TimeSpanDay, pRunDate , pResourceId , vShiftStart,  vShiftEnd);
							 
			SELECT CASE WHEN SUM(NVL(GOODQTY,0)) > 0 THEN SUM(NVL(GOODQTY,0))/SUM(NVL(TOTALQTY,0)) ELSE SUM(NVL(GOODQTY2,0))/SUM(NVL(TOTALQTY2,0)) END INTO pQuality
			  FROM isOEERawDetails 
			 WHERE CalendarShiftId IN 
			   (SELECT CalendarShiftId FROM CalendarShift cs 
				  WHERE (cs.ShiftStart, cs.ShiftEnd, cs.MfgCalendarId) IN (SELECT MIN(cs.ShiftStart), MAX(cs.ShiftEnd), cs.MfgCalendarId
																			 FROM Factory f 
																			   INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
																			   INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
																			   INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
																			   LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
																		    WHERE TRUNC(vShiftStart) = cs.CalendarDate
																			   AND r.ResourceId  = pResourceId
																			GROUP BY  cs.ShiftStart, cs.ShiftEnd, cs.MfgCalendarId ) )
			AND ResourceId = pResourceId AND TxnType in (2180, 2880, 3390, 6660);  
		
		END;
		ELSIF pTimeSpanType = TimeSpanWeek AND NOT gTimeSpanCustomSetting.vSet THEN
		    assert('TimeSpanWeek for Quality has not been set to a custom time span', (pTimeSpanType = TimeSpanWeek));
		ELSIF gTimeSpanCustomSetting.vSet THEN 
		BEGIN
			--The isOEERawDetails table stores the throughput and quality data that has not been aggregated. 
			--TxnDate field specifica the transaction occurred.
		    SELECT CASE WHEN SUM(NVL(GOODQTY,0)) > 0 THEN SUM(NVL(GOODQTY,0))/SUM(NVL(TOTALQTY,0)) ELSE SUM(NVL(GOODQTY2,0))/SUM(NVL(TOTALQTY2,0)) END INTO pQuality  --For all transaction occurred in the TimeSpan 
		      FROM isOEERawDetails 
            WHERE ResourceId = pResourceId
              AND TxnType in (2180, 2880, 3390, 6660) and TxnDate BETWEEN gTimeSpanCustomSetting.vFrom AND gTimeSpanCustomSetting.vTo;
		END;	
		END IF; 
		
		RETURN COALESCE(pQuality,0);

	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		DBMS_OUTPUT.PUT_LINE('Quality pResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN 0;    
	END Quality;

	FUNCTION Performance( pTimeSpanType IN NUMBER, 
						  pRunDate IN DATE, 
						  pResourceId IN VARCHAR2 ) 
	RETURN NUMBER
	AS
		vPerformance NUMBER:=0;
		vIdealCycleTime NUMBER:=0;
		vOperatingTime NUMBER:=0;

		vCalendarShiftId VARCHAR2(16);
		vShiftStart DATE;
		vShiftEnd DATE;
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):='';	
	BEGIN

		IF pTimeSpanType= TimeSpanShift AND NOT gTimeSpanCustomSetting.vSet THEN
		BEGIN 
			SELECT cs.CalendarShiftId INTO vCalendarShiftId
			FROM Factory f 
				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
			WHERE pRunDate >= cs.ShiftStart AND pRunDate < cs.ShiftEnd
			  AND r.ResourceId = pResourceId;

			SELECT SUM(IdealCycleTime)/60/24 INTO vPerformance -- IdealCycleTime convert from minute in days
				FROM isOEEThruputDetails 
			 WHERE CalendarShiftId = vCalendarShiftId 
			   AND ResourceId = pResourceId;
		END;
		ELSIF pTimeSpanType = TimeSpanDay AND NOT gTimeSpanCustomSetting.vSet THEN
		BEGIN
		     --it is necessary to Get the ShiftStart date and to sicure CalendarDate for the WHERE clausule because these always coincide
            GetCalendarShiftBy (TimeSpanDay, pRunDate , pResourceId , vShiftStart,  vShiftEnd);
		   
		     --IdealCycleTime: Fastest time required to process the total quantity. This is calculated based on
             --the ideal cycle time for the product and resource multiplied by the total quantity.
			SELECT SUM(IdealCycleTime)/60/24 INTO vPerformance  -- IdealCycleTime convert from minute in days
			  FROM isOEERawDetails 
			 WHERE CalendarShiftId IN 
			   (SELECT CalendarShiftId FROM CalendarShift cs 
				  WHERE (cs.ShiftStart, cs.ShiftEnd, cs.MfgCalendarId) IN (SELECT MIN(cs.ShiftStart), MAX(cs.ShiftEnd), cs.MfgCalendarId
																			 FROM Factory f 
																				INNER JOIN MfgCalendar mc ON mc.MfgCalendarId = f.MfgCalendarId
																				INNER JOIN CalendarShift cs ON mc.MfgCalendarId = cs.MfgCalendarId
																				INNER JOIN ResourceDef    r ON r.FactoryId		= f.FactoryId
																				LEFT JOIN ResourceFamily rf ON  rf.ResourceFamilyId = r.ResourceFamilyId 
																			  WHERE TRUNC(vShiftStart) = cs.CalendarDate
																			    AND r.ResourceId  = pResourceId
																			  GROUP BY  cs.ShiftStart, cs.ShiftEnd, cs.MfgCalendarId ) )
			AND ResourceId = pResourceId AND TxnType in (2180, 2880, 3390, 6660);  
		END;
		ELSIF pTimeSpanType = TimeSpanWeek AND NOT gTimeSpanCustomSetting.vSet THEN
		  assert('TimeSpanWeek for Performance has not been set to a custom time span', (pTimeSpanType = TimeSpanWeek));
		ELSIF gTimeSpanCustomSetting.vSet THEN 
		BEGIN
			--The isOEERawDetails table stores the throughput and quality data that has not been aggregated. 
			--TxnDate field the specific transaction occurred.
		    SELECT SUM(IdealCycleTime)/60/24 INTO vPerformance  -- IdealCycleTime convert from minute in days
		      FROM isOEERawDetails 
            WHERE ResourceId = pResourceId
              AND TxnType in (2180, 2880, 3390, 6660) and TxnDate BETWEEN gTimeSpanCustomSetting.vFrom AND gTimeSpanCustomSetting.vTo;
		END;
		END IF; 

		vIdealCycleTime:= COALESCE(vPerformance,0);
		
		vOperatingTime := OperatingTime( pTimeSpanType, pRunDate, pResourceId);
		vPerformance   := vPerformance / vOperatingTime;
		
		assert( '***' , TRUE);
		assert( 'Performance:' || vPerformance, TRUE);
		assert( '  SUM(IdealCycleTime):' || vIdealCycleTime, TRUE);
		assert( '  OperatingTime:' || vOperatingTime, TRUE);
		
		RETURN COALESCE(vPerformance,0);
	EXCEPTION
		WHEN ZERO_DIVIDE THEN
		RETURN 0; -- The OperatingTime or the expressly Production time is not planned;

		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' || v_errm; 
		DBMS_OUTPUT.PUT_LINE('Performance ResourceId:' || pResourceId ||  ' ' || ErrorMessage);
		RETURN 0;    
	END Performance;
	
	FUNCTION GetOEEDowntimeDistribution(pResourceFamilyId   IN VARCHAR2, 
										pResourceId   IN VARCHAR2,
										pTimeSpanType IN NUMBER,
										pFromDate IN DATE DEFAULT NULL,
										pToDate   IN DATE DEFAULT NULL,
										pRunDate  IN DATE DEFAULT SYSDATE) 
	RETURN TAB_OEEDwntDistributionValues PIPELINED
	AS
		vCurrResourceName ResourceDef.ResourceName%TYPE := '';
		vCurrResourceId ResourceDef.ResourceId%TYPE := '';
		vCDO NUMBER;
		CDOResource  NUMBER := 0;
		vFrom DATE;
		vTo DATE;
		vRunDate DATE;

		vTmpOEEDwntDistributionCount NUMBER:=0;

	    recOEEDwntDistributionValues TYP_OEEDwntDistributionValues;
	    tabOEEDwntDistributionValues TAB_OEEDwntDistributionValues;
		
		Duration NUMBER :=0;
		LastDuration NUMBER :=0;
		
		CountChangeState NUMBER;
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):=''; 
		
	BEGIN
		
		tabOEEDwntDistributionValues := TAB_OEEDwntDistributionValues();
	
		vCDO := GetCDONameById(pResourceId);
		
		IF vCDO = CDOResource THEN
		   SELECT rd.ResourceName INTO vCurrResourceName FROM ResourceDef rd WHERE rd.ResourceId  =  pResourceId;
		   vCurrResourceId := pResourceId;
		ELSE --vCDO = -1
		   SELECT rd.ResourceId   INTO vCurrResourceId   FROM ResourceDef rd WHERE rd.ResourceName = pResourceId; --Only automation test
		   vCurrResourceName := pResourceId;
		END IF;
	
		BEGIN --Initialize from to dates from timespan setting
			IF NOT (pTimeSpanType = TimeSpanCustom) AND 
			   NOT (pTimeSpanType = TimeSpanWeek) THEN
				GetCalendarShiftBy (pTimeSpanType, pRunDate , vCurrResourceId, vFrom,  vTo);
				assert( 'GetCalendarShiftBy : pTimeSpanType, pRunDate, vFrom,  vTo: ' || pTimeSpanType || ' / ' || pRunDate || ' / ' ||  vFrom  || ' / ' || vTo ,  TRUE,FALSE);
			END IF;
			IF pTimeSpanType = TimeSpanWeek  THEN  
				--Search vFrom and vTo for Week.
				GetTimeSpanWeek (pRunDate, vCurrResourceId, vFrom, vTo); 
				assert( 'GetTimeSpanWeek : pRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
			END IF;
			IF  (pTimeSpanType = TimeSpanCustom AND pRunDate >= pFromDate) THEN 
			   vFrom:= pFromDate;
			   vTo  := pToDate;
			   vRunDate := pRunDate;
			ELSIF (pTimeSpanType = TimeSpanCustom AND pRunDate < pFromDate) THEN 
				vFrom	:= pRunDate;
				vTo		:= pRunDate;
			END IF;
			--Normalize vTo at pRunDate.
			--Normalize vTo and vFrom at pRunDate for span Reason.
			IF pRunDate < vTo THEN vTo := pRunDate; END IF;
			assert( 'vRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
		END;

		BEGIN
			SELECT COUNT(*) INTO CountChangeState FROM ResourceStatusHistory WHERE OldLastStatusChangeDate IS NOT NULL AND HistoryId  = pResourceId;
			
			IF CountChangeState  > 0 THEN
			BEGIN
				-- The logic is to Do sum of the intervals that intercepts as for as Overlapping of the 
				-- Records for resource in Down Status for all change status closed.
				
				-- QUERY for Central Status Chenge
				WITH RSH AS (SELECT SUBSTR(rscFROM.DESCRIPTION, 1,50) ResourceStatusCodeDesc, 
									--SUBSTR(rsrFrom.DESCRIPTION, 1,50) FromResourceStatusReasonDesc, 
									--SUBSTR(rsrTo.DESCRIPTION, 1,50) ToResourceStatusReasonDesc, 
									rscFrom.RESOURCESTATUSCODENAME FromResourceStatusCodeName,
									rscTo.RESOURCESTATUSCODENAME ToResourceStatusCodeName,
									rsh.OldLastStatusChangeDate olds,
									rsh.LastStatusChangeDate    lasts,
									rsrFROM.RESOURCESTATUSREASONNAME FromReasonName,
									rsrTO.RESOURCESTATUSREASONNAME ToReasonName,
									rscFROM.RESOURCESTATE, 
									rsh.HistoryId,
									(rsh.LastStatusChangeDate- rsh.OldLastStatusChangeDate) Duration,
									rsh.isOldOEELossCategory isOEELossCategory
								FROM ResourceStatusHistory rsh 
								LEFT JOIN ResourceStatusCode rscFrom ON rsh.OLDRESOURCESTATUSCODEID = rscFrom.RESOURCESTATUSCODEID
								JOIN ResourceStatusCode rscTo ON rsh.RESOURCESTATUSCODEID = rscTo.RESOURCESTATUSCODEID
								LEFT JOIN ResourceStatusReason rsrTo ON rsh.RESOURCESTATUSREASONCODEID = rsrTo.RESOURCESTATUSREASONID
								LEFT JOIN ResourceStatusReason rsrFrom ON rsh.OLDRESOURCESTATUSREASONCODEID = rsrFrom.RESOURCESTATUSREASONID 
							WHERE rsh.HISTORYID = pResourceId  
							ORDER BY rsh.LastStatusChangeDate ASC
				), CustomDate AS (
					SELECT vFrom, vTo FROM DUAL
					
				)SELECT vCurrResourceName , 
						rsh.FromResourceStatusCodeName CategoryName, 
						CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
						  WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
						  WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
						  WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
						ELSE 0 END *24*60*60 DURATION, 
						rsh.isOEELossCategory,
						'#FFAF60' CategoryCode
						BULK COLLECT INTO tabOEEDwntDistributionValues
					FROM RSH rsh JOIN CustomDate cd ON 
					  (( (cd.vFrom >= rsh.olds AND cd.vFrom < rsh.lasts ) OR (rsh.olds >= cd.vFrom AND  rsh.olds < cd.vTo ) )
					  OR NOT (rsh.olds >= cd.vTo OR rsh.lasts <= cd.vFrom ))
					  AND rsh.olds IS NOT NULL
					  AND rsh.lasts < (SELECT MAX(LASTSTATUSCHANGEDATE) FROM ResourceStatusHistory WHERE HISTORYID = pResourceId)
					  AND rsh.HistoryId = pResourceId;	 
					  
					  vTmpOEEDwntDistributionCount := tabOEEDwntDistributionValues.count;
					  assert( 'Central tabOEEDwntDistributionValues.count' || '  ' || to_char(tabOEEDwntDistributionValues.count),  TRUE,FALSE);
					  
			EXCEPTION
				WHEN NO_DATA_FOUND THEN
				vTmpOEEDwntDistributionCount :=0;
			END;
			BEGIN
				DECLARE 
					i NUMBER;
					vResourceStatusCodeName VARCHAR2(30);
					vDuration NUMBER;
					vIsOEELossCategory NUMBER;
					vCategoryCode VARCHAR2(7);
					
					CURSOR cLasStatusChange IS 
					WITH RSH AS (SELECT SUBSTR(rscFROM.DESCRIPTION, 1,50) ResourceStatusCodeDesc, 
										--SUBSTR(rsrFrom.DESCRIPTION, 1,50) FromResourceStatusReasonDesc, 
										--SUBSTR(rsrTo.DESCRIPTION, 1,50) ToResourceStatusReasonDesc, 
										rscFrom.RESOURCESTATUSCODENAME FromResourceStatusCodeName,
										rscTo.RESOURCESTATUSCODENAME ToResourceStatusCodeName,
										rsh.OldLastStatusChangeDate olds,
										rsh.LastStatusChangeDate    lasts,
										rsrFROM.RESOURCESTATUSREASONNAME FromReasonName,
										rsrTO.RESOURCESTATUSREASONNAME ToReasonName,
										rscFROM.RESOURCESTATE, 
										rsh.HistoryId,
										(rsh.LastStatusChangeDate- rsh.OldLastStatusChangeDate) Duration,
										rsh.isOldOEELossCategory isOEELossCategory,
										rsh.isOEELossCategory isNextOEELossCategory 
									FROM ResourceStatusHistory rsh 
									LEFT JOIN ResourceStatusCode rscFrom ON rsh.OLDRESOURCESTATUSCODEID = rscFrom.RESOURCESTATUSCODEID
									JOIN ResourceStatusCode rscTo ON rsh.RESOURCESTATUSCODEID = rscTo.RESOURCESTATUSCODEID
									LEFT JOIN ResourceStatusReason rsrTo ON rsh.RESOURCESTATUSREASONCODEID = rsrTo.RESOURCESTATUSREASONID
									LEFT JOIN ResourceStatusReason rsrFrom ON rsh.OLDRESOURCESTATUSREASONCODEID = rsrFrom.RESOURCESTATUSREASONID 
								WHERE rsh.HISTORYID = pResourceId   
						), CustomDate AS (
							SELECT vFrom, vTo FROM DUAL
						) SELECT f.CategoryName, f.DURATION, f.isOEELossCategory, f.CategoryCode
							FROM ( SELECT rsh.FromResourceStatusCodeName CategoryName, 
										 CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											  WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											  WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											  WHEN  cd.vFrom >= rsh.olds AND cd.vFrom <= rsh.lasts AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
											ELSE 0 END *24*60*60 DURATION,
										 rsh.isOEELossCategory,
										 '#FFFF66' CategoryCode,
										 rsh.Olds lasts
									  FROM RSH rsh JOIN CustomDate cd ON cd.vTo > rsh.olds
									   AND rsh.lasts = (SELECT MAX(LASTSTATUSCHANGEDATE) FROM ResourceStatusHistory WHERE HISTORYID = pResourceId)
									   AND rsh.HistoryId = pResourceId 
								UNION
								 SELECT rsh.ToResourceStatusCodeName CategoryName, 
										 CASE WHEN  cd.vFrom >= rsh.olds AND cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - cd.vFrom  --> per ToResourceStatusCodeName
											  WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts 						   --> per ToResourceStatusCodeName
											  WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts 						   --> per ToResourceStatusCodeName
											ELSE 0 END *24*60*60 DURATION,
										 rsh.isNextOEELossCategory isOEELossCategory,
										 '#FFFDD0' CategoryCode,
										 rsh.lasts
									  FROM RSH rsh JOIN CustomDate cd ON cd.vTo > rsh.lasts
									   AND rsh.lasts = (SELECT MAX(LASTSTATUSCHANGEDATE) FROM ResourceStatusHistory WHERE HISTORYID = pResourceId)
									   AND rsh.HistoryId = pResourceId ) f ORDER BY f.lasts ASC ;
				BEGIN
					--Query for the last Status Change.
					i := vTmpOEEDwntDistributionCount;
					OPEN cLasStatusChange;
					LOOP
						i := i+1;
						FETCH cLasStatusChange INTO vResourceStatusCodeName, vDuration , vIsOEELossCategory, vCategoryCode
;
						EXIT WHEN cLasStatusChange%NOTFOUND;
						recOEEDwntDistributionValues.ResourceName := vCurrResourceName;
						recOEEDwntDistributionValues.CategoryName := vResourceStatusCodeName;
						recOEEDwntDistributionValues.Duration := vDuration;
						recOEEDwntDistributionValues.isOEELossCategory := vIsOEELossCategory;
						recOEEDwntDistributionValues.CategoryCode := vCategoryCode;
						tabOEEDwntDistributionValues.extend;
						tabOEEDwntDistributionValues(i):=recOEEDwntDistributionValues;
					END LOOP;
					CLOSE cLasStatusChange;
						
					assert( 'Last tabOEEDwntDistributionValues.count' || '  ' || to_char(tabOEEDwntDistributionValues.count),  TRUE,FALSE);
				EXCEPTION
					WHEN NO_DATA_FOUND THEN 
					PIPE ROW (NULL); 	
				END;	
			END;
			ELSE
			BEGIN
				--One records history query
				WITH RSH AS (SELECT SUBSTR(rscFROM.DESCRIPTION, 1,50) ResourceStatusCodeDesc, 
									--SUBSTR(rsrFrom.DESCRIPTION, 1,50) FromResourceStatusReasonDesc, 
									--SUBSTR(rsrTo.DESCRIPTION, 1,50) ToResourceStatusReasonDesc, 
									rscFrom.RESOURCESTATUSCODENAME FromResourceStatusCodeName,
									rscTo.RESOURCESTATUSCODENAME ToResourceStatusCodeName,
									rsh.OldLastStatusChangeDate olds,
									rsh.LastStatusChangeDate    lasts,
									rsrFROM.RESOURCESTATUSREASONNAME FromReasonName,
									rsrTO.RESOURCESTATUSREASONNAME ToReasonName,
									rscFROM.RESOURCESTATE, 
									rsh.HistoryId,
									(rsh.LastStatusChangeDate- rsh.OldLastStatusChangeDate) Duration,
									rsh.isOldOEELossCategory isOEELossCategory
								FROM ResourceStatusHistory rsh 
								LEFT JOIN ResourceStatusCode rscFrom ON rsh.OLDRESOURCESTATUSCODEID = rscFrom.RESOURCESTATUSCODEID
								JOIN ResourceStatusCode rscTo ON rsh.RESOURCESTATUSCODEID = rscTo.RESOURCESTATUSCODEID
								LEFT JOIN ResourceStatusReason rsrTo ON rsh.RESOURCESTATUSREASONCODEID = rsrTo.RESOURCESTATUSREASONID
								LEFT JOIN ResourceStatusReason rsrFrom ON rsh.OLDRESOURCESTATUSREASONCODEID = rsrFrom.RESOURCESTATUSREASONID 
							WHERE rsh.HISTORYID = pResourceId   
				)SELECT vCurrResourceName , 
						rsh.ToResourceStatusCodeName CategoryName, 
						CASE  --Resource Down
					        WHEN vFrom <=  lasts AND vTo <=  lasts THEN 0
					        WHEN vFrom <=  lasts AND vTo >  lasts THEN vTo - lasts
					        WHEN vFrom >=  lasts AND vTo >  lasts THEN vTo - vFrom 
					        WHEN vFrom >=  lasts AND vTo <=  lasts THEN 0 --Input Error 
					     END *24*60*60 DURATION,
						 rsh.isOEELossCategory,
						 '#2FF9D0' CategoryCode
					BULK COLLECT INTO tabOEEDwntDistributionValues
				  FROM RSH rsh
				  WHERE rsh.olds IS NULL
				   AND rsh.HISTORYID = pResourceId; 
				   
				assert( 'One tabOEEDwntDistributionValues.count' || '  ' || to_char(tabOEEDwntDistributionValues.count),  TRUE,FALSE);
				
 			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				PIPE ROW (NULL);
			END;
			END IF;
		END;
		
		FOR i IN tabOEEDwntDistributionValues.FIRST..tabOEEDwntDistributionValues.LAST 
		LOOP
			PIPE ROW(tabOEEDwntDistributionValues(i));
		END LOOP;
		
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		PIPE ROW (NULL);    
	END GetOEEDowntimeDistribution;
	
	FUNCTION GetOEETopScrapReason ( pResourceFamilyId   IN VARCHAR2,
									pResourceId   IN VARCHAR2,
								    pTimeSpanType IN NUMBER,
									pFromDate IN DATE DEFAULT NULL,
									pToDate   IN DATE DEFAULT NULL,
								    pRunDate  IN DATE DEFAULT SYSDATE) 
	RETURN TAB_OEEScrapReasonValues PIPELINED
	AS
		vCurrResourceName ResourceDef.ResourceName%TYPE := '';
		vCurrResourceId ResourceDef.ResourceId%TYPE := '';
		vCDO NUMBER;
		CDOResource  NUMBER := 0;
		vFrom DATE;
		vTo DATE;
		vRunDate DATE;
		recOEEScrapReasonValues TYP_OEEScrapReasonValues;
		tabOEEScrapReasonValues TAB_OEEScrapReasonValues;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):=''; 
			
	BEGIN
		
		tabOEEScrapReasonValues := TAB_OEEScrapReasonValues();

		vCDO := GetCDONameById(pResourceId);
		
		IF vCDO = CDOResource THEN
		   SELECT rd.ResourceName INTO vCurrResourceName FROM ResourceDef rd WHERE rd.ResourceId  =  pResourceId;
		   vCurrResourceId := pResourceId;
		ELSE --vCDO = -1
		   SELECT rd.ResourceId   INTO vCurrResourceId   FROM ResourceDef rd WHERE rd.ResourceName = pResourceId; --Only automation test
		   vCurrResourceName := pResourceId;
		END IF;

		BEGIN --Initialize from to dates from timespan setting
			IF NOT (pTimeSpanType = TimeSpanCustom) AND 
			   NOT (pTimeSpanType = TimeSpanWeek) THEN
				GetCalendarShiftBy (pTimeSpanType, pRunDate , vCurrResourceId, vFrom,  vTo);
				assert( 'GetCalendarShiftBy : pTimeSpanType, pRunDate, vFrom,  vTo: ' || pTimeSpanType || ' / ' || pRunDate || ' / ' ||  vFrom  || ' / ' || vTo ,  TRUE,FALSE);
			END IF;
			IF pTimeSpanType = TimeSpanWeek  THEN  
				--Search vFrom and vTo for Week.
				GetTimeSpanWeek (pRunDate, vCurrResourceId, vFrom, vTo); 
				assert( 'GetTimeSpanWeek : pRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
			END IF;
			IF  (pTimeSpanType = TimeSpanCustom AND pRunDate >= pFromDate) THEN 
			   vFrom:= pFromDate;
			   vTo  := pToDate;
			   vRunDate := pRunDate;
			ELSIF (pTimeSpanType = TimeSpanCustom AND pRunDate < pFromDate) THEN 
				vFrom	:= pRunDate;
				vTo		:= pRunDate;
			END IF;
			--Normalize vTo at pRunDate.
			--Normalize vTo and vFrom at pRunDate for span Reason.
			IF pRunDate < vTo THEN vTo := pRunDate; END IF;
			assert( 'vRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
		END;
		BEGIN
			WITH ALLQTYREWORK AS (
				SELECT hml.RESOURCEID, 
					   mh.TXNID, 
					   hml.TxnDate, 
					   ScrapRework LossType , 
					   rwr.REWORKREASONNAME REASON,  
					   mh.QTY
				FROM  HistoryMainline hml
				JOIN  MOVEHISTORY mh ON mh.HISTORYMAINLINEID = hml.HISTORYMAINLINEID AND mh.TXNID = hml.TXNID -- and INREWORK = 1
				JOIN  REWORKREASON rwr on  rwr.REWORKREASONID = mh.REWORKREASONID 
				AND HML.RESOURCEID IS NOT NULL
				AND hml.TxnDate BETWEEN vFrom AND vTo
				AND hml.RESOURCEID= vCurrResourceId
			
			), ALLQTYLOSS AS (
				SELECT  HML.RESOURCEID, 
						hml.TXNID,
						hml.TxnDate,
						CASE qhd.ChangeQtyType  WHEN 2 then ScrapLoss -- 'LossReason'
												WHEN 4 then ScrapAdjust
						END LossType,
						lr.LossReasonName Reason,
						qhd.Qty -- positivo a indicare le quantità loss
				FROM QtyHistory qh
				 LEFT OUTER JOIN  QtyHistoryDetails qhd on qhd.QtyHistoryId = qh.QtyHistoryId
				 LEFT OUTER JOIN  LossReason lr on lr.LossReasonId = qhd.ReasonCodeId
				 --left outer join QtyAdjustReason qar on qar.QtyAdjustReasonId = qhd.ReasonCodeId
				 LEFT OUTER JOIN HistoryMainline hml ON hml.HistoryMainlineId = qh.HistoryMainlineId
				 WHERE ChangeQtyType =2 
				 AND HML.RESOURCEID IS NOT NULL
				 AND hml.TxnDate BETWEEN vFrom AND vTo
				 AND hml.RESOURCEID= vCurrResourceId
				 
			), ALLQTYADJUST AS (
				SELECT  HML.RESOURCEID, 
						hml.TXNID,
						hml.TxnDate,
						CASE qhd.ChangeQtyType  WHEN 2 then ScrapLoss -- 'LossReason'
												WHEN 4 then ScrapAdjust
						END LossType,
						--lr.LossReasonName,
						qar.QTYADJUSTREASONNAME Reason,
						qhd.Qty  -- solo qty negativi.. I positivo a indicano le quantità agiustate positive
						--qhd.Qty -- positivo a indicare le quantità loss
				FROM QtyHistory qh
				 LEFT OUTER JOIN  QtyHistoryDetails qhd on qhd.QtyHistoryId = qh.QtyHistoryId
				 --left outer join LossReason lr on lr.LossReasonId = qhd.ReasonCodeId
				 LEFT OUTER JOIN  QtyAdjustReason qar on qar.QtyAdjustReasonId = qhd.ReasonCodeId
				 LEFT OUTER JOIN HistoryMainline hml ON hml.HistoryMainlineId = qh.HistoryMainlineId
				 WHERE ChangeQtyType =4 
				 AND HML.RESOURCEID IS NOT NULL
				 AND hml.TxnDate BETWEEN vFrom AND vTo
				 AND hml.RESOURCEID= vCurrResourceId
	
			), ALLSCRAP AS (
			  SELECT * FROM ALLQTYLOSS 
			  UNION  
			  SELECT * FROM ALLQTYADJUST
			), ALLSCRAPREASON AS (
				SELECT SC.LOSSTYPE AS SCRAP, SC.REASON , -1 * SUM(SC.QTY) QTY FROM ALLSCRAP SC WHERE SC.LOSSTYPE =ScrapAdjust AND SC.QTY < 0 GROUP BY SC.REASON , SC.LOSSTYPE
				UNION
				SELECT SC.LOSSTYPE AS SCRAP, SC.REASON , SUM(SC.QTY) QTY FROM ALLSCRAP SC WHERE SC.LOSSTYPE = ScrapLoss  GROUP BY SC.REASON, SC.LOSSTYPE
				
			),ALLREWORKREASON AS (
				SELECT SC.LOSSTYPE, SC.REASON , SUM(SC.QTY) QTY FROM ALLQTYREWORK SC GROUP BY SC.REASON , SC.LOSSTYPE
			
			), SCRAPREASON AS(
				SELECT * FROM (
					SELECT * FROM ALLSCRAPREASON
					UNION
					SELECT * FROM ALLREWORKREASON)
				ORDER BY QTY DESC
			)SELECT vCurrResourceName , s.SCRAP, s.REASON, s.QTY, s.ReasonRank
			   BULK COLLECT INTO tabOEEScrapReasonValues
				FROM (SELECT SCRAP, 
							 REASON, 
							 QTY,
							 RANK() OVER (ORDER BY QTY DESC) ReasonRank  
						FROM SCRAPREASON) s;
			
		EXCEPTION
		WHEN NO_DATA_FOUND THEN 
		tabOEEScrapReasonValues.extend;
		recOEEScrapReasonValues.QTY:=0;
		tabOEEScrapReasonValues(1) := recOEEScrapReasonValues ; 
		assert( 'GetOEETopScrapReason: null', TRUE,FALSE);
		END;

		FOR i IN tabOEEScrapReasonValues.FIRST..tabOEEScrapReasonValues.LAST 
		LOOP
			PIPE ROW(tabOEEScrapReasonValues(i));
		END LOOP;
	
	EXCEPTION		
	WHEN OTHERS THEN
	v_code := SQLCODE;
	v_errm := SUBSTR(SQLERRM, 1 , 64);
	ErrorMessage := v_code || '-' ||v_errm; 
	PIPE ROW (NULL); 	
	END GetOEETopScrapReason;
	
    FUNCTION GetOEETopDowntimeReasons ( pResourceFamilyId   IN VARCHAR2,
										pResourceId   IN VARCHAR2,
										pTimeSpanType IN NUMBER,
										pFromDate IN DATE DEFAULT NULL, 
										pToDate   IN DATE DEFAULT NULL,
										pRunDate  IN DATE DEFAULT SYSDATE) 
	RETURN TAB_OEEResourceDowntimeValues PIPELINED
	AS

		--FOR TEST: 
		-- The sum values of the ReasonDuration for all ranks have equal values at the availabilty loss for any timespan.
		-- 
		vFrom DATE;
		vTo   DATE;
		CountChangeState NUMBER:=0;
		vRunDate DATE;

		recOEEResourceDowntimeValues TYP_OEEResourceDowntimeValues;
		tabOEEResourceDowntimeValues TAB_OEEResourceDowntimeValues;
		
		vResourceName ResourceDef.ResourceName%TYPE;
		vCurrResourceId ResourceDef.ResourceId%TYPE := '';
		
		vCDO NUMBER;
		CDOResource  NUMBER := 0;
		
		v_code NUMBER;
		v_errm VARCHAR2(64); 
		ErrorMessage VARCHAR2(4000):=''; 
	
	BEGIN
		tabOEEResourceDowntimeValues := TAB_OEEResourceDowntimeValues();

		vCDO := GetCDONameById(pResourceId);
		
		IF vCDO = CDOResource THEN
		   SELECT rd.ResourceName INTO vResourceName FROM ResourceDef rd WHERE rd.ResourceId  =  pResourceId;
		   vCurrResourceId := pResourceId;
		ELSE --vCDO = -1
		   SELECT rd.ResourceId   INTO vCurrResourceId   FROM ResourceDef rd WHERE rd.ResourceName = pResourceId; --Only automation test
		   vResourceName := pResourceId;
		END IF;

		
		--  For now not used Custom time span directly because the function is called out of the context of the 
		--	OEE Calculation. That is an autonomous API!
		--	Unlike the functions that are called in the OEE Calculation context does not need to set CustomTimeSpan 
		--	for use of pakage variables gTimeSpanCustomSetting. In fact the next three calls are not necessary and are comments. */
		--vFrom:= gTimeSpanCustomSetting.vFrom;
		--vTo  := gTimeSpanCustomSetting.vTo;
  
		BEGIN --Initialize from to dates from timespan setting
			IF NOT (pTimeSpanType = TimeSpanCustom) AND 
			   NOT (pTimeSpanType = TimeSpanWeek) THEN
				GetCalendarShiftBy (pTimeSpanType, pRunDate , vCurrResourceId, vFrom,  vTo);
				assert( 'GetCalendarShiftBy : pTimeSpanType, pRunDate, vFrom,  vTo: ' || pTimeSpanType || ' / ' || pRunDate || ' / ' ||  vFrom  || ' / ' || vTo ,  TRUE,FALSE);
			END IF;
			IF pTimeSpanType = TimeSpanWeek  THEN -- In This function TimeSpaWeek not use the time span Custom case 
				--Search vFrom and vTo for Week.
				GetTimeSpanWeek (pRunDate, vCurrResourceId, vFrom, vTo); 
				assert( 'GetTimeSpanWeek : pRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
			END IF;
			IF  (pTimeSpanType = TimeSpanCustom AND pRunDate >= pFromDate) THEN 
			   vFrom:= pFromDate;
			   vTo  := pToDate;
			   vRunDate := pRunDate;
			ELSIF (pTimeSpanType = TimeSpanCustom AND pRunDate < pFromDate) THEN 
				vFrom	:= pRunDate;
				vTo		:= pRunDate;
			END IF;
			--Normalize vTo at pRunDate.
			--Normalize vTo and vFrom at pRunDate for span Reason.
			IF pRunDate < vTo THEN vTo := pRunDate; END IF;
			assert( 'vRunDate, vFrom,  vTo: ' || pRunDate || '  ' ||  vFrom  || '  ' || vTo ,  TRUE,FALSE);
		END;
		
		BEGIN
			SELECT COUNT(*) INTO CountChangeState FROM ResourceStatusHistory WHERE OldLastStatusChangeDate IS NOT NULL AND HistoryId  = vCurrResourceId;
			
			IF CountChangeState  > 0 THEN
			BEGIN
				-- The logic is to Do sum of the intervals that are intercepts as for as Overlapping of the 
				-- records for resource in down status for all changed.
				
				-- Querys for intermediate and last Status Changed.
								
				WITH PREVHSR AS (
					SELECT  LEVEL ParentLevel , 
							rsr.RESOURCESTATUSREASONNAME ReasonName, 
							rsc.RESOURCESTATUSCODENAME,
							rsh.ISOLDOEELOSSCATEGORY, 
							rsh.ISOEELOSSCATEGORY,
							rsh.OLDAVAILABILITY, 
							rsh.AVAILABILITY, 
							rsh.OLDLASTSTATUSCHANGEDATE olds,
							rsh.LASTSTATUSCHANGEDATE    lasts,
							rsh.HISTORYID 
						FROM ResourceStatusHistory rsh 
						JOIN ResourceStatusCode rsc ON rsh.RESOURCESTATUSCODEID = rsc.RESOURCESTATUSCODEID
						LEFT JOIN ResourceStatusReason rsr ON rsh.RESOURCESTATUSREASONCODEID = rsr.RESOURCESTATUSREASONID 
						WHERE rsh.HISTORYID = vCurrResourceId 
					CONNECT BY PRIOR  rsh.LASTSTATUSCHANGEDATE =rsh.OLDLASTSTATUSCHANGEDATE
					START WITH rsh.OLDLASTSTATUSCHANGEDATE IS NULL
					
				), RSH AS ( 
					SELECT  hsr1.ParentLevel, 
							hsr1.RESOURCESTATUSCODENAME, 
							hsr1.ISOLDOEELOSSCATEGORY , 
							hsr1.ISOEELOSSCATEGORY , 
							hsr1.OLDAVAILABILITY , 
							hsr1.AVAILABILITY , 
							hsr1.olds , 
							hsr1.lasts, 
							hsr1.HISTORYID ,
							hsr2.REASONNAME AS PARENTREASONNAME,
							hsr1.REASONNAME AS REASONNAME
					  FROM PREVHSR hsr1 INNER JOIN PREVHSR hsr2
						ON hsr1.ParentLevel - 1 = hsr2.ParentLevel
				), CustomDate AS (
					SELECT  vFrom,  vTo FROM DUAL
				
				), GrpDwtReasonCentral AS (
					  SELECT rsh.ParentReasonName, 
						 CASE WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory IS NULL --Up to Up
							  THEN 0
							  WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN 0
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN 0 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN 0  
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN 0 
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1  
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
											ELSE 0 END)
							  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory IS NULL   
							  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
											WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
											WHEN  cd.vFrom >= rsh.olds AND cd.vTo >= rsh.lasts THEN rsh.lasts - cd.vFrom  
											ELSE 0 END)
							  ELSE 0              
						   END AS ReasonDuration   
					FROM RSH rsh JOIN CustomDate cd ON 
					  (( (cd.vFrom >= rsh.olds AND cd.vFrom < rsh.lasts ) OR (rsh.olds >= cd.vFrom AND  rsh.olds < cd.vTo ) )
					  OR NOT (rsh.olds >= cd.vTo OR rsh.lasts <= cd.vFrom ))
					  AND rsh.olds IS NOT NULL
					  AND rsh.lasts < (SELECT MAX(lasts) FROM RSH WHERE HISTORYID = vCurrResourceId)
					  AND rsh.HistoryId = vCurrResourceId 
					GROUP BY rsh.ParentReasonName, rsh.olds, rsh.lasts, rsh.isOldOEELossCategory, rsh.isOEELossCategory, rsh.HistoryId, rsh.AVAILABILITY, rsh.OLDAVAILABILITY
							
				), GrpDwtReasonPreLast AS ( 
						--This query processes and generates DX Part Union Sx Part for the last record interval and sets FromReasoname to ToReasonName (ParentReasonName name would be FromReasonName)					  
						SELECT CASE WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory = 1 
								  THEN CASE WHEN cd.vTo > rsh.lasts THEN  rsh.ReasonName END
											WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1   
								  THEN  CASE WHEN cd.vFrom >  rsh.lasts AND cd.vTo > rsh.lasts THEN rsh.ReasonName  
											 WHEN cd.vFrom <=  rsh.lasts AND cd.vTo > rsh.lasts THEN rsh.ReasonName END    
							   END AS ReasonName,
							 olds, lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY
						  FROM RSH rsh JOIN CustomDate cd ON cd.vTo > rsh.olds 
						   AND rsh.lasts = (SELECT MAX(lasts) FROM RSH WHERE HISTORYID = vCurrResourceId)
						   AND rsh.HistoryId = vCurrResourceId 
					  UNION 
						SELECT CASE WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1  
									THEN CASE WHEN  cd.vTo < rsh.lasts THEN ':Parent' || rsh.ParentReasonName --
											  WHEN  cd.vFrom < rsh.lasts AND cd.vTo <= rsh.lasts THEN ':Parent' || rsh.ParentReasonName 
											  WHEN  cd.vFrom < rsh.lasts AND cd.vTo > rsh.lasts THEN ':Parent' || rsh.ParentReasonName 
											  WHEN  cd.vFrom >  rsh.lasts AND cd.vTo > rsh.lasts THEN rsh.ReasonName    
											  WHEN  cd.vFrom <=  rsh.lasts AND cd.vTo > rsh.lasts THEN rsh.ReasonName  END   
									WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory IS NULL   
									THEN CASE WHEN  cd.vFrom < rsh.olds AND cd.vTo <= rsh.lasts THEN ':Parent' ||  rsh.ParentReasonName  
											  WHEN  cd.vFrom <= rsh.olds AND cd.vTo >= rsh.lasts THEN ':Parent' ||  rsh.ParentReasonName 
											  WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN ':Parent' ||  rsh.ParentReasonName 
											  WHEN  cd.vFrom <=  rsh.lasts AND cd.vTo >= rsh.lasts THEN ':Parent' ||  rsh.ParentReasonName   											  
											  END 
							   END AS ReasonName,
						 olds, lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY
					  FROM RSH rsh JOIN CustomDate cd ON cd.vTo > rsh.olds 
					   AND rsh.lasts = (SELECT MAX(lasts) FROM RSH WHERE HISTORYID = vCurrResourceId)
					   AND rsh.HistoryId = vCurrResourceId 
					   
				),GrpDwtReasonLast AS(
						SELECT replace(rsh.ReasonName,':Parent' ,  '') ReasonName,
							 CASE WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory IS NULL --Up to Up
								  THEN 0
								  WHEN rsh.isOldOEELossCategory IS NULL AND rsh.isOEELossCategory = 1  
								  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts
												WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN 0 
												WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN 0  
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom <= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - rsh.lasts 
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - cd.vFrom
												ELSE 0 END)
								  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory = 1  
								  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds
												WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo - cd.vFrom   
												WHEN  cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN cd.vTo - cd.vFrom 
												--WHEN  cd.vFrom < rsh.lasts AND cd.vTo > rsh.lasts
												WHEN  cd.vFrom < rsh.olds AND cd.vTo > rsh.lasts AND substr(rsh.ReasonName,1,6)= ':Parent' THEN rsh.lasts - rsh.olds 
												WHEN  cd.vFrom < rsh.olds AND cd.vTo > rsh.lasts AND substr(rsh.ReasonName,1,6)<> ':Parent' THEN cd.vTo- rsh.lasts 
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom < rsh.lasts AND cd.vTo >= rsh.lasts  AND substr(rsh.ReasonName,1,6)= ':Parent' THEN rsh.lasts - cd.vFrom  
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom < rsh.lasts AND cd.vTo >= rsh.lasts  AND substr(rsh.ReasonName,1,6)<> ':Parent' THEN cd.vTo -rsh.lasts 
												ELSE 0 END)
								  WHEN rsh.isOldOEELossCategory = 1 AND rsh.isOEELossCategory IS NULL   
								  THEN SUM(CASE WHEN  cd.vFrom < rsh.olds AND  cd.vTo >= rsh.lasts THEN rsh.lasts - rsh.olds
												WHEN  cd.vFrom < rsh.olds AND  cd.vTo <= rsh.lasts THEN cd.vTo - rsh.olds 
												WHEN  cd.vFrom >= rsh.olds AND cd.vTo <= rsh.lasts THEN cd.vTo -  cd.vFrom   
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom <= rsh.lasts AND cd.vTo >= rsh.lasts THEN rsh.lasts- cd.vFrom
												WHEN  cd.vFrom >= rsh.olds AND cd.vFrom >= rsh.lasts AND cd.vTo >= rsh.lasts THEN 0
												ELSE 0 END)
								  ELSE 0              
							   END AS ReasonDuration  
						  FROM GrpDwtReasonPreLast rsh JOIN CustomDate cd ON cd.vTo > rsh.olds AND ReasonName IS NOT NULL         
						GROUP BY rsh.ReasonName, olds, lasts, isOldOEELossCategory, isOEELossCategory, HistoryId, AVAILABILITY, OLDAVAILABILITY
			
				),UnionGrps AS (
					SELECT ParentReasonName AS ReasonName, SUM(ReasonDuration) ReasonDuration 
						FROM GrpDwtReasonCentral 
					  GROUP BY ParentReasonName
					UNION 
					SELECT ReasonName , SUM(ReasonDuration) ReasonDuration 
						FROM GrpDwtReasonLast 
					  GROUP BY ReasonName
					  
				), UnionReasonNameReasonDuration AS (
					SELECT ReasonName, SUM(ReasonDuration) ReasonDuration 
						FROM UnionGrps 
					  GROUP BY ReasonName
				)  
				   SELECT vResourceName  ResourceName, u.ReasonName, u.ReasonDuration, u.ReasonRank
					 BULK COLLECT INTO tabOEEResourceDowntimeValues
					 FROM (SELECT ReasonName, 
								  ReasonDuration*24*60*60 ReasonDuration, 
								  RANK() OVER (ORDER BY ReasonDuration DESC) ReasonRank  
							 FROM UnionReasonNameReasonDuration) u;
					
			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				tabOEEResourceDowntimeValues.extend;
				recOEEResourceDowntimeValues.ReasonDuration:=0;
				tabOEEResourceDowntimeValues(1) := recOEEResourceDowntimeValues ; -- Resource not initilized (assign default values)) ! -- The Resource has not status down . 
				assert( 'GetOEETopDowntimeReasons: null', TRUE,FALSE);
			END;		 
			ELSE
			BEGIN
				--Query for One records in ResourceStatusHistory
				WITH RSH AS (
					SELECT  rsh.RESOURCESTATUSHISTORYID , 
					        NVL(rsr.RESOURCESTATUSREASONNAME, 'UNASSIGNED') ReasonName, 
							rsc.RESOURCESTATUSCODENAME,
							rsh.isOldOEELossCategory, 
							rsh.isOEELossCategory,
							rsh.OLDAVAILABILITY, 
							rsh.AVAILABILITY, 
							rsh.OldLastStatusChangeDate olds,
							rsh.LastStatusChangeDate    lasts,
							rsc.RESOURCESTATUSCODEID, rsc.RESOURCESTATUSREASONSID, 
							SUBSTR(rsc.DESCRIPTION, 1,50) ResourceStatusCodeDesc, 
							SUBSTR(rsr.DESCRIPTION, 1,50) ResourceStatusReasonDesc, 
							rsc.RESOURCESTATE, rsh.HistoryId 
						FROM ResourceStatusHistory rsh 
						JOIN ResourceStatusCode rsc ON rsh.RESOURCESTATUSCODEID = rsc.RESOURCESTATUSCODEID
						LEFT JOIN ResourceStatusReason rsr ON rsh.RESOURCESTATUSREASONCODEID = rsr.RESOURCESTATUSREASONID  
					WHERE rsh.HISTORYID = vCurrResourceId 

				)SELECT vResourceName, ReasonName,
						  CASE WHEN ISOEELOSSCATEGORY IS NULL THEN 0  --Resource Up
							   WHEN ISOEELOSSCATEGORY = 1 			  --Resource Down 
							   THEN CASE  
										  WHEN vFrom < lasts AND vTo < lasts
										  THEN 0
										  WHEN vFrom < lasts AND vTo > lasts
										  THEN vTo -  lasts
										  WHEN vFrom > lasts AND vTo > lasts
										  THEN vTo - vFrom 
										  WHEN vFrom > lasts AND vTo < lasts --Input Error 
										  THEN 0
									 END 
						  END *24*60*60 ReasonDuration ,
						  1   ReasonRank
					BULK COLLECT INTO tabOEEResourceDowntimeValues
					FROM RSH
				 WHERE olds IS NULL
				   AND HISTORYID = vCurrResourceId; 
         
			EXCEPTION
				WHEN NO_DATA_FOUND THEN 
				tabOEEResourceDowntimeValues.extend;
				recOEEResourceDowntimeValues.ReasonDuration:=0;
				tabOEEResourceDowntimeValues(1) := recOEEResourceDowntimeValues ; -- Resource not initilized (assign default values)) ! -- The Resource has not status down . 
				assert( 'GetOEETopDowntimeReasons: null', TRUE,FALSE);
			END;
			END IF;
        
		END;
		
		FOR i IN tabOEEResourceDowntimeValues.FIRST..tabOEEResourceDowntimeValues.LAST 
		LOOP
			PIPE ROW(tabOEEResourceDowntimeValues(i));
		END LOOP;
		
	EXCEPTION
		WHEN OTHERS THEN
		v_code := SQLCODE;
		v_errm := SUBSTR(SQLERRM, 1 , 64);
		ErrorMessage := v_code || '-' ||v_errm; 
		assert( 'PIPE ROW (NULL): null', TRUE,FALSE);
		PIPE ROW (NULL); 
	END GetOEETopDowntimeReasons;
	
END isOEECalculation;
--#delimiter