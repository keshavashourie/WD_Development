/*
-- © 2020 Siemens Product Lifecycle Management Software Inc. --
*/
--#delimiter
Declare
	Table_exists INTEGER;
	Type_exists INTEGER;
	Type2_exists INTEGER;
  
BEGIN
	SELECT Count(*) into Table_exists FROM user_tables WHERE table_name = 'ISOEEERRORLOG';
	SELECT Count(*) into Type_exists FROM sys.user_objects WHERE object_name = 'OTAB_ISOEEUPDATEEQP';
	SELECT Count(*) into Type2_exists FROM sys.user_objects WHERE object_name = 'OTYP_ISOEEUPDATEEQP';

    if Table_exists = 1 Then 
		Execute Immediate 'Drop Table ISOEEERRORLOG'; 
	end if;
    Execute Immediate 'CREATE TABLE ISOEEERRORLOG ("CREATION_DATETIME" DATE DEFAULT sysdate NOT NULL, "PROGID" VARCHAR2(255) NULL, "ERRMSG" VARCHAR2(4000) NULL )';
    Execute Immediate 'CREATE INDEX isOEEErrorlog_IX1 on isOEEErrorlog (Creation_Datetime)';

    if Type_exists = 1 Then
      DROP_DATABASE_OBJECT('OTAB_ISOEEUPDATEEQP', 'TYPE');
		--EXECUTE IMMEDIATE 'DROP TYPE OTAB_ISOEEUPDATEEQP'; 
	end if;

    if Type2_exists = 1 Then
      DROP_DATABASE_OBJECT('OTYP_ISOEEUPDATEEQP', 'TYPE');
		--EXECUTE IMMEDIATE 'DROP TYPE OTYP_ISOEEUPDATEEQP VALIDATE'; 
	end if;
END;
--#delimiter
create or replace TYPE OTYP_ISOEEUPDATEEQP AS OBJECT (
    -- Common Attributes
    -- Column Attributes
    EquipmentId    VARCHAR2(30),
    FromOEELossCategory       VARCHAR2(30),
    FromResourceState VARCHAR2(30),
    FromStatus 	VARCHAR2(30),
    FromReason      VARCHAR2(30),
    FromAvailability     VARCHAR2(30),
    FromStatusChangedate      VARCHAR2(30),
    ToStatus      VARCHAR2(30),
    ToReason      VARCHAR2(30),
    ToAvailability      VARCHAR2(30),
    ToStatusChangedate         VARCHAR2(30),
    WhichCase         VARCHAR2(30),
    StatusChangeDuration         VARCHAR2(100),
    -- Other non-table attributes 
    CONSTRUCTOR FUNCTION OTYP_ISOEEUPDATEEQP
    RETURN SELF AS RESULT 
)
--#delimiter

create or replace TYPE OTAB_ISOEEUPDATEEQP AS TABLE OF OTYP_ISOEEUPDATEEQP;
--#delimiter

create or replace TYPE BODY OTYP_ISOEEUPDATEEQP AS

  CONSTRUCTOR FUNCTION OTYP_ISOEEUPDATEEQP
    RETURN SELF AS RESULT AS
  BEGIN
    
    RETURN;
  END OTYP_ISOEEUPDATEEQP;

END;
--#delimiter

