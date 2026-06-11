--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetAccessibleOrgs.sql
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
 		WHERE lower(routine_name) = lower('csiAuthGetAccessibleOrgs')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiAuthGetAccessibleOrgs;
 	END IF;
END $$;
CREATE FUNCTION csiAuthGetAccessibleOrgs(p_UserName varchar(100))
RETURNS TABLE (OrganizationId varchar(16), OrganizationName varchar(255)) AS $$
DECLARE
    v_OrganizationId varchar(16);
    v_PropagateToChildOrgs integer;
    v_OrganizationName varchar(100);
    v_RecOrganizationName varchar(100);
    v_Orgs cursor FOR
        SELECT er.OrganizationId,
               er.PropagateToChildOrgs
        FROM Employee e
        JOIN EmployeeRole er ON er.EmployeeId = e.EmployeeId
        WHERE e.EmployeeName = p_UserName;
    --v_tmpOrgs TABLE (OrganizationId varchar(16), OrganizationName varchar(255));
    v_temp text;
begin
	
	v_temp := 'v_tmpOrgs_' || to_char(current_timestamp, 'YYYYMMDDHH24MISSMS');
	DROP TABLE IF EXISTS v_temp;
	CREATE TEMPORARY TABLE v_temp (
	    OrganizationId varchar(16),
	    OrganizationName varchar(255)
	);

    OPEN v_Orgs;
    LOOP
        FETCH v_Orgs INTO v_OrganizationId, v_PropagateToChildOrgs;
        EXIT WHEN NOT FOUND;

        IF (LENGTH(v_OrganizationId) > 0) THEN
            SELECT O2.OrganizationName INTO v_OrganizationName
            FROM Organization O2
            WHERE O2.OrganizationId = v_OrganizationId;

            INSERT INTO v_temp (OrganizationId, OrganizationName) VALUES (v_OrganizationId, v_OrganizationName);

            IF (v_PropagateToChildOrgs = 1) THEN
                INSERT INTO v_temp
                SELECT DISTINCT o.OrganizationId, o.OrganizationName
                FROM Organization o
                JOIN (WITH RECURSIVE OrgHierarchy AS (
                        SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, 1 AS Level
                        FROM Organization o
                        WHERE o.OrganizationName = v_OrganizationName
                        UNION ALL
                        SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, ch.Level + 1
                        FROM Organization o
                        JOIN OrgHierarchy ch ON o.ParentOrganizationId = ch.OrganizationId AND ch.Level < 10
                    )
                    SELECT * FROM OrgHierarchy) subq ON subq.OrganizationName <> v_OrganizationName
                WHERE o.OrganizationId = subq.OrganizationId;
            END IF;
        END IF;
    END LOOP;
    CLOSE v_Orgs;

    RETURN QUERY SELECT * FROM v_temp;
END;
$$ LANGUAGE plpgsql;
--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetAllowedUsers.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAuthGetAllowedUsers')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiAuthGetAllowedUsers;
 	END IF;
END $$;
CREATE FUNCTION csiAuthGetAllowedUsers (
    inOrganizationName varchar(255),
    inRoleName varchar(255)
)
RETURNS TABLE (EmployeeId varchar(16), EmployeeName varchar(255)) AS $$
DECLARE
    v_OrganizationId varchar(16);
    v_PropagateToChildOrgs integer;
    v_EmployeeId varchar(16);
    v_EmployeeName varchar(100);
    v_RoleName varchar(255);
    v_OrganizationName varchar(100);
   	v_rec cursor FOR
        SELECT
            er.OrganizationId,
            er.PropagateToChildOrgs,
            e.EmployeeId,
            e.EmployeeName,
            r.RoleName
        FROM Employee e
        JOIN EmployeeRole er ON er.EmployeeId = e.EmployeeId
        JOIN RoleDef r ON r.RoleId = er.RoleId
        WHERE (r.RoleName = inRoleName OR LENGTH(inRoleName) = 0);
   	v_temp text;
