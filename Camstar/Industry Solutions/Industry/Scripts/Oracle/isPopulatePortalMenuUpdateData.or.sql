/*
-- SCRIPT:PopulatePortalMenuDefaultData.or.sql
-- DESCR: Creates stored procedures used to create PortalMenuDefinitions and PortalMenuItems
--        and then uses those stored procedures to populate the default data
--
--  Copyright Siemens 2024

-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        Instance Id number
--
*/
--#delimiter
CREATE OR REPLACE
PROCEDURE getNextInstanceId
  (
    pCDODefId IN NUMBER,
    pInstanceIdStr OUT VARCHAR2)
AS
  vCDODefIdStr    VARCHAR2(16);
  vInstanceId     NUMBER;
  vInstIdNewValue VARCHAR2(16);
BEGIN
  csiUpdateInstanceId(0,pCDODefId,1,vInstIdNewValue);
  vInstIdNewValue:=LPAD(SUBSTR(vInstIdNewValue,14,10),10,'0');
  vCDODefIdStr   :=LPAD(LTRIM(TO_CHAR(pCDODefId,'xxxxxx')),6,'0');
  pInstanceIdStr :=LOWER(vCDODefIdStr||vInstIdNewValue);
END;
--#delimiter

CREATE OR REPLACE PROCEDURE csiCreateGUID(pPermissionName IN VARCHAR2, pRoleGId OUT VARCHAR2)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
   v_checksum VARCHAR2(36);

    BEGIN
        select 
 (      SUBSTR(ExportImportKeyGUID, 1, 8) ||
'-' || SUBSTR(ExportImportKeyGUID, 9, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 13, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 17, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 21)) INTO pRoleGId
from (
select UPPER( RAWTOHEX( DBMS_CRYPTO.Hash (UTL_RAW.CAST_TO_RAW (pPermissionName),2) ) ) as ExportImportKeyGUID  from dual
);
END;
--#delimiter

-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalmenuDefinition record
--
CREATE OR REPLACE
PROCEDURE createPortalMenuDefinition
  (
    pPortalMenuDefinitionName IN VARCHAR2,
    pDescription              IN VARCHAR2,
    pNotes                    IN VARCHAR2,
    pInstanceId OUT VARCHAR2)
    
AS
  vPortalMenuDefCDODefId NUMBER := 7828;
  reccount INT;
BEGIN
  getNextInstanceId(vPortalMenuDefCDODefId, pInstanceId);
  
   SELECT count(*) into reccount FROM PortalMenuDefinition WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName;
  If (reccount = 0) then
  INSERT INTO PortalMenuDefinition
    (
      PortalMenuDefinitionId ,
      CDOTypeId ,
      ChangeCount ,
      Notes ,
      ChangeHistoryId ,
      Description ,
      IconId ,
      IsFrozen ,
      PortalMenuDefinitionName
    )
    VALUES
    (
      pInstanceId ,
      vPortalMenuDefCDODefId ,
      1 ,
      pNotes ,
      NULL ,
      pDescription ,
      0 ,
      0 ,
      pPortalMenuDefinitionName
    );
    
    ELSE
	select distinct Portalmenudefinitionid into pinstanceid from PortalMenuDefinition where PortalMenuDefinitionName = pPortalMenuDefinitionName;
  END IF;
END;
--#delimiter

-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE
PROCEDURE createPortalMenuItem
  (
    pParentId     IN VARCHAR2,
    pCDOTypeId    IN NUMBER,
    pSequence     IN NUMBER,
    pCaption      IN VARCHAR2,
    pLabelName    IN VARCHAR2,
    pPageName     IN VARCHAR2,
    pPageFlowName IN VARCHAR2,
	pSubMenuName		IN VARCHAR2,
	pApolloIconName		IN VARCHAR2 DEFAULT NULL
  )
AS
  vMenuItemId VARCHAR2
  (
    16
  )
  ;
  vSubMenuId VARCHAR2
  (
    16
  )
  ;
  vVirtualPageId VARCHAR2
  (
    16
  )
  ;
  vPageFlowId VARCHAR2
  (
    16
  )
  ;
  reccount INT;
