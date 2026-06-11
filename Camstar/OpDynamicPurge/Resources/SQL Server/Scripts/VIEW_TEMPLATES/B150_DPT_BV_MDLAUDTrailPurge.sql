IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_BV_MDLAUDTrailPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_BV_MDLAUDTrailPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_BV_MDLAUDTrailPurge 
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 30 Sep 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the Modeling Audit Trail related tables.
-- Purge all ModelingAuditTrail transactions  
--     1. if the difference (in days) between today AND the latest transaction date is more than the configured retention period.
--        ie : The txndate is less than the cut-off date.
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
WITH vResult AS 
(   
    SELECT TOP 50
        math.ModelingAuditTrailHeaderId
        , math.ObjectTypeName
        , math.ObjectName
        , math.TxnDate
        , math.TxnDateGMT 
    FROM $(DatabaseName).$(SchemaName).ModelingAuditTrailHeader math 
    -- Please change any purging criteria(s) in the WHERE clause based on your requirements.
    WHERE 
        /* get the date portion only of today's date */
        (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - math.TxnDate > (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname=':vSetupName'))
    ORDER BY math.TxnDate, math.ModelingAuditTrailHeaderId 
)
SELECT DISTINCT ModelingAuditTrailHeaderId, ObjectTypeName, ObjectName, TxnDate, TxnDateGMT
FROM vResult
GO
