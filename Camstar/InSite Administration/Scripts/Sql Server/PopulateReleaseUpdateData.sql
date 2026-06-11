--------------------------------------------------------------------------------
-- SCRIPT:PopulateReleaseUpdateData.sql
-- DESCR: Populate specific release default data for an upgrade installation.
--
-- Change History:
--  08/16/2022 Insert Job related HISTINQ during upgrade.
--  11/11/2022 Insert the default numbering rule (JobOrder_Name) for Job Create if it does not exist yet.
--  06/27/2024 Update the action rule (isSingleContainerRule) for SMEL during database update.
--  02/18/2025 Changes to set MarkerType to 'Resource' for existing data while version upgrade in ResourceLayoutDetails
--  03/13/2025 Expansion of the 'ViolationName' field in the 'SPCViolationHistoryDetail' table for use in the 'InlineSPC' server logic.
--  04/22/2025 Added User Query named 'mxEProcContainerHeader' for Eproc Container Header 
--  04/25/2025 Added User Query named 'mxContainerSearch' to return the container information on the Operational View page 
--  07/03/2025 The 'csiSTInstall_UpdateSPCRulesData' procedure sets the 'IsFrozen' field to '0' in the 'A_SPCRules' and 'SPCViolation' tables if both the tables and the field exist.
-- Copyright Siemens 2025

BEGIN
	SET NOCOUNT ON
	declare @mycount int;
	declare @recordcount int;
	declare @updatecount int;
	declare @InstanceId varchar(16);

select @mycount = count(*) from histinq;
select @recordcount = count(*) from HistInq WHERE HISTINQID = '000dca000000008b';
if @mycount > 0 AND @recordcount = 0
    BEGIN
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008b','JobOrderName',0,'JobOrderName','<Unknown Job Order>',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008c','JobModelName',0,'JobModelName','<Unknown Job Model>',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008d','ToStageName',0,'ToStageName','',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008e','ToStageSequence',0,'ToStageSequence','',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008f','PartName',0,'PartDetails.Name','<Unknown Part Name>',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000090','RequestOrderName',0,'RequestOrderName','<Unknown Request Order Name>',1,3530);
	INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000091','ReleaseReason',0,'Transaction::InProcessHistoryCDO.ReleaseReason.Name','<Unknown Release Reason Name>',1,3530);
   UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000090' WHERE CDODEFID = 3530;
	END

select @mycount = count(*) from numberingrule;
select @recordcount = count(*) from numberingrule where NUMBERINGRULENAME = 'JobOrder_Name';
if @mycount > 0 AND @recordcount = 0
    BEGIN
		EXEC csiPRDGetNextInstanceId 7722, @InstanceId OUTPUT
		INSERT INTO NUMBERINGRULE (NUMBERINGRULEID, NUMBERINGRULENAME, NUMBERINGRULETYPE, PREFIX, SEQUENCELENGTH, USEALPHANUMBERICVALUE, USEHEXADECIMALVALUE,
                           USEPREFIXBASED, CHANGECOUNT, ISFROZEN, ISROLLOVER, EXCLUDEDVALUES, CDOTypeId) 
		VALUES (@InstanceId, 'JobOrder_Name', 0, '"JobOrder-"', 6, 0, 0, 0, 1, 0, 0, 33636608, 7722);

		INSERT INTO IDControl (IDType,  NextID) 
		VALUES (@InstanceId, 0);
	END

select @mycount = count(*) from ACTIONRULE;	
select @recordcount = count(*) from ACTIONRULE WHERE ACTIONRULENAME = 'IsSingleContainerRule';
if @mycount > 0 AND @recordcount > 0
    BEGIN
		UPDATE ACTIONRULE SET EXPRESSION =  'not(IsFieldDefined("Containers", GetCurrentService())) or GetListCount(GetCurrentService().Containers) = 1 or IsFieldDefined("ServiceIsContainerTxn",GetCurrentService())' WHERE  ACTIONRULENAME = 'IsSingleContainerRule';
	END
END

BEGIN TRY
SELECT @mycount = COUNT(*) FROM ResourceLayoutDetails;	
SELECT @updatecount = COUNT(*) FROM ResourceLayoutDetails WHERE ResourceId IS NOT NULL AND MarkerType IS NULL;
if @mycount > 0 AND @updatecount > 0
    BEGIN
		UPDATE ResourceLayoutDetails SET MarkerType = 'Resource' WHERE ResourceId IS NOT NULL AND MarkerType IS NULL;
	END
	
PRINT 'Number of rows updated: ' + CAST(@recordcount AS NVARCHAR(10));	
END TRY
BEGIN CATCH
    -- Capture and print the error message
    DECLARE @ErrorMessage NVARCHAR(4000);
    SET @ErrorMessage = ERROR_MESSAGE();

    PRINT 'Error occurred: ' + @ErrorMessage;