BEGIN
  BEGIN
    IF LENGTH
      (
        pSubMenuName
      )
      >0 THEN
      SELECT Distinct PortalMenuDefinitionId
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
      SELECT Distinct UIVirtualPageId
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
      SELECT Distinct UIPageFlowId
      INTO vPageFlowId
      FROM UIPageFlow
      WHERE UIPageFlowName = pPageFlowName;
    END IF;
  EXCEPTION
  WHEN NO_DATA_FOUND THEN
    vPageFlowId := NULL;
  END;
  getNextInstanceId(pCDOTypeId, vMenuItemId);
  if pSubMenuName = 'csiChangeManagement' then
			SELECT count(*) into reccount FROM PortalMenuitem WHERE caption = 'Change Management';
          If (reccount = 0) then
                        INSERT INTO PortalMenuItem
                        (
                                PortalMenuItemId ,
                                CDOTypeId ,
                                ChangeCount ,
                                ParentId ,
                                IsFrozen ,
                                Caption ,
                                LabelName,
                                Sequence ,
                                MenuDefinitionId ,
                                VirtualPageId ,
                                PageFlowId ,
                                PageURL ,
                                PageDisplay ,
								QueryString,
								ApolloIcon
                                )
                                VALUES
                                (
                                  vMenuItemId ,
                                  pCDOTypeId ,
                                  1 ,
                                  pParentId ,
                                  0 ,
                                  pCaption ,
                                  pLabelName,
                                  pSequence ,
                                  vSubMenuId ,
                                  vVirtualPageId ,
                                  vPageFlowId ,
                                  NULL ,
                                  NULL ,
								  NULL,
								  pApolloIconName
                                );
    
            else
                              DBMS_OUTPUT.put_line('csichangemanagement already exists');
					End if;
		else
                      SELECT count(*) into reccount FROM PortalMenuitem WHERE caption= pCaption AND ParentId= pParentId;
                          if (reccount = 0 ) then
                                        INSERT
                            INTO PortalMenuItem
                              (
                                PortalMenuItemId ,
                                CDOTypeId ,
                                ChangeCount ,
                                ParentId ,
                                IsFrozen ,
                                Caption ,
                                LabelName,
                                Sequence ,
                                MenuDefinitionId ,
                                VirtualPageId ,
                                PageFlowId ,
                                PageURL ,
                                PageDisplay ,
								QueryString,
								ApolloIcon
                              )
                              VALUES
                              (
                                vMenuItemId ,
                                pCDOTypeId ,
                                1 ,
                                pParentId ,
                                0 ,
                                pCaption ,
                                pLabelName,
                                pSequence ,
                                vSubMenuId ,
                                vVirtualPageId ,
                                vPageFlowId ,
                                NULL ,
                                NULL ,
								NULL,
								pApolloIconName
                              );
                            else
                            select portalmenuitemid into vMenuItemId from portalmenuitem where Caption = pCaption AND ParentId= pParentId;
                 end if;
    end if;
              
END;
--#delimiter

-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- © 2017 Siemens Product Lifecycle Management Software Inc.
CREATE OR REPLACE PROCEDURE csiRBACAssignRole(pRoleId IN VARCHAR2, pRoleDescription VARCHAR2, pEmployeeName IN VARCHAR2, pOrganizationName IN VARCHAR2, pPropagate IN NUMBER)
AS
    vCDODefId NUMBER:=7782;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR2(16);
    vRoleGID VARCHAR2(36);
    vRoleDescription VARCHAR2(255);
      reccount INT;
BEGIN
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
	SELECT count(*) into reccount FROM employeerole WHERE Roleid = pRoleId;
    if (reccount = 0) then
	   INSERT INTO EmployeeRole(ExportImportkey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);
       end if;
	   
END;
--#delimiter

-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
--  © 2017 Siemens Product Lifecycle Management Software Inc.
CREATE OR REPLACE
PROCEDURE csiRBACCreatePermission
  (
    pRoleId              IN VARCHAR2,
    pPermissionName      IN VARCHAR2,
    pPermissionType      IN NUMBER,
    pObjectMetaId        IN NUMBER,
    pPermissionModesFlag IN NUMBER,
    pObjectInstanceId    IN VARCHAR2 DEFAULT NULL
  )
  -- pPermissionModesFlag Valid values:
  --  * 0 => None    - Inserts all permission modes based on the security type
  --  * 1 => Read-Only  - Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
  --  * 2 => No-Sec-Admin  - Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
  vIID VARCHAR2
  (
    16
  )
  ;
  reccount INT;
