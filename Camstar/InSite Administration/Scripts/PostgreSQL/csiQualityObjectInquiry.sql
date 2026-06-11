DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('QualityObject') 
	) THEN
		DROP VIEW IF EXISTS QualityObject;
	END IF;
END $$;
CREATE VIEW QualityObject AS 
SELECT
    Event.CDOTypeId AS QualityObjectType,
    1 AS Type,
    Event.EventId AS Id,
    OwnerId AS Owner,
    RoleId AS Role,
    ReporterId AS Reporter,
    Status,
    Category,
    ClassificationId,
    SubclassificationId,
    OrganizationId AS Organization,
    PriorityLevelId AS PriorityLevel,
    ProcessModelId AS ProcessModel,
    TriageComplete,
    ReportedDate,
    ReportedDateGMT
FROM
    Event
JOIN
    EventData ON Event.EventId = EventData.EventId
JOIN
    CDODefinition CDO ON EventData.CDOTypeId = CDO.CDODefId
UNION ALL
SELECT
    CDOTypeId AS QualityObjectType,
    2 AS Type,
    CAPAId AS Id,
    OwnerId AS Owner,
    RoleId AS Role,
    ReporterId AS Reporter,
    Status,
    Category,
    ClassificationId,
    SubclassificationId,
    OrganizationId AS Organization,
    PriorityLevelId AS PriorityLevel,
    ProcessModelId AS ProcessModel,
    TriageComplete,
    ReportedDate,
    ReportedDateGMT
FROM
    CAPA;
	
	
DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('ParentObject') 
	) THEN
		DROP VIEW IF EXISTS ParentObject;
	END IF;
END $$;
CREATE VIEW ParentObject AS
SELECT
    PlanId AS Id,
    AssigneeId AS Owner,
    Stage,
    1 AS ParentType
FROM
    ActivityPlan
UNION ALL
SELECT
    EventId AS Id,
    OwnerId AS Owner,
    Status AS Stage,
    2 AS ParentType
FROM
    Event
UNION ALL
SELECT
    CAPAId AS Id,
    OwnerId AS Owner,
    Status AS Stage,
    2 AS ParentType
FROM
    CAPA;
	
	
DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiLabelView') 
	) THEN
		DROP VIEW IF EXISTS csiLabelView;
	END IF;
END $$;
CREATE VIEW csiLabelView AS
SELECT 
	Labels.LabelID,
	Labels.Name,
	Labels.CategoryID,
	COALESCE(Emp.TermId,'0000000000000000') AS TermId,
	COALESCE(Emp.LangId,'0000000000000000') AS LangId,
	COALESCE(Term.labelvalue, Lang.labelvalue, Labels.labelvalue) AS LabelValue
FROM Labels
CROSS JOIN (SELECT DISTINCT LanguageDictionaryId AS LangId, TerminologyDictionaryId TermId FROM Employee) Emp
LEFT JOIN DictionaryLabel Term ON Term.labelid = Labels.labelid AND Term.dictionaryid = Emp.LangId
LEFT JOIN DictionaryLabel Lang ON Lang.labelid = Labels.labelid AND Lang.dictionaryid = Emp.TermId;
	
	
DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('csiActivityView') 
	) THEN
		DROP VIEW IF EXISTS csiActivityView;
	END IF;
END $$;
CREATE VIEW csiActivityView AS
SELECT
	Activity.ActivityId
    ,Activity.ActivityName
    ,Activity.AllowReassignment
    ,Activity.AssigneeId 
    ,Activity.AssigneeRoleId
    ,Activity.AttachmentsId
    ,Activity.AutoComplete
    ,Activity.AutoStart
    ,Activity.CDOTypeId
    ,Activity.CompleteBy
    ,Activity.CompleteByGMT
    ,Activity.CompleteWithinQty
    ,Activity.DataPointCollectionId
    ,Activity.Description
    ,Activity.Designated
    ,Activity.DocumentSetId
    ,Activity.FirstRoutedOnGMT
    ,Activity.IsRequired
    ,Activity.LastCompletedById
    ,Activity.LastCompletedOnGMT
    ,Activity.LastDesignatedOn
    ,Activity.LastDesignatedOnGMT
    ,Activity.LastStage
    ,Activity.Comments 
    ,Activity.ReassignmentComments
    ,Activity.Stage
    ,Activity.ParentId PlanId
    ,ActivityPlan.ParentId ProcessModelId
    ,ActivityPlan.Stage PlanStage
    ,ActivityPlan.LastStage PlanLastStage
