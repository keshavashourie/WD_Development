ALTER PROCEDURE CSI_PurgeUtil_Global_Log 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_Log
                     Execution log : Record a row into the CSI_PurgeUtil_Errorlog table
					 whenever user ran the purging script.   For every stored procedure or function,
					 it will record a 'START' row at the beginning of the sp/function and a 'SUCCESSFUL' 
					 row at the end of the sp/function.  As and when needed, any lines between the 'START'
					 and 'SUCCESSFUL' can be recorded into the CSI_PurgeUtil_Errorlog table.
                     The purpose of recording is to enable program line tracing when the user run the purging 
					 script to purge lots transactions, equipments transactions, and other type of transactions.
					 With recording in place, the task of troubleshooting the program should the execution
					 returns error(s) can be easily traced and therefore resolved.
  Author           : Benny.Chia 
  Date             : 01 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : CSI_PurgeUtil_ErrorLog_Record()
--------------------------------------------------------------------------- */    
    ( @pvProgId              NVARCHAR(255)
	, @pvModuleType          NVARCHAR(40) = 'EXECUTION'
	, @pvErrMsg              NVARCHAR(4000) = Null
	, @pvErrCode             NVARCHAR(255)  = Null
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
--
BEGIN 
	SET NOCOUNT ON;
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
        --
		---------------------------------------------------------------------------
        --IF @pvErrMsg IS NOT NULL BEGIN
            --PRINT '@pvErrMsg IS NOT NULL';
		    INSERT INTO @vCSI_PurgeUtil_ErrorLog_Tab (modulename, moduletype, progid, ErrMsg, ErrCode) SELECT 'PURGING', @pvModuleType, @pvProgId, @pvErrMsg, @pvErrCode; 
		    EXECUTE CSI_PurgeUtil_ErrorLog_Record @vCSI_PurgeUtil_ErrorLog_Tab;
        --END
        /*
        ELSE IF (SELECT t.TVALUE FROM CSI_PURGEUTIL_CONFIG t WHERE t.TNAME = 'DebugEnabled' ) = 'Y' 
        BEGIN
            --PRINT '@pvErrMsg IS NULL';
		    INSERT INTO @vCSI_PurgeUtil_ErrorLog_Tab (modulename, moduletype, progid, ErrMsg, ErrCode) SELECT 'PURGING', @pvModuleType, @pvProgId, @pvErrMsg, @pvErrCode; 
		    --PRINT @pvModuleType
		    --PRINT @pvProgId
		    --PRINT @pvErrMsg
		    --PRINT @pvErrCode
		    EXECUTE CSI_PurgeUtil_ErrorLog_Record @vCSI_PurgeUtil_ErrorLog_Tab;
        END
        */
        --------------------------------------------------------------------------
	    RETURN 0;                   
	END TRY
	BEGIN CATCH
		-- Use RAISERROR inside the CATCH block to return error information about the original error that caused execution to jump to the CATCH block.
		SET @ErrorMessage = ' OTHER ERROR : ' + ERROR_MESSAGE(); SET @ErrorSeverity = ERROR_SEVERITY(); SET @ErrorState = ERROR_STATE();
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
