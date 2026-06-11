DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiReturnTransactionID')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiReturnTransactionID;
 	END IF;
END $$;

CREATE PROCEDURE csiReturnTransactionID(OUT IDVal char(16))
language plpgsql
as $$
declare
	s bigint;
begin
	perform nextval('txnidseq')  ;
    select currval('txnidseq') into s;
    IDVal := cast (s as char(16));
       /* Output with leading 0
        -- IDVal := to_char(s, 'FM0000000000000000'); */
	ROLLBACK;
end;
$$;