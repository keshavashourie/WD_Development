IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_RV_UserPermission' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_RV_UserPermission AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_RV_UserPermission
AS

WITH perms_cte as
(
     select USER_NAME(p.grantee_principal_id) AS principal_name,
                dp.principal_id,
                dp.type_desc AS principal_type_desc,
                p.class_desc,
                OBJECT_NAME(p.major_id) AS object_name,
                p.permission_name,
                p.state_desc AS permission_state_desc 
      from    sys.database_permissions p
        inner   JOIN sys.database_principals dp
        on     p.grantee_principal_id = dp.principal_id
)

--users
SELECT p.principal_name as "Principle Name", p.principal_type_desc as "Principle Type Description", p.class_desc as "Class Description", p.[object_name] as "Object Name", p.permission_name as "Permission Name", p.permission_state_desc as "Permission State Description", cast(NULL as sysname) as "Role Name"
FROM perms_cte p
WHERE principal_type_desc <> 'DATABASE_ROLE'
UNION

--role members
SELECT TOP 200 rm.member_principal_name as "Principle Name", rm.principal_type_desc as "Principle Type Description", p.class_desc as "Class Description", p.object_name as "Object Name", p.permission_name as "Permission Name", p.permission_state_desc as "Permission State Description",rm.role_name as "Role Name"
FROM perms_cte p
right outer JOIN (
    select role_principal_id, dp.type_desc as principal_type_desc, member_principal_id,user_name(member_principal_id) as member_principal_name,user_name(role_principal_id) as role_name--,*
    from    sys.database_role_members rm
    INNER   JOIN sys.database_principals dp
    ON     rm.member_principal_id = dp.principal_id
) rm

ON     rm.role_principal_id = p.principal_id
order by 1


GO
