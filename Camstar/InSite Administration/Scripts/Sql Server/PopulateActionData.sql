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


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'cleanupExistingSystemRecords' 
	   AND 	  type = 'P')
    DROP PROCEDURE cleanupExistingSystemRecords
GO
CREATE PROCEDURE cleanupExistingSystemRecords
AS 
BEGIN 
    SET NOCOUNT ON;

	-- Clean up existing system records
	DELETE FROM ActionCategory; 
	DELETE FROM UIAction;
	DELETE FROM UIFloatPageLocation;
	DELETE FROM UISourcePage; 
	DELETE FROM ActionDefActionRules;
	DELETE FROM ActionRule;
	DELETE FROM ActionDef;		
	
END
GO



IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionRule' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionRule
GO
CREATE PROCEDURE createActionRule(
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Expression			nvarchar(1000))
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM ActionRule WHERE ActionRuleName = @Name)
	BEGIN
		PRINT('Inserting ActionRule: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionRule'		
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
		INSERT INTO ActionRule
			(ActionRuleId
			,ActionRuleName
			,CDOTypeId
			,ChangeCount
			,Description
			,Expression
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,@Expression		-- nvarchar(255)
			,0);                -- bit
	END
	ELSE
		PRINT('ActionRule ' + @Name + ' already exists');
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: createActionCategory
-- DESCR: Helper function to create an Action Category record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionCategory' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionCategory
GO
CREATE PROCEDURE createActionCategory(
	@Name				nvarchar(30),
	@LabelName			nvarchar(50),
	@Sequence			int)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM ActionCategory WHERE ActionCategoryName = @Name)
	BEGIN
		PRINT('Inserting ActionCategory: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionCategory'	
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
		
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
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,@LabelName		    -- nvarchar(50)
			,NULL				-- nvarchar(255)
			,@Sequence			-- int
			,0);				-- bit
	END
	ELSE
		PRINT('ActionCategory ' + @Name + ' already exists');
END
GO

-----------------------------------------------------------------------------
-- PROCEDURE: createActionDef
-- DESCR: Helper function to create ActionDef record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE createActionDef
GO
CREATE PROCEDURE createActionDef(
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Type				int, 
    @InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int

	SELECT @InstanceId = ActionId FROM ActionDef WHERE ActionName = @Name;
	
	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting Action: ' + @Name);
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'ActionDef'
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
		INSERT INTO ActionDef
			(ActionId
			,ActionName
			,ActionType
			,CDOTypeId
			,ChangeCount
			,Description
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@Type				-- int
			,@CDOTypeId         -- int
			,1                  -- int
			,@Description       -- nvarchar(255)
			,0);                -- bit

	END
	ELSE
		PRINT('Action ' + @Name + ' already exists');
END
GO
	
-----------------------------------------------------------------------------
-- PROCEDURE: createUIAction
-- DESCR: Helper function to create UIAction record
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createUIAction' 
	   AND 	  type = 'P')
    DROP PROCEDURE createUIAction
GO
CREATE PROCEDURE createUIAction(
	@ActionId			char(16),
	@Name				nvarchar(30),
	@Type				int,
	@Description		nvarchar(255),
	@UIType				nvarchar(30),
	@UIVirtualPageName	nvarchar(30),
	@UIPageFlowName	    nvarchar(30),
	@MapItem		    nvarchar(30),
	@PortalTabOption	int,
	@ClearValues		bit,
	@ServiceName		nvarchar(30),
	@LabelName			nvarchar(66),
	@ShowButtons		bit,
	@IsPrimary          bit,
	@ActionCategoryName	nvarchar(30),
	@Sequence			int,
	@Width				int,
	@Height				int,
	@ForceRedirect		bit,
	@InstanceId varchar(16) OUTPUT)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @CDOTypeId	int	
	DECLARE @UIVirtualPageId	varchar(16);
	DECLARE @UIPageFlowId		varchar(16);
	DECLARE @ActionCategoryId	varchar(16);
	DECLARE @FloatPageLocationId	varchar(16);

	SELECT @InstanceId = UIActionId FROM UIAction WHERE UIActionName = @Name ;

	IF (@InstanceId IS NULL)
	BEGIN
		PRINT('Inserting UIAction: ' + @Name);

		SET @UIVirtualPageId = NULL;
		IF ISNULL(@UIVirtualPageName,'') <> ''
			SELECT @UIVirtualPageId = UIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = @UIVirtualPageName;  
		
		SET @UIPageFlowId = NULL;
		IF ISNULL(@UIPageFlowName,'') <> ''
			SELECT @UIPageFlowId = UIPageFlowId FROM UIPageFlow WHERE UIPageFlowName = @UIPageFlowName;  
		
		SET @ActionCategoryId = NULL;
		IF ISNULL(@ActionCategoryName,'') <> ''
			SELECT @ActionCategoryId = ActionCategoryId FROM ActionCategory WHERE ActionCategoryName = @ActionCategoryName;  
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = @UIType
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;
				
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
			(@ActionCategoryId  -- char(16)
			,@CDOTypeId         -- int
			,1                  -- int
			,@ClearValues       -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@ForceRedirect     -- bit
			,0                  -- int
			,0                  -- int
			,0                  -- int
			,@IsPrimary         -- bit
			,0                  -- int
			,@LabelName			-- nvarchar(30)
			,NULL				-- nvarchar(255)
			,@ActionId			-- char(16)
			,0                  -- int
			,@ServiceName       -- nvarchar(30)
			,@Sequence			-- int
			,@ShowButtons		-- bit
			,@InstanceId        -- char(16)
			,@Name              -- nvarchar(30)
			,@UIVirtualPageId   -- char(16)
			,@UIPageFlowId		-- char(16)
			,@MapItem			-- nvarchar(30)
			,@PortalTabOption	-- int
			,0);				-- int  
	  IF (@Width IS NOT NULL) AND (@Height IS NOT NULL)
	  BEGIN
	    SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'UIFloatPageLocation';
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @FloatPageLocationId OUTPUT;
		
        
        INSERT INTO UIFloatPageLocation (UIFloatPageLocationId, CDOTypeId, ChangeCount, IsFrozen, UIFloatPageOpenActionId, Width, Height) 
        VALUES (@FloatPageLocationId, @CDOTypeId, 1, 0, @InstanceId, @Width, @Height);
        
        UPDATE UIAction SET FrameLocationId = @FloatPageLocationId WHERE UIActionId = @InstanceId;
	  END
	END
	ELSE
		PRINT('UIAction ' + @Name + ' already exists');
