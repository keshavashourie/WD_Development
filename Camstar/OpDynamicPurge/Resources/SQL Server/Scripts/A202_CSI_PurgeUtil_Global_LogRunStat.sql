ALTER PROCEDURE CSI_PurgeUtil_Global_LogRunStat 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_LogRunStat
                     Log execution run statistics of each subprocess into a table
                     whenever user ran a process.
  Author           : Benny.Chia 
  Date             : 23 Feb 2016
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvCSI_PurgeUtil_RunLog_Tab     CSI_PurgeUtil_RunLog_Tab READONLY
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
		--IF @pvCSI_PurgeUtil_RunLog_Tab.COUNT = 0 
		--	RAISERROR ('CSI_PurgeUtil_RunLog_Tab must not be empty', 16, 1);
        ---------------------------------------------------------------------------
	    BEGIN TRY 
            BEGIN TRANSACTION;
			INSERT INTO CSI_PurgeUtil_RunLog
			    (
				    SetupName
				    , RowId
				    , BatchExecutionId
                    , RestoreId
                    , RunStage
				    , Action
				    , TableName
				    , RecordsAffected
                    , gLevel
				    , ExecutionTime
				    , SQLStatement
				    , Creation_Datetime
			    )
			SELECT DISTINCT
			     SetupName
			     , RowId
				 , BatchExecutionId
                 , RestoreId
                 , RunStage
 				 , Action
                 , TableName
                 , RecordsAffected
                 , gLevel
                 , ExecutionTime
                 , SQLStatement
				 , IsNull(creation_datetime, CURRENT_TIMESTAMP) 
			FROM @pvCSI_PurgeUtil_RunLog_Tab
			ORDER BY SetupName, RowId
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
			SET @vProgID = @vObject_Name + '.' + 'INSERT INTO CSI_PurgeUtil_RunLog.'; 
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
