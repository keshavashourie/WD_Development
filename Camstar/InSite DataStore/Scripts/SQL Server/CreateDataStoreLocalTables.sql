------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      CreateDataStoreLocalTables.sql
-- DESCR:       Creates the Datastore tables in the datastore database.  Executes on the ODS
-- HISTORY:
--              05/04/2005                       Added Id column 
--              06/24/2005                       Changed to support Master tables
--              07/19/2005                       Added copy of InSiteSiteInfo to DataStoreSetup. Added support for
--                                               missed transactions. Removed linked server support.
--              08/11/2005                       Added DataStoreSync table.
--              11/08/2005                       Added LOGDATE column to DATASTORERRORS, DATASTOREMISSINGTXNS
--              12/02/2005                       Added VERSION parameter
--              12/28/2005                       Added copy from DataStoreSetup to InSiteSiteInfo. Removed table/trigger
--                                               modifications since replication takes care of creating the tables.
--              03/01/2006                       Added STOP_IF_RETRIES_EXCEEDED parameter
--              08/11/2006                       Changed nvarchar columns to MAX
--              12/06/2006                       Updated/added copyright notice(s) (SPR S9984) Bill Lippard.
--              04/23/2007                       Updated copyright notice(s) (SPR S9984) Bill Lippard.
--              06/06/2016      Dan maloney      Added check to see if DataStoreSetUp exists and set DATASTORE_TERMINATE = Y to stop the DataStore then
--                                               wait for a the amount of time set by INSERT_UPDATE_WAIT_TIME + 3 seconds before continuing the script
--              06/07/2016      Dan Maloney      Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--              06/07/2016      Dan Maloney      Added explicit transactions handling for DML
--              06/07/2016      Dan Maloney      Changed variable naming to prefix with data type.  Example @ErrorStatus INT has become @i_ErrorStatus INT
--              06/07/2016      Dan Maloney      Removed unused variables like @Enabled Varchar(2)
--              06/07/2016      Dan maloney      Restructed code 
--              06/07/2016      Dan Maloney      Added variable @i_WaitTime Int
--              06/07/2016      Dan Maloney      Added variable @i_NumTables Int
--              06/07/2016      Dan Maloney      Added variable @b_Exists Bit
--              06/07/2016      Dan Maloney      Added variable @i_RowCount Int
--              04/20/2017      Dan maloney      Added OOTB email trigger (US 45098)
--              05/05/2017      Dan Maloney      Moved DataStore OOTB Email Trigger to a new script called CreateDataStoreEmailTrigger.sql (US 45098)
--              06/14/2017      Dan Maloney      Added DDL for DataStoreWhiteList table (US 51393)
--              06/14/2017      Dan Maloney      Remove code that updates the DATASTOR_TERMINATE to Y as the stopping and starting of the DataStore is done in Management Studio (US 51393)
--              06/14/2017      Dan Maloney      Remove code to create DataStoreSetUp and populate it with data as this is done in Management Studio (US51393)
--              06/14/2017      Dan Maloney      Remove code to create DataStoreEmailSetUp and populate it with data as this is done in Management Studio (US51393)
--              06/14/2017      Dan Maloney      Remove code to populate InsiteSiteInfo  from DataStoreSetUp for DataStorepresent, DataStoreInsertTables and DataStoreDelimiter (US 51393)
--              08/04/2017      Dan Maloney      Removed variable @i_ErrorStatus (US 51393)
--              10/23/2019      Dan maloney      Modify logic to catch blocks to add THROW and removed insert into DaatStoreSetUp for rows for 
--                                               DataStoreDelimiter, DataStorePresent, DataStoreInsertTables (CPR 1260)
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------

