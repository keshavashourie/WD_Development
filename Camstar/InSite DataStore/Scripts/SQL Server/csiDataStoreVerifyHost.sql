------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreVerifyHost.sql
-- DESCR:       Function to verify the current dataabse name and schema to what's stored in the DBDataSourceNames table on the OLTP server.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreVerifyHost' AND Type = 'P')
	DROP PROCEDURE csiDataStoreVerifyHost
GO

CREATE PROCEDURE csiDataStoreVerifyHost 
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreVerifyHost
-- Params:      <in>  @pc_Veify_Host            CHAR(1) 'Y' means that a verification should be done.  'N' means skip verification of host on ODS to OLTP
--              <in>  @pnv_TableName			NVARCHAR(64)
--              <in>  @pnv_Job                  NVARCHAR(50)
--              <in>  @pnv_PackageExecuting     NVARCHAR(128)
--              <in>  @pi_Log_Level             INT
--              <out> @pbi_LogSeq               BIGINT          OUTPUT
-- Descr:       Function to verify the current dataabase name and schema to what's stored in the DBDataSourceNames table on the OLTP server.  Executes on the ODS	
--		
 
-- HISTORY:
--              06/25/2016      Dan Maloney      New function
--              07/24/2017      Dan Maloney      Added @b_WhiteListed, @i_DynamicMsgLogLevel, @BIT_TRUE, @I_LOG_LEVEL_WHITELISTED , @nv_Err variables (US 51393)
--              07/24/2017      Dan Maloney      Added WhiteList logic to exception handler (US 51393)
--              07/24/2017      Dan Maloney      Added THROW to catch block (US 51393)
--              07/24/2017      Dan Maloney      Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/26/2017      Dan Maloney      Added logging to catch block of procedure (US 51393)
--              06/13/2018      Dan Maloney      Modifications to Verify_Host for US 4122 to better detect if the stored OLTP/ODS pair
--                                               metadata matches the executing environment for the OLTP/ODS pair.
--                                               A mismatch may result from restoring the ODS to another server, database, schema and
--                                               and failing to run a DB Update in CEP Management Studio prior to starting the ODS
--				12/06/2018		Alex Lind		simplify validation to host and database names.  (US 17618).  
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------

( 
      @pc_Verify_Host                           CHAR(1), 
	  @pnv_TableName							NVARCHAR(64), 
      @pnv_Job                                  NVARCHAR(50), 
      @pnv_PackageExecuting                     NVARCHAR(128), 
      @pi_Log_Level                             INT, 
      @pbi_LogSeq                               BIGINT             OUTPUT 
) 
AS 
-- Variables defined as constants
DECLARE @BIT_TRUE                               BIT = 1 
DECLARE @I_LOG_LEVEL_MAX                        INT = 2 
DECLARE @I_LOG_LEVEL_ERROR                      INT = 0 
DECLARE @I_LOG_LEVEL_WHITELISTED                INT = -1  
DECLARE @bit_WhiteListed                        BIT = 0 
DECLARE @i_DataSourceNameId 					INT = 2  -- ODS is source 2

DECLARE @i_DynamicMsgLogLevel                   INT = @I_LOG_LEVEL_ERROR
DECLARE @i_ErrorNumber                          INT   
DECLARE @nv_ErrorMessage                        NVARCHAR(4000) 
DECLARE @nv_Loc                                 NVARCHAR(64)  
DECLARE	@nv_Msg                                 NVARCHAR(MAX)   
DECLARE @nv_DBName      						NVARCHAR(128) 
DECLARE @nv_ServerName                          NVARCHAR(256) 
DECLARE @nv_DatabaseHostName                   	NVARCHAR(256) 

