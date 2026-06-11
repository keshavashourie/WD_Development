--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Copyright Siemens 2023
-- 
--
--Description:
--	This script is called by DataStoreEmailSetUp.ps1.  It can be run directly from sqlcmd or SQL Server Management Studio but the preferred method of execution is by invoking the DataStoreEmailSetUp.ps1 script
--	This sql script should be executed as the Admin or SA server login.  It creates the Email Profile, Email Account and a relationship between the Email Profile and Email Account.  
--	The DataStore Username is needed to issue a grant to the DatabaseMailUserRole role and to create the profile principal relationship and is passed to this sql script as parameter 1 which is assigned to $DATASTORE_USERNAME.  
--	The Email Server IP address or name is needed to crate the Email Account and is passed to this sql script as parameter 2 which is assigned to $EMAIL_SERVER.  
--	The Email Server should be a SMTP relay server that doesn't require authentication.
--	The Action is passed in as parameter 3 to direct the sql script on whether to ADD or DROP email configurations
--	The Email Server port is passed in as parameter 4 to setup the Datastore email account with provided port.
--
--Execution:
--	Execute script via DataStoreEmailSetUp.ps1 (preferred method).  This sql script can be executed via sqlcmd or SQL Server Management Studio as the SA server login.  Instructions in the comments below
--	explain how to execute this script from sqlcmd command line or from SQL Server Management Studio
--	
--	Directions on how to execute this sql script from SQL Server Management Studio or sqlcmd are in the comments in the script below
--
--
--
--	Note: 		The recommended way to run DataStoreEmailSetUp.sql is by invoking DataStoreEmailSetUp.ps1 (use the batch file)
--			See the comments in the DataStoreEmailSetUp.ps1 to get the syntax to call DataStoreEmailSetUp.ps1 and pass in parameter values
--			The parameter values passed into the batch file, will get passed into the DataStoreEmailSetUp.sql file.  
--
--Input Parameters:
--
--	Parameter 1 which becomes $(DATASTORE_USERNAME)
--	Parameter 2 which becomes $(EMAIL_SERVER)
--	Parameter 3 which becomes $(ACTION)
--  Parameter 4 which becomes $(EMAIL_SERVER_PORT)
--
--
--Output:
--	Log File of script execution:  DataStoreEmailSetUp.or.log 
--
--Modification History:
--	Name				Date		Action
--	--------------------------	----------	----------------
--	dmaloney			04/24/2017	New Script
--	dmaloney			05/04/2017	Add ADD and DROP functionality
--	dmaloney			06/04/2017	Modify comments
--	dmaloney			07/17/2018	Replaced DATASTORE_SERVER_LOGIN with DATASTORE_USERNAME and replaced SYS.SERVER_PRINCIPALS with SYS.DATABASE_PRINCIPALS
--  mbhandari			13/10/2023	Added parameter for Email Server Port as input parameter
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
SET NOCOUNT ON	

--Configure MSDB for Database Emails
USE [msdb]
GO

exec sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'Database Mail XPs', 1;
GO
RECONFIGURE
GO


--{*** 1) Executing this script file via batch file DataStoreEmailSetUp.ps1 ***} 
--	If executing via the DataStoreEmailSetUp.ps1 powershell file, leave the :setvar commands commented out.  If you uncomment the :setvar commands below
--	to execute this sql script, the local :setvar commands below will override the parameter values the powershell file passes in.  This is because the local :setvar commands take 
--	higher precedence over the values passed in from the powershell file.  This is not the desired behavior so it is important to keep the :setvar commands below commented out 
--	when executing this script via the batch file DataStoreEmailSetUp.ps1 

--{*** 2) Executing this script via SQL Server Management Studio in SQLCMD Mode ***}  
--	If executed this way, you must enable SQLCMD mode before executing this script and uncomment the :setvar commands and edit the :setvar variable values to
--	the values you want the stored procedure to accept for DATASTORE_USERNAME, EMAIL_SERVER, ACTION and EMAIL_SERVER_PORT

--{*** 3) Executing this script via SQL Server Management Studio Execution NOT in SQLCMD Mode ***} 
--	If executed this way, leave the :setvar commands commented out and comment replace $(DATASTORE_USERNAME) with the DataStore Database User Name in the MSDB  
--	replace $(ACTION) with ADD or DROP, $(EMAIL_SERVER) with IP address or name of the Email Server and $(EMAIL_SERVER_PORT) with port number of Email Server

--{*** 4) Executing this script via sqlcmd utility using -v to pass in parameter valued ***} 
--	If executing this sql script from a DOS Window using sqlcmd instead of using the powershell file, leave the :setvar commands commented out 
--	Use the -v flag of sqlcmd to specify the DATASTORE_USERNAME, EMAIL_SERVER, ACTION and EMAIL_SERVER_PORT
--	The DataStoreEmailSetUp.ps1 script executes this sql script using this method by using the -v option with sqlcmd
--	EXAMPLE:	
--		sqlcmd -S <Database Server> -d <Database> -U SA -P <SA_Password> -i DataStoreEmailSetUp.sql -v DATASTORE_USERNAME=<[DataStore User Name]> EMAIL_SERVER=<[IP address or Name of Email relay server]>
--		ACTION=<[ADD|DROP]> EMAIL_SERVER_PORT=<[EMAIL SERVER PORT]>

