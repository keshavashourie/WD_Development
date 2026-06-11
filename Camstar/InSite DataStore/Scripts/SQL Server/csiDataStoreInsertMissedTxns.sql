------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreInsertMissedTxns.sql
-- DESCR:       Inserts records into the DATASTOREMISSINGTXNS table. Either.  Either a single record or a range.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE  Name = 'csiDataStoreInsertMissedTxns' AND Type = 'P')
    DROP PROCEDURE csiDataStoreInsertMissedTxns
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE csiDataStoreInsertMissedTxns
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name: 	    csiDataStoreInsertMissedTxns
-- Params: 	    <in>  @pnv_TableName		NVARCHAR(50)
--		        <in>  @pnv_Type			    NVARCHAR(25)	MISSING or NO ROWS AFFECTED
--		        <in>  @pbi_StartId			BIGINT		    Starting Txn Id		
--		        <in>  @pbi_LastId			BIGINT = 0	    Ending Txn Id (Ending Txn Id ofr a range or  0 = Single Id)
--		        <in>  @pv_Message		    VARCAHR(MAX)
--		        <in>  @pnv_Job			    NVARCHAR(50)
--		        <in>  @pnv_PackageExecuting	NVARCHAR(128)
--		        <in>  @pi_Log_Level		    INT
--		        <out> @pbi_LogSeq		    BIGINT 		     OUTPUT
--
-- Descr:       Inserts records into the DATASTOREMISSINGTXNS table. Either.  Either a single record or a range.  Executes on the ODS
--
-- HISTORY:
--              07/14/2016      Dan Maloney      New stored procedure to write error to DataStoreErrors. 
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/24/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              01/31/2019      Dan Maloney      Implemented work by Alex Lind in V6 for Bug 14453 ODS: flag STOP_IF_RETRIES_EXCEEDED = 'N' does not work correctly with UNCOMMITTED errors.
--                                               Changed Retries exceeded logic to handle Uncommitted and Missing IDs seperately. 
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
      @pnv_TableName            NVARCHAR(50),
      @pnv_Type                 NVARCHAR(25),
      @pbi_StartId           	BIGINT,
      @pbi_LastId            	BIGINT = 0,
      @pnv_Job                  NVARCHAR(50),
      @pnv_PackageExecuting     NVARCHAR(128),
      @pi_Log_Level             INT,
      @pbi_LogSeq               BIGINT            OUTPUT
)
AS
--Variables defined as constants
DECLARE @BIT_TRUE                     BIT = 1
DECLARE @I_LOG_LEVEL_MAX              INT = 2
DECLARE @I_LOG_LEVEL_MIN              INT = 1
DECLARE @I_LOG_LEVEL_ERROR            INT = 0
DECLARE @I_LOG_LEVEL_WHITELISTED      INT = -1

DECLARE @bit_WhiteListed              BIT = 0
DECLARE @bi_LastTxnId                 BIGINT = @pbi_LastId
DECLARE @bi_ThisTxnId                 BIGINT = @pbi_StartId
DECLARE @i_DynamicMsgLogLevel         INT
DECLARE @i_ErrorNumber	              INT 
DECLARE @i_ErrorSeverity              INT  
DECLARE @i_ErrorState                 INT 
DECLARE	@i_RowsAffected               INT
DECLARE	@nv_Loc                       NVARCHAR(64)
DECLARE	@nv_Err                       NVARCHAR(MAX)
DECLARE @nv_ErrorMessage              NVARCHAR(4000)
DECLARE	@nv_Msg                       NVARCHAR(MAX)  
BEGIN
   	SET NOCOUNT ON
	BEGIN TRY
		SET @nv_Loc = N'csiDataStoreInsertMissingTxns'
		SET @nv_Msg = N'Insert into DataStoreMissingTxns'

		IF @pbi_LastId = 0
		BEGIN
			SET @bi_LastTxnId = @pbi_StartId
			SET @nv_Msg = N'@pbi_LastId = 0.  Setting @bi_LastTxnId to @pbi_StartId [ ' + CONVERT(NVARCHAR, @pbi_StartId) + N' ]'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT
		END

		WHILE (@bi_ThisTxnId <= @bi_LastTxnId)
		BEGIN
			SET @nv_Msg = N'@bi_ThisTxnId [ ' + CONVERT(NVARCHAR, @bi_ThisTxnId) + N' ] @bi_LastTxnId [ ' + CONVERT(NVARCHAR, @bi_LastTxnId) + N' ]'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT

			INSERT 
			INTO DataStoreMissingTxns 
			(SessionName, MissedId, Type)
			VALUES
			(@pnv_TableName, @bi_ThisTxnId, @pnv_Type)

			SET @i_RowsAffected = @@ROWCOUNT

			SET @nv_Msg = N'Rows affected [ ' + CONVERT(NVARCHAR, @i_RowsAffected) + N' ] INSERT INTO DataStoreMissingTxns (SessionName, MissedId, Type) VALUES ( [ '
			+ @pnv_TableName + N' ], [ ' + CONVERT(NVARCHAR, @bi_ThisTxnId) + N' ], [ ' + @pnv_Type + N' ] )'
			EXEC csiDataStoreLogMessage 
				@pnv_Msg = @nv_Msg, 
				@pnv_Loc = @nv_Loc, 
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT

			SET @bi_ThisTxnId = @bi_ThisTxnId + 1
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



