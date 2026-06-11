--------------------------------------------------------------------------------
-- SCRIPT:ReloadCMDefaultData.sql
-- DESCR: Reloads the default OOB ChangeManagement Specs, Business Rules, Business Rule Handlers, Approval Decision, Action, Action Rules, Workflows. 
--		  This will be run through a batch file csiReloadCMData.bat. This batch file will get executed when user checks on Load Change Management in the Management studio
--Copyright Siemens 2025  


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCreateGUID' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCreateGUID
GO
CREATE PROCEDURE csiCreateGUID(@PermissionName VARCHAR(255),@RoleGID varchar(36) OUTPUT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

select @RoleGID =	
 (      SubString(ExportImportKeyGUID, 1, 8) +
'-' + SubString(ExportImportKeyGUID, 9, 4) +
'-' + SubString(ExportImportKeyGUID, 13, 4) +
'-' + SubString(ExportImportKeyGUID, 17, 4) +
'-' + SubString(ExportImportKeyGUID, 21,12))
from (
select upper(convert(nvarchar(36),hashbytes('MD5',@PermissionName),2)) as ExportImportKeyGUID
) c;


END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCreateGUID' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCreateGUID
GO
CREATE PROCEDURE csiCreateGUID(@PermissionName NVARCHAR(255),@RoleGID nvarchar(36) OUTPUT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

select @RoleGID =	
 (      SubString(ExportImportKeyGUID, 1, 8) +
'-' + SubString(ExportImportKeyGUID, 9, 4) +
'-' + SubString(ExportImportKeyGUID, 13, 4) +
'-' + SubString(ExportImportKeyGUID, 17, 4) +
'-' + SubString(ExportImportKeyGUID, 21,12))
from (
select upper(convert(nvarchar(36),hashbytes('MD5',@PermissionName),2)) as ExportImportKeyGUID
) c;


END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateSpec
-- DESCR: Inserts the CM Specs. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
  
    IF NOT EXISTS (SELECT * FROM [BusinessProcessSpecBase] WHERE BusinessProcessSpecName = @CMSpecName)
           Begin
           
				INSERT INTO [BusinessProcessSpecBase]
						([BusinessProcessSpecBaseId],[BusinessProcessSpecName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
				VALUES
					  (@InstanceId1,@CMSpecName,@CMSpecBaseCDODefId,1,NULL,@InstanceId)
					  
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
           
           
						print @CMSpecName
						if @CMSpecName ='Draft' 
							select @RoleId = RoleId from [RoleDef] where RoleName = 'DraftPermissions'
						else
								select @RoleId = RoleId from [RoleDef] where Description like @CMSpecName+'%'
					   
						INSERT INTO [ChangeMgtSpecAllowableRoles]
						  ([AllowableRolesId],[BusinessProcessSpecId],[FieldId],[Sequence])
						 VALUES
						(@RoleId,@InstanceId,@CMFieldRoleDefId,1)
           
		  END
	ELSE
		Begin
			Delete FROM [BusinessProcessSpec] where [BusinessProcessSpecBaseId] in (select a.[BusinessProcessSpecBaseId] from [BusinessProcessSpec] a, [BusinessProcessSpecBase] b
			where a.businessprocessspecbaseid = b.businessprocessspecbaseid and b.BusinessProcessSpecName = @CMSpecName)
			Delete FROM [BusinessProcessSpecBase] WHERE BusinessProcessSpecName = @CMSpecName
		
				INSERT INTO [BusinessProcessSpecBase]
						([BusinessProcessSpecBaseId],[BusinessProcessSpecName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
				VALUES
					  (@InstanceId1,@CMSpecName,@CMSpecBaseCDODefId,1,NULL,@InstanceId)
					  
					   If @CMSpecName = 'Draft Camstar'
							INSERT INTO [BusinessProcessSpec]
										([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
										,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
								VALUES
										(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
										,NULL,NULL,'',NULL,1,1,@StepIcon,'',1,1,0,0)
					  else if @CMSpecName = 'Draft PLM'
											INSERT INTO [BusinessProcessSpec]
									([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
									,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
								 VALUES
										(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
											,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,1,0,0)
						else if @CMSpecName = 'Draft'
							 INSERT INTO [BusinessProcessSpec]
										 ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
										,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
								VALUES
										(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
											,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,0,0)
						else if @CMSpecName = 'Pending Approval Camstar'
									INSERT INTO [BusinessProcessSpec]
												([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
												,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
									VALUES
												(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
												 ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1)
						else if @CMSpecName = 'Pending Approval PLM'
									INSERT INTO [BusinessProcessSpec]
												 ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
											  ,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
									  VALUES
											  (@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
												 ,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1)
						else 
									INSERT INTO [BusinessProcessSpec]
										 ([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
											,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval])
									VALUES
											(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription
									,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0)
           
           
						print @CMSpecName
						PRINT('after ' + @CMSpecName + ' insert');
						if @CMSpecName ='Draft' 
							select @RoleId = RoleId from [RoleDef] where RoleName = 'DraftPermissions'
						else
								select @RoleId = RoleId from [RoleDef] where Description like @CMSpecName+'%'
					   
						INSERT INTO [ChangeMgtSpecAllowableRoles]
						  ([AllowableRolesId],[BusinessProcessSpecId],[FieldId],[Sequence])
						 VALUES
						(@RoleId,@InstanceId,@CMFieldRoleDefId,1)
						 PRINT('[BusinessProcessSpecBase] ' + @CMSpecName + ' deleted and created');
		  END
END
GO
   
   
           
		   
--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWorkFlow
-- DESCR: Inserts the CM Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
           
    
           
           
    IF NOT EXISTS (SELECT * FROM [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName)
           Begin
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
			end
    ELSE
		
		Begin
			delete from [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName
		
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
    INSERT INTO [WorkflowStep]
    ([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES(@CMWrkFlowStepCDODefId,1,NULL,'The Package automatically enters Draft state after creation. In Draft state the user edits core package attributes and assigns instance content. Outside of Draft state, package attributes and instance content cannot be altered. ',NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft',45,214)
			
	Update [BusinessProcessWorkflow] set FirstStepId = @InstanceId4	where BusinessProcessWorkflowId = @InstanceId3				
				
			
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDI = @InstanceId4;
	--select @SpecBaseId = [BusinessProcessSpecBaseId] from [BusinessProcessSpecBase] where [BusinessProcessSpecName] = 'Deployment Incomplete'  
	select @SpecId = BusinessProcessSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'

	INSERT INTO [WorkflowStep]
   ([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES(@CMWrkFlowStepCDODefId,1,NULL,'Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.',NULL,0,0,NULL,0,
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

	INSERT INTO [WorkflowStep]
   ([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
           ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
           ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
    VALUES(@CMWrkFlowStepCDODefId,1,NULL,'Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.',NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',@SpecId,1, 
			NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Complete',212,205)
	
		
			
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
           ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           (@CMPathCDODefId,1,NULL,NULL,@CMDI,0,NULL,
           @InstanceId5,'Deployment Complete_1',NULL, NULL,@CMDC,NULL)

	Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = @InstanceId3 

      EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
          
	INSERT INTO [Path]
           ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
           ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
    VALUES
           (@CMPathCDODefId,1,NULL,NULL,@CMDraft,0,NULL,
           @InstanceId5,'Deployment Complete',NULL, NULL,@CMDC,NULL)

    Update [WorkflowStep] set DefaultPathId =  @InstanceId5 where WorkflowStepName = 'Draft' and WorkflowId = @InstanceId3  
    PRINT('[[BusinessProcessWorkflowBase]] ' + @WFName + ' deleted and created');
	end

END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWFCamstarPLM
-- DESCR: Inserts the CM PLM and Camstar Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
           
           
   IF NOT EXISTS (SELECT * FROM [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName)
           Begin
           
				INSERT INTO [BusinessProcessWorkflowBase]
					 ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
				 VALUES(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
    
				INSERT INTO [BusinessProcessWorkflow]
					 ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
					,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
				VALUES
					 (@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
					 
					           
     
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
			end
    ELSE
    
		
        Begin
           delete from [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName
				INSERT INTO [BusinessProcessWorkflowBase]
					 ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
				 VALUES(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
    
				INSERT INTO [BusinessProcessWorkflow]
					 ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
					,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
				VALUES
					 (@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
					 
					           
     
   
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
    PRINT('[BusinessProcessWorkflowBase] ' + @WFName + ' deleted and created'); 
			end
		
      
END
GO




--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateBusinessRule
-- DESCR: Inserts the CM BusinessRules. If it exists then deletes and reloads
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateBusinessRule' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateBusinessRule
GO
CREATE PROCEDURE csiCMCreateBusinessRule(@BRDescription NVARCHAR(255),@BRName NVARCHAR(30), @BRHName NVARCHAR(30), @SBRName NVARCHAR(30), @BRSCRIPT NVARCHAR(4000), @SBRIsAdvancedMode BIT, @SBRRecurrenceFrequency INT, @SBRRecurrencePattern INT, @SBRScheduleHours NVARCHAR(255),
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
 
           
         
         IF NOT EXISTS (SELECT * FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = @BRHName)
           Begin  
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
	
            end
    ELSE
    Begin 
    delete  FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = @BRHName
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
           PRINT('[BusinessRuleHandler] ' + @BRHName + 'deleted and added');
		   
		   delete  FROM BusinessRuleHandlerData WHERE BusinessRuleHandlerDataName = @BRHName
		   
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
            end
		
		
         IF NOT EXISTS (SELECT * FROM BusinessRule WHERE BusinessRuleName = @BRName)
           Begin 
     
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
			   ([AlwaysExecute]
			   ,[BusinessRuleCondition]
			   ,[BusinessRuleDataId]
			   ,[BusinessRuleDataName]
			   ,[BusinessRuleId]
			   ,[CDOTypeId]
			   ,[ChangeCount]
			   ,[ContextType]
			   ,[IsFrozen]
			   ,[Scope])
		VALUES(1,NULL,@BRDataInstanceId,@BRName,@InstanceId1,@BRDataCDODefId,1,1140,0,0)
		
		
    end
    ELSE
     Begin 
     delete FROM BusinessRule WHERE BusinessRuleName = @BRName
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
    PRINT('[BusinessRule] ' + @BRName + 'deleted and inserted');
	
	delete FROM BusinessRuleData WHERE BusinessRuleDataName = @BRName
	 
	INSERT INTO BusinessRuleData
           ([AlwaysExecute]
           ,[BusinessRuleCondition]
           ,[BusinessRuleDataId]
           ,[BusinessRuleDataName]
           ,[BusinessRuleId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ContextType]
           ,[IsFrozen]
           ,[Scope])
    VALUES(1,NULL,@BRDataInstanceId,@BRName,@InstanceId1,@BRDataCDODefId,1,1140,0,0)
	
	
    end
		
			
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


IF NOT EXISTS (SELECT * FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = @SBRName)
           Begin 
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
			  end
    ELSE
    Begin 
    delete FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = @SBRName
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
	VALUES(@SchedBRCDODefId,1,NULL,NULL,NULL,NULL,@StartDate,@StartDateGMT,
			NULL,NULL,'0004740000000001',1140,NULL,@SBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,@InstanceId1,NULL,@SBRRecurrenceFrequency,@SBRRecurrencePattern,
			@InstanceId2,@SBRName,@SBRScheduleHours,@StartDate,@StartDateGMT,1)
			PRINT('[ScheduledBusinessRule] ' + @SBRName + ' deleted and added');
			  end
		
			
          
END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM reakted roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreateRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreateRole
GO
CREATE PROCEDURE csiRBACCreateRole(@RoleName NVARCHAR(50), @RoleDescription NVARCHAR(255), @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleCDODefId INT
    SET @RoleCDODefId=7130

    EXEC csiPRDGetNextInstanceId @RoleCDODefId,@InstanceId OUTPUT
     IF NOT EXISTS (SELECT * FROM RoleDef WHERE RoleName = @RoleName)
           Begin
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
        end
    ELSE
        Begin
    delete from RoleDef WHERE RoleName = @RoleName
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
       PRINT('[RoleDef] ' + @RoleName + ' deleted and created');
        end
		
     
END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermission' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermission
GO
CREATE PROCEDURE csiRBACCreatePermission(@RoleId CHAR(16), @PermissionName VARCHAR(255), @PermissionType INT, @ObjectMetaId INT, @PermissionModesFlag INT, @ObjectInstanceId CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    DECLARE @IID VARCHAR(16)
	 DECLARE @RoleGID VARCHAR(36)
BEGIN
    SET NOCOUNT ON;

    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
     EXEC csiCreateGUID @PermissionName, @RoleGID OUTPUT
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(@RoleGID,@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the @PermissionModesFlag value
	IF ( @PermissionModesFlag = 0 )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
    Else IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180) )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
          AND BitNumber = 2
    ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType = 180 )
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=@PermissionType
          AND BitNumber IN (1,2,3,4)
	ELSE
		PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignPermissionToRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignPermissionToRole
GO

CREATE PROCEDURE csiRBACAssignPermissionToRole(@RoleName VARCHAR(255), @PermissionName VARCHAR(255), @PermissionType INT, @ObjectMetaId INT, @PermissionModesFlag INT, @ObjectInstanceId CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    DECLARE @IID VARCHAR(16)
	 DECLARE @RoleGID VARCHAR(36)
	 DECLARE @RoleId CHAR(16)
BEGIN
    SET NOCOUNT ON;
    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
    EXEC csiCreateGUID @PermissionName, @RoleGID OUTPUT
	SELECT @RoleId = RoleId FROM [RoleDef] WHERE RoleName = @RoleName
	IF NOT EXISTS (SELECT * FROM [RolePermission] WHERE RoleId = @RoleId AND RolePermissionName = @PermissionName)
    Begin  
		INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
				VALUES(@RoleGID,@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);

		-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
		-- based on the @PermissionModesFlag value
		IF ( @PermissionModesFlag = 0 )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
		Else IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180, 230) )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
			  AND BitNumber = 2
		ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType IN (180, 230) )
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=@PermissionType
			  AND BitNumber IN (1,2,3,4)
		ELSE
			PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
	END
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACDeleteUnavailablePermissions' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACDeleteUnavailablePermissions
GO

CREATE PROCEDURE csiRBACDeleteUnavailablePermissions(@PermissionType INT, @PermissionName VARCHAR(255))
AS
	DECLARE @UnavialiblePermissions CHAR(16)
	BEGIN
		SET NOCOUNT ON;
		DECLARE @CURSOR CURSOR
		SET @CURSOR  = CURSOR SCROLL
		FOR
		SELECT [RolePermission].RolePermissionId FROM [RolePermission] 
		WHERE RolePermissionName = @PermissionName AND PermissionType <> @PermissionType
		OPEN @CURSOR
		FETCH NEXT FROM @CURSOR INTO @UnavialiblePermissions
		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM [RolePermission] WHERE RolePermissionId = @UnavialiblePermissions
			DELETE FROM [RolePermissionModes] WHERE RolePermissionId = @UnavialiblePermissions
			FETCH NEXT FROM @CURSOR INTO @UnavialiblePermissions
		END
	CLOSE @CURSOR
	END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateApprovalDecision
-- DESCR: Inserts the CM reakted approval decision. If it exists then deletes and recreates 
--------------------------------------------------------------------------------
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
  
  IF NOT EXISTS (SELECT * FROM ApprovalDecision WHERE ApprovalDecisionName = @DecisionName)
           Begin
    INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
    VALUES
           (@InstanceId,'001e850000000000',@DecisionName,7857,1,@DecisionType, 0,0)
            end
    ELSE
     Begin
     delete from ApprovalDecision WHERE ApprovalDecisionName = @DecisionName
    INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
    VALUES
           (@InstanceId,'001e850000000000',@DecisionName,7857,1,@DecisionType, 0,0)
           PRINT('[ApprovalDecision] ' + @DecisionName + ' deleted and created');
            end
		
          
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMCreateIDControl' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMCreateIDControl
GO
CREATE PROCEDURE csiCMCreateIDControl(@IDType NVARCHAR(30), @NextId INT)
AS
BEGIN
    SET NOCOUNT ON;
  
  IF NOT EXISTS (SELECT * FROM IDControl WHERE IDType = @IDType)
           Begin
    INSERT INTO IDControl (IDType,  NextID) 
        VALUES (@IDType, @NextId); 
            end
    ELSE
		PRINT('[IDType] ' + @IDType + ' already exists');
          
END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionRule' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionRule
GO
CREATE PROCEDURE createActionRule(
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Expression			nvarchar(1000))
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM ActionRule WHERE ActionRuleName = @Name)
	BEGIN
		PRINT('Inserting ActionRule: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionRule'		
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT INTO ActionRule
			(ActionRuleId
			,ActionRuleName
			,CDOTypeId
			,ChangeCount
			,Description
			,Expression
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,@Expression		-- nvarchar(255)
			,0);                -- bit
	END
	ELSE
	BEGIN
	delete FROM ActionRule WHERE ActionRuleName = @Name
		PRINT('deleteing and Inserting ActionRule: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionRule'		
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT INTO ActionRule
			(ActionRuleId
			,ActionRuleName
			,CDOTypeId
			,ChangeCount
			,Description
			,Expression
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,@Expression		-- nvarchar(255)
			,0);                -- bit
			PRINT('ActionRule ' + @Name + 'deleted and inserted');
	END
		
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: createActionCategory
-- DESCR: Helper function to create an Action Category record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionCategory' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionCategory
GO
CREATE PROCEDURE createActionCategory(
	@Name				nvarchar(30),
	@LabelName			nvarchar(50),
	@Sequence			int)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM ActionCategory WHERE ActionCategoryName = @Name)
	BEGIN
		PRINT('Inserting ActionCategory: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionCategory'	
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT INTO ActionCategory
			(ActionCategoryId, 
			ActionCategoryName, 
			CDOTypeId, 
			ChangeCount, 
			LabelName, 
			LabelText, 
			Sequence,
			IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@LabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,@Sequence			-- int
			,0);				-- bit
	END
	ELSE
		Begin
		PRINT('Deleting and Inserting ActionCategory: ' + @Name);
		delete FROM ActionCategory WHERE ActionCategoryName = @Name
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionCategory'	
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT INTO ActionCategory
			(ActionCategoryId, 
			ActionCategoryName, 
			CDOTypeId, 
			ChangeCount, 
			LabelName, 
			LabelText, 
			Sequence,
			IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@LabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,@Sequence			-- int
			,0);				-- bit
			
		PRINT('ActionCategory ' + @Name + ' deleted and inserted');
		End
END
GO

-----------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create ActionDef record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionDef
GO
CREATE PROCEDURE createActionDef(
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Type				int, 
    @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int

	SELECT @InstanceId = ActionId FROM ActionDef WHERE ActionName = @Name;
	
	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting Action: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionDef'
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT INTO ActionDef
			(ActionId
			,ActionName
			,ActionType
			,CDOTypeId
			,ChangeCount
			,Description
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@Type				-- int
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,0);                -- bit

	END
	ELSE
		Begin
		delete FROM ActionDef WHERE ActionName = @Name
		PRINT('Deleting & Inserting Action: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionDef'
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT INTO ActionDef
			(ActionId
			,ActionName
			,ActionType
			,CDOTypeId
			,ChangeCount
			,Description
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@Type				-- int
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,0);   
		PRINT('Action ' + @Name + 'deleted and inserted');
		end
END
GO
	
-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createUIAction' 
	   AND 	  type = 'P')
    DROP PROCEDURE createUIAction
GO
CREATE PROCEDURE createUIAction(
	@ActionId			char(16),
	@Name				nvarchar(30),
	@Type				int,
	@Description		nvarchar(255),
	@UIType				nvarchar(30),
	@UIVirtualPageName	nvarchar(30),
	@UIPageFlowName	    nvarchar(30),
	@MapItem		    nvarchar(30),
	@PortalTabOption	int,
	@ClearValues		bit,
	@ServiceName		nvarchar(30),
	@LabelName			nvarchar(66),
	@ShowButtons		bit,
	@IsPrimary          bit,
	@ActionCategoryName	nvarchar(30),
	@Sequence			int,
	@Width				int,
	@Height				int,
	@ForceRedirect		bit,
	@InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int	
	DECLARE @UIVirtualPageId	varchar(16);
	DECLARE @UIPageFlowId		varchar(16);
	DECLARE @ActionCategoryId	varchar(16);
	DECLARE @FloatPageLocationId	varchar(16);

	SELECT @InstanceId = UIActionId FROM UIAction WHERE UIActionName = @Name ;

	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting UIAction: ' + @Name);

		SET @UIVirtualPageId = NULL;
		IF ISNULL(@UIVirtualPageName,'') <> ''
			SELECT @UIVirtualPageId = UIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = @UIVirtualPageName;  
		
		SET @UIPageFlowId = NULL;
		IF ISNULL(@UIPageFlowName,'') <> ''
			SELECT @UIPageFlowId = UIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = @UIPageFlowName;  
		
		SET @ActionCategoryId = NULL;
		IF ISNULL(@ActionCategoryName,'') <> ''
			SELECT @ActionCategoryId = ActionCategoryId FROM ActionCategory WHERE ActionCategoryName = @ActionCategoryName;  
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = @UIType
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT INTO UIAction
		    (ActionCategoryId
		    ,CDOTypeId
		    ,ChangeCount
		    ,ClearValues
		    ,CloseButtonOnly
		    ,DenyDataContract
		    ,ESigRequired
		    ,ExecuteOnSubmit
		    ,ForceRedirect
		    ,IsDisabled
		    ,IsFrozen
		    ,IsHidden
			,IsPrimary
		    ,IsReturn
		    ,LabelName
		    ,LabelText
		    ,ParentId
		    ,ReloadValues
		    ,ServiceName
		    ,Sequence
		    ,ShowButtons
		    ,UIActionId
		    ,UIActionName
		    ,UIVirtualPageId
		    ,UIPageFlowId
		    ,MapItem
		    ,PortalTabOption
		    ,WIPMessagesRequired)
			VALUES
			(@ActionCategoryId  -- char(16)
			,@CDOTypeId         -- int
			,1                  -- int
			,@ClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@ForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@IsPrimary         -- bit
			,0                  -- int
			,@LabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,@ActionId			-- char(16)
			,0                  -- int
			,@ServiceName       -- nvarchar(30)
			,@Sequence			-- int
			,@ShowButtons		-- bit
			,@InstanceId        -- char(16)
			,@Name              -- nvarchar(30)
			,@UIVirtualPageId   -- char(16)
			,@UIPageFlowId		-- char(16)
			,@MapItem			-- nvarchar(30)
			,@PortalTabOption	-- int
			,0);				-- int  
	  IF (@Width IS NOT NULL) AND (@Height IS NOT NULL)
	  BEGIN
	    SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @FloatPageLocationId OUTPUT;
		
        
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (@FloatPageLocationId, @CDOTypeId, 1, 0, @InstanceId, @Width, @Height);
        
        UPDATE UIAction SET FrameLocationId = @FloatPageLocationId WHERE UIActionId = @InstanceId;
	  END
	END
	ELSE
	BEGIN
		PRINT('Deleting & Inserting UIAction: ' + @Name);
		delete FROM UIAction WHERE UIActionName = @Name 
		SET @UIVirtualPageId = NULL;
		IF ISNULL(@UIVirtualPageName,'') <> ''
			SELECT @UIVirtualPageId = UIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = @UIVirtualPageName;  
		
		SET @UIPageFlowId = NULL;
		IF ISNULL(@UIPageFlowName,'') <> ''
			SELECT @UIPageFlowId = UIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = @UIPageFlowName;  
		
		SET @ActionCategoryId = NULL;
		IF ISNULL(@ActionCategoryName,'') <> ''
			SELECT @ActionCategoryId = ActionCategoryId FROM ActionCategory WHERE ActionCategoryName = @ActionCategoryName;  
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = @UIType
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT INTO UIAction
		    (ActionCategoryId
		    ,CDOTypeId
		    ,ChangeCount
		    ,ClearValues
		    ,CloseButtonOnly
		    ,DenyDataContract
		    ,ESigRequired
		    ,ExecuteOnSubmit
		    ,ForceRedirect
		    ,IsDisabled
		    ,IsFrozen
		    ,IsHidden
			,IsPrimary
		    ,IsReturn
		    ,LabelName
		    ,LabelText
		    ,ParentId
		    ,ReloadValues
		    ,ServiceName
		    ,Sequence
		    ,ShowButtons
		    ,UIActionId
		    ,UIActionName
		    ,UIVirtualPageId
		    ,UIPageFlowId
		    ,MapItem
		    ,PortalTabOption
		    ,WIPMessagesRequired)
			VALUES
			(@ActionCategoryId  -- char(16)
			,@CDOTypeId         -- int
			,1                  -- int
			,@ClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@ForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@IsPrimary         -- bit
			,0                  -- int
			,@LabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,@ActionId			-- char(16)
			,0                  -- int
			,@ServiceName       -- nvarchar(30)
			,@Sequence			-- int
			,@ShowButtons		-- bit
			,@InstanceId        -- char(16)
			,@Name              -- nvarchar(30)
			,@UIVirtualPageId   -- char(16)
			,@UIPageFlowId		-- char(16)
			,@MapItem			-- nvarchar(30)
			,@PortalTabOption	-- int
			,0);				-- int  
	  IF (@Width IS NOT NULL) AND (@Height IS NOT NULL)
	  BEGIN
	    SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @FloatPageLocationId OUTPUT;
		
        
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (@FloatPageLocationId, @CDOTypeId, 1, 0, @InstanceId, @Width, @Height);
        
        UPDATE UIAction SET FrameLocationId = @FloatPageLocationId WHERE UIActionId = @InstanceId;
	  END
	  PRINT('UIAction ' + @Name + 'deleted and added');
	END
		
END
GO
	
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createAction' 
	   AND 	  type = 'P')
    DROP PROCEDURE createAction
GO
CREATE PROCEDURE createAction(
	@Name				nvarchar(30),
	@ActionCategoryName	nvarchar(30),
	@Type				int,
	@Description		nvarchar(255),
	@UIType				nvarchar(30),
	@UIVirtualPageName	nvarchar(30),
	@UIPageFlowName	    nvarchar(30),
	@ClearValues		bit,
	@Sequence			int,
	@ServiceName		nvarchar(30),
	@LabelName			nvarchar(66),
	@MapItem			nvarchar(30),
	@PortalTabOption	int,
	@ShowButtons		bit,
	@IsPrimary          bit,
	@Width				int,
	@Height				int,
	@ForceRedirect		bit)
AS
BEGIN
    SET NOCOUNT ON;
	/* It is currently unclear to me why the UIAction has a Name or Description */
	DECLARE @ActionId			varchar(16);
	DECLARE @UIActionId			varchar(16);
	
	EXEC CreateActionDef @Name, @Description, @Type, @ActionId OUTPUT
	EXEC createUIAction @ActionId,@Name,@Type,@Description,@UIType,@UIVirtualPageName,@UIPageFlowName, @MapItem,
		@PortalTabOption, @ClearValues, @ServiceName,@LabelName,@ShowButtons,@IsPrimary,@ActionCategoryName,@Sequence,@Width,@Height,@ForceRedirect,
		@UIActionId OUTPUT
			
	UPDATE ActionDef SET UIActionId = @UIActionId WHERE ActionId=@ActionId;
END
GO


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'addActionRuleToActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE addActionRuleToActionDef
GO
CREATE PROCEDURE addActionRuleToActionDef(
	@ActionRule			nvarchar(30), 
	@Action				nvarchar(30))
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @FieldId		int
	DECLARE @ActionId		varchar(16)
	DECLARE @ActionRuleId	varchar(16)
	DECLARE @Sequence		int
	
	
	IF NOT EXISTS (	SELECT * FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = @Action AND r.ActionRuleName = @ActionRule
				   )
	BEGIN
		PRINT('Adding ActionRule ' + @ActionRule + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId FROM ActionDef WHERE ActionName = @Action;
		SELECT @ActionRuleId = ActionRuleId FROM ActionRule WHERE ActionRuleName = @ActionRule;

		IF @ActionId IS NULL
		BEGIN
			PRINT('Action ' + @Action + ' could not be found');
			RETURN
		END
		IF @ActionRuleId IS NULL
		BEGIN
			PRINT('ActionRule ' + @ActionRule + ' could not be found');
			RETURN
		END
		
		SELECT @FieldID = f.FieldID FROM CDOFields f 
			JOIN CDODefinition d ON d.CDODefID=f.CDODefID
			WHERE f.FieldName='ActionRules' and d.CDOName='ActionDef';
		
		SELECT @Sequence = COUNT(*) + 1 FROM ActionDefActionRules WHERE ActionId = @ActionId;
		
		INSERT INTO ActionDefActionRules 
			(ActionId
			,ActionRulesId
			,FieldId
			,Sequence)
			VALUES
			(@ActionId           -- char(16)
			,@ActionRuleId		 -- char(16)
			,@FieldId			 -- int
			,@Sequence);         -- int
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the ActionRule ' + @ActionRule);
END
GO


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'addSourcePageToActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE addSourcePageToActionDef
GO
CREATE PROCEDURE addSourcePageToActionDef(
	@VirtualPage	nvarchar(30),
	@Action			nvarchar(30),
	@ExportImportKey nvarchar(36))
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @ActionId		varchar(16)
	DECLARE @ActionRuleId	varchar(16)
	DECLARE @CDOTypeId		int
	DECLARE @InstanceId		varchar(16);
	DECLARE @UIVirtualPageId varchar(16);
	
	
	IF NOT EXISTS (	SELECT * FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = @Action AND v.UIVirtualPageName = @VirtualPage
				   )
	BEGIN
		PRINT('Adding UISourcePage ' + @VirtualPage + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId FROM ActionDef WHERE ActionName = @Action;
		SELECT @UIVirtualPageId = UIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = @VirtualPage;  

		IF @ActionId IS NULL
		BEGIN
			PRINT('Action ' + @Action + ' could not be found');
			RETURN
		END
		IF @UIVirtualPageId IS NULL
		BEGIN
			PRINT('UIVirtualPage ' + @VirtualPage + ' could not be found');
			RETURN
		END
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName ='UISourcePage';
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;

		INSERT INTO UISourcePage 
			(ActionId
			,CDOTypeId
			,ChangeCount
			,UISourcePageId
			,UIVirtualPageId
			,ExportImportKey)
			VALUES
			(@ActionId           -- char(16)
			,@CDOTypeId			 -- int
			,0					 -- int
			,@InstanceId		 -- char(16)
			,@UIVirtualPageId
			,@ExportImportKey);  -- char(16)
				
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the UISourcePage ' + @VirtualPage);
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignRoleIfExist' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignRoleIfExist
GO
CREATE PROCEDURE csiRBACAssignRoleIfExist(@ExRoleName NVARCHAR(255), @NewRoleId CHAR(16), @NewRoleDescription NVARCHAR(255), @OrganizationName NVARCHAR(255), @Propagate INT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
    DECLARE @RoleGID NVARCHAR(36)
	DECLARE @EmployeeName VARCHAR (500)


BEGIN
    SET NOCOUNT ON;
	--------------------------------------------------
	DECLARE @CURSOR CURSOR
	SET @CURSOR  = CURSOR SCROLL
	FOR
	SELECT [Employee].EmployeeName FROM [EmployeeRole] 
	LEFT JOIN [RoleDef] ON [EmployeeRole].RoleId = [RoleDef].RoleId
	LEFT JOIN [Employee] ON [EmployeeRole].EmployeeId = [Employee].EmployeeId
	WHERE [RoleDef].RoleName = @ExRoleName
	OPEN @CURSOR
	FETCH NEXT FROM @CURSOR INTO @EmployeeName
	WHILE @@FETCH_STATUS = 0
	BEGIN
	--------------------------------------------------
		SELECT @EmployeeId=EmployeeId
		FROM Employee
		WHERE EmployeeName=@EmployeeName

		SET @OrgId=NULL
		IF NOT @OrganizationName IS NULL
        SELECT @OrgId=OrganizationId
        FROM Organization
        WHERE OrganizationName=@OrganizationName

		SET @CDODefId=7782

		EXEC csiPRDGetNextInstanceId @CDODefId,@IID OUTPUT
		set @NewRoleDescription = @NewRoleDescription + @EmployeeName
		EXEC csiCreateGUID @NewRoleDescription, @RoleGID OUTPUT
		INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
        VALUES (@RoleGID,@IID, @CDODefId, @NewRoleId, @EmployeeId, 0, @Propagate, @OrgId);
	   FETCH NEXT FROM @CURSOR INTO @EmployeeName
	 --------------------------------------------------
	 END
	 CLOSE @CURSOR
END

GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreatePermissionsForQry' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreatePermissionsForQry
GO
CREATE PROCEDURE csiRBACCreatePermissionsForQry(@RoleId CHAR(16), @SecurityList NVARCHAR(MAX), @PermissionModesFlag INT)
AS
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
    DECLARE @ObjectMetaId INT
    DECLARE @PermissionType INT
    DECLARE @PermissionName VARCHAR(255)
BEGIN
   SET NOCOUNT ON;
   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 'Select CDO.CDODefId As ObjectMetaId ' +
								', CDO.SecurityTypeId as PermissionType ' +
								', Labels.LabelValue as PermissionName ' +
						  'From	CDODefinition CDO ' +
						  '		, Labels ' +
						  'Where	Labels.LabelId = CDO.DisplayNameLabelId ' +
						     'And		CDO.IsAbstract = 0 ' +
						     'And		CDO.SecurityTypeId In ('+ @SecurityList +' ) ' +
						   'ORDER By CDO.SecurityTypeId, CDO.CDOName ' +
                     ' FOR READ ONLY; OPEN @c1'
   EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
   FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   WHILE(@@fetch_status = 0)
   BEGIN
      EXEC csiRBACCreatePermission @RoleId, @PermissionName, @PermissionType, @ObjectMetaId, @PermissionModesFlag
      FETCH NEXT FROM @c1 INTO @ObjectMetaId,@PermissionType,@PermissionName
   END
   CLOSE @c1
   DEALLOCATE @c1
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACAssignRole' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACAssignRole
GO
CREATE PROCEDURE csiRBACAssignRole(@RoleId CHAR(16), @RoleDescription NVARCHAR(255), @EmployeeName NVARCHAR(255), @OrganizationName NVARCHAR(255), @Propagate INT)
AS
    DECLARE @CDODefId INT
    DECLARE @EmployeeId CHAR(16)
    DECLARE @OrgId CHAR(16)
    DECLARE @IID VARCHAR(16)
     DECLARE @RoleGID NVARCHAR(36)


BEGIN
    SET NOCOUNT ON;

    SELECT @EmployeeId=EmployeeId
    FROM Employee
    WHERE EmployeeName=@EmployeeName

    SET @OrgId=NULL
    IF NOT @OrganizationName IS NULL
        SELECT @OrgId=OrganizationId
        FROM Organization
        WHERE OrganizationName=@OrganizationName

    SET @CDODefId=7782

      EXEC csiPRDGetNextInstanceId @CDODefId,@IID OUTPUT
   set @RoleDescription = @RoleDescription + @EmployeeName
    EXEC csiCreateGUID @RoleDescription, @RoleGID OUTPUT
    INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (@RoleGID,@IID, @CDODefId, @RoleId, @EmployeeId, 0, @Propagate, @OrgId);
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
    
    DECLARE @RoleId VARCHAR(16)
    DECLARE @IID VARCHAR(16)
    DECLARE @SessionId VARCHAR(16)
    DECLARE @InstanceId VARCHAR(16)    
    DECLARE @PermissionSQL NVARCHAR(MAX)
    DECLARE @AdminPresent INT
    DECLARE @InSiteAdminPresent INT
    DECLARE @PermissionModesFlag_None INT
    DECLARE @PermissionModesFlag_ReadOnly INT
    DECLARE @PermissionModesFlag_NoSecAdmin INT    
        
BEGIN
    SET NOCOUNT ON;
    	
	SET @PermissionModesFlag_None = 0
    SET @PermissionModesFlag_ReadOnly = 1
    SET @PermissionModesFlag_NoSecAdmin = 2

	PRINT('Creating "DraftPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftPermissions','Draft Permissions Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DraftCamstarPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftCamstarPermissions','Draft Camstar Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DraftPLMPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DraftPLMPermissions','Draft PLM Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DEPCPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DEPCPermissions','Deployment Complete Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "DEPIPermissions" Role...'); 
	EXEC csiRBACCreateRole 'DEPIPermissions','Deployment Incomplete Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
	
	PRINT('Creating "PackageCreator" Role...');
	EXEC csiRBACCreateRole 'Package Creator','Package Creator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'Start Change Pkg', 120, 8500, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ChangeMgtWorkflow', 110, 8519, @PermissionModesFlag_ReadOnly
	
	PRINT('Creating "PackageOwner" Role...');
	EXEC csiRBACCreateRole 'Package Owner','Package Owner Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'UpdateChangePkg', 120, 8485, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDO Inquiry', 140, 7398, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ChangeMgtWorkflow', 110, 8519, @PermissionModesFlag_ReadOnly
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PLMApprovePackage', 120, 8582, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'RouteApproval', 120, 8567, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Routing Sheet Maint', 110, 7820, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None



	
	
	PRINT('Creating "Package Deployer" Role...'); 
	EXEC csiRBACCreateRole 'Package Deployer','Package Deployer Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Track Target Deployment', 120, 8507, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ChangeMgtWorkflow', 110, 8519, @PermissionModesFlag_ReadOnly
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	


	
	PRINT('Creating "Package Activator" Role...'); 
	EXEC csiRBACCreateRole 'Package Activator','Package Activator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'ActivateChangePkg', 120, 8528, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Activation Inquiry', 140, 8554, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None    
	EXEC csiRBACCreatePermission @RoleId, 'Export/Import Controller', 160, 7392, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Import Status Inquiry', 140, 7397, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Modeling data Import', 150, 7391, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None

    
	PRINT('Creating "Package Approver" Role...');
	EXEC csiRBACCreateRole 'Package Approver','Package Approver Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None 
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ChangeMgtWorkflow', 110, 8519, @PermissionModesFlag_ReadOnly   
	EXEC csiRBACCreatePermission @RoleId, 'SignatureApproval', 120, 8568, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	



	PRINT('Creating "Package Collaborator" Role...'); 
	EXEC csiRBACCreateRole 'Package Collaborator','Package Collaborator Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'Change Mgt Workflow Maint', 110, 8519, @PermissionModesFlag_ReadOnly 
	EXEC csiRBACCreatePermission @RoleId, 'AssignChangePkgContent', 120, 8524, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDO Inquiry', 140, 7398, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Change Package Modeling Inquiry', 140, 8599, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'ContentChangeHistoryInquiry', 140, 8628, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CDOInstanceInfoInquiry', 140, 8633, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetImpactDetailsInquiry', 140, 8634, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AttachDocument', 120, 8573, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DocumentMaint', 110, 5620, @PermissionModesFlag_None
	

	
	
	PRINT('Creating "PDPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PDPermissions','Pending Deployment Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'DeployChangePkg', 120, 8526, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None

	PRINT('Creating "RejectPermissions" Role...'); 
	EXEC csiRBACCreateRole 'RejectPermissions','Rejected Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
    
	PRINT('Creating "PACPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PACPermissions','Pending Approval Camstar Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'SignatureApproval', 120, 8568, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None
		
		
	PRINT('Creating "PAPLMPermissions" Role...'); 
	EXEC csiRBACCreateRole 'PAPLMPermissions','Pending Approval PLM Permissions Role',@RoleId OUTPUT
	EXEC csiRBACCreatePermission @RoleId, 'PLMApprovePackage', 120, 8582, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CancelApproval', 120, 8566, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveNonStdChangePkg', 120, 8551, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'GetChangePackageDetails', 140, 8558, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'PackageInquiry', 140, 8547, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Approval Cycle Inquiry', 140, 8003, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'MoveStdChangePkg', 120, 8545, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateTaskInquiry', 140, 8680, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DelegateDateInquiry', 140, 8697, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatus', 120, 8709, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatus', 120, 8710, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'CloseCPStatuses', 120, 8725, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'VoidCPStatuses', 120, 8727, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatus', 120, 8735, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'OpenCPStatuses', 120, 8736, @PermissionModesFlag_None


	PRINT('Creating "Default Modeling Advanced" Role...'); 
	EXEC csiRBACCreateRole 'Default Modeling Advanced','Modeling Services for Advanced Users',@RoleId OUTPUT
	EXEC csiRBACAssignRoleIfExist 'Default Modeling', @RoleId, 'Modeling Services for Advanced Users', NULL, 0
    SET @PermissionSQL = '230'
    EXEC csiRBACCreatePermissionsForQry @RoleId, @PermissionSQL, @PermissionModesFlag_None
	EXEC csiRBACDeleteUnavailablePermissions 230, 'User Query Maint'
	EXEC csiRBACDeleteUnavailablePermissions 230, 'Business Rule Handler Maint'
	EXEC csiRBACDeleteUnavailablePermissions 230, 'Business Rule Maint'
	EXEC csiRBACDeleteUnavailablePermissions 230, 'Scheduled Business Rule Maint'
	EXEC csiRBACDeleteUnavailablePermissions 230, 'Summary Table Def Maint'

	PRINT('Assign "User Query Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'User Query Maint', 230, 7069, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Business Rule Handler Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Business Rule Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Scheduled Business Rule Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, @PermissionModesFlag_ReadOnly
	PRINT('Assign "Summary Table Def Maint" Permission...');
	EXEC csiRBACAssignPermissionToRole 'Default Modeling', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly
	EXEC csiRBACAssignPermissionToRole 'Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, @PermissionModesFlag_ReadOnly


	PRINT('Creating "Portal Configuration" Role...');
    EXEC csiRBACCreateRole 'Portal Configuration','Portal Configuration Role',@RoleId OUTPUT
    IF (@AdminPresent=1)
    BEGIN
        EXEC csiRBACAssignRole @RoleId,'Portal Configuration Role', 'Administrator', NULL, 0
    END
    EXEC csiRBACAssignRole @RoleId,'Portal Configuration Role', 'CamstarAdmin', NULL, 0
    EXEC csiRBACCreatePermission @RoleId, 'Configurator', 210, 1, @PermissionModesFlag_None
    EXEC csiRBACCreatePermission @RoleId, 'Portal Studio', 210, 2, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'Portal Studio RBAC', 210, 3, @PermissionModesFlag_None

	
    
	PRINT('Creating Approval Decisions');
	EXEC csiCMCreateApprovalDecision 'Approved',10,@ApprovalDecisionId OUTPUT
	EXEC csiCMCreateApprovalDecision 'Rejected',20,@ApprovalDecisionId OUTPUT

	EXEC csiCMCreateIDControl 'ExportImportTarget',0
	EXEC csiCMCreateIDControl 'CPFileSequence',0

	PRINT('Creating Draft...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
   
	PRINT('Creating Draft...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft Camstar',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
   
   PRINT('Creating Draft...');
	EXEC csiCMCreateSpec 'Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft PLM',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
  

    PRINT('Creating Deployment Complete');
    EXEC csiCMCreateSpec 'Deployment_Complete.png','Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.','Deployment Complete',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
    
    PRINT('Creating Deployment Incomplete');
	EXEC csiCMCreateSpec 'Deployment_InComplete.png','Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.','Deployment Incomplete',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
       
	PRINT('Creating Pending Approval Camstar');
	EXEC csiCMCreateSpec 'Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval Camstar',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
   
   PRINT('Creating Pending Approval PLM');
	EXEC csiCMCreateSpec 'Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval PLM',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
   
   
    PRINT('Creating Pending Deployment...');
	EXEC csiCMCreateSpec 'Open.png','In the Pending Deployment step the Package can be manually deployed to one or more remote targets, changing the status to Deployment Complete or Deployment Incomplete, if deployment fails. If deployment fails, user can redeploy or Close the Package','Pending Deployment',@CMSpecId OUTPUT,@CMSpecBaseId OUTPUT
       
       PRINT('Creating Rejected...');
	EXEC csiCMCreateSpec 'Rejected.png','In the Rejected step the package can be Voided or go back to Draft.','Rejected',@CMSpecId OUTPUT, @CMSpecBaseId OUTPUT
    
       
    PRINT('Creating Workflow...');
	EXEC csiCMCreateWorkFlow 'No Approval','No Approval', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
     
	PRINT('Creating Workflow Camstar...');
	EXEC csiCMCreateWFCamstarPLM 'Camstar','Camstar', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
     
    PRINT('Creating Workflow PLM...');
	EXEC csiCMCreateWFCamstarPLM 'PLM','PLM', @CMWrkFlowBaseCDODefId OUTPUT, @CMWrkFlowCDODefId OUTPUT, @CMWrkFlowStepCDODefId OUTPUT, @CMPathCDODefId OUTPUT
     
     
    PRINT('Creating Business Rule Export...');
	EXEC csiCMCreateBusinessRule 'This script will process exports associated with a change packages.','BR_DEPLOY','BRH_DEPLOY','SBR_DEPLOY','ExecuteQueryEX("ChangePackage_GetExportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ExportImportName", 1,Transaction::__Const.DataType.String, CLF::ExportNames, 0);ForEach(CLF::ExportName,CLF::ExportNames){InitQueryParametersEx("ExportName",CLF::ExportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByExportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeployment,ExportName,CLF::ExportName);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Deploy...');
	EXEC csiCMCreateBusinessRule 'This script will process exports associated with a change packages.','BR_DEPLOYSTATUS','BRH_DEPLOYSTATUS','SBR_DEPLOYSTATUS','ExecuteQueryEX("ChangePackage_GetDeploymentsInQueue", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"Name", 1,Transaction::__Const.DataType.String, CLF::PackageNames, 0);ForEach(CLF::ChangePackageName,CLF::PackageNames){CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeploymentInQueue);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Import...');
	EXEC csiCMCreateBusinessRule 'This script will process imports (activations) associated with a change packages.','BR_ACTIVATION','BRH_ACTIVATION','SBR_ACTIVATION', 'ExecuteQueryEX("ChangePackage_GetImportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ImportSetName", 1,Transaction::__Const.DataType.String, CLF::ImportNames, 0);ForEach(CLF::ImportName,CLF::ImportNames){InitQueryParametersEx("ImportName",CLF::ImportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByImportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessActivation,ImportSetName,CLF::ImportName);}}',0,5,6,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT
	
	PRINT('Creating Business Rule Notifications...');
	EXEC csiCMCreateBusinessRule 'This script will support email notifications for Change Package.','BR_NOTIFICATIONS','BRH_NOTIFICATIONS','SBR_NOTIFICATIONS', 'if(CLF::__CDOID.SessionValues and CLF::__CDOID.SessionValues.Factory){Call(CLF::__CDOID.SessionValues.Factory,SendReminderEmails);}',1,NULL,NULL,'4',
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT

	PRINT('Creating RPT Control Loop Limits Business Rule...');
	EXEC csiCMCreateBusinessRule 'This script will run the RPT Control Loop Limits Calculations.','BR_RPTControlLimits','BRH_RPTControlLimits_Monthly', 'SBR_RPTControlLimits', 'CLF::RPTControlLimitsUpdateObject = null;CreateCDO("RPTControlLimitsUpdate", false, false, CLF::RPTControlLimitsUpdateObject);CLF::RPTControlLimitsUpdateObject.RPTUpdateOccurrencePattern = 2;CLF::RPTControlLimitsUpdateObject.NoOfRPTCombinations = 100;CLF::RPTControlLimitsUpdateObject.HistoryTimePeriod = 30;CLF::RPTControlLimitsUpdateObject.Factory = CLF::__CDOID.SessionValues.Factory;Call(CLF::RPTControlLimitsUpdateObject, ProcessRPTControlLimits);',0,NULL,NULL,NULL,
	@BusinessRuleId OUTPUT, @SchedBusinessRuleId OUTPUT


--- Change Management Actions

	EXEC createActionRule 'CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True';
	EXEC createActionRule 'CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True or IsCollaborator = True';
	EXEC createActionRule 'CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided';
	EXEC createActionRule 'CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = True';
	EXEC createActionRule 'CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed';
	EXEC createActionRule 'CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers';
	EXEC createActionRule 'CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers';
	EXEC createActionRule 'CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed';
	EXEC createActionRule 'CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated';
	EXEC createActionRule 'CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1';
	EXEC createActionRule 'CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated';

	EXEC createAction 'CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305, 0
	
	EXEC createAction 'CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMCloseImpPackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'CloseSingleActivation_VP', NULL, 0, 3, 'CloseCPImportStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidImpPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidSingleActivation_VP', NULL, 0, 4, 'VoidCPImportStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMCloseImpPackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'CloseMultiActivation_VP', NULL, 0, 5, 'CloseCPImportStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidImpPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidMultiActivation_VP', NULL, 0, 6, 'VoidCPImportStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0

	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMActivatePackageAction'
	EXEC addActionRuleToActionDef 'CMActivatePackageRule', 'CMActivatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction'
	
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMCollaboratorRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsAssignApprovalRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsApprovePLMRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMCancelApprovalRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMRouteApprovalRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMPackageDetailsAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMDeployPackageAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMContentHistoryAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMClosePackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMVoidPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMActivationImpactAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMOpenPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction'

	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1'
	
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6'
	
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD'
	
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8'



    PRINT('Complete.');
END
GO
EXEC CMPopulateDefaultData
GO

