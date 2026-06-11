--------------------------------------------------------------------------------
-- SCRIPT:isGetRecipeFromMatrix.sql
-- DESCR: Defines table value function to get a recipe from a recipe matrix
--
-- Copyright Siemens 2022

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isGetRecipeFromMatrix' 
	   AND 	  type = 'TF')
    DROP FUNCTION isGetRecipeFromMatrix
GO

CREATE FUNCTION isGetRecipeFromMatrix
(	
	@ResourceName NVARCHAR(30),
	@SpecId CHAR(16),
	@RecipePlanId CHAR(16)
)
RETURNS @Recipe TABLE
(
	Recipe NVARCHAR(100)
)
AS
BEGIN
	DECLARE @ResourceId CHAR(16)
	SELECT @ResourceId = ResourceId FROM ResourceDef WHERE ResourceName = @ResourceName

	DECLARE @TempRecipes TABLE
	(
		Recipe NVARCHAR(100),
		PlanPriority INT
	)

	DECLARE @PlanResourceId CHAR(16)
	DECLARE @PlanSpecId CHAR(16)
	DECLARE @PlanSpecBaseId CHAR(16)
	DECLARE @PlanDocumentId CHAR(16)
	DECLARE @PlanRecipe NVARCHAR(100)

	DECLARE csr CURSOR FOR 
		SELECT
			rpd.ResourceId, rpd.SpecId, rpd.SpecBaseId, db.DocumentName + ':' + d.DocumentRevision AS Recipe
		FROM
			isRecipePlanDetails rpd
			LEFT JOIN Document d ON
				(rpd.RecipeId <> '0000000000000000' AND rpd.RecipeId = d.DocumentId) OR
				(rpd.RecipeBaseId <> '0000000000000000' AND d.DocumentId IN (SELECT RevOfRcdId FROM DocumentBase WHERE DocumentBaseId = rpd.RecipeBaseId))
			LEFT JOIN DocumentBase db ON db.DocumentBaseId = d.DocumentBaseId
		WHERE
			rpd.isRecipePlanId = @RecipePlanId
		
		OPEN csr
		FETCH NEXT FROM csr into @PlanResourceId, @PlanSpecId, @PlanSpecBaseId, @PlanRecipe
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF(@PlanResourceId IS NOT NULL AND @PlanSpecId IS NOT NULL AND @PlanResourceId = @ResourceId AND (
				(@PlanSpecId <> '0000000000000000' AND @PlanSpecId = @SpecId) OR
				(@PlanSpecBaseId <> '0000000000000000' AND @SpecId IN (SELECT RevOfRcdId FROM SpecBase WHERE SpecBaseId = @PlanSpecBaseId))))
				INSERT INTO @TempRecipes VALUES (@PlanRecipe, 1)
			ELSE IF(@PlanResourceId IS NOT NULL AND @PlanSpecId IS NULL AND @PlanResourceId = @ResourceId)
				INSERT INTO @TempRecipes VALUES (@PlanRecipe, 2)
			ELSE IF(@PlanResourceId IS NULL AND @PlanSpecId IS NOT NULL AND (
				(@PlanSpecId <> '0000000000000000' AND @PlanSpecId = @SpecId) OR
				(@PlanSpecBaseId <> '0000000000000000' AND @SpecId IN (SELECT RevOfRcdId FROM SpecBase WHERE SpecBaseId = @PlanSpecBaseId))))
				INSERT INTO @TempRecipes VALUES (@PlanRecipe, 3)
			ELSE IF(@PlanResourceId IS NULL AND @PlanSpecId IS NULL)
				INSERT INTO @TempRecipes VALUES (@PlanRecipe, 4)
			FETCH NEXT FROM csr into @PlanResourceId, @PlanSpecId, @PlanSpecBaseId, @PlanRecipe
		END
		CLOSE csr
		DEALLOCATE csr

		-- get the recipe with top priority
		INSERT INTO @Recipe SELECT Recipe FROM @TempRecipes WHERE PlanPriority = (Select MIN(PlanPriority) FROM @TempRecipes)

	RETURN
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isDefectReopened' 
	   AND 	  type = 'FN')
    DROP FUNCTION isDefectReopened
GO

