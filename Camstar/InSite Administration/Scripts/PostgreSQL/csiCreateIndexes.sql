DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCreateIndexes')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCreateIndexes;
 	END IF;
END $$;

CREATE PROCEDURE csiCreateIndexes(
p_Scope varchar(10),
p_action varchar(10),
p_DBType varchar(10))
language plpgsql
as $$
DECLARE
	i_ErrorNumber TEXT;
    i_ErrorSeverity TEXT;
    i_ErrorState TEXT;

    v_IndexName VARCHAR(30);
    v_ColumnName VARCHAR(255);
    v_TableName VARCHAR(30);
    --v_IsUnique CHAR(1);
    v_IsUnique INT;
    v_SortOrder VARCHAR(4);
    v_SQLStmt VARCHAR(2000);
    v_UseCaseSensitiveRef CHAR(1);
    v_UseUpper CHAR(1);
    v_ErrMsg VARCHAR(2048);
    -- v_Online1 VARCHAR(256) := '';
    -- v_Online2 VARCHAR(256) := '';
    -- v_Banner VARCHAR(256) := NULL;
    --v_IsDisable CHAR(1);
    v_IsDisable INT;
   
   	-- temp variables
    t_DBColumnId NUMERIC := 0;
    t_ColumnName VARCHAR(255);
    t_TableName VARCHAR(30);
    t_IsUnique INT;
    t_SortOrder VARCHAR(4);
    t_IsDisable INT;
   
    t_WorkspaceCode VARCHAR(5);

    n_ExistsCheck NUMERIC := 0;
    n_ColumnLoop NUMERIC := 0;
	n_ErrLocator NUMERIC := 0;
    n_IndexCount NUMERIC := 0;
    n_DBColumnId NUMERIC := 0;
    n_FieldCount NUMERIC := 0;

    si_IndexColumnsDefined SMALLINT;

    rc_IndexCursor REFCURSOR;
    Index_Column_Cur REFCURSOR;

    v_Unique VARCHAR(10);
	
	v_WorkspaceCode VARCHAR(5);
	v_CustomScriptName VARCHAR(30);
	WorkspaceCursor REFCURSOR;

	
	
