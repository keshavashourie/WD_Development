--------------------------------------------------------------------------------
-- SCRIPT:PopulatePortalMenuDefaultData.sql
-- DESCR: Creates stored procedures used to create PortalMenuDefinitions and PortalMenuItems
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2024  

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
END;
$$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM reakted roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
	pRoleName VARCHAR(50)
	, pRoleDescription VARCHAR(255)
	, OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vRoleCDODefId INTEGER := 7130;
BEGIN

    CALL csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
	
    IF NOT EXISTS (SELECT * FROM RoleDef WHERE RoleName = pRoleName) THEN
    Begin
		INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
		   VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
	end;
    ELSE
	Begin
		delete from EmployeeRole WHERE RoleId = (Select RoleId from RoleDef WHERE RoleName = pRoleName);
		delete from RoleDef WHERE RoleName = pRoleName;
		INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
		   VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
		
		RAISE NOTICE '[RoleDef] % deleted and created', pRoleName;
	end;
	END IF;
		
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignRoleIfExist')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignRoleIfExist;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignRoleIfExist(
	pExRoleName VARCHAR(255)
	, pNewRoleId CHAR(16)
	, pNewRoleDescription VARCHAR(255)
	, pOrganizationName VARCHAR(255)
	, pPropagate INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
    vCDODefId INTEGER;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR(16);
    vRoleGID VARCHAR(36);
	vEmployeeName VARCHAR (500);
BEGIN
	
	FOR vEmployeeName IN 
        SELECT e.EmployeeName 
        FROM EmployeeRole er
        LEFT JOIN RoleDef rd ON er.RoleId = rd.RoleId
        LEFT JOIN Employee e ON er.EmployeeId = e.EmployeeId
        WHERE rd.RoleName = pExRoleName
    LOOP
	
		SELECT EmployeeId INTO vEmployeeId
		FROM Employee
		WHERE EmployeeName=vEmployeeName;

		vOrgId:=NULL;
		
		IF NOT pOrganizationName IS NULL THEN
			SELECT OrganizationId INTO vOrgId
			FROM Organization
			WHERE OrganizationName=pOrganizationName;
		END IF;			

		vCDODefId := 7782;

		CALL csiPRDGetNextInstanceId(vCDODefId,vIID);
		
		pNewRoleDescription := pNewRoleDescription || vEmployeeName;
		
		CALL csiCreateGUID(pNewRoleDescription, vRoleGID);
		
		INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
        VALUES (vRoleGID,vIID, vCDODefId, pNewRoleId, vEmployeeId, 0, pPropagate, vOrgId);
			   	
	END LOOP;	 
		
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        return InstanceId number
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('getNextInstanceId')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS getNextInstanceId;
 	END IF;
END $$;
CREATE PROCEDURE getNextInstanceId(
	pCDODefId integer
	, OUT pInstanceIdStr varchar(16))
 LANGUAGE plpgsql
AS $$
DECLARE
    vCDODefIdStr VARCHAR(16);
    vInstanceId INTEGER;
    vInstIdNewValue CHAR(16);
begin
	
    -- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
    -- Length should be 10 chars
	call csiUpdateInstanceId(0,pCDODefId,1,vInstIdNewValue);
    vInstIdNewValue := LPAD(SUBSTRING(vInstIdNewValue FROM 14 FOR 10), 10, '0');
    
    -- Convert CDODef Id to hex (pad to 6 chars)
    vCDODefIdStr := LPAD(TO_HEX(pCDODefId), 6, '0');

    -- Join CDODef hex and instance id hex strings. Length=16 chars
    pInstanceIdStr := LOWER(vCDODefIdStr || vInstIdNewValue);
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalMenuDefinition record
--
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createPortalMenuDefinition')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createPortalMenuDefinition;
 	END IF;