END CATCH;
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiInstall_CreateNewDefinition
-- DESCR: Helper function to create summary table def
--
-- Copyright Siemens 2024  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiInstall_CreateNewDefinition' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiInstall_CreateNewDefinition
GO
CREATE PROCEDURE csiInstall_CreateNewDefinition(@Name NVARCHAR(255)
                                                      ,@Description NVARCHAR(512)
                                                      ,@Notes NVARCHAR(512)
                                                      ,@SummarySQL NVARCHAR(MAX)
                                                      ,@IsView BIT
                                                      ,@TableName NVARCHAR(255)
                                                      ,@IsManuallyExecuted BIT
                                                      ,@ScheduleDaysOfWeek NVARCHAR(255)
                                                      ,@ScheduleDaysOfMonth NVARCHAR(255)
                                                      ,@ScheduleHours NVARCHAR(255)
                                                      ,@ScheduleMonths NVARCHAR(255)
                                                      ,@IsFrozen BIT
                                                      ,@IsEnabled BIT)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CDODefId INT
    DECLARE @InstanceId CHAR(16)
    SET @CDODefId=8236
    DECLARE @summDefExists INT

    SELECT @summDefExists = COUNT(1) FROM SummaryTableDef WHERE SummaryTableDefName = @Name

	IF @summDefExists = 0
	BEGIN
		EXEC csiPRDGetNextInstanceId @CDODefId,@InstanceId OUTPUT
		INSERT INTO SummaryTableDef(SummaryTableDefId
								   ,SummaryTableDefName
								   ,Description
								   ,Notes
								   ,SummarySQL
								   ,IsView
								   ,TargetTableName
								   ,IsManuallyExecuted
								   ,ScheduleDaysOfWeek
								   ,ScheduleDaysOfMonth
								   ,ScheduleHours
								   ,ScheduleMonths
								   ,IsFrozen
								   ,CDOTypeId
								   ,ChangeCount
								   ,IsEnabled)
		   VALUES (@InstanceId
				  ,@Name
				  ,@Description
				  ,@Notes
				  ,@SummarySQL
				  ,@IsView
				  ,@TableName
				  ,@IsManuallyExecuted
				  ,@ScheduleDaysOfWeek
				  ,@ScheduleDaysOfMonth
				  ,@ScheduleHours
				  ,@ScheduleMonths
				  ,@IsFrozen
				  ,@CDODefId
				  ,0
				  ,@IsEnabled)
	END
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiInstall_PopulateDefaultData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiInstall_PopulateDefaultData
GO
CREATE PROCEDURE csiInstall_PopulateDefaultData
AS
DECLARE @SQLString NVARCHAR(MAX)
BEGIN
    SET NOCOUNT ON;
	SET @SQLString= 'WITH CDOData AS ( '+
    'SELECT cf.*  '+
    'FROM CDODefinition cd  '+
    'JOIN CDOFields cf ON cd.CDODefID = cf.CDODefID '+
    ' WHERE cd.CDODefID = 4841613 '+
    '), CombinedData AS ( '+
    'SELECT p.ProductId, '+
        'pb.ProductName,  '+
        'ISNULL(c.ContainerName, mqd.isLot) AS "Container", '+
        'ISNULL(c.Qty, mqd.isQtyAvailable) AS "AvailableQty", '+
        'mqd.isConsumedQty, '+
        'mqd.isRemovalStrategy, '+
        'ISNULL(c.ExpirationDate, isExpirationDate) AS "ExpirationDate", '+
        'rd.ResourceName, '+
        'NULL AS isInventoryLocationName, '+
        'mq.isMaterialQueueName, '+
        'o.OperationName, '+
        'uom.UOMName, '+
        '(cdo.FieldName + '' - '' + cdo.FieldDescription) AS RemovalStrategy, '+
        'CASE WHEN mq.isActive = 1 THEN ''Active'' ELSE ''Inactive'' END AS isActive, '+
        ' ''Queue'' AS indicator,(Select b.ContainerName from Container b where b.ContainerId=mq.isContainerObjectId) as IssueContainer, '+
		'(Select mo.MfgOrderName from MfgOrder mo where mo.MfgOrderId =mq.isMfgOrderId) as "Mfg Order", '+
		'pt.ProductTypeName '+
    'FROM isMaterialQueueDetails mqd  '+
    'JOIN Product p ON p.ProductId = mqd.isProductId '+ 
	'JOIN ProductBase pb ON pb.ProductBaseId = p.ProductBaseId   '+
    'LEFT JOIN ProductType pt ON p.ProductTypeId = pt.ProductTypeId  '+
    'LEFT JOIN isMaterialQueue mq ON mq.isMaterialQueueId = mqd.isMaterialQueueId '+
    'LEFT JOIN Container c ON c.ContainerId = mqd.isContainerId  '+
    'LEFT JOIN ResourceDef rd ON rd.ResourceId = mq.isResourceId  '+
    'LEFT JOIN Operation o ON o.OperationId = mq.isOperationId  '+
    ' LEFT JOIN UOM uom ON mqd.isUOMid = uom.UOMId '+
    'LEFT JOIN CDOData cdo ON mqd.isRemovalStrategy = cdo.DefaultValue '+
    'UNION '+
    'SELECT  p.ProductId, '+
        'pb.ProductName, '+
        'ISNULL(c.ContainerName, id.isLot) AS "Container", '+
        'ISNULL(c.Qty, id.isQty) AS "AvailableQty", '+
        'NULL AS isConsumedQty,  '+
        'id.isRemovalStrategy, '+
        'ISNULL(c.ExpirationDate, isExpirationDate) AS "ExpirationDate", '+
        'rd.ResourceName, '+
        'il.isInventoryLocationName, '+ 
        'NULL AS isMaterialQueueName, '+
        'NULL AS OperationName, '+
        'uom.UOMName,  '+
        '(cdo.FieldName + '' - '' + cdo.FieldDescription) AS RemovalStrategy,  '+
        'NULL AS isActive, '+
        '''Inventory'' AS indicator, '+
		 'NULL AS IssueContainer,  '+
        'NULL AS "Mfg Order", '+
		'pt.ProductTypeName '+
    'FROM isInventoryDetails id  '+
    'JOIN Product p ON p.ProductId = id.isProductId  '+
    'JOIN ProductBase pb ON pb.ProductBaseId = p.ProductBaseId  '+
    'LEFT JOIN ProductType pt ON p.ProductTypeId = pt.ProductTypeId  '+
    'LEFT JOIN Container c ON c.ContainerId = id.isContainerId '+
    'LEFT JOIN isInventoryLocation il ON il.isInventoryLocationId = id.isInventoryLocationId  '+
    'LEFT JOIN ResourceDef rd ON rd.ResourceId = il.isParentResourceId   '+
    'LEFT JOIN UOM uom ON id.isUOMid = uom.UOMId  '+
    'LEFT JOIN CDOData cdo ON id.isRemovalStrategy = cdo.DefaultValue '+
    ') '+
'SELECT  '+
    '*, '+
	 'Case when isMaterialQueueName is not null then isMaterialQueueName else isInventoryLocationName end as LocationName '+
'FROM CombinedData'

	EXEC csiInstall_CreateNewDefinition N'MATQUEUENINV', 
										  'Material Queue And Inventory Details', 
										  NULL, 
										  @SQLString, 
										  1, 
										  'MATQUEUENINV', 
										  0, 
										  NULL, 
										  NULL, 
										  '0,', 
										  NULL, 
										  0,
										  1 
 
END
GO
EXEC csiInstall_PopulateDefaultData
GO
DROP PROCEDURE csiInstall_PopulateDefaultData
GO
DROP PROCEDURE csiInstall_CreateNewDefinition
GO


--------------------------------------------------------------------------------
-- Expansion of the 'ViolationName' field in the 'SPCViolationHistoryDetail' table for use in the 'InlineSPC' server logic.
BEGIN
EXEC csiIncreaseStringColMaxLength 'SPCViolationHistoryDetail','VIOLATIONNAME',2000
END

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2025  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_CreateNewUserQuery' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_CreateNewUserQuery
GO
CREATE PROCEDURE csiSTInstall_CreateNewUserQuery(@Name NVARCHAR(255)
                                                      ,@Description NVARCHAR(512)
                                                      ,@Notes NVARCHAR(512)
                                                      ,@QueryText NVARCHAR(MAX)                                                      
                                                      ,@IsFrozen BIT
													  ,@Param1Name NVARCHAR(50)
                                                      ,@Param1Type INT
													  ,@Param2Name NVARCHAR(50)
                                                      ,@Param2Type INT
													  ,@Param3Name NVARCHAR(50)
                                                      ,@Param3Type INT
													  ,@Param4Name NVARCHAR(50)
                                                      ,@Param4Type INT)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InstanceID CHAR(16)
    DECLARE @CurrInstanceID CHAR(16)
    DECLARE @CDODefId INT
    DECLARE @ParamCDODefId INT
    DECLARE @ParamFieldId INT
    DECLARE @QueryTypeId INT
    DECLARE @IID CHAR(16)
    SET @CDODefId=7068
    SET @ParamCDODefId=7071
    SET @ParamFieldId=8597
    SET @QueryTypeId = 4  
    

    EXEC csiPRDGetNextInstanceId @CDODefId,@InstanceId OUTPUT
    INSERT INTO UserQuery(UserQueryId
                         ,UserQueryName
                         ,Description
                         ,Notes
                         ,QueryText
                         ,IsFrozen
                         ,CDOTypeId
                         ,QueryTypeId)
       VALUES (@InstanceId
              ,@Name
              ,@Description
              ,@Notes
              ,@QueryText
              ,@IsFrozen
              ,@CDODefId
              ,@QueryTypeId)   
			  
	IF (@Param1Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param1Name,@InstanceID,@IsFrozen,@Param1Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,1,@InstanceID,@IID)
    END
    IF (@Param2Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param2Name,@InstanceID,@IsFrozen,@Param2Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,2,@InstanceID,@IID)
    END
    IF (@Param3Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param3Name,@InstanceID,@IsFrozen,@Param3Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,3,@InstanceID,@IID)
    END
    IF (@Param4Name IS NOT NULL)
    BEGIN       
       EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (@IID,@ParamCDODefId,@Param4Name,@InstanceID,@IsFrozen,@Param4Type,0)
       --
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (@ParamFieldId,4,@InstanceID,@IID)
    END
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_CreateNewOnlineQuerySetup' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_CreateNewOnlineQuerySetup
GO
CREATE PROCEDURE csiSTInstall_CreateNewOnlineQuerySetup(@Name NVARCHAR(255)
                                                      ,@Description NVARCHAR(512)
                                                      ,@Notes NVARCHAR(512)
                                                      ,@QueryText NVARCHAR(MAX)                                                      
                                                      ,@IsFrozen BIT
                                                      ,@Param1Name NVARCHAR(50)
                                                      ,@Param1Type INT
													  ,@Param1Value NVARCHAR(500)
                                                      ,@Param2Name NVARCHAR(50)
                                                      ,@Param2Type INT
													  ,@Param2Value NVARCHAR(500)
                                                      ,@Param3Name NVARCHAR(50)
                                                      ,@Param3Type INT
													  ,@Param3Value NVARCHAR(500))
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InstanceID CHAR(16)
    DECLARE @CurrInstanceID CHAR(16)
    DECLARE @CDODefId INT
    DECLARE @ParamCDODefId INT
    DECLARE @ParamFieldId INT
    DECLARE @QueryTypeId INT
    DECLARE @IID CHAR(16)
    SET @CDODefId=4751422
    SET @ParamCDODefId=4751770
    SET @ParamFieldId=8597
    SET @QueryTypeId = 4
    
    -- Remove the existing UserQuery and its Parameters, if it exists
    IF NOT EXISTS (SELECT UserQueryId
               FROM UserQuery
               WHERE UserQueryName = @Name)
    BEGIN
        EXEC csiPRDGetNextInstanceId @CDODefId,@InstanceId OUTPUT
        INSERT INTO UserQuery(UserQueryId
                             ,UserQueryName
                             ,Description
                             ,Notes
                             ,QueryText
                             ,IsFrozen
                             ,CDOTypeId
                             ,QueryTypeId)
           VALUES (@InstanceId
                  ,@Name
                  ,@Description
                  ,@Notes
                  ,@QueryText
                  ,@IsFrozen
                  ,@CDODefId
                  ,@QueryTypeId)
        IF (@Param1Name IS NOT NULL)
        BEGIN       
           EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (@IID,@ParamCDODefId,@Param1Name,@Param1Value,@InstanceID,@IsFrozen,@Param1Type,0)
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (@ParamFieldId,1,@InstanceID,@IID)
        END
    
        IF (@Param2Name IS NOT NULL)
        BEGIN       
           EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT
           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (@IID,@ParamCDODefId,@Param2Name,@Param2Value,@InstanceID,@IsFrozen,@Param2Type,0)
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (@ParamFieldId,2,@InstanceID,@IID)
        END
    
        IF (@Param3Name IS NOT NULL)
        BEGIN       
           EXEC csiPRDGetNextInstanceId @ParamCDODefId,@IID OUTPUT

           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (@IID,@ParamCDODefId,@Param3Name,@Param3Value,@InstanceID,@IsFrozen,@Param3Type,0)
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (@ParamFieldId,3,@InstanceID,@IID)
        END
    END
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_PopulateDefaultUserQueryData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO
CREATE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
AS
BEGIN
DECLARE @SQLString NVARCHAR(MAX)
DECLARE @userQueryExists INT

    SELECT @userQueryExists = COUNT(1) FROM UserQuery WHERE UserQueryName = 'mxEProcContainerHeader'

	IF @userQueryExists = 0
    BEGIN
    SET NOCOUNT ON;
    
SET @SQLString = convert(nvarchar(max), N'') + N'SELECT $Container.InstanceId '+ 
    ',$CurrentStatus.Spec AS @fieldheader:name="Spec" :datatype=String'+
    ',$HoldReason.InstanceId AS @fieldheader:name="HoldReasonId" :datatype=String'+
    ',$Container.CurrentHoldCount AS @fieldheader : name = "CurrentHoldCount" : datatype = Integer'+
    ',$Container.Name AS @fieldheader:name="Container":label=SelVal_Container '+ 
    ',$ContainerLevel.Name AS @fieldheader:name="Level":label=SelVal_Level '+ 
    ',$Carrier.InstanceId AS @fieldheader:name="Carrier":datatype=Object '+ 
    ',$Carrier.Name AS @fieldheader:name="CarrierName":datatype=String:label=CSICDOName_Carrier '+ 
    ',$Step.Name AS @fieldheader:name="Step":label=SelVal_Step '+ 
    ',$PriorityCode.Name AS @fieldheader:name="Priority":label=SelVal_PriorityCode '+ 
    ',Product.ProductName AS @header:name="Product":datatype=String:label=SelVal_Product '+ 
    ',Product.ProductRevision AS @header:name="Revision":datatype=String:label=SelVal_Revision '+ 
    ',Product.Description AS @header:name="ProductDescription":datatype=String:label=SelVal_ProductDescription '+ 
	',ProductROR AS @header:name="ProductROR": datatype = Boolean '+
    ',$Container.Qty '+ 
    ',$CurrentStatus.InProcess '+ 
    ',ContainerStatusLabel.LabelValue AS @header:name="Status":datatype=String:label=Container_Status '+ 
    ',$Operation.Name AS @fieldheader:name="Operation":label=SelVal_Operation '+ 
    ',$Operation.UseQueue '+
	',$MfgOrder.Name AS @fieldheader:name="MfgOrder":label=SelVal_ManufacturingOrder '+
	',$UOM.Name AS @fieldheader:name="UOM":label=SelVal_UOM '+
	',COALESCE(Container.DueDate, CAST(''12/31/9999'' AS DATETIME)) AS @header:name="DueDate":datatype=Timestamp:label=Container_DueDate '+
	',COALESCE(Container.OriginalStartDate, CAST(''12/31/9999'' AS DATETIME)) AS @header:name="OriginalStartDate":datatype=Timestamp:label=ContainerHistoryInquiry_StartDate '+
	',CASE COALESCE(Container.CurrentHoldCount, 0) WHEN 0 THEN 0 ELSE 1 END AS @header:name="IsOnHold":datatype=Boolean:label=Container_IsOnHold '+
	',$HoldReason.Name AS @fieldheader:name="HoldReason":label=Container_HoldReason '+
	',$Owner.Name AS @fieldheader:name="OwnerName":label=Container_Owner '+
	',COALESCE(CurrentStatus.TimersCount, 0) AS @header:name="TimersCount":datatype=Integer:label=CurrentStatus_TimersCount '+
	',$Spec.ElectronicProcedure '+
	',ResDef.ResourceName AS @header:name="ResourceName":datatype=String '+
	',$Workflow.Revision AS @fieldheader:name="WorkflowRevision" :datatype=String '+
	',$WorkflowBase.Name AS @fieldheader:name="WorkflowName" :datatype=String '+
	',$ElectronicProcedure.Revision AS @fieldheader:name="ElectronicProcedureRevision" :datatype=String '+
	',$ElectronicProcedureBase.Name AS @fieldheader:name="ElectronicProcedureName" :datatype=String '+
	'FROM $Container '+
	'LEFT JOIN $MfgOrder ON $MfgOrder.InstanceId = $Container.MfgOrder '+ 
	'LEFT JOIN $Owner ON $Owner.InstanceID = $Container.Owner '+
	'LEFT JOIN (SELECT * FROM csiGetEnumerationLabels
    (''ContainerStatusEnum'', $$primaryDictionary, $$secondaryDictionary)) ContainerStatusLabel
  ON ContainerStatusLabel.DefaultValue = $Container.Status '+
  'LEFT JOIN $PriorityCode ON $PriorityCode.InstanceId = $Container.Priority '+
  'LEFT JOIN $HoldReason ON $HoldReason.InstanceId = $Container.HoldReason '+
  'LEFT JOIN (
     SELECT Product.ProductId, ProductBase.ProductName, Product.ProductRevision, Product.Description, CASE WHEN ProductBase.RevOfRcdId = Product.ProductId THEN 1 ELSE 0 END As ProductROR FROM Product LEFT JOIN ProductBase ON Product.ProductBaseId = ProductBase.ProductBaseId
     )     Product ON Product.ProductId = Container.ProductId '+
	 'LEFT JOIN $UOM ON $UOM.InstanceId = $Container.UOM '+
	 'JOIN      $CurrentStatus ON $CurrentStatus.InstanceId = $Container.CurrentStatus '+
	 'JOIN      $Spec ON $Spec.InstanceId = $CurrentStatus.Spec  '+
	 'JOIN      $Step ON $Step.InstanceId = $CurrentStatus.WorkflowStep '+
	 'JOIN      $Operation ON $Operation.InstanceId = $Spec.Operation '+
	 'JOIN      $ContainerLevel ON $ContainerLevel.InstanceId = $Container.Level '+
	 'LEFT JOIN $WorkCenter ON $WorkCenter.InstanceId = $Operation.WorkCenter '+
	 'LEFT JOIN ResourceDef ResDef ON ResDef.ResourceId = $CurrentStatus.Resource '+
	 'LEFT JOIN $Carrier ON $CurrentStatus.Carrier = $Carrier.InstanceId '+
	 'LEFT JOIN (
            SELECT Timer.ParentId, Timer.ProcessTimerName, Timer.ProcessTimerRevision, Timer.StartTimeGMT, TimerMin.MinEndWarningTimeGMT, TimerMin.MinWarningTimeColor
                , TimerMin.MinEndTimeGMT, TimerMin.MinTimeColor, Timer.MaxEndWarningTimeGMT, Timer.MaxWarningTimeColor, Timer.MaxEndTimeGMT, Timer.MaxTimeColor, Timer.IsStoped
            FROM (SELECT TBL1.ParentId, MIN(TMax.TimerId) as MaxTimerId, MIN(TMin.TimerId) as MinTimerId
                    FROM (SELECT ParentId
                                 ,MIN (CASE WHEN (MaxEndTimeGMT is not null and IsStoped = ''false'') THEN MaxEndTimeGMT END ) as MaxEndTimeGMT
                                 ,MIN (CASE WHEN (MinEndTimeGMT is not null and MaxEndTimeGMT is null and IsStoped = ''false'') THEN MinEndTimeGMT END ) as MinEndTimeGMT
                            FROM Timer
                        GROUP BY ParentId) TBL1
                LEFT JOIN Timer TMax on TBL1.ParentId = TMax.ParentId and TBL1.MaxEndTimeGMT = TMax.MaxEndTimeGMT
                LEFT JOIN Timer TMin on TBL1.ParentId = TMin.ParentId and TBL1.MinEndTimeGMT = TMin.MinEndTimeGMT
            GROUP BY TBL1.ParentId
            ) TBL2
            JOIN Timer ON Timer.TimerId = COALESCE(TBL2.MaxTimerId, TBL2.MinTimerId)
            LEFT JOIN Timer TimerMin ON TimerMin.TimerId = TBL2.MinTimerId
   	   ) Timer on Timer.ParentId = CurrentStatus.CurrentStatusId '+
	  'LEFT JOIN $Workflow ON $Workflow.InstanceId = $Step.Parent '+
	  'LEFT JOIN $WorkflowBase ON $WorkflowBase.InstanceId = $Workflow.Base '+
	  'LEFT JOIN $ElectronicProcedureBase ON $ElectronicProcedureBase.InstanceId =
	    (SELECT SP.ElectronicProcedureBaseId FROM Spec as SP WHERE SP.SpecId = $Spec.InstanceId) '+
	  'LEFT JOIN $ElectronicProcedure ON $ElectronicProcedure.Base = $ElectronicProcedureBase.InstanceId '+
	  'WHERE ($Container.Status = 1 OR $Container.Status = 3) '+
	  'AND $Container.Name = ?ContainerName '

	EXEC csiSTInstall_CreateNewUserQuery 'mxEProcContainerHeader','Container Header information used in Electronic procedure','OOB query for container header used in E procedure',@SQLString,0,'ContainerName',4, null,4,null,4,null,4
    END


SET @SQLString = convert(nvarchar(max), N'') + N'SELECT '+
'  Container.ContainerName, '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  Operation.OperationName, '+
'  CDODefinition.CDOName, '+
'  HistoryMainline.Comments '+
' FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+
'  WHERE '+
'  HistoryMainline.Comments  Is Not Null   '+
'   AND '+
'   HistoryMainline.ReversalStatus  =  1 '+
' AND '+
' Container.ContainerName = ?ContainerName '

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionComments','Comments','Comments query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N' SELECT '+
'  DataPointHistoryDetail.DataName, '+
'  DataPointHistoryDetail.DataValue, '+
'  DataPointHistoryDetail.LowerLimit, '+
'  DataPointHistoryDetail.UpperLimit, '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Operation.OperationName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  DataPointUOM.UOMName, '+
'  HistoryMainline.Comments '+
' FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   LEFT OUTER JOIN DataPointHistory ON (DataPointHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN DataPointHistoryDetail ON (DataPointHistoryDetail.DataPointHistoryId=DataPointHistory.DataPointHistoryId) '+
'   LEFT OUTER JOIN UOM  DataPointUOM ON (DataPointUOM.UOMId=DataPointHistoryDetail.UOMId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+
'  WHERE '+
'   DataPointHistoryDetail.DataName IS NOT NULL '+
' AND '+
' DataPointHistoryDetail.DataValue not Between DataPointHistoryDetail.LowerLimit and DataPointHistoryDetail.UpperLimit '+
'  and Container.ContainerName = ?ContainerName '

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionDataCollection','Data Collections','Data Collection query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'  SELECT '+
'  Quality_Event.EventName, '+
'  Quality_Event.ReportedDate, '+
'  FailureMode.Description, '+
'  FailureMode.FailureModeName, '+
'  DT_EventData.NCRFailureTypeName, '+
'  DT_EventData.FS_FailureSeverityName, '+
'  DT_EventData.ProductName, '+
'  CASE Quality_Event.Status WHEN 1 THEN ''Active'' WHEN 2 THEN ''Pending''  WHEN 3 THEN ''Escalated'' WHEN 4 THEN ''Void''  WHEN 5 THEN ''Closed'' WHEN 6 THEN ''Deleted'' WHEN 7 THEN ''Initiated'' WHEN 8 THEN ''InReview'' END as Status, '+
'  EventLot.Lot, '+
'  Initiator.EmployeeName, '+
'  EventFailure.Comments, '+
'  Event_QualityResolutionCode.QualityResolutionCodeName, '+
'  Event_NCRCauseCode.NCRCauseCodeName '+
'FROM '+
'  EventLot RIGHT OUTER JOIN (  '+
'  SELECT  '+
'ED.EventId, '+ 
'ED.EventDataId, '+ 
'ED.WorkflowName, '+ 
'ED.WorkflowRev, '+ 
'ED.OperationName, '+ 
'ED.SpecRev, '+ 
'ED.SpecName, '+ 
'ED.ResourceName, '+ 
'ED.WorkCenterName, '+ 
'ED.MaintenanceReqRev, '+ 
'ED.MaintenanceReqName, '+ 
'ED.ProductName, '+ 
'ED.ProductRev, '+ 
'ED.ContactCustomerId, '+ 
'ED.OccupationId, '+ 
'ED.ReportingCustomerId, '+ 
'ED.ReportDate, '+ 
'ED.EventDate, '+ 
'ED.DateReceived, '+ 
'ED.ProblemDescription, '+ 
'ED.SampleQuantity, '+ 
'ED.DeviceLocation, '+ 
'ED.RMANumber, '+ 
'ED.ReturnedPhoneNumber, '+ 
'ED.ReturnedContactName, '+ 
'ED.CompensateCustomerActionId, '+ 
'ED.RecallNumber, '+ 
'ED.AdverseEventId, '+ 
'ED.DeviceEvaluatedId, '+ 
'ED.DeviceAvailableId, '+ 
'ED.DeviceReturnedId, '+ 
'ED.DeviceOperatorId, '+ 
'ED.ProductProblemId, '+ 
'ED.HealthProfessionalId, '+ 
'ED.EventTypeId, '+ 
'ED.ReportFiledWithFDAId, '+ 
'ED.ReportSourceId, '+ 
'ED.DateReceivedGMT, '+ 
'ED.ReportDateGMT, '+ 
'EF.ChangeCount AS EF_ChangeCount, '+ 
'EF.FailureModeId as EF_FailureModeId, '+ 
'EF.Description AS EF_Description, '+ 
'EF.FailureTypeId as EF_FailureTypeId, '+ 
'EF.FailureSeverityId as EF_FailureSeverityId, '+ 
'EF.Comments as EF_Comments, '+
'FS.ChangeCount AS FS_ChangeCount, '+ 
'FS.Notes as FS_Notes, '+ 
'FS.ChangeHistoryId as FS_ChangeHistoryId, '+ 
'FS.Description AS FS_Description, '+ 
'FS.FailureSeverityName as FS_FailureSeverityName, '+ 
'FM.FailureModeName as FM_FailureModeName, '+ 
'FM.DefaultTypeId as FM_DefaultTypeId, '+ 
'FM.DefaultSeverityId, '+ 
'FM.Description AS FM_Description, '+ 
'FM.ChangeCount AS FM_ChangeCount, '+
'FM.Notes AS FM_Notes, '+ 
'FM.ChangeHistoryId AS FM_ChnageHistoryId, '+
'FT.NCRFailureTypeName '+
' FROM EventData ED  '+
'LEFT OUTER JOIN EventFailure EF ON ED.EventDataId = EF.EventDataId  '+
'LEFT OUTER JOIN FailureSeverity FS ON EF.FailureSeverityId = FS.FailureSeverityId  '+
'LEFT OUTER JOIN FailureMode FM ON EF.FailureModeId = FM.FailureModeId '+
'LEFT OUTER JOIN NCRFailureType FT ON EF.FailureTypeId = FT.NCRFailureTypeId '+
'  )  DT_EventData ON (DT_EventData.EventDataId=EventLot.EventDataId) '+
'   RIGHT OUTER JOIN Event  Quality_Event ON (Quality_Event.EventId=DT_EventData.EventId) '+
'   LEFT OUTER JOIN EventFailure ON (Quality_Event.EventDataId=EventFailure.EventDataId) '+
'   LEFT OUTER JOIN EventFailureCause ON (EventFailure.EventFailureId=EventFailureCause.EventFailureId) '+
'   LEFT OUTER JOIN NCRCauseCode  Event_NCRCauseCode ON (EventFailureCause.CauseCodeId=Event_NCRCauseCode.NCRCauseCodeId) '+
'   LEFT OUTER JOIN FailureMode ON (EventFailure.FailureModeId=FailureMode.FailureModeId) '+
'   LEFT OUTER JOIN Employee  Initiator ON (Quality_Event.InitiatorId=Initiator.EmployeeId) '+
'   LEFT OUTER JOIN QualityResolutionCode  Event_QualityResolutionCode ON (Event_QualityResolutionCode.QualityResolutionCodeId=Quality_Event.QualityResolutionCodeId) '+  
'WHERE '+
'( Quality_Event.InitiatorId=Initiator.EmployeeId  ) '+
'  AND   '+
'  EventLot.Lot  =  ?ContainerName '

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionEvents','Events','Events query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'SELECT '+
'  TaskItem.TaskItemName, '+
'  TaskItem.ReportInstruction AS TIReportInstruction, '+
'  DataPointHistoryDetail.DataName, '+
'  DataPointHistoryDetail.DataValue, '+
'  (case when '+
' ExecuteTaskHistory.Pass = 1 then ''PASS'' when '+
' ExecuteTaskHistory.Pass = 0 then ''FAIL'' end) AS TaskStatus, '+
'  DataPointHistoryDetail.LowerLimit, '+
'  DataPointHistoryDetail.UpperLimit, '+
'  TaskListBase.TaskListName, '+
'  ExecuteTaskHistory.TaskListSequence, '+
'  TaskList.Instruction AS TLInstruction, '+
'  TranWorkflowStep.WorkflowStepName, '+
'  ElectronicProcedureBase2.ElectronicProcedureName, '+
'  Operation.OperationName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  ComputationHistory.ComputationName, '+
'  ComputationHistory.ResultValue, '+
'  CPPDataTypes.Name, '+
'  TaskItem_Computation.Instruction AS TIInstruction, '+
'  ESigMeaning.ESigMeaningName, '+
'  ESigHistoryDetail.SignerFullName, '+
'  ESigHistoryDetail.CosignerFullName, '+
'  HistoryMainline.Comments, '+
'  DataPointUOM.UOMName, '+
'  ExecuteTaskHistory.Sequence, '+
'  TaskList.ReportInstruction AS TLReportInstruction, '+
'  TranWorkflowBase.WorkflowName '+
' FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   LEFT OUTER JOIN ESigHistorySummary ON (ESigHistorySummary.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN ESigHistoryDetail ON (ESigHistoryDetail.ESigHistorySummaryId=ESigHistorySummary.ESigHistorySummaryId) '+
'   LEFT OUTER JOIN ESigMeaning ON (ESigMeaning.ESigMeaningId=ESigHistorySummary.MeaningId) '+
'   INNER JOIN ExecuteTaskHistory ON (HistoryMainline.HistoryMainlineId=ExecuteTaskHistory.HistoryMainlineId) '+
'   LEFT OUTER JOIN TaskItem ON (ExecuteTaskHistory.TaskId=TaskItem.TaskItemId) '+
'   LEFT OUTER JOIN TaskList ON (TaskItem.TaskListId=TaskList.TaskListId) '+
'   LEFT OUTER JOIN TaskListBase ON (TaskList.TaskListBaseId=TaskListBase.TaskListBaseId) '+
'   LEFT OUTER JOIN ElectronicProcedure  ElectronicProcedure2 ON (ExecuteTaskHistory.ElectronicProcedureId=ElectronicProcedure2.ElectronicProcedureId) '+
'   LEFT OUTER JOIN ElectronicProcedureBase  ElectronicProcedureBase2 ON (ElectronicProcedure2.ElectronicProcedureBaseId=ElectronicProcedureBase2.ElectronicProcedureBaseId) '+
'   LEFT OUTER JOIN DataPointHistory ON (DataPointHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN DataPointHistoryDetail ON (DataPointHistoryDetail.DataPointHistoryId=DataPointHistory.DataPointHistoryId) '+
'   LEFT OUTER JOIN UOM DataPointUOM ON (DataPointUOM.UOMId=DataPointHistoryDetail.UOMId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN ComputationHistory ON (HistoryMainline.HistoryMainlineId=ComputationHistory.HistoryMainlineId) '+
'   LEFT OUTER JOIN CPPDataTypes ON (ComputationHistory.ResultDataType=CPPDataTypes.DataTypeID) '+
'   LEFT OUTER JOIN Computation ON (ComputationHistory.ComputationId=Computation.ComputationId) '+
'   LEFT OUTER JOIN ComputationParamSpec ON (Computation.ComputationId=ComputationParamSpec.ParentId) '+
'   LEFT OUTER JOIN ComputationParamMap ON (ComputationParamSpec.ComputationParamSpecId=ComputationParamMap.ComputationVariableId) '+
'   LEFT OUTER JOIN TaskItem  TaskItem_Computation ON (ComputationParamMap.ParentId=TaskItem_Computation.TaskItemId) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+
'   LEFT OUTER JOIN Workflow  TranWorkflow ON (TranWorkflowStep.WorkflowId=TranWorkflow.WorkflowId) '+
'   LEFT OUTER JOIN WorkflowBase  TranWorkflowBase ON (TranWorkflowBase.WorkflowBaseId=TranWorkflow.WorkflowBaseId) '+
' where '+ 
' Container.ContainerName = ?ContainerName '+
' AND ExecuteTaskHistory.Pass = 0 '

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionFailedTasks','Failed Tasks','Failed Tasks query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N' SELECT '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  Operation.OperationName, '+
'  CDODefinition.CDOName, '+
'  HistoryMainline.Comments, '+
'  HoldReasonHistorical.HoldReasonName '+
'FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN HoldReleaseHistory ON (HoldReleaseHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   INNER JOIN HoldReason  HoldReasonHistorical ON (HoldReleaseHistory.HoldReasonId=HoldReasonHistorical.HoldReasonId) '+
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+ 
'WHERE '+
'  ( '+
'   Container.ContainerName  =  ?ContainerName '+
'   AND '+
'   HoldReasonHistorical.HoldReasonName  Is Not Null   '+
'   AND '+
'   CDODefinition.CDODefID  =  6898 '+
'   AND '+
'   HistoryMainline.ReversalStatus  =  1 '+
'   )'

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionHolds','Holds','Holds query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'  SELECT '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  Operation.OperationName, '+
'  CDODefinition.CDOName, '+
'  HistoryMainline.Comments, '+
'  To_WorkflowStep.WorkflowStepName as ToStep, '+
'  HistoryMainline.ReversalStatus '+
'FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN MoveHistory ON (MoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   INNER JOIN WorkflowStep  To_WorkflowStep ON (MoveHistory.ToStepId=To_WorkflowStep.WorkflowStepId) '+
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId)  '+
'WHERE '+
'  ( '+
'   Container.ContainerName  =  ?ContainerName '+
'   AND '+
'   CDODefinition.CDODefID  IN  ( 6650  ) '+
'   AND '+
'   HistoryMainline.ReversalStatus  =  1 '+
' )'

    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionMoveNonStds','Move Non Standards','MoveNonStd query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'SELECT '+
'  Container.ContainerName AS DHR_ContainerName, '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  ProductBase3.ProductName, '+
'  Product3.ProductRevision, '+
'  RemoveHistoryDetail.QtyRemoved, '+
'  RemovalReason.RemovalReasonName, '+
'  RemoveHistoryDetail.DestinationLot, '+
'  RemovalContainer.ContainerName AS RemovalContainer, '+
'  ESigHistoryDetail.SignerFullName, '+
'  ESigHistoryDetail.CosignerFullName, '+
'  ESigMeaning.ESigMeaningName, '+
'  Container.ContainerName, '+
'  RemovedQuantityUOM.UOMName '+
' FROM '+
'  ProductBase  ProductBase3 LEFT OUTER JOIN Product  Product3 ON (Product3.ProductBaseId=ProductBase3.ProductBaseId) '+
'   RIGHT OUTER JOIN RemoveHistoryDetail ON (Product3.ProductId=RemoveHistoryDetail.ProductId) '+
'   LEFT OUTER JOIN ComponentRemoveHistory ON (ComponentRemoveHistory.ComponentRemoveHistoryId=RemoveHistoryDetail.ComponentRemoveHistoryId) '+
'   RIGHT OUTER JOIN HistoryMainline ON (ComponentRemoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN ESigHistorySummary ON (ESigHistorySummary.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN ESigHistoryDetail ON (ESigHistoryDetail.ESigHistorySummaryId=ESigHistorySummary.ESigHistorySummaryId) '+
'   LEFT OUTER JOIN ESigMeaning ON (ESigMeaning.ESigMeaningId=ESigHistorySummary.MeaningId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+
'   LEFT OUTER JOIN Container  RemovalContainer ON (RemoveHistoryDetail.DestinationContainerId=RemovalContainer.ContainerId) '+
'   LEFT OUTER JOIN RemovalReason ON (RemoveHistoryDetail.RemovalReasonId=RemovalReason.RemovalReasonId) '+
'   LEFT OUTER JOIN UOM  RemovedQuantityUOM ON (RemoveHistoryDetail.UOMId=RemovedQuantityUOM.UOMId) '+
'  WHERE '+
'   CDODefinition.CDODefID  =  6862 '+
'   AND '+
'   ( HistoryMainline.ContainerId  = HistoryCrossRef.TrackingId  ) '+
' AND '+
' Container.ContainerName = ?ContainerName '

	EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionRemovals','Removals','Removals query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL

SET @SQLString = convert(nvarchar(max), N'') + N'SELECT '+
'  TranWorkflowStep.WorkflowStepName, '+
'  Employee.EmployeeName, '+
'  HistoryMainline.TxnDate, '+
'  Operation.OperationName, '+
'  CDODefinition.CDOName, '+
'  HistoryMainline.Comments, '+
'  ReworkReason.ReworkReasonName '+
'FROM '+
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) '+
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) '+
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) '+
'   LEFT OUTER JOIN MoveHistory ON (MoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) '+
'   LEFT OUTER JOIN ReworkReason ON (MoveHistory.ReworkReasonId=ReworkReason.ReworkReasonId) '+
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) '+
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) '+
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) '+
' WHERE(Container.ContainerName  =  ?ContainerName AND ReworkReason.ReworkReasonName  Is Not Null   AND HistoryMainline.ReversalStatus  =  1)'
    
    EXEC csiSTInstall_CreateNewOnlineQuerySetup 'ExceptionRework','Rework query for Exception Review','Rework query for Exception Review.',@SQLString,0,'ContainerName',4,'Container.Name',NULL,NULL,NULL,NULL,NULL,NULL
    
END
GO
EXEC csiSTInstall_PopulateDefaultUserQueryData
GO

DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSTInstall_PopulateDefaultUserQueryData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO
CREATE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
AS
DECLARE @SQLString NVARCHAR(MAX)
DECLARE @userQueryExists INT

    SELECT @userQueryExists = COUNT(1) FROM UserQuery WHERE UserQueryName = 'mxContainerSearch'

	IF @userQueryExists = 0
BEGIN
    SET NOCOUNT ON;
    
SET @SQLString = convert(nvarchar(max), N'') + N'SELECT '+ 
'container.ContainerName AS @header : name = "ContainerName" : datatype = String '+ 
',container.Status AS @header : name = "Status" : datatype = Integer '+
',CSE.LabelValue AS @header : name = "STATUSNAME" : datatype = String  '+
',container.ContainerId AS @header : name = "ContainerId" : datatype = Object '+
',container.ParentContainerId AS @header : name = "ParentContainerId" : datatype = Object '+
',ProductBase.ProductName AS @header : name = "ProductName" : datatype = String '+
',container.Qty AS @header : name = "Qty" : datatype = Fixed '+
',container.CurrentHoldCount AS @header : name = "CurrentHoldCount" : datatype = Integer '+
',uom.UOMName AS @header : name = "UOMName" : datatype = String '+
',specBase.SpecName AS @header : name = "SpecName" : datatype = String '+
',spec.SpecId AS @header : name = "SpecId" : datatype = Object '+
',Operation.OperationName AS @header : name = "OperationName" : datatype = String '+
',WorkCenter.WorkCenterName AS @header : name = "WorkCenterName" : datatype = String  '+
',MfgOrder.MfgOrderName AS @header : name = "MfgOrderName" : datatype = String '+
',Operation.UseQueue AS @header : name = "UseQueue" : datatype = Boolean '+
',container.HoldReasonId AS @header : name = "HoldReasonId" : datatype = Object '+
',currentStatus.InProcess AS @header : name = "InProcess" : datatype = Boolean '+
'FROM Container '+
'JOIN CurrentStatus ON container.CurrentStatusId = currentStatus.CurrentStatusId  '+
'LEFT OUTER JOIN ResourceDef ON resourceDef.ResourceId = currentStatus.ResourceId '+
'LEFT OUTER JOIN Product ON container.ProductId = product.ProductId '+
'LEFT OUTER JOIN ProductBase ON ProductBase.ProductBaseId = product.ProductBaseId '+
'LEFT OUTER JOIN UOM ON uom.UOMId = container.UOMId '+
'LEFT OUTER JOIN Spec ON spec.SpecId = currentStatus.SpecId '+
'LEFT OUTER JOIN SpecBase ON specBase.SpecBaseId = spec.SpecBaseId '+
'LEFT OUTER JOIN Operation ON Operation.OperationId= spec.OperationId '+
'LEFT OUTER JOIN WorkCenter ON WorkCenter.WorkCenterId = Operation.WorkCenterId '+
'LEFT OUTER JOIN MfgOrder ON MfgOrder.MfgOrderId = container.MfgOrderId '+
'LEFT JOIN (SELECT * FROM csiGetEnumerationLabels'+
' (''ContainerStatusEnum'', $$primaryDictionary, $$secondaryDictionary)) CSE ON CSE.DefaultValue = Container.Status '+
'WHERE (WorkCenterName =?workCenterName OR OperationName=?operationName OR SpecName=?specName OR resourceDef.ResourceName=?resourceName)'

	EXEC csiSTInstall_CreateNewUserQuery 'mxContainerSearch','Container list for Operational view','OOB query for Container list in Operational view',@SQLString,0,'workCenterName',4,'operationName',4,'specName',4,'resourceName',4
 
  
END
GO
EXEC csiSTInstall_PopulateDefaultUserQueryData
GO
DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData
GO

DROP PROCEDURE csiSTInstall_CreateNewUserQuery
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_UpdateSPCRulesData
-- DESCR: Unlock SPCRule and SPCViolation objects
--
-- Copyright Siemens 2025  
IF EXISTS (SELECT 1 
           FROM sys.objects 
           WHERE object_id = OBJECT_ID(N'csiSTInstall_UpdateSPCRulesData') 
             AND type = 'P')
    DROP PROCEDURE csiSTInstall_UpdateSPCRulesData
GO
CREATE PROCEDURE csiSTInstall_UpdateSPCRulesData
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID(N'A_SPCRules', N'U') IS NOT NULL
       AND COL_LENGTH(N'A_SPCRules', N'IsFrozen') IS NOT NULL
    BEGIN
        UPDATE A_SPCRules
        SET IsFrozen = 0;
    END

    IF OBJECT_ID(N'SPCViolation', N'U') IS NOT NULL
       AND COL_LENGTH(N'SPCViolation', N'IsFrozen') IS NOT NULL
    BEGIN
        UPDATE SPCViolation
        SET IsFrozen = 0;
    END
END
GO

EXEC csiSTInstall_UpdateSPCRulesData
GO
DROP PROCEDURE csiSTInstall_UpdateSPCRulesData
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiIncreaseStringColMaxLength
-- DESCR:     Expansion of the 'XShareCollabspaceId' field in the 'Document' table for use in the 'SSO:SAM TCShare' Upload and View Document
--
-- Copyright Siemens 2025
--------------------------------------------------------------------------------
BEGIN
EXEC csiIncreaseStringColMaxLength 'DOCUMENT','XSHARECOLLABSPACEID',100
END
