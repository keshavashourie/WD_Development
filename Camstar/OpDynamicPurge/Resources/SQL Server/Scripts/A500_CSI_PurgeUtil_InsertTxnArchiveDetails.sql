/**
This script is used to insert the Transaction and Archive Database Name & Schema respectively into the Configuration DB
*/
USE DPT;

BEGIN
DECLARE @TransactionDBName NVARCHAR(1000);
DECLARE @TransactionDBSchema NVARCHAR(1000);
DECLARE @ArchiveDBName NVARCHAR(1000);
DECLARE @ArchiveDBSchema NVARCHAR(1000);
DECLARE @DesignerMetadataDatabase NVARCHAR(1000);
DECLARE @SiteInfoDatabase NVARCHAR(1000);

SET @TransactionDBName = '$(TransactionDBName)';
SET @TransactionDBSchema = '$(TransactionDBSchema)';
SET @ArchiveDBName = '$(ArchiveDBName)';
SET @ArchiveDBSchema = '$(ArchiveDBSchema)';
SET @DesignerMetadataDatabase = '$(DesignerMetadataDatabase)';
SET @SiteInfoDatabase = '$(SiteInfoDatabase)';

/** Check if values exist, if values already exist then replace them by updating */
/** TransactionDatabaseName **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'TransactionDatabaseName')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @TransactionDBName WHERE TNAME = 'TransactionDatabaseName';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('TransactionDatabaseName', @TransactionDBName);
	END
	
/** TransactionDatabaseSchema **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'TransactionDatabaseSchema')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @TransactionDBSchema WHERE TNAME = 'TransactionDatabaseSchema';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('TransactionDatabaseSchema',@TransactionDBSchema);
	END
	
/** TransactionDatabaseType **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'TransactionDatabaseType')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = 'SQLServer' WHERE TNAME = 'TransactionDatabaseType';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('TransactionDatabaseType', 'SQLServer');
	END

/** ArchiveDatabaseName **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'ArchiveDatabaseName')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @ArchiveDBName WHERE TNAME = 'ArchiveDatabaseName';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('ArchiveDatabaseName', @ArchiveDBName);
	END
	
/** ArchiveSchemaName **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'ArchiveSchemaName')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @ArchiveDBSchema WHERE TNAME = 'ArchiveSchemaName';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('ArchiveSchemaName', @ArchiveDBSchema);
	END

/** DesignerMetadataDatabase **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'DesignerMetadataDatabase')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @DesignerMetadataDatabase WHERE TNAME = 'DesignerMetadataDatabase';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('DesignerMetadataDatabase', @DesignerMetadataDatabase);
	END
	
/** SiteInfoDatabase **/
IF EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'SiteInfoDatabase')
	BEGIN
		UPDATE CSI_PURGEUTIL_CONFIG SET TVALUE = @SiteInfoDatabase WHERE TNAME = 'SiteInfoDatabase';
	END
ELSE
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('SiteInfoDatabase', @SiteInfoDatabase);
	END

	
/** Version **/
-- Implemented versioning in CSI_PURGEUTIL_INSTALLATION
-- If config version increase all setup will required saving again
IF NOT EXISTS(SELECT * FROM CSI_PURGEUTIL_CONFIG WHERE TNAME = 'Version')
	BEGIN
		INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('Version', '1');
	END


END