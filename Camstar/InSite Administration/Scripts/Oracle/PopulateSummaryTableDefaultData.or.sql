--------------------------------------------------------------------------------
-- SCRIPT:PopulateSummerTableDefaultData.or.sql
-- DESCR: Adds data collection for SPC tester as well as modeling data for SPCCharts.
--        
--
-- Change History:
--	03/29/2012 - Initial Creation.
-- 
--  Copyright Siemens 2023  
--------------------------------------------------------------------------------

CREATE OR REPLACE PROCEDURE csiCreateView(pName VARCHAR2, pSQL VARCHAR2)
AS
   vCnt NUMBER;
BEGIN
   SELECT COUNT(*)
   INTO vCnt
   FROM USER_VIEWS
   WHERE VIEW_NAME = UPPER(pName);
   --
   IF (vCnt>0) THEN
      EXECUTE IMMEDIATE 'DROP VIEW '||pName;
   END IF;
   --
   EXECUTE IMMEDIATE 'CREATE VIEW '||pName||' AS '||pSQL;
END;
/
CREATE OR REPLACE PROCEDURE csiSTInstall_NewDef(pName VARCHAR2
                                                      ,pDescription VARCHAR2
                                                      ,pNotes VARCHAR2
                                                      ,pSummarySQL VARCHAR2
                                                      ,pIsView NUMBER
                                                      ,pTableName VARCHAR2
                                                      ,pIsManuallyExecuted NUMBER
                                                      ,pScheduleDaysOfWeek VARCHAR2
                                                      ,pScheduleDaysOfMonth VARCHAR2
                                                      ,pScheduleHours VARCHAR2
                                                      ,pScheduleMonths VARCHAR2
                                                      ,pIsFrozen NUMBER
                                                      ,pIsEnabled NUMBER)
AS
    vCDODefId   NUMBER := 8236;
    vInstanceID CHAR(16);
BEGIN
    DELETE FROM SummaryTableDef WHERE SummaryTableDefName = pName;

    csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
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
       VALUES (vInstanceId
              ,pName
              ,pDescription
              ,pNotes
              ,pSummarySQL
              ,pIsView
              ,pTableName
              ,pIsManuallyExecuted
              ,pScheduleDaysOfWeek
              ,pScheduleDaysOfMonth
              ,pScheduleHours
              ,pScheduleMonths
              ,pIsFrozen
              ,vCDODefId
              ,0
              ,pIsEnabled);

END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiST_PopulateDefaultData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  

CREATE OR REPLACE PROCEDURE csiST_PopulateDefaultData
AS
   VSQLString CLOB;
