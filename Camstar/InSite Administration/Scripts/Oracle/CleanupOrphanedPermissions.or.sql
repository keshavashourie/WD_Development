--------------------------------------------------------------------------------
-- SCRIPT:CleanupOrphanedPermissions.sql
-- DESCR: Creates stored procedures used to delete Role-Permissions that are orphaned due to the deletion of CDO services
--
--  Copyright Siemens 2023  

--------------------------------------------------------------------------------
-- PROCEDURE: rbacCleanupOrphanedPermissions
-- DESCR: Procedure to delete Role-Permissions that are orphaned due to the deletion of CDO services
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE rbacCleanupOrphanedPermissions
AS
	vRolePermissionId VARCHAR2(16);
    vRoleId VARCHAR2(16);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
BEGIN
	-- Fetch orphaned permissions for all the deleted services
	vQuerySQL	:=	'SELECT	RolePermissionId, RoleId ' ||
					'FROM		RolePermission ' ||
					'WHERE	PermissionType <= 180 ' ||
					'AND		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition )';

	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vRolePermissionId, vRoleId;
	WHILE (c1%FOUND) LOOP
		-- For each orphaned entry delete permission modes
		DELETE FROM RolePermissionModes WHERE RolePermissionId = vRolePermissionId;

		FETCH c1 INTO vRolePermissionId, vRoleId;
	END LOOP;
	CLOSE c1;

	-- Finally, delete the orphaned permissions
	DELETE FROM RolePermission 
	Where	PermissionType <= 180
	And		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition );

END;
/
BEGIN
	rbacCleanupOrphanedPermissions;
END;
/
DROP PROCEDURE rbacCleanupOrphanedPermissions
/
