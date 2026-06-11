--------------------------------------------------------------------------------
-- SCRIPT:PopulateRBACDefaultData.sql
-- DESCR: Creates stored procedures used to create Roles, Permissions, etc.
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2023  


--------------------------------------------------------------------------------------------------
-- Function to create instance id strings from a CDODefId and Instance Id number
--
-- 
--  Modification History:
--  Name            	Date        Action
--  --------------      ----------  ----------------
--  Patrick Miller      8/18/2009    Initial Creation
--  Preston Holder      10/13/09     Added check for site, if found include in instanceId.
--  Ramesh Nagamalli    09/21/2011   Separated the csiPRDGetNextInstanceId function from this script so that it could be used elsewhere as well.
--  Oleg Khlus		02/20/2014   Removed inserting WebMenuDefinitionId for Employee.
--  Maksim Kutsak		04/29/2014 - Added new Roles to support Change Management.
--  Sergey Yakimchik	05/13/2014 - Added new Permissions for Default Pages
--  Maksim Kutsak		05/20/2014 - Added new Roles and permissions to support Change Management.
--  Alexey Miroshnichenko 04/02/2019 - Added new Roles and Permissions with User Query Maint service attached.
--  Oleg Kirasov        04/22/2019 - Added new Advanced Modeling Permissions.
--
--Copyright Siemens 2023  
 
--------------------------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCreateGUID')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCreateGUID;
 	END IF;
END $$;
CREATE PROCEDURE csiCreateGUID ( 
	PermissionName		VARCHAR(255),
	OUT RoleGID			VARCHAR(36)
)
LANGUAGE plpgsql
AS $$
DECLARE
	CDODefId	INTEGER;
	EmployeeId 	CHAR(16);
	OrgId		CHAR(16);
	IID			VARCHAR(16);
BEGIN
	SELECT (substr(ExportImportKeyGUID, 1, 8) ||
		'-' || substr(ExportImportKeyGUID, 9, 4) ||
		'-' || substr(ExportImportKeyGUID, 13, 4) ||
		'-' || substr(ExportImportKeyGUID, 17, 4) ||
		'-' || substr(ExportImportKeyGUID, 21,12))
	INTO RoleGID
	from 
	(
		SELECT UPPER(md5(PermissionName)) as ExportImportKeyGUID
	) c;
	raise notice '% perm', PermissionName;
	raise notice '% RoleGID', RoleGID;
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Helper function to create Role record
--
--  Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreateRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreateRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreateRole(
	pRoleName VARCHAR(50), 
	pRoleDescription VARCHAR(255)
	, OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE 
	vRoleCDODefId INTEGER := 7130;
BEGIN    

    CALL csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
	
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignRole(
	pRoleId CHAR(16), 
	pRoleDescription VARCHAR(255), 
	pEmployeeName VARCHAR(255), 
	pOrganizationName VARCHAR(255), 
	pPropagate INT
	)
LANGUAGE plpgsql
AS $$
DECLARE 
    vCDODefId INTEGER;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR(16);
    vRoleGID VARCHAR(36);
BEGIN
    
    SELECT EmployeeId INTO vEmployeeId
    FROM Employee
    WHERE EmployeeName=pEmployeeName;

    vOrgId := NULL;
    IF NOT pOrganizationName IS NULL THEN
        SELECT OrganizationId INTO vOrgId
        FROM Organization
        WHERE OrganizationName=pOrganizationName;
	END IF;
	
    vCDODefId := 7782;

    CALL csiPRDGetNextInstanceId(vCDODefId,vIID);
	pRoleDescription := pRoleDescription || pEmployeeName;
   
    CALL csiCreateGUID(pRoleDescription, vRoleGID);
    INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);
	
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreatePermission')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreatePermission;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreatePermission(
	pRoleId CHAR(16)
	, pPermissionName VARCHAR(255)
	, pPermissionType INTEGER
	, pObjectMetaId INTEGER
	, pPermissionModesFlag INTEGER
	, pObjectInstanceId CHAR(16) = NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
