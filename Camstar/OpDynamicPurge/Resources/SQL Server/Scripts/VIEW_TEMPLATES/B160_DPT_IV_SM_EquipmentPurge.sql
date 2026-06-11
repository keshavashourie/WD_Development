IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_IV_SM_EquipmentPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_IV_SM_EquipmentPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_IV_SM_EquipmentPurge 
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 12 Sep 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the equipment related tables.
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
SELECT TOP 50 
    hml.HistoryMainlineId, 
    hml.TxnDate,
    rd.ResourceId, 
    rd.Resourcename
FROM $(DatabaseName).$(SchemaName).ResourceDef rd
INNER JOIN $(DatabaseName).$(SchemaName).HistoryMainline hml ON hml.ResourceId = rd.ResourceId
-- Please change any purging criteria(s) in the WHERE clause based on your requirements.
WHERE 
	rd.ResourceId IN (':vInstanceValue')
ORDER BY hml.TxnDate, hml.HistoryMainlineId
GO