BEGIN
    vSQLString := 'with t1 as '||
					'(SELECT	wfs2.workflowstepid, '||
					'mh2.historyid, '||
					'MIN(txndate) AS txndate '||
					'FROM movehistory mh2 '||
					'INNER JOIN historymainline hml2 ON hml2.historymainlineid = mh2.historymainlineid '||
					'and mh2.historyid = hml2.containerid '||
					'INNER JOIN workflowstep    wfs2 ON wfs2.workflowstepid = mh2.stepid '||
					'GROUP BY wfs2.workflowstepid, '||
					'mh2.historyid), '||
					'hml1 as (select historymainlineid, '||
					'txndate, '||
					'operationid, '||
					'employeeid, '||
					'basetxntype, '||
					'workflowstepid, '||
					'productid  '||
					'from historymainline hml where  hml.txndate >= sysdate - 6 ) '||
					'SELECT pb.productname, '||
					'p.productrevision, '||
					'wfs.workflowstepname, '||
					'wfs.sequence, '||
					'c.containername, '||
					't.txndate, '||
					'SUM(t.qty) qty, '||
					't.productid, '||
					't.txnid, '||
					't.historyid, '||
					't.historymainlineid, '||
					'cdodefinition.cdoname, '||
					'operation.operationname, '||
					'employee.employeename '||
					'FROM (SELECT mh.historyid, '||
					'mh.historymainlineid, '||
					'mh.txnid, '||
					'mh.tostepid AS workflowstepid, '||
					'mh.qty, '||
					'mh.productid, '||
					'hml1.txndate, '||
					'hml1.operationid, '||
					'hml1.employeeid, '||
					'hml1.basetxntype '||
					'FROM movehistory mh '||
					'INNER JOIN hml1 ON mh.historymainlineid = hml1.historymainlineid '||
					'INNER JOIN t1 ON t1.workflowstepid = mh.tostepid '||
					'AND t1.historyid = mh.historyid '||
					'AND t1.txndate = hml1.txndate '||
					'WHERE				mh.reworkreasonid IS NULL '||
					'UNION '||
					'SELECT qh.historyid, '||
					'qh.historymainlineid, '||
					'qh.txnid, '||
					'hml1.workflowstepid, '||
					'qh.qty, '||
					'hml1.productid, '||
					'hml1.txndate, '||
					'hml1.operationid, '||
					'hml1.employeeid, '||
					'hml1.basetxntype '||
					'FROM qtyhistory qh '||
					'INNER JOIN hml1 ON qh.historymainlineid = hml1.historymainlineid '||
					'INNER JOIN t1 ON t1.historyid = qh.historyid '||
					'AND hml1.txndate > t1.txndate '||
					'UNION '||
					'SELECT starthistorydetail.historyid, '||
					'starthistorydetail.historymainlineid, '||
					'starthistorydetail.txnid, '||
					'starthistorydetail.workflowstepid, '||
					'starthistorydetail.qty, '||
					'hml1.productid, '||
					'hml1.txndate, '||
					'hml1.operationid, '||
					'hml1.employeeid, '||
					'hml1.basetxntype '||
					'FROM starthistorydetail '||
					'INNER JOIN hml1 ON starthistorydetail.historymainlineid = hml1.historymainlineid '||
					'UNION '||
					'SELECT mh.historyid, '||
					'mh.historymainlineid, '||
					'mh.txnid, '||
					'mh.stepid AS workflowstepid, '||
					'mh.qty, '||
					'mh.productid, '||
					'hml1.txndate, '||
					'hml1.operationid, '||
					'hml1.employeeid, '||
					'hml1.basetxntype '||
					'FROM movehistory mh '||
					'INNER JOIN  hml1 ON mh.historymainlineid = hml1.historymainlineid '||
					'INNER JOIN t1 ON t1.workflowstepid = mh.tostepid '||
					'AND t1.historyid = mh.historyid '||
					'AND hml1.txndate > t1.txndate '||
					'WHERE mh.cumulativereworkstepcount = 1) t '||
					'INNER JOIN container       c ON t.historyid = c.containerid '||
					'INNER JOIN employee ON t.employeeid = employee.employeeid '||
					'INNER JOIN operation ON t.operationid = operation.operationid '||
					'INNER JOIN product         p ON t.productid = p.productid '||
					'INNER JOIN productbase     pb ON pb.productbaseid = p.productbaseid '||
					'INNER JOIN cdodefinition ON t.basetxntype = cdodefinition.cdodefid '||
					'INNER JOIN workflowstep    wfs ON t.workflowstepid = wfs.workflowstepid '||
					'INNER JOIN workflow        wf ON wfs.workflowid = wf.workflowid '||
					'GROUP BY pb.productname, '||
					'p.productrevision, '||
					'wfs.workflowstepname, '||
					'wfs.sequence, '||
					'c.containername, '||
					't.txndate, '||
					't.productid, '||
					't.txnid, '||
					't.historyid, '||
					't.historymainlineid, '||
					'cdodefinition.cdoname, '||
					'operation.operationname, '||
					'employee.employeename';
    csiCreateView('csiYieldSTView',vSQLString); -- If the SQL is longer than 4,000 chars, we must use a view (DataDirect and DataStore limitations)
    vSQLString := 'SELECT * FROM csiYieldSTView';	                
    csiSTInstall_NewDef ('csiYield'
	                       ,'Yield'
	                       ,'Used by Camstar Yield Summary Table.'
	                       ,vSQLString
	                       ,0
	                       ,'Yield'
	                       ,0
	                       ,NULL
	                       ,NULL
	                       ,'0,4,8,12,16,20'
	                       ,NULL
	                        ,1
                          ,1);
                          
                          
   vSQLString := ' SELECT   CdoDefinition.CDOName,  HistoryMainline.Txndate,  Container.ContainerName,   WorkflowStep.WorkflowStepName,   WorkflowStep.Sequence,   ProductBase.ProductName,  Product.ProductRevision,  Container.qty ContainerQty, NVL2(MoveHistory.qty,MoveHistory.qty,MoveInHistory.Qty) Qty, NVL2(Movehistory.CycleTime,Movehistory.CycleTime,MoveInHistory.CycleTime)*(24) Cycletime  FROM     (((   HistoryMainline HistoryMainline    Inner Join CDODefinition on Historymainline.TxnType = CdoDefinition.CDODefID  LEFT OUTER JOIN MoveHistory on HistoryMainline.HistoryMainlineId = MoveHistory.HistoryMainlineId  LEFT OUTER JOIN MoveInHistory on HistoryMainline.HistoryMainlineId = MoveInHistory.HistoryMainlineId  INNER JOIN  Container Container ON HistoryMainline.ContainerId=Container.ContainerId)   INNER JOIN  Product Product ON HistoryMainline.ProductId=Product.ProductId)   INNER JOIN  WorkflowStep WorkflowStep ON HistoryMainline.WorkflowStepId=WorkflowStep.WorkflowStepId)   INNER JOIN  ProductBase ProductBase ON Product.ProductBaseId=ProductBase.ProductBaseId  Where   HistoryMainline.ContainerId in   ( SELECT   Container.ContainerId  FROM     ((  HistoryMainline HistoryMainline   INNER JOIN MoveHistory MoveHistory ON HistoryMainline.HistoryMainlineId=MoveHistory.HistoryMainlineId)   INNER JOIN Container Container ON HistoryMainline.ContainerId=Container.ContainerId)   INNER JOIN WorkflowStep FinalStep ON MoveHistory.ToStepId=FinalStep.WorkflowStepId  WHERE  finalstep.IsLastStep=1         and  HistoryMainline.TxnDate >= sysdate-6)  ';
	                
    csiSTInstall_NewDef ('csiProductionDashboard'
	                       ,'ProductionDashboard'
	                       ,''
	                       ,vSQLString
	                       ,0
	                       ,'ProductionDashboard'
	                       ,0
	                       ,NULL
	                       ,NULL
	                       ,'0,4,8,12,16,20'
	                       ,NULL
	                        ,1
                          ,1);    
                          
                                           
                          
                          
    vSQLString := 'SELECT TRUNC(TxnDate) TxnDate, ProductName, ProductRevision,  WorkflowStepName, Sequence,  SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) AS Move,SUM(CASE WHEN CDOName IN (''Rework'', ''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END) AS Loss,CASE WHEN SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) > 0 THEN ((SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) + SUM(CASE WHEN CDOName IN (''Rework'',''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END)) / SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END)) ELSE 0 END AS Yield FROM csiTbl_Yield GROUP BY TRUNC(TxnDate), ProductName, ProductRevision,  Sequence, WorkflowStepName';
	                
    csiSTInstall_NewDef ('csiFirstPassYield'
	                       ,'FirstPassYield'
	                       ,''
	                       ,vSQLString
	                       ,1
	                       ,'FirstPassYield'
	                       ,0
	                       ,NULL
	                       ,NULL
	                       ,NULL
	                       ,NULL
	                        ,1
                          ,0); 
     
    vSQLString := 'SELECT Event.EventID instanceID, '||
				  'Event.EventName Name, '||
				  'Event.EventDataID, '||
				  'Event.BriefDescription, '||
				  'Event.Description, '||
				  'Event.ReportedDate ReportedDate, '||
				  'Event.AttachmentsId, '||
				  'EventLot.Lot, '||
				  'EventLot.Qty, '||
				  '(select CategoryField.FieldName from CDOFields CategoryField where Event.Category=CategoryField.DefaultValue  '||
				  ' AND CategoryField.CDODefId=7520 ) as CategoryName, '||
				  'Event.Category, '||
				  '(select Classification.ClassificationName from  Classification where Event.ClassificationId =Classification.ClassificationId )Classification, '||
				  '(select SubClassification.SubClassificationName from SubClassification where Event.SubClassificationId =SubClassification.SubClassificationId) SubClassification, '||
				  'Event.DiscoveryArea, '||
				  'Event.OccurrenceDate OccurrenceDate, '||
				  '(select Organization.OrganizationName  from Organization where Event.OrganizationId=Organization.OrganizationId  )Organization, '||
				  '(select Owner.EmployeeName from Employee Owner where Event.OwnerId=Owner.EmployeeId )Owner , '||
				  '(select PriorityLevel.PriorityLevelName PriorityLevel from  PriorityLevel where  Event.PriorityLevelId=PriorityLevel.PriorityLevelId) as PriorityLevel, '||
				  '(select Reporter.EmployeeName from  Employee Reporter where Event.ReporterId=Reporter.EmployeeId) Reporter, '||
				  '(select ReporterOrganization.OrganizationName from organization ReporterOrganization where Event.ReporterOrganizationId=ReporterOrganization.OrganizationId ) ReporterOrganization, '||
				  '(select Initiator.EmployeeName from  Employee Initiator where Event.InitiatorId=Initiator.EmployeeId ) Initiator, '||
				  '(select InitiatorOrganization.OrganizationName from Organization InitiatorOrganization where Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId) InitiatorOrganization, '||
				  '(select Role.RoleName  from RoleDef Role where Event.RoleId=Role.RoleId ) as Role, '||
				  '(select StatusField.FieldName from CDOFields StatusField where Event.Status=StatusField.DefaultValue AND StatusField.CDODefId=7658) as StatusName, '||
				  'Event.Status, '||
				  '(select QualityResolutionCode.QualityResolutionCodeName from QualityResolutionCode where Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId) as ResolutionCode, '||
				  'Event.CloseDescription CloseDescription, '||
				  'Event.CloseDate CloseDate, '||
				  '(select ClosedBy.EmployeeName from Employee ClosedBy where Event.ClosedById=ClosedBy.EmployeeId ) ClosedBy, '||
				  'EventData.ProductName EventDataProduct, '||
				  'EventData.ProductRev EventDataProductRev, '||
				  'EventLot.ProductName EventLotProduct, '||
				  'EventLot.ProductRev EventLotProductRev, '||
				  'EventData.MaintenanceReqName MaintenanceReqName, '||
				  'EventData.OperationName OperationName, '||
				  'EventData.ResourceName ResourceName, '||
				  'EventData.WorkflowName, '||
				  'Eventdata.WorkflowRev, '||
				  'EventData.WorkflowStepName, '||
				  'EventData.EventDate, '||
				  'FailureMode.FailureModeName, '||
				  'EventFailure.FailureModeId, '||
				  'EventFailure.EventFailureId, '||
				  '(select ProductFamilyName from ProductFamily, Product, ProductBase '||
				  'where  Product.ProductFamilyId = ProductFamily.ProductFamilyID  '||
				  'and  Product.ProductBaseId=ProductBase.ProductBaseId '||
				  'and  EventData.ProductName = Productbase.ProductName   '||
				  'And EventData.ProductRev = Product.ProductRevision) as ProductFamilyName '||
				  'FROM  event  '||
				  'inner JOIN  EventData ON Event.EventId=EventData.EventId  '||
				  'LEFT OUTER JOIN  EventFailure on eventdata.EventDataId = EventFailure.EventDataId  '||
				  'LEFT OUTER JOIN FailureMode on EventFailure.FailureModeId = FailureMode.FailureModeId   '||
				  'left outer join  EventLot on Event.EventDataId = EventLot.EventDataId '||
				  'WHERE trunc(Event.ReportedDate) >= trunc(SYSDATE - 90)';	


    csiSTInstall_NewDef ('csiQualityObject'
	                       ,'QualityObject'
	                       ,''
	                       ,vSQLString
	                       ,0
	                       ,'QualityObject'
	                       ,0
	                       ,NULL
	                       ,NULL
	                       ,'0,4,8,12,16,20'
	                       ,NULL
	                       ,1
                          ,1);
     
    vSQLString := 'SELECT     c.ContainerId '|| 
    ', c.ContainerName '|| 
    ', c.FactoryStartQty '|| 
    ', c.Qty AS ContainerCurrentQty '|| 
    ', c.OnHoldDate '|| 
    ', c.LastActivityDate '|| 
    ', UOM.UOMName '|| 
    ', cs.InRework '|| 
    ', wfs.IsLastStep '|| 
    ', CASE WHEN wfs.IsLastStep = 1 THEN cs.LastMoveDate END AS MoveToLastStepDate '|| 
    ', CASE WHEN wfs.IsLastStep = 1 THEN (cs.lastMoveDate-c.factoryStartDate)*86400 / c.Qty END AS secondsPerPiece '|| 
    ', CASE WHEN wfs.IsLastStep = 1 THEN (cs.lastMoveDate-c.factoryStartDate)*86400  END AS secondsPerContainer '|| 
    ', c.HoldReasonId '|| 
    ', wfs.WorkflowStepName '|| 
    ', MfgOrder.MfgOrderId '|| 
    ', MfgOrder.MfgOrderName '|| 
    ', MfgOrder.PlannedCompletionDate '|| 
    ', MfgOrder.DefaultLot '|| 
    ', OrderStatus.OrderStatusName '|| 
    ', MfgOrder.PlannedStartDate '|| 
    ', MfgOrder.Qty AS OrderQty '|| 
    ', pb.ProductName '|| 
    ', p.ProductRevision '|| 
    ', p.ProductId '|| 
    ', NVL(CASE WHEN wfs.IsLastStep = 1 THEN c.Qty END, 0) AS ContainerCompletedQty '|| 
    ', NVL(CASE WHEN wfs.IsLastStep = 1 THEN c.FactoryStartQty END, 0)  AS CompletedStartQty '|| 
    'FROM          '|| 
    ' Container  c  '|| 
    ' INNER JOIN CurrentStatus  cs ON c.CurrentStatusId = cs.CurrentStatusId  '|| 
    ' INNER JOIN WorkflowStep  wfs ON cs.WorkflowStepId = wfs.WorkflowStepId '|| 
    'INNER JOIN MfgOrder ON c.MfgOrderId = MfgOrder.MfgOrderId '|| 
    'LEFT OUTER JOIN OrderStatus ON MfgOrder.OrderStatusId = OrderStatus.OrderStatusId '|| 
    'LEFT OUTER JOIN PriorityCode ON MfgOrder.PriorityId = PriorityCode.PriorityCodeId '|| 
    'INNER JOIN UOM ON c.UOMId = UOM.UOMId '|| 
    'INNER JOIN Product  p ON c.ProductId = p.ProductId '|| 
    'INNER JOIN ProductBase  pb ON p.ProductBaseId = pb.ProductBaseId '|| 
    'WHERE (MfgOrder.MfgOrderId IN(SELECT DISTINCT Container.MfgOrderId '|| 
    '                              FROM  Container '|| 
    '                              INNER JOIN CurrentStatus ON Container.CurrentStatusId = CurrentStatus.CurrentStatusId '|| 
    '                              INNER JOIN WorkflowStep ON CurrentStatus.WorkflowStepId = WorkflowStep.WorkflowStepId '|| 
    '                              WHERE (WorkflowStep.IsLastStep = 0))) '||
	'AND trunc(c.LastActivityDate) >= trunc(sysdate - 90)';
                          
    csiSTInstall_NewDef('csiOpenOrdersSummary', NULL, NULL, vSQLString, 1, 'OpenOrdersSummary', 0, NULL, NULL, '0,', NULL, 0,1);                          
	
  
    vSQLString := '/*--Get the beginning qty at each step except first step*/ '|| 
    'select * from (WITH cont_data AS ('||
		'SELECT '||
		'p.productid           AS productid, '||
		'pb.productname        productname, '||
		'p.productrevision     productrevision, '||
		'wfs.workflowstepname  workflowstepname, '||
		'wfs.islaststep        islaststep, '||
		'wfs.workflowstepid    workflowstepid, '||
		'wfs.sequence          sequence, '||
		'wf.workflowid         workflowid, '||
		'c.containername       containername, '||
		'hml.resourceid        resourceid, '||
		'hml.txndate           txndate, '||
		'cdodefinition.cdoname AS cdoname, '||
		'c.ownerid             ownerid, '||
		'c.mfgorderid          mfgorderid, '||
		'hml.factoryid         factoryid, '||
		'hml.shiftname         shiftname, '||
		'hml.calendarshiftid   calendarshiftid, '||
		'hml.steppass          steppass, '||
		'to_char(NULL)         AS reasoncodeid, '||
		'sd.normalcycletime    AS targetcycletime, '||
		'sd.yield              AS targetyield, '||
		'sd.unitsperhour       AS targetunitsperhour, '||
		'c.containerid         AS containerid, '||
		'hml.historymainlineid AS historymainlineid, '||
		'hml.historyid         AS historyid '||
		' FROM '||
		' container c '||
		'INNER JOIN historymainline      hml ON hml.containerid = c.containerid '||
		'INNER JOIN workflowstep         wfs ON hml.workflowstepid = wfs.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid '||
		'INNER JOIN product              p ON hml.productid = p.productid '||
		'INNER JOIN productbase          pb ON pb.productbaseid = p.productbaseid '||
		'INNER JOIN cdodefinition ON hml.basetxntype = cdodefinition.cdodefid '||
		'INNER JOIN workflow             wf ON wfs.workflowid = wf.workflowid '||
		'where trunc(hml.txndate) >= trunc(sysdate) - 90 ) '||
		'SELECT '||
		'movehistory.qty            qty, '||
		'movehistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'wfs2.workflowstepname      AS workflowstepname, '||
		'wfs2.islaststep, '||
		'wfs2.workflowstepid        AS stepid, '||
		'wfs2.sequence              AS stepsequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'cont_data.historyid, '||
		'hml1.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'movehistory.toresourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'cont_data.workflowstepname AS fromstep, '||
		'cont_data.workflowstepid   AS fromstepid, '||
		'cont_data.sequence         AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)              AS reasoncodeid, '||
		'sd2.normalcycletime        AS targetcycletime, '||
		'cont_data.targetyield      AS targetyield, '||
		'sd2.unitsperhour           AS targetunitsperhour, '||
		'sysdate                    AS lastrefreshdate '||
		'FROM '||
		'cont_data, movehistory  '||
		'INNER JOIN historymainline      hml1 ON movehistory.historymainlineid = hml1.historymainlineid '||
		'INNER JOIN workflowstep         wfs2 ON movehistory.tostepid = wfs2.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '||
		'WHERE movehistory.reworkreasonid IS NULL '||
		'AND movehistory.historyid = cont_data.containerid '||
		'AND movehistory.stepid = cont_data.workflowstepid '||
		'and cont_data.cdoname = ''MoveTxn'' '||
		'AND wfs2.sequence <> 1 '||
		'UNION  /*--Get Scrap Qtys*/ '||
		'SELECT '||
		'qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier qty, '||
		'to_number(0)                                            AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'qtyhistory.historyid, '||
		'qtyhistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'qtyhistory.resourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'to_char(NULL)                                           AS fromstep, '||
		'to_char(NULL)                                           AS fromstepid, '||
		'to_number(NULL)                                         AS fromstepsequence, '||
		'cont_data.steppass, '||
		'qtyhistorydetails.reasoncodeid                          AS reasoncodeid, '||
		'to_number(NULL)                                         AS targetcycletime, '||
		'cont_data.targetyield                                   AS targetyield, '||
		'to_number(NULL)                                         AS targetunitsperhour, '||
		'sysdate                                                 AS lastrefreshdate '||
		'FROM '||
		'cont_data, qtyhistory '||
		'INNER JOIN qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid '||
		'WHERE '||
		'qtyhistory.historyid = cont_data.containerid '||
		'AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid '||
		'AND qtyhistory.historymainlineid = cont_data.historymainlineid '||
		'UNION  /*--Get first workflow Steps starting qty from start history */ '||
		'SELECT '||
		'nvl(starthistorydetail.qty, 0) qty, '||
		'to_number(0)                   AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'starthistorydetail.historyid, '||
		'starthistorydetail.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'starthistorydetail.resourceid, '||
		'starthistorydetail.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'to_char(NULL)                  AS fromstep, '||
		'to_char(NULL)                  AS fromstepid, '||
		'to_number(NULL)                AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)                  AS reasoncodeid, '||
		'to_number(NULL)                AS targetcycletime, '||
		'cont_data.targetyield          AS targetyield, '||
		'to_number(NULL)                AS targetunitsperhour, '||
		'sysdate                        AS lastrefreshdate '||
		'FROM '||
		'cont_data, '||
		'starthistorydetail '||
		'WHERE '||
		'starthistorydetail.historyid = cont_data.containerid '||
		'AND starthistorydetail.historymainlineid = cont_data.historymainlineid '||
		'AND starthistorydetail.workflowstepid = cont_data.workflowstepid '||
		'UNION  /*--Get the Reworked Qty*/ '||
		'SELECT '||
		'( movehistory.qty ) * - 1  qty, '||
		'movehistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'movehistory.historyid, '||
		'movehistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'movehistory.toresourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'wfs2.workflowstepname      AS fromstep, '||
		'wfs2.workflowstepid        AS fromstepid, '||
		'wfs2.sequence              AS fromstepsequence, '||
		'cont_data.steppass, '||
		'movehistory.reworkreasonid AS reasoncodeid, '||
		'sd2.normalcycletime        AS targetcycletime, '||
		'cont_data.targetyield      AS targetyield, '||
		'sd2.unitsperhour           AS targetunitsperhour, '||
		'sysdate                    AS lastrefreshdate '||
		'FROM '||
		'cont_data, movehistory '||
		'INNER JOIN workflowstep         wfs2 ON movehistory.stepid = wfs2.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '||
		'WHERE '||
		'movehistory.reworkreasonid IS NOT NULL '||
		'AND movehistory.historyid = cont_data.containerid '||
		'AND movehistory.stepid = cont_data.workflowstepid '||
		'AND movehistory.historymainlineid = cont_data.historymainlineid '||
		'UNION  /*--Get the first pass movein qty*/ '||
		'SELECT '||
		'moveinhistory.qty            qty, '||
		'moveinhistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'moveinhistory.historyid, '||
		'moveinhistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'cont_data.resourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'cont_data.workflowstepname   AS fromstep, '||
		'cont_data.workflowstepid     AS fromstepid, '||
		'cont_data.sequence           AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)                AS reasoncodeid, '||
		'cont_data.targetcycletime    AS targetcycletime, '||
		'cont_data.targetyield        AS targetyield, '||
		'cont_data.targetunitsperhour AS targetunitsperhour, '||
		'sysdate                      AS lastrefreshdate '||
		'FROM '||
		'moveinhistory moveinhistory, '||
		'cont_data '||
		'WHERE '||
		'moveinhistory.historyid = cont_data.containerid '||
		' and moveinhistory.HistoryMainlineId = cont_data.historymainlineid)';
        
    csiCreateView('csiFactPerfSumSTView',vSQLString); -- If the SQL is longer than 4,000 chars, we must use a view (DataDirect and DataStore limitations)
    vSQLString := 'SELECT * FROM csiFactPerfSumSTView';
    csiSTInstall_NewDef('csiFactoryPerformanceSummary', NULL, NULL, vSQLString, 0, 'FactoryPerformanceSum', 0, NULL, NULL, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,', NULL, 0,1); 
    
    
    vSQLString :=  '/*--Get the beginning qty at each step except first step*/ '|| 
    'select * from (WITH cont_data AS ('||
		'SELECT '||
		'p.productid           AS productid, '||
		'pb.productname        productname, '||
		'p.productrevision     productrevision, '||
		'wfs.workflowstepname  workflowstepname, '||
		'wfs.islaststep        islaststep, '||
		'wfs.workflowstepid    workflowstepid, '||
		'wfs.sequence          sequence, '||
		'wf.workflowid         workflowid, '||
		'c.containername       containername, '||
		'hml.resourceid        resourceid, '||
		'hml.txndate           txndate, '||
		'cdodefinition.cdoname AS cdoname, '||
		'c.ownerid             ownerid, '||
		'c.mfgorderid          mfgorderid, '||
		'hml.factoryid         factoryid, '||
		'hml.shiftname         shiftname, '||
		'hml.calendarshiftid   calendarshiftid, '||
		'hml.steppass          steppass, '||
		'to_char(NULL)         AS reasoncodeid, '||
		'sd.normalcycletime    AS targetcycletime, '||
		'sd.yield              AS targetyield, '||
		'sd.unitsperhour       AS targetunitsperhour, '||
		'c.containerid         AS containerid, '||
		'hml.historymainlineid AS historymainlineid, '||
		'hml.historyid         AS historyid '||
		' FROM '||
		' container c '||
		'INNER JOIN historymainline      hml ON hml.containerid = c.containerid '||
		'INNER JOIN workflowstep         wfs ON hml.workflowstepid = wfs.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid '||
		'INNER JOIN product              p ON hml.productid = p.productid '||
		'INNER JOIN productbase          pb ON pb.productbaseid = p.productbaseid '||
		'INNER JOIN cdodefinition ON hml.basetxntype = cdodefinition.cdodefid '||
		'INNER JOIN workflow             wf ON wfs.workflowid = wf.workflowid '||
		'where trunc(hml.txndate) = trunc(sysdate) ) '||
		'SELECT '||
		'movehistory.qty            qty, '||
		'movehistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'wfs2.workflowstepname      AS workflowstepname, '||
		'wfs2.islaststep, '||
		'wfs2.workflowstepid        AS stepid, '||
		'wfs2.sequence              AS stepsequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'cont_data.historyid, '||
		'hml1.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'movehistory.toresourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'cont_data.workflowstepname AS fromstep, '||
		'cont_data.workflowstepid   AS fromstepid, '||
		'cont_data.sequence         AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)              AS reasoncodeid, '||
		'sd2.normalcycletime        AS targetcycletime, '||
		'cont_data.targetyield      AS targetyield, '||
		'sd2.unitsperhour           AS targetunitsperhour, '||
		'sysdate                    AS lastrefreshdate '||
		'FROM '||
		'cont_data, movehistory  '||
		'INNER JOIN historymainline      hml1 ON movehistory.historymainlineid = hml1.historymainlineid '||
		'INNER JOIN workflowstep         wfs2 ON movehistory.tostepid = wfs2.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '||
		'WHERE movehistory.reworkreasonid IS NULL '||
		'AND movehistory.historyid = cont_data.containerid '||
		'AND movehistory.stepid = cont_data.workflowstepid '||
		'and cont_data.cdoname = ''MoveTxn'' '||
		'AND wfs2.sequence <> 1 '||
		'UNION  /*--Get Scrap Qtys*/ '||
		'SELECT '||
		'qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier qty, '||
		'to_number(0)                                            AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'qtyhistory.historyid, '||
		'qtyhistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'qtyhistory.resourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'to_char(NULL)                                           AS fromstep, '||
		'to_char(NULL)                                           AS fromstepid, '||
		'to_number(NULL)                                         AS fromstepsequence, '||
		'cont_data.steppass, '||
		'qtyhistorydetails.reasoncodeid                          AS reasoncodeid, '||
		'to_number(NULL)                                         AS targetcycletime, '||
		'cont_data.targetyield                                   AS targetyield, '||
		'to_number(NULL)                                         AS targetunitsperhour, '||
		'sysdate                                                 AS lastrefreshdate '||
		'FROM '||
		'cont_data, qtyhistory '||
		'INNER JOIN qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid '||
		'WHERE '||
		'qtyhistory.historyid = cont_data.containerid '||
		'AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid '||
		'AND qtyhistory.historymainlineid = cont_data.historymainlineid '||
		'UNION  /*--Get first workflow Steps starting qty from start history */ '||
		'SELECT '||
		'nvl(starthistorydetail.qty, 0) qty, '||
		'to_number(0)                   AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'starthistorydetail.historyid, '||
		'starthistorydetail.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'starthistorydetail.resourceid, '||
		'starthistorydetail.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'to_char(NULL)                  AS fromstep, '||
		'to_char(NULL)                  AS fromstepid, '||
		'to_number(NULL)                AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)                  AS reasoncodeid, '||
		'to_number(NULL)                AS targetcycletime, '||
		'cont_data.targetyield          AS targetyield, '||
		'to_number(NULL)                AS targetunitsperhour, '||
		'sysdate                        AS lastrefreshdate '||
		'FROM '||
		'cont_data, '||
		'starthistorydetail '||
		'WHERE '||
		'starthistorydetail.historyid = cont_data.containerid '||
		'AND starthistorydetail.historymainlineid = cont_data.historymainlineid '||
		'AND starthistorydetail.workflowstepid = cont_data.workflowstepid '||
		'UNION  /*--Get the Reworked Qty*/ '||
		'SELECT '||
		'( movehistory.qty ) * - 1  qty, '||
		'movehistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'movehistory.historyid, '||
		'movehistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'movehistory.toresourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'wfs2.workflowstepname      AS fromstep, '||
		'wfs2.workflowstepid        AS fromstepid, '||
		'wfs2.sequence              AS fromstepsequence, '||
		'cont_data.steppass, '||
		'movehistory.reworkreasonid AS reasoncodeid, '||
		'sd2.normalcycletime        AS targetcycletime, '||
		'cont_data.targetyield      AS targetyield, '||
		'sd2.unitsperhour           AS targetunitsperhour, '||
		'sysdate                    AS lastrefreshdate '||
		'FROM '||
		'cont_data, movehistory '||
		'INNER JOIN workflowstep         wfs2 ON movehistory.stepid = wfs2.workflowstepid '||
		'LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid '||
		'WHERE '||
		'movehistory.reworkreasonid IS NOT NULL '||
		'AND movehistory.historyid = cont_data.containerid '||
		'AND movehistory.stepid = cont_data.workflowstepid '||
		'AND movehistory.historymainlineid = cont_data.historymainlineid '||
		'UNION  /*--Get the first pass movein qty*/ '||
		'SELECT '||
		'moveinhistory.qty            qty, '||
		'moveinhistory.cycletime      AS movecycletime, '||
		'cont_data.productname, '||
		'cont_data.productrevision, '||
		'cont_data.productid, '||
		'cont_data.workflowstepname, '||
		'cont_data.islaststep, '||
		'cont_data.workflowstepid, '||
		'cont_data.sequence, '||
		'cont_data.workflowid, '||
		'cont_data.containername, '||
		'cont_data.txndate, '||
		'moveinhistory.historyid, '||
		'moveinhistory.historymainlineid, '||
		'cont_data.cdoname, '||
		'cont_data.ownerid, '||
		'cont_data.resourceid, '||
		'cont_data.mfgorderid, '||
		'cont_data.factoryid, '||
		'cont_data.shiftname, '||
		'cont_data.calendarshiftid, '||
		'cont_data.workflowstepname   AS fromstep, '||
		'cont_data.workflowstepid     AS fromstepid, '||
		'cont_data.sequence           AS fromstepsequence, '||
		'cont_data.steppass, '||
		'to_char(NULL)                AS reasoncodeid, '||
		'cont_data.targetcycletime    AS targetcycletime, '||
		'cont_data.targetyield        AS targetyield, '||
		'cont_data.targetunitsperhour AS targetunitsperhour, '||
		'sysdate                      AS lastrefreshdate '||
		'FROM '||
		'moveinhistory moveinhistory, '||
		'cont_data '||
		'WHERE '||
		'moveinhistory.historyid = cont_data.containerid '||
		' and moveinhistory.HistoryMainlineId = cont_data.historymainlineid)';

    csiCreateView('csi24HrFactPerfSumSTView',vSQLString); -- If the SQL is longer than 4,000 chars, we must use a view (DataDirect and DataStore limitations)
    vSQLString := 'SELECT * FROM csi24HrFactPerfSumSTView';        
    csiSTInstall_NewDef('csi24HrFactoryPerformance', NULL, NULL, vSQLString, 1, '24HrFactoryPerformance', 0, NULL, NULL, '0,', NULL, 0,1); 
    
    
    vSQLString := 'SELECT     wf.WorkflowId '|| 
    ', wf.WorkflowRevision '|| 
    ', wfb.WorkflowName '|| 
    ', wfs.WorkflowStepName '|| 
    ', wfs.WorkflowStepId '|| 
    ', s.SpecId '|| 
    ', r.ResourceName '|| 
    ', rg.ResourceGroupName '|| 
    ', ps.MfgOrderId '|| 
    ', ps.LastActivityDate '|| 
    ', ps.LastStatusChangeDate '|| 
    ', to_char(trunc( sysdate - ps.LastStatusChangeDate ),''FM999999990'') || '':'' || to_char(trunc( mod( (sysdate - ps.LastStatusChangeDate)*24, 24 ) ),''FM00'') || '':'' || to_char(trunc( mod( (sysdate - ps.LastStatusChangeDate)*24*60, 60 ) ),''FM00'') || '':'' || to_char(trunc( mod( (sysdate - ps.LastStatusChangeDate)*24*60*60, 60 ) ),''FM00'')  AS TimeDown '|| 
    ', ps.ContainerId '|| 
    ', ps.ResourceId '|| 
    ', ps.ResourceState '|| 
    ', ps.Availability '|| 
    ', ps.ReasonId '|| 
    ', ps.StatusId '|| 
    ', rsc.ResourceStatusCodeName '|| 
    ',rsr.ResourceStatusReasonName '|| 
    'FROM Workflow  wf  '|| 
    'INNER JOIN WorkflowBase  wfb ON wf.WorkflowBaseId = wfb.WorkflowBaseId  '|| 
    'INNER JOIN WorkflowStep  wfs ON wfs.WorkflowId = wf.WorkflowId  '|| 
    'INNER JOIN Spec s ON wfs.SpecId = s.SpecId OR wfs.SpecBaseId = s.SpecBaseId  '|| 
    'INNER JOIN ResourceGroup rg ON s.ResourceGroupId = rg.ResourceGroupId  '|| 
    'INNER JOIN ResourceGroupEntries rge ON rg.ResourceGroupId = rge.ResourceGroupId  '|| 
    'INNER JOIN ResourceDef r ON rge.EntriesId = r.ResourceId  '|| 
    'INNER JOIN ProductionStatus ps ON r.ResourceId = ps.ResourceId  '|| 
    'LEFT OUTER JOIN ResourceStatusCode rsc ON ps.StatusId = rsc.ResourceStatusCodeId  '|| 
    'LEFT OUTER JOIN ResourceStatusReason rsr ON ps.ReasonId = rsr.ResourceStatusReasonId '||
	'WHERE trunc(ps.LastStatusChangeDate) >= trunc(SYSDATE - 90)';
    
    csiSTInstall_NewDef('csiWFResourceDowntime', NULL, NULL, vSQLString, 1, 'csiWFResourceDowntime', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    
    vSQLString := 'Select  '|| 
    'sum(CycleTime)/COUNT(distinct ContainerName) Cycletime_Product, '|| 
    'sum(CycleTime) Cycletime, '|| 
    'MIN(ContainerQty) Qty, '|| 
    'sum(CycleTime)/MIN(ContainerQty) Throughput_Product, '|| 
    'ProductName, '|| 
    'containername '|| 
    'from csiTbl_ProductionDashboard  '|| 
    'Group by ProductName,containername ';
    csiSTInstall_NewDef('csiCycleTimeProduct', 'Gets Product Cycle Time Information for Production Dashboard.', NULL, vSQLString, 1, 'CycleTimeProduct', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    
    vSQLString := 'Select  '|| 
    'ProductName, '|| 
    'ProductRevision, '|| 
    'WorkflowStepName, '|| 
    'Sequence, '|| 
    'COUNT(distinct ContainerName) ContainerCnt, '|| 
    'Sum(Qty) as Qty, '|| 
    'Sum(CycleTime) Cycletime, '|| 
    'sum(CycleTime)/COUNT(distinct ContainerName) as Cycletime_Step '|| 
    'From  '|| 
    'csiTbl_ProductionDashboard '|| 
    'group by  '|| 
    'ProductName, '|| 
    'ProductRevision, '|| 
    'Sequence, '|| 
    'WorkflowStepName ';
    csiSTInstall_NewDef('csiCycleTimeStep', 'Gets Step Cycle Time Information for Production Dashboard.', NULL, vSQLString, 1, 'CycleTimeStep', 0, NULL, NULL, '0,', NULL, 1,1); 
    
        vSQLString := ' SELECT decode(Event.CloseDate, null, ''Open'',  ''Closed'') MyStatus, '||  
		 'Event.ReportedDate EventDate, '||     
		 'Event.ReportedDate , '||     
		 'Event.CloseDate, '||     
		 'Event.EventId, '||     
		 'Event.EventName, '||     
		 'FailureMode.FailureModeName, '||     
		 'NCRCauseCode.NCRCauseCodeName, '||     
		 'FailureSeverity.FailureSeverityName  '||     
		 'FROM   ((((( Event Event    '||     
		 'LEFT OUTER JOIN EventFailure EventFailure   ON Event.EventDataId=EventFailure.EventDataId) '||     
		 'LEFT OUTER JOIN FailureMode FailureMode   ON EventFailure.FailureModeId=FailureMode.FailureModeId) '||     
		 'LEFT OUTER JOIN FailureSeverity FailureSeverity   ON EventFailure.FailureSeverityId=FailureSeverity.FailureSeverityId) '||     
		 'LEFT OUTER JOIN EventFailureCause EventFailureCause   ON EventFailure.EventFailureId=EventFailureCause.EventFailureId) '||     
		 'LEFT OUTER JOIN NCRCauseCode NCRCauseCode   ON EventFailureCause.CauseCodeId=NCRCauseCode.NCRCauseCodeId) '||     
		 'LEFT OUTER JOIN FailureSeverity FailureSeverity_Default   ON FailureMode.DefaultSeverityId=FailureSeverity_Default.FailureSeverityId '||
		 'WHERE trunc(Event.ReportedDate) >= trunc(sysdate - 90)';
    csiSTInstall_NewDef('csiEventTrend', 'Used for the Event Trend report.', NULL, vSQLString, 1, 'EventTrend', 0, NULL, NULL, '0,', NULL, 1,1); 
    
        vSQLString := 'SELECT Event.EventID InstanceID, '||
  	    'Event.EventName Name, '||
  	    'Event.BriefDescription, '||
  	    'Event.Description, '||
  	    'Event.ReportedDateGMT ReportedDate, '||
  	    'EventLot.Lot, '||
  	    'EventLot.Qty, '||
  	    '(select CategoryField.FieldName  '||
  	    'from CDOFields CategoryField  '||
  	    'where Event.Category=CategoryField.DefaultValue AND CategoryField.CDODefId=7520 )  CategoryName, '||
  	    'Event.Category, '||
  	    'Classification.ClassificationName Classification, '||
  	    'SubClassification.SubClassificationName SubClassification, '||
  	    'Event.DiscoveryArea, '||
  	    'Event.OccurrenceDateGMT OccurrenceDate, '||
  	    'Organization.OrganizationName Organization, '||
  	    'Owner.EmployeeName Owner, '||
  	    'PriorityLevel.PriorityLevelName PriorityLevel, '||
  	    'Reporter.EmployeeName Reporter, '||
  	    'ReporterOrganization.OrganizationName ReporterOrganization, '||
  	    'Initiator.EmployeeName Initiator, '||
  	    'InitiatorOrganization.OrganizationName InitiatorOrganization, '||
  	    '(select Role.RoleName from RoleDef Role where Event.RoleId=Role.RoleId )Role, '||
  	    '(select StatusField.FieldName  '||
  	    'from CDOFields StatusField  '||
  	    'where Event.Status=StatusField.DefaultValue AND StatusField.CDODefId=7658 )  StatusName, '||
  	    'Event.Status, '||
  	    '(select QualityResolutionCode.QualityResolutionCodeName from QualityResolutionCode  '||
  	    'where Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId) ResolutionCode, '||
  	    'Event.CloseDescription CloseDescription, '||
  	    'Event.CloseDateGMT CloseDate, '||
  	    'ClosedBy.EmployeeName ClosedBy, '||
  	    'NVL(EventData.ProductName,EventLot.ProductName) Product, '||
  	    'NVL(EventData.ProductRev,EventLot.ProductRev) ProductRev, '||
  	    'EventData.MaintenanceReqName MaintenanceReqName, '||
  	    'NVL(EventLot.OperationName,EventData.OperationName )OperationName, '||
  	    'EventData.ResourceName ResourceName, '||
  	    'FailureMode.FailureModeName  '||
  	    'FROM  event  '||
  	    'inner JOIN  EventData ON Event.EventId=EventData.EventId  '||
  	    'LEFT OUTER JOIN  EventFailure on eventdata.EventDataId = EventFailure.EventDataId  '||
  	    'LEFT OUTER JOIN FailureMode on EventFailure.FailureModeId = FailureMode.FailureModeId '||
  	    'inner JOIN  Classification ON Event.ClassificationId =Classification.ClassificationId  '||
  	    'inner JOIN  SubClassification ON Event.SubClassificationId =SubClassification.SubClassificationId  '||
  	    'LEFT OUTER JOIN  Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId  '||
  	    'LEFT OUTER JOIN  Organization ON Event.OrganizationId=Organization.OrganizationId  '||
  	    'LEFT OUTER JOIN  Employee Owner ON Event.OwnerId=Owner.EmployeeId  '||
  	    'inner JOIN  PriorityLevel ON Event.PriorityLevelId=PriorityLevel.PriorityLevelId     '||
  	    'inner JOIN  Employee Reporter ON Event.ReporterId=Reporter.EmployeeId  '||
  	    'inner JOIN  Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId  '||
  	    'inner JOIN  Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId  '||
  	    'inner JOIN  Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId  '||
  	    'left outer join  EventLot on Eventdata.EventDataId = EventLot.EventDataId  '||
  	    'where Event.Status in (1,3,4) '||
  	    'and  Event.Category = 3 '||
		'and trunc(Event.ReportedDate) >= trunc(sysdate - 90)';
  
    csiSTInstall_NewDef('csiNonconformance', 'Gets Nonconformance information for the Process Health Dashboard.', NULL, vSQLString, 1, 'Nonconformance', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    
    vSQLString := 'Select  '|| 
    'ProductName, '|| 
    'ProductRevision, '|| 
    'ProductFamilyname '|| 
    'from  '|| 
    'Product '|| 
    'Inner Join ProductBase on Product.ProductBaseid = ProductBase.ProductBaseID '|| 
    'Inner Join ProductFamily on Product.ProductFamilyId= ProductFamily.ProductFamilyId ';
    csiSTInstall_NewDef('csiProductFamily', 'Gets the Product, Product Family Information', NULL, vSQLString, 1, 'ProductFamily', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    vSQLString := 'select   '|| 
    'csiView_FirstPassYield.txnDate,  '|| 
    'csiView_FirstPassYield.ProductName,  '|| 
    'csiView_FirstPassYield.ProductRevision,  '|| 
    'csiView_FirstPassYield.WorkflowStepName,  '|| 
    'csiView_FirstPassYield.Sequence,  '|| 
    'csiView_FirstPassYield.Loss,  '|| 
    'csiView_FirstPassYield.Move, '|| 
    '( '|| 
    'select   '|| 
    'EXP(SUM(LN(ABS(Yield)))) Yield   '|| 
    'from csiView_FirstPassYield  ct  '|| 
    'where Yield <>0  '|| 
    'AND txndate>=sysdate-6  '|| 
    'and ct.ProductName= csiView_FirstPassYield.ProductName  '|| 
    'and ct.WorkflowStepName = csiView_FirstPassYield.WorkflowStepName  '|| 
    'group by   '|| 
    'WorkflowstepName  '|| 
    ') as Yield_Workflow,  '|| 
    '( '|| 
    'select  '|| 
    'EXP(SUM(LN(ABS(Yield)))) Yield   '|| 
    'from csiView_FirstPassYield ct3  '|| 
    'where Yield <>0  '|| 
    'AND txndate>=sysdate-6  '|| 
    'and ct3.TxnDate = csiView_FirstPassYield.TxnDate  '|| 
    'group by   '|| 
    'txndate  '|| 
    ') as Yield_Day,  '|| 
    '( '|| 
    'select  '|| 
    'EXP(SUM(LN(ABS(Yield)))) Yield   '|| 
    'from csiView_FirstPassYield ct2  '|| 
    'where Yield <>0  '|| 
    'AND txndate>=sysdate-6  '|| 
    'and ct2.ProductName= csiView_FirstPassYield.ProductName  '|| 
    'group by   '|| 
    'Productname  '|| 
    ') as Yield_Product  '|| 
    'from  '|| 
    'csiView_FirstPassYield  '|| 
    'where  txndate>=sysdate-6  '|| 
    'order by  '|| 
    'ProductName,  '|| 
    'Sequence,  '|| 
    'WorkflowStepName  ';
    csiSTInstall_NewDef('csiProductionDashboardYield', 'Used for the Yield report on the Production Dashboard.', NULL, vSQLString, 1, 'ProdDashbrdYield', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    vSQLString := 'Select  '|| 
    'sum(CycleTime)/COUNT(distinct ContainerName) Cycletime_Product, '|| 
    'sum(CycleTime) Cycletime, '|| 
    'MIN(Qty) Qty, '|| 
    'case when (sum(Cycletime)) <>0 then min(Qty)/(sum(Cycletime)) else 0 end as Throughput_Product, '|| 
    'ProductName, '|| 
    'containername '|| 
    'from csiTbl_ProductionDashboard  '|| 
    'Group by  '|| 
    'ProductName, '|| 
    'Containername ';
    csiSTInstall_NewDef('csiThroughputProduct', 'Gets Product Throughput Information for Production Dashboard.', NULL, vSQLString, 1, 'ThroughputProduct', 0, NULL, NULL, '0,', NULL, 1,1); 
    
    vSQLString := 'Select  '|| 
    'ProductName, '|| 
    'ProductRevision, '|| 
    'Sequence, '|| 
    'WorkflowStepName, '|| 
    'ContainerName, '|| 
    'min(Qty) as Qty, '|| 
    'Sum(Cycletime) as Cycletime, '|| 
    'case when (sum(Cycletime)) <>0 then min(Qty)/(sum(Cycletime)) else 0 end as Throughput_Step '|| 
    'From  '|| 
    'csiTbl_ProductionDashboard  '|| 
    'group by  '|| 
    'ProductName, '|| 
    'ProductRevision, '|| 
    'Sequence, '|| 
    'WorkflowStepName, '|| 
    'containerName ';
    csiSTInstall_NewDef('csiThroughputStep', 'Gets Step Throughput Information for Production Dashboard.', NULL, vSQLString, 1, 'ThroughputStep', 0, NULL, NULL, '0,', NULL, 1,1); 
END;
/

BEGIN
   csiST_PopulateDefaultData;
   COMMIT;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiST_PopulateDefaultData';
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiSTInstall_NewDef';
END;
/
/******************************************************************************************
//
//
//
//
//
/******************************************************************************************/
--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2023  

CREATE OR REPLACE PROCEDURE csiSTInstall_NewUserQuery(pName VARCHAR2
                                                      ,pDescription VARCHAR2
                                                      ,pNotes VARCHAR2
                                                      ,pQueryText VARCHAR2                                                      
                                                      ,pIsFrozen NUMBER
                                                      ,pParam1Name VARCHAR2
                                                      ,pParam1Type NUMBER
                                                      ,pParam2Name VARCHAR2
                                                      ,pParam2Type NUMBER
                                                      ,pParam3Name VARCHAR2
                                                      ,pParam3Type NUMBER)
AS
    vCDODefId       NUMBER := 7068;
    vInstanceID     CHAR(16);
    vCurrInstanceID CHAR(16);
    vParamCDODefId  NUMBER := 7071;
    vParamFieldId   NUMBER := 8597;
    vQueryTypeId    NUMBER := 4;
    vIID            CHAR(16);
    vCount          NUMBER;
BEGIN
   
    -- Remove the existing UserQuery and its Parameters, if it exists
    SELECT COUNT(*)
    INTO vCount
    FROM UserQuery
    WHERE UPPER(UserQueryName) = UPPER(pName);
    
    IF (vCount > 0) THEN    
       BEGIN
          SELECT UserQueryId
          INTO vCurrInstanceID
          FROM UserQuery
          WHERE UserQueryName = pName;
               
          DELETE FROM UserQueryParameter WHERE UserQueryId = vCurrInstanceID;
          DELETE FROM UserQueryUserQueryParameters WHERE UserQueryId = vCurrInstanceID;
          DELETE FROM UserQuery WHERE UserQueryId = vCurrInstanceID;
       EXCEPTION 
          WHEN OTHERS THEN NULL;
       END;
    END IF;

    csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
    INSERT INTO UserQuery(UserQueryId
                         ,UserQueryName
                         ,Description
                         ,Notes
                         ,QueryText
                         ,IsFrozen
                         ,CDOTypeId
                         ,QueryTypeId)
       VALUES (vInstanceId
              ,pName
              ,pDescription
              ,pNotes
              ,pQueryText
              ,pIsFrozen
              ,vCDODefId
              ,vQueryTypeId);
              
    IF (pParam1Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam1Name,vInstanceID,pIsFrozen,pParam1Type,0);
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,1,vInstanceID,vIID);
    END IF;
    
    IF (pParam2Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam2Name,vInstanceID,pIsFrozen,pParam2Type,0);
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,2,vInstanceID,vIID);
    END IF;
    
    IF (pParam3Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam3Name,vInstanceID,pIsFrozen,pParam3Type,0);
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,3,vInstanceID,vIID);
    END IF;
    
END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: csiST_PopulateDefaultUQData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  

CREATE OR REPLACE PROCEDURE csiST_PopulateDefaultUQData
AS
   VSQLString CLOB;
BEGIN
    vSQLString := 'Select wfs2.WorkflowStepName as FromStep '|| 
    ',sum(movehistory.CycleTime)*24*60 as ProcessTime '|| 
    ',sum(mih.CycleTime)*24*60 as waittime '|| 
    ',(sum(mih.CycleTime)*24*60 + sum(movehistory.CycleTime)*24*60)/COUNT(c.ContainerId) as CycleTime '|| 
    ',COUNT(c.containername) as ContainerCount '|| 
    'from MoveHistory '|| 
    'Inner Join Container c on MoveHistory.HistoryId = c.containerID '|| 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId '|| 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId '|| 
    'Inner join MoveInHistory mih on c.ContainerId = mih.HistoryId '|| 
    'Inner join HistoryMainline hml2 on mih.HistoryMainlineId = hml2.HistoryMainlineId '|| 
    'Where '|| 
    'TRUNC(hml.txnDate) = TRUNC(SYSDATE) '|| 
    'and hml2.containerId = c.containerId and hml2.WorkflowStepId=wfs2.WorkflowStepId  '|| 
    'and wfs2.WorkflowStepName = ?WorkflowStepName '|| 
    'Group By wfs2.WorkflowStepName ';

    csiSTInstall_NewUserQuery('CurrentCycletimeMetric','CycleTime by step in hours','OOB CycleTime query for the OOB PI in the Camstar Portal.',vSQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL);
   
   
    vSQLString := 'Select wfs2.WorkflowStepName as FromStep ,Sum(movehistory.qty) as goodQty ,Sum(MoveHistory.Qty)+Sum(ABS(NVL(qhd.Qty,0)*NVL(qhd.QtyMultiplier,0))) as TotalQtyscrappedPlusGood  '|| 
    'from MoveHistory  '|| 
    'Inner Join Container c on MoveHistory.HistoryId = c.containerID  '|| 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId  '|| 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId  '|| 
    'Left  join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId  '|| 
    'Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId   '|| 
    'Where TRUNC(hml.txnDate) = TRUNC(SYSDATE) '|| 
    'and wfs2.WorkflowStepName = ?workflowstepname '|| 
    'Group By wfs2.workflowstepname  ';
    
    csiSTInstall_NewUserQuery('CurrentThroughputMetric','Current Throughput by workflowstep','OOB Throughput query for the OOB PI in the Camstar Portal.',vSQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL);
   
   
    vSQLString := 'Select (Sum(movehistory.qty)/(Sum(MoveHistory.Qty)+Sum(ABS(NVL(qhd.Qty,0)*NVL(qhd.QtyMultiplier,0)))))*100 as yield  '|| 
    'from MoveHistory Inner Join Container c on MoveHistory.HistoryId = c.containerID '|| 
    'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId  '|| 
    'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId  '|| 
    'Left join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId  '|| 
    'Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId   '|| 
    'Where TRUNC(hml.txnDate) = TRUNC(SYSDATE)  '|| 
    'and wfs2.WorkflowStepName = ?WorkflowStepName '|| 
    'Group By wfs2.workflowstepname ';
    
    csiSTInstall_NewUserQuery('CurrentYieldMetric','Current Yield by workflowstep','OOB Yield query for the OOB PI in the Camstar Portal.',vSQLString,0,'WorkflowStepName',4,NULL,NULL,NULL,NULL);
   
END;
/
BEGIN
   csiST_PopulateDefaultUQData;
   COMMIT;
   
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiST_PopulateDefaultUQData';
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiSTInstall_NewUserQuery';
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiCreateView';
END;
/
