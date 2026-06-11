-- Copyright Siemens 2025  
CREATE or Replace PROCEDURE csiCMCreateSpec(pStepIcon IN VARCHAR2, pRoleDescription IN VARCHAR2,pCMSpecName IN VARCHAR2, pInstanceId OUT VARCHAR2, pInstanceId1 OUT VARCHAR2)
AS
   vRoleId CHAR(16);
   reccount Int;
BEGIN
  
    csiPRDGetNextInstanceId(8513,pInstanceId);
    csiPRDGetNextInstanceId(8514,pInstanceId1);
  
  
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
              
INSERT INTO BusinessProcessSpecBase
           (BusinessProcessSpecBaseId,BusinessProcessSpecName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
VALUES (pInstanceId1,pCMSpecName,8514,1,NULL,pInstanceId);

  
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
           
END;
/
CREATE  or REPLACE PROCEDURE  csiCMCreateWorkFlow(pWFDescription IN VARCHAR2,pWFName IN VARCHAR2, pInstanceId2 OUT VARCHAR2, pInstanceId3 OUT VARCHAR2, pInstanceId4 OUT VARCHAR2, pInstanceId5 OUT VARCHAR2)
AS
   vCMDRAFT CHAR(16);
   vCMDI CHAR(16);
   vCMDC CHAR(16);
   vCMCL CHAR(16);
   vSpecId CHAR(16);
   vCMPD CHAR(16);
   vSpecDescription VARCHAR2(255);
   vCMPathSelectorInstanceId CHAR(16);
   
BEGIN
      
   csiPRDGetNextInstanceId(8518,pInstanceId2);
   csiPRDGetNextInstanceId(8517,pInstanceId3);
   csiPRDGetNextInstanceId(8578,pInstanceId4);
           
   INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
   INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
     
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

    
         
END;
/

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
   vPAName VARCHAR2(255);
    vSpecDescription VARCHAR2(255);
BEGIN
      
   csiPRDGetNextInstanceId(8518,pInstanceId2);
   csiPRDGetNextInstanceId(8517,pInstanceId3);
   csiPRDGetNextInstanceId(8578,pInstanceId4);
           
   INSERT INTO BusinessProcessWorkflow
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowId,CDOTypeId,ChangeCount,ChangeHistoryId
           ,Description,ECO,FirstStepId,IconId,IsFrozen,Notes,Revision,Status,WIPMsgDefMgrId)
     VALUES (pInstanceId2,pInstanceId3,8517,1,NULL,null,NULL,null, NULL,0,NULL,1,1,NULL);
           
           
   INSERT INTO BusinessProcessWorkflowBase
           (BusinessProcessWorkflowBaseId,BusinessProcessWorkflowName,CDOTypeId,ChangeCount,IconId,RevOfRcdId)
     VALUES(pInstanceId2,pWFName,8518,1,NULL,pInstanceId3);
     
     
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
     
	  if pWFDescription = 'Camstar' then
		vPAName := 'Pending Approval Camstar';  
     elsif pWFDescription = 'PLM' then
		vPAName := 'Pending Approval PLM';  
        End If;

    csiPRDGetNextInstanceId(8578,pInstanceId4);
     vCMPA := pInstanceId4;
     
    select BusinessProcessSpecId 
      into vSpecId    
      from BusinessProcessSpecBase,
      	   BusinessProcessSpec
	 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
	   and BusinessProcessSpecBase.BusinessProcessSpecName = vPAName
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
      
         
END;
/




CREATE  or REPLACE PROCEDURE  csiCMCreateBusinessRule(pExportImportKey IN VARCHAR2,pBRDescription IN VARCHAR2, pBRName IN VARCHAR2, pBRHName IN VARCHAR2, pSBRName IN VARCHAR2, pBRSCRIPT IN VARCHAR2, pSBRIsAdvancedMode IN NUMBER, pSBRRecurrenceFrequency IN NUMBER,pSBRRecurrencePattern IN NUMBER, pSBRScheduleHours IN VARCHAR2, pInstanceId1 OUT VARCHAR2, pInstanceId2 OUT VARCHAR2)
AS
   vBRDataInstanceId CHAR(16);
   vBRHandlerInstanceId CHAR(16);
   vBRHandlerDataInstanceId CHAR(16);
   vDueDate TIMESTAMP;
   vDueDateGMT TIMESTAMP;
   
BEGIN
      
   csiPRDGetNextInstanceId(7569,pInstanceId1);
   csiPRDGetNextInstanceId(7573,vBRDataInstanceId);
   csiPRDGetNextInstanceId(7564,vBRHandlerInstanceId);
   csiPRDGetNextInstanceId(7567,vBRHandlerDataInstanceId);
   csiPRDGetNextInstanceId(7587,pInstanceId2);
           
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
               
         
END;
/

CREATE or Replace PROCEDURE csiCMCreateApprovalDecision(pDecisionName IN VARCHAR2, pDecisionType IN INT, pInstanceId OUT VARCHAR2)
AS
   reccount Int;
BEGIN
  
    csiPRDGetNextInstanceId(7857,pInstanceId);


	INSERT INTO ApprovalDecision
           (ApprovalDecisionId,ApprovalDecisionListId,ApprovalDecisionName,CDOTypeId,ChangeCount,DecisionType,IncludeComments,IsFrozen)
	VALUES (pInstanceId,'001e850000000000',pDecisionName,7857,1,pDecisionType, 0,0);

  
	DBMS_OUTPUT.put_line('ApprovalDecision '|| pDecisionName);
         
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
            
BEGIN

	csiCMCreateApprovalDecision('Approved',10,vApprovalDecisionId);

	csiCMCreateApprovalDecision('Rejected',20,vApprovalDecisionId);

    
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
END;
/


BEGIN
CMPopulateDefaultData;
COMMIT;
END;
/

BEGIN
Execute immediate 'drop procedure csiCMCreateSpec';
Execute immediate 'drop procedure csiCMCreateWorkFlow';
Execute immediate 'drop procedure csiCMCreateBusinessRule';
Execute immediate 'drop procedure csiCMCreateApprovalDecision';
Execute immediate 'drop procedure CMPopulateDefaultData';
END;
/
