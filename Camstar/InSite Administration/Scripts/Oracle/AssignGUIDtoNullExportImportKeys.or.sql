--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Copyright Siemens 2023  
-- 
--
--Description:
--	This script creates stored procedure csiAssignGUIDtoNullExpImpKeys that identifies tables in the schema that have an ExportImportKey column
--	and have a CDODefinition.StorageCategoryId that matches one of the input parameters that have a 1 value.  See Input Parameters
--	The stored procedure csiAssignGUIDtoNullExpImpKeys is created, then executed, then dropped 
 
--	Any row in a table containing the ExportImportKey that has a null value will be updated with a generated unique GUID
--      This is an all or none update.  If any row fails an update, the transaction rolls back any uncommitted updates.
--	This script can be rerun.  If there are no NULL values for ExportImportKey columns, then no rows will be updated
--	If NULL values still exist for ExportImportKey columns of the targeted table, those rows will be updated, however, rows already updated will not be updated again
--
--Execution:
--	Execute script via SQL Plus or SQL Developer as the application schema 
--
--	Note:  	This script should be executed as the application schema that owns objects that you want to drop.
--
--	EXAMPLE (SQL Developer interactive)			
--		Logged into SQL Developer as <User>, execute the script @AssignGUIDtoNullExportImportKeys.or.sql
--
--		SQL Developer will prompt the user to enter a value for &1, &2, &3, &4, &5, &6, &7 which will be assigned to the input parameters for the 
--		stored procedure csiAssignGUIDtoNullExpImpKeys as string values.  If the <ENTER> key is used to skip over a parameter it will be set to NULL which will
--		cause the parameter to take the default value defined by the input parameters of the stored procedure
--
--	EXAMPLE (SQL Plus command line)	
--		sqlplus <User>\<Password> @AssignGUIDtoNullExportImportKeys.or.sql
--
--		In the above example, the user will be prompter for the input parameter values as if run interactively from SQL Developer if the parameters are not supplied on the 
--		command line
--
--		sqlplus <User>\<Password> @AssignGUIDtoNullExportImportKeys.or.sql '' '' '' '' '' '' ''
--
--		In the above example, the user supplied '' (two single quotes side by side) for each input parameter value.  This is the same thing as sending a NULL value or
--		hitting the <ENTER> for the parameter value
--
--		sqlplus <User>\<Password> @AssignGUIDtoNullExportImportKeys.or.sql 1 0 0 1 0 0 0
--
--		In the above example, the user supplied the value of 0 or 1 for each of the input parameters
--
--
--	Note:		Executing SQL Plus from the command line to invoke this script requires all parameters be supplied on the command line.  If all parameters are not supplied, those missing
--			parameters will be prompted for interactively
--
--	Note: 		The recommended way to run AssignGUIDtoNullExportImportKeys.or.sql is by invoking AssignGUIDtoNullExportImportKeys.bat (use the batch file)
--			See the comments in the AssignGUIDtoNullExportImportKeys.bat to get the syntax to call AssignGUIDtoNullExportImportKeys.bat and pass in parameter values
--			The parameter values passed into the batch file, will get passed into the AssignGUIDtoNullExportImportKeys.or.sql file.  The batch file will default the
--			input parameter values if not supplied so that they need not be entered when calling the batch file
--
--Input Parameters:
--	When values are entered from the command line, if all values are not supplied, the user will be prompted to enter values for the missing parameters
--	When values are supplied from the command line, the way to accept a default value defined by the stored procedure for that parameter is to enter two single quotes side by side (i.e. '')
--	Values of 0 or 1 may be entered for each parameter from the command line.  Values other than 0 or 1 will cause the stored procedure to fail with a message indicating that an 
--	invalid value was passed as a parameter value
--
--	It is best to avoid using single quotes when entering values interactively or from the command line to avoid confusion, unless using '' from the command line to accept a default value for a parameter
--	Values of 0 or 1 or <ENTER> may be entered for each parameter interactively
--  	Hitting the <ENTER> key for a parameter value interactively will cause that value to take the default value defined by the stored procedure
--	
--
--	Note: if any value other than 1 or 0 is passed in as a parameter value, the stored procedure will fail input parameter value validation
--
--	NOTE:	Boolean values of TRUE and FALSE are not accepted.  TRUE and FALASE must be passed in as values 0 or 1.  
--Output:
--	Log File of script execution:  AssignGUIDtoNullExportImportKeys.or.log 
--
--Modification History:
--	Name				Date		Action
--	--------------------------	----------	----------------
--	dmaloney			2/15/2015	Modification to put logic into a stored procedure called csiAssignGUIDtoNullExpImpKeys
--							The stored procedure accepts (optional) input parameters and the script executes the stored procedure and then drops the stored procedure 
--  Alind				5/30/2018	tuning and log cleanup.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
WHENEVER SQLERROR EXIT
SET SERVEROUTPUT ON SIZE UNLIMITED
SET TIMING ON
SET LINESIZE 300

