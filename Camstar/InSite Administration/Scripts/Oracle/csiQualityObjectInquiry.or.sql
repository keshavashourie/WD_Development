-- Copyright Siemens 2025  
CREATE OR REPLACE View QualityObject ( QualityObjectType
			   ,Type
			   ,Id
			   ,Owner
               ,Role
			   ,Reporter
			   ,Status
			   ,Category
			   ,ClassificationId
			   ,SubclassificationId
			   ,Organization
			   ,PriorityLevel
			   ,ProcessModel
			   ,TriageComplete
			   ,ReportedDate
			   ,ReportedDateGMT )
AS
( SELECT Event.CDOTypeId AS QualityObjectType
	,1 AS Type
	,Event.EventId AS Id
	,OwnerId AS Owner
	,RoleId AS Role
	,ReporterId AS Reporter
	,Status
	,Category
	,ClassificationId
	,SubclassificationId
	,OrganizationId AS Organization
	,PriorityLevelId AS PriorityLevel
	,ProcessModelId AS ProcessModel
	,TriageComplete
	,ReportedDate
	,ReportedDateGMT
    FROM Event 
      JOIN EventData ON Event.EventId = EventData.EventId
      JOIN CDODefinition CDO ON EventData.CDOTypeId = CDO.CDODefId
  UNION ALL
  SELECT CDOTypeId AS QualityObjectType
	,2 AS Type
	,CAPAId AS Id
	,OwnerId AS Owner
	,RoleId AS Role
	,ReporterId AS Reporter
	,Status
	,Category
	,ClassificationId
	,SubclassificationId
	,OrganizationId AS Organization
	,PriorityLevelId AS PriorityLevel
	,ProcessModelId AS ProcessModel
	,TriageComplete
	,ReportedDate
	,ReportedDateGMT
   FROM CAPA )
/

CREATE OR REPLACE View ProcessObject_Parent ( Id
				     ,Owner
				     ,Stage
				     ,ParentType )
AS
( SELECT PlanId AS "Id"
	,AssigneeId AS "Owner"
	,Stage
	,1 AS "ParentType"
    FROM ActivityPlan 
  UNION 
  SELECT EventId AS "Id"
	,OwnerId AS "Owner"
	,Status AS "Stage"
	,2 AS "ParentType"
    FROM Event 
  UNION 
  SELECT CAPAId AS "Id"
	,OwnerId AS "Owner"
	,Status AS "Stage"
	,2 AS "ParentType"
   FROM CAPA )
/

CREATE OR REPLACE View csiLabelView ( LabelID
            ,Name
            ,CategoryID
            ,TermId 
            ,LangId
            ,LabelValue )
AS
(  SELECT 
		Labels.LabelID,
		Labels.Name,
		Labels.CategoryID,
		COALESCE(Emp.TermId,'0000000000000000') TermId,
		COALESCE(Emp.LangId,'0000000000000000') LangId,
		COALESCE(Term.labelvalue, Lang.labelvalue, Labels.labelvalue) LabelValue
	FROM Labels
	JOIN (SELECT DISTINCT LanguageDictionaryId LangId, TerminologyDictionaryId TermId FROM Employee) Emp ON 1 = 1
	LEFT JOIN DictionaryLabel Term ON Term.labelid = Labels.labelid AND Term.dictionaryid = Emp.LangId
	LEFT JOIN DictionaryLabel Lang ON Lang.labelid = Labels.labelid AND Lang.dictionaryid = Emp.TermId )
/

CREATE OR REPLACE View csiActivityView ( ActivityId
            ,ActivityName
            ,AllowReassignment
            ,AssigneeId 
            ,AssigneeRoleId
            ,AttachmentsId
            ,AutoComplete
            ,AutoStart
            ,CDOTypeId
            ,CompleteBy
            ,CompleteByGMT
            ,CompleteWithinQty
            ,DataPointCollectionId
            ,Description
            ,Designated
            ,DocumentSetId
            ,FirstRoutedOnGMT
            ,IsRequired
            ,LastCompletedById
            ,LastCompletedOnGMT
            ,LastDesignatedOn
            ,LastDesignatedOnGMT
            ,LastStage
            ,Comments 
            ,ReassignmentComments
            ,Stage
            ,PlanId
            ,ProcessModelId
            ,PlanStage
            ,PlanLastStage )
AS
(  SELECT Activity.ActivityId
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
     SELECT Activity.ActivityId
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
            ,NULL PlanId
            ,Activity.ParentId ProcessModelId
            ,0 PlanStage
            ,0 PlanLastStage
     FROM Activity
     JOIN ProcessModel ON ProcessModel.ProcessModelId = Activity.ParentId )
/

CREATE OR REPLACE View TODOListEntries ( Identifier
                 ,IdentifierType
                 ,IdentifierCDOType
                 ,Owner
                 ,Role
                 ,DesignatedDate
                 ,DesignatedDateGMT
                 ,DueDate
                 ,DueDateGMT
                 ,ReportedDate
                 ,ReportedDateGMT
                 ,NotificationType
                 ,ToDoListItemType
                 ,QualityObject
                 ,DataCollectionDefined
                 ,ApprovalSheetParent
                 ,ApprovalEntryRole
                 ,Category
                 ,ClassificationId
                 ,SubclassificationId
                 ,Organization
                 ,PriorityLevel
                 ,ProcessModel
                 ,TriageComplete
                 ,QualityStatus
                 ,ProcessStage
                 ,ApprovalStatus
                 ,ActivityPlan
                 ,ApprovalSubstituteOption )
