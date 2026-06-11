--------------------------------------------------------------------------------
-- SCRIPT: CreateDataStoreLocalTables.sql
-- DESCR: Creates the Datastore tables in the datastore database
-- HISTORY:
--   	05/03/2005 			Added PROCESSEDID column to DataStoreSessionTracking
--   	06/23/2005 			Added master table references
--   	06/30/2005 			Changed DATASTORETEMP from GLOBAL TEMPORARY to regular, added
--               			SESSIONNAME column.
--   	07/01/2005 			Added DATASTOREMISSEDTXNS table.
--   	08/10/2005 			Added support for DATASTORESYNC
--   	09/02/2005 			Removed DATASTOREINFO synonym
--  	11/08/2005 			Added LOGDATE column to DATASTORERRORS, DATASTOREMISSINGTXNS
--   	12/02/2005 			Added VERSION parameter. Change to drop/recreate triggers.
--  	12/28/2005 			Added copy from DataStoreSetup to InSiteSiteInfo
--  	03/01/2006 			Added STOP_IF_RETRIES_EXCEEDED parameter
--  	12/06/2006 			Updated copyright notice (SPR S9984). Bill Lippard
--  	01/03/2007 	BillLippard	Changed default setting of DataStoreSetup.Missing_Txn_Retries
--		       			from 4 to 20 (SPR S11319).  Updated copyright notice to
--		        		2007 (SPR S9984). 
--  	06/04/2012  			Update master table creation/alter to include new columns for extended DS functionality
--	04/19/2016	Dan Maloney	Cleaned up script, added indentation, add place in history comments for name of person making changes
--	04/19/2016	Dan Maloney	Added new inserts into DATASTORESETUP for parameters LOG_LEVEL and LOG_RETENTION
--	04/19/2106	Dan Maloney	Added drop table for DATASTORELOG table if it exists
--	04/19/2106	Dan Maloney	Added create table for DATASTORELOG to create it with new definition
--	04/19/2106	Dan Maloney	Added create table for DATASTOREEMAILSETUP 
--	04/19/2106	Dan Maloney	Added new inserts into DATASTOREEMAILSETUP for parameters MAIL_SERVER, MAIL_SENDER adn MAIL_RECIPIENTS
--	04/19/2106	Dan Maloney	Add new trigger DATASTORESETUPMAILTRG on DATASTORESETUP table to send emails when DATASTORESETUP.DATASTORE_TERMINATE 
--					chnges to a Y or N value
--	06/21/2016	Dan Maloney	Added logic to test is DataStoreLog is a pre V6SU11 version and if so drop the table and recreate it
--					If DataStoreLog is V6SU11+ or later, do not drop it and just skip th logic that creates the table
--	10/12/2016	Dan Maloney	Remove code to stop DataStore since this is now done in CMS SU13+
--	10/12/2016	Dan Maloney	Remove code to create DataStoreSetup and populate DataStoreSetUp since this is now done in CMS SU13+
--	10/12/2016	Dan Maloney	Remove code to create DataStoreEmailSetUp since this is now done in CMS SU13+
--	05/04/2017	Dan Maloney	Added comments before Oracle Email Trigger US (45098)
--	05/04/2017	Dan Maloney	Added v_DBName with the value of CON_NAME and added v_seesionUser with value of SESSION_USER to Oracle Email Trigger (US 45098)
--	05/05/2017	Dan Maloney	Moved DataStore OOTB Email Trigger to its own new script called CreateDataStoreEmailTrigger.sql (US 45098)
--	06/14/2017	Dan Maloney	Added DDL for DataStoreWhiteList table (US 51393)
--	06/14/2017	Dan Maloney	Remove code to populate InsiteSiteInfo  from DataStoreSetUp for DataStorepresent, DataStoreInsertTables and DataStoreDelimiter (US 51393)
--  10/23/2019  Dan maloney Modify logic to drop synonyms from ODS to OLTP and recreate them based on InsiteSiteInfo.DataStoreInsertTables (CPR 1260)
--  v8.4 (1)    Dan Maloney Replace DBMS_JOB with DBMS_SCHEDULER (US 11177).  The JOB column of the DATASTORELOG table is changed from NUMBER to VARCHAR2(128)
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------

