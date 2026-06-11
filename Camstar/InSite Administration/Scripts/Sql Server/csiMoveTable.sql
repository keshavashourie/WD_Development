SET NOCOUNT ON
BEGIN
	--
	------------------------------------------------------------------------------------------------
	-- This script is called from ..\scripts\MoveTable.bat.  It moves a user-specified 
	-- table in a SQL Server database to a user-specified Filegroup by moving the table's
	-- Clustered Index (CI) to the target Filegroup.  If the table does not have a CI, one is
	-- created and then dropped after the move is completed.
	-- Inputs:
	-- 	TableToMove - Name of the table to be moved  
	--	FGName      - Name of the target filegroup
	--
	--
	-- CAUTION ****	CAUTION ****	CAUTION ****	CAUTION ****	CAUTION
	-- 
	-- Before running this script, please ensure to back up the database
	-- and verify the back ups are good.
	-- 
	-- CAUTION ****	CAUTION ****	CAUTION ****	CAUTION ****	CAUTION
	--
	--
	--  Modification History:
	--  Name			Date		Action
	--  --------------------------	----------	----------------
	--  Bill Lippard		09/04/2008	Initial Creation (SPR S13641)
	--  Purushotham Neelakantachar	10/13/2009  	Modified the code as below:
	--						- Formatted the code as appropriate
	--
	-- Copyright Siemens 2023  

	------------------------------------------------------------------------------------------------
	--
	DECLARE @TargetFileGroupID INT
	DECLARE @FGName NVARCHAR(128)
	DECLARE @TableToMove NVARCHAR(128)
	DECLARE @TableToMoveObjID INT
	DECLARE @ScriptMsg NVARCHAR(512)
	DECLARE @DatabaseName SYSNAME
	DECLARE @ServerName SYSNAME
	DECLARE @TableHasCI BIT
	DECLARE @TableHasIdent BIT
	DECLARE @TableHasPK BIT
	DECLARE @TableHasUQ BIT
	DECLARE @IdentColName NVARCHAR(128)
	DECLARE @ColList NVARCHAR(1024)
	DECLARE @indid NVARCHAR(128)
	DECLARE @Type CHAR(2)
	DECLARE @KeyName NVARCHAR(128)
	DECLARE @CIName NVARCHAR(128)
	DECLARE @IsPadIndex BIT
	DECLARE @i INT
	DECLARE @j INT
	DECLARE @SQLStr NVARCHAR(4000)
	--
	-- Set table and filegroup names from parameters passed to calling script
	--
	SET @TableToMove = '$(tablename)'
	SET @FGName = '$(filegroup)'
