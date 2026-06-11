--------------------------------------------------------------------------------
-- SCRIPT:PopulateActionData.or.sql
-- DESCR: Creates stored procedures used to create Actions and related data
--
--  Copyright Siemens 2024

--------------------------------------------------------------------------------
-- PROCEDURE: cleanupExistingSystemRecords
-- DESCR: Helper function to clean up existing system records
--
CREATE OR REPLACE PROCEDURE cleanupExistingSystemRecords
AS
BEGIN 
  --
	DELETE FROM ActionCategory ;   
	DELETE FROM UIAction ;   
  DELETE FROM UIFloatPageLocation;
	DELETE FROM UISourcePage ;   
	DELETE FROM ActionDefActionRules ;
	DELETE FROM ActionRule ;  
	DELETE FROM ActionDef ;			
	
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
CREATE OR REPLACE PROCEDURE CreateActionRule(
	pName               IN VARCHAR2, 
	pDescription			  IN VARCHAR2, 
	pExpression				  IN VARCHAR2)
AS
	vCDOTypeId	NUMBER;
	vInstanceId VARCHAR2(16);
BEGIN

  BEGIN
    SELECT ActionRuleId INTO vInstanceId FROM ActionRule WHERE ActionRuleName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN vInstanceId := NULL; 
  END;

  IF (vInstanceId IS NULL) THEN
  
    DBMS_OUTPUT.PUT_LINE('Inserting ActionRule: ' || pName);
    
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

		INSERT INTO ActionRule
			(ActionRuleId
			,ActionRuleName
			,CDOTypeId
			,ChangeCount
			,Description
			,Expression
			,IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nVARCHAR(255)
			,pExpression				-- nVARCHAR(255)
			,0);                -- bit

  ELSE
      DBMS_OUTPUT.PUT_LINE('ActionRule ' || pName || ' already exists');
  END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: CreateActionCategory
-- DESCR: Helper function to create an Action Category record
--
CREATE OR REPLACE PROCEDURE CreateActionCategory(
	pName               IN VARCHAR2, 
	pLabelName   			  IN VARCHAR2, 
	pSequence           IN NUMBER)
AS
	vCDOTypeId	NUMBER;
	vInstanceId VARCHAR2(16);