END
GO
	
--------------------------------------------------------------------------------------------------------
-- PROCEDURE: createAction
-- DESCR: Helper function to create UIAction records
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createAction' 
	   AND 	  type = 'P')
    DROP PROCEDURE createAction
GO
CREATE PROCEDURE createAction(
	@Name				nvarchar(30),
	@ActionCategoryName	nvarchar(30),
	@Type				int,
	@Description		nvarchar(255),
	@UIType				nvarchar(30),
	@UIVirtualPageName	nvarchar(30),
	@UIPageFlowName	    nvarchar(30),
	@ClearValues		bit,
	@Sequence			int,
	@ServiceName		nvarchar(30),
	@LabelName			nvarchar(66),
	@MapItem			nvarchar(30),
	@PortalTabOption	int,
	@ShowButtons		bit,
	@IsPrimary          bit,
	@Width				int,
	@Height				int,
	@ForceRedirect		bit)
AS
BEGIN
    SET NOCOUNT ON;
	/* It is currently unclear to me why the UIAction has a Name or Description */
	DECLARE @ActionId			varchar(16);
	DECLARE @UIActionId			varchar(16);
	
	EXEC CreateActionDef @Name, @Description, @Type, @ActionId OUTPUT
	EXEC createUIAction @ActionId,@Name,@Type,@Description,@UIType,@UIVirtualPageName,@UIPageFlowName, @MapItem,
		@PortalTabOption, @ClearValues, @ServiceName,@LabelName,@ShowButtons,@IsPrimary,@ActionCategoryName,@Sequence,@Width,@Height,@ForceRedirect,
		@UIActionId OUTPUT
			
	UPDATE ActionDef SET UIActionId = @UIActionId WHERE ActionId=@ActionId;
END
GO


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addActionRuleToActionDef
-- DESCR: Helper function to add ActionRules to the list on an Action
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'addActionRuleToActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE addActionRuleToActionDef
GO
CREATE PROCEDURE addActionRuleToActionDef(
	@ActionRule			nvarchar(30), 
	@Action				nvarchar(30))
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @FieldId		int
	DECLARE @ActionId		varchar(16)
	DECLARE @ActionRuleId	varchar(16)
	DECLARE @Sequence		int
	
	
	IF NOT EXISTS (	SELECT * FROM ActionDefActionRules dr
					JOIN ActionDef d ON d.ActionId = dr.ActionId
					JOIN ActionRule r ON r.ActionRuleId = dr.ActionRulesId	
					WHERE d.ActionName = @Action AND r.ActionRuleName = @ActionRule
				   )
	BEGIN
		PRINT('Adding ActionRule ' + @ActionRule + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId FROM ActionDef WHERE ActionName = @Action;
		SELECT @ActionRuleId = ActionRuleId FROM ActionRule WHERE ActionRuleName = @ActionRule;

		IF @ActionId IS NULL
		BEGIN
			PRINT('Action ' + @Action + ' could not be found');
			RETURN
		END
		IF @ActionRuleId IS NULL
		BEGIN
			PRINT('ActionRule ' + @ActionRule + ' could not be found');
			RETURN
		END
		
		SELECT @FieldID = f.FieldID FROM CDOFields f 
			JOIN CDODefinition d ON d.CDODefID=f.CDODefID
			WHERE f.FieldName='ActionRules' and d.CDOName='ActionDef';
		
		SELECT @Sequence = COUNT(*) + 1 FROM ActionDefActionRules WHERE ActionId = @ActionId;
		
		INSERT INTO ActionDefActionRules 
			(ActionId
			,ActionRulesId
			,FieldId
			,Sequence)
			VALUES
			(@ActionId           -- char(16)
			,@ActionRuleId		 -- char(16)
			,@FieldId			 -- int
			,@Sequence);         -- int
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the ActionRule ' + @ActionRule);
END
GO


-------------------------------------------------------------------------------------------------------
-- PROCEDURE: addSourcePageToActionDef
-- DESCR: Helper function to add SourcePages to the list on an Action
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'addSourcePageToActionDef' 
	   AND 	  type = 'P')
    DROP PROCEDURE addSourcePageToActionDef
GO
CREATE PROCEDURE addSourcePageToActionDef(
	@VirtualPage	nvarchar(30),
	@Action			nvarchar(30),
	@ExportImportKey nvarchar(36))
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @ActionId		varchar(16)
	DECLARE @ActionRuleId	varchar(16)
	DECLARE @CDOTypeId		int
	DECLARE @InstanceId		varchar(16);
	DECLARE @UIVirtualPageId varchar(16);
	
	
	IF NOT EXISTS (	SELECT * FROM UISourcePage s
					JOIN ActionDef d ON d.ActionId = s.ActionId
					JOIN UIVirtualPage v ON v.UIVirtualPageId = s.UIVirtualPageId
					WHERE d.ActionName = @Action AND v.UIVirtualPageName = @VirtualPage
				   )
	BEGIN
		PRINT('Adding UISourcePage ' + @VirtualPage + ' to Action ' + @Action);
		
		SELECT @ActionId = ActionId FROM ActionDef WHERE ActionName = @Action;
		SELECT @UIVirtualPageId = UIVirtualPageId FROM UIVirtualPage WHERE UIVirtualPageName = @VirtualPage;  

		IF @ActionId IS NULL
		BEGIN
			PRINT('Action ' + @Action + ' could not be found');
			RETURN
		END
		IF @UIVirtualPageId IS NULL
		BEGIN
			PRINT('UIVirtualPage ' + @VirtualPage + ' could not be found');
			RETURN
		END
		
		SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName ='UISourcePage';
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT;

		INSERT INTO UISourcePage 
			(ActionId
			,CDOTypeId
			,ChangeCount
			,UISourcePageId
			,UIVirtualPageId
			,ExportImportKey)
			VALUES
			(@ActionId           -- char(16)
			,@CDOTypeId			 -- int
			,0					 -- int
			,@InstanceId		 -- char(16)
			,@UIVirtualPageId
			,@ExportImportKey);  -- char(16)
				
	END
	ELSE
		PRINT('Action ' + @Action + ' has been already linked to the UISourcePage ' + @VirtualPage);
END
GO

BEGIN

