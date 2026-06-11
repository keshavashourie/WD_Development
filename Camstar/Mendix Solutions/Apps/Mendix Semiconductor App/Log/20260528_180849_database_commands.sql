ALTER DATABASE [Mendix] SET READ_COMMITTED_SNAPSHOT ON WITH NO_WAIT;
GO
ALTER DATABASE [Mendix] SET ALLOW_SNAPSHOT_ISOLATION ON;
GO
CREATE PROCEDURE [usp_nextsequencevalue]
@SeqName nvarchar(128)
AS
BEGIN
DECLARE @NewSeqVal bigint
SET NOCOUNT ON
UPDATE [mendixsystem$sequence]
SET @NewSeqVal = [current_value] = ISNULL([current_value],0) + 1
WHERE [name] = @SeqName
RETURN @NewSeqVal
END;
GO
CREATE TABLE [mendixsystem$entity] (
	[id] nvarchar(36) NOT NULL,
	[entity_name] nvarchar(511) NOT NULL,
	[table_name] nvarchar(255) NOT NULL,
	[superentity_id] nvarchar(255) NULL,
	[remote] bit NULL,
	[remote_primary_key] bit NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mendixsystem$entity_entity_name] ON [mendixsystem$entity] ([entity_name] ASC);
GO
CREATE TABLE [mendixsystem$attribute] (
	[id] nvarchar(36) NOT NULL,
	[entity_id] nvarchar(255) NOT NULL,
	[attribute_name] nvarchar(255) NOT NULL,
	[column_name] nvarchar(255) NOT NULL,
	[type] int NOT NULL,
	[length] int NULL,
	[default_value] nvarchar(max) NULL,
	[is_auto_number] bit NOT NULL,
	PRIMARY KEY([id]));
GO
CREATE TABLE [mendixsystem$index] (
	[id] nvarchar(36) NOT NULL,
	[table_id] nvarchar(36) NOT NULL,
	[index_name] nvarchar(255) NOT NULL,
	PRIMARY KEY([id]));
GO
CREATE TABLE [mendixsystem$index_column] (
	[index_id] nvarchar(36) NOT NULL,
	[column_id] nvarchar(36) NOT NULL,
	[sort_order] bit NOT NULL,
	[ordinal] int NOT NULL,
	PRIMARY KEY([index_id],[column_id]));
GO
CREATE TABLE [mendixsystem$association] (
	[id] nvarchar(36) NOT NULL,
	[association_name] nvarchar(511) NOT NULL,
	[table_name] nvarchar(255) NOT NULL,
	[parent_entity_id] nvarchar(36) NOT NULL,
	[child_entity_id] nvarchar(36) NOT NULL,
	[parent_column_name] nvarchar(255) NOT NULL,
	[child_column_name] nvarchar(255) NOT NULL,
	[pk_index_name] nvarchar(255) NULL,
	[index_name] nvarchar(255) NULL,
	[parent_fkc_name] nvarchar(255) NULL,
	[child_fkc_name] nvarchar(255) NULL,
	[parent_fkc_action] smallint NULL,
	[child_fkc_action] smallint NULL,
	[storage_format] smallint NULL,
	PRIMARY KEY([id]));
GO
CREATE TABLE [mendixsystem$version] (
	[versionnumber] nvarchar(255) NOT NULL,
	[lastsyncdate] datetime2(3) NOT NULL,
	[preanalysismigrationversionnumber] nvarchar(255) NOT NULL,
	[modelversionnumber] nvarchar(255) NULL,
	[sprintrprojectname] nvarchar(511) NULL,
	[mendixversion] nvarchar(255) NULL,
	PRIMARY KEY([versionnumber]));
GO
CREATE TABLE [mendixsystem$sequence] (
	[name] nvarchar(255) NOT NULL,
	[attribute_id] nvarchar(36) NOT NULL,
	[start_value] bigint NOT NULL,
	[current_value] bigint NULL,
	PRIMARY KEY([attribute_id]));
GO
CREATE INDEX [idx_mendixsystem$sequence_name] ON [mendixsystem$sequence] ([name] ASC);
GO
CREATE TABLE [mendixsystem$entityidentifier] (
	[id] nvarchar(36) NOT NULL,
	[short_id] smallint NULL,
	[object_sequence] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mendixsystem$entityidentifier_short_id] ON [mendixsystem$entityidentifier] ([short_id] ASC);
GO
CREATE TABLE [mendixsystem$unique_constraint] (
	[name] nvarchar(255) NOT NULL,
	[table_id] nvarchar(36) NOT NULL,
	[column_id] nvarchar(36) NOT NULL,
	PRIMARY KEY([name],[column_id]));
GO
CREATE TABLE [mendixsystem$remote_primary_key] (
	[id] nvarchar(36) NOT NULL,
	[entity_id] nvarchar(255) NOT NULL,
	[attribute_name] nvarchar(255) NOT NULL,
	[column_name] nvarchar(255) NOT NULL,
	[type] int NOT NULL,
	[length] int NULL,
	PRIMARY KEY([id]));
