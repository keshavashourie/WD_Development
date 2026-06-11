ALTER PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetup 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_UpdateSetup
                     Insert/Update a purging setup in the CSI_PURGEUTIL_SETUP table.
  Author           : Benny.Chia 
  Date             : 03 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName					NVARCHAR(40)
	, @pvMainTableName				NVARCHAR(30)
	, @pvMainTableInstanceCol		NVARCHAR(30)
	, @pvArchiveSQLByInstanceId		TEXT = ''
	, @pvArchiveSQL					TEXT = ''
	, @pvRetentionPeriod			FLOAT
	, @pvPurgeRequired              INTEGER = 1
	, @pvArchiveRequired            INTEGER = 1
	, @pvCommitBatchSize            INTEGER = 1000
	, @pvSetupBatchSize             INTEGER = 50
	, @pvDMLOption                  NVARCHAR(255)='OPTION (MAXDOP 14)'
	, @pvLockType                   NVARCHAR(255)=' '
	, @pvValidateExecutionRule1     NVARCHAR(2000) = ''
	, @pvValidateExecutionRule2     NVARCHAR(2000) = ''
	, @pvValidateExecutionRule3     NVARCHAR(2000) = ''
	, @pvBatchViewTemplate			NVARCHAR(255)
	, @pvInstanceViewTemplate		NVARCHAR(255) = ''
	, @pvBatchView			NVARCHAR(255)
	, @pvInstanceView		NVARCHAR(255) = ''
	, @pvRemarks			NVARCHAR(1000) = ''
	, @pvVersion			NVARCHAR(255)
	, @pvSourceDatabase		NVARCHAR(128)
	, @pvSourceSchema		NVARCHAR(128)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE @vFound                     INT=0;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @pvNextInstanceID           NVARCHAR(16);
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
		--
        SET @vFound = 0; 
		EXEC CSI_PurgeUtil_Global_TableExists @vFound OUTPUT, @pvMainTableName, @pvSourceDatabase;
		IF @vFound = 0 
		BEGIN
		    SET @vProgID = @vObject_Name + '.Check Table ' + @pvMainTableName + ' for existence. ';
			SET @ErrorMessage = 'Table ' + @pvMainTableName + ' does not exist';
			RAISERROR (@ErrorMessage, 16, 1);
		END;
		--
        SET @vFound = 0; 
		EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvMainTableName, @pvMainTableInstanceCol, @pvSourceDatabase, @pvSourceSchema;
		IF @vFound = 0 
		BEGIN
		    SET @vProgID = @vObject_Name + '.Check Column ' + @pvMainTableInstanceCol + ' for existence in table ' + @pvMainTableName;
			SET @ErrorMessage = 'Column ' + @pvMainTableInstanceCol + ' does not exist in table ' + @pvMainTableName;
			RAISERROR (@ErrorMessage, 16, 1);
		END;
		-- Remove this validation. Ie : allow negative value.
		--IF @pvRetentionPeriod < 0
		--	RAISERROR ('RetentionPeriod cannot be set less than 0', 16, 1);
		--
		---------------------------------------------------------------------------
        SET @vFound = 0; 
		EXEC CSI_PurgeUtil_TableSetup_SetupExists @vFound OUTPUT, @pvSetupName;
	    BEGIN TRY 
			BEGIN TRANSACTION;
        	IF @vFound = 0 
			BEGIN
				EXECUTE CSI_PurgeUtil_GetInstance @pvNextInstanceID OUTPUT, 'CSI_PURGEUTIL_SEQ';
				INSERT INTO CSI_PurgeUtil_Setup
				(
					SetupId
					, SetupName
					, SetupStatus
					, MainTableName
					, MainTableInstanceCol
					, ArchiveSQLByInstanceId
					, ArchiveSQL
					, TxnDate
					, RetentionPeriod
					, PurgeRequired
					, ArchiveRequired
                    , CommitBatchSize
					, SetupBatchSize
                    , DMLOption
                    , LockType 
					, ValidateExecutionRule1
					, ValidateExecutionRule2
					, ValidateExecutionRule3
					, BatchViewTemplate
					, InstanceViewTemplate
					, BatchView
					, InstanceView
					, Remarks
					, Version
				)
				VALUES
				(
					@pvNextInstanceID
					, @pvSetupName
					, 'SETUP'
					, @pvMainTableName
					, @pvMainTableInstanceCol
					, @pvArchiveSQLByInstanceId
					, @pvArchiveSQL
					, GetDate()
					, @pvRetentionPeriod
					, @pvPurgeRequired
					, @pvArchiveRequired
                    , @pvCommitBatchSize
					, @pvSetupBatchSize
                    , @pvDMLOption
                    , @pvLockType 
					, @pvValidateExecutionRule1
					, @pvValidateExecutionRule2
					, @pvValidateExecutionRule3	
					, @pvBatchViewTemplate
					, @pvInstanceViewTemplate
					, @pvBatchView
					, @pvInstanceView
					, @pvRemarks
					, @pvVersion
				);
			END;
			ELSE
			BEGIN
				SET @vFound = 0; 
				EXEC CSI_PurgeUtil_TableSetup_SetupTableExists @vFound OUTPUT, @pvSetupName, @pvMainTableName;
				IF @vFound > 1 
				BEGIN
		            SET @vProgID = @vObject_Name + '.Disallow if main table ' + @pvMainTableName + ' is also defined as a child table. ';
					SET @ErrorMessage = 'Table ' + @pvMainTableName + ' is defined as a child table for the setup hence cannot be set as the main table.';
					RAISERROR (@ErrorMessage, 16, 1);
				END;
				--
				UPDATE
					CSI_PurgeUtil_Setup
				SET
					SetupName                    = @pvSetupName
					, SetupStatus                = 'SETUP'
					, MainTableName              = @pvMainTableName
					, MainTableInstanceCol       = @pvMainTableInstanceCol
					, ArchiveSQL                 = @pvArchiveSQL
					, TxnDate                    = GetDate()
					, RetentionPeriod            = @pvRetentionPeriod
					, PurgeRequired              = @pvPurgeRequired
					, ArchiveRequired            = @pvArchiveRequired
                    , CommitBatchSize            = @pvCommitBatchSize
					, SetupBatchSize			 = @pvSetupBatchSize
                    , DMLOption                  = @pvDMLOption
                    , LockType                   = @pvLockType 
					, ValidateExecutionRule1     = @pvValidateExecutionRule1
					, ValidateExecutionRule2     = @pvValidateExecutionRule2
					, ValidateExecutionRule3     = @pvValidateExecutionRule3
					, BatchViewTemplate			 = @pvBatchViewTemplate
					, InstanceViewTemplate		 = @pvInstanceViewTemplate
					, BatchView					 = @pvBatchView
					, InstanceView				 = @pvInstanceView
					, Remarks					 = @pvRemarks
					, Version					 = @pvVersion
				WHERE
					UPPER(SetupName) = UPPER(@pvSetupName);
			END;
			---------------------------------------------------------------------------
			-- 	Set the Setup Status to SETUP		
			---------------------------------------------------------------------------
			UPDATE
				CSI_PurgeUtil_Setup
			SET
				SetupStatus = 'SETUP'
			WHERE
				UPPER(SetupName) = UPPER(@pvSetupName);
			--
			IF @@TRANCOUNT > 0
				COMMIT TRANSACTION;
	    END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'Insert/Update a purging setup ' + @pvSetupName; 
    		RAISERROR (@ErrorMessage, 16, 1);
		END CATCH
        --------------------------------------------------------------------------
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
