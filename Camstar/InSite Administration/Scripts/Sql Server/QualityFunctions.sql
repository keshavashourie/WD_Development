--------------------------------------------------------------------------------
-- SCRIPT: QualityFunctions.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT NAME FROM sysobjects WHERE NAME = 'csiQualityObjectView' AND TYPE='V')
   DROP VIEW csiQualityObjectView
GO
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
LEFT OUTER JOIN Employee ON Employee.EmployeeID = Event.OwnerID
GO
IF EXISTS (SELECT NAME FROM sysobjects WHERE NAME = 'csiPlanOrActivityView' AND TYPE='V')
   DROP VIEW csiPlanOrActivityView
GO
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
     JOIN ActivityPlan ON ActivityPlan.PlanId = Activity.ParentId
GO
IF EXISTS (SELECT NAME FROM sysobjects WHERE NAME = 'csiApprovalCycleView' AND TYPE='V')
   DROP VIEW csiApprovalCycleView
GO
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
LEFT JOIN CDOFields StatusField ON ApprovalSheet.Status=StatusField.DefaultValue AND StatusField.CDODefId=7809
LEFT JOIN CDOFields DecisionTypeField ON ApprovalSheetEntry.DecisionType=DecisionTypeField.DefaultValue AND DecisionTypeField.CDODefId=7810
UNION
SELECT  PlanOrActivity.Id
       ,PlanOrActivity.Name  
       ,RIGHT( '000' + CAST(ROW_NUMBER () OVER (ORDER BY ApprovalSheetHistory.ApprovalCycleGMT ASC) AS VARCHAR),3) ApprovalCycle
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
GO
IF EXISTS (SELECT NAME FROM sysobjects WHERE NAME = 'csiEventView' AND TYPE='V')
   DROP VIEW csiEventView
GO
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
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetDatePlaceholders' 
	   AND 	  type = 'TF')
    DROP FUNCTION csiGetDatePlaceholders
GO
CREATE FUNCTION csiGetDatePlaceholders (@StartDate DateTime, @EndDate DateTime, @Interval Varchar(20))
RETURNS @retDates TABLE (ID Integer, StartDate DateTime, EndDate DateTime)
AS
--
-- Copyright Siemens 2023  
--
BEGIN
   DECLARE @Label Integer
   DECLARE @CurrEndDate DateTime
   DECLARE @CurrStartDate DateTime
   DECLARE @TargetEndDate DateTime
   
   DECLARE @Hour  Varchar(2)
   DECLARE @Day   Varchar(2)
   DECLARE @Month Varchar(2)
   DECLARE @Year  Varchar(4)
   DECLARE @Trunc DateTime

   SET @Label = 1

   -- Attempt to find the beginning of the first interval
   SET @Hour=DATEPART(hour,@StartDate)
   SET @Day=DATEPART(day,@StartDate)
   SET @Month=DATEPART(month,@StartDate)
   SET @Year=DATEPART(year,@StartDate)
   SET @Trunc=CONVERT(VARCHAR(10), @StartDate, 111)  -- Returns mm/dd/yyyy with no time

   SET @CurrStartDate=
      CASE 
         WHEN @Interval='daily' THEN CONVERT(datetime,@Year+'-'+@Month+'-'+@Day+' 00:00:00')
         WHEN @Interval='hourly' THEN CONVERT(datetime,@Year+'-'+@Month+'-'+@Day+' 00:00:00') -- Always start at midnight
         WHEN @Interval='weekly' THEN DATEADD(DD, 2 - DATEPART(DW, @Trunc),@Trunc) -- Gets the previous Monday
         WHEN @Interval='monthly' THEN CONVERT(datetime,@Year+'-'+@Month+'-01 00:00:00')
         WHEN @Interval='yearly' THEN CONVERT(datetime,@Year+'-01-01 00:00:00')
      END

   -- For Hourly, we want to go to the end of the day. All others (for now) just use EndDate
   SET @TargetEndDate=
      CASE 
         WHEN @Interval='daily' THEN CAST(CONVERT(VARCHAR(10), @EndDate, 111) AS DATETIME)+1
         WHEN @Interval='hourly' THEN CAST(CONVERT(VARCHAR(10), @EndDate, 111) AS DATETIME)+1
         WHEN @Interval='weekly' THEN @EndDate
         WHEN @Interval='monthly' THEN @EndDate
         WHEN @Interval='yearly' THEN @EndDate
      END
      
   WHILE (@CurrStartDate<=@TargetEndDate)
   BEGIN    
      SET @CurrEndDate=
      CASE 
         WHEN @Interval='daily' THEN DateAdd(day, 1, @CurrStartDate)
         WHEN @Interval='hourly' THEN DateAdd(hour, 1, @CurrStartDate)
         WHEN @Interval='weekly' THEN DateAdd(week, 1, @CurrStartDate)
         WHEN @Interval='monthly' THEN DateAdd(month, 1, @CurrStartDate)
         WHEN @Interval='yearly' THEN DateAdd(year, 1, @CurrStartDate)
      END
      -- The end date for this record needs to be just less than the next start date, so back off 1 second
      INSERT INTO @retDates(ID, StartDate, EndDate) VALUES (@Label, @CurrStartDate, DATEADD(second,-1,@CurrEndDate))

      SET @CurrStartDate=@CurrEndDate
      SET @Label=@Label+1
   END

   RETURN
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetEventLotStatus' 
	   AND 	  type = 'FN')
    DROP FUNCTION csiGetEventLotStatus
