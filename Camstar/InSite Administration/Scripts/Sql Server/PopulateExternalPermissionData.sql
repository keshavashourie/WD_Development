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
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiRBACCreateExternalPermission' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiRBACCreateExternalPermission
GO
CREATE PROCEDURE csiRBACCreateExternalPermission(@ExternalPermissionName NVARCHAR(255), @SRCApplication NVARCHAR(255), @SRCModule NVARCHAR(255), @SRCLevel NVARCHAR(30))
AS
    DECLARE @IID VARCHAR(16)
BEGIN
    SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM ExternalPermission WHERE ExternalPermissionName = @ExternalPermissionName)
	BEGIN
		EXEC csiPRDGetNextInstanceId 8872,@IID OUTPUT
		set @ExternalPermissionName=LTRIM(RTRIM(@ExternalPermissionName))
    
		INSERT INTO ExternalPermission(CDOTypeId,ChangeCount,ExternalPermissionId,ExternalPermissionName,IsFrozen,SRCApplication,SRCModule,SRCLevel,Required)
			VALUES(8872,1,@IID,@ExternalPermissionName,0,@SRCApplication,@SRCModule,@SRCLevel, 1);
	END

END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: rbacPopulateExternalPermissionData
-- DESCR: Helper function to create External Permission record
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'rbacPopulateExternalPermissionData' 
	   AND 	  type = 'P')
    DROP PROCEDURE rbacPopulateExternalPermissionData
GO
CREATE PROCEDURE rbacPopulateExternalPermissionData
AS
    
BEGIN
    SET NOCOUNT ON;

	PRINT('Creating "External Permission" Items...'); 
	EXEC csiRBACCreateExternalPermission 'Administrative',NULL,'Valor', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Editing Assembly Properties',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Ignoring Symptom',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow IPASS In vQM',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Scanning Work Order Numbers',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Setting Equipment And Assembly Properties',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Trashing A Board',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Working With Assembly Registration',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Allow Working With vQM',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Admin Business Category','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create AQL',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Assembly','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Attachements','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Calendar','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Causer Operation',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Customer','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Database Editor',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Defect And Repair',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Equipment','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Factory Business Category','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Group','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Label Definition',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Operation Type','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Order Business Category','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Panel','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Parts','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Plan Business Category','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Product Business Category','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Production Limit And Notification',NULL,'Quality Management'
	EXEC csiRBACCreateExternalPermission 'Create Route','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Create Skill','WorkSpace','Foundation', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Create Symptom Template And Edit Failure History',NULL,'Quality Management'
	EXEC csiRBACCreateExternalPermission 'Create Work Order','WorkSpace','Foundation', 'Create'
	EXEC csiRBACCreateExternalPermission 'Disable System',NULL,'Production', 'Create'
	EXEC csiRBACCreateExternalPermission 'Force Release Machine','Portal','VIoT', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Modify AQL',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Assembly','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Attachements','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Calendar','WorkSpace','Foundation', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Modify Causer Operation',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Customer','WorkSpace','Foundation', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Modify Database Editor',NULL,'Quality Management', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Modify Defect And Repair',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Equipment','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Group','WorkSpace','Foundation', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Modify Label Definition',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Operation Type','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Panel','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Parts','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify PCB Buffer','Modify PCB Buffer','Production', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Modify Production Limit And Notification',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Route','WorkSpace','Foundation', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Skill','WorkSpace','Foundation', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Modify Symptom Template And Failure History',NULL,'Quality Management', 'Modify'
	EXEC csiRBACCreateExternalPermission 'Modify Work Order','WorkSpace','Foundation', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'Quality Management Administrator',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Run AOI Reader','AOIDataReader.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run AVL Configuration','AvlConfiguration.exe','Production', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Run Camera Calibration Tool','CameraCalibrationTool.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Configure Barcodes','ConfigBarcodes.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Configure Blocked Reels','ConfigBlockedReels.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Create Database','CreateDatabase.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run DB Browser','DbBrowser.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run DB Connection Tool','DbConnectionTool.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run DB Exporter','TXDBExporter.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Dry Oven','DryStorage.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Dry Storage','DryStorage.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Error Code Update','ErrorListInstall.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Feeder Maintenance','FeedMaint.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Generic Program Editor','GenericProgramEditor.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Generic Program Loader','GenericProgramLoader.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Global Performance Monitor','GlobalPerformanceMonitor.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Incoming Material Registration','IncomingMaterialRegistration.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run KIC Data Monitor','KICDataMonitor.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Kitting Optimization','FeederSetup.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Kitting Station','FeederSetup.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Light Tower Configuration','LightTowerConfiguration.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Line Status Selector','LineStatusSelector.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Low Level Warning','LowLevelWarning.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Manual Assembly','Shopfloor Manager.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Configuration','MaterialConfiguration.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Attribute Config','MaterialManageAttributeConfig.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management ChangeOver','MaterialManageChangeOver.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Configuration','MaterialManageConfigAndScheduler.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Picklist Searcher','MaterialManagePickListSearcher.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Pull','MaterialPull.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Scheduler','MaterialManageConfigAndScheduler.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Searcher','MaterialManageSearcher.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Transfer Station','MaterialManageVerifier.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Trolley Editor','TrolleyEditor.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Trolley Editor Server','TrolleyEditorModelServer.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Verifier','MaterialManageVerifier.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Material Management Verifier server','MaterialManageVerifierServer.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Mobile Pinger','WindowsPing.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Mobile Test Framework','ServiceView.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run MSD Configuration','MsdConfiguration.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Offline Verifier','Shopfloor Manager.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Pallet Board Scanner','PalletBoardScanner.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Panel Block Scanner','PanelBoardScanner.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Part Thresholds','PartThresholds.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Partial Verification Configuration','PartialVerificationConfiguration.exe','Production', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Run PCB Post Trace Configuration','PostTraceConfig.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run PCB Reader','PcbReader.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run PCB Scanner Report','PcbScannerReport.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run PCB to Program','PcbToProgram.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run PDA Ping','PdaPing.exe','Production', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Run Process Preparation',NULL,'Process Preparation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Program Variant','ProgramVariant.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Repair Station','RepairStation.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Report Generator','TxReport.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Screen Printer','ScreenPrinter.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Serial Port Test','SerialPortTest.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Shopfloor Manager','Shopfloor Manager.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Split','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Splice','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Change','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Detach','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Move','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice View','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Multi Split','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Split and Splice Empty','MMSplitAndSplice.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Status Configuration','ConfigAppl.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run Stencil Cleaner','StencilCleaner.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run User Administration','userAdmin.exe','Production', 'Read'
	EXEC csiRBACCreateExternalPermission 'Run vManage Config','vManageConfig.exe','Production', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Settings Editor Admininstrator',NULL,'Quality Management', 'Create'
	EXEC csiRBACCreateExternalPermission 'Show Admin Business Category','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show AQL',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Assembly','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Attachements','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Calendar','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Causer Operation',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Customer','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Database Editor',NULL,'Quality Management', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Defect And Repair',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Equipment','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Factory Business Category','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Group','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Label Definition',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Operation Type','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Order Business Category','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Panel','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Parts','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Plan Business Category','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Product Business Category','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Production Limit And Notification',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Route','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Skill','WorkSpace','Foundation', 'Read'
	--EXEC csiRBACCreateExternalPermission 'Show Symptom Template And Failure History',NULL,'Quality Management', 'Read'
	EXEC csiRBACCreateExternalPermission 'Show Work Order','WorkSpace','Foundation', 'Read'
	EXEC csiRBACCreateExternalPermission 'Unlock Machine','Portal','VIoT', 'Create'
	EXEC csiRBACCreateExternalPermission 'IPL Configuration','Workspace','SRC', 'Modify'
	--EXEC csiRBACCreateExternalPermission 'vQM Administrator',NULL,'Quality Management', 'Create'
	--EXEC csiRBACCreateExternalPermission 'Work In Manual Data Entry Mode',NULL,'Quality Management', 'Create'

	PRINT('Complete.');
