--------------------------------------------------------------------------------
-- SCRIPT: DBCLFFunctions.or.sql
-- DESCR:  Oracle stored procedures related to DB CLFs
--
-- Copyright Siemens 2023
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE clfutilUpdateInstanceID( 
  instanceType    IN   NUMBER, 
  cdodefid_val    IN   NUMBER, 
  incrementAmt    IN   NUMBER, 
  InstIdNewValue  OUT  VARCHAR2) 
AS 
   -- This procedure is simply a wrapper to the existing csiUpdateInstanceId stored procedure
   -- except that it is wrapped in an autonomous transaction so the COMMIT that occurs will
   -- be independent.
   PRAGMA AUTONOMOUS_TRANSACTION;
BEGIN
   csiUpdateInstanceId(instanceType,cdodefid_val,incrementAmt,InstIdNewValue); 
END;
/
DECLARE
   I NUMBER;
   v_sql VARCHAR2(512);
BEGIN
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'CLFPARAMETERCACHE';
   IF I=1 THEN     
      v_sql:='DROP TABLE CLFPARAMETERCACHE ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
   v_sql:='CREATE GLOBAL TEMPORARY TABLE CLFPARAMETERCACHE (Name VARCHAR2(50), Value CLOB) ON COMMIT DELETE ROWS';
  EXECUTE IMMEDIATE v_sql;
END;
/
CREATE OR REPLACE FUNCTION clfGetCLFParameter(ParameterName IN VARCHAR2)
RETURN CLOB
IS
   vParmValue CLOB;
BEGIN
   SELECT Value
   INTO vParmValue
   FROM CLFParameterCache
   WHERE TO_CHAR(REPLACE(REPLACE(UPPER(Name),'DBCLF::'),'CLF::')) = TO_CHAR(REPLACE(REPLACE(UPPER(ParameterName),'DBCLF::'),'CLF::'));
   
   RETURN vParmValue;
EXCEPTION WHEN NO_DATA_FOUND THEN
   RETURN NULL;
END;
/
CREATE OR REPLACE PROCEDURE clfSetCLFParameter(ParameterName IN VARCHAR2, ParameterValue IN CLOB)
IS
   vParmValue CLOB;
BEGIN
   DELETE FROM CLFParameterCache WHERE Name = UPPER(ParameterName);
   INSERT INTO CLFParameterCache(Name,Value) VALUES (UPPER(ParameterName),ParameterValue);
EXCEPTION WHEN NO_DATA_FOUND THEN
   NULL;
END;
/
CREATE OR REPLACE FUNCTION clfutilStringToArray_Varchar2(pInputString IN CLOB) RETURN DBMS_SQL.VARCHAR2A
IS
   -- This function takes an input delimited string and converts it to a PL/SQL array
   vDelim      VARCHAR2(1) := '|';
   nDelimLen   NUMBER;
   nOffSet     NUMBER := 1; 
   nLastOffset NUMBER := 1;
   vItem       CLOB;
   vMultiVals  DBMS_SQL.VARCHAR2A;
   vIdx        INTEGER;
   nStrLen     INTEGER;
BEGIN
   nDelimLen := LENGTH(vDelim); -- So we don't have to do a LENGTH every loop
   nStrLen := DBMS_LOB.GETLENGTH(pInputString);
   vIdx:=0;
   WHILE nOffset != 0
   LOOP
      nOffset := DBMS_LOB.INSTR(pInputString,vDelim,nLastOffset,1);
      IF nOffset != 0 THEN
         vItem := DBMS_LOB.SUBSTR(pInputString, nOffset-nLastOffset, nLastOffset);
         vMultiVals(vIdx) :=  clfutilConvertToEmptyString(vItem);
         nOffset := nOffset + nDelimLen;
         nLastOffset := nOffset;
         vIdx := vIdx + 1;
      END IF;      
   END LOOP;
   -- By standard, we require that the string does NOT end in a delimiter, meaning that anything
   -- remaining past the last offset is part of the list EVEN IF IT'S NULL.
   -- This means that we will ALWAYS add this last list element:
   vItem := DBMS_LOB.SUBSTR(pInputString, nStrLen - nLastOffset + 1, nLastOffset);
   vMultiVals(vIdx) := clfutilConvertToEmptyString(vItem);
   
   clfutilLogTrace('List size: '||vMultiVals.COUNT,2);
   RETURN vMultiVals;
END;
/
CREATE OR REPLACE FUNCTION clfutilStringToArray_Clob(pInputString IN CLOB) RETURN DBMS_SQL.CLOB_TABLE
IS
   -- This function takes an input delimited string and converts it to a PL/SQL array
   vDelim      VARCHAR2(1) := '|';
   nDelimLen   NUMBER;
   nOffSet     NUMBER := 1; 
   nLastOffset NUMBER := 1;
   vItem       CLOB;
   vMultiVals  DBMS_SQL.CLOB_TABLE;
   vIdx        INTEGER;
   nStrLen     INTEGER;
BEGIN
   nDelimLen := LENGTH(vDelim); -- So we don't have to do a LENGTH every loop
   nStrLen := DBMS_LOB.GETLENGTH(pInputString);
   vIdx:=0;
   WHILE nOffset != 0
   LOOP
      nOffset := DBMS_LOB.INSTR(pInputString,vDelim,nLastOffset,1);
      IF nOffset != 0 THEN
         vItem := DBMS_LOB.SUBSTR(pInputString, nOffset-nLastOffset, nLastOffset);
         vMultiVals(vIdx) := clfutilConvertToEmptyString(vItem);
         nOffset := nOffset + nDelimLen;
         nLastOffset := nOffset;
         vIdx := vIdx + 1;
      END IF;      
   END LOOP;
   -- By standard, we require that the string does NOT end in a delimiter, meaning that anything
   -- remaining past the last offset is part of the list EVEN IF IT'S NULL.
   -- This means that we will ALWAYS add this last list element:
   vItem := DBMS_LOB.SUBSTR(pInputString, nStrLen - nLastOffset + 1, nLastOffset);
   vMultiVals(vIdx) := clfutilConvertToEmptyString(vItem);
   clfutilLogTrace('List size: '||vMultiVals.COUNT,2);
   RETURN vMultiVals;
END;
/
CREATE OR REPLACE FUNCTION clfutilStringToArray_Number(pInputString IN CLOB) RETURN DBMS_SQL.NUMBER_TABLE
IS
   -- This function takes an input delimited string and converts it to a PL/SQL array
   vDelim      VARCHAR2(1) := '|';
   nDelimLen   NUMBER;
   nOffSet     NUMBER := 1; 
   nLastOffset NUMBER := 1;
   vItem       CLOB;
   vMultiVals  DBMS_SQL.NUMBER_TABLE;
   vIdx        INTEGER;
   nStrLen     INTEGER;
BEGIN
   nDelimLen := LENGTH(vDelim); -- So we don't have to do a LENGTH every loop
   nStrLen := DBMS_LOB.GETLENGTH(pInputString);
   vIdx:=0;
   WHILE nOffset != 0
   LOOP
      nOffset := DBMS_LOB.INSTR(pInputString,vDelim,nLastOffset,1);
      IF nOffset != 0 THEN
         vItem := DBMS_LOB.SUBSTR(pInputString, nOffset-nLastOffset, nLastOffset);
         vMultiVals(vIdx) := TO_NUMBER(vItem);
         nOffset := nOffset + nDelimLen;
         nLastOffset := nOffset;
         vIdx := vIdx + 1;
      END IF;      
   END LOOP;
   -- By standard, we require that the string does NOT end in a delimiter, meaning that anything
   -- remaining past the last offset is part of the list EVEN IF IT'S NULL.
   -- This means that we will ALWAYS add this last list element:
   vItem := DBMS_LOB.SUBSTR(pInputString, nStrLen - nLastOffset + 1, nLastOffset);
   vMultiVals(vIdx) := TO_NUMBER(vItem);

   clfutilLogTrace('List size: '||vMultiVals.COUNT,2);
   RETURN vMultiVals;