BEGIN

  BEGIN
    SELECT ActionCategoryId INTO vInstanceId FROM ActionCategory WHERE ActionCategoryName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN vInstanceId := NULL; 
  END;

  IF (vInstanceId IS NULL) THEN
  
    DBMS_OUTPUT.PUT_LINE('Inserting ActionCategory: ' || pName);
  
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

		INSERT INTO ActionCategory
			(ActionCategoryId
			,ActionCategoryName
			,CDOTypeId
			,ChangeCount
			,LabelName 
			,LabelText 
			,Sequence
			,IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName	      			-- nVARCHAR(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName         -- nVARCHAR(50)
      ,NULL
			,pSequence  				-- int
			,0);                -- bit
  
  ELSE
      DBMS_OUTPUT.PUT_LINE('ActionCategory ' || pName || ' already exists');
  END IF;
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create Action records
--
CREATE OR REPLACE PROCEDURE CreateActionDef(
	pName               IN VARCHAR2, 
	pDescription			  IN VARCHAR2, 
  pType               IN NUMBER,
	pInstanceId				  OUT VARCHAR2)
AS
	vCDOTypeId	NUMBER;
BEGIN

  BEGIN
    SELECT ActionId INTO pInstanceId FROM ActionDef WHERE ActionName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
  END;

  IF (pInstanceId IS NULL) THEN
  
      DBMS_OUTPUT.PUT_LINE('Inserting Action: ' || pName);

      SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
      csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
  
      INSERT INTO ActionDef
        (ActionId
        ,ActionName
        ,ActionType
        ,CDOTypeId
        ,ChangeCount
        ,Description
        ,IsFrozen)
        VALUES
        (pInstanceId        -- char(16)
        ,pName			      	-- nVARCHAR(30)
        ,pType      				-- int
        ,vCDOTypeId         -- int
        ,1                  -- int
        ,pDescription       -- nVARCHAR(255)
        ,0);                -- bit
  
  ELSE
        DBMS_OUTPUT.PUT_LINE('Action ' || pName || ' already exists');
  END IF;

END;
/

-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
CREATE OR REPLACE PROCEDURE CreateUIAction(
	pActionId			      IN VARCHAR2,
	pName				        IN VARCHAR2,
	pType				        IN NUMBER,
	pDescription		    IN VARCHAR2,
	pUIType				      IN VARCHAR2,
	pUIVirtualPageName	IN VARCHAR2,
	pUIPageFlowName	    IN VARCHAR2,
	pMapItem			      IN VARCHAR2,
  pPortalTabOption    IN NUMBER,
	pClearValues		    IN NUMBER,
	pServiceName		    IN VARCHAR2,
	pLabelName			    IN VARCHAR2,
	pShowButtons		    IN NUMBER, 
	pIsPrimary		      IN NUMBER, 
	pActionCategoryName	IN VARCHAR2,
	pSequence	          IN NUMBER,
  pWidth              IN NUMBER,
  pHeight             IN NUMBER,
  pInstanceId         OUT VARCHAR2)
  
AS
	vCDOTypeId	      NUMBER;	
	vUIVirtualPageId	VARCHAR2(16);
	vUIPageFlowId		  VARCHAR2(16);
	vActionCategoryId	VARCHAR2(16);
  vFloatPageLocationId VARCHAR2(16);

BEGIN
  BEGIN
    SELECT UIActionId INTO pInstanceId FROM UIAction WHERE UIActionName = pName; 
    EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
  END;

  IF (pInstanceId IS NULL) THEN
		DBMS_OUTPUT.PUT_LINE('Inserting UIAction: ' || pName);

    BEGIN
      IF LENGTH(pUIVirtualPageName) > 0 THEN
        SELECT UIVirtualPageId INTO vUIVirtualPageId from UIVirtualPage where UIVirtualPageName = pUIVirtualPageName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIVirtualPageId := NULL; 
    END;

    BEGIN
      IF LENGTH(pUIPageFlowName) > 0 THEN
        SELECT UIPageFlowId INTO vUIPageFlowId from UIPageFlow where UIPageFlowName = pUIPageFlowName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vUIPageFlowId := NULL; 
    END;
				
    BEGIN
      IF LENGTH(pActionCategoryName) > 0 THEN
        SELECT ActionCategoryId INTO vActionCategoryId from ActionCategory where ActionCategoryName = pActionCategoryName; 
      END IF;
      EXCEPTION  WHEN NO_DATA_FOUND THEN vActionCategoryId := NULL; 
    END;
				
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
		INSERT INTO UIAction
		    (ActionCategoryId
		    ,CDOTypeId
		    ,ChangeCount
		    ,ClearValues
		    ,CloseButtonOnly
		    ,DenyDataContract
		    ,ESigRequired
		    ,ExecuteOnSubmit
		    ,ForceRedirect
		    ,IsDisabled
		    ,IsFrozen
		    ,IsHidden
        ,IsPrimary
		    ,IsReturn
		    ,LabelName
		    ,LabelText
		    ,ParentId
		    ,ReloadValues
		    ,ServiceName
		    ,Sequence
		    ,ShowButtons
		    ,UIActionId
		    ,UIActionName
		    ,UIVirtualPageId
		    ,UIPageFlowId
		    ,MapItem
        ,PortalTabOption
        ,WIPMessagesRequired)
			VALUES
			(vActionCategoryId  -- char(16)
      ,vCDOTypeId         -- int
			,1                  -- int
			,pClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- int
      ,0                  -- int
			,pLabelName		    	-- nVARCHAR(30)
			,NULL     		    	-- nVARCHAR(255)
			,pActionId		  	  -- char(16)
			,0                  -- int
			,pServiceName       -- nVARCHAR(30)
      ,pSequence          -- int
			,pShowButtons	    	-- bit
			,pInstanceId        -- char(16)
			,pName              -- nVARCHAR(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId      -- char(16)
			,pMapItem           -- nVARCHAR(30)
			,pPortalTabOption		-- int
			,0);                -- int
      
    IF pWidth IS NOT NULL and pHeight IS NOT NULL THEN
        SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
        csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);
        
        UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
    END IF;
      
	ELSE
      DBMS_OUTPUT.PUT_LINE('UIAction ' || pName || ' already exists');
  END IF;
END;
/

--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
CREATE OR REPLACE PROCEDURE CreateAction(
	pName						    IN VARCHAR2,
	pActionCategoryName	IN VARCHAR2,
	pType						    IN NUMBER,
	pDescription		    IN VARCHAR2,
	pUIType					    IN VARCHAR2,
	pUIVirtualPageName	IN VARCHAR2,
	pUIPageFlowName			IN VARCHAR2,
	pClearValues		    IN NUMBER,
	pSequence				    IN NUMBER,
	pServiceName		    IN VARCHAR2,
	pLabelName			    IN VARCHAR2,
	pMapItem				    IN VARCHAR2,
  pPortalTabOption    IN NUMBER,
	pShowButtons		    IN NUMBER,
  pIsPrimary		      IN NUMBER,
  pWidth              IN NUMBER,
  pHeight             IN NUMBER)
AS
	vActionId			    VARCHAR2(16);
	vUIActionId			  VARCHAR2(16);
BEGIN
	/* It is currently unclear to me why the UIAction has a Name or Description */
	CreateActionDef(pName, pDescription, pType, vActionId);
	CreateUIAction(vActionId, pName, pType, pDescription, pUIType, pUIVirtualPageName, 
            pUIPageFlowName, pMapItem, pPortalTabOption, pClearValues, pServiceName, pLabelName,
            pShowButtons, pIsPrimary, pActionCategoryName, pSequence, pWidth, pHeight, vUIActionId);
			
	UPDATE ActionDef SET UIActionId = vUIActionId WHERE ActionId = vActionId;

END;
/


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
CREATE OR REPLACE PROCEDURE AddActionRuleToActionDef(
	pActionRule         IN VARCHAR2, 
	pAction     			  IN VARCHAR2)
AS
  vFieldId		    NUMBER;
	vActionId		    VARCHAR2(16);
  vActionRuleId   VARCHAR2(16);
	vSequence		    NUMBER;
	vCount  		    NUMBER;
	
BEGIN
	
  BEGIN
    SELECT COUNT(*) INTO vCount FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = pAction AND r.ActionRuleName = pActionRule;
    EXCEPTION  WHEN NO_DATA_FOUND THEN vCount := 0; 
  END;

  IF (vCount = 0) THEN
		DBMS_OUTPUT.PUT_LINE('Adding ActionRule ' || pActionRule || ' to Action ' || pAction);
		
    BEGIN
      SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' could not be found');
    END;
    
    BEGIN
      SELECT ActionRuleId INTO vActionRuleId FROM ActionRule WHERE ActionRuleName = pActionRule;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('ActionRule ' || pActionRule || ' could not be found');
    END;
    
    SELECT f.FieldID INTO vFieldId FROM CDOFields f 
			JOIN CDODefinition d ON d.CDODefID=f.CDODefID
			WHERE f.FieldName='ActionRules' and d.CDOName='ActionDef';
		
		SELECT COUNT(*) + 1 INTO vSequence FROM ActionDefActionRules WHERE ActionId = vActionId;
		
		INSERT INTO ActionDefActionRules 
			(ActionId
			,ActionRulesId
			,FieldId
			,Sequence)
			VALUES
			(vActionId           -- char(16)
			,vActionRuleId		 -- char(16)
			,vFieldId			 -- int
			,vSequence);         -- int
	ELSE
  		DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' has been already linked to the ActionRule ' ||  pActionRule);
	END IF;
END;
/

-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
CREATE OR REPLACE PROCEDURE AddSourcePageToActionDef(
	pVirtualPage        IN VARCHAR2, 
	pAction     			  IN VARCHAR2,
	pExportImportKey   IN VARCHAR2)
AS

  vActionId		      VARCHAR2(16);
  vCDOTypeId		    NUMBER;
  vInstanceId   		VARCHAR2(16);
  vUIVirtualPageId  VARCHAR2(16);
	vCount  		      NUMBER;
	
BEGIN
	
  BEGIN
    SELECT COUNT(*) INTO vCount FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = pAction AND v.UIVirtualPageName = pVirtualPage;
    EXCEPTION  WHEN NO_DATA_FOUND THEN vCount := 0; 
  END;

  IF (vCount = 0) THEN
  
		DBMS_OUTPUT.PUT_LINE('Adding UISourcePage ' || pVirtualPage || ' to Action ' || pAction);
		
    BEGIN	
      SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' could not be found');
    END;
    
    BEGIN
      SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pVirtualPage;
      EXCEPTION  WHEN NO_DATA_FOUND THEN DBMS_OUTPUT.PUT_LINE('UIVirtualPage ' || pVirtualPage || ' could not be found');
    END;
    
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName ='UISourcePage';
    csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

			INSERT INTO UISourcePage 
				(ActionId
				,CDOTypeId
				,ChangeCount
				,UISourcePageId
				,UIVirtualPageId
				,ExportImportKey)
				VALUES
				(vActionId           -- char(16)
				,vCDOTypeId			 -- int
				,0					 -- int
				,vInstanceId		 -- char(16)
				,vUIVirtualPageId
				,pExportImportKey);  -- char(16)
  ELSE        
      DBMS_OUTPUT.PUT_LINE('Action ' || pAction || ' has been already linked to the UISourcePage ' || pVirtualPage);  
  END	IF;		
END;
/
	



CREATE OR REPLACE PROCEDURE PopulateActionData
AS
BEGIN
-- Clean up existing system records
	cleanupExistingSystemRecords;

-- Action Categories
	createActionCategory('QualityActions','Lbl_QualityActions',1);
	createActionCategory('ShopfloorActions','Lbl_ShopfloorActions',2);
	createActionCategory('ReportingActions','Lbl_ReportingActions',3);

--- Container actions
	CreateActionRule('ContainerInProcessRule', 'Container In Process Action Rule', '(Operation.UseQueue = True and CurrentContainerStatus.InProcess = True) or Operation.UseQueue = False');
	CreateActionRule('ContainerInQueueRule', 'Container In Queue Action Rule', 'Operation.UseQueue = True and CurrentContainerStatus.InProcess = False');
	CreateActionRule('ContainerOnHoldRule', 'Container OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = True');
	CreateActionRule('ContainerNotOnHoldRule', 'Container Not OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = False');
	CreateActionRule('ContainerClosedRule', 'Container Closed Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed');
	CreateActionRule('ContainerActiveRule', 'Container Active Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active');
    CreateActionRule('ContainerActiveOrInTransitRule', 'Container In Process Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit');
	CreateActionRule('ContainerClosedOrInTransitRule', 'Container Is Closed / In Transit Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit');
	CreateActionRule('IsSingleContainerRule', 'Container Single Action Rule', 'not(IsFieldDefined("Containers", GetCurrentService())) or GetListCount(GetCurrentService().Containers) = 1 or IsFieldDefined("ServiceIsContainerTxn",GetCurrentService())');
	CreateActionRule('IsMultiContainerRule', 'Container Multi Action Rule', 'IsFieldDefined("Containers", GetCurrentService()) and GetListCount(GetCurrentService().Containers) > 1');
	CreateActionRule('ContainerIsNotChildRule', 'Container Is Not Child Rule', 'Container.Parent = null');
	CreateActionRule('ContainerNotClosedRule', 'Container Is Not Closed Rule', 'CurrentContainerStatus.Status <> Constants.ContainerStatus.Closed');

  CreateAction('MoveInPopupAction', NULL, 1, 'MoveIn...', 'UIFloatPageOpenAction', 'MoveInVP', NULL, 0, 
		1, NULL,	'Action_MoveIn_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MoveStdPopupAction', NULL, 1, 'Move...', 'UIFloatPageOpenAction', 'MoveStdVP',NULL, 0,
		2, NULL,	'Action_Move_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MoveInImmediateAction', NULL, 1, 'Move In Immediate ', 'UISubmitAction', NULL, NULL, 0,
		3, 'MoveIn',  'Action_MoveIn_Immediate', NULL, 1, 0, 1, NULL, NULL);
	CreateAction('MoveImmediateAction', NULL, 1, 'Move Immediate', 'UISubmitAction', NULL, NULL, 0,
		4, 'MoveStd', 'Action_Move_Immediate', NULL, 1, 0, 1, NULL, NULL);
	CreateAction('ContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'ContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_Hold_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('ContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'ContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_Release_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('ContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'OpenVP', NULL, 0, 
		7, NULL,	'Action_Open_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('ContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'CloseVP', NULL, 0, 
		8, NULL,	'Action_Close_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('ContainerReworkAction', NULL, 1, 'Rework...', 'UIFloatPageOpenAction', 'ReworkVP', NULL, 0, 
		9, NULL,	'Action_Rework_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('CreateProductionEventAction', NULL, 1, 'Create Production Event...', 'UIFloatPageOpenAction', 'ProductionEventRecord_VPR2', NULL, 0,
		10, 'CreateProductionEvent',  'Action_CreateProductionEvent_PopUp', NULL, 1, 0, 0, 1220, 700);
	CreateAction('MultiContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'MultiContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_MultiHold_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MultiContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'MultiContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_MultiRelease_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MultiContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'MultiContainerOpenVP', NULL, 0, 
		7, NULL,	'Action_MultiOpen_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MultiContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'MultiContainerCloseVP', NULL, 0, 
		8, NULL,	'Action_MultiClose_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MultiContainerMoveNonStdAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MultiContainerMoveNonStdVP', NULL, 0, 
		9, NULL,	'Action_MultiMoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('MoveNonStdPopupAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MoveNonStdVP', NULL, 0, 
		10, NULL,	'Action_MoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL);

				
	AddActionRuleToActionDef('ContainerActiveRule', 'MoveInPopupAction');
	AddActionRuleToActionDef('ContainerInQueueRule', 'MoveInPopupAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveInPopupAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MoveInPopupAction');
  AddActionRuleToActionDef('IsSingleContainerRule', 'MoveInPopupAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'MoveInImmediateAction');
	AddActionRuleToActionDef('ContainerInQueueRule', 'MoveInImmediateAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveInImmediateAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MoveInImmediateAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'MoveInImmediateAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'MoveStdPopupAction');
	AddActionRuleToActionDef('ContainerInProcessRule', 'MoveStdPopupAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveStdPopupAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MoveStdPopupAction');
 	AddActionRuleToActionDef('IsSingleContainerRule', 'MoveStdPopupAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'MoveImmediateAction');
	AddActionRuleToActionDef('ContainerInProcessRule', 'MoveImmediateAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveImmediateAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MoveImmediateAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'MoveImmediateAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'MoveNonStdPopupAction');
	AddActionRuleToActionDef('ContainerInProcessRule', 'MoveNonStdPopupAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveNonStdPopupAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MoveNonStdPopupAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'MoveNonStdPopupAction');

	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'ContainerHoldAction');
	AddActionRuleToActionDef('ContainerActiveRule', 'ContainerHoldAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'ContainerHoldAction');

	AddActionRuleToActionDef('ContainerOnHoldRule', 'ContainerReleaseAction');
	AddActionRuleToActionDef('ContainerActiveRule', 'ContainerReleaseAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'ContainerReleaseAction');

	AddActionRuleToActionDef('ContainerClosedOrInTransitRule', 'ContainerOpenAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'ContainerOpenAction');

	AddActionRuleToActionDef('ContainerActiveOrInTransitRule', 'ContainerCloseAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'ContainerCloseAction');
  
	AddActionRuleToActionDef('ContainerActiveRule', 'ContainerReworkAction');
	AddActionRuleToActionDef('ContainerInProcessRule', 'ContainerReworkAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'ContainerReworkAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'ContainerReworkAction');
	AddActionRuleToActionDef('IsSingleContainerRule', 'ContainerReworkAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'CreateProductionEventAction');

	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MultiContainerHoldAction');
	AddActionRuleToActionDef('ContainerActiveRule', 'MultiContainerHoldAction');
	AddActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerHoldAction');

	AddActionRuleToActionDef('ContainerOnHoldRule', 'MultiContainerReleaseAction');
	AddActionRuleToActionDef('ContainerActiveRule', 'MultiContainerReleaseAction');
	AddActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerReleaseAction');

	AddActionRuleToActionDef('ContainerClosedOrInTransitRule', 'MultiContainerOpenAction');
	AddActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerOpenAction');

	AddActionRuleToActionDef('ContainerActiveOrInTransitRule', 'MultiContainerCloseAction');
	AddActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerCloseAction');

	AddActionRuleToActionDef('ContainerActiveRule', 'MultiContainerMoveNonStdAction');
	AddActionRuleToActionDef('ContainerInProcessRule', 'MultiContainerMoveNonStdAction');
	AddActionRuleToActionDef('ContainerNotOnHoldRule', 'MultiContainerMoveNonStdAction');
	AddActionRuleToActionDef('ContainerIsNotChildRule', 'MultiContainerMoveNonStdAction');
	AddActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerMoveNonStdAction' );
 
 
 	AddSourcePageToActionDef('ContainerSearchVP', 'MoveInPopupAction','260A22C2-B4CE-4A5F-B0D5-8EBFE97CA9ED');
	AddSourcePageToActionDef('ContainerSearchVP', 'MoveStdPopupAction','24B18EA1-FDEA-4C1D-8031-6A9B90C1D420');
	AddSourcePageToActionDef('ContainerSearchVP', 'MoveInImmediateAction','6EB3D47B-33B1-48FA-B960-31464E9C25ED');
	AddSourcePageToActionDef('ContainerSearchVP', 'ContainerHoldAction','00B2384A-AAA1-4E33-A788-494E3BFEFFFD');
	AddSourcePageToActionDef('ContainerSearchVP', 'ContainerReleaseAction','C79059FB-4330-4AEA-987A-55189C46A16B');
	AddSourcePageToActionDef('ContainerSearchVP', 'ContainerOpenAction','2CB56FA4-F203-473F-90A7-4DE3C0DF44F6');
	AddSourcePageToActionDef('ContainerSearchVP', 'ContainerCloseAction','43EFCB05-9D06-4474-B761-6C854652466F');
	AddSourcePageToActionDef('ContainerSearchVP', 'ContainerReworkAction','3AC8D39C-9561-4AE6-B34A-A2287809C8C8');
	AddSourcePageToActionDef('ContainerSearchVP', 'CreateProductionEventAction','D4477E8E-6E1B-43D9-8F7B-496AA2B45477');
	AddSourcePageToActionDef('ContainerSearchVP', 'MoveNonStdPopupAction','D85A27D5-2DE5-45B4-91C6-2193916BD7E4');
	AddSourcePageToActionDef('ContainerSearchVP', 'MultiContainerHoldAction','3519F13C-B4F5-413D-881E-54889EFB0B47');
	AddSourcePageToActionDef('ContainerSearchVP', 'MultiContainerReleaseAction','CAF63050-340F-412C-B8B7-016BBB7D435A');
	AddSourcePageToActionDef('ContainerSearchVP', 'MultiContainerOpenAction','056ED1D3-704E-409C-A63F-9B94754A64DF');
	AddSourcePageToActionDef('ContainerSearchVP', 'MultiContainerCloseAction','B205F2A4-4672-41AE-B099-04F241DFAA44');
	AddSourcePageToActionDef('ContainerSearchVP', 'MultiContainerMoveNonStdAction','159B7CFA-17B5-4260-9AE0-BD3908316D2F');
		
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MoveInPopupAction','0C45506F-5793-40A4-8EB1-00CDD863939F');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MoveStdPopupAction','41447F41-EBA6-4018-A688-B1660B2CD283');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MoveInImmediateAction','431370C4-292A-4AC4-9E97-F0E75BB39733');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerHoldAction','2834C71A-5275-4559-BB84-C7E8CD81609A');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerReleaseAction','2DD7FB0A-026E-4605-A8AB-BE4C638AF103');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerOpenAction','5DFA3318-8628-472B-95A7-D04499917432');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerCloseAction','33C013FF-5395-4FE5-A33C-4B60025988D4');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerReworkAction','3D9E3BBA-6387-4D17-B7D5-2DD70D2DDEFE');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'CreateProductionEventAction','3B4FE960-2F03-470C-924E-9E7CDCF9228A');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MoveNonStdPopupAction','526EE7D5-81AC-4A68-B392-7B9BE3A63251');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerHoldAction','0F92D6B5-A45E-4B95-AED2-52FF1655CB95');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerReleaseAction','20420500-29CA-43EC-8E5F-755F7EE05440');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerOpenAction','29945AB3-B6CE-4324-9C1B-3C1699B2C8DD');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerCloseAction','A97B3311-90BB-4173-9B4E-A5C3CA20A1C5');
	AddSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerMoveNonStdAction','96BAA722-48C6-42A1-A336-956362FA9D8E');
  
	AddSourcePageToActionDef('OperationalViewVPR2', 'MoveInPopupAction','FB57C691-5FAE-4305-92FD-F240E324978C');
	AddSourcePageToActionDef('OperationalViewVPR2', 'MoveStdPopupAction','B0262728-2560-458B-BBCB-B2C5D5818770');
	AddSourcePageToActionDef('OperationalViewVPR2', 'MoveImmediateAction','B0AB31D7-E415-43A8-B837-EA88236B7B26');
	AddSourcePageToActionDef('OperationalViewVPR2', 'MoveInImmediateAction','E9CC90B6-70F6-4B83-ABB3-6ED51569D193');
  
	AddSourcePageToActionDef('OperationalViewVP', 'MoveInPopupAction','5E522399-94E9-4FFC-B267-000641FECF4D');
	AddSourcePageToActionDef('OperationalViewVP', 'MoveStdPopupAction','BCBAC511-C22E-462F-BA55-3001C0E1186F');
	AddSourcePageToActionDef('OperationalViewVP', 'MoveImmediateAction','2FA4C318-2BDB-4012-82D0-5FD805576CB1');
	AddSourcePageToActionDef('OperationalViewVP', 'MoveInImmediateAction','25AA2BFB-C522-4C5D-8F66-031972747ABF');
	
	AddSourcePageToActionDef('OperationalViewScanVP', 'MoveInPopupAction','708AF31C-C372-4178-AACF-65219E8DCFAD');
	AddSourcePageToActionDef('OperationalViewScanVP', 'MoveStdPopupAction','82C06C64-ECEC-4CE2-8109-67EB76B649D1');
	AddSourcePageToActionDef('OperationalViewScanVP', 'MoveImmediateAction','852AA642-07AC-46FE-8254-EE829AA1EC30');
	AddSourcePageToActionDef('OperationalViewScanVP', 'MoveInImmediateAction','15E73454-727D-465A-91E1-5DE07060DED3');

--- Quality Object actions
	CreateActionRule('QOPendingDeletedRule', 'QualityObject Pending-Deleted restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Pending or QualityObjectDetail.Status = Constants.QualityStatus.Deleted)');
	CreateActionRule('QOClosedRestrictRule', 'QualityObject Closed restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Closed)');
	CreateActionRule('QOInReviewRestrictRule', 'QualityObject InReview restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview)');
	CreateActionRule('QOUserHasRoleRule', 'QualityObject User Has Role Rule', 'QualityObjectDetail.UserHasRole = True');
	CreateActionRule('QOCategoryEvent', 'QualityObject Category is Event', 'QualityObjectDetail.Category = Constants.Category.Event');
	CreateActionRule('QOCategoryNonconformance', 'QualityObject Category is Nonconformance', 'QualityObjectDetail.Category = Constants.Category.Nonconformance');
	CreateActionRule('QOIsOwnerRule', 'QualityObject User Is Owner Rule', 'QualityObjectDetail.UserIsOwner = True');
	CreateActionRule('QOIsApprovalRequiredRule', 'QualityObject Is Approval Required Rule', 'QualityObjectDetail.IsApprovalRequired = True');
	CreateActionRule('QOInReviewRule', 'QualityObject In Review Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.InReview');
	CreateActionRule('QOTriageRule', 'QualityObject Triage Rule', 'QualityObjectDetail.TriageComplete = False');
	CreateActionRule('QOChecklistRule', 'QualityObject Checklist Rule', 'QualityObjectDetail.ChecklistSaved = False');
	CreateActionRule('QOReopenRule', 'QualityObject Reopen Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.Closed');
	CreateActionRule('QOResolveRule', 'QualityObject Resolve Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview and QualityObjectDetail.CompletionApprovalStatus <> Constants.ApprovalStatus.Approved)');
	CreateActionRule('QOSelectedTabLotsOrDisp', 'Affected Materials/Disposition tab for Manage Event', 'ActionSelectedTab = 1 or ActionSelectedTab = 2');
	CreateActionRule('QOSelectedTabDisp', 'Disposition tab for Manage Event', 'ActionSelectedTab = 2');

	CreateAction('QOManage', NULL, 2, 'Manage Event', 'UIPageMappingAction', 
		NULL, NULL, 0, 1, NULL, 'Action_ManageRecords', 'EventRecordView', 2, 1, 0, NULL, NULL);
	CreateAction('QOTriageBS', NULL, 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 2, NULL, 'Action_Triage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('QOApproveBS', NULL, 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 3, NULL, 'Action_Approve', NULL, 2, 1, 0, NULL, NULL);

	CreateAction('QOTriage', 'QualityActions', 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 1, NULL, 'Action_Triage', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOChangeCategory', 'QualityActions', 2, 'QualityObject Change Category', 'UIPageRedirectAction', 
		'ChangeCategoryVP', NULL, 0, 2, NULL, 'Action_ChangeCategory', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOChangeOwner', 'QualityActions', 2, 'QualityObject Change Owner', 'UIPageRedirectAction', 
		'ReassignOwnerVP', NULL, 0, 3, NULL, 'Action_Reassign', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOApprove', 'QualityActions', 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 4, NULL, 'Action_Approve', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOCancelApproval', 'QualityActions', 2, 'QualityObject Cancel Approval', 'UIPageRedirectAction', 
		'CancelApprovalSheet_VP', NULL, 0, 5, NULL, 'Action_CancelApproval', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOResolve', 'QualityActions', 2, 'QualityObject Resolve', 'UIPageRedirectAction', 
		'QualityObjectResolution_VP', NULL, 0, 6, NULL, 'Action_Resolve', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOReopen', 'QualityActions', 2, 'QualityObject Reopen', 'UIPageRedirectAction', 
		'ReOpenQOVP', NULL, 0, 7, NULL, 'Action_Reopen', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOAssignChecklist', 'QualityActions', 2, 'QualityObject Assign Checklist', 'UIPageRedirectAction', 
		'EventAssignChecklist_VP', NULL, 0, 8, NULL, 'Action_AssignChecklist', NULL, 1, 1, 0, NULL, NULL);

	CreateAction('QOHolds', 'ShopfloorActions', 2, 'QO Hold Containers', 'UIFloatPageOpenAction', 
		'MultiContainerHoldVP', NULL, 0, 1, NULL, 'Action_HoldMultiple', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOReleases', 'ShopfloorActions', 2, 'QO Release Containers', 'UIFloatPageOpenAction', 
		'MultiContainerReleaseVP', NULL, 0, 2, NULL, 'Action_ReleaseMultiple', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOMoveNonStds', 'ShopfloorActions', 2, 'QO MoveNonStd Containers', 'UIFloatPageOpenAction', 
		'MultiContainerMoveNonStdVP', NULL, 0, 3, NULL, 'Action_MoveNonStdMultiple', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOScrap', 'ShopfloorActions', 2, 'QO Scrap Container', 'UIFloatPageOpenAction', 
		'ChangeQtyVP', NULL, 0, 4, NULL, 'Action_Scrap', NULL, 1, 1, 0, NULL, NULL);
	CreateAction('QOSplit', 'ShopfloorActions', 2, 'QO Split Container', 'UIFloatPageOpenAction', 
		'SplitContainerVP', NULL, 0, 5, NULL, 'Action_Split', NULL, 1, 1, 0, NULL, NULL);
	


	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOManage');
	AddActionRuleToActionDef('QOUserHasRoleRule', 'QOManage');
	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOTriageBS');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOTriageBS');
	AddActionRuleToActionDef('QOInReviewRestrictRule', 'QOTriageBS');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOTriageBS');
	AddActionRuleToActionDef('QOTriageRule', 'QOTriageBS');
	AddActionRuleToActionDef('QOInReviewRule', 'QOApproveBS');
	AddActionRuleToActionDef('QOIsApprovalRequiredRule', 'QOApproveBS');

	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOTriage');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOTriage');
	AddActionRuleToActionDef('QOInReviewRestrictRule', 'QOTriage');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOTriage');
	AddActionRuleToActionDef('QOTriageRule', 'QOTriage');

	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOChangeCategory');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOChangeCategory');
	AddActionRuleToActionDef('QOInReviewRestrictRule', 'QOChangeCategory');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOChangeCategory');

	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOChangeOwner');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOChangeOwner');
	AddActionRuleToActionDef('QOInReviewRestrictRule', 'QOChangeOwner');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOChangeOwner');

	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOAssignChecklist');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOAssignChecklist');
	AddActionRuleToActionDef('QOInReviewRestrictRule', 'QOAssignChecklist');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOAssignChecklist');
	AddActionRuleToActionDef('QOChecklistRule', 'QOAssignChecklist');
	
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOReopen');
	AddActionRuleToActionDef('QOReopenRule', 'QOReopen');
	
	AddActionRuleToActionDef('QOInReviewRule', 'QOApprove');
	AddActionRuleToActionDef('QOIsApprovalRequiredRule', 'QOApprove');
	
	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOResolve');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOResolve');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOResolve');
	AddActionRuleToActionDef('QOResolveRule', 'QOResolve');
	
	AddActionRuleToActionDef('QOPendingDeletedRule', 'QOCancelApproval');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOCancelApproval');
	AddActionRuleToActionDef('QOInReviewRule', 'QOCancelApproval');

	
	AddActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOHolds');
	AddActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOReleases');
	AddActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOMoveNonStds');
	AddActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOScrap');
	AddActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOSplit');

	AddSourcePageToActionDef('QualitySearch_VP', 'QOManage','36D29BDF-FDCB-4681-8D86-9086C42F9568');
	AddSourcePageToActionDef('QualitySearch_VP', 'QOTriageBS','7E02CC6E-9F7A-40F6-A143-0E71758B6C20');
	AddSourcePageToActionDef('QualitySearch_VP', 'QOApproveBS','9DE57065-A5F6-4C9F-86A1-88DBDCF79DDB');

	AddSourcePageToActionDef('MessageCenterVP', 'QOManage','9D5AC9D4-84B4-4E87-BF56-A0F1FF0DCBC0');
	AddSourcePageToActionDef('MessageCenterVP', 'QOTriageBS','A8505EC9-DB95-43C6-BCBF-EBBD49E2C355');
	AddSourcePageToActionDef('MessageCenterVP', 'QOApproveBS','6CC43E67-62EA-4FFE-BD3A-3783C0518E52');

	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOTriage','92023AAE-EF4F-4413-A101-3F3C12D0B0E0');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOChangeCategory','3F021407-65AC-49E3-B523-4D58E29C11A1');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOChangeOwner','D21218CA-E7C0-42B0-8F24-8F76973FE908');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOApprove','20A47398-9022-4E2D-9ED1-316466ABF1B8');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOCancelApproval','7F473B4E-C98A-4151-AB10-2198C1DC8571');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOResolve','BE065FA6-EF96-4D42-9879-A084016BDF10');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOReopen','91266A95-4A02-4DD3-A42B-599B2EE2960A');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOAssignChecklist','79F16A5A-F52A-4D32-ADE5-D0C75D168665');
	
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOHolds','B5D7C345-D0CE-49BA-A6A6-9063228F893C');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOReleases','9C988A6A-403E-4715-980F-A198E9072F29');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOMoveNonStds','9DC296B7-F54A-4B58-9754-9CAE0527240B');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOScrap','08F62A76-5377-4081-9DC2-EE712CF723AD');
	AddSourcePageToActionDef('ProductionEventManage_VP', 'QOSplit','D12904A7-C13A-43E6-B404-24A65E977D64');
		

	AddSourcePageToActionDef('GenericEventManage_VP', 'QOTriage','D0A70FCF-E6C4-4E37-8248-78CF63944534');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOChangeCategory','8992A2F8-6DEF-40C0-903D-F941C6CAF77B');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOChangeOwner','348576F4-BBFA-4ADE-941A-D4C70CB5FAE2');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOApprove','43C80C99-4D64-4819-A124-A144C09C0ED7');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOCancelApproval','B3480C4F-66AC-47BE-B135-5F650946DCCC');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOResolve','BA38B835-BB9B-4D26-94EA-49FD07BFF674');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOReopen','A4BF3DED-4ADD-4462-B919-77128CE2C25A');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOAssignChecklist','DA50C8BE-680A-41C6-9FC9-F185C3E89F8D');

	AddSourcePageToActionDef('GenericEventManage_VP', 'QOHolds','9E60AA29-4D3E-4DCA-A4DD-C2EF2017ECA6');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOReleases','7B1163AE-F0D9-44CC-B207-832C5E94E02B');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOMoveNonStds','70D487F0-803D-4B67-8A16-BE73D47AA0DC');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOScrap','1BFAD01B-1424-4DA5-B8A4-DFADFEE19E92');
	AddSourcePageToActionDef('GenericEventManage_VP', 'QOSplit','3B245215-7058-43DB-8D3E-4B37639F5E35');
	
--- Change Management actions

	CreateActionRule('CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True');
	CreateActionRule('CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True or IsCollaborator = True');
	CreateActionRule('CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided');
	CreateActionRule('CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = True');
	CreateActionRule('CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed');
	CreateActionRule('CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers');
	CreateActionRule('CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers');
	CreateActionRule('CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed');
	CreateActionRule('CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');
	CreateActionRule('CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1');
	CreateActionRule('CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided');

	CreateAction('CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL);
  CreateAction('CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305);
	CreateAction('CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305);
	
	CreateAction('CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL);
	CreateAction('CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL);
  
    AddActionRuleToActionDef('CMIsSingleCPRule', 'CMActivatePackageAction');
	AddActionRuleToActionDef('CMActivatePackageRule', 'CMActivatePackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction');
	
	
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMCollaboratorRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsAssignApprovalRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsApprovePLMRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMCancelApprovalRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMRouteApprovalRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMPackageDetailsAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignContentAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMUpdatePackageAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMRouteApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMApprovePLMAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMCancelApprovalAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMDeployPackageAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMAlterStateAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMContentHistoryAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMClosePackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMVoidPackagePopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMActivationImpactAction');
	AddActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction');
	AddActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction');
	AddActionRuleToActionDef('CMIsSingleCPRule', 'CMOpenPackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction');
	AddActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction');
  
	
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3');
	AddSourcePageToActionDef('PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1');
		
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A');
	AddSourcePageToActionDef('PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6');
	
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933');
	AddSourcePageToActionDef('ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD');
		
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3');
	AddSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8');
	
	--Page Action for XceleratorShare
	CreateAction('QOUploadToXceleratorShare', 'QualityActions', 2, 'Quality Object Upload To Xcelerator Share', 'UIFloatPageOpenAction', 'AttachToXceleratorShare_VP',
	NULL, 0, 9, NULL, 'Action_UploadToXceleratorShare_Popup', NULL, 1, 1, 0, 650, 500);
	AddSourcePageToActionDef ('GenericEventManage_VP', 'QOUploadToXceleratorShare','FE260C7E-EB9E-4E33-BCED-A2837A45F10C');
	AddSourcePageToActionDef ('ProductionEventManage_VP', 'QOUploadToXceleratorShare','6269633E-53F0-4044-A708-95F51A82FBD2');
	AddActionRuleToActionDef('QOClosedRestrictRule', 'QOUploadToXceleratorShare');
	AddActionRuleToActionDef('QOIsOwnerRule', 'QOUploadToXceleratorShare');

END;
/

BEGIN
  PopulateActionData();
  COMMIT;
END;
/

BEGIN
EXECUTE IMMEDIATE 'DROP PROCEDURE PopulateActionData';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionRule';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionCategory';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateActionDef';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateUIAction';
EXECUTE IMMEDIATE 'DROP PROCEDURE CreateAction';
EXECUTE IMMEDIATE 'DROP PROCEDURE AddActionRuleToActionDef';
EXECUTE IMMEDIATE 'DROP PROCEDURE AddSourcePageToActionDef';
EXECUTE IMMEDIATE 'DROP PROCEDURE cleanupExistingSystemRecords';
END;
/

