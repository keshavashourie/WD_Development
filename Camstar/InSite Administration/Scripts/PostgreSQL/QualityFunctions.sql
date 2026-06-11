DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiQualityObjectView') 
	) THEN
		DROP VIEW IF EXISTS csiQualityObjectView;
	END IF;
END $$;
CREATE VIEW csiQualityObjectView AS
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
LEFT OUTER JOIN Employee ON Employee.EmployeeID = Event.OwnerID;


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiPlanOrActivityView') 
	) THEN
		DROP VIEW IF EXISTS csiPlanOrActivityView;
	END IF;
END $$;
CREATE VIEW csiPlanOrActivityView AS
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
     JOIN ActivityPlan ON ActivityPlan.PlanId = Activity.ParentId;


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiApprovalCycleView') 
	) THEN
		DROP VIEW IF EXISTS csiApprovalCycleView;
	END IF;
END $$;
CREATE VIEW csiApprovalCycleView AS
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
JOIN ApprovalSheet ON PlanOrActivity.Id = ApprovalSheet.ParentId
LEFT OUTER JOIN ApprovalSheetEntry ON ApprovalSheet.ApprovalSheetId = ApprovalSheetEntry.ParentId
LEFT OUTER JOIN Employee ApprovedBy ON ApprovalSheetEntry.ApprovedById = ApprovedBy.EmployeeId
LEFT OUTER JOIN Employee RoleEmployee ON ApprovalSheetEntry.ApproverId = RoleEmployee.EmployeeId
LEFT OUTER JOIN RoleDef Role ON ApprovalSheetEntry.ApproverRoleId = Role.RoleId
LEFT JOIN CDOFields StatusField ON ApprovalSheet.Status=StatusField.DefaultValue::integer AND StatusField.CDODefId=7809
LEFT JOIN CDOFields DecisionTypeField ON ApprovalSheetEntry.DecisionType=DecisionTypeField.DefaultValue::integer AND DecisionTypeField.CDODefId=7810
UNION
SELECT  PlanOrActivity.Id
       ,PlanOrActivity.Name  
       ,LPAD(CAST(ROW_NUMBER() OVER (ORDER BY ApprovalSheetHistory.ApprovalCycleGMT ASC) AS VARCHAR), 3, '0') AS ApprovalCycle
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
LEFT JOIN CDOFields StatusField ON ApprovalSheet.Status=StatusField.DefaultValue::integer AND StatusField.CDODefId=7809
LEFT JOIN CDOFields DecisionTypeField ON SignApprovalHistory.DecisionType=DecisionTypeField.DefaultValue::integer AND DecisionTypeField.CDODefId=7810;
    
    
   
DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiEventView') 
	) THEN
		DROP VIEW IF EXISTS csiEventView;
	END IF;
END $$;   
CREATE VIEW csiEventView AS
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
LEFT JOIN CDOFields CategoryField ON Event.Category = CategoryField.DefaultValue::integer AND CategoryField.CDODefId=7520
LEFT JOIN Classification on Event.ClassificationId = Classification.ClassificationId
LEFT JOIN SubClassification on Event.SubClassificationId = SubClassification.SubClassificationId
LEFT JOIN CDOFields StatusField ON Event.Status=StatusField.DefaultValue::integer AND StatusField.CDODefId=7658
LEFT OUTER JOIN Employee ClosedBy ON Event.ClosedById=ClosedBy.EmployeeId
LEFT OUTER JOIN Organization ON Event.OrganizationId=Organization.OrganizationId
LEFT OUTER JOIN Employee Owner ON Event.OwnerId=Owner.EmployeeId
LEFT JOIN PriorityLevel ON Event.PriorityLevelId=PriorityLevel.PriorityLevelId
LEFT OUTER JOIN QualityResolutionCode ON Event.QualityResolutionCodeId=QualityResolutionCode.QualityResolutionCodeId
LEFT OUTER JOIN Employee Reporter ON Event.ReporterId=Reporter.EmployeeId
LEFT OUTER JOIN Organization ReporterOrganization ON Event.ReporterOrganizationId=ReporterOrganization.OrganizationId
LEFT OUTER JOIN Employee Initiator ON Event.InitiatorId=Initiator.EmployeeId
LEFT OUTER JOIN Organization InitiatorOrganization ON Event.InitiatorOrganizationId=InitiatorOrganization.OrganizationId
LEFT OUTER JOIN RoleDef Role ON Event.RoleId=Role.RoleId;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetDatePlaceholders')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetDatePlaceholders;
 	END IF;
