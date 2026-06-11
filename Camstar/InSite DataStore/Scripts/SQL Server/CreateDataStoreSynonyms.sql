------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      CreateDataStoreSynonyms.sql
-- DESCR:       Creates the Datastore synonyms in the datastore database pointing to production database queue tables.  Executes on the ODS
-- HISTORY:
--              06/10/2106      Dan Maloney      Fixed comment and renamed script from CreateDataStoreLocalTables.sql to CreateDataStoreSynonym.sql
--              04/19/2016      Dan Maloney      Cleaned up script, added indentation, add place in history comments for name of person making changes
--              06/07/2016      Dan Maloney      Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--              06/13/2016      Dan Maloney      Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--              08/04/2017      Dan Maloney      Removed variable @i_ErrorStatus (US 51393)
--              10/23/2019      Dan maloney      Modify logic to drop synonyms from ODS to OLTP and recreate them based on InsiteSiteInfo.DataStoreInsertTables (CPR 1260)
--              06/11/2024      Dan Maloney      Modify logic to drop the OLTP_DBDataSourceNames synonym and recreate it everytime this script runs (every DBCreate or DBUpdate) to ensure the synonym is correctly pointing
--              06/11/2024      Dan Maloney      CPR 414156 to drop and recreate the OLTP_DBDataSourceNames synonym on the ODS
--
-- Copyright Siemens 2024
------------------------------------------------------------------------------------------------------------------------------------------------------


DECLARE @i_NumTables    INT
DECLARE @i_TblNum       INT
DECLARE @v_Enabled      VARCHAR(1)
DECLARE @v_TblName      VARCHAR(50)
DECLARE @v_MastTblName  VARCHAR(50)
DECLARE @nv_RemoteDB    NVARCHAR(512)
DECLARE @nv_Sql         NVARCHAR(4000)
DECLARE @nv_SynName		NVARCHAR(128)
DECLARE @nv_Msg         NVARCHAR(2048)

DECLARE cur_Syn CURSOR FOR
	SELECT NAME 
	FROM SYSOBJECTS 
	WHERE TYPE = 'SN' AND NAME LIKE 'DataStore%';
	
BEGIN
	SET NOCOUNT ON

	-- Check to see if DataStore is running
	SELECT @v_Enabled=TVALUE
   	FROM InsiteSiteInfo
   	WHERE TName = 'DataStorePresent'

   	IF @v_Enabled = 'N'
	BEGIN
      	--DataStore is running.  Exit
		PRINT 'DataStore is running.'
		PRINT 'Script CreateDataStoreSynonyms.sql exiting'
 		RETURN
	END

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

	--Drop existing queue table synonyms to the production database
	OPEN cur_Syn
	FETCH NEXT FROM cur_Syn INTO @nv_SynName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		BEGIN TRY
			SET @nv_Sql = N'DROP SYNONYM ' + @nv_SynName
			EXEC (@nv_Sql) 
			FETCH NEXT FROM cur_Syn INTO @nv_SynName
		END TRY
		BEGIN CATCH
			-- Drop synonym failed
			SET @nv_Msg = N'Unable to drop ' + @nv_SynName + N' synonym.  Script CreateDataStoreSynonym.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
	END
	CLOSE cur_Syn
	DEALLOCATE cur_Syn
	
	
    SET @nv_RemoteDB=N'[:OLTPDBHOST].[:DBName].[:OLTPDBSCHEMA]'
      
    SET @i_TblNum = 0
    WHILE @i_TblNum <= @i_NumTables
	BEGIN
         	IF @i_TblNum = 0
         	BEGIN
            	SET @v_MastTblName = 'DataStoreUpdatesMaster'
            	SET @v_TblName = 'DataStoreUpdates'            
         	END
         	ELSE
         	BEGIN
            	SET @v_MastTblName = 'DataStoreInserts' + CONVERT(NVARCHAR, @i_TblNum) + 'Master'
            	SET @v_TblName = 'DataStoreInserts' + CONVERT(NVARCHAR, @i_TblNum)            
         	END

         	-- Create master (control) table synonym to the production database
         	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = @v_MastTblName AND Type='SN')
         	BEGIN
				BEGIN TRY
					SET @nv_Sql = N'CREATE SYNONYM ' + @v_MastTblName + N' FOR ' + @nv_RemoteDB + N'.' + @v_MastTblName
					EXEC (@nv_Sql) 
					PRINT @nv_Sql   
				END TRY
				BEGIN CATCH
					-- Unable to create synonym
					SET @nv_Msg = N'Unable to create ' + @v_MastTblName + N' synonym.  Script CreateDataStoreSynonym.sql exiting.';
					PRINT @nv_Msg
					PRINT ERROR_MESSAGE();
					THROW 51000, @nv_Msg, 1
				END CATCH              
         	END

         	-- Create detail table synonym to the production database
         	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = @v_TblName AND Type='SN')
         	BEGIN
				BEGIN TRY
					SET @nv_Sql = N'CREATE SYNONYM ' + @v_TblName+ N' FOR ' + @nv_RemoteDB + N'.' + @v_TblName
					EXEC (@nv_Sql)  
					PRINT @nv_Sql  
				END TRY  
				BEGIN CATCH
					-- Unable to create synonym
					SET @nv_Msg = N'Unable to create ' + @v_TblName + N' synonym.  Script CreateDataStoreSynonym.sql exiting.';
					PRINT @nv_Msg
					PRINT ERROR_MESSAGE();
					THROW 51000, @nv_Msg, 1
				END CATCH              
         	END
			
         	SET @i_TblNum = @i_TblNum+1
	END  

	-- Create synonym to OLTP DBDataSourcenames table
	  BEGIN
		BEGIN TRY
			SET @nv_Sql = N'DROP SYNONYM IF EXISTS ' + 'OLTP_DBDataSourceNames'
			EXEC (@nv_Sql) 
			PRINT @nv_Sql   
		END TRY
		BEGIN CATCH
			-- Unable to drop synonym
				SET @nv_Msg = N'Unable to drop OLTP_DBDataSourcenames synonym in order to recreate the OLTP_DBDataSourcenames synonym  Script CreateDataStoreSynonym.sql exiting.';
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
		END CATCH

		BEGIN TRY
        	SET @nv_Sql = N'CREATE SYNONYM ' + 'OLTP_DBDataSourceNames' + N' FOR ' + @nv_RemoteDB + N'.DBDataSourcenames'
            EXEC (@nv_Sql) 
			PRINT @nv_Sql   
		END TRY
		BEGIN CATCH
			-- Unable to create synonym
			SET @nv_Msg = N'Unable to create OLTP_DBDataSourcenames synonym.  Script CreateDataStoreSynonym.sql exiting.';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH              
	END
END
GO


