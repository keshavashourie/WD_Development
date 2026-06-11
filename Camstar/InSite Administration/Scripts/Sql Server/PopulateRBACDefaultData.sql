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
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCreateGUID' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCreateGUID
GO
CREATE PROCEDURE csiCreateGUID(@PermissionName VARCHAR(255),@RoleGID varchar(36) OUTPUT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

select @RoleGID =	
 (      SubString(ExportImportKeyGUID, 1, 8) +
'-' + SubString(ExportImportKeyGUID, 9, 4) +
'-' + SubString(ExportImportKeyGUID, 13, 4) +
'-' + SubString(ExportImportKeyGUID, 17, 4) +
'-' + SubString(ExportImportKeyGUID, 21,12))
from (
select upper(convert(nvarchar(36),hashbytes('MD5',@PermissionName),2)) as ExportImportKeyGUID
) c;

print @PermissionName + 'perm'
print @RoleGID + 'RoleGID'

END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Helper function to create Role record
--
--  Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreateRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreateRole
GO
CREATE PROCEDURE csiRBACCreateRole(@RoleName NVARCHAR(50), @RoleDescription NVARCHAR(255), @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleCDODefId INT
    SET @RoleCDODefId=7130

    EXEC csiPRDGetNextInstanceId @RoleCDODefId,@InstanceId OUTPUT
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignRole
GO
CREATE PROCEDURE csiRBACAssignRole(@RoleId CHAR(16), @RoleDescription NVARCHAR(255), @EmployeeName NVARCHAR(255), @OrganizationName NVARCHAR(255), @Propagate INT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
     DECLARE @RoleGID NVARCHAR(36)


BEGIN
    SET NOCOUNT ON;

    SELECT @EmployeeId=EmployeeId
    FROM Employee
    WHERE EmployeeName=@EmployeeName

    SET @OrgId=NULL
    IF NOT @OrganizationName IS NULL
        SELECT @OrgId=OrganizationId
        FROM Organization
        WHERE OrganizationName=@OrganizationName

    SET @CDODefId=7782

      EXEC csiPRDGetNextInstanceId @CDODefId,@IID OUTPUT
   set @RoleDescription = @RoleDescription + @EmployeeName
    EXEC csiCreateGUID @RoleDescription, @RoleGID OUTPUT
    INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (@RoleGID,@IID, @CDODefId, @RoleId, @EmployeeId, 0, @Propagate, @OrgId);
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermission' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermission
GO
CREATE PROCEDURE csiRBACCreatePermission(@RoleId CHAR(16), @PermissionName VARCHAR(255), @PermissionType INT, @ObjectMetaId INT, @PermissionModesFlag INT, @ObjectInstanceId CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    DECLARE @IID VARCHAR(16)
	 DECLARE @RoleGID VARCHAR(36)
BEGIN
    SET NOCOUNT ON;

    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
    set @PermissionName=LTRIM(RTRIM(@PermissionName))
    EXEC csiCreateGUID @PermissionName, @RoleGID OUTPUT
    print @RoleGID + 'RoleGID'
    
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(@RoleGID,@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the @PermissionModesFlag value
	IF ( @PermissionModesFlag = 0 )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
    Else IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180) )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
          AND BitNumber = 2
    ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType = 180 )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
          AND BitNumber IN (1,2,3,4)
	ELSE
		PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermissionsForQry' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermissionsForQry
GO
CREATE PROCEDURE csiRBACCreatePermissionsForQry(@RoleId CHAR(16), @SecurityList NVARCHAR(MAX), @PermissionModesFlag INT)
AS
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
    DECLARE @ObjectMetaId INT
    DECLARE @PermissionType INT
    DECLARE @PermissionName VARCHAR(255)
BEGIN
   SET NOCOUNT ON;
   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 'Select DISTINCT CDO.CDODefId As ObjectMetaId ' +
								', CDO.SecurityTypeId as PermissionType ' +
								', Labels.LabelValue as PermissionName ' +
						  'From	CDODefinition CDO ' +
						  '		, Labels ' +
						  'Where	Labels.LabelId = CDO.DisplayNameLabelId ' +
						     'And		CDO.IsAbstract = 0 ' +
						     'And		CDO.SecurityTypeId In ('+ @SecurityList +' ) ' +
						   'GROUP BY CDO.CDODefId, CDO.SecurityTypeId, Labels.LabelValue ORDER BY Labels.LabelValue ' +
                     ' FOR READ ONLY; OPEN @c1'
   EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
   FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   WHILE(@@fetch_status = 0)
   BEGIN
      EXEC csiRBACCreatePermission @RoleId, @PermissionName, @PermissionType, @ObjectMetaId, @PermissionModesFlag
      FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   END
   CLOSE @c1
   DEALLOCATE @c1
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForExternal
-- DESCR: Creates list of External Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermissionsForExternal' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermissionsForExternal
GO
CREATE PROCEDURE csiRBACCreatePermissionsForExternal(@RoleId CHAR(16), @ExternalPermission VARCHAR(255), @PermissionModesFlag INT)
AS
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
    DECLARE @ObjectMetaId INT
    DECLARE @PermissionType INT
    DECLARE @PermissionName VARCHAR(255)
BEGIN
   SET NOCOUNT ON;

   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 N'SELECT CDOTypeId, SecTypeId, ExternalPermissionName
							FROM ExternalPermission
							OUTER APPLY (SELECT SecurityMaskId AS SecTypeId FROM SecurityMaskDefinition WHERE Name = ''' + @ExternalPermission + ''') OA1 ' +
						   'ORDER By ExternalPermissionName ' +
                     ' FOR READ ONLY; OPEN @c1'
   EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
   FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   WHILE(@@fetch_status = 0)
   BEGIN
      EXEC csiRBACCreatePermission @RoleId, @PermissionName, @PermissionType, @ObjectMetaId, @PermissionModesFlag
      FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   END
   CLOSE @c1
   DEALLOCATE @c1
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAddVirtualPagePermissions 
-- DESCR: Creates permissions for all virtual pages
--		  @RoleId - The role to which the permissions are added
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAddVirtualPagePermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAddVirtualPagePermissions
GO
CREATE PROCEDURE csiRBACAddVirtualPagePermissions(@RoleId CHAR(16))
AS
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
    DECLARE @UIVirtualPageId CHAR(16)
    DECLARE @PermissionName VARCHAR(255)
BEGIN
   SET NOCOUNT ON;
   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 'Select UIVirtualPageName, UIVirtualPageId ' +
						 'From	UIVirtualPage ' +
					   	 'Order By UIVirtualPageName ' +
                     ' FOR READ ONLY; OPEN @c1'
   EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
   FETCH NEXT FROM @c1 INTO @PermissionName,@UIVirtualPageId
   WHILE(@@fetch_status = 0)
   BEGIN
      EXEC csiRBACCreatePermission @RoleId, @PermissionName, 200, NULL, 0, @UIVirtualPageId
      FETCH NEXT FROM @c1 INTO @PermissionName,@UIVirtualPageId
   END
   CLOSE @c1
   DEALLOCATE @c1
END

GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignPermissionToRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignPermissionToRole
GO

CREATE PROCEDURE csiRBACAssignPermissionToRole(@RoleName VARCHAR(255), @PermissionName VARCHAR(255), @PermissionType INT, @ObjectMetaId INT, @PermissionModesFlag INT, @ObjectInstanceId CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    DECLARE @IID VARCHAR(16)
	 DECLARE @RoleGID VARCHAR(36)
	 DECLARE @RoleId CHAR(16)
BEGIN
    SET NOCOUNT ON;
    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
    EXEC csiCreateGUID @PermissionName, @RoleGID OUTPUT
	SELECT @RoleId = RoleId FROM [RoleDef] WHERE RoleName = @RoleName
	IF NOT EXISTS (SELECT * FROM [RolePermission] WHERE RoleId = @RoleId AND RolePermissionName = @PermissionName)
    Begin  
		INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
				VALUES(@RoleGID,@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);

		-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
		-- based on the @PermissionModesFlag value
		IF ( @PermissionModesFlag = 0 )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
		Else IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180, 230) )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
			  AND BitNumber = 2
		ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType IN (180, 230) )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
			  AND BitNumber IN (1,2,3,4)
		ELSE
			PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
	END