AS
( SELECT QualityObject.Id AS "Identifier"    -- Quality Objects
	,5 AS "IdentifierType" -- Quality Object
    ,QualityObject.QualityObjectType AS "IdentifierCDOType"
    ,CASE QualityObject.Status WHEN 2 THEN QualityObject.Reporter ELSE QualityObject.Owner END AS "Owner"
    ,QualityObject.Role AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,NULL AS "DueDate"
    ,NULL AS "DueDateGMT"
    ,QualityObject.ReportedDate AS "ReportedDate"
    ,QualityObject.ReportedDateGMT AS "ReportedDateGMT"
    ,CASE WHEN QualityObject.Status = 2 THEN 34  -- QualityRecPendingAssignment
		ELSE 1 END AS "NotificationType"         -- QualityRecOwnershipAssignment
    ,CASE QualityObject.Type
       WHEN 1 THEN CASE QualityObject.Status
                     WHEN 2 THEN 'EventCreatePageflow'
                     ELSE 'EventRecordView'
                   END
       ELSE        CASE QualityObject.Status
                     WHEN 2 THEN 'CAPACreatePageflow'
                     ELSE 'CAPARecordView'
                   END
      END AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,QualityObject.TriageComplete AS "TriageComplete"
    ,QualityObject.Status AS "QualityStatus"
    ,NULL AS "ProcessStage"
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan"
    ,NULL AS "ApprovalSubstituteOption"
    FROM QualityObject
   WHERE QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- Process Models
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,2 AS "NotificationType"    -- ProcessModelAssignment
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel 
            INNER    JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
   AND     ProcessModel.Designated = 1
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Plans
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ActivityPlan.AssigneeId AS "Owner" 
    ,ActivityPlan.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,4 AS "NotificationType"    -- PlanAssignment
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ActivityPlan.Stage IN (45, 25, 20, 30) -- (InProcess,InCollaboration,Pending,InReview)
   AND     ActivityPlan.Designated = 1
   AND   NOT( (ProcessModel.Stage = 20 AND ProcessModel.LastStage = 45) )       -- IsInModify (for Process Model)
  UNION ALL
  SELECT Activity.ActivityId AS "Identifier"   -- Activities
	,25 AS "IdentifierType" -- Activity
    ,Activity.CDOTypeId AS "IdentifierCDOType"
    ,Activity.AssigneeId AS "Owner" 
    ,Activity.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,Activity.CompleteBy AS "DueDate" 
    ,Activity.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE Activity.LastStage WHEN 20 THEN 5 WHEN 50 THEN 29 ELSE 0 END AS "NotificationType"    -- (5=>ActivityAssignment, 29=>ActivityReprocessed)
    ,CASE Activity.CDOTypeId 
        WHEN 7800 THEN 'ActivityDispExecution' -- Activity Disposition
        WHEN 7870 THEN 'ActivityInvestExecution' -- Activity Investigation
        ELSE 'ActivityExecution' -- General Activity
        END AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,CASE Activity.DataPointCollectionId WHEN NULL THEN 0 ELSE 1 END AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,Activity.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,Activity.PlanId AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM csiActivityView Activity
            INNER        JOIN ProcessModel    ON Activity.ProcessModelId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE Activity.Stage = (45) -- (InProcess)
   AND     Activity.LastStage IN (20, 50)  -- (Pending, Completed)
   AND   NOT(   (Activity.PlanStage = 20 AND Activity.PlanLastStage = 45)  -- IsInModify (for Plan)
             OR (ProcessModel.Stage = 20 AND ProcessModel.LastStage = 45)  -- IsInModify (for Process Model)
            )
  UNION ALL
  SELECT Entry.ApprovalSheetEntryId "Identifier"   -- Approvals
	,30 AS "IdentifierType" -- Approval
    ,Entry.CDOTypeId AS "IdentifierCDOType"
    ,Entry.ApproverId AS "Owner"
    ,Entry.ApproverRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,Entry.CompleteBy AS "DueDate" 
    ,Entry.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE ProcessObject_Parent.ParentType 
         WHEN 1 THEN 8  -- (8=>PlanApprovalAssignment)
         WHEN 2 THEN 6 ELSE 0 END AS "NotificationType"    -- (6=>QualityRecordApprovalAssignment)
    ,CASE ProcessObject_Parent.ParentType 
         WHEN 1 THEN 'ApprovalPlanEntry'
         ELSE 'ApprovalQualityObjectEntry' END AS "ToDoListItemType"    
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,ProcessObject_Parent.Id AS "ApprovalSheetParent"
    ,Entry.ApproverRoleId AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,NULL AS "ProcessStage" 
    ,Entry.Status AS "ApprovalStatus"
    ,ActivityPlan.PlanId AS "ActivityPlan" 
    ,Entry.SubstituteOption AS "ApprovalSubstituteOption"
    FROM ApprovalSheetEntry Entry 
            INNER    JOIN ApprovalSheet Sheet ON Entry.ParentId = Sheet.ApprovalSheetId
            INNER    JOIN ProcessObject_Parent         ON Sheet.ParentId = ProcessObject_Parent.Id
            LEFT OUTER  JOIN ActivityPlan   ON ProcessObject_Parent.Id = ActivityPlan.PlanId
            LEFT OUTER    JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessObject_Parent.Id       = QualityObject.Id
                                            OR ProcessModel.ParentId = QualityObject.Id
   WHERE Entry.Status = 10
     AND Sheet.Status = 10
     AND ( (ProcessObject_Parent.Stage = 30 AND ProcessObject_Parent.ParentType = 1) 
           OR 
           (ProcessObject_Parent.Stage = 8 AND ProcessObject_Parent.ParentType = 2 AND Sheet.ApprovalSheetName = 'FromCompleted')
         )
  --
  -- Additional Messages for Quality Object Owner
  --
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- End PM Collaboration for Quality Object Owner
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,QualityObject.Owner AS "Owner"
    ,QualityObject.Role AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,11 AS "NotificationType"    -- Process Model End Collaboration
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage = 20      -- Pending
     AND ProcessModel.LastStage = 25  -- In Collaboration
     AND QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- Process Model Completion for Quality Object Owner
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,QualityObject.Owner AS "Owner"
    ,QualityObject.Role AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,16 AS "NotificationType"    -- Process Model Completion
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage = 50      -- Completed
     AND QualityObject.Status  Not IN (5, 6)  -- (Closed,Deleted)
  UNION ALL
  SELECT QualityObject.Id AS "Identifier"    -- Quality Object Reviews Completion for Quality Object Owner
	,5 AS "IdentifierType" -- Quality Object
    ,QualityObject.QualityObjectType AS "IdentifierCDOType"
    ,QualityObject.Owner AS "Owner"
    ,QualityObject.Role AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,NULL AS "DueDate"
    ,NULL AS "DueDateGMT"
    ,QualityObject.ReportedDate AS "ReportedDate"
    ,QualityObject.ReportedDateGMT AS "ReportedDateGMT"
    ,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 9  -- Quality Record Resolution Approved
        WHEN 30 THEN 10  -- Quality Record Resolution Rejected
      END AS "NotificationType"
    ,'ApprovalQualityObjectComplete' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,QualityObject.TriageComplete AS "TriageComplete"
    ,QualityObject.Status AS "QualityStatus"
    ,NULL AS "ProcessStage"
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan"
    ,NULL AS "ApprovalSubstituteOption"
    FROM QualityObject
            INNER       JOIN ApprovalSheet  ON QualityObject.Id   = ApprovalSheet.ParentId
                                           AND ApprovalSheet.ApprovalSheetName = 'FromCompleted'   -- From Completed
   WHERE QualityObject.Status = 8   -- In Review
     AND ApprovalSheet.Status IN (40,30)   -- (Approved,Rejected)
  --
  -- Additional Messages for Process Model Assignee
  --
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- End Plan Collaboration for Process Model Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,18 AS "NotificationType"    -- Plan End Collaboration
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
     AND ActivityPlan.Stage = 20      -- Pending
     AND ActivityPlan.LastStage = 25  -- InCollaboration
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- Plan Completion for Process Model Assignee
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
	,CASE ProcessModel.OutstandingDetailStatus
		WHEN 3 THEN 30                      -- Disposition Plan Completion Lots Complete
		WHEN 2 THEN 31                      -- Disposition Plan Completion Lots Open
		ELSE 12 END AS "NotificationType"	-- Process Model Plan Completion
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel 
            INNER    JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
            LEFT    JOIN (SELECT ParentId FROM ActivityPlan 
                GROUP BY ParentId) Plan1 ON ProcessModel.ProcessModelId = Plan1.ParentId
            LEFT    JOIN (SELECT ParentId FROM ActivityPlan 
                WHERE Stage NOT IN (5,50) GROUP BY ParentId) Plan2 ON ProcessModel.ProcessModelId = Plan2.ParentId
   WHERE ProcessModel.Stage IN (20,45) -- (Pending,InProcess)
     AND ProcessModel.Designated = 1
     AND Plan1.ParentId IS NOT NULL
     AND Plan2.ParentId IS NULL
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- Activity Completion for Process Model Assignee
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
	,CASE ProcessModel.OutstandingDetailStatus
		WHEN 3 THEN 32                      -- Disposition Activity Completion Lots Complete
		WHEN 2 THEN 33                      -- Disposition Plan Completion Lots Open
		ELSE 14 END AS "NotificationType"	-- Process Model Activity Completion
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel 
            INNER    JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
            LEFT    JOIN (SELECT ParentId FROM Activity 
                GROUP BY ParentId) Act1 ON ProcessModel.ProcessModelId = Act1.ParentId
            LEFT    JOIN (SELECT ParentId FROM Activity 
                WHERE Stage NOT IN (5,50) GROUP BY ParentId) Act2 ON ProcessModel.ProcessModelId = Act2.ParentId
   WHERE ProcessModel.Stage IN (20,45) -- (Pending,InProcess)
     AND ProcessModel.Designated = 1
     AND Act1.ParentId IS NOT NULL
     AND Act2.ParentId IS NULL
  UNION ALL
  SELECT ProcessModel.ProcessModelId AS "Identifier"   -- Child Completion Failure for Process Model Assignee
	,10 AS "IdentifierType" -- Process Model
    ,ProcessModel.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,ProcessModel.LastDesignatedOn AS "DesignatedDate"
    ,ProcessModel.LastDesignatedOnGMT AS "DesignatedDateGMT"
    ,NULL AS "DueDate" 
    ,NULL AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,17 AS "NotificationType"    -- Process Model Auto Complete Failure
    ,'ProcessModelOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ProcessModel.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ProcessModel 
            INNER    JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage = 45 -- InProcess
     AND ProcessModel.AutoComplete = 1
     AND ProcessModel.AutoCompleteFailure = 1
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Plan Routing Approvals completion for Process Model Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 22  -- Plan Routing Approvals Approved
        WHEN 30 THEN 23  -- Plan Routing Approvals Rejected
      END AS "NotificationType"
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
                                           AND ApprovalSheet.ApprovalSheetName = '20-45'   -- Pending to InProcess
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
     AND (
             (ActivityPlan.Stage = 45 AND ActivityPlan.LastStage = 30 AND ActivityPlan.AssigneeOption = 1 AND ApprovalSheet.Status = 40)  -- Approved
          OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 20 AND ActivityPlan.AssigneeOption = 1 AND ApprovalSheet.Status = 30)  -- Rejected
         )
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Plan Open Approvals completion for Process Model Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ProcessModel.AssigneeId AS "Owner" 
    ,ProcessModel.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 26  -- Plan Open Approvals Approved
        WHEN 30 THEN 27  -- Plan Open Approvals Rejected
      END AS "NotificationType"
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
                                           AND ApprovalSheet.ApprovalSheetName IN ('50-20','5-20')   -- Completed to Pending, Void to Pending 
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
     AND (
             (ActivityPlan.Stage = 20 AND ActivityPlan.LastStage = 30 AND ApprovalSheet.Status = 40)  -- Approved
          OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage IN (50,5) AND ApprovalSheet.Status = 30)  -- Rejected
         )
  --
  -- Additional Messages for Plan Assignee
  --
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Activity Completion for Plan Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ActivityPlan.AssigneeId AS "Owner" 
    ,ActivityPlan.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,19 AS "NotificationType"    -- Plan Activity Completion
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
            LEFT    JOIN (SELECT ParentId FROM Activity 
                GROUP BY ParentId) Act1 ON ActivityPlan.PlanId = Act1.ParentId
            LEFT    JOIN (SELECT ParentId FROM Activity 
                WHERE Stage NOT IN (5,50) GROUP BY ParentId) Act2 ON ActivityPlan.PlanId = Act2.ParentId
   WHERE ActivityPlan.Stage IN (20,45) -- (Pending,InProcess)
     AND ActivityPlan.Designated = 1
     AND Act1.ParentId IS NOT NULL
     AND Act2.ParentId IS NULL
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Plan Routing Approvals Completion for Plan Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ActivityPlan.AssigneeId AS "Owner" 
    ,ActivityPlan.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 22  -- Plan Routing Approvals Approved
        WHEN 30 THEN 23  -- Plan Routing Approvals Rejected
      END AS "NotificationType"
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
                                           AND ApprovalSheet.ApprovalSheetName = '20-45'   -- Pending to InProcess
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE (
             (ActivityPlan.Stage = 45 AND ActivityPlan.LastStage = 30 AND ActivityPlan.AssigneeOption = 2 AND ApprovalSheet.Status = 40)  -- Approved
          OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 20 AND ActivityPlan.AssigneeOption = 2 AND ApprovalSheet.Status = 30)  -- Rejected
         )
  UNION ALL
  SELECT ActivityPlan.PlanId AS "Identifier"   -- Plan Completion Approvals Completion for Plan Assignee
	,20 AS "IdentifierType" -- Plan
    ,ActivityPlan.CDOTypeId AS "IdentifierCDOType"
    ,ActivityPlan.AssigneeId AS "Owner" 
    ,ActivityPlan.AssigneeRoleId AS "Role"
    ,NULL AS "DesignatedDate"
    ,NULL AS "DesignatedDateGMT"
    ,ActivityPlan.CompleteBy AS "DueDate" 
    ,ActivityPlan.CompleteByGMT AS "DueDateGMT" 
    ,NULL AS "ReportedDate"
    ,NULL AS "ReportedDateGMT"
    ,CASE (ApprovalSheet.Status)
        WHEN 40 THEN 24  -- Plan Completion Approvals Approved
        WHEN 30 THEN 25  -- Plan Completion Approvals Rejected
      END AS "NotificationType"
    ,'PlanOwnership' AS "ToDoListItemType"
    ,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,NULL AS "ApprovalSheetParent"
    ,NULL AS "ApprovalEntryRole"
    ,QualityObject.Category AS "Category"
    ,QualityObject.ClassificationId AS "ClassificationId"
    ,QualityObject.SubclassificationId AS "SubclassificationId"
    ,QualityObject.Organization AS "Organization"
    ,QualityObject.PriorityLevel AS "PriorityLevel"
    ,QualityObject.ProcessModel AS "ProcessModel"
    ,NULL AS "TriageComplete"
    ,NULL AS "QualityStatus"
    ,ActivityPlan.Stage AS "ProcessStage" 
    ,NULL AS "ApprovalStatus"
    ,NULL AS "ActivityPlan" 
    ,NULL AS "ApprovalSubstituteOption"
    FROM ActivityPlan 
            INNER       JOIN ApprovalSheet  ON ActivityPlan.LastApprovalRoutedId = ApprovalSheet.ApprovalSheetId
                                           AND ApprovalSheet.ApprovalSheetName = '45-50'   -- InProcess to Completed
            INNER        JOIN ProcessModel    ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
            INNER        JOIN QualityObject    ON ProcessModel.ParentId = QualityObject.Id
   WHERE (
             (ActivityPlan.Stage = 50 AND ActivityPlan.LastStage = 30 AND ApprovalSheet.Status = 40)  -- Approved
          OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 45 AND ApprovalSheet.Status = 30)  -- Rejected
         )
     AND ProcessModel.Stage <> 50  -- Completed
)
/

