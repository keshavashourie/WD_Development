--------------------------------------------------------------------------------
-- SCRIPT:CMDefaultDataUpdate.sql
-- DESCR: Creates stored procedures used to update change management related
-- speecs, busniess rules, paths  and workflows from previous environments
--Copyright Siemens 2025 


/****** Script for SelectTopNRows command from SSMS  ******/


IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCreateGUID' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCreateGUID
GO

CREATE PROCEDURE csiCreateGUID(
@PermissionName		VARCHAR(255),
@RoleGID			VARCHAR(36) OUTPUT)
AS
DECLARE @CDODefId	INT
DECLARE @EmployeeId	CHAR(16)
DECLARE @OrgId		CHAR(16)
DECLARE @IID		VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

	SELECT @RoleGID =	
	(      SubString(ExportImportKeyGUID, 1, 8) +
	'-' + SubString(ExportImportKeyGUID, 9, 4) +
	'-' + SubString(ExportImportKeyGUID, 13, 4) +
	'-' + SubString(ExportImportKeyGUID, 17, 4) +
	'-' + SubString(ExportImportKeyGUID, 21,12))
	FROM 
	(
		SELECT upper(convert(nvarchar(36),hashbytes('MD5',@PermissionName),2)) AS ExportImportKeyGUID
	) c;
END
GO



IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateSpec' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateSpec
GO

