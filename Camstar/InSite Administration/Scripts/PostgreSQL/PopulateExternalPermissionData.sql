--------------------------------------------------------------------------------
-- SCRIPT:PopulateExternalPermissionData.sql
-- DESCR: Creates stored procedures used to create External Permissions, etc.
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2023  


--------------------------------------------------------------------------------------------------
-- Function to create instance id strings from a CDODefId and Instance Id number
--
-- 
-- Modification History:
-- Name            	Date        Action
-- --------------   ----------  ----------------
-- Winfred Nah		12/17/2021  Added all External Permission Items for External Permission Modeling page.
-- Gene Taylor		10/28/2022  Added stored proc named csiRemoveExternalPermission and exec a call to it.
-- Matt Nolte		11/03/2022  Added more "Split and Splice" permissions

-- Copyright Siemens 2023  
 
--------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateExternalPermission
-- DESCR: Creates a External Permission Items
-- Copyright Siemens 2023 

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRBACCreateExternalPermission')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRBACCreateExternalPermission;
 	END IF;
END $$;
CREATE PROCEDURE csiRBACCreateExternalPermission(
	pExternalPermissionName VARCHAR(255), 
	pSRCApplication VARCHAR(255), 
	pSRCModule VARCHAR(255), 
	pSRCLevel VARCHAR(30))
LANGUAGE plpgsql
AS $$
DECLARE
    vIID VARCHAR(16);
BEGIN    

	IF NOT EXISTS (SELECT * FROM ExternalPermission WHERE ExternalPermissionName = pExternalPermissionName) THEN
	BEGIN
		CALL csiPRDGetNextInstanceId(8872,vIID);
		pExternalPermissionName := LTRIM(RTRIM(pExternalPermissionName));
    
		INSERT INTO ExternalPermission(CDOTypeId,ChangeCount,ExternalPermissionId,ExternalPermissionName,IsFrozen,SRCApplication,SRCModule,SRCLevel,Required)
			VALUES(8872,1,vIID,pExternalPermissionName,0,pSRCApplication,pSRCModule,pSRCLevel, 1);
	END;
	END IF;
	
