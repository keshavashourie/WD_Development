--------------------------------------------------------------------------------
-- SCRIPT:PopulatePortalMenuDefaultData.sql
-- DESCR: Creates stored procedures used to create PortalMenuDefinitions and PortalMenuItems
--        and then uses those stored procedures to populate the default data
--
-- © 2018 Siemens Product Lifecycle Management Software Inc.

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCreateGUID' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCreateGUID
GO
CREATE PROCEDURE csiCreateGUID(@PermissionName NVARCHAR(255),@RoleGID varchar(36) OUTPUT)
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


END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        return InstanceId number
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'getNextInstanceId' 
	   AND 	  type = 'P')
    DROP PROCEDURE getNextInstanceId
GO
CREATE PROCEDURE getNextInstanceId(@CDODefId INT, @InstanceIdStr VARCHAR(16) OUTPUT)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CDODefIdStr VARCHAR(16)
    DECLARE @InstanceId INT
    DECLARE @InstIdNewValue VARCHAR(16)

    -- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
    -- Length should be 10 chars
    EXEC csiUpdateInstanceID 0,@CDODefId,1,@InstIdNewValue OUTPUT
	SET @InstIdNewValue=SUBSTRING(@InstIdNewValue,14,10)
	SET @InstIdNewValue=REPLICATE('0', (10 - LEN(@InstIdNewValue))) + @InstIdNewValue

    -- Convert CDODef Id to hex (pad to 6 chars)
	SET @CDODefIdStr=REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@CDODefId), 1, 2, ''), '0', ' ')), ' ', '0')
	SET @CDODefIdStr=REPLICATE('0', (6 - LEN(@CDODefIdStr))) + @CDODefIdStr

    -- Join CDODef hex and instance id hex strings. Length=16 chars
	SET @InstanceIdStr=LOWER(@CDODefIdStr+@InstIdNewValue)
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalMenuDefinition record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createPortalMenuDefinition' 
	   AND 	  type = 'P')
    DROP PROCEDURE createPortalMenuDefinition
GO
CREATE PROCEDURE createPortalMenuDefinition(
	@PortalMenuDefinitionName NVARCHAR(30), 
	@Description NVARCHAR(255), 
	@Notes NVARCHAR(2000), 
    @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;
    
    
    DECLARE @PortalMenuDefCDODefId INT
    SET @PortalMenuDefCDODefId=7828

    EXEC getNextInstanceId @PortalMenuDefCDODefId,@InstanceId OUTPUT
    
     IF NOT EXISTS (SELECT * FROM PortalMenuDefinition WHERE [PortalMenuDefinitionName] = @PortalMenuDefinitionName)
           Begin
    INSERT INTO PortalMenuDefinition
           ([PortalMenuDefinitionId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[Notes]
           ,[ChangeHistoryId]
           ,[Description]
           ,[IconId]
           ,[IsFrozen]
           ,[PortalMenuDefinitionName])
     VALUES
           (@InstanceId					--, char(16),>
           ,@PortalMenuDefCDODefId		--, int,>
           ,1							--, int,>
           ,@Notes						--, nvarchar(2000),>
           ,NULL						--, char(16),>
           ,@Description				--, nvarchar(255),>
           ,0							--, int,>
           ,0							--, bit,>
           ,@PortalMenuDefinitionName)	--, nvarchar(30),>)
            end
    ELSE
	select distinct @instanceid=Portalmenudefinitionid from PortalMenuDefinition where [PortalMenuDefinitionName] = @PortalMenuDefinitionName
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createPortalMenuItem' 
	   AND 	  type = 'P')
    DROP PROCEDURE createPortalMenuItem
