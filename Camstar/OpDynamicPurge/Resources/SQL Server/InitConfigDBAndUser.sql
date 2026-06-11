DECLARE @TransactionDBName NVARCHAR(256);
DECLARE @TransactionDBSchema NVARCHAR(256);
DECLARE @ConfigDBLoginUser NVARCHAR(256);
DECLARE @ConfigDBLoginPassword NVARCHAR(256);
DECLARE @ConfigDBUser NVARCHAR(256);
DECLARE @ArchiveDBName NVARCHAR(256);
DECLARE @ArchiveDBSchema NVARCHAR(256);

DECLARE @tSQL NVARCHAR(1000);

SET @TransactionDBName = 'OPCORE2310';
SET @TransactionDBSchema = 'opcoreschema';
SET @ConfigDBLoginUser = 'DPTUser';
SET @ConfigDBLoginPassword = 'DPT1User';
SET @ConfigDBUser = 'DPTUser';
SET @ArchiveDBName = 'DPTArcDB';
SET @ArchiveDBSchema = 'DPTArcSCH';


-- Note: The Database Name (DPT) must not be changed!
PRINT 'Creating ConfigDB Database(DPT)...';
IF EXISTS (SELECT 1 FROM SYS.DATABASES WHERE NAME = 'DPT')
BEGIN
	PRINT 'ConfigDB Database(DPT) exists. Dropping the existing ConfigDB Database...';
	SET @tSQL = N'DROP DATABASE DPT';
	EXECUTE sp_executesql @tSQL;
	PRINT 'ConfigDB Database(DPT) is dropped.';
END;
SET @tSQL = N'CREATE DATABASE DPT';
EXECUTE sp_executesql @tSQL;
PRINT 'ConfigDB Database(DPT) creation is completed.';
PRINT '';

--
PRINT 'Creating ConfigDB Login User...';
IF EXISTS (SELECT 1 FROM SYS.syslogins WHERE NAME = @ConfigDBLoginUser)
BEGIN
	PRINT 'ConfigDB Login User exists. Dropping the existing login user...';
	SET @tSQL = N'USE [DPT];DROP LOGIN ' + @ConfigDBLoginUser;
	EXECUTE sp_executesql @tSQL;
	PRINT 'ConfigDB Login User is dropped.';
