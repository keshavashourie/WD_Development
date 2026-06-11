--------------------------------------------------------------------------------
-- SCRIPT: csiAuthRetrieveUserPermissions.or.sql
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
   IF (I=1) THEN 
      -- Attempt to delete the older version. If it's in use, this will fail but we
      -- want to continue regardless.
      v_sql := 'DROP TABLE RESOLVEDPERMISSIONS_TEMP';
      BEGIN
         EXECUTE IMMEDIATE v_sql;
      EXCEPTION
         WHEN OTHERS THEN NULL;
      END;         
   END IF;
   --
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'RESOLVEDPERMISSIONS_TEMPV2'; -- Note the V2.
   IF (I=0) THEN        
      v_sql:='CREATE GLOBAL TEMPORARY TABLE RESOLVEDPERMISSIONS_TEMPV2 ' ||
                '(rolename                      VARCHAR2(255), ' ||
                'permissionname                 VARCHAR2(255), ' ||
                'permissiontype                 NUMBER, ' ||
                'permissionmode                 NUMBER, ' ||
                'organizationname               VARCHAR2(255), ' ||
                'objectmetaid                   NUMBER, ' ||
                'objectinstanceid               CHAR(16), ' ||
                'objectcdoname                  VARCHAR2(255) ' ||
                ') ' ||
            'ON COMMIT PRESERVE ROWS ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
   --
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'RESOLVEDPERMISSIONS_TEMPV3'; -- Note the V3.
   IF (I=0) THEN        
      v_sql:='CREATE GLOBAL TEMPORARY TABLE RESOLVEDPERMISSIONS_TEMPV3 ' ||
                '(rolename                      VARCHAR2(255), ' ||
                'organizationname               VARCHAR2(255) ' ||
                ') ' ||
            'ON COMMIT PRESERVE ROWS ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
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
