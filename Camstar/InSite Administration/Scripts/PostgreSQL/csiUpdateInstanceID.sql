DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiUpdateInstanceID')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiUpdateInstanceID;
 	END IF;
END $$;

CREATE PROCEDURE csiUpdateInstanceID(instanceType integer,
  i_CDODefID integer,
  incrementAmt integer,
  OUT InstIdNewValue char(16))
language plpgsql
as $$
declare
	InstIdOldValue char(16);
BEGIN
    -- BEGIN TRANSACTION
    BEGIN
        -- DECLARE a variable to hold the old value
        IF instanceType = 0 THEN
            SELECT ClientInstanceID INTO InstIdOldValue
            FROM InstanceIDCount i
            WHERE i.CDODefID = i_CDODefID
            FOR UPDATE;
        ELSE
            SELECT CSiInstanceID INTO InstIdOldValue
            FROM InstanceIDCount i
            WHERE i.CDODefID = i_CDODefID
            FOR UPDATE;
        END IF;
        
        -- EXECUTE csiIncrementString64
        CALL csiIncrementString64(InstIdOldValue, incrementAmt, InstIdNewValue);

        -- UPDATE InstanceIDCount
        IF instanceType = 0 THEN
            UPDATE InstanceIDCount i
            SET ClientInstanceID = InstIdNewValue
            WHERE i.CDODefID = i_CDODefID;
        ELSE
            UPDATE InstanceIDCount i
            SET CSiInstanceID = InstIdNewValue
            WHERE i.CDODefID = i_CDODefID;
        END IF;

        -- Check if rows were updated
        IF NOT FOUND THEN
            RAISE NOTICE 'Error Occurred, no rows were updated';
            RETURN;
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE NOTICE 'Error Occurred';
            RETURN;
    END;
END;
$$;