CREATE OR REPLACE VIEW EVENT_CAPA_V ( ObjectName
				     ,ObjectId
				     ,OwnerName
				     ,OwnerOrgName
				     ,OwnerId
				     ,OwnerOrgId
				     ,PriorityLevelName
				     ,PriorityLevelId
				     ,DiscoveryArea
				     ,ReportedDate
				     ,ReportedDateGMT
				     ,Category
				     ,CategoryName
				     ,ClassificationId
				     ,ClassificationName
				     ,SubClassificationId
				     ,SubClassificationName
				     ,CDOName
				     ,Status
				     ,InitiatorName
				     ,InitiatorOrgName
				     ,InitiatorId
				     ,InitiatorOrgId
				     ,ReporterName
				     ,ReporterOrgName
				     ,ReporterId
				     ,ReporterOrgId
				     ,StatusName
				     ,IsCARRequiredToClose
				     ,RoleId
				     ,RoleName
				     ,LoginId
				     ,TriageComplete
				     ,IncludeAllRoles )
AS
( SELECT E.EventName				AS ObjectName
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
	,LABELS.LabelValue			AS StatusName
	,IsCARRequiredToClose			AS IsCARRequiredToClose
	,R.RoleId				AS RoleId
	,R.RoleName				AS RoleName
	,NULL					AS LoginId
	,E.TriageComplete			AS TriageComplete
	,NULL					AS IncludeAllRoles
    FROM EVENT E 
		 LEFT JOIN CDOFIELDS CATEGORYFIELD ON E.Category = CATEGORYFIELD.DefaultValue AND CATEGORYFIELD.CDODefId = 7520
		 LEFT JOIN Classification CL on E.ClassificationId = CL.ClassificationId
		 LEFT JOIN SubClassification SCL on E.SubClassificationId = SCL.SubClassificationId
		 LEFT JOIN CDOFIELDS STATUSFIELD ON E.Status = STATUSFIELD.DefaultValue AND STATUSFIELD.CDODefId = 7657
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
  SELECT C.CAPAName				AS ObjectName
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
	,LABELS.LabelValue			AS StatusName
	,0					AS IsCARRequiredToClose
	,R.RoleId				AS RoleId
	,R.RoleName				AS RoleName
	,NULL					AS LoginId
	,C.TriageComplete			AS TriageComplete
	,NULL					AS IncludeAllRoles
    FROM CAPA C 
		LEFT JOIN CDOFIELDS CATEGORYFIELD ON C.Category = CATEGORYFIELD.DefaultValue AND CATEGORYFIELD.CDODefId = 7520
		LEFT JOIN Classification CL on C.ClassificationId = CL.ClassificationId
		LEFT JOIN SubClassification SCL on C.SubClassificationId = SCL.SubClassificationId
		LEFT JOIN CDOFIELDS STATUSFIELD ON C.Status = STATUSFIELD.DefaultValue AND STATUSFIELD.CDODefId = 7658
		LEFT JOIN LABELS ON LABELS.LabelId = STATUSFIELD.LabelId
		LEFT JOIN EMPLOYEE  EMP1 ON C.OwnerId = EMP1.EmployeeId
		LEFT JOIN EMPLOYEE  EMP2 ON C.InitiatorId = EMP2.EmployeeId
		LEFT JOIN EMPLOYEE  EMP3 ON C.ReporterId = EMP3.EmployeeId
		LEFT JOIN ORGANIZATION ORG1 ON C.OrganizationId = ORG1.OrganizationId
		LEFT JOIN ORGANIZATION ORG2 ON C.InitiatorOrganizationId = ORG2.OrganizationId
		LEFT JOIN ORGANIZATION ORG3 ON C.ReporterOrganizationId = ORG3.OrganizationId
		LEFT JOIN PRIORITYLEVEL PL ON C.PriorityLevelId = PL.PriorityLevelId
		INNER JOIN CDODEFINITION CD ON C.CDOTypeId = CD.CDODefId
		LEFT JOIN ROLEDEF R ON C.RoleId = R.RoleId )
