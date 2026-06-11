------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreReplicator.sql
-- DESCR:       Main data store "worker" procedure.  Executes on the ODS
-- Copyright Siemens 2023 
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreReplicator' AND Type = 'P')
	DROP PROCEDURE csiDataStoreReplicator
GO

CREATE PROCEDURE csiDataStoreReplicator
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreReplicator
-- Params:      <in> @pnv_TableName      NVARCHAR(100)
--              <in> @pbit_Debug         BIT 1 means Debug and run just one iteration, 0 means no debug
--
-- Descr:       Deletes processed records.  Executes on the ODS
--		
-- HISTORY:
--              06/28/2005                       Added support for Master/Detail architecture.
--              07/15/2005                       Added missing transaction support.
--              01/30/2006                       Added WaitingId parameter (not used here)
--              02/20/2006                       Removed gRemoteDB check.
--              03/01/2006                       Added STOP_IF_RETRIES_EXCEEDED parameter (not used here)
--              08/11/2006                       Changed nvarchar columns to MAX
--              12/06/2006                       Updated/added copyright notice(s) (SPR S9984) Bill Lippard.
--              04/23/2007                       Updated copyright notice(s) (SPR S9984) Bill Lippard.
--              06/28/2016      Dan Maloney      Resturctured Code
--              03/01/2017      Alex Lind        remove orphaned variable.
--              03/03/2017      Alex Lind        move validation of indexes and columns to manager
--              03/03/2017      Alex Lind        Update job names to database based. 
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/24/2017      Dan Maloney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/24/2017      Dan Maloney      Replaced RAISERROR with THROW statement
--		        07/26/2017      Dan Maloney      Removed unused variables  @C_NO, @C_DUMMYTYPE, @C_INSERTS, @C_TXNTYPE, @C_UPDATES (US 51393)
--		        07/26/2017      Dan Maloney      Removed unused variables  @ti_DeadLockRetries, @ti_MaxDeadLockRetries, @bit_VerifyHost, @i_CntIdx (US 51393)
--		        07/26/2017      Dan Maloney      Removed unused variables  @nv_SessionName, @nv_Sql,  @nv_DynLoopBackLogSql  (US 51393)
--              10/09/2017      Dan Maloney      Added SET DEADLOCK_PRIORITY 6 (BUG 57522)
--				10/10/2017      Dan Maloney      Added logic to check (IF @c_Stop_If_Retries_Exceeded = 'N') logic (US 57436)
--				10/10/2017      Dan Maloney      Added "DELETE FROM DataStoreMissingTxns..." logic after call to csiDataStoreParseAndExecute and 
--                                               removed the "DELETE FROM DataStoreMissingTxns..." logic from csiDataStoreSetLastProcessed  (US 57436)
--              10/11/2017      Dan Maloney      Removed unused variables  @C_YES (US 57436)
--              06/13/2018      Dan Maloney      Modifications for Verify_Host for US 4122 
--              07/17/2018      Dan Maloney      Modified csiDataStoreReplicator to add SESSIONNAME to where clause for DELETE FROM DATASTOREMISSINGTXNS statement for CPR 8538 
--              08/29/2018      Dan Maloney      Modified REPLICATOR call to SET_LAST_PROCESSED to pass NULL for v_Status parameter in the <IF gSTOP_IF_RETRIES_EXCEEDED = FALSE  THEN> block 
--                                               and pass n_NextId-1 for pn_Id parameter for CPR 9767 
--              01/31/2019      Dan Maloney      Implemented work by Alex Lind in V6 for Bug 14453 ODS: flag STOP_IF_RETRIES_EXCEEDED = 'N' does not work correctly with UNCOMMITTED errors.
--                                               Changed Retries exceeded logic to handle Uncommitted and Missing IDs seperately. 
--
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
        @pnv_TableName                  NVARCHAR(100),
        @pbit_Debug                     BIT = 0		
)
AS
-- Variables defined as constants
DECLARE @BIT_RESURRECT                  BIT = NULL
DECLARE @BIT_DISABLE                    BIT = 0
DECLARE @BIT_FALSE						BIT = 0
DECLARE @BIT_TRUE                       BIT = 1
DECLARE @I_LOG_LEVEL_MAX                INT = 2
DECLARE @I_LOG_LEVEL_MIN                INT = 1
DECLARE @I_LOG_LEVEL_ERROR              INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED        INT = -1
DECLARE @C_NO                           CHAR(1) = 'N'
DECLARE @C_YES                          CHAR(1) = 'Y'
DECLARE @C_RESURRECT                     CHAR(1) = 'R'
DECLARE @C_MISSED_MISSING               CHAR(7) = 'MISSING' 
DECLARE @C_MISSED_UNCOMMITTED           CHAR(11) = 'UNCOMMITTED'  
DECLARE @C_STATUS_PROCESS               CHAR(1) = 'P'
DECLARE @C_STATUS_ROLLBACK              CHAR(1) = 'R'
DECLARE @C_STATUS_UNCOMMITTED           CHAR(1) = 'U'

DECLARE @cur_QueueTable                 CURSOR
DECLARE @si_CursorStatus                SMALLINT
DECLARE @si_FetchStatus                 SMALLINT
DECLARE @bit_InsertsPending             BIT
DECLARE @bit_Slept                      BIT = 0
DECLARE @bit_WhiteListed                BIT = 0
DECLARE @i_DynamicMsgLogLevel           INT
DECLARE @i_MissTxnCnt                   INT
DECLARE @i_CDOId                        INT
DECLARE @i_ErrorNumber                  INT 
DECLARE @i_ErrorSeverity                INT  
DECLARE @i_ErrorState                   INT
DECLARE @bi_ConnectionLost				INT
DECLARE	@i_RowsAffected	                INT = 0
DECLARE	@i_RowCount                     INT = 0
DECLARE @i_TransactionCnt               INT = 0
DECLARE @bi_LogSeq                      BIGINT
DECLARE @bi_LastMissedId                BIGINT
DECLARE @bi_FirstMissedId               BIGINT
DECLARE @bi_NextId                      BIGINT
DECLARE @bi_ReplicatorSeq               BIGINT
DECLARE @c_Status                       CHAR(1)
DECLARE @c_TxnId                        CHAR(16)
DECLARE	@nv_Err                         NVARCHAR(MAX) 
DECLARE @nv_ErrorMessage                NVARCHAR(4000)
DECLARE @nv_Job	                        NVARCHAR(50)
DECLARE @nv_JobName                     NVARCHAR(50)
DECLARE @nv_Loc                         NVARCHAR(64) 
DECLARE	@nv_Msg                         NVARCHAR(MAX) 
DECLARE	@nv_PackageExecuting            NVARCHAR(128) 
DECLARE @nv_Sql                         NVARCHAR(4000)
DECLARE @nv_TxnType                     NVARCHAR(10)
DECLARE @nv_Type						NVARCHAR(128)
DECLARE @dt_CurrentDateTime             DATETIME

-- Declare csiDataStoreInit OUTPUT variables
DECLARE @nv_TableType                   NVARCHAR(8)
DECLARE @c_LastTxnId                    CHAR(16)
DECLARE @bi_LastId                      BIGINT
DECLARE @bi_WaitingId                   BIGINT
DECLARE @nv_Access_Mode                 NVARCHAR(8)
DECLARE @i_Cleanup_Batch_Size           INT
DECLARE @nv_DataStoreDelimiter          NVARCHAR(10)
DECLARE @i_DataStoreInsertTables        INT
DECLARE @c_DataStorePresent             CHAR(1)
DECLARE @c_DataStore_Terminate          CHAR(1)
DECLARE @i_Insert_Update_Batch_Size     INT
DECLARE @i_WaitTime                     INT
DECLARE @c_Keep_Remote_Records          CHAR(1)
DECLARE @i_Log_Level                    INT
DECLARE @i_Log_Retention                INT
DECLARE @i_Missing_Txn_Retries          INT
DECLARE @c_Stop_If_Retries_Exceeded     CHAR(1)
DECLARE @c_Stop_On_Duplicate_Insert     CHAR(1)
DECLARE @c_Stop_On_No_Update            CHAR(1)
DECLARE @c_Verify_Host                  CHAR(1)
DECLARE @nv_Version                     NVARCHAR(10)