FROM Activity
JOIN ActivityPlan ON ActivityPlan.PlanId = Activity.ParentId
UNION
SELECT 
	Activity.ActivityId
	,Activity.ActivityName
    ,Activity.AllowReassignment
    ,Activity.AssigneeId 
    ,Activity.AssigneeRoleId
    ,Activity.AttachmentsId
    ,Activity.AutoComplete
    ,Activity.AutoStart
    ,Activity.CDOTypeId
    ,Activity.CompleteBy
    ,Activity.CompleteByGMT
    ,Activity.CompleteWithinQty
    ,Activity.DataPointCollectionId
    ,Activity.Description
    ,Activity.Designated
    ,Activity.DocumentSetId
    ,Activity.FirstRoutedOnGMT
    ,Activity.IsRequired
    ,Activity.LastCompletedById
    ,Activity.LastCompletedOnGMT
    ,Activity.LastDesignatedOn
    ,Activity.LastDesignatedOnGMT
    ,Activity.LastStage
    ,Activity.Comments 
    ,Activity.ReassignmentComments
    ,Activity.Stage
    ,NULL as PlanId
    ,Activity.ParentId ProcessModelId
    ,0 as PlanStage
    ,0 as PlanLastStage
FROM Activity
JOIN ProcessModel ON ProcessModel.ProcessModelId = Activity.ParentId;


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('TODOListEntries') 
	) THEN
		DROP VIEW IF EXISTS TODOListEntries;
	END IF;
END $$;
CREATE VIEW TODOListEntries AS
SELECT 
	QualityObject.Id AS Identifier    -- Quality Objects
	,5 AS IdentifierType -- Quality Object
	,QualityObject.QualityObjectType AS IdentifierCDOType
	,CASE QualityObject.Status WHEN 2 THEN QualityObject.Reporter ELSE QualityObject.Owner END AS Owner
    ,QualityObject.Role AS Role
    ,NULL AS DesignatedDate
    ,NULL AS DesignatedDateGMT
    ,null::timestamp AS DueDate
    ,NULL::timestamp AS DueDateGMT
    ,QualityObject.ReportedDate AS ReportedDate
    ,QualityObject.ReportedDateGMT AS ReportedDateGMT
    ,CASE WHEN QualityObject.Status = 2 THEN 34  -- QualityRecPendingAssignment
		ELSE 1 END AS NotificationType         -- QualityRecOwnershipAssignment
    ,CASE QualityObject.Type
       WHEN 1 THEN CASE QualityObject.Status
                     WHEN 2 THEN 'EventCreatePageflow'
                     ELSE 'EventRecordView'
                   END
       ELSE        CASE QualityObject.Status
                     WHEN 2 THEN 'CAPACreatePageflow'
                     ELSE 'CAPARecordView'
                   END
      END AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,QualityObject.TriageComplete AS TriageComplete
    ,QualityObject.Status AS QualityStatus
    ,NULL AS ProcessStage
    ,NULL::integer AS ApprovalStatus
    ,NULL AS ActivityPlan
    ,NULL::integer AS ApprovalSubstituteOption
FROM QualityObject
WHERE QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- Process Models
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,null::timestamp AS DueDate 
	,NULL::timestamp AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,2 AS NotificationType	-- ProcessModelAssignment
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel 
INNER JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
	AND	 ProcessModel.Designated = 1 -- TRUE
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Plans
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ActivityPlan.AssigneeId AS Owner 
    ,ActivityPlan.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,4 AS NotificationType	-- PlanAssignment
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
INNER JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ActivityPlan.Stage IN (45, 25, 20, 30) -- (InProcess,InCollaboration,Pending,InReview)
	AND	 ActivityPlan.Designated = 1 -- TRUE
	AND   NOT( (ProcessModel.Stage = 20 AND ProcessModel.LastStage = 45) )       -- IsInModify (for Process Model)
UNION ALL
SELECT 
	Activity.ActivityId AS Identifier   -- Activities
	,25 AS IdentifierType -- Activity
	,Activity.CDOTypeId AS IdentifierCDOType
	,Activity.AssigneeId AS Owner 
    ,Activity.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,Activity.CompleteBy AS DueDate 
	,Activity.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE Activity.LastStage WHEN 20 THEN 5 WHEN 50 THEN 29 ELSE 0 END AS NotificationType	-- (5=>ActivityAssignment, 29=>ActivityReprocessed)
    ,CASE Activity.CDOTypeId 
        WHEN 7800 THEN 'ActivityDispExecution' -- Activity Disposition
        WHEN 7870 THEN 'ActivityInvestExecution' -- Activity Investigation
        ELSE 'ActivityExecution' -- General Activity
        END AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,CASE Activity.DataPointCollectionId WHEN NULL THEN 0 ELSE 1 END AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,Activity.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,Activity.PlanId AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM csiActivityView Activity