DECLARE
  	I NUMBER;
  	n_TblNum 				NUMBER;
	n_InsertUpdateWaitTime	NUMBER;

	v_NumTables 			NUMBER;
	v_DataType				VARCHAR2(30);
	v_Sql 					VARCHAR2(512);
  	v_TblName 				VARCHAR2(50);
  	v_MastTblTrgName 		VARCHAR2(50);

	b_Exists				BOOLEAN;
	
   	Column_Exists 			EXCEPTION;
   	PRAGMA EXCEPTION_INIT(Column_Exists, -01430);

BEGIN
	DBMS_OUTPUT.ENABLE(NULL);

   	SELECT TO_NUMBER(TVALUE) 
   	INTO v_NumTables
	FROM INSITESITEINFO WHERE TNAME = 'DataStoreInsertTables';
      	IF (v_NumTables=0) THEN
      		RAISE_APPLICATION_ERROR(-20001, 'DataStoreInsertTables is "0", should be >0.');
   	END IF;
	
	--Drop existing queue table synonyms to the production database
	FOR cur_Syn IN (SELECT SYNONYM_NAME FROM USER_SYNONYMS WHERE SYNONYM_NAME LIKE '%DATASTORE%')
	LOOP	
		v_Sql := 'DROP SYNONYM ' || cur_Syn.SYNONYM_NAME;
		EXECUTE IMMEDIATE v_Sql;
	END LOOP;
	
   	-- Create the synonyms to the production database  
   	FOR n_TblNum IN 0..v_NumTables LOOP
      		IF (n_TblNum=0) THEN
         		v_TblName := 'DATASTOREUPDATES';
         		v_MastTblTrgName := 'DATASTOREUPDMTRG';  
      		ELSE
         		v_TblName := 'DATASTOREINSERTS' || n_TblNum;
         		v_MastTblTrgName := 'DATASTOREINS' || n_TblNum || 'MTRG';
      		END IF;
     
		-- Create detail table synonym to the production database 
		SELECT COUNT(*) 
		INTO I
		FROM USER_SYNONYMS 
		WHERE SYNONYM_NAME = 'REMOTE_' || v_TblName;
		IF (I=0) THEN
			v_Sql:='CREATE SYNONYM REMOTE_' || v_TblName || ' FOR ' || v_TblName || '@TO_INSITE_PRODUCTION';
			EXECUTE IMMEDIATE v_Sql;
		END IF;      
		  
		-- Create master (control) table synonym to the production database
		SELECT COUNT(*) 
		INTO I
		FROM USER_SYNONYMS WHERE SYNONYM_NAME = 'REMOTE_' || v_TblName || 'MAST';
		IF (I=0) THEN
			v_Sql:='CREATE SYNONYM REMOTE_' || v_TblName || 'MAST FOR ' || v_TblName || 'MASTER@TO_INSITE_PRODUCTION';
			EXECUTE IMMEDIATE v_Sql;
		END IF; 
		  
		--Create local queue tables (detail and master) on the ODS
		SELECT COUNT(*) 
		INTO I
		FROM USER_TABLES 
		WHERE TABLE_NAME = 'LOCAL_' || v_TblName || 'MAST';
		IF (I=0) THEN
			v_Sql := 'CREATE TABLE LOCAL_' || v_TblName || 'MAST (ID NUMBER, TXNID CHAR(16), TXNTYPE CHAR(1), STATUS CHAR(1), RECORDDATE DATE DEFAULT SYSDATE, SERVER VARCHAR2(50), CDOID NUMBER, ERROR VARCHAR2(1000) )';
			EXECUTE IMMEDIATE v_Sql;
			v_Sql := 'ALTER TABLE LOCAL_' || v_TblName || 'MAST ADD CONSTRAINT PK_LCLM' || v_TblName || ' PRIMARY KEY (ID)'; 
			EXECUTE IMMEDIATE v_Sql;
		ELSE
			v_Sql := 'ALTER TABLE LOCAL_' || v_TblName || 'MAST ADD (STATUS CHAR(1), RECORDDATE DATE DEFAULT SYSDATE, SERVER VARCHAR2(50), CDOID NUMBER, ERROR VARCHAR2(1000) )';
			BEGIN
				EXECUTE IMMEDIATE v_Sql;
			EXCEPTION 
			WHEN Column_Exists THEN
				DBMS_Output.Put_Line ('column exists');
			END;
		END IF; 
		  
		SELECT COUNT(*) 
		INTO I
		FROM USER_TABLES 
		WHERE TABLE_NAME = 'LOCAL_' || v_TblName;
		IF (I=0) THEN
			v_Sql := 'CREATE TABLE LOCAL_' || v_TblName || ' (TXNID CHAR(16), SEQUENCE NUMBER, TXNTYPE CHAR(1), SQLSTMT CLOB)';
			EXECUTE IMMEDIATE v_Sql;
			v_Sql := 'ALTER TABLE LOCAL_' || v_TblName || ' ADD CONSTRAINT PK_LCL' || v_TblName || ' PRIMARY KEY (TXNID,SEQUENCE)';
			EXECUTE IMMEDIATE v_Sql;
		END IF; 
		  
		-- Create the delete trigger
		SELECT COUNT(*) 
		INTO I
		FROM USER_TRIGGERS 
		WHERE TRIGGER_NAME = v_MastTblTrgName;
		IF (I>0) THEN
			v_Sql := 'DROP TRIGGER ' || v_MastTblTrgName;
			EXECUTE IMMEDIATE v_Sql;
		END IF;
		v_Sql := 'CREATE TRIGGER ' || v_MastTblTrgName || ' BEFORE DELETE ON LOCAL_' || v_TblName || 'MAST FOR EACH ROW ';
		v_Sql := v_Sql || ' BEGIN ';
		v_Sql := v_Sql || '    DELETE FROM LOCAL_' || v_TblName || ' WHERE TXNID=:OLD.TXNID; ';
		v_Sql := v_Sql || ' END; ';
		EXECUTE IMMEDIATE v_Sql;
   	END LOOP;   
   
   	-- DATASTOREINFO was a synonym previously. Drop it if it exists 
   	SELECT COUNT(*) 
	INTO I
	FROM USER_SYNONYMS 
	WHERE SYNONYM_NAME = 'DATASTOREINFO';
   	IF (I=1) THEN
      		v_Sql := 'DROP SYNONYM DATASTOREINFO';
      		EXECUTE IMMEDIATE v_Sql;      
      		-- If applying this script after a previous install copy records to DATASTORESETUP
      		v_Sql := 'INSERT INTO DATASTORESETUP (PARAMETER, VALUE, DESCRIPTION) SELECT TNAME,TVALUE,''Used internally. Do not modify.'' FROM INSITESITEINFO WHERE TNAME IN (''DataStoreInsertTables'',''DataStoreDelimiter'') '
		|| 'AND TNAME NOT IN (SELECT PARAMETER FROM DATASTORESETUP)';
      		EXECUTE IMMEDIATE v_Sql;
   	END IF;

	-- Create the LOG table
   	SELECT COUNT(*) 
	INTO I
	FROM USER_TABLES 
	WHERE TABLE_NAME = 'DATASTORELOG';
   	IF (I=1) THEN
		--DataStoreLog table exists.  Check to see if it is version V6SU11.  If it is V6SU11 or higher it will have a Job column.  
		--If it is before version V6SU11, drop table, then create newer version of the table
		SELECT COUNT(*)
		INTO I
		FROM USER_TAB_COLUMNS
		WHERE TABLE_NAME = 'DATASTORELOG' AND COLUMN_NAME = 'JOB';
		IF (I=0) THEN
			--Pre V6SU11 version of table, drop it
      		v_Sql := 'DROP TABLE DATASTORELOG CASCADE CONSTRAINTS';
      		EXECUTE IMMEDIATE v_Sql;
			b_Exists := FALSE;
		ELSE
			--V6SU11+ version, dont drop it
			b_Exists := TRUE;
			--US 11177, If JOB column is NUMBER, change it to VARCHAR2(128)
			SELECT DATA_TYPE INTO v_DataType FROM USER_TAB_COLUMNS WHERE TABLE_NAME='DATASTORELOG' AND COLUMN_NAME='JOB';
			IF v_DataType = 'NUMBER' THEN
				v_Sql := 'TRUNCATE TABLE DATASTORELOG';
				EXECUTE IMMEDIATE v_Sql;
				v_Sql := 'ALTER TABLE DATASTORELOG MODIFY JOB VARCHAR2(128)';
				EXECUTE IMMEDIATE v_Sql;
			END IF;
		END IF;
	ELSE
			b_Exists := FALSE;
   	END IF;
	IF b_Exists = FALSE THEN
		--Create V6SU11+ version if it doesnt exist
		v_Sql :='CREATE TABLE DATASTORELOG (JOB VARCHAR2(128) NOT NULL,';
		v_Sql := v_Sql || 'LOG_SEQ NUMBER	NOT NULL,';	
		v_Sql := v_Sql || 'LOG_TIMESTAMP TIMESTAMP(9) WITH LOCAL TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,';
		v_Sql := v_Sql || 'LOG_LEVEL NUMBER NOT NULL,';
		v_Sql := v_Sql || 'PACKAGE_EXECUTING VARCHAR2(128) NOT NULL,';
		v_Sql := v_Sql || 'LOC VARCHAR(64) NOT NULL,';
		v_Sql := v_Sql || 'MESSAGE VARCHAR2(4000) NOT NULL,';
		v_Sql := v_Sql || 'CONSTRAINT PK_DATASTORELOG PRIMARY KEY (JOB, LOG_SEQ, LOG_TIMESTAMP))';
		EXECUTE IMMEDIATE v_Sql;
	END IF;
  
   	-- Create the ERROR table
   	SELECT COUNT(*)
   	INTO I
   	FROM USER_TABLES
   	WHERE TABLE_NAME = 'DATASTOREERRORS';
  	IF (I=0) THEN
      		v_Sql := 'CREATE TABLE DATASTOREERRORS (SESSIONNAME VARCHAR2(25), TXNID CHAR(16), MESSAGE VARCHAR2(512), LOGDATE DATE DEFAULT SYSDATE, SQL VARCHAR2(4000))';
      		EXECUTE IMMEDIATE v_Sql;
   	END IF;
 
   	-- Create the DATASTORESESSIONTRACKING table
   	SELECT COUNT(*)
   	INTO I
   	FROM USER_TABLES
   	WHERE TABLE_NAME = 'DATASTORESESSIONTRACKING';
   	IF (I=0) THEN
      		v_Sql := 'CREATE TABLE DATASTORESESSIONTRACKING (SESSIONNAME VARCHAR2(25) NOT NULL, PROCESSEDTXNID CHAR(16), PROCESSEDID NUMBER, WAITINGID NUMBER, TIMESTAMP DATE, ';
		v_Sql := v_Sql || 'CONSTRAINT PK_DATASTORESESSIONTRACKING PRIMARY KEY (SESSIONNAME))';
      		EXECUTE IMMEDIATE v_Sql;
   	END IF;
   
   	-- Create the DATASTORESYNC table
   	SELECT COUNT(*)
   	INTO I
   	FROM USER_TABLES
   	WHERE TABLE_NAME = 'DATASTORESYNC';
   	IF (I=0) THEN
      		v_Sql := 'CREATE TABLE DATASTORESYNC (PROCESSEDTXNID CHAR(16) NOT NULL, CONSTRAINT PK_DATASTORESYNC PRIMARY KEY (PROCESSEDTXNID))';
      		EXECUTE IMMEDIATE v_Sql;
   	END IF;
   
   	-- Create the DATASTOREMISSINGTXNS table
   	SELECT COUNT(*)
   	INTO I
   	FROM USER_TABLES
   	WHERE TABLE_NAME = 'DATASTOREMISSINGTXNS';
   	IF (I=0) THEN
     	 	v_Sql := 'CREATE TABLE DATASTOREMISSINGTXNS (SESSIONNAME VARCHAR2(25), MISSEDID NUMBER, TYPE VARCHAR2(25), LOGDATE DATE DEFAULT SYSDATE)';
      		EXECUTE IMMEDIATE v_Sql;
      		v_Sql := 'CREATE INDEX DATASTOREMISSINGTXNS1 ON DATASTOREMISSINGTXNS (MISSEDID ASC) NOPARALLEL';
      		EXECUTE IMMEDIATE v_Sql;
  	END IF;

  	-- Create the DATASTOREWHITELIST table
   	SELECT COUNT(*)
   	INTO I
   	FROM USER_TABLES
   	WHERE TABLE_NAME = 'DATASTOREWHITELIST';
  	IF (I=0) THEN
      		v_Sql := 'CREATE TABLE DATASTOREWHITELIST (ERRORID NUMBER NOT NULL, DESCRIPTION VARCHAR2(512), CONSTRAINT PK_DATASTOREWHITELIST PRIMARY KEY (ERRORID))';
      		EXECUTE IMMEDIATE v_Sql;
   	END IF;
 
EXCEPTION
WHEN OTHERS THEN 
	ROLLBACK;
	RAISE;
END;
/