GO
CREATE FUNCTION csiGetEventLotStatus(@pEventId As NVarchar(20), @pEventLotId As NVarchar(20)) RETURNS NVarchar(50)
BEGIN
--
-- Copyright Siemens 2023  
--
   -- Constants
   DECLARE @C_NONE_ASSIGNED Integer;
   DECLARE @C_ASSIGNED      Integer;
   DECLARE @C_IN_PROCESS    Integer;
   DECLARE @C_COMPLETE      Integer;
   DECLARE @C_VOIDED        Integer;
   DECLARE @C_CDODEFID      Integer;
   
   -- Cursor variables
   DECLARE @c1                     Cursor;
   DECLARE @crec_Stage             Integer;
   DECLARE @crec_QtyAssigned       Integer;
   DECLARE @crec_ReconcileQuantity Integer;
   DECLARE @crec_QtyDispositioned  Integer;
   
   -- Local variables  
   DECLARE @vRowsFound              Bit; -- true/false
   DECLARE @vStatusId               Integer;
   DECLARE @vMinStage               Integer;
   DECLARE @vMaxStage               Integer;
   DECLARE @vQtyAssigned            Integer;
   DECLARE @vSumRecQtyDispositioned Integer;
   DECLARE @vReturn                 NVarchar(50);

   -- Initial values
   SET @C_NONE_ASSIGNED= 1;
   SET @C_ASSIGNED     = 2;
   SET @C_IN_PROCESS   = 3;
   SET @C_COMPLETE     = 4;
   SET @C_VOIDED       = 5;
   SET @C_CDODEFID     = 8025;
   
   SET @vRowsFound = 0;
   SET @vStatusId = 0;
   SET @vMinStage = 9999;
   SET @vMaxStage = 0;
   SET @vQtyAssigned = 0;
   SET @vSumRecQtyDispositioned = 0;
   SET @vReturn = N'';
      
   
   -- The main cursor should return all DispositionData records of non-voided Activities
   -- for the Event's ProcessModel, matching the EventLot passed into the function.
   SET @c1 = CURSOR FOR
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
      WHERE EV.EVENTID=@pEventId
         AND PA.TYPE IN ('PlanActivity','ProcessModelActivity')
         AND DD.EVENTLOTID=@pEventLotId
         AND A.STAGE!=@C_VOIDED;
         
   OPEN @c1
   FETCH NEXT FROM @c1 INTO @crec_Stage, @crec_QtyAssigned, @crec_ReconcileQuantity, @crec_QtyDispositioned

   -- Loop over the records. To analyze the rules, we need to know the highest and lowest stages
   -- along with the QtyDispositioned for reconciled dispositions. 
   -- The QtyAssigned is at the EventLot level, so is the same for every record, but we need to copy
   -- it to a local varible, vQtyAssigned.
   WHILE (@@fetch_status = 0)      
   BEGIN
      SET @vRowsFound = 1;
      SET @vQtyAssigned=@crec_QtyAssigned;
     
	  IF (@crec_Stage<@vMinStage)
	     SET @vMinStage=@crec_Stage;
                 
	  IF (@crec_Stage>@vMaxStage)
	     SET @vMaxStage=@crec_Stage;
        
	  IF (@crec_ReconcileQuantity=1)
	     SET @vSumRecQtyDispositioned=@vSumRecQtyDispositioned+@crec_QtyDispositioned;
     
	  FETCH NEXT FROM @c1 INTO @crec_Stage, @crec_QtyAssigned, @crec_ReconcileQuantity, @crec_QtyDispositioned
   END -- END WHILE
      
   IF (@vRowsFound=0)
-- RULE #1
--    If no DispositionData records are found for the Lot, then the status is ‘None Assigned’
      SET @vStatusId=@C_NONE_ASSIGNED;
   ELSE
   BEGIN
-- RULE #2
--    If the parent Activities for all of the DispositionData records have a stage >= 50 (Completed) 
--    and the sum of the QuantityDispositioned for all of the Reconciled DispositionData records is 
--    equal to the Qty assigned to the Lot (Reconciled DispositionData records are those that have a 
--    parent Activity where the ReconcileQuantity flag is set to true), then the status is 'Complete’
      IF (@vMinStage>=50 AND @vMaxStage>=50 AND @vSumRecQtyDispositioned=@vQtyAssigned)
         SET @vStatusId=@C_COMPLETE;       
      ELSE IF (@vMaxStage>20)
-- RULE #3
--    If the parent Activities for any of the DispositionData records have a stage > 20 (Pending), then the status is ‘In Process’ 
         SET @vStatusId=@C_IN_PROCESS;
      ELSE 
-- RULE #4
--    Otherwise the status is ‘Assigned’
         SET @vStatusId=@C_ASSIGNED;

   END -- END ELSE
   CLOSE @c1
   DEALLOCATE @c1
         
   -- Resolve the enumeration using the LotDispositionStatusEnum (CDODefId=8025)
   SELECT @vReturn=FieldName   
   FROM CDOFields
   WHERE CDODefId=@C_CDODEFID
   AND DefaultValue=@vStatusId;
   
   RETURN @vReturn;
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
