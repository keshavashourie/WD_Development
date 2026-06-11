--------------------------------------------------------------------------------
-- SCRIPT:ReloadCMDefaultData.sql
-- DESCR: Reloads the default OOB ChangeManagement Specs, Business Rules, Business Rule Handlers, Approval Decision, Action, Action Rules, Workflows. 
--		  This will be run through a batch file csiReloadCMData.bat. This batch file will get executed when user checks on Load Change Management in the Management studio
--Copyright Siemens 2023  

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCreateGUID')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCreateGUID;
 	END IF;
END $$;
CREATE PROCEDURE csiCreateGUID(pPermissionName VARCHAR(255),pRoleGID OUT VARCHAR(36))
LANGUAGE plpgsql
AS $$
BEGIN

	SELECT (substr(ExportImportKeyGUID, 1, 8) ||
		'-' || substr(ExportImportKeyGUID, 9, 4) ||
		'-' || substr(ExportImportKeyGUID, 13, 4) ||
		'-' || substr(ExportImportKeyGUID, 17, 4) ||
		'-' || substr(ExportImportKeyGUID, 21,12))
	INTO pRoleGID
	from 
	(
		SELECT UPPER(md5(pPermissionName)) as ExportImportKeyGUID
	) c;

END;
$$;

--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateSpec
-- DESCR: Inserts the CM Specs. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
CREATE PROCEDURE csiCMCreateSpec(pStepIcon varchar(512), pRoleDescription varchar(255),pCMSpecName varchar(30), OUT pInstanceId varchar(16), OUT pInstanceId1 varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	pCMSpecCDODefId INTEGER := 8513;
	pCMSpecBaseCDODefId INTEGER := 8514;
	pCMWrkFlowCDODefId INTEGER := 8517;
	pCMWrkFlowBaseCDODefId INTEGER := 8518;
	pCMFieldRoleDefId INTEGER := 22504;
	pRoleId VARCHAR(50);
BEGIN        

    CALL csiPRDGetNextInstanceId(pCMSpecCDODefId,pInstanceId);
    CALL csiPRDGetNextInstanceId(pCMSpecBaseCDODefId,pInstanceId1);
  
    IF NOT EXISTS (SELECT * FROM BusinessProcessSpecBase WHERE BusinessProcessSpecName = pCMSpecName) THEN
		BEGIN
           
				INSERT INTO BusinessProcessSpecBase
						(BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
				VALUES
					  (pInstanceId1,pCMSpecName,pCMSpecBaseCDODefId,1,NULL,pInstanceId);
					  
				If pCMSpecName = 'Draft Camstar' THEN
					INSERT INTO BusinessProcessSpec
								(BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
								,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
						VALUES
								(pInstanceId1,pInstanceId,pCMSpecCDODefId,1,NULL,pRoleDescription
								,NULL,NULL,0,NULL,1,1,pStepIcon,'',1,1,0,0,1);
								
				ELSEIF pCMSpecName = 'Draft PLM' THEN
					INSERT INTO BusinessProcessSpec
							(BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
							,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals,LockInstances,RequiresApproval,PackageStatus)
						 VALUES
								(pInstanceId1,pInstanceId,pCMSpecCDODefId,1,NULL,pRoleDescription
									,NULL,NULL,0,NULL,1,1,pStepIcon,'',0,1,0,0,1);
									
				ELSEIF pCMSpecName = 'Draft' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, '', NULL, 1, 1, pStepIcon, '', 0, 0, 0, 0, 1);

				ELSEIF pCMSpecName = 'Pending Approval Camstar' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, '', NULL, 1, 1, pStepIcon, '', 0, 0, 1, 1, 1);

				ELSEIF pCMSpecName = 'Pending Approval PLM' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 1, 1);
 
				ELSEIF pCMSpecName = 'Pending Deployment' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0, 1);
						

				ELSEIF pCMSpecName = 'Deployment Incomplete' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0, 2);

				ELSEIF pCMSpecName = 'Deployment Complete' THEN									
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0, 2);
						
				ELSEIF pCMSpecName = 'Rejected' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0, 3);

				else 	
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals,
						LockInstances, RequiresApproval, PackageStatus)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0, 0);

				END IF;
   				
				RAISE NOTICE 'Spec Name: %', pCMSpecName;
				
				IF pCMSpecName ='Draft'  THEN
					SELECT RoleId INTO pRoleId FROM RoleDef WHERE RoleName = 'DraftPermissions';
				ELSE
					SELECT RoleId INTO pRoleId FROM RoleDef WHERE Description LIKE pCMSpecName || '%';
				END IF;
			   
				INSERT INTO ChangeMgtSpecAllowableRoles
					(AllowableRolesId,BusinessProcessSpecId,FieldId,Sequence)
				 VALUES
					(pRoleId,pInstanceId,pCMFieldRoleDefId,1);
           
		  END;
	ELSE
		BEGIN
			
			DELETE FROM BusinessProcessSpec
			WHERE BusinessProcessSpecBaseId IN (
				SELECT a.BusinessProcessSpecBaseId
				FROM BusinessProcessSpec a
				JOIN BusinessProcessSpecBase b ON a.BusinessProcessSpecBaseId = b.BusinessProcessSpecBaseId
				WHERE b.BusinessProcessSpecName = pCMSpecName
			);

			DELETE FROM BusinessProcessSpecBase WHERE BusinessProcessSpecName = pCMSpecName;
		
				INSERT INTO BusinessProcessSpecBase
						(BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
				VALUES
					  (pInstanceId1,pCMSpecName,pCMSpecBaseCDODefId,1,NULL,pInstanceId);
					  
				IF pCMSpecName = 'Draft Camstar' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 1, 1, 0, 0);
								
				ELSEIF pCMSpecName = 'Draft PLM' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 1, 0, 0);
					
				ELSEIF pCMSpecName = 'Draft' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 0, 0);

				ELSEIF pCMSpecName = 'Pending Approval Camstar' THEN
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 1);

				ELSEIF pCMSpecName = 'Pending Approval PLM' THEN
					
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 1);

				else
					INSERT INTO BusinessProcessSpec
						(BusinessProcessSpecBaseId, BusinessProcessSpecId, CDOTypeId, ChangeCount, ChangeHistoryId, Description,
						 ECO, IconId, IsFrozen, Notes, Revision, Status, StepIcon, WIPMsgDefMgrId, AssignApprovers, ResetApprovals, LockInstances, RequiresApproval)
					VALUES
						(pInstanceId1, pInstanceId, pCMSpecCDODefId, 1, NULL, pRoleDescription,
						 NULL, NULL, 0, NULL, 1, 1, pStepIcon, '', 0, 0, 1, 0);
						 
				END IF;
   				
				RAISE NOTICE 'Spec Name: %', pCMSpecName;
				RAISE NOTICE 'after % insert', pCMSpecName;

				IF pCMSpecName ='Draft'  THEN
					select RoleId INTO pRoleId from RoleDef where RoleName = 'DraftPermissions';
				ELSE
					select RoleId INTO pRoleId from RoleDef where Description like pCMSpecName || '%';
				END IF;
			   
				INSERT INTO ChangeMgtSpecAllowableRoles
					(AllowableRolesId,BusinessProcessSpecId,FieldId,Sequence)
				VALUES
					(pRoleId,pInstanceId,pCMFieldRoleDefId,1);
					
				RAISE NOTICE 'BusinessProcessSpecBase % deleted and created', pCMSpecName;

		  END;
		  
	END IF;