END

GO
--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateDefaultData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'rbacPopulateDefaultData' 
	   AND 	  type = 'P')
    DROP PROCEDURE rbacPopulateDefaultData
GO
CREATE PROCEDURE rbacPopulateDefaultData
AS
    DECLARE @RoleId VARCHAR(16)
    DECLARE @IID VARCHAR(16)
    DECLARE @SessionId VARCHAR(16)
    DECLARE @InstanceId VARCHAR(16) 
	  
    
    DECLARE @PermissionSQL NVARCHAR(MAX)
    DECLARE @AdminPresent INT
    DECLARE @InSiteAdminPresent INT
    
    DECLARE @PermissionModesFlag_None INT
    DECLARE @PermissionModesFlag_ReadOnly INT
    DECLARE @PermissionModesFlag_NoSecAdmin INT
BEGIN
    SET NOCOUNT ON;
	
	SET @PermissionModesFlag_None = 0
    SET @PermissionModesFlag_ReadOnly = 1
    SET @PermissionModesFlag_NoSecAdmin = 2

	SELECT @AdminPresent = COUNT(*) FROM Employee WHERE EmployeeName='Administrator'
	SELECT @InSiteAdminPresent = COUNT(*) FROM Employee WHERE EmployeeName='InSiteAdmin'


    PRINT('Creating CamstarAdmin...');
	-- Create Employee: CamstarAdmin (copy from InSiteAdmin)
    IF NOT EXISTS (SELECT EmployeeName FROM Employee WHERE EmployeeName='CamstarAdmin')
    BEGIN
        EXEC csiPRDGetNextInstanceId 1140,@IID OUTPUT
        EXEC csiPRDGetNextInstanceId 1130,@SessionId OUTPUT
        INSERT INTO SessionValues (ExportImportKey,SessionValuesId, EmployeeId,  FactoryId, Application, 
                                Client, ChangeCount, CDOTypeId)  
        VALUES ('F18A0493-C55F-3D63-3B52-2FBF325A4930',@SessionId,  @IID, NULL, 0,  0, 1, 1130);

        INSERT INTO Employee (EmployeeId,  EmployeeName, FullName, SessionValuesId, CanLogin, 
                           ChangeCount, ModelerAccess, CDOTypeId) 
        VALUES (@IID, 'CamstarAdmin', 'Camstar Administrator', @SessionId, 1,   1, 1, 1140);

		UPDATE EMPLOYEE 
		SET  PortalMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenu'),
		PortalMobileMenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiMobileMenu'),
		PortalV8MenuDefinitionId = (SELECT PortalMenuDefinitionId FROM portalmenuDefinition WHERE PortalMenuDefinitionName = 'csiPortalMenuV8')
		WHERE EmployeeName = 'CamstarAdmin';
    END

    PRINT('Creating "Corporate" Organization...');
    -- Create Default Organization
    EXEC csiPRDGetNextInstanceId 7543,@IID OUTPUT
    INSERT INTO Organization(OrganizationId, OrganizationName, CDOTypeId, ChangeCount, Notes, ChangeHistoryId, Description, IconId, IsFrozen, ParentOrganizationId, OrganizationNumber)
       VALUES( @IID, 'Corporate', 7543, 1, NULL, NULL, 'Corporate Organization', 0, 0, NULL, NULL);

   PRINT('Creating "Login" Role...');
	EXEC csiRBACCreateRole 'Login','Login Access Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Login Access Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Login Access Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Login Access Role','CamstarAdmin', NULL, 0
    EXEC csiRBACCreatePermission @RoleId, 'System', 190, 1, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'MenuDefinitionMaint', 110, 6907, @PermissionModesFlag_ReadOnly
    EXEC csiRBACCreatePermission @RoleId, 'EmployeeMaint', 180, 3760, @PermissionModesFlag_ReadOnly
    
    PRINT('Creating "Security Administration" Role...');
    EXEC csiRBACCreateRole 'Security Administration','Security Administration Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Security Administration Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId,'Security Administration Role', 'Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId,'Security Administration Role', 'CamstarAdmin', NULL, 0
    EXEC csiRBACCreatePermission @RoleId, 'EmployeeMaint', 180, 3760, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'RoleMaint', 180, 7140, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'SetSessionFilterTagMaint', 180, 8777, @PermissionModesFlag_None
    -- Each of the following blocks represent a query-based Role/Permission assignment for default services
    PRINT('Creating "Default Modeling" Role...');
    EXEC csiRBACCreateRole 'Default Modeling','Default Modeling Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Modeling Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Modeling Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Modeling Role','CamstarAdmin', NULL, 0
    SET @PermissionSQL = '110'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'EmployeeMaint', 180, 3760, @PermissionModesFlag_NoSecAdmin
    EXEC csiRBACCreatePermission @RoleId, 'RoleMaint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	EXEC csiRBACCreatePermissionsForExternal @RoleId, 'ExternalPermission', @PermissionModesFlag_NoSecAdmin

    PRINT('Creating "Default Modeling Read-Only" Role...');
    EXEC csiRBACCreateRole 'Default Modeling Read-Only','Default Modeling Read-Only Role',@RoleId OUTPUT
    SET @PermissionSQL = '110'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_ReadOnly
    EXEC csiRBACCreatePermission @RoleId, 'EmployeeMaint', 180, 3760, @PermissionModesFlag_ReadOnly
    EXEC csiRBACCreatePermission @RoleId, 'RoleMaint', 180, 7140, @PermissionModesFlag_ReadOnly
	EXEC csiRBACCreatePermissionsForExternal @RoleId, 'ExternalPermission', @PermissionModesFlag_ReadOnly

    PRINT('Creating "Default Mfg" Role...');
    EXEC csiRBACCreateRole 'Default Mfg','Default Manufacturing Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Manufacturing Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Manufacturing Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Manufacturing Role','CamstarAdmin', NULL, 0
    SET @PermissionSQL = '100, 120, 130'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None


    PRINT('Creating "Default Quality" Role...');
    EXEC csiRBACCreateRole 'Default Quality','Default Quality Role',@RoleId OUTPUT
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Quality Role','Administrator', 'Corporate', 1
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Quality Role','CamstarAdmin', 'Corporate', 1
	SET @PermissionSQL = '170'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
	

    PRINT('Creating "Default Inquiry" Role...');
    EXEC csiRBACCreateRole 'Default Inquiry','Default Inquiry Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Inquiry Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Inquiry Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Inquiry Role','CamstarAdmin', NULL, 0
	SET @PermissionSQL = '140' 
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None


    PRINT('Creating "Default Export Import" Role...');
    EXEC csiRBACCreateRole 'Default Export Import','Default Export Import Role',@RoleId OUTPUT
    IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Export Import Role','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Export Import Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Export Import Role','CamstarAdmin', NULL, 0
    SET @PermissionSQL = '150,160' 
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None

    
    PRINT('Creating "Default Pages" Role...');
    EXEC csiRBACCreateRole 'Default Pages','Default Portal Pages Role',@RoleId OUTPUT
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0
	EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0
    EXEC csiRBACAddVirtualPagePermissions @RoleId
	EXEC csiRBACCreatePermission @RoleId, 'UI Virtual Page Maint', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Web Part Maint', 110, 8143, @PermissionModesFlag_None
    
    PRINT('Creating "Mfg Audit Trail Inquiry" Role...');
    EXEC csiRBACCreateRole 'Mfg Audit Trail Inquiry','Mfg Audit Trail Inquiry Role',@RoleId OUTPUT
    EXEC csiRBACCreatePermission @RoleId, 'Container Txn Rev', 130, 5440, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'Container History Inquiry', 140, 6908, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'History View Maint', 110, 7083, @PermissionModesFlag_ReadOnly
    
    PRINT('Creating "Portal Configuration" Role...');
    EXEC csiRBACCreateRole 'Portal Configuration','Portal Configuration Role',@RoleId OUTPUT
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId,'Portal Configuration Role', 'Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId,'Portal Configuration Role', 'CamstarAdmin', NULL, 0
    EXEC csiRBACCreatePermission @RoleId, 'Configurator', 210, 1, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'Portal Studio', 210, 2, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Portal Studio RBAC', 210, 3, @PermissionModesFlag_None

    PRINT('Creating "SPC" Role...');
    
    SELECT @InstanceId=UIVirtualPageId
      FROM UIVirtualPage
     WHERE UIVirtualPageName='SPCTesterFormVP'
     
     IF (@InstanceId is null) 
        BEGIN
           set @InstanceId=null;
        END       
    
	 

	EXEC csiRBACCreateRole 'SPC','SPC',@RoleId OUTPUT
    EXEC csiRBACAssignRole @RoleId, 'SPC','CamstarAdmin', NULL, 0
    EXEC csiRBACCreatePermission @RoleId, 'SPCChartDefMaint', 110, 8204, @PermissionModesFlag_None    
    EXEC csiRBACCreatePermission @RoleId, 'SPCTesterFormVP', 200, NULL, @PermissionModesFlag_None, @InstanceId        
    EXEC csiRBACCreatePermission @RoleId, 'Add SPC Annotation', 120, 8393, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'Record SPC Violation', 120, 8396, @PermissionModesFlag_None
	
	PRINT('Creating "DraftPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftPermissions','Draft Permissions Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DraftCamstarPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftCamstarPermissions','Draft Camstar Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DraftPLMPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftPLMPermissions','Draft PLM Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DEPCPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DEPCPermissions','Deployment Complete Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DEPIPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DEPIPermissions','Deployment Incomplete Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "PackageCreator" Role...');
	EXEC csiRBACCreateRole 'Package Creator','Package Creator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'Start Change Pkg', 120, 8500, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly
	
	PRINT('Creating "PackageOwner" Role...');
	EXEC csiRBACCreateRole 'Package Owner','Package Owner Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDO Inquiry', 140, 7398, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PLMApprovePackage', 120, 8582, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Routing Sheet Maint', 110, 7820, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Package Modeling Inquiry', 140, 8599, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	


	
	
	PRINT('Creating "Package Deployer" Role...'); 
	EXEC csiRBACCreateRole 'Package Deployer','Package Deployer Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	


	
	PRINT('Creating "Package Activator" Role...'); 
	EXEC csiRBACCreateRole 'Package Activator','Package Activator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'ActivateChangePkg', 120, 8528, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ActivationInquiry', 140, 8554, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None    
	EXEC csiRBACCreatePermission @RoleId, 'Export/Import Controller', 160, 7392, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Import Status Inquiry', 140, 7397, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Modeling data Import', 150, 7391, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
    
	PRINT('Creating "Package Approver" Role...');
	EXEC csiRBACCreateRole 'Package Approver','Package Approver Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None 
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly   
	EXEC csiRBACCreatePermission @RoleId, 'SignatureApproval', 120, 8568, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	



	PRINT('Creating "Package Collaborator" Role...'); 
	EXEC csiRBACCreateRole 'Package Collaborator','Package Collaborator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly 
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDO Inquiry', 140, 7398, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Package Modeling Inquiry', 140, 8599, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	



	PRINT('Creating "PDPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PDPermissions','Pending Deployment Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None

	PRINT('Creating "RejectPermissions" Role...'); 
	EXEC csiRBACCreateRole 'RejectPermissions','Rejected Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
    
	PRINT('Creating "PACPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PACPermissions','Pending Approval Camstar Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'SignatureApproval', 120, 8568, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
		
		
	PRINT('Creating "PAPLMPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PAPLMPermissions','Pending Approval PLM Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'PLMApprovePackage', 120, 8582, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	

	PRINT('Creating "Default Modeling Advanced" Role...'); 
	EXEC csiRBACCreateRole 'Default Modeling Advanced','Modeling Services for Advanced Users',@RoleId OUTPUT
	IF (@InSiteAdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Modeling Services for Advanced Users','InSiteAdmin', NULL, 0 
    END
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId, 'Modeling Services for Advanced Users','Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId, 'Modeling Services for Advanced Users','CamstarAdmin', NULL, 0
    SET @PermissionSQL = '230'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None

	PRINT('Assign "User Query Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Business Rule Handler Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Business Rule Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Scheduled Business Rule Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Summary Table Def Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly

	PRINT('Complete.');
END
GO
EXEC rbacPopulateDefaultData
GO
DROP PROCEDURE rbacPopulateDefaultData
GO
DROP PROCEDURE csiRBACAddVirtualPagePermissions 
GO


/*
// Scripts to remove all RBAC data
DELETE FROM EMPLOYEE WHERE EMPLOYEENAME IN ('CamstarAdmin');
DELETE FROM ROLEDEF;
DELETE FROM ORGANIZATION WHERE ORGANIZATIONNAME = 'Corporate';
DELETE FROM EMPLOYEEROLE;
DELETE FROM ROLEPERMISSION;
DELETE FROM ROLEPERMISSIONMODES;
*/
