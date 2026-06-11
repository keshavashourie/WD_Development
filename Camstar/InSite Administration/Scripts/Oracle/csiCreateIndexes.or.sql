create or replace PROCEDURE csiCreateIndexes ( p_Scope		IN VARCHAR2
					      ,p_Action		IN VARCHAR2
					      ,p_DBType		IN VARCHAR2 )
IS
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
-- Name				                             Date		      Action
-- --------------------------	                 ----------	      ----------------
-- N/A                                                04/21/2005     SPR S9321  - Added DBType parameter
-- N/A                                                03/13/2006     SPR S10630 - Employee table index to use UPPER
-- Bill Lippard                                     12/04/2006     SPR S9984  - Added copyright notice to procedure
-- Bill Lippard                                      04/23/2007     SPR S9984  - Updated copyright notices
-- N/A                                                08/22/2007     SPR S10648 - Added logic to determine whether or not to use UPPER
-- Purushotham Neelakantachar	         01/27/2009     SPR S12761 - Added hints to SELECT statements to improve performance
-- Purushotham Neelakantachar	         02/04/2009     SPR S14883 - Added logic to handle indexes with no columns
--                                                      - Added code to handle exceptions
--                                                      - Implemented coding standards and indented the code
--
-- Purushotham Neelakantachar           01/28/2010     - DBUPdate Performance Enhancement
--                                                      - Added code to create indexes online
--                                                      - Modified logic to display meaningful error messages
--
-- Alex Lind                                        04/27/2010     SPR S17625  - Add Version check to Handle Standard Edition without ONLINE function
--
-- Purushotham Neelakantachar           05/24/2010     SPR S17625  - Implemented coding standards indented the code
--
-- Oleg Kirasov                                   11/16/2010     - Update the Index cursor to retrieve
--                                                      - existing indexes when new fields have been adde
--                                                      - or the existing fields have been deleted
--                                                      - Update the logic to add DROP_EXISTING = ON option for all modes
--
-- Dan Maloney                                   12/03/2018     Bug 17200	- Bug fix for V7 SU8 to change WHERE clause to avoid ORA-01652 that sometimes occurs on
--                                                      - Oracle instances running on VMs that are using virtual disk on the SAN in the Harris Building
--                                                      - no root cause found but it is suspected it is disk latency or bad secotrs that started
--                                                      - manifesting an issue in the sql statements WHERE (DIE.DBIndexEntryID is NULL) statement that
--                                                      - is in a sql statenmet that is executed after ELSIF ( UPPER(p_Scope) = 'NEW' ) -- CREATE MISSING/NEW INDEXES ONLY
--
-- Ramesh Nagamalli                          04/10/2012     Bug #36763   - Add Indices for Metadata Tables
--
-- Dan Maloney                                  10/10/2019     CPR  46992  - Remove NOLOGGING clause from create index DDM as customers are having issues if databae fails and
--                                                      -  needs to be restoed and recovered to a point in time after the creatye indexes with NOLOGGING occurred
--                                                      -  We may lose a bit of performance creating indexes by removing the NOLOGGING in the create index DML but
--                                                      -  we eliminate the destability that can occur if the customer does a DBUpdate that creates indexes with NOLOGING and
--                                                      -  the customer fails to do an immedaite cold backup after the DBUpdate
--
--Milind Lokhande                               12/15/2021     CPR  45706  - Add Column IsDisable for Dropping index WITHOUT recreating them.
--
-- Nick Aghazarian	                            04/26/2016     Task 38998 Support for custom/manual index creation
--
-- Reshma Mindhe                              04/29/2022     CPR 252106:V(8) PR182855: TAC9905251: Function Based indexes rebuilding unnecessarily on DBUpdate.
--
-- Eugene Androsov			                12/13/2023     Added Index for CLFEventMap table. CPR 368740:(2404) PR? - TAC10849714 API startup fails.
--
-- Dan Maloney                                  03/26/2024     US 381238 Modify csiCreateIndexes for Oracle to add new PK Index for Labels.LabelId
--      
-- Copyright Siemens 2024  
------------------------------------------------------------------------------------------------
	--
	c_IsNullable						CHAR(1);
	v_ExistingIndexName			VARCHAR2(30);
	v_IndexName						VARCHAR2(30);
	v_ColumnName					VARCHAR2(255);
	v_ConstraintName				VARCHAR2(64);
	v_TableName					VARCHAR2(30);
	v_Owner							VARCHAR2(30);
	v_IsUnique						CHAR(1);
	v_SortOrder						VARCHAR2(4);
	v_SQLStmt						VARCHAR2(2000);
	v_UseCaseSensitiveRef		CHAR(1);
	v_UseUpper						CHAR(1);
	v_Online							VARCHAR2(10) := '';
	--
	n_ExistsCheck					NUMBER := 0;
	n_ColumnLoop					NUMBER := 0;
	n_ErrLocator						NUMBER;
	n_IndexCount					NUMBER := 0;
	n_EditionCount					NUMBER := 0;
	
	-- Custom index creation 
	v_WorkspaceCode				VARCHAR(5);
	v_CustomScriptName 		VARCHAR(30);
	v_IsDisable						CHAR(1);
	
	CURSOR WorkspaceCursor IS 
	SELECT 
		WS.WorkspaceCode
	FROM Workspace WS
	WHERE WS.IsActive = 1
	ORDER BY WS.Sequence;
	--
	b_IndexColumnsDefined	BOOLEAN := FALSE;
	--
	TYPE trc_IndexCursor IS REF CURSOR;
	rc_IndexCursor		trc_IndexCursor;
	--
	e_KeyAlreadyIndexed		EXCEPTION;
	e_IndexExists			EXCEPTION;
	--
	PRAGMA EXCEPTION_INIT(e_KeyAlreadyIndexed,-1408);
	PRAGMA EXCEPTION_INIT(e_IndexExists,-0955);
	--
	v_Unique			VARCHAR2(10);
	--
	CURSOR Index_Column_Cur ( p_IndexName	VARCHAR2 )
	IS
	SELECT 
		DC.DBColumnId			ColumnId
		,DTD.DBTableName			TableName
		,DC.ColumnName			ColumnName
		,DID.IsUnique			IsUnique
		,DECODE(DIE.SortOrder,2,' DESC')	SortOrder
		,DID.IsDisable  IsDisable
	FROM DBINDEXENTRIES		DIE
		,DBINDEXDEFINITION	DID
		,DBCOLUMNS		DC
		,DBTABLEDEFINITION	DTD
	WHERE DIE.DBIndexId = DID.DBIndexId
	AND DIE.DBColumnId = DC.DBColumnId
	AND DID.DBTableDefId = DTD.DBTableId
	AND DID.DBIndexName = p_IndexName
	ORDER BY DTD.DBTableName
		,DIE.DBIndexId
		,DIE.Sequence;
	--
	CURSOR IsNameField_Cur (p_DBColumnId	NUMBER)
	IS
	SELECT 
		'Y'
	FROM CDOFIELDS
	WHERE DBColumnId = p_DBColumnId
	AND CDOFieldUsageId = 1;
	--
