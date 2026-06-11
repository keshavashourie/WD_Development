-- Copyright Siemens 2023  
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiWorkflowBOPOverlay' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiWorkflowBOPOverlay
GO
CREATE PROCEDURE csiWorkflowBOPOverlay @p_WorkflowName nvarchar(100), @p_WorkflowRevision int, @p_BillOfProcessName nvarchar(100), @p_BillOfProcessRevision int
AS
	DECLARE cBOP CURSOR LOCAL 
	FOR 
		SELECT BOPO.BillOfProcessOverrideName
			  ,S.SpecId
			  ,BOPO.ElectronicProcedureBaseId
			  ,BOPO.ElectronicProcedureId
			  ,BOPO.DocumentSetId
			  ,BOPO.RecipeFileBaseId
			  ,BOPO.RecipeFileId
			  ,BOPO.SetupId
			  ,BOPO.SetupBaseId
			  ,BOPO.TrainingReqGroupId
			  ,BOPO.ResourceGroupId
		FROM BillOfProcessBase BOPB
			,BillOfProcess BOP
			,BillOfProcessOverride BOPO
			,Spec S
			,SpecBase SB
		WHERE BOPB.BillOfProcessName = @p_BillOfProcessName
		AND ((BOP.Revision = @p_BillOfProcessRevision AND @p_BillOfProcessRevision IS NOT NULL) OR (BOPB.RevOfRcdId=BOP.BillOfProcessId))
		AND BOPO.BillOfProcessId = BOP.BillOfProcessId
		AND ((BOPO.SpecId = S.SpecId AND SB.SpecBaseId = S.SpecBaseId) OR (BOPO.SpecBaseId = SB.SpecBaseId AND S.SpecId = SB.RevOfRcdId))
		
	DECLARE @vBillOfProcessOverrideName nvarchar(100)	
	DECLARE @vSpecId char(16)
	--DECLARE @vSpecName nvarchar(255)
	DECLARE @vElectronicProcedureBaseId char(16)
	DECLARE @vElectronicProcedureId char(16)
	DECLARE @vDocumentSetId char(16)
	DECLARE @vRecipeFileBaseId char(16)
	DECLARE @vRecipeFileId char(16)
	DECLARE @vSetupId char(16)
	DECLARE @vSetupBaseId char(16)
	DECLARE @vTrainingReqGroupId char(16)
	DECLARE @vResourceGroupId char(16)
BEGIN
   SELECT WFS.Sequence
		  ,WFS.WorkflowStepName
		  ,WFS.SubWorkflowBaseId
		  ,WFS.SubWorkflowId
		  ,S.SpecId
		  ,SB.SpecName
		  ,CAST(NULL AS nvarchar(100)) As BillOfProcessOverrideName
		  ,S.ElectronicProcedureBaseId
		  ,S.ElectronicProcedureId
		  ,S.DocumentSetId
		  ,S.RecipeFileBaseId
		  ,S.RecipeFileId
		  ,S.SetupId
		  ,S.SetupBaseId
		  ,S.TrainingReqGroupId
		  ,S.ResourceGroupId
    INTO #WorkflowBOP
	FROM WorkflowBase WFB
		,Workflow WF
		,WorkflowStep WFS
		,SpecBase SB
		,Spec S
	WHERE WFB.WorkflowName = @p_WorkflowName
	AND WF.WorkflowBaseId = WFB.WorkflowBaseId
	AND ((WF.WorkflowRevision = @p_WorkflowRevision AND @p_WorkflowRevision IS NOT NULL) OR (WFB.RevOfRcdId=WF.WorkflowId))
	AND WFS.WorkflowId = WF.WorkflowId
	AND ((WFS.SpecId = S.SpecId AND SB.SpecBaseId = S.SpecBaseId) OR (WFS.SpecBaseId = SB.SpecBaseId AND S.SpecId = SB.RevOfRcdId))
	ORDER BY WFS.Sequence;
		
	OPEN cBOP 
	--
	FETCH NEXT FROM cBOP INTO @vBillOfProcessOverrideName
							,@vSpecId
							,@vElectronicProcedureBaseId
							,@vElectronicProcedureId
							,@vDocumentSetId
							,@vRecipeFileBaseId
							,@vRecipeFileId
							,@vSetupId
							,@vSetupBaseId
							,@vTrainingReqGroupId
							,@vResourceGroupId
	WHILE (@@FETCH_STATUS=0) 
	BEGIN
	   UPDATE #WorkflowBOP
	   SET BillOfProcessOverrideName = @vBillOfProcessOverrideName
	      ,ElectronicProcedureBaseId = ISNULL(@vElectronicProcedureBaseId,ElectronicProcedureBaseId)
		  ,ElectronicProcedureId = ISNULL(@vElectronicProcedureId,ElectronicProcedureId)
		  ,DocumentSetId = ISNULL(@vDocumentSetId,DocumentSetId)
		  ,RecipeFileBaseId = ISNULL(@vRecipeFileBaseId,RecipeFileBaseId)
		  ,RecipeFileId = ISNULL(@vRecipeFileId,RecipeFileId)
		  ,SetupId = ISNULL(@vSetupId,SetupId)
		  ,SetupBaseId = ISNULL(@vSetupBaseId,SetupBaseId)
		  ,TrainingReqGroupId = ISNULL(@vTrainingReqGroupId,TrainingReqGroupId)
		  ,ResourceGroupId = ISNULL(@vResourceGroupId,ResourceGroupId)
	   WHERE SpecId = @vSpecId
	   
	   FETCH NEXT FROM cBOP INTO @vBillOfProcessOverrideName
							,@vSpecId
							,@vElectronicProcedureBaseId
							,@vElectronicProcedureId
							,@vDocumentSetId
							,@vRecipeFileBaseId
							,@vRecipeFileId
							,@vSetupId
							,@vSetupBaseId
							,@vTrainingReqGroupId
							,@vResourceGroupId
	END
	CLOSE cBOP;
	DEALLOCATE cBOP;
	SELECT * FROM #WorkflowBOP;
END
GO
--EXEC csiWorkflowBOPOverlay 'Standard', null, 'BarryBOP', 1
--GO
