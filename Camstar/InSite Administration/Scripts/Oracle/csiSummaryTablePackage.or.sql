-- Copyright Siemens 2023  

CREATE OR REPLACE 
PACKAGE csiSummaryTablePackage
IS
   PROCEDURE RunScheduled;
   PROCEDURE RunAll;
   PROCEDURE RunSingle(pID IN CHAR);
   PROCEDURE RunSingleByName(pName IN VARCHAR2);
   FUNCTION GetNextRunDate(pID IN CHAR, pCurrentDate IN DATE) RETURN DATE;
END; -- Package spec
/
CREATE OR REPLACE 
PACKAGE BODY csiSummaryTablePackage IS
--------------------------------------------------------------------
-- Name: RunScheduled
-- Params: 
--
-- Descr: Processes any waiting summary jobs that are enabled and not
--        marked with IsManuallyExecuted
--
-- History:
--
PROCEDURE RunScheduled IS
   CURSOR cJob IS
     SELECT SummaryTableDefId, SummaryTableDefName
       FROM SummaryTableDef 
      WHERE (IsEnabled=1 OR IsView=1)
        AND (IsManuallyExecuted=0 OR IsView=1)
		AND TargetTableName IS NOT NULL
        AND (NextRunDate<SYSDATE OR NextRunDate IS NULL)
        AND (EndDate>SYSDATE OR EndDate IS NULL)
        AND (StartDate<=SYSDATE OR StartDate IS NULL)
      UNION
       SELECT SummaryTableDefId, SummaryTableDefName
        FROM SummaryTableDef
       WHERE (ForceExecute=1 OR ForceRefresh=1)
	   AND TargetTableName IS NOT NULL;
BEGIN
   -- Find any summary jobs that are waiting to be processed
   FOR crec IN cJob LOOP
      BEGIN
         DBMS_OUTPUT.PUT_LINE('Summary waiting: '||crec.SummaryTableDefName||'('||crec.SummaryTableDefId||').');
         RunSingle(crec.SummaryTableDefId);
      EXCEPTION WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE('Unhandled exception in RunScheduled. '||SQLERRM);
      END;
   END LOOP;
     
EXCEPTION WHEN OTHERS THEN
   DBMS_OUTPUT.PUT_LINE('Unhandled exception in RunScheduled. '||SQLERRM);
END;
--------------------------------------------------------------------
-- Name: RunAll
-- Params: 
--
-- Descr: Processes all summary jobs that are enabled
--
-- History:
--
PROCEDURE RunAll IS
   CURSOR cJob IS
     SELECT SummaryTableDefId, SummaryTableDefName
       FROM SummaryTableDef 
      WHERE IsEnabled=1;
BEGIN
   FOR crec IN cJob LOOP
      BEGIN
         RunSingle(crec.SummaryTableDefId);
      EXCEPTION WHEN OTHERS THEN
         DBMS_OUTPUT.PUT_LINE('Unhandled exception in RunAll. '||SQLERRM);
      END;
   END LOOP;
     
EXCEPTION WHEN OTHERS THEN
   DBMS_OUTPUT.PUT_LINE('Unhandled exception in RunAll. '||SQLERRM);
END;
--------------------------------------------------------------------
-- Name: RunSingle
-- Params: 
--
-- Descr: Processes a single summary job regardless of whether or
--        not it is enabled.
--
-- History:
-- 
PROCEDURE RunSingle(pID IN CHAR) IS
  v_CurrDate    DATE;
  v_NextDate    DATE;
  --
  v_TargetTableName VARCHAR2(255);
  v_TempTableName   VARCHAR2(255);
  v_SummarySQL      CLOB;
  v_LastRunDate     DATE;
  v_AppendToTable   NUMBER;
  v_IsView          NUMBER;
  v_ForceRefresh    NUMBER;
  --
  v_Cnt  NUMBER;
  v_Find NUMBER;
  v_Type VARCHAR2(128);
  v_Err  VARCHAR2(512);
  v_ErrLoc NUMBER;