DECLARE @b_Exists       INT
DECLARE @i_NumTables    INT
DECLARE @i_RowCount     INT
DECLARE @i_WaitTime     INT
DECLARE @v_Value        VARCHAR(512)
DECLARE @nv_Msg			NVARCHAR(2048)
DECLARE @nv_Sql 		NVARCHAR(4000)
BEGIN
	SET NOCOUNT ON
  
	-- Check value of DataStoreInsertTables to make sure its > 0
	SELECT @i_NumTables = CONVERT(INT,TValue)
	FROM INSITESITEINFO 
	WHERE TNAME = 'DataStoreInsertTables'
    IF (@i_NumTables <= 0)
	BEGIN
		-- DataStoreInsertTables must be > 0
		SET @nv_Msg = N'InsiteSiteInfo.DataStoreInsertTables must be > 0.  Script CreateDataStoreSynonym.sql exiting.';
		PRINT @nv_Msg
		PRINT ERROR_MESSAGE();
		THROW 51000, @nv_Msg, 1
   	END

	-- Create the LOG table
	IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreLog' AND TYPE='U')
    BEGIN
		--DataStoreLog table exists.  Check to see if it is version SU12.  If it is SU12 or higher it will have a Job column.  If it is before version SU12, drop table, then create newer version of the table
		IF NOT EXISTS (SELECT obj.Name FROM sysobjects obj, syscolumns col WHERE obj.id = col.id AND obj.Name = 'DataStoreLog' AND obj.Type='U' and col.Name = 'Job')
		BEGIN
			BEGIN TRY
         		SET @nv_SQL = N'DROP TABLE DataStoreLog'
				EXEC (@nv_Sql)
       
				SET @b_Exists=0
				PRINT 'Dropped pre SU12 version of DataStoreLog'
			END TRY
			BEGIN CATCH
				-- Unable to drop table
				SET @nv_Msg = N'Unable to drop DataStoreLog table.  Script CreateDataStoreLocalTables.sql exiting.';
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
			END CATCH
		END
		ELSE
			SET @b_Exists=1
	END
	ELSE	
			SET @b_Exists=0

	IF @b_Exists = 0
	BEGIN
		BEGIN TRY
			-- DataStoreLog doesnt exist now, create current version of table
			SET @nv_Sql = N'CREATE TABLE DataStoreLog 
			(Job VARCHAR(50) NOT NULL, 
			Log_Seq BIGINT NOT NULL,
			Log_TimeStamp DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
			Log_Level BIGINT NOT NULL, 
			Package_Executing VARCHAR(128) NOT NULL, 
			Loc VARCHAR(64) NOT NULL, 
			Message VARCHAR(MAX),
			CONSTRAINT PK_DataStoreLog PRIMARY KEY CLUSTERED (Job ASC, Log_Seq, Log_TimeStamp))'
			EXEC (@nv_Sql)

			PRINT 'Created SU12+ version of DataStoreLog table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreLog table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
	END

	-- Create the ERROR table
    IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreErrors' AND Type='U')
    BEGIN
		BEGIN TRY
			-- DataStoreErrors doesnt exist now, create current version of table
			SET @nv_Sql = N'CREATE TABLE DataStoreErrors 
			(SessionName VARCHAR(25), 
			TxnId CHAR(16),  
			Message NVARCHAR(MAX), 
			LogDate DATETIME DEFAULT GETDATE(), 
			SqlStmt NVARCHAR(MAX))'
			EXEC (@nv_Sql)
			PRINT 'Created DataStoreErrors table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreErrors table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
    END

	-- Create the DATASTORESESSIONTRACKING table
	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreSessionTracking' AND Type='U')
    BEGIN
		BEGIN TRY
			-- DataStoreSessionTracking doesnt exist now, create current version of table
         	SET @nv_Sql = N'CREATE TABLE DataStoreSessionTracking 
			(SessionName VARCHAR(25) NOT NULL, 
			ProcessedTxnId CHAR(16), 
			ProcessedId BIGINT, 
			WaitingId BIGINT, 
			Timestamp DATETIME,
			CONSTRAINT PK_DataStoreSessionTracking PRIMARY KEY CLUSTERED (SessionName))'
			EXEC (@nv_Sql)

			PRINT 'Created DataStoreSessionTracking table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreSessionTracking table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
    END

	-- Create the DATASTORESYNC table
    IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreSync' AND Type='U')
    BEGIN
		BEGIN TRY
			-- DataStoreSync doesnt exist now, create current version of table
        	SET @nv_Sql = N'CREATE TABLE DataStoreSync 
			(ProcessedTxnId CHAR(16) NOT NULL,
			CONSTRAINT PK_DataStoreSync PRIMARY KEY CLUSTERED (ProcessedTxnId))'
     		EXEC (@nv_Sql)
			
			PRINT 'Created DataStoreSyncLog table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreSync table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
    END

	-- Create the DATASTOREMISSINGTXNS table
	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreMissingTxns' AND Type='U')
    BEGIN
		BEGIN TRY
			-- DataStoreMissingTxns doesnt exist now, create current version of table
       		SET @nv_Sql = N'CREATE TABLE DataStoreMissingTxns 
			(SessionName VARCHAR(25), 
			MissedId BIGINT,
			Type VARCHAR(25),
			LogDate DATETIME DEFAULT GETDATE())'
			EXEC (@nv_Sql)

			PRINT 'Created DataStoreMissingTxns table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreMissingTxns table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
	END

	IF NOT EXISTS (SELECT Name FROM sysindexes WHERE Name = 'DataStoreMissingTxns1')
    BEGIN
		BEGIN TRY
			-- Index DataStoreMissingTxns1 doesnt exist now, create nonclustered index
 			SET @nv_Sql = N'CREATE NONCLUSTERED INDEX DataStoreMissingTxns1 ON DataStoreMissingTxns (MissedId ASC)'
			EXEC (@nv_Sql)

			PRINT 'Created DataStoreMissingTxns1 nonclustered index'
		END TRY
		BEGIN CATCH
			-- Unable to create nonclustered index
			SET @nv_Msg = N'Unable to create DataStoreMissingTxns1 nonclustered index.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
    END

	-- Create the DATASTOREWHITELIST table
	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'DataStoreWhiteList' AND Type='U')
    BEGIN
		BEGIN TRY
			-- DataStoreWhiteList doesnt exist now, create current version of table
			SET @nv_Sql = N'CREATE TABLE DataStoreWhiteList
			(ErrorId INT NOT NULL, 
			Description NVARCHAR(512),
			CONSTRAINT PK_DataStoreWhiteList PRIMARY KEY CLUSTERED (ErrorId))'
			EXEC (@nv_Sql)

			PRINT 'Created DataStoreWhiteList table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg = N'Unable to create DataStoreWhiteList table.  Script CreateDataStoreLocalTables.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
	END
END;
GO


