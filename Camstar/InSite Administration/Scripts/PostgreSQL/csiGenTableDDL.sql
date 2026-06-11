DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGenTableDDL')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGenTableDDL;
 	END IF;
END $$;

CREATE PROCEDURE csiGenTableDDL(
  owner varchar(128),
  table_name varchar(128),
  OUT tab_ddl varchar(8000)
)
language plpgsql
as $$
DECLARE
    column_name varchar(128);
    column_type varchar(128);
    column_precision varchar(30);
    nullable varchar(128);
    ddl_cursor REFCURSOR;
    ddl_statement varchar(8000);
BEGIN
    -- Set up the DDL statement for creating the table
    tab_ddl := 'CREATE TABLE ' || owner_name || '.' || table_name || ' (' || chr(13);
    
    -- Open a cursor to fetch column details
    OPEN ddl_cursor FOR
        SELECT 
            LEFT(a.attname, 30) AS Name, 
            LEFT(pg_catalog.format_type(a.atttypid, a.atttypmod), 15) AS Type, 
            CAST(a.atttypmod AS VARCHAR), 
            CASE a.attnotnull 
                WHEN true THEN 'NOT NULL' 
                ELSE 'NULL' 
            END AS Nullable
        FROM pg_attribute a 
        WHERE a.attnum > 0 
            AND NOT a.attisdropped 
            AND a.attrelid = (SELECT c.oid FROM pg_class c JOIN pg_namespace n ON c.relnamespace = n.oid WHERE c.relname = table_name AND n.nspname = owner_name)
        ORDER BY a.attnum;
    
    -- Fetch column details and build the DDL statement
    LOOP
        FETCH ddl_cursor INTO column_name, column_type, column_precision, nullable;
        EXIT WHEN NOT FOUND;
        
        ddl_statement := '   ' || column_name || ' ' || column_type;
        
        IF column_type IN ('character varying', 'character', 'varchar', 'char') THEN
            ddl_statement := ddl_statement || '(' || column_precision || ')';
        END IF;
        
        ddl_statement := ddl_statement || ' ' || nullable || ',' || chr(13);
        
        tab_ddl := tab_ddl || ddl_statement;
    END LOOP;
    
    -- Close and deallocate the cursor
    CLOSE ddl_cursor;
    
    -- Append the closing parentheses and semicolon
    tab_ddl := tab_ddl || ');' || chr(13) || chr(13);
END;
$$;