GO
CREATE TABLE [saml20$keystore] (
	[id] bigint NOT NULL,
	[lastchangedon] datetime2(3) NULL,
	[alias] nvarchar(200) NULL,
	[rebuildkeystore] bit NULL,
	[password] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('712b9e2b-202a-496a-896b-61440043e0b8', 'SAML20.KeyStore', 'saml20$keystore', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2202d53b-b3c2-4012-8a29-a9e43bf977cf', '712b9e2b-202a-496a-896b-61440043e0b8', 'LastChangedOn', 'lastchangedon', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('991faccb-13e8-4a57-a95d-883015da40a8', '712b9e2b-202a-496a-896b-61440043e0b8', 'Alias', 'alias', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0269930a-0e82-4dd8-b0da-220ab0e2af15', '712b9e2b-202a-496a-896b-61440043e0b8', 'RebuildKeyStore', 'rebuildkeystore', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('28b80670-7c43-44c7-9d30-23407128580a', '712b9e2b-202a-496a-896b-61440043e0b8', 'Password', 'password', 30, 200, '', 0);
GO
CREATE TABLE [system$offlinecreatedguids] (
	[id] bigint NOT NULL,
	[guid] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_system$offlinecreatedguids_guid] UNIQUE ([guid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('89cea6a8-a2df-4925-85e4-2b0c447e98c3', 'System.OfflineCreatedGuids', 'system$offlinecreatedguids', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7919a281-0b72-4dc4-87e1-fffaced4c8d1', '89cea6a8-a2df-4925-85e4-2b0c447e98c3', 'Guid', 'guid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('22289399-6f82-3eda-bbdf-15ddacf9f408', '89cea6a8-a2df-4925-85e4-2b0c447e98c3', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$offlinecreatedguids_guid', '89cea6a8-a2df-4925-85e4-2b0c447e98c3', '7919a281-0b72-4dc4-87e1-fffaced4c8d1');
GO
CREATE TABLE [saml20$attribute] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[nameformat] nvarchar(200) NULL,
	[friendlyname] nvarchar(200) NULL,
	[isrequired] bit NULL,
	[manuallycreated] bit NULL,
	[updated] bit NULL,
	[isincommonfederation] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1fe71b95-a95e-4ddd-a9f9-501182b13749', 'SAML20.Attribute', 'saml20$attribute', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d8597691-1a62-4237-9856-aeafb7cfa70e', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0b335872-4226-4121-aa3f-7b8136ffd85d', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'NameFormat', 'nameformat', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('879c2f1a-aa73-42b0-bf72-f409a8d64c4c', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'FriendlyName', 'friendlyname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a4093cc7-64af-47e9-a04f-62760243524c', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'isRequired', 'isrequired', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('000b18bf-7ed6-43c3-9a57-69000ad894e6', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'ManuallyCreated', 'manuallycreated', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5ca59903-774a-47da-b62a-5bc5d139bbdb', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'Updated', 'updated', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('25d50dbc-903b-4324-87f6-461218770eb9', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'IsInCommonFederation', 'isincommonfederation', 10, 0, 'false', 0);
GO
CREATE TABLE [saml20$contactproperty] (
	[id] bigint NOT NULL,
	[_content_] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2ef9b190-f184-4d26-af23-ef27d8caef1a', 'SAML20.ContactProperty', 'saml20$contactproperty', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('46e88280-6f70-4ff3-9f77-6b4456284361', '2ef9b190-f184-4d26-af23-ef27d8caef1a', '_content_', '_content_', 30, 200, '', 0);
GO
CREATE TABLE [system$changehash] (
	[id] bigint NOT NULL,
	[objectid] bigint NULL,
	[attribute] nvarchar(200) NULL,
	[hash] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('24f72d72-3c66-46e4-a08b-09daf0f451d8', 'System.ChangeHash', 'system$changehash', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('47b5e625-85c7-431f-8aaa-3d1e532287f9', '24f72d72-3c66-46e4-a08b-09daf0f451d8', 'ObjectId', 'objectid', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('28dc5eba-42c6-4602-ac97-35eda6b366b0', '24f72d72-3c66-46e4-a08b-09daf0f451d8', 'Attribute', 'attribute', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a142f7d6-8ed6-4753-8706-d5938e9879aa', '24f72d72-3c66-46e4-a08b-09daf0f451d8', 'Hash', 'hash', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('13f98cae-7256-37ee-a5de-fdb7aa36e8b5', '24f72d72-3c66-46e4-a08b-09daf0f451d8', 'createdDate', 'createddate', 20, 0, '', 0);
GO
CREATE TABLE [excr_inlinespc$tempspcsetup] (
	[id] bigint NOT NULL,
	[name] nvarchar(max) NULL,
	[instanceid] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('8881af53-24fc-46ff-96ce-1da98c93b400', 'EXCR_InlineSPC.TempSPCSetup', 'excr_inlinespc$tempspcsetup', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ad33634f-f297-4ae8-960c-3fa9f93a54be', '8881af53-24fc-46ff-96ce-1da98c93b400', 'Name', 'name', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('49a1eccb-8f75-4637-b9aa-b4af7fe2f8cf', '8881af53-24fc-46ff-96ce-1da98c93b400', 'InstanceId', 'instanceid', 30, 0, '', 0);
GO
CREATE TABLE [system$workflowversion] (
	[id] bigint NOT NULL,
	[versionhash] nvarchar(200) NULL,
	[modeljson] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'System.WorkflowVersion', 'system$workflowversion', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ee842048-ff1d-4ea4-80b3-2d1123437d5f', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'VersionHash', 'versionhash', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2c5449d3-09f4-463f-8a99-439c6cb74fed', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'ModelJSON', 'modeljson', 30, 0, '', 0);
GO
CREATE TABLE [system$workflowactivityusertaskoutcome] (
	[id] bigint NOT NULL,
	[outcome] nvarchar(200) NULL,
	[time] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1ebda4ad-6e00-4b19-8b95-6b6261beb937', 'System.WorkflowActivityUserTaskOutcome', 'system$workflowactivityusertaskoutcome', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f854eb95-236e-4c3f-af60-0d72c0cbf27b', '1ebda4ad-6e00-4b19-8b95-6b6261beb937', 'Outcome', 'outcome', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9c9b1db9-e8ff-4192-b11d-4127162aa51a', '1ebda4ad-6e00-4b19-8b95-6b6261beb937', 'Time', 'time', 20, 0, '', 0);
GO
CREATE TABLE [saml20$entitydescriptor] (
	[id] bigint NOT NULL,
	[entityid] nvarchar(200) NULL,
	[validuntil] datetime2(3) NULL,
	[cacheduration] nvarchar(200) NULL,
	[_id] nvarchar(200) NULL,
	[updated] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('50527e51-152a-40ca-98fa-75a0b24193bb', 'SAML20.EntityDescriptor', 'saml20$entitydescriptor', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('41eac392-1a9e-42c2-aede-080998b69987', '50527e51-152a-40ca-98fa-75a0b24193bb', 'entityID', 'entityid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f713d5e9-d9bb-4b47-8e2a-2a5f56649ab4', '50527e51-152a-40ca-98fa-75a0b24193bb', 'validUntil', 'validuntil', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3da040cb-57dc-4dd9-b77b-f15fce611730', '50527e51-152a-40ca-98fa-75a0b24193bb', 'cacheDuration', 'cacheduration', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f0ca7e04-5add-48f7-907a-f58817b959b1', '50527e51-152a-40ca-98fa-75a0b24193bb', '_ID', '_id', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('bc659901-1882-4830-9c7c-1eb98a2f742a', '50527e51-152a-40ca-98fa-75a0b24193bb', 'Updated', 'updated', 10, 0, 'true', 0);
GO
CREATE TABLE [saml20$keydescriptor] (
	[id] bigint NOT NULL,
	[use] nvarchar(20) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('86288f14-2268-41a8-8cad-a48c1ec43836', 'SAML20.KeyDescriptor', 'saml20$keydescriptor', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4139b38e-ca62-41d0-9202-3cf8ffb666ae', '86288f14-2268-41a8-8cad-a48c1ec43836', 'use', 'use', 30, 20, '', 0);
GO
CREATE TABLE [excr_commons_api$document] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('018cd62e-fa91-4249-b530-c30936c283f3', 'EXCR_Commons_API.Document', 'excr_commons_api$document', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [mxmodelreflection$mxobjectenum] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('c7be83f4-3dd2-4168-a27b-702c1e0511b3', 'MxModelReflection.MxObjectEnum', 'mxmodelreflection$mxobjectenum', '398a1f70-2b2c-408e-8778-f4a923fda765', 0, 0);
GO
CREATE TABLE [saml20$keyinfo] (
	[id] bigint NOT NULL,
	[_id] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', 'SAML20.KeyInfo', 'saml20$keyinfo', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5f1a906d-84df-4dc0-bd03-5783bc986469', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', '_Id', '_id', 30, 200, '', 0);
GO
CREATE TABLE [mxmodelreflection$token] (
	[id] bigint NOT NULL,
	[token] nvarchar(50) NULL,
	[prefix] nvarchar(3) NULL,
	[suffix] nvarchar(3) NULL,
	[combinedtoken] nvarchar(56) NULL,
	[description] nvarchar(300) NULL,
	[metamodelpath] nvarchar(1000) NULL,
	[tokentype] nvarchar(9) NULL,
	[status] nvarchar(7) NULL,
	[findobjectstart] nvarchar(200) NULL,
	[findobjectreference] nvarchar(200) NULL,
	[findreference] nvarchar(200) NULL,
	[findmember] nvarchar(200) NULL,
	[findmemberreference] nvarchar(200) NULL,
	[isoptional] bit NULL,
	[displaypattern] nvarchar(50) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$token_combinedtoken_asc] ON [mxmodelreflection$token] ([combinedtoken] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$token_system$changedby] ON [mxmodelreflection$token] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$token_system$owner] ON [mxmodelreflection$token] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1532681f-31fb-402f-991d-87e23ef1bcf4', 'MxModelReflection.Token', 'mxmodelreflection$token', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7bbe5cdb-bbd5-4a83-9bde-f2ed2e2db304', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'Token', 'token', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4785010e-ef32-43ad-885b-b3cea8fe8b2d', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'Prefix', 'prefix', 30, 3, '{%', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('989bf954-e4ca-4e26-8a93-ff53ac635d86', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'Suffix', 'suffix', 30, 3, '%}', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('19712a0b-17d2-4ca6-80ab-822d245a3c8f', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'CombinedToken', 'combinedtoken', 30, 56, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2d04352d-b669-4acb-9e93-1cbf78455dd3', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'Description', 'description', 30, 300, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9238edc4-198b-4444-ba24-f3d115cb5730', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'MetaModelPath', 'metamodelpath', 30, 1000, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e1d92d6f-ccaa-4c7d-9315-5a92fb0d0e72', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'TokenType', 'tokentype', 40, 9, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2925a582-4173-4225-9e77-e37d032b22cf', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'Status', 'status', 40, 7, 'Invalid', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('bc793329-b0aa-41c3-9cf7-a62640064965', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'FindObjectStart', 'findobjectstart', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('54e3d5c0-739e-4631-92ac-7a8a8ceec1fa', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'FindObjectReference', 'findobjectreference', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c4137039-03b3-46ae-a629-f64c2a530958', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'FindReference', 'findreference', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ee35c7a1-2ff0-47de-bbcd-7ca80f325e1c', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'FindMember', 'findmember', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2ac71fb0-26b7-4d15-bf0a-fb44251c0ea6', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'FindMemberReference', 'findmemberreference', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('671f0912-b83b-437c-8b1d-8b2447982456', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'IsOptional', 'isoptional', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a1ab0f23-b587-4763-9b00-29ce60e0bac0', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'DisplayPattern', 'displaypattern', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d271af5c-0bea-3d4f-92aa-3087823f6212', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2947ce7e-3bcc-3866-a2ea-bab82d23b615', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('7f075c7c-afe9-48a3-ac6f-fbd1060e0977', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'idx_mxmodelreflection$token_combinedtoken_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('7f075c7c-afe9-48a3-ac6f-fbd1060e0977', '19712a0b-17d2-4ca6-80ab-822d245a3c8f', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('e0827c3f-ecac-35cf-ac7e-da3f1fb04222', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'idx_mxmodelreflection$token_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('e0827c3f-ecac-35cf-ac7e-da3f1fb04222', 'ac496cfd-3ab6-33cf-a2cb-f8d522ef965d', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('615bef4d-fdd0-3e17-ace4-0866fff94d93', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'idx_mxmodelreflection$token_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('615bef4d-fdd0-3e17-ace4-0866fff94d93', 'f3f32dbd-41b8-33e2-bc66-e7f895c4193b', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('ac496cfd-3ab6-33cf-a2cb-f8d522ef965d', 'System.changedBy', 'mxmodelreflection$token', '1532681f-31fb-402f-991d-87e23ef1bcf4', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('f3f32dbd-41b8-33e2-bc66-e7f895c4193b', 'System.owner', 'mxmodelreflection$token', '1532681f-31fb-402f-991d-87e23ef1bcf4', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [excr_extentityaccess$resources$pk] (
	[id] bigint NOT NULL,
	[instanceid] nvarchar(200) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_excr_extentityaccess$resources$pk_instanceid] UNIQUE ([instanceid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2e0a3f16-2575-4f70-8436-f41214d34f68', 'EXCR_ExtEntityAccess.Resources', 'excr_extentityaccess$resources$pk', 1, 1);
GO
INSERT INTO [mendixsystem$remote_primary_key] ([id], [entity_id], [attribute_name], [column_name], [type], [length]) VALUES ('036cb4ca-9090-3380-9f83-5703223d0830', '2e0a3f16-2575-4f70-8436-f41214d34f68', 'InstanceId', 'instanceid', 30, 200);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_extentityaccess$resources$pk_instanceid', '2e0a3f16-2575-4f70-8436-f41214d34f68', '036cb4ca-9090-3380-9f83-5703223d0830');
GO
CREATE TABLE [saml20$ssoconfiguration] (
	[id] bigint NOT NULL,
	[issamlloggingenabled] bit NULL,
	[authncontext] nvarchar(7) NULL,
	[idpmetadataurl] nvarchar(200) NULL,
	[readidpmetadatafromurl] bit NULL,
	[wizardmode] bit NULL,
	[createusers] bit NULL,
	[currentwizardstep] nvarchar(5) NULL,
	[alias] nvarchar(300) NULL,
	[active] bit NULL,
	[allowidpinitiatedauthentication] bit NULL,
	[identifyingassertiontype] nvarchar(19) NULL,
	[customidentifyingassertionname] nvarchar(max) NULL,
	[usecustomlogicforprovisioning] bit NULL,
	[usecustomaftersigninlogic] bit NULL,
	[disablenameidpolicy] bit NULL,
	[enabledelegatedauthentication] bit NULL,
	[delegatedauthenticationurl] nvarchar(500) NULL,
	[enablemobileauthtoken] bit NULL,
	[migratedtoprioritizedsamlauthncontexts] bit NULL,
	[responseprotocolbinding] nvarchar(16) NULL,
	[enableassertionconsumerserviceindex] nvarchar(3) NULL,
	[assertionconsumerserviceindex] int NULL,
	[enableforceauthentication] bit NULL,
	[useencryption] bit NULL,
	[encryptionmethod] nvarchar(13) NULL,
	[encryptionkeylength] nvarchar(19) NULL,
	[isuploadnewkeypair] bit NULL,
	[customaftersigninmicroflow] nvarchar(200) NULL,
	[customevaluateinsessionauthenticationmicroflow] nvarchar(200) NULL,
	[customprepareinsessionauthenticationmicroflow] nvarchar(200) NULL,
	[customuserprovisioningmicroflow] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'SAML20.SSOConfiguration', 'saml20$ssoconfiguration', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('37860711-7066-4502-85a2-19b142414e07', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'IsSAMLLoggingEnabled', 'issamlloggingenabled', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1682357d-64ce-4d50-b2a7-70176e1b9901', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'AuthnContext', 'authncontext', 40, 7, 'EXACT', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cc02d276-204c-4336-bff3-28551fad668f', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'IdPMetadataURL', 'idpmetadataurl', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e5fa88fa-507b-4ea5-98a7-67b0daece68d', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'ReadIdPMetadataFromURL', 'readidpmetadatafromurl', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5d88ab73-8f9f-4baf-8b00-bacbb8d2f24e', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'WizardMode', 'wizardmode', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4c1678c6-6c3c-4162-90f8-c6471b32738e', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CreateUsers', 'createusers', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0fe7f6f6-c5aa-4e3b-ab84-5f0c5ff10940', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CurrentWizardStep', 'currentwizardstep', 40, 5, 'Step1', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c56975fc-8b19-40a3-8fb2-1c9a59d17915', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'Alias', 'alias', 30, 300, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f7f70ed6-c877-44f5-a8e2-72972aa57579', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'Active', 'active', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('268f1536-1b4d-4a84-a52a-90fa73166943', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'AllowIdpInitiatedAuthentication', 'allowidpinitiatedauthentication', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c3bd88e6-0251-4de0-91b7-80ed68aadb69', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'IdentifyingAssertionType', 'identifyingassertiontype', 40, 19, 'IdP_Provided', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0af9ace3-1bae-4c8f-b8f4-c940586dfa63', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CustomIdentifyingAssertionName', 'customidentifyingassertionname', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('de8db942-5d63-4bf2-a05b-8772660410a9', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'UseCustomLogicForProvisioning', 'usecustomlogicforprovisioning', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('24bc77e9-0369-4160-9650-dcaea3a15456', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'UseCustomAfterSigninLogic', 'usecustomaftersigninlogic', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('66a1c892-1c49-4e8a-b048-50ff3b07b39d', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'DisableNameIDPolicy', 'disablenameidpolicy', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('52f0793c-e5e4-4dae-9587-35cba9a218de', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EnableDelegatedAuthentication', 'enabledelegatedauthentication', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ca33946b-079e-45c1-bd65-583ab72a8154', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'DelegatedAuthenticationURL', 'delegatedauthenticationurl', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6c76ae03-e500-4200-8952-f432876db2ce', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EnableMobileAuthToken', 'enablemobileauthtoken', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('74c03094-6c74-4b47-b741-51e774ef98b5', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'MigratedToPrioritizedSAMLAuthnContexts', 'migratedtoprioritizedsamlauthncontexts', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('31334193-396e-4df8-b475-ce069894abae', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'ResponseProtocolBinding', 'responseprotocolbinding', 40, 16, 'POST_BINDING', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a64d7ded-21ab-4b2d-88ca-4aa7d5610499', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EnableAssertionConsumerServiceIndex', 'enableassertionconsumerserviceindex', 40, 3, 'No', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0ed76a26-a478-471a-929c-68d53dabf357', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'AssertionConsumerServiceIndex', 'assertionconsumerserviceindex', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('867ca531-0fa4-4d52-8dc4-9f8605081299', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EnableForceAuthentication', 'enableforceauthentication', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4af59183-f294-42b1-b9dd-a15159959b9f', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'UseEncryption', 'useencryption', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7133eb96-84d1-4f52-8753-38dc0ce138d6', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EncryptionMethod', 'encryptionmethod', 40, 13, 'SHA256WithRSA', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c2988c71-a3d8-44e1-8dcf-6c53f85c0206', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'EncryptionKeyLength', 'encryptionkeylength', 40, 19, '_2048bit_Encryption', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b91cb46e-6078-4921-8668-314b1e44a8e6', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'isUploadNewKeyPair', 'isuploadnewkeypair', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e4b44c29-9be7-47d7-b409-befb84b67cfa', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CustomAfterSigninMicroflow', 'customaftersigninmicroflow', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('139b2627-ea2f-4218-bb7a-0d3e2942ee6e', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CustomEvaluateInSessionAuthenticationMicroflow', 'customevaluateinsessionauthenticationmicroflow', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('90cd2db8-8b93-440c-a7bb-f5f9393ed715', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CustomPrepareInSessionAuthenticationMicroflow', 'customprepareinsessionauthenticationmicroflow', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1ce03357-86a1-4f73-8ac7-2e872d072933', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'CustomUserProvisioningMicroflow', 'customuserprovisioningmicroflow', 30, 200, '', 0);
GO
CREATE TABLE [documentgeneration$documentrequest] (
	[id] bigint NOT NULL,
	[requestid] nvarchar(200) NULL,
	[filename] nvarchar(200) NULL,
	[resultentity] nvarchar(200) NULL,
	[microflowname] nvarchar(200) NULL,
	[contextobjectguid] bigint NULL,
	[securitytoken] nvarchar(200) NULL,
	[expirationdate] datetime2(3) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_documentgeneration$documentrequest_requestid] UNIQUE ([requestid]));
GO
CREATE INDEX [idx_documentgeneration$documentrequest_requestid_asc] ON [documentgeneration$documentrequest] ([requestid] ASC,[id] ASC);
GO
CREATE INDEX [idx_documentgeneration$documentrequest_expirationdate_asc] ON [documentgeneration$documentrequest] ([expirationdate] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'DocumentGeneration.DocumentRequest', 'documentgeneration$documentrequest', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('bab6abf3-5a4a-4652-9e6d-733c7d4d5c8d', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'RequestId', 'requestid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6f706d27-15ee-4fff-b01b-0531e72666b1', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'FileName', 'filename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a8f68f66-600d-488c-9e9b-d868aef25b90', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'ResultEntity', 'resultentity', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c79f3248-a9e4-491a-965e-86beac5e09dc', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'MicroflowName', 'microflowname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f83a5d07-db25-4ab7-92aa-381c62a7f134', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'ContextObjectGuid', 'contextobjectguid', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fdc01439-bae0-4202-ae4b-4464b1cf2c20', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'SecurityToken', 'securitytoken', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('101ff72e-e388-46d3-807b-1e9f1541c6d1', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'ExpirationDate', 'expirationdate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('30731a0f-87d3-3955-a7e7-0c538a77f70c', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('d972a9ba-bb19-46ae-a1cf-278f1cace565', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'idx_documentgeneration$documentrequest_requestid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('d972a9ba-bb19-46ae-a1cf-278f1cace565', 'bab6abf3-5a4a-4652-9e6d-733c7d4d5c8d', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('f2d1e8ca-af49-4119-9026-b85a33d31417', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'idx_documentgeneration$documentrequest_expirationdate_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('f2d1e8ca-af49-4119-9026-b85a33d31417', '101ff72e-e388-46d3-807b-1e9f1541c6d1', 0, 0);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_documentgeneration$documentrequest_requestid', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', 'bab6abf3-5a4a-4652-9e6d-733c7d4d5c8d');
GO
CREATE TABLE [system$language] (
	[id] bigint NOT NULL,
	[code] nvarchar(20) NULL,
	[description] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('76805df3-dede-435f-92a6-d6525c68a693', 'System.Language', 'system$language', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('88390044-5b74-4e71-8b88-a6e4e91f6f2e', '76805df3-dede-435f-92a6-d6525c68a693', 'Code', 'code', 30, 20, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d879cb38-5630-4fdf-9e39-f03da0aa8ede', '76805df3-dede-435f-92a6-d6525c68a693', 'Description', 'description', 30, 200, '', 0);
GO
CREATE TABLE [mxmodelreflection$module] (
	[id] bigint NOT NULL,
	[modulename] nvarchar(200) NULL,
	[synchronizeobjectswithinmodule] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'MxModelReflection.Module', 'mxmodelreflection$module', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('59c0d2d7-1d03-407a-bf49-641dcc98d7c0', '2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'ModuleName', 'modulename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('50ae7750-7fdb-4537-831c-5fe48b593d01', '2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'SynchronizeObjectsWithinModule', 'synchronizeobjectswithinmodule', 10, 0, 'false', 0);
GO
CREATE TABLE [system$privatefiledocument] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('3b6f5ca3-28d6-4581-b26e-7ce5bd0e6eeb', 'System.PrivateFileDocument', 'system$privatefiledocument', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [system$synchronizationerrorfile] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('9b26443c-f4bb-4252-aa62-9eaffb71c4db', 'System.SynchronizationErrorFile', 'system$synchronizationerrorfile', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [excr_resourcemanagement_api$resimage] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('062006cd-b52e-4e0c-b505-f1f275611e1e', 'EXCR_ResourceManagement_API.ResImage', 'excr_resourcemanagement_api$resimage', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [system$scheduledeventinformation] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[description] nvarchar(max) NULL,
	[starttime] datetime2(3) NULL,
	[endtime] datetime2(3) NULL,
	[status] nvarchar(9) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'System.ScheduledEventInformation', 'system$scheduledeventinformation', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8b7184a0-cd05-4c75-89f9-be6e9349783b', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('26ccae8a-22b1-4899-87c9-c5b4915dbf28', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'Description', 'description', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('651e7007-7fcd-43b3-a918-a0de81de34bf', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'StartTime', 'starttime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('33adad79-f658-4e69-8c58-e003fb3c78be', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'EndTime', 'endtime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6d2a7545-4e52-4c5d-ac02-0b8211d0585f', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'Status', 'status', 40, 9, '', 0);
GO
CREATE TABLE [saml20$claimmap] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('114fb822-41ba-4482-90e3-2c7a8743bf7f', 'SAML20.ClaimMap', 'saml20$claimmap', 0, 0);
GO
CREATE TABLE [excr_eprocedure_api$eproceduredocument] (
	[id] bigint NOT NULL,
	[description] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('30107e65-7110-4622-9eb1-281e21c60737', 'EXCR_EProcedure_API.EProcedureDocument', 'excr_eprocedure_api$eproceduredocument', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3849ccf4-b6d4-4ebc-a258-2b4ac0049044', '30107e65-7110-4622-9eb1-281e21c60737', 'Description', 'description', 30, 200, '', 0);
GO
CREATE TABLE [excr_commons$documenthelper] (
	[id] bigint NOT NULL,
	[showviewer] bit NULL,
	[islistview] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('3e1d89f1-d6a2-4604-8c9f-310600889ba9', 'EXCR_Commons.DocumentHelper', 'excr_commons$documenthelper', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('795af287-b4ae-4f6e-b1f0-d6a16a536499', '3e1d89f1-d6a2-4604-8c9f-310600889ba9', 'showViewer', 'showviewer', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cb480eda-9c1b-4afa-b170-2c2dd9623ea8', '3e1d89f1-d6a2-4604-8c9f-310600889ba9', 'isListView', 'islistview', 10, 0, 'false', 0);
GO
CREATE TABLE [system$workflowusertaskoutcome] (
	[id] bigint NOT NULL,
	[outcome] nvarchar(200) NULL,
	[time] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d753ad05-63c3-4d18-9424-1dd97c7d1a05', 'System.WorkflowUserTaskOutcome', 'system$workflowusertaskoutcome', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1c43b5b0-f645-4bc1-83fe-2da08ef1eba4', 'd753ad05-63c3-4d18-9424-1dd97c7d1a05', 'Outcome', 'outcome', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2b63667d-dda0-4033-b432-679f51187ebb', 'd753ad05-63c3-4d18-9424-1dd97c7d1a05', 'Time', 'time', 20, 0, '', 0);
GO
CREATE TABLE [system$thumbnail] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('4babd4c0-b903-4cb4-b1af-e59c4a5fcf3d', 'System.Thumbnail', 'system$thumbnail', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [documentgeneration$configuration] (
	[id] bigint NOT NULL,
	[deploymenttype] nvarchar(20) NULL,
	[registrationstatus] nvarchar(12) NULL,
	[applicationurl] nvarchar(200) NULL,
	[accesstoken] nvarchar(max) NULL,
	[accesstokenexpirationdate] datetime2(3) NULL,
	[refreshtoken] nvarchar(max) NULL,
	[verificationtoken] nvarchar(200) NULL,
	[verificationtokenexpirationdate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d27f9367-8f2f-4064-b68e-6753495e7904', 'DocumentGeneration.Configuration', 'documentgeneration$configuration', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7bba4207-391a-417e-b7ea-120d6a3adcf9', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'DeploymentType', 'deploymenttype', 40, 20, 'MendixPublicCloud', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d238f308-a781-43f1-8636-922e4e98524b', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'RegistrationStatus', 'registrationstatus', 40, 12, 'Unregistered', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('03def04e-8ff0-47e8-9367-02b476d08564', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'ApplicationUrl', 'applicationurl', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('665def66-1c16-4460-9c5c-af7f5ac9445d', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'AccessToken', 'accesstoken', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7e274ab6-e0cf-424d-967d-19dcdfd7f1d9', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'AccessTokenExpirationDate', 'accesstokenexpirationdate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e6b41b2a-d506-47c0-92a7-c8c9755e931b', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'RefreshToken', 'refreshtoken', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e4e0019f-e317-4e39-a214-7a902d7e74d6', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'VerificationToken', 'verificationtoken', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('09afee10-11ed-4ba9-815d-9ea2125a36b1', 'd27f9367-8f2f-4064-b68e-6753495e7904', 'VerificationTokenExpirationDate', 'verificationtokenexpirationdate', 20, 0, '', 0);
GO
CREATE TABLE [administration$account] (
	[id] bigint NOT NULL,
	[fullname] nvarchar(200) NULL,
	[email] nvarchar(200) NULL,
	[islocaluser] bit NULL,
	[sessionid] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('c921ccbb-a670-48d9-833d-6a76c1406917', 'Administration.Account', 'administration$account', '282e2e60-88a5-469d-84a5-ba8d9151644f', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('57b3cac6-4999-4794-b4d9-682943024c60', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'FullName', 'fullname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4d882b45-41be-4012-a44e-54a08c290413', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'Email', 'email', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('40f47cfd-60b5-4db3-bc4a-f63d5fdf2564', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'IsLocalUser', 'islocaluser', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('43e31088-f185-4389-ad67-484a749c0893', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'SessionID', 'sessionid', 30, 0, '', 0);
GO
CREATE TABLE [saml20$contact] (
	[id] bigint NOT NULL,
	[contacttype] nvarchar(200) NULL,
	[company] nvarchar(200) NULL,
	[givenname] nvarchar(200) NULL,
	[surname] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('3cfaa082-c97b-4635-a503-8806c914525f', 'SAML20.Contact', 'saml20$contact', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c43c63a7-3126-4da0-bde1-466050a20c21', '3cfaa082-c97b-4635-a503-8806c914525f', 'contactType', 'contacttype', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('de3edef3-f63f-4c9b-8d3f-9ea5fedc7401', '3cfaa082-c97b-4635-a503-8806c914525f', 'Company', 'company', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('90f270f3-63d9-4a74-b217-2afe46874daa', '3cfaa082-c97b-4635-a503-8806c914525f', 'GivenName', 'givenname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('da960f49-a4f5-400c-a29a-7d4b4395b236', '3cfaa082-c97b-4635-a503-8806c914525f', 'SurName', 'surname', 30, 200, '', 0);
GO
CREATE TABLE [saml20$spattributeconsumingservice] (
	[id] bigint NOT NULL,
	[servicename] nvarchar(200) NULL,
	[lang] nvarchar(200) NULL,
	[index] int NULL,
	[isdefault] bit NULL,
	[isactive] bit NULL,
	[logintype] nvarchar(15) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('0f381b16-223a-4086-b6a0-18f5a76553af', 'SAML20.SPAttributeConsumingService', 'saml20$spattributeconsumingservice', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b37241e6-f86e-4aee-be06-1ef12dd7cf63', '0f381b16-223a-4086-b6a0-18f5a76553af', 'ServiceName', 'servicename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3fbd478e-a181-44dc-acbd-04c9ce477d98', '0f381b16-223a-4086-b6a0-18f5a76553af', 'lang', 'lang', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('68613809-1a93-41ed-872c-1efab084eef0', '0f381b16-223a-4086-b6a0-18f5a76553af', 'index', 'index', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('608de68d-fde5-4aef-b0c2-acc71fbb99ad', '0f381b16-223a-4086-b6a0-18f5a76553af', 'isDefault', 'isdefault', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('18847a52-a543-4a6e-ab16-ec42a2737a39', '0f381b16-223a-4086-b6a0-18f5a76553af', 'isActive', 'isactive', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3ae5141c-623f-447e-8559-864db97e72b3', '0f381b16-223a-4086-b6a0-18f5a76553af', 'LoginType', 'logintype', 40, 15, '', 0);
GO
CREATE TABLE [system$autocommitentry] (
	[id] bigint NOT NULL,
	[sessionid] nvarchar(36) NULL,
	[objectid] bigint NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$autocommitentry_sessionid_asc_objectid_asc] ON [system$autocommitentry] ([sessionid] ASC,[objectid] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('67c28960-7a7a-11e6-bdf4-0800200c9a66', 'System.AutoCommitEntry', 'system$autocommitentry', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('67c28961-7a7a-11e6-bdf4-0800200c9a66', '67c28960-7a7a-11e6-bdf4-0800200c9a66', 'SessionId', 'sessionid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('67c28962-7a7a-11e6-bdf4-0800200c9a66', '67c28960-7a7a-11e6-bdf4-0800200c9a66', 'ObjectId', 'objectid', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('bbe9c632-f0be-31a1-a709-ab0d57c79c84', '67c28960-7a7a-11e6-bdf4-0800200c9a66', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('67c28963-7a7a-11e6-bdf4-0800200c9a66', '67c28960-7a7a-11e6-bdf4-0800200c9a66', 'idx_system$autocommitentry_sessionid_asc_objectid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('67c28963-7a7a-11e6-bdf4-0800200c9a66', '67c28961-7a7a-11e6-bdf4-0800200c9a66', 0, 0);
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('67c28963-7a7a-11e6-bdf4-0800200c9a66', '67c28962-7a7a-11e6-bdf4-0800200c9a66', 0, 1);
GO
CREATE TABLE [saml20$configuredsamlauthncontext] (
	[id] bigint NOT NULL,
	[priority] int NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('52e902d2-ef0c-4c71-9629-9131bd78c2e8', 'SAML20.ConfiguredSAMLAuthnContext', 'saml20$configuredsamlauthncontext', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('53275c00-ffce-4f20-a19a-392443f21408', '52e902d2-ef0c-4c71-9629-9131bd78c2e8', 'Priority', 'priority', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9abc9152-f6ff-3b90-bb71-70cbb1905ce3', '52e902d2-ef0c-4c71-9629-9131bd78c2e8', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0d508843-eb29-3ad4-99c1-de7520b22837', '52e902d2-ef0c-4c71-9629-9131bd78c2e8', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
CREATE TABLE [saml20$samlrequest] (
	[id] bigint NOT NULL,
	[requestid] nvarchar(200) NULL,
	[hasrequest] nvarchar(3) NULL,
	[hasresponse] nvarchar(3) NULL,
	[returnedprincipal] nvarchar(200) NULL,
	[responseid] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('0550ede5-68a7-4dcf-ad37-389501ca7d90', 'SAML20.SAMLRequest', 'saml20$samlrequest', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2cc90027-4f21-4a84-91e2-29d7e16b563d', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'RequestID', 'requestid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5b39a735-c36a-4771-80eb-66835b84e16b', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'hasRequest', 'hasrequest', 40, 3, 'No', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ed121984-f8b5-4f3f-8853-6b19e5cb03dd', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'hasResponse', 'hasresponse', 40, 3, 'No', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4b02cb3d-43c0-41f4-a717-059a8d059df4', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'ReturnedPrincipal', 'returnedprincipal', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d3e6a438-70f7-4b7b-a4ba-b48272567ab7', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'ResponseID', 'responseid', 30, 200, '', 0);
GO
CREATE TABLE [mxmodelreflection$dbsizeestimate] (
	[id] bigint NOT NULL,
	[nrofrecords] int NULL,
	[calculatedsizeinbytes] bigint NULL,
	[calculatedsizeinkilobytes] bigint NULL,
	[findobjecttype] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'MxModelReflection.DbSizeEstimate', 'mxmodelreflection$dbsizeestimate', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('be287507-ccfa-4506-900a-19a50b17176e', 'f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'NrOfRecords', 'nrofrecords', 3, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('266aedc3-e5d0-4a02-922c-b80f4d678b6c', 'f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'CalculatedSizeInBytes', 'calculatedsizeinbytes', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('08b84820-c764-4d81-88b6-4aacb52176f6', 'f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'CalculatedSizeInKiloBytes', 'calculatedsizeinkilobytes', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c78a720c-5094-4a56-81fb-5c073dfd438b', 'f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'FindObjectType', 'findobjecttype', 30, 200, '', 0);
GO
CREATE TABLE [mxmodelreflection$valuetype] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[typeenum] nvarchar(11) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$valuetype_system$changedby] ON [mxmodelreflection$valuetype] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$valuetype_system$owner] ON [mxmodelreflection$valuetype] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'MxModelReflection.ValueType', 'mxmodelreflection$valuetype', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4e293e94-5998-483f-a389-696c342e0bc7', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fca87c9e-e201-421b-9ae0-608ce9fb543f', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'TypeEnum', 'typeenum', 40, 11, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('48102919-1ebc-3fb8-bdaf-44544ecd6dc1', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('37a7aeb1-962a-3012-9bb6-dd1628d49da0', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('f402c0f4-7163-3f14-b805-75111de7c34d', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'idx_mxmodelreflection$valuetype_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('f402c0f4-7163-3f14-b805-75111de7c34d', '3f4df266-c8e2-3937-bd4e-21141e1f705c', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('cdfba826-3948-322b-a90f-2c096adc31f3', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'idx_mxmodelreflection$valuetype_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('cdfba826-3948-322b-a90f-2c096adc31f3', '43baa441-c514-35ed-b6e7-889467d96c96', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('3f4df266-c8e2-3937-bd4e-21141e1f705c', 'System.changedBy', 'mxmodelreflection$valuetype', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('43baa441-c514-35ed-b6e7-889467d96c96', 'System.owner', 'mxmodelreflection$valuetype', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [system$synchronizationerror] (
	[id] bigint NOT NULL,
	[reason] nvarchar(max) NULL,
	[objectid] nvarchar(200) NULL,
	[objecttype] nvarchar(1000) NULL,
	[objectcontent] nvarchar(max) NULL,
	[createddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$synchronizationerror_system$owner] ON [system$synchronizationerror] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('f9818ad8-3214-4b1d-b837-3181863f5ed5', 'System.SynchronizationError', 'system$synchronizationerror', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2f76e5ca-ccbe-4137-856b-1999a51431f2', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'Reason', 'reason', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7e215c6c-0cfd-4533-8529-23e4f5359b7a', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'ObjectId', 'objectid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b1684d75-8427-4ed6-be7a-3f4d4e59d61e', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'ObjectType', 'objecttype', 30, 1000, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('29d27bde-bc08-4f80-9d6c-9f47a3209452', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'ObjectContent', 'objectcontent', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a3e1c07d-2b2b-3e20-bbf0-de44e27d29f4', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('02390f36-727f-3e8e-8698-51de6cbd7dd0', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'idx_system$synchronizationerror_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('02390f36-727f-3e8e-8698-51de6cbd7dd0', 'b822057e-d9ee-3cd6-8535-6bc9268cb842', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('b822057e-d9ee-3cd6-8535-6bc9268cb842', 'System.owner', 'system$synchronizationerror', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [excr_commons_api$entity] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('207dc8fc-5a16-4a95-8922-22c66a998f0a', 'EXCR_Commons_API.Entity', 'excr_commons_api$entity', 0, 0);
GO
CREATE TABLE [saml20$attributeconsumingservice] (
	[id] bigint NOT NULL,
	[index] int NULL,
	[isdefault] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'SAML20.AttributeConsumingService', 'saml20$attributeconsumingservice', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5ce65bbd-ca9c-473d-a10d-5c84f9a28f1e', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'index', 'index', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f5be6373-ebeb-4c19-9bc2-d570aec39f72', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'isDefault', 'isdefault', 10, 0, 'false', 0);
GO
CREATE TABLE [excr_commons$menucontext] (
	[id] bigint NOT NULL,
	[isadmin] bit NULL,
	[showhamburgerbutton] bit NULL,
	[showmenuitems] bit NULL,
	[containername] nvarchar(200) NULL,
	[showholdcontainermenu] bit NULL,
	[showreleasecontainermenu] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('9857a113-daeb-4c66-912f-b3edbe4a0047', 'EXCR_Commons.MenuContext', 'excr_commons$menucontext', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c4551100-6214-487b-a774-9281200b7996', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'IsAdmin', 'isadmin', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0762d85e-d15a-4dd2-abb8-96d73f9bbb40', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'ShowHamburgerButton', 'showhamburgerbutton', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7f119e88-69b3-4b25-acd8-75a157ebf4eb', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'ShowMenuItems', 'showmenuitems', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('04f22ddd-4f7b-4c93-b138-abc512251d19', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'ContainerName', 'containername', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4d835e26-8ccc-4f88-a879-7e854a67cf08', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'ShowHoldContainerMenu', 'showholdcontainermenu', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('85771f83-8601-478f-ace6-a6b6bf350868', '9857a113-daeb-4c66-912f-b3edbe4a0047', 'ShowReleaseContainerMenu', 'showreleasecontainermenu', 10, 0, 'false', 0);
GO
CREATE TABLE [mxmodelreflection$parameter] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$parameter_system$changedby] ON [mxmodelreflection$parameter] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$parameter_system$owner] ON [mxmodelreflection$parameter] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'MxModelReflection.Parameter', 'mxmodelreflection$parameter', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a68b60af-c591-4375-beb1-7c6e5cf8fa7d', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d5a05d67-a0bf-3247-af79-8aa9e65cc892', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('490dde9b-49c7-30ee-a3b2-08a85dc998b9', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('8e4a5d95-2e3a-3532-9dba-cfe42b1d94d9', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'idx_mxmodelreflection$parameter_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('8e4a5d95-2e3a-3532-9dba-cfe42b1d94d9', '34975cac-c9e1-38ff-82e5-086208ab1490', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('41ba99f6-4c02-3720-9a16-b7aaeb526911', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'idx_mxmodelreflection$parameter_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('41ba99f6-4c02-3720-9a16-b7aaeb526911', '67f83b80-28bf-3c29-98f2-9a1e7d67a134', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('34975cac-c9e1-38ff-82e5-086208ab1490', 'System.changedBy', 'mxmodelreflection$parameter', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('67f83b80-28bf-3c29-98f2-9a1e7d67a134', 'System.owner', 'mxmodelreflection$parameter', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [saml20$x509certificate] (
	[id] bigint NOT NULL,
	[issuername] nvarchar(500) NULL,
	[serialnumber] nvarchar(max) NULL,
	[subject] nvarchar(500) NULL,
	[validfrom] datetime2(3) NULL,
	[validuntil] datetime2(3) NULL,
	[base64] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'SAML20.X509Certificate', 'saml20$x509certificate', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2cccfa51-8de3-4020-bf77-50de7b3691e0', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'IssuerName', 'issuername', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('636a2e07-70e9-4d23-ab2c-1eb1f537742f', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'SerialNumber', 'serialnumber', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1a475283-7def-46b2-991d-b9613a7b11db', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'Subject', 'subject', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('038f9b57-62d5-465e-8519-8a20a23f64c1', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'ValidFrom', 'validfrom', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0a608492-03b0-4d18-9118-8d873d21c05e', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'ValidUntil', 'validuntil', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d19d1d83-1ccf-4527-bf3e-242dddab1e9c', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'Base64', 'base64', 30, 0, '', 0);
GO
CREATE TABLE [system$unreferencedfile] (
	[id] bigint NOT NULL,
	[filekey] nvarchar(36) NULL,
	[state] nvarchar(8) NULL,
	[transactionid] nvarchar(36) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('4e336d7d-71e8-41f4-9f07-8f0646543e81', 'System.UnreferencedFile', 'system$unreferencedfile', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a7934e8b-3623-45c7-abf4-e8a1d93770a2', '4e336d7d-71e8-41f4-9f07-8f0646543e81', 'FileKey', 'filekey', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a7bd62b4-180e-4f89-8767-4df1817e5419', '4e336d7d-71e8-41f4-9f07-8f0646543e81', 'State', 'state', 40, 8, 'New', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('752e783a-9db4-4825-a87c-7e143366bd85', '4e336d7d-71e8-41f4-9f07-8f0646543e81', 'TransactionId', 'transactionid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cbd2b083-ad9a-3c24-8d5e-01e8c3994cad', '4e336d7d-71e8-41f4-9f07-8f0646543e81', 'createdDate', 'createddate', 20, 0, '', 0);
GO
CREATE TABLE [saml20$entitiesdescriptor] (
	[id] bigint NOT NULL,
	[validuntil] datetime2(3) NULL,
	[cacheduration] nvarchar(200) NULL,
	[_id] nvarchar(200) NULL,
	[name] nvarchar(200) NULL,
	[updated] bit NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('41287618-1d36-43d4-ba70-d34884a5017d', 'SAML20.EntitiesDescriptor', 'saml20$entitiesdescriptor', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a2874cc1-940b-483b-af85-e25ed5a12628', '41287618-1d36-43d4-ba70-d34884a5017d', 'validUntil', 'validuntil', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a660a928-ccf4-438c-b684-62a7aa7ef276', '41287618-1d36-43d4-ba70-d34884a5017d', 'cacheDuration', 'cacheduration', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b84c0e00-7797-4399-bcae-62131846fa60', '41287618-1d36-43d4-ba70-d34884a5017d', '_ID', '_id', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('24927029-40a3-4f3b-afa4-6604b52202cb', '41287618-1d36-43d4-ba70-d34884a5017d', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cd86c319-9d4d-4c09-afcf-21bb224ca4cf', '41287618-1d36-43d4-ba70-d34884a5017d', 'Updated', 'updated', 10, 0, 'false', 0);
GO
CREATE TABLE [system$workflowdefinition] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[title] nvarchar(200) NULL,
	[isobsolete] bit NULL,
	[islocked] bit NULL,
	[modelguid] nvarchar(36) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_system$workflowdefinition_modelguid] UNIQUE ([modelguid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('5c570d3b-7b31-44fe-abd6-269a234584c5', 'System.WorkflowDefinition', 'system$workflowdefinition', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d16c4272-c9d3-4371-86f6-69eb263033e1', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e023e8ca-3319-4698-a841-30430fdca099', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'Title', 'title', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8554021f-9842-4c51-b124-86a102d33da7', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'IsObsolete', 'isobsolete', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5544e32c-48e4-4580-bac2-ed8a14f1e098', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'IsLocked', 'islocked', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d61ef304-2773-4336-a146-8997dfccae8a', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'ModelGUID', 'modelguid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowdefinition_modelguid', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'd61ef304-2773-4336-a146-8997dfccae8a');
GO
CREATE TABLE [system$workflowusertaskdefinition] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[isobsolete] bit NULL,
	[modelguid] nvarchar(36) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('e09e866f-288b-475c-9465-792cde8b878c', 'System.WorkflowUserTaskDefinition', 'system$workflowusertaskdefinition', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('895f51f8-ff84-4694-aa65-1ba19eaeca5e', 'e09e866f-288b-475c-9465-792cde8b878c', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a6f93dd6-2725-4746-8283-c5c1e1f16d3f', 'e09e866f-288b-475c-9465-792cde8b878c', 'IsObsolete', 'isobsolete', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('015434e4-2727-4ee8-aef4-49d17b16afb1', 'e09e866f-288b-475c-9465-792cde8b878c', 'ModelGUID', 'modelguid', 30, 36, '', 0);
GO
CREATE TABLE [saml20$roledescriptor] (
	[id] bigint NOT NULL,
	[_id] nvarchar(200) NULL,
	[validuntil] datetime2(3) NULL,
	[cacheduration] nvarchar(200) NULL,
	[protocolsupportenumeration] nvarchar(200) NULL,
	[errorurl] nvarchar(200) NULL,
	[authnrequestssigned] bit NULL,
	[wantauthnrequestssigned] bit NULL,
	[wantassertionssigned] bit NULL,
	[roledescriptortype] nvarchar(28) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('8c7c691b-0561-4729-b9da-ad76c4403f4f', 'SAML20.RoleDescriptor', 'saml20$roledescriptor', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4f124b38-e584-4901-806e-00fe16959ae1', '8c7c691b-0561-4729-b9da-ad76c4403f4f', '_ID', '_id', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f49f0ae1-83f4-4486-b30b-60d88e658b92', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'validUntil', 'validuntil', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('576358e3-d94c-4280-b31e-22a355bd57b1', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'cacheDuration', 'cacheduration', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e9eb0138-3e46-4614-a312-97a11e6c0374', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'protocolSupportEnumeration', 'protocolsupportenumeration', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('edb31e78-fe60-4392-8641-443a28b2f663', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'errorURL', 'errorurl', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('382c6bcb-f0c4-4869-91e2-9dafeac98d60', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'AuthnRequestsSigned', 'authnrequestssigned', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f4879aaf-4086-4b5a-9023-7a09c0b596c1', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'WantAuthnRequestsSigned', 'wantauthnrequestssigned', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3c098d35-32b1-4595-ab9a-97ff8b77ba21', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'WantAssertionsSigned', 'wantassertionssigned', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('64c15503-ba3f-4309-bc2d-df81e550cba1', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'RoleDescriptorType', 'roledescriptortype', 40, 28, '', 0);
GO
CREATE TABLE [mxmodelreflection$mxobjectenumvalue] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectenumvalue_system$owner] ON [mxmodelreflection$mxobjectenumvalue] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectenumvalue_system$changedby] ON [mxmodelreflection$mxobjectenumvalue] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'MxModelReflection.MxObjectEnumValue', 'mxmodelreflection$mxobjectenumvalue', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0a8b69ac-a061-488d-99a6-212d84397082', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('55c8ce06-86f3-30cb-83ab-6fb97ce2032a', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('76fd88a6-7c24-3f98-bab8-941d26b75311', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('d25326cd-d515-3ff9-9a40-699a1a03f21b', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'idx_mxmodelreflection$mxobjectenumvalue_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('d25326cd-d515-3ff9-9a40-699a1a03f21b', '87505873-5cd1-3693-9f93-df6839c56335', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('3e5a37c4-6616-3fa9-ad28-70c9c98e0f8a', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'idx_mxmodelreflection$mxobjectenumvalue_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('3e5a37c4-6616-3fa9-ad28-70c9c98e0f8a', '0395f520-0533-3c68-a12b-e9354cada907', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('87505873-5cd1-3693-9f93-df6839c56335', 'System.owner', 'mxmodelreflection$mxobjectenumvalue', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('0395f520-0533-3c68-a12b-e9354cada907', 'System.changedBy', 'mxmodelreflection$mxobjectenumvalue', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [encryption$pgpcertificate] (
	[id] bigint NOT NULL,
	[certificatetype] nvarchar(10) NULL,
	[passphrase_plain] nvarchar(20) NULL,
	[passphrase_encrypted] nvarchar(100) NULL,
	[reference] nvarchar(100) NULL,
	[emailaddress] nvarchar(50) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'Encryption.PGPCertificate', 'encryption$pgpcertificate', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('290e98f8-0e78-4fa7-98dc-baf5ce1d8ddf', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'CertificateType', 'certificatetype', 40, 10, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8e4a8d69-d498-4a0c-a2f1-2050b846479e', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'PassPhrase_Plain', 'passphrase_plain', 30, 20, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d3ae1e93-e7fd-498f-af7a-6affb52af029', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'PassPhrase_Encrypted', 'passphrase_encrypted', 30, 100, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ead99d7d-d943-41c5-a8e8-dae01b263569', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'Reference', 'reference', 30, 100, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fe2a2842-7e58-4233-bb2b-ffc9c0ba2669', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'EmailAddress', 'emailaddress', 30, 50, '', 0);
GO
CREATE TABLE [saml20$organizationproperty] (
	[id] bigint NOT NULL,
	[_content_] nvarchar(200) NULL,
	[lang] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('b6bf5da5-8043-4474-a926-19b295e0b71d', 'SAML20.OrganizationProperty', 'saml20$organizationproperty', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('678a963f-834f-4cdc-8c49-808e77f2b9d8', 'b6bf5da5-8043-4474-a926-19b295e0b71d', '_content_', '_content_', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5e0807e7-ddf6-49e9-9ac6-a156ecfd2eee', 'b6bf5da5-8043-4474-a926-19b295e0b71d', 'lang', 'lang', 30, 200, '', 0);
GO
CREATE TABLE [disw_designsystem$accountextension] (
	[id] bigint NOT NULL,
	[initials] nvarchar(2) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('851ba5bf-ee08-4920-8155-c987c34bb925', 'DISW_DesignSystem.AccountExtension', 'disw_designsystem$accountextension', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e9f605ca-b307-48ef-8afc-c40d87e4fa1b', '851ba5bf-ee08-4920-8155-c987c34bb925', 'Initials', 'initials', 30, 2, '', 0);
GO
CREATE TABLE [system$tokeninformation] (
	[id] bigint NOT NULL,
	[token] nvarchar(200) NULL,
	[expirydate] datetime2(3) NULL,
	[useragent] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('09b2f0fe-4a11-4afc-a16e-94992a3ebc3d', 'System.TokenInformation', 'system$tokeninformation', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a169a8d1-b10f-427b-b492-3aebeabb7cd6', '09b2f0fe-4a11-4afc-a16e-94992a3ebc3d', 'Token', 'token', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8bb77a0c-0461-43f9-bb27-e91fb9e3623f', '09b2f0fe-4a11-4afc-a16e-94992a3ebc3d', 'ExpiryDate', 'expirydate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0b9398e4-9bb7-4ab8-958c-43526f2c83bf', '09b2f0fe-4a11-4afc-a16e-94992a3ebc3d', 'UserAgent', 'useragent', 30, 0, '', 0);
GO
CREATE TABLE [exsm_recipemgmt$recipemgmtuserconfig] (
	[id] bigint NOT NULL,
	[recipemgmtgridconfig] nvarchar(max) NULL,
	[recipemtrxgridconfig] nvarchar(max) NULL,
	[recipeauditmaingridconfig] nvarchar(max) NULL,
	[recipeauditfieldsgridconfig] nvarchar(max) NULL,
	[recipemgntparamgridconfig] nvarchar(max) NULL,
	[recipemgntsubparamgridconfig] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('ac828022-994f-48dd-9add-57ad0e8bce36', 'EXSM_RecipeMgmt.RecipeMgmtUserConfig', 'exsm_recipemgmt$recipemgmtuserconfig', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5595c628-43d4-4278-a80f-e7163d8033d0', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeMgmtGridConfig', 'recipemgmtgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c55d3223-214a-45b8-bf57-809b7d4ffbf6', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeMtrxGridConfig', 'recipemtrxgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('57cba67c-228f-4f05-8f1b-ba17b8cb6026', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeAuditMainGridConfig', 'recipeauditmaingridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5e37b22b-c6a9-4862-8f48-8eb307dde453', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeAuditFieldsGridConfig', 'recipeauditfieldsgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ea5f2ba1-2c7d-4958-8367-60d84a1488b1', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeMgntParamGridConfig', 'recipemgntparamgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c2b4a66c-e6ac-478f-b414-f0a18b70fe81', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'RecipeMgntSubParamGridConfig', 'recipemgntsubparamgridconfig', 30, 0, '', 0);
GO
CREATE TABLE [exsm_experimentmgmt$expmgmtuserconfig] (
	[id] bigint NOT NULL,
	[expmgmtmaingridconfig] nvarchar(max) NULL,
	[expmgmtblanketexpgridconfig] nvarchar(max) NULL,
	[expmgmtlotassigngridconfig] nvarchar(max) NULL,
	[expmgmtactiondetailsgridconfig] nvarchar(max) NULL,
	[expmgmtparamsoverridegridconfig] nvarchar(max) NULL,
	[expmgmtslotwaferinstgridconfig] nvarchar(max) NULL,
	[expmgmtworkflowstepgridconfig] nvarchar(max) NULL,
	[expmgmtaudittrailheadergridconfig] nvarchar(max) NULL,
	[expmgmtaudittraildelgridconfig] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('0eb6734c-7785-4fab-b58c-5e1758b0b636', 'EXSM_ExperimentMgmt.ExpMgmtUserConfig', 'exsm_experimentmgmt$expmgmtuserconfig', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('41064707-b592-4cb3-b5eb-f33cbe40767e', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtMainGridConfig', 'expmgmtmaingridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ba218ed2-fe33-4d3e-9794-dc10e9969b4a', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtBlanketExpGridConfig', 'expmgmtblanketexpgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8ce7136b-ada6-4506-a9ae-47ece0a8481c', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtLotAssignGridConfig', 'expmgmtlotassigngridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b42e6cb2-7a1b-4937-8dc3-9fea5f13e24e', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtActionDetailsGridConfig', 'expmgmtactiondetailsgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('213d3838-68b5-4d9e-b93a-e87286226c03', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtParamsOverrideGridConfig', 'expmgmtparamsoverridegridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0b6c5ccb-aaae-487c-98ae-65c08c58b8d5', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtSlotWaferInstGridConfig', 'expmgmtslotwaferinstgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('df288ca7-c582-4aff-a5fe-e176b4b6fde7', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtWorkflowStepGridConfig', 'expmgmtworkflowstepgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7a205d8a-cab1-4717-aaa2-d1a5e46dce18', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtAuditTrailHeaderGridConfig', 'expmgmtaudittrailheadergridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e1633773-86cb-4ffa-b9fe-a96a949dd717', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'ExpMgmtAuditTrailDelGridConfig', 'expmgmtaudittraildelgridconfig', 30, 0, '', 0);
GO
CREATE TABLE [saml20$samlauthncontext] (
	[id] bigint NOT NULL,
	[description] nvarchar(200) NULL,
	[value] nvarchar(200) NULL,
	[defaultpriority] int NULL,
	[provisioned] bit NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'SAML20.SAMLAuthnContext', 'saml20$samlauthncontext', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5b78ee05-f1c2-45bc-8da6-9dd3aa5eab38', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'Description', 'description', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('34496c55-74b1-4224-b2d6-e40045c051ad', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'Value', 'value', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('dac31f0d-2efc-45f0-a146-ac99b18d79d5', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'DefaultPriority', 'defaultpriority', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fd563763-f2e0-443a-a9d2-d3c51734088e', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'Provisioned', 'provisioned', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5ada203a-1964-3341-8c24-226a74629552', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('faa5a985-0b03-3b65-8e78-42f9b4b86680', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
CREATE TABLE [mxmodelreflection$mxobjecttype] (
	[id] bigint NOT NULL,
	[completename] nvarchar(200) NULL,
	[name] nvarchar(200) NULL,
	[module] nvarchar(200) NULL,
	[readablename] nvarchar(400) NULL,
	[persistencetype] nvarchar(14) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjecttype_system$owner] ON [mxmodelreflection$mxobjecttype] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjecttype_system$changedby] ON [mxmodelreflection$mxobjecttype] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('aae44a99-27e6-437f-9c9b-964fdcbeb825', 'MxModelReflection.MxObjectType', 'mxmodelreflection$mxobjecttype', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b3acf6c7-947f-43f3-8c7d-bfa9ff974bf6', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'CompleteName', 'completename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7a272a09-0290-4b52-9833-60036b374873', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('64b5d7ed-0e59-4042-87fe-910e1d5a6dc7', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'Module', 'module', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('809b3e1c-0fd5-41aa-bdb4-046e83b13efc', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'ReadableName', 'readablename', 30, 400, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('eb0ef8ce-d802-409b-8c76-04e70a9180ec', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'PersistenceType', 'persistencetype', 40, 14, 'Persistable', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7f01bfc8-10d4-31fe-aed4-dc5f422d349c', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('70b941e2-8e8d-3ade-8c0d-b1048ea0645e', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('38ddb073-39b8-34fb-a922-af0365e20b3c', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'idx_mxmodelreflection$mxobjecttype_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('38ddb073-39b8-34fb-a922-af0365e20b3c', '15066120-8c96-3957-bca6-ebee368534ff', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a46daf5f-5e7f-3a8c-8bb9-b178afccbd65', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'idx_mxmodelreflection$mxobjecttype_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a46daf5f-5e7f-3a8c-8bb9-b178afccbd65', '079c5543-660b-3bda-a1da-0756763575b5', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('15066120-8c96-3957-bca6-ebee368534ff', 'System.owner', 'mxmodelreflection$mxobjecttype', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('079c5543-660b-3bda-a1da-0756763575b5', 'System.changedBy', 'mxmodelreflection$mxobjecttype', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [usercommons$claimentityattribute] (
	[id] bigint NOT NULL,
	[entitymembername] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('a648ef0c-10cd-4a08-85e5-02165439a164', 'UserCommons.ClaimEntityAttribute', 'usercommons$claimentityattribute', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('948b9b67-4be3-4434-87ef-459e4d1766a2', 'a648ef0c-10cd-4a08-85e5-02165439a164', 'EntityMemberName', 'entitymembername', 30, 200, '', 0);
GO
CREATE TABLE [excr_extentityaccess$resourcelayouts$pk] (
	[id] bigint NOT NULL,
	[instanceid] nvarchar(200) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_excr_extentityaccess$resourcelayouts$pk_instanceid] UNIQUE ([instanceid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('681becde-01b6-4fbe-958f-b9f454a788ba', 'EXCR_ExtEntityAccess.ResourceLayouts', 'excr_extentityaccess$resourcelayouts$pk', 1, 1);
GO
INSERT INTO [mendixsystem$remote_primary_key] ([id], [entity_id], [attribute_name], [column_name], [type], [length]) VALUES ('25b4f44c-e6fd-3258-bd77-587db47f58da', '681becde-01b6-4fbe-958f-b9f454a788ba', 'InstanceId', 'instanceid', 30, 200);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_extentityaccess$resourcelayouts$pk_instanceid', '681becde-01b6-4fbe-958f-b9f454a788ba', '25b4f44c-e6fd-3258-bd77-587db47f58da');
GO
CREATE TABLE [saml20$spmetadata] (
	[id] bigint NOT NULL,
	[entityid] nvarchar(200) NULL,
	[organizationname] nvarchar(200) NULL,
	[organizationdisplayname] nvarchar(200) NULL,
	[organizationurl] nvarchar(200) NULL,
	[contactgivenname] nvarchar(200) NULL,
	[contactsurname] nvarchar(200) NULL,
	[contactemailaddress] nvarchar(200) NULL,
	[applicationurl] nvarchar(200) NULL,
	[doesentityiddifferfromappurl] bit NULL,
	[logavailabledays] int NULL,
	[useencryption] bit NULL,
	[encryptionmethod] nvarchar(13) NULL,
	[encryptionkeylength] nvarchar(19) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('2125eb04-8bb1-42af-a5b2-53ead7168a69', 'SAML20.SPMetadata', 'saml20$spmetadata', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('620ec9ca-78c4-478d-b47c-f0050ae65f42', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'EntityID', 'entityid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('eb1340ec-19a8-43ff-b177-42daa2a6e6d5', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'OrganizationName', 'organizationname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e9f8a32c-8942-4c6e-b939-1f5d6b82bba2', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'OrganizationDisplayName', 'organizationdisplayname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1c298168-a7b7-463e-85c8-a5708337d929', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'OrganizationURL', 'organizationurl', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0f08020d-dacc-4e29-9c35-717c5bc888f5', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'ContactGivenName', 'contactgivenname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f0b98560-b3f5-4d95-93c7-6cd676c92477', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'ContactSurName', 'contactsurname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('026a69b0-a13c-477e-ba0c-fe9d74bcc881', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'ContactEmailAddress', 'contactemailaddress', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9800b3d3-c33d-454d-a45f-008c1af8591b', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'ApplicationURL', 'applicationurl', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('09aa1a8e-3474-410e-b23a-c2464f339b9b', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'DoesEntityIdDifferFromAppURL', 'doesentityiddifferfromappurl', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6b4ceaf8-0b86-483c-8e0f-71b3fafcfaea', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'LogAvailableDays', 'logavailabledays', 3, 0, '7', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6f82d17c-f496-479e-a098-b2a3a77dd52e', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'UseEncryption', 'useencryption', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('dc8e8a9a-e8f2-4016-b739-0e9a609e2e4d', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'EncryptionMethod', 'encryptionmethod', 40, 13, 'SHA256WithRSA', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('14b07d86-99c0-4b10-8e58-6a3c4b69945e', '2125eb04-8bb1-42af-a5b2-53ead7168a69', 'EncryptionKeyLength', 'encryptionkeylength', 40, 19, '_2048bit_Encryption', 0);
GO
CREATE TABLE [saml20$endpoint] (
	[id] bigint NOT NULL,
	[binding] nvarchar(500) NULL,
	[location] nvarchar(500) NULL,
	[responselocation] nvarchar(500) NULL,
	[index] int NULL,
	[isdefault] bit NULL,
	[servicetype] nvarchar(25) NULL,
	[bindingtype] nvarchar(29) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'SAML20.Endpoint', 'saml20$endpoint', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('db098080-3221-4865-90ad-7c15ccd3abd4', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'Binding', 'binding', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e48eb825-9715-4b6f-bdb9-e0c43688266c', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'Location', 'location', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('257a8e0f-bf3f-41e6-abd9-ff619e60eca6', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'ResponseLocation', 'responselocation', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5f253a3d-42b9-4841-a8f0-2c1f3d02314b', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'index', 'index', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4c0d3bd8-1a52-4d71-a12e-47787eb88a62', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'isDefault', 'isdefault', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4db0ca2b-41e8-433d-bbde-54528a3ad720', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'ServiceType', 'servicetype', 40, 25, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('75cd1209-b42d-4ec3-bfdc-aa5c18df524d', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'BindingType', 'bindingtype', 40, 29, '', 0);
GO
CREATE TABLE [exsm_wipdatasetup$wipdatasetupuserconfig] (
	[id] bigint NOT NULL,
	[wipdetailsmaingridconfig] nvarchar(max) NULL,
	[wipcreategridconfig] nvarchar(max) NULL,
	[wipauditmaingridconfig] nvarchar(max) NULL,
	[wipauditdetailsgridconfig] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'EXSM_WIPDataSetup.WIPDataSetupUserConfig', 'exsm_wipdatasetup$wipdatasetupuserconfig', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('98105f26-07fa-4b3d-8133-b1012ac0632b', 'd847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'WipDetailsMainGridConfig', 'wipdetailsmaingridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('97e4bf16-066b-46b9-84f2-e4873fc6bc5e', 'd847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'WipCreateGridConfig', 'wipcreategridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cbad5b3d-d29c-4b06-b64d-23e7cfdfb766', 'd847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'WipAuditMainGridConfig', 'wipauditmaingridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5b8182f3-fd5f-40cd-9e98-dc72151dae5d', 'd847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'WipAuditDetailsGridConfig', 'wipauditdetailsgridconfig', 30, 0, '', 0);
GO
CREATE TABLE [system$filedocument] (
	[id] bigint NOT NULL,
	[fileid] bigint NULL,
	[name] nvarchar(400) NULL,
	[deleteafterdownload] bit NULL,
	[contents] varbinary(max) NULL,
	[hascontents] bit NULL,
	[size] bigint NULL,
	[__filename__] bigint NULL,
	[__uuid__] nvarchar(36) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[submetaobjectname] nvarchar(255) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$filedocument_fileid_asc] ON [system$filedocument] ([fileid] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$filedocument_size_asc] ON [system$filedocument] ([size] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$filedocument___uuid___asc] ON [system$filedocument] ([__uuid__] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$filedocument_submetaobjectname_asc] ON [system$filedocument] ([submetaobjectname] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$filedocument_system$owner] ON [system$filedocument] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$filedocument_system$changedby] ON [system$filedocument] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'System.FileDocument', 'system$filedocument', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0f81688b-e719-4204-8f86-8fcd664a0992', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'FileID', 'fileid', 0, 0, '1', 1);
GO
INSERT INTO [mendixsystem$sequence] ([attribute_id], [name], [start_value], [current_value]) VALUES ('0f81688b-e719-4204-8f86-8fcd664a0992', 'system$filedocument_fileid_mxseq', 1, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3501ab9f-42c7-46e4-ac8f-c51e256c934e', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'Name', 'name', 30, 400, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5fecca4d-0f28-484d-9fe7-1afde250b07d', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'DeleteAfterDownload', 'deleteafterdownload', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e6dfb82a-54fe-4fcd-a513-a086f508c2db', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'Contents', 'contents', 50, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4c9627fb-3b64-4239-95eb-f51fb8d3f2b3', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'HasContents', 'hascontents', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('14018140-78df-4e36-9869-d0b53129d2c9', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'Size', 'size', 4, 0, '-1', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a02027b1-e24d-49fc-9b3f-ade644070879', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', '__FileName__', '__filename__', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('96445370-6fed-11e4-9803-0800200c9a66', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', '__UUID__', '__uuid__', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8655b482-0ac3-31db-8289-b05f505b77cb', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('245def96-0172-3d83-96fe-0ee1ba825f26', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b51ea62a-1838-3f74-9c5f-07b5b5a92a45', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'submetaobjectname', 'submetaobjectname', 30, 255, 'System.FileDocument', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('6ceea2cb-6acf-457a-852f-eb7deab79430', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument_fileid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('6ceea2cb-6acf-457a-852f-eb7deab79430', '0f81688b-e719-4204-8f86-8fcd664a0992', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('59de2f9e-3e30-4dd7-a5de-58594a63ea2b', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument_size_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('59de2f9e-3e30-4dd7-a5de-58594a63ea2b', '14018140-78df-4e36-9869-d0b53129d2c9', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('17f7461b-5b7c-494d-8fde-b531299c42b0', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument___uuid___asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('17f7461b-5b7c-494d-8fde-b531299c42b0', '96445370-6fed-11e4-9803-0800200c9a66', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('dfd88a6e-a3b6-3080-b6cc-d5c61334281d', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument_submetaobjectname_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('dfd88a6e-a3b6-3080-b6cc-d5c61334281d', 'b51ea62a-1838-3f74-9c5f-07b5b5a92a45', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a5c117d9-85fc-365e-9a66-909509269987', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a5c117d9-85fc-365e-9a66-909509269987', '1442c9da-d4ae-3cf5-b3c0-6c878743e4e5', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a1966801-fcea-3251-82e4-bf4178bdc504', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'idx_system$filedocument_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a1966801-fcea-3251-82e4-bf4178bdc504', '956c1382-b9fc-3367-b0b2-cb67ee9ef13f', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('1442c9da-d4ae-3cf5-b3c0-6c878743e4e5', 'System.owner', 'system$filedocument', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('956c1382-b9fc-3367-b0b2-cb67ee9ef13f', 'System.changedBy', 'system$filedocument', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [excr_extentityaccess$operations$pk] (
	[id] bigint NOT NULL,
	[instanceid] nvarchar(200) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_excr_extentityaccess$operations$pk_instanceid] UNIQUE ([instanceid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('25e486fa-9929-4e2d-8989-cf05d1436512', 'EXCR_ExtEntityAccess.Operations', 'excr_extentityaccess$operations$pk', 1, 1);
GO
INSERT INTO [mendixsystem$remote_primary_key] ([id], [entity_id], [attribute_name], [column_name], [type], [length]) VALUES ('de7cb945-fe12-3f31-9af2-8182166f58b0', '25e486fa-9929-4e2d-8989-cf05d1436512', 'InstanceId', 'instanceid', 30, 200);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_extentityaccess$operations$pk_instanceid', '25e486fa-9929-4e2d-8989-cf05d1436512', 'de7cb945-fe12-3f31-9af2-8182166f58b0');
GO
CREATE TABLE [disw_designsystem$accountprofilepicture] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('efffc7a2-c42d-4d08-abda-3dd03d539994', 'DISW_DesignSystem.AccountProfilePicture', 'disw_designsystem$accountprofilepicture', '37827192-315d-4ab6-85b8-f626f866ea76', 0, 0);
GO
CREATE TABLE [encryption$exampleconfiguration] (
	[id] bigint NOT NULL,
	[title] nvarchar(200) NULL,
	[username] nvarchar(200) NULL,
	[password] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('31a565b8-4131-42ae-ba0f-d613fb82393d', 'Encryption.ExampleConfiguration', 'encryption$exampleconfiguration', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('88624697-1f2e-4d1f-8b60-47dd7aef22de', '31a565b8-4131-42ae-ba0f-d613fb82393d', 'Title', 'title', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4267846f-035d-4bde-92ef-c614d94cd1ea', '31a565b8-4131-42ae-ba0f-d613fb82393d', 'Username', 'username', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1087ac1d-4fa7-456a-8ae1-e6f375799ec1', '31a565b8-4131-42ae-ba0f-d613fb82393d', 'Password', 'password', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cfe40b12-4eae-3700-a8dd-ff6011272925', '31a565b8-4131-42ae-ba0f-d613fb82393d', 'createdDate', 'createddate', 20, 0, '', 0);
GO
CREATE TABLE [exsm_experimentmgmt$expplanstatus] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1712c689-b512-4bd1-8df3-acdd0d1c7f2f', 'EXSM_ExperimentMgmt.ExpPlanStatus', 'exsm_experimentmgmt$expplanstatus', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('627c475d-dd77-4937-b300-e2a358985865', '1712c689-b512-4bd1-8df3-acdd0d1c7f2f', 'Name', 'name', 30, 200, '', 0);
GO
CREATE TABLE [system$session] (
	[id] bigint NOT NULL,
	[sessionid] nvarchar(50) NULL,
	[csrftoken] nvarchar(36) NULL,
	[lastactive] datetime2(3) NULL,
	[longlived] bit NULL,
	[readonlyhashkey] nvarchar(36) NULL,
	[lastactionexecution] datetime2(3) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$session_sessionid_asc] ON [system$session] ([sessionid] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('37f9fd49-5318-4c63-9a51-f761779b202f', 'System.Session', 'system$session', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d50c78a5-d740-4a76-a356-47659cfd515e', '37f9fd49-5318-4c63-9a51-f761779b202f', 'SessionId', 'sessionid', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f4d19200-071c-45e5-af25-321354f0702b', '37f9fd49-5318-4c63-9a51-f761779b202f', 'CSRFToken', 'csrftoken', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1ffdcb00-e7a4-4303-8b40-2319d9ba01b7', '37f9fd49-5318-4c63-9a51-f761779b202f', 'LastActive', 'lastactive', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('875dc581-177c-457a-8406-814676ccdb05', '37f9fd49-5318-4c63-9a51-f761779b202f', 'LongLived', 'longlived', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f1949c9c-7b28-11e6-8b77-86f30ca893d3', '37f9fd49-5318-4c63-9a51-f761779b202f', 'ReadOnlyHashKey', 'readonlyhashkey', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8b42eee4-92e5-4472-9f12-3a4e73291f2b', '37f9fd49-5318-4c63-9a51-f761779b202f', 'LastActionExecution', 'lastactionexecution', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0c2b42ef-02bd-3783-bf55-02a92f4275c7', '37f9fd49-5318-4c63-9a51-f761779b202f', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('6127a5ae-0a96-4df5-9856-17baf94b2351', '37f9fd49-5318-4c63-9a51-f761779b202f', 'idx_system$session_sessionid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('6127a5ae-0a96-4df5-9856-17baf94b2351', 'd50c78a5-d740-4a76-a356-47659cfd515e', 0, 0);
GO
CREATE TABLE [system$userreportinfo] (
	[id] bigint NOT NULL,
	[usertype] nvarchar(8) NULL,
	[hash] nvarchar(64) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('1c90a770-98ef-45df-9267-b87973cc6581', 'System.UserReportInfo', 'system$userreportinfo', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d6f4a7fc-3c2d-4793-bfc0-8dde42937863', '1c90a770-98ef-45df-9267-b87973cc6581', 'UserType', 'usertype', 40, 8, 'Internal', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e959c75a-c655-45d8-8b7d-a4335dcbf581', '1c90a770-98ef-45df-9267-b87973cc6581', 'Hash', 'hash', 30, 64, '', 0);
GO
CREATE TABLE [system$workflow] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[description] nvarchar(max) NULL,
	[starttime] datetime2(3) NULL,
	[endtime] datetime2(3) NULL,
	[duedate] datetime2(3) NULL,
	[canberestarted] bit NULL,
	[canbecontinued] bit NULL,
	[canapplyjumpto] bit NULL,
	[state] nvarchar(12) NULL,
	[reason] nvarchar(max) NULL,
	[previousstate] nvarchar(12) NULL,
	[objectid] bigint NULL,
	[processingstate] nvarchar(30) NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$workflow_system$owner] ON [system$workflow] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('2ae37bf5-ecb8-4c55-b967-d7383925b208', 'System.Workflow', 'system$workflow', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('77cf3524-fcfe-40cf-8ac0-b073015550ef', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8a6b4eb4-9b10-4060-a823-79dd4c19c217', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'Description', 'description', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c627be00-3ea1-4890-9621-d3dad9f11c21', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'StartTime', 'starttime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('59f6ed7a-8e1a-46c5-a288-c60cdd1baf50', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'EndTime', 'endtime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('80796d39-0dde-4af7-b619-53ec9950014b', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'DueDate', 'duedate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('49d88092-1ce9-46e5-baad-b6c22831824d', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'CanBeRestarted', 'canberestarted', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('dec2408c-8fea-4232-8208-cad1117ca406', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'CanBeContinued', 'canbecontinued', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('986e45cf-3a56-4836-8ece-53df4d3dcf9e', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'CanApplyJumpTo', 'canapplyjumpto', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ec48ea64-d4ae-42dd-8fbe-6c3716181dc7', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'State', 'state', 40, 12, 'InProgress', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('89e83bbd-6379-4601-89b4-825c02c7de6b', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'Reason', 'reason', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4c70166c-8ebb-4105-a35a-d1e15a82d925', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'PreviousState', 'previousstate', 40, 12, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e8722447-9692-4c59-8a28-153a4f6ebddb', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'ObjectId', 'objectid', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cbff7155-11df-47da-8619-b8bd72f40604', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'ProcessingState', 'processingstate', 30, 30, 'Ready', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('ac892dd9-fb9e-3590-aaf5-e3e2c7fbc021', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'idx_system$workflow_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('ac892dd9-fb9e-3590-aaf5-e3e2c7fbc021', '2cf6fdd7-e448-3a4d-b70e-6d875c2136d7', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('2cf6fdd7-e448-3a4d-b70e-6d875c2136d7', 'System.owner', 'system$workflow', '2ae37bf5-ecb8-4c55-b967-d7383925b208', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [system$processedqueuetask] (
	[id] bigint NOT NULL,
	[sequence] bigint NULL,
	[status] nvarchar(12) NULL,
	[queueid] nvarchar(36) NULL,
	[queuename] nvarchar(200) NULL,
	[contexttype] nvarchar(14) NULL,
	[contextdata] nvarchar(max) NULL,
	[microflowname] nvarchar(200) NULL,
	[useractionname] nvarchar(200) NULL,
	[arguments] nvarchar(max) NULL,
	[xasid] nvarchar(50) NULL,
	[threadid] bigint NULL,
	[created] datetime2(3) NULL,
	[startat] datetime2(3) NULL,
	[started] datetime2(3) NULL,
	[finished] datetime2(3) NULL,
	[duration] bigint NULL,
	[retried] bigint NULL,
	[errormessage] nvarchar(max) NULL,
	[scheduledeventname] nvarchar(200) NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$processedqueuetask_system$owner] ON [system$processedqueuetask] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'System.ProcessedQueueTask', 'system$processedqueuetask', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a1c19d5b-0798-452a-b8d9-4f60e684cc1b', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Sequence', 'sequence', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6609f51d-d19f-49df-92d8-582e78e55ba0', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Status', 'status', 40, 12, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6c8ea01f-faf8-40dd-8b81-297fbe14cda4', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'QueueId', 'queueid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('354f1eb1-13a0-4bc4-8fdf-0faad10e2b81', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'QueueName', 'queuename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8e8e6dfa-87a2-413a-89c1-a2b23037b792', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'ContextType', 'contexttype', 40, 14, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7cbcf835-1193-4d91-84f5-40bde9ddb9e4', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'ContextData', 'contextdata', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2fc91d80-70d3-4868-9332-142b1f447888', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'MicroflowName', 'microflowname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c891eb17-76d6-4600-a96f-b02ef10aa921', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'UserActionName', 'useractionname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e6f7a526-84ed-4f4e-8d32-d49d79dd5174', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Arguments', 'arguments', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e7792091-da19-4946-a07d-b11f857c4d1d', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'XASId', 'xasid', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('09951e9a-b362-499d-bf7c-68300a314110', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'ThreadId', 'threadid', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6c24f3a3-6473-4a2b-8a93-fca27764f394', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Created', 'created', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2b58adc9-b35a-4803-912d-e376a3ad89c9', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'StartAt', 'startat', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3023edea-95d3-4a8b-84e3-32a749729400', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Started', 'started', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f4f115ad-2bb9-4452-9bfb-f666afdebbb4', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Finished', 'finished', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6dab51ec-3ecc-43ac-a8d1-ba6815ad0fd7', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Duration', 'duration', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7627493b-b7d4-4afa-99e7-5957889ed081', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'Retried', 'retried', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('73d41075-6c63-4c64-92e3-ce264ddabe59', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'ErrorMessage', 'errormessage', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('526fe440-6d73-4c41-8f72-e5b5b4f2641e', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'ScheduledEventName', 'scheduledeventname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a28ff41f-1610-3353-9957-9e8d776b805e', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', 'idx_system$processedqueuetask_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a28ff41f-1610-3353-9957-9e8d776b805e', '2a2739dd-9160-3616-8999-a7a147bf4cda', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('2a2739dd-9160-3616-8999-a7a147bf4cda', 'System.owner', 'system$processedqueuetask', 'eb5c32a1-85ec-49d1-8bca-ecca779cd539', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [system$user] (
	[id] bigint NOT NULL,
	[name] nvarchar(100) NULL,
	[password] nvarchar(200) NULL,
	[lastlogin] datetime2(3) NULL,
	[blocked] bit NULL,
	[blockedsince] datetime2(3) NULL,
	[active] bit NULL,
	[failedlogins] int NULL,
	[webserviceuser] bit NULL,
	[isanonymous] bit NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[submetaobjectname] nvarchar(255) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_system$user_name] UNIQUE ([name]));
GO
CREATE INDEX [idx_system$user_name_asc] ON [system$user] ([name] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$user_submetaobjectname_asc] ON [system$user] ([submetaobjectname] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$user_system$changedby] ON [system$user] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$user_system$owner] ON [system$user] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('282e2e60-88a5-469d-84a5-ba8d9151644f', 'System.User', 'system$user', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('69acb4a2-be26-4cc5-902a-a8591d357510', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'Name', 'name', 30, 100, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ef366bc1-ac94-4fd6-bafd-7cee2be459e6', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'Password', 'password', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('040db5be-7810-48b3-a569-516191e8803d', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'LastLogin', 'lastlogin', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d149bcc3-5e80-46da-ac3f-ee734a64cce1', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'Blocked', 'blocked', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5b1e816f-0495-4baa-9d21-f1e779923898', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'BlockedSince', 'blockedsince', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b22d0982-fbee-43a7-8d20-c200d319a3e5', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'Active', 'active', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('84845531-dbd9-4e00-8afb-c2adc08699bb', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'FailedLogins', 'failedlogins', 3, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('15e3e13d-2df6-4d8a-a1cc-58eea4cec602', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'WebServiceUser', 'webserviceuser', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('28d526e0-915b-466f-80c8-56af32ece225', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'IsAnonymous', 'isanonymous', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9c09d4eb-9c9c-303e-951e-8c3ea32db37a', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e9446b4c-b0f6-3f04-8b0a-264d2384b449', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c2dd7e10-28b4-304c-9ddf-104be6be9cde', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'submetaobjectname', 'submetaobjectname', 30, 255, 'System.User', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('5711e9d5-7b67-4579-b730-2ed0b852b799', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'idx_system$user_name_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('5711e9d5-7b67-4579-b730-2ed0b852b799', '69acb4a2-be26-4cc5-902a-a8591d357510', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('d45d41fb-40ec-3b91-becf-455eb7f35bff', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'idx_system$user_submetaobjectname_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('d45d41fb-40ec-3b91-becf-455eb7f35bff', 'c2dd7e10-28b4-304c-9ddf-104be6be9cde', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('291e0cde-0e23-351e-8001-6240437d1e0e', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'idx_system$user_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('291e0cde-0e23-351e-8001-6240437d1e0e', '6013226d-aeae-3cd2-acec-d95d8bd5c3ad', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('3515901d-e8f5-3173-87bb-11ae532c243e', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'idx_system$user_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('3515901d-e8f5-3173-87bb-11ae532c243e', '07738295-23fe-3fc1-832b-ed18b22727f0', 0, 0);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$user_name', '282e2e60-88a5-469d-84a5-ba8d9151644f', '69acb4a2-be26-4cc5-902a-a8591d357510');
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('6013226d-aeae-3cd2-acec-d95d8bd5c3ad', 'System.changedBy', 'system$user', '282e2e60-88a5-469d-84a5-ba8d9151644f', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('07738295-23fe-3fc1-832b-ed18b22727f0', 'System.owner', 'system$user', '282e2e60-88a5-469d-84a5-ba8d9151644f', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [saml20$sprequestedattribute] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[isrequired] bit NULL,
	[value] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('93fbe9f3-e101-48f8-bd2f-13c7c04429b9', 'SAML20.SPRequestedAttribute', 'saml20$sprequestedattribute', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9bdc1c18-61cc-481b-b572-ebc49de4d3b2', '93fbe9f3-e101-48f8-bd2f-13c7c04429b9', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('82c4d0fd-c549-491c-89c8-a0da916fe01a', '93fbe9f3-e101-48f8-bd2f-13c7c04429b9', 'isRequired', 'isrequired', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('5055155c-f80a-4854-8db4-a2966aa7b2e2', '93fbe9f3-e101-48f8-bd2f-13c7c04429b9', 'Value', 'value', 30, 200, '', 0);
GO
CREATE TABLE [system$xasinstance] (
	[id] bigint NOT NULL,
	[xasid] nvarchar(50) NULL,
	[lastupdate] datetime2(3) NULL,
	[allowednumberofconcurrentusers] int NULL,
	[partnername] nvarchar(200) NULL,
	[customername] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d4154981-8dac-4150-aec5-efa3ef62a7a2', 'System.XASInstance', 'system$xasinstance', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('91b58eb9-c16c-4e33-b66b-28489e7fb783', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'XASId', 'xasid', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0c4060ab-4901-419c-a184-81f20fa0460e', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'LastUpdate', 'lastupdate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2dbe88f5-2b15-4ec3-b295-2e2b496a1ebd', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'AllowedNumberOfConcurrentUsers', 'allowednumberofconcurrentusers', 3, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('4359850e-675d-49db-a25c-d78ee530dc33', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'PartnerName', 'partnername', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7ca1bcd3-9355-472f-9e3f-4440366297d6', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'CustomerName', 'customername', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('65b02632-d981-3a1c-8ec5-2a36fe6fd7d8', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'createdDate', 'createddate', 20, 0, '', 0);
GO
CREATE TABLE [system$workflowusertask] (
	[id] bigint NOT NULL,
	[name] nvarchar(max) NULL,
	[description] nvarchar(max) NULL,
	[starttime] datetime2(3) NULL,
	[duedate] datetime2(3) NULL,
	[endtime] datetime2(3) NULL,
	[outcome] nvarchar(200) NULL,
	[state] nvarchar(10) NULL,
	[completiontype] nvarchar(9) NULL,
	[processingstate] nvarchar(30) NULL,
	[error] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('3729d27c-735b-457a-b210-9dffb125c3f3', 'System.WorkflowUserTask', 'system$workflowusertask', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('122c4e1e-edda-4311-85b7-2a715626b869', '3729d27c-735b-457a-b210-9dffb125c3f3', 'Name', 'name', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('544b4b9a-c5ac-4785-8efb-647a51648024', '3729d27c-735b-457a-b210-9dffb125c3f3', 'Description', 'description', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('beeda34a-8cd1-4bbe-abd3-b18a3a0ea0ef', '3729d27c-735b-457a-b210-9dffb125c3f3', 'StartTime', 'starttime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('17ae7bb0-2dea-4860-9c7b-f236aaf5a790', '3729d27c-735b-457a-b210-9dffb125c3f3', 'DueDate', 'duedate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('49503f62-1887-4823-bf94-db88a332f316', '3729d27c-735b-457a-b210-9dffb125c3f3', 'EndTime', 'endtime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('047e7010-cbc4-4bba-bf64-774fa656d010', '3729d27c-735b-457a-b210-9dffb125c3f3', 'Outcome', 'outcome', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f87a5a98-730e-4c57-b6c4-ae09cd057e65', '3729d27c-735b-457a-b210-9dffb125c3f3', 'State', 'state', 40, 10, 'Created', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7d72fc37-c5cd-425d-82d2-0b8559b28314', '3729d27c-735b-457a-b210-9dffb125c3f3', 'CompletionType', 'completiontype', 40, 9, 'Single', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('99f67785-67f1-40e0-91dc-6cdec5e2b3e5', '3729d27c-735b-457a-b210-9dffb125c3f3', 'ProcessingState', 'processingstate', 30, 30, 'Ready', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('90233d09-6c96-487e-a89e-a31f29b81bd1', '3729d27c-735b-457a-b210-9dffb125c3f3', 'Error', 'error', 30, 0, '', 0);
GO
CREATE TABLE [excr_resourcemanagement_api$reqimage] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('e62642b6-b018-4f75-bda7-d0a71592ae40', 'EXCR_ResourceManagement_API.ReqImage', 'excr_resourcemanagement_api$reqimage', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [administration$accountextension] (
	[id] bigint NOT NULL,
	[initials] nvarchar(2) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('f3d35cb5-9618-4e98-8ebb-d2e047ab404f', 'Administration.AccountExtension', 'administration$accountextension', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ee65ec15-feb0-4824-be24-3fb3ed159937', 'f3d35cb5-9618-4e98-8ebb-d2e047ab404f', 'Initials', 'initials', 30, 2, '', 0);
GO
CREATE TABLE [mxmodelreflection$mxobjectenumcaptions] (
	[id] bigint NOT NULL,
	[caption] nvarchar(200) NULL,
	[languagecode] nvarchar(8) NULL,
	[languagename] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectenumcaptions_system$owner] ON [mxmodelreflection$mxobjectenumcaptions] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectenumcaptions_system$changedby] ON [mxmodelreflection$mxobjectenumcaptions] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('c68f5bf4-27ae-4833-829d-737ff277dab6', 'MxModelReflection.MxObjectEnumCaptions', 'mxmodelreflection$mxobjectenumcaptions', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d2e889aa-b0fe-4bfa-b71e-2c91c8497c27', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'Caption', 'caption', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0767ff2e-d930-4594-90d2-e9e301ba6421', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'LanguageCode', 'languagecode', 30, 8, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e36ce42b-717a-4db7-90e6-3378465fbc2c', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'LanguageName', 'languagename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('70fc46c4-cff7-37b3-be3c-2ccf50f0cad0', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('49853d61-84d1-308a-a99d-d90ff89b277c', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('7a87cd46-4020-31d8-bbf7-b31391eadf31', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'idx_mxmodelreflection$mxobjectenumcaptions_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('7a87cd46-4020-31d8-bbf7-b31391eadf31', '9d949bdd-dc77-357e-b7db-887d515c1f47', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('5b5c327a-0569-3a8a-b4b9-b768e7ea80a3', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'idx_mxmodelreflection$mxobjectenumcaptions_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('5b5c327a-0569-3a8a-b4b9-b768e7ea80a3', '991760dd-1e9c-353d-8b74-be508c98f179', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('9d949bdd-dc77-357e-b7db-887d515c1f47', 'System.owner', 'mxmodelreflection$mxobjectenumcaptions', 'c68f5bf4-27ae-4833-829d-737ff277dab6', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('991760dd-1e9c-353d-8b74-be508c98f179', 'System.changedBy', 'mxmodelreflection$mxobjectenumcaptions', 'c68f5bf4-27ae-4833-829d-737ff277dab6', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [system$workflowactivity] (
	[id] bigint NOT NULL,
	[modelguid] nvarchar(36) NULL,
	[activityguid] nvarchar(36) NULL,
	[caption] nvarchar(max) NULL,
	[detailsjson] nvarchar(max) NULL,
	[state] nvarchar(9) NULL,
	[starttime] datetime2(3) NULL,
	[endtime] datetime2(3) NULL,
	[actiontime] datetime2(3) NULL,
	[reason] nvarchar(max) NULL,
	[activityhash] nvarchar(200) NULL,
	[isderivedactivity] bit NULL,
	[outcome] nvarchar(200) NULL,
	[outcomemodelguid] nvarchar(36) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('a5952592-bb2c-4798-9805-f9ff91ad97de', 'System.WorkflowActivity', 'system$workflowactivity', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('941e921b-8935-402e-9d93-7894c5cc9164', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'ModelGUID', 'modelguid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('11384083-d925-4b16-a625-60af27227bb4', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'ActivityGUID', 'activityguid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3236d0ea-2456-447a-b2ff-fc3b10a6ddb2', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'Caption', 'caption', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f3e0983a-dbe5-4287-af2d-9f455e847851', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'DetailsJson', 'detailsjson', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b0f8b9bd-f006-43a3-9c9f-edb70cd1642c', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'State', 'state', 40, 9, 'Started', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1c9a62fd-2e39-4fd3-92a4-748940ae67ba', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'StartTime', 'starttime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('dc169e92-887a-4fc5-a21e-51d99b41314b', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'EndTime', 'endtime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('431cc58a-fc36-427b-82e5-0751fc5fce22', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'ActionTime', 'actiontime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3b8d6bea-dfb5-497b-b2ad-c423efbd66eb', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'Reason', 'reason', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('84cfff18-42dc-4442-b783-3ca923fcde81', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'ActivityHash', 'activityhash', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('cace349b-8e30-437e-95df-c4fd4225490d', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'IsDerivedActivity', 'isderivedactivity', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a23daff4-8363-47ea-862f-4c85a29929a2', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'Outcome', 'outcome', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c39dca31-4f4a-4e52-a633-aa7840fd0894', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'OutcomeModelGUID', 'outcomemodelguid', 30, 36, '', 0);
GO
CREATE TABLE [mxmodelreflection$microflows] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[module] nvarchar(200) NULL,
	[completename] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$microflows_system$changedby] ON [mxmodelreflection$microflows] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$microflows_system$owner] ON [mxmodelreflection$microflows] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('17e6df01-1431-4547-b824-d471e84f719c', 'MxModelReflection.Microflows', 'mxmodelreflection$microflows', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('39f2a9e7-80e8-4158-9efa-22aee37ef1c2', '17e6df01-1431-4547-b824-d471e84f719c', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('77080986-0763-4353-b14d-e5911e69559e', '17e6df01-1431-4547-b824-d471e84f719c', 'Module', 'module', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1239f340-a627-44d1-a935-640d56ef3b78', '17e6df01-1431-4547-b824-d471e84f719c', 'CompleteName', 'completename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ac3af421-a7fc-3f47-b5de-41722e7f0834', '17e6df01-1431-4547-b824-d471e84f719c', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('58371d04-d104-335d-b5b6-749e5926e9e8', '17e6df01-1431-4547-b824-d471e84f719c', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('49a0d24f-ac7d-3177-a860-f625dd01fd56', '17e6df01-1431-4547-b824-d471e84f719c', 'idx_mxmodelreflection$microflows_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('49a0d24f-ac7d-3177-a860-f625dd01fd56', '6962744e-aa04-3d26-a55e-8656379b5893', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('9be4174d-7376-3fac-bab1-1d34b36e0779', '17e6df01-1431-4547-b824-d471e84f719c', 'idx_mxmodelreflection$microflows_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('9be4174d-7376-3fac-bab1-1d34b36e0779', 'c0da5a24-3273-3135-883d-35254d86f480', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('6962744e-aa04-3d26-a55e-8656379b5893', 'System.changedBy', 'mxmodelreflection$microflows', '17e6df01-1431-4547-b824-d471e84f719c', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('c0da5a24-3273-3135-883d-35254d86f480', 'System.owner', 'mxmodelreflection$microflows', '17e6df01-1431-4547-b824-d471e84f719c', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [system$queuedtask] (
	[id] bigint NOT NULL,
	[sequence] bigint NULL,
	[status] nvarchar(12) NULL,
	[queueid] nvarchar(36) NULL,
	[queuename] nvarchar(200) NULL,
	[contexttype] nvarchar(14) NULL,
	[contextdata] nvarchar(max) NULL,
	[microflowname] nvarchar(200) NULL,
	[useractionname] nvarchar(200) NULL,
	[arguments] nvarchar(max) NULL,
	[xasid] nvarchar(50) NULL,
	[threadid] bigint NULL,
	[created] datetime2(3) NULL,
	[startat] datetime2(3) NULL,
	[started] datetime2(3) NULL,
	[retried] bigint NULL,
	[retry] nvarchar(200) NULL,
	[scheduledeventname] nvarchar(200) NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$queuedtask_queueid_asc_sequence_asc] ON [system$queuedtask] ([queueid] ASC,[sequence] ASC,[id] ASC);
GO
CREATE INDEX [idx_system$queuedtask_system$owner] ON [system$queuedtask] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('c6c131c8-8779-4213-9b26-a64e141f26a8', 'System.QueuedTask', 'system$queuedtask', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d26c3f20-7dc5-4a65-bbf6-c84ec5b5fe9f', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Sequence', 'sequence', 0, 0, '1', 1);
GO
INSERT INTO [mendixsystem$sequence] ([attribute_id], [name], [start_value], [current_value]) VALUES ('d26c3f20-7dc5-4a65-bbf6-c84ec5b5fe9f', 'system$queuedtask_sequence_mxseq', 1, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2223cc2b-6f68-4964-90c4-46ceed8c2f62', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Status', 'status', 40, 12, 'Idle', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('77f66f6e-3794-4338-8a7d-eb4538dcd6db', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'QueueId', 'queueid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3898cc5e-0a5c-402a-8f4b-ffa2271fe5b5', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'QueueName', 'queuename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d6288735-aea7-416a-91d2-1735aa7c0ea3', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'ContextType', 'contexttype', 40, 14, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6534293a-7a10-451c-8b3d-a689d3a281f3', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'ContextData', 'contextdata', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('8b835017-7d42-401f-8271-02c232066d49', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'MicroflowName', 'microflowname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b38b8b74-2e97-47aa-96c6-3a14ecef522c', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'UserActionName', 'useractionname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b47c6adc-1d13-4f3b-8172-0ace129df25f', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Arguments', 'arguments', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0918f273-80a5-42d6-ae35-a7a50ab61210', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'XASId', 'xasid', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('df70e053-7126-4c45-a76b-22c2814babf8', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'ThreadId', 'threadid', 4, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('94e618c6-5158-44c1-baf6-ef6af8a76b6d', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Created', 'created', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9b8a26f0-39d8-4419-b064-c58a60be8578', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'StartAt', 'startat', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3aadbd20-595c-4fdf-84e1-2b65639f8d4b', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Started', 'started', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6bd4228f-341d-45b4-a411-765b024ccdfa', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Retried', 'retried', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a5c75f55-38b4-4061-a674-5cca84850223', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'Retry', 'retry', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1cb5f39d-db9c-4f70-ac6a-ee3c5f5a1dcf', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'ScheduledEventName', 'scheduledeventname', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('0eb7513a-a55e-4145-922a-1856104655f7', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'idx_system$queuedtask_queueid_asc_sequence_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('0eb7513a-a55e-4145-922a-1856104655f7', '77f66f6e-3794-4338-8a7d-eb4538dcd6db', 0, 0);
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('0eb7513a-a55e-4145-922a-1856104655f7', 'd26c3f20-7dc5-4a65-bbf6-c84ec5b5fe9f', 0, 1);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('8de203bb-c0c1-349b-8aef-037cdb0cd348', 'c6c131c8-8779-4213-9b26-a64e141f26a8', 'idx_system$queuedtask_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('8de203bb-c0c1-349b-8aef-037cdb0cd348', 'f6de554a-f765-3d80-aa59-2b3da4167137', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('f6de554a-f765-3d80-aa59-2b3da4167137', 'System.owner', 'system$queuedtask', 'c6c131c8-8779-4213-9b26-a64e141f26a8', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [excr_extentityaccess$specs$pk] (
	[id] bigint NOT NULL,
	[instanceid] nvarchar(200) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_excr_extentityaccess$specs$pk_instanceid] UNIQUE ([instanceid]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d7fc0858-86f8-4557-ba76-dd7bd4725db0', 'EXCR_ExtEntityAccess.Specs', 'excr_extentityaccess$specs$pk', 1, 1);
GO
INSERT INTO [mendixsystem$remote_primary_key] ([id], [entity_id], [attribute_name], [column_name], [type], [length]) VALUES ('9eb0e3b1-1fc4-31cc-96eb-39032110173c', 'd7fc0858-86f8-4557-ba76-dd7bd4725db0', 'InstanceId', 'instanceid', 30, 200);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_extentityaccess$specs$pk_instanceid', 'd7fc0858-86f8-4557-ba76-dd7bd4725db0', '9eb0e3b1-1fc4-31cc-96eb-39032110173c');
GO
CREATE TABLE [saml20$samlresponse] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('b91df9be-c86f-4037-be6b-355ad22a04ff', 'SAML20.SAMLResponse', 'saml20$samlresponse', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [saml20$ssolog] (
	[id] bigint NOT NULL,
	[message] nvarchar(max) NULL,
	[logonresult] nvarchar(7) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_saml20$ssolog_system$owner] ON [saml20$ssolog] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_saml20$ssolog_system$changedby] ON [saml20$ssolog] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('d46b9125-0254-44d0-982a-7f638d2b1909', 'SAML20.SSOLog', 'saml20$ssolog', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ca56db7b-3579-4c20-a299-b191e44e1310', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'Message', 'message', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ee3c9ed4-0358-413e-be2a-cb9c00617ab9', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'LogonResult', 'logonresult', 40, 7, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f4b8de29-2f9e-3721-a221-4ea04fb1b8ec', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2f767ecc-777b-348d-94d0-7d5a80355771', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('d44758c7-aa9e-39c5-b209-aadc447b42f7', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'idx_saml20$ssolog_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('d44758c7-aa9e-39c5-b209-aadc447b42f7', '4c52c487-ae76-3ff2-b381-12bab05c76de', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('59e81f1b-f798-3fd9-ab82-3dfa9d887f33', 'd46b9125-0254-44d0-982a-7f638d2b1909', 'idx_saml20$ssolog_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('59e81f1b-f798-3fd9-ab82-3dfa9d887f33', 'e642f693-4c7c-3ea5-bb35-0ec5296a9d0e', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('4c52c487-ae76-3ff2-b381-12bab05c76de', 'System.owner', 'saml20$ssolog', 'd46b9125-0254-44d0-982a-7f638d2b1909', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('e642f693-4c7c-3ea5-bb35-0ec5296a9d0e', 'System.changedBy', 'saml20$ssolog', 'd46b9125-0254-44d0-982a-7f638d2b1909', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [administration$accountprofilepicture] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('1e9692fd-cd51-4a4b-b09a-4764c899e10d', 'Administration.AccountProfilePicture', 'administration$accountprofilepicture', '37827192-315d-4ab6-85b8-f626f866ea76', 0, 0);
GO
CREATE TABLE [mxmodelreflection$mxobjectmember] (
	[id] bigint NOT NULL,
	[attributename] nvarchar(200) NULL,
	[attributetype] nvarchar(200) NULL,
	[attributetypeenum] nvarchar(11) NULL,
	[completename] nvarchar(400) NULL,
	[descriptivename] nvarchar(200) NULL,
	[fieldlength] int NULL,
	[isvirtual] bit NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[submetaobjectname] nvarchar(255) NULL,
	[system$changedby] bigint NULL,
	[system$owner] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectmember_submetaobjectname_asc] ON [mxmodelreflection$mxobjectmember] ([submetaobjectname] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectmember_system$changedby] ON [mxmodelreflection$mxobjectmember] ([system$changedby] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectmember_system$owner] ON [mxmodelreflection$mxobjectmember] ([system$owner] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('398a1f70-2b2c-408e-8778-f4a923fda765', 'MxModelReflection.MxObjectMember', 'mxmodelreflection$mxobjectmember', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('21e92527-a2f5-4f25-a0e0-7cbc02f15d2a', '398a1f70-2b2c-408e-8778-f4a923fda765', 'AttributeName', 'attributename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2cdadcaf-3ae1-4913-af21-19445f4edbe6', '398a1f70-2b2c-408e-8778-f4a923fda765', 'AttributeType', 'attributetype', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a28f7140-7b3f-484a-965e-0af20922f600', '398a1f70-2b2c-408e-8778-f4a923fda765', 'AttributeTypeEnum', 'attributetypeenum', 40, 11, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('25284f2d-d1de-4a40-a0bd-afe0bcbf5d6a', '398a1f70-2b2c-408e-8778-f4a923fda765', 'CompleteName', 'completename', 30, 400, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('67229c22-bd38-4097-a91b-f72a25bb5ecf', '398a1f70-2b2c-408e-8778-f4a923fda765', 'DescriptiveName', 'descriptivename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('19910e8a-c795-4c86-b8d0-ca5e5ead0af6', '398a1f70-2b2c-408e-8778-f4a923fda765', 'FieldLength', 'fieldlength', 3, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('df8843fb-e92f-498b-9344-504d072bbc20', '398a1f70-2b2c-408e-8778-f4a923fda765', 'IsVirtual', 'isvirtual', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b7caede4-02b2-3401-a8a4-6d03a079c201', '398a1f70-2b2c-408e-8778-f4a923fda765', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('381de829-fda3-39b0-95b7-2656afdaceaa', '398a1f70-2b2c-408e-8778-f4a923fda765', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('55ac1e96-8b79-38f6-a15c-7146ce4a9ae8', '398a1f70-2b2c-408e-8778-f4a923fda765', 'submetaobjectname', 'submetaobjectname', 30, 255, 'MxModelReflection.MxObjectMember', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a59e36d1-c7e6-3b81-88be-8a5b72b30d11', '398a1f70-2b2c-408e-8778-f4a923fda765', 'idx_mxmodelreflection$mxobjectmember_submetaobjectname_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a59e36d1-c7e6-3b81-88be-8a5b72b30d11', '55ac1e96-8b79-38f6-a15c-7146ce4a9ae8', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('bc494a30-8711-38b5-8b51-95176f34b50d', '398a1f70-2b2c-408e-8778-f4a923fda765', 'idx_mxmodelreflection$mxobjectmember_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('bc494a30-8711-38b5-8b51-95176f34b50d', 'c39ff4d3-8ea3-3918-91d1-0fa86ebc84a5', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('c45b816b-e177-3251-8438-42cc5acda8cb', '398a1f70-2b2c-408e-8778-f4a923fda765', 'idx_mxmodelreflection$mxobjectmember_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('c45b816b-e177-3251-8438-42cc5acda8cb', 'e6d51df0-6376-3f4d-a5f1-8eb7cda7038d', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('c39ff4d3-8ea3-3918-91d1-0fa86ebc84a5', 'System.changedBy', 'mxmodelreflection$mxobjectmember', '398a1f70-2b2c-408e-8778-f4a923fda765', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('e6d51df0-6376-3f4d-a5f1-8eb7cda7038d', 'System.owner', 'mxmodelreflection$mxobjectmember', '398a1f70-2b2c-408e-8778-f4a923fda765', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
CREATE TABLE [excr_operatorlog$doc] (
	[id] bigint NOT NULL,
	[filepath] nvarchar(200) NULL,
	[filename] nvarchar(200) NULL,
	[comments] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('d3d8e7df-c160-4db2-9f59-0541efbc663f', 'EXCR_OperatorLog.Doc', 'excr_operatorlog$doc', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('85b2aa3a-3bd2-4072-89bf-d828d76bcc6a', 'd3d8e7df-c160-4db2-9f59-0541efbc663f', 'FilePath', 'filepath', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('18353879-ec88-40fa-acd7-92cb95d07702', 'd3d8e7df-c160-4db2-9f59-0541efbc663f', 'FileName', 'filename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('82e6a7d7-82fa-44d4-813a-cbdb7b49334d', 'd3d8e7df-c160-4db2-9f59-0541efbc663f', 'Comments', 'comments', 30, 200, '', 0);
GO
CREATE TABLE [saml20$organization] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('ad6340d4-b92a-4a0c-9239-9ce623c0c372', 'SAML20.Organization', 'saml20$organization', 0, 0);
GO
CREATE TABLE [system$offlinesynchronizationhistory] (
	[id] bigint NOT NULL,
	[syncid] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	PRIMARY KEY([id]),
	CONSTRAINT [uniq_system$offlinesynchronizationhistory_syncid] UNIQUE ([syncid]));
GO
CREATE INDEX [idx_system$offlinesynchronizationhistory_syncid_asc] ON [system$offlinesynchronizationhistory] ([syncid] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('137064d4-4cf1-4a0b-92a5-a11b66360ff9', 'System.OfflineSynchronizationHistory', 'system$offlinesynchronizationhistory', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a91934fd-65f2-404e-bff8-7b370c20d687', '137064d4-4cf1-4a0b-92a5-a11b66360ff9', 'SyncId', 'syncid', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fa5f48d5-d89f-3022-bdab-059cfc5b12ec', '137064d4-4cf1-4a0b-92a5-a11b66360ff9', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('7a35f5fa-69a5-42e6-842d-c5bf65735a0f', '137064d4-4cf1-4a0b-92a5-a11b66360ff9', 'idx_system$offlinesynchronizationhistory_syncid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('7a35f5fa-69a5-42e6-842d-c5bf65735a0f', 'a91934fd-65f2-404e-bff8-7b370c20d687', 0, 0);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$offlinesynchronizationhistory_syncid', '137064d4-4cf1-4a0b-92a5-a11b66360ff9', 'a91934fd-65f2-404e-bff8-7b370c20d687');
GO
CREATE TABLE [saml20$idpmetadata] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('2e765af5-79d0-4b5c-8d04-5a7e8691b4a7', 'SAML20.IdPMetadata', 'saml20$idpmetadata', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [system$userrole] (
	[id] bigint NOT NULL,
	[modelguid] nvarchar(36) NULL,
	[name] nvarchar(100) NULL,
	[description] nvarchar(1000) NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$userrole_name_asc] ON [system$userrole] ([name] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'System.UserRole', 'system$userrole', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3cb7dc95-eac8-4999-8af4-492a4f2c0d73', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'ModelGUID', 'modelguid', 30, 36, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('3a3aca86-2f34-4038-a62f-7c0654ce21b7', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'Name', 'name', 30, 100, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a33fbc53-ecf5-46c5-bad2-a364686e19dc', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'Description', 'description', 30, 1000, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a46808f5-f89a-41eb-81e2-217968eac118', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'idx_system$userrole_name_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a46808f5-f89a-41eb-81e2-217968eac118', '3a3aca86-2f34-4038-a62f-7c0654ce21b7', 0, 0);
GO
CREATE TABLE [system$timezone] (
	[id] bigint NOT NULL,
	[code] nvarchar(50) NULL,
	[description] nvarchar(100) NULL,
	[rawoffset] int NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('7f7c72af-1ab7-4bf9-bed6-16db5c8fcf6f', 'System.TimeZone', 'system$timezone', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6abafab4-6a96-46c0-9475-b72cc4d3ffd6', '7f7c72af-1ab7-4bf9-bed6-16db5c8fcf6f', 'Code', 'code', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('954c43f0-3333-4264-813b-e8f1c8f2f0b6', '7f7c72af-1ab7-4bf9-bed6-16db5c8fcf6f', 'Description', 'description', 30, 100, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1060f919-60c9-4f90-91ee-81b4bf584bcd', '7f7c72af-1ab7-4bf9-bed6-16db5c8fcf6f', 'RawOffset', 'rawoffset', 3, 0, '', 0);
GO
CREATE TABLE [excr_inlinespc$tempspctxndata] (
	[id] bigint NOT NULL,
	[name] nvarchar(max) NULL,
	[instanceid] nvarchar(max) NULL,
	[spcresult] nvarchar(max) NULL,
	[spcresultfilename] nvarchar(max) NULL,
	[chartheight] nvarchar(max) NULL,
	[chartwidth] nvarchar(max) NULL,
	[spcfailureaction] nvarchar(max) NULL,
	[htmlcontent] nvarchar(max) NULL,
	[serverurl] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'EXCR_InlineSPC.TempSPCTxnData', 'excr_inlinespc$tempspctxndata', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e9f549ed-ecbd-41a9-8127-0b008601d3be', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'Name', 'name', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('b826c629-2379-4b4a-a7a3-46ac477fa497', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'InstanceId', 'instanceid', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e0e0966d-15ae-4516-894d-06470f9f3f34', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'SPCResult', 'spcresult', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('bc561c18-b432-4e18-8d27-d292ecbd1040', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'SPCResultFilename', 'spcresultfilename', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c7014938-9316-470f-b36a-2399735912f3', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'ChartHeight', 'chartheight', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('1cd4fe54-e3f6-4700-84df-4a21888002f3', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'ChartWidth', 'chartwidth', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0c193537-c725-46e1-88f9-6ca97d338f00', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'SPCFailureAction', 'spcfailureaction', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('db5452ca-5618-4460-9688-783be1848ef8', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'htmlContent', 'htmlcontent', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2d424426-b231-40e4-9e69-595969aaca4a', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', 'serverURL', 'serverurl', 30, 200, '', 0);
GO
CREATE TABLE [system$taskqueuetoken] (
	[id] bigint NOT NULL,
	[queuename] nvarchar(200) NULL,
	[xasid] nvarchar(50) NULL,
	[validuntil] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('bb60ef05-6d17-48ad-a4ef-559310c30c5b', 'System.TaskQueueToken', 'system$taskqueuetoken', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f1c72f88-9feb-409f-b592-385be28eed47', 'bb60ef05-6d17-48ad-a4ef-559310c30c5b', 'QueueName', 'queuename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('46add4ce-cd71-4db0-8fc7-0d1ab16954fa', 'bb60ef05-6d17-48ad-a4ef-559310c30c5b', 'XASId', 'xasid', 30, 50, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c05017ba-1faa-4f63-9d4c-06918a947700', 'bb60ef05-6d17-48ad-a4ef-559310c30c5b', 'ValidUntil', 'validuntil', 20, 0, '', 0);
GO
CREATE TABLE [excr_mfgordermanagement_api$imageconvert] (
	[id] bigint NOT NULL,
	[output] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('8dc2bd21-43d7-4b36-b364-75134f015556', 'EXCR_MfgOrderManagement_API.ImageConvert', 'excr_mfgordermanagement_api$imageconvert', '37827192-315d-4ab6-85b8-f626f866ea76', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2d02764c-35b4-4aca-894f-1f0b799cc8a8', '8dc2bd21-43d7-4b36-b364-75134f015556', 'Output', 'output', 30, 0, '', 0);
GO
CREATE TABLE [excr_resourcemanagement$dhr] (
	[id] bigint NOT NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('b19cdf2b-50bf-48a1-96b4-820d636183fc', 'EXCR_ResourceManagement.DHR', 'excr_resourcemanagement$dhr', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
CREATE TABLE [system$image] (
	[id] bigint NOT NULL,
	[publicthumbnailpath] nvarchar(500) NULL,
	[enablecaching] bit NULL,
	[submetaobjectname] nvarchar(255) NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$image_submetaobjectname_asc] ON [system$image] ([submetaobjectname] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('37827192-315d-4ab6-85b8-f626f866ea76', 'System.Image', 'system$image', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('41c141fa-700b-44de-870d-1aa971e11689', '37827192-315d-4ab6-85b8-f626f866ea76', 'PublicThumbnailPath', 'publicthumbnailpath', 30, 500, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0a5f1064-838e-4b91-a9c3-904428d203d2', '37827192-315d-4ab6-85b8-f626f866ea76', 'EnableCaching', 'enablecaching', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('272f49fe-6a64-3ede-a32d-344a34e57b9f', '37827192-315d-4ab6-85b8-f626f866ea76', 'submetaobjectname', 'submetaobjectname', 30, 255, 'System.Image', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('00a9ec1c-4fab-368f-83d4-ffa8ff501c8c', '37827192-315d-4ab6-85b8-f626f866ea76', 'idx_system$image_submetaobjectname_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('00a9ec1c-4fab-368f-83d4-ffa8ff501c8c', '272f49fe-6a64-3ede-a32d-344a34e57b9f', 0, 0);
GO
CREATE TABLE [saml20$nameidformat] (
	[id] bigint NOT NULL,
	[description] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('ca71a18f-d692-43d9-80cb-ee92e8fa4c5e', 'SAML20.NameIDFormat', 'saml20$nameidformat', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a180db92-eada-4333-bf53-b56126f6bac8', 'ca71a18f-d692-43d9-80cb-ee92e8fa4c5e', 'Description', 'description', 30, 200, '', 0);
GO
CREATE TABLE [usercommons$claim] (
	[id] bigint NOT NULL,
	[name] nvarchar(200) NULL,
	[friendlyname] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('ea6ca13e-3ef5-44aa-b22a-60f69fb100bd', 'UserCommons.Claim', 'usercommons$claim', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('dc27c47c-3434-4ba8-acc3-d23cf43a9043', 'ea6ca13e-3ef5-44aa-b22a-60f69fb100bd', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c9bce217-3a5c-41cf-b70c-dfae75dd6214', 'ea6ca13e-3ef5-44aa-b22a-60f69fb100bd', 'FriendlyName', 'friendlyname', 30, 200, '', 0);
GO
CREATE TABLE [exsm_futureholdsetup$futureholduserconfig] (
	[id] bigint NOT NULL,
	[futureholdauditfieldgridconfig] nvarchar(max) NULL,
	[futureholdauditgridconfig] nvarchar(max) NULL,
	[futureholdauditmaingridconfig] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('baba0a15-413d-40c7-867b-e36b80208457', 'EXSM_FutureHoldSetup.FutureHoldUserConfig', 'exsm_futureholdsetup$futureholduserconfig', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d88d42ea-720e-4401-a2cf-8d0dc5213700', 'baba0a15-413d-40c7-867b-e36b80208457', 'FutureHoldAuditFieldGridConfig', 'futureholdauditfieldgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('53dd561f-8d95-4946-9419-2f78f6912d0f', 'baba0a15-413d-40c7-867b-e36b80208457', 'FutureHoldAuditGridConfig', 'futureholdauditgridconfig', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('51129dc2-48f0-40ae-bd0c-ace7463cf8df', 'baba0a15-413d-40c7-867b-e36b80208457', 'FutureHoldAuditMainGridConfig', 'futureholdauditmaingridconfig', 30, 0, '', 0);
GO
CREATE TABLE [excr_commons$configuration] (
	[id] bigint NOT NULL,
	[serverurl] nvarchar(max) NULL,
	[accesstoken] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('0593fd24-73a3-48c4-b29e-c9b1009278bd', 'EXCR_Commons.Configuration', 'excr_commons$configuration', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('70307dc4-79af-4a90-af21-278627959b78', '0593fd24-73a3-48c4-b29e-c9b1009278bd', 'ServerURL', 'serverurl', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c33eaccd-911b-45dd-926b-f01ee18bf76a', '0593fd24-73a3-48c4-b29e-c9b1009278bd', 'AccessToken', 'accesstoken', 30, 0, '', 0);
GO
CREATE TABLE [mxmodelreflection$mxobjectreference] (
	[id] bigint NOT NULL,
	[completename] nvarchar(200) NULL,
	[module] nvarchar(200) NULL,
	[name] nvarchar(200) NULL,
	[readablename] nvarchar(200) NULL,
	[referencetype] nvarchar(12) NULL,
	[associationowner] nvarchar(8) NULL,
	[parententity] nvarchar(200) NULL,
	[createddate] datetime2(3) NULL,
	[changeddate] datetime2(3) NULL,
	[system$owner] bigint NULL,
	[system$changedby] bigint NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_system$owner] ON [mxmodelreflection$mxobjectreference] ([system$owner] ASC,[id] ASC);
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_system$changedby] ON [mxmodelreflection$mxobjectreference] ([system$changedby] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('057a1e77-1c44-46fa-9eef-6809c800641f', 'MxModelReflection.MxObjectReference', 'mxmodelreflection$mxobjectreference', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d7d1efa2-e812-4987-a07b-521e0f84ed0a', '057a1e77-1c44-46fa-9eef-6809c800641f', 'CompleteName', 'completename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('0bbca4fa-11c6-42c1-b50c-9482e4d92a25', '057a1e77-1c44-46fa-9eef-6809c800641f', 'Module', 'module', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('74e6d92c-6e84-4020-8878-a0b0c3962bd6', '057a1e77-1c44-46fa-9eef-6809c800641f', 'Name', 'name', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('684fac64-3922-48b3-a0b8-5a7b3aab4b07', '057a1e77-1c44-46fa-9eef-6809c800641f', 'ReadableName', 'readablename', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('46b85aaa-3832-4384-a6a8-91940d287f73', '057a1e77-1c44-46fa-9eef-6809c800641f', 'ReferenceType', 'referencetype', 40, 12, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('473e3229-9e37-4381-8e12-e283b467b798', '057a1e77-1c44-46fa-9eef-6809c800641f', 'AssociationOwner', 'associationowner', 40, 8, '_Default', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f159123a-2b8b-4cf5-8971-4467da352a1b', '057a1e77-1c44-46fa-9eef-6809c800641f', 'ParentEntity', 'parententity', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('aafe0247-63e5-39e0-984b-73b1190724b8', '057a1e77-1c44-46fa-9eef-6809c800641f', 'createdDate', 'createddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f26939b3-a009-3b82-8e05-3a18982c4fa6', '057a1e77-1c44-46fa-9eef-6809c800641f', 'changedDate', 'changeddate', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('813acfdf-4bc1-3534-830b-19d028a275b5', '057a1e77-1c44-46fa-9eef-6809c800641f', 'idx_mxmodelreflection$mxobjectreference_system$owner');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('813acfdf-4bc1-3534-830b-19d028a275b5', '18be4865-dd78-30a5-9363-f0dcae89d11b', 0, 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('caf6e3be-2874-33e6-8846-d3851420a807', '057a1e77-1c44-46fa-9eef-6809c800641f', 'idx_mxmodelreflection$mxobjectreference_system$changedby');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('caf6e3be-2874-33e6-8846-d3851420a807', '7cba83a9-e4d8-32ab-896e-c74d7cc4dcda', 0, 0);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('18be4865-dd78-30a5-9363-f0dcae89d11b', 'System.owner', 'mxmodelreflection$mxobjectreference', '057a1e77-1c44-46fa-9eef-6809c800641f', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$owner', 1);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [storage_format]) VALUES ('7cba83a9-e4d8-32ab-896e-c74d7cc4dcda', 'System.changedBy', 'mxmodelreflection$mxobjectreference', '057a1e77-1c44-46fa-9eef-6809c800641f', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'id', 'system$changedby', 1);
GO
CREATE TABLE [excr_mfgordermanagement$imageconvert] (
	[id] bigint NOT NULL,
	[output] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('8a886ab4-f649-4007-a8d8-843f83242d2a', 'EXCR_MfgOrderManagement.ImageConvert', 'excr_mfgordermanagement$imageconvert', '37827192-315d-4ab6-85b8-f626f866ea76', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7cfa7908-a3d9-44dd-9327-ff40b42f8414', '8a886ab4-f649-4007-a8d8-843f83242d2a', 'Output', 'output', 30, 0, '', 0);
GO
CREATE TABLE [excr_commons$sessionvalues] (
	[id] bigint NOT NULL,
	[factory] nvarchar(200) NULL,
	[workcenter] nvarchar(200) NULL,
	[operation] nvarchar(200) NULL,
	[spec] nvarchar(200) NULL,
	[resource] nvarchar(200) NULL,
	[workstation] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('5ff1a85a-2331-493b-a227-3bfce73f148f', 'EXCR_Commons.SessionValues', 'excr_commons$sessionvalues', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('991089c3-7da3-4ac1-9d3c-30da7d9de014', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'Factory', 'factory', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('60636500-f64d-45ef-81bf-b5b0756d0733', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'WorkCenter', 'workcenter', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('504c3af7-72d1-4010-a60c-65f026455fca', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'Operation', 'operation', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d897cbcf-bf43-41b8-b972-39e8dff4b532', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'Spec', 'spec', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9fbef687-e850-44b1-853a-70baf8c38961', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'Resource', 'resource', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('879370e6-975c-40eb-a053-71d646f8e95a', '5ff1a85a-2331-493b-a227-3bfce73f148f', 'Workstation', 'workstation', 30, 200, '', 0);
GO
CREATE TABLE [saml20$serviceproperty] (
	[id] bigint NOT NULL,
	[_content_] nvarchar(200) NULL,
	[lang] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('cc40cb4e-50ad-4df6-a982-ff2a2e5732f1', 'SAML20.ServiceProperty', 'saml20$serviceproperty', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('fa6c152d-d34d-4634-9e84-4fe50b48bd0e', 'cc40cb4e-50ad-4df6-a982-ff2a2e5732f1', '_content_', '_content_', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('e230e60c-1d89-40d2-a1e5-e4b5d817aab3', 'cc40cb4e-50ad-4df6-a982-ff2a2e5732f1', 'lang', 'lang', 30, 200, '', 0);
GO
CREATE TABLE [usercommons$userprovisioning] (
	[id] bigint NOT NULL,
	[allowcreateusers] bit NULL,
	[usertype] nvarchar(8) NULL,
	[customentity] nvarchar(200) NULL,
	[customentitymember] nvarchar(200) NULL,
	[customuserprovisioning] nvarchar(200) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('7e717f5c-e099-43e0-9cb7-738cb027cff0', 'UserCommons.UserProvisioning', 'usercommons$userprovisioning', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('ec164b27-f724-4d15-84ca-7110cfe9f22e', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'AllowCreateUsers', 'allowcreateusers', 10, 0, 'true', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('98b35366-bad0-4540-8bb0-cd8e01d6c58c', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'UserType', 'usertype', 40, 8, 'Internal', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('f3b99419-d831-467a-a3ad-a01529f2f3af', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'CustomEntity', 'customentity', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('6a5a1ccf-f06a-41a8-a358-d95695dbcf35', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'CustomEntityMember', 'customentitymember', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('7ba12c57-ebf1-478e-bd90-e920ab8919f8', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'CustomUserProvisioning', 'customuserprovisioning', 30, 200, '', 0);
GO
CREATE TABLE [system$backgroundjob] (
	[id] bigint NOT NULL,
	[jobid] bigint NULL,
	[starttime] datetime2(3) NULL,
	[endtime] datetime2(3) NULL,
	[result] nvarchar(max) NULL,
	[successful] bit NULL,
	PRIMARY KEY([id]));
GO
CREATE INDEX [idx_system$backgroundjob_jobid_asc] ON [system$backgroundjob] ([jobid] ASC,[id] ASC);
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('660db38b-5ab4-4d15-b649-93a947ecea82', 'System.BackgroundJob', 'system$backgroundjob', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('97bcc327-4d2b-4a28-a57a-7e7437416bfe', '660db38b-5ab4-4d15-b649-93a947ecea82', 'JobId', 'jobid', 4, 0, '0', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d5aa1ceb-6dfe-457b-afb7-e969a814eafd', '660db38b-5ab4-4d15-b649-93a947ecea82', 'StartTime', 'starttime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9c471b02-f266-4e27-9e2f-907d0fad6552', '660db38b-5ab4-4d15-b649-93a947ecea82', 'EndTime', 'endtime', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c33867e7-7263-4ce6-826a-714e7493f07a', '660db38b-5ab4-4d15-b649-93a947ecea82', 'Result', 'result', 30, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('c56553ba-a3d4-4d9e-bc81-e61ee64da79b', '660db38b-5ab4-4d15-b649-93a947ecea82', 'Successful', 'successful', 10, 0, 'false', 0);
GO
INSERT INTO [mendixsystem$index] ([id], [table_id], [index_name]) VALUES ('a5ff48ca-56d6-4f43-8e3a-7743fd025974', '660db38b-5ab4-4d15-b649-93a947ecea82', 'idx_system$backgroundjob_jobid_asc');
GO
INSERT INTO [mendixsystem$index_column] ([index_id], [column_id], [sort_order], [ordinal]) VALUES ('a5ff48ca-56d6-4f43-8e3a-7743fd025974', '97bcc327-4d2b-4a28-a57a-7e7437416bfe', 0, 0);
GO
CREATE TABLE [excr_resourcemanagement$dhrholder] (
	[id] bigint NOT NULL,
	[container] nvarchar(200) NULL,
	[search] nvarchar(200) NULL,
	[datestart] datetime2(3) NULL,
	[dateend] datetime2(3) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [remote], [remote_primary_key]) VALUES ('28e1dc1d-2ea1-4dda-a591-9891235815b8', 'EXCR_ResourceManagement.DHRHolder', 'excr_resourcemanagement$dhrholder', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('9be6ce37-9d33-4831-8c7f-4f0d2d5914a6', '28e1dc1d-2ea1-4dda-a591-9891235815b8', 'container', 'container', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('da0d54bf-1607-4a8b-8ae3-3c9f553ecdea', '28e1dc1d-2ea1-4dda-a591-9891235815b8', 'search', 'search', 30, 200, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('a5056b4a-a1e9-49e8-ab14-61b15de8c52f', '28e1dc1d-2ea1-4dda-a591-9891235815b8', 'dateStart', 'datestart', 20, 0, '', 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('2cbe2876-2231-4581-bae7-02cea5b1654c', '28e1dc1d-2ea1-4dda-a591-9891235815b8', 'dateEnd', 'dateend', 20, 0, '', 0);
GO
CREATE TABLE [excr_mfgordermanagement$tasklistimageconvert] (
	[id] bigint NOT NULL,
	[output] nvarchar(max) NULL,
	PRIMARY KEY([id]));
GO
INSERT INTO [mendixsystem$entity] ([id], [entity_name], [table_name], [superentity_id], [remote], [remote_primary_key]) VALUES ('a56c36ed-533d-4a48-8f28-adc726c25242', 'EXCR_MfgOrderManagement.TasklistImageConvert', 'excr_mfgordermanagement$tasklistimageconvert', '37827192-315d-4ab6-85b8-f626f866ea76', 0, 0);
GO
INSERT INTO [mendixsystem$attribute] ([id], [entity_id], [attribute_name], [column_name], [type], [length], [default_value], [is_auto_number]) VALUES ('d5d756cd-185a-48b6-b68a-3a8f06b43143', 'a56c36ed-533d-4a48-8f28-adc726c25242', 'Output', 'output', 30, 0, '', 0);
GO
CREATE TABLE [saml20$attribute_idpmetadata] (
	[saml20$attributeid] bigint NOT NULL,
	[saml20$idpmetadataid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeid],[saml20$idpmetadataid]),
	CONSTRAINT [uniq_saml20$attribute_idpmetadata_saml20$attributeid] UNIQUE ([saml20$attributeid]));
GO
CREATE INDEX [idx_saml20$attribute_idpmetadata_saml20$idpmetadata_saml20$attribute] ON [saml20$attribute_idpmetadata] ([saml20$idpmetadataid] ASC,[saml20$attributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('66bcb582-880c-470d-8559-c65d0fec8b67', 'SAML20.Attribute_IdPMetadata', 'saml20$attribute_idpmetadata', '1fe71b95-a95e-4ddd-a9f9-501182b13749', '2e765af5-79d0-4b5c-8d04-5a7e8691b4a7', 'saml20$attributeid', 'saml20$idpmetadataid', 'idx_saml20$attribute_idpmetadata_saml20$idpmetadata_saml20$attribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$attribute_idpmetadata_saml20$attributeid', '66bcb582-880c-470d-8559-c65d0fec8b67', '4812d337-b173-3de8-b604-11492ff24146');
GO
CREATE TABLE [saml20$attribute_roledescriptor] (
	[saml20$attributeid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$attribute_roledescriptor_saml20$attributeid] UNIQUE ([saml20$attributeid]));
GO
CREATE INDEX [idx_saml20$attribute_roledescriptor_saml20$roledescriptor_saml20$attribute] ON [saml20$attribute_roledescriptor] ([saml20$roledescriptorid] ASC,[saml20$attributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('881518df-0033-4f92-b155-f77d8e943292', 'SAML20.Attribute_RoleDescriptor', 'saml20$attribute_roledescriptor', '1fe71b95-a95e-4ddd-a9f9-501182b13749', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$attributeid', 'saml20$roledescriptorid', 'idx_saml20$attribute_roledescriptor_saml20$roledescriptor_saml20$attribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$attribute_roledescriptor_saml20$attributeid', '881518df-0033-4f92-b155-f77d8e943292', '9db85d58-2b4b-3d97-8917-15016371bd65');
GO
CREATE TABLE [saml20$attribute_attributeconsumingservice] (
	[saml20$attributeid] bigint NOT NULL,
	[saml20$attributeconsumingserviceid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeid],[saml20$attributeconsumingserviceid]),
	CONSTRAINT [uniq_saml20$attribute_attributeconsumingservice_saml20$attributeid] UNIQUE ([saml20$attributeid]));
GO
CREATE INDEX [idx_saml20$attribute_attributeconsumingservice_saml20$attributeconsumingservice_saml20$attribute] ON [saml20$attribute_attributeconsumingservice] ([saml20$attributeconsumingserviceid] ASC,[saml20$attributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('c1961e51-e07e-4338-a9f4-c3b3848086ff', 'SAML20.Attribute_AttributeConsumingService', 'saml20$attribute_attributeconsumingservice', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'saml20$attributeid', 'saml20$attributeconsumingserviceid', 'idx_saml20$attribute_attributeconsumingservice_saml20$attributeconsumingservice_saml20$attribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$attribute_attributeconsumingservice_saml20$attributeid', 'c1961e51-e07e-4338-a9f4-c3b3848086ff', '888b3104-b1c4-3238-bbf9-4f333df35d81');
GO
CREATE TABLE [saml20$contacttype_telephonenumber] (
	[saml20$contactpropertyid] bigint NOT NULL,
	[saml20$contactid] bigint NOT NULL,
	PRIMARY KEY([saml20$contactpropertyid],[saml20$contactid]),
	CONSTRAINT [uniq_saml20$contacttype_telephonenumber_saml20$contactpropertyid] UNIQUE ([saml20$contactpropertyid]));
GO
CREATE INDEX [idx_saml20$contacttype_telephonenumber_saml20$contact_saml20$contactproperty] ON [saml20$contacttype_telephonenumber] ([saml20$contactid] ASC,[saml20$contactpropertyid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('5063ee9d-0c67-4ca7-b1b5-8e0bc41cc5c9', 'SAML20.ContactType_TelephoneNumber', 'saml20$contacttype_telephonenumber', '2ef9b190-f184-4d26-af23-ef27d8caef1a', '3cfaa082-c97b-4635-a503-8806c914525f', 'saml20$contactpropertyid', 'saml20$contactid', 'idx_saml20$contacttype_telephonenumber_saml20$contact_saml20$contactproperty', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$contacttype_telephonenumber_saml20$contactpropertyid', '5063ee9d-0c67-4ca7-b1b5-8e0bc41cc5c9', '41bd13c3-54fb-3025-8642-5daf6d784fde');
GO
CREATE TABLE [saml20$contacttype_emailaddress] (
	[saml20$contactpropertyid] bigint NOT NULL,
	[saml20$contactid] bigint NOT NULL,
	PRIMARY KEY([saml20$contactpropertyid],[saml20$contactid]),
	CONSTRAINT [uniq_saml20$contacttype_emailaddress_saml20$contactpropertyid] UNIQUE ([saml20$contactpropertyid]));
GO
CREATE INDEX [idx_saml20$contacttype_emailaddress_saml20$contact_saml20$contactproperty] ON [saml20$contacttype_emailaddress] ([saml20$contactid] ASC,[saml20$contactpropertyid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('ca28ceeb-ad96-4132-9d9e-ef76125c3c71', 'SAML20.ContactType_EmailAddress', 'saml20$contacttype_emailaddress', '2ef9b190-f184-4d26-af23-ef27d8caef1a', '3cfaa082-c97b-4635-a503-8806c914525f', 'saml20$contactpropertyid', 'saml20$contactid', 'idx_saml20$contacttype_emailaddress_saml20$contact_saml20$contactproperty', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$contacttype_emailaddress_saml20$contactpropertyid', 'ca28ceeb-ad96-4132-9d9e-ef76125c3c71', 'af129c18-5ab7-3c1e-8667-3c88d01a3345');
GO
CREATE TABLE [system$changehash_session] (
	[system$changehashid] bigint NOT NULL,
	[system$sessionid] bigint NOT NULL,
	PRIMARY KEY([system$changehashid],[system$sessionid]),
	CONSTRAINT [uniq_system$changehash_session_system$changehashid] UNIQUE ([system$changehashid]));
GO
CREATE INDEX [idx_system$changehash_session_system$session_system$changehash] ON [system$changehash_session] ([system$sessionid] ASC,[system$changehashid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('1aa853ae-4871-4f54-9716-af1e36b6b031', 'System.ChangeHash_Session', 'system$changehash_session', '24f72d72-3c66-46e4-a08b-09daf0f451d8', '37f9fd49-5318-4c63-9a51-f761779b202f', 'system$changehashid', 'system$sessionid', 'idx_system$changehash_session_system$session_system$changehash', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$changehash_session_system$changehashid', '1aa853ae-4871-4f54-9716-af1e36b6b031', 'e4d85c0c-652d-3e54-859c-3b0f1db86277');
GO
CREATE TABLE [system$workflowversion_workflowusertaskdefinition] (
	[system$workflowversionid] bigint NOT NULL,
	[system$workflowusertaskdefinitionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowversionid],[system$workflowusertaskdefinitionid]));
GO
CREATE INDEX [idx_system$workflowversion_workflowusertaskdefinition_system$workflowusertaskdefinition_system$workflowversion] ON [system$workflowversion_workflowusertaskdefinition] ([system$workflowusertaskdefinitionid] ASC,[system$workflowversionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('3348e396-6643-4a5b-bcb1-a939cdcdf435', 'System.WorkflowVersion_WorkflowUserTaskDefinition', 'system$workflowversion_workflowusertaskdefinition', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'e09e866f-288b-475c-9465-792cde8b878c', 'system$workflowversionid', 'system$workflowusertaskdefinitionid', 'idx_system$workflowversion_workflowusertaskdefinition_system$workflowusertaskdefinition_system$workflowversion', 2);
GO
CREATE TABLE [system$workflowversion_previousversion] (
	[system$workflowversionid1] bigint NOT NULL,
	[system$workflowversionid2] bigint NOT NULL,
	PRIMARY KEY([system$workflowversionid1],[system$workflowversionid2]),
	CONSTRAINT [uniq_system$workflowversion_previousversion_system$workflowversionid1] UNIQUE ([system$workflowversionid1]));
GO
CREATE INDEX [idx_system$workflowversion_previousversion_system$workflowversion_system$workflowversion] ON [system$workflowversion_previousversion] ([system$workflowversionid2] ASC,[system$workflowversionid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('61a2af90-0720-41a0-bea9-8a3d60de71d0', 'System.WorkflowVersion_PreviousVersion', 'system$workflowversion_previousversion', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'system$workflowversionid1', 'system$workflowversionid2', 'idx_system$workflowversion_previousversion_system$workflowversion_system$workflowversion', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowversion_previousversion_system$workflowversionid1', '61a2af90-0720-41a0-bea9-8a3d60de71d0', 'a0f7479f-37c0-39ac-9fb9-5589bd8627c5');
GO
CREATE TABLE [system$workflowversion_workflowdefinition] (
	[system$workflowversionid] bigint NOT NULL,
	[system$workflowdefinitionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowversionid],[system$workflowdefinitionid]),
	CONSTRAINT [uniq_system$workflowversion_workflowdefinition_system$workflowversionid] UNIQUE ([system$workflowversionid]));
GO
CREATE INDEX [idx_system$workflowversion_workflowdefinition_system$workflowdefinition_system$workflowversion] ON [system$workflowversion_workflowdefinition] ([system$workflowdefinitionid] ASC,[system$workflowversionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('c063e3e7-a440-47f0-8065-6fac7c723690', 'System.WorkflowVersion_WorkflowDefinition', 'system$workflowversion_workflowdefinition', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'system$workflowversionid', 'system$workflowdefinitionid', 'idx_system$workflowversion_workflowdefinition_system$workflowdefinition_system$workflowversion', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowversion_workflowdefinition_system$workflowversionid', 'c063e3e7-a440-47f0-8065-6fac7c723690', '47c7ac0f-8b15-3178-ae6c-8b0cb0debb61');
GO
CREATE TABLE [system$workflowactivityusertaskoutcome_workflowactivity] (
	[system$workflowactivityusertaskoutcomeid] bigint NOT NULL,
	[system$workflowactivityid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityusertaskoutcomeid],[system$workflowactivityid]),
	CONSTRAINT [uniq_system$workflowactivityusertaskoutcome_workflowactivity_system$workflowactivityusertaskoutcomeid] UNIQUE ([system$workflowactivityusertaskoutcomeid]));
GO
CREATE INDEX [idx_system$workflowactivityusertaskoutcome_workflowactivity_system$workflowactivity_system$workflowactivityusertaskoutcome] ON [system$workflowactivityusertaskoutcome_workflowactivity] ([system$workflowactivityid] ASC,[system$workflowactivityusertaskoutcomeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('9ce50251-ec5a-46d0-ba7e-cb91876078f0', 'System.WorkflowActivityUserTaskOutcome_WorkflowActivity', 'system$workflowactivityusertaskoutcome_workflowactivity', '1ebda4ad-6e00-4b19-8b95-6b6261beb937', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'system$workflowactivityusertaskoutcomeid', 'system$workflowactivityid', 'idx_system$workflowactivityusertaskoutcome_workflowactivity_system$workflowactivity_system$workflowactivityusertaskoutcome', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivityusertaskoutcome_workflowactivity_system$workflowactivityusertaskoutcomeid', '9ce50251-ec5a-46d0-ba7e-cb91876078f0', 'ced1f439-bb5d-34eb-9344-68a02c7877e7');
GO
CREATE TABLE [system$workflowactivityusertaskoutcome_user] (
	[system$workflowactivityusertaskoutcomeid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityusertaskoutcomeid],[system$userid]),
	CONSTRAINT [uniq_system$workflowactivityusertaskoutcome_user_system$workflowactivityusertaskoutcomeid] UNIQUE ([system$workflowactivityusertaskoutcomeid]));
GO
CREATE INDEX [idx_system$workflowactivityusertaskoutcome_user_system$user_system$workflowactivityusertaskoutcome] ON [system$workflowactivityusertaskoutcome_user] ([system$userid] ASC,[system$workflowactivityusertaskoutcomeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('69f85619-f30b-427c-adb1-7be7ce8cc3dd', 'System.WorkflowActivityUserTaskOutcome_User', 'system$workflowactivityusertaskoutcome_user', '1ebda4ad-6e00-4b19-8b95-6b6261beb937', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$workflowactivityusertaskoutcomeid', 'system$userid', 'idx_system$workflowactivityusertaskoutcome_user_system$user_system$workflowactivityusertaskoutcome', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivityusertaskoutcome_user_system$workflowactivityusertaskoutcomeid', '69f85619-f30b-427c-adb1-7be7ce8cc3dd', '0e354480-b4c1-3655-9f12-e34dc1488c2f');
GO
CREATE TABLE [saml20$entitydescriptor_entitiesdescriptor] (
	[saml20$entitydescriptorid] bigint NOT NULL,
	[saml20$entitiesdescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$entitydescriptorid],[saml20$entitiesdescriptorid]),
	CONSTRAINT [uniq_saml20$entitydescriptor_entitiesdescriptor_saml20$entitydescriptorid] UNIQUE ([saml20$entitydescriptorid]));
GO
CREATE INDEX [idx_saml20$entitydescriptor_entitiesdescriptor_saml20$entitiesdescriptor_saml20$entitydescriptor] ON [saml20$entitydescriptor_entitiesdescriptor] ([saml20$entitiesdescriptorid] ASC,[saml20$entitydescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('22f4f592-0452-4792-a4f1-b189e1e1b551', 'SAML20.EntityDescriptor_EntitiesDescriptor', 'saml20$entitydescriptor_entitiesdescriptor', '50527e51-152a-40ca-98fa-75a0b24193bb', '41287618-1d36-43d4-ba70-d34884a5017d', 'saml20$entitydescriptorid', 'saml20$entitiesdescriptorid', 'idx_saml20$entitydescriptor_entitiesdescriptor_saml20$entitiesdescriptor_saml20$entitydescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$entitydescriptor_entitiesdescriptor_saml20$entitydescriptorid', '22f4f592-0452-4792-a4f1-b189e1e1b551', '1074783b-8d72-30fa-9923-5d6b3451cb70');
GO
CREATE TABLE [saml20$keydescriptor_roledescriptor] (
	[saml20$keydescriptorid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$keydescriptorid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$keydescriptor_roledescriptor_saml20$keydescriptorid] UNIQUE ([saml20$keydescriptorid]));
GO
CREATE INDEX [idx_saml20$keydescriptor_roledescriptor_saml20$roledescriptor_saml20$keydescriptor] ON [saml20$keydescriptor_roledescriptor] ([saml20$roledescriptorid] ASC,[saml20$keydescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('bb72804b-d22b-4889-bb40-caf2c17ed210', 'SAML20.KeyDescriptor_RoleDescriptor', 'saml20$keydescriptor_roledescriptor', '86288f14-2268-41a8-8cad-a48c1ec43836', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$keydescriptorid', 'saml20$roledescriptorid', 'idx_saml20$keydescriptor_roledescriptor_saml20$roledescriptor_saml20$keydescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$keydescriptor_roledescriptor_saml20$keydescriptorid', 'bb72804b-d22b-4889-bb40-caf2c17ed210', 'f4e9139a-dd6d-3876-b926-c5058c8cfcc4');
GO
CREATE TABLE [saml20$keydescriptor_keyinfo] (
	[saml20$keydescriptorid] bigint NOT NULL,
	[saml20$keyinfoid] bigint NOT NULL,
	PRIMARY KEY([saml20$keydescriptorid],[saml20$keyinfoid]),
	CONSTRAINT [uniq_saml20$keydescriptor_keyinfo_saml20$keydescriptorid] UNIQUE ([saml20$keydescriptorid]));
GO
CREATE INDEX [idx_saml20$keydescriptor_keyinfo_saml20$keyinfo_saml20$keydescriptor] ON [saml20$keydescriptor_keyinfo] ([saml20$keyinfoid] ASC,[saml20$keydescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('e17ef2ca-d41e-454b-a953-8007192d0f88', 'SAML20.KeyDescriptor_KeyInfo', 'saml20$keydescriptor_keyinfo', '86288f14-2268-41a8-8cad-a48c1ec43836', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', 'saml20$keydescriptorid', 'saml20$keyinfoid', 'idx_saml20$keydescriptor_keyinfo_saml20$keyinfo_saml20$keydescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$keydescriptor_keyinfo_saml20$keydescriptorid', 'e17ef2ca-d41e-454b-a953-8007192d0f88', 'c3abbf21-f3e1-35bd-a04d-18f6b50f525a');
GO
CREATE TABLE [mxmodelreflection$values] (
	[mxmodelreflection$mxobjectenumid] bigint NOT NULL,
	[mxmodelreflection$mxobjectenumvalueid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectenumid],[mxmodelreflection$mxobjectenumvalueid]));
GO
CREATE INDEX [idx_mxmodelreflection$values_mxmodelreflection$mxobjectenumvalue_mxmodelreflection$mxobjectenum] ON [mxmodelreflection$values] ([mxmodelreflection$mxobjectenumvalueid] ASC,[mxmodelreflection$mxobjectenumid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('51e21528-a6f6-48f2-8723-ee958073cf38', 'MxModelReflection.Values', 'mxmodelreflection$values', 'c7be83f4-3dd2-4168-a27b-702c1e0511b3', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'mxmodelreflection$mxobjectenumid', 'mxmodelreflection$mxobjectenumvalueid', 'idx_mxmodelreflection$values_mxmodelreflection$mxobjectenumvalue_mxmodelreflection$mxobjectenum', 2);
GO
CREATE TABLE [saml20$keyinfo_x509certificate] (
	[saml20$keyinfoid] bigint NOT NULL,
	[saml20$x509certificateid] bigint NOT NULL,
	PRIMARY KEY([saml20$keyinfoid],[saml20$x509certificateid]));
GO
CREATE INDEX [idx_saml20$keyinfo_x509certificate_saml20$x509certificate_saml20$keyinfo] ON [saml20$keyinfo_x509certificate] ([saml20$x509certificateid] ASC,[saml20$keyinfoid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('314cbf78-6572-48c1-afd6-71830bd72177', 'SAML20.KeyInfo_X509Certificate', 'saml20$keyinfo_x509certificate', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', '2c7ce803-c9d2-4d2e-8f14-10d251a8f8bc', 'saml20$keyinfoid', 'saml20$x509certificateid', 'idx_saml20$keyinfo_x509certificate_saml20$x509certificate_saml20$keyinfo', 2);
GO
CREATE TABLE [saml20$keyinfo_keyinfo] (
	[saml20$keyinfoid1] bigint NOT NULL,
	[saml20$keyinfoid2] bigint NOT NULL,
	PRIMARY KEY([saml20$keyinfoid1],[saml20$keyinfoid2]),
	CONSTRAINT [uniq_saml20$keyinfo_keyinfo_saml20$keyinfoid1] UNIQUE ([saml20$keyinfoid1]));
GO
CREATE INDEX [idx_saml20$keyinfo_keyinfo_saml20$keyinfo_saml20$keyinfo] ON [saml20$keyinfo_keyinfo] ([saml20$keyinfoid2] ASC,[saml20$keyinfoid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('fed24753-086b-4857-aa65-070f37737516', 'SAML20.KeyInfo_KeyInfo', 'saml20$keyinfo_keyinfo', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', 'saml20$keyinfoid1', 'saml20$keyinfoid2', 'idx_saml20$keyinfo_keyinfo_saml20$keyinfo_saml20$keyinfo', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$keyinfo_keyinfo_saml20$keyinfoid1', 'fed24753-086b-4857-aa65-070f37737516', 'c40d8ca2-f87e-31f4-9634-00b1a4454ab2');
GO
CREATE TABLE [saml20$keyinfo_entitydescriptor] (
	[saml20$keyinfoid] bigint NOT NULL,
	[saml20$entitydescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$keyinfoid],[saml20$entitydescriptorid]),
	CONSTRAINT [uniq_saml20$keyinfo_entitydescriptor_saml20$keyinfoid] UNIQUE ([saml20$keyinfoid]));
GO
CREATE INDEX [idx_saml20$keyinfo_entitydescriptor_saml20$entitydescriptor_saml20$keyinfo] ON [saml20$keyinfo_entitydescriptor] ([saml20$entitydescriptorid] ASC,[saml20$keyinfoid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('371fbacf-5ecd-4f50-84e1-7f88e1489314', 'SAML20.KeyInfo_EntityDescriptor', 'saml20$keyinfo_entitydescriptor', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', '50527e51-152a-40ca-98fa-75a0b24193bb', 'saml20$keyinfoid', 'saml20$entitydescriptorid', 'idx_saml20$keyinfo_entitydescriptor_saml20$entitydescriptor_saml20$keyinfo', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$keyinfo_entitydescriptor_saml20$keyinfoid', '371fbacf-5ecd-4f50-84e1-7f88e1489314', '8e320b8f-80f4-309c-bf14-e0f7c41ef16e');
GO
CREATE TABLE [mxmodelreflection$token_mxobjecttype_start] (
	[mxmodelreflection$tokenid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$tokenid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$token_mxobjecttype_start_mxmodelreflection$tokenid] UNIQUE ([mxmodelreflection$tokenid]));
GO
CREATE INDEX [idx_mxmodelreflection$token_mxobjecttype_start_mxmodelreflection$mxobjecttype_mxmodelreflection$token] ON [mxmodelreflection$token_mxobjecttype_start] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$tokenid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('02cc1df6-dcdf-4626-bd39-5ad0bd838808', 'MxModelReflection.Token_MxObjectType_Start', 'mxmodelreflection$token_mxobjecttype_start', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$tokenid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$token_mxobjecttype_start_mxmodelreflection$mxobjecttype_mxmodelreflection$token', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$token_mxobjecttype_start_mxmodelreflection$tokenid', '02cc1df6-dcdf-4626-bd39-5ad0bd838808', '951aeadf-2258-36ce-82ae-d77a99e8f90b');
GO
CREATE TABLE [mxmodelreflection$token_mxobjectreference] (
	[mxmodelreflection$tokenid] bigint NOT NULL,
	[mxmodelreflection$mxobjectreferenceid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$tokenid],[mxmodelreflection$mxobjectreferenceid]),
	CONSTRAINT [uniq_mxmodelreflection$token_mxobjectreference_mxmodelreflection$tokenid] UNIQUE ([mxmodelreflection$tokenid]));
GO
CREATE INDEX [idx_mxmodelreflection$token_mxobjectreference_mxmodelreflection$mxobjectreference_mxmodelreflection$token] ON [mxmodelreflection$token_mxobjectreference] ([mxmodelreflection$mxobjectreferenceid] ASC,[mxmodelreflection$tokenid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('cce184f4-66c0-4f65-9720-365fa4d9b2cb', 'MxModelReflection.Token_MxObjectReference', 'mxmodelreflection$token_mxobjectreference', '1532681f-31fb-402f-991d-87e23ef1bcf4', '057a1e77-1c44-46fa-9eef-6809c800641f', 'mxmodelreflection$tokenid', 'mxmodelreflection$mxobjectreferenceid', 'idx_mxmodelreflection$token_mxobjectreference_mxmodelreflection$mxobjectreference_mxmodelreflection$token', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$token_mxobjectreference_mxmodelreflection$tokenid', 'cce184f4-66c0-4f65-9720-365fa4d9b2cb', 'aabe41b1-785e-3db4-a378-644cccc7a127');
GO
CREATE TABLE [mxmodelreflection$token_mxobjecttype_referenced] (
	[mxmodelreflection$tokenid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$tokenid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$token_mxobjecttype_referenced_mxmodelreflection$tokenid] UNIQUE ([mxmodelreflection$tokenid]));
GO
CREATE INDEX [idx_mxmodelreflection$token_mxobjecttype_referenced_mxmodelreflection$mxobjecttype_mxmodelreflection$token] ON [mxmodelreflection$token_mxobjecttype_referenced] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$tokenid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('e536a541-fdfe-433b-955a-24792442d98a', 'MxModelReflection.Token_MxObjectType_Referenced', 'mxmodelreflection$token_mxobjecttype_referenced', '1532681f-31fb-402f-991d-87e23ef1bcf4', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$tokenid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$token_mxobjecttype_referenced_mxmodelreflection$mxobjecttype_mxmodelreflection$token', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$token_mxobjecttype_referenced_mxmodelreflection$tokenid', 'e536a541-fdfe-433b-955a-24792442d98a', 'd62f1240-96f3-3078-932e-e6d696071185');
GO
CREATE TABLE [mxmodelreflection$token_mxobjectmember] (
	[mxmodelreflection$tokenid] bigint NOT NULL,
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$tokenid],[mxmodelreflection$mxobjectmemberid]),
	CONSTRAINT [uniq_mxmodelreflection$token_mxobjectmember_mxmodelreflection$tokenid] UNIQUE ([mxmodelreflection$tokenid]));
GO
CREATE INDEX [idx_mxmodelreflection$token_mxobjectmember_mxmodelreflection$mxobjectmember_mxmodelreflection$token] ON [mxmodelreflection$token_mxobjectmember] ([mxmodelreflection$mxobjectmemberid] ASC,[mxmodelreflection$tokenid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('f527482b-65c3-4636-8a72-e8e83209ed0a', 'MxModelReflection.Token_MxObjectMember', 'mxmodelreflection$token_mxobjectmember', '1532681f-31fb-402f-991d-87e23ef1bcf4', '398a1f70-2b2c-408e-8778-f4a923fda765', 'mxmodelreflection$tokenid', 'mxmodelreflection$mxobjectmemberid', 'idx_mxmodelreflection$token_mxobjectmember_mxmodelreflection$mxobjectmember_mxmodelreflection$token', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$token_mxobjectmember_mxmodelreflection$tokenid', 'f527482b-65c3-4636-8a72-e8e83209ed0a', '4456d093-80d4-31ae-bf2b-e387e0509db9');
GO
CREATE TABLE [saml20$ssoconfiguration_keystore] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$keystoreid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$keystoreid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_keystore_saml20$keystoreid] UNIQUE ([saml20$keystoreid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_keystore_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_keystore_saml20$keystore_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_keystore] ([saml20$keystoreid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('f442f977-1d5e-4ca8-8ac8-36c1fd968a62', 'SAML20.SSOConfiguration_KeyStore', 'saml20$ssoconfiguration_keystore', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '712b9e2b-202a-496a-896b-61440043e0b8', 'saml20$ssoconfigurationid', 'saml20$keystoreid', 'idx_saml20$ssoconfiguration_keystore_saml20$keystore_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_keystore_saml20$keystoreid', 'f442f977-1d5e-4ca8-8ac8-36c1fd968a62', '9271ebdc-1690-3eae-b6a4-1d004e254aa9');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_keystore_saml20$ssoconfigurationid', 'f442f977-1d5e-4ca8-8ac8-36c1fd968a62', '0493d10f-724a-3b8b-a037-5639a4a6dfe5');
GO
CREATE TABLE [saml20$ssoconfiguration_attribute] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$attributeid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$attributeid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_attribute_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_attribute_saml20$attribute_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_attribute] ([saml20$attributeid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('459192c8-79b3-48ff-bfb5-b03a3a8eb0d5', 'SAML20.SSOConfiguration_Attribute', 'saml20$ssoconfiguration_attribute', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'saml20$ssoconfigurationid', 'saml20$attributeid', 'idx_saml20$ssoconfiguration_attribute_saml20$attribute_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_attribute_saml20$ssoconfigurationid', '459192c8-79b3-48ff-bfb5-b03a3a8eb0d5', '0a7f8174-448b-355c-b182-75f388d1aa19');
GO
CREATE TABLE [saml20$ssoconfiguration_customaftersigninmicroflow] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$microflowsid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_customaftersigninmicroflow_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_customaftersigninmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_customaftersigninmicroflow] ([mxmodelreflection$microflowsid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('0c9fd6df-5350-4448-80e6-82a42a8ec7e1', 'SAML20.SSOConfiguration_CustomAfterSigninMicroflow', 'saml20$ssoconfiguration_customaftersigninmicroflow', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '17e6df01-1431-4547-b824-d471e84f719c', 'saml20$ssoconfigurationid', 'mxmodelreflection$microflowsid', 'idx_saml20$ssoconfiguration_customaftersigninmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_customaftersigninmicroflow_saml20$ssoconfigurationid', '0c9fd6df-5350-4448-80e6-82a42a8ec7e1', '70a2d472-bb11-353d-b7ba-9b842408a42c');
GO
CREATE TABLE [saml20$ssoconfiguration_defaultuserroletoassign] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[system$userroleid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[system$userroleid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_defaultuserroletoassign_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_defaultuserroletoassign_system$userrole_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_defaultuserroletoassign] ([system$userroleid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('12c28c1c-cf96-4398-beb6-619eabe198b4', 'SAML20.SSOConfiguration_DefaultUserRoleToAssign', 'saml20$ssoconfiguration_defaultuserroletoassign', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'saml20$ssoconfigurationid', 'system$userroleid', 'idx_saml20$ssoconfiguration_defaultuserroletoassign_system$userrole_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_defaultuserroletoassign_saml20$ssoconfigurationid', '12c28c1c-cf96-4398-beb6-619eabe198b4', 'efac73f4-d9db-3d5b-b646-fac18556e5db');
GO
CREATE TABLE [saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$microflowsid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow] ([mxmodelreflection$microflowsid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('62800896-b4b3-49f8-b48c-943eaa5e4027', 'SAML20.SSOConfiguration_CustomPrepareInSessionAuthenticationMicroflow', 'saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '17e6df01-1431-4547-b824-d471e84f719c', 'saml20$ssoconfigurationid', 'mxmodelreflection$microflowsid', 'idx_saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_customprepareinsessionauthenticationmicroflow_saml20$ssoconfigurationid', '62800896-b4b3-49f8-b48c-943eaa5e4027', '1b69c565-78d1-356e-bbfe-aded44bfcc57');
GO
CREATE TABLE [saml20$ssoconfiguration_customuserprovisioningmicroflow] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$microflowsid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_customuserprovisioningmicroflow_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_customuserprovisioningmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_customuserprovisioningmicroflow] ([mxmodelreflection$microflowsid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('eb7764a5-cee4-44c2-a376-f1825020ca90', 'SAML20.SSOConfiguration_CustomUserProvisioningMicroflow', 'saml20$ssoconfiguration_customuserprovisioningmicroflow', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '17e6df01-1431-4547-b824-d471e84f719c', 'saml20$ssoconfigurationid', 'mxmodelreflection$microflowsid', 'idx_saml20$ssoconfiguration_customuserprovisioningmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_customuserprovisioningmicroflow_saml20$ssoconfigurationid', 'eb7764a5-cee4-44c2-a376-f1825020ca90', '85611d11-2c8b-3fd3-bee6-68ca29ef9f43');
GO
CREATE TABLE [saml20$ssoconfiguration_samlauthncontext] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$samlauthncontextid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$samlauthncontextid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_samlauthncontext_saml20$samlauthncontext_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_samlauthncontext] ([saml20$samlauthncontextid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d5878af1-a9ae-4cec-a95d-cb67b9007de9', 'SAML20.SSOConfiguration_SAMLAuthnContext', 'saml20$ssoconfiguration_samlauthncontext', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'saml20$ssoconfigurationid', 'saml20$samlauthncontextid', 'idx_saml20$ssoconfiguration_samlauthncontext_saml20$samlauthncontext_saml20$ssoconfiguration', 2);
GO
CREATE TABLE [saml20$ssoconfiguration_idpmetadata] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$idpmetadataid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$idpmetadataid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_idpmetadata_saml20$idpmetadataid] UNIQUE ([saml20$idpmetadataid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_idpmetadata_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_idpmetadata_saml20$idpmetadata_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_idpmetadata] ([saml20$idpmetadataid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('b3a11942-d94b-4def-8b65-bcc6b7e19014', 'SAML20.SSOConfiguration_IdPMetadata', 'saml20$ssoconfiguration_idpmetadata', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '2e765af5-79d0-4b5c-8d04-5a7e8691b4a7', 'saml20$ssoconfigurationid', 'saml20$idpmetadataid', 'idx_saml20$ssoconfiguration_idpmetadata_saml20$idpmetadata_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_idpmetadata_saml20$idpmetadataid', 'b3a11942-d94b-4def-8b65-bcc6b7e19014', '4f66a17c-4217-3c4f-8d14-330c08a27829');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_idpmetadata_saml20$ssoconfigurationid', 'b3a11942-d94b-4def-8b65-bcc6b7e19014', '8e1e68b6-8db7-3e07-9c91-45afb185a243');
GO
CREATE TABLE [saml20$ssoconfiguration_mxobjectmember] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$mxobjectmemberid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_mxobjectmember_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_mxobjectmember_mxmodelreflection$mxobjectmember_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_mxobjectmember] ([mxmodelreflection$mxobjectmemberid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2d4ab2cb-51a7-4d2b-aace-7a5a364aa555', 'SAML20.SSOConfiguration_MxObjectMember', 'saml20$ssoconfiguration_mxobjectmember', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '398a1f70-2b2c-408e-8778-f4a923fda765', 'saml20$ssoconfigurationid', 'mxmodelreflection$mxobjectmemberid', 'idx_saml20$ssoconfiguration_mxobjectmember_mxmodelreflection$mxobjectmember_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_mxobjectmember_saml20$ssoconfigurationid', '2d4ab2cb-51a7-4d2b-aace-7a5a364aa555', '27ff54f0-c381-35ea-8b11-ea8bb210cb5e');
GO
CREATE TABLE [saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$microflowsid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow] ([mxmodelreflection$microflowsid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2941ae06-5805-49e0-acca-34b651b22b46', 'SAML20.SSOConfiguration_CustomEvaluateInSessionAuthenticationMicroflow', 'saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '17e6df01-1431-4547-b824-d471e84f719c', 'saml20$ssoconfigurationid', 'mxmodelreflection$microflowsid', 'idx_saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow_mxmodelreflection$microflows_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_customevaluateinsessionauthenticationmicroflow_saml20$ssoconfigurationid', '2941ae06-5805-49e0-acca-34b651b22b46', '65c2a93b-caad-3f93-abc9-b45e1967fb14');
GO
CREATE TABLE [saml20$ssoconfiguration_userprovisioning] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[usercommons$userprovisioningid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[usercommons$userprovisioningid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_userprovisioning_usercommons$userprovisioningid] UNIQUE ([usercommons$userprovisioningid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_userprovisioning_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_userprovisioning_usercommons$userprovisioning_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_userprovisioning] ([usercommons$userprovisioningid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('052913f3-81a7-40a8-a875-9a2aa2d1c906', 'SAML20.SSOConfiguration_UserProvisioning', 'saml20$ssoconfiguration_userprovisioning', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'saml20$ssoconfigurationid', 'usercommons$userprovisioningid', 'idx_saml20$ssoconfiguration_userprovisioning_usercommons$userprovisioning_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_userprovisioning_usercommons$userprovisioningid', '052913f3-81a7-40a8-a875-9a2aa2d1c906', 'da000a33-f8e7-3087-93de-5dce382763f8');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_userprovisioning_saml20$ssoconfigurationid', '052913f3-81a7-40a8-a875-9a2aa2d1c906', 'd084cf29-8c15-391b-b9da-c4d104d127c1');
GO
CREATE TABLE [saml20$ssoconfiguration_mxobjecttype] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_mxobjecttype_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_mxobjecttype_mxmodelreflection$mxobjecttype_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('e0bd0f95-e8e2-4e6c-b5a3-33aaa5ca0c8e', 'SAML20.SSOConfiguration_MxObjectType', 'saml20$ssoconfiguration_mxobjecttype', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'saml20$ssoconfigurationid', 'mxmodelreflection$mxobjecttypeid', 'idx_saml20$ssoconfiguration_mxobjecttype_mxmodelreflection$mxobjecttype_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_mxobjecttype_saml20$ssoconfigurationid', 'e0bd0f95-e8e2-4e6c-b5a3-33aaa5ca0c8e', '33fd3448-9252-39d1-bc0f-f486bd74cd9d');
GO
CREATE TABLE [saml20$ssoconfiguration_nameidformat] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$nameidformatid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$nameidformatid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_nameidformat_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_nameidformat_saml20$nameidformat_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_nameidformat] ([saml20$nameidformatid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('b8dae704-1004-463b-9d3d-4cf77d7b9706', 'SAML20.SSOConfiguration_NameIDFormat', 'saml20$ssoconfiguration_nameidformat', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'ca71a18f-d692-43d9-80cb-ee92e8fa4c5e', 'saml20$ssoconfigurationid', 'saml20$nameidformatid', 'idx_saml20$ssoconfiguration_nameidformat_saml20$nameidformat_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_nameidformat_saml20$ssoconfigurationid', 'b8dae704-1004-463b-9d3d-4cf77d7b9706', '77c22d9b-1aa2-3e85-ba92-b3dc5a101571');
GO
CREATE TABLE [saml20$ssoconfiguration_preferedentitydescriptor] (
	[saml20$ssoconfigurationid] bigint NOT NULL,
	[saml20$entitydescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$ssoconfigurationid],[saml20$entitydescriptorid]),
	CONSTRAINT [uniq_saml20$ssoconfiguration_preferedentitydescriptor_saml20$ssoconfigurationid] UNIQUE ([saml20$ssoconfigurationid]));
GO
CREATE INDEX [idx_saml20$ssoconfiguration_preferedentitydescriptor_saml20$entitydescriptor_saml20$ssoconfiguration] ON [saml20$ssoconfiguration_preferedentitydescriptor] ([saml20$entitydescriptorid] ASC,[saml20$ssoconfigurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('c4956206-873f-42fb-9983-8b62ac60ec1c', 'SAML20.SSOConfiguration_PreferedEntityDescriptor', 'saml20$ssoconfiguration_preferedentitydescriptor', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', '50527e51-152a-40ca-98fa-75a0b24193bb', 'saml20$ssoconfigurationid', 'saml20$entitydescriptorid', 'idx_saml20$ssoconfiguration_preferedentitydescriptor_saml20$entitydescriptor_saml20$ssoconfiguration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$ssoconfiguration_preferedentitydescriptor_saml20$ssoconfigurationid', 'c4956206-873f-42fb-9983-8b62ac60ec1c', 'd2952288-fc4c-35bd-b0ef-ef35889e9d74');
GO
CREATE TABLE [documentgeneration$documentrequest_documentuser] (
	[documentgeneration$documentrequestid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([documentgeneration$documentrequestid],[system$userid]),
	CONSTRAINT [uniq_documentgeneration$documentrequest_documentuser_documentgeneration$documentrequestid] UNIQUE ([documentgeneration$documentrequestid]));
GO
CREATE INDEX [idx_documentgeneration$documentrequest_documentuser_system$user_documentgeneration$documentrequest] ON [documentgeneration$documentrequest_documentuser] ([system$userid] ASC,[documentgeneration$documentrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d6eb6cfe-9fdb-420e-b251-16e3975129da', 'DocumentGeneration.DocumentRequest_DocumentUser', 'documentgeneration$documentrequest_documentuser', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'documentgeneration$documentrequestid', 'system$userid', 'idx_documentgeneration$documentrequest_documentuser_system$user_documentgeneration$documentrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_documentgeneration$documentrequest_documentuser_documentgeneration$documentrequestid', 'd6eb6cfe-9fdb-420e-b251-16e3975129da', '176ba3a4-76ef-3f4f-bec0-7f1b4ba9baaf');
GO
CREATE TABLE [documentgeneration$documentrequest_filedocument] (
	[documentgeneration$documentrequestid] bigint NOT NULL,
	[system$filedocumentid] bigint NOT NULL,
	PRIMARY KEY([documentgeneration$documentrequestid],[system$filedocumentid]),
	CONSTRAINT [uniq_documentgeneration$documentrequest_filedocument_documentgeneration$documentrequestid] UNIQUE ([documentgeneration$documentrequestid]));
GO
CREATE INDEX [idx_documentgeneration$documentrequest_filedocument_system$filedocument_documentgeneration$documentrequest] ON [documentgeneration$documentrequest_filedocument] ([system$filedocumentid] ASC,[documentgeneration$documentrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('a05c3b5b-12dc-4f19-8250-9b79e704eecc', 'DocumentGeneration.DocumentRequest_FileDocument', 'documentgeneration$documentrequest_filedocument', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', '170ce49d-f29c-4fac-99a6-b55e8a3aeb39', 'documentgeneration$documentrequestid', 'system$filedocumentid', 'idx_documentgeneration$documentrequest_filedocument_system$filedocument_documentgeneration$documentrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_documentgeneration$documentrequest_filedocument_documentgeneration$documentrequestid', 'a05c3b5b-12dc-4f19-8250-9b79e704eecc', '687226fe-dcbb-383a-849d-1bccaa2f56c8');
GO
CREATE TABLE [documentgeneration$documentrequest_session] (
	[documentgeneration$documentrequestid] bigint NOT NULL,
	[system$sessionid] bigint NOT NULL,
	PRIMARY KEY([documentgeneration$documentrequestid],[system$sessionid]),
	CONSTRAINT [uniq_documentgeneration$documentrequest_session_documentgeneration$documentrequestid] UNIQUE ([documentgeneration$documentrequestid]));
GO
CREATE INDEX [idx_documentgeneration$documentrequest_session_system$session_documentgeneration$documentrequest] ON [documentgeneration$documentrequest_session] ([system$sessionid] ASC,[documentgeneration$documentrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('662f6725-1df0-4414-a026-008dfbae6543', 'DocumentGeneration.DocumentRequest_Session', 'documentgeneration$documentrequest_session', '2cbe0e0d-64fb-498a-851d-0e2c9bf86cbc', '37f9fd49-5318-4c63-9a51-f761779b202f', 'documentgeneration$documentrequestid', 'system$sessionid', 'idx_documentgeneration$documentrequest_session_system$session_documentgeneration$documentrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_documentgeneration$documentrequest_session_documentgeneration$documentrequestid', '662f6725-1df0-4414-a026-008dfbae6543', '19a38912-8f14-343a-aabd-63ab717075d0');
GO
CREATE TABLE [system$synchronizationerrorfile_synchronizationerror] (
	[system$synchronizationerrorfileid] bigint NOT NULL,
	[system$synchronizationerrorid] bigint NOT NULL,
	PRIMARY KEY([system$synchronizationerrorfileid],[system$synchronizationerrorid]),
	CONSTRAINT [uniq_system$synchronizationerrorfile_synchronizationerror_system$synchronizationerrorfileid] UNIQUE ([system$synchronizationerrorfileid]));
GO
CREATE INDEX [idx_system$synchronizationerrorfile_synchronizationerror_system$synchronizationerror_system$synchronizationerrorfile] ON [system$synchronizationerrorfile_synchronizationerror] ([system$synchronizationerrorid] ASC,[system$synchronizationerrorfileid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('6440aa38-83ff-4ba6-8c85-5ff785956b09', 'System.SynchronizationErrorFile_SynchronizationError', 'system$synchronizationerrorfile_synchronizationerror', '9b26443c-f4bb-4252-aa62-9eaffb71c4db', 'f9818ad8-3214-4b1d-b837-3181863f5ed5', 'system$synchronizationerrorfileid', 'system$synchronizationerrorid', 'idx_system$synchronizationerrorfile_synchronizationerror_system$synchronizationerror_system$synchronizationerrorfile', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$synchronizationerrorfile_synchronizationerror_system$synchronizationerrorfileid', '6440aa38-83ff-4ba6-8c85-5ff785956b09', '1e2455b8-3c89-380e-983f-14ead6e94b4c');
GO
CREATE TABLE [excr_resourcemanagement_api$resimage_session] (
	[excr_resourcemanagement_api$resimageid] bigint NOT NULL,
	[system$sessionid] bigint NOT NULL,
	PRIMARY KEY([excr_resourcemanagement_api$resimageid],[system$sessionid]),
	CONSTRAINT [uniq_excr_resourcemanagement_api$resimage_session_excr_resourcemanagement_api$resimageid] UNIQUE ([excr_resourcemanagement_api$resimageid]));
GO
CREATE INDEX [idx_excr_resourcemanagement_api$resimage_session_system$session_excr_resourcemanagement_api$resimage] ON [excr_resourcemanagement_api$resimage_session] ([system$sessionid] ASC,[excr_resourcemanagement_api$resimageid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('7b398475-9218-49c0-9591-19a45cbc9022', 'EXCR_ResourceManagement_API.ResImage_Session', 'excr_resourcemanagement_api$resimage_session', '062006cd-b52e-4e0c-b505-f1f275611e1e', '37f9fd49-5318-4c63-9a51-f761779b202f', 'excr_resourcemanagement_api$resimageid', 'system$sessionid', 'idx_excr_resourcemanagement_api$resimage_session_system$session_excr_resourcemanagement_api$resimage', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_resourcemanagement_api$resimage_session_excr_resourcemanagement_api$resimageid', '7b398475-9218-49c0-9591-19a45cbc9022', '7711672e-0d24-35e4-b848-ffc71d7efbb8');
GO
CREATE TABLE [system$scheduledeventinformation_xasinstance] (
	[system$scheduledeventinformationid] bigint NOT NULL,
	[system$xasinstanceid] bigint NOT NULL,
	PRIMARY KEY([system$scheduledeventinformationid],[system$xasinstanceid]),
	CONSTRAINT [uniq_system$scheduledeventinformation_xasinstance_system$scheduledeventinformationid] UNIQUE ([system$scheduledeventinformationid]));
GO
CREATE INDEX [idx_system$scheduledeventinformation_xasinstance_system$xasinstance_system$scheduledeventinformation] ON [system$scheduledeventinformation_xasinstance] ([system$xasinstanceid] ASC,[system$scheduledeventinformationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('7b8a18de-fe6d-4735-9841-0d9d4760697e', 'System.ScheduledEventInformation_XASInstance', 'system$scheduledeventinformation_xasinstance', '685df5a6-1e02-49bb-a0b5-5a55c5e8313d', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'system$scheduledeventinformationid', 'system$xasinstanceid', 'idx_system$scheduledeventinformation_xasinstance_system$xasinstance_system$scheduledeventinformation', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$scheduledeventinformation_xasinstance_system$scheduledeventinformationid', '7b8a18de-fe6d-4735-9841-0d9d4760697e', 'b1eb1dd4-9f4c-3dae-9cde-9a7df66af36c');
GO
CREATE TABLE [saml20$claimmap_attribute] (
	[saml20$claimmapid] bigint NOT NULL,
	[saml20$attributeid] bigint NOT NULL,
	PRIMARY KEY([saml20$claimmapid],[saml20$attributeid]),
	CONSTRAINT [uniq_saml20$claimmap_attribute_saml20$claimmapid] UNIQUE ([saml20$claimmapid]));
GO
CREATE INDEX [idx_saml20$claimmap_attribute_saml20$attribute_saml20$claimmap] ON [saml20$claimmap_attribute] ([saml20$attributeid] ASC,[saml20$claimmapid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('b8ea4538-0216-4e4c-b6e2-14eaaa43fad2', 'SAML20.ClaimMap_Attribute', 'saml20$claimmap_attribute', '114fb822-41ba-4482-90e3-2c7a8743bf7f', '1fe71b95-a95e-4ddd-a9f9-501182b13749', 'saml20$claimmapid', 'saml20$attributeid', 'idx_saml20$claimmap_attribute_saml20$attribute_saml20$claimmap', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$claimmap_attribute_saml20$claimmapid', 'b8ea4538-0216-4e4c-b6e2-14eaaa43fad2', '1f82c68e-1d50-365d-a068-161868a0acde');
GO
CREATE TABLE [saml20$claimmap_ssoconfiguration] (
	[saml20$claimmapid] bigint NOT NULL,
	[saml20$ssoconfigurationid] bigint NOT NULL,
	PRIMARY KEY([saml20$claimmapid],[saml20$ssoconfigurationid]),
	CONSTRAINT [uniq_saml20$claimmap_ssoconfiguration_saml20$claimmapid] UNIQUE ([saml20$claimmapid]));
GO
CREATE INDEX [idx_saml20$claimmap_ssoconfiguration_saml20$ssoconfiguration_saml20$claimmap] ON [saml20$claimmap_ssoconfiguration] ([saml20$ssoconfigurationid] ASC,[saml20$claimmapid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('c1118c86-af82-4b74-ad63-3206ef8cc2fe', 'SAML20.ClaimMap_SSOConfiguration', 'saml20$claimmap_ssoconfiguration', '114fb822-41ba-4482-90e3-2c7a8743bf7f', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'saml20$claimmapid', 'saml20$ssoconfigurationid', 'idx_saml20$claimmap_ssoconfiguration_saml20$ssoconfiguration_saml20$claimmap', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$claimmap_ssoconfiguration_saml20$claimmapid', 'c1118c86-af82-4b74-ad63-3206ef8cc2fe', 'afcd7dbd-2657-326c-be6f-b517debf06e8');
GO
CREATE TABLE [saml20$claimmap_mxobjectmember] (
	[saml20$claimmapid] bigint NOT NULL,
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	PRIMARY KEY([saml20$claimmapid],[mxmodelreflection$mxobjectmemberid]),
	CONSTRAINT [uniq_saml20$claimmap_mxobjectmember_saml20$claimmapid] UNIQUE ([saml20$claimmapid]));
GO
CREATE INDEX [idx_saml20$claimmap_mxobjectmember_mxmodelreflection$mxobjectmember_saml20$claimmap] ON [saml20$claimmap_mxobjectmember] ([mxmodelreflection$mxobjectmemberid] ASC,[saml20$claimmapid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('671364bc-5af5-48b6-a2a6-6a7fbf3ea785', 'SAML20.ClaimMap_MxObjectMember', 'saml20$claimmap_mxobjectmember', '114fb822-41ba-4482-90e3-2c7a8743bf7f', '398a1f70-2b2c-408e-8778-f4a923fda765', 'saml20$claimmapid', 'mxmodelreflection$mxobjectmemberid', 'idx_saml20$claimmap_mxobjectmember_mxmodelreflection$mxobjectmember_saml20$claimmap', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$claimmap_mxobjectmember_saml20$claimmapid', '671364bc-5af5-48b6-a2a6-6a7fbf3ea785', '4abe5469-dfee-3d79-9f3b-2c4189efb994');
GO
CREATE TABLE [system$workflowusertaskoutcome_user] (
	[system$workflowusertaskoutcomeid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskoutcomeid],[system$userid]),
	CONSTRAINT [uniq_system$workflowusertaskoutcome_user_system$workflowusertaskoutcomeid] UNIQUE ([system$workflowusertaskoutcomeid]));
GO
CREATE INDEX [idx_system$workflowusertaskoutcome_user_system$user_system$workflowusertaskoutcome] ON [system$workflowusertaskoutcome_user] ([system$userid] ASC,[system$workflowusertaskoutcomeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d49ae7eb-5886-4a82-8659-be0b01d13104', 'System.WorkflowUserTaskOutcome_User', 'system$workflowusertaskoutcome_user', 'd753ad05-63c3-4d18-9424-1dd97c7d1a05', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$workflowusertaskoutcomeid', 'system$userid', 'idx_system$workflowusertaskoutcome_user_system$user_system$workflowusertaskoutcome', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowusertaskoutcome_user_system$workflowusertaskoutcomeid', 'd49ae7eb-5886-4a82-8659-be0b01d13104', '1416fe96-0847-37c7-8af0-e159dca1b3c7');
GO
CREATE TABLE [system$workflowusertaskoutcome_workflowusertask] (
	[system$workflowusertaskoutcomeid] bigint NOT NULL,
	[system$workflowusertaskid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskoutcomeid],[system$workflowusertaskid]),
	CONSTRAINT [uniq_system$workflowusertaskoutcome_workflowusertask_system$workflowusertaskoutcomeid] UNIQUE ([system$workflowusertaskoutcomeid]));
GO
CREATE INDEX [idx_system$workflowusertaskoutcome_workflowusertask_system$workflowusertask_system$workflowusertaskoutcome] ON [system$workflowusertaskoutcome_workflowusertask] ([system$workflowusertaskid] ASC,[system$workflowusertaskoutcomeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('37818a79-121d-447e-aa9d-89e88b145180', 'System.WorkflowUserTaskOutcome_WorkflowUserTask', 'system$workflowusertaskoutcome_workflowusertask', 'd753ad05-63c3-4d18-9424-1dd97c7d1a05', '3729d27c-735b-457a-b210-9dffb125c3f3', 'system$workflowusertaskoutcomeid', 'system$workflowusertaskid', 'idx_system$workflowusertaskoutcome_workflowusertask_system$workflowusertask_system$workflowusertaskoutcome', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowusertaskoutcome_workflowusertask_system$workflowusertaskoutcomeid', '37818a79-121d-447e-aa9d-89e88b145180', '350d214b-6b9c-3c5d-a6d2-3483fecc59d8');
GO
CREATE TABLE [system$thumbnail_image] (
	[system$thumbnailid] bigint NOT NULL,
	[system$imageid] bigint NOT NULL,
	PRIMARY KEY([system$thumbnailid],[system$imageid]),
	CONSTRAINT [uniq_system$thumbnail_image_system$imageid] UNIQUE ([system$imageid]),
	CONSTRAINT [uniq_system$thumbnail_image_system$thumbnailid] UNIQUE ([system$thumbnailid]));
GO
CREATE INDEX [idx_system$thumbnail_image_system$image_system$thumbnail] ON [system$thumbnail_image] ([system$imageid] ASC,[system$thumbnailid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('3dbea779-c8af-467e-a957-140c313ac1b7', 'System.Thumbnail_Image', 'system$thumbnail_image', '4babd4c0-b903-4cb4-b1af-e59c4a5fcf3d', '37827192-315d-4ab6-85b8-f626f866ea76', 'system$thumbnailid', 'system$imageid', 'idx_system$thumbnail_image_system$image_system$thumbnail', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$thumbnail_image_system$imageid', '3dbea779-c8af-467e-a957-140c313ac1b7', '580b34f8-f2b7-3c00-a872-d0e0b53778ef');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$thumbnail_image_system$thumbnailid', '3dbea779-c8af-467e-a957-140c313ac1b7', '9c4f4f6d-6094-3a1b-a97c-09277561b351');
GO
CREATE TABLE [administration$account_configuration] (
	[administration$accountid] bigint NOT NULL,
	[excr_commons$configurationid] bigint NOT NULL,
	PRIMARY KEY([administration$accountid],[excr_commons$configurationid]),
	CONSTRAINT [uniq_administration$account_configuration_excr_commons$configurationid] UNIQUE ([excr_commons$configurationid]),
	CONSTRAINT [uniq_administration$account_configuration_administration$accountid] UNIQUE ([administration$accountid]));
GO
CREATE INDEX [idx_administration$account_configuration_excr_commons$configuration_administration$account] ON [administration$account_configuration] ([excr_commons$configurationid] ASC,[administration$accountid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('47b286db-bb57-499e-82fe-5f4acb93a119', 'Administration.Account_Configuration', 'administration$account_configuration', 'c921ccbb-a670-48d9-833d-6a76c1406917', '0593fd24-73a3-48c4-b29e-c9b1009278bd', 'administration$accountid', 'excr_commons$configurationid', 'idx_administration$account_configuration_excr_commons$configuration_administration$account', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$account_configuration_excr_commons$configurationid', '47b286db-bb57-499e-82fe-5f4acb93a119', '707f81a6-1e29-3e14-a6e4-e26ce3fc3cf9');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$account_configuration_administration$accountid', '47b286db-bb57-499e-82fe-5f4acb93a119', '4ba04b41-f069-393c-a393-43c757d21d1b');
GO
CREATE TABLE [saml20$roledescriptortype_contactperson] (
	[saml20$contactid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$contactid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$roledescriptortype_contactperson_saml20$contactid] UNIQUE ([saml20$contactid]));
GO
CREATE INDEX [idx_saml20$roledescriptortype_contactperson_saml20$roledescriptor_saml20$contact] ON [saml20$roledescriptortype_contactperson] ([saml20$roledescriptorid] ASC,[saml20$contactid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('9896d8f6-4a5c-4358-a70d-035071045791', 'SAML20.RoleDescriptorType_ContactPerson', 'saml20$roledescriptortype_contactperson', '3cfaa082-c97b-4635-a503-8806c914525f', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$contactid', 'saml20$roledescriptorid', 'idx_saml20$roledescriptortype_contactperson_saml20$roledescriptor_saml20$contact', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$roledescriptortype_contactperson_saml20$contactid', '9896d8f6-4a5c-4358-a70d-035071045791', '761f89bd-85c1-3e04-9567-d984f5112ac3');
GO
CREATE TABLE [saml20$contacttype_entitydescriptor] (
	[saml20$contactid] bigint NOT NULL,
	[saml20$entitydescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$contactid],[saml20$entitydescriptorid]),
	CONSTRAINT [uniq_saml20$contacttype_entitydescriptor_saml20$contactid] UNIQUE ([saml20$contactid]));
GO
CREATE INDEX [idx_saml20$contacttype_entitydescriptor_saml20$entitydescriptor_saml20$contact] ON [saml20$contacttype_entitydescriptor] ([saml20$entitydescriptorid] ASC,[saml20$contactid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('89716a51-60f8-4126-951d-631b702aa7de', 'SAML20.ContactType_EntityDescriptor', 'saml20$contacttype_entitydescriptor', '3cfaa082-c97b-4635-a503-8806c914525f', '50527e51-152a-40ca-98fa-75a0b24193bb', 'saml20$contactid', 'saml20$entitydescriptorid', 'idx_saml20$contacttype_entitydescriptor_saml20$entitydescriptor_saml20$contact', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$contacttype_entitydescriptor_saml20$contactid', '89716a51-60f8-4126-951d-631b702aa7de', 'cf439eb4-5b2e-32a8-8b45-93f87e645561');
GO
CREATE TABLE [saml20$spattributeconsumingservice_ssoconfiguration] (
	[saml20$spattributeconsumingserviceid] bigint NOT NULL,
	[saml20$ssoconfigurationid] bigint NOT NULL,
	PRIMARY KEY([saml20$spattributeconsumingserviceid],[saml20$ssoconfigurationid]),
	CONSTRAINT [uniq_saml20$spattributeconsumingservice_ssoconfiguration_saml20$spattributeconsumingserviceid] UNIQUE ([saml20$spattributeconsumingserviceid]));
GO
CREATE INDEX [idx_saml20$spattributeconsumingservice_ssoconfiguration_saml20$ssoconfiguration_saml20$spattributeconsumingservice] ON [saml20$spattributeconsumingservice_ssoconfiguration] ([saml20$ssoconfigurationid] ASC,[saml20$spattributeconsumingserviceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('683da4a6-eb14-4f62-a124-945288886c0b', 'SAML20.SPAttributeConsumingService_SSOConfiguration', 'saml20$spattributeconsumingservice_ssoconfiguration', '0f381b16-223a-4086-b6a0-18f5a76553af', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'saml20$spattributeconsumingserviceid', 'saml20$ssoconfigurationid', 'idx_saml20$spattributeconsumingservice_ssoconfiguration_saml20$ssoconfiguration_saml20$spattributeconsumingservice', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$spattributeconsumingservice_ssoconfiguration_saml20$spattributeconsumingserviceid', '683da4a6-eb14-4f62-a124-945288886c0b', '5078262b-00bf-345f-9b73-f356be6c8896');
GO
CREATE TABLE [saml20$configuredsamlauthncontext_samlauthncontext] (
	[saml20$configuredsamlauthncontextid] bigint NOT NULL,
	[saml20$samlauthncontextid] bigint NOT NULL,
	PRIMARY KEY([saml20$configuredsamlauthncontextid],[saml20$samlauthncontextid]),
	CONSTRAINT [uniq_saml20$configuredsamlauthncontext_samlauthncontext_saml20$configuredsamlauthncontextid] UNIQUE ([saml20$configuredsamlauthncontextid]));
GO
CREATE INDEX [idx_saml20$configuredsamlauthncontext_samlauthncontext_saml20$samlauthncontext_saml20$configuredsamlauthncontext] ON [saml20$configuredsamlauthncontext_samlauthncontext] ([saml20$samlauthncontextid] ASC,[saml20$configuredsamlauthncontextid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('c566e9be-4d14-42b8-85a2-69b82f17d40d', 'SAML20.ConfiguredSAMLAuthnContext_SAMLAuthnContext', 'saml20$configuredsamlauthncontext_samlauthncontext', '52e902d2-ef0c-4c71-9629-9131bd78c2e8', 'a5edf8c6-c78f-4c02-ae75-0e8a03eabf4c', 'saml20$configuredsamlauthncontextid', 'saml20$samlauthncontextid', 'idx_saml20$configuredsamlauthncontext_samlauthncontext_saml20$samlauthncontext_saml20$configuredsamlauthncontext', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$configuredsamlauthncontext_samlauthncontext_saml20$configuredsamlauthncontextid', 'c566e9be-4d14-42b8-85a2-69b82f17d40d', 'd73ef66f-b414-3a2a-b768-e124a4fd5557');
GO
CREATE TABLE [saml20$configuredsamlauthncontext_ssoconfiguration] (
	[saml20$configuredsamlauthncontextid] bigint NOT NULL,
	[saml20$ssoconfigurationid] bigint NOT NULL,
	PRIMARY KEY([saml20$configuredsamlauthncontextid],[saml20$ssoconfigurationid]),
	CONSTRAINT [uniq_saml20$configuredsamlauthncontext_ssoconfiguration_saml20$configuredsamlauthncontextid] UNIQUE ([saml20$configuredsamlauthncontextid]));
GO
CREATE INDEX [idx_saml20$configuredsamlauthncontext_ssoconfiguration_saml20$ssoconfiguration_saml20$configuredsamlauthncontext] ON [saml20$configuredsamlauthncontext_ssoconfiguration] ([saml20$ssoconfigurationid] ASC,[saml20$configuredsamlauthncontextid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('01aa5641-4653-4566-a67b-e0563fe4b667', 'SAML20.ConfiguredSAMLAuthnContext_SSOConfiguration', 'saml20$configuredsamlauthncontext_ssoconfiguration', '52e902d2-ef0c-4c71-9629-9131bd78c2e8', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'saml20$configuredsamlauthncontextid', 'saml20$ssoconfigurationid', 'idx_saml20$configuredsamlauthncontext_ssoconfiguration_saml20$ssoconfiguration_saml20$configuredsamlauthncontext', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$configuredsamlauthncontext_ssoconfiguration_saml20$configuredsamlauthncontextid', '01aa5641-4653-4566-a67b-e0563fe4b667', 'a33a3633-ae67-3cbc-bcca-c4ba30937af4');
GO
CREATE TABLE [saml20$samlrequest_endpoint] (
	[saml20$samlrequestid] bigint NOT NULL,
	[saml20$endpointid] bigint NOT NULL,
	PRIMARY KEY([saml20$samlrequestid],[saml20$endpointid]),
	CONSTRAINT [uniq_saml20$samlrequest_endpoint_saml20$samlrequestid] UNIQUE ([saml20$samlrequestid]));
GO
CREATE INDEX [idx_saml20$samlrequest_endpoint_saml20$endpoint_saml20$samlrequest] ON [saml20$samlrequest_endpoint] ([saml20$endpointid] ASC,[saml20$samlrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('f2a4ffd9-0a42-489a-88b7-9de3b62b285d', 'SAML20.SAMLRequest_Endpoint', 'saml20$samlrequest_endpoint', '0550ede5-68a7-4dcf-ad37-389501ca7d90', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', 'saml20$samlrequestid', 'saml20$endpointid', 'idx_saml20$samlrequest_endpoint_saml20$endpoint_saml20$samlrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$samlrequest_endpoint_saml20$samlrequestid', 'f2a4ffd9-0a42-489a-88b7-9de3b62b285d', '16586831-7ab1-3148-9f2b-0f7ef177912e');
GO
CREATE TABLE [saml20$samlrequest_ssoconfiguration] (
	[saml20$samlrequestid] bigint NOT NULL,
	[saml20$ssoconfigurationid] bigint NOT NULL,
	PRIMARY KEY([saml20$samlrequestid],[saml20$ssoconfigurationid]),
	CONSTRAINT [uniq_saml20$samlrequest_ssoconfiguration_saml20$samlrequestid] UNIQUE ([saml20$samlrequestid]));
GO
CREATE INDEX [idx_saml20$samlrequest_ssoconfiguration_saml20$ssoconfiguration_saml20$samlrequest] ON [saml20$samlrequest_ssoconfiguration] ([saml20$ssoconfigurationid] ASC,[saml20$samlrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('4304d4fb-9f1c-4aae-b733-7d4b3eeca8ec', 'SAML20.SAMLRequest_SSOConfiguration', 'saml20$samlrequest_ssoconfiguration', '0550ede5-68a7-4dcf-ad37-389501ca7d90', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'saml20$samlrequestid', 'saml20$ssoconfigurationid', 'idx_saml20$samlrequest_ssoconfiguration_saml20$ssoconfiguration_saml20$samlrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$samlrequest_ssoconfiguration_saml20$samlrequestid', '4304d4fb-9f1c-4aae-b733-7d4b3eeca8ec', 'b27d8ce0-b523-3feb-96d5-939643c0d695');
GO
CREATE TABLE [saml20$samlrequest_samlresponse] (
	[saml20$samlrequestid] bigint NOT NULL,
	[saml20$samlresponseid] bigint NOT NULL,
	PRIMARY KEY([saml20$samlrequestid],[saml20$samlresponseid]),
	CONSTRAINT [uniq_saml20$samlrequest_samlresponse_saml20$samlresponseid] UNIQUE ([saml20$samlresponseid]),
	CONSTRAINT [uniq_saml20$samlrequest_samlresponse_saml20$samlrequestid] UNIQUE ([saml20$samlrequestid]));
GO
CREATE INDEX [idx_saml20$samlrequest_samlresponse_saml20$samlresponse_saml20$samlrequest] ON [saml20$samlrequest_samlresponse] ([saml20$samlresponseid] ASC,[saml20$samlrequestid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('7d31610a-ae0e-443b-9ee3-ad6d46b81729', 'SAML20.SAMLRequest_SAMLResponse', 'saml20$samlrequest_samlresponse', '0550ede5-68a7-4dcf-ad37-389501ca7d90', 'b91df9be-c86f-4037-be6b-355ad22a04ff', 'saml20$samlrequestid', 'saml20$samlresponseid', 'idx_saml20$samlrequest_samlresponse_saml20$samlresponse_saml20$samlrequest', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$samlrequest_samlresponse_saml20$samlresponseid', '7d31610a-ae0e-443b-9ee3-ad6d46b81729', '96bb96f2-fcbf-3ca7-baa7-200aa56f933f');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$samlrequest_samlresponse_saml20$samlrequestid', '7d31610a-ae0e-443b-9ee3-ad6d46b81729', 'e8d0eccb-b86a-3ca1-b5f8-f7948be71606');
GO
CREATE TABLE [mxmodelreflection$dbsizeestimate_mxobjecttype] (
	[mxmodelreflection$dbsizeestimateid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$dbsizeestimateid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$dbsizeestimate_mxobjecttype_mxmodelreflection$dbsizeestimateid] UNIQUE ([mxmodelreflection$dbsizeestimateid]));
GO
CREATE INDEX [idx_mxmodelreflection$dbsizeestimate_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$dbsizeestimate] ON [mxmodelreflection$dbsizeestimate_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$dbsizeestimateid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('833e3806-11f4-485d-bd00-f917ff098def', 'MxModelReflection.DbSizeEstimate_MxObjectType', 'mxmodelreflection$dbsizeestimate_mxobjecttype', 'f5a397de-57b3-4bcb-9eed-4a143e8f7c58', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$dbsizeestimateid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$dbsizeestimate_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$dbsizeestimate', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$dbsizeestimate_mxobjecttype_mxmodelreflection$dbsizeestimateid', '833e3806-11f4-485d-bd00-f917ff098def', 'dd47901a-33ce-38fa-9c76-1e4b5b4d63f7');
GO
CREATE TABLE [mxmodelreflection$valuetype_mxobjecttype] (
	[mxmodelreflection$valuetypeid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$valuetypeid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$valuetype_mxobjecttype_mxmodelreflection$valuetypeid] UNIQUE ([mxmodelreflection$valuetypeid]));
GO
CREATE INDEX [idx_mxmodelreflection$valuetype_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$valuetype] ON [mxmodelreflection$valuetype_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$valuetypeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('492438f7-fc31-4344-a7e4-0a0f108641fb', 'MxModelReflection.ValueType_MxObjectType', 'mxmodelreflection$valuetype_mxobjecttype', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$valuetypeid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$valuetype_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$valuetype', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$valuetype_mxobjecttype_mxmodelreflection$valuetypeid', '492438f7-fc31-4344-a7e4-0a0f108641fb', '204622bf-3588-3314-b772-33c2641eb305');
GO
CREATE TABLE [saml20$attributeconsumingservice_roledescriptor] (
	[saml20$attributeconsumingserviceid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeconsumingserviceid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$attributeconsumingservice_roledescriptor_saml20$attributeconsumingserviceid] UNIQUE ([saml20$attributeconsumingserviceid]));
GO
CREATE INDEX [idx_saml20$attributeconsumingservice_roledescriptor_saml20$roledescriptor_saml20$attributeconsumingservice] ON [saml20$attributeconsumingservice_roledescriptor] ([saml20$roledescriptorid] ASC,[saml20$attributeconsumingserviceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d3aefffb-d392-4eb7-ada0-abd39fd632f1', 'SAML20.AttributeConsumingService_RoleDescriptor', 'saml20$attributeconsumingservice_roledescriptor', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$attributeconsumingserviceid', 'saml20$roledescriptorid', 'idx_saml20$attributeconsumingservice_roledescriptor_saml20$roledescriptor_saml20$attributeconsumingservice', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$attributeconsumingservice_roledescriptor_saml20$attributeconsumingserviceid', 'd3aefffb-d392-4eb7-ada0-abd39fd632f1', 'e689c268-06af-30a2-a1a6-83e94ee5a1ae');
GO
CREATE TABLE [saml20$attributeconsumingservicetype_servicedescription] (
	[saml20$attributeconsumingserviceid] bigint NOT NULL,
	[saml20$servicepropertyid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeconsumingserviceid],[saml20$servicepropertyid]));
GO
CREATE INDEX [idx_saml20$attributeconsumingservicetype_servicedescription_saml20$serviceproperty_saml20$attributeconsumingservice] ON [saml20$attributeconsumingservicetype_servicedescription] ([saml20$servicepropertyid] ASC,[saml20$attributeconsumingserviceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2f267590-391e-4838-96b3-5133a4425a95', 'SAML20.AttributeConsumingServiceType_ServiceDescription', 'saml20$attributeconsumingservicetype_servicedescription', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'cc40cb4e-50ad-4df6-a982-ff2a2e5732f1', 'saml20$attributeconsumingserviceid', 'saml20$servicepropertyid', 'idx_saml20$attributeconsumingservicetype_servicedescription_saml20$serviceproperty_saml20$attributeconsumingservice', 2);
GO
CREATE TABLE [saml20$attributeconsumingservicetype_servicename] (
	[saml20$attributeconsumingserviceid] bigint NOT NULL,
	[saml20$servicepropertyid] bigint NOT NULL,
	PRIMARY KEY([saml20$attributeconsumingserviceid],[saml20$servicepropertyid]),
	CONSTRAINT [uniq_saml20$attributeconsumingservicetype_servicename_saml20$attributeconsumingserviceid] UNIQUE ([saml20$attributeconsumingserviceid]));
GO
CREATE INDEX [idx_saml20$attributeconsumingservicetype_servicename_saml20$serviceproperty_saml20$attributeconsumingservice] ON [saml20$attributeconsumingservicetype_servicename] ([saml20$servicepropertyid] ASC,[saml20$attributeconsumingserviceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('6cdbf553-499f-4eda-aa0d-c7dd5ac1f908', 'SAML20.AttributeConsumingServiceType_ServiceName', 'saml20$attributeconsumingservicetype_servicename', 'faa0b6e7-b9d3-4318-b735-00ae67380cc8', 'cc40cb4e-50ad-4df6-a982-ff2a2e5732f1', 'saml20$attributeconsumingserviceid', 'saml20$servicepropertyid', 'idx_saml20$attributeconsumingservicetype_servicename_saml20$serviceproperty_saml20$attributeconsumingservice', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$attributeconsumingservicetype_servicename_saml20$attributeconsumingserviceid', '6cdbf553-499f-4eda-aa0d-c7dd5ac1f908', '32ac3b49-4850-389d-8d5f-36b682bca214');
GO
CREATE TABLE [mxmodelreflection$parameter_mxobjecttype] (
	[mxmodelreflection$parameterid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$parameterid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$parameter_mxobjecttype_mxmodelreflection$parameterid] UNIQUE ([mxmodelreflection$parameterid]));
GO
CREATE INDEX [idx_mxmodelreflection$parameter_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$parameter] ON [mxmodelreflection$parameter_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$parameterid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('3ad04e1d-7013-4e7e-ad2c-924bea353b5d', 'MxModelReflection.Parameter_MxObjectType', 'mxmodelreflection$parameter_mxobjecttype', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$parameterid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$parameter_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$parameter', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$parameter_mxobjecttype_mxmodelreflection$parameterid', '3ad04e1d-7013-4e7e-ad2c-924bea353b5d', 'c0663e3c-3d88-3abd-9dca-4553e408ce4e');
GO
CREATE TABLE [mxmodelreflection$parameter_valuetype] (
	[mxmodelreflection$parameterid] bigint NOT NULL,
	[mxmodelreflection$valuetypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$parameterid],[mxmodelreflection$valuetypeid]),
	CONSTRAINT [uniq_mxmodelreflection$parameter_valuetype_mxmodelreflection$parameterid] UNIQUE ([mxmodelreflection$parameterid]));
GO
CREATE INDEX [idx_mxmodelreflection$parameter_valuetype_mxmodelreflection$valuetype_mxmodelreflection$parameter] ON [mxmodelreflection$parameter_valuetype] ([mxmodelreflection$valuetypeid] ASC,[mxmodelreflection$parameterid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('497f081a-9b17-4332-a93d-2d1a02f9b0f3', 'MxModelReflection.Parameter_ValueType', 'mxmodelreflection$parameter_valuetype', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'mxmodelreflection$parameterid', 'mxmodelreflection$valuetypeid', 'idx_mxmodelreflection$parameter_valuetype_mxmodelreflection$valuetype_mxmodelreflection$parameter', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$parameter_valuetype_mxmodelreflection$parameterid', '497f081a-9b17-4332-a93d-2d1a02f9b0f3', 'c6733485-4cef-34b2-97c6-a4c0a61adff0');
GO
CREATE TABLE [system$unreferencedfile_xasinstance] (
	[system$unreferencedfileid] bigint NOT NULL,
	[system$xasinstanceid] bigint NOT NULL,
	PRIMARY KEY([system$unreferencedfileid],[system$xasinstanceid]),
	CONSTRAINT [uniq_system$unreferencedfile_xasinstance_system$unreferencedfileid] UNIQUE ([system$unreferencedfileid]));
GO
CREATE INDEX [idx_system$unreferencedfile_xasinstance_system$xasinstance_system$unreferencedfile] ON [system$unreferencedfile_xasinstance] ([system$xasinstanceid] ASC,[system$unreferencedfileid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('1cbb5da6-dbb6-447a-8ea2-b193035320c7', 'System.UnreferencedFile_XASInstance', 'system$unreferencedfile_xasinstance', '4e336d7d-71e8-41f4-9f07-8f0646543e81', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'system$unreferencedfileid', 'system$xasinstanceid', 'idx_system$unreferencedfile_xasinstance_system$xasinstance_system$unreferencedfile', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$unreferencedfile_xasinstance_system$unreferencedfileid', '1cbb5da6-dbb6-447a-8ea2-b193035320c7', 'dd0763aa-e3a9-35bd-98ce-4d39d939ad40');
GO
CREATE TABLE [saml20$entitiesdescriptor_keyinfo] (
	[saml20$entitiesdescriptorid] bigint NOT NULL,
	[saml20$keyinfoid] bigint NOT NULL,
	PRIMARY KEY([saml20$entitiesdescriptorid],[saml20$keyinfoid]),
	CONSTRAINT [uniq_saml20$entitiesdescriptor_keyinfo_saml20$entitiesdescriptorid] UNIQUE ([saml20$entitiesdescriptorid]));
GO
CREATE INDEX [idx_saml20$entitiesdescriptor_keyinfo_saml20$keyinfo_saml20$entitiesdescriptor] ON [saml20$entitiesdescriptor_keyinfo] ([saml20$keyinfoid] ASC,[saml20$entitiesdescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('5ba69937-1785-4c39-91eb-8f5ccd4fabeb', 'SAML20.EntitiesDescriptor_KeyInfo', 'saml20$entitiesdescriptor_keyinfo', '41287618-1d36-43d4-ba70-d34884a5017d', 'bd027dd6-47cc-494f-b6ab-5fc140a4b9f2', 'saml20$entitiesdescriptorid', 'saml20$keyinfoid', 'idx_saml20$entitiesdescriptor_keyinfo_saml20$keyinfo_saml20$entitiesdescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$entitiesdescriptor_keyinfo_saml20$entitiesdescriptorid', '5ba69937-1785-4c39-91eb-8f5ccd4fabeb', 'c6613903-b9b6-3f86-9391-cee645b36b6a');
GO
CREATE TABLE [saml20$entitiesdescriptor_idpmetadata] (
	[saml20$entitiesdescriptorid] bigint NOT NULL,
	[saml20$idpmetadataid] bigint NOT NULL,
	PRIMARY KEY([saml20$entitiesdescriptorid],[saml20$idpmetadataid]),
	CONSTRAINT [uniq_saml20$entitiesdescriptor_idpmetadata_saml20$entitiesdescriptorid] UNIQUE ([saml20$entitiesdescriptorid]));
GO
CREATE INDEX [idx_saml20$entitiesdescriptor_idpmetadata_saml20$idpmetadata_saml20$entitiesdescriptor] ON [saml20$entitiesdescriptor_idpmetadata] ([saml20$idpmetadataid] ASC,[saml20$entitiesdescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('7c7c1b6b-b94d-4ac9-bcfe-0cc497a5e233', 'SAML20.EntitiesDescriptor_IdPMetadata', 'saml20$entitiesdescriptor_idpmetadata', '41287618-1d36-43d4-ba70-d34884a5017d', '2e765af5-79d0-4b5c-8d04-5a7e8691b4a7', 'saml20$entitiesdescriptorid', 'saml20$idpmetadataid', 'idx_saml20$entitiesdescriptor_idpmetadata_saml20$idpmetadata_saml20$entitiesdescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$entitiesdescriptor_idpmetadata_saml20$entitiesdescriptorid', '7c7c1b6b-b94d-4ac9-bcfe-0cc497a5e233', '9e9689da-2b14-3476-a7cb-b869ff379fa6');
GO
CREATE TABLE [system$workflowdefinition_currentworkflowversion] (
	[system$workflowdefinitionid] bigint NOT NULL,
	[system$workflowversionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowdefinitionid],[system$workflowversionid]),
	CONSTRAINT [uniq_system$workflowdefinition_currentworkflowversion_system$workflowdefinitionid] UNIQUE ([system$workflowdefinitionid]));
GO
CREATE INDEX [idx_system$workflowdefinition_currentworkflowversion_system$workflowversion_system$workflowdefinition] ON [system$workflowdefinition_currentworkflowversion] ([system$workflowversionid] ASC,[system$workflowdefinitionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2b065cdd-3d2c-4517-9727-ced57d97fd03', 'System.WorkflowDefinition_CurrentWorkflowVersion', 'system$workflowdefinition_currentworkflowversion', '5c570d3b-7b31-44fe-abd6-269a234584c5', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'system$workflowdefinitionid', 'system$workflowversionid', 'idx_system$workflowdefinition_currentworkflowversion_system$workflowversion_system$workflowdefinition', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowdefinition_currentworkflowversion_system$workflowdefinitionid', '2b065cdd-3d2c-4517-9727-ced57d97fd03', 'eb384fa0-c9cd-3568-bdd6-1501cb6e352e');
GO
CREATE TABLE [system$workflowusertaskdefinition_workflowdefinition] (
	[system$workflowusertaskdefinitionid] bigint NOT NULL,
	[system$workflowdefinitionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskdefinitionid],[system$workflowdefinitionid]),
	CONSTRAINT [uniq_system$workflowusertaskdefinition_workflowdefinition_system$workflowusertaskdefinitionid] UNIQUE ([system$workflowusertaskdefinitionid]));
GO
CREATE INDEX [idx_system$workflowusertaskdefinition_workflowdefinition_system$workflowdefinition_system$workflowusertaskdefinition] ON [system$workflowusertaskdefinition_workflowdefinition] ([system$workflowdefinitionid] ASC,[system$workflowusertaskdefinitionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('685c576c-19af-4ea7-983d-ece147c1cebc', 'System.WorkflowUserTaskDefinition_WorkflowDefinition', 'system$workflowusertaskdefinition_workflowdefinition', 'e09e866f-288b-475c-9465-792cde8b878c', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'system$workflowusertaskdefinitionid', 'system$workflowdefinitionid', 'idx_system$workflowusertaskdefinition_workflowdefinition_system$workflowdefinition_system$workflowusertaskdefinition', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowusertaskdefinition_workflowdefinition_system$workflowusertaskdefinitionid', '685c576c-19af-4ea7-983d-ece147c1cebc', 'ce3750f0-8db1-37ca-95b5-892696a3d9e2');
GO
CREATE TABLE [saml20$roledescriptor_entitydescriptor] (
	[saml20$roledescriptorid] bigint NOT NULL,
	[saml20$entitydescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$roledescriptorid],[saml20$entitydescriptorid]),
	CONSTRAINT [uniq_saml20$roledescriptor_entitydescriptor_saml20$roledescriptorid] UNIQUE ([saml20$roledescriptorid]));
GO
CREATE INDEX [idx_saml20$roledescriptor_entitydescriptor_saml20$entitydescriptor_saml20$roledescriptor] ON [saml20$roledescriptor_entitydescriptor] ([saml20$entitydescriptorid] ASC,[saml20$roledescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('445f18c2-bc7e-4f2a-b731-32d89f44f38e', 'SAML20.RoleDescriptor_EntityDescriptor', 'saml20$roledescriptor_entitydescriptor', '8c7c691b-0561-4729-b9da-ad76c4403f4f', '50527e51-152a-40ca-98fa-75a0b24193bb', 'saml20$roledescriptorid', 'saml20$entitydescriptorid', 'idx_saml20$roledescriptor_entitydescriptor_saml20$entitydescriptor_saml20$roledescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$roledescriptor_entitydescriptor_saml20$roledescriptorid', '445f18c2-bc7e-4f2a-b731-32d89f44f38e', '5d7ec847-6661-31fb-a244-3659beff0686');
GO
CREATE TABLE [saml20$roledescriptor_organization] (
	[saml20$roledescriptorid] bigint NOT NULL,
	[saml20$organizationid] bigint NOT NULL,
	PRIMARY KEY([saml20$roledescriptorid],[saml20$organizationid]),
	CONSTRAINT [uniq_saml20$roledescriptor_organization_saml20$roledescriptorid] UNIQUE ([saml20$roledescriptorid]));
GO
CREATE INDEX [idx_saml20$roledescriptor_organization_saml20$organization_saml20$roledescriptor] ON [saml20$roledescriptor_organization] ([saml20$organizationid] ASC,[saml20$roledescriptorid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('b2cf5cd7-91fb-4523-a571-d05224cce71d', 'SAML20.RoleDescriptor_Organization', 'saml20$roledescriptor_organization', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'ad6340d4-b92a-4a0c-9239-9ce623c0c372', 'saml20$roledescriptorid', 'saml20$organizationid', 'idx_saml20$roledescriptor_organization_saml20$organization_saml20$roledescriptor', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$roledescriptor_organization_saml20$roledescriptorid', 'b2cf5cd7-91fb-4523-a571-d05224cce71d', 'e596c85d-bd71-3954-9a03-d758df857333');
GO
CREATE TABLE [mxmodelreflection$captions] (
	[mxmodelreflection$mxobjectenumvalueid] bigint NOT NULL,
	[mxmodelreflection$mxobjectenumcaptionsid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectenumvalueid],[mxmodelreflection$mxobjectenumcaptionsid]));
GO
CREATE INDEX [idx_mxmodelreflection$captions_mxmodelreflection$mxobjectenumcaptions_mxmodelreflection$mxobjectenumvalue] ON [mxmodelreflection$captions] ([mxmodelreflection$mxobjectenumcaptionsid] ASC,[mxmodelreflection$mxobjectenumvalueid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('591d258e-2a65-4571-b2e1-2538b2dca6ea', 'MxModelReflection.Captions', 'mxmodelreflection$captions', '1aedc225-dcc0-48b2-84f9-3e435eaf89a9', 'c68f5bf4-27ae-4833-829d-737ff277dab6', 'mxmodelreflection$mxobjectenumvalueid', 'mxmodelreflection$mxobjectenumcaptionsid', 'idx_mxmodelreflection$captions_mxmodelreflection$mxobjectenumcaptions_mxmodelreflection$mxobjectenumvalue', 2);
GO
CREATE TABLE [encryption$secretkey_publickey] (
	[encryption$pgpcertificateid1] bigint NOT NULL,
	[encryption$pgpcertificateid2] bigint NOT NULL,
	PRIMARY KEY([encryption$pgpcertificateid1],[encryption$pgpcertificateid2]),
	CONSTRAINT [uniq_encryption$secretkey_publickey_encryption$pgpcertificateid1] UNIQUE ([encryption$pgpcertificateid1]));
GO
CREATE INDEX [idx_encryption$secretkey_publickey_encryption$pgpcertificate_encryption$pgpcertificate] ON [encryption$secretkey_publickey] ([encryption$pgpcertificateid2] ASC,[encryption$pgpcertificateid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('46ef2359-e4e8-47f9-b234-8be9480a7ec8', 'Encryption.SecretKey_PublicKey', 'encryption$secretkey_publickey', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', '7ced6dc8-1a8e-4abf-8910-c84fd7aa1016', 'encryption$pgpcertificateid1', 'encryption$pgpcertificateid2', 'idx_encryption$secretkey_publickey_encryption$pgpcertificate_encryption$pgpcertificate', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_encryption$secretkey_publickey_encryption$pgpcertificateid1', '46ef2359-e4e8-47f9-b234-8be9480a7ec8', 'fb8f711d-9ac5-385d-962a-d8be6f79d914');
GO
CREATE TABLE [saml20$organization_organizationurl] (
	[saml20$organizationpropertyid] bigint NOT NULL,
	[saml20$organizationid] bigint NOT NULL,
	PRIMARY KEY([saml20$organizationpropertyid],[saml20$organizationid]),
	CONSTRAINT [uniq_saml20$organization_organizationurl_saml20$organizationpropertyid] UNIQUE ([saml20$organizationpropertyid]));
GO
CREATE INDEX [idx_saml20$organization_organizationurl_saml20$organization_saml20$organizationproperty] ON [saml20$organization_organizationurl] ([saml20$organizationid] ASC,[saml20$organizationpropertyid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('927a000f-9437-4cbb-b4c4-5db91d1d3c98', 'SAML20.Organization_OrganizationURL', 'saml20$organization_organizationurl', 'b6bf5da5-8043-4474-a926-19b295e0b71d', 'ad6340d4-b92a-4a0c-9239-9ce623c0c372', 'saml20$organizationpropertyid', 'saml20$organizationid', 'idx_saml20$organization_organizationurl_saml20$organization_saml20$organizationproperty', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$organization_organizationurl_saml20$organizationpropertyid', '927a000f-9437-4cbb-b4c4-5db91d1d3c98', 'b73ab676-bb20-3445-b57a-50853b2f26d3');
GO
CREATE TABLE [saml20$organization_organizationname] (
	[saml20$organizationpropertyid] bigint NOT NULL,
	[saml20$organizationid] bigint NOT NULL,
	PRIMARY KEY([saml20$organizationpropertyid],[saml20$organizationid]),
	CONSTRAINT [uniq_saml20$organization_organizationname_saml20$organizationpropertyid] UNIQUE ([saml20$organizationpropertyid]));
GO
CREATE INDEX [idx_saml20$organization_organizationname_saml20$organization_saml20$organizationproperty] ON [saml20$organization_organizationname] ([saml20$organizationid] ASC,[saml20$organizationpropertyid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('149b9bb1-7453-4353-bc86-dd5101fb1109', 'SAML20.Organization_OrganizationName', 'saml20$organization_organizationname', 'b6bf5da5-8043-4474-a926-19b295e0b71d', 'ad6340d4-b92a-4a0c-9239-9ce623c0c372', 'saml20$organizationpropertyid', 'saml20$organizationid', 'idx_saml20$organization_organizationname_saml20$organization_saml20$organizationproperty', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$organization_organizationname_saml20$organizationpropertyid', '149b9bb1-7453-4353-bc86-dd5101fb1109', '17b1fc04-bb2e-3b01-a8b2-0cfaaa6582ef');
GO
CREATE TABLE [saml20$organization_organizationdisplayname] (
	[saml20$organizationpropertyid] bigint NOT NULL,
	[saml20$organizationid] bigint NOT NULL,
	PRIMARY KEY([saml20$organizationpropertyid],[saml20$organizationid]),
	CONSTRAINT [uniq_saml20$organization_organizationdisplayname_saml20$organizationpropertyid] UNIQUE ([saml20$organizationpropertyid]));
GO
CREATE INDEX [idx_saml20$organization_organizationdisplayname_saml20$organization_saml20$organizationproperty] ON [saml20$organization_organizationdisplayname] ([saml20$organizationid] ASC,[saml20$organizationpropertyid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d19eccd7-f39e-4586-85bc-22341e134f73', 'SAML20.Organization_OrganizationDisplayName', 'saml20$organization_organizationdisplayname', 'b6bf5da5-8043-4474-a926-19b295e0b71d', 'ad6340d4-b92a-4a0c-9239-9ce623c0c372', 'saml20$organizationpropertyid', 'saml20$organizationid', 'idx_saml20$organization_organizationdisplayname_saml20$organization_saml20$organizationproperty', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$organization_organizationdisplayname_saml20$organizationpropertyid', 'd19eccd7-f39e-4586-85bc-22341e134f73', '54f7e2b8-c6fa-32f4-9f13-d9803d74b4cf');
GO
CREATE TABLE [system$tokeninformation_user] (
	[system$tokeninformationid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$tokeninformationid],[system$userid]),
	CONSTRAINT [uniq_system$tokeninformation_user_system$tokeninformationid] UNIQUE ([system$tokeninformationid]));
GO
CREATE INDEX [idx_system$tokeninformation_user_system$user_system$tokeninformation] ON [system$tokeninformation_user] ([system$userid] ASC,[system$tokeninformationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('20ca86b2-5a00-4131-aee1-427cb2e94425', 'System.TokenInformation_User', 'system$tokeninformation_user', '09b2f0fe-4a11-4afc-a16e-94992a3ebc3d', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$tokeninformationid', 'system$userid', 'idx_system$tokeninformation_user_system$user_system$tokeninformation', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$tokeninformation_user_system$tokeninformationid', '20ca86b2-5a00-4131-aee1-427cb2e94425', '4abdbc47-924f-3c57-9257-190d5521d13e');
GO
CREATE TABLE [exsm_recipemgmt$recipemgmtuserconfig_account] (
	[exsm_recipemgmt$recipemgmtuserconfigid] bigint NOT NULL,
	[administration$accountid] bigint NOT NULL,
	PRIMARY KEY([exsm_recipemgmt$recipemgmtuserconfigid],[administration$accountid]),
	CONSTRAINT [uniq_exsm_recipemgmt$recipemgmtuserconfig_account_administration$accountid] UNIQUE ([administration$accountid]),
	CONSTRAINT [uniq_exsm_recipemgmt$recipemgmtuserconfig_account_exsm_recipemgmt$recipemgmtuserconfigid] UNIQUE ([exsm_recipemgmt$recipemgmtuserconfigid]));
GO
CREATE INDEX [idx_exsm_recipemgmt$recipemgmtuserconfig_account_administration$account_exsm_recipemgmt$recipemgmtuserconfig] ON [exsm_recipemgmt$recipemgmtuserconfig_account] ([administration$accountid] ASC,[exsm_recipemgmt$recipemgmtuserconfigid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('9115c39a-e253-4b5c-a3d1-11a34c812bb5', 'EXSM_RecipeMgmt.RecipeMgmtUserConfig_Account', 'exsm_recipemgmt$recipemgmtuserconfig_account', 'ac828022-994f-48dd-9add-57ad0e8bce36', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'exsm_recipemgmt$recipemgmtuserconfigid', 'administration$accountid', 'idx_exsm_recipemgmt$recipemgmtuserconfig_account_administration$account_exsm_recipemgmt$recipemgmtuserconfig', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_recipemgmt$recipemgmtuserconfig_account_administration$accountid', '9115c39a-e253-4b5c-a3d1-11a34c812bb5', '2189cb8d-6a5d-3ac2-a52b-5a7e6a22e255');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_recipemgmt$recipemgmtuserconfig_account_exsm_recipemgmt$recipemgmtuserconfigid', '9115c39a-e253-4b5c-a3d1-11a34c812bb5', 'f66300b8-00cd-3b3f-b7e6-1c10978556b7');
GO
CREATE TABLE [exsm_experimentmgmt$expmgmtuserconfig_account] (
	[exsm_experimentmgmt$expmgmtuserconfigid] bigint NOT NULL,
	[administration$accountid] bigint NOT NULL,
	PRIMARY KEY([exsm_experimentmgmt$expmgmtuserconfigid],[administration$accountid]),
	CONSTRAINT [uniq_exsm_experimentmgmt$expmgmtuserconfig_account_administration$accountid] UNIQUE ([administration$accountid]),
	CONSTRAINT [uniq_exsm_experimentmgmt$expmgmtuserconfig_account_exsm_experimentmgmt$expmgmtuserconfigid] UNIQUE ([exsm_experimentmgmt$expmgmtuserconfigid]));
GO
CREATE INDEX [idx_exsm_experimentmgmt$expmgmtuserconfig_account_administration$account_exsm_experimentmgmt$expmgmtuserconfig] ON [exsm_experimentmgmt$expmgmtuserconfig_account] ([administration$accountid] ASC,[exsm_experimentmgmt$expmgmtuserconfigid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('ca964d2f-c21d-4a98-ad24-e51bbfe3d4bc', 'EXSM_ExperimentMgmt.ExpMgmtUserConfig_Account', 'exsm_experimentmgmt$expmgmtuserconfig_account', '0eb6734c-7785-4fab-b58c-5e1758b0b636', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'exsm_experimentmgmt$expmgmtuserconfigid', 'administration$accountid', 'idx_exsm_experimentmgmt$expmgmtuserconfig_account_administration$account_exsm_experimentmgmt$expmgmtuserconfig', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_experimentmgmt$expmgmtuserconfig_account_administration$accountid', 'ca964d2f-c21d-4a98-ad24-e51bbfe3d4bc', 'e0f296ff-7ecb-3ed6-bef5-37082580eefe');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_experimentmgmt$expmgmtuserconfig_account_exsm_experimentmgmt$expmgmtuserconfigid', 'ca964d2f-c21d-4a98-ad24-e51bbfe3d4bc', 'fdba61cc-5f78-3694-875f-0224112608ce');
GO
CREATE TABLE [mxmodelreflection$mxobjecttype_subclassof_mxobjecttype] (
	[mxmodelreflection$mxobjecttypeid1] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid2] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjecttypeid1],[mxmodelreflection$mxobjecttypeid2]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjecttype_subclassof_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjecttype] ON [mxmodelreflection$mxobjecttype_subclassof_mxobjecttype] ([mxmodelreflection$mxobjecttypeid2] ASC,[mxmodelreflection$mxobjecttypeid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('41392426-e696-4365-ba71-58b626f71530', 'MxModelReflection.MxObjectType_SubClassOf_MxObjectType', 'mxmodelreflection$mxobjecttype_subclassof_mxobjecttype', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$mxobjecttypeid1', 'mxmodelreflection$mxobjecttypeid2', 'idx_mxmodelreflection$mxobjecttype_subclassof_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjecttype', 2);
GO
CREATE TABLE [mxmodelreflection$mxobjecttype_module] (
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	[mxmodelreflection$moduleid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjecttypeid],[mxmodelreflection$moduleid]),
	CONSTRAINT [uniq_mxmodelreflection$mxobjecttype_module_mxmodelreflection$mxobjecttypeid] UNIQUE ([mxmodelreflection$mxobjecttypeid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjecttype_module_mxmodelreflection$module_mxmodelreflection$mxobjecttype] ON [mxmodelreflection$mxobjecttype_module] ([mxmodelreflection$moduleid] ASC,[mxmodelreflection$mxobjecttypeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('5ac3ceb1-df16-48c6-afdf-dd55e55bdb49', 'MxModelReflection.MxObjectType_Module', 'mxmodelreflection$mxobjecttype_module', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', '2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'mxmodelreflection$mxobjecttypeid', 'mxmodelreflection$moduleid', 'idx_mxmodelreflection$mxobjecttype_module_mxmodelreflection$module_mxmodelreflection$mxobjecttype', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$mxobjecttype_module_mxmodelreflection$mxobjecttypeid', '5ac3ceb1-df16-48c6-afdf-dd55e55bdb49', '0dcf69a0-d1ab-3d09-9120-041a811bc2cd');
GO
CREATE TABLE [usercommons$claimentityattribute_userprovisioning] (
	[usercommons$claimentityattributeid] bigint NOT NULL,
	[usercommons$userprovisioningid] bigint NOT NULL,
	PRIMARY KEY([usercommons$claimentityattributeid],[usercommons$userprovisioningid]),
	CONSTRAINT [uniq_usercommons$claimentityattribute_userprovisioning_usercommons$claimentityattributeid] UNIQUE ([usercommons$claimentityattributeid]));
GO
CREATE INDEX [idx_usercommons$claimentityattribute_userprovisioning_usercommons$userprovisioning_usercommons$claimentityattribute] ON [usercommons$claimentityattribute_userprovisioning] ([usercommons$userprovisioningid] ASC,[usercommons$claimentityattributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('7940dc8c-5880-462b-8b32-a2d98737174d', 'UserCommons.ClaimEntityAttribute_UserProvisioning', 'usercommons$claimentityattribute_userprovisioning', 'a648ef0c-10cd-4a08-85e5-02165439a164', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'usercommons$claimentityattributeid', 'usercommons$userprovisioningid', 'idx_usercommons$claimentityattribute_userprovisioning_usercommons$userprovisioning_usercommons$claimentityattribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$claimentityattribute_userprovisioning_usercommons$claimentityattributeid', '7940dc8c-5880-462b-8b32-a2d98737174d', 'e430475c-4d58-3d6b-95fe-1b04502f7bcf');
GO
CREATE TABLE [usercommons$claimentityattribute_claim] (
	[usercommons$claimentityattributeid] bigint NOT NULL,
	[usercommons$claimid] bigint NOT NULL,
	PRIMARY KEY([usercommons$claimentityattributeid],[usercommons$claimid]),
	CONSTRAINT [uniq_usercommons$claimentityattribute_claim_usercommons$claimentityattributeid] UNIQUE ([usercommons$claimentityattributeid]));
GO
CREATE INDEX [idx_usercommons$claimentityattribute_claim_usercommons$claim_usercommons$claimentityattribute] ON [usercommons$claimentityattribute_claim] ([usercommons$claimid] ASC,[usercommons$claimentityattributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d5dff01c-8eb1-4fef-a399-b1dfd5ae3c4f', 'UserCommons.ClaimEntityAttribute_Claim', 'usercommons$claimentityattribute_claim', 'a648ef0c-10cd-4a08-85e5-02165439a164', 'ea6ca13e-3ef5-44aa-b22a-60f69fb100bd', 'usercommons$claimentityattributeid', 'usercommons$claimid', 'idx_usercommons$claimentityattribute_claim_usercommons$claim_usercommons$claimentityattribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$claimentityattribute_claim_usercommons$claimentityattributeid', 'd5dff01c-8eb1-4fef-a399-b1dfd5ae3c4f', '29e678ea-66d2-3d21-9e80-1279efb89193');
GO
CREATE TABLE [usercommons$claimentityattribute_entityattribute] (
	[usercommons$claimentityattributeid] bigint NOT NULL,
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	PRIMARY KEY([usercommons$claimentityattributeid],[mxmodelreflection$mxobjectmemberid]),
	CONSTRAINT [uniq_usercommons$claimentityattribute_entityattribute_usercommons$claimentityattributeid] UNIQUE ([usercommons$claimentityattributeid]));
GO
CREATE INDEX [idx_usercommons$claimentityattribute_entityattribute_mxmodelreflection$mxobjectmember_usercommons$claimentityattribute] ON [usercommons$claimentityattribute_entityattribute] ([mxmodelreflection$mxobjectmemberid] ASC,[usercommons$claimentityattributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('45c2ef3c-b84c-4291-962b-8616fdbf9676', 'UserCommons.ClaimEntityAttribute_EntityAttribute', 'usercommons$claimentityattribute_entityattribute', 'a648ef0c-10cd-4a08-85e5-02165439a164', '398a1f70-2b2c-408e-8778-f4a923fda765', 'usercommons$claimentityattributeid', 'mxmodelreflection$mxobjectmemberid', 'idx_usercommons$claimentityattribute_entityattribute_mxmodelreflection$mxobjectmember_usercommons$claimentityattribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$claimentityattribute_entityattribute_usercommons$claimentityattributeid', '45c2ef3c-b84c-4291-962b-8616fdbf9676', 'a5ea5afe-4db1-37da-a1f0-ef8444969cf9');
GO
CREATE TABLE [saml20$spmetadata_keystore] (
	[saml20$spmetadataid] bigint NOT NULL,
	[saml20$keystoreid] bigint NOT NULL,
	PRIMARY KEY([saml20$spmetadataid],[saml20$keystoreid]),
	CONSTRAINT [uniq_saml20$spmetadata_keystore_saml20$keystoreid] UNIQUE ([saml20$keystoreid]),
	CONSTRAINT [uniq_saml20$spmetadata_keystore_saml20$spmetadataid] UNIQUE ([saml20$spmetadataid]));
GO
CREATE INDEX [idx_saml20$spmetadata_keystore_saml20$keystore_saml20$spmetadata] ON [saml20$spmetadata_keystore] ([saml20$keystoreid] ASC,[saml20$spmetadataid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('521c4854-9de4-4a69-9a9f-8ba01feb94cc', 'SAML20.SPMetadata_KeyStore', 'saml20$spmetadata_keystore', '2125eb04-8bb1-42af-a5b2-53ead7168a69', '712b9e2b-202a-496a-896b-61440043e0b8', 'saml20$spmetadataid', 'saml20$keystoreid', 'idx_saml20$spmetadata_keystore_saml20$keystore_saml20$spmetadata', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$spmetadata_keystore_saml20$keystoreid', '521c4854-9de4-4a69-9a9f-8ba01feb94cc', 'd89651f5-ff59-3a74-b82e-e343e4757cde');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$spmetadata_keystore_saml20$spmetadataid', '521c4854-9de4-4a69-9a9f-8ba01feb94cc', '1ee08882-424b-37e2-9d5a-efcdc8563ccd');
GO
CREATE TABLE [saml20$endpoint_roledescriptor] (
	[saml20$endpointid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$endpointid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$endpoint_roledescriptor_saml20$endpointid] UNIQUE ([saml20$endpointid]));
GO
CREATE INDEX [idx_saml20$endpoint_roledescriptor_saml20$roledescriptor_saml20$endpoint] ON [saml20$endpoint_roledescriptor] ([saml20$roledescriptorid] ASC,[saml20$endpointid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('32dc02c8-d092-4b1c-b707-14605436f082', 'SAML20.Endpoint_RoleDescriptor', 'saml20$endpoint_roledescriptor', '6ff6f1e1-6b77-4544-b9ec-4058e85708f1', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$endpointid', 'saml20$roledescriptorid', 'idx_saml20$endpoint_roledescriptor_saml20$roledescriptor_saml20$endpoint', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$endpoint_roledescriptor_saml20$endpointid', '32dc02c8-d092-4b1c-b707-14605436f082', '3bfbef0b-2270-3031-934e-378dc8e139f6');
GO
CREATE TABLE [exsm_wipdatasetup$wipdatasetupuserconfig_account] (
	[exsm_wipdatasetup$wipdatasetupuserconfigid] bigint NOT NULL,
	[administration$accountid] bigint NOT NULL,
	PRIMARY KEY([exsm_wipdatasetup$wipdatasetupuserconfigid],[administration$accountid]),
	CONSTRAINT [uniq_exsm_wipdatasetup$wipdatasetupuserconfig_account_administration$accountid] UNIQUE ([administration$accountid]),
	CONSTRAINT [uniq_exsm_wipdatasetup$wipdatasetupuserconfig_account_exsm_wipdatasetup$wipdatasetupuserconfigid] UNIQUE ([exsm_wipdatasetup$wipdatasetupuserconfigid]));
GO
CREATE INDEX [idx_exsm_wipdatasetup$wipdatasetupuserconfig_account_administration$account_exsm_wipdatasetup$wipdatasetupuserconfig] ON [exsm_wipdatasetup$wipdatasetupuserconfig_account] ([administration$accountid] ASC,[exsm_wipdatasetup$wipdatasetupuserconfigid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('87b770fc-f0a8-46d6-a956-62a3ec22f7a6', 'EXSM_WIPDataSetup.WIPDataSetupUserConfig_Account', 'exsm_wipdatasetup$wipdatasetupuserconfig_account', 'd847da64-8ff6-4bf7-8f0d-3c747031e0b9', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'exsm_wipdatasetup$wipdatasetupuserconfigid', 'administration$accountid', 'idx_exsm_wipdatasetup$wipdatasetupuserconfig_account_administration$account_exsm_wipdatasetup$wipdatasetupuserconfig', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_wipdatasetup$wipdatasetupuserconfig_account_administration$accountid', '87b770fc-f0a8-46d6-a956-62a3ec22f7a6', 'bbccbf38-9ab9-3a6f-b5d0-eeff9807acc0');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_wipdatasetup$wipdatasetupuserconfig_account_exsm_wipdatasetup$wipdatasetupuserconfigid', '87b770fc-f0a8-46d6-a956-62a3ec22f7a6', '0ac30011-83ce-3786-82df-69315a3c01ed');
GO
CREATE TABLE [disw_designsystem$accountprofilepicture_accountextension] (
	[disw_designsystem$accountprofilepictureid] bigint NOT NULL,
	[disw_designsystem$accountextensionid] bigint NOT NULL,
	PRIMARY KEY([disw_designsystem$accountprofilepictureid],[disw_designsystem$accountextensionid]),
	CONSTRAINT [uniq_disw_designsystem$accountprofilepicture_accountextension_disw_designsystem$accountextensionid] UNIQUE ([disw_designsystem$accountextensionid]),
	CONSTRAINT [uniq_disw_designsystem$accountprofilepicture_accountextension_disw_designsystem$accountprofilepictureid] UNIQUE ([disw_designsystem$accountprofilepictureid]));
GO
CREATE INDEX [idx_disw_designsystem$accountprofilepicture_accountextension] ON [disw_designsystem$accountprofilepicture_accountextension] ([disw_designsystem$accountextensionid] ASC,[disw_designsystem$accountprofilepictureid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('df2dd36c-feec-47d1-841e-fefa2461ac20', 'DISW_DesignSystem.AccountProfilePicture_AccountExtension', 'disw_designsystem$accountprofilepicture_accountextension', 'efffc7a2-c42d-4d08-abda-3dd03d539994', '851ba5bf-ee08-4920-8155-c987c34bb925', 'disw_designsystem$accountprofilepictureid', 'disw_designsystem$accountextensionid', 'idx_disw_designsystem$accountprofilepicture_accountextension', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_disw_designsystem$accountprofilepicture_accountextension_disw_designsystem$accountextensionid', 'df2dd36c-feec-47d1-841e-fefa2461ac20', '3aa270da-d889-3f73-aa17-8dd92b23d803');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_disw_designsystem$accountprofilepicture_accountextension_disw_designsystem$accountprofilepictureid', 'df2dd36c-feec-47d1-841e-fefa2461ac20', 'dc9b11bc-0348-39a6-8ba1-07de17640b0d');
GO
CREATE TABLE [system$session_user] (
	[system$sessionid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$sessionid],[system$userid]),
	CONSTRAINT [uniq_system$session_user_system$sessionid] UNIQUE ([system$sessionid]));
GO
CREATE INDEX [idx_system$session_user_system$user_system$session] ON [system$session_user] ([system$userid] ASC,[system$sessionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('546aaff5-62e1-40ce-ab45-d40d0a0478f1', 'System.Session_User', 'system$session_user', '37f9fd49-5318-4c63-9a51-f761779b202f', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$sessionid', 'system$userid', 'idx_system$session_user_system$user_system$session', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$session_user_system$sessionid', '546aaff5-62e1-40ce-ab45-d40d0a0478f1', '142c3a11-004d-3f79-916b-d0347144970b');
GO
CREATE TABLE [system$userreportinfo_user] (
	[system$userreportinfoid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$userreportinfoid],[system$userid]),
	CONSTRAINT [uniq_system$userreportinfo_user_system$userreportinfoid] UNIQUE ([system$userreportinfoid]));
GO
CREATE INDEX [idx_system$userreportinfo_user_system$user_system$userreportinfo] ON [system$userreportinfo_user] ([system$userid] ASC,[system$userreportinfoid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d88b344c-b1e5-4759-b60e-0348e63ac445', 'System.UserReportInfo_User', 'system$userreportinfo_user', '1c90a770-98ef-45df-9267-b87973cc6581', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$userreportinfoid', 'system$userid', 'idx_system$userreportinfo_user_system$user_system$userreportinfo', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$userreportinfo_user_system$userreportinfoid', 'd88b344c-b1e5-4759-b60e-0348e63ac445', '677bda5e-706d-3d41-b007-247640ca3be1');
GO
CREATE TABLE [system$workflow_parentworkflow] (
	[system$workflowid1] bigint NOT NULL,
	[system$workflowid2] bigint NOT NULL,
	PRIMARY KEY([system$workflowid1],[system$workflowid2]),
	CONSTRAINT [uniq_system$workflow_parentworkflow_system$workflowid1] UNIQUE ([system$workflowid1]));
GO
CREATE INDEX [idx_system$workflow_parentworkflow_system$workflow_system$workflow] ON [system$workflow_parentworkflow] ([system$workflowid2] ASC,[system$workflowid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('79314052-9dcc-47e5-954b-80ec631ddd41', 'System.Workflow_ParentWorkflow', 'system$workflow_parentworkflow', '2ae37bf5-ecb8-4c55-b967-d7383925b208', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'system$workflowid1', 'system$workflowid2', 'idx_system$workflow_parentworkflow_system$workflow_system$workflow', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflow_parentworkflow_system$workflowid1', '79314052-9dcc-47e5-954b-80ec631ddd41', '84b93b3b-9d0a-342e-b78b-17657b92fc22');
GO
CREATE TABLE [system$workflow_workflowdefinition] (
	[system$workflowid] bigint NOT NULL,
	[system$workflowdefinitionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowid],[system$workflowdefinitionid]),
	CONSTRAINT [uniq_system$workflow_workflowdefinition_system$workflowid] UNIQUE ([system$workflowid]));
GO
CREATE INDEX [idx_system$workflow_workflowdefinition_system$workflowdefinition_system$workflow] ON [system$workflow_workflowdefinition] ([system$workflowdefinitionid] ASC,[system$workflowid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('77c87c19-f28d-4ca3-870c-351722cf5e9e', 'System.Workflow_WorkflowDefinition', 'system$workflow_workflowdefinition', '2ae37bf5-ecb8-4c55-b967-d7383925b208', '5c570d3b-7b31-44fe-abd6-269a234584c5', 'system$workflowid', 'system$workflowdefinitionid', 'idx_system$workflow_workflowdefinition_system$workflowdefinition_system$workflow', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflow_workflowdefinition_system$workflowid', '77c87c19-f28d-4ca3-870c-351722cf5e9e', '593e832a-6cbc-3208-b1a1-06b8b873428f');
GO
CREATE TABLE [system$workflow_currentactivity] (
	[system$workflowid] bigint NOT NULL,
	[system$workflowactivityid] bigint NOT NULL,
	PRIMARY KEY([system$workflowid],[system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflow_currentactivity_system$workflowactivity_system$workflow] ON [system$workflow_currentactivity] ([system$workflowactivityid] ASC,[system$workflowid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('58aa640e-8db7-479b-9f91-2425b009ee06', 'System.Workflow_CurrentActivity', 'system$workflow_currentactivity', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'system$workflowid', 'system$workflowactivityid', 'idx_system$workflow_currentactivity_system$workflowactivity_system$workflow', 2);
GO
CREATE TABLE [system$user_timezone] (
	[system$userid] bigint NOT NULL,
	[system$timezoneid] bigint NOT NULL,
	PRIMARY KEY([system$userid],[system$timezoneid]),
	CONSTRAINT [uniq_system$user_timezone_system$userid] UNIQUE ([system$userid]));
GO
CREATE INDEX [idx_system$user_timezone_system$timezone_system$user] ON [system$user_timezone] ([system$timezoneid] ASC,[system$userid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('bab4a1ab-7d40-47d5-8f21-fc99d089211d', 'System.User_TimeZone', 'system$user_timezone', '282e2e60-88a5-469d-84a5-ba8d9151644f', '7f7c72af-1ab7-4bf9-bed6-16db5c8fcf6f', 'system$userid', 'system$timezoneid', 'idx_system$user_timezone_system$timezone_system$user', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$user_timezone_system$userid', 'bab4a1ab-7d40-47d5-8f21-fc99d089211d', '61482ff9-64e6-366d-9055-524387b93b37');
GO
CREATE TABLE [system$userroles] (
	[system$userid] bigint NOT NULL,
	[system$userroleid] bigint NOT NULL,
	PRIMARY KEY([system$userid],[system$userroleid]));
GO
CREATE INDEX [idx_system$userroles_system$userrole_system$user] ON [system$userroles] ([system$userroleid] ASC,[system$userid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('6adaf137-4299-435e-9475-a871a4f21471', 'System.UserRoles', 'system$userroles', '282e2e60-88a5-469d-84a5-ba8d9151644f', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'system$userid', 'system$userroleid', 'idx_system$userroles_system$userrole_system$user', 2);
GO
CREATE TABLE [system$user_language] (
	[system$userid] bigint NOT NULL,
	[system$languageid] bigint NOT NULL,
	PRIMARY KEY([system$userid],[system$languageid]),
	CONSTRAINT [uniq_system$user_language_system$userid] UNIQUE ([system$userid]));
GO
CREATE INDEX [idx_system$user_language_system$language_system$user] ON [system$user_language] ([system$languageid] ASC,[system$userid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('00640985-3c73-4b15-9705-d4ec3ff58e6b', 'System.User_Language', 'system$user_language', '282e2e60-88a5-469d-84a5-ba8d9151644f', '76805df3-dede-435f-92a6-d6525c68a693', 'system$userid', 'system$languageid', 'idx_system$user_language_system$language_system$user', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$user_language_system$userid', '00640985-3c73-4b15-9705-d4ec3ff58e6b', '37d87db4-942f-301e-b1d7-ca1c940655fa');
GO
CREATE TABLE [saml20$sprequestedattribute_spattributeconsumingservice] (
	[saml20$sprequestedattributeid] bigint NOT NULL,
	[saml20$spattributeconsumingserviceid] bigint NOT NULL,
	PRIMARY KEY([saml20$sprequestedattributeid],[saml20$spattributeconsumingserviceid]),
	CONSTRAINT [uniq_saml20$sprequestedattribute_spattributeconsumingservice_saml20$sprequestedattributeid] UNIQUE ([saml20$sprequestedattributeid]));
GO
CREATE INDEX [idx_saml20$sprequestedattribute_spattributeconsumingservice_saml20$spattributeconsumingservice_saml20$sprequestedattribute] ON [saml20$sprequestedattribute_spattributeconsumingservice] ([saml20$spattributeconsumingserviceid] ASC,[saml20$sprequestedattributeid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('8828c072-f662-423b-8a80-c017018f2567', 'SAML20.SPRequestedAttribute_SPAttributeConsumingService', 'saml20$sprequestedattribute_spattributeconsumingservice', '93fbe9f3-e101-48f8-bd2f-13c7c04429b9', '0f381b16-223a-4086-b6a0-18f5a76553af', 'saml20$sprequestedattributeid', 'saml20$spattributeconsumingserviceid', 'idx_saml20$sprequestedattribute_spattributeconsumingservice_saml20$spattributeconsumingservice_saml20$sprequestedattribute', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$sprequestedattribute_spattributeconsumingservice_saml20$sprequestedattributeid', '8828c072-f662-423b-8a80-c017018f2567', '4886414d-ca80-3b28-a0d9-c0d35be846b8');
GO
CREATE TABLE [system$workflowusertask_workflowusertaskdefinition] (
	[system$workflowusertaskid] bigint NOT NULL,
	[system$workflowusertaskdefinitionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskid],[system$workflowusertaskdefinitionid]),
	CONSTRAINT [uniq_system$workflowusertask_workflowusertaskdefinition_system$workflowusertaskid] UNIQUE ([system$workflowusertaskid]));
GO
CREATE INDEX [idx_system$workflowusertask_workflowusertaskdefinition_system$workflowusertaskdefinition_system$workflowusertask] ON [system$workflowusertask_workflowusertaskdefinition] ([system$workflowusertaskdefinitionid] ASC,[system$workflowusertaskid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('0169cc0e-491b-4ee3-812d-6bf3ba28e287', 'System.WorkflowUserTask_WorkflowUserTaskDefinition', 'system$workflowusertask_workflowusertaskdefinition', '3729d27c-735b-457a-b210-9dffb125c3f3', 'e09e866f-288b-475c-9465-792cde8b878c', 'system$workflowusertaskid', 'system$workflowusertaskdefinitionid', 'idx_system$workflowusertask_workflowusertaskdefinition_system$workflowusertaskdefinition_system$workflowusertask', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowusertask_workflowusertaskdefinition_system$workflowusertaskid', '0169cc0e-491b-4ee3-812d-6bf3ba28e287', '0643c851-59f7-3428-92a1-a970ad21ad18');
GO
CREATE TABLE [system$workflowusertask_assignees] (
	[system$workflowusertaskid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskid],[system$userid]));
GO
CREATE INDEX [idx_system$workflowusertask_assignees_system$user_system$workflowusertask] ON [system$workflowusertask_assignees] ([system$userid] ASC,[system$workflowusertaskid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('929fbbed-d3a8-4ea2-b6ad-b28de4f77776', 'System.WorkflowUserTask_Assignees', 'system$workflowusertask_assignees', '3729d27c-735b-457a-b210-9dffb125c3f3', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$workflowusertaskid', 'system$userid', 'idx_system$workflowusertask_assignees_system$user_system$workflowusertask', 2);
GO
CREATE TABLE [system$workflowusertask_workflow] (
	[system$workflowusertaskid] bigint NOT NULL,
	[system$workflowid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskid],[system$workflowid]),
	CONSTRAINT [uniq_system$workflowusertask_workflow_system$workflowusertaskid] UNIQUE ([system$workflowusertaskid]));
GO
CREATE INDEX [idx_system$workflowusertask_workflow_system$workflow_system$workflowusertask] ON [system$workflowusertask_workflow] ([system$workflowid] ASC,[system$workflowusertaskid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('53a1c6d7-5e4d-4a2d-81ec-58fde4bbba8a', 'System.WorkflowUserTask_Workflow', 'system$workflowusertask_workflow', '3729d27c-735b-457a-b210-9dffb125c3f3', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'system$workflowusertaskid', 'system$workflowid', 'idx_system$workflowusertask_workflow_system$workflow_system$workflowusertask', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowusertask_workflow_system$workflowusertaskid', '53a1c6d7-5e4d-4a2d-81ec-58fde4bbba8a', 'bb1eaa2c-e600-3a88-85d0-08d5f5ca94da');
GO
CREATE TABLE [system$workflowusertask_targetusers] (
	[system$workflowusertaskid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$workflowusertaskid],[system$userid]));
GO
CREATE INDEX [idx_system$workflowusertask_targetusers_system$user_system$workflowusertask] ON [system$workflowusertask_targetusers] ([system$userid] ASC,[system$workflowusertaskid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2b9c1990-302f-474c-9341-9d5d23b27653', 'System.WorkflowUserTask_TargetUsers', 'system$workflowusertask_targetusers', '3729d27c-735b-457a-b210-9dffb125c3f3', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$workflowusertaskid', 'system$userid', 'idx_system$workflowusertask_targetusers_system$user_system$workflowusertask', 2);
GO
CREATE TABLE [administration$accountextension_account] (
	[administration$accountextensionid] bigint NOT NULL,
	[administration$accountid] bigint NOT NULL,
	PRIMARY KEY([administration$accountextensionid],[administration$accountid]),
	CONSTRAINT [uniq_administration$accountextension_account_administration$accountid] UNIQUE ([administration$accountid]),
	CONSTRAINT [uniq_administration$accountextension_account_administration$accountextensionid] UNIQUE ([administration$accountextensionid]));
GO
CREATE INDEX [idx_administration$accountextension_account_administration$account_administration$accountextension] ON [administration$accountextension_account] ([administration$accountid] ASC,[administration$accountextensionid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('958eda36-ef52-40a9-b4de-2e238ded5a97', 'Administration.AccountExtension_Account', 'administration$accountextension_account', 'f3d35cb5-9618-4e98-8ebb-d2e047ab404f', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'administration$accountextensionid', 'administration$accountid', 'idx_administration$accountextension_account_administration$account_administration$accountextension', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$accountextension_account_administration$accountid', '958eda36-ef52-40a9-b4de-2e238ded5a97', '21e36f4f-b1ba-3392-8a07-4edd9f5d26df');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$accountextension_account_administration$accountextensionid', '958eda36-ef52-40a9-b4de-2e238ded5a97', 'b7ff10fe-f974-37ae-a467-89e34635f4c7');
GO
CREATE TABLE [system$workflowactivity_previousactivity] (
	[system$workflowactivityid1] bigint NOT NULL,
	[system$workflowactivityid2] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid1],[system$workflowactivityid2]));
GO
CREATE INDEX [idx_system$workflowactivity_previousactivity_system$workflowactivity_system$workflowactivity] ON [system$workflowactivity_previousactivity] ([system$workflowactivityid2] ASC,[system$workflowactivityid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('8d8c8ffc-08d6-4dc5-88f6-5b344763d948', 'System.WorkflowActivity_PreviousActivity', 'system$workflowactivity_previousactivity', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'a5952592-bb2c-4798-9805-f9ff91ad97de', 'system$workflowactivityid1', 'system$workflowactivityid2', 'idx_system$workflowactivity_previousactivity_system$workflowactivity_system$workflowactivity', 2);
GO
CREATE TABLE [system$workflowactivity_actor] (
	[system$workflowactivityid] bigint NOT NULL,
	[system$userid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid],[system$userid]),
	CONSTRAINT [uniq_system$workflowactivity_actor_system$workflowactivityid] UNIQUE ([system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflowactivity_actor_system$user_system$workflowactivity] ON [system$workflowactivity_actor] ([system$userid] ASC,[system$workflowactivityid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('63446029-b863-4c07-ab91-22219be89b70', 'System.WorkflowActivity_Actor', 'system$workflowactivity_actor', 'a5952592-bb2c-4798-9805-f9ff91ad97de', '282e2e60-88a5-469d-84a5-ba8d9151644f', 'system$workflowactivityid', 'system$userid', 'idx_system$workflowactivity_actor_system$user_system$workflowactivity', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivity_actor_system$workflowactivityid', '63446029-b863-4c07-ab91-22219be89b70', '38f7a539-d2df-398f-99b4-047c6f7ff859');
GO
CREATE TABLE [system$workflowactivity_workflow] (
	[system$workflowactivityid] bigint NOT NULL,
	[system$workflowid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid],[system$workflowid]),
	CONSTRAINT [uniq_system$workflowactivity_workflow_system$workflowactivityid] UNIQUE ([system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflowactivity_workflow_system$workflow_system$workflowactivity] ON [system$workflowactivity_workflow] ([system$workflowid] ASC,[system$workflowactivityid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('ef863cc9-2d20-4a74-af65-0320a76b6a10', 'System.WorkflowActivity_Workflow', 'system$workflowactivity_workflow', 'a5952592-bb2c-4798-9805-f9ff91ad97de', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'system$workflowactivityid', 'system$workflowid', 'idx_system$workflowactivity_workflow_system$workflow_system$workflowactivity', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivity_workflow_system$workflowactivityid', 'ef863cc9-2d20-4a74-af65-0320a76b6a10', '56628087-ac77-34ca-bd95-b86020fc4ffa');
GO
CREATE TABLE [system$workflowactivity_workflowversion] (
	[system$workflowactivityid] bigint NOT NULL,
	[system$workflowversionid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid],[system$workflowversionid]),
	CONSTRAINT [uniq_system$workflowactivity_workflowversion_system$workflowactivityid] UNIQUE ([system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflowactivity_workflowversion_system$workflowversion_system$workflowactivity] ON [system$workflowactivity_workflowversion] ([system$workflowversionid] ASC,[system$workflowactivityid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2e5166f9-7430-4265-8465-f7405d6fe1e9', 'System.WorkflowActivity_WorkflowVersion', 'system$workflowactivity_workflowversion', 'a5952592-bb2c-4798-9805-f9ff91ad97de', '30834a21-e81c-4cbf-a10b-5f60f5fddc82', 'system$workflowactivityid', 'system$workflowversionid', 'idx_system$workflowactivity_workflowversion_system$workflowversion_system$workflowactivity', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivity_workflowversion_system$workflowactivityid', '2e5166f9-7430-4265-8465-f7405d6fe1e9', 'f25c7cf4-22a7-30e7-a3b6-1cda08ccc618');
GO
CREATE TABLE [system$workflowactivity_workflowusertask] (
	[system$workflowactivityid] bigint NOT NULL,
	[system$workflowusertaskid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid],[system$workflowusertaskid]),
	CONSTRAINT [uniq_system$workflowactivity_workflowusertask_system$workflowactivityid] UNIQUE ([system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflowactivity_workflowusertask_system$workflowusertask_system$workflowactivity] ON [system$workflowactivity_workflowusertask] ([system$workflowusertaskid] ASC,[system$workflowactivityid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('aaebf783-447c-4386-ba25-969132aa6f7c', 'System.WorkflowActivity_WorkflowUserTask', 'system$workflowactivity_workflowusertask', 'a5952592-bb2c-4798-9805-f9ff91ad97de', '3729d27c-735b-457a-b210-9dffb125c3f3', 'system$workflowactivityid', 'system$workflowusertaskid', 'idx_system$workflowactivity_workflowusertask_system$workflowusertask_system$workflowactivity', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivity_workflowusertask_system$workflowactivityid', 'aaebf783-447c-4386-ba25-969132aa6f7c', '096b955d-f88f-303f-b43e-deef60b57065');
GO
CREATE TABLE [system$workflowactivity_subworkflow] (
	[system$workflowactivityid] bigint NOT NULL,
	[system$workflowid] bigint NOT NULL,
	PRIMARY KEY([system$workflowactivityid],[system$workflowid]),
	CONSTRAINT [uniq_system$workflowactivity_subworkflow_system$workflowactivityid] UNIQUE ([system$workflowactivityid]));
GO
CREATE INDEX [idx_system$workflowactivity_subworkflow_system$workflow_system$workflowactivity] ON [system$workflowactivity_subworkflow] ([system$workflowid] ASC,[system$workflowactivityid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('5c89ec5d-0378-4afd-afd4-0e7a9fde45a9', 'System.WorkflowActivity_SubWorkflow', 'system$workflowactivity_subworkflow', 'a5952592-bb2c-4798-9805-f9ff91ad97de', '2ae37bf5-ecb8-4c55-b967-d7383925b208', 'system$workflowactivityid', 'system$workflowid', 'idx_system$workflowactivity_subworkflow_system$workflow_system$workflowactivity', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$workflowactivity_subworkflow_system$workflowactivityid', '5c89ec5d-0378-4afd-afd4-0e7a9fde45a9', '2d65cc25-b704-3604-8f2b-e3c73d37fb2b');
GO
CREATE TABLE [mxmodelreflection$microflows_output_type] (
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	[mxmodelreflection$valuetypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$microflowsid],[mxmodelreflection$valuetypeid]),
	CONSTRAINT [uniq_mxmodelreflection$microflows_output_type_mxmodelreflection$microflowsid] UNIQUE ([mxmodelreflection$microflowsid]));
GO
CREATE INDEX [idx_mxmodelreflection$microflows_output_type_mxmodelreflection$valuetype_mxmodelreflection$microflows] ON [mxmodelreflection$microflows_output_type] ([mxmodelreflection$valuetypeid] ASC,[mxmodelreflection$microflowsid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d226669a-242c-4975-9a4d-f30a4082207e', 'MxModelReflection.Microflows_Output_Type', 'mxmodelreflection$microflows_output_type', '17e6df01-1431-4547-b824-d471e84f719c', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'mxmodelreflection$microflowsid', 'mxmodelreflection$valuetypeid', 'idx_mxmodelreflection$microflows_output_type_mxmodelreflection$valuetype_mxmodelreflection$microflows', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$microflows_output_type_mxmodelreflection$microflowsid', 'd226669a-242c-4975-9a4d-f30a4082207e', '7b9982a2-725e-3ae8-9add-44aa87d29ae1');
GO
CREATE TABLE [mxmodelreflection$microflows_inputparameter] (
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	[mxmodelreflection$parameterid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$microflowsid],[mxmodelreflection$parameterid]));
GO
CREATE INDEX [idx_mxmodelreflection$microflows_inputparameter_mxmodelreflection$parameter_mxmodelreflection$microflows] ON [mxmodelreflection$microflows_inputparameter] ([mxmodelreflection$parameterid] ASC,[mxmodelreflection$microflowsid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('32ebec48-898b-46f5-8b89-0381bdadad7b', 'MxModelReflection.Microflows_InputParameter', 'mxmodelreflection$microflows_inputparameter', '17e6df01-1431-4547-b824-d471e84f719c', '51b74cdc-8f51-44c5-b859-6d0b4b2f40b0', 'mxmodelreflection$microflowsid', 'mxmodelreflection$parameterid', 'idx_mxmodelreflection$microflows_inputparameter_mxmodelreflection$parameter_mxmodelreflection$microflows', 2);
GO
CREATE TABLE [mxmodelreflection$microflows_module] (
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	[mxmodelreflection$moduleid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$microflowsid],[mxmodelreflection$moduleid]),
	CONSTRAINT [uniq_mxmodelreflection$microflows_module_mxmodelreflection$microflowsid] UNIQUE ([mxmodelreflection$microflowsid]));
GO
CREATE INDEX [idx_mxmodelreflection$microflows_module_mxmodelreflection$module_mxmodelreflection$microflows] ON [mxmodelreflection$microflows_module] ([mxmodelreflection$moduleid] ASC,[mxmodelreflection$microflowsid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('73d20a6b-8573-4d27-b580-9c9da5951d47', 'MxModelReflection.Microflows_Module', 'mxmodelreflection$microflows_module', '17e6df01-1431-4547-b824-d471e84f719c', '2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'mxmodelreflection$microflowsid', 'mxmodelreflection$moduleid', 'idx_mxmodelreflection$microflows_module_mxmodelreflection$module_mxmodelreflection$microflows', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$microflows_module_mxmodelreflection$microflowsid', '73d20a6b-8573-4d27-b580-9c9da5951d47', '1f541365-a12d-373b-8201-6dc251e62e2a');
GO
CREATE TABLE [saml20$samlresponse_ssoconfiguration] (
	[saml20$samlresponseid] bigint NOT NULL,
	[saml20$ssoconfigurationid] bigint NOT NULL,
	PRIMARY KEY([saml20$samlresponseid],[saml20$ssoconfigurationid]),
	CONSTRAINT [uniq_saml20$samlresponse_ssoconfiguration_saml20$samlresponseid] UNIQUE ([saml20$samlresponseid]));
GO
CREATE INDEX [idx_saml20$samlresponse_ssoconfiguration_saml20$ssoconfiguration_saml20$samlresponse] ON [saml20$samlresponse_ssoconfiguration] ([saml20$ssoconfigurationid] ASC,[saml20$samlresponseid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('9da09ed9-fec9-4203-94a8-a4cdfc0b56e5', 'SAML20.SAMLResponse_SSOConfiguration', 'saml20$samlresponse_ssoconfiguration', 'b91df9be-c86f-4037-be6b-355ad22a04ff', '4a97f5e4-a2da-4d20-a500-8f2ec8482696', 'saml20$samlresponseid', 'saml20$ssoconfigurationid', 'idx_saml20$samlresponse_ssoconfiguration_saml20$ssoconfiguration_saml20$samlresponse', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$samlresponse_ssoconfiguration_saml20$samlresponseid', '9da09ed9-fec9-4203-94a8-a4cdfc0b56e5', 'f98b3583-512a-3168-b157-7cb176ecd24d');
GO
CREATE TABLE [administration$accountprofilepicture_accountextension] (
	[administration$accountprofilepictureid] bigint NOT NULL,
	[administration$accountextensionid] bigint NOT NULL,
	PRIMARY KEY([administration$accountprofilepictureid],[administration$accountextensionid]),
	CONSTRAINT [uniq_administration$accountprofilepicture_accountextension_administration$accountextensionid] UNIQUE ([administration$accountextensionid]),
	CONSTRAINT [uniq_administration$accountprofilepicture_accountextension_administration$accountprofilepictureid] UNIQUE ([administration$accountprofilepictureid]));
GO
CREATE INDEX [idx_administration$accountprofilepicture_accountextension_administration$accountextension_administration$accountprofilepicture] ON [administration$accountprofilepicture_accountextension] ([administration$accountextensionid] ASC,[administration$accountprofilepictureid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('e8e5dafb-ed84-469a-9e79-f8181ff34a04', 'Administration.AccountProfilePicture_AccountExtension', 'administration$accountprofilepicture_accountextension', '1e9692fd-cd51-4a4b-b09a-4764c899e10d', 'f3d35cb5-9618-4e98-8ebb-d2e047ab404f', 'administration$accountprofilepictureid', 'administration$accountextensionid', 'idx_administration$accountprofilepicture_accountextension_administration$accountextension_administration$accountprofilepicture', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$accountprofilepicture_accountextension_administration$accountextensionid', 'e8e5dafb-ed84-469a-9e79-f8181ff34a04', '8279aedf-8091-385e-940d-3d31a6e79e98');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_administration$accountprofilepicture_accountextension_administration$accountprofilepictureid', 'e8e5dafb-ed84-469a-9e79-f8181ff34a04', '16cf2cb8-3329-3ba1-bf38-847f489f1b64');
GO
CREATE TABLE [mxmodelreflection$mxobjectmember_type] (
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	[mxmodelreflection$valuetypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectmemberid],[mxmodelreflection$valuetypeid]),
	CONSTRAINT [uniq_mxmodelreflection$mxobjectmember_type_mxmodelreflection$mxobjectmemberid] UNIQUE ([mxmodelreflection$mxobjectmemberid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectmember_type_mxmodelreflection$valuetype_mxmodelreflection$mxobjectmember] ON [mxmodelreflection$mxobjectmember_type] ([mxmodelreflection$valuetypeid] ASC,[mxmodelreflection$mxobjectmemberid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('dcaff6a6-1d1d-45b6-8e9f-abce1d8e617f', 'MxModelReflection.MxObjectMember_Type', 'mxmodelreflection$mxobjectmember_type', '398a1f70-2b2c-408e-8778-f4a923fda765', '2288ba1c-0d4a-4ea1-b9ea-add7f28c0b37', 'mxmodelreflection$mxobjectmemberid', 'mxmodelreflection$valuetypeid', 'idx_mxmodelreflection$mxobjectmember_type_mxmodelreflection$valuetype_mxmodelreflection$mxobjectmember', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$mxobjectmember_type_mxmodelreflection$mxobjectmemberid', 'dcaff6a6-1d1d-45b6-8e9f-abce1d8e617f', '79e37c84-aefc-3f3c-88b2-a525913b8e3b');
GO
CREATE TABLE [mxmodelreflection$mxobjectmember_mxobjecttype] (
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectmemberid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_mxmodelreflection$mxobjectmember_mxobjecttype_mxmodelreflection$mxobjectmemberid] UNIQUE ([mxmodelreflection$mxobjectmemberid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectmember_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectmember] ON [mxmodelreflection$mxobjectmember_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$mxobjectmemberid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d5245022-311c-40a5-a119-9d429b2829f9', 'MxModelReflection.MxObjectMember_MxObjectType', 'mxmodelreflection$mxobjectmember_mxobjecttype', '398a1f70-2b2c-408e-8778-f4a923fda765', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$mxobjectmemberid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$mxobjectmember_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectmember', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$mxobjectmember_mxobjecttype_mxmodelreflection$mxobjectmemberid', 'd5245022-311c-40a5-a119-9d429b2829f9', '4c7b177a-992b-3d6d-a5c5-e8a0d279c5cf');
GO
CREATE TABLE [saml20$organization_entitydescriptor] (
	[saml20$organizationid] bigint NOT NULL,
	[saml20$entitydescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$organizationid],[saml20$entitydescriptorid]),
	CONSTRAINT [uniq_saml20$organization_entitydescriptor_saml20$organizationid] UNIQUE ([saml20$organizationid]));
GO
CREATE INDEX [idx_saml20$organization_entitydescriptor_saml20$entitydescriptor_saml20$organization] ON [saml20$organization_entitydescriptor] ([saml20$entitydescriptorid] ASC,[saml20$organizationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('749c6846-4921-4c0d-abee-d41fd312445b', 'SAML20.Organization_EntityDescriptor', 'saml20$organization_entitydescriptor', 'ad6340d4-b92a-4a0c-9239-9ce623c0c372', '50527e51-152a-40ca-98fa-75a0b24193bb', 'saml20$organizationid', 'saml20$entitydescriptorid', 'idx_saml20$organization_entitydescriptor_saml20$entitydescriptor_saml20$organization', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$organization_entitydescriptor_saml20$organizationid', '749c6846-4921-4c0d-abee-d41fd312445b', '9655aa5a-9066-3ca6-b0fa-57c08d9ea7b4');
GO
CREATE TABLE [system$grantableroles] (
	[system$userroleid1] bigint NOT NULL,
	[system$userroleid2] bigint NOT NULL,
	PRIMARY KEY([system$userroleid1],[system$userroleid2]));
GO
CREATE INDEX [idx_system$grantableroles_system$userrole_system$userrole] ON [system$grantableroles] ([system$userroleid2] ASC,[system$userroleid1] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('1adca745-c7a9-44ff-92bb-5d41cb2a1743', 'System.grantableRoles', 'system$grantableroles', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'system$userroleid1', 'system$userroleid2', 'idx_system$grantableroles_system$userrole_system$userrole', 2);
GO
CREATE TABLE [excr_inlinespc$tempspctxndata_tempspcsetup] (
	[excr_inlinespc$tempspctxndataid] bigint NOT NULL,
	[excr_inlinespc$tempspcsetupid] bigint NOT NULL,
	PRIMARY KEY([excr_inlinespc$tempspctxndataid],[excr_inlinespc$tempspcsetupid]),
	CONSTRAINT [uniq_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspcsetupid] UNIQUE ([excr_inlinespc$tempspcsetupid]),
	CONSTRAINT [uniq_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspctxndataid] UNIQUE ([excr_inlinespc$tempspctxndataid]));
GO
CREATE INDEX [idx_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspcsetup_excr_inlinespc$tempspctxndata] ON [excr_inlinespc$tempspctxndata_tempspcsetup] ([excr_inlinespc$tempspcsetupid] ASC,[excr_inlinespc$tempspctxndataid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('0ff4bd70-e5d9-426f-8436-d87dcd9ae4b5', 'EXCR_InlineSPC.TempSPCTxnData_TempSPCSetup', 'excr_inlinespc$tempspctxndata_tempspcsetup', '0c297ddd-636c-4cab-ab5c-1db3f17d9028', '8881af53-24fc-46ff-96ce-1da98c93b400', 'excr_inlinespc$tempspctxndataid', 'excr_inlinespc$tempspcsetupid', 'idx_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspcsetup_excr_inlinespc$tempspctxndata', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspcsetupid', '0ff4bd70-e5d9-426f-8436-d87dcd9ae4b5', '7350b696-8eeb-310c-b578-8aac414b77cf');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_inlinespc$tempspctxndata_tempspcsetup_excr_inlinespc$tempspctxndataid', '0ff4bd70-e5d9-426f-8436-d87dcd9ae4b5', 'cc927a8b-9b6b-37a6-9302-8e06aae4c6c6');
GO
CREATE TABLE [saml20$nameidformat_roledescriptor] (
	[saml20$nameidformatid] bigint NOT NULL,
	[saml20$roledescriptorid] bigint NOT NULL,
	PRIMARY KEY([saml20$nameidformatid],[saml20$roledescriptorid]),
	CONSTRAINT [uniq_saml20$nameidformat_roledescriptor_saml20$nameidformatid] UNIQUE ([saml20$nameidformatid]));
GO
CREATE INDEX [idx_saml20$nameidformat_roledescriptor_saml20$roledescriptor_saml20$nameidformat] ON [saml20$nameidformat_roledescriptor] ([saml20$roledescriptorid] ASC,[saml20$nameidformatid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('2d765322-0f7e-4de5-8b1a-9d5e85668ea4', 'SAML20.NameIDFormat_RoleDescriptor', 'saml20$nameidformat_roledescriptor', 'ca71a18f-d692-43d9-80cb-ee92e8fa4c5e', '8c7c691b-0561-4729-b9da-ad76c4403f4f', 'saml20$nameidformatid', 'saml20$roledescriptorid', 'idx_saml20$nameidformat_roledescriptor_saml20$roledescriptor_saml20$nameidformat', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_saml20$nameidformat_roledescriptor_saml20$nameidformatid', '2d765322-0f7e-4de5-8b1a-9d5e85668ea4', '6529d19e-d79c-362d-b980-b9a5a6cc5f59');
GO
CREATE TABLE [usercommons$claim_userprovisioning] (
	[usercommons$claimid] bigint NOT NULL,
	[usercommons$userprovisioningid] bigint NOT NULL,
	PRIMARY KEY([usercommons$claimid],[usercommons$userprovisioningid]),
	CONSTRAINT [uniq_usercommons$claim_userprovisioning_usercommons$claimid] UNIQUE ([usercommons$claimid]));
GO
CREATE INDEX [idx_usercommons$claim_userprovisioning_usercommons$userprovisioning_usercommons$claim] ON [usercommons$claim_userprovisioning] ([usercommons$userprovisioningid] ASC,[usercommons$claimid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('a2270134-f3d5-4f7e-aa50-a23e96cf1b9d', 'UserCommons.Claim_UserProvisioning', 'usercommons$claim_userprovisioning', 'ea6ca13e-3ef5-44aa-b22a-60f69fb100bd', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'usercommons$claimid', 'usercommons$userprovisioningid', 'idx_usercommons$claim_userprovisioning_usercommons$userprovisioning_usercommons$claim', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$claim_userprovisioning_usercommons$claimid', 'a2270134-f3d5-4f7e-aa50-a23e96cf1b9d', 'a2138b1f-7fa3-3805-8ff8-288ade5faaa6');
GO
CREATE TABLE [exsm_futureholdsetup$futureholduserconfig_account] (
	[exsm_futureholdsetup$futureholduserconfigid] bigint NOT NULL,
	[administration$accountid] bigint NOT NULL,
	PRIMARY KEY([exsm_futureholdsetup$futureholduserconfigid],[administration$accountid]),
	CONSTRAINT [uniq_exsm_futureholdsetup$futureholduserconfig_account_administration$accountid] UNIQUE ([administration$accountid]),
	CONSTRAINT [uniq_exsm_futureholdsetup$futureholduserconfig_account_exsm_futureholdsetup$futureholduserconfigid] UNIQUE ([exsm_futureholdsetup$futureholduserconfigid]));
GO
CREATE INDEX [idx_exsm_futureholdsetup$futureholduserconfig_account_administration$account_exsm_futureholdsetup$futureholduserconfig] ON [exsm_futureholdsetup$futureholduserconfig_account] ([administration$accountid] ASC,[exsm_futureholdsetup$futureholduserconfigid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('80711df4-471b-4050-b352-adaaf6d44de3', 'EXSM_FutureHoldSetup.FutureHoldUserConfig_Account', 'exsm_futureholdsetup$futureholduserconfig_account', 'baba0a15-413d-40c7-867b-e36b80208457', 'c921ccbb-a670-48d9-833d-6a76c1406917', 'exsm_futureholdsetup$futureholduserconfigid', 'administration$accountid', 'idx_exsm_futureholdsetup$futureholduserconfig_account_administration$account_exsm_futureholdsetup$futureholduserconfig', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_futureholdsetup$futureholduserconfig_account_administration$accountid', '80711df4-471b-4050-b352-adaaf6d44de3', 'f6f2de27-fa94-382f-91d2-4b4166b9d9e9');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_exsm_futureholdsetup$futureholduserconfig_account_exsm_futureholdsetup$futureholduserconfigid', '80711df4-471b-4050-b352-adaaf6d44de3', 'a96508a0-88ee-31ea-a36e-28996d5a291e');
GO
CREATE TABLE [excr_commons$configuration_session] (
	[excr_commons$configurationid] bigint NOT NULL,
	[system$sessionid] bigint NOT NULL,
	PRIMARY KEY([excr_commons$configurationid],[system$sessionid]),
	CONSTRAINT [uniq_excr_commons$configuration_session_excr_commons$configurationid] UNIQUE ([excr_commons$configurationid]));
GO
CREATE INDEX [idx_excr_commons$configuration_session_system$session_excr_commons$configuration] ON [excr_commons$configuration_session] ([system$sessionid] ASC,[excr_commons$configurationid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('dd8dc757-79a3-4d89-b381-fadbae0fdb7a', 'EXCR_Commons.Configuration_Session', 'excr_commons$configuration_session', '0593fd24-73a3-48c4-b29e-c9b1009278bd', '37f9fd49-5318-4c63-9a51-f761779b202f', 'excr_commons$configurationid', 'system$sessionid', 'idx_excr_commons$configuration_session_system$session_excr_commons$configuration', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_commons$configuration_session_excr_commons$configurationid', 'dd8dc757-79a3-4d89-b381-fadbae0fdb7a', 'f3d3c097-5b2f-35be-baab-9745b1dd3e6b');
GO
CREATE TABLE [mxmodelreflection$mxobjectreference_mxobjecttype_parent] (
	[mxmodelreflection$mxobjectreferenceid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectreferenceid],[mxmodelreflection$mxobjecttypeid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_mxobjecttype_parent_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference] ON [mxmodelreflection$mxobjectreference_mxobjecttype_parent] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$mxobjectreferenceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('58ea084e-9d4d-4b53-bcab-4be6378d1e8d', 'MxModelReflection.MxObjectReference_MxObjectType_Parent', 'mxmodelreflection$mxobjectreference_mxobjecttype_parent', '057a1e77-1c44-46fa-9eef-6809c800641f', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$mxobjectreferenceid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$mxobjectreference_mxobjecttype_parent_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference', 2);
GO
CREATE TABLE [mxmodelreflection$mxobjectreference_mxobjecttype] (
	[mxmodelreflection$mxobjectreferenceid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectreferenceid],[mxmodelreflection$mxobjecttypeid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference] ON [mxmodelreflection$mxobjectreference_mxobjecttype] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$mxobjectreferenceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('1a017923-2c29-4278-b39e-fae791fd01cf', 'MxModelReflection.MxObjectReference_MxObjectType', 'mxmodelreflection$mxobjectreference_mxobjecttype', '057a1e77-1c44-46fa-9eef-6809c800641f', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$mxobjectreferenceid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$mxobjectreference_mxobjecttype_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference', 2);
GO
CREATE TABLE [mxmodelreflection$mxobjectreference_module] (
	[mxmodelreflection$mxobjectreferenceid] bigint NOT NULL,
	[mxmodelreflection$moduleid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectreferenceid],[mxmodelreflection$moduleid]),
	CONSTRAINT [uniq_mxmodelreflection$mxobjectreference_module_mxmodelreflection$mxobjectreferenceid] UNIQUE ([mxmodelreflection$mxobjectreferenceid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_module_mxmodelreflection$module_mxmodelreflection$mxobjectreference] ON [mxmodelreflection$mxobjectreference_module] ([mxmodelreflection$moduleid] ASC,[mxmodelreflection$mxobjectreferenceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('d3572116-ffa0-4bef-8b18-bf839c8a6cbd', 'MxModelReflection.MxObjectReference_Module', 'mxmodelreflection$mxobjectreference_module', '057a1e77-1c44-46fa-9eef-6809c800641f', '2d8551fa-3c00-4a14-b0b5-ed8e384ed58b', 'mxmodelreflection$mxobjectreferenceid', 'mxmodelreflection$moduleid', 'idx_mxmodelreflection$mxobjectreference_module_mxmodelreflection$module_mxmodelreflection$mxobjectreference', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_mxmodelreflection$mxobjectreference_module_mxmodelreflection$mxobjectreferenceid', 'd3572116-ffa0-4bef-8b18-bf839c8a6cbd', '3e63306a-7df6-383f-810a-7155becd29e8');
GO
CREATE TABLE [mxmodelreflection$mxobjectreference_mxobjecttype_child] (
	[mxmodelreflection$mxobjectreferenceid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([mxmodelreflection$mxobjectreferenceid],[mxmodelreflection$mxobjecttypeid]));
GO
CREATE INDEX [idx_mxmodelreflection$mxobjectreference_mxobjecttype_child_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference] ON [mxmodelreflection$mxobjectreference_mxobjecttype_child] ([mxmodelreflection$mxobjecttypeid] ASC,[mxmodelreflection$mxobjectreferenceid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('99e94fc5-b387-45b1-918c-3f57f40c8c24', 'MxModelReflection.MxObjectReference_MxObjectType_Child', 'mxmodelreflection$mxobjectreference_mxobjecttype_child', '057a1e77-1c44-46fa-9eef-6809c800641f', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'mxmodelreflection$mxobjectreferenceid', 'mxmodelreflection$mxobjecttypeid', 'idx_mxmodelreflection$mxobjectreference_mxobjecttype_child_mxmodelreflection$mxobjecttype_mxmodelreflection$mxobjectreference', 2);
GO
CREATE TABLE [excr_commons$sessionvalues_configuration] (
	[excr_commons$sessionvaluesid] bigint NOT NULL,
	[excr_commons$configurationid] bigint NOT NULL,
	PRIMARY KEY([excr_commons$sessionvaluesid],[excr_commons$configurationid]),
	CONSTRAINT [uniq_excr_commons$sessionvalues_configuration_excr_commons$configurationid] UNIQUE ([excr_commons$configurationid]),
	CONSTRAINT [uniq_excr_commons$sessionvalues_configuration_excr_commons$sessionvaluesid] UNIQUE ([excr_commons$sessionvaluesid]));
GO
CREATE INDEX [idx_excr_commons$sessionvalues_configuration_excr_commons$configuration_excr_commons$sessionvalues] ON [excr_commons$sessionvalues_configuration] ([excr_commons$configurationid] ASC,[excr_commons$sessionvaluesid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('e2d2794e-adcb-4caa-999d-5aeeb4d5eb2a', 'EXCR_Commons.SessionValues_Configuration', 'excr_commons$sessionvalues_configuration', '5ff1a85a-2331-493b-a227-3bfce73f148f', '0593fd24-73a3-48c4-b29e-c9b1009278bd', 'excr_commons$sessionvaluesid', 'excr_commons$configurationid', 'idx_excr_commons$sessionvalues_configuration_excr_commons$configuration_excr_commons$sessionvalues', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_commons$sessionvalues_configuration_excr_commons$configurationid', 'e2d2794e-adcb-4caa-999d-5aeeb4d5eb2a', '58653102-e923-3374-af58-6220ffe92c93');
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_excr_commons$sessionvalues_configuration_excr_commons$sessionvaluesid', 'e2d2794e-adcb-4caa-999d-5aeeb4d5eb2a', '441bf779-5751-3c84-8f37-7d289d8be65b');
GO
CREATE TABLE [usercommons$userprovisioning_userrole] (
	[usercommons$userprovisioningid] bigint NOT NULL,
	[system$userroleid] bigint NOT NULL,
	PRIMARY KEY([usercommons$userprovisioningid],[system$userroleid]),
	CONSTRAINT [uniq_usercommons$userprovisioning_userrole_usercommons$userprovisioningid] UNIQUE ([usercommons$userprovisioningid]));
GO
CREATE INDEX [idx_usercommons$userprovisioning_userrole_system$userrole_usercommons$userprovisioning] ON [usercommons$userprovisioning_userrole] ([system$userroleid] ASC,[usercommons$userprovisioningid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('3384400e-d0ca-4f48-94e9-187a54fd80fa', 'UserCommons.UserProvisioning_UserRole', 'usercommons$userprovisioning_userrole', '7e717f5c-e099-43e0-9cb7-738cb027cff0', '92ef30a6-de04-423c-84fd-a21e9b9eeae2', 'usercommons$userprovisioningid', 'system$userroleid', 'idx_usercommons$userprovisioning_userrole_system$userrole_usercommons$userprovisioning', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$userprovisioning_userrole_usercommons$userprovisioningid', '3384400e-d0ca-4f48-94e9-187a54fd80fa', 'e628e906-2481-3f4b-93ae-16db76959f30');
GO
CREATE TABLE [usercommons$userprovisioning_principalattribute] (
	[usercommons$userprovisioningid] bigint NOT NULL,
	[mxmodelreflection$mxobjectmemberid] bigint NOT NULL,
	PRIMARY KEY([usercommons$userprovisioningid],[mxmodelreflection$mxobjectmemberid]),
	CONSTRAINT [uniq_usercommons$userprovisioning_principalattribute_usercommons$userprovisioningid] UNIQUE ([usercommons$userprovisioningid]));
GO
CREATE INDEX [idx_usercommons$userprovisioning_principalattribute_mxmodelreflection$mxobjectmember_usercommons$userprovisioning] ON [usercommons$userprovisioning_principalattribute] ([mxmodelreflection$mxobjectmemberid] ASC,[usercommons$userprovisioningid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('59c02eb4-24e6-4794-bd68-c57d0f44b2ab', 'UserCommons.UserProvisioning_PrincipalAttribute', 'usercommons$userprovisioning_principalattribute', '7e717f5c-e099-43e0-9cb7-738cb027cff0', '398a1f70-2b2c-408e-8778-f4a923fda765', 'usercommons$userprovisioningid', 'mxmodelreflection$mxobjectmemberid', 'idx_usercommons$userprovisioning_principalattribute_mxmodelreflection$mxobjectmember_usercommons$userprovisioning', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$userprovisioning_principalattribute_usercommons$userprovisioningid', '59c02eb4-24e6-4794-bd68-c57d0f44b2ab', '7e36541f-6b77-3ef6-bada-39550d3640d6');
GO
CREATE TABLE [usercommons$userprovisioning_customuserprovisioning] (
	[usercommons$userprovisioningid] bigint NOT NULL,
	[mxmodelreflection$microflowsid] bigint NOT NULL,
	PRIMARY KEY([usercommons$userprovisioningid],[mxmodelreflection$microflowsid]),
	CONSTRAINT [uniq_usercommons$userprovisioning_customuserprovisioning_usercommons$userprovisioningid] UNIQUE ([usercommons$userprovisioningid]));
GO
CREATE INDEX [idx_usercommons$userprovisioning_customuserprovisioning_mxmodelreflection$microflows_usercommons$userprovisioning] ON [usercommons$userprovisioning_customuserprovisioning] ([mxmodelreflection$microflowsid] ASC,[usercommons$userprovisioningid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('30423d7d-ba17-4afa-a5a1-d4e95cf71f68', 'UserCommons.UserProvisioning_CustomUserProvisioning', 'usercommons$userprovisioning_customuserprovisioning', '7e717f5c-e099-43e0-9cb7-738cb027cff0', '17e6df01-1431-4547-b824-d471e84f719c', 'usercommons$userprovisioningid', 'mxmodelreflection$microflowsid', 'idx_usercommons$userprovisioning_customuserprovisioning_mxmodelreflection$microflows_usercommons$userprovisioning', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$userprovisioning_customuserprovisioning_usercommons$userprovisioningid', '30423d7d-ba17-4afa-a5a1-d4e95cf71f68', '49b8e8ad-a539-3ca5-ab32-a5be477e7a6e');
GO
CREATE TABLE [usercommons$userprovisioning_customentity] (
	[usercommons$userprovisioningid] bigint NOT NULL,
	[mxmodelreflection$mxobjecttypeid] bigint NOT NULL,
	PRIMARY KEY([usercommons$userprovisioningid],[mxmodelreflection$mxobjecttypeid]),
	CONSTRAINT [uniq_usercommons$userprovisioning_customentity_usercommons$userprovisioningid] UNIQUE ([usercommons$userprovisioningid]));
GO
CREATE INDEX [idx_usercommons$userprovisioning_customentity_mxmodelreflection$mxobjecttype_usercommons$userprovisioning] ON [usercommons$userprovisioning_customentity] ([mxmodelreflection$mxobjecttypeid] ASC,[usercommons$userprovisioningid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('ee4c5a15-e277-474a-a2e4-105d546870b2', 'UserCommons.UserProvisioning_CustomEntity', 'usercommons$userprovisioning_customentity', '7e717f5c-e099-43e0-9cb7-738cb027cff0', 'aae44a99-27e6-437f-9c9b-964fdcbeb825', 'usercommons$userprovisioningid', 'mxmodelreflection$mxobjecttypeid', 'idx_usercommons$userprovisioning_customentity_mxmodelreflection$mxobjecttype_usercommons$userprovisioning', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_usercommons$userprovisioning_customentity_usercommons$userprovisioningid', 'ee4c5a15-e277-474a-a2e4-105d546870b2', 'b3b91c53-e6d8-32ed-b8b6-509cc4f66920');
GO
CREATE TABLE [system$backgroundjob_session] (
	[system$backgroundjobid] bigint NOT NULL,
	[system$sessionid] bigint NOT NULL,
	PRIMARY KEY([system$backgroundjobid],[system$sessionid]),
	CONSTRAINT [uniq_system$backgroundjob_session_system$backgroundjobid] UNIQUE ([system$backgroundjobid]));
GO
CREATE INDEX [idx_system$backgroundjob_session_system$session_system$backgroundjob] ON [system$backgroundjob_session] ([system$sessionid] ASC,[system$backgroundjobid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('19892a2b-f17a-4c29-80c1-c81f8025b36c', 'System.BackgroundJob_Session', 'system$backgroundjob_session', '660db38b-5ab4-4d15-b649-93a947ecea82', '37f9fd49-5318-4c63-9a51-f761779b202f', 'system$backgroundjobid', 'system$sessionid', 'idx_system$backgroundjob_session_system$session_system$backgroundjob', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$backgroundjob_session_system$backgroundjobid', '19892a2b-f17a-4c29-80c1-c81f8025b36c', '60770e0f-201c-3f24-8a1e-d8b42a715ddb');
GO
CREATE TABLE [system$backgroundjob_xasinstance] (
	[system$backgroundjobid] bigint NOT NULL,
	[system$xasinstanceid] bigint NOT NULL,
	PRIMARY KEY([system$backgroundjobid],[system$xasinstanceid]),
	CONSTRAINT [uniq_system$backgroundjob_xasinstance_system$backgroundjobid] UNIQUE ([system$backgroundjobid]));
GO
CREATE INDEX [idx_system$backgroundjob_xasinstance_system$xasinstance_system$backgroundjob] ON [system$backgroundjob_xasinstance] ([system$xasinstanceid] ASC,[system$backgroundjobid] ASC);
GO
INSERT INTO [mendixsystem$association] ([id], [association_name], [table_name], [parent_entity_id], [child_entity_id], [parent_column_name], [child_column_name], [index_name], [storage_format]) VALUES ('fc3944c4-7a19-4a4d-9b0d-4a0c9d7aeb23', 'System.BackgroundJob_XASInstance', 'system$backgroundjob_xasinstance', '660db38b-5ab4-4d15-b649-93a947ecea82', 'd4154981-8dac-4150-aec5-efa3ef62a7a2', 'system$backgroundjobid', 'system$xasinstanceid', 'idx_system$backgroundjob_xasinstance_system$xasinstance_system$backgroundjob', 2);
GO
INSERT INTO [mendixsystem$unique_constraint] ([name], [table_id], [column_id]) VALUES ('uniq_system$backgroundjob_xasinstance_system$backgroundjobid', 'fc3944c4-7a19-4a4d-9b0d-4a0c9d7aeb23', '4fcadd5b-cfd5-3991-bdb8-19c4d63b1aa5');
GO
INSERT INTO [mendixsystem$version] ([versionnumber], [lastsyncdate], [preanalysismigrationversionnumber]) VALUES ('4.2', '20260528 18:08:45', '4.4.0');
GO
