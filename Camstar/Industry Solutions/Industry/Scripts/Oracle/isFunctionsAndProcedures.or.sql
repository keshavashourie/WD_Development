/*
-- SCRIPT:isFunctionsAndProcedures.or.sql
-- DESCR: 
-- 
--  Copyright Siemens 2020  
*/
--#delimiter
CALL DROP_DATABASE_OBJECT('isDefectRepairs', 'VIEW')
--#delimiter
CREATE OR REPLACE VIEW isDefectRepairs
AS
    SELECT 
		dhd.isDefectHistoryDetailId, 
		dov.ParentId as ChildId,
		ra.isRepairActionId,
		ra.isRepairActionName,
		hm.ProductId,
		dhd.isRefDes,
		dhd.isX,
		dhd.isY,
		dhd.isDefectReasonId
	from
		isDefectHistoryDetail dhd
		left join isDefectHistory dh on dhd.DefectHistoryId = dh.isDefectHistoryId
		left join HistoryMainline hm on dh.HistoryMainlineId = hm.HistoryMainlineId
		left join isDefectOldValue dov on dhd.isDefectHistoryDetailId = dov.isDefectHistoryDetailId
		left join isRepairActionHistDetails rahd on rahd.isDefectHistoryDetailId = dhd.isDefectHistoryDetailId
		left join isRepairAction ra on ra.isRepairActionId = rahd.isRepairActionId
	where
		dhd.isStatus = 1   -- 1=Repaired
		and isRepairActionName IS NOT NULL
--#delimiter
CALL DROP_DATABASE_OBJECT('isDefectHistoryView', 'VIEW')
--#delimiter
CREATE OR REPLACE VIEW isDefectHistoryView
AS
	SELECT
		hm.TxnDate, 
		dhd.isDefectHistoryDetailId, 
		dov.ParentId AS ChildId, 
		hm.ProductId, 
		dhd.isRefDes, 
		dhd.isX, 
		dhd.isY, 
		dhd.isDefectReasonId, 
		isdf.isDefectReasonName, 
		isdf.Description, dhd.isInspectNote
	FROM
		isDefectHistoryDetail dhd
		left join isDefectHistory dh on dhd.DefectHistoryId = dh.isDefectHistoryId
		left join HistoryMainline hm on dh.HistoryMainlineId = hm.HistoryMainlineId
		left join isDefectOldValue dov on dhd.isDefectHistoryDetailId = dov.isDefectHistoryDetailId
		left join isDefectReason isdf on isdf.isDefectReasonId = dhd.isDefectReasonId
	WHERE 
		dov.isDefectOldValueId is not null
		and hm.TxnType IN (select CDODefID from CDODefinition where CDOName in ('isDefect','isDefectUpdate'))
--#delimiter
CALL DROP_DATABASE_OBJECT ( 'isGetRecipeFromMatrix', 'FUNCTION' )
--#delimiter
CALL DROP_DATABASE_OBJECT ( 'otab_RecipeFromMatrix', 'TYPE' )
--#delimiter
CALL DROP_DATABASE_OBJECT ( 'otyp_RecipeFromMatrix', 'TYPE' )
--#delimiter
CREATE OR REPLACE 
TYPE otyp_RecipeFromMatrix AS OBJECT (Recipe VARCHAR2(100))
--#delimiter
CREATE OR REPLACE 
TYPE otab_RecipeFromMatrix AS TABLE OF otyp_RecipeFromMatrix
--#delimiter
CREATE OR REPLACE
FUNCTION isGetRecipeFromMatrix( p_ResourceName IN VARCHAR2, p_SpecId IN VARCHAR2, p_RecipePlanId IN VARCHAR2 )
RETURN otab_RecipeFromMatrix
AS
    v_results_t otab_RecipeFromMatrix := otab_RecipeFromMatrix();
    
    v_ResourceId CHAR(16);
    v_P1Recipe VARCHAR2(100);
    v_P2Recipe VARCHAR2(100);
    v_P3Recipe VARCHAR2(100);
    v_P4Recipe VARCHAR2(100);
    v_Recipe VARCHAR2(100);