end $$;
           
		   
--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWorkFlow
-- DESCR: Inserts the CM Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
CREATE PROCEDURE csiCMCreateWorkFlow(pWFDescription VARCHAR(255),pWFName VARCHAR(30), OUT pInstanceId2 VARCHAR(16), OUT pInstanceId3 VARCHAR(16), OUT pInstanceId4 VARCHAR(16), OUT pInstanceId5 VARCHAR(16))
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
           
    IF NOT EXISTS (SELECT * FROM BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName) THEN	
	   BEGIN
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
				
			UPDATE BusinessProcessWorkflow set FirstStepId = pInstanceId4 where BusinessProcessWorkflowId = pInstanceId3;	
					
					
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

			Update WorkflowStep set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;
				   
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

			Update WorkflowStep set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;

			CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
				  
			INSERT INTO Path
				   (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
				   ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
			VALUES
				   ('07f12fc3-1a58-4933-9b4f-5a577b07f0e5',vCMPathCDODefId,1,NULL,NULL,vCMDRAFT,0,NULL,
				   pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);

			Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Draft' and WorkflowId = pInstanceId3;
		end;
    ELSE
		Begin
			delete from BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
		
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
	   
			INSERT INTO WorkflowStep
			(CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
				   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
				   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES(vCMWrkFlowStepCDODefId,1,NULL,'The Package automatically enters Draft state after creation. In Draft state the user edits core package attributes and assigns instance content. Outside of Draft state, package attributes and instance content cannot be altered. ',NULL,0,0,NULL,1,
					 NULL,NULL,1,'0000000000000000',vSpecId,1, 
					NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',45,214);
			
			Update BusinessProcessWorkflow set FirstStepId = pInstanceId4 where BusinessProcessWorkflowId = pInstanceId3;
			
			
			CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
			vCMDI := pInstanceId4;
			--select @SpecBaseId = [BusinessProcessSpecBaseId] from [BusinessProcessSpecBase] where [BusinessProcessSpecName] = 'Deployment Incomplete'  
			select BusinessProcessSpecId INTO vSpecId
			  from BusinessProcessSpec,
				   BusinessProcessSpecBase 
			 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
			   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
			   and BusinessProcessSpec.Revision = '1';

			INSERT INTO WorkflowStep
			(CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
				   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
				   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES(vCMWrkFlowStepCDODefId,1,NULL,'Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.',NULL,0,0,NULL,0,
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

			INSERT INTO WorkflowStep
		   (CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
				   ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
				   ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES(vCMWrkFlowStepCDODefId,1,NULL,'Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.',NULL,0,0,NULL,1,
					 NULL,NULL,1,'0000000000000000',vSpecId,1, 
					NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',212,205);
	
	
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
				   (CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
				   ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
			VALUES
				   (vCMPathCDODefId,1,NULL,NULL,vCMDI,0,NULL,
				   pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);

			Update WorkflowStep set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;

			  CALL csiPRDGetNextInstanceId(vCMPathCDODefId,pInstanceId5);
				  
			INSERT INTO Path
				   (CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
				   ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
			VALUES
				   (vCMPathCDODefId,1,NULL,NULL,vCMDRAFT,0,NULL,
				   pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);

			Update WorkflowStep set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Draft' and WorkflowId = pInstanceId3;
				
			RAISE NOTICE '[[BusinessProcessWorkflowBase]] % deleted and created', pWFName;
		END;
	END IF;
end $$;

--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWFCamstarPLM
-- DESCR: Inserts the CM PLM and Camstar Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
CREATE PROCEDURE csiCMCreateWFCamstarPLM(pWFDescription VARCHAR(255),pWFName VARCHAR(30), OUT pInstanceId2 varchar(16), OUT pInstanceId3 varchar(16), OUT pInstanceId4 varchar(16), OUT pInstanceId5 varchar(16))
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
           
	IF NOT EXISTS (SELECT * FROM BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName) THEN
		BEGIN
           
			INSERT INTO BusinessProcessWorkflowBase
				 (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
			 VALUES(pInstanceId2,pWFName,vCMWrkFlowBaseCDODefId,1,NULL,pInstanceId3);
    
			INSERT INTO BusinessProcessWorkflow
				 (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
				,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
			VALUES
				 (pInstanceId2,pInstanceId3,vCMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
     
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
			   
			select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId ;

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
				vPAName = 'Pending Approval PLM';
			END IF;
			
			CALL csiPRDGetNextInstanceId(vCMWrkFlowStepCDODefId,pInstanceId4);
			vCMPA := pInstanceId4;

			select BusinessProcessSpecId INTO vSpecId
			  from BusinessProcessSpec,
				   BusinessProcessSpecBase 
			 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
			   and BusinessProcessSpecBase.BusinessProcessSpecName = vPAName
			   and BusinessProcessSpec.Revision = '1';			   
			select Description INTO vSpecDescription from BusinessProcessSpec where BusinessProcessSpecId = vSpecId ;

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
		END;
    ELSE
        BEGIN
			delete from BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
			
			INSERT INTO BusinessProcessWorkflowBase
				 (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
			 VALUES(pInstanceId2,pWFName,vCMWrkFlowBaseCDODefId,1,NULL,pInstanceId3);

			INSERT INTO BusinessProcessWorkflow
				 (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
				,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
			VALUES
				 (pInstanceId2,pInstanceId3,vCMWrkFlowCDODefId,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);

			if pWFDescription = 'No Approval' THEN
				vCMDRAFTName := 'Draft';
			elseif  pWFDescription = 'Camstar' THEN
				vCMDRAFTName := 'Draft Camstar';
			elseif  pWFDescription = 'PLM' THEN
				vCMDRAFTName := 'Draft PLM';
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
				   pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL) ;
			Update WorkflowStep set DefaultPathId =  pInstanceId5 where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;
						
			RAISE NOTICE 'BusinessProcessWorkflowBase % deleted and created', pWFName;
		END;
	END IF;      
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateBusinessRule
-- DESCR: Inserts the CM BusinessRules. If it exists then deletes and reloads
--------------------------------------------------------------------------------
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
CREATE PROCEDURE csiCMCreateBusinessRule(pBRDescription VARCHAR(255),pBRName VARCHAR(30), pBRHName VARCHAR(30), pSBRName VARCHAR(30), pBRSCRIPT VARCHAR(4000), pSBRIsAdvancedMode INT, pSBRRecurrenceFrequency INTEGER, pSBRRecurrencePattern INTEGER, pSBRScheduleHours VARCHAR(255),
	OUT pInstanceId1 varchar(16), OUT pInstanceId2 varchar(16))
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
                    
	IF NOT EXISTS (SELECT * FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = pBRHName) THEN
		BEGIN
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

		END;
    ELSE
		BEGIN
			DELETE FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = pBRHName;
			
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
						
			RAISE NOTICE 'BusinessRuleHandler % deleted and added', pBRHName;
			
			DELETE FROM BusinessRuleHandlerData WHERE BusinessRuleHandlerDataName = pBRHName;
			
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

		END;
		
		

        IF NOT EXISTS (SELECT * FROM BusinessRule WHERE BusinessRuleName = pBRName) THEN
			BEGIN
     
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
					   (AlwaysExecute
					   ,BusinessRuleCondition
					   ,BusinessRuleDataId
					   ,BusinessRuleDataName
					   ,BusinessRuleId
					   ,CDOTypeId
					   ,ChangeCount
					   ,ContextType
					   ,IsFrozen
					   ,Scope)
				VALUES(1,NULL,vBRDataInstanceId,pBRName,pInstanceId1,vBRDataCDODefId,1,1140,0,0);
			END;
		ELSE
			BEGIN
				DELETE FROM BusinessRule WHERE BusinessRuleName = pBRName;
						
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
								
				RAISE NOTICE 'BusinessRule % deleted and inserted', pBRName;
				
				DELETE FROM BusinessRuleData WHERE BusinessRuleDataName = pBRName;
				
				INSERT INTO BusinessRuleData
					   (AlwaysExecute
					   ,BusinessRuleCondition
					   ,BusinessRuleDataId
					   ,BusinessRuleDataName
					   ,BusinessRuleId
					   ,CDOTypeId
					   ,ChangeCount
					   ,ContextType
					   ,IsFrozen
					   ,Scope)
				VALUES(1,NULL,vBRDataInstanceId,pBRName,pInstanceId1,vBRDataCDODefId,1,1140,0,0);
					
			END;
		END IF;
				
				
		
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
	
		IF NOT EXISTS (SELECT * FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = pSBRName) THEN
			BEGIN
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
			END;
		ELSE
			BEGIN
				DELETE FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = pSBRName;
				
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
				VALUES(vSchedBRCDODefId,1,NULL,NULL,NULL,NULL,vStartDate,vStartDateGMT,
						NULL,NULL,'0004740000000001',1140,NULL,pSBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,pInstanceId1,NULL,pSBRRecurrenceFrequency,pSBRRecurrencePattern,
						pInstanceId2,pSBRName,pSBRScheduleHours,vStartDate,vStartDateGMT,1);
										
				RAISE NOTICE 'ScheduledBusinessRule % deleted and added', pSBRName;

			END;
		END IF;	
	END IF;
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM reakted roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreateRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreateRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreateRole(pRoleName VARCHAR(50), pRoleDescription VARCHAR(255), OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vRoleCDODefId INTEGER :=7130;
BEGIN
    
    CALL csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
    IF NOT EXISTS (SELECT * FROM RoleDef WHERE RoleName = pRoleName) THEN
		BEGIN
			INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
			   VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
        END;
    ELSE
        BEGIN
			DELETE from RoleDef WHERE RoleName = pRoleName;
			
			INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
			   VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
						
			RAISE NOTICE '[RoleDef] % deleted and created', pRoleName;

        END;
	END IF;
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreatePermission')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreatePermission;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreatePermission(pRoleId CHAR(16), pPermissionName VARCHAR(255), pPermissionType INTEGER, pObjectMetaId INTEGER, pPermissionModesFlag INTEGER, pObjectInstanceId CHAR(16) = NULL)
LANGUAGE plpgsql
AS $$
DECLARE
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)

    vIID VARCHAR(16);
	vRoleGID VARCHAR(36);
BEGIN
    
    CALL csiPRDGetNextInstanceId(7783,vIID);
    CALL csiCreateGUID(pPermissionName, vRoleGID);
	
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(vRoleGID,vIID,7783,pRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the pPermissionModesFlag value
	IF ( pPermissionModesFlag = 0 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType;
    ElseIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180) ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
          AND BitNumber = 2;
    ELSEIF ( pPermissionModesFlag = 2 AND pPermissionType = 180 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
          AND BitNumber IN (1,2,3,4);
	ELSE
		RAISE NOTICE 'Error - Invalid value passed for @PermissionModeFlag parameter...';
	END IF;

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM reakted roles permissions. 
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignPermissionToRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignPermissionToRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignPermissionToRole(pRoleName VARCHAR(255), pPermissionName VARCHAR(255), pPermissionType INTEGER, pObjectMetaId INTEGER, pPermissionModesFlag INTEGER, pObjectInstanceId CHAR(16) = NULL)
LANGUAGE plpgsql
AS $$
DECLARE
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
    vIID VARCHAR(16);
	vRoleGID VARCHAR(36);
	vRoleId CHAR(16);
BEGIN
    
    CALL csiPRDGetNextInstanceId(7783,vIID);
    CALL csiCreateGUID(pPermissionName, vRoleGID);
	
	SELECT RoleId INTO vRoleId FROM RoleDef WHERE RoleName = pRoleName;
	IF NOT EXISTS (SELECT * FROM RolePermission WHERE RoleId = vRoleId AND RolePermissionName = pPermissionName) THEN
    Begin  
		INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
				VALUES(vRoleGID,vIID,7783,vRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);
				
		-- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
		-- based on the pPermissionModesFlag value
		IF ( pPermissionModesFlag = 0 ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType;
		ElseIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType
			  AND BitNumber = 2;
		ELSEIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230) ) THEN
		   INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
			  SELECT vIID, 15263, BitNumber, ROW_NUMBER() OVER (ORDER BY BitNumber)
			  FROM SecurityMaskDetail
			  WHERE SecurityMaskId=pPermissionType
			  AND BitNumber IN (1,2,3,4);
		ELSE
			RAISE EXCEPTION 'Error - Invalid value passed for @PermissionModeFlag parameter...';
		END IF;
	END;
	END IF;
		
end $$;



--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACDeleteUnavailablePermissions')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACDeleteUnavailablePermissions;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACDeleteUnavailablePermissions(pPermissionType INTEGER, pPermissionName VARCHAR(255))
LANGUAGE plpgsql
AS $$
DECLARE
	vUnavialiblePermissions CHAR(16);	
BEGIN
	
	FOR vUnavialiblePermissions IN 
        SELECT RolePermissionId FROM RolePermission 
        WHERE RolePermissionName = pPermissionName AND PermissionType <> pPermissionType
    LOOP
        DELETE FROM RolePermission WHERE RolePermissionId = vUnavialiblePermissions;
        DELETE FROM RolePermissionModes WHERE RolePermissionId = vUnavialiblePermissions;
    END LOOP;

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateApprovalDecision
-- DESCR: Inserts the CM reakted approval decision. If it exists then deletes and recreates 
--------------------------------------------------------------------------------
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
CREATE PROCEDURE csiCMCreateApprovalDecision(pDecisionName VARCHAR(30), pDecisionType INTEGER, OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
BEGIN    

	CALL csiPRDGetNextInstanceId(7857,pInstanceId);
  
	IF NOT EXISTS (SELECT * FROM ApprovalDecision WHERE ApprovalDecisionName = pDecisionName) THEN
	Begin
		INSERT INTO ApprovalDecision
			   (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
		VALUES
			   (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);
	end;
    ELSE
    Begin
		delete from ApprovalDecision WHERE ApprovalDecisionName = pDecisionName;
		INSERT INTO ApprovalDecision
			   (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
		VALUES
			   (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);
		
		RAISE NOTICE '[ApprovalDecision] % deleted and created', pDecisionName;

	end;
	END IF;
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiCMCreateIDControl')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiCMCreateIDControl;
 	END IF;
END $$;
CREATE OR REPLACE PROCEDURE csiCMCreateIDControl(pIDType VARCHAR(30), pNextId INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
BEGIN    
  
	IF NOT EXISTS (SELECT * FROM IDControl WHERE IDType = pIDType) THEN
	Begin
		INSERT INTO IDControl (IDType,  NextID) 
			VALUES (pIDType, pNextId); 
	end;
    ELSE
		RAISE NOTICE '[IDType] % already exists', pIDType;
	END IF;
          
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionRule')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionRule;
 	END IF;
END $$;
CREATE PROCEDURE createActionRule(
	pName				varchar(30),
	pDescription		varchar(255),
	pExpression			varchar(1000))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId	integer;
	vInstanceId varchar(16);
BEGIN

	IF NOT EXISTS (SELECT * FROM ActionRule WHERE ActionRuleName = pName) THEN
	BEGIN
		
		RAISE NOTICE 'Inserting ActionRule: %', pName;

		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
		
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
		
		INSERT INTO ActionRule
			(ActionRuleId
			,ActionRuleName
			,CDOTypeId
			,ChangeCount
			,Description
			,Expression
			,IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nvarchar(255)
			,pExpression		-- nvarchar(255)
			,0);                -- bit
	END;
	ELSE
	BEGIN
			delete FROM ActionRule WHERE ActionRuleName = pName;
			
			RAISE NOTICE 'deleting and Inserting ActionRule: %', pName;
			
			SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
			
			CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
			
			INSERT INTO ActionRule
				(ActionRuleId
				,ActionRuleName
				,CDOTypeId
				,ChangeCount
				,Description
				,Expression
				,IsFrozen)
				VALUES
				(vInstanceId        -- char(16)
				,pName				-- nvarchar(30)
				,vCDOTypeId         -- int
				,1                  -- int
				,pDescription       -- nvarchar(255)
				,pExpression		-- nvarchar(255)
				,0);                -- bit
						
			RAISE NOTICE 'ActionRule % deleted and inserted', pName;
	END;
	END IF;
		
end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: createActionCategory
-- DESCR: Helper function to create an Action Category record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionCategory')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionCategory;
 	END IF;
END $$;
CREATE PROCEDURE createActionCategory(
	pName				varchar(30),
	pLabelName			varchar(50),
	pSequence			integer)
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId integer;
	vInstanceId varchar(16);
BEGIN

	IF NOT EXISTS (SELECT * FROM ActionCategory WHERE ActionCategoryName = pName) THEN
	BEGIN
		
		RAISE NOTICE 'Inserting ActionCategory: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
		
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
			(vInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,pSequence			-- int
			,0);				-- bit
	END;
	ELSE
	Begin
		
		RAISE NOTICE 'Deleting and Inserting ActionCategory: %', pName;
		
		delete FROM ActionCategory WHERE ActionCategoryName = pName;
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
		
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
			(vInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,pSequence			-- int
			,0);				-- bit
			
		RAISE NOTICE 'ActionCategory % deleted and inserted', pName;
	End;
	END IF;
		
end $$;


-----------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create ActionDef record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionDef;
 	END IF;
END $$;
CREATE PROCEDURE createActionDef(
	pName				varchar(30),
	pDescription		varchar(255),
	pType				integer, 
    OUT pInstanceId 	varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId integer;
BEGIN

	SELECT ActionId INTO pInstanceId FROM ActionDef WHERE ActionName = pName;
	
	IF (pInstanceId IS NULL) THEN
	BEGIN
		
		RAISE NOTICE 'Inserting Action: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
		INSERT INTO ActionDef
			(ActionId
			,ActionName
			,ActionType
			,CDOTypeId
			,ChangeCount
			,Description
			,IsFrozen)
			VALUES
			(pInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,pType				-- int
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nvarchar(255)
			,0);                -- bit

	END;
	ELSE
	Begin
		delete FROM ActionDef WHERE ActionName = pName;
		
		RAISE NOTICE 'Deleting & Inserting Action: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
		INSERT INTO ActionDef
			(ActionId
			,ActionName
			,ActionType
			,CDOTypeId
			,ChangeCount
			,Description
			,IsFrozen)
			VALUES
			(pInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,pType				-- int
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nvarchar(255)
			,0);   
					
		RAISE NOTICE 'Action % deleted and inserted', pName;
		
	end;
	END IF;
		
end $$;

	
-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createUIAction')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createUIAction;
 	END IF;
END $$;
CREATE PROCEDURE createUIAction(
	pActionId			char(16),
	pName				varchar(30),
	pType				integer,
	pDescription		varchar(255),
	pUIType				varchar(30),
	pUIVirtualPageName	varchar(30),
	pUIPageFlowName	    varchar(30),
	pMapItem		    varchar(30),
	pPortalTabOption	integer,
	pClearValues		int,
	pServiceName		varchar(30),
	pLabelName			varchar(66),
	pShowButtons		int,
	pIsPrimary          int,
	pActionCategoryName	varchar(30),
	pSequence			integer,
	pWidth				integer,
	pHeight				integer,
	pForceRedirect		int,
	OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId	integer;	
	vUIVirtualPageId	varchar(16);
	vUIPageFlowId		varchar(16);
	vActionCategoryId	varchar(16);
	vFloatPageLocationId	varchar(16);
BEGIN
    
	SELECT UIActionId INTO pInstanceId FROM UIAction WHERE UIActionName = pName ;

	IF (pInstanceId IS NULL) THEN
	BEGIN
		
		RAISE NOTICE 'Inserting UIAction: %', pName;

		vUIVirtualPageId := NULL;
		IF pUIVirtualPageName IS NOT NULL AND pUIVirtualPageName <> '' THEN
			SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pUIVirtualPageName;
		END IF;		
		
		vUIPageFlowId := NULL;
		IF pUIPageFlowName IS NOT NULL AND pUIPageFlowName <> '' THEN
			SELECT UIPageFlowId INTO vUIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = pUIPageFlowName;  
		END IF;
		
		vActionCategoryId := NULL;
		IF pActionCategoryName IS NOT NULL AND pActionCategoryName <> '' THEN
			SELECT ActionCategoryId INTO vActionCategoryId FROM ActionCategory WHERE ActionCategoryName = pActionCategoryName;  
		END IF;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			(vActionCategoryId  -- char(16)
			,vCDOTypeId         -- int
			,1                  -- int
			,pClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- bit
			,0                  -- int
			,pLabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,pActionId			-- char(16)
			,0                  -- int
			,pServiceName       -- nvarchar(30)
			,pSequence			-- int
			,pShowButtons		-- bit
			,pInstanceId        -- char(16)
			,pName              -- nvarchar(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId		-- char(16)
			,pMapItem			-- nvarchar(30)
			,pPortalTabOption	-- int
			,0);				-- int 
			
		IF (pWidth IS NOT NULL) AND (pHeight IS NOT NULL) THEN
		  BEGIN
			SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
			CALL csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);			
			
			INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
			VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);
			
			UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
		  END;
		END IF;
	END;
	ELSE
	BEGIN
		
		RAISE NOTICE 'Deleting & Inserting UIAction: %', pName;

		delete FROM UIAction WHERE UIActionName = pName;
		
		vUIVirtualPageId := NULL;				
		IF pUIVirtualPageName IS NOT NULL AND pUIVirtualPageName <> '' THEN
			SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pUIVirtualPageName;  
		END IF;
		
		vUIPageFlowId := NULL;
		IF pUIPageFlowName IS NOT NULL AND pUIPageFlowName <> '' THEN
			SELECT UIPageFlowId INTO vUIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = pUIPageFlowName;  
		END IF;
		
		vActionCategoryId := NULL;
		IF pActionCategoryName IS NOT NULL AND pActionCategoryName <> '' THEN
			SELECT ActionCategoryId INTO vActionCategoryId FROM ActionCategory WHERE ActionCategoryName = pActionCategoryName;  
		END IF;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			(vActionCategoryId  -- char(16)
			,vCDOTypeId         -- int
			,1                  -- int
			,pClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- bit
			,0                  -- int
			,pLabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,pActionId			-- char(16)
			,0                  -- int
			,pServiceName       -- nvarchar(30)
			,pSequence			-- int
			,pShowButtons		-- bit
			,pInstanceId        -- char(16)
			,pName              -- nvarchar(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId		-- char(16)
			,pMapItem			-- nvarchar(30)
			,pPortalTabOption	-- int
			,0);				-- int  
			
		IF (pWidth IS NOT NULL) AND (pHeight IS NOT NULL) THEN
		BEGIN
			SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
			CALL csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);
		        
			INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
			VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);
        
			UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
		END;
		END IF;		
		
		RAISE NOTICE 'UIAction % deleted and added', pName;

	END;
	END IF;		
		
end $$;

	
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createAction')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createAction;
 	END IF;
END $$;
CREATE PROCEDURE createAction(
	pName				varchar(30),
	pActionCategoryName	varchar(30),
	pType				integer,
	pDescription		varchar(255),
	pUIType				varchar(30),
	pUIVirtualPageName	varchar(30),
	pUIPageFlowName	    varchar(30),
	pClearValues		int,
	pSequence			integer,
	pServiceName		varchar(30),
	pLabelName			varchar(66),
	pMapItem			varchar(30),
	pPortalTabOption	integer,
	pShowButtons		int,
	pIsPrimary          int,
	pWidth				integer,
	pHeight				integer,
	pForceRedirect		int)
LANGUAGE plpgsql
AS $$
DECLARE
	vActionId			varchar(16);
	vUIActionId			varchar(16);
BEGIN    	
	
	CALL CreateActionDef(pName, pDescription, pType, vActionId);
	CALL createUIAction(vActionId,pName,pType,pDescription,pUIType,pUIVirtualPageName,pUIPageFlowName, pMapItem,
		pPortalTabOption, pClearValues, pServiceName,pLabelName,pShowButtons,pIsPrimary,pActionCategoryName,pSequence,pWidth,pHeight,pForceRedirect,
		vUIActionId);
			
	UPDATE ActionDef SET UIActionId = vUIActionId WHERE ActionId=vActionId;

end $$;


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('addActionRuleToActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS addActionRuleToActionDef;
 	END IF;
END $$;
CREATE PROCEDURE addActionRuleToActionDef(
	pActionRule			varchar(30), 
	pAction				varchar(30))
LANGUAGE plpgsql
AS $$
DECLARE
	vFieldId		integer;
	vActionId		varchar(16);
	vActionRuleId	varchar(16);
	vSequence		integer;
BEGIN
    
	IF NOT EXISTS (	SELECT * FROM ActionDefActionRules dr 
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = pAction AND r.ActionRuleName = pActionRule
				   ) THEN
	BEGIN
		
		RAISE NOTICE 'Adding ActionRule % to Action %', pActionRule, pAction;

		SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
		SELECT ActionRuleId INTO vActionRuleId FROM ActionRule WHERE ActionRuleName = pActionRule;

		IF vActionId IS NULL THEN
		BEGIN
			RAISE NOTICE 'Action % could not be found', pAction;
			RETURN;
		END;
		END IF;
		
		IF vActionRuleId IS NULL THEN
		BEGIN
			RAISE NOTICE 'ActionRule % could not be found', pActionRule;
			RETURN;
		END;
		END IF;
		
		SELECT f.FieldID INTO vFieldId FROM CDOFields f 
			JOIN CDODefinition d ON d.CDODefID=f.CDODefID
			WHERE f.FieldName='ActionRules' and d.CDOName='ActionDef';
		
		SELECT COUNT(*) + 1 INTO vSequence FROM ActionDefActionRules WHERE ActionId = vActionId;
		
		INSERT INTO ActionDefActionRules 
			(ActionId
			,ActionRulesId
			,FieldId
			,Sequence)
			VALUES
			(vActionId           -- char(16)
			,vActionRuleId		 -- char(16)
			,vFieldId			 -- int
			,vSequence);         -- int
	END;
	ELSE
		RAISE NOTICE 'Action % has already been linked to the ActionRule %', pAction, pActionRule;
	END IF;

end $$;


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('addSourcePageToActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS addSourcePageToActionDef;
 	END IF;
END $$;
CREATE PROCEDURE addSourcePageToActionDef(
	pVirtualPage	varchar(30),
	pAction			varchar(30),
	pExportImportKey varchar(36))
LANGUAGE plpgsql
AS $$
DECLARE
	vActionId		varchar(16);
	vActionRuleId	varchar(16);
	vCDOTypeId		integer;
	vInstanceId		varchar(16);
	vUIVirtualPageId varchar(16);
BEGIN
	
	IF NOT EXISTS (	SELECT * FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = pAction AND v.UIVirtualPageName = pVirtualPage
				   ) THEN
	BEGIN
		RAISE NOTICE 'Adding UISourcePage % to Action %', pVirtualPage, pAction;

		SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
		SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pVirtualPage;  

		IF vActionId IS NULL THEN
		BEGIN
			RAISE NOTICE 'Action % could not be found', pAction;
			RETURN;
		END;
		END IF;
		
		IF vUIVirtualPageId IS NULL THEN
		BEGIN
			RAISE NOTICE 'UIVirtualPage % could not be found', pVirtualPage;
			RETURN;
		END;
		END IF;
		
		SELECT CDODefId FROM INTO vCDOTypeId CDODefinition WHERE CDOName ='UISourcePage';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

		INSERT INTO UISourcePage 
			(ActionId
			,CDOTypeId
			,ChangeCount
			,UISourcePageId
			,UIVirtualPageId
			,ExportImportKey)
			VALUES
			(vActionId           -- char(16)
			,vCDOTypeId			 -- int
			,0					 -- int
			,vInstanceId		 -- char(16)
			,vUIVirtualPageId
			,pExportImportKey);  -- char(16)
			
	END;
	ELSE
		RAISE NOTICE 'Action % has already been linked to the UISourcePage %', pAction, pVirtualPage;
	END IF;

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignRoleIfExist')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignRoleIfExist;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignRoleIfExist(
	pExRoleName VARCHAR(255), 
	pNewRoleId CHAR(16), 
	pNewRoleDescription VARCHAR(255), 
	pOrganizationName VARCHAR(255), 
	pPropagate INT)
LANGUAGE plpgsql
AS $$
DECLARE
    vCDODefId INTEGER;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR(16);
    vRoleGID VARCHAR(36);
	vEmployeeName VARCHAR (500);
BEGIN

	FOR vEmployeeName IN 
        SELECT e.EmployeeName 
        FROM EmployeeRole er
        LEFT JOIN RoleDef rd ON er.RoleId = rd.RoleId
        LEFT JOIN Employee e ON er.EmployeeId = e.EmployeeId
        WHERE rd.RoleName = pExRoleName
    LOOP
	
		SELECT EmployeeId INTO vEmployeeId
		FROM Employee
		WHERE EmployeeName=vEmployeeName;

		vOrgId:=NULL;
		
		IF NOT pOrganizationName IS NULL THEN
			SELECT OrganizationId INTO vOrgId
			FROM Organization
			WHERE OrganizationName=pOrganizationName;
		END IF;			

		vCDODefId := 7782;

		CALL csiPRDGetNextInstanceId(vCDODefId,vIID);
		
		pNewRoleDescription := pNewRoleDescription || vEmployeeName;
		
		CALL csiCreateGUID(pNewRoleDescription, vRoleGID);
		
		INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
        VALUES (vRoleGID,vIID, vCDODefId, pNewRoleId, vEmployeeId, 0, pPropagate, vOrgId);
			   	
	END LOOP;	 

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermissionsForQry
-- DESCR: Creates a Permissions based on a query
--		  @PermissionModesFlag - See rbacCreatePermissions
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreatePermissionsForQry')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreatePermissionsForQry;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreatePermissionsForQry(
	pRoleId CHAR(16), 
	pSecurityList TEXT, 
	pPermissionModesFlag INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
    vSQLString TEXT;
    vObjectMetaId INTEGER;
    vPermissionType INTEGER;
    vPermissionName VARCHAR(255);
	
	r refcursor; 
	rec record;
BEGIN
		 
	vSQLString := 'SELECT CDO.CDODefId AS ObjectMetaId, ' ||
                       'CDO.SecurityTypeId AS PermissionType, ' ||
                       'Labels.LabelValue AS PermissionName ' ||
                 'FROM CDODefinition CDO, Labels ' ||
                 'WHERE Labels.LabelId = CDO.DisplayNameLabelId ' ||
                 'AND CDO.IsAbstract = 0 ' ||
                 'AND CDO.SecurityTypeId IN (' || pSecurityList || ') ' ||
                 'ORDER BY CDO.SecurityTypeId, CDO.CDOName;';
				 
	open r for execute vSQLString; 
	fetch next from r into rec;
	while found 
		loop
			vObjectMetaId := rec.ObjectMetaId;
			vPermissionType := rec.PermissionType;
			vPermissionName := rec.PermissionName;
			
			CALL csiRBACCreatePermission(pRoleId, vPermissionName, vPermissionType, vObjectMetaId, pPermissionModesFlag);
			
			fetch next from r into rec; 
		end loop;
	close r; 

end $$;


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRole
-- DESCR: Assigns a Role to an Employee
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACAssignRole')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACAssignRole;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACAssignRole(
	pRoleId CHAR(16), 
	pRoleDescription VARCHAR(255), 
	pEmployeeName VARCHAR(255), 
	pOrganizationName VARCHAR(255), 
	pPropagate INT)
LANGUAGE plpgsql
AS $$
DECLARE
    vCDODefId INTEGER;
    vEmployeeId CHAR(16);
    vOrgId CHAR(16);
    vIID VARCHAR(16);
    vRoleGID VARCHAR(36);
BEGIN
   
    SELECT EmployeeId INTO vEmployeeId
    FROM Employee
    WHERE EmployeeName=pEmployeeName;

    vOrgId := NULL;
    IF NOT pOrganizationName IS NULL THEN
        SELECT OrganizationId INTO vOrgId
        FROM Organization
        WHERE OrganizationName=pOrganizationName;
	END IF;

    vCDODefId := 7782;

    CALL csiPRDGetNextInstanceId(vCDODefId,vIID);
	
	pRoleDescription := pRoleDescription || pEmployeeName;
    CALL csiCreateGUID(pRoleDescription, vRoleGID);
	
    INSERT INTO EmployeeRole(ExportImportKey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);

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
    
    vRoleId VARCHAR(16);
    vIID VARCHAR(16);
    vSessionId VARCHAR(16);
    vInstanceId VARCHAR(16);    
    vPermissionSQL TEXT;
    vAdminPresent INTEGER;
    vInSiteAdminPresent INTEGER;
    vPermissionModesFlag_None INTEGER := 0;
    vPermissionModesFlag_ReadOnly INTEGER :=1;
    vPermissionModesFlag_NoSecAdmin INTEGER :=2; 
        
BEGIN
    
    RAISE NOTICE 'Creating "DraftPermissions" Role...';	
	CALL csiRBACCreateRole('DraftPermissions','Draft Permissions Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DraftCamstarPermissions" Role...'; 
	CALL csiRBACCreateRole('DraftCamstarPermissions','Draft Camstar Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DraftPLMPermissions" Role...'; 
	CALL csiRBACCreateRole('DraftPLMPermissions','Draft PLM Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DEPCPermissions" Role...'; 
	CALL csiRBACCreateRole('DEPCPermissions','Deployment Complete Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "DEPIPermissions" Role...'; 
	CALL csiRBACCreateRole('DEPIPermissions','Deployment Incomplete Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	RAISE NOTICE 'Creating "PackageCreator" Role...';
	CALL csiRBACCreateRole('Package Creator','Package Creator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'Start Change Pkg', 120, 8500, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	
	RAISE NOTICE 'Creating "PackageOwner" Role...';
	CALL csiRBACCreateRole('Package Owner','Package Owner Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Routing Sheet Maint', 110, 7820, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);



	
	
	RAISE NOTICE 'Creating "Package Deployer" Role...'; 
	CALL csiRBACCreateRole('Package Deployer','Package Deployer Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	


	
	RAISE NOTICE 'Creating "Package Activator" Role...'; 
	CALL csiRBACCreateRole('Package Activator','Package Activator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'ActivateChangePkg', 120, 8528, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Activation Inquiry', 140, 8554, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Export/Import Controller', 160, 7392, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Import Status Inquiry', 140, 7397, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Modeling data Import', 150, 7391, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

    
	RAISE NOTICE 'Creating "Package Approver" Role...';
	CALL csiRBACCreateRole('Package Approver','Package Approver Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	CALL csiRBACCreatePermission(vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	



	RAISE NOTICE 'Creating "Package Collaborator" Role...'; 
	CALL csiRBACCreateRole('Package Collaborator','Package Collaborator Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly); 
	CALL csiRBACCreatePermission(vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	

	
	
	RAISE NOTICE 'Creating "PDPermissions" Role...'; 
	CALL csiRBACCreateRole('PDPermissions','Pending Deployment Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

	RAISE NOTICE 'Creating "RejectPermissions" Role...'; 
	CALL csiRBACCreateRole('RejectPermissions','Rejected Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	RAISE NOTICE 'Creating "PACPermissions" Role...'; 
	CALL csiRBACCreateRole('PACPermissions','Pending Approval Camstar Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
		
		
	RAISE NOTICE 'Creating "PAPLMPermissions" Role...'; 
	CALL csiRBACCreateRole('PAPLMPermissions','Pending Approval PLM Permissions Role',vRoleId);
	CALL csiRBACCreatePermission(vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);


	RAISE NOTICE 'Creating "Default Modeling Advanced" Role...'; 
	CALL csiRBACCreateRole('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
	CALL csiRBACAssignRoleIfExist('Default Modeling', vRoleId, 'Modeling Services for Advanced Users', NULL, 0);
    vPermissionSQL := '230';
    CALL csiRBACCreatePermissionsForQry(vRoleId, vPermissionSQL, vPermissionModesFlag_None);
	CALL csiRBACDeleteUnavailablePermissions(230, 'User Query Maint');
	CALL csiRBACDeleteUnavailablePermissions(230, 'Business Rule Handler Maint');
	CALL csiRBACDeleteUnavailablePermissions(230, 'Business Rule Maint');
	CALL csiRBACDeleteUnavailablePermissions(230, 'Scheduled Business Rule Maint');
	CALL csiRBACDeleteUnavailablePermissions(230, 'Summary Table Def Maint');

	RAISE NOTICE 'Assign "User Query Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Business Rule Handler Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Business Rule Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Scheduled Business Rule Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
	RAISE NOTICE 'Assign "Summary Table Def Maint" Permission...';
	CALL csiRBACAssignPermissionToRole('Default Modeling', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
	CALL csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);


	RAISE NOTICE 'Creating "Portal Configuration" Role...';
    CALL csiRBACCreateRole('Portal Configuration','Portal Configuration Role',vRoleId);
    IF (vAdminPresent=1) THEN
    BEGIN
        CALL csiRBACAssignRole(vRoleId,'Portal Configuration Role', 'Administrator', NULL, 0);
    END;
	END IF;
    CALL csiRBACAssignRole(vRoleId,'Portal Configuration Role', 'CamstarAdmin', NULL, 0);
    CALL csiRBACCreatePermission(vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
    CALL csiRBACCreatePermission(vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);
	CALL csiRBACCreatePermission(vRoleId, 'Portal Studio RBAC', 210, 3, vPermissionModesFlag_None);

	
    
	RAISE NOTICE 'Creating Approval Decisions';
	CALL csiCMCreateApprovalDecision('Approved',10,vApprovalDecisionId);
	CALL csiCMCreateApprovalDecision('Rejected',20,vApprovalDecisionId);

	CALL csiCMCreateIDControl('ExportImportTarget',0);
	CALL csiCMCreateIDControl('CPFileSequence',0);

	RAISE NOTICE 'Creating Draft...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft',vCMSpecID,vCMSpecBaseID);
   
	RAISE NOTICE 'Creating Draft...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft Camstar',vCMSpecID,vCMSpecBaseID);
   
	RAISE NOTICE 'Creating Draft...';
	CALL csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft PLM',vCMSpecID,vCMSpecBaseID);
  

    RAISE NOTICE 'Creating Deployment Complete';
    CALL csiCMCreateSpec('Deployment_Complete.png','Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.','Deployment Complete',vCMSpecID, vCMSpecBaseID);
    
    RAISE NOTICE 'Creating Deployment Incomplete';
	CALL csiCMCreateSpec('Deployment_InComplete.png','Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.','Deployment Incomplete',vCMSpecID, vCMSpecBaseID);
       
	RAISE NOTICE 'Creating Pending Approval Camstar';
	CALL csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval Camstar',vCMSpecID, vCMSpecBaseID);
   
	RAISE NOTICE 'Creating Pending Approval PLM';
	CALL csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval PLM',vCMSpecID, vCMSpecBaseID);
   
   
    RAISE NOTICE 'Creating Pending Deployment...';
	CALL csiCMCreateSpec('Open.png','In the Pending Deployment step the Package can be manually deployed to one or more remote targets, changing the status to Deployment Complete or Deployment Incomplete, if deployment fails. If deployment fails, user can redeploy or Close the Package','Pending Deployment',vCMSpecID,vCMSpecBaseID);
       
    RAISE NOTICE 'Creating Rejected...';
	CALL csiCMCreateSpec('Rejected.png','In the Rejected step the package can be Voided or go back to Draft.','Rejected',vCMSpecID, vCMSpecBaseID);
    
       
    RAISE NOTICE 'Creating Workflow...';
	CALL csiCMCreateWorkFlow('No Approval','No Approval', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
     
	RAISE NOTICE 'Creating Workflow Camstar...';
	CALL csiCMCreateWFCamstarPLM('Camstar','Camstar', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
     
    RAISE NOTICE 'Creating Workflow PLM...';
	CALL csiCMCreateWFCamstarPLM('PLM','PLM', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
     
     
    RAISE NOTICE 'Creating Business Rule Export...';
	CALL csiCMCreateBusinessRule('This script will process exports associated with a change packages.','BR_DEPLOY','BRH_DEPLOY','SBR_DEPLOY','ExecuteQueryEX("ChangePackage_GetExportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ExportImportName", 1,Transaction::__Const.DataType.String, CLF::ExportNames, 0);ForEach(CLF::ExportName,CLF::ExportNames){InitQueryParametersEx("ExportName",CLF::ExportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByExportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeployment,ExportName,CLF::ExportName);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Deploy...';
	CALL csiCMCreateBusinessRule('This script will process exports associated with a change packages.','BR_DEPLOYSTATUS','BRH_DEPLOYSTATUS','SBR_DEPLOYSTATUS','ExecuteQueryEX("ChangePackage_GetDeploymentsInQueue", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"Name", 1,Transaction::__Const.DataType.String, CLF::PackageNames, 0);ForEach(CLF::ChangePackageName,CLF::PackageNames){CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeploymentInQueue);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Import...';
	CALL csiCMCreateBusinessRule('This script will process imports (activations) associated with a change packages.','BR_ACTIVATION','BRH_ACTIVATION','SBR_ACTIVATION', 'ExecuteQueryEX("ChangePackage_GetImportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ImportSetName", 1,Transaction::__Const.DataType.String, CLF::ImportNames, 0);ForEach(CLF::ImportName,CLF::ImportNames){InitQueryParametersEx("ImportName",CLF::ImportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByImportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessActivation,ImportSetName,CLF::ImportName);}}',0,5,6,NULL,
	vBusinessRuleId, vSchedBusinessRuleId);
	
	RAISE NOTICE 'Creating Business Rule Notifications...';
	CALL csiCMCreateBusinessRule('This script will support email notifications for Change Package.','BR_NOTIFICATIONS','BRH_NOTIFICATIONS','SBR_NOTIFICATIONS', 'if(CLF::__CDOID.SessionValues and CLF::__CDOID.SessionValues.Factory){Call(CLF::__CDOID.SessionValues.Factory,SendReminderEmails);}',1,NULL,NULL,'4',
	vBusinessRuleId, vSchedBusinessRuleId);

	RAISE NOTICE 'Creating Business Rule RPT Control Loop Limits Calculations...';
	CALL csiCMCreateBusinessRule('This script will run the RPT Control Loop Limits Calculations.','BR_RPTControlLimits','BRH_RPTControlLimits_Monthly', 'SBR_RPTControlLimits', 'CLF::RPTControlLimitsUpdateObject = null;CreateCDO("RPTControlLimitsUpdate", false, false, CLF::RPTControlLimitsUpdateObject);CLF::RPTControlLimitsUpdateObject.RPTUpdateOccurrencePattern = 2;CLF::RPTControlLimitsUpdateObject.NoOfRPTCombinations = 100;CLF::RPTControlLimitsUpdateObject.HistoryTimePeriod = 30;CLF::RPTControlLimitsUpdateObject.Factory = CLF::__CDOID.SessionValues.Factory;Call(CLF::RPTControlLimitsUpdateObject, ProcessRPTControlLimits);',0,NULL,NULL,NULL,vBusinessRuleId, vSchedBusinessRuleId);

--- Change Management Actions

	CALL createActionRule('CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = 1');
	CALL createActionRule('CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = 1 or IsCollaborator = 1');
	CALL createActionRule('CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided');
	CALL createActionRule('CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = 1');
	CALL createActionRule('CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed');
	CALL createActionRule('CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = 1 and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers');
	CALL createActionRule('CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = 1 and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers');
	CALL createActionRule('CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed');
	CALL createActionRule('CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');
	CALL createActionRule('CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1');
	CALL createActionRule('CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');

	CALL createAction('CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	
	CALL createAction('CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMCloseImpPackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'CloseSingleActivation_VP', NULL, 0, 3, 'CloseCPImportStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidImpPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidSingleActivation_VP', NULL, 0, 4, 'VoidCPImportStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMCloseImpPackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'CloseMultiActivation_VP', NULL, 0, 5, 'CloseCPImportStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidImpPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidMultiActivation_VP', NULL, 0, 6, 'VoidCPImportStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0);

	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMActivatePackageAction');
	CALL addActionRuleToActionDef('CMActivatePackageRule', 'CMActivatePackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction');
	
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMCollaboratorRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsAssignApprovalRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsApprovePLMRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMCancelApprovalRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMRouteApprovalRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMPackageDetailsAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMDeployPackageAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMContentHistoryAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMClosePackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMVoidPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMActivationImpactAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMOpenPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction');

	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1');
	
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6');
	
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD');
	
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8');



   
	RAISE NOTICE 'Complete';

end $$;


do $$
begin	
 CALL CMPopulateDefaultData();
end $$;


