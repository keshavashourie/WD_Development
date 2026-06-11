------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreCleanup.sql
-- DESCR:       Deletes processed records.  Executes on the ODS
-- Copyright Siemens 2024 
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreCleanup' AND Type = 'P')
	DROP PROCEDURE csiDataStoreCleanup
GO

CREATE PROCEDURE csiDataStoreCleanup
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreCleanup
-- Params:      None
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
--              06/28/2016      Dan Maloney      Restructured Code
--              03/03/2017      Alex Lind        Update job name to db name not user name
--              05/12/2017      Alex Lind        Modify Catch block to move select statement for error above rollback call 
--              07/14/2017      Dan Maloney      Moved code to get LOG_LEVEL before check to see if csiDataStoreCleanup Job is running.  (US 51393)
--              07/14/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/14/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/14/2017      Dan Maloney      Added THROW to catch block (US 51393)
--              07/14/2017      Dan Maloney      Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/24/2017      Dan Maloney      Replaced RAISERROR with THROW statement
--              07/28/2017      Dan Maloney      Removed unused variables @C_NO, @C_YES (US 51393)
--              06/12/2018      Dan Maloney      Modifications for Verify_Host for US 4122 
--              10/24/2019      Dan Maloney      Modified csiDataStoreCleanup procedure to change @cur_Tracking cursor to select only rows from DataStoreSessionTracking that have a matching
--                                               synonym name for the SessionName (CPR 1260)
--              09/18/2024      Jim Saunders     Change SELECTs against queue tables to use NOT EXISTS and create IN list for DELETE from cursor of IDs to eliminate join
--                                               Only use NOT EXISTS if there are corresponding rows in the DataStoreMissingTxns table, handle no rows to cleanup case
--
-- Copyright Siemens 2024  
-----------------------------------------------------------------------------------------------------------------------------------------------------
AS
-- Variables defined as constants
DECLARE @BIT_RESURRECT                  BIT = NULL
DECLARE @BIT_DISABLE                    BIT = 0
DECLARE @BIT_FALSE                      BIT = 0
DECLARE @BIT_TRUE                       BIT = 1
DECLARE @I_LOG_LEVEL_MAX                INT = 2
DECLARE @I_LOG_LEVEL_MIN                INT = 1
DECLARE @I_LOG_LEVEL_ERROR              INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED        INT = -1
DECLARE @C_NO                           CHAR(1) = 'N'
DECLARE @C_YES                          CHAR(1) = 'Y'
DECLARE @C_RESURRECT                    CHAR(1) = 'R'

DECLARE @cur_Tracking                   CURSOR
DECLARE @bit_VerifyHost                 BIT
DECLARE @bit_WhiteListed                BIT = 0
DECLARE @i_DynamicMsgLogLevel           INT
DECLARE @i_ErrorNumber                  INT 
DECLARE @i_ErrorSeverity                INT  
DECLARE @i_ErrorState                   INT
DECLARE @bi_ConnectionLost				INT
DECLARE	@i_RowsAffected	                INT  
DECLARE	@i_RowCount                     INT 
DECLARE @bi_CleanupSeq                  BIGINT
DECLARE @bi_LogSeq                      BIGINT 	
DECLARE @bi_MinProcessedId              BIGINT
DECLARE @bi_ProcessedId                 BIGINT
DECLARE @nv_Job	                        NVARCHAR(50)
DECLARE @nv_JobName                     NVARCHAR(50)
DECLARE	@nv_Err                         NVARCHAR(MAX) 
DECLARE @nv_ErrorMessage                NVARCHAR(4000)
DECLARE @nv_Loc	                        NVARCHAR(64) 
DECLARE	@nv_Msg	                        NVARCHAR(MAX) 
DECLARE @nv_PackageExecuting            NVARCHAR(128)
DECLARE	@nv_SessionName	                NVARCHAR(100)
DECLARE @nv_Sql	                        NVARCHAR(MAX)
DECLARE @nv_TableName                   NVARCHAR(50)
DECLARE @bi_ProcessedIdCount            BIGINT
DECLARE @nv_SqlSelectJoin				NVARCHAR(4000)
DECLARE @nv_SqlDeleteJoin               NVARCHAR(4000)
DECLARE @bi_DeleteProcessedId			BIGINT
DECLARE @nv_DeleteINList                NVARCHAR(max)

