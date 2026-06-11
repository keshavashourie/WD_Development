--------------------------------------------------------------------------------
-- SCRIPT: csiGetEnumerationLabels.sql
-- DESCR: Returns a list of Enum values and appropriate labels based on Dictionaries
--
-- Copyright Siemens 2025
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('CSIGETENUMERATIONLABELS')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS CSIGETENUMERATIONLABELS;
 	END IF;
END $$;
CREATE FUNCTION CSIGETENUMERATIONLABELS(
    p_Enumeration VARCHAR(30),
    p_PrimaryDictionary VARCHAR(16),
    p_SecondaryDictionary VARCHAR(16)
) 
RETURNS TABLE(DefaultValue TEXT, LabelValue VARCHAR) AS $$
BEGIN
    RETURN QUERY 
        SELECT
            f.DefaultValue,
            COALESCE(Term.labelvalue, Lang.labelvalue, l.labelvalue) AS LabelValue
        FROM CDOFields f
        JOIN CDODefinition c ON c.CDODefID = f.CDODefID
        JOIN Labels l ON l.LabelID = f.LabelID
        LEFT JOIN DictionaryLabel Term ON Term.labelid = l.labelid AND Term.dictionaryid = p_PrimaryDictionary
        LEFT JOIN DictionaryLabel Lang ON Lang.labelid = l.labelid AND Lang.dictionaryid = p_SecondaryDictionary
        WHERE c.CDOName = p_Enumeration;

    RETURN;
END;
$$ LANGUAGE plpgsql;

--------------------------------------------------------------------------------
-- PROCEDURE: csiPRDGetNextInstanceId
-- DESCR: Helper function to create instance id string from a CDODefId and
--        Instance Id number and Site.
--
-- Copyright Siemens 2023  
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiPRDGetNextInstanceId')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiPRDGetNextInstanceId;
 	END IF;
END $$;
CREATE PROCEDURE csiPRDGetNextInstanceId(
	pCDODefId IN INTEGER, 
	pInstanceIdStr OUT VARCHAR(16)
)
LANGUAGE plpgsql
AS
$$
DECLARE
	n_ErrLocator			NUMERIC;
	v_ErrMsg				VARCHAR(1024);
	v_CDODefIdStr 			VARCHAR(16);
	v_InstIdNewValue 		CHAR(16);
	i_InstIdInt				BIGINT;
	v_HexSite				VARCHAR(16);
	v_HexId					VARCHAR(16);
	v_Query					VARCHAR(100);
	v_IdCount              	INTEGER;
-- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
-- Length should be 10 chars
BEGIN
	CALL csiUpdateInstanceID (0, pCDODefId, 1, v_InstIdNewValue);
	--
	-- Add the Site
	--
	SELECT COUNT(*) INTO v_IdCount FROM DBIdentifier;
	IF(v_IdCount != 1) THEN
		SELECT ('x'||lpad('0000000000',16,'0'))::bit(64)::bigint INTO v_HexSite FROM generate_series(1, 1);
	ELSE
		SELECT ('x'||lpad(COALESCE(DBIdentifier,'0'),16,'0'))::bit(64)::bigint INTO v_HexSite FROM DBIdentifier;
	END IF;
	v_InstIdNewValue := SUBSTRING(v_InstIdNewValue, 7, 10);

	v_HexId := ('x'||lpad(v_InstIdNewValue,16,'0'))::bit(64)::bigint;
	i_InstIdInt := v_HexSite::BIGINT + v_HexId::BIGINT - (v_HexSite::BIGINT & v_HexId::BIGINT);	-- this is doing a Bitwise or
	v_InstIdNewValue:=LPAD(to_hex(i_InstIdInt), 10, '0');
	--
	v_CDODefIdStr:=LPAD(to_hex(pCDODefId),6,'0');
	pInstanceIdStr:=LOWER(v_CDODefIdStr||v_InstIdNewValue);
	
	EXCEPTION WHEN NO_DATA_FOUND THEN
		v_InstIdNewValue:=LPAD(SUBSTR(v_InstIdNewValue,14,10),10,'0');      
		v_CDODefIdStr:=LPAD(to_hex(pCDODefId),6,'0');
		pInstanceIdStr:=LOWER(v_CDODefIdStr||v_InstIdNewValue);
END;
$$;

--------------------------------------------------------------------------------------------------
-- Function to recursively check if a resource is in a resource group
-- DESCR: Can specify resource and group by ID or name. 
--		  Support for name was mainly a development convenience so can remove if not needed.
-- 
-- Copyright Siemens 2023  
--------------------------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiResourceInGroup')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiResourceInGroup;
 	END IF;
END $$;
CREATE FUNCTION csiResourceInGroup(
    inResourceId VARCHAR(16),
    inResourceName VARCHAR(30),
    inResourceGroupId VARCHAR(16),
    inResourceGroupName VARCHAR(30)
)
RETURNS TABLE (FoundItem INTEGER, ResourceName VARCHAR(30)) AS $$
DECLARE
    v_Found INTEGER;
    v_ChildGroupId VARCHAR(16);
   
    v_FoundRow record;
    tmpResourceId VARCHAR(16);
    tmpResourceName VARCHAR(30);
    tmpResourceGroupId VARCHAR(16);
    tmpResourceGroupName VARCHAR(30);
