IF EXISTS (SELECT Name 
	   FROM   SYSOBJECTS
	   WHERE  Name = 'Workspace_WSCode_CreateIndexes' 
	   AND 	  Type = 'P')
	--
	DROP PROCEDURE Workspace_WSCode_CreateIndexes;
	--
--
GO

CREATE PROCEDURE Workspace_WSCode_CreateIndexes ( @p_Scope		VARCHAR(10)
				   ,@p_Action		VARCHAR(10)
				   ,@p_DBType		VARCHAR(10) )
AS
BEGIN
------------------------------------------------------------------------------------------------
-- Workspace_WSCode_CreateIndexes.sql
-- NEED to drop existing for ALL EXECUTE
-- Procedure used to Create indexes for Workspace WSCode that cannot be created in Designer
--
-- Inputs:
--     Scope - <'NEW'/'ALL'> - Not used at this time, as New indexes can be handled by the designer
--           NEW - Will create/report only "new" indexes that exist in the InSite MetaAdmin tables 
--                 but not the database catalog
--           ALL - Will create/report ALL indexes that are defined in the InSite MetaAdmin tables
--     Action - <'REPORT'/'EXECUTE'>
--           REPORT - Display the generated create index statements
--           EXECUTE - Execute the default create index statement
--     DBType - <'OLTP'/'DATASTORE'>  
--           OLTP - Unique indexes will use the UNIQUE tag
--           DATASTORE - Unique indexes will not use the UNIQUE tag.
--
-- Usage: EXEC Workspace_WSCode_CreateIndexes (['NEW'|'ALL'], ['REPORT'|'EXECUTE'], ['OLTP'|'DATASTORE'])
--
-- Modification History:
-- Name				Date		Action
-- ----------------------------	----------	----------------
-- Nick Aghazarian	2016.04.27	Created
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------
	--
	BEGIN TRY
		--
		DECLARE @i_ErrorNumber			INTEGER;
		DECLARE @i_ErrorSeverity		INTEGER;
		DECLARE @i_ErrorState			INTEGER;

		--
		DECLARE @v_IndexName			VARCHAR(30);
		DECLARE @v_TableName			VARCHAR(30);
		DECLARE @v_SQLStmt				VARCHAR(2000);
		DECLARE @v_Unique				VARCHAR(8);

		DECLARE @n_ExistsCheck			NUMERIC;
		DECLARE @n_ErrLocator			NUMERIC;
		DECLARE @v_ErrMsg				VARCHAR(2048);

		--
		SET @v_IndexName = 'IndexName';
		SET @v_TableName = 'TableName';
		--
		SET @n_ExistsCheck = 0;
		SELECT @n_ExistsCheck = COUNT(Name) FROM SYSINDEXES WHERE Name = @v_IndexName;


		-- Add "UNIQUE" keyword as needed, Datastore is usually not unique
		IF ( UPPER(@p_DBType)='OLTP' ) 
			SET @v_Unique = ' ';
		ELSE
			SET @v_Unique = ' ';

			--

		SET @v_SQLStmt = 'CREATE ' + @v_Unique + ' INDEX ' + @v_IndexName + ' ON '+ @v_TableName + '(';
		--
		-- Set Index Columns
		SET @v_SQLStmt = @v_SQLStmt + 'Name ASC,';

		-- Remove last comma and close list
		SET @v_SQLStmt = SUBSTRING ( @v_SQLStmt,1,LEN(@v_SQLStmt)-1); 
		SET @v_SQLStmt = @v_SQLStmt + ') '; 

		-- Drop if exists
		if (@n_ExistsCheck > 0)
			SET @v_SQLStmt = @v_SQLStmt + '	WITH(DROP_EXISTING = ON) ';


		PRINT 'Index ' + @v_IndexName + ' will be dropped and recreated';
		IF ( UPPER( @p_Action ) = 'EXECUTE' )
			--
			BEGIN TRY
				--
				SET @n_ErrLocator = 70;
				--
				EXEC ( @v_SQLStmt );
				--
				PRINT 'Index ' + @v_IndexName + ' created successfully';
				--
			END TRY
			--
			BEGIN CATCH
			--
				SELECT @i_ErrorSeverity = ERROR_SEVERITY()
				      ,@i_ErrorState = ERROR_STATE()
				      ,@i_ErrorNumber = ERROR_NUMBER();
				--
				PRINT 'ErrorNum: ' + CAST(@i_ErrorNumber AS VARCHAR(10))
				--
				IF ( @i_ErrorNumber IN ( 1913 ) )
					--
					PRINT 'Index ' + @v_IndexName + ' already exists. Skipping...';
					--
			END CATCH
			--
		ELSE
			--
			BEGIN
				PRINT @v_SQLStmt;
			END;
			--
	END TRY
	BEGIN CATCH
	--
		SELECT @i_ErrorSeverity = ERROR_SEVERITY()
		      ,@i_ErrorState = ERROR_STATE()
		      ,@i_ErrorNumber = ERROR_NUMBER();
		--
		PRINT 'ErrorNum: ' + CAST(@i_ErrorNumber AS VARCHAR(10))
		--
	END CATCH
END;
GO