-- Declare csiDataStoreInit OUTPUT variables
DECLARE @nv_TableType                   NVARCHAR(8)
DECLARE @c_LastTxnId                    CHAR(16)
DECLARE @bi_LastId                      BIGINT
DECLARE @bi_WaitingId                   BIGINT
DECLARE @nv_Access_Mode	                NVARCHAR(8)
DECLARE @i_Cleanup_Batch_Size           INT
DECLARE @nv_DataStoreDelimiter          NVARCHAR(10)
DECLARE @i_DataStoreInsertTables        INT
DECLARE @c_DataStorePresent             CHAR(1)
DECLARE @c_DataStore_Terminate          CHAR(1)
DECLARE @i_Insert_Update_Batch_Size     INT
DECLARE @i_WaitTime                     INT
DECLARE @c_Keep_Remote_Records          CHAR(1)
DECLARE @i_Log_Level	                INT
DECLARE @i_Log_Retention                INT
DECLARE @i_Missing_Txn_Retries          INT
DECLARE @c_Stop_If_Retries_Exceeded     CHAR(1)
DECLARE @c_Stop_On_Duplicate_Insert     CHAR(1)
DECLARE @c_Stop_On_No_Update            CHAR(1)
DECLARE @c_Verify_Host	                CHAR(1)
DECLARE @nv_Version                     NVARCHAR(10)

BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_JobName = N'csiDataStoreCleanup' 
		SET @nv_Loc = N'csiDataStoreCleanup'

		SELECT 
		@i_Log_Level = CONVERT(INT,VALUE) 
		FROM DataStoreSetUp WITH(NOLOCK)
		WHERE Parameter = 'LOG_LEVEL'

		SET @nv_PackageExecuting = @nv_JobName
		SET @bi_LogSeq = 1

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
		SET @nv_Loc = N'[ 0 ] csiDataStoreCleanup Initialization'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @bi_CleanupSeq = 1
		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] Begin'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Loc = N'csiDataStoreCLeanup'
		SET @nv_TableName = N'csiDataStoreCLeanup'

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
			@pi_WaitTime = @i_WaitTime OUT, 
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

			SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
			SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] End.'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'csiDataStoreCleanup'
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
			@pnv_TableName = @nv_TableName,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT

		SET @nv_Msg = N'Check to see if Keep_Remote_Records = ' + N'''' + N'Y' + N''''
		IF @c_Keep_Remote_Records = 'Y'
		BEGIN
			-- Keep_Remote_records = 'Y'.  Exit
			SET @nv_Msg = N'*** KEEP_REMOTE_RECORDS = ' + N'''' + N'Y' + N'''' + N' ***' 
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
			SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] End.'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
				@pnv_Job = @nv_Job,
				@pnv_PackageExecuting = @nv_PackageExecuting,
				@pi_Log_Level = @i_Log_Level,
				@pbi_LogSeq = @bi_LogSeq OUT

			SET @nv_Loc = N'csiDataStoreCleanup'
			RETURN
		END

		SET @nv_Msg = N'WHILE @c_DataStore_Terminate = ' + N'''' + N'N' + N'''' + N' LOOP'
		WHILE @c_DataStore_Terminate = @C_NO
		BEGIN
			IF @bi_CleanupSeq > 1 
			BEGIN
				SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'
				SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR, @bi_CleanUpSeq) + N' ] Begin.  DATASTORESETUP.INSERT_UPDATE_WAIT_TIME [ ' +  CONVERT(NVARCHAR, @i_WaitTime) + N' ] seconds reached.'	
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT

				SET @nv_Loc = N'csiDataStoreCleanup'
			END
			SET @i_RowsAffected = 0
			SET @nv_Msg = N'Open cursor cur_Tracking'

			SET @cur_Tracking = CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR
      				SELECT SessionName,ProcessedId   
      				FROM DataStoreSessionTracking  WITH(NOLOCK)
					WHERE SessionName IN (SELECT Name FROM SYSOBJECTS WHERE TYPE = 'SN' AND NAME LIKE 'DataStore%')
	   		OPEN @cur_Tracking
   			FETCH NEXT FROM @cur_Tracking INTO @nv_SessionName, @bi_ProcessedId
	   		WHILE (@@FETCH_STATUS = 0)
   			BEGIN
				SET @nv_Msg = N'DELETING FROM ' + @nv_SessionName + N'MASTER queue table'
      				SET @bi_MinProcessedId = NULL

					set @bi_ProcessedIdCount = 0
					set @nv_SqlSelectJoin = N''
					set @nv_SqlDeleteJoin = N''

					-- find out if there are any datastoremissingtxn rows for this queue table, only use join with that table if there are
					SET @nv_Sql = N'SELECT @pbi_ProcessedIdCount=COUNT(MissedId) FROM DataStoreMissingTxns WITH(NOLOCK) WHERE SessionName = ' + N'''' + @nv_SessionName + N'''' +
						N' AND MissedId <= @pbi_ProcessedId'

					EXEC sp_executesql @nv_Sql,  N'@pbi_ProcessedIdCount BIGINT OUTPUT, @pbi_ProcessedId BIGINT', @pbi_ProcessedIdCount = @bi_ProcessedIdCount OUTPUT, @pbi_ProcessedId = @bi_ProcessedId

					if @bi_ProcessedIdCount > 0
					BEGIN
						SET @nv_SqlSelectJoin = N' WHERE NOT EXISTS (select MissedId from DataStoreMissingTxns MT WITH (NOLOCK) where QT.ID = MT.MissedId AND MT.SessionName = ' + N'''' + @nv_SessionName + N''')' 
						SET @nv_SqlDeleteJoin =   N' AND NOT EXISTS (select MissedId from DataStoreMissingTxns MT WITH (NOLOCK) where QT.ID = MT.MissedId AND MT.SessionName = ' + N'''' + @nv_SessionName + N''')' 
					END -- @bi_ProcessedIdCount > 0

					-- Select against synonym which selects across linked server to query Master queue table on OLTP to get lowest ID, only use join
					-- if there are corresponding rows in datastoremissingtxns table
					SET @nv_Sql = N'SELECT @pbi_MinProcessedId = ISNULL(MIN(QT.ID),0) FROM ' + @nv_SessionName + N'Master QT WITH (NOLOCK) ' + 
						@nv_SqlSelectJoin
    				

      				EXEC sp_executesql @nv_Sql, N'@pbi_MinProcessedId BIGINT OUTPUT', 
					@pbi_MinProcessedId = @bi_MinProcessedId OUTPUT

				IF @bi_MinProcessedId IS NOT NULL
      			BEGIN
         			SET @bi_MinProcessedId = @bi_MinProcessedId + @i_Cleanup_Batch_Size
         	  
			       	-- Delete from the master queue table on the OLTP where the ID is less than the most recent processed id in the DataStoreSessionTracking table and the ID is less than the lowest ID
					-- in the Master queue table on the OLTP plus the cleanup batch size
         			-- To avoid join with remote synonym queue table, create cursor to make comma delimited list of IDs to delete

					set @nv_Sql = 'DECLARE cur_DeleteRows CURSOR STATIC READ_ONLY FORWARD_ONLY FOR ' +   
					N'SELECT Id FROM ' + @nv_SessionName + N'Master QT WITH (NOLOCK) ' +
					N'WHERE QT.ID <= @pbi_ProcessedId AND QT.Id < @pbi_MinProcessedId ' + 
					@nv_SqlDeleteJoin
		
					exec sp_executesql @nv_Sql, N'@pbi_ProcessedId BIGINT, @pbi_MinProcessedId BigInt',  @pbi_ProcessedId = @bi_ProcessedId, @pbi_MinProcessedId = @bi_MinProcessedId  

					OPEN cur_DeleteRows
					set @nv_DeleteINList = N''
   					FETCH NEXT FROM cur_DeleteRows INTO @bi_DeleteProcessedId	
					WHILE (@@FETCH_STATUS = 0) 
   						BEGIN 
							if @nv_DeleteINList != ''
								set @nv_DeleteINList = @nv_DeleteINList + N','

							set @nv_DeleteINList = @nv_DeleteINList + CAST(@bi_DeleteProcessedId as NVARCHAR(max))
						
							FETCH NEXT FROM cur_DeleteRows INTO @bi_DeleteProcessedId	
						END -- while (@@FETCH_STATUS = 0) 

					CLOSE cur_DeleteRows
   					DEALLOCATE cur_DeleteRows
					
					IF @nv_DeleteINList != ''
					BEGIN
						SET @nv_Sql = N'DELETE FROM ' + @nv_SessionName + N'Master ' +  
										N'WHERE ID IN (' + @nv_DeleteINList + N')'
						EXEC sp_executesql @nv_Sql 
						SET @i_RowCount = @@ROWCOUNT 
					
						SET @i_RowsAffected = @i_RowsAffected + @i_RowCount

						SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowCount) + N' ] ' + 
						REPLACE(REPLACE(@nv_SQL,'@pbi_ProcessedId', N'[ ' + CONVERT(NVARCHAR, @bi_ProcessedId) + N' ]'),'@pbi_MinProcessedId', N'[ ' + CONVERT(NVARCHAR, @bi_MinProcessedId) + N' ]')
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT

						IF @i_RowCount > 0 
						BEGIN
							SET @nv_Msg = N'COMMIT (@@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : Issued after DELETE FROM ' + @nv_SessionName + N'Master'
							EXEC csiDataStoreLogMessage 
								@pnv_Msg = @nv_Msg, 
								@pnv_Loc = @nv_Loc, 
								@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
								@pnv_Job = @nv_Job,
								@pnv_PackageExecuting = @nv_PackageExecuting,
								@pi_Log_Level = @i_Log_Level,
								@pbi_LogSeq = @bi_LogSeq OUT
						END -- IF @i_RowCount > 0 
					END -- IF @nv_DeleteINList != ''
					ELSE -- @nv_DeleteINList != ''
					BEGIN
						SET @nv_Msg = N'No rows found to delete in ' + @nv_SessionName + N'Master'
						EXEC csiDataStoreLogMessage 
							@pnv_Msg = @nv_Msg, 
							@pnv_Loc = @nv_Loc, 
							@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
							@pnv_Job = @nv_Job,
							@pnv_PackageExecuting = @nv_PackageExecuting,
							@pi_Log_Level = @i_Log_Level,
							@pbi_LogSeq = @bi_LogSeq OUT
					END  -- @nv_DeleteINList != ''
				END -- IF @bi_MinProcessedId IS NOT NULL
  
      				FETCH NEXT FROM @cur_Tracking INTO @nv_SessionName, @bi_ProcessedId  
   			END -- WHILE (@@FETCH_STATUS = 0)

	   		CLOSE @cur_Tracking
   			DEALLOCATE @cur_Tracking

			SET @nv_Msg = N'Evaluate if any rows were deleted'
			IF @i_RowsAffected = 0
			BEGIN
				-- No rows were deleted, so its ok to sleep
				WAITFOR DELAY @i_WaitTime
			END
			ELSE
			BEGIN
				SET @nv_Msg = N'Total rows affected [ ' +  CONVERT(NVARCHAR,@i_RowsAffected) + N' ] from DATASTORE%MASTER queue tables '
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 		
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
					@pnv_Job = @nv_Job,
					@pnv_PackageExecuting = @nv_PackageExecuting,
					@pi_Log_Level = @i_Log_Level,
					@pbi_LogSeq = @bi_LogSeq OUT
			END

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
				@pi_WaitTime = @i_WaitTime OUT, 
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

			IF @c_DataStore_Terminate = @C_NO
				SET @bi_CleanupSeq = @bi_CleanupSeq +1

		END -- WHILE @c_DataStore_Terminate = @C_NO

		SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
		SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] End.'
		EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_MIN,
			@pnv_Job = @nv_Job,
			@pnv_PackageExecuting = @nv_PackageExecuting,
			@pi_Log_Level = @i_Log_Level,
			@pbi_LogSeq = @bi_LogSeq OUT
	
		SET @nv_Loc = N'csiDataStoreCleanup'
	END TRY
	BEGIN CATCH
		SELECT   
        @nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),    
        @i_ErrorSeverity = ERROR_SEVERITY(),  
        @i_ErrorState = ERROR_STATE(),		
		@i_ErrorNumber = ERROR_NUMBER()
		
		SET @nv_JobName = N'csiDataStoreCleanup' 
		SET @nv_Loc = N'csiDataStoreCleanup'
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
											
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] End.'
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
						
					SET @nv_Loc = N'[ ' + CONVERT(NVARCHAR, @bi_CleanupSeq) + N' ] csiDataStoreCleanup'	
					SET @nv_Msg = N'Iteration [ ' + CONVERT(NVARCHAR,@bi_CleanupSeq) + N' ] End.'
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
