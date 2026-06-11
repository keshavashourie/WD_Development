------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreLogError.sql
-- DESCR:       Stored procedure to write error to DataStoreErrors.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreLogError' AND Type = 'P')
    DROP PROCEDURE csiDataStoreLogError
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE csiDataStoreLogError
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreLogError
-- Params:      <in>  @pnv_TableName            NVARCHAR(50)
--              <in>  @pc_TxnId                 CHAR(16)		
--              <in>  @pnv_SQLStmt              NVARCHAR(MAX)
--              <in>  @pnv_Err                  VARCAHR(MAX)
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT            OUTPUT
--
-- Descr:       Stored procedure to write error to DataStoreErrors.  Executes on the ODS
--
-- HISTORY:
--              06/17/2016      Dan Maloney      New stored procedure to write error to DataStoreErrors. 
--              07/25/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/25/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/25/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/25/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName            NVARCHAR(50),
      @pc_TxnId                 CHAR(16),
      @pnv_SQLStmt              NVARCHAR(MAX),
      @pnv_Err                  NVARCHAR(MAX),
      @pnv_Job                  NVARCHAR(50),
      @pnv_PackageExecuting     NVARCHAR(128),
      @pi_Log_Level             INT,
      @pbi_LogSeq               BIGINT            OUTPUT
)
AS
--Variables defined as constants
DECLARE @BIT_TRUE                    BIT = 1
DECLARE @I_LOG_LEVEL_MAX             INT = 2
DECLARE @I_LOG_LEVEL_MIN             INT = 1
DECLARE @I_LOG_LEVEL_ERROR           INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED     INT = -1

DECLARE @bit_WhiteListed             BIT = 0
DECLARE @i_DynamicMsgLogLevel        INT
DECLARE @i_ErrorNumber               INT 
DECLARE @i_ErrorSeverity             INT  
DECLARE @i_ErrorState                INT 
DECLARE	@i_RowsAffected              INT
DECLARE	@nv_Loc                      NVARCHAR(64)
DECLARE	@nv_Err                      NVARCHAR(MAX)
DECLARE @nv_ErrorMessage             NVARCHAR(4000)
DECLARE	@nv_Msg                      NVARCHAR(MAX)  
BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = N'LOG_ERROR'
		SET @nv_Msg = N'INSERT INTO DataStoreErrors'

		BEGIN TRAN
			INSERT 
			INTO DataStoreErrors 
			(SessionName, TxnId, Message, SqlStmt)
			VALUES 
			(@pnv_TableName, @pc_TxnId, @pnv_Err, @pnv_SQLStmt) 

			SET @i_RowsAffected = @@ROWCOUNT
		COMMIT TRAN

		SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ] INSERT INTO DataStoreErrors : SessionName [ ' + @pnv_TableName + N' ] TxnId [ ' + @pc_TxnId + N' ]' 
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT

		SET @nv_Msg = N'COMMIT (@@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : Issued after INSERT INTO DataStoreErrors';
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT
	END TRY
	BEGIN CATCH
		SELECT   
		@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
		@i_ErrorSeverity = ERROR_SEVERITY(),  
		@i_ErrorState = ERROR_STATE(),
		@i_ErrorNumber = ERROR_NUMBER()

		IF (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
			ROLLBACK TRAN

		EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
			@pn_ErrorNumber	 = @i_ErrorNumber;

		IF @bit_WhiteListed = @BIT_TRUE
			SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
		ELSE
			SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR;

    		SET @nv_Err = N'Exception Handler (procedure catch block) : Last message set : ' + @nv_Msg 
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Err, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT

		SET @nv_Err = N'Exception Handler (procedure catch block) : Exception : ' + @nv_ErrorMessage ; 
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Err, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT

		SET @nv_Msg = N'Exception Handler (procedure catch block) : ROLLBACK';
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT;

		THROW;
	END CATCH
END
GO



