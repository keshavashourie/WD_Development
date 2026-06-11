ALTER PROCEDURE CSI_PurgeUtil_Rest_tpl 
    ( @pvBatchExecutionId           NVARCHAR(16)
      -- If pvInstanceId is null, then restore the entire batch based on pvBatchExecutionId
      -- If pvInstanceId is not null, then restore the instanceid within the batch, pvBatchExecutionId.
    , @pvInstanceId                 NVARCHAR(16) = Null 
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE @vMessageText               NVARCHAR(500)='';
DECLARE @vRowCount                  INTEGER=0;
DECLARE @vLoopNumber                INTEGER=0;
DECLARE @vTableRowCount             INTEGER=0;
DECLARE @vStartDataRestByTableTime  DATETIME2;  
DECLARE @vEndDataRestByTableTime    DATETIME2;
DECLARE @vDataRestByTableExecutionTime  FLOAT;
DECLARE @vRestoreId                 NVARCHAR(16);
DECLARE @vCSI_PurgeUtil_RunLog_Tab CSI_PurgeUtil_RunLog_Tab;
DECLARE @RunLogRowId				INTEGER = 1;
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
        -----------------------------------------------------------------------
        --#DELETERUNLOG#
        -----------------------------------------------------------------------
	    -- vRestoreId is used to unique identify the restore.  The system allow user to restore 2 or more times for the same batchexecutionid.
        EXECUTE CSI_PurgeUtil_GetInstance @vRestoreId OUTPUT, 'CSI_PURGEUTIL_RESTOREID';
		--#RESTOREDATA#
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