END
GO
EXEC rbacPopulateExternalPermissionData
GO
DROP PROCEDURE rbacPopulateExternalPermissionData
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiUpdateResourceStatusCodes' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiUpdateResourceStatusCodes
GO

CREATE PROCEDURE csiUpdateResourceStatusCodes 
AS
	DECLARE @StatusId INT
	DECLARE @ResourceStatusCodeId CHAR(16)
BEGIN
	SELECT @StatusId = ISNULL(MAX(StatusId),12) FROM ResourceStatusCode

	IF @StatusId < 12
		SET @StatusId = 12
	
	DECLARE curResourceStatusCode CURSOR LOCAL FORWARD_ONLY STATIC READ_ONLY
	FOR SELECT ResourceStatusCodeId FROM ResourceStatusCode WHERE (StatusId IS NULL or StatusId=0) AND (
	ResourceStatusCodeName != 'PCBFinish' AND ResourceStatusCodeName != 'Working' AND ResourceStatusCodeName != 'PowerON' AND
	ResourceStatusCodeName != 'ErrorStop' AND ResourceStatusCodeName != 'ChangeStop' AND ResourceStatusCodeName != 'WaitStart' AND
	ResourceStatusCodeName != 'WaitReset' AND ResourceStatusCodeName != 'WaitComp' AND ResourceStatusCodeName != 'WaitBoard' AND
	ResourceStatusCodeName != 'WaitNext' AND ResourceStatusCodeName != 'PowerOff')
	
	OPEN curResourceStatusCode
	IF @@CURSOR_ROWS > 0 
	BEGIN
		FETCH NEXT FROM curResourceStatusCode INTO @ResourceStatusCodeId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			UPDATE ResourceStatusCode SET StatusId = @StatusId WHERE ResourceStatusCodeId = @ResourceStatusCodeId
			SET @StatusId = @StatusId + 1
			FETCH NEXT FROM curResourceStatusCode INTO @ResourceStatusCodeId
		END
	END
	CLOSE curResourceStatusCode
	DEALLOCATE curResourceStatusCode
END
GO

EXEC csiUpdateResourceStatusCodes;
GO

--	Drops the stored procedure after it is executed
IF OBJECT_ID(N'csiUpdateResourceStatusCodes',N'P') IS NOT NULL
	DROP PROCEDURE csiUpdateResourceStatusCodes;
GO

--------------------------------------------------------------------------------------------------
-- PROCEDURE: csiRemoveExternalPermission
-- DESCR: Remove external permissions that are no longer needed
CREATE OR ALTER PROCEDURE csiRemoveExternalPermission (@externalPermissionName NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;
	
	IF EXISTS (SELECT * FROM ExternalPermission where externalpermissionName=@externalPermissionName)
	BEGIN
		PRINT('Deleting External Permission: ' + @externalPermissionName);

		DELETE FROM RolePermission WHERE PermissionType = 240 AND RolePermissionName=@externalPermissionName
		DELETE FROM ExternalPermission where externalpermissionName=@externalPermissionName
	END

END
GO

EXEC csiRemoveExternalPermission 'IPLSettingsReadOnly'
GO

IF OBJECT_ID(N'csiRemoveExternalPermission',N'P') IS NOT NULL
	DROP PROCEDURE csiRemoveExternalPermission;
GO