CREATE PROCEDURE csiCMCreateSpec(
@StepIcon			NVARCHAR(512), 
@RoleDescription	NVARCHAR(255),
@CMSpecName			NVARCHAR(30), 
@InstanceId			VARCHAR(16) OUTPUT, 
@InstanceId1		VARCHAR(16) OUTPUT)
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
	DECLARE @BusinessProcessSpecBaseId VARCHAR(16)

    EXEC csiPRDGetNextInstanceId @CMSpecCDODefId,@InstanceId OUTPUT
    EXEC csiPRDGetNextInstanceId @CMSpecBaseCDODefId,@InstanceId1 OUTPUT
  
    SELECT @BusinessProcessSpecBaseId =[BusinessProcessSpecBaseId]  
	FROM [BusinessProcessSpecBase] 
	WHERE BusinessProcessSpecName = @CMSpecName
    
	IF NOT EXISTS (SELECT * FROM [BusinessProcessSpec] WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId)
    BEGIN
		IF NOT EXISTS (SELECT * FROM [BusinessProcessSpecBase] WHERE BusinessProcessSpecName = @CMSpecName)
		BEGIN
			IF @CMSpecName = 'Draft Camstar'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals]
				,[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',1,1,0,0,1)
			ELSE IF @CMSpecName = 'Draft PLM'
           		INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,1,0,0,1)
			ELSE IF	@CMSpecName = 'Draft'
           		INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,0,0,1)
			ELSE IF @CMSpecName = 'Pending Approval Camstar'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1,1)
			ELSE IF @CMSpecName = 'Pending Approval PLM'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,1,1)
			ELSE IF @CMSpecName = 'Pending Deployment'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,1)
			ELSE IF @CMSpecName = 'Deployment Incomplete'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,2)
			ELSE IF @CMSpecName = 'Deployment Complete'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,2)
			ELSE IF @CMSpecName = 'Rejected'
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,3)
			ELSE 
				INSERT 
				INTO [BusinessProcessSpec]
				([BusinessProcessSpecBaseId],[BusinessProcessSpecId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
				,[ECO],[IconId],[IsFrozen],[Notes],[Revision],[Status],[StepIcon],[WIPMsgDefMgrId],[AssignApprovers],[ResetApprovals],[LockInstances],[RequiresApproval],[PackageStatus])
				VALUES
				(@InstanceId1,@InstanceId,@CMSpecCDODefId,1,NULL,@RoleDescription,NULL,NULL,'',NULL,1,1,@StepIcon,'',0,0,1,0,0)
       END
	END
	ELSE
	BEGIN
		UPDATE [BusinessProcessSpec]
		SET [StepIcon] = @StepIcon
		WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId

		IF @CMSpecName = 'Draft Camstar'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers] =1,[ResetApprovals]=1,[LockInstances]=0,[RequiresApproval]=0,[PackageStatus]=1
			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Draft PLM'
			Update [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=1,[LockInstances]=0,[RequiresApproval]=0,[PackageStatus]=1
      			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Draft'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=0,[RequiresApproval]=0,[PackageStatus]=1
       	  	WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Pending Approval Camstar'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=1,[PackageStatus]=1
       	    WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Pending Approval PLM'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=1,[PackageStatus]=1
    	   	WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Pending Deployment'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=0,[PackageStatus]=1
			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Deployment Incomplete'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=0,[PackageStatus]=2
			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Deployment Complete'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=0,[PackageStatus]=2
			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE IF @CMSpecName = 'Rejected'
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=0,[PackageStatus]=3
			WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
		ELSE 
			UPDATE [BusinessProcessSpec]
			SET [AssignApprovers]=0,[ResetApprovals]=0,[LockInstances]=1,[RequiresApproval]=0,[PackageStatus]=0
		   	WHERE [BusinessProcessSpecBaseId] = @BusinessProcessSpecBaseId
	END
           
	IF NOT EXISTS (SELECT * FROM [BusinessProcessSpecBase] WHERE BusinessProcessSpecName = @CMSpecName)
	BEGIN
		INSERT 
		INTO [BusinessProcessSpecBase]
		([BusinessProcessSpecBaseId],[BusinessProcessSpecName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
		VALUES
		(@InstanceId1,@CMSpecName,@CMSpecBaseCDODefId,1,NULL,@InstanceId)
	END
	ELSE
	BEGIN
		SELECT @BusinessProcessSpecBaseId =[BusinessProcessSpecBaseId]  
		FROM [BusinessProcessSpecBase] 
		WHERE BusinessProcessSpecName = @CMSpecName

		PRINT('[BusinessProcessSpecBase] ' + @CMSpecName + ' already exists');
	END
           
	IF @CMSpecName ='Draft' 
		SELECT @RoleId = RoleId 
		FROM [RoleDef] 
		WHERE RoleName = 'DraftPermissions'
	ELSE
		SELECT @RoleId = RoleId 
		FROM [RoleDef] 
		WHERE Description like @CMSpecName+'%'
     
	BEGIN TRY
		INSERT 
		INTO [ChangeMgtSpecAllowableRoles]
		([AllowableRolesId],[BusinessProcessSpecId],[FieldId],[Sequence])
		VALUES
		(@RoleId,@InstanceId,@CMFieldRoleDefId,1)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
END
GO
           



IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateWorkFlow' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateWorkFlow
GO

CREATE PROCEDURE csiCMCreateWorkFlow(
@WFDescription	NVARCHAR(255),
@WFName			NVARCHAR(30), 
@InstanceId2	VARCHAR(16) OUTPUT, 
@InstanceId3	VARCHAR(16) OUTPUT, 
@InstanceId4	VARCHAR(16) OUTPUT, 
@InstanceId5	VARCHAR(16) OUTPUT)
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

    DECLARE @CMDRAFT CHAR(16), @CMDI CHAR(16), @CMDC CHAR(16), @CMCL CHAR(16), @SpecBaseId CHAR(16), @CMPD CHAR(16), @SpecBaseDescription NVARCHAR(255)
    
    EXEC csiPRDGetNextInstanceId @CMWrkFlowBaseCDODefId,@InstanceId2 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowCDODefId,@InstanceId3 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
     
	 BEGIN TRY      
		INSERT 
		INTO [BusinessProcessWorkflow]
		([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
		,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
		VALUES
		(@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
           
	IF NOT EXISTS (SELECT * FROM [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName)
    BEGIN
		INSERT 
		INTO [BusinessProcessWorkflowBase]
        ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
		VALUES
		(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
    END
    ELSE
		PRINT('[[BusinessProcessWorkflowBase]] ' + @WFName + ' already exists');
          
    SET @CMDRAFT = @InstanceId4;
    
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Draft' 
    
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId  
    
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
		,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
		,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,@SpecBaseId,'0000000000000000',1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft',45,214)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH	
	
	UPDATE [BusinessProcessWorkflow] 
	SET FirstStepId = @InstanceId4	
	WHERE BusinessProcessWorkflowId = @InstanceId3
					
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDI = @InstanceId4;

	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Deployment Incomplete'

	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId  
	 
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,0,NULL,NULL,1,@SpecBaseId,'0000000000000000',1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Incomplete',179,77)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDC = @InstanceId4;
	
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Deployment Complete' 
	
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId  
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,@SpecBaseId,'0000000000000000',1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Complete',212,205)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
				
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
		
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,@InstanceId5,'Deployment Complete_2',NULL, NULL,@CMDC,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Deployment Complete' AND WorkflowId = @InstanceId3
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
        
	BEGIN TRY   
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,@InstanceId5,'Deployment Incomplete',NULL, NULL,@CMDI,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
           
	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [PathSelector]
        ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],[Expression])
		VALUES
        ('245DF453-8CAB-4592-916D-50180F1A4723',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMDC,1,0,NULL,'TrackableObject.DeploymentFailed')
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
         
	BEGIN TRY
		INSERT 
		INTO [WorkflowStepPathSelectors]
        ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
		VALUES
        (4403,@CMPathSelectorInstanceId,1,@CMDC)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
		     
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
        
	BEGIN TRY  
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDI,0,NULL,@InstanceId5,'Deployment Complete_1',NULL, NULL,@CMDC,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Deployment Incomplete' AND WorkflowId = @InstanceId3 

    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
          
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDraft,0,NULL,@InstanceId5,'Deployment Complete',NULL, NULL,@CMDC,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Draft' AND WorkflowId = @InstanceId3  
END
GO



IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateWFCamstarPLM' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateWFCamstarPLM
GO

CREATE PROCEDURE csiCMCreateWFCamstarPLM(
@WFDescription	NVARCHAR(255),
@WFName			NVARCHAR(30), 
@InstanceId2	VARCHAR(16) OUTPUT, 
@InstanceId3	VARCHAR(16) OUTPUT, 
@InstanceId4	VARCHAR(16) OUTPUT, 
@InstanceId5	VARCHAR(16) OUTPUT)
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
    
    DECLARE @CMDRAFT CHAR(16), @CMDI CHAR(16), @CMDC CHAR(16), @CMCL CHAR(16), @SpecBaseId CHAR(16),@CMVOID CHAR(16), @CMREJ CHAR(16), @CMPD CHAR(16), @CMPA CHAR(16)
    DECLARE @CMDRAFTName NVARCHAR(255), @SpecBaseDescription NVARCHAR(255)

    EXEC csiPRDGetNextInstanceId @CMWrkFlowBaseCDODefId,@InstanceId2 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowCDODefId,@InstanceId3 OUTPUT
    EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
      
	BEGIN TRY     
		INSERT 
		INTO [BusinessProcessWorkflow]
        ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowId],[CDOTypeId],[ChangeCount],[ChangeHistoryId],[Description]
        ,[ECO],[FirstStepId],[IconId],[IsFrozen],[Notes],[Revision],[Status],[WIPMsgDefMgrId])
		VALUES
        (@InstanceId2,@InstanceId3,@CMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL)
    END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
	   
    IF NOT EXISTS (SELECT * FROM [BusinessProcessWorkflowBase] WHERE BusinessProcessWorkflowName = @WFName)
    BEGIN
		INSERT 
		INTO [BusinessProcessWorkflowBase]
        ([BusinessProcessWorkflowBaseId],[BusinessProcessWorkflowName],[CDOTypeId],[ChangeCount],[IconId],[RevOfRcdId])
		VALUES
		(@InstanceId2,@WFName,@CMWrkFlowBaseCDODefId,1,NULL,@InstanceId3)
    END
    ELSE
		PRINT('[BusinessProcessWorkflowBase] ' + @WFName + ' already exists');
     
    IF @WFDescription = 'No Approval'
		SET @CMDRAFTName = 'Draft' 
	ELSE IF  @WFDescription = 'Camstar'
		SET @CMDRAFTName = 'Draft Camstar' 
    ELSE IF  @WFDescription = 'PLM'
		SET @CMDRAFTName = 'Draft PLM' 
     
    SET @CMDRAFT = @InstanceId4;
    
	select @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = @CMDRAFTName
    
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId 
    
    IF @WFDescription = 'No Approval'
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft',37,24)
	ELSE IF  @WFDescription = 'Camstar'
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft Camstar',37,24)
	ELSE IF  @WFDescription = 'PLM'
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Draft PLM',37,24)
			
	UPDATE [BusinessProcessWorkflow] 
	SET FirstStepId = @InstanceId4	
	WHERE BusinessProcessWorkflowId = @InstanceId3
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMPA = @InstanceId4;

	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Pending Approval '+@WFDescription

	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId 

	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
		,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
		,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,0,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Pending Approval',35,137)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMPD = @InstanceId4;
	
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Pending Deployment'  
	
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId 
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,0,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Pending Deployment',32,248)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMREJ = @InstanceId4;
	
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Rejected'  
	
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId 
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,1,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Rejected',180,135)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH	
	
			
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDC = @InstanceId4;
	
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Deployment Complete'  
	
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId 
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,0,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Complete',227,365)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
	
	EXEC csiPRDGetNextInstanceId @CMWrkFlowStepCDODefId,@InstanceId4 OUTPUT
	SET @CMDI = @InstanceId4;
	
	SELECT @SpecBaseId = [BusinessProcessSpecBaseId] 
	FROM [BusinessProcessSpecBase] 
	WHERE [BusinessProcessSpecName] = 'Deployment Incomplete' 
	
	SELECT @SpecBaseDescription = Description 
	FROM [BusinessProcessSpec] 
	WHERE [BusinessProcessSpecBaseId] = @SpecBaseId  
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStep]
		([CDOTypeId],[ChangeCount],[DefaultPathId],[Description],[IconId],[IsFrozen],[IsLastStep],[Notes],[OnDefaultRoute]
        ,[RouteStepId],[SchedulingDetailId],[Sequence],[SpecBaseId],[SpecId],[StepType]
        ,[SubWorkflowBaseId],[SubWorkflowId],[WIPMsgLabel],[WorkflowId],[WorkflowStepId],[WorkflowStepName],[Xlocation],[Ylocation])
		VALUES
		(@CMWrkFlowStepCDODefId,1,NULL,@SpecBaseDescription,NULL,0,0,NULL,0,NULL,NULL,1,'0000000000000000',@SpecBaseId,1, 
		NULL,NULL,NULL,@InstanceId3,@InstanceId4,'Deployment Incomplete',391,254)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
			
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
	
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDRAFT,0,NULL,@InstanceId5,'Pending Approval',NULL, NULL,@CMPA,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

	UPDATE [WorkflowStep] 
	SET DefaultPathId =  @InstanceId5 
	WHERE WorkflowStepName = @CMDRAFTName AND WorkflowId = @InstanceId3	
           
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,@InstanceId5,'Pending Deployment',NULL, NULL,@CMPD,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Pending Approval' AND WorkflowId = @InstanceId3
           
	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [PathSelector]
        ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],[Expression])
    VALUES
           ('F3A54DBA-568A-464A-801F-E0B9FEA4552D',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Approved')
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

	BEGIN TRY
		INSERT 
		INTO [WorkflowStepPathSelectors]
        ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
    VALUES
		(4403,@CMPathSelectorInstanceId,1,@CMPA)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,@InstanceId5,'Rejected',NULL, NULL,@CMREJ,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
       
	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    
	BEGIN TRY
			INSERT 
			INTO [PathSelector]
           ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],
           [Expression])
    VALUES
           ('32F00D0C-4486-470B-98ED-FB1EB2AC0AE2',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Rejected')
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

    BEGIN TRY
		INSERT 
		INTO [WorkflowStepPathSelectors]
        ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
		VALUES
        (4403,@CMPathSelectorInstanceId,2,@CMPA)      
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH     
    
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMPA,0,NULL,@InstanceId5,'Draft',NULL, NULL,@CMDRAFT,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT

    BEGIN TRY
		INSERT 
		INTO [PathSelector]
        ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],[Expression])
		VALUES
        ('1C7AF084-F98F-4825-A0EF-6D71891E84E6',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMPA,1,0,NULL,
        'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Pending or TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Cancelled')
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
	
	BEGIN TRY
		INSERT 
		INTO [WorkflowStepPathSelectors]
        ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
		VALUES
        (4403,@CMPathSelectorInstanceId,3,@CMPA)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
      
	EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMPD,0,NULL,@InstanceId5,'Deployment Complete',NULL, NULL,@CMDC,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Pending Deployment' AND WorkflowId = @InstanceId3 
     
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,@InstanceId5,'Deployment Complete_2',NULL, NULL,@CMDC,NULL)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Deployment Complete' AND WorkflowId = @InstanceId3   
      
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDC,0,NULL,@InstanceId5,'Deployment Incomplete',NULL, NULL,@CMDI,NULL)    
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH  
	 
	EXEC csiPRDGetNextInstanceId @CMPathSelectorCDODefId,@CMPathSelectorInstanceId OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [PathSelector]
        ([ExportImportKey],[CDOTypeId],[ChangeCount],[Description],[PathSelectorId],[PathId],[StepId],[Status],[IsFrozen],[Notes],[Expression])
		VALUES
        ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',@CMPathSelectorCDODefId,1,NULL,@CMPathSelectorInstanceId,@InstanceId5,@CMDC,1,0,NULL,'TrackableObject.DeploymentFailed')
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
	
    BEGIN TRY
		INSERT 
		INTO [WorkflowStepPathSelectors]
        ([FieldId],[PathSelectorsId],[Sequence],[WorkflowStepId])
		VALUES
        (4403,@CMPathSelectorInstanceId,1,@CMDC)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
		         
    EXEC csiPRDGetNextInstanceId @CMPathCDODefId,@InstanceId5 OUTPUT
    
	BEGIN TRY
		INSERT 
		INTO [Path]
        ([CDOTypeId],[ChangeCount],[Description],[EndReworkStepId],[FromStepId],[IsFrozen],[Notes]
        ,[PathId],[PathName],[ReEntryStepId],[ReturnToStepId],[ToStepId],[TxnDetailsId])
		VALUES
        (@CMPathCDODefId,1,NULL,NULL,@CMDI,0,NULL,@InstanceId5,'Deployment Complete_1',NULL, NULL,@CMDC,NULL) 
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH
    
	UPDATE [WorkflowStep] 
	SET DefaultPathId = @InstanceId5 
	WHERE WorkflowStepName = 'Deployment Incomplete' AND WorkflowId = @InstanceId3       
