DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiResetTXNID')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiResetTXNID;
 	END IF;
END $$;

CREATE PROCEDURE csiResetTXNID()
language plpgsql
as $$
DECLARE
	v_maxhexid varchar(15);
	v_count	integer;
BEGIN
	-- Call the csiGetMaxTxnId
    CALL csiGetMaxTxnId(v_maxhexid);
    
    -- Call the csiValidateMaxTxnId
    CALL csiValidateMaxTxnId(v_maxhexid, v_count);
    
    IF v_count <> 1 THEN
        RAISE NOTICE 'Please check TRANSACTIONIDTABLERESETDATA Table for the Max TxnID';
        RAISE NOTICE 'Create the Sequence and Drop the Table manually';
    ELSE
        -- Call the csiSetNextTxnId Stored Procedure
        CALL csiSetNextTxnId(v_maxhexid);
        RAISE NOTICE 'Sequence has been recreated successfully';
    END IF;
END;
$$;