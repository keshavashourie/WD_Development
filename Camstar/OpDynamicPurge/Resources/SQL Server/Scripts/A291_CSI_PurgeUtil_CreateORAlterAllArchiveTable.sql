ALTER PROCEDURE CSI_PurgeUtil_CreateORAlterAllArchiveTable 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_CreateORAlterAllArchiveTable
                     Syncronize all database table structure in the archive schema
                     with that of the source schema.
  Author           : Benny.Chia 
  Date             : 14 Jan 2016
  Compile in       : Source schema
  Called By        : a database job that runs before purging and/or archiving commence.
  Call             : CSI_PurgeUtil_CreateORAlterArchiveTable
--------------------------------------------------------------------------- */
    ( @pvSourceDBName               NVARCHAR(40)
	, @pvArchiveDBName              NVARCHAR(40)
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
DECLARE @vArchiveTable_tab          TABLE 
    (RowID int not null primary key identity(1,1)
    , tablename     NVARCHAR(30)
    , parenttable   NVARCHAR(30)
    );
DECLARE @vArchiveTableRowsToProcess INTEGER=0;
DECLARE @vArchiveTableCurrentRow    INTEGER=0;
DECLARE @vTablename                 NVARCHAR(30);
DECLARE @vParentTable               NVARCHAR(30);
--
DECLARE @vSetup_tab                TABLE 
    (RowID int not null primary key identity(1,1)
    , setupname         NVARCHAR(40)
    , maintablename     NVARCHAR(30)
    );
DECLARE @vSetupRowsToProcess INTEGER=0;
DECLARE @vSetupCurrentRow    INTEGER=0;
DECLARE @vSetupName          NVARCHAR(40);
DECLARE @vMainTableName      NVARCHAR(30);
DECLARE @vTableAlteredFlag   INTEGER=0;
--
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		--
		---------------------------------------------------------------------------
        -- During this stored procedure execution, the setup status is set to 'SETUP'. 
        -- This will prevent the purging and/or archiving process to run.
		---------------------------------------------------------------------------
        BEGIN TRY
            UPDATE CSI_PURGEUTIL_SETUP
            SET SetupStatus = 'SETUP'
            WHERE ArchiveRequired = 1 
            ;   
        END TRY
        BEGIN CATCH
            -- If no record found, then return since there is setup created yet.
            PRINT 'No setup records found';
            RETURN 0;
        END CATCH
		---------------------------------------------------------------------------
        BEGIN TRY
            INSERT INTO @vArchiveTable_tab
            SELECT DISTINCT sut.TableName, sut.ParentTable 
            FROM CSI_PURGEUTIL_SETUPTABLES sut		
            INNER JOIN CSI_PURGEUTIL_SETUP su ON su.SetupId = sut.SetupId
            WHERE su.ArchiveRequired = 1
            ORDER BY sut.ParentTable desc
            ;   -- Root table will be the last table in the Table sync process.
        END TRY
        BEGIN CATCH
            -- If no record found, then return since there is no archival process needed.
            PRINT 'No setuptable records found';
            RETURN 0;
        END CATCH
        --
        SELECT @vArchiveTableRowsToProcess = COUNT(1) FROM @vArchiveTable_tab;
        --PRINT '@vArchiveTableRowsToProcess=' + STR(@vArchiveTableRowsToProcess);
		---------------------------------------------------------------------------
        -- For each setuptables, run CSI_PurgeUtil_CreateORAlterArchiveTable
		---------------------------------------------------------------------------
        WHILE @vArchiveTableCurrentRow < @vArchiveTableRowsToProcess
        BEGIN
            SET @vArchiveTableCurrentRow = @vArchiveTableCurrentRow + 1;
            SELECT @vTablename = tablename
                 , @vParentTable = parenttable
            FROM @vArchiveTable_tab
            WHERE RowID = @vArchiveTableCurrentRow;
            --
            EXECUTE CSI_PurgeUtil_CreateORAlterArchiveTable 
                  @pvSourcedbname  		        = @pvSourceDBName
                , @pvArchivedbname         		= @pvArchiveDBName
                , @pvSourceSchemaName    		= @pvSourceSchemaName
	            , @pvArchiveSchemaName			= @pvArchiveSchemaName
	            , @pvTableName            		= @vTableName
	            , @pvParentTable         		= @vParentTable
	            , @pvTableAlteredFlag           = @vTableAlteredFlag OUTPUT    
	            ;
        END   
        --PRINT '@vTableAlteredFlag is ' + STR(@vTableAlteredFlag);
        -- If at least 1 table structure has changes, then regenerate all stored procedures starting with 'CSI_PurgeUtil_Dele%' and 'CSI_PurgeUtil_Rest%'.
        IF @vTableAlteredFlag = 1
        BEGIN
		    ---------------------------------------------------------------------------
            -- Regenerate all the stored procedures  starting with 'CSI_PurgeUtil_Dele%' and 'CSI_PurgeUtil_Rest%'
            --    that are needed for the purging and/or archival process.
            -- Note : the reason is to get a list of columns per table that are required in order for the
            --        SQL statements that are responsible @pvSourceSchemaName the insertion of records into the archive tables.
		    ---------------------------------------------------------------------------
            BEGIN TRY
                INSERT INTO @vSetup_tab
                SELECT su.SetupName, sut.TableName MainTablename
                FROM CSI_PURGEUTIL_SETUPTABLES sut		
                INNER JOIN CSI_PURGEUTIL_SETUP su ON su.SetupId = sut.SetupId
                WHERE su.ArchiveRequired = 1
                    AND sut.ParentTable IS NULL
                ;   -- Only pickup the setups that has the ArchiveRequired set as 1.
            END TRY
            BEGIN CATCH
                -- If no record found, then return since there is no archival process needed.
                PRINT 'No records found';
                RETURN 0;
            END CATCH
            --
            SELECT @vSetupRowsToProcess = COUNT(1) FROM @vSetup_tab;
            --PRINT '@vSetupRowsToProcess=' + STR(@vSetupRowsToProcess);
		    ---------------------------------------------------------------------------
            -- For each setup, run CSI_PurgeUtil_TableSetup_GenerateObject to regenerate
            -- the stored procedure on the fly.
		    ---------------------------------------------------------------------------
            WHILE @vSetupCurrentRow < @vSetupRowsToProcess
            BEGIN
                SET @vSetupCurrentRow = @vSetupCurrentRow + 1;
                SELECT @vSetupName = setupname
                     , @vMainTableName = maintablename
                FROM @vSetup_tab
                WHERE RowID = @vSetupCurrentRow;
                --
                --PRINT '@vSetupName=' + @vSetupName;
                --PRINT '@vMainTableName=' + @vMainTableName;
                EXECUTE CSI_PurgeUtil_TableSetup_GenerateObject 
                      @pvSetupName                 = @vSetupName
	                , @pvRootTableName   		   = @vMainTableName
                    , @pvSourceDBName              = @pvSourcedbname
                    , @pvArchiveDBName             = @pvArchivedbname
	                , @pvSourceSchemaName		   = @pvSourceSchemaName
	                , @pvArchiveSchemaName		   = @pvArchiveSchemaName
	                ;    
            END   
        END
		---------------------------------------------------------------------------
        -- During this stored procedure execution, the setup status is set to 'SETUP'. 
        -- This will prevent the purging and/or archiving process to run.
		---------------------------------------------------------------------------
        BEGIN TRY
            UPDATE CSI_PURGEUTIL_SETUP
            SET SetupStatus = 'ACTIVE'
            WHERE ArchiveRequired = 1 
            ;   
        END TRY
        BEGIN CATCH
            PRINT Null;
        END CATCH
		---------------------------------------------------------------------------
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
END;
GO
