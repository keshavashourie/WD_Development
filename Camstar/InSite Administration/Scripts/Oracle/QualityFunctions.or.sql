/* ==========================================================================
--  Quality Functions
--  Copyright Siemens 2025
-- ========================================================================== */

BEGIN
 DROP_DATABASE_OBJECT ( 'OTAB_DATEPLACEHOLDERS', 'TYPE' );

END;
/
CREATE OR REPLACE VIEW csiQualityObjectView AS
SELECT 'CAPA' ObjectType
       ,CAPA.CAPAId InstanceID
       ,CAPA.CAPAName ObjectName
       ,Organization.OrganizationName
       ,Organization.OrganizationId
       ,RoleDef.RoleName
       ,Employee.EmployeeName OwnerName
       ,CAPA.ReportedDate
       ,CAPA.ReportedDateGMT
       ,EventCrossRef.DiscoveryArea
FROM CAPA
LEFT OUTER JOIN (SELECT  TrackingId MyRecordId
                   ,CrossReferenceId Reference
            FROM QualityCrossReference
            UNION
            SELECT  CrossReferenceId MyRecordId
                   ,TrackingId Reference
            FROM QualityCrossReference) CrossRef ON CAPA.CAPAId = CrossRef.MyRecordId
LEFT OUTER JOIN CAPA CAPACrossRef on CrossRef.Reference = CAPACrossRef.CAPAId
LEFT OUTER JOIN Event EventCrossRef on CrossRef.Reference = EventCrossRef.EventId
LEFT OUTER JOIN Organization ON Organization.OrganizationID=CAPA.OrganizationID
LEFT OUTER JOIN RoleDef ON RoleDef.RoleID = CAPA.RoleID
LEFT OUTER JOIN Employee ON Employee.EmployeeID = CAPA.OwnerID
UNION
SELECT 'Event' ObjectType
       ,EventId InstanceID
       ,EventName ObjectName
       ,Organization.OrganizationName
       ,Organization.OrganizationId
       ,RoleDef.RoleName
       ,Employee.EmployeeName OwnerName
       ,Event.ReportedDate
       ,Event.ReportedDateGMT
       ,Event.DiscoveryArea
FROM Event
LEFT OUTER JOIN Organization ON Organization.OrganizationID=Event.OrganizationID
LEFT OUTER JOIN RoleDef ON RoleDef.RoleID = Event.RoleID
LEFT OUTER JOIN Employee ON Employee.EmployeeID = Event.OwnerID
/
CREATE OR REPLACE VIEW csiPlanOrActivityView AS
SELECT 'Plan' Type
       ,ActivityPlan.CDOTypeId
       ,ActivityPlan.PlanId Id
       ,ActivityPlan.PlanId SortId
       ,'1' SortLevel
       ,ActivityPlan.ParentId ProcessModelId
       ,ActivityPlan.PlanName Name
       ,ActivityPlan.Stage
       ,ActivityPlan.AssigneeId
       ,ActivityPlan.CompleteByGMT
       ,ActivityPlan.LastCompletedById
       ,ActivityPlan.LastCompletedOnGMT
       ,ActivityPlan.AutoStart
       ,ActivityPlan.AutoComplete
       ,ActivityPlan.IsRequired
       ,ActivityPlan.DocumentSetId
       ,ActivityPlan.AssigneeRoleId
       ,ActivityPlan.AllowReassignment
       ,ActivityPlan.ReassignmentComments
       ,ActivityPlan.AttachmentsId
       ,ActivityPlan.FirstRoutedOnGMT
       ,ActivityPlan.Description Instruction
       ,ActivityPlan.Notes
       ,ActivityPlan.CompleteWithinQty
       ,ProcessModel.AssigneeId OwnerId
       ,ProcessModel.AssigneeRoleId OwnerRoleId
       ,ActivityPlan.LastDesignatedOnGMT AssignedOnGMT
FROM ActivityPlan
  JOIN ProcessModel ON ProcessModel.ProcessModelId=ActivityPlan.ParentId
