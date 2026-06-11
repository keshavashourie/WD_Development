--------------------------------------------------------------------------------
-- SCRIPT: DBCLFFunctions.sql
-- DESCR:  SQL Server stored procedures related to DB CLFs
-- JSON functions that are used in this file are supported only SQL Server 2016 (13.x)
-- and later with compatibility level not lower than 130.
--
-- Copyright Siemens 2023
--------------------------------------------------------------------------------
-- CLFErrorLog CLFTraceLog
--------------------------------------------------------------------------------
DO $$ 
DECLARE
    I INT;
    v_sql VARCHAR(512);
BEGIN
    SELECT COUNT(*)
    INTO I
    FROM pg_tables
    WHERE lower(tablename) = lower('CLFErrorLog');

    IF I = 1 THEN
        v_sql := 'DROP TABLE CLFErrorLog';
        EXECUTE v_sql;
    END IF;

    v_sql := 'CREATE TABLE CLFErrorLog (LogDate TIMESTAMP, LogMessage VARCHAR(4000), CLFPkg TEXT)';
    EXECUTE v_sql;

    SELECT COUNT(*)
    INTO I
    FROM pg_tables
    WHERE lower(tablename) = lower('CLFTraceLog');

    IF I = 1 THEN
        v_sql := 'DROP TABLE CLFTraceLog';
        EXECUTE v_sql;
    END IF;

    v_sql := 'CREATE TABLE CLFTraceLog (LogDate TIMESTAMP, CLFID VARCHAR(255), LogMessage TEXT)';
    EXECUTE v_sql;
END $$;



--------------------------------------------------------------------------------
--clfutilNewInstanceID
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilNewInstanceID(
	pCDOType VARCHAR(255), 
	OUT pInstanceIdStr VARCHAR, 
	pAmt INTEGER = 1)
LANGUAGE plpgsql
AS $$
DECLARE
	vLoopBackNewInstanceID VARCHAR;
	vCDODefId INTEGER;
	vLastInstanceIdStr VARCHAR(16);
BEGIN
    
    SELECT CDODefID INTO vCDODefID
    FROM CDODefinition
    WHERE CDOName = pCDOType;
   
   CALL csiPRDGetNextInstanceId(vCDODefId, pInstanceIdStr);
	
end $$;



--------------------------------------------------------------------------------
--csiUpdateBatchInstanceID
--------------------------------------------------------------------------------
-- Skip



--------------------------------------------------------------------------------
--csiPRDGetBatchNextInstanceIds
--------------------------------------------------------------------------------
CREATE OR REPLACE  PROCEDURE csiPRDGetBatchNextInstanceIds( 
	pCDODefId INTEGER, 
	pAmt INTEGER, 
	OUT pInstanceIdStr VARCHAR(16))
LANGUAGE plpgsql
AS $$
DECLARE
	vErrLocator       NUMERIC;
	vErrMsg           VARCHAR(1024);
	vInstIdNewValues  CHAR(16);
BEGIN
    BEGIN
        --
        -- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
        -- Length should be 10 chars
        --
        vErrLocator := 5;

        CALL csiUpdateInstanceID(0, pCDODefId, pAmt, vInstIdNewValues);
        pInstanceIdStr := vInstIdNewValues;
        
    END;
    --
	EXCEPTION WHEN OTHERS THEN
        vErrMsg := 'csiPRDGetNextInstanceId - ErrLoc: ' || vErrLocator || ' ErrMsg: ' || SQLERRM || ' ErrNum: ' || SQLSTATE;

        -- Assuming you want to print the error message
        RAISE NOTICE '%', vErrMsg;
    --
end $$;



--------------------------------------------------------------------------------
--clfsqlGetQueryText
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfsqlGetQueryText (
	pSQLStatementName VARCHAR(255), 
	OUT pvQueryText VARCHAR)
LANGUAGE plpgsql
AS $$
DECLARE
BEGIN 
   SELECT QueryText INTO pvQueryText
   FROM QueryText
   WHERE QueryDefID = (SELECT QueryDefID
                  FROM QueryDef
                  WHERE UPPER(Name) = UPPER(pSQLStatementName))
   AND DBTYPEID IN (0,1); --DBTYPEID : Generic (0) | SQLServer (1) | Oracle (2) | DB2 (3)

end $$;



--------------------------------------------------------------------------------
--clfGetCLFParameter
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfGetCLFParameter (
	pParameterName VARCHAR(512), 
	OUT pParmValue VARCHAR)
LANGUAGE plpgsql
AS $$
DECLARE
	vSql VARCHAR(1000);
	vParam VARCHAR(100);
BEGIN

	IF(pParameterName LIKE 'CLF::%') THEN
		pParameterName := SUBSTRING(pParameterName FROM 6);
	END IF;

	IF(pParameterName LIKE 'DBCLF::%') THEN
		pParameterName := SUBSTRING(pParameterName FROM 8);
	END IF;

	vSql := 'SELECT Value FROM CLFParameterCache_' || pg_backend_pid() || ' WHERE Name = $1';
    EXECUTE vSql INTO pParmValue USING pParameterName;
	
	IF pParmValue IS NULL THEN
        RAISE NOTICE 'Parameter % not found or null', pParameterName;
    END IF;
	
end $$;



--------------------------------------------------------------------------------
--clfSetCLFParameter
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfSetCLFParameter (
	pParameterName VARCHAR(255), 
	pParameterValue VARCHAR)
LANGUAGE plpgsql
AS $$
DECLARE
	vSql VARCHAR(1000);
	vParam VARCHAR(100);
BEGIN
	
	BEGIN
		pParameterName := REPLACE(pParameterName, 'DBCLF::', '');
	
		EXECUTE 'DELETE FROM CLFParameterCache_' || pg_backend_pid() || ' WHERE Name = $1' USING pParameterName;
        
        EXECUTE 'INSERT INTO CLFParameterCache_' || pg_backend_pid() || ' VALUES ($1, $2)'
        USING pParameterName, pParameterValue;
		
	EXCEPTION 
		WHEN OTHERS THEN
			RAISE NOTICE 'Error when insert data into CLFParameterCache';
	END;
end $$;



--------------------------------------------------------------------------------
--clfutilLogError
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilLogError(
    pCLFPkg VARCHAR,
    pMsg VARCHAR(4000)
)
LANGUAGE plpgsql
AS $$
DECLARE
    vLoopBackLogError VARCHAR;
BEGIN
    vLoopBackLogError := 'CALL HPECSILOOPBACK.' || current_database() || '.' || current_schema() || '.logError($1, $2)';
    
    EXECUTE vLoopBackLogError USING pCLFPkg, pMsg;

end $$;



CREATE OR REPLACE PROCEDURE logError(
    pCLFPkg VARCHAR,
    pMsg VARCHAR(4000)
)
LANGUAGE plpgsql
AS $$
BEGIN
    BEGIN
        -- The ClfErrorLog table's LogMessage column has a limited size (4000). Make sure we don't overrun it.
        INSERT INTO CLFErrorLog(LogDate, LogMessage, CLFPkg)
        VALUES (CURRENT_TIMESTAMP, LEFT(pMsg, 4000), pCLFPkg);
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'HPECSILOOPBACK error when inserting data into CLFErrorLog';
            IF (current_transaction_status() = 'in transaction') THEN
                ROLLBACK;
            END IF;
            RAISE;
    END;
end $$;