BEGIN
	n_ExistsCheck := 0;
	n_ColumnLoop := 0;
	n_IndexCount := 0;
	si_IndexColumnsDefined := 0;
	n_FieldCount := 0;
	--v_Online1 := '';
	--v_Online2 := '';
	--v_Banner := NULL;

	--
	n_ErrLocator := 10;
	--
	SELECT COUNT(indexname) 
	INTO n_IndexCount
	FROM pg_indexes
	WHERE tablename = 'dbindexentries' AND indexname = 'dbindexentries_nui1';
	
	IF n_IndexCount = 0 THEN
        v_SQLStmt := 'CREATE INDEX dbindexentries_nui1 ON dbindexentries (dbindexid)';
        EXECUTE v_SQLStmt;
		n_ErrLocator := 15;
        RAISE NOTICE 'Index "dbindexentries_nui1" created successfully';
    END IF;
	--
	n_IndexCount := 0;
	--
	n_ErrLocator := 20;
	--
	SELECT COUNT(indexname) 
	INTO n_IndexCount
	FROM pg_indexes
	WHERE tablename = 'dbindexentries' AND indexname = 'dbindexdefinition_ui1';
	
	IF n_IndexCount = 0 THEN
        v_SQLStmt := 'CREATE INDEX dbindexdefinition_ui1 ON dbindexentries (dbindexid)';
        EXECUTE v_SQLStmt;
		n_ErrLocator := 25;
        RAISE NOTICE 'Index "dbindexdefinition_ui1" created successfully';
	END IF;
	--
	n_IndexCount := 0;
	--
	n_ErrLocator := 30;
	--
	SELECT COUNT(indexname) 
	INTO n_IndexCount
	FROM pg_indexes
	WHERE tablename = 'dbindexentries' AND indexname = 'dbindexdefinition_nui1';
	
	IF n_IndexCount = 0 THEN
        v_SQLStmt := 'CREATE INDEX dbindexdefinition_nui1 ON dbindexentries (dbindexid)';
        EXECUTE v_SQLStmt;
		n_ErrLocator := 35;
        RAISE NOTICE 'Index "dbindexdefinition_nui1" created successfully';
	END IF;
	--
	n_ErrLocator := 40;
	--
	SELECT TValue
	INTO v_UseCaseSensitiveRef
	FROM INSITESITEINFO
	WHERE TName = 'CaseSensitiveRefInfo';
	--
	IF (UPPER(p_Scope) = 'ALL') THEN -- CREATE/REPLACE ALL INDEXES DEFINED IN DBINDEXDEFINITION
		open rc_IndexCursor FOR SELECT DBIndexName, DTD.DBTableName
							FROM DBINDEXDEFINITION DID
							INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
							ORDER BY DTD.DBTableName, DID.DBIndexName;
	ELSE -- CREATE MISSING/NEW INDEXES ONLY
		open rc_IndexCursor FOR 
					-- Get indexes where any columns in existing index have been added
					-- Get new indices
					SELECT DISTINCT DID.DBINDEXNAME, DTD.DBTableName
					FROM DBINDEXDEFINITION DID
					INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
					INNER JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID
					INNER JOIN DBColumns DTC ON DTC.DBColumnID = DIE.DBColumnID
					left join pg_catalog.pg_indexes IND on UPPER(DID.DBIndexName) = UPPER(IND.indexname)					    
					left join information_schema.tables TAB on UPPER(TAB.table_name) = UPPER(IND.Tablename) and UPPER(IND.schemaname) = UPPER(TAB.table_schema)
					left join information_schema.columns COL on UPPER(TAB.table_schema) = UPPER(COL.table_schema) and UPPER(TAB.table_name) = UPPER(COL.table_name)
						and upper(col.column_name ) = upper(DTC.columnname)
					left join (SELECT
						t.relname AS table_name,
						i.relname AS index_name,
						a.attname AS column_name,
						idx.indisunique AS is_unique,
						idx.indisprimary AS is_primary
					from pg_index idx
					join pg_class t ON idx.indrelid = t.oid
					join pg_class i ON idx.indexrelid = i.oid
					join pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(idx.indkey)
					WHERE
						t.relkind = 'r' -- Filters for tables (r = table)
					) INDC on upper(IND.indexname) = upper(INDC.index_name) and upper(COL.column_name) = upper(INDC.column_name)
					left join (
					SELECT
						t.relname AS table_name,
						i.relname AS index_name,
						idx.indisunique AS is_unique,
						idx.indisprimary AS is_primary
					FROM pg_index idx
					JOIN pg_class t ON idx.indrelid = t.oid
					JOIN pg_class i ON idx.indexrelid = i.oid
					WHERE t.relkind = 'r' -- Filters for tables (r = table)
					) IDX on upper(IDX.index_name) = upper(IND.indexname) 
					WHERE 
					((IND.indexname IS NULL) 
					  OR (INDC.index_name is NULL)  
					  OR coalesce(IDX.is_unique, false) <> (CASE WHEN p_DBType = 'DATASTORE'
						then false -- unique indexs should be rebuilt
						ELSE
						DID.IsUnique = 1  --look for uniqueness changed in OLTP
						END))
					Or DID.IsDisable = 1
					UNION
					-- Get indexes where any columns in the existing index have been deleted
					SELECT 
						DISTINCT DID.DBINDEXNAME, DTD.DBTableName
					FROM pg_catalog.pg_indexes IND 				    
					inner join information_schema.tables TAB on UPPER(TAB.table_name) = UPPER(IND.Tablename) and UPPER(IND.schemaname) = UPPER(TAB.table_schema)
					inner join information_schema.columns COL on UPPER(TAB.table_schema) = UPPER(COL.table_schema) and UPPER(TAB.table_name) = UPPER(COL.table_name)
					inner join (SELECT
						t.relname AS table_name,
						i.relname AS index_name,
						a.attname AS column_name,
						idx.indisunique AS is_unique,
						idx.indisprimary AS is_primary
					from pg_index idx
					join pg_class t ON idx.indrelid = t.oid
					join pg_class i ON idx.indexrelid = i.oid
					join pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(idx.indkey)
					WHERE
						t.relkind = 'r' -- Filters for tables (r = table)
					) INDC on upper(IND.indexname) = upper(INDC.index_name) and upper(COL.column_name) = upper(INDC.column_name)
					INNER JOIN DBINDEXDEFINITION DID on UPPER(DID.DBIndexName) = UPPER(IND.indexname)	
					INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
					LEFT JOIN DBColumns DTC ON DTC.DBTableID = DTD.DBTableId AND UPPER(DTC.ColumnName) = UPPER(COL.column_name)
					LEFT JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID and DIE.DBColumnId = DTC.DBColumnID
					WHERE (DIE.DBIndexEntryID is NULL)
					ORDER BY
					    DBTableName, DBIndexName;
	END IF;
	--
	n_ErrLocator := 45;
	--
	--OPEN rc_IndexCursor;
	--
	LOOP
		FETCH NEXT FROM rc_IndexCursor INTO v_IndexName, v_TableName;
		EXIT WHEN NOT FOUND;
		--
		open Index_Column_Cur FOR SELECT
							DC.DBColumnId AS ColumnId,
							DTD.DBTableName AS TableName,
							DC.ColumnName AS ColumnName,
							DID.IsUnique AS IsUnique,
							CASE DIE.Sortorder
								WHEN 2 THEN 'DESC'
								ELSE 'ASC'
							END AS SortOrder,
							DID.IsDisable AS IsDisable
						FROM
							DBINDEXENTRIES DIE
						INNER JOIN
							DBINDEXDEFINITION DID ON DIE.DBIndexId = DID.DBIndexId
						INNER JOIN
							DBCOLUMNS DC ON DIE.DBColumnId = DC.DBColumnId
						INNER JOIN
							DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId
						WHERE
							lower(DID.DBIndexName) = lower(v_IndexName)
						ORDER BY
							DTD.DBTableName,
							DIE.DBIndexId,
							DIE.Sequence;
		--
		n_ColumnLoop := 1;
		--
		--OPEN Index_Column_Cur;
		--
	
		LOOP
			FETCH NEXT FROM Index_Column_Cur INTO n_DBColumnId, v_TableName, v_ColumnName, v_IsUnique, v_SortOrder, v_IsDisable;
			EXIT WHEN NOT FOUND;
			--
			si_IndexColumnsDefined := 1;
			--
			IF n_ColumnLoop = 1 THEN -- FOR THE FIRST COLUMN IN THE LIST
				--
				IF (UPPER(p_DBType)='OLTP') THEN
					n_ErrLocator := 50;
					v_Unique := CASE
									WHEN v_IsUnique = 1 THEN 'UNIQUE '
									ELSE ''
								END;
				ELSE
					v_Unique := '';
				END IF;
				--
				v_SQLStmt := 'CREATE ' || v_Unique || 'INDEX IF NOT EXISTS ' || v_IndexName || ' ON '|| v_TableName || '(';
				--
			END IF;
			--
			IF (v_UseCaseSensitiveRef = 'Y') THEN
				v_UseUpper = 'N';
			ELSE
			--
				n_ErrLocator := 55;
				
				SELECT COUNT(*)
				INTO n_FieldCount
				FROM CDOFIELDS
				WHERE DBColumnId = n_DBColumnId
				AND CDOFielddUsageId = 1;
				
				IF n_FieldCount > 0 THEN
					v_UseUpper = 'Y';
				ELSE
					v_UseUpper = 'N';
				END IF;
			--
			END IF;
			--
			IF v_UseUpper = 'Y' THEN
				v_ColumnName := UPPER(v_ColumnName);
			ELSE
				v_ColumnName := v_ColumnName;
			END IF;
			--
			v_SQLStmt := v_SQLStmt || v_ColumnName || ' ' || v_SortOrder || ',';
			--
			n_ColumnLoop := n_ColumnLoop + 1;
			--
			-- assign the current data to temp variable 
			t_DBColumnId := n_DBColumnId;
		    t_ColumnName := v_ColumnName;
		    t_TableName := v_TableName;
		    t_IsUnique := v_IsUnique;
		    t_SortOrder := v_SortOrder;
		    t_IsDisable := v_IsDisable;
		END LOOP;
		--
		-- assign the temp variable back to original variable
		n_DBColumnId := t_DBColumnId;
		v_ColumnName := t_ColumnName;
		v_TableName := t_TableName;
		v_IsUnique := t_IsUnique;
		v_SortOrder := t_SortOrder;
		v_IsDisable := t_IsDisable;
		--
		CLOSE Index_Column_Cur;
		--
		v_SQLStmt := substring(v_SQLStmt, 1, length(v_SQLStmt) - 1) || ') '; -- CLOSE THE COLUMN LIST
		--
		IF si_IndexColumnsDefined = 1 THEN
			--
			n_ErrLocator := 60;
			--
			SELECT COUNT(indexname)
			INTO n_ExistsCheck
			FROM pg_indexes
			WHERE lower(indexname) = lower(v_IndexName);
			--
			IF n_ExistsCheck > 0 then
				--
				n_ErrLocator := 65;
				--
				-- ADD DROP_EXISTING CLAUSE TO STATEMENT IF THE INDEX 
				-- EXISTS AND SCOPE IN 'NEW' OR 'ALL'
				--
				IF v_IsDisable = 1 THEN
					--
					v_SQLStmt := 'DROP INDEX ' || v_IndexName || ' ON ' || v_TableName;
					RAISE NOTICE 'Index % will be dropped', v_IndexName;
					--
				ELSE
					--
					--v_SQLStmt := v_SQLStmt || CASE 
					--	WHEN v_Online2 IS NULL THEN ')'
					--	ELSE v_Online2
					--END;
					v_SQLStmt := v_SQLStmt;

					RAISE NOTICE 'Index % will be dropped and recreated', v_IndexName;
					--
				END IF;
			ELSE
				--
				IF v_IsDisable = 1 THEN
					--
					v_SQLStmt = '';
				ELSE
					-- v_SQLStmt := v_SQLStmt || ' ' || v_Online1;
					v_SQLStmt := v_SQLStmt;
					--
				END IF;
				--
			END IF;
			--
			IF UPPER(p_Action) = 'EXECUTE' then
			--
				BEGIN
					n_ErrLocator := 70;

					EXECUTE v_SQLStmt;
					-- Check if it was a disable operation and the index already existed
					IF v_IsDisable = 1 AND n_ExistsCheck > 0 THEN
						RAISE NOTICE 'Index % dropped successfully', v_IndexName;
					ELSIF v_IsDisable <> 1 THEN
						RAISE NOTICE 'Index % created successfully', v_IndexName;
					END IF;
