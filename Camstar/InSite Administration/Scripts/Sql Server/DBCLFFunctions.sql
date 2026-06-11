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
DECLARE
    @I INT,
    @v_sql NVARCHAR(512);
BEGIN
    SELECT @I = COUNT(*)
    FROM sys.tables
    WHERE Name = 'CLFErrorLog';

    IF(@I = 1)
    BEGIN
        SET @v_sql = N'DROP TABLE CLFErrorLog';
        EXECUTE sp_executesql @v_sql;
    END;

    SET @v_sql = N'CREATE TABLE CLFErrorLog (LogDate DATETIME, LogMessage NVARCHAR(4000), CLFPkg NVARCHAR(max))';
    EXECUTE sp_executesql @v_sql;

    SELECT @I = COUNT(*)
    FROM sys.tables
    WHERE Name = 'CLFTraceLog';

    IF(@I = 1)
    BEGIN
        SET @v_sql = N'DROP TABLE CLFTraceLog';
        EXECUTE sp_executesql @v_sql;
    END;

    SET @v_sql = N'CREATE TABLE CLFTraceLog (LogDate DATETIME, CLFID NVARCHAR(255), LogMessage NVARCHAR(max))';
    EXECUTE sp_executesql @v_sql;

END;
GO

--------------------------------------------------------------------------------
--clfutilNewInstanceID
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilNewInstanceID(@pCDOType NVARCHAR(255), @vInstanceIdStr VARCHAR(max) OUTPUT, @v_Amt INT = 1)
AS
BEGIN
    DECLARE
        @vLoopBackNewInstanceID NVARCHAR(MAX),
        @vCDODefId INT,
        @vLastInstanceIdStr VARCHAR(16);

    SELECT @vCDODefId = CDODefID 
    FROM CDODefinition
    WHERE CDOName = @pCDOType;

    SET @vLoopBackNewInstanceID = N'EXEC [HPECSILOOPBACK].[' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].' 
    + N'csiPRDGetBatchNextInstanceIds @p_CDODefId = @vCDODefId, @Amt = @v_Amt, @p_InstanceIdStr = @vLastInstanceIdStr OUTPUT';

    EXEC sp_executesql @vLoopBackNewInstanceID, N'@vCDODefId INT, @v_Amt INT, @vLastInstanceIdStr VARCHAR(16) OUTPUT', @vCDODefId, @v_Amt, @vLastInstanceIdStr OUTPUT;
    
    EXEC csiUpdateBatchInstanceID @vCDODefId, @v_Amt, @vLastInstanceIdStr, @vInstanceIdStr OUTPUT;
END;
GO



--------------------------------------------------------------------------------
--csiUpdateBatchInstanceID
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [csiUpdateBatchInstanceID]  
    @CDODefID         INT,
    @Amt              INT,
    @InstIdLastValue  VARCHAR(16),
    @InstanceIdStr    VARCHAR(max) OUTPUT
AS
BEGIN
    DECLARE 
        @ind INT = 0,
        @InstIdCurValue CHAR(16),
        @v_InstIdNewValue CHAR(16),
        @v_IdCount INT,
        @v_HexSite VARCHAR(16),
        @v_HexId VARCHAR(16),
        @v_Query NVARCHAR(100),
        @i_InstIdInt BIGINT,
        @v_CDODefIdStr VARCHAR(16);
    SET NOCOUNT ON

    SET @InstanceIdStr = '';
    SET @v_IdCount = 0;
    SET @v_IdCount = (SELECT count(*) from DBIdentifier);
    IF(@v_IdCount != 1 )
        SET @v_HexSite = '0x0';
    ELSE
        SET @v_HexSite = '0x'+(SELECT dbidentifier FROM DBIdentifier);
            
    IF(@v_HexSite is null)
        SET @v_HexSite = '0x0';

    SET @InstIdCurValue = @InstIdLastValue;

    SET @v_InstIdNewValue = @InstIdCurValue;
    SET @v_InstIdNewValue = SUBSTRING(@v_InstIdNewValue,7,10); --ToDo: merge

    SET @v_HexId = '0x'+ @v_InstIdNewValue;
    SET @v_Query = N'Select @Result = CONVERT(BIGINT,'+@v_HexId+') | CONVERT(BIGINT,'+@v_HexSite+')';
    EXEC sp_executesql @v_Query, N'@Result bigint output', @i_InstIdInt OUTPUT; --ToDo: remove dynamic

    SET @v_InstIdNewValue = REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@i_InstIdInt), 1, 2, ''), '0', ' ')), ' ', '0');
    SET @v_InstIdNewValue = REPLICATE('0', (10 - LEN(@v_InstIdNewValue))) + @v_InstIdNewValue;

    SET @v_CDODefIdStr = REPLACE(LTRIM(REPLACE(STUFF(master.sys.fn_varbintohexstr(@CDODefID), 1, 2, ''), '0', ' ')), ' ', '0');
    SET @v_CDODefIdStr = REPLICATE('0', (6 - LEN(@v_CDODefIdStr))) + @v_CDODefIdStr;
    
    SET @InstIdCurValue = @v_CDODefIdStr + @v_InstIdNewValue;

    DECLARE @Results TABLE ( id VARCHAR(16) ); 

    SET @Amt = @Amt * (-1);
    EXECUTE csiIncrementString64 @InstIdCurValue, @Amt, @InstIdCurValue OUTPUT; --
    SET @Amt = @Amt * (-1);

    WHILE(@ind < @Amt)
    BEGIN
        SET @ind += 1;
        EXECUTE csiIncrementString64 @InstIdCurValue, 1, @InstIdCurValue OUTPUT;
        INSERT INTO @Results VALUES (LOWER(@InstIdCurValue));
    END

    IF (@Amt = 0)
      INSERT INTO @Results VALUES ('');

    SELECT @InstanceIdStr = STRING_AGG(cast(id AS NVARCHAR(max) ), '|') FROM @Results;
END
GO