END;
/
CREATE OR REPLACE FUNCTION clfutilStringToArray_Timestamp(pInputString IN CLOB) RETURN DBMS_SQL.TIMESTAMP_TABLE
IS
   -- This function takes an input delimited string and converts it to a PL/SQL array
   vDelim      VARCHAR2(1)  := '|';
   vDateFormat VARCHAR2(25) := 'MM/DD/YYYY HH:MI:SS AM';
   nDelimLen   NUMBER;
   nOffSet     NUMBER := 1; 
   nLastOffset NUMBER := 1;
   vItem       CLOB;
   vMultiVals  DBMS_SQL.TIMESTAMP_TABLE;
   vIdx        INTEGER;   
   nStrLen     INTEGER;
BEGIN
   nDelimLen := LENGTH(vDelim); -- So we don't have to do a LENGTH every loop
   nStrLen := DBMS_LOB.GETLENGTH(pInputString);
   vIdx:=0;
   WHILE nOffset != 0
   LOOP
      nOffset := DBMS_LOB.INSTR(pInputString,vDelim,nLastOffset,1);
      IF nOffset != 0 THEN
         vItem := DBMS_LOB.SUBSTR(pInputString, nOffset-nLastOffset, nLastOffset);
         vMultiVals(vIdx) := TO_DATE(vItem,vDateFormat);
         nOffset := nOffset + nDelimLen;
         nLastOffset := nOffset;
         vIdx := vIdx + 1;
      END IF;      
   END LOOP;
   -- By standard, we require that the string does NOT end in a delimiter, meaning that anything
   -- remaining past the last offset is part of the list EVEN IF IT'S NULL.
   -- This means that we will ALWAYS add this last list element:
   vItem := DBMS_LOB.SUBSTR(pInputString, nStrLen - nLastOffset + 1, nLastOffset);
   vMultiVals(vIdx) := TO_DATE(vItem,vDateFormat);
   
   clfutilLogTrace('List size: '||vMultiVals.COUNT,2);
   RETURN vMultiVals;
END;
/
DECLARE
   I NUMBER;
   v_sql VARCHAR2(512);
BEGIN
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'CLFERRORLOG';
   IF I=1 THEN     
      v_sql:='DROP TABLE CLFERRORLOG ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
   v_sql:='CREATE TABLE CLFERRORLOG ' ||
                '(LOGDATE TIMESTAMP,
                 LOGMESSAGE VARCHAR2(4000),
                 CLFPKG CLOB) ';
  EXECUTE IMMEDIATE v_sql;
END;
/
DECLARE
   I NUMBER;
   v_sql VARCHAR2(512);
BEGIN
   SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = 'CLFTRACELOG';
   IF I=1 THEN     
      v_sql:='DROP TABLE CLFTRACELOG ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
   v_sql:='CREATE TABLE CLFTRACELOG ' ||
                '(LOGDATE TIMESTAMP,
                 CLFID VARCHAR2(255),
                 LOGMESSAGE CLOB) ';
  EXECUTE IMMEDIATE v_sql;
END;
/
CREATE OR REPLACE PROCEDURE clfutilLogError(pCLFPkg CLOB, pMsg IN VARCHAR2)
IS
PRAGMA AUTONOMOUS_TRANSACTION;
BEGIN
   -- The ClfErrorLog table's LogMessage column has a limited size (4000). 
   -- Make sure we don't overrun it.
   DBMS_OUTPUT.PUT_LINE(pMsg);
   INSERT INTO CLFERRORLOG(LOGDATE,LOGMESSAGE,CLFPKG) VALUES (SYSTIMESTAMP,SUBSTR(pMsg,1,4000),pCLFPkg);
   COMMIT;
END;
/
CREATE OR REPLACE PROCEDURE clfutilLogTrace(pMsg IN CLOB, pTraceLevel IN NUMBER)
IS
   vConfigTraceLevel INTEGER := 1;
   vCLFID VARCHAR2(255);
   
   -- We use this nested procedure because we need to commit autonomously,
   -- but also need to have access to the CLFParameterCache temporary table
   PROCEDURE WRITE_TO_LOG_TABLE(pCLFID VARCHAR2,pLOGMESSAGE CLOB) IS
   PRAGMA AUTONOMOUS_TRANSACTION;
   BEGIN
      INSERT INTO clfTraceLog(LOGDATE,CLFID,LOGMESSAGE) VALUES (SYSTIMESTAMP,pCLFID,pMsg);
      COMMIT;
   END;
BEGIN
   DBMS_OUTPUT.PUT_LINE(SUBSTR(pMsg,1,2000));
   
   BEGIN
      SELECT TO_NUMBER(Value,0)
      INTO vConfigTraceLevel
      FROM CLFParameterCache
      WHERE Name = 'CURRENT_TRACELEVEL';
   EXCEPTION WHEN NO_DATA_FOUND THEN
      vConfigTraceLevel := 0;
   END;
   
   BEGIN
      SELECT Value
      INTO vCLFID
      FROM CLFParameterCache
      WHERE Name = 'CURRENT_CLFID';
   EXCEPTION WHEN NO_DATA_FOUND THEN
      vCLFID := '';
   END;
   
   IF (vConfigTraceLevel >= pTraceLevel) THEN
      WRITE_TO_LOG_TABLE(vCLFID,pMsg);
   END IF;
EXCEPTION WHEN OTHERS THEN
   DBMS_OUTPUT.PUT_LINE(SQLERRM);
END;
/

CREATE OR REPLACE FUNCTION clfutilNewInstanceID(pCDOType IN VARCHAR2)
RETURN VARCHAR2
IS
PRAGMA AUTONOMOUS_TRANSACTION;
   vInstanceIdStr VARCHAR2(128);
   vCDODefId   NUMBER;
BEGIN
   SELECT CDODefID 
   INTO vCDODefID
   FROM CDODefinition
   WHERE CDOName = pCDOType;
   
   csiPRDGetNextInstanceId(vCDODefId, vInstanceIdStr);

   COMMIT;
   RETURN vInstanceIdStr;
END;
/
CREATE OR REPLACE FUNCTION clfsqlGetParameterValue(pFuncPkg IN CLOB, oParamName IN VARCHAR2, oDefaultValue IN VARCHAR2)
RETURN CLOB
IS
   -- Parses the input JSON block for a "Parameters" list, and finds the given ParamName
   -- Example: {"Params":[{"Name":"SQLStatementName","Value":"MySQLQuery"}]}
   -- If the parameter is not found, return DefaultValue
   vDataObj      JSON_OBJECT_T;
   vParmArray    JSON_ARRAY_T;
   vParmObj      JSON_OBJECT_T;
   vParmName     VARCHAR2(512);
   vParmValue    CLOB := oDefaultValue;
