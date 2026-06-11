
DECLARE
	--
	v_TableFound		VARCHAR2(30) := 'FALSE';
	--
BEGIN
	--
	SELECT 'TRUE'
	  INTO v_TableFound
	  FROM USER_TABLES
	 WHERE Table_Name = 'WORKFLOWBOP';
	--
	DBMS_OUTPUT.PUT_LINE('WORKFLOWBOP Table Found: '||v_TableFound);
	--
EXCEPTION
	WHEN NO_DATA_FOUND THEN
		--
		DBMS_OUTPUT.PUT_LINE('WORKFLOWBOP Table Found: '||v_TableFound);
		--
		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE WORKFLOWBOP ( SEQUENCE NUMBER,
									       WORKFLOWSTEPNAME VARCHAR2(100),
									       SUBWORKFLOWBASEID CHAR(16),
                         SUBWORKFLOWID CHAR(16),
                         SPECID CHAR(16),
                         SPECNAME VARCHAR2(100),
                         BILLOFPROCESSOVERRIDENAME VARCHAR2(100),
                         ELECTRONICPROCEDUREBASEID CHAR(16),
                         ELECTRONICPROCEDUREID CHAR(16),
                         DOCUMENTSETID CHAR(16),
                         RECIPEFILEBASEID CHAR(16),
                         RECIPEFILEID CHAR(16),
                         SETUPID CHAR(16),
                         SETUPBASEID CHAR(16),
                         TRAININGREQGROUPID CHAR(16),
                         RESOURCEGROUPID CHAR(16)                         
                         )
						   ON COMMIT PRESERVE ROWS';
		--
	WHEN OTHERS THEN
		--
		DBMS_OUTPUT.PUT_LINE('Error creating WORKFLOWBOP table: '||SQLERRM);
		--
		RAISE_APPLICATION_ERROR(-20001,'Error retrieving WORKFLOWBOP table information: '||SQLERRM);
		--
END;
/
CREATE or REPLACE PACKAGE csiWorkflowBOPOverlay_data AS
TYPE BOPCurTyp IS REF CURSOR RETURN
WORKFLOWBOP%ROWTYPE;
END csiWorkflowBOPOverlay_data;
/
CREATE OR REPLACE 
PROCEDURE csiWorkflowBOPOverlay( p_WorkflowName VARCHAR2, p_WorkflowRevision NUMBER, p_BillOfProcessName VARCHAR2, p_BillOfProcessRevision NUMBER, p_cvBOP IN OUT csiWorkflowBOPOverlay_data.BOPCurTyp )
AS
-----------------------------------------------------------------------------------------------
-- This procedure will overlay all BillOfProcess overrides onto the given Workflow
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------
	n_ErrLocator        NUMBER;
  v_ErrMsg            VARCHAR2(512);
  CURSOR cBOP IS
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
		WHERE UPPER(BOPB.BillOfProcessName) = UPPER(p_BillOfProcessName)
		AND ((BOP.Revision = p_BillOfProcessRevision AND p_BillOfProcessRevision IS NOT NULL) OR (BOPB.RevOfRcdId=BOP.BillOfProcessId))
		AND BOPO.BillOfProcessId = BOP.BillOfProcessId
		AND ((BOPO.SpecId = S.SpecId AND SB.SpecBaseId = S.SpecBaseId) OR (BOPO.SpecBaseId = SB.SpecBaseId AND S.SpecId = SB.RevOfRcdId));
BEGIN
     n_ErrLocator := 10;
     EXECUTE IMMEDIATE 'TRUNCATE TABLE WORKFLOWBOP';
     --
     n_ErrLocator := 20;
     INSERT INTO WORKFLOWBOP SELECT WFS.Sequence
        ,WFS.WorkflowStepName
        ,WFS.SubWorkflowBaseId
        ,WFS.SubWorkflowId
        ,S.SpecId
        ,SB.SpecName
        ,NULL BillOfProcessOverrideName
        ,S.ElectronicProcedureBaseId
        ,S.ElectronicProcedureId
        ,S.DocumentSetId
        ,S.RecipeFileBaseId
        ,S.RecipeFileId
        ,S.SetupId
        ,S.SetupBaseId
        ,S.TrainingReqGroupId
        ,S.ResourceGroupId
    FROM WorkflowBase WFB
      ,Workflow WF
      ,WorkflowStep WFS
      ,SpecBase SB
      ,Spec S
    WHERE UPPER(WFB.WorkflowName) = UPPER(p_WorkflowName)
    AND WF.WorkflowBaseId = WFB.WorkflowBaseId
    AND ((WF.WorkflowRevision = p_WorkflowRevision AND p_WorkflowRevision IS NOT NULL) OR (WFB.RevOfRcdId=WF.WorkflowId))
    AND WFS.WorkflowId = WF.WorkflowId
    AND ((WFS.SpecId = S.SpecId AND SB.SpecBaseId = S.SpecBaseId) OR (WFS.SpecBaseId = SB.SpecBaseId AND S.SpecId = SB.RevOfRcdId))
    ORDER BY WFS.Sequence;
    --
    n_ErrLocator := 30;
    FOR crec IN cBOP LOOP
      UPDATE WORKFLOWBOP
       SET BillOfProcessOverrideName = crec.BillOfProcessOverrideName
          ,ElectronicProcedureBaseId = NVL(crec.ElectronicProcedureBaseId,ElectronicProcedureBaseId)
          ,ElectronicProcedureId = NVL(crec.ElectronicProcedureId,ElectronicProcedureId)
          ,DocumentSetId = NVL(crec.DocumentSetId,DocumentSetId)
          ,RecipeFileBaseId = NVL(crec.RecipeFileBaseId,RecipeFileBaseId)
          ,RecipeFileId = NVL(crec.RecipeFileId,RecipeFileId)
          ,SetupId = NVL(crec.SetupId,SetupId)
          ,SetupBaseId = NVL(crec.SetupBaseId,SetupBaseId)
          ,TrainingReqGroupId = NVL(crec.TrainingReqGroupId,TrainingReqGroupId)
          ,ResourceGroupId = NVL(crec.ResourceGroupId,ResourceGroupId)
       WHERE SpecId = crec.SpecId;
    END LOOP;
    --
    n_ErrLocator := 40;
    OPEN p_cvBOP FOR SELECT * FROM WORKFLOWBOP;
EXCEPTION
    WHEN OTHERS THEN
        --
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('OTHERS Error - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
        --
END;
/
/*
declare
   p_cvBOP csiWorkflowBOPOverlay_data.BOPCurTyp;
BEGIN
csiWorkflowBOPOverlay('A_Workflow',1,'BarryBOP',1,p_cvBOP);
END;
/

*/