END $$;
CREATE PROCEDURE createPortalMenuDefinition(
	pPortalMenuDefinitionName VARCHAR(30), 
	pDescription VARCHAR(255), 
	pNotes VARCHAR(2000), 
	OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vPortalMenuDefCDODefId INTEGER := 7828;
BEGIN
        
    CALL getNextInstanceId(vPortalMenuDefCDODefId,pInstanceId);
    
    IF NOT EXISTS (SELECT * FROM PortalMenuDefinition WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName) THEN
    BEGIN
		INSERT 
		INTO PortalMenuDefinition
        (PortalMenuDefinitionId
        ,CDOTypeId
        ,ChangeCount
        ,Notes
        ,ChangeHistoryId
        ,Description
        ,IconId
        ,IsFrozen
        ,PortalMenuDefinitionName)
		VALUES
        (pInstanceId				--, char(16),>
        ,vPortalMenuDefCDODefId		--, int,>
        ,1							--, int,>
        ,pNotes						--, nvarchar(2000),>
        ,NULL						--, char(16),>
        ,pDescription				--, nvarchar(255),>
        ,0							--, int,>
        ,0							--, bit,>
        ,pPortalMenuDefinitionName);	--, nvarchar(30),>)
	END;
    ELSE
		SELECT DISTINCT Portalmenudefinitionid INTO pInstanceId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName;
	END IF;

END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACDeleteUnavailablePermissions')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACDeleteUnavailablePermissions;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACDeleteUnavailablePermissions(
	pPermissionType INTEGER, 
	pPermissionName VARCHAR(255))
LANGUAGE plpgsql
AS $$
DECLARE
	vUnavialiblePermissions CHAR(16);
BEGIN
	
	FOR vUnavialiblePermissions IN 
        SELECT RolePermission.RolePermissionId FROM RolePermission
		WHERE RolePermissionName = pPermissionName AND PermissionType <> pPermissionType
    LOOP
	
		DELETE FROM RolePermission WHERE RolePermissionId = vUnavialiblePermissions;
		DELETE FROM RolePermissionModes WHERE RolePermissionId = vUnavialiblePermissions;
		
	END LOOP;	

END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createPortalMenuItem')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createPortalMenuItem;
 	END IF;
END $$;
CREATE PROCEDURE createPortalMenuItem(
	pParentId				char(16),
	pCDOTypeId				integer, 
	pSequence				integer,
	pCaption				varchar(50),
	pLabelname				varchar(66),
	pVirtualPageName		varchar(30),
	pPageFlowName			varchar(30),
	pSubMenuName			varchar(30),
	pApolloIconName			varchar(128) = NULL,
	pServiceName		    varchar(128) = NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
	vMenuItemId		varchar(16);
	vSubMenuId		varchar(16);
	vVirtualPageId	varchar(16);
	vPageFlowId		varchar(16);
BEGIN    

	IF LENGTH(pSubMenuName)>0 THEN
		SELECT DISTINCT PortalMenuDefinitionId INTO vSubMenuId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = pSubMenuName;
	END IF;
	IF LENGTH(pVirtualPageName)>0 THEN
		SELECT DISTINCT UIVirtualPageId INTO vVirtualPageId
		FROM UIVirtualPage 
		WHERE UIVirtualPageName = pVirtualPageName;
	END IF;
	IF LENGTH(pPageFlowName)>0 THEN
		SELECT DISTINCT UIPageFlowId INTO vPageFlowId
		FROM UIPageFlow 
		WHERE UIPageFlowName = pPageFlowName;
	END IF;
		
    CALL getNextInstanceId(pCDOTypeId,vMenuItemId);

	IF pSubMenuName = 'csiChangeManagement' THEN
	BEGIN
		IF NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption = 'Change Management') THEN
        BEGIN
			INSERT 
			INTO PortalMenuItem
      		(PortalMenuItemId
      		,CDOTypeId
      		,ChangeCount
      		,ParentId
      		,IsFrozen
       		,Caption
       		,LabelName
      		,Sequence
       		,MenuDefinitionId
 			,VirtualPageId
 			,PageFlowId
 			,PageURL
 			,PageDisplay
 			,QueryString
			,ApolloIcon
			,ServiceName)
 			VALUES
      		(vMenuItemId			--, char(16),>
      		,pCDOTypeId				--, int,>
      		,1						--, int,>
     		,pParentId				--, char(16),>
      		,0						--, bit,>
    		,pCaption				--, nvarchar(50),>
    		,pLabelname
			,pSequence				--, int,>
			,vSubMenuId				--, char(16),>
			,vVirtualPageId			--, char(16),>
 			,vPageFlowId				--, char(16),>
			,null					--, nvarchar(512),>
			,null					--, int,>
 			,null					--, nvarchar(512),>
			,pApolloIconName		--, nvarchar(30),>
			,pServiceName);          --, nvarchar(32),>)
		END;
		ELSE
			RAISE NOTICE 'csichangemanagement already exists';
		END IF;
	END;
	ELSE
		IF ((NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption= pCaption)) 
		OR 
			(EXISTS(
				SELECT * from PortalMenuDefinition 
				WHERE pParentId = PortalMenuDefinitionId AND PortalMenuDefinitionName LIKE '%V8%' 
				)
			AND
			(NOT EXISTS 
				(SELECT * FROM PortalMenuitem 
				JOIN PortalMenuDefinition ON ParentId= PortalMenuDefinitionId
					WHERE pParentId = PortalMenuDefinitionId AND PortalMenuDefinitionName LIKE '%V8%' AND caption= pCaption
					)
				)
			)
		) THEN
        BEGIN
			INSERT 
			INTO PortalMenuItem
           (PortalMenuItemId
           ,CDOTypeId
           ,ChangeCount
           ,ParentId
           ,IsFrozen
           ,Caption
           ,LabelName
           ,Sequence
           ,MenuDefinitionId
           ,VirtualPageId
           ,PageFlowId
           ,PageURL
           ,PageDisplay
           ,QueryString
           ,ApolloIcon)
			VALUES
           (vMenuItemId				--, char(16),>
           ,pCDOTypeId				--, int,>
           ,1						--, int,>
           ,pParentId				--, char(16),>
           ,0						--, bit,>
           ,pCaption				--, nvarchar(50),>
           ,pLabelname
           ,pSequence				--, int,>
           ,vSubMenuId				--, char(16),>
           ,vVirtualPageId			--, char(16),>
           ,vPageFlowId				--, char(16),>
           ,null					--, nvarchar(512),>
           ,null					--, int,>
           ,null					--, nvarchar(512),>
		   ,pApolloIconName);		--, nvarchar(30),>)
		END;
		ELSE
			SELECT portalmenuitemid INTO vMenuItemId
			FROM portalmenuitem 
			WHERE Caption = pCaption;
		END IF;
	END IF;

