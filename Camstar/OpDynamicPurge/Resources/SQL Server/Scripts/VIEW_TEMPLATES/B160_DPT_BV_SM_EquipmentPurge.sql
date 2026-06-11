IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_BV_SM_EquipmentPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_BV_SM_EquipmentPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_BV_SM_EquipmentPurge
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 12 Sep 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the equipment related tables.
-- Purge all equipment transactions (except the records in RESEOURCEDEF and PRODUCTIONSTATUS tables) 
--     if the difference (in days) between today AND the latest transaction date 
--     is more than the configured retention period.
-- Only applicable to SEMI Suite :
--     Check also that if there is an existing job running against this equipment,  
--     then ensure that the A_JOB.JobStatus must be 'COMPLETED' OR 'CANCELLED' before the equipment transactions can be purged.
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
SELECT DISTINCT TOP 50
    hml.HistoryMainlineId,
    hml.TxnDate,
    rd.Resourceid,
    rd.Resourcename, 
    cdod.CDOName
FROM $(DatabaseName).$(SchemaName).Resourcedef rd
INNER JOIN $(DatabaseName).$(SchemaName).ProductionStatus ps ON ps.resourceid = rd.resourceid
INNER JOIN $(DatabaseName).$(SchemaName).CDODefinition cdod ON cdod.CDODefID = rd.CDOTypeId
INNER JOIN $(DatabaseName).$(SchemaName).HistoryMainline hml ON hml.ResourceId = rd.ResourceId
WHERE 
    cdod.CDOName LIKE '%Equipment'  -- Only fetch equipments transactions
    AND hml.txndate < (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname=':vSetupName'))
    -- The statement "AND (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())...))" below is hardcoded in the equipment deletion stored procedure 
    --     and will delete all transaction records in the historymainline that are older than the configured retention period.
    --     AND ((DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - hml.txndate) > (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname='EQUIPMENT') 
    -- ps.jobcount is only applicable to SEMI Suite.
    -- If there is a job running against this equipment, jobcount will be set to 1.
    -- Once the job completes or is cancelled by the supervisor, then jobcount will be set back to 0.  
    -- AND ps.jobcount = 0
ORDER BY hml.TxnDate, hml.HistoryMainlineId
GO
/*
-- Use the following sql syntax to select equipment transactions for purging :
SELECT veps.* 
FROM DPT_BV_EquipmentPurge veps
ORDER BY veps.resourcename
GO
*/
