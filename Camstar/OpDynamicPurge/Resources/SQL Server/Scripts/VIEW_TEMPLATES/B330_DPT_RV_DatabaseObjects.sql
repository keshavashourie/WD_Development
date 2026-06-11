IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_RV_DatabaseObjects' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_RV_DatabaseObjects AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_RV_DatabaseObjects
AS

SELECT TOP 200 result.[Object Type Description], result.[Object Name], result.[Object Created On], result.[Object Modified On]
FROM (
	SELECT so.type_desc as "Object Name", so.name as "Object Type Description", so.create_date as "Object Created On", so.modify_date as "Object Modified On"
	FROM sys.objects so 
	WHERE UPPER(so.name) LIKE 'CSI_PURGEUTIL%' 
		OR UPPER(so.name) LIKE 'DPT_%' 

	UNION ALL

	SELECT 'USER_DEFINED_TABLE_TYPE' as "Object Name", tt.name as "Object Type Description", NULL as "Object Created On", NULL as "Object Modified On"
	FROM sys.table_types tt
	WHERE UPPER(tt.name) LIKE 'CSI_PURGEUTIL%' 
		OR UPPER(tt.name) LIKE 'DPT_%'
) result
ORDER BY result.[Object Type Description], result.[Object Name]

GO
