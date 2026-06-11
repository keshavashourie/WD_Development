ALTER PROCEDURE CSI_PurgeUtil_TableSetup_DeleteSetup 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_DeleteSetup
                     Delete an existing purging setup tables CSI_PURGEUTIL_SETUPTABLES.
  Author           : Benny.Chia 
  Date             : 02 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName     NVARCHAR(40)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
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
		---------------------------------------------------------------------------
	    BEGIN TRY 
			BEGIN TRANSACTION;
			-- Delete the Setup tables skipped
            DELETE 
            FROM CSI_PurgeUtil_SetupTablesSkipped
            WHERE SetupId IN (SELECT SetupId FROM CSI_PurgeUtil_Setup WHERE UPPER(SetupName) = UPPER(@pvSetupName));
			-- Delete the Setup tables
            DELETE 
			FROM CSI_PurgeUtil_SetupTables 
			WHERE SetupId IN (SELECT SetupId FROM CSI_PurgeUtil_Setup WHERE UPPER(SetupName) = UPPER(@pvSetupName));
			--
			IF @@TRANCOUNT > 0 
				COMMIT TRANSACTION;
	    END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'Delete purging setup ' + @pvSetupName; 
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