BEGIN
    IF (p_ResourceName is not null) THEN
        SELECT ResourceId INTO v_ResourceId FROM ResourceDef WHERE ResourceName = p_ResourceName;
    END IF;
    
    FOR detailItem IN (
		SELECT
			rpd.ResourceId, rpd.SpecId, rpd.SpecBaseId, db.DocumentName || ':' || d.DocumentRevision AS Recipe, sb.RevOfRcdId 
		FROM
			isRecipePlanDetails rpd
			LEFT JOIN Document d ON
				(rpd.RecipeId <> '0000000000000000' AND rpd.RecipeId = d.DocumentId) OR
				(rpd.RecipeBaseId <> '0000000000000000' AND d.DocumentId IN (SELECT RevOfRcdId FROM DocumentBase WHERE DocumentBaseId = rpd.RecipeBaseId))
			LEFT JOIN DocumentBase db ON db.DocumentBaseId = d.DocumentBaseId
            LEFT JOIN SpecBase sb on rpd.SpecBaseId <> '0000000000000000' AND rpd.SpecBaseId = sb.SpecBaseId
		WHERE
			rpd.isRecipePlanId = p_RecipePlanId
    )
    LOOP
        IF (detailItem.ResourceId is not null AND detailItem.SpecId is not null AND detailItem.ResourceId = v_ResourceId AND (
            (detailItem.SpecId <> '0000000000000000' AND detailItem.SpecId = p_SpecId) OR
            (detailItem.SpecBaseId <> '0000000000000000' AND p_SpecId = detailItem.RevOfRcdId)))
            THEN v_P1Recipe := detailItem.Recipe;
        ELSIF (detailItem.ResourceId is not null AND detailItem.SpecId is null AND detailItem.ResourceId = v_ResourceId )
            THEN v_P2Recipe := detailItem.Recipe;
        ELSIF (detailItem.ResourceId is null AND detailItem.SpecId is not null AND (
            (detailItem.SpecId <> '0000000000000000' AND detailItem.SpecId = p_SpecId) OR
            (detailItem.SpecBaseId <> '0000000000000000' AND p_SpecId = detailItem.RevOfRcdId)))
            THEN v_P3Recipe := detailItem.Recipe;
        ELSIF (detailItem.ResourceId is null AND detailItem.SpecId is null) 
            THEN v_P4Recipe := detailItem.Recipe;
        END IF;
    END LOOP;
    
    IF v_P1Recipe is not null THEN v_Recipe := v_P1Recipe;
    ELSIF v_P2Recipe is not null THEN v_Recipe := v_P2Recipe;
    ELSIF v_P3Recipe is not null THEN v_Recipe := v_P3Recipe;
    ELSIF v_P4Recipe is not null THEN v_Recipe := v_P4Recipe;
    END IF; 
    
    IF v_Recipe is not null THEN
        v_results_t.extend();
        v_results_t(v_results_t.count) := otyp_RecipeFromMatrix(v_Recipe);
    END IF;
    
    RETURN (v_results_t);
END;
--#delimiter

-- Make sure functions and types we depend on are dropped and recreated

CALL DROP_DATABASE_OBJECT('isDefectReopened', 'FUNCTION')
--#delimiter
CALL DROP_DATABASE_OBJECT('isREPAIR_ACTION_COUNT_TABLE', 'TYPE')
--#delimiter
CALL DROP_DATABASE_OBJECT('isREPAIR_ACTION_COUNT_OBJ', 'TYPE')
--#delimiter
CREATE OR REPLACE TYPE isREPAIR_ACTION_COUNT_OBJ AS OBJECT (
	RepairActionId CHAR(16),
	RepairActionName VARCHAR(30),
	SuccessCount DECIMAL,
	FailCount DECIMAL
)
--#delimiter
CREATE TYPE isREPAIR_ACTION_COUNT_TABLE AS TABLE OF isREPAIR_ACTION_COUNT_OBJ
--#delimiter
-- Returns 1 or 0 to indicate if the given defect was reopened after it was repaired
CREATE OR REPLACE FUNCTION isDefectReopened
(	
	p_isDefectHistoryDetailId IN VARCHAR,
	p_isDefectHistoryDetailChildId IN VARCHAR,
	p_isStatus IN number
)
RETURN number
AS
	nextDefectHistoryDetailChildId char(16);
	nextStatus number;
    recursiveReturn number;