END $$;
CREATE FUNCTION csiGetDatePlaceholders(
	i_StartDate timestamp with time zone, i_EndDate timestamp with time zone, i_Interval varchar(20)
)
RETURNS TABLE (
	ID Integer, 
	StartDate timestamp, 
	EndDate timestamp
)
language plpgsql
as $$
DECLARE
   Label integer;
   CurrEndDate timestamp;
   CurrStartDate timestamp;
   TargetEndDate timestamp;
   
   Hour  varchar(2);
   Day   varchar(2);
   Month varchar(2);
   Year  varchar(4);
   v_Trunc timestamp;
   v_temp text;
   
begin
	
	-- Create temporary table
	v_temp := 'tmpresults_' || to_char(clock_timestamp(), 'YYYYMMDDHH24MISSMS');
	execute format('create local TEMPORARY TABLE %I (
		ID Integer, 
		StartDate timestamp, 
		EndDate timestamp
	) on commit drop', v_temp);
	
	Label := 1;
	
	-- Attempt to find the beginning of the first interval
	Hour := EXTRACT(HOUR FROM i_StartDate)::VARCHAR;
	Day := EXTRACT(DAY FROM i_StartDate)::VARCHAR;
	Month := EXTRACT(MONTH FROM i_StartDate)::VARCHAR;
	Year := EXTRACT(YEAR FROM i_StartDate)::VARCHAR;
	v_Trunc := DATE_TRUNC('day', i_StartDate)::date; -- Returns mm/dd/yyyy with no time
	
	CurrStartDate :=
      CASE 
         WHEN i_Interval = 'daily' THEN (Year || '-' || Month || '-' || Day || ' 00:00:00')::timestamp
         WHEN i_Interval = 'hourly' THEN (Year || '-' || Month || '-' || Day || ' 00:00:00')::timestamp -- Always start at midnight
         WHEN i_Interval = 'weekly' THEN (v_Trunc - (EXTRACT(DOW FROM v_Trunc) - 1) * INTERVAL '1 day')::timestamp -- Gets the previous Monday
         WHEN i_Interval = 'monthly' THEN (Year || '-' || Month || '-01 00:00:00')::timestamp
         WHEN i_Interval = 'yearly' THEN (Year || '-01-01 00:00:00')::timestamp
      END;

	-- For Hourly, we want to go to the end of the day. All others (for now) just use EndDate
	TargetEndDate :=
      CASE 
         WHEN i_Interval = 'daily' THEN (DATE_TRUNC('day', i_EndDate) + INTERVAL '1 day')::timestamp
         WHEN i_Interval = 'hourly' THEN (DATE_TRUNC('day', i_EndDate) + INTERVAL '1 day')::timestamp
         WHEN i_Interval = 'weekly' THEN i_EndDate
         WHEN i_Interval = 'monthly' THEN i_EndDate
         WHEN i_Interval = 'yearly' THEN i_EndDate
      END;
	
	WHILE CurrStartDate <= TargetEndDate LOOP  
      CurrEndDate :=
      CASE 
         WHEN i_Interval = 'daily' THEN CurrStartDate + INTERVAL '1 day'
         WHEN i_Interval = 'hourly' THEN CurrStartDate + INTERVAL '1 hour'
         WHEN i_Interval = 'weekly' THEN CurrStartDate + INTERVAL '1 week'
         WHEN i_Interval = 'monthly' THEN CurrStartDate + INTERVAL '1 month'
         WHEN i_Interval = 'yearly' THEN CurrStartDate + INTERVAL '1 year'
      END;

      -- The end date for this record needs to be just less than the next start date, so back off 1 second
      execute format('INSERT INTO %I (ID, StartDate, EndDate) VALUES ($1, $2, $3)', v_temp) using Label, CurrStartDate, CurrEndDate - INTERVAL '1 second';

      CurrStartDate := CurrEndDate;
      Label := Label + 1;
	END LOOP;
	
	RETURN QUERY
	execute format('SELECT * FROM %I', v_temp);
	
END;
$$;



DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetEventLotStatus')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetEventLotStatus;
 	END IF;
