DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGenStageDDL')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGenStageDDL;
 	END IF;
END $$;

CREATE PROCEDURE csiGenStageDDL(
	owner_name  varchar(128)
)
language plpgsql
as $$
DECLARE
 table_name varchar(128);
 tab_ddl varchar(8000);
 table_cursor cursor 
 		FOR
        SELECT tablename 
        FROM pg_catalog.pg_tables
        --WHERE tablename LIKE 'container%' AND schemaname = owner_name;
        WHERE tablename LIKE 'LS#%' AND schemaname = owner_name;
BEGIN
-- Open a cursor to fetch table names
    OPEN table_cursor;
    
    -- Fetch table names and generate DDL
    LOOP
        FETCH table_cursor INTO table_name;
        EXIT WHEN NOT FOUND;
        
        -- Call the CsiGenTableDDL function
        CALL csiGenTableDDL(owner_name, table_name, tab_ddl);
        
        -- Print the generated DDL
        RAISE NOTICE '%', tab_ddl;
    END LOOP;
    
    -- Close and deallocate the cursor
    CLOSE table_cursor;
END;
$$;