BEGIN
	-- If there are no more descendants 
	IF(p_isDefectHistoryDetailChildId IS NULL) THEN
		IF p_isStatus = 1 THEN
			RETURN 0;		-- the defect status is repaired, we conclude it wasn't reopened
		ELSE
			RETURN 1;		-- defect status is open
		END IF;
	END IF;

	-- There are more children, but this version of the defect is open
	IF(p_isStatus = 0) THEN
		RETURN 1;
	END IF;

	-- Load child record and check it
	SELECT 
		 dov.ParentId,
         nextDhd.isStatus
	INTO
        nextDefectHistoryDetailChildId,
        nextStatus
    FROM 
		isDefectHistoryDetail nextDhd  
		left join isDefectOldValue dov on dov.isDefectHistoryDetailId = nextDhd.isDefectHistoryDetailId
	WHERE
		nextDhd.isDefectHistoryDetailId = p_isDefectHistoryDetailChildId
	;

    recursiveReturn := ISDefectReopened(p_isDefectHistoryDetailChildId, nextDefectHistoryDetailChildId, nextStatus);
	RETURN recursiveReturn;

END;
--#delimiter
-- Return table for number of times the given defect reason was repaired with a repair action at the given location
create or replace FUNCTION isGetRepairActionCounts
(	
	p_DefectReasonId VARCHAR,
	p_ContainerProductId VARCHAR,
	p_RefDes VARCHAR,
	p_X VARCHAR,
	p_Y VARCHAR
)
RETURN isREPAIR_ACTION_COUNT_TABLE
AS
    TYPE reopened_counts_type IS TABLE OF NUMBER  -- Associative array  - number is true/false if defect has been reopened
    INDEX BY VARCHAR2(16);                        --  indexed by string - DefectHistoryDetailId
    reopened_counts reopened_counts_type;        -- Associative array variable
    
    TYPE ra_counts_type IS TABLE OF ISREPAIR_ACTION_COUNT_OBJ
    INDEX BY VARCHAR2(16);            --  indexed by string
    ra_counts ra_counts_type;
    
    RepairActionCounts isREPAIR_ACTION_COUNT_TABLE := ISREPAIR_ACTION_COUNT_TABLE();   -- return value

    defectReopened INT := 0;  -- was the defect we are currently considering ever reopened?
    repActionIdIterator VARCHAR2(16);
    
	-- Get details of repaired defects
    CURSOR defHistDetCursor IS
        SELECT
            isDefectHistoryDetailId, 
            ChildId,
            isRepairActionId,
            isRepairActionName
        FROM
            isDefectRepairs
        WHERE
            ProductId = (select ProductId from container where containername = p_ContainerProductId)
			and (p_RefDes is NULL OR isRefDes = p_RefDes)
			and (p_X IS NULL OR p_X = '0' OR isX = p_X)
			and (p_Y IS NULL OR p_Y = '0' OR isY = p_Y)
			and isDefectReasonId = p_DefectReasonId
        ;
