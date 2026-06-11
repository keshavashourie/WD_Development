IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_RunLog' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_RunLog AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_RunLog
AS

SELECT TOP 2000 t.SetupName as "Setup Name", t.BatchExecutionId, t.RunStage as "Run Stage", t.Action, t.TableName, t.gLevel as "Table Level", t.RecordsAffected as "Records Affected"
    , (t.ExecutionTime/1000) "Execution Time in Sec"
    , t.Creation_Datetime as "Record Creation DateTime", t.SQLStatement as "SQL Statement"
FROM CSI_PURGEUTIL_RUNLOG t 
WHERE t.SetupName = ':vSetupName' 
ORDER BY t.SetupName, t.RowId

GO