--	SET @TableToMove = '$(tablename)'
--	SET @FGName = 'insiteadmin_fg'
	--
	-- Get the ObjectID of the table to be moved
	--
	SELECT @TableToMoveObjID = ( SELECT OBJECT_ID from SYS.OBJECTS
				      WHERE NAME = @TableToMove )
	--
	-- Temporary table for the column names of keys and constraints
	--
	IF OBJECT_ID('tempdb..#ttColTable', 'U') IS NOT NULL
		DROP TABLE #ttColTable
	--
	CREATE TABLE #ttColTable ( Idx		INT IDENTITY(1, 1)
				  ,ColName	NVARCHAR(128)
				  ,IdxOrder	CHAR(4) )
	--
	-- Set server and database names
	--
	SET @ServerName = CAST(ISNULL(SERVERPROPERTY('ServerName'), 'Unknown') AS SYSNAME)
	SET @DatabaseName = DB_NAME()
	--
	-- Check that file group and table exist.
	--
	SET @TargetFileGroupID = FILEGROUP_ID(@FGName)
	--
	IF @TargetFileGroupID IS NULL
	BEGIN
		--
	        SET @ScriptMsg = N'The target file group ' + @FGName + N' does not exist in database ' + @DatabaseName + N', on server ' + @ServerName + N'. Please provide a valid filegroup name.'
		RAISERROR(@ScriptMsg, 16, 1)
		RETURN
	END
	--
	SET @TableToMove = LTRIM(RTRIM(@TableToMove))
	--
	IF RIGHT(@TableToMove, 1) = ']'
        	SET @TableToMove = LEFT(@TableToMove, LEN(@TableToMove) - 1)
	--
	IF LEFT(@TableToMove, 1) = '['
        	SET @TableToMove = RIGHT(@TableToMove, LEN(@TableToMove) - 1)
	--
	-- Verify that the table exists
	--
	IF NOT EXISTS( SELECT *
			 FROM INFORMATION_SCHEMA.TABLES
			WHERE TABLE_NAME = @TableToMove
			  AND TABLE_TYPE = 'BASE TABLE' )
	BEGIN
		--
        	SET @ScriptMsg = N'The table '+ @TableToMove + N' does not exist in database ' +
				 @DatabaseName + N', on server ' + @ServerName + N'. Please provide a valid table name.'
		RAISERROR(@ScriptMsg, 16, 1)
		RETURN
		--
	END
	--
	-- Confirm that the target file group is not read-only.
	--
	IF FILEGROUPPROPERTY(FILEGROUP_NAME(@TargetFileGroupID), 'IsReadOnly') = 1
	BEGIN
		--
        	SET @ScriptMsg = N'The taget file group (i.e., with file group id = ' + CAST(@TargetFileGroupID AS VARCHAR(32)) + N') is read-only. Aborting table move.'
		RAISERROR(@ScriptMsg, 16, 1)
		RETURN
	END
	--
	-- If the script makes it to this point, all validations passed and  
	-- the table can be moved to the target filegroup
	--
	-- See if the table has a clustered index (CI)
	--
	SET @TableHasCI = OBJECTPROPERTY(OBJECT_ID(@TableToMove), 'TableHasClustIndex')
	--
	-- If no CI, check for an identity column. If one exists, create the CI 
	-- on the target filegroup using the identity column.  After the table
	-- is moved, remove the CI.   
	-- If no identity column exists, check for a primary key.  If one exists,
	-- create the CI on the new filegroup using the PK.
	-- If the table does not have an identity column or a PK, then
	-- create a new identity column for the table and create the CI on the target
	-- filegroup using the identity. Then remove the CI and the identity column.
	--
	IF @TableHasCI = 0
	BEGIN
		--
		SET @TableHasIdent = OBJECTPROPERTY(OBJECT_ID(@TableToMove), 'TableHasIdentity')
		--
		IF @TableHasIdent = 0
		BEGIN
			--
			SET @TableHasPK = OBJECTPROPERTY(OBJECT_ID(@TableToMove), 'TableHasPrimaryKey')
			SET @TableHasUQ = OBJECTPROPERTY(OBJECT_ID(@TableToMove), 'TableHasUniqueCnst')
			--
			-- If the table has no PK/UQ or clustered index, then create an identity
			-- column on it. The CI will be created using the new column.
			IF @TableHasPK = 0 AND @TableHasUQ = 0
			BEGIN
				--
				EXEC(N' ALTER TABLE [' + @TableToMove + N'] ADD [This_Is_My_Ident_Col_Name] BIGINT IDENTITY (1, 1) ')
				SET @IdentColName = 'This_Is_My_Ident_Col_Name'
				--
				-- Create the CI on the identity column. The identity column may be non-unique
				-- due to reseeding so CI is created as NON-UNIQUE.
				--
				EXEC (N'CREATE CLUSTERED INDEX [This_Is_My_Clustered_Index_Name] ON 
					[' + @TableToMove + N']([' + @IdentColName + ']) ON
					[' + @FGName + N']')
				--
				-- The table has been moved.  Remove the CI.
				--
				EXEC(N'DROP INDEX [' + @TableToMove + N'].[This_Is_My_Clustered_Index_Name]')
				--
				-- Drop the temporary identity column
				--
				EXEC(N' ALTER TABLE [' + @TableToMove + N']
                                DROP COLUMN [This_Is_My_Ident_Col_Name] ')
				--
			END
			--
                	ELSE
                	BEGIN
				--
				-- If the table has a PK/UQ, then create the
				-- CI on the columns of the PK/UQ.
				--
				-- Get the columns used for the PK/UQ.
				--
				SELECT @KeyName = CONSTRAINT_NAME
				  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WITH (NOLOCK)
				 WHERE TABLE_NAME = @TableToMove
				   AND CONSTRAINT_TYPE = 'PRIMARY KEY'
				--
				-- Get the name of the PK/UQ
				--
	                        IF @@ROWCOUNT = 0
					--
					SELECT TOP 1 @KeyName = CONSTRAINT_NAME
					  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WITH (NOLOCK)
					 WHERE TABLE_NAME = @TableToMove
					   AND CONSTRAINT_TYPE = 'UNIQUE'
					--
				--
				INSERT INTO #ttColTable (ColName, IdxOrder)
				SELECT COL_NAME(OBJECT_ID(@TableToMove), colid)
				       --
				       -- Determine whether column is indexed ASC or DESC
				       --
				      ,CASE WHEN INDEXKEY_PROPERTY( OBJECT_ID(@TableToMove)
								   ,INDEXPROPERTY( OBJECT_ID(@TableToMove)
										  ,@KeyName
										  ,'IndexID' )
								   ,keyno
								   ,'IsDescending' ) = 1
					    THEN 'DESC'
					    ELSE 'ASC'
				       END
				  FROM sysindexkeys
				 WHERE id = OBJECT_ID(@TableToMove)
				   AND indid = INDEXPROPERTY(OBJECT_ID(@TableToMove), @KeyName, 'IndexID')
				--
                	        IF @@ROWCOUNT > 0
					--
					SET @i = 1
					--
				--
                        	SET @ColList = N''
				--
        	                WHILE EXISTS(SELECT * FROM #ttColTable WHERE Idx = @i)
                	        BEGIN
					--
					SELECT @ColList = @ColList + N'[' + ColName + N'] ' + IdxOrder + N' ,'
					  FROM #ttColTable
					 WHERE Idx = @i
					--
					SET @i = @i + 1
					--
				END
				--
                	        SET @ColList = LEFT(@ColList, LEN(@ColList) - 1)
				--
	                        -- Create the CI on the primary key columns. The CI is not
        	                -- created as unique, since there could be duplicate entries
                	        -- if the PK/UQ was created with NOCHECK.
				--
				EXEC(N'CREATE CLUSTERED INDEX [Temp_Clustered_Index] ON
				    [' + @TableToMove + N'](' + @ColList + ') ON
				    [' + @FGName + N']')
				--
	                        -- The CI (and table) has been moved so the CI can be dropped
				--
				EXEC(N'DROP INDEX [' + @TableToMove + N'].[Temp_Clustered_Index]')
	                END
			--
		END
		--
		ELSE
		BEGIN
			--
			-- In this case the table has an identity. The CI
			-- is created on the identity column and then dropped.
			--
			SELECT @IdentColName = COLUMN_NAME
			  FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK)
			 WHERE TABLE_NAME = @TableToMove
			   AND COLUMNPROPERTY(OBJECT_ID(@TableToMove), COLUMN_NAME, 'IsIdentity') = 1
			--
			EXEC(N'CREATE CLUSTERED INDEX [Temp_Clustered_Index] ON
			    [' + @TableToMove + N']([' + @IdentColName + ']) ON
			    [' + @FGName + N']')
			--
			-- The table has been moved, so drop the CI.
			--
			EXEC(N'DROP INDEX [' + @TableToMove + N'].[Temp_Clustered_Index]')
			--
		END
		--
	END
	--
	ELSE
	BEGIN
		--
		-- In this case, the table has an existing clustered index (CI).
		-- First we get the name of the existing CI, then drop and recreate
		-- it on the target filegroup using the same columns and order used in the
		-- existing CI.
		-- If the CI is also a PK/UQ/unique index, the PK/UQ/UI is dropped
		-- then recreate PK/UQ/UI as CLUSTERED on the target filegroup.
		-- If the CI is non-unique then it is dropped and recreated on the
		-- target filegroup.
		--
		SELECT @CIName = [name]
		  FROM sysindexes WITH (NOLOCK)
		 WHERE id = ( SELECT OBJECT_ID
				 FROM SYS.OBJECTS
				WHERE NAME = @TableToMove )
		  AND indid = 1
		--
		DELETE FROM #ttColTable
		--
		INSERT INTO #ttColTable (ColName, IdxOrder)
		SELECT COL_NAME(@TableToMoveObjID,colid)
		       --
		       -- Determine whether the PK columns are indexed ASCENDING or DESCENDING
		       --
		      ,CASE WHEN INDEXKEY_PROPERTY( @TableToMoveObjID
						   ,INDEXPROPERTY( @TableToMoveObjID
								  ,@CIName
								  ,'IndexID' )
						   ,keyno
						   ,'IsDescending' ) = 1
			    THEN 'DESC'
			    ELSE 'ASC'
		       END
		 FROM sysindexkeys WITH (NOLOCK)
		WHERE id = @TableToMoveObjID
		  AND indid = 1
		ORDER BY keyno ASC
		--
		SELECT @i = MIN(Idx)
		  FROM #ttColTable
		--
		SET @ColList = N''
		--
		WHILE EXISTS(SELECT * FROM #ttColTable WHERE Idx = @i)
		BEGIN
			--
			SELECT @ColList = @ColList + N'[' + ColName + N'] ' + IdxOrder + N' ,'
			  FROM #ttColTable
			 WHERE Idx = @i
			--
			SET @i = @i + 1
		END
		--
		SET @ColList = LEFT(@ColList, LEN(@ColList) - 1)
		--
		-- Check whether the clustered index is the PK, a unique constraint (UQ),
		-- or a unique index (UI) that is not a PK or UQ.  If the CI is none of
		-- the above, it is a non-unique clustered index and will be dropped and
		-- recreated on the target filegroup.
		--
		IF OBJECTPROPERTY(OBJECT_ID(@CIName), 'IsPrimaryKey') = 1 OR
		   OBJECTPROPERTY(OBJECT_ID(@CIName), 'IsUniqueCnst') = 1 OR
		   INDEXPROPERTY(OBJECT_ID(@TableToMove), @CIName, 'IsUnique') = 1
		BEGIN
			--
			IF OBJECTPROPERTY(OBJECT_ID(@CIName), 'IsPrimaryKey') = 1
			BEGIN
				--
				EXEC(N' ALTER TABLE [' + @TableToMove + N']
				DROP CONSTRAINT [' + @CIName + N'] ')
				--
				EXEC(N' ALTER TABLE [' + @TableToMove + N']
				WITH NOCHECK ADD CONSTRAINT [' + @CIName + N']
				PRIMARY KEY CLUSTERED (' + @ColList + N')
				ON [' + @FGName + N']')
			END
			ELSE
			BEGIN
				--
				IF OBJECTPROPERTY(OBJECT_ID(@CIName), 'IsUniqueCnst') = 1
                        	BEGIN
					--
					EXEC(N' ALTER TABLE [' + @TableToMove + N']
					DROP CONSTRAINT [' + @CIName + N'] ')
					--
					EXEC(N' ALTER TABLE [' + @TableToMove + N']
					WITH NOCHECK ADD CONSTRAINT [' + @CIName + N']
					UNIQUE CLUSTERED (' + @ColList + N')
					ON [' + @FGName + N']')
				END
				ELSE -- CI is a unique index
				BEGIN
					--
					SET @IsPadIndex = INDEXPROPERTY(OBJECT_ID(@TableToMove), @CIName, 'IsPadIndex')
					--
					EXEC(N'DROP INDEX [' + @TableToMove + N'].[' + @CIName + N']')
					--
					-- Recreate the unique CI with the same columns and order as 
					-- defined on the original table
					--
					IF @IsPadIndex = 1
						--
						EXEC(N'CREATE UNIQUE CLUSTERED INDEX [' + @CIName + N']
						ON [' + @TableToMove + N'](' + @ColList + N')
						WITH PAD_INDEX
						ON [' + @FGName + N']')
						--
					ELSE
						--
						EXEC(N'CREATE UNIQUE CLUSTERED INDEX [' + @CIName + N']
						ON [' + @TableToMove + N'](' + @ColList + N')
						ON [' + @FGName + N']')
						--
				END
				--
			END
			--
		END
		--
		ELSE
		BEGIN
			--
			-- In this case the CI is not a PK, UQ, or UI, so the CI is 
			-- dropped and recreated on the target filegroup as a non-unique index
			--
			SET @IsPadIndex = INDEXPROPERTY(OBJECT_ID(@TableToMove), @CIName, 'IsPadIndex')
			--
			EXEC(N'DROP INDEX [' + @TableToMove + N'].[' + @CIName + N']')
			--
			-- Recreate the index with the same columns and column order as 
			-- defined on the original table. In this case the CI is non-unique
			--
			IF @IsPadIndex = 1
				--
				EXEC(N'CREATE CLUSTERED INDEX [' + @CIName + N']
				ON [' + @TableToMove + N'](' + @ColList + N')
				WITH PAD_INDEX
				ON [' + @FGName + N']')
				--
			ELSE
				--
				EXEC(N'CREATE CLUSTERED INDEX [' + @CIName + N']
				ON [' + @TableToMove + N'](' + @ColList + N')
				ON [' + @FGName + N']')
				--
		END
		--
	END
	--
END
GO