begin
	
	v_temp := 'v_tmpUsers_' || to_char(current_timestamp, 'YYYYMMDDHH24MISSMS');
	DROP TABLE IF EXISTS v_temp;
	CREATE TEMPORARY TABLE v_temp (
	    EmployeeId varchar(16),
	    EmployeeName varchar(255)
	);

    OPEN v_rec;
    loop
	    FETCH v_rec INTO v_OrganizationId, v_PropagateToChildOrgs, v_EmployeeId, v_EmployeeName, v_RoleName;
        EXIT WHEN NOT FOUND;

        IF LENGTH(v_OrganizationId) > 0 then
        
            SELECT OrganizationName INTO v_OrganizationName
            FROM Organization
            WHERE OrganizationId = v_OrganizationId;

            IF (v_OrganizationName = inOrganizationName OR LENGTH(inOrganizationName) = 0) THEN
                INSERT INTO v_temp (EmployeeId, EmployeeName)
                VALUES (v_EmployeeId, v_EmployeeName);
            END IF;

            IF (v_PropagateToChildOrgs = 1 AND (v_RoleName = inRoleName OR LENGTH(inRoleName) = 0)) THEN
                INSERT INTO v_temp (EmployeeId, EmployeeName)
                SELECT DISTINCT v_EmployeeId, v_EmployeeName
                FROM (WITH RECURSIVE OrgHierarchy AS (
                        SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, 1 AS Level
                        FROM Organization o
                        WHERE o.OrganizationName = v_OrganizationName
                        UNION ALL
                        SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, ch.Level + 1
                        FROM Organization o
                        JOIN OrgHierarchy ch ON o.ParentOrganizationId = ch.OrganizationId AND ch.Level < 10
                    )
					SELECT * FROM OrgHierarchy) subq
                WHERE subq.OrganizationName <> v_OrganizationName
                AND (subq.OrganizationName = inOrganizationName OR LENGTH(inOrganizationName) = 0);
            END IF;
        END IF;
	    
    END LOOP;
    CLOSE v_rec;

    RETURN QUERY SELECT * FROM v_temp;   
END;
$$ LANGUAGE plpgsql;
--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetUsersRoles.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAuthGetUsersRoles')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiAuthGetUsersRoles;
 	END IF;
END $$;
CREATE FUNCTION csiAuthGetUsersRoles (OwnerId varchar(16))
RETURNS TABLE (OrganizationId varchar(16), RoleId varchar(16)) AS $$
DECLARE
    v_OrganizationId varchar(16);
    v_RoleId varchar(16);
    v_PropagateToChildOrgs integer;
   	v_rec cursor FOR
		SELECT DISTINCT er.OrganizationId,
            r.RoleId,
            er.PropagateToChildOrgs
        FROM Employee e
        JOIN EmployeeRole er ON er.EmployeeId = e.EmployeeId
        JOIN RoleDef r ON r.RoleId = er.RoleId
        WHERE e.EmployeeId = OwnerId AND er.OrganizationId IS NOT null;
	v_temp text;
begin
	
	v_temp := 'v_tmpUsersRoles_' || to_char(current_timestamp, 'YYYYMMDDHH24MISSMS');
	DROP TABLE IF EXISTS v_temp;
	CREATE TEMPORARY TABLE v_temp (
	    OrganizationId varchar(16),
	    RoleId varchar(16)
	);

    OPEN v_rec; 
    loop
	    FETCH v_rec INTO v_OrganizationId, v_RoleId, v_PropagateToChildOrgs;
        EXIT WHEN NOT FOUND;

        IF v_PropagateToChildOrgs = 1 THEN
            INSERT INTO v_temp (OrganizationId, RoleId)
            SELECT DISTINCT subq.OrganizationId, v_RoleId
            FROM (
	            WITH RECURSIVE OrgHierarchy (OrganizationId, ParentOrganizationId, OrganizationName, Level) AS (
	                -- Anchor member definition - The top-level parent
	                SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, 1 AS Level
	                FROM Organization o
	                WHERE o.OrganizationId = v_OrganizationId
	                UNION ALL
	                -- Recursive member definition - Children, grandchildren, etc.
	                SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, ch.Level + 1
	                FROM Organization o
	                JOIN OrgHierarchy ch ON o.ParentOrganizationId = ch.OrganizationId AND ch.Level < 10
	            ) SELECT * FROM OrgHierarchy
            ) subq;
        ELSE
            INSERT INTO v_temp (OrganizationId, RoleId)
            SELECT o.OrganizationId, v_RoleId
            FROM Organization o
            WHERE o.OrganizationId = v_OrganizationId;
        END IF;
    END LOOP;
    CLOSE v_rec;

    RETURN QUERY SELECT * FROM v_temp;
END;
$$ LANGUAGE plpgsql;