END $$;


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
	pRoleName VARCHAR(255), 
	pPermissionName VARCHAR(255), 
	pPermissionType INTEGER, 
	pObjectMetaId INTEGER, 
	pPermissionModesFlag INTEGER, 
	pObjectInstanceId CHAR(16) = NULL)
LANGUAGE plpgsql
AS $$
DECLARE
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    vIID 		VARCHAR(16);
	vRoleGID 	VARCHAR(36);
	vRoleId 	CHAR(16);
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

END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
-- Copyright Siemens 2023  
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
	pRoleName VARCHAR(255), 
	pPermissionName VARCHAR(255), 
	pPermissionType INTEGER, 
	pObjectMetaId INTEGER, 
	pPermissionModesFlag INTEGER, 
	pObjectInstanceId CHAR(16) = NULL)
LANGUAGE plpgsql
AS $$
DECLARE
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    vIID 		VARCHAR(16);
	vRoleGID 	VARCHAR(36);
	vRoleId 	CHAR(16);
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

END $$;


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
	pPropagate INTEGER)
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

    vOrgId:=NULL;
    IF NOT pOrganizationName IS NULL THEN	
        SELECT OrganizationId INTO vOrgId
        FROM Organization
        WHERE OrganizationName=pOrganizationName;
	END IF;
	
	vCDODefId:=7782;

	CALL csiPRDGetNextInstanceId(vCDODefId,vIID);
	pRoleDescription = pRoleDescription || pEmployeeName;
	CALL csiCreateGUID(pRoleDescription, vRoleGID);

	IF NOT EXISTS (SELECT * FROM employeerole WHERE Roleid = pRoleId) THEN
	BEGIN
		INSERT 
		INTO EmployeeRole
		(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
		VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);
	END;
	END IF;

END $$;


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
	pRoleId CHAR(16), 
	pSecurityList TEXT, 
	pPermissionModesFlag INTEGER)
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
	
	vSQLString := 'Select CDO.CDODefId As ObjectMetaId ' ||
					', CDO.SecurityTypeId as PermissionType ' ||
					', Labels.LabelValue as PermissionName ' ||
					'From	CDODefinition CDO ' ||
					'		, Labels ' ||
					'Where	Labels.LabelId = CDO.DisplayNameLabelId ' ||
					'And		CDO.IsAbstract = 0 ' ||
					'And		CDO.SecurityTypeId In ('|| pSecurityList ||' ) ' ||
					'ORDER By CDO.SecurityTypeId, CDO.CDOName ';

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

