IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_RunLogSummary' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_RunLogSummary AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_RunLogSummary
AS

SELECT TOP 200 
    t.Setupname as "SetupName", t.BatchExecutionId, t.RunStage as "Run Stage"
    , SUM(t.ExecutionTime/1000) "Execution Time in Sec"
    , SUM(t.ExecutionTime/1000/60) "Execution Time in Min"
    , SUM(t.RecordsAffected) "Records Affected"
    , (SELECT rl.RecordsAffected FROM CSI_PURGEUTIL_RUNLOG rl WHERE rl.Setupname = ':vSetupName' AND rl.gLevel = 1 AND rl.RunStage=t.RunStage ) "Batch Size" 
FROM CSI_PURGEUTIL_RUNLOG t
WHERE t.SetupName = ':vSetupName'  
GROUP BY t.Setupname, t.BatchExecutionId, t.RunStage
HAVING t.RunStage IN ('PREPAREDATA', 'VALIDATEDATA', 'ARCHIVEDATA', 'DELETEDATA', 'RESTOREDATA')
ORDER BY t.SetupName, t.BatchExecutionId desc, 
CASE 
WHEN t.RunStage = 'PREPAREDATA' THEN 1
WHEN t.RunStage = 'VALIDATEDATA' THEN 2
WHEN t.RunStage = 'ARCHIVEDATA' THEN 3
WHEN t.RunStage = 'DELETEDATA' THEN 4
WHEN t.RunStage = 'RESTOREDATA' THEN 5
END						


GO