BEGIN
  csiPRDGetNextInstanceId
  (
    7783,vIID
  )
  ;
   SELECT count(*) into reccount FROM RolePermission WHERE RolePermissionName = pPermissionName and RoleId = pRoleId;
           if (reccount = 0) then
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
  IF
    (
      pPermissionModesFlag = 0
    )
    THEN
    INSERT INTO RolePermissionModes
      (RolePermissionId, FieldId, Modes, Sequence
      )
    SELECT vIID,
      15263,
      BitNumber,
      ROWNUM
    FROM SecurityMaskDetail
    WHERE SecurityMaskId=PPermissionType
    ORDER BY BitNumber;
  ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180) ) THEN
    INSERT INTO RolePermissionModes
      (RolePermissionId, FieldId, Modes, Sequence
      )
    SELECT vIID,
      15263,
      BitNumber,
      ROWNUM
    FROM SecurityMaskDetail
    WHERE SecurityMaskId=PPermissionType
    AND BitNumber       = 2
    ORDER BY BitNumber;
  ELSIF ( pPermissionModesFlag = 2 AND pPermissionType = 180 ) THEN
    INSERT INTO RolePermissionModes
      (RolePermissionId, FieldId, Modes, Sequence
      )
    SELECT vIID,
      15263,
      BitNumber,
      ROWNUM
    FROM SecurityMaskDetail
    WHERE SecurityMaskId=PPermissionType
    AND BitNumber      IN (1,2,3,4)
    ORDER BY BitNumber;
  ELSE
    DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for @PermissionModeFlag parameter...');
  END IF;
END;
--#delimiter

-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  pPermissionModesFlag - See rbacCreatePermissions
--  © 2017 Siemens Product Lifecycle Management Software Inc.
CREATE OR REPLACE PROCEDURE csiRBACCreatePermissionsForQry(pRoleId VARCHAR2, pSecurityList VARCHAR2, pPermissionModesFlag NUMBER)
AS
    vSQLString VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vObjectMetaId NUMBER;
    vPermissionType NUMBER;
    vPermissionName VARCHAR2(255);
BEGIN
    vSQLString := 'Select CDO.CDODefId As ObjectMetaId ' ||
		', CDO.SecurityTypeId as PermissionType ' ||
		', Labels.LabelValue as PermissionName ' ||
		'From	CDODefinition CDO, Labels ' ||
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
--#delimiter

-- PROCEDURE: csiRBACAddVPagePermissions
-- DESCR: Creates a Permissions based on a query
--    pRoleId - The role to which the permissions are added
--  © 2017 Siemens Product Lifecycle Management Software Inc.
CREATE OR REPLACE
PROCEDURE csiRBACAddCMVPPermissions
  (
    pRoleId VARCHAR2)
AS
  vSQLString VARCHAR2(4000);
  c1 SYS_REFCURSOR;
  vUIVirtualPageId VARCHAR2(16);
  vPermissionName  VARCHAR2(255);
BEGIN
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
--#delimiter

-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE
PROCEDURE populatePortalMenuUpdateData
AS
	vMenuDefId         VARCHAR2(16);
	vAssignedMenuDefId VARCHAR2(16);
	vHomePage          VARCHAR2(16);
	vDefaultNotes      VARCHAR2(1000);
	vRoleId            VARCHAR2(16);
	vPermissionModesFlag_None NUMBER;
	vPermissionModesFlag_NoSecAdmn NUMBER;
	vPermissionSQL VARCHAR2(4000);
	vPermissionModesFlag_ReadOnly NUMBER;
	pInstanceId VARCHAR2(16);

BEGIN
	vPermissionModesFlag_None := 0;
	vPermissionModesFlag_ReadOnly := 1;
	vPermissionModesFlag_NoSecAdmn := 2;
     
	vDefaultNotes := 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.';

	--'Creating OEE MenuItem into existent Resource Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiResource';
	
	createPortalMenuItem (vMenuDefId, 7835, 110, 'OEE', 'LblMenuItemOEE','isOEEIndicator_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 120, 'OEE Details', 'LblMenuItemOEEDetails','isOEEIndicatorDetails_VP', '', '' );

	--'Creating OEE MenuItem into existent Resource V8 Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiResourceV8';
	
	createPortalMenuItem (vMenuDefId, 7835, 110, 'OEE', 'LblMenuItemOEE','isOEEIndicator_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 120, 'OEE Details', 'LblMenuItemOEEDetails','isOEEIndicatorDetails_VP', '', '' );	
	
    --'isMaterialQueues...'
	createPortalMenuDefinition('isMaterialQueues', 'Material Queues pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem (vMenuDefId, 7835, 0, 'Manage Inventory', 'isWeb_ManageInventory','isManageInventory_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 10, 'Manage Material Queue', 'isWeb_ManageMaterialQueue','isLoadUnloadResourceQueue_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 20, 'Manage Material Queue Simple', 'isWeb_ManageMaterialQueueSimple','isManageMaterialQueue_VP', '', '' );
	createPortalMenuItem (vMenuDefId, 7835, 30, 'Material Request Acknowledge', 'isWeb_MaterialRequestAcknowledge', 'isMaterialRequestAcknowledge_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 40, 'Material Request Status Board', 'isWeb_MaterialRequestStatusBoard', 'isMaterialRequestStatusBoard_VP', '', '');
	createPortalMenuItem (vMenuDefId, 7835, 50, 'Material Request', 'isWeb_MaterialRequest', 'isMaterialRequest_VP', '', '');

	 --'Creating Material Queues MenuItem into existent Portal Menu V8...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiPortalMenuV8';

	createPortalMenuItem(vMenuDefId, 7833, 100, 'Material Queue', 'isWeb_MaterialQueues', '', '', 'isMaterialQueues');
		
    --'Creating Material Queues MenuItem into existent Portal Mobile Menu ...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiMobileMenu';

	createPortalMenuItem(vMenuDefId, 7833, 100, 'Material Queue', 'isWeb_MaterialQueues', '', '', 'isMaterialQueues');

    --'Creating Material Queues MenuItem into existent Portal Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiPortalMenu';

	createPortalMenuItem(vMenuDefId, 7833, 100, 'Material Queue', 'isWeb_MaterialQueues', '', '', 'isMaterialQueues');


	-- Default Modeling Role
	select RoleId into vRoleId from RoleDef where Rolename = 'Default Modeling';
	vPermissionSQL := '110';
	csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'is Device Family Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Device Group Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Device Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Preactor Schedule Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Inventory Location Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Material Queue Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is OEE Settings Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Auto Start Settings Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'is Defect Mapping Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Image Maint', 110, 3760, vPermissionModesFlag_NoSecAdmn);

    -- Default Inquiry Role
	select RoleId into vRoleId from RoleDef where Rolename = 'Default Inquiry';
	vPermissionSQL := '140';
	csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Resource OEE Inquiry', 140, 4841607, vPermissionModesFlag_None);	
	
	
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE Rolename = 'Default Pages';
	csiRBACAssignRole (vRoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0);
	csiRBACAssignRole (vRoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0);
	csiRBACAddCMVPPermissions(vRoleId);
	csiRBACCreatePermission (vRoleId, 'isERPRoute_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isOrderStatusMaint', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isInventoryLocation_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isManageInventoryLocation_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isMaterialQueue_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isManageMaterailQueue_InvDetailPopup_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isManageMaterialQueue_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isOEESettings_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isOEEIndicator_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isOEEIndicatorDetails_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isImageMaint_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isDefectMapping_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isAutoStartSettingsMaint_VP', 110, 8143, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'isScheduledOrderDetailsVP', 110, 8143, vPermissionModesFlag_None);

	
	-- Default Manufacturing Role
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE Rolename = 'Default Mfg';
	vPermissionSQL := '100, 120, 130' ;
	csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Manage Inventory', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Remove Invnetory', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Transfer Inventory', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Load Material Queue', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Manage Material Queue', 110, 3760, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'Unload Material Queue', 110, 3760, vPermissionModesFlag_NoSecAdmn);

	--'Creating Industry Defect MenuItem into existent Container V8 Menu...'
	SELECT PortalMenuDefinitionId
	INTO vMenuDefId
	FROM PortalMenuDefinition
	WHERE PortalMenuDefinitionName = 'csiContainerV8';
	
	createPortalMenuItem (vMenuDefId, 7835, 600, 'Industry Defect', 'isLblMenuIndustryDefect','isCurrentDefect_VP', '', '' );
		
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE Rolename = 'Default Pages';
	csiRBACCreatePermission (vRoleId, 'isCurrentDefect_VP', 110, 8143, vPermissionModesFlag_None);

	delete  from SecurityCacheRefreshRequest;

	INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (47,SYSTIMESTAMP,SYS_EXTRACT_UTC(SYSTIMESTAMP),N'Default Pages',2);
	INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (49,SYSTIMESTAMP,SYS_EXTRACT_UTC(SYSTIMESTAMP),N'Default Modeling',2);
 
END;
--#delimiter
BEGIN
	populatePortalMenuUpdateData;
	COMMIT;
END;
--#delimiter