UNION
SELECT 'ProcessModelActivity' Type
       ,Activity.CDOTypeId
       ,Activity.ActivityId Id
       ,Activity.ActivityId SortId
       ,'1' SortLevel
       ,Activity.ParentId ProcessModelId
       ,Activity.ActivityName Name
       ,Activity.Stage
       ,Activity.AssigneeId
       ,Activity.CompleteByGMT
       ,Activity.LastCompletedById
       ,Activity.LastCompletedOnGMT
       ,Activity.AutoStart
       ,Activity.AutoComplete
       ,Activity.IsRequired
       ,Activity.DocumentSetId
       ,Activity.AssigneeRoleId
       ,Activity.AllowReassignment
       ,Activity.ReassignmentComments
       ,Activity.AttachmentsId
       ,Activity.FirstRoutedOnGMT
       ,Activity.Description Instruction
       ,Activity.Comments
       ,Activity.CompleteWithinQty
       ,ProcessModel.AssigneeId OwnerId
       ,ProcessModel.AssigneeRoleId OwnerRoleId
       ,Activity.LastDesignatedOnGMT AssignedOnGMT
FROM Activity
    JOIN ProcessModel ON ProcessModel.ProcessModelId=Activity.ParentId
UNION
SELECT 'PlanActivity' Type
       ,Activity.CDOTypeId
       ,Activity.ActivityId Id
       ,ActivityPlan.PlanId SortId
       ,'2' SortLevel
       ,ActivityPlan.ParentId ProcessModelId
       ,Activity.ActivityName Name
       ,Activity.Stage
       ,Activity.AssigneeId
       ,Activity.CompleteByGMT
       ,Activity.LastCompletedById
       ,Activity.LastCompletedOnGMT
       ,Activity.AutoStart
       ,Activity.AutoComplete
       ,Activity.IsRequired
       ,Activity.DocumentSetId
       ,Activity.AssigneeRoleId
       ,Activity.AllowReassignment
       ,Activity.ReassignmentComments
       ,Activity.AttachmentsId
       ,Activity.FirstRoutedOnGMT
       ,Activity.Description Instruction
       ,Activity.Comments
       ,Activity.CompleteWithinQty
       ,ActivityPlan.AssigneeId OwnerId
       ,ActivityPlan.AssigneeRoleId OwnerRoleId
       ,Activity.LastDesignatedOnGMT AssignedOnGMT
FROM Activity
   JOIN ActivityPlan ON ActivityPlan.PlanId = Activity.ParentId
/
CREATE OR REPLACE VIEW csiApprovalCycleView AS
SELECT PlanOrActivity.Id
       ,PlanOrActivity.Name
       ,'Current' ApprovalCycle
       ,StatusField.FieldName Status
       ,ApprovalSheet.GeneralInstructions
       ,ApprovalSheetEntry.SheetLevel
       ,Role.RoleName
       ,RoleEmployee.EmployeeName
       ,DecisionTypeField.FieldName DecisionType
       ,ApprovalSheetEntry.LastRoutedOnGMT
       ,ApprovalSheetEntry.CompleteByGMT
       ,ApprovalSheetEntry.SpecialInstructions
       ,ApprovalSheetEntry.Comments
       ,ApprovalSheetEntry.LastCompletedOnGMT
       ,ApprovedBy.EmployeeName ApprovedBy