INNER JOIN ProcessModel	ON Activity.ProcessModelId = ProcessModel.ProcessModelId
INNER JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE Activity.Stage = 45 -- (InProcess)
	AND	 Activity.LastStage IN (20, 50)  -- (Pending, Completed)
	AND   NOT(   (Activity.PlanStage = 20 AND Activity.PlanLastStage = 45)  -- IsInModify (for Plan)
	OR (ProcessModel.Stage = 20 AND ProcessModel.LastStage = 45)  -- IsInModify (for Process Model)
            )
UNION ALL
SELECT 
	Entry.ApprovalSheetEntryId Identifier   -- Approvals
	,30 AS IdentifierType -- Approval
	,Entry.CDOTypeId AS IdentifierCDOType
	,Entry.ApproverId AS Owner
    ,Entry.ApproverRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,Entry.CompleteBy AS DueDate 
	,Entry.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE ParentObject.ParentType 
         WHEN 1 THEN 8  -- (8=>PlanApprovalAssignment)
         WHEN 2 THEN 6 ELSE 0 END AS NotificationType	-- (6=>QualityRecordApprovalAssignment)
	,CASE ParentObject.ParentType 
         WHEN 1 THEN 'ApprovalPlanEntry'
         ELSE 'ApprovalQualityObjectEntry' END AS ToDoListItemType	
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,ParentObject.Id AS ApprovalSheetParent
    ,Entry.ApproverRoleId AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,NULL AS ProcessStage 
    ,Entry.Status AS ApprovalStatus
	,ActivityPlan.PlanId AS ActivityPlan 
    ,Entry.SubstituteOption AS ApprovalSubstituteOption
FROM ApprovalSheetEntry Entry 
INNER	JOIN ApprovalSheet Sheet ON Entry.ParentId = Sheet.ApprovalSheetId
INNER	JOIN (select * from ParentObject) ParentObject		 ON Sheet.ParentId = ParentObject.Id
LEFT OUTER  JOIN ActivityPlan   ON ParentObject.Id = ActivityPlan.PlanId
LEFT OUTER	JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER		JOIN QualityObject	ON ParentObject.Id       = QualityObject.Id
	OR ProcessModel.ParentId = QualityObject.Id
WHERE Entry.Status = 10
	AND Sheet.Status = 10
	AND ( 
		(ParentObject.Stage = 30 AND ParentObject.ParentType = 1) 
		OR 
		(ParentObject.Stage = 8 AND ParentObject.ParentType = 2 AND Sheet.ApprovalSheetName = 'FromCompleted')
	)
--
-- Additional Messages for Quality Object Owner
--
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- End PM Collaboration for Quality Object Owner
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,QualityObject.Owner AS Owner
    ,QualityObject.Role AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,NULL AS DueDate 
	,NULL AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,11 AS NotificationType	-- Process Model End Collaboration
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel
INNER JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage = 20      -- Pending
	AND ProcessModel.LastStage = 25  -- In Collaboration
	AND QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- Process Model Completion for Quality Object Owner
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,QualityObject.Owner AS Owner
    ,QualityObject.Role AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,NULL AS DueDate 
	,NULL AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,16 AS NotificationType	-- Process Model Completion
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel
   INNER JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage = 50      -- Completed
	AND QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
UNION ALL
SELECT 
	QualityObject.Id AS Identifier    -- Quality Object Reviews Completion for Quality Object Owner
	,5 AS IdentifierType -- Quality Object
	,QualityObject.QualityObjectType AS IdentifierCDOType
	,QualityObject.Owner AS Owner
    ,QualityObject.Role AS Role
    ,NULL AS DesignatedDate
    ,NULL AS DesignatedDateGMT
    ,NULL AS DueDate
    ,NULL AS DueDateGMT
    ,QualityObject.ReportedDate AS ReportedDate
    ,QualityObject.ReportedDateGMT AS ReportedDateGMT
	,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 9  -- Quality Record Resolution Approved
        WHEN 30 THEN 10  -- Quality Record Resolution Rejected
      END AS NotificationType
    ,'ApprovalQualityObjectComplete' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,QualityObject.TriageComplete AS TriageComplete
    ,QualityObject.Status AS QualityStatus
    ,NULL AS ProcessStage
    ,NULL::integer AS ApprovalStatus
    ,NULL AS ActivityPlan
    ,NULL::integer AS ApprovalSubstituteOption
FROM QualityObject
	INNER JOIN ApprovalSheet  ON QualityObject.Id   = ApprovalSheet.ParentId
    AND ApprovalSheet.ApprovalSheetName = 'FromCompleted'   -- From Completed
WHERE QualityObject.Status = 8   -- In Review
    AND ApprovalSheet.Status IN (40,30)   -- (Approved,Rejected)