-- Clean up existing system records
	EXEC cleanupExistingSystemRecords
	
	
-- Action Categories
	EXEC createActionCategory 'QualityActions','Lbl_QualityActions',1;
	EXEC createActionCategory 'ShopfloorActions','Lbl_ShopfloorActions',2;
	EXEC createActionCategory 'ReportingActions','Lbl_ReportingActions',3;

--- Container actions
	EXEC createActionRule 'ContainerInProcessRule', 'Container In Process Action Rule', '(Operation.UseQueue = True and CurrentContainerStatus.InProcess = True) or Operation.UseQueue = False';
	EXEC createActionRule 'ContainerInQueueRule', 'Container In Queue Action Rule', 'Operation.UseQueue = True and CurrentContainerStatus.InProcess = False';
	EXEC createActionRule 'ContainerOnHoldRule', 'Container OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = True';
	EXEC createActionRule 'ContainerNotOnHoldRule', 'Container Not OnHold Action Rule', 'CurrentContainerStatus.IsOnHold = False';
	EXEC createActionRule 'ContainerClosedRule', 'Container Closed Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed';
	EXEC createActionRule 'ContainerActiveRule', 'Container Active Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active';
	EXEC createActionRule 'ContainerActiveOrInTransitRule', 'Container In Process Action Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Active or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit';
	EXEC createActionRule 'ContainerClosedOrInTransitRule', 'Container Is Closed / In Transit Rule', 'CurrentContainerStatus.Status = Constants.ContainerStatus.Closed or CurrentContainerStatus.Status = Constants.ContainerStatus.InTransit';
	EXEC createActionRule 'IsSingleContainerRule', 'Container Single Action Rule', 'not(IsFieldDefined("Containers", GetCurrentService())) or GetListCount(GetCurrentService().Containers) = 1 or IsFieldDefined("ServiceIsContainerTxn",GetCurrentService())';
	EXEC createActionRule 'IsMultiContainerRule', 'Container Multi Action Rule', 'IsFieldDefined("Containers", GetCurrentService()) and GetListCount(GetCurrentService().Containers) > 1';
	EXEC createActionRule 'ContainerIsNotChildRule', 'Container Is Not Child Rule', 'Container.Parent = null';
	EXEC createActionRule 'ContainerNotClosedRule', 'Container Is Not Closed Rule', 'CurrentContainerStatus.Status <> Constants.ContainerStatus.Closed';
	
	---[AMB] NOTE: the labels may not be correct for Container Search page.  may need new/separate actions for Container Search page.
	EXEC createAction 'MoveInPopupAction', NULL, 1, 'MoveIn...', 'UIFloatPageOpenAction', 'MoveInVP', NULL, 0, 
		1, NULL,	'Action_MoveIn_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MoveStdPopupAction', NULL, 1, 'Move...', 'UIFloatPageOpenAction', 'MoveStdVP',NULL, 0,
		2, NULL,	'Action_Move_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MoveInImmediateAction', NULL, 1, 'Move In Immediate ', 'UISubmitAction', NULL, NULL, 0,
		3, 'MoveIn',  'Action_MoveIn_Immediate', NULL, 1, 0, 1, NULL, NULL, 0
	EXEC createAction 'MoveImmediateAction', NULL, 1, 'Move Immediate', 'UISubmitAction', NULL, NULL, 0,
		4, 'MoveStd', 'Action_Move_Immediate', NULL, 1, 0, 1, NULL, NULL, 0
	EXEC createAction 'ContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'ContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_Hold_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'ContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'ContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_Release_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'ContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'OpenVP', NULL, 0, 
		7, NULL,	'Action_Open_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'ContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'CloseVP', NULL, 0, 
		8, NULL,	'Action_Close_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'ContainerReworkAction', NULL, 1, 'Rework...', 'UIFloatPageOpenAction', 'ReworkVP', NULL, 0, 
		9, NULL,	'Action_Rework_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'CreateProductionEventAction', NULL, 1, 'Create Production Event...', 'UIFloatPageOpenAction', 'ProductionEventRecord_VPR2', NULL, 0,
		10, 'CreateProductionEvent',  'Action_CreateProductionEvent_PopUp', NULL, 1, 0, 0, 1220, 700, 0
	EXEC createAction 'MultiContainerHoldAction', NULL, 1, 'Hold...', 'UIFloatPageOpenAction', 'MultiContainerHoldVP', NULL, 0, 
		5, NULL,	'Action_MultiHold_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MultiContainerReleaseAction', NULL, 1, 'Release...', 'UIFloatPageOpenAction', 'MultiContainerReleaseVP', NULL, 0, 
		6, NULL,	'Action_MultiRelease_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MultiContainerOpenAction', NULL, 1, 'Open...', 'UIFloatPageOpenAction', 'MultiContainerOpenVP', NULL, 0, 
		7, NULL,	'Action_MultiOpen_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MultiContainerCloseAction', NULL, 1, 'Close...', 'UIFloatPageOpenAction', 'MultiContainerCloseVP', NULL, 0, 
		8, NULL,	'Action_MultiClose_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MultiContainerMoveNonStdAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MultiContainerMoveNonStdVP', NULL, 0, 
		9, NULL,	'Action_MultiMoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'MoveNonStdPopupAction', NULL, 1, 'Move Non Std...', 'UIFloatPageOpenAction', 'MoveNonStdVP', NULL, 0, 
		10, NULL,	'Action_MoveNonStd_PopUp', NULL, 1, 1, 0, NULL, NULL, 0


	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MoveInPopupAction'
	EXEC addActionRuleToActionDef 'ContainerInQueueRule', 'MoveInPopupAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MoveInPopupAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MoveInPopupAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'MoveInPopupAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MoveInImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerInQueueRule', 'MoveInImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MoveInImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MoveInImmediateAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'MoveInImmediateAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MoveImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerInProcessRule', 'MoveImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MoveImmediateAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MoveImmediateAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'MoveImmediateAction'
 
	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MoveStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerInProcessRule', 'MoveStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MoveStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MoveStdPopupAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'MoveStdPopupAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MoveNonStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerInProcessRule', 'MoveNonStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MoveNonStdPopupAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MoveNonStdPopupAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'MoveNonStdPopupAction'

	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'ContainerHoldAction'
	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'ContainerHoldAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'ContainerHoldAction'

	EXEC addActionRuleToActionDef 'ContainerOnHoldRule', 'ContainerReleaseAction'
	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'ContainerReleaseAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'ContainerReleaseAction'

	EXEC addActionRuleToActionDef 'ContainerClosedOrInTransitRule', 'ContainerOpenAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'ContainerOpenAction'

	EXEC addActionRuleToActionDef 'ContainerActiveOrInTransitRule', 'ContainerCloseAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'ContainerCloseAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'ContainerReworkAction'
	EXEC addActionRuleToActionDef 'ContainerInProcessRule', 'ContainerReworkAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'ContainerReworkAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'ContainerReworkAction'
	EXEC addActionRuleToActionDef 'IsSingleContainerRule', 'ContainerReworkAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'CreateProductionEventAction'

	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MultiContainerHoldAction'
	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MultiContainerHoldAction'
	EXEC addActionRuleToActionDef 'IsMultiContainerRule', 'MultiContainerHoldAction'

	EXEC addActionRuleToActionDef 'ContainerOnHoldRule', 'MultiContainerReleaseAction'
	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MultiContainerReleaseAction'
	EXEC addActionRuleToActionDef 'IsMultiContainerRule', 'MultiContainerReleaseAction'

	EXEC addActionRuleToActionDef 'ContainerClosedOrInTransitRule', 'MultiContainerOpenAction'
	EXEC addActionRuleToActionDef 'IsMultiContainerRule', 'MultiContainerOpenAction'

	EXEC addActionRuleToActionDef 'ContainerActiveOrInTransitRule', 'MultiContainerCloseAction'
	EXEC addActionRuleToActionDef 'IsMultiContainerRule', 'MultiContainerCloseAction'

	EXEC addActionRuleToActionDef 'ContainerActiveRule', 'MultiContainerMoveNonStdAction'
	EXEC addActionRuleToActionDef 'ContainerInProcessRule', 'MultiContainerMoveNonStdAction'
	EXEC addActionRuleToActionDef 'ContainerNotOnHoldRule', 'MultiContainerMoveNonStdAction'
	EXEC addActionRuleToActionDef 'ContainerIsNotChildRule', 'MultiContainerMoveNonStdAction'
	EXEC addActionRuleToActionDef 'IsMultiContainerRule', 'MultiContainerMoveNonStdAction' 

 
 	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MoveInPopupAction','260A22C2-B4CE-4A5F-B0D5-8EBFE97CA9ED'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MoveStdPopupAction','24B18EA1-FDEA-4C1D-8031-6A9B90C1D420'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MoveInImmediateAction','6EB3D47B-33B1-48FA-B960-31464E9C25ED'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'ContainerHoldAction','00B2384A-AAA1-4E33-A788-494E3BFEFFFD'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'ContainerReleaseAction','C79059FB-4330-4AEA-987A-55189C46A16B'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'ContainerOpenAction','2CB56FA4-F203-473F-90A7-4DE3C0DF44F6'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'ContainerCloseAction','43EFCB05-9D06-4474-B761-6C854652466F'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'ContainerReworkAction','3AC8D39C-9561-4AE6-B34A-A2287809C8C8'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'CreateProductionEventAction','D4477E8E-6E1B-43D9-8F7B-496AA2B45477'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MoveNonStdPopupAction','D85A27D5-2DE5-45B4-91C6-2193916BD7E4'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MultiContainerHoldAction','3519F13C-B4F5-413D-881E-54889EFB0B47'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MultiContainerReleaseAction','CAF63050-340F-412C-B8B7-016BBB7D435A'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MultiContainerOpenAction','056ED1D3-704E-409C-A63F-9B94754A64DF'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MultiContainerCloseAction','B205F2A4-4672-41AE-B099-04F241DFAA44'
	EXEC addSourcePageToActionDef 'ContainerSearchVP', 'MultiContainerMoveNonStdAction','159B7CFA-17B5-4260-9AE0-BD3908316D2F'
	
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MoveInPopupAction','0C45506F-5793-40A4-8EB1-00CDD863939F'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MoveStdPopupAction','41447F41-EBA6-4018-A688-B1660B2CD283'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MoveInImmediateAction','431370C4-292A-4AC4-9E97-F0E75BB39733'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'ContainerHoldAction','2834C71A-5275-4559-BB84-C7E8CD81609A'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'ContainerReleaseAction','2DD7FB0A-026E-4605-A8AB-BE4C638AF103'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'ContainerOpenAction','5DFA3318-8628-472B-95A7-D04499917432'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'ContainerCloseAction','33C013FF-5395-4FE5-A33C-4B60025988D4'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'ContainerReworkAction','3D9E3BBA-6387-4D17-B7D5-2DD70D2DDEFE'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'CreateProductionEventAction','3B4FE960-2F03-470C-924E-9E7CDCF9228A'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MoveNonStdPopupAction','526EE7D5-81AC-4A68-B392-7B9BE3A63251'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MultiContainerHoldAction','0F92D6B5-A45E-4B95-AED2-52FF1655CB95'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MultiContainerReleaseAction','20420500-29CA-43EC-8E5F-755F7EE05440'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MultiContainerOpenAction','29945AB3-B6CE-4324-9C1B-3C1699B2C8DD'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MultiContainerCloseAction','A97B3311-90BB-4173-9B4E-A5C3CA20A1C5'
	EXEC addSourcePageToActionDef 'ContainerSearchVP_R2', 'MultiContainerMoveNonStdAction','96BAA722-48C6-42A1-A336-956362FA9D8E'
  
	EXEC addSourcePageToActionDef 'OperationalViewVP', 'MoveInPopupAction','5E522399-94E9-4FFC-B267-000641FECF4D'
	EXEC addSourcePageToActionDef 'OperationalViewVP', 'MoveStdPopupAction','BCBAC511-C22E-462F-BA55-3001C0E1186F'
	EXEC addSourcePageToActionDef 'OperationalViewVP', 'MoveImmediateAction','2FA4C318-2BDB-4012-82D0-5FD805576CB1'
	EXEC addSourcePageToActionDef 'OperationalViewVP', 'MoveInImmediateAction','25AA2BFB-C522-4C5D-8F66-031972747ABF'
	
	EXEC addSourcePageToActionDef 'OperationalViewVPR2', 'MoveInPopupAction','FB57C691-5FAE-4305-92FD-F240E324978C'
	EXEC addSourcePageToActionDef 'OperationalViewVPR2', 'MoveStdPopupAction','B0262728-2560-458B-BBCB-B2C5D5818770'
	EXEC addSourcePageToActionDef 'OperationalViewVPR2', 'MoveImmediateAction','B0AB31D7-E415-43A8-B837-EA88236B7B26'
	EXEC addSourcePageToActionDef 'OperationalViewVPR2', 'MoveInImmediateAction','E9CC90B6-70F6-4B83-ABB3-6ED51569D193'

	EXEC addSourcePageToActionDef 'OperationalViewScanVP', 'MoveInPopupAction','708AF31C-C372-4178-AACF-65219E8DCFAD'
	EXEC addSourcePageToActionDef 'OperationalViewScanVP', 'MoveStdPopupAction','82C06C64-ECEC-4CE2-8109-67EB76B649D1'
	EXEC addSourcePageToActionDef 'OperationalViewScanVP', 'MoveImmediateAction','852AA642-07AC-46FE-8254-EE829AA1EC30'
	EXEC addSourcePageToActionDef 'OperationalViewScanVP', 'MoveInImmediateAction','15E73454-727D-465A-91E1-5DE07060DED3'

--- Quality Object actions
	EXEC createActionRule 'QOPendingDeletedRule', 'QualityObject Pending-Deleted restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Pending or QualityObjectDetail.Status = Constants.QualityStatus.Deleted)';
	EXEC createActionRule 'QOClosedRestrictRule', 'QualityObject Closed restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.Closed)';
	EXEC createActionRule 'QOInReviewRestrictRule', 'QualityObject InReview restriction Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview)';
	EXEC createActionRule 'QOUserHasRoleRule', 'QualityObject User Has Role Rule', 'QualityObjectDetail.UserHasRole = True';
	EXEC createActionRule 'QOCategoryEvent', 'QualityObject Category is Event', 'QualityObjectDetail.Category = Constants.Category.Event';
	EXEC createActionRule 'QOCategoryNonconformance', 'QualityObject Category is Nonconformance', 'QualityObjectDetail.Category = Constants.Category.Nonconformance';
	EXEC createActionRule 'QOIsOwnerRule', 'QualityObject User Is Owner Rule', 'QualityObjectDetail.UserIsOwner = True';
	EXEC createActionRule 'QOIsApprovalRequiredRule', 'QualityObject Is Approval Required Rule', 'QualityObjectDetail.IsApprovalRequired = True';
	EXEC createActionRule 'QOInReviewRule', 'QualityObject In Review Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.InReview';
	EXEC createActionRule 'QOTriageRule', 'QualityObject Triage Rule', 'QualityObjectDetail.TriageComplete = False';
	EXEC createActionRule 'QOChecklistRule', 'QualityObject Checklist Rule', 'QualityObjectDetail.ChecklistSaved = False';
	EXEC createActionRule 'QOReopenRule', 'QualityObject Reopen Rule', 'QualityObjectDetail.Status = Constants.QualityStatus.Closed';
	EXEC createActionRule 'QOResolveRule', 'QualityObject Resolve Rule', 'not (QualityObjectDetail.Status = Constants.QualityStatus.InReview and QualityObjectDetail.CompletionApprovalStatus <> Constants.ApprovalStatus.Approved)';
	EXEC createActionRule 'QOSelectedTabLotsOrDisp', 'Affected Materials tab for Manage Event', 'ActionSelectedTab = 1 or ActionSelectedTab = 2';
	EXEC createActionRule 'QOSelectedTabDisp', 'Disposition tab for Manage Event', 'ActionSelectedTab = 2';

	EXEC createAction 'QOManage', NULL, 2, 'Manage Event', 'UIPageMappingAction', 
		NULL, NULL, 0, 1, NULL, 'Action_ManageRecords', 'EventRecordView', 2, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOTriageBS', NULL, 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 2, NULL, 'Action_Triage', NULL, 2, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOApproveBS', NULL, 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 3, NULL, 'Action_Approve', NULL, 2, 1, 0, NULL, NULL, 1

	EXEC createAction 'QOTriage', 'QualityActions', 2, 'QualityObject Triage', 'UIPageRedirectAction', 
		'TriageVP', NULL, 0, 1, NULL, 'Action_Triage', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOChangeCategory', 'QualityActions', 2, 'QualityObject Change Category', 'UIPageRedirectAction', 
		'ChangeCategoryVP', NULL, 0, 2, NULL, 'Action_ChangeCategory', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOChangeOwner', 'QualityActions', 2, 'QualityObject Change Owner', 'UIPageRedirectAction', 
		'ReassignOwnerVP', NULL, 0, 3, NULL, 'Action_Reassign', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOApprove', 'QualityActions', 2, 'QualityObject Approve', 'UIPageRedirectAction', 
		'SignApproval_VP', NULL, 0, 4, NULL, 'Action_Approve', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOCancelApproval', 'QualityActions', 2, 'QualityObject Cancel Approval', 'UIPageRedirectAction', 
		'CancelApprovalSheet_VP', NULL, 0, 5, NULL, 'Action_CancelApproval', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOResolve', 'QualityActions', 2, 'QualityObject Resolve', 'UIPageRedirectAction', 
		'QualityObjectResolution_VP', NULL, 0, 6, NULL, 'Action_Resolve', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOReopen', 'QualityActions', 2, 'QualityObject Reopen', 'UIPageRedirectAction', 
		'ReOpenQOVP', NULL, 0, 7, NULL, 'Action_Reopen', NULL, 1, 1, 0, NULL, NULL, 1
	EXEC createAction 'QOAssignChecklist', 'QualityActions', 2, 'QualityObject Assign Checklist', 'UIPageRedirectAction', 
		'EventAssignChecklist_VP', NULL, 0, 8, NULL, 'Action_AssignChecklist', NULL, 1, 1, 0, NULL, NULL, 1

	EXEC createAction 'QOHolds', 'ShopfloorActions', 2, 'QO Hold Containers', 'UIFloatPageOpenAction', 
		'MultiContainerHoldVP', NULL, 0, 1, NULL, 'Action_HoldMultiple', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'QOReleases', 'ShopfloorActions', 2, 'QO Release Containers', 'UIFloatPageOpenAction', 
		'MultiContainerReleaseVP', NULL, 0, 2, NULL, 'Action_ReleaseMultiple', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'QOMoveNonStds', 'ShopfloorActions', 2, 'QO MoveNonStd Containers', 'UIFloatPageOpenAction', 
		'MultiContainerMoveNonStdVP', NULL, 0, 3, NULL, 'Action_MoveNonStdMultiple', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'QOScrap', 'ShopfloorActions', 2, 'QO Scrap Container', 'UIFloatPageOpenAction', 
		'ChangeQtyVP', NULL, 0, 4, NULL, 'Action_Scrap', NULL, 1, 1, 0, NULL, NULL, 0
	EXEC createAction 'QOSplit', 'ShopfloorActions', 2, 'QO Split Container', 'UIFloatPageOpenAction', 
		'SplitContainerVP', NULL, 0, 5, NULL, 'Action_Split', NULL, 1, 1, 0, NULL, NULL, 0
	
	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOManage'
	EXEC addActionRuleToActionDef 'QOUserHasRoleRule', 'QOManage'
	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOTriageBS'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOTriageBS'
	EXEC addActionRuleToActionDef 'QOInReviewRestrictRule', 'QOTriageBS'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOTriageBS'
	EXEC addActionRuleToActionDef 'QOTriageRule', 'QOTriageBS'
	EXEC addActionRuleToActionDef 'QOInReviewRule', 'QOApproveBS'
	EXEC addActionRuleToActionDef 'QOIsApprovalRequiredRule', 'QOApproveBS'

	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOTriage'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOTriage'
	EXEC addActionRuleToActionDef 'QOInReviewRestrictRule', 'QOTriage'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOTriage'
	EXEC addActionRuleToActionDef 'QOTriageRule', 'QOTriage'

	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOChangeCategory'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOChangeCategory'
	EXEC addActionRuleToActionDef 'QOInReviewRestrictRule', 'QOChangeCategory'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOChangeCategory'

	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOChangeOwner'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOChangeOwner'
	EXEC addActionRuleToActionDef 'QOInReviewRestrictRule', 'QOChangeOwner'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOChangeOwner'

	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOAssignChecklist'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOAssignChecklist'
	EXEC addActionRuleToActionDef 'QOInReviewRestrictRule', 'QOAssignChecklist'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOAssignChecklist'
	EXEC addActionRuleToActionDef 'QOChecklistRule', 'QOAssignChecklist'
	
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOReopen'
	EXEC addActionRuleToActionDef 'QOReopenRule', 'QOReopen'
	
	EXEC addActionRuleToActionDef 'QOInReviewRule', 'QOApprove'
	EXEC addActionRuleToActionDef 'QOIsApprovalRequiredRule', 'QOApprove'
	
	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOResolve'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOResolve'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOResolve'
	EXEC addActionRuleToActionDef 'QOResolveRule', 'QOResolve'
	
	EXEC addActionRuleToActionDef 'QOPendingDeletedRule', 'QOCancelApproval'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOCancelApproval'
	EXEC addActionRuleToActionDef 'QOInReviewRule', 'QOCancelApproval'

	EXEC addActionRuleToActionDef 'QOSelectedTabLotsOrDisp', 'QOHolds'
	EXEC addActionRuleToActionDef 'QOSelectedTabLotsOrDisp', 'QOReleases'
	EXEC addActionRuleToActionDef 'QOSelectedTabLotsOrDisp', 'QOMoveNonStds'
	EXEC addActionRuleToActionDef 'QOSelectedTabLotsOrDisp', 'QOScrap'
	EXEC addActionRuleToActionDef 'QOSelectedTabLotsOrDisp', 'QOSplit'

	EXEC addSourcePageToActionDef 'QualitySearch_VP', 'QOManage','36D29BDF-FDCB-4681-8D86-9086C42F9568'
	EXEC addSourcePageToActionDef 'QualitySearch_VP', 'QOTriageBS','7E02CC6E-9F7A-40F6-A143-0E71758B6C20'
	EXEC addSourcePageToActionDef 'QualitySearch_VP', 'QOApproveBS','9DE57065-A5F6-4C9F-86A1-88DBDCF79DDB'

	EXEC addSourcePageToActionDef 'MessageCenterVP', 'QOManage','9D5AC9D4-84B4-4E87-BF56-A0F1FF0DCBC0'
	EXEC addSourcePageToActionDef 'MessageCenterVP', 'QOTriageBS','A8505EC9-DB95-43C6-BCBF-EBBD49E2C355'
	EXEC addSourcePageToActionDef 'MessageCenterVP', 'QOApproveBS','6CC43E67-62EA-4FFE-BD3A-3783C0518E52'

	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOTriage','92023AAE-EF4F-4413-A101-3F3C12D0B0E0'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOChangeCategory','3F021407-65AC-49E3-B523-4D58E29C11A1'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOChangeOwner','D21218CA-E7C0-42B0-8F24-8F76973FE908'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOApprove','20A47398-9022-4E2D-9ED1-316466ABF1B8'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOCancelApproval','7F473B4E-C98A-4151-AB10-2198C1DC8571'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOResolve','BE065FA6-EF96-4D42-9879-A084016BDF10'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOReopen','91266A95-4A02-4DD3-A42B-599B2EE2960A'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOAssignChecklist','79F16A5A-F52A-4D32-ADE5-D0C75D168665'

	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOHolds','B5D7C345-D0CE-49BA-A6A6-9063228F893C'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOReleases','9C988A6A-403E-4715-980F-A198E9072F29'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOMoveNonStds','9DC296B7-F54A-4B58-9754-9CAE0527240B'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOScrap','08F62A76-5377-4081-9DC2-EE712CF723AD'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOSplit','D12904A7-C13A-43E6-B404-24A65E977D64'
	

	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOTriage','D0A70FCF-E6C4-4E37-8248-78CF63944534'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOChangeCategory','8992A2F8-6DEF-40C0-903D-F941C6CAF77B'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOChangeOwner','348576F4-BBFA-4ADE-941A-D4C70CB5FAE2'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOApprove','43C80C99-4D64-4819-A124-A144C09C0ED7'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOCancelApproval','B3480C4F-66AC-47BE-B135-5F650946DCCC'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOResolve','BA38B835-BB9B-4D26-94EA-49FD07BFF674'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOReopen','A4BF3DED-4ADD-4462-B919-77128CE2C25A'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOAssignChecklist','DA50C8BE-680A-41C6-9FC9-F185C3E89F8D'

	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOHolds','9E60AA29-4D3E-4DCA-A4DD-C2EF2017ECA6'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOReleases','7B1163AE-F0D9-44CC-B207-832C5E94E02B'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOMoveNonStds','70D487F0-803D-4B67-8A16-BE73D47AA0DC'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOScrap','1BFAD01B-1424-4DA5-B8A4-DFADFEE19E92'
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOSplit','3B245215-7058-43DB-8D3E-4B37639F5E35'

