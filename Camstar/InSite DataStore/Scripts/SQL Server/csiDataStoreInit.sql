------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreInit.sql
-- Descr:       Gets setup information from InSiteSiteInfo table and DataStoreSetUp and inserts record into DataStoreSessionTracking if missing for 
--              SessionName (@pnv_TableName) or updates DataStoreSessionTracking Timestamp field for SessionName (@pnv_TableName) if it exists.
--              Execute on ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreInit' AND Type = 'P')
    DROP PROCEDURE csiDataStoreInit
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE csiDataStoreInit
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreInit
-- Params:      <in>  @pnv_TableName                    NVARCAHR(50)    (DATASTOREUPDATES, DATASTOREINSERTS1,2,3,4...) 
--              <out> @pnv_TableType                    NVARCHAR(8)     OUTPUT
--              <out> @pc_LastTxnId                     CHAR(16)        OUTPUT
--              <out> @pbi_LastId                       BIGINT          OUTPUT
--              <out> @pbi_WaitingId                    BIGINT          OUTPUT	
--              <out> @pnv_Access_Mode                  NVARCHAR(8)     OUTPUT
--              <out> @pi_Cleanup_Batch_Size            INT             OUTPUT
--              <out> @pnv_DataStoreDelimiter           NVARCHAR(10)    OUTPUT
--              <out> @pi_DataStoreInsertTables         INT             OUTPUT
--              <out> @pc_DataStorePresent              CHAR(1)         OUTPUT
--              <out> @pc_DataStore_Terminate           CHAR(1)         OUTPUT
--              <out> @pi_Insert_Update_Batch_Size      INT             OUTPUT
--              <out> @pi_WaitTime                      INT             OUTPUT
--              <out> @pc_Keep_Remote_Records           CHAR(1)         OUTPUT
--              <out> @pi_Log_Level                     INT             OUTPUT
--              <out> @pi_Log_Retention                 INT             OUTPUT
--              <out> @pi_Missing_Txn_Retries           INT             OUTPUT
--              <out> @pc_Stop_If_Retries_Exceeded      CHAR(1)         OUTPUT
--              <out> @pc_Stop_On_Duplicate_Insert      CHAR(1)	        OUTPUT
--              <out> @pc_Stop_On_No_Update             CHAR(1)         OUTPUT
--              <out> @pc_Verify_Host                   CHAR(1)         OUTPUT
--              <out> @pnv_Version                      NVARCHAR(10)    OUTPUT
--              <in>  @pnv_Job                          NVARCHAR(50)
--              <in>  @pnv_PackageExecuting             NVARCHAR(128)
--              <out> @pbi_LogSeq                       BIGINT          OUTPUT
--
-- Descr:       Gets setup information from InSiteSiteInfo table and DataStoreSetUp and inserts record into DataStoreSessionTracking if missing for 
--              SessionName (@pnv_TableName) or updates DataStoreSessionTracking Timestamp field for SessionName (@pnv_TableName) if it exists.
--              Execute on ODS
--
-- HISTORY:
--              05/04/2005                       Added PROCESSEDID column in DataStoreSessionTracking
--              05/20/2005                       Added session tracking TIMESTAMP update.
--              06/06/2005 			 Made (remote-accessing) cursors read-only and forward-only
--              07/11/2005 			 Added MISSING_TXN_RETRIES
--              07/15/2005                       Converted to local (replication) mode
--              01/30/2006                       Added WaitingId to parameters
--              02/10/2006                       Fixed RemoteDB
--              03/01/2006                       Added STOP_IF_RETRIES_EXCEEDED parameter
--              08/11/2006                       Changed nvarchar columns to MAX
--              12/06/2006                       Updated/added copyright notice(s) (SPR S9984) Bill Lippard.
--              04/23/2007                       Updated copyright notice(s) (SPR S9984) Bill Lippard.
--              07/03/2007                       Changed RemoteDB to be local schema name
--              05/04/2005                       Added ID column.
--              05/20/2005                       Added index.
--              06/23/2005                       Added master tables, triggers, etc. to support new master/detail architecture.
--              08/11/2006                       Changed nvarchar columns to MAX
--              12/06/2006                       Updated/added copyright notice(s) (SPR S9984) Bill Lippard
--              04/23/2007                       Updated copyright notice(s) (SPR S9984) Bill Lippard
--              06/13/2016      Dan Maloney      Restructured Code
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/24/2017      Dan Maloney      Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/24/2017      Dan Maloney      Added @i_ErrorNumber variable declaration and added to SELECT to get error information in catch block (US 51393)
--              07/24/2017      Dan Maloney      Removed stopping of ODS from catch block as the caller will do the stopping of the ODS if the exception is not WhiteListed (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variables @BIT_ENABLE, @BIT_DISABLE, @bi_Job, @bi_LogSeq, @nv_Sql (US 51393)
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName                  NVARCHAR(50),
      @pnv_TableType                  NVARCHAR(8)     OUTPUT,
      @pc_LastTxnId                   CHAR(16)        OUTPUT,
      @pbi_LastId                     BIGINT          OUTPUT,
      @pbi_WaitingId                  BIGINT          OUTPUT,
      @pnv_Access_Mode                NVARCHAR(8)     OUTPUT,
      @pi_Cleanup_Batch_Size          INT             OUTPUT,
      @pnv_DataStoreDelimiter         NVARCHAR(10)    OUTPUT,
      @pi_DataStoreInsertTables       INT             OUTPUT,
      @pc_DataStorePresent            CHAR(1)         OUTPUT,
      @pc_DataStore_Terminate         CHAR(1)         OUTPUT,
      @pi_Insert_Update_Batch_Size    INT             OUTPUT,
      @pi_WaitTime                    INT             OUTPUT,
      @pc_Keep_Remote_Records         CHAR(1)         OUTPUT,
      @pi_Log_Level                   INT             OUTPUT,
      @pi_Log_Retention               INT             OUTPUT,
      @pi_Missing_Txn_Retries         INT             OUTPUT,
      @pc_Stop_If_Retries_Exceeded    CHAR(1)         OUTPUT,
      @pc_Stop_On_Duplicate_Insert    CHAR(1)         OUTPUT,
      @pc_Stop_On_No_Update           CHAR(1)         OUTPUT,
      @pc_Verify_Host                 CHAR(1)	      OUTPUT,
      @pnv_Version                    NVARCHAR(10)    OUTPUT,
      @pnv_Job                        NVARCHAR(50),
      @pnv_PackageExecuting           NVARCHAR(128),
      @pbi_LogSeq                     BIGINT 	      OUTPUT
)
AS
--Variables defined as constants
DECLARE @BIT_TRUE                     BIT = 1
DECLARE @I_LOG_LEVEL_MAX              INT = 2
DECLARE @I_LOG_LEVEL_MIN              INT = 1
DECLARE @I_LOG_LEVEL_ERROR            INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED      INT = -1

