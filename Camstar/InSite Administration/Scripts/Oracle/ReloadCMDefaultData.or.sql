--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:ReloadCMDefaultData.sql
-- DESCR: Reloads the default OOB ChangeManagement Specs, Business Rules, Business Rule Handlers, Approval Decision, Action, Action Rules, Workflows. 
--		  This will be run through a batch file csiReloadCMData.bat. This batch file will get executed when user checks on Load Change Management in the Management studio
-- Copyright Siemens 2025  
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiCreateGUID(pPermissionName IN VARCHAR2, pRoleGId OUT VARCHAR2)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110) and 'Security Administration' security type (180)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180)
AS
   v_checksum VARCHAR2(36);

    BEGIN
        select 
 (      SUBSTR(ExportImportKeyGUID, 1, 8) ||
'-' || SUBSTR(ExportImportKeyGUID, 9, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 13, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 17, 4) ||
'-' || SUBSTR(ExportImportKeyGUID, 21)) INTO pRoleGId
from (
select UPPER( RAWTOHEX( DBMS_CRYPTO.Hash (UTL_RAW.CAST_TO_RAW (pPermissionName),2) ) ) as ExportImportKeyGUID  from dual
);

    END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateSpec
-- DESCR: Inserts the CM Specs. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE or Replace PROCEDURE csiCMCreateSpec(pStepIcon IN VARCHAR2, pRoleDescription IN VARCHAR2,pCMSpecName IN VARCHAR2, pInstanceId OUT VARCHAR2, pInstanceId1 OUT VARCHAR2)
AS
   vRoleId CHAR(16);
   reccount Int;
BEGIN
  
    csiPRDGetNextInstanceId(8513,pInstanceId);
    csiPRDGetNextInstanceId(8514,pInstanceId1);
    SELECT count(*) into reccount FROM BusinessProcessSpecBase WHERE BusinessProcessSpecName = pCMSpecName;
     
     IF (reccount = 0) then      
    INSERT INTO BusinessProcessSpecBase
           (BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES (pInstanceId1,pCMSpecName,8514,1,NULL,pInstanceId);
      
          IF pCMSpecName = 'Draft Camstar' THEN
                      INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                  ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
          VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                  ,NULL,NULL,'',NULL,1,1,pStepIcon,'',1,1,0,0,1);
          elsif pCMSpecName = 'Draft PLM' then
                INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                 ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                  ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,1,0,0,1);
        elsif pCMSpecName = 'Draft' then
        INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                 ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                 ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,0,0,1);
        elsif pCMSpecName = 'Pending Approval Camstar' then
               INSERT INTO BusinessProcessSpec
                 (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
               VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,1,1);
        elsif pCMSpecName = 'Pending Approval PLM' then
              INSERT INTO BusinessProcessSpec
                (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
              VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,1,1);
		elsif pCMSpecName = 'Pending Deployment' then
				INSERT INTO BusinessProcessSpec
			   (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
				   ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
				VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
				  ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0,1);
		elsif pCMSpecName = 'Deployment Incomplete' then
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
               ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
			VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
              ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0,2);
		elsif pCMSpecName = 'Deployment Complete' then
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
               ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
			VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
              ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0,2);
		elsif pCMSpecName = 'Rejected' then
			INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
               ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
			VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
              ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0,3);
        else
        INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
               ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval,PackageStatus)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
              ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0,0);
            
        END IF;
       
      DBMS_OUTPUT.put_line('CMSpec'|| pcmspecname);
     DBMS_OUTPUT.put_line(pCMSpecName);
    
    if pCMSpecName ='Draft' then
          select RoleId into vRoleId from RoleDef where RoleName = 'DraftPermissions';
      else
        select RoleId into vRoleId from RoleDef where Description like pCMSpecName||'%';
      end if;
 
     INSERT INTO ChangeMgtSpecAllowableRoles
           (AllowableRolesId,BusinessProcessSpecId,FieldId,Sequence)
     VALUES (vRoleId,pInstanceId,22504,1);
 
	ELSE
      Delete FROM BusinessProcessSpec where BusinessProcessSpecBaseId in (select a.BusinessProcessSpecBaseId from BusinessProcessSpec a, BusinessProcessSpecBase b
      where a.businessprocessspecbaseid = b.businessprocessspecbaseid and b.BusinessProcessSpecName = pCMSpecName);
		
    	Delete FROM BusinessProcessSpecBase WHERE BusinessProcessSpecName = pCMSpecName;
	
  
      INSERT INTO BusinessProcessSpecBase
           (BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES (pInstanceId1,pCMSpecName,8514,1,NULL,pInstanceId);
      
          IF pCMSpecName = 'Draft Camstar' THEN
                      INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
          VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
           ,NULL,NULL,'',NULL,1,1,pStepIcon,'',1,1,0,0);
          elsif pCMSpecName = 'Draft PLM' then
                INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
           ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,1,0,0);
        elsif pCMSpecName = 'Draft' then
        INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
           ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,0,0);
        elsif pCMSpecName = 'Pending Approval Camstar' then
               INSERT INTO BusinessProcessSpec
                 (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
               VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,1);
        elsif pCMSpecName = 'Pending Approval PLM' then
              INSERT INTO BusinessProcessSpec
                (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
                ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
                ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,1);
        else
        INSERT INTO BusinessProcessSpec
           (BusinessProcessSpecBaseId,BusinessProcessSpecId,CDOTypeId,ChangeCount,ChangeHistoryId,Description
           ,ECO,IconId,IsFrozen,Notes,Revision,Status,StepIcon,WIPMsgDefMgrId,AssignApprovers,ResetApprovals, Lockinstances, RequiresApproval)
        VALUES (pInstanceId1,pInstanceId,8513,1,NULL,pRoleDescription
           ,NULL,NULL,'',NULL,1,1,pStepIcon,'',0,0,1,0);
            
        END IF;
       
      DBMS_OUTPUT.put_line('CMSpec'|| pcmspecname);
      DBMS_OUTPUT.put_line(pCMSpecName);
    
      if pCMSpecName ='Draft' then
          select RoleId into vRoleId from RoleDef where RoleName = 'DraftPermissions';
      else
          select RoleId into vRoleId from RoleDef where Description like pCMSpecName||'%';
      end if;
 
     INSERT INTO ChangeMgtSpecAllowableRoles
           (AllowableRolesId,BusinessProcessSpecId,FieldId,Sequence)
     VALUES (vRoleId,pInstanceId,22504,1);
      
      
      DBMS_OUTPUT.put_line('BusinessProcessSpecBase'|| pCMSpecName || 'deleted and inserted');
   
     end if;
   
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWorkFlow
-- DESCR: Inserts the CM Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE  or REPLACE PROCEDURE  csiCMCreateWorkFlow(pWFDescription IN VARCHAR2,pWFName IN VARCHAR2, pInstanceId2 OUT VARCHAR2, pInstanceId3 OUT VARCHAR2, pInstanceId4 OUT VARCHAR2, pInstanceId5 OUT VARCHAR2)
AS
   vCMDRAFT CHAR(16);
   vCMDI CHAR(16);
   vCMDC CHAR(16);
   vCMCL CHAR(16);
   vSpecId CHAR(16);
   vCMPD CHAR(16);
    reccount Int;
	vCMPathSelectorInstanceId CHAR(16);
  vSpecDescription VARCHAR2(255);
BEGIN
      
   csiPRDGetNextInstanceId(8518,pInstanceId2);
   csiPRDGetNextInstanceId(8517,pInstanceId3);
   csiPRDGetNextInstanceId(8578,pInstanceId4);
           
            SELECT count(*) into reccount FROM BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
         If (reccount = 0) then
           
   INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
   INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
     
     vCMDRAFT := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Draft'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
    
   INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('ccfba42a-ca60-4227-8383-7aba97f3a44c',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',45,214);
		
    Update BusinessProcessWorkflow
			set FirstStepId = pInstanceId4
			where BusinessProcessWorkflowId = pInstanceId3;
     
		csiPRDGetNextInstanceId(8578,pInstanceId4);
    vCMDI := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
    
		INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('8d25b828-d7ee-404f-9874-767d1ad39f1b',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',179,77);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDC := pInstanceId4;
   
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;    
   
  INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('36b147dc-92ff-4e90-975e-d2422eb54223',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',212,205);
         
    csiPRDGetNextInstanceId(1440,pInstanceId5);
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('458565e9-422c-4c5a-9aed-a858adecf3af',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);
           
	 Update WorkflowStep	set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;

    csiPRDGetNextInstanceId(1440,pInstanceId5);
	  INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('8e64ba4f-5b60-45a9-920a-29bf429faa55',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);
           
		  
		   csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('245DF453-8CAB-4592-916D-50180F1A4723',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
		    
    csiPRDGetNextInstanceId(1440,pInstanceId5);
	  INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('4a679c35-1b40-4bb6-a315-256ffafd47b3',1440,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);
           
           
        Update WorkflowStep set DefaultPathId = pInstanceId5	where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;

    csiPRDGetNextInstanceId(1440,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('07f12fc3-1a58-4933-9b4f-5a577b07f0e5',1440,1,NULL,NULL,vCMDRAFT,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);

     Update WorkflowStep set DefaultPathId = pInstanceId5	where WorkflowStepName = 'Draft'  and WorkflowId = pInstanceId3;

        
    ELSE
      delete from BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
     
       INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
        VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
        INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
         VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
           
         vCMDRAFT := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Draft'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
    
   INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('ccfba42a-ca60-4227-8383-7aba97f3a44c',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',45,214);
		
    Update BusinessProcessWorkflow
			set FirstStepId = pInstanceId4
			where BusinessProcessWorkflowId = pInstanceId3;
			
		csiPRDGetNextInstanceId(8578,pInstanceId4);
    vCMDI := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
    
		INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('8d25b828-d7ee-404f-9874-767d1ad39f1b',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',179,77);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDC := pInstanceId4;
   
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
   
  INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('36b147dc-92ff-4e90-975e-d2422eb54223',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',212,205);
         
    csiPRDGetNextInstanceId(1440,pInstanceId5);
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('458565e9-422c-4c5a-9aed-a858adecf3af',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);
           
	 Update WorkflowStep	set DefaultPathId = pInstanceId5 where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;

    csiPRDGetNextInstanceId(1440,pInstanceId5);
	  INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('8e64ba4f-5b60-45a9-920a-29bf429faa55',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);
           
		  
		   csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
		    
    csiPRDGetNextInstanceId(1440,pInstanceId5);
	  INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('4a679c35-1b40-4bb6-a315-256ffafd47b3',1440,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);
           
           
        Update WorkflowStep set DefaultPathId = pInstanceId5	where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;

    csiPRDGetNextInstanceId(1440,pInstanceId5);
    INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('07f12fc3-1a58-4933-9b4f-5a577b07f0e5',1440,1,NULL,NULL,vCMDRAFT,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);

     Update WorkflowStep set DefaultPathId = pInstanceId5	where WorkflowStepName = 'Draft'  and WorkflowId = pInstanceId3;

    DBMS_OUTPUT.put_line('BusinessProcessWorkflowBase'|| pWFName || 'deleted and inserted');
    End if;
     
     
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateWorkFlow
-- DESCR: Inserts the CM PLM and Camstar Workflows. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE  or REPLACE PROCEDURE  csiCMCreateWFCamstarPLM(pWFDescription IN VARCHAR2,pWFName IN VARCHAR2, pInstanceId2 OUT VARCHAR2, pInstanceId3 OUT VARCHAR2, pInstanceId4 OUT VARCHAR2, pInstanceId5 OUT VARCHAR2)
AS
   vCMDRAFT CHAR(16);
   vCMDI CHAR(16);
   vCMDC CHAR(16);
   vCMCL CHAR(16);
   vSpecId CHAR(16);
   vCMVOID CHAR(16);
   vCMREJ CHAR(16);
   vCMPD CHAR(16);
   vCMPA CHAR(16);
   vCMPathSelectorInstanceId CHAR(16);
   vCMDRAFTName VARCHAR2(255);
   reccount INT;
   vSpecDescription VARCHAR2(255);
   
BEGIN
      
   csiPRDGetNextInstanceId(8518,pInstanceId2);
   csiPRDGetNextInstanceId(8517,pInstanceId3);
   csiPRDGetNextInstanceId(8578,pInstanceId4);
           
           SELECT count(*) into reccount FROM BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
           
           If (reccount = 0) then     
   INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
   INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
        
     if pWFDescription = 'No Approval' then
		vCMDRAFTName := 'Draft';  
     elsif pWFDescription = 'Camstar' then
		vCMDRAFTName := 'Draft Camstar';  
     elsif pWFDescription = 'PLM' then
		vCMDRAFTName := 'Draft PLM';  
    End If;
    

    vCMDRAFT := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = vCMDRAFTName
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;    
    
    
     if pWFDescription = 'No Approval' then
      INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1fbf9172-3525-476d-9eed-5e869a4d2f00',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',37,24);
        elsif  pWFDescription = 'Camstar' then
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('84ff7c87-122b-4e5b-a7c7-c8784836791a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft Camstar',37,24);
    elsif pWFDescription = 'PLM' then
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('c6b8c4a2-0c30-47f2-a6f2-9f81a43e157a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft PLM',37,24);
       
       end if;
		
     
    Update BusinessProcessWorkflow
			set FirstStepId = pInstanceId4
			where BusinessProcessWorkflowId = pInstanceId3;
     
     
       
       
    csiPRDGetNextInstanceId(8578,pInstanceId4);
     vCMPA := pInstanceId4;
         
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Approval ' || pWFDescription
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;         
     
    
	 if  pWFDescription = 'Camstar' then
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('e25e9d3a-fe51-4f48-9640-04064abcad15',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval Camstar',35,137);
	elsif pWFDescription = 'PLM' then
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('27df4f12-d414-4535-b62f-bdf4a505e485',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval PLM',35,137);
	end if;
			
      
		csiPRDGetNextInstanceId(8578,pInstanceId4);
    vCMPD := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Deployment'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
     
	INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1f7db9c4-5737-4ac4-a2dd-89ab47c85fb0',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Deployment',32,248);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMREJ := pInstanceId4;
   
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Rejected'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
   
   INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('fbdd0b51-2e43-47d9-ac29-d7f9b6eaf07f',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Rejected',180,135);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDC := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
    
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('1f3f665c-583d-4aea-b648-e07227764b5b',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',277,365);
         
         
         csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDI := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
    
     INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('de0081b2-d8e8-4460-bb6c-5540a480352a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',391,254);
         
		 
    csiPRDGetNextInstanceId(1440,pInstanceId5);
		
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('24980df0-db54-454f-bfae-9019740bcbe9',1440,1,NULL,NULL,vCMDraft,0,NULL,
           pInstanceId5,'Pending Approval',NULL, NULL,vCMPA,NULL);
           
           
	Update WorkflowStep
			set DefaultPathId = pInstanceId5
				where WorkflowStepName = vCMDRAFTName and WorkflowId = pInstanceId3;


    csiPRDGetNextInstanceId(1440,pInstanceId5);
           
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('d84b65c1-e7a8-4b17-ba1b-b5b9520e7372',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Pending Deployment',NULL, NULL,vCMPD,NULL);
      Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Pending Approval' and WorkflowId = pInstanceId3;
           
    csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);

    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('F3A54DBA-568A-464A-801F-E0B9FEA4552D',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Approved');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMPA);

     
    csiPRDGetNextInstanceId(1440,pInstanceId5);
          
		INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('93fc44ef-1c3b-48ab-a021-21b424e89ee0',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Rejected',NULL, NULL,vCMREJ,NULL);
           
      csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);

    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('32F00D0C-4486-470B-98ED-FB1EB2AC0AE2',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Rejected');
           
    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,2,vCMPA);
      

    csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Draft',NULL, NULL,vCMDraft,NULL);

		   csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);


    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('1C7AF084-F98F-4825-A0EF-6D71891E84E6',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Pending or TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Cancelled');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,3,vCMPA);


    
 csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',1440,1,NULL,NULL,vCMPD,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Pending Deployment' and WorkflowId = pInstanceId3;
      
       csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('157222b5-582c-4955-a1f3-55deb75f2907',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;
      
       csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('076c6a19-f638-4745-a3e2-788d94c42395',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);


           
		    csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
           
         csiPRDGetNextInstanceId(1440,pInstanceId5);
          
		INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('d57d4000-9dc5-412b-be63-db7363429e2c',1440,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;
      
     
    ELSE
    delete from BusinessProcessWorkflowBase WHERE BusinessProcessWorkflowName = pWFName;
     
     INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
   INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
     
      if pWFDescription = 'No Approval' then
		vCMDRAFTName := 'Draft';  
     elsif pWFDescription = 'Camstar' then
		vCMDRAFTName := 'Draft Camstar';  
     elsif pWFDescription = 'PLM' then
		vCMDRAFTName := 'Draft PLM';  
    End If;
    

    vCMDRAFT := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = vCMDRAFTName
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;         
    
    
     if pWFDescription = 'No Approval' then
      INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1fbf9172-3525-476d-9eed-5e869a4d2f00',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft',37,24);
        elsif  pWFDescription = 'Camstar' then
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('84ff7c87-122b-4e5b-a7c7-c8784836791a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft Camstar',37,24);
    elsif pWFDescription = 'PLM' then
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('c6b8c4a2-0c30-47f2-a6f2-9f81a43e157a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Draft PLM',37,24);
       
       end if;
		
     
    Update BusinessProcessWorkflow
			set FirstStepId = pInstanceId4
			where BusinessProcessWorkflowId = pInstanceId3;
     
     
       
       
    csiPRDGetNextInstanceId(8578,pInstanceId4);
     vCMPA := pInstanceId4;
         
    select BusinessProcessSpecId
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Approval ' || pWFDescription
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
     
    
	 if  pWFDescription = 'Camstar' then
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('e25e9d3a-fe51-4f48-9640-04064abcad15',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval Camstar',35,137);
	elsif pWFDescription = 'PLM' then
			INSERT INTO WorkflowStep
			(ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
			VALUES('27df4f12-d414-4535-b62f-bdf4a505e485',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Approval PLM',35,137);
	end if;
			
      
		csiPRDGetNextInstanceId(8578,pInstanceId4);
    vCMPD := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Pending Deployment'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;      
    
	INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
    VALUES('1f7db9c4-5737-4ac4-a2dd-89ab47c85fb0',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Pending Deployment',32,248);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMREJ := pInstanceId4;
   
    select BusinessProcessSpecId 
      into vSpecId
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Rejected'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
   
   INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('fbdd0b51-2e43-47d9-ac29-d7f9b6eaf07f',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,1,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Rejected',180,135);
			
   csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDC := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Complete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;    
    
    INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('1f3f665c-583d-4aea-b648-e07227764b5b',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Complete',277,365);
         
         
         csiPRDGetNextInstanceId(8578,pInstanceId4);
   vCMDI := pInstanceId4;
    
    select BusinessProcessSpecId 
      into vSpecId
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = 'Deployment Incomplete'
	   and BusinessProcessSpec.Revision = '1'; 	
     
    select Description 
      into vSpecDescription 
      from BusinessProcessSpec 
     where BusinessProcessSpecId = vSpecId;     
    
     INSERT INTO WorkflowStep
     (ExportImportKey,CDOTypeId,ChangeCount,DefaultPathId,Description,IconId,IsFrozen,IsLastStep,Notes,OnDefaultRoute
           ,RouteStepId,SchedulingDetailId,Sequence,SpecBaseId,SpecId,StepType
           ,SubWorkflowBaseId,SubWorkflowId,WIPMsgLabel,WorkflowId,WorkflowStepId,WorkflowStepName,Xlocation,Ylocation)
   VALUES('de0081b2-d8e8-4460-bb6c-5540a480352a',8578,1,NULL,vSpecDescription,NULL,0,0,NULL,0,
			 NULL,NULL,1,'0000000000000000',vSpecId,1, 
			 NULL,NULL,NULL,pInstanceId3,pInstanceId4,'Deployment Incomplete',391,254);
         
       
    csiPRDGetNextInstanceId(1440,pInstanceId5);
		
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('24980df0-db54-454f-bfae-9019740bcbe9',1440,1,NULL,NULL,vCMDraft,0,NULL,
           pInstanceId5,'Pending Approval',NULL, NULL,vCMPA,NULL);
           
           
	Update WorkflowStep
			set DefaultPathId = pInstanceId5
				where WorkflowStepName = vCMDRAFTName and WorkflowId = pInstanceId3;


    csiPRDGetNextInstanceId(1440,pInstanceId5);
           
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('d84b65c1-e7a8-4b17-ba1b-b5b9520e7372',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Pending Deployment',NULL, NULL,vCMPD,NULL);
      Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Pending Approval' and WorkflowId = pInstanceId3;
           
    csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);

    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('F3A54DBA-568A-464A-801F-E0B9FEA4552D',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Approved');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMPA);

     
    csiPRDGetNextInstanceId(1440,pInstanceId5);
          
		INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('93fc44ef-1c3b-48ab-a021-21b424e89ee0',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Rejected',NULL, NULL,vCMREJ,NULL);
           
      csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);

    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('32F00D0C-4486-470B-98ED-FB1EB2AC0AE2',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Rejected');
           
    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,2,vCMPA);
      

    csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',1440,1,NULL,NULL,vCMPA,0,NULL,
           pInstanceId5,'Draft',NULL, NULL,vCMDraft,NULL);

		   csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);


    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('1C7AF084-F98F-4825-A0EF-6D71891E84E6',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMPA,1,0,NULL,
           'TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Pending or TrackableObject.ApprovalStatus == Transaction::__Const.ApprovalStatus.Cancelled');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,3,vCMPA);


    
 csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('7654a066-bd9d-4d17-9062-592f6ec3f381',1440,1,NULL,NULL,vCMPD,0,NULL,
           pInstanceId5,'Deployment Complete',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Pending Deployment' and WorkflowId = pInstanceId3;
      
       csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('157222b5-582c-4955-a1f3-55deb75f2907',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Complete_2',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Deployment Complete' and WorkflowId = pInstanceId3;
      
       csiPRDGetNextInstanceId(1440,pInstanceId5);
          
	INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('076c6a19-f638-4745-a3e2-788d94c42395',1440,1,NULL,NULL,vCMDC,0,NULL,
           pInstanceId5,'Deployment Incomplete',NULL, NULL,vCMDI,NULL);


           
		    csiPRDGetNextInstanceId(1840,vCMPathSelectorInstanceId);
    INSERT INTO PathSelector
           (ExportImportKey,CDOTypeId,ChangeCount,Description,PathSelectorId,PathId,StepId,Status,IsFrozen,Notes,
           Expression)
    VALUES
           ('6523211D-1DAC-41D3-9C8F-2C7B628FC020',1840,1,NULL,vCMPathSelectorInstanceId,pInstanceId5,vCMDC,1,0,NULL,
           'TrackableObject.DeploymentFailed');

    INSERT INTO WorkflowStepPathSelectors
           (FieldId,PathSelectorsId,Sequence,WorkflowStepId)
    VALUES
           (4403,vCMPathSelectorInstanceId,1,vCMDC);
           
         csiPRDGetNextInstanceId(1440,pInstanceId5);
          
		INSERT INTO Path
           (ExportImportKey,CDOTypeId,ChangeCount,Description,EndReworkStepId,FromStepId,IsFrozen,Notes
           ,PathId,PathName,ReEntryStepId,ReturnToStepId,ToStepId,TxnDetailsId)
     VALUES
           ('d57d4000-9dc5-412b-be63-db7363429e2c',1440,1,NULL,NULL,vCMDI,0,NULL,
           pInstanceId5,'Deployment Complete_1',NULL, NULL,vCMDC,NULL);
     Update WorkflowStep
			set DefaultPathId = pInstanceId5
			where WorkflowStepName = 'Deployment Incomplete' and WorkflowId = pInstanceId3;
      DBMS_OUTPUT.put_line('BusinessProcessWorkflowBase'|| pWFName || 'deleted and inserted');
     
  end if;
         
END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateBusinessRule
-- DESCR: Inserts the CM BusinessRules. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE  or REPLACE PROCEDURE  csiCMCreateBusinessRule(pExportImportKey IN VARCHAR2,pBRDescription IN VARCHAR2, pBRName IN VARCHAR2, pBRHName IN VARCHAR2, pSBRName IN VARCHAR2, pBRSCRIPT IN VARCHAR2, pSBRIsAdvancedMode IN NUMBER, pSBRRecurrenceFrequency IN NUMBER,pSBRRecurrencePattern IN NUMBER, pSBRScheduleHours IN VARCHAR2, pInstanceId1 OUT VARCHAR2, pInstanceId2 OUT VARCHAR2)

AS
   vBRDataInstanceId CHAR(16);
   vBRHandlerInstanceId CHAR(16);
   vBRHandlerDataInstanceId CHAR(16);
   reccount INT;
   vDueDate TIMESTAMP;
   vDueDateGMT TIMESTAMP;
 
BEGIN
      
   csiPRDGetNextInstanceId(7569,pInstanceId1);
   csiPRDGetNextInstanceId(7573,vBRDataInstanceId);
   csiPRDGetNextInstanceId(7564,vBRHandlerInstanceId);
   csiPRDGetNextInstanceId(7567,vBRHandlerDataInstanceId);
   csiPRDGetNextInstanceId(7587,pInstanceId2);
           
          SELECT count(*) into reccount FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = pBRHName;
          if (reccount = 0) then
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
           (vBRHandlerDataInstanceId,vBRHandlerInstanceId,pBRHName,7564,1,NULL,pBRDescription,NULL,0,1,NULL,0);
    
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
    VALUES(1,vBRHandlerDataInstanceId,pBRHName,vBRHandlerInstanceId, 7567,1,0,pBRSCRIPT,NULL);
	
	
    ELSE
    delete  FROM BusinessRuleHandler WHERE BusinessRuleHandlerName = pBRHName;
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
           (vBRHandlerDataInstanceId,vBRHandlerInstanceId,pBRHName,7564,1,NULL,pBRDescription,NULL,0,1,NULL,0);
           
    DBMS_OUTPUT.put_line('BusinessRuleHandler'|| pBRHName || 'deleted and inserted');
	
	delete  FROM BusinessRuleHandlerData WHERE BusinessRuleHandlerDataName = pBRHName;
	
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
    VALUES(1,vBRHandlerDataInstanceId,pBRHName,vBRHandlerInstanceId, 7567,1,0,pBRSCRIPT,NULL);
	
    		END if;
           
           
    
     
     SELECT count(*) into reccount FROM BusinessRule WHERE BusinessRuleName = pBRName;
           If (reccount = 0) then 
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
    VALUES(vBRDataInstanceId,pInstanceId1,pBRName,7569,1,NULL,pBRDescription,NULL,0,NULL);

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
    VALUES(pExportImportKey,1,NULL,vBRDataInstanceId,pBRName,pInstanceId1,7573,1,1140,0,0);
		
    ELSE
    delete FROM BusinessRule WHERE BusinessRuleName = pBRName;
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
    VALUES(vBRDataInstanceId,pInstanceId1,pBRName,7569,1,NULL,pBRDescription,NULL,0,NULL);

    DBMS_OUTPUT.put_line('BusinessRule'|| pBRName || 'deleted and inserted');
	
	delete FROM BusinessRuleData WHERE BusinessRuleDataName = pBRName;
	
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
    VALUES(pExportImportKey,1,NULL,vBRDataInstanceId,pBRName,pInstanceId1,7573,1,1140,0,0);
		
    END IF;
	
			
	INSERT INTO BusinessRuleDataHandlers
           (BusinessRuleDataId
           ,FieldId
           ,HandlersId
           ,Sequence)
    VALUES(vBRDataInstanceId,13314,vBRHandlerInstanceId,1);


	IF ( pSBRIsAdvancedMode = 1) THEN
		vDueDate := CASE WHEN TO_CHAR(SYSDATE,'HH24') > 4 THEN TRUNC(SYSDATE + 1) ELSE TRUNC(SYSDATE) END  + INTERVAL '4' HOUR;
	ELSE
		vDueDate := SYSDATE;
	END IF;
	
	vDueDateGMT := vDueDate AT TIME ZONE 'GMT';
	
	
	SELECT count(*) into reccount FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = pSBRName;
	IF (reccount  = 0) then
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
	VALUES(7587,1,NULL,NULL,NULL,NULL,vDueDate,vDueDateGMT,
			NULL,NULL,'0004740000000001',1140,NULL,pSBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,pInstanceId1,NULL,pSBRRecurrenceFrequency,pSBRRecurrencePattern,
			pInstanceId2,pSBRName,pSBRScheduleHours,SYSDATE,SYSDATE,1);
               
    ELSE
    delete FROM ScheduledBusinessRule WHERE ScheduledBusinessRuleName = pSBRName;
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
	VALUES(7587,1,NULL,NULL,NULL,NULL,vDueDate,vDueDate,
			NULL,NULL,'0004740000000001',1140,NULL,pSBRIsAdvancedMode,0,0,0,NULL,NULL,NULL,pInstanceId1,NULL,pSBRRecurrenceFrequency,pSBRRecurrencePattern,
			pInstanceId2,pSBRName,pSBRScheduleHours,SYSDATE,SYSDATE,1);
    DBMS_OUTPUT.put_line('ScheduledBusinessRule'|| pSBRName || 'deleted and inserted');
		END IF;
         
         
END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateRole
-- DESCR: Inserts the CM related roles. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiRBACCreateRole(pRoleName IN VARCHAR2, pRoleDescription IN VARCHAR2, pInstanceId OUT VARCHAR2)
AS
   vRoleCDODefId NUMBER := 7130;
   reccount INT;
BEGIN
    csiPRDGetNextInstanceId(vRoleCDODefId,pInstanceId);
     SELECT count(*) into reccount FROM RoleDef WHERE RoleName = pRoleName;
           If (reccount = 0) then
    INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
             ELSE
             delete from RoleDef WHERE RoleName = pRoleName;
              INSERT INTO RoleDef(RoleId, CDOTypeId, Notes, ChangeCount, Description, ChangeHistoryId, IsFrozen, IconId, RoleName)
       VALUES (pInstanceId, vRoleCDODefId, NULL, 1, pRoleDescription, NULL, 0, 0, pRoleName);
                 DBMS_OUTPUT.put_line('RoleDef'|| pRoleName || 'deleted and inserted');
		
    end if;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreatePermission
-- DESCR: Inserts the CM realted role permissions. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiRBACCreatePermission(pRoleId IN VARCHAR2, pPermissionName IN VARCHAR2, pPermissionType IN NUMBER, pObjectMetaId IN NUMBER, pPermissionModesFlag IN NUMBER, pObjectInstanceId IN VARCHAR2 DEFAULT NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110), 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
AS
    vIID VARCHAR2(16);
	 vRoleGID VARCHAR2(36);
BEGIN
    csiPRDGetNextInstanceId(7783,vIID);
    INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
			VALUES(vRoleGID,vIID,7783,pRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);

    -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
    -- based on the pPermissionModesFlag value
	IF ( pPermissionModesFlag = 0 ) THEN
       INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=PPermissionType
          ORDER BY BitNumber;
    ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
		INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
		  SELECT vIID, 15263, BitNumber, ROWNUM
		  FROM SecurityMaskDetail
		  WHERE SecurityMaskId=PPermissionType
          AND BitNumber = 2
		  ORDER BY BitNumber;
    ELSIF ( pPermissionModesFlag = 2 AND pPermissionType IN (110, 180, 230) ) THEN
		INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
		  SELECT vIID, 15263, BitNumber, ROWNUM
		  FROM SecurityMaskDetail
		  WHERE SecurityMaskId=PPermissionType
          AND BitNumber IN (1,2,3,4)
		  ORDER BY BitNumber;
	ELSE
		DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for @PermissionModeFlag parameter...');
	END IF;

END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: csiCMCreateApprovalDecision
-- DESCR: Inserts the CM approval decision. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE or Replace PROCEDURE csiCMCreateApprovalDecision(pDecisionName IN VARCHAR2, pDecisionType IN INT, pInstanceId OUT VARCHAR2)
AS
   reccount Int;
BEGIN
  
    csiPRDGetNextInstanceId(7857,pInstanceId);

SELECT count(*) into reccount FROM ApprovalDecision WHERE ApprovalDecisionName = pDecisionName;
           If (reccount = 0) then
	INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
	VALUES (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);

    ELSE
    delete from ApprovalDecision WHERE ApprovalDecisionName = pDecisionName;
    INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
	VALUES (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);

		DBMS_OUTPUT.put_line('ApprovalDecision '|| pDecisionName || 'deleted and inserted');
    END IF;

  
  
         
END;
/

CREATE or Replace PROCEDURE csiCMCreateIDControl(pIDType IN VARCHAR2, pNextId IN INT)
AS
   reccount Int;
BEGIN
  
   
	SELECT count(*) into reccount FROM IDControl WHERE IDType = pIDType;
    If (reccount = 0) then
		INSERT INTO IDControl (IDType,  NextID) 
        VALUES (pIDType, pNextId); 
    ELSE
		DBMS_OUTPUT.put_line('IDControl '|| pIDType || 'exist');
    END IF;
	         
END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
CREATE OR REPLACE PROCEDURE CreateActionRule(
	pName               IN VARCHAR2, 
	pDescription			  IN VARCHAR2, 
	pExpression				  IN VARCHAR2)
AS
	vCDOTypeId	NUMBER;
	vInstanceId VARCHAR2(16);
BEGIN

  BEGIN
    SELECT ActionRuleId INTO vInstanceId FROM ActionRule WHERE ActionRuleName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN vInstanceId := NULL; 
  END;

  IF (vInstanceId IS NULL) THEN
  
    DBMS_OUTPUT.PUT_LINE('Inserting ActionRule: ' || pName);
    
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

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
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nVARCHAR(255)
			,pExpression				-- nVARCHAR(255)
			,0);                -- bit

  ELSE
  delete FROM ActionRule WHERE ActionRuleName = pName;
  DBMS_OUTPUT.PUT_LINE('Inserting ActionRule: ' || pName);
    
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

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
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nVARCHAR(255)
			,pExpression				-- nVARCHAR(255)
			,0);                -- bit
      DBMS_OUTPUT.PUT_LINE('ActionRule ' || pName || ' deleted and inserted');
  END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: CreateActionCategory
-- DESCR: Helper function to create an Action Category record
--
CREATE OR REPLACE PROCEDURE CreateActionCategory(
	pName               IN VARCHAR2, 
	pLabelName   			  IN VARCHAR2, 
	pSequence           IN NUMBER)
AS
	vCDOTypeId	NUMBER;
	vInstanceId VARCHAR2(16);
BEGIN

  BEGIN
    SELECT ActionCategoryId INTO vInstanceId FROM ActionCategory WHERE ActionCategoryName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN vInstanceId := NULL; 
  END;

  IF (vInstanceId IS NULL) THEN
  
    DBMS_OUTPUT.PUT_LINE('Inserting ActionCategory: ' || pName);
  
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

		INSERT INTO ActionCategory
			(ActionCategoryId
			,ActionCategoryName
			,CDOTypeId
			,ChangeCount
			,LabelName 
			,LabelText 
			,Sequence
			,IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName         -- nVARCHAR(50)
      ,NULL
			,pSequence  				-- int
			,0);                -- bit
  
  ELSE
   DBMS_OUTPUT.PUT_LINE('Inserting ActionCategory: ' || pName);
  delete FROM ActionCategory WHERE ActionCategoryName = pName;
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

		INSERT INTO ActionCategory
			(ActionCategoryId
			,ActionCategoryName
			,CDOTypeId
			,ChangeCount
			,LabelName 
			,LabelText 
			,Sequence
			,IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName         -- nVARCHAR(50)
      ,NULL
			,pSequence  				-- int
			,0);                -- bit
  
      DBMS_OUTPUT.PUT_LINE('ActionCategory ' || pName || ' deleted and inserted');
  END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create Action records
--
CREATE OR REPLACE PROCEDURE CreateActionDef(
	pName               IN VARCHAR2, 
	pDescription			  IN VARCHAR2, 
  pType               IN NUMBER,
	pInstanceId				  OUT VARCHAR2)
AS
	vCDOTypeId	NUMBER;
BEGIN

  BEGIN
    SELECT ActionId INTO pInstanceId FROM ActionDef WHERE ActionName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
  END;

  IF (pInstanceId IS NULL) THEN
  
      DBMS_OUTPUT.PUT_LINE('Inserting Action: ' || pName);

      SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
      csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
  
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
        ,pName			      	-- nVARCHAR(30)
        ,pType      				-- int
        ,vCDOTypeId         -- int
        ,1                  -- int
        ,pDescription       -- nVARCHAR(255)
        ,0);                -- bit
  
  ELSE
  DBMS_OUTPUT.PUT_LINE('Inserting Action: ' || pName);
delete FROM ActionDef WHERE ActionName = pName;
      SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
      csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
  
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
        ,pName			      	-- nVARCHAR(30)
        ,pType      				-- int
        ,vCDOTypeId         -- int
        ,1                  -- int
        ,pDescription       -- nVARCHAR(255)
        ,0);                -- bit
  
        DBMS_OUTPUT.PUT_LINE('Action ' || pName || ' deleted and inserted');
  END IF;

END;
/

-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
CREATE OR REPLACE PROCEDURE CreateUIAction(
	pActionId			      IN VARCHAR2,
	pName				        IN VARCHAR2,
	pType				        IN NUMBER,
	pDescription		    IN VARCHAR2,
	pUIType				      IN VARCHAR2,
	pUIVirtualPageName	IN VARCHAR2,
	pUIPageFlowName	    IN VARCHAR2,
	pMapItem			      IN VARCHAR2,
  pPortalTabOption    IN NUMBER,
	pClearValues		    IN NUMBER,
	pServiceName		    IN VARCHAR2,
	pLabelName			    IN VARCHAR2,
	pShowButtons		    IN NUMBER, 
	pIsPrimary		      IN NUMBER, 
	pActionCategoryName	IN VARCHAR2,
	pSequence	          IN NUMBER,
  pWidth              IN NUMBER,
  pHeight             IN NUMBER,
  pInstanceId         OUT VARCHAR2)
  
AS
	vCDOTypeId	      NUMBER;	
	vUIVirtualPageId	VARCHAR2(16);
	vUIPageFlowId		  VARCHAR2(16);
	vActionCategoryId	VARCHAR2(16);
  vFloatPageLocationId VARCHAR2(16);

BEGIN
  BEGIN
    SELECT UIActionId INTO pInstanceId FROM UIAction WHERE UIActionName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
  END;

  IF (pInstanceId IS NULL) THEN
		DBMS_OUTPUT.PUT_LINE('Inserting UIAction: ' || pName);

    BEGIN
      IF LENGTH(pUIVirtualPageName) > 0 THEN
        SELECT UIVirtualPageId INTO vUIVirtualPageId from UIVirtualPage where UIVirtualPageName = pUIVirtualPageName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIVirtualPageId := NULL; 
    END;

    BEGIN
      IF LENGTH(pUIPageFlowName) > 0 THEN
        SELECT UIPageFlowId INTO vUIPageFlowId from UIPageFlow where UIPageFlowName = pUIPageFlowName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIPageFlowId := NULL; 
    END;
				
    BEGIN
      IF LENGTH(pActionCategoryName) > 0 THEN
        SELECT ActionCategoryId INTO vActionCategoryId from ActionCategory where ActionCategoryName = pActionCategoryName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vActionCategoryId := NULL; 
    END;
				
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- int
      ,0                  -- int
			,pLabelName		    	-- nVARCHAR(30)
			,NULL     		    	-- nVARCHAR(255)
			,pActionId		  	  -- char(16)
			,0                  -- int
			,pServiceName       -- nVARCHAR(30)
      ,pSequence          -- int
			,pShowButtons	    	-- bit
			,pInstanceId        -- char(16)
			,pName              -- nVARCHAR(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId      -- char(16)
			,pMapItem           -- nVARCHAR(30)
			,pPortalTabOption		-- int
			,0);                -- int
      
    IF pWidth IS NOT NULL and pHeight IS NOT NULL THEN
        SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
        csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);
        
        UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
    END IF;
      
	ELSE
  DBMS_OUTPUT.PUT_LINE('Inserting UIAction: ' || pName);
delete FROM UIAction WHERE UIActionName = pName;
    BEGIN
      IF LENGTH(pUIVirtualPageName) > 0 THEN
        SELECT UIVirtualPageId INTO vUIVirtualPageId from UIVirtualPage where UIVirtualPageName = pUIVirtualPageName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIVirtualPageId := NULL; 
    END;

    BEGIN
      IF LENGTH(pUIPageFlowName) > 0 THEN
        SELECT UIPageFlowId INTO vUIPageFlowId from UIPageFlow where UIPageFlowName = pUIPageFlowName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIPageFlowId := NULL; 
    END;
				
    BEGIN
      IF LENGTH(pActionCategoryName) > 0 THEN
        SELECT ActionCategoryId INTO vActionCategoryId from ActionCategory where ActionCategoryName = pActionCategoryName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vActionCategoryId := NULL; 
    END;
				
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- int
      ,0                  -- int
			,pLabelName		    	-- nVARCHAR(30)
			,NULL     		    	-- nVARCHAR(255)
			,pActionId		  	  -- char(16)
			,0                  -- int
			,pServiceName       -- nVARCHAR(30)
      ,pSequence          -- int
			,pShowButtons	    	-- bit
			,pInstanceId        -- char(16)
			,pName              -- nVARCHAR(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId      -- char(16)
			,pMapItem           -- nVARCHAR(30)
			,pPortalTabOption		-- int
			,0);                -- int
      
    IF pWidth IS NOT NULL and pHeight IS NOT NULL THEN
        SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
        csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);
        
        UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
    END IF;
      
      DBMS_OUTPUT.PUT_LINE('UIAction ' || pName || 'deleted and inserted');
  END IF;
END;
/

--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
CREATE OR REPLACE PROCEDURE CreateAction(
	pName						    IN VARCHAR2,
	pActionCategoryName	IN VARCHAR2,
	pType						    IN NUMBER,
	pDescription		    IN VARCHAR2,
	pUIType					    IN VARCHAR2,
	pUIVirtualPageName	IN VARCHAR2,
	pUIPageFlowName			IN VARCHAR2,
	pClearValues		    IN NUMBER,
	pSequence				    IN NUMBER,
	pServiceName		    IN VARCHAR2,
	pLabelName			    IN VARCHAR2,
	pMapItem				    IN VARCHAR2,
  pPortalTabOption    IN NUMBER,
	pShowButtons		    IN NUMBER,
  pIsPrimary		      IN NUMBER,
  pWidth              IN NUMBER,
  pHeight             IN NUMBER)
AS
	vActionId			    VARCHAR2(16);
	vUIActionId			  VARCHAR2(16);
BEGIN
	/* It is currently unclear to me why the UIAction has a Name or Description */
	CreateActionDef(pName, pDescription, pType, vActionId);
	CreateUIAction(vActionId, pName, pType, pDescription, pUIType, pUIVirtualPageName, 
            pUIPageFlowName, pMapItem, pPortalTabOption, pClearValues, pServiceName, pLabelName,
            pShowButtons, pIsPrimary, pActionCategoryName, pSequence, pWidth, pHeight, vUIActionId);
			
	UPDATE ActionDef SET UIActionId = vUIActionId WHERE ActionId = vActionId;

END;
/


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
CREATE OR REPLACE PROCEDURE AddActionRuleToActionDef(
	pActionRule         IN VARCHAR2, 
	pAction     			  IN VARCHAR2)
AS
  vFieldId		    NUMBER;
	vActionId		    VARCHAR2(16);
  vActionRuleId   VARCHAR2(16);
	vSequence		    NUMBER;
	vCount  		    NUMBER;
	
BEGIN
	
  BEGIN
    SELECT COUNT(*) INTO vCount FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = pAction AND r.ActionRuleName = pActionRule;
    EXCEPTION  WHEN NO_DATA_FOUND THEN vCount := 0; 
  END;

  IF (vCount = 0) THEN
		DBMS_OUTPUT.PUT_LINE('Adding ActionRule ' || pActionRule || ' to Action ' || pAction);
		
    BEGIN
      SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' could not be found');
    END;
    
    BEGIN
      SELECT ActionRuleId INTO vActionRuleId FROM ActionRule WHERE ActionRuleName = pActionRule;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('ActionRule ' || pActionRule || ' could not be found');
    END;
    
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
	ELSE
  		DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' has been already linked to the ActionRule ' ||  pActionRule);
	END IF;
END;
/

-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
CREATE OR REPLACE PROCEDURE AddSourcePageToActionDef(
	pVirtualPage        IN VARCHAR2, 
	pAction     			  IN VARCHAR2,
	pExportImportKey   IN VARCHAR2)
AS

  vActionId		      VARCHAR2(16);
  vCDOTypeId		    NUMBER;
  vInstanceId   		VARCHAR2(16);
  vUIVirtualPageId  VARCHAR2(16);
	vCount  		      NUMBER;
	
BEGIN
	
  BEGIN
    SELECT COUNT(*) INTO vCount FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = pAction AND v.UIVirtualPageName = pVirtualPage;
    EXCEPTION  WHEN NO_DATA_FOUND THEN vCount := 0; 
  END;

  IF (vCount = 0) THEN
  
		DBMS_OUTPUT.PUT_LINE('Adding UISourcePage ' || pVirtualPage || ' to Action ' || pAction);
		
    BEGIN	
      SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' could not be found');
    END;
    
    BEGIN
      SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pVirtualPage;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('UIVirtualPage ' || pVirtualPage || ' could not be found');
    END;
    
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName ='UISourcePage';
    csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

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
  ELSE        
      DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' has been already linked to the UISourcePage ' || pVirtualPage);  
  END	IF;		
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignRoleIfExist
-- DESCR: Assigns a role to an employee who already has a selected role.
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACAssignRoleIfExist(ExRoleName IN VARCHAR2, pRoleId IN VARCHAR2, pRoleDescription IN VARCHAR2, pOrganizationName IN VARCHAR2, pPropagate IN NUMBER)
AS
    vCDODefId NUMBER:=7782;
    vEmployeeId VARCHAR2(16);
    vOrgId VARCHAR2(16);
    vIID VARCHAR2(16);
    vRoleGID VARCHAR2(36);
    vRoleDescription VARCHAR2(255);
    pEmployeeName VARCHAR2(36);
BEGIN
  DECLARE
	CURSOR csr
  IS
	SELECT Employee.EmployeeName FROM EmployeeRole 
	LEFT JOIN RoleDef ON EmployeeRole.RoleId = RoleDef.RoleId
	LEFT JOIN Employee ON EmployeeRole.EmployeeId = Employee.EmployeeId
	WHERE RoleDef.RoleName = ExRoleName;
	BEGIN
	Open csr;
	LOOP
	FETCH csr into pEmployeeName;
	EXIT WHEN csr%NOTFOUND;
	--------------------------------------------------
    SELECT EmployeeId
    INTO vEmployeeId
    FROM Employee
    WHERE EmployeeName=pEmployeeName;
    
    vOrgId:=NULL;
    IF (NOT pOrganizationName IS NULL) THEN
       SELECT OrganizationId
       INTO vOrgId
       FROM Organization
       WHERE OrganizationName=pOrganizationName;
    END IF;

    csiPRDGetNextInstanceId(vCDODefId,vIID);
    vRoleDescription := pRoleDescription || pEmployeeName;
    csiCreateGUID(vRoleDescription, vRoleGID);
    INSERT INTO EmployeeRole(ExportImportkey, EmployeeRoleId, CDOTypeId, RoleId, EmployeeId, IsFrozen, PropagateToChildOrgs, OrganizationId)
       VALUES (vRoleGID,vIID, vCDODefId, pRoleId, vEmployeeId, 0, pPropagate, vOrgId);

	END LOOP;
	CLOSE csr;
	END;
END;
/
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRBACDeleteUnavailablePermissions
-- DESCR: Removes permissions that do not match the specified type.
--
CREATE OR REPLACE PROCEDURE csiRBACDeleteUnavailablePermissions(
	vPermissionType			IN NUMBER,
	PermissionName		  IN VARCHAR2)
AS
	UnavialiblePermissions VARCHAR2(16);
BEGIN
    DECLARE
    CURSOR csr
    IS
		SELECT RolePermission.RolePermissionId FROM RolePermission
		WHERE RolePermissionName = PermissionName AND PermissionType <> vPermissionType;
    BEGIN
    Open csr;
    LOOP
    FETCH csr into UnavialiblePermissions;
		EXIT WHEN csr%NOTFOUND;
	--------------------------------------------------
			DELETE FROM RolePermission WHERE RolePermissionId = UnavialiblePermissions;
			DELETE FROM RolePermissionModes WHERE RolePermissionId = UnavialiblePermissions;
	--------------------------------------------------
    END LOOP;
    CLOSE csr;
    END;
	END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACAssignPermissionToRole
-- DESCR: Inserts the CM realted role permissions. If it exists then deletes and reloads
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiRBACAssignPermissionToRole(vRoleName IN VARCHAR2, pPermissionName IN VARCHAR2, pPermissionType IN NUMBER, pObjectMetaId IN NUMBER, pPermissionModesFlag IN NUMBER, pObjectInstanceId IN VARCHAR2 DEFAULT NULL)
--	pPermissionModesFlag Valid values:
--		* 0 => None				- Inserts all permission modes based on the security type
--		* 1 => Read-Only		- Inserts only 'Read' permission mode for 'Modeling' security type (110), 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
--		* 2 => No-Sec-Admin		- Inserts only 'C-R-U-D' permission modes for 'Security Administration' security type (180) and 'Modeling Advanced' security type (230)
AS
   vIID VARCHAR2(16);
	 vRoleGID VARCHAR2(36);
   vRoleId VARCHAR2(16);
BEGIN
    SELECT RoleId INTO vRoleId FROM RoleDef WHERE RoleName = vRoleName;
          csiPRDGetNextInstanceId(7783,vIID);
          INSERT INTO RolePermission(ExportImportKey,RolePermissionId,CDOTypeId,RoleId,ChangeCount,RolePermissionName,IsFrozen,ObjectMetaId,PermissionType,ObjectInstanceId)
            VALUES(vRoleGID,vIID,7783,vRoleId,1,pPermissionName,0,pObjectMetaId,pPermissionType,pObjectInstanceId);
        -- For each Mode defined in SecurityMaskDetail, insert a record into RolePermissionModes
        -- based on the pPermissionModesFlag value
      IF ( pPermissionModesFlag = 0 ) THEN
           INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
              SELECT vIID, 15263, BitNumber, ROWNUM
              FROM SecurityMaskDetail
              WHERE SecurityMaskId=pPermissionType
              ORDER BY BitNumber;
        ELSIF ( pPermissionModesFlag = 1 AND pPermissionType IN (110, 180, 230) ) THEN
        INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
              AND BitNumber = 2
          ORDER BY BitNumber;
        ELSIF ( pPermissionModesFlag = 2 AND pPermissionType IN (180, 230)) THEN
        INSERT INTO RolePermissionModes(RolePermissionId, FieldId, Modes, Sequence)
          SELECT vIID, 15263, BitNumber, ROWNUM
          FROM SecurityMaskDetail
          WHERE SecurityMaskId=pPermissionType
              AND BitNumber IN (1,2,3,4)
          ORDER BY BitNumber;
      ELSE
        DBMS_OUTPUT.PUT_LINE('Error - Invalid value passed for PermissionModeFlag parameter...');
      END IF;
END;
/

CREATE or Replace PROCEDURE CMPopulateDefaultData
AS
    
    vCMSpecId VARCHAR(16);   
    vCMSpecBaseId VARCHAR(16);    
    vCMWrkFlowCDODefId VARCHAR(16);    
    vCMWrkFlowBaseCDODefId VARCHAR(16);    
    vCMWrkFlowStepCDODefId VARCHAR(16);   
    vCMPathCDODefId VARCHAR(16);    
    vBusinessRuleId VARCHAR(16);  
    vSchedBusinessRuleId VARCHAR(16);  
	vApprovalDecisionId VARCHAR(16);  
    
    vRoleId VARCHAR2(16);
    vIID VARCHAR2(16);
    vSessionId VARCHAR2(16);
    vPermissionSQL VARCHAR2(4000);
    vCnt NUMBER;
    pInstanceId VARCHAR2(16);    
    vAdminPresent NUMBER;
    vInSiteAdminPresent NUMBER;
    vPermissionModesFlag_None NUMBER;
    vPermissionModesFlag_ReadOnly NUMBER;
    vPermissionModesFlag_NoSecAdmn NUMBER;
        
BEGIN
    
    
    vPermissionModesFlag_None := 0;
   	vPermissionModesFlag_ReadOnly := 1;
   	vPermissionModesFlag_NoSecAdmn := 2;

-- DraftPermissions Role
    csiRBACCreateRole ('DraftPermissions','Draft Permissions Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	-- DraftCamstarPermissions Role
    csiRBACCreateRole ('DraftCamstarPermissions','Draft Camstar Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
  -- DraftPLMPermissions Role
    csiRBACCreateRole ('DraftPLMPermissions','Draft PLM Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- DEPCPermissions Role
    csiRBACCreateRole ('DEPCPermissions','Deployment Complete Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- DEPIPermissions Role
    csiRBACCreateRole ('DEPIPermissions','Deployment Incomplete Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
	-- PackageCreator Role
	csiRBACCreateRole ('Package Creator','Package Creator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'Start Change Pkg', 120, 8500, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	
	-- PackageOwner Role
	csiRBACCreateRole ('Package Owner','Package Owner Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'UpdateChangePkg', 120, 8485, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'RouteApproval', 120, 8567, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval routing Sheet Maint', 110, 7820, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
  
	-- Package Deployer Role 
	csiRBACCreateRole ('Package Deployer','Package Deployer Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Track Target Deployment', 120, 8507, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
    csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
	
	-- Package Activator Role 
	csiRBACCreateRole ('Package Activator','Package Activator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'ActivateChangePkg', 120, 8528, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Activation Inquiry', 140, 8554, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Export/Import Controller', 160, 7392, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Import Status Inquiry', 140, 7397, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Modeling data Import', 150, 7391, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
             

	-- Package Approver Role
	csiRBACCreateRole ('Package Approver','Package Approver Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ChangeMgtWorkflow', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);

  --Package Collaborator Role
	csiRBACCreateRole ('Package Collaborator','Package Collaborator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'WhereUsedInquiry', 140, 8614, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'ContentChangeHistoryInquiry', 140, 8628, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDOInstanceInfoInquiry', 140, 8633, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'GetImpactDetailsInquiry', 140, 8634, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'AttachDocument', 120, 8573, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DocumentMaint', 110, 5620, vPermissionModesFlag_None);
		
        
  --PDPermissions Role 
	csiRBACCreateRole ('PDPermissions','Pending Deployment Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'DeployChangePkg', 120, 8526, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

	--RejectPermissions Role 
	csiRBACCreateRole ('RejectPermissions','Rejected Permissions Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
    
	--PACPermissions Role
	csiRBACCreateRole ('PACPermissions','Pending Approval Camstar Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'SignatureApproval', 120, 8568, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None); 
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
        
	--PAPLMPermissions Role
	csiRBACCreateRole ('PAPLMPermissions','Pending Approval PLM Permissions Role',vRoleId);
    csiRBACCreatePermission (vRoleId, 'PLMApprovePackage', 120, 8582, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CancelApproval', 120, 8566, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'MoveNonStdChangePkg', 120, 8551, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None); 
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Approval Cycle Inquiry', 140, 8003, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'MoveStdChangePkg', 120, 8545, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);
	
  --Package Collaborator Role
	csiRBACCreateRole ('Package Collaborator','Package Collaborator Role',vRoleId);
	csiRBACCreatePermission (vRoleId, 'Change Mgt Workflow Maint', 110, 8519, vPermissionModesFlag_ReadOnly);
	csiRBACCreatePermission (vRoleId, 'AssignChangePkgContent', 120, 8524, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'PackageInquiry', 140, 8547, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CDO Inquiry', 140, 7398, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'GetChangePackageDetails', 140, 8558, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Change Package Modeling Inquiry', 140, 8599, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'AssignSingleCPContent', 120, 8610, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DetachSingleCPContent', 120, 8611, vPermissionModesFlag_None);
 	csiRBACCreatePermission (vRoleId, 'DelegateTaskInquiry', 140, 8680, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'DelegateDateInquiry', 140, 8697, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatus', 120, 8709, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatus', 120, 8710, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'CloseCPStatuses', 120, 8725, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'VoidCPStatuses', 120, 8727, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatus', 120, 8735, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'OpenCPStatuses', 120, 8736, vPermissionModesFlag_None);

  
  csiRBACDeleteUnavailablePermissions (0, 'User Query Maint');
  csiRBACDeleteUnavailablePermissions (0, 'Business Rule Handler Maint');
  csiRBACDeleteUnavailablePermissions (0, 'Business Rule Maint');
  csiRBACDeleteUnavailablePermissions (0, 'Scheduled Business Rule Maint');
  csiRBACDeleteUnavailablePermissions (0, 'Summary Table Def Maint');

	--Modeling Advanced Role
	csiRBACCreateRole ('Default Modeling Advanced','Modeling Services for Advanced Users',vRoleId);
	csiRBACAssignRoleIfExist ('Default Modeling', vRoleId, 'Modeling Services for Advanced Users', NULL, 0);
	vPermissionSQL := '230';
  csiRBACCreatePermissionsForQry (vRoleId, vPermissionSQL, vPermissionModesFlag_None);
  
  csiRBACAssignPermissionToRole('Default Modeling', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
  csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'User Query Maint', 230, 7069, vPermissionModesFlag_ReadOnly);
  
  csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
  csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Handler Maint', 230, 7565, vPermissionModesFlag_ReadOnly);
  
  csiRBACAssignPermissionToRole('Default Modeling', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
  csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Business Rule Maint', 230, 7570, vPermissionModesFlag_ReadOnly);
  
  csiRBACAssignPermissionToRole('Default Modeling', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
  csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Scheduled Business Rule Maint', 230, 7588, vPermissionModesFlag_ReadOnly);
  
  csiRBACAssignPermissionToRole('Default Modeling', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
  csiRBACAssignPermissionToRole('Default Modeling Read-Only', 'Summary Table Def Maint', 230, 8238, vPermissionModesFlag_ReadOnly);
	

	-- Portal Configuration Role
    csiRBACCreateRole ('Portal Configuration','Portal Configuration Role',vRoleId);
    IF (vAdminPresent = 1) THEN
		csiRBACAssignRole (vRoleId, 'Portal Configuration Role','Administrator', NULL, 0);
    END IF;    
    csiRBACAssignRole (vRoleId, 'Portal Configuration Role','CamstarAdmin', NULL, 0);
    csiRBACCreatePermission (vRoleId, 'Configurator', 210, 1, vPermissionModesFlag_None);
    csiRBACCreatePermission (vRoleId, 'Portal Studio', 210, 2, vPermissionModesFlag_None);
	csiRBACCreatePermission (vRoleId, 'Portal Studio RBAC', 210, 3, vPermissionModesFlag_None);
	            

	-- Approval Decisions
    csiCMCreateApprovalDecision('Approved',10,vApprovalDecisionId);
	csiCMCreateApprovalDecision('Rejected',20,vApprovalDecisionId);

	csiCMCreateIDControl('ExportImportTarget',0);
	csiCMCreateIDControl('CPFileSequence',0);

   
    csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft',vCMSpecId,vCMSpecBaseId);

	csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft Camstar',vCMSpecId,vCMSpecBaseId);
    
    csiCMCreateSpec('Open.png','The Package automatically enters Draft step after creation. In Draft step the user edits core package attributes and assigns instance content. Outside of Draft step, package attributes and instance content cannot be altered. ','Draft PLM',vCMSpecId,vCMSpecBaseId);
        
    csiCMCreateSpec('Deployment_Complete.png','Deployment Complete means that all configured targets have been successfully deployed. After Deployment complete the Owner should close the package when no further deployments required.','Deployment Complete',vCMSpecId,vCMSpecBaseId);
    
    csiCMCreateSpec('Deployment_InComplete.png','Deployment Incomplete means that deployment has been attempted but at least one target failed.  The Owner may attempt to redeploy, or may choose to close the package.','Deployment Incomplete',vCMSpecId,vCMSpecBaseId);

	 csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval Camstar',vCMSpecId , vCMSpecBaseId);
   
    csiCMCreateSpec('Open.png','When all assigned approvals are made, the system updates the step to Pending Deployment.  If any approver rejects the Package, the step is set to Rejected.  If the owner executes Cancel Approval, the step is returned to Draft.','Pending Approval PLM',vCMSpecId , vCMSpecBaseId);
   
    csiCMCreateSpec('Rejected.png','In the Rejected step the package can be Voided or go back to Draft.','Rejected',vCMSpecId, vCMSpecBaseId);
   
   
    csiCMCreateSpec ('Open.png','In the Pending Deployment step the Package can be manually deployed to one or more remote targets, changing the status to Deployment Complete or Deployment Incomplete, if deployment fails. If deployment fails, user can redeploy or Close the Package','Pending Deployment',vCMSpecId,vCMSpecBaseId);
   
    csiCMCreateWorkFlow('No Approval','No Approval',vCMWrkFlowBaseCDODefId,vCMWrkFlowCDODefId,vCMWrkFlowStepCDODefId,vCMPathCDODefId );
	
     
	csiCMCreateWFCamstarPLM('Camstar','Camstar', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
    
    csiCMCreateWFCamstarPLM('PLM','PLM', vCMWrkFlowBaseCDODefId, vCMWrkFlowCDODefId, vCMWrkFlowStepCDODefId, vCMPathCDODefId);
   
    csiCMCreateBusinessRule('c3575681-5d0c-4c5d-817e-ab70049107b3','This script will process exports associated with a change packages.','BR_DEPLOY','BRH_DEPLOY','SBR_DEPLOY','ExecuteQueryEX("ChangePackage_GetExportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ExportImportName", 1,Transaction::__Const.DataType.String, CLF::ExportNames, 0);ForEach(CLF::ExportName,CLF::ExportNames){InitQueryParametersEx("ExportName",CLF::ExportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByExportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeployment,ExportName,CLF::ExportName);}}',0,5,6,NULL,vBusinessRuleId, vSchedBusinessRuleId);

    csiCMCreateBusinessRule('486d13ad-8e78-424e-a7a7-0bf7ae9375aa', 'This script will process exports associated with a change packages.','BR_DEPLOYSTATUS','BRH_DEPLOYSTATUS','SBR_DEPLOYSTATUS','ExecuteQueryEX("ChangePackage_GetDeploymentsInQueue", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"Name", 1,Transaction::__Const.DataType.String, CLF::PackageNames, 0);ForEach(CLF::ChangePackageName,CLF::PackageNames){CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessDeploymentInQueue);}}',0,5,6,NULL,vBusinessRuleId, vSchedBusinessRuleId);
    
    csiCMCreateBusinessRule('88729868-5ee0-4f77-9884-98e0349eb969','This script will process imports (activations) associated with a change packages.','BR_ACTIVATION','BRH_ACTIVATION','SBR_ACTIVATION', 'ExecuteQueryEX("ChangePackage_GetImportNameForChangePackage", ,0,-1,1,CLF::ResultSet);ConvertResultsetToListOrScalar(CLF::ResultSet,"ImportSetName", 1,Transaction::__Const.DataType.String, CLF::ImportNames, 0);ForEach(CLF::ImportName,CLF::ImportNames){InitQueryParametersEx("ImportName",CLF::ImportName, CLF::CPQueryParms);ExecuteQueryEx("ChangePackage_GetChangePackageByImportName", CLF::CPQueryParms ,0,-1,1,CLF::ChangePackageName);ConvertResultsetToListOrScalar(CLF::ChangePackageName,"Name", 0,Transaction::__Const.DataType.String, CLF::ChangePackageName, 0);CLF::ResolvedCP = GetNamedObject(CLF::ChangePackageName,"ChangePackage");if(CLF::ResolvedCP){Call(CLF::ResolvedCP, ProcessActivation,ImportSetName,CLF::ImportName);}}',0,5,6,NULL,vBusinessRuleId, vSchedBusinessRuleId);
	
	csiCMCreateBusinessRule('E78C5FFF-75AF-49d6-A964-A8772AE00437','This script will support email notifications for Change Package.','BR_NOTIFICATIONS','BRH_NOTIFICATIONS','SBR_NOTIFICATIONS', 'if(CLF::__CDOID.SessionValues and CLF::__CDOID.SessionValues.Factory){Call(CLF::__CDOID.SessionValues.Factory,SendReminderEmails);}',1,NULL,NULL,'4',vBusinessRuleId, vSchedBusinessRuleId);

	csiCMCreateBusinessRule('aafd3304-a163-460a-bb06-a6b15f5c63a1','This script will run the RPT Control Loop Limits Calculations.','BR_RPTControlLimits','BRH_RPTControlLimits_Monthly', 'SBR_RPTControlLimits', 'CLF::RPTControlLimitsUpdateObject = null;CreateCDO("RPTControlLimitsUpdate", false, false, CLF::RPTControlLimitsUpdateObject);CLF::RPTControlLimitsUpdateObject.RPTUpdateOccurrencePattern = 2;CLF::RPTControlLimitsUpdateObject.NoOfRPTCombinations = 100;CLF::RPTControlLimitsUpdateObject.HistoryTimePeriod = 30;CLF::RPTControlLimitsUpdateObject.Factory = CLF::__CDOID.SessionValues.Factory;Call(CLF::RPTControlLimitsUpdateObject, ProcessRPTControlLimits);',0,NULL,NULL,NULL,vBusinessRuleId, vSchedBusinessRuleId);


--- Change Management actions

	CreateActionRule('CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True');
	CreateActionRule('CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True or IsCollaborator = True');
	CreateActionRule('CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided');
	CreateActionRule('CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = True');
	CreateActionRule('CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed');
	CreateActionRule('CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers');
	CreateActionRule('CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers');
	CreateActionRule('CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed');
	CreateActionRule('CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');
	CreateActionRule('CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1');
	CreateActionRule('CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');

	CreateAction('CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305);

	CreateAction('CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL);
  
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMActivatePackageAction');
	AddActionRuleToActionDef('CMActivatePackageRule', 'CMActivatePackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction');
	
	
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMCollaboratorRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsAssignApprovalRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsApprovePLMRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMCancelApprovalRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMRouteApprovalRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMPackageDetailsAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMDeployPackageAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMContentHistoryAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMClosePackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMVoidPackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMActivationImpactAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMOpenPackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction');
	
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1');
		
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6');
	
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD');
	
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8');

    END;
/
    
	
BEGIN
CMPopulateDefaultData;
COMMIT;
END;
/

BEGIN
EXECUTE IMMEDIATE 'drop procedure csiCMCreateSpec';
EXECUTE IMMEDIATE 'drop procedure csiCMCreateWorkFlow';
EXECUTE IMMEDIATE 'drop procedure csiCMCreateBusinessRule';
EXECUTE IMMEDIATE 'drop procedure csiCMCreateApprovalDecision';
EXECUTE IMMEDIATE 'drop procedure CMPopulateDefaultData';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionRule';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionCategory';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionDef';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateUIAction';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateAction';
EXECUTE IMMEDIATE 'DROP PROCEDURE AddActionRuleToActionDef';
EXECUTE IMMEDIATE 'DROP PROCEDURE AddSourcePageToActionDef';
END;
/
