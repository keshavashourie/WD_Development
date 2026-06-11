--------------------------------------------------------------------------------
-- SCRIPT:PopulateSummaryTableDefaultData.sql
-- DESCR: Creates stored procedures used to create Summary Table Definitions
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2023  


--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewDefinition
-- DESCR: 
--
-- Copyright Siemens 2023  
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_CreateNewDefinition' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_CreateNewDefinition
GO
CREATE PROCEDURE csiSTInstall_CreateNewDefinition(@Name NVARCHAR(255)
                                                      ,@Description NVARCHAR(512)
                                                      ,@Notes NVARCHAR(512)
                                                      ,@SummarySQL NVARCHAR(MAX)
                                                      ,@IsView BIT
                                                      ,@TableName NVARCHAR(255)
                                                      ,@IsManuallyExecuted BIT
                                                      ,@ScheduleDaysOfWeek NVARCHAR(255)
                                                      ,@ScheduleDaysOfMonth NVARCHAR(255)
                                                      ,@ScheduleHours NVARCHAR(255)
                                                      ,@ScheduleMonths NVARCHAR(255)
                                                      ,@IsFrozen BIT
                                                      ,@IsEnabled BIT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CDODefId INT
    DECLARE @InstanceId CHAR(16)
    SET @CDODefId=8236
    
    DELETE FROM SummaryTableDef WHERE SummaryTableDefName = @Name

    EXEC csiPRDGetNextInstanceId @CDODefId,@InstanceId OUTPUT
    INSERT INTO SummaryTableDef(SummaryTableDefId
                               ,SummaryTableDefName
                               ,Description
                               ,Notes
                               ,SummarySQL
                               ,IsView
                               ,TargetTableName
                               ,IsManuallyExecuted
                               ,ScheduleDaysOfWeek
                               ,ScheduleDaysOfMonth
                               ,ScheduleHours
                               ,ScheduleMonths
                               ,IsFrozen
                               ,CDOTypeId
                               ,ChangeCount
                               ,IsEnabled)
       VALUES (@InstanceId
              ,@Name
              ,@Description
              ,@Notes
              ,@SummarySQL
              ,@IsView
              ,@TableName
              ,@IsManuallyExecuted
              ,@ScheduleDaysOfWeek
              ,@ScheduleDaysOfMonth
              ,@ScheduleHours
              ,@ScheduleMonths
              ,@IsFrozen
              ,@CDODefId
              ,0
              ,@IsEnabled)
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_PopulateDefaultData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_PopulateDefaultData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_PopulateDefaultData
GO
CREATE PROCEDURE csiSTInstall_PopulateDefaultData
AS
DECLARE @SQLString NVARCHAR(MAX)
BEGIN
    SET NOCOUNT ON;
    
   SET @SQLString=N'with t1 as '+
		'(SELECT	wfs2.workflowstepid, '+
		'mh2.historyid, '+
		'MIN(txndate) AS txndate '+
		'FROM movehistory mh2 '+
		'INNER JOIN historymainline hml2 ON hml2.historymainlineid = mh2.historymainlineid '+
		'and mh2.historyid = hml2.containerid '+
		'INNER JOIN workflowstep    wfs2 ON wfs2.workflowstepid = mh2.stepid '+
		'GROUP BY wfs2.workflowstepid, '+
		'mh2.historyid), '+
		'hml1 as (select historymainlineid, txndate, operationid, '+
		'employeeid, '+
		'basetxntype, '+
		'workflowstepid, '+
		'productid from historymainline hml where  hml.txndate >= DATEADD(day,-90,GETDATE())) '+
		'SELECT pb.productname, '+
		'p.productrevision, '+
		'wfs.workflowstepname, '+
		'wfs.sequence, '+
		'c.containername, '+
		't.txndate, '+
		'SUM(t.qty) qty, '+
		't.productid, '+
		't.txnid, '+
		't.historyid, '+
		't.historymainlineid, '+
		'cdodefinition.cdoname, '+
		'operation.operationname, '+
		'employee.employeename '+
		'FROM (SELECT mh.historyid, '+
		'mh.historymainlineid, '+
		'mh.txnid, '+
		'mh.tostepid AS workflowstepid, '+
		'mh.qty, '+
		'mh.productid, '+
		'hml1.txndate, '+
		'hml1.operationid, '+
		'hml1.employeeid, '+
		'hml1.basetxntype '+
		'FROM movehistory mh '+
		'INNER JOIN hml1 ON mh.historymainlineid = hml1.historymainlineid '+
		'INNER JOIN t1 ON t1.workflowstepid = mh.tostepid '+
		'AND t1.historyid = mh.historyid '+
		'AND t1.txndate = hml1.txndate '+
		'wHERE				mh.reworkreasonid IS NULL '+
		'UNION '+
		'SELECT qh.historyid, '+
		'qh.historymainlineid, '+
		'qh.txnid, '+
		'hml1.workflowstepid, '+
		'qh.qty, '+
		'hml1.productid, '+
		'hml1.txndate, '+
		'hml1.operationid, '+
		'hml1.employeeid, '+
		'hml1.basetxntype '+
		'FROM qtyhistory qh '+
		'INNER JOIN hml1 ON qh.historymainlineid = hml1.historymainlineid '+
		'INNER JOIN t1 ON t1.historyid = qh.historyid '+
		'AND hml1.txndate > t1.txndate '+
		'UNION '+
		'SELECT '+
		'starthistorydetail.historyid, '+
		'starthistorydetail.historymainlineid, '+
		'starthistorydetail.txnid, '+
		'starthistorydetail.workflowstepid, '+
		'starthistorydetail.qty, '+
		'hml1.productid, '+
		'hml1.txndate, '+
		'hml1.operationid, '+
		'hml1.employeeid, '+
		'hml1.basetxntype '+
		'FROM starthistorydetail '+
		'INNER JOIN hml1 ON starthistorydetail.historymainlineid = hml1.historymainlineid '+
		'UNION '+
		'SELECT mh.historyid, '+
		'mh.historymainlineid, '+
		'mh.txnid, '+
		'mh.stepid AS workflowstepid, '+
		'mh.qty, '+
		'mh.productid, '+
		'hml1.txndate, '+
		'hml1.operationid, '+
		'hml1.employeeid, '+
		'hml1.basetxntype '+
		'FROM movehistory mh '+
		'INNER JOIN  hml1 ON mh.historymainlineid = hml1.historymainlineid '+
		'INNER JOIN t1 ON t1.workflowstepid = mh.tostepid '+
		'AND t1.historyid = mh.historyid '+
		'AND hml1.txndate > t1.txndate '+
		'WHERE mh.cumulativereworkstepcount = 1 '+
		') t '+
		'INNER JOIN container       c ON t.historyid = c.containerid '+
		'INNER JOIN employee ON t.employeeid = employee.employeeid '+
		'INNER JOIN operation ON t.operationid = operation.operationid '+
		'INNER JOIN product         p ON t.productid = p.productid '+
		'INNER JOIN productbase     pb ON pb.productbaseid = p.productbaseid '+
		'INNER JOIN cdodefinition ON t.basetxntype = cdodefinition.cdodefid '+
		'INNER JOIN workflowstep    wfs ON t.workflowstepid = wfs.workflowstepid '+
		'INNER JOIN workflow        wf ON wfs.workflowid = wf.workflowid '+
		'GROUP BY pb.productname,	p.productrevision,	wfs.workflowstepname, wfs.sequence, c.containername, t.txndate, t.productid, t.txnid, t.historyid, t.historymainlineid, cdodefinition.cdoname, operation.operationname, employee.employeename'

   
	EXEC csiSTInstall_CreateNewDefinition N'csiYield'
	                                      ,N'Yield'
	                                      ,NULL
	                                      ,@SQLString
	                                      ,0
	                                      ,'Yield'
	                                      ,0
	                                      ,NULL
	                                      ,NULL
	                                      ,'0,4,8,12,16,20'
	                                      ,NULL
	                                      ,1
	                                      ,1
	                                     
	                                     
	                                     
	 SET @SQLString=N'SELECT '+
 'CdoDefinition.CDOName, '+
 'HistoryMainline.Txndate, '+
 'Container.ContainerName, '+
 'WorkflowStep.WorkflowStepName, '+
 'WorkflowStep.Sequence, '+
 'ProductBase.ProductName, '+
 'Product.ProductRevision, '+
 'Container.qty ContainerQty, '+
 'isnull(MoveHistory.qty,MoveInHistory.Qty) Qty, '+
 'isnull(Movehistory.CycleTime,MoveInHistory.CycleTime)*(24) Cycletime '+
 'FROM   '+
 '((( '+
 ' HistoryMainline HistoryMainline '+
 ' Inner Join CDODefinition on Historymainline.TxnType = CdoDefinition.CDODefID '+
 'LEFT OUTER JOIN MoveHistory on HistoryMainline.HistoryMainlineId = MoveHistory.HistoryMainlineId '+
 'LEFT OUTER JOIN MoveInHistory on HistoryMainline.HistoryMainlineId = MoveInHistory.HistoryMainlineId '+
 'INNER JOIN  Container Container ON HistoryMainline.ContainerId=Container.ContainerId) '+
 'INNER JOIN  Product Product ON HistoryMainline.ProductId=Product.ProductId) '+
 'INNER JOIN  WorkflowStep WorkflowStep ON HistoryMainline.WorkflowStepId=WorkflowStep.WorkflowStepId) '+
 'INNER JOIN  ProductBase ProductBase ON Product.ProductBaseId=ProductBase.ProductBaseId '+
 'Where '+
 ' HistoryMainline.ContainerId in '+
 ' ( SELECT '+
 'Container.ContainerId '+
 'FROM   '+
 '(( '+
 'HistoryMainline HistoryMainline '+
 'INNER JOIN MoveHistory MoveHistory ON HistoryMainline.HistoryMainlineId=MoveHistory.HistoryMainlineId) '+
 'INNER JOIN Container Container ON HistoryMainline.ContainerId=Container.ContainerId) '+
 'INNER JOIN WorkflowStep FinalStep ON MoveHistory.ToStepId=FinalStep.WorkflowStepId '+
 'WHERE  finalstep.IsLastStep=1 '+
 'and  HistoryMainline.TxnDate >= DATEADD(day,-6,getdate())) '
                                      
	                                     
	                                     
	                                     
	EXEC csiSTInstall_CreateNewDefinition N'csiProductionDashboard'
	                                      ,N'ProductionDashboard'
	                                      ,NULL
	                                      ,@SQLString
	                                      ,0
	                                      ,'ProductionDashboard'
	                                      ,0
	                                      ,NULL
	                                      ,NULL
	                                      ,'0,4,8,12,16,20'
	                                      ,NULL
	                                      ,1
	                                      ,1
	                                      
SET @SQLString=N'SELECT   DATEADD(dd, 0, DATEDIFF(dd, 0, TxnDate)) AS TxnDate,  ProductName, ProductRevision,  WorkflowStepName, Sequence,  SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) AS Move,  SUM(CASE WHEN CDOName IN (''Rework'', ''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END) AS Loss,  CASE WHEN SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) > 0 THEN ((SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) + SUM(CASE WHEN CDOName IN (''Rework'',       ''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END)) / SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END)) ELSE 0 END AS Yield FROM  csiTbl_Yield where DATEADD(dd, 0, DATEDIFF(dd, 0, TxnDate))>=DATEADD(day,-6,getdate())  GROUP BY  DATEADD(dd, 0, DATEDIFF(dd, 0, TxnDate)),  ProductName,  ProductRevision,  Sequence, WorkflowStepName '


	EXEC csiSTInstall_CreateNewDefinition N'csiFirstPassYield'
	                                      ,N' '
	                                      ,N' '
	                                      ,@SQLString
	                                      ,1
	                                      ,'FirstPassYield'
	                                      ,0
	                                      ,NULL
	                                      ,NULL
	                                      ,NULL
	                                      ,NULL
	                                      ,1
	                                      ,0


SET @SQLString=N'SELECT Event.EventID InstanceID,Event.EventName Name,Event.EventDataID, '+
				'Event.BriefDescription, Event.Description, Event.ReportedDate ReportedDate, '+
				'Event.AttachmentsId, EventLot.Lot,EventLot.Qty, '+
				'(select CategoryField.FieldName from CDOFields CategoryField where Event.Category=CategoryField.DefaultValue  '+
				'AND CategoryField.CDODefId=7520 ) as CategoryName, '+
				'Event.Category,Classification.ClassificationName Classification, SubClassification.SubClassificationName SubClassification, '+
				'Event.DiscoveryArea,Event.OccurrenceDate OccurrenceDate, Organization.OrganizationName Organization,Owner.EmployeeName Owner , '+
				'(select PriorityLevel.PriorityLevelName PriorityLevel from PriorityLevel where  Event.PriorityLevelId=PriorityLevel.PriorityLevelId) as PriorityLevel, '+
				'Reporter.EmployeeName Reporter,ReporterOrganization.OrganizationName ReporterOrganization,Initiator.EmployeeName Initiator, '+
				'InitiatorOrganization.OrganizationName InitiatorOrganization, '+
				'(select Role.RoleName  from RoleDef Role  where Event.RoleId=Role.RoleId ) as Role, '+
				'(select StatusField.FieldName from CDOFields StatusField where Event.Status=StatusField.DefaultValue  '+
				'AND StatusField.CDODefId=7658) as StatusName, '+
				'Event.Status, '+
				'(select QualityResolutionCode.QualityResolutionCodeName from QualityResolutionCode 
				where Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId) as ResolutionCode, '+
				'Event.CloseDescription CloseDescription,Event.CloseDate CloseDate, ClosedBy.EmployeeName ClosedBy,EventData.ProductName EventDataProduct, '+
				'EventData.ProductRev EventDataProductRev, EventLot.ProductName EventLotProduct,EventLot.ProductRev EventLotProductRev, '+
				'EventData.MaintenanceReqName MaintenanceReqName,  EventData.OperationName OperationName,EventData.ResourceName ResourceName, '+
				'EventData.WorkflowName,Eventdata.WorkflowRev, EventData.WorkflowStepName,EventData.EventDate,FailureMode.FailureModeName, '+
				'EventFailure.FailureModeId,EventFailure.EventFailureId, '+
				'(select ProductFamilyName From ProductFamily, Product, ProductBase '+
				'where  Product.ProductFamilyId = ProductFamily.ProductFamilyID and  Product.ProductBaseId=ProductBase.ProductBaseId '+
				'and  EventData.ProductName = Productbase.ProductName  And EventData.ProductRev = Product.ProductRevision) as ProductFamilyName '+
				'FROM  event  '+
				'inner JOIN  EventData ON Event.EventId=EventData.EventId  '+
				'LEFT OUTER JOIN  EventFailure on eventdata.EventDataId = EventFailure.EventDataId  '+
				'LEFT OUTER JOIN FailureMode on EventFailure.FailureModeId = FailureMode.FailureModeId  '+
				'inner JOIN  Classification ON Event.ClassificationId =Classification.ClassificationId  '+
				'inner JOIN  SubClassification ON Event.SubClassificationId =SubClassification.SubClassificationId   '+
				'LEFT OUTER JOIN  Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId  '+
				'LEFT OUTER JOIN  Organization ON Event.OrganizationId=Organization.OrganizationId  '+
				'LEFT OUTER JOIN  Employee Owner ON Event.OwnerId=Owner.EmployeeId  '+
				'inner JOIN  Employee Reporter ON Event.ReporterId=Reporter.EmployeeId  '+
				'inner JOIN  Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId  '+
				'inner JOIN  Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId  '+
				'inner JOIN  Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId '+
				'left outer join  EventLot on Event.EventDataId = EventLot.EventDataId '+
				'where  convert(date, Event.ReportedDate, 101) >= CONVERT(date, getdate()-90, 101)'