END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAddCMVirtualPagePermissions 
-- DESCR: Creates permissions for all virtual pages
--		  @RoleId - The role to which the permissions are added
-- Copyright Siemens 2023  

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAddCMVirtualPagePermissions')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAddCMVirtualPagePermissions;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAddCMVirtualPagePermissions(
	pRoleId CHAR(16))
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
	
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Create Portal Menus 
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('populatePortalMenuUpdateData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS populatePortalMenuUpdateData;
 	END IF;
END $$;
CREATE PROCEDURE populatePortalMenuUpdateData()
LANGUAGE plpgsql
AS $$
DECLARE
	vMenuDefId varchar(16);
	vAssignedMenuDefId varchar(16);
	vHomePage varchar(16);
	vDefaultNotes varchar(2000);
	vRoleId VARCHAR(16);
	vPermissionModesFlag_None INTEGER := 0;
	vPermissionModesFlag_ReadOnly INTEGER := 1;
	vPermissionModesFlag_NoSecAdmin INTEGER := 2;
	vPermissionSQL TEXT;
	vInstanceId VARCHAR(16);
	vcsiPortalMenuPortalMenuDefinitionId varchar(16);
	vcsiMobileMenuPortalMenuDefinitionId varchar(16);
	vcsiPortalMenuV8PortalMenuDefinitionId varchar(16);
BEGIN
		
		vDefaultNotes := 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.';
			
		RAISE NOTICE 'Creating Change Management Menu...';
		CALL createPortalMenuDefinition('csiChangeManagement', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', '');
		
		RAISE NOTICE 'Creating Attachments Menu...';
		CALL createPortalMenuDefinition('csiAttachments', 'Attachments', vDefaultNotes, vMenuDefId);

		DELETE FROM PortalMenuItem 
		WHERE  Caption = 'Attach Documents';

		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Attach Document','Lbl_AttachDocument_Title', 'AttachDocument_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', '');

		SELECT PortalMenuDefinitionId INTO vMenuDefId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiPortalMenu';

		RAISE NOTICE 'Creating Portal Main Menu...';
		CALL createPortalMenuItem(vMenuDefId, 7833, 03, 'Attachments', 'LblMenuAttachments','', '', 'csiAttachments', 'cmdAttach');
		CALL createPortalMenuItem(vMenuDefId, 7833, 05, 'Change Management','LblMenuChgMngt', '', '', 'csiChangeManagement', 'cmdChangeManagement');
		
		--'Creating Container Mobile Sub Menu'
		RAISE NOTICE 'Creating Container Mobile Sub Menu...';
		CALL createPortalMenuDefinition('csiContainerMobileMenu', 'Mobile Menu for Container Txns', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Move', 'LblMenuMove', 'MoveStdVP_R2', '', '');

		--'Creating Mobile Main Menu'
		RAISE NOTICE 'Creating Mobile Main Menu...';
		CALL createPortalMenuDefinition('csiMobileMenu', 'The top level Mobile Portal menu', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7833, 03, 'Container', 'LblMenuCont','', '', 'csiContainerMobileMenu', 'cmdLot');

		SELECT PortalMenuDefinitionId INTO vMenuDefId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiSearch';

		DELETE FROM PortalMenuItem 
		WHERE  Caption = 'Audit Trail';

		RAISE NOTICE 'Creating Search Menu...';
		CALL createPortalMenuItem(vMenuDefId, 7835, 35, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 45, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 55, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP', '', '');

		--Classic Container menu update
		SELECT PortalMenuDefinitionId INTO vMenuDefId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiContainer';

		CALL createPortalMenuItem(vMenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '');
	
		CALL createPortalMenuItem(vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate');
		CALL createPortalMenuItem(vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate');
		CALL createPortalMenuItem(vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
		CALL createPortalMenuItem(vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');
	
		SELECT PortalMenuDefinitionId INTO vMenuDefId
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiModelingMenu';

		DELETE FROM PortalMenuItem 
		WHERE Caption = 'Audit Trail';

		RAISE NOTICE 'Creating Modeling Menu...';
		CALL createPortalMenuItem(vMenuDefId, 7835, 25, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', '');
	
		-- Each of the following blocks represent a query-based Role/Permission assignment for default services
		RAISE NOTICE 'Creating "Default Modeling" Role...';

		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling';

		vPermissionSQL := '110';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Employee Maint', 180, 3760, vPermissionModesFlag_NoSecAdmin);
		--CALL csiRBACCreatePermission(vRoleId, 'Role Maint', 180, 7140, vPermissionModesFlag_NoSecAdmin);

		--RAISE NOTICE 'Creating "Default Modeling Advanced" Role...'; 
		--CALL csiRBACCreateRole('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
		--CALL csiRBACAssignRoleIfExist('Default Modeling', vRoleId, 'Modeling Services for Advanced Users', NULL, 0);
		vPermissionSQL := '230';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
		--CALL csiRBACDeleteUnavailablePermissions(230, 'User Query Maint');
		--CALL csiRBACDeleteUnavailablePermissions(230, 'Business Rule Handler Maint');
		--CALL csiRBACDeleteUnavailablePermissions(230, 'Business Rule Maint');
		--CALL csiRBACDeleteUnavailablePermissions(230, 'Scheduled Business Rule Maint');
		--CALL csiRBACDeleteUnavailablePermissions(230, 'Summary Table Def Maint');


		RAISE NOTICE 'Creating "Default Modeling Advanced" Role...'; 
		
		SELECT RoleId INTO vRoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling Advanced';

		vPermissionSQL = '230';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);


		RAISE NOTICE 'Creating "Default Inquiry" Role...';
    
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Inquiry';

		vPermissionSQL := '140';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);


		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Pages';

		--CALL csiRBACAssignRole(vRoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0);
		--CALL csiRBACAssignRole(vRoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0);
		--CALL csiRBACAddCMVirtualPagePermissions(vRoleId);
		--CALL csiRBACCreatePermission(vRoleId, 'UI Virtual Page Maint', 110, 7755, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Web Part Maint', 110, 8143, vPermissionModesFlag_None);
	
	    --RAISE NOTICE 'Creating "Login" Role...';
		
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Login';

		--CALL csiRBACCreatePermission(vRoleId, 'System', 190, 1, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Menu Definition Maint', 110, 6907, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACCreatePermission(vRoleId, 'Employee Maint', 180, 3760, vPermissionModesFlag_ReadOnly);
    
		--RAISE NOTICE 'Creating "Security Administration" Role...';
    
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Security Administration';

		--CALL csiRBACCreatePermission(vRoleId, 'Employee Maint', 180, 3760, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Role Maint', 180, 7140, vPermissionModesFlag_None);

		--RAISE NOTICE 'Creating "Default Modeling Read-Only" Role...';
    
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling Read-Only';

		vPermissionSQL := '110';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACCreatePermission(vRoleId, 'Employee Maint', 180, 3760, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACCreatePermission(vRoleId, 'Role Maint', 180, 7140, vPermissionModesFlag_ReadOnly);

		--RAISE NOTICE 'Assign "User Query Maint" Permission...';
		--CALL csiRBACAssignPermissionToRole('Default Modeling', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
		--RAISE NOTICE 'Assign "Business Rule Handler Maint" Permission...';
		--CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
		--RAISE NOTICE 'Assign "Business Rule Maint" Permission...';
		--CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
		--RAISE NOTICE 'Assign "Scheduled Business Rule Maint" Permission...';
		--CALL csiRBACAssignPermissionToRole('Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
		--RAISE NOTICE 'Assign "Summary Table Def Maint" Permission...';
		--CALL csiRBACAssignPermissionToRole('Default Modeling', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
		--CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);


		--RAISE NOTICE 'Creating "Default Mfg" Role...';
    
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Mfg';

		vPermissionSQL := '100, 120, 130';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);

		RAISE NOTICE 'Creating "Default Quality" Role...';
		
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Quality';

		vPermissionSQL := '170';
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);

		RAISE NOTICE 'Creating "Default Export Import" Role...';
		
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Default Export Import';

		vPermissionSQL := '150,160' ;
		--CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
       
		--RAISE NOTICE 'Creating "Mfg Audit Trail Inquiry" Role...';
		
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Mfg Audit Trail Inquiry';

		--CALL csiRBACCreatePermission(vRoleId, 'Container Txn Rev', 130, 5440, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Container History Inquiry', 140, 6908, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'History View Maint', 110, 7083, vPermissionModesFlag_ReadOnly);
    
		--RAISE NOTICE 'Creating "Portal Configuration" Role...';
		
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'Portal Configuration';

		--CALL csiRBACCreatePermission(vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);

		--RAISE NOTICE 'Creating "SPC" Role...';
    
		SELECT UIVirtualPageId INTO vInstanceId
		FROM UIVirtualPage
		WHERE UIVirtualPageName='SPCTesterFormVP';
     
		IF (vInstanceId is null) THEN
        BEGIN
			vInstanceId:=null;
        END;
		END IF;
    
		SELECT RoleId INTO vRoleId
		FROM RoleDef 
		WHERE Rolename = 'SPC';

		--CALL csiRBACCreatePermission(vRoleId, 'SPCChartDefMaint', 110, 8204, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'SPCTesterFormVP', 200, NULL, vPermissionModesFlag_None, vInstanceId);
		--CALL csiRBACCreatePermission(vRoleId, 'Add SPC Annotation', 120, 8393, vPermissionModesFlag_None);
		--CALL csiRBACCreatePermission(vRoleId, 'Record SPC Violation', 120, 8396, vPermissionModesFlag_None);

		--Create V8 Menu with Apollo
		RAISE NOTICE 'Creating Container Transaction V8 Menu Definition...';
		CALL createPortalMenuDefinition('csiContainerV8', 'Container transactions used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssue_VPR2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 130, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 134, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate');
		CALL createPortalMenuItem(vMenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EProcedureVPR2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVPR2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VPR2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
		CALL createPortalMenuItem(vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');
		CALL createPortalMenuItem(vMenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 410, 'HV Component Issue', 'CSICDOName_HVComponentIssue','HVComponentIssueVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 420, 'Slitting', 'CSICDOName_Slitting','SlittingVP', '', ''); 	
	
		RAISE NOTICE 'Creating Job Services Menu...';
		CALL createPortalMenuDefinition('csiResourceTxn_JobSvc', 'Job Services', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Job Create', 'CSICDOName_JobCreate','JobCreateVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Job Assign', 'CSICDOName_JobAssign','JobAssign_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Job Acknowledge', 'CSICDOName_JobAcknowledge','JobAcknowledge_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Job Clock On', 'CSICDOName_JobClockOn','JobClockOn_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Job Progress', 'CSICDOName_JobProgress','JobProgress_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 60, 'Job Clock Off', 'CSICDOName_JobClockOff','JobClockOff_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 70, 'Job Complete', 'CSICDOName_JobComplete','JobComplete_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 80, 'Job Cancel', 'CSICDOName_JobCancel','JobCancel_VP', '', '');
		
		RAISE NOTICE 'Creating Jobs Menu...';
		CALL createPortalMenuDefinition('csiResourceTxn_Job', 'Jobs', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Supervisor Jobs', 'LblMenuSupervisorJobs','JobSupervisor_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Technician Jobs', 'LblMenuTechnicianJobs','JobTechnicians_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7833, 30, 'Job Services', 'PortalUI_JobServices', '','', 'csiResourceTxn_JobSvc', '');
		
		RAISE NOTICE 'Creating Part Services Menu...';
		CALL createPortalMenuDefinition('csiResourceTxn_PartSvc', 'Part Services', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Part Create', 'CSICDOName_PartCreate','PartCreate_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Part Setup', 'CSICDOName_PartSetup','PartSetupVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Part Scrap', 'CSICDOName_PartScrap','PartScrap_VP', '', ''	);
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Part Request', 'CSICDOName_PartRequest','PartRequestVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Part Request Acknowledge', 'CSICDOName_PartRequestAcknowledge','PartRequestAcknowledgeVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 60, 'Part Request Assign', 'CSICDOName_PartRequestAssign','PartRequestAssign_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 70, 'Part Request Update', 'CSICDOName_PartRequestUpdate','PartRequestUpdateVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 80, 'Part Request Issue', 'CSICDOName_PartRequestIssue','PartRequestIssue_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 90, 'Part Request Complete', 'CSICDOName_PartRequestComplete','PartRequestComplete_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 100, 'Part Request Cancel', 'CSICDOName_PartRequestCancel','PartRequestCancel_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 110, 'Part Request Cancel Acknowledge', 'CSICDOName_PartRequestCancelAcknowledge','PartRequestCancelAcknowledgeVP', '', '');
		
		RAISE NOTICE 'Creating Parts Menu...';
		CALL createPortalMenuDefinition('csiResourceTxn_Part', 'Parts', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Material Parts', 'PartRequestOrder_MaterialParts','MaterialPartMaintenanceVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Part Maintenance', 'LblPartMaintenance','PartMaintenanceVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Technician Requests', 'LblTechnicianRequests','PartRequestMain_VP', '', '', '', 'PartRequest');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Inventory Requests', 'LblInventoryRequests','PartRequestMain_VP', '', '', '', 'PartRequestAssign');
		CALL createPortalMenuItem(vMenuDefId, 7833, 50, 'Part Services', 'PortalUI_PartServices', '','', 'csiResourceTxn_PartSvc', '');

		RAISE NOTICE 'Creating Resource V8 Menu...';
		CALL createPortalMenuDefinition('csiResourceV8', 'Resource transactions available in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP_R2', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 80, 'Jobs', 'PortalUI_Jobs', '','', 'csiResourceTxn_Job', '' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 90, 'Parts', 'Resource_Parts', '','', 'csiResourceTxn_Part', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 100, 'HV Resource Setup', 'CSICDOName_HVResourceSetup','HVResourceSetupVP', '', '' );

		RAISE NOTICE 'Creating Events V8 Menu...';
		CALL createPortalMenuDefinition('csiEventV8', 'Quality event transactions in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', '' );

		RAISE NOTICE 'Creating Search V8 Menu...';
		CALL createPortalMenuDefinition('csiSearchV8', 'Search options in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP_R2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP_R2', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '' );

		RAISE NOTICE 'Creating SPC V8 Menu...';
		CALL createPortalMenuDefinition('csiSPCV8', 'SPC pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'SPC Realtime Monitoring', 'LblSPCRealtimeMonitoring', 'SPCRealtimeMonitoring_VP', '', '' );
		
		RAISE NOTICE 'Creating Modeling V8 Menu...';
		CALL createPortalMenuDefinition('csiModelingMenuV8', 'Modeling pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', '');
		
		RAISE NOTICE 'Creating Training V8 Menu...';
		CALL createPortalMenuDefinition('csiTrainingMenuV8', 'Training pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', '');
			
		RAISE NOTICE 'Creating Export/Import Menu...';
		CALL createPortalMenuDefinition('csiExport/ImportV8', 'Export/Import pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', '');
		
		RAISE NOTICE 'Creating Change Management V8 Menu...';
		CALL createPortalMenuDefinition('csiChangeManagementV8', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', '');
		CALL createPortalMenuItem(vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', '');

		RAISE NOTICE 'Creating Attachments V8 Menu...';
		CALL createPortalMenuDefinition('csiAttachmentsV8', 'Attachments', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' );
		CALL createPortalMenuItem(vMenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', '');


		RAISE NOTICE 'Creating Portal V8 Main Menu...';
		CALL createPortalMenuDefinition('csiPortalMenuV8', 'The top level Portal menu', vDefaultNotes, vMenuDefId);
		CALL createPortalMenuItem(vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach');
		CALL createPortalMenuItem(vMenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement');
		CALL createPortalMenuItem(vMenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality');
		CALL createPortalMenuItem(vMenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport');
		CALL createPortalMenuItem(vMenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch' );
		CALL createPortalMenuItem(vMenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph');
		CALL createPortalMenuItem(vMenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining');
		
		--SELECT PortalMenuDefinitionId INTO vcsiPortalMenuPortalMenuDefinitionId
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiPortalMenu';

		--SELECT PortalMenuDefinitionId INTO vcsiMobileMenuPortalMenuDefinitionId
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiMobileMenu';

		--SELECT PortalMenuDefinitionId INTO vcsiPortalMenuV8PortalMenuDefinitionId
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiPortalMenuV8';


END $$;


do $$ 
begin	
 call populatePortalMenuUpdateData();
end $$;

do $$ 
begin	
 DROP PROCEDURE IF EXISTS getNextInstanceId;
 DROP PROCEDURE IF EXISTS createPortalMenuItem;
 DROP PROCEDURE IF EXISTS createPortalMenuDefinition;
 DROP PROCEDURE IF EXISTS populatePortalMenuUpdateData;
end $$;

