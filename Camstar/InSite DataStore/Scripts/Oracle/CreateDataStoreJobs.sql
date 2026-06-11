--------------------------------------------------------------------------------
-- SCRIPT: CreateDataStoreJobs.sql
-- DESCR: Creates the Datastore jobs
-- HISTORY:
--   	10/12/2016	Dan Maloney		Removed code that starts the DataStore.  CMS SU13+ takes care of restoring the DataStore state
--      v8.4 (1)    Dan Maloney     Replace DBMS_JOB with DBMS_SCHEDULER (US 11177) 
--		01/29/2025	Madhuri B		Updated the job class name to be specific to the schema in use.
--
-- Copyright Siemens 2025  

DECLARE
   INSUFFICIENT_PRIVILEGES EXCEPTION;
   PRAGMA EXCEPTION_INIT(INSUFFICIENT_PRIVILEGES, -27486);
   
	--Get DataStore Jobs that are defined for DBMS_JOB for this schema
   	CURSOR c1 IS
   	SELECT Job
   	FROM USER_JOBS
  	WHERE WHAT LIKE '%csiDataStorePackage%';
	
	--US 11177, Get DataStore Jobs that are defined for DBMS_SCHEDULER for this schema
   	CURSOR c2 IS
   	SELECT Job_Name AS JOB_NAME
   	FROM USER_SCHEDULER_JOBS
  	WHERE JOB_ACTION LIKE '%csiDataStorePackage%';
   	--
	CURSOR c3 IS
   	SELECT '"' || Job_Class_Name || '"' AS JOB_CLASS_NAME
   	FROM ALL_SCHEDULER_JOB_CLASSES
  	WHERE upper(JOB_CLASS_NAME) = upper('OPCENTER_'||SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA'));
	--
	v_schemaName			VARCHAR2(128);
	v_Database				VARCHAR2(128);
	v_JobClass				VARCHAR2(128);
	v_SQL					VARCHAR2(1024);
	gDateFormat             VARCHAR2(32) :='YYYY-MM-DD HH24:MI:SS';    -- configurable format for package to use in alter stmt.
	gTimeStampFormat        VARCHAR(32) := 'YYYY-MM-DD HH24:MI:SSXFF'; -- configurable format for package to use in alter stmt.
	n_LogRetentionDays		NUMBER;
BEGIN

	v_SQL := 'ALTER SESSION SET NLS_DATE_FORMAT=''' || gDateFormat || '''';    
	EXECUTE IMMEDIATE v_SQL;  
 
	v_SQL := 'ALTER SESSION SET NLS_TIMESTAMP_FORMAT=''' || gTimeStampFormat || '''';    
	EXECUTE IMMEDIATE v_SQL;  
		
	v_Database := SYS_CONTEXT('USERENV','CON_NAME'); 
	v_schemaName := SYS_CONTEXT('USERENV','CURRENT_SCHEMA');
	v_JobClass := '"OPCENTER_' || v_schemaName || '"'; 
	
	--Remove all DataStore Jobs that are defined for DBMS_JOB for this schema
   	FOR crec IN c1 LOOP
      		DBMS_JOB.REMOVE(JOB => CREC.JOB);
   	END LOOP;
	
	
	--US 11177, Remove all DataStore Job classes that are defined in DBMS_SCHEDULER for the ODS
   	FOR crec IN c2 LOOP
      		DBMS_SCHEDULER.DROP_JOB(JOB_NAME => CREC.JOB_NAME, FORCE => TRUE);
   	END LOOP;
   	
	
	--US 11177, Remove all DataStore Jobs that are defined for DBMS_SCHEDULER for this schema for the ODS
   	FOR crec IN c3 LOOP
      		DBMS_SCHEDULER.DROP_JOB_CLASS(JOB_CLASS_NAME => CREC.JOB_CLASS_NAME, FORCE => TRUE);
   	END LOOP;
   	--
    	
	--US 11177, Create job class for all ODS jobs that links to the servicename the ODS sessions run under
	BEGIN
		DBMS_SCHEDULER.CREATE_JOB_CLASS(
			JOB_CLASS_NAME => v_JobClass,
			SERVICE => NULL, 
			LOGGING_LEVEL => DBMS_SCHEDULER.LOGGING_RUNS, 
			LOG_HISTORY => 1000000, 
			COMMENTS => 'Job Class for the DataStore Jobs for the Database [ ' || v_Database || ' ]'
		);
	EXCEPTION
	WHEN OTHERS THEN
		NULL;
	END;
	
	--US 11177, Create the csiDataStorePackage.MANAGER job amd set it to run immediately and at an interval of every minute
	DBMS_SCHEDULER.CREATE_JOB(
		JOB_NAME => 'OPCENTER_DATASTORE_MANAGER',
		JOB_TYPE => 'PLSQL_BLOCK',
		JOB_ACTION => 'BEGIN csiDataStorePackage.MANAGER; END;',
		START_DATE =>  TRUNC(SYSDATE,'MI'),
		REPEAT_INTERVAL => 'FREQ=MINUTELY',
		JOB_CLASS => v_JobClass,
		ENABLED => TRUE,
		COMMENTS => 'DataStore Manager Job for Database : [ ' || v_Database  || ' ] Schema : [ ' || v_schemaName || ' ]',
		AUTO_DROP => FALSE
	);
	
EXCEPTION
WHEN INSUFFICIENT_PRIVILEGES THEN
		RAISE_APPLICATION_ERROR(-20000,'Insufficient Privileges.  Check the DataStore Reference Guide prerequisites.  GRANT MANAGE SCHEDULER permission may not have been granted to ' || user);
WHEN OTHERS THEN
		RAISE;
END;
/