/
BEGIN
   DROP_DATABASE_OBJECT ( 'OTAB_EVENTCAPA', 'TYPE' );
END;
/
CREATE OR REPLACE TYPE OTYP_EVENTCAPA AS OBJECT
	( OBJECTNAME			VARCHAR2(30)
	 ,OBJECTID			VARCHAR2(30)
	 ,OWNERNAME			VARCHAR2(256)
	 ,OWNERORGNAME			VARCHAR2(256)
	 ,PRIORITYLEVELNAME		VARCHAR2(256)
	 ,DISCOVERYAREA			VARCHAR2(256)
	 ,REPORTEDDATE			DATE
	 ,REPORTEDDATEGMT		DATE
	 ,CATEGORY			INTEGER
	 ,CATEGORYNAME			VARCHAR2(256)
	 ,CLASSIFICATIONID		VARCHAR2(30)
	 ,CLASSIFICATIONNAME	VARCHAR2(256)
	 ,SUBCLASSIFICATIONID	VARCHAR2(30)
	 ,SUBCLASSIFICATIONNAME	VARCHAR2(256)
	 ,CDONAME			VARCHAR2(256)
	 ,STATUS			INTEGER
	 ,STATUSNAME			VARCHAR2(256)
	 ,ISCARREQUIREDTOCLOSE		INTEGER
	 ,ROLENAME			VARCHAR(256)
	 ,TRIAGECOMPLETE		INTEGER )
/

CREATE OR REPLACE TYPE OTAB_EVENTCAPA AS TABLE OF OTYP_EVENTCAPA
/
BEGIN
   DROP_DATABASE_OBJECT ( 'CDOFIELDS_NUI1', 'INDEX' );
END;
/

CREATE INDEX CDOFIELDS_NUI1 ON CDOFIELDS (CDODefId) COMPUTE STATISTICS
/
BEGIN
   DROP_DATABASE_OBJECT ( 'CSI_QUERY_LOG', 'TABLE' );
END;
/
CREATE TABLE CSI_QUERY_LOG ( Query_Name		VARCHAR2(256) DEFAULT 'SelectionValueEx_QualityObjectInquiry_QualityObject'
			    ,Message_Text	CLOB
			    ,Log_Date		TIMESTAMP DEFAULT SYSDATE )
/

