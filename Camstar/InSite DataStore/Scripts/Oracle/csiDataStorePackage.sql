--------------------------------------------------------------------------------
-- SCRIPT: csiDataStorePackage.sql
-- DESCR: Updates the Datastore tables in the datastore database
-- HISTORY:
--     04/21/2016     Dan Maloney     New version of csiDataStorePackage.  See comments in package code 
--     04/21/2016     Dan Maloney     Streamlined code at start end end of script to stop the DataStore before the package is compiled
--                                    and start the DataStore at the end of the script 
--     03/23/2017     Alex Lind       remove validation for LOB data columns in manager procedure.
--     04/10/2017     Dan Maloney     Modifications to Verify_Host for US 1867 to better detect if the stored OLTP/ODS pair
--                                    metadata matches the executing environment for the OLTP/ODS pair.
--                                    A mismatch may result from restoring the ODS to another server, database, schema and
--                                    and failing to run a DB Update in CEP Management Studio prior to starting the ODS
--                                    Changed Verify_Host from function to procedure
--     12/06/2018     Dan Maloney     US 17617.  Simplify Veify Host logic and remove logic for checking for IP address
--     01/31/2019     Dan Maloney     Implemented work by Alex Lind in V6 for Bug 14453 ODS: flag STOP_IF_RETRIES_EXCEEDED = 'N' does not work correctly with UNCOMMITTED errors.
--                                    Changed Retries exceeded logic to handle Uncommitted and Missing IDs seperately. 
--     10/24/2019     Dan Maloney     Modified CLEANUP procedure to change c_tracking cursor to select only rows from DataStoreSessionTracking that have a matching
--                                    synonymn name for the SessionName (CPR 1260)
--     05/27/2020     Dan Maloney     PARSE_AND_EXECUTE : Removed logic log log a reveral message in DataStoreLog if the Camstar transactions contain reversals CPR 82735 (TFS05)
--                                    for v8.3 and CPR 82747 (TFS05) for V7 MU2.  These 2 CPRs have parent PR 26237 (TFS05) which has parent IR 94110101
--     v8.4 (1)       Dan Maloney     Replace DBMS_JOB with DBMS_SCHEDULER (US 11177)
--     v8.4 (3)       Dan Maloney     10/05/2020 replace DBMS_LOCK.SLEEP with DBMS_SESSION.SLEEP as DBMS_LOCK.SLEEP is depreciated.  US 109349
--	   01/29/2025	  Madhuri B		  Updated the job class name to be specific to the schema in use. 
--Copyright Siemens 2025  


CREATE OR REPLACE PACKAGE CSIDATASTOREPACKAGE
-- History:
--     04/05/2016     Dan Maloney      Added cVERSION constant
--
-- Copyright Siemens 2025  
--
AUTHID CURRENT_USER 
IS 

PROCEDURE REPLICATOR
(
	pv_TableName    		IN VARCHAR2, 
	pb_Debug        		IN BOOLEAN DEFAULT FALSE
); 

PROCEDURE MANAGER; 

PROCEDURE ENABLE
(
	pb_Enable       		IN BOOLEAN DEFAULT TRUE
); 
   
PROCEDURE CLEANUP; 
 
PROCEDURE COPY_MANAGER
(
	pTableName      		IN VARCHAR2
); 

FUNCTION IS_ERROR_WHITELISTED
(
	pn_SQLCODE      		IN NUMBER
)
RETURN BOOLEAN;

END CSIDATASTOREPACKAGE; -- Package spec
/









CREATE OR REPLACE PACKAGE BODY CSIDATASTOREPACKAGE IS     
--------------------------------------------------------------------------------
-- PACKAGE: csiDataStorePackage
--
-- History:
--    * Perf - Implement batching
--    * Make cBATCHDELSIZE a parameter
--    * Perf - Move the MarkTxnAsProcessed to a different thread to help perf.
--    * Create clean-up process for retained records (future)
--    * Change INSITESITEINFO to DATASTOREINFO (synonym) to match DB2
--    * Change DATASTORESETUP. Add Description column. (DB2)
--    * Change 'Enable' setup name to DATASTORE_TERMINATE (DB2)
--    * Change name of DATASTOREERROR table, and columns to match DB2
--    * Create and use DATASTORELOG table from DB2 (create LOG_MESSAGE function)
--    * Use (new) INSERT_UPDATE_WAIT_TIME parameter instead of hard-coded "4"
--    * Account for keeping buffer records after execution
--    * For updates, lock if there are any INSERTS with lower TxnIds
--    * Change name of synonym to REMOTE_XXX to match DB2
--    * Perf - Only INIT during sleep, otherwise just get DATASTORE_TERMINATE
--    * Perf - Implement DATASTORESESSIONTRACKING architecture
--    * Added ID column support
--    * Removed many string literals
--    * Added support for master/detail architecture
--    * Added check for out of sequence Ids
--    * Added DATASTOREMISSINGTXNS and "rows affected" support
--    * Added LOCAL mode support
--    * Added support for DATASTORESYNC
--    * Added handler for DROP TABLE error on DBReset
--    * Added STOP_IF_RETRIES_EXCEEDED parameter
--    * Updated/added copyright notices (SPR S9984) 12/06/2006 Bill Lippard
--    * Updated copyright notices (SPR S9984) 04/23/2007 Bill Lippard
--
--     04/05/2016     Dan Maloney     Organized package global variables by type and then alphabetically ordered
--     04/05/2016     Dan Maloney     Changed package global variable gTableType VARCHAR2(10) to gTableType VARCHAR2(16)
--     04/05/2016     Dan Maloney     Changed package global variable gTableName VARCHAR2(50) to gTableName VARCHAR2(64)
--     04/05/2016     Dan Maloney     Changed package global variable gDelimiter VARCHAR2(10) to gDelimiter VARCHAR2(16)
--     04/05/2016     Dan Maloney     Changed default value of TRUE to FALSE for package global variable gTerminate
--     04/05/2016     Dan Maloney     Changed default value of TRUE to FALSE for package global variable gKeepRecords
--     04/05/2016     Dan Maloney     Changed package global variable gLastProcessedTxn VARCHAR2(20) to  gLastProcessedTxn VARCHAR2(16)
--     04/05/2016     Dan Maloney     Added package global type typ_AssocArray_JobInfo IS TABLE OF VARCHAR2(4000) INDEX BY BINARY_INTEGER
--     04/05/2016     Dan Maloney     Added package global variable tab_JobInfo to track job information from USER_JOBS for the datastore jobs
--                                    CLEANUP, COPY_MANAGER, REPLICATOR and COPY_MANAGER
--     04/05/2016     Dan Maloney     Added default value of 2 to package global variable gNumInsertTables
--     04/05/2016     Dan Maloney     Added package global variable gVerifyHost BOOLEAN := TRUE
--     04/05/2016     Dan Maloney     Added package global variable gLogLevel PLS_INTEGER := 0
--     04/05/2016     Dan Maloney     Added package global variable gLogRetention PLS_INTEGER := 180
--     04/05/2016     Dan Maloney     Added package global variable gCopyManagerBatchSize NUMBER := 1000
--     04/05/2016     Dan Maloney     Added package global variable gLogSeq NUMBER := 0
--     04/05/2016     Dan Maloney     Added package global variable gJobId VARCHAR2(64)
--     04/05/2016     Dan Maloney     Added package global variable gSID VARCHAR2(16)
--     04/05/2016     Dan Maloney     Added package global constant cLogLevelError CONSTANT PLS_INTEGER := 0
--     04/05/2016     Dan Maloney     Added package global constant cLogLevelMax CONSTANT PLS_INTEGER := 2
--     04/05/2016     Dan Maloney     Added package global constant cLogLevelMin CONSTANT PLS_INTEGER := 1
--     04/05/2016     Dan Maloney     Added package global constant cPARM_LOG_LEVEL CONSTANT VARCHAR2(9) := 'LOG_LEVEL'
--     04/05/2016     Dan Maloney     Added package global constant cPARM_LOG_RETENTION CONSTANT VARCHAR2(13) := 'LOG_RETENTION'
--     04/05/2016     Dan Maloney     Added package global constant cSTATUSROLLBACK CONSTANT VARCHAR2(1) := 'R'	
--     04/05/2016     Dan Maloney     Added package global constant cSTATUSUNCOMMITTED ONSTANT VARCHAR2(1) := 'U'
--     04/05/2016     Dan Maloney     Added package global constant cSTATUSPROCESS ONSTANT VARCHAR2(1) := 'P'
--     04/05/2016     Dan Maloney     Added package global constant cPARM_VERIFY_HOST CONSTANT VARCHAR2(13) := 'VERIFY_HOST'
--     04/05/2016     Dan Maloney     Removed package global constant cMISSED_NO_ROWS CONSTANT VARCHAR2(25):='NO ROWS AFFECTED'
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE VERIFY_JOB
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE INIT 
--     04/05/2016     Dan Maloney     Renamed IN OUT and OUT parameter names in PROCEDURE GET_NEXT_TXNID 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in FUNCTION IS_LOCKED 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE PARSE_AND_EXECUTE_TXN 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE SET_LAST_PROCESSED 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in FUNCTION ARE_INSERTS_PENDING 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE INSERT_MISSED_TXNS
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE OutputDebug 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE LOG_ERROR 
--     04/05/2016     Dan Maloney     Renamed IN parameter names in PROCEDURE LOG_MESSAGE and added new IN parameters
--     04/19/2016     Dan Maloney     Add IN parameter in PROCEDURE SET_LAST_PROCESSED pv_Status VARCHAR2
--     06/30/2016     Dan Maloney     Added package global variable gVersion VARCHAR2(10)
--     06/30/2016     Dan Maloney     Added package global variable gDataStorePresent CHAR(1)
--     06/30/2016     Dan Maloney     Added package global constant cPARM_VERIFY_HOST	CONSTANT VARCHAR2(13)
--     06/30/2016     Dan Maloney     Added package global constant cPARM_DATASTOREPRESENT VARCHAR2(13)
--     06/14/2017     Dan Maloney     Preston merged millisecond change back to DEV branch to add gTimeStampFormat
--     06/14/2017     Dan Maloney     Preston merged millisecond change back to DEV branch to add ALTER SESSION SET NLS_TIMESTAMP_FORMAT to INIT
--     06/14/2017     Dan Maloney     Added log messages to exception handler to log rollback in CLEANUP, MANAGER, REPLICATOR, INIT (US 51393)
--     06/14/2017     Dan Maloney     Modified INIT to move ALTER SESSION statements to package initialization (US 51393)
--     06/14/2017     Dan Maloney     Removed stopping of ODS from INIT so calling procedure or package initialization is responsible for stopping ODS (US 51393)
--     06/14/2017     Dan Maloney     Added exception handler to CHECK_TERMINATE (US 51393)
--     06/14/2017     Dan Maloney     Added new function IS_ERROR_WHITELISTED (US 51393)
--     06/14/2017     Dan Maloney     Modified package initialization to add exception handler (US 51393)
--     06/14/2017     Dan Maloney     Changed hard-coded 0 value in call to LOG_MESSAGE in VERIFY_HOST to the constant cLogLevelError (US51393)
--     06/15/2017     Dan Maloney     Added constant cLogLevelWhiteList := -1 (US 51393)
--     06/18/2017     Dan Maloney     Modified exception handler in all procedures and functions to call IS_ERROR_WHITELISTED and modified calls to LOG_MESSAGE 
--                                    to pass cLogLevelWhiteList if IS_ERROR_WHITELISTED returns TRUE
--     06/21/2017     Dan Maloney     Remove excpetion eDupInsert (US 51393)
--     04/10/2017     Dan Maloney     Modifications to Verify_Host for US 1867 to better detect if the stored OLTP/ODS pair
--                                    metadata matches the executing environment for the OLTP/ODS pair.
--                                    A mismatch may result from restoring the ODS to another server, database, schema and
--                                    and failing to run a DB Update in CEP Management Studio prior to starting the ODS
--                                    Changed Verify_Host from function to procedure
--
--     07/17/2018     Dan Maloney     Modified REPLICATOR to add SESSIONNAME to where clause for DELETE FROM DATASTOREMISSINGTXNS statement for CPR 8538 
--     08/28/2018     Dan Maloney     Modified REPLICATOR call to SET_LAST_PROCESSED to pass NULL for v_Status parameter in the <IF gSTOP_IF_RETRIES_EXCEEDED = FALSE  THEN> block 
--                                    and pass n_NextId-1 for pn_Id parameter for CPR 9767 
--     10/24/2019     Dan Maloney     Modified CLEANUP procedure to change c_tracking cursor to select only rows from DataStoreSessionTracking that have a matching
--                                    synonymn name for the SessionName (CPR 1260)
--
-- Copyright Siemens 2025
--
TYPE typ_Cursor IS REF CURSOR; 
TYPE typ_AssocArray_JobInfo IS TABLE OF VARCHAR2(4000) INDEX BY VARCHAR2(128); 
tab_JobInfo typ_AssocArray_JobInfo;  
  
--Exceptions
eNoRowsOnUpdate             			EXCEPTION; 
eVerifyHostFailure						EXCEPTION;   

-- Global variables 
gKeepRecords                			BOOLEAN := FALSE;   
gTerminate                  			BOOLEAN := FALSE;    
gSTOP_IF_RETRIES_EXCEEDED   	 		BOOLEAN := TRUE;    
gSTOP_ON_DUPLICATE_INSERT   			BOOLEAN := TRUE;    
gSTOP_ON_NO_UPDATE          			BOOLEAN := TRUE; 
gVerifyHost                 			BOOLEAN := TRUE; 
gLogLevel                   			PLS_INTEGER := 0;  
gLogRetention               			PLS_INTEGER := 180;  
gBatchSize                  			NUMBER := 50; 
gCleanupBatchSize           			NUMBER := 5000;  
gCopyManagerBatchSize       			NUMBER := 1000;    
gLastProcessedId            			NUMBER := 0;  
gLogSeq                     			NUMBER := 0; 
gMissTxnMax                 			NUMBER := 1;    
gNumInsertTables            			NUMBER := 2;  
gSleepTime                  			NUMBER := 4;   
gDataStorePresent           			CHAR(1); 
gAccessMode                 			VARCHAR2(20) := 'LOCAL'; 
gDateFormat                 			VARCHAR2(32) :='YYYY-MM-DD HH24:MI:SS';    -- configurable format for package to use in alter stmt.
gTimeStampFormat            			VARCHAR(32) := 'YYYY-MM-DD HH24:MI:SSXFF'; -- configurable format for package to use in alter stmt.
gDelimiter                  			VARCHAR2(16) := ';$$$;';   
gJobId                      			VARCHAR2(128);
gLastProcessedTxn           			VARCHAR2(16) :='0000000000000000';  
gSID                        			VARCHAR2(16);  
gTableName                  			VARCHAR2(64);      
gTableType                  			VARCHAR2(16); -- 'INSERT' or 'UPDATE'
gVersion                    			VARCHAR2(10); 
  
-- Constants
cLogLevelWhiteListed        			CONSTANT PLS_INTEGER  := -1;
cLogLevelError              			CONSTANT PLS_INTEGER  := 0; 
cLogLevelMax                			CONSTANT PLS_INTEGER  := 2; 
cLogLevelMin                			CONSTANT PLS_INTEGER  := 1; 
cUPDATES                    			CONSTANT VARCHAR2(7)  := 'UPDATES';     
cINSERTS                    			CONSTANT VARCHAR2(7)  := 'INSERTS';     
cMANAGER                    			CONSTANT VARCHAR2(10) := 'MANAGER';     
cREPLICATOR                 			CONSTANT VARCHAR2(10) := 'REPLICATOR';     
cCLEANUP                    			CONSTANT VARCHAR2(25) := 'CLEANUP';     
cCOPYMGR                    			CONSTANT VARCHAR2(20) := 'COPY_MANAGER';     
cDATASTOREUPDATES           			CONSTANT VARCHAR2(20) := 'DATASTOREUPDATES';     
cDATASTOREINSERTS           			CONSTANT VARCHAR2(20) := 'DATASTOREINSERTS';     
cPACKAGENAME                			CONSTANT VARCHAR2(20) := 'csiDataStorePackage';   
cPARM_ACCESSMODE            			CONSTANT VARCHAR2(20) := 'ACCESS_MODE'; 
cPARM_INSUPDSIZE            			CONSTANT VARCHAR2(50) := 'INSERT_UPDATE_BATCH_SIZE';  
cPARM_INSERTTABLES          			CONSTANT VARCHAR2(25) := 'DATASTOREINSERTTABLES';     
cPARM_DELIMITER             			CONSTANT VARCHAR2(25) := 'DATASTOREDELIMITER';     
cPARM_TERMINATE             			CONSTANT VARCHAR2(20) := 'DATASTORE_TERMINATE';     
cPARM_INSUPDWAIT            			CONSTANT VARCHAR2(30) := 'INSERT_UPDATE_WAIT_TIME';     
cPARM_KEEPRECORDS           			CONSTANT VARCHAR2(30) := 'KEEP_REMOTE_RECORDS';     
cPARM_CLEANUPSIZE           			CONSTANT VARCHAR2(30) := 'CLEANUP_BATCH_SIZE';     
cPARM_MISSTXNMAX            			CONSTANT VARCHAR2(50) := 'MISSING_TXN_RETRIES';     
cPARM_RETRYSTOP             			CONSTANT VARCHAR2(50) := 'STOP_IF_RETRIES_EXCEEDED';     
cPARM_DUPSTOP               			CONSTANT VARCHAR2(50) := 'STOP_ON_DUPLICATE_INSERT';     
cPARM_NOUPDSTOP             			CONSTANT VARCHAR2(50) := 'STOP_ON_NO_UPDATE';   
cPARM_LOG_LEVEL             			CONSTANT VARCHAR2(9)  := 'LOG_LEVEL';    
cPARM_LOG_RETENTION         			CONSTANT VARCHAR2(13) := 'LOG_RETENTION';    
cPARM_VERIFY_HOST           			CONSTANT VARCHAR2(13) := 'VERIFY_HOST';   
cPARM_VERSION               			CONSTANT VARCHAR2(13) := 'VERSION';   
cPARM_DATASTOREPRESENT      			CONSTANT VARCHAR2(25) := 'DATASTOREPRESENT';   
cMISSED_MISSING             			CONSTANT VARCHAR2(15) := 'MISSING';  
cMISSED_UNCOMMITTED         			CONSTANT VARCHAR2(15) := 'UNCOMMITTED';     
cZEROID                     			CONSTANT VARCHAR2(16) := '0000000000000000';     
cTXNTYPE                    			CONSTANT VARCHAR2(1)  := 'T';     
cDUMMYTYPE                  			CONSTANT VARCHAR2(1)  := 'D';     
cYES                        			CONSTANT VARCHAR2(1)  := 'Y';     
cNO                         			CONSTANT VARCHAR2(1)  := 'N';     
cUPDATE                     			CONSTANT VARCHAR2(1)  := 'U';  
cSTATUSROLLBACK             			CONSTANT VARCHAR2(1)  := 'R'; 
cSTATUSUNCOMMITTED          			CONSTANT VARCHAR2(1)  := 'U'; 
cSTATUSPROCESS              			CONSTANT VARCHAR2(1)  := 'P'; 
cMODELOCAL                  			CONSTANT VARCHAR2(15) := 'LOCAL'; 
cMODEREMOTE                 			CONSTANT VARCHAR2(15) := 'REMOTE'; 
 