create or replace PACKAGE isOEERESOURCETIMEINSTATUS AUTHID CURRENT_USER
AS
--######
-- This package is a utility package. All helper functions and procedures for Camstar Products
-- should be defined in this package
--
--  Modification History:
--  Name	Date		Action
--  is		11/22/2016	Initial Creation
--
-- © 2017 Siemens Product Lifecycle Management Software Inc.
--######
	--
	--######
	-- This function calculates the time in each status for a resoruce for a shift
	-- 
	--
	--  Modification History:
	--  Name	Date		Action
	--  is		11/22/2016  	Initial Creation
	--
	-- © 2017 Siemens Product Lifecycle Management Software Inc.
	--######
    FUNCTION isOEEGetEquipmentStatusChange (
    pFromDate IN  TIMESTAMP, 
    pToDate IN  TIMESTAMP, 
    pRunDate IN TIMESTAMP 
    )
    RETURN OTAB_ISOEEUPDATEEQP;
	--
	--
	--######
	-- This procedure is used to log error messages for the OEE functions and stores procedures.
	--
	--  Modification History:
	--  Name	Date		Action
	--  is		11/22/2016  	Initial Creation
	--
	-- © 2017 Siemens Product Lifecycle Management Software Inc.
	--######
	PROCEDURE WRITE_LOG 
  (
	pvProgId     IN VARCHAR2,
	pvErrMsg     IN VARCHAR2,
  pRetVal      OUT NUMBER
  );
	--
	--
  --######
	-- The isOEEUPdateEqmtStatusByShift procedure will create the resource time in status records for 
	-- the last completed shift for each resource flagged as include in OEE calculatioins.
	-- 
	--
	--  Modification History:
	--  Name	Date		Action
	--  ----	----------	----------------
	--  is		12/05/2008  	Initial Creation
	--
	-- © 2017 Siemens Product Lifecycle Management Software Inc.
	--######
  PROCEDURE isOEEUpdateEqmtStatusByShift (
    pRunDate TIMESTAMP DEFAULT NULL
    );
  --
  --
END;
--#delimiter

create or replace PACKAGE BODY isOEERESOURCETIMEINSTATUS
AS
--######
-- This package is a utility package. All helper functions and procedures for Camstar Applications
-- should be defined in this package
--
--  Modification History:
--  Name	Date		Action
--  ----	----------	----------------
--  is		12/05/2008  	Initial Creation
--
-- © 2017 Siemens Product Lifecycle Management Software Inc.
--######
	--
FUNCTION isOEEGetEquipmentStatusChange (
    pFromDate IN  TIMESTAMP, 
    pToDate IN  TIMESTAMP, 
    pRunDate IN TIMESTAMP 
    )