CREATE OR REPLACE PROCEDURE csiAssignGUIDtoNullExpImpKeys 
(	
	iparm_v_ModelingFlag 		VARCHAR2 DEFAULT '1',
	iparm_v_HistoryFlag			VARCHAR2 DEFAULT '0',
	iparm_v_SystemFlag			VARCHAR2 DEFAULT '0',
	iparm_v_TrackingFlag		VARCHAR2 DEFAULT '0',
	iparm_v_NoneFlag			VARCHAR2 DEFAULT '0',
	iparm_v_UndefinedFlag		VARCHAR2 DEFAULT '0',
	iparm_v_UnknownFlag			VARCHAR2 DEFAULT '0'
)
AS
	--
	--Declare Exceptions
	eInvalidInputParameter		EXCEPTION;
		PRAGMA EXCEPTION_INIT(eInvalidInputParameter, -20100);
	eUpdateError			EXCEPTION;
		PRAGMA EXCEPTION_INIT(eUpdateError, -20101);
	--
	nv_TableSchema 			NVARCHAR2(100);
	nv_TableName 			NVARCHAR2(100);
	nv_FullyQualTableName 	NVARCHAR2(200);
	v_SQL 					VARCHAR2(2000);
	--
	bi_RowsUpdated 			BINARY_INTEGER;
	bi_TotalRowsUpdated 	BINARY_INTEGER;
	bi_ModelingVal			BINARY_INTEGER;
	bi_HistoryVal			BINARY_INTEGER;
	bi_TrackingVal			BINARY_INTEGER;
	bi_SystemVal			BINARY_INTEGER;
	bi_NoneVal				BINARY_INTEGER;
	bi_UndefinedVal			BINARY_INTEGER;
	bi_UnknownVal			BINARY_INTEGER;	
	bi_TargetTablesUpdated	BINARY_INTEGER;
	bi_TargetTables			BINARY_INTEGER;
	n_ErrLocator			NUMBER := 0;

	CURSOR cur_TargetTableColumns 
	(	
		iparm_v_ModelingVal 	BINARY_INTEGER,
		iparm_v_HistoryVal 		BINARY_INTEGER,
		iparm_v_SystemVal		BINARY_INTEGER,
		iparm_v_TrackingVal		BINARY_INTEGER,
		iparm_v_NoneVal			BINARY_INTEGER,
		iparm_v_UndefinedVal 	BINARY_INTEGER,
		iparm_v_UnknownVal 		BINARY_INTEGER 	
	)
	IS 
	SELECT USER, UTABCOL.TABLE_NAME 
	FROM USER_TAB_COLUMNS UTABCOL,
	CDODEFINITION CDODEF
	WHERE UTABCOL.COLUMN_NAME = 'EXPORTIMPORTKEY' 
	AND UTABCOL.TABLE_NAME = UPPER(CDODEF.CDONAME)
	AND 
	( 
		CDODEF.STORAGECATEGORYID = iparm_v_ModelingVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_HistoryVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_SystemVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_TrackingVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_NoneVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_UndefinedVal OR
		CDODEF.STORAGECATEGORYID = iparm_v_UnknownVal 
	)
	ORDER BY UTABCOL.TABLE_NAME;
	--
