ALTER PROCEDURE CSI_PurgeUtil_Global_LogMessage 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_LogMessage
                     Log a start and end message into the message table
                     whenever user ran a process.
  Author           : Benny.Chia 
  Date             : 31 Mar 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvMessageText     NVARCHAR(500)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @iDurationOfLogData         INT=30;  -- in terms of days
DECLARE @iNoOfHistoricalRecs        INT=0;
DECLARE @pvNextInstanceID           NVARCHAR(16);
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.Validate parameters';
		--IF @pvMessageText IS Null 
		--	RAISERROR ('Message Text must not be blank', 16, 1);
        ---------------------------------------------------------------------------
		SET @pvMessageText = ISNULL(@pvMessageText, ' ');
	    BEGIN TRY 
			EXECUTE CSI_PurgeUtil_GetInstance @pvNextInstanceID OUTPUT, 'CSI_PURGEUTIL_MESSAGEID';
			--PRINT 'CSI_PurgeUtil_GetInstance=' + @pvNextInstanceID; 
            BEGIN TRANSACTION;
			INSERT INTO CSI_PurgeUtil_MessageLogs
			    (
				    MessageId
				    , MessageDate
				    , MessageText
			    )
			VALUES
			    (
				    @pvNextInstanceID  
				    , GetDate()
				    , SUBSTRING(@pvMessageText, 1, 500)
			    )
			    ;
			--SELECT 55/0;
			----------------------------------------------------------------------
			-- Housekeep this table based on date of historical records (keep for <iDurationOfLogData> days)
			----------------------------------------------------------------------
			SELECT @iNoOfHistoricalRecs = COUNT(*) FROM CSI_PurgeUtil_MessageLogs
			WHERE MessageDate < (GetDate() - @iDurationOfLogData);    
			IF @iNoOfHistoricalRecs > 0  -- found
			  DELETE FROM CSI_PurgeUtil_MessageLogs 
			  WHERE MessageDate < (GetDate() - @iDurationOfLogData)
			  ;    
			--
			IF @@TRANCOUNT > 0 
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
		    IF @@TRANCOUNT > 0 
			    ROLLBACK TRANSACTION;
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'INSERT INTO CSI_PurgeUtil_MessageLogs.'; 
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