begin

	tmpResourceId := inResourceId;
    tmpResourceName := inResourceName;
    tmpResourceGroupId := inResourceGroupId;
    tmpResourceGroupName := inResourceGroupName;
   
    -- support params set by id or name and validate
    IF tmpResourceId IS NULL OR tmpResourceId = '' THEN
        IF tmpResourceName IS NOT NULL THEN
           SELECT rd.ResourceId INTO tmpResourceId FROM ResourceDef rd WHERE rd.ResourceName = tmpResourceName;
        END IF;
    END IF;

    IF tmpResourceGroupId IS NULL OR tmpResourceGroupId = '' THEN
        IF tmpResourceGroupName IS NOT NULL THEN
            SELECT ResourceGroupId INTO tmpResourceGroupId FROM ResourceGroup WHERE ResourceGroupName = tmpResourceGroupName;
        END IF;
    END IF;

    IF tmpResourceId IS NULL OR tmpResourceGroupId IS NULL THEN
        RETURN;
    END IF;

    IF tmpResourceName IS NULL THEN
        SELECT rd.ResourceName INTO tmpResourceName FROM ResourceDef rd WHERE rd.ResourceId = tmpResourceId;
    END IF;

    -- check if the resource is one of the entries of the specified group
    SELECT COUNT(*) INTO v_Found FROM ResourceGroupEntries WHERE ResourceGroupId = tmpResourceGroupId AND EntriesId = tmpResourceId;

    -- if not, check the child groups of the specified group
    IF v_Found = 0 THEN
        FOR v_ChildGroupId IN
            SELECT GroupsId FROM ResourceGroupGroups WHERE ResourceGroupId = tmpResourceGroupId
        loop
	       
            SELECT COUNT(*) INTO v_Found FROM ResourceGroupEntries WHERE ResourceGroupId = v_ChildGroupId AND EntriesId = tmpResourceId;
            IF v_Found = 1 THEN
                EXIT;
            END IF;

			FOR v_FoundRow IN SELECT * FROM csiResourceInGroup(tmpResourceId, NULL, v_ChildGroupId, NULL)
			loop
			    v_Found := v_FoundRow.FoundItem;
			    IF v_Found = 1 THEN
			        EXIT;
			    END IF;
			END LOOP;

        END LOOP;
    END IF;
   
    IF v_Found = 1 THEN
        RETURN QUERY SELECT v_Found, tmpResourceName;
    END IF;
END;
$$ LANGUAGE plpgsql;

--------------------------------------------------------------------------------
-- PROCEDURE: csiGenerateAutoNumber
-- DESCR: Function to generate sequences (for NumberingRule) with the following format <vPrefix><sequence><vSuffix>
--        Returns the Auto Numbers in the following format <AutoNumber01>|<AutoNumber02>|<AutoNumber03>
--
-- Copyright Siemens 2023  
--------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGenerateAutoNumber')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGenerateAutoNumber;
 	END IF;
END $$;
CREATE FUNCTION csiGenerateAutoNumber(
    SequencesRequested integer, 
    i_Prefix varchar(40), 
    Suffix varchar(40), 
    LastSequence integer, 
    SequenceLength integer, 
    UseHex integer)
RETURNS TABLE (AutoNumber TEXT)
language plpgsql
as $$
DECLARE
    v_Sequence integer;
    SequenceValue varchar(40);
    IsFirst integer;
    ParmValue TEXT;
    RowsProcessed integer;
    
    ErrLoc integer;
    Delim char(1);
    Counter integer;
	v_temp TEXT;
BEGIN

    IsFirst := 1;
    ParmValue := '';
    RowsProcessed := 0;
	
	Delim := '|';
    Counter := 0;

	-- Create temp table to simulate table-valued FUNCTION
	v_temp := 'tmpresults_' || to_char(clock_timestamp(), 'YYYYMMDDHH24MISSMS');
    execute format('create local TEMPORARY TABLE %I (
		AutoNumber TEXT
	) on commit drop', v_temp);

	ErrLoc := 1;

    -- Append the padding for the sequence length
    v_Sequence := LastSequence;

    Counter := 1;
    WHILE Counter <= SequencesRequested LOOP
        -- Increment the sequence
        v_Sequence := v_Sequence + 1;

        -- Set the padding for the sequence length
        SequenceValue := RIGHT(LPAD(v_Sequence::TEXT, SequenceLength, '0'), SequenceLength);

        IF UseHex = 1 THEN
            SequenceValue := RIGHT(LPAD(UPPER(TO_HEX(v_Sequence)), SequenceLength, '0'), SequenceLength);
        END IF;

        SequenceValue := i_Prefix || SequenceValue || Suffix;

        IF IsFirst <> 1 THEN
            ParmValue := ParmValue || Delim;
        END IF;

        ParmValue := ParmValue || SequenceValue;
        IsFirst := 2;

        Counter := Counter + 1;
    END LOOP;

    -- Insert into temp_table
	EXECUTE FORMAT('INSERT INTO %I (AutoNumber) VALUES ($1)', v_temp) using ParmValue;
	
	-- Return QUERY
	RETURN QUERY
	execute format('SELECT * FROM %I', v_temp);
END;
$$;

-------------------------------------------------------------------------------
-- csiGetContainerMaterialList
-- Gets material list that defines required components for the container.
-- Used with a high volume machine setup to determine what components were issued.
-- All params should come from the HV history records to use MfgOrder and BOM at time of HV issue.
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetContainerMaterialList')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetContainerMaterialList;
 	END IF;
END $$;
CREATE FUNCTION csiGetContainerMaterialList(
    inContainerId VARCHAR(16),
    inMfgOrderId VARCHAR(16),
    inBOMId VARCHAR(16)
)
RETURNS TABLE (
    ProductId VARCHAR(16),
    ReferenceDesignator VARCHAR(30),
    SpecID VARCHAR(16),
    QtyRequired FLOAT,
    MaterialListItemId VARCHAR(16)
) AS $$
DECLARE
    HaveList INTEGER;
    ItemCount INTEGER;
   
    tmpBOMId VARCHAR(16);
