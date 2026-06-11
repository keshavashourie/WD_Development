--------------------------------------------------------------------------------
-- SCRIPT: csiAuthRetrieveUserPermissions.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAuthRetrieveUserPermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiAuthRetrieveUserPermissions
GO

CREATE PROCEDURE csiAuthRetrieveUserPermissions 
      @UserName nvarchar(100)
AS
BEGIN  
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;          
	
	CREATE TABLE #tmpresults
    (  
        RoleName nvarchar(255),  
        OrganizationName nvarchar(100)
    )
 
	Insert into #tmpresults 
	EXEC csiAuthRetrieveUserRoles
		@UserName = @UserName;

		SELECT tmp.RoleName,  
			rp.RolePermissionName PermissionName,  
			rp.PermissionType,  
			rpm.Modes PermissionMode,  
			tmp.OrganizationName,  
			rp.ObjectMetaId,  
			rp.ObjectInstanceId,  
			ISNULL(cdo.CDOName,'System') ObjectCDOName 
		FROM #tmpresults tmp 
		JOIN RoleDef r ON r.RoleName=tmp.RoleName 
		JOIN RolePermission rp ON rp.RoleId=r.RoleId  
		JOIN RolePermissionModes rpm ON rpm.RolePermissionId=rp.RolePermissionId 
		LEFT OUTER JOIN CDODefinition cdo ON cdo.CDODefID=rp.ObjectMetaId    
  
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