--------------------------------------------------------------------------------
--csiPRDGetBatchNextInstanceIds
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [csiPRDGetBatchNextInstanceIds]( @p_CDODefId INT, @Amt INT, @p_InstanceIdStr VARCHAR(16) OUTPUT )
AS
BEGIN
    BEGIN TRY
        --
        SET NOCOUNT ON;
        --
        DECLARE @n_ErrLocator       NUMERIC;
        DECLARE @v_ErrMsg           VARCHAR(1024);
        DECLARE @v_InstIdNewValues  VARCHAR(16);
        --
        -- Get next instance id and trim the leading 0's off so we can append the CDO Def hex string
        -- Length should be 10 chars
        --
        SET @n_ErrLocator = 5;

        EXEC csiUpdateInstanceID 0, @p_CDODefId, @Amt, @v_InstIdNewValues OUTPUT;
        SET @p_InstanceIdStr = @v_InstIdNewValues;
        
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


--------------------------------------------------------------------------------
--clfsqlGetQueryText
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfsqlGetQueryText (@SQLStatementName NVARCHAR(255), @vQueryText NVARCHAR(max) OUTPUT)
AS
BEGIN 
   SELECT @vQueryText = QueryText 
   FROM QueryText
   WHERE QueryDefID = (SELECT QueryDefID
                  FROM QueryDef
                  WHERE UPPER(Name) = UPPER(@SQLStatementName))
   AND DBTYPEID IN (0,1); --DBTYPEID : Generic (0) | SQLServer (1) | Oracle (2) | DB2 (3)
END;
GO

--------------------------------------------------------------------------------
--clfGetCLFParameter
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfGetCLFParameter (@ParameterName NVARCHAR(512), @vParmValue NVARCHAR(max) OUTPUT)
AS
BEGIN
DECLARE @sql NVARCHAR(1000), @param NVARCHAR(100);

IF(@ParameterName LIKE 'CLF::%')
    SET @ParameterName = SUBSTRING(@ParameterName, 6, LEN(@ParameterName))

IF(@ParameterName LIKE 'DBCLF::%')
    SET @ParameterName = SUBSTRING(@ParameterName, 8, LEN(@ParameterName))

SET @sql = N'SELECT @vParmValue = Value FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE Name = @ParameterName;';
EXEC sp_executesql @sql, @param = N'@vParmValue NVARCHAR(max) OUT, @ParameterName NVARCHAR(512)',
    @vParmValue = @vParmValue OUT, @ParameterName = @ParameterName;

IF(@vParmValue IS NULL) --if @ParameterName not found
    PRINT(N'Parameter ' + @ParameterName + N' not found or null');
END;
GO

--------------------------------------------------------------------------------
--clfSetCLFParameter
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfSetCLFParameter (@ParameterName NVARCHAR(255), @ParameterValue NVARCHAR(max))
AS
BEGIN
BEGIN TRY
DECLARE @sql NVARCHAR(1000), @param NVARCHAR(100);

SET @ParameterName = REPLACE(@ParameterName, 'DBCLF::', '')

SET @sql = N'DELETE FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE Name = @ParameterName;';
EXEC sp_executesql @sql, @param = N'@ParameterName NVARCHAR(255)', @ParameterName = @ParameterName;

SET @sql = N'INSERT INTO ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' VALUES (@ParameterName, @ParameterValue)';
EXEC sp_executesql @sql, @param = N'@ParameterName NVARCHAR(255), @ParameterValue NVARCHAR(max)',
    @ParameterName = @ParameterName, @ParameterValue = @ParameterValue;
END TRY
BEGIN CATCH
    PRINT('Error when insert data into ##CLFParameterCache');
    THROW
END CATCH
END;
GO

--------------------------------------------------------------------------------
--clfutilLogError
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilLogError(@pCLFPkg NVARCHAR(max), @pMsg NVARCHAR(4000))
AS
BEGIN
DECLARE @vLoopBackLogError NVARCHAR(MAX);

SET @vLoopBackLogError = N'EXEC [HPECSILOOPBACK].[' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].' 
+ N'logError @pCLFPkg = @pCLFPkg, @pMsg = @pMsg';

EXEC sp_executesql @vLoopBackLogError, N'@pCLFPkg NVARCHAR(max), @pMsg NVARCHAR(4000)', @pCLFPkg, @pMsg;
END;
GO

CREATE OR ALTER PROCEDURE logError(@pCLFPkg NVARCHAR(max), @pMsg NVARCHAR(4000))
AS
BEGIN
BEGIN TRY
    BEGIN TRAN
        -- The ClfErrorLog table's LogMessage column has a limited size (4000). Make sure we don't overrun it.
        INSERT INTO CLFErrorLog(LogDate, LogMessage, CLFPkg) VALUES (CURRENT_TIMESTAMP, SUBSTRING(@pMsg, 1, 4000), @pCLFPkg);
    COMMIT TRAN
END TRY
BEGIN CATCH
    PRINT('HPECSILOOPBACK error when insert data into CLFErrorLog');
    IF (XACT_STATE()) = -1
        ROLLBACK TRAN
    THROW
END CATCH
END;
GO

--------------------------------------------------------------------------------
--clfutilLogTrace
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilLogTrace(@pMsg NVARCHAR(max), @pTraceLevel INT)
AS
BEGIN
SET NOCOUNT ON
DECLARE
   @vConfigTraceLevel   NVARCHAR(16),
   @iConfigTraceLevel   INT,
   @vCLFID              NVARCHAR(255),
   @vLoopBacklogTrace   NVARCHAR(MAX),
   @sql                 NVARCHAR(1000),
   @param               NVARCHAR(100);

SET @sql = N'SELECT @vConfigTraceLevel = Value FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) +
    + ' WHERE Name = ''CURRENT_TRACELEVEL''';
EXEC sp_executesql @sql, @param = N'@vConfigTraceLevel NVARCHAR(16) OUT', @vConfigTraceLevel = @vConfigTraceLevel OUT;

IF(@vConfigTraceLevel IS NULL) --if CURRENT_TRACELEVEL not found
BEGIN
    SET @iConfigTraceLevel = 0;
    PRINT('CURRENT_TRACELEVEL not found or null');
END;
ELSE
    SET @iConfigTraceLevel = CAST(@vConfigTraceLevel AS INT);

