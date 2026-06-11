------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreGetNextTxnId.sql
-- DESCR:       Queries the appropriate datastore queue table to get the next TxnId to be processed.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreGetNextTxnId' AND Type = 'P')
    DROP PROCEDURE csiDataStoreGetNextTxnId
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO



CREATE PROCEDURE csiDataStoreGetNextTxnId
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreGetNextTxnId
-- Params:      <in>  @pnv_TableName            NVARCHAR(50)
--              <in>  @pbi_LastId               BIGINT
--              <out> @pc_TxnId	                CHAR(16)        OUTPUT
--              <out> @pnv_TxnType              NVARCHAR(10)    OUTPUT
--              <out> @pc_Status                CHAR(1)         OUTPUT
--              <out> @pbi_NextId               BIGINT          OUTPUT
--              <out> @pi_CDOId	                INT             OUTPUT
--              <out> @pnv_Err	                NVARCHAR(MAX)   OUTPUT
--              <out> @psi_FetchStatus          SMALLINT        OUTPUT
--              <out> @pcur_QueueTable          CURSOR VARYING  OUTPUT 
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT          OUTPUT
--
-- Descr:       Queries the appropriate datastore queue table to get the next TxnId to be processed.  Executes on the ODS
--
-- HISTORY:
--              07/12/2016      Dan Maloney	 New procedure to write a record to DataStoreLog table.  
--              04/20/2017      Dan Maloney 	 Removed DECALRE @bi_LastIdUpperBound 
--                                               Removed SET @bi_LastIdUpperBound = @pbi_LastId + 1000
--                                               Added TOP 1000 to query to limit results to first 10000 rows
--              05/12/2017      Alex Lind        Change raiserror to throw and include line number
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/24/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--		07/27/2017      Dan Maloney      Removed unused variables @BIT_DISABLE, @i_CursorStatus (US 51393)
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName            NVARCHAR(50),
      @pbi_LastId               BIGINT,
      @pc_TxnId                 CHAR(16)        OUTPUT,
      @pnv_TxnType              NVARCHAR(10)    OUTPUT,
      @pc_Status                CHAR(1)         OUTPUT,
      @pbi_NextId               BIGINT          OUTPUT,
      @pi_CDOId                 INT             OUTPUT,
      @pnv_Err                  NVARCHAR(MAX)   OUTPUT,
      @psi_FetchStatus          SMALLINT        OUTPUT,
      @pcur_QueueTable          CURSOR VARYING  OUTPUT,
      @pnv_Job                  NVARCHAR(50),
      @pnv_PackageExecuting     NVARCHAR(128),
      @pi_Log_Level             INT,
      @pbi_LogSeq               BIGINT          OUTPUT
)
AS
--Variables defined as constants
DECLARE @BIT_TRUE                     BIT = 1
DECLARE @I_LOG_LEVEL_MAX              INT = 2
DECLARE @I_LOG_LEVEL_MIN              INT = 1
DECLARE @I_LOG_LEVEL_ERROR            INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED      INT = -1
DECLARE @C_SPACE                      CHAR(1) = ' '

DECLARE @bit_WhiteListed              BIT = 0
DECLARE @i_DynamicMsgLogLevel         INT
DECLARE @i_ErrorNumber                INT 
DECLARE @i_ErrorSeverity              INT 
DECLARE @i_ErrorState                 INT 
DECLARE	@i_RowsAffected               INT
DECLARE	@nv_Err                       NVARCHAR(MAX)
DECLARE @nv_ErrorMessage              NVARCHAR(4000)
DECLARE @nv_Loc                       NVARCHAR(64) 
DECLARE	@nv_Msg                       NVARCHAR(MAX)  
DECLARE	@nv_Sql	                      NVARCHAR(4000) 
DECLARE @nv_TxnId                     NVARCHAR(16) 
BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @pbi_NextId = NULL
		SET @pc_TxnId = NULL
		SET @pnv_TxnType = NULL
		SET @pc_Status = NULL
		SET @pi_CDOId = NULL
		SET @pnv_Err = NULL
		SET @nv_Loc = N'csiDataStoreGetNextTxnId'
		

		IF @psi_FetchStatus != 0
		BEGIN
      			SELECT @nv_Sql = N'SET @pcur_QueueTable = CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR SELECT TOP 10000 Id, TxnId, TxnType, Status, CDOId, Error FROM ' 
			+ @pnv_TableName + N'Master WITH(NOLOCK) WHERE ID > @pbi_LastId ORDER BY ID; OPEN @pcur_QueueTable'

			SET @nv_Msg = N'Open queue table cursor for : SELECT TOP 10000 Id, TxnId, TxnType, Status, CDOId, Error FROM ' + @pnv_TableName + N'Master WITH(NOLOCK) WHERE ID > ' + CONVERT(NVARCHAR,@pbi_LastId) 
			+ N' ORDER BY ID'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT

			-- Assign SQL to cursor variable and open cursor
			EXEC sp_executesql @nv_Sql, N'@pbi_LastId BIGINT, @pcur_QueueTable CURSOR OUTPUT', 
				@pbi_LastId = @pbi_LastId, @pcur_QueueTable = @pcur_QueueTable OUTPUT
		END

		FETCH @pcur_QueueTable INTO @pbi_NextId, @pc_TxnId, @pnv_TxnType, @pc_Status, @pi_CDOId, @pnv_Err
		SET @psi_FetchStatus = @@FETCH_STATUS
		
		SET @nv_TxnId = @pc_TxnId
		SET @nv_Msg = N'@pc_TxnId [ ' + ISNULL(@nv_TxnId,@C_SPACE) + N' ] @pnv_TxnType [ ' + ISNULL(@pnv_TxnType,@C_SPACE) + N' ] @pc_Status [ ' + ISNULL(@pc_Status,@C_SPACE) + N' ] @pnv_Err [ ' 
		+ ISNULL(@pnv_Err,@C_SPACE) + N' ] @pi_CDOId [ ' + ISNULL(CONVERT(NVARCHAR,@pi_CDOId),@C_SPACE) + N' ] @pbi_Nextid [ ' + ISNULL(CONVERT(NVARCHAR,@pbi_NextId),@C_SPACE) + N' ]'

		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT

		IF @psi_FetchStatus != 0
		BEGIN
			SET @pc_TxnId = NULL
			SET @pnv_txnType = NULL
			SET @pc_Status = NULL
			SET @pnv_Err = NULL
			SET @pi_CDOId = 0
			SET @pbi_NextId = 0

			CLOSE @pcur_QueueTable
			DEALLOCATE @pcur_QueueTable

			SET @nv_Msg = N'Close queue table cursor.  Queue table cursor contains no rows to fetch.'
			EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT
		END

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
			@pbi_LogSeq = @pbi_LogSeq OUT;
		THROW;
	END CATCH
END
GO