BEGIN
    HaveList := 0;
    
    -- Container.MaterialList
    SELECT count(*) INTO ItemCount FROM ContainerMaterialListItem c WHERE c.ContainerId = inContainerId;
    IF (ItemCount > 0) THEN
        RETURN QUERY 
        SELECT
            p.ProductId::VARCHAR(16),
            item.ReferenceDesignator::VARCHAR(30),
            NULL::VARCHAR(16),
            item.QtyRequired,
            item.ContainerMaterialListItemId::VARCHAR(16)
        FROM ContainerMaterialListItem item
        LEFT JOIN Product p ON (
            (p.ProductId = item.ProductId) OR
            (item.ProductBaseId <> '0000000000000000' AND p.ProductId IN (SELECT RevOfRcdId FROM ProductBase WHERE ProductBaseId = item.ProductBaseId))
        )
        WHERE item.ContainerId = inContainerId;
        HaveList := 1;
    END IF;

    -- MfgOrder.MaterialList
    IF (HaveList = 0 AND inMfgOrderId IS NOT NULL AND inMfgOrderId <> '') THEN
        SELECT count(*) INTO ItemCount FROM MfgOrderMaterialListItem WHERE MfgOrderId = inMfgOrderId;
        IF (ItemCount > 0) THEN
            RETURN QUERY 
            SELECT
                p.ProductId::VARCHAR(16),
                item.ReferenceDesignator::VARCHAR(30),
                NULL::VARCHAR(16),
                item.QtyRequired,
                item.MfgOrderMaterialListItemId::VARCHAR(16)
            FROM MfgOrderMaterialListItem item
            LEFT JOIN Product p ON (
                (p.ProductId = item.ProductId) OR
                (item.ProductBaseId <> '0000000000000000' AND p.ProductId IN (SELECT RevOfRcdId FROM ProductBase WHERE ProductBaseId = item.ProductBaseId))
            )
            WHERE item.MfgOrderId = inMfgOrderId;
            HaveList := 1;
        END IF;
    END IF;

    -- BOM.MaterialList
    -- Check if BOMId is an ERPBOM
    IF (HaveList = 0 AND inBOMId IS NOT NULL AND inBOMId <> '') then    
   		IF EXISTS (SELECT 1 FROM ERPBOM WHERE ERPBOMId = inBOMId) then -- Is an ERPBOM
            RETURN QUERY 
            SELECT
                p.ProductId::VARCHAR(16),
                item.ReferenceDesignator::VARCHAR(30),
                NULL::VARCHAR(16),
                item.QtyRequired,
                item.BOMMaterialListItemId::VARCHAR(16)
            FROM BOMMaterialListItem item
            LEFT JOIN Product p ON (
                (p.ProductId = item.ProductId) OR
                (item.ProductBaseId <> '0000000000000000' AND p.ProductId IN (SELECT RevOfRcdId FROM ProductBase WHERE ProductBaseId = item.ProductBaseId))
            )
            WHERE item.ERPBOMId = inBOMId;
        else
        	RETURN QUERY 
            SELECT
                p.ProductId::VARCHAR(16),
                item.ReferenceDesignator::VARCHAR(30),
                s.SpecId::VARCHAR(16),
                item.QtyRequired,
                item.ProductMaterialListItemId::VARCHAR(16)
            FROM ProductMaterialListItem item
            LEFT JOIN Product p ON (
                (p.ProductId = item.ProductId) OR
                (item.ProductBaseId <> '0000000000000000' AND p.ProductId IN (SELECT RevOfRcdId FROM ProductBase WHERE ProductBaseId = item.ProductBaseId))
            )
            LEFT JOIN Spec s ON (
                (s.SpecId = item.SpecId) OR
                (item.SpecBaseId <> '0000000000000000' AND s.SpecId IN (SELECT RevOfRcdId FROM SpecBase WHERE SpecBaseId = item.SpecBaseId))
            )
            WHERE item.BOMId = inBOMId;
        END IF;
    END IF;
    
    RETURN;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- procedure to increase length
-- Works OK to increase, but does not reduce. don't know why but following works when run directly
-- alter table PRODUCTBASE modify PRODUCTNAME VARCHAR2(100);
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiIncreaseStringColMaxLength')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiIncreaseStringColMaxLength;
 	END IF;
END $$;
CREATE PROCEDURE csiIncreaseStringColMaxLength(
	TableName VARCHAR(50),
	ColName VARCHAR(50),
	NewMaxLength INTEGER
)
LANGUAGE plpgsql
AS
$$
DECLARE
	CurrentMaxLength INTEGER;
	AlterSQL VARCHAR(1000);
	Message VARCHAR(1000);
	CurrentMaxLengthStr VARCHAR(10);
	NewMaxLengthStr VARCHAR(10);
BEGIN
	SELECT CHARACTER_MAXIMUM_LENGTH INTO CurrentMaxLength FROM INFORMATION_SCHEMA.COLUMNS WHERE lower(TABLE_NAME) = lower(TableName) and lower(COLUMN_NAME) = lower(ColName);
	CurrentMaxLengthStr := CAST(CurrentMaxLength as VARCHAR(10));
	NewMaxLengthStr := CAST(NewMaxLength as VARCHAR(10));
	
	IF CurrentMaxLength < NewMaxLength THEN
		AlterSQL := 'alter table ' || TableName || ' alter column ' || ColName || ' type varchar(' || NewMaxLengthStr || ');' ;
		RAISE NOTICE 'AlterSQL: %', AlterSQL;
		EXECUTE AlterSQL;
	END IF;
END;
$$;

-------------------------------------------------------------------------------
-- Procedure to validate NDO given its Table Name, Value and its Name field's Column Name
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidNDO')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidNDO;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidNDO (   
pTableName  VARCHAR(40),					 
pFieldValue VARCHAR(4000),
DBNameColumnName VARCHAR(40),
OUT oResult INTEGER
)
AS $$
DECLARE   
    vCount INTEGER;
    sql_stmt VARCHAR(2000);
    vFieldName VARCHAR(40);
	sql_table text;
BEGIN 
    vCount := 0;
	sql_stmt := 'select count(*) from %s where UPPER(%s)=UPPER($1);';
	sql_table := quote_ident(current_database()) || '.' || quote_ident(current_schema()) || '.' || pTableName;
									
   RAISE NOTICE 'stmt: %', format(sql_stmt, sql_table, DBNameColumnName);

   EXECUTE format(sql_stmt, sql_table, DBNameColumnName) INTO vCount USING pFieldValue;

	RAISE NOTICE 'Count: %', vCount::VARCHAR;

   IF(vCount = 0) THEN oResult := 1;
   END IF;
END; 
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate RDO given its Table Name, Value and its Name field's Column Name
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidRDO')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidRDO;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidRDO (   
pTableName  VARCHAR(40),				 
pFieldValue VARCHAR(4000),
pFieldRevision VARCHAR(4000),
DBColumnName VARCHAR(40),
DBBaseColumnName VARCHAR(40),
DBRevisionColName VARCHAR(40),
OUT oResult INTEGER
)
AS $$
DECLARE
    vCount INTEGER;
    sql_stmt VARCHAR(2000);
	sql_table text;