SET @sql = N'SELECT @vCLFID = Value FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + 
    + ' WHERE Name = ''CURRENT_CLFID''';
EXEC sp_executesql @sql, @param = N'@vCLFID NVARCHAR(255) OUT', @vCLFID = @vCLFID OUT;

IF(@vCLFID IS NULL) --if CURRENT_CLFID not found
BEGIN
    SET @vCLFID = '';
    PRINT('CURRENT_CLFID not found or null');
END;

SET @vLoopBacklogTrace = N'EXEC [HPECSILOOPBACK].[' + DB_NAME() + N'].[' +  SCHEMA_NAME() + N'].' 
+ N'logTrace @pMsg = @pMsg, @vCLFID = @vCLFID';

IF(@iConfigTraceLevel >= @pTraceLevel)
    EXEC sp_executesql @vLoopBacklogTrace, N'@pMsg NVARCHAR(max), @vCLFID NVARCHAR(255)', @pMsg, @vCLFID;
END;
GO

CREATE OR ALTER PROCEDURE logTrace(@pMsg NVARCHAR(max), @vCLFID NVARCHAR(255))
AS
BEGIN
BEGIN TRY
    BEGIN TRAN
        INSERT INTO clfTraceLog(LogDate, CLFID, LogMessage) VALUES (CURRENT_TIMESTAMP, @vCLFID, @pMsg);
    COMMIT TRAN
END TRY
BEGIN CATCH
    PRINT('HPECSILOOPBACK error when insert data into clfTraceLog');
    IF (XACT_STATE()) = -1
        ROLLBACK TRAN
    THROW
END CATCH
END;
GO

--------------------------------------------------------------------------------
--clfsqlGetParameterValue
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfsqlGetParameterValue(@vDataObj NVARCHAR(max), @oParmName NVARCHAR(50), @oDefaultValue NVARCHAR(512), @ReturnParmValue NVARCHAR(512) OUTPUT)
AS
BEGIN
SET NOCOUNT ON
DECLARE
    @vParmArray NVARCHAR(max);
    SET @ReturnParmValue = @oDefaultValue;
    
    SET @vParmArray = JSON_QUERY(@vDataObj, N'$.Parameters');
    IF (@vParmArray IS NOT NULL)
    BEGIN
        SELECT @ReturnParmValue = Parms.ParmValue
        FROM OPENJSON (@vParmArray, N'$') 
        WITH ( 
            ParmName NVARCHAR(512) N'$.Name', 
            ParmValue NVARCHAR(512) N'$.Value') 
        AS Parms 
        WHERE Parms.ParmName = @oParmName;
    END;

    IF(@oParmName != 'Result' AND (@ReturnParmValue LIKE 'CLF::%' OR @ReturnParmValue LIKE 'DBCLF::%'))
        EXECUTE clfGetCLFParameter @ReturnParmValue, @ReturnParmValue OUTPUT;
END;
GO

--------------------------------------------------------------------------------
--clffuncFindClosingBracket
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncFindClosingBracket(@query NVARCHAR(max), @startInd INT, @ind INT, @oResult INT OUTPUT)
AS
BEGIN
SET NOCOUNT ON
DECLARE 
    @openingBracketCount INT = 0,
    @closingBracketCount INT = 0,
    @tempStr NVARCHAR(max),
    @tempStr2 NVARCHAR(max);

    SET @ind = CHARINDEX(')', @query, @ind + 1);

    SET @tempStr = SUBSTRING(@query, @startInd + 1, @ind - @startInd);
    SET @tempStr = REPLACE(@tempStr, ' ', '');
    SET @tempStr2 = REPLACE(@tempStr, '(', '');
    SET @openingBracketCount = LEN(@tempStr) - LEN(@tempStr2);

    SET @tempStr2 = REPLACE(@tempStr, ')', '');
    SET @closingBracketCount = LEN(@tempStr) - LEN(@tempStr2);

    IF(@closingBracketCount != @openingBracketCount)
        EXEC clffuncFindClosingBracket @query, @startInd, @ind, @oResult OUTPUT;
    ELSE
        SET @oResult = @ind;
END
GO

--------------------------------------------------------------------------------
--clffuncFindMainSelectFrom
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncFindMainSelectFrom(@query NVARCHAR(max), @curInd INT, @oResult INT OUTPUT, @wordToSearch NVARCHAR(16) = 'FROM')
AS
BEGIN
    DECLARE 
    @indComma INT,
    @indSelect INT,
    @indWith INT,
    @indClosingBracket INT;

    SET @indWith = CHARINDEX('WITH', @query);
    IF(@indWith = 0)
        SET @oResult = CHARINDEX('FROM', @query, @curInd);
    ELSE
    BEGIN
        EXEC clffuncFindClosingBracket @query, @curInd, @curInd, @indClosingBracket OUTPUT;
        SET @indComma = CHARINDEX(',', @query, @indClosingBracket);
        IF(@indComma = 0)
            SET @indComma = 999999;
        SET @indSelect = CHARINDEX('SELECT', @query, @indClosingBracket);

        IF(@indSelect < @indComma)
            SET @oResult = CHARINDEX(@wordToSearch, @query, @indClosingBracket);
        ELSE
        BEGIN
            SET @indClosingBracket += 1;
            EXEC clffuncFindMainSelectFrom @query, @indClosingBracket, @oResult OUTPUT, @wordToSearch;
        END
    END
END
GO



