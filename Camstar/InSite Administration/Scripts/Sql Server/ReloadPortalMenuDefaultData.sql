--------------------------------------------------------------------------------
-- SCRIPT:ReloadPortalMenuDefaultData.sql
-- DESCR: Reloads to the default Out Of Box Menu. 
--		  This will be run through a batch file csiReloadPortalmenu.bat. This batch file will get executed when user checks on Load Menu in the Management studio
--Copyright Siemens 2023  


--------------------------------------------------------------------------------
-- PROCEDURE: getNextInstanceId
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        return InstanceId number
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'getNextInstanceId' 
	   AND 	  type = 'P')
    DROP PROCEDURE getNextInstanceId
GO
CREATE PROCEDURE getNextInstanceId(@CDODefId INT, @InstanceIdStr VARCHAR(16) OUTPUT)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CDODefIdStr VARCHAR(16)
    DECLARE @InstanceId INT
    DECLARE @InstIdNewValue VARCHAR(16)

    -- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
    -- Length should be 10 chars
    EXEC csiUpdateInstanceID 0,@CDODefId,1,@InstIdNewValue OUTPUT
	SET @InstIdNewValue=SUBSTRING(@InstIdNewValue,14,10)
	SET @InstIdNewValue=REPLICATE('0', (10 - LEN(@InstIdNewValue))) + @InstIdNewValue

    -- Convert CDODef Id to hex (pad to 6 chars)
	SET @CDODefIdStr=REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@CDODefId), 1, 2, ''), '0', ' ')), ' ', '0')
	SET @CDODefIdStr=REPLICATE('0', (6 - LEN(@CDODefIdStr))) + @CDODefIdStr

    -- Join CDODef hex and instance id hex strings. Length=16 chars
	SET @InstanceIdStr=LOWER(@CDODefIdStr+@InstIdNewValue)
END
GO
--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuDefinition
-- DESCR: Helper function to create PortalMenuDefinition record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createPortalMenuDefinition' 
	   AND 	  type = 'P')
    DROP PROCEDURE createPortalMenuDefinition
GO
CREATE PROCEDURE createPortalMenuDefinition(
	@PortalMenuDefinitionName NVARCHAR(30), 
	@Description NVARCHAR(255), 
	@Notes NVARCHAR(2000), 
    @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PortalMenuDefCDODefId INT
    SET @PortalMenuDefCDODefId=7828

    EXEC getNextInstanceId @PortalMenuDefCDODefId,@InstanceId OUTPUT
    IF NOT EXISTS (SELECT * FROM PortalMenuDefinition WHERE PortalMenuDefinitionName = @PortalMenuDefinitionName)
           Begin
					INSERT INTO PortalMenuDefinition
								([PortalMenuDefinitionId],[CDOTypeId],[ChangeCount],[Notes],[ChangeHistoryId],[Description],[IconId],[IsFrozen],[PortalMenuDefinitionName])
					 VALUES
								 (@InstanceId					--, char(16),>
								 ,@PortalMenuDefCDODefId		--, int,>
								 ,1							--, int,>
								 ,@Notes						--, nvarchar(2000),>
								 ,NULL						--, char(16),>
								 ,@Description				--, nvarchar(255),>
								 ,0							--, int,>
								 ,0							--, bit,>
								 ,@PortalMenuDefinitionName)	--, nvarchar(30),>)
           End
	else 
           Begin
					delete FROM PortalMenuDefinition WHERE PortalMenuDefinitionName = @PortalMenuDefinitionName
					INSERT INTO PortalMenuDefinition
								([PortalMenuDefinitionId],[CDOTypeId],[ChangeCount],[Notes],[ChangeHistoryId],[Description],[IconId],[IsFrozen],[PortalMenuDefinitionName])
					 VALUES
								 (@InstanceId					--, char(16),>
								 ,@PortalMenuDefCDODefId		--, int,>
								 ,1							--, int,>
								 ,@Notes						--, nvarchar(2000),>
								 ,NULL						--, char(16),>
								 ,@Description				--, nvarchar(255),>
								 ,0							--, int,>
								 ,0							--, bit,>
								 ,@PortalMenuDefinitionName)	--, nvarchar(30),>)
			end
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: createPortalMenuItem
-- DESCR: Helper function to create PortalMenuItem record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createPortalMenuItem' 
	   AND 	  type = 'P')
    DROP PROCEDURE createPortalMenuItem
GO
CREATE PROCEDURE createPortalMenuItem(
	@MenuGUID			nvarchar(36),
	@ParentId			char(16),
	@CDOTypeId			int, 
	@Sequence			int,
	@Caption			nvarchar(50),
	@LabelName			nvarchar(66),
	@VirtualPageName	varchar(30),
	@PageFlowName       varchar(30),
	@SubMenuName		varchar(30),
	@ApolloIconName		varchar(128) = NULL,
	@ServiceName		varchar(128) = NULL
)
AS
DECLARE @MenuItemId		varchar(16)
DECLARE @SubMenuId		varchar(16)
DECLARE @VirtualPageId	varchar(16)
DECLARE @PageFlowId		varchar(16)
BEGIN
    SET NOCOUNT ON;

	IF LEN(@SubMenuName)>0 
		Select @SubMenuId = PortalMenuDefinitionId from PortalMenuDefinition where PortalMenuDefinitionName = @SubMenuName
	if LEN(@VirtualPageName)>0 
		select @VirtualPageId = UIVirtualPageId from UIVirtualPage where UIVirtualPageName = @VirtualPageName
	if LEN(@PageFlowName)>0 
		select @PageFlowId = UIPageFlowId from UIPageFlow where UIPageFlowName = @PageFlowName
		
    EXEC getNextInstanceId @CDOTypeId,@MenuItemId OUTPUT
	
    INSERT INTO PortalMenuItem
           ([PortalMenuItemId]
           ,[CDOTypeId]
           ,[ChangeCount]
           ,[ExportImportKey]
           ,[ParentId]
           ,[IsFrozen]
           ,[Caption]
		   ,[LabelName]
           ,[Sequence]
           ,[MenuDefinitionId]
           ,[VirtualPageId]
           ,[PageFlowId]
           ,[PageURL]
           ,[PageDisplay]
           ,[QueryString]
		   ,[ApolloIcon]
		   ,[ServiceName])
     VALUES
           (@MenuItemId				--, char(16),>
           ,@CDOTypeId				--, int,>
           ,1						--, int,>
           ,@MenuGUID				--, nvarchar(36),>
           ,@ParentId				--, char(16),>
           ,0						--, bit,>
           ,@Caption				--, nvarchar(50),>
		   ,@LabelName
           ,@Sequence				--, int,>
           ,@SubMenuId				--, char(16),>
           ,@VirtualPageId			--, char(16),>
           ,@PageFlowId				--, char(16),>
           ,null					--, nvarchar(512),>
           ,null					--, int,>
           ,null					--, nvarchar(512),>
		   ,@ApolloIconName			--, nvarchar(30),>
		   ,@ServiceName)           --, nvarchar(32),>
		   
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: populatePortalMenuDefaultData
-- DESCR: Create Portal Menus 
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'populatePortalMenuDefaultData' 
	   AND 	  type = 'P')
    DROP PROCEDURE populatePortalMenuDefaultData