FROM csiPlanOrActivityView PlanOrActivity
LEFT OUTER JOIN ApprovalSheet ON PlanOrActivity.Id = ApprovalSheet.ParentId
LEFT OUTER JOIN ApprovalSheetEntry ON ApprovalSheet.ApprovalSheetId = ApprovalSheetEntry.ParentId
LEFT OUTER JOIN Employee ApprovedBy ON ApprovalSheetEntry.ApprovedById = ApprovedBy.EmployeeId
LEFT OUTER JOIN Employee RoleEmployee ON ApprovalSheetEntry.ApproverId = RoleEmployee.EmployeeId
LEFT OUTER JOIN RoleDef Role ON ApprovalSheetEntry.ApproverRoleId = Role.RoleId
LEFT JOIN CDOFields StatusField ON ApprovalSheet.Status=StatusField.DefaultValue AND StatusField.CDODefId=7809
LEFT JOIN CDOFields DecisionTypeField ON ApprovalSheetEntry.DecisionType=DecisionTypeField.DefaultValue AND DecisionTypeField.CDODefId=7810
UNION
SELECT *
FROM (
SELECT  PlanOrActivity.Id
       ,PlanOrActivity.Name
       ,LPAD(CAST(ROWNUM AS VARCHAR2(16)),3,'0') ApprovalCycle
       ,StatusField.FieldName Status
       ,ApprovalSheetHistory.GeneralInstructions
       ,SignApprovalHistory.SheetLevel
       ,Role.RoleName
       ,RoleEmployee.EmployeeName
       ,DecisionTypeField.FieldName DecisionType
       ,SignApprovalHistory.LastRoutedOnGMT
       ,SignApprovalHistory.CompleteByGMT
       ,SignApprovalHistory.SpecialInstructions
       ,SignApprovalHistory.Comments
       ,SignApprovalHistory.LastCompletedOnGMT
	   ,ApprovedBy.EmployeeName ApprovedBy
FROM csiPlanOrActivityView PlanOrActivity
JOIN ProcessModel ON PlanOrActivity.ProcessModelId = ProcessModel.ProcessModelId
JOIN csiQualityObjectView QualityObj ON ProcessModel.ParentId=QualityObj.InstanceId
INNER JOIN HistoryMainLine ON QualityObj.InstanceId = HistoryMainLine.HistoryID
INNER JOIN ApprovalSheet ON PlanOrActivity.Id = ApprovalSheet.ParentId
INNER JOIN ApprovalSheetHistory On (HistoryMainLine.HistoryMainlineId = ApprovalSheetHistory.HistoryMainLineId
				And ApprovalSheetHistory.Status='10'
				And ApprovalSheet.ApprovalSheetId = ApprovalSheetHistory.ApprovalSheetId)
LEFT OUTER JOIN SignApprovalHistory ON ApprovalSheetHistory.ApprovalCycleGMT = SignApprovalHistory.ApprovalCycleGMT
				AND ApprovalSheetHistory.ApprovalSheetId = SignApprovalHistory.ApprovalSheetId
LEFT OUTER JOIN Employee ApprovedBy ON SignApprovalHistory.ApprovedById = ApprovedBy.EmployeeId
LEFT OUTER JOIN Employee RoleEmployee ON SignApprovalHistory.ApproverId = RoleEmployee.EmployeeId
LEFT OUTER JOIN RoleDef Role ON SignApprovalHistory.ApproverRoleId = Role.RoleId
LEFT JOIN CDOFields StatusField ON ApprovalSheet.Status=StatusField.DefaultValue AND StatusField.CDODefId=7809
LEFT JOIN CDOFields DecisionTypeField ON SignApprovalHistory.DecisionType=DecisionTypeField.DefaultValue AND DecisionTypeField.CDODefId=7810
ORDER BY SignApprovalHistory.LastCompletedOnGMT ASC
)
/
CREATE OR REPLACE VIEW csiEventView AS
SELECT
	 Event.EventID InstanceID
	,Event.EventName Name
	,Event.BriefDescription
	,Event.Description
	,Event.ReportedDateGMT ReportedDate
	,CategoryField.FieldName CategoryName
	,Classification.ClassificationName Classification
	,SubClassification.SubClassificationName SubClassification
	,Event.DiscoveryArea
	,Event.OccurrenceDateGMT OccurrenceDate
	,Organization.OrganizationName Organization
	,Owner.EmployeeName Owner
	,PriorityLevel.PriorityLevelName PriorityLevel
	,Reporter.EmployeeName Reporter
	,ReporterOrganization.OrganizationName ReporterOrganization
	,Initiator.EmployeeName Initiator
	,InitiatorOrganization.OrganizationName InitiatorOrganization
	,Role.RoleName Role
	,StatusField.FieldName StatusName
	,QualityResolutionCode.QualityResolutionCodeName ResolutionCode
	,Event.CloseDescription CloseDescription
	,Event.CloseDateGMT CloseDate
	,ClosedBy.EmployeeName ClosedBy
	,EventData.ProductName Product
	,EventData.ProductRev ProductRev 
	,EventData.MaintenanceReqName MaintenanceReqName
	,EventData.OperationName OperationName
	,EventData.ResourceName ResourceName