BEGIN
    vCount := 0;

	sql_stmt := 'select count(*) from %s PS join %sBase PSB on PS.%s = PSB.%s where PSB.%s = $1 and PS.%s = $2;';
	sql_table := quote_ident(current_database()) || '.' || quote_ident(current_schema()) || '.' || pTableName;

	RAISE NOTICE 'stmt: %', format(sql_stmt, sql_table, pTableName, DBBaseColumnName, DBBaseColumnName, DBColumnName, DBRevisionColName);

	EXECUTE format(sql_stmt, sql_table, pTableName, DBBaseColumnName, DBBaseColumnName, DBColumnName, DBRevisionColName) INTO vCount USING pFieldValue, pFieldRevision;

	RAISE NOTICE 'Count2: %', vCount::VARCHAR; 

	IF(vCount = 0) THEN oResult := 1;
   	END IF;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a String given its value and length
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidStrLength')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidStrLength;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidStrLength(
pPrecisionValue  INTEGER,
pFieldValue VARCHAR(4000),
OUT oResult INTEGER
)
AS $$
DECLARE  
    vLenght INTEGER;
    sql_stmt VARCHAR(2000);
	
BEGIN
    vLenght := 0;
	sql_stmt := 'select Length($1);';

   RAISE NOTICE 'stmt: %', sql_stmt;

   EXECUTE format(sql_stmt) INTO vLenght USING pFieldValue;

   RAISE NOTICE 'Count2: %', vLenght::VARCHAR;

   IF(vLenght > pPrecisionValue) THEN oResult = 1;
   END IF;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a given value is Numeric
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('is_numeric')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS is_numeric;
 	END IF;
END $$;
CREATE FUNCTION is_numeric(
	value VARCHAR(4000)
) 
RETURNS INTEGER 
AS $$
DECLARE 
	x NUMERIC;
BEGIN
    IF value IS NULL THEN
    	RETURN 0;
    ELSE
		-- Cast the given value into Numeric, return true when cast without issue
		x = value::NUMERIC;
    	RETURN 1;
    END IF;
	-- If the given value is unable to cast, return false
	EXCEPTION WHEN others THEN
		RETURN 0;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a Number given its value
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidNumber')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidNumber;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidNumber(
pFieldValue VARCHAR(4000),
OUT oResult INTEGER
)
AS $$
DECLARE  
    vNumberStatus INTEGER;
BEGIN
	vNumberStatus := 0;
	
	vNumberStatus := is_numeric(pFieldValue);

	RAISE NOTICE '%', vNumberStatus::VARCHAR;

	IF(vNumberStatus = 0) THEN oResult := 1;
	END IF;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a Boolean value given its value
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidBoolean')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidBoolean;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidBoolean(
pFieldValue VARCHAR(4000),
OUT oResult INTEGER
)
AS $$
DECLARE 
	vStatus VARCHAR(40);

BEGIN
	oResult := 1;
	IF ( UPPER(TRIM(pFieldValue)) = 'TRUE' OR pFieldValue = '1' OR UPPER(TRIM(pFieldValue)) = 'FALSE' OR pFieldValue = '0' )
    THEN oResult = 0;
    END IF;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a given value is TimeStamp
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('is_date')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS is_date;
 	END IF;
END $$;
CREATE FUNCTION is_date(
	value VARCHAR(4000)
) 
RETURNS INTEGER 
AS $$
BEGIN
    IF value IS NULL THEN
    	RETURN 0;
    ELSE
		-- Cast the given value into TimeStamp, return true when cast without issue
		PERFORM value::TIMESTAMP;
    	RETURN 1;
    END IF;
	EXCEPTION WHEN others THEN
		RETURN 0;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a TimeStamp given its value
-- output True or False
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('clfutilIsValidTimeStamp')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS clfutilIsValidTimeStamp;
 	END IF;
END $$;
CREATE PROCEDURE clfutilIsValidTimeStamp(
pFieldValue VARCHAR(4000),
OUT oResult INTEGER
)
AS
$$
DECLARE 
	vDateStatus INTEGER;
    sql_stmt VARCHAR(2000);
    param VARCHAR(100);
BEGIN   
    vDateStatus := 0;

	vDateStatus := is_date( pFieldValue );

	RAISE NOTICE '%', CAST(@vDateStatus AS VARCHAR);

   	IF(vDateStatus = 0) THEN
		oResult := 1;
	END IF;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------
