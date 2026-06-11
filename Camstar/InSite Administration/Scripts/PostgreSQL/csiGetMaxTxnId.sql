DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetMaxTxnId')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGetMaxTxnId;
 	END IF;
END $$;

CREATE PROCEDURE csiGetMaxTxnId(
OUT p_maxhexid varchar(15))
language plpgsql
as $$
DECLARE
	v_nibble integer;
BEGIN

	-- Get the number of Nibble (sites) to be excluded
    SELECT CEIL((CAST(NumberOfBitsUsed AS decimal) + 1) / 4) + 1
    INTO v_nibble
    FROM DBIDConfiguration;

    -- Get current max used value
    SELECT MAX(txnid)
    INTO p_maxhexid
    FROM (
        SELECT COALESCE(MAX(SUBSTRING(txnid, v_nibble, 16)), '0') AS txnid
        FROM historymainline
        UNION
        SELECT COALESCE(MAX(SUBSTRING(txnid, v_nibble, 16)), '0') AS txnid
        FROM transrevoids
        UNION
        SELECT COALESCE(MAX(SUBSTRING(txnid, v_nibble, 16)), '0') AS txnid
        FROM OutboundXMLDoc
        UNION
        SELECT COALESCE(MAX(SUBSTRING(txnid, v_nibble, 16)), '0') AS txnid
        FROM ProcessedTxnGUID
    ) a;
END;
$$;