FROM Event
LEFT OUTER JOIN EventData ON Event.EventId=EventData.EventId
LEFT JOIN CDOFields CategoryField ON Event.Category=CategoryField.DefaultValue AND CategoryField.CDODefId=7520
LEFT JOIN Classification on Event.ClassificationId = Classification.ClassificationId
LEFT JOIN SubClassification on Event.SubClassificationId = SubClassification.SubClassificationId
LEFT JOIN CDOFields StatusField ON Event.Status=StatusField.DefaultValue AND StatusField.CDODefId=7658
LEFT OUTER JOIN Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId
LEFT OUTER JOIN Organization ON Event.OrganizationId=Organization.OrganizationId
LEFT OUTER JOIN Employee Owner ON Event.OwnerId=Owner.EmployeeId
LEFT JOIN PriorityLevel ON Event.PriorityLevelId=PriorityLevel.PriorityLevelId
LEFT OUTER JOIN QualityResolutionCode ON Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId
LEFT OUTER JOIN Employee Reporter ON Event.ReporterId=Reporter.EmployeeId
LEFT OUTER JOIN Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId
LEFT OUTER JOIN Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId
LEFT OUTER JOIN Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId
LEFT OUTER JOIN RoleDef Role ON Event.RoleId=Role.RoleId
/
CREATE OR REPLACE 
TYPE otyp_DatePlaceholder AS OBJECT (ID NUMBER, StartDate DATE, EndDate DATE)
/
CREATE OR REPLACE 
TYPE otab_DatePlaceholders AS TABLE OF otyp_DatePlaceholder
/
CREATE OR REPLACE FUNCTION csiGetDatePlaceholders (pStartDate DATE, pEndDate DATE, pInterval VARCHAR2 )
RETURN otab_DatePlaceholders
PIPELINED
AS
--
-- Copyright Siemens 2023  
--
   vLabel         NUMBER;
   vCurrStartDate DATE;
   vCurrEndDate   DATE;
   vTargetEndDate DATE;
   vIntervalLower VARCHAR2(50) := LOWER(pInterval);
   vOneSecond     NUMBER := 1/86400; -- To save time later

   vHour          VARCHAR2(2);
   vDay           VARCHAR2(2);
   vMonth         VARCHAR2(2);
   vYear          VARCHAR2(4);
