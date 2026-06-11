IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_RunLogHistory' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_RunLogHistory AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_RunLogHistory
AS

SELECT TOP 2000 t.SetupName, t.BatchExecutionId, t.RunStage, t.Action, t.TableName, t.RecordsAffected, t.ExecutionTime, t.Creation_Datetime, t.SQLStatement
FROM CSI_PURGEUTIL_RUNLOGHISTORY t  
WHERE t.SetupName = ':vSetupName'
ORDER BY t.Creation_Datetime desc, t.batchexecutionid desc, t.RowId desc	

GO