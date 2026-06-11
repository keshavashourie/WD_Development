--------------------------------------------------------------------------------
-- SCRIPT:PopulateRBACDefaultData.sql
-- DESCR: Creates stored procedures used to create Roles, Permissions, etc.
--        and then uses those stored procedures to populate the default data
--
--
-- Change History:
--	09/21/2011 - Separated the function from other script so that this SP could be used elsewhere
--			Ramesh Nagamalli
--  	02/20/2014   Removed inserting WebMenuDefinitionId for Employee.
--			Oleg Khlus
-- 04/29/2014 - Added new Roles to support Change Management.
--			Maksim Kutsak
-- 05/13/2014 - Added new Permissions for Default Pages.
--			Sergey Yakimchik
--
-- 05/20/2014 - Added new Roles and Permissions to support Change Management.
--			Maksim Kutsak
--
-- 04/02/2019 - Added new Roles and Permissions with User Query Maint service attached.
--			Alexey Miroshnichenko
--
-- 04/22/2019 - Added new Advanced Modeling Permissions.
--			Oleg Kirasov
--
-- 06/03/2024 - Replaced dbms_obfuscation_toolkit package with dbms_crypto, as package dbms_obfuscation_toolkit will be obsolete with Oracle 21C onwards.
--			Madhuri Bhandari
/*
// Scripts to remove all RBAC data
DELETE FROM EMPLOYEE WHERE EMPLOYEENAME IN ('CamstarAdmin');
DELETE FROM ROLEDEF;
DELETE FROM ORGANIZATION WHERE ORGANIZATIONNAME='Corporate';
DELETE FROM ROLEPERMISSION;
DELETE FROM ROLEPERMISSIONMODES;
DELETE FROM EMPLOYEEROLE;
*/

--  Copyright Siemens 2024  

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
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Helper function to create Role record
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreateRole(pRoleName IN VARCHAR2, pRoleDescription IN VARCHAR2, pInstanceId OUT VARCHAR2)
AS
   vRoleCDODefId NUMBER := 7130;
BEGIN
    csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAssignRole(pRoleId IN VARCHAR2, pRoleDescription VARCHAR2, pEmployeeName IN VARCHAR2, pOrganizationName IN VARCHAR2, pPropagate IN NUMBER)
AS
    vCDODefId NUMBER:=7782;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR2(16);
    vRoleGID VARCHAR2(36);
    vRoleDescription VARCHAR2(255);
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
    INSERT INTO EmployeeRole(ExportImportkey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);