CREATE OR REPLACE PACKAGE CSI_QUALITY_OBJECT_INQUIRY
AS
	--
	TYPE t_StringTableType IS TABLE OF VARCHAR2(32767);
	--
	TYPE r_WhereClauseColumns IS RECORD
	( Name			VARCHAR2(256)
	 ,Value			VARCHAR2(256)
	 ,DataType		VARCHAR2(256) );
	--
	TYPE t_WhereClauseColumns IS TABLE OF r_WhereClauseColumns
	INDEX BY BINARY_INTEGER;
	--
	tt_WhereClauseColumns    	t_WhereClauseColumns;
	--
	-- Helper function for SELECT statement
	--
	FUNCTION EVENT_CAPA ( p_ObjectName		IN VARCHAR2
			     ,p_ObjectId		IN VARCHAR2
			     ,p_OwnerId			IN VARCHAR2
			     ,p_OwnerOrgId		IN VARCHAR2
			     ,p_PriorityLevelId		IN VARCHAR2
			     ,p_DiscoveryArea		IN VARCHAR2
			     ,p_ReportFromGMT		IN DATE	DEFAULT '1900-01-01'
			     ,p_ReportToGMT		IN DATE DEFAULT TRUNC(SYSDATE + 1)
			     ,p_Category		IN INTEGER
			     ,p_ClassificationId	IN VARCHAR2
			     ,p_SubClassificationId	IN VARCHAR2
			     ,p_Status			IN INTEGER
			     ,p_InitiatorId		IN VARCHAR2
			     ,p_InitiatorOrgId		IN VARCHAR2
			     ,p_ReporterId		IN VARCHAR2
			     ,p_ReporterOrgId		IN VARCHAR2
			     ,p_RoleId			IN VARCHAR2
			     ,p_IncludeAllRoles  	IN INTEGER DEFAULT 0
			     ,p_EmployeeLoginId  	IN VARCHAR2
			     ,p_ProcessDebugMode 	IN INTEGER DEFAULT 0
			     ,p_QueryName		IN VARCHAR2 DEFAULT 'SelectionValueEx_QualityObjectInquiry_QualityObject' )
	 RETURN otab_EventCAPA
	 PIPELINED;
	--
	--
END CSI_QUALITY_OBJECT_INQUIRY;
/