--				EXCEPTION
--					WHEN OTHERS THEN
--						-- Catch and handle exceptions
--						GET STACKED DIAGNOSTICS
----							i_ErrorSeverity = RETURNED_SQLSTATE,
----							i_ErrorState = PG_EXCEPTION_CONTEXT,
--							i_ErrorNumber = RETURNED_SQLSTATE;
--						RAISE NOTICE 'ErrorNum: %', i_ErrorNumber;
--						-- Check for a specific error code (1913 in this case)
--						IF i_ErrorNumber IN('1913') THEN
--							RAISE NOTICE 'Index "%" already exists. Skipping...', v_IndexName;
--						END IF;
				END;
				--
			ELSE
				--
				IF UPPER(p_Action) = 'REPORT' THEN
					--
					RAISE NOTICE '%', v_SQLStmt;
					--
				END IF;
				--
			END IF;
			--
			v_SQLStmt := NULL;
			v_IndexName := NULL;
			n_FieldCount := 0;
			n_ExistsCheck := 0;
			--
			si_IndexColumnsDefined := 0;
			--
		ELSE
			--
			RAISE NOTICE 'WARNING - No columns in table "%" defined for index "%". Please contact System Administrator.', v_TableName, v_IndexName;
			--
		END IF;
		
	END LOOP;
	CLOSE rc_IndexCursor;
	----
	-- This section will check for the existence of any workspace specific index creation scripts
	-- and execute them if found.
	RAISE NOTICE 'Processing custom scripts...';
	open WorkspaceCursor FOR SELECT WS.WorkspaceCode
								FROM Workspace WS
								WHERE WS.IsActive = 1
								ORDER BY WS.Sequence;
	--
	n_ErrLocator := 200;
	--
	--OPEN WorkspaceCursor;
	--
	n_ErrLocator := n_ErrLocator + 5;
	LOOP
		--
		FETCH NEXT FROM WorkspaceCursor INTO v_WorkspaceCode;
		EXIT WHEN NOT FOUND;
		--
		n_ErrLocator := n_ErrLocator + 1;
		--
		v_CustomScriptName := 'Workspace_' || v_WorkspaceCode || '_CreateIndexes';
		--
		-- Check to see if the stored procedure EXISTS
		SELECT COUNT(*) INTO n_ExistsCheck FROM pg_proc WHERE UPPER(proname) = UPPER(v_CustomScriptName);
		--
		-- If it does, then call it.
		IF n_ExistsCheck = 1 THEN
			--
			CALL v_CustomScriptName(p_Scope, p_Action, p_DBType);
			n_ErrLocator := n_ErrLocator + 1;
			--
		END IF;
		--
		--t_WorkspaceCode := v_WorkspaceCode;
		--
	END LOOP;
	CLOSE WorkspaceCursor;
	--
	n_ErrLocator := n_ErrLocator + 5;
	--
	-- End of Custom Index Creation Script section
	----
	--
	BEGIN
		-- Create Metadata Indices
		n_ErrLocator := 75;
		v_IndexName := 'CDO_Parent_Idx';
		
		v_SQLStmt := 'CREATE UNIQUE INDEX IF NOT EXISTS ' || v_IndexName || ' ON CDODefinition (ParentCDOID ASC, CDODefID ASC)';
		
		SELECT COUNT(indexname) INTO n_ExistsCheck
		FROM pg_indexes
		WHERE lower(indexname) = lower(v_IndexName) AND LOWER(tablename) = LOWER('CDODefinition');
		
		IF n_ExistsCheck > 0 THEN
			-- ADD DROP_EXISTING CLAUSE TO STATEMENT SINCE THE INDEX EXISTS
			-- EXISTS AND SCOPE IN 'NEW' OR 'ALL'
			n_ErrLocator := 80;
			
			-- v_SQLStmt := v_SQLStmt || CASE WHEN COALESCE(v_Online2, 'XXX') = 'XXX' THEN '' ELSE v_Online2 END;
			v_SQLStmt := v_SQLStmt;
			
			RAISE NOTICE 'Index % will be dropped and recreated', v_IndexName;
		ELSE
			-- v_SQLStmt := v_SQLStmt || ' ' || v_Online1;
			v_SQLStmt := v_SQLStmt;
		END IF;
		
		n_ErrLocator := 85;
		
		IF UPPER(p_Action) = 'EXECUTE' THEN
			BEGIN
				-- Attempt to execute the SQL statement
				EXECUTE v_SQLStmt;
				RAISE NOTICE 'Index % created successfully', v_IndexName;
			EXCEPTION
				WHEN others THEN
					-- Catch and handle exceptions
					GET STACKED DIAGNOSTICS
--						i_ErrorSeverity = RETURNED_SQLSTATE,
--						i_ErrorState = PG_EXCEPTION_CONTEXT,
						i_ErrorNumber = RETURNED_SQLSTATE;
					RAISE NOTICE 'ErrorNum: %, ErrLoc: %, ErrMsg: %', i_ErrorNumber, n_ErrLocator, SQLERRM;
					-- Check for a specific error code (1913 in this case)
					IF i_ErrorNumber IN('1913') THEN
						RAISE NOTICE 'Index "%" already exists. Skipping...', v_IndexName;
					END IF;
			END;
		ELSE
			BEGIN
				IF UPPER(p_Action) = 'REPORT' THEN
					RAISE NOTICE '%', v_SQLStmt;
				END IF;
			END;
		END IF;
	END;
	EXCEPTION
	    WHEN OTHERS THEN
        v_ErrMsg := 'Error creating indexes - ErrLoc: ' || CAST(n_ErrLocator AS VARCHAR(8)) || ' ErrMsg: ' || SQLERRM;
        RAISE NOTICE '%, ErrorSeverity: %, ErrorState: %', v_ErrMsg, SQLSTATE, SQLSTATE;
END;
$$;