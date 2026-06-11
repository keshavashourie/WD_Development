DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('CDODefinitionOrderedView') 
	) THEN
		DROP VIEW IF EXISTS CDODefinitionOrderedView CASCADE;
	END IF;
END $$;

CREATE VIEW CDODefinitionOrderedView AS 
WITH RECURSIVE cdos(ChildLevel,CDODefID,  
	ParentCDOID,  
	CDOName,  
	CDODescription,  
	ObjectsToCache,  
	ReadOverride,  
	WriteOverride,  
	SecurityTypeID,  
	DefaultTableID,  
	CDOUsageMask,  
	MaintenanceTypeID,  
	RevisionTypeId,  
	EnforceIntegrity,  
	UseInstanceSecurity,  
	UIDetailsId,  
	IsAbstract,  
	IsExposedToClientUI,  
	DisplayNameLabelId,  
	SelValQueryDefID) AS 
( 
	SELECT  
		1 As ChildLevel, 
		CDODefID,  
		ParentCDOID,  
		CDOName,  
		CDODescription,  
		ObjectsToCache,  
		ReadOverride,  
		WriteOverride,  
		SecurityTypeID,  
		DefaultTableID,  
		CDOUsageMask,  
		MaintenanceTypeID,  
		RevisionTypeId,  
		EnforceIntegrity,  
		UseInstanceSecurity,  
		UIDetailsId,  
		IsAbstract,  
		IsExposedToClientUI,  
		DisplayNameLabelId,  
		SelValQueryDefID 
	FROM  
	CDODefinition 
	WHERE CDOName = 'BaseObject' 
	
	UNION ALL 
	
	SELECT  
		cdos.ChildLevel + 1 As ChildLevel, 
		child.CDODefID,  
		child.ParentCDOID,  
		child.CDOName,  
		child.CDODescription,  
		child.ObjectsToCache,  
		child.ReadOverride,  
		child.WriteOverride,  
		child.SecurityTypeID,  
		child.DefaultTableID,  
		child.CDOUsageMask,  
		child.MaintenanceTypeID,  
		child.RevisionTypeId,  
		child.EnforceIntegrity,  
		child.UseInstanceSecurity,  
		child.UIDetailsId,  
		child.IsAbstract,  
		child.IsExposedToClientUI,  
		child.DisplayNameLabelId,  
		child.SelValQueryDefID 
	FROM CDODefinition AS child, cdos 
	WHERE cdos.CDODefId = child.ParentCDOID
) 
SELECT * FROM cdos;


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('CLFEventMapOrderedView') 
	) THEN
		DROP VIEW IF EXISTS CLFEventMapOrderedView;
	END IF;
END $$;
CREATE VIEW CLFEventMapOrderedView 
(
	CDOName, ChildLevel, CLFEventMapID, CallerID, CDODefID, CLFEventID, 
	CLFID, Name, Description, PermissionMask, WorkspaceCode, WorkspaceSequence
) AS 
(
	SELECT CDOView.CDOName,
		CDOView.ChildLevel,
		CLFEventMap.CLFEventMapID,   
		CLFEventMap.CallerID,   
		CLFEventMap.CDODefID,   
		CLFEventMap.CLFEventID,   
		CLFEventMap.CLFID,   
		CLFEventMap.Name,   
		CLFEventMap.Description,   
		CLFEventMap.PermissionMask,
		Workspace.WorkspaceCode,
		Workspace.Sequence
	FROM CDODefinitionOrderedView cdoview, CLFEventMap, workspace
	WHERE Workspace.IsActive = 1
	AND CLFEventMap.CDODefID = cdoview.CDODefid
	AND clfeventmap.WorkspaceCode = Workspace.WorkspaceCode
);


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('ResourceFactory') 
	) THEN
		DROP VIEW IF EXISTS ResourceFactory;
	END IF;
END $$;
CREATE VIEW ResourceFactory 
AS
-- not using Factory Level
SELECT
	null as EquipmentName, null as CellName, null as AreaName, r.ResourceName, f.FactoryName, f.FactoryId, r.ResourceId
FROM 
	ResourceDef r
	join Factory f on f.FactoryId = r.FactoryId
WHERE
	r.FactoryLevel is null
	
UNION

-- Factory Level = 1(Area)
SELECT
	null as EquipmentName, null as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, area.ResourceId
FROM 
	ResourceDef area
	join Factory f on f.FactoryId = area.FactoryId
WHERE
	area.FactoryLevel = 1

UNION

-- Factory Level = 2(Cell)
SELECT
	null as EquipmentName, cell.ResourceName as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, cell.ResourceId
FROM 
	ResourceDef cell
	join ResourceDef area on area.ResourceId = cell.ParentResourceId
	join Factory f on f.FactoryId = area.FactoryId
WHERE
	cell.FactoryLevel = 2

UNION

-- Factory Level = 3(Equipment)
SELECT
	eq.ResourceName as EqipmentName, cell.ResourceName as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, eq.ResourceId
FROM 
	ResourceDef eq
	join ResourceDef cell on cell.ResourceId = eq.ParentResourceId
	join ResourceDef area on area.ResourceId = cell.ParentResourceId
	join Factory f on f.FactoryId = area.FactoryId
WHERE
	eq.FactoryLevel = 3;