-------------------------------------------------------------------------------------------------------
-- SCRIPT:ReloadPortalMenuDefaultData.or.sql
-- DESCR: Reloads to the default Out Of Box Menu. 
--		    This will be run through a batch file csiReloadPortalmenu.bat. 
--        This batch file will get executed when user checks on Load Menu in the Management studio
-- Copyright Siemens 2023  
-------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        Instance Id number
--
CREATE OR REPLACE PROCEDURE getNextInstanceId(
	pCDODefId IN NUMBER, 
	pInstanceIdStr OUT VARCHAR2)
AS
   vCDODefIdStr VARCHAR2(16);
   vInstanceId NUMBER;
   vInstIdNewValue VARCHAR2(16);
BEGIN
	csiUpdateInstanceId(0,pCDODefId,1,vInstIdNewValue);
	vInstIdNewValue:=LPAD(SUBSTR(vInstIdNewValue,14,10),10,'0');
	
	vCDODefIdStr:=LPAD(LTRIM(TO_CHAR(pCDODefId,'xxxxxx')),6,'0');
	pInstanceIdStr:=LOWER(vCDODefIdStr||vInstIdNewValue);
END;
/
--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalmenuDefinition record
--
CREATE OR REPLACE PROCEDURE createPortalMenuDefinition(
	pPortalMenuDefinitionName IN VARCHAR2, 
	pDescription			  IN VARCHAR2, 
	pNotes					  IN VARCHAR2, 
    pInstanceId		          OUT VARCHAR2)
AS
	vPortalMenuDefCDODefId NUMBER := 7828;
  vCount  		      NUMBER;
BEGIN
    getNextInstanceId(vPortalMenuDefCDODefId, pInstanceId);
    Select count(*) into vcount from PortalMenuDefinition where PortalMenuDefinitionName = pPortalMenuDefinitionName;
     IF (vCount = 0) THEN
     
    INSERT INTO PortalMenuDefinition
           (PortalMenuDefinitionId
           ,CDOTypeId
           ,ChangeCount
           ,Notes
           ,ChangeHistoryId
           ,Description
           ,IconId
           ,IsFrozen
           ,PortalMenuDefinitionName)
     VALUES
           (pInstanceId
           ,vPortalMenuDefCDODefId		
           ,1							
           ,pNotes						
           ,NULL						
           ,pDescription				
           ,0							
           ,0							
           ,pPortalMenuDefinitionName);
            
    ELSE
           
           delete FROM PortalMenuDefinition WHERE PortalMenuDefinitionName = pPortalMenuDefinitionName;
            INSERT INTO PortalMenuDefinition
           (PortalMenuDefinitionId
           ,CDOTypeId
           ,ChangeCount
           ,Notes
           ,ChangeHistoryId
           ,Description
           ,IconId
           ,IsFrozen
           ,PortalMenuDefinitionName)
            VALUES
           (pInstanceId
           ,vPortalMenuDefCDODefId		
           ,1							
           ,pNotes						
           ,NULL						
           ,pDescription				
           ,0							
           ,0							
           ,pPortalMenuDefinitionName);
           END IF;
           
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE PROCEDURE createPortalMenuItem(
	pMenuGUID		IN VARCHAR2,
	pParentId		IN VARCHAR2,
	pCDOTypeId		IN NUMBER, 
	pSequence		IN NUMBER,
	pCaption		IN VARCHAR2,
	pLabelName		IN VARCHAR2,
	pPageName		IN VARCHAR2,
	pPageFlowName	IN VARCHAR2,
	pSubMenuName	IN VARCHAR2,
	pApolloIconName	IN VARCHAR2 DEFAULT NULL,
	pServiceName    IN VARCHAR2 DEFAULT NULL)
AS
	vMenuItemId VARCHAR2(16);
	vSubMenuId VARCHAR2(16);
	vVirtualPageId VARCHAR2(16);
	vPageFlowId VARCHAR2(16);
BEGIN
  begin
    if LENGTH(pSubMenuName)>0 THEN
      Select PortalMenuDefinitionId into vSubMenuId from PortalMenuDefinition where PortalMenuDefinitionName = pSubMenuName;
    END IF;
  exception  WHEN NO_DATA_FOUND THEN vSubMenuId := null; end;

  begin
    if LENGTH(pPageName)>0 THEN
      select UIVirtualPageId into vVirtualPageId from UIVirtualPage where UIVirtualPageName = pPageName;
    END IF;
  exception  WHEN NO_DATA_FOUND THEN vVirtualPageId := null; end;

  begin
    if LENGTH(pPageFlowName)>0 THEN
      select UIPageFlowId into vPageFlowId from UIPageFlow where UIPageFlowName = pPageFlowName;
    END IF;
  exception  WHEN NO_DATA_FOUND THEN vPageFlowId := null; end;
		

	getNextInstanceId(pCDOTypeId, vMenuItemId);
    INSERT INTO PortalMenuItem
           (PortalMenuItemId
           ,CDOTypeId
           ,ChangeCount
           ,ExportImportKey
           ,ParentId
           ,IsFrozen
           ,Caption
           ,LabelName
           ,Sequence
           ,MenuDefinitionId
           ,VirtualPageId
           ,PageFlowId
           ,PageURL
           ,PageDisplay
           ,QueryString
		   ,ApolloIcon
		   ,ServiceName)
     VALUES
           (vMenuItemId				
           ,pCDOTypeId				
           ,1			
           ,pMenuGUID
           ,pParentId				
           ,0						
           ,pCaption				
           ,pLabelName
           ,pSequence				
           ,vSubMenuId		
           ,vVirtualPageId			
           ,vPageFlowId				
           ,null				
           ,null			
           ,null
		   ,pApolloIconName
		   ,pServiceName);	
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Helper function to create PortalMenuItem record
--
CREATE OR REPLACE PROCEDURE populatePortalMenuDefaultData
AS
vMenuDefId varchar2(16);
vAssignedMenuDefId varchar2(16);
vHomePage varchar2(16);
vDefaultNotes varchar2(1000);
portalMenuDefId CHAR(16);
mobileMenuDefId CHAR(16);
portalV8MenuDefId CHAR(16);

