IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_BV_EventPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_BV_EventPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_BV_EventPurge 
AS
--===========================================================================
-- Author      : Benny Chia
-- Release Date: 29 Oct 2015
--===========================================================================
-- Description:
-- View used to fetch records to purge from the event related tables.
-- Purge all event transactions 
--     if the difference (in days) between today AND the event close date 
--     is more than the configured retention period.
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
WITH vResult AS 
(   SELECT TOP 50
    evt.EventId,
	evt.EventName,
	evt.BriefDescription,
	evt.OccurrenceDate,
	evt.DiscoveryArea,
	class.ClassificationName,
	subclass.SubClassificationName,
	org.OrganizationName as InitiatorOrganizationName,
	pl.PriorityLevelName,
	iorg.OrganizationName,
	evt.CloseDate
	FROM $(DatabaseName).$(SchemaName).Event evt 
	INNER JOIN $(DatabaseName).$(SchemaName).Classification class  ON evt.ClassificationId = class.ClassificationId
	INNER JOIN $(DatabaseName).$(SchemaName).SubClassification subclass ON evt.SubClassificationId = subclass.SubClassificationId
	INNER JOIN $(DatabaseName).$(SchemaName).Organization org ON evt.OrganizationId = org.OrganizationId
	INNER JOIN $(DatabaseName).$(SchemaName).PriorityLevel pl ON evt.PriorityLevelId = pl.PriorityLevelId
	INNER JOIN $(DatabaseName).$(SchemaName).Organization iorg ON evt.OrganizationId = iorg.OrganizationId
	-- Please change any purging criteria(s) in the WHERE clause based on your requirements.
	WHERE 
		evt.CloseDate IS NOT NULL AND (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - evt.CloseDate > (SELECT su.retentionperiod FROM CSI_PURGEUTIL_SETUP su WHERE su.setupname=':vSetupName'))
		ORDER BY evt.CloseDate, evt.EventId
)
SELECT DISTINCT EventId, EventName, BriefDescription, OccurrenceDate, DiscoveryArea, ClassificationName, SubClassificationName, InitiatorOrganizationName, PriorityLevelName, OrganizationName, CloseDate
FROM vResult
GO
