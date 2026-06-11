--------------------------------------------------------------------------------
-- SCRIPT:CMSpecDefaultData.sql
-- DESCR: Creates stored procedures used to create change management related
-- speecs, busniess rules, paths  and workflows. 
--Copyright Siemens 2023  

/****** Script for SelectTopNRows command from SSMS  ******/
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateSpec')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateSpec;
 	END IF;
END $$;
CREATE PROCEDURE csiCMCreateSpec(
	pStepIcon VARCHAR(512), 
	pRoleDescription VARCHAR(255),
	pCMSpecName VARCHAR(30), 
	OUT pInstanceId varchar(16), 
	OUT pInstanceId1 varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCMSpecCDODefId INTEGER := 8513;
	vCMSpecBaseCDODefId INTEGER := 8514;
	vCMWrkFlowCDODefId INTEGER := 8517;
	vCMWrkFlowBaseCDODefId INTEGER := 8518;
	vCMFieldRoleDefId INTEGER := 22504;
	vRoleId VARCHAR(50);
BEGIN

    CALL csiPRDGetNextInstanceId(vCMSpecCDODefId,pInstanceId);
    CALL csiPRDGetNextInstanceId(vCMSpecBaseCDODefId,pInstanceId1);
      
   If pCMSpecName = 'Draft Camstar' THEN
           INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
           VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',1,1,0,0,1);
    elseif pCMSpecName = 'Draft PLM' THEN
           INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
           VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,1,0,0,1);
    elseif pCMSpecName = 'Draft' THEN
           INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
           VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,0,0,1);
    elseif pCMSpecName = 'Pending Approval Camstar' THEN
           INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
           VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,1,1);
    elseif pCMSpecName = 'Pending Approval PLM' THEN
           INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
           VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,1,1);
	elseif pCMSpecName = 'Pending Deployment' THEN
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
            VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,0,1);
	elseif pCMSpecName = 'Deployment Incomplete' THEN
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
            VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,0,2);
	elseif pCMSpecName = 'Deployment Complete' THEN
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
            VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,0,2);
	elseif pCMSpecName = 'Rejected' THEN
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
            VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,0,3);
    else 
            INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
            VALUES
           (pInstanceId1,pInstanceId,vCMSpecCDODefId,1,NULL,pRoleDescription
           ,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,0,1,0,0);
	END IF;
                                 
    INSERT INTO BusinessProcessSpecBase
           (BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
    VALUES
           (pInstanceId1,pCMSpecName,vCMSpecBaseCDODefId,1,NULL,pInstanceId);
           
     RAISE NOTICE '%', pCMSpecName;
     select RoleId INTO vRoleId from RoleDef where Description like pCMSpecName || '%';
     if pCMSpecName ='Draft' THEN
		select RoleId INTO  vRoleId from RoleDef where RoleName = 'DraftPermissions';
	END IF;
     
     INSERT INTO ChangeMgtSpecAllowableRoles
           (AllowableRolesId,BusinessProcessSpecId,FieldId,Sequence)
     VALUES
           (vRoleId,pInstanceId,vCMFieldRoleDefId,1);
          
end $$;

           
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateWorkFlow')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateWorkFlow;
 	END IF;
END $$;
CREATE PROCEDURE csiCMCreateWorkFlow(
	pWFDescription VARCHAR(255),
	pWFName VARCHAR(30), 
	OUT pInstanceId2 varchar(16), 
	OUT pInstanceId3 varchar(16), 
	OUT pInstanceId4 varchar(16), 
	OUT pInstanceId5 varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCMWrkFlowCDODefId INTEGER := 8517;
	vCMWrkFlowBaseCDODefId INTEGER := 8518;
	vCMWrkFlowStepCDODefId INTEGER := 8578;
	vCMPathCDODefId INTEGER := 1440;
	vCMPathSelectorCDODefId INTEGER := 1840;
	vCMPathSelectorInstanceId varchar(16);
	vCMDRAFT CHAR(16);
	vCMDI CHAR(16);
	vCMDC CHAR(16); 
	vCMCL CHAR(16);
	vSpecId CHAR(16); 
	vCMPD CHAR(16);
	vSpecDescription VARCHAR(255);
BEGIN
    
    CALL csiPRDGetNextInstanceId(vCMWrkFlowBaseCDODefId,pInstanceId2);
    CALL csiPRDGetNextInstanceId(vCMWrkFlowCDODefId,pInstanceId3);
    CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
           
    INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES
           (pInstanceId2,pInstanceId3,vCMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
    INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
    VALUES(pInstanceId2,pWFName,vCMWrkFlowBaseCDODefId,1,NULL,pInstanceId3);
     
     
    vCMDRAFT := pInstanceId4;
	
	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Draft'
	   and BusinessProcessSpec.Revision = '1';	
	 
    select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
    INSERT INTO WorkflowStep
    (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('ccfba42a-ca60-4227-8383-7aba97f3a44c',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',45,214);
			
	Update BusinessProcessWorkflow set FirstStepId = pInstanceId4	where BusinessProcessWorkflowId = pInstanceId3;
			
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMDI := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1';

	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('8d25b828-d7ee-404f-9874-767d1ad39f1b',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',179,77);
	
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMDC := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1';

	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('36b147dc-92ff-4e90-975e-d2422eb54223',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',212,205);
	
			
	CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
		
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('458565e9-422c-4c5a-9aed-a858adecf3af', vCMPathCDODefId,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);

	Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;
           
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
           
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('8e64ba4f-5b60-45a9-920a-29bf429faa55',vCMPathCDODefId,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);
           
    CALL csiPRDGetNextInstanceId(vCMPathSelectorCDODefId,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('245DF453-8CAB-4592-916D-50180F1A4723',vCMPathSelectorCDODefId,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');
         
	
    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
           
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('4a679c35-1b40-4bb6-a315-256ffafd47b3',vCMPathCDODefId,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);

	Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3 ;
     
      CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('07f12fc3-1a58-4933-9b4f-5a577b07f0e5',vCMPathCDODefId,1,NULL,NULL,vCMDRAFT,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);

    Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Draft' and WorkflowId = pInstanceId3;
     
             
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateWFCamstarPLM')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateWFCamstarPLM;
 	END IF;
END $$;
CREATE PROCEDURE csiCMCreateWFCamstarPLM(
	pWFDescription VARCHAR(255),
	pWFName VARCHAR(30), 
	OUT pInstanceId2 varchar(16), 
	OUT pInstanceId3 varchar(16), 
	OUT pInstanceId4 varchar(16), 
	OUT pInstanceId5 varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCMWrkFlowCDODefId INTEGER:=8517;
	vCMWrkFlowBaseCDODefId INTEGER:=8518;
	vCMWrkFlowStepCDODefId INTEGER:=8578;
	vCMPathCDODefId INTEGER:=1440;
	vCMPathSelectorCDODefId INTEGER:=1840;
	vCMPathSelectorInstanceId varchar(16);
	vCMDRAFT CHAR(16);
	vCMDI CHAR(16);
	vCMDC CHAR(16);
	vCMCL CHAR(16);
	vSpecId CHAR(16);
	vCMVOID CHAR(16);
	vCMREJ CHAR(16);
	vCMPD CHAR(16);
	vCMPA CHAR(16);
	vSpecDescription VARCHAR(255);
    vCMDRAFTName VARCHAR(255);
	vPAName VARCHAR(255);
BEGIN
    
    CALL csiPRDGetNextInstanceId(vCMWrkFlowBaseCDODefId,pInstanceId2);
    CALL csiPRDGetNextInstanceId(vCMWrkFlowCDODefId,pInstanceId3);
    CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
           
    INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES
           (pInstanceId2,pInstanceId3,vCMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
    INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
    VALUES(pInstanceId2,pWFName,vCMWrkFlowBaseCDODefId,1,NULL,pInstanceId3);
     
    if pWFDescription = 'No Approval' THEN
		vCMDRAFTName := 'Draft';
	elseif  pWFDescription = 'Camstar' THEN
		vCMDRAFTName := 'Draft Camstar';
    elseif  pWFDescription = 'PLM' THEN
		vCMDRAFTName := 'Draft PLM' ;
	END IF;
		
     
    vCMDRAFT := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = vCMDRAFTName
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
    
    if pWFDescription = 'No Approval' THEN
		INSERT INTO WorkflowStep
		(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
			   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
			   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
		VALUES('1fbf9172-3525-476d-9eed-5e869a4d2f00',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
				 NULL,NULL,1,'0000000000000000',vSpecId,1, 
				NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',37,24);
	elseif  pWFDescription = 'Camstar' THEN
		INSERT INTO WorkflowStep
		(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
			   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
			   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
		VALUES('84ff7c87-122b-4e5b-a7c7-c8784836791a',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
				 NULL,NULL,1,'0000000000000000',vSpecId,1, 
				NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft Camstar',37,24);
	elseif  pWFDescription = 'PLM' THEN
		INSERT INTO WorkflowStep
		(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
			   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
			   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
		VALUES('c6b8c4a2-0c30-47f2-a6f2-9f81a43e157a',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
				 NULL,NULL,1,'0000000000000000',vSpecId,1, 
				NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft PLM',37,24);
	END IF;
	
	Update BusinessProcessWorkflow set FirstStepId = pInstanceId4	where BusinessProcessWorkflowId = pInstanceId3;
	
		
	if  pWFDescription = 'Camstar' THEN
		vPAName := 'Pending Approval Camstar';
    elseif  pWFDescription = 'PLM' THEN
		vPAName := 'Pending Approval PLM';
	END IF;
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMPA := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = vPAName
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
	if  pWFDescription = 'Camstar' THEN
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('e25e9d3a-fe51-4f48-9640-04064abcad15',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval Camstar',35,137);
	elseif  pWFDescription = 'PLM' THEN
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('27df4f12-d414-4535-b62f-bdf4a505e485',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval PLM',35,137);
	END IF;
		
				
		
				
			
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMPD := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Deployment' 
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;
	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1f7db9c4-5737-4ac4-a2dd-89ab47c85fb0',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Deployment',32,248);
	
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMREJ := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Rejected' 
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;

	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('fbdd0b51-2e43-47d9-ac29-d7f9b6eaf07f',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Rejected',180,135);
	
	
			
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMDC := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete' 
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;

	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1f3f665c-583d-4aea-b648-e07227764b5b',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',227,365);
	
	CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
	vCMDI := pInstanceId4;

	select BusinessProcessSpecId INTO vSpecId
	  from BusinessProcessSpec,
		   BusinessProcessSpecBase 
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete' 
	   and BusinessProcessSpec.Revision = '1';
	select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId;

	INSERT INTO WorkflowStep
   (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('de0081b2-d8e8-4460-bb6c-5540a480352a',vCMWrkFlowStepCDODefId,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',391,254);
	
			
	CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('24980df0-db54-454f-bfae-9019740bcbe9',vCMPathCDODefId,1,NULL,NULL,vCMDRAFT,0,NULL,
           pInstanceId5,'Pending Approval',NULL, NULL,vCMPA,NULL);
	Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = vCMDRAFTName and WorkflowId = pInstanceId3;
           
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('d84b65c1-e7a8-4b17-ba1b-b5b9520e7372',vCMPathCDODefId,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Pending Deployment',NULL, NULL,vCMPD,NULL);
    Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Pending Approval' and WorkflowId = pInstanceId3;

    CALL csiPRDGetNextInstanceId(vCMPathSelectorCDODefId,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('F3A54DBA-568A-464A-801F-E0B9FEA4552D',vCMPathSelectorCDODefId,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Approved');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMPA);
           
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('93fc44ef-1c3b-48ab-a021-21b424e89ee0',vCMPathCDODefId,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Rejected',NULL, NULL,vCMREJ,NULL);

    CALL csiPRDGetNextInstanceId(vCMPathSelectorCDODefId,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('32F00D0C-4486-470B-98ED-FB1EB2AC0AE2',vCMPathSelectorCDODefId,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Rejected');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,2,vCMPA);
           
       
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',vCMPathCDODefId,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Draft',NULL, NULL,vCMDRAFT,NULL);

    CALL csiPRDGetNextInstanceId(vCMPathSelectorCDODefId,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('1C7AF084-F98F-4825-A0EF-6D71891E84E6',vCMPathSelectorCDODefId,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Pending or TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Cancelled');
	
    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,3,vCMPA);
           
           
	CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',vCMPathCDODefId,1,NULL,NULL,vCMPD,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);
    Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Pending Deployment' and WorkflowId = pInstanceId3;
     
     
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('157222b5-582c-4955-a1f3-55deb75f2907',vCMPathCDODefId,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);
    Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;
    
        
      
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('076c6a19-f638-4745-a3e2-788d94c42395',vCMPathCDODefId,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);


 
	CALL csiPRDGetNextInstanceId(vCMPathSelectorCDODefId,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',vCMPathSelectorCDODefId,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');
         
	
    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
               
           
         
           
    CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
    VALUES
           ('d57d4000-9dc5-412b-be63-db7363429e2c',vCMPathCDODefId,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);
    Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;
              
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateBusinessRule')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateBusinessRule;
 	END IF;
END $$;

CREATE PROCEDURE csiCMCreateBusinessRule(
	pExportImportKey VARCHAR(36),
	pBRDescription VARCHAR(255),
	pBRName VARCHAR(30), 
	pBRHName VARCHAR(30), 
	pSBRName VARCHAR(30), 
	pBRSCRIPT VARCHAR(4000), 
	pSBRIsAdvancedMode INT, 
	pSBRRecurrenceFrequency INTEGER, 
	pSBRRecurrencePattern INTEGER, 
	pSBRScheduleHours VARCHAR(255),
	OUT pInstanceId1 varchar(16), 
	OUT pInstanceId2 varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vBRCDODefId INTEGER := 7569;
	vBRDataCDODefId INTEGER := 7573;
	vBRHandlerCDODefId INTEGER := 7564;
	vBRHandlerDataCDODefId INTEGER := 7567;
	vSchedBRCDODefId INTEGER := 7587;
	vBRDataInstanceId VARCHAR(16);
	vBRHandlerInstanceId VARCHAR(16);
	vBRHandlerDataInstanceId VARCHAR(16);
	
	vStartDate TIMESTAMP;
	vStartDateGMT TIMESTAMP;
	vDueDate TIMESTAMP;
	vDueDateGMT TIMESTAMP;
BEGIN
    
    CALL csiPRDGetNextInstanceId(vBRCDODefId,pInstanceId1);
    CALL csiPRDGetNextInstanceId(vBRDataCDODefId,vBRDataInstanceId);
    CALL csiPRDGetNextInstanceId(vBRHandlerCDODefId,vBRHandlerInstanceId);
    CALL csiPRDGetNextInstanceId(vBRHandlerDataCDODefId,vBRHandlerDataInstanceId);
    CALL csiPRDGetNextInstanceId(vSchedBRCDODefId,pInstanceId2);
 
           
    INSERT INTO BusinessRuleHandler
           (BusinessRuleHandlerDataId
           ,BusinessRuleHandlerId
           ,BusinessRuleHandlerName
           ,CDOTypeId
           ,ChangeCount
           ,ChangeHistoryId
           ,Description
           ,IconId
           ,IsFrozen
           ,IsValid
           ,Notes
           ,ValidateOnSave)
    VALUES
           (vBRHandlerDataInstanceId,vBRHandlerInstanceId,pBRHName,vBRHandlerCDODefId,1,NULL,pBRDescription,NULL,0,1,NULL,0);
           
           
           



  INSERT INTO BusinessRuleHandlerData
           (BizRuleHandlerType
           ,BusinessRuleHandlerDataId
           ,BusinessRuleHandlerDataName
           ,BusinessRuleHandlerId
           ,CDOTypeId
           ,ChangeCount
           ,IsFrozen
           ,Script
           ,ServiceType)
    VALUES(1,vBRHandlerDataInstanceId,pBRHName,vBRHandlerInstanceId, vBRHandlerDataCDODefId,1,0,pBRSCRIPT,NULL);




     
     
	INSERT INTO BusinessRule
           (BusinessRuleDataId
           ,BusinessRuleId
           ,BusinessRuleName
           ,CDOTypeId
           ,ChangeCount
           ,ChangeHistoryId
           ,Description
           ,IconId
           ,IsFrozen
           ,Notes)
    VALUES(vBRDataInstanceId,pInstanceId1,pBRName,vBRCDODefId,1,NULL,pBRDescription,NULL,0,NULL);


	INSERT INTO BusinessRuleData
           (ExportImportKey
		   ,AlwaysExecute
           ,BusinessRuleCondition
           ,BusinessRuleDataId
           ,BusinessRuleDataName
           ,BusinessRuleId
           ,CDOTypeId
           ,ChangeCount
           ,ContextType
           ,IsFrozen
           ,Scope)
    VALUES(pExportImportKey,1,NULL,vBRDataInstanceId,pBRName,pInstanceId1,vBRDataCDODefId,1,1140,0,0);
			
			
	INSERT INTO BusinessRuleDataHandlers
           (BusinessRuleDataId
           ,FieldId
           ,HandlersId
           ,Sequence)
    VALUES(vBRDataInstanceId,13314,vBRHandlerInstanceId,1);


    
	vStartDate := CURRENT_TIMESTAMP;
    vStartDateGMT := CURRENT_TIMESTAMP AT TIME ZONE 'UTC';
	
		
    IF (pSBRIsAdvancedMode = 1) THEN
    BEGIN

		vDueDate := 
			CASE
				WHEN EXTRACT(HOUR FROM CURRENT_TIMESTAMP) > 4 THEN 
					DATE_TRUNC('DAY', CURRENT_TIMESTAMP + INTERVAL '1 DAY') + INTERVAL '4 HOURS'
				ELSE 
					DATE_TRUNC('DAY', CURRENT_TIMESTAMP) + INTERVAL '4 HOURS'
				END;
		
		vDueDateGMT := 
			CASE
				WHEN EXTRACT(HOUR FROM CURRENT_TIMESTAMP) > 4 THEN 
					(DATE_TRUNC('DAY', CURRENT_TIMESTAMP + INTERVAL '1 DAY') + INTERVAL '4 HOURS') AT TIME ZONE 'UTC'
				ELSE 
					(DATE_TRUNC('DAY', CURRENT_TIMESTAMP) + INTERVAL '4 HOURS') AT TIME ZONE 'UTC'
				END;
						
	END;
	ELSE
	BEGIN
		vDueDate := vStartDate;
		vDueDateGMT := vStartDateGMT;
	END;
	END IF;

	INSERT INTO ScheduledBusinessRule
           (CDOTypeId
           ,ChangeCount
           ,ChangeHistoryId
           ,DayOfMonth
           ,DayOfWeek
           ,Description
           ,DueTime
           ,DueTimeGMT
           ,EndDate
           ,EndDateGMT
           ,ExecutionContext
           ,ExecutionContextType
           ,IconId
		   ,IsAdvancedMode
           ,IsFrozen
           ,IsLastDayOfMonth
           ,IsSystemDefined
           ,LockGUID
           ,MonthOfYear
           ,Notes
           ,OnExecute
           ,RecurrenceCount
           ,RecurrenceFrequency
           ,RecurrencePattern
           ,ScheduledBusinessRuleId
           ,ScheduledBusinessRuleName
		   ,ScheduleHours
           ,StartDate
           ,StartDateGMT
           ,Status)    
	VALUES(vSchedBRCDODefId,1,NULL,NULL,NULL,NULL,vDueDate,vDueDateGMT,
			NULL,NULL,'0004740000000001',1140,NULL,pSBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,pInstanceId1,NULL,pSBRRecurrenceFrequency,pSBRRecurrencePattern,
			pInstanceId2,pSBRName,pSBRScheduleHours,vStartDate,vStartDateGMT,1);
			   
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateApprovalDecision')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateApprovalDecision;
 	END IF;
END $$;
CREATE PROCEDURE csiCMCreateApprovalDecision(
	pDecisionName VARCHAR(30), 
	pDecisionType INTEGER, 
	OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
BEGIN    

	CALL csiPRDGetNextInstanceId(7857,pInstanceId);
  
    INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
    VALUES
           (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);
          
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('CMPopulateDefaultData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS CMPopulateDefaultData;
 	END IF;
END $$;
CREATE PROCEDURE CMPopulateDefaultData()
LANGUAGE plpgsql
AS $$
DECLARE
    vCMSpecID VARCHAR(16);
    vCMSpecBaseID VARCHAR(16);
    vCMWrkFlowCDODefId VARCHAR(16);
    vCMWrkFlowBaseCDODefId VARCHAR(16);
    vCMWrkFlowStepCDODefId VARCHAR(16);  
    vCMPathCDODefId VARCHAR(16);
    vBusinessRuleId VARCHAR(16);   
    vSchedBusinessRuleId VARCHAR(16);
    vApprovalDecisionId VARCHAR(16);
BEGIN
    

    RAISE NOTICE 'Creating Approval Decisions';
	CALL csiCMCreateApprovalDecision('Approved',10,vApprovalDecisionId);
	CALL csiCMCreateApprovalDecision('Rejected',20,vApprovalDecisionId);

     RAISE NOTICE 'Creating Draft...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft',vCMSpecID,vCMSpecBaseID);
	
	RAISE NOTICE 'Creating Draft Camstar...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft Camstar',vCMSpecID,vCMSpecBaseID);
   
   RAISE NOTICE 'Creating Draft PLM...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft PLM',vCMSpecID,vCMSpecBaseID);
   
   RAISE NOTICE 'Creating Deployment Complete';
    CALL csiCMCreateSpec('Deployment_Complete.png','Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.','Deployment Complete',vCMSpecID, vCMSpecBaseID);
    
    RAISE NOTICE 'Creating Deployment Incomplete';
	CALL csiCMCreateSpec('Deployment_InComplete.png','Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.','Deployment Incomplete',vCMSpecID, vCMSpecBaseID);
       
    RAISE NOTICE 'Creating Pending Approval Camstar';
	CALL csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval Camstar',vCMSpecID, vCMSpecBaseID);
   
   RAISE NOTICE 'Creating Pending Approval PLM';
	CALL csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval PLM',vCMSpecID, vCMSpecBaseID);
   
   
    RAISE NOTICE 'Creating Rejected...';
	CALL csiCMCreateSpec('Rejected.png','In the Rejected step the package can be Voided or go back to Draft.','Rejected',vCMSpecID, vCMSpecBaseID);
   
    RAISE NOTICE 'Creating Pending Deployment...';
	CALL csiCMCreateSpec('Open.png','In the Pending Deployment step the Package can be manually deployed to one or more remote targets, changing the status to Deployment Complete or Deployment Incomplete, if deployment fails. If deployment fails, user can redeploy or Close the Package','Pending Deployment',vCMSpecID,vCMSpecBaseID);
       
       
       
    RAISE NOTICE 'Creating Workflow...';
	CALL csiCMCreateWorkFlow('No Approval','No Approval', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId,  vCMPathCDODefId);
	
	RAISE NOTICE 'Creating Workflow Camstar...';
	CALL csiCMCreateWFCamstarPLM('Camstar','Camstar', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
     
    RAISE NOTICE 'Creating Workflow PLM...';
	CALL csiCMCreateWFCamstarPLM('PLM','PLM', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
     
     
    RAISE NOTICE 'Creating Business Rule Export...';
	CALL csiCMCreateBusinessRule('c3575681-5d0c-4c5d-817e-ab70049107b3', 'This script will process exports associated with a change packages.','BR_DEPLOY','BRH_DEPLOY','SBR_DEPLOY','ExecuteQueryEX("ChangePackage_GetExportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ExportImportName", 1,Transaction::__Const.DataType.String, CLF::ExportNames, 0);ForEach(CLF::ExportName,CLF::ExportNames){InitQueryParametersEx("ExportName",CLF::ExportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByExportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeployment,ExportName,CLF::ExportName);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Deploy...';
	CALL csiCMCreateBusinessRule('486d13ad-8e78-424e-a7a7-0bf7ae9375aa', 'This script will process exports associated with a change packages.','BR_DEPLOYSTATUS','BRH_DEPLOYSTATUS','SBR_DEPLOYSTATUS','ExecuteQueryEX("ChangePackage_GetDeploymentsInQueue", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"Name", 1,Transaction::__Const.DataType.String, CLF::PackageNames, 0);ForEach(CLF::ChangePackageName,CLF::PackageNames){CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeploymentInQueue);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Import...';
	CALL csiCMCreateBusinessRule('88729868-5ee0-4f77-9884-98e0349eb969', 'This script will process imports (activations) associated with a change packages.','BR_ACTIVATION','BRH_ACTIVATION','SBR_ACTIVATION', 'ExecuteQueryEX("ChangePackage_GetImportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ImportSetName", 1,Transaction::__Const.DataType.String, CLF::ImportNames, 0);ForEach(CLF::ImportName,CLF::ImportNames){InitQueryParametersEx("ImportName",CLF::ImportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByImportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessActivation,ImportSetName,CLF::ImportName);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Notifications...';
	CALL csiCMCreateBusinessRule('E78C5FFF-75AF-49d6-A964-A8772AE00437', 'This script will support email notifications for Change Package.','BR_NOTIFICATIONS','BRH_NOTIFICATIONS','SBR_NOTIFICATIONS', 'if(CLF::__CDOID.SessionValues and CLF::__CDOID.SessionValues.Factory){Call(CLF::__CDOID.SessionValues.Factory,SendReminderEmails);}',1,NULL,NULL,'4',
	vBusinessRuleId, vSchedBusinessRuleId);

	RAISE NOTICE 'Creating Business Rule RPT Control Loop Limits Calculations...';
    CALL csiCMCreateBusinessRule('aafd3304-a163-460a-bb06-a6b15f5c63a1','This script will run the RPT Control Loop Limits Calculations.','BR_RPTControlLimits','BRH_RPTControlLimits_Monthly', 'SBR_RPTControlLimits', 'CLF::RPTControlLimitsUpdateObject = null;CreateCDO("RPTControlLimitsUpdate", false, false, CLF::RPTControlLimitsUpdateObject);CLF::RPTControlLimitsUpdateObject.RPTUpdateOccurrencePattern = 2;CLF::RPTControlLimitsUpdateObject.NoOfRPTCombinations = 100;CLF::RPTControlLimitsUpdateObject.HistoryTimePeriod = 30;CLF::RPTControlLimitsUpdateObject.Factory = CLF::__CDOID.SessionValues.Factory;Call(CLF::RPTControlLimitsUpdateObject, ProcessRPTControlLimits);',0,NULL,NULL,NULL,vBusinessRuleId, vSchedBusinessRuleId);
	
    RAISE NOTICE 'Complete.';
 
end $$;

do $$ 
begin	
	CALL CMPopulateDefaultData();
end $$;