BEGIN
	
	vDefaultNotes := 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.';

 createPortalMenuDefinition('csiContainer', 'Container transactions used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem('da9b33c4-706f-4929-9e06-036e45ef0deb',vMenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate','AssociateVP', '', '');
	createPortalMenuItem('34A9528F-1393-495f-BC43-B5260F51953E',vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate');
	createPortalMenuItem('b975d8ef-e809-4961-bc4d-4c25109a5c04',vMenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', '');
	createPortalMenuItem('1d20ffd0-b8fe-454c-897c-8ac5235c3eaa',vMenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR', 'ChangeQtyMultiReasonVP', '', '');
	createPortalMenuItem('2f85cb3a-ebf5-4c40-8cd8-9efdcac35fc7',vMenuDefId, 7835, 40, 'Close', 'LblMenuClose','CloseVP', '', '');
	createPortalMenuItem('ce9f42fe-a221-4a53-95e2-c1e501200033',vMenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti','MultiContainerCloseVP', '', '');
	createPortalMenuItem('f6e695e4-b5d7-4829-9aa5-9be8d3309065',vMenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData','DataCollectionVP', '', '');
	createPortalMenuItem('7738ba83-31ed-414b-8d3a-ddeaed3fd502',vMenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', '');
	createPortalMenuItem('6ecca050-5417-4c1c-b66f-0394d4227462',vMenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', '');
	createPortalMenuItem('9a321fa2-5b27-43d5-b9df-d42efd93fbdd',vMenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', '');
	createPortalMenuItem('4838b9d4-065b-4f66-ad3f-f13e6d00cec9',vMenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', '');
	createPortalMenuItem('a6cf06f2-1b1d-49a9-9075-6f0fcef99140',vMenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', '');
	createPortalMenuItem('d09012eb-c0c9-4d10-bf6c-0143d5e2f093',vMenuDefId, 7835, 100, 'Component Issue', 'LblMenuCompIssue','ComponentIssueVP', '', '');
	createPortalMenuItem('d988f667-9021-4c84-bf1e-46ce8f50a138',vMenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', '');
	createPortalMenuItem('5cd901ff-e4c0-4d01-866e-fb38ec8dfa38',vMenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', '');
	createPortalMenuItem('b5a2daf4-4f8e-4688-b634-3f317c1c99ab',vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '');
	createPortalMenuItem('b5a2daf4-4f8e-4688-b634-3f317c1c11qr',vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '');
	createPortalMenuItem('4006998c-1e19-4812-9244-62701fe8876c',vMenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '');
	createPortalMenuItem('ed8a737f-b261-493a-bf3a-e9ff1545c93c',vMenuDefId, 7835, 130, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', '');
	createPortalMenuItem('b5a2daf4-4f8e-4688-b634-3f317c1c99aa',vMenuDefId, 7835, 134, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', '');
	createPortalMenuItem('384c9d95-ee58-4938-8f08-f952e0bb8eb8',vMenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', '');
	createPortalMenuItem('ea3ccba1-bb13-4179-b1d8-33dfeda2dd78',vMenuDefId, 7835, 137, 'Current Sampling Status Update','LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', '');
	createPortalMenuItem('9e385ec2-54dd-472f-897b-f01056e55968',vMenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', '');
	createPortalMenuItem('75be54ef-9420-44a6-8323-ee6a5b6e289d',vMenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', '');
	createPortalMenuItem('8c420adf-2751-4719-8eda-594b439f9cc8',vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate');
	createPortalMenuItem('7bbe4fe8-01a8-4f53-9ac2-a33e402ce175',vMenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EProcedureVP', '', '');
	createPortalMenuItem('1f808640-b796-4f41-9747-c375f16cd51c',vMenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', '');
	createPortalMenuItem('55e7c800-66a2-40d4-9569-345605439d07',vMenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', '');
	createPortalMenuItem('44617b23-46d5-480e-a7c7-aecf1bc85cd2',vMenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', '');
	createPortalMenuItem('50ebc64b-09c9-400e-a06c-35ca90eb34c2',vMenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', '');
	createPortalMenuItem('6346c577-f62c-4807-8988-df1ecf6320e1',vMenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', '');
	createPortalMenuItem('422d2fc3-3176-4730-b464-2c98d6eaea87',vMenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', '');
	createPortalMenuItem('3e22b63f-ff87-4462-9043-11029d991c9e',vMenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', '');
	createPortalMenuItem('f4d04a19-8bba-4fd3-ae92-7089282f69ae',vMenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', '');
	createPortalMenuItem('1935a479-4b54-476f-aef3-b3734273a761',vMenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVP', '', '');
	createPortalMenuItem('59c79761-8d1a-422e-a266-826846145e5c',vMenuDefId, 7835, 250, 'Operational View - Scan', 'LblMenuOperViewScan','OperationalViewScanVP', '', '');
	createPortalMenuItem('a3ef82e1-3354-4199-9a47-fce313518c7c',vMenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', '');
	createPortalMenuItem('6fb5848c-951d-41e1-a846-8631f4752e16',vMenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', '');
	createPortalMenuItem('4bf3df0c-43ff-4548-90d3-3ded6e161dfd',vMenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', '');
	createPortalMenuItem('712e0383-0214-4528-a626-720a147c65c0',vMenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VP', '', '');
	createPortalMenuItem('926a9d1e-f017-4284-81e0-bd3c893dad07',vMenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', '');
	createPortalMenuItem('1e8fd079-e1a5-458d-9529-567b370e6dc1',vMenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', '');
	createPortalMenuItem('875cec25-5abf-402b-8ebd-3b0206c497da',vMenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', '');
	createPortalMenuItem('34e6f590-0500-40e2-b34a-d6d2f3fcd3f1',vMenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', '');
	createPortalMenuItem('3c1658da-d2b4-40fb-92d6-33285c699ee0',vMenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', '');
	createPortalMenuItem('9bb01c81-3409-44b3-b52d-63a8ab5173e8',vMenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', '');
	createPortalMenuItem('61fed6ec-f4ba-44bb-b6aa-f876b7b7797a',vMenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', '');
	createPortalMenuItem('b672a55c-7d76-4bfa-8270-15f5c1f5fded',vMenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', '');
	createPortalMenuItem('230c4d84-55bb-4f11-b9db-61bbe1a5795f',vMenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', '');
	createPortalMenuItem('fba89d41-924b-465b-bc45-e7ea9e780120',vMenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', '');
	createPortalMenuItem('6480B670-7196-4e31-95D6-513AD91F9337',vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
	createPortalMenuItem('6F6A3637-E475-4de8-9913-2D8C2E3E786D',vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');
	createPortalMenuItem('d5c83ef5-99fa-4005-b54a-d154fe070996',vMenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', '');
	createPortalMenuItem('8b540073-a012-4f3c-8366-3482f2842b3c',vMenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', '');	
	
	
    createPortalMenuDefinition('csiResource', 'Resource transactions available in the Portal', vDefaultNotes, vMenuDefId);
    createPortalMenuItem('792161b4-70b1-44ad-a482-f411245dfacf',vMenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', '');
    createPortalMenuItem('33c7a587-dfe0-4242-97fb-7692194891c3',vMenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', '');
    createPortalMenuItem('b43b198f-0826-465e-93dc-134476df82cd',vMenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', '');
	createPortalMenuItem('cda03299-ecd7-41f0-9b62-4658e21635b6',vMenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP', '', '');
    createPortalMenuItem('3f8864db-c143-4fef-9437-9b74ddad4759',vMenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', '');
	createPortalMenuItem('38928290-fc89-449f-a74f-2ba26351cf8d',vMenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', '');
	createPortalMenuItem('0526db1b-98dc-48c3-a57c-b4331e1d5f13',vMenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '');	
	
	
	createPortalMenuDefinition('csiEvent', 'Quality event transactions in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem('dc6950fc-be5b-4c4e-8a0d-265b1808b758',vMenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt', '', 'CreateGenericEvent_PF.1', '');

	--'Creating Search Menu...'
	createPortalMenuDefinition('csiSearch', 'Search options in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem('e97814d1-9deb-4478-985c-b85ee25c2691',vMenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP', '', '');
	createPortalMenuItem('f5d5aa48-dabb-4d31-9dd2-2de4516671f5',vMenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '');
	createPortalMenuItem('8162b166-dece-476c-9bb5-c6f21e0e4c78',vMenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter', 'MessageCenterVP', '', '');
	createPortalMenuItem('56c79c12-3137-4e82-9a53-c33acab9534e',vMenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail','MfgAuditTrailVP', '', '');
	createPortalMenuItem('d0d5ff96-55c4-40e7-85c1-89f52ad17561',vMenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '');
	createPortalMenuItem('5885791f-400f-43b0-9cae-b3cf01a30eab',vMenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '');
	
	--'Creating SPC Menu...'
	 createPortalMenuDefinition ('csiSPC', 'SPC pages used in the Portal', vDefaultNotes, vMenuDefId );
	 createPortalMenuItem ('692445b9-b186-4ca7-a8fc-ebf06baa6737',vMenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester','SPCTesterFormVP', '', '' );
	 
	 --'Creating Modeling Menu...'
	 createPortalMenuDefinition ('csiModelingMenu', 'Modeling pages used in the Portal', vDefaultNotes, vMenuDefId );
 	 createPortalMenuItem ('a701f05e-0721-422a-b664-12468edaa57b',vMenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', '' );
	 createPortalMenuItem ('29be1c6e-38ee-40d6-9f32-89bb2e5e79d2',vMenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail','ModelingAuditTrail_VP', '', '' );
	 createPortalMenuItem ('3fa8ee45-ff69-4a55-982b-f9223978a473',vMenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', '' );
	 createPortalMenuItem ('10419bfe-b75f-4d49-b1d4-9fb40cd9a5db',vMenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', '' );
	 
	 --'Creating Training Menu...'
	 createPortalMenuDefinition ('csiTrainingMenu', 'Training pages used in the Portal', vDefaultNotes, vMenuDefId );
	 createPortalMenuItem ('fee14386-7d0f-42c9-a657-c8c3c64b5c00',vMenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', '' );
	 createPortalMenuItem ('e83c6e8c-4b4e-45f1-aa02-5bdaf9c950ce',vMenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', '' );
	 
	 --'Creating Export/Import Menu...'
	 createPortalMenuDefinition ('csiExport/Import', 'Export/Import pages used in the Portal', vDefaultNotes, vMenuDefId );
	 createPortalMenuItem('d38844a6-552e-4e5b-bffd-019e55cd2612',vMenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', '');

	 --'Creating Change Management Menu...'
	 createPortalMenuDefinition('csiChangeManagement', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
	 createPortalMenuItem ('1b08241e-1a14-46bb-a928-788877dd46a4',vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '' );
	 createPortalMenuItem ('0340f41f-9e2c-4aac-a438-451c852125b4',vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
	 createPortalMenuItem ('bbdec625-bd4d-4908-a545-6b86c8e416b9',vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '' );
	 createPortalMenuItem ('ffa55915-3b91-4798-ae44-bbea44f3213f',vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', '' );
	 createPortalMenuItem ('ec29e444-8b90-4e9c-bac3-315a3ef1c581',vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', '' );
	 
	 --'Creating Attachments Menu...'
	 createPortalMenuDefinition ('csiAttachments', 'Attachments', vDefaultNotes, vMenuDefId );
	 createPortalMenuItem ('d1d8c4a5-bcff-45ec-8ffe-a293eb1137a1',vMenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' );
	 createPortalMenuItem ('55c20586-706f-4cad-b985-dd42ac3605db',vMenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', '' );
	

	 createPortalMenuDefinition('csiPortalMenu', 'The top level Portal menu', vDefaultNotes, vMenuDefId);
	 createPortalMenuItem('6ef8e978-7263-478f-be4d-393cfb645797',vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments','', '', 'csiAttachments', 'cmdAttach');
	 createPortalMenuItem('679fefbb-a5df-4b6a-bb9e-f5296343895f',vMenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagement', 'cmdChangeManagement');
	 createPortalMenuItem('da7550a9-b7d8-415e-868c-c64d4534ae4d',vMenuDefId, 7833, 30, 'Container', 'LblMenuCont', '', '', 'csiContainer', 'cmdLot');
	 createPortalMenuItem('02c8704f-6a36-454c-96b3-5ae11ebefd21',vMenuDefId, 7833, 40, 'Event', 'LblMenuEvent','', '', 'csiEvent', 'cmdNonConformanceQuality');
	 createPortalMenuItem('c7973073-db88-45a7-aff2-439661e922a5',vMenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/Import', 'cmdImportExport'); 
	 createPortalMenuItem('7a479937-410f-49d2-a062-51af870508d9',vMenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenu', 'cmdModelItem');
	 createPortalMenuItem('35ee5f34-a1d4-4eae-8e2f-581462dd7ce6',vMenuDefId, 7833, 70, 'Resource', 'LblMenuRes', '', '', 'csiResource', 'cmdMachine');
	 createPortalMenuItem('60367011-f6a6-432a-b041-00b11440f650',vMenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearch', 'cmdSearch');
	 createPortalMenuItem('229a27bb-d11d-47fc-ab9f-a5e9a1ceb638',vMenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPC', 'cmdGraph');
	 createPortalMenuItem('3ac566b5-8c1d-43e8-8a26-49ef393ae7b3',vMenuDefId, 7833, 95, 'Training','LblMenuTrg', '', '', 'csiTrainingMenu', 'cmdTraining');
 
 --Create V8 Menu with Apollo
	--Container Menu
	createPortalMenuDefinition ('csiContainerV8', 'Container transactions used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('55a0b6aa-8b5b-49d9-8d6d-ee8623fbd4bc',vMenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', ''); 
	createPortalMenuItem ('84A4B5FC-24DE-4380-8DE8-EE41790AA92B',vMenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate');
	createPortalMenuItem ('f440d6d7-a274-458a-a3ce-995dfc8cdecc',vMenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', ''); 
	createPortalMenuItem ('a9b41235-9f0f-480c-aba7-7a68d73083b5',vMenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', ''); 
	createPortalMenuItem ('c186f6ac-565a-47b7-b888-59cc05141644',vMenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', ''); 
	createPortalMenuItem ('231cfca7-9a45-491a-82d1-be44b8ff3c8a',vMenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', ''); 
	createPortalMenuItem ('3b5302e3-4d5d-4dce-8248-c73fef87cb01',vMenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', '');
	createPortalMenuItem ('572bf394-f371-41cb-a567-285d31dd9669',vMenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', ''); 
	createPortalMenuItem ('42649f60-5ea0-4e95-b05e-0aaf75ee2bf4',vMenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', ''); 
	createPortalMenuItem ('497a5174-e419-40e3-8279-8071c0f047d4',vMenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', ''); 
	createPortalMenuItem ('c4978511-88e4-45f1-9b60-532eda1bb0f3',vMenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', ''); 
	createPortalMenuItem ('68d7aa17-3d96-4080-93fe-9b3a4dd41cdd',vMenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', ''); 
	createPortalMenuItem ('38b2e099-8177-4aad-9a6e-5ca54113d15a',vMenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssue_VPR2', '', ''); 
	createPortalMenuItem ('27c284ea-d208-409d-8773-4e605633e527',vMenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', ''); 
	createPortalMenuItem ('c0a83130-2419-4701-b03c-388cb0ada026',vMenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', ''); 
	createPortalMenuItem ('fdf43c45-b722-460d-9b68-2203b9ec7197',vMenuDefId, 7835, 121, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', ''); 
	createPortalMenuItem ('b5a2daf4-4f8e-4688-b634-3f317c1c99ac',vMenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', '');
	createPortalMenuItem ('b5a2daf4-4f8e-4688-b634-3f317c1c11qr',vMenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '');
	createPortalMenuItem ('fa645aa7-c7d8-4176-9854-33b087cee863',vMenuDefId, 7835, 124, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', ''); 
	createPortalMenuItem ('b973940f-b162-4fe2-af15-fb84fd1a38be',vMenuDefId, 7835, 125, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', '');
	createPortalMenuItem ('4642d6d8-4f17-4e0d-ac5c-0ab74c69001c',vMenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', ''); 
	createPortalMenuItem ('0cad9a20-ef44-4c44-adc6-8387ea503f5a',vMenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', ''); 
	createPortalMenuItem ('9da6e49c-fe85-4ce3-82d6-a41afaab9081',vMenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', ''); 
	createPortalMenuItem ('94d4e5ee-4b80-411e-952d-9052871cb8ae',vMenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', ''); 
	createPortalMenuItem ('53a5dfbf-1a0c-4fc1-83a7-f9210355bec5',vMenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate');	
	createPortalMenuItem ('8d026f03-26d5-4364-9da5-43f96e0ccffc',vMenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EProcedureVPR2', '', '');
	createPortalMenuItem ('d992a558-e51f-4f08-b24c-be29da73f978',vMenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', ''); 
	createPortalMenuItem ('c7cdb3ca-b85a-4193-9dcc-e2956e862317',vMenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', ''); 
	createPortalMenuItem ('a4b47820-afc5-4903-9298-bd272bfc971f',vMenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', ''); 
	createPortalMenuItem ('ac26d32a-d827-41a5-9f89-3f76a5a8b613',vMenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', ''); 
	createPortalMenuItem ('7eeaecd2-f566-4c38-b8cd-36da03499c96',vMenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', ''); 
	createPortalMenuItem ('640f68c5-7ab9-4929-9b74-36f26de0e0b7',vMenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', ''); 
	createPortalMenuItem ('ed74b799-6b15-4cfe-b9c9-d4181657ef60',vMenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', ''); 
	createPortalMenuItem ('6f3c248a-2d4c-426e-976d-f2ba7e89ab65',vMenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', ''); 
	createPortalMenuItem ('6c84fe43-e4e0-4779-90fd-9f76afe97f35',vMenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVPR2', '', ''); 
	createPortalMenuItem ('4fa4c534-03b3-4ae9-aa0b-13e85bf20171',vMenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', ''); 
	createPortalMenuItem ('c8084d25-25dd-4ce8-a071-6986e5232372',vMenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', ''); 
	createPortalMenuItem ('622b76da-8860-4b2a-b909-fe05e153e478',vMenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', ''); 
	createPortalMenuItem ('36d6e3df-a19c-4d8d-8e62-00610c6eda1d',vMenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VPR2', '', ''); 
	createPortalMenuItem ('6cac0633-d2df-42c2-ad07-e85b00a2990f',vMenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', ''); 
	createPortalMenuItem ('5b7fe08a-6b42-465f-b5de-c6f2d18b1d36',vMenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', ''); 
	createPortalMenuItem ('1636b79c-c29e-4d42-b490-20a519f4ab9a',vMenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', ''); 
	createPortalMenuItem ('01477baf-7d36-49ea-b8fe-bcd92afff448',vMenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', ''); 
	createPortalMenuItem ('be687870-a160-41ea-a42b-a540575dedfc',vMenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', ''); 
	createPortalMenuItem ('72d52fa2-129c-4adc-b311-817ebeac5835',vMenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', ''); 
	createPortalMenuItem ('45c73a6d-e442-4d59-a9f5-4d31536b5b45',vMenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', ''); 
	createPortalMenuItem ('c315bae8-409d-4a4c-acfd-44e66f004fa2',vMenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', ''); 
	createPortalMenuItem ('c4fcb1f9-b119-47bf-86ea-d29cf97a5cf6',vMenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', ''); 
	createPortalMenuItem ('8188609b-d795-4d37-a860-69097e218487',vMenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', ''); 
	createPortalMenuItem ('B60123DD-0F34-4bb6-AB59-8E2FA391A6EA',vMenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart');
	createPortalMenuItem ('8A7B2E39-631C-4410-B04E-D661FB021AF8',vMenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple');
	createPortalMenuItem ('74ed56d8-c225-4448-b01a-34bf30bb954b',vMenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', ''); 
	createPortalMenuItem ('1ead6953-5719-47f5-abfa-58cae66ad026',vMenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', ''); 
	createPortalMenuItem ('02d26499-f1a2-4ede-b317-257beb01c8d5',vMenuDefId, 7835, 410, 'HV Component Issue', 'CSICDOName_HVComponentIssue','HVComponentIssueVP', '', '');	
	createPortalMenuItem ('149efb0f-330b-4966-b890-37186e230624',vMenuDefId, 7835, 420, 'Slitting', 'CSICDOName_Slitting','SlittingVP', '', '');		
	
	--Job Services menu
	createPortalMenuDefinition ('csiResourceTxn_JobSvc', 'Job Services', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('0ff85e59-45e3-4e84-b0d1-fa93633bf0e8',vMenuDefId, 7835, 10, 'Job Create', 'CSICDOName_JobCreate','JobCreateVP', '', '');
	createPortalMenuItem ('56d4f7f2-cdd7-4e7e-8382-a4e9df3e8594',vMenuDefId, 7835, 20, 'Job Assign', 'CSICDOName_JobAssign','JobAssign_VP', '', '');
	createPortalMenuItem ('a4a9b01a-3172-42e0-8932-b30ef646e69d',vMenuDefId, 7835, 30, 'Job Acknowledge', 'CSICDOName_JobAcknowledge','JobAcknowledge_VP', '', '');
	createPortalMenuItem ('060ef436-8984-4351-ab9b-8d810ce1ddd0',vMenuDefId, 7835, 40, 'Job Clock On', 'CSICDOName_JobClockOn','JobClockOn_VP', '', '');
	createPortalMenuItem ('060ef436-8984-4351-ab9b-8d810ce1ddd0',vMenuDefId, 7835, 50, 'Job Progress', 'CSICDOName_JobProgress','JobProgress_VP', '', '');
	createPortalMenuItem ('0a4a3756-3347-4491-a84a-007b5d3d8d2f',vMenuDefId, 7835, 60, 'Job Clock Off', 'CSICDOName_JobClockOff','JobClockOff_VP', '', '');
	createPortalMenuItem ('9747345c-4bb7-46ad-825d-bfc9efdb5eba',vMenuDefId, 7835, 70, 'Job Complete', 'CSICDOName_JobComplete','JobComplete_VP', '', '');
	createPortalMenuItem ('8173d081-9bcd-4f0e-8f12-9bafb85ea71f',vMenuDefId, 7835, 80, 'Job Cancel', 'CSICDOName_JobCancel','JobCancel_VP', '', '');
	
	--Jobs menu
	createPortalMenuDefinition ('csiResourceTxn_Job', 'Jobs', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('2fcbbba7-4cd4-44fe-9ebb-4b59a85c059f',vMenuDefId, 7835, 10, 'Supervisor Jobs', 'LblMenuSupervisorJobs','JobSupervisor_VP', '', '');
	createPortalMenuItem ('5dccb0f0-10d8-4eae-818b-9170425becc3',vMenuDefId, 7835, 20, 'Technician Jobs', 'LblMenuTechnicianJobs','JobTechnicians_VP', '', '');
	createPortalMenuItem ('11ba1a9b-eed9-4b69-9b8f-552c8b9af22c',vMenuDefId, 7833, 30, 'Job Services', 'PortalUI_JobServices', '','', 'csiResourceTxn_JobSvc');

	--Part Services menu
	createPortalMenuDefinition ('csiResourceTxn_PartSvc', 'Part Services', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('48b7ecf8-cd8b-4e82-b728-8780e841dc46',vMenuDefId, 7835, 10, 'Part Create', 'CSICDOName_PartCreate','PartCreate_VP', '', '');
	createPortalMenuItem ('fdec5a97-aa40-43bf-9447-4a3ac43f986e',vMenuDefId, 7835, 20, 'Part Setup', 'CSICDOName_PartSetup','PartSetupVP', '', '');
	createPortalMenuItem ('5610baf0-b012-421c-9616-6520b8937230',vMenuDefId, 7835, 30, 'Part Scrap', 'CSICDOName_PartScrap','PartScrap_VP', '', '');	
	createPortalMenuItem ('c2b8aeb3-54a6-44ff-be63-96cd0a3dcc04',vMenuDefId, 7835, 40, 'Part Request', 'CSICDOName_PartRequest','PartRequestVP', '', '');
	createPortalMenuItem ('c68e1bd9-1e74-4583-95e1-7012892ed9a3',vMenuDefId, 7835, 50, 'Part Request Acknowledge', 'CSICDOName_PartRequestAcknowledge','PartRequestAcknowledgeVP', '', '');
	createPortalMenuItem ('cfeb52a7-0550-4da2-9878-c507cdfcdbdb',vMenuDefId, 7835, 60, 'Part Request Assign', 'CSICDOName_PartRequestAssign','PartRequestAssign_VP', '', '');
	createPortalMenuItem ('cfcf7fe0-5a97-4d5d-a0b1-5c5d3c6e83b4',vMenuDefId, 7835, 70, 'Part Request Update', 'CSICDOName_PartRequestUpdate','PartRequestUpdateVP', '', '');
	createPortalMenuItem ('192e8ba2-1b46-45d1-b9fa-d3372fb42b39',vMenuDefId, 7835, 80, 'Part Request Issue', 'CSICDOName_PartRequestIssue','PartRequestIssue_VP', '', '');
	createPortalMenuItem ('c90a1000-f1ec-49ed-bf90-30054da09189',vMenuDefId, 7835, 90, 'Part Request Complete', 'CSICDOName_PartRequestComplete','PartRequestComplete_VP', '', '');
	createPortalMenuItem ('c11945de-03ee-4df9-b1ed-90b143c3a65a',vMenuDefId, 7835, 100, 'Part Request Cancel', 'CSICDOName_PartRequestCancel','PartRequestCancel_VP', '', '');
	createPortalMenuItem ('a0fd92e2-1f04-403a-916a-773327edd988',vMenuDefId, 7835, 110, 'Part Request Cancel Acknowledge', 'CSICDOName_PartRequestCancelAcknowledge','PartRequestCancelAcknowledgeVP', '', '');
		
	--Parts menu
	createPortalMenuDefinition ('csiResourceTxn_Part', 'Parts', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('b0e625f9-c13a-42c7-85c9-82e13a5a4c79',vMenuDefId, 7835, 10, 'Material Parts', 'PartRequestOrder_MaterialParts', 'MaterialPartMaintenanceVP', '', '');
	createPortalMenuItem ('11cb9e4f-60bd-4dcb-abf4-b88612cfa729',vMenuDefId, 7835, 20, 'Part Maintenance', 'LblPartMaintenance', 'PartMaintenanceVP', '', '');
	createPortalMenuItem ('457df0b1-4ea1-46d9-8263-e20312bb2a24',vMenuDefId, 7835, 30, 'Technician Requests', 'LblTechnicianRequests','PartRequestMain_VP', '', '', '', 'PartRequest');
	createPortalMenuItem ('eb52a625-ea91-4f2f-a859-2e07b98bdc6f',vMenuDefId, 7835, 40, 'Inventory Requests', 'LblInventoryRequests','PartRequestMain_VP', '', '', '', 'PartRequestAssign');
	createPortalMenuItem ('ee002858-401d-4b31-8621-a524fc364a40',vMenuDefId, 7833, 50, 'Part Services', 'PortalUI_PartServices', '','', 'csiResourceTxn_PartSvc');
	
	--Resource menu
	createPortalMenuDefinition ('csiResourceV8', 'Resource transactions available in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('d6200a6b-1666-47ae-9859-ca57906481c6',vMenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', '');
	createPortalMenuItem ('394d4941-c79d-47f4-8cc2-5d1a7d19904e',vMenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', '');
	createPortalMenuItem ('6907d956-424f-4010-9c2c-c14502317a2a',vMenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', '');
	createPortalMenuItem ('c6c19c25-d580-4c43-afa9-5df1faee2791',vMenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP_R2', '', '');
	createPortalMenuItem ('c3544fca-240e-4a35-b7b0-a163fc2b64b3',vMenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', '');
	createPortalMenuItem ('d754fd28-e8a2-47b0-a6e7-869fca382fa6',vMenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', '');
	createPortalMenuItem ('dcd75e8e-4f2e-497f-8923-d5e151ef364d',vMenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '');
	createPortalMenuItem ('64fbdadb-bf64-4b58-b122-3e33198c4b34',vMenuDefId, 7833, 80, 'Jobs', 'PortalUI_Jobs', '','', 'csiResourceTxn_Job', '');
	createPortalMenuItem ('24193c6e-aea6-40c6-9019-7f78d5b459e8',vMenuDefId, 7833, 90, 'Parts', 'Resource_Parts', '','', 'csiResourceTxn_Part', '');
	createPortalMenuItem ('cef6c008-c78d-4424-861e-289ae5e19508',vMenuDefId, 7835, 100, 'HV Resource Setup', 'CSICDOName_HVResourceSetup','HVResourceSetupVP', '', '');		

	--Creating Events Menu
	createPortalMenuDefinition ('csiEventV8', 'Quality event transactions in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('29d6e58c-1fdf-478f-858d-4a37f6c6d08f',vMenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', ''); 

	--Creating Search Menu
	createPortalMenuDefinition ('csiSearchV8', 'Search options in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('00fde9db-f437-4cd1-a489-2ab04eba6cac',vMenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP_R2', '', '');
	createPortalMenuItem ('ad8c718b-f42c-40d8-ad79-107bfe35b4f7',vMenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '');
	createPortalMenuItem ('23ae4e7b-8264-42f3-a5b9-7bc6e6af6c8c',vMenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '');
	createPortalMenuItem ('a4d03dbb-82d4-46a8-a2d9-4df5b7167bc5',vMenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP_R2', '', '');
	createPortalMenuItem ('45a90235-67d5-46a5-a078-fbc98daf87fa',vMenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', ''); 
	createPortalMenuItem ('242b00aa-a7e0-4b13-a2eb-0127ccbf0037',vMenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '');

	--Creating SPC Menu
	createPortalMenuDefinition ('csiSPCV8', 'SPC pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('e9ce8f2b-593e-4bd8-9dcf-2fc1b7b47207',vMenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', ''); 
	createPortalMenuItem ('7230227c-803f-429a-a403-d0801e9f7131',vMenuDefId, 7835, 20, 'SPC Realtime Monitoring', 'LblSPCRealtimeMonitoring','SPCRealtimeMonitoring_VP', '', '' );
	
	--Creating Modeling Menu
	createPortalMenuDefinition ('csiModelingMenuV8', 'Modeling pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('95c84560-dca1-4217-98be-853c6a1a65b8',vMenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', ''); 
	createPortalMenuItem ('35e02673-a6b8-4f70-8140-78815de31c5f',vMenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', '');
	createPortalMenuItem ('9ccae5c7-fc8c-47df-a15e-38939d36a83d',vMenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', '');
	createPortalMenuItem ('eb39dbab-e3d7-4acd-bbf9-c2480cb22d9f',vMenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', '' );
	
	--Creating Training Menu
	createPortalMenuDefinition ('csiTrainingMenuV8', 'Training pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('1ec4517a-7418-41b4-adc6-b440d2ccf934',vMenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', '');
	createPortalMenuItem ('5e2942b5-402b-48ef-beaf-316a88928202',vMenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', '');
	
	--Creating Export/Import Menu
	createPortalMenuDefinition ('csiExport/ImportV8', 'Export/Import pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('a1b686f0-031b-4bf5-add6-3ccc058737a9',vMenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', '');
	
	--Creating Change Management Menu
	createPortalMenuDefinition ('csiChangeManagementV8', 'Change Management pages used in the Portal', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('441b4d9f-fcaf-47e6-ac91-dc9316989d95',vMenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', '');
	createPortalMenuItem ('de1977c3-b43f-4ba6-8bce-9476fff22121',vMenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', '');
	createPortalMenuItem ('88d5dcd5-24c5-4507-8fbc-90dccb006af9',vMenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', '');
	createPortalMenuItem ('e0ae88de-3b17-4d6c-9acb-102f5cf2d6cb',vMenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', '');
	createPortalMenuItem ('cf0fc4e0-7f1d-4ef9-a3f9-24d9bfdf4106',vMenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', '');

	--Creating Attachments Menu
	createPortalMenuDefinition ('csiAttachmentsV8', 'Attachments', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('bc1cbd8c-9a5c-4339-80bb-e097316eca05',vMenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', ''); 
	createPortalMenuItem ('56b93028-39e9-4ffc-abec-d2b0090f22d2',vMenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', '');


	--Creating Portal V8 Main Menu
	createPortalMenuDefinition ('csiPortalMenuV8', 'The top level Portal menu', vDefaultNotes, vMenuDefId);
	createPortalMenuItem ('c1d5a088-8cdd-46ae-9fdd-7c04b629067b',vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach');
	createPortalMenuItem ('c8f8266f-25c5-44f9-8aea-5527e0d0ddb7',vMenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement');
	createPortalMenuItem ('dd35afd2-96f4-46f4-a1b6-0ac553b701d3',vMenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot'); 
	createPortalMenuItem ('54ac8bb0-6c05-4ecd-9392-4050df554cee',vMenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality');
	createPortalMenuItem ('34fe7232-e872-4eb2-9a2e-a212bab0bfb2',vMenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport');
	createPortalMenuItem ('3bdc329a-1122-4c0d-b683-5ca2ea894f5d',vMenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem'); 
	createPortalMenuItem ('f0b71eee-e488-4e44-8dcb-eeac1c7f6719',vMenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine'); 
	createPortalMenuItem ('a011f6d3-6b3d-452b-954b-7de8248791b5',vMenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch'); 
	createPortalMenuItem ('6b2ecf0c-e188-41bf-8cfc-bde8acc60a99',vMenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph');
	createPortalMenuItem ('b8eb02d6-0949-4bfc-bf08-d90aaac48d4c',vMenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining');

		 --'Creating Container Mobile Sub Menu'
	 createPortalMenuDefinition ('csiContainerMobileMenu', 'Mobile Menu for Container Txns', vDefaultNotes, vMenuDefId);
	 createPortalMenuItem ('08e9b5b2-a2fd-4af2-8078-1d9c2b9e9767',vMenuDefId, 7835, 10, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP_R2', '', '');
	 createPortalMenuItem ('b402a88b-89ff-4001-919f-c7867e0de2bb',vMenuDefId, 7835, 20, 'Hold', 'LblMenuHold','ContainerHoldVP_R2', '', '');
	 createPortalMenuItem ('7a098af8-0b62-4a3a-a5fa-77bd76432b4e',vMenuDefId, 7835, 30, 'Move In','LblMenuMoveIn', 'MoveInVP_R2', '', '' );
	 createPortalMenuItem ('6f006bf0-9da5-43bf-ab81-99eaea81d7e4',vMenuDefId, 7835, 40, 'Move', 'LblMenuMove', 'MoveStdVP_R2', '', '');
	 createPortalMenuItem ('e1a721a9-839a-4777-93a0-7a34d72f5563',vMenuDefId, 7835, 50, 'Rework', 'LblMenurework','ReworkVP_R2', '', '');
	
	 --'Creating Mobile Menu'
 	 createPortalMenuDefinition('csiMobileMenu', 'The top level Mobile Portal menu', vDefaultNotes, vMenuDefId);
	 createPortalMenuItem('d6fb38eb-c15e-4518-9a68-d482391e4a19', vMenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach');
	 createPortalMenuItem('6faf9186-4a1b-4636-af34-a2749e0c248b', vMenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement');
	 createPortalMenuItem('dd3958f2-d62e-42ec-aea8-39254fd491ee', vMenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot' );
	 createPortalMenuItem('ca3f3dd0-f757-486d-b1c0-5ebca6af65c3', vMenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality');
	 createPortalMenuItem('f145426b-9253-490d-a9c3-f0370c35ffb6', vMenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport'); 
	 createPortalMenuItem('1b4bca0e-b1ae-48dd-b5e9-e05cf4aeb339', vMenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem');
	 createPortalMenuItem('eb8e51e0-d2bb-4ed5-9197-58be49b22857', vMenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine');
	 createPortalMenuItem('99485ba4-0815-464e-a959-821f94d7d0dc', vMenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch');
	 createPortalMenuItem('e6ae0b73-528f-407f-94fb-6f1a30613c4c', vMenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph');
	 createPortalMenuItem('41709d3c-062d-408c-9a45-9d749dbc762c', vMenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining');

 	SELECT PortalMenuDefinitionId INTO portalMenuDefId
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiPortalMenu';

	SELECT PortalMenuDefinitionId INTO mobileMenuDefId
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiMobileMenu';

	SELECT PortalMenuDefinitionId INTO portalV8MenuDefId
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiPortalMenuV8';
  
	UPDATE EMPLOYEE SET  PortalMenuDefinitionId = portalMenuDefId,
	PortalMobileMenuDefinitionId = mobileMenuDefId,
	PortalV8MenuDefinitionId = portalV8MenuDefId
	WHERE EmployeeName = 'CamstarAdmin';
	--This gets set in the Loader. Not needed here for all 3 users.
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'CamstarAdmin');
	
	UPDATE EMPLOYEE SET  PortalMenuDefinitionId = portalMenuDefId,
	PortalMobileMenuDefinitionId = mobileMenuDefId,
	PortalV8MenuDefinitionId = portalV8MenuDefId
	WHERE EmployeeName = 'InSiteAdmin';
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'InSiteAdmin');
	
	UPDATE EMPLOYEE SET  PortalMenuDefinitionId = portalMenuDefId,
	PortalMobileMenuDefinitionId = mobileMenuDefId,
	PortalV8MenuDefinitionId = portalV8MenuDefId
	WHERE EmployeeName = 'Administrator';
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'Administrator');
	
  	
END;
/

BEGIN
populatePortalMenuDefaultData;
COMMIT;
END;
/

BEGIN
EXECUTE IMMEDIATE 'DROP PROCEDURE getNextInstanceId';
EXECUTE IMMEDIATE 'DROP PROCEDURE createPortalMenuItem';
EXECUTE IMMEDIATE 'DROP PROCEDURE createPortalMenuDefinition';
EXECUTE IMMEDIATE 'DROP PROCEDURE populatePortalMenuDefaultData';
END;
/

