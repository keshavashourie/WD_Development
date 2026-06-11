DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetColumnList')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGetColumnList;
 	END IF;
END $$;

CREATE PROCEDURE csiGetColumnList(
  owner varchar(128),
  i_table_name varchar(128),
  OUT column_list varchar(8000)
)
language plpgsql
as $$
DECLARE
	v_column_name varchar(128);
	column_cursor CURSOR FOR
        SELECT column_name
        FROM information_schema.columns 
        WHERE table_schema = owner AND table_name = i_table_name;
BEGIN
	column_list := ' ';
	
	OPEN column_cursor;
	
    LOOP
	
		FETCH column_cursor INTO v_column_name;
		EXIT WHEN NOT FOUND;
	
		column_list := column_list || v_column_name || ',';

    END LOOP;

    CLOSE column_cursor;

    -- Remove the trailing comma
    column_list := SUBSTRING(column_list, 1, LENGTH(column_list) - 1);

END;
$$;