LANGUAGE plpgsql
AS $$
DECLARE 
    vIID VARCHAR(16);
	vRoleGID VARCHAR(36);
BEGIN    

    CALL csiPRDGetNextInstanceId(7783,vIID);    
	pPermissionName := TRIM(BOTH FROM pPermissionName);
	
    CALL csiCreateGUID(pPermissionName, vRoleGID);
    raise notice '% RoleGID', vRoleGID;
    
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(vRoleGID,vIID,7783,pRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the pPermissionModesFlag value
	IF ( pPermissionModesFlag = 0 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType;
    ElseIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180) ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
          AND BitNumber = 2;
    ELSEIF ( pPermissionModesFlag = 2 AND pPermissionType = 180 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
          AND BitNumber IN (1,2,3,4);
	ELSE
		RAISE NOTICE 'Error - Invalid value passed for @PermissionModeFlag parameter...';	
	END IF;

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreatePermissionsForQry')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreatePermissionsForQry;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreatePermissionsForQry(
	pRoleId CHAR(16)
	, pSecurityList TEXT
	, pPermissionModesFlag INTEGER
	)
LANGUAGE plpgsql
AS $$
DECLARE 
    vSQLString TEXT;
    vObjectMetaId INTEGER;
    vPermissionType INTEGER;
    vPermissionName VARCHAR(255);
	
	r refcursor; 
	rec record;
BEGIN
   
	vSQLString := 'Select DISTINCT CDO.CDODefId As ObjectMetaId ' ||
								', CDO.SecurityTypeId as PermissionType ' ||
								', Labels.LabelValue as PermissionName ' ||
						  'From	CDODefinition CDO ' ||
						  '		, Labels ' ||
						  'Where	Labels.LabelId = CDO.DisplayNameLabelId ' ||
						     'And		CDO.IsAbstract = 0 ' ||
						     'And		CDO.SecurityTypeId In (' || pSecurityList ||' ) ' ||
						   'GROUP BY CDO.CDODefId, CDO.SecurityTypeId, Labels.LabelValue ORDER BY Labels.LabelValue ';
	
	open r for execute vSQLString; 
	fetch next from r into rec;
	while found 
		loop
			vObjectMetaId := rec.ObjectMetaId;
			vPermissionType := rec.PermissionType;
			vPermissionName := rec.PermissionName;
			
			CALL csiRBACCreatePermission(pRoleId, vPermissionName, vPermissionType, vObjectMetaId, pPermissionModesFlag);
			
			fetch next from r into rec; 
		end loop;
	close r; 
	
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForExternal
-- DESCR: Creates list of External Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreatePermissionsForExternal')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreatePermissionsForExternal;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreatePermissionsForExternal(
	pRoleId CHAR(16)
	, pExternalPermission VARCHAR(255)
	, pPermissionModesFlag INTEGER
	)
LANGUAGE plpgsql
AS $$
DECLARE 
	vSQLString TEXT;    
    vObjectMetaId INTEGER;
    vPermissionType INTEGER;
    vPermissionName VARCHAR(255);
	
	r refcursor; 
	rec record;