DECLARE @cur_Parms                    CURSOR
DECLARE @bit_WhiteListed              BIT = 0
DECLARE @i_DynamicMsgLogLevel         INT
DECLARE @i_ErrorNumber		      INT 
DECLARE @i_ErrorSeverity              INT  
DECLARE @i_ErrorState                 INT
DECLARE	@i_RowsAffected	              INT 
DECLARE @dt_CurrentDateTime           DATETIME
DECLARE @nv_Loc                       NVARCHAR(64)
DECLARE @nv_Name                      NVARCHAR(100)
DECLARE @nv_Value                     NVARCHAR(512)
DECLARE	@nv_Err                       NVARCHAR(MAX)
DECLARE @nv_ErrorMessage              NVARCHAR(4000)
DECLARE	@nv_Msg                       NVARCHAR(MAX) 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = N'csiDataStoreInit'

		-- Default values
		SET @pnv_Access_Mode = N'LOCAL'
   		SET @pi_Cleanup_Batch_Size = 5000
		SET @pc_DataStore_Terminate = 'N'
		SET @pnv_DataStoreDelimiter = N';$$$;'
		SET @pi_DataStoreInsertTables = 2
		SET @pc_DataStorePresent = 'Y'
	   	SET @pi_Insert_Update_Batch_Size = 100
   		SET @pi_WaitTime = 2
	   	SET @pc_Keep_Remote_Records = 'N'
		SET @pi_Log_Level = 0
		SET @pi_Log_Retention = 180
		SET @pi_Missing_Txn_Retries = 10
   		SET @pc_Stop_If_Retries_Exceeded = 'Y'
   		SET @pc_Stop_On_Duplicate_Insert = 'Y'
   		SET @pc_Stop_On_No_Update = 'Y'
		SET @pc_Verify_Host = 'Y'
		SET @pnv_Version = NULL

   		SET @pbi_WaitingId = 0

	   	IF UPPER(@pnv_TableName) = N'DATASTOREUPDATES'
      			SET @pnv_TableType = N'UPDATES'
	   	ELSE
      			SET @pnv_TableType = N'INSERTS'

		-- DataStoreSetUp
	   	SET @cur_Parms = CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR
      			SELECT Parameter, Value
      			FROM DataStoreSetUp WITH(NOLOCK)

	   	OPEN @cur_Parms
   		FETCH NEXT FROM @cur_Parms INTO @nv_Name, @nv_Value
	   	WHILE (@@FETCH_STATUS = 0)
   		BEGIN

			SET @nv_Msg = N'Set @pnv_Access_Mode [ ' + @nv_Value + ' ]'
     			IF (@nv_Name = UPPER(N'ACESS_MODE'))		
				SET @pnv_Access_Mode = @nv_Value

			SET @nv_Msg = N'Set @pi_Cleanup_Batch_size[ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'CLEANUP_BATCH_SIZE'))
        	 		SET @pi_Cleanup_Batch_Size= CONVERT(INT,@nv_Value)

			SET @nv_Msg = N'Set @pc_DataStore_Terminate [ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'DATASTORE_TERMINATE'))
        	 		SET @pc_DataStore_Terminate = @nv_Value

			SET @nv_Msg = N'Set @pi_Insert_Update_Bacth_Size[ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'INSERT_UPDATE_BATCH_SIZE'))
        	 		SET @pi_Insert_Update_Batch_Size = CONVERT(INT,@nv_Value)

			SET @nv_Msg = N'Set @pi_WaitTime [ ' + @nv_Value + ' ]'         
      			IF (@nv_Name = UPPER(N'INSERT_UPDATE_WAIT_TIME'))	
				SET @pi_WaitTime = CONVERT(INT,REPLACE(@nv_Value ,':',''))

			SET @nv_Msg = N'Set @pc_Keep_Remote_Records [ ' + @nv_Value + ' ]'      
      			IF (@nv_Name = UPPER(N'KEEP_REMOTE_RECORDS'))
        	 		SET @pc_Keep_Remote_Records = @nv_Value

			SET @nv_Msg = N'Set @pi_Log_Level [ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'LOG_LEVEL'))
			BEGIN
        	 		SET @pi_Log_Level = CONVERT(INT,@nv_Value)
			END

			SET @nv_Msg = N'Set @pi_Log_Retention [ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'LOG_RETENTION'))
        	 		SET @pi_Log_Retention = CONVERT(INT,@nv_Value)

			SET @nv_Msg = N'Set @pi_Missing_Txn_Retries [ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'MISSING_TXN_RETRIES'))
        	 		SET @pi_Missing_Txn_Retries = CONVERT(integer,@nv_Value)

	    		
			SET @nv_Msg = N'Set @pc_Stop_If_Retries_Exceeded [ ' + @nv_Value + ' ]'
			IF (@nv_Name = UPPER(N'STOP_IF_RETRIES_EXCEEDED'))
        	 		SET @pc_Stop_If_Retries_Exceeded = @nv_Value

			SET @nv_Msg = N'Set @pc_Stop_On_Duplicate_Insert [ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'STOP_ON_DUPLICATE_INSERT'))
        	 		SET @pc_Stop_On_Duplicate_Insert = @nv_Value

			SET @nv_Msg = N'Set @pc_Stop_On_No_Update[ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'STOP_ON_NO_UPDATE'))
        	 		SET @pc_Stop_On_No_Update = @nv_Value

			SET @nv_Msg = N'Set @pnv_Verify_Host [ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'VERIFY_HOST'))
        	 		SET @pc_Verify_Host = @nv_Value

			SET @nv_Msg = N'Set @pnv_Version [ ' + @nv_Value + ' ]'
	    		IF (@nv_Name = UPPER(N'VERSION'))
        	 		SET @pnv_Version = @nv_Value
         
      			FETCH NEXT FROM @cur_Parms INTO @nv_Name, @nv_Value    
   		END

	   	CLOSE @cur_Parms
   		DEALLOCATE @cur_Parms

		-- InsiteSiteInfo
	   	SET @cur_Parms = CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR
      			SELECT TName, TValue
      			FROM InsiteSiteInfo WITH(NOLOCK)

	   	OPEN @cur_Parms
   		FETCH NEXT FROM @cur_Parms INTO @nv_Name, @nv_Value
	   	WHILE (@@FETCH_STATUS = 0)
   		BEGIN
			SET @nv_Msg = N'Set @pnv_DataStoreDelimiter [ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'DATASTOREDELIMITER'))
        	 		SET @pnv_DataStoreDelimiter = @nv_Value

			SET @nv_Msg = N'Set @pnv_DataStoreInsertTables[ ' + @nv_Value + ' ]'
     			IF (@nv_Name = UPPER(N'DATASTOREINSERTTABLES'))		
				SET @pi_DataStoreInsertTables = CONVERT(INT,@nv_Value)

			SET @nv_Msg = N'Set @pnv_DataStorePresent [ ' + @nv_Value + ' ]'
	      		IF (@nv_Name = UPPER(N'DATASTOREPRESENT'))
        	 		SET @pc_DataStorePresent = @nv_Value

 			FETCH NEXT FROM @cur_Parms INTO @nv_Name, @nv_Value    
   		END

	   	CLOSE @cur_Parms
   		DEALLOCATE @cur_Parms

		IF @pnv_TableName IS NOT NULL
		BEGIN
			SET @nv_Msg = N'Parameters set from DataStoreSetUp and InsiteSiteInfo'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT
		END

		-- DataStoreSessionTracking logic for DATASTOREUPDATES, DATASTOREINSERTSx
		IF UPPER(@pnv_TableName) LIKE N'DATASTORE%'	
   		BEGIN
	      		SELECT 
			@pc_LastTxnId = ProcessedTxnId, 
			@pbi_LastId = ProcessedId, 
			@pbi_WaitingId = WaitingId 
			FROM DataStoreSessionTracking WITH(NOLOCK)
			WHERE SessionName = UPPER(@pnv_TableName)

      			IF @@ROWCOUNT = 0
			BEGIN
					SET @nv_Msg = N'Populate DataStoreSessionTracking with zero ProcessedTxnId and ProcessedId'; 
					SET @dt_CurrentDateTime = GETDATE()
		
       					INSERT 
					INTO DataStoreSessionTracking 
					(SessionName, ProcessedTxnId, ProcessedId, WaitingId, Timestamp) 
					VALUES 
					(UPPER(@pnv_TableName), '0000000000000000', 0, 0, @dt_CurrentDateTime)

					SET @i_RowsAffected = @@ROWCOUNT
       					SET @pc_LastTxnId = '0000000000000000'
	       				SET @pbi_LastId = 0

				SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ] INSERT INTO DataStoreSessionTracking (SessionName, ProcessedTxnId, ProcessedId, WaitingId, TimeStamp) VALUES (' +
					+ N'''' + UPPER(@pnv_TableName) + N'''' + N',' + N'''' + N'0000000000000000' + N'''' +  N', 0, 0,' + N'''' + CONVERT(NVARCHAR, @dt_CurrentDateTime) + N'''' + N')'
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
					@pnv_Job = @pnv_Job,
					@pnv_PackageExecuting = @pnv_PackageExecuting,
					@pi_Log_Level = @pi_Log_Level,
					@pbi_LogSeq = @pbi_LogSeq OUT

				SET @nv_Msg = N'COMMIT (@@TRANCOUNT [ ' + CONVERT(NVARCHAR,@@TRANCOUNT) + N' ] : Issued after INSERT INTO DataStoreSessionTracking';
				EXEC csiDataStoreLogMessage 
					@pnv_Msg = @nv_Msg, 
					@pnv_Loc = @nv_Loc, 
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
					@pnv_Job = @pnv_Job,
					@pnv_PackageExecuting = @pnv_PackageExecuting,
					@pi_Log_Level = @pi_Log_Level,
					@pbi_LogSeq = @pbi_LogSeq OUT
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
		BEGIN
			WHILE @@TRANCOUNT > 0
				ROLLBACK TRAN
		END

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



