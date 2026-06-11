--------------------------------------------------------------------------------
-- SCRIPT: csiGetEnumerationLabels.sql
-- DESCR: 
-- HISTORY:
--	09/21/2011 - Extracted the csiPRDGetNextInstanceId procedure from other script so that this SP could be used elsewhere
--			Ramesh Nagamalli
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS
	    WHERE Name = 'csiGetEnumerationLabels'
	      AND Type = 'TF')
	--
	DROP FUNCTION csiGetEnumerationLabels
	--
GO


CREATE FUNCTION csiGetEnumerationLabels ( 
	@Enumeration nvarchar(30), @PrimaryDictionary nvarchar(16), @SecondaryDictionary nvarchar(16)
) 
RETURNS @retTable TABLE (DefaultValue int, LabelValue nvarchar(255)) 
AS 
BEGIN 
	INSERT INTO @retTable  
	SELECT
		f.DefaultValue,
		COALESCE(Term.labelvalue, Lang.labelvalue, l.labelvalue) LabelValue
	FROM CDOFields f
	JOIN CDODefinition c on c.CDODefID = f.CDODefID
	JOIN Labels l on l.LabelID = f.LabelID
	LEFT JOIN DictionaryLabel Term ON Term.labelid = l.labelid AND Term.dictionaryid = @PrimaryDictionary
	LEFT JOIN DictionaryLabel Lang ON Lang.labelid = l.labelid AND Lang.dictionaryid = @SecondaryDictionary
	WHERE c.CDOName = @Enumeration
 
    RETURN 
END
GO

--------------------------------------------------------------------------------------------------
-- Function to create instance id strings from a CDODefId and Instance Id number
-- DESCR: Helper function to create instance id strings from a CDODefId and
--        Instance Id number
-- 
--  Modification History:
--  Name            	Date        Action
--  --------------      ----------  ----------------
--  Patrick Miller      8/18/2009    Initial Creation
--  Preston Holder      10/13/09     Added check for site, if found include in instanceId.
--
-- Copyright Siemens 2023  
--------------------------------------------------------------------------------------------------
IF EXISTS (SELECT Name
	   FROM   SYSOBJECTS
	   WHERE  Name = 'csiPRDGetNextInstanceId' 
	   AND 	  Type = 'P')
	--
	DROP PROCEDURE csiPRDGetNextInstanceId
	--
GO

CREATE PROCEDURE csiPRDGetNextInstanceId( @p_CDODefId 		INT
						 ,@p_InstanceIdStr 	VARCHAR(16) OUTPUT )
AS
BEGIN
	BEGIN TRY
		--
		SET NOCOUNT ON;
		--
		DECLARE @n_ErrLocator			NUMERIC;
		--
		DECLARE @v_ErrMsg			VARCHAR(1024);
		--
		DECLARE @v_CDODefIdStr 			VARCHAR(16)
		DECLARE @v_InstIdNewValue 		VARCHAR(16)
		DECLARE @i_InstIdInt			BIGINT
		DECLARE @v_HexSite				VARCHAR(16)
		DECLARE @v_HexId				VARCHAR(16)
		DECLARE @v_Query				NVARCHAR(100)
		DECLARE @v_IdCount              INT;
		--
		-- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
		-- Length should be 10 chars
		--
		SET @n_ErrLocator = 5;
		--
		EXEC csiUpdateInstanceID 0,@p_CDODefId,1,@v_InstIdNewValue OUTPUT
		--
		SET @v_InstIdNewValue=SUBSTRING(@v_InstIdNewValue,7,10)
		--
		-- Add the Site
		--
		SET @v_IdCount = 0
		SET @v_IdCount = (SELECT count(*) from DBIdentifier)

		IF(@v_IdCount != 1 )
		   SET @v_HexSite = '0x0';
		ELSE
		   SET @v_HexSite = '0x'+(SELECT dbidentifier FROM DBIdentifier) 
		
        if(@v_HexSite is null)
          SET @v_HexSite = '0x0';

		SET @v_HexId = '0x'+@v_InstIdNewValue
		SET @v_Query=N'Select @Result = CONVERT(BIGINT,'+@v_HexId+') | CONVERT(BIGINT,'+@v_HexSite+')'
		Exec sp_executesql @v_Query, N'@Result bigint output', @i_InstIdInt Output
		SET @v_InstIdNewValue = REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@i_InstIdInt), 1, 2, ''), '0', ' ')), ' ', '0')
		SET @v_InstIdNewValue=REPLICATE('0', (10 - LEN(@v_InstIdNewValue))) + @v_InstIdNewValue
		--
		-- Convert CDODef Id to hex (pad to 6 chars)
		--
		SET @n_ErrLocator = 10;
		--
		SET @v_CDODefIdStr=REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@p_CDODefId), 1, 2, ''), '0', ' ')), ' ', '0')
		SET @v_CDODefIdStr=REPLICATE('0', (6 - LEN(@v_CDODefIdStr))) + @v_CDODefIdStr
		--
		-- Join CDODef hex and instance id hex strings. Length=16 chars
		--
		SET @p_InstanceIdStr = @v_CDODefIdStr + @v_InstIdNewValue
		--
	END TRY
	--
	BEGIN CATCH
		--
		SELECT @v_ErrMsg = 'csiPRDGetNextInstanceId - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
		--
		PRINT @v_ErrMsg;
		--
	END CATCH;
	--
END
GO
--------------------------------------------------------------------------------------------------
-- Function to recursively check if a resource is in a resource group
-- DESCR: Can specify resource and group by ID or name. 
--		  Support for name was mainly a development convenience so can remove if not needed.
-- 
--  Modification History:
--  Name            	Date         Action
--  --------------      -----------  ----------------
--  John Rumpf          26-Jan-2020  Created
--  John Rumpf			26-Apr-2024  Update to return Resource and Resource Group IDs
--
-- Copyright Siemens 2024  
--------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiResourceInGroup'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiResourceInGroup
END
GO
CREATE FUNCTION csiResourceInGroup
(	
	@ResourceId CHAR(16),
	@ResourceName NVARCHAR(30),
	@ResourceGroupId CHAR(16),	
	@ResourceGroupName NVARCHAR(30)
)
RETURNS @returnTable TABLE (Found INT, ResourceName NVARCHAR(30), ResouceId CHAR(16), ResourceGroupId CHAR(16))
AS
BEGIN
	DECLARE @found INT
	DECLARE @ChildGroupId CHAR(16)

	-- support params set by id or name and validate
	IF @ResourceId is null OR @ResourceId = ''
		IF @ResourceName is not null
			SELECT @ResourceId = ResourceId FROM ResourceDef WHERE ResourceName = @ResourceName

	IF @ResourceGroupId is null OR @ResourceGroupId = ''
		IF @ResourceGroupName is not null
			SELECT @ResourceGroupId = ResourceGroupId FROM ResourceGroup WHERE ResourceGroupName = @ResourceGroupName

	IF @ResourceId is null or @ResourceGroupId is null
		RETURN

	IF @ResourceName is null
		SELECT @ResourceName = ResourceName from ResourceDef WHERE ResourceId = @ResourceId

	-- check if the resource is one of the entries of the specified group
	SELECT @found = count(*) FROM ResourceGroupEntries WHERE ResourceGroupId = @ResourceGroupId and EntriesId = @ResourceId

	-- if not, check the child groups of the specified group
	IF @found = 0
	BEGIN
		DECLARE csr CURSOR FOR SELECT GroupsId FROM ResourceGroupGroups WHERE ResourceGroupId = @ResourceGroupId
		OPEN csr
		FETCH NEXT FROM csr into @ChildGroupId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @found = count(*) FROM ResourceGroupEntries WHERE ResourceGroupId = @ChildGroupId and EntriesId = @ResourceId
			IF @found = 1 
				BREAK
			
			SELECT @found = Found FROM csiResourceInGroup(@ResourceId, null, @ChildGroupId, null)
			IF @found = 1 
				BREAK
			FETCH NEXT FROM csr into @ChildGroupId
		END
		CLOSE csr
		DEALLOCATE csr
	END

	IF @found = 1
		INSERT INTO @returnTable VALUES (@found , @ResourceName, @ResourceId, @ResourceGroupId)
	RETURN 
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: csiGenerateAutoNumber
-- DESCR: Function to generate sequences (for NumberingRule) with the following format <vPrefix><sequence><vSuffix>
--        Returns the Auto Numbers in the following format <AutoNumber01>|<AutoNumber02>|<AutoNumber03>
--
-- Copyright Siemens 2023  
--------------------------------------------------------------------------------
IF EXISTS (SELECT * 
FROM   sysobjects 
WHERE  name = 'csiGenerateAutoNumber'
AND type = 'FN')
BEGIN
    DROP FUNCTION csiGenerateAutoNumber
END

IF EXISTS (SELECT * 
FROM   sysobjects 
WHERE  name = 'csiGenerateAutoNumber'
AND type = 'TF')
BEGIN
    DROP FUNCTION csiGenerateAutoNumber
END
GO

CREATE FUNCTION csiGenerateAutoNumber (
    @SequencesRequested INT, 
    @Prefix VARCHAR(40), 
    @Suffix VARCHAR(40), 
    @LastSequence INT, 
    @SequenceLength INT, 
    @UseHex INT)
RETURNS @returnTable TABLE (AutoNumber NVARCHAR(max)) --NVARCHAR(max)
AS
BEGIN
    DECLARE @Sequence INT
    DECLARE @SequenceValue VARCHAR(40)
    DECLARE @IsFirst INT = 1
    DECLARE @ParmValue NVARCHAR(max)
    DECLARE @RowsProcessed INT
    
    DECLARE @ErrLoc INT
    DECLARE @Delim CHAR(1) = '|'
    DECLARE @Counter INT = 0
    
    SET @ErrLoc = 1 

    -- Append the padding for the sequence length
    SET @Sequence = @LastSequence

	SET @Counter = 1
    WHILE ( @Counter <= @SequencesRequested)
    BEGIN
        -- increment the sequence
       SET  @Sequence = @Sequence + 1

        -- set the padding for the sequence length  
        SET @SequenceValue = RIGHT(REPLICATE('0', @SequenceLength) + LTRIM(@Sequence), @SequenceLength)
        
        IF (@UseHex = 1) 
        BEGIN
           SET @SequenceValue = RIGHT(REPLICATE('0', @SequenceLength) + LTRIM(FORMAT(@Sequence, 'x')), @SequenceLength)
        END

        SET @SequenceValue = CONCAT(@Prefix, @SequenceValue, @Suffix)

        IF (@IsFirst <> 1)
        BEGIN
            SET @ParmValue = CONCAT(@ParmValue, @Delim)
        END

        SET @ParmValue = CONCAT(@ParmValue, @SequenceValue)
        SET @IsFirst = 2

        SET @Counter = @Counter  + 1        
    END
    
    INSERT INTO @returnTable VALUES (@ParmValue)
    RETURN --@ParmValue
END
GO

-------------------------------------------------------------------------------
-- csiGetContainerMaterialList
-- Gets material list that defines required components for the container.
-- Used with a high volume machine setup to determine what components were issued.
-- All params should come from the HV history records to use MfgOrder and BOM at time of HV issue.
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetContainerMaterialList'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetContainerMaterialList
END
GO

