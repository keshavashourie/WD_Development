DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCDOInstanceCount')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiCDOInstanceCount;
 	END IF;
END $$;

CREATE FUNCTION csiCDOInstanceCount(pTableName VARCHAR(30)) RETURNS integer AS
$$
DECLARE
  v_count integer := 0;
BEGIN
	BEGIN
		EXECUTE 'SELECT COUNT(*) FROM ' || pTableName INTO v_count;
	EXCEPTION
		WHEN undefined_table THEN
			RETURN NULL;
		WHEN others THEN
			RETURN NULL;
	END;

	RETURN v_count;
END;
$$
LANGUAGE plpgsql;