BEGIN
	n_ErrLocator := 0;
	DBMS_OUTPUT.ENABLE(NULL);
	DBMS_OUTPUT.PUT_LINE( 'Start: [ ' || TO_CHAR(SYSDATE,'MM/DD/RRRR HH24:MI:SS') || ' ]' );
	bi_RowsUpdated := 0;
	bi_TotalRowsUpdated := 0;
	bi_TargetTablesUpdated := 0;
	bi_TargetTables := 0;
	--
	--Validate input parameters for valid values and translate varchar values to binary integer to match DBCategories.DBCategoryId
	n_ErrLocator := 1;
	IF iparm_v_ModelingFlag = '1' OR iparm_v_ModelingFlag IS NULL THEN
		bi_ModelingVal := 1;
	ELSIF iparm_v_ModelingFlag = '0' THEN
		bi_ModelingVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 2;
	IF iparm_v_HistoryFlag = '1' THEN
		bi_HistoryVal := 2;
	ELSIF iparm_v_HistoryFlag = '0' OR iparm_v_HistoryFlag IS NULL THEN
		bi_HistoryVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 3;
	IF iparm_v_SystemFlag = '1' THEN
		bi_SystemVal := 5;
	ELSIF iparm_v_SystemFlag = '0' OR iparm_v_SystemFlag IS NULL THEN
		bi_SystemVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 4;
	IF iparm_v_TrackingFlag = '1' THEN
		bi_TrackingVal := 6;
	ELSIF iparm_v_TrackingFlag = '0' OR iparm_v_TrackingFlag IS NULL THEN
		bi_TrackingVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 5;
	IF iparm_v_NoneFlag = '1' THEN
		bi_NoneVal := 8;
	ELSIF iparm_v_NoneFlag = '0' OR iparm_v_NoneFlag IS NULL THEN
		bi_NoneVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 6;
	IF iparm_v_UndefinedFlag = '1' THEN
		bi_UndefinedVal := 0;
	ELSIF iparm_v_UndefinedFlag = '0' OR iparm_v_UndefinedFlag IS NULL THEN
		bi_UndefinedVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 7;
	IF iparm_v_UnknownFlag = '1' THEN
		bi_UnknownVal := 4;
	ELSIF iparm_v_UnknownFlag = '0' OR iparm_v_UnknownFlag IS NULL  THEN
		bi_UnknownVal := -1;
	ELSE
		RAISE eInvalidInputParameter;
	END IF;
	--
	n_ErrLocator := 8;
	DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Input parameters Modeling:[' || NVL( iparm_v_ModelingFlag, '1' )
                               || '] ,History:[' || NVL( iparm_v_HistoryFlag, '0' )  
                               || '] ,System:[' || NVL( iparm_v_SystemFlag, ' 0' ) 
                               || '] ,Tracking:[' || NVL( iparm_v_TrackingFlag, '0' ) 
                               || '] ,None:[' || NVL( iparm_v_NoneFlag,  '0') 
                               || '] ,Undefined:[' || NVL( iparm_v_UndefinedFlag, '0' ) 
                               || '] ,Unknown:[' || NVL( iparm_v_UnknownFlag, '0' )|| ']');
	DBMS_OUTPUT.PUT_LINE( CHR(10) );
	--
	--
	n_ErrLocator := 9;
	--Open cursor whose result set contains table schema and table name of any table with a column called ExportImportKey and has a StorgaeCategorId that maps to
	--one of the input parameters that have a TRUE value
	--
	bi_TargetTablesUpdated := 0;
	OPEN cur_TargetTableColumns (bi_ModelingVal, bi_HistoryVal, bi_SystemVal, bi_TrackingVal, bi_NoneVal, bi_UndefinedVal, bi_UnknownVal);
	LOOP
		FETCH cur_TargetTableColumns INTO nv_TableSchema, nv_TableName;
		EXIT WHEN cur_TargetTableColumns%NOTFOUND;
		nv_FullyQualTableName := nv_TableSchema || '.' || nv_TableName;
		--
		DBMS_OUTPUT.PUT_LINE( CHR(9) || '[ ' || nv_FullyQualTableName || ' ]' );
		--
		n_ErrLocator := 10;
		v_SQL := ' update '||nv_FullyQualTableName||' set exportimportkey= sys_guid() where exportimportkey is null';
		EXECUTE IMMEDIATE v_SQL;
		bi_RowsUpdated := sql%rowcount;
		bi_TotalRowsUpdated:= bi_TotalRowsUpdated+bi_RowsUpdated;
		v_SQL := 'update '||nv_FullyQualTableName||' set exportimportkey = SUBSTR(exportimportkey,1,8) || ''-'' || SUBSTR(exportimportkey, 9, 4) || ''-'' || SUBSTR(exportimportkey, 13, 4) 
			|| ''-'' || SUBSTR(exportimportkey, 17, 4) || ''-'' || SUBSTR(exportimportkey, 21) where INSTR(exportimportkey,''-'')=0';
		EXECUTE IMMEDIATE v_SQL;
		COMMIT;
		--
		IF bi_RowsUpdated>0 THEN
			bi_TargetTablesUpdated := bi_TargetTablesUpdated +1;
			DBMS_OUTPUT.PUT_LINE( CHR(9) || CHR(9) ||CHR(9)|| ' Rows updated: [ ' || bi_RowsUpdated || ' ]');
		END IF;
	--
	END LOOP;
	--
  bi_TargetTables:=cur_TargetTableColumns%rowcount;
	n_ErrLocator := 12;
	IF cur_TargetTableColumns%ISOPEN THEN
		CLOSE cur_TargetTableColumns;
	END IF;
	--
	DBMS_OUTPUT.PUT_LINE( CHR(10) || CHR(9) || 'Target Tables with EXPORTIMPORTKEY column: [ ' || bi_TargetTables || ' ]' );
	DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Target Tables updated (i.e. EXPORTIMPORTKEY column contains NULL): [ ' || bi_TargetTablesUpdated || ' ]' );
	DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Total Rows Updated: [ ' || bi_TotalRowsUpdated || ' ]' );
	DBMS_OUTPUT.PUT_LINE( 'End: [ ' || TO_CHAR(SYSDATE,'MM/DD/RRRR HH24:MI:SS') || ']' );
	--
