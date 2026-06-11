/* ==========================================================================
--  Common Query Functions
--     csiGetEnumerationLabels - Returns a list of Enum values and appropriate labels based on Dictionaries
-- HISTORY:
-- 09/21/2011     Ramesh Nagamalli     Extracted the csiPRDGetNextInstanceId procedure from other script so that this SP could be used elsewhere
-- 02/05/2025     Dan Maloney             Updated copyright to 2025
--                                                       Modified csiIncreaseStringColMaxLength stored procedure for BUG 458744.  
--                                                       See header comments in csiIncreaseStringColMaxLength
-- 07/29/2025     Madhuri Bhandari	Moved procedure DROP_DATABASE_OBJECT from UserAccessRoleFunctions.or.sql .
--									Removed call to DROP_DATABASE_OBJECT where not requird.
--
-- ========================================================================== */

CREATE OR REPLACE PROCEDURE DROP_DATABASE_OBJECT ( p_ObjectName        IN VARCHAR2
                          ,p_ObjectType        IN VARCHAR2 DEFAULT 'TABLE' )
IS
------------------------------------------------------------------------------------------------
-- This program is a helper procedure. Based on the input arguments, relevant database objects are
-- dropped from the user schema. 
-- 
--
-- Modification History:
-- Name            Date        Action
-- --------------------------  ----------    ----------------
-- Purushotham Neelakantachar    06/20/2008    Initial Creation
--
-- Copyright Siemens 2025  
------------------------------------------------------------------------------------------------
--
n_ObjectCount        NUMBER;
--
v_SQLStmt        VARCHAR2(512);
--
e_InvalidUser        EXCEPTION;
--
BEGIN
    --
    IF ( USER NOT IN ( 'SYS', 'SYSTEM', 'SYSMAN', 'DBSNMP', 'PERFSTAT', 'CTXSYS' ) )
    THEN
        --
        IF ( p_ObjectName <> 'DROP_DATABASE_OBJECT' )
        THEN
            --
            SELECT COUNT(Object_Name)
              INTO n_ObjectCount
              FROM USER_OBJECTS 
             WHERE Object_Name = UPPER(p_ObjectName)
               AND Object_Type = UPPER(p_ObjectType);
            --
            IF ( n_ObjectCount = 1 )
            THEN
                --
                v_SQLStmt := 'DROP '||p_ObjectType||' '||p_ObjectName;
                --
                EXECUTE IMMEDIATE v_SQLStmt;
                --
                DBMS_OUTPUT.PUT_LINE(INITCAP(p_ObjectType)||' '|| p_ObjectName||' dropped successfully');
                --
            ELSE
                --
                DBMS_OUTPUT.PUT_LINE(INITCAP(p_ObjectType)||' '|| p_ObjectName||' not found. Skipping...');
                --
            END IF; -- End IF ( n_ObjectCount = 1 )
            --
        END IF; -- End IF ( p_ObjectName <> 'DROP_DATABASE_OBJECT' )
        --
    ELSE
        --
        RAISE e_InvalidUser;
        --
    END IF; -- End IF ( USER NOT IN ( 'SYS', 'SYSTEM'
    --
EXCEPTION
    WHEN e_InvalidUser THEN
        --
        RAISE_APPLICATION_ERROR(-20300, 'DROP_DATABASE_OBJECT - The users "SYS", "SYSTEM", "SYSMAN", "DBSNMP", "PERFSTAT" and "CTXSYS" are not permitted. Please execute with a INSITE database user name');
        --
        
    WHEN OTHERS THEN
        --
        RAISE_APPLICATION_ERROR(-20301,'DROP_DATABASE_OBJECT - Error dropping objects: '||SQLERRM);
        --
END;
/
BEGIN
 DROP_DATABASE_OBJECT ( 'OTAB_ENUMLABEL', 'TYPE' );
END;
/
CREATE OR REPLACE 
TYPE otyp_EnumLabel AS OBJECT (DefaultValue INT, LabelValue VARCHAR2(255))
/
CREATE OR REPLACE 
TYPE otab_EnumLabel AS TABLE OF otyp_EnumLabel
/
CREATE OR REPLACE
FUNCTION CSIGETENUMERATIONLABELS( Enumeration IN VARCHAR2, PrimaryDictionary IN VARCHAR2, SecondaryDictionary IN VARCHAR2 ) 
RETURN otab_EnumLabel
PIPELINED
AS
   CURSOR cEnumLabels(pEnumeration IN VARCHAR2, pPrimaryDictionary IN VARCHAR2, pSecondaryDictionary IN VARCHAR2) IS
      SELECT
        f.DefaultValue,
        COALESCE(Term.labelvalue, Lang.labelvalue, l.labelvalue) LabelValue
    FROM CDOFields f
    JOIN CDODefinition c on c.CDODefID = f.CDODefID
    JOIN Labels l on l.LabelID = f.LabelID
    LEFT JOIN DictionaryLabel Term ON Term.labelid = l.labelid AND Term.dictionaryid = pPrimaryDictionary
    LEFT JOIN DictionaryLabel Lang ON Lang.labelid = l.labelid AND Lang.dictionaryid = pSecondaryDictionary
    WHERE c.CDOName = pEnumeration;
BEGIN    
    FOR crec IN cEnumLabels(Enumeration, PrimaryDictionary, SecondaryDictionary) LOOP
       PIPE ROW(otyp_EnumLabel(crec.DefaultValue, crec.LabelValue));
    END LOOP;           
    
    RETURN;   
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiPRDGetNextInstanceId
-- DESCR: Helper function to create instance id string from a CDODefId and
--        Instance Id number and Site.
--
-- Copyright Siemens 2025  
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiPRDGetNextInstanceId(pCDODefId IN NUMBER, pInstanceIdStr OUT VARCHAR2)
AS
   vCDODefIdStr    VARCHAR2(16);
   vInstanceId     NUMBER;
   vInstIdNewValue VARCHAR2(16);
   
   iInstIdInt NUMBER;
   iHexSite   NUMBER;
   iHexId     NUMBER;  
   IdCount    NUMBER;       
BEGIN
	csiUpdateInstanceId(0,pCDODefId,1,vInstIdNewValue);
	--
	-- Add the Site
	--
	SELECT COUNT(*) INTO IdCount FROM DBIdentifier;
	IF(IdCount != 1) then
	  begin
		SELECT TO_NUMBER(0000000000,'xxxxxxxxxx') INTO iHexSite FROM dual; 
	  end;
	else
	  begin
		SELECT TO_NUMBER(nvl(dbidentifier,0),'xxxxxxxxxx') INTO iHexSite FROM DBIdentifier; 
	  end;
	end if;
	vInstIdNewValue:=SUBSTR(vInstIdNewValue,7,10);        
        
	iHexId := TO_NUMBER(vInstIdNewValue,'xxxxxxxxxx');
	iInstIdInt := iHexSite + iHexId - BITAND(iHexSite,iHexId);	-- this is doing a Bitwise OR
	vInstIdNewValue:=LPAD(LTRIM(TO_CHAR(iInstIdInt,'xxxxxxxxxx')),10,'0');
	--	
	vCDODefIdStr:=LPAD(LTRIM(TO_CHAR(pCDODefId,'xxxxxx')),6,'0');
	pInstanceIdStr:=LOWER(vCDODefIdStr||vInstIdNewValue);
	        
        EXCEPTION WHEN NO_DATA_FOUND THEN
	  vInstIdNewValue:=LPAD(SUBSTR(vInstIdNewValue,14,10),10,'0');        
	  vCDODefIdStr:=LPAD(LTRIM(TO_CHAR(pCDODefId,'xxxxxx')),6,'0');
	  pInstanceIdStr:=LOWER(vCDODefIdStr||vInstIdNewValue);

END;
/
--------------------------------------------------------------------------------------------------
-- Function to recursively check if a resource is in a resource group
-- DESCR: Can specify resource and group by ID or name. 
--		  Support for name was mainly a development convenience so can remove if not needed.
-- 
--  Modification History:
--  Name            	Date         Action
--  --------------      -----------  ----------------
--  John Rumpf          26-Jan-2020  Created
--  John Rumpf			26-Apr-2024  Update to return Resource and Resource Group IDs
--
-- Copyright Siemens 2024  
--------------------------------------------------------------------------------------------------
BEGIN
 DROP_DATABASE_OBJECT ( 'otab_ResourceFound', 'TYPE' );
END;
/
CREATE OR REPLACE 
TYPE otyp_ResourceFound AS OBJECT (Found INT, ResourceName VARCHAR2(30), ResourceId CHAR(16), ResourceGroupId CHAR(16))
/
CREATE OR REPLACE 
TYPE otab_ResourceFound AS TABLE OF otyp_ResourceFound
/
CREATE OR REPLACE
FUNCTION csiResourceInGroup( p_ResourceId IN VARCHAR2, p_ResourceName IN VARCHAR2, p_ResourceGroupId IN VARCHAR2, p_ResourceGroupName VARCHAR2 ) 
RETURN otab_ResourceFound
AS
    v_results_t otab_ResourceFound := otab_ResourceFound();    

    v_Found INTEGER;
    v_ChildGroupId CHAR(16);
    v_ResourceId CHAR(16);
    v_ResourceName VARCHAR2(30);
    v_ResourceGroupId CHAR(16);
BEGIN
    v_ResourceId := p_ResourceId;
    v_ResourceName := p_ResourceName;
    v_ResourceGroupId := p_ResourceGroupId;
    
	-- support params set by id or name and validate
	IF v_ResourceId is null OR v_ResourceId = '' THEN
		IF v_ResourceName is not null THEN
			SELECT ResourceId INTO v_ResourceId FROM ResourceDef WHERE ResourceName = v_ResourceName;
        END IF;
    END IF;

	IF v_ResourceGroupId is null OR v_ResourceGroupId = '' THEN
		IF p_ResourceGroupName is not null THEN
			SELECT ResourceGroupId INTO v_ResourceGroupId FROM ResourceGroup WHERE ResourceGroupName = p_ResourceGroupName;
        END IF;
    END IF;

	IF v_ResourceId is null or v_ResourceGroupId is null THEN
		RETURN (v_results_t);
    END IF;

	IF v_ResourceName is null THEN
		SELECT ResourceName INTO v_ResourceName from ResourceDef WHERE ResourceId = v_ResourceId;
    END IF;
    
	-- check if the resource is one of the entries of the specified group
	SELECT count(*) INTO v_Found FROM ResourceGroupEntries WHERE ResourceGroupId = v_ResourceGroupId and EntriesId = v_ResourceId;

	IF v_Found = 0 THEN
        FOR groups IN(
            SELECT GroupsId FROM ResourceGroupGroups WHERE ResourceGroupId = v_ResourceGroupId
        )
        LOOP
            SELECT count(*) INTO v_Found FROM ResourceGroupEntries WHERE ResourceGroupId = groups.GroupsId and EntriesId = v_ResourceId;
            EXIT WHEN v_Found = 1;
            
            FOR findResult IN (SELECT * FROM TABLE(csiResourceInGroup(v_ResourceId, null, groups.GroupsId, null)))
            LOOP
                v_Found := findResult.Found;
                -- function only returns one row so no need to exit the loop
            END LOOP;
            EXIT WHEN v_Found = 1;
        END LOOP;
    END IF;
        
    IF v_Found = 1 THEN
        v_results_t.extend();
        v_results_t(v_results_t.count) := otyp_ResourceFound(v_Found, v_ResourceName, v_ResourceId, v_ResourceGroupId);
    END IF;
    
    RETURN (v_results_t);   
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiGenerateAutoNumber
-- DESCR: Function to generate sequences (for NumberingRule) with the following format <vPrefix><sequence><vSuffix>
--        Returns the Auto Numbers in the following format <AutoNumber01>|<AutoNumber02>|<AutoNumber03>
--
-- Copyright Siemens 2025  
--------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION csiGenerateAutoNumber (vSequencesRequested IN VARCHAR2, vPrefix IN VARCHAR2, vSuffix IN VARCHAR2, vLastSequence IN NUMBER, vSequenceLength IN NUMBER, vUseHex IN NUMBER)
RETURN CLOB
IS
    vSequence      NUMBER;
    vSequenceValue VARCHAR2(40);
    vIsFirst       BOOLEAN := TRUE;
    vParmValue     CLOB;
    vRowsProcessed NUMBER;
   
    vErrLoc        NUMBER;
    vDelim         VARCHAR2(1) := '|';
BEGIN
    vErrLoc := 1;  

    -- Append the padding for the sequence length
    vSequence := vLastSequence;
    FOR indx IN 0 .. vSequencesRequested - 1
    LOOP
        -- increment the sequence
        vSequence := vSequence + 1;
        
        -- set the padding for the sequence length  
        vSequenceValue := LPAD(LTRIM(vSequence), vSequenceLength, '0');      
        IF (vUseHex = 1) THEN
            vSequenceValue := LPAD(LTRIM(TO_CHAR(vSequence,'xxxxxxxxxxxxxxxx')),vSequenceLength,'0');
        END IF;
        
        vSequenceValue := vPrefix||vSequenceValue||vSuffix;

        IF (NOT vIsFirst) THEN
            vParmValue := vParmValue||vDelim;
        END IF;
   	vParmValue := vParmValue||vSequenceValue;
	vIsFirst := FALSE;

    END LOOP;

    RETURN vParmValue;
    
END;
/
CREATE OR REPLACE PROCEDURE csiAuthRetrieveUserPermissions
  ( cur_OUT OUT SYS_REFCURSOR, UserName IN varchar2
    ) AS
	c_rolepermission SYS_REFCURSOR; 
    temp_rolepermission RESOLVEDPERMISSIONS_TEMPV3%ROWTYPE; 
BEGIN
    delete from RESOLVEDPERMISSIONS_TEMPV2;
    delete from RESOLVEDPERMISSIONS_TEMPV3;
    --records are assign to cursor 'c_rolepermission'
    CSIAUTHRETRIEVEUSERROLES (c_rolepermission, UserName);
    LOOP
        --fetch cursor 'c_rolepermission' into tmpresults table type 'temp_rolepermission'
    FETCH c_rolepermission INTO temp_rolepermission;
    --exit if no more records
    EXIT WHEN c_rolepermission%NOTFOUND;
    INSERT INTO RESOLVEDPERMISSIONS_TEMPV3
     (RoleName, OrganizationName)
    VALUES
     (temp_rolepermission.RoleName, temp_rolepermission.OrganizationName);
    END LOOP;
    CLOSE c_rolepermission; 

    INSERT INTO RESOLVEDPERMISSIONS_TEMPV2(RoleName,PermissionName,PermissionType,PermissionMode,OrganizationName,ObjectMetaId,ObjectInstanceId,ObjectCDOName) 
		SELECT tmp.RoleName,  
        rp.RolePermissionName PermissionName,  
        rp.PermissionType,  
        rpm.Modes PermissionMode,  
        tmp.OrganizationName,  
        rp.ObjectMetaId,  
        rp.ObjectInstanceId,  
        NVL(cdo.CDOName,'System') ObjectCDOName 
    FROM RESOLVEDPERMISSIONS_TEMPV3 tmp 
    JOIN RoleDef r ON r.RoleName=tmp.RoleName 
    JOIN RolePermission rp ON rp.RoleId=r.RoleId  
    JOIN RolePermissionModes rpm ON rpm.RolePermissionId=rp.RolePermissionId 
    LEFT OUTER JOIN CDODefinition cdo ON cdo.CDODefID=rp.ObjectMetaId;
		
	OPEN cur_OUT FOR SELECT RoleName,PermissionName,PermissionType,PermissionMode,OrganizationName,ObjectMetaId,ObjectInstanceId,ObjectCDOName 
    FROM RESOLVEDPERMISSIONS_TEMPV2;
    
 END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAddVPagePermissions
-- DESCR: Creates a Permissions based on a query
--    pRoleId - The role to which the permissions are added
--  Copyright Siemens 2025  
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
CREATE OR REPLACE PROCEDURE csiRBACCreatePermission(
pRoleId IN VARCHAR2, 
pPermissionName IN VARCHAR2, 
pPermissionType IN NUMBER, 
pObjectMetaId IN NUMBER, 
pPermissionModesFlag IN NUMBER, 
pObjectInstanceId IN VARCHAR2 DEFAULT NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
vIID		VARCHAR2(16);
vRoleGID	VARCHAR2(36);
reccount	INT;
BEGIN
	DBMS_OUTPUT.ENABLE(NULL);
	csiPRDGetNextInstanceId(7783,vIID);

	SELECT count(*) INTO reccount 
	FROM RolePermission 
	WHERE RoleId = pRoleId and RolePermissionname = pPermissionName;

	IF (reccount = 0) THEN
		csiCreateGUID(pPermissionName, vRoleGID);
		INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
		VALUES(vRoleGID,vIID,7783,pRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);
	END IF;

	-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
	-- based on the pPermissionModesFlag value
	IF ( pPermissionModesFlag = 0 ) THEN
		BEGIN
			INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			SELECT vIID, 15263, BitNumber, ROWNUM
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=PPermissionType
			ORDER BY BitNumber;
		EXCEPTION
		WHEN DUP_VAL_ON_INDEX THEN
			NULL;
		END;
	ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180) ) THEN
		BEGIN
			INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			SELECT vIID, 15263, BitNumber, ROWNUM
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=PPermissionType
			AND BitNumber = 2
			ORDER BY BitNumber;
		EXCEPTION
		WHEN DUP_VAL_ON_INDEX THEN
			NULL;
		END;
    ELSIF ( pPermissionModesFlag = 2 AND pPermissionType = 180 ) THEN
		BEGIN
			INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			SELECT vIID, 15263, BitNumber, ROWNUM
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=PPermissionType
			AND BitNumber IN (1,2,3,4)
			ORDER BY BitNumber;
		EXCEPTION
		WHEN DUP_VAL_ON_INDEX THEN
			NULL;
		END;
	ELSE
		DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for @PermissionModeFlag parameter...');
	END IF;
END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  pPermissionModesFlag - See rbacCreatePermissions
--  Copyright Siemens 2025  
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
-- PROCEDURE: csiRBACCreatePermissionsForExternal
-- DESCR: Creates list of External Permissions based on a query
--		  pPermissionModesFlag - See rbacCreatePermissions
--  Copyright Siemens 2025  
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
-------------------------------------------------------------------------------
-- Gets material list that defines required components for the container
-- Used with an HV machine setup to determine what components were issued
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiContainerMaterialList_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiContainerMaterialList_typ AS OBJECT (
    ProductId CHAR(16), 
    ReferenceDesignator VARCHAR2(30), 
    SpecId CHAR(16),
    QtyRequired NUMBER,
	MaterialListItemId CHAR(16)    
)
/
CREATE OR REPLACE TYPE csiContainerMaterialList_tab AS TABLE OF csiContainerMaterialList_typ
/
CREATE OR REPLACE FUNCTION csiGetContainerMaterialList(
    p_ContainerId IN VARCHAR2, 
    p_MfgOrderId IN VARCHAR2, 
    p_BOMId IN VARCHAR2
)
RETURN csiContainerMaterialList_tab
AS
    v_results_t csiContainerMaterialList_tab := csiContainerMaterialList_tab();

    v_HaveList INT;
    v_ItemCount INT;
BEGIN
	-- follow priority used by CVE on Container.MaterialList: This_Value;ContainerMaterialList;MfgOrder.MaterialList;BOM.MaterialList
	-- CVE on Container.BOM: This_Value;Product.BOM;Product.ERPBOM

    v_HaveList := 0;
    
	-- Material list directly on container
	select count(*) into v_ItemCount from ContainerMaterialListItem where ContainerId = p_ContainerId;
	IF v_ItemCount > 0 THEN
        FOR matItem IN (
            select 
                p.ProductId,
                item.ReferenceDesignator,
                item.QtyRequired,
                item.ContainerMaterialListItemId
            from ContainerMaterialListItem item
            left join Product p on (
                (p.ProductId = item.ProductId) or
                (item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
            )
            where ContainerId = p_ContainerId
        )
        LOOP
            v_results_t.extend;
            v_results_t(v_results_t.count) := csiContainerMaterialList_typ(matItem.ProductId, matItem.ReferenceDesignator, null, matItem.QtyRequired, matItem.ContainerMaterialListItemId);
        END LOOP;
		v_HaveList := 1;
	END IF;
	
	-- Material list on MfgOrder
	IF v_HaveList = 0 and p_MfgOrderId is not null THEN
		select count(*) into v_ItemCount from MfgOrderMaterialListItem where MfgOrderId = p_MfgOrderId;
		IF v_ItemCount > 0 THEN
			FOR matItem IN (
				select 
					p.ProductId,
					item.ReferenceDesignator,
                    item.QtyRequired,
                    item.MfgOrderMaterialListItemId
				from 
					MfgOrderMaterialListItem item
					left join Product p on (
						(p.ProductId = item.ProductId) or
						(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
					)
				where MfgOrderId = p_MfgOrderId
			)
			LOOP
				v_results_t.extend;
				v_results_t(v_results_t.count) := csiContainerMaterialList_typ(matItem.ProductId, matItem.ReferenceDesignator, null, matItem.QtyRequired, matItem.MfgOrderMaterialListItemId);
			END LOOP;
			v_HaveList := 1;
		END IF;
	END IF;

	-- BOM.MaterialList
	-- CVE on Container.BOM: This_Value;Product.BOM;Product.ERPBOM
	-- for BOM directly on container or Product.BOM, material table is ProductMaterialListItem which has Spec
	-- for Product.ERPBOM, table is BOMMaterialListItem which does not have Spec
    IF v_HaveList = 0 and p_BOMId is not null THEN
        select count(*) into v_ItemCount from ERPBOM where ERPBOMId = p_BOMId;
		IF v_ItemCount > 0 THEN -- Is an ERPBOM
			select count(*) into v_ItemCount from BOMMaterialListItem where ERPBOMId = p_BOMId;
			IF v_ItemCount > 0 THEN
				FOR matItem IN(
					select
						p.ProductId,
						item.ReferenceDesignator,
                        item.QtyRequired,
                        item.BOMMaterialListItemId
					from	
						BOMMaterialListItem item
						left join Product p on (
							(p.ProductId = item.ProductId) or
							(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
						)
					where item.ERPBOMId = p_BOMId
				)
				LOOP
					v_results_t.extend;
					v_results_t(v_results_t.count) := csiContainerMaterialList_typ(matItem.ProductId, matItem.ReferenceDesignator, null, matItem.QtyRequired, matItem.BOMMaterialListItemId);
				END LOOP;
			END IF;
		ELSE
			select count(*) into v_ItemCount from ProductMaterialListItem where BOMId = p_BOMId;
			IF v_ItemCount > 0 THEN
                FOR matItem IN (
                    select 
                        p.ProductId,
                        item.ReferenceDesignator,
                        s.SpecId,
                        item.QtyRequired,
                        item.ProductMaterialListItemId
                    from 
                        ProductMaterialListItem item
                        left join Product p on (
                            (p.ProductId = item.ProductId) or
                            (item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
                        )
                        left join Spec s on (
                            (s.SpecId = item.SpecId) or
                            (item.SpecBaseId <> '0000000000000000' and s.SpecId in (select RevOfRcdId from SpecBase where SpecBaseId = item.SpecBaseId))
                        )
                    where item.BOMId = p_BOMId
                )
                LOOP
                    v_results_t.extend;
                    v_results_t(v_results_t.count) := csiContainerMaterialList_typ(matItem.ProductId, matItem.ReferenceDesignator, matItem.SpecId, matItem.QtyRequired, matItem.ProductMaterialListItemId);
                END LOOP;
			END IF;
		END IF;
	END IF;
    
    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Gets HV setup details for a specified setup
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiHVSetupDetail_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiHVSetupDetail_typ AS OBJECT (
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	CompID VARCHAR2(100),
	CompName VARCHAR2(100),
	Slot INT,
	SubSlot INT,
    ProductName VARCHAR2(100),
    ProductRevision VARCHAR2(25)
)
/
CREATE OR REPLACE TYPE csiHVSetupDetail_tab AS TABLE OF csiHVSetupDetail_typ
/
CREATE OR REPLACE FUNCTION csiGetHVSetupDetails(
    p_HVSetupId IN VARCHAR2
)
RETURN csiHVSetupDetail_tab
AS
    v_results_t csiHVSetupDetail_tab := csiHVSetupDetail_tab();
BEGIN
	-- For initial implementation each HVSetup is a full setup, no deltas, so a simple select works.
	-- To suport deltas, we would need to update this to build the component list from multiple setups.
	-- The basic process would be
	--		initialize a local var @SetupId to passed in value @HVSetupId (this is the top level setup set on the resource)
	--		while (@SetupId is not null)
	--		{
	--			get all details for @SetupId
	--			insert all into results table where Slot/SubSlot not already in results table (so could probably do first insert separate without check)
	--			set @SetupId = PriorSetupId (current field ParentSetupId in HVComponentIssueHistory needs to change to PriorSetupId)
	--		}
	--		return
    FOR detail IN (
        select
            d.ParentId,
            d.HVSetupHistoryDetailId,
            d.ProductId,
            d.CompId,
            d.CompName,
            d.Slot,
            d.SubSlot,
            pb.ProductName,
            p.ProductRevision
        from HVSetupHistoryDetail d
        join Product p on p.ProductId = d.ProductId
        join ProductBase pb on pb.ProductBaseId = p.ProductBaseId
        where ParentId = p_HVSetupId
    )
    LOOP
        v_results_t.extend;
        v_results_t(v_results_t.count) := csiHVSetupDetail_typ(
            detail.ParentId,
            detail.HVSetupHistoryDetailId,
            detail.ProductId,
            detail.CompId,
            detail.CompName,
            detail.Slot,
            detail.SubSlot,
            detail.ProductName,
            detail.ProductRevision
        );
    END LOOP;
    
    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Get params needed to query for additional HV component issue information.
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiHVIssueParams_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiHVIssueParams_typ AS OBJECT (
	HVSetupId CHAR(16),
    SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	ContainerId CHAR(16),
	MfgOrderId CHAR(16),
	BOMId CHAR(16),
	ResourceName VARCHAR2(100),
	WorkflowStepName VARCHAR2(100),
	SpecName VARCHAR2(100),
	TxnDateGMT DATE,
	HVComponentIssueHistoryId CHAR(16)
)
/
CREATE OR REPLACE TYPE csiHVIssueParams_tab AS TABLE OF csiHVIssueParams_typ
/
CREATE OR REPLACE FUNCTION csiGetHVIssueHistoryParams(
    p_HVIssueHistoryDetailId IN VARCHAR2
)
RETURN csiHVIssueParams_tab
AS
    v_results_t csiHVIssueParams_tab := csiHVIssueParams_tab();
BEGIN
    FOR params IN (
        select 
            summary.HVSetupId,
            summary.SpecId,
            summary.WorkflowStepId,
            detail.ContainerId,
            detail.MfgOrderId, 
            detail.BOMId,
            res.ResourceName,
            wfs.WorkflowStepName,
            sb.SpecName || ' (' || spec.SpecRevision || ')' as SpecName, -- TODO: verify this is correct
            summary.TxnDateGMT,
            summary.HVComponentIssueHistoryId
        from HVIssueHistoryDetail detail
        join HVComponentIssueHistory summary on summary.HVComponentIssueHistoryId = detail.ParentId
        join ResourceDef res on res.ResourceId = summary.ResourceId
        join WorkflowStep wfs on wfs.WorkflowStepId = summary.WorkflowStepId
        join Spec spec on spec.SpecId = summary.SpecId
        join SpecBase sb on sb.SpecBaseId = spec.SpecBaseId
        where detail.HVIssueHistoryDetailId = p_HVIssueHistoryDetailId
    )
    LOOP
        v_results_t.extend;
        v_results_t(v_results_t.count) := csiHVIssueParams_typ(
            params.HVSetupId,
            params.SpecId,
            params.WorkflowStepId,
            params.ContainerId,
            params.MfgOrderId,
            params.BOMId,
            params.ResourceName,
            params.WorkflowStepName,
            params.SpecName,
            params.TxnDateGMT,
            params.HVComponentIssueHistoryId
        );
    END LOOP;

    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Get HV removed component information for a given container
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiHVRemovedComponents_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiHVRemovedComponents_typ AS OBJECT (
	HVRemoveId CHAR(16),
	HVRemoveDetailId CHAR(16),
	ContainerId CHAR(16),
	MaterialListItemId CHAR(16),
	QtyRemoved NUMBER, 
	DestinationLot VARCHAR2(100),
	DestinationStockPoint VARCHAR2(100),
	RemovalReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	HVSetupId CHAR(16),
	HVSetupDetailId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	TxnDateGMT DATE
)
/
CREATE OR REPLACE TYPE csiHVRemovedComponents_tab AS TABLE OF csiHVRemovedComponents_typ
/
CREATE OR REPLACE FUNCTION csiGetHVRemovedComponents(
    p_ContainerId IN VARCHAR2,
	p_HVIssueHistoryDetailId IN VARCHAR2
)
RETURN csiHVRemovedComponents_tab
AS
    v_results_t csiHVRemovedComponents_tab := csiHVRemovedComponents_tab();
BEGIN
    FOR removedComponent IN (
    select
		rmDetail.HVRemoveHistoryDetailId as HVRemoveId,
		rmSetupDetail.HVRemoveHistorySetupDetailId as HVRemoveDetailId, 
		rmDetail.ContainerId, 
		rmDetail.MaterialListItemId, 
		rmDetail.QtyRemoved,
		rmDetail.DestinationLot,
		rmDetail.DestinationStockPoint,
		rmDetail.RemovalReasonId,
		rmDetail.RemoveDifferenceReasonId,
		rmSetupDetail.HVSetupId,
		rmSetupDetail.HVSetupDetailId, 
		rmSetupDetail.HVIssueHistoryDetailId,
		rmDetail.SpecId,
		rmDetail.WorkflowStepId,
		rmDetail.TxnDateGMT
	from 
		HVRemoveHistoryDetail rmDetail
		join HVRemoveHistorySetupDetail rmSetupDetail on rmSetupDetail.ParentId = rmDetail.HVRemoveHistoryDetailId
	where 
		rmSetupDetail.ContainerId = p_ContainerId -- container match
		and rmDetail.HVIssueHistoryDetailId = p_HVIssueHistoryDetailId
    )
    LOOP
        v_results_t.extend;
        v_results_t(v_results_t.count) := csiHVRemovedComponents_typ(
            removedComponent.HVRemoveId,
            removedComponent.HVRemoveDetailId,
            removedComponent.ContainerId,
            removedComponent.MaterialListItemId,
            removedComponent.QtyRemoved,
            removedComponent.DestinationLot,
            removedComponent.DestinationStockPoint,
            removedComponent.RemovalReasonId,
            removedComponent.RemoveDifferenceReasonId,
            removedComponent.HVSetupId,
            removedComponent.HVSetupDetailId,
            removedComponent.HVIssueHistoryDetailId,
            removedComponent.SpecId,
            removedComponent.WorkflowStepId,
            removedComponent.TxnDateGMT
        );
    END LOOP;

    RETURN (v_results_t);
END;
/

-------------------------------------------------------------------------------
-- Get details for a single HV issue record (one container processed at one resource with an HV setup)
-- May return multiple rows for a ref des if component product defined in multiple slots

-- HVIssueHistoryDetail holds the container info, but HV setup, Resource, Spec and WorkflowStep are set in the parent HVComponentIssueHistory.
-- If Spec is set, it is used as a filter to get only material items with matching Spec. Otherwise, all material items are used.
-- The history detail record saves the BOM and MfgOrder at the time of container move, just in case these were changed after the move.
-- However, we cannot compensate for the material list for that BOM or MfgOrder being changed. 

-- We are requiring a material list since standard Component Issue page shows no materials to issue for a container if it has no material list
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiHVIssueDetail_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiHVIssueDetail_typ AS OBJECT (
	ReferenceDesignator VARCHAR2(100),
	CompName VARCHAR2(32),
	ProductName VARCHAR2(100),
	ProductRevision VARCHAR2(25),
	FromLot VARCHAR2(100),
	IssueControl INT,
	ResourceName VARCHAR2(100),
	Slot INT,
	SubSlot INT,
    QtyIssued NUMBER,
	WorkflowStepName VARCHAR2(100),
	SpecName VARCHAR2(100),
	TxnDateGMT DATE,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved NUMBER,
	NetQtyIssued NUMBER,
	RemoveDestinationLot VARCHAR2(100),
	RemoveDestinationStockPoint VARCHAR2(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATE
)
/
CREATE OR REPLACE TYPE csiHVIssueDetail_tab AS TABLE OF csiHVIssueDetail_typ
/
CREATE OR REPLACE FUNCTION csiGetDetailsForSingleHVIssue(
    p_HVIssueHistoryDetailId IN VARCHAR2
)
RETURN csiHVIssueDetail_tab
AS
    v_results_t csiHVIssueDetail_tab := csiHVIssueDetail_tab();

	v_HVSetupId CHAR(16);
	v_SpecId CHAR(16);
	v_WorkflowStepId CHAR(16);
	v_ContainerId CHAR(16);
	v_MfgOrderId CHAR(16);
	v_BOMId CHAR(16);
	v_ResourceName VARCHAR2(100);
	v_WorkflowStepName VARCHAR2(100);
	v_SpecName VARCHAR2(100);
	v_TxnDateGMT DATE;
    v_HVComponentIssueHistoryId CHAR(16);

BEGIN
	-- get params needed from the history records
	select 
		HVSetupId, SpecId, WorkflowStepId, ContainerId, MfgOrderId, BOMId, ResourceName, WorkflowStepName, SpecName, TxnDateGMT, HVComponentIssueHistoryId
    into
		v_HVSetupId, v_SpecId, v_WorkflowStepId, v_ContainerId, v_MfgOrderId, v_BOMId, v_ResourceName, v_WorkflowStepName, v_SpecName, v_TxnDateGMT, v_HVComponentIssueHistoryId
    from table(csiGetHVIssueHistoryParams(p_HVIssueHistoryDetailId));

    FOR issueDetail IN (
        with MaterialList(ProductId, ReferenceDesignator, SpecId, QtyRequired, MaterialListItemId) as
        (
            select * from TABLE(csiGetContainerMaterialList(v_ContainerID, v_MfgOrderId, v_BOMId))
        ),
        SetupDetails(HVResourceSetupHistoryId, HVSetupHistoryDetailId, ProductId, CompId, CompName, Slot, SubSlot, ProductName, ProductRevision) as
        (
            select * from TABLE(csiGetHVSetupDetails(v_HVSetupId))
        ),
        RemovedComponents(HVRemoveId, HVRemoveDetailId, ContainerId, MaterialListItemId, QtyRemoved, DestinationLot, DestinationStockPoint, RemovalReasonId, RemoveDifferenceReasonId, 
                          HVSetupId, HVSetupDetailId, HVIssueHistoryDetailId, SpecId, WorkflowStepId, TxnDateGMT )  as 
        (
            select * from table(csiGetHVRemovedComponents(v_ContainerID, p_HVIssueHistoryDetailId))
        )        
        select 
            m.ReferenceDesignator,
            sd.CompName,
            sd.ProductName,
            sd.ProductRevision,
            sd.CompId as FromLot,
            sd.Slot,
            sd.SubSlot,
            m.QtyRequired as QtyIssued,
            sd.HVResourceSetupHistoryId,
            sd.HVSetupHistoryDetailId,
            sd.ProductId,
            res.ResourceId,
            m.MaterialListItemId,
            COALESCE(rc.QtyRemoved,0) as QtyRemoved,
            CASE WHEN rc.QtyRemoved is not null THEN m.QtyRequired - rc.QtyRemoved ELSE m.QtyRequired END as NetQtyIssued,
            rc.DestinationLot as RemoveDestinationLot,
            rc.DestinationStockPoint as RemoveDestinationStockPoint,
            rc.RemovalReasonId as RemoveReasonId,
            rc.RemoveDifferenceReasonId,
            rc.SpecId as RemoveSpecId,
            rc.WorkflowStepId as RemoveStepId,
            rc.TxnDateGMT as RemoveTxnDateGMT
        from 
            SetupDetails sd
            join MaterialList m on m.ProductId = sd.ProductId and (m.SpecId is null or m.SpecId = v_SpecId)
            join HVResourceSetupHistory setup on setup.HVResourceSetupHistoryId = sd.HVResourceSetupHistoryId
            join ResourceDef res on res.ResourceId = setup.ResourceId
            left join RemovedComponents rc on                               -- function getting removed components already filters on container. then also filter by
                rc.MaterialListItemId = m.MaterialListItemId				-- bom item
                and rc.HVSetupDetailId = sd.HVSetupHistoryDetailId			-- setup detail (component mounted on the HV setup)
                and rc.HVIssueHistoryDetailId = p_HVIssueHistoryDetailId	-- HV issue txn (remove was for issue done by the specified issue txn)
    )
    LOOP
        v_results_t.extend;
        v_results_t(v_results_t.count) := csiHVIssueDetail_typ(
            issueDetail.ReferenceDesignator,
            issueDetail.CompName,
            issueDetail.ProductName,
            issueDetail.ProductRevision,
            issueDetail.FromLot,
            3, -- IssueControl fixed to Lot and Stock Point
            v_ResourceName,
            issueDetail.Slot,
            issueDetail.SubSlot,
            issueDetail.QtyIssued,
            v_WorkflowStepName,
            v_SpecName,
            v_TxnDateGMT,
            issueDetail.HVResourceSetupHistoryId,
            issueDetail.HVSetupHistoryDetailId,
            issueDetail.ProductId,
            issueDetail.ResourceId,
            v_WorkflowStepId,
            v_SpecId,
            issueDetail.MaterialListItemId,
            v_HVComponentIssueHistoryId,
            p_HVIssueHistoryDetailId,
            issueDetail.QtyRemoved,
            issueDetail.NetQtyIssued,
            issueDetail.RemoveDestinationLot,
            issueDetail.RemoveDestinationStockPoint,
            issueDetail.RemoveReasonId,
            issueDetail.RemoveDifferenceReasonId,
            issueDetail.RemoveSpecId,
            issueDetail.RemoveStepId,
            issueDetail.RemoveTxnDateGMT
        );
    END LOOP;
    
    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Gets all HV issue details for a given container
-- May return multiple rows for a ref des if component product defined in multiple slots on same resource, or on different resources

-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'csiHVContainerIssueDetail_tab', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE csiHVContainerIssueDetail_typ AS OBJECT (
	ContainerId CHAR(16),
	ContainerName VARCHAR2(100),
	ReferenceDesignator VARCHAR2(100),
	CompName VARCHAR2(32),
	ProductName VARCHAR2(100),
	ProductRevision VARCHAR2(25),
	FromLot VARCHAR2(100),
	IssueControl INT,
	ResourceName VARCHAR2(100),
	Slot INT,
	SubSlot INT,
    QtyIssued NUMBER,
	WorkflowStepName VARCHAR2(100),
	SpecName VARCHAR2(100),
	TxnDateGMT DATE,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved NUMBER,
	NetQtyIssued NUMBER,
	RemoveDestinationLot VARCHAR2(100),
	RemoveDestinationStockPoint VARCHAR2(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATE    -- 33 fields
)
/
CREATE OR REPLACE TYPE csiHVContainerIssueDetail_tab AS TABLE OF csiHVContainerIssueDetail_typ
/
CREATE OR REPLACE FUNCTION csiGetAllHVIssuesForContainer(
    p_ContainerId IN VARCHAR2,
    p_ContainerName IN VARCHAR2,
	p_ContainerOption INT
)
RETURN csiHVContainerIssueDetail_tab
AS
    v_results_t csiHVContainerIssueDetail_tab := csiHVContainerIssueDetail_tab();

	v_HVIssueHistoryDetailId CHAR(16);
    v_ContainerId CHAR(16);
	v_ContainerName VARCHAR2(100);
	v_ChildCount INTEGER;
	v_ContainerStatus INTEGER;
    v_ParentContainerId CHAR(16);
    
	v_GetSingle INT;
	v_GetByParent INT;
	v_GetByChildren INT;

	c_ChildrenOrSingle CONSTANT INT := 0;
	c_Siblings CONSTANT INT := 1;
	c_Single CONSTANT INT := 2;
BEGIN
    -- get info about specified container
    IF p_ContainerId is not null THEN
    	select ContainerId, ContainerName, ChildCount, Status, ParentContainerId 
        into v_ContainerId, v_ContainerName, v_ChildCount, v_ContainerStatus, v_ParentContainerId 
        from Container where ContainerId = p_ContainerId;
    ELSE
    	select ContainerId, ContainerName, ChildCount, Status, ParentContainerId 
        into v_ContainerId, v_ContainerName, v_ChildCount, v_ContainerStatus, v_ParentContainerId 
        from Container where ContainerName = p_ContainerName;
    END IF;
    
	-- based on options param and container info, determine how to load issue details
	IF(p_ContainerOption = c_Single or (p_ContainerOption = c_Siblings and v_ChildCount = 0 and v_ParentContainerId is null) or (p_ContainerOption = c_ChildrenOrSingle and v_ChildCount = 0)) THEN
		v_GetSingle := 1; 
	ELSIF(p_ContainerOption = c_Siblings and v_ChildCount = 0 and v_ParentContainerId is not null) THEN
		v_GetByParent := 1;  
	ELSIF((p_ContainerOption = c_Siblings or p_ContainerOption = c_ChildrenOrSingle) and v_ChildCount > 0) THEN
		v_GetByChildren := 1;
    END IF;
    
    -- load the issue details
    IF(v_GetSingle = 1 and v_ContainerStatus > 0) THEN
        -- get issue details for only the specified container
        FOR hvIssueHistoryDetail IN (
            select HVIssueHistoryDetailId from HVIssueHistoryDetail where ContainerId = v_ContainerId 
        )
        LOOP
            FOR issueDetail IN (
                select v_ContainerId as ContainerId, v_ContainerName as ContainerName, details.* from TABLE(csiGetDetailsForSingleHVIssue(hvIssueHistoryDetail.HVIssueHistoryDetailId)) details
            )
            LOOP
                v_results_t.extend;
                v_results_t(v_results_t.count) := csiHVContainerIssueDetail_typ(
                    issueDetail.ContainerId,
                    issueDetail.ContainerName,
                    issueDetail.ReferenceDesignator,
                    issueDetail.CompName,
                    issueDetail.ProductName,
                    issueDetail.ProductRevision,
                    issueDetail.FromLot,
                    issueDetail.IssueControl,
                    issueDetail.ResourceName,
                    issueDetail.Slot,
                    issueDetail.SubSlot,
                    issueDetail.QtyIssued,
                    issueDetail.WorkflowStepName,
                    issueDetail.SpecName,
                    issueDetail.TxnDateGMT,
                    issueDetail.HVResourceSetupHistoryId,
                    issueDetail.HVSetupHistoryDetailId,
                    issueDetail.ProductId,
                    issueDetail.ResourceId,
                    issueDetail.WorkflowStepId,
                    issueDetail.SpecId,
                    issueDetail.MaterialListItemId,
                    issueDetail.HVComponentIssueHistoryId,
                    issueDetail.HVIssueHistoryDetailId,
                    issueDetail.QtyRemoved,
                    issueDetail.NetQtyIssued,
                    issueDetail.RemoveDestinationLot,
                    issueDetail.RemoveDestinationStockPoint,
                    issueDetail.RemoveReasonId,
                    issueDetail.RemoveDifferenceReasonId,
                    issueDetail.RemoveSpecId,
                    issueDetail.RemoveStepId,
                    issueDetail.RemoveTxnDateGMT    -- 33 fields
                );
            END LOOP;
        END LOOP;
    ELSIF(v_GetByParent = 1) THEN
        -- specified container is a child, get issue details for all with same parent
        FOR childContainer IN (
            select ContainerId from Container where ParentContainerId = v_ParentContainerId
        )
        LOOP
            FOR issueDetail IN (
                select * from TABLE(csiGetAllHVIssuesForContainer(childContainer.ContainerId, null, c_Single))
            )
            LOOP
                v_results_t.extend;
                v_results_t(v_results_t.count) := csiHVContainerIssueDetail_typ(
                    issueDetail.ContainerId,
                    issueDetail.ContainerName,
                    issueDetail.ReferenceDesignator,
                    issueDetail.CompName,
                    issueDetail.ProductName,
                    issueDetail.ProductRevision,
                    issueDetail.FromLot,
                    issueDetail.IssueControl,
                    issueDetail.ResourceName,
                    issueDetail.Slot,
                    issueDetail.SubSlot,
                    issueDetail.QtyIssued,
                    issueDetail.WorkflowStepName,
                    issueDetail.SpecName,
                    issueDetail.TxnDateGMT,
                    issueDetail.HVResourceSetupHistoryId,
                    issueDetail.HVSetupHistoryDetailId,
                    issueDetail.ProductId,
                    issueDetail.ResourceId,
                    issueDetail.WorkflowStepId,
                    issueDetail.SpecId,
                    issueDetail.MaterialListItemId,
                    issueDetail.HVComponentIssueHistoryId,
                    issueDetail.HVIssueHistoryDetailId,
                    issueDetail.QtyRemoved,
                    issueDetail.NetQtyIssued,
                    issueDetail.RemoveDestinationLot,
                    issueDetail.RemoveDestinationStockPoint,
                    issueDetail.RemoveReasonId,
                    issueDetail.RemoveDifferenceReasonId,
                    issueDetail.RemoveSpecId,
                    issueDetail.RemoveStepId,
                    issueDetail.RemoveTxnDateGMT    -- 33 fields
                );
            END LOOP;
        END LOOP;
    ELSIF(v_GetByChildren = 1) THEN
		-- specified container is a parent, get issue details for all children
        FOR childContainer IN (
            select ContainerId from Container where ParentContainerId = v_ContainerId
        )
        LOOP
            FOR issueDetail IN (
                select * from TABLE(csiGetAllHVIssuesForContainer(childContainer.ContainerId, null, c_Single))
            )
            LOOP
                v_results_t.extend;
                v_results_t(v_results_t.count) := csiHVContainerIssueDetail_typ(
                    issueDetail.ContainerId,
                    issueDetail.ContainerName,
                    issueDetail.ReferenceDesignator,
                    issueDetail.CompName,
                    issueDetail.ProductName,
                    issueDetail.ProductRevision,
                    issueDetail.FromLot,
                    issueDetail.IssueControl,
                    issueDetail.ResourceName,
                    issueDetail.Slot,
                    issueDetail.SubSlot,
                    issueDetail.QtyIssued,
                    issueDetail.WorkflowStepName,
                    issueDetail.SpecName,
                    issueDetail.TxnDateGMT,
                    issueDetail.HVResourceSetupHistoryId,
                    issueDetail.HVSetupHistoryDetailId,
                    issueDetail.ProductId,
                    issueDetail.ResourceId,
                    issueDetail.WorkflowStepId,
                    issueDetail.SpecId,
                    issueDetail.MaterialListItemId,
                    issueDetail.HVComponentIssueHistoryId,
                    issueDetail.HVIssueHistoryDetailId,
                    issueDetail.QtyRemoved,
                    issueDetail.NetQtyIssued,
                    issueDetail.RemoveDestinationLot,
                    issueDetail.RemoveDestinationStockPoint,
                    issueDetail.RemoveReasonId,
                    issueDetail.RemoveDifferenceReasonId,
                    issueDetail.RemoveSpecId,
                    issueDetail.RemoveStepId,
                    issueDetail.RemoveTxnDateGMT    -- 33 fields
                );
            END LOOP;
        END LOOP;
    END IF;

    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Gets all high volume issue details for a given container
-- but returns only one row per container/materialListItem(refDes) combination

-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION csiGetHVIssuesForContainer(
    p_ContainerId IN VARCHAR2,
    p_ContainerName IN VARCHAR2,
	p_ContainerOption INT
)
RETURN csiHVContainerIssueDetail_tab
AS
    v_results_t csiHVContainerIssueDetail_tab := csiHVContainerIssueDetail_tab();

    v_LastConName VARCHAR2(100);
    v_LastMatItemId CHAR(16);
BEGIN
    v_LastConName := '';    
    v_LastMatItemId := '';
    
    FOR containerIssue IN (
        select * from table(csiGetAllHVIssuesForContainer(p_ContainerId, p_ContainerName, p_ContainerOption)) 
        order by ContainerName, MaterialListItemId, TxnDateGMT desc, HVIssueHistoryDetailId
    )
    LOOP
        IF v_LastConName <> containerIssue.ContainerName or v_LastMatItemId <> containerIssue.MaterialListItemId THEN
            v_results_t.extend;
            v_results_t(v_results_t.count) := csiHVContainerIssueDetail_typ(
                containerIssue.ContainerId,
                containerIssue.ContainerName,
                containerIssue.ReferenceDesignator,
                containerIssue.CompName,
                containerIssue.ProductName,
                containerIssue.ProductRevision,
                containerIssue.FromLot,
                containerIssue.IssueControl,
                containerIssue.ResourceName,
                containerIssue.Slot,
                containerIssue.SubSlot,
                containerIssue.QtyIssued,
                containerIssue.WorkflowStepName,
                containerIssue.SpecName,
                containerIssue.TxnDateGMT,
                containerIssue.HVResourceSetupHistoryId,
                containerIssue.HVSetupHistoryDetailId,
                containerIssue.ProductId,
                containerIssue.ResourceId,
                containerIssue.WorkflowStepId,
                containerIssue.SpecId,
                containerIssue.MaterialListItemId,
                containerIssue.HVComponentIssueHistoryId,
                containerIssue.HVIssueHistoryDetailId,
                containerIssue.QtyRemoved,
                containerIssue.NetQtyIssued,
                containerIssue.RemoveDestinationLot,
                containerIssue.RemoveDestinationStockPoint,
                containerIssue.RemoveReasonId,
                containerIssue.RemoveDifferenceReasonId,
                containerIssue.RemoveSpecId,
                containerIssue.RemoveStepId,
                containerIssue.RemoveTxnDateGMT    
            );
        END IF;
        v_LastConName := containerIssue.ContainerName;
        v_LastMatItemId := containerIssue.MaterialListItemId;
    END LOOP;

    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- Get all machine setup details for HV issues to a given container and material list item(Ref Des)
-- Purpose is to get the setup details that could have issued components.
-- Later processing adds history records that indicate components issued to the container from the setup have been removed.
-- This allows for excluding those components from queries to see what was issued, or included in other queries to get what has been removed.
-------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION csiGetHVRemoveSetupDetails(
    p_ContainerId IN VARCHAR2,
    p_ContainerName IN VARCHAR2,
	p_ContainerOption INT,
    p_MaterialListItemId VARCHAR2
)
RETURN csiHVContainerIssueDetail_tab
AS
    v_results_t csiHVContainerIssueDetail_tab := csiHVContainerIssueDetail_tab();
BEGIN
    
    FOR containerIssue IN (
        select * from TABLE(csiGetAllHVIssuesForContainer(p_ContainerId, p_ContainerName, p_ContainerOption))
        where MaterialListItemId = p_MaterialListItemId
    )
    LOOP
        v_results_t.extend;
        v_results_t(v_results_t.count) := csiHVContainerIssueDetail_typ(
            containerIssue.ContainerId,
            containerIssue.ContainerName,
            containerIssue.ReferenceDesignator,
            containerIssue.CompName,
            containerIssue.ProductName,
            containerIssue.ProductRevision,
            containerIssue.FromLot,
            containerIssue.IssueControl,
            containerIssue.ResourceName,
            containerIssue.Slot,
            containerIssue.SubSlot,
            containerIssue.QtyIssued,
            containerIssue.WorkflowStepName,
            containerIssue.SpecName,
            containerIssue.TxnDateGMT,
            containerIssue.HVResourceSetupHistoryId,
            containerIssue.HVSetupHistoryDetailId,
            containerIssue.ProductId,
            containerIssue.ResourceId,
            containerIssue.WorkflowStepId,
            containerIssue.SpecId,
            containerIssue.MaterialListItemId,
            containerIssue.HVComponentIssueHistoryId,
            containerIssue.HVIssueHistoryDetailId,
            containerIssue.QtyRemoved,
            containerIssue.NetQtyIssued,
            containerIssue.RemoveDestinationLot,
            containerIssue.RemoveDestinationStockPoint,
            containerIssue.RemoveReasonId,
            containerIssue.RemoveDifferenceReasonId,
            containerIssue.RemoveSpecId,
            containerIssue.RemoveStepId,
            containerIssue.RemoveTxnDateGMT    
        );
    END LOOP;

    RETURN (v_results_t);
END;
/
-------------------------------------------------------------------------------
-- procedure to increase length
-- 02/05/2025     Dan Maloney              Added NO_DATA_FOUND exception to csiIncreaseStringColMaxLength BUG 458744 and updated copyright to 2025
--                                                        Changed variable name from v_CurrentMaxLength INT to i_CurrentMaxLength INT for more clarity
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiIncreaseStringColMaxLength(
	p_TableName IN VARCHAR2, 
    p_ColName IN VARCHAR2, 
    p_NewMaxLength IN INT
)
IS 
	i_CurrentMaxLength INT;
	v_AlterSQL VARCHAR2(1000);
	v_Message VARCHAR2(1000);
	v_CurrentMaxLengthStr VARCHAR2(10);
	v_NewMaxLengthStr VARCHAR2(10);
BEGIN
    select data_length into i_CurrentMaxLength from user_tab_columns where table_name = UPPER(p_TableName) and column_name = UPPER(p_ColName);
    v_CurrentMaxLengthStr := CAST(i_CurrentMaxLength as VARCHAR2);
    v_NewMaxLengthStr := CAST(p_NewMaxLength as VARCHAR2);
    
	IF i_CurrentMaxLength < p_NewMaxLength THEN
		v_AlterSQL := 'alter table ' || p_TableName || ' modify ' || p_ColName || ' VARCHAR2(' || v_NewMaxLengthStr || ')'; 
		execute immediate v_AlterSQL;
    END IF;
EXCEPTION
WHEN NO_DATA_FOUND THEN
	NULL; --Do nothing if the table name and column name are not found in USER_TAB_COLUMNS
WHEN OTHERS THEN
	RAISE; -- Raise error to caller on any other unhandled error
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate NDO given its Table Name, Value and its Name field's Column Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidNDO(
    pTableName  IN VARCHAR2,
    pFieldValue IN VARCHAR2,
    pDBNameColumnName IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
	vCount NUMBER :=0;
    vSql_stmt VARCHAR2(2000);
BEGIN
    vSql_stmt := 'select count(*) from '|| pTableName || ' where UPPER(' || pDBNameColumnName || ') = UPPER( '''|| pFieldValue || ''' )';
    EXECUTE IMMEDIATE vSql_stmt INTO vCount ;  

	IF(vCount = 0) THEN
        oValidateFail := true;
    END IF;
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate RDO given its Table Name, Value, Revision, Name field's Column Name, Base Id Column Name and Revision Column Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidRDO(
    pTableName  IN VARCHAR2,
    pFieldValue IN VARCHAR2,
    pFieldRevision IN VARCHAR2,
    pDBNameColumnName IN VARCHAR2,
    pDBBaseIdColName IN VARCHAR2,
    pDBRevisionColName IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
	vCount NUMBER :=0;
    vSql_stmt VARCHAR2(2000); 
BEGIN

    vSql_stmt := 'Select count(*) from ' || pTableName || ' PS join ' || pTableName || 'Base PSB on PS.' || pDBBaseIdColName || ' = PSB.' 
                    || pDBBaseIdColName || ' where PSB.' || pDBNameColumnName || '= '''|| pFieldValue ||''' and PS.'|| pDBRevisionColName 
                    || ' = '''|| pFieldRevision ||'''';
    EXECUTE IMMEDIATE vSql_stmt INTO vCount ;  

	IF(vCount = 0) THEN
        oValidateFail := true;
    END IF;

END;
/
-------------------------------------------------------------------------------
-- Procedure to validate a String given its value and length
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidStrLength(
    pPrecisionValue IN INTEGER,
    pFieldValue IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
	vLength INTEGER :=0;
    vSql_stmt VARCHAR2(2000);
BEGIN

    vSql_stmt := 'select LENGTH ( '''|| pFieldValue ||''' ) from dual';
    EXECUTE IMMEDIATE vSql_stmt INTO vLength ; 
   
	IF(vLength > pPrecisionValue) THEN
        oValidateFail := True;
    END IF;
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate a Boolean value given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidBoolean(
    pFieldValue IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
	vLength INTEGER :=0;
BEGIN
	
    oValidateFail := True;
    IF( UPPER(TRIM(pFieldValue)) = 'TRUE' OR pFieldValue = '1' OR UPPER(TRIM(pFieldValue)) = 'FALSE' OR pFieldValue = '0' ) THEN
        oValidateFail := False;
    END IF;
    
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate a Number given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidNumber(
    pFieldValue IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
    vSql_stmt VARCHAR2(2000);
    vIsNumber BOOLEAN;
BEGIN

    vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| pFieldValue ||''' AS NUMBER) from dual';
    EXECUTE IMMEDIATE vSql_stmt INTO vIsNumber ;  
   
	IF(NOT vIsNumber) THEN
        oValidateFail := True;
    END IF;
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate a TimeStamp given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidTimeStamp(
    pFieldValue IN VARCHAR2,
    oValidateFail OUT BOOLEAN
)
IS
    vSql_stmt VARCHAR2(2000);
    vIsTImeStamp BOOLEAN;
BEGIN
	
		vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| pFieldValue ||''' AS TIMESTAMP, ''MM/DD/YY HH24:MI:SS'') from dual';
		EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ; 
    
		IF(NOT vIsTImeStamp) THEN
			vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| pFieldValue ||''' AS TIMESTAMP, ''MM/DD/YY HH:MI:SS AM'') from dual';
			EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ;  
		END IF;

		IF(NOT vIsTImeStamp) THEN
			vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| pFieldValue ||''' AS TIMESTAMP, ''MM/DD/YY HH:MI AM'') from dual';
			EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ;  
		END IF;
		
		IF(NOT vIsTImeStamp) THEN
			oValidateFail := True;
		END IF;

END;
/
-------------------------------------------------------------------------------
-- Procedure to validate a Location NDO that model within in Facotry of a container
-- Given Location Value and Container Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidLocation(
vFieldValue IN VARCHAR2,
vContainerName IN VARCHAR2,
oResult OUT BOOLEAN
)
IS
    vSql_stmt VARCHAR2(2000);
	vFactoryName VARCHAR2(40);
    vEmployeeName VARCHAR2(40);
    vLoginUser VARCHAR2(40);
	vLCount INTEGER;
BEGIN
	vEmployeeName := '';
    vLoginUser := 'DBCLF::LoginUser';

    vSql_stmt := 'select value from CLFParameterCache where Name = UPPER(:LoginUser) ';

	EXECUTE IMMEDIATE vSql_stmt INTO vEmployeeName USING vLoginUser;
        
	IF ( vEmployeeName = '') THEN
        vLCount := 0; 
	ELSE
        vSql_stmt := 'SELECT f.factoryname FROM Employee e JOIN sessionvalues s ON e.employeeid = s.employeeid JOIN factory f ON f.factoryid = s.factoryid WHERE employeename = :EmployeeName';

		EXECUTE IMMEDIATE vSql_stmt INTO vFactoryName USING vEmployeeName ;

        vSql_stmt := 'SELECT count(*) FROM Factory F INNER JOIN Location L ON F.FactoryId = L.FactoryId WHERE UPPER(F.FactoryName) = UPPER( :FactoryName ) AND UPPER(L.LocationName) = UPPER( :FieldValue )';

        EXECUTE IMMEDIATE vSql_stmt INTO vLCount USING vFactoryName,vFieldValue ;

    END IF; 

	IF(vLCount = 0) THEN
		oResult := True;
	END IF;
END;
/
-------------------------------------------------------------------------------
-- Procedure to validate data submit to Multi Lots Modify Attrs (HPE) service
-- This procedure will update the status column is a temp table to 1 if the data of a row is invalid
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilValidateMultiLotsAttrs 
AS
    vTableName  VARCHAR2(40);
	vAttributeName  VARCHAR2(40);
	vAttributeValue VARCHAR2(4000);
	vAttributeRevision VARCHAR2(40);
	vContainerName VARCHAR2(40);
	vFieldType  VARCHAR2(40);
	vPrecisionValue INTEGER;
    vDBColumnName  VARCHAR2(40);
    vDBBaseColumnName  VARCHAR2(40);
    vDBNameColumnName VARCHAR2(40);
    vDBRevisionColName VARCHAR2(40);
    vObjectId  VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vValidationFail BOOLEAN;
    vSql_stmt VARCHAR2(2000);
BEGIN   
    vQuerySQL	:= 'SELECT ContainerName,AttributeName,AttributeValue,AttributeRevision,TableName,FieldType,PrecisionValue,DBColumnName,DBBaseColumnName,DBNameColumnName,DBRevisionColName '
					|| 'FROM		MultiLotsModifyAttrsTemp ';

	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerName,vAttributeName,vAttributeValue,vAttributeRevision,vTableName,vFieldType,vPrecisionValue,vDBColumnName,vDBBaseColumnName,vDBNameColumnName, vDBRevisionColName;
	WHILE (c1%FOUND) LOOP     
		-- For each entry validate NDO and RDO record exist
        IF ( vAttributeValue IS NOT NULL ) THEN
        
            IF (vFieldType = 'NDO') THEN
                IF (vAttributeName = 'Location') THEN
                    clfutilIsValidLocation ( vAttributeValue, vContainerName, vValidationFail ) ; 
                ELSE
                    clfutilIsValidNDO ( vTableName, vAttributeValue, vDBNameColumnName, vValidationFail ) ;
                END IF;
            END IF;
    
            IF (vFieldType ='RDO') THEN
                clfutilIsValidRDO ( vTableName, vAttributeValue, vAttributeRevision, vDBNameColumnName, vDBBaseColumnName, vDBRevisionColName, vValidationFail ) ; 
            END IF;
    
            IF (vFieldType ='STRING') THEN
                clfutilIsValidStrLength (vPrecisionValue, vAttributeValue, vValidationFail); 
            END IF;
    
            IF (vFieldType ='NUMBER') THEN
                clfutilIsValidNumber (vAttributeValue, vValidationFail); 
            END IF;
    
            IF (vFieldType ='TIMESTAMP') THEN
                clfutilIsValidTimeStamp (vAttributeValue, vValidationFail); 
            END IF;
    
            IF (vFieldType ='BOOLEAN') THEN
                clfutilIsValidBoolean (vAttributeValue, vValidationFail); 
            END IF;
    
            IF(vValidationFail) THEN
                vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET ValidationFail = 1 WHERE ContainerName = :ContainerName AND AttributeName = :AttributeName';
                EXECUTE IMMEDIATE vSql_stmt USING vContainerName,vAttributeName;  
            END IF;
        
        END IF;

        FETCH c1 INTO vContainerName,vAttributeName,vAttributeValue,vAttributeRevision,vTableName,vFieldType,vPrecisionValue,vDBColumnName,vDBBaseColumnName,vDBNameColumnName, vDBRevisionColName;

	END LOOP;
	CLOSE c1;
END;
/
-------------------------------------------------------------------------------
-- Procedure to update initial data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilUpdateMultiLotsAttrsTemp
AS
    vTableName  VARCHAR2(40);
	vAttributeName  VARCHAR2(40);
	vAttributeValue VARCHAR2(4000);
	vAttributeRevision VARCHAR2(40);
	vContainerName VARCHAR2(40);
	vFieldType  VARCHAR2(40);
	vPrecisionValue INTEGER;
    vDBColumnName  VARCHAR2(40);
    vDOColumnName  VARCHAR2(40);
    vDBBaseColumnName  VARCHAR2(40);
    vDBNameColumnName VARCHAR2(40);
    vDBRevisionColName VARCHAR2(40);
    vObjectId  VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vSql_stmt VARCHAR2(2000);
    vContainerId VARCHAR2(16);
    vAttrCDOName VARCHAR2(40);
    vTempSQL VARCHAR2(200);
    vCurrentStatusId CHAR(16);
    vAttrTableName VARCHAR2(40);
BEGIN  

    vQuerySQL	:= 'SELECT ContainerName,AttributeName,AttributeValue,AttributeRevision,TableName,FieldType,PrecisionValue,'
                    || 'DBColumnName,DOColumnName,DBBaseColumnName,DBNameColumnName,DBRevisionColName,AttributeCDOName '
					|| 'FROM		MultiLotsModifyAttrsTemp ';

	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerName,vAttributeName,vAttributeValue,vAttributeRevision,vTableName,vFieldType,vPrecisionValue,
                    vDBColumnName,vDOColumnName,vDBBaseColumnName,vDBNameColumnName, vDBRevisionColName, vAttrCDOName;
	WHILE (c1%FOUND) LOOP  

        vSql_stmt := 'Select ContainerId,CurrentStatusId From Container Where ContainerName ='''|| vContainerName ||'''';

        EXECUTE IMMEDIATE vSql_stmt INTO vContainerId,vCurrentStatusId ;   

        IF (UPPER(vAttributeName) = 'LOCATION' OR UPPER(vAttributeName) = 'FACTORY') THEN
            vAttrCDOName := 'CurrentStatus' ;
            vTempSQL := ' WHERE CurrentStatusId = ''' || vCurrentStatusId;
        ELSE
            vTempSQL := ' WHERE ContainerId = ''' || vContainerId;
        END IF;
        
        vAttrTableName := vAttrCDOName;
        IF(vAttrCDOName='LotAttributes') THEN
            vAttrTableName := 'A_'||vAttrCDOName;
        END IF;

        IF(vFieldType ='NDO') THEN
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET ObjectId = ( Select '|| vDOColumnName ||' from ' || vTableName 
                            || ' where ' || vDBNameColumnName || ' = '''|| vAttributeValue ||''') Where ContainerName = :ContainerName AND AttributeName = :AttributeName';
            EXECUTE IMMEDIATE vSql_stmt USING vContainerName,vAttributeName;  
            
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET OldObjectId = ( Select FT. ' || vDBColumnName || ' FROM '|| vAttrTableName ||' FT JOIN '||vTableName||' TB ON FT. '|| vDBColumnName || '= TB.'|| vDOColumnName 
							|| vTempSQL || ''' ), OldValue = ( Select TB. ' || vDBNameColumnName || ' FROM '|| vAttrTableName ||' FT JOIN '||vTableName||' TB ON FT. '|| vDBColumnName || '= TB.'|| vDOColumnName || vTempSQL 
							|| ''' ) WHERE AttributeName = '''|| vAttributeName || ''' AND ContainerName ='''|| vContainerName ||'''';
            EXECUTE IMMEDIATE vSql_stmt;  

        ELSIF(vFieldType ='RDO') THEN
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET ObjectId = ( Select '|| vDBColumnName ||' from ' || vTableName || ' PS join ' || vTableName || 'Base PSB on PS.' || vDBBaseColumnName || ' = PSB.' 
                    || vDBBaseColumnName || ' where PSB.' || vDBNameColumnName || ' = '''|| vAttributeValue ||''' and PS.'|| vDBRevisionColName 
                    || ' = '''|| vAttributeRevision ||''') Where ContainerName = :ContainerName AND AttributeName = :AttributeName';
            EXECUTE IMMEDIATE vSql_stmt USING vContainerName,vAttributeName;  
			
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET OldObjectId = ( Select FT. '|| vDBColumnName ||' FROM '|| vAttrTableName ||' FT WHERE ContainerId = ''' || vContainerId 
				|| ''' ) , OldValue = ( Select TBB.'|| vDBNameColumnName || ' FROM '|| vAttrTableName ||' FT JOIN '||vTableName||' TB ON FT. '|| vDBColumnName || '= TB.'|| vDOColumnName 
                || ' JOIN '||vTableName||'Base TBB ON TB. '|| vDBBaseColumnName || '= TBB.'|| vDBBaseColumnName || ' WHERE ContainerId = ''' || vContainerId 
				|| ''' ), OldRevision = ( Select TB.'|| vDBRevisionColName || ' FROM '|| vAttrTableName ||' FT JOIN '||vTableName||' TB ON FT. '|| vDBColumnName || '= TB.'|| vDOColumnName 
                || ' JOIN '||vTableName||'Base TBB ON TB. '|| vDBBaseColumnName || '= TBB.'|| vDBBaseColumnName || ' WHERE ContainerId = ''' || vContainerId 
				|| ''' ) WHERE AttributeName = '''|| vAttributeName || ''' AND ContainerName ='''|| vContainerName ||'''';
            EXECUTE IMMEDIATE vSql_stmt;

        ELSIF(vAttrCDOName='LotAttributesEx') THEN
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET OldValue = ( Select DISTINCT FT.AttributeValue FROM '
                            || 'A_LotAttributesEx FT JOIN MultiLotsModifyAttrsTemp BT ON BT.ModifyAttributeSetupId  = FT.AttributeId WHERE ContainerId = ''' 
                            || vContainerId || ''' AND BT.AttributeName = '''|| vAttributeName ||''') WHERE AttributeName = '''|| vAttributeName || ''' AND ContainerName ='''|| vContainerName ||'''';
            EXECUTE IMMEDIATE vSql_stmt; 
        ELSE 
            vSql_stmt := 'UPDATE MultiLotsModifyAttrsTemp SET OldValue = ( Select FT.' || vAttributeName || ' AttributeValue FROM '
                            || vAttrTableName ||' FT ' || vTempSQL || ''') WHERE AttributeName = '''|| vAttributeName || ''' AND ContainerName ='''|| vContainerName 
                            ||'''' ;
            EXECUTE IMMEDIATE vSql_stmt;             
        END IF;
        FETCH c1 INTO vContainerName,vAttributeName,vAttributeValue,vAttributeRevision,vTableName,vFieldType,vPrecisionValue,vDBColumnName,vDOColumnName,vDBBaseColumnName,vDBNameColumnName, vDBRevisionColName,vAttrCDOName;

	END LOOP;
	CLOSE c1;
END;
/
---------------------------------------------------------------------------------------------------------
-- Procedure to update container/lotattribute/lotattributeex use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a container, currentstatus, lotattribute and lotattributeex table
---------------------------------------------------------------------------------------------------------
create or replace PROCEDURE clfutilUpdateMultiLotsValueTemp
AS
	vTableName  VARCHAR2(40);
	vContainerName  VARCHAR2(40);
	vHistoryMainline  VARCHAR2(40);
	vContainerStatusChangeHistory  VARCHAR2(40);
	vModifyAttrsHistory  VARCHAR2(40);
    vModifyAttributeSetupId VARCHAR2(40);
    vCurrentStatusId VARCHAR2(40);
    vTxnId  VARCHAR2(40);
    vContainerId  VARCHAR2(40);
    vLotAttributeId VARCHAR2(40);
    vAttributeName VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    c2 SYS_REFCURSOR;
	vQuerySQL2 VARCHAR2(4000);
    vQuerySQL3 VARCHAR2(4000);
    vQuerySQL4 VARCHAR2(4000);
	VColumnName VARCHAR2(4000);
	VColumnValue VARCHAR2(4000);
    VCDOName VARCHAR2(4000);
    VRowCount NUMBER :=0;
    vFieldType VARCHAR2(4000);
    vStringVal VARCHAR2(4000);
	vSql_stmt VARCHAR2(2000);
    vIsTImeStamp BOOLEAN;
    vLotAttributesChangeCount BOOLEAN:= TRUE;
    vCurrentStatusChangeCOunt BOOLEAN:= TRUE;

BEGIN   
    vQuerySQL	:= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, TxnId, LotAttributesId, CurrentStatusId '
					|| 'FROM		ora$ptt_MultiLotsModifyAttrsTempContainer   WHERE ContainerSequence = 1';
	
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory, vTxnID, vLotAttributeId,vCurrentStatusId;
	WHILE (c1%FOUND) LOOP
        vLotAttributesChangeCount := TRUE;
        vCurrentStatusChangeCount := TRUE;
		vQuerySQL2	:= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value, AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
					|| 'FROM		MultiLotsModifyAttrsTemp WHERE ContainerName = '''||vContainerName||''' AND IsUpdated = 1';
		Open c2	FOR vQuerySQL2;
        FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;

        WHILE (c2%FOUND) LOOP
         dbms_output.put_line(vFieldType);
            IF(vFieldType = 'TIMESTAMP') THEN
                vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YY HH24:MI:SS'') from dual';
                EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ; 
            
                IF(vIsTImeStamp) THEN
                    vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH24:MI:SS');
                ELSE 
				    vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YYYY HH:MI:SS AM'') from dual';
					EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ;
					
					IF(vIsTImeStamp) THEN
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI:SS AM');
					 ELSE 
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI AM');
					END IF;
                END IF;
            ELSE
                vStringVal := VColumnValue;
            END IF;
            IF(VCDOName ='Container') THEN
                IF (vAttributeName != 'Factory' AND vAttributeName != 'Location') THEN
                    vQuerySQL3	:= 'UPDATE CONTAINER SET '|| VColumnName ||' = '''|| vStringVal ||''', LastActivityDate = SYSDATE, LastRevTxnId = '''||vTxnID||''''
                    || 'WHERE ContainerName = :ContainerName ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                    EXECUTE IMMEDIATE vQuerySQL3 USING vContainerName;
                ELSE
                    IF(vCurrentStatusChangeCount) THEN
                        vQuerySQL3	:= 'UPDATE CurrentStatus SET '|| VColumnName ||' = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1, LastRevTxnId = '''||vTxnID||''''
                        || 'WHERE CurrentStatusId = :CurrentStatusId ';
                        vCurrentStatusChangeCount := FALSE;
                    ELSE
                        vQuerySQL3	:= 'UPDATE CurrentStatus SET '|| VColumnName ||' = '''|| vStringVal ||''', LastRevTxnId = '''||vTxnID||''''
                        || 'WHERE CurrentStatusId = :CurrentStatusId ';
                    END IF;
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                    EXECUTE IMMEDIATE vQuerySQL3 USING vCurrentStatusId;
                END IF;
            END IF;
            
            IF(VCDOName ='LotAttributes') THEN
                IF(vLotAttributesChangeCount) THEN
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1 '
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                    vLotAttributesChangeCount := FALSE;
                ELSE
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''''
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                END IF;
                DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL3 USING vLotAttributeId;
            END IF;

            
            IF(VCDOName ='LotAttributesEx') THEN
            vQuerySQL3	:= 'Select COUNT(*) FROM A_LotAttributesEx '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
            EXECUTE IMMEDIATE vQuerySQL3 INTO VRowCount USING vModifyAttributeSetupId, vContainerId;
            IF (VRowCount > 0) THEN
                vQuerySQL4	:= 'UPDATE A_LotAttributesEx SET AttributeValue = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1, LastTimestamp = SYSDATE '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL4 USING vModifyAttributeSetupId, vContainerId;
            END IF;
            END IF;
            
            FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;
        
        END LOOP;
		
		
		
		FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory,vTxnID,vLotAttributeId,vCurrentStatusId;
	END LOOP;

	CLOSE c1;
END;
/
-------------------------------------------------------------------------------
-- Procedure to update lotattributeex data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
create or replace PROCEDURE clfutilGetInsertLotAttrEx
AS
	vTableName  VARCHAR2(40);
	vContainerName  VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
	vQuerySQL2 VARCHAR2(4000);
    vQuerySQL3 VARCHAR2(4000);
	VColumnName VARCHAR2(4000);
	VColumnValue VARCHAR2(4000);
    VCDOName VARCHAR2(4000);
    VRowCount NUMBER :=0;
    vContainerId  VARCHAR2(40);
    vModifyAttributeSetupId VARCHAR2(40);
    vAttributeName VARCHAR2(40);
BEGIN   
    vQuerySQL	:= 'SELECT MT.DBColumnName, MT.AttributeValue, MT.AttributeCDOName, MT.ModifyAttributeSetupId, MT.AttributeName, C.ContainerID '
					|| 'FROM		MultiLotsModifyAttrsTemp MT LEFT JOIN CONTAINER C ON C.ContainerName = MT.ContainerName WHERE AttributeCDOName = ''LotAttributesEx'' AND IsUpdated = 1';
	
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vContainerId;
	WHILE (c1%FOUND) LOOP
		vQuerySQL2	:= 'Select COUNT(*) FROM A_LotAttributesEx '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container';
         EXECUTE IMMEDIATE vQuerySQL2 INTO VRowCount USING vModifyAttributeSetupId, vContainerId;
            IF (VRowCount = 0) THEN
                vQuerySQL3	:= 'INSERT INTO ora$ptt_MultiLotsModifyAttrsTempAttrEx (AttributeId,ContainerID,AttributeValue,AttributeName) VALUES ('''|| vModifyAttributeSetupId ||''', '''|| vContainerId ||''','''|| VColumnValue ||''','''|| vAttributeName ||''') ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL3;
            END IF;
		
		
		FETCH c1 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vContainerId;
	END LOOP;
	CLOSE c1;
END;
/
---------------------------------------------------------------------------------------------------------
-- Procedure to update container/lotattribute/lotattributeex use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a container, currentstatus, lotattribute and lotattributeex table
---------------------------------------------------------------------------------------------------------
create or replace PROCEDURE clfutilUpdateMultiLotsValueChildTemp
AS
	vTableName  VARCHAR2(40);
	vContainerName  VARCHAR2(40);
	vHistoryMainline  VARCHAR2(40);
	vContainerStatusChangeHistory  VARCHAR2(40);
	vModifyAttrsHistory  VARCHAR2(40);
    vModifyAttributeSetupId VARCHAR2(40);
    vCurrentStatusId VARCHAR2(40);
    vTxnId  VARCHAR2(40);
    vContainerId  VARCHAR2(40);
    vLotAttributeId VARCHAR2(40);
    vAttributeName VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    c2 SYS_REFCURSOR;
	vQuerySQL2 VARCHAR2(4000);
    vQuerySQL3 VARCHAR2(4000);
    vQuerySQL4 VARCHAR2(4000);
	VColumnName VARCHAR2(4000);
	VColumnValue VARCHAR2(4000);
    VCDOName VARCHAR2(4000);
    VRowCount NUMBER :=0;
    vFieldType VARCHAR2(4000);
    vStringVal VARCHAR2(4000);
	vSql_stmt VARCHAR2(2000);
    vIsTImeStamp BOOLEAN;
    vLotAttributesChangeCount BOOLEAN := TRUE;
BEGIN   
    vQuerySQL	:= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, TxnId, LotAttributesId, CurrentStatusId '
					|| 'FROM		ora$ptt_MultiLotsModifyAttrsTempChildContainer';
	
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory, vTxnID, vLotAttributeId,vCurrentStatusId;
	WHILE (c1%FOUND) LOOP
        vLotAttributesChangeCount := TRUE;
		vQuerySQL2	:= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value, AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
					|| 'FROM		MultiLotsModifyAttrsChildTemp WHERE ContainerName = '''||vContainerName||''' AND IsUpdated = 1';
		Open c2	FOR vQuerySQL2;
        FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;
        WHILE (c2%FOUND) LOOP
            IF(vFieldType = 'TIMESTAMP') THEN
                vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YY HH24:MI:SS'') from dual';
                EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ; 
            
                IF(vIsTImeStamp) THEN
                    vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH24:MI:SS');
                ELSE 
				    vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YYYY HH:MI:SS AM'') from dual';
					EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ;
					
					IF(vIsTImeStamp) THEN
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI:SS AM');
					 ELSE 
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI AM');
					END IF;
                END IF;
            ELSE
                vStringVal := VColumnValue;
            END IF;
            IF(VCDOName ='Container') THEN
                IF (vAttributeName != 'Factory' AND vAttributeName != 'Location') THEN
                    vQuerySQL3	:= 'UPDATE CONTAINER SET '|| VColumnName ||' = '''|| vStringVal ||''', LastActivityDate = SYSDATE, LastRevTxnId = '''||vTxnID||''''
                            || 'WHERE ContainerName = :ContainerName ';
                            DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                    EXECUTE IMMEDIATE vQuerySQL3 USING vContainerName;
                END IF;
            END IF;
            
            IF(VCDOName ='LotAttributes') THEN
                IF(vLotAttributesChangeCount) THEN
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1 '
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                    vLotAttributesChangeCount := FALSE;
                ELSE
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''''
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                END IF;
                DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL3 USING vLotAttributeId;
            END IF;
            
            IF(VCDOName ='LotAttributesEx') THEN
            vQuerySQL3	:= 'Select COUNT(*) FROM A_LotAttributesEx '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
            EXECUTE IMMEDIATE vQuerySQL3 INTO VRowCount USING vModifyAttributeSetupId, vContainerId;
            IF (VRowCount > 0) THEN
                vQuerySQL4	:= 'UPDATE A_LotAttributesEx SET AttributeValue = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1, LastTimestamp = SYSDATE '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL4 USING vModifyAttributeSetupId, vContainerId;
            END IF;
            END IF;
            
            FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;
        
        END LOOP;
		 
		
		
		FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory,vTxnID,vLotAttributeId,vCurrentStatusId;
	END LOOP;
	CLOSE c1;
END;
/
-------------------------------------------------------------------------------
-- Procedure to update lotattributeex data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
create or replace PROCEDURE clfutilGetInsertLotAttrExChild
AS
	vTableName  VARCHAR2(40);
	vContainerName  VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
	vQuerySQL2 VARCHAR2(4000);
    vQuerySQL3 VARCHAR2(4000);
	VColumnName VARCHAR2(4000);
	VColumnValue VARCHAR2(4000);
    VCDOName VARCHAR2(4000);
    VRowCount NUMBER :=0;
    vContainerId  VARCHAR2(40);
    vModifyAttributeSetupId VARCHAR2(40);
    vAttributeName VARCHAR2(40);
BEGIN   
    vQuerySQL	:= 'SELECT MT.DBColumnName, MT.AttributeValue, MT.AttributeCDOName, MT.ModifyAttributeSetupId, MT.AttributeName, C.ContainerID '
					|| 'FROM		MultiLotsModifyAttrsChildTemp MT LEFT JOIN CONTAINER C ON C.ContainerName = MT.ContainerName WHERE AttributeCDOName = ''LotAttributesEx'' AND IsUpdated = 1';
	
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vContainerId;
	WHILE (c1%FOUND) LOOP
		vQuerySQL2	:= 'Select COUNT(*) FROM A_LotAttributesEx '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container';
         EXECUTE IMMEDIATE vQuerySQL2 INTO VRowCount USING vModifyAttributeSetupId, vContainerId;
            IF (VRowCount = 0) THEN
                vQuerySQL3	:= 'INSERT INTO ora$ptt_MultiLotsModifyAttrsTempAttrExChild (AttributeId,ContainerID,AttributeValue,AttributeName) VALUES ('''|| vModifyAttributeSetupId ||''', '''|| vContainerId ||''','''|| VColumnValue ||''','''|| vAttributeName ||''') ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL3;
            END IF;
		
		
		FETCH c1 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vContainerId;
	END LOOP;
	CLOSE c1;
END;
/
create or replace PROCEDURE clfutilUpdateMultiLotsValueTemp02
AS
	vTableName  VARCHAR2(40);
	vContainerName  VARCHAR2(40);
	vHistoryMainline  VARCHAR2(40);
	vContainerStatusChangeHistory  VARCHAR2(40);
	vModifyAttrsHistory  VARCHAR2(40);
    vModifyAttributeSetupId VARCHAR2(40);
    vCurrentStatusId VARCHAR2(40);
    vTxnId  VARCHAR2(40);
    vContainerId  VARCHAR2(40);
    vLotAttributeId VARCHAR2(40);
    vAttributeName VARCHAR2(40);
	vQuerySQL VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    c2 SYS_REFCURSOR;
	vQuerySQL2 VARCHAR2(4000);
    vQuerySQL3 VARCHAR2(4000);
    vQuerySQL4 VARCHAR2(4000);
	VColumnName VARCHAR2(4000);
	VColumnValue VARCHAR2(4000);
    VCDOName VARCHAR2(4000);
    VRowCount NUMBER :=0;
    vFieldType VARCHAR2(4000);
    vStringVal VARCHAR2(4000);
	vSql_stmt VARCHAR2(2000);
    vIsTImeStamp BOOLEAN;
    vLotAttributesChangeCount BOOLEAN := TRUE;
    vCurrentStatusChangeCount BOOLEAN := TRUE;
BEGIN   
    vQuerySQL	:= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, TxnId, LotAttributesId, CurrentStatusId '
					|| 'FROM		ora$ptt_MultiLotsModifyAttrsTempContainer   WHERE ContainerSequence = 0';
	
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory, vTxnID, vLotAttributeId,vCurrentStatusId;
	WHILE (c1%FOUND) LOOP
        vLotAttributesChangeCount := TRUE;
        vCurrentStatusChangeCount := TRUE;
		vQuerySQL2	:= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value, AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
					|| 'FROM		MultiLotsModifyAttrsTemp WHERE ContainerName = '''||vContainerName||''' AND IsUpdated = 1';
		Open c2	FOR vQuerySQL2;
        FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;
        WHILE (c2%FOUND) LOOP
         dbms_output.put_line(vFieldType);
            IF(vFieldType = 'TIMESTAMP') THEN
                vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YY HH24:MI:SS'') from dual';
                EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ; 
            
                IF(vIsTImeStamp) THEN
                    vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH24:MI:SS');
                ELSE 
				    vSql_stmt := 'Select VALIDATE_CONVERSION( '''|| VColumnValue ||''' AS TIMESTAMP, ''MM/DD/YYYY HH:MI:SS AM'') from dual';
					EXECUTE IMMEDIATE vSql_stmt INTO vIsTImeStamp ;
					
					IF(vIsTImeStamp) THEN
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI:SS AM');
					 ELSE 
						vStringVal := TO_DATE(VColumnValue, 'MM/DD/YYYY HH:MI AM');
					END IF;
                END IF;
            ELSE
                vStringVal := VColumnValue;
            END IF;
            IF(VCDOName ='Container') THEN
                IF (vAttributeName != 'Factory' AND vAttributeName != 'Location') THEN
                    vQuerySQL3	:= 'UPDATE CONTAINER SET '|| VColumnName ||' = '''|| vStringVal ||''', LastActivityDate = SYSDATE, LastRevTxnId = '''||vTxnID||''''
                    || 'WHERE ContainerName = :ContainerName ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                    EXECUTE IMMEDIATE vQuerySQL3 USING vContainerName;
                ELSE
                     IF(vCurrentStatusChangeCount) THEN
                        vQuerySQL3	:= 'UPDATE CurrentStatus SET '|| VColumnName ||' = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1, LastRevTxnId = '''||vTxnID||''''
                        || 'WHERE CurrentStatusId = :CurrentStatusId ';
                        vCurrentStatusChangeCount := FALSE;
                    ELSE
                        vQuerySQL3	:= 'UPDATE CurrentStatus SET '|| VColumnName ||' = '''|| vStringVal ||''', LastRevTxnId = '''||vTxnID||''''
                        || 'WHERE CurrentStatusId = :CurrentStatusId ';
                    END IF;
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                    EXECUTE IMMEDIATE vQuerySQL3 USING vCurrentStatusId;
                END IF;
            END IF;
            
            IF(VCDOName ='LotAttributes') THEN
                IF(vLotAttributesChangeCount) THEN
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1 '
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                    vLotAttributesChangeCount := FALSE;
                ELSE
                    vQuerySQL3	:= 'UPDATE A_LotAttributes SET '|| VColumnName ||' = '''|| vStringVal ||''''
					|| 'WHERE LotAttributesId = :LotAttributesId ';
                END IF;
                DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL3 USING vLotAttributeId;
            END IF;
            
            IF(VCDOName ='LotAttributesEx') THEN
            vQuerySQL3	:= 'Select COUNT(*) FROM A_LotAttributesEx '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
            EXECUTE IMMEDIATE vQuerySQL3 INTO VRowCount USING vModifyAttributeSetupId, vContainerId;
            IF (VRowCount > 0) THEN
                vQuerySQL4	:= 'UPDATE A_LotAttributesEx SET AttributeValue = '''|| vStringVal ||''', ChangeCount = ChangeCount + 1, LastTimestamp = SYSDATE '
					|| 'WHERE AttributeId = :LotAttributesId AND ContainerId = :Container ';
                    DBMS_OUTPUT.PUT_LINE('vSql_stmt: '||vQuerySQL3);
                EXECUTE IMMEDIATE vQuerySQL4 USING vModifyAttributeSetupId, vContainerId;
            END IF;
            END IF;
            
            FETCH c2 INTO VColumnName,VColumnValue,VCDOName,vModifyAttributeSetupId,vAttributeName,vFieldType;
        
        END LOOP;
		
		
		
		FETCH c1 INTO vContainerId,vContainerName,vHistoryMainline,vContainerStatusChangeHistory,vModifyAttrsHistory,vTxnID,vLotAttributeId,vCurrentStatusId;
	END LOOP;

	CLOSE c1;
END;
/
---------------------------------------------------------------------------------------------------------
-- Procedure to insert pre-build attribute details to a general temp that use by Multi Lots Modify Attrs (HPE) service
-- This procedure will flatten the long string in table BaseAttrTemp and insert to ora$ptt_MultiLotsModifyAttrsTemp
---------------------------------------------------------------------------------------------------------

CREATE OR REPLACE PROCEDURE clfutilInsertSplitTempData (
     vDelimiter IN CHAR
    )
AS
		vContainerName VARCHAR2(40);
		vAttributeName CLOB;
		vAttributeValue CLOB;
		vAttributeRevision CLOB;
		vAttributeNameStr CLOB;
		vAttributeValueStr CLOB;
		vAttributeRevisionStr CLOB;
        vApplyToChildLots INT(1);
        vpos1 INT := 0;
		vpos2 INT := 0;
		vpos3 INT := 0;
		vlen INT := 0;
        vQuerySQL VARCHAR2(2000);
		vDelLength INT := 0;
        c1 SYS_REFCURSOR;
BEGIN
	vQuerySQL := 'select ContainerName, AttributeName, AttributeValue, AttributeRevision, ApplyToChildLots from BaseAttrTemp';
	OPEN c1 FOR vQuerySQL;
	FETCH c1 INTO vContainerName, vAttributeNameStr, vAttributeValueStr, vAttributeRevisionStr, vApplyToChildLots;
	WHILE (c1%FOUND) LOOP    
			vDelLength := LENGTH(vDelimiter);
			vpos1 := 1;
			vpos2 := 1;
			vpos3 := 1;
			vlen := 0;
			vAttributeNameStr := vAttributeNameStr || vDelimiter;
			vAttributeValueStr := vAttributeValueStr || vDelimiter;
			vAttributeRevisionStr := vAttributeRevisionStr || vDelimiter;   

			WHILE INSTR(vAttributeNameStr,vDelimiter, vpos1) > 0 LOOP

					vlen := CASE WHEN INSTR(vAttributeNameStr, vDelimiter, vpos1) - (vpos1)  <= 0  THEN 0
								ELSE INSTR(vAttributeNameStr, vDelimiter, vpos1)  - vpos1  END;
					vAttributeName  := SUBSTR(vAttributeNameStr, vpos1, vlen);
					vpos1 := INSTR(vAttributeNameStr, vDelimiter, vpos1 + vlen) + vDelLength ;
                    
					vlen := CASE WHEN INSTR(vAttributeValueStr, vDelimiter, vpos2) - (vpos2) <= 0  THEN 0
								ELSE INSTR(vAttributeValueStr, vDelimiter, vpos2)  - vpos2  END;					
					vAttributeValue  := SUBSTR(vAttributeValueStr, vpos2, vlen);
					vpos2 := INSTR(vAttributeValueStr, vDelimiter, vpos2 + vlen) + vDelLength ;
					
					vlen := CASE WHEN INSTR(vAttributeRevisionStr, vDelimiter, vpos3) - (vpos3) <= 0  THEN 0
								ELSE INSTR(vAttributeRevisionStr, vDelimiter, vpos3)  - vpos3 END;
					vAttributeRevision := SUBSTR(vAttributeRevisionStr, vpos3, vlen);
					vpos3 := INSTR(vAttributeRevisionStr,vDelimiter, vpos3 + vlen) + vDelLength ;    

					vQuerySQL := 'INSERT INTO MultiLotsModifyAttrsTemp (ContainerName,AttributeName,AttributeValue,AttributeRevision,ApplyToChildLots)
					SELECT LTRIM(RTRIM(:vContainerName)) As Column1,LTRIM(RTRIM(:vAttributeName)) As Column2,
                    LTRIM(RTRIM(:vAttributeValue)) As Column3,LTRIM(RTRIM(:vAttributeRevision)) As Column4,:vApplyToChildLots As Column5 FROM DUAL';
                    
                    EXECUTE IMMEDIATE vQuerySQL USING vContainerName,vAttributeName,vAttributeValue,vAttributeRevision, vApplyToChildLots ; 
                    
				END LOOP;
			FETCH c1 INTO vContainerName, vAttributeNameStr, vAttributeValueStr, vAttributeRevisionStr, vApplyToChildLots;
		END LOOP;
        CLOSE c1;
END;
/

-------------------------------------------------------------------------------
-- Recursively check a workflow for any step requiring a Pre-Production Procedure
-------------------------------------------------------------------------------
BEGIN
	DROP_DATABASE_OBJECT ( 'otab_PreProductionProcedureRequired', 'TYPE' );
END;
/
CREATE OR REPLACE 
TYPE otyp_PreProductionProcedureRequired AS OBJECT (RequirePreProc INT)
/
CREATE OR REPLACE 
TYPE otab_PreProductionProcedureRequired AS TABLE OF otyp_PreProductionProcedureRequired
/
CREATE OR REPLACE 
FUNCTION csiGetPreProductionProcedureRequired( p_WorkflowId VARCHAR2 )
RETURN otab_PreProductionProcedureRequired
AS
    v_results_t otab_PreProductionProcedureRequired := otab_PreProductionProcedureRequired();    
    
	v_RequirePreProc INT;
BEGIN
    v_RequirePreProc := 0;
    
    FOR step IN(
        select step.StepType, subFlow.WorkflowId, spec.RequirePreProductionProcedure
        from WorkflowStep step
        left join Spec spec on ( step.SpecId is not null and (
            (spec.SpecId = step.SpecId) or
            (step.SpecBaseId <> '0000000000000000' and spec.SpecId in (select RevOfRcdId from SpecBase where SpecBaseId = step.SpecBaseId))))
        left join SpecBase sb on sb.SpecBaseId = spec.SpecBaseId
        left join Workflow subFlow on ( step.SubWorkflowId is not null and (
            (subFlow.WorkflowId = step.SubWorkflowId) or
            (step.SubWorkflowBaseId <> '0000000000000000' and subFlow.WorkflowId in (select RevOfRcdId from WorkflowBase where WorkflowBaseId = step.SubWorkflowBaseId))))
        where step.WorkflowId = p_WorkflowId
    )
    LOOP
        IF step.StepType = 1 THEN
            -- is a spec step
            v_RequirePreProc := step.RequirePreProductionProcedure;
        ELSE
            -- is a sub-workflow step
            select RequirePreProc into v_RequirePreProc from TABLE(csiGetPreProductionProcedureRequired(step.WorkflowId));
        END IF;
        
        EXIT WHEN v_RequirePreProc = 1;
    END LOOP;
    
    v_results_t.extend();
    v_results_t(v_results_t.count) := otyp_PreProductionProcedureRequired(v_RequirePreProc);
    RETURN (v_results_t);   
END;
/
-------------------------------------------------------------------------------
-- RTYPercent (Summary Table Def)
-------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION GET_RTYPERCENT
RETURN Number
AS
FLT_RTYPERCENT Number;
SQL_TEXT VARCHAR2(200);

BEGIN
	SQL_TEXT := 'SELECT NVL(RTYPercent, 0) FROM csiTbl_ESRTYSUMMARY ORDER BY "Timestamp" DESC FETCH FIRST 1 ROWS ONLY';
	EXECUTE IMMEDIATE SQL_TEXT INTO FLT_RTYPERCENT;
	RETURN FLT_RTYPERCENT;
EXCEPTION
  WHEN OTHERS THEN
	RETURN 0;
END;
/

-----------------------------------------------------------------------------------------
-- PROCEDURE: csiIssuedHierarchy
-- DESCR: New Function to return data for Oracle Procedure
-- Execute the function as last row to pull the data into the report getting generated 
-----------------------------------------------------------------------------------------
create or replace function get_issuedHierarchy(pLotOrContainerName varchar2)
return csiIssuedHierarchy_data.tab_IssuedHierarchy pipelined
as
pragma autonomous_transaction;
begin
	csiGetIssuedHierarchy(pLotOrContainerName);
	for v_rec in (select * from ISSUEDHIERARCHY ORDER BY TopLevelContainerName, RecurseLevel) 
	loop
		pipe row(v_rec);
	end loop;
 
end;
/
