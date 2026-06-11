--------------------------------------------------------------------------------
-- SCRIPT:PopulateReleaseUpdateData.or.sql
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
--  11/25/2025 Updated User Query named 'mxContainerSearch' to return the containers based on selected Marker type 
-- Copyright Siemens 2025
 
CREATE OR REPLACE PROCEDURE populateReleaseUpdateData
AS
mycount    NUMBER;
recordcount   NUMBER;
vInstanceId VARCHAR2(16);
BEGIN
    select count(*) into mycount from histinq;
	select count(*) into recordcount from HistInq WHERE HISTINQID = '000dca000000008b';
    if mycount > 0 and recordcount = 0 then
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca000000008b', 'JobOrderName', 0, 'JobOrderName', '<Unknown Job Order>', 1, 3530);
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca000000008c', 'JobModelName', 0, 'JobModelName', '<Unknown Job Model>', 1, 3530);
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca000000008d', 'ToStageName', 0, 'ToStageName', '', 1, 3530);
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca000000008e', 'ToStageSequence', 0, 'ToStageSequence', '', 1, 3530);
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca000000008f', 'PartName', 0, 'PartDetails.Name', '<Unknown Part Name>', 1, 3530);
      insert into HISTINQ (HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca0000000090', 'RequestOrderName', 0, 'RequestOrderName', '<Unknown Request Order Name>', 1, 3530);
      insert into HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        values ('000dca0000000091','ReleaseReason',0,'Transaction::InProcessHistoryCDO.ReleaseReason.Name','<Unknown Release Reason Name>',1,3530);
    update INSTANCEIDCOUNT set CLIENTINSTANCEID = '0000000000000090' where CDODEFID = 3530;
	end if;
 
	select count(*) into mycount from numberingrule;
	select count(*) into recordcount from numberingrule where NUMBERINGRULENAME = 'JobOrder_Name';
    if mycount > 0 and recordcount = 0 then
		csiPRDGetNextInstanceId(7722,vInstanceId);
		INSERT INTO NUMBERINGRULE (NUMBERINGRULEID, NUMBERINGRULENAME, NUMBERINGRULETYPE, PREFIX, SEQUENCELENGTH, USEALPHANUMBERICVALUE, USEHEXADECIMALVALUE,
                           USEPREFIXBASED, CHANGECOUNT, ISFROZEN, ISROLLOVER, EXCLUDEDVALUES, CDOTypeId) 
		VALUES (vInstanceId, 'JobOrder_Name', 0, '"JobOrder-"', 6, 0, 0, 0, 1, 0, 0, 33636608, 7722);
 
		INSERT INTO IDControl (IDType,  NextID) 
		VALUES (vInstanceId, 0);
	end if;
	
	select count(*) into mycount from ACTIONRULE;
	select count(*) into recordcount from ACTIONRULE WHERE ACTIONRULENAME = 'IsSingleContainerRule';
	if mycount > 0 and recordcount > 0 then
		UPDATE ACTIONRULE SET EXPRESSION =  'not(IsFieldDefined("Containers", GetCurrentService())) or GetListCount(GetCurrentService().Containers) = 1 or IsFieldDefined("ServiceIsContainerTxn",GetCurrentService())' WHERE  ACTIONRULENAME = 'IsSingleContainerRule';
	end if;
	
	select count(*) into mycount from RESOURCELAYOUTDETAILS;
	select count(*) into recordcount from RESOURCELAYOUTDETAILS WHERE RESOURCEID IS NOT NULL AND MARKERTYPE IS NULL;
	if mycount > 0 and recordcount > 0 then
		UPDATE RESOURCELAYOUTDETAILS SET MARKERTYPE = 'Resource' WHERE RESOURCEID IS NOT NULL AND MARKERTYPE IS NULL;
	end if;
 
END;
/
 
BEGIN
    populateReleaseUpdateData;
    COMMIT;
END;
/
 
BEGIN    
    EXECUTE IMMEDIATE 'DROP PROCEDURE populateReleaseUpdateData';
END;
/



--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_NewDef
-- DESCR:     Helper procedure to create the summary table definition if not exists
--
-- Copyright Siemens 2024  
--------------------------------------------------------------------------------


CREATE OR REPLACE PROCEDURE csiSTInstall_NewDef(pName VARCHAR2
                                                      ,pDescription VARCHAR2
                                                      ,pNotes VARCHAR2
                                                      ,pSummarySQL VARCHAR2
                                                      ,pIsView NUMBER
                                                      ,pTableName VARCHAR2
                                                      ,pIsManuallyExecuted NUMBER
                                                      ,pScheduleDaysOfWeek VARCHAR2
                                                      ,pScheduleDaysOfMonth VARCHAR2
                                                      ,pScheduleHours VARCHAR2
                                                      ,pScheduleMonths VARCHAR2
                                                      ,pIsFrozen NUMBER
                                                      ,pIsEnabled NUMBER)
AS
    vCDODefId   NUMBER := 8236;
    vInstanceID CHAR(16);
	vSummTableDefExists NUMBER;
BEGIN
    SELECT COUNT(1) INTO vSummTableDefExists FROM SummaryTableDef WHERE SummaryTableDefName = pName;

	IF vSummTableDefExists = 0
	THEN
		csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
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
		   VALUES (vInstanceId
				  ,pName
				  ,pDescription
				  ,pNotes
				  ,pSummarySQL
				  ,pIsView
				  ,pTableName
				  ,pIsManuallyExecuted
				  ,pScheduleDaysOfWeek
				  ,pScheduleDaysOfMonth
				  ,pScheduleHours
				  ,pScheduleMonths
				  ,pIsFrozen
				  ,vCDODefId
				  ,0
				  ,pIsEnabled);
	END IF;

END;
/


--------------------------------------------------------------------------------
-- PROCEDURE: csiST_PopulateDefaultData
-- DESCR:     Calling procedure to populate summary table definition
--
-- Copyright Siemens 2024  
--------------------------------------------------------------------------------



CREATE OR REPLACE PROCEDURE csiST_PopulateDefaultData
AS
   VSQLString CLOB;
BEGIN
   
    vSQLString := 'WITH CDOData AS ( '||
    'SELECT cf.*  '||
    'FROM CDODefinition cd  '||
    'JOIN CDOFields cf ON cd.CDODefID = cf.CDODefID '||
    'WHERE cd.CDODefID = 4841613 '||
'), CombinedData AS ( '||
    'SELECT  '||
        'p.ProductId, '||
        'pb.ProductName,   '||
        'NVL(c.ContainerName, mqd.isLot) AS Container, '||
        'NVL(c.Qty, mqd.isQtyAvailable) AS AvailableQty, '||
        'mqd.isConsumedQty,  '||
        'mqd.isRemovalStrategy, '||
        'NVL(c.ExpirationDate, isExpirationDate) AS ExpirationDate, '||
        'rd.ResourceName, '||
        'NULL AS isInventoryLocationName,  '||
        'mq.isMaterialQueueName, '||
        'o.OperationName,  '||
        'uom.UOMName,  '||
        '(cdo.FieldName || '' - '' || cdo.FieldDescription) AS RemovalStrategy,  '||
        'CASE WHEN mq.isActive = 1 THEN ''Active'' ELSE ''Inactive'' END AS isActive, '||
        ' ''Queue'' AS indicator, '||
        '(SELECT b.ContainerName FROM Container b WHERE b.ContainerId = mq.isContainerObjectId) AS IssueContainer, '||
        '(SELECT mo.MfgOrderName FROM MfgOrder mo WHERE mo.MfgOrderId = mq.isMfgOrderId) AS Mfg_Order, '||
        'pt.ProductTypeName '||
    'FROM isMaterialQueueDetails mqd  '||
    'JOIN Product p ON p.ProductId = mqd.isProductId  '||
	'JOIN ProductBase pb ON pb.ProductBaseId = p.ProductBaseId  '||
    'LEFT JOIN ProductType pt ON p.ProductTypeId = pt.ProductTypeId '||
    'LEFT JOIN isMaterialQueue mq ON mq.isMaterialQueueId = mqd.isMaterialQueueId  '||
    'LEFT JOIN Container c ON c.ContainerId = mqd.isContainerId  '||
    'LEFT JOIN ResourceDef rd ON rd.ResourceId = mq.isResourceId  '||
    'LEFT JOIN Operation o ON o.OperationId = mq.isOperationId  '||
    'LEFT JOIN UOM uom ON mqd.isUOMid = uom.UOMId '|| 
    'LEFT JOIN CDOData cdo ON mqd.isRemovalStrategy = cdo.DefaultValue  '||
    'UNION ALL '||
    'SELECT  '||
        'p.ProductId, '||
        'pb.ProductName,  '||
        'NVL(c.ContainerName, id.isLot) AS Container, '||
        'NVL(c.Qty, id.isQty) AS AvailableQty, '||
        'NULL AS isConsumedQty,  '||
        'id.isRemovalStrategy, '||
        'NVL(c.ExpirationDate, isExpirationDate) AS ExpirationDate, '||
        'rd.ResourceName,  '||
        'il.isInventoryLocationName,  '||
        'NULL AS isMaterialQueueName, '||
        'NULL AS OperationName, '||
        'uom.UOMName,  '||
        '(cdo.FieldName || '' - '' || cdo.FieldDescription) AS RemovalStrategy,   '||
        'NULL AS isActive, '||
        ' ''Inventory'' AS indicator, '||
        'NULL AS IssueContainer, '|| 
        'NULL AS Mfg_Order, '||
        'pt.ProductTypeName '||
    'FROM isInventoryDetails id  '||
    'JOIN Product p ON p.ProductId = id.isProductId  '||
    'JOIN ProductBase pb ON pb.ProductBaseId = p.ProductBaseId  '||
    'LEFT JOIN ProductType pt ON p.ProductTypeId = pt.ProductTypeId  '||
    'LEFT JOIN Container c ON c.ContainerId = id.isContainerId  '||
    'LEFT JOIN isInventoryLocation il ON il.isInventoryLocationId = id.isInventoryLocationId  '||
    'LEFT JOIN ResourceDef rd ON rd.ResourceId = il.isParentResourceId '||
    'LEFT JOIN UOM uom ON id.isUOMid = uom.UOMId  '||
    'LEFT JOIN CDOData cdo ON id.isRemovalStrategy = cdo.DefaultValue '||
') '||
'SELECT  '||
    'ProductId, '||
    'ProductName, '||
    'Container, '||
    'AvailableQty, '||
    'isConsumedQty, '||
    'isRemovalStrategy, '||
    'ExpirationDate, '||
    'ResourceName, '||
    'isInventoryLocationName, '||
    'isMaterialQueueName, '||
    'OperationName, '|| 
    'UOMName, '||
    'RemovalStrategy, '||
    'isActive, '||
    'indicator, '||
    'IssueContainer, '||
    'Mfg_Order, '||
    'ProductTypeName, '||
    'CASE  '||
        'WHEN isMaterialQueueName IS NOT NULL THEN isMaterialQueueName  '||
        'ELSE isInventoryLocationName '||
   ' END AS LocationName '||
'FROM CombinedData';
    
    csiSTInstall_NewDef(
        'MATQUEUENINV', 
        'Material Queue And Inventory Details.', 
        NULL, 
        vSQLString, 
        1, 
        'MATQUEUENINV', 
        0, 
        NULL, 
        NULL, 
        '0,', 
        NULL, 
        1,
        1
    ); 

END;
/
--#delimiter

BEGIN
   csiST_PopulateDefaultData;
   COMMIT;
END;
/
--#delimiter
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiST_PopulateDefaultData';
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiSTInstall_NewDef';
    csiIncreaseStringColMaxLength('SPCVIOLATIONHISTORYDETAIL','VIOLATIONNAME',2000);
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2025  
CREATE OR REPLACE PROCEDURE csiSTInstall_NewUserQuery(pName VARCHAR2
                                                      ,pDescription VARCHAR2
                                                      ,pNotes VARCHAR2
                                                      ,pQueryText VARCHAR2                                                      
                                                      ,pIsFrozen NUMBER
                                                      ,pParam1Name VARCHAR2
                                                      ,pParam1Type NUMBER
                                                      ,pParam2Name VARCHAR2
                                                      ,pParam2Type NUMBER
                                                      ,pParam3Name VARCHAR2
                                                      ,pParam3Type NUMBER
                                                      ,pParam4Name VARCHAR2
                                                      ,pParam4Type NUMBER)
AS
    vCDODefId       NUMBER := 7068;
    vInstanceID     CHAR(16);
    vCurrInstanceID CHAR(16);
    vParamCDODefId  NUMBER := 7071;
    vParamFieldId   NUMBER := 8597;
    vQueryTypeId    NUMBER := 4;
    vIID            CHAR(16);
    vCount          NUMBER;
BEGIN

    csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
    INSERT INTO UserQuery(UserQueryId
                         ,UserQueryName
                         ,Description
                         ,Notes
                         ,QueryText
                         ,IsFrozen
                         ,CDOTypeId
                         ,QueryTypeId)
       VALUES (vInstanceId
              ,pName
              ,pDescription
              ,pNotes
              ,pQueryText
              ,pIsFrozen
              ,vCDODefId
              ,vQueryTypeId);
              
    IF (pParam1Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam1Name,vInstanceID,pIsFrozen,pParam1Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,1,vInstanceID,vIID);
    END IF; 
    IF (pParam2Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam2Name,vInstanceID,pIsFrozen,pParam2Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,2,vInstanceID,vIID);
    END IF;
    IF (pParam3Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam3Name,vInstanceID,pIsFrozen,pParam3Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,3,vInstanceID,vIID);
    END IF;
    IF (pParam4Name IS NOT NULL) THEN   
       csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam4Name,vInstanceID,pIsFrozen,pParam4Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,4,vInstanceID,vIID);
    END IF;
    
