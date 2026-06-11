------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreLogMessage.sql
-- DESCR:       Stored procedure to write a log message to DataStoreLog table.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreLogMessage' AND Type = 'P')
    DROP PROCEDURE csiDataStoreLogMessage
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE csiDataStoreLogMessage
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreLogMessage
-- Params:      <in>  @pnv_Msg                  NVARCHAR(MAX)   Message Text
--              <in>  @pnv_Loc                  NVARCHAR(64)    Location Message originated
--              <in>  @pi_MsgLogLevel           INT             Message log level that is compared to DataStoreSetUp Log_Level to determine if a messages is logged 
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT          OUTPUT
--
-- Descr:       Stored procedure to write a log message to DataStoreLog table.  Executes on the ODS
--
-- HISTORY:
--	        06/17/2016      Dan Maloney      New procedure to write a record to DataStoreLog table.  
--              07/25/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/25/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variable @nv_Er, @nv_Msg, @i_RowsAffected (US 51393)
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_Msg                  NVARCHAR(MAX),
      @pnv_Loc                  NVARCHAR(64),
      @pi_MsgLogLevel           INT,
      @pnv_Job                  NVARCHAR(50),
      @pnv_PackageExecuting     NVARCHAR(128),
      @pi_Log_Level             INT,
      @pbi_LogSeq               BIGINT             OUTPUT
)
AS
DECLARE @i_ErrorNumber          INT 
DECLARE @i_ErrorSeverity        INT 
DECLARE @i_ErrorState           INT 
DECLARE @nv_ErrorMessage        NVARCHAR(4000)
BEGIN
	SET NOCOUNT ON
	BEGIN TRY

		IF @pi_MsgLogLevel <= @pi_Log_Level
		BEGIN
			BEGIN TRAN
				INSERT 
				INTO DataStoreLog 
				(Job, Package_Executing, Log_Seq, Log_Level, Loc, Message)
				VALUES 
				(@pnv_Job, @pnv_PackageExecuting, @pbi_LogSeq, @pi_MsgLogLevel, @pnv_Loc, @pnv_Msg);

				SET @pbi_LogSeq = @pbi_LogSeq +1
			COMMIT TRAN
		END
	END TRY
	BEGIN CATCH
		SELECT   
		@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
		@i_ErrorSeverity = ERROR_SEVERITY(),  
		@i_ErrorState = ERROR_STATE(),
		@i_ErrorNumber = ERROR_NUMBER()

		IF (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
			ROLLBACK TRAN;

    		THROW;
	END CATCH
END
GO