BEGIN
   -- Since the view query may take a while, get the current date here and use throughout
   v_CurrDate := SYSDATE;   
   v_ErrLoc := 1;
   --
   SELECT case IsView when 0 then 'csiTbl_'||TargetTableName else 'csiView_'||TargetTableName end
          ,SummarySQL
          ,LastRunDate
	        ,AppendToTable
	        ,IsView
	        ,ForceRefresh
   INTO v_TargetTableName
        ,v_SummarySQL
        ,v_LastRunDate
        ,v_AppendToTable
        ,v_IsView
        ,v_ForceRefresh
   FROM SummaryTableDef
   WHERE SummaryTableDefId=pID;
   --
   IF (DBMS_LOB.GETLENGTH(v_SummarySQL) = 0) THEN
      RETURN;
   END IF;
   --
   IF (LENGTH(v_TargetTableName) > 30) THEN
      UPDATE SummaryTableDef
         SET LastRunDate = v_CurrDate,
             LastRunMessage = 'Summary Table name is too long. Maximum is 22 characters.',
             LastRunSuccess = 0,
             NextRunDate = NULL,
             LastRunElapsedSeconds = (SYSDATE-v_CurrDate)*(86400),
             ForceRefresh=0,
             ForceExecute=0
      WHERE SummaryTableDefId=pID;
      COMMIT;
      RETURN;
   END IF;
   --
   v_ErrLoc := 10;
   IF (v_ForceRefresh = 1) THEN
      -- Drop the view/table and reset the LastRunDate=NULL
		  SELECT COUNT(*), MAX(OBJECT_TYPE) 
      INTO v_Cnt, v_Type
      FROM USER_OBJECTS
		  WHERE OBJECT_NAME=UPPER(v_TargetTableName);
		
      v_ErrLoc := 20;
		  IF (v_Cnt>0) THEN
			   EXECUTE IMMEDIATE 'DROP '||v_Type||' '||v_TargetTableName;
		  END IF;
		  v_LastRunDate := NULL;
   END IF;
   --
   IF (v_IsView = 1) THEN
      v_ErrLoc := 30;
      SELECT COUNT(*)
      INTO v_Cnt
      FROM USER_OBJECTS
		  WHERE OBJECT_NAME=UPPER(v_TargetTableName);
       
      v_ErrLoc := 40;  
      IF (v_Cnt = 0) THEN
         EXECUTE IMMEDIATE 'CREATE VIEW '||v_TargetTableName||' AS '||v_SummarySQL;
      END IF;
   ELSE 
      v_ErrLoc := 50;
      -- It's a table instead of a view
      SELECT COUNT(*)
      INTO v_Cnt
      FROM USER_OBJECTS
		  WHERE OBJECT_NAME=UPPER(v_TargetTableName);
      
      IF (v_AppendToTable = 1 AND v_Cnt > 0) THEN -- If we are appending AND the table already exists
			   -- The SQL Query may have a parameter named :LastRunDate.
			   -- Usage: INSERT INTO ExistingTable (SQL Query)
         v_ErrLoc := 60;
         v_Find := DBMS_LOB.INSTR(v_SummarySQL,':');
         IF (v_find > 0) THEN
            -- Query parameter was found. 
            v_ErrLoc := 70;
            EXECUTE IMMEDIATE 'INSERT INTO '||v_TargetTableName||' '||v_SummarySQL USING v_LastRunDate;
         ELSE
            v_ErrLoc := 80;
            EXECUTE IMMEDIATE 'INSERT INTO '||v_TargetTableName||' '||v_SummarySQL;
         END IF;
      ELSE
         v_ErrLoc := 90;
         -- Drop and recreate the entire table using a temp table
         v_TempTableName := 'tmp_'||SUBSTR(v_TargetTableName,1,26);
         SELECT COUNT(*)
         INTO v_Cnt
         FROM USER_OBJECTS
         WHERE OBJECT_TYPE='TABLE'
         AND OBJECT_NAME=UPPER(v_TempTableName);
         
         IF (v_Cnt>0) THEN
            v_ErrLoc := 100;
            EXECUTE IMMEDIATE 'DROP TABLE '||v_TempTableName;
         END IF;
         --
         v_ErrLoc := 110;
         EXECUTE IMMEDIATE 'CREATE TABLE '||v_TempTableName||' AS '||v_SummarySQL;
         --
         v_ErrLoc := 120;
         SELECT COUNT(*)
         INTO v_Cnt
         FROM USER_OBJECTS
         WHERE OBJECT_TYPE='TABLE'
         AND OBJECT_NAME=UPPER(v_TargetTableName);
          IF (v_Cnt>0) THEN
            v_ErrLoc := 130;
            EXECUTE IMMEDIATE 'DROP TABLE '||v_TargetTableName;
         END IF;
         --
         v_ErrLoc := 140;
         EXECUTE IMMEDIATE 'ALTER TABLE '||v_TempTableName||' RENAME TO '||v_TargetTableName;
      END IF;
   END IF;
   --
   v_ErrLoc := 150;
   v_NextDate := GetNextRunDate(pID,v_CurrDate);
   UPDATE SummaryTableDef
   SET LastRunDate = v_CurrDate,
       LastRunMessage = NULL,
       LastRunSuccess = 1,
       NextRunDate = v_NextDate,
       LastRunElapsedSeconds = (SYSDATE-v_CurrDate)*(86400),
       ForceRefresh=0,
       ForceExecute=0
   WHERE SummaryTableDefId=pID;
   --
EXCEPTION WHEN OTHERS THEN
   v_Err := 'ErrLoc('||TO_CHAR(v_ErrLoc)||') '||SQLERRM;
   DBMS_OUTPUT.PUT_LINE('Unhandled exception in RunSingle. '||v_Err);
   UPDATE SummaryTableDef
   SET LastRunDate = v_CurrDate,
       LastRunMessage = v_Err,
       LastRunSuccess = 0,
       NextRunDate = v_NextDate,
       LastRunElapsedSeconds = (SYSDATE-v_CurrDate)*(86400),
       ForceRefresh=0,
       ForceExecute=0
   WHERE SummaryTableDefId=pID;
