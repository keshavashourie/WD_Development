DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('rbacCleanupOrphanedPermissions')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS rbacCleanupOrphanedPermissions;
 	END IF;
END $$;
CREATE PROCEDURE rbacCleanupOrphanedPermissions ()
LANGUAGE plpgsql
AS $$
DECLARE
	v_RolePermissionId CHAR(16);
    v_RoleId CHAR(16);
	QuerySQL TEXT;
	c1 refcursor;
BEGIN
	-- Fetch orphaned permissions for all the deleted services
	RAISE NOTICE 'Fetching orphaned permissions...';
	QuerySQL := 'SELECT	RolePermissionId, RoleId '
				|| 'FROM		RolePermission '
				|| 'WHERE	PermissionType <= 180 '
				|| 'AND		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition )';
	OPEN c1 FOR EXECUTE QuerySQL;
	LOOP
		FETCH c1 INTO v_RolePermissionId, v_RoleId;
		EXIT WHEN NOT FOUND;
		BEGIN
			-- For each orphaned entry delete permission modes
			RAISE NOTICE 'Deleting Permission: % for Role: %', v_RolePermissionId, v_RoleId;
			
			DELETE FROM RolePermissionModes WHERE RolePermissionId = v_RolePermissionId;
		END;
	END LOOP;
	CLOSE c1;
	
	-- Finally, delete the orphaned permissions
	DELETE FROM RolePermission 
	Where	PermissionType <= 180
	And		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition );

	RAISE NOTICE 'Cleanup of Orphaned Permissions is completed!';
END;
$$;

CALL rbacCleanupOrphanedPermissions();

DROP PROCEDURE rbacCleanupOrphanedPermissions();
