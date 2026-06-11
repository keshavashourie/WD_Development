------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      CreateDataStoreEmailTrigger.sql
-- DESCR:       Creates the DataStore OOTB Email Trigger.  Executes on the ODS
-- HISTORY:
--              05/05/2017      Dan maloney       Creates DataStore OOTB Email Trigger
--              07/27/2017      Dan Maloney       Formatting (US 51393)
--              07/27/2017      Dan Maloney       Removed Unused variables @I_LOG_LEVEL_MAX, @I_LOG_LEVEL_MIN, @i_MailItemId, @bi_EmailAccountId, @bi_EmailProfileId (US 51393)
--              07/27/2017      Dan Maloney       Removed Unused variables @nv_DynLoopBackSendEmailSql,  @nv_DynLoopBackLogSql, @nv_EmailSetUpValue (US 51393)
--              08/31/2022      Dan maloney       Modified trigger so that the trigger only calls csiDataStoreSendEmail if the value of DATASTORE_TERMINATE changes AND the new 
--                                                value of DATASTORE_TERMINATE is 'Y' or 'N'.  The trigger will ignore a value of 'R' (resurrect) (Task 286697)
--				03/01/2024      Madhuri Bhamdari  Modified trigger to check the old and new value of DATABASE_TERMINATE is different and if the value differs then only trigger the email.
--              12/04/2024      Dan Maloney        Removed [CSILOOPBACK] from @nv_DynLoopBackSendEmailSql in trigger
--
-- Copyright Siemens 2024 
------------------------------------------------------------------------------------------------------------------------------------------------------

-- Email trigger
-- Out Of The Box SQL Server DataStore Email Alerts expect an internal SMTP server that does not require authentication.  
-- Anonymous authentication is the Out Of The Box expectation for the SQL Server DataStore Email Account.
-- If your SMTP server requires authentication, you must manually edit the Email Account using sysmail_update_Account_sp or via 
-- SQL Server Managment Studio, Database Mail Configuration Wizard.  Or, set up an SMTP relay that does not require authentication
-- and use the SMTP relay server as the Email Server in Camstar management Studio's DataStore Confiuration Pane.


--Remove old email alert trigger that was defined in the DataStore Reference Guide if it exists
IF EXISTS (SELECT name FROM sysobjects WHERE  name = 'DSalert' AND type = 'TR')
	DROP TRIGGER DSalert
GO

--Check to see if csiDataStoreSendEmail exists, if it exists drop and recreate the procedure
IF EXISTS (SELECT name FROM sysobjects WHERE  name = 'csiDataStoreSendEmail' AND type = 'P')
	DROP PROCEDURE csiDataStoreSendEmail;
GO