EXEC csiSTInstall_CreateNewDefinition N'csiQualityObject'
	                                      ,N'QualityObject'
	                                      ,NULL
	                                      ,@SQLString
	                                      ,0
	                                      ,'QualityObject'
	                                      ,0
	                                      ,NULL
	                                      ,NULL
	                                      ,'0,4,8,12,16,20'
	                                      ,NULL
	                                      ,1
	                                      ,1
	                                      	                                    

SET @SQLString = N'SELECT     c.ContainerId, c.ContainerName, c.FactoryStartQty, c.Qty AS ContainerCurrentQty, c.OnHoldDate, c.LastActivityDate, UOM.UOMName, cs.InRework, '+ 
                      'wfs.IsLastStep, CASE WHEN wfs.IsLastStep = 1 THEN cs.LastMoveDate END AS MoveToLastStepDate, CASE WHEN wfs.IsLastStep = 1 THEN DATEDIFF(ss,  '+
                      'c.factoryStartDate, cs.lastMoveDate) / c.Qty END AS secondsPerPiece, CASE WHEN wfs.IsLastStep = 1 THEN DATEDIFF(ss, c.factoryStartDate, cs.lastMoveDate)  '+
                      'END AS secondsPerContainer, c.HoldReasonId, wfs.WorkflowStepName, MfgOrder.MfgOrderId, MfgOrder.MfgOrderName,  '+
                      'MfgOrder.PlannedCompletionDate, MfgOrder.DefaultLot, OrderStatus.OrderStatusName,  '+
                      'MfgOrder.PlannedStartDate, MfgOrder.Qty AS OrderQty, pb.ProductName, p.ProductRevision, p.ProductId,  '+
                      'ISNULL(CASE WHEN wfs.IsLastStep = 1 THEN c.Qty END, 0) AS ContainerCompletedQty, ISNULL(CASE WHEN wfs.IsLastStep = 1 THEN c.FactoryStartQty END, 0)  '+
                      'AS CompletedStartQty '+
                 'FROM         Container AS c INNER JOIN '+
                      'CurrentStatus AS cs ON c.CurrentStatusId = cs.CurrentStatusId INNER JOIN '+
                      'WorkflowStep AS wfs ON cs.WorkflowStepId = wfs.WorkflowStepId INNER JOIN '+
                      'MfgOrder ON c.MfgOrderId = MfgOrder.MfgOrderId LEFT OUTER JOIN '+
                      'OrderStatus ON MfgOrder.OrderStatusId = OrderStatus.OrderStatusId LEFT OUTER JOIN '+
                      'PriorityCode ON MfgOrder.PriorityId = PriorityCode.PriorityCodeId INNER JOIN '+
                      'UOM ON c.UOMId = UOM.UOMId INNER JOIN '+
                      'Product AS p ON c.ProductId = p.ProductId INNER JOIN '+
                      'ProductBase AS pb ON p.ProductBaseId = pb.ProductBaseId '+
                 'WHERE     (MfgOrder.MfgOrderId IN '+
                 '         (SELECT DISTINCT Container.MfgOrderId '+
                 '           FROM          Container INNER JOIN '+
                 '                                  CurrentStatus ON Container.CurrentStatusId = CurrentStatus.CurrentStatusId INNER JOIN '+
                 '                                  WorkflowStep ON CurrentStatus.WorkflowStepId = WorkflowStep.WorkflowStepId '+
                 '           WHERE      (WorkflowStep.IsLastStep = 0))) '+
				 'AND convert(date,  c.LastActivityDate, 101) >= CONVERT(date, getdate()-90, 101)'
                 