RETURN OTAB_ISOEEUPDATEEQP
IS PRAGMA AUTONOMOUS_TRANSACTION;
    iSOEEEQP	OTYP_ISOEEUPDATEEQP;
    tabISOEEEQP	OTAB_ISOEEUPDATEEQP;
    i NUMBER;
    EquipmentId    VARCHAR2(30);
    FromOEELossCategory       VARCHAR2(30);
    FromResourceState VARCHAR2(30);
    FromStatus 	VARCHAR2(30);
    FromReason      VARCHAR2(30);
    FromAvailability     VARCHAR2(30);
    FromStatusChangedate      VARCHAR2(30);
    ToStatus      VARCHAR2(30);
    ToReason      VARCHAR2(30);
    ToAvailability      VARCHAR2(30);
    ToStatusChangedate         VARCHAR2(30);
    WhichCase         VARCHAR2(30);
    StatusChangeDuration         VARCHAR2(1000);

    CURSOR C1 IS WITH
    vtResourceHistory AS  
    (
        SELECT
            R.ResourceId                                            EquipmentId
            , RSH.isOldOEELossCategory                              FromOEELossCategory
            , RSH.OldResourceState                                  FromResourceState
            , oldrsc.ResourceStatusCodeName                         FromStatus
            , oldrsr.ResourceStatusReasonName FromReason
            , RSH.OldAvailability                                   FromAvailability
            ,   NVL(TO_CHAR(rsh.oldlaststatuschangedate), TO_CHAR(pFromDate,'dd-mon-yyyy hh.mi.ss.ff4 AM') )         FromStatusChangedate
            , RSH.isOEELossCategory                                 ToOEELossCategory
            , RSH.ResourceState                                     ToResourceState
            , newrsc.ResourceStatusCodeName                         ToStatus
            , newrsr.ResourceStatusReasonName                       ToReason
		    , rsh.availability                                      ToAvailability
		    , rsh.laststatuschangedate                              ToStatusChangedate
        , CASE  WHEN NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) < pFromDate AND RSH.LastStatusChangeDate > pToDate THEN 110  
			    WHEN RSH.LastStatusChangeDate >= pFromDate AND RSH.LastStatusChangeDate < pToDate AND NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) < pFromDate THEN 120
			    WHEN NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) >= pFromDate AND NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) < pToDate AND RSH.LastStatusChangeDate > pToDate THEN 130
          WHEN NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) >= pFromDate AND NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) < pToDate AND RSH.LastStatusChangeDate <= pToDate THEN 140
         WHEN RSH.LastStatusChangeDate >= pFromDate AND RSH.LastStatusChangeDate < pToDate AND NVL(rsh.oldlaststatuschangedate, cs.lastchangedate) <= pFromDate THEN 150
          ELSE -1 
            END WhichCase
        FROM ResourceDef R
        INNER JOIN ChangeStatus CS ON R.ResourceId = CS.ParentId
        INNER JOIN ResourceStatusHistory RSH ON R.ResourceId = RSH.HistoryId
        -- Old
		LEFT OUTER JOIN ResourceStatusCode oldrsc ON oldrsc.ResourceStatusCodeId = rsh.OldResourceStatusCodeId
		LEFT OUTER JOIN ResourceStatusReason oldrsr ON oldrsr.ResourceStatusReasonId = rsh.OldResourceStatusReasonCodeId
        -- New
		LEFT OUTER JOIN ResourceStatusCode newrsc ON newrsc.ResourceStatusCodeId = rsh.ResourceStatusCodeId
		LEFT OUTER JOIN ResourceStatusReason newrsr ON newrsr.ResourceStatusReasonId = rsh.ResourceStatusReasonCodeId
        WHERE r.isIncludeInOEE = 1 
    ),
    vtResourceCurrent AS  -- From Equipment Up/Down Current Status : the resource that are up/down at this point of time within the reporting period
    (
        SELECT
            R.ResourceId                                            EquipmentId
            , ps.isOEELossCategory                                  FromOEELossCategory
            , ps.ResourceState                                      FromResourceState
            , rsc.ResourceStatusCodeName                            FromStatus
            , rsr.ResourceStatusReasonName                          FromReason
            , ps.Availability                                       FromAvailability
            , CASE WHEN ps.LastStatusChangeDate IS NULL THEN TO_CHAR(pFromDate,'dd-mon-yyyy hh.mi.ss.ff4 AM') 
              ELSE TO_CHAR(ps.LastStatusChangeDate,'dd Month yyyy')
              END                                                   FromStatusChangedate
            , NULL                                                  ToOEELossCategory
            , NULL                                                  ToResourceState
            , NULL                                                  ToStatus
            , NULL                                                  ToReason
            , NULL                                                  ToAvailability
            , NULL                                                  ToStatusChangedate
            , CASE 
			        WHEN pFromDate >= pRunDate 
				        THEN  210
                
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < pFromDate 
                        AND pToDate > pRunDate
                        THEN   220   
                   
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < pFromDate 
                        AND pToDate <= pRunDate
                        THEN   230

				    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) >= pFromDate 
                        AND (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < pToDate
                        AND pToDate < pRunDate
                        THEN   240
                  
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) >= pFromDate 
                        AND (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < pToDate
                        AND pToDate >= pRunDate
                        THEN   250
                  ELSE -- 
                        -1 
            END WhichCase 
        FROM
            ResourceDef R
            LEFT OUTER JOIN ProductionStatus PS ON R.ProductionStatusId = PS.ProductionStatusId
            INNER JOIN ChangeStatus CS ON R.ResourceId = CS.ParentId
		    LEFT OUTER JOIN ResourceStatusCode rsc ON rsc.ResourceStatusCodeId = ps.statusid
		    LEFT OUTER JOIN ResourceStatusReason rsr ON rsr.ResourceStatusReasonId = ps.ReasonId
        WHERE r.isIncludeInOEE = 1 
    )
    SELECT * 
    FROM (
	    SELECT EquipmentId, FromOEELossCategory, FromResourceState, FromStatus, FromReason, FromAvailability, FromStatusChangedate, ToStatus, ToReason, ToAvailability, ToStatusChangedate,  WhichCase 
         ,CASE 
              WHEN WhichCase = 110 THEN TO_CHAR((CAST(pToDate AS Date) - CAST(pFromDate AS Date)) * 24 * 60)
              WHEN WhichCase = 120 THEN TO_CHAR((CAST(t1.ToStatusChangedate AS DATE) - CAST(pFromDate AS Date)) * 24 * 60 ) 
              WHEN WhichCase = 130 THEN TO_CHAR((CAST(pToDate AS Date) - CAST(t1.FromStatusChangedate AS Date)) * 24 * 60 ) 
              WHEN WhichCase = 140 THEN TO_CHAR((CAST(t1.ToStatusChangedate AS DATE) - CAST(t1.FromStatusChangedate AS DATE)) * 24 * 60 )
              WHEN WhichCase = 150 THEN TO_CHAR((CAST(t1.ToStatusChangedate AS DATE) - CAST(pFromDate AS Date)) * 24 * 60 )
              WHEN WhichCase = 160 THEN TO_CHAR((CAST(pToDate AS Date) - CAST(pFromDate AS Date)) * 24 * 60 )
             else '0'
              END StatusChangeDuration
	    FROM vtResourceHistory t1
	    UNION ALL
	    SELECT EquipmentId, FromOEELossCategory, FromResourceState, FromStatus, FromReason, FromAvailability, FromStatusChangedate, ToStatus, ToReason, ToAvailability, ToStatusChangedate,  WhichCase 
            ,CASE 
              WHEN WhichCase = 210 THEN '0'
              WHEN WhichCase = 220 THEN TO_CHAR((CAST(pRunDate AS Date) - CAST(pFromDate AS Date)) * 24 * 60)
              WHEN WhichCase = 230 THEN TO_CHAR((CAST(pToDate AS Date) - CAST(pFromDate AS Date)) * 24 * 60)
           WHEN WhichCase = 240 THEN TO_CHAR((CAST(pToDate AS Date) - CAST(T2.FromStatusChangedate AS Date)) * 24 * 60 )
           WHEN WhichCase = 250 THEN TO_CHAR((CAST(pRunDate AS Date) - CAST(T2.FromStatusChangedate AS Date)) * 24 * 60 )
              else '0'
              END StatusChangeDuration
	    FROM vtResourceCurrent t2
    ) t
    WHERE t.WhichCase <> -1 
        AND FromStatus IS NOT NULL; -- Note : Equipment which is newly created will not have any Equipment State and therefore should be excluded from the query.
        
         
                --WRITE_LOG (vProgID,vLastShiftFromDate||'aaaa'||vLastShiftToDate||'aaa'||vRunDate,pRetVal); 
                
