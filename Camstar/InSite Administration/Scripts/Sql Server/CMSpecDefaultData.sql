--------------------------------------------------------------------------------
-- SCRIPT:CMSpecDefaultData.sql
-- DESCR: Creates stored procedures used to create change management related
-- speecs, busniess rules, paths  and workflows. 
--Copyright Siemens 2025  

/****** Script for SelectTopNRows command from SSMS  ******/
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateSpec' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateSpec
GO
CREATE PROCEDURE csiCMCreateSpec(@StepIcon NVARCHAR(512), @RoleDescription NVARCHAR(255),@CMSpecName NVARCHAR(30), @InstanceId varchar(16) OUTPUT, @InstanceId1 varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CMSpecCDODefId INT
    SET @CMSpecCDODefId=8513
    
    DECLARE @CMSpecBaseCDODefId INT
    SET @CMSpecBaseCDODefId=8514

	DECLARE @CMWrkFlowCDODefId INT
    SET @CMWrkFlowCDODefId=8517
    
    DECLARE @CMWrkFlowBaseCDODefId INT
    SET @CMWrkFlowBaseCDODefId=8518
    
    DECLARE @CMFieldRoleDefId INT
    SET @CMFieldRoleDefId=22504
    
    DECLARE @RoleId VARCHAR(50)

    EXEC csiPRDGetNextInstanceId @CMSpecCDODefId,@InstanceId OUTPUT
    EXEC csiPRDGetNextInstanceId @CMSpecBaseCDODefId,@InstanceId1 OUTPUT
  
    
   If @CMSpecName = 'Draft Camstar'
           INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
           VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',1,1,0,0,1)
    else if @CMSpecName = 'Draft PLM'
           INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
           VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,1,0,0,1)
    else if @CMSpecName = 'Draft'
           INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
           VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,0,0,1)
    else if @CMSpecName = 'Pending Approval Camstar'
           INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
           VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1,1)
    else if @CMSpecName = 'Pending Approval PLM'
           INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
           VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1,1)
	else if @CMSpecName = 'Pending Deployment'
			INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
            VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,1)
	else if @CMSpecName = 'Deployment Incomplete'
			INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
            VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,2)
	else if @CMSpecName = 'Deployment Complete'
			INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
            VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,2)
	else if @CMSpecName = 'Rejected'
			INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
            VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,3)
    else 
            INSERT INTO [BusinessProcessSpec]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
            VALUES
           (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
           ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,0)
           
           
           
     INSERT INTO [BusinessProcessSpecBase]
           ([BusinessProcessSpecBaseId],[BusinessProcessSpecName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
     VALUES
           (@InstanceId1,@CMSpecName,@CMSpecBaseCDODefId,1,NULL,@InstanceId)
           
     print @CMSpecName
     select @RoleId = RoleId from [RoleDef] where Description like @CMSpecName + '%'
     if @CMSpecName ='Draft'
     select @RoleId = RoleId from [RoleDef] where RoleName = 'DraftPermissions'
     
     INSERT INTO [ChangeMgtSpecAllowableRoles]
           ([AllowableRolesId],[BusinessProcessSpecId],[FieldId],[Sequence])
     VALUES
           (@RoleId,@InstanceId,@CMFieldRoleDefId,1)
          
END
GO
           
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateWorkFlow' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateWorkFlow
GO
CREATE PROCEDURE csiCMCreateWorkFlow(@WFDescription NVARCHAR(255),@WFName NVARCHAR(30), @InstanceId2 varchar(16) OUTPUT, @InstanceId3 varchar(16) OUTPUT, @InstanceId4 varchar(16) OUTPUT, @InstanceId5 varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CMWrkFlowCDODefId INT
    SET @CMWrkFlowCDODefId=8517
    
    DECLARE @CMWrkFlowBaseCDODefId INT
    SET @CMWrkFlowBaseCDODefId=8518
    
    DECLARE @CMWrkFlowStepCDODefId INT
    SET @CMWrkFlowStepCDODefId=8578
    
    DECLARE @CMPathCDODefId INT
    SET @CMPathCDODefId=1440
    
   DECLARE @CMPathSelectorCDODefId INT
    DECLARE @CMPathSelectorInstanceId varchar(16)
    SET @CMPathSelectorCDODefId=1840
    

	DECLARE @CMDRAFT CHAR(16), @CMDI CHAR(16), @CMDC CHAR(16), @CMCL CHAR(16), @SpecId CHAR(16), @CMPD CHAR(16), @SpecDescription NVARCHAR(255)
    
    EXEC csiPRDGetNextInstanceId @CMWrkFlowBaseCDODefId,@InstanceId2 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowCDODefId,@InstanceId3 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
           
    INSERT INTO [BusinessProcessWorkflow]
           ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
     VALUES
           (@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
           
           
    INSERT INTO [BusinessProcessWorkflowBase]
           ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
    VALUES(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
     
     
    SET @CMDRAFT = @InstanceId4;
	
	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Draft'
	   and BusinessProcessSpec.Revision = '1'	
	 
    select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId
    INSERT INTO [WorkflowStep]
    ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('ccfba42a-ca60-4227-8383-7aba97f3a44c',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft',45,214)
			
	Update [BusinessProcessWorkflow] set FirstStepId = @InstanceId4	where BusinessProcessWorkflowId = @InstanceId3			
			
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDI = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'	

	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId 
	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('8d25b828-d7ee-404f-9874-767d1ad39f1b',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Incomplete',179,77)
	
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDC = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1'

	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId  
	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('36b147dc-92ff-4e90-975e-d2422eb54223',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Complete',212,205)
	
			
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
		
	INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('458565e9-422c-4c5a-9aed-a858adecf3af', @CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,
           @InstanceId5,'Deployment Complete_2',NULL, NULL,@CMDC,NULL)

	Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = @InstanceId3
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
           
	INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('8e64ba4f-5b60-45a9-920a-29bf429faa55',@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,
           @InstanceId5,'Deployment Incomplete',NULL, NULL,@CMDI,NULL)
           
    EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    INSERT INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('245DF453-8CAB-4592-916D-50180F1A4723',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed')
         
	
    INSERT INTO [WorkflowStepPathSelectors]
           ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
           (4403,@CMPathSelectorInstanceId,1,@CMDC)
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
          
	INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('4a679c35-1b40-4bb6-a315-256ffafd47b3',@CMPathCDODefId,1,NULL,NULL,@CMDI,0,NULL,
           @InstanceId5,'Deployment Complete_1',NULL, NULL,@CMDC,NULL)

	Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = @InstanceId3 
     
      EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
          
	INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('07f12fc3-1a58-4933-9b4f-5a577b07f0e5',@CMPathCDODefId,1,NULL,NULL,@CMDraft,0,NULL,
           @InstanceId5,'Deployment Complete',NULL, NULL,@CMDC,NULL)

    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Draft' and WorkflowId = @InstanceId3  
     
     
           
    
END
GO


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateWFCamstarPLM' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateWFCamstarPLM
GO
CREATE PROCEDURE csiCMCreateWFCamstarPLM(@WFDescription NVARCHAR(255),@WFName NVARCHAR(30), @InstanceId2 varchar(16) OUTPUT, @InstanceId3 varchar(16) OUTPUT, @InstanceId4 varchar(16) OUTPUT, @InstanceId5 varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CMWrkFlowCDODefId INT
    SET @CMWrkFlowCDODefId=8517
    
    DECLARE @CMWrkFlowBaseCDODefId INT
    SET @CMWrkFlowBaseCDODefId=8518
    
    DECLARE @CMWrkFlowStepCDODefId INT
    SET @CMWrkFlowStepCDODefId=8578
    
    DECLARE @CMPathCDODefId INT
    SET @CMPathCDODefId=1440
    
    DECLARE @CMPathSelectorCDODefId INT
    DECLARE @CMPathSelectorInstanceId varchar(16)
    SET @CMPathSelectorCDODefId=1840
    
	DECLARE @CMDRAFT CHAR(16), @CMDI CHAR(16), @CMDC CHAR(16), @CMCL CHAR(16), @SpecId CHAR(16),@CMVOID CHAR(16), @CMREJ CHAR(16), @CMPD CHAR(16), @CMPA CHAR(16), @SpecDescription NVARCHAR(255)
    DECLARE @CMDRAFTName NVARCHAR(255)
	DECLARE @PAName NVARCHAR(255)

    EXEC csiPRDGetNextInstanceId @CMWrkFlowBaseCDODefId,@InstanceId2 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowCDODefId,@InstanceId3 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
           
    INSERT INTO [BusinessProcessWorkflow]
           ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
           ,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
     VALUES
           (@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
           
           
    INSERT INTO [BusinessProcessWorkflowBase]
           ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
    VALUES(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
     
    if @WFDescription = 'No Approval'
		SET @CMDRAFTName = 'Draft'
	else if  @WFDescription = 'Camstar'
		SET @CMDRAFTName = 'Draft Camstar'
    else if  @WFDescription = 'PLM'
		SET @CMDRAFTName = 'Draft PLM' 
		
     
    SET @CMDRAFT = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = @CMDRAFTName
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId 
    
    if @WFDescription = 'No Approval'
    INSERT INTO [WorkflowStep]
    ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('1fbf9172-3525-476d-9eed-5e869a4d2f00',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft',37,24)
	else if  @WFDescription = 'Camstar'
	INSERT INTO [WorkflowStep]
    ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('84ff7c87-122b-4e5b-a7c7-c8784836791a',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft Camstar',37,24)
	 else if  @WFDescription = 'PLM'
	INSERT INTO [WorkflowStep]
    ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('c6b8c4a2-0c30-47f2-a6f2-9f81a43e157a',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft PLM',37,24)
			
	Update [BusinessProcessWorkflow] set FirstStepId = @InstanceId4	where BusinessProcessWorkflowId = @InstanceId3
	
		
	if  @WFDescription = 'Camstar'
		SET @PAName = 'Pending Approval Camstar'
    else if  @WFDescription = 'PLM'
		SET @PAName = 'Pending Approval PLM' 
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMPA = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = @PAName
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId
	if  @WFDescription = 'Camstar'
			INSERT INTO [WorkflowStep]
			([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
			VALUES('e25e9d3a-fe51-4f48-9640-04064abcad15',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Pending Approval Camstar',35,137)
	else if  @WFDescription = 'PLM'
			INSERT INTO [WorkflowStep]
			([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
			VALUES('27df4f12-d414-4535-b62f-bdf4a505e485',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Pending Approval PLM',35,137)
		
				
		
				
			
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMPD = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Deployment' 
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId   
	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('1f7db9c4-5737-4ac4-a2dd-89ab47c85fb0',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Pending Deployment',32,248)
	
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMREJ = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Rejected' 
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId   

	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('fbdd0b51-2e43-47d9-ac29-d7f9b6eaf07f',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Rejected',180,135)
	
	
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDC = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete' 
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId 

	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('1f3f665c-583d-4aea-b648-e07227764b5b',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Complete',227,365)
	
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDI = @InstanceId4;

	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete' 
	   and BusinessProcessSpec.Revision = '1'
	select @SpecDescription = Description from [BusinessProcessSpec] where [BusinessProcessSpecId] = @SpecId 

	INSERT INTO [WorkflowStep]
   ([ExportImportKey],[CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES('de0081b2-d8e8-4460-bb6c-5540a480352a',@CMWrkFlowStepCDODefId,1,NULL,@SpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Incomplete',391,254)
	
			
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
	INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('24980df0-db54-454f-bfae-9019740bcbe9',@CMPathCDODefId,1,NULL,NULL,@CMDRAFT,0,NULL,
           @InstanceId5,'Pending Approval',NULL, NULL,@CMPA,NULL)
	Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = @CMDRAFTName and WorkflowId = @InstanceId3	
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('d84b65c1-e7a8-4b17-ba1b-b5b9520e7372',@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,
           @InstanceId5,'Pending Deployment',NULL, NULL,@CMPD,NULL)
    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Pending Approval' and WorkflowId = @InstanceId3

    EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    INSERT INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('F3A54DBA-568A-464A-801F-E0B9FEA4552D',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Approved')

    INSERT INTO [WorkflowStepPathSelectors]
           ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
           (4403,@CMPathSelectorInstanceId,1,@CMPA)
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('93fc44ef-1c3b-48ab-a021-21b424e89ee0',@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,
           @InstanceId5,'Rejected',NULL, NULL,@CMREJ,NULL)

    EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    INSERT INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('32F00D0C-4486-470B-98ED-FB1EB2AC0AE2',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Rejected')

    INSERT INTO [WorkflowStepPathSelectors]
           ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
           (4403,@CMPathSelectorInstanceId,2,@CMPA)
           
       
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,
           @InstanceId5,'Draft',NULL, NULL,@CMDRAFT,NULL)

    EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    INSERT INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('1C7AF084-F98F-4825-A0EF-6D71891E84E6',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Pending or TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Cancelled')
	
    INSERT INTO [WorkflowStepPathSelectors]
           ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
           (4403,@CMPathSelectorInstanceId,3,@CMPA)
           
           
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',@CMPathCDODefId,1,NULL,NULL,@CMPD,0,NULL,
           @InstanceId5,'Deployment Complete',NULL, NULL,@CMDC,NULL)
    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Pending Deployment' and WorkflowId = @InstanceId3 
     
     
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('157222b5-582c-4955-a1f3-55deb75f2907',@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,
           @InstanceId5,'Deployment Complete_2',NULL, NULL,@CMDC,NULL)
    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = @InstanceId3   
    
        
      
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('076c6a19-f638-4745-a3e2-788d94c42395',@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,
           @InstanceId5,'Deployment Incomplete',NULL, NULL,@CMDI,NULL)      


 
	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    INSERT INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed')
         
	
    INSERT INTO [WorkflowStepPathSelectors]
           ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
           (4403,@CMPathSelectorInstanceId,1,@CMDC)
               
           
         
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    INSERT INTO [Path]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           ('d57d4000-9dc5-412b-be63-db7363429e2c',@CMPathCDODefId,1,NULL,NULL,@CMDI,0,NULL,
           @InstanceId5,'Deployment Complete_1',NULL, NULL,@CMDC,NULL) 
    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = @InstanceId3       
       
       
          
END
GO


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateBusinessRule' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateBusinessRule
GO
CREATE PROCEDURE csiCMCreateBusinessRule(@ExportImportKey NVARCHAR(36),@BRDescription NVARCHAR(255),@BRName NVARCHAR(30), @BRHName NVARCHAR(30), @SBRName NVARCHAR(30), @BRSCRIPT NVARCHAR(4000), @SBRIsAdvancedMode BIT, @SBRRecurrenceFrequency INT, @SBRRecurrencePattern INT, @SBRScheduleHours NVARCHAR(255),
	@InstanceId1 varchar(16) OUTPUT, @InstanceId2 varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BRCDODefId INT
    SET @BRCDODefId=7569
    
    DECLARE @BRDataCDODefId INT
    SET @BRDataCDODefId=7573

    DECLARE @BRHandlerCDODefId INT
    SET @BRHandlerCDODefId=7564
    
    DECLARE @BRHandlerDataCDODefId INT
    SET @BRHandlerDataCDODefId=7567
    
    DECLARE @SchedBRCDODefId INT
    SET @SchedBRCDODefId=7587
    
    
    DECLARE @BRDataInstanceId CHAR(16), @BRHandlerInstanceId CHAR(16), @BRHandlerDataInstanceId CHAR(16)
    
    EXEC csiPRDGetNextInstanceId @BRCDODefId,@InstanceId1 OUTPUT
    EXEC csiPRDGetNextInstanceId @BRDataCDODefId,@BRDataInstanceId OUTPUT
    EXEC csiPRDGetNextInstanceId @BRHandlerCDODefId,@BRHandlerInstanceId OUTPUT
    EXEC csiPRDGetNextInstanceId @BRHandlerDataCDODefId,@BRHandlerDataInstanceId OUTPUT
    EXEC csiPRDGetNextInstanceId @SchedBRCDODefId,@InstanceId2 OUTPUT
 
           
    INSERT INTO BusinessRuleHandler
           ([BusinessRuleHandlerDataId]
           ,[BusinessRuleHandlerId]
           ,[BusinessRuleHandlerName]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ChangeHistoryId]
           ,[Description]
           ,[IconId]
           ,[IsFrozen]
           ,[IsValid]
           ,[Notes]
           ,[ValidateOnSave])
    VALUES
           (@BRHandlerDataInstanceId,@BRHandlerInstanceId,@BRHName,@BRHandlerCDODefId,1,NULL,@BRDescription,NULL,0,1,NULL,0)
           
           
           



  INSERT INTO BusinessRuleHandlerData
           ([BizRuleHandlerType]
           ,[BusinessRuleHandlerDataId]
           ,[BusinessRuleHandlerDataName]
           ,[BusinessRuleHandlerId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[IsFrozen]
           ,[Script]
           ,[ServiceType])
    VALUES(1,@BRHandlerDataInstanceId,@BRHName,@BRHandlerInstanceId, @BRHandlerDataCDODefId,1,0,@BRScript,NULL)




     
     
	INSERT INTO BusinessRule
           ([BusinessRuleDataId]
           ,[BusinessRuleId]
           ,[BusinessRuleName]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ChangeHistoryId]
           ,[Description]
           ,[IconId]
           ,[IsFrozen]
           ,[Notes])
    VALUES(@BRDataInstanceId,@InstanceId1,@BRName,@BRCDODefId,1,NULL,@BRDescription,NULL,0,NULL)


	INSERT INTO BusinessRuleData
           ([ExportImportKey]
		   ,[AlwaysExecute]
           ,[BusinessRuleCondition]
           ,[BusinessRuleDataId]
           ,[BusinessRuleDataName]
           ,[BusinessRuleId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ContextType]
           ,[IsFrozen]
           ,[Scope])
    VALUES(@ExportImportKey,1,NULL,@BRDataInstanceId,@BRName,@InstanceId1,@BRDataCDODefId,1,1140,0,0)
			
			
	INSERT INTO BusinessRuleDataHandlers
           ([BusinessRuleDataId]
           ,[FieldId]
           ,[HandlersId]
           ,[Sequence])
    VALUES(@BRDataInstanceId,13314,@BRHandlerInstanceId,1)


    DECLARE @StartDate DATETIME, @StartDateGMT DATETIME, @DueDate DATETIME, @DueDateGMT DATETIME
	SET @StartDate = GETDATE()
	SET @StartDateGMT = GETUTCDATE()
    IF (@SBRIsAdvancedMode = 1)
    BEGIN
		SET @DueDate = case when DATEPART(hh,GETDATE()) > 4 then DATEADD(hh,4,CAST(CAST(DATEADD(dd,1,GETDATE()) AS date) AS datetime)) else DATEADD(hh,4,CAST(CAST(GETDATE() AS date) AS datetime)) end
		SET @DueDateGMT = DATEADD(minute, DATEDIFF(minute, GETDATE(), GETUTCDATE()), case when DATEPART(hh,GETDATE()) > 4 then DATEADD(hh,4,CAST(CAST(DATEADD(dd,1,GETDATE()) AS date) AS datetime)) else DATEADD(hh,4,CAST(CAST(GETDATE() AS date) AS datetime)) end)
	END
	ELSE
	BEGIN
		SET @DueDate = @StartDate
		SET @DueDateGMT = @StartDateGMT
	END

	INSERT INTO ScheduledBusinessRule
           ([CDOTypeId]
           ,[ChangeCount]
           ,[ChangeHistoryId]
           ,[DayOfMonth]
           ,[DayOfWeek]
           ,[Description]
           ,[DueTime]
           ,[DueTimeGMT]
           ,[EndDate]
           ,[EndDateGMT]
           ,[ExecutionContext]
           ,[ExecutionContextType]
           ,[IconId]
		   ,[IsAdvancedMode]
           ,[IsFrozen]
           ,[IsLastDayOfMonth]
           ,[IsSystemDefined]
           ,[LockGUID]
           ,[MonthOfYear]
           ,[Notes]
           ,[OnExecute]
           ,[RecurrenceCount]
           ,[RecurrenceFrequency]
           ,[RecurrencePattern]
           ,[ScheduledBusinessRuleId]
           ,[ScheduledBusinessRuleName]
		   ,[ScheduleHours]
           ,[StartDate]
           ,[StartDateGMT]
           ,[Status])    
	VALUES(@SchedBRCDODefId,1,NULL,NULL,NULL,NULL,@DueDate,@DueDateGMT,
			NULL,NULL,'0004740000000001',1140,NULL,@SBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,@InstanceId1,NULL,@SBRRecurrenceFrequency,@SBRRecurrencePattern,
			@InstanceId2,@SBRName,@SBRScheduleHours,@StartDate,@StartDateGMT,1)
			
          
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateApprovalDecision' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateApprovalDecision
GO
CREATE PROCEDURE csiCMCreateApprovalDecision(@DecisionName NVARCHAR(30), @DecisionType INT, @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	EXEC csiPRDGetNextInstanceId 7857,@InstanceId OUTPUT
  
    INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
    VALUES
           (@InstanceId,'001e850000000000',@DecisionName,7857,1,@DecisionType, 0,0)
          
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CMPopulateDefaultData' 
	   AND 	  type = 'P')
    DROP PROCEDURE CMPopulateDefaultData
GO
CREATE PROCEDURE CMPopulateDefaultData
AS
    
    DECLARE @CMSpecID VARCHAR(16)   
    DECLARE @CMSpecBaseID VARCHAR(16)    
    DECLARE @CMWrkFlowCDODefId VARCHAR(16)    
    DECLARE @CMWrkFlowBaseCDODefId VARCHAR(16)    
    DECLARE @CMWrkFlowStepCDODefId VARCHAR(16)   
    DECLARE @CMPathCDODefId VARCHAR(16)    
    DECLARE @BusinessRuleId VARCHAR(16)    
    DECLARE @SchedBusinessRuleId VARCHAR(16)    
    DECLARE @ApprovalDecisionId VARCHAR(16)   
        
BEGIN
    SET NOCOUNT ON;

    PRINT('Creating Approval Decisions');
	EXEC csiCMCreateApprovalDecision 'Approved',10,@ApprovalDecisionId OUTPUT
	EXEC csiCMCreateApprovalDecision 'Rejected',20,@ApprovalDecisionId OUTPUT

     PRINT('Creating Draft...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
	
	PRINT('Creating Draft Camstar...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft Camstar',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
   
   PRINT('Creating Draft PLM...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft PLM',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
   
   PRINT('Creating Deployment Complete');
    EXEC csiCMCreateSpec 'Deployment_Complete.png','Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.','Deployment Complete',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
    
    PRINT('Creating Deployment Incomplete');
	EXEC csiCMCreateSpec 'Deployment_InComplete.png','Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.','Deployment Incomplete',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
       
    PRINT('Creating Pending Approval Camstar');
	EXEC csiCMCreateSpec 'Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval Camstar',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
   
   PRINT('Creating Pending Approval PLM');
	EXEC csiCMCreateSpec 'Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval PLM',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
   
   
    PRINT('Creating Rejected...');
	EXEC csiCMCreateSpec 'Rejected.png','In the Rejected step the package can be Voided or go back to Draft.','Rejected',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
   
    PRINT('Creating Pending Deployment...');
	EXEC csiCMCreateSpec 'Open.png','In the Pending Deployment step the Package can be manually deployed to one or more remote targets, changing the status to Deployment Complete or Deployment Incomplete, if deployment fails. If deployment fails, user can redeploy or Close the Package','Pending Deployment',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
       
       
       
    PRINT('Creating Workflow...');
	EXEC csiCMCreateWorkFlow 'No Approval','No Approval', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
	
	PRINT('Creating Workflow Camstar...');
	EXEC csiCMCreateWFCamstarPLM 'Camstar','Camstar', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
     
    PRINT('Creating Workflow PLM...');
	EXEC csiCMCreateWFCamstarPLM 'PLM','PLM', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
     
     
    PRINT('Creating Business Rule Export...');
	EXEC csiCMCreateBusinessRule 'c3575681-5d0c-4c5d-817e-ab70049107b3', 'This script will process exports associated with a change packages.','BR_DEPLOY','BRH_DEPLOY','SBR_DEPLOY','ExecuteQueryEX("ChangePackage_GetExportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ExportImportName", 1,Transaction::__Const.DataType.String, CLF::ExportNames, 0);ForEach(CLF::ExportName,CLF::ExportNames){InitQueryParametersEx("ExportName",CLF::ExportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByExportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeployment,ExportName,CLF::ExportName);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Deploy...');
	EXEC csiCMCreateBusinessRule '486d13ad-8e78-424e-a7a7-0bf7ae9375aa', 'This script will process exports associated with a change packages.','BR_DEPLOYSTATUS','BRH_DEPLOYSTATUS','SBR_DEPLOYSTATUS','ExecuteQueryEX("ChangePackage_GetDeploymentsInQueue", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"Name", 1,Transaction::__Const.DataType.String, CLF::PackageNames, 0);ForEach(CLF::ChangePackageName,CLF::PackageNames){CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeploymentInQueue);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Import...');
	EXEC csiCMCreateBusinessRule '88729868-5ee0-4f77-9884-98e0349eb969', 'This script will process imports (activations) associated with a change packages.','BR_ACTIVATION','BRH_ACTIVATION','SBR_ACTIVATION', 'ExecuteQueryEX("ChangePackage_GetImportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ImportSetName", 1,Transaction::__Const.DataType.String, CLF::ImportNames, 0);ForEach(CLF::ImportName,CLF::ImportNames){InitQueryParametersEx("ImportName",CLF::ImportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByImportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessActivation,ImportSetName,CLF::ImportName);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Notifications...');
	EXEC csiCMCreateBusinessRule 'E78C5FFF-75AF-49d6-A964-A8772AE00437', 'This script will support email notifications for Change Package.','BR_NOTIFICATIONS','BRH_NOTIFICATIONS','SBR_NOTIFICATIONS', 'if(CLF::__CDOID.SessionValues and CLF::__CDOID.SessionValues.Factory){Call(CLF::__CDOID.SessionValues.Factory,SendReminderEmails);}',1,NULL,NULL,'4',
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT

	PRINT('Creating RPT Control Loop Limits Business Rule...');
	EXEC csiCMCreateBusinessRule 'aafd3304-a163-460a-bb06-a6b15f5c63a1', 'This script will run the RPT Control Loop Limits Calculations.','BR_RPTControlLimits','BRH_RPTControlLimits_Monthly', 'SBR_RPTControlLimits', 'CLF::RPTControlLimitsUpdateObject = null;CreateCDO("RPTControlLimitsUpdate", false, false, CLF::RPTControlLimitsUpdateObject);CLF::RPTControlLimitsUpdateObject.RPTUpdateOccurrencePattern = 2;CLF::RPTControlLimitsUpdateObject.NoOfRPTCombinations = 100;CLF::RPTControlLimitsUpdateObject.HistoryTimePeriod = 30;CLF::RPTControlLimitsUpdateObject.Factory = CLF::__CDOID.SessionValues.Factory;Call(CLF::RPTControlLimitsUpdateObject, ProcessRPTControlLimits);',0,NULL,NULL,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
   
    
    PRINT('Complete.');
END
GO
EXEC CMPopulateDefaultData
GO

