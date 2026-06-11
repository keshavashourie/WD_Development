/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_CreateArchiveUser 
                     Archive User creation script : 
					 1. Create an Archive Database. 
					 2. Create an Archive Login User
					 3. Create Archive database User.
					 4. Grant the necessary privileges to this archive user.
					 Note : Login as sa to create this user.
  Author           : Benny.Chia 
  Date             : 09 Nov 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
--------------------------------------------------------------------------------------------------
-- Note : Globally replace the 6 following values to the desired value in your database 
--        before running this script.   
---------------------------------------------------------------------------------------------------
-- 0. Archive Database Name         
-- 1. Archive Database User Name    
-- 2. Archive Schema Name       
-- 3. Source DATABASE		

BEGIN
	DECLARE @tSQL NVARCHAR(1000);
	--------------------------------------------------------------------------------------------------
	-- Step 1 : Create an Archive Database 
	---------------------------------------------------------------------------------------------------	
	IF NOT EXISTS (SELECT 1 FROM SYS.DATABASES WHERE NAME = '{0}')
	BEGIN
		PRINT 'Creating Archive Database...';
		SET @tSQL = N'CREATE DATABASE {0}';
		EXECUTE sp_executesql @tSQL;
		PRINT 'Database creation is completed.'
	END
	---------------------------------------------------------------------------------------------------
	-- Step 3 : Create an Archive Database User
	---------------------------------------------------------------------------------------------------
	IF NOT EXISTS (SELECT 1 FROM {0}.SYS.database_principals WHERE NAME = '{1}')
	BEGIN
		PRINT 'Creating Archive Database User...'
		SET @tSQL =  N'CREATE USER {1} WITHOUT LOGIN';
		EXECUTE {0}.SYS.sp_executesql @tSQL;
		PRINT 'Archive Database User created.'

		---------------------------------------------------------------------------------------------------
		-- Step 5 : Grant Permissions to Archive Database User 
		---------------------------------------------------------------------------------------------------
		PRINT 'Granting Permissions to Archive Database User...'
		SET @tSQL = N'GRANT ALTER, CREATE FUNCTION, CREATE PROCEDURE, CREATE SYNONYM, CREATE TABLE, CREATE TYPE, CREATE VIEW TO [{1}]'
		EXECUTE sp_executesql @tSQL;
		SET @tSQL = N'GRANT REFERENCES, SHOWPLAN, VIEW DATABASE STATE, VIEW DEFINITION TO [{1}]'
		EXECUTE sp_executesql @tSQL;
		SET @tSQL = N'GRANT CHECKPOINT, DELETE, EXECUTE, INSERT, SELECT, UPDATE TO [{1}]'
		EXECUTE sp_executesql @tSQL;
		PRINT 'Permissions to Archive Database User granted.'

		PRINT 'Authorising configDB login which contains source user access to the Archive Database, including creating tables ...'
		SET @tSQL = N'ALTER AUTHORIZATION ON DATABASE::[{0}] to [{1}];';
		EXECUTE sp_executesql @tSQL;
		PRINT 'Authorising configDB login which contains source login user access to the Archive Database completed.'
	END
	---------------------------------------------------------------------------------------------------
	-- Step 4 : Create Archive Schema
	---------------------------------------------------------------------------------------------------	
	IF NOT EXISTS (SELECT 1 FROM {0}.SYS.SCHEMAS WHERE NAME = '{2}')
	BEGIN
		PRINT 'Creating Archive Schema...';
		SET @tSQL = N'CREATE SCHEMA [{2}] AUTHORIZATION [{1}]';
		EXECUTE {0}.SYS.sp_executesql @tSQL;
		SET @tSQL = N'ALTER USER [{1}] WITH DEFAULT_SCHEMA=[{2}]';
		EXECUTE {0}.SYS.sp_executesql @tSQL;
		PRINT 'Archive Schema created.'
	END
	
	---------------------------------------------------------------------------------------------------
	-- Step 6 : Authorising ConfigDB login user access to the Archive Database.
	--          Alter the Archive Database Collation to be the same as the Source Database Collation.
	-- Note : Once authorisation is granted, the ConfigDB login user is able to create tables in the archive database.
	--        
	---------------------------------------------------------------------------------------------------
	PRINT 'Altering the Archive Database Collation to be the same as the Source Database Collation...'
	DECLARE @vSourceDatabaseCollation AS SYSNAME;
	DECLARE @vSQLAlterDatabaseCollation AS NVARCHAR(4000);
	SELECT @vSourceDatabaseCollation = collation_name 
	FROM sys.databases WHERE name = '{3}';
	PRINT 'The Source Database Collation is ' + @vSourceDatabaseCollation;
	SET @vSQLAlterDatabaseCollation = 'ALTER DATABASE {0} COLLATE ' + @vSourceDatabaseCollation; 
	EXECUTE sp_executesql @vSQLAlterDatabaseCollation;
	PRINT 'Altering the Archive Database Collation to ' + @vSourceDatabaseCollation + ' completed.';
END