BEGIN 
	BEGIN TRY 
		SET @nv_Loc = N'csiDataStoreVerifyHost' 
		IF @pc_Verify_Host = 'N' 
		BEGIN 
			SET @nv_Msg =  'Verify DB Host is No. Validation of OLTP/ODS pairing will NOT be performed.' 
			EXEC csiDataStoreLogMessage  
				@pnv_Msg = @nv_Msg,  
				@pnv_Loc = @nv_Loc,  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX, 
				@pnv_Job = @pnv_Job, 
				@pnv_PackageExecuting = @pnv_PackageExecuting, 
				@pi_Log_Level = @pi_Log_Level, 
				@pbi_LogSeq = @pbi_LogSeq OUT 
			RETURN 
		END 
		ELSE 
		BEGIN 
			SET @nv_Msg = 'Verify DB Host is Yes. Validation of OLTP/ODS pairing will be performed.' 
			EXEC csiDataStoreLogMessage  
				@pnv_Msg = @nv_Msg,  
				@pnv_Loc = @nv_Loc,  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX, 
				@pnv_Job = @pnv_Job, 
				@pnv_PackageExecuting = @pnv_PackageExecuting, 
				@pi_Log_Level = @pi_Log_Level, 
				@pbi_LogSeq = @pbi_LogSeq OUT 
		END 
		
		-- Compare OLTP/ODS Info
		SET @nv_Msg = 'Get the ODS metadata (DataBaseHostName, DatabaseName) stored in the local DBDataSourceNames table' 
		SELECT @nv_DBName=databasename, @nv_DatabaseHostName  = databasehostname, @nv_ServerName= host FROM (SELECT databasename, databasehostname, CASE WHEN CHARINDEX('.', databasehostname)>0 -- is host fqdn?
												THEN  --yes strip out domain data
													CONCAT(SUBSTRING(databasehostname,0, CHARINDEX('.', databasehostname) ),-- get just host name
													 SUBSTRING(databasehostname,CHARINDEX('\',databasehostname), CHARINDEX('\', REVERSE(databasehostname))))  --add instance name if present
												ELSE databasehostname 
												END AS host 
						FROM OLTP_DBDataSourceNames WHERE DataSourceNameID= @i_DataSourceNameID) DUAL
						
		

		SET @nv_Msg = 'ODS metadata retrieved from OLTP across link server : '
		+ 'DatabaseHostName [ ' + @nv_DatabaseHostName + ' ]  '
		+ 'DatabaseName [ ' + @nv_DBName + ' ]  ';
		EXEC csiDataStoreLogMessage  
				@pnv_Msg = @nv_Msg,  
				@pnv_Loc = @nv_Loc,  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX, 
				@pnv_Job = @pnv_Job, 
				@pnv_PackageExecuting = @pnv_PackageExecuting, 
				@pi_Log_Level = @pi_Log_Level, 
				@pbi_LogSeq = @pbi_LogSeq OUT
			
		SET @nv_Msg = N'Executing ODS : '
		+ 'Executing ODS @@SERVERNAME [ ' + @@SERVERNAME + ' ]  '
		+ 'Executing ODS Database Name[ ' + DB_NAME() + ' ]  ';			
		EXEC csiDataStoreLogMessage  
				@pnv_Msg = @nv_Msg,  
				@pnv_Loc = @nv_Loc,  
				@pi_MsgLogLevel = @I_LOG_LEVEL_MAX, 
				@pnv_Job = @pnv_Job, 
				@pnv_PackageExecuting = @pnv_PackageExecuting, 
				@pi_Log_Level = @pi_Log_Level, 
				@pbi_LogSeq = @pbi_LogSeq OUT

		--NOTE: Microsoft allows implicit login when default instance is named, but includes name in @@SERVERNAME.  Strip off name from servername if location of \ doesn't match 

		IF (@nv_DBName<>DB_NAME() OR (CASE WHEN CHARINDEX('\',@@SERVERNAME) <> CHARINDEX ('\',@nv_ServerName)
									THEN SUBSTRING(@@SERVERNAME,0,CHARINDEX ('\',@@SERVERNAME)) 
									ELSE @@SERVERNAME END <> @nv_ServerName))
		BEGIN 
			-- If there are any mismatches, raise error condition eVerifyHostFailure
			SET @nv_Msg = N'eVerifyHostFailure'; 
			THROW 50600, @nv_Msg, 1 
		END 
	END TRY 
	BEGIN CATCH 
		SELECT    
		@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line : ' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),      
		@i_ErrorNumber = ERROR_NUMBER() 

		IF @i_ErrorNumber  = 50600 --eVerifyHostFailure
		BEGIN 
			SET @nv_Msg = N'Exception Handler (eVerifyHostFailure) : OLTP expects ODS [' + @nv_DatabaseHostName  +N'].['+@nv_DBName + N'], ODS reports as ['+ @@SERVERNAME + N'].['+DB_NAME()+N'].   NOTE: ODS does not support IP addresses if Verify DB Host is "Yes".'; 
		END 
		ELSE -- Not eVerifyHostFailure, but some other error occurred
		BEGIN 
			EXEC @bit_WhiteListed = csiDataStoreIsErrorWhiteListed 
											@pn_ErrorNumber	 = @i_ErrorNumber 
			IF @bit_WhiteListed = @BIT_TRUE 
				SET @i_DynamicMsgLogLevel = @I_LOG_LEVEL_WHITELISTED 
			ELSE 			 
			SET @nv_Msg = N'Exception Handler (procedure catch block) : ' + @nv_ErrorMessage  
		END

		EXEC csiDataStoreLogMessage  
				@pnv_Msg = @nv_Msg,  
				@pnv_Loc = @nv_Loc,  
				@pi_MsgLogLevel = @i_DynamicMsgLogLevel, 
				@pnv_Job = @pnv_Job, 
				@pnv_PackageExecuting = @pnv_PackageExecuting, 
				@pi_Log_Level = @pi_Log_Level, 
				@pbi_LogSeq = @pbi_LogSeq OUT; 

		EXEC csiDataStoreLogError
				@pnv_TableName = @pnv_TableName,
				@pc_TxnId = N'N/A',
				@pnv_SQLStmt = N'N/A',
				@pnv_Err = @nv_Msg,
				@pnv_Job = @pnv_Job,
				@pnv_PackageExecuting = @pnv_PackageExecuting,
				@pi_Log_Level = @pi_Log_Level,
				@pbi_LogSeq = @pbi_LogSeq OUT;
		THROW 

	END CATCH 
END
