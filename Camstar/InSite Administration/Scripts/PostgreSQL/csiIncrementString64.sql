DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiincrementstring64')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiincrementstring64;
 	END IF;
END $$;

CREATE PROCEDURE csiincrementstring64(oldvalue char(16), valuetoadd integer, OUT newvalue char(16))
language plpgsql
as $$
DECLARE

tmpvalue bigint;
tempstr char(18);

BEGIN

CALL csihextodec(oldvalue, tmpvalue);
tmpvalue := tmpvalue + valuetoadd;
CALL csibiginttohex(tmpvalue, tempstr);
newvalue := upper(substring(tempstr,3,16));

END;
$$;