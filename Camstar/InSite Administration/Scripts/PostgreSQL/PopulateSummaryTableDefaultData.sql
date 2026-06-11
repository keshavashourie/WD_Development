--------------------------------------------------------------------------------
-- SCRIPT:PopulateSummaryTableDefaultData.sql
-- DESCR: Creates stored procedures used to create Summary Table Definitions
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2024  


--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewDefinition
-- DESCR: 
--
-- Copyright Siemens 2024  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_CreateNewDefinition')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_CreateNewDefinition;
 	END IF;
END $$;
CREATE PROCEDURE csiSTInstall_CreateNewDefinition(
    p_Name VARCHAR(255),
    p_Description VARCHAR(512),
    p_Notes VARCHAR(512),
    p_SummarySQL TEXT,
    p_IsView INTEGER,
    p_TableName VARCHAR(255),
    p_IsManuallyExecuted INTEGER,
    p_ScheduleDaysOfWeek VARCHAR(255),
    p_ScheduleDaysOfMonth VARCHAR(255),
    p_ScheduleHours VARCHAR(255),
    p_ScheduleMonths VARCHAR(255),
    p_IsFrozen INTEGER,
    p_IsEnabled INTEGER
)
LANGUAGE PLPGSQL
AS $$
DECLARE
	InstanceId VARCHAR(16);
	CDODefId INTEGER := 8236;
BEGIN

	DELETE FROM SummaryTableDef WHERE SummaryTableDefName = p_Name;
	
	CALL csiPRDGetNextInstanceId(CDODefId, InstanceId);
	
	INSERT INTO SummaryTableDef(
        SummaryTableDefId,
        SummaryTableDefName,
        Description,
        Notes,
        SummarySQL,
        IsView,
        TargetTableName,
        IsManuallyExecuted,
        ScheduleDaysOfWeek,
        ScheduleDaysOfMonth,
        ScheduleHours,
        ScheduleMonths,
        IsFrozen,
        CDOTypeId,
        ChangeCount,
        IsEnabled
    )
    VALUES (
        InstanceId,
        p_Name,
        p_Description,
        p_Notes,
        p_SummarySQL,
        p_IsView,
        p_TableName,
        p_IsManuallyExecuted,
        p_ScheduleDaysOfWeek,
        p_ScheduleDaysOfMonth,
        p_ScheduleHours,
        p_ScheduleMonths,
        p_IsFrozen,
        CDODefId,
        0,
        p_IsEnabled
    );
END;
$$;

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_PopulateDefaultData
-- DESCR: Helper function to create Role record
--
-- Copyright Siemens 2023  

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_PopulateDefaultData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_PopulateDefaultData;
 	END IF;
END $$;
CREATE PROCEDURE csiSTInstall_PopulateDefaultData()
LANGUAGE plpgsql
AS $$
DECLARE
	SQLString TEXT;
