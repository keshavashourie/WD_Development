--------------------------------------------------------------------------------
-- SCRIPT:PopulatePortalMenuDefaultData.or.sql
-- DESCR: Creates stored procedures used to create PortalMenuDefinitions and PortalMenuItems
--        and then uses those stored procedures to populate the default data
--
--  Copyright Siemens 2024  
--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        Instance Id number
--
CREATE OR REPLACE PROCEDURE getNextInstanceId(
pCDODefId 		IN NUMBER,
pInstanceIdStr	OUT VARCHAR2)
AS
vCDODefIdStr	VARCHAR2(16);
vInstanceId		NUMBER;
vInstIdNewValue	VARCHAR2(16);
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	csiUpdateInstanceId(0,pCDODefId,1,vInstIdNewValue);
	vInstIdNewValue:=LPAD(SUBSTR(vInstIdNewValue,14,10),10,'0');
	vCDODefIdStr   :=LPAD(LTRIM(TO_CHAR(pCDODefId,'xxxxxx')),6,'0');
	pInstanceIdStr :=LOWER(vCDODefIdStr||vInstIdNewValue);
END;
/



CREATE OR REPLACE PROCEDURE csiCreateGUID(
pPermissionName 	IN VARCHAR2, 
pRoleGId 			OUT VARCHAR2)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
v_checksum 			VARCHAR2(36);
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);

	SELECT
	(      SUBSTR(ExportImportKeyGUID, 1, 8) ||
	'-' || SUBSTR(ExportImportKeyGUID, 9, 4) ||
	'-' || SUBSTR(ExportImportKeyGUID, 13, 4) ||
	'-' || SUBSTR(ExportImportKeyGUID, 17, 4) ||
	'-' || SUBSTR(ExportImportKeyGUID, 21)) INTO pRoleGId
	FROM 
	(
		select UPPER( RAWTOHEX( DBMS_CRYPTO.Hash (UTL_RAW.CAST_TO_RAW (pPermissionName),2) ) ) as ExportImportKeyGUID  from dual
	);
END;
/




--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalmenuDefinition record
--
CREATE OR REPLACE PROCEDURE createPortalMenuDefinition(
pPortalMenuDefinitionName	IN VARCHAR2,
pDescription				IN VARCHAR2,
pNotes						IN VARCHAR2,
pInstanceId					OUT VARCHAR2)
AS
vPortalMenuDefCDODefId		NUMBER := 7828;
reccount 					INT;
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	getNextInstanceId(vPortalMenuDefCDODefId, pInstanceId);
  
	SELECT COUNT(*) INTO reccount 
	FROM PortalMenuDefinition 
	WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName;
  
	IF (reccount = 0) THEN
		INSERT 
		INTO PortalMenuDefinition
		(
		PortalMenuDefinitionId,
		CDOTypeId,
		ChangeCount,
		Notes,
		ChangeHistoryId,
		Description,
		IconId,
		IsFrozen,
		PortalMenuDefinitionName
		)
		VALUES
		(
		pInstanceId,
		vPortalMenuDefCDODefId,
		1,
		pNotes,
		NULL,
		pDescription,
		0,
		0,
		pPortalMenuDefinitionName
		);
	ELSE
		SELECT  DISTINCT Portalmenudefinitionid INTO pinstanceid 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName;
  END IF;

END;
/




--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE PROCEDURE createPortalMenuItem(
	pParentId			IN VARCHAR2,
	pCDOTypeId			IN NUMBER,
	pSequence			IN NUMBER,
	pCaption			IN VARCHAR2,
	pLabelName			IN VARCHAR2,
	pPageName			IN VARCHAR2,
	pPageFlowName		IN VARCHAR2,
	pSubMenuName		IN VARCHAR2,	
	pApolloIconName		IN VARCHAR2 DEFAULT NULL,
	pServiceName        IN VARCHAR2 DEFAULT NULL
 )
AS
vMenuItemId 	VARCHAR2(16);
vSubMenuId 		VARCHAR2(16);
vVirtualPageId 	VARCHAR2(16);
vPageFlowId 	VARCHAR2(16);
reccount 		INT;
countV8			INT;	
reccountV8		INT;

BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	BEGIN
		IF LENGTH (pSubMenuName) >0 THEN
			SELECT DISTINCT PortalMenuDefinitionId
			INTO vSubMenuId
			FROM PortalMenuDefinition
			WHERE PortalMenuDefinitionName = pSubMenuName;
		END IF;
	EXCEPTION
	WHEN NO_DATA_FOUND THEN
		vSubMenuId := NULL;
	END;

	BEGIN
		IF LENGTH(pPageName)>0 THEN
			SELECT DISTINCT UIVirtualPageId
			INTO vVirtualPageId
			FROM UIVirtualPage
			WHERE UIVirtualPageName = pPageName;
		END IF;
	EXCEPTION
	WHEN NO_DATA_FOUND THEN
			vVirtualPageId := NULL;
	END;

	BEGIN
		IF LENGTH(pPageFlowName)>0 THEN
			SELECT DISTINCT UIPageFlowId
			INTO vPageFlowId
			FROM UIPageFlow
			WHERE UIPageFlowName = pPageFlowName;
		END IF;
	EXCEPTION
	WHEN NO_DATA_FOUND THEN
		vPageFlowId := NULL;
	END;

	getNextInstanceId(pCDOTypeId, vMenuItemId);
  
	IF pSubMenuName = 'csiChangeManagement' THEN
		SELECT count(*) INTO reccount 
		FROM PortalMenuitem 
		WHERE caption = 'Change Management';

		IF (reccount = 0) THEN
			INSERT 
			INTO PortalMenuItem
			(
			PortalMenuItemId,
			CDOTypeId,
			ChangeCount,
			ParentId,
			IsFrozen,
			Caption,
			LabelName,
			Sequence,
			MenuDefinitionId,
			VirtualPageId,
			PageFlowId,
			PageURL,
			PageDisplay,
			QueryString,
			ApolloIcon,
			ServiceName
			)
			VALUES
			(
			vMenuItemId,
			pCDOTypeId,
			1,
			pParentId,
			0,
			pCaption,
			pLabelName,
			pSequence,
			vSubMenuId,
			vVirtualPageId,
			vPageFlowId,
			NULL,
			NULL,
			NULL,
			pApolloIconName,
			pServiceName
			);
 		ELSE
			DBMS_OUTPUT.put_line('csichangemanagement already exists');
		END IF;
	ELSE
		SELECT COUNT(*) INTO reccount 
		FROM PortalMenuitem 
		WHERE caption= pCaption;

		SELECT COUNT(*) INTO countV8 
		from PortalMenuDefinition 
				WHERE pParentId = PortalMenuDefinitionId AND PortalMenuDefinitionName LIKE '%V8%';

		SELECT COUNT(*) INTO reccountV8 FROM PortalMenuitem INNER JOIN
		PortalMenuDefinition ON PortalMenuitem.ParentId= PortalMenuDefinition.PortalMenuDefinitionId
		WHERE (PortalMenuDefinition.PortalMenuDefinitionName LIKE '%V8%' AND PortalMenuitem.Caption= pCaption AND PortalMenuitem.CDOTypeId = pCDOTypeId);

       IF (reccount = 0 OR (countV8 > 0 AND reccountV8 = 0)) THEN
			INSERT
			INTO PortalMenuItem
			(
			PortalMenuItemId,
			CDOTypeId,
			ChangeCount,
			ParentId,
			IsFrozen,
			Caption,
			LabelName,
			Sequence,
			MenuDefinitionId,
			VirtualPageId,
			PageFlowId,
			PageURL,
			PageDisplay,
			QueryString,
			ApolloIcon
			)
			VALUES
            (
			vMenuItemId,
			pCDOTypeId,
			1,
			pParentId,
			0,
			pCaption,
			pLabelName,
			pSequence,
			vSubMenuId,
			vVirtualPageId,
			vPageFlowId,
			NULL,
			NULL,
			NULL,
			pApolloIconName
			);
		ELSE
				SELECT portalmenuitemid INTO vMenuItemId 
				FROM portalmenuitem 
				WHERE Caption = pCaption and ROWNUM = 1;
		END if;
	END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM related roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiRBACCreateRole(pRoleName IN VARCHAR2, pRoleDescription IN VARCHAR2, pInstanceId OUT VARCHAR2)
AS
   vRoleCDODefId NUMBER := 7130;
   reccount INT;
   vRoleDefId VARCHAR2(16);
BEGIN
    csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
     SELECT count(*) into reccount FROM RoleDef WHERE RoleName = pRoleName;
           If (reccount = 0) then
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
             ELSE
			 SELECT ROLEID into vRoleDefId FROM RoleDef WHERE RoleName = pRoleName;
			 delete from EmployeeRole WHERE ROLEID = vRoleDefId;
             delete from RoleDef WHERE RoleName = pRoleName;
              INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
                 DBMS_OUTPUT.put_line('RoleDef'|| pRoleName || 'deleted and inserted');
		
    end if;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAssignRoleIfExist(ExRoleName IN VARCHAR2, pRoleId IN VARCHAR2, pRoleDescription IN VARCHAR2, pOrganizationName IN VARCHAR2, pPropagate IN NUMBER)