END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreatePermission(pRoleId IN VARCHAR2, pPermissionName IN VARCHAR2, pPermissionType IN NUMBER, pObjectMetaId IN NUMBER, pPermissionModesFlag IN NUMBER, pObjectInstanceId IN VARCHAR2 DEFAULT NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
    vIID VARCHAR2(16);
   vRoleGID VARCHAR2(36);
BEGIN
    csiPRDGetNextInstanceId(7783,vIID);
    csiCreateGUID(pPermissionName, vRoleGID);
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(vRoleGID,vIID,7783,pRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the pPermissionModesFlag value
	IF ( pPermissionModesFlag = 0 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=PPermissionType
          ORDER BY BitNumber;
    ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
		INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
		  SELECT vIID, 15263, BitNumber, ROWNUM
		  FROM SecurityMaskDetail
		  WHERE SecurityMaskId=PPermissionType
          AND BitNumber = 2
		  ORDER BY BitNumber;
    ELSIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230) ) THEN
		INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
		  SELECT vIID, 15263, BitNumber, ROWNUM
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
CREATE OR REPLACE PROCEDURE csiRBACCreatePermissionsForQry(pRoleId VARCHAR2, pSecurityList VARCHAR2, pPermissionModesFlag NUMBER)
AS
    vSQLString VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vObjectMetaId NUMBER;
    vPermissionType NUMBER;
    vPermissionName VARCHAR2(255);
BEGIN
    vSQLString := 'Select DISTINCT CDO.CDODefId As ObjectMetaId ' ||
		', CDO.SecurityTypeId as PermissionType ' ||
		', Labels.LabelValue as PermissionName ' ||
		'From	CDODefinition CDO ' ||
		'		, Labels ' ||
		'Where	Labels.LabelId = CDO.DisplayNameLabelId ' ||
		'And		CDO.IsAbstract = 0 ' ||
		'And		CDO.SecurityTypeId In ( ' || pSecurityList || ' ) ' ||
		'GROUP BY CDO.CDODefId, CDO.SecurityTypeId, Labels.LabelValue ORDER BY Labels.LabelValue ' ;

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
-- PROCEDURE: csiRBACCreatePermissionsForExternal
-- DESCR: Creates list of External Permissions based on a query
--		  pPermissionModesFlag - See rbacCreatePermissions
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreatePermissionsForExternal(pRoleId VARCHAR2, pExternalPermission VARCHAR2, pPermissionModesFlag NUMBER)
AS
    vSQLString VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vObjectMetaId NUMBER;
    vPermissionType NUMBER;
    vPermissionName VARCHAR2(255);
BEGIN
    vSQLString := 'SELECT CDOTypeId, SecTypeId, ExternalPermissionName ' ||
		'FROM ExternalPermission ' ||
		'OUTER APPLY (SELECT SecurityMaskId AS SecTypeId FROM SecurityMaskDefinition WHERE Name = ''' || pExternalPermission || ''') OA1 ' ||
		'ORDER By ExternalPermissionName ' ;

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
--		  pRoleId - The role to which the permissions are added
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAddVPagePermissions(pRoleId VARCHAR2)
AS
    vSQLString VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vUIVirtualPageId VARCHAR2(16);
    vPermissionName VARCHAR2(255);
BEGIN
    vSQLString := 'Select UIVirtualPageName, UIVirtualPageId ' ||
						 'From	UIVirtualPage ' ||
					   	 'Order By UIVirtualPageName ';
    OPEN c1 FOR vSQLString;
    FETCH c1 INTO vPermissionName,vUIVirtualPageId;
    WHILE (c1%FOUND) LOOP
       csiRBACCreatePermission (pRoleId, vPermissionName, 200, NULL, 0, vUIVirtualPageId);
       FETCH c1 INTO vPermissionName,vUIVirtualPageId;
    END LOOP;
    CLOSE c1;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateDefaultData
-- DESCR: Helper function to create Role record
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE rbacPopulateDefaultData
AS
    vRoleId VARCHAR2(16);
    vIID VARCHAR2(16);
    vSessionId VARCHAR2(16);
    vPermissionSQL VARCHAR2(4000);
    vCnt NUMBER;
    pInstanceId VARCHAR2(16);    

    vAdminPresent NUMBER;
    vInSiteAdminPresent NUMBER;

    vPermissionModesFlag_None NUMBER;
    vPermissionModesFlag_ReadOnly NUMBER;
    vPermissionModesFlag_NoSecAdmn NUMBER;
BEGIN
	vPermissionModesFlag_None := 0;
    	vPermissionModesFlag_ReadOnly := 1;
    	vPermissionModesFlag_NoSecAdmn := 2;

	SELECT COUNT(*) INTO vAdminPresent FROM Employee WHERE EmployeeName='Administrator';
	SELECT COUNT(*) INTO vInSiteAdminPresent FROM Employee WHERE EmployeeName='InSiteAdmin';

	-- Create Employee: CamstarAdmin (copy from InSiteAdmin)
	SELECT COUNT(*)
	INTO vCnt
	FROM Employee WHERE EmployeeName='CamstarAdmin';
    IF (vCnt=0) THEN
        csiPRDGetNextInstanceId (1140,vIID);
        csiPRDGetNextInstanceId (1130,vSessionId);
        INSERT INTO SessionValues (SessionValuesId, EmployeeId,  FactoryId, Application, 
                                Client, ChangeCount, CDOTypeId)  
        VALUES (vSessionId,  vIID, NULL, 0,  0, 1, 1130);
        
        INSERT INTO Employee (EmployeeId,  EmployeeName, FullName, SessionValuesId, CanLogin, 
                           ChangeCount, ModelerAccess, CDOTypeId) 
        VALUES (vIID, 'CamstarAdmin', 'Camstar Administrator', vSessionId, 1,   1, 1, 1140);
        
        UPDATE EMPLOYEE 
        SET  PortalMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenu'),
		PortalMobileMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiMobileMenu'),
		PortalV8MenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenuV8')
        WHERE EmployeeName = 'CamstarAdmin';
    END IF;    
    
    -- Create Default Organization
    csiPRDGetNextInstanceId (7543,vIID);
    INSERT INTO Organization(OrganizationId, OrganizationName, CDOTypeId, ChangeCount, Notes, ChangeHistoryId, Description, IconId, IsFrozen, ParentOrganizationId, OrganizationNumber)
       VALUES( vIID, 'Corporate', 7543, 1, NULL, NULL, 'Corporate Organization', 0, 0, NULL, NULL);

	-- Login Access Role
	csiRBACCreateRole ('Login','Login Access Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Login Access Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Login Access Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId,'Login Access Role', 'CamstarAdmin', NULL, 0);

    csiRBACCreatePermission (vRoleId, 'System', 190, 1, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'MenuDefinitionMaint', 110, 6907, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);

	-- Security Administration Role
	csiRBACCreateRole ('Security Administration','Security Administration Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Security Administration Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Security Administration Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Security Administration Role','CamstarAdmin', NULL, 0);

    csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'SetSessionFilterTagMaint', 180, 8777, vPermissionModesFlag_None);

    -- Each of the following blocks represent a query-based Role/Permission assignment for default services

	-- Default Modeling Role
	csiRBACCreateRole ('Default Modeling','Default Modeling Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Default Modeling Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Default Modeling Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Modeling Role','CamstarAdmin', NULL, 0);

    vPermissionSQL := '110';
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

    csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_NoSecAdmn);
    csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermissionsForExternal (vRoleId, 'ExternalPermission', vPermissionModesFlag_NoSecAdmn);
    csiRBACCreatePermission (vRoleId, 'UserQueryMaint', 230, 7069, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'BusinessRuleHandleMaint', 230, 7565, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'BusinessRuleMaint', 230, 7570, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'ScheduledBusinessRuleMaint', 230, 7588, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'SummaryTableDefMaint', 230, 8238, vPermissionModesFlag_ReadOnly);

	-- Default Modeling Read-Only Role...
    csiRBACCreateRole ('Default Modeling Read-Only','Default Modeling Read-Only Role',vRoleId);
    vPermissionSQL := '110';
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'EmployeeMaint', 180, 3760, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'RoleMaint', 180, 7140, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermissionsForExternal (vRoleId, 'ExternalPermission', vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'UserQueryMaint', 230, 7069, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'BusinessRuleHandleMaint', 230, 7565, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'BusinessRuleMaint', 230, 7570, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'ScheduledBusinessRuleMaint', 230, 7588, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'SummaryTableDefMaint', 230, 8238, vPermissionModesFlag_ReadOnly);


	-- Default Manufacturing Role
    csiRBACCreateRole ('Default Mfg','Default Manufacturing Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Default Manufacturing Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Default Manufacturing Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Manufacturing Role','CamstarAdmin', NULL, 0);

    vPermissionSQL := '100, 120, 130' ;
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);


	-- Default Quality Role
    csiRBACCreateRole ('Default Quality','Default Quality Role',vRoleId);
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Default Quality Role','Administrator', 'Corporate', 1);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Quality Role','CamstarAdmin', 'Corporate', 1);

    vPermissionSQL := '170'; 
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

	
	-- Default Inquiry Role
	csiRBACCreateRole ('Default Inquiry','Default Inquiry Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Default Inquiry Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Default Inquiry Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Inquiry Role','CamstarAdmin', NULL, 0);

    vPermissionSQL := '140'; 
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);

    
	-- Default Inquiry Role
    csiRBACCreateRole ('Default Export Import','Default Export Import Role',vRoleId);
    IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Default Export Import Role','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Default Export Import Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Export Import Role','CamstarAdmin', NULL, 0);

    vPermissionSQL := '150,160'; 
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);


	-- Default Pages Role
    csiRBACCreateRole ('Default Pages','Default Pages Role',vRoleId);
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Default Pages Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Default Pages Role','CamstarAdmin', NULL, 0);
	csiRBACAssignRole (vRoleId, 'Default Pages Role','InSiteAdmin', NULL, 0);
    csiRBACAddVPagePermissions (vRoleId);
	csiRBACCreatePermission (vRoleId, 'UI Virtual Page Maint', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Web Part Maint', 110, 8143, vPermissionModesFlag_None);
	
	
	-- Mfg Audit Trail Inquiry Role
    csiRBACCreateRole ('Mfg Audit Trail Inquiry','Mfg Audit Trail Inquiry Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'Container Txn Rev', 130, 5440, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'Container History Inquiry', 140, 6908, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'History View Maint', 110, 7083, vPermissionModesFlag_ReadOnly);

	-- Portal Configuration Role
    csiRBACCreateRole ('Portal Configuration','Portal Configuration Role',vRoleId);
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Portal Configuration Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Portal Configuration Role','CamstarAdmin', NULL, 0);
    csiRBACCreatePermission (vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Portal Studio RBAC', 210, 3, vPermissionModesFlag_None);
    
    
    -- Create SPC Role...
    BEGIN           
		select uivirtualpageid
		  INTO pInstanceId	
		  from uivirtualpage
		 where uivirtualpagename = 'SPCTesterFormVP';	
      EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 		 
    END;
    
    csiRBACCreateRole ('SPC','SPC',vRoleId); 
    csiRBACAssignRole (vRoleId, 'SPC','CamstarAdmin', NULL, 0);
    csiRBACCreatePermission (vRoleId,'SPCChartDefMaint', 110, 8204, vPermissionModesFlag_None);    
    csiRBACCreatePermission ( vRoleId,'SPCTesterFormVP', 200, NULL, vPermissionModesFlag_None, pInstanceId);        
    csiRBACCreatePermission (vRoleId,'Add SPC Annotation', 120, 8393, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId,'Record SPC Violation', 120, 8396, vPermissionModesFlag_None);
	
	
	-- DraftPermissions Role
    csiRBACCreateRole ('DraftPermissions','Draft Permissions Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
 	   
	-- DraftCamstarPermissions Role
    csiRBACCreateRole ('DraftCamstarPermissions','Draft Camstar Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
  -- DraftPLMPermissions Role
    csiRBACCreateRole ('DraftPLMPermissions','Draft PLM Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- DEPCPermissions Role
    csiRBACCreateRole ('DEPCPermissions','Deployment Complete Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- DEPIPermissions Role
    csiRBACCreateRole ('DEPIPermissions','Deployment Incomplete Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- PackageCreator Role
	csiRBACCreateRole ('Package Creator','Package Creator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'Start Change Pkg', 120, 8500, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	
	-- PackageOwner Role
	csiRBACCreateRole ('Package Owner','Package Owner Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval routing Sheet Maint', 110, 7820, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
 	
	-- Package Deployer Role 
	csiRBACCreateRole ('Package Deployer','Package Deployer Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	
	-- Package Activator Role 
	csiRBACCreateRole ('Package Activator','Package Activator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'ActivateChangePkg', 120, 8528, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ActivationInquiry', 140, 8554, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Export/Import Controller', 160, 7392, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Import Status Inquiry', 140, 7397, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Modeling data Import', 150, 7391, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
             

	-- Package Approver Role
	csiRBACCreateRole ('Package Approver','Package Approver Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);

  --Package Collaborator Role
	csiRBACCreateRole ('Package Collaborator','Package Collaborator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	
	
        
  --PDPermissions Role 
	csiRBACCreateRole ('PDPermissions','Pending Deployment Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

	--RejectPermissions Role 
	csiRBACCreateRole ('RejectPermissions','Rejected Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	--PACPermissions Role
	csiRBACCreateRole ('PACPermissions','Pending Approval Camstar Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None); 
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
  	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
       
	--PAPLMPermissions Role
	csiRBACCreateRole ('PAPLMPermissions','Pending Approval PLM Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None); 
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

	--Default Advanced Role
	csiRBACCreateRole ('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
	IF (vInSiteAdminPresent = 1) THEN
	    csiRBACAssignRole (vRoleId, 'Modeling Services for Advanced Users','InSiteAdmin', NULL, 0);
    END IF;    
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Modeling Services for Advanced Users','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Modeling Services for Advanced Users','CamstarAdmin', NULL, 0);
	vPermissionSQL := '230';
    csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
        
END;
/
BEGIN
rbacPopulateDefaultData;
END;
/


BEGIN
EXECUTE IMMEDIATE 'DROP PROCEDURE rbacPopulateDefaultData';
EXECUTE IMMEDIATE 'DROP PROCEDURE csiRBACAddVPagePermissions'; 
END;
/


