------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreEnable.sql
-- DESCR:       Stored procedure to set the 'DATASTORE_TERMINATE' parameter of the  DataStoreSetUp table.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreEnable' AND Type = 'P')
    DROP PROCEDURE csiDataStoreEnable
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE csiDataStoreEnable
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreEnable
-- Params:      <in>  @pbit_Enable              BIT             1 (enable or start DataStore), 0 (disable or stop DataStore)
--              <out> @pc_DataStore_Terminate   CHAR(1)         OUTPUT 
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT          OUTPUT
--
-- Descr:       Stored procedure to set the 'DATASTORE_TERMINATE' parameter of the  DataStoreSetUp table.  Executes on the ODS
--
-- HISTORY:
--              06/17/2016      Dan Maloney      New stored procedure to set the 'DATASTORE_TERMINATE' parameter of the DataStoreSetUp table. 
--              05/12/2017      Alex Lind        Update error with query in front of rollback
--              07/24/2017	    Dan Maloney      Removed BEGIN TRAN and COMMIT (US 51393)
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/24/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/24/2017      Dan Maloney      Replaced RAISERROR with THROW statement
--		        07/26/2017      Dan Maloney      Changed @pi_Log_Level_MAX  to @I_LOG_LEVEL_MAX, @i_Log_Level_MIN to @I_LOG_LEVEL_MIN, @i_Log_Level_ERROR to @I_LOG_LEVEL_ERROR (US 51393)
--              07/20/2022      Dan Maloney      Added a new DATASTORE_TERMINATE code.  The new code is R (resurrect)  
--
-- Copyright Siemens 2023 
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pbit_Enable                    BIT,
      @pc_DataStore_Terminate         CHAR(1)         OUTPUT,
      @pnv_Job                        NVARCHAR(50),
      @pnv_PackageExecuting           NVARCHAR(128),
      @pi_Log_Level                   INT,
      @pbi_LogSeq                     BIGINT          OUTPUT
)
AS
--Variables defined as constants
DECLARE @BIT_DISABLE                  BIT = 0
DECLARE @BIT_ENABLE                   BIT = 1
DECLARE @BIT_TRUE                     BIT = 1
DECLARE @I_LOG_LEVEL_MAX              INT = 2
DECLARE @I_LOG_LEVEL_MIN              INT = 1
DECLARE @I_LOG_LEVEL_ERROR            INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED      INT = -1

DECLARE @bit_WhiteListed              BIT = 0
DECLARE @i_DynamicMsgLogLevel         INT
DECLARE @i_ErrorNumber                INT  
DECLARE @i_ErrorSeverity              INT  
DECLARE @i_ErrorState                 INT 
DECLARE	@nv_Loc                       NVARCHAR(64)
DECLARE	@nv_Err                       NVARCHAR(MAX)
DECLARE @nv_ErrorMessage              NVARCHAR(4000)
DECLARE	@nv_Msg                       NVARCHAR(MAX)  
BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = N'ENABLE'
		SET @nv_Msg = N'Evaluate pv_Enable'

		-- Any int value other than 1, 0 or NULL will be set to 1.  If a 9 is passed in, it will be set to the value of 1.  If NULL is passed in, throw error
		IF (@pbit_Enable IS NULL)
			-- Put DataStore in resurrect state
			SET @pc_DataStore_Terminate = 'R'
		ELSE IF @pbit_Enable = @BIT_DISABLE 
			-- Stop DataStore
			SET @pc_DataStore_Terminate = 'Y'
		ELSE
			-- If pbit_Enable = @BIT_ENABLE or any integer value other than 0 or -1, Start DataStore
			SET @pc_DataStore_Terminate = 'N'

		UPDATE 
		DataStoreSetUp 
		SET Value = @pc_DataStore_Terminate 
		WHERE Parameter = 'DATASTORE_TERMINATE'
		
		IF @pbit_Enable = 0
		BEGIN
			SET @nv_Msg = N'*** DATASTORE STOPPED ***'
			EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
			@pnv_Job = @pnv_Job,
			@pnv_PackageExecuting = @pnv_PackageExecuting,
			@pi_Log_Level = @pi_Log_Level,
			@pbi_LogSeq = @pbi_LogSeq OUT
		END
		ELSE IF (@pbit_Enable IS NULL)
		BEGIN
			SET @nv_Msg = N'*** DATASTORE SET TO RESURRECT  ***'
			EXEC csiDataStoreLogMessage 
			@pnv_Msg = @nv_Msg, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
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