--------------------------------------------------------------------------------
--clfutilBuildMessageFromLabel
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfutilBuildMessageFromLabel(@pLabelName NVARCHAR(2000), @vTableName NVARCHAR(max), @vLblText NVARCHAR(2000) OUTPUT)
AS
BEGIN
SET NOCOUNT ON
DECLARE
    @vLabelID INT,
    @vDefaultLblText  VARCHAR(2000),
    @vPrimaryDictId   VARCHAR(16) = NULL,
    @vSecondaryDictId VARCHAR(16) = NULL,
    @ind INT = 1,
    @charInd INT = 0,
    @columnCount INT,
    @curColumn VARCHAR(256),
    @sp_Execute NVARCHAR(2000),
    @pValue NVARCHAR(256) = '',
    @pTypes NVARCHAR(256) = '',
    @sql NVARCHAR(256);
    
    BEGIN TRY
        SELECT @vLabelID = LabelID
        FROM Labels
        WHERE Name = @pLabelName;

        EXECUTE clfGetCLFParameter '__PrimaryDictionary', @vPrimaryDictId OUTPUT;
        EXECUTE clfGetCLFParameter '__SecondaryDictionary', @vSecondaryDictId OUTPUT;

       -- The order of dictionaries is:
       --   1) Primary Dictionary
       --   2) Secondary Dictionary
       --   3) Default Label

       SELECT @vDefaultLblText = LabelValue 
       FROM Labels
       WHERE LabelId = @vLabelID;

       IF (@vPrimaryDictId IS NOT NULL AND @vPrimaryDictId <> '')
       Begin
            SELECT @vLblText = LabelValue
            FROM DictionaryLabel
            WHERE DictionaryId = @vPrimaryDictId
            AND LabelId = @vLabelID;
            IF @@ROWCOUNT = 0
                SET @vLblText = NULL;
       END

       ELSE IF (@vSecondaryDictId IS NOT NULL AND @vSecondaryDictId <> '' AND @vLblText IS NULL)
        Begin
            SELECT @vLblText = LabelValue
            FROM DictionaryLabel
            WHERE DictionaryId = @vSecondaryDictId
            AND LabelId = @vLabelID;
            IF @@ROWCOUNT = 0
                SET @vLblText = NULL;
        END
    
       ELSE IF (@vLblText IS NULL)
            SET @vLblText = @vDefaultLblText;

        DECLARE @columns TABLE (id INT, columnName NVARCHAR(max));

        INSERT INTO @columns
        SELECT ROW_NUMBER() OVER(ORDER BY LEN(name) DESC) AS id, name
        FROM tempdb.sys.columns
        WHERE object_id = Object_id('tempdb..[' + @vTableName + ']');
    
        SET @columnCount = @@ROWCOUNT;

        SET @pTypes = N'@queryResultOut NVARCHAR(MAX) OUTPUT';

        WHILE @ind <= @columnCount
        BEGIN
            SELECT @curColumn = columnName
            FROM @columns
            WHERE id =  @ind;

            SET @sp_Execute = N'SELECT @queryResultOut = ' + @curColumn + ' FROM [' + @vTableName + ']';
            EXECUTE sp_executesql @sp_Execute, @pTypes, @queryResultOut = @pValue OUTPUT;
        
            SET @charInd = CHARINDEX('#ErrorMsg.' + @curColumn, @vLblText);
            IF @charInd > 0
                SET @vLblText = STUFF(@vLblText, @charInd, LEN('#ErrorMsg.' + @curColumn), @pValue);
            SET @ind += 1;
        END
    END TRY

    BEGIN CATCH
        THROW 51000, 'Error in CLF Function clfutilBuildMessageFromLabel', 16;
    END CATCH
END;
GO

--------------------------------------------------------------------------------
--clffuncExecuteSingleSQL
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncExecuteSingleSQL (@pFuncPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(2000) OUTPUT)
AS
BEGIN
SET NOCOUNT ON;
DECLARE
   @vDataObj       NVARCHAR(max),
   @vSQLQueryName  NVARCHAR(255),
   @errorMsg       NVARCHAR(max),
   @vErrLoc        INT,
   @spExecute      NVARCHAR(max),
   @vFuncName      NVARCHAR(80),
   @vRowsProcessed INT,
   @affectedRowsCount INT = 0;

   -- TODO
   -- * Error handling
   -- * Check JSON string to see if its well-formed
   -- * Return success/failure and possibly a JSON document
SET @vErrLoc = 1;
SET @oResponse = '';
SET @vFuncName = JSON_VALUE(@pFuncPkg,'$.Name');

IF (@vFuncName IS NULL)
BEGIN 
-- Something must be wrong with the JSON document
    SET @oResponse = 'Invalid JSON document.';
    SET @oResult = 0;
    RETURN;
END;

SET @vErrLoc = 10;
SET @vDataObj = @pFuncPkg;

SET @vErrLoc = 800;
BEGIN TRY
	DECLARE @SesId nvarchar(32), @sql NVARCHAR(1000);
	SET @SesId = CAST (@@spid AS NVARCHAR(32));
	EXEC PrepareSql @vDataObj, @SesId, @spExecute OUTPUT, @oResult OUTPUT, @oResponse OUTPUT;
	IF(@oResult = 0)
		THROW 51000, @oResponse, 16;
	
	EXECUTE (@spExecute);

    SET @vRowsProcessed = @@rowcount;
    EXECUTE clfGetCLFParameter 'affectedRowCount', @affectedRowsCount OUTPUT;
    IF @affectedRowsCount > 0
    BEGIN
        SET @vRowsProcessed = @affectedRowsCount;
        EXECUTE clfSetCLFParameter 'affectedRowCount', 0;
    END
    SET @oResponse = 'Rows processed: ' + CAST(@vRowsProcessed as nvarchar);
    EXEC clfutilLogTrace @oResponse, 1;
    SET @vErrLoc = 810;
    SET @oResult = 1;
END TRY
BEGIN CATCH
    SET @oResponse = 'Error in DBCLF Function ' + @vFuncName + ': ' + ERROR_MESSAGE();
    SET @oResult = 0;
END CATCH
END;
GO

--------------------------------------------------------------------------------
--clfExecute
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clfExecute (@pTxnId NVARCHAR(30), @pCLFPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(max) OUTPUT)
AS
   /*
   PROCEDURE: clfExecute
   DESCR: The main logic for parsing and executing DB CLF packages
   PARAMS: 
      pCLFPkg: A JSON document representing the CLF to be executed
      oResult: (out) 1=SUCCESS, 0=FAILURE
      oResponse: (out) Returned JSON document, but for now, just an error message.
   */
