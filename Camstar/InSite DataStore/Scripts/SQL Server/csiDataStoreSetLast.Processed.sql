------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreSetLastProcessed.sql
-- DESCR:       Updates DataStoreSessionTracking.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreSetLastProcessed' AND Type = 'P')
    DROP PROCEDURE csiDataStoreSetLastProcessed
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO



CREATE PROCEDURE csiDataStoreSetLastProcessed
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreSetLastProcessed
-- Params:      <in>  @pnv_TableName			NVARCHAR(50)
--              <in>  @pnv_TableType			NVARCHAR(8)	
--              <in>  @pc_TxnId					CHAR(16) 
--              <in>  @pbi_NextId				BIGINT
--              <in>  @pnv_TxnType				NVARCHAR(10)
--              <in>  @pc_Status				CHAR(1)
--              <out> @pc_LastTxnId				CHAR(16)			OUTPUT
--              <out> @pbi_LastId				BIGINT				OUTPUT
--              <in>  @pnv_Job					NVARCHAR(50)
--              <in>  @pnv_PackageExecuting		NVARCHAR(128)
--              <in>  @pi_Log_Level				INT
--              <out> @pbi_LogSeq				BIGINT				OUTPUT
--
-- Descr:       Updates DataStoreSessionTracking.  Executes on the ODS
--
-- HISTORY:
--              07/13/2016      Dan Maloney      New procedure to write a record to DataStoreLog table.  
--              06/30/2017      Alex Lind        update to Throw to address PR 48868 
--              07/26/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/26/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/26/2017      Dan Malney       Verified THROW in catch block (US 51393)
--              07/26/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variables @BIT_DISABLE, @nv_Sql (US 51393)
--              10/10/2017      Dan Maloney      Removed code, "DELETE FROM DataStoreMissingTxns where MissedId = @pbi_NextId" (BUG 57436)
--                                               The removal of the record from the DataStoreMissingTxns table has been moved to the csiDataStoreReplicator procedure
--              08/29/2018      Dan Maloney      Modified condition to check @pc_Status to check if @pc_Status IS NOT NULL for CPR 9767 
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName							NVARCHAR(50),
      @pnv_TableType							NVARCHAR(8),
      @pc_TxnId									CHAR(16),
      @pbi_NextId								BIGINT,
      @pnv_TxnType								NVARCHAR(10),
      @pc_Status                                CHAR(1),
      @pc_LastTxnId                             CHAR(16)			OUTPUT,
      @pbi_LastId                               BIGINT				OUTPUT,
      @pnv_Job                                  NVARCHAR(50),
      @pnv_PackageExecuting						NVARCHAR(128),
      @pi_Log_Level                             INT,
      @pbi_LogSeq                               BIGINT				OUTPUT
)
AS

--Variables defined as constants
DECLARE @BIT_TRUE								BIT = 1
DECLARE @I_LOG_LEVEL_MAX						INT = 2
DECLARE @I_LOG_LEVEL_MIN						INT = 1
DECLARE @I_LOG_LEVEL_ERROR						INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED				INT = -1
DECLARE @C_STATUS_ROLLBACK						CHAR(1) = 'R'
DECLARE @C_UPDATES								CHAR(7) = 'UPDATES'
DECLARE @bit_WhiteListed						BIT = 0
DECLARE @i_DynamicMsgLogLevel					INT
DECLARE @i_ErrorNumber							INT 
DECLARE @i_ErrorSeverity						INT 
DECLARE @i_ErrorState							INT 
DECLARE	@i_RowsAffected							INT
DECLARE @nv_DynLoopBackLogSql					NVARCHAR(MAX)
DECLARE	@nv_Err                                 NVARCHAR(MAX)
DECLARE @nv_ErrorMessage						NVARCHAR(4000)
DECLARE @nv_Loc                                 NVARCHAR(64) 
DECLARE	@nv_Msg									NVARCHAR(MAX)  
DECLARE @dt_CurrentDateTime						DATETIME
BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = 'csiDataStoreSetLastProcessed'

		SET @nv_DynLoopBackLogSql = N'EXEC [CSILOOPBACK].[' + DB_NAME() + '].[' +  SCHEMA_NAME() + '].'
		+ 'csiDataStoreLogMessage @pnv_Msg = @nv_Msg, @pnv_Loc = @nv_Loc, @pi_MsgLogLevel = @i_MsgLogLevel, '
		+ '@pnv_Job = @pnv_Job, @pnv_PackageExecuting = @pnv_PackageExecuting, @pi_Log_Level = @pi_Log_Level, @pbi_LogSeq = @pbi_LogSeq OUTPUT'
		
		SET @dt_CurrentDateTime = GETDATE()

		UPDATE DataStoreSessionTracking
		SET 
		ProcessedTxnId = @pc_TxnId,
		ProcessedId = @pbi_Nextid,
		TimeStamp = @dt_CurrentDateTime,
		WaitingId = 0
		WHERE
		SessionName = @pnv_TableName

		SET @i_RowsAffected = @@ROWCOUNT

		SET @nv_Msg = N'[CSILOOPBACK] Advance Camstar transaction.  Set ProcessedTxnId to [ ' + @pc_TxnId + N' ].  Set ProcessedId to [ ' + CONVERT(NVARCHAR, @pbi_NextId) + N' ]'
		EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
			N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
			@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT

		SET @nv_Msg = N'[CSILOOPBACK] Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  UPDATE DataStoreSessionTracking SET ProcessedTxnId = [ ' + @pc_TxnId 
		+ N' ], ProcessedId = [ ' + CONVERT(NVARCHAR, @pbi_NextId) + N' ], TimeStamp = [ ' + CONVERT(NVARCHAR, @dt_CurrentDateTime, 121) + N' ], WaitingId = [ 0 ] WHERE SessionName = [ ' + @pnv_TableName + N' ]'
		EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
			N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
			@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT

		SET @pc_LastTxnId = @pc_TxnId
		SET @pbi_LastId = @pbi_NextId
		
		-- IF @pc_Status is NULL, then no need to check @pnv_TableType as SET_LAST_PROCESSED is being called from out of sync processing loop in REPLICATOR and no transaction
		-- has processed so DATASTORESYNC does not need to be inserted into or deleted from
		IF @pc_Status != @C_STATUS_ROLLBACK AND @pc_Status IS NOT NULL
		BEGIN
			IF (@pnv_TableType = @C_UPDATES)
			BEGIN
				DELETE
				FROM DataStoreSync
				WHERE 
				ProcessedTxnId = @pc_TxnId

				SET @i_RowsAffected = @@ROWCOUNT

				IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
				BEGIN
					SET @nv_Msg = N'[CSILOOPBACK] Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  DELETE FROM DataStoreSync WHERE ProcessedTxnId = [ ' + @pc_TxnId + N' ]'
					EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
						N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
						@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				END
			END
			ELSE
			BEGIN
				INSERT
				INTO DataStoreSync (ProcessedTxnId)
				VALUES
				(@pc_TxnId)

				SET @i_RowsAffected = @@ROWCOUNT

			
				IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
				BEGIN
					SET @nv_Msg = N'[CSILOOPBACK] Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  INSERT INTO DataStoreSync (ProcessedTxnId) VALUES ( [ ' + @pc_TxnId + N' ] )'
					EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
						N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
						@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				END
			END
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


   		SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block) : Last message set : ' + @nv_Msg 
		EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
			N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
			@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT


		SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block) : Exception : ' + @nv_ErrorMessage ; 
		EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
			N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
			@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT;

    		THROW;
	END CATCH
END
GO



