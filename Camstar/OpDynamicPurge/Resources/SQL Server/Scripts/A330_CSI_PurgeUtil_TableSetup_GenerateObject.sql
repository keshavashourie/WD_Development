ALTER PROCEDURE CSI_PurgeUtil_TableSetup_GenerateObject 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_GenerateObject
                     Create delete/restore stored procedure on the fly
					 based on the delete/restore template and the table setup.
					 In other words, this SP is a code generator.
  Author           : Benny.Chia 
  Date             : 13 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName                  NVARCHAR(40)
	, @pvRootTableName              NVARCHAR(40)  -- 1st level table name
	, @pvSourceDBName        		NVARCHAR(40)
	, @pvArchiveDBName			    NVARCHAR(40)
	, @pvSourceSchemaName    		NVARCHAR(40)
	, @pvArchiveSchemaName			NVARCHAR(40)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @vBatchExecutionId          NVARCHAR(16);
-- Hierarchy Data ---------------------------------------------------------------------
DECLARE @vHierarchyData_tab          TABLE (RowID int not null primary key identity(1,1)
                                          , Level INTEGER
                                          , tablename NVARCHAR(30)
                                          , instancecol1A NVARCHAR(30)
                                          , instancecol1B NVARCHAR(30)
                                          , instancecol1C NVARCHAR(30)
                                          , instancecol2 NVARCHAR(30)
                                          , instanceparentcol NVARCHAR(30)
                                          , parenttable NVARCHAR(30)
                                          , ParentTableLinkCol NVARCHAR(30)
                                          , CommitBatchSize INTEGER
                                          , SQLQueryPredicate NVARCHAR(40)
                                            );
