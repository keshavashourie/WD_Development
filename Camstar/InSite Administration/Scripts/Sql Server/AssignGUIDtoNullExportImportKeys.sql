--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Copyright Siemens 2023  
-- 
--
--Description:
--	This script creates the stored procedure csiAssignGUIDtoNullExpImpKeys that identifies tables in the schema that have an ExportImportKey column
--	and have a CDODefinition.StorageCategoryId that matches one of the input parameters that have a 1 value.  See Input Parameters
--	The stored procedure CSIAssignGUIDtoNullExpImpKeys is created, then executed, then dropped 
--
--	Any row in a table containing the ExportImportKey that has a null value will be updated with a generated unique GUID
--      This is an all or none update.  If any row fails an update, the transaction rolls back
--	This script can be rerun.  If there are no NULL values for ExportImportKey columns, then no rows will be updated
--	The script output shows the update SQL statements for any update of rows with a NULL value for ExportImportKey
--
--
--Execution:
--	This script should be executed as the application schema that owns objects that you want to drop.
--
--	Although this script was designed to be executed from the batch file AssignGUIDtoNullExportImportKeys.bat, there are a few other ways this script can be
--	executed directly without using the batch file AssignGUIDtoNullExportImportKeys.bat
--
--	1)	This script is designed to be executed from the batch file AssignGUIDtoNullExportImportKeys.bat.  See comments in AssignGUIDtoNullExportImportKeys.bat for details
--		on how to execute the batch file and the input parameter values to pass into the batch file.  If invoked via the batch file, this script file does not need any 
--		editing to comment or uncomment any code
--		See the section just before the EXEC statement in this script called  
--		{*** 1) Executing via batch file AssignGUIDtoNullExportImportKeys.bat ***}  
--
--	2)	This script can be run from SQL Server Management Studio in a New Query Window by enabling SQLCMD mode and uncommenting the :setvar commands in this script.  
--		See the section just before the EXEC statement in this script called  
--		{*** 2) SQL Server management Studio Execution in SQLCMD Mode ***}  
--	
--		SQLCMD mode can be enabled in SQL Server management Studio by going through the Tools -> Customize 
--		and selecting the Commands Tab.  Then select the Toolbar radio button and select SQL Editor from the drop down.  Then click the Add Command button.   Select 
--		Query from the list of Categories and select SQLCMD Mode from the list of command and click OK.  This will add a toolbar icon to your SQL Server Management Studio
--		You should enable SQLCMD mode (using this toolbar icon) to allow you to execute this script as this script uses :setvar commands which SQL Server Management Studio
--		normally does not recognize.  But with SQLCMD mode enabled, SQL Server Management Studio will understand these sqlcmd commands when you uncomment the :setvar commands
--
--		If executed this way, you must enable SQLCMD mode before executing this script and uncomment the :setvar commands and edit the :setvar variable values to
--		the values you want the stored procedure to accept, or leave the hard-coded default values you see in this script in the :setvar commands
--		
--		Failure to enable SQLCMD mode in SQL Server management Studio when the :setvar commands are uncommented will cause an execution error because the :setvar commands are
--		not recognized by SQL Server management Studio unless SQLCMD Mode is enabled
--
--		Failure to enable SQLCMD mode in SQL Server management Studio when the :setvar commands are commented out will cause an execution error because the :setvar commands need
--		to execute in order to assign values to the parameters PROC_ModelingFlag, PROC_HistoryFlag, PROC_SystemFlag, PROC_TrackingFlag, PROC_NoneFlag, PROC_UndefinedFlag, 
--		PROC_UNKNOWNFLAG
--
--	3)	This script can be run from SQL Server Management Studio in a New Query Window without enabling SQLCMD mode and leaving the :setvar commands commented out in this script.  
--		See the section just before the EXEC statement in this script called  
--		{*** 3) SQL Server management Studio Execution NOT in SQLCMD Mode ***} 
--
--		If executed this way, leave the :setvar commands commented out and comment out the EXEC csiAssignGUIDtoNullExpImpKeys statement that passes the 
--		$(PROC_ModelingFlag), $(PROC_HistoryFlag), $(PROC_SystemFlag), $(PROC_TrackingFlag), $(PROC_NoneFlag), $(PROC_UndefinedFlag), and $(PROC_UnknownFlag) parameters
--		Instead, execute the stored procedure as defined in the comment block {*** 3) SQL Server management Studio Execution NOT in SQLCMD Mode ***} 
--
--	4) 	This script can be executed using sqlcmd utility from a DOS window.   If run from sqlcmd in a DOS window, the input parameters to the stored procedure 
--		csiAssignGUIDtoNullExpImpKeys must be passed in using the -v option of sqlcmd and explicitly name the parameters so they match the parameter names in the EXEC call
--		to the stored procedure PROC_ModelingFlag, PROC_HistoryFlag, PROC_SystemFlag, PROC_TrackingFlag, PROC_NoneFlag, PROC_UndefinedFlag, PROC_UNKNOWNFLAG 
--		See the section just before the EXEC statement in this script called  
--		{*** 4) sqlcmd utility using -v to pass in parameter valued ***}  
--
--		If the environment variables are not passed in using the -v option with the named parameters as shown in the example above, when using sqlcmd, an error message will be 
--		returned to the DOS window stating that these variables are undefined (i.e. PROC_ModelingFlag, PROC_HistoryFlag, PROC_SystemFlag, PROC_TrackingFlag, PROC_NoneFlag, 
--		PROC_UndefinedFlag, PROC_UnknownFlag)
--
--		EXAMPLE (sqlcmd utility from DOS Window):	
--		sqlcmd -S <Database Server> -d <Database> -U <User> -P <Password> -i AssignGUIDtoNullExportImportKeys.sql -v PROC_ModelingFlag=<[1|0]> ... continued on next line
--		PROC_HistoryFlag=<[1|0]> PROC_SystemFlag=<[1|0]> PROC_TrackingFlag=<[1|0]> PROC_NoneFlag=<[1|0]> PROC_UndefinedFlag=<[1|0]> PROC_UnknownFlag=<[1|0]> 
--
--	5)	This script can be executed via SQL Server Management Studio using the Execute Stored Procedure from the Object Explorer.
--		If executed this way, SQL Server Management Studio will display a window in which you can enter the values for the input parameters to the stored procedure
--
--Input Parameters:
--	When values are entered from the DOS Window command line using sqlcmd, if no parameters are passed via the -v option of sqlcmd, an error will be displayed in the DOS window
--	names input parameters are not defined
--	When values are supplied from the command line with the -v sqlcmd option, the values defined in PROC_ModelingFlag, PROC_HistoryFlag, PROC_SystemFlag, 
--	PROC_TrackingFlag, PROC_NoneFlag, PROC_UndefinedFlag, PROC_UnknownFlag are passed to the stored procedure.  If you do not set these environment variables with the SET
--	DOS command in the DOS window, or name them in the -v option of sqlcmd (as shown in the execution example above), an error will be displayed in the DOS window stating that these
--	names input parameters are not defined
--
--
--	PROC_ModelingFlag (in batch file or from :setvar command) passes into iparm_bit_ModelingFlag (in stored procedure)	DEFAULT '1' 	
--	PROC_HistoryFlag (in batch file or from :setvar command) passes into iparm_v_IncHistoryFlag (in stored procedure)	DEFAULT '0' 	
--	PROC_SystemFlag (in batch file or from :setvar command) passes into iparm_v_IncSystemFlag (in stored procedure)		DEFAULT '0'	
--	PROC_TrackingFlag (in batch file or from :setvar command) passes into iparm_v_IncTrackingFlag (in stored procedure)	DEFAULT	'0'	
--	PROC_NoneFlag (in batch file or from :setvar command) passes into iparm_v_IncNoneFlag (in stored procedure)		DEFAULT '0'	
--	PROC_UndefinedFlag (in batch file or from :setvar command) passes into iparm_v_IncUndefinedFlag (in stored procedure)	DEFAULT	'0'	
--	PROC_UnknownFlag (in batch file or from :setvar command) passes into iparm_v_IncUnknownFlag (in stored procedure)	DEFAULT '0'	
--	
--	In all the examples above, [1|0] are valid for each input parameter 
--Output:
--	Log File of script execution:  AssignGUIDtoNullExportImportKeys.sql.log 
--
--Modification History:
--	Name				Date		Action
--	--------------------------	----------	----------------
--	dmaloney			2/18/2015	Modification to put logic into a stored procedure called csiAssignGUIDtoNullExpImpKeys
--							The stored procedure accepts (optional) input parameters and the script executes the stored procedure and then drops the 
--							stored procedure
-- ALind				5/29/2018   Performance tuning.  
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
SET NOCOUNT ON	