BEGIN
	
	-- csiYield
	SQLString := N'WITH t1 AS (
		SELECT
			wfs2.workflowstepid,
			mh2.historyid,
			MIN(txndate) AS txndate
		FROM
			movehistory mh2
		INNER JOIN
			historymainline hml2 ON hml2.historymainlineid = mh2.historymainlineid AND mh2.historyid = hml2.containerid
		INNER JOIN
			workflowstep wfs2 ON wfs2.workflowstepid = mh2.stepid
		GROUP BY
			wfs2.workflowstepid,
			mh2.historyid
	),
	hml1 AS (
		SELECT
			historymainlineid,
			txndate,
			operationid,
			employeeid,
			basetxntype,
			workflowstepid,
			productid
		FROM
			historymainline hml
		WHERE
			hml.txndate >= CURRENT_DATE - INTERVAL ''90 days''
	)
	SELECT
		pb.productname,
		p.productrevision,
		wfs.workflowstepname,
		wfs.sequence,
		c.containername,
		t.txndate,
		SUM(t.qty) AS qty,
		t.productid,
		t.txnid,
		t.historyid,
		t.historymainlineid,
		cdodefinition.cdoname,
		operation.operationname,
		employee.employeename
	FROM
		(
			SELECT
				mh.historyid,
				mh.historymainlineid,
				mh.txnid,
				mh.tostepid AS workflowstepid,
				mh.qty,
				mh.productid,
				hml1.txndate,
				hml1.operationid,
				hml1.employeeid,
				hml1.basetxntype
			FROM
				movehistory mh
			INNER JOIN
				hml1 ON mh.historymainlineid = hml1.historymainlineid
			INNER JOIN
				t1 ON t1.workflowstepid = mh.tostepid AND t1.historyid = mh.historyid AND t1.txndate = hml1.txndate
			WHERE
				mh.reworkreasonid IS NULL

			UNION

			SELECT
				qh.historyid,
				qh.historymainlineid,
				qh.txnid,
				hml1.workflowstepid,
				qh.qty,
				hml1.productid,
				hml1.txndate,
				hml1.operationid,
				hml1.employeeid,
				hml1.basetxntype
			FROM
				qtyhistory qh
			INNER JOIN
				hml1 ON qh.historymainlineid = hml1.historymainlineid
			INNER JOIN
				t1 ON t1.historyid = qh.historyid AND hml1.txndate > t1.txndate

			UNION

			SELECT
				starthistorydetail.historyid,
				starthistorydetail.historymainlineid,
				starthistorydetail.txnid,
				starthistorydetail.workflowstepid,
				starthistorydetail.qty,
				hml1.productid,
				hml1.txndate,
				hml1.operationid,
				hml1.employeeid,
				hml1.basetxntype
			FROM
				starthistorydetail
			INNER JOIN
				hml1 ON starthistorydetail.historymainlineid = hml1.historymainlineid

			UNION

			SELECT
				mh.historyid,
				mh.historymainlineid,
				mh.txnid,
				mh.stepid AS workflowstepid,
				mh.qty,
				mh.productid,
				hml1.txndate,
				hml1.operationid,
				hml1.employeeid,
				hml1.basetxntype
			FROM
				movehistory mh
			INNER JOIN
				hml1 ON mh.historymainlineid = hml1.historymainlineid
			INNER JOIN
				t1 ON t1.workflowstepid = mh.tostepid AND t1.historyid = mh.historyid AND hml1.txndate > t1.txndate
			WHERE
				mh.cumulativereworkstepcount = 1
		) t
	INNER JOIN
		container c ON t.historyid = c.containerid
	INNER JOIN
		employee ON t.employeeid = employee.employeeid
	INNER JOIN
		operation ON t.operationid = operation.operationid
	INNER JOIN
		product p ON t.productid = p.productid
	INNER JOIN
		productbase pb ON pb.productbaseid = p.productbaseid
	INNER JOIN
		cdodefinition ON t.basetxntype = cdodefinition.cdodefid
	INNER JOIN
		workflowstep wfs ON t.workflowstepid = wfs.workflowstepid
	INNER JOIN
		workflow wf ON wfs.workflowid = wf.workflowid
	GROUP BY
		pb.productname,
		p.productrevision,
		wfs.workflowstepname,
		wfs.sequence,
		c.containername,
		t.txndate,
		t.productid,
		t.txnid,
		t.historyid,
		t.historymainlineid,
		cdodefinition.cdoname,
		operation.operationname,
		employee.employeename';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiYield',
		N'Yield',
		null::VARCHAR(512),
		SQLString,
		0,
		'Yield',
		0,
		null::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,4,8,12,16,20',
		NULL::VARCHAR(255),
		1,
		1
	);
	
	-- csiProductionDashboard
	SQLString := N'SELECT
		CDODefinition.CDOName,
		HistoryMainline.Txndate,
		Container.ContainerName,
		WorkflowStep.WorkflowStepName,
		WorkflowStep.Sequence,
		ProductBase.ProductName,
		Product.ProductRevision,
		Container.qty AS ContainerQty,
		coalesce(MoveHistory.qty, MoveInHistory.Qty) AS Qty,
		coalesce(MoveHistory.CycleTime, MoveInHistory.CycleTime) * 24 AS Cycletime
	FROM
		HistoryMainline HistoryMainline
		INNER JOIN CDODefinition ON HistoryMainline.TxnType = CDODefinition.CDODefID
		LEFT OUTER JOIN MoveHistory ON HistoryMainline.HistoryMainlineId = MoveHistory.HistoryMainlineId
		LEFT OUTER JOIN MoveInHistory ON HistoryMainline.HistoryMainlineId = MoveInHistory.HistoryMainlineId
		INNER JOIN Container Container ON HistoryMainline.ContainerId = Container.ContainerId
		INNER JOIN Product Product ON HistoryMainline.ProductId = Product.ProductId
		INNER JOIN WorkflowStep WorkflowStep ON HistoryMainline.WorkflowStepId = WorkflowStep.WorkflowStepId
		INNER JOIN ProductBase ProductBase ON Product.ProductBaseId = ProductBase.ProductBaseId
	WHERE
		HistoryMainline.ContainerId IN (
			SELECT
				Container.ContainerId
			FROM
				HistoryMainline HistoryMainline
				INNER JOIN MoveHistory MoveHistory ON HistoryMainline.HistoryMainlineId = MoveHistory.HistoryMainlineId
				INNER JOIN Container Container ON HistoryMainline.ContainerId = Container.ContainerId
				INNER JOIN WorkflowStep FinalStep ON MoveHistory.ToStepId = FinalStep.WorkflowStepId
			WHERE
				FinalStep.IsLastStep = 1
				AND HistoryMainline.TxnDate >= clock_timestamp()  - INTERVAL ''6 days''
		)';
		
	CALL csiSTInstall_CreateNewDefinition(
		N'csiProductionDashboard',
		N'ProductionDashboard',
		NULL::VARCHAR(512),
		SQLString,
		0,
		'ProductionDashboard',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,4,8,12,16,20',
		NULL::VARCHAR(255),
		1,
		1
	);
	
	-- csiFirstPassYield
    SQLString := N'SELECT
		DATE_TRUNC(''day'', TxnDate) AS TxnDate,
		ProductName,
		ProductRevision,
		WorkflowStepName,
		Sequence,
		SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) AS Move,
		SUM(CASE WHEN CDOName IN (''Rework'', ''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END) AS Loss,
		CASE
			WHEN SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) > 0 THEN
				((SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END) +
				SUM(CASE WHEN CDOName IN (''Rework'', ''ChangeQty'', ''Scrap'') THEN qty ELSE 0 END)) /
				SUM(CASE WHEN CDOName IN (''MoveTxn'', ''Start'') THEN qty ELSE 0 END))
			ELSE 0
		END AS Yield
	FROM
		csiTbl_Yield
	WHERE
		DATE_TRUNC(''day'', TxnDate) >= clock_timestamp() - interval ''6 days''
	GROUP BY
		DATE_TRUNC(''day'', TxnDate),
		ProductName,
		ProductRevision,
		Sequence,
		WorkflowStepName';
		
	CALL csiSTInstall_CreateNewDefinition(
		N'csiFirstPassYield', 
		N'', 
		N'', 
		SQLString, 
		1, 
		'FirstPassYield', 
		0, 
		NULL::VARCHAR(255), 
		NULL::VARCHAR(255), 
		NULL::VARCHAR(255), 
		NULL::VARCHAR(255), 
		1, 
		0
	);
	
	-- csiQualityObject
	SQLString := N'SELECT Event.EventID InstanceID,Event.EventName Name,Event.EventDataID, Event.BriefDescription, Event.Description, Event.ReportedDate ReportedDate, Event.AttachmentsId, EventLot.Lot,EventLot.Qty, (select CategoryField.FieldName from CDOFields CategoryField where Event.Category=CategoryField.DefaultValue::integer  AND CategoryField.CDODefId=7520 ) as CategoryName, Event.Category,Classification.ClassificationName Classification, SubClassification.SubClassificationName SubClassification, Event.DiscoveryArea,Event.OccurrenceDate OccurrenceDate, Organization.OrganizationName Organization,Owner.EmployeeName Owner , (select PriorityLevel.PriorityLevelName PriorityLevel from PriorityLevel where  Event.PriorityLevelId=PriorityLevel.PriorityLevelId) as PriorityLevel, Reporter.EmployeeName Reporter,ReporterOrganization.OrganizationName ReporterOrganization,Initiator.EmployeeName Initiator, InitiatorOrganization.OrganizationName InitiatorOrganization, (select Role.RoleName  from RoleDef Role  where Event.RoleId=Role.RoleId ) as Role, (select StatusField.FieldName from CDOFields StatusField where Event.Status=StatusField.DefaultValue::integer  AND StatusField.CDODefId=7658) as StatusName, Event.Status, (select QualityResolutionCode.QualityResolutionCodeName from QualityResolutionCode       where Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId) as ResolutionCode, Event.CloseDescription CloseDescription,Event.CloseDate CloseDate, ClosedBy.EmployeeName ClosedBy,EventData.ProductName EventDataProduct, EventData.ProductRev EventDataProductRev, EventLot.ProductName EventLotProduct,EventLot.ProductRev EventLotProductRev, EventData.MaintenanceReqName MaintenanceReqName,  EventData.OperationName OperationName,EventData.ResourceName ResourceName, EventData.WorkflowName,Eventdata.WorkflowRev, EventData.WorkflowStepName,EventData.EventDate,FailureMode.FailureModeName, EventFailure.FailureModeId,EventFailure.EventFailureId, (select ProductFamilyName From ProductFamily, Product, ProductBase where  Product.ProductFamilyId = ProductFamily.ProductFamilyID and  Product.ProductBaseId=ProductBase.ProductBaseId and  EventData.ProductName = Productbase.ProductName  And EventData.ProductRev = Product.ProductRevision) as ProductFamilyName FROM  event  inner JOIN  EventData ON Event.EventId=EventData.EventId  LEFT OUTER JOIN  EventFailure on eventdata.EventDataId = EventFailure.EventDataId  LEFT OUTER JOIN FailureMode on EventFailure.FailureModeId = FailureMode.FailureModeId  inner JOIN  Classification ON Event.ClassificationId =Classification.ClassificationId  inner JOIN  SubClassification ON Event.SubClassificationId =SubClassification.SubClassificationId   LEFT OUTER JOIN  Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId  LEFT OUTER JOIN  Organization ON Event.OrganizationId=Organization.OrganizationId  LEFT OUTER JOIN  Employee Owner ON Event.OwnerId=Owner.EmployeeId  inner JOIN  Employee Reporter ON Event.ReporterId=Reporter.EmployeeId  inner JOIN  Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId  inner JOIN  Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId  inner JOIN  Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId left outer join  EventLot on Event.EventDataId = EventLot.EventDataId where  Event.ReportedDate >= clock_timestamp() - INTERVAL ''90 days''';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiQualityObject',
		N'QualityObject',
		NULL::VARCHAR(512),
		SQLString,
		0,
		'QualityObject',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,4,8,12,16,20',
		NULL::VARCHAR(255),
		1,
		1
	);
	
	-- csiOpenOrdersSummary (pending validation 90 days)
	SQLString := N'SELECT
		c.ContainerId,
		c.ContainerName,
		c.FactoryStartQty,
		c.Qty AS ContainerCurrentQty,
		c.OnHoldDate,
		c.LastActivityDate,
		UOM.UOMName,
		cs.InRework,
		wfs.IsLastStep,
		CASE WHEN wfs.IsLastStep = 1 THEN cs.LastMoveDate END AS MoveToLastStepDate,
		CASE WHEN wfs.IsLastStep = 1 THEN EXTRACT(EPOCH FROM (cs.lastMoveDate - c.factoryStartDate)) / c.Qty END AS secondsPerPiece,
		CASE WHEN wfs.IsLastStep = 1 THEN EXTRACT(EPOCH FROM (cs.lastMoveDate - c.factoryStartDate)) END AS secondsPerContainer,
		c.HoldReasonId,
		wfs.WorkflowStepName,
		MfgOrder.MfgOrderId,
		MfgOrder.MfgOrderName,
		MfgOrder.PlannedCompletionDate,
		MfgOrder.DefaultLot,
		OrderStatus.OrderStatusName,
		MfgOrder.PlannedStartDate,
		MfgOrder.Qty AS OrderQty,
		pb.ProductName,
		p.ProductRevision,
		p.ProductId,
		coalesce(CASE WHEN wfs.IsLastStep = 1 THEN c.Qty END, 0) AS ContainerCompletedQty,
		coalesce(CASE WHEN wfs.IsLastStep = 1 THEN c.FactoryStartQty END, 0) AS CompletedStartQty
	FROM
		Container AS c
	INNER JOIN
		CurrentStatus AS cs ON c.CurrentStatusId = cs.CurrentStatusId
	INNER JOIN
		WorkflowStep AS wfs ON cs.WorkflowStepId = wfs.WorkflowStepId
	INNER JOIN
		MfgOrder ON c.MfgOrderId = MfgOrder.MfgOrderId
	LEFT OUTER JOIN
		OrderStatus ON MfgOrder.OrderStatusId = OrderStatus.OrderStatusId
	LEFT OUTER JOIN
		PriorityCode ON MfgOrder.PriorityId = PriorityCode.PriorityCodeId
	INNER JOIN
		UOM ON c.UOMId = UOM.UOMId
	INNER JOIN
		Product AS p ON c.ProductId = p.ProductId
	INNER JOIN
		ProductBase AS pb ON p.ProductBaseId = pb.ProductBaseId
	WHERE
		MfgOrder.MfgOrderId IN (
			SELECT DISTINCT Container.MfgOrderId
			FROM Container
			INNER JOIN CurrentStatus ON Container.CurrentStatusId = CurrentStatus.CurrentStatusId
			INNER JOIN WorkflowStep ON CurrentStatus.WorkflowStepId = WorkflowStep.WorkflowStepId
			WHERE WorkflowStep.IsLastStep = 0
		) AND c.LastActivityDate >= clock_timestamp() - INTERVAL ''90 days''';
		
	CALL csiSTInstall_CreateNewDefinition(
		N'csiOpenOrdersSummary',
		null::VARCHAR(512),
		NULL::VARCHAR(512),
		SQLString,
		1,
		'OpenOrdersSummary',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		null::VARCHAR(255),
		0,
		1
	);
	
	-- csiFactoryPerformanceSummary
	SQLString := N'WITH cont_data AS (
		SELECT 
			p.productid AS productid, 
			pb.productname AS productname, 
			p.productrevision AS productrevision, 
			wfs.workflowstepname AS workflowstepname, 
			wfs.islaststep AS islaststep, 
			wfs.workflowstepid AS workflowstepid, 
			wfs.sequence AS sequence, 
			wf.workflowid AS workflowid, 
			c.containername AS containername, 
			hml.resourceid AS resourceid, 
			hml.txndate AS txndate, 
			cdodefinition.cdoname AS cdoname, 
			c.ownerid AS ownerid, 
			c.mfgorderid AS mfgorderid, 
			hml.factoryid AS factoryid, 
			hml.shiftname AS shiftname, 
			hml.calendarshiftid AS calendarshiftid, 
			hml.steppass AS steppass, 
			'''' AS reasoncodeid, 
			sd.normalcycletime AS targetcycletime, 
			sd.yield AS targetyield, 
			sd.unitsperhour AS targetunitsperhour, 
			c.containerid AS containerid, 
			hml.historymainlineid AS historymainlineid, 
			hml.historyid AS historyid 
		FROM 
			container c 
			INNER JOIN historymainline hml ON hml.containerid = c.containerid 
			INNER JOIN workflowstep wfs ON hml.workflowstepid = wfs.workflowstepid 
			LEFT OUTER JOIN stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid 
			INNER JOIN product p ON hml.productid = p.productid 
			INNER JOIN productbase pb ON pb.productbaseid = p.productbaseid 
			INNER JOIN cdodefinition ON hml.basetxntype = cdodefinition.cdodefid 
			INNER JOIN workflow wf ON wfs.workflowid = wf.workflowid 
		WHERE 
			hml.txndate >= current_date - interval ''90 days''
	)
	SELECT 
		movehistory.qty AS qty, 
		movehistory.cycletime AS movecycletime, 
		cont_data.productname, 
		cont_data.productrevision, 
		cont_data.productid, 
		wfs2.workflowstepname AS workflowstepname, 
		wfs2.islaststep, 
		wfs2.workflowstepid AS stepid, 
		wfs2.sequence AS stepsequence, 
		cont_data.workflowid, 
		cont_data.containername, 
		cont_data.txndate, 
		cont_data.historyid, 
		hml1.historymainlineid, 
		cont_data.cdoname, 
		cont_data.ownerid, 
		movehistory.toresourceid, 
		cont_data.mfgorderid, 
		cont_data.factoryid, 
		cont_data.shiftname, 
		cont_data.calendarshiftid, 
		cont_data.workflowstepname AS fromstep, 
		cont_data.workflowstepid AS fromstepid, 
		cont_data.sequence AS fromstepsequence, 
		cont_data.steppass, 
		'''' AS reasoncodeid, 
		sd2.normalcycletime AS targetcycletime, 
		cont_data.targetyield AS targetyield, 
		sd2.unitsperhour AS targetunitsperhour, 
		clock_timestamp() AS lastrefreshdate 
	FROM 
		cont_data, movehistory 
		INNER JOIN historymainline hml1 ON movehistory.historymainlineid = hml1.historymainlineid 
		INNER JOIN workflowstep wfs2 ON movehistory.tostepid = wfs2.workflowstepid 
		LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid 
	WHERE 
		movehistory.reworkreasonid IS NULL 
		AND movehistory.historyid = cont_data.containerid 
		AND MoveHistory.HistoryMainlineId = cont_data.historymainlineid 
		AND movehistory.stepid = cont_data.workflowstepid 
		AND wfs2.sequence <> 1 

	UNION

	SELECT 
		qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier AS qty, 
		0 AS movecycletime, 
		cont_data.productname, 
		cont_data.productrevision, 
		cont_data.productid, 
		cont_data.workflowstepname, 
		cont_data.islaststep, 
		cont_data.workflowstepid, 
		cont_data.sequence, 
		cont_data.workflowid, 
		cont_data.containername, 
		cont_data.txndate, 
		qtyhistory.historyid, 
		qtyhistory.historymainlineid, 
		cont_data.cdoname, 
		cont_data.ownerid, 
		qtyhistory.resourceid, 
		cont_data.mfgorderid, 
		cont_data.factoryid, 
		cont_data.shiftname, 
		cont_data.calendarshiftid, 
		'''' AS fromstep, 
		'''' AS fromstepid, 
		null::integer AS fromstepsequence, 
		cont_data.steppass, 
		qtyhistorydetails.reasoncodeid AS reasoncodeid, 
		null::double precision AS targetcycletime, 
		cont_data.targetyield AS targetyield, 
		null::double precision AS targetunitsperhour, 
		clock_timestamp() AS lastrefreshdate 
	FROM 
		cont_data, qtyhistory 
		INNER JOIN qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid 
	WHERE 
		qtyhistory.historyid = cont_data.containerid 
		AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid 
		AND qtyhistory.historymainlineid = cont_data.historymainlineid 

	UNION

	SELECT 
		COALESCE(starthistorydetail.qty, 0) AS qty, 
		0 AS movecycletime, 
		cont_data.productname, 
		cont_data.productrevision, 
		cont_data.productid, 
		cont_data.workflowstepname, 
		cont_data.islaststep, 
		cont_data.workflowstepid, 
		cont_data.sequence, 
		cont_data.workflowid, 
		cont_data.containername, 
		cont_data.txndate, 
		starthistorydetail.historyid, 
		starthistorydetail.historymainlineid, 
		cont_data.cdoname, 
		cont_data.ownerid, 
		starthistorydetail.resourceid, 
		starthistorydetail.mfgorderid, 
		cont_data.factoryid, 
		cont_data.shiftname, 
		cont_data.calendarshiftid, 
		'''' AS fromstep, 
		'''' AS fromstepid, 
		null::integer AS fromstepsequence, 
		cont_data.steppass, 
		'''' AS reasoncodeid, 
		null::double precision AS targetcycletime, 
		cont_data.targetyield AS targetyield, 
		null::double precision AS targetunitsperhour, 
		clock_timestamp() AS lastrefreshdate 
	FROM 
		cont_data, starthistorydetail 
	WHERE 
		starthistorydetail.historyid = cont_data.containerid 
		AND starthistorydetail.historymainlineid = cont_data.historymainlineid 
		AND starthistorydetail.workflowstepid = cont_data.workflowstepid 

	UNION

	SELECT 
		(movehistory.qty) * -1 AS qty, 
		movehistory.cycletime AS movecycletime, 
		cont_data.productname, 
		cont_data.productrevision, 
		cont_data.productid, 
		cont_data.workflowstepname, 
		cont_data.islaststep, 
		cont_data.workflowstepid, 
		cont_data.sequence, 
		cont_data.workflowid, 
		cont_data.containername, 
		cont_data.txndate, 
		movehistory.historyid, 
		movehistory.historymainlineid, 
		cont_data.cdoname, 
		cont_data.ownerid, 
		movehistory.toresourceid, 
		cont_data.mfgorderid, 
		cont_data.factoryid, 
		cont_data.shiftname, 
		cont_data.calendarshiftid, 
		wfs2.workflowstepname AS fromstep, 
		wfs2.workflowstepid AS fromstepid, 
		wfs2.sequence AS fromstepsequence, 
		cont_data.steppass, 
		movehistory.reworkreasonid AS reasoncodeid, 
		sd2.normalcycletime AS targetcycletime, 
		cont_data.targetyield AS targetyield, 
		sd2.unitsperhour AS targetunitsperhour, 
		clock_timestamp() AS lastrefreshdate 
	FROM 
		cont_data, movehistory 
		INNER JOIN workflowstep wfs2 ON movehistory.stepid = wfs2.workflowstepid 
		LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid 
	WHERE 
		movehistory.reworkreasonid IS NOT NULL 
		AND movehistory.historyid = cont_data.containerid 
		AND movehistory.stepid = cont_data.workflowstepid 
		AND movehistory.historymainlineid = cont_data.historymainlineid 

	UNION

	SELECT 
		moveinhistory.qty AS qty, 
		moveinhistory.cycletime AS movecycletime, 
		cont_data.productname, 
		cont_data.productrevision, 
		cont_data.productid, 
		cont_data.workflowstepname, 
		cont_data.islaststep, 
		cont_data.workflowstepid, 
		cont_data.sequence, 
		cont_data.workflowid, 
		cont_data.containername, 
		cont_data.txndate, 
		moveinhistory.historyid, 
		moveinhistory.historymainlineid, 
		cont_data.cdoname, 
		cont_data.ownerid, 
		cont_data.resourceid, 
		cont_data.mfgorderid, 
		cont_data.factoryid, 
		cont_data.shiftname, 
		cont_data.calendarshiftid, 
		cont_data.workflowstepname AS fromstep, 
		cont_data.workflowstepid AS fromstepid, 
		cont_data.sequence AS fromstepsequence, 
		cont_data.steppass, 
		'''' AS reasoncodeid, 
		cont_data.targetcycletime AS targetcycletime, 
		cont_data.targetyield AS targetyield, 
		cont_data.targetunitsperhour AS targetunitsperhour, 
		clock_timestamp() AS lastrefreshdate 
	FROM 
		moveinhistory moveinhistory, cont_data 
	WHERE 
		moveinhistory.historyid = cont_data.containerid 
		AND moveinhistory.HistoryMainlineId = cont_data.historymainlineid';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiFactoryPerformanceSummary',
		null::varchar(512),
		NULL::VARCHAR(255),
		SQLString,
		0,
		'FactoryPerformanceSum',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csi24HrFactoryPerformance
	SQLString := N'
	WITH cont_data AS (
		SELECT 
			p.productid AS productid,
			pb.productname AS productname,
			p.productrevision AS productrevision,
			wfs.workflowstepname AS workflowstepname,
			wfs.islaststep AS islaststep,
			wfs.workflowstepid AS workflowstepid,
			wfs.sequence AS sequence,
			wf.workflowid AS workflowid,
			c.containername AS containername,
			hml.resourceid AS resourceid,
			hml.txndate AS txndate,
			cdodefinition.cdoname AS cdoname,
			c.ownerid AS ownerid,
			c.mfgorderid AS mfgorderid,
			hml.factoryid AS factoryid,
			hml.shiftname AS shiftname,
			hml.calendarshiftid AS calendarshiftid,
			hml.steppass AS steppass,
			'''' AS reasoncodeid,
			sd.normalcycletime AS targetcycletime,
			sd.yield AS targetyield,
			sd.unitsperhour AS targetunitsperhour,
			c.containerid AS containerid,
			hml.historymainlineid AS historymainlineid,
			hml.historyid AS historyid
		FROM 
			container c
			INNER JOIN historymainline hml ON hml.containerid = c.containerid
			INNER JOIN workflowstep wfs ON hml.workflowstepid = wfs.workflowstepid
			LEFT OUTER JOIN stepschedulingdetail sd ON wfs.schedulingdetailid = sd.stepschedulingdetailid
			INNER JOIN product p ON hml.productid = p.productid
			INNER JOIN productbase pb ON pb.productbaseid = p.productbaseid
			INNER JOIN cdodefinition ON hml.basetxntype = cdodefinition.cdodefid
			INNER JOIN workflow wf ON wfs.workflowid = wf.workflowid
		WHERE 
			hml.txndate >= current_date - interval ''90 days''
	)

	SELECT 
		movehistory.qty AS qty,
		movehistory.cycletime AS movecycletime,
		cont_data.productname,
		cont_data.productrevision,
		cont_data.productid,
		wfs2.workflowstepname AS workflowstepname,
		wfs2.islaststep,
		wfs2.workflowstepid AS stepid,
		wfs2.sequence AS stepsequence,
		cont_data.workflowid,
		cont_data.containername,
		cont_data.txndate,
		cont_data.historyid,
		hml1.historymainlineid,
		cont_data.cdoname,
		cont_data.ownerid,
		movehistory.toresourceid,
		cont_data.mfgorderid,
		cont_data.factoryid,
		cont_data.shiftname,
		cont_data.calendarshiftid,
		cont_data.workflowstepname AS fromstep,
		cont_data.workflowstepid AS fromstepid,
		cont_data.sequence AS fromstepsequence,
		cont_data.steppass,
		'''' AS reasoncodeid,
		sd2.normalcycletime AS targetcycletime,
		cont_data.targetyield AS targetyield,
		sd2.unitsperhour AS targetunitsperhour,
		clock_timestamp() AS lastrefreshdate
	FROM 
		cont_data, movehistory 
		INNER JOIN historymainline hml1 ON movehistory.historymainlineid = hml1.historymainlineid 
		INNER JOIN workflowstep wfs2 ON movehistory.tostepid = wfs2.workflowstepid 
		LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid 
	WHERE 
		movehistory.reworkreasonid IS NULL
		AND movehistory.historyid = cont_data.containerid
		AND MoveHistory.HistoryMainlineId = cont_data.historymainlineid
		AND movehistory.stepid = cont_data.workflowstepid
		AND wfs2.sequence <> 1

	UNION 

	SELECT 
		qtyhistorydetails.qty * qtyhistorydetails.qtymultiplier AS qty,
		0 AS movecycletime,
		cont_data.productname,
		cont_data.productrevision,
		cont_data.productid,
		cont_data.workflowstepname,
		cont_data.islaststep,
		cont_data.workflowstepid,
		cont_data.sequence,
		cont_data.workflowid,
		cont_data.containername,
		cont_data.txndate,
		qtyhistory.historyid,
		qtyhistory.historymainlineid,
		cont_data.cdoname,
		cont_data.ownerid,
		qtyhistory.resourceid,
		cont_data.mfgorderid,
		cont_data.factoryid,
		cont_data.shiftname,
		cont_data.calendarshiftid,
		'''' AS fromstep,
		'''' AS fromstepid,
		null::integer AS fromstepsequence,
		cont_data.steppass,
		qtyhistorydetails.reasoncodeid AS reasoncodeid,
		null::double precision AS targetcycletime,
		cont_data.targetyield AS targetyield,
		null::double precision AS targetunitsperhour,
		clock_timestamp() AS lastrefreshdate
	FROM 
		cont_data, qtyhistory 
		INNER JOIN qtyhistorydetails ON qtyhistory.qtyhistoryid = qtyhistorydetails.qtyhistoryid 
	WHERE 
		qtyhistory.historyid = cont_data.containerid
		AND qtyhistorydetails.chargetostepid = cont_data.workflowstepid
		AND qtyhistory.historymainlineid = cont_data.historymainlineid

	UNION 

	SELECT 
		COALESCE(starthistorydetail.qty, 0) AS qty, 
		0 AS movecycletime,
		cont_data.productname,
		cont_data.productrevision,
		cont_data.productid,
		cont_data.workflowstepname,
		cont_data.islaststep,
		cont_data.workflowstepid,
		cont_data.sequence,
		cont_data.workflowid,
		cont_data.containername,
		cont_data.txndate,
		starthistorydetail.historyid,
		starthistorydetail.historymainlineid,
		cont_data.cdoname,
		cont_data.ownerid,
		starthistorydetail.resourceid,
		starthistorydetail.mfgorderid,
		cont_data.factoryid,
		cont_data.shiftname,
		cont_data.calendarshiftid,
		'''' AS fromstep,
		'''' AS fromstepid,
		null::integer AS fromstepsequence,
		cont_data.steppass,
		'''' AS reasoncodeid,
		null::double precision AS targetcycletime,
		cont_data.targetyield AS targetyield,
		null::double precision AS targetunitsperhour,
		clock_timestamp() AS lastrefreshdate
	FROM 
		cont_data
		INNER JOIN starthistorydetail ON starthistorydetail.historyid = cont_data.containerid
		AND starthistorydetail.historymainlineid = cont_data.historymainlineid
		AND starthistorydetail.workflowstepid = cont_data.workflowstepid

	UNION 

	SELECT 
		(movehistory.qty) * -1 AS qty,
		movehistory.cycletime AS movecycletime,
		cont_data.productname,
		cont_data.productrevision,
		cont_data.productid,
		cont_data.workflowstepname,
		cont_data.islaststep,
		cont_data.workflowstepid,
		cont_data.sequence,
		cont_data.workflowid,
		cont_data.containername,
		cont_data.txndate,
		movehistory.historyid,
		movehistory.historymainlineid,
		cont_data.cdoname,
		cont_data.ownerid,
		movehistory.toresourceid,
		cont_data.mfgorderid,
		cont_data.factoryid,
		cont_data.shiftname,
		cont_data.calendarshiftid,
		wfs2.workflowstepname AS fromstep,
		wfs2.workflowstepid AS fromstepid,
		wfs2.sequence AS fromstepsequence,
		cont_data.steppass,
		movehistory.reworkreasonid AS reasoncodeid,
		sd2.normalcycletime AS targetcycletime,
		cont_data.targetyield AS targetyield,
		sd2.unitsperhour AS targetunitsperhour,
		clock_timestamp() AS lastrefreshdate
	FROM 
		cont_data, movehistory 
		INNER JOIN workflowstep wfs2 ON movehistory.stepid = wfs2.workflowstepid 
		LEFT OUTER JOIN stepschedulingdetail sd2 ON wfs2.schedulingdetailid = sd2.stepschedulingdetailid 
	WHERE 
		movehistory.reworkreasonid IS NOT NULL
		AND movehistory.historyid = cont_data.containerid
		AND movehistory.stepid = cont_data.workflowstepid
		AND movehistory.historymainlineid = cont_data.historymainlineid

	UNION 

	SELECT 
		moveinhistory.qty AS qty,
		moveinhistory.cycletime AS movecycletime,
		cont_data.productname,
		cont_data.productrevision,
		cont_data.productid,
		cont_data.workflowstepname,
		cont_data.islaststep,
		cont_data.workflowstepid,
		cont_data.sequence,
		cont_data.workflowid,
		cont_data.containername,
		cont_data.txndate,
		moveinhistory.historyid,
		moveinhistory.historymainlineid,
		cont_data.cdoname,
		cont_data.ownerid,
		cont_data.resourceid,
		cont_data.mfgorderid,
		cont_data.factoryid,
		cont_data.shiftname,
		cont_data.calendarshiftid,
		cont_data.workflowstepname AS fromstep,
		cont_data.workflowstepid AS fromstepid,
		cont_data.sequence AS fromstepsequence,
		cont_data.steppass,
		'''' AS reasoncodeid,
		cont_data.targetcycletime AS targetcycletime,
		cont_data.targetyield AS targetyield,
		cont_data.targetunitsperhour AS targetunitsperhour,
		clock_timestamp() AS lastrefreshdate
	FROM 
		moveinhistory moveinhistory
		INNER JOIN cont_data ON moveinhistory.historyid = cont_data.containerid
		AND moveinhistory.HistoryMainlineId = cont_data.historymainlineid';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csi24HrFactoryPerformance',
		null::varchar(512),
		NULL::VARCHAR(255),
		SQLString,
		1,
		'24HrFactoryPerformance',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiWFResourceDowntime
	SQLString := N'SELECT
		wf.WorkflowId,
		wf.WorkflowRevision,
		wfb.WorkflowName,
		wfs.WorkflowStepName,
		wfs.workflowstepid,
		s.SpecId,
		r.ResourceName,
		rg.ResourceGroupName,
		ps.MfgOrderId,
		ps.LastActivityDate,
		ps.LastStatusChangeDate,
		TO_CHAR(
			clock_timestamp() - (ps.LastStatusChangeDate - clock_timestamp())::INTERVAL,
			''HH24:MI:SS''
		)AS TimeDown,
		ps.ContainerId,
		ps.ResourceId,
		ps.ResourceState,
		ps.Availability,
		ps.ReasonId,
		ps.StatusId,
		rsc.ResourceStatusCodeName,
		rsr.ResourceStatusReasonName
	FROM
		Workflow wf
	INNER JOIN
		WorkflowBase wfb ON wf.WorkflowBaseId = wfb.WorkflowBaseId
	INNER JOIN
		WorkflowStep wfs ON wfs.WorkflowId = wf.WorkflowId
	INNER JOIN
		Spec s ON wfs.SpecId = s.SpecId OR wfs.SpecBaseId = s.specbaseid
	INNER JOIN
		ResourceGroup rg ON s.resourcegroupid = rg.ResourceGroupId
	INNER JOIN
		ResourceGroupEntries rge ON rg.ResourceGroupId = rge.ResourceGroupId
	INNER JOIN
		ResourceDef r ON rge.EntriesId = r.ResourceId
	INNER JOIN
		ProductionStatus ps ON r.ResourceId = ps.ResourceId
	LEFT OUTER JOIN
		ResourceStatusCode rsc ON ps.StatusId = rsc.ResourceStatusCodeId
	LEFT OUTER JOIN
		ResourceStatusReason rsr ON ps.ReasonId = rsr.ResourceStatusReasonId
	WHERE ps.LastActivityDate >= clock_timestamp() - INTERVAL ''90 days''';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiWFResourceDowntime',
		null::varchar(512),
		NULL::VARCHAR(512),
		SQLString,
		1,
		'csiWFResourceDowntime',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiCycleTimeProduct
	SQLString := N'SELECT
		SUM(CycleTime) / COUNT(DISTINCT ContainerName) AS Cycletime_Product,
		SUM(CycleTime) AS Cycletime,
		MIN(ContainerQty) AS Qty,
		SUM(CycleTime) / MIN(ContainerQty) AS Throughput_Product,
		ProductName,
		ContainerName
	FROM
		csiTbl_ProductionDashboard
	GROUP BY
		ProductName,
		ContainerName';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiCycleTimeProduct',
		N'Gets Product Cycle Time Information for Production Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'CycleTimeProduct',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiCycleTimeStep
	SQLString := N'SELECT
		ProductName,
		ProductRevision,
		WorkflowStepName,
		Sequence,
		COUNT(DISTINCT ContainerName) AS ContainerCnt,
		SUM(Qty) AS Qty,
		SUM(CycleTime) AS Cycletime,
		SUM(CycleTime) / COUNT(DISTINCT ContainerName) AS Cycletime_Step
	FROM
		csiTbl_ProductionDashboard
	GROUP BY
		ProductName,
		ProductRevision,
		Sequence,
		WorkflowStepName'; 
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiCycleTimeStep',
		N'Gets Step Cycle Time Information for Production Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'CycleTimeStep',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiEventTrend
	SQLString := N'SELECT 
		CASE 
			WHEN Event.CloseDate IS NULL THEN ''Open''
			ELSE ''Closed''
		END AS MyStatus, 
		Event.ReportedDate AS EventDate, 
		Event.ReportedDate, 
		Event.CloseDate, 
		Event.EventId, 
		Event.EventName, 
		FailureMode.FailureModeName, 
		NCRCauseCode.NCRCauseCodeName, 
		FailureSeverity.FailureSeverityName
	FROM 
		Event
	LEFT OUTER JOIN 
		EventFailure ON Event.EventDataId = EventFailure.EventDataId
	LEFT OUTER JOIN 
		FailureMode ON EventFailure.FailureModeId = FailureMode.FailureModeId
	LEFT OUTER JOIN 
		FailureSeverity ON EventFailure.FailureSeverityId = FailureSeverity.FailureSeverityId
	LEFT OUTER JOIN 
		EventFailureCause ON EventFailure.EventFailureId = EventFailureCause.EventFailureId
	LEFT OUTER JOIN 
		NCRCauseCode ON EventFailureCause.CauseCodeId = NCRCauseCode.NCRCauseCodeId
	LEFT OUTER JOIN 
		FailureSeverity FailureSeverity_Default ON FailureMode.DefaultSeverityId = FailureSeverity_Default.FailureSeverityId
	WHERE 
		DATE(Event.ReportedDate) >= CURRENT_DATE - INTERVAL ''90 days''';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiEventTrend',
		N'Used for the Event Trend report.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'EventTrend',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiNonconformance
	SQLString := N'SELECT 
		Event.EventID AS InstanceID, 
		Event.EventName AS Name, 
		Event.BriefDescription, 
		Event.Description, 
		Event.ReportedDateGMT AS ReportedDate, 
		EventLot.Lot, 
		EventLot.Qty, 
		(
			SELECT CategoryField.FieldName 
			FROM CDOFields CategoryField 
			WHERE Event.Category = CategoryField.DefaultValue::integer 
			AND CategoryField.CDODefId = 7520
		) AS CategoryName, 
		Event.Category,  
		Classification.ClassificationName AS Classification, 
		SubClassification.SubClassificationName AS SubClassification,  
		Event.DiscoveryArea, 
		Event.OccurrenceDateGMT AS OccurrenceDate,  
		Organization.OrganizationName AS Organization,  
		Owner.EmployeeName AS Owner, 
		(
			SELECT PriorityLevel.PriorityLevelName 
			FROM PriorityLevel 
			WHERE Event.PriorityLevelId = PriorityLevel.PriorityLevelId
		) AS PriorityLevel, 
		Reporter.EmployeeName AS Reporter, 
		ReporterOrganization.OrganizationName AS ReporterOrganization, 
		Initiator.EmployeeName AS Initiator, 
		InitiatorOrganization.OrganizationName AS InitiatorOrganization, 
		(
			SELECT Role.RoleName 
			FROM RoleDef Role 
			WHERE Event.RoleId = Role.RoleId
		) AS Role, 
		(
			SELECT StatusField.FieldName 
			FROM CDOFields StatusField 
			WHERE Event.Status = StatusField.DefaultValue::integer 
			AND StatusField.CDODefId = 7658
		) AS StatusName, 
		Event.Status, 
		(
			SELECT QualityResolutionCode.QualityResolutionCodeName 
			FROM QualityResolutionCode 
			WHERE Event.QualityResolutionCodeId = QualityResolutionCode.QualityResolutionCodeId
		) AS ResolutionCode, 
		Event.CloseDescription, 
		Event.CloseDateGMT AS CloseDate, 
		ClosedBy.EmployeeName AS ClosedBy,  
		COALESCE(EventData.ProductName, EventLot.ProductName) AS Product, 
		COALESCE(EventData.ProductRev, EventLot.ProductRev) AS ProductRev, 
		EventData.MaintenanceReqName, 
		COALESCE(EventLot.OperationName, EventData.OperationName) AS OperationName, 
		EventData.ResourceName, 
		FailureMode.FailureModeName 
	FROM  
		event  
	LEFT OUTER JOIN  
		EventData ON Event.EventId = EventData.EventId  
	LEFT OUTER JOIN  
		EventFailure ON eventdata.EventDataId = EventFailure.EventDataId 
	LEFT OUTER JOIN 
		FailureMode ON EventFailure.FailureModeId = FailureMode.FailureModeId  
	INNER JOIN  
		Classification ON Event.ClassificationId = Classification.ClassificationId  
	INNER JOIN  
		SubClassification ON Event.SubClassificationId = SubClassification.SubClassificationId   
	LEFT OUTER JOIN  
		Employee ClosedBy ON Event.ClosedById = ClosedBy.EmployeeId  
	LEFT OUTER JOIN  
		Organization ON Event.OrganizationId = Organization.OrganizationId  
	LEFT OUTER JOIN  
		Employee Owner ON Event.OwnerId = Owner.EmployeeId  
	INNER JOIN  
		Employee Reporter ON Event.ReporterId = Reporter.EmployeeId 
	INNER JOIN  
		Organization ReporterOrganization ON Event.ReporterOrganizationId = ReporterOrganization.OrganizationId 
	INNER JOIN  
		Employee Initiator ON Event.InitiatorId = Initiator.EmployeeId 
	INNER JOIN  
		Organization InitiatorOrganization ON Event.InitiatorOrganizationId = InitiatorOrganization.OrganizationId 
	LEFT OUTER JOIN  
		EventLot ON Eventdata.EventDataId = EventLot.EventDataId  
	WHERE 
		Event.Status IN (1,3,4) 
		AND Event.Category = 3  
		AND DATE(Event.ReportedDate) >= CURRENT_DATE - INTERVAL ''90 days''';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiNonconformance',
		N'Gets Nonconformance information for the Process Health Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'ProcessHealthNonconformance',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiProductFamily
	SQLString := N'Select  ProductName, ProductRevision, ProductFamilyname from  Product Inner Join ProductBase on Product.ProductBaseid = ProductBase.ProductBaseID Inner Join ProductFamily on Product.ProductFamilyId= ProductFamily.ProductFamilyId';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiProductFamily',
		N'Gets the Product, Product Family Information',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'ProductFamily',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiProductionDashboardYield
	SQLString := N'SELECT
		ProductName,
		ProductRevision,
		WorkflowStepName,
		Sequence,
		Yield,
		Yield AS Yield_Workflow,
		Move,
		Loss,
		(
			SELECT EXP(SUM(LN(Yield)))
			FROM csiView_FirstPassYield FPY2
			WHERE FPY2.ProductName = FPY.ProductName
			AND FPY2.ProductRevision = FPY.ProductRevision
			AND Yield <> 0
		) AS Yield_Product
	FROM csiView_FirstPassYield FPY';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiProductionDashboardYield',
		N'Used for the Yield report on the Production Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'csiProductionDashboardYield',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiThroughputProduct
	SQLString := N'SELECT
		SUM(CycleTime) / COUNT(DISTINCT ContainerName) AS Cycletime_Product,
		SUM(CycleTime) AS Cycletime,
		MIN(Qty) AS Qty,
		CASE
			WHEN SUM(CycleTime) <> 0
			THEN MIN(Qty) / SUM(CycleTime)
			ELSE 0
		END AS Throughput_Product,
		ProductName,
		ContainerName
	FROM
		csiTbl_ProductionDashboard
	GROUP BY
		ProductName,
		ContainerName';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiThroughputProduct',
		N'Gets Product Throughput Information for Production Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'ThroughputProduct',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
	
	-- csiThroughputStep
	SQLString := N'SELECT
		ProductName,
		ProductRevision,
		Sequence,
		WorkflowStepName,
		ContainerName,
		MIN(Qty) AS Qty,
		SUM(Cycletime) AS Cycletime,
		CASE
			WHEN SUM(Cycletime) <> 0
			THEN MIN(Qty) / SUM(Cycletime)
			ELSE 0
		END AS Throughput_Step
	FROM
		csiTbl_ProductionDashboard
	GROUP BY
		ProductName,
		ProductRevision,
		Sequence,
		WorkflowStepName,
		ContainerName';
	
	CALL csiSTInstall_CreateNewDefinition(
		N'csiThroughputStep',
		N'Gets Step Throughput Information for Production Dashboard.',
		NULL::VARCHAR(512),
		SQLString,
		1,
		'ThroughputStep',
		0,
		NULL::VARCHAR(255),
		NULL::VARCHAR(255),
		'0,',
		NULL::VARCHAR(255),
		0,
		1
	);
