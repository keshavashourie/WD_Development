DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csibiginttohex')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csibiginttohex;
 	END IF;
END $$;

create procedure csibiginttohex(intvalue bigint, out charvalue char(255))
language plpgsql
as $$
declare 
BEGIN
  charvalue := '0x' || lpad(to_hex(intvalue),16,'0');
end;
$$;