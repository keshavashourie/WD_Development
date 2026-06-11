DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csicopytable')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csicopytable;
 	END IF;
END $$;

CREATE PROCEDURE csicopytable(
	source_owner   varchar(255),
	source_name    varchar(255),
	target_owner   varchar(255),
	target_name    varchar(255),
	gen_columns    char(1) = 'N'   /* SPECIFY WHICH TABLE TO USE TO GENERATE COLUMN LIST
                                     'S' = SOURCE TABLE 'T' = TARGET TABLE */
)
language plpgsql
as $$
DECLARE
	sqlstatement TEXT;
	column_list varchar(8000);
BEGIN
IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = target_owner AND table_name = target_name
    ) THEN
        IF gen_columns = 'T' THEN
            -- Generate column list for the target table
            CALL CsiGetColumnList(target_owner, target_name, column_list);
            
            sqlstatement := 'TRUNCATE TABLE ' || target_owner || '.' || target_name;
            EXECUTE sqlstatement;
            
            sqlstatement := 'INSERT INTO ' || target_owner || '.' || target_name ||
                             '(' || column_list || ')' ||
                             ' SELECT ' || column_list || ' FROM ' || source_owner || '.' || source_name;
            EXECUTE sqlstatement;
        ELSIF gen_columns = 'S' THEN
            -- Generate column list for the source table
            CALL CsiGetColumnList(source_owner, source_name, column_list);
            
            sqlstatement := 'TRUNCATE TABLE ' || target_owner || '.' || target_name;
            EXECUTE sqlstatement;
            
            sqlstatement := 'INSERT INTO ' || target_owner || '.' || target_name ||
                             '(' || column_list || ')' ||
                             ' SELECT ' || column_list || ' FROM ' || source_owner || '.' || source_name;
            EXECUTE sqlstatement;
        ELSE
            RAISE NOTICE 'Target Table exists and no column list specified';
        END IF;
    ELSE
        -- Target table does not exist, perform CREATE with INSERT INTO syntax
        sqlstatement := 'CREATE TABLE ' || target_owner || '.' || target_name ||
                         ' AS SELECT * FROM ' || source_owner || '.' || source_name;
        EXECUTE sqlstatement;
    END IF;
END;
$$;