END
GO





IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateBusinessRule' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateBusinessRule
GO

CREATE PROCEDURE csiCMCreateBusinessRule(
@BRDescription			NVARCHAR(255),
@BRName					NVARCHAR(30), 
@BRHName				NVARCHAR(30), 
@SBRName				NVARCHAR(30), 
@BRSCRIPT				NVARCHAR(4000), 
@SBRIsAdvancedMode		BIT, 
@SBRRecurrenceFrequency INT, 
@SBRRecurrencePattern	INT, 
@SBRScheduleHours		NVARCHAR(255),
@InstanceId1			VARCHAR(16) OUTPUT, 
@InstanceId2			VARCHAR(16) OUTPUT)
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
    BEGIN  
		INSERT 
		INTO BusinessRuleHandler
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
		(@BRHandlerDataInstanceId
		,@BRHandlerInstanceId
		,@BRHName
		,@BRHandlerCDODefId
		,1
		,NULL
		,@BRDescription
		,NULL
		,0
		,1
		,NULL
		,0)
		
		BEGIN TRY
			INSERT 
			INTO BusinessRuleHandlerData
			([BizRuleHandlerType]
			,[BusinessRuleHandlerDataId]
			,[BusinessRuleHandlerDataName]
			,[BusinessRuleHandlerId]
			,[CDOTypeId]
			,[ChangeCount]
			,[IsFrozen]
			,[Script]
			,[ServiceType])
			VALUES
			(1
			,@BRHandlerDataInstanceId
			,@BRHName
			,@BRHandlerInstanceId
			,@BRHandlerDataCDODefId
			,1
			,0
			,@BRScript
			,NULL)
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
    END
    ELSE
		PRINT('[BusinessRuleHandler] ' + @BRHName + ' already exists');

	
     
    IF NOT EXISTS (SELECT * FROM BusinessRule WHERE BusinessRuleName = @BRName)
    BEGIN 
		INSERT 
		INTO BusinessRule
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
		VALUES
		(@BRDataInstanceId
		,@InstanceId1
		,@BRName
		,@BRCDODefId
		,1
		,NULL
		,@BRDescription
		,NULL
		,0
		,NULL)
		
		BEGIN TRY
			INSERT 
			INTO BusinessRuleData
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
			VALUES
			(1
			,NULL
			,@BRDataInstanceId
			,@BRName
			,@InstanceId1
			,@BRDataCDODefId
			,1
			,1140
			,0
			,0)
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
	END
	ELSE
		PRINT('[BusinessRule] ' + @BRName + ' already exists');

		
			
	BEGIN TRY
		INSERT 
		INTO BusinessRuleDataHandlers
        ([BusinessRuleDataId]
        ,[FieldId]
        ,[HandlersId]
        ,[Sequence])
		VALUES
		(@BRDataInstanceId
		,13314
		,@BRHandlerInstanceId
		,1)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() NOT IN (2627, 2601)
			THROW;
	END CATCH

    DECLARE @StartDate DATETIME, @StartDateGMT DATETIME, @DueDate DATETIME, @DueDateGMT DATETIME
	SET @StartDate = GETDATE()
	SET @StartDateGMT = GETUTCDATE()
    
	IF (@SBRIsAdvancedMode = 1)
    BEGIN
		SET @DueDate = CASE WHEN DATEPART(hh,GETDATE()) > 4 THEN DATEADD(hh,4,CAST(CAST(DATEADD(dd,1,GETDATE()) AS date) AS datetime)) ELSE DATEADD(hh,4,CAST(CAST(GETDATE() AS date) AS datetime)) END
		SET @DueDateGMT = DATEADD(minute, DATEDIFF(minute, GETDATE(), GETUTCDATE()), CASE WHEN DATEPART(hh,GETDATE()) > 4 THEN DATEADD(hh,4,CAST(CAST(DATEADD(dd,1,GETDATE()) AS date) AS datetime)) 
		ELSE DATEADD(hh,4,CAST(CAST(GETDATE() AS date) AS datetime)) END)
	END
	ELSE
	BEGIN
		SET @DueDate = @StartDate
		SET @DueDateGMT = @StartDateGMT
	END

	IF NOT EXISTS (SELECT * FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = @SBRName)
    BEGIN 
		INSERT 
		INTO ScheduledBusinessRule
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
		VALUES
		(@SchedBRCDODefId
		,1
		,NULL
		,NULL
		,NULL
		,NULL
		,@DueDate
		,@DueDateGMT
		,NULL
		,NULL
		,'0004740000000001'
		,1140
		,NULL
		,@SBRIsAdvancedMode
		,0
		,0
		,0
		,NULL
		,NULL,NULL
		,@InstanceId1,NULL
		,@SBRRecurrenceFrequency
		,@SBRRecurrencePattern
		,@InstanceId2
		,@SBRName
		,@SBRScheduleHours
		,@StartDate
		,@StartDateGMT
		,1)
	END
    ELSE
		PRINT('[ScheduledBusinessRule] ' + @SBRName + ' already exists');
