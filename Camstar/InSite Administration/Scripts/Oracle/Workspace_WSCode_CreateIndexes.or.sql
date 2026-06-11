CREATE OR REPLACE PROCEDURE Workspace_WSCode_CreateIndexes ( p_Scope		IN VARCHAR2
					      ,p_Action		IN VARCHAR2
					      ,p_DBType		IN VARCHAR2 )
IS
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
-- Nick Aghazarian	2016.04.28	Created
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------
	v_IndexName		VARCHAR2(30);
	v_TableName		VARCHAR2(30);
	n_ErrLocator	NUMBER;
	v_Unique		  VARCHAR2(8);
	v_SQLStmt		  VARCHAR2(2000);
	n_ExistsCheck	NUMBER := 0;
  --
	e_KeyAlreadyIndexed	EXCEPTION;
	e_IndexExists		EXCEPTION;
	--
	PRAGMA EXCEPTION_INIT(e_KeyAlreadyIndexed,-1408);
	PRAGMA EXCEPTION_INIT(e_IndexExists,-0955);
	--
  BEGIN
    v_IndexName := 'IndexName';
    v_TableName := 'TableName';
  
    -- Delete the index if it already exists...   
    BEGIN
      SELECT COUNT(*) INTO n_ExistsCheck FROM user_indexes WHERE upper(index_name) = upper(v_IndexName);
      --
      n_ErrLocator := 50;
      --
      IF ( n_ExistsCheck > 0 )
        THEN
            --
            n_ErrLocator := 65;
            --
            EXECUTE IMMEDIATE 'DROP INDEX '||v_IndexName;
            --
            n_ErrLocator := 55;
           --
            DBMS_OUTPUT.PUT_LINE('Index '||v_IndexName||' dropped successfully');
            --
        END IF;    
    END;
    
    -- Generate the create index statement
    BEGIN
      -- Add "UNIQUE" keyword as needed, Datastore is usually not unique
      IF ( UPPER(p_DBType)='OLTP' ) 
      THEN
        v_Unique := ' ';
      ELSE
        v_Unique := ' ';
      END IF;
      --
      n_ErrLocator := 58;

      -- Create base of statement    
      v_SQLStmt := 'CREATE ' || v_Unique || ' INDEX ' || v_IndexName || ' ON ' || v_TableName || '(';
      --
      n_ErrLocator := 60;
  
      --
      -- Set Index Columns
      v_SQLStmt := v_SQLStmt || 'Name,';
  
      -- Remove last comma and close list
			v_SQLStmt := SUBSTR ( v_SQLStmt,1,LENGTH(v_SQLStmt)-1)||') NOLOGGING ';
      --
      n_ErrLocator := 70;
    END;  
    
    -- Create the index
			IF ( UPPER( p_Action ) = 'EXECUTE' )
			THEN
        BEGIN
          --
          EXECUTE IMMEDIATE ( v_SQLStmt );
          --
          n_ErrLocator := 80;
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
    ELSE
      DBMS_OUTPUT.PUT_LINE(v_SQLStmt);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      RAISE_APPLICATION_ERROR (-20901,'Error creating custom indexes - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
      --

  END;
  