--------------------------------------------------------------------------------
--clfutilLogTrace
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilLogTrace(
	pMsg VARCHAR, 
	pTraceLevel INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
   vConfigTraceLevel   VARCHAR(16);
   vIConfigTraceLevel   INTEGER;
   vCLFID              VARCHAR(255);
   vLoopBacklogTrace   VARCHAR;
   vSql                 VARCHAR(1000);
   vParam               VARCHAR(100);
BEGIN

	vSql := 'SELECT Value FROM CLFParameterCache_' || pg_backend_pid() || ' WHERE Name = ''CURRENT_TRACELEVEL''';
    EXECUTE vSql INTO vConfigTraceLevel;
	
	IF(vConfigTraceLevel IS NULL) THEN--if CURRENT_TRACELEVEL not found
	BEGIN
		vIConfigTraceLevel := 0;
		RAISE NOTICE 'CURRENT_TRACELEVEL not found or null';
	END;
	ELSE
		vIConfigTraceLevel := CAST(vConfigTraceLevel AS INT);
	END IF;
	

	vSql := 'SELECT Value FROM CLFParameterCache_' || pg_backend_pid() || ' WHERE Name = ''CURRENT_CLFID''';
    EXECUTE vSql INTO vCLFID;
	
	IF(vCLFID IS NULL) THEN--if CURRENT_CLFID not found
	BEGIN
		vCLFID := '';
		RAISE NOTICE 'CURRENT_CLFID not found or null';
	END;
	END IF;

	vLoopBacklogTrace := 'CALL HPECSILOOPBACK.' || current_database() || '.' || current_schema() || '.logTrace($1, $2)';
	
	IF(vIConfigTraceLevel >= pTraceLevel) THEN
		EXECUTE vLoopBacklogTrace USING pMsg, vCLFID;
	END IF;

end $$;


CREATE OR REPLACE PROCEDURE logTrace(
    pMsg VARCHAR,
    pCLFID VARCHAR(255)
)
LANGUAGE plpgsql
AS $$
BEGIN
    BEGIN
        -- The clfTraceLog table's LogMessage column has a limited size (4000). Make sure we don't overrun it.
        INSERT INTO clfTraceLog(LogDate, CLFID, LogMessage)
        VALUES (CURRENT_TIMESTAMP, pCLFID, LEFT(pMsg, 4000));
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'HPECSILOOPBACK error when inserting data into clfTraceLog';
            IF (current_transaction_status() = 'in transaction') THEN
                ROLLBACK;
            END IF;
            RAISE;
    END;

end $$;



--------------------------------------------------------------------------------
--clfsqlGetParameterValue
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfsqlGetParameterValue(
	pDataObj VARCHAR, 
	pParmName VARCHAR(50), 
	pDefaultValue VARCHAR(512), 
	OUT pReturnParmValue VARCHAR(512))
LANGUAGE plpgsql
AS $$
DECLARE
    vParmArray JSONB;
BEGIN

    pReturnParmValue := pDefaultValue;
    
	vParmArray := pDataObj::JSONB #>> '{Parameters}';

    IF (vParmArray IS NOT NULL) THEN
    BEGIN
		SELECT value->>'Value'
        INTO pReturnParmValue
        FROM jsonb_array_elements(vParmArray)
        WHERE jsonb_extract_path_text(value, 'Name') = pParmName;
    END;
	END IF;

    IF(pParmName != 'Result' AND (pReturnParmValue LIKE 'CLF::%' OR pReturnParmValue LIKE 'DBCLF::%')) THEN
        CALL clfGetCLFParameter(pReturnParmValue, pReturnParmValue);       
	END IF;

end $$;



--------------------------------------------------------------------------------
--clffuncFindClosingBracket
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncFindClosingBracket(
	pQuery VARCHAR, 
	pStartInd INTEGER, 
	pInd INTEGER, 
	OUT pResult INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
    vOpeningBracketCount INTEGER := 0;
    vClosingBracketCount INTEGER := 0;
    vTempStr VARCHAR;
    vTempStr2 VARCHAR;
BEGIN

    pInd := POSITION(')' IN SUBSTRING(pQuery FROM pInd + 1)) + pInd;
    
	vTempStr := SUBSTRING(pQuery FROM pStartInd + 1 FOR pInd - pStartInd);
    
	vTempStr := REPLACE(vTempStr, ' ', '');
	vTempStr2 := REPLACE(vTempStr, '(', '');
	vOpeningBracketCount := LENGTH(vTempStr) - LENGTH(vTempStr2);

	vTempStr2 := REPLACE(vTempStr, ')', '');
    vClosingBracketCount := LENGTH(vTempStr) - LENGTH(vTempStr2);

   IF vOpeningBracketCount <> vClosingBracketCount THEN
        call clffuncFindClosingBracket(pQuery, pStartInd, pInd, pResult);
    ELSE
        pResult := pInd;
    END IF;
	
end $$;



--------------------------------------------------------------------------------
--clffuncFindMainSelectFrom
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncFindMainSelectFrom(
	pQuery VARCHAR, 
	pCurInd INT, 
	OUT pResult INT, 
	pWordToSearch VARCHAR(16) = 'FROM')
LANGUAGE plpgsql
AS $$
DECLARE 
	vIndComma INT;
    vIndSelect INT;
    vIndWith INT;
    vIndClosingBracket INT;
BEGIN
	
	vIndWith := POSITION('WITH' IN upper(pQuery));

    IF (vIndWith = 0) THEN
        pResult := POSITION(upper(pWordToSearch) IN SUBSTRING(upper(pQuery) FROM pCurInd));
    ELSE
        call clffuncFindClosingBracket(pQuery, pCurInd, pCurInd, vIndClosingBracket);

        vIndComma := POSITION(',' IN SUBSTRING(pQuery FROM vIndClosingBracket));
        IF (vIndComma = 0) THEN
            vIndComma := 999999;
        END IF;

        vIndSelect := POSITION('SELECT' IN SUBSTRING(upper(pQuery) FROM vIndClosingBracket));

        IF (vIndSelect < vIndComma) THEN
            pResult := POSITION(upper(pWordToSearch) IN SUBSTRING(upper(pQuery) FROM vIndClosingBracket + 1)) + vIndClosingBracket;           
        ELSE
            vIndClosingBracket := vIndClosingBracket;
            call clffuncFindMainSelectFrom(pQuery, vIndClosingBracket, pResult, pWordToSearch);
        END IF;
    END IF;
	
end $$;




--------------------------------------------------------------------------------
--clffuncisQueryForModifyPrcd
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncisQueryForModifyPrcd(
    query VARCHAR,
    OUT oResult BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    oResult := FALSE;

    IF (POSITION('VALUES' IN upper(query)) > 0 AND
        POSITION('MERGE' IN upper(query)) = 0 AND
        POSITION('SELECT ' IN upper(query)) = 0 AND
        POSITION('FROM' IN upper(query)) = 0 AND
        POSITION('WITH' IN upper(query)) = 0) THEN
        oResult := TRUE;
    END IF;

end $$;



--------------------------------------------------------------------------------
--clfsqlPrepareSQL
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfsqlPrepareSQL(pFuncPkg VARCHAR, OUT spExecute VARCHAR, OUT oResult INT, OUT oResponse VARCHAR(2000))
LANGUAGE plpgsql
AS $$
DECLARE
    vDataObj       		VARCHAR;
    vErrLoc        		INT;  
    vParmObj       		VARCHAR;
    vParmName      		VARCHAR(512);
    vParmValue     		VARCHAR(512);
    vCLFParmValue  		VARCHAR;
    vErrorMsg       	VARCHAR;
    vSQL           		VARCHAR;
    vSQLQueryName  		VARCHAR;
    vTempParmName      	VARCHAR(50);
    vTempParmValue    	VARCHAR;
    vTempParmType       VARCHAR(30);
    vValues             VARCHAR := '';
    vTypes              VARCHAR := '';
    vQueryText          VARCHAR := '';
    vCreateTable        VARCHAR;
    vTempTable          VARCHAR(512);
    vIsFirst            BOOLEAN:= 1;
    vIsList             BOOLEAN:= 0;
    vIsSeq              BOOLEAN:= 0;
    vListInd            INT := 0;
    vListCount          INT := 0;
    vCurentValue        VARCHAR;
    vFstListCount       INT := 0;
    vCurrentSQLPart     VARCHAR;
    vOriginalSQL        VARCHAR;
    vStrOldParmsNames   VARCHAR := '';
    vOldParmsInd        INT;
    vOldParmsCnt        INT;
    vCurentOldParmName  VARCHAR;
    vCurentNewParmName  VARCHAR;
    vIsInsertQuery      BOOLEAN:= 0;
    vParmCnt            INT;
    vParmInfo           VARCHAR;
    vInd                INT := 0;
    vStrParmsNames      VARCHAR := '';
    vCurentParmValue    VARCHAR;    
	vIndx 				INT := 0;
	
	vIsQueryForModify 	BOOLEAN := 0;
	vInd1              	INT := 0;
	vInd2              	INT := 0;
	vIndVal            	INT := 0;
	vListCol           	VARCHAR := '';
	vListVal           	VARCHAR := '';
	vIsParamsHasList   	BOOLEAN := false;
	vParamsCnt         	INT;
	vCurValue          	VARCHAR;
	vEndOfString       	VARCHAR(4);
	vParsedValuesStr   	VARCHAR;
	vAlterSpExecute    	VARCHAR := '';
	vLimit             	INT := 1000;
		
	vName      			VARCHAR := '';
	vCurVal     		VARCHAR;
	
BEGIN
	DROP TABLE IF EXISTS vSQLParms; 
	CREATE TABLE vSQLParms (
        rowId serial PRIMARY KEY,
        ParmName varchar(50),
        ParmValue text,
        ParmType varchar(30),
        IsList boolean
    );

    vDataObj := pFuncPkg;
    vErrLoc := 20;
    CALL clfsqlGetParameterValue(vDataObj, 'SQLStatementName', '', vSQLQueryName);	
    oResponse := vSQLQueryName;
	
	
    -- Loop through the parameters defined for the Query and add them to the parameter array.
    -- Values will be populated later by either CLF-level or Function-level data.
    vErrLoc := 30;
    INSERT INTO vSQLParms
    SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rowId, p.Name AS ParmName, '' AS ParmValue, dt.Name AS ParmType, p.IsList
    FROM QueryParms AS p
    JOIN CppDataTypes dt ON dt.DataTypeId = p.CppDataTypeId
    WHERE QueryDefId = (SELECT QueryDefId FROM QueryDef WHERE Name = vSQLQueryName);

    -- Get the SQL Statement (QueryText) from metadata
    vErrLoc := 40;
    call clfsqlGetQueryText(vSQLQueryName, vSQL);

    IF (vSQL IS NULL) THEN
    BEGIN
        oResponse := 'Query not found: ' || vSQLQueryName;
        oResult := 0;
        RETURN;
    END;
	END IF;
	
    vSQLQueryName := 'SQL (' || vSQLQueryName || '): ' || vSQL;
    vSQL := REPLACE(vSQL, '''', '''''');

	
	-- Loop through CLF-level parameters. For each one that exists in our vSQLParms, set its value.
    vErrLoc := 100;
	vParmObj := vDataObj::JSONB -> 'SQLParameters' -> 0;
	
	WHILE vParmObj IS NOT NULL LOOP
    BEGIN        
		vErrLoc := vErrLoc + 1;            
		vParmObj := vDataObj::JSONB -> 'SQLParameters' -> vIndx;
        IF(vParmObj IS NULL) THEN
            EXIT;
		END IF;
		vIndx := vIndx + 1;
		
		vParmName := vParmObj::JSONB->>'Name';
        vParmValue := vParmObj::JSONB->>'Value';
					
		IF((SELECT COUNT(*) FROM vSQLParms AS parms WHERE parms.ParmName = vParmName) > 0 AND LENGTH(vParmValue) > 0) THEN
		BEGIN
			
			IF(vParmValue LIKE 'CLF::%' OR vParmValue LIKE 'DBCLF::%') THEN
			-- This is an expression, which points to a CLF-level parameter value,
			-- hopefully included in the vCLFParms collection.
			BEGIN
				CALL clfGetCLFParameter(vParmValue, vCLFParmValue);
				IF(vCLFParmValue IS NULL) THEN
				BEGIN
					oResponse := 'Parameter ' || vParmName || ' references CLF variable ' || vParmValue || ' but that variable does not exist.';
					oResult := 0;
					RETURN;
				END;
				END IF;

				--Handler for values like '0000000000000000|0000000000000000|...' and '|||...'
				IF( POSITION('0000000000000000' IN vCLFParmValue) > 0 OR LENGTH(REPLACE(vCLFParmValue, '|', '')) = 0 ) THEN				
					vCLFParmValue := '';
				END IF;
				
				UPDATE vSQLParms
				SET ParmValue = vCLFParmValue				
				WHERE ParmName = vParmName;
			END;
			ELSE
				UPDATE vSQLParms
				SET ParmValue = vParmValue				
				WHERE ParmName = vParmName;
			END IF;
		END;
		END IF;
    END;
	END LOOP;
	

	--values logging
	SELECT COUNT(*) INTO vParmCnt FROM vSQLParms;	
	WHILE(vInd < vParmCnt) LOOP
	BEGIN
		vInd := vInd + 1;
		SELECT ParmName INTO vTempParmName FROM vSQLParms WHERE rowId = vInd;
		SELECT ParmValue INTO vTempParmValue FROM vSQLParms WHERE rowId = vInd;
		SELECT ParmType INTO vTempParmType FROM vSQLParms WHERE rowId = vInd;
		SELECT IsList INTO vIsList FROM vSQLParms WHERE rowId = vInd;

		IF (vIsList = TRUE and POSITION('|' IN vTempParmValue) > 0) THEN
			vParmInfo := UPPER(vTempParmName) || '(' || UPPER(vTempParmType) || ')(List)=' || vTempParmValue;
		ELSE
			vParmInfo := UPPER(vTempParmName) || '(' || UPPER(vTempParmType) || ')=' || vTempParmValue;
		END IF;

		CALL clfutilLogTrace(vParmInfo, 2);

		IF (vIsList = TRUE and POSITION('|' IN vTempParmValue) > 0) THEN
		BEGIN
			vListCount := LENGTH(RTRIM(vTempParmValue)) - LENGTH( REPLACE(RTRIM(vTempParmValue), '|', '') ) + 1;
			vParmInfo := 'List size: ' || vListCount;
			CALL clfutilLogTrace(vParmInfo, 2);
		END;
		END IF;
		
	END;
	END LOOP;
	
	vErrLoc := 500;

	DROP TABLE IF EXISTS listParms; 
    CREATE TABLE listParms (
        rowId serial PRIMARY KEY,
        listParmValue text
    );

	DROP TABLE IF EXISTS listOldParmsNames; 
    CREATE TABLE listOldParmsNames (
        rowId serial PRIMARY KEY,
        OldParmName text
    );

	DROP TABLE IF EXISTS listParsedParms; 
    CREATE TABLE listParsedParms (
        queryId integer,
        ParmName text,
        ParmValue text
    );
		
	IF POSITION('INSERT' IN vSQL) > 0 THEN
        vIsInsertQuery := TRUE;
    END IF;
	
    IF(vIsInsertQuery = true) THEN
        CALL clffuncisQueryForModifyPrcd(vSQL, vIsQueryForModify);
	END IF;

    DROP TABLE IF EXISTS QD; 
	CREATE TABLE QD (
		rowId INT, 
		listCol VARCHAR, 
		listVal VARCHAR
	);
	
	DROP TABLE IF EXISTS listOfValuesParts; 
	CREATE TABLE listOfValuesParts (
		rowId INT, 
		val VARCHAR
	);
	
    IF(vIsQueryForModify = true) THEN
    BEGIN        		
		vInd1 := POSITION('(' IN vSQL) + 1;
		vInd2 := POSITION(')' IN vSQL);
		vListCol := SUBSTRING(vSQL, vInd1, vInd2 - vInd1);      

		vIndVal := POSITION('VALUES' IN vSQL);
		vInd1 := POSITION('(' IN SUBSTRING(vSQL FROM vIndVal)) + vIndVal;		
		call clffuncFindClosingBracket(vSQL, vIndVal, vIndVal, vInd2);
		vListVal := SUBSTRING(vSQL, vInd1, vInd2 - vInd1);
		vListVal := REPLACE(vListVal, '?', '@');
		
        WITH tbl1 (rowId, listCol) AS (
		SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rowId, LTRIM(RTRIM(VALUE)) AS listCol FROM (SELECT UNNEST(STRING_TO_ARRAY(vListCol,',')) as value) temp1
		),
		tbl2 (rowId, listVal) AS (
		SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rowId2, LTRIM(RTRIM(VALUE)) AS listVal FROM (select unnest(STRING_TO_ARRAY(vListVal,',')) as value) temp2
		)        
		INSERT INTO QD SELECT tbl1.rowId, listCol, listVal FROM tbl1 LEFT JOIN tbl2 ON tbl1.rowId = tbl2.rowId;
		
        SELECT COUNT(*) INTO vParamsCnt FROM QD;        
        vInd := 0;
        WHILE(vInd < vParamsCnt) LOOP
        BEGIN
            vInd := vInd + 1;
            SELECT listVal INTO vName FROM QD WHERE rowId = vInd;
			IF POSITION('@' IN vName) > 0 THEN
            BEGIN
                vName := REPLACE(vName, '@', '');				
                SELECT parmValue INTO vCurVal FROM vSQLParms WHERE ParmName = vName;
                UPDATE QD SET listVal = vCurVal WHERE rowId = vInd;
                
				IF POSITION('|' IN vCurVal) > 0 THEN
                BEGIN
                    vIsParamsHasList = true;                    
					SELECT COUNT(*) INTO vListCount FROM unnest(STRING_TO_ARRAY(vCurVal, '|'));
				
                    IF(vFstListCount = 0) THEN
                        vFstListCount = vListCount;
                    ELSE
                        IF(vFstListCount <> vListCount) THEN
                        BEGIN
                            oResponse := 'The number of query parameters does not match';
                            oResult = 0;
                            RETURN;
                        END;
						END IF;
					END IF;

                END;
				END IF;
            END;
			END IF;
        END;
		END LOOP;

        IF(vIsParamsHasList = true) THEN
        BEGIN
            vInd := 0;
            WHILE(vInd < vListCount) LOOP
            BEGIN
                vInd := vInd + 1;
                INSERT INTO listOfValuesParts VALUES (vInd, '(');
            END;
			END LOOP;

            vInd := 0;
            WHILE(vInd < vParamsCnt) LOOP
            BEGIN
                vInd := vInd + 1;
                IF(vInd != vParamsCnt) THEN
                    vEndOfString := ''',';
                ELSE
                    vEndOfString := ''')';
				END IF;

                SELECT listVal INTO vCurValue FROM QD WHERE rowId = vInd;
               
				IF(POSITION('|' IN vCurValue) > 0) THEN
                BEGIN                    
					WITH tbl1 (rowId, splitedValue) AS (
						SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rowId, LTRIM(RTRIM(VALUE)) AS splitedValue FROM (select unnest(STRING_TO_ARRAY (vCurValue,'|')) as value) temp1
					)
					UPDATE listOfValuesParts 
					SET val = val || '''' || splitedValue || vEndOfString
					FROM tbl1
					WHERE listOfValuesParts.rowId = tbl1.rowId;				   
                END;
                ELSEIF (POSITION('(' IN vCurValue) > 0 OR vCurValue = 'NULL') THEN
                BEGIN
                    vEndOfString := SUBSTRING(vEndOfString, 2, 1);
                    update listOfValuesParts SET val = val || vCurValue || vEndOfString;
                END;
                ELSEIF(vCurValue = '') THEN
                BEGIN
                    vEndOfString := SUBSTRING(vEndOfString, 2, 1);
                    update listOfValuesParts SET val = val || 'NULL' || vEndOfString;
                END;
                ELSE
                BEGIN
                    vCurValue := REPLACE(vCurValue, '''', '');
                    UPDATE listOfValuesParts SET val = val || '''' || vCurValue || vEndOfString;
                END;
				END IF;
            END;
			END LOOP;
            

			vAlterSpExecute:= 'call clfSetCLFParameter(''affectedRowCount'', ''0'');' || E'\r\n';
            vInd := 0;
            WHILE(vInd < vListCount) LOOP
            BEGIN								
				SELECT STRING_AGG(val, ',' || E'\r\n') INTO vParsedValuesStr
				FROM (
					SELECT *
					FROM listOfValuesParts
					ORDER BY rowId
					OFFSET (vInd) ROWS
					FETCH NEXT vLimit ROWS ONLY
				) AS tbl2;
				vInd := vInd + vLimit;
				
				vAlterSpExecute := vAlterSpExecute || RTRIM(SUBSTRING(vSQL, 1, vIndVal + LENGTH('VALUES') - 1)) || E'\r\n';
				vAlterSpExecute := vAlterSpExecute || vParsedValuesStr || ';' || E'\r\n';
				
				vAlterSpExecute := vAlterSpExecute || 'UPDATE CLFParameterCache_' || pg_backend_pid();
				vAlterSpExecute := vAlterSpExecute || ' SET Value = Value || FOUND WHERE Name = ''affectedRowCount'';' || E'\r\n';
				
            END;
			END LOOP;			
			
        END;
		END IF;
    END;
	END IF;
	
	
	IF(vIsParamsHasList = true) THEN
    BEGIN
        vInd := 0; 
        SELECT COUNT(*) INTO vParamsCnt FROM vSQLParms; 

        WHILE(vInd < vParamsCnt) LOOP
        BEGIN
            vErrLoc := vErrLoc + 1;
            vInd := vInd + 1; 

            SELECT ParmName INTO vTempParmName FROM vSQLParms WHERE rowId = vInd; 
            SELECT ParmValue INTO vTempParmValue FROM vSQLParms WHERE rowId = vInd; 
            SELECT ParmType INTO vTempParmType FROM vSQLParms WHERE rowId = vInd; 
            SELECT IsList INTO vIsList FROM vSQLParms WHERE rowId = vInd; 

            vSQL = REPLACE(vSQL, '?' || vTempParmName, '@' || vTempParmName);
            IF(vIsFirst != true) THEN
            BEGIN
                vTypes := vTypes || ', ';
                vValues := vValues || ', ';
            END;
			END IF;
        


            --list supporting
			IF (vIsList = TRUE and POSITION('|' IN vTempParmValue) > 0) THEN
            BEGIN
                vListInd := 0;
                DELETE FROM listParms;
                INSERT INTO listParms                
				SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS rowId, value AS listParmValue FROM (select unnest(STRING_TO_ARRAY (vTempParmValue, '|')) as value) temp1;
            
                SELECT COUNT(*) INTO vListCount FROM listParms;
                IF(vFstListCount = 0) THEN
                    vFstListCount := vListCount;
                ELSE
                    IF(vFstListCount <> vListCount) THEN
                    BEGIN
                        oResponse := 'The number of query parameters does not match';
                        oResult := 0;
                        RETURN;
                    END;
					END IF;
				END IF;

                INSERT INTO listParsedParms(queryId, ParmName, ParmValue)
                SELECT rowId, vTempParmName, listParmValue
                FROM listParms;

                WHILE(vListInd < vListCount) LOOP
                BEGIN
                    vListInd := vListInd + 1;
                    IF(vListInd != 1) THEN
                    BEGIN
                        vTypes := vTypes || ', ';
                        vValues := vValues || ', ';
                    END;
					END IF;
                    vTypes := vTypes || '@' || vTempParmName || '_' || CAST(vListInd AS varchar(50)) || ' ' || vTempParmType;
                    SELECT listParmValue INTO vCurentValue FROM listParms WHERE rowId = vListInd;
                    vValues := vValues || '@' || vTempParmName || '_' || CAST(vListInd AS varchar(50)) || ' = ''' || vCurentValue || '''';
                END;
				END LOOP;

                IF(vStrParmsNames = '') THEN
                    vStrParmsNames := vStrParmsNames || '@' || vTempParmName;
                ELSE
                    vStrParmsNames := vStrParmsNames || '|@' || vTempParmName;
				END IF;
            END;
            ELSE
            BEGIN
                vTypes := vTypes || '@' || vTempParmName || ' ' || vTempParmType;
                IF(vTempParmValue = '') THEN
                BEGIN
                    vValues := vValues || '@' || vTempParmName || ' = NULL';
                END;
                ELSE
                BEGIN
                    vValues := vValues || '@' || vTempParmName || ' = ''' || vTempParmValue || '''';
                END;
				END IF;

                INSERT INTO listParsedParms(queryId, ParmName, ParmValue)
                VALUES (NULL, vTempParmName, vTempParmValue);

                IF(vStrParmsNames = '') THEN
                    vStrParmsNames := vStrParmsNames || '@' || vTempParmName;
                ELSE
                    vStrParmsNames := vStrParmsNames || '|@' || vTempParmName;
				END IF;
            END;
			END IF;

            vIsFirst := false;
        END;
		END LOOP;
    
        vErrLoc := 700;
        --convert C++ types to SQL
        vTypes := REPLACE(vTypes, ' OBJECT', ' CHAR(16)'); 
        vTypes := REPLACE(vTypes, ' STRING', ' VARCHAR'); 
        vTypes := REPLACE(vTypes, ' INTEGER', ' INT'); 
        vTypes := REPLACE(vTypes, ' TIMESTAMP', ' TIMESTAMP'); 
        vTypes := REPLACE(vTypes, ' BOOLEAN', ' BOOLEAN');
        vTypes := REPLACE(vTypes, ' FIXED', ' FLOAT');
		
    END;
	END IF;
	
	
	IF(vIsParamsHasList = true) THEN
        spExecute := vAlterSpExecute;
    ELSEIF (vTypes = '') THEN
        spExecute := 'sp_executesql ' || 'N''' || vSQL || '''';	
    ELSE
    BEGIN
        IF(vListCount > 1) THEN
        BEGIN
            vListInd := 0;
            INSERT INTO listOldParmsNames
			SELECT ROW_NUMBER() OVER(ORDER BY value DESC) AS rowId, value AS OldParmName FROM (select unnest(STRING_TO_ARRAY (vStrParmsNames,'|')) as value) temp1;
            SELECT COUNT(*) INTO vOldParmsCnt FROM listOldParmsNames;
            vOriginalSQL := vSQL;

            WHILE(vListInd < vListCount) LOOP
            BEGIN
                vListInd := vListInd + 1;
                vOldParmsInd := 0;
                vCurrentSQLPart := vOriginalSQL || ';';

                WHILE(vOldParmsInd < vOldParmsCnt) LOOP
                BEGIN
                    vOldParmsInd := vOldParmsInd + 1;
                    SELECT OldParmName INTO vCurentOldParmName FROM listOldParmsNames WHERE rowId = vOldParmsInd;
                    vCurentOldParmName = REPLACE(vCurentOldParmName, '@', '');
                    IF((SELECT queryId FROM listParsedParms WHERE ParmName = vCurentOldParmName LIMIT 1) IS NULL) THEN
                        SELECT ParmValue INTO vCurentParmValue FROM listParsedParms WHERE ParmName = vCurentOldParmName;
                    ELSE
                        SELECT ParmValue INTO vCurentParmValue FROM listParsedParms WHERE queryId = vListInd AND ParmName = vCurentOldParmName;
					END IF;

                    vCurentParmValue := '''''' || vCurentParmValue || '''''';
                        
                    vCurrentSQLPart := REPLACE(vCurrentSQLPart, '@' || vCurentOldParmName || ',', vCurentParmValue || ',');
                    vCurrentSQLPart := REPLACE(vCurrentSQLPart, '@' || vCurentOldParmName || ')', vCurentParmValue || ')');
                    vCurrentSQLPart := REPLACE(vCurrentSQLPart, '@' || vCurentOldParmName || ' ', vCurentParmValue || ' ');
                    vCurrentSQLPart := REPLACE(vCurrentSQLPart, '@' || vCurentOldParmName || ';', vCurentParmValue || ';');
                    vCurrentSQLPart := REPLACE(vCurrentSQLPart, '@' || vCurentOldParmName || E'\r\n', vCurentParmValue || E'\r\n');

                END;
				END LOOP;

                vCurrentSQLPart := vCurrentSQLPart || E'\r\n';
                vCurrentSQLPart := vCurrentSQLPart || 'UPDATE CLFParameterCache_' || pg_backend_pid() || ' SET Value = Value || FOUND WHERE Name = ''affectedRowCount'';' || E'\r\n';
                IF(vListInd = 1) THEN
                    vAlterSpExecute := vCurrentSQLPart;
                ELSE
                    vAlterSpExecute := vAlterSpExecute || vCurrentSQLPart;
				END IF;
            END;
			END LOOP;

            vCurrentSQLPart := 'CALL clfSetCLFParameter(''affectedRowCount'', ''0'');' || E'\r\n';
            vAlterSpExecute := vCurrentSQLPart || vAlterSpExecute;
            vAlterSpExecute := 'sp_executesql ' || 'N''' || vAlterSpExecute || '''';
            vSQL := vCurrentSQLPart || vSQL;
            spExecute := vAlterSpExecute;
        END;
        ELSE
            spExecute := 'sp_executesql ' || 'N''' || vSQL || '''' || ', N''' || vTypes || ''', ' || vValues;
		END IF;
    END;
	END IF;

    oResult := 1;
end $$;


--------------------------------------------------------------------------------
--clfutilBuildMessageFromLabel
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilBuildMessageFromLabel(
	pLabelName VARCHAR(2000), 
	pTableName TEXT, 
	OUT pLblText VARCHAR(2000))
LANGUAGE plpgsql
AS $$
DECLARE
    vLabelID INTEGER;
    vDefaultLblText  VARCHAR(2000);
    vPrimaryDictId   VARCHAR(16) := NULL;
    vSecondaryDictId VARCHAR(16) := NULL;
    vInd INTEGER := 1;
    vCharInd INTEGER := 0;
    vColumnCount INTEGER;
    vCurColumn VARCHAR(256);
    vSp_Execute VARCHAR(2000);
    vValue VARCHAR(256) := '';
    vTypes VARCHAR(256) := '';
BEGIN
	SELECT LabelID INTO vLabelID
	FROM Labels
	WHERE Name = pLabelName;

	CALL clfGetCLFParameter ('__PrimaryDictionary', vPrimaryDictId);
	CALL clfGetCLFParameter ('__SecondaryDictionary', vSecondaryDictId);

   -- The order of dictionaries is:
   --   1) Primary Dictionary
   --   2) Secondary Dictionary
   --   3) Default Label

   SELECT LabelValue INTO vDefaultLblText
   FROM Labels
   WHERE LabelId = vLabelID;

   IF (vPrimaryDictId IS NOT NULL AND vPrimaryDictId <> '') THEN
	   BEGIN
			SELECT LabelValue INTO pLblText
			FROM DictionaryLabel
			WHERE DictionaryId = vPrimaryDictId
			AND LabelId = vLabelID;
			IF NOT FOUND THEN
				pLblText := NULL;
			END IF;
	   END;
   ELSEIF (vSecondaryDictId IS NOT NULL AND vSecondaryDictId <> '' AND pLblText IS NULL) THEN
	   BEGIN
			SELECT LabelValue INTO pLblText
			FROM DictionaryLabel
			WHERE DictionaryId = vSecondaryDictId
			AND LabelId = vLabelID;
			IF NOT FOUND THEN
				pLblText := NULL;
			END IF;
	   END;
   ELSEIF (pLblText IS NULL) THEN
	   pLblText := vDefaultLblText;
	
	   DROP TABLE IF EXISTS columns_temp;
	   CREATE TEMPORARY TABLE columns_temp (id INTEGER, columnName TEXT);
	 
	   INSERT INTO columns_temp (id, columnName)
	   SELECT ROW_NUMBER() OVER(ORDER BY LENGTH(column_name) DESC), column_name
	   FROM information_schema.columns
	   where table_name = lower('test');
	
	   GET DIAGNOSTICS vColumnCount = ROW_COUNT;

	   WHILE vInd <= vColumnCount LOOP
			SELECT columnName INTO vCurColumn
			FROM columns_temp
			WHERE id = vInd;

			vSp_Execute := 'SELECT ' || vCurColumn || ' FROM ' + @pTableName;
			EXECUTE vSp_Execute INTO vValue;
		
			vCharInd := POSITION(lower('#ErrorMsg.' || vCurColumn) IN lower(pLblText));
			IF vCharInd > 0 THEN
				pLblText := OVERLAY(pLblText PLACING vValue FROM vCharInd FOR LENGTH('#ErrorMsg.' || vCurColumn));
			END IF;
	   END LOOP;
	   
	END IF;
EXCEPTION
	WHEN OTHERS THEN
	RAISE NOTICE 'Error in CLF Function clfutilBuildMessageFromLabel';
END $$;

--------------------------------------------------------------------------------
--clffuncExecuteSingleSQL
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncExecuteSingleSQL (
	pFuncPkg TEXT,
	OUT pResult INTEGER,
	OUT pResponse VARCHAR(2000))
LANGUAGE plpgsql
AS $$
DECLARE
   vDataObj       TEXT;
   vSQLQueryName  VARCHAR(255);
   vErrorMsg      TEXT;
   vErrLoc        INTEGER;
   vSpExecute     TEXT;
   vFuncName      VARCHAR(80) := '';
   vRowsProcessed INTEGER;
--   vAffectedRowsCount INTEGER := '0';
--   vSesId VARCHAR(32);
BEGIN
   --CALL clfutilLogTrace ('BEGIN_ExecuteSingleSQL', 0); -- START LOGGING !!!!!!!
   -- TODO
   -- * Error handling
   -- * Check JSON string to see if its well-formed
   -- * Return success/failure and possibly a JSON document
--	BEGIN 
	vErrLoc := 1;
	pResponse := '';
	vFuncName := (pFuncPkg::JSONB)->>'Name';

	IF (vFuncName IS NULL) THEN
		BEGIN 
		-- Something must be wrong with the JSON document
			SET pResponse = 'Invalid JSON document.';
			SET pResult = 0;
			RETURN;
		END;
	END IF;	

	vErrLoc := 10;
	vDataObj := pFuncPkg;
	
	vErrLoc := 800;
	CALL clfutilLogTrace ('BEGIN_Prepare_ExecuteSingleSQL', 0);
	CALL clfsqlPrepareSQL (vDataObj, vSpExecute, pResult, pResponse);
	CALL clfutilLogTrace (vSpExecute, 0);
	CALL clfutilLogTrace ('END_Prepare_ExecuteSingleSQL', 0);
	IF(pResult = 0) THEN
		RAISE EXCEPTION '%', pResponse;
	END IF;
	CALL clfutilLogTrace ('BEGIN_Execute_ExecuteSingleSQL', 0);
	EXECUTE vSpExecute;
	CALL clfutilLogTrace ('END_Execute_ExecuteSingleSQL', 0);

	-- vSesId = CAST (pg_backend_pid() AS NVARCHAR(32));
	-- CALL clfutilLogTrace ('BEGIN_Prepare_ExecuteSingleSQL', 0);
	-- exec dbo.PrepareSql @vDataObj, @SesId, @spExecute OUTPUT, @pResult OUTPUT;
	-- CALL clfutilLogTrace (vSpExecute, 0);
	-- CALL clfutilLogTrace ('END_Prepare_ExecuteSingleSQL', 0);
	-- CALL clfutilLogTrace ('BEGIN_Execute_ExecuteSingleSQL', 0);
	-- exec dbo.ExecuteSql @spExecute, @pResult OUTPUT;
	-- CALL clfutilLogTrace ('END_Execute_ExecuteSingleSQL', 0);

	GET DIAGNOSTICS vRowsProcessed = ROW_COUNT;
--  	Don't validate RowsProcessed here. If the user wants to validate, they can do it explicitly.
--		CALL clfGetCLFParameter ('affectedRowCount', vAffectedRowsCount);
--		IF (vAffectedRowsCount > 0) THEN
--			BEGIN
--				vRowsProcessed := vAffectedRowsCount;
--				EXECUTE clfSetCLFParameter ('affectedRowCount', 0);
--			END;
--		END IF; 
	pResponse := 'Rows processed: ' + CAST(vRowsProcessed AS varchar);
	CALL clfutilLogTrace (pResponse, 1);
	vErrLoc := 810;
	pResult := 1;
EXCEPTION WHEN OTHERS THEN
	pResponse := 'Error in DBCLF Function ' || vFuncName || ': ' || SQLERRM;
	pResult := 0;
--	END; 
--EXEC clfutilLogTrace 'END_ExecuteSingleSQL', 0; -- END LOGGING !!!!!!!
END $$;

--------------------------------------------------------------------------------
--clfExecute
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfExecute(
	pTxnId VARCHAR(30),
	pCLFPkg TEXT,
	oResult OUT INTEGER,
	oResponse OUT TEXT
)
	/*
	PROCEDURE: clfExecute
	DESCR: The main logic for parsing and executing DB CLF packages
	PARAMS: 
		pCLFPkg: A JSON document representing the CLF to be executed
		oResult: (out) 1=SUCCESS, 0=FAILURE
		oResponse: (out) Returned JSON document, but for now, just an error message.
   */
LANGUAGE plpgsql
AS $$
DECLARE
	vTxnId VARCHAR(30);
	vCLFName VARCHAR(128);
	vFuncJSON TEXT;
	vCLFParamPkg TEXT;
	vFuncArray TEXT;
	vFuncResponse VARCHAR(2000);
	vFuncName VARCHAR(80);
	vSQL VARCHAR(512);
	vFuncResult INTEGER;
	vErrLoc INTEGER;
	vTraceLevel INTEGER; -- Used to identify location of errors  
	sql VARCHAR(1000);
	param VARCHAR(100);
	indx INTEGER;
	
	-- NOTES
    --    * oResponse must be limited in size because the calling application has to pre-allocate
    --      memory for the string. 
    --      Currently: 8000
    --    * For now, oResponse will simply be an error message (not JSON) 
	
BEGIN
	vErrLoc := 1;
	oResponse := '';
	
	IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'clfparametercache_' || pg_backend_pid()::TEXT) THEN
		sql := 'CREATE TEMP TABLE CLFParameterCache_' || pg_backend_pid()::TEXT || '(Name VARCHAR(50), Value TEXT)';
		EXECUTE sql;
	END IF;
	
	IF NOT f_is_json(pCLFPkg) THEN
		-- Something must be wrong with the JSON document
		oResponse := 'Invalid JSON document.';
		oResult := 0;
		CALL clfutilLogError(pCLFPkg, oResponse);
		CALL clfutilLogTrace(oResponse, 1);
		CALL clfutilLogTrace('END', 1);
		RETURN;
	END IF;
	
	sql := 'DELETE FROM CLFParameterCache_' || pg_backend_pid()::TEXT || ' WHERE NAME = ''TXNID''';
	EXECUTE sql;
	sql := 'INSERT INTO CLFParameterCache_' || pg_backend_pid()::TEXT || ' VALUES (''TXNID'', $1)';
    EXECUTE sql USING pTxnId;
	
	-- Iterate through the CLF-level parameters to get certain values
	vErrLoc := 2;
	vCLFName := 'Unknown CLF';
	vCLFParamPkg := (pCLFPkg->>'CLFParameters')::JSONB;
	
	sql := 'DELETE FROM CLFParameterCache_' || pg_backend_pid()::TEXT || ' WHERE NAME IN (SELECT ''' || vCLFParamPkg || '''::jsonb->>''Name'')';
	EXECUTE sql;
	sql := 'INSERT INTO CLFParameterCache_' || pg_backend_pid()::TEXT || 
            ' VALUES ( (''' || vCLFParamPkg || '''::jsonb->>''CLFParameters'')::JSONB->>''Name'', (''' || vCLFParamPkg || '''::jsonb->>''CLFParameters'')::JSONB->>''Value'')';
	EXECUTE sql;
	
	SELECT tmp."Value" INTO vCLFName
    FROM jsonb_to_record(vCLFParamPkg::JSONB->'CLFParameters') AS tmp("Name" text, "Value" text)
    WHERE tmp."Name" = '__MethodName';
	
	BEGIN
		SELECT CAST(TValue AS INT) INTO vTraceLevel
		FROM InsiteSiteInfo
		WHERE UPPER(TName) = 'DBCLFTRACELEVEL';
	EXCEPTION
		WHEN NO_DATA_FOUND THEN
			vTraceLevel := 0;
	END;
	
	sql := 'DELETE FROM CLFParameterCache_' || pg_backend_pid()::TEXT || ' WHERE NAME = ''CURRENT_CLFID''';
	EXECUTE sql;
	sql := 'INSERT INTO CLFParameterCache_' || pg_backend_pid()::TEXT || ' VALUES (''CURRENT_CLFID'', $1)';
	EXECUTE sql USING pTxnId;
	
	sql := 'DELETE FROM CLFParameterCache_' || pg_backend_pid()::TEXT || ' WHERE NAME=''CURRENT_TRACELEVEL''';
	EXECUTE sql;
	sql := 'INSERT INTO CLFParameterCache_' || pg_backend_pid()::TEXT || ' VALUES(''CURRENT_TRACELEVEL'', $1)';
	EXECUTE sql USING vTraceLevel;
	
	CALL clfutilLogTrace('BEGIN', 1);
	CALL clfutilLogTrace(pCLFPkg, 2);
	
	vErrLoc := 5;
	vFuncArray := (pCLFPkg->>'Functions')::JSONB;
	IF (vFuncArray IS NULL) THEN
		-- Something must be wrong with the JSON document
		oResponse := 'Error in CLF ' || vCLFName || ': Invalid JSON document.';
		oResult := 0;
		CALL clfutilLogError(pCLFPkg, oResponse);
		CALL clfutilLogTrace(oResponse, 1);
		CALL clfutilLogTrace('END', 1);
		RETURN;
	END IF;
	
	-- Iterate through all the Functions within the document. Each one should identify
	-- its corresponding stored procedure (eg ExecuteSingleSQL, etc.) along with 
	-- Parameter values to be passed to that procedure.
	--
	-- All of the "child" procedures MUST have the same signature, namely:
	-- PROCEDURE clffunc<FunctionName>(pJSONPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
	-- This way, we can add Functions just by adding them to Designer (metadata) and creating a corresponding stored procedure.
	
	vErrLoc := 100;
	vFuncJSON := (pCLFPkg::JSONB->'Functions'->0);
	
	indx := 0;
	
	BEGIN
		WHILE vFuncJSON IS NOT NULL LOOP
			vErrLoc := vErrLoc + 1;
			vFuncJSON := (pCLFPkg::JSONB->'Functions'->indx);
			IF vFuncJSON IS NULL THEN
				EXIT;
			END IF;
			indx := indx + 1;
			vFuncName := (vFuncJSON::JSONB->>'Name')::JSONB;

			-- Map from vFuncName to the actual stored procedure name.
			vSQL := 'CALL clffunc' || vFuncName || '($1::JSONB, $2, $3)';
			EXECUTE vSQL USING vFuncJSON, vFuncResult, vFuncResponse;

			IF vFuncResult = 0 THEN
				oResponse := oResponse || vFuncResponse;
				oResult := 0;
				CALL clfutilLogError(pCLFPkg, oResponse);
				CALL clfutilLogTrace(oResponse, 1);
				CALL clfutilLogTrace('END', 1);
				RETURN;
			END IF;
		END LOOP;

		-- TODO: Drop ##tempTable
		oResult := 1;
		oResponse := '';
		CALL clfutilLogTrace('END', 1);
	EXCEPTION
		WHEN OTHERS THEN
			oResponse := 'Error in DBCLF ' || vCLFName || ': ErrLoc: ' || vErrLoc::TEXT || ' Function: ' || vFuncName || ': ' || SQLERRM;
			oResult := 0;
			CALL clfutilLogError(pCLFPkg, oResponse);
			CALL clfutilLogTrace(oResponse, 1);
			CALL clfutilLogTrace('END', 1);
	END;
END;
$$;

-- helper function to validate JSON
CREATE OR REPLACE FUNCTION f_is_json(_txt text)
  RETURNS bool
  LANGUAGE plpgsql IMMUTABLE STRICT AS
$func$
BEGIN
   RETURN _txt::json IS NOT NULL;
EXCEPTION
   WHEN SQLSTATE '22P02' THEN  -- invalid_text_representation
      RETURN false;
END
$func$;

--------------------------------------------------------------------------------
--clffuncDBGenerateInstanceIDs
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncDBGenerateInstanceIDs (
	pFuncPkg TEXT, 
	OUT pResult INTEGER, 
	OUT pResponse VARCHAR(2000)) 
LANGUAGE plpgsql
AS $$
DECLARE
   vDataObj        	TEXT;
   vFuncName       	VARCHAR(255);
   vParmArray      	TEXT;
   vParmObj        	TEXT;
   vParmName       	VARCHAR(512);
   vParmValue      	TEXT;
   vCDOTypeName    	VARCHAR(255);
   vResultVar      	VARCHAR(255);
   vErrorMsg       	TEXT;
   vIID            	VARCHAR(2000);
   vNumInstanceIDs 	VARCHAR(512);
   vINumInstanceIDs INTEGER;
   vIsFirst        	BOOLEAN;
   vErrLoc         	INTEGER;
   vIsCLR		   	BOOLEAN := 1;
   vSpId		   	TEXT;
BEGIN
--	IF(vIsCLR == 1) THEN
--		Begin
--			CALL clfutilLogTrace ('Start Gen_ID CLR', 0);
--			vSpId = CAST(pg_backend_pid() AS VARCHAR);
--			CALL dbo.DBGenerateInstanceIDs (pFuncPkg, vSpId, OUT pResult);
--			CALL clfutilLogTrace ('End Gen_ID CLR', 0);
--		End
--	ELSE
--	Begin
	CALL clfutilLogTrace ('Start Gen_ID SP', 0);

	vErrLoc := 1;
	pResponse := '';

	vDataObj := pFuncPkg;
	vFuncName := (vDataObj::JSONB)->>'Name';

	IF (vFuncName IS NULL) THEN
		BEGIN
			-- Something must be wrong with the JSON document
			pResponse := 'Invalid JSON document.';
			pResult := 0;
			RETURN;
		END;
	END IF;

	vErrLoc := 11;
	CALL clfsqlGetParameterValue (vDataObj, 'CDOTypeName', '', vCDOTypeName);
	vErrLoc := 12;
	CALL clfsqlGetParameterValue (vDataObj, 'NumInstanceIDs', '', vNumInstanceIDs);
	IF(LENGTH(vNumInstanceIDs) > 0) THEN
		vINumInstanceIDs := CAST(vNumInstanceIDs AS INT);
	END IF;
	vErrLoc := 13;
	CALL clfsqlGetParameterValue (vDataObj, 'Result', '', vResultVar);

	vErrLoc := 15;
	BEGIN
		CALL clfutilNewInstanceID (vCDOTypeName, vIID, vINumInstanceIDs);
		CALL clfSetCLFParameter (vResultVar, vIID);
		pResult := 1;
	EXCEPTION WHEN OTHERS THEN
		pResponse := 'Error in CLF Function ' || vFuncName || ', ErrLoc: '|| CAST(vErrLoc AS VARCHAR) || ': ' || SQLERRM;
		pResult := 0;
	END;
	
	CALL clfutilLogTrace ('End Gen_ID SP', 0);
--	End;
END $$;

--------------------------------------------------------------------------------
--clffuncDBGenerateSequence
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncDBGenerateSequence (
	pFuncPkg TEXT, 
	OUT pResult INTEGER, 
	OUT pResponse VARCHAR(2000)) 
LANGUAGE plpgsql
AS $$
DECLARE
    vDataObj       TEXT;
    vFuncName      VARCHAR(255);
    vParmValue     TEXT;
    vErrorMsg      TEXT;
    vResultVar     VARCHAR(255);
    vStartNum      VARCHAR(512);
    vEndNum        VARCHAR(512);
    vStepSize      VARCHAR(512);
    vDelim         VARCHAR(1);
    vIStartNum     INTEGER;
    vIEndNum       INTEGER;
    vIStepSize     INTEGER;
    vINum          INTEGER;
    vErrLoc        INTEGER;
    vIsFirst       BOOLEAN;
BEGIN
    vErrLoc := 1;
    pResponse := '';
    vDelim := '|';

    vDataObj := pFuncPkg;
    vFuncName := (vDataObj::JSONB)->>'Name';

    IF (vFuncName IS NULL) THEN
		BEGIN
			-- Something must be wrong with the JSON document
			pResponse := 'Invalid JSON document.';
			pResult := 0;
			RETURN;
		END;
    END IF;

    vErrLoc := 10;

    CALL clfsqlGetParameterValue (vDataObj, 'StartNumber', '', vStartNum);
    IF(LENGTH(vStartNum) > 0) THEN
        vIStartNum := CAST(vStartNum AS INTEGER);
    END IF;

    CALL clfsqlGetParameterValue (vDataObj, 'EndNumber', '', vEndNum);
    IF(LENGTH(vStartNum) > 0) THEN
        vIEndNum := CAST(vEndNum AS INTEGER);
    END IF;

    CALL clfsqlGetParameterValue (vDataObj, 'StepSize', '1', vStepSize);
    IF(LENGTH(vStepSize) > 0) THEN
        vIStepSize := CAST(vStepSize AS INTEGER);
    END IF;

    CALL clfsqlGetParameterValue (vDataObj, 'Result', '', vResultVar);

    vErrLoc := 15;

    vINum := vIStartNum;
    vParmValue := '';
    BEGIN 
        WHILE vINum <= vIEndNum LOOP
			vErrLoc := vErrLoc + 1;
			IF(vIsFirst != 1) THEN
				vParmValue := vParmValue + vDelim;
			END IF;
			vParmValue := vParmValue + CAST(vINum AS VARCHAR);
			vINum := vINum + vIStepSize;
			vIsFirst := 0;
        END LOOP;
		
        CALL clfSetCLFParameter (vResultVar, vParmValue);
        pResult := 1;
	EXCEPTION WHEN OTHERS THEN
        pResponse := 'Error in CLF Function ' || vFuncName || ', ErrLoc: '|| CAST(vErrLoc AS VARCHAR) || ': ' || SQLERRM;
        pResult := 0;
    END;
END $$;


--------------------------------------------------------------------------------
--clffuncDBValidateNotExists
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncDBValidateNotExists(
	pFuncPkg TEXT, 
	OUT pResult INTEGER, 
	OUT pResponse VARCHAR(2000))
LANGUAGE plpgsql
AS $$
DECLARE
    -- This validation parses and executes the given query. If any rows are returned, the
    -- validation fails. No data is actually fetched; the validation simply looks at the
    -- number of rows 
    --
    vDataObj           TEXT;
    vFuncName          VARCHAR(255);
    vSQLQueryName      VARCHAR(255);
    vRowsProcessed     INTEGER;
    vErrLoc            INTEGER;
    vReturnErrorMsg    VARCHAR(512);
    vErrorMsg          VARCHAR(2000);
    vSpExecute         TEXT;
    vSpExecuteInTable  TEXT := '';
    vSpExecuteDropTbl  VARCHAR(255);
    vLabelName         VARCHAR(2000);
    vMyid              UUID;
    vResTableName      TEXT;
    vCharInd           INTEGER := 0;
    vAsInd             INTEGER;
    vWithoutSpaces     TEXT;
    vWithoutPatern     TEXT;
    vQueriesCount      INTEGER;
    vWithCount         INTEGER;
    vWithInd           INTEGER := 0;
    vInd               INTEGER := 0;
    vIndFrom           INTEGER := 0;
    vAffectedRowsCount INTEGER := 0;
	vRow           	   TEXT;
	vCount             TEXT;

BEGIN
	CALL clfutilLogTrace ('BEGIN_DBValidateNotExists', 0); -- START LOGGING !!!!!!!
    vErrLoc := 1;
    pResponse := '';
    vDataObj := pFuncPkg;
    vFuncName := (vDataObj::JSONB)->>'Name';

    IF (vFuncName IS NULL) THEN
		BEGIN 
			-- Something must be wrong with the JSON document
			pResponse := 'Invalid JSON document ' || vFuncName || '.';
			pResult := 0;
			RETURN;
		END;
	END IF;
    vErrLoc := 10;
    CALL clfsqlGetParameterValue (vDataObj, 'ErrorMessage', 'Validation failed', vReturnErrorMsg);
    vErrLoc := 800;
    BEGIN
        CALL clfsqlPrepareSQL (vDataObj, vSpExecute, pResult, pResponse);

		--declare @SesId nvarchar(32), @sql NVARCHAR(1000);
		--SesId := CAST (@@spid AS NVARCHAR(32));
		--exec dbo.PrepareSql @vDataObj, @SesId, @vSpExecute OUTPUT, @oResult OUTPUT;
		CALL clfutilLogTrace (vSpExecute, 0);
		pResponse := 'Response';


        vSQLQueryName := pResponse;
        vWithoutSpaces := RTRIM(vSpExecute);
        vWithoutPatern := REPLACE(vWithoutSpaces, '##CLFParameterCache', '');
        vQueriesCount := ( LENGTH(vWithoutSpaces) - LENGTH(vWithoutPatern) ) / LENGTH('##CLFParameterCache');

        IF(vQueriesCount = 0) THEN
            vQueriesCount := 1;
		END IF;
        -- Find the 'WITH' section
        vWithoutPatern := REPLACE(vWithoutSpaces, 'WITH ', '');
        vWithCount := ( LENGTH(vWithoutSpaces) - LENGTH(vWithoutPatern) ) / LENGTH('WITH ');

        CALL clfsqlGetParameterValue (vDataObj, 'ErrorID', 'Validation failed', vLabelName);
        -- Handling Variables Inside a Label Text
        IF (vLabelName IS NOT NULL AND vLabelName <> '') THEN
        BEGIN
            vMyid := gen_random_uuid();
            vResTableName := '##TempResTable_' + CAST(vMyid AS varchar(255));

            vSpExecuteInTable := vSpExecute;

            IF(vWithCount == vQueriesCount OR vWithCount == 0) THEN
            BEGIN
                WHILE vInd < vQueriesCount LOOP
                BEGIN
                    IF(vInd = 0) THEN
                    BEGIN
                        IF(vWithCount != 0) THEN
                        BEGIN
                            vWithInd := POSITION('WITH ' IN SUBSTRING(vSpExecuteInTable FROM vWithInd)) + 1;
                            vAsInd := POSITION('AS' IN SUBSTRING(vSpExecuteInTable FROM vWithInd)) + 1;
                
                            CALL clffuncFindMainSelectFrom (vSpExecuteInTable, vAsInd, vIndFrom);
                            vWithInd := vWithInd + 1;
                        END;
                        ELSE
                            CALL clffuncFindMainSelectFrom (vSpExecuteInTable, vIndFrom, vIndFrom);
						END IF;
                        vSpExecuteInTable := OVERLAY(vSpExecuteInTable PLACING 'INTO [' || vResTableName || '] ' FROM vIndFrom FOR 0);

                        vIndFrom := vIndFrom || LENGTH('INTO [' || vResTableName || '] ') || LENGTH('FROM') + 1;
                    END;
                    ELSE
                    BEGIN
                        IF(vWithCount != 0) THEN
                        BEGIN
                        -- Find the main SELECT After 'WITH'
                            vWithInd:= POSITION('WITH ' IN SUBSTRING(vSpExecuteInTable FROM vWithInd)) + 1;
                            vAsInd := POSITION('AS' IN SUBSTRING(vSpExecuteInTable FROM vWithInd)) + 1;

                            CALL clffuncFindMainSelectFrom (vSpExecuteInTable, vAsInd, vIndFrom, 'SELECT');
                            vWithInd := vWithInd + 1;
                        END;
                        ELSE
                            CALL clffuncFindMainSelectFrom (vSpExecuteInTable, vIndFrom, vIndFrom, 'SELECT');
						END IF;
                    
                        vSpExecuteInTable := OVERLAY(vSpExecuteInTable PLACING 'INSERT INTO [' || vResTableName || '] ' FROM vIndFrom FOR 0);

                        vIndFrom := vIndFrom || LENGTH('INSERT INTO [' || vResTableName || '] ') || LEN('FROM') + 1;
                    END;
					END IF;
                    
                    vInd := vInd + 1;
                END;
                END LOOP;
            END;
            END IF;

			CALL clfutilLogTrace (vSpExecuteInTable, 0);
            EXECUTE vSpExecuteInTable;
			GET DIAGNOSTICS vRowsProcessed = ROW_COUNT;
			--exec dbo.ExecuteSql @spExecuteInTable, @oResult OUTPUT;
            --SET @vRowsProcessed := @oResult;


			---------------------------------------
			vRow := CAST (vRowsProcessed AS TEXT);
			vCount := CAST (vQueriesCount AS TEXT);
			--EXEC clfutilLogTrace @vRow, 0;
			--EXEC clfutilLogTrace @qCount, 0;
			---------------------------------------


            IF(vQueriesCount > 1) THEN
            BEGIN
                CALL clfGetCLFParameter ('affectedRowCount', vAffectedRowsCount);
                -- Find the matches
                vRowsProcessed := vAffectedRowsCount;
                CALL clfSetCLFParameter ('affectedRowCount', 0);
            END;
			END IF;

            CALL clfutilBuildMessageFromLabel (vLabelName, vResTableName, vErrorMsg);
            
            vSpExecuteDropTbl := 'IF EXISTS ( SELECT 1 FROM information_schema.tables WHERE lower(table_name) = lower('|| vResTableName ||') ) THEN
				DROP TABLE IF EXISTS '|| vResTableName ||';
			END IF;';
            EXECUTE vSpExecuteDropTbl;

            vReturnErrorMsg := vErrorMsg;
        END;
        ELSE
        BEGIN
			CALL clfutilLogTrace (vSpExecute, 0);
            EXECUTE vSpExecute;
			GET DIAGNOSTICS vRowsProcessed = ROW_COUNT;
			--exec dbo.ExecuteSql @spExecute, @oResult OUTPUT;
            --SET @vRowsProcessed = @oResult;

			---
			vRow := CAST (vRowsProcessed AS TEXT);
			vCount := CAST (vQueriesCount AS TEXT);
			--EXEC clfutilLogTrace @vRow, 0;
			--EXEC clfutilLogTrace @qCount, 0;
			---

            IF(vQueriesCount > 1) THEN
            BEGIN
                -- Find the matches
                CALL clfGetCLFParameter ('affectedRowCount', vAffectedRowsCount);
                vRowsProcessed := vAffectedRowsCount;
            END;
			END IF;
        END;
		END IF;

        pResponse := 'Rows processed: ' || CAST(vRowsProcessed AS VARCHAR);
        CALL clfutilLogTrace (pResponse, 1);
        IF(vRowsProcessed != 0) THEN
        BEGIN
            pResponse := vReturnErrorMsg;
            pResult := 0;
            RETURN;
        END;
		END IF;
        pResult := 1;
	EXCEPTION WHEN OTHERS THEN
        pResponse := 'Error in CLF Function ' || vFuncName || ': PSQLStatementName: ' || vSQLQueryName || ', ErrLoc: ' || CAST(vErrLoc AS VARCHAR) || ': ' || SQLERRM;
        pResult := 0;
    END;
	
	CALL clfutilLogTrace ('END_DBValidateNotExists', 0); -- END LOGGING !!!!!!!
END $$;

--------------------------------------------------------------------------------
--clffuncExecuteAndFetchSQL
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clffuncExecuteAndFetchSQL (pFuncPkg TEXT, OUT pResult INTEGER, OUT pResponse VARCHAR(2000))
LANGUAGE plpgsql
AS $$
DECLARE
    vDataObj          TEXT;
    vFuncName         VARCHAR(255);
    vResultVar        VARCHAR(512);
    vSpExecute        TEXT;
    vFirstArgument    TEXT;
    vSelectPos        INTEGER;
    vFromPos          INTEGER;
    vRowsProcessed    INTEGER := 0;
    vErrLoc           INTEGER;
    vCurRes           TEXT;
    vRes              TEXT := '';
	result_cursor	  REFCURSOR;
BEGIN 
	--CALL clfutilLogTrace ('BEGIN_ExecuteAndFetchSQL', 0); -- START LOGGING !!!!!!!
    -- This function executes the given SQL statement and fetches the results from the
    -- first column in the resultset to build a DBCLF:: parameter.
	vErrLoc := 1;
	pResponse := '';
	vFuncName := (pFuncPkg::JSONB)->>'Name';

	IF (vFuncName IS NULL) THEN
	BEGIN 
	-- Something must be wrong with the JSON document
		pResponse := 'Invalid JSON document.';
		pResult := 0;
		RETURN;
	END;
	END IF;

	vErrLoc := 10;
	vDataObj := pFuncPkg;
	CALL clfsqlGetParameterValue (vDataObj, 'Result', '', vResultVar);

	BEGIN
		vErrLoc := 100;

		CALL clfsqlPrepareSQL (vDataObj, vSpExecute, pResult, pResponse);
		IF(pResult = 0) THEN
			RAISE EXCEPTION '%', pResponse;
		END IF;

		--declare @SesId nvarchar(32), @sql NVARCHAR(1000);
		--set @SesId = CAST (@@spid AS NVARCHAR(32));
		--exec dbo.PrepareSql @vDataObj, @SesId, @spExecute OUTPUT, @oResult OUTPUT;

		vErrLoc := 200;
		vSpExecute := SUBSTRING(vSpExecute FROM POSITION('SELECT' IN vSpExecute));
		vErrLoc := 300;
		IF((SELECT COUNT(*) FROM pg_cursors WHERE name = 'result_cursor') > 0) THEN
			EXECUTE 'CLOSE result_cursor';
		END IF;

		--exec dbo.ExecuteSql @spExecute, @oResult OUTPUT;

		OPEN result_cursor FOR EXECUTE vSpExecute;
		LOOP
			FETCH NEXT FROM result_cursor INTO vCurRes;
			EXIT WHEN NOT FOUND;
			
			BEGIN
				vRowsProcessed := vRowsProcessed + 1;
				vRes := vRes || '|' || vCurRes;
			END;
		END LOOP;
		CLOSE result_cursor;
		
		--CALL (clfutilLogTrace vCurRes, 0);
		vRes := OVERLAY(vRes PLACING '' FROM 1 FOR 1);

		pResponse := 'ExecuteAndFetchSQL (' || CAST(vRowsProcessed AS VARCHAR) || ' rows)';
		vErrLoc := 400;
		CALL clfSetCLFParameter (vResultVar, vRes);
		CALL clfutilLogTrace (pResponse, 2);
		vErrLoc := 510;
		pResult := 1;
	EXCEPTION WHEN OTHERS THEN
		pResponse := 'Error in DBCLF Function ' || vFuncName || ': ErrLoc: '|| CAST(vErrLoc AS VARCHAR) || ': ' || SQLERRM;
		pResult := 0;
	END;
--CALL clfutilLogTrace ('END_ExecuteAndFetchSQL', 0); -- END LOGGING !!!!!!!
END $$;

-- this function replaces any instance of 0000000000000000 with a blank string.
-- it can be modified later on if needed to detect and replace a wider range if search values
CREATE OR REPLACE FUNCTION clfutilConvertToEmptyString(
	vItem TEXT)
RETURNS TEXT
LANGUAGE plpgsql
AS $$
DECLARE
	vModified TEXT;
BEGIN
	vModified := REPLACE(vItem, '0000000000000000', '');
	RETURN vModified;
END;
$$;