END
GO



IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACCreateRole' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACCreateRole
GO

CREATE PROCEDURE csiRBACCreateRole(
@RoleName					NVARCHAR(50), 
@RoleDescription			NVARCHAR(255), 
@InstanceId					VARCHAR(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleCDODefId	INT

    SET @RoleCDODefId=7130

    EXEC csiPRDGetNextInstanceId @RoleCDODefId,@InstanceId OUTPUT
	
	IF NOT EXISTS (SELECT * FROM RoleDef WHERE RoleName = @RoleName)
    BEGIN
		INSERT 
		INTO RoleDef
		(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
		VALUES 
		(@InstanceId, @RoleCDODefId, NULL, 1, @RoleDescription, NULL, 0, 0, @RoleName);
    END
    ELSE
	BEGIN
		PRINT('[RoleDef] ' + @RoleName + ' already exists');

		SELECT @InstanceId = RoleId 
		FROM RoleDef 
		WHERE RoleName = @RoleName;
	END;
END
GO




IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiRBACCreatePermission' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiRBACCreatePermission
GO

CREATE PROCEDURE csiRBACCreatePermission(
@RoleId					CHAR(16), 
@PermissionName			VARCHAR(255), 
@PermissionType			INT, 
@ObjectMetaId			INT,
@PermissionModesFlag	INT, 
@ObjectInstanceId		CHAR(16) = NULL)
AS
--	@PermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

DECLARE @IID			VARCHAR(16)
DECLARE @RoleGID		VARCHAR(36)
BEGIN
    SET NOCOUNT ON;

    EXEC csiPRDGetNextInstanceId 7783,@IID OUTPUT
	
	IF NOT EXISTS (SELECT * FROM RolePermission WHERE RoleId = @RoleId and RolePermissionname = @PermissionName )
    BEGIN
		EXEC csiCreateGUID @PermissionName, @RoleGID OUTPUT

		INSERT 
		INTO RolePermission
		(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
		VALUES
		(@RoleGID,@IID,7783,@RoleId,1,@PermissionName,0,@ObjectMetaId,@PermissionType,@ObjectInstanceId);
	END;

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the @PermissionModesFlag value
	IF ( @PermissionModesFlag = 0 )
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
        SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
        FROM SecurityMaskDetail
        WHERE SecurityMaskId=@PermissionType
    ELSE IF ( @PermissionModesFlag = 1 AND @PermissionType IN (110, 180) )
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
        SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
        FROM SecurityMaskDetail
        WHERE SecurityMaskId=@PermissionType
        AND BitNumber = 2
    ELSE IF ( @PermissionModesFlag = 2 AND @PermissionType = 180 )
		INSERT 
		INTO RolePermissionModes
		(RolePermissionId, FieldId, Modes, Sequence)
        SELECT @IID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
        FROM SecurityMaskDetail
        WHERE SecurityMaskId=@PermissionType
        AND BitNumber IN (1,2,3,4)
	ELSE
		PRINT('Error - Invalid value passed for @PermissionModeFlag parameter...');	
END
GO




IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateApprovalDecision' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateApprovalDecision
GO

CREATE PROCEDURE csiCMCreateApprovalDecision(
@DecisionName		NVARCHAR(30), 
@DecisionType		INT, 
@InstanceId			VARCHAR(16) OUTPUT)
AS
BEGIN
	SET NOCOUNT ON;

	EXEC csiPRDGetNextInstanceId 7857,@InstanceId OUTPUT
  
	IF NOT EXISTS (SELECT * FROM ApprovalDecision WHERE ApprovalDecisionName = @DecisionName)
    BEGIN
		INSERT 
		INTO ApprovalDecision
        (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
		VALUES
        (@InstanceId,'001e850000000000',@DecisionName,7857,1,@DecisionType, 0,0)
	END
    ELSE
		PRINT('[ApprovalDecision] ' + @DecisionName + ' already exists');
END
GO




IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'csiCMCreateIDControl' 
	AND 	  type = 'P'
)
DROP PROCEDURE csiCMCreateIDControl
GO

CREATE PROCEDURE csiCMCreateIDControl(
@IDType			NVARCHAR(30), 
@NextId			INT)
AS
BEGIN
	SET NOCOUNT ON;
  
	IF NOT EXISTS (SELECT * FROM IDControl WHERE IDType = @IDType)
    BEGIN
		INSERT 
		INTO IDControl 
		(IDType,  NextID) 
        VALUES 
		(@IDType, @NextId); 
    END
    ELSE
		PRINT('[IDType] ' + @IDType + ' already exists');
END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createActionRule' 
	AND 	  type = 'P'
)
DROP PROCEDURE createActionRule
GO

CREATE PROCEDURE createActionRule(
@Name						NVARCHAR(30),
@Description				NVARCHAR(255),
@Expression					NVARCHAR(1000))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CDOTypeId		INT
	DECLARE @InstanceId		VARCHAR(16)

	IF NOT EXISTS (SELECT * FROM ActionRule WHERE ActionRuleName = @Name)
	BEGIN
		PRINT('Inserting ActionRule: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId 
		FROM CDODefinition 
		WHERE CDOName = 'ActionRule'	
			
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;

		INSERT 
		INTO ActionRule
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
		PRINT('ActionRule ' + @Name + ' already exists');
END
GO



--------------------------------------------------------------------------------
-- PROCEDURE: createActionCategory
-- DESCR: Helper function to create an Action Category record
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createActionCategory' 
	AND 	  type = 'P'
)
DROP PROCEDURE createActionCategory
GO

CREATE PROCEDURE createActionCategory(
@Name						NVARCHAR(30),
@LabelName					NVARCHAR(50),
@Sequence					INT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId		INT
	DECLARE @InstanceId		VARCHAR(16)

	IF NOT EXISTS (SELECT * FROM ActionCategory WHERE ActionCategoryName = @Name)
	BEGIN
		PRINT('Inserting ActionCategory: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId 
		FROM CDODefinition 
		WHERE CDOName = 'ActionCategory'
			
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT 
		INTO ActionCategory
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
		PRINT('ActionCategory ' + @Name + ' already exists');
END
GO



-----------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create ActionDef record
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createActionDef' 
	AND 	  type = 'P'
)
DROP PROCEDURE createActionDef
GO

CREATE PROCEDURE createActionDef(
@Name						NVARCHAR(30),
@Description				NVARCHAR(255),
@Type						INT, 
@InstanceId					VARCHAR(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId		INT

	SELECT @InstanceId = ActionId FROM ActionDef WHERE ActionName = @Name;
	
	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting Action: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId 
		FROM CDODefinition 
		WHERE CDOName = 'ActionDef'

		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT 
		INTO ActionDef
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
		PRINT('Action ' + @Name + ' already exists');
END
GO


	
-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createUIAction' 
	AND 	  type = 'P'
)
DROP PROCEDURE createUIAction
GO

CREATE PROCEDURE createUIAction(
@ActionId							CHAR(16),
@Name								NVARCHAR(30),
@Type								INT,
@Description						NVARCHAR(255),
@UIType								NVARCHAR(30),
@UIVirtualPageName					NVARCHAR(30),
@UIPageFlowName						NVARCHAR(30),
@MapItem							NVARCHAR(30),
@PortalTabOption					INT,
@ClearValues						BIT,
@ServiceName						NVARCHAR(30),
@LabelName							NVARCHAR(66),
@ShowButtons						BIT,
@IsPrimary							BIT,
@ActionCategoryName					NVARCHAR(30),
@Sequence							INT,
@Width								INT,
@Height								INT,
@ForceRedirect						BIT,
@InstanceId							VARCHAR(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId				INT	
	DECLARE @UIVirtualPageId		VARCHAR(16);
	DECLARE @UIPageFlowId			VARCHAR(16);
	DECLARE @ActionCategoryId		VARCHAR(16);
	DECLARE @FloatPageLocationId	VARCHAR(16);

	SELECT @InstanceId = UIActionId 
	FROM UIAction 
	WHERE UIActionName = @Name ;

	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting UIAction: ' + @Name);

		SET @UIVirtualPageId = NULL;
		IF ISNULL(@UIVirtualPageName,'') <> ''
			SELECT @UIVirtualPageId = UIVirtualPageId 
			FROM UIVirtualPage 
			WHERE UIVirtualPageName = @UIVirtualPageName;  
		
		SET @UIPageFlowId = NULL;
		IF ISNULL(@UIPageFlowName,'') <> ''
			SELECT @UIPageFlowId = UIPageFlowId 
			FROM UIPageFlow 
			WHERE UIPageFlowName = @UIPageFlowName;  
		
		SET @ActionCategoryId = NULL;
		IF ISNULL(@ActionCategoryName,'') <> ''
			SELECT @ActionCategoryId = ActionCategoryId 
			FROM ActionCategory 
			WHERE ActionCategoryName = @ActionCategoryName;  
		
		SELECT @CDOTypeId = CDODefId 
		FROM CDODefinition 
		WHERE CDOName = @UIType

		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT 
		INTO UIAction
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
			SELECT @CDOTypeId = CDODefId 
			FROM CDODefinition
			WHERE CDOName = 'UIFloatPageLocation';

			EXEC csiPRDGetNextInstanceId @CDOTypeId, @FloatPageLocationId OUTPUT;
        
			INSERT 
			INTO UIFloatPageLocation 
			(UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
			VALUES 
			(@FloatPageLocationId, @CDOTypeId, 1, 0, @InstanceId, @Width, @Height);
	    
			UPDATE UIAction 
			SET FrameLocationId = @FloatPageLocationId 
			WHERE UIActionId = @InstanceId;
		END
	END
	ELSE
	BEGIN
		UPDATE UIAction 
		SET ServiceName = @ServiceName, LabelName = @LabelName 
		WHERE UIActionId = @InstanceId;

		PRINT('UIAction ' + @Name + ' already exists. Update Service Name and Label.');
	END
END
GO


	
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'createAction' 
	AND 	  type = 'P'
)
DROP PROCEDURE createAction
GO

CREATE PROCEDURE createAction(
@Name						NVARCHAR(30),
@ActionCategoryName			NVARCHAR(30),
@Type						INT,
@Description				NVARCHAR(255),
@UIType						NVARCHAR(30),
@UIVirtualPageName			NVARCHAR(30),
@UIPageFlowName				NVARCHAR(30),
@ClearValues				BIT,
@Sequence					INT,
@ServiceName				NVARCHAR(30),
@LabelName					NVARCHAR(66),
@MapItem					NVARCHAR(30),
@PortalTabOption			INT,
@ShowButtons				BIT,
@IsPrimary					BIT,
@Width						INT,
@Height						INT,
@ForceRedirect				BIT)
AS
BEGIN
    SET NOCOUNT ON;

	/* It is currently unclear to me why the UIAction has a Name or Description */
	DECLARE @ActionId		VARCHAR(16);
	DECLARE @UIActionId		VARCHAR(16);
	
	EXEC CreateActionDef @Name, @Description, @Type, @ActionId OUTPUT

	EXEC createUIAction @ActionId,@Name,@Type,@Description,@UIType,@UIVirtualPageName,@UIPageFlowName, @MapItem,
		@PortalTabOption, @ClearValues, @ServiceName,@LabelName,@ShowButtons,@IsPrimary,@ActionCategoryName,@Sequence,@Width,@Height,@ForceRedirect,
		@UIActionId OUTPUT
			
	UPDATE ActionDef 
	SET UIActionId = @UIActionId 
	WHERE ActionId=@ActionId;
END
GO



-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'addActionRuleToActionDef' 
	AND 	  type = 'P'
	)
DROP PROCEDURE addActionRuleToActionDef
GO

CREATE PROCEDURE addActionRuleToActionDef(
@ActionRule					NVARCHAR(30), 
@Action						NVARCHAR(30))
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @FieldId		INT
	DECLARE @ActionId		VARCHAR(16)
	DECLARE @ActionRuleId	VARCHAR(16)
	DECLARE @Sequence		INT
	
	IF NOT EXISTS (	SELECT * FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = @Action AND r.ActionRuleName = @ActionRule
				   )
	BEGIN
		PRINT('Adding ActionRule ' + @ActionRule + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId 
		FROM ActionDef 
		WHERE ActionName = @Action;

		SELECT @ActionRuleId = ActionRuleId 
		FROM ActionRule 
		WHERE ActionRuleName = @ActionRule;

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
		
		SELECT @FieldID = f.FieldID 
		FROM CDOFields f 
		JOIN CDODefinition d ON d.CDODefID=f.CDODefID
		WHERE f.FieldName='ActionRules' AND d.CDOName='ActionDef';
		
		SELECT @Sequence = COUNT(*) + 1 
		FROM ActionDefActionRules 
		WHERE ActionId = @ActionId;
		
		BEGIN TRY
			INSERT 
			INTO ActionDefActionRules 
			(ActionId
			,ActionRulesId
			,FieldId
			,Sequence)
			VALUES
			(@ActionId           -- char(16)
			,@ActionRuleId		 -- char(16)
			,@FieldId			 -- int
			,@Sequence);         -- int
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the ActionRule ' + @ActionRule);
END
GO




-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'addSourcePageToActionDef' 
	AND 	  type = 'P'
)
DROP PROCEDURE addSourcePageToActionDef
GO

CREATE PROCEDURE addSourcePageToActionDef(
@VirtualPage					NVARCHAR(30),
@Action							NVARCHAR(30),
@ExportImportKey                NVARCHAR(36))
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @ActionId			VARCHAR(16)
	DECLARE @ActionRuleId		VARCHAR(16)
	DECLARE @CDOTypeId			INT
	DECLARE @InstanceId			VARCHAR(16);
	DECLARE @UIVirtualPageId	VARCHAR(16);
	
	
	IF NOT EXISTS (	SELECT * FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = @Action AND v.UIVirtualPageName = @VirtualPage
				   )
	BEGIN
		PRINT('Adding UISourcePage ' + @VirtualPage + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId 
		FROM ActionDef 
		WHERE ActionName = @Action;

		SELECT @UIVirtualPageId = UIVirtualPageId 
		FROM UIVirtualPage 
		WHERE UIVirtualPageName = @VirtualPage;  

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
		
		SELECT @CDOTypeId = CDODefId 
		FROM CDODefinition 
		WHERE CDOName ='UISourcePage';

		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;

		BEGIN TRY
			INSERT 
			INTO UISourcePage 
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
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() NOT IN (2627, 2601)
				THROW;
		END CATCH
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the UISourcePage ' + @VirtualPage);
END
GO



IF EXISTS 
(
	SELECT name 
	FROM   sysobjects 
	WHERE  name = 'CMPopulateDefaultData' 
	AND 	  type = 'P'
)
DROP PROCEDURE CMPopulateDefaultData
GO

CREATE PROCEDURE CMPopulateDefaultData
AS
DECLARE @CMSpecID						VARCHAR(16)   
DECLARE @CMSpecBaseID					VARCHAR(16)    
DECLARE @CMWrkFlowCDODefId				VARCHAR(16)    
DECLARE @CMWrkFlowBaseCDODefId			VARCHAR(16)    
DECLARE @CMWrkFlowStepCDODefId			VARCHAR(16)   
DECLARE @CMPathCDODefId					VARCHAR(16)    
DECLARE @BusinessRuleId					VARCHAR(16)    
DECLARE @SchedBusinessRuleId			VARCHAR(16)
DECLARE @ApprovalDecisionId				VARCHAR(16) 
DECLARE @RoleId							VARCHAR(16)
DECLARE @IID							VARCHAR(16)
DECLARE @SessionId						VARCHAR(16)
DECLARE @InstanceId						VARCHAR(16)    
DECLARE @PermissionSQL					NVARCHAR(MAX)
DECLARE @AdminPresent					INT
DECLARE @InSiteAdminPresent				INT
DECLARE @PermissionModesFlag_None		INT
DECLARE @PermissionModesFlag_ReadOnly	INT
DECLARE @PermissionModesFlag_NoSecAdmin	INT    
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
	EXEC csiRBACCreatePermission @RoleId, 'TrackTargetDeployment', 120, 8507, @PermissionModesFlag_None
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
	EXEC csiRBACCreatePermission @RoleId, 'TrackTargetDeployment', 120, 8507, @PermissionModesFlag_None
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
	EXEC csiRBACCreatePermission @RoleId, 'WhereUsedInquiry', 140, 8614, @PermissionModesFlag_None
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
	EXEC csiRBACCreatePermission @RoleId, 'Change Package Modeling Inquiry', 140, 8599, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'AssignSingleCPContent', 120, 8610, @PermissionModesFlag_None
	EXEC csiRBACCreatePermission @RoleId, 'DetachSingleCPContent', 120, 8611, @PermissionModesFlag_None
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
	EXEC csiRBACCreatePermission @RoleId, 'TrackTargetDeployment', 120, 8507, @PermissionModesFlag_None
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
	EXEC csiRBACCreatePermission @RoleId, 'ActivationInquiry', 140, 8554, @PermissionModesFlag_None
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
	EXEC createActionRule 'CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided';
	
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