BEGIN
   vLabel := 1;

   -- Attempt to find the beginning of the first interval
   vHour := TO_CHAR(pStartDate,'HH24');
   vDay  := TO_CHAR(pStartDate,'DD');
   vMonth:= TO_CHAR(pStartDate,'MM');
   vYear := TO_CHAR(pStartDate,'YYYY');

   vCurrStartDate :=
      CASE
         WHEN vIntervalLower='hourly' THEN TO_DATE(vYear||'/'||vMonth||'/'||vDay||' 00:00:00','YYYY/MM/DD HH24:MI:SS') -- Always start at midnight
         WHEN vIntervalLower='daily' THEN TO_DATE(vYear||'/'||vMonth||'/'||vDay||' 00:00:00','YYYY/MM/DD HH24:MI:SS')
         WHEN vIntervalLower='weekly' THEN TRUNC(NEXT_DAY(pStartDate-7, 'MON'))
         WHEN vIntervalLower='monthly' THEN TO_DATE(vYear||'/'||vMonth||'/01 00:00:00','YYYY/MM/DD HH24:MI:SS')
         WHEN vIntervalLower='yearly' THEN TO_DATE(vYear||'/01/01 00:00:00','YYYY/MM/DD HH24:MI:SS')
      END;

   -- For Hourly, we want to go to the end of the day. All others (for now) just use EndDate
   vTargetEndDate := 
      CASE
         WHEN vIntervalLower='hourly' THEN TRUNC(pEndDate)+1
         WHEN vIntervalLower='daily' THEN TRUNC(pEndDate)+1
         WHEN vIntervalLower='weekly' THEN pEndDate
         WHEN vIntervalLower='monthly' THEN pEndDate
         WHEN vIntervalLower='yearly' THEN pEndDate
      END;
      
   -- Oracle date math is day-based, so use the appropriate fractions, multipliers or functions
   -- based on interval:
   -- Hour  = +1/24
   -- Day   = +1
   -- Week  = +7
   -- Month = +NumToYMInterval(1, 'MONTH')  (months have varying # of days)
   -- Year  =  +NumToYMInterval(1, 'YEAR')  (years have varying # of days)
   WHILE (vCurrStartDate<=vTargetEndDate) LOOP
      vCurrEndDate :=
      CASE
         WHEN vIntervalLower='hourly' THEN vCurrStartDate+1/24
         WHEN vIntervalLower='daily' THEN vCurrStartDate+1
         WHEN vIntervalLower='weekly' THEN vCurrStartDate+7
         WHEN vIntervalLower='monthly' THEN vCurrStartDate+NumToYMInterval(1, 'MONTH')
         WHEN vIntervalLower='yearly' THEN vCurrStartDate+NumToYMInterval(1, 'YEAR')
      END;

      -- The end date for this record needs to be just less than the next start date, so back off 1 second
      PIPE ROW(otyp_DatePlaceholder(vLabel, vCurrStartDate, vCurrEndDate-vOneSecond));
      vCurrStartDate:=vCurrEndDate;
      vLabel:=vLabel+1;
   END LOOP;
   --
   RETURN;
END;
/
CREATE OR REPLACE
FUNCTION csiGetEventLotStatus(pEventID VARCHAR2, pEventLotId VARCHAR2) RETURN VARCHAR2
AS
   C_NONE_ASSIGNED NUMBER := 1;
   C_ASSIGNED      NUMBER := 2;
   C_IN_PROCESS    NUMBER := 3;
   C_COMPLETE      NUMBER := 4;   
   C_VOIDED        NUMBER := 5;
   C_CDODEFID      NUMBER := 8025;
   
   CURSOR c1 IS
      SELECT A.Stage
            ,NVL(EL.Qty,0) QtyAssigned
            ,A.ReconcileQuantity
            ,NVL(DD.QuantityDispositioned,0) QtyDispositioned
      FROM EVENT EV
         JOIN PROCESSMODEL PM ON PM.PARENTID=EV.EVENTID
         JOIN csiPlanOrActivityView PA ON PA.PROCESSMODELID=PM.PROCESSMODELID
         JOIN ACTIVITY A ON A.ACTIVITYID=PA.ID
         JOIN DISPOSITIONDATA DD ON DD.PARENTID=A.ACTIVITYID
         JOIN EVENTLOT EL ON EL.EVENTLOTID=DD.EVENTLOTID
      WHERE EV.EVENTID=pEventID
         AND PA.TYPE IN ('PlanActivity','ProcessModelActivity')
         AND DD.EVENTLOTID=pEventLotId
         AND A.STAGE!=C_VOIDED;
    
   crec c1%ROWTYPE;
   vStatusId NUMBER := 0;
   vMinStage NUMBER := 9999;
   vMaxStage NUMBER := 0;
   vQtyAssigned NUMBER :=0;
   vSumRecQtyDispositioned NUMBER := 0;
   vReturn VARCHAR2(50) := '';
BEGIN
   -- The main cursor should return all DispositionData records of non-voided Activities
   -- for the Event's ProcessModel, matching the EventLot passed into the function.
   OPEN c1;
   FETCH c1 INTO crec;
   IF (c1%NOTFOUND) THEN
-- RULE #1
--    If no DispositionData records are found for the Lot, then the status is 'None Assigned'
      vStatusId:=C_NONE_ASSIGNED;
   ELSE
      -- Loop over the records. To analyze the rules, we need to know the highest and lowest stages
      -- along with the QtyDispositioned for reconciled dispositions. 
      -- The QtyAssigned is at the EventLot level, so is the same for every record, but we need to copy
      -- it to a local varible, vQtyAssigned.
      WHILE (c1%FOUND) LOOP
         vQtyAssigned:=crec.QtyAssigned;
         IF (crec.Stage<vMinStage) THEN
            vMinStage:=crec.Stage;
         END IF;
         IF (crec.Stage>vMaxStage) THEN
            vMaxStage:=crec.Stage;
         END IF;
         IF (crec.ReconcileQuantity=1) THEN
            vSumRecQtyDispositioned:=vSumRecQtyDispositioned+crec.QtyDispositioned;
         END IF;     
         
         FETCH c1 INTO crec;
      END LOOP;
      
-- RULE #2
--    If the parent Activities for all of the DispositionData records have a stage >= 50 (Completed) 
--    and the sum of the QuantityDispositioned for all of the Reconciled DispositionData records is 
--    equal to the Qty assigned to the Lot (Reconciled DispositionData records are those that have a 
--    parent Activity where the ReconcileQuantity flag is set to true), then the status is 'Complete'
      IF (vMinStage>=50 AND vMaxStage>=50 AND vSumRecQtyDispositioned=vQtyAssigned) THEN
         vStatusId:=C_COMPLETE;       
      ELSIF (vMaxStage>20) THEN
-- RULE #3
--    If the parent Activities for any of the DispositionData records have a stage > 20 (Pending), then the status is 'In Process'
         vStatusId:=C_IN_PROCESS;
      ELSE 
-- RULE #4
--    Otherwise the status is 'Assigned'
         vStatusId:=C_ASSIGNED;
      END IF;
   END IF;
   CLOSE c1;
   
   -- Resolve the enumeration using the LotDispositionStatusEnum (CDODefId=8025)
   SELECT FieldName
   INTO vReturn
   FROM CDOFields
   WHERE CDODefId=C_CDODEFID
   AND DefaultValue=vStatusId;
   
   RETURN vReturn;
END;
/