-- Local forward declarations
PROCEDURE VERIFY_HOST;

PROCEDURE VERIFY_JOB
(
	pv_ProcName         	IN VARCHAR2, 
	pv_TableName        	IN VARCHAR2
);    

PROCEDURE INIT 
(
	pv_TableName        	IN VARCHAR2
);    

PROCEDURE GET_NEXT_TXNID 
(
	pcur_QueueTable     	IN OUT typ_Cursor, 
	pv_TxnId            	OUT VARCHAR2, 
	pv_TxnType          	OUT VARCHAR2, 
	pv_Status           	OUT VARCHAR2, 
	pn_NextId           	OUT NUMBER,    
	pn_CDOID            	OUT NUMBER, 
	pv_Msg              	OUT VARCHAR2,
	pv_SourceQueue      	OUT VARCHAR2 
); 

FUNCTION IS_LOCKED 
(
	pv_TxnId            	IN VARCHAR2, 
	pv_TxnType          	IN VARCHAR2, 
	pn_Id               	IN NUMBER
) 
RETURN BOOLEAN;

PROCEDURE PARSE_AND_EXECUTE_TXN 
(
	pv_TxnId        	IN VARCHAR2, 
	pv_TxnType      	IN VARCHAR2
);    
  
PROCEDURE SET_LAST_PROCESSED 
(
	pv_TxnId        	IN VARCHAR2, 
	pn_Id           	IN NUMBER, 
	pv_TxnType      	IN VARCHAR2,
	pv_Status       	IN VARCHAR2
);  

FUNCTION ARE_INSERTS_PENDING 
(
	pv_TxnId        	IN VARCHAR2
)
RETURN BOOLEAN;
 
PROCEDURE INSERT_MISSED_TXNS 
(
	pv_Type         	IN VARCHAR2, 
	pn_StartId      	IN NUMBER, 
	pn_LastId       	IN NUMBER DEFAULT 0
);  

PROCEDURE LOG_ERROR 
(
	pv_TxnId        	IN VARCHAR2, 
	pv_SQLStmt      	IN VARCHAR2,
	pv_Err          	IN VARCHAR2
);
    
PROCEDURE LOG_MESSAGE 
(
	pv_Msg          	IN VARCHAR2, 
	pv_Loc          	IN VARCHAR2 DEFAULT 'Unassigned', 
	pi_LogLevel     	IN PLS_INTEGER DEFAULT 0
); 
 
FUNCTION CHECK_TERMINATE 
RETURN BOOLEAN;  
 
    
--------------------------------------------------------------------
-- Name:        CLEANUP
-- Params:      None
--
-- Descr:       Deletes processed records.  Executes on the ODS
--
-- History:
--              06/19/2006                       Made this procedure an infinite loop so the cleanup process could keep up in a high-volume environment.
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed local variable v_err VARCHAR2(512) to v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(2000) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_delcnt NUMBER to n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable n_CLeanUpSeq NUMBER := 1
--              04/05/2016      Dan Maloney      Added local variable n_RowCount NUMBER
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Removed local variable v_ErrLoc VARCHAR2(100)
--              06/18/2017      Dan Maloney      Added b_WhiteListed variable (US 51393)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              10/24/2019      Dan Maloney      Modified CLEANUP procedure to change c_tracking cursor to select only rows from DataStoreSessionTracking that have a matching
--                                               synonymn name for the SessionName (CPR 1260)
--              v8.4 (3)        Dan Maloney      10/05/2020 replace DBMS_LOCK.SLEEP with DBMS_SESSION.SLEEP as DBMS_LOCK.SLEEP is depreciated.  US 109349
--
--
-- Copyright Siemens 2025  
--
PROCEDURE CLEANUP 
IS  
	b_WhiteListed           		BOOLEAN := FALSE;
	n_CleanUpSeq            		NUMBER := 1;
	n_RowCount              		NUMBER;
	n_RowsAffected          		NUMBER := 0; 
	v_Err                   		VARCHAR2(4000);    
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_SQL                   		VARCHAR2(4000); 

	CURSOR c_Tracking IS    
	SELECT SESSIONNAME,PROCESSEDID    
	FROM DATASTORESESSIONTRACKING
    WHERE SESSIONNAME IN (SELECT TABLE_NAME FROM USER_SYNONYMS WHERE SYNONYM_NAME LIKE 'REMOTE_DATASTORE%');   