BEGIN
    -- check each repaired defect - perhaps it was reopened
    FOR defHistDet IN defHistDetCursor
    LOOP
		-- See if we already checked if this defect was reopened.  Try to limit use of recursive function
        IF (reopened_counts.exists(defHistDet.isDefectHistoryDetailId)) THEN
			-- Use the value we previously determined
            defectReopened := reopened_counts(defHistDet.isDefectHistoryDetailId);
        ELSE
			-- Call the recursive function to determine if defect was reopened
			defectReopened := isDefectReopened(defHistDet.isDefectHistoryDetailId, defHistDet.ChildId, 1);
            reopened_counts(defHistDet.isDefectHistoryDetailId) := defectReopened;
        END IF;
        
        IF ra_counts.exists(defHistDet.isRepairActionId) THEN
            -- already have an entry for this repair action.  Increment count
            IF (defectReopened = 0) THEN
                ra_counts(defHistDet.isRepairActionId).SuccessCount := ra_counts(defHistDet.isRepairActionId).SuccessCount + 1;
            ELSE
                ra_counts(defHistDet.isRepairActionId).FailCount := ra_counts(defHistDet.isRepairActionId).FailCount + 1;
            END IF;
        ELSE
            IF (defectReopened = 0) THEN
                ra_counts(defHistDet.isRepairActionId) := ISREPAIR_ACTION_COUNT_OBJ (defHistDet.isRepairActionId, defHistDet.isRepairActionName, 1, 0);
            ELSE
                ra_counts(defHistDet.isRepairActionId) := ISREPAIR_ACTION_COUNT_OBJ (defHistDet.isRepairActionId, defHistDet.isRepairActionName, 0, 1);
            END IF;
        END IF;
    END LOOP;
    
    -- copy into array to return
    repActionIdIterator := ra_counts.first;
    while(repActionIdIterator is not null)
    LOOP
        RepairActionCounts.extend;
        RepairActionCounts(RepairActionCounts.count) := ra_counts(repActionIdIterator);

        repActionIdIterator := ra_counts.next(repActionIdIterator);
    END LOOP;
    
	RETURN RepairActionCounts;
END;
--#delimiter
CALL DROP_DATABASE_OBJECT('isGetDefectHistory', 'FUNCTION')
--#delimiter
CALL DROP_DATABASE_OBJECT('isDEFECT_HISTORY_TBL', 'TYPE')
--#delimiter
CALL DROP_DATABASE_OBJECT('isDEFECT_HISTORY_OBJ', 'TYPE')
--#delimiter
CREATE OR REPLACE TYPE isDEFECT_HISTORY_OBJ AS OBJECT (
	OldDefectReasonId CHAR(16),
	OldDefectReasonName VARCHAR2(100),
	OldDefectDescription VARCHAR2(100),
	Quantity INTEGER,
	CausePercentage FLOAT
)
--#delimiter
CREATE OR REPLACE TYPE isDEFECT_HISTORY_TBL AS TABLE OF isDEFECT_HISTORY_OBJ
--#delimiter
CREATE OR REPLACE FUNCTION isGetDefectHistory
(	
	p_ContainerProductId VARCHAR,
	p_RefDes VARCHAR,
	p_X VARCHAR,
	p_Y VARCHAR
)
RETURN isDEFECT_HISTORY_TBL
AS

  DefectHistoryTbl isDEFECT_HISTORY_TBL := isDEFECT_HISTORY_TBL();   -- return value
  
  TOTALRECORDS INTEGER := 0;
      
  CURSOR defHistDetCursor IS
  	SELECT
      isDefectReasonId,
      isDefectReasonName,
      Description,
      COUNT(*) AS SINGLECOUNT,
      (COUNT(*) * 100.0 / TOTALRECORDS) AS PERCENTAGE
    FROM
      isDefectHistoryView
    WHERE
      ProductId = (select ProductId from container where containername = p_ContainerProductId)
		    and (p_RefDes IS NULL OR isRefDes = p_RefDes)
			and (p_X IS NULL OR p_X = '0' OR isX = p_X)
			and (p_Y IS NULL OR p_Y = '0' OR isY = p_Y)
    GROUP BY isDefectReasonId,
      isDefectReasonName,
      Description;

BEGIN

	SELECT COUNT(*)
    INTO TOTALRECORDS
	FROM
      isDefectHistoryView
    WHERE
      ProductId = (select ProductId from container where containername = p_ContainerProductId)
		    and (p_RefDes IS NULL OR isRefDes = p_RefDes)
			and (p_X IS NULL OR p_X = '0' OR isX = p_X)
			and (p_Y IS NULL OR p_Y = '0' OR isY = p_Y);

    FOR defHistDet IN defHistDetCursor
    LOOP
       DefectHistoryTbl.extend;
       DefectHistoryTbl(DefectHistoryTbl.count) := isDEFECT_HISTORY_OBJ(defHistDet.isDefectReasonId,defHistDet.isDefectReasonName,defHistDet.Description,defHistDet.SINGLECOUNT,(defHistDet.SINGLECOUNT * 100.0 / TOTALRECORDS));
    END LOOP;

	RETURN DefectHistoryTbl;

END;
--#delimiter