BEGIN   

	vSQLString := 'SELECT ep.CDOTypeId, smd.SecurityMaskId AS SecTypeId, ep.ExternalPermissionName' ||
		' FROM ExternalPermission ep' ||
		' LEFT JOIN LATERAL (' ||
			'SELECT SecurityMaskId FROM SecurityMaskDefinition' ||
			' WHERE Name = ''' || pExternalPermission || ''' ) AS smd ON true' ||
		' ORDER BY ep.ExternalPermissionName;';

	open r for execute vSQLString; 
	fetch next from r into rec;
	while found 
		loop
			vObjectMetaId := rec.CDOTypeId;
			vPermissionType := rec.SecTypeId;
			vPermissionName := rec.ExternalPermissionName;
			
			CALL csiRBACCreatePermission(pRoleId, vPermissionName, vPermissionType, vObjectMetaId, pPermissionModesFlag);
			
			fetch next from r into rec; 
		end loop;
	close r; 
	
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAddVirtualPagePermissions 
-- DESCR: Creates permissions for all virtual pages
--		  @RoleId - The role to which the permissions are added
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAddVirtualPagePermissions')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAddVirtualPagePermissions;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAddVirtualPagePermissions(pRoleId CHAR(16))
LANGUAGE plpgsql
AS $$
DECLARE 
    vSQLString TEXT;
    vUIVirtualPageId CHAR(16);
    vPermissionName VARCHAR(255);
	
	r refcursor; 
	rec record;
BEGIN
   
   vSQLString := 'Select UIVirtualPageName, UIVirtualPageId ' ||
						 'From	UIVirtualPage ' ||
					   	 'Order By UIVirtualPageName ';
                  
	open r for execute vSQLString; 
	fetch next from r into rec;
	while found 
		loop
			
			vUIVirtualPageId := rec.UIVirtualPageId;
			vPermissionName := rec.UIVirtualPageName;
			
			CALL csiRBACCreatePermission(pRoleId, vPermissionName, 200, null, 0, vUIVirtualPageId);
			
			fetch next from r into rec; 
		end loop;
	close r; 

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignPermissionToRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignPermissionToRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignPermissionToRole(
	pRoleName VARCHAR(255)
	, pPermissionName VARCHAR(255)
	, pPermissionType INTEGER
	, pObjectMetaId INTEGER
	, pPermissionModesFlag INTEGER
	, pObjectInstanceId CHAR(16) = NULL)
LANGUAGE plpgsql
AS $$
DECLARE 
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    vIID VARCHAR(16);
	vRoleGID VARCHAR(36);
	vRoleId CHAR(16);
BEGIN
    
    CALL csiPRDGetNextInstanceId(7783,vIID);
    CALL csiCreateGUID(pPermissionName, vRoleGID);
	
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE RoleName = pRoleName;
	
	IF NOT EXISTS (SELECT * FROM RolePermission WHERE RoleId = vRoleId AND RolePermissionName = pPermissionName) THEN
    Begin  
		INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
				VALUES(vRoleGID,vIID,7783,vRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);

		-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
		-- based on the pPermissionModesFlag value
		IF ( pPermissionModesFlag = 0 ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType;
		ElseIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType
			  AND BitNumber = 2;
		ELSEIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230) ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType
			  AND BitNumber IN (1,2,3,4);
		ELSE
			RAISE NOTICE 'Error - Invalid value passed for @PermissionModeFlag parameter...';
		END IF;
	END;
	END IF;

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateDefaultData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('rbacPopulateDefaultData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS rbacPopulateDefaultData;
 	END IF;
END $$;
CREATE PROCEDURE rbacPopulateDefaultData()
LANGUAGE plpgsql
AS $$
DECLARE 
    vRoleId VARCHAR(16);
    vIID VARCHAR(16);
    vSessionId VARCHAR(16);
    vInstanceId VARCHAR(16);
    
    vPermissionSQL TEXT;
    vAdminPresent INTEGER;
    vInSiteAdminPresent INTEGER;
    
    vPermissionModesFlag_None INTEGER := 0;
    vPermissionModesFlag_ReadOnly INTEGER := 1;
    vPermissionModesFlag_NoSecAdmin INTEGER := 2;
BEGIN    

	SELECT COUNT(*) INTO vAdminPresent FROM Employee WHERE EmployeeName='Administrator';
	SELECT COUNT(*) INTO vInSiteAdminPresent FROM Employee WHERE EmployeeName='InSiteAdmin';

    RAISE NOTICE 'Creating CamstarAdmin...';
	-- Create Employee: CamstarAdmin (copy from InSiteAdmin)
    IF NOT EXISTS (SELECT EmployeeName FROM Employee WHERE EmployeeName='CamstarAdmin') THEN
    BEGIN
        CALL csiPRDGetNextInstanceId(1140,vIID);
        CALL csiPRDGetNextInstanceId(1130,vSessionId);
        INSERT INTO SessionValues (ExportImportKey,SessionValuesId, EmployeeId,  FactoryId, Application, Client, ChangeCount, CDOTypeId)  
        VALUES ('F18A0493-C55F-3D63-3B52-2FBF325A4930',vSessionId, vIID, NULL, 0,  0, 1, 1130);

        INSERT INTO Employee (EmployeeId,  EmployeeName, FullName, SessionValuesId, CanLogin, 
                           ChangeCount, ModelerAccess, CDOTypeId) 
        VALUES (vIID, 'CamstarAdmin', 'Camstar Administrator', vSessionId, 1, 1, 1, 1140);

		UPDATE EMPLOYEE 
		SET  PortalMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenu'),
		PortalMobileMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiMobileMenu'),
		PortalV8MenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenuV8')
		WHERE EmployeeName = 'CamstarAdmin';
    END;
	END IF;

    RAISE NOTICE 'Creating "Corporate" Organization...';
    -- Create Default Organization
    CALL csiPRDGetNextInstanceId(7543,vIID);
    INSERT INTO Organization(OrganizationId, OrganizationName, CDOTypeId, ChangeCount, Notes, ChangeHistoryId, Description, IconId, IsFrozen, ParentOrganizationId, OrganizationNumber)
       VALUES( vIID, 'Corporate', 7543, 1, NULL, NULL, 'Corporate Organization', 0, 0, NULL, NULL);

	RAISE NOTICE 'Creating "Login" Role...';
	CALL csiRBACCreateRole('Login','Login Access Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Login Access Role','InSiteAdmin', NULL, 0); 
    END;
	END IF;
	
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Login Access Role','Administrator', NULL, 0);
    END;
	END IF;

    CALL csiRBACAssignRole(vRoleId, 'Login Access Role','CamstarAdmin', NULL, 0);
    CALL csiRBACCreatePermission(vRoleId, 'System', 190, 1, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'MenuDefinitionMaint', 110, 6907, vPermissionModesFlag_ReadOnly);
    CALL csiRBACCreatePermission(vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);
    
    RAISE NOTICE 'Creating "Security Administration" Role...';
    CALL csiRBACCreateRole('Security Administration','Security Administration Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Security Administration Role','InSiteAdmin', NULL, 0);
    END;
	END IF;
	
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId,'Security Administration Role', 'Administrator', NULL, 0);
    END;
	END IF;
	
    CALL csiRBACAssignRole(vRoleId,'Security Administration Role', 'CamstarAdmin', NULL, 0);
    CALL csiRBACCreatePermission(vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'SetSessionFilterTagMaint', 180, 8777, vPermissionModesFlag_None);

    -- Each of the following blocks represent a query-based Role/Permission assignment for default services
    RAISE NOTICE 'Creating "Default Modeling" Role...';
    CALL csiRBACCreateRole('Default Modeling','Default Modeling Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Modeling Role','InSiteAdmin', NULL, 0);
    END;
	END IF;
	
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Modeling Role','Administrator', NULL, 0);
    END;
	END IF;
	
    CALL csiRBACAssignRole(vRoleId, 'Default Modeling Role','CamstarAdmin', NULL, 0);
    vPermissionSQL := '110';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_NoSecAdmin);
    CALL csiRBACCreatePermission(vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_NoSecAdmin);
	CALL csiRBACCreatePermissionsForExternal(vRoleId, 'ExternalPermission', vPermissionModesFlag_NoSecAdmin);

    RAISE NOTICE 'Creating "Default Modeling Read-Only" Role...';
    CALL csiRBACCreateRole('Default Modeling Read-Only','Default Modeling Read-Only Role',vRoleId);
    vPermissionSQL := '110';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_ReadOnly);
    CALL csiRBACCreatePermission(vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);
    CALL csiRBACCreatePermission(vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermissionsForExternal(vRoleId, 'ExternalPermission', vPermissionModesFlag_ReadOnly);

    RAISE NOTICE 'Creating "Default Mfg" Role...';
    CALL csiRBACCreateRole('Default Mfg','Default Manufacturing Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Manufacturing Role','InSiteAdmin', NULL, 0);
    END;
	END IF;
	
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Manufacturing Role','Administrator', NULL, 0);
    END;
	END IF;
	
    CALL csiRBACAssignRole(vRoleId, 'Default Manufacturing Role','CamstarAdmin', NULL, 0);
    vPermissionSQL := '100, 120, 130';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);


    RAISE NOTICE 'Creating "Default Quality" Role...';
    CALL csiRBACCreateRole('Default Quality','Default Quality Role',vRoleId);
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Quality Role','Administrator', 'Corporate', 1);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId, 'Default Quality Role','CamstarAdmin', 'Corporate', 1);
	vPermissionSQL := '170';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	

    RAISE NOTICE 'Creating "Default Inquiry" Role...';
    CALL csiRBACCreateRole('Default Inquiry','Default Inquiry Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Inquiry Role','InSiteAdmin', NULL, 0);
    END;
	END IF;
	
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Inquiry Role','Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId, 'Default Inquiry Role','CamstarAdmin', NULL, 0);
	vPermissionSQL := '140';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);


    RAISE NOTICE 'Creating "Default Export Import" Role...';
    CALL csiRBACCreateRole('Default Export Import','Default Export Import Role',vRoleId);
    IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Export Import Role','InSiteAdmin', NULL, 0);
    END;
	END IF;
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Export Import Role','Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId, 'Default Export Import Role','CamstarAdmin', NULL, 0);
    vPermissionSQL := '150,160';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);

    
    RAISE NOTICE 'Creating "Default Pages" Role...';
    CALL csiRBACCreateRole('Default Pages','Default Portal Pages Role',vRoleId);
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Default Portal Pages Role','Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0);
	CALL csiRBACAssignRole(vRoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0);
    CALL csiRBACAddVirtualPagePermissions(vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UI Virtual Page Maint', 110, 7755, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Web Part Maint', 110, 8143, vPermissionModesFlag_None);
    
    RAISE NOTICE 'Creating "Mfg Audit Trail Inquiry" Role...';
    CALL csiRBACCreateRole('Mfg Audit Trail Inquiry','Mfg Audit Trail Inquiry Role',vRoleId);
    CALL csiRBACCreatePermission(vRoleId, 'Container Txn Rev', 130, 5440, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'Container History Inquiry', 140, 6908, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'History View Maint', 110, 7083, vPermissionModesFlag_ReadOnly);
    
    RAISE NOTICE 'Creating "Portal Configuration" Role...';
    CALL csiRBACCreateRole('Portal Configuration','Portal Configuration Role',vRoleId);
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId,'Portal Configuration Role', 'Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId,'Portal Configuration Role', 'CamstarAdmin', NULL, 0);
    CALL csiRBACCreatePermission(vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Portal Studio RBAC', 210, 3, vPermissionModesFlag_None);

    RAISE NOTICE 'Creating "SPC" Role...';
    
    SELECT UIVirtualPageId INTO vInstanceId
      FROM UIVirtualPage
     WHERE UIVirtualPageName='SPCTesterFormVP';
     
     IF (vInstanceId is null) THEN
        BEGIN
           vInstanceId := null;
        END;
	END IF;
    
	 

	CALL csiRBACCreateRole('SPC','SPC',vRoleId);
    CALL csiRBACAssignRole(vRoleId, 'SPC','CamstarAdmin', NULL, 0);
    CALL csiRBACCreatePermission(vRoleId, 'SPCChartDefMaint', 110, 8204, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'SPCTesterFormVP', 200, NULL, vPermissionModesFlag_None, vInstanceId);
    CALL csiRBACCreatePermission(vRoleId, 'Add SPC Annotation', 120, 8393, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'Record SPC Violation', 120, 8396, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DraftPermissions" Role...'; 
	CALL csiRBACCreateRole('DraftPermissions','Draft Permissions Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DraftCamstarPermissions" Role...'; 
	CALL csiRBACCreateRole('DraftCamstarPermissions','Draft Camstar Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DraftPLMPermissions" Role...'; 
	CALL csiRBACCreateRole('DraftPLMPermissions','Draft PLM Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DEPCPermissions" Role...'; 
	CALL csiRBACCreateRole('DEPCPermissions','Deployment Complete Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DEPIPermissions" Role...'; 
	CALL csiRBACCreateRole('DEPIPermissions','Deployment Incomplete Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "PackageCreator" Role...';
	CALL csiRBACCreateRole('Package Creator','Package Creator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'Start Change Pkg', 120, 8500, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	
	RAISE NOTICE 'Creating "PackageOwner" Role...';
	CALL csiRBACCreateRole('Package Owner','Package Owner Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Routing Sheet Maint', 110, 7820, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	


	
	
	RAISE NOTICE 'Creating "Package Deployer" Role...'; 
	CALL csiRBACCreateRole('Package Deployer','Package Deployer Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	


	
	RAISE NOTICE 'Creating "Package Activator" Role...'; 
	CALL csiRBACCreateRole('Package Activator','Package Activator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'ActivateChangePkg', 120, 8528, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ActivationInquiry', 140, 8554, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None); 
	CALL csiRBACCreatePermission(vRoleId, 'Export/Import Controller', 160, 7392, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Import Status Inquiry', 140, 7397, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Modeling data Import', 150, 7391, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	RAISE NOTICE 'Creating "Package Approver" Role...';
	CALL csiRBACCreateRole('Package Approver','Package Approver Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	



	RAISE NOTICE 'Creating "Package Collaborator" Role...'; 
	CALL csiRBACCreateRole('Package Collaborator','Package Collaborator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	



	RAISE NOTICE 'Creating "PDPermissions" Role...'; 
	CALL csiRBACCreateRole('PDPermissions','Pending Deployment Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

	RAISE NOTICE 'Creating "RejectPermissions" Role...'; 
	CALL csiRBACCreateRole('RejectPermissions','Rejected Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	RAISE NOTICE 'Creating "PACPermissions" Role...'; 
	CALL csiRBACCreateRole('PACPermissions','Pending Approval Camstar Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
		
		
	RAISE NOTICE 'Creating "PAPLMPermissions" Role...'; 
	CALL csiRBACCreateRole('PAPLMPermissions','Pending Approval PLM Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	

	RAISE NOTICE 'Creating "Default Modeling Advanced" Role...'; 
	CALL csiRBACCreateRole('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
	IF (vInSiteAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Modeling Services for Advanced Users','InSiteAdmin', NULL, 0);
    END;
	END IF;
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId, 'Modeling Services for Advanced Users','Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId, 'Modeling Services for Advanced Users','CamstarAdmin', NULL, 0);
    vPermissionSQL := '230';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	RAISE NOTICE 'Assign "User Query Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Business Rule Handler Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Business Rule Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Scheduled Business Rule Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Summary Table Def Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);

	RAISE NOTICE 'Complete.';

end $$;


do $$ 
begin	
 call rbacPopulateDefaultData();
 DROP PROCEDURE IF EXISTS rbacPopulateDefaultData;
end $$;

do $$ 
begin	
 DROP PROCEDURE IF EXISTS csiRBACAddVirtualPagePermissions;
end $$;


/*
// Scripts to remove all RBAC data
DELETE FROM EMPLOYEE WHERE EMPLOYEENAME IN ('CamstarAdmin');
DELETE FROM ROLEDEF;
DELETE FROM ORGANIZATION WHERE ORGANIZATIONNAME = 'Corporate';
DELETE FROM EMPLOYEEROLE;
DELETE FROM ROLEPERMISSION;
DELETE FROM ROLEPERMISSIONMODES;
*/
