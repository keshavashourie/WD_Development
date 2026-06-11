IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_RunLogHistorySummary' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_RunLogHistorySummary AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_RunLogHistorySummary
AS

SELECT TOP 200 * FROM (
SELECT 
    t.Setupname
    , t.BatchExecutionId
    , t.RunStage
 , t.RestoreId
    , SUM(t.ExecutionTime/1000) ExecutionTimeInSec
    , SUM(t.ExecutionTime/1000/60) ExecutionTimeInMin
    , SUM(t.RecordsAffected) RecordsAffected
    , (SELECT TOP 1 rlh.RecordsAffected FROM CSI_PURGEUTIL_RUNLOGHISTORY rlh 
    WHERE rlh.Setupname = ':vSetupName' 
           AND rlh.gLevel = 1 
           AND rlh.BatchExecutionId = t.BatchExecutionId
           AND rlh.RunStage=t.RunStage
           AND ISNULL(rlh.RestoreId, 1) = ISNULL(t.RestoreId, 1) 
     ) BatchSize 
FROM CSI_PURGEUTIL_RUNLOGHISTORY t
WHERE t.SetupName = ':vSetupName'  
GROUP BY t.Setupname, t.BatchExecutionId, t.RunStage, t.RestoreId
HAVING t.RunStage IN ('PREPAREDATA', 'VALIDATEDATA', 'ARCHIVEDATA', 'DELETEDATA', 'RESTOREDATA')
 ) t2
ORDER BY t2.Setupname
, t2.BatchExecutionId desc
, CASE 
WHEN t2.RunStage = 'PREPAREDATA' THEN 1
WHEN t2.RunStage = 'VALIDATEDATA' THEN 2
WHEN t2.RunStage = 'ARCHIVEDATA' THEN 3
WHEN t2.RunStage = 'DELETEDATA' THEN 4
WHEN t2.RunStage = 'RESTOREDATA' THEN 5
END				

GO