BEGIN
iSOEEEQP := NEW	OTYP_ISOEEUPDATEEQP();	
tabISOEEEQP := NEW	OTAB_ISOEEUPDATEEQP();
i := 0;
OPEN C1;
LOOP
    EquipmentId := '';
    FromOEELossCategory :='';
    FromResourceState :='';
    FromStatus :='';
    FromReason :='';
    FromAvailability :='';
    FromStatusChangedate :='';
    ToStatus :='';
    ToReason :='';
    ToAvailability :='';
    ToStatusChangedate :='';
    WhichCase :='';
    StatusChangeDuration :='';
    
    FETCH C1 INTO EquipmentId, FromOEELossCategory, FromResourceState, FromStatus, FromReason, FromAvailability, FromStatusChangedate, ToStatus, ToReason, ToAvailability, ToStatusChangedate, WhichCase, StatusChangeDuration;
    EXIT WHEN c1%NOTFOUND;
    i := i+1;
    iSOEEEQP.EquipmentId := EquipmentId;
    iSOEEEQP.FromOEELossCategory := FromOEELossCategory;
    iSOEEEQP.FromResourceState := FromResourceState;
    iSOEEEQP.FromStatus := FromStatus;
    iSOEEEQP.FromReason := FromReason;
    iSOEEEQP.FromAvailability := FromAvailability;
    iSOEEEQP.FromStatusChangedate := FromStatusChangedate;
    iSOEEEQP.ToStatus := ToStatus;
    iSOEEEQP.ToReason := ToReason;
    iSOEEEQP.ToAvailability := ToAvailability;
    iSOEEEQP.ToStatusChangedate := ToStatusChangedate;
    iSOEEEQP.WhichCase := WhichCase;
    iSOEEEQP.StatusChangeDuration := StatusChangeDuration;
    tabISOEEEQP.extend;
    tabISOEEEQP(i) := iSOEEEQP;
    END LOOP;
    CLOSE C1;
    RETURN (tabISOEEEQP);
