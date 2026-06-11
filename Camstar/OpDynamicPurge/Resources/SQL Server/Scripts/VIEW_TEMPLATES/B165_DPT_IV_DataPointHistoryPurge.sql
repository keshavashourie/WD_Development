IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_IV_DataPointHistoryPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_IV_DataPointHistoryPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_IV_DataPointHistoryPurge
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 12 Sep 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the DataPointHistory parent table based on the setup retentionperiod.
-- Copyright Siemens 2025  
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
WITH vResults AS 
(	SELECT TOP 50
	dph.DataPointHistoryId, 
	dph.HistoryMainLineId, 
	dph.HistoryId, 
	hml.TxnDate 
	FROM $(DatabaseName).$(SchemaName).DataPointHistory dph 
	INNER JOIN $(DatabaseName).$(SchemaName).HistoryMainLine hml ON dph.HistoryMainLineId = hml.HistoryMainLineId
	WHERE 
	(
		dph.DatapointHistoryId IN (':vInstanceValue')
	)
	ORDER BY hml.TxnDate, hml.HistoryMainLineId
)
SELECT DISTINCT DataPointHistoryId, HistoryMainLineId, HistoryId, TxnDate from vResults
GO
