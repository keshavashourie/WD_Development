--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetAccessibleOrgs.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAuthGetAccessibleOrgs' 
	   AND 	  type = 'TF')
    DROP FUNCTION csiAuthGetAccessibleOrgs
GO

CREATE FUNCTION csiAuthGetAccessibleOrgs ( @UserName nvarchar(100) )
RETURNS @retOrganization TABLE (OrganizationId nvarchar(16), OrganizationName nvarchar(255))
AS
--
-- Copyright Siemens 2023  
--
BEGIN

    DECLARE @c1 CURSOR
    DECLARE @PropagateToChildOrgs bit
    DECLARE @OrganizationId nvarchar(16)
    DECLARE @OrganizationName nvarchar(100)
    DECLARE @recOrganizationName nvarchar(100)
    DECLARE @cOrgs CURSOR
    DECLARE @tmpOrgs TABLE (OrganizationId nvarchar(16), OrganizationName nvarchar(255))

    SET @c1 = CURSOR FOR
		SELECT er.OrganizationId,
			   er.PropagateToChildOrgs	   
		FROM Employee e,
			 EmployeeRole er
		WHERE e.EmployeeName=@UserName
		AND er.EmployeeId=e.EmployeeId	
		       

    OPEN @c1
    FETCH NEXT FROM @c1 INTO @OrganizationId, @PropagateToChildOrgs    
    WHILE (@@fetch_status=0)
    BEGIN
        IF (LEN(@OrganizationId)>0)
        BEGIN
		    SELECT @OrganizationName=OrganizationName
			FROM Organization
			WHERE OrganizationId=@OrganizationId 
			
		    INSERT INTO @tmpOrgs VALUES (@OrganizationId, @OrganizationName)
			
           -- If the record has the PropagateToChildOrgs bit set, then copy this record for all (recursive) child orgs
           IF (@PropagateToChildOrgs=1)
           BEGIN
               WITH OrgHierarchy (OrganizationId,ParentOrganizationId, Name, Level)
			   AS
			   (
				-- Anchor member definition - The top-level parent
				SELECT o.OrganizationId, o.ParentOrganizationId,o.OrganizationName,
                      1 As Level
				FROM Organization o
				WHERE o.OrganizationName=@OrganizationName
				UNION ALL
				-- Recursive member definition - Children, grandchildren, etc.
				SELECT o.OrganizationId, o.ParentOrganizationId,o.OrganizationName,
                      ch.Level+1
				FROM Organization o, OrgHierarchy ch
				WHERE o.ParentOrganizationId = ch.OrganizationId
                AND ch.Level<10
			)
			INSERT INTO @tmpOrgs SELECT DISTINCT OrganizationId, Name FROM OrgHierarchy WHERE Name<>@OrganizationName  -- Don't re-insert the original Org
            END

         END
        
         FETCH NEXT FROM @c1 INTO @OrganizationId, @PropagateToChildOrgs     
    END
    CLOSE @c1
    DEALLOCATE @c1

    INSERT INTO @retOrganization SELECT DISTINCT OrganizationId,OrganizationName FROM @tmpOrgs
    RETURN
END
GO
--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetAllowedUsers.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAuthGetAllowedUsers' 
	   AND 	  type = 'TF')
    DROP FUNCTION csiAuthGetAllowedUsers
GO