END;
/
CREATE OR REPLACE PROCEDURE csiSTInstall_NewOnlineQuerySetup(pName VARCHAR2
                                                      ,pDescription VARCHAR2
                                                      ,pNotes VARCHAR2
                                                      ,pQueryText VARCHAR2                                                      
                                                      ,pIsFrozen NUMBER
                                                      ,pParam1Name VARCHAR2
                                                      ,pParam1Type NUMBER
                                                      ,pParam1Value VARCHAR2
                                                      ,pParam2Name VARCHAR2
                                                      ,pParam2Type NUMBER
                                                      ,pParam2Value VARCHAR2
                                                      ,pParam3Name VARCHAR2
                                                      ,pParam3Type NUMBER
                                                      ,pParam3Value VARCHAR2)
AS
    vCDODefId       NUMBER := 4751422;
    vInstanceID     CHAR(16);
    vCurrInstanceID CHAR(16);
    vParamCDODefId  NUMBER := 4751770;
    vParamFieldId   NUMBER := 8597;
    vQueryTypeId    NUMBER := 4;
    vIID            CHAR(16);
    vCount          NUMBER;
BEGIN
   
    -- Only insert if the query does not yet exist.  Do not update existing instances
	BEGIN
    SELECT COUNT(*) INTO vCount FROM UserQuery WHERE UPPER(UserQueryName) = UPPER(pName);
	EXCEPTION  WHEN NO_DATA_FOUND THEN vCount := 0; 	
    END;
    
    IF (vCount <= 0) THEN    
       BEGIN
        csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
        INSERT INTO UserQuery(UserQueryId
                             ,UserQueryName
                             ,Description
                             ,Notes
                             ,QueryText
                             ,IsFrozen
                             ,CDOTypeId
                             ,QueryTypeId)
           VALUES (vInstanceId
                  ,pName
                  ,pDescription
                  ,pNotes
                  ,pQueryText
                  ,pIsFrozen
                  ,vCDODefId
                  ,vQueryTypeId);
              
        IF (pParam1Name IS NOT NULL) THEN   
           csiPRDGetNextInstanceId (vParamCDODefId,vIID);
           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (vIID,vParamCDODefId,pParam1Name,pParam1Value,vInstanceID,pIsFrozen,pParam1Type,0);
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (vParamFieldId,1,vInstanceID,vIID);
        END IF;
    
        IF (pParam2Name IS NOT NULL) THEN   
           csiPRDGetNextInstanceId (vParamCDODefId,vIID);
           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (vIID,vParamCDODefId,pParam2Name,pParam2Value,vInstanceID,pIsFrozen,pParam2Type,0);
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (vParamFieldId,2,vInstanceID,vIID);
        END IF;
    
        IF (pParam3Name IS NOT NULL) THEN   
           csiPRDGetNextInstanceId (vParamCDODefId,vIID);
           INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,DynamicValue,UserQueryId,IsFrozen,DataType,ChangeCount)
           VALUES (vIID,vParamCDODefId,pParam3Name,pParam3Value,vInstanceID,pIsFrozen,pParam3Type,0);
           --
           --INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId) VALUES (vParamFieldId,3,vInstanceID,vIID);
        END IF;
       EXCEPTION 
          WHEN OTHERS THEN NULL;
       END;
    END IF;    
