IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_BV_ContainerPurge' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_BV_ContainerPurge AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_BV_ContainerPurge 
AS
--===========================================================================
-- Author      : Ni Jia, Yeow
-- Release Date: 16 Jul 2024
--===========================================================================
-- Description:
-- View used to fetch records to purge from the lot related tables.
-- Purge all lot transactions if 
--     1. Lot Status is Active.
--     2. The Lot is in the system for a period longer than the configured retention period.
--        based on the difference between GetDate() and the lot's LastActivityDate.
--	   3. If any illegible containers that have relations with the eligible container (direct AND indirect),
--		  the containers tree would not be purged.
-----------------------------------------------------------------------------
-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)
WITH vInitPurgeParent AS (SELECT TOP 50
        ctn.ContainerId,
        ctn.ContainerName,
        ctn.Status,
        ctn.LastActivityDate,
        ctn.SplitFromId,
		ctn.ParentContainerId,
		ctn.IssuedToContainerId,
		0 AS LEVEL,
        ctn.ContainerName as TREE
    FROM $(DatabaseName).$(SchemaName).Container ctn
	WHERE 
		(
			ctn.Status <> 1
            AND ctn.ParentContainerId IS NULL
			--AND ctn.OriginalFactoryId = (SELECT FactoryId FROM Factory WHERE FactoryName = 'Factory_1')
               AND (DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - ISNULL(ctn.LastActivityDate, ctn.FactoryStartDate)) >
                   (SELECT su.retentionperiod
                    FROM CSI_PURGEUTIL_SETUP su
                    WHERE su.setupname = ':vSetupName')
		)
    ORDER BY ctn.LastActivityDate, ctn.ContainerId
	UNION ALL
    /* Recursive member definition, get parent based on SplitFromId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vInitPurgeParent child ON child.SplitFromId = par.ContainerId
	UNION ALL
    /* Recursive member definition, get parent based on ParentContainerId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vInitPurgeParent child ON child.ParentContainerId = par.ContainerId
	UNION ALL
    /* Recursive member definition, get parent based on IssuedToContainerId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vInitPurgeParent child ON child.IssuedToContainerId = par.ContainerId
), vPurgeMaxLevelCTE AS (
    SELECT MAX(LEVEL) AS MaxLevel
    FROM vInitPurgeParent
	GROUP BY TREE
), vInitPurgeList AS (
	select 
		ipr.ContainerId,
        ipr.ContainerName,
        ipr.Status,
        ipr.LastActivityDate,
        ipr.SplitFromId,
		ipr.ParentContainerId,
		ipr.IssuedToContainerId,
		0 AS LEVEL,
		TREE
	from vInitPurgeParent ipr
	WHERE ipr.SplitFromId IS NULL AND ipr.ParentContainerId IS NULL AND ipr.IssuedToContainerId IS NULL
	OR LEVEL IN (SELECT MaxLevel FROM vPurgeMaxLevelCTE)
	UNION ALL
    /* Recursive member definition, get child based on SplitFromId*/
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(ContainerBySplitFromId))
    INNER JOIN vInitPurgeList par ON par.ContainerId = child.SplitFromId
	UNION ALL
    /* Recursive member definition, get child/related based on ParentContainerId (Associate) */
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(Container2))
    INNER JOIN vInitPurgeList par ON par.ContainerId = child.ParentContainerId
	UNION ALL
    /* Recursive member definition, get child/related based on IssueToContainerId (ComponentIssue) */
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(Container3))
    INNER JOIN vInitPurgeList par ON par.ContainerId = child.IssuedToContainerId
), vPurgeParent2 AS (
	SELECT 
		ctn.ContainerId,
        ctn.ContainerName,
        ctn.Status,
        ctn.LastActivityDate,
        ctn.SplitFromId,
		ctn.ParentContainerId,
		ctn.IssuedToContainerId,
		0 AS LEVEL,
		TREE
	FROM vInitPurgeList ctn
	UNION ALL
    /* Recursive member definition, get parent based on SplitFromId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vPurgeParent2 child ON child.SplitFromId = par.ContainerId
	UNION ALL
    /* Recursive member definition, get parent based on ParentContainerId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vPurgeParent2 child ON child.ParentContainerId = par.ContainerId
	UNION ALL
    /* Recursive member definition, get parent based on IssuedToContainerId*/
    SELECT
        par.ContainerId,
        par.ContainerName,
        par.Status,
        par.LastActivityDate,
        par.SplitFromId,
		par.ParentContainerId,
		par.IssuedToContainerId,
		LEVEL + 1 AS LEVEL,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container par WITH (INDEX(Container450))
    INNER JOIN vPurgeParent2 child ON child.IssuedToContainerId = par.ContainerId
), vPurgeParentChildCTE2 AS (
    SELECT MAX(LEVEL) AS HighestLevel
    FROM vPurgeParent2
	GROUP BY TREE
), vPurgeList2 AS (
	SELECT 
		pp2.ContainerId,
        pp2.ContainerName,
        pp2.Status,
        pp2.LastActivityDate,
        pp2.SplitFromId,
		pp2.ParentContainerId,
		pp2.IssuedToContainerId,
		TREE
	FROM vPurgeParent2 pp2
	WHERE pp2.SplitFromId IS NULL AND pp2.ParentContainerId IS NULL AND pp2.IssuedToContainerId IS NULL
	OR LEVEL IN (SELECT HighestLevel FROM vPurgeParentChildCTE2)
	UNION ALL
    /* Recursive member definition, get child based on SplitFromId*/
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(ContainerBySplitFromId)) 
    INNER JOIN vPurgeList2 par ON par.ContainerId = child.SplitFromId
	UNION ALL
    /* Recursive member definition, get child/related based on ParentContainerId (Associate) */
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(Container2))
    INNER JOIN vPurgeList2 par ON par.ContainerId = child.ParentContainerId
	UNION ALL
    /* Recursive member definition, get child/related based on IssueToContainerId (ComponentIssue) */
    SELECT
        child.ContainerId,
        child.ContainerName,
        child.Status,
        child.LastActivityDate,
        child.SplitFromId,
		child.ParentContainerId,
		child.IssuedToContainerId,
		TREE
    FROM $(DatabaseName).$(SchemaName).Container child WITH (INDEX(Container3))
    INNER JOIN vPurgeList2 par ON par.ContainerId = child.IssuedToContainerId
), vRemoveOpenContainers AS (
    SELECT DISTINCT TREE
    FROM vPurgeList2
	WHERE STATUS = 1
),vFinalPurgeList AS (
	SELECT
	 vPurgeList2.ContainerId,
     vPurgeList2.ContainerName,
     vPurgeList2.Status,
     vPurgeList2.LastActivityDate,
     vPurgeList2.SplitFromId,
	 vPurgeList2.ParentContainerId,
	 vPurgeList2.IssuedToContainerId,
	 vPurgeList2.TREE
	FROM vPurgeList2
	WHERE NOT EXISTS ( SELECT TREE FROM vRemoveOpenContainers WHERE vPurgeList2.TREE = vRemoveOpenContainers.TREE)
)
SELECT DISTINCT ContainerId , ContainerName, SplitFromId, ParentContainerId, IssuedToContainerId, Status, LastActivityDate FROM vFinalPurgeList 

--*/
GO
