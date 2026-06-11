DO $$
DECLARE
	mycount INTEGER;
	recordcount INTEGER;
	v_InstanceId VARCHAR(16);
BEGIN
	SELECT COUNT(*) INTO mycount FROM histinq;
	SELECT COUNT(*) INTO recordcount FROM HistInq WHERE HISTINQID = '000dca000000008b';
	  
	IF (mycount > 0 AND recordcount = 0) THEN
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
			UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000090' WHERE CDODEFID = 3530;
		END;
	END IF;
	
	SELECT COUNT(*) INTO mycount FROM numberingrule;
	SELECT COUNT(*) INTO recordcount FROM numberingrule WHERE NUMBERINGRULENAME = 'JobOrder_Name';
	  
	IF (mycount > 0 AND recordcount = 0) THEN
		BEGIN 
			CALL csiPRDGetNextInstanceId (7722, v_InstanceId);
			
			INSERT INTO NUMBERINGRULE (NUMBERINGRULEID, NUMBERINGRULENAME, NUMBERINGRULETYPE, PREFIX, SEQUENCELENGTH, USEALPHANUMBERICVALUE, USEHEXADECIMALVALUE,
                           USEPREFIXBASED, CHANGECOUNT, ISFROZEN, ISROLLOVER, EXCLUDEDVALUES, CDOTypeId) 
			VALUES (v_InstanceId, 'JobOrder_Name', 0, '"JobOrder-"', 6, 0, 0, 0, 1, 0, 0, 33636608, 7722);

			INSERT INTO IDControl (IDType,  NextID) VALUES (v_InstanceId, 0);
		END;
	END IF;
END;
$$;

--------------------------------------------------------------------------------
-- FUNCTION: csiIncreaseStringColMaxLength
-- DESCR:     Expansion of the 'XShareCollabspaceId' field in the 'Document' table for use in the 'SSO:SAM TCShare CollabSpaceID' Upload and View Document
--
-- Copyright Siemens 2025  
--------------------------------------------------------------------------------
DO $$
BEGIN
CALL csiIncreaseStringColMaxLength('DOCUMENT','XSHARECOLLABSPACEID',100);
END
$$;


DO 
$$
BEGIN
    CALL csiIncreaseStringColMaxLength('SPCVIOLATIONHISTORYDETAIL','VIOLATIONNAME',2000);
END;
$$;

--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_CreateNewUserQuery
-- DESCR: 
--
-- Copyright Siemens 2025  
CREATE OR REPLACE PROCEDURE csiSTInstall_NewUserQuery(pName VARCHAR(512)
                                                      ,pDescription VARCHAR(512)
                                                      ,pNotes VARCHAR(512)
                                                      ,pQueryText TEXT                                                      
                                                      ,pIsFrozen INTEGER
                                                      ,pParam1Name VARCHAR(512)
                                                      ,pParam1Type INTEGER
                                                      ,pParam2Name VARCHAR(512)
                                                      ,pParam2Type INTEGER
                                                      ,pParam3Name VARCHAR(512)
                                                      ,pParam3Type INTEGER
                                                      ,pParam4Name VARCHAR(512)
                                                      ,pParam4Type INTEGER)
LANGUAGE plpgsql
AS
$$
DECLARE
    vCDODefId       INTEGER := 7068;
    vInstanceID     VARCHAR(16);
    vCurrInstanceID VARCHAR(16);
    vParamCDODefId  INTEGER := 7071;
    vParamFieldId   INTEGER := 8597;
    vQueryTypeId    INTEGER := 4;
    vIID            VARCHAR(16);
    vCount          INTEGER;
BEGIN

    CALL csiPRDGetNextInstanceId (vCDODefId,vInstanceId);
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
       CALL csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam1Name,vInstanceID,pIsFrozen,pParam1Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,1,vInstanceID,vIID);
    END IF; 
    IF (pParam2Name IS NOT NULL) THEN   
       CALL csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam2Name,vInstanceID,pIsFrozen,pParam2Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,2,vInstanceID,vIID);
    END IF;
    IF (pParam3Name IS NOT NULL) THEN   
       CALL csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam3Name,vInstanceID,pIsFrozen,pParam3Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,3,vInstanceID,vIID);
    END IF;
    IF (pParam4Name IS NOT NULL) THEN   
       CALL csiPRDGetNextInstanceId (vParamCDODefId,vIID);
       INSERT INTO UserQueryParameter (UserQueryParameterId,CDOTypeId,UserQueryParameterName,UserQueryId,IsFrozen,DataType,ChangeCount)
       VALUES (vIID,vParamCDODefId,pParam4Name,vInstanceID,pIsFrozen,pParam4Type,0);
       
       INSERT INTO UserQueryUserQueryParameters (FieldId,Sequence,UserQueryId,UserQueryParametersId)
       VALUES (vParamFieldId,4,vInstanceID,vIID);
    END IF;
    
END;
$$;

CREATE OR REPLACE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData()
LANGUAGE plpgsql
AS
$block$
DECLARE
	v_SQLString TEXT;
  	v_userQueryExists INTEGER;
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
  ON ContainerStatusLabel.DefaultValue = cast(Container.Status as varchar)'||
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
                                 ,MIN (CASE WHEN (MaxEndTimeGMT is not null and IsStoped = 0) THEN MaxEndTimeGMT END ) as MaxEndTimeGMT
                                 ,MIN (CASE WHEN (MinEndTimeGMT is not null and MaxEndTimeGMT is null and IsStoped = 0) THEN MinEndTimeGMT END ) as MinEndTimeGMT
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
	   'LEFT JOIN ElectronicProcedureBase ON ElectronicProcedureBase.ElectronicProcedureBaseId = Spec.ElectronicProcedureBaseId '||
	   'LEFT JOIN ElectronicProcedure ON ElectronicProcedure.ElectronicProcedureBaseId = ElectronicProcedureBase.ElectronicProcedureBaseId '||
	   'WHERE (Container.Status = 1 OR Container.Status = 3) '||
	   'AND Container.ContainerName = ?ContainerName';

    CALL csiSTInstall_NewUserQuery(
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
$block$;
--#delimiter

DO
$$
BEGIN
   CALL csiSTInstall_PopulateDefaultUserQueryData();
 --  COMMIT;
END;
$$;
--#delimiter
DO
$$
BEGIN
     	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_PopulateDefaultUserQueryData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_PopulateDefaultUserQueryData;
 	END IF; 
END;
$$;

CREATE OR REPLACE PROCEDURE csiSTInstall_PopulateDefaultUserQueryData()
LANGUAGE plpgsql
AS
$block$
DECLARE
  v_SQLString TEXT;
  v_userQueryExists INTEGER;
BEGIN
  SELECT COUNT(1) INTO v_userQueryExists 
  FROM UserQuery 
  WHERE Upper(UserQueryName) = Upper('mxContainerSearch');

  IF v_userQueryExists = 0 THEN
    v_SQLString := 'SELECT  ' ||
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
    'FROM CurrentStatus ' ||
    'LEFT OUTER JOIN Container ON container.CurrentStatusId = currentStatus.CurrentStatusId  ' ||
    'LEFT OUTER JOIN ResourceDef ON resourceDef.ResourceId = currentStatus.ResourceId ' ||
    'LEFT OUTER JOIN Product ON container.ProductId = product.ProductId ' ||
    'LEFT OUTER JOIN ProductBase ON ProductBase.ProductBaseId = product.ProductBaseId ' ||
    'LEFT OUTER JOIN UOM ON uom.UOMId = container.UOMId ' ||
    'LEFT OUTER JOIN Spec ON spec.SpecId = currentStatus.SpecId ' ||
    'LEFT OUTER JOIN SpecBase ON specBase.SpecBaseId = spec.SpecBaseId ' ||
    'LEFT OUTER JOIN Operation ON Operation.OperationId= spec.OperationId ' ||
    'LEFT OUTER JOIN WorkCenter ON WorkCenter.WorkCenterId = Operation.WorkCenterId ' ||
    'LEFT OUTER JOIN MfgOrder ON MfgOrder.MfgOrderId = container.MfgOrderId ' ||
    'LEFT JOIN (SELECT * FROM csiGetEnumerationLabels(''ContainerStatusEnum'', $$primaryDictionary, $$secondaryDictionary)) CSE ON CSE.DefaultValue = cast(Container.Status as varchar) ' ||
    'WHERE (WorkCenterName =?workCenterName OR OperationName=?operationName OR SpecName=?specName OR resourceDef.ResourceName=?resourceName)';

    CALL csiSTInstall_NewUserQuery(
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
  END IF;
END;
$block$;
--#delimiter
DO $$
BEGIN
   CALL csiSTInstall_PopulateDefaultUserQueryData();
   --COMMIT;
END;
$$;
--#delimiter

DO
$$
BEGIN
	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_PopulateDefaultUserQueryData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_PopulateDefaultUserQueryData;
 	END IF; 

	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_NewUserQuery')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_NewUserQuery;
 	END IF; 
END;
$$;

--#delimiter
--------------------------------------------------------------------------------
-- PROCEDURE: csiSTInstall_UpdateSPCRulesData
-- DESCR: 
--
-- Copyright Siemens 2025 
CREATE OR REPLACE PROCEDURE csiSTInstall_UpdateSPCRulesData()
LANGUAGE plpgsql
AS
$$
DECLARE
  v_tbl_count INTEGER;
  v_col_count INTEGER;
  v_updateSql character(200);
BEGIN
  -- 1) A_SPCRules
  SELECT COUNT(*) INTO v_tbl_count
    FROM information_schema.tables
   WHERE table_name = 'A_SPCRULES';

  IF v_tbl_count > 0 THEN
    SELECT COUNT(*) INTO v_col_count
      FROM information_schema.columns
     WHERE table_name  = 'A_SPCRULES'
       AND column_name = 'ISFROZEN';

    IF v_col_count > 0 THEN
		v_updateSql := 'UPDATE A_SPCRULES SET ISFROZEN = 0';
      EXECUTE v_updateSql;
    END IF;
  END IF;

  -- 2) SPCViolation
  SELECT COUNT(*) INTO v_tbl_count
    FROM information_schema.tables
   WHERE table_name = 'SPCVIOLATION';

  IF v_tbl_count > 0 THEN
    SELECT COUNT(*) INTO v_col_count
      FROM information_schema.columns
     WHERE table_name  = 'SPCVIOLATION'
       AND column_name = 'ISFROZEN';

    IF v_col_count > 0 THEN
		v_updateSql := 'UPDATE SPCVIOLATION SET ISFROZEN = 0';
      	EXECUTE v_updateSql;
    END IF;
  END IF;

 -- COMMIT;
END;
$$;
--#delimiter
DO $$
BEGIN
   CALL csiSTInstall_UpdateSPCRulesData();
   --COMMIT;
END;
$$;
--#delimiter
DO $$
BEGIN
	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSTInstall_UpdateSPCRulesData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSTInstall_UpdateSPCRulesData;
 	END IF; 
	 
END;
$$;