EXEC csiSTInstall_CreateNewDefinition N'csiOpenOrdersSummary', NULL, NULL, @SQLString, 1, 'OpenOrdersSummary', 0, NULL, NULL, '0,', NULL, 0,1   

-- Note: Without the convert(nvarchar(max), N''), the string is treated as a regular nvarchar(4000). The prepended concat forces it to nvarchar(max).
SET @SQLString = convert(nvarchar(max), N'') + N'/*--Get the beginning qty at each step except first step*/ '+ 
    'WITH cont_data AS ( '+
				'SELECT '+
				'p.productid           AS productid, '+
				'pb.productname        productname, '+
				'p.productrevision     productrevision, '+
				'wfs.workflowstepname  workflowstepname, '+
				'wfs.islaststep        islaststep, '+
				'wfs.workflowstepid    workflowstepid, '+
				'wfs.sequence          sequence, '+
				'wf.workflowid         workflowid, '+
				'c.containername       containername, '+
				'hml.resourceid        resourceid, '+
				'hml.txndate           txndate, '+
				'cdodefinition.cdoname AS cdoname, '+
				'c.ownerid             ownerid, '+
				'c.mfgorderid          mfgorderid, '+
				'hml.factoryid         factoryid, '+
				'hml.shiftname         shiftname, '+
				'hml.calendarshiftid   calendarshiftid, '+
				'hml.steppass          steppass, '+
				'''''         AS reasoncodeid, '+
				'sd.normalcycletime    AS targetcycletime, '+
				'sd.yield              AS targetyield, '+
				'sd.unitsperhour       AS targetunitsperhour, '+
				'c.containerid         AS containerid, '+
				'hml.historymainlineid AS historymainlineid, '+
				'hml.historyid         AS historyid '+
				'FROM '+
				'container c '+
				'INNER JOIN  historymainline      hml ON hml.containerid = c.containerid '+
				'INNER JOIN  workflowstep         wfs ON hml.workflowstepid = wfs.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid '+
				'INNER JOIN  product              p ON hml.productid = p.productid '+
				'INNER JOIN  productbase          pb ON pb.productbaseid = p.productbaseid '+
				'INNER JOIN  cdodefinition ON hml.basetxntype = cdodefinition.cdodefid '+
				'INNER JOIN  workflow             wf ON wfs.workflowid = wf.workflowid '+
				'WHERE convert(date, hml.txndate, 101) >= CONVERT(date, getdate()-90, 101)) '+
				'SELECT '+
				'movehistory.qty            qty, '+
				'movehistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'wfs2.workflowstepname      AS workflowstepname, '+
				'wfs2.islaststep, '+
				'wfs2.workflowstepid        AS stepid, '+
				'wfs2.sequence              AS stepsequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'cont_data.historyid, '+
				'hml1.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'movehistory.toresourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'cont_data.workflowstepname AS fromstep, '+
				'cont_data.workflowstepid   AS fromstepid, '+
				'cont_data.sequence         AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''              AS reasoncodeid, '+
				'sd2.normalcycletime        AS targetcycletime, '+
				'cont_data.targetyield      AS targetyield, '+
				'sd2.unitsperhour           AS targetunitsperhour, '+
				'GETDATE()                    AS lastrefreshdate '+
				'FROM '+
				'cont_data,  movehistory '+
				'INNER JOIN  historymainline      hml1 ON movehistory.historymainlineid = hml1.historymainlineid '+
				'INNER JOIN  workflowstep         wfs2 ON movehistory.tostepid = wfs2.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '+
				'WHERE	movehistory.reworkreasonid IS NULL '+
				'AND movehistory.historyid = cont_data.containerid '+
				'and MoveHistory.HistoryMainlineId = cont_data.historymainlineid '+
				'AND movehistory.stepid = cont_data.workflowstepid '+
				'AND wfs2.sequence <> 1 '+
				'UNION  /*--Get Scrap Qtys*/ '+
				'SELECT '+
				'qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier qty, '+
				'0                                            AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'qtyhistory.historyid, '+
				'qtyhistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'qtyhistory.resourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'''''                                           AS fromstep, '+
				'''''                                           AS fromstepid, '+
				'''''                                         AS fromstepsequence, '+
				'cont_data.steppass, '+
				'qtyhistorydetails.reasoncodeid                          AS reasoncodeid, '+
				'''''                                         AS targetcycletime, '+
				'cont_data.targetyield                                   AS targetyield, '+
				'''''                                         AS targetunitsperhour, '+
				'GETDATE()                                                 AS lastrefreshdate '+
				'FROM '+
				'cont_data,  qtyhistory '+
				'INNER JOIN  qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid '+
				'WHERE '+
				'qtyhistory.historyid = cont_data.containerid '+
				'AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid '+
				'AND qtyhistory.historymainlineid = cont_data.historymainlineid '+
				'UNION  /*--Get first workflow Steps starting qty from start history */ '+
				'SELECT '+
				'isnull(starthistorydetail.qty, 0) qty, '+
				'0                   AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'starthistorydetail.historyid, '+
				'starthistorydetail.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'starthistorydetail.resourceid, '+
				'starthistorydetail.mfgorderid, '+
				'cont_data.factoryid,
				cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'''''                  AS fromstep, '+
				'''''                  AS fromstepid, '+
				'''''                AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''                  AS reasoncodeid, '+
				'''''                AS targetcycletime, '+
				'cont_data.targetyield          AS targetyield, '+
				'''''                AS targetunitsperhour, '+
				'GETDATE()                        AS lastrefreshdate '+
				'FROM '+
				'cont_data, '+
				'starthistorydetail '+
				'WHERE '+
				'starthistorydetail.historyid = cont_data.containerid '+
				'AND starthistorydetail.historymainlineid = cont_data.historymainlineid '+
				'AND starthistorydetail.workflowstepid = cont_data.workflowstepid '+
				'UNION  /*--Get the Reworked Qty*/ '+
				'SELECT '+
				'( movehistory.qty ) * - 1  qty, '+
				'movehistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'movehistory.historyid, '+
				'movehistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'movehistory.toresourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'wfs2.workflowstepname      AS fromstep, '+
				'wfs2.workflowstepid        AS fromstepid, '+
				'wfs2.sequence              AS fromstepsequence, '+
				'cont_data.steppass, '+
				'movehistory.reworkreasonid AS reasoncodeid, '+
				'sd2.normalcycletime        AS targetcycletime, '+
				'cont_data.targetyield      AS targetyield, '+
				'sd2.unitsperhour           AS targetunitsperhour, '+
				'GETDATE()                    AS lastrefreshdate '+
				'FROM '+
				'cont_data,  movehistory '+
				'INNER JOIN  workflowstep         wfs2 ON movehistory.stepid = wfs2.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '+
				'WHERE '+
				'movehistory.reworkreasonid IS NOT NULL '+
				'AND movehistory.historyid = cont_data.containerid '+
				'AND movehistory.stepid = cont_data.workflowstepid '+
				'AND movehistory.historymainlineid = cont_data.historymainlineid '+
				'UNION  /*--Get the first pass movein qty*/ '+
				'SELECT '+
				'moveinhistory.qty            qty, '+
				'moveinhistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'moveinhistory.historyid, '+
				'moveinhistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'cont_data.resourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'cont_data.workflowstepname   AS fromstep, '+
				'cont_data.workflowstepid     AS fromstepid, '+
				'cont_data.sequence           AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''                AS reasoncodeid, '+
				'cont_data.targetcycletime    AS targetcycletime, '+
				'cont_data.targetyield        AS targetyield, '+
				'cont_data.targetunitsperhour AS targetunitsperhour, '+
				'GETDATE()                      AS lastrefreshdate '+
				'FROM '+
				'moveinhistory moveinhistory, '+
				'cont_data '+
				'WHERE '+
				'moveinhistory.historyid = cont_data.containerid '+
				'and moveinhistory.HistoryMainlineId = cont_data.historymainlineid' 

EXEC csiSTInstall_CreateNewDefinition N'csiFactoryPerformanceSummary', NULL, NULL, @SQLString, 0, 'FactoryPerformanceSum', 0, NULL, NULL, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,', NULL, 0,1 


SET @SQLString = convert(nvarchar(max), N'') + N'/*--Get the beginning qty at each step except first step*/ '+ 
    'WITH cont_data AS ( '+
				'SELECT '+
				'p.productid           AS productid, '+
				'pb.productname        productname, '+
				'p.productrevision     productrevision, '+
				'wfs.workflowstepname  workflowstepname, '+
				'wfs.islaststep        islaststep, '+
				'wfs.workflowstepid    workflowstepid, '+
				'wfs.sequence          sequence, '+
				'wf.workflowid         workflowid, '+
				'c.containername       containername, '+
				'hml.resourceid        resourceid, '+
				'hml.txndate           txndate, '+
				'cdodefinition.cdoname AS cdoname, '+
				'c.ownerid             ownerid, '+
				'c.mfgorderid          mfgorderid, '+
				'hml.factoryid         factoryid, '+
				'hml.shiftname         shiftname, '+
				'hml.calendarshiftid   calendarshiftid, '+
				'hml.steppass          steppass, '+
				'''''         AS reasoncodeid, '+
				'sd.normalcycletime    AS targetcycletime, '+
				'sd.yield              AS targetyield, '+
				'sd.unitsperhour       AS targetunitsperhour, '+
				'c.containerid         AS containerid, '+
				'hml.historymainlineid AS historymainlineid, '+
				'hml.historyid         AS historyid '+
				'FROM '+
				'container c '+
				'INNER JOIN  historymainline      hml ON hml.containerid = c.containerid '+
				'INNER JOIN  workflowstep         wfs ON hml.workflowstepid = wfs.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid '+
				'INNER JOIN  product              p ON hml.productid = p.productid '+
				'INNER JOIN  productbase          pb ON pb.productbaseid = p.productbaseid '+
				'INNER JOIN  cdodefinition ON hml.basetxntype = cdodefinition.cdodefid '+
				'INNER JOIN  workflow             wf ON wfs.workflowid = wf.workflowid '+
				'Where CONVERT(date, hml.txnDate, 101) = CONVERT(date, GETDATE(), 101) '+
				') '+
				'SELECT '+
				'movehistory.qty            qty, '+
				'movehistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'wfs2.workflowstepname      AS workflowstepname, '+
				'wfs2.islaststep, '+
				'wfs2.workflowstepid        AS stepid, '+
				'wfs2.sequence              AS stepsequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'cont_data.historyid, '+
				'hml1.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'movehistory.toresourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'cont_data.workflowstepname AS fromstep, '+
				'cont_data.workflowstepid   AS fromstepid, '+
				'cont_data.sequence         AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''              AS reasoncodeid, '+
				'sd2.normalcycletime        AS targetcycletime, '+
				'cont_data.targetyield      AS targetyield, '+
				'sd2.unitsperhour           AS targetunitsperhour, '+
				'GETDATE()                    AS lastrefreshdate '+
				'FROM '+
				'cont_data,  movehistory '+
				'INNER JOIN  historymainline      hml1 ON movehistory.historymainlineid = hml1.historymainlineid '+
				'INNER JOIN  workflowstep         wfs2 ON movehistory.tostepid = wfs2.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '+
				'WHERE	movehistory.reworkreasonid IS NULL '+
				'AND movehistory.historyid = cont_data.containerid '+
				'and MoveHistory.HistoryMainlineId = cont_data.historymainlineid '+
				'AND movehistory.stepid = cont_data.workflowstepid '+
				'AND wfs2.sequence <> 1 '+
				'UNION  /*--Get Scrap Qtys*/ '+
				'SELECT '+
				'qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier qty, '+
				'0                                            AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'qtyhistory.historyid, '+
				'qtyhistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'qtyhistory.resourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'''''                                           AS fromstep, '+
				'''''                                           AS fromstepid, '+
				'''''                                         AS fromstepsequence, '+
				'cont_data.steppass, '+
				'qtyhistorydetails.reasoncodeid                          AS reasoncodeid, '+
				'''''                                         AS targetcycletime, '+
				'cont_data.targetyield                                   AS targetyield, '+
				'''''                                         AS targetunitsperhour, '+
				'GETDATE()                                                 AS lastrefreshdate '+
				'FROM '+
				'cont_data,  qtyhistory '+
				'INNER JOIN  qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid '+
				'WHERE '+
				'qtyhistory.historyid = cont_data.containerid '+
				'AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid '+
				'AND qtyhistory.historymainlineid = cont_data.historymainlineid '+
				'UNION  /*--Get first workflow Steps starting qty from start history */ '+
				'SELECT '+
				'isnull(starthistorydetail.qty, 0) qty, '+
				'0                   AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'starthistorydetail.historyid, '+
				'starthistorydetail.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'starthistorydetail.resourceid, '+
				'starthistorydetail.mfgorderid, '+
				'cont_data.factoryid,
				cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'''''                  AS fromstep, '+
				'''''                  AS fromstepid, '+
				'''''                AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''                  AS reasoncodeid, '+
				'''''                AS targetcycletime, '+
				'cont_data.targetyield          AS targetyield, '+
				'''''                AS targetunitsperhour, '+
				'GETDATE()                        AS lastrefreshdate '+
				'FROM '+
				'cont_data, '+
				'starthistorydetail '+
				'WHERE '+
				'starthistorydetail.historyid = cont_data.containerid '+
				'AND starthistorydetail.historymainlineid = cont_data.historymainlineid '+
				'AND starthistorydetail.workflowstepid = cont_data.workflowstepid '+
				'UNION  /*--Get the Reworked Qty*/ '+
				'SELECT '+
				'( movehistory.qty ) * - 1  qty, '+
				'movehistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'movehistory.historyid, '+
				'movehistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'movehistory.toresourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'wfs2.workflowstepname      AS fromstep, '+
				'wfs2.workflowstepid        AS fromstepid, '+
				'wfs2.sequence              AS fromstepsequence, '+
				'cont_data.steppass, '+
				'movehistory.reworkreasonid AS reasoncodeid, '+
				'sd2.normalcycletime        AS targetcycletime, '+
				'cont_data.targetyield      AS targetyield, '+
				'sd2.unitsperhour           AS targetunitsperhour, '+
				'GETDATE()                    AS lastrefreshdate '+
				'FROM '+
				'cont_data,  movehistory '+
				'INNER JOIN  workflowstep         wfs2 ON movehistory.stepid = wfs2.workflowstepid '+
				'LEFT OUTER JOIN  stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '+
				'WHERE '+
				'movehistory.reworkreasonid IS NOT NULL '+
				'AND movehistory.historyid = cont_data.containerid '+
				'AND movehistory.stepid = cont_data.workflowstepid '+
				'AND movehistory.historymainlineid = cont_data.historymainlineid '+
				'UNION  /*--Get the first pass movein qty*/ '+
				'SELECT '+
				'moveinhistory.qty            qty, '+
				'moveinhistory.cycletime      AS movecycletime, '+
				'cont_data.productname, '+
				'cont_data.productrevision, '+
				'cont_data.productid, '+
				'cont_data.workflowstepname, '+
				'cont_data.islaststep, '+
				'cont_data.workflowstepid, '+
				'cont_data.sequence, '+
				'cont_data.workflowid, '+
				'cont_data.containername, '+
				'cont_data.txndate, '+
				'moveinhistory.historyid, '+
				'moveinhistory.historymainlineid, '+
				'cont_data.cdoname, '+
				'cont_data.ownerid, '+
				'cont_data.resourceid, '+
				'cont_data.mfgorderid, '+
				'cont_data.factoryid, '+
				'cont_data.shiftname, '+
				'cont_data.calendarshiftid, '+
				'cont_data.workflowstepname   AS fromstep, '+
				'cont_data.workflowstepid     AS fromstepid, '+
				'cont_data.sequence           AS fromstepsequence, '+
				'cont_data.steppass, '+
				'''''                AS reasoncodeid, '+
				'cont_data.targetcycletime    AS targetcycletime, '+
				'cont_data.targetyield        AS targetyield, '+
				'cont_data.targetunitsperhour AS targetunitsperhour, '+
				'GETDATE()                      AS lastrefreshdate '+
				'FROM '+
				'moveinhistory moveinhistory, '+
				'cont_data '+
				'WHERE '+
				'moveinhistory.historyid = cont_data.containerid '+
				'and moveinhistory.HistoryMainlineId = cont_data.historymainlineid'
    
EXEC csiSTInstall_CreateNewDefinition N'csi24HrFactoryPerformance', NULL, NULL, @SQLString, 1, '24HrFactoryPerformance', 0, NULL, NULL, '0,', NULL, 0,1 

SET @SQLString = convert(nvarchar(max), N'') + N'Select '+ 
    'wf.WorkflowId '+ 
    ',wf.WorkflowRevision '+ 
    ',wfb.WorkflowName '+ 
    ',wfs.WorkflowStepName '+ 
    ',wfs.workflowstepid '+ 
    ',s.SpecId '+ 
    ',r.ResourceName '+ 
    ',rg.ResourceGroupName  '+ 
    ',ps.MfgOrderId '+ 
    ',ps.LastActivityDate '+ 
    ',ps.LastStatusChangeDate '+ 
    ',CONVERT(varchar, DATEADD(s, DateDiff(second,ps.LastStatusChangeDate,getDate()), 0), 108) as TimeDown '+ 
    ',ps.ContainerId '+ 
    ',ps.ResourceId '+ 
    ',ps.ResourceState '+ 
    ',ps.Availability '+ 
    ',ps.ReasonId '+ 
    ',ps.StatusId '+ 
    ',rsc.ResourceStatusCodeName '+ 
    ',rsr.ResourceStatusReasonName '+ 
    'From '+ 
    'Workflow wf '+ 
    'inner join WorkflowBase wfb on wf.WorkflowBaseId = wfb.WorkflowBaseId  '+ 
    'inner join WorkflowStep wfs on wfs.WorkflowId = wf.WorkflowId '+ 
    'inner join Spec s on wfs.SpecId = s.SpecId or wfs.SpecBaseId = s.specbaseid '+ 
    'inner join ResourceGroup rg on s.resourcegroupid = rg.ResourceGroupId '+ 
    'Inner join ResourceGroupEntries rge on rg.ResourceGroupId=rge.ResourceGroupId '+ 
    'Inner join ResourceDef r on rge.EntriesId = r.ResourceId '+ 
    'Inner join ProductionStatus ps on r.ResourceId = ps.ResourceId '+ 
    'Left Outer Join ResourceStatusCode rsc on ps.StatusId =rsc.ResourceStatusCodeId '+ 
    'Left Outer Join ResourceStatusReason rsr on ps.ReasonId = rsr.ResourceStatusReasonId   '+
	'where  convert(date, ps.LastStatusChangeDate, 101) >= CONVERT(date, getdate()-90, 101)'
    
 EXEC csiSTInstall_CreateNewDefinition N'csiWFResourceDowntime', NULL, NULL, @SQLString, 1, 'csiWFResourceDowntime', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'sum(CycleTime)/COUNT(distinct ContainerName) Cycletime_Product, '+ 
    'sum(CycleTime) Cycletime, '+ 
    'MIN(ContainerQty) Qty, '+ 
    'sum(CycleTime)/MIN(ContainerQty) Throughput_Product, '+ 
    'ProductName, '+ 
    'containername '+ 
    'from csiTbl_ProductionDashboard  '+ 
    'Group by ProductName,containername '
 EXEC csiSTInstall_CreateNewDefinition N'csiCycleTimeProduct', N'Gets Product Cycle Time Information for Production Dashboard.', NULL, @SQLString, 1, 'CycleTimeProduct', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'WorkflowStepName, '+ 
    'Sequence, '+ 
    'COUNT(distinct ContainerName) ContainerCnt, '+ 
    'Sum(Qty) as Qty, '+ 
    'Sum(CycleTime) Cycletime, '+ 
    'sum(CycleTime)/COUNT(distinct ContainerName) as Cycletime_Step '+ 
    'From  '+ 
    'csiTbl_ProductionDashboard '+ 
    'group by  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'Sequence, '+ 
    'WorkflowStepName '
 EXEC csiSTInstall_CreateNewDefinition N'csiCycleTimeStep', N'Gets Step Cycle Time Information for Production Dashboard.', NULL, @SQLString, 1, 'CycleTimeStep', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'SELECT case when  Event.CloseDate = null then  ''Open'' else  ''Closed'' end MyStatus, '+  
												 'Event.ReportedDate EventDate,   Event.ReportedDate , Event.CloseDate, Event.EventId, '+  
												 'Event.EventName, FailureMode.FailureModeName,   NCRCauseCode.NCRCauseCodeName, '+  
												 ' FailureSeverity.FailureSeverityName  '+  
												 'FROM     ((((( Event Event   '+  
												 ' LEFT OUTER JOIN EventFailure EventFailure   ON Event.EventDataId=EventFailure.EventDataId)   '+  
												 'LEFT OUTER JOIN FailureMode FailureMode   ON EventFailure.FailureModeId=FailureMode.FailureModeId)   '+  
												 'LEFT OUTER JOIN FailureSeverity FailureSeverity   ON EventFailure.FailureSeverityId=FailureSeverity.FailureSeverityId)  '+  
												 'LEFT OUTER JOIN EventFailureCause EventFailureCause   ON EventFailure.EventFailureId=EventFailureCause.EventFailureId)   '+  
												 'LEFT OUTER JOIN NCRCauseCode NCRCauseCode   ON EventFailureCause.CauseCodeId=NCRCauseCode.NCRCauseCodeId)  '+  
												 'LEFT OUTER JOIN FailureSeverity FailureSeverity_Default   ON FailureMode.DefaultSeverityId=FailureSeverity_Default.FailureSeverityId '+
												 'where  convert(date, Event.ReportedDate, 101) >= CONVERT(date, getdate()-90, 101)'


 EXEC csiSTInstall_CreateNewDefinition N'csiEventTrend', N'Used for the Event Trend report.', NULL, @SQLString, 1, 'EventTrend', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'SELECT Event.EventID InstanceID, '+
 'Event.EventName Name, '+
 'Event.BriefDescription, '+
 'Event.Description, '+
 'Event.ReportedDateGMT ReportedDate, '+
 'EventLot.Lot, '+
 'EventLot.Qty, '+
 '(select CategoryField.FieldName from CDOFields CategoryField where Event.Category=CategoryField.DefaultValue AND CategoryField.CDODefId=7520 ) CategoryName, '+
 'Event.Category,  '+
 'Classification.ClassificationName Classification,  '+
 'SubClassification.SubClassificationName SubClassification,  '+
 'Event.DiscoveryArea, '+
 'Event.OccurrenceDateGMT OccurrenceDate,  '+
 'Organization.OrganizationName Organization,  '+
 'Owner.EmployeeName Owner, '+
 '(select PriorityLevel.PriorityLevelName from   PriorityLevel where Event.PriorityLevelId=PriorityLevel.PriorityLevelId) PriorityLevel, '+
 'Reporter.EmployeeName Reporter, '+
 'ReporterOrganization.OrganizationName ReporterOrganization, '+
 'Initiator.EmployeeName Initiator, '+
 'InitiatorOrganization.OrganizationName InitiatorOrganization,  '+
 '(select Role.RoleName from  RoleDef Role where Event.RoleId=Role.RoleId ) Role, '+
 '(select StatusField.FieldName from  CDOFields StatusField where Event.Status=StatusField.DefaultValue AND StatusField.CDODefId=7658) StatusName, '+
 'Event.Status, '+
 '(select QualityResolutionCode.QualityResolutionCodeName from   QualityResolutionCode where Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId) ResolutionCode, '+
 'Event.CloseDescription CloseDescription, '+
 'Event.CloseDateGMT CloseDate, '+
 'ClosedBy.EmployeeName ClosedBy,  '+
 'isnull(EventData.ProductName,EventLot.ProductName) Product, '+
 'isnull(EventData.ProductRev,EventLot.ProductRev) ProductRev, '+
 'EventData.MaintenanceReqName MaintenanceReqName, '+
 'isnull(Eventlot.OperationName,EventData.OperationName) OperationName, '+
 'EventData.ResourceName ResourceName, '+
 'FailureMode.FailureModeName  '+
 'FROM  event  '+
 'LEFT OUTER JOIN  EventData ON Event.EventId=EventData.EventId  '+
 'LEFT OUTER JOIN  EventFailure on eventdata.EventDataId = EventFailure.EventDataId '+
 'LEFT OUTER JOIN FailureMode on EventFailure.FailureModeId = FailureMode.FailureModeId  '+
 'inner JOIN  Classification ON Event.ClassificationId =Classification.ClassificationId  '+
 'inner JOIN  SubClassification ON Event.SubClassificationId =SubClassification.SubClassificationId   '+
 'LEFT OUTER JOIN  Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId  '+
 'LEFT OUTER JOIN  Organization ON Event.OrganizationId=Organization.OrganizationId  '+
 'LEFT OUTER JOIN  Employee Owner ON Event.OwnerId=Owner.EmployeeId  '+
 'inner join  Employee Reporter ON Event.ReporterId=Reporter.EmployeeId '+
 'inner JOIN  Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId '+
 'inner JOIN  Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId '+
 'inner JOIN  Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId '+
 'left outer join  EventLot on Eventdata.EventDataId = EventLot.EventDataId  '+
 'where Event.Status in (1,3,4) and  Event.Category = 3  '+
 'and  convert(date, Event.ReportedDate, 101) >= CONVERT(date, getdate()-90, 101)'


 EXEC csiSTInstall_CreateNewDefinition N'csiNonconformance', N'Gets Nonconformance information for the Process Health Dashboard.', NULL, @SQLString, 1, 'ProcessHealthNonconformance', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'ProductFamilyname '+ 
    'from  '+ 
    'Product '+ 
    'Inner Join ProductBase on Product.ProductBaseid = ProductBase.ProductBaseID '+ 
    'Inner Join ProductFamily on Product.ProductFamilyId= ProductFamily.ProductFamilyId '
 EXEC csiSTInstall_CreateNewDefinition N'csiProductFamily', N'Gets the Product, Product Family Information', NULL, @SQLString, 1, 'ProductFamily', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'WorkflowStepName, '+ 
    'Sequence, '+ 
    'Yield Yield_Workflow, '+ 
    'Move, '+ 
    'Loss, '+ 
    '(select '+ 
    'EXP(SUM(Log(Yield)))   Yield '+ 
    'from csiView_FirstPassYield FPY2 '+ 
    'Where FPY2.ProductName = FPY.ProductName and FPY2.ProductRevision = FPY.ProductRevision '+ 
    'And Yield<>0 '+ 
    ') Yield_Product '+ 
    'From '+ 
    'csiView_FirstPassYield FPY '
 EXEC csiSTInstall_CreateNewDefinition N'csiProductionDashboardYield', N'Used for the Yield report on the Production Dashboard.', NULL, @SQLString, 1, 'ProductionDashboardYield', 0, NULL, NULL, '0,', NULL, 0,1    
 
 SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'sum(CycleTime)/COUNT(distinct ContainerName) Cycletime_Product, '+ 
    'sum(CycleTime) Cycletime, '+ 
    'MIN(Qty) Qty, '+
    'case when (sum(Cycletime)) <>0 then MIN(Qty)/(sum(CycleTime)) else 0 end as Throughput_Product, '+ 
    'ProductName, '+ 
    'containername '+ 
    'from csiTbl_ProductionDashboard  '+ 
    'Group by  '+ 
    'ProductName, '+ 
    'Containername '
 EXEC csiSTInstall_CreateNewDefinition N'csiThroughputProduct', N'Gets Product Throughput Information for Production Dashboard.', NULL, @SQLString, 1, 'ThroughputProduct', 0, NULL, NULL, '0,', NULL, 0,1    
 
SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'Sequence, '+ 
    'WorkflowStepName, '+ 
    'ContainerName, '+ 
    'min(Qty) as Qty, '+ 
    'Sum(Cycletime) as Cycletime, '+ 
    'case when (sum(Cycletime)) <>0 then min(Qty)/(sum(Cycletime)) else 0 end as Throughput_Step '+ 
    'From  '+ 
    'csiTbl_ProductionDashboard  '+ 
    'group by  '+ 
    'ProductName, '+ 
    'ProductRevision, '+ 
    'Sequence, '+ 
    'WorkflowStepName, '+ 
    'containerName '
 EXEC csiSTInstall_CreateNewDefinition N'csiThroughputStep', N'Gets Step Throughput Information for Production Dashboard.', NULL, @SQLString, 1, 'ThroughputStep', 0, NULL, NULL, '0,', NULL, 0,1    	                                     
END
GO
EXEC csiSTInstall_PopulateDefaultData
GO
DROP PROCEDURE csiSTInstall_PopulateDefaultData
GO
DROP PROCEDURE csiSTInstall_CreateNewDefinition
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_CreateNewUserQuery' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_CreateNewUserQuery
GO
CREATE PROCEDURE csiSTInstall_CreateNewUserQuery(@Name NVARCHAR(255)
                                                      ,@Description NVARCHAR(512)
                                                      ,@Notes NVARCHAR(512)
                                                      ,@QueryText NVARCHAR(MAX)                                                      
                                                      ,@IsFrozen BIT
                                                      ,@Param1Name NVARCHAR(50)
                                                      ,@Param1Type INT
                                                      ,@Param2Name NVARCHAR(50)
                                                      ,@Param2Type INT
                                                      ,@Param3Name NVARCHAR(50)
                                                      ,@Param3Type INT)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InstanceID CHAR(16)
    DECLARE @CurrInstanceID CHAR(16)
    DECLARE @CDODefId INT
    DECLARE @ParamCDODefId INT
    DECLARE @ParamFieldId INT
    DECLARE @QueryTypeId INT
    DECLARE @IID CHAR(16)
    SET @CDODefId=7068
    SET @ParamCDODefId=7071
    SET @ParamFieldId=8597
    SET @QueryTypeId = 4
    
    -- Remove the existing UserQuery and its Parameters, if it exists
    IF EXISTS (SELECT UserQueryId
               FROM UserQuery
               WHERE UserQueryName = @Name)
    BEGIN
       SELECT @CurrInstanceID=UserQueryId
               FROM UserQuery
               WHERE UserQueryName = @Name
               
       DELETE FROM UserQueryParameter WHERE UserQueryId = @CurrInstanceID
       DELETE FROM UserQueryUserQueryParameters WHERE UserQueryId = @CurrInstanceID
       DELETE FROM UserQuery WHERE UserQueryName = @Name
    END

    EXEC csiPRDGetNextInstanceId @CDODefId,@InstanceId OUTPUT
    INSERT INTO UserQuery(UserQueryId
                         ,UserQueryName
                         ,Description
                         ,Notes
                         ,QueryText
                         ,IsFrozen
                         ,CDOTypeId
                         ,QueryTypeId)
       VALUES (@InstanceId
              ,@Name
              ,@Description
              ,@Notes
              ,@QueryText
              ,@IsFrozen
              ,@CDODefId
              ,@QueryTypeId)
    IF (@Param1Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param1Name,@InstanceID,@IsFrozen,@Param1Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,1,@InstanceID,@IID)
    END
    
    IF (@Param2Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param2Name,@InstanceID,@IsFrozen,@Param2Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,2,@InstanceID,@IID)
    END
    
    IF (@Param3Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT

       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param3Name,@InstanceID,@IsFrozen,@Param3Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,3,@InstanceID,@IID)
    END
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_PopulateDefaultUserQueryData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO
CREATE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
AS
DECLARE @SQLString NVARCHAR(MAX)
BEGIN
    SET NOCOUNT ON;
    
SET @SQLString = convert(nvarchar(max), N'') + N'Select wfs2.WorkflowStepName as FromStep '+ 
    ',sum(movehistory.CycleTime)*24*60 as ProcessTime '+ 
    ',sum(mih.CycleTime)*24*60 as waittime '+ 
    ',(sum(mih.CycleTime)*24*60 + sum(movehistory.CycleTime)*24*60)/COUNT(c.ContainerId) as CycleTime '+ 
    ',COUNT(c.containername)as ContainerCount '+ 
    'from MoveHistory '+ 
    'Inner Join Container c on MoveHistory.HistoryId = c.containerID '+ 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId '+ 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId '+ 
    'Inner join MoveInHistory mih on c.ContainerId = mih.HistoryId '+ 
    'Inner join HistoryMainline hml2 on mih.HistoryMainlineId = hml2.HistoryMainlineId '+ 
    'Where CONVERT(date, hml.txnDate, 101) = CONVERT(date, GETDATE(), 101) '+ 
    'and hml2.containerId = c.containerId and hml2.WorkflowStepId=wfs2.WorkflowStepId  '+ 
    'and wfs2.WorkflowStepName = ?WorkflowStepName '+ 
    'Group By wfs2.WorkflowStepName '
    
   EXEC csiSTInstall_CreateNewUserQuery 'CurrentCycletimeMetric','CycleTime by step in hours','OOB CycleTime query for the OOB PI in the Camstar Portal.',@SQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL
 
 
SET @SQLString = convert(nvarchar(max), N'') + N'Select '+ 
    'wfs2.WorkflowStepName as FromStep '+ 
    ',Sum(movehistory.qty) as goodQty '+ 
    ',Sum(MoveHistory.Qty)+Sum(ABS(isNull(qhd.Qty,0)*isnull(qhd.QtyMultiplier,0))) as TotalQtyscrappedPlusGood '+ 
    'from MoveHistory '+ 
    'Inner Join Container c on MoveHistory.HistoryId = c.containerID '+ 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId '+ 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId '+ 
    'Left  join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId '+ 
    'Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId  '+ 
    'Where '+ 
    'CONVERT(date, hml.txnDate, 101) = CONVERT(date, GETDATE(), 101) '+ 
    'and wfs2.WorkflowStepName = ?workflowstepname '+ 
    'Group By wfs2.workflowstepname '
    
   EXEC csiSTInstall_CreateNewUserQuery 'CurrentThroughputMetric','Current Throughput by workflowstep','OOB Throughput query for the OOB PI in the Camstar Portal.',@SQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'Select  '+ 
    '(Sum(movehistory.qty)/(Sum(MoveHistory.Qty)+Sum(ABS(isNull(qhd.Qty,0)*isnull(qhd.QtyMultiplier,0)))))*100 as yield  '+ 
    'from MoveHistory Inner Join Container c on MoveHistory.HistoryId = c.containerID '+ 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId  '+ 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId  '+ 
    'Left join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId  '+ 
    'Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId   '+ 
    'Where CONVERT(date, hml.txnDate, 101) = CONVERT(date, GETDATE(), 101)  '+ 
    'and wfs2.WorkflowStepName = ?WorkflowStepName '+ 
    'Group By wfs2.workflowstepname '
    
    EXEC csiSTInstall_CreateNewUserQuery 'CurrentYieldMetric','Current Yield by workflowstep','OOB Yield query for the OOB PI in the Camstar Portal.',@SQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL
    
       
END
GO
EXEC csiSTInstall_PopulateDefaultUserQueryData
GO
DROP PROCEDURE csiSTInstall_CreateNewUserQuery
GO
DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO
