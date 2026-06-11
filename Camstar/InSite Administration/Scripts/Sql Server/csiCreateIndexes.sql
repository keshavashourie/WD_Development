IF EXISTS (SELECT Name 
	   FROM   SYSOBJECTS
	   WHERE  Name = 'csiCreateIndexes' 
	   AND 	  Type = 'P')
	--
	DROP PROCEDURE csiCreateIndexes;
	--
--
GO

CREATE PROCEDURE csiCreateIndexes ( @p_Scope		VARCHAR(10)
				   ,@p_Action		VARCHAR(10)
				   ,@p_DBType		VARCHAR(10) )
AS
BEGIN
------------------------------------------------------------------------------------------------
-- csiCreateIndexes.or.sql
-- NEED to drop existing for ALL EXECUTE
-- Procedure used to Create indexes based on the definitions in the InSite MetaAdmin tables
--
-- Inputs:
--     Scope - <'NEW'/'ALL'>
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
-- Usage: EXEC csiCreateIndexes (<'NEW'/'ALL'>, <'REPORT'/'EXECUTE'>, <'OLTP'/'DATASTORE'>)
-- Note:  The create statements generated do not include any database specific options
--        ex: tablespace or storage parms
-- Note2: The ALL option will drop all existing indexes prior to replacing them.
--
-- Modification History:
-- Name				Date		Action
-- --------------------------	----------	----------------
-- N/A				                         04/21/2005	SPR S9321  - Added DBType parameter
-- N/A				                         06/27/2006 	SPR S11028 - Fixed query that retrieves indexes (for SQL 2005)
-- Bill Lippard			                     12/04/2006 	SPR S9984  - Updated/added copyright notices
-- Bill Lippard			                     04/23/2007	SPR S9984  - Updated copyright notices
-- Bill Lippard			                     05/07/2007	SPR S11840 - Fix error when running against existing
--							                      database. Existing code was incompatible
--							                      with SQL Server 2005.
-- Purushotham Neelakantachar	 06/30/2008	SPR S13239 - Added logic to check for NULL value
--							                     in the @sqlstmt variable. Indented the code 
--							                     for better readability.   		
-- Purushotham Neelakantachar	 07/01/2008	SPR S12807 - Modified the cursors to check for the
--							                     IsUnique flag.
--							                     - Removed SYS.INDEXES check for existing indexes
--
-- Purushotham Neelakantachar	 02/17/2009	SPR S14787 - Migrated Oracle 10g version to SQL Server 2005 version
--							                     - Code was rewritten from scratch
--							                     - Added logic to handle indexes with no columns
--							                     - Added code to handle exceptions
--							                     - Implemented coding standards
--
-- Purushotham Neelakantachar	 02/03/2010	DBUPdate Performance Enhancement
--						                         - Added code for creating indexes online
--						                         - Modified logic to display meaningful error messages
--
-- Oleg Kirasov                           11/12/2010	Update the Index cursor to retrieve
--						                         - existing indexes when new fields have been added 
--						                         - or the existing fields have been deleted
--						                          - Update the logic to add DROP_EXISTING = ON option for all modes
--
-- Purushotham Neelakantachar	 12/15/2010	SPR S18309 - Add Edition check to handle Standard Edition without ONLINE function
--
-- Ramesh Nagamalli                   04/10/2012  Bug #36763 - Add Indices for Metadata Tables
--
-- Nick Aghazarian	                     2016.04.26 Task 38998 Support for custom/manual index creation
--
-- Vishal Mowade                        12/15/2021  CPR 98471 - Add new column IsDisable in DBIndexDefinition Table
--                                               - If Isdisable is true, then drop index without recreating them
--
-- Eugene Androsov			         12/13/2023   Added Index for CLFEventMap table. CPR 368740:(2404) PR? - TAC10849714 API startup fails.
--
-- Dan Maloney                           03/26/2024  US 381239 Modify csiCreateIndexes for SQL Server to add new PK Index for Labels.LabelId
--      
-- Copyright Siemens 2024  
------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------
	--
	BEGIN TRY
		--
		DECLARE @i_ErrorNumber						INTEGER;
		DECLARE @i_ErrorSeverity					INTEGER;
		DECLARE @i_ErrorState						INTEGER;
		DECLARE @i_WorkspaceCreateIndexes	INTEGER;

		--
		DECLARE @v_IndexName						VARCHAR(30);
		DECLARE @v_ColumnName					VARCHAR(255);
		DECLARE @v_TableName						VARCHAR(30);
		DECLARE @v_ConstraintName				VARCHAR(80);
		DECLARE @v_DataType							VARCHAR(30);
		DECLARE @v_Owner								VARCHAR(30);
		DECLARE @v_IsUnique							CHAR(1);
		DECLARE @v_SortOrder						VARCHAR(4);
		DECLARE @v_SQLStmt							VARCHAR(2000);
		DECLARE @v_UseCaseSensitiveRef			CHAR(1);
		DECLARE @v_UseUpper							CHAR(1);
		DECLARE @v_ErrMsg							VARCHAR(2048);
		DECLARE @v_Online1							VARCHAR(256);
		DECLARE @v_Online2							VARCHAR(256);
		DECLARE @v_Banner							VARCHAR(256);
		DECLARE @v_IsDisable							CHAR(1);
		DECLARE @v_IsNullable							VARCHAR(3);
		--
		DECLARE @n_ExistsCheck						NUMERIC;
		DECLARE @n_ColumnLoop						NUMERIC;
		DECLARE @n_ErrLocator						NUMERIC;
		DECLARE @n_IndexCount						NUMERIC;
		DECLARE @n_DBColumnId						NUMERIC;
		DECLARE @n_FieldCount						NUMERIC;
		--
		DECLARE @si_IndexColumnsDefined		SMALLINT;
		--
		DECLARE @rc_IndexCursor					CURSOR;
		DECLARE @Index_Column_Cur				CURSOR;
		--
		DECLARE @v_Unique								VARCHAR(10);
		--
		-- Custom index creation 
		DECLARE @v_WorkspaceCode				VARCHAR(5);
		DECLARE @v_CustomScriptName 			VARCHAR(30);
		DECLARE @WorkspaceCursor				CURSOR;
		--
		SET @n_ExistsCheck = 0;
		SET @n_ColumnLoop = 0;
		SET @n_IndexCount = 0;
		SET @si_IndexColumnsDefined = 0;
		SET @n_FieldCount = 0;
		SET @v_Online1 = '';
		SET @v_Online2 = '';
		SET @v_Banner = NULL;
		--
		-- Check for the installed Oracle Product Edition
		--
		SET @n_ErrLocator = 5;
		--
		SELECT @v_Banner = CAST(SERVERPROPERTY ('edition') AS VARCHAR(256))
		--
		PRINT 'Database Edition: ' + @v_Banner;
		--
        	IF ( ISNULL(@v_Banner,'XXX') LIKE 'Enterprise%' )
			--
			BEGIN
				--
				SET @v_Online1 = 'WITH (ONLINE = ON)';
				SET @v_Online2 = 'WITH (DROP_EXISTING = ON, ONLINE = ON)';
				--
			END;
			--
		ELSE
			--
			BEGIN
				--
				SET @v_Online2 = 'WITH (DROP_EXISTING = ON)';
				--
			END;
			--
		--
        	SET @n_ErrLocator = 10;
		--
		SELECT @n_IndexCount = COUNT(Name)
		  FROM SYSINDEXES
		 WHERE Name = 'DBINDEXENTRIES_NUI1';
		--
		IF ( @n_IndexCount = 0 )
			--
			BEGIN
				--
				SET @v_SQLStmt = 'CREATE INDEX DBINDEXENTRIES_NUI1 ON DBINDEXENTRIES ( DBINDEXID ) ' + @v_Online1;
				--
				SET @n_ErrLocator = 15;
				--
				EXEC ( @v_SQLStmt );
				--
				PRINT 'Index "DBINDEXENTRIES_NUI1" created successfully';
				--
			END;
			--
		--
		SET @n_IndexCount = 0;
		--
		SET @n_ErrLocator = 20;
		--
		SELECT @n_IndexCount = COUNT(Name)
		  FROM SYSINDEXES
		 WHERE Name = 'DBINDEXDEFINITION_UI1';
		--
		IF ( @n_IndexCount = 0 )
			--
			BEGIN
				--
				SET @v_SQLStmt =  'CREATE UNIQUE INDEX DBINDEXDEFINITION_UI1 ON DBINDEXDEFINITION ( DBINDEXNAME ) ' + @v_Online1;
				--
				SET @n_ErrLocator = 25;
				--
				EXEC ( @v_SQLStmt );
				--
				PRINT 'Index "DBINDEXDEFINITION_UI1" created successfully';
				--
			END;
			--
		--
		SET @n_IndexCount = 0;
		--
		SET @n_ErrLocator = 30;
		--
		SELECT @n_IndexCount = COUNT(Name)
		  FROM SYSINDEXES
		 WHERE Name = 'DBINDEXDEFINITION_NUI1';
		--
		IF ( @n_IndexCount = 0 )
			--
			BEGIN
				--
				SET @v_SQLStmt =  'CREATE INDEX DBINDEXDEFINITION_NUI1 ON DBINDEXDEFINITION ( DBTABLEDEFID ) ' + @v_Online1;
				--
				SET @n_ErrLocator = 35;
				--
					EXEC ( @v_SQLStmt );
				--
				PRINT 'Index "DBINDEXDEFINITION_NUI1" created successfully';
				--
			END;
			--
		--
		SET @n_ErrLocator = 40;
		--
		SELECT @v_UseCaseSensitiveRef = TValue
		  FROM INSITESITEINFO
		 WHERE TName = 'CaseSensitiveRefInfo';
		--
		IF ( UPPER(@p_Scope) = 'ALL' ) -- CREATE/REPLACE ALL INDEXES DEFINED IN DBINDEXDEFINITION
			--
			BEGIN
				--
				SET @rc_IndexCursor = CURSOR FOR SELECT DBIndexName
								       ,DTD.DBTableName
								   FROM DBINDEXDEFINITION	DID
								       ,DBTABLEDEFINITION	DTD
								  WHERE DID.DBTableDefId = DTD.DBTableId 
								  ORDER BY DTD.DBTableName
									  ,DID.DBIndexName;
				--
			END;
			--
		ELSE -- CREATE MISSING/NEW INDEXES ONLY
			--
			BEGIN
				--
				SET @rc_IndexCursor = CURSOR FOR 
					-- Get indexes where any columns in existing index have been added
					-- Get new indices
					SELECT DISTINCT DID.DBINDEXNAME
					   ,DTD.DBTableName
					FROM DBINDEXDEFINITION DID 
					  INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
					  INNER JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID
					  INNER JOIN DBColumns DTC ON DTC.DBColumnID = DIE.DBColumnID
					  LEFT JOIN SYS.indexes IND ON DID.DBIndexName = IND.Name
					  LEFT JOIN SYS.tables TAB ON TAB.object_id = IND.object_id
					  LEFT JOIN SYS.columns COL on COL.object_id = TAB.object_id
						and COL.name = DTC.ColumnName
					  LEFT JOIN SYS.index_columns INDC on INDC.index_id = IND.index_id
						 and INDC.object_id = COL.object_id and INDC.column_id = COL.column_id
					WHERE ((IND.Name IS NULL) 
					  OR (INDC.index_column_id is NULL)
					  OR IND.is_unique <> (CASE WHEN @p_DBType = 'DATASTORE'
						THEN 0 -- unique indexs should be rebuilt
						ELSE
						DID.IsUnique --look for uniqueness changed in OLTP
						END)) Or DID.IsDisable = 1
					UNION  
					-- Get indexes where any columns in existing index have been deleted
					SELECT DISTINCT DID.DBINDEXNAME
					   ,DTD.DBTableName
					FROM SYS.indexes IND
					  INNER JOIN SYS.tables TAB ON TAB.object_id = IND.object_id
					  INNER JOIN SYS.columns COL on COL.object_id = TAB.object_id
					  INNER JOIN SYS.index_columns INDC on INDC.index_id = IND.index_id
						 and INDC.object_id = COL.object_id and INDC.column_id = COL.column_id
					  INNER JOIN DBINDEXDEFINITION DID ON DID.DBIndexName = IND.Name
					  INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
					  LEFT JOIN DBColumns DTC ON DTC.DBTableID = DTD.DBTableId
						and DTC.ColumnName = COL.name
					  LEFT JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID
						and DIE.DBColumnId = DTC.DBColumnID
					WHERE (DIE.DBIndexEntryID is NULL)
					ORDER BY DBTableName, DBIndexName
				--
			END;
			--
		--
		SET @n_ErrLocator = 45;
		--
		OPEN @rc_IndexCursor;
		FETCH NEXT FROM @rc_IndexCursor INTO @v_IndexName
						    ,@v_TableName;
		--
		WHILE (@@FETCH_STATUS <> -1)
			--
			BEGIN --2
				--
				SET @Index_Column_Cur = CURSOR FOR SELECT DC.DBColumnId		ColumnId
								         ,DTD.DBTableName	TableName
								         ,DC.ColumnName		ColumnName
								         ,DID.IsUnique		IsUnique
								         ,SortOrder = CASE DIE.Sortorder 
											   WHEN 2 
											   THEN 'DESC' 
											   ELSE 'ASC' 
										      END
										,DID.IsDisable IsDisable
								     FROM DBINDEXENTRIES	DIE
								         ,DBINDEXDEFINITION	DID
								         ,DBCOLUMNS		DC
								         ,DBTABLEDEFINITION	DTD
								    WHERE DIE.DBIndexId = DID.DBIndexId
								      AND DIE.DBColumnId = DC.DBColumnId
								      AND DID.DBTableDefId = DTD.DBTableId
								      AND DID.DBIndexName = @v_IndexName
								   ORDER BY DTD.DBTableName
									   ,DIE.DBIndexId
									   ,DIE.Sequence;
				--
				SET @n_ColumnLoop = 1;
				--
				OPEN @Index_Column_Cur;
				FETCH NEXT FROM @Index_Column_Cur INTO @n_DBColumnId
								      ,@v_TableName
								      ,@v_ColumnName
								      ,@v_IsUnique
								      ,@v_SortOrder
									  ,@v_IsDisable;
				--
				WHILE (@@FETCH_STATUS <> -1)
					--
					BEGIN --2
						--
						SET @si_IndexColumnsDefined = 1;
						--
						IF ( @n_ColumnLoop = 1 ) -- FOR THE FIRST COLUMN IN THE LIST
							--
							BEGIN
								--
								IF ( UPPER(@p_DBType)='OLTP' ) 
									--
									BEGIN
										--
										SET @n_ErrLocator = 50;
										--
										SELECT @v_Unique = CASE ( @v_IsUnique )
													WHEN 1
													THEN 'UNIQUE '
													ELSE ''
												   END;
										--
									END;
									--
								ELSE
									--
									SET @v_Unique = '';
									--
								--
								SET @v_SQLStmt = 'CREATE ' + @v_Unique + 'INDEX ' + @v_IndexName + ' ON '+ @v_TableName + '(';
								--
							END;
							--
						--
						IF ( @v_UseCaseSensitiveRef = 'Y' )
							--
							SET @v_UseUpper = 'N';
							--
						ELSE
							--
							BEGIN
								--
								SET @n_ErrLocator = 55;
								--
								SELECT @n_FieldCount = COUNT(*)
								  FROM CDOFIELDS
								 WHERE DBColumnId = @n_DBColumnId
								   AND CDOFieldUsageId = 1;
								--
								IF ( @n_FieldCount > 0 )
									--
									SET @v_UseUpper = 'Y';
									--
								ELSE
									--
									SET @v_UseUpper = 'N';
									--
								--
							END;
							--
						--
						IF ( @v_UseUpper = 'Y' )
							--
							SELECT @v_ColumnName = UPPER(@v_ColumnName);
							--
						ELSE
							--
							SET @v_ColumnName = @v_ColumnName;
							--
						--
						SET @v_SQLStmt = @v_SQLStmt + @v_ColumnName + ' ' + @v_SortOrder + ',';
						--
						SET @n_ColumnLoop = @n_ColumnLoop + 1;
						--
						FETCH NEXT FROM @Index_Column_Cur INTO @n_DBColumnId
										      ,@v_TableName
										      ,@v_ColumnName
										      ,@v_IsUnique
										      ,@v_SortOrder
											  ,@v_IsDisable;
						--
					END;
					--
				CLOSE @Index_Column_Cur;
				DEALLOCATE @Index_Column_Cur;
				--
				SET @v_SQLStmt = SUBSTRING ( @v_SQLStmt,1,LEN(@v_SQLStmt)-1) + ') '; -- CLOSE THE COLUMN LIST
				--
				IF ( @si_IndexColumnsDefined = 1 )
					--
					BEGIN
						--
						SET @n_ErrLocator = 60;
						--
						SELECT @n_ExistsCheck = COUNT(Name)
						  FROM SYSINDEXES
						 WHERE Name = @v_IndexName;
						--
						IF ( @n_ExistsCheck > 0 )								
							--
							BEGIN
								--
								SET @n_ErrLocator = 65;
								--
								-- ADD DROP_EXISTING CLAUSE TO STATEMENT IF THE INDEX 
								-- EXISTS AND SCOPE IN 'NEW' OR 'ALL'
								--
								IF( @v_IsDisable = 1 )
								--
								BEGIN
								     SET @v_SQLStmt = 'DROP INDEX ' + @v_IndexName + ' ON ' + @v_TableName;
									 PRINT 'Index ' + @v_IndexName + ' will be dropped';
								END;
								--
								ELSE
								--
								    BEGIN
								         SET @v_SQLStmt = @v_SQLStmt + CASE ( ISNULL(@v_Online2,'XXX') )
												WHEN 'XXX'
												THEN ')'
												ELSE @v_Online2
											      END;
								--
								PRINT 'Index ' + @v_IndexName + ' will be dropped and recreated';
								--
								END;
							END;
							--
						ELSE
							--
							BEGIN
								--
								IF( @v_IsDisable = 1)
								  BEGIN
								  SET @v_SQLStmt ='';
								  END
								ELSE
								  BEGIN
								  SET @v_SQLStmt = @v_SQLStmt + ' ' + @v_Online1;
								  END
								--
							END;
							--
						--
						IF ( UPPER( @p_Action ) = 'EXECUTE' )
							--
							BEGIN TRY
								--
								SET @n_ErrLocator = 70;
								--
								EXEC ( @v_SQLStmt );
								--
								IF( @v_IsDisable = 1 and @n_ExistsCheck > 0 )
								--
								  BEGIN
								       PRINT 'Index ' + @v_IndexName + ' dropped successfully';
								  END
								--
                               ELSE IF (@v_IsDisable <> 1)
							    --
							      BEGIN
								       PRINT 'Index ' + @v_IndexName + ' created successfully';
								  END
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
									PRINT 'Index "' + @v_IndexName + '" already exists. Skipping...';
									--
								--
							END CATCH
							--
						ELSE
							--
							BEGIN
								--
								IF ( UPPER( @p_Action ) = 'REPORT' )
									--
									PRINT @v_SQLStmt;
									--
								--
							END;
							--
						--
						SET @v_SQLStmt = NULL;
						SET @v_IndexName = NULL;
						SET @n_FieldCount = 0;
						SET @n_ExistsCheck = 0;
						--
						SET @si_IndexColumnsDefined = 0;
						--
					END;
					--
				ELSE
					--
					PRINT 'WARNING - No columns in table "' + @v_TableName + '" defined for index "' + @v_IndexName + '". Please contact System Administrator.';
					--
				--
				FETCH NEXT FROM @rc_IndexCursor INTO @v_IndexName
								    ,@v_TableName;
				--
			END;
			--
		CLOSE @rc_IndexCursor;
		DEALLOCATE @rc_IndexCursor;
		----
		-- This section will check for the existence of any workspace specific index creation scripts
		-- and execute them if found.
		PRINT 'Processing custom scripts...';
		SET @WorkspaceCursor = CURSOR FOR SELECT WS.WorkspaceCode
								FROM Workspace WS
								WHERE WS.IsActive = 1
								ORDER BY WS.Sequence;
		--
		SET @n_ErrLocator = 200;
		--
		OPEN @WorkspaceCursor;
		--
		SET @n_ErrLocator = @n_ErrLocator + 5;
		FETCH NEXT FROM @WorkspaceCursor INTO @v_WorkspaceCode;
		WHILE (@@FETCH_STATUS <> -1)
			--
			BEGIN 
			SET @n_ErrLocator = @n_ErrLocator + 1;
			--
			SET @v_CustomScriptName = 'Workspace_' + @v_WorkspaceCode + '_CreateIndexes';
			--
			-- Check to see if the stored procedure exists
			SELECT @n_ExistsCheck = COUNT(Name) FROM sys.procedures WHERE Upper(Name) = Upper(@v_CustomScriptName);
			--
			-- If it does, then call it.
			IF (@n_ExistsCheck = 1)
				BEGIN
					EXEC @v_CustomScriptName @p_Scope, @p_Action, @p_DBType;
					SET @n_ErrLocator = @n_ErrLocator + 1;
				END
			--
			FETCH NEXT FROM @WorkspaceCursor INTO @v_WorkspaceCode;
			END 
		CLOSE @WorkspaceCursor ;
		DEALLOCATE @WorkspaceCursor ;
		--
		SET @n_ErrLocator = @n_ErrLocator + 5;
		--
		-- End of Custom Index Creation Script section
		----
		--
		BEGIN
			-- Create Metadata Indices
			SET @n_ErrLocator = 75;
			SET @v_IndexName = 'CDO_Parent_Idx';
			--
			SET @v_SQLStmt = 'CREATE UNIQUE NONCLUSTERED INDEX [' + @v_IndexName + '] ON [CDODefinition] ([ParentCDOID] ASC, [CDODefID] ASC ) ';
			--
			SELECT @n_ExistsCheck = COUNT(Name)
				FROM SYSINDEXES
				WHERE Name = @v_IndexName;
			--
			IF ( @n_ExistsCheck > 0 )								
				--
				BEGIN
					--
					-- ADD DROP_EXISTING CLAUSE TO STATEMENT SINCE THE INDEX EXISTS
					-- EXISTS AND SCOPE IN 'NEW' OR 'ALL'
					--
					--
					SET @n_ErrLocator = 80;
					--
					SET @v_SQLStmt = @v_SQLStmt + CASE ( ISNULL(@v_Online2,'XXX') )
									WHEN 'XXX'
									THEN ''
									ELSE @v_Online2
										END;
					--
					PRINT 'Index ' + @v_IndexName + ' will be dropped and recreated';
					--
				END;
				--
			ELSE
				--
				BEGIN
					--
					SET @v_SQLStmt = @v_SQLStmt + ' ' + @v_Online1;
					--
				END;
				--
			--
			SET @n_ErrLocator = 85;
			--
			IF ( UPPER( @p_Action ) = 'EXECUTE' )
				--
				BEGIN TRY
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
						PRINT 'Index "' + @v_IndexName + '" already exists. Skipping...';
						--
					--
				END CATCH
				--
			ELSE
				--
				BEGIN
					--
					IF ( UPPER( @p_Action ) = 'REPORT' )
						--
						PRINT @v_SQLStmt;
						--
					--
				END;
				--
			--
			
			SET @n_ErrLocator = 86;
			SET @v_IndexName = 'CLFEventMap_Event_Idx';
			--
			SET @v_SQLStmt = 'CREATE NONCLUSTERED INDEX [' + @v_IndexName + '] ON [CLFEventMap] ([CLFEventID]) INCLUDE ([CDODefID],[CLFID],[Name]) ';

			--
			SELECT @n_ExistsCheck = COUNT(Name)
				FROM SYSINDEXES
				WHERE Name = @v_IndexName;
			--
			IF ( @n_ExistsCheck > 0 )								
				--
				BEGIN
					--
					-- ADD DROP_EXISTING CLAUSE TO STATEMENT SINCE THE INDEX EXISTS
					-- EXISTS AND SCOPE IN 'NEW' OR 'ALL'
					--
					--
					SET @n_ErrLocator = 87;
					--
					SET @v_SQLStmt = @v_SQLStmt + CASE ( ISNULL(@v_Online2,'XXX') )
									WHEN 'XXX'
									THEN ''
									ELSE @v_Online2
										END;
					--
					PRINT 'Index ' + @v_IndexName + ' will be dropped and recreated';
					--
				END;
				--
			ELSE
				--
				BEGIN
					--
					SET @v_SQLStmt = @v_SQLStmt + ' ' + @v_Online1;
					--
				END;
				--
				
				SET @n_ErrLocator = 88;
			--
			IF ( UPPER( @p_Action ) = 'EXECUTE' )
				--
				BEGIN TRY
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
						PRINT 'Index "' + @v_IndexName + '" already exists. Skipping...';
						--
					--
				END CATCH
				--
			ELSE
				--
				BEGIN
					--
					IF ( UPPER( @p_Action ) = 'REPORT' )
						--
						PRINT @v_SQLStmt;
						--
					--
				END;
				--
			--
			
			--US 381239  Modify csiCreateIndexes for SQL Server to add new PK Index for Labels.LabelId
			SET @n_ErrLocator = 90;
			-- Check to see if primary key exists on Labels table
			SELECT @v_ConstraintName = CONSTRAINT_NAME
			FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
			WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME='LABELS' AND TABLE_SCHEMA = SCHEMA_NAME();

			IF ( @v_ConstraintName IS NOT NULL)								
			BEGIN
				PRINT 'Primary key constraint ' + @v_ConstraintName + ' already exists on Labels table';
			END;
			ELSE
			BEGIN
				-- Get data type of LabelID column of Labels table
				SELECT @v_DataType = DATA_TYPE, @v_IsNullable = IS_NULLABLE  FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Labels'  and COLUMN_NAME = 'LabelId' AND TABLE_SCHEMA = SCHEMA_NAME();
				
				-- Alter LabelId column to NOT NULL 
				SET @v_ConstraintName = 'PK_Labels';
				PRINT 'Primary key constraint ' + @v_ConstraintName + ' will be created';
				IF @v_IsNullable = 'YES' 
				BEGIN
					SET @v_SQLStmt = 'ALTER TABLE LABELS ALTER COLUMN LabelId ' + @v_DataType + ' NOT NULL';
					IF ( UPPER( @p_Action ) = 'EXECUTE' )
						BEGIN TRY
							--PRINT @v_SQLStmt;
							EXEC ( @v_SQLStmt );
							PRINT 'LabelId in Labels table set to NOT NULL';
						END TRY
						BEGIN CATCH
							SELECT @i_ErrorSeverity = ERROR_SEVERITY()
							,@i_ErrorState = ERROR_STATE()
							,@i_ErrorNumber = ERROR_NUMBER();
							PRINT 'ErrorNum: ' + CAST(@i_ErrorNumber AS VARCHAR(10))
						END CATCH;
					ELSE
					BEGIN
						IF ( UPPER( @p_Action ) = 'REPORT' )
							PRINT @v_SQLStmt;
					END;
				END;
				ELSE
					PRINT 'LabelId in Labels table is already set to NOT NULL';
				
				-- Create Primary Key Constraint on LabelsId table
				SET @v_SQLStmt = 'ALTER TABLE LABELS ADD CONSTRAINT ' + @v_ConstraintName + ' PRIMARY KEY (LabelId)';
				IF ( UPPER( @p_Action ) = 'EXECUTE' )
					BEGIN TRY
						--PRINT @v_SQLStmt;					
						EXEC ( @v_SQLStmt );
						PRINT 'Primary Key Constraint ' + @v_ConstraintName + ' created on Labels table successfully';
					END TRY
					BEGIN CATCH
						SELECT @i_ErrorSeverity = ERROR_SEVERITY()
						,@i_ErrorState = ERROR_STATE()
						,@i_ErrorNumber = ERROR_NUMBER();
						PRINT 'ErrorNum: ' + CAST(@i_ErrorNumber AS VARCHAR(10))
						IF ( @i_ErrorNumber IN ( 1779, 1760 ) )
							PRINT 'Primary key constraint  "' + @v_ConstraintName + '" already exists. Skipping...';
						ELSE
							PRINT 'ErrorNum: ' + CAST(@i_ErrorNumber AS VARCHAR(10));
					END CATCH;
				ELSE
				BEGIN
					IF ( UPPER( @p_Action ) = 'REPORT' )
						PRINT @v_SQLStmt;
				END;
			END;

		END -- Create Metadata Indices
		--
	END TRY
	BEGIN CATCH
		--
		SELECT @v_ErrMsg = 'Error creating indexes - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE()
		      ,@i_ErrorSeverity = ERROR_SEVERITY()
		      ,@i_ErrorState = ERROR_STATE();
		--
	END CATCH;
	--
END;
GO