CREATE FUNCTION isDefectReopened
(	
	@isDefectHistoryDetailId char(16),
	@isDefectHistoryDetailChildId char(16),
	@isStatus INT
)
RETURNS INT
AS
BEGIN
	-- If there are no more descendants 
	IF(@isDefectHistoryDetailChildId IS NULL)
		IF @isStatus = 1  
			RETURN 0		-- the defect status is repaired, we conclude it wasn't reopened
		ELSE
			RETURN 1		-- defect status is open

	-- There are more children, but this version of the defect is open
	IF(@isStatus = 0)
		RETURN 1

	DECLARE @nextDefectHistoryDetailChildId char(16)
	DECLARE @nextStatus INT
	
	-- Load child record and check it
	SELECT 
		@nextDefectHistoryDetailChildId=dov.ParentId
		,@nextStatus = nextDhd.isStatus
	FROM 
		isDefectHistoryDetail nextDhd  
		left join isDefectOldValue dov on dov.isDefectHistoryDetailId = nextDhd.isDefectHistoryDetailId
	WHERE
		nextDhd.isDefectHistoryDetailId = @isDefectHistoryDetailChildId
	
	RETURN {Schema}.isDefectReopened(@isDefectHistoryDetailChildId, @nextDefectHistoryDetailChildId, @nextStatus)

END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isGetRepairActionCounts' 
	   AND 	  type = 'TF')
    DROP FUNCTION isGetRepairActionCounts
GO

CREATE FUNCTION isGetRepairActionCounts
(	
	@DefectReasonId CHAR(16),
	@ContainerProductId CHAR(16),
	@RefDes NVARCHAR(30),
	@X NVARCHAR(30),
	@Y NVARCHAR(30)
)
RETURNS @RepairActionCounts TABLE
(
	RepairActionId CHAR(16),
	RepairActionName NVARCHAR(30),
	SuccessCount DECIMAL,
	FailCount DECIMAL
)
AS
BEGIN
	-- Keep track of defect history detail records whose defect was later reopened
	DECLARE @reopened TABLE
	(
		isDefectHistoryDetailId char(16) NOT NULL,
		wasReopened int NULL
	)

	DECLARE @defHistDet TABLE
	(
		isDefectHistoryDetailId char(16) NOT NULL,
		isChildDefectHistoryDetailId char(16) NULL,
		isRepairActionId char(16) NULL,
		isRepairActionName nvarchar(30) NULL
	);

	-- Get details of repaired defects
	INSERT INTO @defHistDet(isDefectHistoryDetailId, isChildDefectHistoryDetailId, isRepairActionId, isRepairActionName)
	select
		isDefectHistoryDetailId, 
		ChildId,
		isRepairActionId,
		isRepairActionName
	from
		isDefectRepairs
	where
		ProductId = (select ProductId from container where containername = @ContainerProductId)
		and (@RefDes = '' OR isRefDes = @RefDes)
		and (@X = '' OR isX = @X)
		and (@Y = '' OR isY = @Y)
		and isDefectReasonId = @DefectReasonId
	
	-- check each repaired defect - perhaps it was reopened
	DECLARE @isDefectHistoryDetailId char(16)
	DECLARE @isChildDefectHistoryDetailId char(16)
	DECLARE @isRepairActionId char(16)
	DECLARE @isRepairActionName nvarchar(30)

	DECLARE @defectReopened int
	
	DECLARE repairedCursor CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
		SELECT isDefectHistoryDetailId, isChildDefectHistoryDetailId, isRepairActionId, isRepairActionName
		from @defHistDet

	OPEN repairedCursor
	
	FETCH NEXT FROM repairedCursor INTO @isDefectHistoryDetailId, @isChildDefectHistoryDetailId, @isRepairActionId, @isRepairActionName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- See if we already checked if this defect was reopened.  Try to limit use of recursive function
		IF EXISTS(select wasReopened from @reopened where isDefectHistoryDetailId=@isDefectHistoryDetailId)
		BEGIN
			-- Use the value we previously determined
			select @defectReopened = wasReopened from @reopened where isDefectHistoryDetailId=@isDefectHistoryDetailId
		END
		ELSE
		BEGIN
			-- Call the recursive function to determine if defect was reopened
			set @defectReopened = {Schema}.isDefectReopened(@isDefectHistoryDetailId, @isChildDefectHistoryDetailId, 1)
			INSERT INTO @reopened VALUES(@isDefectHistoryDetailId, @defectReopened) 
		END

		IF EXISTS (select RepairActionId from @RepairActionCounts where RepairActionId = @isRepairActionId)
			BEGIN
				-- already have an entry for this repair action.  Increment count
				IF(@defectReopened = 0)
					UPDATE @RepairActionCounts 
						SET SuccessCount = (select SuccessCount+1 from @RepairActionCounts where RepairActionId = @isRepairActionId)
						WHERE RepairActionId = @isRepairActionId
				ELSE
					UPDATE @RepairActionCounts 
						SET FailCount = (select FailCount+1 from @RepairActionCounts where RepairActionId = @isRepairActionId)
						WHERE RepairActionId = @isRepairActionId
			END
		ELSE
			BEGIN
				IF(@defectReopened = 0)
					INSERT INTO @RepairActionCounts VALUES(@isRepairActionId, @isRepairActionName, 1, 0)
				ELSE
					INSERT INTO @RepairActionCounts VALUES(@isRepairActionId, @isRepairActionName, 0, 1)
			END

		FETCH NEXT FROM repairedCursor INTO @isDefectHistoryDetailId, @isChildDefectHistoryDetailId, @isRepairActionId, @isRepairActionName
	END

	RETURN
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isGetDefectHistory' 
	   AND 	  type = 'TF')
    DROP FUNCTION isGetDefectHistory
