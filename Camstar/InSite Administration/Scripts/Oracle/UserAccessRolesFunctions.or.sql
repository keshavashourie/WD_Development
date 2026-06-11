/* ==========================================================================
--  User Access Roles Query Functions
--     csiAuthGetAccessibleOrgs - Returns a list of Orgs for a given Employee
--     csiAuthGetAllowedUsers - Returns a list of users who have Role(s) in an Org
--	Copyright Siemens 2025
-- ========================================================================== */


BEGIN
 DROP_DATABASE_OBJECT ( 'OTAB_ORGANIZATION', 'TYPE' );
 
 DROP_DATABASE_OBJECT ( 'OTAB_EMPLOYEE', 'TYPE' );

 DROP_DATABASE_OBJECT ( 'OTAB_ROLEORG', 'TYPE' );
END;
/

CREATE OR REPLACE 
TYPE otyp_organization AS OBJECT (OrganizationId VARCHAR2(16), OrganizationName VARCHAR2(255))
/
CREATE OR REPLACE 
TYPE otab_organization AS TABLE OF otyp_Organization
/
CREATE OR REPLACE
FUNCTION CSIAUTHGETACCESSIBLEORGS( UserName IN varchar2 )
RETURN otab_Organization
PIPELINED
AS
--
-- Copyright Siemens 2023  
--
   CURSOR cEmpRole IS
      SELECT  er.OrganizationId
             ,NVL(er.PropagateToChildOrgs,0) PropagateToChildOrgs
      FROM  Employee e
           ,EmployeeRole er
      WHERE UPPER(e.EmployeeName)=UPPER(UserName)
      AND er.EmployeeId=e.EmployeeId;

   CURSOR cPropagate(pOrganizationName VARCHAR2) IS
      SELECT OrganizationId, OrganizationName
      FROM Organization
      WHERE OrganizationName != pOrganizationName
      CONNECT BY NOCYCLE PRIOR OrganizationId = ParentOrganizationId
      START WITH OrganizationName=pOrganizationName;
  
    -- Return a unique list of Orgs. This local table is used to determine
    -- if an Org has already been returned. 
    TYPE otype_UniqueOrg IS TABLE OF VARCHAR2(16) INDEX BY VARCHAR2(16); 
    otab_UniqueOrg otype_UniqueOrg;
     
    v_OrganizationName  VARCHAR2(255);
BEGIN
    FOR crec IN cEmpRole LOOP
      IF (NOT crec.OrganizationId IS NULL ) THEN
         SELECT OrganizationName
         INTO v_OrganizationName
         FROM Organization
         WHERE OrganizationId=crec.OrganizationId;

         IF (crec.PropagateToChildOrgs=1) THEN
            FOR crecOrg IN cPropagate(v_OrganizationName) LOOP
               IF (NOT otab_UniqueOrg.Exists(crecOrg.OrganizationId)) THEN
                  PIPE ROW(otyp_Organization(crecOrg.OrganizationId, crecOrg.OrganizationName));
                   otab_UniqueOrg(crecOrg.OrganizationId) := crecOrg.OrganizationId;
               END IF;
            END LOOP;
         END IF;
      ELSE
         v_OrganizationName := NULL;
      END IF;
      IF (v_OrganizationName IS NOT NULL) THEN
         IF (NOT otab_UniqueOrg.Exists(crec.OrganizationId)) THEN
             PIPE ROW(otyp_Organization(crec.OrganizationId, v_OrganizationName));
             otab_UniqueOrg(crec.OrganizationId) := crec.OrganizationId;
         END IF;
      END IF;
   END LOOP;

   RETURN;

END;
/

CREATE OR REPLACE 
TYPE otyp_Employee AS OBJECT (EmployeeId VARCHAR2(16), EmployeeName VARCHAR2(255))
/
CREATE OR REPLACE 
TYPE otab_Employee AS TABLE OF otyp_Employee
/
CREATE OR REPLACE
FUNCTION CSIAUTHGETALLOWEDUSERS( OrganizationName IN VARCHAR2, RoleName IN VARCHAR2 ) 
RETURN otab_Employee
PIPELINED
AS
--
-- Copyright Siemens 2023  
--
   CURSOR cEmpRoleOrg(pOrgName VARCHAR2, pRoleName VARCHAR2) IS
      SELECT  DISTINCT e.EmployeeId
             ,e.EmployeeName             
      FROM  EmployeeRole er
           ,Employee e
           ,RoleDef r
           ,Organization o
      WHERE e.EmployeeId=er.EmployeeId
      AND r.RoleId=er.RoleId
      AND (UPPER(r.RoleName)=UPPER(pRoleName) OR pRoleName IS NULL)
      AND (UPPER(o.OrganizationName)=UPPER(pOrgName) OR pOrgName IS NULL)
      AND o.OrganizationId IN (
          SELECT er.OrganizationId
          FROM DUAL
          UNION
          SELECT OrganizationId
          FROM Organization
          WHERE er.PropagateToChildOrgs=1
          CONNECT BY PRIOR OrganizationId=ParentOrganizationId
          START WITH OrganizationId=er.OrganizationId
          );
BEGIN    
    FOR crec IN cEmpRoleOrg(OrganizationName,RoleName) LOOP
       PIPE ROW(otyp_Employee(crec.EmployeeId, crec.EmployeeName));
    END LOOP;           
    
    RETURN;   
END;
/

CREATE OR REPLACE 
TYPE otyp_RoleOrg AS OBJECT (OrganizationId VARCHAR2(16), RoleId VARCHAR2(16))
/
CREATE OR REPLACE 
TYPE otab_RoleOrg AS TABLE OF otyp_RoleOrg
/
CREATE OR REPLACE
FUNCTION CSIAUTHGETUSERSROLES( OwnerId IN VARCHAR2 ) 
RETURN otab_RoleOrg
PIPELINED
AS
--
-- Copyright Siemens 2023  
--
   CURSOR cEmpRole IS
        -- Retrieve all relations between employees and roles where organization is not empty
        SELECT DISTINCT er.OrganizationId,
            r.RoleId,
            er.PropagateToChildOrgs
        FROM Employee e
        JOIN EmployeeRole er ON er.EmployeeId = e.EmployeeId
        JOIN RoleDef r ON r.RoleId = er.RoleId
        WHERE e.EmployeeId = OwnerId AND er.OrganizationId IS NOT NULL;

   CURSOR cPropagate(pOrganizationId VARCHAR2) IS
      SELECT OrganizationId
      FROM Organization
      CONNECT BY NOCYCLE PRIOR OrganizationId = ParentOrganizationId
      START WITH OrganizationId = pOrganizationId;
  
BEGIN
   FOR crec IN cEmpRole LOOP
      IF (crec.PropagateToChildOrgs = 1) THEN
          FOR crecOrg IN cPropagate(crec.OrganizationId) LOOP
             PIPE ROW(otyp_RoleOrg(crecOrg.OrganizationId, crec.RoleId));
          END LOOP;
      ELSE
          PIPE ROW(otyp_RoleOrg(crec.OrganizationId, crec.RoleId));
      END IF;
   END LOOP;

   RETURN;

END;
/