END;
$$;

CALL csiSTInstall_PopulateDefaultData();
DROP PROCEDURE csiSTInstall_PopulateDefaultData;
DROP PROCEDURE csiSTInstall_CreateNewDefinition;

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_CreateNewUserQuery')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_CreateNewUserQuery;
 	END IF;
END $$;
CREATE PROCEDURE csiSTInstall_CreateNewUserQuery(
	Name VARCHAR(255),
	Description VARCHAR(512),
	Notes VARCHAR(512),
	QueryText TEXT,
	i_IsFrozen INTEGER,
	Param1Name VARCHAR(50),
	Param1Type INTEGER,
	Param2Name VARCHAR(50),
	Param2Type INTEGER,
	Param3Name VARCHAR(50),
	Param3Type INTEGER
)
LANGUAGE PLPGSQL
AS $$
DECLARE
    v_InstanceID VARCHAR(16);
    v_CurrInstanceID CHAR(16);
    v_CDODefId INTEGER;
    v_ParamCDODefId INTEGER;
    v_ParamFieldId INTEGER;
    v_QueryTypeId INTEGER;
    v_IID VARCHAR(16);
BEGIN
    v_CDODefId := 7068;
    v_ParamCDODefId := 7071;
    v_ParamFieldId := 8597;
    v_QueryTypeId := 4;
	
	-- Remove the existing UserQuery and its Parameters, if it exists
    IF EXISTS (SELECT UserQueryId
               FROM UserQuery
               WHERE UserQueryName = Name) THEN
        SELECT UserQueryId INTO v_CurrInstanceID
        FROM UserQuery
        WHERE UserQueryName = Name;

        DELETE FROM UserQueryParameter WHERE UserQueryId = v_CurrInstanceID;
        DELETE FROM UserQueryUserQueryParameters WHERE UserQueryId = v_CurrInstanceID;
        DELETE FROM UserQuery WHERE UserQueryName = Name;
    END IF;

    CALL csiPRDGetNextInstanceId(v_CDODefId, v_InstanceID);
    INSERT INTO UserQuery(UserQueryId, UserQueryName, Description, Notes, QueryText, IsFrozen, CDOTypeId, QueryTypeId)
    VALUES (v_InstanceID, Name, Description, Notes, QueryText, i_IsFrozen, v_CDODefId, v_QueryTypeId);

    IF Param1Name IS NOT NULL THEN
        CALL csiPRDGetNextInstanceId(v_ParamCDODefId, v_IID);
        INSERT INTO UserQueryParameter (UserQueryParameterId, CDOTypeId, UserQueryParameterName, UserQueryId, IsFrozen, DataType, ChangeCount)
        VALUES (v_IID, v_ParamCDODefId, Param1Name, v_InstanceID, i_IsFrozen, Param1Type, 0);

        INSERT INTO UserQueryUserQueryParameters (FieldId, Sequence, UserQueryId, UserQueryParametersId)
        VALUES (v_ParamFieldId, 1, v_InstanceID, v_IID);
    END IF;

    IF Param2Name IS NOT NULL THEN
        CALL csiPRDGetNextInstanceId(v_ParamCDODefId, v_IID);
        INSERT INTO UserQueryParameter (UserQueryParameterId, CDOTypeId, UserQueryParameterName, UserQueryId, IsFrozen, DataType, ChangeCount)
        VALUES (v_IID, v_ParamCDODefId, Param2Name, v_InstanceID, i_IsFrozen, Param2Type, 0);

        INSERT INTO UserQueryUserQueryParameters (FieldId, Sequence, UserQueryId, UserQueryParametersId)
        VALUES (v_ParamFieldId, 2, v_InstanceID, v_IID);
    END IF;

    IF Param3Name IS NOT NULL THEN
        CALL csiPRDGetNextInstanceId(v_ParamCDODefId, v_IID);
        INSERT INTO UserQueryParameter (UserQueryParameterId, CDOTypeId, UserQueryParameterName, UserQueryId, IsFrozen, DataType, ChangeCount)
        VALUES (v_IID, v_ParamCDODefId, Param3Name, v_InstanceID, i_IsFrozen, Param3Type, 0);

        INSERT INTO UserQueryUserQueryParameters (FieldId, Sequence, UserQueryId, UserQueryParametersId)
        VALUES (v_ParamFieldId, 3, v_InstanceID, v_IID);
    END IF;
