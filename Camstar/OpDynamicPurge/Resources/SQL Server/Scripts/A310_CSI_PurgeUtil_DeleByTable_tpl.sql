ALTER PROCEDURE CSI_PurgeUtil_DeleByTable_tpl 
    ( @pvSetupName                      NVARCHAR(40)
	, @pvPurgeType                      NVARCHAR(40)
	, @pvCSI_PurgeUtil_Instance_Tab     CSI_PurgeUtil_Instance_Tab READONLY
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE @vBatchExecutionId          NVARCHAR(16);
DECLARE @vMessageText               NVARCHAR(500)='';
DECLARE @vRowCount                  INTEGER=0;
DECLARE @vStartProcessTime			DATETIME2;  -- Start time to measure the length of time taken to purge all lots/equipment txns for the entire process.
DECLARE @vEndProcessTime			DATETIME2;  -- End time to measure the length of time taken to purge all lots/equipment txns for the entire process. 
DECLARE @vProcessExecutionTime      FLOAT;      -- Time taken to purge all lots/equipment txn for the entire process.
DECLARE @vStartDeleByTableTime		DATETIME2;  -- Start time to measure the length of time taken to purge all lots/equipment txns on each table.
DECLARE @vEndDeleByTableTime		DATETIME2;  -- End time to measure the length of time taken to purge all lots/equipment txns on each table
DECLARE @vDeleByTableExecutionTime  FLOAT;      -- Time taken to purge all lots/equipment txn on each table.
DECLARE @vStartDataPrepTime		    DATETIME2;  -- Start time to measure the length of time taken to prepare all lots/equipment txns on all tables for purging and/or archiving.
DECLARE @vEndDataPrepTime		    DATETIME2;  -- End time to measure the length of time taken to prepare all lots/equipment txns on all tables for purging and/or archiving.
DECLARE @vDataPrepExecutionTime     FLOAT; -- Time taken to prepare all lots/equipment txn on all tables for purging and/or archiving.
DECLARE @vStartDataPrepByTableTime		DATETIME2;  -- Start time to measure the length of time taken to insert records of all lots/equipment txns into the CSI_PURGEUTIL_INSTVALUE.
DECLARE @vEndDataPrepByTableTime		DATETIME2;  -- End time to measure the length of time taken to insert records of all lots/equipment txns into the CSI_PURGEUTIL_INSTVALUE
DECLARE @vDataPrepByTableExecutionTime  FLOAT;      -- Time taken 
DECLARE @vStartArchiveByTableTime		DATETIME2;  -- Start time to measure the length of time taken to insert records of all lots/equipment txns into each archive table.
DECLARE @vEndArchiveByTableTime		    DATETIME2;  -- End time to measure the length of time taken to insert records of all lots/equipment txns into each archive table.
DECLARE @vArchiveByTableExecutionTime   FLOAT;      -- Time taken 
--
DECLARE @vTableName                 NVARCHAR(30)='';
DECLARE @vPurgeRequired             INTEGER;
DECLARE @vArchiveRequired           INTEGER;
DECLARE @vCSI_PurgeUtil_RunLog_Tab CSI_PurgeUtil_RunLog_Tab;
DECLARE @vCSI_PurgeUtil_ArchiveLog_Tab CSI_PurgeUtil_ArchiveLog_Tab;
DECLARE @vSetupVersion				INTEGER;
DECLARE @vRecordsAffected           INTEGER=0;
DECLARE @RunLogRowId				INTEGER = 1;
--DECLARE @vLevel                     INTEGER=0;
DECLARE @vStartDataArchTime		    DATETIME2;  -- Start time to measure the length of time taken to archive all lots/equipment txns on all tables.
DECLARE @vEndDataArchTime		    DATETIME2;  -- End time to measure the length of time taken to archive all lots/equipment txns on all tables.
DECLARE @vDataArchExecutionTime     FLOAT;      -- Time taken to archive all lots/equipment txn on all tables.
DECLARE @vStartDataDeleTime		    DATETIME2;  -- Start time to measure the length of time taken to purge all lots/equipment txns for all tables.
DECLARE @vEndDataDeleTime		    DATETIME2;  -- End time to measure the length of time taken to purge all lots/equipment txns for all tables.
DECLARE @vDataDeleExecutionTime     FLOAT;      -- Time taken to purge all lots/equipment txn for all tables.
DECLARE @vStartDataValidateTime		DATETIME2;  -- Start time to measure the length of time taken to validate all the execution rules.
DECLARE @vEndDataValidateTime	    DATETIME2;  -- End time to measure the length of time taken to validate all the execution rules.
DECLARE @vDataValidateExecutionTime FLOAT;      -- Time taken to validate all the execution rules.
DECLARE @vRuleReturnValue           NVARCHAR(255)='OK';
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';
    -- Log a start message 
	SET @vMessageText = @pvSetupName + ' Setup : Purge/archive process started at ' + convert(varchar(25), sysdatetime(), 121);
	EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessageText;
	-- vBatchExecutionId is found in all tables involved in the purging to identify instances that are purged per batch run
    EXECUTE CSI_PurgeUtil_GetInstance @vBatchExecutionId OUTPUT, 'CSI_PURGEUTIL_BATCHEXECUTIONID';
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block
	    SET @vStartProcessTime = sysdatetime();
		-- CSI_PurgeUtil_DeleByTable_tpl commit one table at a time 
		BEGIN TRY
		    SELECT @vArchiveRequired = su.ArchiveRequired
		        , @vPurgeRequired = su.PurgeRequired 
				, @vSetupVersion = su.Version
		    FROM CSI_PURGEUTIL_SETUP su 
		    WHERE UPPER(su.SetupName) = UPPER(@pvSetupName);
        END TRY
        BEGIN CATCH
            SET @vPurgeRequired = 0;   -- Default to 0 if this flag is not setup
            SET @vArchiveRequired = 0; -- Default to 0 if this flag is not setup
        END CATCH
		---------------------------------------------------------------------------
		BEGIN TRY
            BEGIN TRANSACTION;
		    SET @vStartDataPrepTime = sysdatetime();
            DELETE FROM CSI_PURGEUTIL_RUNLOG
            WHERE SETUPNAME=@pvSetupName;
            SET @vRecordsAffected = @@ROWCOUNT;
            COMMIT TRANSACTION;
		    SET @vEndDataPrepTime = sysdatetime();
		    SET @vDataPrepExecutionTime = DateDiff(millisecond, @vStartDataPrepTime, @vEndDataPrepTime);
            -- Capturing Run Statistics
	        --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	        --SELECT @pvSetupName, @vBatchExecutionId, 'Delete records FROM', 'CSI_PURGEUTIL_RUNLOG', @vRecordsAffected, @vDataPrepExecutionTime, @vEndDataPrepTime, 'DELETE FROM CSI_PURGEUTIL_RUNLOG WHERE SETUPNAME=''' + @pvSetupName + ''';'; 
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'DELETE FROM CSI_PURGEUTIL_RUNLOG'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
		---------------------------------------------------------------------------
		BEGIN TRY
            BEGIN TRANSACTION;
		    SET @vStartDataPrepTime = sysdatetime();
            DELETE FROM CSI_PURGEUTIL_INSTVALUE
            WHERE SETUPNAME=@pvSetupName;
            SET @vRecordsAffected = @@ROWCOUNT;
		    SET @vEndDataPrepTime = sysdatetime();
		    SET @vDataPrepExecutionTime = DateDiff(millisecond, @vStartDataPrepTime, @vEndDataPrepTime);
            -- Capturing Run Statistics
	        --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	        --SELECT @pvSetupName, @vBatchExecutionId, 'Delete records FROM', 'CSI_PURGEUTIL_INSTVALUE', @vRecordsAffected, @vDataPrepExecutionTime, @vEndDataPrepTime, 'DELETE FROM CSI_PURGEUTIL_INSTVALUE WHERE SETUPNAME=''' + @pvSetupName + ''';'; 
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'DELETE FROM CSI_PURGEUTIL_INSTVALUE'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
-------------------------------------------------------------------------------------
	    SET @vStartDataPrepTime = sysdatetime();
	    --#PREPAREDATA#
        IF @@TRANCOUNT > 0
            COMMIT TRANSACTION;
		SET @vEndDataPrepTime = sysdatetime();
		SET @vDataPrepExecutionTime = DateDiff(millisecond, @vStartDataPrepTime, @vEndDataPrepTime);
        --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	    --SELECT @pvSetupName, Null, 'Execute data preparation process', Null, Null, @vDataPrepExecutionTime, @vEndDataPrepTime, Null; 
-------------------------------------------------------------------------------------
        IF @pvPurgeType = 'PurgeBySetup' BEGIN 
-------------------------------------------------------------------------------------
	        SET @vStartDataValidateTime = sysdatetime();
            SET @vProgID = @vObject_Name + '.' + 'Executing ValidateExecutionRule'; 
 	        --#VALIDATEDATA#
        END
-------------------------------------------------------------------------------------
        IF @vArchiveRequired = 1 
-------------------------------------------------------------------------------------
        BEGIN
	        SET @vStartDataArchTime = sysdatetime();
            BEGIN TRANSACTION;
		    --#ARCHIVEDATA#
            IF @@TRANCOUNT > 0
                COMMIT TRANSACTION;
		    SET @vEndDataArchTime = sysdatetime();
		    SET @vDataArchExecutionTime = DateDiff(millisecond, @vStartDataArchTime, @vEndDataArchTime);
            --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	        --SELECT @pvSetupName, @vBatchExecutionId, 'Execute data archival process', Null, Null, @vDataArchExecutionTime, @vEndDataArchTime, Null; 
        END;
-------------------------------------------------------------------------------------
        IF @vPurgeRequired = 1 
-------------------------------------------------------------------------------------
        BEGIN
	        SET @vStartDataDeleTime = sysdatetime();
    		--#DELETEBYTABLEDATA#  
		    SET @vEndDataDeleTime = sysdatetime();
		    SET @vDataDeleExecutionTime = DateDiff(millisecond, @vStartDataDeleTime, @vEndDataDeleTime);
            --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	        --SELECT @pvSetupName, @vBatchExecutionId, 'Execute data deletion process', Null, Null, @vDataDeleExecutionTime, @vEndDataDeleTime, Null; 
		END
		SET @vEndProcessTime = sysdatetime();
		SET @vProcessExecutionTime = DateDiff(millisecond, @vStartProcessTime, @vEndProcessTime);
        --INSERT INTO @vCSI_PurgeUtil_RunLog_Tab ( SetupName, BatchExecutionId, Action, TableName, RecordsAffected, ExecutionTime, Creation_Datetime, SQLStatement) 
	    --SELECT @pvSetupName, Null, 'Execute entire process', Null, Null, @vProcessExecutionTime, @vEndProcessTime, Null; 

		--#ARCHIVELOG#
		EXECUTE CSI_PurgeUtil_Global_LogArchiveStat @vCSI_PurgeUtil_ArchiveLog_Tab;
		-- Log a completion message 
	    SET @vMessageText = @pvSetupName + ' Setup : Purge/archive process duration in milliseconds : ' + LTRIM(STR(@vProcessExecutionTime))
                                         + '  BatchExecutionId : ' + @vBatchExecutionId
	                                     + '  Purge/archive process completed at ' + convert(varchar(25), sysdatetime(), 121);
	    EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessageText;
		---------------------------------------------------------------------------
		--
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL'; 
		RETURN 0;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 
			ROLLBACK TRANSACTION;
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