BEGIN 
SET NOCOUNT ON;
DECLARE 
   @vTxnId        NVARCHAR(30), 
   @vCLFName      NVARCHAR(128), 
   @vFuncJSON     NVARCHAR(max), 
   @vCLFParamPkg  NVARCHAR(max), 
   @vFuncArray    NVARCHAR(max),
   @vFuncResponse NVARCHAR(2000), 
   @vFuncName     NVARCHAR(80), 
   @vSQL          NVARCHAR(512), 
   @vFuncResult   INT, 
   @vErrLoc       INT, -- Used to identify location of errors   
   @vTraceLevel   INT,
   @sql           NVARCHAR(1000),
   @param         NVARCHAR(100);

   -- NOTES
   --    * oResponse must be limited in size because the calling application has to pre-allocate
   --      memory for the string. 
   --      Currently: 8000
   --    * For now, oResponse will simply be an error message (not JSON) 

SET @vErrLoc = 1;
SET @oResponse = '';

IF((SELECT COUNT(*) FROM tempdb.sys.tables WHERE Name = ('##CLFParameterCache_' + CAST(@@spid AS nvarchar))) = 0)
    EXEC ('CREATE TABLE ##CLFParameterCache_' + @@spid + ' (Name NVARCHAR(50), Value NVARCHAR(max))')

IF (ISJSON(@pCLFPkg) <= 0) 
   BEGIN 
   -- Something must be wrong with the JSON document
      SET @oResponse = 'Invalid JSON document.';
      SET @oResult = 0; 
      EXEC clfutilLogError @pCLFPkg, @oResponse;
      EXEC clfutilLogTrace @oResponse, 1;
      EXEC clfutilLogTrace 'END', 1;
      RETURN; 
   END; 

SET @sql = N'DELETE FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE NAME = ''TXNID''';
EXEC sp_executesql @sql;
SET @sql = N'INSERT INTO ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' VALUES (''TXNID'', @pTxnId)';
EXEC sp_executesql @sql, @param = N'@pTxnId NVARCHAR(30)', @pTxnId = @pTxnId;

-- Iterate through the CLF-level parameters to get certain values
SET @vErrLoc = 2; 
SET @vCLFName = 'Unknown CLF'; 
SET @vCLFParamPkg = JSON_QUERY(@pCLFPkg, N'$.CLFParameters');

SET @sql = N'DELETE FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + 
' WHERE NAME IN (SELECT ParmName FROM OPENJSON (@vCLFParamPkg, N''$'')' +
' WITH (ParmName NVARCHAR(50) N''$.Name''))';
EXEC sp_executesql @sql, @param = N'@vCLFParamPkg NVARCHAR(max)', @vCLFParamPkg = @vCLFParamPkg;
SET @sql = N'INSERT INTO ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' SELECT * FROM OPENJSON (@vCLFParamPkg, N''$'')' +
    + 'WITH (ParmName NVARCHAR(50) N''$.Name'', ParamValue NVARCHAR(max) N''$.Value'')';
EXEC sp_executesql @sql, @param = N'@vCLFParamPkg NVARCHAR(max)', @vCLFParamPkg = @vCLFParamPkg;
 
SELECT @vCLFName = Params.ParamValue 
FROM OPENJSON (@vCLFParamPkg, N'$') 
WITH ( 
   ParmName NVARCHAR(50) N'$.Name', 
   ParamValue NVARCHAR(max) N'$.Value') 
AS Params 
WHERE Params.ParmName = '__MethodName';

BEGIN TRY 
   SELECT @vTraceLevel = CAST (TValue AS INT) 
   FROM InsiteSiteInfo 
   WHERE UPPER(TName) = 'DBCLFTRACELEVEL' 
END TRY 
BEGIN CATCH 
   SET @vTraceLevel = 0; 
END CATCH;

SET @sql = N'DELETE FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE NAME = ''CURRENT_CLFID''';
EXEC sp_executesql @sql;
SET @sql = N'INSERT INTO ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' VALUES (''CURRENT_CLFID'',@pTxnId)';
EXEC sp_executesql @sql, @param = N'@pTxnId NVARCHAR(30)', @pTxnId = @pTxnId;

