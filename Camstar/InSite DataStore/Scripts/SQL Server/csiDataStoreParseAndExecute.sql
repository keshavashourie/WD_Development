------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreParseAndExecute.sql
-- DESCR:       Main data store "worker" procedure.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreParseAndExecute' AND Type = 'P')
	DROP PROCEDURE csiDataStoreParseAndExecute
GO

CREATE PROCEDURE csiDataStoreParseAndExecute
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreParseAndExecute
-- Params:      <in> @pnv_TableName             NVARCHAR(50)
--              <in> @pnv_TableType             NVARCHAR(8)
--              <in> @pc_TxnId                  CHAR(16)
--              <in> @pnv_TxnType               CHAR(1)
--              <in> @pnv_DataStoreDelimiter    NVARCHAR(10)	
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT            OUTPUT
-- Descr: 
-- 		This program performs the below operations:
--		Parses the incoming OLTP transaction CLOB
--		Splits the string into individual DML statements
--		Executes each individual statement	
--
-- HISTORY:
--              05/12/2017      Alex Lind        update Raiserror to Throw, fix error message for duplicate key,    PR 48868
--              07/26/2017	    Dan Maloney      Removed BEGIN TRAN and COMMIT (US 51393)
--              07/26/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/26/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/26/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/26/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/26/2017      Dan Maloney      Replaced RAISERROR with THROW statement
--              07/28/2017      Dan Maloney      Remove unused variables @BIT_DISABLE, @BIT_FALSE, @C_DUMMYTYPE, @C_INSERTS, @C_MISSED_MISSING, @C_MISSED_UNCOMMITTED (US 51393)
--              07/28/2017      Dan Maloney      Remove unused variables @C_STATUS_PROCESS, @C_STATUS_ROLLBACK, @C_STATS_UNCOMMITTED, @C_YES (US 51393)
--              07/28/2017      Dan Maloney      Removed log messaging from exception handler as it will be handled in REPLICATOR (US 51393)
--              10/28/2019      Dan Maloney      Changes @v_Buf VARCHAR(MAX) to @nv_Buf VARCAHR(MAX) and @v_SqlStmt VARCHAR(MAX) to @nv_SqlStmt NVARCHAR(MAX).  
--                                               Added missing N in front of strings assigned to NVARCHAR variables.
--              05/27/2020      Dan Maloney      Removed logic log log a reveral message in DataStoreLog if the Camstar transactions contain reversals CPR 82735 (TFS05)
--                                               for v8.3 and CPR 82747 (TFS05) for V7 MU2.  These 2 CPRs have parent PR 26237 (TFS05) which has parent IR 94110101
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName                  NVARCHAR(50),
      @pnv_TableType                  NVARCHAR(8),
      @pc_TxnId                       CHAR(16), 
      @pnv_TxnType                    CHAR(1),
      @pnv_DataStoreDelimiter         NVARCHAR(10),
      @pnv_Job                        NVARCHAR(50),
      @pnv_PackageExecuting           NVARCHAR(128),
      @pi_Log_Level                   INT,
      @pbi_LogSeq                     BIGINT            OUTPUT
)
AS 
--Variables defined as constants
DECLARE @BIT_TRUE                      BIT = 1
DECLARE @I_LOG_LEVEL_MAX               INT = 2
DECLARE @I_LOG_LEVEL_MIN               INT = 1
DECLARE @I_LOG_LEVEL_ERROR             INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED       INT = -1
DECLARE @C_NO                          CHAR(1) = 'N'
DECLARE @c_TXNTYPE                     CHAR(1) = 'T'
DECLARE @C_UPDATE                      CHAR(1) = 'U' 
DECLARE @C_UPDATES                     CHAR(7) = 'UPDATES'  

DECLARE @bit_WhiteListed               BIT = 0
DECLARE @cur_TranSql                   CURSOR 
DECLARE @i_DelimLen                    INT = 0
DECLARE @i_DynamicMsgLogLevel          INT
DECLARE @i_ErrorNumber                 INT 
DECLARE @i_ErrorSeverity               INT  
DECLARE @i_ErrorState                  INT
DECLARE @i_LastOffset                  INT = 1
DECLARE @i_Offset                      INT = 1 
DECLARE	@i_RowsAffected                INT = 0
--DECLARE @i_ReversalLen                 INT = 0
DECLARE @i_Seq                         INT = 0
DECLARE @c_Stop_On_No_Update           CHAR(1)
DECLARE @nv_Buf                        NVARCHAR(MAX) 
DECLARE @nv_DynLoopBackErrorSql        NVARCHAR(4000)
DECLARE @nv_DynLoopBackLogSql          NVARCHAR(4000)
DECLARE	@nv_Err                        NVARCHAR(MAX) 
DECLARE @nv_ErrorMessage               NVARCHAR(4000)
DECLARE @nv_Loc	                       NVARCHAR(64) 
DECLARE	@nv_Msg                        NVARCHAR(MAX) 
DECLARE @nv_Sql	                       NVARCHAR(4000)
DECLARE @nv_SqlStmt                    NVARCHAR(MAX) 

BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = N'csiDataStoreParseAndExecute'
		SET @nv_DynLoopBackErrorSql = N'EXEC [CSILOOPBACK].[' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].'
		+ N'csiDataStoreLogError @pnv_TableName = @pnv_TableName, @pc_TxnId = @pc_TxnId, @pnv_SqlStmt = @nv_Buf, @pnv_Err = @nv_Err, '
		+ N'@pnv_Job = @pnv_Job, @pnv_PackageExecuting = @pnv_PackageExecuting, @pi_Log_Level = @pi_Log_Level, @pbi_LogSeq = @pbi_LogSeq OUTPUT'

		SET @nv_DynLoopBackLogSql = N'EXEC [CSILOOPBACK].[' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].'
		+ N'csiDataStoreLogMessage @pnv_Msg = @nv_Msg, @pnv_Loc = @nv_Loc, @pi_MsgLogLevel = @i_MsgLogLevel, '
		+ N'@pnv_Job = @pnv_Job, @pnv_PackageExecuting = @pnv_PackageExecuting, @pi_Log_Level = @pi_Log_Level, @pbi_LogSeq = @pbi_LogSeq OUTPUT'

		IF (@pnv_TxnType='D') 
		BEGIN 
			IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
			BEGIN
				SET @nv_Msg = N'[CSILOOPBACK] No SQL Statements to parse for Transaction {@pc_TxnId} [ ' + @pc_TxnId + N' ].'
				EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				
			END
			RETURN 
		END 

		SET @i_DelimLen = LEN(@pnv_DataStoreDelimiter)

		SET @nv_Sql = N'SET @pcur_TranSql = CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR SELECT Sequence, SqlStmt FROM ' + @pnv_TableName + N' WITH(NOLOCK) WHERE TxnId = @pc_TxnId ORDER BY Sequence; OPEN @pcur_TranSql' 	
		EXEC sp_executesql @nv_Sql, N'@pcur_TranSql CURSOR OUTPUT, @pc_TxnId CHAR(16)', 
			@pc_TxnId = @pc_TxnId, @pcur_TranSql = @cur_TranSql OUTPUT

		FETCH NEXT FROM @cur_TranSql INTO @i_Seq, @nv_SqlStmt 

		WHILE @@FETCH_STATUS = 0 
		BEGIN -- (0)
			SET @i_LastOffSet = 1
			SET @i_OffSet = 1
				
			IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
			BEGIN
				SET @nv_Msg = N'[CSILOOPBACK] BEGIN SQL Statements for Transcation {@pc_TxnId} [ ' + @pc_TxnId + N' ]'
				EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				
			END

			WHILE @i_OffSet != 0
			BEGIN -- (1a)
				SET @i_OffSet = CHARINDEX(@pnv_DataStoreDelimiter, @nv_SqlStmt, @i_LastOffSet)

				IF @i_OffSet != 0
				BEGIN -- (2a)
					SET @nv_Buf = SUBSTRING(@nv_SqlStmt , @i_LastOffSet, @i_OffSet - @i_LastOffSet)

					IF (LEN(@nv_Buf) > 0)
					BEGIN --(3)
						SET @nv_Msg = N'SQL [ ' + @nv_Buf + N' ]';  
						EXEC (@nv_Buf) 
						SET @i_RowsAffected = @@ROWCOUNT

						IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
						BEGIN
							SET @nv_Msg = N'[CSILOOPBACK]       Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ] ' + @nv_Buf
							EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
								N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
								@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
								
						END

    					-- Check the result of the statement if this is the
						-- UPDATE queue and it's a regular transaction.
						-- If no rows were affected on an Update
						-- statement, log an error (but continue processing).

						IF (@pnv_TxnType = @C_TXNTYPE AND @pnv_TableType = @C_UPDATES) 
							IF (@i_RowsAffected = 0)
								IF (SUBSTRING(@nv_Buf, 1, 1) = @C_UPDATE)
								BEGIN
									SET @nv_Msg = N'NoRowsOnUpdate';
									THROW 50500, @nv_Msg, 1;
								END 
					END -- (3)

					SET @i_OffSet = @i_OffSet + @i_DelimLen
					SET @i_LastOffSet = @i_OffSet
				END -- (2a)
			END -- (1a) WHILE @i_OffSet ! =0

			FETCH NEXT FROM @cur_TranSql INTO @i_Seq, @nv_SqlStmt
		END -- (0) WHILE @@FETCH_STATUS = 0 

		IF @I_LOG_LEVEL_MAX <= @pi_Log_Level
		BEGIN
			SET @nv_Msg = N'[CSILOOPBACK] END SQL Statements for Transcation {@pc_TxnId} [ ' + @pc_TxnId + N' ]'
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Msg, @nv_Loc, @I_LOG_LEVEL_MAX, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				
		END

		CLOSE @cur_TranSql
		DEALLOCATE @cur_TranSql
	END TRY
	BEGIN CATCH
  		SELECT   
		@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
		@i_ErrorSeverity = ERROR_SEVERITY(),  
		@i_ErrorState = ERROR_STATE(),
		@i_ErrorNumber = ERROR_NUMBER()

		EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
			@pn_ErrorNumber	 = @i_ErrorNumber;

		IF @bit_WhiteListed = @BIT_TRUE
			SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
		ELSE
			SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR;

		IF @i_ErrorNumber = 50500
		BEGIN -- (1a)
			-- No rows updated
			SELECT @c_Stop_On_No_Update = Value FROM DataStoreSetUp WHERE Parameter = 'STOP_ON_NO_UPDATE';

			IF @c_Stop_On_No_Update = @C_NO
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
				SET @nv_Err = N'[CSILOOPBACK] STOP_ON_NO_UPDATE =  "N" : No rows updated'
			END
			ELSE
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR
				SET @nv_Err = N'[CSILOOPBACK] STOP_ON_NO_UPDATE =  "Y" : No rows updated'
			END
			
			EXEC SP_EXECUTESQL @nv_DynLoopBackErrorSql, 
				N'@pnv_TableName NVARCHAR(50), @pc_TxnId CHAR(16), @nv_Buf VARCHAR(MAX), @nv_Err NVARCHAR(MAX), @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT',
				@pnv_TableName, @pc_TxnId, @nv_Buf, @nv_Err, @pnv_Job, @pnv_PackageExecuting, @i_DynamicMsgLogLevel, @pbi_LogSeq OUTPUT
				
								
			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block NoRowsOnUpdate) : No Rows Updated from SQL [ ' + @nv_Buf + N' ]' 
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @i_DynamicMsgLogLevel, @pbi_LogSeq OUTPUT
				

			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block NoRowsOnUpdate) : See DataStoreErrors for details.  TXNID [ ' + @pc_TxnId + N' ]'
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @i_DynamicMsgLogLevel, @pbi_LogSeq OUTPUT;
				
		END -- (1a)
		ELSE IF @i_ErrorNumber = 2601 OR @i_ErrorNumber = 2627 --DuplicateKeyError.  Duplicate primary key value or duplicate unique index value
		BEGIN -- (2a)
			-- Duplicate (unique constraint violated)
			EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
				@pn_ErrorNumber	 = @i_ErrorNumber;

			IF @bit_WhiteListed = @BIT_TRUE
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
				SET @nv_Err = N'[CSILOOPBACK] WhiteListed : ' + @nv_ErrorMessage
			END
			ELSE
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR
				SET @nv_Err = N'[CSILOOPBACK] : ' + @nv_ErrorMessage
			END
			
			EXEC SP_EXECUTESQL @nv_DynLoopBackErrorSql, 
				N'@pnv_TableName NVARCHAR(50), @pc_TxnId CHAR(16), @nv_Buf VARCHAR(MAX), @nv_Err NVARCHAR(MAX), @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT',
				@pnv_TableName, @pc_TxnId, @nv_Buf, @nv_Err, @pnv_Job, @pnv_PackageExecuting, @i_DynamicMsgLogLevel, @pbi_LogSeq OUTPUT
				
								
			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block DuplicateKeyError) : Duplicate Value on Index from SQL [ ' + @nv_Buf + N' ]' 
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				

			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block DuplicateKeyError) : See DataStoreErrors for details.  TXNID [ ' + @pc_TxnId + N' ]'
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT;
				
		END -- (2a)
		ELSE
		BEGIN -- (2b)
			--Other error

			EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
				@pn_ErrorNumber	 = @i_ErrorNumber;

			IF @bit_WhiteListed = @BIT_TRUE
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
				SET @nv_Err = N'[CSILOOPBACK] WhiteListed : ' + @nv_ErrorMessage
			END
			ELSE
			BEGIN
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR
				SET @nv_Err = N'[CSILOOPBACK] : ' + @nv_ErrorMessage
			END
			
			EXEC SP_EXECUTESQL @nv_DynLoopBackErrorSql, 
				N'@pnv_TableName NVARCHAR(50), @pc_TxnId CHAR(16), @nv_Buf VARCHAR(MAX), @nv_Err NVARCHAR(MAX), @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT',
				@pnv_TableName, @pc_TxnId, @nv_Buf, @nv_Err, @pnv_Job, @pnv_PackageExecuting, @i_DynamicMsgLogLevel, @pbi_LogSeq OUTPUT
				
								
			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block when others) : Last message set : ' + @nv_Msg 
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT
				

			SET @nv_Err = N'[CSILOOPBACK] Exception Handler (procedure catch block when others) : See DataStoreErrors for details.  TXNID [ ' + @pc_TxnId + N' ]'
			EXEC SP_EXECUTESQL @nv_DynLoopBackLogSql, 
				N'@nv_Msg NVARCHAR(MAX), @nv_Loc NVARCHAR(64), @i_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(129), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT', 
				@nv_Err, @nv_Loc, @i_DynamicMsgLogLevel, @pnv_Job, @pnv_PackageExecuting, @pi_Log_Level, @pbi_LogSeq OUTPUT;
				
		END; -- (2b)

		-- Raise error back to csiDataStoreReplicator
		THROW;
	END CATCH
END
GO