CREATE OR REPLACE PACKAGE BODY CSI_QUALITY_OBJECT_INQUIRY
AS
	--
	tt_WhereClauseValues			t_StringTableType := t_StringTableType();
	--
	TYPE t_EventCAPA IS TABLE OF EVENT_CAPA_V%ROWTYPE;
	tt_EventCAPACache			t_EventCAPA := t_EventCAPA();
	--
	gv_WhereClause				VARCHAR2(2048) := NULL;
	gV_ReportFromGMT			VARCHAR2(30) := '1900-01-01 00:00:00';
	gv_ReportToGMT				VARCHAR2(30) := TO_CHAR(TRUNC(SYSDATE + 1),'YYYY-MM-DD HH24:MI:SS');
	--
	gb_ProcessDebugMode			BOOLEAN := FALSE;
	--
	PROCEDURE LOG_MESSAGE ( p_MessageText		IN VARCHAR2 )
	IS
	--
	PRAGMA AUTONOMOUS_TRANSACTION;
	--
	BEGIN
		--
		IF ( gb_ProcessDebugMode )
		THEN
			--
			INSERT INTO CSI_QUERY_LOG ( Message_Text )
			VALUES ( p_MessageText );
			--
			COMMIT;
			--
		END IF;
		--
	EXCEPTION
		WHEN OTHERS THEN
			--
			DBMS_OUTPUT.PUT_LINE('Error logging messages - ErrMsg: '||SQLERRM );
			--
	END;
	--
	PROCEDURE TRUNCATE_TABLE ( p_TableName		IN VARCHAR2 )
	IS
	--
	PRAGMA AUTONOMOUS_TRANSACTION;
	--
	BEGIN
		--
		EXECUTE IMMEDIATE 'TRUNCATE TABLE '||p_TableName||' DROP STORAGE';
		--
	EXCEPTION
		WHEN OTHERS THEN
			--
			DBMS_OUTPUT.PUT_LINE('Error truncating DEBUG table - ErrMsg: '||SQLERRM );
			--
	END;
	--
	--
	FUNCTION STRING_TO_ARRAY ( p_InputString		IN VARCHAR2
				  ,p_Delimiter			IN VARCHAR2 DEFAULT ','
				  ,p_OutputArray		OUT NOCOPY t_StringTableType )
	RETURN BOOLEAN
	IS
	--
	-------------------------------------------------------------------------
	-- This function converts a delimited string into an array
	-- 
	--
	-- Modification History:
	-- Name				Date		Action
	-- --------------------------  ----------	----------------
	-- Purushotham Neelakantachar	08/12/2009	Initial Creation
	--
	--
	-- Copyright Siemens 2023  
	-------------------------------------------------------------------------
	--
	n_ErrLocator			NUMBER;
	--
	i_RowCount			PLS_INTEGER := 1;
	i_DelimiterPosition		PLS_INTEGER := 0;
	i_DelimiterLength		PLS_INTEGER := 0;
	--
	v_InputString			VARCHAR2(1024) := p_InputString;
	--
	BEGIN
		--
		n_ErrLocator := 5;
		--
		i_DelimiterPosition := INSTR(v_InputString,p_Delimiter);
		i_DelimiterLength := LENGTH(p_Delimiter);
		--
		p_OutputArray := t_StringTableType();
		--
		n_ErrLocator := 10;
		--
		WHILE i_DelimiterPosition > 0 
		LOOP
			--
			p_OutputArray.EXTEND;
			p_OutputArray(p_OutputArray.LAST) := SUBSTR(v_InputString,1,i_DelimiterPosition - 1);
			--
			v_InputString := SUBSTR(v_InputString,i_DelimiterPosition + i_DelimiterLength);
			--
			i_DelimiterPosition := INSTR(v_InputString,p_Delimiter);
			i_RowCount := i_RowCount + 1;
			--
		END LOOP;
		--
		p_OutputArray.EXTEND;
		p_OutputArray(p_OutputArray.LAST) := v_InputString;
		--
		i_RowCount := i_RowCount + 1;
		--
		LOG_MESSAGE('STRING_TO_ARRAY Count: '||p_OutputArray.COUNT);
		--
		RETURN ( TRUE );
		--
	EXCEPTION
		WHEN OTHERS THEN
			--
			LOG_MESSAGE('CSI_ENTERPRISE_UTIL.STRING_TO_ARRAY - OTHERS Exception - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
			--
			RAISE_APPLICATION_ERROR ( -20001,'CSI_ENTERPRISE_UTIL.STRING_TO_ARRAY failed - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
			--
	END;
	--
	--
	--
	FUNCTION BUILD_WHERE_CLAUSE ( p_TableName		IN VARCHAR2
				     ,p_ErrorMessage		OUT NOCOPY VARCHAR2 )
	RETURN BOOLEAN
	IS
	-------------------------------------------------------------------------
	-- This function builds the WHERE clause based on the non-NULL input
	-- parameters to the CSI_QUALITY_OBJECT_INQUIRY function.
	-- 
	--
	-- Modification History:
	-- Name				Date		Action
	-- --------------------------  ----------	----------------
	-- Purushotham Neelakantachar	08/12/2009	Initial Creation
	-- Oleg Kirasov					10/07/2009	Reworked the WHERE CLAUSE building - S16069
	--
	--
	-- Copyright Siemens 2023  
	-------------------------------------------------------------------------
	--
	i_Index				        INTEGER := 1;
	i_NullParameterCount		INTEGER := 0;
	--
	n_ErrLocator			    NUMBER;
	--
	v_WhereClauseRole		    VARCHAR2(1024) := NULL;
	v_WhereClauseOwner		    VARCHAR2(1024) := NULL;
	v_WhereClauseAllowedOwners	VARCHAR2(1024) := NULL;
	v_WhereClauseLogin		    VARCHAR2(1024) := NULL;
	--
	b_RoleIdFilterIsNull		BOOLEAN := FALSE;
	b_OwnerIdFilterIsNull		BOOLEAN := FALSE;
	b_IncludeAllRoles   		BOOLEAN := FALSE;
	--
	BEGIN
		--
		tt_WhereClauseColumns.DELETE;
		--
		gv_WhereClause := 'WHERE ';
		--
		n_ErrLocator := 5;
		--
		FOR CurrRow IN ( SELECT Column_Name,Data_Type
				   FROM USER_TAB_COLS
				  WHERE Table_Name = p_TableName )
		LOOP
			--
			-- Build the WHERE clause based on the input filter columns. Exclude all
			-- the columns that will be returned back to the client.
			--
			IF ( CurrRow.Column_Name NOT IN ( 'OWNERNAME'
							 ,'OWNERORGNAME'
							 ,'PRIORITYLEVELNAME'
							 ,'REPORTEDDATE'
							 ,'CATEGORYNAME'
							 ,'CLASSIFICATIONNAME'
							 ,'SUBCLASSIFICATIONNAME'
							 ,'CDONAME'
							 ,'INITIATORNAME'
							 ,'INITIATORORGNAME'
							 ,'REPORTERNAME'
							 ,'REPORTERORGNAME'
							 ,'STATUSNAME'
							 ,'ISCARREQUIREDTOCLOSE'
							 ,'ROLENAME'
							 ,'TRIAGECOMPLETE' ) )
			THEN
				--
				n_ErrLocator := 10;
				--
				tt_WhereClauseColumns(i_Index).Name := CurrRow.Column_Name;
				--
				LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Name: '||tt_WhereClauseColumns(i_Index).Name||' Index: '||i_Index||' Value(1): '||tt_WhereClauseValues(i_Index));
				--
				-- Massage the input filter data as appropriate
				--
				tt_WhereClauseColumns(i_Index).Value := CASE (tt_WhereClauseValues(i_Index))
										--
										WHEN Q'('')'
										THEN NULL
										WHEN '-1'
										THEN NULL
										WHEN Q'('|')'
										THEN NULL
										ELSE
											--
											CASE (CurrRow.Data_Type)
												WHEN 'DATE'
												THEN CASE (CurrRow.Column_Name)
													WHEN 'REPORTEDDATEGMT'
													THEN 'BETWEEN '||'TO_DATE('''||gv_ReportFromGMT||''','''||'YYYY-MM-DD HH24:MI:SS'||''')'||CHR(10)||
													     '			  AND '||'TO_DATE('''||gv_ReportToGMT||''','''||'YYYY-MM-DD HH24:MI:SS'||''')'
												     END
												     --
												WHEN 'VARCHAR2'
												THEN CASE (CurrRow.Column_Name)
													WHEN 'OBJECTNAME'
													THEN 'LIKE '||tt_WhereClauseValues(i_Index)
													ELSE tt_WhereClauseValues(i_Index)
												     END
												     --
												ELSE tt_WhereClauseValues(i_Index)
											END
											--
										--
									END;
									--
				--
				LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Name: '||tt_WhereClauseColumns(i_Index).Name||' Index: '||i_Index||' Value(2): '||tt_WhereClauseColumns(i_Index).Value);
				--
				n_ErrLocator := 12;
				--
				tt_WhereClauseColumns(i_Index).DataType := CurrRow.Data_Type;
				--
				IF ( NVL(tt_WhereClauseColumns(i_Index).Value,'XXX') <> 'XXX' )
				THEN
					--
					IF ( tt_WhereClauseColumns(i_Index).Value = Q'('NULL')' )
					THEN
    					--
    					IF ( gv_WhereClause <> 'WHERE ' )
    					THEN
							--
							gv_WhereClause := gv_WhereClause||CHR(10)||'  AND ';
							--
                        END IF;    
						--
						gv_WhereClause := gv_WhereClause||tt_WhereClauseColumns(i_Index).Name||' IS NULL';
                    END IF;    
					--
					IF ( tt_WhereClauseColumns(i_Index).Name NOT IN ( 'ROLEID'
											 ,'OWNERID'
											 ,'TRIAGECOMPLETE'
											 ,'INCLUDEALLROLES'
											 ,'LOGINID' ) )
					THEN
    					IF ( gv_WhereClause <> 'WHERE ' )
    					THEN
							--
							gv_WhereClause := gv_WhereClause||CHR(10)||'  AND ';
							--
                        END IF;    
						--
						gv_WhereClause := gv_WhereClause||tt_WhereClauseColumns(i_Index).Name||CASE ( CurrRow.Column_Name )
															WHEN 'OBJECTNAME'
															THEN ' '
															WHEN 'REPORTEDDATEGMT'
															THEN ' '
															ELSE ' = '
														 END ||tt_WhereClauseColumns(i_Index).Value;
						--
					END IF; -- End IF ( tt_WhereClauseColumns(i_Index).Name NOT IN ( 'ROLEID'
					--
					-- Now, build the WHERE/AND clauses based on Roles
					--
					IF ( tt_WhereClauseColumns(i_Index).Name = 'ROLEID' )
					THEN
						--
						v_WhereClauseRole := '  AND ( RoleId = '||tt_WhereClauseColumns(i_Index).Value||') ';
						--
					ELSIF ( tt_WhereClauseColumns(i_Index).Name = 'OWNERID' )
					THEN
						--
						v_WhereClauseOwner := '  AND ( OwnerId = '||tt_WhereClauseColumns(i_Index).Value||') ';
						v_WhereClauseAllowedOwners := '	AND ( '||tt_WhereClauseColumns(i_Index).Value||' IN ( SELECT EmployeeId '||CHR(10)||
								       '			FROM TABLE(CSIAUTHGETALLOWEDUSERS(OwnerOrgName,RoleName)) ) )';
						--
					ELSIF ( tt_WhereClauseColumns(i_Index).Name = 'INCLUDEALLROLES' 
                              AND tt_WhereClauseColumns(i_Index).Value = '1')
					THEN
						--
                        b_IncludeAllRoles := TRUE;
						--
					ELSIF ( tt_WhereClauseColumns(i_Index).Name = 'LOGINID' )
					THEN
						--
						v_WhereClauseLogin := '  AND ( '||tt_WhereClauseColumns(i_Index).Value||' IN ( SELECT EmployeeId '||CHR(10)||
								       '			FROM TABLE(CSIAUTHGETALLOWEDUSERS(OwnerOrgName,NULL)) ) ) ';
						--
					END IF; -- End IF ( tt_WhereClauseColumns(i_Index).Name = 'ROLEID' )

				ELSE
					--
					i_NullParameterCount := i_NullParameterCount + 1;
					--
					-- Flag if OwnerId and/or RoleId filters are NULL
					-- This will help in building WHERE/AND clauses based
					-- on Roles
					--
					IF ( NVL(tt_WhereClauseColumns(i_Index).Value,'XXX') = 'XXX' )
					THEN
						--
						IF ( tt_WhereClauseColumns(i_Index).Name = 'OWNERID' )
						THEN
							--
							b_OwnerIdFilterIsNull := TRUE;
							--
						ELSIF ( tt_WhereClauseColumns(i_Index).Name = 'ROLEID' )
						THEN
							--
							b_RoleIdFilterIsNull := TRUE;
							--
						END IF; -- End IF ( tt_WhereClauseColumns(i_Index).Name = 'OWNERID' )
						--
					END IF; -- End IF ( NVL(tt_WhereClauseColumns(i_Index).Value,'XXX') = 'XXX' )
					--
				END IF; -- ENd IF ( NVL(tt_WhereClauseColumns(i_Index).Value,'XXX') <> 'XXX' )
				i_Index := i_Index + 1;
				--
				LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - CurrentIndex: '||i_Index);
				LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - WhereClause: '||gv_WhereClause);
				--
			END IF; -- End IF ( CurrRow.Column_Name NOT IN ( 'OBJECTNAME'
			--
		END LOOP;
		--
		-- Define v_WhereClauseBasedOnRoles
		--
		IF ( b_IncludeAllRoles )
		THEN
			--
			gv_WhereClause := gv_WhereClause||CHR(10)||v_WhereClauseAllowedOwners;
			--
        ELSE
		    IF ( NOT (b_RoleIdFilterIsNull) )
		    THEN
    			--
    			gv_WhereClause := gv_WhereClause||CHR(10)||v_WhereClauseRole||CHR(10)||v_WhereClauseAllowedOwners;
    			--
            END IF;    		
            IF ( NOT (b_OwnerIdFilterIsNull) )
		    THEN
			    --
			    gv_WhereClause := gv_WhereClause||CHR(10)||v_WhereClauseOwner;
			    --
            END IF;    		
        END IF; 
		--
		gv_WhereClause := gv_WhereClause||CHR(10)||v_WhereClauseLogin;
		--
		LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Parameter Values Count (NULLs): '||i_NullParameterCount);
		LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Parameter Column Count: '||tt_WhereClauseColumns.COUNT);
		LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Final Where Clause: '||CHR(10)||gv_WhereClause);
		--
		p_ErrorMessage := 'CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Successful';
		RETURN ( TRUE );
		--
	EXCEPTION
		WHEN OTHERS THEN
			--
			tt_WhereClauseColumns.DELETE;
			tt_WhereClauseValues.DELETE;
			--
			LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Unable to build WHERE clause - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
			--
			p_ErrorMessage := 'CSI_QUALITY_OBJECT_INQUIRY.BUILD_WHERE_CLAUSE - Unable to build WHERE clause - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM;
			RETURN ( FALSE );
			--
	END;
	--
	-- Helper function for SELECT statement
	--
	FUNCTION EVENT_CAPA ( p_ObjectName		IN VARCHAR2
			     ,p_ObjectId		IN VARCHAR2
			     ,p_OwnerId			IN VARCHAR2
			     ,p_OwnerOrgId		IN VARCHAR2
			     ,p_PriorityLevelId		IN VARCHAR2
			     ,p_DiscoveryArea		IN VARCHAR2
			     ,p_ReportFromGMT		IN DATE	DEFAULT '1900-01-01'
			     ,p_ReportToGMT		IN DATE DEFAULT TRUNC(SYSDATE + 1)
			     ,p_Category		IN INTEGER
			     ,p_ClassificationId	IN VARCHAR2
			     ,p_SubClassificationId	IN VARCHAR2
			     ,p_Status			IN INTEGER
			     ,p_InitiatorId		IN VARCHAR2
			     ,p_InitiatorOrgId		IN VARCHAR2
			     ,p_ReporterId		IN VARCHAR2
			     ,p_ReporterOrgId		IN VARCHAR2
			     ,p_RoleId			IN VARCHAR2
			     ,p_IncludeAllRoles  	IN INTEGER DEFAULT 0
			     ,p_EmployeeLoginId  	IN VARCHAR2
			     ,p_ProcessDebugMode 	IN INTEGER DEFAULT 0
			     ,p_QueryName		IN VARCHAR2 DEFAULT 'SelectionValueEx_QualityObjectInquiry_QualityObject' )
	 RETURN otab_EventCAPA
	 PIPELINED

	IS
	-------------------------------------------------------------------------
	-- This function pipes the rows from the local cache to Quality Object
	-- Inquiry collections
	--
	-- -1 equates to NULL for all INTEGER columns
	--
	-- Modification History:
	-- Name				Date		Action
	-- --------------------------  ----------	----------------
	-- Purushotham Neelakantachar	08/12/2009	Initial Creation
	--
	--
	-- Copyright Siemens 2023  
	-------------------------------------------------------------------------
	--
	v_SQLStmt1			VARCHAR2(1024);
	v_SQLStmt2			VARCHAR2(1024);
	v_FinalSQLStmt			VARCHAR2(32767);
	v_WhereClauseValues		VARCHAR2(1024);  
	v_ErrMsg			VARCHAR2(1024);
	--
	i_RowCount			INTEGER := 0;
	--
	rc_EventCAPA			SYS_REFCURSOR;
	--
	n_ErrLocator			NUMBER;
	--
	e_StringToArray			EXCEPTION;
	e_BuildWhereClause		EXCEPTION;
	--
	BEGIN
		--
		IF ( p_ProcessDebugMode = 1 )
		THEN
			--
			n_ErrLocator := 5;
			--
			TRUNCATE_TABLE('CSI_QUERY_LOG');
			--
			gb_ProcessDebugMode := TRUE;
			--
		END IF;
		--
		n_ErrLocator := 10;
		--
		IF ( p_ReportFromGMT IS NOT NULL )
		THEN
			BEGIN
				--
				gV_ReportFromGMT := TO_CHAR(TRUNC(p_ReportFromGMT),'YYYY-MM-DD HH24:MI:SS');
				--
			END;
			--
		END IF;
		--
		n_ErrLocator := 15;
		--
		IF ( p_ReportToGMT IS NOT NULL )
		THEN
			--
			gV_ReportToGMT := TO_CHAR(TRUNC(p_ReportToGMT+1),'YYYY-MM-DD HH24:MI:SS');
			--
		END IF;
		--
		n_ErrLocator := 20;
		--
		v_SQLStmt1 := 'SELECT ObjectName '||CHR(10)||
			      '      ,ObjectId '||CHR(10)||
			      '      ,OwnerName '||CHR(10)||
			      '      ,OwnerOrgName'||CHR(10)||
			      '      ,OwnerId '||CHR(10)||
			      '      ,OwnerOrgID'||CHR(10)||
			      '      ,PriorityLevelName '||CHR(10)||
			      '      ,PriorityLevelId '||CHR(10)||
			      '      ,DiscoveryArea '||CHR(10)||
			      '      ,ReportedDate '||CHR(10)||
			      '      ,ReportedDateGMT '||CHR(10)||
			      '      ,Category '||CHR(10)||
			      '      ,CategoryName '||CHR(10)||
			      '      ,ClassificationId '||CHR(10)||
			      '      ,ClassificationName '||CHR(10)||
			      '      ,SubClassificationId '||CHR(10)||
			      '      ,SubClassificationName '||CHR(10)||
			      '      ,CDOName '||CHR(10)||
			      '      ,Status '||CHR(10)||
			      '      ,InitiatorName '||CHR(10)||
			      '      ,InitiatorOrgName '||CHR(10)||
			      '      ,InitiatorId '||CHR(10)||
			      '      ,InitiatorOrgId '||CHR(10)||
			      '      ,ReporterName '||CHR(10)||
			      '      ,ReporterOrgName '||CHR(10)||
			      '      ,ReporterId '||CHR(10)||
			      '      ,ReporterOrgId '||CHR(10)||
			      '      ,StatusName '||CHR(10)||
			      '      ,IsCARRequiredToClose'||CHR(10)||
			      '      ,RoleId'||CHR(10)||
			      '      ,RoleName '||CHR(10)||
			      '      ,LoginId '||CHR(10)||
			      '      ,TriageComplete '||CHR(10)||
			      '      ,IncludeAllRoles '||CHR(10)||
			      '  FROM EVENT_CAPA_V';
		--
		v_WhereClauseValues := ''''||p_ObjectName||''','''||p_ObjectId||''','''||p_OwnerId||''','''||p_OwnerOrgId||''','''||p_PriorityLevelId||''','''||p_DiscoveryArea||''','''||p_ReportFromGMT
					   ||'|'||p_ReportToGMT||''','||p_Category||','||p_ClassificationId||','||p_SubClassificationId||','
					   ||p_Status||','''||p_InitiatorId||''','''||p_InitiatorOrgId||''','''||p_ReporterId||''','''||p_ReporterOrgId||''','''||p_RoleId
					   ||''','''||p_EmployeeLoginId||''','||p_IncludeAllRoles;
		--
		LOG_MESSAGE('Input Parameters Comma Delimited String: '||v_WhereClauseValues);
		--
		tt_WhereClauseValues := t_StringTableType();
		--
		n_ErrLocator := 25;
		--
		IF ( STRING_TO_ARRAY ( p_InputString => v_WhereClauseValues
				      ,p_Delimiter => ','
				      ,p_OutputArray => tt_WhereClauseValues ) )
		THEN
			--
			LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Parameter Values Count (Total): '||tt_WhereClauseValues.COUNT);
			--
			n_ErrLocator := 30;
			--
			IF ( BUILD_WHERE_CLAUSE ( 'EVENT_CAPA_V'
						 ,V_ErrMsg  ) )
			THEN
				--
				v_FinalSQLStmt := v_SQLStmt1||CHR(10)||gv_WhereClause;
				--
				LOG_MESSAGE('Final SQL Statement: '||CHR(10)||v_FinalSQLStmt);
				--
				tt_EventCAPACache.DELETE;
				--
				n_ErrLocator := 35;
				--
				OPEN rc_EventCAPA FOR v_FinalSQLStmt;
				LOOP
					--
					n_ErrLocator := 36;
					--
					FETCH rc_EventCAPA BULK COLLECT INTO tt_EventCAPACache LIMIT 100;
					--
					i_RowCount := i_RowCount + tt_EventCAPACache.COUNT;
					--
					IF ( tt_EventCAPACache.COUNT > 0 )
					THEN
						--
						n_ErrLocator := 40;
						--
						FOR CurrIndex IN tt_EventCAPACache.FIRST .. tt_EventCAPACache.LAST
						LOOP
							--
							PIPE ROW( otyp_EventCAPA ( tt_EventCAPACache(CurrIndex).OBJECTNAME
										  ,tt_EventCAPACache(CurrIndex).OBJECTID
										  ,tt_EventCAPACache(CurrIndex).OWNERNAME
										  ,tt_EventCAPACache(CurrIndex).OWNERORGNAME
										  ,tt_EventCAPACache(CurrIndex).PRIORITYLEVELNAME
										  ,tt_EventCAPACache(CurrIndex).DISCOVERYAREA
										  ,tt_EventCAPACache(CurrIndex).REPORTEDDATE
										  ,tt_EventCAPACache(CurrIndex).REPORTEDDATEGMT
										  ,tt_EventCAPACache(CurrIndex).CATEGORY
										  ,tt_EventCAPACache(CurrIndex).CATEGORYNAME
										  ,tt_EventCAPACache(CurrIndex).CLASSIFICATIONID
										  ,tt_EventCAPACache(CurrIndex).CLASSIFICATIONNAME
										  ,tt_EventCAPACache(CurrIndex).SUBCLASSIFICATIONID
										  ,tt_EventCAPACache(CurrIndex).SUBCLASSIFICATIONNAME
										  ,tt_EventCAPACache(CurrIndex).CDONAME
										  ,tt_EventCAPACache(CurrIndex).STATUS
										  ,tt_EventCAPACache(CurrIndex).STATUSNAME
										  ,tt_EventCAPACache(CurrIndex).ISCARREQUIREDTOCLOSE
										  ,tt_EventCAPACache(CurrIndex).ROLENAME
										  ,tt_EventCAPACache(CurrIndex).TRIAGECOMPLETE ) );
							--
						END LOOP;
						--
					END IF;	-- End IF ( tt_EventCAPACache.COUNT > 0 )
					--
					IF ( rc_EventCAPA%NOTFOUND )
					THEN
						--
						LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - ObjectQualityInquiry Total Cache Count: '||i_RowCount);
						--
						EXIT;
						--
					END IF;
					--
				END LOOP;
				CLOSE rc_EventCAPA;
				--
			ELSE
				--
				RAISE e_BuildWhereClause;
				--
			END IF;
			--
		ELSE
			--
			RAISE e_StringToArray;
			--
		END IF;
		--
		RETURN;
		--
	EXCEPTION
		WHEN e_StringToArray THEN
			--
			tt_WhereClauseColumns.DELETE;
			tt_WhereClauseValues.DELETE;
			tt_EventCAPACache.DELETE;
			--
			LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed - ErrLoc: '||n_ErrLocator);
			--
			RAISE_APPLICATION_ERROR ( -20002,'CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed - ErrLoc: '||n_ErrLocator);

		WHEN e_BuildWhereClause THEN
			--
			IF ( rc_EventCAPA%ISOPEN )
			THEN
				--
				CLOSE rc_EventCAPA;
				--
			END IF;
			--
			tt_WhereClauseColumns.DELETE;
			tt_WhereClauseValues.DELETE;
			tt_EventCAPACache.DELETE;
			--
			LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed - ErrLoc: '||n_ErrLocator||' ErrMsg: '||v_ErrMsg);
			--
			RAISE_APPLICATION_ERROR ( -20003,'CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed - ErrLoc: '||n_ErrLocator||' ErrMsg: '||v_ErrMsg);

		WHEN OTHERS THEN
			--
			IF ( rc_EventCAPA%ISOPEN )
			THEN
				--
				CLOSE rc_EventCAPA;
				--
			END IF;
			--
			tt_WhereClauseColumns.DELETE;
			tt_WhereClauseValues.DELETE;
			tt_EventCAPACache.DELETE;
			--
			LOG_MESSAGE('CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed: '||SQLERRM);
			--
			RAISE_APPLICATION_ERROR ( -20004,'CSI_QUALITY_OBJECT_INQUIRY.EVENT_CAPA - Failed - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
	END;
	--
	--
END;
/