BEGIN
   vDataObj := new JSON_OBJECT_T(pFuncPkg); 
   -- Iterate through the Function Params to get what we need
   vParmArray := vDataObj.get_Array('Parameters');
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := vParmObj.get_string('Name');
      vParmValue := vParmObj.get_string('Value');
      IF (vParmName = oParamName) THEN
         RETURN vParmValue;
      END IF;
   END LOOP;
   RETURN vParmValue;
END;
/
CREATE OR REPLACE FUNCTION clfsqlGetQueryText(SQLStatementName IN VARCHAR2)
RETURN QUERYTEXT.QUERYTEXT%TYPE
IS
   vQueryText QUERYTEXT.QUERYTEXT%TYPE;
BEGIN
   SELECT /*+ RESULT_CACHE */ QueryText
   INTO vQueryText
   FROM QueryText
   WHERE QueryDefID = (SELECT /*+ RESULT_CACHE */ QueryDefID FROM QueryDef WHERE UPPER(Name) = UPPER(SQLStatementName))
   AND DBTYPEID IN (0,2); -- Generic (1) or Oracle (2)
   
   RETURN vQueryText;
EXCEPTION WHEN NO_DATA_FOUND THEN
   RETURN NULL;
END;
/
CREATE OR REPLACE PROCEDURE clfsqlBindVariable(pDynCursor IN INTEGER, pVariableName IN VARCHAR2, pDataType IN VARCHAR2, pValue IN CLOB)
IS
   vDateFormat       VARCHAR2(25) := 'MM/DD/YYYY HH:MI:SS AM';
   vFullVariableName VARCHAR2(255) := ':'||pVariableName;
BEGIN
   -- In most cases, Oracle can convert from a string to other data types, but not always.
   -- CLOB to NUMBER:
   --   1) If the QueryDef has the correct datatype of INTEGER, we can convert explicitly
   --   2) If the CLOB string is small enough, we can down-convert it to a VARCHAR2, which
   --      is much easier for Oracle to implicitly convert to the other data types.
   -- CLOB to DATE
   --   1) The query itself has to have a TO_DATE(), which is ugly
   --   2) If the parameter is correctly set to TIMESTAMP in Designer, we can do that TO_DATE here, assuming the format doesn't change.
   --
   IF (pDataType= 'INTEGER') THEN
      DBMS_SQL.BIND_VARIABLE(pDynCursor, vFullVariableName, TO_NUMBER(pValue));
   ELSIF (pDataType='TIMESTAMP') THEN
      -- The app server appears to be using the following date format: 10/16/2020 9:58:07 AM
      DBMS_SQL.BIND_VARIABLE(pDynCursor, vFullVariableName, TO_DATE(pValue,vDateFormat));
   ELSE
     IF (LENGTH(pValue) < 100) THEN
         -- It should be safe to down-convert the CLOB to a regular VARCHAR2 so that Oracle will handle
         -- most implicit conversions. TO_CHAR does the conversion from CLOB to VARCHAR2.
         DBMS_SQL.BIND_VARIABLE(pDynCursor, vFullVariableName, TO_CHAR(pValue));
      ELSE
         DBMS_SQL.BIND_VARIABLE(pDynCursor, vFullVariableName, pValue);
      END IF;
   END IF;
END;
/
CREATE OR REPLACE PROCEDURE clfsqlBindListVariable(pDynCursor IN INTEGER, pVariableName IN VARCHAR2, pDataType IN VARCHAR2, pValue IN CLOB)
IS
   vMultiVals_String    DBMS_SQL.VARCHAR2A;
   vMultiVals_Number    DBMS_SQL.NUMBER_TABLE;
   vMultiVals_Timestamp DBMS_SQL.TIMESTAMP_TABLE;

BEGIN
   -- In most cases, Oracle can convert from a string to other data types, but not always.
   -- CLOB to NUMBER:
   --   1) If the QueryDef has the correct datatype of INTEGER, we can convert explicitly
   --   2) If the CLOB string is small enough, we can down-convert it to a VARCHAR2, which
   --      is much easier for Oracle to implicitly convert to the other data types.
   -- CLOB to DATE
   --   1) The query itself has to have a TO_DATE(), which is ugly
   --   2) If the parameter is correctly set to TIMESTAMP in Designer, we can do that TO_DATE here, assuming the format doesn't change.
   --


   IF (pDataType= 'INTEGER') THEN
      vMultiVals_Number := clfutilStringToArray_Number(pValue);
      DBMS_SQL.BIND_ARRAY(pDynCursor, ':'||pVariableName, vMultiVals_Number); 
   ELSIF (pDataType='TIMESTAMP') THEN
      vMultiVals_Timestamp := clfutilStringToArray_Timestamp(pValue);
      DBMS_SQL.BIND_ARRAY(pDynCursor, ':'||pVariableName, vMultiVals_Timestamp); 
   ELSE
      -- We are using the _Varchar2 version of StringToArray instead of the _Clob
      -- version so that we can support an IN (:PARM) scenario. It seems that Oracle
      -- doesn't like to bind :PARM to a CLOB_TABLE if the variable is within the IN
      -- clause. If we need to support both Varchar2 and Clob lists, we will need to 
      -- address the issue.
      vMultiVals_String := clfutilStringToArray_Varchar2(pValue);
      DBMS_SQL.BIND_ARRAY(pDynCursor, ':'||pVariableName, vMultiVals_String); 
   END IF;
END;
/
CREATE OR REPLACE FUNCTION clfutilBuildMessageFromLabel(pLabelName IN VARCHAR2, vNameArray IN DBMS_SQL.VARCHAR2A,vValueArray IN DBMS_SQL.VARCHAR2A) RETURN VARCHAR2
IS
   -- This method queries a label based on its name (and optionally the Dictionary), then
   -- uses the SQLCursor's result columns to search/replace #ErrorMsg parameters in the label.
   -- The returned string is the final label text.
   vLblText         VARCHAR2(2000) := NULL;
   vDefaultLblText  VARCHAR2(2000);
   vLabelID         NUMBER;   
   vDelim           VARCHAR2(20) := '#ErrorMsg\.';
   vMaxLen          INTEGER:=0;
   vPrimaryDictId   VARCHAR2(16) := NULL;
   vSecondaryDictId VARCHAR2(16) := NULL;
   