--
-- Additional Messages for Process Model Assignee
--
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- End Plan Collaboration for Process Model Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,18 AS NotificationType	-- Plan End Collaboration
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
INNER JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER JOIN QualityObject ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
    AND ActivityPlan.Stage = 20      -- Pending
    AND ActivityPlan.LastStage = 25  -- InCollaboration
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- Plan Completion for Process Model Assignee
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,NULL AS DueDate 
	,NULL AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE ProcessModel.OutstandingDetailStatus
		WHEN 3 THEN 30                      -- Disposition Plan Completion Lots Complete
		WHEN 2 THEN 31                      -- Disposition Plan Completion Lots Open
		ELSE 12 END AS NotificationType	-- Process Model Plan Completion
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel 
INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
LEFT	JOIN (SELECT ParentId FROM ActivityPlan 
	GROUP BY ParentId) Plan1 ON ProcessModel.ProcessModelId = Plan1.ParentId
LEFT	JOIN (SELECT ParentId FROM ActivityPlan 
	WHERE Stage NOT IN (5,50) GROUP BY ParentId) Plan2 ON ProcessModel.ProcessModelId = Plan2.ParentId
WHERE ProcessModel.Stage IN (20,45) -- (Pending,InProcess)
    AND ProcessModel.Designated = 1 -- True
    AND Plan1.ParentId IS NOT NULL
    AND Plan2.ParentId IS NULL
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- Activity Completion for Process Model Assignee
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,NULL AS DueDate 
	,NULL AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE ProcessModel.OutstandingDetailStatus
		WHEN 3 THEN 32                      -- Disposition Activity Completion Lots Complete
		WHEN 2 THEN 33                      -- Disposition Plan Completion Lots Open
		ELSE 14 END AS NotificationType	-- Process Model Activity Completion
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel 
INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
LEFT	JOIN (SELECT ParentId FROM Activity 
	GROUP BY ParentId) Act1 ON ProcessModel.ProcessModelId = Act1.ParentId
LEFT	JOIN (SELECT ParentId FROM Activity 
	WHERE Stage NOT IN (5,50) GROUP BY ParentId) Act2 ON ProcessModel.ProcessModelId = Act2.ParentId
WHERE ProcessModel.Stage IN (20,45) -- (Pending,InProcess)
    AND ProcessModel.Designated = 1 -- True
    AND Act1.ParentId IS NOT NULL
    AND Act2.ParentId IS NULL
UNION ALL
SELECT 
	ProcessModel.ProcessModelId AS Identifier   -- Child Completion Failure for Process Model Assignee
	,10 AS IdentifierType -- Process Model
	,ProcessModel.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,ProcessModel.LastDesignatedOn AS DesignatedDate
	,ProcessModel.LastDesignatedOnGMT AS DesignatedDateGMT
	,NULL AS DueDate 
	,NULL AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,17 AS NotificationType	-- Process Model Auto Complete Failure
    ,'ProcessModelOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ProcessModel.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ProcessModel 
INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage = 45 -- InProcess
    AND ProcessModel.AutoComplete = 1 -- True
    AND ProcessModel.AutoCompleteFailure = 1 -- True
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Plan Routing Approvals completion for Process Model Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 22  -- Plan Routing Approvals Approved
        WHEN 30 THEN 23  -- Plan Routing Approvals Rejected
      END AS NotificationType
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
    INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
        AND ApprovalSheet.ApprovalSheetName = '20-45'   -- Pending to InProcess
	INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
	INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
    AND (
            (ActivityPlan.Stage = 45 AND ActivityPlan.LastStage = 30 AND ActivityPlan.AssigneeOption = 1 AND ApprovalSheet.Status = 40)  -- Approved
			OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 20 AND ActivityPlan.AssigneeOption = 1 AND ApprovalSheet.Status = 30)  -- Rejected
        )
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Plan Open Approvals completion for Process Model Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ProcessModel.AssigneeId AS Owner 
    ,ProcessModel.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 26  -- Plan Open Approvals Approved
        WHEN 30 THEN 27  -- Plan Open Approvals Rejected
      END AS NotificationType
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
        INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
            AND ApprovalSheet.ApprovalSheetName IN ('50-20','5-20')   -- Completed to Pending, Void to Pending 
		INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
    AND (
            (ActivityPlan.Stage = 20 AND ActivityPlan.LastStage = 30 AND ApprovalSheet.Status = 40)  -- Approved
			OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage IN (50,5) AND ApprovalSheet.Status = 30)  -- Rejected
        )
--
-- Additional Messages for Plan Assignee
--
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Activity Completion for Plan Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ActivityPlan.AssigneeId AS Owner 
    ,ActivityPlan.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,19 AS NotificationType	-- Plan Activity Completion
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
LEFT	JOIN (SELECT ParentId FROM Activity 
	GROUP BY ParentId) Act1 ON ActivityPlan.PlanId = Act1.ParentId
