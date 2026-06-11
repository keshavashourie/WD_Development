-- Copyright Siemens 2025 --
DECLARE @isStatusModelUpdated AS bit ='false'
DECLARE @isStatusModelDetailUpdated AS bit ='false'
DECLARE @isStatusModelDetailToReasonUpdated AS bit ='false'
DECLARE @isResourceLayoutUpdated AS bit ='false'
DECLARE @isResourceLayoutDetailsUpdated AS bit ='false'

-- StatusModel Table
IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'scsStatusModel'))
	BEGIN
		BEGIN
			--Finds the existing data from A_StatusModel
			IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_StatusModel') AND (SELECT COUNT(*) FROM A_StatusModel WHERE StatusModelId LIKE '48806e%') > 0)
			BEGIN
				-- Clone the scsStatusModel table first
				SELECT * INTO tempTable FROM scsStatusModel WHERE StatusModelId LIKE '48806e%'
				-- Deletes the newly added values
				DELETE FROM scsStatusModel WHERE StatusModelId LIKE '48806e%'
				-- Include the previous records using clone technique
				INSERT INTO scsStatusModel(CDOTypeId, ChangeCount, ChangeHistoryId, Description, FilterTags, IconId, isFrozen, Notes, SetupAccessId, StatusModelId, StatusModelName) SELECT CDOTypeId, ChangeCount, ChangeHistoryId, Description, FilterTags, IconId, isFrozen, Notes, SetupAccessId, StatusModelId, StatusModelName FROM A_StatusModel WHERE StatusModelId LIKE '48806e%'
				-- DELETE the previous records on A_StatusModel
				DELETE FROM A_StatusModel WHERE StatusModelId LIKE '48806e%'
				-- INSERT current record to the updated table
				INSERT INTO scsStatusModel SELECT * FROM tempTable
				-- Drop the temp table
				DROP TABLE tempTable
				-- Drop Index
				DROP INDEX scsStatusModel.A_StatusModel1;
			END;
		END;
	END;
	
-- StatusModelDetail Table
IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'scsStatusModelDetail'))
	BEGIN
		BEGIN
			--Finds the existing data from A_StatusModelDetail
			IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_StatusModelDetail') AND (SELECT COUNT(*) FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE '48822e%') > 0)
			BEGIN
				-- Clone the scsStatusModelDetail table first
				SELECT * INTO tempTable FROM scsStatusModelDetail WHERE StatusModelDetailId LIKE '48822e%'
				-- Deletes the newly added values
				DELETE FROM scsStatusModelDetail WHERE StatusModelDetailId LIKE '48822e%'
				-- Include the previous records using clone technique
				INSERT INTO scsStatusModelDetail(AutoSetPrecondition, AutoSetPreconditionForChild, AutoSetPreconditionForParent, AutoSetReasonId, CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, StatusId, StatusModelDetailId, StatusModelId, StatusSequence, ToStatusId) SELECT AutoSetPrecondition, AutoSetPreconditionForChild, AutoSetPreconditionForParent, AutoSetReasonId, CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, StatusId, StatusModelDetailId, StatusModelId, StatusSequence, ToStatusId FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE '48822e%'
				-- DELETE the previous records on A_StatusModelDetail
				DELETE FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE '48822e%'
				-- INSERT current record to the updated table
				INSERT INTO scsStatusModelDetail SELECT * FROM tempTable
				-- Drop the temp table
				DROP TABLE tempTable
				-- Drop Index
				DROP INDEX scsStatusModelDetail.A_StatusModelDetail1;
				DROP INDEX scsStatusModelDetail.A_StatusModelDetail2;
			END;
		END;
	END;
	
-- StatusModelDetailToReason Table
IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'scsStatusModelDetailToReason'))
	BEGIN
		BEGIN
			--Finds the existing data from A_StatusModelDetailToReason
			IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_StatusModelDetailToReason') AND (SELECT COUNT(*) FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE '48822f%') > 0)
			BEGIN
				-- Clone the scsStatusModelDetailToReason table first
				SELECT * INTO tempTable FROM scsStatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE '48822f%'
				-- Deletes the newly added values
				DELETE FROM scsStatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE '48822f%'
				-- Include the previous records using clone technique
				INSERT INTO scsStatusModelDetailToReason(AutoJobCreateJobModelId, AutoJobCreatePrecondition, CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ReasonId, StatusModelDetailId, StatusModelDetailToReasonId) SELECT AutoJobCreateJobModelId, AutoJobCreatePrecondition, CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ReasonId, StatusModelDetailId, StatusModelDetailToReasonId FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE '48822f%'
				-- DELETE the previous records on A_StatusModelDetailToReason
				DELETE FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE '48822f%'
				-- INSERT current record to the updated table
				INSERT INTO scsStatusModelDetailToReason SELECT * FROM tempTable
				-- Drop the temp table
				DROP TABLE tempTable
				-- Drop Index
				DROP INDEX scsStatusModelDetailToReason.A_StatusModelDetailToReason1;
			END;
		END;
	END;

