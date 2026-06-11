--------------------------------------------------------------------------------
-- SCRIPT:PopulatePortalMenuDefaultData.sql
-- DESCR: Creates stored procedures used to create PortalMenuDefinitions and PortalMenuItems
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2023  

IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCreateGUID' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCreateGUID
GO

CREATE PROCEDURE csiCreateGUID(
@PermissionName		NVARCHAR(255)
,@RoleGID			VARCHAR(36) OUTPUT)
AS
DECLARE @CDODefId	INT
DECLARE @EmployeeId CHAR(16)
DECLARE @OrgId		CHAR(16)
DECLARE @IID		VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

	select @RoleGID =	
	(      SubString(ExportImportKeyGUID, 1, 8) +
	'-' + SubString(ExportImportKeyGUID, 9, 4) +
	'-' + SubString(ExportImportKeyGUID, 13, 4) +
	'-' + SubString(ExportImportKeyGUID, 17, 4) +
	'-' + SubString(ExportImportKeyGUID, 21,12))
	from 
	(
			select upper(convert(nvarchar(36),hashbytes('MD5',@PermissionName),2)) as ExportImportKeyGUID
	) c;	
END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM reakted roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
     IF NOT EXISTS (SELECT * FROM RoleDef WHERE RoleName = @RoleName)
           Begin
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
        end
    ELSE
        Begin
	delete from EmployeeRole WHERE RoleId = (Select RoleId from RoleDef WHERE RoleName = @RoleName)
    delete from RoleDef WHERE RoleName = @RoleName
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
       PRINT('[RoleDef] ' + @RoleName + ' deleted and created');
        end
		
     
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignRoleIfExist' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignRoleIfExist
GO
CREATE PROCEDURE csiRBACAssignRoleIfExist(@ExRoleName NVARCHAR(255), @NewRoleId CHAR(16), @NewRoleDescription NVARCHAR(255), @OrganizationName NVARCHAR(255), @Propagate INT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
    DECLARE @RoleGID NVARCHAR(36)
	DECLARE @EmployeeName VARCHAR (500)


BEGIN
    SET NOCOUNT ON;
	--------------------------------------------------
	DECLARE @CURSOR CURSOR
	SET @CURSOR  = CURSOR SCROLL
	FOR
	SELECT [Employee].EmployeeName FROM [EmployeeRole] 
	LEFT JOIN [RoleDef] ON [EmployeeRole].RoleId = [RoleDef].RoleId
	LEFT JOIN [Employee] ON [EmployeeRole].EmployeeId = [Employee].EmployeeId
	WHERE [RoleDef].RoleName = @ExRoleName
	OPEN @CURSOR
	FETCH NEXT FROM @CURSOR INTO @EmployeeName
	WHILE @@FETCH_STATUS = 0
	BEGIN
	--------------------------------------------------
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
		set @NewRoleDescription = @NewRoleDescription + @EmployeeName
		EXEC csiCreateGUID @NewRoleDescription, @RoleGID OUTPUT
		INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
        VALUES (@RoleGID,@IID, @CDODefId, @NewRoleId, @EmployeeId, 0, @Propagate, @OrgId);
	   FETCH NEXT FROM @CURSOR INTO @EmployeeName
	 --------------------------------------------------
	 END
	 CLOSE @CURSOR
END

GO

--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        return InstanceId number
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'getNextInstanceId' 
	AND 	  type = 'P'
)
DROP PROCEDURE getNextInstanceId
GO

