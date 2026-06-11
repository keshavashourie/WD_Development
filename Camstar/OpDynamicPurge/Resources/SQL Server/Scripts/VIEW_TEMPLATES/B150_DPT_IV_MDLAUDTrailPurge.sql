IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_IV_MDLAUDTrailPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_IV_MDLAUDTrailPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_IV_MDLAUDTrailPurge 
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 30 Sep 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the Modeling Audit Trail related tables.
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
        math.ModelingAuditTrailHeaderId IN (':vInstanceValue')
    ORDER BY math.TxnDate, math.ModelingAuditTrailHeaderId 
)
SELECT DISTINCT ModelingAuditTrailHeaderId, ObjectTypeName, ObjectName, TxnDate, TxnDateGMT
FROM vResult
GO