AS
    vCDODefId NUMBER:=7782;
    vEmployeeId VARCHAR2(16);
    vOrgId VARCHAR2(16);
    vIID VARCHAR2(16);
    vRoleGID VARCHAR2(36);
    vRoleDescription VARCHAR2(255);
    pEmployeeName VARCHAR2(36);
BEGIN
  DECLARE
	CURSOR csr
  IS
	SELECT Employee.EmployeeName FROM EmployeeRole 
	LEFT JOIN RoleDef ON EmployeeRole.RoleId = RoleDef.RoleId
	LEFT JOIN Employee ON EmployeeRole.EmployeeId = Employee.EmployeeId
	WHERE RoleDef.RoleName = ExRoleName;
	BEGIN
	Open csr;
	LOOP
	FETCH csr into pEmployeeName;
	EXIT WHEN csr%NOTFOUND;
	--------------------------------------------------
    SELECT EmployeeId
    INTO vEmployeeId
    FROM Employee
    WHERE EmployeeName=pEmployeeName;
    
    vOrgId:=NULL;
    IF (NOT pOrganizationName IS NULL) THEN
       SELECT OrganizationId
       INTO vOrgId
       FROM Organization
       WHERE OrganizationName=pOrganizationName;
    END IF;

    csiPRDGetNextInstanceId(vCDODefId,vIID);
    vRoleDescription := pRoleDescription || pEmployeeName;
    csiCreateGUID(vRoleDescription, vRoleGID);
    INSERT INTO EmployeeRole(ExportImportkey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);

	END LOOP;
	CLOSE csr;
	END;
END;
/

--------------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--
CREATE OR REPLACE PROCEDURE csiRBACDeleteUnavailablePermissions(
	vPermissionType			IN NUMBER,
	PermissionName		  IN VARCHAR2)
AS
	UnavialiblePermissions VARCHAR2(16);
BEGIN
    DECLARE
    CURSOR csr
    IS
		SELECT RolePermission.RolePermissionId FROM RolePermission
		WHERE RolePermissionName = PermissionName AND PermissionType <> vPermissionType;
    BEGIN
		Open csr;
		LOOP
		FETCH csr into UnavialiblePermissions;
			EXIT WHEN csr%NOTFOUND;
		--------------------------------------------------
			DELETE FROM RolePermission WHERE RolePermissionId = UnavialiblePermissions;
			DELETE FROM RolePermissionModes WHERE RolePermissionId = UnavialiblePermissions;
		--------------------------------------------------
		END LOOP;
		CLOSE csr;
    END;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM realted role permissions. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiRBACAssignPermissionToRole(vRoleName IN VARCHAR2, pPermissionName IN VARCHAR2, pPermissionType IN NUMBER, pObjectMetaId IN NUMBER, pPermissionModesFlag IN NUMBER, pObjectInstanceId IN VARCHAR2 DEFAULT NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110), 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
AS
   vIID VARCHAR2(16);
	 vRoleGID VARCHAR2(36);
   vRoleId VARCHAR2(16);
BEGIN
    SELECT RoleId INTO vRoleId FROM RoleDef WHERE RoleName = vRoleName;
          csiPRDGetNextInstanceId(7783,vIID);
          INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
            VALUES(vRoleGID,vIID,7783,vRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);
        -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
        -- based on the pPermissionModesFlag value
      IF ( pPermissionModesFlag = 0 ) THEN
           INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
              SELECT vIID, 15263, BitNumber, ROWNUM
              FROM SecurityMaskDetail
              WHERE SecurityMaskId=pPermissionType
              ORDER BY BitNumber;
        ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
        INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
              AND BitNumber = 2
          ORDER BY BitNumber;
        ELSIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230)) THEN
        INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
              AND BitNumber IN (1,2,3,4)
          ORDER BY BitNumber;
      ELSE
        DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for PermissionModeFlag parameter...');
      END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAssignRole(
pRoleId 			IN VARCHAR2, 
pRoleDescription 	VARCHAR2, 
pEmployeeName 		IN VARCHAR2, 
pOrganizationName 	IN VARCHAR2, 
pPropagate 			IN NUMBER)
AS
vCDODefId 			NUMBER:=7782;
vEmployeeId 		CHAR(16);
vOrgId 				CHAR(16);
vIID 				VARCHAR2(16);
vRoleGID 			VARCHAR2(36);
vRoleDescription 	VARCHAR2(255);
reccount 			INT;
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);

	SELECT EmployeeId
	INTO vEmployeeId
	FROM Employee
	WHERE EmployeeName=pEmployeeName;
    
	vOrgId:=NULL;
    
	IF (NOT pOrganizationName IS NULL) THEN
			SELECT OrganizationId
			INTO vOrgId
			FROM Organization
			WHERE OrganizationName=pOrganizationName;
	END IF;

	csiPRDGetNextInstanceId(vCDODefId,vIID);
	vRoleDescription := pRoleDescription || pEmployeeName;
	csiCreateGUID(vRoleDescription, vRoleGID);

	SELECT COUNT(*) INTO reccount 
	FROM employeerole 
	WHERE Roleid = pRoleId;

	IF (reccount = 0) THEN
		INSERT 
		INTO EmployeeRole
		(ExportImportkey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
		VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);
	END IF;  