--See DataStoreEmailSetUp.ps1 comments for more information on the parameters.  The preferred for executing this sql script is by executing DataStoreEmailSetUp.ps1

--:setvar DATASTORE_USERNAME 'DataStore Username'
--:setvar EMAIL_SERVER 'Name or IP address of email relay server'
--:setvar ACTION 'ADD or DROP'
--:setvar EMAIL_SERVER_PORT 'Port of email server'

USE MSDB
GO

BEGIN
	DECLARE @nv_Action 		NVARCHAR(4);
	DECLARE @i_ProfileId		INT;
	DECLARE @i_AccountId		INT;
	DECLARE @i_Count		INT;
	DECLARE @i_PrincipalId		INT;
	DECLARE @vb_PrincipalSid	VARBINARY(85);
	DECLARE @i_emailServerPort INT;
	
	SET @nv_Action = '$(ACTION)';
	SET @i_emailServerPort = $(EMAIL_SERVER_PORT);
	PRINT "Email Server Port : " + CAST(@i_emailServerPort as nvarchar(10));
	
	PRINT ""
	PRINT "Action: " + @nv_Action;
	print "Email Server : " + '$(EMAIL_SERVER)'
	
	SET @i_Count = 0;
	SET @i_ProfileId = NULL;
	SET @i_AccountId = NULL;
	SET @i_PrincipalId = NULL;

	SELECT @i_AccountId = ACCOUNT_ID FROM DBO.SYSMAIL_ACCOUNT WHERE NAME = 'DataStore Email Account';
	SELECT @i_ProfileId = PROFILE_ID FROM DBO.SYSMAIL_PROFILE WHERE NAME = 'DataStore Email Profile'; 
	SELECT @i_PrincipalId = PRINCIPAL_ID, @vb_PrincipalSid = SID FROM  SYS.DATABASE_PRINCIPALS WHERE NAME = '$(DATASTORE_USERNAME)';
	SELECT @i_Count = COUNT(*) FROM DBO.SYSMAIL_PRINCIPALPROFILE WHERE PROFILE_ID = @i_ProfileId AND PRINCIPAL_SID = (SELECT SID FROM  SYS.DATABASE_PRINCIPALS WHERE NAME = '$(DATASTORE_USERNAME)');

	IF @nv_Action = "ADD" 
	BEGIN
		ALTER ROLE [DatabaseMailUserRole] ADD MEMBER $(DATASTORE_USERNAME);
		GRANT EXECUTE ON MSDB.DBO.SYSMAIL_UPDATE_ACCOUNT_SP TO $(DATASTORE_USERNAME);
		GRANT EXECUTE ON MSDB.DBO.SP_SEND_DBMAIL TO $(DATASTORE_USERNAME);

		--Add DataStore Email Account if it doesnt already exist
		IF @i_AccountId IS NULL 
		BEGIN
			--Add DataStore Email Account
			BEGIN TRY
				EXEC MSDB.DBO.SYSMAIL_ADD_ACCOUNT_SP 
					@account_name = 'DataStore Email Account', 
					@description = 'DataStore Email Account',
					@display_name = 'DataStore Email Account',
					@email_address = 'undefined@undefined.com',  
	    			@mailserver_name = '$(EMAIL_SERVER)',
					@port = @i_emailServerPort,
					@account_id = @i_AccountId OUTPUT ; 

				PRINT 'DataStore Email Account added ' + CAST(@i_AccountId AS VARCHAR(16));
			END TRY
			BEGIN CATCH
				PRINT 'Msg ' + ERROR_MESSAGE();
			END CATCH;
		END;
		ELSE
			PRINT 'DataStore Email Account exists ' + CAST(@i_AccountId AS VARCHAR(16));	

		--Add DataStore Email Profile if it doesnt already exist
		IF @i_ProfileId IS NULL
		BEGIN
			--Add DataStore Email Profile
			BEGIN TRY
				EXEC MSDB.DBO.SYSMAIL_ADD_PROFILE_SP 
					@profile_name = 'DataStore Email Profile', 
					@description = 'DataStore Email Profile',
					@profile_id = @i_ProfileId OUT;

				PRINT 'DataStore Email Profile added ' + CAST(@i_ProfileId AS VARCHAR(16));
			END TRY
			BEGIN CATCH
				PRINT 'Msg ' + ERROR_MESSAGE();
			END CATCH;
		END;
		ELSE
			PRINT 'DataStore Email Profile exists ' + CAST(@i_ProfileId AS VARCHAR(16));

		
		--Add relationship between DataStore Email Profile and DataStore Email Account. 
		BEGIN TRY
			EXEC MSDB.DBO.SYSMAIL_ADD_PROFILEACCOUNT_SP 
				@profile_id = @i_ProfileId, 
				@account_id = @i_AccountId,
				@sequence_number = 1;
			PRINT 'DataStore Email Profile - DataStore Email Account relationship added';
		END TRY
		BEGIN CATCH
			PRINT 'DataStore Email Profile - DataStore Email Account relationship exists';
		END CATCH;

		--Add principal to DataStore Email Profile
		IF @i_Count = 0
		BEGIN
			--Add principal to DataStore Email Profile
			BEGIN TRY
				EXEC MSDB.DBO.SYSMAIL_ADD_PRINCIPALPROFILE_SP
					@principal_name = '$(DATASTORE_USERNAME)',
					@profile_id = @i_ProfileId,
					@is_default = 0;

				PRINT '$(DATASTORE_USERNAME) added to private DataStore Email Profile';
			END TRY
			BEGIN CATCH
				PRINT 'Msg ' + ERROR_MESSAGE();
			END CATCH;
		END;
		ELSE
			PRINT '$(DATASTORE_USERNAME) already exists for private DataStore Email Profile';
	END;
	ELSE IF @nv_Action = "DROP"
	BEGIN
		ALTER ROLE [DatabaseMailUserRole] DROP MEMBER $(DATASTORE_USERNAME);
		REVOKE EXECUTE ON MSDB.DBO.SYSMAIL_UPDATE_ACCOUNT_SP FROM  $(DATASTORE_USERNAME);
		REVOKE EXECUTE ON MSDB.DBO.SP_SEND_DBMAIL FROM  $(DATASTORE_USERNAME);

		SELECT @i_Count = COUNT(*) FROM DBO.SYSMAIL_PRINCIPALPROFILE WHERE PROFILE_ID = @i_ProfileId;
		IF @i_Count = 0
		BEGIN

			IF @i_ProfileId IS NULL AND @i_AccountId IS NULL
				PRINT 'DataStore Email Profile and DataStore Email Account do not exists.  Execute DataStoreEmailSetUp.bat with ADD action.';
			ELSE
			BEGIN
				--Drop Email Account.  If DataStore Email Account does not already exists, catch error and do nothing
				BEGIN TRY
					EXEC MSDB.DBO.SYSMAIL_DELETE_ACCOUNT_SP 
						@account_name = 'DataStore Email Account';
					PRINT 'DataStore Email Account dropped';
				END TRY
				BEGIN CATCH
					PRINT 'Msg ' + ERROR_MESSAGE();
				END CATCH;

				--Drop Email Profile.  If DataStore Email Profile does not already exists, catch error and do nothing
				BEGIN TRY
					EXEC MSDB.DBO.SYSMAIL_DELETE_PROFILE_SP 
						@profile_name = 'DataStore Email Profile';
					PRINT 'DataStore Email profile dropped';
				END TRY
				BEGIN CATCH
					PRINT 'Msg ' + ERROR_MESSAGE();;
				END CATCH;
			END;
		END;
		ELSE
		BEGIN
			SELECT @i_Count = COUNT(*) FROM DBO.SYSMAIL_PRINCIPALPROFILE WHERE PROFILE_ID = @i_ProfileId AND PRINCIPAL_SID = (SELECT SID FROM  SYS.DATABASE_PRINCIPALS WHERE NAME = '$(DATASTORE_USERNAME)');
			IF @i_Count = 1
			BEGIN
				--Delete princpal from DataStore Email Profile
				BEGIN TRY
					EXEC MSDB.DBO.SYSMAIL_DELETE_PRINCIPALPROFILE_SP
						@principal_id = @i_PrincipalId,
						@profile_id = @i_ProfileId;

					PRINT '$(DATASTORE_USERNAME) removed from private DataStore Email Profile';
				END TRY
				BEGIN CATCH
					PRINT 'Msg ' + ERROR_MESSAGE();
				END CATCH;
			END;
			ELSE
				PRINT '$(DATASTORE_USERNAME) does not exists for private DataStore Email Profile';
		END;
	END;

	PRINT ' ';
	PRINT 'Email Server for private DataStore Email Profile:';
	SELECT ServerType, ServerName, Port, Enable_SSL FROM  DBO.SYSMAIL_SERVER WHERE Account_Id = @i_AccountId; 

	PRINT ' ';
	PRINT 'Principal(s) associated with private DataStore Email Profile:';
	SELECT Name FROM  SYS.DATABASE_PRINCIPALS WHERE Sid IN (SELECT Principal_Sid FROM DBO.SYSMAIL_PRINCIPALPROFILE WHERE Profile_Id = @i_ProfileId )
END;
GO










SET NOCOUNT OFF