GO

CREATE FUNCTION isGetDefectHistory
(	
	@ContainerProductId CHAR(16),
	@RefDes NVARCHAR(30),
	@X NVARCHAR(30),
	@Y NVARCHAR(30)
)
RETURNS @DefectHistory TABLE
(
	OldDefectReasonId CHAR(16),
	OldDefectReasonName NVARCHAR(100),
	OldDefectDescription NVARCHAR(100),
	Quantity Int,
	CausePercentage Float
)
AS
BEGIN

	DECLARE @TotalRecords int;

	SET  @TotalRecords = (SELECT 
							COUNT(*) 
						  FROM 
							isDefectHistoryView
						  WHERE
							ProductId = (select ProductId from container where containername = @ContainerProductId)
							and (@RefDes = '' OR isRefDes = @RefDes)
							and (@X = '' OR isX = @X)
							and (@Y = '' OR isY = @Y))

	INSERT INTO @DefectHistory(OldDefectReasonId,OldDefectReasonName,OldDefectDescription,Quantity,CausePercentage)
	select
		isDefectReasonId,
		isDefectReasonName,
		Description,
		COUNT(*) As RecordsPerGroup,
		COUNT(*) * 100.0 / @TotalRecords As DefectPercentage
	from
		isDefectHistoryView
	where
		ProductId = (select ProductId from container where containername = @ContainerProductId)
		and (@RefDes = '' OR isRefDes = @RefDes)
		and (@X = '' OR isX = @X)
		and (@Y = '' OR isY = @Y)
	group by isDefectReasonId,
		isDefectReasonName,
		Description

	RETURN
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isDefectRepairs' 
	   AND 	  type = 'V')
    DROP VIEW isDefectRepairs
GO

CREATE VIEW isDefectRepairs
AS
	select
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
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'isDefectHistoryView' 
	   AND 	  type = 'V')
    DROP VIEW isDefectHistoryView
GO

CREATE VIEW isDefectHistoryView
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
		isDefectHistoryDetail AS dhd 
		LEFT JOIN isDefectHistory AS dh ON dhd.DefectHistoryId = dh.isDefectHistoryId 
		LEFT JOIN HistoryMainline AS hm ON dh.HistoryMainlineId = hm.HistoryMainlineId 
		LEFT JOIN isDefectOldValue AS dov ON dhd.isDefectHistoryDetailId = dov.isDefectHistoryDetailId 
		LEFT JOIN isDefectReason AS isdf ON isdf.isDefectReasonId = dhd.isDefectReasonId
	WHERE        
		(dov.isDefectOldValueId IS NOT NULL) AND (hm.TxnType IN (select CDODefID from CDODefinition where CDOName in ('isDefect','isDefectUpdate')))
GO
