DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetPKColumns')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGetPKColumns;
 	END IF;
END $$;

CREATE PROCEDURE csiGetPKColumns(
	tabowner varchar(128),
	tabname varchar(128),
	OUT constraintname varchar(128),
	OUT col_list varchar(2000)
)
language plpgsql
as $$
declare
	v_constraint_name varchar(128);
	col_name varchar(128);
	constraint_curs CURSOR FOR
        SELECT constraint_name, column_name
        FROM information_schema.key_column_usage 
        WHERE table_name = tabname
            AND table_schema = tabowner
        ORDER BY ordinal_position;
BEGIN
	col_list := ' ';
	
	OPEN constraint_curs;
	
	LOOP
		FETCH constraint_curs INTO v_constraint_name, col_name;
		EXIT WHEN NOT FOUND;
		
		constraintname := v_constraint_name;
		col_list := col_list || col_name || ',';

	END LOOP;
	
	IF LENGTH(col_list) > 1 THEN
		col_list := SUBSTRING(col_list, 1, LENGTH(col_list) - 1);
	END IF;
	
	CLOSE constraint_curs;
END;
$$;