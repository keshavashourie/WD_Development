IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_RunLogPivot' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_RunLogPivot AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_RunLogPivot
AS

	WITH vtAllStages AS 
	(
		SELECT 
			t.SetupName, 
			t.BatchExecutionId, 
			t.TableName, 
			t.gLevel, 
			t.RunStage,
			t.RecordsAffected,
			(t.ExecutionTime/1000) AS ExecutionTimeInSec
		FROM CSI_PURGEUTIL_RUNLOG t 
		WHERE t.SetupName = ':vSetupName' 
			AND t.RunStage IN ('PREPAREDATA', 'ARCHIVEDATA', 'DELETEDATA', 'RESTOREDATA')
	),
	vtProcessType AS 
	(
		SELECT 
			SetupName,
			CASE 
				WHEN COUNT(CASE WHEN RunStage = 'RESTOREDATA' THEN 1 END) > 0 THEN 'RESTORE'
				ELSE 'PURGE'
			END AS ProcessType
		FROM vtAllStages
		GROUP BY SetupName
	),
	vtPivotData AS 
	(
		SELECT 
			a.SetupName,
			a.BatchExecutionId,
			a.TableName,
			a.gLevel,
			p.ProcessType,
			SUM(CASE WHEN a.RunStage = 'PREPAREDATA' THEN a.RecordsAffected END) AS PrepareData_Records,
			SUM(CASE WHEN a.RunStage = 'ARCHIVEDATA' THEN a.RecordsAffected END) AS ArchiveData_Records,
			SUM(CASE WHEN a.RunStage = 'DELETEDATA' THEN a.RecordsAffected END) AS DeleteData_Records,
			SUM(CASE WHEN a.RunStage = 'RESTOREDATA' THEN a.RecordsAffected END) AS RestoreData_Records,
			SUM(CASE WHEN a.RunStage = 'PREPAREDATA' THEN a.ExecutionTimeInSec END) AS PrepareData_Time,
			SUM(CASE WHEN a.RunStage = 'ARCHIVEDATA' THEN a.ExecutionTimeInSec END) AS ArchiveData_Time,
			SUM(CASE WHEN a.RunStage = 'DELETEDATA' THEN a.ExecutionTimeInSec END) AS DeleteData_Time,
			SUM(CASE WHEN a.RunStage = 'RESTOREDATA' THEN a.ExecutionTimeInSec END) AS RestoreData_Time
		FROM vtAllStages a
		JOIN vtProcessType p ON a.SetupName = p.SetupName
		GROUP BY a.SetupName, a.BatchExecutionId, a.TableName, a.gLevel, p.ProcessType
	)
	SELECT TOP 2000
		SetupName AS "Setup Name",
		BatchExecutionId,
		TableName,
		gLevel AS "Table Level",
		ProcessType AS "Process Type",
		-- Show relevant columns based on process type
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(PrepareData_Records AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "PREPAREDATA: Records Affected",
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(ArchiveData_Records AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "ARCHIVEDATA: Records Affected",
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(DeleteData_Records AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "DELETEDATA: Records Affected",
		CASE 
			WHEN ProcessType = 'RESTORE' THEN COALESCE(CAST(RestoreData_Records AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "RESTOREDATA: Records Affected",
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(PrepareData_Time AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "PREPAREDATA: Execution Time in Sec",
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(ArchiveData_Time AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "ARCHIVEDATA: Execution Time in Sec",
		CASE 
			WHEN ProcessType = 'PURGE' THEN COALESCE(CAST(DeleteData_Time AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "DELETEDATA: Execution Time in Sec",
		CASE 
			WHEN ProcessType = 'RESTORE' THEN COALESCE(CAST(RestoreData_Time AS VARCHAR), 'N/A')
			ELSE 'N/A'
		END AS "RESTOREDATA: Execution Time in Sec"
	FROM vtPivotData
	ORDER BY SetupName, gLevel, TableName

GO