BEGIN
	--
	-- Check for the installed Oracle Product Edition
	--
	n_ErrLocator := 5;
	--
	SELECT COUNT(Banner)
	INTO n_EditionCount
	FROM v$VERSION
	WHERE Banner LIKE '%Enterprise%';
	--
    IF ( n_EditionCount <> 0 )
	THEN
		--
		v_Online := 'ONLINE';
		--
    END IF;
	--
	n_ErrLocator := 10;
	--
	SELECT COUNT(Index_Name)
	INTO n_IndexCount
	FROM USER_IND_COLUMNS
	WHERE Table_Name = 'DBINDEXENTRIES'
	AND Column_Name = 'DBINDEXID';
	--
	IF ( n_IndexCount = 0 )
	THEN
		--
		n_ErrLocator := 15;
		--
		EXECUTE IMMEDIATE 'CREATE INDEX DBINDEXENTRIES_NUI1 ON DBINDEXENTRIES ( DBINDEXID ) '||v_Online;
		--
	END IF;
	--
	n_IndexCount := 0;
	--
	n_ErrLocator := 20;
	--
	SELECT COUNT(Index_Name)
	INTO n_IndexCount
	FROM USER_IND_COLUMNS
	WHERE Table_Name = 'DBINDEXDEFINITION'
	AND Column_Name = 'DBINDEXNAME';
	--
	IF ( n_IndexCount = 0 )
	THEN
		--
		n_ErrLocator := 25;
		--
		EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX DBINDEXDEFINITION_UI1 ON DBINDEXDEFINITION ( DBINDEXNAME ) '||v_Online;
		--
	END IF;
	--
	n_IndexCount := 0;
	--
	n_ErrLocator := 30;
	--
	SELECT COUNT(Index_Name)
	INTO n_IndexCount
	FROM USER_IND_COLUMNS
	WHERE Table_Name = 'DBINDEXDEFINITION'
	AND Column_Name = 'DBTABLEDEFID';
	--
	IF ( n_IndexCount = 0 )
	THEN
		--
		n_ErrLocator := 35;
		--
		EXECUTE IMMEDIATE 'CREATE INDEX DBINDEXDEFINITION_NUI1 ON DBINDEXDEFINITION ( DBTABLEDEFID ) '||v_Online;
		--
	END IF;
	--
	n_ErrLocator := 40;
	--
	SELECT TValue
	INTO v_UseCaseSensitiveRef
	FROM INSITESITEINFO
	WHERE TName = 'CaseSensitiveRefInfo';
	--
	IF ( UPPER(p_Scope) = 'ALL' ) -- CREATE/REPLACE ALL INDEXES DEFINED IN DBINDEXDEFINITION
	THEN
		--
		OPEN rc_IndexCursor FOR
		'SELECT DBIndexName '||CHR(10)||
		'     	 ,DTD.DBTableName '||CHR(10)||
		'		,DID.IsDisable '||CHR(10)||
		'  FROM DBINDEXDEFINITION	DID '||CHR(10)||
		'      ,DBTABLEDEFINITION	DTD '||CHR(10)||
		' WHERE DID.DBTableDefId = DTD.DBTableId '||CHR(10)||
		'ORDER BY DTD.DBTableName '||CHR(10)||
		'	 ,DID.DBIndexName';
		--
	ELSIF ( UPPER(p_Scope) = 'NEW' ) -- CREATE MISSING/NEW INDEXES ONLY
	THEN
		--
		OPEN rc_IndexCursor FOR
        -- Get indexes where any columns in existing index have been added
        -- Get new indices
        'SELECT DISTINCT DBINDEXNAME, DBTableName,IsDisable FROM ( '||CHR(10)||
        'SELECT DID.DBINDEXNAME '||CHR(10)||
        '   ,DTD.DBTableName '||CHR(10)||
		'		,DID.IsDisable '||CHR(10)||
        'FROM DBINDEXDEFINITION DID '||CHR(10)||
        '  INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId '||CHR(10)||
        '  INNER JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID '||CHR(10)||
        '  INNER JOIN DBColumns DTC ON DTC.DBColumnID = DIE.DBColumnID '||CHR(10)||
        '  LEFT JOIN USER_INDEXES IND ON UPPER(DID.DBIndexName) = UPPER(IND.Index_Name) '||CHR(10)||
        '     and UPPER(IND.Table_Name) = UPPER(DTD.DBTableName) '||CHR(10)||
        '  LEFT JOIN USER_IND_COLUMNS INDC on UPPER(INDC.Index_Name) = UPPER(IND.Index_Name) '||CHR(10)||
        '     and UPPER(INDC.Table_Name) = UPPER(IND.Table_Name) '||CHR(10)||
        '     and (UPPER(INDC.Column_Name) = UPPER(DTC.ColumnName) OR (substr(INDC.column_name,0,3) = ''SYS'' AND indc.column_position= Die.sequence)) '||CHR(10)||
        'WHERE ((IND.Index_Name IS NULL) '||CHR(10)||
        'OR (INDC.Index_Name is NULL) '||CHR(10)||
		' OR IND.UNIQUENESS <> (CASE WHEN '''||UPPER(p_DBType)||'''=''DATASTORE'' ' ||CHR(10)||
		' THEN ''NONUNIQUE'' ' ||CHR(10)||
		' ELSE ' ||CHR(10)||
		' DECODE(DID.ISUNIQUE,1,''UNIQUE'',''NONUNIQUE'')'||CHR(10)||
		' END )' ||CHR(10)||
		' ) OR  DID.IsDisable = 1'||CHR(10)||
        '  UNION ALL '||CHR(10)||
        -- Get indexes where any columns in existing index have been deleted
        'SELECT DID.DBINDEXNAME '||CHR(10)||
        '   ,DTD.DBTableName '||CHR(10)||
		'		,DID.IsDisable '||CHR(10)||
        'FROM USER_INDEXES IND '||CHR(10)||
        '   INNER JOIN USER_IND_COLUMNS INDC on UPPER(INDC.Index_Name) = UPPER(IND.Index_Name) '||CHR(10)||
        '     and UPPER(INDC.Table_Name) = UPPER(IND.Table_Name) '||CHR(10)||
        '   INNER JOIN DBINDEXDEFINITION DID ON UPPER(DID.DBIndexName) = UPPER(IND.Index_Name) '||CHR(10)||
        '   INNER JOIN DBTABLEDEFINITION DTD ON DID.DBTableDefId = DTD.DBTableId '||CHR(10)||
        '   LEFT JOIN DBColumns DTC ON DTC.DBTableID = DTD.DBTableId '||CHR(10)||
        '     and UPPER(DTC.ColumnName) = UPPER(INDC.Column_Name) '||CHR(10)||
        '   LEFT JOIN DBIndexEntries DIE ON DIE.DBIndexID = DID.DBIndexID '||CHR(10)||
        '     and DIE.DBColumnId = DTC.DBColumnID '||CHR(10)||
        '   WHERE (NVL(DIE.DBIndexEntryID,-1) = -1 AND substr(INDC.column_name,0,3) <> ''SYS'' )'||CHR(10)||
        ' ) '||CHR(10)||
        ' ORDER BY DBTableName, DBIndexName';
		--
	END IF; -- End IF ( UPPER(p_Scope) = 'ALL' )
	--
	LOOP
		--
		n_ErrLocator := 45;
		--
		FETCH rc_IndexCursor 
		INTO v_IndexName,v_TableName,v_IsDisable;
		EXIT WHEN rc_IndexCursor%NOTFOUND;
		--
		n_ColumnLoop := 1;
		--
		FOR CurrIndexColumn IN Index_Column_Cur ( v_IndexName )
		LOOP
			--
			b_IndexColumnsDefined := TRUE;
			--
			IF ( n_ColumnLoop = 1 ) -- FOR THE FIRST COLUMN IN THE LIST
			THEN
				--
				IF ( UPPER(p_DBType)='OLTP' ) 
				THEN
					--
					n_ErrLocator := 50;
					--
					SELECT DECODE(CurrIndexColumn.IsUnique,1,'UNIQUE ')
					INTO v_Unique
					FROM DUAL;
					--
				ELSE
					--
					v_Unique:='';
					--
				END IF;                
               
				--
				v_SQLStmt := 'CREATE '||v_Unique||'INDEX '||v_IndexName||' ON '||CurrIndexColumn.TableName||'(';
				--
			ELSE
				v_SQLStmt := v_SQLStmt||', '; -- starting a new column
			END IF; -- End IF ( n_ColumnLoop = 1 )
			--
			IF ( v_UseCaseSensitiveRef = 'Y' )
			THEN
				--
				v_UseUpper := 'N';
				--
			ELSE
				--
				n_ErrLocator := 55;
				--
				OPEN IsNameField_Cur ( CurrIndexColumn.ColumnId );
				LOOP
					--
					FETCH IsNameField_Cur INTO v_UseUpper;
					--
					IF ( IsNameField_Cur%NOTFOUND)
					THEN
						--
						v_UseUpper := 'N';
						--
						EXIT;
						--
					END IF;
					--
				END LOOP;
				CLOSE IsNameField_Cur;
				--
			END IF; -- End IF ( v_UseCaseSensitiveRef = 'Y' )
			--
			IF ( v_UseUpper = 'Y' )
			THEN
				--
				v_ColumnName := 'UPPER('||CurrIndexColumn.ColumnName||')';
				--
			ELSE
				--
				v_ColumnName := CurrIndexColumn.ColumnName;
				--
			END IF; -- End IF ( v_UseUpper = 'Y' )
			--
			v_SQLStmt := v_SQLStmt||v_ColumnName||CurrIndexColumn.SortOrder;
			--
			n_ColumnLoop := n_ColumnLoop + 1;
			--
		END LOOP; -- End FOR CurrIndexColumn IN Index_Column_Cur ( v_IndexName )
		--
		IF ( b_IndexColumnsDefined )
		THEN
			--
			v_SQLStmt := v_SQLStmt||') '||v_Online; -- CLOSE THE COLUMN LIST
			--
			IF ( UPPER( p_Action ) = 'EXECUTE' )
			THEN
				--
				n_ErrLocator := 60;
				--
				EXECUTE IMMEDIATE 'SELECT COUNT(Object_Name) '||CHR(10)||
						'  FROM USER_OBJECTS '||CHR(10)||
						' WHERE Object_Name = UPPER('''||v_IndexName||''') '||CHR(10)||
						'   AND Object_Type = ''INDEX'''
				INTO n_ExistsCheck;
				--
				IF ( n_ExistsCheck > 0 )
				THEN
					--
					n_ErrLocator := 65;
					--
					EXECUTE IMMEDIATE 'DROP INDEX '||v_IndexName;
					--
					DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' dropped successfully');
					--
				END IF;
				--
				BEGIN
					--
					DBMS_OUTPUT.PUT_LINE('Index does not exist : '  || v_IndexName);
					n_ErrLocator := 70;
					--
                    --DBMS_OUTPUT.PUT_LINE(v_IsDisable);
					IF ( v_IsDisable <> '1' )-- Do not recreate index if IsDisable is true.
					THEN
					DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
					EXECUTE IMMEDIATE ( v_SQLStmt );
				
					--
					DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' created successfully');
					End IF;
					--
				EXCEPTION
					WHEN e_KeyAlreadyIndexed THEN
						--
						BEGIN
							EXECUTE IMMEDIATE 'SELECT INDEX_NAME FROM USER_IND_COLUMNS WHERE TABLE_NAME = :b1 AND COLUMN_NAME = :b2'
							INTO v_ExistingIndexName USING UPPER(v_TableName), UPPER(v_ColumnName);
							DBMS_OUTPUT.PUT_LINE(CHR(9) || 'Column list "' || v_ColumnName|| '" for index "'||v_IndexName||'" already indexed as "' || v_ExistingIndexName || '". Skipping...');
						EXCEPTION
						WHEN OTHERS THEN
							DBMS_OUTPUT.PUT_LINE(CHR(9) || 'Column list  for index "'||v_IndexName||'" already indexed . Skipping...');
						END ;
						--

					WHEN e_IndexExists THEN
						--
						DBMS_OUTPUT.PUT_LINE(CHR(9) || 'Index "'||v_IndexName||'" already exists . Skipping...');
						--
				END;
				--
			ELSE
				--
				IF ( UPPER( p_Action ) = 'REPORT' )
				THEN
					--
					DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
					--
				END IF;
				--
			END IF; -- End IF ( UPPER( p_Action ) = 'EXECUTE' )
			--
			v_SQLStmt := NULL;
			v_IndexName := NULL;
			--
			b_IndexColumnsDefined := FALSE;
			--
		ELSE
			--
			DBMS_OUTPUT.PUT_LINE ('WARNING - No columns in table "'||v_TableName||'" defined for index "'||v_IndexName||'". Please contact System Administrator.');
			--
		END IF; -- End IF ( b_IndexColumnsDefined )
		--
	END LOOP;
	CLOSE rc_IndexCursor;
	--
	----
	-- This section will check for the existence of any workspace specific index creation scripts
	-- and execute them if found.
	n_ErrLocator := 200;
	DBMS_OUTPUT.PUT_LINE ('Processing custom procedure section...');
	--
	OPEN WorkspaceCursor;
	--
	n_ErrLocator := n_ErrLocator + 5;
	--
	LOOP
		FETCH WorkspaceCursor INTO v_WorkspaceCode;
		EXIT WHEN WorkspaceCursor%NOTFOUND;
		--
		n_ErrLocator := n_ErrLocator + 1;
		--
		v_CustomScriptName := 'Workspace_' || v_WorkspaceCode || '_CreateIndexes';
		--
		-- Check to see if the stored procedure exists
		EXECUTE IMMEDIATE 'SELECT COUNT(Object_Name) FROM USER_PROCEDURES ' ||
		' WHERE Upper(Object_Name) = UPPER(''' || v_CustomScriptName || ''') '
		INTO n_ExistsCheck;
		--
		-- If it does, then call it.
		IF ( n_ExistsCheck = 1 )
		THEN
			DBMS_OUTPUT.PUT_LINE ('Executing procedure '||v_CustomScriptName||'.');
			EXECUTE IMMEDIATE 'begin ' || v_CustomScriptName || '(''' || p_Scope || ''',''' || p_Action || ''',''' || p_DBType || '''); end;';
			n_ErrLocator := n_ErrLocator + 1;
		END IF;
		--
	END LOOP;
	CLOSE WorkspaceCursor ;
	--
	n_ErrLocator := n_ErrLocator + 5;
	--
	DBMS_OUTPUT.PUT_LINE ('Finished custom procedure section...');
	-----
	-- End of Custom Index Creation Script section
	----
	--
	BEGIN
		-- Create Metadata Indices
		--
		n_ErrLocator := 75;
		v_IndexName := 'CDO_Parent_Idx';
		--
		v_SQLStmt := 'CREATE UNIQUE INDEX ' || v_IndexName || ' ON CDODefinition (ParentCDOID ASC, CDODefID ASC ) ' || v_Online;
		--
		IF ( UPPER( p_Action ) = 'EXECUTE' )
		THEN
			--
			EXECUTE IMMEDIATE 'SELECT COUNT(Object_Name) '||CHR(10)||
			'  FROM USER_OBJECTS '||CHR(10)||
			' WHERE Object_Name = UPPER('''||v_IndexName||''') '||CHR(10)||
			'   AND Object_Type = ''INDEX'''
			INTO n_ExistsCheck;
			--
			IF ( n_ExistsCheck > 0 )
			THEN
				--
				n_ErrLocator := 80;
				--
				EXECUTE IMMEDIATE 'DROP INDEX '||v_IndexName;
				--
				DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' dropped successfully');
				--
			END IF;
			--
			BEGIN
				--
				n_ErrLocator := 85;
				--
				EXECUTE IMMEDIATE ( v_SQLStmt );
				--
				DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' created successfully');
				--
			EXCEPTION
			WHEN e_KeyAlreadyIndexed THEN
				--
				DBMS_OUTPUT.PUT_LINE('Column list for index "'||v_IndexName||'" already indexed. Skipping...');
				--
			WHEN e_IndexExists THEN
				--
				DBMS_OUTPUT.PUT_LINE('Index "'||v_IndexName||'" already exists. Skipping...');
				--
			END;
			--
		ELSE
			--
			IF ( UPPER( p_Action ) = 'REPORT' )
			THEN
				--
				DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
				--
			END IF;
			--
		END IF; -- End IF ( UPPER( p_Action ) = 'EXECUTE' )
			--
			
			--
		n_ErrLocator := 86;
		v_IndexName := 'CLFEventMap_Event_Idx';
		--
		v_SQLStmt := 'CREATE INDEX ' || v_IndexName || ' ON CLFEventMap (CLFEventID, CDODefID, CLFID, Name) ' || v_Online;
		--
		IF ( UPPER( p_Action ) = 'EXECUTE' )
		THEN
			--
			EXECUTE IMMEDIATE 'SELECT COUNT(Object_Name) '||CHR(10)||
			'  FROM USER_OBJECTS '||CHR(10)||
			' WHERE Object_Name = UPPER('''||v_IndexName||''') '||CHR(10)||
			'   AND Object_Type = ''INDEX'''
			INTO n_ExistsCheck;
			--
			IF ( n_ExistsCheck > 0 )
			THEN
				--
				n_ErrLocator := 87;
				--
				EXECUTE IMMEDIATE 'DROP INDEX '||v_IndexName;
				--
				DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' dropped successfully');
				--
			END IF;
			--
			BEGIN
				--
				n_ErrLocator := 88;
				--
				EXECUTE IMMEDIATE ( v_SQLStmt );
				--
				DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' created successfully');
				--
			EXCEPTION
			WHEN e_KeyAlreadyIndexed THEN
				--
				DBMS_OUTPUT.PUT_LINE('Column list for index "'||v_IndexName||'" already indexed. Skipping...');
				--
			WHEN e_IndexExists THEN
				--
				DBMS_OUTPUT.PUT_LINE('Index "'||v_IndexName||'" already exists. Skipping...');
				--
			END;
			--
		ELSE
			--
			IF ( UPPER( p_Action ) = 'REPORT' )
			THEN
				--
				DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
				--
			END IF;
			--
		END IF; -- End IF ( UPPER( p_Action ) = 'EXECUTE' )
			--
	END; 
	
	--US 381238  Modify csiCreateIndexes for Oracle to add new PK Index for Labels.LabelId
	n_ErrLocator := 90;
	-- Check to see if primary key exists on Labels table
	BEGIN
		SELECT CONSTRAINT_NAME INTO v_ConstraintName
		FROM USER_CONSTRAINTS
		WHERE CONSTRAINT_TYPE= 'P' AND TABLE_NAME= 'LABELS' ;
			
		DBMS_OUTPUT.PUT_LINE('Primary key constraint ' || v_ConstraintName|| ' already exists on Labels table');
	EXCEPTION
	WHEN NO_DATA_FOUND THEN
		-- Get NULLABLE value of LabelID column of Labels table
		SELECT NULLABLE INTO c_IsNullable
		FROM USER_TAB_COLUMNS
		WHERE TABLE_NAME = 'LABELS'
		AND COLUMN_NAME = 'LABELID';
		
		IF c_IsNullable = 'Y'
		THEN
			-- Alter LabelId column to NOT NULL 
			v_ConstraintName := 'PK_Labels';
			DBMS_OUTPUT.PUT_LINE('Primary key constraint ' || v_ConstraintName || ' will be created');
			v_SQLStmt := 'ALTER TABLE LABELS MODIFY LABELID NOT NULL';
			IF UPPER(p_Action) = 'EXECUTE'
			THEN
				BEGIN
					-- DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
					EXECUTE IMMEDIATE v_SQLStmt;
					DBMS_OUTPUT.PUT_LINE('LabelId in Labels table set to NOT NULL');
				EXCEPTION
				WHEN OTHERS THEN
						DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
				END;
			ELSE
				IF UPPER(p_Action ) = 'REPORT' 
				THEN
					DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
				END IF;
			END IF;
		ELSE
			DBMS_OUTPUT.PUT_LINE( 'LabelId in Labels table is already set to NOT NULL');
		END IF;
			
		-- Create Primary Key Constraint on LabelsId table
		v_SQLStmt := 'ALTER TABLE LABELS ADD CONSTRAINT ' || v_ConstraintName || ' PRIMARY KEY (LabelId)';
		IF UPPER(p_Action) = 'EXECUTE'
		THEN
			BEGIN
				-- DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
				EXECUTE IMMEDIATE v_SQLStmt;
				DBMS_OUTPUT.PUT_LINE('Primary Key Constraint ' || v_ConstraintName || ' created on Labels table successfully');
			EXCEPTION
			WHEN OTHERS THEN
				DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
				IF SQLCODE = -2260 THEN
					DBMS_OUTPUT.PUT_LINE('Primary key constraint  "' || v_ConstraintName ||'" already exists. Skipping...');
				ELSE
					DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
				END IF;
			END;
		ELSE
			IF UPPER(p_Action ) = 'REPORT' 
			THEN
				DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
			END IF;
		END IF;
	END;
--
EXCEPTION
WHEN OTHERS THEN
	--
	IF ( rc_IndexCursor%ISOPEN )
	THEN
		--
		CLOSE rc_IndexCursor;
		--
	END IF;
	--
	IF ( IsNameField_Cur%ISOPEN )
	THEN
		--
		CLOSE IsNameField_Cur;
		--
	END IF;
	--
	RAISE_APPLICATION_ERROR (-20901,'Error creating indexes - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
	--
END;
/