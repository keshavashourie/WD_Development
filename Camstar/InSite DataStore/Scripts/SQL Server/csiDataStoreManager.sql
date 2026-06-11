------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreManager.sql
-- DESCR:       Stored procedure to manager jobs.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreManager' AND Type = 'P')
	DROP PROCEDURE csiDataStoreManager
GO

CREATE PROCEDURE csiDataStoreManager
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreManager
-- Params:      None
-- Descr:       Stored procedure to manager jobs.  Executes on the ODS	
--		

-- HISTORY:
--              06/24/2016      Dan Maloney      New stored procedure
--              03/03/2017      Alex Lind            move and address logic of validation of indexes and columns from replicator
--              03/03/2017      Alex Lind            fix job name to database rather than user name, set initial log level to error 
--              03/23/2017      Alex Lind            remove validation of LOB columns
--              07/26/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/26/2017      Dan Maloney      Added @b_WhiteListed and @i_DynamicMsgLogLevel variables (US 51393)
--              07/26/2017      Dan Malney        Added THROW to catch block (US 51393)
--              07/26/2017      Dan Malney        Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/26/2017      Dan Maloney      Replaced RAISERROR with THROW statement
--              07/27/2017      Dan Maloney      Removed unused variables @BIT_ENABLE, @nv_Sql (US 51393)
--              06/12/2018      Dan Maloney      Modifications for Verify_Host for US 4122 
--              02/07/2024      Dan Maloney      CPR 392739:TAC 10926362 ODS Deadlock.  Add local try...catch block around code that checks for unique indexes on tables set to propagate.
--                                                             The local catch block detects a 1205 (deadlock victim error) and logs a message to the DataStoreLog table and exists csiDataStoreManager
--                                                             and allows the ODS to continue to run and not terminate
--                                                             Also added WITH (NOLOCK) hints to all tables involved in the query that checks for unique indexes on tables set to propagate.  These added NOLOCK
--                                                             hints on the query should prevent a deadlock victim from occuring and the added local catch block should never be triggered.  The catch block is there
--                                                             as a safety net if the NOLOCK hints on the query failed to preven the deadlock victim (1205) error from being raised.
--
--
-- Copyright Siemens 2023, 2024
-----------------------------------------------------------------------------------------------------------------------------------------------------
AS
-- Variables defined as constants
DECLARE @BIT_RESURRECT                  BIT = NULL
DECLARE @BIT_DISABLE                    BIT = 0
DECLARE @BIT_ENABLE                     BIT = 1
DECLARE @BIT_FALSE			            BIT = 0
DECLARE @BIT_TRUE                       BIT = 1
DECLARE @I_LOG_LEVEL_MAX                INT = 2
DECLARE @I_LOG_LEVEL_MIN                INT = 1
DECLARE @I_LOG_LEVEL_ERROR              INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED        INT = -1
DECLARE @C_NO                           CHAR(1) = 'N'
DECLARE @C_YES                          CHAR(1) = 'Y'
DECLARE @C_RESURRECT                    CHAR(1) = 'R'
DECLARE @bit_VerifyHost                 BIT
DECLARE @bit_WhiteListed                BIT = 0
DECLARE @i_DynamicMsgLogLevel           INT
DECLARE @i_ErrorNumber                  INT 
DECLARE @i_ErrorSeverity                INT  
DECLARE @i_ErrorState                   INT
DECLARE @i                              INT
DECLARE @bi_ConnectionLost				INT
DECLARE @bi_LogSeq                      BIGINT
DECLARE	@bi_ManagerSeq                  BIGINT
DECLARE @bi_RowsAffected                BIGINT
DECLARE	@i_RowCount                     INT = 0
DECLARE @d_DelBeforeDate                DATE
DECLARE @nv_Job	                        NVARCHAR(50)
DECLARE @nv_JobName                     NVARCHAR(100) 
DECLARE @nv_LinkedServer                NVARCHAR(128)
DECLARE @nv_Loc                         NVARCHAR(64) 
DECLARE	@nv_Err                         NVARCHAR(MAX) 
DECLARE @nv_ErrorMessage                NVARCHAR(4000)
DECLARE	@nv_Msg                    		NVARCHAR(MAX) 
DECLARE @nv_OLTP                        NVARCHAR(128)
DECLARE @nv_PackageExecuting            NVARCHAR(128)
DECLARE @nv_TableName                   NVARCHAR(50)
DECLARE @nv_Sql                         NVARCHAR(4000)
DECLARE @i_cnt                          INT 
DECLARE @i_RetryAttempts				INT
	
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
	BEGIN TRY
		SET @nv_JobName = N'csiDataStoreManager' 
		SET @nv_Loc = N'csiDataStoreManager'
		
		SET @nv_PackageExecuting = @nv_JobName
		SET @bi_LogSeq = 1
		SELECT @i_Log_Level = CONVERT(INT,VALUE) 
		FROM DataStoreSetUp WITH(NOLOCK)
		WHERE Parameter = 'LOG_LEVEL'

		-- Get Job ID for logging
   		SELECT 
		@nv_Job = j.Job_Id
		FROM msdb..sysjobs_view j WITH(NOLOCK)
		WHERE j.Name = @nv_JobName + N' (' + DB_NAME() + N')'

		IF @@ROWCOUNT = 0
		BEGIN
			SET @nv_Msg = N'Job : ' + @nv_JobName + N' doesnt exist for database ' + DB_NAME()
			SET @nv_Job= N'Unknown Job';
			THROW 50200, @nv_Msg, 1
		END

		SET @nv_Msg = N'Update job information in procedure variable'
		SET @nv_Loc = N'[ 0 ] csiDataStoreManager Initialization'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @bi_ManagerSeq = 1
		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] Begin'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Loc = N'csiDataStoreManager'
		SET @nv_TableName = N'csiDataStoreManager'

		-- Call csiDataStoreInit
		SET @nv_Msg = N'csiDataStoreInit ' + @nv_TableName 
		EXEC csiDataStoreInit 
			@pnv_TableName = @nv_TableName,
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
			
		--Get ODS to OLTP linked server name
		SELECT @nv_LinkedServer = SUBSTRING(Base_Object_Name, 1, CHARINDEX(']', Base_Object_Name))  FROM sys.synonyms WHERE Name = 'OLTP_DBDataSourceNames';
		--Get OLTP host that linked server is connecting to
		BEGIN TRY
			SET @nv_SQL = N'SELECT @nv_OLTP = OLTP FROM OPENQUERY('+ @nv_LinkedServer + N',''' + N'SELECT @@SERVERNAME as OLTP'')'
			EXEC SP_EXECUTESQL @Query =  @nv_Sql,  @Parms = N'@nv_OLTP NVARCHAR(80) OUTPUT',  @nv_OLTP =  @nv_OLTP OUTPUT

			SET @nv_Msg = N'ODS to OLTP linked server is ' + @nv_LinkedServer + N' and connects to host [ ' +  @nv_OLTP + N' ]'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,			
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
		END TRY
		BEGIN CATCH
			SET @nv_Msg = N'ODS to OLTP linked server is ' + @nv_LinkedServer + N' OLTP is unreachable'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,			
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
		
		END CATCH


		SET @nv_Msg = N'Check to see if Datastore is enabled'
		IF @c_DataStore_Terminate = @C_YES
		BEGIN
			-- DataStore is not enabled.  Exit
			SET @nv_Msg = N'*** DATASTORE STOPPED ***' 
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,			
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
			SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] End.'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'csiDataStoreManager'
			RETURN
		END
		ELSE IF @c_DataStore_Terminate = @C_RESURRECT
		BEGIN
			SET @i_RetryAttempts = 1
			SET @bi_ConnectionLost = 1
			WHILE @bi_ConnectionLost = 1 AND @i_RetryAttempts <= @i_Missing_Txn_Retries
			BEGIN
				-- Keep checking to see if connection has been restored
				BEGIN TRY
					-- Check to see if connection to the OLTP has been lost by selecting across synonym that uses linked server that connects the ODS to OLTP
					SET @nv_Msg = N'*** Retry connection count | max retry connection count [ ' + CONVERT(VARCHAR(2),@i_RetryAttempts) 
					+ N' | ' + CONVERT(NVARCHAR, @i_Missing_Txn_Retries) + N' ] to OLTP through linked server ***' 
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,			
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
					
					-- EXEC is used because if the synonym is missing we want the catch block to get control.  If we did not use EXEC to execute the select
					-- and the synonym OLTP_DBDataSourceNames did not exist, the catch block would not get control.  Object does not exist error does not
					-- get trapped in the catch block unless the select is called by EXEC
					EXEC('SELECT COUNT(*) FROM [OLTP_DBDataSourceNames]');
					
					-- Connection has been established
					SET @bi_ConnectionLost = 0
					-- DataStore is set to resurrect.  Start the ODS
					SET @nv_Msg = N'csiDataStoreEnable'
					EXEC csiDataStoreEnable 
						@pbit_Enable = @BIT_ENABLE, 
						@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
						
					SET @nv_SQL = N'SELECT @nv_OLTP = OLTP FROM OPENQUERY('+ @nv_LinkedServer + N',''' + N'SELECT @@SERVERNAME as OLTP'')'
					EXEC SP_EXECUTESQL @Query =  @nv_Sql,  @Parms = N'@nv_OLTP NVARCHAR(80) OUTPUT',  @nv_OLTP =  @nv_OLTP OUTPUT

					SET @nv_Msg = N'ODS to OLTP linked server is ' + @nv_LinkedServer + N' and connects to host [ ' +  @nv_OLTP + N' ]'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,			
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
								
					SET @nv_Msg = N'*** DATASTORE RESURRECTED ***' 
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,			
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
						
					-- Call csiDataStoreInit
					SET @nv_Msg = N'csiDataStoreInit ' + @nv_TableName 
					EXEC csiDataStoreInit 
						@pnv_TableName = @nv_TableName,
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
				END TRY
				BEGIN CATCH
					-- Connection to OLTP failed 
					WAITFOR DELAY '00:00:05'
					SET @i_RetryAttempts = @i_RetryAttempts +1 
				END CATCH
			END -- WHILE @bi_ConnectionLost = 1 
		END
		
		IF @i_RetryAttempts > @i_Missing_Txn_Retries
		BEGIN
			--Connection not established in max retry attempts
			-- Stop DataStore
			SET @nv_Msg = N'csiDataStoreEnable'
			EXEC csiDataStoreEnable 
				@pbit_Enable = @BIT_DISABLE, 
				@pc_DataStore_Terminate = @c_DataStore_Terminate OUT,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT
						
			SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
			SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] End.'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'csiDataStoreManager'
			RETURN
		END;
		

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
			@pnv_TableName = @nv_TableName,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		IF @c_DataStore_Terminate = @C_NO
		BEGIN  
			BEGIN TRY
				-- Verify no unique indexes on propagated tables
				SELECT @i_cnt = COUNT(1)   
					FROM sys.Indexes i WITH (NOLOCK)  
						join sys.objects o  WITH (NOLOCK)  on  o.object_id = i.object_id 
						join DBTableDefinition dt  WITH (NOLOCK)  on o.name= dt.DBTableName   
					where i.is_primary_key= 0  
						and i.is_unique = 1 
						and i.type <> 0 
						and propagate = 1  
				IF (@i_cnt > 0)  
				BEGIN  
					SET @nv_Msg = N'Unexpected unique indexes found on ODS.  Cannot continue.';   -- JAL
					THROW 50201, @nv_Msg, 1
				END 
			END TRY
			BEGIN CATCH
				SELECT   
				@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
				@i_ErrorSeverity = ERROR_SEVERITY(),  
				@i_ErrorState = ERROR_STATE(),
				@i_ErrorNumber = ERROR_NUMBER()
				
				IF @i_ErrorNumber = 1205 
				BEGIN
					-- A Deadlock occurred that resulted in the select statement that checks for non-primary key, unique indexes  on tables set to propagate exist  in the ODS. 
					-- csiDataStoreManager was selected as the deadlock victim  Error Number 1205 is a deadlock victim.  In the case of a deadlock victim, this catch block will log the deadlock and exit
					-- and the ODS will not be stopped.  The next execution of csiDataStoreManager (1 minute later as determined the ODS Manager Job schedule) will execute the select statement 
					-- that checks for non-primary key, unique indexes on tables set to propagate exist  in the ODS and if the deadlock no longer exists, the csiDataStoreManager code will continue, 
					-- otherwise, this catch block will execute again.  This will repeat until the deadlock situation resolves.
					
					SET @nv_JobName = N'csiDataStoreManager' 
					SET @nv_Loc = N'csiDataStoreManager'
					SET @nv_PackageExecuting = @nv_JobName
			

					SET @nv_Msg = N'csiDataStoreManager has been selected as a deadlock victim while trying to verify no unique indexes exist on tables set to propagate.  This deadlock victim error will be ignored and the ODS will continue processing.'
					EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
						
					IF (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
					BEGIN
						WHILE @@TRANCOUNT > 0
							ROLLBACK TRAN
					END
				 
					RETURN
				END
				ELSE
					-- Some other error ocurred other than a deadlock victim error (1205) and this error will be raised to the outer catch block to be processed.
					THROW
			END CATCH
			
			-- Check jobs
			SET @nv_Msg = N'Verify csiDataStoreReplicator job for DATASTOREUPDATES' 
			SET @nv_TableName = N'DATASTOREUPDATES'
			EXEC csiDataStoreVerifyJob 
				@pnv_ProcName = 'csiDataStoreReplicator',
				@pnv_TableName = @nv_TableName,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @i = 1
			WHILE @i <= @i_DataStoreInsertTables 
			BEGIN		
				SET @nv_Msg = N'Verify csiDataStoreReplicator REPLICATOR job for DATASTOREINSERTS' + CONVERT(NVARCHAR,@i)
				SET @nv_TableName = N'DATASTOREINSERTS' + CONVERT(NVARCHAR,@i)
				EXEC csiDataStoreVerifyJob 
					@pnv_ProcName = N'csiDataStoreReplicator',
					@pnv_TableName = @nv_TableName,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
				SET @i = @i + 1
			END

			SET @nv_Msg = N'Verify csiDataStoreCleanup Job' 
		   	EXEC csiDataStoreVerifyJob 
				@pnv_ProcName = N'csiDataStoreCleanup',
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			-- DataStoreLog management
			IF @i_Log_Retention != 0
			BEGIN
				SET @d_DelBeforeDate = GETDATE() - @i_Log_Retention
				SET @bi_RowsAffected = 0

				DELETE 
				FROM DataStoreLog 
				WHERE Log_TimeStamp < @d_DelBeforeDate

				SET @bi_RowsAffected = @@ROWCOUNT


				SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @bi_RowsAffected) + N' ] DELETE FROM DATASTORELOG.  DELETE FROM DataStoreLog WHERE Log_TimeStamp < [ ' + CONVERT(NVARCHAR,@d_DelBeforeDate, 121) + N' ].'
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 		
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
			
				IF @bi_RowsAffected > 0
				BEGIN
					SET @nv_Msg = N'COMMIT (@@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : Issued after DELETE FROM DataStoreLog WHERE Log_TimeStamp < [ ' + CONVERT(NVARCHAR,@d_DelBeforeDate, 121) + N' ].'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END

				SET @bi_RowsAffected = 0

				DELETE 
				FROM DataStoreErrors 
				WHERE LogDate < @d_DelBeforeDate

				SET @bi_RowsAffected = @@ROWCOUNT

				SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @bi_RowsAffected) + N' ] DELETE FROM DATASTOREERRORS.  DELETE FROM DataStoreErrors WHERE LogDate < [ ' + CONVERT(NVARCHAR,@d_DelBeforeDate, 121) + N' ].'
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT

				IF @bi_RowsAffected > 0
				BEGIN
					SET @nv_Msg = N'COMMIT (@@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] :  Issued after DELETE FROM DataStoreErrors WHERE LogDate < [ ' + CONVERT(NVARCHAR,@d_DelBeforeDate, 121) + N' ].'
					EXEC csiDataStoreLogMessage 
						@pnv_Msg = @nv_Msg, 
						@pnv_Loc = @nv_Loc, 
						@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
						@pnv_Job = @nv_Job,
						@pnv_PackageExecuting = @nv_PackageExecuting,
						@pi_Log_Level = @i_Log_Level,
						@pbi_LogSeq = @bi_LogSeq OUT
				END
			END
		END


		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] End.'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Loc = N'csiDataStoreManager'
	END TRY
	BEGIN CATCH
		SELECT   
		@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
		@i_ErrorSeverity = ERROR_SEVERITY(),  
		@i_ErrorState = ERROR_STATE(),
		@i_ErrorNumber = ERROR_NUMBER()
		
		SET @nv_JobName = N'csiDataStoreManager' 
		SET @nv_Loc = N'csiDataStoreManager'
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
						@pnv_TableName = @nv_TableName,
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
											
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] End.'
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
						@pnv_TableName = @nv_TableName,
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
						
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_ManagerSeq) + N' ] csiDataStoreManager'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_ManagerSeq) + N' ] End.'
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
					@pnv_TableName = @nv_TableName,
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
