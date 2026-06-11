--------------------------------------------------------------------------------
-- SCRIPT: CDODefinitionOrderedView.sql
-- DESCR: Creates a CDO view that correctly handles parent/child ordering
--
--  Copyright Siemens 2023  

--------------------------------------------------------------------------------
CREATE OR REPLACE VIEW CDODefinitionOrderedView AS 
	SELECT Level ChildLevel,CDODefID,  
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
FROM CDODEFINITION
CONNECT BY PRIOR CDODEFID = PARENTCDOID
START WITH PARENTCDOID = 0
/

CREATE OR REPLACE VIEW CLFEventMapOrderedView AS 
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
	Workspace.Sequence as WorkspaceSequence
  from CDODefinitionOrderedView cdoview, CLFEventMap, workspace
 where Workspace.IsActive = 1
   and CLFEventMap.CDODefID = cdoview.CDODefid
   and clfeventmap.WorkspaceCode = Workspace.WorkspaceCode
/

CREATE OR REPLACE VIEW ResourceFactory
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
/
