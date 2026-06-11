--------------------------------------------------------------------------------
-- SCRIPT: csiAuthRetrieveUserRoles.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2025  
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAuthRetrieveUserRoles')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiAuthRetrieveUserRoles;
 	END IF;
END $$;

CREATE FUNCTION csiAuthRetrieveUserRoles (UserName VARCHAR(100))
RETURNS TABLE (RoleName VARCHAR(255), OrganizationName VARCHAR(100))
AS $$
DECLARE
    c1 CURSOR FOR
        SELECT er.OrganizationId,
               r.RoleName,
               er.PropagateToChildOrgs	   
        FROM Employee e
        JOIN EmployeeRole er ON e.EmployeeId = er.EmployeeId
        JOIN RoleDef r ON er.RoleId = r.RoleId
        WHERE lower(e.EmployeeName) = lower(UserName);
    
    fiRoleName VARCHAR(255);
    fiPropagateToChildOrgs BOOLEAN;
    fiOrganizationId VARCHAR(16);
    tOrganizationName VARCHAR(100);
   	v_temp text;
begin
	
	v_temp := 'tmpresults_' || to_char(current_timestamp, 'YYYYMMDDHH24MISSMS');
    execute format('create local TEMPORARY TABLE %I (
        RoleName VARCHAR(255),
        OrganizationName VARCHAR(100)
    ) on commit drop', v_temp);

    OPEN c1;
    FETCH NEXT FROM c1 INTO fiOrganizationId, fiRoleName, fiPropagateToChildOrgs;
    WHILE FOUND LOOP
        IF (LENGTH(fiOrganizationId) = 0 OR fiOrganizationId IS NULL) THEN
            tOrganizationName := '';
        ELSE
            SELECT o.OrganizationName INTO tOrganizationName
            FROM Organization o
            WHERE o.OrganizationId = fiOrganizationId;
        END IF;
      
      	EXECUTE format('INSERT INTO %I VALUES ($1, $2)', v_temp) USING fiRoleName, tOrganizationName;
      
        -- If the record has the PropagateToChildOrgs bit set, then copy this record for all (recursive) child orgs
        IF (fiPropagateToChildOrgs = TRUE) then
        
            execute format( 'INSERT INTO %I
            WITH RECURSIVE OrgHierarchy (OrganizationId, ParentOrganizationId, Name, Level) AS (
                
                SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, 1 AS Level
                FROM Organization o
                WHERE o.OrganizationName = $1

                UNION ALL
                
                SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, ch.Level + 1
                FROM Organization o, OrgHierarchy ch
                WHERE o.ParentOrganizationId = ch.OrganizationId AND ch.Level < 10
            )
            SELECT DISTINCT $3 as RoleName, Name FROM OrgHierarchy WHERE Name <> $2 
            ', v_temp) using tOrganizationName, tOrganizationName, fiRoleName;
        END IF;

        FETCH NEXT FROM c1 INTO fiOrganizationId, fiRoleName, fiPropagateToChildOrgs;
    END LOOP;   
    CLOSE c1;
   
	return QUERY execute format('SELECT * FROM %I', v_temp);
 
end ;
$$ LANGUAGE plpgsql;