GO
CREATE PROCEDURE populatePortalMenuDefaultData
AS
DECLARE @MenuDefId varchar(16)
DECLARE @AssignedMenuDefId varchar(16)
DECLARE @HomePage varchar(16)
DECLARE @DefaultNotes varchar(100)
DECLARE @csiPortalMenuPortalMenuDefinitionId varchar(16)
DECLARE @csiMobileMenuPortalMenuDefinitionId varchar(16)
DECLARE @csiPortalMenuV8PortalMenuDefinitionId varchar(16)
BEGIN
    SET NOCOUNT ON;
	
	BEGIN TRAN
	
	SET @DefaultNotes = 'This menu is created by the install process.  Best practice is to copy this menu and modify the copy, instead of modifying this menu directly.'
	
	
	
		PRINT('Creating Container Transaction Menu Definition...')
	EXEC createPortalMenuDefinition 'csiContainer', 'Container transactions used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'da9b33c4-706f-4929-9e06-036e45ef0deb',@MenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', '' 
	EXEC createPortalMenuItem '34a9528f-1393-495f-bc43-b5260d51953e',@MenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate' 
	EXEC createPortalMenuItem 'b975d8ef-e809-4961-bc4d-4c25109a5c04',@MenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', '' 
	EXEC createPortalMenuItem '1d20ffd0-b8fe-454c-897c-8ac5235c3eaa',@MenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', '' 
	EXEC createPortalMenuItem '2f85cb3a-ebf5-4c40-8cd8-9efdcac35fc7',@MenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', '' 
	EXEC createPortalMenuItem 'ce9f42fe-a221-4a53-95e2-c1e501200033',@MenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', '' 
	EXEC createPortalMenuItem 'f6e695e4-b5d7-4829-9aa5-9be8d3309065',@MenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', ''
	EXEC createPortalMenuItem '7738ba83-31ed-414b-8d3a-ddeaed3fd502',@MenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', '' 
	EXEC createPortalMenuItem '6ecca050-5417-4c1c-b66f-0394d4227462',@MenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', '' 
	EXEC createPortalMenuItem '9a321fa2-5b27-43d5-b9df-d42efd93fbdd',@MenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', '' 
	EXEC createPortalMenuItem '4838b9d4-065b-4f66-ad3f-f13e6d00cec9',@MenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', '' 
	EXEC createPortalMenuItem 'a6cf06f2-1b1d-49a9-9075-6f0fcef99140',@MenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', '' 
	EXEC createPortalMenuItem 'd09012eb-c0c9-4d10-bf6c-0143d5e2f093',@MenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssueVP', '', '' 
	EXEC createPortalMenuItem 'd988f667-9021-4c84-bf1e-46ce8f50a138',@MenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', '' 
	EXEC createPortalMenuItem '5cd901ff-e4c0-4d01-866e-fb38ec8dfa38',@MenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', '' 
	EXEC createPortalMenuItem 'b5a2daf4-4f8e-4688-b634-3f317c1c99ab',@MenuDefId, 7835, 122, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', ''  
	EXEC createPortalMenuItem 'b5a2daf4-4f8e-4688-b634-3f317c1c11qr',@MenuDefId, 7835, 123, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '' 
	EXEC createPortalMenuItem '4006998c-1e19-4812-9244-62701fe8876c',@MenuDefId, 7835, 125, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' 
	EXEC createPortalMenuItem 'ed8a737f-b261-493a-bf3a-e9ff1545c93c',@MenuDefId, 7835, 130, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', '' 
	EXEC createPortalMenuItem 'b5a2daf4-4f8e-4688-b634-3f317c1c99aa',@MenuDefId, 7835, 134, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', ''
	EXEC createPortalMenuItem '384c9d95-ee58-4938-8f08-f952e0bb8eb8',@MenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', '' 
	EXEC createPortalMenuItem 'ea3ccba1-bb13-4179-b1d8-33dfeda2dd78',@MenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', '' 
	EXEC createPortalMenuItem '9e385ec2-54dd-472f-897b-f01056e55968',@MenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', '' 
	EXEC createPortalMenuItem '75be54ef-9420-44a6-8323-ee6a5b6e289d',@MenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', '' 
	EXEC createPortalMenuItem '8c420adf-2751-4719-8eda-594b439f9cc8',@MenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate'	
	EXEC createPortalMenuItem '7bbe4fe8-01a8-4f53-9ac2-a33e402ce175',@MenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EProcedureVP', '', '' 
	EXEC createPortalMenuItem '1f808640-b796-4f41-9747-c375f16cd51c',@MenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', '' 
	EXEC createPortalMenuItem '55e7c800-66a2-40d4-9569-345605439d07',@MenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', '' 
	EXEC createPortalMenuItem '44617b23-46d5-480e-a7c7-aecf1bc85cd2',@MenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', '' 
	EXEC createPortalMenuItem '50ebc64b-09c9-400e-a06c-35ca90eb34c2',@MenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', '' 
	EXEC createPortalMenuItem '6346c577-f62c-4807-8988-df1ecf6320e1',@MenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', '' 
	EXEC createPortalMenuItem '422d2fc3-3176-4730-b464-2c98d6eaea87',@MenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', '' 
	EXEC createPortalMenuItem '3e22b63f-ff87-4462-9043-11029d991c9e',@MenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', '' 
	EXEC createPortalMenuItem 'f4d04a19-8bba-4fd3-ae92-7089282f69ae',@MenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', '' 
	EXEC createPortalMenuItem '1935a479-4b54-476f-aef3-b3734273a761',@MenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVP', '', '' 
	EXEC createPortalMenuItem '59c79761-8d1a-422e-a266-826846145e5c',@MenuDefId, 7835, 250, 'Operational View - Scan', 'LblMenuOperViewScan','OperationalViewScanVP', '', '' 
	EXEC createPortalMenuItem 'a3ef82e1-3354-4199-9a47-fce313518c7c',@MenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', '' 
	EXEC createPortalMenuItem '6fb5848c-951d-41e1-a846-8631f4752e16',@MenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', '' 
	EXEC createPortalMenuItem '4bf3df0c-43ff-4548-90d3-3ded6e161dfd',@MenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', '' 
	EXEC createPortalMenuItem '712e0383-0214-4528-a626-720a147c65c0',@MenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VP', '', '' 
	EXEC createPortalMenuItem '926a9d1e-f017-4284-81e0-bd3c893dad07',@MenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', '' 
	EXEC createPortalMenuItem '1e8fd079-e1a5-458d-9529-567b370e6dc1',@MenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', '' 
	EXEC createPortalMenuItem '875cec25-5abf-402b-8ebd-3b0206c497da',@MenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', '' 
	EXEC createPortalMenuItem '34e6f590-0500-40e2-b34a-d6d2f3fcd3f1',@MenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', '' 
	EXEC createPortalMenuItem '3c1658da-d2b4-40fb-92d6-33285c699ee0',@MenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', '' 
	EXEC createPortalMenuItem '9bb01c81-3409-44b3-b52d-63a8ab5173e8',@MenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', '' 
	EXEC createPortalMenuItem '61fed6ec-f4ba-44bb-b6aa-f876b7b7797a',@MenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', '' 
	EXEC createPortalMenuItem 'b672a55c-7d76-4bfa-8270-15f5c1f5fded',@MenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', '' 
	EXEC createPortalMenuItem '230c4d84-55bb-4f11-b9db-61bbe1a5795f',@MenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', '' 
	EXEC createPortalMenuItem 'fba89d41-924b-465b-bc45-e7ea9e780120',@MenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', '' 
	EXEC createPortalMenuItem '6480B670-7196-4e31-95D6-513AD91F9337',@MenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart'
	EXEC createPortalMenuItem '6f6a3637-e475-4de8-9913-2d8c2e3e786d',@MenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple'
	EXEC createPortalMenuItem 'd5c83ef5-99fa-4005-b54a-d154fe070996',@MenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', '' 
	EXEC createPortalMenuItem '8b540073-a012-4f3c-8366-3482f2842b3c',@MenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', '' 
	


	PRINT('Creating Resource Menu...')
	EXEC createPortalMenuDefinition 'csiResource', 'Resource transactions available in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '792161b4-70b1-44ad-a482-f411245dfacf',@MenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', ''
	EXEC createPortalMenuItem '33c7a587-dfe0-4242-97fb-7692194891c3',@MenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', ''
	EXEC createPortalMenuItem 'b43b198f-0826-465e-93dc-134476df82cd',@MenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', ''
	EXEC createPortalMenuItem 'cda03299-ecd7-41f0-9b62-4658e21635b6',@MenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP', '', ''
	EXEC createPortalMenuItem '3f8864db-c143-4fef-9437-9b74ddad4759',@MenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', ''
	EXEC createPortalMenuItem '38928290-fc89-449f-a74f-2ba26351cf8d',@MenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', ''
	EXEC createPortalMenuItem '0526db1b-98dc-48c3-a57c-b4331e1d5f13',@MenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', ''    
	
	PRINT('Creating Events Menu...')
	EXEC createPortalMenuDefinition 'csiEvent', 'Quality event transactions in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'dc6950fc-be5b-4c4e-8a0d-265b1808b758',@MenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', '' 

	PRINT('Creating Search Menu...')
	EXEC createPortalMenuDefinition 'csiSearch', 'Search options in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'e97814d1-9deb-4478-985c-b85ee25c2691',@MenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP', '', '' 
	EXEC createPortalMenuItem 'f5d5aa48-dabb-4d31-9dd2-2de4516671f5',@MenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '' 
	EXEC createPortalMenuItem '8162b166-dece-476c-9bb5-c6f21e0e4c78',@MenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '' 
	EXEC createPortalMenuItem '56c79c12-3137-4e82-9a53-c33acab9534e',@MenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP', '', '' 
	EXEC createPortalMenuItem 'd0d5ff96-55c4-40e7-85c1-89f52ad17561',@MenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' 
	EXEC createPortalMenuItem '5885791f-400f-43b0-9cae-b3cf01a30eab',@MenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '' 

	PRINT('Creating SPC Menu...')
	EXEC createPortalMenuDefinition 'csiSPC', 'SPC pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '692445b9-b186-4ca7-a8fc-ebf06baa6737',@MenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', '' 
	
	PRINT('Creating Modeling Menu...')
	EXEC createPortalMenuDefinition 'csiModelingMenu', 'Modeling pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'a701f05e-0721-422a-b664-12468edaa57b',@MenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', '' 
	EXEC createPortalMenuItem '29be1c6e-38ee-40d6-9f32-89bb2e5e79d2',@MenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', ''
	EXEC createPortalMenuItem '3fa8ee45-ff69-4a55-982b-f9223978a473',@MenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', ''
	EXEC createPortalMenuItem '10419bfe-b75f-4d49-b1d4-9fb40cd9a5db',@MenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', ''
	
	PRINT('Creating Training Menu...')
	EXEC createPortalMenuDefinition 'csiTrainingMenu', 'Training pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'fee14386-7d0f-42c9-a657-c8c3c64b5c00',@MenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', ''
	EXEC createPortalMenuItem 'e83c6e8c-4b4e-45f1-aa02-5bdaf9c950ce',@MenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', ''
		
	PRINT('Creating Export/Import Menu...')
	EXEC createPortalMenuDefinition 'csiExport/Import', 'Export/Import pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'd38844a6-552e-4e5b-bffd-019e55cd2612',@MenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', ''
	
	PRINT('Creating Change Management Menu...')
	EXEC createPortalMenuDefinition 'csiChangeManagement', 'Change Management pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '1b08241e-1a14-46bb-a928-788877dd46a4',@MenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', ''
	EXEC createPortalMenuItem '0340f41f-9e2c-4aac-a438-451c852125b4',@MenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', ''
	EXEC createPortalMenuItem 'bbdec625-bd4d-4908-a545-6b86c8e416b9',@MenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', ''
	EXEC createPortalMenuItem 'ffa55915-3b91-4798-ae44-bbea44f3213f',@MenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', ''
	EXEC createPortalMenuItem 'ec29e444-8b90-4e9c-bac3-315a3ef1c581',@MenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', ''

	PRINT('Creating Attachments Menu...')
	EXEC createPortalMenuDefinition 'csiAttachments', 'Attachments', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'd1d8c4a5-bcff-45ec-8ffe-a293eb1137a1',@MenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' 
	EXEC createPortalMenuItem '55c20586-706f-4cad-b985-dd42ac3605db',@MenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', ''


	PRINT('Creating Portal Main Menu...')
	EXEC createPortalMenuDefinition 'csiPortalMenu', 'The top level Portal menu', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '6ef8e978-7263-478f-be4d-393cfb645797',@MenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachments', 'cmdAttach'
	EXEC createPortalMenuItem '679fefbb-a5df-4b6a-bb9e-f5296343895f',@MenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagement', 'cmdChangeManagement'
	EXEC createPortalMenuItem 'da7550a9-b7d8-415e-868c-c64d4534ae4d',@MenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainer', 'cmdLot'
	EXEC createPortalMenuItem '02c8704f-6a36-454c-96b3-5ae11ebefd21',@MenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEvent', 'cmdNonConformanceQuality'
	EXEC createPortalMenuItem 'c7973073-db88-45a7-aff2-439661e922a5',@MenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/Import', 'cmdImportExport'
	EXEC createPortalMenuItem '7a479937-410f-49d2-a062-51af870508d9',@MenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenu', 'cmdModelItem'
	EXEC createPortalMenuItem '35ee5f34-a1d4-4eae-8e2f-581462dd7ce6',@MenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResource', 'cmdMachine'
	EXEC createPortalMenuItem '60367011-f6a6-432a-b041-00b11440f650',@MenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearch', 'cmdSearch'
	EXEC createPortalMenuItem '229a27bb-d11d-47fc-ab9f-a5e9a1ceb638',@MenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPC', 'cmdGraph'
	EXEC createPortalMenuItem '3ac566b5-8c1d-43e8-8a26-49ef393ae7b3',@MenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenu', 'cmdTraining'
	
	--Create V8 Menu with Apollo
	PRINT('Creating Container Transaction Menu Definition...')
	EXEC createPortalMenuDefinition 'csiContainerV8', 'Container transactions used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '55a0b6aa-8b5b-49d9-8d6d-ee8623fbd4bc',@MenuDefId, 7835, 10, 'Associate', 'LblMenuAssociate', 'AssociateVP', '', '' 
	EXEC createPortalMenuItem '34a9528f-1393-495f-bc43-b5260d51953e',@MenuDefId, 7835, 15, 'Associate (HPE)', 'LblMenuAssociateHPE', 'AssociateVP', '', '', '', 'DBAssociate' 
	EXEC createPortalMenuItem 'f440d6d7-a274-458a-a3ce-995dfc8cdecc',@MenuDefId, 7835, 20, 'Change Qty', 'LblMenuChangeQty', 'ChangeQtyVP', '', '' 
	EXEC createPortalMenuItem 'a9b41235-9f0f-480c-aba7-7a68d73083b5',@MenuDefId, 7835, 30, 'Change Qty Multi-Reason', 'LblMenuChangeQtyMR','ChangeQtyMultiReasonVP', '', '' 
	EXEC createPortalMenuItem 'c186f6ac-565a-47b7-b888-59cc05141644',@MenuDefId, 7835, 40, 'Close', 'LblMenuClose', 'CloseVP', '', '' 
	EXEC createPortalMenuItem '231cfca7-9a45-491a-82d1-be44b8ff3c8a',@MenuDefId, 7835, 50, 'Close (Multiple)', 'LblMenuCloseMulti', 'MultiContainerCloseVP', '', '' 
	EXEC createPortalMenuItem '3b5302e3-4d5d-4dce-8248-c73fef87cb01',@MenuDefId, 7835, 60, 'Collect Data', 'LblMenuCollectData', 'DataCollectionVP', '', ''
	EXEC createPortalMenuItem '572bf394-f371-41cb-a567-285d31dd9669',@MenuDefId, 7835, 65, 'Collect Sampling Data', 'LblMenuCollectSampData', 'CollectSamplingDataVP', '', '' 
	EXEC createPortalMenuItem '42649f60-5ea0-4e95-b05e-0aaf75ee2bf4',@MenuDefId, 7835, 67, 'Collect Lot Sampling Data', 'LblMenuCollectLotSampData','CollectLotSamplingData_VP', '', '' 
	EXEC createPortalMenuItem '497a5174-e419-40e3-8279-8071c0f047d4',@MenuDefId, 7835, 70, 'Combine Container', 'LblMenuCombCont','CombineContainersVP', '', '' 
	EXEC createPortalMenuItem 'c4978511-88e4-45f1-9b60-532eda1bb0f3',@MenuDefId, 7835, 80, 'Combine Qty', 'LblMenuCombQty','CombineQtyVP', '', '' 
	EXEC createPortalMenuItem '68d7aa17-3d96-4080-93fe-9b3a4dd41cdd',@MenuDefId, 7835, 90, 'Component Defect', 'LblMenuCompDef','ComponentDefectVP', '', '' 
	EXEC createPortalMenuItem '38b2e099-8177-4aad-9a6e-5ca54113d15a',@MenuDefId, 7835, 100, 'Component Issue','LblMenuCompIssue', 'ComponentIssue_VPR2', '', '' 
	EXEC createPortalMenuItem '27c284ea-d208-409d-8773-4e605633e527',@MenuDefId, 7835, 110, 'Component Issue - Advanced', 'LblMenuCompIssueAdv','ComponentIssueAdvancedVP', '', '' 
	EXEC createPortalMenuItem 'c0a83130-2419-4701-b03c-388cb0ada026',@MenuDefId, 7835, 120, 'Component Remove', 'LblMenuCompRemove','ComponentRemoveVP', '', '' 
	EXEC createPortalMenuItem 'fdf43c45-b722-460d-9b68-2203b9ec7197',@MenuDefId, 7835, 121, 'Component Replace', 'CSICDOName_ComponentReplace','ComponentReplaceVP', '', '' 
	EXEC createPortalMenuItem 'fa645aa7-c7d8-4176-9854-33b087cee863',@MenuDefId, 7835, 122, 'Container Attribute Maintenance', 'LblMenuContAttrMaint','ContainerAttrMaintVP', '', '' 
	EXEC createPortalMenuItem 'b973940f-b162-4fe2-af15-fb84fd1a38be',@MenuDefId, 7835, 123, 'Container Maintenance', 'LblMenuContMaint','ContainerMaintenanceVP', '', ''
	EXEC createPortalMenuItem 'b5a2daf4-4f8e-4688-b634-3f317c1c99ac',@MenuDefId, 7835, 124, 'Container Rename', 'CSICDOName_ContainerRename','RenameVP', '', ''  
	EXEC createPortalMenuItem 'b5a2daf4-4f8e-4688-b634-3f317c1c11qr',@MenuDefId, 7835, 125, 'Container Rename (Multiple)', 'LblMenuRenameMulti','MultiContainerRenameVP', '', '' 
	EXEC createPortalMenuItem '4642d6d8-4f17-4e0d-ac5c-0ab74c69001c',@MenuDefId, 7835, 136, 'Create Sampling Lot', 'LblMenuCreateSampLot','CreateSamplingLot_VP', '', '' 
	EXEC createPortalMenuItem '0cad9a20-ef44-4c44-adc6-8387ea503f5a',@MenuDefId, 7835, 137, 'Current Sampling Status Update', 'LblMenuCurrSampStatusUpd','CurrentSamplingStatusUpdate_VP', '', '' 
	EXEC createPortalMenuItem '9da6e49c-fe85-4ce3-82d6-a41afaab9081',@MenuDefId, 7835, 138, 'Defect', 'LblMenuDefect','ContainerDefectVP', '', '' 
	EXEC createPortalMenuItem '94d4e5ee-4b80-411e-952d-9052871cb8ae',@MenuDefId, 7835, 140, 'Disassociate', 'LblMenuDisassociate','DisassociateVP', '', '' 
	EXEC createPortalMenuItem '8c420adf-2751-4719-8eda-594b439f9cc8',@MenuDefId, 7835, 145, 'Disassociate (HPE)', 'LblMenuDisassociateHPE', 'DisassociateVP', '', '', '', 'DBDisassociate'
	EXEC createPortalMenuItem '8d026f03-26d5-4364-9da5-43f96e0ccffc',@MenuDefId, 7835, 150, 'EProcedure', 'LblMenuEProc','EprocedureVPR2', '', '' 
	EXEC createPortalMenuItem 'd992a558-e51f-4f08-b24c-be29da73f978',@MenuDefId, 7835, 160, 'Hold', 'LblMenuHold','ContainerHoldVP', '', '' 
	EXEC createPortalMenuItem 'c7cdb3ca-b85a-4193-9dcc-e2956e862317',@MenuDefId, 7835, 170, 'Hold (Multiple)', 'LblMenuHoldMulti','MultiContainerHoldVP', '', '' 
	EXEC createPortalMenuItem 'a4b47820-afc5-4903-9298-bd272bfc971f',@MenuDefId, 7835, 180, 'Move', 'LblMenuMove','MoveStdVP', '', '' 
	EXEC createPortalMenuItem 'ac26d32a-d827-41a5-9f89-3f76a5a8b613',@MenuDefId, 7835, 190, 'Move In', 'LblMenuMoveIn','MoveInVP', '', '' 
	EXEC createPortalMenuItem '7eeaecd2-f566-4c38-b8cd-36da03499c96',@MenuDefId, 7835, 200, 'Move Non-Std', 'LblMenuMoveNonStd','MoveNonStdVP', '', '' 
	EXEC createPortalMenuItem '640f68c5-7ab9-4929-9b74-36f26de0e0b7',@MenuDefId, 7835, 210, 'Move Non-Std (Multiple)', 'LblMenuMoveNonStdMulti','MultiContainerMoveNonStdVP', '', '' 
	EXEC createPortalMenuItem 'ed74b799-6b15-4cfe-b9c9-d4181657ef60',@MenuDefId, 7835, 220, 'Open', 'LblMenuOpen','OpenVP', '', '' 
	EXEC createPortalMenuItem '6f3c248a-2d4c-426e-976d-f2ba7e89ab65',@MenuDefId, 7835, 230, 'Open (Multiple)', 'LblMenuOpenMulti', 'MultiContainerOpenVP', '', '' 
	EXEC createPortalMenuItem '6c84fe43-e4e0-4779-90fd-9f76afe97f35',@MenuDefId, 7835, 240, 'Operational View', 'LblMenuOperView','OperationalViewVPR2', '', '' 
	EXEC createPortalMenuItem '4fa4c534-03b3-4ae9-aa0b-13e85bf20171',@MenuDefId, 7835, 260, 'Order Dispatch', 'LblMenuOrderDisp','OrderDispatchVP', '', '' 
	EXEC createPortalMenuItem 'c8084d25-25dd-4ce8-a071-6986e5232372',@MenuDefId, 7835, 270, 'Print Container Label', 'LblMenuPrtContLab','PrintContainerLabelVP', '', '' 
	EXEC createPortalMenuItem '622b76da-8860-4b2a-b909-fe05e153e478',@MenuDefId, 7835, 280, 'Print Production Event Label', 'LblMenuPrtProdEventlab','PrintProductionEventLabelVP', '', '' 
	EXEC createPortalMenuItem '36d6e3df-a19c-4d8d-8e62-00610c6eda1d',@MenuDefId, 7835, 290, 'Record Production Event', 'LblMenuRecProdEvt','ProductionEventRecord_VPR2', '', '' 
	EXEC createPortalMenuItem '6cac0633-d2df-42c2-ad07-e85b00a2990f',@MenuDefId, 7835, 300, 'Release', 'LblMenuRelease','ContainerReleaseVP', '', '' 
	EXEC createPortalMenuItem '5b7fe08a-6b42-465f-b5de-c6f2d18b1d36',@MenuDefId, 7835, 310, 'Release (Multiple)', 'LblMenuReleaseMulti','MultiContainerReleaseVP', '', '' 
	EXEC createPortalMenuItem '1636b79c-c29e-4d42-b490-20a519f4ab9a',@MenuDefId, 7835, 320, 'Reprint Container Label', 'LblMenuReprtContLabel','ReprintContainerLabelVP', '', '' 
	EXEC createPortalMenuItem '01477baf-7d36-49ea-b8fe-bcd92afff448',@MenuDefId, 7835, 330, 'Reverse Last Transaction', 'LblRevLastTran','TxnReversalVP', '', '' 
	EXEC createPortalMenuItem 'be687870-a160-41ea-a42b-a540575dedfc',@MenuDefId, 7835, 334, 'Rework', 'LblMenurework','ReworkVP', '', '' 
	EXEC createPortalMenuItem '72d52fa2-129c-4adc-b311-817ebeac5835',@MenuDefId, 7835, 340, 'Ship', 'LblMenuShip', 'ShipVP', '', '' 
	EXEC createPortalMenuItem '45c73a6d-e442-4d59-a9f5-4d31536b5b45',@MenuDefId, 7835, 350, 'Split Container', 'LblMenuSplitCont','SplitContainerVP', '', '' 
	EXEC createPortalMenuItem 'c315bae8-409d-4a4c-acfd-44e66f004fa2',@MenuDefId, 7835, 360, 'Split Qty', 'LblMenuSplitQty','SplitQuantityVP', '', '' 
	EXEC createPortalMenuItem 'c4fcb1f9-b119-47bf-86ea-d29cf97a5cf6',@MenuDefId, 7835, 370, 'Start', 'LblMenuStart','StartPage', '', '' 
	EXEC createPortalMenuItem '8188609b-d795-4d37-a860-69097e218487',@MenuDefId, 7835, 380, 'Start - Two Level', 'LblMenuStartTwoLev','TwoLevelStartVP', '', '' 
	EXEC createPortalMenuItem '6480B670-7196-4e31-95D6-513AD91F9337',@MenuDefId, 7835, 385, 'Start - Bulk (HPE)', 'LblStartBulkHPE','TwoLevelStartVP', '', '', '', 'DBStart'
	EXEC createPortalMenuItem '6f6a3637-e475-4de8-9913-2d8c2e3e786d',@MenuDefId, 7835, 386, 'Start - Bulk Simple (HPE)', 'LblStartBulkSimpleHPE','TwoLevelStartVP', '', '', '', 'DBStartSimple'
	EXEC createPortalMenuItem '74ed56d8-c225-4448-b01a-34bf30bb954b',@MenuDefId, 7835, 390, 'Thruput', 'LblMenuThruput','ContainerThruputVP', '', '' 
	EXEC createPortalMenuItem '1ead6953-5719-47f5-abfa-58cae66ad026',@MenuDefId, 7835, 400, 'Update Sampling Lot', 'LblUpdSampLot','UpdateSamplingLot_VP', '', '' 
	EXEC createPortalMenuItem '02d26499-f1a2-4ede-b317-257beb01c8d5',@MenuDefId, 7835, 410, 'HV Component Issue', 'CSICDOName_HVComponentIssue','HVComponentIssueVP', '', '' 
	EXEC createPortalMenuItem '149efb0f-330b-4966-b890-37186e230624',@MenuDefId, 7835, 420, 'Slitting', 'CSICDOName_Slitting','SlittingVP', '', '' 	 	
	
	PRINT('Creating Job Services Menu...')
	EXEC createPortalMenuDefinition 'csiResourceTxn_JobSvc', 'Job Services', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '0ff85e59-45e3-4e84-b0d1-fa93633bf0e8',@MenuDefId, 7835, 10, 'Job Create', 'CSICDOName_JobCreate','JobCreateVP', '', ''
	EXEC createPortalMenuItem '56d4f7f2-cdd7-4e7e-8382-a4e9df3e8594',@MenuDefId, 7835, 20, 'Job Assign', 'CSICDOName_JobAssign','JobAssign_VP', '', ''
	EXEC createPortalMenuItem 'a4a9b01a-3172-42e0-8932-b30ef646e69d',@MenuDefId, 7835, 30, 'Job Acknowledge', 'CSICDOName_JobAcknowledge','JobAcknowledge_VP', '', ''
	EXEC createPortalMenuItem '053f0546-5802-452a-9adf-eb1dc938fec0',@MenuDefId, 7835, 40, 'Job Clock On', 'CSICDOName_JobClockOn','JobClockOn_VP', '', ''
	EXEC createPortalMenuItem '0dfd4298-739c-4697-98d3-215eaec40921',@MenuDefId, 7835, 50, 'Job Progress', 'CSICDOName_JobProgress','JobProgress_VP', '', ''
	EXEC createPortalMenuItem '0a4a3756-3347-4491-a84a-007b5d3d8d2f',@MenuDefId, 7835, 60, 'Job Clock Off', 'CSICDOName_JobClockOff','JobClockOff_VP', '', ''
	EXEC createPortalMenuItem '9747345c-4bb7-46ad-825d-bfc9efdb5eba',@MenuDefId, 7835, 70, 'Job Complete', 'CSICDOName_JobComplete','JobComplete_VP', '', ''
	EXEC createPortalMenuItem '8173d081-9bcd-4f0e-8f12-9bafb85ea71f',@MenuDefId, 7835, 80, 'Job Cancel', 'CSICDOName_JobCancel','JobCancel_VP', '', ''
	
	PRINT('Creating Jobs Menu...')
	EXEC createPortalMenuDefinition 'csiResourceTxn_Job', 'Jobs', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '2fcbbba7-4cd4-44fe-9ebb-4b59a85c059f',@MenuDefId, 7835, 10, 'Supervisor Jobs', 'LblMenuSupervisorJobs','JobSupervisor_VP', '', ''
	EXEC createPortalMenuItem '5dccb0f0-10d8-4eae-818b-9170425becc3',@MenuDefId, 7835, 20, 'Technician Jobs', 'LblMenuTechnicianJobs','JobTechnicians_VP', '', ''
	EXEC createPortalMenuItem '11ba1a9b-eed9-4b69-9b8f-552c8b9af22c',@MenuDefId, 7833, 30, 'Job Services', 'PortalUI_JobServices', '','', 'csiResourceTxn_JobSvc', ''

	PRINT('Creating Part Services Menu...')
	EXEC createPortalMenuDefinition 'csiResourceTxn_PartSvc', 'Part Services', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '48b7ecf8-cd8b-4e82-b728-8780e841dc46',@MenuDefId, 7835, 10, 'Part Create', 'CSICDOName_PartCreate','PartCreate_VP', '', ''
	EXEC createPortalMenuItem 'fdec5a97-aa40-43bf-9447-4a3ac43f986e',@MenuDefId, 7835, 20, 'Part Setup', 'CSICDOName_PartSetup','PartSetupVP', '', ''
	EXEC createPortalMenuItem '5610baf0-b012-421c-9616-6520b8937230',@MenuDefId, 7835, 30, 'Part Scrap', 'CSICDOName_PartScrap','PartScrap_VP', '', ''
	EXEC createPortalMenuItem 'c2b8aeb3-54a6-44ff-be63-96cd0a3dcc04',@MenuDefId, 7835, 40, 'Part Request', 'CSICDOName_PartRequest','PartRequestVP', '', ''
	EXEC createPortalMenuItem 'c68e1bd9-1e74-4583-95e1-7012892ed9a3',@MenuDefId, 7835, 50, 'Part Request Acknowledge', 'CSICDOName_PartRequestAcknowledge','PartRequestAcknowledgeVP', '', ''
	EXEC createPortalMenuItem 'cfeb52a7-0550-4da2-9878-c507cdfcdbdb',@MenuDefId, 7835, 60, 'Part Request Assign', 'CSICDOName_PartRequestAssign','PartRequestAssign_VP', '', ''
	EXEC createPortalMenuItem 'cfcf7fe0-5a97-4d5d-a0b1-5c5d3c6e83b4',@MenuDefId, 7835, 70, 'Part Request Update', 'CSICDOName_PartRequestUpdate','PartRequestUpdateVP', '', ''
	EXEC createPortalMenuItem '192e8ba2-1b46-45d1-b9fa-d3372fb42b39',@MenuDefId, 7835, 80, 'Part Request Issue', 'CSICDOName_PartRequestIssue','PartRequestIssue_VP', '', ''
	EXEC createPortalMenuItem 'c90a1000-f1ec-49ed-bf90-30054da09189',@MenuDefId, 7835, 90, 'Part Request Complete', 'CSICDOName_PartRequestComplete','PartRequestComplete_VP', '', ''
	EXEC createPortalMenuItem 'c11945de-03ee-4df9-b1ed-90b143c3a65a',@MenuDefId, 7835, 100, 'Part Request Cancel', 'CSICDOName_PartRequestCancel','PartRequestCancel_VP', '', ''
	EXEC createPortalMenuItem 'a0fd92e2-1f04-403a-916a-773327edd988',@MenuDefId, 7835, 110, 'Part Request Cancel Acknowledge', 'CSICDOName_PartRequestCancelAcknowledge','PartRequestCancelAcknowledgeVP', '', ''
	
	PRINT('Creating Parts Menu...')
	EXEC createPortalMenuDefinition 'csiResourceTxn_Part', 'Parts', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'b0e625f9-c13a-42c7-85c9-82e13a5a4c79',@MenuDefId, 7835, 10, 'Material Parts', 'PartRequestOrder_MaterialParts','MaterialPartMaintenanceVP', '', ''
	EXEC createPortalMenuItem '11cb9e4f-60bd-4dcb-abf4-b88612cfa729',@MenuDefId, 7835, 20, 'Part Maintenance', 'LblPartMaintenance','PartMaintenanceVP', '', ''
	EXEC createPortalMenuItem '457df0b1-4ea1-46d9-8263-e20312bb2a24',@MenuDefId, 7835, 30, 'Technician Requests', 'LblTechnicianRequests','PartRequestMain_VP', '', '', '', 'PartRequest'
	EXEC createPortalMenuItem 'eb52a625-ea91-4f2f-a859-2e07b98bdc6f',@MenuDefId, 7835, 40, 'Inventory Requests', 'LblInventoryRequests','PartRequestMain_VP', '', '', '', 'PartRequestAssign'
	EXEC createPortalMenuItem 'ee002858-401d-4b31-8621-a524fc364a40',@MenuDefId, 7833, 50, 'Part Services', 'PortalUI_PartServices', '','', 'csiResourceTxn_PartSvc', ''
	
	PRINT('Creating Resource Menu...')
	EXEC createPortalMenuDefinition 'csiResourceV8', 'Resource transactions available in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'd6200a6b-1666-47ae-9859-ca57906481c6',@MenuDefId, 7835, 10, 'Maintenance Class Activation', 'LblMenuMaintClassAct','MaintClassActivation_VP', '', ''
	EXEC createPortalMenuItem '394d4941-c79d-47f4-8cc2-5d1a7d19904e',@MenuDefId, 7835, 20, 'Maintenance Management', 'LblMenuMaintMngt','MaintenanceManagementVP', '', ''
	EXEC createPortalMenuItem '6907d956-424f-4010-9c2c-c14502317a2a',@MenuDefId, 7835, 30, 'Resource Activation', 'LblMenuResAct','ResourceActivation_VP', '', ''
	EXEC createPortalMenuItem 'c6c19c25-d580-4c43-afa9-5df1faee2791',@MenuDefId, 7835, 40, 'Resource Audit Trail', 'LblMenuResourceAuditTrail','ResourceAuditTrailVP_R2', '', ''
	EXEC createPortalMenuItem 'c3544fca-240e-4a35-b7b0-a163fc2b64b3',@MenuDefId, 7835, 50, 'Resource Data Collection', 'LblMenuResDataColl','ResourceCollectDataVP', '', ''
	EXEC createPortalMenuItem 'd754fd28-e8a2-47b0-a6e7-869fca382fa6',@MenuDefId, 7835, 60, 'Resource Setup', 'LblMenuResSetup','ResourceSetupVP', '', ''
	EXEC createPortalMenuItem 'dcd75e8e-4f2e-497f-8923-d5e151ef364d',@MenuDefId, 7835, 70, 'Resource Thruput', 'LblMenuResThruput','ResourceThruputVP', '', '' 
	EXEC createPortalMenuItem '64fbdadb-bf64-4b58-b122-3e33198c4b34',@MenuDefId, 7833, 80, 'Jobs', 'PortalUI_Jobs', '','', 'csiResourceTxn_Job', ''
	EXEC createPortalMenuItem '24193c6e-aea6-40c6-9019-7f78d5b459e8',@MenuDefId, 7833, 90, 'Parts', 'Resource_Parts', '','', 'csiResourceTxn_Part', ''
	EXEC createPortalMenuItem 'cef6c008-c78d-4424-861e-289ae5e19508',@MenuDefId, 7835, 100, 'HV Resource Setup', 'CSICDOName_HVResourceSetup','HVResourceSetupVP', '', ''

	PRINT('Creating Events Menu...')
	EXEC createPortalMenuDefinition 'csiEventV8', 'Quality event transactions in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '29d6e58c-1fdf-478f-858d-4a37f6c6d08f',@MenuDefId, 7835, 10, 'Record Generic Event', 'LblMenuRecGenericEvt','', 'CreateGenericEvent_PF.1', '' 

	PRINT('Creating Search Menu...')
	EXEC createPortalMenuDefinition 'csiSearchV8', 'Search options in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '00fde9db-f437-4cd1-a489-2ab04eba6cac',@MenuDefId, 7835, 10, 'Container Search', 'LblMenuContSearch','ContainerSearchVP_R2', '', '' 
	EXEC createPortalMenuItem 'ad8c718b-f42c-40d8-ad79-107bfe35b4f7',@MenuDefId, 7835, 20, 'Delegation Search', 'LblMenuDelegationSearch', 'DelegationSearch_VP', '', '' 
	EXEC createPortalMenuItem '23ae4e7b-8264-42f3-a5b9-7bc6e6af6c8c',@MenuDefId, 7835, 30, 'Message Center', 'LblMenuMsgCenter','MessageCenterVP', '', '' 
	EXEC createPortalMenuItem 'a4d03dbb-82d4-46a8-a2d9-4df5b7167bc5',@MenuDefId, 7835, 40, 'Mfg Audit Trail', 'LblMenuMfgAuditTrail', 'MfgAuditTrailVP_R2', '', '' 
	EXEC createPortalMenuItem '45a90235-67d5-46a5-a078-fbc98daf87fa',@MenuDefId, 7835, 50, 'Process Timer Search', 'LblMenuProcesstimerSearch', 'ProcessTimerInquiry_VP', '', '' 
	EXEC createPortalMenuItem '242b00aa-a7e0-4b13-a2eb-0127ccbf0037',@MenuDefId, 7835, 60, 'Quality Search', 'LblMenuQualSearch','QualitySearch_VP', '', '' 

	PRINT('Creating SPC Menu...')
	EXEC createPortalMenuDefinition 'csiSPCV8', 'SPC pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'e9ce8f2b-593e-4bd8-9dcf-2fc1b7b47207',@MenuDefId, 7835, 10, 'SPC Tester', 'LblMenuSPCTester', 'SPCTesterFormVP', '', '' 
	EXEC createPortalMenuItem 'a3c99df1-3f08-4e4c-be30-96dd1487a1a5',@MenuDefId, 7835, 20, 'SPC Realtime Monitoring', 'LblSPCRealtimeMonitoring', 'SPCRealtimeMonitoring_VP', '', '' 
	
	PRINT('Creating Modeling Menu...')
	EXEC createPortalMenuDefinition 'csiModelingMenuV8', 'Modeling pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '95c84560-dca1-4217-98be-853c6a1a65b8',@MenuDefId, 7835, 10, 'Modeling', 'LblMenuModeling','ModelingVP', '', '' 
	EXEC createPortalMenuItem '35e02673-a6b8-4f70-8140-78815de31c5f',@MenuDefId, 7835, 20, 'Modeling Audit Trail', 'LblMenuModelingAuditTrail', 'ModelingAuditTrail_VP', '', ''
	EXEC createPortalMenuItem '9ccae5c7-fc8c-47df-a15e-38939d36a83d',@MenuDefId, 7835, 30, 'Modeling ESig', 'LblMenuModEsig','ModelingESig_VP', '', ''
	EXEC createPortalMenuItem 'eb39dbab-e3d7-4acd-bbf9-c2480cb22d9f',@MenuDefId, 7835, 40, 'Factory Hierarchy', 'FactoryHierarchy','FactoryHierarchy_VP', '', ''
	
	PRINT('Creating Training Menu...')
	EXEC createPortalMenuDefinition 'csiTrainingMenuV8', 'Training pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '1ec4517a-7418-41b4-adc6-b440d2ccf934',@MenuDefId, 7835, 10, 'Training Record Comparison', 'LblMenuTrgRecComp','TrainingRecordComparison_VP', '', ''
	EXEC createPortalMenuItem '5e2942b5-402b-48ef-beaf-316a88928202',@MenuDefId, 7835, 20, 'Training Record Management', 'LblMenuTrgRecMngt','TrainingRecordManagement_VP', '', ''
		
	PRINT('Creating Export/Import Menu...')
	EXEC createPortalMenuDefinition 'csiExport/ImportV8', 'Export/Import pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'a1b686f0-031b-4bf5-add6-3ccc058737a9',@MenuDefId, 7835, 10, 'Export/Import', 'LblMenuExpImp','', 'DataTransferPF.1', ''
	
	PRINT('Creating Change Management Menu...')
	EXEC createPortalMenuDefinition 'csiChangeManagementV8', 'Change Management pages used in the Portal', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '441b4d9f-fcaf-47e6-ac91-dc9316989d95',@MenuDefId, 7835, 10, 'Activation Search', 'LblMenuActSearch','ActivationInquiry_VP', '', ''
	EXEC createPortalMenuItem 'de1977c3-b43f-4ba6-8bce-9476fff22121',@MenuDefId, 7835, 20, 'Activation Search (Multiple)', 'LblMenuActSearchMultiple','ActivationSearchMultiple_VP', '', ''
	EXEC createPortalMenuItem '88d5dcd5-24c5-4507-8fbc-90dccb006af9',@MenuDefId, 7835, 30, 'Create Package', 'LblMenuCreatePkg','StartChangePkg_VP', '', ''
	EXEC createPortalMenuItem 'e0ae88de-3b17-4d6c-9acb-102f5cf2d6cb',@MenuDefId, 7835, 40, 'Package Search', 'LblMenuPackSearch','PackageInquiry_VP', '', ''
	EXEC createPortalMenuItem 'cf0fc4e0-7f1d-4ef9-a3f9-24d9bfdf4106',@MenuDefId, 7835, 50, 'Package Search (Multiple)', 'LblMenuPackSearchMultiple','PackageSearchMultiple_VP', '', ''

	PRINT('Creating Attachments Menu...')
	EXEC createPortalMenuDefinition 'csiAttachmentsV8', 'Attachments', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'bc1cbd8c-9a5c-4339-80bb-e097316eca05',@MenuDefId, 7835, 10, 'Attach Document', 'Lbl_AttachDocument_Title','AttachDocument_VP', '', '' 
	EXEC createPortalMenuItem '56b93028-39e9-4ffc-abec-d2b0090f22d2',@MenuDefId, 7835, 20, 'Manage Attachments', 'LblMenuManageAttachments','AttachDocumentManagement_VP', '', ''


	PRINT('Creating Portal V8 Main Menu...')
	EXEC createPortalMenuDefinition 'csiPortalMenuV8', 'The top level Portal menu', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'c1d5a088-8cdd-46ae-9fdd-7c04b629067b',@MenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach'
	EXEC createPortalMenuItem 'c8f8266f-25c5-44f9-8aea-5527e0d0ddb7',@MenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement'
	EXEC createPortalMenuItem 'dd35afd2-96f4-46f4-a1b6-0ac553b701d3',@MenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot' 
	EXEC createPortalMenuItem '54ac8bb0-6c05-4ecd-9392-4050df554cee',@MenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality'
	EXEC createPortalMenuItem '34fe7232-e872-4eb2-9a2e-a212bab0bfb2',@MenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport'
	EXEC createPortalMenuItem '3bdc329a-1122-4c0d-b683-5ca2ea894f5d',@MenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem' 
	EXEC createPortalMenuItem 'f0b71eee-e488-4e44-8dcb-eeac1c7f6719',@MenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine' 
	EXEC createPortalMenuItem 'a011f6d3-6b3d-452b-954b-7de8248791b5',@MenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch' 
	EXEC createPortalMenuItem '6b2ecf0c-e188-41bf-8cfc-bde8acc60a99',@MenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph'
	EXEC createPortalMenuItem 'b8eb02d6-0949-4bfc-bf08-d90aaac48d4c',@MenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining'

		--'Creating Container Mobile Sub Menu'
	PRINT('Creating Container Mobile Sub Menu...')
	EXEC createPortalMenuDefinition 'csiContainerMobileMenu', 'Mobile Menu for Container Txns', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem '6f006bf0-9da5-43bf-ab81-99eaea81d7e4',@MenuDefId, 7835, 40, 'Move', 'LblMenuMove', 'MoveStdVP_R2', '', ''

	--'Creating Mobile Menu'
	PRINT('Creating Mobile Main Menu...')
	EXEC createPortalMenuDefinition 'csiMobileMenu', 'The top level Mobile Portal menu', @DefaultNotes, @MenuDefId OUTPUT
	EXEC createPortalMenuItem 'd6fb38eb-c15e-4518-9a68-d482391e4a19',@MenuDefId, 7833, 10, 'Attachments', 'LblMenuAttachments', '', '', 'csiAttachmentsV8', 'cmdAttach'
	EXEC createPortalMenuItem '6faf9186-4a1b-4636-af34-a2749e0c248b',@MenuDefId, 7833, 20, 'Change Management', 'LblMenuChgMngt','', '', 'csiChangeManagementV8', 'cmdChangeManagement'
	EXEC createPortalMenuItem 'dd3958f2-d62e-42ec-aea8-39254fd491ee',@MenuDefId, 7833, 30, 'Container','LblMenuCont', '', '', 'csiContainerV8', 'cmdLot' 
	EXEC createPortalMenuItem 'ca3f3dd0-f757-486d-b1c0-5ebca6af65c3',@MenuDefId, 7833, 40, 'Event', 'LblMenuEvent', '', '', 'csiEventV8', 'cmdNonConformanceQuality'
	EXEC createPortalMenuItem 'f145426b-9253-490d-a9c3-f0370c35ffb6',@MenuDefId, 7833, 50, 'Export/Import', 'LblMenuExpImp','', '', 'csiExport/ImportV8', 'cmdImportExport'
	EXEC createPortalMenuItem '1b4bca0e-b1ae-48dd-b5e9-e05cf4aeb339',@MenuDefId, 7833, 60, 'Modeling', 'LblMenuModeling','', '', 'csiModelingMenuV8', 'cmdModelItem' 
	EXEC createPortalMenuItem 'eb8e51e0-d2bb-4ed5-9197-58be49b22857',@MenuDefId, 7833, 70, 'Resource', 'LblMenuRes','', '', 'csiResourceV8', 'cmdMachine' 
	EXEC createPortalMenuItem '99485ba4-0815-464e-a959-821f94d7d0dc',@MenuDefId, 7833, 80, 'Search', 'LblMenuSearch','', '', 'csiSearchV8', 'cmdSearch' 
	EXEC createPortalMenuItem 'e6ae0b73-528f-407f-94fb-6f1a30613c4c',@MenuDefId, 7833, 90, 'SPC', 'LblMenuSPC','', '', 'csiSPCV8', 'cmdGraph'
	EXEC createPortalMenuItem '41709d3c-062d-408c-9a45-9d749dbc762c',@MenuDefId, 7833, 95, 'Training', 'LblMenuTrg','', '', 'csiTrainingMenuV8', 'cmdTraining'


	SELECT @csiPortalMenuPortalMenuDefinitionId = PortalMenuDefinitionId 
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiPortalMenu'

	SELECT @csiMobileMenuPortalMenuDefinitionId = PortalMenuDefinitionId 
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiMobileMenu'

	SELECT @csiPortalMenuV8PortalMenuDefinitionId = PortalMenuDefinitionId 
	FROM portalmenuDefinition 
	WHERE PortalMenuDefinitionName = 'csiPortalMenuV8'

	UPDATE EMPLOYEE 
	SET  PortalMenuDefinitionId = @csiPortalMenuPortalMenuDefinitionId,
	PortalMobileMenuDefinitionId = @csiMobileMenuPortalMenuDefinitionId,
	PortalV8MenuDefinitionId = @csiPortalMenuV8PortalMenuDefinitionId
	WHERE EmployeeName = 'CamstarAdmin'
	--This gets set in the Loader. Not needed here for all 3 users.
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' ) 
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'CamstarAdmin')
			
	UPDATE EMPLOYEE 
	SET  PortalMenuDefinitionId = @csiPortalMenuPortalMenuDefinitionId,
	PortalMobileMenuDefinitionId = @csiMobileMenuPortalMenuDefinitionId,
	PortalV8MenuDefinitionId = @csiPortalMenuV8PortalMenuDefinitionId
	WHERE EmployeeName = 'InsiteAdmin'
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' )
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'InsiteAdmin')
		
	UPDATE EMPLOYEE 
	SET  PortalMenuDefinitionId = @csiPortalMenuPortalMenuDefinitionId,
	PortalMobileMenuDefinitionId = @csiMobileMenuPortalMenuDefinitionId,
	PortalV8MenuDefinitionId = @csiPortalMenuV8PortalMenuDefinitionId
	WHERE EmployeeName = 'Administrator'
	--UPDATE UIPortalProfile SET PortalHomePageId = (SELECT UIVirtualPageId from UIVirtualPage where UIVirtualPageName = 'EProceduresVP' )
	--WHERE ParentId IN (SELECT employeeid from Employee WHERE EmployeeName = 'Administrator')

	COMMIT TRAN
END
GO

EXEC populatePortalMenuDefaultData
GO
--DROP PROCEDURE getNextInstanceId
--GO
--DROP PROCEDURE createPortalMenuItem
--GO
--DROP PROCEDURE createPortalMenuDefinition
--GO
--DROP PROCEDURE populatePortalMenuDefaultData
--GO