END;
/

CREATE OR REPLACE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData AS
  v_SQLString NCLOB;
  v_userQueryExists NUMBER;
BEGIN
  SELECT COUNT(1) INTO v_userQueryExists 
  FROM UserQuery 
  WHERE Upper(UserQueryName) = Upper('mxEProcContainerHeader');

  IF v_userQueryExists = 0 THEN
    v_SQLString := 'SELECT Container.ContainerId AS "InstanceId",' ||
    'CurrentStatus.SpecId AS "Spec", ' ||
    'HoldReason.HoldReasonId  AS "HoldReasonId", ' || 
    'Container.CurrentHoldCount  AS "CurrentHoldCount", ' || 
    'Container.ContainerName  AS "Container", ' || 
    'ContainerLevel.ContainerLevelName  AS "Level", '|| 
    'ResourceDef.ResourceId AS "Carrier", '|| 
    'ResourceDef.ResourceName AS "CarrierName", '||
    'WorkflowStep.WorkflowStepName AS "Step", '|| 
    'PriorityCode.PriorityCodeName AS "Priority", '|| 
    'Product.ProductName AS "Product", '|| 
    'Product.ProductRevision AS "Revision", '|| 
    'Product.Description AS "ProductDescription", '|| 
	'ProductROR AS "ProductROR", '||
    'Container.Qty AS "Qty", '|| 
    'CurrentStatus.InProcess AS "InProcess", '|| 
    'ContainerStatusLabel.LabelValue AS "Status", '|| 
    'Operation.OperationName AS "Operation", '|| 
    'Operation.UseQueue AS "UseQueue", '||
	'MfgOrder.MfgOrderName AS "MfgOrder", '||
	'UOM.UOMName AS "UOM", '||
	'COALESCE(Container.DueDate, to_date(''12/31/9999'', ''mm/dd/yyyy'')) AS "DueDate", '||
	'COALESCE(Container.OriginalStartDate, to_date(''12/31/9999'', ''mm/dd/yyyy'')) AS "OriginalStartDate", '||
	'CASE COALESCE(Container.CurrentHoldCount, 0) WHEN 0 THEN 0 ELSE 1 END AS "IsOnHold", '||
	'HoldReason.HoldReasonName AS "HoldReason", '||
	'Owner.OwnerName AS "OwnerName", '||
	'COALESCE(CurrentStatus.TimersCount, 0) AS "TimersCount", '||
	'Spec.ElectronicProcedureId AS "ElectronicProcedureId", '||
	'ResDef.ResourceName AS "ResourceName", '||
	'Workflow.WorkflowRevision AS "WorkflowRevision", '||
	'WorkflowBase.WorkflowName AS "WorkflowName", '||
	'ElectronicProcedure.ElectronicProcedureRevision AS "ElectronicProcedureRevision", '||
	'ElectronicProcedureBase.ElectronicProcedureName AS "ElectronicProcedureName" '||
	'FROM Container '||
	'LEFT JOIN MfgOrder ON MfgOrder.MfgOrderId  = Container.MfgOrderId '|| 
	'LEFT JOIN Owner ON Owner.OwnerId  = Container.OwnerId '||
	'LEFT JOIN (SELECT * FROM csiGetEnumerationLabels
    (''ContainerStatusEnum'', $$primaryDictionary, $$secondaryDictionary)) ContainerStatusLabel
  ON ContainerStatusLabel.DefaultValue = Container.Status '||
  'LEFT JOIN PriorityCode ON PriorityCode.PriorityCodeId  = Container.PriorityCodeId '||
  'LEFT JOIN HoldReason ON HoldReason.HoldReasonId  = Container.HoldReasonId '||
  'LEFT JOIN (
     SELECT Product.ProductId, ProductBase.ProductName, Product.ProductRevision, Product.Description, CASE WHEN ProductBase.RevOfRcdId = Product.ProductId THEN 1 ELSE 0 END As ProductROR FROM Product LEFT JOIN ProductBase ON Product.ProductBaseId = ProductBase.ProductBaseId
     )     Product ON Product.ProductId = Container.ProductId '||
	 'LEFT JOIN UOM ON UOM.UOMId  = Container.UOMId '||
	 'JOIN      CurrentStatus ON CurrentStatus.CurrentStatusId  = Container.CurrentStatusId '||
	 'JOIN      Spec ON Spec.SpecId  = CurrentStatus.SpecId  '||
	 'JOIN      WorkflowStep ON WorkflowStep.WorkflowStepId  = CurrentStatus.WorkflowStepId '||
	 'JOIN      Operation ON Operation.OperationId  = Spec.OperationId '||
	 'JOIN      ContainerLevel ON ContainerLevel.ContainerLevelId  = Container.LevelId '||
	 'LEFT JOIN WorkCenter ON WorkCenter.WorkCenterId  = Operation.WorkCenterId '||
	 'LEFT JOIN ResourceDef ResDef ON ResDef.ResourceId = CurrentStatus.ResourceId '||
	 'LEFT JOIN ResourceDef ON CurrentStatus.CarrierId  = ResourceDef.ResourceId '||
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
   	   ) Timer on Timer.ParentId = CurrentStatus.CurrentStatusId '||
	   'LEFT JOIN Workflow ON Workflow.WorkflowId = WorkflowStep.WorkflowId '||
	   'LEFT JOIN WorkflowBase ON WorkflowBase.WorkflowBaseId = Workflow.WorkflowBaseId '||
	   'LEFT JOIN ElectronicProcedureBase ON ElectronicProcedureBase.ElectronicProcedureBaseId = Spec.ElectronicProcedureBaseId ' ||
	   'LEFT JOIN ElectronicProcedure ON ElectronicProcedure.ElectronicProcedureBaseId = ElectronicProcedureBase.ElectronicProcedureBaseId '||
	   'WHERE (Container.Status = 1 OR Container.Status = 3) '||
	   'AND Container.ContainerName = ?ContainerName';

    csiSTInstall_NewUserQuery(
      'mxEProcContainerHeader',
      'Container Header information used in Electronic procedure',
      'OOB query for container header used in E procedure',
      v_SQLString,
      0,
      'ContainerName',
      4,
      null,
      4,
      null,
      4,
      null,
      4
    );
  END IF;
