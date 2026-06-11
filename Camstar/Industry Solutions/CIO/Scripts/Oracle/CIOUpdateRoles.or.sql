--------------------------------------------------------------------------------
-- SCRIPT:CIOUpdateRoles.or.sql
-- DESCR: Creates stored procedures used to add CIO Roles for 
--		  portal pages and maint txns
-- Copyright Siemens 2019

-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
--#delimiter
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
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110), 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
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
  ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
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
  ELSIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230) ) THEN
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

-- PROCEDURE: csiRBACDelUnavailPermissions
-- DESCR: Removes permissions that do not match the specified type.

CREATE OR REPLACE PROCEDURE csiRBACDelUnavailPermissions(
	PermissionType			IN NUMBER,
	PermissionName		  IN VARCHAR2)
AS
	UnavialiblePermissions VARCHAR2(16);
BEGIN
    DECLARE
    CURSOR csr
    IS
		SELECT RolePermission.RolePermissionId FROM RolePermission
		WHERE RolePermissionName = PermissionName AND PermissionType <> PermissionType;
    BEGIN
    Open csr;
    LOOP
    FETCH csr into UnavialiblePermissions;
		EXIT WHEN csr%NOTFOUND;
			DELETE FROM RolePermission WHERE RolePermissionId = UnavialiblePermissions;
			DELETE FROM RolePermissionModes WHERE RolePermissionId = UnavialiblePermissions;
    END LOOP;
    CLOSE csr;
    END;
	END;
--#delimiter

-- PROCEDURE: csiRBACAddVPagePermissions
-- DESCR: Creates a Permissions based on a query
--    pRoleId - The role to which the permissions are added

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

	 csiRBACDelUnavailPermissions (230, 'CIO User Query Maint');
	-- Default Modeling Role
	select RoleId into vRoleId from RoleDef where Rolename = 'Default Modeling';
	vPermissionSQL := '110';
	csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO List Processor Maint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'CIO Outbound Msg Def Maint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'CIO Query Portal Page Maint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'CIO Settings Maint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'CIO Template Maint', 180, 7140, vPermissionModesFlag_NoSecAdmn);
	csiRBACCreatePermission (vRoleId, 'CIO User Query Maint', 230, 4727012, vPermissionModesFlag_ReadOnly);

	-- Default Modeling Read-Only Role
	select RoleId into vRoleId from RoleDef where Rolename = 'Default Modeling Read-Only';
	csiRBACCreatePermission (vRoleId, 'CIO User Query Maint', 230, 4727012, vPermissionModesFlag_ReadOnly);

	-- Default Modeling Advanced Role
	select RoleId into vRoleId from RoleDef where Rolename = 'Default Modeling Advanced';
	csiRBACCreatePermission (vRoleId, 'CIO User Query Maint', 230, 4727012, vPermissionModesFlag_NoSecAdmn);

	SELECT RoleId INTO vRoleId FROM RoleDef WHERE Rolename = 'Default Inquiry';
	csiRBACCreatePermission (vRoleId, 'CIOInquiry', 140, 4726863, vPermissionModesFlag_None);
	
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE Rolename = 'Default Pages';
	csiRBACAssignRole (vRoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0);
	csiRBACAddCMVPPermissions(vRoleId);

	csiRBACCreatePermission (vRoleId, 'CIO_ExpressionbuilderpopupVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_ListProcessorVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_OutboundMessageDefVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_QueryPortalPageMaintVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_QueryPortalPageVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_Settings_VP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_TemplateVP', 110, 7755, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CIO_UserQueryVP', 110, 7755, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'CIO_OutboundConnectionVP', 110, 7755, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'CIO_GenericRESTAPIVP', 110, 7755, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'CIO_OpcenterConnectMOMVP', 110, 7755, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'CIO_OutboundMessageDefVP2', 110, 7755, vPermissionModesFlag_None);

	delete  from SecurityCacheRefreshRequest;

	INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (47,SYSTIMESTAMP,SYS_EXTRACT_UTC(SYSTIMESTAMP),N'Default Pages',2);
	INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (49,SYSTIMESTAMP,SYS_EXTRACT_UTC(SYSTIMESTAMP),N'Default Modeling',2);
	
END;
--#delimiter


CREATE OR REPLACE
PROCEDURE AddCIOSessionInstance
AS
	vInstanceId VARCHAR2(16);
	vName  VARCHAR2(30):='ciosettings';
	vCDOTypeId NUMBER:=4726796;
	vURL  VARCHAR2(256):='https://127.0.0.1:13024/ClientGateway/gateway';
	vTimeout INT:=5;
	vDescription VARCHAR2(256);
        reccount INT:=0;
BEGIN

	SELECT count(*) into reccount FROM CIOSettings WHERE CIOSettingsName = vName;
        csiPRDGetNextInstanceId (vCDOTypeId, vInstanceId);

	IF (reccount = 0) THEN
		INSERT INTO CIOSettings
			(CIOSettingsId
			,CIOSettingsName
			,CDOTypeId
			,ChangeCount
			,ChangeHistoryId
			,Description
			,ClientGatewayURL
			,ClienbtGatewayPostTimeout
			,IconId
			,IsFrozen)
			VALUES
			(vInstanceId        
			,vName			
			,vCDOTypeId         
			,1                 
			,0                 
			,vDescription      
			,vURL			
			,vTimeout
			,0                 
			,0);               

	END IF;
END;
--#delimiter

BEGIN
	populatePortalMenuUpdateData;
        AddCIOSessionInstance;
	COMMIT;
END;
--#delimiter
BEGIN
	EXECUTE IMMEDIATE 'DROP PROCEDURE populatePortalMenuUpdateData';
	EXECUTE IMMEDIATE 'DROP PROCEDURE AddCIOSessionInstance';
END;
--#delimiter  


CREATE or REPLACE PROCEDURE CIO_CheckInboundMessage(pRetMsgName OUT VARCHAR2, pRetMsgType OUT VARCHAR2, pAdapterName OUT VARCHAR2)
AS
	pTxnId VARCHAR2(50) := '';
	pXMLResult XMLType;
BEGIN 
	begin
		SELECT DICTKEY, MESSAGENAME, MESSAGETYPE, DESTINATION, XMLType(CONTENTS) into pTxnId, pRetMsgName,  pRetMsgType, pAdapterName, pXMLResult
		FROM CIOMessage
		WHERE ROWNUM=1 AND OKtoDelete = 0
		AND Owners = 0
		ORDER BY MessageTimeStampGMT;
    exception
        when no_data_found then
        pTxnId := '';
    end;                       

	IF LENGTH(pTxnID) > 0 THEN
		UPDATE CIOMessage SET Owners = 1 
		WHERE DictKey = pTxnId;
	END IF; 
END;
--#delimiter

CREATE OR REPLACE PROCEDURE CIO_InboundMessageReceived(pMsgName IN VARCHAR2)
AS
BEGIN
	UPDATE CIOMessage SET OKtoDelete = 1 WHERE MessageName = pMsgName;
END;
--#delimiter

CREATE OR REPLACE PROCEDURE CIO_WakeUpAdapter(pRecordCount IN NUMBER)
AS
BEGIN
    DELETE FROM CIOMessage WHERE CIOMESSAGEID IN (select * from (SELECT CIOMESSAGEID
        FROM CIOMessage
        WHERE OKtoDelete = 1
        ORDER BY MessageTimeStampGMT) WHERE ROWNUM < pRecordCount);
END;
--#delimiter