END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateExternalPermissionData
-- DESCR: Helper function to create External Permission record
--
-- Copyright Siemens 2023  
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('rbacPopulateExternalPermissionData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS rbacPopulateExternalPermissionData;
 	END IF;
END $$;
CREATE PROCEDURE rbacPopulateExternalPermissionData()
LANGUAGE plpgsql
AS $$
BEGIN    

	RAISE NOTICE 'Creating "External Permission" Items...'; 
	CALL csiRBACCreateExternalPermission('Administrative',NULL,'Valor', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Editing Assembly Properties',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Ignoring Symptom',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow IPASS In vQM',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Scanning Work Order Numbers',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Setting Equipment And Assembly Properties',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Trashing A Board',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Working With Assembly Registration',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Allow Working With vQM',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Create Admin Business Category','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create AQL',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Create Assembly','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Attachements','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Calendar','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Causer Operation',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Create Customer','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Database Editor',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Defect And Repair',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Create Equipment','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Factory Business Category','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Group','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Label Definition',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Create Operation Type','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Order Business Category','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Panel','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Parts','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Plan Business Category','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Product Business Category','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Production Limit And Notification',NULL,'Quality Management');
	CALL csiRBACCreateExternalPermission('Create Route','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Create Skill','WorkSpace','Foundation', 'Create');
	--CALL csiRBACCreateExternalPermission('Create Symptom Template And Edit Failure History',NULL,'Quality Management');
	CALL csiRBACCreateExternalPermission('Create Work Order','WorkSpace','Foundation', 'Create');
	CALL csiRBACCreateExternalPermission('Disable System',NULL,'Production', 'Create');
	CALL csiRBACCreateExternalPermission('Force Release Machine','Portal','VIoT', 'Create');
	--CALL csiRBACCreateExternalPermission('Modify AQL',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Assembly','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Attachements','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Calendar','WorkSpace','Foundation', 'Modify');
	--CALL csiRBACCreateExternalPermission('Modify Causer Operation',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Customer','WorkSpace','Foundation', 'Modify');
	--CALL csiRBACCreateExternalPermission('Modify Database Editor',NULL,'Quality Management', 'Modify');
	--CALL csiRBACCreateExternalPermission('Modify Defect And Repair',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Equipment','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Group','WorkSpace','Foundation', 'Modify');
	--CALL csiRBACCreateExternalPermission('Modify Label Definition',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Operation Type','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Panel','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Parts','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify PCB Buffer','Modify PCB Buffer','Production', 'Create');
	--CALL csiRBACCreateExternalPermission('Modify Production Limit And Notification',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Route','WorkSpace','Foundation', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Skill','WorkSpace','Foundation', 'Modify');
	--CALL csiRBACCreateExternalPermission('Modify Symptom Template And Failure History',NULL,'Quality Management', 'Modify');
	CALL csiRBACCreateExternalPermission('Modify Work Order','WorkSpace','Foundation', 'Modify');
	--CALL csiRBACCreateExternalPermission('Quality Management Administrator',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Run AOI Reader','AOIDataReader.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run AVL Configuration','AvlConfiguration.exe','Production', 'Read');
	--CALL csiRBACCreateExternalPermission('Run Camera Calibration Tool','CameraCalibrationTool.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Configure Barcodes','ConfigBarcodes.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Configure Blocked Reels','ConfigBlockedReels.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Create Database','CreateDatabase.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run DB Browser','DbBrowser.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run DB Connection Tool','DbConnectionTool.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run DB Exporter','TXDBExporter.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Dry Oven','DryStorage.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Dry Storage','DryStorage.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Error Code Update','ErrorListInstall.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Feeder Maintenance','FeedMaint.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Generic Program Editor','GenericProgramEditor.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Generic Program Loader','GenericProgramLoader.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Global Performance Monitor','GlobalPerformanceMonitor.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Incoming Material Registration','IncomingMaterialRegistration.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run KIC Data Monitor','KICDataMonitor.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Kitting Optimization','FeederSetup.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Kitting Station','FeederSetup.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Light Tower Configuration','LightTowerConfiguration.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Line Status Selector','LineStatusSelector.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Low Level Warning','LowLevelWarning.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Manual Assembly','Shopfloor Manager.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Configuration','MaterialConfiguration.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Attribute Config','MaterialManageAttributeConfig.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management ChangeOver','MaterialManageChangeOver.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Configuration','MaterialManageConfigAndScheduler.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Picklist Searcher','MaterialManagePickListSearcher.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Pull','MaterialPull.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Scheduler','MaterialManageConfigAndScheduler.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Searcher','MaterialManageSearcher.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Transfer Station','MaterialManageVerifier.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Trolley Editor','TrolleyEditor.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Trolley Editor Server','TrolleyEditorModelServer.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Verifier','MaterialManageVerifier.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Material Management Verifier server','MaterialManageVerifierServer.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Mobile Pinger','WindowsPing.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Mobile Test Framework','ServiceView.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run MSD Configuration','MsdConfiguration.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Offline Verifier','Shopfloor Manager.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Pallet Board Scanner','PalletBoardScanner.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Panel Block Scanner','PanelBoardScanner.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Part Thresholds','PartThresholds.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Partial Verification Configuration','PartialVerificationConfiguration.exe','Production', 'Read');
	--CALL csiRBACCreateExternalPermission('Run PCB Post Trace Configuration','PostTraceConfig.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run PCB Reader','PcbReader.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run PCB Scanner Report','PcbScannerReport.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run PCB to Program','PcbToProgram.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run PDA Ping','PdaPing.exe','Production', 'Read');
	--CALL csiRBACCreateExternalPermission('Run Process Preparation',NULL,'Process Preparation', 'Read');
	CALL csiRBACCreateExternalPermission('Run Program Variant','ProgramVariant.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Repair Station','RepairStation.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Report Generator','TxReport.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Screen Printer','ScreenPrinter.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Serial Port Test','SerialPortTest.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Shopfloor Manager','Shopfloor Manager.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Split','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Splice','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Change','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Detach','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Move','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice View','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Multi Split','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Split and Splice Empty','MMSplitAndSplice.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Status Configuration','ConfigAppl.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run Stencil Cleaner','StencilCleaner.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run User Administration','userAdmin.exe','Production', 'Read');
	CALL csiRBACCreateExternalPermission('Run vManage Config','vManageConfig.exe','Production', 'Read');
	--CALL csiRBACCreateExternalPermission('Settings Editor Admininstrator',NULL,'Quality Management', 'Create');
	CALL csiRBACCreateExternalPermission('Show Admin Business Category','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show AQL',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Assembly','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Attachements','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Calendar','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Causer Operation',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Customer','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Database Editor',NULL,'Quality Management', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Defect And Repair',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Equipment','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Factory Business Category','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Group','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Label Definition',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Operation Type','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Order Business Category','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Panel','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Parts','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Plan Business Category','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Product Business Category','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Production Limit And Notification',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Route','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Show Skill','WorkSpace','Foundation', 'Read');
	--CALL csiRBACCreateExternalPermission('Show Symptom Template And Failure History',NULL,'Quality Management', 'Read');
	CALL csiRBACCreateExternalPermission('Show Work Order','WorkSpace','Foundation', 'Read');
	CALL csiRBACCreateExternalPermission('Unlock Machine','Portal','VIoT', 'Create');
	CALL csiRBACCreateExternalPermission('IPL Configuration','Workspace','SRC', 'Modify');
	--CALL csiRBACCreateExternalPermission('vQM Administrator',NULL,'Quality Management', 'Create');
	--CALL csiRBACCreateExternalPermission('Work In Manual Data Entry Mode',NULL,'Quality Management', 'Create');

	RAISE NOTICE 'Complete.';
	
END $$;

do $$ 
begin	
 CALL rbacPopulateExternalPermissionData();
 DROP PROCEDURE IF EXISTS rbacPopulateExternalPermissionData;
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiUpdateResourceStatusCodes')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiUpdateResourceStatusCodes;
 	END IF;
END $$;
CREATE PROCEDURE csiUpdateResourceStatusCodes()
LANGUAGE plpgsql
AS $$
DECLARE 
	vStatusId INTEGER;
	vResourceStatusCodeId CHAR(16);
BEGIN
	
	SELECT COALESCE(MAX(StatusId), 12) INTO vStatusId FROM ResourceStatusCode;

	IF vStatusId < 12 THEN
		vStatusId := 12;
	END IF;
	
	FOR vResourceStatusCodeId IN
        SELECT ResourceStatusCodeId
        FROM ResourceStatusCode
        WHERE (StatusId IS NULL OR StatusId = 0)
        AND ResourceStatusCodeName NOT IN ('PCBFinish', 'Working', 'PowerON', 'ErrorStop', 'ChangeStop', 'WaitStart', 'WaitReset', 'WaitComp', 'WaitBoard', 'WaitNext', 'PowerOff')
    LOOP
        vStatusId := vStatusId + 1;
		
		UPDATE ResourceStatusCode
        SET StatusId = vStatusId
        WHERE ResourceStatusCodeId = vResourceStatusCodeId;        
        
    END LOOP;

END $$;

do $$ 
begin	
 CALL csiUpdateResourceStatusCodes();
 DROP PROCEDURE IF EXISTS csiUpdateResourceStatusCodes;
end $$;

--------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRemoveExternalPermission
-- DESCR: Remove external permissions that are no longer needed
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiRemoveExternalPermission')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiRemoveExternalPermission;
 	END IF;
END $$;
CREATE PROCEDURE csiRemoveExternalPermission (
	pexternalPermissionName VARCHAR(255))
LANGUAGE plpgsql
AS $$
BEGIN	
	
	IF EXISTS (SELECT * FROM ExternalPermission where externalpermissionName=pexternalPermissionName) THEN
	BEGIN
		RAISE NOTICE 'Deleting External Permission: %', pexternalPermissionName;

		DELETE FROM RolePermission WHERE PermissionType = 240 AND RolePermissionName=pexternalPermissionName;
		DELETE FROM ExternalPermission where externalpermissionName=pexternalPermissionName;
	END;
	END IF;

END $$;

do $$ 
begin	
 CALL csiRemoveExternalPermission('IPLSettingsReadOnly');
 DROP PROCEDURE IF EXISTS csiRemoveExternalPermission;
end $$;