CREATE PROCEDURE csiDataStoreSendEmail
(
      @pnv_DataStoreTerminate                           NVARCHAR(256),
      @pnv_DataStoreDB                                  NVARCHAR(256),
      @pnv_DataStoreSchema                              NVARCHAR(256)
)
AS
     --Variables defined as constants
      DECLARE @I_LOG_LEVEL_ERROR			INT = 0;

      DECLARE @i_ErrorNumber				INT ;
      DECLARE @i_ErrorSeverity                          INT;  
      DECLARE @i_ErrorState                             INT;
      DECLARE @i_ReturnCode                             INT;
      DECLARE @bi_LogSeq                                BIGINT;
      DECLARE @cCRLF                                    CHAR(2);
      DECLARE @nv_CSS                                   NVARCHAR(2048);
      DECLARE @nv_DBName                                NVARCHAR(256);
      DECLARE @nv_DynSqlParmDef                         NVARCHAR(256);
      DECLARE @nv_DBDataBaseHostName                    NVARCHAR(256);
      DECLARE @nv_EmailBody                             NVARCHAR(MAX);
      DECLARE @nv_EmailProfileName                      NVARCHAR(256);
      DECLARE @nv_EmailAccountName                      NVARCHAR(256);
      DECLARE @nv_EmailRecipients                       NVARCHAR(4000);					
      DECLARE @nv_EmailSender                           NVARCHAR(256);
      DECLARE @nv_EmailSetUpParameter                   NVARCHAR(256);
      DECLARE @nv_Err                                   NVARCHAR(MAX);
      DECLARE @nv_ErrorMessage                          NVARCHAR(4000);
      DECLARE @nv_Loc	                                NVARCHAR(256);
      DECLARE @nv_Msg                                   NVARCHAR(MAX);
      DECLARE @nv_ServerHost                            NVARCHAR(256);
      DECLARE @nv_OriginalLogin                         NVARCHAR(256);
      DECLARE @nv_SchemaName                            NVARCHAR(256);
      DECLARE @nv_Subject                               NVARCHAR(1024);
      DECLARE @nv_UserName                              NVARCHAR(256);
      DECLARE @nv_SQL                                   NVARCHAR(1024);
	BEGIN
		SET XACT_ABORT OFF

		BEGIN TRY
			SET NOCOUNT ON;
			SET @i_ReturnCode = 0;
			SET @bi_LogSeq = 0;
			SET @cCRLF = CHAR(13) + CHAR(10); 
			SET @nv_DBName = DB_NAME();
			SET @nv_SchemaName = SCHEMA_NAME()
			SET @nv_UserName = USER_NAME();
			SET @nv_OriginalLogin = ORIGINAL_LOGIN();
			SET @nv_EmailProfileName = N'DataStore Email Profile';
			SET @nv_EmailAccountName = N'DataStore Email Account';
			SELECT @nv_DBDataBaseHostName = DataBaseHostName FROM dbdatasourcenames WITH (NOLOCK)  WHERE DataSourceNameId = 2;
			SELECT  @nv_ServerHost = CAST( SERVERPROPERTY('SERVERNAME') AS NVARCHAR(256) );

			IF CHARINDEX(@nv_DBDataBaseHostName, @nv_ServerHost ) = 0 
				SET @nv_ServerHost  = '<' + @nv_DBDataBaseHostName +  '>' +@nv_ServerHost

			-- Get Email Config Info from DataStoreEmailSetUp table
			SET @nv_Msg = N'Get Email Config info from DataStoreEmailSetUp table';
			SET @nv_SQL = N'SELECT @pnv_EmailSetUpValue  = VALUE FROM ' + @pnv_DataStoreDB + '.' + @pnv_DataStoreSchema + '.' + 'DATASTOREEMAILSETUP WHERE PARAMETER = @pnv_EmailSetUpParameter';
			SET @nv_DynSqlParmDef =  N'@pnv_EmailSetUpParameter NVARCHAR(256), @pnv_EmailSetUpValue NVARCHAR(256) OUTPUT'

			SET @nv_EmailSetUpParameter = N'EMAIL_RECIPIENTS';
			EXECUTE sp_executesql 
			@nv_SQL,
			@nv_DynSqlParmDef,
			@pnv_EmailSetUpParameter = @nv_EmailSetUpParameter, 
			@pnv_EmailSetUpValue = @nv_EmailRecipients OUTPUT;

			SET @nv_EmailSetUpParameter = N'EMAIL_SENDER';
			EXECUTE sp_executesql 
			@nv_SQL, 
			@nv_DynSqlParmDef,
			@pnv_EmailSetUpParameter = @nv_EmailSetUpParameter, 
			@pnv_EmailSetUpValue = @nv_EmailSender OUTPUT;
				
			--Validate Email Server, Sender and Recipients are populated.  If not, exit without attempting to send an email
			SET @nv_Msg = N'Validate email server, email sender and email recipients';
			IF LEN(@nv_EmailSender) = 0 OR LEN (@nv_EmailRecipients) = 0 
				RETURN;

			-- Update Email Account
			SET @nv_Msg = N'Update Email Account [ ' + @nv_EmailAccountName + ' ]';
			EXEC @i_ReturnCode = msdb.dbo.sysmail_update_account_sp  
			@account_name = @nv_EmailAccountName, 
			@email_address = @nv_EmailSender,  
			@replyto_address = @nv_EmailSender;

			SET @nv_CSS =
			N'<style type="text/css">
			h3 {
			font-family: "Lucida Grande", Tahoma;
			font-size: 26px;
 			font-weight: 500;
		 	font-variant: normal;
 			color: #ffa500;
		        margin-top: 1px;
	 		text-align: center!important;
	 		letter-spacing: 1px;
			text-align: left;
			}
			h1 {
			font-family: "Lucida Grande", Tahoma;
			font-size: 16px;
			font-weight: 200;
			font-variant: normal;
			color: #8B8B83;
		       	margin-top: 1px;
			ext-align: center!important;
			letter-spacing: 1px;
			text-align: justify;
			}
			#box-table
			{
			font-family: "Lucida Grande", Tahoma;
			font-size: 14px;
			text-align: justify;
			border-collapse: collapse;
			border-top: 7px solid #c0c0c0;
			border-bottom: 7px solid #c0c0c0c;
			vertical-align: text-top;
			max-width:1200px; min-width:1200px;
			}
			#box-table th
			{
			font-size: 16px;
			font-weight: normal;
			background: #caff70;
			color: #000000;
			border-right: 2px solid #c0c0c0;
			border-left: 2px solid #c0c0c0;
			border-bottom: 2px solid #c0c0c0;
			text-align: center;
			vertical-align: middle;
			}
			#box-table td
			{
			font-size: 12px;
			border-right: 1px solid #c0c0c0;
			border-left: 1px solid #c0c0c0;
			border-bottom: 1px solid #c0c0c0;
			color: #000000;
			text-align: left;
			vertical-align: text-top;
			}
			</style>';

			IF @pnv_DataStoreTerminate = 'Y' 
			BEGIN
				SET @nv_Msg = N'DataStore has stopped.  Preparing to send email.';
				SET @nv_Subject = N'DataStore Database has stopped';

				SET @nv_EmailBody = @nv_CSS +
				N'<h3>The DataStore Database has stopped</h3>' +
				N'<table style="border-collapse: collapse;">' +
				N'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Host:</td>' +
				N'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' + @nv_ServerHost + N'</td></tr>' +
				N'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left;">Database:</td>' +
				N'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 200px;">' + @nv_DBName + N'</td></tr><tr style="height: 40px;"></tr>' +
				N'</table>' +
				N'<p></p>' +
				N'<h3 style="color: #000000; font-size: 20px; font-weight: 200;">Check the DataStoreErrors and DataStoreLog tables in the DataStore database for details.</h3>';
	

			END;
			ELSE
				IF @pnv_DataStoreTerminate = 'N'
				BEGIN

					SET @nv_Msg = N'DataStore has started.  Preparing to send email.';
					SET @nv_Subject = N'DataStore Database has started';

					SET @nv_EmailBody = @nv_CSS +
					N'<h3 style="color: #32cd32;">The DataStore Database has started</h3>' +
					N'<table style="border-collapse: collapse;">' +
					N'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Host:</td>' +
					N'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' + @nv_ServerHost + N'</td></tr>' +
					N'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left;">Database:</td>' +
					N'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 200px;">' + @nv_DBName + N'</td></tr><tr style="height: 60px;"></tr>' +
					N'</table>';

				END;

			EXEC @i_ReturnCode = msdb.dbo.sp_send_dbmail
			@profile_name= @nv_EmailProfileName,
			@recipients = @nv_EmailRecipients,
			@from_address = @nv_EmailSender,
			@reply_to = @nv_EmailSender,
			@subject = @nv_Subject,
			@body = @nv_EmailBody,
			@body_format ='HTML',
			@importance = 'High';

		END TRY
		BEGIN CATCH
			SELECT   
			@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line :' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),  
	       		@i_ErrorSeverity = ERROR_SEVERITY(),  
			@i_ErrorState = ERROR_STATE(),
			@i_ErrorNumber = ERROR_NUMBER();

			IF @pnv_DataStoreTerminate = 'Y'
				SET @nv_Loc = N'csiDataStoreSendEmail <Stop>';

			IF @pnv_DataStoreTerminate = 'N'
				SET @nv_Loc = N'csiDataStoreSendEmail <Start>';


			SET @nv_SQL = N'EXEC [CSILOOPBACK].[' + @pnv_DataStoreDB + '].[' +  @pnv_DataStoreSchema + '].'
			+ N'csiDataStoreLogMessage @pnv_Msg = @pnv_Msg, @pnv_Loc = @pnv_Loc, @pi_MsgLogLevel = @pi_MsgLogLevel, '
			+ N'@pnv_Job = @pnv_Job, @pnv_PackageExecuting = @pnv_PackageExecuting, @pi_Log_Level = @pi_Log_Level, @pbi_LogSeq = @pbi_LogSeq OUTPUT';

			SET @nv_DynSqlParmDef =  N'@pnv_Msg NVARCHAR(MAX), @pnv_Loc NVARCHAR(64), @pi_MsgLogLevel INT, @pnv_Job NVARCHAR(50), @pnv_PackageExecuting NVARCHAR(128), @pi_Log_Level INT, @pbi_LogSeq BIGINT OUTPUT';

			SET @nv_Err = N'Exception Handler (procedure catch block) : Last message set : ' + @nv_Msg;
			EXECUTE sp_executesql 
			@nv_SQL,
			@nv_DynSqlParmDef,
			@pnv_Msg = @nv_Err, 
			@pnv_Loc =  @nv_Loc,
			@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
			@pnv_Job = 0,
			@pnv_PackageExecuting = N'N/A',
			@pi_Log_Level = 0,
			@pbi_LogSeq =  @bi_LogSeq OUT;

			SET @nv_Err = N'Exception Handler (procedure catch block) : ' + SUBSTRING(@nv_ErrorMessage,1,1024);
			EXECUTE sp_executesql 
			@nv_SQL,
			@nv_DynSqlParmDef,
			@pnv_Msg = @nv_Err, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
			@pnv_Job = 0,
			@pnv_PackageExecuting = N'N/A',
			@pi_Log_Level = 0,
			@pbi_LogSeq =  @bi_LogSeq OUT;

			SET @nv_Err = N'Exception Handler (procedure catch block) : The SQL Server DataStore email profile, DataStore email account or SQL Server login principal association to the DataStore email profile may not be set up correctly.  '
			+ N'See DataStore Reference Guide for information on configuring email alerts.';
			EXECUTE sp_executesql 
			@nv_SQL,
			@nv_DynSqlParmDef,
			@pnv_Msg = @nv_Err, 
			@pnv_Loc = @nv_Loc, 
			@pi_MsgLogLevel = @I_LOG_LEVEL_ERROR,
			@pnv_Job = 0,
			@pnv_PackageExecuting = N'N/A',
			@pi_Log_Level = 0,
			@pbi_LogSeq =  @bi_LogSeq OUT;

		END CATCH;
	END;