IF OBJECT_ID(N'csiAssignGUIDtoNullExpImpKeys',N'P') IS NOT NULL
	DROP PROCEDURE csiAssignGUIDtoNullExpImpKeys;
GO

CREATE PROCEDURE csiAssignGUIDtoNullExpImpKeys 
	@iparm_bit_ModelingFlag 		BIT = 1,
	@iparm_bit_HistoryFlag			BIT = 0,
	@iparm_bit_SystemFlag			BIT = 0,
	@iparm_bit_TrackingFlag			BIT = 0,
	@iparm_bit_NoneFlag			BIT = 0,
	@iparm_bit_UndefinedFlag		BIT = 0,
	@iparm_bit_UnknownFlag			BIT = 0
AS
	DECLARE @CURRENTSCHEMA			SYSNAME;
	DECLARE @CURRENTDB			SYSNAME;
	DECLARE @nv_TableSchema			NVARCHAR(100);
	DECLARE @nv_TableName			NVARCHAR(100);
	DECLARE @nv_FullyQualTableName		NVARCHAR(200);
	DECLARE @nv_SQL				NVARCHAR(2000);
	--
	DECLARE @i_RowsUpdated			INT = 0;
	DECLARE @i_TotalRowsUpdated		INT = 0;
	DECLARE @i_ModelingVal	 		INT = 1;
	DECLARE @i_HistoryVal			INT = 0;
	DECLARE @i_SystemVal			INT = 0;
	DECLARE @i_TrackingVal			INT = 0;
	DECLARE @i_NoneVal			INT = 0;
	DECLARE @i_UndefinedVal			INT = 0;
	DECLARE @i_UnknownVal			INT = 0;
	DECLARE @i_TargetTablesUpdated		INT = 0;
	DECLARE @i_TargetTables			INT = 0;
	DECLARE @i_ErrLocator			INT = 0;
	DECLARE @t_runtime			DateTime;
	--
  	--
