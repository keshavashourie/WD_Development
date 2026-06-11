DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetDerivedCDOs')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetDerivedCDOs;
 	END IF;
END $$;

CREATE FUNCTION csiGetDerivedCDOs(IncludeParent integer, inCDODefID integer)
RETURNS TABLE (CDODefID integer) AS $$
DECLARE
    tempCDODefID integer;
begin

    tempCDODefID := inCDODefID;

 

    IF (IncludeParent = 1) THEN
        RETURN QUERY SELECT tempCDODefID;
    END IF;

 

    RETURN QUERY
    WITH RECURSIVE CDODerived AS (
        SELECT c2.CDODefId
        FROM CDODefinition c2
        WHERE c2.ParentCDOId = tempCDODefID
        UNION ALL
        SELECT c.CDODefId
        FROM CDODefinition c
        INNER JOIN CDODerived d ON c.ParentCDOId = d.CDODefId
    )
    SELECT c3.CDODefId FROM CDODerived c3;

 

    RETURN;
END;
$$ LANGUAGE plpgsql;