GO






--Check to see if DataStoreTrg exists, if it exists drop and recreate the trigger
IF EXISTS (SELECT name FROM sysobjects WHERE  name = 'DataStoreEmailTrg' AND type = 'TR')
	DROP TRIGGER DataStoreEmailTrg;
GO

CREATE TRIGGER DataStoreEmailTrg
ON DataStoreSetUp
FOR UPDATE
NOT FOR REPLICATION
AS
DECLARE @nv_DynLoopBackSendEmailSql             NVARCHAR(4000);
DECLARE @nv_NewParameter                        NVARCHAR(256);
DECLARE @nv_NewValue                            NVARCHAR(256);
DECLARE @nv_OldValue                            NVARCHAR(256);
DECLARE @nv_DataStoreDB	                        NVARCHAR(256);
DECLARE @nv_DataStoreSchema                     NVARCHAR(256);
IF UPDATE(VALUE)
BEGIN TRY
	SET NOCOUNT ON;
	SET XACT_ABORT  OFF
	SET @nv_DataStoreDB = DB_NAME();
	SET @nv_DataStoreSchema = SCHEMA_NAME();
	SELECT @nv_OldValue = UPPER(Value) FROM DELETED;
	SELECT @nv_NewValue = UPPER(Value), @nv_NewParameter = UPPER(PARAMETER) FROM INSERTED;
	IF @nv_NewParameter = N'DATASTORE_TERMINATE'
	BEGIN
		IF ( @nv_NewValue IN ('Y','N') AND  @nv_OldValue IN ('Y','N') AND @nv_NewValue <> @nv_OldValue) OR  ( @nv_NewValue = 'Y' AND  @nv_OldValue = 'R' )
		BEGIN	
			SET @nv_DynLoopBackSendEmailSql = N'EXEC [' + DB_NAME() + '].[' +  SCHEMA_NAME() + '].'
			+ 'csiDataStoreSendEmail @pnv_DataStoreTerminate = @pnv_DataStoreTerminate, @pnv_DataStoreDB =  @pnv_DataStoreDB, @pnv_DataStoreSchema = @pnv_DataStoreSchema';

			EXEC SP_EXECUTESQL @nv_DynLoopBackSendEmailSql, 
			N'@pnv_DataStoreTerminate NVARCHAR(256), @pnv_DataStoreDB NVARCHAR(256), @pnv_DataStoreSchema NVARCHAR(256)', 
			@nv_NewValue, @nv_DataStoreDB, @nv_DataStoreSchema;
		END;
	END;
END TRY
BEGIN CATCH
END CATCH
GO