BEGIN
	--
	--
	SET @t_runtime = SYSDATETIME();
	SET @i_ErrLocator = 0;
	PRINT N'Start: [ ' + CONVERT(NVARCHAR(50),@t_runtime,109) + N' ]';
	PRINT CHAR(13);
	SET @CURRENTSCHEMA = SCHEMA_NAME();
	SET @CURRENTDB = DB_NAME();
	SET @i_RowsUpdated = 0;
	SET @i_TotalRowsUpdated = 0;
	SET @i_TargetTablesUpdated = 0;
	SET @i_TargetTables = 0;
	--
	--Validate input parameters for valid values and translate varchar values to binary integer to match DBCategories.DBCategoryId
	SET @i_ErrLocator = 1;
	IF ISNULL(@iparm_bit_ModelingFlag, 1) = 1 
		SET @i_ModelingVal = 1;
	ELSE
		SET @i_ModelingVal = -1;
	--
	SET @i_ErrLocator = 2;
	IF ISNULL(@iparm_bit_HistoryFlag, 0) = 1 
		SET @i_HistoryVal = 2;
	ELSE
		SET @i_HistoryVal = -1;
	--
	SET @i_ErrLocator = 3;
	IF ISNULL(@iparm_bit_SystemFlag, 0) = 1 
		SET @i_SystemVal = 5;
	ELSE
		SET @i_SystemVal = -1;
	--
	SET @i_ErrLocator = 4;
	IF ISNULL(@iparm_bit_TrackingFlag, 0) = 1 
		SET @i_TrackingVal = 6;
	ELSE
		SET @i_TrackingVal = -1;
	--
	SET @i_ErrLocator = 5;
	IF ISNULL(@iparm_bit_NoneFlag, 0) = 1 
		SET @i_NoneVal = 8;
	ELSE
		SET @i_NoneVal = -1;
	--
	SET @i_ErrLocator = 6;
	IF ISNULL(@iparm_bit_UndefinedFlag, 0) = 1 
		SET @i_UndefinedVal = 0;
	ELSE
		SET @i_UndefinedVal = -1;
	--
	SET @i_ErrLocator = 7;
		IF ISNULL(@iparm_bit_UnknownFlag, 0) = 1 
		SET @i_UnknownVal = 4;
	ELSE
		SET @i_UnknownVal = -1;
	--
	SET @i_ErrLocator = 8;
	PRINT CHAR(9)	+ N'Parameters used: Modeling: [' +  CONVERT(CHAR(1),@iparm_bit_ModelingFlag) 
					+ N'], History: [' +  CONVERT(CHAR(1),@iparm_bit_HistoryFlag) 
					+ N'], System: [' +  CONVERT(CHAR(1),@iparm_bit_SystemFlag) 
					+ N'], Tracking: [' +  CONVERT(CHAR(1),@iparm_bit_TrackingFlag) 
					+ N'], None: [' +  CONVERT(CHAR(1),@iparm_bit_NoneFlag) 
					+ N'], Undefined: [' +  CONVERT(CHAR(1),@iparm_bit_UndefinedFlag) 
					+ N'], Unknown: [' +  CONVERT(CHAR(1),@iparm_bit_UnknownFlag) + N'] ';
	PRINT CHAR(13);
	PRINT CHAR(13);
	--
	DECLARE cur_TargetTableColumns CURSOR LOCAL FORWARD_ONLY STATIC READ_ONLY
	FOR SELECT UTABCOL.TABLE_SCHEMA, UTABCOL.TABLE_NAME 
	FROM INFORMATION_SCHEMA.COLUMNS UTABCOL, CDODEFINITION CDODEF
	WHERE UPPER(UTABCOL.COLUMN_NAME) = 'EXPORTIMPORTKEY' 
	AND UPPER(UTABCOL.TABLE_NAME) = UPPER(CDODEF.CDONAME)
	AND 
	( 
		CDODEF.STORAGECATEGORYID = @i_ModelingVal OR
		CDODEF.STORAGECATEGORYID = @i_HistoryVal OR
		CDODEF.STORAGECATEGORYID = @i_SystemVal OR
		CDODEF.STORAGECATEGORYID = @i_TrackingVal OR
		CDODEF.STORAGECATEGORYID = @i_NoneVal OR
		CDODEF.STORAGECATEGORYID = @i_UndefinedVal OR
		CDODEF.STORAGECATEGORYID = @i_UnknownVal 
	)
	ORDER BY UTABCOL.TABLE_SCHEMA, UTABCOL.TABLE_NAME;
	--
	SET @i_ErrLocator = 9;
	--Open cursor with result set contains table schema and table name of any table with a column called ExportImportKey
	--
	SET @i_TargetTablesUpdated = 0;
	OPEN cur_TargetTableColumns;
	SET @i_TargetTables = @@CURSOR_ROWS;
	IF @@CURSOR_ROWS > 0 
	BEGIN
		FETCH NEXT FROM cur_TargetTableColumns INTO @nv_TableSchema, @nv_TableName;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @nv_FullyQualTableName = @nv_TableSchema + N'.' + @nv_TableName;
			--
			PRINT CHAR(9) + N'[ ' + @nv_FullyQualTableName + N' ]';
			--
			SET @nv_SQL = N'UPDATE ' + @nv_FullyQualTableName + N' SET ExportImportKey = NewID()  WHERE ExportImportKey IS NULL ';
			BEGIN TRY	
					BEGIN TRAN
					SET @i_ErrLocator = 11;
					EXEC sp_executesql @nv_SQL;
					SET @i_RowsUpdated = @@ROWCOUNT;
					COMMIT TRAN
                 	if(@i_RowsUpdated >0)
					BEGIN		
						SET @i_TotalRowsUpdated= @i_TotalRowsUpdated +@i_RowsUpdated;
						PRINT CHAR(9) + CHAR(9) +N' Rows Updated: [ ' + CONVERT(VARCHAR,@i_RowsUpdated) + N' ]';
						SET @i_TargetTablesUpdated = @i_TargetTablesUpdated +1;
					END
			END TRY
			BEGIN CATCH
					IF @@TRANCOUNT = 1
						ROLLBACK TRAN;
					--An update failed, close cursor and rollback transaction and exit
					PRINT CHAR(13);
					PRINT CHAR(9) + N'Execution error occured at location: [ ' + CONVERT(VARCHAR,@i_ErrLocator) + ' ] ' + CONVERT(VARCHAR,ERROR_NUMBER()) + N':' + ERROR_MESSAGE();
					PRINT CHAR(9) +@nv_FullyQualTableName+ N'Update Failed';
                 			PRINT CHAR(9) + N'Tables Updated: [ ' + CONVERT(VARCHAR,@i_TargetTablesUpdated) + N' ]';
					PRINT CHAR(9) + N'Total Rows Updated: [ ' + CONVERT(VARCHAR,@i_TotalRowsUpdated) + N' ]';
					PRINT N'End: [ ' + CONVERT(NVARCHAR(50),SYSDATETIME(),109) + N' ]';
					PRINT N' Runtime: [ '+ CONVERT(varchar, DATEADD(ms, datediff(ms,@t_runtime, SYSDATETIME()), 0), 114) + N' ]';
					CLOSE cur_TargetTableColumns;
					DEALLOCATE cur_TargetTableColumns;
					RETURN;
			END CATCH
				--
		FETCH NEXT FROM cur_TargetTableColumns INTO @nv_TableSchema, @nv_TableName;
		--
	END;
	CLOSE cur_TargetTableColumns;
	DEALLOCATE cur_TargetTableColumns;
	END;
	--
	PRINT CHAR(13);
	PRINT  N'Target Tables with EXPORTIMPORTKEY column: [ ' + CONVERT(VARCHAR,@i_TargetTables) + N' ]';
	PRINT  N'Target Tables Updated (i.e. EXPORTIMPORTKEY column value was NULL): [ ' + CONVERT(VARCHAR,@i_TargetTablesUpdated) + N' ]';
	PRINT N'Total Rows Updated: [ ' + CONVERT(VARCHAR,@i_TotalRowsUpdated) + N' ]';
	PRINT N'End: [ ' + CONVERT(NVARCHAR(50),SYSDATETIME(),109) + N' ]';
	PRINT N'Runtime: [ '+ CONVERT(varchar, DATEADD(ms, datediff(ms,@t_runtime, SYSDATETIME()), 0), 114) + N' ]';