CREATE FUNCTION csiAuthGetAllowedUsers ( @inOrganizationName nvarchar(255), @inRoleName nvarchar(255) )
RETURNS @retEmployee TABLE (EmployeeId nvarchar(16), EmployeeName nvarchar(255))
AS
--
-- Copyright Siemens 2023  
--
BEGIN

    DECLARE @c1 CURSOR
    DECLARE @PropagateToChildOrgs bit
    DECLARE @OrganizationId nvarchar(16)
    DECLARE @OrganizationName nvarchar(100)
    DECLARE @recOrganizationName nvarchar(100)
    DECLARE @EmployeeId nvarchar(16)
    DECLARE @EmployeeName nvarchar(100)
    DECLARE @RoleName nvarchar(255)
    DECLARE @tmpEmployee TABLE (EmployeeId nvarchar(16), EmployeeName nvarchar(255))

    SET @c1 = CURSOR FOR
		SELECT er.OrganizationId,
			   er.PropagateToChildOrgs,
               e.EmployeeId,
               e.EmployeeName,
               r.RoleName
		FROM Employee e,
			 EmployeeRole er,
             RoleDef r
		WHERE er.EmployeeId=e.EmployeeId	
        AND r.RoleId=er.RoleId
        AND (r.RoleName=@inRoleName OR LEN(@inRoleName)=0)
		       

    OPEN @c1
    FETCH NEXT FROM @c1 INTO @OrganizationId, @PropagateToChildOrgs, @EmployeeId, @EmployeeName, @RoleName 
    WHILE (@@fetch_status=0)
    BEGIN
        IF (LEN(@OrganizationId)>0)
        BEGIN
		    SELECT @OrganizationName=OrganizationName
			FROM Organization
			WHERE OrganizationId=@OrganizationId 
			
		    IF (@OrganizationName=@inOrganizationName OR LEN(@inOrganizationName)=0) 
                INSERT INTO @tmpEmployee VALUES (@EmployeeId, @EmployeeName)
			
           -- If the record has the PropagateToChildOrgs bit set, then copy this record for all (recursive) child orgs
           IF (@PropagateToChildOrgs=1 AND (@RoleName=@inRoleName OR LEN(@inRoleName)=0))
           BEGIN
               WITH OrgHierarchy (OrganizationId,ParentOrganizationId, OrganizationName, Level)
			   AS
			   (
				-- Anchor member definition - The top-level parent
				SELECT o.OrganizationId, o.ParentOrganizationId,o.OrganizationName,
                      1 As Level
				FROM Organization o
				WHERE o.OrganizationName=@OrganizationName
				UNION ALL
				-- Recursive member definition - Children, grandchildren, etc.
				SELECT o.OrganizationId, o.ParentOrganizationId,o.OrganizationName,
                      ch.Level+1
				FROM Organization o, OrgHierarchy ch
				WHERE o.ParentOrganizationId = ch.OrganizationId
                AND ch.Level<10
			)
			INSERT INTO @tmpEmployee 
               SELECT DISTINCT @EmployeeId, @EmployeeName 
               FROM OrgHierarchy 
               WHERE OrganizationName<>@OrganizationName  -- Don't re-insert the original Org
               AND (OrganizationName=@inOrganizationName OR LEN(@inOrganizationName)=0)               
            END

         END
        
        FETCH NEXT FROM @c1 INTO @OrganizationId, @PropagateToChildOrgs, @EmployeeId, @EmployeeName, @RoleName 
    END
    CLOSE @c1
    DEALLOCATE @c1

    INSERT INTO @retEmployee SELECT DISTINCT EmployeeId,EmployeeName FROM @tmpEmployee

    RETURN
END
GO
--------------------------------------------------------------------------------
-- SCRIPT: csiAuthGetUsersRoles.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAuthGetUsersRoles' 
	   AND 	  type = 'TF')
    DROP FUNCTION csiAuthGetUsersRoles
GO

CREATE FUNCTION csiAuthGetUsersRoles ( @OwnerId nvarchar(16) )
RETURNS @retTable TABLE (OrganizationId nvarchar(16), RoleId nvarchar(16))
AS
--
-- Copyright Siemens 2023  
--
BEGIN
    DECLARE @OrganizationId nvarchar(16)
    DECLARE @EmployeeId nvarchar(16)
    DECLARE @RoleId nvarchar(16)
    DECLARE @PropagateToChildOrgs bit

	DECLARE @RoleOrgs TABLE (OrganizationId nvarchar(16), RoleId nvarchar(16))

	DECLARE curEmployeeRole CURSOR FOR
		-- Retrieve all relations between employees and roles where organization is not empty
		SELECT DISTINCT er.OrganizationId,
			r.RoleId,
			er.PropagateToChildOrgs
		FROM Employee e
		JOIN EmployeeRole er ON er.EmployeeId = e.EmployeeId
		JOIN RoleDef r ON r.RoleId = er.RoleId
		WHERE e.EmployeeId = @OwnerId AND er.OrganizationId IS NOT NULL
	OPEN curEmployeeRole	
    FETCH NEXT FROM curEmployeeRole INTO @OrganizationId, @RoleId, @PropagateToChildOrgs 
    WHILE @@FETCH_STATUS = 0
    BEGIN

		IF (@PropagateToChildOrgs = 1)
		BEGIN

			WITH OrgHierarchy (OrganizationId, ParentOrganizationId, OrganizationName, Level)
			AS
			(
				-- Anchor member definition - The top-level parent
				SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, 1 As Level
				FROM Organization o
				WHERE o.OrganizationId = @OrganizationId
				UNION ALL
				-- Recursive member definition - Children, grandchildren, etc.
				SELECT o.OrganizationId, o.ParentOrganizationId, o.OrganizationName, ch.Level+1
				FROM Organization o, OrgHierarchy ch
				WHERE o.ParentOrganizationId = ch.OrganizationId AND ch.Level < 10
			)
			INSERT INTO @RoleOrgs SELECT DISTINCT OrganizationId, @RoleId
			FROM OrgHierarchy 
			
		END
		ELSE
		BEGIN
			INSERT INTO @RoleOrgs SELECT OrganizationId, @RoleId
			FROM Organization
			WHERE OrganizationId = @OrganizationId
		END
	
	
        FETCH NEXT FROM curEmployeeRole INTO @OrganizationId, @RoleId, @PropagateToChildOrgs
    END
    CLOSE curEmployeeRole
    DEALLOCATE curEmployeeRole

	INSERT INTO @retTable 
	SELECT DISTINCT OrganizationId, RoleId FROM @RoleOrgs

    RETURN
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
