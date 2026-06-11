ALTER PROCEDURE CSI_PurgeUtil_Execute_RestoreByBatchExecutionId 
    ( @pvSetupName                      NVARCHAR(40)
	, @pvArchiveDBName                  NVARCHAR(40)
	, @pvArchiveSchemaName			    NVARCHAR(40)
	, @pvBatchExecutionId               NVARCHAR(16)
	) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text.
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vRootTableName             NVARCHAR(30) = '';
DECLARE @vRootTableInstanceCol      NVARCHAR(30) = '';
DECLARE @vPurgeId                   NVARCHAR(16) = '';
DECLARE @vSQLStatement              NVARCHAR(4000) = '';
DECLARE @vParmDefinition            NVARCHAR(500) = '';
DECLARE @vRowProcessed              INT=0;
--
DECLARE @vStartProcessTime          DATETIME2;  -- Start time to measure the length of time taken to restore all lots/equipment txns for the entire process based on batchexecutionid.
DECLARE @vEndProcessTime            DATETIME2;  -- End time to measure the length of time taken to restore all lots/equipment txns for the entire process based on batchexecutionid.
DECLARE @vProcessExecutionTime      FLOAT;      -- Time taken to restore all lots/equipment txn for the entire process based on batchexecutionid.
BEGIN 
    SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
    -- Log a start message 
    SET @vStartProcessTime = sysdatetime();
	SET @vMessage = @pvSetupName + ' Setup : Restore process started at ' + convert(varchar(25), sysdatetime(), 121);
    EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvSetupName IS Null 
			RAISERROR ('Setup Name must not be blank', 16, 1);
		IF @pvArchiveDBName IS Null 
			RAISERROR ('Archive Database Name must not be blank', 16, 1);
		IF @pvArchiveSchemaName IS Null 
			RAISERROR ('ArchiveSchemaName must not be blank', 16, 1);
		IF @pvBatchExecutionId IS Null 
			RAISERROR ('BatchExecutionId must not be blank', 16, 1);
		--
		---------------------------------------------------------------------------
        -- Validate if the setup exist.
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate Setup record for existence';
        IF (SELECT COUNT(1) 
            FROM CSI_PURGEUTIL_SETUP ps
            WHERE UPPER(ps.setupname) = UPPER(@pvSetupName)) = 0  
           RAISERROR ('Setup record is not found in the CSI_PURGEUTIL_SETUP table', 16, 1);
		---------------------------------------------------------------------------
        -- Get the root table name based on Setup Name
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Get the root table name based on Setup Name';
        --PRINT '@pvSetupName:' + @pvSetupName;
        SELECT @vRootTableName = ps.maintablename
            , @vRootTableInstanceCol = ps.maintableinstancecol
        FROM CSI_PURGEUTIL_SETUP ps
        WHERE UPPER(ps.setupname) = UPPER(@pvSetupName);
        --PRINT '@vRootTableName:' + @vRootTableName;
        --PRINT '@vRootTableInstanceCol:' + @vRootTableInstanceCol;
		---------------------------------------------------------------------------
        -- Execute the restore instances stored procedure.
		---------------------------------------------------------------------------
        SET @vSQLStatement = N'CSI_PurgeUtil_Rest_' + @pvSetupName + ' @pvBatchExecutionId = ''' + @pvBatchExecutionId + ''';';
        --PRINT @vSQLStatement;
        SET @vParmDefinition = N'@pvBatchExecutionId NVARCHAR(16)';
    	EXECUTE sp_executesql @vSQLStatement
            , @vParmDefinition
            , @pvBatchExecutionId = @pvBatchExecutionId;
		---------------------------------------------------------------------------
        -- Get the number of instances that are restored.
		---------------------------------------------------------------------------
        SET @vSQLStatement = N'SELECT @vRowProcessedOUT = COUNT(1) FROM ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vRootTableName + ' t ';
        SET @vSQLStatement = @vSQLStatement + 'WHERE t.BatchExecutionId = ''' + @pvBatchExecutionId + ''''; 
        --PRINT @vSQLStatement;
        SET @vParmDefinition = N'@vRowProcessedOUT INT OUTPUT';
    	EXECUTE sp_executesql @vSQLStatement 
            , @vParmDefinition
            , @vRowProcessedOUT = @vRowProcessed OUTPUT;
        --PRINT 'Number of instances restored=' + STR(@vRowProcessed);
		SET @vEndProcessTime = sysdatetime();
		SET @vProcessExecutionTime = DateDiff(millisecond, @vStartProcessTime, @vEndProcessTime);
        -- Log a completion message 
 	    SET @vMessage = @pvSetupName + ' Setup : Restore from BatchExecutionId ''' + @pvBatchExecutionId + ''''
                                         + '  Duration of execution for the entire restore process in milliseconds : ' + LTRIM(STR(@vProcessExecutionTime))
	                                     + '  Restore process completed at ' + convert(varchar(25), sysdatetime(), 121);
        EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        --
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL'; 
		RETURN 0;
	END TRY
	BEGIN CATCH
        --DEALLOCATE cur_RestoreList;
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