LEFT	JOIN (SELECT ParentId FROM Activity 
	WHERE Stage NOT IN (5,50) GROUP BY ParentId) Act2 ON ActivityPlan.PlanId = Act2.ParentId
WHERE ActivityPlan.Stage IN (20,45) -- (Pending,InProcess)
    AND ActivityPlan.Designated = 1 -- True
    AND Act1.ParentId IS NOT NULL
    AND Act2.ParentId IS NULL
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Plan Routing Approvals Completion for Plan Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ActivityPlan.AssigneeId AS Owner 
    ,ActivityPlan.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 22  -- Plan Routing Approvals Approved
        WHEN 30 THEN 23  -- Plan Routing Approvals Rejected
      END AS NotificationType
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
    AND ApprovalSheet.ApprovalSheetName = '20-45'   -- Pending to InProcess
INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE (
        (ActivityPlan.Stage = 45 AND ActivityPlan.LastStage = 30 AND ActivityPlan.AssigneeOption = 2 AND ApprovalSheet.Status = 40)  -- Approved
        OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 20 AND ActivityPlan.AssigneeOption = 2 AND ApprovalSheet.Status = 30)  -- Rejected
      )
UNION ALL
SELECT 
	ActivityPlan.PlanId AS Identifier   -- Plan Completion Approvals Completion for Plan Assignee
	,20 AS IdentifierType -- Plan
	,ActivityPlan.CDOTypeId AS IdentifierCDOType
	,ActivityPlan.AssigneeId AS Owner 
    ,ActivityPlan.AssigneeRoleId AS Role
	,NULL AS DesignatedDate
	,NULL AS DesignatedDateGMT
	,ActivityPlan.CompleteBy AS DueDate 
	,ActivityPlan.CompleteByGMT AS DueDateGMT 
    ,NULL AS ReportedDate
    ,NULL AS ReportedDateGMT
	,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 24  -- Plan Completion Approvals Approved
        WHEN 30 THEN 25  -- Plan Completion Approvals Rejected
      END AS NotificationType
    ,'PlanOwnership' AS ToDoListItemType
	,QualityObject.Id AS QualityObject
    ,0 AS DataCollectionDefined
    ,NULL AS ApprovalSheetParent
    ,NULL AS ApprovalEntryRole
    ,QualityObject.Category AS Category
    ,QualityObject.ClassificationId AS ClassificationId
    ,QualityObject.SubclassificationId AS SubclassificationId
    ,QualityObject.Organization AS Organization
    ,QualityObject.PriorityLevel AS PriorityLevel
    ,QualityObject.ProcessModel AS ProcessModel
    ,NULL AS TriageComplete
    ,NULL AS QualityStatus
	,ActivityPlan.Stage AS ProcessStage 
    ,NULL::integer AS ApprovalStatus
	,NULL AS ActivityPlan 
    ,NULL::integer AS ApprovalSubstituteOption
FROM ActivityPlan 
INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
    AND ApprovalSheet.ApprovalSheetName = '45-50'   -- InProcess to Completed
INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
WHERE (
        (ActivityPlan.Stage = 50 AND ActivityPlan.LastStage = 30 AND ApprovalSheet.Status = 40)  -- Approved
        OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 45 AND ApprovalSheet.Status = 30)  -- Rejected
      )
AND ProcessModel.Stage <> 50;  -- Completed


DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.views 
		WHERE lower(table_name) = lower('EVENT_CAPA_V') 
	) THEN
		DROP VIEW IF EXISTS EVENT_CAPA_V;
	END IF;
END $$;
CREATE VIEW EVENT_CAPA_V AS
SELECT 
	E.EventName				AS ObjectName
	,E.EventId				AS ObjectId
	,EMP1.EmployeeName			AS OwnerName
	,ORG1.OrganizationName			AS OwnerOrgName
	,EMP1.EmployeeId			AS OwnerId
	,ORG1.OrganizationId			AS OwnerOrgId
	,PL.PriorityLevelName			AS PriorityLevelName
	,PL.PriorityLevelId			AS PriorityLevelId
	,E.DiscoveryArea			AS DiscoveryArea
	,E.ReportedDate				AS ReportedDate
	,E.ReportedDateGMT			AS ReportedDateGMT
	,E.Category				AS Category
	,CATEGORYFIELD.FieldName		AS CategoryName
	,E.ClassificationId			AS ClassificationId
	,CL.ClassificationName		AS ClassificationName
	,E.SubClassificationId			AS SubClassificationId
	,SCL.SubClassificationName	AS SubClassificationName
	,CD.CDOName				AS CDOName
	,E.Status				AS Status
	,EMP2.EmployeeName			AS InitiatorName
	,ORG2.OrganizationName			AS InitiatorOrgName
	,EMP2.EmployeeID			AS InitiatorId
	,ORG2.OrganizationId			AS InitiatorOrgId
	,EMP3.EmployeeName			AS ReporterName
	,ORG3.OrganizationName			AS ReporterOrgName
	,EMP3.EmployeeId			AS ReporterId
	,ORG3.OrganizationId			AS ReporterOrgId
	,LABELS.LabelValue 			AS StatusName
	,IsCARRequiredToClose			AS IsCARRequiredToClose
	,R.RoleId				AS RoleId
	,R.RoleName				AS RoleName
	,NULL					AS LoginId
	,E.TriageComplete			AS TriageComplete
	,NULL					AS IncludeAllRoles