GO
CREATE PROCEDURE createPortalMenuItem(
	@ParentId			char(16),
	@CDOTypeId			int, 
	@Sequence			int,
	@LabelName			nvarchar(66),
	@Caption			nvarchar(50),
	@VirtualPageName	varchar(30),
	@PageFlowName       varchar(30),
	@SubMenuName			varchar(30),
	@ApolloIconName			varchar(128) = NULL
)
AS
DECLARE @MenuItemId		varchar(16)
DECLARE @SubMenuId		varchar(16)
DECLARE @VirtualPageId	varchar(16)
DECLARE @PageFlowId		varchar(16)
BEGIN
    SET NOCOUNT ON;

	IF LEN(@SubMenuName)>0 
		Select Distinct @SubMenuId = PortalMenuDefinitionId from PortalMenuDefinition where PortalMenuDefinitionName = @SubMenuName
	if LEN(@VirtualPageName)>0 
		select Distinct @VirtualPageId = UIVirtualPageId from UIVirtualPage where UIVirtualPageName = @VirtualPageName
	if LEN(@PageFlowName)>0 
		select Distinct @PageFlowId = UIPageFlowId from UIPageFlow where UIPageFlowName = @PageFlowName
		
    EXEC getNextInstanceId @CDOTypeId,@MenuItemId OUTPUT
	if @SubMenuName = 'csiChangeManagement'
	begin
		IF NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption = 'Change Management')
           		Begin
   				 INSERT INTO PortalMenuItem
      				     ([PortalMenuItemId]
      				     ,[CDOTypeId]
      				     ,[ChangeCount]
      				     ,[ParentId]
      				     ,[IsFrozen]
       				    ,[Caption]
						,[LabelName]
      				     ,[Sequence]
       				    ,[MenuDefinitionId]
 				          ,[VirtualPageId]
 				          ,[PageFlowId]
 				          ,[PageURL]
 				          ,[PageDisplay]
 						,[QueryString]
						,[ApolloIcon])
 				    VALUES
      				     (@MenuItemId				--, char(16),>
      				     ,@CDOTypeId				--, int,>
      				     ,1						--, int,>
     				      ,@ParentId				--, char(16),>
      				     ,0						--, bit,>
    				       ,@Caption				--, nvarchar(50),>
						   ,@LabelName				--, nvarchar(66),>
				           ,@Sequence				--, int,>
				           ,@SubMenuId				--, char(16),>
				           ,@VirtualPageId			--, char(16),>
 				          ,@PageFlowId				--, char(16),>
				           ,null					--, nvarchar(512),>
				           ,null					--, int,>
 						,null					--, nvarchar(512),>
						,@ApolloIconName)		--, nvarchar(30),>
			end
			else
				PRINT('csichangemanagement already exists');
		end
		else
		IF NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption= @Caption AND ParentId= @ParentId)
           		Begin
		INSERT INTO PortalMenuItem
           ([PortalMenuItemId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ParentId]
           ,[IsFrozen]
           ,[Caption]
		   ,[LabelName]
           ,[Sequence]
           ,[MenuDefinitionId]
           ,[VirtualPageId]
           ,[PageFlowId]
           ,[PageURL]
           ,[PageDisplay]
           ,[QueryString]
           ,[ApolloIcon])
     VALUES
           (@MenuItemId				--, char(16),>
           ,@CDOTypeId				--, int,>
           ,1						--, int,>
           ,@ParentId				--, char(16),>
           ,0						--, bit,>
           ,@Caption				--, nvarchar(50),>
		   ,@LabelName				--, nvarchar(66),>
           ,@Sequence				--, int,>
           ,@SubMenuId				--, char(16),>
           ,@VirtualPageId			--, char(16),>
           ,@PageFlowId				--, char(16),>
           ,null					--, nvarchar(512),>
           ,null					--, int,>
           ,null					--, nvarchar(512),>
		   ,@ApolloIconName)		--, nvarchar(30),>)
	end
	else
	select @MenuItemId = portalmenuitemid from portalmenuitem where Caption = @Caption AND ParentId= @ParentId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
