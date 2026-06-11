ALTER PROCEDURE CSI_PurgeUtil_Global_LogArchiveStat 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_LogArchiveStat
                     Log execution run statistics of each subprocess into a table
                     whenever user ran a process.
  Author           : Benny.Chia 
  Date             : 23 Feb 2016
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvCSI_PurgeUtil_ArchiveLog_Tab     CSI_PurgeUtil_ArchiveLog_Tab READONLY
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.Validate parameters';
		--IF @pvCSI_PurgeUtil_ArchiveLog_Tab.COUNT = 0 
		--	RAISERROR ('CSI_PurgeUtil_ArchiveLog_Tab must not be empty', 16, 1);
        ---------------------------------------------------------------------------
	    BEGIN TRY 
            BEGIN TRANSACTION;
			INSERT INTO CSI_PurgeUtil_ArchiveLog
			    (
				 SetupName                       
				 , BatchExecutionId
				 , PurgeType
				 , TxnDate
				 , Version
				 , ArchiveDBName
				 , ArchiveSchemaName
			    )
			SELECT DISTINCT
				 SetupName                       
				 , BatchExecutionId
				 , PurgeType
				 , IsNull(TxnDate, CURRENT_TIMESTAMP) 
				 , Version
				 , ArchiveDBName
				 , ArchiveSchemaName
			FROM @pvCSI_PurgeUtil_ArchiveLog_Tab
			;
    	    ;
			--
			IF @@TRANCOUNT > 0 
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
		    IF @@TRANCOUNT > 0 
			    ROLLBACK TRANSACTION;
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'INSERT INTO CSI_PurgeUtil_ArchiveLog.'; 
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