END;
--
--------------------------------------------------------------------
-- Name: RunSingleByName
-- Params: 
--
-- Descr: Looks up a SummaryTableDef by Name, then executes it via
--        RunSingle()
--
-- History:
-- 
PROCEDURE RunSingleByName(pName IN VARCHAR2) IS
   v_ID CHAR(16);
BEGIN
   SELECT SummaryTableDefId
   INTO v_ID
   FROM SummaryTableDef
   WHERE UPPER(SummaryTableDefName) = UPPER(pName);
   --
   RunSingle(v_ID);
END;
--------------------------------------------------------------------
-- Name: GetNextRunDate
-- Params: 
--
-- Descr: For the given SummaryTableDef item and a given date/time,
--        find the next date in which this job should run.
--
-- History:
-- 
FUNCTION GetNextRunDate(pID IN CHAR, pCurrentDate IN DATE) RETURN DATE 
IS
   v_CurrDate    DATE;
   v_Hours       VARCHAR2(255);
   v_DaysOfWeek  VARCHAR2(255);
   v_DaysOfMonth VARCHAR2(255);
   v_Months      VARCHAR2(255);
   v_NextDate    DATE;
   v_ContinueLoop BOOLEAN;
   v_SQL   VARCHAR2(512);
BEGIN
   -- We only go as granular as hours, so take the input date, strip off the minutes and seconds
   v_CurrDate := pCurrentDate;
   v_NextDate := TO_DATE(TO_CHAR(v_CurrDate,'MM/DD/YYYY HH24')||':00:00','MM/DD/YYYY HH24:MI:SS'); 
   
   -- The configuration contains comma-delimited lists of selected hours, days, months, etc.
   -- If any of these are null, it means run for every item (e.g. every hour)
   -- Hours: 0-23
   -- DaysOfWeek: 1-7 (1=Sunday)
   -- DaysOfMonth: 1-31 (depending on month)
   -- Months: 1-12
   SELECT RTRIM(ScheduleHours,','),
          RTRIM(ScheduleDaysOfWeek,','),
          RTRIM(ScheduleDaysOfMonth,','),
          RTRIM(ScheduleMonths,',')
   INTO v_Hours,
        v_DaysOfWeek,
        v_DaysOfMonth,
        v_Months
   FROM SummaryTableDef
   WHERE SummaryTableDefId=pID;
   --
   -- Find the next run date based on the configuration.
   -- The SummaryTableDef allows each summary to be executed based on certain:
   --   HoursOfDay
   --   DaysOfWeek
   --   DaysOfMonth
   --   Months
   -- Each of these columns contains a comma-delimted list of numbers. e.g. '1,3,5,7,9'.
   -- If any of these columns is NULL, it means that the summary should run for EVERY (e.g. every day of the week).
   -- There may be a more efficient algorithm, but the below logic simply steps ahead from the current
   -- date by 1-hour increments and checks to see if that date/time matches the configured hours, days, months
   v_NextDate := v_NextDate + INTERVAL '1' HOUR; -- The shortest update time is 1 hour, so start there and then find the next run time
   v_ContinueLoop:= TRUE;
   WHILE (v_ContinueLoop) LOOP
      v_SQL := 'SELECT TO_DATE('''||TO_CHAR(v_NextDate,'MM/DD/YYYY HH24:MI:SS')||''',''MM/DD/YYYY HH24:MI:SS'') FROM DUAL WHERE 1=1 ';
      IF (v_Hours IS NOT NULL) THEN
         v_SQL := v_SQL||' AND '||TO_NUMBER(TO_CHAR(v_NextDate,'HH24'))||' IN ('||v_Hours||')';
      END IF;
      IF (v_DaysOfWeek IS NOT NULL) THEN
         v_SQL := v_SQL||' AND '||TO_NUMBER(TO_CHAR(v_NextDate,'D'))||' IN ('||v_DaysOfWeek||')';
      END IF;
      IF (v_DaysOfMonth IS NOT NULL) THEN
         v_SQL := v_SQL||' AND '||TO_NUMBER(TO_CHAR(v_NextDate,'DD'))||' IN ('||v_DaysOfMonth||')';
      END IF;
      IF (v_Months IS NOT NULL) THEN
         v_SQL := v_SQL||' AND '||TO_NUMBER(TO_CHAR(v_NextDate,'MM'))||' IN ('||v_Months||')';
      END IF;
      BEGIN
         EXECUTE IMMEDIATE v_SQL INTO v_NextDate;
         v_ContinueLoop :=  FALSE;
      EXCEPTION WHEN NO_DATA_FOUND THEN
         v_NextDate := v_NextDate + INTERVAL '1' HOUR;
         IF (v_NextDate>SYSDATE+100) THEN -- Safety net
            v_ContinueLoop:=false;
         END IF;
      END;
   END LOOP;
   RETURN v_NextDate;
END;
--
END;
/