-- Procedure to validate a Location NDO that model within in Facotry of a container
-- Given Location Value and Container Name
-- output True or False
-------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilIsValidLocation(
	pFieldName VARCHAR(40),
	pFieldValue VARCHAR(4000),
	pContainerName VARCHAR(40),
	OUT oResult INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE 
    vSql_stmt VARCHAR(2000);
	vFactoryName VARCHAR(40);
	vEmployeeName VARCHAR(40);
	vLoginUser VARCHAR(40);
	vCount INTEGER;
	vLCount INTEGER;
    vParam VARCHAR(100);
BEGIN
	vFactoryName := '';
	vEmployeeName := '';
    vLoginUser := 'LoginUser';

	vSql_stmt := 'Select value FROM CLFParameterCache_' || pg_backend_pid() || ' WHERE Name = UPPER($1); ';

	EXECUTE format(vSql_stmt) INTO vEmployeeName USING vLoginUser;

	IF (vEmployeeName = '') THEN
		vLCount := 0;
	ELSE
		BEGIN
			vSql_stmt := 'SELECT f.factoryname FROM Employee e JOIN sessionvalues s ON e.employeeid = s.employeeid JOIN factory f ON f.factoryid = s.factoryid WHERE employeename = $1;';

			EXECUTE format(vSql_stmt) INTO vFactoryName USING vEmployeeName;

			RAISE NOTICE 'Name: %', vFactoryName;

			vSql_stmt := 'SELECT count(*) FROM Factory F INNER JOIN Location L ON F.FactoryId = L.FactoryId WHERE F.FactoryName = UPPER( $1 ) AND L.LocationName = UPPER( $2 );';

			EXECUTE format(vSql_stmt) INTO vLCount USING vFactoryName, pFieldValue;
		END;
	END IF;

	IF(vLCount = 0) THEN
	BEGIN
		oResult := 1;
	END;
	END IF;
END $$;

-------------------------------------------------------------------------------
-- Gets high volume setup details for a specified setup
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetHVSetupDetails')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetHVSetupDetails;
 	END IF;
END $$;
CREATE FUNCTION csiGetHVSetupDetails(
	HVSetupId CHAR(16)
)
RETURNS TABLE (
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	CompID VARCHAR(32),
	CompName VARCHAR(32),
	Slot INTEGER,
	SubSlot INTEGER,
	ProductName VARCHAR(100),
	ProductRevision VARCHAR(25)
)
LANGUAGE plpgsql
as $$
DECLARE
BEGIN
	
	return query
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
	where ParentId = HVSetupId;

END;
$$;

-------------------------------------------------------------------------------
-- Get params needed to query for additional HV component issue information.
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetHVIssueHistoryParams')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetHVIssueHistoryParams;
 	END IF;
END $$;
CREATE FUNCTION csiGetHVIssueHistoryParams(
	i_HVIssueHistoryDetailId CHAR(16)
)
RETURNS TABLE(
	HVSetupId CHAR(16),
	SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	ContainerId CHAR(16),
	MfgOrderId CHAR(16),
	BOMId CHAR(16),
	ResourceName VARCHAR(100),
	WorkflowStepName VARCHAR(100),
	SpecName VARCHAR(100),
	TxnDateGMT TIMESTAMP,
	HVComponentIssueHistoryId CHAR(16)
)
LANGUAGE plpgsql
as $$
DECLARE
begin
	
	return query
	select 
		summary.HVSetupId,
		summary.SpecId,
		summary.WorkflowStepId,
		detail.ContainerId,
		detail.MfgOrderId, 
		detail.BOMId,
		res.ResourceName,
		wfs.WorkflowStepName,
		(sb.SpecName || ' (' || spec.SpecRevision || ')')::varchar(100),
		summary.TxnDateGMT,
		summary.HVComponentIssueHistoryId
	from HVIssueHistoryDetail detail
	join HVComponentIssueHistory summary on summary.HVComponentIssueHistoryId = detail.ParentId
	join ResourceDef res on res.ResourceId = summary.ResourceId
	join WorkflowStep wfs on wfs.WorkflowStepId = summary.WorkflowStepId
	join Spec spec on spec.SpecId = summary.SpecId
	join SpecBase sb on sb.SpecBaseId = spec.SpecBaseId
	where detail.HVIssueHistoryDetailId = i_HVIssueHistoryDetailId;
	
END;
$$;

-------------------------------------------------------------------------------
-- Get HV removed component information for a given container
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetHVRemovedComponents')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetHVRemovedComponents;
 	END IF;
END $$;
CREATE FUNCTION csiGetHVRemovedComponents(
	i_ContainerId CHAR(16),
	i_HVIssueHistoryDetailId CHAR(16)
)
RETURNS TABLE(
	HVRemoveId CHAR(16),
	HVRemoveDetailId CHAR(16),
	ContainerId CHAR(16),
	MaterialListItemId CHAR(16),
	QtyRemoved DOUBLE PRECISION, 
	DestinationLot VARCHAR(100),
	DestinationStockPoint VARCHAR(100),
	RemovalReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	HVSetupId CHAR(16),
	HVSetupDetailId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	SpecId CHAR(16),
	WorkflowStepId CHAR(16),
	TxnDateGMT TIMESTAMP
)
language plpgsql
as $$
DECLARE
BEGIN
	
	return query
	select
		rmDetail.HVRemoveHistoryDetailId as HVRemoveId,
		rmSetupDetail.HVRemoveHistorySetupDetailId as HVRemoveDetailId, 
		rmDetail.ContainerId, 
		rmDetail.MaterialListItemId::char(30), 
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
		rmSetupDetail.ContainerId = i_ContainerId -- container match
		and rmDetail.HVIssueHistoryDetailId = i_HVIssueHistoryDetailId;

END;
$$;

-------------------------------------------------------------------------------
-- Get details for a single high volume issue record (one container processed at one resource with an HV setup)
-- May return multiple rows for a ref des if component product defined in multiple slots

-- HVIssueHistoryDetail holds the container info, but HV setup, Resource, Spec and WorkflowStep are set in the parent HVComponentIssueHistory.
-- If Spec is set, it is used as a filter to get only material items with matching Spec. Otherwise, all material items are used.
-- The history detail record saves the BOM and MfgOrder at the time of container move, just in case these were changed after the move.
-- However, we cannot compensate for the material list for that BOM or MfgOrder being changed. 

-- We are requiring a material list since standard Component Issue page shows no materials to issue for a container if it has no material list
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetDetailsForSingleHVIssue')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetDetailsForSingleHVIssue;
 	END IF;
END $$;
CREATE FUNCTION csiGetDetailsForSingleHVIssue
(	
	i_HVIssueHistoryDetailId CHAR(16)
)
RETURNS TABLE 
(
	ReferenceDesignator VARCHAR(100),
	CompName VARCHAR(32),
	ProductName VARCHAR(100),
	ProductRevision VARCHAR(25),
	FromLot VARCHAR(100),
	IssueControl INTEGER,
	ResourceName VARCHAR(100),
	Slot INTEGER,
	SubSlot INTEGER,
	QtyIssued FLOAT,
	WorkflowStepName VARCHAR(100),
	SpecName VARCHAR(100),
	TxnDateGMT TIMESTAMP,
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
	RemoveDestinationLot VARCHAR(100),
	RemoveDestinationStockPoint VARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT TIMESTAMP
)
LANGUAGE plpgsql
AS $$
DECLARE
	-- get params needed from the history records
	v_HVSetupId CHAR(16);
	v_SpecId CHAR(16);
	v_WorkflowStepId CHAR(16);
	v_ContainerId CHAR(16);
	v_MfgOrderId CHAR(16);
	v_BOMId CHAR(16);
	v_ResourceName VARCHAR(100);
	v_WorkflowStepName VARCHAR(100);
	v_SpecName VARCHAR(100);
	v_TxnDateGMT TIMESTAMP;
	v_HVComponentIssueHistoryId CHAR(16);
BEGIN
	-- Create temporary table
--	DROP TABLE IF EXISTS temp_tablee;
--	create temporary table temp_tablee (
--		ReferenceDesignator VARCHAR(100),
--		CompName VARCHAR(32),
--		ProductName VARCHAR(100),
--		ProductRevision VARCHAR(25),
--		FromLot VARCHAR(100),
--		IssueControl INT,
--		ResourceName VARCHAR(100),
--		Slot INT,
--		SubSlot INT,
--		QtyIssued FLOAT,
--		WorkflowStepName VARCHAR(100),
--		SpecName VARCHAR(100),
--		TxnDateGMT TIMESTAMP,
--		HVResourceSetupHistoryId CHAR(16),
--		HVSetupHistoryDetailId CHAR(16),
--		ProductId CHAR(16),
--		ResourceId CHAR(16),
--		WorkflowStepId CHAR(16),
--		SpecId CHAR(16),
--		MaterialListItemId CHAR(16),
--		HVComponentIssueHistoryId CHAR(16),
--		HVIssueHistoryDetailId CHAR(16),
--		QtyRemoved FLOAT,
--		NetQtyIssued FLOAT,
--		RemoveDestinationLot VARCHAR(100),
--		RemoveDestinationStockPoint VARCHAR(100),
--		RemoveReasonId CHAR(16),
--		RemoveDifferenceReasonId CHAR(16),
--		RemoveSpecId CHAR(16),
--		RemoveStepId CHAR(16),
--		RemoveTxnDateGMT TIMESTAMP
--	) on commit DROP;
	
	
	select t.HVSetupId, t.SpecId, t.WorkflowStepId, t.ContainerId, t.MfgOrderId, t.BOMId, t.ResourceName, t.WorkflowStepName, t.SpecName, t.TxnDateGMT, t.HVComponentIssueHistoryId
	into v_HVSetupId, v_SpecId, v_WorkflowStepId, v_ContainerId, v_MfgOrderId, v_BOMId, v_ResourceName, v_WorkflowStepName, v_SpecName, v_TxnDateGMT, v_HVComponentIssueHistoryId
	from csiGetHVIssueHistoryParams(i_HVIssueHistoryDetailId) t;

	return query
	with MaterialList(ProductId, ReferenceDesignator, SpecId, QtyIssued, MaterialListItemId) as
	(
		select t.ProductId, t.ReferenceDesignator, t.SpecId, t.QtyRequired, t.MaterialListItemId from csiGetContainerMaterialList(v_ContainerId, v_MfgOrderId, v_BOMId) t
	),
	SetupDetails(HVResourceSetupHistoryId, HVSetupHistoryDetailId, ProductId, CompId, CompName, Slot, SubSlot, ProductName, ProductRevision) as
	(
		select t.HVResourceSetupHistoryId, t.HVSetupHistoryDetailId, t.ProductId, t.CompId, t.CompName, t.Slot, t.SubSlot, t.ProductName, t.ProductRevision from csiGetHVSetupDetails(v_HVSetupId) t
	),
	RemovedComponents(HVRemoveId, HVRemoveDetailId, ContainerId, MaterialListItemId, QtyRemoved, DestinationLot, DestinationStockPoint, RemovalReasonId, RemoveDifferenceReasonId, 
					  HVSetupId, HVSetupDetailId, HVIssueHistoryDetailId, SpecId, WorkflowStepId, TxnDateGMT
	) as (
		select * from csiGetHVRemovedComponents(v_ContainerId, i_HVIssueHistoryDetailId)
	)
	select 
		m.ReferenceDesignator,
		sd.CompName,
		sd.ProductName,
		sd.ProductRevision,
		sd.CompId as FromLot,
		3, -- IssueControl fixed to Lot and Stock Point
		v_ResourceName,
		sd.Slot,
		sd.SubSlot,
		m.QtyIssued,
		v_WorkflowStepName,
		v_SpecName,
		v_TxnDateGMT,
		sd.HVResourceSetupHistoryId,
		sd.HVSetupHistoryDetailId,
		sd.ProductId,
		res.ResourceId,
		v_WorkflowStepId,
		v_SpecId,
		m.MaterialListItemId::char(16),
		v_HVComponentIssueHistoryId,
		i_HVIssueHistoryDetailId,
		COALESCE(rc.QtyRemoved, 0),
		CASE WHEN rc.QtyRemoved is not null THEN m.QtyIssued - rc.QtyRemoved ELSE m.QtyIssued END,
		rc.DestinationLot,
		rc.DestinationStockPoint,
		rc.RemovalReasonId,
		rc.RemoveDifferenceReasonId,
		rc.SpecId,
		rc.WorkflowStepId,
		rc.TxnDateGMT
	from 
		SetupDetails sd
		join MaterialList m on m.ProductId = sd.ProductId and (m.SpecId is null or m.SpecId = v_SpecId)
		join HVResourceSetupHistory setup on setup.HVResourceSetupHistoryId = sd.HVResourceSetupHistoryId
		join ResourceDef res on res.ResourceId = setup.ResourceId
		left join RemovedComponents rc on
			rc.MaterialListItemId = m.MaterialListItemId				-- bom item
			and rc.HVSetupDetailId = sd.HVSetupHistoryDetailId			-- setup detail
			and rc.HVIssueHistoryDetailId = i_HVIssueHistoryDetailId;	-- HV issue txn
			
END;
$$;

-------------------------------------------------------------------------------
-- Gets all high volume issue details for a given container
-- May return multiple rows for a ref des if component product defined in multiple slots on same resource, or on different resources
 
-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetAllHVIssuesForContainer')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetAllHVIssuesForContainer;
 	END IF;
END $$;
CREATE FUNCTION csiGetAllHVIssuesForContainer
(	
	i_ContainerId CHAR(16),
	i_ContainerName varchar(100),
	i_ContainerOption integer
)
RETURNS TABLE 
(
	ContainerId CHAR(16),
	ContainerName varchar(100),
	ReferenceDesignator varchar(100),
	CompName varchar(32),
	ProductName varchar(100),
	ProductRevision varchar(25),
	FromLot varchar(100),
	IssueControl integer,
	ResourceName varchar(100),
	Slot integer,
	SubSlot integer,
	QtyIssued double precision,
	WorkflowStepName varchar(100),
	SpecName varchar(100),
	TxnDateGMT timestamp,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved double precision,
	NetQtyIssued double precision,
	RemoveDestinationLot varchar(100),
	RemoveDestinationStockPointeger varchar(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT timestamp
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
language plpgsql
as $$
declare
	v_HVIssueHistoryDetailId CHAR(16);
	v_ChildContainerId CHAR(16);
	v_ChildCount integer;
	v_ContainerStatus integer;
	v_ParentContainerId CHAR(16);

	ChildrenOrSingle integer;	--SET @ChildrenOrSingle = 0;
	Siblings integer;			--SET @Siblings = 1;
	Single integer;			--SET @Single = 2;

	GetSingle integer;
	GetByParent integer;
	GetByChildren integer;
	v_temp text;
BEGIN
	-- Create temporary table
	
	v_temp := 'tmpresults_' || to_char(clock_timestamp(), 'YYYYMMDDHH24MISSMS');
    execute format('create local TEMPORARY TABLE %I (
        ContainerId CHAR(16),
		ContainerName varchar(100),
		ReferenceDesignator varchar(100),
		CompName varchar(32),
		ProductName varchar(100),
		ProductRevision varchar(25),
		FromLot varchar(100),
		IssueControl integer,
		ResourceName varchar(100),
		Slot integer,
		SubSlot integer,
		QtyIssued double precision,
		WorkflowStepName varchar(100),
		SpecName varchar(100),
		TxnDateGMT timestamp,
		HVResourceSetupHistoryId CHAR(16),
		HVSetupHistoryDetailId CHAR(16),
		ProductId CHAR(16),
		ResourceId CHAR(16),
		WorkflowStepId CHAR(16),
		SpecId CHAR(16),
		MaterialListItemId CHAR(16),
		HVComponentIssueHistoryId CHAR(16),
		HVIssueHistoryDetailId CHAR(16),
		QtyRemoved double precision,
		NetQtyIssued double precision,
		RemoveDestinationLot varchar(100),
		RemoveDestinationStockPointeger varchar(100),
		RemoveReasonId CHAR(16),
		RemoveDifferenceReasonId CHAR(16),
		RemoveSpecId CHAR(16),
		RemoveStepId CHAR(16),
		RemoveTxnDateGMT timestamp
    ) on commit drop', v_temp);
   
	-- get info about specified container
	IF(i_ContainerId is not null and i_ContainerId <> '') THEN
		select c.ContainerName, c.ChildCount, c.Status, c.ParentContainerId into i_ContainerName, v_ChildCount, v_ContainerStatus, v_ParentContainerId from Container c where c.ContainerId = i_ContainerId;
	ELSE
		select c.ContainerId, c.ChildCount, c.Status, c.ParentContainerId  into i_ContainerId, v_ChildCount, v_ContainerStatus, v_ParentContainerId from Container c where c.ContainerName = i_ContainerName;
	END IF;

	-- based on options param and container info, determine how to load issue details
	if(i_ContainerOption = Single or (i_ContainerOption = Siblings and v_ChildCount = 0 and v_ParentContainerId is null) or (i_ContainerOption = ChildrenOrSingle and v_ChildCount = 0)) THEN
		GetSingle := 1; 
	elsif(i_ContainerOption = Siblings and v_ChildCount = 0 and v_ParentContainerId is not null) THEN
		GetByParent := 1;  
	elsif((i_ContainerOption = Siblings or i_ContainerOption = ChildrenOrSingle) and v_ChildCount > 0) THEN
		GetByChildren := 1;
	END if;

	-- load the issue details
	if(GetSingle = 1 and v_ContainerStatus > 0) THEN
		ChildrenOrSingle := 0;
		Siblings := 1;
		Single := 2;
	
		FOR v_HVIssueHistoryDetailId IN SELECT HVIssueHistoryDetailId FROM HVIssueHistoryDetail WHERE ContainerId = i_ContainerId
		LOOP
		   execute format('insert into %I select $1, $2, * from csiGetDetailsForSingleHVIssue($3)', v_temp) using i_ContainerId,i_ContainerName, v_HVIssueHistoryDetailId;	
		END LOOP;
	
	elsif(GetByParent = 1) then
		for v_ChildContainerId in select c.ContainerId from Container c where c.ParentContainerId = v_ParentContainerId
		loop
			execute format('insert into %I select * from csiGetAllHVIssuesForContainer($1, null, 2)', v_temp) using v_ChildContainerId;
		end loop;
	
	elsif(GetByChildren = 1) then
		for v_ChildContainerId in select c.ContainerId from Container c where c.ParentContainerId = i_ContainerId
		loop 
			execute format('insert into %I select * from csiGetAllHVIssuesForContainer($1, null, 2)', v_temp) using v_ChildContainerId;
		end loop;		
	end if;

	return QUERY 
	execute format('SELECT * FROM %I', v_temp);
end;
$$;

-------------------------------------------------------------------------------
-- Gets high volume issue details for a given container
-- but returns only one row per container/materialListItem(refDes) combination

-- Param 'ContainerOption' determines if issue details are retrieved for a single container, all children or all siblings
--     0 - Children or single:	If has children, get issues for all children, else get issues for specified container only.
--     1 - Siblings:			If has children, get issues for all children, else if has parent, get for all children of parent, else for specified container.
--     2 - Single only:			Get issues only for the specified container
-- High volume component issue is always done for a container with no children, so we never check for issues on a parent container.
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetHVIssuesForContainer')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetHVIssuesForContainer;
 	END IF;
END $$;
CREATE FUNCTION csiGetHVIssuesForContainer
(
	i_ContainerId CHAR(16),
	i_ContainerName varchar(100),
	i_ContainerOption INTEGER
)
RETURNS TABLE 
(
	ContainerId CHAR(16),
	ContainerName VARCHAR(100),
	ReferenceDesignator VARCHAR(100),
	CompName VARCHAR(32),
	ProductName VARCHAR(100),
	ProductRevision VARCHAR(25),
	FromLot VARCHAR(100),
	IssueControl INTEGER,
	ResourceName VARCHAR(100),
	Slot INTEGER,
	SubSlot INTEGER,
	QtyIssued DOUBLE PRECISION,
	WorkflowStepName VARCHAR(100),
	SpecName VARCHAR(100),
	TxnDateGMT TIMESTAMP,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved DOUBLE PRECISION,
	NetQtyIssued DOUBLE PRECISION,
	RemoveDestinationLot VARCHAR(100),
	RemoveDestinationStockPoint VARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT TIMESTAMP
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
LANGUAGE plpgsql
AS $$
DECLARE
	LastConName VARCHAR(100) := '';
	LastRefDes VARCHAR(100) := '';
	LastMatItemId CHAR(16) := '';
	v_results record;
BEGIN
	FOR v_results IN
	(
		SELECT * FROM csiGetAllHVIssuesForContainer(i_ContainerId, i_ContainerName, i_ContainerOption) c
		ORDER BY c.ContainerName, c.MaterialListItemId, c.TxnDateGMT DESC, c.HVIssueHistoryDetailId
	) 
	LOOP
		IF(LastConName <> v_results.ContainerName or LastMatItemId <> v_results.MaterialListItemId) THEN
			ContainerId := v_results.ContainerId;
			ContainerName := v_results.ContainerName;
			ReferenceDesignator := v_results.ReferenceDesignator;
			CompName := v_results.CompName;
			ProductName := v_results.ProductName;
			ProductRevision := v_results.ProductRevision;
			FromLot := v_results.FromLot;
			IssueControl := v_results.IssueControl;
			ResourceName := v_results.ResourceName;
			Slot := v_results.Slot;
			SubSlot := v_results.SubSlot;
			QtyIssued := v_results.QtyIssued;
			WorkflowStepName := v_results.WorkflowStepName;
			SpecName := v_results.SpecName;
			TxnDateGMT := v_results.TxnDateGMT;
			HVResourceSetupHistoryId := v_results.HVResourceSetupHistoryId;
			HVSetupHistoryDetailId := v_results.HVSetupHistoryDetailId;
			ProductId := v_results.ProductId;
			ResourceId := v_results.ResourceId;
			WorkflowStepId := v_results.WorkflowStepId;
			SpecId := v_results.SpecId;
			MaterialListItemId := v_results.MaterialListItemId;
			HVComponentIssueHistoryId := v_results.HVComponentIssueHistoryId;
			HVIssueHistoryDetailId := v_results.HVIssueHistoryDetailId;
			QtyRemoved := v_results.QtyRemoved;
			NetQtyIssued := v_results.NetQtyIssued;
			RemoveDestinationLot := v_results.RemoveDestinationLot;
			RemoveDestinationStockPoint := v_results.RemoveDestinationStockPoint;
			RemoveReasonId := v_results.RemoveReasonId;
			RemoveDifferenceReasonId := v_results.RemoveDifferenceReasonId;
			RemoveSpecId := v_results.RemoveSpecId;
			RemoveStepId := v_results.RemoveStepId;
			RemoveTxnDateGMT := v_results.RemoveTxnDateGMT;
			--RemoveUOMId := v_results.RemoveUOMId;
			--RemoveVendorItemId := v_results.RemoveVendorItemId;
		
			LastConName = v_results.ContainerName;
			LastMatItemId = v_results.MaterialListItemId;
		
        	RETURN NEXT;
		END IF;
    END LOOP;
END;
$$;

-------------------------------------------------------------------------------
-- Get all machine setup details for HV issues to a given container and material list item(Ref Des)
-- Purpose is to get the setup details that could have issued components.
-- Later processing adds history records that indicate components issued to the container from the setup have been removed.
-- This allows for excluding those components from queries to see what was issued, or included in other queries to get what has been removed.
-------------------------------------------------------------------------------
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetHVRemoveSetupDetails')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiGetHVRemoveSetupDetails;
 	END IF;
END $$;
CREATE FUNCTION csiGetHVRemoveSetupDetails
(
	i_ContainerId CHAR(16),
	i_ContainerName VARCHAR(100),
	i_ContainerOption INTEGER,
	i_MaterialListItemId VARCHAR(30)
)
RETURNS TABLE (
	ContainerId CHAR(16),
	ContainerName VARCHAR(100),
	ReferenceDesignator VARCHAR(100),
	CompName VARCHAR(32),
	ProductName VARCHAR(100),
	ProductRevision VARCHAR(25),
	FromLot VARCHAR(100),
	IssueControl INTEGER,
	ResourceName VARCHAR(100),
	Slot INTEGER,
	SubSlot INTEGER,
	QtyIssued DOUBLE PRECISION,
	WorkflowStepName VARCHAR(100),
	SpecName VARCHAR(100),
	TxnDateGMT TIMESTAMP,
	HVResourceSetupHistoryId CHAR(16),
	HVSetupHistoryDetailId CHAR(16),
	ProductId CHAR(16),
	ResourceId CHAR(16),
	WorkflowStepId CHAR(16),
	SpecId CHAR(16),
	MaterialListItemId CHAR(16),
	HVComponentIssueHistoryId CHAR(16),
	HVIssueHistoryDetailId CHAR(16),
	QtyRemoved DOUBLE PRECISION,
	NetQtyIssued DOUBLE PRECISION,
	RemoveDestinationLot VARCHAR(100),
	RemoveDestinationStockPoINTEGER VARCHAR(100),
	RemoveReasonId CHAR(16),
	RemoveDifferenceReasonId CHAR(16),
	RemoveSpecId CHAR(16),
	RemoveStepId CHAR(16),
	RemoveTxnDateGMT TIMESTAMP
	--RemoveUOMId CHAR(16),
	--RemoveVendorItemId CHAR(16)
)
LANGUAGE plpgsql
as $$
DECLARE
BEGIN
	RETURN QUERY
	SELECT * FROM csiGetAllHVIssuesForContainer(i_ContainerId, i_ContainerName, i_ContainerOption) t
	WHERE t.MaterialListItemId = i_MaterialListItemId;
END;
$$;