CREATE FUNCTION csiGetContainerMaterialList
(	
	@ContainerId CHAR(16),
	@MfgOrderId CHAR(16),
	@BOMId CHAR(16)
)
RETURNS @ContainerMaterialList TABLE 
(
	ProductId CHAR(16),					
	ReferenceDesignator NVARCHAR(30),	
	SpecID CHAR(16),
	QtyRequired FLOAT,
	MaterialListItemId CHAR(16)
)
AS
BEGIN
	-- follow priority used by CVE on Container.MaterialList: This_Value;ContainerMaterialList;MfgOrder.MaterialList;BOM.MaterialList
	-- CVE on Container.BOM: This_Value;Product.BOM;Product.ERPBOM

	DECLARE @HaveList INT
	DECLARE @ItemCount INT
	SET @HaveList = 0
	
	-- Container.MaterialList
	select @ItemCount = count(*) from ContainerMaterialListItem where ContainerId = @ContainerId
	if(@ItemCount > 0)
	BEGIN
		insert into @ContainerMaterialList 
		select 
			p.ProductId,
			item.ReferenceDesignator,
			null,
			item.QtyRequired,
			item.ContainerMaterialListItemId
		from ContainerMaterialListItem item
		left join Product p on (
			(p.ProductId = item.ProductId) or
			(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
		)
		where ContainerId = @ContainerId
		set @HaveList = 1;
	END
	
	-- MfgOrder.MaterialList
	if(@HaveList = 0 and @MfgOrderId is not null and @MfgOrderId <> '')
	BEGIN
		select @ItemCount = count(*) from MfgOrderMaterialListItem where MfgOrderId = @MfgOrderId
		if(@ItemCount > 0)
		BEGIN
			insert into @ContainerMaterialList
			select 
				p.ProductId,
				item.ReferenceDesignator,
				null,
				item.QtyRequired,
				item.MfgOrderMaterialListItemId
			from 
				MfgOrderMaterialListItem item
				left join Product p on (
					(p.ProductId = item.ProductId) or
					(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
				)
			where MfgOrderId = @MfgOrderId
			set @HaveList = 1;
		END
	END

	-- BOM.MaterialList
	-- CVE on Container.BOM: This_Value;Product.BOM;Product.ERPBOM
	-- for BOM directly on container or Product.BOM, material table is ProductMaterialListItem which has Spec
	-- for Product.ERPBOM, table is BOMMaterialListItem which does not have Spec
	if(@HaveList = 0 and @BOMId is not null and @BOMId <> '')
	BEGIN
		if(@BOMId in (select ERPBOMId from ERPBOM)) -- Is an ERPBOM
		BEGIN
			insert into @ContainerMaterialList
			select
				p.ProductId,
				item.ReferenceDesignator,
				null,
				item.QtyRequired,
				item.BOMMaterialListItemId
			from	
				BOMMaterialListItem item
				left join Product p on (
					(p.ProductId = item.ProductId) or
					(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
				)
			where item.ERPBOMId = @BOMId
		END
		else
		BEGIN
			insert into @ContainerMaterialList
			select 
				p.ProductId,
				item.ReferenceDesignator,
				s.SpecId,
				item.QtyRequired,
				item.ProductMaterialListItemId
			from 
				ProductMaterialListItem item
				left join Product p on (
					(p.ProductId = item.ProductId) or
					(item.ProductBaseId <> '0000000000000000' and p.ProductId in (select RevOfRcdId from ProductBase where ProductBaseId = item.ProductBaseId))
				)
				left join Spec s on (
					(s.SpecId = item.SpecId) or
					(item.SpecBaseId <> '0000000000000000' and s.SpecId in (select RevOfRcdId from SpecBase where SpecBaseId = item.SpecBaseId))
				)
				where item.BOMId = @BOMId
		END
	END
RETURN
END
GO

-------------------------------------------------------------------------------
-- Gets high volume setup details for a specified setup
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetHVSetupDetails'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetHVSetupDetails
END
GO

CREATE FUNCTION csiGetHVSetupDetails
(	
	@HVSetupId CHAR(16)
)
RETURNS @HVSetupDetails TABLE 
(
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	CompID NVARCHAR(32),
	CompName NVARCHAR(32),
	Slot INT,
	SubSlot INT,
	ProductName NVARCHAR(100),
	ProductRevision NVARCHAR(25)
)
AS
BEGIN
	-- For initial implementation each HVSetup is a full setup, no deltas, so a simple select works.
	-- To suport deltas, we would need to update this to build the component list from multiple setups.
	-- The basic process would be
	--		initialize a local var @SetupId to passed in value @HVSetupId (this is the top level setup set on the resource)
	--		while (@SetupId is not null)
	--		{
	--			get all details for @SetupId
	--			insert all into results table where Slot/SubSlot not already in results table (so could probably do first insert separate without check)
	--			set @SetupId = PriorSetupId
	--		}
	--		return

	insert into @HVSetupDetails
	select
		d.ParentId,
		d.HVSetupHistoryDetailId,
		d.ProductId,
		d.CompId,
		d.CompName,
		d.Slot,
		d.SubSlot,
		pb.ProductName,
		p.ProductRevision
	from HVSetupHistoryDetail d 
	join Product p on p.ProductId = d.ProductId
	join ProductBase pb on pb.ProductBaseId = p.ProductBaseId
	where ParentId = @HVSetupId
RETURN
END
GO

-------------------------------------------------------------------------------
-- Get params needed to query for additional HV component issue information.
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetHVIssueHistoryParams'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetHVIssueHistoryParams
END
GO

CREATE FUNCTION csiGetHVIssueHistoryParams
(
	@HVIssueHistoryDetailId CHAR(16)
)
RETURNS @HVIssueParams TABLE
(
	HVSetupId CHAR(16),
	SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	ContainerId CHAR(16),
	MfgOrderId CHAR(16),
	BOMId CHAR(16),
	ResourceName NVARCHAR(100),
	WorkflowStepName NVARCHAR(100),
	SpecName NVARCHAR(100),
	TxnDateGMT DATETIME,
	HVComponentIssueHistoryId CHAR(16)
)
AS
BEGIN
	insert into @HVIssueParams
	select 
		summary.HVSetupId,
		summary.SpecId,
		summary.WorkflowStepId,
		detail.ContainerId,
		detail.MfgOrderId, 
		detail.BOMId,
		res.ResourceName,
		wfs.WorkflowStepName,
		sb.SpecName + ' (' + spec.SpecRevision + ')',
		summary.TxnDateGMT,
		summary.HVComponentIssueHistoryId
	from HVIssueHistoryDetail detail
	join HVComponentIssueHistory summary on summary.HVComponentIssueHistoryId = detail.ParentId
	join ResourceDef res on res.ResourceId = summary.ResourceId
	join WorkflowStep wfs on wfs.WorkflowStepId = summary.WorkflowStepId
	join Spec spec on spec.SpecId = summary.SpecId
	join SpecBase sb on sb.SpecBaseId = spec.SpecBaseId
	where detail.HVIssueHistoryDetailId = @HVIssueHistoryDetailId;

	RETURN
END
GO

-------------------------------------------------------------------------------
-- Get HV removed component information for a given container
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetHVRemovedComponents'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetHVRemovedComponents
END
GO

CREATE FUNCTION csiGetHVRemovedComponents
(
	@ContainerId CHAR(16),
	@HVIssueHistoryDetailId CHAR(16)
)
RETURNS @HVRemovedComponents TABLE
(

	HVRemoveId CHAR(16),
	HVRemoveDetailId CHAR(16),
	ContainerId CHAR(16),
	MaterialListItemId CHAR(16),
	QtyRemoved FLOAT, 
	DestinationLot NVARCHAR(100),
	DestinationStockPoint NVARCHAR(100),
	RemovalReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	HVSetupId CHAR(16),
	HVSetupDetailId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	TxnDateGMT DATETIME
)
AS
BEGIN
	insert into @HVRemovedComponents
	select
		rmDetail.HVRemoveHistoryDetailId as HVRemoveId,
		rmSetupDetail.HVRemoveHistorySetupDetailId as HVRemoveDetailId, 
		rmDetail.ContainerId, 
		rmDetail.MaterialListItemId, 
		rmDetail.QtyRemoved,
		rmDetail.DestinationLot,
		rmDetail.DestinationStockPoint,
		rmDetail.RemovalReasonId,
		rmDetail.RemoveDifferenceReasonId,
		rmSetupDetail.HVSetupId,
		rmSetupDetail.HVSetupDetailId, 
		rmSetupDetail.HVIssueHistoryDetailId,
		rmDetail.SpecId,
		rmDetail.WorkflowStepId,
		rmDetail.TxnDateGMT
	from 
		HVRemoveHistoryDetail rmDetail
		join HVRemoveHistorySetupDetail rmSetupDetail on rmSetupDetail.ParentId = rmDetail.HVRemoveHistoryDetailId
	where 
		rmSetupDetail.ContainerId = @ContainerId -- container match
		and rmDetail.HVIssueHistoryDetailId = @HVIssueHistoryDetailId

	RETURN
END
GO

-------------------------------------------------------------------------------
-- Get details for a single high volume issue record (one container processed at one resource with an HV setup)
-- May return multiple rows for a ref des if component product defined in multiple slots

-- HVIssueHistoryDetail holds the container info, but HV setup, Resource, Spec and WorkflowStep are set in the parent HVComponentIssueHistory.
-- If Spec is set, it is used as a filter to get only material items with matching Spec. Otherwise, all material items are used.
-- The history detail record saves the BOM and MfgOrder at the time of container move, just in case these were changed after the move.
-- However, we cannot compensate for the material list for that BOM or MfgOrder being changed. 

-- We are requiring a material list since standard Component Issue page shows no materials to issue for a container if it has no material list
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetDetailsForSingleHVIssue'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetDetailsForSingleHVIssue
END
GO

CREATE FUNCTION csiGetDetailsForSingleHVIssue
(	
	@HVIssueHistoryDetailId CHAR(16)
)
RETURNS @HVIssues TABLE 
(
	ReferenceDesignator NVARCHAR(100),
	CompName NVARCHAR(32),
	ProductName NVARCHAR(100),
	ProductRevision NVARCHAR(25),
	FromLot NVARCHAR(100),
	IssueControl INT,
	ResourceName NVARCHAR(100),
	Slot INT,
	SubSlot INT,
	QtyIssued FLOAT,
	WorkflowStepName NVARCHAR(100),
	SpecName NVARCHAR(100),
	TxnDateGMT DATETIME,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved FLOAT,
	NetQtyIssued FLOAT,
	RemoveDestinationLot NVARCHAR(100),
	RemoveDestinationStockPoint NVARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATETIME
)
AS
BEGIN
	-- get params needed from the history records
	DECLARE @HVSetupId CHAR(16)
	DECLARE @SpecId CHAR(16)
	DECLARE @WorkflowStepId CHAR(16)
	DECLARE @ContainerId CHAR(16)
	DECLARE @MfgOrderId CHAR(16)
	DECLARE @BOMId CHAR(16)
	DECLARE @ResourceName NVARCHAR(100)	
	DECLARE @WorkflowStepName NVARCHAR(100)
	DECLARE @SpecName NVARCHAR(100)
	DECLARE @TxnDateGMT DATETIME
	DECLARE @HVComponentIssueHistoryId CHAR(16)
	
	select 
		@HVSetupId = HVSetupId,
		@SpecId = SpecId,
		@WorkflowStepId = WorkflowStepId,
		@ContainerId = ContainerId,
		@MfgOrderId = MfgOrderId, 
		@BOMId = BOMId,
		@ResourceName = ResourceName,
		@WorkflowStepName = WorkflowStepName,
		@SpecName = SpecName,
		@TxnDateGMT = TxnDateGMT,
		@HVComponentIssueHistoryId = HVComponentIssueHistoryId
	from csiGetHVIssueHistoryParams(@HVIssueHistoryDetailId);

	with MaterialList(ProductId, ReferenceDesignator, SpecId, QtyIssued, MaterialListItemId) as
	(
		select ProductId, ReferenceDesignator, SpecId, QtyRequired, MaterialListItemId from csiGetContainerMaterialList(@ContainerId, @MfgOrderId, @BOMId)
	),
	SetupDetails(HVResourceSetupHistoryId, HVSetupHistoryDetailId, ProductId, CompId, CompName, Slot, SubSlot, ProductName, ProductRevision) as
	(
		select HVResourceSetupHistoryId, HVSetupHistoryDetailId, ProductId, CompId, CompName, Slot, SubSlot, ProductName, ProductRevision from csiGetHVSetupDetails(@HVSetupId)
	),
	RemovedComponents(HVRemoveId, HVRemoveDetailId, ContainerId, MaterialListItemId, QtyRemoved, DestinationLot, DestinationStockPoint, RemovalReasonId, RemoveDifferenceReasonId, 
					  HVSetupId, HVSetupDetailId, HVIssueHistoryDetailId, SpecId, WorkflowStepId, TxnDateGMT
	) as (
		select * from csiGetHVRemovedComponents(@ContainerId, @HVIssueHistoryDetailId)
	)
	insert into @HVIssues
	select 
		m.ReferenceDesignator,
		sd.CompName,
		sd.ProductName,
		sd.ProductRevision,
		sd.CompId as FromLot,
		3, -- IssueControl fixed to Lot and Stock Point
		@ResourceName,
		sd.Slot,
		sd.SubSlot,
		m.QtyIssued,
		@WorkflowStepName,
		@SpecName,
		@TxnDateGMT,
		sd.HVResourceSetupHistoryId,
		sd.HVSetupHistoryDetailId,
		sd.ProductId,
		res.ResourceId,
		@WorkflowStepId,
		@SpecId,
		m.MaterialListItemId,
		@HVComponentIssueHistoryId,
		@HVIssueHistoryDetailId,
		ISNULL(rc.QtyRemoved,0),
		CASE WHEN rc.QtyRemoved is not null THEN m.QtyIssued - rc.QtyRemoved ELSE m.QtyIssued END,
		rc.DestinationLot,
		rc.DestinationStockPoint,
		rc.RemovalReasonId,
		rc.RemoveDifferenceReasonId,
		rc.SpecId,
		rc.WorkflowStepId,
		rc.TxnDateGMT
		--rc.UOMId,
		--rc.VendorItemId
	from 
		SetupDetails sd
		join MaterialList m on m.ProductId = sd.ProductId and (m.SpecId is null or m.SpecId = @SpecId)
		join HVResourceSetupHistory setup on setup.HVResourceSetupHistoryId = sd.HVResourceSetupHistoryId
		join ResourceDef res on res.ResourceId = setup.ResourceId
		left join RemovedComponents rc on
			rc.MaterialListItemId = m.MaterialListItemId				-- bom item
			and rc.HVSetupDetailId = sd.HVSetupHistoryDetailId			-- setup detail
			and rc.HVIssueHistoryDetailId = @HVIssueHistoryDetailId		-- HV issue txn

	RETURN
END
GO

-------------------------------------------------------------------------------
-- Gets all high volume issue details for a given container
-- May return multiple rows for a ref des if component product defined in multiple slots on same resource, or on different resources
 
-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetAllHVIssuesForContainer'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetAllHVIssuesForContainer
END
GO

CREATE FUNCTION csiGetAllHVIssuesForContainer
(	
	@ContainerId CHAR(16),
	@ContainerName NVARCHAR(100),
	@ContainerOption INT
)
RETURNS @HVIssues TABLE 
(
	ContainerId CHAR(16),
	ContainerName NVARCHAR(100),
	ReferenceDesignator NVARCHAR(100),
	CompName NVARCHAR(32),
	ProductName NVARCHAR(100),
	ProductRevision NVARCHAR(25),
	FromLot NVARCHAR(100),
	IssueControl INT,
	ResourceName NVARCHAR(100),
	Slot INT,
	SubSlot INT,
	QtyIssued FLOAT,
	WorkflowStepName NVARCHAR(100),
	SpecName NVARCHAR(100),
	TxnDateGMT DATETIME,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved FLOAT,
	NetQtyIssued FLOAT,
	RemoveDestinationLot NVARCHAR(100),
	RemoveDestinationStockPoint NVARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATETIME
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
AS
BEGIN
	DECLARE @HVIssueHistoryDetailId CHAR(16)
	DECLARE @ChildContainerId CHAR(16)
	DECLARE @ChildCount INTEGER
	DECLARE @ContainerStatus INTEGER
	DECLARE @ParentContainerId CHAR(16)

	DECLARE @ChildrenOrSingle INT;	SET @ChildrenOrSingle = 0;
	DECLARE @Siblings INT;			SET @Siblings = 1;
	DECLARE @Single INT;			SET @Single = 2;

	DECLARE @GetSingle INT;
	DECLARE @GetByParent INT;
	DECLARE @GetByChildren INT;
	
	-- get info about specified container
	IF(@ContainerId is not null and @ContainerId <> '')
		select @ContainerName = ContainerName, @ChildCount = ChildCount, @ContainerStatus = Status, @ParentContainerId = ParentContainerId from Container where ContainerId = @ContainerId
	ELSE
		select @ContainerId = ContainerId, @ChildCount = ChildCount, @ContainerStatus = Status, @ParentContainerId = ParentContainerId from Container where ContainerName = @ContainerName

	-- based on options param and container info, determine how to load issue details
	if(@ContainerOption = @Single or (@ContainerOption = @Siblings and @ChildCount = 0 and @ParentContainerId is null) or (@ContainerOption = @ChildrenOrSingle and @ChildCount = 0))
		SET @GetSingle = 1; 
	else
	if(@ContainerOption = @Siblings and @ChildCount = 0 and @ParentContainerId is not null)
		SET @GetByParent = 1;  
	else
	if((@ContainerOption = @Siblings or @ContainerOption = @ChildrenOrSingle) and @ChildCount > 0)
		SET @GetByChildren = 1;

	-- load the issue details
	if(@GetSingle = 1 and @ContainerStatus > 0)
	BEGIN
		-- get issue details for only the specified container
		DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR	select HVIssueHistoryDetailId from HVIssueHistoryDetail where ContainerId = @ContainerId
		OPEN csr
		FETCH NEXT FROM csr into @HVIssueHistoryDetailId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			insert into @HVIssues select @ContainerId, @ContainerName, * from csiGetDetailsForSingleHVIssue(@HVIssueHistoryDetailId)
			FETCH NEXT FROM csr into @HVIssueHistoryDetailId
		END
		CLOSE csr
		DEALLOCATE csr
	END
	else if(@GetByParent = 1)
	BEGIN
		-- specified container is a child, get issue details for all with same parent
		DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR	select ContainerId from Container where ParentContainerId = @ParentContainerId
		OPEN csr
		FETCH NEXT FROM csr into @ChildContainerId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			insert into @HVIssues select * from csiGetAllHVIssuesForContainer(@ChildContainerId, null, 2)
			FETCH NEXT FROM csr into @ChildContainerId
		END
		CLOSE csr
		DEALLOCATE csr
	END
	else if(@GetByChildren = 1)
	BEGIN
		-- specified container is a parent, get issue details for all children
		DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR	select ContainerId from Container where ParentContainerId = @ContainerId
		OPEN csr
		FETCH NEXT FROM csr into @ChildContainerId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			insert into @HVIssues select * from csiGetAllHVIssuesForContainer(@ChildContainerId, null, 2)
			FETCH NEXT FROM csr into @ChildContainerId
		END
		CLOSE csr
		DEALLOCATE csr
	END

	RETURN
END
GO

-------------------------------------------------------------------------------
-- Gets high volume issue details for a given container
-- but returns only one row per container/materialListItem(refDes) combination

-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetHVIssuesForContainer'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetHVIssuesForContainer
END
GO

CREATE FUNCTION csiGetHVIssuesForContainer
(	
	@ContainerId CHAR(16),
	@ContainerName NVARCHAR(100),
	@ContainerOption INT
)
RETURNS @HVIssues TABLE 
(
	ContainerId CHAR(16),
	ContainerName NVARCHAR(100),
	ReferenceDesignator NVARCHAR(100),
	CompName NVARCHAR(32),
	ProductName NVARCHAR(100),
	ProductRevision NVARCHAR(25),
	FromLot NVARCHAR(100),
	IssueControl INT,
	ResourceName NVARCHAR(100),
	Slot INT,
	SubSlot INT,
	QtyIssued FLOAT,
	WorkflowStepName NVARCHAR(100),
	SpecName NVARCHAR(100),
	TxnDateGMT DATETIME,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved FLOAT,
	NetQtyIssued FLOAT,
	RemoveDestinationLot NVARCHAR(100),
	RemoveDestinationStockPoint NVARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATETIME
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
AS
BEGIN
	DECLARE @ConId CHAR(16)
	DECLARE @ConName NVARCHAR(100)
	DECLARE @RefDes NVARCHAR(100)
	DECLARE @CompName NVARCHAR(100)
	DECLARE @ProdName NVARCHAR(100)
	DECLARE @ProdRev NVARCHAR(25)
	DECLARE @FromLot NVARCHAR(100)
	DECLARE @IssueControl INT
	DECLARE @ResourceName NVARCHAR(100)
	DECLARE @Slot INT
	DECLARE @SubSlot INT
	DECLARE @Qty FLOAT
	DECLARE @StepName NVARCHAR(100)
	DECLARE @SpecName NVARCHAR(100)
	DECLARE @TxnDateGMT DATETIME
	DECLARE @HVSetupId CHAR(16)
	DECLARE @HVSetupDetailId CHAR(16)
	DECLARE @ProductId CHAR(16)
	DECLARE @ResourceId CHAR(16)
	DECLARE @StepId CHAR(16)
	DECLARE @SpecId CHAR(16)
	DECLARE @MatItemId CHAR(16)
	DECLARE @HVIssueId CHAR(16)
	DECLARE @HVIssueDetailId CHAR(16)
	DECLARE @QtyRm FLOAT
	DECLARE @NetQty FLOAT
	DECLARE @RmDestLot NVARCHAR(100)
	DECLARE @RmDestStockPt NVARCHAR(100)
	DECLARE @RmReasonId CHAR(16)
	DECLARE @RmDiffReasonId CHAR(16)
	DECLARE @RmSpecId CHAR(16)
	DECLARE @RmWorkflowStepId CHAR(16)
	DECLARE @RmTxnDateGMT DATETIME
	--DECLARE @RmUOMId CHAR(16)
	--DECLARE @RmVendorItemId CHAR(16)


	DECLARE @LastConName NVARCHAR(100)
	DECLARE @LastRefDes NVARCHAR(100)
	DECLARE @LastMatItemId CHAR(16)
	SET @LastConName = ''
	SET @LastRefDes = ''
	SET @LastMatItemId = ''

	DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
		select * from csiGetAllHVIssuesForContainer(@ContainerId, @ContainerName, @ContainerOption) 
		order by ContainerName, MaterialListItemId, TxnDateGMT desc, HVIssueHistoryDetailId
	OPEN csr
	FETCH NEXT FROM csr into	
		@ConId, @ConName, @RefDes, @CompName, @ProdName, @ProdRev, @FromLot, @IssueControl, @ResourceName, @Slot, @SubSlot, @Qty, 
		@StepName, @SpecName, @TxnDateGMT, @HVSetupId, @HVSetupDetailId, @ProductId, @ResourceId, @StepId, @SpecId, @MatItemId, @HVIssueId, @HVIssueDetailId, 
		@QtyRm, @NetQty, @RmDestLot, @RmDestStockPt, @RmReasonId, @RmDiffReasonId, @RmSpecId, @RmWorkflowStepId, @RmTxnDateGMT --, @RmUOMId, @RmVendorItemId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- include only first entry for Container/MaterialListItem combo
		if(@LastConName <> @ConName or @LastMatItemId <> @MatItemId)
			insert into @HVIssues values(
				@ConId, @ConName, @RefDes, @CompName, @ProdName, @ProdRev, @FromLot, @IssueControl, @ResourceName, @Slot, @SubSlot, @Qty, 
				@StepName, @SpecName, @TxnDateGMT, @HVSetupId, @HVSetupDetailId, @ProductId, @ResourceId, @StepId, @SpecId, @MatItemId, @HVIssueId, @HVIssueDetailId, 
				@QtyRm, @NetQty, @RmDestLot, @RmDestStockPt, @RmReasonId, @RmDiffReasonId, @RmSpecId, @RmWorkflowStepId, @RmTxnDateGMT --, @RmUOMId, @RmVendorItemId
			)
		set @LastConName = @ConName 
		set @LastMatItemId = @MatItemId
		FETCH NEXT FROM csr into 
			@ConId, @ConName, @RefDes, @CompName, @ProdName, @ProdRev, @FromLot, @IssueControl, @ResourceName, @Slot, @SubSlot, @Qty, 
			@StepName, @SpecName, @TxnDateGMT, @HVSetupId, @HVSetupDetailId, @ProductId, @ResourceId, @StepId, @SpecId, @MatItemId, @HVIssueId, @HVIssueDetailId, 
			@QtyRm, @NetQty, @RmDestLot, @RmDestStockPt, @RmReasonId, @RmDiffReasonId, @RmSpecId, @RmWorkflowStepId, @RmTxnDateGMT --, @RmUOMId, @RmVendorItemId
	END
	CLOSE csr
	DEALLOCATE csr

	RETURN
END
GO

-------------------------------------------------------------------------------
-- Get all machine setup details for HV issues to a given container and material list item(Ref Des)
-- Purpose is to get the setup details that could have issued components.
-- Later processing adds history records that indicate components issued to the container from the setup have been removed.
-- This allows for excluding those components from queries to see what was issued, or included in other queries to get what has been removed.
-------------------------------------------------------------------------------
IF EXISTS (SELECT * 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetHVRemoveSetupDetails'
	   AND type = 'TF')
BEGIN
    DROP FUNCTION csiGetHVRemoveSetupDetails
END
GO

CREATE FUNCTION csiGetHVRemoveSetupDetails
(	
	@ContainerId CHAR(16),
	@ContainerName NVARCHAR(100),
	@ContainerOption INT,
	@MaterialListItemId NVARCHAR(30)
)
RETURNS @HVIssues TABLE 
(
	ContainerId CHAR(16),
	ContainerName NVARCHAR(100),
	ReferenceDesignator NVARCHAR(100),
	CompName NVARCHAR(32),
	ProductName NVARCHAR(100),
	ProductRevision NVARCHAR(25),
	FromLot NVARCHAR(100),
	IssueControl INT,
	ResourceName NVARCHAR(100),
	Slot INT,
	SubSlot INT,
	QtyIssued FLOAT,
	WorkflowStepName NVARCHAR(100),
	SpecName NVARCHAR(100),
	TxnDateGMT DATETIME,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved FLOAT,
	NetQtyIssued FLOAT,
	RemoveDestinationLot NVARCHAR(100),
	RemoveDestinationStockPoint NVARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT DATETIME
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
AS
BEGIN

	insert into @HVIssues
	select * from csiGetAllHVIssuesForContainer(@ContainerId, @ContainerName, @ContainerOption)
	where MaterialListItemId = @MaterialListItemId

	RETURN
END
GO

IF OBJECT_ID(N'csiIncreaseStringColMaxLength',N'P') IS NOT NULL
	DROP PROCEDURE csiIncreaseStringColMaxLength;
GO

CREATE PROCEDURE csiIncreaseStringColMaxLength
	@TableName NVARCHAR(50), @ColName NVARCHAR(50), @NewMaxLength INT
AS 
	DECLARE @CurrentMaxLength INT
	DECLARE @AlterSQL NVARCHAR(1000)
	DECLARE @Message NVARCHAR(1000)
	DECLARE @CurrentMaxLengthStr NVARCHAR(10)
	DECLARE @NewMaxLengthStr NVARCHAR(10)

BEGIN
	select @CurrentMaxLength = CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @TableName and COLUMN_NAME = @ColName;
	set @CurrentMaxLengthStr = CAST(@CurrentMaxLength as NVARCHAR(10));
	set @NewMaxLengthStr = CAST(@NewMaxLength as NVARCHAR(10));

	if (@CurrentMaxLength < @NewMaxLength)
	BEGIN
		set @AlterSQL = 'alter table ' + @TableName + ' alter column ' + @ColName + ' nvarchar(' + @NewMaxLengthStr + ');' 
		print @AlterSQL;
		begin try
			exec (@alterSQL);
			set @Message = 'Max length of Column ' + @ColName + ' on Table ' + @TableName + ' was updated from (' + @CurrentMaxLengthStr + ') to (' + @NewMaxLengthStr + ').';
		end try
		begin catch
			set @message = 'MSSQLError- There was a problem updating Column ' + @ColName + ' on Table ' + @TableName + '. The error returned was [ ' + cast(Error_Number() as varchar) + ' ' + ERROR_MESSAGE() + ']'; 
			print @Message;
		end catch
	END
	--else
	--set @Message = 'Column ' + @ColName + ' on Table ' + @TableName + ' not updated. Requested length (' + @NewMaxLengthStr + ') not greater than current length (' + @CurrentMaxLengthStr + ').';
	print @Message;
END
GO

-------------------------------------------------------------------------------
-- Procedure to validate NDO given its Table Name, Value and its Name field's Column Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidNDO (   
@pTableName  NVARCHAR(40),					 
@pFieldValue NVARCHAR(4000),
@DBNameColumnName NVARCHAR(40),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE   
    @vCount INT,
    @sql_stmt NVARCHAR(2000),
    @vFieldName NVARCHAR(40),
    @param NVARCHAR(100)
    SET @vCount = 0;
										   
    SET @sql_stmt = N'select @vCount = count(*) from [' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].'
    + @pTableName + N' where UPPER( '+ @DBNameColumnName + N' ) = UPPER( @pFieldValue );';

   PRINT(N'stmt: ' + @sql_stmt);

   EXEC sp_executesql @sql_stmt, @param = N'@pFieldValue NVARCHAR(4000), @vCount INT OUTPUT', @pFieldValue = @pFieldValue, @vCount = @vCount OUTPUT;

   PRINT (N'Count: ' + CAST(@vCount as nvarchar));

   IF(@vCount = 0)
	BEGIN
		SET @oResult = 1;
	END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate RDO given its Table Name, Value and its Name field's Column Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidRDO (   
@pTableName  NVARCHAR(40),				 
@pFieldValue NVARCHAR(4000),
@pFieldRevision NVARCHAR(4000),
@DBColumnName NVARCHAR(40),
@DBBaseColumnName NVARCHAR(40),
@DBRevisionColName NVARCHAR(40),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE   
    @vCount INT,
    @sql_stmt NVARCHAR(2000),
    @vFieldName NVARCHAR(40),
	@vTableName NVARCHAR(40),
    @param NVARCHAR(100)
    SET @vCount = 0;

	SET @sql_stmt = N'select @vCount = count(*) from [' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].'
	+ @pTableName + ' PS join ' + @pTableName + 'Base PSB on PS.' + @DBBaseColumnName + ' = PSB.'+ @DBBaseColumnName + ' where PSB.' + @DBColumnName  + ' = '''+ @pFieldValue +''' and PS.'+ @DBRevisionColName + ' = '''+ @pFieldRevision +''';';

	PRINT(N'stmt: ' + @sql_stmt);

	EXEC sp_executesql @sql_stmt, @param = N'@vCount INT OUTPUT', @vCount = @vCount OUTPUT

	PRINT (N'Count2: ' + CAST(@vCount as nvarchar));

	IF(@vCount = 0)
	BEGIN
		SET @oResult = 1;
	END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate a String given its value and length
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE  clfutilIsValidStrLength(
@pPrecisionValue  INT,
@pFieldValue NVARCHAR(4000),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE   
    @vLenght INT,
    @sql_stmt NVARCHAR(2000),
    @param NVARCHAR(100)
    SET @vLenght = 0;

	SET @sql_stmt = N'select @vLenght = Len ( @pFieldValue );';

   PRINT(N'stmt: ' + @sql_stmt);

   EXEC sp_executesql @sql_stmt, @param = N'@pFieldValue NVARCHAR(4000), @vLenght INT OUTPUT', @pFieldValue = @pFieldValue, @vLenght = @vLenght OUTPUT

   PRINT (N'Count2: ' + CAST(@vLenght as nvarchar));

   IF(@vLenght > @pPrecisionValue)
    BEGIN
       SET @oResult = 1;
    END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate a Number given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidNumber(
@pFieldValue NVARCHAR(4000),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE  
    @vNumberStatus INT,
    @sql_stmt NVARCHAR(2000),
    @param NVARCHAR(100)
    SET @vNumberStatus = 0;
    SET @sql_stmt = N'select @vNumberStatus = ISNUMERIC ( @pFieldValue );';

   PRINT(N'stmt: ' + @sql_stmt);

   EXEC sp_executesql @sql_stmt, @param = N'@pFieldValue NVARCHAR(4000), @vNumberStatus INT OUTPUT', @pFieldValue = @pFieldValue, @vNumberStatus = @vNumberStatus OUTPUT

   PRINT (CAST(@vNumberStatus as nvarchar));

   IF(@vNumberStatus = 0)
    BEGIN
        SET @oResult = 1;
    END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate a Boolean value given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidBoolean(
@pFieldValue NVARCHAR(4000),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE 
	@vStatus NVARCHAR(40)

	SET @oResult = 1;
	IF ( UPPER(TRIM(@pFieldValue)) = 'TRUE' OR @pFieldValue = '1' OR UPPER(TRIM(@pFieldValue)) = 'FALSE' OR @pFieldValue = '0' )
    BEGIN
        SET @oResult = 0;
    END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate a TimeStamp given its value
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidTimeStamp(
@pFieldValue NVARCHAR(4000),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE 
	@vDateStatus INT,
    @sql_stmt NVARCHAR(2000),
    @param NVARCHAR(100)
    SET @vDateStatus = 0;
    SET @sql_stmt = N'select @vDateStatus = ISDATE ( @pFieldValue ) ;';

		BEGIN
		   PRINT(N'stmt: ' + @sql_stmt);

		   EXEC sp_executesql @sql_stmt, @param = N'@pFieldValue NVARCHAR(4000), @vDateStatus INT OUTPUT', @pFieldValue = @pFieldValue, @vDateStatus = @vDateStatus OUTPUT

		   PRINT (CAST(@vDateStatus as nvarchar));

		   IF(@vDateStatus = 0)
			BEGIN
				SET @oResult = 1;
			END;
		END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate a Location NDO that model within in Facotry of a container
-- Given Location Value and Container Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilIsValidLocation(
@pFieldName NVARCHAR(40),
@pFieldValue NVARCHAR(4000),
@pContainerName NVARCHAR(40),
@oResult INT OUTPUT
)
AS BEGIN
DECLARE   
    @sql_stmt NVARCHAR(2000),
	@vFactoryName NVARCHAR(40),
	@vEmployeeName NVARCHAR(40),
	@vLoginUser NVARCHAR(40),
	@vCount INT,
	@vLCount INT,
    @param NVARCHAR(100)

	SET @vFactoryName = '';
	SET @vEmployeeName = '';
    SET @vLoginUser = 'LoginUser';

	SET @Sql_stmt = N'Select @vEmployeeName = value FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE Name = UPPER(@vLoginUser); ';

	EXEC sp_executesql @sql_stmt, @param = N' @vLoginUser NVARCHAR(40), @vEmployeeName NVARCHAR(40) OUTPUT', @vLoginUser = @vLoginUser,  @vEmployeeName = @vEmployeeName OUTPUT

	IF ( @vEmployeeName = '')
		SET @vLCount = 0;
	ELSE
		BEGIN
			SET @Sql_stmt = N'SELECT @vFactoryName = f.factoryname FROM Employee e JOIN sessionvalues s ON e.employeeid = s.employeeid JOIN factory f ON f.factoryid = s.factoryid WHERE employeename = @vEmployeeName;';

			EXEC sp_executesql @sql_stmt, @param = N' @vEmployeeName NVARCHAR(40), @vFactoryName NVARCHAR(40) OUTPUT', @vEmployeeName = @vEmployeeName, @vFactoryName = @vFactoryName OUTPUT

			PRINT(N'Name: ' + @vFactoryName);

			SET @sql_stmt = N'SELECT @vLCount = count(*) FROM Factory F​ INNER JOIN Location L ON F.FactoryId = L.FactoryId ​ WHERE F.FactoryName = UPPER( @vFactoryName ) AND L.LocationName = UPPER( @pFieldValue );';

			EXEC sp_executesql @sql_stmt, @param = N' @vFactoryName NVARCHAR(40), @pFieldValue NVARCHAR(4000), @vLCount INT OUTPUT', @vFactoryName = @vFactoryName, @pFieldValue = @pFieldValue, @vLCount = @vLCount OUTPUT
		END

	IF(@vLCount = 0)
	BEGIN
		SET @oResult = 1;
	END;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to validate data submit to Multi Lots Modify Attrs (HPE) service
-- This procedure will update the status column is a temp table to 1 if the data of a row is invalid
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilValidateMultiLotsAttrs 
AS
BEGIN
--
-- Copyright Siemens 2023  
--
	DECLARE @TableName  NVARCHAR(40)
	DECLARE @AttributeName  NVARCHAR(4000)
	DECLARE @AttributeValue NVARCHAR(4000)
	DECLARE @AttributeRevision NVARCHAR(40)
	DECLARE @ContainerName NVARCHAR(40)
	DECLARE @QuerySQL NVARCHAR(MAX)
    DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
	DECLARE @FieldType  NVARCHAR(40)
	DECLARE @PrecisionValue INT
	DECLARE @DBColumnName NVARCHAR(40)
    DECLARE @DBBaseColumnName NVARCHAR(40)
    DECLARE @DBNameColumnName NVARCHAR(40)
    DECLARE @DBRevisionColName NVARCHAR(40)
    DECLARE @ObjectId  NVARCHAR(40)								  
	DECLARE @ValidationFail INT
	DECLARE @sql_stmt NVARCHAR(2000)

	SET @QuerySQL	= 'SELECT ContainerName, AttributeName, AttributeValue, AttributeRevision, TableName, FieldType, PrecisionValue,DBColumnName,DBBaseColumnName,DBNameColumnName,DBRevisionColName '
					+ 'FROM		##MultiLotsModifyAttrsTemp  '

	SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'
	EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	FETCH NEXT FROM @c1 INTO @ContainerName, @AttributeName, @AttributeValue, @AttributeRevision, @TableName, @FieldType, @PrecisionValue,@DBColumnName,@DBBaseColumnName,@DBNameColumnName, @DBRevisionColName
	WHILE(@@fetch_status = 0)       
	BEGIN
	IF ( @AttributeValue != '' AND @AttributeValue IS NOT NULL )
		BEGIN
			PRINT('Container: ' + @ContainerName + ',AttributeName: ' + @AttributeName);
			SET @ValidationFail = 0;
			IF ((@FieldType = 'NDO'))
				IF (@AttributeName = 'Location')
					BEGIN
						EXEC clfutilIsValidLocation  @AttributeName , @AttributeValue, @ContainerName, @ValidationFail OUT;
					END
				ELSE
					BEGIN
						EXEC clfutilIsValidNDO @TableName , @AttributeValue, @DBNameColumnName, @ValidationFail OUT;
					END	

			IF ((@FieldType = 'RDO'))
				BEGIN
					EXEC clfutilIsValidRDO @TableName , @AttributeValue, @AttributeRevision, @DBNameColumnName, @DBBaseColumnName, @DBRevisionColName, @ValidationFail OUT;
				END	

			IF ((@FieldType ='STRING'))
				BEGIN
					EXEC clfutilIsValidStrLength @PrecisionValue , @AttributeValue, @ValidationFail OUT;
				END

			IF ((@FieldType ='NUMBER'))
				BEGIN
					EXEC clfutilIsValidNumber @AttributeValue, @ValidationFail OUT; 
				END

			IF ((@FieldType ='BOOLEAN'))
				BEGIN
					EXEC clfutilIsValidBoolean @AttributeValue, @ValidationFail OUT; 
				END
			
			IF ((@FieldType ='TIMESTAMP'))
				BEGIN
					EXEC clfutilIsValidTimeStamp @AttributeValue, @ValidationFail OUT; 
				END

			IF @ValidationFail = 1 
				BEGIN
					SET @sql_stmt = N'UPDATE ##MultiLotsModifyAttrsTemp  SET ValidationFail = 1 WHERE ContainerName = @ContainerName AND AttributeName = @AttributeName';
					EXECUTE sp_executesql @sql_stmt, N'@ContainerName NVARCHAR(40), @AttributeName NVARCHAR(40)', @ContainerName = @ContainerName, @AttributeName = @AttributeName ;
					PRINT(N'stmt: ' + @sql_stmt + @AttributeName + @ContainerName);
				END
		END
		FETCH NEXT FROM @c1 INTO @ContainerName,@AttributeName,@AttributeValue,@AttributeRevision,@TableName, @FieldType, @PrecisionValue,@DBColumnName,@DBBaseColumnName,@DBNameColumnName, @DBRevisionColName
	END
	CLOSE @c1
	DEALLOCATE @c1

	PRINT('DONE');
END
GO

-------------------------------------------------------------------------------
-- Procedure to update initial data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilUpdateMultiLotsAttrsTemp
AS
BEGIN
    DECLARE @TableName  NVARCHAR(40);
    DECLARE @AttributeName  NVARCHAR(40);
    DECLARE @AttributeValue NVARCHAR(4000);
    DECLARE @AttributeRevision NVARCHAR(40);
    DECLARE @ContainerName NVARCHAR(40);
    DECLARE @FieldType  NVARCHAR(40);
    DECLARE @PrecisionValue INTEGER;
    DECLARE @DBColumnName  NVARCHAR(40);
    DECLARE @DOColumnName  NVARCHAR(40);
    DECLARE @DBBaseColumnName  NVARCHAR(40);
    DECLARE @DBNameColumnName NVARCHAR(40);
    DECLARE @DBRevisionColName NVARCHAR(40);
    DECLARE @ObjectId  NVARCHAR(40);
    DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
    DECLARE @Sql_stmt NVARCHAR(2000);
    DECLARE @ContainerId NVARCHAR(16);
    DECLARE @AttrCDOName NVARCHAR(40);
    DECLARE @param NVARCHAR(100);
	DECLARE @TempSQL NVARCHAR(200);
	DECLARE @CurrentStatusId NVARCHAR(16);
	DECLARE @AttrTableName NVARCHAR(40);
	
    SET @QuerySQL    = 'SELECT ContainerName,AttributeName,AttributeValue,AttributeRevision,TableName,FieldType,'
                        +'PrecisionValue,DBColumnName,DOColumnName,DBBaseColumnName,DBNameColumnName,DBRevisionColName,AttributeCDOName '
                        + 'FROM        ##MultiLotsModifyAttrsTemp ';
    
    SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
    
    FETCH NEXT FROM @c1 INTO @ContainerName,@AttributeName,@AttributeValue,@AttributeRevision,@TableName,@FieldType,@PrecisionValue,@DBColumnName,@DOColumnName,@DBBaseColumnName,@DBNameColumnName, @DBRevisionColName, @AttrCDOName;
    WHILE(@@fetch_status = 0)
        BEGIN
            SET @Sql_stmt = 'Select @ContainerId = ContainerId,@CurrentStatusId = CurrentStatusId From [' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].Container Where ContainerName ='''+ @ContainerName +'''';
			EXEC sp_executesql @Sql_stmt, @param = N'@ContainerId NVARCHAR(16) OUTPUT,@CurrentStatusId NVARCHAR(16) OUTPUT', @ContainerId = @ContainerId OUTPUT,@CurrentStatusId = @CurrentStatusId OUTPUT
            PRINT('Container: ' + @ContainerName + ' ,AttributeName: ' + @AttributeName + ' ,ContainerId: ' + @ContainerId);

			IF (@AttributeName = 'Location' OR @AttributeName = 'Factory')
				BEGIN
					SET @AttrCDOName  = 'CurrentStatus';
					SET @TempSQL = ' WHERE CurrentStatusId = ''' + @CurrentStatusId ;
				END;
			ELSE
				BEGIN
					SET @TempSQL = ' WHERE ContainerId = ''' + @ContainerId ;
				END;
				
			SET @AttrTableName= @AttrCDOName; 
			IF(@AttrCDOName='LotAttributes') 
				BEGIN 
					SET @AttrTableName = 'A_'+ @AttrCDOName; 
				END;
				
			IF(@FieldType ='NDO')
                BEGIN
                    SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET ObjectId = ( Select '+ @DOColumnName +' from ' + @TableName
                                    + ' where ' + @DBNameColumnName + ' = '''+ @AttributeValue +''') Where ContainerName = @ContainerName AND AttributeName = @AttributeName';
                    PRINT('@Sql_stmt: '+@Sql_stmt);

                    EXEC sp_executesql @sql_stmt, @param = N' @ContainerName NVARCHAR(40), @AttributeName NVARCHAR(40)', @ContainerName = @ContainerName, @AttributeName = @AttributeName;
                    
					SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET OldObjectId = ( Select FT. ' + @DBColumnName + ' FROM '+ @AttrTableName +' FT JOIN '+@TableName+' TB ON FT. '+ @DBColumnName + '= TB.'+ @DOColumnName 
							+ @TempSQL + ''' ), OldValue = ( Select TB. ' + @DBNameColumnName + ' FROM '+ @AttrTableName +' FT JOIN '+@TableName+' TB ON FT. '+ @DBColumnName + '= TB.'+ @DOColumnName + @TempSQL 
							+ ''' ) WHERE AttributeName = '''+ @AttributeName + ''' AND ContainerName ='''+ @ContainerName +'''';
                    PRINT('@Sql_stmt: '+@Sql_stmt);
                    EXECUTE sp_executesql @sql_stmt;  
                END;
           
		    ELSE IF(@FieldType ='RDO')
                BEGIN
                    SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET ObjectId = ( Select '+ @DBColumnName +' from ' + @TableName + ' PS join ' + @TableName + 'Base PSB on PS.' + @DBBaseColumnName + ' = PSB.'
                            + @DBBaseColumnName + ' where PSB.' + @DBNameColumnName + ' = '''+ @AttributeValue +''' and PS.'+ @DBRevisionColName
                            + ' = '''+ @AttributeRevision +''') Where ContainerName = @ContainerName AND AttributeName = @AttributeName';
                    PRINT('@Sql_stmt: '+@Sql_stmt);
                    EXECUTE sp_executesql @sql_stmt, @param = N'@ContainerName NVARCHAR(40), @AttributeName NVARCHAR(40)', @ContainerName = @ContainerName, @AttributeName = @AttributeName;   
                    
					SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET OldObjectId = ( Select FT. '+ @DBColumnName +' FROM '+ @AttrTableName +' FT WHERE ContainerId = ''' + @ContainerId 
						+ ''' ) , OldValue = ( Select TBB.'+ @DBNameColumnName + ' FROM '+ @AttrTableName +' FT JOIN '+ @TableName+' TB ON FT. '+ @DBColumnName + '= TB.'+ @DOColumnName 
						+ ' JOIN '+ @TableName+'Base TBB ON TB. '+ @DBBaseColumnName + '= TBB.'+ @DBBaseColumnName + ' WHERE ContainerId = ''' + @ContainerId 
						+ ''' ), OldRevision = ( Select TB.'+ @DBRevisionColName + ' FROM '+ @AttrTableName +' FT JOIN '+ @TableName+' TB ON FT. '+ @DBColumnName + '= TB.'+ @DOColumnName 
						+ ' JOIN '+ @TableName+'Base TBB ON TB. '+ @DBBaseColumnName + '= TBB.'+ @DBBaseColumnName + ' WHERE ContainerId = ''' + @ContainerId 
						+ ''' ) WHERE AttributeName = '''+ @AttributeName + ''' AND ContainerName ='''+ @ContainerName +'''';
                    PRINT('@Sql_stmt: '+@Sql_stmt);
                    EXECUTE sp_executesql @sql_stmt;  
                END;
			ELSE IF(@AttrCDOName='LotAttributesEx') 
				BEGIN
					SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET OldValue = ( Select DISTINCT FT.AttributeValue FROM '
                            + 'A_LotAttributesEx FT JOIN ##MultiLotsModifyAttrsTemp BT ON BT.ModifyAttributeSetupId  = FT.AttributeId WHERE ContainerId = ''' 
                            + @ContainerId + ''' AND BT.AttributeName = '''+ @AttributeName +''') WHERE AttributeName = '''+ @AttributeName + ''' AND ContainerName ='''+ @ContainerName +'''';
					EXECUTE sp_executesql @sql_stmt;  
				END;
			ELSE
			
				BEGIN
					SET @Sql_stmt = 'UPDATE ##MultiLotsModifyAttrsTemp SET OldValue = ( Select FT.' + @AttributeName + ' AttributeValue FROM '
										+ @AttrTableName +' FT ' + @TempSQL + ''') WHERE AttributeName = '''+ @AttributeName + ''' AND ContainerName ='''+ @ContainerName 
										+'''' ;
					EXECUTE sp_executesql @Sql_stmt;  
				END;
           FETCH NEXT FROM @c1 INTO @ContainerName,@AttributeName,@AttributeValue,@AttributeRevision,@TableName,@FieldType,@PrecisionValue,@DBColumnName,@DOColumnName,@DBBaseColumnName,@DBNameColumnName, @DBRevisionColName, @AttrCDOName;
        END;
    CLOSE @c1
    DEALLOCATE @c1
END;
GO

---------------------------------------------------------------------------------------------------------
-- Procedure to update container/lotattribute/lotattributeex use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a container, currentstatus, lotattribute and lotattributeex table
---------------------------------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilUpdateMultiLotsValueTemp
AS
BEGIN
	DECLARE @TableName  NVARCHAR(40);
	DECLARE @ContainerName  NVARCHAR(40);
	DECLARE @HistoryMainline  CHAR(16);
	DECLARE @ContainerStatusChangeHistory  CHAR(16);
	DECLARE @ModifyAttrsHistory  CHAR(16);
    DECLARE @ModifyAttributeSetupId CHAR(16);
    DECLARE @CurrentStatusId CHAR(16);
    DECLARE @TxnId  CHAR(16);
    DECLARE @ContainerId  CHAR(16);
    DECLARE @LotAttributeId CHAR(16);
    DECLARE @AttributeName NVARCHAR(40);
	DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
    DECLARE @c2 CURSOR;
	DECLARE @QuerySQL2 NVARCHAR(4000);
    DECLARE @QuerySQL3 NVARCHAR(4000);
    DECLARE @QuerySQL4 NVARCHAR(4000);
	DECLARE @ColumnName NVARCHAR(4000);
	DECLARE @ColumnValue NVARCHAR(4000);
    DECLARE @CDOName NVARCHAR(4000);
    DECLARE @RowCount INTEGER;
	DECLARE @Sql_stmt NVARCHAR(2000);
	DECLARE @Sql_stmt2 NVARCHAR(2000);
	DECLARE @FieldType NVARCHAR(40);
	DECLARE @StringValue NVARCHAR(4000);
	DECLARE @LotAttributesChangeCount bit;
	DECLARE @CurrentStatusChangeCount bit;
   
    SET @QuerySQL	= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, '
					+ ' TxnId,LotAttributesId, CurrentStatusId '
					+ ' FROM		##MultiLotsModifyAttrsTempContainer  WHERE ContainerSequence = 1';
	
	SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	
	FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;

	WHILE (@@fetch_status = 0)
		BEGIN
			SET @LotAttributesChangeCount = 1;
			SET @CurrentStatusChangeCount = 1;
			SET @QuerySQL2	= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value,'
								+ 'AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
								+ 'FROM		##MultiLotsModifyAttrsTemp WHERE ContainerName = '''+@ContainerName+''' AND IsUpdated = 1';
			PRINT('@Sql_stmt: '+@QuerySQL2);	
			
			SET @Sql_stmt2 = N'SET @c2 = CURSOR FAST_FORWARD FOR ' + @QuerySQL2 + ' FOR READ ONLY; OPEN @c2'    
			EXEC sp_executesql @Sql_stmt2, N'@c2 CURSOR OUTPUT', @c2 OUTPUT
			
			FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
			
			WHILE(@@fetch_status = 0)
				BEGIN
				PRINT('DBColumnName: ' + @ColumnName + ' ,Value: ' + @ColumnValue + ' ,AttributeName: ' + @AttributeName+',@CurrentStatusId' +@CurrentStatusId);
					
						BEGIN
							IF (@ColumnValue IS NULL) 
								BEGIN
									SET @StringValue = 'NULL' ;
								END
							ELSE
								BEGIN
									SET @StringValue = ''''+@ColumnValue+'''' ;
								END
						END;
					IF(@CDOName ='Container') 
						BEGIN
							IF (@AttributeName != 'Factory' AND @AttributeName != 'Location') 
								BEGIN
									SET @QuerySQL3	= 'UPDATE CONTAINER SET '+ @ColumnName +' = '+ @StringValue +', '
													+ ' LastActivityDate = GETDATE(), '
													+ 'LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE ContainerName = @ContainerName ';
									PRINT('@Sql_stmt: '+@QuerySQL3);
									EXEC sp_executesql @QuerySQL3, @param = N' @ContainerName NVARCHAR(40)', @ContainerName = @ContainerName;
								END;
							ELSE
								BEGIN
									IF (@CurrentStatusChangeCount = 1)
										BEGIN
											SET @QuerySQL3	 = 'UPDATE CurrentStatus SET '+ @ColumnName +' = '+ @StringValue +','
													+ ' ChangeCount = ChangeCount + 1, LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE CurrentStatusId = @CurrentStatusId';
											SET @CurrentStatusChangeCount = 0;
										END;
									ELSE
										BEGIN
											SET @QuerySQL3	 = 'UPDATE CurrentStatus SET '+ @ColumnName +' = '+ @StringValue +','
													+ ' LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE CurrentStatusId = @CurrentStatusId';
										END;
									PRINT('@Sql_stmt: '+@QuerySQL3);
									EXEC sp_executesql @QuerySQL3, @param = N' @CurrentStatusId CHAR(16)', @CurrentStatusId = @CurrentStatusId;
								END;
						END;
					
					IF(@CDOName ='LotAttributes')
						BEGIN
							IF (@LotAttributesChangeCount = 1)
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue +','
											+ ' ChangeCount = ChangeCount + 1 '
											+ ' WHERE LotAttributesId = @LotAttributesId ';
									SET @LotAttributesChangeCount = 0;
								END;
							ELSE
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue
											+ ' WHERE LotAttributesId = @LotAttributesId ';
								END;
							EXEC sp_executesql @QuerySQL3, @param = N' @LotAttributesId NVARCHAR(40)', @LotAttributesId = @LotAttributeId;
						END;
						
					IF(@CDOName ='LotAttributesEx')
						BEGIN
							SET @QuerySQL3	= 'Select @RowCount = COUNT(*) FROM A_LotAttributesEx '
											+ 'WHERE AttributeId = '''+@ModifyAttributeSetupId+''' AND ContainerId = '''+@ContainerId+'''';
                    
							EXEC sp_executesql @QuerySQL3, @param = N' @RowCount INTEGER OUTPUT', @RowCount = @RowCount OUTPUT;
							
							IF (@RowCount > 0) 
								BEGIN
									SET @QuerySQL4	= 'UPDATE A_LotAttributesEx SET AttributeValue = '+ @StringValue +',' 	
													+ ' ChangeCount = ChangeCount + 1, LastTimestamp = GETDATE() '
													+ ' WHERE AttributeId = @LotAttributesId AND ContainerId = @Container ';
										
									EXEC sp_executesql @QuerySQL4, @param = N' @LotAttributesId CHAR(16), @Container CHAR(16)', @LotAttributesId = @ModifyAttributeSetupId, @Container = @ContainerId;
								END;
						END;
				
					FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
				END;
			CLOSE @c2;
			DEALLOCATE @c2;
			
			FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;
		END;
		CLOSE @c1;
		DEALLOCATE @c1;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to update lotattributeex data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilGetInsertLotAttrEx
AS
BEGIN 
	DECLARE @TableName  NVARCHAR(40);
	DECLARE @ContainerName  NVARCHAR(40);
	DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
	DECLARE @QuerySQL2 NVARCHAR(4000);
    DECLARE @QuerySQL3 NVARCHAR(4000);
	DECLARE @ColumnName NVARCHAR(4000);
	DECLARE @ColumnValue NVARCHAR(4000);
    DECLARE @CDOName NVARCHAR(4000);
    DECLARE @RowCount INTEGER;
    DECLARE @ContainerId  CHAR(16);
    DECLARE @ModifyAttributeSetupId CHAR(16);
    DECLARE @AttributeName NVARCHAR(40);
	DECLARE @Sql_stmt NVARCHAR(2000);
   
    SET @QuerySQL	= 'SELECT MT.DBColumnName, MT.AttributeValue, MT.AttributeCDOName, MT.ModifyAttributeSetupId,' 
					+ ' MT.AttributeName, C.ContainerID '
					+ ' FROM		##MultiLotsModifyAttrsTemp MT LEFT JOIN CONTAINER C ON C.ContainerName = MT.ContainerName' 
					+ ' WHERE AttributeCDOName = ''LotAttributesEx'' AND IsUpdated = 1';

					PRINT('@Sql_stmt: '+@QuerySQL);  
	
	SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	
	FETCH NEXT FROM @c1 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@ContainerId;
	WHILE(@@fetch_status = 0)
		BEGIN
			SET @QuerySQL2	= 'Select @RowCount = COUNT(*) FROM A_LotAttributesEx '
							+ 'WHERE AttributeId = '''+@ModifyAttributeSetupId+''' AND ContainerId = '''+@ContainerId+'''';
            PRINT('@Sql_stmt: '+@QuerySQL2);        
			EXEC sp_executesql @QuerySQL2, @param = N' @RowCount INTEGER OUTPUT', @RowCount = @RowCount OUTPUT;
			
			IF (@RowCount = 0)
				BEGIN
					SET @QuerySQL3	= 'INSERT INTO ##MultiLotsModifyAttrsTempAttrEx (AttributeId,ContainerID,AttributeValue,AttributeName) '
								+ ' VALUES ('''+ @ModifyAttributeSetupId +''', '''+ @ContainerId +''','''+ @ColumnValue +''','''+ @AttributeName +''') ';
					PRINT('@Sql_stmt: '+@QuerySQL3);   
                    EXECUTE sp_executesql @QuerySQL3; 
				END;
		
		FETCH NEXT FROM @c1 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@ContainerId;
		END;
	CLOSE @c1;
	DEALLOCATE @c1;
END;
GO

---------------------------------------------------------------------------------------------------------
-- Procedure to update container/lotattribute/lotattributeex use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a container, currentstatus, lotattribute and lotattributeex table
---------------------------------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilUpdateMultiLotsValueChildTemp
AS
BEGIN
	DECLARE @TableName  NVARCHAR(40);
	DECLARE @ContainerName  NVARCHAR(40);
	DECLARE @HistoryMainline  CHAR(16);
	DECLARE @ContainerStatusChangeHistory  CHAR(16);
	DECLARE @ModifyAttrsHistory  CHAR(16);
    DECLARE @ModifyAttributeSetupId CHAR(16);
    DECLARE @CurrentStatusId CHAR(16);
    DECLARE @TxnId  CHAR(16);
    DECLARE @ContainerId  CHAR(16);
    DECLARE @LotAttributeId CHAR(16);
    DECLARE @AttributeName NVARCHAR(40);
	DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
    DECLARE @c2 CURSOR;
	DECLARE @QuerySQL2 NVARCHAR(4000);
    DECLARE @QuerySQL3 NVARCHAR(4000);
    DECLARE @QuerySQL4 NVARCHAR(4000);
	DECLARE @ColumnName NVARCHAR(4000);
	DECLARE @ColumnValue NVARCHAR(4000);
    DECLARE @CDOName NVARCHAR(4000);
    DECLARE @RowCount INTEGER;
	DECLARE @Sql_stmt NVARCHAR(2000);
	DECLARE @Sql_stmt2 NVARCHAR(2000);
	DECLARE @FieldType NVARCHAR(40);
	DECLARE @StringValue NVARCHAR(4000);
	DECLARE @LotAttributesChangeCount bit;
   
    SET @QuerySQL	= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, '
					+ ' TxnId,LotAttributesId, CurrentStatusId '
					+ ' FROM		##MultiLotsModifyAttrsTempChildContainer ';
	
	SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	
	FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;

	WHILE (@@fetch_status = 0)
		BEGIN
			SET @LotAttributesChangeCount = 1;
			SET @QuerySQL2	= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value,'
								+ 'AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
								+ 'FROM		##MultiLotsModifyAttrsChildTemp WHERE ContainerName = '''+@ContainerName+''' AND IsUpdated = 1';
			PRINT('@Sql_stmt: '+@QuerySQL2);	
			
			SET @Sql_stmt2 = N'SET @c2 = CURSOR FAST_FORWARD FOR ' + @QuerySQL2 + ' FOR READ ONLY; OPEN @c2'    
			EXEC sp_executesql @Sql_stmt2, N'@c2 CURSOR OUTPUT', @c2 OUTPUT
			
			FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
			
			WHILE(@@fetch_status = 0)
				BEGIN
				PRINT('DBColumnName: ' + @ColumnName + ' ,Value: ' + @ColumnValue + ' ,AttributeName: ' + @AttributeName+',@CurrentStatusId' +@CurrentStatusId);
					
						BEGIN
							IF (@ColumnValue IS NULL) 
								BEGIN
									SET @StringValue = 'NULL' ;
								END
							ELSE
								BEGIN
									SET @StringValue = ''''+@ColumnValue+'''' ;
								END
						END;
					IF(@CDOName ='Container') 
						BEGIN
							IF (@AttributeName != 'Factory' AND @AttributeName != 'Location') 
								BEGIN
									SET @QuerySQL3	= 'UPDATE CONTAINER SET '+ @ColumnName +' = '+ @StringValue +', '
													+ ' LastActivityDate = GETDATE(), '
													+ 'LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE ContainerName = @ContainerName ';
									PRINT('@Sql_stmt: '+@QuerySQL3);
									EXEC sp_executesql @QuerySQL3, @param = N' @ContainerName NVARCHAR(40)', @ContainerName = @ContainerName;
								END;						
						END;
					
					IF(@CDOName ='LotAttributes')
						BEGIN
							IF (@LotAttributesChangeCount = 1)
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue +','
											+ ' ChangeCount = ChangeCount + 1 '
											+ ' WHERE LotAttributesId = @LotAttributesId ';
									SET @LotAttributesChangeCount = 0;
								END;
							ELSE
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue
											+ ' WHERE LotAttributesId = @LotAttributesId ';
								END;
							EXEC sp_executesql @QuerySQL3, @param = N' @LotAttributesId NVARCHAR(40)', @LotAttributesId = @LotAttributeId;
						END;
						
					IF(@CDOName ='LotAttributesEx')
						BEGIN
							SET @QuerySQL3	= 'Select @RowCount = COUNT(*) FROM A_LotAttributesEx '
											+ 'WHERE AttributeId = '''+@ModifyAttributeSetupId+''' AND ContainerId = '''+@ContainerId+'''';
                    
							EXEC sp_executesql @QuerySQL3, @param = N' @RowCount INTEGER OUTPUT', @RowCount = @RowCount OUTPUT;
							
							IF (@RowCount > 0) 
								BEGIN
									SET @QuerySQL4	= 'UPDATE A_LotAttributesEx SET AttributeValue = '+ @StringValue +',' 	
													+ ' ChangeCount = ChangeCount + 1, LastTimestamp = GETDATE() '
													+ ' WHERE AttributeId = @LotAttributesId AND ContainerId = @Container ';
										
									EXEC sp_executesql @QuerySQL4, @param = N' @LotAttributesId CHAR(16), @Container CHAR(16)', @LotAttributesId = @ModifyAttributeSetupId, @Container = @ContainerId;
								END;
						END;
				
					FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
				END;
			CLOSE @c2;
			DEALLOCATE @c2;
			
			FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;
		END;
		CLOSE @c1;
		DEALLOCATE @c1;
END;
GO

-------------------------------------------------------------------------------
-- Procedure to update lotattributeex data use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a temp table 
-------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilGetInsertLotAttrExChild
AS
BEGIN 
	DECLARE @TableName  NVARCHAR(40);
	DECLARE @ContainerName  NVARCHAR(40);
	DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
	DECLARE @QuerySQL2 NVARCHAR(4000);
    DECLARE @QuerySQL3 NVARCHAR(4000);
	DECLARE @ColumnName NVARCHAR(4000);
	DECLARE @ColumnValue NVARCHAR(4000);
    DECLARE @CDOName NVARCHAR(4000);
    DECLARE @RowCount INTEGER;
    DECLARE @ContainerId  CHAR(16);
    DECLARE @ModifyAttributeSetupId CHAR(16);
    DECLARE @AttributeName NVARCHAR(40);
	DECLARE @Sql_stmt NVARCHAR(2000);
   
    SET @QuerySQL	= 'SELECT MT.DBColumnName, MT.AttributeValue, MT.AttributeCDOName, MT.ModifyAttributeSetupId,' 
					+ ' MT.AttributeName, C.ContainerID '
					+ ' FROM		##MultiLotsModifyAttrsChildTemp MT LEFT JOIN CONTAINER C ON C.ContainerName = MT.ContainerName' 
					+ ' WHERE AttributeCDOName = ''LotAttributesEx'' AND IsUpdated = 1';

					PRINT('@Sql_stmt: '+@QuerySQL);  
	
	SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	
	FETCH NEXT FROM @c1 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@ContainerId;
	WHILE(@@fetch_status = 0)
		BEGIN
			SET @QuerySQL2	= 'Select @RowCount = COUNT(*) FROM A_LotAttributesEx '
							+ 'WHERE AttributeId = '''+@ModifyAttributeSetupId+''' AND ContainerId = '''+@ContainerId+'''';
            PRINT('@Sql_stmt: '+@QuerySQL2);        
			EXEC sp_executesql @QuerySQL2, @param = N' @RowCount INTEGER OUTPUT', @RowCount = @RowCount OUTPUT;
			
			IF (@RowCount = 0)
				BEGIN
					SET @QuerySQL3	= 'INSERT INTO ##MultiLotsModifyAttrsTempAttrExChild (AttributeId,ContainerID,AttributeValue,AttributeName) '
								+ ' VALUES ('''+ @ModifyAttributeSetupId +''', '''+ @ContainerId +''','''+ @ColumnValue +''','''+ @AttributeName +''') ';
					PRINT('@Sql_stmt: '+@QuerySQL3);   
                    EXECUTE sp_executesql @QuerySQL3; 
				END;
		
		FETCH NEXT FROM @c1 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@ContainerId;
		END;
	CLOSE @c1;
	DEALLOCATE @c1;
END;
GO

---------------------------------------------------------------------------------------------------------
-- Procedure to update container/lotattribute/lotattributeex use by Multi Lots Modify Attrs (HPE) service
-- This procedure will update data in a container, currentstatus, lotattribute and lotattributeex table
---------------------------------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilUpdateMultiLotsValueTemp02
AS
BEGIN
	DECLARE @TableName  NVARCHAR(40);
	DECLARE @ContainerName  NVARCHAR(40);
	DECLARE @HistoryMainline  CHAR(16);
	DECLARE @ContainerStatusChangeHistory  CHAR(16);
	DECLARE @ModifyAttrsHistory  CHAR(16);
    DECLARE @ModifyAttributeSetupId CHAR(16);
    DECLARE @CurrentStatusId CHAR(16);
    DECLARE @TxnId  CHAR(16);
    DECLARE @ContainerId  CHAR(16);
    DECLARE @LotAttributeId CHAR(16);
    DECLARE @AttributeName NVARCHAR(40);
	DECLARE @QuerySQL NVARCHAR(4000);
    DECLARE @c1 CURSOR;
    DECLARE @c2 CURSOR;
	DECLARE @QuerySQL2 NVARCHAR(4000);
    DECLARE @QuerySQL3 NVARCHAR(4000);
    DECLARE @QuerySQL4 NVARCHAR(4000);
	DECLARE @ColumnName NVARCHAR(4000);
	DECLARE @ColumnValue NVARCHAR(4000);
    DECLARE @CDOName NVARCHAR(4000);
    DECLARE @RowCount INTEGER;
	DECLARE @Sql_stmt NVARCHAR(2000);
	DECLARE @Sql_stmt2 NVARCHAR(2000);
	DECLARE @FieldType NVARCHAR(40);
	DECLARE @StringValue NVARCHAR(4000);
	DECLARE @LotAttributesChangeCount bit;
	DECLARE @CurrentStatusChangeCount bit;
   
    SET @QuerySQL	= 'SELECT ContainerId, ContainerName,HistoryMainlineId, ContainerStatusChangeHistory,ModifyAttrsHistoryId, '
					+ ' TxnId,LotAttributesId, CurrentStatusId '
					+ ' FROM		##MultiLotsModifyAttrsTempContainer  WHERE ContainerSequence = 0';
	
	SET @Sql_stmt = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + @QuerySQL + ' FOR READ ONLY; OPEN @c1'    
    EXEC sp_executesql @Sql_stmt, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	
	FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;

	WHILE (@@fetch_status = 0)
		BEGIN
			SET @LotAttributesChangeCount = 1;
			SET @CurrentStatusChangeCount = 1;
			SET @QuerySQL2	= 'SELECT DBColumnName,CASE WHEN FieldType = ''NDO'' OR FieldType = ''RDO'' THEN ObjectID ELSE AttributeValue END AS Value,'
								+ 'AttributeCDOName, ModifyAttributeSetupId, AttributeName, FieldType '
								+ 'FROM		##MultiLotsModifyAttrsTemp WHERE ContainerName = '''+@ContainerName+''' AND IsUpdated = 1';
			PRINT('@Sql_stmt: '+@QuerySQL2);	
			
			SET @Sql_stmt2 = N'SET @c2 = CURSOR FAST_FORWARD FOR ' + @QuerySQL2 + ' FOR READ ONLY; OPEN @c2'    
			EXEC sp_executesql @Sql_stmt2, N'@c2 CURSOR OUTPUT', @c2 OUTPUT
			
			FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
			
			WHILE(@@fetch_status = 0)
				BEGIN
				PRINT('DBColumnName: ' + @ColumnName + ' ,Value: ' + @ColumnValue + ' ,AttributeName: ' + @AttributeName+',@CurrentStatusId' +@CurrentStatusId);
					
						BEGIN
							IF (@ColumnValue IS NULL) 
								BEGIN
									SET @StringValue = 'NULL' ;
								END
							ELSE
								BEGIN
									SET @StringValue = ''''+@ColumnValue+'''' ;
								END
						END;
					IF(@CDOName ='Container') 
						BEGIN
							IF (@AttributeName != 'Factory' AND @AttributeName != 'Location') 
								BEGIN
									SET @QuerySQL3	= 'UPDATE CONTAINER SET '+ @ColumnName +' = '+ @StringValue +', '
													+ ' LastActivityDate = GETDATE(), '
													+ 'LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE ContainerName = @ContainerName ';
									PRINT('@Sql_stmt: '+@QuerySQL3);
									EXEC sp_executesql @QuerySQL3, @param = N' @ContainerName NVARCHAR(40)', @ContainerName = @ContainerName;
								END;
							ELSE
								BEGIN
									IF (@CurrentStatusChangeCount = 1)
										BEGIN
											SET @QuerySQL3	 = 'UPDATE CurrentStatus SET '+ @ColumnName +' = '+ @StringValue +','
													+ ' ChangeCount = ChangeCount + 1, LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE CurrentStatusId = @CurrentStatusId';
											SET @CurrentStatusChangeCount = 0;
										END;
									ELSE
										BEGIN
											SET @QuerySQL3	 = 'UPDATE CurrentStatus SET '+ @ColumnName +' = '+ @StringValue +','
													+ ' LastRevTxnId = '''+@TxnID+''''
													+ ' WHERE CurrentStatusId = @CurrentStatusId';
										END;
									PRINT('@Sql_stmt: '+@QuerySQL3);
									EXEC sp_executesql @QuerySQL3, @param = N' @CurrentStatusId CHAR(16)', @CurrentStatusId = @CurrentStatusId;
								END;
						END;
					
					IF(@CDOName ='LotAttributes')
						BEGIN
							IF (@LotAttributesChangeCount = 1)
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue +','
											+ ' ChangeCount = ChangeCount + 1 '
											+ ' WHERE LotAttributesId = @LotAttributesId ';
									SET @LotAttributesChangeCount = 0;
								END;
							ELSE
								BEGIN
									SET @QuerySQL3	= 'UPDATE A_LotAttributes SET '+ @ColumnName +' = '+ @StringValue
											+ ' WHERE LotAttributesId = @LotAttributesId ';
								END;
							EXEC sp_executesql @QuerySQL3, @param = N' @LotAttributesId NVARCHAR(40)', @LotAttributesId = @LotAttributeId;
						END;
						
					IF(@CDOName ='LotAttributesEx')
						BEGIN
							SET @QuerySQL3	= 'Select @RowCount = COUNT(*) FROM A_LotAttributesEx '
											+ 'WHERE AttributeId = '''+@ModifyAttributeSetupId+''' AND ContainerId = '''+@ContainerId+'''';
                    
							EXEC sp_executesql @QuerySQL3, @param = N' @RowCount INTEGER OUTPUT', @RowCount = @RowCount OUTPUT;
							
							IF (@RowCount > 0) 
								BEGIN
									SET @QuerySQL4	= 'UPDATE A_LotAttributesEx SET AttributeValue = '+ @StringValue +',' 	
													+ ' ChangeCount = ChangeCount + 1, LastTimestamp = GETDATE() '
													+ ' WHERE AttributeId = @LotAttributesId AND ContainerId = @Container ';
										
									EXEC sp_executesql @QuerySQL4, @param = N' @LotAttributesId CHAR(16), @Container CHAR(16)', @LotAttributesId = @ModifyAttributeSetupId, @Container = @ContainerId;
								END;
						END;
				
					FETCH NEXT FROM @c2 INTO @ColumnName,@ColumnValue,@CDOName,@ModifyAttributeSetupId,@AttributeName,@FieldType;
				END;
			CLOSE @c2;
			DEALLOCATE @c2;
			
			FETCH NEXT FROM @c1 INTO @ContainerId,@ContainerName,@HistoryMainline,@ContainerStatusChangeHistory,@ModifyAttrsHistory, @TxnID, @LotAttributeId,@CurrentStatusId;
		END;
		CLOSE @c1;
		DEALLOCATE @c1;
END;
GO

---------------------------------------------------------------------------------------------------------
-- Procedure to insert pre-build attribute details to a general temp that use by Multi Lots Modify Attrs (HPE) service
-- This procedure will flatten the long string in ##BaseAttrTempupdate and insert to ##MultiLotsModifyAttrsTemp
---------------------------------------------------------------------------------------------------------
CREATE or ALTER PROCEDURE clfutilInsertSplitTempData (
    @delimiter CHAR(2)
    )
AS
BEGIN
    DECLARE 
		@ContainerName NVARCHAR(40),
		@ApplyToChildLots NUMERIC(1),
		@AttributeName NVARCHAR(Max),
		@AttributeValue NVARCHAR(Max),
		@AttributeRevision NVARCHAR(Max),
		@AttributeNameStr NVARCHAR(Max),
		@AttributeValueStr NVARCHAR(Max),
		@AttributeRevisionStr NVARCHAR(Max),
        @pos1 INT = 0,
		@pos2 INT = 0,
		@pos3 INT = 0,
		@len INT = 0,
        @SQLString NVARCHAR(2000),
		@DelLength INT = 0,
		@c1 CURSOR

	SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR select ContainerName, AttributeName, AttributeValue, AttributeRevision,ApplyToChildLots from ##BaseAttrTemp FOR READ ONLY; OPEN @c1'
	EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
	FETCH NEXT FROM @c1 INTO @ContainerName, @AttributeNameStr, @AttributeValueStr, @AttributeRevisionStr, @ApplyToChildLots
	WHILE(@@fetch_status = 0)   
		BEGIN

			SET @DelLength = LEN(@delimiter);
			SET @pos1 = 1;
			SET @pos2 = 1;
			SET @pos3 = 1;
			SET @len = 0;
			SET @AttributeNameStr = @AttributeNameStr + @delimiter
			SET @AttributeValueStr = @AttributeValueStr + @delimiter
			SET @AttributeRevisionStr = @AttributeRevisionStr + @delimiter
			
			WHILE CHARINDEX(@delimiter, @AttributeNameStr, @pos1) > 0
				BEGIN
					
					SET @len = CASE WHEN CHARINDEX(@delimiter, @AttributeNameStr, @pos1 ) - (@pos1) <= 0 THEN 0
					ELSE CHARINDEX(@delimiter, @AttributeNameStr, @pos1) - @pos1 END

					SET @AttributeName  = SUBSTRING(@AttributeNameStr, @pos1, @len)
					SET @pos1 = CHARINDEX(@delimiter, @AttributeNameStr, @pos1 + @len) + @DelLength
					
					SET @len = CASE WHEN CHARINDEX(@delimiter, @AttributeValueStr, @pos2 ) - (@pos2) <= 0 THEN 0
					ELSE CHARINDEX(@delimiter, @AttributeValueStr, @pos2) - @pos2 END

					SET @AttributeValue  = SUBSTRING(@AttributeValueStr, @pos2, @len)
					SET @pos2 = CHARINDEX(@delimiter, @AttributeValueStr, @pos2 + @len) + @DelLength
					
					SET @len = CASE WHEN CHARINDEX(@delimiter, @AttributeRevisionStr, @pos3 ) - (@pos3) <= 0 THEN 0
					ELSE CHARINDEX(@delimiter, @AttributeRevisionStr, @pos3) - @pos3 END

					SET @AttributeRevision = SUBSTRING(@AttributeRevisionStr, @pos3, @len )
					SET @pos3 = CHARINDEX(@delimiter, @AttributeRevisionStr, @pos3 + @len) + @DelLength
					
					INSERT INTO ##MultiLotsModifyAttrsTemp (ContainerName,AttributeName,AttributeValue,AttributeRevision,ApplyToChildLots)
					SELECT LTRIM(RTRIM(@ContainerName)) As Column1,LTRIM(RTRIM(@AttributeName)) As Column2,LTRIM(RTRIM(@AttributeValue)) As Column3,LTRIM(RTRIM(@AttributeRevision)) As Column4,CONVERT(Numeric, @ApplyToChildLots) As Column5
				END
			FETCH NEXT FROM @c1 INTO @ContainerName, @AttributeNameStr, @AttributeValueStr, @AttributeRevisionStr, @ApplyToChildLots
		END
		CLOSE @c1
		DEALLOCATE @c1
    RETURN
END
GO

-------------------------------------------------------------------------------
-- Recursively check a workflow for any step requiring a Pre-Production Procedure
-------------------------------------------------------------------------------
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetPreProductionProcedureRequired' 
	   AND 	  type = 'TF')
    DROP FUNCTION csiGetPreProductionProcedureRequired
GO

CREATE FUNCTION csiGetPreProductionProcedureRequired( @WorkflowId CHAR(16) )
RETURNS @ReturnTable TABLE (RequirePreProc INT)
AS
BEGIN
	DECLARE @RequirePreProc INT
	DECLARE @StepType INT
	DECLARE @SubWorkflowId CHAR(16)
	DECLARE @SpecRequiresPreProc INT

	DECLARE csr CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR 
	select step.StepType, subFlow.WorkflowId, spec.RequirePreProductionProcedure
	from WorkflowStep step
	left join Spec spec on ( step.SpecId is not null and (
		(spec.SpecId = step.SpecId) or
		(step.SpecBaseId <> '0000000000000000' and spec.SpecId in (select RevOfRcdId from SpecBase where SpecBaseId = step.SpecBaseId))))
	left join SpecBase sb on sb.SpecBaseId = spec.SpecBaseId
	left join Workflow subFlow on ( step.SubWorkflowId is not null and (
		(subFlow.WorkflowId = step.SubWorkflowId) or
		(step.SubWorkflowBaseId <> '0000000000000000' and subFlow.WorkflowId in (select RevOfRcdId from WorkflowBase where WorkflowBaseId = step.SubWorkflowBaseId))))
	where step.WorkflowId = @WorkflowId

	SET @RequirePreProc = 0;

	OPEN csr
	FETCH NEXT FROM csr into @StepType, @SubWorkflowId, @SpecRequiresPreProc
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @StepType = 1
			-- is a spec step
			SET @RequirePreProc = @SpecRequiresPreProc
		ELSE 
			-- is a sub-workflow step
			select @RequirePreProc = RequirePreProc from csiGetPreProductionProcedureRequired(@SubWorkflowId)

		IF @RequirePreProc = 1
			BREAK

		FETCH NEXT FROM csr into @StepType, @SubWorkflowId, @SpecRequiresPreProc
	END
	CLOSE csr
	DEALLOCATE csr

	INSERT INTO @returnTable VALUES (@RequirePreProc)
	RETURN 

END
GO