END;
/
--#delimiter

BEGIN
   csiSTInstall_PopulateDefaultUserQueryData;
   COMMIT;
END;
/
--#delimiter
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE csiSTInstall_PopulateDefaultUserQueryData';
END;
/



CREATE OR REPLACE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData2 AS
  v_SQLString NCLOB;
  v_userQueryExists NUMBER;
  v_UserQueryID varchar2(20);
BEGIN
  BEGIN
		SELECT UserQueryId INTO v_UserQueryID  FROM UserQuery  WHERE Upper(UserQueryName) = Upper('mxContainerSearch');
	EXCEPTION  WHEN NO_DATA_FOUND THEN v_UserQueryID := NULL; 	
    END;
  
    v_SQLString := N'SELECT ' ||
    'container.ContainerName AS @header : name = "ContainerName" : datatype = String ' || 
    ',container.Status AS @header : name = "Status" : datatype = Integer ' ||
    ',CSE.LabelValue AS @header : name = "STATUSNAME" : datatype = String  ' ||
    ',container.ContainerId AS @header : name = "ContainerId" : datatype = Object ' ||
    ',container.ParentContainerId AS @header : name = "ParentContainerId" : datatype = Object ' ||
    ',ProductBase.ProductName AS @header : name = "ProductName" : datatype = String ' ||
    ',container.Qty AS @header : name = "Qty" : datatype = Fixed ' ||
    ',container.CurrentHoldCount AS @header : name = "CurrentHoldCount" : datatype = Integer ' ||
    ',uom.UOMName AS @header : name = "UOMName" : datatype = String ' ||
    ',specBase.SpecName AS @header : name = "SpecName" : datatype = String ' ||
    ',spec.SpecId AS @header : name = "SpecId" : datatype = Object ' ||
    ',Operation.OperationName AS @header : name = "OperationName" : datatype = String ' ||
    ',WorkCenter.WorkCenterName AS @header : name = "WorkCenterName" : datatype = String  ' ||
    ',MfgOrder.MfgOrderName AS @header : name = "MfgOrderName" : datatype = String ' ||
    ',Operation.UseQueue AS @header : name = "UseQueue" : datatype = Boolean ' ||
    ',container.HoldReasonId AS @header : name = "HoldReasonId" : datatype = Object ' ||
    ',currentStatus.InProcess AS @header : name = "InProcess" : datatype = Boolean ' ||
    'FROM Container ' ||
    'JOIN CurrentStatus ON container.CurrentStatusId = currentStatus.CurrentStatusId  ' ||
    'LEFT OUTER JOIN ResourceDef ON resourceDef.ResourceId = currentStatus.ResourceId ' ||
    'LEFT OUTER JOIN Product ON container.ProductId = product.ProductId ' ||
    'LEFT OUTER JOIN ProductBase ON ProductBase.ProductBaseId = product.ProductBaseId ' ||
    'LEFT OUTER JOIN UOM ON uom.UOMId = container.UOMId ' ||
    'LEFT OUTER JOIN Spec ON spec.SpecId = currentStatus.SpecId ' ||
    'LEFT OUTER JOIN SpecBase ON specBase.SpecBaseId = spec.SpecBaseId ' ||
    'LEFT OUTER JOIN Operation ON Operation.OperationId= spec.OperationId ' ||
    'LEFT OUTER JOIN WorkCenter ON WorkCenter.WorkCenterId = Operation.WorkCenterId ' ||
    'LEFT OUTER JOIN MfgOrder ON MfgOrder.MfgOrderId = container.MfgOrderId ' ||
    'LEFT JOIN (SELECT * FROM csiGetEnumerationLabels' ||
    ' (''ContainerStatusEnum'', $$primaryDictionary, $$secondaryDictionary)) CSE ON CSE.DefaultValue = Container.Status ' ||
    'WHERE ((?workCenterName IS NOT NULL AND WorkCenterName =?workCenterName) OR (?operationName IS NOT NULL AND OperationName=?operationName) OR (?specName IS NOT NULL AND SpecName=?specName) OR (?resourceName IS NOT NULL AND resourceDef.ResourceName=?resourceName)) ';

	IF v_UserQueryID IS NULL
    THEN
    csiSTInstall_NewUserQuery(
      'mxContainerSearch',
      'Container list for Operational view',
      'OOB query for Container list in Operational view',
      v_SQLString,
      0,
      'workCenterName',
      4,
      'operationName',
      4,
      'specName',
      4,
      'resourceName',
      4
    );
	
	 ELSE
    
        UPDATE UserQuery Set QueryText = v_SQLString
        WHERE UserQueryID = v_UserQueryID;
  END IF;


    v_SQLString := N'SELECT ' ||
