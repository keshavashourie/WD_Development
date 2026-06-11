DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetAdjustedTime')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetAdjustedTime;
 	END IF;
END $$;

CREATE FUNCTION csiGetAdjustedTime(
	pValue timestamp with time zone,
	pOffset integer)
RETURNS TIMESTAMP with time zone
language plpgsql
as $$
DECLARE
BEGIN
	RETURN pValue + (pOffset || ' minutes')::interval;
END;
$$;