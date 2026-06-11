------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreVerifyJob.sql
-- DESCR:       Checks to see if the job exists. If not, creates it.
-- Copyright Siemens 2024  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreVerifyJob' AND Type = 'P')
	DROP PROCEDURE csiDataStoreVerifyJob
GO




CREATE PROCEDURE csiDataStoreVerifyJob
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreVerifyJob
-- Params:      <in>  @pnv_ProcName             NVARCHAR(50)
--              <in>  @pnv_TableName            NVARCHAR(50) = NULL
--              <out> @pnv_msg                  NVARCHAR(4000) = NULL
--              <in>  @pnv_Job                  NVARCHAR(50) = NULL
--              <in>  @pnv_PackageExecuting     NVARCHAR(128) = NULL
--              <in>  @pi_Log_Level             INT = NULL
--              <out> @pbi_LogSeq               BIGINT 	= NULL	OUTPUT
-- Descr:       Checks to see if the job exists. If not, creates it. If disabled, enables it.  If stopped, starts it.  Executes on the ODS	
--		
-- HISTORY:
--              06/29/2016      Dan Maloney      New stored procedure
--              03/02/2017      Alex Lind        rebuild  to make job db name based, and rework validation of job status to remove dependency on user name or schema name.  
--              07/26/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/26/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/26/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/26/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/26/2017      Dan Maloney      Added logging to catch block of procedure (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variables @BIT_DISABLE, @BIT_ENABLE, @BIT_FALSE (US 51393)
--              12/04/2024      Dan Maloney      Replace MSDB..SYSJOBS with MSDB..SYSJOBS_VIEW
--
-- Copyright Siemens 2024 
-----------------------------------------------------------------------------------------------------------------------------------------------------
(  
        @pnv_ProcName                   NVARCHAR(50),  
        @pnv_TableName                  NVARCHAR(50) = NULL,  
        @pnv_Job                        NVARCHAR(50) = NULL,  
        @pnv_PackageExecuting           NVARCHAR(128) = NULL,  
        @pi_Log_Level                   INT = NULL,  
        @pbi_LogSeq                     BIGINT = NULL OUTPUT  
)  
AS  
--Variables defined as constants
DECLARE @BIT_TRUE                       BIT = 1 
DECLARE @I_LOG_LEVEL_MAX                INT = 2  
DECLARE @I_LOG_LEVEL_MIN                INT = 1  
DECLARE @I_LOG_LEVEL_ERROR              INT = 0  
DECLARE @I_LOG_LEVEL_WHITELISTED        INT = -1

DECLARE @bit_WhiteListed                BIT = 0
DECLARE @i_DynamicMsgLogLevel           INT
DECLARE @i_JobEnabled                   INT  
DECLARE @i_ErrorNumber                  INT  
DECLARE @i_ErrorSeverity                INT    
DECLARE @i_ErrorState                   INT  
DECLARE @i_ReturnCode                   INT  
DECLARE @bi_RowsAffected                BIGINT  
DECLARE @bin_JobId                      BINARY(16)  
DECLARE @dt_Stop_Execution_Date         DATETIME  
DECLARE @dt_Start_Execution_Date        DATETIME  
DECLARE	@nv_Command                     NVARCHAR(100)
DECLARE @nv_DatabaseName                NVARCHAR(50)   
DECLARE @nv_JobId                       NVARCHAR(50)  
DECLARE @nv_JobName                     NVARCHAR(100)  
DECLARE @nv_OldName                     NVARCHAR(100)   
DECLARE @nv_Loc                         NVARCHAR(64)   
DECLARE	@nv_Err                         NVARCHAR(MAX)   
DECLARE @nv_ErrorMessage                NVARCHAR(4000)  
DECLARE	@nv_Msg                         NVARCHAR(MAX)   
BEGIN  
   	SET NOCOUNT ON  
 
	BEGIN TRY  
		SET @nv_Loc = N'csiDataStoreVerifyJob'  
		SET @nv_Msg = N' Init Verify Job' 
		SET @nv_JobName = @pnv_ProcName 
		SET @nv_DatabaseName = DB_NAME()    
		 
		IF @pnv_TableName IS NULL  
		BEGIN  
			SET @nv_JobName = @pnv_ProcName + N' (' + @nv_DatabaseName + N')'  
			SET @nv_Command = N'exec ' + @pnv_ProcName  
		END  
		ELSE  
		BEGIN  
			SET @nv_JobName = @pnv_ProcName + N'(' + '''' + @pnv_TableName + '''' + N')' + N' (' + @nv_DatabaseName + N')'  
			SET @nv_Command = N'exec ' + @pnv_ProcName + N' ' + N'''' + @pnv_TableName + N''''  
		END  
		 
		--Check old name version and remove
		IF(DB_NAME()<>USER_NAME())
		BEGIN
			SET @nv_OldName = replace(@nv_JobName,DB_NAME(),USER_NAME()) 
			IF(EXISTS (Select 1 from msdb..sysjobs_view where name= @nv_OldName))			  
				EXEC msdb.dbo.sp_delete_job  @job_name = @nv_OldName 
		END
		
		-- Get Job Info
			SELECT top(1) @i_JobEnabled = j.Enabled, @dt_Start_Execution_Date=a.start_execution_date ,@dt_Stop_Execution_Date= a.Stop_Execution_Date 
			FROM msdb..sysjobs_view j WITH(NOLOCK)  
			LEFT JOIN msdb.dbo.sysjobactivity a WITH(NOLOCK)  
			ON a.Job_Id = j.Job_Id  
			WHERE j.name= @nv_JobName  
			ORDER BY start_execution_date DESC 
          	 
		SET @bi_RowsAffected = @@ROWCOUNT  
		  
		--Check Job Status
		IF @bi_RowsAffected = 0  --No Job, create Job
		BEGIN
			SET @nv_Loc = @nv_Loc + N' : Create Job'    
			EXEC @i_ReturnCode = msdb.dbo.sp_add_job   
			@job_id = @bin_JobId OUTPUT, 	  
			@job_name = @nv_JobName,   
			@description = @nv_JobName  
  
			EXEC @i_ReturnCode = msdb.dbo.sp_add_jobstep   
 			@job_id = @bin_JobId,   
			@step_id = 1, 		  
			@step_name = N'Step 1',   
 			@command = @nv_Command,   
 			@database_name = @nv_DatabaseName,  
 			@server = N'',  
 			@on_fail_action = 2,  
			@subsystem = N'TSQL'  
  
			EXEC @i_ReturnCode = msdb.dbo.sp_update_job   
 			@job_id = @bin_JobId,   
			@start_step_id = 1,   
			@enabled = 1  
  
			EXEC @i_ReturnCode = msdb.dbo.sp_add_jobschedule   
 			@job_id = @bin_JobId, 	  
			@name = N'Every Minute', 	  
			@freq_type = 4,   
 			@freq_interval = 1,   
 			@freq_subday_type = 4,   
 			@freq_subday_interval = 1  
  
			EXEC @i_ReturnCode = msdb.dbo.sp_add_jobserver   
 			@job_id = @bin_JobId,   
			@server_name = N'(LOCAL)'  
  
			SELECT @nv_JobId = j.Job_Id  
			FROM msdb..sysjobs_view j WITH(NOLOCK)  
			WHERE j.Name = @nv_JobName  
  
			SET @nv_Msg = N'Job [ ' + @nv_JobId + N' ' +  @nv_JobName + N' ] submitted to run every minute'  
  
			EXEC csiDataStoreLogMessage   
				@pnv_Msg = @nv_Msg,   
				@pnv_Loc = @nv_Loc, 		  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,  
				@pnv_Job = @pnv_Job,  
				@pnv_PackageExecuting = @pnv_PackageExecuting,  
				@pi_Log_Level = @pi_Log_Level,  
				@pbi_LogSeq = @pbi_LogSeq OUT  
			
			EXEC msdb..sp_start_job   -- start the new job
				@job_name = @nv_JobName   
		END  
		ELSE IF @i_JobEnabled = 0  -- Job exists but is disabled.  Enable the Job
		BEGIN 			 
			SET @nv_Loc = @nv_Loc + N' : Enable Job'
			EXEC msdb..sp_update_job  
				@job_name = @nv_JobName,  
				@enabled = 1  

			SET @nv_Msg = N'Job [ ' + @nv_JobName + N' ] enabled'  
 
			EXEC csiDataStoreLogMessage   
				@pnv_Msg = @nv_Msg,   
				@pnv_Loc = @nv_Loc, 		  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,  
				@pnv_Job = @pnv_Job,  
				@pnv_PackageExecuting = @pnv_PackageExecuting,  
				@pi_Log_Level = @pi_Log_Level,  
				@pbi_LogSeq = @pbi_LogSeq OUT  
		END  
		ELSE IF @dt_Stop_Execution_Date is not null -- Last history has stop time, job has stopped
		BEGIN  
			SET @nv_Loc = @nv_Loc + N' : Restart Job'
			-- Start the job
			EXEC msdb..sp_start_job   
				@job_name = @nv_JobName   
			
			SET @nv_Msg = N'Job [ ' +  @nv_JobName + N' ] started'  
  
			EXEC csiDataStoreLogMessage   
				@pnv_Msg = @nv_Msg,   
				@pnv_Loc = @nv_Loc, 		  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,  
				@pnv_Job = @pnv_Job,  
				@pnv_PackageExecuting = @pnv_PackageExecuting,  
				@pi_Log_Level = @pi_Log_Level,  
				@pbi_LogSeq = @pbi_LogSeq OUT  
		END  
		ELSE IF GetDate()<@dt_Start_Execution_Date -- Job is scheduled for later
			BEGIN  
				SET @nv_Loc = @nv_Loc + N' : Scheduled Job' 
				SET @nv_Msg = N'Job [ ' +   @nv_JobName + N' ] is scheduled'  

				EXEC csiDataStoreLogMessage   
					@pnv_Msg = @nv_Msg,   
					@pnv_Loc = @nv_Loc, 		  
					@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,  
					@pnv_Job = @pnv_Job,  
					@pnv_PackageExecuting = @pnv_PackageExecuting,  
					@pi_Log_Level = @pi_Log_Level,  
					@pbi_LogSeq = @pbi_LogSeq OUT  
			END   
		ELSE  -- Job exists, has a past start date, and no stop date, therefore should be running.
			BEGIN 
				SET @nv_Loc = @nv_Loc + N' : Running Job'
				SET @nv_Msg = N'Job [ ' +  @nv_JobName + N' ] is running' 
 
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


