DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAuthRetrieveUserPermissions')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiAuthRetrieveUserPermissions;
 	END IF;
END $$;

CREATE function csiAuthRetrieveUserPermissions(
	UserName varchar(100)
)
RETURNS TABLE (	
	RoleName VARCHAR,  
   	PermissionName VARCHAR,
   	PermissionType INTEGER,  
   PermissionMode INTEGER,
   OrganizationName VARCHAR,  
   ObjectMetaId INTEGER,  
   ObjectInstanceId BPCHAR(16),
   ObjectCDOName VARCHAR
)
as $$
declare
	v_system text;
	v_temp text;
begin
	
	v_system := 'SYSTEM';

	v_temp := 'tmpUserPer_' || to_char(current_timestamp, 'YYYYMMDDHH24MISSMS');
    execute format('create local TEMPORARY TABLE %I (
        RoleName varchar(255),  
        OrganizationName varchar(100)
    ) on commit drop', v_temp);

	EXECUTE format('INSERT INTO %I SELECT * FROM csiAuthRetrieveUserRoles($1)', v_temp) USING UserName;

	return QUERY
		execute format('SELECT 
		   tmp.RoleName,  
	       rp.RolePermissionName AS PermissionName,  
	       rp.PermissionType,  
	       rpm.Modes AS PermissionMode,  
	       tmp.OrganizationName,  
	       rp.ObjectMetaId,
	       rp.ObjectInstanceId ,
	       COALESCE(cdo.CDOName, $1)::varchar AS ObjectCDOName 
		FROM %I tmp 
		JOIN RoleDef r ON r.RoleName = tmp.RoleName 
		JOIN RolePermission rp ON rp.RoleId = r.RoleId  
		JOIN RolePermissionModes rpm ON rpm.RolePermissionId = rp.RolePermissionId 
		LEFT OUTER JOIN CDODefinition cdo ON cdo.CDODefID = rp.ObjectMetaId', v_temp) using v_system;

END;
$$ LANGUAGE plpgsql;