-- ResourceLayout Table
IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ResourceLayout'))
	BEGIN
		BEGIN
			--Finds the existing data from A_ResourceLayout
			IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_ResourceLayout') AND (SELECT COUNT(*) FROM A_ResourceLayout WHERE ResourceLayoutId LIKE '488067%') > 0)
			BEGIN
				-- Clone the ResourceLayout table first
				SELECT * INTO tempTable FROM ResourceLayout WHERE ResourceLayoutId LIKE '488067%'
				-- Deletes the newly added values
				DELETE FROM ResourceLayout WHERE ResourceLayoutId LIKE '488067%'
				-- Include the previous records using clone technique
				INSERT INTO ResourceLayout(BackgroundFileName, CDOTypeId, ChangeCount, ChangeHistoryId, Description, FilterTags, IconId, IsFrozen, LayoutHeight, LayoutWidth, Notes, ResourceLayoutId, ResourceLayoutName, SetupAccessId) SELECT BackgroundFileName, CDOTypeId, ChangeCount, ChangeHistoryId, Description, FilterTags, IconId, IsFrozen, LayoutHeight, LayoutWidth, Notes, ResourceLayoutId, ResourceLayoutName, SetupAccessId FROM A_ResourceLayout WHERE ResourceLayoutId LIKE '488067%'
				-- DELETE the previous records on A_ResourceLayout
				DELETE FROM A_ResourceLayout WHERE ResourceLayoutId LIKE '488067%'
				-- INSERT current record to the updated table
				INSERT INTO ResourceLayout SELECT * FROM tempTable
				-- Drop the temp table
				DROP TABLE tempTable
				-- Drop Index
				DROP INDEX ResourceLayout.A_ResourceLayout1;
			END;
		END;
	END;
	
-- ResourceLayoutDetails Table
IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ResourceLayoutDetails'))
	BEGIN
		BEGIN
			--Finds the existing data from A_ResourceLayoutDetails
			IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_ResourceLayoutDetails') AND (SELECT COUNT(*) FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE '488223%') > 0)
			BEGIN
				-- Clone the ResourceLayout table first
				SELECT * INTO tempTable FROM ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE '488223%'
				-- Deletes the newly added values
				DELETE FROM ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE '488223%'
				-- Include the previous records using clone technique
				INSERT INTO ResourceLayoutDetails(CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ResourceId, ResourceLayoutDetailsId, ResourceLayoutId, XLocation, YLocation) SELECT CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ResourceId, ResourceLayoutDetailsId, ResourceLayoutId, XLocation, YLocation FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE '488223%'
				-- DELETE the previous records on A_ResourceLayoutDetails
				DELETE FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE '488223%'
				-- INSERT current record to the updated table
				INSERT INTO ResourceLayoutDetails SELECT * FROM tempTable
				-- Drop the temp table
				DROP TABLE tempTable
				-- Drop Index
				DROP INDEX ResourceLayoutDetails.A_ResourceLayoutDetails1;
				DROP INDEX ResourceLayoutDetails.A_ResourceLayoutDetails2;
			END;
		END;
	END;
	

IF ((@isStatusModelUpdated <> 1) AND NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'scsStatusModel'))
	BEGIN
		EXEC sp_rename 'A_StatusModel', 'scsStatusModel';

		EXEC sp_rename N'scsStatusModel.A_StatusMode679477466', N'scsStatusMode679477466', N'INDEX';
		EXEC sp_rename N'scsStatusModel.A_StatusModel1', N'scsStatusModel1', N'INDEX';
	END;

IF ((@isStatusModelDetailUpdated <> 1) AND NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'scsStatusModelDetail'))
	BEGIN
		EXEC sp_rename 'A_StatusModelDetail', 'scsStatusModelDetail';

		EXEC sp_rename N'scsStatusModelDetail.A_StatusMode679477467', N'scsStatusMode679477467', N'INDEX';
		EXEC sp_rename N'scsStatusModelDetail.A_StatusModelDetail1', N'scsStatusModelDetail1', N'INDEX';
		EXEC sp_rename N'scsStatusModelDetail.A_StatusModelDetail2', N'scsStatusModelDetail2', N'INDEX';
	END;

IF ((@isStatusModelDetailToReasonUpdated <> 1) AND NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'A_StatusModelDetailToReason'))
	BEGIN
		EXEC sp_rename 'A_StatusModelDetailToReason', 'scsStatusModelDetailToReason';

		EXEC sp_rename N'scsStatusModelDetailToReason.A_StatusMode679477468', N'scsStatusMode679477468', N'INDEX';
		EXEC sp_rename N'scsStatusModelDetailToReason.A_StatusModelDetailToReason1', N'scsStatusModelDetailToReason1', N'INDEX';
	END;
	
IF ((@isResourceLayoutUpdated <> 1) AND NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ResourceLayout'))
	BEGIN
		EXEC sp_rename 'A_ResourceLayout', 'ResourceLayout';

		EXEC sp_rename N'ResourceLayout.A_ResourceLa679477421', N'ResourceLayo679477421', N'INDEX';
		EXEC sp_rename N'ResourceLayout.A_ResourceLayout1', N'ResourceLayout1', N'INDEX';
	END;
	
IF ((@isResourceLayoutDetailsUpdated <> 1) AND NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ResourceLayoutDetails'))
	BEGIN
		EXEC sp_rename 'A_ResourceLayoutDetails', 'ResourceLayoutDetails';

		EXEC sp_rename N'ResourceLayoutDetails.A_ResourceLa679477423', N'ResourceLayo679477423', N'INDEX';
		EXEC sp_rename N'ResourceLayoutDetails.A_ResourceLayoutDetails1', N'ResourceLayoutDetails1', N'INDEX';
		EXEC sp_rename N'ResourceLayoutDetails.A_ResourceLayoutDetails2', N'ResourceLayoutDetails2', N'INDEX';
	END;