BEGIN    
	v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';
	v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] Begin';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	v_Loc := 'CLEANUP';

	v_Msg := 'INIT(' || cCLEANUP || ')';
	INIT(cCLEANUP);  
	
	v_Msg := 'Check to see if DataStore is enabled';
	IF gTerminate THEN
		v_Msg := '*** DATASTORE STOPPED ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';	
		v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := 'CLEANUP';
		RETURN; 
	ELSE
		v_Msg := 'DataStore running'; 
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
		
		-- Call Verify Host
		v_Msg := 'Call VERIFY_HOST';
		VERIFY_HOST;
	END IF;

	--Stop DataStore if KEEP_REMOTE_RECORDS is set to Y, this is a depreciated parameter
	IF gKeepRecords THEN 
		v_Msg := '*** KEEP_REMOTE_RECORDS = ' || '''' || 'Y' || '''' || ' ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';	
		v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := 'CLEANUP';
		RETURN;     
	END IF; 
 
	v_Msg := 'WHILE gTerminate = FALSE LOOP';
	WHILE (gTerminate = FALSE) 
	LOOP 
		IF n_CleanUpSeq > 1 THEN
			v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';
			v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] Begin.  DATASTORESETUP.INSERT_UPDATE_WAIT_TIME [ ' || gSleepTime || ' ] seconds reached.';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
			v_Loc := 'CLEANUP';
		END IF;
		
		n_RowsAffected:=0;   
		v_Msg := 'FOR LOOP open cursor';
		FOR CREC IN c_Tracking 
		LOOP   
			v_Msg:='DELETING FROM LOCAL_' || crec.SESSIONNAME || 'MAST queue table';  
   
			/*********************************************************************************************************************************************************************************/
			v_SQL:='DELETE FROM ' || gAccessMode || '_' || crec.SESSIONNAME || 'MAST WHERE ID <= :ID';    
			v_SQL:=v_SQL || ' AND ID NOT IN (SELECT MISSEDID FROM DATASTOREMISSINGTXNS WHERE SESSIONNAME=''' || crec.SESSIONNAME || ''')';    
			v_SQL:=v_SQL || ' AND ROWNUM <= ' || gCleanupBatchSize; 
			/*********************************************************************************************************************************************************************************/   

			v_Msg := v_SQL;
     
			EXECUTE IMMEDIATE v_SQL USING crec.PROCESSEDID;  
			n_RowCount := SQL%ROWCOUNT;
			n_RowsAffected := n_RowsAffected + n_RowCount;

			v_Msg := 'Rows affected [ ' ||  n_RowCount || ' ] ' || REPLACE(v_SQL,':ID', '[ ' || crec.PROCESSEDID || ' ]');
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			IF n_RowCOunt > 0 THEN    
				COMMIT;
				v_Msg := 'COMMIT : Issued after DELETE FROM LOCAL_' || crec.SESSIONNAME || 'MAST';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
			END IF;
		END LOOP;    
 
		v_Msg := 'Evaluate if any rows were deleted';
		IF (n_RowsAffected = 0) THEN 
			-- No rows where processed, so it's okay to sleep.
			DBMS_SESSION.SLEEP(gSleepTime);  
		ELSE 
			v_Msg := 'Total rows affected [ ' ||  n_RowsAffected || ' ] from LOCAL_DATASTORE%MAST queue tables ';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		END IF;    

		v_Msg := 'INIT(' || cCLEANUP || ')';    
		INIT(cCLEANUP);

		IF NOT gTerminate THEN   
			n_CleanUpSeq := n_CleanUpSeq +1;
		END IF;
	END LOOP; 

	v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';
	v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] End';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

	v_Loc := 'CLEANUP'; 
EXCEPTION   
WHEN eVerifyHostFailure  THEN

	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, cLogLevelError);
	
	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Exception : eVerifyHostFailure';
	LOG_MESSAGE(v_Err,v_Loc, cLogLevelError);

	--Disable DataStore
	v_Msg := 'ENABLE(FALSE)';
	ENABLE(FALSE);
	
	v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';	
	v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
WHEN OTHERS THEN 
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	v_Err := CASE b_WhiteListed WHEN TRUE THEN 'WhiteListed : ' || tab_JobInfo(gJobId) || ' : ' || SQLERRM ELSE  tab_JobInfo(gJobId) || ' : ' || SQLERRM END;
	LOG_ERROR( 'N/A', 'N/A', v_Err ); 

	IF (NOT b_WhiteListed) THEN
		--Disable DataStore
		ENABLE(FALSE);  
		
		v_Loc := '[ ' || n_CleanUpSeq || ' ] CLEANUP';	
		v_Msg := 'Iteration [ ' || n_CleanUpSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	END IF;  
END;



---------------------------------------------------------------------
-- Name:        COPY_MANAGER
-- Params:      None
-- Descr:       Copies transactions from OLTP to ODS database.  
--              loop until gTerminate = TRUE (i.e. DATASTORESETUP.DATASTORE_TERMINATE = 'Y')
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_CUR tCursor to v_Cur typ_Cursor
--              04/05/2016      Dan Maloney      Changed local variable v_CurSQL VARCHAR2(255) to v_CurSQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_MastSQL VARCHAR2(255) to v_MastSQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_DtlSQL VARCHAR2(255) to v_DtlSQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_DelSQL VARCHAR2(255) to v_DelSQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_id NUMBER to n_id NUMBER
--              04/05/2016      Dan Maloney      Changed local variable v_Err VARCHAR2(512) to v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable b_DMLSubmitted BOOLEAN := FALSE
--              04/05/2016      Dan Maloney      Added local variable n_CopyManagerSeq NUMBER := 1
--              04/05/2016      Dan Maloney      Added local variable n_CommitFullBatchCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_CommitPartialBatchCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_DelMastCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_FetchCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_InsDtlCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_InsMastCnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Removed local variable v_ErrLoc VARCHAR2(255)
--              04/05/2016      Dan Maloney      Removed local variable v_Cnt NUMBER
--              06/18/2017      Dan Maloney      Added b_WhiteListed variable (US 51393
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              v8.4 (3)        Dan Maloney      10/05/2020 replace DBMS_LOCK.SLEEP with DBMS_SESSION.SLEEP as DBMS_LOCK.SLEEP is depreciated.  US 109349
--
--
-- Copyright Siemens 2025  
--
PROCEDURE COPY_MANAGER
(
		pTableName              		IN VARCHAR2
) 
IS   
	v_Cur                   		typ_Cursor; 
	b_DMLSubmitted          		BOOLEAN := FALSE;
	b_WhiteListed           		BOOLEAN := FALSE;
	n_CommitFullBatchCnt    		NUMBER;
	n_CommitPartialBatchCnt 		NUMBER;
	n_CopyManagerSeq        		NUMBER := 1;
	n_DelMastCnt            		NUMBER; 
	n_FetchCnt              		NUMBER; 
	n_Id                    		NUMBER;    
	n_InsDtlCnt             		NUMBER;
	n_InsMastCnt            		NUMBER;
	v_CurSQL                		VARCHAR2(4000);  
	v_DelSQL                		VARCHAR2(4000);  
	v_DtlSQL                		VARCHAR2(4000);  
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_MastSQL               		VARCHAR2(4000);    
	v_Msg                   		VARCHAR2(4000); 
	v_TxnId                 		VARCHAR(16); 
BEGIN  
	v_Loc := '[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';  
	v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] Begin';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	v_Loc := 'COPY_MANAGER';

	v_Msg := 'INIT(' || cCOPYMGR || ')';
	INIT(cCOPYMGR);
	gTableName := pTableName;
	
	v_Msg := 'Check to see if DataStore is enabled';
	IF gTerminate THEN
		v_Msg := '*** DATASTORE STOPPED ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := '[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';	
		v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := 'CLEANUP';
		RETURN; 
	ELSE
		v_Msg := 'DataStore running';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
		
		-- Call Verify Host
		v_Msg := 'Call VERIFY_HOST';
		VERIFY_HOST;	
	END IF;

	n_FetchCnt := 0;
	v_Msg := 'WHILE (gTerminate=FALSE) LOOP';  
	WHILE (gTerminate = FALSE AND gAccessMode = cMODELOCAL) 
	LOOP 
		IF n_CopyManagerSeq > 1 THEN
			v_Loc:='[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';
			v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] Begin.  DATASTORESETUP.INSERT_UPDATE_WAIT_TIME [ ' || gSleepTime || ' ] seconds reached.';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
			v_Loc := 'COPY_MANAGER';
		END IF;
		
		v_CurSQL:='SELECT ID,TXNID FROM REMOTE_' || pTableName || 'MAST WHERE STATUS in ( ' || CHR(39) || cSTATUSPROCESS || CHR(39) || ',' || CHR(39) || cSTATUSROLLBACK || CHR(39) || ')  ORDER BY ID';    --JAL added where status check

		v_MastSQL:='INSERT INTO LOCAL_' || pTableName || 'MAST(ID,TXNID,TXNTYPE,STATUS,SERVER,CDOID,ERROR ) SELECT ID,TXNID,TXNTYPE,STATUS,SERVER,CDOID,ERROR FROM REMOTE_' 
		|| pTableName || 'MAST WHERE ID = :ID'; 

		v_DtlSQL:='INSERT INTO LOCAL_' || pTableName || '(TXNID,TXNTYPE,SEQUENCE,SQLSTMT) SELECT TXNID,TXNTYPE,SEQUENCE,SQLSTMT FROM REMOTE_' || pTableName 
		|| ' WHERE TXNID = :TXNID'; 

		v_DelSQL:='DELETE FROM REMOTE_' || pTableName || 'MAST WHERE ID = :ID';    

		n_FetchCnt:=0; 
		n_CommitFullBatchCnt := 0;
		n_CommitPartialBatchCnt := 0;
		n_InsMastCnt :=0;
		n_InsDtlCnt :=0;
		n_DelMastCnt :=0; 
		b_DMLSubmitted := FALSE;

		v_Msg := 'Open cursor';
		OPEN v_Cur FOR v_CurSQL; 
		v_Msg := 'Fetch row from cursor';   
		FETCH v_Cur INTO n_Id,v_TxnId;  
		WHILE (v_CUR%FOUND) 
		LOOP  
			BEGIN 
				v_Msg := 'Insert into LOCAL_' || pTableName || 'MAST from REMOTE_' || pTableName || 'MAST WHERE ID = ' || n_Id;    
				EXECUTE IMMEDIATE v_MastSQL USING n_Id;
				n_InsMastCnt := n_InsMastCnt + SQL%ROWCOUNT;
				b_DMLSubmitted := TRUE;
			EXCEPTION    
			WHEN DUP_VAL_ON_INDEX THEN 
				--Warning messages not errors.  Processing continues 
 				v_Err:='Exception Handler (DUP_VAL_ON_INDEX) Warning: ' || SQLERRM; 
				LOG_MESSAGE(v_Err, v_Loc, cLogLevelMin);  
				--
				v_Err:='Exception Handler (DUP_VAL_ON_INDEX) Warning: Dup Val on Index for SQL [ ' || REPLACE(v_MastSQL,':ID',n_Id) || ' ]';  
				LOG_MESSAGE(v_Err, v_Loc, cLogLevelMin);  
			END;    

			BEGIN   
				v_Msg := 'Insert into LOCAL_' || pTableName || ' from REMOTE_' || pTablename || ' WHERE TXNID = ' || v_TxnId;    
				EXECUTE IMMEDIATE v_DtlSQL USING v_TxnId; 
				n_InsDtlCnt := n_InsDtlCnt + SQL%ROWCOUNT;
				b_DMLSubmitted := TRUE;
			EXCEPTION    
			WHEN DUP_VAL_ON_INDEX THEN 
				--Warning messages not errors.  Processing continues   
				v_Err:='Exception Handler (DUP_VAL_ON_INDEX) Warning: ' || SQLERRM; 
				LOG_MESSAGE(v_Err, v_Loc, cLogLevelMin);  
				--
				v_Err:='Exception Handler (DUP_VAL_ON_INDEX) Warning: Dup Val on Index for SQL [ ' || REPLACE(v_DtlSQL,':TXNID',v_TxnId) || ' ]';  
				LOG_MESSAGE(v_Err, v_Loc, cLogLevelMin);  
			END;    

			v_Msg := 'Removing record ID = ' || n_Id; 
			IF gKeepRecords = FALSE THEN					--JAL Added NOT to enable keep remote functionality
				v_Msg := 'Delete records from REMOTE_' || pTableName || 'MAST from REMOTE_' || pTablename || ' WHERE ID = ' || n_Id;  
				EXECUTE IMMEDIATE v_DelSQL USING n_Id;  
				n_DelMastCnt := n_DelMastCnt + SQL%ROWCOUNT;
				b_DMLSubmitted := TRUE;
			END IF;    
   
			n_FetchCnt := n_FetchCnt + 1;  

			IF MOD(n_FetchCnt, gCopyManagerBatchSize) = 0 THEN  
				IF b_DMLSubmitted THEN
					COMMIT;    
					n_CommitFullBatchCnt := n_CommitFullBatchCnt +1;
					b_DMLSubmitted := FALSE;
					v_Msg := 'COMMIT : gCopyManagerBatchSize [ ' || gCopyManagerBatchSize || ' ] reached.';
					LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
				END IF;

				v_Msg := 'gTerminate := CHECK_TERMINATE';  
				gTerminate := CHECK_TERMINATE; 
			END IF;    

			v_Msg := 'Fetch row from cursor'; 
			FETCH v_Cur INTO n_Id,v_TxnId;   
		END LOOP;  --WHILE (v_CUR%FOUND) 

		v_Msg := 'Close REMOTE_' || pTableName || 'MAST cursor'; 
		CLOSE v_Cur; 

		IF b_DMLSubmitted THEN
			COMMIT; 
			n_CommitPartialBatchCnt := n_CommitPartialBatchCnt +1;
			b_DMLSubmitted := FALSE;
		END IF;

		v_Msg := 'Check if fetch count not equal to zero';
		IF n_FetchCnt != 0 THEN
			v_Msg := 'Rows fetched [ ' || n_FetchCnt  || ' ] from REMOTE_' || pTableName || 'MAST';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			v_Msg := 'Rows affected [ ' || n_InsMastCnt  || ' ] Inserted into LOCAL_' || pTableName || 'MAST from REMOTE_' || pTableName || 'MAST';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			v_Msg := 'Rows affected [ ' || n_InsDtlCnt  || ' ] Inserted into LOCAL_' || pTableName || ' from REMOTE_' || pTableName;
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			v_Msg := 'Rows affected [ ' || n_DelMastCnt  || ' ] Deleted from REMOTE_' || pTableName || 'MAST';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			v_Msg := 'COMMIT : Full Batches [ ' || n_CommitFullBatchCnt || ' ] Partial Batches [ ' || n_CommitPartialBatchCnt || ' ]';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		END IF;

		v_Msg := 'Sleep ' || gSleepTime || ' seconds';    
		DBMS_SESSION.SLEEP(gSleepTime);    

		v_Msg := 'INIT(' || cCOPYMGR || ')';   
		INIT(cCOPYMGR); 
		gTableName := pTableName;

		IF NOT gTerminate THEN   
			n_CopyManagerSeq := n_CopyManagerSeq +1;
		END IF;
 
		gTableName := pTableName;
	END LOOP; -- WHILE (gTerminate=FALSE AND gAccessMode=cMODELOCAL) 

	v_Loc:='[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';
	v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

	v_Loc := 'COPY_MANAGER';
EXCEPTION  
WHEN eVerifyHostFailure  THEN

	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, cLogLevelError);
	
	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Exception : eVerifyHostFailure';
	LOG_MESSAGE(v_Err,v_Loc, cLogLevelError);

	--Disable DataStore
	v_Msg := 'ENABLE(FALSE)';
	ENABLE(FALSE); 

	v_Loc := '[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';	
	v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);	
WHEN OTHERS THEN    
	ROLLBACK; 

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	v_Err := CASE b_WhiteListed WHEN TRUE THEN 'WhiteListed : ' || tab_JobInfo(gJobId) || ' : ' || SQLERRM ELSE  tab_JobInfo(gJobId) || ' : ' || SQLERRM END;
	LOG_ERROR( 'N/A', 'N/A', v_Err ); 

	IF v_Cur%ISOPEN THEN
		CLOSE v_Cur;
		LOG_MESSAGE('Exception Handler (procedure when others) : Close REMOTE_' || pTableName || 'MAST cursor', v_Loc, cLogLevelMax);
	END IF;

	IF (NOT b_WhiteListed) THEN
		--Disable DataStore
		ENABLE(FALSE);  
		
		v_Loc := '[ ' || n_CopyManagerSeq || ' ] COPY_MANAGER';	
		v_Msg := 'Iteration [ ' || n_CopyManagerSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	END IF;  
END;  

  

---------------------------------------------------------------------
-- Name:        MANAGER
-- Params:      None
-- Descr:       Stored procedure to manager jobs.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(2000) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_err VARCHAR2(512) to v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable d_DelBeforeDate DATE
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected PLS_INTEGER := 0
--              04/05/2016      Dan Maloney      Added local variable n_ManagerSeq NUMBER
--              04/05/2016      Dan Maloney      Removed local variable v_ErrLoc VARCHAR2(100)
--              03/03/2017      Dan Maloney      Modified logic to check for unique indexes that used to be in REPLICATOR to check for unique indexes on
--                                               non-primary key columns for Camstar tables marked to propagate
--              03/03/2017      Dan Maloney      Added logic to check for Camstar tables with BLOB and CLOB columns that are marked for propagation
--              03/23/2017      Dan Maloney      Removed logic to check for Camstar tables with BLOB and CLOB columns that are marked for propagation
--              06/18/2017      Dan Maloney      Added b_WhiteListed variable (US 51393
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              08/28/2020      Dan Maloney      Added purging of scheduler log tables (US 11177)
--
--
-- Copyright Siemens 2025  
--
PROCEDURE MANAGER 
IS    
	b_WhiteListed           		BOOLEAN := FALSE;
	d_DelBeforeDate         		DATE;
	n_CntIdx                		NUMBER := 0;
	n_ManagerSeq            		NUMBER := 1;
	n_RowsAffected          		PLS_INTEGER := 0;
	v_Err                   		VARCHAR2(4000);    
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);     
	v_SQL                   		VARCHAR2(4000);
	
	CURSOR cSchedulerJobs IS
   	SELECT Job_Name AS JOB_NAME
   	FROM USER_SCHEDULER_JOBS
  	WHERE JOB_ACTION LIKE '%csiDataStorePackage%';
	
BEGIN   
	v_Loc:='[ ' || n_ManagerSeq || ' ] MANAGER';
	v_Msg := 'Iteration [ ' || n_ManagerSeq || ' ] Begin';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	v_Loc := 'MANAGER';

	v_Msg := 'INIT(' || cMANAGER || ')';    
	INIT(cMANAGER); 
	
	v_Msg := 'Check to see if Datastore is enabled'; 
	IF gTerminate THEN
		v_Msg := '*** DATASTORE STOPPED ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc:='[ ' || n_ManagerSeq || ' ] MANAGER';	
		v_Msg := 'Iteration [ ' || n_ManagerSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := 'MANAGER';
		RETURN; 
	ELSE
		v_Msg := 'DataStore running';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
		
		-- Call Verify Host
		v_Msg := 'Call VERIFY_HOST';
		VERIFY_HOST;	
	END IF;
		
	IF gTerminate = FALSE THEN
		-- Validation: indexes must be non-unique, except for primary key indexes and lob indexes for tables in DbTableDefinition that are set to propagate
		SELECT COUNT(*) INTO n_CntIdx
		FROM
			(
				SELECT I.TABLE_NAME, I.INDEX_NAME
				FROM USER_INDEXES I INNER JOIN DBTABLEDEFINITION D
				ON  I.TABLE_NAME = UPPER(D.DBTABLENAME)
				WHERE I.UNIQUENESS = 'UNIQUE' AND INDEX_TYPE != 'LOB' AND D.PROPAGATE = 1 
				MINUS
				SELECT C.TABLE_NAME, C.INDEX_NAME
				FROM USER_CONSTRAINTS C  
				WHERE C.CONSTRAINT_TYPE = 'P' 
			);
			
		IF (n_CntIdx > 0) THEN  
			v_Msg := 'Unexpected unique indexes found on ODS.  Cannot continue.';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);   
			RAISE_APPLICATION_ERROR(-20001, v_Msg);    
		END IF;   

		--Check Jobs
		--Replicator Jobs
		v_Msg:='Call VERIFY_JOB(' || cREPLICATOR || ',' || cDATASTOREUPDATES || ')';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
		VERIFY_JOB(cREPLICATOR,cDATASTOREUPDATES); 

		FOR tNum IN 1..gNumInsertTables 
		LOOP
			v_Msg:='Call VERIFY_JOB(' || cREPLICATOR || ',' || cDATASTOREINSERTS || tNum || ')';  
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
			VERIFY_JOB(cREPLICATOR,cDATASTOREINSERTS||tNum);    
		END LOOP;  

		--Cleanup Job
		v_Msg:='Call VERIFY_JOB(' || cCLEANUP || ',NULL)'; 
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);   
		VERIFY_JOB(cCLEANUP,NULL);    

		/*********************************************************************************************************************************************************************************/
		--Copy_Manager Jobs
		IF (gAccessMode=cMODELOCAL) THEN  
			v_Msg:=' Call VERIFY_JOB(' || cCOPYMGR || ',' || cDATASTOREUPDATES || ')'; 
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
			VERIFY_JOB(cCOPYMGR,cDATASTOREUPDATES);    

			FOR tNum IN 1..gNumInsertTables 
			LOOP    
				v_Msg:='Call VERIFY_JOB(' || cCOPYMGR || ',' || cDATASTOREINSERTS || tNum || ')'; 
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
				VERIFY_JOB(cCOPYMGR,cDATASTOREINSERTS||tNum);  
			END LOOP;  
		END IF;
		/*********************************************************************************************************************************************************************************/
 
		--DATASTORELOG management
		IF gLogRetention  != 0 THEN
			d_DelBeforeDate := TRUNC(SYSDATE) - gLogRetention;
			n_RowsAffected := 0;
			v_SQL := 'DELETE FROM DATASTORELOG WHERE LOG_TIMESTAMP < :DelBeforeDate';
			EXECUTE IMMEDIATE v_SQL USING d_DelBeforeDate;

			n_RowsAffected := SQL%ROWCOUNT;

			v_Msg := 'Rows affected [ ' || n_RowsAffected  || ' ] Deleted from DATASTORELOG.  ' || REPLACE(v_SQL, ':DelBeforeDate', '[ ' || d_DelBeforeDate || ' ].');
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			IF n_RowsAffected > 0 THEN
				COMMIT;
				v_Msg := 'COMMIT : Issued after ' || REPLACE(v_SQL, ':DelBeforeDate', '[ ' || d_DelBeforeDate || ' ]');
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
			END IF;

			n_RowsAffected := 0;
			v_SQL := 'DELETE FROM DATASTOREERRORS WHERE LOGDATE < :DelBeforeDate';
			EXECUTE IMMEDIATE v_SQL USING d_DelBeforeDate;

			n_RowsAffected := SQL%ROWCOUNT;

			v_Msg := 'Rows affected [ ' || n_RowsAffected  || ' ] Deleted from DATASTOREERRORS.  ' || REPLACE(v_SQL, ':DelBeforeDate', '[ ' || d_DelBeforeDate || ' ].');
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			IF n_RowsAffected > 0 THEN
				COMMIT;
				v_Msg := 'COMMIT : Issued after ' || REPLACE(v_SQL, ':DelBeforeDate', '[ ' || d_DelBeforeDate || ' ]');
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
			END IF;
			
			--Purge scheduler log tables
			BEGIN
			   	FOR crec IN cSchedulerJobs LOOP
					DBMS_SCHEDULER.purge_log (log_history => gLogRetention, job_name => crec.JOB_NAME);
				END LOOP;
			END;
			
			v_Msg := 'Records older than [ ' || gLogRetention  || ' ] purged from user_scheduler_job_log and user_scheduler_job_run_details';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			
		END IF;
	END IF;

	v_Loc:='[ ' || n_ManagerSeq || ' ] MANAGER';
	v_Msg := 'Iteration [ ' || n_ManagerSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

	v_Loc := 'MANAGER';
EXCEPTION   
WHEN eVerifyHostFailure  THEN

	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, cLogLevelError);
	
	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Exception : eVerifyHostFailure';
	LOG_MESSAGE(v_Err,v_Loc, cLogLevelError);

	--Disable DataStore
	v_Msg := 'ENABLE(FALSE)';
	ENABLE(FALSE);
	
	v_Loc:='[ ' || n_ManagerSeq || ' ] MANAGER';	
	v_Msg := 'Iteration [ ' || n_ManagerSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
WHEN OTHERS THEN 
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	v_Err := CASE b_WhiteListed WHEN TRUE THEN 'WhiteListed : ' || tab_JobInfo(gJobId) || ' : ' || SQLERRM ELSE  tab_JobInfo(gJobId) || ' : ' || SQLERRM END;
	LOG_ERROR ('N/A', 'N/A', v_Err ); 

	IF (NOT b_WhiteListed) THEN
		--Disable DataStore
		ENABLE(FALSE);  
		
		v_Loc:='[ ' || n_ManagerSeq || ' ] MANAGER';	
		v_Msg := 'Iteration [ ' || n_ManagerSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	END IF;  
END;  




---------------------------------------------------------------------
-- Name:        REPLICATOR
-- Params:      <in> pv_TableName      (DATASTOREUPDATES, DATASTOREINSERTS1,2,3,4...)
--              <in> pb_Debug           Used for testing and debugging
--
-- Descr:       Main worker of data store. Executes SQL from production database on the DataStore
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_CUR tCursor to v_QueueTable typ_Cursor
--              04/05/2016      Dan Maloney      Changed local variable v_TxnId VARCHAR2(100) to v_TxnId VARCHAR2(16)
--              04/05/2016      Dan Maloney      Changed local variable v_NextId NUMBER to n_NextId NUMBER
--              04/05/2016      Dan Maloney      Changed local variable v_TxnType VARCHAR2(10) to v_TxnType VARCHAR2(16)
--              04/05/2016      Dan Maloney      Changed local variable v_err VARCHAR2(512) to v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_CommitCnt NUMBER:=0 to n_TransactionCommitCnt NUMBER := 0
--              04/05/2016      Dan Maloney      Changed local variable v_MisTxnCnt NUMBER:=0 to _MissTxnCnt NUMBER := 0
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(512) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_CntIdx NUMBER:=0 to n_CntIdx NUMBER := 0
--              04/05/2016      Dan Maloney      Changed local variable v_CDOID NUMBER:=0 to n_CDOID NUMBER := 0
--              04/05/2016      Dan Maloney      Changed local variable n_ReplicatorSeq NUMBER := 1
--              04/05/2016      Dan Maloney      Changed local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable n_TransactionCnt NUMBER := 0;
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable b_InsertsPending BOOLEAN
--              04/05/2016      Dan Maloney      Removed local variable v_ErrLoc VARCHAR2(100)
--              05/25/2016      Dan Maloney      Changed log level of out of sync messages from level 0 to level 1 for warnings before max retries is reached
--              05/25/2016      Dan Maloney      Added logic to ignore first 3 detections of an out of sync condition so messages are not logged until the 4th detection
--              01/03/2017      Dan Maloney      Moved logic to check for unique indexes on non primary key columns for Camstar tables marked to propagate from REPLICATOR to MANAGER
--              06/18/2017      Dan Maloney      Added b_WhiteListed variable (US 51393)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              06/21/2017      Dan Maloney      Changed inserts into DATASTOREMISSINGTXNS to calls to INSERT_MISSED_TXNS
--              10/16/2017      Dan Maloney      Added logic to check (IF gSTOP_IF_RETRIES_EXCEEDED  = 'N') logic (US 57436)
--              10/16/2017      Dan Maloney      Added "DELETE FROM DataStoreMissingTxns..." logic after call to PARSE_AND_EXECUTE_TXN and 
--                                               removed the "DELETE FROM DataStoreMissingTxns..." logic from SET_LAST_PROCESSED  (US 57436)
--              07/17/2018      Dan Maloney      Modified REPLICATOR to add SESSIONNAME to where clause for DELETE FROM DATASTOREMISSINGTXNS statement for CPR 8538 
--              08/28/2018      Dan Maloney      Modified REPLICATOR call to SET_LAST_PROCESSED to pass NULL for v_Status parameter in the <IF gSTOP_IF_RETRIES_EXCEEDED = FALSE  THEN> block 
--                                               and pass n_NextId-1 for pn_Id parameter for CPR 9767 
--              01/31/2019      Dan Maloney      Implemented work by Alex Lind in V6 for Bug 14453 ODS: flag STOP_IF_RETRIES_EXCEEDED = 'N' does not work correctly with UNCOMMITTED errors.
--                                               Changed Retries exceeded logic to handle Uncommitted and Missing IDs seperately. 
--              08/22/2019      Dan maloney      Moved prcoessing for ORA-08176 from REPLICATOR to to GET_NEXT_TXNID for Bug 39355
--              v8.4 (3)        Dan Maloney      10/05/2020 replace DBMS_LOCK.SLEEP with DBMS_SESSION.SLEEP as DBMS_LOCK.SLEEP is depreciated.  US 109349
--
--
-- Copyright Siemens 2025  
--
PROCEDURE REPLICATOR
(
	pv_TableName            		IN VARCHAR2, 
	pb_Debug                		IN BOOLEAN DEFAULT FALSE
) 
IS    
	cur_QueueTable          		typ_Cursor; 

	b_InsertsPending        		BOOLEAN;
	b_Slept                 	 	BOOLEAN := FALSE;
	b_WhiteListed           		BOOLEAN := FALSE;
	d_CurrentDate           		DATE;
	n_CDOID                 		NUMBER := 0;  
	n_CntIdx                		NUMBER := 0;
	n_MissTxnCnt            		NUMBER := 0; 
	n_NextId                	   	NUMBER; 
	n_ReplicatorSeq         		NUMBER := 1;
	n_RowsAffected          		NUMBER := 0;
	v_Err                   	  	VARCHAR2(4000); 
	v_Loc                   	 	VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_SourceQueue           		VARCHAR2(16);   
	v_SQL                   		VARCHAR2(4000);     
	v_Status                		VARCHAR2(1); 
	n_TransactionCnt        		NUMBER := 0;
	v_TxnId                 		VARCHAR2(16);    
	v_TxnType               		VARCHAR2(16);
BEGIN    
	v_Loc := '[ ' || n_ReplicatorSeq || ' ] REPLICATOR';
	v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] Begin';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	v_Loc := 'REPLICATOR';

	-- Call Init
	v_Msg:='INIT(' || pv_TableName || ')';
	INIT(pv_TableName);   
	
	v_Msg := 'Check to see if Datastore is enabled'; 
	IF gTerminate THEN
		v_Msg := '*** DATASTORE STOPPED ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc:='[ ' || n_ReplicatorSeq || ' ] REPLICATOR';	
		v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);

		v_Loc := 'MANAGER';
		RETURN; 
	ELSE
		v_Msg := 'DataStore running';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
		
		-- Call Verify Host
		v_Msg := 'Call VERIFY_HOST';
		VERIFY_HOST;
	END IF;
			

	-- >> START CODE WHILE DATASTORE IS RUNNING
	WHILE (gTerminate = FALSE)  -- (0)
	LOOP    
		IF n_ReplicatorSeq > 1 THEN
			v_Loc := '[ ' || n_ReplicatorSeq || ' ] REPLICATOR';
			v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] Begin.  ';
			IF b_Slept THEN
				v_Msg := v_Msg || 'DATASTORESETUP.INSERT_UPDATE_WAIT_TIME [ ' || gSleepTime || ' ] seconds reached.';
			END IF;
			b_Slept := FALSE;
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
			v_Loc := 'REPLICATOR';
		END IF;
		
		-- Get next TxnId for this replication session
		v_Msg := 'GET_NEXT_TXNID(cur_QueueTable' || ',' || v_TxnId || ',' || v_TxnType || ',' || v_Status || ',' || n_NextId || ',' || n_CDOID || ',' || NVL(v_Err,' ') || ',' || v_SourceQueue || ')';
		
		GET_NEXT_TXNID(cur_QueueTable, v_TxnId, v_TxnType, v_Status , n_NextId, n_CDOID, v_Err, v_SourceQueue);    

		--Decrease sensitivity for out of sequence so first 3 detections of an out of sync condition wont be acted upon (i.e. wont be logged or counted against retry count)
		--3 iterations sleep for the amount of seconds defined by MISSING_TXN_RETRIES (which defaults to 2), which equates to 6 seconds
		--Once the missed txn count reaches 0, messages begin logging to the DataStoreLog table 
		n_MissTxnCnt := -3;

		-- >> START OUT OF SEQUENCE PROCESSING 
		-- While loop executes if out of sequence
		WHILE ( ( (gLastProcessedId > 0 AND n_NextId > 0 AND n_NextId > gLastProcessedId + 1) OR (v_Status = cSTATUSUNCOMMITTED) ) AND (n_MissTxnCnt < gMissTxnMax) )  
		LOOP   
			-- Log message, sleep and retry
			v_Msg := 'Out of sequence while loop'; 
			n_MissTxnCnt := n_MissTxnCnt + 1;    
      
			IF (cur_QueueTable%ISOPEN) THEN  
				v_Msg:='Close queue table cursor';  
				CLOSE cur_QueueTable; 
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
			END IF; 

			IF (n_MissTxnCnt = gMissTxnMax) THEN 
				v_Msg := 'OUT OF SEQUENCE ERROR : missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax || ' ] : Transaction Status {v_Status} [ ' || v_status  
				|| ' ] Transaction {v_TxnId} [ ' || v_TxnId || ' ] Next Transaction ID {n_NextId} [ ' || n_NextId || ' ] > Last Processed Transaction {gLastProcessedId} [ ' 
				|| TO_CHAR(gLastProcessedId) || ' ].  No Retries left';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);
				
				IF (v_Status= cSTATUSUNCOMMITTED) THEN  
				
					-- Uncommitted transaction
					v_Msg := 'INSERT_MISSED_TXNS';
					INSERT_MISSED_TXNS(cMISSED_UNCOMMITTED,n_NextId); 
					
					-- Check to Stop DataStore.						
					IF(gSTOP_IF_RETRIES_EXCEEDED = FALSE) THEN  
						v_Msg := 'Uncommitted Retries exceeded, STOP_IF_RETRIES_EXCEEDED = FALSE.  DataStore Continuing.';  
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);
						SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);  
					ELSE --(gSTOP_IF_RETRIES_EXCEEDED = FALSE )
						ENABLE(FALSE); 
					END IF; --(gSTOP_IF_RETRIES_EXCEEDED = FALSE )
					
				ELSE --(v_Status= cSTATUSUNCOMMITTED)
				
					-- Missed transaction
					IF (gLastProcessedId > 0) THEN 
						v_Msg := 'INSERT_MISSED_TXNS';
						INSERT_MISSED_TXNS(cMISSED_MISSING, gLastProcessedId +1, n_NextId -1);
						
						-- Check to Stop DataStore.
						IF (gSTOP_IF_RETRIES_EXCEEDED = FALSE) THEN
							v_Msg := 'Missing Retries exceeded, STOP_IF_RETRIES_EXCEEDED = FALSE.  Datastore Continuing.'; 
							LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError); 
							SET_LAST_PROCESSED(v_TxnId,n_NextId-1,v_TxnType, cSTATUSROLLBACK); -- Note override of status, this is to bypass the sync logic
						ELSE --(gSTOP_IF_RETRIES_EXCEEDED = FALSE )
							v_Msg := 'ENABLE(FALSE)';
							ENABLE(FALSE);
						END IF; --(gSTOP_IF_RETRIES_EXCEEDED = FALSE )
						
					END IF; --(gLastProcessedId > 0) 
					
				END IF; --(V_Status= cSTATUSUNCOMMITTED)

				COMMIT;
				v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ].   Issued after out of sync condition could not be resolved before MISSING_TXN_RETRIES [ ' || gMissTxnMax || ' ] reached.';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin); 
				n_TransactionCnt := 0;
			ELSE --(n_MissTxnCnt = gMissTxnMax)
				IF n_MissTxnCnt > 0 THEN
					v_Msg := 'OUT OF SEQUENCE WARNING: missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax || ' ] : Transaction Status {v_Status} [ ' || v_status  
					|| ' ] Transaction {v_TxnId} [ ' || v_TxnId || ' ] Next Transaction ID {n_NextId} [ ' || n_NextId || ' ] > Last Processed Transaction {gLastProcessedId} [ ' 
					|| TO_CHAR(gLastProcessedId) || ' ]';
					LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);

					IF (v_Status = cSTATUSUNCOMMITTED) THEN
						v_Msg := 'OUT OF SEQUENCE WARNING: missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax || ' ] : Waiting for Transaction {v_TxnId} [ ' 
						|| v_TxnId || ' ] Next Transaction ID {n_NextId} [ ' || n_NextId || ' ] to commit or roll back on transaction database';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
					ELSE
						v_Msg := 'OUT OF SEQUENCE WARNING: missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax || ' ] : Missing Transaction between ' 
						|| 'Next Transaction ID {n_NextId} [ ' || n_NextId || ' ] and Last Processed Transaction {gLastProcessedId} [ ' || TO_CHAR(gLastProcessedId) || ' ]';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
					END IF;
					
					v_Msg := 'OUT OF SEQUENCE WARNING: missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax || ' ] : Sleep [ ' || gSleepTime || ' ] seconds';
					LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);  

					v_Msg := 'OUT OF SEQUENCE WARNING : missed txn count|max missed txn count [ ' || n_MissTxnCnt || '|' || gMissTxnMax 
					|| ' ] : Reopen queue table cursor after sleep to fetch records to process with updated status values';
					LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);  
				END IF; -- IF @i_MissTxnCnt > 0

				DBMS_SESSION.SLEEP(gSleepTime); 
				b_Slept := TRUE;
				
				-- Call Init
				v_Msg:='INIT(' || pv_TableName || ')';
				INIT(pv_TableName);   

				--Open cursor again and fetch row
				v_Msg := 'GET_NEXT_TXNID(cur_QueueTable' || ',' || v_TxnId || ',' || v_TxnType || ',' || v_Status || ',' || n_NextId || ',' || n_CDOID || ',' || NVL(v_Err,' ') || ',' || v_SourceQueue || ')';
				GET_NEXT_TXNID(cur_QueueTable,v_TxnId, v_TxnType, v_Status, n_NextId, n_CDOID, v_Err, v_SourceQueue);  

			END IF;  -- IF (n_MissTxnCnt = gMissTxnMax)
		END LOOP;   -- WHILE out of sequence 
		-- >> END OUT OF SEQUENCE PROCESSING 

		-- >> START TRANSACTION PROCESSING
		IF (gTerminate = FALSE) THEN -- (1a)
			IF (v_Status = cSTATUSROLLBACK) THEN -- (2a)
				--The Camstar Transaction status is rollback 
				--In this case, SET_LAST_PROCESSED is called to advance the Camstar Transaction to indicate we processed the rollback transaction on the ODS
				--No SQL is actually processed on the ODS.  

				v_Msg := 'Rolled back transaction detected.  Skip processing on ODS for Transaction {v_TxnId} [ ' || v_TxnId || ' ] CDOID {n_CDOID} [ ' || n_CDOID || ' ]';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

				v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_txnType || ',' || v_Status || ')';
				SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);
			ELSIF (v_Status = cSTATUSPROCESS) THEN  -- (2b)
				--The Camstar Transaction status is process/proceed to be processed by replicator 
				--If this Camstar Transaction is not a dummy transaction, the SQL making up the Camstar Transaction is parsed and executed on the ODS
				--Camstar Transactions are committed one at a time if gBatchSize = 0  or as a batch if gBatchSize > 0
				--SET_LAST_PROCESSED is called to advance the Camstar Transaction to indicate we processed if no inserts are pending.  If inserts are pending, go to else section of this if-then-else block
				-- An insert is pending if there is not a row for a Camstar Transaction in DataStoreSync.  If there is a row in DataStoreSync, that means the insert is complete and the update for the
				-- corresponding Camstar Transaction can continue to process
				BEGIN  -- (3a)  
					v_Msg := 'b_InsertsPending := IS_LOCKED(' || v_TxnId || ',' || v_TxnType || ',' || n_NextId || ')';
					b_InsertsPending := IS_LOCKED(v_TxnId, v_TxnType, n_NextId);
					IF (b_InsertsPending = FALSE) THEN -- (4a)
						SAVEPOINT CURRENT_CAMSTAR_TRANSACTION;
						v_Msg := 'PARSE_AND_EXECUTE(' || v_TxnId || ',' || v_TxnType || ')';
						PARSE_AND_EXECUTE_TXN(v_TxnId,v_TxnType); 
						
						--Advance to next Camstar Transaction
						v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_TxnType || ',' || v_Status || ')';
						SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);

						DELETE FROM DATASTOREMISSINGTXNS WHERE SESSIONNAME = pv_TableName AND MISSEDID = n_NextId;
						n_RowsAffected := SQL%ROWCOUNT;  

						v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] DELETE FROM DATASTOREMISSINGTXNS WHERE SESSIONNAME = [ ' 
						|| pv_TableName || ' ] AND MISSEDID = [ ' || n_NextId || ' ]';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

						n_TransactionCnt := n_TransactionCnt +1;

						v_Msg := 'Commit Logic - Each Camstar Transaction or in Batches of Camstar Transactions';
						IF (gBatchSize = 0) THEN -- (5a)
						-- Commit work per Camstar Transaction
							IF (n_TransactionCnt > 0) THEN -- (6a)
								COMMIT;
								v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ].  Transaction [ ' ||  v_TxnId || ' ].';
								LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
  
								n_TransactionCnt := 0; 

								/*****************************************************************************************************************************/
								IF (gAccessMode=cMODEREMOTE) THEN 
									v_SQL := 'DELETE FROM LOCAL_'||gTableName;    
									EXECUTE IMMEDIATE v_SQL;    
								END IF; 
								/*****************************************************************************************************************************/
							END IF;	-- (6a)
						ELSE -- (5b)
						-- Commit work by batch defined by gBatchSize
							IF (n_TransactionCnt >= gBatchSize) THEN -- (6a)
								IF (cur_QueueTable%ISOPEN) THEN   -- (7a) 
									v_Msg:='Close queue table cursor.  gBatchSize [ ' || gBatchSize || ' ] reached.';
									CLOSE cur_QueueTable;  
									LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);     
								END IF; -- (7a)

								--All Camstar Transactions processed on the ODS are committed.  A new batch is started
								COMMIT; 

								v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ].  gBatchSize [ ' || gBatchSize || ' ] reached.';
								LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 

								n_TransactionCnt := 0;  

								/*****************************************************************************************************************************/
								IF (gAccessMode=cMODEREMOTE) THEN 
									v_SQL := 'DELETE FROM LOCAL_'||gTableName;    
									EXECUTE IMMEDIATE v_SQL;    
								END IF; 
								/*****************************************************************************************************************************/
							ELSE -- (6b)
								-- gBatchSize not reached.  Just log the transaction count
								v_Msg := 'n_TransactionCnt [ ' || n_TransactionCnt || ' ].  gBatchSize [ ' || gBatchSize || ' ].';
								LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
							END IF; -- (6a, 6b)
						END IF; -- (5a, 5b)
					ELSE -- (4b)
						--Inserts are pending.  Do not advance Camstar Transaction
						--Close the queue table cursor so it is reopened next iteration, commit any pending work, a sleep is performed and INIT is called

						COMMIT; 

						v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ]';
						IF gBatchSize = 0 THEN
							v_Msg := v_Msg || '.  Transaction [ ' ||  v_TxnId || ' ].';
						END IF;
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 

						n_TransactionCnt := 0;

						IF (cur_QueueTable%ISOPEN) THEN  -- (5a) 
							v_Msg:='Close queue table cursor';
							CLOSE cur_QueueTable; 
							LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  
						END IF; -- (5a)

						d_CurrentDate := SYSDATE;

						UPDATE DATASTORESESSIONTRACKING    
						SET TIMESTAMP = d_CurrentDate    
						WHERE SESSIONNAME = pv_TableName; 

						n_RowsAffected := SQL%ROWCOUNT;   

						v_Msg := 'Rows affected [ ' || n_RowsAffected  || ' ] UPDATE DATASTORESESSIONTRACKING SET TIMESTAMP = [ ' || TO_CHAR(d_CurrentDate, 'YYYY-MM-DD HH24:MI:SS.SSSSS')
						|| ' ] WHERE SESSIONNAME = [ ' || pv_TableName || ' ]'; 
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

						v_Msg := 'Sleep [ ' || gSleepTime || ' ] seconds';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);   
						DBMS_SESSION.SLEEP(gSleepTime); 
						b_Slept := TRUE;
						


						v_Msg:='INIT(' || pv_TableName || ')';    
						INIT(pv_TableName);
					END IF; --(4a, 4b)
				EXCEPTION -- (3a)   
				WHEN eNoRowsOnUpdate THEN  
					ROLLBACK TO CURRENT_CAMSTAR_TRANSACTION;

					IF (gSTOP_ON_NO_UPDATE = TRUE) THEN
						LOG_MESSAGE('Exception Handler (procedure when eNoRowsOnUpdate) : STOP_ON_NO_UPDATE = "Y"',v_Loc, cLogLevelError);

						--Stop DataStore
						v_Msg := 'ENABLE(FALSE)';
						ENABLE(FALSE);  
					ELSE  
						--Log the missed Camstar Transaction and continue processing in while loop until gTerminate = TRUE
						v_Msg := 'INSERT_MISSED_TXNS';
						INSERT_MISSED_TXNS('NO UPD : ' || v_TxnId,n_NextId); 

						v_Msg := 'Exception Handler (procedure when eNoRowsOnUpdate) : Row to update not found.  STOP_ON_NO_UPDATE = "N", skipping txn.  Txn SQL will be saved in buffer table for review.';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelWhiteListed);

						v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_TxnType || ',' || v_Status || ')';
						SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);  
					END IF;  
				WHEN DUP_VAL_ON_INDEX  THEN  
					--DUP_VAL_ON_INDEX is SQLCODE of -1
					ROLLBACK TO CURRENT_CAMSTAR_TRANSACTION; 
					b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

					IF (b_WhiteListed) THEN
						--SQLCODE of -1 is WhiteListed.  WhiteListing overrides setting for STOP_ON_DUPLICATE_INSERT and the SQLCODE exception
						IF (gSTOP_ON_DUPLICATE_INSERT = TRUE) THEN
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : ' || SQLERRM, v_Loc, cLogLevelWhiteListed); 
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : White List overrides SQLCODE [ ' || SQLCODE || ' ].  '
							|| 'White List overrides STOP_ON_DUPLICATE_INSERT = "Y".  '
							|| 'Skipping Camstar Transaction.',v_Loc, cLogLevelWhiteListed);
						ELSE
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : ' || SQLERRM, v_Loc, cLogLevelWhiteListed); 
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : White List overrides SQLCODE [ ' || SQLCODE || ' ].  '
							|| 'White List concurs with STOP_ON_DUPLICATE_INSERT = "N".  ' 
							|| 'Skipping Camstar Transaction.',v_Loc, cLogLevelWhiteListed);
						END IF;
    
						--Log the missed Camstar Transaction and continue processing in while loop until gTerminate = TRUE  
						v_Msg := 'INSERT_MISSED_TXNS';
						INSERT_MISSED_TXNS('DUPVAL : ' || v_TxnId,n_NextId); 

						v_Msg := 'Duplicate entry encountered.  STOP_ON_DUPLICATE_INSERT = "N", skipping txn.  Txn SQL will be saved in buffer table for review.';
						LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);

						v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_TxnType || ',' || v_Status || ')';
						SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);  
					ELSE
						--SQLCODE of -1 is not WhiteListed 
						IF (gSTOP_ON_DUPLICATE_INSERT = TRUE) THEN
							--STOP_ON_DUPLICATE_INSERT is True and SQLCODE of -1 is not WhiteListed, So stop the ODS
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : ' || SQLERRM, v_Loc, cLogLevelError);
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : STOP_ON_DUPLICATE_INSERT = "Y"',v_Loc, cLogLevelError);

							--Stop DataStore 
							v_Msg := 'ENABLE(FALSE)';
							ENABLE(FALSE);
						ELSE
							--SQLCODE of -1 is not WhiteListed and STOP_ON_DUPLICATE_INSERT is FALSE

							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : ' || SQLERRM, v_Loc, cLogLevelError);
							LOG_MESSAGE('Exception Handler (procedure when DUP_VAL_ON_INDEX) : STOP_ON_DUPLICATE_INSERT = "N", skipping txn.  Txn SQL will be saved in buffer table for review.',v_Loc, cLogLevelError);

							--Log the missed Camstar Transaction and continue processing in while loop until gTerminate = TRUE 
							v_Msg := 'INSERT_MISSED_TXN'; 
							INSERT_MISSED_TXNS('DUPVAL : ' || v_TxnId,n_NextId); 

							v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_TxnType || ',' || v_Status || ')';
							SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);  
						END IF;
					END IF;  
				WHEN OTHERS THEN 
					ROLLBACK TO CURRENT_CAMSTAR_TRANSACTION; 
					b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);
					IF (b_WhiteListed) THEN 
						--SQLCODE of -1 is WhiteListed.  WhiteListing overrides violation

						LOG_MESSAGE('Exception Handler (procedure when others) : ' || SQLERRM, v_Loc, cLogLevelWhiteListed);
						LOG_MESSAGE('Exception Handler (procedure when others) : White List overrides SQLCODE [ ' || SQLCODE || ' ].  Skipping Camstar Transaction.',v_Loc, cLogLevelWhiteListed);

						--Log the missed Camstar Transaction and continue processing in while loop until gTerminate = TRUE
						v_Msg := 'INSERT_MISSED_TXNS';
						INSERT_MISSED_TXNS('ERROR : ' || v_TxnId,n_NextId); 

						v_Msg := 'SET_LAST_PROCESSED(' || v_TxnId || ',' || n_NextId || ',' || v_TxnType || ',' || v_Status || ')';
						SET_LAST_PROCESSED(v_TxnId,n_NextId,v_TxnType, v_Status);  
					ELSE  
						LOG_MESSAGE('Exception Handler (procedure when others) : ' || SQLERRM, v_Loc, cLogLevelError);

						--Stop DataStore 
						v_Msg := 'ENABLE(FALSE)';
						ENABLE(FALSE);
					END IF; 
				END; -- (3a)
			ELSE  -- (2c)
				--The Camstar Transaction must be NULL (v_TxnId is NULL) 
				--Commit any pending work, a sleep is performed and INIT is called
   	
				COMMIT; 

				v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ]';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 

				n_TransactionCnt := 0;

				d_CurrentDate := SYSDATE;

				UPDATE DATASTORESESSIONTRACKING    
				SET TIMESTAMP = d_CurrentDate  
				WHERE SESSIONNAME = pv_TableName; 
				n_RowsAffected := SQL%ROWCOUNT;   

				v_Msg := 'Rows affected [ ' || n_RowsAffected  || ' ] UPDATE DATASTORESESSIONTRACKING SET TIMESTAMP = [ ' || TO_CHAR(d_CurrentDate, 'YYYY-MM-DD HH24:MI:SS.SSSSS') || ' ] WHERE SESSIONNAME = [ ' || pv_TableName || ' ]'; 
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

				v_Msg := 'Sleep [ ' || gSleepTime || ' ] seconds';
				LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);   
				DBMS_SESSION.SLEEP(gSleepTime); 
				b_Slept := TRUE;
				
				v_Msg:='INIT(' || pv_TableName || ')';    
				INIT(pv_TableName);
			END IF;  -- (2a, 2b, 2c)
		END IF;  -- (1a) 
		-- >> END TRANSACTION PROCESSING

		v_Msg:='gTerminate := CHECK_TERMINATE';    
		gTerminate := CHECK_TERMINATE;   

		IF pb_Debug THEN    
			-- In debug mode, only run REPLICATOR loop once, then exit.
			gTerminate := TRUE;    
		END IF; 

		IF NOT gTerminate THEN   
			n_ReplicatorSeq := n_ReplicatorSeq +1;
		END IF;

	END LOOP;  -- (0)  WHILE (gTerminate = FALSE) 
	-- >> END CODE WHILE DATASTORE IS RUNNING

	--Datastore has stopped.  Commit all successful Camstar Transactions
	COMMIT; 

	v_Msg := 'COMMIT : n_TransactionCnt [ ' || n_TransactionCnt || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 

	n_TransactionCnt := 0;

	IF (cur_QueueTable%ISOPEN) THEN  
		v_Msg:='Close queue table cursor';  
		CLOSE cur_QueueTable;  
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);     
	END IF;    

	v_Loc := '[ ' || n_ReplicatorSeq || ' ] REPLICATOR'; 
	v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);   

	v_Loc := 'REPLICATOR';
EXCEPTION  
WHEN eVerifyHostFailure  THEN

	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, cLogLevelError);
	
	v_Err := 'Exception Handler (procedure when eVerifyHostFailure) : Exception : eVerifyHostFailure';
	LOG_MESSAGE(v_Err,v_Loc, cLogLevelError);

	--Disable DataStore
	v_Msg := 'ENABLE(FALSE)';
	ENABLE(FALSE);
	
	v_Loc:='[ ' || n_ReplicatorSeq || ' ] REPLICATOR';
	v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] End.';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
WHEN OTHERS THEN  
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);
	

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	
	v_Err := CASE b_WhiteListed WHEN TRUE THEN 'WhiteListed : ' || tab_JobInfo(gJobId) || ' : ' || SQLERRM ELSE  tab_JobInfo(gJobId) || ' : ' || SQLERRM END;
	LOG_ERROR( NVL(v_TxnId,'N/A'), 'N/A', v_Err ); 

	IF (cur_QueueTable%ISOPEN) THEN 
		CLOSE cur_QueueTable;  
		LOG_MESSAGE('Exception Handler (procedure when others) : Close queue table cursor' , v_Loc, cLogLevelMax);
	END IF; 

	IF (NOT b_WhiteListed) THEN
		--Disable DataStore
		ENABLE(FALSE);  
		
		v_Loc:='[ ' || n_ReplicatorSeq || ' ] REPLICATOR';	
		v_Msg := 'Iteration [ ' || n_ReplicatorSeq || ' ] End.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);
	END IF;  
END;    






---------------------------------------------------------------------
-- Name:        VERIFY_HOST
-- Params:      None
-- Descr:       Compares the executing ODS environment against metadata stored in the local DBDataStoreNames table.
--              Compares the OLTP executing environment via the database link TO_INSITE_PRODUCTION against metadata stored in the local 
--              DBDataStoreNames table.  Compares the metadata on the local DBDataStoreNames table to the metadata stored in the remote 
--              DBDataSourceNames table.
--              If the ODS is copied from production environment to a test or development
--              environment, we don't want the ODS to start processing records
--              from the production OLTP.
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_HostName VARCHAR2(100) to v_HostName VARCHAR2(256)
--              04/05/2016      Dan Maloney      Changed local variable v_ConfiguredHostName VARCHAR2(100) to v_ConfiguredHostName VARCHAR2(256)
--              04/05/2016      Dan Maloney      Changed local variable v_Ret BOOLEAN to b_Ret BOOLEAN
--              04/05/2016      Dan Maloney      Added local variable n_Seq NUMBER
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Removed local variable v_SetUpParm VARCHAR2(10)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              04/10/2017      Dan Maloney      Modifications to Verify_Host for US 1867 to better detect if the stored OLTP/ODS pair
--                                               metadata matches the executing environment for the OLTP/ODS pair.
--                                               A mismatch may result from restoring the ODS to another server, database, schema and
--                                               and failing to run a DB Update in CEP Management Studio prior to starting the ODS
--                                               Changed Verify_Host from function to procedure
--              12/06/2018      Dan Maloney      US 17617.  Simplify Veify Host logic and remove logic for checking for IP address
--                              Dan Maloney      US 11177.  Modifications to log messages
--
-- Copyright Siemens 2025  
--
PROCEDURE VERIFY_HOST 
IS 
	b_WhiteListed						BOOLEAN := FALSE;
	
	v_Err								VARCHAR2(4000);
	v_ExecOdsContainerDatabaseInd		VARCHAR2(3);
	v_ExecOdsSchemaName					VARCHAR2(64);
	v_ExecOdsDatabaseDomain				VARCHAR2(256);
	v_ExecOdsHostName					VARCHAR2(256); 
	v_ExecOdsSessionDatabaseName			VARCHAR2(256);
	v_ExecOdsUnQualifiedHostName		VARCHAR2(256);
	v_Loc								VARCHAR2(128); 
	v_Msg								VARCHAR2(4000); 	
	v_RemoteSavedOdsSchemaName			VARCHAR2(64);
	v_RemoteSavedOdsDatabaseName		VARCHAR2(64);	
	v_RemoteSavedOdsHostName			VARCHAR2(256); 
	v_RemoteOdsUnQualifiedHostName		VARCHAR2(256); 
	v_SQL								VARCHAR2(4000);
BEGIN 
	v_Loc := 'VERIFY_HOST';
	v_Msg := 'Check value of gVerifyHost';

	IF gVerifyHost = FALSE THEN 
		v_Msg := 'Verify DB Host is No.   Validation of OLTP/ODS pairing will NOT be performed.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		RETURN;
	ELSE
		v_Msg := 'Verify DB Host is Yes.   Validation of OLTP/ODS pairing will be  performed.';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	END IF; 
 
	--Get the remote ODS metadata in the DBDataSourcenames table from across the database link, TO_INSITE_PROUCTION
	v_Msg := 'Get the ODS metadata (DataBaseHostName, DatabaseName and SchemaName) stored in the remote DBDataSourceNames table';
	SELECT 
	UPPER( DatabaseHostName ), 
	UPPER( DatabaseName ), 
	UPPER( SchemaName ) 
	INTO 
	v_RemoteSavedOdsHostName, 
	v_RemoteSavedOdsDatabaseName, 
	v_RemoteSavedOdsSchemaName
	FROM DBDataSourceNames@TO_INSITE_PRODUCTION WHERE DataSourceNameId = 2;

	--Get the Executing ODS host name, unqualified host name, and schema name
	v_Msg := 'Get the executing ODS host name, unqualified host name, and schema name';
	SELECT 
	UPPER( SYS_CONTEXT('USERENV', 'SERVER_HOST') ) AS UNQUAILIFIED_HOST_NAME, 
	UPPER( HOST_NAME ) AS HOST_NAME
	INTO
	v_ExecOdsUnQualifiedHostName,
	v_ExecOdsHostName
	FROM V$INSTANCE;

	--Get the Executing ODS container database indicator, database name of the ODS session, database nomain and schema name where the ODS session is executing 
	v_Msg := 'Get the Executing ODS container database indicator, database name of the ODS session, database name and schema name where the ODS session is executing';
	SELECT 
	CDB AS CONTAINER_DATABASE_IND,
	UPPER( SYS_CONTEXT('USERENV', 'CON_NAME') ) AS SESSION_DATABASE_NAME,
	UPPER( SYS_CONTEXT('USERENV', 'DB_DOMAIN') ) AS DATABASE_DOMAIN,
	UPPER( SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') ) AS SCHEMA_NAME
	INTO
	v_ExecOdsContainerDatabaseInd,
	v_ExecOdsSessionDatabaseName,
	v_ExecOdsDatabaseDomain,
	v_ExecOdsSchemaName
	FROM V$DATABASE;
		
	--Get unqualified host name for the DatabaseHostName retrieved from the metadata on the OLTP
	IF INSTR(v_RemoteSavedOdsHostName,'.') > 0
	--Remove domain portion from the value stored in v_RemoteSavedOdsHostName
	THEN
		v_RemoteOdsUnQualifiedHostName := SUBSTR(v_RemoteSavedOdsHostName,1,INSTR(v_RemoteSavedOdsHostName,'.')-1);
	ELSE
		v_RemoteOdsUnQualifiedHostName := v_RemoteSavedOdsHostName;
	END IF;
	
	v_Msg := 'ODS metadata retrieved from OLTP across database link : '
	|| 'Database Host Name [ ' || v_RemoteSavedOdsHostName || ' ]  '
	|| 'Database Unqualified Host Name [ ' || v_RemoteOdsUnQualifiedHostName || ' ]  '
	|| 'Session Database Name [ ' || v_RemoteSavedOdsDatabaseName || ' ]  '
	|| 'Schema Name [ ' || v_RemoteSavedOdsSchemaName || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
		
	v_Msg := 'Executing ODS : '
	|| 'Executing ODS Host Name [ ' || v_ExecOdsHostName || ' ]  '
	|| 'Executing ODS Unqualified Host Name [ ' || v_ExecOdsUnQualifiedHostName	|| ' ]  '
	|| 'Executing ODS Container Instance [ ' || v_ExecOdsContainerDatabaseInd	|| ' ]  '
	|| 'Executing ODS Session Database Name [ ' || v_ExecOdsSessionDatabaseName	|| ' ]  '
	--|| 'Executing ODS Domain Name[ ' || v_ExecOdsDatabaseDomain || ' ]  '
	|| 'Executing ODS Schema Name [ ' || v_ExecOdsSchemaName || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
	
	--Begin Validation
	--Compare the executing ODS database name to the DatabaseName retrieved from the metadata on the OLTP
	v_Msg := 'Compare the executing ODS database name to the DatabaseName retrieved from the metadata on the OLTP';
	IF v_ExecOdsSessionDatabaseName != v_RemoteSavedOdsDatabaseName
	THEN	
		RAISE eVerifyHostFailure;
	END IF;
	
	--Compare the executing ODS schema name to the SchemaName retrieved from the metadata on the OLTP
	v_Msg := 'Compare the executing ODS schema name to the SchemaName retrieved from the metadata on the OLTP';
	IF v_ExecOdsSchemaName != v_RemoteSavedOdsSchemaName
	THEN	
		RAISE eVerifyHostFailure;
	END IF;
		
	--Compare the executing ODS host name to the DatabaseHostName retrieved from the metadata on the OLTP
	v_Msg := 'Compare the executing ODS host name to the DatabaseHostName retrieved from the metadata on the OLTP';
	IF (v_ExecOdsHostName != v_RemoteSavedOdsHostName) 
	THEN	
		--Compare the executing ODS unqualified host name to the to the unqualified DatabaseHostName retrieved from the metadata on the OLTP
		v_Msg := 'Compare the executing ODS unqualified host name to the to the unqualified DatabaseHostName retrieved from the metadata on the OLTP';
		IF (v_ExecOdsUnQualifiedHostName != v_RemoteOdsUnQualifiedHostName)
		THEN
			RAISE eVerifyHostFailure;
		END IF;
	END IF;

EXCEPTION  
WHEN eVerifyHostFailure THEN
	v_Err := tab_JobInfo(gJobId) || ' : ' || SQLERRM || ' eVerifyHostFailure';
	LOG_ERROR( 'N/A', 'N/A', v_Err ); 
	
	v_Msg := 'Exception Handler (procedure when eVerifyHostFailure) : OLTP expects ODS '
	|| '[' || v_RemoteSavedOdsHostName || '].'
	|| '[' || v_RemoteSavedOdsDatabaseName || '].'
	|| '[' || v_RemoteSavedOdsSchemaName || '], ODS reports as '
	|| '[' || v_ExecOdsHostName	|| '].'
	|| '[' || v_ExecOdsSessionDatabaseName || '].'
	|| '[' || v_ExecOdsSchemaName || '].  NOTE: ODS does not support IP addresses if Verify DB Host is "Yes".';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError); 
	RAISE;
WHEN OTHERS THEN 
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE; 
END;    
  


  
---------------------------------------------------------------------
-- Name:        VERIFY_JOB
-- Params:      <in> pv_TableName (DATASTOREUPDATES, DATASTOREINSERTS1,2,3,4...)
--              <in> pv_TableName
-- Descr:       Checks to see if the job exists. If not, creates it.
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_JobId NUMBER to n_JobId NUMBER
--              04/05/2016      Dan Maloney      Changed local variable v_Status VARCHAR2(10) to v_Broken VARCHAR2(1)
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(2000) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_What VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added logic to populate package global variable associative array, tab_JobInfo when a job is submitted.
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              v8.4 (1)        Dan Maloney      Replace DBMS_JOB with DBMS_SCHEDULER (US 11177)
--
--
-- Copyright Siemens 2025  
--
PROCEDURE VERIFY_JOB
(
	pv_ProcName             		IN VARCHAR2, 
	pv_TableName            		IN VARCHAR2
) 
IS   
	b_WhiteListed           		BOOLEAN := FALSE;
	--n_JobId                 		NUMBER;  
	
	v_JobName						VARCHAR2(128);
	v_JobAction						VARCHAR2(128);
	v_Broken                		VARCHAR2(1); 
	v_Err                   		VARCHAR2(4000); 
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000); 
	v_SQL                   		VARCHAR2(4000); 
	v_What                  		VARCHAR2(4000);   
	v_Database						VARCHAR2(128);
	v_JobClass						VARCHAR2(128);
	v_Argument						VARCHAR2(128);
BEGIN   
	v_Loc := 'VERIFY_JOB';

	v_Database :=  SYS_CONTEXT('USERENV','CON_NAME');
	v_JobClass := '"OPCENTER_' ||SYS_CONTEXT('USERENV','CURRENT_SCHEMA') || '"';

	v_Msg := 'Job Class : [ ' || v_JobClass || ' ] Database : [ ' || v_Database || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);   
	
	IF pv_TableName IS NULL THEN 
		v_JobName :=  'OPCENTER_DATASTORE_' || pv_ProcName ;
		v_JobAction := 'BEGIN ' || cPACKAGENAME || '.' || pv_ProcName || '; END;';
	ELSE
		v_JobName :=  'OPCENTER_DATASTORE_' || pv_ProcName || '_' || pv_TableName;
		v_JobAction := 'BEGIN ' || cPACKAGENAME || '.' || pv_ProcName || '(''' || pv_TableName || ''')' || '; END;';
	END IF;
	
	v_Msg := 'Check if [ ' || v_JobName || ' ] exists in USER_SCHEDULER_JOBS'; 
	BEGIN
		v_SQL := 'SELECT Job_Name FROM USER_SCHEDULER_JOBS WHERE JOB_NAME = :b1';
		EXECUTE IMMEDIATE v_SQL INTO v_JobName USING v_JobName;
		v_JobAction := REPLACE(REPLACE(REPLACE(v_JobAction,'BEGIN',''),'END;',''),' ','');
		v_Msg := 'Job : [ ' || v_JobName || ' ]  : Job Action : [ ' || v_JobAction || ' ] is scheduled'; 
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	EXCEPTION
	WHEN NO_DATA_FOUND THEN
		v_Msg := 'Creating Job : [ ' || v_JobName || ' ] Job Action : [ ' || v_JobAction;
						
		DBMS_SCHEDULER.CREATE_JOB(
			JOB_NAME => v_JobName,
			JOB_TYPE => 'PLSQL_BLOCK',
			JOB_ACTION => v_JobAction,
			START_DATE =>  TRUNC(SYSDATE,'MI'),
			REPEAT_INTERVAL => 'FREQ=MINUTELY',
			JOB_CLASS => v_JobClass,
			ENABLED => TRUE,
			COMMENTS => 'DataStore ' || INITCAP(pv_ProcName) || ' Job for Database Name : [ ' || v_Database  || ' ] Schema : [ ' || user || ' ]',
			AUTO_DROP => FALSE
		);		
			
		--Populate associative array.  The assoicative array is indexed by the JOB_NAME (v_JobName)
		tab_JobInfo(v_JobName) := REPLACE(REPLACE(REPLACE(v_JobAction,'BEGIN',''),'END;',''),' ','');

		v_Msg := 'Job : [ ' || v_JobName || ' ]  : Job Action : [ ' || tab_JobInfo(v_JobName) || ' ] submitted to run every minute'; 
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		LOG_MESSAGE('Update job information in global array', 'Package Initialization', cLogLevelMin);
	END;
EXCEPTION    
WHEN OTHERS THEN 
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;
END;    






--------------------------------------------------------------------
-- Name:        INIT
-- Params:      <in> pv_TableName (DATASTOREUPDATES, DATASTOREINSERTS1,2,3,4...)
--
-- Descr:       Gets setup information from InSiteSiteInfo table and DataStoreSetUp and inserts record into DataStoreSessionTracking if missing for 
--              SessionName (@pnv_TableName) or updates DataStoreSessionTracking Timestamp field for SessionName (@pnv_TableName) if it exists.
--              Execute on ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(255) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added logic to check DATASTORESETUP.VALUE for DATASTORESETUP.PARAMETER = 'LOG_LEVEL'
--              04/05/2016      Dan Maloney      Added logic to check DATASTORESETUP.VALUE for DATASTORESETUP.PARAMETER = 'LOG_RETENTION'
--              04/05/2016      Dan Maloney      Added logic to check DATASTORESETUP.VALUE for DATASTORESETUP.PARAMETER = 'VERIFY_HOST'
--              06/30/2016      Dan Maloney      Reordered the order of assignment of global variables in cParm cursor fetch loop
--              06/30/2016      Dan Maloney      Added cInsiteSiteInfo cursor and logic to read DataStoreDelimiter, DataStorePresent and DataStoreInsertTable 
--                                               from InsiteSiteInfo instead of DataStoreSetUp
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
-- Copyright Siemens 2025  
--
PROCEDURE INIT
(
	pv_TableName            		IN VARCHAR2
) 
IS  
	b_WhiteListed           		BOOLEAN := FALSE;
	d_CurrentDate           		DATE; 
	n_RowsAffected          		NUMBER := 0;   
	v_Err                   		VARCHAR2(4000); 
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_SQL                   		VARCHAR2(4000); 

	CURSOR cParms IS    
	SELECT PARAMETER, VALUE    
	FROM DATASTORESETUP;    

	CURSOR cInsiteSIteInfoParms IS    
	SELECT TNAME, TVALUE   
	FROM INSITESITEINFO
	WHERE TNAME IN ('DataStorePresent', 'DataStoreDelimiter', 'DataStoreInsertTables'); 
 
	CURSOR cTracking IS    
	SELECT PROCESSEDTXNID,PROCESSEDID    
	FROM DATASTORESESSIONTRACKING    
	WHERE SESSIONNAME = gTableName;    
  
BEGIN  
	v_Loc := 'INIT ';
	gTableName := UPPER(pv_TableName);

	IF (gTableName = 'DATASTOREUPDATES') THEN  
		gTableType := cUPDATES;  
   	ELSE  
		gTableType := cINSERTS;
	END IF;  

	v_Msg := 'Open FOR LOOP cursor';
	FOR crec IN cParms 
	LOOP   
		v_Msg := 'Set gAccessMode [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER)=cPARM_ACCESSMODE THEN    
			gAccessMode := UPPER(crec.VALUE);    
		END IF;

		v_Msg := 'Set gBatchSize [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER)=cPARM_INSUPDSIZE THEN    
			gBatchSize := TO_NUMBER(crec.VALUE);    
		END IF;  

		v_Msg := 'Set gCleanUpBatchSize [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_CLEANUPSIZE THEN    
			gCleanupBatchSize := TO_NUMBER(crec.VALUE);  
		END IF;    

		v_Msg := 'Set gTerminate [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_TERMINATE THEN    
			IF crec.VALUE = cNO THEN    
				gTerminate := FALSE;
			ELSE 
				gTerminate := TRUE;  
			END IF;    
		END IF;   

		v_Msg := 'Set gSleepTime [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_INSUPDWAIT THEN   
			gSleepTime := TO_NUMBER(crec.VALUE); 
		END IF;    

		v_Msg := 'Set gKeepRecords [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_KEEPRECORDS THEN    
			IF crec.VALUE = 'Y' THEN  
				gKeepRecords := TRUE;  
			ELSE    
				gKeepRecords := FALSE;  
         	END IF;    
		END IF;    

		v_Msg := 'Set gLogLevel [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_LOG_LEVEL THEN  
			gLogLevel := TO_NUMBER(crec.VALUE);  
		END IF; 

		v_Msg := 'Set gLogRetention [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_LOG_RETENTION THEN  
			gLogRetention := TO_NUMBER(crec.VALUE);  
		END IF; 

		v_Msg := 'Set gMissTxnMax [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_MISSTXNMAX THEN    
			gMissTxnMax := TO_NUMBER(crec.VALUE); 
		END IF;    

		v_Msg := 'Set gSTOP_IF_RETRIES_EXCEEDED [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_RETRYSTOP THEN    
			IF crec.VALUE = cNO THEN    
				gSTOP_IF_RETRIES_EXCEEDED := FALSE;    
			ELSE 
				gSTOP_IF_RETRIES_EXCEEDED := TRUE; 
			END IF;    
		END IF;    

		v_Msg := 'Set gSTOP_ON_DUPLICATE_INSERT [ ' || crec.VALUE || ' ]';	
		IF UPPER(crec.PARAMETER) = cPARM_DUPSTOP THEN    
			IF crec.VALUE=cNO THEN  
				gSTOP_ON_DUPLICATE_INSERT := FALSE;  
			ELSE  
				gSTOP_ON_DUPLICATE_INSERT := TRUE; 
			END IF;    
		END IF;    

		v_Msg := 'Set gSTOP_ON_NO_UPDATE [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_NOUPDSTOP THEN    
			IF crec.VALUE = cNO THEN  
				gSTOP_ON_NO_UPDATE := FALSE; 
			ELSE  
				gSTOP_ON_NO_UPDATE := TRUE; 
			END IF;    
		END IF;    

		v_Msg := 'Set gVerifyHost [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_VERIFY_HOST THEN    
			IF crec.VALUE = 'Y' THEN  
				gVerifyHost := TRUE;  
			ELSE    
				gVerifyHost := FALSE;  
			END IF;    
		END IF;   

		v_Msg := 'Set gVersion [ ' || crec.VALUE || ' ]';
		IF UPPER(crec.PARAMETER) = cPARM_VERSION THEN    
			gVersion := crec.VALUE;
		END IF;   
	END LOOP;

	v_Msg := 'Open FOR LOOP cursor';
	FOR crec IN cInsiteSiteInfoParms 
	LOOP   
		v_Msg := 'Set gDelimiter [ ' || crec.TVALUE || ' ]';
		IF UPPER(crec.TNAME)=cPARM_DELIMITER THEN   
			gDelimiter:=crec.TVALUE;   
		END IF;   

		v_Msg := 'Set gNumInsertTables [ ' || crec.TVALUE || ' ]';
		IF UPPER(crec.TNAME) = cPARM_INSERTTABLES THEN   
			gNumInsertTables := TO_NUMBER(crec.TVALUE);  
		END IF;  
  
		v_Msg := 'Set gDataStorePresent [ ' || crec.TVALUE || ' ]';
		IF UPPER(crec.TNAME) = cPARM_DATASTOREPRESENT THEN   
			gDataStorePresent := crec.TVALUE;  
		END IF;    
	END LOOP;

	IF pv_TableName IS NOT NULL THEN
		v_Msg := 'Package global variables set from DATASTOREETUP and INSITESITEINFO';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	END IF;

	IF (pv_TableName LIKE 'DATASTORE%') THEN 
		OPEN cTracking; 
		FETCH cTracking INTO gLastProcessedTxn, gLastProcessedId;    

		IF cTracking%NOTFOUND THEN  
			v_Msg := 'Populate DataStoreSessionTracking with zero ProcessedTxnId and ProcessedId'; 
			d_CurrentDate := SYSDATE; 

			INSERT INTO DATASTORESESSIONTRACKING (SESSIONNAME, PROCESSEDTXNID, PROCESSEDID, WAITINGID, TIMESTAMP)    
			VALUES (gTableName, cZEROID, 0, 0, d_CurrentDate); 

			gLastProcessedTxn := cZEROID; 
			gLastProcessedId := 0;  
			n_RowsAffected := SQL%ROWCOUNT;  
			COMMIT;

			v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ]  INSERT INTO DATASTORESESSIONTRACKING (SESSIONNAME, PROCESSEDTXNID, PROCESSEDID, WAITINGID, TIMESTAMP)' 
			|| 'VALUES (' || gTableName || ',' || cZEROID || ', 0, 0,' || TO_CHAR(d_CurrentDate,'YYYY-MM-DD HH24:MI:SS.SSSSS') || ')'; 
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

			v_Msg := 'COMMIT : Issued after INSERT INTO DATASTORESESSIONTRACKING';
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		END IF; 

		v_Msg := 'Close DATASTORESESSIONTRACKING cursor';   
   		CLOSE cTracking;  
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);  

		/*********************************************************************************************************************************************************************************/
		IF (gAccessMode=cMODEREMOTE) THEN   
			v_SQL := 'DELETE FROM LOCAL_'||gTableName;    
			EXECUTE IMMEDIATE v_SQL;    
		END IF;  
		/*********************************************************************************************************************************************************************************/  
   
	END IF;    
EXCEPTION 
WHEN OTHERS THEN 
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	IF cTracking%ISOPEN THEN
		CLOSE cTracking;  
		LOG_MESSAGE('Exception Handler (procedure when others) : Close DATASTORESESSIONTRACKING cursor', v_Loc, cLogLevelMax);  
	END IF;
 
	RAISE;   
END;    



--------------------------------------------------------------------
-- Name:        GET_NEXT_TXNID
-- Params:      <in><out> pcur_QueueTable 
--              <out> pv_TxnId
--              <out> pv_TxnType
--              <out> pv_Status 
--              <out> pn_NextId
--              <out> pn_CDOID
--              <out> pv_Msg
--              <out> pv_SourceQueue
--
-- Descr:       Queries the appropriate datastore queue table to get the next TxnId to be processed
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(512) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Removed logic that evaluated gAccessMode=cMODELOCAL
--              04/18/2016      Dan Maloney      Added pv_SourceQueue as OUT parameter
--              04/18/2016      Dan Maloney      Added local variable v_SourceQueue VARCHAR2(16)
--              04/20/2017      Dan Maloney      Added WHERE ROWNUM <- 1000 to limit query results to first 1000 rows
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--                                               Changed Retries exceeded logic to handle Uncommitted and Missing IDs seperately. 
--              08/22/2019      Dan maloney      Moved prcoessing for ORA-08176 from REPLICATOR to to GET_NEXT_TXNID for Bug 39355
--                                               When ORA-08176 is detected, log it in the DataStoreLog table as a LOG_LEVEL 2 message 
--                                               CLOSE the open cursor and continue ODS processing

--
-- Copyright Siemens 2025  
--
PROCEDURE GET_NEXT_TXNID
(
	pcur_QueueTable         		IN OUT typ_Cursor, 
	pv_TxnId                		OUT VARCHAR2, 
	pv_TxnType              		OUT VARCHAR2, 
	pv_Status               		OUT VARCHAR2, 
	pn_NextId               		OUT NUMBER,    
	pn_CDOID                		OUT NUMBER, 
	pv_Msg                  		OUT VARCHAR2,
	pv_SourceQueue          		OUT VARCHAR2  
) 
IS   
	b_WhiteListed           		BOOLEAN := FALSE; 
	v_Err                   		VARCHAR2(4000); 
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_SQL                   		VARCHAR2(4000);  
BEGIN  
	v_Loc := 'GET_NEXT_TXNID';

	IF NOT pcur_QueueTable%ISOPEN THEN  
		v_SQL := 'SELECT ID,TXNID,TXNTYPE,STATUS, CDOID, ERROR, ''LOCAL'' AS SOURCE FROM ' || gAccessMode || '_' || gTableName || 'MAST WHERE ID > :LastId ORDER BY ID';  -- JAL ADD STATUS  

		/*********************************************************************************************************************************************************************************/
		IF gAccessMode = cMODELOCAL THEN
			v_SQL := 'SELECT ID,TXNID,TXNTYPE,STATUS, CDOID, ERROR, ''REMOTE'' AS SOURCE FROM REMOTE_' || gTableName || 'MAST WHERE ID > :LastId AND STATUS = ' || CHR(39) || cSTATUSUNCOMMITTED || CHR(39) 
			|| ' UNION ' || v_SQL;  -- JAL union database tables to insure uncommitted aren't skipped. 
		END IF;
		/*********************************************************************************************************************************************************************************/

		v_SQL := 'SELECT * FROM ( ' || v_SQL || ' ) WHERE ROWNUM <= 10000'; 
		v_Msg:='Open queue table cursor for: ' || REPLACE(v_SQL, ':LastId', '[ ' || gLastProcessedId || ' ]');
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

		/*********************************************************************************************************************************************************************************/
		--If gAccessMode != cMODELOCAL (i.e. LOCAL), the OPEN cursor statement will fail as the SQL will have just one bind variable and the following open cursor statement passes in 2 values
		--expecting 2 bind variables in the SQL statement
		/*********************************************************************************************************************************************************************************/
		OPEN pcur_QueueTable FOR v_SQL USING gLastProcessedId, gLastProcessedId;
	END IF;    
	
	v_Msg:='Fetch from queue table cursor';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	
	FETCH pcur_QueueTable INTO pn_NextId, pv_TxnId, pv_TxnType, pv_Status, pn_CDOID, pv_Msg, pv_SourceQueue; 

	v_Msg:='pv_TxnId [ ' || pv_TxnId || ' ] pv_TxnType [ ' || pv_TxnType || ' ] pv_Status [ ' || pv_Status || ' ] pv_Msg [ ' || pv_Msg || ' ] pn_CDOID [ ' 
	|| pn_CDOID || ' ] pn_NextId [ ' || pn_NextId || ' ] pv_SourceQueue [ ' || pv_SourceQueue || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMin);  

	IF pcur_QueueTable%NOTFOUND THEN    
 		pv_TxnId := NULL;    
		pv_TxnType := NULL;    
		pv_Status := NULL;    
		pv_Msg := NULL;    
		pn_CDOID := 0;    
		pn_NextId := 0; 
		pv_SourceQueue := NULL;   

		v_Msg:='Close queue table cursor.  Queue table cursor contains no rows to fetch.';
		CLOSE pcur_QueueTable;  
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
	END IF;
EXCEPTION
WHEN OTHERS THEN

	--ORA-08176: consistent read failure; rollback data no available
	--When an ORA-08176 occurs, log it as min detail and continue ODS procesing
	IF SQLCODE = -8176 THEN
		v_Err := 'Exception Handler (procedure when others) : [ ' || SQLERRM || ' ] occurred.  Continuing ODS processing';
		LOG_MESSAGE(v_Err, v_Loc, cLogLevelMax);
		
		IF pcur_QueueTable%ISOPEN THEN
			CLOSE pcur_QueueTable;  
		END IF; 
		
		v_Err :='Exception Handler (procedure when others) : Close queue table cursor.  Queue table cursor contains no rows to fetch.';
		LOG_MESSAGE(v_Err, v_Loc, cLogLevelMax); 
	ELSE
		b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);
		
		v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
		LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

		v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
		LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

		RAISE;
	END IF;
END;    



--------------------------------------------------------------------
-- Name:        IS_LOCKED
-- Params:      <in> pv_TxnId 
--              <in> pv_TxnType
-- Return:      TRUE/FALSE
--
-- Descr:       Performs synchronization logic to determine if the txn can be executed
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_cnt NUMBER to n_Cnt NUMBER
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/18/2016      Dan Maloney      Added local variable b_InsertsPending BOOLEAN := FALSE
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
--
FUNCTION IS_LOCKED
(
	pv_TxnId                		IN VARCHAR2,
	pv_TxnType              		IN VARCHAR2,
	pn_Id                   		IN NUMBER
) 
RETURN BOOLEAN 
IS    
	b_InsertsPending        		BOOLEAN := FALSE;
	b_WhiteListed           		BOOLEAN := FALSE;
	n_Cnt                   		NUMBER;
	n_RowsAffected          		NUMBER := 0;
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
BEGIN
	v_Loc := 'IS_LOCKED';

	-- Rules for locking:
	-- 	For gTableType = 'UPDATES'
	--  	1) If TxnType='T' (i.e. cTXNTYPE), all inserts must be completed for this txnid and lower txn ids

	IF (gTableType = cUPDATES) THEN 
		IF (pv_TxnType = cTXNTYPE OR pv_TxnType = cDUMMYTYPE) THEN 
			--If processing DATASTOREUPDATESMAST and transaction is a regular transaction or a dummy transaction, 
			--return TRUE if inserts are pending or FALSE if inserts are not pending 
			v_Msg := 'b_InsertsPending := ARE_INSERTS_PENDING(' || pv_TxnId || ')';  		
			b_InsertsPending := ARE_INSERTS_PENDING(pv_TxnId);  
		END IF;    
	END IF;    

	v_Msg := 'Function return value [ ' || CASE b_InsertsPending WHEN TRUE THEN 'TRUE' ELSE 'FALSE' END || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	RETURN b_InsertsPending;  
EXCEPTION
WHEN OTHERS THEN
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (function when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (function when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;
END;    



--------------------------------------------------------------------
-- Name:        PARSE_AND_EXECUTE_TXN
-- Params:      <in> pv_TxnId           TxnId to execute
--              <in> pv_TxnType         T = Regular transaction, D = Dummy
--
-- Descr:       Queries the SQL for the given TxnId, parses it and executes.
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_CUR tCursor to cur_QueueTable typ_Cursor
--              04/05/2016      Dan Maloney      Changed local variable v_CurRec CLOB to clob_CurRec CLOB
--              04/05/2016      Dan Maloney      Changed local variable v_FullLOB CLOB to clob_FullLOB CLOB
--              04/05/2016      Dan Maloney      Changed local variable v_ID NUMBER to n_ID NUMBER
--              04/05/2016      Dan Maloney      Changed local variable v_ErrCode NUMBER to n_ErrCode NUMBER
--              04/05/2016      Dan Maloney      Changed local variable v_SQL VARCHAR2(512) to v_SQL VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Changed local variable v_ErrMsg VARCHAR2(512) to v_ErrMsg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Removed local variable v_Delimiter
--              04/05/2016      Dan Maloney      Removed local variable v_ErrHandled BOOLEAN
--              04/05/2016      Dan Maloney      Replaced removed pakcage global variable gAccessMode with hard-coded 'LOCAL'
--              05/12/2016      Dan Maloney      Added local variable n_ReversalLen NUMBER := 0
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              06/21/2017      Dan Maloney      Removed log messaging from exception handler as it will be handled in REPLICATOR (US 51393)
--              05/27/2020      Dan Maloney      Removed logic log log a reveral message in DataStoreLog if the Camstar transactions contain reversals CPR 82735 (TFS05)
--                                               for v8.3 and CPR 82747 (TFS05) for V7 MU2.  These 2 CPRs have parent PR 26237 (TFS05) which has parent IR 94110101
--
--
-- Copyright Siemens 2025  
--
PROCEDURE PARSE_AND_EXECUTE_TXN
(
        pv_TxnId                		IN VARCHAR2,
        pv_TxnType              		IN VARCHAR2
) 
IS    
        cur_QueueTable          		typ_Cursor; 
        b_WhiteListed           		BOOLEAN := FALSE;
        clob_CurRec             		CLOB;    
        clob_FullLOB            		CLOB;  
        n_DelimLen              		NUMBER;  
        n_ErrCode               		NUMBER;   
        n_ID                    		NUMBER;  
        n_LastOffSet            		NUMBER := 1; 
        n_OffSet                		NUMBER := 1;   
        --n_ReversalLen           		NUMBER := 0;
        n_RowsAffected          		NUMBER := 0; 

        $IF DBMS_DB_VERSION.VER_LE_10 $THEN 
                v_Buf           		VARCHAR2(32767);   
        $Else 
                v_buf           		CLOB;  
        $End 

        v_Err                   		VARCHAR2(4000);  
        v_ErrMsg                		VARCHAR2(4000);    
        v_Loc                   		VARCHAR2(128); 
        v_Msg                   		VARCHAR2(4000); 
        v_SQL                   		VARCHAR2(4000); 
BEGIN    
	v_Loc := 'PARSE_AND_EXECUTE_TXN';

	IF (pv_TxnType = cDUMMYTYPE) THEN    
		-- TxnType=D, ignore
		v_Msg := 'No SQL Statements to parse for Transaction {pv_TxnId} [ ' || pv_TxnId || ' ]';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
		RETURN;    
	END IF;    

	n_DelimLen:=LENGTH(gDelimiter);    

	DBMS_LOB.createtemporary(clob_FullLOB, TRUE);   

	/*********************************************************************************************************************************************************************************/
	IF (gAccessMode=cMODEREMOTE) THEN 
		v_SQL := 'INSERT INTO LOCAL_'||gTableName||'(TXNID,SEQUENCE,SQLSTMT) SELECT TXNID,SEQUENCE,SQLSTMT FROM REMOTE_'||gTableName||' WHERE TXNID=:txnid';    
		EXECUTE IMMEDIATE v_SQL USING pv_TxnId;    
	END IF;  
	/*********************************************************************************************************************************************************************************/

	v_SQL:='SELECT sqlstmt FROM LOCAL_' || gTableName || ' WHERE TXNID = :TXNID ORDER BY SEQUENCE';    

	OPEN cur_QueueTable FOR v_SQL USING pv_TxnId;    
	FETCH cur_QueueTable INTO clob_CurRec;    
 
	WHILE (cur_QueueTable%FOUND) 
	LOOP    
		DBMS_LOB.append(clob_FullLOB, clob_CurRec);    
		FETCH cur_QueueTable INTO clob_CurRec;   
   	END LOOP; 

	IF cur_QueueTable%ISOPEN THEN
		v_Msg:='Close LOCAL_' || gTableName || ' cursor';  
		CLOSE cur_QueueTable; 
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
	END IF;

	v_Msg := 'BEGIN SQL Statements for Transaction pv_TxnId [ ' || pv_TxnId || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
	    
	WHILE n_OffSet != 0 
	LOOP  -- (1a)
		n_OffSet := DBMS_LOB.INSTR(clob_FullLOB, gDelimiter, n_OffSet, 1);    

		IF n_OffSet != 0 THEN 
			$IF DBMS_DB_VERSION.VER_LE_10 $THEN    
				v_buf := DBMS_LOB.SUBSTR(clob_FullLOB, n_OffSet-n_LastOffSet, n_LastOffSet); 
			$Else 
				v_buf := SUBSTR(clob_FullLOB, n_LastOffSet, n_OffSet-n_LastOffSet); 
	 		$End    

			IF (LENGTH(v_buf) > 0) THEN 
				EXECUTE IMMEDIATE v_buf;
				n_RowsAffected := SQL%ROWCOUNT;
				LOG_MESSAGE('Rows affected [ ' || n_RowsAffected || ' ] ' || v_buf, v_Loc, cLogLevelMax);    

				-- Check the result of the statement if this is the
				-- UPDATE queue and it's a regular transaction.
				-- If no rows were affected on an Update
				-- statement, log an error (but continue processing).

				IF (pv_TxnType = cTXNTYPE AND gTableType = cUPDATES) THEN 
					IF (n_RowsAffected = 0) THEN    
						IF (SUBSTR(v_buf, 1, 1) = cUPDATE) THEN 
							RAISE eNoRowsOnUpdate;    
						END IF;    
					END IF;  
				END IF; 
			END IF;  
  
			n_OffSet := n_OffSet + n_DelimLen;    
			n_LastOffSet := n_OffSet;  
		END IF;   
	END LOOP; -- (1a) WHILE n_OffSet != 0 
 
	v_Msg := 'END SQL Statements for Transaction pv_TxnId [ ' || pv_TxnId || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax); 
EXCEPTION
WHEN eNoRowsOnUpdate THEN   
	LOG_ERROR(pv_TxnId, v_buf, 
	CASE gSTOP_ON_NO_UPDATE 
	WHEN FALSE THEN 'STOP_ON_NO_UPDATE = "N" : No rows updated' 
	ELSE 
		'STOP_ON_NO_UPDATE = "Y" : No rows updated' 
	END);  

	v_Err := 'Exception Handler (procedure when eNoRowsOnUpdate) : No Rows Updated from SQL [ ' || v_buf || ' ]';  
	LOG_MESSAGE(v_Err, v_Loc, CASE gSTOP_ON_NO_UPDATE WHEN FALSE THEN cLogLevelWhiteListed ELSE cLogLevelError END); 

	v_Err := 'Exception Handler (procedure when eNoRowsOnUpdate) : See DATASTOREERRORS for details.  TXNID [ ' || pv_TxnId || ' ]';  
	LOG_MESSAGE(v_Err, v_Loc, CASE gSTOP_ON_NO_UPDATE WHEN FALSE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE; --Raise exception to REPLICATOR
WHEN DUP_VAL_ON_INDEX  THEN  
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE); 

	LOG_ERROR(pv_TxnId, v_buf, 
	CASE b_WhiteListed 
	WHEN TRUE THEN 'WhiteListed : ' || SQLERRM 
	ELSE 
		CASE gSTOP_ON_DUPLICATE_INSERT 
		WHEN FALSE THEN 'STOP_ON_DUPLICATE_INSERT = "N" : ' || SQLERRM 
		ELSE 'STOP_ON_DUPLICATE_INSERT = "Y" : ' || SQLERRM 
		END
	END);  

	v_Err := 'Exception Handler (procedure when DUP_VAL_ON_INDEX) : Duplicate Value on Index from SQL [ ' || v_buf || ' ]';  
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when DUP_VAL_ON_INDEX) : See DATASTOREERRORS for details. TXNID [ ' || pv_TxnId || ' ]';    
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END); 

	RAISE;  --Raise exception to  REPLICATOR
WHEN OTHERS THEN  
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE); 

	LOG_ERROR(pv_TxnId, v_buf, 
	CASE b_WhiteListed 
	WHEN TRUE THEN 'WhiteListed : ' || SQLERRM 
	ELSE 
		SQLERRM 
	END);  

	v_Err := 'Exception Handler (procedure when others) : SQL [ ' || v_buf || ' ]';  
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);
  
	v_Err := 'Exception Handler (procedure when others) : See DATASTOREERRORS for details. TXNID [ ' || pv_TxnId || ' ]';  
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;  --Raise exception to REPLICATOR 
END;    


--------------------------------------------------------------------
-- Name:        SET_LAST_PROCESSED
-- Params:      <in> pv_TxnId
--              <in> pn_Id
--              <in> pTxnType
--
-- Descr:       Updates DATASTORESESSIONTRACKING
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/19/2016      Dan Maloney      Added IN parameter pv_Status VARCHAR2
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              06/21/2017      Dan Maloney      Removed delete from DATASTOREMISSINGTXNS
--              10/10/2017      Dan Maloney      Removed code, "DELETE FROM DataStoreMissingTxns where MISSEDID = pn_Id" (BUG 57436)
--                                               The removal of the record from the DataStoreMissingTxns table has been moved to the REPLICATOR procedure
--               08/28/2018     Dan Maloney      Modified condition to check pv_Status to check if pv_Status IS NOT NULL for CPR 9767 
--
--
-- Copyright Siemens 2025  
--
PROCEDURE SET_LAST_PROCESSED
(
	pv_TxnId                		IN VARCHAR2, 
	pn_Id                   		IN NUMBER, 
	pv_TxnType              		IN VARCHAR2,
	pv_Status               		IN VARCHAR2
) 
IS  
	b_WhiteListed           		BOOLEAN := FALSE;
	d_CurrentDate           		DATE; 
	n_RowsAffected          		NUMBER := 0;
	v_Err                   		VARCHAR2(4000);   
	v_Loc                   	 	VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000); 
BEGIN   
	v_Loc := 'SET_LAST_PROCESSED';

	d_CurrentDate := SYSDATE;

	UPDATE DATASTORESESSIONTRACKING    
	SET PROCESSEDTXNID = pv_Txnid,    
	PROCESSEDID = pn_Id,    
	TIMESTAMP = d_CurrentDate,    
	WAITINGID = 0    
	WHERE SESSIONNAME = gTableName; 
 
	n_RowsAffected := SQL%ROWCOUNT;  

	v_Msg := 'Advance Camstar transaction.  Set ProcessedTxnId to [ ' ||  pv_TxnId || ' ].  Set ProcessedId to [ ' || pn_Id || ' ]';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] UPDATE DATASTORESESSIONTRACKING SET PROCESSEDTXNID = [ ' || pv_Txnid || ' ], PROCESSEDID = [ ' 
	|| pn_Id || ' ], TIMESTAMP = [ ' || TO_CHAR(d_CurrentDate, 'YYYY-MM-DD HH24:MI:SS.SSSS') || ' ], WAITING = [ 0 ] WHERE SESSIONNAME = [ ' || gTableName || ' ]';  
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	gLastProcessedTxn := pv_TxnId;    
	gLastProcessedId := pn_Id;   

	--IF pv_Status is NULL, then no need to check gTableType as SET_LAST_PROCESSED is being called from out of sync processing loop in REPLICATOR and no transaction
	--has processed so DATASTORESYNC does not need to be inserted into or deleted from
	IF pv_Status != cSTATUSROLLBACK	AND pv_Status IS NOT NULL THEN
		IF (gTableType = cUPDATES) THEN 
			DELETE FROM DATASTORESYNC WHERE PROCESSEDTXNID = pv_TxnId;  
			n_RowsAffected := SQL%ROWCOUNT;  

			v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] DELETE FROM DATASTORESYNC WHERE PROCESSEDTXNID =  [ ' || pv_TxnId || ' ]'; 
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		ELSE 
			INSERT INTO DATASTORESYNC(processedtxnid) VALUES (pv_TxnId); 
			n_RowsAffected := SQL%ROWCOUNT;    

			v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] INSERT INTO DATASTORESYNC(processedtxnid) VALUES ( [ ' || pv_TxnId || ' ] )';   
			LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
		END IF; 
	END IF;
EXCEPTION
WHEN OTHERS THEN
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;
END;    



--------------------------------------------------------------------
-- Name:        ARE_INSERTS_PENDING
-- Params:      <in> pv_TxnId
--
-- Descr:       Determines if INSERTS are pending. Used in synchronization.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_cnt NUMBER := 0 to n_Cnt NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Removed local variable v_LeastInsert VARCHAR2(20)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
-- Copyright Siemens 2025  
--

FUNCTION ARE_INSERTS_PENDING
(
	pv_TxnId                 		IN VARCHAR2
) 
RETURN BOOLEAN 
IS   
	b_WhiteListed           		BOOLEAN := FALSE; 
	n_Cnt                   		NUMBER := 0; 
	v_Err                   		VARCHAR2(4000); 
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
BEGIN    
	v_Loc := 'ARE_INSERTS_PENDING';
	v_Msg := 'Check to see if inserts are pending in DATASTORESYNC for PROCESSEDTXNID = ' || pv_TxnId;	

	SELECT COUNT(*)    
	INTO n_Cnt    
	FROM DATASTORESYNC    
	WHERE PROCESSEDTXNID = pv_TxnId;    

	IF (n_Cnt=0) THEN 
		RETURN TRUE;   
	ELSE   
			RETURN FALSE;  
	END IF;  
EXCEPTION 
WHEN OTHERS THEN
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (function when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (function when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;
END;    



--------------------------------------------------------------------
-- Name:        INSERT_MISSED_TXNS
-- Params:      <in> pv_Type            MISSING or NO ROWS AFFECTED
--              <in> pn_StartId         Starting Id
--              <in> pn_LastId          Ending Id (if a range. 0=Single Id)
--
-- Descr:       Inserts records into the DATASTOREMISSINGTXNS table. Either
--              a single record (pLastId=0) or a range.
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_ThisId NUMBER := pStartId to n_ThisId NUMBER := pn_StartId
--              04/05/2016      Dan Maloney      Changed local variable v_LastId NUMBER := pLastId to n_LastId NUMBER := pn_LastId
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
-- TO DO:  update functionality to differentiate between rolled back and  missing id 

PROCEDURE INSERT_MISSED_TXNS
(
	pv_Type                 		IN VARCHAR2,
	pn_StartId              		IN NUMBER,
	pn_LastId               		IN NUMBER DEFAULT 0
) 
IS    
	b_WhiteListed           		BOOLEAN := FALSE;
	n_LastId                		NUMBER := pn_LastId;  
	n_ThisId                		NUMBER := pn_StartId;    
	n_RowsAffected          		NUMBER := 0;
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
BEGIN   
	v_Loc := 'INSERT_MISSED_TXNS';
	v_Msg := 'Insert into DATASTOREMISSINGTXNS';

	IF pn_LastId = 0 THEN  
		n_LastId := pn_StartId; 
		v_Msg := 'pn_Lastid = 0.  Setting n_LastId to pn_startid [ ' || pn_StartId || ' ]';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
	END IF;    

	WHILE (n_ThisId <= n_LastId) 
	LOOP  
		v_Msg := 'n_ThisId [ ' || n_ThisId || ' ] n_LastId [ ' || n_LastId || ' ]';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

		INSERT INTO DATASTOREMISSINGTXNS(SESSIONNAME, MISSEDID, TYPE) VALUES (gTableName, n_ThisId, pv_Type);   
		n_RowsAffected := SQL%ROWCOUNT;

		v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] INSERT INTO DATASTOREMISSINGTXNS(SESSIONNAME, MISSEDID, TYPE) VALUES ( [ ' || gTableName || ' ], [ ' ||  n_ThisId || ' ], [ ' || pv_Type || ' ] )';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

		n_ThisId := n_ThisId + 1; 
	END LOOP; 
EXCEPTION 
WHEN OTHERS THEN 
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;
END;    





--------------------------------------------------------------------
-- Name:        LOG_ERROR
-- Params:      <in> pv_TxnId
--              <in> pv_SQLStmt
--              <in> pv_Message
--
-- Descr:       Stored procedure to write error to DataStoreErrors.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney     Added additional logging
--              04/05/2016      Dan Maloney     Changed IN parameter names
--              04/05/2016      Dan Maloney     Changed local variable v_SQL VARCHAR2(2000) := SUBSTR(pSQLStmt,1,400) to _SQL VARCHAR2(4000) := SUBSTR(pv_SQLStmt,1,4000)
--              04/05/2016      Dan Maloney     Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney     Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney     Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney     Added local variable v_Msg VARCHAR2(4000)
--              06/21/2017      Dan Maloney     Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
--
PROCEDURE LOG_ERROR
(
	pv_TxnId                		IN VARCHAR2, 
	pv_SQLStmt              		IN VARCHAR2, 
	pv_Err                  		IN VARCHAR2
) 
IS  
	PRAGMA AUTONOMOUS_TRANSACTION;  --JAL 
	b_WhiteListed           		BOOLEAN := FALSE; 
	n_RowsAffected          		NUMBER := 0;
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_SQL                   		VARCHAR2(4000) := SUBSTR(pv_SQLStmt,1,4000); 
 BEGIN  
	v_Loc := 'LOG_ERROR';
	v_Msg := 'INSERT INTO DATASTOREERRORS';

	INSERT INTO DATASTOREERRORS(SESSIONNAME, TXNID, MESSAGE, SQL) VALUES (gTableName, pv_TxnId, pv_Err, v_SQL);  
	n_RowsAffected := SQL%ROWCOUNT;  
	COMMIT; 

	v_Msg := 'Rows affected [ ' || n_RowsAffected || ' ] INSERT INTO DATASTOREERRORS: SESSIONNAME [ ' || gTableName || ' ] TXNID [ ' || pv_TxnId || ' ]';    
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	v_Msg := 'COMMIT : Issued after INSERT INTO DATASTOREERRORS';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);
EXCEPTION
WHEN OTHERS THEN
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);
		
	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc, cLogLevelMax);

	RAISE; 
END;    



--------------------------------------------------------------------
-- Name:        LOG_MESSAGE
-- Params:      <in> pv_Msg            Message text
--              <in> pv_Loc            Location in the code the message originates
--              <in> pi_LogLevel       Level that is compared to gLogLevel to determine if a messages is logged 
--
-- Descr:       Stored procedure to write a log message to DataStoreLog table.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              07/27/2017      Dan Maloney     Removed unused variabled v_Err, v_Loc, v_Msg
--
--
-- Copyright Siemens 2025  
--
PROCEDURE LOG_MESSAGE
(
	pv_Msg                  		IN VARCHAR2,
	pv_Loc                  		IN VARCHAR2 DEFAULT 'Unassigned',
	pi_LogLevel             		IN PLS_INTEGER DEFAULT 0
) 
IS    
	PRAGMA AUTONOMOUS_TRANSACTION;  --JAL
BEGIN   
	IF pi_LogLevel <= gLogLevel THEN
		gLogSeq := gLogSeq +1;	
		INSERT INTO DATASTORELOG(JOB, PACKAGE_EXECUTING, LOG_SEQ, LOG_LEVEL, LOC, MESSAGE)
		VALUES 
		(gJobId, tab_JobInfo(gJobId), gLogSeq, pi_LogLevel, pv_Loc, SUBSTR( REPLACE(REPLACE(pv_Msg,CHR(10),':'),CHR(13),':') ,1,4000));
	END IF;

	COMMIT;
EXCEPTION
WHEN OTHERS THEN
	ROLLBACK;

	RAISE;
END;    



--------------------------------------------------------------------
-- Name:        ENABLE
-- Params:      <in> pb_Enable (TRUE/FALSE)
--
-- Descr:       Stored procedure to set the 'DATASTORE_TERMINATE' parameter of the  DataStoreSetUp table.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Added local variable n_RowsAffected NUMBER := 0
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
--
PROCEDURE ENABLE
(
	pb_Enable                		IN BOOLEAN DEFAULT TRUE
) 
IS    
	PRAGMA AUTONOMOUS_TRANSACTION;  --JAL

	b_WhiteListed           		BOOLEAN := FALSE;
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_Terminate             		VARCHAR2(1);   
BEGIN  
	v_Loc := 'ENABLE';
	v_Msg := 'Evaluate pEnable and set v_Terminate and gTerminate';

	IF pb_Enable THEN    
		v_Terminate:=cNO;    
		gTerminate:=FALSE; 
	ELSE  
			v_Terminate:=cYES;    
			gTerminate:=TRUE;   
	END IF;    

	IF pb_Enable = FALSE THEN
		v_Msg := '*** DATASTORE STOPPED ***';
		LOG_MESSAGE(v_Msg, v_Loc, cLogLevelError);
	END IF;

	UPDATE DATASTORESETUP SET VALUE = v_Terminate WHERE PARAMETER = cPARM_TERMINATE;    
	COMMIT; 
EXCEPTION
WHEN OTHERS THEN
	ROLLBACK;

	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (procedure when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (procedure when others) : Exception : ' || SQLERRM; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Msg := 'Exception Handler (procedure when others) : ROLLBACK';
	LOG_MESSAGE(v_Msg, v_Loc,cLogLevelMax);

	RAISE;  
END;    


--------------------------------------------------------------------
-- Name:        CHECK_TERMINATE
-- Params:      None
-- Return:      TRUE/FALSE
--
-- Descr:       Queries 'DATASTORE_TERMINATE' and returns TRUE for 'Y' and FALSE for 'N'.  Executes on the ODS
--
-- History:
--              04/05/2016      Dan Maloney      Added additional logging
--              04/05/2016      Dan Maloney      Changed IN parameter names
--              04/05/2016      Dan Maloney      Changed local variable v_Val VARCHAR2(2) to v_Val VARCHAR2(512)
--              04/05/2016      Dan Maloney      Added local variable v_Err VARCHAR2(4000)
--              04/05/2016      Dan Maloney      Added local variable v_Loc VARCHAR2(128)
--              04/05/2016      Dan Maloney      Added local variable v_Msg VARCHAR2(4000)
--              06/21/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
--
FUNCTION CHECK_TERMINATE 
RETURN BOOLEAN 
IS    
	b_Ret                   		BOOLEAN;
	b_WhiteListed           		BOOLEAN := FALSE;
	v_Err                   		VARCHAR2(4000);     
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
	v_Val                   		VARCHAR2(512);  

BEGIN   
	v_Loc := 'CHECK_TERMINATE';
	v_Msg := 'Get DATASTORE_TERMINATE value';

	SELECT VALUE    
	INTO v_Val    
	FROM DATASTORESETUP    
	WHERE PARAMETER=cPARM_TERMINATE;    

	IF (v_Val=cYES) THEN    
		b_Ret:=TRUE;
	END IF;

	IF (v_Val=cNO) THEN    
		b_Ret:=FALSE;
	END IF;

	RETURN b_Ret;  
EXCEPTION 
WHEN OTHERS THEN
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (function when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (function when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;   
END;  




--------------------------------------------------------------------
-- Name:        IS_ERROR_WHITELISTED
-- Params:      <in> pb_SQLCODE (SQLCODE)
-- Return:      0TRUE/FALSE
--
-- Descr:       Queries DATASTOREWHITELIST to see SQLCODE (pn_SQLCODE) is in the table.  Returns TRUE if SQLCODE is white listed, otherwise the function returns FALSE.  Executes on the ODS
--
-- History:
--              06/14/2017      Dan Maloney      New function (US 51393)

--
--
-- Copyright Siemens 2025  
--
FUNCTION IS_ERROR_WHITELISTED
(
	pn_SQLCODE              		IN NUMBER
)
RETURN BOOLEAN 
IS    
	b_Ret                   		BOOLEAN;
	b_WhiteListed           		BOOLEAN := FALSE;
	n_Cnt                   		NUMBER;
	v_Err                   		VARCHAR2(4000);  
	v_Loc                   		VARCHAR2(128); 
	v_Msg                   		VARCHAR2(4000);  
BEGIN   
	v_Loc := 'IS_ERROR_WHITELISTED';
	v_Msg := 'Check to see is pn_SQLCODE (SQLCODE) is in DATASTOREWHITELIST table';

	SELECT COUNT(*)    
	INTO n_Cnt    
	FROM DATASTOREWHITELIST   
	WHERE ERRORID = pn_SQLCODE;    

	IF (n_Cnt = 0) THEN    
		b_Ret:=FALSE;
	ELSE
		b_Ret:=TRUE;
	END IF;

	RETURN b_Ret;  
EXCEPTION 
WHEN OTHERS THEN
	b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

	v_Err := 'Exception Handler (function when others) : Last message set : ' || v_Msg ; 
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	v_Err := 'Exception Handler (function when others) : Exception : ' || SQLERRM;
	LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

	RAISE;  
END;  




--Package Initialization
-- Descr:         Package initialization executes when packag loads into the session
--
-- History:
--              07/18/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--
--
-- Copyright Siemens 2025  
--
BEGIN
	DECLARE 
		b_WhiteListed           		BOOLEAN := FALSE;
		v_Err                   		VARCHAR2(4000);  
		v_Loc                   		VARCHAR2(128); 
		v_Msg                   		VARCHAR2(256);
		v_SQL                   		VARCHAR2(256);  
		v_ObjectName					VARCHAR2(128);
	BEGIN
		v_Loc := 'Package Initialization';

		v_Msg := 'Alter session to set NLS_DATE_FORMAT';
		v_SQL := 'ALTER SESSION SET NLS_DATE_FORMAT=''' || gDateFormat || '''';    
		EXECUTE IMMEDIATE v_SQL;  
 
		v_Msg := 'Alter session to set NLS_TIMESTAMP_FORMAT';
		v_SQL := 'ALTER SESSION SET NLS_TIMESTAMP_FORMAT=''' || gTimeStampFormat || '''';    
		EXECUTE IMMEDIATE v_SQL;  
		
		v_Msg := 'Populate associative array';
		--Populate associative array with the abbreviated JOB_NAME from USER_SCHEDULER_JOBS.  The assoicative array is indexed by the Job (i.e. Job Id) from USER_JOBS
		FOR cur_JobInfo IN
		(
			SELECT JOB_NAME, JOB_ACTION
			FROM USER_SCHEDULER_JOBS 
			WHERE UPPER(JOB_NAME) LIKE 'OPCENTER_DATASTORE%' ORDER BY JOB_NAME
		)
		LOOP
			tab_JobInfo(cur_JobInfo.JOB_NAME) := REPLACE(REPLACE(REPLACE(cur_JobInfo.JOB_ACTION,'BEGIN',''),'END;',''),' ','');
		END LOOP;
		
		v_Msg := 'Get SID and BG_JOB_ID';
		SELECT SYS_CONTEXT('USERENV','SID') INTO gSID FROM DUAL;
		SELECT SYS_CONTEXT('USERENV','BG_JOB_ID') INTO gJobId FROM DUAL; 
		SELECT OBJECT_NAME INTO v_ObjectName FROM USER_OBJECTS WHERE OBJECT_ID = gJobId;
		gJobId := v_ObjectName;

		v_Msg := 'Update job information in global array';
		gLogSeq := 0;
		LOG_MESSAGE('Update job information in global array', '[ 0 ] csiDataStorePackage Initialization', cLogLevelMax);

		v_Msg := 'INIT(NULL)';
		INIT(NULL);

		v_Msg := 'Update job information in global array';
		IF (gJobId IS NOT NULL) THEN
			LOG_MESSAGE('Update job information in global array', '[ 0 ] csiDataStorePackage Initialization', cLogLevelMax);
		END IF;
		
	EXCEPTION    
	WHEN OTHERS THEN 
		b_WhiteListed := IS_ERROR_WHITELISTED(SQLCODE);

		v_Err :=' Exception Handler (package body when others) : Last message set : ' || v_Msg ; 
		LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

		v_Err := 'Exception Handler (package body when others) : Exception : ' || SQLERRM;
		LOG_MESSAGE(v_Err, v_Loc, CASE b_WhiteListed WHEN TRUE THEN cLogLevelWhiteListed ELSE cLogLevelError END);

		IF (NOT b_WhiteListed) THEN
			--Disable DataStore
			ENABLE(FALSE);  
		END IF;  
	END;  
END CSIDATASTOREPACKAGE;
/

