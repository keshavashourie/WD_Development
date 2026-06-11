IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_IV_ResourceHistoryPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_IV_ResourceHistoryPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_IV_ResourceHistoryPurge 
AS
--===========================================================================
-- Author      : Dan Maloney
-- Release Date: 01 Apr 2025
--===========================================================================
-- Description:
-- View used to fetch HistoryMainlineIds to purge from HistoryMainline and related tables for a given resource (ResourceId)
-- that has records in ResourceStatusHistory or CompleteMaintenanceHistory
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
SELECT TOP 50 Result.HistoryMainLIneId, Result.TxnDate, Result.TxnServiceName, Result.ResourceId, Result.ResourceName
FROM
(
	SELECT HML.HistoryMainLIneId, HML.TxnDate, HML.TxnServiceName, RES.ResourceId, RES.ResourceName
	FROM $(DatabaseName).$(SchemaName).ResourceDef RES
	INNER JOIN $(DatabaseName).$(SchemaName).ResourceStatusHistory RSH
	ON RES.ResourceId = RSH.HistoryId
	INNER JOIN $(DatabaseName).$(SchemaName).HistoryMainline HML
	ON RSH.HistoryMainLineId  = HML.HistoryMainLIneId
	UNION
	SELECT HML.HistoryMainLIneId, HML.TxnDate, HML.TxnServiceName, RES.ResourceId, RES.ResourceName
	FROM $(DatabaseName).$(SchemaName).ResourceDef RES
	INNER JOIN $(DatabaseName).$(SchemaName).CompleteMaintenanceHistory CMH
	ON RES.ResourceId = CMH.HistoryId
	INNER JOIN $(DatabaseName).$(SchemaName).HistoryMainline HML
	ON CMH.HistoryMainLineId  = HML.HistoryMainLIneId
) Result
-- Please change any purging criteria(s) in the WHERE clause based on your requirements.
WHERE 
	Result.ResourceId IN (':vInstanceValue') 
	ORDER BY Result.TxnDate, Result.HistoryMainLIneId
GO