EXCEPTION 
	WHEN eInvalidInputParameter THEN
		DBMS_OUTPUT.PUT_LINE( CHR(10) );
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Execution error occured at location: [ ' || n_ErrLocator ||  ' ] ' || SQLCODE || ':' || REPLACE(REPLACE(SQLERRM,CHR(13),NULL), CHR(10),NULL));
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Invalid input parameter value');
		DBMS_OUTPUT.PUT_LINE( 'End: [ ' || TO_CHAR(SYSDATE,'MM/DD/RRRR HH24:MI:SS') || ']' );
		IF cur_TargetTableColumns%ISOPEN THEN
			CLOSE cur_TargetTableColumns;
		END IF;
		ROLLBACK;
	--
	WHEN eUpdateError THEN
		DBMS_OUTPUT.PUT_LINE( CHR(10) );
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Execution error occured at location: [ ' || n_ErrLocator ||  ' ] ' || SQLCODE || ':' || REPLACE(REPLACE(SQLERRM,CHR(13),NULL), CHR(10),NULL) );
		DBMS_OUTPUT.PUT_LINE( CHR(9) ||nv_FullyQualTableName|| ' Update Failed');
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Target Tables updated (i.e. EXPORTIMPORTKEY column contains NULL): [ ' || bi_TargetTablesUpdated || ' ]' );
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Total Rows Updated: [ ' || bi_TotalRowsUpdated || ' ]' );
		DBMS_OUTPUT.PUT_LINE( 'End: [ ' || TO_CHAR(SYSDATE,'MM/DD/RRRR HH24:MI:SS') || ']' );
		IF cur_TargetTableColumns%ISOPEN THEN
			CLOSE cur_TargetTableColumns;
		END IF;
		ROLLBACK;
	--
	WHEN OTHERS THEN
		DBMS_OUTPUT.PUT_LINE( CHR(10) );
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Execution error occured at location: [ ' || n_ErrLocator ||  ' ] ' || SQLCODE || ':' || REPLACE(REPLACE(SQLERRM,CHR(13),NULL), CHR(10),NULL));
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Target Tables updated (i.e. EXPORTIMPORTKEY column contains NULL): [ ' || bi_TargetTablesUpdated || ' ]' );
		DBMS_OUTPUT.PUT_LINE( CHR(9) || 'Total Rows Updated: [ ' || bi_TotalRowsUpdated || ' ]' );
		DBMS_OUTPUT.PUT_LINE( 'End: [ ' || TO_CHAR(SYSDATE,'MM/DD/RRRR HH24:MI:SS') || ']' );
		IF cur_TargetTableColumns%ISOPEN THEN
			CLOSE cur_TargetTableColumns;
		END IF;
		ROLLBACK;
	--