END isOEEGetEquipmentStatusChange;
	--
	--
	--
     PROCEDURE WRITE_LOG 
  (
	pvProgId     IN VARCHAR2,
	pvErrMsg     IN VARCHAR2,
  pRetVal      OUT NUMBER
  )
     IS
     --######
     -- This procedure is used for logging process messages. Depending on the process configuration,
     -- the logs will be written accordingly. By default, logs are always written to a database table.
     --
     --  Modification History:
     --  Name   Date       Action
     --  ----   ---------- ----------------
     --  is          12/05/2008      Initial Creation
     --
     -- © 2017 Siemens Product Lifecycle Management Software Inc.
     --######
     --
     PRAGMA AUTONOMOUS_TRANSACTION;
     --
     n_ErrLocator          NUMBER;
     --
     BEGIN
	--
	INSERT INTO isOEEERRORLOG (progId, ErrMsg) values (pvProgId, pvErrMsg);

	COMMIT;
	--
     EXCEPTION
           WHEN OTHERS THEN
                --
                RAISE_APPLICATION_ERROR ( -20104,'WRITE_LOG failed - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
                --
     END;
     --

PROCEDURE isOEEUpdateEqmtStatusByShift (
    pRunDate TIMESTAMP DEFAULT NULL
    )
AS
vCheckStoredCalendar    VARCHAR2(30);
pRetVal                    NUMBER;
 v_code                   NUMBER;
 v_errm                   VARCHAR2(64); 
checkcurrentshift           NUMBER;
 ErrorMessage               VARCHAR2(4000):='';
  ErrorSeverity              NUMBER;            -- Severity.
 ErrorState                 NUMBER;            -- State.
 xstate                     NUMBER;
 vObject_Name               VARCHAR2(128); 
 vProgID                    VARCHAR2(255); 
--
 vRunDate                   TIMESTAMP;
 vRunFromDate               TIMESTAMP;
 vRunToDate                 TIMESTAMP;
 vCurrentStoredCalendarShiftId  CHAR(16) := Null;  -- The CalendarShiftId that was stored in the isOEECurrentShift table
 vCurrentRunCalendarShiftId     CHAR(16) := Null;  -- The CalendarShiftId of the current run of this stored procedure
--
 vLastShiftDate             TIMESTAMP; -- Any DATETIME value that falls within the Last Shift Period
 vLastShiftFromDate         TIMESTAMP; -- Last Shift Period's From Date
 vLastShiftToDate           TIMESTAMP; -- Last Shift Period's To Date
 vShiftName                 VARCHAR2(30);
 vCalendarDate              TIMESTAMP;
BEGIN
 -- Message text.
 

  vObject_Name:='isOEEUpdateEqmtStatusByShift';
  vProgID                     := vObject_Name;

    --    
    vProgID := vObject_Name || '.START';
 

  
        IF pRunDate IS NULL THEN
            vRunDate := SYSTIMESTAMP;
        ELSE
            vRunDate := pRunDate;
            END IF;
            
  ---------------------------------------------------------------------------
		-- Get the CurrentStoredCalendarShiftId.
		---------------------------------------------------------------------------
	   BEGIN
            SELECT NVL(oeeCS.CurrentCalendarShiftId,'noCurrentStoredCalendar') INTO vCheckStoredCalendar
            FROM isOEECurrentShift oeeCS
            ;
            
            IF vCheckStoredCalendar = 'noCurrentStoredCalendar' THEN
                  
                vCurrentStoredCalendarShiftId := NULL;
            ELSE
                vCurrentStoredCalendarShiftId := vCheckStoredCalendar;
                
           END IF;
	       -- RAISERROR (ErrorMessage, 16, 1);
          EXCEPTION
           WHEN NO_DATA_FOUND THEN
          vCurrentStoredCalendarShiftId:= NULL;
          
        WHEN OTHERS THEN
          v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm;
	        vProgID := vObject_Name || '.' || 'Failed to get the CurrentStoredCalendarShiftId from isOEECurrentShift table'; 
          WRITE_LOG (vProgID,ErrorMessage,pRetVal); 
          raise_application_error(-20001,'An error was encountered - '||v_code||' -ERROR- '||v_errm);
	       -- RAISERROR (ErrorMessage, 16, 1);
       END;
        --
		---------------------------------------------------------------------------
		-- Get the CurrentRunCalendarShiftId.
		---------------------------------------------------------------------------

	    BEGIN
            SELECT 
                cs.CalendarShiftId, cs.ShiftStart, cs.ShiftEnd into vCurrentRunCalendarShiftId, vRunFromDate, vRunToDate
            FROM CalendarShift cs 
            WHERE vRunDate BETWEEN cs.ShiftStart AND cs.ShiftEnd
            ;
           EXCEPTION
       WHEN OTHERS THEN
          v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm; 
	        vProgID := vObject_Name || '.' || 'Failed to get the CurrentRunCalendarShiftId from CalendarShift table'; 
          WRITE_LOG (vProgID,ErrorMessage,pRetVal); 
          raise_application_error(-20001,'An error was encountered - '||v_code||' -ERROR- '||v_errm);
	     
          
        END;
        
         BEGIN
            IF vCurrentStoredCalendarShiftId IS NULL OR 
               vCurrentRunCalendarShiftId <> vCurrentStoredCalendarShiftId THEN
               BEGIN
                dbms_output.put_line ('Computing the Equipment StatusChange information of the last shift...');
 
                vLastShiftDate := INTERVAL '-1' hour + vRunFromDate;
                --
                SELECT 
                     cs.ShiftStart, cs.ShiftEnd, s.ShiftName, cs.CalendarDate INTO vLastShiftFromDate, vLastShiftToDate, vShiftName, vCalendarDate
                FROM CalendarShift cs 
                INNER JOIN Shift s ON s.shiftid = cs.shiftid
                WHERE vLastShiftDate BETWEEN cs.ShiftStart AND cs.ShiftEnd
                ;
                
                INSERT INTO isOEEResourceDetailsByShift (
                    CalendarDate
                    , CalendarShiftId
                    , OEELossCategory
                    , OEELossCategoryName 
                    , ResourceFamily
                    , ResourceId
                    , ResourceName
                    , ResourceState
                    , ResourceStateName
                    , ResourceStatusCode
                    , ResourceStatusReason
                    , Shift
                    , TimeInStatus
                    )
                SELECT 
                    vCalendarDate
                    , vCurrentRunCalendarShiftId
                    , esc.FromOEELossCategory
                    , CASE 
                      WHEN FromOEELossCategory = 1 THEN 'Availability Loss'
                      WHEN FromOEELossCategory = 2 THEN 'Performance Loss'
                      WHEN FromOEELossCategory = 3 THEN 'Schedule Loss'
                      ELSE 'No Such Loss Category'
                      END FromOEELossCategoryName 
                    , rf.ResourceFamilyName
                    , esc.EquipmentId
                    , rd.ResourceName
                    , esc.FromResourceState
                    , CASE 
                      WHEN FromResourceState = 1 THEN 'NonScheduled'
                      WHEN FromResourceState = 2 THEN 'UnscheduleDown'
                      WHEN FromResourceState = 3 THEN 'ScheduleDown'
                      WHEN FromResourceState = 4 THEN 'Engineering'
                      WHEN FromResourceState = 5 THEN 'Productive'
                      WHEN FromResourceState = 6 THEN 'Standby'
                      ELSE 'No Such Resource State'
                      END FromResourceStateName 
                    , esc.FromStatus
                    , esc.FromReason
                    , vShiftName
                    , esc.StatusChangeDuration
                FROM TABLE (isOEEGetEquipmentStatusChange (vLastShiftFromDate, vLastShiftToDate, vRunDate)) esc 
                INNER JOIN ResourceDef rd ON rd.ResourceId = esc.EquipmentId
                LEFT OUTER JOIN ResourceFamily rf ON rf.resourcefamilyid = rd.resourcefamilyid
                ORDER BY rd.ResourceName, esc.FromStatusChangedate
                ;
  
                   
              END;
            ELSE
                dbms_output.put_line ('There is no need to recompute the Equipment StatusChange information of the last shift.');
      
         
              
           END IF;
           
           EXCEPTION
       WHEN OTHERS THEN
	        v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm;
	        vProgID := vObject_Name || '.' || 'Failed to insert record(s) into isOEEResourceDetailsByShift table'; 
          WRITE_LOG (vProgID,ErrorMessage,pRetVal); 
	        raise_application_error(-20001,'An error was encountered - '||v_code||' -ERROR- '||v_errm);
     
           
           END;
           
           
           
        BEGIN
                 SELECT COUNT(oeeCS.CurrentCalendarShiftId ) INTO checkcurrentshift
                       FROM isOEECurrentShift oeeCS;
                       
                        
                       IF checkcurrentshift >0 THEN
                          UPDATE isOEECurrentShift SET
                          CurrentCalendarShiftId = vCurrentRunCalendarShiftId ;
                       ELSE
                            INSERT INTO isOEECurrentShift (CurrentCalendarShiftId)
                            VALUES (vCurrentRunCalendarShiftId)
                              ;
                        END IF;
        --   END TRY
        --   BEGIN CATCH
        EXCEPTION
        WHEN OTHERS THEN
	        v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm;
	        vProgID := vObject_Name || '.' || 'Failed to INSERT/UPDATE CurrentCalendarShiftId into the isOEECurrentShift table'; 
          WRITE_LOG (vProgID,ErrorMessage,pRetVal); 
	        raise_application_error(-20001,'An error was encountered - '||v_code||' -ERROR- '||v_errm);
        END;

			COMMIT;
        --
        --------------------------------------------------------------------------
        vProgID := vObject_Name || '.SUCCESSFULL'; 
		RETURN;


EXCEPTION
WHEN OTHERS THEN
		IF ErrorMessage IS NULL THEN
		     v_code := SQLCODE;
          v_errm := SUBSTR(SQLERRM, 1 , 64);
          ErrorMessage := v_code || '-' ||v_errm;
         END IF;
	
 		IF vProgID IS NULL THEN
		     vProgID := vObject_Name || '.OTHER ERROR'; 
         END IF;
      
            ROLLBACK;
        WRITE_LOG (vProgID,ErrorMessage,pRetVal); 
        raise_application_error(-20001,'An error was encountered - '||v_code||' -ERROR- '||v_errm);

  
END;
END;
--#delimiter