END $$;
CREATE FUNCTION csiGetEventLotStatus(
	pEventId varchar(20),
	pEventLotId varchar(20)
)
RETURNS varchar(50)
language plpgsql
as $$
DECLARE
   -- Constants
   C_NONE_ASSIGNED Integer;
   C_ASSIGNED      Integer;
   C_IN_PROCESS    Integer;
   C_COMPLETE      Integer;
   C_VOIDED        Integer;
   C_CDODEFID      Integer;
   
   -- Cursor variables
   c1                     cursor FOR
      SELECT A.Stage
            ,COALESCE(EL.Qty,0) QtyAssigned
            ,A.ReconcileQuantity
            ,COALESCE(DD.QuantityDispositioned,0) QtyDispositioned
      FROM EVENT EV
         JOIN PROCESSMODEL PM ON PM.PARENTID=EV.EVENTID
         JOIN csiPlanOrActivityView PA ON PA.PROCESSMODELID=PM.PROCESSMODELID
         JOIN ACTIVITY A ON A.ACTIVITYID=PA.ID
         JOIN DISPOSITIONDATA DD ON DD.PARENTID=A.ACTIVITYID
         JOIN EVENTLOT EL ON EL.EVENTLOTID=DD.EVENTLOTID
      WHERE EV.EVENTID=pEventId
         AND PA.TYPE IN ('PlanActivity','ProcessModelActivity')
         AND DD.EVENTLOTID=pEventLotId
         AND A.STAGE!=C_VOIDED;
   crec_Stage             Integer;
   crec_QtyAssigned       Integer;
   crec_ReconcileQuantity Integer;
   crec_QtyDispositioned  Integer;
   
   -- Local variables  
   vRowsFound              BOOLEAN; -- true/false
   vStatusId               Integer;
   vMinStage               Integer;
   vMaxStage               Integer;
   vQtyAssigned            Integer;
   vSumRecQtyDispositioned Integer;
   vReturn                 Varchar(50);

BEGIN

   -- Initial values
   C_NONE_ASSIGNED:= 1;
   C_ASSIGNED     := 2;
   C_IN_PROCESS   := 3;
   C_COMPLETE     := 4;
   C_VOIDED       := 5;
   C_CDODEFID     := 8025;
   
   vRowsFound := FALSE;
   vStatusId := 0;
   vMinStage := 9999;
   vMaxStage := 0;
   vQtyAssigned := 0;
   vSumRecQtyDispositioned := 0;
   vReturn := '';
   
   -- The main cursor should return all DispositionData records of non-voided Activities
   -- for the Event's ProcessModel, matching the EventLot passed into the function.
   OPEN c1;
		 
	LOOP
      FETCH c1 INTO crec_Stage, crec_QtyAssigned, crec_ReconcileQuantity, crec_QtyDispositioned;
      EXIT WHEN NOT FOUND;
      
      vRowsFound := TRUE;
      vQtyAssigned := crec_QtyAssigned;
      
      IF crec_Stage < vMinStage THEN
         vMinStage := crec_Stage;
      END IF;
                 
      IF crec_Stage > vMaxStage THEN
         vMaxStage := crec_Stage;
      END IF;
        
      IF crec_ReconcileQuantity = 1 THEN
         vSumRecQtyDispositioned := vSumRecQtyDispositioned + crec_QtyDispositioned;
      END IF;
   END LOOP;
   
   IF NOT vRowsFound THEN
      -- RULE #1
      -- If no DispositionData records are found for the Lot, then the status is ‘None Assigned’
      vStatusId := C_NONE_ASSIGNED;
   ELSE
      -- RULE #2
      -- If the parent Activities for all of the DispositionData records have a stage >= 50 (Completed) 
      -- and the sum of the QuantityDispositioned for all of the Reconciled DispositionData records is 
      -- equal to the Qty assigned to the Lot, then the status is 'Complete’
      IF vMinStage >= 50 AND vMaxStage >= 50 AND vSumRecQtyDispositioned = vQtyAssigned THEN
         vStatusId := C_COMPLETE;
      ELSE 
         IF vMaxStage > 20 THEN
            -- RULE #3
            -- If the parent Activities for any of the DispositionData records have a stage > 20 (Pending), then the status is ‘In Process’ 
            vStatusId := C_IN_PROCESS;
         ELSE 
            -- RULE #4
            -- Otherwise the status is ‘Assigned’
            vStatusId := C_ASSIGNED;
         END IF;
      END IF;
   END IF;

   CLOSE c1;
   
   -- Resolve the enumeration using the LotDispositionStatusEnum (CDODefId=8025)
   SELECT FieldName INTO vReturn   
   FROM CDOFields
   WHERE CDODefId = C_CDODEFID AND DefaultValue = vStatusId::varchar(255);
   
   RETURN vReturn;
END;
$$;