IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_IV_MfgOrderPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_IV_MfgOrderPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_IV_MfgOrderPurge
AS
--===========================================================================
-- Author      : Dan Maloney
-- Release Date: 06 May 2025
--===========================================================================
-- Description:
-- View used to fetch MfgOrdereIds to purge from MfgOrder and related tables for a given MfgOrderId that has no associated containers. 
-- Additional commented logic is in the view definition that may be useful when modifying a setup user view created from this OOB template 
-- view is shown in the comments.
--
-- Change purging criteria in the WHERE clause based on your requirements in the setup user view created from this OOB template view
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
SELECT TOP 50 mo.MfgOrderId, mo.MfgOrderName, cs.LastChangeDate, mo.PlannedCompletionDate
FROM $(DatabaseName).$(SchemaName).MfgOrder mo
INNER JOIN $(DatabaseName).$(SchemaName).ChangeStatus cs ON mo.ChangeStatusId = cs.ChangeStatusId
WHERE mo.MfgOrderId IN (':vInstanceValue')
-- This OOB template view uses a NOT EXISTS clause so that manufacturing orders that link to container records are excluded from eligibility
-- to be purged. You may modify the setup user view created from this OOB template view and comment out the AND NOT EXISTS clause and the 
-- subquery is the NOT EXIST clause if you wish for the setup user view, created from this OOB template view, to purge manufacturing orders 
-- that have associated containers. In this case, you could add HTML entries to the application.config file to link the container table to the 
-- MfgOrder table on the MfgOrderId column that exists in both tables.  
-- Making such a modification to this view and application.config file would allow you to do a DETECT in the setup configuration page and purge 
-- manufacturing orders, related containers and all related Container tables and MfgOrder tables.
AND NOT EXISTS
	(SELECT ContainerId 
	FROM $(DatabaseName).$(SchemaName).Container con
	WHERE con.MfgOrderId = mo.MfgOrderId)
ORDER BY cs.LastChangeDate, mo.MfgOrderId
GO
