DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiIDControl')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiIDControl;
 	END IF;
END $$;

CREATE PROCEDURE csiIDControl(
	i_idtype     varchar(30),
	OUT o_nextid     integer,
	seqcount   integer = 1
)
language plpgsql
as $$
declare
	v_rowcount integer;
BEGIN
	
    -- Increment the nextid value in IDControl table
    UPDATE IDControl
    SET nextid = nextid + seqcount
    WHERE IDType = i_idtype;

	SELECT nextid INTO o_nextid
	FROM IDControl
	WHERE IDType = i_idtype;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    -- Check if any rows were updated
    IF v_rowcount = 0 THEN
      RAISE NOTICE 'Error Occurred, no rows were updated';
    END IF;

  END;
$$;