CREATE PROCEDURE getNextInstanceId(
@CDODefId		INT,
@InstanceIdStr	VARCHAR(16) OUTPUT)
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
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createPortalMenuDefinition' 
	AND 	  type = 'P'
)
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
     BEGIN
		INSERT 
		INTO PortalMenuDefinition
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
        (@InstanceId				--, char(16),>
        ,@PortalMenuDefCDODefId		--, int,>
        ,1							--, int,>
        ,@Notes						--, nvarchar(2000),>
        ,NULL						--, char(16),>
        ,@Description				--, nvarchar(255),>
        ,0							--, int,>
        ,0							--, bit,>
        ,@PortalMenuDefinitionName)	--, nvarchar(30),>)
	END
    ELSE
		SELECT DISTINCT @instanceid=Portalmenudefinitionid 
		FROM PortalMenuDefinition 
		WHERE [PortalMenuDefinitionName] = @PortalMenuDefinitionName
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACDeleteUnavailablePermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACDeleteUnavailablePermissions
GO

CREATE PROCEDURE csiRBACDeleteUnavailablePermissions(@PermissionType INT, @PermissionName VARCHAR(255))
AS
	DECLARE @UnavialiblePermissions CHAR(16)
	BEGIN
		SET NOCOUNT ON;
		DECLARE @CURSOR CURSOR
		SET @CURSOR  = CURSOR SCROLL
		FOR
		SELECT [RolePermission].RolePermissionId FROM [RolePermission] 
		WHERE RolePermissionName = @PermissionName AND PermissionType <> @PermissionType
		OPEN @CURSOR
		FETCH NEXT FROM @CURSOR INTO @UnavialiblePermissions
		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM [RolePermission] WHERE RolePermissionId = @UnavialiblePermissions
			DELETE FROM [RolePermissionModes] WHERE RolePermissionId = @UnavialiblePermissions
			FETCH NEXT FROM @CURSOR INTO @UnavialiblePermissions
		END
	CLOSE @CURSOR
	END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createPortalMenuItem' 
	AND 	  type = 'P'
)
DROP PROCEDURE createPortalMenuItem
GO

CREATE PROCEDURE createPortalMenuItem(
	@ParentId				char(16),
	@CDOTypeId				int, 
	@Sequence				int,
	@Caption				nvarchar(50),
	@Labelname				nvarchar(66),
	@VirtualPageName		varchar(30),
	@PageFlowName			varchar(30),
	@SubMenuName			varchar(30),
	@ApolloIconName			varchar(128) = NULL,
	@ServiceName		    varchar(128) = NULL
)
AS
DECLARE @MenuItemId		varchar(16)
DECLARE @SubMenuId		varchar(16)
DECLARE @VirtualPageId	varchar(16)
DECLARE @PageFlowId		varchar(16)
BEGIN
    SET NOCOUNT ON;

	IF LEN(@SubMenuName)>0 
		SELECT DISTINCT @SubMenuId = PortalMenuDefinitionId 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = @SubMenuName
	IF LEN(@VirtualPageName)>0 
		SELECT DISTINCT @VirtualPageId = UIVirtualPageId 
		FROM UIVirtualPage 
		WHERE UIVirtualPageName = @VirtualPageName
	IF LEN(@PageFlowName)>0 
		SELECT DISTINCT @PageFlowId = UIPageFlowId 
		FROM UIPageFlow 
		WHERE UIPageFlowName = @PageFlowName
		
    EXEC getNextInstanceId @CDOTypeId,@MenuItemId OUTPUT

	IF @SubMenuName = 'csiChangeManagement'
	BEGIN
		IF NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption = 'Change Management')
        BEGIN
			INSERT 
			INTO PortalMenuItem
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
			,[ApolloIcon]
			,[ServiceName])
 			VALUES
      		(@MenuItemId			--, char(16),>
      		,@CDOTypeId				--, int,>
      		,1						--, int,>
     		,@ParentId				--, char(16),>
      		,0						--, bit,>
    		,@Caption				--, nvarchar(50),>
    		,@Labelname
			,@Sequence				--, int,>
			,@SubMenuId				--, char(16),>
			,@VirtualPageId			--, char(16),>
 			,@PageFlowId				--, char(16),>
			,null					--, nvarchar(512),>
			,null					--, int,>
 			,null					--, nvarchar(512),>
			,@ApolloIconName		--, nvarchar(30),>
			,@ServiceName)          --, nvarchar(32),>)
		END
		ELSE
			PRINT('csichangemanagement already exists');
		END
	ELSE
		IF ((NOT EXISTS (SELECT * FROM PortalMenuitem WHERE caption= @Caption))
		OR 
			(EXISTS(
				SELECT * from PortalMenuDefinition 
				WHERE @ParentId = PortalMenuDefinitionId AND PortalMenuDefinitionName LIKE '%V8%' 
				)
			AND
			(NOT EXISTS 
				(SELECT * FROM PortalMenuitem 
				JOIN PortalMenuDefinition ON ParentId= PortalMenuDefinitionId
					WHERE @ParentId = PortalMenuDefinitionId AND PortalMenuDefinitionName LIKE '%V8%' AND caption= @Caption
					)
				)
			)
		)
        BEGIN
			INSERT 
			INTO PortalMenuItem
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
           ,@Labelname
           ,@Sequence				--, int,>
           ,@SubMenuId				--, char(16),>
           ,@VirtualPageId			--, char(16),>
           ,@PageFlowId				--, char(16),>
           ,null					--, nvarchar(512),>
           ,null					--, int,>
           ,null					--, nvarchar(512),>
		   ,@ApolloIconName)		--, nvarchar(30),>)
		END
		ELSE
			SELECT @MenuItemId = portalmenuitemid 
			FROM portalmenuitem 
			WHERE Caption = @Caption;
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
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
-- Copyright Siemens 2023  

IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACCreatePermission' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACCreatePermission
GO

CREATE PROCEDURE csiRBACCreatePermission(
@RoleId CHAR(16), 
@PermissionName NVARCHAR(255), 
@PermissionType INT, 
@ObjectMetaId INT, 
@PermissionModesFlag INT, 
@ObjectInstanceId CHAR(16) = NULL)
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
    BEGIN
		INSERT 
		INTO RolePermission
		(RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
		VALUES(@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);
	END
    --ELSE
		--	PRINT('[RolePermissionName] ' + @PermissionName + ' already exists');

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the @PermissionModesFlag value
	IF ( @PermissionModesFlag = 0 )
		BEGIN TRY
			INSERT 
			INTO RolePermissionModes
			(RolePermissionId, FieldId, Modes, Sequence)
			SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=@PermissionType
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
    ELSE IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180) )
		BEGIN TRY
			INSERT 
			INTO RolePermissionModes
			(RolePermissionId, FieldId, Modes, Sequence)
			SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=@PermissionType
			AND BitNumber = 2
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
	ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType = 180 )
		BEGIN TRY
			INSERT 
			INTO RolePermissionModes
			(RolePermissionId, FieldId, Modes, Sequence)
			SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			FROM SecurityMaskDetail
			WHERE SecurityMaskId=@PermissionType
			AND BitNumber IN (1,2,3,4)
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
	ELSE
		PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
	END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  

IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACAssignRole' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACAssignRole
GO

CREATE  PROCEDURE csiRBACAssignRole(
@RoleId CHAR(16), 
@RoleDescription NVARCHAR(255), 
@EmployeeName NVARCHAR(255), 
@OrganizationName NVARCHAR(255), 
@Propagate INT)
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
		SET @RoleDescription = @RoleDescription + @EmployeeName
		EXEC csiCreateGUID @RoleDescription, @RoleGID OUTPUT

		IF NOT EXISTS (SELECT * FROM employeerole WHERE Roleid = @RoleId)
        BEGIN
			INSERT 
			INTO EmployeeRole
			(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
			VALUES (@RoleGID,@IID, @CDODefId, @RoleId, @EmployeeId, 0, @Propagate, @OrgId);
		END
END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  

IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACCreatePermissionsForQry' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACCreatePermissionsForQry
GO

CREATE PROCEDURE csiRBACCreatePermissionsForQry(
@RoleId CHAR(16), 
@SecurityList NVARCHAR(MAX), 
@PermissionModesFlag INT)
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
-- Copyright Siemens 2023  

IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACAddCMVirtualPagePermissions' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACAddCMVirtualPagePermissions
GO

CREATE PROCEDURE csiRBACAddCMVirtualPagePermissions(
@RoleId CHAR(16))
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
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'populatePortalMenuUpdateData' 
	AND 	  type = 'P'
)
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
DECLARE @csiPortalMenuPortalMenuDefinitionId varchar(16)
DECLARE @csiMobileMenuPortalMenuDefinitionId varchar(16)
DECLARE @csiPortalMenuV8PortalMenuDefinitionId varchar(16)

