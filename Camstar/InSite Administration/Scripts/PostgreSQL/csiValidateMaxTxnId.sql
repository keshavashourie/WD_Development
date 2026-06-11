DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiValidateMaxTxnId')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiValidateMaxTxnId;
 	END IF;
END $$;

CREATE PROCEDURE csiValidateMaxTxnId(
	p_maxhexid varchar(15),
	OUT p_count integer
)
language plpgsql
as $$
DECLARE
	v_nibble integer;
 	v_sqlstmt varchar(1000);
BEGIN
	-- Create TRANSACTIONIDTABLERESETDATA Table if it does not exist
    BEGIN
        EXECUTE 'CREATE TEMPORARY TABLE IF NOT EXISTS TRANSACTIONIDTABLERESETDATA (MAXTXNIDS CHAR(16))';
        EXECUTE 'DELETE FROM TRANSACTIONIDTABLERESETDATA';
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'Error occurred while creating/deleting table';
    END;

    -- Get the number of Nibble (sites) to be excluded
    SELECT CEIL((CAST(NumberOfBitsUsed AS decimal) + 1) / 4) + 1
    INTO v_nibble
    FROM DBIDConfiguration;

    -- Get all the max TxnIDs into the table
    INSERT INTO TRANSACTIONIDTABLERESETDATA
    SELECT TXNID
    FROM transrevoids
    WHERE SUBSTRING(txnid, v_nibble, 15) = p_maxhexid;

    INSERT INTO TRANSACTIONIDTABLERESETDATA
    SELECT TXNID
    FROM historymainline
    WHERE SUBSTRING(txnid, v_nibble, 15) = p_maxhexid
        AND txnid NOT IN (SELECT MAXTXNIDS FROM TRANSACTIONIDTABLERESETDATA);

    INSERT INTO TRANSACTIONIDTABLERESETDATA
    SELECT DISTINCT TXNID
    FROM OutboundXMLDoc
    WHERE SUBSTRING(txnid, v_nibble, 15) = p_maxhexid
        AND txnid NOT IN (SELECT MAXTXNIDS FROM TRANSACTIONIDTABLERESETDATA);

    INSERT INTO TRANSACTIONIDTABLERESETDATA
    SELECT TXNID
    FROM ProcessedTxnGUID
    WHERE SUBSTRING(txnid, v_nibble, 15) = p_maxhexid
        AND txnid NOT IN (SELECT MAXTXNIDS FROM TRANSACTIONIDTABLERESETDATA);

    -- Recreate the Sequence if the count of TXNID in the TRANSACTIONIDTABLERESETDATA is 1
    SELECT COUNT(*)
    INTO p_count
    FROM TRANSACTIONIDTABLERESETDATA;
END;
$$;