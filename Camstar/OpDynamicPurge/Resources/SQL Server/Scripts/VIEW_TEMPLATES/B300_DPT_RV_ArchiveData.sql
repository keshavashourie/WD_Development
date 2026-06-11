IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_RV_ArchiveData' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_RV_ArchiveData AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_RV_ArchiveData
AS

SELECT TOP 2000 TableLevel as "Level",
    SetupName as "Setup Name",
    TableName as "Table Name",
    'SELECT ''' + st.TableName + ''' TableName,' + st.InstanceCol1a + ', * FROM :vArchiveDB.' + ':vArchiveSchema.' + st.tablename + ' WHERE batchexecutionid = (SELECT MAX(tbl.batchexecutionid) FROM ' + ':vArchiveDB.' + ':vArchiveSchema.' + s.MainTableName + ' tbl) ORDER BY Purge_Datetime;' "Select Statement"
FROM CSI_PURGEUTIL_SETUPTABLES st
     INNER JOIN CSI_PURGEUTIL_SETUP s ON st.SetupId = s.SetupId
WHERE SetupName = ':vSetupName'
ORDER BY TableLevel ASC, ParentTable ASC

GO
