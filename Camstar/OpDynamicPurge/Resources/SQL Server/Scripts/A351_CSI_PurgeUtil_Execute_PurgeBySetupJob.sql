ALTER PROCEDURE CSI_PurgeUtil_Execute_PurgeBySetupJob
    ( @pvSetupName                      NVARCHAR(40)
	) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text. 
DECLARE @ErrorMessage               NVARCHAR(4000); -- Error Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vSQLStatement              NVARCHAR(4000) = '';
DECLARE @vSetupStatus               NVARCHAR(10);    
DECLARE @pvBatchExecutionId         NVARCHAR(16);   
--
DECLARE @vArchiveSchemaSyncOk       INT=0;
DECLARE @vArchiveRequired           INT=1;
DECLARE @vSetupVersion           	INT=0;
DECLARE @vConfigVersion           	INT=0;
DECLARE @vSourceDatabaseName        nvarchar(255);
DECLARE @vArchiveDatabaseName       nvarchar(255);
DECLARE @vSourceSchemaName          nvarchar(255);
DECLARE @vArchiveSchemaName         nvarchar(255);
DECLARE @vIsActive	                BIT; 
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvSetupName IS Null 
			RAISERROR ('Setup Name must not be blank', 16, 1);
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
        -- Find out if user wants to archive data before purging.
		---------------------------------------------------------------------------
        SELECT @vSetupStatus = su.SetupStatus
            , @vArchiveRequired = su.ArchiveRequired
            , @vSetupVersion = su.Version
			, @vIsActive = su.IsActive
        FROM CSI_PURGEUTIL_SETUP su 
        WHERE UPPER(su.SetupName) = UPPER(@pvSetupName);   
        --
		SET @vProgID = @vObject_Name + '.Validate Setup Status';
        IF @vSetupStatus IS NULL OR @vSetupStatus <> 'ACTIVE'
			RAISERROR ('Setup Status is not set to ACTIVE', 16, 1);
        IF @vIsActive <> 1
			RAISERROR ('Setup is set to inactive', 16, 1);
        --
		SET @vProgID = @vObject_Name + '.Validate Setup Version';
		SELECT @vConfigVersion = c.TVALUE
		FROM CSI_PURGEUTIL_CONFIG c
		WHERE UPPER(c.TNAME) = UPPER('Version');
        IF @vSetupVersion <> @vConfigVersion
		BEGIN
			EXEC CSI_PurgeUtil_Execute_DeActivateSetup @pvSetupName;
			RAISERROR ('Setup Version is different from Configuration Version. Please update the Setup through the Dynamic Purging Tool and rescheduled.', 16, 1);
		END
        --PRINT '@vArchiveRequired=' + Convert(nvarchar, @vArchiveRequired);
		---------------------------------------------------------------------------
        -- Validate that the critical database connection parameters are saved in the CSI_PURGEUTIL_CONFIG table.
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate database connection parameters existence';
        IF @vArchiveRequired = 1 BEGIN
            IF (SELECT COUNT(1)
                FROM CSI_PURGEUTIL_CONFIG cfg 
                WHERE UPPER(cfg.TNAME) IN ('TRANSACTIONDATABASENAME', 'ARCHIVEDATABASENAME', 'TRANSACTIONDATABASESCHEMA', 'ARCHIVESCHEMANAME')) <> 4
                RAISERROR (
		 	    'Database connection parameters are not found in the CSI_PURGEUTIL_CONFIG table. 
		 	     To save these parameters, open the Dynamic Purging Tool and navigate to the Purging Database Configuration section.
				 In this section, update both the Transaction Database and Archive Database fields, then click save to store the updated connection parameters'
			   , 16, 1);
		    ---------------------------------------------------------------------------
            -- Schema Syncronization
		    ---------------------------------------------------------------------------
            SELECT @vSourceDatabaseName = c.TVALUE
            FROM CSI_PURGEUTIL_CONFIG c
            WHERE UPPER(c.TNAME) = UPPER('TransactionDatabaseName');
            -- 
            SELECT @vArchiveDatabaseName = c.TVALUE
            FROM CSI_PURGEUTIL_CONFIG c
            WHERE UPPER(c.TNAME) = UPPER('ArchiveDatabaseName');
            -- 
            SELECT @vSourceSchemaName = c.TVALUE
            FROM CSI_PURGEUTIL_CONFIG c
            WHERE UPPER(c.TNAME) = UPPER('TransactionDatabaseSchema');
            -- 
            SELECT @vSourceSchemaName = c.TVALUE
            FROM CSI_PURGEUTIL_CONFIG c
            WHERE UPPER(c.TNAME) = UPPER('ArchiveSchemaName');
                -- 
		    SET @vProgID = @vObject_Name + '.Schema Syncronization in progress...';
            EXECUTE CSI_PurgeUtil_CreateORAlterAllArchiveTable @vSourceDatabaseName, @vArchiveDatabaseName, @vSourceSchemaName, @vSourceSchemaName;
        END
		---------------------------------------------------------------------------
        -- Proceed only if the status is ACTIVE 
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Purging/archiving in progress...';
        IF @vSetupStatus = 'ACTIVE' BEGIN
            EXECUTE CSI_PurgeUtil_Execute_PurgeBySetup 
                @pvSetupName = @pvSetupName;
        END 
        --------------------------------------------------------------------------
        --
	    SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
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