END;
/

--Execute stored procedure
SET VERIFY OFF

--Undefine and script variables used in this script
UNDEF IncModelingFlag
UNDEF IncHistoryFlag
UNDEF IncSystemFlag
UNDEF IncTrackingFlag
UNDEF IncNoneFlag
UNDEF IncUndefinedFlag
UNDEF IncUnknownFlag



PROMPT Include Modeling?  (default 1)  [ 1|0|<ENTER> ]  
DEFINE IncModelingFlag = '&1'
DEFINE IncModelingFlag 
PROMPT

PROMPT Include History? (default 0)   [ 1|0|<ENTER> ]  
DEFINE IncHistoryFlag = '&2'
DEFINE IncHistoryFlag 
PROMPT
PROMPT Include System? (default 0)    [ 1|0|<ENTER> ]   
DEFINE IncSystemFlag = '&3'
DEFINE IncSystemFlag 
PROMPT

PROMPT Include Tracking? (default 0)     [ 1|0|<ENTER> ]  
DEFINE IncTrackingFlag = '&4'
DEFINE IncTrackingFlag 
PROMPT

PROMPT Include Not set? (default 0)     [ 1|0|<ENTER> ]  
DEFINE IncNoneFlag = '&5'
DEFINE IncNoneFlag 
PROMPT

PROMPT Include Undefined? (default 0)     [ 1|0|<ENTER> ]   
DEFINE IncUndefinedFlag = '&6'
DEFINE IncUndefinedFlag 
PROMPT


PROMPT Include Unknown? (default 0)    [ 1|0|<ENTER> ]   
DEFINE IncUnknownFlag = '&7'
DEFINE IncUnknownFlag 
PROMPT


BEGIN
	csiAssignGUIDtoNullExpImpKeys (
		'&&IncModelingFlag', 
		'&&IncHistoryFlag', 
		'&&IncSystemFlag', 
		'&&IncTrackingFlag', 
		'&&IncNoneFlag', 
		'&&IncUndefinedFlag', 
		'&&IncUnknownFlag'
		);
END;
/

--Drop stored procedure
DROP PROCEDURE csiAssignGUIDtoNullExpImpKeys;

--Undefine and script variables used in this script
UNDEF 1
UNDEF 2
UNDEF 3
UNDEF 4
UNDEF 5
UNDEF 6
UNDEF 7
UNDEF IncModelingFlag
UNDEF IncHistoryFlag
UNDEF IncSystemFlag
UNDEF IncTrackingFlag
UNDEF IncNoneFlag
UNDEF IncUndefinedFlag
UNDEF IncUnknownFlag

SPOOL OFF

QUIT