DECLARE @vHierarchyData          NVARCHAR(MAX);
DECLARE @vHierarchyRowsToProcess INTEGER;
DECLARE @vHierarchyCurrentRow    INTEGER;
-- DeleteTpl Data -----------------------------------------------------------------
DECLARE @vDeleteTplData_tab      CSI_PurgeUtil_RowData_Tab;
DECLARE @vDeleteTplData          NVARCHAR(MAX);
DECLARE @vDeleteTplRowsToProcess INTEGER;
DECLARE @vDeleteTplCurrentRow    INTEGER;
-- RestoreTpl Data -----------------------------------------------------------------
DECLARE @vRestoreTplData_tab      CSI_PurgeUtil_RowData_Tab;
DECLARE @vRestoreTplData          NVARCHAR(MAX);
DECLARE @vRestoreTplRowsToProcess INTEGER;
DECLARE @vRestoreTplCurrentRow    INTEGER;
-- DeleteByTableTpl Data -----------------------------------------------------------------
DECLARE @vDeleteByTableTplData_tab      CSI_PurgeUtil_RowData_Tab;
DECLARE @vDeleteByTableTplData          NVARCHAR(MAX);
DECLARE @vDeleteByTableTplRowsToProcess INTEGER;
DECLARE @vDeleteByTableTplCurrentRow    INTEGER;
--
-- Scripts Data for creating CSI_PURGEUTIL_DELE_ + @pvSetupName stored proedure on the fly -----
DECLARE @vScriptsDELEData_tab      CSI_PurgeUtil_RowData_Tab;
-- Scripts Data for creating CSI_PURGEUTIL_REST_ + @pvSetupName stored proedure on the fly -----
DECLARE @vScriptsRESTData_tab      CSI_PurgeUtil_RowData_Tab;
-- Scripts Data for creating CSI_PURGEUTIL_DELEBYTABLE_ + @pvSetupName stored proedure on the fly -----
DECLARE @vScriptsDELEBYTABLEData_tab      CSI_PurgeUtil_RowData_Tab;
--
DECLARE @vLevel                       INTEGER=0;
DECLARE @vTablename                   NVARCHAR(30);
DECLARE @vInstanceCol1A               NVARCHAR(30);
DECLARE @vInstanceCol1B               NVARCHAR(30);
DECLARE @vInstanceCol1C               NVARCHAR(30);
DECLARE @vInstanceCol2                NVARCHAR(30);
DECLARE @vInstanceParentCol           NVARCHAR(30);
DECLARE @vParentTable                 NVARCHAR(30);
DECLARE @vParentTableString           NVARCHAR(40);
DECLARE @vParentTableLinkCol          NVARCHAR(30);
------------------------------------------------------------------------------------
DECLARE @vColNames                   NVARCHAR(max)='';
DECLARE @vDMLOption                  NVARCHAR(255)=' ';
DECLARE @vLockType                   NVARCHAR(255)=' ';
--
DECLARE @vCommitBatchSizePerTable    INT=1000;
DECLARE @vSQLQueryPredicate          NVARCHAR(40)='';
DECLARE @vPurgeRequired              INTEGER;
DECLARE @vArchiveRequired            INTEGER;
DECLARE @vSQLStatement               NVARCHAR(MAX);
DECLARE @vValidateExecutionRule1     NVARCHAR(2000)='';
DECLARE @vValidateExecutionRule2     NVARCHAR(2000)='';
DECLARE @vValidateExecutionRule3     NVARCHAR(2000)='';
DECLARE @vValidateExecutionRule      NVARCHAR(3000)='';
DECLARE @vLogSQLStatement1           NVARCHAR(MAX)='';
DECLARE @vLogSQLStatement2           NVARCHAR(MAX)='';
DECLARE @vLogSQLStatement3           NVARCHAR(MAX)='';
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvSetupName IS Null 
			RAISERROR ('SetupName must not be blank', 16, 1);
		IF @pvRootTableName IS Null 
			RAISERROR ('RootTableName must not be blank', 16, 1);
		IF @pvSourceDBName IS Null 
			RAISERROR ('Source Database Name must not be blank', 16, 1);
		IF @pvArchiveDBName IS Null 
			RAISERROR ('Archive Database Name must not be blank', 16, 1);
		IF @pvSourceSchemaName IS Null 
			RAISERROR ('SourceSchemaName must not be blank', 16, 1);
		IF @pvArchiveSchemaName IS Null 
			RAISERROR ('ArchiveSchemaName must not be blank', 16, 1);
		--
		---------------------------------------------------------------------------
		BEGIN TRY
		    SELECT DISTINCT
		        @vPurgeRequired = su.PurgeRequired
		        , @vArchiveRequired = su.ArchiveRequired
		        , @vDMLOption = su.DMLOption
		        , @vLockType = su.LockType
		        , @vValidateExecutionRule1 = su.ValidateExecutionRule1
		        , @vValidateExecutionRule2 = su.ValidateExecutionRule2
		        , @vValidateExecutionRule3 = su.ValidateExecutionRule3
		    FROM CSI_PURGEUTIL_SETUP su 
		    WHERE UPPER(su.SetupName) = UPPER(@pvSetupName);
        END TRY
        BEGIN CATCH
            SET @vDMLOption = ' ';
            SET @vLockType = ' ';
            SET @vValidateExecutionRule1 = '';
            SET @vValidateExecutionRule2 = '';
            SET @vValidateExecutionRule3 = '';
        END CATCH
		--
		---------------------------------------------------------------------------
        BEGIN TRY 
            ;  -- for SQLServer 2008, a semi colon is needed just before the WITH predicate.   Semi colon is not needed for SQLServer 2012 but we can leave semi colon here.
            WITH vtHierarchyList -- Rows of records with different level starting from the Root table, otherwise known as the Level 1 table.
            AS 
            (
                -- Anchor member definition
                SELECT st.tablelevel AS Level
		            , st.tablename
                    , st.instancecol1A    -- primary key 1st column 
                    , st.instancecol1B    -- primary key 2nd column 
                    , st.instancecol1C    -- primary key 3rd column
		            , st.instancecol2       -- alternate key
		            , st.instanceparentcol  -- foreign key
		            , st.parenttable
		            , st.parenttablelinkcol 
		            , st.CommitBatchSize
                    , st.SQLQueryPredicate
                FROM CSI_PURGEUTIL_SETUPTABLES st
                INNER JOIN CSI_PURGEUTIL_SETUP su ON su.setupid = st.setupid
                WHERE UPPER(su.setupname) = UPPER(@pvSetupName)
            )
            INSERT INTO @vHierarchyData_tab
                ( Level
                , tablename
                , instancecol1A
                , instancecol1B
                , instancecol1C
                , instancecol2
                , instanceparentcol
                , parenttable
                , parenttablelinkcol
                , CommitBatchSize
                , SQLQueryPredicate
                ) 
            SELECT DISTINCT vhl.*
            FROM vtHierarchyList vhl
            ORDER BY vhl.Level asc, vhl.parenttable asc  -- must be ascending
            ;
            --
            SET @vHierarchyRowsToProcess = @@ROWCOUNT;  -- the number of rows that is inserted into @vHierarchyData_tab
            --SELECT * FROM @vHierarchyData_tab;
            --PRINT @vHierarchyRowsToProcess;
 	    END TRY
		BEGIN CATCH
			BEGIN
		        SET @ErrorMessage = ERROR_MESSAGE(); 
    		    SET @vProgID = @vObject_Name + '.' + 'INSERT INTO @vHierarchyData_tab(RowData) '; 
        		RAISERROR (@ErrorMessage, 16, 1);
			END;
		END CATCH
		--
        --------------------------------------------------------------------------
        -- CSI_PURGEUTIL_DELEBYTABLE_TPL template stored procedure
	    ---------------------------------------------------------------------------
        BEGIN TRY 
            INSERT INTO @vDeleteByTableTplData_tab(RowData) 
            EXECUTE sp_helptext @objname = 'CSI_PURGEUTIL_DELEBYTABLE_TPL';  -- getting the compiled source code
            SET @vDeleteByTableTplRowsToProcess = @@ROWCOUNT;  -- the number of rows that is inserted into @vDeleteByTableTplData_tab
            --PRINT '@vDeleteByTableTplRowsToProcess=' + STR(@vDeleteByTableTplRowsToProcess);
        END TRY
	    BEGIN CATCH
		    BEGIN
			    SET @ErrorMessage = ERROR_MESSAGE(); 
			    SET @vProgID = @vObject_Name + '.' + 'DELETEBYTABLE:INSERT INTO @vDeleteByTableTplData_tab(RowData) '; 
        		RAISERROR (@ErrorMessage, 16, 1);
		    END;
	    END CATCH
        --SELECT * FROM @vDeleteByTableTplData_tab;
        --
	    ---------------------------------------------------------------------------
        -- Inserting rows into the table @vScriptsDELEBYTABLEData_tab based on the template stored procedure
        -- and table @vHierarchyData_tab.
	    ---------------------------------------------------------------------------
        BEGIN TRY 
            SET @vDeleteByTableTplCurrentRow = 0;
            --PRINT @vScriptsDELEBYTABLEData_tab;
            WHILE @vDeleteByTableTplCurrentRow < @vDeleteByTableTplRowsToProcess
            BEGIN
                SET @vDeleteByTableTplCurrentRow = @vDeleteByTableTplCurrentRow + 1;
                SELECT DISTINCT
                    @vDeleteByTableTplData = RowData 
                FROM @vDeleteByTableTplData_tab
                WHERE RowID = @vDeleteByTableTplCurrentRow;
                --PRINT @vDeleteByTableTplData;
                --
                ---------------------------------------------------------------------------
                -- Inserting lines from the source code template to create SP CSI_PurgeUtil_DeleByTable_' + @pvSetupName;
                ---------------------------------------------------------------------------
                -- Changing the 1st line from the source code template
                IF @vDeleteByTableTplCurrentRow = 1 
                BEGIN 
                    SET @vDeleteByTableTplData = 'IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = '''+ 'CSI_PurgeUtil_DeleByTable_' + @pvSetupName + ''' AND type = ''P'')' + char(13) + char(10);
                    SET @vDeleteByTableTplData = @vDeleteByTableTplData + '    EXECUTE sp_executesql N' + '''CREATE PROCEDURE CSI_PurgeUtil_DeleByTable_' + @pvSetupName + ' AS' + ''';' + char(13) + char(10);
                    -- Run the CREATE PROCEDURE stub
                    EXECUTE sp_executesql @vDeleteByTableTplData;
                    --
                    SET @vDeleteByTableTplData = 'ALTER PROCEDURE CSI_PurgeUtil_DeleByTable_' + @pvSetupName + char(13) + char(10);
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData) SELECT @vDeleteByTableTplData
                END
                ELSE
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData) SELECT @vDeleteByTableTplData;
                --PRINT @vDeleteByTableTplData
                ---------------------------------------------------------------------------
                -- Prepare the sql statement to insert rows into the CSI_PurgeUtil_InstValue table on a batch basis.
                ---------------------------------------------------------------------------
                IF CHARINDEX('#PREPAREDATA#', @vDeleteByTableTplData, 1) > 0 
                BEGIN
                    SET @vProgID = @vObject_Name + '.PREPAREDATA';  
                    SET @vHierarchyCurrentRow = 0;
                    --PRINT 'vHierarchyRowsToProcess=' + STR(@vHierarchyRowsToProcess);
                    WHILE @vHierarchyCurrentRow < @vHierarchyRowsToProcess
                    BEGIN
                        SET @vHierarchyCurrentRow = @vHierarchyCurrentRow + 1;
                        SELECT @vTablename = tablename
                             , @vInstanceCol1A = InstanceCol1A 
                             , @vInstanceCol1B = InstanceCol1B 
                             , @vInstanceCol1C = InstanceCol1C 
                             , @vInstanceCol2 = InstanceCol2  
                             , @vInstanceParentCol = InstanceParentCol 
                             , @vParentTable = parenttable 
                             , @vParentTableLinkCol = ParentTableLinkCol
                             , @vLevel = Level
                             , @vCommitBatchSizePerTable = CommitBatchSize
                             , @vSQLQueryPredicate = SQLQueryPredicate
                        FROM @vHierarchyData_tab
                        WHERE RowID = @vHierarchyCurrentRow;
                        --
                        SET @vSQLStatement = '';
                        SET @vHierarchyData = '';
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vRecordsAffected = 0;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vStartDataPrepByTableTime = sysdatetime();' + char(13) + char(10);
                        --
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'INSERT INTO CSI_PURGEUTIL_INSTVALUE (setupname, tablename, instancecol1A, instancecol1B, instancecol1C, instancecol2, instancevalue1A, instancevalue1B, instancevalue1C, instancevalue2, maintableinstancevalue, level, instanceparentcol, parenttable)' + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'SELECT ' + @vSQLQueryPredicate + ' ''' + @pvSetupName + ''', ' + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vTablename + ''', ' + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vInstanceCol1A + ''', ' + char(13) + char(10);
                        IF @vInstanceCol1B IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vInstanceCol1B + ''', ' + char(13) + char(10);
                        IF @vInstanceCol1C IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vInstanceCol1C + ''', ' + char(13) + char(10);
                        IF @vInstanceCol2 IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vInstanceCol2 + ''', ' + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    t.' + @vInstanceCol1A + ', ' + char(13) + char(10);
                        IF @vInstanceCol1B IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    t.' + @vInstanceCol1B + ', ' + char(13) + char(10);
                        IF @vInstanceCol1C IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    t.' + @vInstanceCol1C + ', ' + char(13) + char(10);
                        IF @vInstanceCol2 IS NULL 
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    t.' + @vInstanceCol2 + ', ' + char(13) + char(10);
                        IF @vLevel = 1
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    t.' + @vInstanceCol1A + ' maintableinstancevalue, ' + char(13) + char(10);
                        ELSE
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    iv.maintableinstancevalue, ' + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ' + CAST(@vLevel AS NVARCHAR) + ', ' + char(13) + char(10);
						IF @vInstanceParentCol IS NULL
							SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null, ' + char(13) + char(10);
						ELSE
							SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vInstanceParentCol + ''', ' + char(13) + char(10);
						IF @vParentTable IS NULL
							SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    Null ' + char(13) + char(10);
						ELSE
							SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    ''' + @vParentTable + ''' '+ char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'FROM ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + ' t ' + char(13) + char(10);
                        IF @vLevel = 1
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'WHERE t.' + @vInstanceCol1A + ' IN (SELECT t.InstanceId FROM @pvCSI_PurgeUtil_Instance_Tab t) ' + char(13) + char(10);
                        ELSE BEGIN -- Level 2 onwards
	                        -- Validate Link Column value
                            IF UPPER(@vParentTableLinkCol) NOT IN ('INSTANCECOL', 'INSTANCECOL2')  BEGIN
		                        SET @ErrorMessage = 'For Setup Table ' + @vTablename + ', Parent Table ' + @vParentTable + ' : ';
			                    IF @vParentTableLinkCol IS NULL
			                        SET @ErrorMessage = @ErrorMessage + 'Link Column value cannot be blank.';
			                    ELSE 
			                        SET @ErrorMessage = @ErrorMessage + 'Link Column value can either be InstanceCol or InstanceCol2 only.';
		                        SET @vProgID = @vObject_Name + '.' + 'Validate Link Column value'; 
		                        RAISERROR (@ErrorMessage, 16, 1);
		                    END;                            
                            -- For Purging by Historymainline and other histories tables, purge only aged records and keep recent records that are within the retention period.
                            IF @pvSetupName = 'EQUIPMENT' AND UPPER(@vTablename) = 'HISTORYMAINLINE' BEGIN 
                                -- Tables that is linked to level 1 do not need to include instancecol into the subquery
                                --SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'WHERE t.' + @vInstanceParentCol + ' IN (SELECT iv.instancevalue1A FROM CSI_PURGEUTIL_INSTVALUE iv WHERE iv.setupname=''EQUIPMENT'' AND iv.tablename=''' + @vParentTable + ''')' + char(13) + char(10);
					            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'INNER JOIN CSI_PURGEUTIL_INSTVALUE iv ON iv.instancevalue1A=t.' + @vInstanceParentCol + ' AND iv.setupname=''' + @pvSetupName + ''' AND iv.tablename=''' + @vParentTable + ''' AND iv.level=''' + CAST((@vLevel - 1) AS NVARCHAR) + '''' + char(13) + char(10);
                                -- add in the getdate() filter to restrict the deletion to aged records.  Also note that all subsequent tables and level thereof that are linked to the historymainline will have only their aged records removed.
                                -- eg: A_EquipmentSetupHistoryDetails (Level 4) which is linked to A_EquipmentSetupHistory (Level 3) will have also the aged records removed 
                                --         since A_EquipmentSetupHistory (Level 3) is linked to Historymainline (Level 2).
                                --SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '    AND t.txndate < (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname=''EQUIPMENT'')) ' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'WHERE t.txndate < (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname=''EQUIPMENT'')) ' + char(13) + char(10);
                            END
                            ELSE BEGIN -- Level 2 : Other tables
						        IF UPPER(@vParentTableLinkCol) = 'INSTANCECOL' -- use Primary Key in subquery
					                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'INNER JOIN CSI_PURGEUTIL_INSTVALUE iv ON iv.instancevalue1A=t.' + @vInstanceParentCol + ' AND iv.setupname=''' + @pvSetupName + ''' AND iv.tablename=''' + @vParentTable + ''' AND iv.level=''' + CAST((@vLevel - 1) AS NVARCHAR) + '''' + char(13) + char(10);
					    	    ELSE IF UPPER(@vParentTableLinkCol) = 'INSTANCECOL2' -- use Alternate Key in subquery
					                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + 'INNER JOIN CSI_PURGEUTIL_INSTVALUE iv ON iv.instancevalue2=t.' + @vInstanceParentCol + ' AND iv.setupname=''' + @pvSetupName + ''' AND iv.tablename=''' + @vParentTable + ''' AND iv.level=''' + CAST((@vLevel - 1) AS NVARCHAR) + '''' + char(13) + char(10);
                                END;
                        END;
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + @vDMLOption + ';'; 
                        SET @vHierarchyData = @vHierarchyData + @vSQLStatement + char(13) + char(10); 
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '--' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vRecordsAffected = @@ROWCOUNT;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vEndDataPrepByTableTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vDataPrepByTableExecutionTime = DateDiff(millisecond, @vStartDataPrepByTableTime, @vEndDataPrepByTableTime);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '-- Capturing Run Statistics' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''PREPAREDATA'', ''    Insert records INTO CSI_PURGEUTIL_INSTVALUE FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vDataPrepByTableExecutionTime, @vEndDataPrepByTableTime,''' + REPLACE(LTRIM(@vSQLStatement), '''', '''''') + ''';' + char(13) + char(10);
						
                        --
                        INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'PREPAREDATA';

						SET @vHierarchyData = REPLICATE(' ', 8) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''PREPAREDATA'', ''    Insert records INTO CSI_PURGEUTIL_INSTVALUE FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vDataPrepByTableExecutionTime, @vEndDataPrepByTableTime,''' + REPLACE(LTRIM(@vSQLStatement), '''', '''''') + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @RunLogRowId = @RunLogRowId + 1' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '--' + char(13) + char(10);

						INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'PREPAREDATA';
                    END
                END
   			    ---------------------------------------------------------------------------
				-- Validate the execution rules individually
				---------------------------------------------------------------------------
				IF CHARINDEX('#VALIDATEDATA#', @vDeleteByTableTplData, 1) > 0 BEGIN
					SET @vProgID = @vObject_Name + '.VALIDATEDATA'; 
			        SET @vHierarchyData = '';
					SET @vProgID = @vObject_Name + '.ValidateExecutionRule1'; 
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule1))) > 0 BEGIN
                        SET @vValidateExecutionRule = 'SET @vRuleReturnValue = (' + @vValidateExecutionRule1 + ')';
					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + @vValidateExecutionRule + ';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'IF UPPER(@vRuleReturnValue) <> ''OK'' BEGIN ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vEndDataValidateTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vDataValidateExecutionTime = DateDiff(millisecond, @vStartDataValidateTime, @vEndDataValidateTime);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    -- Capturing Run Statistics' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule1), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule1), '''', '''''') + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    RAISERROR (''Purge/archive process cannot continue due to one or more validation execution rule failure.'', 16, 1);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'END' + char(13) + char(10);
                    END
                    ELSE 
 					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vRuleReturnValue = ''OK'';' + char(13) + char(10);
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData)
                    SELECT @vHierarchyData;
                    --
			        SET @vHierarchyData = '';
					SET @vProgID = @vObject_Name + '.ValidateExecutionRule2'; 
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule2))) > 0 BEGIN
                        SET @vValidateExecutionRule = 'SET @vRuleReturnValue = (' + @vValidateExecutionRule2 + ')';
					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + @vValidateExecutionRule + ';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'IF UPPER(@vRuleReturnValue) <> ''OK'' BEGIN ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vEndDataValidateTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vDataValidateExecutionTime = DateDiff(millisecond, @vStartDataValidateTime, @vEndDataValidateTime);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    -- Capturing Run Statistics' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule2), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule2), '''', '''''') + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    RAISERROR (''Purge/archive process cannot continue due to one or more validation execution rule failure.'', 16, 1);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'END' + char(13) + char(10);
                    END
                    ELSE 
 					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vRuleReturnValue = ''OK'';' + char(13) + char(10);
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData)
                    SELECT @vHierarchyData;
                    --
			        SET @vHierarchyData = '';
					SET @vProgID = @vObject_Name + '.ValidateExecutionRule3'; 
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule3))) > 0 BEGIN
                        SET @vValidateExecutionRule = 'SET @vRuleReturnValue = (' + @vValidateExecutionRule3 + ')';
					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + @vValidateExecutionRule + ';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'IF UPPER(@vRuleReturnValue) <> ''OK'' BEGIN ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vEndDataValidateTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vDataValidateExecutionTime = DateDiff(millisecond, @vStartDataValidateTime, @vEndDataValidateTime);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    -- Capturing Run Statistics' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupNamem RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule3), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupNamem RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule3), '''', '''''') + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    RAISERROR (''Purge/archive process cannot continue due to one or more validation execution rule failure.'', 16, 1);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'END' + char(13) + char(10);
                    END
                    ELSE 
 					    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vRuleReturnValue = ''OK'';' + char(13) + char(10);
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData)
                    SELECT @vHierarchyData;
                    --
			        SET @vHierarchyData = '';
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '--' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vEndDataValidateTime = sysdatetime();' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vDataValidateExecutionTime = DateDiff(millisecond, @vStartDataValidateTime, @vEndDataValidateTime);' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '-- Capturing Run Statistics' + char(13) + char(10);
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule1))) > 0 BEGIN
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule1), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule1), '''', '''''') + ''';' + char(13) + char(10);
                    END
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule2))) > 0 BEGIN 
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule2), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule2), '''', '''''') + ''';' + char(13) + char(10);
                    END
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule3))) > 0 BEGIN
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule3), '''', '''''') + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process.  The validation return value is '' + @vRuleReturnValue, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + REPLACE(LTRIM(@vValidateExecutionRule3), '''', '''''') + ''';' + char(13) + char(10);
                    END
                    IF LEN(LTRIM(RTRIM(@vValidateExecutionRule1+@vValidateExecutionRule2+@vValidateExecutionRule3))) = 0 BEGIN 
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process'', Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + 'There is no validation execution rule defined.' + ''';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''VALIDATEDATA'', ''Execute data validation process'', Null, Null, Null, @vDataValidateExecutionTime, @vEndDataValidateTime,''' + 'There is no validation execution rule defined.' + ''';' + char(13) + char(10);
                    END
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @RunLogRowId = @RunLogRowId + 1;' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '--' + char(13) + char(10);
                    INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData)
                    SELECT @vHierarchyData;
                END
                --
                IF @vArchiveRequired = 1
                BEGIN 
                    ---------------------------------------------------------------------------
                    -- Archive the data into the archive schema
                    ---------------------------------------------------------------------------
                    IF CHARINDEX('#ARCHIVEDATA#', @vDeleteByTableTplData, 1) > 0 BEGIN
                        SET @vProgID = @vObject_Name + '.ARCHIVEDATA'; 
                        SET @vHierarchyCurrentRow = 0;
                        --PRINT @vHierarchyRowsToProcess;
                        WHILE @vHierarchyCurrentRow < @vHierarchyRowsToProcess
                        BEGIN
                            SET @vHierarchyCurrentRow = @vHierarchyCurrentRow + 1;
                            SELECT @vTablename = tablename
                                 , @vInstanceCol1A = InstanceCol1A 
                                 , @vInstanceCol1B = InstanceCol1B 
                                 , @vInstanceCol1C = InstanceCol1C 
                                 , @vInstanceCol2 = InstanceCol2
                                 , @vInstanceParentCol = InstanceParentCol 
                                 , @vParentTable = parenttable 
                                 , @vParentTableLinkCol = ParentTableLinkCol
                                 , @vLevel = Level
                                 , @vCommitBatchSizePerTable = CommitBatchSize
                                 , @vSQLQueryPredicate = SQLQueryPredicate
                            FROM @vHierarchyData_tab
                            WHERE RowID = @vHierarchyCurrentRow;
                            --
                            SET @vColNames = '';
                            EXECUTE CSI_Purgeutil_Global_GetTableColumnNames 
                                  @pvColNames                  = @vColNames OUTPUT
                                , @pvTableName   		       = @vTablename
								,@pvSourceDBName        	   = @pvSourceDBName
                                ;
                            SET @vColNames = ISNULL(@vColNames, '');
                            SET @vSQLStatement = '';
                            SET @vLogSQLStatement1 = '';
                            SET @vHierarchyData = '';
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vRecordsAffected = 0;' + char(13) + char(10);
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vStartArchiveByTableTime = sysdatetime();' + char(13) + char(10);
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vTablename = ''' + @vTablename + ''';' + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + 'INSERT INTO ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTablename + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '(BatchExecutionId,PurgeID,maintableinstancevalue,' + @vColNames + ') ' + char(13) + char(10);
                            SET @vLogSQLStatement1 = @vSQLStatement;
                            SET @vHierarchyData = @vHierarchyData + @vSQLStatement; 
                            --
                            INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                            SELECT @vHierarchyData, 'ARCHIVEDATA';
                            --
                            SET @vSQLStatement = '';
                            SET @vLogSQLStatement2 = '';
                            SET @vHierarchyData = '';
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + 'SELECT ' + @vSQLQueryPredicate + ' @vBatchExecutionId,' + @vInstanceCol1A + ', iv.maintableinstancevalue,' + @vColNames + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + 'FROM ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + ' t ' + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + 'INNER JOIN CSI_PURGEUTIL_INSTVALUE iv ON iv.instancevalue1A=t.' + @vInstanceCol1A + ' ' + char(13) + char(10);
                            IF @vInstanceCol1B IS NOT NULL 
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.instancevalue1B=t.' + @vInstanceCol1B + ' ' + char(13) + char(10);
                            IF @vInstanceCol1C IS NOT NULL 
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.instancevalue1C=t.' + @vInstanceCol1C + ' ' + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.SetupName=''' + @pvSetupName + ''' ' + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.tablename=''' + @vTablename + ''' ' + char(13) + char(10);
							SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.level=''' + CAST(@vLevel AS NVARCHAR) + ''' ' + char(13) + char(10);
							IF @vInstanceParentCol IS NOT NULL 
								SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.instanceparentcol=''' + @vInstanceParentCol + ''' ' + char(13) + char(10);
							IF @vParentTable IS NOT NULL 
								SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '    AND iv.parenttable=''' + @vParentTable + ''' ' + char(13) + char(10);
                            SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + @vDMLOption + ';' + char(13) + char(10);
                            SET @vLogSQLStatement2 = @vSQLStatement;
                            SET @vHierarchyData = @vHierarchyData + @vSQLStatement; 
                            --
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '--' + char(13) + char(10);
                            INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                            SELECT @vHierarchyData, 'ARCHIVEDATA';
                            --
                            SET @vHierarchyData = '';
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vRecordsAffected = @@ROWCOUNT;' + char(13) + char(10);
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vEndArchiveByTableTime = sysdatetime();' + char(13) + char(10);
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @vArchiveByTableExecutionTime = DateDiff(millisecond, @vStartArchiveByTableTime, @vEndArchiveByTableTime);' + char(13) + char(10);
                            SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '--' + char(13) + char(10);
                            --
                            INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                            SELECT @vHierarchyData, 'ARCHIVEDATA';
                            ---------------------------------------------------------------------------
                            -- Capture run statistics on each table using @vSQLStatement 
                            ---------------------------------------------------------------------------
                            SET @vHierarchyData = '';
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '-- Capturing Run Statistics' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''ARCHIVEDATA'', ''    Insert records INTO Archive Schema FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vArchiveByTableExecutionTime, @vEndArchiveByTableTime,' + char(13) + char(10);
							INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
							SELECT @vHierarchyData, 'ARCHIVEDATA';
							--
							SET @vLogSQLStatement1 = @vLogSQLStatement1 + @vLogSQLStatement2;
							SET @vHierarchyData = REPLICATE(' ', 12) + '''' + REPLACE(LTRIM(@vLogSQLStatement1), '''', '''''');
							INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
							SELECT @vHierarchyData, 'ARCHIVEDATA';

							SET @vHierarchyData = REPLICATE(' ', 12) + '''' + ';' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''ARCHIVEDATA'', ''    Insert records INTO Archive Schema FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vArchiveByTableExecutionTime, @vEndArchiveByTableTime,' + char(13) + char(10);
							INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
							SELECT @vHierarchyData, 'ARCHIVEDATA';

							SET @vHierarchyData = REPLICATE(' ', 12) + '''' + REPLACE(LTRIM(@vLogSQLStatement1), '''', '''''');
							INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
							SELECT @vHierarchyData, 'ARCHIVEDATA';

							SET @vHierarchyData = REPLICATE(' ', 12) + '''' + ';' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'SET @RunLogRowId = @RunLogRowId + 1;' + char(13) + char(10);
							SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '--' + char(13) + char(10);
							INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
							SELECT @vHierarchyData, 'ARCHIVEDATA';
							
                        END
                    END
                END
                IF @vPurgeRequired = 1
                BEGIN 
                    ---------------------------------------------------------------------------
                    -- Deleting data in source table based on CSI_PURGEUTIL_INSTVALUE that is populated by the #PREPAREDATA# stage.
                    ---------------------------------------------------------------------------
                    IF CHARINDEX('#DELETEBYTABLEDATA#', @vDeleteByTableTplData, 1) > 0 BEGIN
                        SET @vProgID = @vObject_Name + '.DELETEBYTABLEDATA';  
                        -- For deletion of records by table, it must adopt the bottom up approach.
                        -- Ie: Delete records starting with table with the highest level to the lowest level.  Lowest Level table is in fact Level 1 table.
                        SET @vHierarchyData = REPLICATE(' ', 12) + REPLICATE('-', 80) + char(13) + char(10);
                        SET @vHierarchyCurrentRow = @vHierarchyRowsToProcess;
                        WHILE @vHierarchyCurrentRow <> 0
                        BEGIN
                            SELECT @vTablename = tablename
                                 , @vInstanceCol1A = InstanceCol1A 
                                 , @vInstanceCol1B = InstanceCol1B 
                                 , @vInstanceCol1C = InstanceCol1C 
                                 , @vInstanceCol2 = InstanceCol2
                                 , @vInstanceParentCol = InstanceParentCol 
                                 , @vParentTable = parenttable 
                                 , @vParentTableLinkCol = ParentTableLinkCol
                                 , @vLevel = Level
                                 , @vCommitBatchSizePerTable = CommitBatchSize
                                 , @vSQLQueryPredicate = SQLQueryPredicate
                            FROM @vHierarchyData_tab
                            WHERE RowID = @vHierarchyCurrentRow;
                            --  
                            SET @vSQLStatement = '';
                            SET @vHierarchyData = '';
                            IF NOT UPPER(@vTablename) IN ('RESOURCEDEF', 'PRODUCTIONSTATUS') 
                            BEGIN
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'BEGIN TRY ' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vRecordsAffected = 0;' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vStartDeleByTableTime = sysdatetime();' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    WHILE (1=1)' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    BEGIN' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        BEGIN TRANSACTION;' + char(13) + char(10);
                                -- Add LockType locking mechanism
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        WITH RowsToDelete AS (' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            SELECT DISTINCT TOP (' + CAST(@vCommitBatchSizePerTable AS NVARCHAR) + ') t.' + @vInstanceCol1A + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            FROM ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + ' t ' + @vLockType  + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            INNER JOIN CSI_PURGEUTIL_INSTVALUE iv ON iv.instancevalue1A = t.' + @vInstanceCol1A + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            AND iv.setupname=''' + @pvSetupName + '''' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            AND iv.tablename=''' + @vTablename + ''' ' + char(13) + char(10);
								SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            AND iv.level=''' + CAST(@vLevel AS NVARCHAR) + ''' ' + char(13) + char(10);
								IF @vInstanceParentCol IS NOT NULL 
									SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            AND iv.instanceparentcol=''' + @vInstanceParentCol + ''' ' + char(13) + char(10);
								IF @vParentTable IS NOT NULL 
									SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '            AND iv.parenttable=''' + @vParentTable + ''' ' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        )' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        DELETE dtl' + char(13) + char(10);
                                IF @vTablename = 'Container'
                                    SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        OUTPUT Deleted.ContainerId, Deleted.ContainerName, CURRENT_TIMESTAMP INTO CSI_PURGEUTIL_PURGEDLOTS ' + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        FROM ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + ' dtl ' + @vLockType  + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        INNER JOIN RowsToDelete r ON r.' + @vInstanceCol1A + ' = dtl.' + @vInstanceCol1A + char(13) + char(10);
                                SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 12) + '        ' + @vDMLOption + ';' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + @vSQLStatement + char(13) + char(10);
                                --
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        --' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        SET @vRowCount = @@ROWCOUNT' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        SET @vRecordsAffected = @vRecordsAffected + @vRowCount;' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        IF @@TRANCOUNT > 0' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '            COMMIT TRANSACTION;' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '        IF @vRowCount < ' + CAST(@vCommitBatchSizePerTable AS NVARCHAR) + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '            BREAK' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    END' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    --' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vEndDeleByTableTime = sysdatetime();' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vDeleByTableExecutionTime = DateDiff(millisecond, @vStartDeleByTableTime, @vEndDeleByTableTime);' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    -- Capturing Run Statistics' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''DELETEDATA'', ''    Delete records FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vDeleByTableExecutionTime, @vEndDeleByTableTime,' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    ''' + REPLACE(LTRIM(@vSQLStatement), '''', '''''') + REPLICATE(' ', 12) + '''' + ';' + char(13) + char(10);
								INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
								SELECT @vHierarchyData, 'DELETEDATA';

								SET @vHierarchyData = REPLICATE(' ', 12) + '    INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SELECT @pvSetupName, @RunLogRowId, @vBatchExecutionId, Null, ''DELETEDATA'', ''    Delete records FROM source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vRecordsAffected, @vDeleByTableExecutionTime, @vEndDeleByTableTime,' + char(13) + char(10);
								SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    ''' + REPLACE(LTRIM(@vSQLStatement), '''', '''''') + REPLICATE(' ', 12) + '''' + ';' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @RunLogRowId = @RunLogRowId + 1;' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    --' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'END TRY ' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'BEGIN CATCH ' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @vProgID = @vObject_Name + ''.Delete table ' + @vTablename + ''';' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    SET @ErrorMessage = ERROR_MESSAGE(); ' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + '    RAISERROR (@ErrorMessage, 16, 1); ' + char(13) + char(10);
                                SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 12) + 'END CATCH ' + char(13) + char(10);
                            END
                            SET @vHierarchyCurrentRow = @vHierarchyCurrentRow - 1;
                            --
                            INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData, RowType)
                            SELECT @vHierarchyData, 'DELETEDATA';
                        END
                    END
                END
				---------------------------------------------------------------------------
                IF @vArchiveRequired = 1
				BEGIN
					IF CHARINDEX('#ARCHIVELOG#', @vDeleteByTableTplData, 1) > 0 BEGIN
						SET @vHierarchyData = '';
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'INSERT INTO @vCSI_PurgeUtil_ArchiveLog_Tab (SetupName, BatchExecutionId, PurgeType, TxnDate, Version, ArchiveDBName, ArchiveSchemaName)' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SELECT @pvSetupName, @vBatchExecutionId, @pvPurgeType, @vEndArchiveByTableTime, @vSetupVersion, ''' + @pvArchiveDBName + ''', ''' + @pvArchiveSchemaName + '''' + char(13) + char(10);
						INSERT INTO @vScriptsDELEBYTABLEData_tab (RowData) SELECT @vHierarchyData;
					END;
				END
            END
	        ---------------------------------------------------------------------------
            -- Creating CSI_PurgeUtil_DeleByTable_' + @pvSetupName stored procedure on the fly now.
	        ---------------------------------------------------------------------------
            --SELECT * FROM @vScriptsDELEBYTABLEData_tab; -- unmask this statement to view the source codes when debugging in sql server management studio
            EXECUTE CSI_PurgeUtil_CreateSP @pvRowData_Tab = @vScriptsDELEBYTABLEData_tab;
        END TRY
	    BEGIN CATCH
		    BEGIN   
			    IF @ErrorMessage IS NULL 
			        SET @ErrorMessage = ERROR_MESSAGE(); 
			    IF @vProgID IS NULL 
			        SET @vProgID = @vObject_Name + '.' + 'Creation of Stored Procedure CSI_PurgeUtil_DeleByTable_' + @pvSetupName; 
    		    RAISERROR (@ErrorMessage, 16, 1);
		    END;
	    END CATCH
		--
	    ---------------------------------------------------------------------------
        -- CSI_PURGEUTIL_REST_TPL template stored procedure
	    ---------------------------------------------------------------------------
        BEGIN TRY 
            INSERT INTO @vRestoreTplData_tab(RowData) 
            EXECUTE sp_helptext @objname = 'CSI_PURGEUTIL_REST_TPL';  -- getting the compiled source code
            SET @vRestoreTplRowsToProcess = @@ROWCOUNT;  -- the number of rows that is inserted into @vRestoreTplData_tab
        END TRY
	    BEGIN CATCH
		    BEGIN
		        SET @ErrorMessage = ERROR_MESSAGE(); 
			    SET @vProgID = @vObject_Name + '.' + 'RESTORE:INSERT INTO @vDeleteTplData_tab(RowData) ';
    		    RAISERROR (@ErrorMessage, 16, 1);
		    END;
	    END CATCH
        --SELECT * FROM @vRestoreTplData_tab;
        --
	    ---------------------------------------------------------------------------
        -- Inserting rows into the table @vScriptsRESTData_tab based on the template stored procedure
        -- and table @vHierarchyData_tab.
	    ---------------------------------------------------------------------------
        BEGIN TRY 
            SET @vRestoreTplCurrentRow = 0;
            --PRINT @vRestoreTplRowsToProcess;
            WHILE @vRestoreTplCurrentRow < @vRestoreTplRowsToProcess
            BEGIN
                SET @vRestoreTplCurrentRow = @vRestoreTplCurrentRow + 1;
                SELECT DISTINCT
                    @vRestoreTplData = RowData 
                FROM @vRestoreTplData_tab
                WHERE RowID = @vRestoreTplCurrentRow;
                --PRINT @vRestoreTplData;
                --
                ---------------------------------------------------------------------------
                -- Inserting lines from the source code template to create SP CSI_PurgeUtil_Rest_' + @pvSetupName;
                ---------------------------------------------------------------------------
                -- Changing the 1st line from the source code template
                IF @vRestoreTplCurrentRow = 1 BEGIN 
                    SET @vRestoreTplData = 'IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = '''+ 'CSI_PurgeUtil_Rest_' + @pvSetupName + ''' AND type = ''P'')' + char(13) + char(10);
                    SET @vRestoreTplData = @vRestoreTplData + '    EXECUTE sp_executesql N' + '''CREATE PROCEDURE CSI_PurgeUtil_Rest_' + @pvSetupName + ' AS' + ''';' + char(13) + char(10);
                    -- Run the CREATE PROCEDURE stub
                    EXECUTE sp_executesql @vRestoreTplData;
                    --
                    SET @vRestoreTplData = 'ALTER PROCEDURE CSI_PurgeUtil_Rest_' + @pvSetupName + char(13) + char(10);
                    INSERT INTO @vScriptsRESTData_tab (RowData) SELECT @vRestoreTplData
                END
                ELSE
                    INSERT INTO @vScriptsRESTData_tab (RowData) SELECT @vRestoreTplData;
                --PRINT @vRestoreTplData
                ---------------------------------------------------------------------------
                -- Deleting data in source table based on CSI_PURGEUTIL_INSTVALUE that is populated by the #PREPAREDATA# stage.
                ---------------------------------------------------------------------------
                IF CHARINDEX('#DELETERUNLOG#', @vRestoreTplData, 1) > 0 BEGIN
                    SET @vProgID = @vObject_Name + '.DELETERUNLOG';  
                    --
                    SET @vHierarchyData = '';
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'BEGIN TRANSACTION; ' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'DELETE FROM CSI_PURGEUTIL_RUNLOG ' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'WHERE SETUPNAME=''' + @pvSetupName + '''; ' + char(13) + char(10);
                    SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'COMMIT TRANSACTION; ' + char(13) + char(10);
                    INSERT INTO @vScriptsRESTData_tab (RowData)
                    SELECT @vHierarchyData;
                END
                ---------------------------------------------------------------------------
                -- Restoring data from Archive
                ---------------------------------------------------------------------------
                IF CHARINDEX('#RESTOREDATA#', @vRestoreTplData, 1) > 0 BEGIN
                    SET @vProgID = @vObject_Name + '.RESTOREDATA'; 
                    -- For restoration of records, it must adopt the bottom up approach.
                    -- Ie: Restore records starting with table with the highest level to the lowest level.  Lowest Level table is in fact Level 1 table.
                    SET @vHierarchyCurrentRow = @vHierarchyRowsToProcess;
                    WHILE @vHierarchyCurrentRow <> 0
                    BEGIN
                        SELECT @vTablename = tablename
                             , @vInstanceCol1A = InstanceCol1A 
                             , @vInstanceCol1B = InstanceCol1B 
                             , @vInstanceCol1C = InstanceCol1C 
                             , @vInstanceCol2 = InstanceCol2 
                             , @vInstanceParentCol = InstanceParentCol 
                             , @vParentTable = parenttable 
                             , @vParentTableLinkCol = ParentTableLinkCol
                             , @vLevel = Level
                             , @vCommitBatchSizePerTable = CommitBatchSize
                             , @vSQLQueryPredicate = SQLQueryPredicate
                        FROM @vHierarchyData_tab
                        WHERE RowID = @vHierarchyCurrentRow;
                        --
                        SET @vColNames = '';
                        EXECUTE CSI_Purgeutil_Global_GetTableColumnNames 
                              @pvColNames                  = @vColNames OUTPUT
                            , @pvTableName   		       = @vTablename
							,@pvSourceDBName        	   = @pvSourceDBName
                            ;
                        SET @vColNames = ISNULL(@vColNames, '');
                        SET @vSQLStatement = '';
                        SET @vLogSQLStatement1 = '';
                        SET @vHierarchyData = '';
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vStartDataRestByTableTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vLoopNumber = 0;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vRowCount = 0;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vTableRowCount = 0;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'WHILE (1=1)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'BEGIN' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '    BEGIN TRY ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        BEGIN TRANSACTION' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        SET @vLoopNumber = @vLoopNumber + 1;' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '        INSERT INTO ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '        (' + @vColNames + ') ' + char(13) + char(10);
                        SET @vLogSQLStatement1 = @vSQLStatement;
                        SET @vHierarchyData = @vHierarchyData + @vSQLStatement;
                        -- Since @vHierarchyData stores only up to a max of 4000, I split the statemeent into 2 inserts
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        --
                        SET @vSQLStatement = '';
                        SET @vLogSQLStatement2 = '';
                        SET @vHierarchyData = '';
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '        SELECT ' + @vSQLQueryPredicate + ' ' + @vColNames  + char(13) + char(10);
                        SET @vSQLStatement = @vSQLStatement + REPLICATE(' ', 8) + '        FROM (' + char(13) + char(10);
                        SET @vLogSQLStatement2 = @vSQLStatement;
                        SET @vHierarchyData = @vHierarchyData + @vSQLStatement;
                        -- Since @vHierarchyData stores only up to a max of 4000, I split the statemeent into 2 inserts
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        --
                        SET @vSQLStatement = '';
                        SET @vLogSQLStatement3 = '';
                        SET @vHierarchyData = '';
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            SELECT ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            CAST(ROW_NUMBER() OVER (Order By (' + @vInstanceCol1A + ')) AS BIGINT) No, '+ @vColNames + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            FROM ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTablename + ' archTab ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            WHERE archTab.BatchExecutionId = @pvBatchExecutionId ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '                AND (CASE WHEN @pvInstanceId IS Null THEN ''0000000000000000'' ELSE archTab.MainTableInstanceValue END) = ISNULL(@pvInstanceId, ''0000000000000000'') ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            ) t ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '        WHERE t.No BETWEEN ((@vLoopNumber-1) * ' + CAST(@vCommitBatchSizePerTable AS NVARCHAR) + ') + 1 AND (@vLoopNumber * ' + CAST(@vCommitBatchSizePerTable AS NVARCHAR) +  ') ' + char(13) + char(10);
                        SET @vSQLStatement  = @vSQLStatement  + REPLICATE(' ', 8) + '            AND NOT EXISTS (SELECT * FROM ' + @pvSourceDBName + '.' + @pvSourceSchemaName + '.' + @vTablename + ' srcTab WHERE srcTab.' + @vInstanceCol1A + '=t.' + @vInstanceCol1A + '); ' + char(13) + char(10);
                        SET @vLogSQLStatement3 = @vSQLStatement;
                        SET @vHierarchyData = @vHierarchyData + @vSQLStatement;
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        --' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        SET @vRowCount = @@ROWCOUNT;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        SET @vTableRowCount = @vTableRowCount + @vRowCount;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        COMMIT TRANSACTION;' + char(13) + char(10);
                        --
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        --
                        SET @vHierarchyData = '';
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '        IF @vRowCount = 0 ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '            BREAK' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '       --' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '    END TRY ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '    BEGIN CATCH ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         ROLLBACK TRANSACTION;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         IF ERROR_NUMBER() = 2627 -- Only raise error if this is a violation of PRIMARY KEY constraint.' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         BEGIN ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             -- Records already existed in the source schema, so ignore unique constraint error. ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             SET @vProgID = @vObject_Name + ''.Violation of PRIMARY KEY constraint. Restore table ' + @vTablename + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             SET @ErrorMessage = ''Info:''+ERROR_MESSAGE(); ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '    RAISERROR (@ErrorMessage, 16, 1); ' + char(13) + char(10);
                        --
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         END ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         ELSE ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         BEGIN ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             SET @vProgID = @vObject_Name + ''.OTHERS. Restore table ' + @vTablename + ''';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             SET @ErrorMessage = ERROR_MESSAGE(); ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '             RAISERROR (@ErrorMessage, 16, 1); ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '         END ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '     END CATCH ' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '     --' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'END' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vEndDataRestByTableTime = sysdatetime();' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @vDataRestByTableExecutionTime = DateDiff(millisecond, @vStartDataRestByTableTime, @vEndDataRestByTableTime);' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '-- Capturing Run Statistics' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'INSERT INTO CSI_PURGEUTIL_RUNLOG (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SELECT ''' + @pvSetupName + ''', @RunLogRowId, @pvBatchExecutionId, @vRestoreId, ''RESTOREDATA'', ''    Insert records FROM archive table to source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vTableRowCount, @vDataRestByTableExecutionTime, @vEndDataRestByTableTime,' + char(13) + char(10);
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
						--
						SET @vLogSQLStatement1 = @vLogSQLStatement1 + @vLogSQLStatement2 + @vLogSQLStatement3;
                        SET @vHierarchyData = REPLICATE(' ', 8) + '''' + REPLACE(LTRIM(@vLogSQLStatement1), '''', '''''');
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        
						SET @vHierarchyData = REPLICATE(' ', 8) + '''' + ';' + char(13) + char(10);
						SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'INSERT INTO CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, RowId, BatchExecutionId, RestoreId, RunStage, Action, TableName, gLevel, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement)' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SELECT ''' + @pvSetupName + ''', @RunLogRowId, @pvBatchExecutionId, @vRestoreId, ''RESTOREDATA'', ''    Insert records FROM archive table to source table '',''' + @vTablename + ''', ' + CAST(@vLevel AS NVARCHAR) + ', @vTableRowCount, @vDataRestByTableExecutionTime, @vEndDataRestByTableTime,' + char(13) + char(10);
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';

						SET @vHierarchyData = REPLICATE(' ', 8) + '''' + REPLACE(LTRIM(@vLogSQLStatement1), '''', '''''');
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        
                        SET @vHierarchyData = REPLICATE(' ', 8) + '''' + ';' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + 'SET @RunLogRowId = @RunLogRowId + 1;' + char(13) + char(10);
                        SET @vHierarchyData = @vHierarchyData + REPLICATE(' ', 8) + '--' + char(13) + char(10);
                        --PRINT @vHierarchyData;
                        INSERT INTO @vScriptsRESTData_tab (RowData, RowType)
                        SELECT @vHierarchyData, 'RESTOREDATA';
                        --
                        SET @vHierarchyCurrentRow = @vHierarchyCurrentRow - 1;
                    END
                END
            END
	        ---------------------------------------------------------------------------
            -- Creating CSI_PurgeUtil_Rest_' + @pvSetupName stored procedure on the fly now.
	        ---------------------------------------------------------------------------
            --SELECT * FROM @vScriptsRESTData_tab; -- unmask this statement to view the source codes when debugging in sql server management studio
            EXECUTE CSI_PurgeUtil_CreateSP @pvRowData_Tab = @vScriptsRESTData_tab;
        END TRY
	    BEGIN CATCH
		    BEGIN
			    IF @ErrorMessage IS NULL 
			        SET @ErrorMessage = ERROR_MESSAGE(); 
			    IF @vProgID IS NULL 
			        SET @vProgID = @vObject_Name + '.' + 'Creation of Stored Procedure CSI_PurgeUtil_Rest_' + @pvSetupName; 
    		    RAISERROR (@ErrorMessage, 16, 1);
		    END;
	    END CATCH
		--
        --------------------------------------------------------------------------
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL';  
		RETURN 0;
	END TRY
	BEGIN CATCH
		IF @ErrorMessage IS NULL
		    SET @ErrorMessage  = ERROR_MESSAGE();
		IF @ErrorSeverity IS NULL
		    SET @ErrorSeverity = ERROR_SEVERITY(); 
		IF @ErrorState IS NULL
		    SET @ErrorState = ERROR_STATE();
		IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
		EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO
/* Sample syntax : 
EXECUTE CSI_PurgeUtil_TableSetup_GenerateObject 
      @pvSetupName                 = 'EQUIPMENT'
	, @pvRootTableName   		   = 'ResourceDef'
    , @pvSourceDBName              = 'InSiteSEMI_DB'
    , @pvArchiveDBName             = 'InSiteSEMIArchive_DB'
	, @pvSourceSchemaName		   = 'InSiteSEMI_SCHEMA'
	, @pvArchiveSchemaName		   = 'InSiteSEMI_SCHEMA'
	;
GO
*/