-- © 2017 Siemens Product Lifecycle Management Software Inc.

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermission' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermission
GO
CREATE PROCEDURE csiRBACCreatePermission(@RoleId CHAR(16), @PermissionName NVARCHAR(255), @PermissionType INT, @ObjectMetaId INT, @PermissionModesFlag INT, @ObjectInstanceId CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    DECLARE @IID VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
    
      IF NOT EXISTS (SELECT * FROM RolePermission WHERE RolePermissionName = @PermissionName and RoleId = @RoleId)
           Begin
    INSERT INTO RolePermission(RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);
			   end
    --ELSE
	--	PRINT('[RolePermissionName] ' + @PermissionName + ' already exists');

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
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- © 2017 Siemens Product Lifecycle Management Software Inc.

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignRole
GO
CREATE  PROCEDURE csiRBACAssignRole(@RoleId CHAR(16), @RoleDescription NVARCHAR(255), @EmployeeName NVARCHAR(255), @OrganizationName NVARCHAR(255), @Propagate INT)
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
	IF NOT EXISTS (SELECT * FROM employeerole WHERE Roleid = @RoleId)
           Begin
    INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (@RoleGID,@IID, @CDODefId, @RoleId, @EmployeeId, 0, @Propagate, @OrgId);
	   end;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- © 2017 Siemens Product Lifecycle Management Software Inc.

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
    DECLARE @PermissionName NVARCHAR(255)
BEGIN
   SET NOCOUNT ON;
   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 'Select CDO.CDODefId As ObjectMetaId ' +
								', CDO.SecurityTypeId as PermissionType ' +
								', Labels.LabelValue as PermissionName ' +
						  'From	CDODefinition CDO ' +
						  '		, Labels ' +
						  'Where	Labels.LabelId = CDO.DisplayNameLabelId ' +
						     'And		CDO.IsAbstract = 0 ' +
						     'And		CDO.SecurityTypeId In ('+ @SecurityList +' ) ' +
						   'ORDER By CDO.SecurityTypeId, CDO.CDOName ' +
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
-- PROCEDURE: csiRBACAddCMVirtualPagePermissions 
-- DESCR: Creates permissions for all virtual pages
--		  @RoleId - The role to which the permissions are added
-- © 2017 Siemens Product Lifecycle Management Software Inc.

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAddCMVirtualPagePermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAddCMVirtualPagePermissions
GO
CREATE PROCEDURE csiRBACAddCMVirtualPagePermissions(@RoleId CHAR(16))
AS
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
    DECLARE @UIVirtualPageId CHAR(16)
    DECLARE @PermissionName NVARCHAR(255)
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
-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Create Portal Menus 
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'populatePortalMenuUpdateData' 
	   AND 	  type = 'P')
    DROP PROCEDURE populatePortalMenuUpdateData
GO
CREATE PROCEDURE populatePortalMenuUpdateData
AS
DECLARE @MenuDefId varchar(16)
DECLARE @AssignedMenuDefId varchar(16)
DECLARE @HomePage varchar(16)
DECLARE @DefaultNotes varchar(100)
DECLARE @RoleId VARCHAR(16)
DECLARE @PermissionModesFlag_None INT
DECLARE @PermissionModesFlag_ReadOnly INT
DECLARE @PermissionModesFlag_NoSecAdmin INT
DECLARE @PermissionSQL NVARCHAR(MAX)
DECLARE @InstanceId VARCHAR(16)
BEGIN
    SET NOCOUNT ON;
	
	SET @PermissionModesFlag_None = 0
SET @PermissionModesFlag_ReadOnly = 1
SET @PermissionModesFlag_NoSecAdmin = 2
	
	BEGIN TRAN
	
	SET @DefaultNotes = 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.'

	-- Each of the following blocks represent a query-based Role/Permission assignment for default services
    PRINT('Creating "Default Modeling" Role...');
    select @RoleId = RoleId from RoleDef where Rolename = 'Default Modeling'
    SET @PermissionSQL = '110'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'bpScaleMaint', 180, 3760, @PermissionModesFlag_NoSecAdmin
    EXEC csiRBACCreatePermission @RoleId, 'bpScaleFamilyMaint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	EXEC csiRBACCreatePermission @RoleId, 'bpScaleGroupMaint', 180, 7140, @PermissionModesFlag_NoSecAdmin

  
	select @RoleId = RoleId from RoleDef where Rolename = 'Default Pages'
	EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0
	EXEC csiRBACAddCMVirtualPagePermissions @RoleId
	EXEC csiRBACCreatePermission @RoleId, 'bpScale_VP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpScaleFamily_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpSpecMaintForm_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpBOMMaint_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpComponentIssueVP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpERPBOMMaint_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpMfgOrder_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpProduct_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpProductFamily_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpTaskList_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpWeighIssue_VP', 110, 8143, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'bpWeighTaskItemDetail_VP', 110, 8143, @PermissionModesFlag_None

truncate table SecurityCacheRefreshRequest

INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (47,GETDATE(),GETUTCDATE(),N'Default Pages',2)
INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (48,GETDATE(),GETUTCDATE(),N'Default Modeling',2)

	COMMIT TRAN
END
GO

EXEC populatePortalMenuUpdateData
GO
DROP PROCEDURE getNextInstanceId
GO
DROP PROCEDURE createPortalMenuItem
GO
DROP PROCEDURE createPortalMenuDefinition
GO
DROP PROCEDURE populatePortalMenuUpdateData
GO