END;
$$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_PopulateDefaultUserQueryData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_PopulateDefaultUserQueryData;
 	END IF;
END $$;
CREATE OR REPLACE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData()
LANGUAGE PLPGSQL
AS $$
DECLARE
	SQLString TEXT;
BEGIN
	SQLString := N'Select wfs2.WorkflowStepName as FromStep ' ||
                 ',sum(movehistory.CycleTime)*24*60 as ProcessTime ' ||
                 ',sum(mih.CycleTime)*24*60 as waittime ' ||
                 ',(sum(mih.CycleTime)*24*60 + sum(movehistory.CycleTime)*24*60)/COUNT(c.ContainerId) as CycleTime ' ||
                 ',COUNT(c.containername)as ContainerCount ' ||
                 'from MoveHistory ' ||
                 'Inner Join Container c on MoveHistory.HistoryId = c.containerID ' ||
                 'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId ' ||
                 'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId ' ||
                 'Inner join MoveInHistory mih on c.ContainerId = mih.HistoryId ' ||
                 'Inner join HistoryMainline hml2 on mih.HistoryMainlineId = hml2.HistoryMainlineId ' ||
                 'Where date_trunc(''day'', hml.txnDate) = date_trunc(''day'', clock_timestamp()) ' ||
                 'and hml2.containerId = c.containerId and hml2.WorkflowStepId=wfs2.WorkflowStepId ' ||
                 'and wfs2.WorkflowStepName = ?WorkflowStepName ' ||
                 'Group By wfs2.WorkflowStepName';
				 
	CALL csiSTInstall_CreateNewUserQuery(
		'CurrentCycletimeMetric',
		'CycleTime by step in hours', 
		'OOB CycleTime query for the OOB PI in the Camstar Portal.',
		SQLString,
		0,
		'WorkflowStepName',
		4,
		NULL,
		NULL,
		NULL,
		NULL
	);
				 
	SQLString := N'Select wfs2.WorkflowStepName as FromStep ' ||
                 ',Sum(movehistory.qty) as goodQty ' ||
                 ',Sum(MoveHistory.Qty)+Sum(ABS(coalesce(qhd.Qty,0)*coalesce(qhd.QtyMultiplier,0))) as TotalQtyscrappedPlusGood ' ||
                 'from MoveHistory ' ||
                 'Inner Join Container c on MoveHistory.HistoryId = c.containerID ' ||
                 'Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId ' ||
                 'Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId ' ||
                 'Left  join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId ' ||
                 'Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId ' ||
                 'Where date_trunc(''day'', hml.txnDate) = date_trunc(''day'', current_date) ' ||
                 'and wfs2.WorkflowStepName = ?WorkflowStepName ' ||
                 'Group By wfs2.workflowstepname ';
	
	CALL csiSTInstall_CreateNewUserQuery(
		'CurrentThroughputMetric',
		'Current Throughput by workflowstep', 
		'OOB Throughput query for the OOB PI in the Camstar Portal.',
		SQLString,
		0,
		'WorkflowStepName',
		4,
		NULL,
		NULL,
		NULL,
		NULL
	);
	
	SQLString := N'Select (Sum(movehistory.qty)/(Sum(MoveHistory.Qty)+Sum(ABS(coalesce(qhd.Qty,0)*coalesce(qhd.QtyMultiplier,0)))))*100 as yield from MoveHistory Inner Join Container c on MoveHistory.HistoryId = c.containerID 
		Inner join HistoryMainline hml on MoveHistory.HistoryMainlineId = hml.HistoryMainlineId 
		Inner join WorkflowStep wfs2 on MoveHistory.StepId = wfs2.WorkflowStepId 
		Left join StartHistoryDetail shd on wfs2.WorkflowStepId = shd.WorkflowStepId and shd.HistoryId = MoveHistory.HistoryId 
		Left join QtyHistoryDetails qhd on wfs2.WorkflowStepId = qhd.ChargeToStepId and qhd.HistoryId =MoveHistory.HistoryId 
		Where date_trunc(''day'', hml.txnDate) = date_trunc(''day'', current_date) 
		and wfs2.WorkflowStepName = ?WorkflowStepName 
		Group By wfs2.workflowstepname ';
	
	CALL csiSTInstall_CreateNewUserQuery(
		'CurrentYieldMetric',
		'Current Yield by workflowstep', 
		'OOB Yield query for the OOB PI in the Camstar Portal.',
		SQLString,
		0,
		'WorkflowStepName',
		4,
		NULL,
		NULL,
		NULL,
		NULL
	);
	
END;
$$;

CALL csiSTInstall_PopulateDefaultUserQueryData();
DROP PROCEDURE csiSTInstall_CreateNewUserQuery;
DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData;