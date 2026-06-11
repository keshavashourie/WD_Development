--------------------------------------------------------------------------------
-- SCRIPT:PopulateActionsDefaultData.sql
-- DESCR: Creates stored procedures used to create ActionDefs, ActionRules and UIActions
--        and then uses those stored procedures to populate the default data
--
-- Copyright Siemens 2024

--------------------------------------------------------------------------------
-- PROCEDURE: createActionRule
-- DESCR: Helper function to create an Action Rule record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('cleanupExistingSystemRecords')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS cleanupExistingSystemRecords;
 	END IF;
END $$;
CREATE PROCEDURE cleanupExistingSystemRecords()
LANGUAGE plpgsql
AS $$
BEGIN 

	-- Clean up existing system records
	DELETE FROM ActionCategory; 
	DELETE FROM UIAction;
	DELETE FROM UIFloatPageLocation;
	DELETE FROM UISourcePage; 
	DELETE FROM ActionDefActionRules;
	DELETE FROM ActionRule;
	DELETE FROM ActionDef;		
	
END $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionRule')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionRule;
 	END IF;
END $$;
CREATE PROCEDURE createActionRule(
	pName				varchar(30),
	pDescription		varchar(255),
	pExpression			varchar(1000))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId	integer;
	vInstanceId varchar(16);
BEGIN

	IF NOT EXISTS (SELECT * FROM ActionRule WHERE ActionRuleName = pName) THEN
	BEGIN
		RAISE NOTICE 'Inserting ActionRule: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionRule';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
		
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
			,pName				-- nvarchar(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nvarchar(255)
			,pExpression		-- nvarchar(255)
			,0);                -- bit
	END;
	ELSE
		RAISE NOTICE 'ActionRule % already exists', pName;
	END IF;

END $$;


--------------------------------------------------------------------------------
-- PROCEDURE: createActionCategory
-- DESCR: Helper function to create an Action Category record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionCategory')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionCategory;
 	END IF;
END $$;
CREATE PROCEDURE createActionCategory(
	pName				varchar(30),
	pLabelName			varchar(50),
	pSequence			integer)
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId	integer;
	vInstanceId varchar(16);
BEGIN

	IF NOT EXISTS (SELECT * FROM ActionCategory WHERE ActionCategoryName = pName) THEN
	BEGIN
		RAISE NOTICE 'Inserting ActionCategory: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionCategory';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);
		
		INSERT INTO ActionCategory
			(ActionCategoryId, 
			ActionCategoryName, 
			CDOTypeId, 
			ChangeCount, 
			LabelName, 
			LabelText, 
			Sequence,
			IsFrozen)
			VALUES
			(vInstanceId        -- char(16)
			,pName				-- nvarchar(30)
			,vCDOTypeId         -- int
			,1                  -- int
			,pLabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,pSequence			-- int
			,0);				-- bit
	END;
	ELSE
		RAISE NOTICE 'ActionCategory % already exists', pName;
	END IF;

END $$;


-----------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create ActionDef record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createActionDef;
 	END IF;
END $$;
CREATE PROCEDURE createActionDef(
	pName				varchar(30),
	pDescription		varchar(255),
	pType				integer, 
    out pInstanceId 	varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId	integer;
BEGIN
    
	SELECT ActionId INTO pInstanceId FROM ActionDef WHERE ActionName = pName;
	
	IF (pInstanceId IS NULL) THEN
	BEGIN
		RAISE NOTICE 'Inserting Action: %', pName;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'ActionDef';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			,pName				-- nvarchar(30)
			,pType				-- int
			,vCDOTypeId         -- int
			,1                  -- int
			,pDescription       -- nvarchar(255)
			,0);                -- bit

	END;
	ELSE
		RAISE NOTICE 'Action % already exists', pName;
	END IF;

END $$;

	
-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createUIAction')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createUIAction;
 	END IF;
END $$;
CREATE PROCEDURE createUIAction(
	pActionId			char(16),
	pName				varchar(30),
	pType				integer,
	pDescription		varchar(255),
	pUIType				varchar(30),
	pUIVirtualPageName	varchar(30),
	pUIPageFlowName	    varchar(30),
	pMapItem		    varchar(30),
	pPortalTabOption	integer,
	pClearValues		integer,
	pServiceName		varchar(30),
	pLabelName			varchar(66),
	pShowButtons		integer,
	pIsPrimary          integer,
	pActionCategoryName	varchar(30),
	pSequence			integer,
	pWidth				integer,
	pHeight				integer,
	pForceRedirect		integer,
	OUT pInstanceId varchar(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vCDOTypeId				integer;	
	vUIVirtualPageId		varchar(16);
	vUIPageFlowId			varchar(16);
	vActionCategoryId		varchar(16);
	vFloatPageLocationId	varchar(16);
BEGIN    

	SELECT UIActionId INTO pInstanceId FROM UIAction WHERE UIActionName = pName ;

	IF (pInstanceId IS NULL) THEN
	BEGIN
		RAISE NOTICE 'Inserting UIAction: %', pName;

		vUIVirtualPageId := NULL;		
		IF COALESCE(pUIVirtualPageName, '') <> '' THEN
			SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pUIVirtualPageName;  
		END IF;
		
		vUIPageFlowId := NULL;
		IF COALESCE(pUIPageFlowName,'') <> '' THEN
			SELECT UIPageFlowId INTO vUIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = pUIPageFlowName;  
		END IF;
		
		vActionCategoryId := NULL;
		IF COALESCE(pActionCategoryName,'') <> '' THEN
			SELECT ActionCategoryId INTO vActionCategoryId FROM ActionCategory WHERE ActionCategoryName = pActionCategoryName;  
		END IF;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = pUIType;
		CALL csiPRDGetNextInstanceId(vCDOTypeId, pInstanceId);
				
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
			,pForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,pIsPrimary         -- bit
			,0                  -- int
			,pLabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,pActionId			-- char(16)
			,0                  -- int
			,pServiceName       -- nvarchar(30)
			,pSequence			-- int
			,pShowButtons		-- bit
			,pInstanceId        -- char(16)
			,pName              -- nvarchar(30)
			,vUIVirtualPageId   -- char(16)
			,vUIPageFlowId		-- char(16)
			,pMapItem			-- nvarchar(30)
			,pPortalTabOption	-- int
			,0);				-- int  
			
		IF (pWidth IS NOT NULL) AND (pHeight IS NOT NULL) THEN
		BEGIN
			SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
			CALL csiPRDGetNextInstanceId(vCDOTypeId, vFloatPageLocationId);

			INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
			VALUES (vFloatPageLocationId, vCDOTypeId, 1, 0, pInstanceId, pWidth, pHeight);

			UPDATE UIAction SET FrameLocationId = vFloatPageLocationId WHERE UIActionId = pInstanceId;
		END;
		END IF;
	END;
	ELSE
		RAISE NOTICE 'UIAction % already exists', pName;
	END IF;

END $$;

	
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('createAction')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS createAction;
 	END IF;
END $$;
CREATE PROCEDURE createAction(
	pName				varchar(30),
	pActionCategoryName	varchar(30),
	pType				integer,
	pDescription		varchar(255),
	pUIType				varchar(30),
	pUIVirtualPageName	varchar(30),
	pUIPageFlowName	    varchar(30),
	pClearValues		integer,
	pSequence			integer,
	pServiceName		varchar(30),
	pLabelName			varchar(66),
	pMapItem			varchar(30),
	pPortalTabOption	integer,
	pShowButtons		integer,
	pIsPrimary          integer,
	pWidth				integer,
	pHeight				integer,
	pForceRedirect		integer)
LANGUAGE plpgsql
AS $$
DECLARE
	vActionId			varchar(16);
	vUIActionId			varchar(16);
BEGIN
    
	/* It is currently unclear to me why the UIAction has a Name or Description */
	CALL CreateActionDef(pName, pDescription, pType, vActionId);
	CALL createUIAction(vActionId,pName,pType,pDescription,pUIType,pUIVirtualPageName,pUIPageFlowName, pMapItem,
		pPortalTabOption, pClearValues, pServiceName,pLabelName,pShowButtons,pIsPrimary,pActionCategoryName,pSequence,pWidth,pHeight,pForceRedirect,
		vUIActionId);
			
	UPDATE ActionDef SET UIActionId = vUIActionId WHERE ActionId=vActionId;

END $$;


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('addActionRuleToActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS addActionRuleToActionDef;
 	END IF;
END $$;
CREATE PROCEDURE addActionRuleToActionDef(
	pActionRule			varchar(30), 
	pAction				varchar(30))
LANGUAGE plpgsql
AS $$
DECLARE
	vFieldId		integer;
	vActionId		varchar(16);
	vActionRuleId	varchar(16);
	vSequence		integer;
BEGIN
	
	IF NOT EXISTS (	SELECT * FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = pAction AND r.ActionRuleName = pActionRule
				   ) THEN
	BEGIN
		RAISE NOTICE 'Adding ActionRule % to Action %', pActionRule, pAction;
		
		SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
		SELECT ActionRuleId INTO vActionRuleId FROM ActionRule WHERE ActionRuleName = pActionRule;

		IF vActionId IS NULL THEN
		BEGIN
			RAISE NOTICE 'Action % could not be found', pAction;
			RETURN;
		END;
		END IF;
		
		IF vActionRuleId IS NULL THEN
		BEGIN
			RAISE NOTICE 'ActionRule % could not be found', pActionRule;
			RETURN;
		END;
		END IF;
		
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
	END;
	ELSE
		RAISE NOTICE 'Action % has been already linked to the ActionRule %', pAction, pActionRule;
	END IF;

END $$;


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('addSourcePageToActionDef')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS addSourcePageToActionDef;
 	END IF;
END $$;
CREATE PROCEDURE addSourcePageToActionDef(
	pVirtualPage	varchar(30),
	pAction			varchar(30),
	pExportImportKey varchar(36))
LANGUAGE plpgsql
AS $$
DECLARE
	vActionId		varchar(16);
	vActionRuleId	varchar(16);
	vCDOTypeId		integer;
	vInstanceId		varchar(16);
	vUIVirtualPageId varchar(16);
BEGIN
    
	IF NOT EXISTS (	SELECT * FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = pAction AND v.UIVirtualPageName = pVirtualPage
				   ) THEN
	BEGIN
		RAISE NOTICE 'Adding UISourcePage % to Action %', pVirtualPage, pAction;
		
		SELECT ActionId INTO vActionId FROM ActionDef WHERE ActionName = pAction;
		SELECT UIVirtualPageId INTO vUIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = pVirtualPage;  

		IF vActionId IS NULL THEN
		BEGIN
			RAISE NOTICE 'Action % could not be found', pAction;
			RETURN;
		END;
		END IF;
		
		IF vUIVirtualPageId IS NULL THEN
		BEGIN
			RAISE NOTICE 'UIVirtualPage % could not be found', pVirtualPage;
			RETURN;
		END;
		END IF;
		
		SELECT CDODefId INTO vCDOTypeId FROM CDODefinition WHERE CDOName ='UISourcePage';
		CALL csiPRDGetNextInstanceId(vCDOTypeId, vInstanceId);

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
				
	END;
	ELSE
		RAISE NOTICE 'Action % has been already linked to the UISourcePage %', pAction, pVirtualPage;
	END IF;

END $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('populateActionData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS populateActionData;
 	END IF;
END $$;
CREATE PROCEDURE populateActionData()
LANGUAGE plpgsql
AS $$
BEGIN
    
-- Clean up existing system records
	CALL cleanupExistingSystemRecords();
	
	
-- Action Categories
	CALL createActionCategory('QualityActions','Lbl_QualityActions',1);
	CALL createActionCategory('ShopfloorActions','Lbl_ShopfloorActions',2);
	CALL createActionCategory('ReportingActions','Lbl_ReportingActions',3);

--- Container actions
	CALL createActionRule('ContainerInProcessRule', 'Container In Process Action Rule', '(Operation.UseQueue = True and CurrentContainerStatus.InProcess = True) or Operation.UseQueue = False');
	CALL createActionRule('ContainerInQueueRule', 'Container In Queue Action Rule', 'Operation.UseQueue = True and CurrentContainerStatus.InProcess = False');
	CALL createActionRule('ContainerOnHoldRule', 'Container OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = True');
	CALL createActionRule('ContainerNotOnHoldRule', 'Container Not OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = False');
	CALL createActionRule('ContainerClosedRule', 'Container Closed Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed');
	CALL createActionRule('ContainerActiveRule', 'Container Active Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active');
	CALL createActionRule('ContainerActiveOrInTransitRule', 'Container In Process Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit');
	CALL createActionRule('ContainerClosedOrInTransitRule', 'Container Is Closed / In Transit Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit');
	CALL createActionRule('IsSingleContainerRule', 'Container Single Action Rule', 'not(IsFieldDefined("Containers", GetCurrentService())) or GetListCount(GetCurrentService().Containers) = 1');
	CALL createActionRule('IsMultiContainerRule', 'Container Multi Action Rule', 'IsFieldDefined("Containers", GetCurrentService()) and GetListCount(GetCurrentService().Containers) > 1');
	CALL createActionRule('ContainerIsNotChildRule', 'Container Is Not Child Rule', 'Container.Parent = null');
	CALL createActionRule('ContainerNotClosedRule', 'Container Is Not Closed Rule', 'CurrentContainerStatus.Status <> Constants.ContainerStatus.Closed');
	
	---[AMB] NOTE: the labels may not be correct for Container Search page.  may need new/separate actions for Container Search page.
	CALL createAction('MoveInPopupAction', NULL, 1, 'MoveIn...', 'UIFloatPageOpenAction', 'MoveInVP', NULL, 0, 
		1, NULL,	'Action_MoveIn_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MoveStdPopupAction', NULL, 1, 'Move...', 'UIFloatPageOpenAction', 'MoveStdVP',NULL, 0,
		2, NULL,	'Action_Move_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MoveInImmediateAction', NULL, 1, 'Move In Immediate ', 'UISubmitAction', NULL, NULL, 0,
		3, 'MoveIn',  'Action_MoveIn_Immediate', NULL, 1, 0, 1, NULL, NULL, 0);
	CALL createAction('MoveImmediateAction', NULL, 1, 'Move Immediate', 'UISubmitAction', NULL, NULL, 0,
		4, 'MoveStd', 'Action_Move_Immediate', NULL, 1, 0, 1, NULL, NULL, 0);
	CALL createAction('ContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'ContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_Hold_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('ContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'ContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_Release_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('ContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'OpenVP', NULL, 0, 
		7, NULL,	'Action_Open_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('ContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'CloseVP', NULL, 0, 
		8, NULL,	'Action_Close_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('ContainerReworkAction', NULL, 1, 'Rework...', 'UIFloatPageOpenAction', 'ReworkVP', NULL, 0, 
		9, NULL,	'Action_Rework_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('CreateProductionEventAction', NULL, 1, 'Create Production Event...', 'UIFloatPageOpenAction', 'ProductionEventRecord_VPR2', NULL, 0,
		10, 'CreateProductionEvent',  'Action_CreateProductionEvent_PopUp', NULL, 1, 0, 0, 1220, 700, 0);
	CALL createAction('MultiContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'MultiContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_MultiHold_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MultiContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'MultiContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_MultiRelease_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MultiContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'MultiContainerOpenVP', NULL, 0, 
		7, NULL,	'Action_MultiOpen_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MultiContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'MultiContainerCloseVP', NULL, 0, 
		8, NULL,	'Action_MultiClose_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MultiContainerMoveNonStdAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MultiContainerMoveNonStdVP', NULL, 0, 
		9, NULL,	'Action_MultiMoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('MoveNonStdPopupAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MoveNonStdVP', NULL, 0, 
		10, NULL,	'Action_MoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL, 0);


	CALL addActionRuleToActionDef('ContainerActiveRule', 'MoveInPopupAction');
	CALL addActionRuleToActionDef('ContainerInQueueRule', 'MoveInPopupAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveInPopupAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MoveInPopupAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'MoveInPopupAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'MoveInImmediateAction');
	CALL addActionRuleToActionDef('ContainerInQueueRule', 'MoveInImmediateAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveInImmediateAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MoveInImmediateAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'MoveInImmediateAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'MoveImmediateAction');
	CALL addActionRuleToActionDef('ContainerInProcessRule', 'MoveImmediateAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveImmediateAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MoveImmediateAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'MoveImmediateAction');
 
	CALL addActionRuleToActionDef('ContainerActiveRule', 'MoveStdPopupAction');
	CALL addActionRuleToActionDef('ContainerInProcessRule', 'MoveStdPopupAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveStdPopupAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MoveStdPopupAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'MoveStdPopupAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'MoveNonStdPopupAction');
	CALL addActionRuleToActionDef('ContainerInProcessRule', 'MoveNonStdPopupAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MoveNonStdPopupAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MoveNonStdPopupAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'MoveNonStdPopupAction');

	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'ContainerHoldAction');
	CALL addActionRuleToActionDef('ContainerActiveRule', 'ContainerHoldAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'ContainerHoldAction');

	CALL addActionRuleToActionDef('ContainerOnHoldRule', 'ContainerReleaseAction');
	CALL addActionRuleToActionDef('ContainerActiveRule', 'ContainerReleaseAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'ContainerReleaseAction');

	CALL addActionRuleToActionDef('ContainerClosedOrInTransitRule', 'ContainerOpenAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'ContainerOpenAction');

	CALL addActionRuleToActionDef('ContainerActiveOrInTransitRule', 'ContainerCloseAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'ContainerCloseAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'ContainerReworkAction');
	CALL addActionRuleToActionDef('ContainerInProcessRule', 'ContainerReworkAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'ContainerReworkAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'ContainerReworkAction');
	CALL addActionRuleToActionDef('IsSingleContainerRule', 'ContainerReworkAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'CreateProductionEventAction');

	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MultiContainerHoldAction');
	CALL addActionRuleToActionDef('ContainerActiveRule', 'MultiContainerHoldAction');
	CALL addActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerHoldAction');

	CALL addActionRuleToActionDef('ContainerOnHoldRule', 'MultiContainerReleaseAction');
	CALL addActionRuleToActionDef('ContainerActiveRule', 'MultiContainerReleaseAction');
	CALL addActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerReleaseAction');

	CALL addActionRuleToActionDef('ContainerClosedOrInTransitRule', 'MultiContainerOpenAction');
	CALL addActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerOpenAction');

	CALL addActionRuleToActionDef('ContainerActiveOrInTransitRule', 'MultiContainerCloseAction');
	CALL addActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerCloseAction');

	CALL addActionRuleToActionDef('ContainerActiveRule', 'MultiContainerMoveNonStdAction');
	CALL addActionRuleToActionDef('ContainerInProcessRule', 'MultiContainerMoveNonStdAction');
	CALL addActionRuleToActionDef('ContainerNotOnHoldRule', 'MultiContainerMoveNonStdAction');
	CALL addActionRuleToActionDef('ContainerIsNotChildRule', 'MultiContainerMoveNonStdAction');
	CALL addActionRuleToActionDef('IsMultiContainerRule', 'MultiContainerMoveNonStdAction' );

 
 	CALL addSourcePageToActionDef('ContainerSearchVP', 'MoveInPopupAction','260A22C2-B4CE-4A5F-B0D5-8EBFE97CA9ED');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MoveStdPopupAction','24B18EA1-FDEA-4C1D-8031-6A9B90C1D420');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MoveInImmediateAction','6EB3D47B-33B1-48FA-B960-31464E9C25ED');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'ContainerHoldAction','00B2384A-AAA1-4E33-A788-494E3BFEFFFD');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'ContainerReleaseAction','C79059FB-4330-4AEA-987A-55189C46A16B');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'ContainerOpenAction','2CB56FA4-F203-473F-90A7-4DE3C0DF44F6');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'ContainerCloseAction','43EFCB05-9D06-4474-B761-6C854652466F');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'ContainerReworkAction','3AC8D39C-9561-4AE6-B34A-A2287809C8C8');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'CreateProductionEventAction','D4477E8E-6E1B-43D9-8F7B-496AA2B45477');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MoveNonStdPopupAction','D85A27D5-2DE5-45B4-91C6-2193916BD7E4');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MultiContainerHoldAction','3519F13C-B4F5-413D-881E-54889EFB0B47');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MultiContainerReleaseAction','CAF63050-340F-412C-B8B7-016BBB7D435A');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MultiContainerOpenAction','056ED1D3-704E-409C-A63F-9B94754A64DF');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MultiContainerCloseAction','B205F2A4-4672-41AE-B099-04F241DFAA44');
	CALL addSourcePageToActionDef('ContainerSearchVP', 'MultiContainerMoveNonStdAction','159B7CFA-17B5-4260-9AE0-BD3908316D2F');
	
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MoveInPopupAction','0C45506F-5793-40A4-8EB1-00CDD863939F');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MoveStdPopupAction','41447F41-EBA6-4018-A688-B1660B2CD283');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MoveInImmediateAction','431370C4-292A-4AC4-9E97-F0E75BB39733');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerHoldAction','2834C71A-5275-4559-BB84-C7E8CD81609A');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerReleaseAction','2DD7FB0A-026E-4605-A8AB-BE4C638AF103');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerOpenAction','5DFA3318-8628-472B-95A7-D04499917432');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerCloseAction','33C013FF-5395-4FE5-A33C-4B60025988D4');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'ContainerReworkAction','3D9E3BBA-6387-4D17-B7D5-2DD70D2DDEFE');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'CreateProductionEventAction','3B4FE960-2F03-470C-924E-9E7CDCF9228A');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MoveNonStdPopupAction','526EE7D5-81AC-4A68-B392-7B9BE3A63251');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerHoldAction','0F92D6B5-A45E-4B95-AED2-52FF1655CB95');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerReleaseAction','20420500-29CA-43EC-8E5F-755F7EE05440');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerOpenAction','29945AB3-B6CE-4324-9C1B-3C1699B2C8DD');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerCloseAction','A97B3311-90BB-4173-9B4E-A5C3CA20A1C5');
	CALL addSourcePageToActionDef('ContainerSearchVP_R2', 'MultiContainerMoveNonStdAction','96BAA722-48C6-42A1-A336-956362FA9D8E');
  
	CALL addSourcePageToActionDef('OperationalViewVP', 'MoveInPopupAction','5E522399-94E9-4FFC-B267-000641FECF4D');
	CALL addSourcePageToActionDef('OperationalViewVP', 'MoveStdPopupAction','BCBAC511-C22E-462F-BA55-3001C0E1186F');
	CALL addSourcePageToActionDef('OperationalViewVP', 'MoveImmediateAction','2FA4C318-2BDB-4012-82D0-5FD805576CB1');
	CALL addSourcePageToActionDef('OperationalViewVP', 'MoveInImmediateAction','25AA2BFB-C522-4C5D-8F66-031972747ABF');
	
	CALL addSourcePageToActionDef('OperationalViewVPR2', 'MoveInPopupAction','FB57C691-5FAE-4305-92FD-F240E324978C');
	CALL addSourcePageToActionDef('OperationalViewVPR2', 'MoveStdPopupAction','B0262728-2560-458B-BBCB-B2C5D5818770');
	CALL addSourcePageToActionDef('OperationalViewVPR2', 'MoveImmediateAction','B0AB31D7-E415-43A8-B837-EA88236B7B26');
	CALL addSourcePageToActionDef('OperationalViewVPR2', 'MoveInImmediateAction','E9CC90B6-70F6-4B83-ABB3-6ED51569D193');

	CALL addSourcePageToActionDef('OperationalViewScanVP', 'MoveInPopupAction','708AF31C-C372-4178-AACF-65219E8DCFAD');
	CALL addSourcePageToActionDef('OperationalViewScanVP', 'MoveStdPopupAction','82C06C64-ECEC-4CE2-8109-67EB76B649D1');
	CALL addSourcePageToActionDef('OperationalViewScanVP', 'MoveImmediateAction','852AA642-07AC-46FE-8254-EE829AA1EC30');
	CALL addSourcePageToActionDef('OperationalViewScanVP', 'MoveInImmediateAction','15E73454-727D-465A-91E1-5DE07060DED3');

--- Quality Object actions
	CALL createActionRule('QOPendingDeletedRule', 'QualityObject Pending-Deleted restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Pending or QualityObjectDetail.Status = Constants.QualityStatus.Deleted)');
	CALL createActionRule('QOClosedRestrictRule', 'QualityObject Closed restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Closed)');
	CALL createActionRule('QOInReviewRestrictRule', 'QualityObject InReview restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview)');
	CALL createActionRule('QOUserHasRoleRule', 'QualityObject User Has Role Rule', 'QualityObjectDetail.UserHasRole = True');
	CALL createActionRule('QOCategoryEvent', 'QualityObject Category is Event', 'QualityObjectDetail.Category = Constants.Category.Event');
	CALL createActionRule('QOCategoryNonconformance', 'QualityObject Category is Nonconformance', 'QualityObjectDetail.Category = Constants.Category.Nonconformance');
	CALL createActionRule('QOIsOwnerRule', 'QualityObject User Is Owner Rule', 'QualityObjectDetail.UserIsOwner = True');
	CALL createActionRule('QOIsApprovalRequiredRule', 'QualityObject Is Approval Required Rule', 'QualityObjectDetail.IsApprovalRequired = True');
	CALL createActionRule('QOInReviewRule', 'QualityObject In Review Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.InReview');
	CALL createActionRule('QOTriageRule', 'QualityObject Triage Rule', 'QualityObjectDetail.TriageComplete = False');
	CALL createActionRule('QOChecklistRule', 'QualityObject Checklist Rule', 'QualityObjectDetail.ChecklistSaved = False');
	CALL createActionRule('QOReopenRule', 'QualityObject Reopen Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.Closed');
	CALL createActionRule('QOResolveRule', 'QualityObject Resolve Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview and QualityObjectDetail.CompletionApprovalStatus <> Constants.ApprovalStatus.Approved)');
	CALL createActionRule('QOSelectedTabLotsOrDisp', 'Affected Materials tab for Manage Event', 'ActionSelectedTab = 1 or ActionSelectedTab = 2');
	CALL createActionRule('QOSelectedTabDisp', 'Disposition tab for Manage Event', 'ActionSelectedTab = 2');

	CALL createAction('QOManage', NULL, 2, 'Manage Event', 'UIPageMappingAction', 
		NULL, NULL, 0, 1, NULL, 'Action_ManageRecords', 'EventRecordView', 2, 1, 0, NULL, NULL, 1);
	CALL createAction('QOTriageBS', NULL, 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 2, NULL, 'Action_Triage', NULL, 2, 1, 0, NULL, NULL, 1);
	CALL createAction('QOApproveBS', NULL, 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 3, NULL, 'Action_Approve', NULL, 2, 1, 0, NULL, NULL, 1);

	CALL createAction('QOTriage', 'QualityActions', 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 1, NULL, 'Action_Triage', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOChangeCategory', 'QualityActions', 2, 'QualityObject Change Category', 'UIPageRedirectAction', 
		'ChangeCategoryVP', NULL, 0, 2, NULL, 'Action_ChangeCategory', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOChangeOwner', 'QualityActions', 2, 'QualityObject Change Owner', 'UIPageRedirectAction', 
		'ReassignOwnerVP', NULL, 0, 3, NULL, 'Action_Reassign', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOApprove', 'QualityActions', 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 4, NULL, 'Action_Approve', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOCancelApproval', 'QualityActions', 2, 'QualityObject Cancel Approval', 'UIPageRedirectAction', 
		'CancelApprovalSheet_VP', NULL, 0, 5, NULL, 'Action_CancelApproval', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOResolve', 'QualityActions', 2, 'QualityObject Resolve', 'UIPageRedirectAction', 
		'QualityObjectResolution_VP', NULL, 0, 6, NULL, 'Action_Resolve', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOReopen', 'QualityActions', 2, 'QualityObject Reopen', 'UIPageRedirectAction', 
		'ReOpenQOVP', NULL, 0, 7, NULL, 'Action_Reopen', NULL, 1, 1, 0, NULL, NULL, 1);
	CALL createAction('QOAssignChecklist', 'QualityActions', 2, 'QualityObject Assign Checklist', 'UIPageRedirectAction', 
		'EventAssignChecklist_VP', NULL, 0, 8, NULL, 'Action_AssignChecklist', NULL, 1, 1, 0, NULL, NULL, 1);

	CALL createAction('QOHolds', 'ShopfloorActions', 2, 'QO Hold Containers', 'UIFloatPageOpenAction', 
		'MultiContainerHoldVP', NULL, 0, 1, NULL, 'Action_HoldMultiple', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('QOReleases', 'ShopfloorActions', 2, 'QO Release Containers', 'UIFloatPageOpenAction', 
		'MultiContainerReleaseVP', NULL, 0, 2, NULL, 'Action_ReleaseMultiple', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('QOMoveNonStds', 'ShopfloorActions', 2, 'QO MoveNonStd Containers', 'UIFloatPageOpenAction', 
		'MultiContainerMoveNonStdVP', NULL, 0, 3, NULL, 'Action_MoveNonStdMultiple', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('QOScrap', 'ShopfloorActions', 2, 'QO Scrap Container', 'UIFloatPageOpenAction', 
		'ChangeQtyVP', NULL, 0, 4, NULL, 'Action_Scrap', NULL, 1, 1, 0, NULL, NULL, 0);
	CALL createAction('QOSplit', 'ShopfloorActions', 2, 'QO Split Container', 'UIFloatPageOpenAction', 
		'SplitContainerVP', NULL, 0, 5, NULL, 'Action_Split', NULL, 1, 1, 0, NULL, NULL, 0);
	
	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOManage');
	CALL addActionRuleToActionDef('QOUserHasRoleRule', 'QOManage');
	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOTriageBS');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOTriageBS');
	CALL addActionRuleToActionDef('QOInReviewRestrictRule', 'QOTriageBS');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOTriageBS');
	CALL addActionRuleToActionDef('QOTriageRule', 'QOTriageBS');
	CALL addActionRuleToActionDef('QOInReviewRule', 'QOApproveBS');
	CALL addActionRuleToActionDef('QOIsApprovalRequiredRule', 'QOApproveBS');

	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOTriage');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOTriage');
	CALL addActionRuleToActionDef('QOInReviewRestrictRule', 'QOTriage');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOTriage');
	CALL addActionRuleToActionDef('QOTriageRule', 'QOTriage');

	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOChangeCategory');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOChangeCategory');
	CALL addActionRuleToActionDef('QOInReviewRestrictRule', 'QOChangeCategory');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOChangeCategory');

	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOChangeOwner');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOChangeOwner');
	CALL addActionRuleToActionDef('QOInReviewRestrictRule', 'QOChangeOwner');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOChangeOwner');

	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOAssignChecklist');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOAssignChecklist');
	CALL addActionRuleToActionDef('QOInReviewRestrictRule', 'QOAssignChecklist');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOAssignChecklist');
	CALL addActionRuleToActionDef('QOChecklistRule', 'QOAssignChecklist');
	
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOReopen');
	CALL addActionRuleToActionDef('QOReopenRule', 'QOReopen');
	
	CALL addActionRuleToActionDef('QOInReviewRule', 'QOApprove');
	CALL addActionRuleToActionDef('QOIsApprovalRequiredRule', 'QOApprove');
	
	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOResolve');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOResolve');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOResolve');
	CALL addActionRuleToActionDef('QOResolveRule', 'QOResolve');
	
	CALL addActionRuleToActionDef('QOPendingDeletedRule', 'QOCancelApproval');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOCancelApproval');
	CALL addActionRuleToActionDef('QOInReviewRule', 'QOCancelApproval');

	CALL addActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOHolds');
	CALL addActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOReleases');
	CALL addActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOMoveNonStds');
	CALL addActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOScrap');
	CALL addActionRuleToActionDef('QOSelectedTabLotsOrDisp', 'QOSplit');

	CALL addSourcePageToActionDef('QualitySearch_VP', 'QOManage','36D29BDF-FDCB-4681-8D86-9086C42F9568');
	CALL addSourcePageToActionDef('QualitySearch_VP', 'QOTriageBS','7E02CC6E-9F7A-40F6-A143-0E71758B6C20');
	CALL addSourcePageToActionDef('QualitySearch_VP', 'QOApproveBS','9DE57065-A5F6-4C9F-86A1-88DBDCF79DDB');

	CALL addSourcePageToActionDef('MessageCenterVP', 'QOManage','9D5AC9D4-84B4-4E87-BF56-A0F1FF0DCBC0');
	CALL addSourcePageToActionDef('MessageCenterVP', 'QOTriageBS','A8505EC9-DB95-43C6-BCBF-EBBD49E2C355');
	CALL addSourcePageToActionDef('MessageCenterVP', 'QOApproveBS','6CC43E67-62EA-4FFE-BD3A-3783C0518E52');

	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOTriage','92023AAE-EF4F-4413-A101-3F3C12D0B0E0');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOChangeCategory','3F021407-65AC-49E3-B523-4D58E29C11A1');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOChangeOwner','D21218CA-E7C0-42B0-8F24-8F76973FE908');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOApprove','20A47398-9022-4E2D-9ED1-316466ABF1B8');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOCancelApproval','7F473B4E-C98A-4151-AB10-2198C1DC8571');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOResolve','BE065FA6-EF96-4D42-9879-A084016BDF10');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOReopen','91266A95-4A02-4DD3-A42B-599B2EE2960A');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOAssignChecklist','79F16A5A-F52A-4D32-ADE5-D0C75D168665');

	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOHolds','B5D7C345-D0CE-49BA-A6A6-9063228F893C');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOReleases','9C988A6A-403E-4715-980F-A198E9072F29');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOMoveNonStds','9DC296B7-F54A-4B58-9754-9CAE0527240B');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOScrap','08F62A76-5377-4081-9DC2-EE712CF723AD');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOSplit','D12904A7-C13A-43E6-B404-24A65E977D64');
	

	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOTriage','D0A70FCF-E6C4-4E37-8248-78CF63944534');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOChangeCategory','8992A2F8-6DEF-40C0-903D-F941C6CAF77B');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOChangeOwner','348576F4-BBFA-4ADE-941A-D4C70CB5FAE2');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOApprove','43C80C99-4D64-4819-A124-A144C09C0ED7');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOCancelApproval','B3480C4F-66AC-47BE-B135-5F650946DCCC');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOResolve','BA38B835-BB9B-4D26-94EA-49FD07BFF674');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOReopen','A4BF3DED-4ADD-4462-B919-77128CE2C25A');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOAssignChecklist','DA50C8BE-680A-41C6-9FC9-F185C3E89F8D');

	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOHolds','9E60AA29-4D3E-4DCA-A4DD-C2EF2017ECA6');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOReleases','7B1163AE-F0D9-44CC-B207-832C5E94E02B');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOMoveNonStds','70D487F0-803D-4B67-8A16-BE73D47AA0DC');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOScrap','1BFAD01B-1424-4DA5-B8A4-DFADFEE19E92');
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOSplit','3B245215-7058-43DB-8D3E-4B37639F5E35');

--- Change Management Actions

	CALL createActionRule('CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True');
	CALL createActionRule('CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True or IsCollaborator = True');
	CALL createActionRule('CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided');
	CALL createActionRule('CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = True');
	CALL createActionRule('CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed');
	CALL createActionRule('CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers');
	CALL createActionRule('CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers');
	CALL createActionRule('CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed');
	CALL createActionRule('CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated');
	CALL createActionRule('CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1');
	CALL createActionRule('CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided');

	CALL createAction('CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305, 0);
	CALL createAction('CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305, 0);

		
	CALL createAction('CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL, 0);
	CALL createAction('CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL, 0);

	
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMActivatePackageAction');
	CALL addActionRuleToActionDef('CMActivatePackageRule', 'CMActivatePackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction');
	
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMCollaboratorRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsAssignApprovalRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsApprovePLMRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMCancelApprovalRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMRouteApprovalRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMPackageDetailsAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignContentAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMUpdatePackageAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMRouteApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAssignApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMApprovePLMAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMCancelApprovalAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMDeployPackageAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMAlterStateAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMContentHistoryAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMClosePackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMVoidPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMActivationImpactAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction');
	CALL addActionRuleToActionDef('CMIsSingleCPRule', 'CMOpenPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction');
	CALL addActionRuleToActionDef('CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction');
	

	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3');
	CALL addSourcePageToActionDef('PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1');
	
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A');
	CALL addSourcePageToActionDef('PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6');

	
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933');
	CALL addSourcePageToActionDef('ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD');
	
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3');
	CALL addSourcePageToActionDef('ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8');

	--Page Action for XceleratorShare
	CALL createAction('QOUploadToXceleratorShare', 'QualityActions', 2, 'Quality Object Upload To Xcelerator Share', 'UIFloatPageOpenAction', 'AttachToXceleratorShare_VP',
	NULL, 0, 9, NULL, 'Action_UploadToXceleratorShare_Popup', NULL, 1, 1, 0, 650, 500, 0);
	CALL addSourcePageToActionDef('GenericEventManage_VP', 'QOUploadToXceleratorShare','FE260C7E-EB9E-4E33-BCED-A2837A45F10C');
	CALL addSourcePageToActionDef('ProductionEventManage_VP', 'QOUploadToXceleratorShare','6269633E-53F0-4044-A708-95F51A82FBD2');
	CALL addActionRuleToActionDef('QOClosedRestrictRule', 'QOUploadToXceleratorShare');
	CALL addActionRuleToActionDef('QOIsOwnerRule', 'QOUploadToXceleratorShare');

END $$;


do $$ 
begin	
	CALL populateActionData();
end $$;

do $$ 
begin
	DROP PROCEDURE IF EXISTS createActionCategory;
	DROP PROCEDURE IF EXISTS createActionRule;
	DROP PROCEDURE IF EXISTS createActionDef;
	DROP PROCEDURE IF EXISTS createUIAction;
	DROP PROCEDURE IF EXISTS createAction;
	DROP PROCEDURE IF EXISTS addActionRuleToActionDef;
	DROP PROCEDURE IF EXISTS addSourcePageToActionDef;
	DROP PROCEDURE IF EXISTS cleanupExistingSystemRecords;
	DROP PROCEDURE IF EXISTS populateActionData;
end $$;