END;

GO

--{*** 1) Executing via batch file AssignGUIDtoNullExportImportKeys.bat ***} 
--	If executing via the AssignGUIDtoNullExportImportKeys.bat batch file, leave the :setvar commands commented out.  If you uncomment the :setvar commands below
--	to execute this sql script, the local :setvar commands below will override the parameter values the batch file passes in.  This is because the local :setvar commands take 
--	higher precedence over the values passed in from the batch file.  This is not the desired behavior so it is important to keep the :setvar commands below commented out 
--	when executing this script via the batch file AssignGUIDtoNullExportImportKeys.bat 

--{*** 2) SQL Server management Studio Execution in SQLCMD Mode ***}  
--	If executed this way, you must enable SQLCMD mode before executing this script and uncomment the :setvar commands and edit the :setvar variable values to
--	the values you want the stored procedure to accept, or leave the hard-coded default values you see in this script in the :setvar commands


--{*** 3) SQL Server management Studio Execution NOT in SQLCMD Mode ***} 
--	If executed this way, leave the :setvar commands commented out and comment out the EXEC csiAssignGUIDtoNullExpImpKeys statement that passes the 
--	$(PROC_ModelingFlag), $(PROC_HistoryFlag), $(PROC_SystemFlag), $(PROC_TrackingFlag), $(PROC_NoneFlag), $(PROC_UndefinedFlag), and $(PROC_UnknownFlag) parameters
--	Instead, the second EXEC csiAssignGUIDtoNullExpImpKeys statement and edit the input parameters 1 or 0 values 