--- Change Management Actions

	EXEC createActionRule 'CMPackageOwnerOROwnerRoleRule', 'Change Management Package Owner OR Owner Role Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True';
	EXEC createActionRule 'CMCollaboratorRule', 'Change Management Package Owner OR Owner Role OR Collaborator Rule', 'ChangePackage.Owner = Employee or IsOwnerRole = True or IsCollaborator = True';
	EXEC createActionRule 'CMActivatePackageRule', 'Change Management Activate Package Rule', 'ChangePackage.CPImportStatus != null and ChangePackage.Status != Constants.PackageStatus.Voided';
	EXEC createActionRule 'CMRouteApprovalRule', 'Change Management Route Approval Rule', 'IsRouteRequired = True';
	EXEC createActionRule 'CMCancelApprovalRule', 'Change Management Cancel Approval Rule', 'ChangePackage.ApprovalStatus = Transaction::__Const.ApprovalStatus.Routed';
	EXEC createActionRule 'CMIsAssignApprovalRule', 'Change Management Is Assign Approval Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.AssignApprovers';
	EXEC createActionRule 'CMIsApprovePLMRule', 'Change Management Is Approve PLM Rule', 'IsApprovalRequired = True and ChangePackage.ApprovalSheet.Name = Transaction::__Const.ApprovalType.NoApprovers';
	EXEC createActionRule 'CMIsCPStatusNotClosedMRule', 'Change Management Is Change Package Status Not Closed Rule', 'ChangePackage.Status != Constants.PackageStatus.Closed';
	EXEC createActionRule 'CMIsCPStatusNotVoidedMRule', 'Change Management Is Change Package Status Not Voided Rule', 'ChangePackage.Status != Constants.PackageStatus.Voided and ChangePackage.Status != Constants.PackageStatus.Closed and ChangePackage.CPImportStatus != Constants.ChangePackageImportStatus.Activated';
	EXEC createActionRule 'CMIsSingleCPRule', 'Change Management Is Single Change Package Rule', 'not(IsFieldDefined("ChangePackages", GetCurrentService())) or GetListCount(GetCurrentService().ChangePackages) = 1';
	EXEC createActionRule 'CMIsCPStatusClosedVoidedRule', 'Change Management Is Change Package Status Closed or Voided Rule', 'ChangePackage.Status == Constants.PackageStatus.Closed or ChangePackage.Status == Constants.PackageStatus.Voided';

	EXEC createAction 'CMPackageDetailsAction', NULL, 3, 'Package Details', 'UIPageRedirectAction', 'CM_PackageDetails_VP', NULL, 0, 0, 'GetChangePackageDetails', 'Action_PackageDetails', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAssignContentAction', NULL, 3, 'Assign Content', 'UIPageRedirectAction', 'AssignChangePkgContent_VP', NULL, 0, 1, 'AssignChangePkgContent', 'Action_AssignContent', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMUpdatePackageAction', NULL, 3, 'Update Package', 'UIPageRedirectAction', 'UpdateChangePkg_VP', NULL, 0, 2, 'UpdateChangePkg', 'Action_UpdatePackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMRouteApprovalAction', NULL, 3, 'Route for Approval', 'UIPageRedirectAction', 'RouteApproval_VP', NULL, 0, 3, 'RouteApproval', 'Action_RouteApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAssignApprovalAction', NULL, 3, 'Approve (Camstar)', 'UIPageRedirectAction', 'SignatureApproval_VP', NULL, 0, 4, 'SignatureApproval', 'Action_AssignApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMApprovePLMAction', NULL, 3, 'Approve (PLM)', 'UIPageRedirectAction', 'ApprovePackagePLM_VP', NULL, 0, 5, 'PLMApprovePackage', 'Action_ApprovePLM', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMCancelApprovalAction', NULL, 3, 'Cancel Approval', 'UIPageRedirectAction', 'CancelApproval_VP', NULL, 0, 6, 'CancelApproval', 'Action_CancelApproval', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMDeployPackageAction', NULL, 3, 'Deploy Package', 'UIPageRedirectAction', 'DeployChangePkg_VP', NULL, 0, 7, 'DeployChangePkg', 'Action_DeployPackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMAlterStateAction', NULL, 3, 'Alter Step', 'UIPageRedirectAction', 'MoveNonStdChangePkg_VP', NULL, 0, 8, 'MoveNonStdChangePkg', 'Action_AlterPackageStep', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMContentHistoryAction', NULL, 3, 'Content History', 'UIPageRedirectAction', 'ContentChangeHistoryInquiry_VP', NULL, 0, 9, 'ContentChangeHistoryInquiry', 'Action_ContentHistory', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMClosePackagePopupAction', NULL, 3, 'Close Package', 'UIFloatPageOpenAction', 'ClosePackageSinglePopup_VP', NULL, 0, 10, 'CloseCPStatus', 'Action_CloseChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidPackagePopupAction', NULL, 3, 'Void Package', 'UIFloatPageOpenAction', 'VoidPackageSinglePopup_VP', NULL, 0, 11, 'VoidCPStatus', 'Action_VoidChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMClosePackagesPopupAction', NULL, 3, 'Close (multiple)', 'UIFloatPageOpenAction', 'ClosePackageMultiPopup_VP', NULL, 0, 12, 'CloseCPStatuses', 'Action_CloseChangePackages', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMVoidPackagesPopupAction', NULL, 3, 'Void (multiple)', 'UIFloatPageOpenAction', 'VoidPackageMultiPopup_VP', NULL, 0, 13, 'VoidCPStatuses', 'Action_VoidChangePackages', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMOpenPackagePopupAction', NULL, 3, 'Open Package', 'UIFloatPageOpenAction', 'OpenPackageSinglePopup_VP', NULL, 0, 14, 'OpenCPStatus', 'Action_OpenChangePackage', NULL, 2, 1, 0, 535, 305, 0
	EXEC createAction 'CMOpenPackagesPopupAction', NULL, 3, 'Open Selected', 'UIFloatPageOpenAction', 'OpenPackageMultiPopup_VP', NULL, 0, 15, 'OpenCPStatuses', 'Action_OpenChangePackages', NULL, 2, 1, 0, 535, 305, 0

		
	EXEC createAction 'CMActivatePackageAction', NULL, 3, 'Activate Package', 'UIPageRedirectAction', 'ActivateChangePkg_VP', NULL, 0, 1, 'ActivateChangePkg', 'Action_ActivatePackage', NULL, 2, 1, 0, NULL, NULL, 0
	EXEC createAction 'CMActivationImpactAction', NULL, 3, 'Activation Impact', 'UIPageRedirectAction', 'ActivationImpact_VP', NULL, 0, 2, 'GetImpactDetailsInquiry', 'Action_ActivationImpact', NULL, 2, 1, 0, NULL, NULL, 0

	
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMActivatePackageAction'
	EXEC addActionRuleToActionDef 'CMActivatePackageRule', 'CMActivatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMActivatePackageAction'
	
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMCollaboratorRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsAssignApprovalRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsApprovePLMRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMCancelApprovalRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMRouteApprovalRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMPackageOwnerOROwnerRoleRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMDeployPackageAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMClosePackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMVoidPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMPackageDetailsAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAssignContentAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMUpdatePackageAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMRouteApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAssignApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMApprovePLMAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMCancelApprovalAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMDeployPackageAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMAlterStateAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMContentHistoryAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMClosePackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMVoidPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMActivationImpactAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotClosedMRule', 'CMClosePackagesPopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusNotVoidedMRule', 'CMVoidPackagesPopupAction'
	EXEC addActionRuleToActionDef 'CMIsSingleCPRule', 'CMOpenPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusClosedVoidedRule', 'CMOpenPackagePopupAction'
	EXEC addActionRuleToActionDef 'CMIsCPStatusClosedVoidedRule', 'CMOpenPackagesPopupAction'
	

	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMPackageDetailsAction','ADB6EA0D-5F55-4676-A160-24402B11C073'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAssignContentAction','90F765B5-3FE6-4B08-B053-F966F4851333'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMUpdatePackageAction','A73EB64B-C27F-4EE7-B64F-3A184B4804C0'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMRouteApprovalAction','25BA0D19-56F3-4BD3-B435-398DF1FEE643'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAssignApprovalAction','8E33311A-83C4-4B84-BB91-257283911E53'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMApprovePLMAction','6DFF0837-8464-4F95-A2D4-2B5E56EB75AD'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMCancelApprovalAction','F5DE857F-CB4D-4847-9583-932FE7FE7724'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMDeployPackageAction','6FC4C02B-DC1B-4225-B261-9B8C6A597F2F'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMAlterStateAction','FA3790F5-74E3-40E0-BDA4-32A8DB0930C6'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMContentHistoryAction','C9C7CC12-2088-4E2D-B90E-9072C015A5D6'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMActivationImpactAction','ED45AB8A-9D75-4A66-8926-91919BEB5923'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMClosePackagePopupAction','0CF51E3E-9A67-46D1-B139-FDA590944E03'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMVoidPackagePopupAction','9DF3DE33-1605-442E-B70D-5AEBAC5E90C3'
	EXEC addSourcePageToActionDef 'PackageInquiry_VP', 'CMOpenPackagePopupAction','78E898DE-D31E-45C5-9F64-596F096C48C1'
	
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMClosePackagesPopupAction','C31058F7-7C8A-4232-8E9A-3D5A91D17058'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMVoidPackagesPopupAction','99F93643-577C-4EC6-86E6-5FE363C2DD5F'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMPackageDetailsAction','76A90013-A9DF-4C1C-BB07-55645477800A'
	EXEC addSourcePageToActionDef 'PackageSearchMultiple_VP', 'CMOpenPackagesPopupAction','E71E0671-21F6-473D-A2BF-050938CCB5E6'

	
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMPackageDetailsAction','B5893D88-8380-44C5-9779-E87C35D3DE20'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMActivatePackageAction','B36DB06E-794F-4268-9A13-396D659E9CE0'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMClosePackagePopupAction','9CA39374-52C4-4C97-AD08-8569F7E41847'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMVoidPackagePopupAction','D84D1F99-407F-4A61-B36E-0704ED218933'
	EXEC addSourcePageToActionDef 'ActivationInquiry_VP', 'CMOpenPackagePopupAction','D54CE7C2-CCE7-4D8A-B77A-06FE2B6234CD'
	
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMPackageDetailsAction','600B7E8D-98B7-441A-A53A-80DCE239A0D1'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMClosePackagesPopupAction','C778349B-BF3B-46B1-8C03-29E3A13D1FD3'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMVoidPackagesPopupAction','717B1E47-2F60-4569-9080-FC8E46A2F8C3'
	EXEC addSourcePageToActionDef 'ActivationSearchMultiple_VP', 'CMOpenPackagesPopupAction','0D7590D1-EB3D-4883-BB84-D264F0D1F0E8'

	--Page Action for XceleratorShare
	EXEC createAction 'QOUploadToXceleratorShare', 'QualityActions', 2, 'Quality Object Upload To Xcelerator Share', 'UIFloatPageOpenAction', 'AttachToXceleratorShare_VP',
	NULL, 0, 9, NULL, 'Action_UploadToXceleratorShare_Popup', NULL, 1, 1, 0, 650, 500, 0
	EXEC addSourcePageToActionDef 'GenericEventManage_VP', 'QOUploadToXceleratorShare','FE260C7E-EB9E-4E33-BCED-A2837A45F10C'
	EXEC addSourcePageToActionDef 'ProductionEventManage_VP', 'QOUploadToXceleratorShare','6269633E-53F0-4044-A708-95F51A82FBD2'
	EXEC addActionRuleToActionDef 'QOClosedRestrictRule', 'QOUploadToXceleratorShare'
	EXEC addActionRuleToActionDef 'QOIsOwnerRule', 'QOUploadToXceleratorShare'
END
GO

DROP PROCEDURE createActionCategory
DROP PROCEDURE createActionRule
DROP PROCEDURE createActionDef
DROP PROCEDURE createUIAction
DROP PROCEDURE createAction
DROP PROCEDURE addActionRuleToActionDef
DROP PROCEDURE addSourcePageToActionDef
DROP PROCEDURE cleanupExistingSystemRecords

GO