END;
/




--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreatePermission(
pRoleId					IN VARCHAR2,
pPermissionName			IN VARCHAR2,
pPermissionType			IN NUMBER,
pObjectMetaId			IN NUMBER,
pPermissionModesFlag	IN NUMBER,
pObjectInstanceId		IN VARCHAR2 DEFAULT NULL)
  -- pPermissionModesFlag Valid values:
  --  * 0 => None    - Inserts all permission modes based on the security type
  --  * 1 => Read-Only  - Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
  --  * 2 => No-Sec-Admin  - Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
vIID 					VARCHAR2(16);
reccount 				INT;
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	csiPRDGetNextInstanceId(7783,vIID);

	SELECT COUNT(*) INTO reccount 
	FROM RolePermission 
	WHERE RolePermissionName = pPermissionName and RoleId = pRoleId;

	IF (reccount = 0) THEN
		INSERT
		INTO RolePermission
		(
		RolePermissionId,
		CDOTypeId,
		RoleId,
		ChangeCount,
		RolePermissionName,
		IsFrozen,
		ObjectMetaId,
		PermissionType,
		ObjectInstanceId
		)
		VALUES
		(
		vIID,
		7783,
		pRoleId,
		1,
		pPermissionName,
		0,
		pObjectMetaId,
		pPermissionType,
		pObjectInstanceId
		);
	END IF;
	-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
	-- based on the pPermissionModesFlag value
	IF (pPermissionModesFlag = 0) THEN
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
		SELECT vIID,
		15263,
		BitNumber,
		ROWNUM
		FROM SecurityMaskDetail
		WHERE SecurityMaskId=PPermissionType
		ORDER BY BitNumber;
	ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180) ) THEN
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
		SELECT vIID,
		15263,
		BitNumber,
		ROWNUM
		FROM SecurityMaskDetail
		WHERE SecurityMaskId=PPermissionType
		AND BitNumber = 2
		ORDER BY BitNumber;
	ELSIF ( pPermissionModesFlag = 2 AND pPermissionType = 180 ) THEN
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
		SELECT vIID,
		15263,
		BitNumber,
		ROWNUM
		FROM SecurityMaskDetail
		WHERE SecurityMaskId=PPermissionType
		AND BitNumber IN (1,2,3,4)
		ORDER BY BitNumber;
	ELSE
		DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for @PermissionModeFlag parameter...');
	END IF;
END;
/




--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  pPermissionModesFlag - See rbacCreatePermissions
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreatePermissionsForQry(
pRoleId					VARCHAR2, 
pSecurityList			VARCHAR2, 
pPermissionModesFlag	NUMBER)
AS
vSQLString				VARCHAR2(4000);
c1 SYS_REFCURSOR;
vObjectMetaId			NUMBER;
vPermissionType			NUMBER;
vPermissionName			VARCHAR2(255);
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);

	vSQLString := 'Select CDO.CDODefId As ObjectMetaId ' ||
	', CDO.SecurityTypeId as PermissionType ' ||
	', Labels.LabelValue as PermissionName ' ||
	'From	CDODefinition CDO ' ||
	'		, Labels ' ||
	'Where	Labels.LabelId = CDO.DisplayNameLabelId ' ||
	'And		CDO.IsAbstract = 0 ' ||
	'And		CDO.SecurityTypeId In ( ' || pSecurityList || ' ) ' ||
	'ORDER By CDO.SecurityTypeId, CDO.CDOName ' ;

   OPEN c1 FOR vSQLString;
   FETCH c1 INTO vObjectMetaId,vPermissionType,vPermissionName;
   WHILE (c1%FOUND) LOOP
      csiRBACCreatePermission (pRoleId, vPermissionName, vPermissionType, vObjectMetaId, pPermissionModesFlag);
      FETCH c1 INTO vObjectMetaId,vPermissionType,vPermissionName;
   END LOOP;
   CLOSE c1;
