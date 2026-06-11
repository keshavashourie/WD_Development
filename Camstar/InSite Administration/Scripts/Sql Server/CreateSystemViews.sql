--------------------------------------------------------------------------------
-- SCRIPT: CDODefinitionOrderedView.sql
-- DESCR: Creates a CDO view that correctly handles parent/child ordering
--
-- Copyright Siemens 2023  


--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CDODefinitionOrderedView')
    DROP VIEW CDODefinitionOrderedView
GO
CREATE VIEW CDODefinitionOrderedView AS 
	WITH cdos(ChildLevel,CDODefID,  
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
	WHERE cdos.CDODefId = child.ParentCDOID) 
	SELECT * 
	FROM cdos
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CLFEventMapOrderedView')
    DROP VIEW CLFEventMapOrderedView
GO
CREATE VIEW CLFEventMapOrderedView
(
CDOName,
ChildLevel,
CLFEventMapID,   
CallerID,   
CDODefID,   
CLFEventID,   
CLFID,   
Name,   
Description,   
PermissionMask,
WorkspaceCode,
WorkspaceSequence
)
AS
(
select CDOView.CDOName,
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
  from CDODefinitionOrderedView cdoview, CLFEventMap, workspace
 where Workspace.IsActive = 1
   and CLFEventMap.CDODefID = cdoview.CDODefid
   and clfeventmap.WorkspaceCode = Workspace.WorkspaceCode
)
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'ResourceFactory' 
	   AND 	  type = 'V')
    DROP VIEW ResourceFactory
GO

CREATE VIEW ResourceFactory
AS
-- not using Factory Level
select
	null as EquipmentName, null as CellName, null as AreaName, r.ResourceName, f.FactoryName, f.FactoryId, r.ResourceId
from 
	ResourceDef r
	join Factory f on f.FactoryId = r.FactoryId
where
	r.FactoryLevel is null
UNION
-- Factory Level = 1(Area)
select
	null as EquipmentName, null as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, area.ResourceId
from 
	ResourceDef area
	join Factory f on f.FactoryId = area.FactoryId
where
	area.FactoryLevel = 1
UNION
-- Factory Level = 2(Cell)
select
	null as EquipmentName, cell.ResourceName as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, cell.ResourceId
from 
	ResourceDef cell
	join ResourceDef area on area.ResourceId = cell.ParentResourceId
	join Factory f on f.FactoryId = area.FactoryId
where
	cell.FactoryLevel = 2
UNION
-- Factory Level = 3(Equipment)
select
	eq.ResourceName as EqipmentName, cell.ResourceName as CellName, area.ResourceName as AreaName, null as ResourceName, f.FactoryName, f.FactoryId, eq.ResourceId
from 
	ResourceDef eq
	join ResourceDef cell on cell.ResourceId = eq.ParentResourceId
	join ResourceDef area on area.ResourceId = cell.ParentResourceId
	join Factory f on f.FactoryId = area.FactoryId
where
	eq.FactoryLevel = 3
GO


