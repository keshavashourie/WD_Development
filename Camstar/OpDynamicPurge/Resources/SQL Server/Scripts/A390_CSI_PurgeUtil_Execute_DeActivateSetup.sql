ALTER PROCEDURE CSI_PurgeUtil_Execute_DeActivateSetup 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Execute_DeActivateSetup
                     Disable the job so as to prevent it from running.  
  Author           : Benny.Chia 
  Date             : 28 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName                  NVARCHAR(40),
	  @pvMode						NVARCHAR(10) = 'DEACTIVATE' -- DEACTIVATE, DELETE
	) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text.
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               SYSNAME = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vJobName                   SYSNAME;
DECLARE @vReturnCode                INTEGER;
--
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';
    --
	BEGIN TRY
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		SET @vJobName = 'CSI_PurgeUtil_Execute_PurgeBySetup @pvSetupName = ''' + @pvSetupName + ''''

		IF @pvSetupName IS Null 
			RAISERROR ('Setup Name must not be blank', 16, 1);
        IF UPPER(@pvMode) NOT IN ('DEACTIVATE', 'DELETE')
            RAISERROR ('Invalid mode type. Must be DEACTIVATE or DELETE', 16, 1);
		IF NOT EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE (name = @vJobName))
			RAISERROR ('Schdule Job not found', 16, 1);

        -- Log a start message 
        SET @vMessage = @pvMode + ' Job Scheduling for '''  + @pvSetupName + '''' + ' purging started. ';
    	EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        
        --------------------------------------------------------------------------------------------------
        -- Deactivate / delete job
        --------------------------------------------------------------------------------------------------
		BEGIN TRANSACTION;
		IF UPPER(@pvMode) = 'DEACTIVATE'
		BEGIN
			BEGIN TRY 
				SET @vProgID = @vObject_Name + '.Disabling Job'; 
				EXECUTE @vReturnCode = msdb.dbo.sp_update_job 
 					@job_name = @vJobName,
					@enabled  = 0;
			END TRY
			BEGIN CATCH
				SET @ErrorMessage = ERROR_MESSAGE(); 
				SET @vProgID = @vObject_Name + '.' + 'Fail to deactivate the job for setup ' + @pvSetupName; 
    			RAISERROR (@ErrorMessage, 16, 1);
			END CATCH
		END
		ELSE IF UPPER(@pvMode) = 'DELETE'
		BEGIN
			BEGIN TRY 
				SET @vProgID = @vObject_Name + '.Deleting Job';
				EXECUTE msdb.dbo.sp_delete_job
					@job_name = @vJobName;

				UPDATE CSI_PURGEUTIL_SETUP
				SET JobNo = null,
					JobStartDate = null,
					JobEndDate = null,
					JobInterval = null,
					JobIntervalType = null
				WHERE SetupId = (SELECT SetupId FROM CSI_PURGEUTIL_SETUP WHERE SetupName = @pvSetupName);
			END TRY
			BEGIN CATCH
				SET @ErrorMessage = ERROR_MESSAGE(); 
				SET @vProgID = @vObject_Name + '.' + 'Fail to delete the job for setup ' + @pvSetupName; 
    			RAISERROR (@ErrorMessage, 16, 1);
			END CATCH
		END

        --------------------------------------------------------------------------
		IF @@TRANCOUNT > 0 
			COMMIT TRANSACTION;
        -- Log a completion message 
        SET @vMessage = @pvMode + ' Job Scheduling for '''  + @pvSetupName + '''' + ' purging completed. ';
		EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        --
	    SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
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