BEGIN   
   SELECT LabelID
   INTO vLabelID
   FROM Labels
   WHERE Name = pLabelName;
   
   vPrimaryDictId := clfGetCLFParameter('__PrimaryDictionary');
   vSecondaryDictId := clfGetCLFParameter('__SecondaryDictionary');
   
   -- The order of dictionaries is:
   --   1) Primary Dictionary
   --   2) Secondary Dictionary
   --   3) Default Label
   
   SELECT LabelValue
   INTO vDefaultLblText
   FROM Labels 
   WHERE LabelId = vLabelID;
   
   IF (vPrimaryDictId IS NOT NULL) THEN
      BEGIN
         SELECT LabelValue
         INTO vLblText
         FROM DictionaryLabel
         WHERE DictionaryId = vPrimaryDictId
         AND LabelId = vLabelID;
      EXCEPTION
         WHEN NO_DATA_FOUND THEN
         vLblText := NULL;
      END;
   END IF;
   
   IF (vSecondaryDictId IS NOT NULL AND vLblText IS NULL) THEN
      BEGIN
         SELECT LabelValue
         INTO vLblText
         FROM DictionaryLabel
         WHERE DictionaryId = vSecondaryDictId
         AND LabelId = vLabelID;
      EXCEPTION
         WHEN NO_DATA_FOUND THEN
         vLblText := NULL;
      END;
   END IF;
   
   IF (vLblText IS NULL) THEN
      vLblText := vDefaultLblText;
   END IF;
   
   -- This is a total hack, but since we are doing a serach/replace of the Label string,
   -- we have to account for "similar" tags. eg "Name" vs "Name2". We don't want to replace
   -- the "Name" part of "Name2". So, start with the LONGEST tags and work backwards.
   -- This works, and since we don't expect to have 1000's of tags, performance should be okay.
   FOR i IN vNameArray.FIRST .. vNameArray.LAST LOOP
      IF (LENGTH(vNameArray(i)) > vMaxLen) THEN
         vMaxLen := LENGTH(vNameArray(i));
       END IF;
   END LOOP;
   FOR len IN REVERSE 1..vMaxLen LOOP
       FOR i IN vNameArray.FIRST .. vNameArray.LAST LOOP
          IF (LENGTH(vNameArray(i)) = len) THEN
            vLblText := REGEXP_REPLACE(vLblText,vDelim||vNameArray(i),vValueArray(i),1,0,'i');
         END IF;
       END LOOP;
   END LOOP;   
   
   RETURN vLblText;
END;
/
CREATE OR REPLACE FUNCTION clfsqlPrepareSQL(pFuncPkg IN CLOB, oResponse OUT VARCHAR2)
RETURN NUMBER
IS
   -- The logic of parsing the input JSON, finding the QueryText, and substituting
   -- bind variables is repeated in several procedures, so this function is called
   -- anywhere a DB CLF function needs to create, parse and bind a dynamic SQL 
   -- statement. eg clffuncExecuteSingleSQL, clffuncValidateNotExists
   --
   -- RETURN: A valid DBMS_SQL cursor, or -1 on failure.\
   --         On success, oResponse will contain the SQLStatementName
   --         On failure, oResponse will contain an error message.
   --
   -- NOTE: The calling procedure will need to close the cursor once it's done:
   --       DBMS_SQL.CLOSE_CURSOR(vDynCur);
   --
   vDataObj      JSON_OBJECT_T;
   vParmArray    JSON_ARRAY_T;
   vParmObj      JSON_OBJECT_T;
   vParmName     VARCHAR2(512);
   vParmValue    CLOB;
   vCLFParmValue CLOB;
   vSQLQueryName VARCHAR2(512) := '';   
   vSQL          QueryText.QueryText%TYPE :='';
   
   TYPE SQLPARM_NAMES IS TABLE OF CLOB INDEX BY VARCHAR2(128);
   vSQLParms     SQLPARM_NAMES;
   
   TYPE SQLPARM_TYPES IS TABLE OF VARCHAR2(30) INDEX BY VARCHAR2(128);
   vSQLParmTypes SQLPARM_TYPES;
   
   TYPE SQLPARM_ISLIST IS TABLE OF BOOLEAN INDEX BY VARCHAR2(128);
   vSQLParmIsList SQLPARM_ISLIST;
   
   vDynCur        NUMBER := -1; -- DBMS_SQL handle
   vRowsProcessed NUMBER;
   
   vErrLoc        NUMBER;
   vDelim         VARCHAR2(1) := '|';
   CURSOR cQueryParms IS 
      SELECT /*+ RESULT_CACHE */ p.Name ParmName,dt.Name ParmType, p.IsList
      FROM QueryParms p
      JOIN CppDataTypes dt ON dt.DataTypeId = p.CppDataTypeId
      WHERE QueryDefId = (SELECT /*+ RESULT_CACHE */ QueryDefId FROM QueryDef WHERE Name = vSQLQueryName);      
BEGIN
   vErrLoc := 1;
   oResponse := '';
   -- JSON Format Example --
   -- {"Name":"ExecuteSingleSQL","ID":"3823","Params":[{"Name":"SQLStatementName","Value":"MySQLQuery"}],"SQLParameters":[{"Name":"Qty","Value":"1234"},{"Name":"ContainerName","Value":"MyContainer1"}]}
      
   vDataObj := new JSON_OBJECT_T(pFuncPkg); 
   -- Iterate through the Function Params to get what we need
   vErrLoc := 10;
   vSQLQueryName := clfsqlGetParameterValue(pFuncPkg,'SQLStatementName','');

   -- Loop through the parameters defined for the Query and add them to the
   -- parameter array. Values will be populated later by either CLF-level or Function-level data.
   vErrLoc := 100;
   FOR crec IN cQueryParms 
   LOOP 
      vParmName := UPPER(crec.ParmName);
      vSQLParms(vParmName) := '';
      vSQLParmTypes(vParmName) := UPPER(crec.ParmType);
      vSQLParmIsList(vParmName) := CASE WHEN crec.IsList=1 THEN TRUE ELSE FALSE END;
   END LOOP;

   -- Get the SQL Statement (QueryText) from metadata
   vErrLoc := 200;
   vSQL := clfsqlGetQueryText(vSQLQueryName);
   IF (vSQL IS NULL) THEN
      oResponse := 'Query not found: '||vSQLQueryName; 
      RETURN -1;     
   END IF;
   clfutilLogTrace('SQL ('||vSQLQueryName||'): '||vSQL,1);   

   -- Loop through SQL parameters. For each one that exists in our vSQLParms, set its value.
   vParmArray := vDataObj.get_Array('SQLParameters');
   vErrLoc := 400;
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vErrLoc := vErrLoc + 1;
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := UPPER(vParmObj.get_string('Name'));
      vParmValue := vParmObj.get_string('Value');
      IF (vSQLParms.EXISTS(vParmName) AND LENGTH(vParmValue) > 0) THEN            
         IF (vParmValue LIKE 'CLF::%' OR vParmValue LIKE 'DBCLF::%') THEN               
            -- This is an expression, which points to a CLF-level parameter value,
            -- hopefully included in the vCLFParms collection.            
            vCLFParmValue := clfGetCLFParameter(vParmValue); 
            IF (vCLFParmValue IS NULL) THEN
               oResponse := 'Parameter '||vParmName||' references CLF variable '||vParmValue||' but that variable does not exist.'; 
               RETURN -1;               
            END IF;
            vSQLParms(vParmName) := vCLFParmValue||''; -- It seems that "modifying" the CLOB string causes a new copy to be created, which is then much faster to parse than the JSON-bound string.          
         ELSE
            vSQLParms(vParmName) := vParmValue||'';
         END IF;   
         IF (vParmName LIKE 'LITERAL%') THEN
            -- Special case: If the parameter name begins with LITERAL, then the parameter value is meant
            -- to be inserted into the SQL statement directly. e.g. SELECT ... WHERE ... IN (?LITERAL_Param) -> SELECT ... WHERE ... IN ('a','b','c')
            clfutilLogTrace(vParmName||'(Literal Replacement)='||vSQLParms(vParmName),2); 
            vSQL := REGEXP_REPLACE(vSQL,'\?'||vParmName,vSQLParms(vParmName),1,0,'i');
            vSQLParms.DELETE(vParmName);
         END IF;
      END IF;
   END LOOP;
   
   -- For each SQL Parameter in the SQL Text, replace ?Param with :Param
   -- Our system expects parameters to be prefixed with ?, but Oracle expects :   
   -- Note that we use REGEXP_REPLACE so we can perform a case-insensitive search
   vErrLoc := 500;
   vParmName := vSQLParms.FIRST;
   WHILE vParmName IS NOT NULL LOOP
      vErrLoc := vErrLoc + 1;
      vSQL := REGEXP_REPLACE(vSQL,'\?'||vParmName,':'||vParmName,1,0,'i');
      vParmName := vSQLParms.NEXT(vParmName);
   END LOOP;

   vErrLoc := 600;   
   -- Using DBMS_SQL, construct the database call and execute it   
   
   vErrLoc := 610;     
   vDynCur := DBMS_SQL.OPEN_CURSOR;
   vErrLoc := 620; 
   DBMS_SQL.PARSE(vDynCur, vSQL, DBMS_SQL.NATIVE);    
   vErrLoc := 700;
   vParmName := vSQLParms.FIRST;
   WHILE vParmName IS NOT NULL LOOP
      vErrLoc := vErrLoc + 1;
      -- If it's marked as a list, and actually contains a comma, bind it as an array
      IF (vSQLParmIsList(vParmName) AND DBMS_LOB.INSTR(vSQLParms(vParmName),vDelim) >0) THEN
         clfutilLogTrace(vParmName||'('||vSQLParmTypes(vParmName)||')'||'(List)='||vSQLParms(vParmName),2);
         clfsqlBindListVariable(vDynCur, vParmName, vSQLParmTypes(vParmName), vSQLParms(vParmName));                   
      ELSE
         clfutilLogTrace(vParmName||'('||vSQLParmTypes(vParmName)||')'||'='||vSQLParms(vParmName),2);
         clfsqlBindVariable(vDynCur, vParmName, vSQLParmTypes(vParmName), vSQLParms(vParmName));          
      END IF;
      vParmName := vSQLParms.NEXT(vParmName);
   END LOOP;
   oResponse := vSQLQueryName;
   RETURN vDynCur;