'  Container.ContainerName, ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  Operation.OperationName, ' ||
'  CDODefinition.CDOName, ' ||
'  HistoryMainline.Comments ' ||
' FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' ||
'  WHERE ' ||
'  HistoryMainline.Comments  Is Not Null   ' ||
'   AND ' ||
'   HistoryMainline.ReversalStatus  =  1 ' ||
' AND ' ||
' Container.ContainerName = ?ContainerName ';

    csiSTInstall_NewOnlineQuerySetup('ExceptionComments','Comments','Comments query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  DataPointHistoryDetail.DataName, ' ||
'  DataPointHistoryDetail.DataValue, ' ||
'  DataPointHistoryDetail.LowerLimit, ' ||
'  DataPointHistoryDetail.UpperLimit, ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Operation.OperationName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  DataPointUOM.UOMName, ' ||
'  HistoryMainline.Comments ' ||
' FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   LEFT OUTER JOIN DataPointHistory ON (DataPointHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN DataPointHistoryDetail ON (DataPointHistoryDetail.DataPointHistoryId=DataPointHistory.DataPointHistoryId) ' ||
'   LEFT OUTER JOIN UOM  DataPointUOM ON (DataPointUOM.UOMId=DataPointHistoryDetail.UOMId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' ||
'  WHERE ' ||
'   DataPointHistoryDetail.DataName IS NOT NULL ' ||
' AND ' ||
' DataPointHistoryDetail.DataValue not Between DataPointHistoryDetail.LowerLimit and DataPointHistoryDetail.UpperLimit ' ||
'  and Container.ContainerName = ?ContainerName ';

    csiSTInstall_NewOnlineQuerySetup('ExceptionDataCollection','Data Collections','Data Collection query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  Quality_Event.EventName, ' ||
'  Quality_Event.ReportedDate, ' ||
'  FailureMode.Description, ' ||
'  FailureMode.FailureModeName, ' ||
'  DT_EventData.NCRFailureTypeName, ' ||
'  DT_EventData.FS_FailureSeverityName, ' ||
'  DT_EventData.ProductName, ' ||
'  CASE Quality_Event.Status WHEN 1 THEN ''Active'' WHEN 2 THEN ''Pending''  WHEN 3 THEN ''Escalated'' WHEN 4 THEN ''Void''  WHEN 5 THEN ''Closed'' WHEN 6 THEN ''Deleted'' WHEN 7 THEN ''Initiated'' WHEN 8 THEN ''InReview'' END as Status, ' ||
'  EventLot.Lot, ' ||
'  Initiator.EmployeeName, ' ||
'  EventFailure.Comments, ' ||
'  Event_QualityResolutionCode.QualityResolutionCodeName, ' ||
'  Event_NCRCauseCode.NCRCauseCodeName ' ||
'FROM ' ||
'  EventLot RIGHT OUTER JOIN (  ' ||
'  SELECT  ' ||
'ED.EventId, ' || 
'ED.EventDataId, ' || 
'ED.WorkflowName, ' || 
'ED.WorkflowRev, ' || 
'ED.OperationName, ' || 
'ED.SpecRev, ' || 
'ED.SpecName, ' || 
'ED.ResourceName, ' || 
'ED.WorkCenterName, ' || 
'ED.MaintenanceReqRev, ' || 
'ED.MaintenanceReqName, ' || 
'ED.ProductName, ' || 
'ED.ProductRev, ' || 
'ED.ContactCustomerId, ' || 
'ED.OccupationId, ' || 
'ED.ReportingCustomerId, ' || 
'ED.ReportDate, ' || 
'ED.EventDate, ' || 
'ED.DateReceived, ' || 
'ED.ProblemDescription, ' || 
'ED.SampleQuantity, ' || 
'ED.DeviceLocation, ' || 
'ED.RMANumber, ' || 
'ED.ReturnedPhoneNumber, ' || 
'ED.ReturnedContactName, ' || 
'ED.CompensateCustomerActionId, ' || 
'ED.RecallNumber, ' || 
'ED.AdverseEventId, ' || 
'ED.DeviceEvaluatedId, ' || 
'ED.DeviceAvailableId, ' || 
'ED.DeviceReturnedId, ' || 
'ED.DeviceOperatorId, ' || 
'ED.ProductProblemId, ' || 
'ED.HealthProfessionalId, ' || 
'ED.EventTypeId, ' || 
'ED.ReportFiledWithFDAId, ' || 
'ED.ReportSourceId, ' || 
'ED.DateReceivedGMT, ' || 
'ED.ReportDateGMT, ' || 
'EF.ChangeCount AS EF_ChangeCount, ' || 
'EF.FailureModeId as EF_FailureModeId, ' || 
'EF.Description AS EF_Description, ' || 
'EF.FailureTypeId as EF_FailureTypeId, ' || 
'EF.FailureSeverityId as EF_FailureSeverityId, ' || 
'EF.Comments as EF_Comments, ' ||
'FS.ChangeCount AS FS_ChangeCount, ' || 
'FS.Notes as FS_Notes, ' || 
'FS.ChangeHistoryId as FS_ChangeHistoryId, ' || 
'FS.Description AS FS_Description, ' || 
'FS.FailureSeverityName as FS_FailureSeverityName, ' || 
'FM.FailureModeName as FM_FailureModeName, ' || 
'FM.DefaultTypeId as FM_DefaultTypeId, ' || 
'FM.DefaultSeverityId, ' || 
'FM.Description AS FM_Description, ' || 
'FM.ChangeCount AS FM_ChangeCount, ' ||
'FM.Notes AS FM_Notes, ' || 
'FM.ChangeHistoryId AS FM_ChnageHistoryId, ' ||
'FT.NCRFailureTypeName ' ||
' FROM EventData ED  ' ||
'LEFT OUTER JOIN EventFailure EF ON ED.EventDataId = EF.EventDataId  ' ||
'LEFT OUTER JOIN FailureSeverity FS ON EF.FailureSeverityId = FS.FailureSeverityId  ' ||
'LEFT OUTER JOIN FailureMode FM ON EF.FailureModeId = FM.FailureModeId ' ||
'LEFT OUTER JOIN NCRFailureType FT ON EF.FailureTypeId = FT.NCRFailureTypeId ' ||
'  )  DT_EventData ON (DT_EventData.EventDataId=EventLot.EventDataId) ' ||
'   RIGHT OUTER JOIN Event  Quality_Event ON (Quality_Event.EventId=DT_EventData.EventId) ' ||
'   LEFT OUTER JOIN EventFailure ON (Quality_Event.EventDataId=EventFailure.EventDataId) ' ||
'   LEFT OUTER JOIN EventFailureCause ON (EventFailure.EventFailureId=EventFailureCause.EventFailureId) ' ||
'   LEFT OUTER JOIN NCRCauseCode  Event_NCRCauseCode ON (EventFailureCause.CauseCodeId=Event_NCRCauseCode.NCRCauseCodeId) ' ||
'   LEFT OUTER JOIN FailureMode ON (EventFailure.FailureModeId=FailureMode.FailureModeId) ' ||
'   LEFT OUTER JOIN Employee  Initiator ON (Quality_Event.InitiatorId=Initiator.EmployeeId) ' ||
'   LEFT OUTER JOIN QualityResolutionCode  Event_QualityResolutionCode ON (Event_QualityResolutionCode.QualityResolutionCodeId=Quality_Event.QualityResolutionCodeId) ' ||  
'WHERE ' ||
'( Quality_Event.InitiatorId=Initiator.EmployeeId  ) ' ||
'  AND   ' ||
'  EventLot.Lot  =  ?ContainerName ';

    csiSTInstall_NewOnlineQuerySetup('ExceptionEvents','Events','Events query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  TaskItem.TaskItemName, ' ||
'  TaskItem.ReportInstruction AS TIReportInstruction, ' ||
'  DataPointHistoryDetail.DataName, ' ||
'  DataPointHistoryDetail.DataValue, ' ||
'  (case when ' ||
' ExecuteTaskHistory.Pass = 1 then ''PASS'' when ' ||
' ExecuteTaskHistory.Pass = 0 then ''FAIL'' end) AS TaskStatus, ' ||
'  DataPointHistoryDetail.LowerLimit, ' ||
'  DataPointHistoryDetail.UpperLimit, ' ||
'  TaskListBase.TaskListName, ' ||
'  ExecuteTaskHistory.TaskListSequence, ' ||
'  TaskList.Instruction AS TLInstruction, ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  ElectronicProcedureBase2.ElectronicProcedureName, ' ||
'  Operation.OperationName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  ComputationHistory.ComputationName, ' ||
'  ComputationHistory.ResultValue, ' ||
'  CPPDataTypes.Name, ' ||
'  TaskItem_Computation.Instruction AS TIInstruction, ' ||
'  ESigMeaning.ESigMeaningName, ' ||
'  ESigHistoryDetail.SignerFullName, ' ||
'  ESigHistoryDetail.CosignerFullName, ' ||
'  HistoryMainline.Comments, ' ||
'  DataPointUOM.UOMName, ' ||
'  ExecuteTaskHistory.Sequence, ' ||
'  TaskList.ReportInstruction AS TLReportInstruction, ' ||
'  TranWorkflowBase.WorkflowName ' ||
' FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   LEFT OUTER JOIN ESigHistorySummary ON (ESigHistorySummary.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN ESigHistoryDetail ON (ESigHistoryDetail.ESigHistorySummaryId=ESigHistorySummary.ESigHistorySummaryId) ' ||
'   LEFT OUTER JOIN ESigMeaning ON (ESigMeaning.ESigMeaningId=ESigHistorySummary.MeaningId) ' ||
'   INNER JOIN ExecuteTaskHistory ON (HistoryMainline.HistoryMainlineId=ExecuteTaskHistory.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN TaskItem ON (ExecuteTaskHistory.TaskId=TaskItem.TaskItemId) ' ||
'   LEFT OUTER JOIN TaskList ON (TaskItem.TaskListId=TaskList.TaskListId) ' ||
'   LEFT OUTER JOIN TaskListBase ON (TaskList.TaskListBaseId=TaskListBase.TaskListBaseId) ' ||
'   LEFT OUTER JOIN ElectronicProcedure  ElectronicProcedure2 ON (ExecuteTaskHistory.ElectronicProcedureId=ElectronicProcedure2.ElectronicProcedureId) ' ||
'   LEFT OUTER JOIN ElectronicProcedureBase  ElectronicProcedureBase2 ON (ElectronicProcedure2.ElectronicProcedureBaseId=ElectronicProcedureBase2.ElectronicProcedureBaseId) ' ||
'   LEFT OUTER JOIN DataPointHistory ON (DataPointHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN DataPointHistoryDetail ON (DataPointHistoryDetail.DataPointHistoryId=DataPointHistory.DataPointHistoryId) ' ||
'   LEFT OUTER JOIN UOM DataPointUOM ON (DataPointUOM.UOMId=DataPointHistoryDetail.UOMId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN ComputationHistory ON (HistoryMainline.HistoryMainlineId=ComputationHistory.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN CPPDataTypes ON (ComputationHistory.ResultDataType=CPPDataTypes.DataTypeID) ' ||
'   LEFT OUTER JOIN Computation ON (ComputationHistory.ComputationId=Computation.ComputationId) ' ||
'   LEFT OUTER JOIN ComputationParamSpec ON (Computation.ComputationId=ComputationParamSpec.ParentId) ' ||
'   LEFT OUTER JOIN ComputationParamMap ON (ComputationParamSpec.ComputationParamSpecId=ComputationParamMap.ComputationVariableId) ' ||
'   LEFT OUTER JOIN TaskItem  TaskItem_Computation ON (ComputationParamMap.ParentId=TaskItem_Computation.TaskItemId) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' ||
'   LEFT OUTER JOIN Workflow  TranWorkflow ON (TranWorkflowStep.WorkflowId=TranWorkflow.WorkflowId) ' ||
'   LEFT OUTER JOIN WorkflowBase  TranWorkflowBase ON (TranWorkflowBase.WorkflowBaseId=TranWorkflow.WorkflowBaseId) ' ||
' where ' || 
' Container.ContainerName = ?ContainerName ' ||
' AND ExecuteTaskHistory.Pass = 0 ';

    csiSTInstall_NewOnlineQuerySetup('ExceptionFailedTasks','Failed Tasks','Failed Tasks query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  Operation.OperationName, ' ||
'  CDODefinition.CDOName, ' ||
'  HistoryMainline.Comments, ' ||
'  HoldReasonHistorical.HoldReasonName ' ||
'FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN HoldReleaseHistory ON (HoldReleaseHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   INNER JOIN HoldReason  HoldReasonHistorical ON (HoldReleaseHistory.HoldReasonId=HoldReasonHistorical.HoldReasonId) ' ||
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' || 
'WHERE ' ||
'  ( ' ||
'   Container.ContainerName  =  ?ContainerName ' ||
'   AND ' ||
'   HoldReasonHistorical.HoldReasonName  Is Not Null   ' ||
'   AND ' ||
'   CDODefinition.CDODefID  =  6898 ' ||
'   AND ' ||
'   HistoryMainline.ReversalStatus  =  1 ' ||
'   )';

    csiSTInstall_NewOnlineQuerySetup('ExceptionHolds','Holds','Holds query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  Operation.OperationName, ' ||
'  CDODefinition.CDOName, ' ||
'  HistoryMainline.Comments, ' ||
'  To_WorkflowStep.WorkflowStepName as ToStep, ' ||
'  HistoryMainline.ReversalStatus ' ||
'FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN MoveHistory ON (MoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   INNER JOIN WorkflowStep  To_WorkflowStep ON (MoveHistory.ToStepId=To_WorkflowStep.WorkflowStepId) ' ||
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId)  ' ||
'WHERE ' ||
'  ( ' ||
'   Container.ContainerName  =  ?ContainerName ' ||
'   AND ' ||
'   CDODefinition.CDODefID  IN  ( 6650  ) ' ||
'   AND ' ||
'   HistoryMainline.ReversalStatus  =  1 ' ||
' )';

    csiSTInstall_NewOnlineQuerySetup('ExceptionMoveNonStds','Move Non Standards','MoveNonStd query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  Container.ContainerName AS DHR_ContainerName, ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  ProductBase3.ProductName, ' ||
'  Product3.ProductRevision, ' ||
'  RemoveHistoryDetail.QtyRemoved, ' ||
'  RemovalReason.RemovalReasonName, ' ||
'  RemoveHistoryDetail.DestinationLot, ' ||
'  RemovalContainer.ContainerName AS RemovalContainer, ' ||
'  ESigHistoryDetail.SignerFullName, ' ||
'  ESigHistoryDetail.CosignerFullName, ' ||
'  ESigMeaning.ESigMeaningName, ' ||
'  Container.ContainerName, ' ||
'  RemovedQuantityUOM.UOMName ' ||
' FROM ' ||
'  ProductBase  ProductBase3 LEFT OUTER JOIN Product  Product3 ON (Product3.ProductBaseId=ProductBase3.ProductBaseId) ' ||
'   RIGHT OUTER JOIN RemoveHistoryDetail ON (Product3.ProductId=RemoveHistoryDetail.ProductId) ' ||
'   LEFT OUTER JOIN ComponentRemoveHistory ON (ComponentRemoveHistory.ComponentRemoveHistoryId=RemoveHistoryDetail.ComponentRemoveHistoryId) ' ||
'   RIGHT OUTER JOIN HistoryMainline ON (ComponentRemoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN ESigHistorySummary ON (ESigHistorySummary.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN ESigHistoryDetail ON (ESigHistoryDetail.ESigHistorySummaryId=ESigHistorySummary.ESigHistorySummaryId) ' ||
'   LEFT OUTER JOIN ESigMeaning ON (ESigMeaning.ESigMeaningId=ESigHistorySummary.MeaningId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' ||
'   LEFT OUTER JOIN Container  RemovalContainer ON (RemoveHistoryDetail.DestinationContainerId=RemovalContainer.ContainerId) ' ||
'   LEFT OUTER JOIN RemovalReason ON (RemoveHistoryDetail.RemovalReasonId=RemovalReason.RemovalReasonId) ' ||
'   LEFT OUTER JOIN UOM  RemovedQuantityUOM ON (RemoveHistoryDetail.UOMId=RemovedQuantityUOM.UOMId) ' ||
'  WHERE ' ||
'   CDODefinition.CDODefID  =  6862 ' ||
'   AND ' ||
'   ( HistoryMainline.ContainerId  = HistoryCrossRef.TrackingId  ) ' ||
' AND ' ||
' Container.ContainerName = ?ContainerName ';

    csiSTInstall_NewOnlineQuerySetup('ExceptionRemovals','Removals','Removals query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

    v_SQLString := N'SELECT ' ||
'  TranWorkflowStep.WorkflowStepName, ' ||
'  Employee.EmployeeName, ' ||
'  HistoryMainline.TxnDate, ' ||
'  Operation.OperationName, ' ||
'  CDODefinition.CDOName, ' ||
'  HistoryMainline.Comments, ' ||
'  ReworkReason.ReworkReasonName ' ||
'FROM ' ||
'  Operation RIGHT OUTER JOIN HistoryMainline ON (Operation.OperationId=HistoryMainline.OperationId) ' ||
'   INNER JOIN HistoryCrossRef ON (HistoryMainline.HistoryId=HistoryCrossRef.HistoryId and HistoryMainline.TxnId between HistoryCrossRef.StartTxnId and HistoryCrossRef.EndTxnId) ' ||
'   LEFT OUTER JOIN Container ON (HistoryCrossRef.TrackingId=Container.ContainerId) ' ||
'   LEFT OUTER JOIN MoveHistory ON (MoveHistory.HistoryMainlineId=HistoryMainline.HistoryMainlineId) ' ||
'   LEFT OUTER JOIN ReworkReason ON (MoveHistory.ReworkReasonId=ReworkReason.ReworkReasonId) ' ||
'   LEFT OUTER JOIN CDODefinition ON (CDODefinition.CDODefID=HistoryMainline.TxnType) ' ||
'   LEFT OUTER JOIN Employee ON (Employee.EmployeeId=HistoryMainline.EmployeeId) ' ||
'   LEFT OUTER JOIN WorkflowStep  TranWorkflowStep ON (TranWorkflowStep.WorkflowStepId=HistoryMainline.WorkflowStepId) ' ||
' WHERE(Container.ContainerName  =  ?ContainerName AND ReworkReason.ReworkReasonName  Is Not Null   AND HistoryMainline.ReversalStatus  =  1)';

    csiSTInstall_NewOnlineQuerySetup('ExceptionRework','Rework query for Exception Review','Rework query for Exception Review.',v_SQLString,0,'ContainerName',4,'Container.Name',NULL,4,NULL,NULL,4,NULL);

END;
/
--#delimiter

BEGIN
   csiSTInstall_PopulateDefaultUserQueryData2;
   COMMIT;
END;
/
--#delimiter
--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_UpdateSPCRulesData
-- DESCR: 
--
-- Copyright Siemens 2025 
CREATE OR REPLACE PROCEDURE csiSTInstall_UpdateSPCRulesData
AS
  v_tbl_count NUMBER;
  v_col_count NUMBER;
BEGIN
  -- 1) A_SPCRules
  SELECT COUNT(*) INTO v_tbl_count
    FROM user_tables
   WHERE table_name = 'A_SPCRULES';

  IF v_tbl_count > 0 THEN
    SELECT COUNT(*) INTO v_col_count
      FROM user_tab_columns
     WHERE table_name  = 'A_SPCRULES'
       AND column_name = 'ISFROZEN';

    IF v_col_count > 0 THEN
      EXECUTE IMMEDIATE 'UPDATE A_SPCRULES SET ISFROZEN = 0';
    END IF;
  END IF;

  -- 2) SPCViolation
  SELECT COUNT(*) INTO v_tbl_count
    FROM user_tables
   WHERE table_name = 'SPCVIOLATION';

  IF v_tbl_count > 0 THEN
    SELECT COUNT(*) INTO v_col_count
      FROM user_tab_columns
     WHERE table_name  = 'SPCVIOLATION'
       AND column_name = 'ISFROZEN';

    IF v_col_count > 0 THEN
      EXECUTE IMMEDIATE 'UPDATE SPCVIOLATION SET ISFROZEN = 0';
    END IF;
  END IF;

  COMMIT;
END;
/
--#delimiter
BEGIN
   csiSTInstall_UpdateSPCRulesData;
   COMMIT;
END;
/
--#delimiter
BEGIN
  EXECUTE IMMEDIATE 'DROP PROCEDURE csiSTInstall_UpdateSPCRulesData';
END;
/
--#delimiter
--------------------------------------------------------------------------------
-- FUNCTION: csiIncreaseStringColMaxLength
-- DESCR:     Expansion of the 'XShareCollabspaceId' field in the 'Document' table for use in the 'SSO:SAM TCShare CollabSpaceID' Upload and View Document
--
-- Copyright Siemens 2025  
--------------------------------------------------------------------------------
BEGIN
csiIncreaseStringColMaxLength('DOCUMENT','XSHARECOLLABSPACEID',100);
END;
/