END;
/



--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAddVPagePermissions
-- DESCR: Creates a Permissions based on a query
--    pRoleId - The role to which the permissions are added
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAddCMVPPermissions (
pRoleId 			VARCHAR2)
AS
vSQLString 			VARCHAR2(4000);
c1 SYS_REFCURSOR;
vUIVirtualPageId 	VARCHAR2(16);
vPermissionName  	VARCHAR2(255);
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	vSQLString := 'Select UIVirtualPageName, UIVirtualPageId ' || 'From UIVirtualPage Order By UIVirtualPageName ';
  
	OPEN c1 FOR vSQLString;
	FETCH c1 INTO vPermissionName,vUIVirtualPageId;
  
	WHILE (c1%FOUND)
	LOOP
		csiRBACCreatePermission (pRoleId, vPermissionName, 200, NULL, 0, vUIVirtualPageId);
		FETCH c1 INTO vPermissionName,vUIVirtualPageId;
	END LOOP;
	CLOSE c1;
END;
/



--------------------------------------------------------------------------------
-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE PROCEDURE populatePortalMenuUpdateData
AS
vMenuDefId						VARCHAR2(16);
vAssignedMenuDefId				VARCHAR2(16);
vHomePage						VARCHAR2(16);
vDefaultNotes					VARCHAR2(1000);
vRoleId							VARCHAR2(16);
vPermissionModesFlag_None		NUMBER;
vPermissionModesFlag_NoSecAdmn	NUMBER;
vPermissionSQL					VARCHAR2(4000);
vPermissionModesFlag_ReadOnly 	NUMBER;
pInstanceId						VARCHAR2(16);
portalMenuDefId CHAR(16);
mobileMenuDefId CHAR(16);
portalV8MenuDefId CHAR(16);
homePageIdForV8 CHAR(16);
employeeIdForV8 CHAR(16);
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	vPermissionModesFlag_None := 0;
	vPermissionModesFlag_ReadOnly := 1;
	vPermissionModesFlag_NoSecAdmn := 2;
     
	vDefaultNotes := 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.';
   
	--'Creating Change Management Menu...'
	 createPortalMenuDefinition('csiChangeManagement', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
	 createPortalMenuItem (vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '' );
	 createPortalMenuItem (vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
	 createPortalMenuItem (vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '' );
	 createPortalMenuItem (vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch', 'PackageInquiry_VP', '', '' );
	 createPortalMenuItem (vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple', 'PackageSearchMultiple_VP', '', '' );
 
	--'Creating Attachments Menu...'
	createPortalMenuDefinition ('csiAttachments', 'Attachments', vDefaultNotes, vMenuDefId );
	
	DELETE PortalMenuItem 
	WHERE  Caption = 'Attach Documents';

	createPortalMenuItem (vMenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Manage Attachments','LblMenuManageAttachments', 'AttachDocumentManagement_VP', '', '' );

	--'Creating Portal Main Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiPortalMenu';

	createPortalMenuItem(vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments','', '', 'csiAttachments', 'cmdAttach');
	createPortalMenuItem(vMenuDefId, 7833, 15, 'Change Management', 'LblMenuChgMngt', '', '', 'csiChangeManagement', 'cmdChangeManagement');
	
	--'Creating Container Mobile Sub Menu'
	createPortalMenuDefinition ('csiContainerMobileMenu', 'Mobile Menu for Container Txns', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Hold', 'LblMenuHold','ContainerHoldVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Move In','LblMenuMoveIn', 'MoveInVP_R2', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Move', 'LblMenuMove', 'MoveStdVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Rework', 'LblMenurework','ReworkVP_R2', '', '');

	createPortalMenuDefinition('csiMobileMenu', 'The top level Mobile Portal menu', vDefaultNotes, vMenuDefId);
	createPortalMenuItem(vMenuDefId, 7833, 10, 'Container', 'LblMenuCont','', '', 'csiContainerMobileMenu', 'cmdLot');

	--Creating Search Menu
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiSearch';
  
	DELETE PortalMenuItem 
	WHERE Caption = 'Audit Trail';

	createPortalMenuItem (vMenuDefId, 7835, 35, 'Delegation Search', 'LblMenuDelegationSearch','DelegationSearch_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 45, 'Process Timer Search','LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 55, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP', '', '');

	--Classic Container menu update
	SELECT PortalMenuDefinitionId 
	INTO vMenuDefId
	FROM PortalMenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiContainer';

	createPortalMenuItem(vMenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '');
	
	createPortalMenuItem(vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '');
	createPortalMenuItem(vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '');
	
	createPortalMenuItem(vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate'); 
	createPortalMenuItem(vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate');
	createPortalMenuItem(vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
	createPortalMenuItem(vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');	
	

	--'Creating Modeling Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiModelingMenu';
  
	DELETE PortalMenuItem 
	WHERE Caption = 'Audit Trail';

	createPortalMenuItem (vMenuDefId, 7835, 25, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', '');
   
	-- Default Modeling Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Modeling';
    
	vPermissionSQL := '110';
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_NoSecAdmn);
	--csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
  
	-- Default Inquiry Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Inquiry';
    
	vPermissionSQL := '140'; 
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Pages';
  
	--csiRBACAssignRole (vRoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0);
	--csiRBACAssignRole (vRoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0);
	--csiRBACAddCMVPPermissions(vRoleId);
	--csiRBACCreatePermission (vRoleId, 'UI Virtual Page Maint', 110, 7755, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'Web Part Maint', 110, 8143, vPermissionModesFlag_None);
  
	-- Login Access Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Login';

	--csiRBACCreatePermission (vRoleId, 'System', 190, 1, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'MenuDefinitionMaint', 110, 6907, vPermissionModesFlag_ReadOnly);
	--csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);

	-- Security Administration Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Security Administration';

	--csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_None);

	-- Each of the following blocks represent a query-based Role/Permission assignment for default services
	-- Default Modeling Read-Only Role...
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Modeling Read-Only';

	vPermissionSQL := '110';
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_ReadOnly);
	--csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);
	--csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_ReadOnly);

	--	--Modeling Advanced Role
	--csiRBACCreateRole ('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
	--csiRBACAssignRoleIfExist ('Default Modeling', vRoleId, 'Modeling Services for Advanced Users', NULL, 0);
	vPermissionSQL := '230';
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	
 --   csiRBACDeleteUnavailablePermissions (0, 'User Query Maint');
 --   csiRBACDeleteUnavailablePermissions (0, 'Business Rule Handler Maint');
 --   csiRBACDeleteUnavailablePermissions (0, 'Business Rule Maint');
 --   csiRBACDeleteUnavailablePermissions (0, 'Scheduled Business Rule Maint');
 --   csiRBACDeleteUnavailablePermissions (0, 'Summary Table Def Maint');

	----	This it totally unnecessary as these permissions only are usable under the Default Modeling Advanced role		  
	--csiRBACAssignPermissionToRole('Default Modeling', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	--csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	
	--csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	--csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	
	--csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	--csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	
	--csiRBACAssignPermissionToRole('Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	--csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	
	--csiRBACAssignPermissionToRole('Default Modeling', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
	--csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);

	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	-- Default Manufacturing Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Mfg';

	vPermissionSQL := '100, 120, 130' ;
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	-- Default Quality Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Quality';

	vPermissionSQL := '170'; 
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	-- Default Export Import Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Default Export Import';

	vPermissionSQL := '150,160'; 
	--csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	-- Mfg Audit Trail Inquiry Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Mfg Audit Trail Inquiry';

	--csiRBACCreatePermission (vRoleId, 'Container Txn Rev', 130, 5440, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'Container History Inquiry', 140, 6908, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'History View Maint', 110, 7083, vPermissionModesFlag_ReadOnly);

	-- Portal Configuration Role
	SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'Portal Configuration';

	--csiRBACCreatePermission (vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);
    
	-- Create SPC Role...
	BEGIN           
		SELECT uivirtualpageid
		INTO pInstanceId	
		FROM uivirtualpage
		WHERE uivirtualpagename = 'SPCTesterFormVP';	
    EXCEPTION  
	WHEN NO_DATA_FOUND THEN 
		pInstanceId := NULL; 		 
    END;
    
    SELECT RoleId INTO vRoleId 
	FROM RoleDef 
	WHERE Rolename = 'SPC'; 

	--csiRBACCreatePermission (vRoleId, 'SPCChartDefMaint', 110, 8204, vPermissionModesFlag_None);    
	--csiRBACCreatePermission (vRoleId, 'SPCTesterFormVP', 200, NULL, vPermissionModesFlag_None, pInstanceId);        
	--csiRBACCreatePermission (vRoleId, 'Add SPC Annotation', 120, 8393, vPermissionModesFlag_None);
	--csiRBACCreatePermission (vRoleId, 'Record SPC Violation', 120, 8396, vPermissionModesFlag_None);

	--Create V8 Menu with Apollo
	--Container Menu
	createPortalMenuDefinition ('csiContainerV8', 'Container transactions used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate');  
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssue_VPR2', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 130, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 134, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate'); 
	createPortalMenuItem (vMenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc', 'EProcedureVPR2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVPR2', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VPR2', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', ''); 
    createPortalMenuItem (vMenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
	createPortalMenuItem (vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');
	createPortalMenuItem (vMenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 410, 'HV Component Issue', 'CSICDOName_HVComponentIssue','HVComponentIssueVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 420, 'Slitting', 'CSICDOName_Slitting','SlittingVP', '', ''); 	
	
	--Job Services menu
	createPortalMenuDefinition ('csiResourceTxn_JobSvc', 'Job Services', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Job Create', 'CSICDOName_JobCreate','JobCreateVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Job Assign', 'CSICDOName_JobAssign','JobAssign_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Job Acknowledge', 'CSICDOName_JobAcknowledge','JobAcknowledge_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Job Clock On', 'CSICDOName_JobClockOn','JobClockOn_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Job Progress', 'CSICDOName_JobProgress','JobProgress_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 60, 'Job Clock Off', 'CSICDOName_ClockOff','JobClockOff_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 70, 'Job Complete', 'CSICDOName_JobComplete','JobComplete_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 80, 'Job Cancel', 'CSICDOName_JobCancel','JobCancel_VP', '', '');
	
	--Jobs menu
	createPortalMenuDefinition ('csiResourceTxn_Job', 'Jobs', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Supervisor Jobs', 'LblMenuSupervisorJobs','JobSupervisor_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Technician Jobs', 'LblMenuTechnicianJobs','JobTechnicians_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7833, 30, 'Job Services', 'PortalUI_JobServices', '','', 'csiResourceTxn_JobSvc');
	
	--Part Services menu
	createPortalMenuDefinition ('csiResourceTxn_PartSvc', 'Part Services', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Part Create', 'CSICDOName_PartCreate','PartCreate_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Part Setup', 'CSICDOName_PartSetup','PartSetupVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Part Scrap', 'CSICDOName_PartScrap','PartScrap_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Part Request', 'CSICDOName_PartRequest','PartRequestVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Part Request Acknowledge', 'CSICDOName_PartRequestAcknowledge','PartRequestAcknowledgeVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 60, 'Part Request Assign', 'CSICDOName_PartRequestAssign','PartRequestAssign_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 70, 'Part Request Update', 'CSICDOName_PartRequestUpdate','PartRequestUpdateVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 80, 'Part Request Issue', 'CSICDOName_PartRequestIssue','PartRequestIssue_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 90, 'Part Request Complete', 'CSICDOName_PartRequestComplete','PartRequestComplete_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 100, 'Part Request Cancel', 'CSICDOName_PartRequestCancel','PartRequestCancel_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 110, 'Part Request Cancel Acknowledge', 'CSICDOName_PartRequestCancelAcknowledge','PartRequestCancelAcknowledgeVP', '', '');
	
	--Parts menu
	createPortalMenuDefinition ('csiResourceTxn_Part', 'Parts', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Material Parts', 'PartRequestOrder_MaterialParts', 'MaterialPartMaintenanceVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Part Maintenance', 'LblPartMaintenance', 'PartMaintenanceVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Technician Requests', 'LblTechnicianRequests','PartRequestMain_VP', '', '', '', 'PartRequest');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Inventory Requests', 'LblInventoryRequests','PartRequestMain_VP', '', '', '', 'PartRequestAssign');
	createPortalMenuItem (vMenuDefId, 7833, 50, 'Part Services', 'PortalUI_PartServices', '','', 'csiResourceTxn_PartSvc');
	
	--Resource menu
	createPortalMenuDefinition ('csiResourceV8', 'Resource transactions available in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '');
	createPortalMenuItem (vMenuDefId, 7833, 80, 'Jobs', 'PortalUI_Jobs', '','', 'csiResourceTxn_Job', '');
	createPortalMenuItem (vMenuDefId, 7833, 90, 'Parts', 'Resource_Parts', '','', 'csiResourceTxn_Part', '');
	createPortalMenuItem (vMenuDefId, 7835, 100, 'HV Resource Setup', 'CSICDOName_HVResourceSetup','HVResourceSetupVP', '', '');

	--Creating Events Menu
	createPortalMenuDefinition ('csiEventV8', 'Quality event transactions in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', ''); 

	--Creating Search Menu
	createPortalMenuDefinition ('csiSearchV8', 'Search options in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP_R2', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '');

	--Creating SPC Menu
	createPortalMenuDefinition ('csiSPCV8', 'SPC pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 20, 'SPC Realtime Monitoring', 'LblSPCRealtimeMonitoring', 'SPCRealtimeMonitoring_VP', '', ''); 
	
	--Creating Modeling Menu
	createPortalMenuDefinition ('csiModelingMenuV8', 'Modeling pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', '');
	
	--Creating Training Menu
	createPortalMenuDefinition ('csiTrainingMenuV8', 'Training pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', '');
	
	--Creating Export/Import Menu
	createPortalMenuDefinition ('csiExport/ImportV8', 'Export/Import pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', '');
	
	--Creating Change Management Menu
	createPortalMenuDefinition ('csiChangeManagementV8', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', '');

	--Creating Attachments Menu
	createPortalMenuDefinition ('csiAttachmentsV8', 'Attachments', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', ''); 
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', '');


	--Creating Portal V8 Main Menu
	createPortalMenuDefinition ('csiPortalMenuV8', 'The top level Portal menu', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach');
	createPortalMenuItem (vMenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement');
	createPortalMenuItem (vMenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot'); 
	createPortalMenuItem (vMenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality');
	createPortalMenuItem (vMenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport');
	createPortalMenuItem (vMenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem'); 
	createPortalMenuItem (vMenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine'); 
	createPortalMenuItem (vMenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch'); 
	createPortalMenuItem (vMenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph');
	createPortalMenuItem (vMenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining');

	--SELECT PortalMenuDefinitionId INTO portalMenuDefId
	--FROM portalmenuDefinition 
	--WHERE PortalMenuDefinitionName = 'csiPortalMenu';

	--SELECT PortalMenuDefinitionId INTO mobileMenuDefId
	--FROM portalmenuDefinition 
	--WHERE PortalMenuDefinitionName = 'csiMobileMenu';
  
	--SELECT PortalMenuDefinitionId INTO portalV8MenuDefId
	--FROM portalmenuDefinition 
	--WHERE PortalMenuDefinitionName = 'csiPortalMenuV8';

	--UPDATE EMPLOYEE 
	--SET PortalMenuDefinitionId = portalMenuDefId,
	--PortalMobileMenuDefinitionId = mobileMenuDefId,
	--PortalV8MenuDefinitionId = portalV8MenuDefId
	--WHERE EmployeeName = 'CamstarAdmin';

	----This gets set in the Loader. Not needed here for all 3 users.
	----UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	----WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'CamstarAdmin');

	--SELECT UIVirtualPageId INTO homePageIdForV8
	--from UIVirtualPage 
	--where UIVirtualPageName = 'OperationalViewVPR2';
	
	--SELECT employeeid INTO employeeIdForV8
	--from Employee 
	--WHERE EmployeeName = 'CamstarAdmin';
	 
	----UPDATE UIPortalProfile SET PortalV8HomePageId = homePageIdForV8
	----WHERE ParentId = employeeIdForV8;

	--UPDATE EMPLOYEE 
	--SET PortalMenuDefinitionId = portalMenuDefId,
	--PortalMobileMenuDefinitionId = mobileMenuDefId,
	--PortalV8MenuDefinitionId = portalV8MenuDefId
	--WHERE EmployeeName = 'InSiteAdmin';

	----UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	----WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'InSiteAdmin');

	--UPDATE EMPLOYEE 
	--SET PortalMenuDefinitionId = portalMenuDefId,
	--PortalMobileMenuDefinitionId = mobileMenuDefId,
	--PortalV8MenuDefinitionId = portalV8MenuDefId
	--WHERE EmployeeName = 'Administrator';


	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'Administrator');


END;
/




BEGIN
	populatePortalMenuUpdateData;
	COMMIT;
END;
/



BEGIN
	EXECUTE IMMEDIATE 'DROP PROCEDURE getNextInstanceId';
	EXECUTE IMMEDIATE 'DROP PROCEDURE createPortalMenuItem';
	EXECUTE IMMEDIATE 'DROP PROCEDURE createPortalMenuDefinition';
	EXECUTE IMMEDIATE 'DROP PROCEDURE populatePortalMenuUpdateData';
END;
/
