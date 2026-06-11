IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_RV_SetupHierarchy' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_RV_SetupHierarchy AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_RV_SetupHierarchy
AS

SELECT TOP 2000 TableLevel as "Table Level",
    TableName as "Setup Table",
    InstanceCol1A as "Setup Table Instance Column",
    InstanceParentCol as "Parent Table Instance Column",
    ParentTable as "Parent Table"
FROM CSI_PURGEUTIL_SETUPTABLES st
    INNER JOIN CSI_PURGEUTIL_SETUP s ON st.SetupId = s.SetupId
WHERE SetupName = ':vSetupName'
ORDER BY TableLevel ASC, ParentTable ASC

GO