BEGIN
	SET NOCOUNT ON;
	
	SET @PermissionModesFlag_None = 0
	SET @PermissionModesFlag_ReadOnly = 1
	SET @PermissionModesFlag_NoSecAdmin = 2
	
	BEGIN TRAN
		SET @DefaultNotes = 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.'
			
		PRINT('Creating Change Management Menu...')
		EXEC createPortalMenuDefinition 'csiChangeManagement', 'Change Management pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', ''
		
		PRINT('Creating Attachments Menu...')
		EXEC createPortalMenuDefinition 'csiAttachments', 'Attachments', @DefaultNotes, @MenuDefId OUTPUT

		DELETE PortalMenuItem 
		WHERE  Caption = 'Attach Documents'

		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Attach Document','Lbl_AttachDocument_Title', 'AttachDocument_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', ''

		SELECT @MenuDefId = PortalMenuDefinitionId 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiPortalMenu' 

		PRINT('Creating Portal Main Menu...')
		EXEC createPortalMenuItem @MenuDefId, 7833, 03, 'Attachments', 'LblMenuAttachments','', '', 'csiAttachments', 'cmdAttach'
		EXEC createPortalMenuItem @MenuDefId, 7833, 05, 'Change Management','LblMenuChgMngt', '', '', 'csiChangeManagement', 'cmdChangeManagement'
		
		--'Creating Container Mobile Sub Menu'
		PRINT('Creating Container Mobile Sub Menu...')
		EXEC createPortalMenuDefinition 'csiContainerMobileMenu', 'Mobile Menu for Container Txns', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Move', 'LblMenuMove', 'MoveStdVP_R2', '', ''

		--'Creating Mobile Main Menu'
		PRINT('Creating Mobile Main Menu...')
		EXEC createPortalMenuDefinition 'csiMobileMenu', 'The top level Mobile Portal menu', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7833, 03, 'Container', 'LblMenuCont','', '', 'csiContainerMobileMenu', 'cmdLot'

		SELECT @MenuDefId = PortalMenuDefinitionId 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiSearch' 

		DELETE PortalMenuItem 
		WHERE  Caption = 'Audit Trail'

		PRINT('Creating Search Menu...')
		EXEC createPortalMenuItem @MenuDefId, 7835, 35, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 45, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 55, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP', '', ''

		--Classic Container menu update
		SELECT @MenuDefId = PortalMenuDefinitionId 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiContainer' 

		EXEC createPortalMenuItem @MenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', ''
	
		EXEC createPortalMenuItem @MenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate'
		EXEC createPortalMenuItem @MenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate'
		EXEC createPortalMenuItem @MenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart'
		EXEC createPortalMenuItem @MenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple'
	
		SELECT @MenuDefId = PortalMenuDefinitionId 
		FROM PortalMenuDefinition 
		WHERE PortalMenuDefinitionName = 'csiModelingMenu'

		DELETE PortalMenuItem 
		WHERE Caption = 'Audit Trail'

		PRINT('Creating Modeling Menu...')
		EXEC createPortalMenuItem @MenuDefId, 7835, 25, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', ''
	
		-- Each of the following blocks represent a query-based Role/Permission assignment for default services
		PRINT('Creating "Default Modeling" Role...');

		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling'

		SET @PermissionSQL = '110'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Employee Maint', 180, 3760, @PermissionModesFlag_NoSecAdmin
		--EXEC csiRBACCreatePermission @RoleId, 'Role Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin

		--PRINT('Creating "Default Modeling Advanced" Role...'); 
		--EXEC csiRBACCreateRole 'Default Modeling Advanced','Modeling Services for Advanced Users',@RoleId OUTPUT
		--EXEC csiRBACAssignRoleIfExist 'Default Modeling', @RoleId, 'Modeling Services for Advanced Users', NULL, 0
		SET @PermissionSQL = '230'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
		--EXEC csiRBACDeleteUnavailablePermissions 230, 'User Query Maint'
		--EXEC csiRBACDeleteUnavailablePermissions 230, 'Business Rule Handler Maint'
		--EXEC csiRBACDeleteUnavailablePermissions 230, 'Business Rule Maint'
		--EXEC csiRBACDeleteUnavailablePermissions 230, 'Scheduled Business Rule Maint'
		--EXEC csiRBACDeleteUnavailablePermissions 230, 'Summary Table Def Maint'


		PRINT('Creating "Default Modeling Advanced" Role...'); 
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling Advanced'

		SET @PermissionSQL = '230'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None


		PRINT('Creating "Default Inquiry" Role...');
    
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Inquiry'

		SET @PermissionSQL = '140' 
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None


		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Pages'

		--EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0
		--EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','InSiteAdmin', NULL, 0
		--EXEC csiRBACAddCMVirtualPagePermissions @RoleId
		--EXEC csiRBACCreatePermission @RoleId, 'UI Virtual Page Maint', 110, 7755, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Web Part Maint', 110, 8143, @PermissionModesFlag_None
	
	    --PRINT('Creating "Login" Role...');
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Login'

		--EXEC csiRBACCreatePermission @RoleId, 'System', 190, 1, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Menu Definition Maint', 110, 6907, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACCreatePermission @RoleId, 'Employee Maint', 180, 3760, @PermissionModesFlag_ReadOnly
    
		--PRINT('Creating "Security Administration" Role...');
    
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Security Administration'

		--EXEC csiRBACCreatePermission @RoleId, 'Employee Maint', 180, 3760, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Role Maint', 180, 7140, @PermissionModesFlag_None

		--PRINT('Creating "Default Modeling Read-Only" Role...');
    
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Modeling Read-Only'

		SET @PermissionSQL = '110'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACCreatePermission @RoleId, 'Employee Maint', 180, 3760, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACCreatePermission @RoleId, 'Role Maint', 180, 7140, @PermissionModesFlag_ReadOnly

		--PRINT('Assign "User Query Maint" Permission...');
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
		--PRINT('Assign "Business Rule Handler Maint" Permission...');
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
		--PRINT('Assign "Business Rule Maint" Permission...');
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
		--PRINT('Assign "Scheduled Business Rule Maint" Permission...');
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
		--PRINT('Assign "Summary Table Def Maint" Permission...');
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly
		--EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly


		--PRINT('Creating "Default Mfg" Role...');
    
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Mfg'

		SET @PermissionSQL = '100, 120, 130'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None

		PRINT('Creating "Default Quality" Role...');
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Quality'

		SET @PermissionSQL = '170'
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None

		PRINT('Creating "Default Export Import" Role...');
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Default Export Import'

		SET @PermissionSQL = '150,160' 
		--EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
       
		--PRINT('Creating "Mfg Audit Trail Inquiry" Role...');
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Mfg Audit Trail Inquiry'

		--EXEC csiRBACCreatePermission @RoleId, 'Container Txn Rev', 130, 5440, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Container History Inquiry', 140, 6908, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'History View Maint', 110, 7083, @PermissionModesFlag_ReadOnly
    
		--PRINT('Creating "Portal Configuration" Role...');
		
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'Portal Configuration'

		--EXEC csiRBACCreatePermission @RoleId, 'Configurator', 210, 1, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Portal Studio', 210, 2, @PermissionModesFlag_None

		--PRINT('Creating "SPC" Role...');
    
		SELECT @InstanceId=UIVirtualPageId
		FROM UIVirtualPage
		WHERE UIVirtualPageName='SPCTesterFormVP'
     
		IF (@InstanceId is null) 
        BEGIN
			SET @InstanceId=null;
        END       
    
		SELECT @RoleId = RoleId 
		FROM RoleDef 
		WHERE Rolename = 'SPC'

		--EXEC csiRBACCreatePermission @RoleId, 'SPCChartDefMaint', 110, 8204, @PermissionModesFlag_None    
		--EXEC csiRBACCreatePermission @RoleId, 'SPCTesterFormVP', 200, NULL, @PermissionModesFlag_None, @InstanceId        
		--EXEC csiRBACCreatePermission @RoleId, 'Add SPC Annotation', 120, 8393, @PermissionModesFlag_None
		--EXEC csiRBACCreatePermission @RoleId, 'Record SPC Violation', 120, 8396, @PermissionModesFlag_None

		--Create V8 Menu with Apollo
		PRINT('Creating Container Transaction V8 Menu Definition...')
		EXEC createPortalMenuDefinition 'csiContainerV8', 'Container transactions used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssue_VPR2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 130, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 134, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate'		
		EXEC createPortalMenuItem @MenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EProcedureVPR2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVPR2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VPR2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart'
		EXEC createPortalMenuItem @MenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple'
		EXEC createPortalMenuItem @MenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 410, 'HV Component Issue', 'CSICDOName_HVComponentIssue','HVComponentIssueVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 420, 'Slitting', 'CSICDOName_Slitting','SlittingVP', '', '' 

		PRINT('Creating Job Services Menu...')
		EXEC createPortalMenuDefinition 'csiResourceTxn_JobSvc', 'Job Services', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Job Create', 'CSICDOName_JobCreate','JobCreateVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Job Assign', 'CSICDOName_JobAssign','JobAssign_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Job Acknowledge', 'CSICDOName_JobAcknowledge','JobAcknowledge_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Job Clock On', 'CSICDOName_JobClockOn','JobClockOn_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Job Progress', 'CSICDOName_JobProgress','JobProgress_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 60, 'Job Clock Off', 'CSICDOName_JobClockOff','JobClockOff_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 70, 'Job Complete', 'CSICDOName_JobComplete','JobComplete_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 80, 'Job Cancel', 'CSICDOName_JobCancel','JobCancel_VP', '', ''
		
		PRINT('Creating Jobs Menu...')
		EXEC createPortalMenuDefinition 'csiResourceTxn_Job', 'Jobs', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Supervisor Jobs', 'LblMenuSupervisorJobs','JobSupervisor_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Technician Jobs', 'LblMenuTechnicianJobs','JobTechnicians_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7833, 30, 'Job Services', 'PortalUI_JobServices', '','', 'csiResourceTxn_JobSvc', ''
		
		PRINT('Creating Part Services Menu...')
		EXEC createPortalMenuDefinition 'csiResourceTxn_PartSvc', 'Part Services', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Part Create', 'CSICDOName_PartCreate','PartCreate_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Part Setup', 'CSICDOName_PartSetup','PartSetupVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Part Scrap', 'CSICDOName_PartScrap','PartScrap_VP', '', ''	
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Part Request', 'CSICDOName_PartRequest','PartRequestVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Part Request Acknowledge', 'CSICDOName_PartRequestAcknowledge','PartRequestAcknowledgeVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 60, 'Part Request Assign', 'CSICDOName_PartRequestAssign','PartRequestAssign_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 70, 'Part Request Update', 'CSICDOName_PartRequestUpdate','PartRequestUpdateVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 80, 'Part Request Issue', 'CSICDOName_PartRequestIssue','PartRequestIssue_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 90, 'Part Request Complete', 'CSICDOName_PartRequestComplete','PartRequestComplete_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 100, 'Part Request Cancel', 'CSICDOName_PartRequestCancel','PartRequestCancel_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 110, 'Part Request Cancel Acknowledge', 'CSICDOName_PartRequestCancelAcknowledge','PartRequestCancelAcknowledgeVP', '', ''
		
		PRINT('Creating Parts Menu...')
		EXEC createPortalMenuDefinition  'csiResourceTxn_Part', 'Parts', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Material Parts', 'PartRequestOrder_MaterialParts','MaterialPartMaintenanceVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Part Maintenance', 'LblPartMaintenance','PartMaintenanceVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Technician Requests', 'LblTechnicianRequests','PartRequestMain_VP', '', '', '', 'PartRequest'
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Inventory Requests', 'LblInventoryRequests','PartRequestMain_VP', '', '', '', 'PartRequestAssign'
		EXEC createPortalMenuItem @MenuDefId, 7833, 50, 'Part Services', 'PortalUI_PartServices', '','', 'csiResourceTxn_PartSvc', ''

		PRINT('Creating Resource V8 Menu...')
		EXEC createPortalMenuDefinition 'csiResourceV8', 'Resource transactions available in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP_R2', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 80, 'Jobs', 'PortalUI_Jobs', '','', 'csiResourceTxn_Job', '' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 90, 'Parts', 'Resource_Parts', '','', 'csiResourceTxn_Part', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 100, 'HV Resource Setup', 'CSICDOName_HVResourceSetup','HVResourceSetupVP', '', '' 

		PRINT('Creating Events V8 Menu...')
		EXEC createPortalMenuDefinition 'csiEventV8', 'Quality event transactions in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', '' 

		PRINT('Creating Search V8 Menu...')
		EXEC createPortalMenuDefinition 'csiSearchV8', 'Search options in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP_R2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP_R2', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '' 

		PRINT('Creating SPC V8 Menu...')
		EXEC createPortalMenuDefinition 'csiSPCV8', 'SPC pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'SPC Realtime Monitoring', 'LblSPCRealtimeMonitoring', 'SPCRealtimeMonitoring_VP', '', '' 
		
		PRINT('Creating Modeling V8 Menu...')
		EXEC createPortalMenuDefinition 'csiModelingMenuV8', 'Modeling pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', ''
		
		PRINT('Creating Training V8 Menu...')
		EXEC createPortalMenuDefinition 'csiTrainingMenuV8', 'Training pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', ''
			
		PRINT('Creating Export/Import Menu...')
		EXEC createPortalMenuDefinition 'csiExport/ImportV8', 'Export/Import pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', ''
		
		PRINT('Creating Change Management V8 Menu...')
		EXEC createPortalMenuDefinition 'csiChangeManagementV8', 'Change Management pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', ''
		EXEC createPortalMenuItem @MenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', ''

		PRINT('Creating Attachments V8 Menu...')
		EXEC createPortalMenuDefinition 'csiAttachmentsV8', 'Attachments', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' 
		EXEC createPortalMenuItem @MenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', ''


		PRINT('Creating Portal V8 Main Menu...')
		EXEC createPortalMenuDefinition 'csiPortalMenuV8', 'The top level Portal menu', @DefaultNotes, @MenuDefId OUTPUT
		EXEC createPortalMenuItem @MenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach'
		EXEC createPortalMenuItem @MenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement'
		EXEC createPortalMenuItem @MenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality'
		EXEC createPortalMenuItem @MenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport'
		EXEC createPortalMenuItem @MenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch' 
		EXEC createPortalMenuItem @MenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph'
		EXEC createPortalMenuItem @MenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining'
		
		--SELECT @csiPortalMenuPortalMenuDefinitionId = PortalMenuDefinitionId 
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiPortalMenu'

		--SELECT @csiMobileMenuPortalMenuDefinitionId = PortalMenuDefinitionId 
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiMobileMenu'

		--SELECT @csiPortalMenuV8PortalMenuDefinitionId = PortalMenuDefinitionId 
		--FROM portalmenuDefinition 
		--WHERE PortalMenuDefinitionName = 'csiPortalMenuV8'

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