END;
SET @tSQL = 'USE [DPT];CREATE LOGIN [' + @ConfigDBLoginUser + N'] WITH PASSWORD=''' + @ConfigDBLoginPassword + N''', CHECK_EXPIRATION=OFF';
EXECUTE sp_executesql @tSQL;
PRINT 'Set ConfigDB Login User''s default database to ConfigDB Database(DPT).';
EXECUTE sp_defaultdb @loginame=@ConfigDBLoginUser, @defdb='DPT';
PRINT 'ConfigDB Login User created.';
PRINT '';

-- Note: The ConfigDB Database User will map to the ConfigDB Login User
PRINT 'Creating ConfigDB Database User...';
SET @tSQL = N'USE [DPT];CREATE USER [' + @ConfigDBUser + N'] FOR LOGIN [' + @ConfigDBLoginUser + N']';
EXECUTE sp_executesql @tSQL;
PRINT 'ConfigDB Database User created.';
PRINT '';

--
PRINT 'Creating ArchiveDB Database...';
IF EXISTS (SELECT 1 FROM SYS.DATABASES WHERE NAME = @ArchiveDBName)
BEGIN
	PRINT 'ArchiveDB Database exists. Dropping the existing ArchiveDB Database...';
	SET @tSQL = N'DROP DATABASE ' + @ArchiveDBName;
	EXECUTE sp_executesql @tSQL;
	PRINT 'ArchiveDB Database is dropped.';
END;
SET @tSQL = N'CREATE DATABASE ' + @ArchiveDBName;
EXECUTE sp_executesql @tSQL;
PRINT 'ArchiveDB Database creation is completed.';
PRINT '';
	
-- Note: The ArchiveDB Database User will map to the ConfigDB Login User
PRINT 'Creating ArchiveDB Database User...';
SET @tSQL = N'USE ' + @ArchiveDBName + N';CREATE USER [' + @ConfigDBUser + N'] FOR LOGIN [' + @ConfigDBLoginUser + N']';
EXECUTE sp_executesql @tSQL;
PRINT 'ArchiveDB Database User created.';
PRINT '';

--
PRINT 'Creating Transaction Database User...';
SET @tSQL = N'USE [' + @TransactionDBName + N'];
	IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE NAME = ''' + @ConfigDBUser + N''')
	BEGIN
		CREATE USER [' + @ConfigDBUser + N'] FOR LOGIN [' + @ConfigDBLoginUser + N'];
	END;';
EXECUTE sp_executesql @tSQL;
PRINT 'Transaction Database User created.';
PRINT '';

-- Grant read and write permission to Transaction Database User
PRINT 'Granting Permissions to Transaction Database User...';
SET @tSQL = N'USE ' + @TransactionDBName + N'; EXEC sp_addrolemember ''db_datareader'', ' + @ConfigDBUser + N'; EXEC sp_addrolemember ''db_datawriter'', ' + @ConfigDBUser;
EXECUTE sp_executesql @tSQL;
PRINT 'Transaction Database User Permission granted.';
PRINT '';

--
PRINT 'Creating ArchiveDB Schema...';
SET @tSQL = N'USE ' + @ArchiveDBName + N'; EXEC(''CREATE SCHEMA [' + @ArchiveDBSchema + N'] AUTHORIZATION [' + @ConfigDBUser + N']'');';
EXECUTE sp_executesql @tSQL;
PRINT 'Changing ArchiveDB Database User default schema to ArchiveDB Schema...';
SET @tSQL = N'USE ' + @ArchiveDBName + N';ALTER USER [' + @ConfigDBUser + N'] WITH DEFAULT_SCHEMA=[' + @ArchiveDBSchema + N']';
EXECUTE sp_executesql @tSQL;
PRINT 'ArchiveDB Schema created.';
PRINT '';

-- Grant create database permisison to ConfigDB Login User
PRINT 'Granting Permissions to ConfigDB Login User...';
SET @tSQL = N'USE [master]; GRANT CREATE ANY DATABASE TO [' + @ConfigDBLoginUser + N']';
EXECUTE sp_executesql @tSQL;
PRINT 'Permissions to ConfigDB Login User granted.';
PRINT '';

--
PRINT 'Granting ConfigDB Database User Permissions...';
SET @tSQL = N'USE [DPT];' + N'
GRANT ALTER, CREATE FUNCTION, CREATE PROCEDURE, CREATE TABLE, CREATE TYPE, REFERENCES, CREATE VIEW TO [' + @ConfigDBUser + '];
GRANT DELETE, EXECUTE, INSERT, SELECT, UPDATE TO [' + @ConfigDBUser + '];
';
EXEC sp_executesql @tSQL;
PRINT 'Done Granting ConfigDB Database User Permissions...';

--
PRINT 'Granting ArchiveDB Database User Permissions...';
SET @tSQL = N'USE ' + QUOTENAME(@ArchiveDBName) + N';
GRANT ALTER, CREATE TABLE TO [' + @ConfigDBUser + '];
GRANT EXECUTE, INSERT, SELECT TO [' + @ConfigDBUser + '];
GRANT CREATE SCHEMA TO [' + @ConfigDBUser + ']
';
EXEC sp_executesql @tSQL;
PRINT 'Done Granting ArchiveDB Database User Permissions...';

-- Note: The msdb Database User will map to the ConfigDB Login User
PRINT 'Creating msdb Database User...';
SET @tSQL = N'USE msdb;CREATE USER [' + @ConfigDBUser + N'] FOR LOGIN [' + @ConfigDBLoginUser + N']';
EXECUTE sp_executesql @tSQL;
PRINT 'msdb Database User created.';
PRINT '';
PRINT 'Granting database role membership for msdb Database User...';
SET @tSQL = N'USE msdb; EXEC sp_addrolemember ''db_datareader'', ' + @ConfigDBUser + N'; EXEC sp_addrolemember ''SQLAgentUserRole'', ' + @ConfigDBUser;
EXECUTE sp_executesql @tSQL;
PRINT 'Granted database role membership for msdb Database User...';
PRINT '';	

--
PRINT 'Altering the Archive Database Collation to be the same as the Transaction Database Collation...';
DECLARE @vTransactionDatabaseCollation AS SYSNAME;
DECLARE @vSQLAlterDatabaseCollation AS NVARCHAR(4000);
SELECT @vTransactionDatabaseCollation = collation_name 
FROM sys.databases WHERE name = @TransactionDBName;
PRINT 'The Source Database Collation is ' + @vTransactionDatabaseCollation;
SET @vSQLAlterDatabaseCollation = 'ALTER DATABASE ' + @ArchiveDBName + ' COLLATE ' + @vTransactionDatabaseCollation; 
EXECUTE sp_executesql @vSQLAlterDatabaseCollation;
PRINT 'Altering the Archive Database Collation to ' + @vTransactionDatabaseCollation + ' completed.';
PRINT '';

--
PRINT 'Altering the Config Database Collation to be the same as the Transaction Database Collation...';
PRINT 'The Source Database Collation is ' + @vTransactionDatabaseCollation;
SET @vSQLAlterDatabaseCollation = 'ALTER DATABASE DPT COLLATE ' + @vTransactionDatabaseCollation; 
EXECUTE sp_executesql @vSQLAlterDatabaseCollation;
PRINT 'Altering the Config Database Collation to ' + @vTransactionDatabaseCollation + ' completed.';
PRINT '';

-- Creating Transaction DB Index
PRINT 'Creating Container table index...';
SET @tSQL = N'USE [' + @TransactionDBName + N'];
	IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = ''ContainerBySplitFromId'' AND object_id = OBJECT_ID(''' + @TransactionDBSchema + N'.Container''))
	BEGIN
		PRINT ''Index ContainerBySplitFromId exists'';
	END
	ELSE
	BEGIN
		CREATE NONCLUSTERED INDEX [ContainerBySplitFromId] ON [' + @TransactionDBSchema + N'].[Container] ([SplitFromId] ASC);
		PRINT ''Index ContainerBySplitFromId created'';
	END;';
EXECUTE sp_executesql @tSQL;
PRINT '';