FROM EVENT E 
LEFT JOIN CDOFIELDS CATEGORYFIELD ON E.Category = CATEGORYFIELD.DefaultValue::integer AND CATEGORYFIELD.CDODefId = 7520
LEFT JOIN Classification CL on E.ClassificationId = CL.ClassificationId
LEFT JOIN SubClassification SCL on E.SubClassificationId = SCL.SubClassificationId
LEFT JOIN CDOFIELDS STATUSFIELD ON E.Status = STATUSFIELD.DefaultValue::integer AND STATUSFIELD.CDODefId = 7657
LEFT JOIN LABELS ON LABELS.LabelId = STATUSFIELD.LabelId
LEFT JOIN EMPLOYEE  EMP1 ON E.OwnerId = EMP1.EmployeeId
LEFT JOIN EMPLOYEE  EMP2 ON E.InitiatorId = EMP2.EmployeeId
LEFT JOIN EMPLOYEE  EMP3 ON E.ReporterId = EMP3.EmployeeId
LEFT JOIN ORGANIZATION ORG1 ON E.OrganizationId = ORG1.OrganizationId
LEFT JOIN ORGANIZATION ORG2 ON E.InitiatorOrganizationId = ORG2.OrganizationId
LEFT JOIN ORGANIZATION ORG3 ON E.ReporterOrganizationId = ORG3.OrganizationId
LEFT JOIN PRIORITYLEVEL PL ON E.PriorityLevelId = PL.PriorityLevelId
INNER JOIN CDODEFINITION CD ON E.CDOTypeId = CD.CDODefId
LEFT JOIN ROLEDEF R ON E.RoleId = R.RoleId
UNION
SELECT 
	C.CAPAName				AS ObjectName
	,C.CAPAId				AS ObjectId
	,EMP1.EmployeeName			AS OwnerName
	,ORG1.OrganizationName			AS OwnerOrgName
	,EMP1.EmployeeId			AS OwnerId
	,ORG1.OrganizationId			AS OwnerOrgId
	,PL.PriorityLevelName			AS PriorityLevelName
	,PL.PriorityLevelId			AS PriorityLevelId
	,NULL					AS DiscoveryArea
	,C.ReportedDate				AS ReportedDate
	,C.ReportedDateGMT			AS ReportedDateGMT
	,C.Category				AS Category
	,CATEGORYFIELD.FieldName		AS CategoryName
	,C.ClassificationId			AS ClassificationId
	,CL.ClassificationName		AS ClassificationName
	,C.SubClassificationId			AS SubClassificationId
	,SCL.SubClassificationName	AS SubClassificationName
	,CD.CDOName				AS CDOName
	,C.Status				AS Status
	,EMP2.EmployeeName			AS InitiatorName
	,ORG2.OrganizationName			AS InitiatorOrgName
	,EMP2.EmployeeId			AS InitiatorId
	,ORG2.OrganizationId			AS InitiatorOrgId
	,EMP3.EmployeeName			AS ReporterName
	,ORG3.OrganizationName			AS ReporterOrgName
	,EMP3.EmployeeId			AS ReporterId
	,ORG3.OrganizationId			AS ReporterOrgId
	,LABELS.LabelValue 			AS StatusName
	,0				AS IsCARRequiredToClose
	,R.RoleId				AS RoleId
	,R.RoleName				AS RoleName
	,NULL					AS LoginId
	,C.TriageComplete			AS TriageComplete
	,NULL					AS IncludeAllRoles
