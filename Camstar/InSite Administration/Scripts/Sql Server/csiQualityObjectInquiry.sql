-- Copyright Siemens 2023  
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'QualityObject'
	      AND Type = 'V')
	--
	DROP VIEW QualityObject
	--
GO

CREATE View QualityObject ( QualityObjectType
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

GO

IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'ParentObject'
	      AND Type = 'V')
	--
	DROP VIEW ParentObject
	--
GO

CREATE View ParentObject ( Id
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
GO


IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'csiLabelView'
	      AND Type = 'V')
	--
	DROP VIEW csiLabelView
	--
GO

CREATE VIEW csiLabelView ( LabelID
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
GO


IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'csiActivityView'
	      AND Type = 'V')
	--
	DROP VIEW csiActivityView
	--
GO


CREATE VIEW csiActivityView ( ActivityId
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
GO

IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'TODOListEntries'
	      AND Type = 'V')
	--
	DROP VIEW TODOListEntries
	--
GO

-- Notes have been added to the following enums to indicate they are used by this view.
--    ApprovalStatusEnum
--    AssigneeOptionEnum
--    NotificationTypeEnum
--    QualityStatusEnum
--    StageEnum
--    ToDoListEntryTypeEnum
CREATE View TODOListEntries ( Identifier
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
	,2 AS "NotificationType"	-- ProcessModelAssignment
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
			INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
   WHERE ProcessModel.Stage IN (45, 25, 20) -- (InProcess,InCollaboration,Pending)
   AND	 ProcessModel.Designated = 1
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
	,4 AS "NotificationType"	-- PlanAssignment
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
   WHERE ActivityPlan.Stage IN (45, 25, 20, 30) -- (InProcess,InCollaboration,Pending,InReview)
   AND	 ActivityPlan.Designated = 1
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
	,CASE Activity.LastStage WHEN 20 THEN 5 WHEN 50 THEN 29 ELSE 0 END AS "NotificationType"	-- (5=>ActivityAssignment, 29=>ActivityReprocessed)
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
			INNER		JOIN ProcessModel	ON Activity.ProcessModelId = ProcessModel.ProcessModelId
			INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
   WHERE Activity.Stage = 45 -- (InProcess)
   AND	 Activity.LastStage IN (20, 50)  -- (Pending, Completed)
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
	,CASE ParentObject.ParentType 
         WHEN 1 THEN 8  -- (8=>PlanApprovalAssignment)
         WHEN 2 THEN 6 ELSE 0 END AS "NotificationType"	-- (6=>QualityRecordApprovalAssignment)
	,CASE ParentObject.ParentType 
         WHEN 1 THEN 'ApprovalPlanEntry'
         ELSE 'ApprovalQualityObjectEntry' END AS "ToDoListItemType"	
	,QualityObject.Id AS "QualityObject"
    ,0 AS "DataCollectionDefined"
    ,ParentObject.Id AS "ApprovalSheetParent"
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
			INNER	JOIN ApprovalSheet Sheet ON Entry.ParentId = Sheet.ApprovalSheetId
			INNER	JOIN ParentObject		 ON Sheet.ParentId = ParentObject.Id
			LEFT OUTER  JOIN ActivityPlan   ON ParentObject.Id = ActivityPlan.PlanId
			LEFT OUTER	JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
			INNER		JOIN QualityObject	ON ParentObject.Id       = QualityObject.Id
											OR ProcessModel.ParentId = QualityObject.Id
   WHERE Entry.Status = 10
	 AND Sheet.Status = 10
	 AND ( (ParentObject.Stage = 30 AND ParentObject.ParentType = 1) 
           OR 
		   (ParentObject.Stage = 8 AND ParentObject.ParentType = 2 AND Sheet.ApprovalSheetName = 'FromCompleted')
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
	,11 AS "NotificationType"	-- Process Model End Collaboration
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
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
	,16 AS "NotificationType"	-- Process Model Completion
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
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
	,18 AS "NotificationType"	-- Plan End Collaboration
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
			INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
			LEFT	JOIN (SELECT ParentId FROM ActivityPlan 
				GROUP BY ParentId) Plan1 ON ProcessModel.ProcessModelId = Plan1.ParentId
			LEFT	JOIN (SELECT ParentId FROM ActivityPlan 
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
			INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
			LEFT	JOIN (SELECT ParentId FROM Activity 
				GROUP BY ParentId) Act1 ON ProcessModel.ProcessModelId = Act1.ParentId
			LEFT	JOIN (SELECT ParentId FROM Activity 
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
	,17 AS "NotificationType"	-- Process Model Auto Complete Failure
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
			INNER	JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
	,19 AS "NotificationType"	-- Plan Activity Completion
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
			LEFT	JOIN (SELECT ParentId FROM Activity 
				GROUP BY ParentId) Act1 ON ActivityPlan.PlanId = Act1.ParentId
			LEFT	JOIN (SELECT ParentId FROM Activity 
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
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
			INNER		JOIN ProcessModel	ON ActivityPlan.ParentId = ProcessModel.ProcessModelId
		    INNER		JOIN QualityObject	ON ProcessModel.ParentId = QualityObject.Id
   WHERE (
             (ActivityPlan.Stage = 50 AND ActivityPlan.LastStage = 30 AND ApprovalSheet.Status = 40)  -- Approved
          OR (ActivityPlan.Stage = 30 AND ActivityPlan.LastStage = 45 AND ApprovalSheet.Status = 30)  -- Rejected
         )
     AND ProcessModel.Stage <> 50  -- Completed
)
GO

IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'EVENT_CAPA_V'
	      AND Type = 'V')
	--
	DROP VIEW EVENT_CAPA_V
	--
GO

CREATE VIEW EVENT_CAPA_V ( ObjectName
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
	,LABELS.LabelValue 			AS StatusName
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
	,LABELS.LabelValue 			AS StatusName
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
GO

IF EXISTS (SELECT Name 
	     FROM SYSINDEXES
	    WHERE Name = 'CDOFIELDS_NUI1' )
	--
	DROP INDEX CDOFIELDS_NUI1 ON CDOFIELDS
	--
GO

CREATE INDEX CDOFIELDS_NUI1 ON CDOFIELDS (CDODefId)
GO

IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'csiQualityObjectInquiry_EventCAPA'
	      AND Type = 'TF')
	--
	DROP FUNCTION csiQualityObjectInquiry_EventCAPA
	--
GO

CREATE FUNCTION csiQualityObjectInquiry_EventCAPA ( @p_ObjectName			VARCHAR(256)
						   ,@p_ObjectId				VARCHAR(30)
						   ,@p_OwnerId				VARCHAR(30)
						   ,@p_OwnerOrgId			VARCHAR(30)
						   ,@p_PriorityLevelId			VARCHAR(30)
						   ,@p_DiscoveryArea			VARCHAR(256)
						   ,@p_ReportFromGMT			DATETIME
						   ,@p_ReportToGMT			DATETIME
						   ,@p_Category				INTEGER
						   ,@p_ClassificationId			VARCHAR(30)
						   ,@p_SubClassificationId		VARCHAR(30)
						   ,@p_Status				INTEGER
						   ,@p_InitiatorId			VARCHAR(30)
						   ,@p_InitiatorOrgId			VARCHAR(30)
						   ,@p_ReporterId			VARCHAR(30)
						   ,@p_ReporterOrgId			VARCHAR(30)
						   ,@p_RoleId				VARCHAR(30)
						   ,@p_IncludeAllRoles  		INTEGER = 0
						   ,@p_EmployeeLoginId  		VARCHAR(30)  )
RETURNS @tt_EventCAPA TABLE ( OBJECTNAME		VARCHAR(30)
			     ,OBJECTID			VARCHAR(30)
			     ,OWNERNAME			VARCHAR(256)
			     ,OWNERORGNAME		VARCHAR(256)
			     ,PRIORITYLEVELNAME		VARCHAR(256)
			     ,DISCOVERYAREA		VARCHAR(256)
			     ,REPORTEDDATE		DATETIME
			     ,REPORTEDDATEGMT		DATETIME
			     ,CATEGORY			INTEGER
			     ,CATEGORYNAME		VARCHAR(256)
			     ,CLASSIFICATIONID		VARCHAR(30)
			     ,CLASSIFICATIONNAME	VARCHAR(256)
			     ,SUBCLASSIFICATIONID	VARCHAR(30)
			     ,SUBCLASSIFICATIONNAME	VARCHAR(256)
			     ,CDONAME			VARCHAR(256)
			     ,STATUS			INTEGER
			     ,STATUSNAME		VARCHAR(256)
			     ,ISCARREQUIREDTOCLOSE	INTEGER
			     ,ROLENAME			VARCHAR(256)
			     ,TRIAGECOMPLETE		INTEGER ) 
AS
BEGIN
	-------------------------------------------------------------------------
	-- This section pipes the rows from the local cache to Quality Object
	-- Inquiry collections
	--
	-- -1 equates to NULL for all INTEGER columns
	--
	-- Modification History:
	-- Name				Date		Action
	-- --------------------------  ----------	----------------
	-- Purushotham Neelakantachar	08/17/2009	Initial Creation
	--
	--
	-- Copyright Siemens 2023  
	-------------------------------------------------------------------------
	--
	INSERT INTO @tt_EventCAPA
	SELECT ObjectName
	      ,ObjectId
	      ,OwnerName
	      ,OwnerOrgName
	      ,PriorityLevelName
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
	      ,StatusName
	      ,IsCARRequiredToClose
	      ,RoleName
	      ,TriageComplete
	  FROM EVENT_CAPA_V
	 WHERE (ObjectId = @p_ObjectId  OR @p_ObjectId  = '')
	   AND (OwnerOrgId = @p_OwnerOrgId OR @p_OwnerOrgId = '')
	   AND (PriorityLevelId = @p_PriorityLevelId OR @p_PriorityLevelId = '')
	   AND (DiscoveryArea = @p_DiscoveryArea OR @p_DiscoveryArea = '')
	   AND (ReportedDateGMT >= CASE ( ISNULL(CAST(@p_ReportFromGMT AS VARCHAR(30)),'XXX') )
					WHEN 'XXX'
					THEN CAST('1900-01-01 00:00:00' AS DATETIME)
					ELSE DATEADD(DD,0, DATEDIFF(DD,0,@p_ReportFromGMT)) 
					END OR @p_ReportFromGMT = '')
	   AND (ReportedDateGMT < CASE ( ISNULL(CAST(@p_ReportToGMT AS VARCHAR(30)),'XXX') )
					WHEN 'XXX'
					THEN DATEADD(DD,0, DATEDIFF(DD,0,GETDATE() + 1))
					ELSE DATEADD(DD,0, DATEDIFF(DD,0,@p_ReportToGMT + 1)) 
					END OR @p_ReportToGMT = '')
	   AND (Category = @p_Category OR @p_Category = -1)
	   AND (ClassificationId = @p_ClassificationId OR @p_ClassificationId = '')
   	   AND (SubClassificationId = @p_SubClassificationId OR @p_SubClassificationId = '')
	   AND (Status = @p_Status OR @p_Status = -1)
	   AND (InitiatorId = @p_InitiatorId OR @p_InitiatorId = '')
	   AND (InitiatorOrgId = @p_InitiatorOrgId  OR @p_InitiatorOrgId  = '')
	   AND (ReporterId = @p_ReporterId OR @p_ReporterId = '')
	   AND (ReporterOrgId = @p_ReporterOrgId OR @p_ReporterOrgId = '')
	   AND (ObjectName LIKE @p_ObjectName )
	   AND ((RoleId = @p_RoleId AND 
		 @p_OwnerId IN (SELECT EmployeeId FROM csiAuthGetAllowedUsers(OwnerOrgName, RoleName)) ) OR 
		@p_RoleId = '' )
	   AND (OwnerId = @p_OwnerId OR 
		@p_OwnerId = '' OR 
		@p_RoleId <> '' OR 
		@p_IncludeAllRoles = 1 )
	   AND (( @p_IncludeAllRoles = 1 AND 
		  @p_OwnerId IN (SELECT EmployeeId FROM csiAuthGetAllowedUsers(OwnerOrgName, RoleName)) ) OR
		  @p_IncludeAllRoles = 0 )
	   AND (@p_EmployeeLoginId IN (select EmployeeId from csiAuthGetAllowedUsers(OwnerOrgName,'')))
	ORDER BY ObjectName;
	--
	RETURN;
	--
END;
GO

/** Code for testing the above Table function
SELECT *
  FROM csiQualityObjectInquiry_EventCAPA ( NULL			-- ObjectName		-VARCHAR(256)
					  ,NULL			-- ObjectId		-VARCHAR(30)
					  ,NULL			-- OwnerId		-VARCHAR(30)
					  ,NULL			-- OwnerOrgId		-VARCHAR(30)
					  ,NULL			-- PriorityLevelId	-VARCHAR(30)
					  ,NULL			-- DiscoveryArea	-VARCHAR(256)
					  ,GETDATE() - 100	-- ReportFromGMT	-DATETIME
					  ,GETDATE() - 10	-- ReportToGMT		-DATETIME
					  ,-1			-- Category		-INTEGER
					  ,-1			-- Classification	-INTEGER
					  ,-1			-- SubClassification	-INTEGER
					  ,-1			-- Status		-INTEGER
					  ,NULL			-- InitiatorId		-VARCHAR(30)
					  ,NULL			-- InitiatorOrgId	-VARCHAR(30)
					  ,NULL			-- ReporterId		-VARCHAR(30)
					  ,NULL			-- ReporterOrgId	-VARCHAR(30)
					  ,NULL			-- RoleId		-VARCHAR(30)
					  ,0			-- IncludeAllRoles  	-INTEGER = 0
					  ,NULL )		-- EmployeeLoginId  	-VARCHAR(30)
					  
**/