SET @sql = N'DELETE FROM ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' WHERE NAME = ''CURRENT_TRACELEVEL''';
EXEC sp_executesql @sql;
SET @sql = N'INSERT INTO ##CLFParameterCache_' + CAST(@@spid AS nvarchar) + ' VALUES(''CURRENT_TRACELEVEL'',@vTraceLevel)';
EXEC sp_executesql @sql, @param = N'@vTraceLevel INT', @vTraceLevel = @vTraceLevel;

EXEC clfutilLogTrace 'BEGIN', 1;
EXEC clfutilLogTrace @pCLFPkg, 2;

SET @vErrLoc = 5; 
SET @vFuncArray = JSON_QUERY(@pCLFPkg, N'$.Functions'); 
IF (@vFuncArray IS NULL) 
   BEGIN 
   -- Something must be wrong with the JSON document
      SET @oResponse = 'Error in CLF ' + @vCLFName + ': Invalid JSON document.'; 
      SET @oResult = 0; 
      EXEC clfutilLogError @pCLFPkg, @oResponse;
      EXEC clfutilLogTrace @oResponse, 1;
      EXEC clfutilLogTrace 'END', 1;
      RETURN; 
   END; 
 
   -- Iterate through all the Functions within the document. Each one should identify
   -- its corresponding stored procedure (eg ExecuteSingleSQL, etc.) along with 
   -- Parameter values to be passed to that procedure.
   --
   -- All of the "child" procedures MUST have the same signature, namely:
   -- PROCEDURE clffunc<FunctionName>(pJSONPkg IN CLOB, oResult OUT NUMBER, oResponse OUT VARCHAR2)
   -- This way, we can add Functions just by adding them to Designer (metadata) and creating a
   -- corresponding stored procedure.
 
SET @vErrLoc = 100; 
SET @vFuncJSON = JSON_QUERY(@pCLFPkg,'$.Functions[0]');

DECLARE @indx INT;
SET @indx = 0;

BEGIN TRY 
WHILE (@vFuncJSON IS NOT NULL)
   BEGIN 
      SET @vErrLoc += 1; 
      SET @vFuncJSON = JSON_QUERY(@pCLFPkg,'$.Functions[' + CAST(@indx AS nvarchar) + ']');
      IF(@vFuncJSON IS NULL)
         BREAK;
      SET @indx += 1; 
      SET @vFuncName = JSON_VALUE(@vFuncJSON,'$.Name');

      -- Map from vFuncName to the actual stored proc name.
      SET @vSQL = N'clffunc' + @vFuncName + N' @vFuncJSON, @vFuncResult OUTPUT, @vFuncResponse OUTPUT';
      EXECUTE sp_executesql @vSQL, N'@vFuncJSON NVARCHAR(max), @vFuncResult INT OUTPUT, @vFuncResponse NVARCHAR(2000) OUTPUT',
                            @vFuncJSON, @vFuncResult OUTPUT, @vFuncResponse OUTPUT;
      IF(@vFuncResult = 0)
      BEGIN
        SET @oResponse += @vFuncResponse;
        SET @oResult = 0;
        EXEC clfutilLogError @pCLFPkg, @oResponse;
        EXEC clfutilLogTrace @oResponse, 1;
        EXEC clfutilLogTrace 'END', 1;
        RETURN;
      END;
   END;
   -- TODO: Drop ##tempTable
   SET @oResult = 1;
   SET @oResponse = '';
   EXEC clfutilLogTrace 'END', 1;
END TRY 
BEGIN CATCH 
   SET @oResponse = 'Error in DBCLF ' + @vCLFName + ': ErrLoc: '+ CAST(@vErrLoc AS nvarchar) + 
   + ' Function: ' + @vFuncName +': ' + ERROR_MESSAGE(); 
   SET @oResult = 0;
   EXEC clfutilLogError @pCLFPkg, @oResponse;
   EXEC clfutilLogTrace @oResponse, 1;
   EXEC clfutilLogTrace 'END', 1;
END CATCH
END;
GO

--------------------------------------------------------------------------------
--clffuncDBGenerateInstanceIDs
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncDBGenerateInstanceIDs (@pFuncPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(2000) OUTPUT) 
AS
BEGIN
SET NOCOUNT ON;
DECLARE
   @vDataObj        NVARCHAR(max),
   @vFuncName       NVARCHAR(255),
   @vParmArray      NVARCHAR(max),
   @vParmObj        NVARCHAR(max),
   @vParmName       NVARCHAR(512),
   @vParmValue      NVARCHAR(max),
   @vCDOTypeName    NVARCHAR(255),
   @vResultVar      NVARCHAR(255),
   @errorMsg        NVARCHAR(max),
   @vIID            VARCHAR(max),
   @vNumInstanceIDs NVARCHAR(512),
   @iNumInstanceIDs INT,
   @isFirst         BIT,
   @vErrLoc         INT;

	SET @vErrLoc = 1;
    SET @oResponse = '';

    SET @vDataObj = @pFuncPkg;
    SET @vFuncName = JSON_VALUE(@vDataObj, N'$.Name');

    IF (@vFuncName IS NULL)
    BEGIN
        -- Something must be wrong with the JSON document
        SET @oResponse = 'Invalid JSON document.';
        SET @oResult = 0;
        RETURN;
    END;
	
	BEGIN TRY
		DECLARE @spId nvarchar(max);
		SET @spId = cast(@@SPID as nvarchar);
		EXEC DBGenerateInstanceIDs @pFuncPkg, @spId, @oResult OUTPUT, @oResponse OUTPUT;
		IF(@oResult = 0)
		BEGIN
			RETURN;
		END
	END TRY
    BEGIN CATCH
        SET @oResponse = 'Error in CLF Function ' + @vFuncName + ', ErrLoc: '+ CAST(@vErrLoc AS nvarchar) + ': ' + ERROR_MESSAGE();
        SET @oResult = 0;
    END CATCH;
END;
GO

--------------------------------------------------------------------------------
--clffuncDBGenerateSequence
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncDBGenerateSequence (@pFuncPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(2000) OUTPUT) 
AS
BEGIN
SET NOCOUNT ON;
DECLARE
    @vDataObj       NVARCHAR(max),
    @vFuncName      NVARCHAR(255),
    @vParmValue     NVARCHAR(max),
    @vErrorMsg      NVARCHAR(max),
    @vResultVar     NVARCHAR(255),
    @vStartNum      NVARCHAR(512),
    @vEndNum        NVARCHAR(512),
    @vStepSize      NVARCHAR(512),
    @vDelim         NVARCHAR(1),
    @iStartNum      INT,
    @iEndNum        INT,
    @iStepSize      INT,
    @iNum           INT,
    @vErrLoc        INT,
    @isFirst        BIT;

    SET @vErrLoc = 1;
    SET @oResponse = '';
    SET @vDelim = '|';

    SET @vDataObj = @pFuncPkg;
    SET @vFuncName = JSON_VALUE(@vDataObj, N'$.Name');

    IF (@vFuncName IS NULL)
    BEGIN
        -- Something must be wrong with the JSON document
        SET @oResponse = 'Invalid JSON document.';
        SET @oResult = 0;
        RETURN;
    END;

    SET @vErrLoc = 10;

    EXECUTE clfsqlGetParameterValue @vDataObj, N'StartNumber', N'', @vStartNum OUTPUT;
    IF(DATALENGTH(@vStartNum) > 0)
        SET @iStartNum = CAST(@vStartNum AS INT);

    EXECUTE clfsqlGetParameterValue @vDataObj, N'EndNumber', N'', @vEndNum OUTPUT;
    IF(DATALENGTH(@vStartNum) > 0)
        SET @iEndNum = CAST(@vEndNum AS INT);

    EXECUTE clfsqlGetParameterValue @vDataObj, N'StepSize', N'1', @vStepSize OUTPUT;
    IF(DATALENGTH(@vStepSize) > 0)
        SET @iStepSize = CAST(@vStepSize AS INT);

    EXECUTE clfsqlGetParameterValue @vDataObj, N'Result', N'', @vResultVar OUTPUT;

    SET @vErrLoc = 15;

    SET @iNum = @iStartNum;
    SET @vParmValue = '';
    BEGIN TRY
        WHILE(@iNum <= @iEndNum)
            BEGIN
                SET @vErrLoc += 1;
                IF(@isFirst != 1)
                    SET @vParmValue += @vDelim;
                SET @vParmValue += CAST(@iNum AS nvarchar);
                SET @iNum += @iStepSize;
                SET @isFirst = 0;
            END;
        EXEC clfSetCLFParameter @vResultVar, @vParmValue;
        SET @oResult = 1;
    END TRY
    BEGIN CATCH
        SET @oResponse = 'Error in CLF Function ' + @vFuncName + ', ErrLoc: '+ CAST(@vErrLoc AS nvarchar) + ': ' + ERROR_MESSAGE();
        SET @oResult = 0;
    END CATCH
END;
GO

--------------------------------------------------------------------------------
--clffuncDBValidateNotExists
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncDBValidateNotExists(@pFuncPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(2000) OUTPUT)
AS
BEGIN
SET NOCOUNT ON
DECLARE
    -- This validation parses and executes the given query. If any rows are returned, the
    -- validation fails. No data is actually fetched; the validation simply looks at the
    -- number of rows 
    --
    @vDataObj          NVARCHAR(max),
    @vFuncName         NVARCHAR(255),
    @vSQLQueryName     NVARCHAR(255),
    @vRowsProcessed    INT,
    @vErrLoc           INT,
    @vReturnErrorMsg   NVARCHAR(512),
    @errorMsg          NVARCHAR(2000),
    @spExecute         NVARCHAR(max),
    @spExecuteInTable  NVARCHAR(max) = '',
    @spExecuteDropTbl  NVARCHAR(255), 
    @labelName         NVARCHAR(2000),
    @myid              uniqueidentifier,
    @resTableName      NVARCHAR(max),
    @charInd           INT = 0,
    @asInd             INT,
    @withoutSpaces           NVARCHAR(max),
    @withoutPatern          NVARCHAR(max),
    @queriesCount      INT,
    @withCount         INT,
    @withInd           INT = 0,
    @ind               INT = 0,
    @indFrom           INT = 0,
    @affectedRowsCount INT = 0;

    SET @vErrLoc = 1;
    SET @oResponse = '';
    SET @vDataObj = @pFuncPkg;
    SET @vFuncName = JSON_VALUE(@vDataObj, N'$.Name');

    IF (@vFuncName IS NULL)
    BEGIN 
        -- Something must be wrong with the JSON document
        SET @oResponse = 'Invalid JSON document ' + @vFuncName + '.';
        SET @oResult = 0;
        RETURN;
    END;
    SET @vErrLoc = 10;
    EXECUTE clfsqlGetParameterValue @vDataObj, N'ErrorMessage', N'Validation failed', @vReturnErrorMsg OUTPUT;
    SET @vErrLoc = 800;
    BEGIN TRY
		DECLARE @SesId nvarchar(32), @sql NVARCHAR(1000);
		SET @SesId = CAST (@@spid AS NVARCHAR(32));
		EXEC PrepareSql @vDataObj, @SesId, @spExecute OUTPUT, @oResult OUTPUT, @oResponse OUTPUT;
		IF(@oResult = 0)
		BEGIN
			RETURN;
		END

        SET @vSQLQueryName = @oResponse;
        SET @withoutSpaces = RTRIM(@spExecute);
        SET @withoutPatern = REPLACE(@withoutSpaces, '##CLFParameterCache', '');
        SET @queriesCount = ( LEN(@withoutSpaces) - LEN(@withoutPatern) ) / LEN('##CLFParameterCache');

        IF(@queriesCount = 0)
            SET @queriesCount = 1;
        -- Find the 'WITH' section
        SET @withoutPatern = REPLACE(@withoutSpaces, 'WITH ', '');
        SET @withCount = ( LEN(@withoutSpaces) - LEN(@withoutPatern) ) / LEN('WITH ');

        EXECUTE clfsqlGetParameterValue @vDataObj, N'ErrorID', N'Validation failed', @labelName OUTPUT;
        -- Handling Variables Inside a Label Text
        IF (@labelName IS NOT NULL AND @labelName <> '')
        BEGIN
            SET @myid = NEWID();
            SET @resTableName = '##TempResTable_' + CONVERT(varchar(255), @myid);

            SET @spExecuteInTable = @spExecute;

            IF(@withCount = @queriesCount OR @withCount = 0)
            BEGIN
                WHILE(@ind < @queriesCount)
                BEGIN
                    IF(@ind = 0)
                    BEGIN
                        IF(@withCount != 0)
                        BEGIN
                            SET @withInd = CHARINDEX('WITH ', @spExecuteInTable, @withInd);
                            SET @asInd = CHARINDEX('AS', @spExecuteInTable, @withInd);
                
                            EXEC clffuncFindMainSelectFrom @spExecuteInTable, @asInd, @indFrom OUTPUT;
                            SET @withInd += 1;
                        END
                        ELSE
                            EXEC clffuncFindMainSelectFrom @spExecuteInTable, @indFrom, @indFrom OUTPUT;
                    
                        SET @spExecuteInTable = STUFF(@spExecuteInTable, @indFrom, 0, 'INTO [' + @resTableName + '] ');

                        SET @indFrom += LEN('INTO [' + @resTableName + '] ') + LEN('FROM') + 1;
                    END
                    ELSE
                    BEGIN
                        IF(@withCount != 0)
                        BEGIN
                        -- Find the main SELECT After 'WITH'
                            SET @withInd = CHARINDEX('WITH ', @spExecuteInTable, @withInd);
                            SET @asInd = CHARINDEX('AS', @spExecuteInTable, @withInd);

                            EXEC clffuncFindMainSelectFrom @spExecuteInTable, @asInd, @indFrom OUTPUT, 'SELECT';
                            SET @withInd += 1;
                        END
                        ELSE
                            EXEC clffuncFindMainSelectFrom @spExecuteInTable, @indFrom, @indFrom OUTPUT, 'SELECT';
                    
                        SET @spExecuteInTable = STUFF(@spExecuteInTable, @indFrom, 0, 'INSERT INTO [' + @resTableName + '] ');

                        SET @indFrom += LEN('INSERT INTO [' + @resTableName + '] ') + LEN('FROM') + 1;
                    END
                    
                    SET @ind += 1;
                END
            END
			
			EXECUTE (@spExecuteInTable);
            SET @vRowsProcessed = @@rowcount;
            IF(@queriesCount > 1)
            BEGIN
                EXECUTE clfGetCLFParameter 'affectedRowCount', @affectedRowsCount OUTPUT;
                -- Find the matches
                SET @vRowsProcessed = @affectedRowsCount;
                EXECUTE clfSetCLFParameter 'affectedRowCount', 0;
            END

            EXECUTE clfutilBuildMessageFromLabel @labelName, @resTableName, @errorMsg OUTPUT;
            
            SET @spExecuteDropTbl = 'IF OBJECT_ID(N''tempdb..[' + @resTableName + ']'') IS NOT NULL 
                DROP TABLE [' + @resTableName + ']';
            EXECUTE sp_executesql @spExecuteDropTbl;

            SET @vReturnErrorMsg = @errorMsg;
        END
        ELSE
        BEGIN
			EXECUTE (@spExecute);
            SET @vRowsProcessed = @@rowcount;
            IF(@queriesCount > 1)
            BEGIN
                -- Find the matches
                EXECUTE clfGetCLFParameter 'affectedRowCount', @affectedRowsCount OUTPUT;
                SET @vRowsProcessed = @affectedRowsCount;
				EXECUTE clfSetCLFParameter 'affectedRowCount', 0;
            END
        END

        SET @oResponse = 'Rows processed: ' + CAST(@vRowsProcessed as nvarchar);
        EXEC clfutilLogTrace @oResponse, 1;
        IF(@vRowsProcessed != 0)
        BEGIN
            SET @oResponse = @vReturnErrorMsg;
            SET @oResult = 0;
            RETURN;
        END;
        SET @oResult = 1;
    END TRY
    BEGIN CATCH
        SET @oResponse = 'Error in CLF Function ' + @vFuncName + ': SQLStatementName: ' + @vSQLQueryName +
        + ', ErrLoc: ' + CAST(@vErrLoc AS nvarchar) + ': ' + ERROR_MESSAGE();
        SET @oResult = 0;
    END CATCH
END;
GO

--------------------------------------------------------------------------------
--clffuncExecuteAndFetchSQL
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE clffuncExecuteAndFetchSQL (@pFuncPkg NVARCHAR(max), @oResult INT OUTPUT, @oResponse NVARCHAR(2000) OUTPUT)
AS
BEGIN
SET NOCOUNT ON;
    -- This function executes the given SQL statement and fetches the results from the
    -- first column in the resultset to build a DBCLF:: parameter.
DECLARE
    @vDataObj          NVARCHAR(MAX),
    @vFuncName         NVARCHAR(255),
    @vResultVar        NVARCHAR(512),
    @spExecute         NVARCHAR(MAX),
    @firstArgument     NVARCHAR(MAX),
    @selectPos         INT,
    @fromPos           INT,
    @vRowsProcessed    INT = 0,
    @vErrLoc           INT,
    @curRes            NVARCHAR(MAX),
    @res               NVARCHAR(MAX) = '';

SET @vErrLoc = 1;
SET @oResponse = '';
SET @vFuncName = JSON_VALUE(@pFuncPkg,'$.Name');

IF (@vFuncName IS NULL)
BEGIN 
-- Something must be wrong with the JSON document
    SET @oResponse = 'Invalid JSON document.';
    SET @oResult = 0;
    RETURN;
END;

SET @vErrLoc = 10;
SET @vDataObj = @pFuncPkg;
EXECUTE clfsqlGetParameterValue @vDataObj, N'Result', N'', @vResultVar OUTPUT;

BEGIN TRY
    SET @vErrLoc = 100;
	DECLARE @SesId nvarchar(32), @sql NVARCHAR(1000);
	SET @SesId = CAST (@@spid AS NVARCHAR(32));
	EXEC PrepareSql @vDataObj, @SesId, @spExecute OUTPUT, @oResult OUTPUT, @oResponse OUTPUT;
	IF(@oResult = 0)
		THROW 51000, @oResponse, 16;

    SET @vErrLoc = 200;
	SET @spExecute = STUFF(@spExecute, CHARINDEX('SELECT', @spExecute), 0, 'DECLARE result_cursor CURSOR FOR ');
	SET @vErrLoc = 300;
    IF CURSOR_STATUS('global','result_cursor')>=-1
        DEALLOCATE result_cursor;
	EXECUTE(@spExecute);
    OPEN result_cursor;
    FETCH NEXT FROM result_cursor
    INTO @curRes;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @vRowsProcessed += 1;
        SET @res += '|' + @curRes;
        FETCH NEXT FROM result_cursor
        INTO @curRes;
    END
	
    IF CURSOR_STATUS('global','result_cursor')>=-1
        DEALLOCATE result_cursor;
    SET @res = STUFF(@res, 1, 1, null);

    SET @oResponse = 'ExecuteAndFetchSQL (' + CAST(@vRowsProcessed as nvarchar) + ' rows)';
    SET @vErrLoc = 400;
    EXECUTE clfSetCLFParameter @vResultVar, @res;
    EXECUTE clfutilLogTrace @oResponse, 2;
    SET @vErrLoc = 510;
    SET @oResult = 1;
END TRY
BEGIN CATCH
    SET @oResponse = 'Error in DBCLF Function ' + @vFuncName + ': ErrLoc: '+ CAST(@vErrLoc AS nvarchar) + ': ' + ERROR_MESSAGE();
    SET @oResult = 0;
END CATCH
END;
GO

CREATE OR ALTER FUNCTION clfutilConvertToEmptyString (@vItem VARCHAR(max)) RETURNS VARCHAR(max)
BEGIN
   -- this function replaces any instance of 0000000000000000 with a blank string.
   -- it can be modified later on if needed to detect and replace a wider range if search values
DECLARE   
    @vModified VARCHAR(max); 
SET
    @vModified = REPLACE(@vItem, '0000000000000000', '');
    RETURN @vModified;    
END;
GO
