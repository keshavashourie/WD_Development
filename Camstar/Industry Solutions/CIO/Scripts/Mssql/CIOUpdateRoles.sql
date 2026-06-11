--------------------------------------------------------------------------------
-- SCRIPT:CIOUpdateRoles.sql
-- DESCR: Creates stored procedures used to add CIO Roles for 
--		  portal pages and maint txns
-- Copyright Siemens 2019 


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Creates a Permission, assigns it to a Role and creates Modes
--
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
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180, 230)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (110, 180, 230)

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


GO



-- amh this, too, exists OOB

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
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


--amh this exists OOB

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
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
-- PROCEDURE: csiRBACAddCMVirtualPagePermissions 
-- DESCR: Creates permissions for all virtual pages
--		  @RoleId - The role to which the permissions are added
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
-- DESCR: Add permissions for CIO Portal Pages and Maint txns
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
		
	

	EXEC csiRBACDeleteUnavailablePermissions 230, 'CIO User Query Maint'

	-- Each of the following blocks represent a query-based Role/Permission assignment for default services
    PRINT('Creating "Default Modeling" Role...');
    SELECT @RoleId = RoleId from RoleDef where Rolename = 'Default Modeling'
    SET @PermissionSQL = '110'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
	--amh I'm not sure about the value 7140, in the IS install scripts
	--some maint txns had different values which makes no sense to me
	PRINT('Creating "Default Modeling" Role List Processor...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO List Processor Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	PRINT('Creating "Default Modeling" Role Outbound Msg Def...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO Outbound Msg Def Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	PRINT('Creating "Default Modeling" Role Post Query Portal Page Maint...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO Query Portal Page Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	PRINT('Creating "Default Modeling" Role Settings Maint...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO Settings Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	PRINT('Creating "Default Modeling" Role Template Main...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO Template Maint', 180, 7140, @PermissionModesFlag_NoSecAdmin
	PRINT('Creating "Default Modeling" Role CIO User Query Maint...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO User Query Maint', 230, 4727012, @PermissionModesFlag_ReadOnly
	PRINT('Complete "Default Modeling" Role...');

	PRINT('Creating "Default Modeling Read-Only" Role...');
    SELECT @RoleId = RoleId from RoleDef where Rolename = 'Default Modeling Read-Only'
	PRINT('Creating "Default Modeling Read-Only" Role CIO User Query Maint...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO User Query Maint', 230, 4727012, @PermissionModesFlag_ReadOnly

	PRINT('Creating "Default Modeling Advanced" Role...');
    SELECT @RoleId = RoleId from RoleDef where Rolename = 'Default Modeling Advanced'
	PRINT('Creating "Default Modeling Advanced" Role CIO User Query Maint...');
	EXEC csiRBACCreatePermission @RoleId, 'CIO User Query Maint', 230, 4727012, @PermissionModesFlag_NoSecAdmin

	PRINT('Creating "Default Inq" Role...');
	SELECT @RoleId = RoleId from RoleDef where Rolename = 'Default Inquiry'
	EXEC csiRBACCreatePermission @RoleId, 'CIOInquiry', 140, 4726863, @PermissionModesFlag_None
	PRINT('Complete "CIO Inquiry" Role...');	

	PRINT('Creating "Default Pages" Role...');
	SELECT @RoleId = RoleId from RoleDef where Rolename = 'Default Pages'
	EXEC csiRBACAssignRole @RoleId, 'Default Portal Pages Role','CamstarAdmin', NULL, 0
	EXEC csiRBACAddCMVirtualPagePermissions @RoleId


	-- IS used 8143 and 7755 - -I have no idea why
	EXEC csiRBACCreatePermission @RoleId, 'CIO_ExpressionbuilderpopupVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_ListProcessorVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_OutboundMessageDefVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_QueryPortalPageMaintVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_QueryPortalPageVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_Settings_VP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_TemplateVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_UserQueryVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_OutboundConnectionVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_GenericRESTAPIVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_OpcenterConnectMOMVP', 110, 7755, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CIO_OutboundMessageDefVP2', 110, 7755, @PermissionModesFlag_None
	PRINT('Complete "Default Pages" Role...');
	



DELETE FROM SecurityCacheRefreshRequest

INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (47,GETDATE(),GETUTCDATE(),N'Default Pages',2)
INSERT INTO SecurityCacheRefreshRequest (SecurityCacheRefreshRequestId, CreatedDate, CreatedDateGMT, EntityName, EntityType) VALUES (49,GETDATE(),GETUTCDATE(),N'Default Modeling',2)


	COMMIT TRAN
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'AddCIOSessionInstance' 
	   AND 	  type = 'P')
    DROP PROCEDURE AddCIOSessionInstance
GO
CREATE PROCEDURE AddCIOSessionInstance
AS
BEGIN
	DECLARE @InstanceId varchar(16)
	DECLARE @Name  varchar(30)
	DECLARE @CDOTypeId int
	DECLARE @URL  varchar(156)
	DECLARE @Timeout int
	DECLARE @Description varchar(256)

	SET @Name = 'ciosettings'
	SET @CDOTypeId = 4726796
	SET @URL = 'https://127.0.0.1:13024/ClientGateway/gateway'
	SET @Timeout = 5

	IF NOT EXISTS (SELECT * FROM CIOSettings WHERE CIOSettingsName = @Name)
	BEGIN
		PRINT('Inserting CIO SEttings: ' + @Name);

		-- Get next InstanceID if one is not provided.
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT

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
			(@InstanceId        
			,@Name			
			,@CDOTypeId         
			,1                 
			,0                 
			,@Description      
			,@URL			
			,@Timeout
			,0                 
			,0);               

	END
	ELSE
		PRINT('cioSettings ' + @Name + ' already exists');

END
GO



EXEC populatePortalMenuUpdateData
GO
DROP PROCEDURE populatePortalMenuUpdateData
GO
EXEC AddCIOSessionInstance
DROP PROCEDURE AddCIOSessionInstance
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CIO_CheckInboundMessage' 
	   AND 	  type = 'P')
    DROP PROCEDURE CIO_CheckInboundMessage
GO

CREATE  PROCEDURE CIO_CheckInboundMessage
	@RetMsgName as VARCHAR(255) OUTPUT,
	@RetMsgType as VARCHAR(255) OUTPUT,
	@AdapterName as VARCHAR(250) OUTPUT
AS
BEGIN TRY
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	Declare @XMLResult xml;
	Declare @TxnId NVARCHAR(50) = '';

	BEGIN TRAN		
		BEGIN TRY
			BEGIN TRAN
				SELECT TOP(1) @TxnId = DictKey
					, @RetMsgName = MessageName
					, @XMLResult = CAST(Contents AS XML)
					, @RetMsgType = MessageType
					, @AdapterName = Destination
				FROM CIOMessage WITH (UPDLOCK)
				WHERE OKtoDelete = 0
				AND Owners = 0
				ORDER BY MessageTimeStampGMT;

				IF LEN(@TxnID) > 0
					BEGIN
						UPDATE CIOMessage SET Owners = 1 
						WHERE DictKey = @TxnId;
					END; 

			COMMIT TRAN 			
		END TRY
		BEGIN CATCH
			ROLLBACK TRAN
			RETURN
		END CATCH

	
	COMMIT TRAN
	-- return the XML in the stored procedure resultset
	SELECT 	@XMLResult	

	RETURN
END TRY
	
	BEGIN CATCH
	  -- if any one thing went wrong, then roll back all updates and inserts
	  IF @@TRANCOUNT > 0
		 ROLLBACK;

	  -- then return the error message to the calling application
	  DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int;
	  SELECT @ErrMsg = ERROR_MESSAGE(),
			 @ErrSeverity = ERROR_SEVERITY();

	  RAISERROR(@ErrMsg, @ErrSeverity, 1);
	END CATCH
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CIO_InboundMessageReceived' 
	   AND 	  type = 'P')
    DROP PROCEDURE CIO_InboundMessageReceived
GO

CREATE  PROCEDURE CIO_InboundMessageReceived
	@MsgName AS NVARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRAN		
		BEGIN TRY
			UPDATE CIOMessage SET OKtoDelete = 1 WHERE MessageName = @MsgName;
		END TRY
		BEGIN CATCH
			ROLLBACK TRAN
			RETURN
		END CATCH
	
	COMMIT TRAN	
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CIO_WakeUpAdapter' 
	   AND 	  type = 'P')
    DROP PROCEDURE CIO_WakeUpAdapter
GO

CREATE PROCEDURE CIO_WakeUpAdapter
	@RecordCount AS Int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRAN		
		BEGIN TRY
			WITH CTE  AS
			(
				SELECT TOP (@RecordCount) *
				FROM CIOMessage
				WHERE OKtoDelete = 1
				ORDER BY MessageTimeStampGMT 
			)
			DELETE FROM CTE		   
		END TRY
		BEGIN CATCH
			ROLLBACK TRAN
			RETURN
		END CATCH
	
	COMMIT TRAN	
END
GO
