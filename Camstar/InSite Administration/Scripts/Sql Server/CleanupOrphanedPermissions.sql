--------------------------------------------------------------------------------
-- SCRIPT:CleanupOrphanedPermissions.sql
-- DESCR: Creates stored procedures used to delete Role-Permissions that are orphaned due to the deletion of CDO services
--
-- Copyright Siemens 2023  


--------------------------------------------------------------------------------
-- PROCEDURE: rbacCleanupOrphanedPermissions
-- DESCR: Procedure to delete Role-Permissions that are orphaned due to the deletion of CDO services
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'rbacCleanupOrphanedPermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE rbacCleanupOrphanedPermissions
GO
CREATE PROCEDURE rbacCleanupOrphanedPermissions
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RolePermissionId CHAR(16)
    DECLARE @RoleId CHAR(16)
	DECLARE @QuerySQL NVARCHAR(MAX)
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR

	-- Fetch orphaned permissions for all the deleted services
    PRINT('Fetching orphaned permissions...');
	SET @QuerySQL	= 'SELECT	RolePermissionId, RoleId '
					+ 'FROM		RolePermission '
					+ 'WHERE	PermissionType <= 180 '
					+ 'AND		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition )'

	SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'
	EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	FETCH NEXT FROM @c1 INTO @RolePermissionId, @RoleId
	WHILE(@@fetch_status = 0)
	BEGIN
		-- For each orphaned entry delete permission modes
	    PRINT('Deleting Permission: ' + @RolePermissionId + ' for Role: ' + @RoleId);
		DELETE FROM RolePermissionModes WHERE RolePermissionId = @RolePermissionId

		FETCH NEXT FROM @c1 INTO @RolePermissionId, @RoleId
	END
	CLOSE @c1
	DEALLOCATE @c1

	-- Finally, delete the orphaned permissions
	DELETE FROM RolePermission 
	Where	PermissionType <= 180
	And		ObjectMetaId NOT IN ( Select CDODefId from CDODefinition )

	PRINT('Cleanup of Orphaned Permissions is completed!');
END
GO

EXEC rbacCleanupOrphanedPermissions
GO
DROP PROCEDURE rbacCleanupOrphanedPermissions
GO
