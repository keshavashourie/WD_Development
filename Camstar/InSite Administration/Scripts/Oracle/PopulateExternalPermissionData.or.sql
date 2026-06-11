--------------------------------------------------------------------------------
-- SCRIPT:PopulateExternalPermissionData.or.sql
-- DESCR: Creates stored procedures used to create External Permissions, etc.
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2023  
--
-- Change History:
-- Name            	Date        Action
-- --------------   ----------  ----------------
--	Winfred Nah		12/17/2021	Added all External Permission Items for External Permission Modeling page.
--  Matt Nolte		11/03/2022	Added more "Split and Splice" permissions
--  Gene Taylor		11/03/2022	Added stored proc named csiRemoveExternalPermission and exec a call to it.
--

--  Copyright Siemens 2023    
 
--------------------------------------------------------------------------------
-- PROCEDURE: csiRBACCreateExternalPermission
-- DESCR: Creates a External Permission Items
-- Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiRBACCreateExternalPermission(pExternalPermissionName IN VARCHAR2, pSRCApplication IN VARCHAR2, pSRCModule IN VARCHAR2, pSRCLevel in VARCHAR2)
AS
    vIID VARCHAR2(16);
	n int;
BEGIN
	n := 0;
	begin
	  select count(*) into n
	  from ExternalPermission
	  where ExternalPermissionName = pExternalPermissionName;
	  if n = 0 then
		csiPRDGetNextInstanceId(8872,vIID);
		INSERT INTO ExternalPermission(CDOTypeId,ChangeCount,ExternalPermissionId,ExternalPermissionName,IsFrozen,SRCApplication,SRCModule,SRCLevel,Required)
			VALUES(8872,1,vIID,pExternalPermissionName,0,pSRCApplication,pSRCModule,pSRCLevel,1);
	  end if;
	end;

END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateExternalPermissionData
-- DESCR: Helper function to create External Permission record
--
-- Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE rbacPopulateExternalPermissionData
AS
    