FROM CAPA C
LEFT JOIN CDOFIELDS CATEGORYFIELD ON C.Category = CATEGORYFIELD.DefaultValue::integer AND CATEGORYFIELD.CDODefId = 7520
LEFT JOIN Classification CL on C.ClassificationId = CL.ClassificationId
LEFT JOIN SubClassification SCL on C.SubClassificationId = SCL.SubClassificationId
LEFT JOIN CDOFIELDS STATUSFIELD ON C.Status = STATUSFIELD.DefaultValue::integer AND STATUSFIELD.CDODefId = 7658
LEFT JOIN LABELS ON LABELS.LabelId = STATUSFIELD.LabelId
LEFT JOIN EMPLOYEE  EMP1 ON C.OwnerId = EMP1.EmployeeId
LEFT JOIN EMPLOYEE  EMP2 ON C.InitiatorId = EMP2.EmployeeId
LEFT JOIN EMPLOYEE  EMP3 ON C.ReporterId = EMP3.EmployeeId
LEFT JOIN ORGANIZATION ORG1 ON C.OrganizationId = ORG1.OrganizationId
LEFT JOIN ORGANIZATION ORG2 ON C.InitiatorOrganizationId = ORG2.OrganizationId
LEFT JOIN ORGANIZATION ORG3 ON C.ReporterOrganizationId = ORG3.OrganizationId
LEFT JOIN PRIORITYLEVEL PL ON C.PriorityLevelId = PL.PriorityLevelId
INNER JOIN CDODEFINITION CD ON C.CDOTypeId = CD.CDODefId
LEFT JOIN ROLEDEF R ON C.RoleId = R.RoleId;


DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM pg_indexes 
        WHERE lower(indexname) = lower('CDOFIELDS_NUI1')
        AND lower(tablename) = lower('CDOFIELDS')
    ) THEN
        EXECUTE 'DROP INDEX CDOFIELDS_NUI1';
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS CDOFIELDS_NUI1
ON CDOFIELDS(
	CDODefId
);


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiQualityObjectInquiry_EventCAPA')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiQualityObjectInquiry_EventCAPA;
 	END IF;
END $$;
CREATE FUNCTION csiQualityObjectInquiry_EventCAPA(
	p_ObjectName			VARCHAR(256)
   ,p_ObjectId				VARCHAR(30)
   ,p_OwnerId				VARCHAR(30)
   ,p_OwnerOrgId			VARCHAR(30)
   ,p_PriorityLevelId			VARCHAR(30)
   ,p_DiscoveryArea			VARCHAR(256)
   ,p_ReportFromGMT			TIMESTAMP
   ,p_ReportToGMT			TIMESTAMP
   ,p_Category				INTEGER
   ,p_ClassificationId			VARCHAR(30)
   ,p_SubClassificationId		VARCHAR(30)
   ,p_Status				INTEGER
   ,p_InitiatorId			VARCHAR(30)
   ,p_InitiatorOrgId			VARCHAR(30)
   ,p_ReporterId			VARCHAR(30)
   ,p_ReporterOrgId			VARCHAR(30)
   ,p_RoleId				VARCHAR(30)
   ,p_EmployeeLoginId  		VARCHAR(30)
   ,p_IncludeAllRoles  		INTEGER = 0
)
RETURNS TABLE (
	OBJECTNAME		VARCHAR(30)
	,OBJECTID			VARCHAR(30)
	,OWNERNAME			VARCHAR(256)
	,OWNERORGNAME		VARCHAR(256)
	,PRIORITYLEVELNAME		VARCHAR(256)
	,DISCOVERYAREA		VARCHAR(256)
	,REPORTEDDATE		TIMESTAMP
	,REPORTEDDATEGMT		TIMESTAMP
	,CATEGORY			INTEGER
	,CATEGORYNAME		VARCHAR(256)
	,CLASSIFICATIONID		VARCHAR(30)
	,CLASSIFICATIONNAME	VARCHAR(256)
	,SUBCLASSIFICATIONID	VARCHAR(30)
	,SUBCLASSIFICATIONNAME	VARCHAR(256)
	,CDONAME			VARCHAR(256)
	,STATUS			INTEGER
	,STATUSNAME		VARCHAR(256)
	,ISCARREQUIREDTOCLOSE	BOOLEAN
	,ROLENAME			VARCHAR(256)
	,TRIAGECOMPLETE		BOOLEAN
)
language plpgsql
as $$
DECLARE
BEGIN
	
	RETURN QUERY
	SELECT 
			o.ObjectName
			,o.ObjectId::VARCHAR(30)
			,o.OwnerName
			,o.OwnerOrgName
			,o.PriorityLevelName
			,o.DiscoveryArea
			,o.ReportedDate
			,o.ReportedDateGMT
			,o.Category
			,o.CategoryName
			,o.ClassificationId::VARCHAR(30)
			,o.ClassificationName
			,o.SubClassificationId::VARCHAR(30)
			,o.SubClassificationName
			,o.CDOName
			,o.Status
			,o.StatusName
			,o.IsCARRequiredToClose
			,o.RoleName
			,o.TriageComplete
		FROM (
			SELECT * FROM EVENT_CAPA_V
		)o
		WHERE (o.ObjectId = p_ObjectId  OR p_ObjectId  = '')
			AND (o.OwnerOrgId = p_OwnerOrgId OR p_OwnerOrgId = '')
			AND (o.PriorityLevelId = p_PriorityLevelId OR p_PriorityLevelId = '')
			   AND (o.DiscoveryArea = p_DiscoveryArea OR p_DiscoveryArea = '')
			   AND (o.ReportedDateGMT >= 
					CASE 
						WHEN COALESCE(p_ReportFromGMT::VARCHAR(30), 'XXX') = 'XXX' 
						THEN '1900-01-01 00:00:00'::TIMESTAMP
						ELSE (p_ReportFromGMT::TIMESTAMP)
					end
					OR p_ReportFromGMT = null::timestamp)
			   AND (o.ReportedDateGMT <
					CASE
						WHEN COALESCE(p_ReportToGMT::VARCHAR(30), 'XXX') = 'XXX' 
						THEN (clock_timestamp() + INTERVAL '1 day')
						ELSE ((p_ReportToGMT + INTERVAL '1 day')::TIMESTAMP)
					end
					OR p_ReportToGMT = null::timestamp)
			   AND (o.Category = p_Category OR p_Category = -1)
			   AND (o.ClassificationId = p_ClassificationId OR p_ClassificationId = '')
			   AND (o.SubClassificationId = p_SubClassificationId OR p_SubClassificationId = '')
			   AND (o.Status = p_Status OR p_Status = -1)
			   AND (o.InitiatorId = p_InitiatorId OR p_InitiatorId = '')
			   AND (o.InitiatorOrgId = p_InitiatorOrgId  OR p_InitiatorOrgId  = '')
			   AND (o.ReporterId = p_ReporterId OR p_ReporterId = '')
			   AND (o.ReporterOrgId = p_ReporterOrgId OR p_ReporterOrgId = '')
			   AND (o.ObjectName LIKE p_ObjectName )
			   AND ((o.RoleId = p_RoleId AND 
				 p_OwnerId IN (SELECT EmployeeId FROM csiAuthGetAllowedUsers(o.OwnerOrgName, o.RoleName)) ) OR 
				p_RoleId = '' )
			   AND (o.OwnerId = p_OwnerId OR 
				p_OwnerId = '' OR 
				p_RoleId <> '' OR 
				p_IncludeAllRoles = 1 )
			   AND (( p_IncludeAllRoles = 1 AND 
				  p_OwnerId IN (SELECT EmployeeId FROM csiAuthGetAllowedUsers(o.OwnerOrgName, o.RoleName)) ) OR
				  p_IncludeAllRoles = 0 )
			   AND (p_EmployeeLoginId IN (select EmployeeId from csiAuthGetAllowedUsers(o.OwnerOrgName,'')))
			ORDER BY o.ObjectName;
