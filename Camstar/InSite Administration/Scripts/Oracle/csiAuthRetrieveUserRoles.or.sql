--------------------------------------------------------------------------------
-- SCRIPT: csiAuthRetrieveUserRoles.or.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
DECLARE
   I NUMBER;
   v_sql VARCHAR2(512);
BEGIN
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'RESOLVEDPERMISSIONS_TEMP';
   IF I=0 THEN     
      v_sql:='CREATE GLOBAL TEMPORARY TABLE RESOLVEDPERMISSIONS_TEMP ' ||
                '(rolename                      VARCHAR2(255), ' ||
                'permissionname                 VARCHAR2(255), ' ||
                'permissiontype                 NUMBER, ' ||
                'permissionmode                 NUMBER, ' ||
                'organizationname               VARCHAR2(255), ' ||
                'objectmetaid                   NUMBER, ' ||
                'objectinstanceid               CHAR(16)) ' ||
            'ON COMMIT PRESERVE ROWS ';
       EXECUTE IMMEDIATE v_sql;
   END IF;
END;
/

CREATE OR REPLACE PROCEDURE CSIAUTHRETRIEVEUSERROLES
  ( cur_OUT OUT SYS_REFCURSOR, UserName IN varchar2
    ) AS
--
-- Copyright Siemens 2023  
--
   CURSOR cStarterRole IS
      SELECT  er.OrganizationId
			 ,r.RoleName
			 ,NVL(er.PropagateToChildOrgs,0) PropagateToChildOrgs  
      FROM  Employee e
		   ,EmployeeRole er
		   ,RoleDef r
      WHERE UPPER(e.EmployeeName)=UPPER(UserName)
      AND er.EmployeeId=e.EmployeeId
      AND r.RoleId=er.RoleId;
      
   v_OrganizationName  VARCHAR2(255);
BEGIN
   DELETE FROM RESOLVEDPERMISSIONS_TEMP;
   FOR crec IN cStarterRole LOOP
      IF (NOT crec.OrganizationId IS NULL ) THEN
         SELECT OrganizationName
         INTO v_OrganizationName
         FROM Organization
         WHERE OrganizationId=crec.OrganizationId;
         
         IF (crec.PropagateToChildOrgs=1) THEN
            INSERT INTO RESOLVEDPERMISSIONS_TEMP(RoleName,OrganizationName) 
		       SELECT crec.RoleName, OrganizationName 
               FROM Organization
               WHERE OrganizationName != v_OrganizationName
               CONNECT BY NOCYCLE PRIOR OrganizationId = ParentOrganizationId
               START WITH OrganizationName=v_OrganizationName;
         END IF; 
      ELSE
         v_OrganizationName := NULL;
      END IF;
      INSERT INTO RESOLVEDPERMISSIONS_TEMP(RoleName,OrganizationName) VALUES(crec.RoleName, v_OrganizationName);
   END LOOP;

   OPEN cur_OUT FOR SELECT RoleName,OrganizationName FROM RESOLVEDPERMISSIONS_TEMP;
END;
/