BEGIN
    -- External Permission Items
	csiRBACCreateExternalPermission ('Administrative',NULL,'Valor','Create');
	--csiRBACCreateExternalPermission ('Allow Editing Assembly Properties',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Ignoring Symptom',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow IPASS In vQM',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Scanning Work Order Numbers',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Setting Equipment And Assembly Properties',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Trashing A Board',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Working With Assembly Registration',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Allow Working With vQM',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Admin Business Category','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create AQL',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Assembly','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Attachements','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Calendar','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create Causer Operation',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Customer','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create Database Editor',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Create Defect And Repair',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Equipment','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Factory Business Category','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Group','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create Label Definition',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Operation Type','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Order Business Category','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Panel','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Parts','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Plan Business Category','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Product Business Category','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create Production Limit And Notification',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Route','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Create Skill','WorkSpace','Foundation','Create');
	--csiRBACCreateExternalPermission ('Create Symptom Template And Edit Failure History',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Create Work Order','WorkSpace','Foundation','Create');
	csiRBACCreateExternalPermission ('Disable System',NULL,'Production','Create');
	csiRBACCreateExternalPermission ('Force Release Machine','Portal','VIoT','Create');
	--csiRBACCreateExternalPermission ('Modify AQL',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Assembly','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Attachements','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Calendar','WorkSpace','Foundation','Modify');
	--csiRBACCreateExternalPermission ('Modify Causer Operation',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Customer','WorkSpace','Foundation','Modify');
	--csiRBACCreateExternalPermission ('Modify Database Editor',NULL,'Quality Management','Modify');
	--csiRBACCreateExternalPermission ('Modify Defect And Repair',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Equipment','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Group','WorkSpace','Foundation','Modify');
	--csiRBACCreateExternalPermission ('Modify Label Definition',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Operation Type','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Panel','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Parts','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify PCB Buffer','Modify PCB Buffer','Production','Create');
	--csiRBACCreateExternalPermission ('Modify Production Limit And Notification',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Route','WorkSpace','Foundation','Modify');
	csiRBACCreateExternalPermission ('Modify Skill','WorkSpace','Foundation','Modify');
	--csiRBACCreateExternalPermission ('Modify Symptom Template And Failure History',NULL,'Quality Management','Modify');
	csiRBACCreateExternalPermission ('Modify Work Order','WorkSpace','Foundation','Modify');
	--csiRBACCreateExternalPermission ('Quality Management Administrator',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Run AOI Reader','AOIDataReader.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run AVL Configuration','AvlConfiguration.exe','Production','Read');
	--csiRBACCreateExternalPermission ('Run Camera Calibration Tool','CameraCalibrationTool.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Configure Barcodes','ConfigBarcodes.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Configure Blocked Reels','ConfigBlockedReels.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Create Database','CreateDatabase.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run DB Browser','DbBrowser.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run DB Connection Tool','DbConnectionTool.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run DB Exporter','TXDBExporter.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Dry Oven','DryStorage.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Dry Storage','DryStorage.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Error Code Update','ErrorListInstall.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Feeder Maintenance','FeedMaint.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Generic Program Editor','GenericProgramEditor.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Generic Program Loader','GenericProgramLoader.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Global Performance Monitor','GlobalPerformanceMonitor.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Incoming Material Registration','IncomingMaterialRegistration.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run KIC Data Monitor','KICDataMonitor.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Kitting Optimization','FeederSetup.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Kitting Station','FeederSetup.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Light Tower Configuration','LightTowerConfiguration.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Line Status Selector','LineStatusSelector.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Low Level Warning','LowLevelWarning.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Manual Assembly','Shopfloor Manager.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Configuration','MaterialConfiguration.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Attribute Config','MaterialManageAttributeConfig.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management ChangeOver','MaterialManageChangeOver.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Configuration','MaterialManageConfigAndScheduler.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Picklist Searcher','MaterialManagePickListSearcher.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Pull','MaterialPull.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Scheduler','MaterialManageConfigAndScheduler.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Searcher','MaterialManageSearcher.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Transfer Station','MaterialManageVerifier.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Trolley Editor','TrolleyEditor.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Trolley Editor Server','TrolleyEditorModelServer.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Verifier','MaterialManageVerifier.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Material Management Verifier server','MaterialManageVerifierServer.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Mobile Pinger','WindowsPing.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Mobile Test Framework','ServiceView.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run MSD Configuration','MsdConfiguration.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Offline Verifier','Shopfloor Manager.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Pallet Board Scanner','PalletBoardScanner.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Panel Block Scanner','PanelBoardScanner.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Part Thresholds','PartThresholds.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Partial Verification Configuration','PartialVerificationConfiguration.exe','Production','Read');
	--csiRBACCreateExternalPermission ('Run PCB Post Trace Configuration','PostTraceConfig.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run PCB Reader','PcbReader.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run PCB Scanner Report','PcbScannerReport.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run PCB to Program','PcbToProgram.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run PDA Ping','PdaPing.exe','Production','Read');
	--csiRBACCreateExternalPermission ('Run Process Preparation',NULL,'Process Preparation','Read');
	csiRBACCreateExternalPermission ('Run Program Variant','ProgramVariant.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Repair Station','RepairStation.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Report Generator','TxReport.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Screen Printer','ScreenPrinter.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Serial Port Test','SerialPortTest.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Shopfloor Manager','Shopfloor Manager.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Split','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Splice','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Change','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Detach','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Move','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice View','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Multi Split','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Split and Splice Empty','MMSplitAndSplice.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Status Configuration','ConfigAppl.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run Stencil Cleaner','StencilCleaner.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run User Administration','userAdmin.exe','Production','Read');
	csiRBACCreateExternalPermission ('Run vManage Config','vManageConfig.exe','Production','Read');
	--csiRBACCreateExternalPermission ('Settings Editor Admininstrator',NULL,'Quality Management','Create');
	csiRBACCreateExternalPermission ('Show Admin Business Category','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show AQL',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Assembly','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Attachements','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Calendar','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show Causer Operation',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Customer','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show Database Editor',NULL,'Quality Management','Read');
	--csiRBACCreateExternalPermission ('Show Defect And Repair',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Equipment','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Factory Business Category','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Group','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show Label Definition',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Operation Type','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Order Business Category','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Panel','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Parts','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Plan Business Category','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Product Business Category','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show Production Limit And Notification',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Route','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Show Skill','WorkSpace','Foundation','Read');
	--csiRBACCreateExternalPermission ('Show Symptom Template And Failure History',NULL,'Quality Management','Read');
	csiRBACCreateExternalPermission ('Show Work Order','WorkSpace','Foundation','Read');
	csiRBACCreateExternalPermission ('Unlock Machine','Portal','VIoT','Create');
	csiRBACCreateExternalPermission ('IPL Configuration','Workspace','SRC','Modify');
	--csiRBACCreateExternalPermission ('vQM Administrator',NULL,'Quality Management','Create');
	--csiRBACCreateExternalPermission ('Work In Manual Data Entry Mode',NULL,'Quality Management','Create');

END;
/
BEGIN
rbacPopulateExternalPermissionData;
END;
/

CREATE OR REPLACE PROCEDURE csiUpdateResourceStatusCodes
AS
  pStatusId	NUMBER;
  
  Cursor curResourceStatusCode IS SELECT ResourceStatusCodeId FROM ResourceStatusCode WHERE (StatusId IS NULL or StatusId=0) AND (
	ResourceStatusCodeName != 'PCBFinish' AND ResourceStatusCodeName != 'Working' AND ResourceStatusCodeName != 'PowerON' AND
	ResourceStatusCodeName != 'ErrorStop' AND ResourceStatusCodeName != 'ChangeStop' AND ResourceStatusCodeName != 'WaitStart' AND
	ResourceStatusCodeName != 'WaitReset' AND ResourceStatusCodeName != 'WaitComp' AND ResourceStatusCodeName != 'WaitBoard' AND
	ResourceStatusCodeName != 'WaitNext' AND ResourceStatusCodeName != 'PowerOff');

BEGIN
	SELECT NVL(MAX(StatusId),11)+1 INTO pStatusId FROM ResourceStatusCode;

  FOR cRec IN curResourceStatusCode LOOP
    BEGIN
      UPDATE ResourceStatusCode SET StatusId = pStatusId WHERE ResourceStatusCodeId = cRec.ResourceStatusCodeId;
      			
      pStatusId := pStatusId + 1;
    END;      
  END Loop;
  COMMIT;
END;
/

BEGIN
csiUpdateResourceStatusCodes;
COMMIT;
END;
/

BEGIN    
EXECUTE IMMEDIATE 'DROP PROCEDURE csiUpdateResourceStatusCodes';
END;
/

--------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRemoveExternalPermission
-- DESCR: Remove external permissions that are no longer needed
CREATE OR REPLACE PROCEDURE csiRemoveExternalPermission (pExternalPermissionName IN VARCHAR2)
AS
	n int;
BEGIN
	n := 0;
	
    select count(*) into n
    from ExternalPermission
    where ExternalPermissionName = pExternalPermissionName;
        
    if n > 0 then            
        DELETE FROM RolePermission WHERE PermissionType = 240 AND RolePermissionName=pExternalPermissionName;
        DELETE FROM ExternalPermission where externalpermissionName=pExternalPermissionName;
    end if;

END;
/

BEGIN
csiRemoveExternalPermission ('IPLSettingsReadOnly');
END;
/

BEGIN    
EXECUTE IMMEDIATE 'DROP PROCEDURE csiRemoveExternalPermission';
END;
/