END;
$$;

/** Code for testing the above Table function
SELECT *
  FROM csiQualityObjectInquiry_EventCAPA ( null::varchar(256)			-- ObjectName		-VARCHAR(256)
					  ,NULL::varchar(30)			-- ObjectId		-VARCHAR(30)
					  ,NULL::varchar(30)			-- OwnerId		-VARCHAR(30)
					  ,NULL::varchar(30)			-- OwnerOrgId		-VARCHAR(30)
					  ,NULL::varchar(30)			-- PriorityLevelId	-VARCHAR(30)
					  ,NULL::varchar(256)			-- DiscoveryArea	-VARCHAR(256)
					  ,clock_timestamp()::timestamp without time zone - interval '100 day'	-- ReportFromGMT	-TIMESTAMP
					  ,clock_timestamp()::timestamp without time zone - interval '10 day'	-- ReportToGMT		-TIMESTAMP
					  ,-1			-- Category		-INTEGER
					  ,CAST(-1 AS VARCHAR(30))			-- Classification	-VARCHAR(16)
					  ,CAST(-1 AS VARCHAR(30))			-- SubClassification -VARCHAR(16)
					  ,-1			-- Status		-INTEGER
					  ,NULL::varchar(30)			-- InitiatorId		-VARCHAR(30)
					  ,NULL::varchar(30)			-- InitiatorOrgId	-VARCHAR(30)
					  ,NULL::varchar(30)			-- ReporterId		-VARCHAR(30)
					  ,NULL::varchar(30)			-- ReporterOrgId	-VARCHAR(30)
					  ,NULL::varchar(30)			-- RoleId		-VARCHAR(30)
					  ,NULL::varchar(30) 			-- EmployeeLoginId  	-VARCHAR(30)
					  ,0);			-- IncludeAllRoles  	-INTEGER = 0)
**/