--{*** 4) sqlcmd utility using -v to pass in parameter valued ***} 
--	If executing this sql script (AssignGUIDtoNullExportImportKeys.sql) from a DOS Window using sqlcmd instead of using the batch file, leave the :setvar commands commented out 
--	Use the -v flag of sqlcmd to specify a 1 or 0 value for each of the stored procedure parameters
--	EXAMPLE:	
--		sqlcmd -S <Database Server> -d <Database> -U <User> -P <Password> -i AssignGUIDtoNullExportImportKeys.sql -v PROC_ModelingFlag=<[1|0]> ... continued on next line
--		PROC_HistoryFlag=<[1|0]> PROC_SystemFlag=<[1|0]> PROC_TrackingFlag=<[1|0]> PROC_NoneFlag=<[1|0]> PROC_UndefinedFlag=<[1|0]> PROC_UnknownFlag=<[1|0]> 



--Execution block.  Comment or uncomment the :setvar commands and the EXEC statements immediately below this comment in accordance with the execution method described above in 
--items 1), 2), 3) or 4)

--:setvar PROC_ModelingFlag 1
--:setvar PROC_HistoryFlag 0
--:setvar PROC_SystemFlag 0
--:setvar PROC_TrackingFlag 0
--:setvar PROC_NoneFlag 0
--:setvar PROC_UndefinedFlag 0
--:setvar PROC_UnknownFlag 0

EXEC csiAssignGUIDtoNullExpImpKeys $(PROC_ModelingFlag), $(PROC_HistoryFlag), $(PROC_SystemFlag), $(PROC_TrackingFlag), $(PROC_NoneFlag), $(PROC_UndefinedFlag), $(PROC_UnknownFlag);
--EXEC csiAssignGUIDtoNullExpImpKeys 1,0,0,0,0,0,0;
GO



--	Drops the stored procedure after it is executed
IF OBJECT_ID(N'csiAssignGUIDtoNullExpImpKeys',N'P') IS NOT NULL
	DROP PROCEDURE csiAssignGUIDtoNullExpImpKeys;
GO



SET NOCOUNT OFF