END;
/

CREATE OR REPLACE PROCEDURE clffuncExecuteSingleSQL(pFuncPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
IS
   vDataObj      JSON_OBJECT_T;
   vFuncName     VARCHAR2(255);
   vDynCur        NUMBER; -- DBMS_SQL handle
   vSQLQueryName VARCHAR2(512) := ''; 
   vRowsProcessed NUMBER;   
   vErrLoc        NUMBER;
BEGIN
   vErrLoc := 1;
   oResponse := '';
   vDataObj := new JSON_OBJECT_T(pFuncPkg); 
   vFuncName := vDataObj.get_String('Name');   
   IF (vFuncName IS NULL) THEN
      -- Something must be wrong with the JSON document
      oResponse := 'Invalid JSON document ($.Name).'; 
      RETURN;
   END IF;
   
   vDynCur := clfsqlPrepareSQL(pFuncPkg, oResponse);
   
   IF (vDynCur = -1) THEN -- invalid handle
      RAISE_APPLICATION_ERROR(-20101,oResponse);
   END IF;
   vSQLQueryName := oResponse;
   oResponse := '';
   
   vErrLoc := 800;
   vRowsProcessed := DBMS_SQL.EXECUTE(vDynCur);
   DBMS_SQL.CLOSE_CURSOR(vDynCur);
   clfutilLogTrace('Rows processed: '||vRowsProcessed,1);
   -- Don't validate RowsProcessed here. If the user wants to validate, they can do it explicitly.
   --IF (vRowsProcessed = 0) THEN
   --   RAISE_APPLICATION_ERROR(-20102, 'No rows processed from SQL statement.');
   --END IF;
   vErrLoc := 810;
   oResult := 1;
EXCEPTION 
   WHEN OTHERS THEN
      oResponse := 'Error in DBCLF Function '||vFuncName||': ErrLoc: '||TO_CHAR(vErrLoc)||': '||SQLERRM;
      IF DBMS_SQL.IS_OPEN(vDynCur) THEN
         DBMS_SQL.CLOSE_CURSOR(vDynCur);      
      END IF;      
   oResult := 0;      
END; 
/

 CREATE OR REPLACE PROCEDURE clfExecute(pTxnId IN VARCHAR2, pCLFPkg IN CLOB, oResult OUT NUMBER, oResponse OUT CLOB)
IS
   /*
   PROCEDURE: clfExecute
   DESCR: The main logic for parsing and executing DB CLF packages
   PARAMS: 
      pTxnId: The TxnId for this transaction (used in HistoryMainline)
      pCLFPkg: A JSON document representing the CLF to be executed
      oResult: (out) 1=SUCCESS, 0=FAILURE
      oResponse: (out) Returned JSON document, but for now, just an error message.
   */
   vCLFObj       JSON_OBJECT_T;
   vCLFName      VARCHAR2(128);
   vFuncJSON     CLOB;
   vParmArray    JSON_ARRAY_T;   
   vParmName     VARCHAR2(512);
   vParmObj      JSON_OBJECT_T;
   vParmValue    CLOB;
   vFuncArray    JSON_ARRAY_T;
   vFuncResponse VARCHAR2(512);
   vFuncName     VARCHAR2(80);
   vFuncRet      NUMBER;
   vErrLoc       NUMBER; -- Used to identify location of errors
   vSQL          VARCHAR2(512);
   vTraceLevel   NUMBER;
BEGIN
   -- NOTES
   --    * oResponse must be limited in size because the calling application has to pre-allocate
   --      memory for the string. 
   --      Currently: 8000
   --    * For now, oResponse will simply be an error message (not JSON) 
   vErrLoc := 1;
   vCLFObj := JSON_OBJECT_T.PARSE(pCLFPkg);

   clfSetCLFParameter('TXNID', pTxnId);
   
   -- Iterate through the CLF-level parameters to get certain values
   vErrLoc := 2;
   vCLFName := 'Unknown CLF';
   vParmArray := vCLFObj.get_Array('CLFParameters');
   vErrLoc := 100;
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vErrLoc := vErrLoc + 1;
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := vParmObj.get_String('Name');
      vParmValue := vParmObj.get_CLOB('Value');
      clfSetCLFParameter(UPPER(vParmName), vParmValue);
      IF (vParmName = '__MethodName') THEN
         vCLFName := vParmValue;
      END IF;
   END LOOP;
   
   BEGIN
      SELECT /*+ RESULT_CACHE */ TO_NUMBER(TValue,0)
      INTO vTraceLevel
      FROM InSiteSiteInfo
      WHERE UPPER(TName) = 'DBCLFTRACELEVEL';
   EXCEPTION WHEN NO_DATA_FOUND THEN
      vTraceLevel := 0;
   END;
   clfSetCLFParameter('CURRENT_CLFID',pTxnId||'-'||vCLFName);
   clfSetCLFParameter('CURRENT_TRACELEVEL',TO_CHAR(vTraceLevel));   
   
   clfutilLogTrace('BEGIN',1);
   clfutilLogTrace(pCLFPkg,2);
   
   vErrLoc := 5;
   vFuncArray := JSON_ARRAY_T(JSON_QUERY(pCLFPkg,'$.Functions'));
   IF (vFuncArray.get_size() = 0) THEN
      -- Something must be wrong with the JSON document
      oResponse := 'Error in CLF "'||vCLFName||'": Invalid JSON document ($.Functions).';
      oResult := 0;
      clfutilLogError(pCLFPkg,oResponse);
      clfutilLogTrace(oResponse,1);
      clfutilLogTrace('END',1);
      RETURN;
   END IF;   
      
   -- Iterate through all the Functions within the document. Each one should identify
   -- its corresponding stored procedure (eg ExecuteSingleSQL, etc.) along with 
   -- Parameter values to be passed to that procedure.
   --
   -- All of the "child" procedures MUST have the same signature, namely:
   -- PROCEDURE clffunc<FunctionName>(pJSONPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
   -- This way, we can add Functions just by adding them to Designer (metadata) and creating a
   -- corresponding stored procedure.
   -- 
   vErrLoc := 100;
   FOR indx IN 0 .. vFuncArray.get_size() - 1
   LOOP
      vFuncJSON := JSON_QUERY(pCLFPkg,'$.Functions['||indx||']');
      vErrLoc := vErrLoc + 1;
      vFuncName := JSON_VALUE(vFuncJSON,'$.Name');
      
      -- TODO: Map from vFuncName to the actual stored proc name. For now, I'm just prepending 'clffunc'
      vSQL := 'CALL clffunc'||vFuncName||'(:p1,:p2,:p3)';
      EXECUTE IMMEDIATE vSQL USING IN vFuncJSON, OUT vFuncRet, OUT vFuncResponse; 
      IF (vFuncRet = 0) THEN
         oResponse := '';
         IF (UPPER(vFuncName) NOT LIKE '%VALIDATE%') THEN
            -- Validation functions supply their own error message. For other functions, we'll build some
            -- info for the message
            oResponse := 'Error in DBCLF "'||vCLFName||'" Function "'||vFuncName||'": ';
         END IF;
         oResponse := oResponse||vFuncResponse;
         oResult := 0;
         clfutilLogError(pCLFPkg,oResponse);
         clfutilLogTrace(oResponse,1);
         clfutilLogTrace('END',1);
         RETURN;
      END IF;
   END LOOP;  
   
   oResult := 1;
   oResponse := '';
   clfutilLogTrace('END',1);
EXCEPTION
WHEN OTHERS THEN
   oResponse := 'Error in DBCLF "'||vCLFName||'" Function "'||vFuncName||'": ErrLoc: '||TO_CHAR(vErrLoc)||': '||SQLERRM;
   oResult := 0;
   clfutilLogError(pCLFPkg,oResponse);
   clfutilLogTrace(oResponse,1);
   clfutilLogTrace('END',1);
END;
/
CREATE OR REPLACE PROCEDURE clffuncDBGenerateInstanceIDs(pFuncPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
IS
   -- Generates one or more InstanceIDs (NumInstanceIDs) based on the CDO Type requested (CDOTypeName). Stores the delimited string list
   -- in CLFParameterCache using the requested variable name (Result).
   vDataObj      JSON_OBJECT_T;
   vFuncName     VARCHAR2(255);
   vParmArray    JSON_ARRAY_T;
   vParmObj      JSON_OBJECT_T;
   vParmName     VARCHAR2(512);
   vParmValue    CLOB;
   vCLFParmValue CLOB;
   vCDOTypeName  VARCHAR2(255);
   vNumInstanceIDs INTEGER;
   vResultVar    VARCHAR2(255);   
   vIID          VARCHAR2(50);
   vCDODefId       NUMBER;
   vCDODefIdStr    VARCHAR2(16);
   vInstIdNewValue VARCHAR2(16); 
   vIDCount        INTEGER;
   iInstIdInt      NUMBER; 
   iHexSite        NUMBER; 
   iHexId          NUMBER;
   
   TYPE PARMS_TYPE IS TABLE OF CLOB INDEX BY VARCHAR2(128);
   vFuncParms    PARMS_TYPE;
   vErrLoc       NUMBER;   
   vDelim        VARCHAR2(1) := '|';
BEGIN
   vErrLoc := 1;
   oResponse := '';
   -- JSON Format --
   -- {"Name":"DBGenerateInstanceIDs","ID":"3834","Parameters":[{"Name":"CDOTypeName","Value":"HistoryMainline"},{"Name":"NumInstanceIDs","Value":"10"},{"Name":"Result","Value":"DBCLF::HistoryMainlineId"}]}
      
   vDataObj := new JSON_OBJECT_T(pFuncPkg);   
   vFuncName := vDataObj.get_String('Name');   
   IF (vFuncName IS NULL) THEN
      -- Something must be wrong with the JSON document
      RAISE_APPLICATION_ERROR(-20101,'Invalid JSON document ($.Name).'); 
   END IF;
      
   -- Iterate through the Function Params to get what we need
   vErrLoc := 10;
   vParmArray := vDataObj.get_Array('Parameters');
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vErrLoc := vErrLoc + 1;
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := vParmObj.get_string('Name');
      vParmValue := vParmObj.get_string('Value');  
      vFuncParms(vParmName) := vParmValue;
   END LOOP;
   
   vErrLoc := 20;
   BEGIN
      vCDOTypeName := vFuncParms('CDOTypeName');      
      vResultVar := vFuncParms('Result');
      vParmValue := vFuncParms('NumInstanceIDs'); -- This may be a DBCLF:: variable, so let's check.      
      IF (vParmValue LIKE 'CLF::%' OR vParmValue LIKE 'DBCLF::%') THEN               
         -- This is an expression, which points to a CLF-level parameter value,
         -- hopefully included in the vCLFParms collection.            
         vCLFParmValue := clfGetCLFParameter(vParmValue); 
         IF (vCLFParmValue IS NULL) THEN
            oResponse := 'Parameter NumInstanceIDs references CLF variable '||vParmValue||' but that variable does not exist.'; 
            oResult := -1;   
            RETURN;
         END IF;
         vNumInstanceIDs := TO_NUMBER(vCLFParmValue); 
      ELSE
         vNumInstanceIDs := TO_NUMBER(vParmValue);
      END IF;
      
      -- This procedure originally called clfutilNewInstanceID in a loop, but that proved to be
      -- inefficient since it was having to go through the entire process of updating instanceIDCount, etc.
      vErrLoc := 15;
      SELECT /*+ RESULT_CACHE */ CDODefID 
      INTO vCDODefID
      FROM CDODefinition
      WHERE CDOName = vCDOTypeName;
   
      -- The call to csiUpdateInstanceID does two things:
      --    1) Increments the value in InstanceIDCount (and Commits)
      --    2) Builds and returns the InstanceID portion of the last available ID in your block
      --       e.g. 00000000000009B6
      clfutilUpdateInstanceID(0,vCDODefID,vNumInstanceIDs,vInstIdNewValue); 
    
      -- Get the Site part of the InstanceId (if we have multiple sites, it's a little different.)
      SELECT /*+ RESULT_CACHE */ COUNT(*) INTO vIDCount FROM DBIdentifier; 
      IF(vIDCount != 1) then 
         iHexSite := TO_NUMBER(0000000000,'xxxxxxxxxx');  
      ELSE 
         SELECT /*+ RESULT_CACHE */ TO_NUMBER(NVL(DBIdentifier,0),'xxxxxxxxxx') INTO iHexSite FROM DBIdentifier;  
      END IF;
     
      -- Now, we have to build the "full" InstanceID by adding the Site prefix and the CDODefId
   
      -- Chop off the first part of the InstanceID portion, convert it to numeric, combine it with the Site
      vInstIdNewValue:=SUBSTR(vInstIdNewValue,7,10);
      iHexId := TO_NUMBER(vInstIdNewValue,'xxxxxxxxxx') - vNumInstanceIDs + 1; -- Go to the beginning of this block of IDs (vInstIdNewValue is the last one; we want the first one).
      iInstIdInt := iHexSite + iHexId - BITAND(iHexSite,iHexId);	-- this is doing a Bitwise OR
      vInstIdNewValue:=LPAD(LTRIM(TO_CHAR(iInstIdInt,'xxxxxxxxxx')),10,'0'); 
    
      -- Finally, embed the CDODefId, and convert the whole string to lower case
      vCDODefIdStr:=LPAD(LTRIM(TO_CHAR(vCDODefID,'xxxxxx')),6,'0'); 
      vIID:=LOWER(vCDODefIdStr||vInstIdNewValue); 
      vParmValue := vIID;
   
      FOR idx IN 1..vNumInstanceIDs-1 LOOP
         -- Since we already have a valid hex string, we can just increment it by one
         -- without having to go through all of the Site/CDODefId logic again.
         csiIncrementString64(vIID,1,vIID);
         vParmValue := vParmValue||vDelim||LOWER(vIID);
      END LOOP;
      clfSetCLFParameter(vResultVar,vParmValue);     
   EXCEPTION 
   WHEN OTHERS THEN
      oResponse := 'Error in CLF Function '||vFuncName||', ErrLoc: '||TO_CHAR(vErrLoc)||': '||SQLERRM||'.';
      oResult := 0;
      RETURN;
   END;   
   oResult := 1;
END;
/

CREATE OR REPLACE PROCEDURE clffuncDBGenerateSequence(pFuncPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
IS
   -- This function generates a numeric sequence of numbers that can be used in other
   -- functions (e.g. ExecuteSingleSQL. The output is stored in the CLFParameterCache
   -- temporary table in list form: 1|2|3|4|5
   --
   vDataObj      JSON_OBJECT_T;
   vFuncName     VARCHAR2(255);
   vParmArray    JSON_ARRAY_T;
   vParmObj      JSON_OBJECT_T;
   vParmName     VARCHAR2(512);
   vParmValue    CLOB;
   
   vStartNum     INTEGER;
   vEndNum       INTEGER;
   vStepSize     INTEGER;
   vNum          INTEGER;
   vResultVar    VARCHAR2(255);   
   
   TYPE PARMS_TYPE IS TABLE OF CLOB INDEX BY VARCHAR2(128);
   vFuncParms    PARMS_TYPE;
   vIsFirst      BOOLEAN := TRUE;
   vErrLoc       NUMBER;   
   vDelim        VARCHAR2(1) := '|';
BEGIN
   vErrLoc := 1;
   oResponse := '';
   -- JSON Parameters: StartNumber, EndNumber, StepSize, Result
   -- {"Name":"DBGenerateSequence","ID":"3834","Parameters":[{"Name":"StartNumber","Value":"1"},{"Name":"EndNumber","Value":"10"},{"Name":"StepSize","Value":"1"},{"Name":"Result","Value":"DBCLF::Sequence"}]}      
   vDataObj := new JSON_OBJECT_T(pFuncPkg);   
   vFuncName := vDataObj.get_String('Name');   
   IF (vFuncName IS NULL) THEN
      -- Something must be wrong with the JSON document
      RAISE_APPLICATION_ERROR(-20101,'Invalid JSON document ($.Name).'); 
   END IF;
      
   -- Iterate through the Function Params to get what we need
   vErrLoc := 10;
   vParmArray := vDataObj.get_Array('Parameters');
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vErrLoc := vErrLoc + 1;
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := vParmObj.get_string('Name');
      vParmValue := vParmObj.get_string('Value');   
      vFuncParms(vParmName) := vParmValue;
   END LOOP;
 
   vErrLoc := 20;
   vStartNum := TO_NUMBER(vFuncParms('StartNumber'));
   vEndNum := TO_NUMBER(vFuncParms('EndNumber'));
   vStepSize := TO_NUMBER(vFuncParms('StepSize'));
   vResultVar := vFuncParms('Result');
      
   vErrLoc := 15;
   vNum := vStartNum;
   vParmValue := '';
   WHILE (vNum <= vEndNum) LOOP
      IF (NOT vIsFirst) THEN
         vParmValue := vParmValue||vDelim;
      END IF;
      vParmValue := vParmValue||TO_CHAR(vNum);
      vNum := vNum + vStepSize;
      vIsFirst := FALSE;
   END LOOP;
   clfSetCLFParameter(vResultVar,vParmValue);      
   oResult := 1;
END;
/

CREATE OR REPLACE PROCEDURE clffuncDBValidateNotExists(pFuncPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
IS
   -- This validation parses and executes the given query. If any rows are returned, the
   -- validation fails. No data is actually fetched; the validation simply looks at the
   -- number of rows returned from DBMS_SQL.PARSE_AND_EXECUTE
   --
   vDataObj          JSON_OBJECT_T;
   vFuncName         VARCHAR2(255);
   vDynCur           NUMBER; -- DBMS_SQL handle
   vSQLQueryName     VARCHAR2(512) := ''; 
   vRowsProcessed    NUMBER;   
   vErrLoc           NUMBER;
   vReturnErrorMsg   VARCHAR2(512);   
   vErrorID          VARCHAR2(255);
   VALIDATION_FAILED EXCEPTION; 
   vColCount         INTEGER := 0;
   vColDesc          DBMS_SQL.DESC_TAB2;
   vNameArray        DBMS_SQL.VARCHAR2A;
   vValueArray       DBMS_SQL.VARCHAR2A;
   vSingleVal_varchar  VARCHAR2(8000);
BEGIN
   vErrLoc := 1;
   oResponse := '';
   vDataObj := new JSON_OBJECT_T(pFuncPkg); 
   vFuncName := vDataObj.get_String('Name');   
   IF (vFuncName IS NULL) THEN
      -- Something must be wrong with the JSON document
      oResponse := 'Invalid JSON document ($.Name).'; 
      RETURN;
   END IF;
   vReturnErrorMsg := clfsqlGetParameterValue(pFuncPkg,'ErrorMessage','Validation failed');
   vErrorID := clfsqlGetParameterValue(pFuncPkg,'ErrorID',NULL);
   vDynCur := clfsqlPrepareSQL(pFuncPkg, oResponse);
   
   IF (vDynCur = -1) THEN -- invalid handle
      RAISE_APPLICATION_ERROR(-20101,oResponse);
   END IF;
   vSQLQueryName := oResponse;
   oResponse := '';
   
   DBMS_SQL.DESCRIBE_COLUMNS2(vDynCur, vColCount, vColDesc);   
   FOR i IN 1 .. vColCount LOOP
      -- TODO: Handle different data types as necessary. For now, most types can be converted to string
      vNameArray(i) := vColDesc(i).col_name;   
      DBMS_SQL.DEFINE_COLUMN(vDynCur,i,vSingleVal_varchar,8000);
   END LOOP;
   
   vErrLoc := 800;
   vRowsProcessed := DBMS_SQL.EXECUTE_AND_FETCH(vDynCur);   
   clfutilLogTrace('Rows processed: '||vRowsProcessed,1);
   IF (vRowsProcessed != 0) THEN
      IF (vErrorID IS NOT NULL) THEN
         -- We need to lookup, parse and return an error message based off of a Label
         FOR i IN 1 .. vColCount LOOP
            DBMS_SQL.COLUMN_VALUE(vDynCur,i,vSingleVal_varchar);
            vValueArray(i) := vSingleVal_varchar;
         END LOOP;
         vReturnErrorMsg := clfutilBuildMessageFromLabel(vErrorID,vNameArray,vValueArray);   
      END IF;
      RAISE VALIDATION_FAILED;
   END IF;
   DBMS_SQL.CLOSE_CURSOR(vDynCur);
   oResult := 1;
EXCEPTION 
   WHEN VALIDATION_FAILED THEN
      oResponse := vReturnErrorMsg;
      IF DBMS_SQL.IS_OPEN(vDynCur) THEN
         DBMS_SQL.CLOSE_CURSOR(vDynCur);      
      END IF;      
      oResult := 0;
      RETURN;
WHEN OTHERS THEN
      oResponse := 'Error in DBCLF Function '||vFuncName||': SQLStatementName: '||vSQLQueryName||', ErrLoc: '||TO_CHAR(vErrLoc)||': '||SQLERRM;
      IF DBMS_SQL.IS_OPEN(vDynCur) THEN
         DBMS_SQL.CLOSE_CURSOR(vDynCur);      
      END IF;      
      oResult := 0;
      RETURN;  
END;
/

CREATE OR REPLACE PROCEDURE clffuncExecuteAndFetchSQL(pFuncPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
IS
   -- This function executes the given SQL statement and fetches the results from the
   -- first column in the resultset to build a DBCLF:: parameter.
   vDataObj      JSON_OBJECT_T;
   vFuncName     VARCHAR2(255);
   vParmArray    JSON_ARRAY_T;
   vParmObj      JSON_OBJECT_T;
   vParmName     VARCHAR2(512);
   vParmValue    CLOB;
   TYPE PARMS_TYPE IS TABLE OF CLOB INDEX BY VARCHAR2(128);
   vFuncParms    PARMS_TYPE;
   vResultVar    VARCHAR2(255);  
   vDynCur       NUMBER; -- DBMS_SQL handle
   vSQLQueryName VARCHAR2(512) := ''; 
   vRowsProcessed NUMBER; 
   vTotalRows     NUMBER := 0; 
   vColCount      INTEGER := 0;
   vColDesc       DBMS_SQL.DESC_TAB2;
   vErrLoc        NUMBER;
   
   vSingleVal_varchar VARCHAR2(8000);
   vSingleVal_clob    CLOB;
   vSingleVal_date    DATE;
   vIsFirst           BOOLEAN := TRUE;  
   vDelim             VARCHAR2(1) := '|';
BEGIN
   vErrLoc := 1;
   oResponse := '';
   vDataObj := new JSON_OBJECT_T(pFuncPkg); 
   vFuncName := vDataObj.get_String('Name');   
   IF (vFuncName IS NULL) THEN
      -- Something must be wrong with the JSON document
      oResponse := 'Invalid JSON document ($.Name).'; 
      RETURN;
   END IF;
   
   -- Iterate through the Function Params to get what we need
   vErrLoc := 10;
   vParmArray := vDataObj.get_Array('Parameters');
   FOR indx IN 0 .. vParmArray.get_size() - 1
   LOOP
      vErrLoc := vErrLoc + 1;
      vParmObj := TREAT(vParmArray.get(indx) AS JSON_OBJECT_T);      
      vParmName  := vParmObj.get_string('Name');
      vParmValue := vParmObj.get_string('Value');   
      vFuncParms(vParmName) := vParmValue;
   END LOOP;
   vResultVar := vFuncParms('Result');
   
   vErrLoc := 100;
   vDynCur := clfsqlPrepareSQL(pFuncPkg, oResponse);
   
   IF (vDynCur = -1) THEN -- invalid handle
      RAISE_APPLICATION_ERROR(-20101,oResponse);
   END IF;
   vSQLQueryName := oResponse;
   oResponse := '';
   
   vErrLoc := 200;
   -- Since this is a dynamic SQL statement, we don't know the column datatypes
   -- (and for now, we are only fetching the first column). We use DESCRIBE_COLUMNS2
   -- to tell us the column's data type. The main expected values for col_type are:
   --    1 = VARCHAR2
   --    2 = NUMBER
   --    8 = LONG
   --   12 = DATE
   --   96 = CHAR
   --  112 = CLOB
   --  180 = TIMESTAMP
   DBMS_SQL.DESCRIBE_COLUMNS2(vDynCur, vColCount, vColDesc);
   
   -- Oracle seems to convert most incoming data types to VARCHAR2 just fine, except
   -- for DATE. So, we treat DATE differently by binding to that data type and then
   -- converting from DATE to VARCHAR2 in the fetch.
   CASE vColDesc(1).col_type
      WHEN 12 THEN DBMS_SQL.DEFINE_COLUMN(vDynCur,1,vSingleVal_date);
      WHEN 112 THEN DBMS_SQL.DEFINE_COLUMN(vDynCur,1,vSingleVal_clob);
      ELSE DBMS_SQL.DEFINE_COLUMN(vDynCur,1,vSingleVal_varchar,8000);
   END CASE;
   
   vErrLoc := 300;
   vParmValue := '';
   vRowsProcessed := DBMS_SQL.EXECUTE_AND_FETCH(vDynCur);
   WHILE(vRowsProcessed>0) LOOP
      vTotalRows := vTotalRows + 1;
      CASE vColDesc(1).col_type
         WHEN 12 THEN -- DATE
            BEGIN
               DBMS_SQL.COLUMN_VALUE(vDynCur,1,vSingleVal_date);
               vSingleVal_varchar := to_char(vSingleVal_date,'MM/DD/YYYY HH24:MI:SS');
            END;
         WHEN 112 THEN -- CLOB
            BEGIN
               DBMS_SQL.COLUMN_VALUE(vDynCur,1,vSingleVal_clob);               
            END;
         ELSE DBMS_SQL.COLUMN_VALUE(vDynCur,1,vSingleVal_varchar);
      END CASE;
      --
      IF (NOT vIsFirst) THEN
         vParmValue := vParmValue||vDelim;
      END IF;
      CASE vColDesc(1).col_type
         WHEN 112 THEN vParmValue := vParmValue||vSingleVal_clob;
         ELSE vParmValue := vParmValue||vSingleVal_varchar;
      END CASE;
      vIsFirst := FALSE;
      vRowsProcessed := DBMS_SQL.FETCH_ROWS(vDynCur);
   END LOOP;
   
   vErrLoc := 400;
   clfSetCLFParameter(vResultVar,vParmValue);
   clfutilLogTrace('ExecuteAndFetchSQL ('||vTotalRows||' rows): '||vParmValue,2);
   DBMS_SQL.CLOSE_CURSOR(vDynCur);
   vErrLoc := 510;
   oResult := 1;
EXCEPTION 
   WHEN OTHERS THEN
      oResponse := 'Error in DBCLF Function '||vFuncName||': ErrLoc: '||TO_CHAR(vErrLoc)||': '||SQLERRM;
      IF DBMS_SQL.IS_OPEN(vDynCur) THEN
         DBMS_SQL.CLOSE_CURSOR(vDynCur);      
      END IF;      
   oResult := 0;      
END; 
/

CREATE OR REPLACE FUNCTION clfutilConvertToEmptyString (vItem IN CLOB) RETURN CLOB
IS
   -- this function replaces any instance of 0000000000000000 with a blank string.
   -- it can be modified later on if needed to detect and replace a wider range if search values
   
    vModified CLOB;
BEGIN   
    vModified := REPLACE(vItem, '0000000000000000', '');
    RETURN vModified;    
END;
