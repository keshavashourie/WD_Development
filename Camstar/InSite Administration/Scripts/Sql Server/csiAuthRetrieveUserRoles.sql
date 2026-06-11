--------------------------------------------------------------------------------
-- SCRIPT: csiAuthRetrieveUserRoles.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAuthRetrieveUserRoles' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiAuthRetrieveUserRoles
GO

CREATE PROCEDURE csiAuthRetrieveUserRoles 
      @UserName nvarchar(100)
AS
--
-- Copyright Siemens 2023  
--
    DECLARE @c1 CURSOR
    DECLARE @RoleName nvarchar(255)
    DECLARE @PropagateToChildOrgs bit
    DECLARE @OrganizationId nvarchar(16)
    DECLARE @OrganizationName nvarchar(100)
    DECLARE @recOrganizationName nvarchar(100)
    DECLARE @cOrgs CURSOR
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    SET @c1 = CURSOR FOR
		SELECT er.OrganizationId,
			   r.RoleName,
			   er.PropagateToChildOrgs	   
		FROM Employee e,
			 EmployeeRole er,
			 RoleDef r
		WHERE e.EmployeeName=@UserName
		AND er.EmployeeId=e.EmployeeId
		AND r.RoleId=er.RoleId
		       

    CREATE TABLE #tmpresults
    (
        RoleName nvarchar(255),
        OrganizationName nvarchar(100)
    )

    OPEN @c1
    FETCH NEXT FROM @c1 INTO @OrganizationId, @RoleName, @PropagateToChildOrgs    
    WHILE (@@fetch_status=0)
    BEGIN
        IF (LEN(@OrganizationId)=0 Or @OrganizationId IS NULL)
		    SET @OrganizationName=""          
        ELSE
			SELECT @OrganizationName=OrganizationName
			FROM Organization
			WHERE OrganizationId=@OrganizationId 
			
			
        INSERT INTO #tmpresults VALUES (@RoleName,@OrganizationName)
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
			INSERT INTO #tmpresults SELECT DISTINCT @RoleName,Name FROM OrgHierarchy WHERE Name<>@OrganizationName  -- Don't re-insert the original Org

		END
        
        FETCH NEXT FROM @c1 INTO @OrganizationId, @RoleName,@PropagateToChildOrgs  
    END
    CLOSE @c1
    DEALLOCATE @c1

    SELECT * FROM #tmpresults

END

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