BEGIN
   	SET NOCOUNT ON
	SET DEADLOCK_PRIORITY 6
	BEGIN TRY
		SET @nv_JobName = N'csiDataStoreReplicator(' + '''' + @pnv_TableName + '''' + N')' 
		SET @nv_Loc = N'csiDataStoreReplicator'

		SET @nv_PackageExecuting = @nv_JobName
		SET @bi_LogSeq = 1
		
		SELECT 
		@i_Log_Level = CONVERT(INT,VALUE) 
		FROM DataStoreSetUp WITH(NOLOCK)
		WHERE Parameter = 'LOG_LEVEL'

		-- Get Job ID for logging
   		SELECT 
		@nv_Job = j.Job_Id
		FROM msdb..sysjobs_view j WITH(NOLOCK)
		WHERE j.Name = @nv_JobName + N' (' + DB_NAME() + N')'

		IF @@ROWCOUNT = 0
		BEGIN
			SET @nv_Msg = N'Job : ' + @nv_JobName + N' doesnt exist for database ' + DB_NAME();
			THROW 50200, @nv_Msg, 1
		END

		SET @nv_Msg = N'Update job information in procedure variable'
		SET @nv_Loc = N'[ 0 ] csiDataStoreReplicator Initialization'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @bi_ReplicatorSeq = 1
		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] Begin'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Loc = N'csiDataStoreReplicator'

		-- Call csiDataStoreInit
		SET @nv_Msg = N'csiDataStoreInit ' + @pnv_TableName 
		EXEC csiDataStoreInit 
			@pnv_TableName = @pnv_TableName,
			@pnv_TableType = @nv_TableType OUT, 
			@pc_LastTxnId = @c_LastTxnId OUT, 
			@pbi_LastId = @bi_LastId OUT, 
			@pbi_WaitingId = @bi_WaitingId OUT, 
			@pnv_Access_Mode = @nv_Access_Mode OUT, 
			@pi_Cleanup_Batch_Size = @i_Cleanup_Batch_Size OUT, 
			@pnv_DataStoreDelimiter = @nv_DataStoreDelimiter OUT, 
			@pi_DataStoreInsertTables = @i_DataStoreInsertTables OUT, 
			@pc_DataStorePresent = @c_DataStorePresent OUT,
			@pc_DataStore_Terminate = @c_DataStore_Terminate OUT, 
			@pi_Insert_Update_Batch_Size = @i_Insert_Update_Batch_Size OUT, 
			@pi_WaitTime= @i_WaitTime OUT, 
			@pc_Keep_Remote_Records = @c_Keep_Remote_Records OUT, 
			@pi_Log_Level = @i_Log_Level OUT,
			@pi_Log_Retention = @i_Log_Retention OUT, 
			@pi_Missing_Txn_Retries = @i_Missing_Txn_Retries OUT, 
			@pc_Stop_If_Retries_Exceeded = @c_Stop_If_Retries_Exceeded OUT, 
			@pc_Stop_On_Duplicate_Insert = @c_Stop_On_Duplicate_Insert OUT, 
			@pc_Stop_On_No_Update = @c_Stop_On_No_Update OUT,
			@pc_Verify_Host = @c_Verify_Host OUT, 
			@pnv_Version = @nv_Version OUT,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Msg = N'Check to see if Datastore is enabled or set to resurrect'
			IF @c_DataStore_Terminate = @C_YES
				SET @nv_Msg = N'*** DATASTORE STOPPED ***' 
			ELSE IF @c_DataStore_Terminate = @C_resurrect
				SET @nv_Msg = N'*** Waiting to be resurrected by csiDataStoreManager job ***' 
			
			IF @c_DataStore_Terminate = @C_YES OR @c_DataStore_Terminate = @C_RESURRECT
			BEGIN
				-- DataStore is not enabled or set to resurrect.  Exit
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT

				SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
				SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] End.'
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT

				SET @nv_Loc = N'csiDataStoreReplicator'
				RETURN
			END			

			SET @nv_Msg = N'DataStore running' 
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
				
			-- Call csiDataStoreVerifyHost
			SET @nv_Msg = N'Call csiDataStoreVerifyHost'
			EXEC csiDataStoreVerifyHost 
				@pc_Verify_host = @c_Verify_Host,
				@pnv_TableName = @pnv_TableName,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
		
		-- Set @si_FetchStatus = -1 so that csiDataStoreGetNextTxnId will define and open the queue table cursor
		SET @si_FetchStatus = -1

		-- >> START CODE WHILE DATASTORE IS RUNNING
		WHILE @c_DataStore_Terminate = @C_NO
		BEGIN -- (0)
			IF @bi_ReplicatorSeq > 1
			BEGIN
				SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
				SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] Begin.  '
				IF @bit_Slept = 1
					SET @nv_Msg = @nv_Msg + 'DATASTORESETUP.INSERT_UPDATE_WAIT_TIME [ ' + CONVERT(NVARCHAR, @i_WaitTime) + N' ] seconds reached.'
				SET @bit_Slept = 0
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT

					SET @nv_Loc = N'csiDataStoreReplicator'
			END

			-- Get next TxnId for this replication session
			IF @si_FetchStatus < 0
			BEGIN
				SET @si_CursorStatus = CURSOR_STATUS('variable', '@cur_QueueTable')
				IF @si_CursorStatus = -1
				BEGIN	
					-- The cursor allocated to this variable is closed.  Deallocate it so it can be reopened by csiDataStoreGetNextTxnId
					DEALLOCATE @cur_QueueTable
				END

				-- Open cursor again and fetch row
				SET @nv_Msg = N'csiDataStoreGetNextTxnId'
				EXEC csiDataStoreGetNextTxnId
					@pnv_TableName = @pnv_TableName,
					@pbi_LastId = @bi_LastId,
					@pc_TxnId = @c_TxnId OUT,
					@pnv_TxnType = @nv_TxnType OUT,
					@pc_Status = @c_Status OUT,
					@pbi_NextId = @bi_NextId OUT,
					@pi_CDOId = @i_CDOId OUT,
					@pnv_Err = @nv_Err OUT,
					@psi_FetchStatus = @si_FetchStatus OUT,
					@pcur_QueueTable = @cur_QueueTable OUT,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
			END
			ELSE
			BEGIN
				-- If @si_FetchStatus = 0 then just fetch again.  Cursor is already open and may still contain data to fetch
				SET @nv_Msg = N'csiDataStoreGetNextTxnId'
				EXEC csiDataStoreGetNextTxnId
					@pnv_TableName = @pnv_TableName,
					@pbi_LastId = @bi_LastId,
					@pc_TxnId = @c_TxnId OUT,
					@pnv_TxnType = @nv_TxnType OUT,
					@pc_Status = @c_Status OUT,
					@pbi_NextId = @bi_NextId OUT,
					@pi_CDOId = @i_CDOId OUT,
					@pnv_Err = @nv_Err OUT,
					@psi_FetchStatus = @si_FetchStatus OUT,
					@pcur_QueueTable = @cur_QueueTable,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
			END

			-- Decrease sensitivity for out of sequence so first 3 detections of an out of sync condition wont be acted upon (i.e. wont be logged or counted against retry count)
			-- 3 iterations sleep for the amount of seconds defined by MISSING_TXN_RETRIES (which defaults to 2), which equates to 6 seconds
			-- Once the missted txn count reaches 0, messages begin logging to the DataStoreLog table 
			SET @i_MissTxnCnt = -3

			-- >> START OUT OF SEQUENCE PROCESSING 
			-- While loop executes if out of sequence
			WHILE ( ( (@bi_LastId > 0  AND @bi_NextId  > 0 AND @bi_NextId >  @bi_LastId +1) OR (@c_Status = @C_STATUS_UNCOMMITTED) ) AND (@i_MissTxnCnt < @i_Missing_Txn_Retries) )
			BEGIN -- (0) WHILE out of sequence
				-- Log message, sleep and retry
				SET @nv_Msg = N'Out of sequence while loop'
				SET @i_MissTxnCnt = @i_MissTxnCnt +1

				SET @si_CursorStatus = CURSOR_STATUS('variable', '@cur_QueueTable')
				IF @si_CursorStatus >= 0
				BEGIN -- (1) IF @si_CursorStatus >= 0
					CLOSE @cur_QueueTable
					DEALLOCATE @cur_QueueTable
					SET @nv_Msg = N'Close queue table cursor'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT

					-- Set @si_FetchStatus = -1 so that csiDataStoreGetNextTxnId will define and open the queue table cursor
					SET @si_FetchStatus = -1
				END -- (1) IF @si_CursorStatus >= 0

				IF (@i_MissTxnCnt = @i_Missing_Txn_Retries)
				BEGIN -- (1a) IF (@i_MissTxnCnt = @i_Missing_Txn_Retries)
					SET @nv_Msg = N'OUT OF SEQUENCE ERROR : missed txn count|max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + N'|' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
					+ N' ] : Transaction Status {@c_Status} [ ' + @c_Status + N' ] Transaction {@c_TxnId} [ ' + @c_TxnId + N' ] Next Transaction ID {@bi_NextId} [ ' + CONVERT(NVARCHAR, @bi_NextId)
					+ N' ] > Last Processed Transaction {@bi_LastId} [ ' + CONVERT(NVARCHAR, @bi_LastId) + N' ].  No Retries left'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT

					IF (@c_Status= @C_STATUS_UNCOMMITTED)
					BEGIN -- (2a) IF (@c_Status= @C_STATUS_UNCOMMITTED)
					
						-- Uncommitted transaction
						SET @nv_Msg = N'csiDataStoreInsertMissedTxns'
						
						EXEC csiDataStoreInsertMissedTxns
							@pnv_TableName = @pnv_TableName,
							@pnv_Type = @C_MISSED_UNCOMMITTED,
							@pbi_StartId = @bi_Nextid,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT
							
						-- Check to stop DataStore.  
						IF (@c_Stop_If_Retries_Exceeded = 'N')
						BEGIN -- (3a) IF (@c_Stop_If_Retries_Exceeded = 'N')
							SET @nv_Msg = N'Uncommitted Retries exceeded, STOP_IF_RETRIES_EXCEEDED = FALSE.  DataStore Continuing.'
							EXEC csiDataStoreLogMessage 
								@pnv_Msg = @nv_Msg, 
								@pnv_Loc = @nv_Loc, 
								@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT

							EXEC csiDataStoreSetLastProcessed
								@pnv_TableName = @pnv_TableName,
								@pnv_TableType = @nv_TableType,
								@pc_TxnId = @c_TxnId,
								@pbi_NextId = @bi_NextId,
								@pnv_TxnType = @nv_TxnType,
								@pc_Status = @c_Status,
								@pc_LastTxnId = @c_LastTxnId OUT,
								@pbi_LastId = @bi_LastId OUT,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT
						END -- (3a) IF (@c_Stop_If_Retries_Exceeded = 'N')
						ELSE
						BEGIN -- (3b) IF (@c_Stop_If_Retries_Exceeded = 'N')
							SET @nv_Msg = N'csiDataStoreEnable ' + CONVERT(NVARCHAR, @BIT_DISABLE)
							EXEC csiDataStoreEnable 
								@pbit_Enable = @BIT_DISABLE, 
								@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT 
						END -- (3b) IF (@c_Stop_If_Retries_Exceeded = 'N')
										
					END -- (2a) IF (@c_Status= @C_STATUS_UNCOMMITTED)
					ELSE	
					BEGIN -- (2b) IF (@c_Status= @C_STATUS_UNCOMMITTED)
					
						-- Missed transaction
						IF (@bi_LastId > 0)
						BEGIN -- (3a) IF (@bi_LastId > 0)
							SET @bi_FirstMissedId= @bi_LastId +1
							SET @bi_LastMissedId = @bi_NextId -1
							SET @nv_Msg = N'csiDataStoreInsertMissedTxns'
							
							EXEC csiDataStoreInsertMissedTxns
								@pnv_TableName = @pnv_TableName,
								@pnv_Type = @C_MISSED_MISSING,
								@pbi_StartId = @bi_FirstMissedId,
								@pbi_LastId = @bi_LastMissedId,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT
		
							-- Check to stop DataStore.  
							IF (@c_Stop_If_Retries_Exceeded = 'N')
							BEGIN -- (4a) IF (@c_Stop_If_Retries_Exceeded = 'N')
								SET @nv_Msg = N'Missing Retries exceeded, STOP_IF_RETRIES_EXCEEDED = FALSE.  Datastore Continuing.'
								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = @nv_Loc, 
									@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT

								EXEC csiDataStoreSetLastProcessed
									@pnv_TableName = @pnv_TableName,
									@pnv_TableType = @nv_TableType,
									@pc_TxnId = @c_TxnId,
									@pbi_NextId = @bi_LastMissedId ,
									@pnv_TxnType = @nv_TxnType,
									@pc_Status = @C_STATUS_ROLLBACK,
									@pc_LastTxnId = @c_LastTxnId OUT,
									@pbi_LastId = @bi_LastId OUT,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT
							END -- (4a) IF (@c_Stop_If_Retries_Exceeded = 'N')
							ELSE
							BEGIN -- (4b) IF (@c_Stop_If_Retries_Exceeded = 'N')
								SET @nv_Msg = N'csiDataStoreEnable ' + CONVERT(NVARCHAR, @BIT_DISABLE)
								EXEC csiDataStoreEnable 
									@pbit_Enable = @BIT_DISABLE, 
									@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT 
							END -- (4b) IF (@c_Stop_If_Retries_Exceeded = 'N')
							
						END -- (3a) IF (@bi_LastId > 0)
						
					END  -- (2b) IF (@c_Status= @C_STATUS_UNCOMMITTED)
					
					WHILE @@TRANCOUNT > 0
						COMMIT TRAN
					
					SET @nv_Msg = N'COMMIT @@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : @i_TransactionCnt [ ' + CONVERT(NVARCHAR, @i_TransactionCnt) 
					+ N' ].   Issued after out of sync condition could not be resolved before MISSING_TXN_RETRIES [ ' 
					+ CONVERT(NVARCHAR, @i_Missing_Txn_Retries) + N' ] reached.'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT

					SET @i_TransactionCnt = 0
									
				END -- (1a) IF (@i_MissTxnCnt = @i_Missing_Txn_Retries)
				ELSE
				BEGIN -- (1b) IF (@i_MissTxnCnt = @i_Missing_Txn_Retries)
					IF @i_MissTxnCnt > 0
					BEGIN -- (2a) IF @i_MissTxnCnt > 0
						SET @nv_Msg = N'OUT OF SEQUENCE WARNING : missed txn count | max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + N' | ' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
						+ N' ] : Transaction Status {@c_Status} [ ' + @c_Status + N' ] Transaction {@c_TxnId} [ ' + @c_TxnId + N' ] Next Transaction ID {@bi_NextId} [ ' + CONVERT(NVARCHAR, @bi_NextId)
						+ N' ] > Last Processed Transaction {@bi_LastId} [ ' + CONVERT(NVARCHAR, @bi_LastId) + N' ]'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

						IF (@c_Status = @C_STATUS_UNCOMMITTED)
						BEGIN -- (3a) IF (@c_Status = @C_STATUS_UNCOMMITTED)
							SET @nv_Msg = N'OUT OF SEQUENCE WARNING : missed txn count|max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + N'|' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
							+ N' ] : Waiting for Transcation {@c_TxnId} [ ' + @c_TxnId + N' ] Next Transaction ID {@bi_NextId} [ ' + CONVERT( NVARCHAR, @bi_NextId) 
							+ N' ] to commit or roll back on transaction database'
							EXEC csiDataStoreLogMessage 
								@pnv_Msg = @nv_Msg, 
								@pnv_Loc = @nv_Loc, 
								@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT
						END -- (3a) IF (@c_Status = @C_STATUS_UNCOMMITTED)
						ELSE
						BEGIN -- (3b) IF (@c_Status = @C_STATUS_UNCOMMITTED)
							SET @nv_Msg = N'OUT OF SEQUENCE WARNING : missed txn count|max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + N'|' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
							+ N' ] : Missing Transaction between Next Transaction ID {@bi_NextId} [ ' + CONVERT(NVARCHAR, @bi_NextId) + N' ] and Last Processed Transaction {@bi_LastId} [ ' 
							+ CONVERT(NVARCHAR,@bi_LastId) + N' ]'
							EXEC csiDataStoreLogMessage 
								@pnv_Msg = @nv_Msg, 
								@pnv_Loc = @nv_Loc, 
								@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT
						END -- (3a) IF (@c_Status = @C_STATUS_UNCOMMITTED)

						SET @nv_Msg = N'OUT OF SEQUENCE WARNING : missed txn count|max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + N'|' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
						+ N' ] : Sleep [ ' + CONVERT(NVARCHAR, @i_WaitTime) + N' ] seconds'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

						SET @nv_Msg = N'OUT OF SEQUENCE WARNING : missed txn count|max missed txn count [ ' + CONVERT(NVARCHAR, @i_MissTxnCnt) + '|' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries)
						+ N' ] : Reopen queue table cursor after sleep to fetch records to process with updated status values'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

					END -- (2a) IF @i_MissTxnCnt > 0

					WAITFOR DELAY @i_WaitTime
					SET @bit_Slept = 1
					
					-- Call csiDataStoreInit
					SET @nv_Msg = N'csiDataStoreInit ' + @pnv_TableName 
					EXEC csiDataStoreInit 
						@pnv_TableName = @pnv_TableName,
						@pnv_TableType = @nv_TableType OUT, 
						@pc_LastTxnId = @c_LastTxnId OUT, 
						@pbi_LastId = @bi_LastId OUT, 
						@pbi_WaitingId = @bi_WaitingId OUT, 
						@pnv_Access_Mode = @nv_Access_Mode OUT, 
						@pi_Cleanup_Batch_Size = @i_Cleanup_Batch_Size OUT, 
						@pnv_DataStoreDelimiter = @nv_DataStoreDelimiter OUT, 
						@pi_DataStoreInsertTables = @i_DataStoreInsertTables OUT, 
						@pc_DataStorePresent = @c_DataStorePresent OUT,
						@pc_DataStore_Terminate = @c_DataStore_Terminate OUT, 
						@pi_Insert_Update_Batch_Size =@i_Insert_Update_Batch_Size OUT, 
						@pi_waitTime = @i_WaitTime OUT, 
						@pc_Keep_Remote_Records = @c_Keep_Remote_Records OUT, 
						@pi_Log_Level = @i_Log_Level OUT,
						@pi_Log_Retention = @i_Log_Retention OUT, 
						@pi_Missing_Txn_Retries = @i_Missing_Txn_Retries OUT, 
						@pc_Stop_If_Retries_Exceeded = @c_Stop_If_Retries_Exceeded OUT, 
						@pc_Stop_On_Duplicate_Insert = @c_Stop_On_Duplicate_Insert OUT, 
						@pc_Stop_On_No_Update = @c_Stop_On_No_Update OUT,
						@pc_Verify_Host = @c_Verify_Host OUT, 
						@pnv_Version = @nv_Version OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pbi_LogSeq = @bi_LogSeq OUT
						

					-- Open cursor again and fetch row
					-- Set @si_FetchStatus = -1 so that csiDataStoreGetNextTxnId will define and open the queue table cursor
					SET @si_FetchStatus = -1
					SET @nv_Msg = N'csiDataStoreGetNextTxnId'
					EXEC csiDataStoreGetNextTxnId
						@pnv_TableName = @pnv_TableName,
						@pbi_LastId = @bi_LastId,
						@pc_TxnId = @c_TxnId OUT,
						@pnv_TxnType = @nv_TxnType OUT,
						@pc_Status = @c_Status OUT,
						@pbi_NextId = @bi_NextId OUT,
						@pi_CDOId = @i_CDOId OUT,
						@pnv_Err = @nv_Err OUT,
						@psi_FetchStatus = @si_FetchStatus OUT,
						@pcur_QueueTable = @cur_QueueTable OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END -- (1b) IF (@i_MissTxnCnt = @i_Missing_Txn_Retries)
			END -- (0) WHILE out of sequence
			-- >> END OUT OF SEQUENCE PROCESSING 

			-- >> START TRANSACTION PROCESSING
			IF (@c_DataStore_Terminate = @C_NO)
			BEGIN -- (1a)
				IF (@c_Status = @C_STATUS_ROLLBACK)
				BEGIN -- (2a)
					-- The Camstar transaction status is rollback 
					-- In this case, SET_LAST_PROCESSED is called to advance the Camstar transaction to indicate we processed the roll back transaction on the ODS
					-- No SQL is actually processed on the ODS.  
					
					SET @nv_Msg = N'Rolled back transaction detected.  Skip processing on ODS for Transaction {@c_TxnId} [ ' + @c_TxnId + N' ] CDOID {@i_CDOID} [ ' + CONVERT(NVARCHAR, @i_CDOID) + N' ]'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT

					SET @nv_Msg = N'csiDataStoreSetLastProcessed'
					EXEC csiDataStoreSetLastProcessed
						@pnv_TableName = @pnv_TableName,
						@pnv_TableType = @nv_TableType,
						@pc_TxnId = @c_TxnId,
						@pbi_NextId = @bi_NextId,
						@pnv_TxnType = @nv_TxnType,
						@pc_Status = @c_Status,
						@pc_LastTxnId = @c_LastTxnId OUT,
						@pbi_LastId = @bi_LastId OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END -- (2a)
				ELSE
				BEGIN -- (2b)
					IF (@c_Status = @C_STATUS_PROCESS)
					-- The Camstar transcation status is process/proceed to be processed by replicator 
					-- If this Camstar transaction is not a dummy transaction, the SQL making up the Camstar transaction is parsed and executed on the ODS
					-- Camstar transactions are committed one at a time if gBatchSize = 0  or as a batch if gBatchSize > 0
					-- SET_LAST_PROCESSED is called to advance the Camstar transaction to indicate we processed if no inserts are pending.  If inserts are pending, go to else section of this if-then-else block
					-- An insert is pending if there is not a row for a Camstar Transaction in DataStoreSync.  If there is a row in DataStoreSync, that means the insert is complete and the update for the
					-- corresponding Camstar Transaction can continue to process
					BEGIN -- (3a)
						BEGIN TRY
							SET @nv_Msg = N'@bit_InsertsPending = csiDataStoreIsLocked @pnv_TableType = ' + @nv_TableType + N'@pc_TxnId = ' + @c_TxnId + N'@pnv_TxnType = ' + @nv_TxnType
							EXEC @bit_InsertsPending = csiDataStoreIsLocked 
								@pnv_TableType = @nv_TableType,
								@pc_TxnId = @c_TxnId, 
								@pnv_TxnType = @nv_TxnType
					
							IF (@bit_InsertsPending = @BIT_FALSE)
							BEGIN -- (4a)
								-- No inserts pending, continue to process
								SET @nv_Msg = N'Function return value [ ' + CONVERT(NVARCHAR, @bit_InsertsPending) + N' (FALSE) ]'
								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = 'csiDataStoreIsLocked', 
									@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT

								IF @@TRANCOUNT = 0 
									BEGIN TRAN

								SET @nv_Msg = N'csiDataStoreParseAndExecute'
								EXEC csiDataStoreParseAndExecute 
									@pnv_TableName = @pnv_TableName,
									@pnv_TableType = @nv_TableType,
									@pc_TxnId = @c_TxnId,
									@pnv_TxnType = @nv_TxnType,
									@pnv_DataStoreDelimiter = @nv_DataStoreDelimiter,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT
			
								-- Advance to next Camstar transaction
								SET @nv_Msg = N'csiDataStoreProcessed'
								EXEC csiDataStoreSetLastProcessed
									@pnv_TableName = @pnv_TableName,
									@pnv_TableType = @nv_TableType,
									@pc_TxnId = @c_TxnId,
									@pbi_NextId = @bi_NextId,
									@pnv_TxnType = @nv_TxnType,
									@pc_Status = @c_Status,
									@pc_LastTxnId = @c_LastTxnId OUT,
									@pbi_LastId = @bi_LastId OUT,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT
									
								DELETE	FROM DataStoreMissingTxns WHERE	SESSIONNAME = @pnv_TableName AND MissedId = @bi_NextId
								SET @i_RowsAffected = @@ROWCOUNT
							
								SET @nv_Msg = N' Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  DELETE FROM DataStoreMissingTxns WHERE SessionName = [ ' 
								+ @pnv_TableName + ' ] AND MissedId = [ ' + CONVERT(NVARCHAR, @bi_NextId) + N' ]'
								EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg, 
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT


								SET @i_TransactionCnt = @i_TransactionCnt +1

								SET @nv_Msg = N'Commit Logic - Each Camstar Transaction'
								BEGIN -- (5a)
								-- Commit work per Camstar Transaction
									IF (@i_TransactionCnt > 0)
									BEGIN -- (6a)
										WHILE @@TRANCOUNT > 0
											COMMIT TRAN

										SET @nv_Msg = 'COMMIT @@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : @i_TransactionCnt [ ' + CONVERT(NVARCHAR,@i_TransactionCnt) + ' ].  Transaction [ ' + @c_TxnId + ' ].'
										EXEC csiDataStoreLogMessage 
											@pnv_Msg = @nv_Msg, 
											@pnv_Loc = @nv_Loc, 
											@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT

										SET @i_TransactionCnt = 0
									END -- (6a)
								END -- (5a)
							END -- (4a)
							ELSE -- IF(@bit_InsertsPending = @BIT_TRUE)
							BEGIN -- (4b)
								-- Inserts are pending.  Do not advance Camstar transcation
								-- Close the queue table cursor so it is reopened next iteration, commit any pending work, a sleep is performed and INIT is called
								-- Inserts pending, continue to process

								WHILE @@TRANCOUNT > 0
									COMMIT TRAN

								SET @nv_Msg = N'Function return value [ ' + CONVERT(NVARCHAR, @bit_InsertsPending) + N' (TRUE) ]'
								SET @nv_Msg = @nv_Msg + '.  Transaction [ ' + @c_TxnId + ' ].'
								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = @nv_Loc, 
									@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT

								SET @nv_Msg = 'COMMIT @@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : @i_TransactionCnt [ ' + CONVERT(NVARCHAR,@i_TransactionCnt) + ' ]'

								IF @i_Insert_Update_Batch_Size = 0
									SET @nv_Msg = @nv_Msg + '.  Transaction [ ' + @c_TxnId + ' ].'

								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = @nv_Loc, 
									@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT

								SET @i_TransactionCnt = 0
							
								SET @si_CursorStatus = CURSOR_STATUS('variable', '@cur_QueueTable')

								IF @si_CursorStatus >= 0
								BEGIN -- (5a)	
									CLOSE @cur_QueueTable
									DEALLOCATE @cur_QueueTable
									SET @nv_Msg = N'Close queue table cursor'
									EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg, 
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT

									-- Set @si_FetchStatus = -1 so that csiDataStoreGetNextTxnId will define and open the queue table cursor
									SET @si_FetchStatus = -1
								END -- (5a)
		
								SET @dt_CurrentDateTime = GETDATE()
				
								UPDATE
								DataStoreSessionTracking
								SET TimeStamp  = @dt_CurrentDateTime
								WHERE SessionName = @pnv_TableName

								SET @i_RowsAffected = @@ROWCOUNT

								SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  UPDATE DataStoreSessionTracking SET TimeStamp = [ ' + CONVERT(NVARCHAR, @dt_CurrentDateTime, 121) 
								+ ' ] WHERE SessionName = [ ' + @pnv_TableName + ' ]'
								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = @nv_Loc, 
									@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT
	
								SET @nv_Msg = 'Sleep [ ' + CONVERT(NVARCHAR,@i_WaitTime) + ' ] seconds'
								EXEC csiDataStoreLogMessage 
									@pnv_Msg = @nv_Msg, 
									@pnv_Loc = @nv_Loc, 
									@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pi_Log_Level = @i_Log_Level,
									@pbi_LogSeq = @bi_LogSeq OUT

									WAITFOR DELAY @i_WaitTime
									SET @bit_Slept = 1

								-- Call csiDataStoreInit
								SET @nv_Msg = N'csiDataStoreInit ' + @pnv_TableName 
								EXEC csiDataStoreInit 
									@pnv_TableName = @pnv_TableName,
									@pnv_TableType = @nv_TableType OUT, 
									@pc_LastTxnId = @c_LastTxnId OUT, 
									@pbi_LastId = @bi_LastId OUT, 
									@pbi_WaitingId = @bi_WaitingId OUT, 
									@pnv_Access_Mode = @nv_Access_Mode OUT, 
									@pi_Cleanup_Batch_Size = @i_Cleanup_Batch_Size OUT, 
									@pnv_DataStoreDelimiter = @nv_DataStoreDelimiter OUT, 
									@pi_DataStoreInsertTables = @i_DataStoreInsertTables OUT, 
									@pc_DataStorePresent = @c_DataStorePresent OUT,
									@pc_DataStore_Terminate = @c_DataStore_Terminate OUT, 
									@pi_Insert_Update_Batch_Size =@i_Insert_Update_Batch_Size OUT, 
									@pi_WaitTime= @i_WaitTime OUT, 
									@pc_Keep_Remote_Records = @c_Keep_Remote_Records OUT, 
									@pi_Log_Level = @i_Log_Level OUT,
									@pi_Log_Retention = @i_Log_Retention OUT, 
									@pi_Missing_Txn_Retries = @i_Missing_Txn_Retries OUT, 
									@pc_Stop_If_Retries_Exceeded = @c_Stop_If_Retries_Exceeded OUT, 
									@pc_Stop_On_Duplicate_Insert = @c_Stop_On_Duplicate_Insert OUT, 
									@pc_Stop_On_No_Update = @c_Stop_On_No_Update OUT,
									@pc_Verify_Host = @c_Verify_Host OUT, 
									@pnv_Version = @nv_Version OUT,
									@pnv_Job = @nv_Job,
									@pnv_PackageExecuting = @nv_PackageExecuting,
									@pbi_LogSeq = @bi_LogSeq OUT
							END -- (4b)
						END TRY
							BEGIN CATCH -- (begin catch block)
								SELECT   
        							@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),   
          							@i_ErrorSeverity = ERROR_SEVERITY(),  
        							@i_ErrorState = ERROR_STATE(),		
									@i_ErrorNumber = ERROR_NUMBER() 

							IF  (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
							BEGIN
								WHILE @@TRANCOUNT > 0
									ROLLBACK TRAN
								END

							-- Log Sequence (@pbi_LogSeq )seems to get lost when returning from a child procedure's catch block to parent catch block via a throw.  
							-- Because @bi_LogSeq does not have its latest value and has the value it had when it called the child procedure, it is possible the next call
							-- to csiDataStoreLogMessage could result in failure due to a duplicate value into the DataStoreLog table.
							-- To fix the possibility of a duplicate row from being added to the DataStoreLog table, thus causing a duplicate key error, 
							-- we will advance the log seq by 10
							SET @bi_LogSeq = @bi_LogSeq + 10

							-- NO ROWS UPDATED ---------------------------------------------------------------------------------------------------------------------------------------------------------------
 							IF @i_ErrorNumber = 50500
							BEGIN -- (begin 50500 NoRowsUpdate)
								-- No rows updated.  This can't be white listed as this error condition is thrown in CsiDataStoreParseAndExecute 
								-- and is not an exception raised by the database engine

								IF ( @c_Stop_On_No_Update = 'Y') 
								BEGIN  -- (begin 50500 NoRowsUpdate STOP_ON_NO_UPDATE = 'Y')
									-- STOP_ON_NO_UPDATE is 'Y'.  Stop the ODS

									SET @nv_Msg = N'Exception Handler (procedure catch block 50500 NoRowsUpdated) : STOP_ON_NO_UPDATE = "Y"'
									EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg,
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT

									-- Stop DataStore
									SET @nv_Msg = N'csiDataStoreEnable ' + CONVERT(NVARCHAR(10), @BIT_DISABLE)
									EXEC csiDataStoreEnable 
										@pbit_Enable = @BIT_DISABLE, 
										@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT

								END -- (end 50500 NoRowsUpdate STOP_ON_NO_UPDATE = 'Y'
								ELSE
								BEGIN -- (begin 50500 NoRowsUpdate STOP_ON_NO_UPDATE = 'N')

									SET @nv_Msg = N'Exception Handler (procedure catch block 50500 NoRowsUpdated) : Row to update not found.  STOP_ON_NO_UPDATE = "N".  Skipping txn.  Txn SQL will be saved in buffer table for review.'
									EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg,
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @I_LOG_LEVEL_WHITELISTED,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT

									-- Log the missed Camstar Transaction and continue processing in while loop until @c_DataStore_Terminate = 'Y'
									SET @nv_type = N'NO UPD : ' + @c_TxnId 
									EXEC csiDataStoreInsertMissedTxns
										@pnv_TableName = @pnv_TableName,
										@pnv_Type = @nv_Type,
										@pbi_StartId = @bi_Nextid,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT
	
									SET @nv_Msg = N'csiDataStoreSetLastProcessed'
									EXEC csiDataStoreSetLastProcessed
										@pnv_TableName = @pnv_TableName,
										@pnv_TableType = @nv_TableType,
										@pc_TxnId = @c_TxnId,
										@pbi_NextId = @bi_NextId,
										@pnv_TxnType = @nv_TxnType,
										@pc_Status = @c_Status,
										@pc_LastTxnId = @c_LastTxnId OUT,
										@pbi_LastId = @bi_LastId OUT,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT
								
								END -- (end 50500 NoRowsUpdate STOP_ON_NO_UPDATE = 'N')
							END -- (end 50500 NoRowsUpdate)
							-- NO ROWS UPDATED ---------------------------------------------------------------------------------------------------------------------------------------------------------------
							ELSE
							BEGIN -- (begin some other exception other than 50500 NoRowsUpdate)

								EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
									@pn_ErrorNumber	 = @i_ErrorNumber

								IF @bit_WhiteListed = @BIT_TRUE
									SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
								ELSE
									SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR

								-- UNIQUE INDEX OR UNIQUE CONSTRAINT VIOLATION -----------------------------------------------------------------------------------------------------------------------------------
								IF @i_ErrorNumber = 2601 OR @i_ErrorNumber = 2627
								BEGIN -- (begin DuplicateKeyError)
									-- DuplicateKeyError

									SET @nv_Msg = N'Exception Handler (procedure catch block DuplicateKeyError) : ' + @nv_ErrorMessage
									EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg,
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT

									IF @bit_WhiteListed = @BIT_TRUE
									-- Duplicate is White Listed
									BEGIN -- (begin DuplicateKeyError is White Listed)

										IF @c_Stop_On_Duplicate_Insert = 'Y'
										BEGIN -- (begin STOP_ON_DUPLICATE_INSERT = 'Y')
											-- DuplicateKeyError is WhiteListed.  WhiteListing overrides the ERROR_NUMBER() exception and the setting for STOP_ON_DUPLICATE_INSERT
								
											SET @nv_Msg = N'Exception Handler (procedure catch block DuplicateKeyError) : White List overrides [ ' + CONVERT(NVARCHAR,@i_ErrorNumber) + N' ].  '
											+ N'White List overrides STOP_ON_DUPLICATE_INSERT = "Y".  '
											+ N'Skipping Camstar Transaction.'
											EXEC csiDataStoreLogMessage 
												@pnv_Msg = @nv_Msg,
												@pnv_Loc = @nv_Loc, 
												@pi_MsgLogLevel = @I_LOG_LEVEL_WHITELISTED,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT

										END -- (end STOP_ON_DUPLICATE_INSERT = 'Y')
										ELSE
										BEGIN -- (begin STOP_ON_DUPLICATE_INSERT = 'N')
											-- DuplicateKeyError is WhiteListed.  WhiteListing overrides the ERROR_NUMBER() exception and concurs with the setting for STOP_ON_DUPLICATE_INSERT
										
											SET @nv_Msg = N'Exception Handler (procedure catch block DuplicateKeyError) : White List overrides [ ' + CONVERT(NVARCHAR,@i_ErrorNumber) + N' ].  '
											+ N'White List concurs with STOP_ON_DUPLICATE_INSERT = "N".  '
											+ N'Skipping CamStar Transaction.'
											EXEC csiDataStoreLogMessage 
												@pnv_Msg = @nv_Msg,
												@pnv_Loc = @nv_Loc, 
												@pi_MsgLogLevel = @I_LOG_LEVEL_WHITELISTED,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT
										END -- (end STOP_ON_DUPLICATE_INSERT = 'Y')
									
										-- Log the missed Camstar Transaction and continue processing in while loop until @c_DataStore_Terminate = 'Y'
										SET @nv_type = N'DUPVAL : ' + @c_TxnId 
										EXEC csiDataStoreInsertMissedTxns
											@pnv_TableName = @pnv_TableName,
											@pnv_Type = @nv_Type,
											@pbi_StartId = @bi_Nextid,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT

										SET @nv_Msg = N'csiDataStoreSetLastProcessed'
										EXEC csiDataStoreSetLastProcessed
											@pnv_TableName = @pnv_TableName,
											@pnv_TableType = @nv_TableType,
											@pc_TxnId = @c_TxnId,
											@pbi_NextId = @bi_NextId,
											@pnv_TxnType = @nv_TxnType,
											@pc_Status = @c_Status,
											@pc_LastTxnId = @c_LastTxnId OUT,
											@pbi_LastId = @bi_LastId OUT,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT
									END -- (end DuplicateKeyError is White Listed)
									ELSE
									BEGIN -- (begin DuplicateKeyError is NOT White Listed)
										-- DuplicateKeyError is NOT WhiteListed

										IF @c_Stop_On_Duplicate_Insert = 'Y'
										BEGIN -- (begin STOP_ON_DUPLICATE_INSERT = 'Y')
											-- STOP_ON_DUPLICATE_INSERT is 'Y' and DuplicateKeyError is not WhiteListed.  Stop the ODS

											SET @nv_Msg = N'Exception Handler (procedure catch block DuplicateKeyError) : STOP_ON_DUPLICATE_INSERT = "Y"'
											EXEC csiDataStoreLogMessage 
												@pnv_Msg = @nv_Msg,
												@pnv_Loc = @nv_Loc, 
												@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT

											-- Stop DataStore
											SET @nv_Msg = N'csiDataStoreEnable ' + CONVERT(NVARCHAR(10), @BIT_DISABLE)
											EXEC csiDataStoreEnable 
												@pbit_Enable = @BIT_DISABLE, 
												@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT
										END -- (end STOP_ON_DUPLICATE_INSERT = 'Y')
										ELSE
										BEGIN -- (begin STOP_ON_DUPLICATE_INSERT = 'N')
											-- DuplicateKeyError is not WhiteListed and STOP_ON_DUPLICATE_INSERT is 'N'

											SET @nv_Msg = N'Exception Handler (procedure catch block DuplicateKeyError) : STOP_ON_DUPLICATE_INSERT = "N".  '
											+ N'Skipping Camstar Transaction.  Txn SQL will be saved in buffer table for review.'													
											EXEC csiDataStoreLogMessage 
												@pnv_Msg = @nv_Msg,
												@pnv_Loc = @nv_Loc, 
												@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT

											-- Log the missed Camstar Transaction and continue processing in while loop until @c_DataStore_Terminate = 'Y'
											SET @nv_type = N'DUPVAL : ' + @c_TxnId 
											EXEC csiDataStoreInsertMissedTxns
												@pnv_TableName = @pnv_TableName,
												@pnv_Type = @nv_Type,
												@pbi_StartId = @bi_Nextid,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT

											SET @nv_Msg = N'csiDataStoreSetLastProcessed'
												EXEC csiDataStoreSetLastProcessed
												@pnv_TableName = @pnv_TableName,
												@pnv_TableType = @nv_TableType,
												@pc_TxnId = @c_TxnId,
												@pbi_NextId = @bi_NextId,
												@pnv_TxnType = @nv_TxnType,
												@pc_Status = @c_Status,
												@pc_LastTxnId = @c_LastTxnId OUT,
												@pbi_LastId = @bi_LastId OUT,
												@pnv_Job = @nv_Job,
												@pnv_PackageExecuting = @nv_PackageExecuting,
												@pi_Log_Level = @i_Log_Level,
												@pbi_LogSeq = @bi_LogSeq OUT

										END -- (end STOP_ON_DUPLICATE_INSERT = 'N')
									END -- (end DuplicateKeyError is NOT White Listed)
								END -- (end DupliateKeyError)
								-- UNIQUE INDEX OR UNIQUE CONSTRAINT VIOLATION -----------------------------------------------------------------------------------------------------------------------------------
								ELSE
								-- OTHER EXCEPTIONS ---------------------------------------------------------------------------------------------------------------------------------------------------------------
								BEGIN -- (begin other)
									-- Other error.  

									SET @nv_Msg = N'Exception Handler (procedure catch block others) : ' + @nv_ErrorMessage
									EXEC csiDataStoreLogMessage 
										@pnv_Msg = @nv_Msg,
										@pnv_Loc = @nv_Loc, 
										@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
										@pnv_Job = @nv_Job,
										@pnv_PackageExecuting = @nv_PackageExecuting,
										@pi_Log_Level = @i_Log_Level,
										@pbi_LogSeq = @bi_LogSeq OUT


									IF @bit_WhiteListed = @BIT_TRUE
									BEGIN -- (begin other ERROR_NUMBER() is White Listed)
										-- ERROR_NUMBER() is White Listed.  WhiteListing overides violation

										SET @nv_Msg = N'Exception Handler (procedure catch block others) : White List overrides [ ' + CONVERT(NVARCHAR,@i_ErrorNumber) + N' ].  '
										+ N'Skipping Camstar Transaction.'
										EXEC csiDataStoreLogMessage 
											@pnv_Msg = @nv_Msg,
											@pnv_Loc = @nv_Loc, 
											@pi_MsgLogLevel = @I_LOG_LEVEL_WHITELISTED,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT

										-- Log the missed Camstar Transaction and continue processing in while loop until @c_DataStore_Terminate = 'Y'
										SET @nv_type = N'ERROR : ' + @c_TxnId 
										EXEC csiDataStoreInsertMissedTxns
											@pnv_TableName = @pnv_TableName,
											@pnv_Type = @nv_Type,
											@pbi_StartId = @bi_Nextid,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT
				
										SET @nv_Msg = N'csiDataStoreSetLastProcessed'
										EXEC csiDataStoreSetLastProcessed
											@pnv_TableName = @pnv_TableName,
											@pnv_TableType = @nv_TableType,
											@pc_TxnId = @c_TxnId,
											@pbi_NextId = @bi_NextId,
											@pnv_TxnType = @nv_TxnType,
											@pc_Status = @c_Status,
											@pc_LastTxnId = @c_LastTxnId OUT,
											@pbi_LastId = @bi_LastId OUT,
											@pnv_Job = @nv_Job,
											@pnv_PackageExecuting = @nv_PackageExecuting,
											@pi_Log_Level = @i_Log_Level,
											@pbi_LogSeq = @bi_LogSeq OUT

									END -- (end other ERROR_NUMBER() is White Listed)
									ELSE
									BEGIN -- (begin other ERROR_NUMBER() is NOT White Listed)
										-- ERROR_NUMBER() is NOT White Listed.  Stop the ODS or set the ODS to resurrect in outer catch block

										THROW;

									END -- (end other ERROR_NUMBER() is NOT White Listed)
								END -- (end other)
								-- OTHER EXCEPTIONS ---------------------------------------------------------------------------------------------------------------------------------------------------------------
							END -- (end some other exception other than 50500 NoRowsUpdate)
						END CATCH -- (end catch block)
					END -- (3a)
					ELSE
					BEGIN -- (3b)
						-- The Camstar Transaction must be NULL (@pc_TxnId is NULL) 
						-- Commit any pending work, a sleep is performed and INIT is called
						WHILE @@TRANCOUNT > 0
							COMMIT TRAN
	
						SET @nv_Msg = 'COMMIT @@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : @i_TransactionCnt [ ' + CONVERT(NVARCHAR,@i_TransactionCnt) + ' ]'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

						SET @i_TransactionCnt = 0

						SET @dt_CurrentDateTime = GETDATE()

						UPDATE
						DataStoreSessionTracking
						SET Timestamp = @dt_CurrentDateTime
						WHERE SessionName = @pnv_TableName

						SET @i_RowsAffected = @@ROWCOUNT
	
						SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ]  UPDATE DataStoreSessionTracking SET TimeStamp = [ ' + CONVERT(NVARCHAR, @dt_CurrentDateTime, 121) 
						+ ' ] WHERE SessionName = [ ' + @pnv_TableName + ' ]'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

						SET @nv_Msg = 'Sleep [ ' + CONVERT(NVARCHAR,@i_WaitTime) + ' ] seconds'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT
	
						WAITFOR DELAY @i_WaitTime
						SET @bit_Slept = 1

						-- Call csiDataStoreInit
						SET @nv_Msg = N'csiDataStoreInit ' + @pnv_TableName 
						EXEC csiDataStoreInit 
							@pnv_TableName = @pnv_TableName,
							@pnv_TableType = @nv_TableType OUT, 
							@pc_LastTxnId = @c_LastTxnId OUT, 
							@pbi_LastId = @bi_LastId OUT, 
							@pbi_WaitingId = @bi_WaitingId OUT, 
							@pnv_Access_Mode = @nv_Access_Mode OUT, 
							@pi_Cleanup_Batch_Size = @i_Cleanup_Batch_Size OUT, 
							@pnv_DataStoreDelimiter = @nv_DataStoreDelimiter OUT, 
							@pi_DataStoreInsertTables = @i_DataStoreInsertTables OUT, 
							@pc_DataStorePresent = @c_DataStorePresent OUT,
							@pc_DataStore_Terminate = @c_DataStore_Terminate OUT, 
							@pi_Insert_Update_Batch_Size =@i_Insert_Update_Batch_Size OUT, 
							@pi_WaitTime= @i_WaitTime OUT, 
							@pc_Keep_Remote_Records = @c_Keep_Remote_Records OUT, 
							@pi_Log_Level = @i_Log_Level OUT,
							@pi_Log_Retention = @i_Log_Retention OUT, 
							@pi_Missing_Txn_Retries = @i_Missing_Txn_Retries OUT, 
							@pc_Stop_If_Retries_Exceeded = @c_Stop_If_Retries_Exceeded OUT, 
							@pc_Stop_On_Duplicate_Insert = @c_Stop_On_Duplicate_Insert OUT, 
							@pc_Stop_On_No_Update = @c_Stop_On_No_Update OUT,
							@pc_Verify_Host = @c_Verify_Host OUT, 
							@pnv_Version = @nv_Version OUT,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pbi_LogSeq = @bi_LogSeq OUT
					END -- (3b)
				END -- (2b)
			END -- (1a) IF (@c_DataStore_Terminate = 'N')
			-- >> END TRANSACTION PROCESSING

			SET @nv_Msg = N'CHECK_TERMINATE'
			EXEC @c_DataStore_Terminate = csiDataStoreCheckTerminate
		
			IF @pbit_Debug = 1
				-- In debug mode, only run REPLICATOR loop once, then exit.
				SET @c_DataStore_Terminate = 'Y'

			IF @c_DataStore_Terminate = @C_NO
				SET @bi_ReplicatorSeq = @bi_ReplicatorSeq +1

		END -- (0) WHILE DataStore_TERMINATE = 'N'
		-- >> END CODE WHILE DATASTORE IS RUNNING

		-- DataStore has stopped.  Commit all successful transactions
		WHILE @@TRANCOUNT > 0
			COMMIT TRAN

		SET @nv_Msg = 'COMMIT @@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : @i_TransactionCnt [ ' + CONVERT(NVARCHAR,@i_TransactionCnt) + ' ]'
				EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

		SET @i_TransactionCnt = 0

		SET @si_CursorStatus = CURSOR_STATUS('variable', '@cur_QueueTable')
		IF @si_CursorStatus >= 0
		BEGIN	
			CLOSE @cur_QueueTable
			DEALLOCATE @cur_QueueTable
			SET @nv_Msg = N'Close queue table cursor'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			-- Set @si_FetchStatus = -1 so that csiDataStoreGetNextTxnId will define and open the queue table cursor
			SET @si_FetchStatus = -1
		END
		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] End.'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Loc = N'csiDataStoreReplicator'
	END TRY
	BEGIN CATCH
		-- Handle exceptions that might occur in REPLICATOR that occur in the code outside of the begin/end block local exception handler defined in the section of code
		-- "ELSIF (v_Status = cSTATUSPROCESS) THEN".  In other words, this procedure exception handler, handles exceptions that are not related to the call to PARSE_AND_EXECUTE_TXN
		SELECT   
        @nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),    
        @i_ErrorSeverity = ERROR_SEVERITY(),  
        @i_ErrorState = ERROR_STATE(),		
		@i_ErrorNumber = ERROR_NUMBER()
		
		SET @nv_JobName = N'csiDataStoreReplicator(' + '''' + @pnv_TableName + '''' + N')' 
		SET @nv_Loc = N'csiDataStoreReplicator'
		SET @nv_PackageExecuting = @nv_JobName
		
		-- Log Sequence (@pbi_LogSeq )seems to get lost when returning from a child procedure's catch block to parent catch block via a throw.  
		-- Because @bi_LogSeq does not have its latest value and has the value it had when it called the child procedure, it is possible the next call
		-- to csiDataStoreLogMessage could result in failure due to a duplicate value into the DataStoreLog table.
		-- To fix the possibility of a duplicate row from being added to the DataStoreLog table, thus causing a duplicate key error, 
		-- we will advance the log seq by 10
		SET @bi_LogSeq = @bi_LogSeq + 50
		
		IF (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
		BEGIN
			WHILE @@TRANCOUNT > 0
				ROLLBACK TRAN
		END
		
		-- Check to see if connection to the OLTP has been lost by selecting across synoyn that uses linked server that connects the ODS to OLTP
		BEGIN TRY
			-- EXEC is used because if the synonym is missing we want the catch block to get control.  If we did not use EXEC to execute the select
			-- and the synonym OLTP_DBDataSourceNames did not exist, the catch block would not get control.  Object does not exist error does not
			-- get trapped in the catch block unless the select is called by EXEC
			EXEC('SELECT COUNT(*) FROM [OLTP_DBDataSourceNames]');
			
			SET @bi_ConnectionLost = 0
			EXEC @c_DataStore_Terminate = csiDataStoreCheckTerminate
		END TRY
		BEGIN CATCH
			SET @bi_ConnectionLost = 1
		END CATCH
		
	
		BEGIN
			EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed
				@pn_ErrorNumber	 = @i_ErrorNumber

			IF @bit_WhiteListed = @BIT_TRUE
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED
			ELSE
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_ERROR
				
			SET @nv_Err = N'Exception Handler (procedure catch block) : Last message set : ' + @nv_Msg 
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Err, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Err = N'Exception Handler (procedure catch block) : Exception : ' + @nv_ErrorMessage  
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Err, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @i_DynamicMsgLogLevel,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
				
			SET @nv_Msg = N'Exception Handler (procedure catch block) : Exception : ROLLBACK'  
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			IF @bit_WhiteListed = @BIT_FALSE 
			BEGIN
				-- Not White listed
				IF @bi_ConnectionLost = 1 OR @c_DataStore_Terminate = @C_RESURRECT
				BEGIN
					SET @nv_Err = N'Resurrect : ' + @nv_PackageExecuting + N' : ' + @nv_ErrorMessage
					EXEC csiDataStoreLogError
						@pnv_TableName = @pnv_TableName,
						@pc_TxnId = N'N/A',
						@pnv_SQLStmt = N'N/A',
						@pnv_Err = @nv_Err,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_DynamicMsgLogLevel,
						@pbi_LogSeq = @bi_LogSeq OUT
						
					-- Set DataStore to resurrect
					SET @nv_Msg = N'csiDataStoreEnable'
					EXEC csiDataStoreEnable 
						@pbit_Enable = @BIT_RESURRECT, 
						@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
											
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] End.'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END
				ELSE
				BEGIN
					-- Connection not lost, log error and stop DataStore
					SET @nv_Err = @nv_PackageExecuting + N' : ' + @nv_ErrorMessage
					EXEC csiDataStoreLogError
						@pnv_TableName = @pnv_TableName,
						@pc_TxnId = N'N/A',
						@pnv_SQLStmt = N'N/A',
						@pnv_Err = @nv_Err,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_DynamicMsgLogLevel,
						@pbi_LogSeq = @bi_LogSeq OUT
				
					-- Stop DataStore
					SET @nv_Msg = N'csiDataStoreEnable'
					EXEC csiDataStoreEnable 
						@pbit_Enable = @BIT_DISABLE, 
						@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
						
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ReplicatorSeq) + N' ] csiDataStoreReplicator'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ReplicatorSeq) + N' ] End.'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END
			END
			ELSE
			BEGIN
				-- White listed
				SET @nv_Err = N'WhiteListed : ' + @nv_PackageExecuting + N' : ' + @nv_ErrorMessage
				EXEC csiDataStoreLogError
					@pnv_TableName = @pnv_TableName,
					@pc_TxnId = N'N/A',
					@pnv_SQLStmt = N'N/A',
					@pnv_Err = @nv_Err,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_DynamicMsgLogLevel,
					@pbi_LogSeq = @bi_LogSeq OUT
			END
		END;
		THROW
	END CATCH
END
GO
