@echo off
REM setlocal
REM  InstallDataStoreProcedures.bat
REM  Simple batch file used install/create the database stored procedures
REM  required for the DataStore application.
REM
REM  HISTORY
REM     5/25/2005 - Changed DB used for GenerateDataStoreServerUpdate.sql
REM    10/06/2005 - Removed SP_PATH from DB2 INSTALL_JAR call, used ~ to strip 
REM                 double-quotes from parameters as needed.
REM    10/29/2005 - Fixed above comment, added ~ to SP_PATH
REM    11/09/2005 - Removed ~ from SP_PATH since it may have spaces and will cause
REM                 a failure in the null check.
REM    12/02/2005 - Added calls to UpdateDataStoreTables.sql.
REM    12/28/2005 - Removed CreateDataStoreInsertUpdateTables.sql from SQL Server ODS scripts.
REM    03/01/2006 - Changed SQL Server job to use -D instead of -d since database name may be different 
REM                 than the DSN name.
REM    04/03/2006 - Cleaned up usage
REM    04/12/2006 - Added DS_ACTUAL_DBNAME for SQL Server
REM    06/20/2006 - Added pause on error
REM    04/23/2007 - Updated copyright notice (SPR S9984) - Bill Lippard
REM    05/16/2007 - Added error check after call to CreateDataStoreJobs.sql
REM                 (SPR 11593) - Bill Lippard
REM    07/03/2007 - Added SQL Server synonym code
REM    05/10/2008 - Modified the code for SPR S12618
REM                 - Included new logic to handle input arguments and validations
REM                 - Included a new input parameter: DB_SERVERNAME
REM                   This parameter will be used only for SQL SERVER
REM                 - Replaced SQL Server's OSQL commands with SQLCMD
REM    07/15/2008 - Added new logic to capture more than 10 input arguments
REM		  - Modified the code to pass control to respective database sections
REM
REM   Copyright Siemens 2023  

TIME /t
SET ERRORSTATUS=0
SET INSITE_HOME=%CD%

REM Set local variables
REM -------------------
:SETVARIABLES
SET DB_TYPE=%1
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (HELP) GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETSQLSERVER
IF /I (%DB_TYPE%) EQU (DB2) GOTO SETDB2
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DS_DBNAME=%~2
SET DS_SCHEMA=%~3
SET DS_PASSWD=%~4
SET SP_PATH=%5
SET TX_DBNAME=%~6
SET TX_SCHEMA=%~7
SET TX_PASSWD=%~8
SET DS_ACTUAL_DBNAME=%~9
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DS_DBSERVERNAME=%~2
SET DS_DBNAME=%~3
SET DS_SCHEMA=%~4
SET DS_PASSWD=%~5
SET SP_PATH=%6
SET TX_DBSERVERNAME=%~7
SET TX_DBNAME=%~8
SET TX_SCHEMA=%~9
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SET TX_PASSWD=%~1
SET DS_ACTUAL_DBNAME=%~2
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set DB2 related data
REM -----------------------
:SETDB2
SET DS_DBNAME=%~2
SET DS_SCHEMA=%~3
SET DS_PASSWD=%~4
SET SP_PATH=%5
SET TX_DBNAME=%~6
SET TX_SCHEMA=%~7
SET TX_PASSWD=%~8
SET DS_ACTUAL_DBNAME=%~9
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
SET INSITE_HOME=%CD%
IF (%SP_PATH%)==() GOTO USAGE
IF (%TX_DBNAME%)==() GOTO USAGE
IF (%DS_DBNAME%)==() GOTO USAGE
IF (%TX_SCHEMA%)==() GOTO USAGE
IF (%TX_PASSWD%)==() GOTO USAGE
IF (%DS_SCHEMA%)==() SET DS_SCHEMA=%TX_SCHEMA%
IF (%DS_PASSWD%)==() SET DS_PASSWD=%TX_PASSWD%
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
IF /I (%DB_TYPE%) EQU (DB2) GOTO DB2
GOTO USAGE
:END



:usage
echo ------------------------------------------------------------------------------
echo "Usage:  InstallDataStoreProcedures <Oracle/SqlServer/DB2> <ODS Database Host> <ODS Database Name> <ODS user> <ODS password> <base dir> <OLTP Database Host> <OLTP Database Name> <OLTP user> <OLTP password> <ODS database name>" 
echo   Where "<base dir>" is the InSite Server scripts directory 
echo              example "c:\program files\camstar\insite datastore\scripts"
echo              example "." To use the current directory
echo.
echo.
echo          "ODS database name"  - Native database name/alias
echo.
echo          "OLTP database name" - Native database name/alias
echo.
echo          "ODS database name" - SQL Server only: The name of the ODS database
echo.
echo ------------------------------------------------------------------------------
goto exit
:end

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor             : %DB_TYPE%
ECHO DataStore Database Name     : %DS_DBNAME%
ECHO DataStore User Name         : %DS_SCHEMA%
ECHO Base Directory              : %SP_PATH%
ECHO OLTP Database Name          : %TX_DBNAME%
ECHO OLTP User Name              : %TX_SCHEMA%
ECHO DataStore Actual DB Name    : %DS_ACTUAL_DBNAME%
ECHO ----------------------------------------------------
cd "%SP_PATH%\oracle"
echo Verifying prerequisites in %DS_DBNAME% ...
echo WHENEVER SQLERROR EXIT FAILURE  > tempproc_%DS_DBNAME%.sql
echo SET SERVEROUTPUT ON >> tempproc_%DS_DBNAME%.sql
echo SET verify OFF >> tempproc_%DS_DBNAME%.sql
echo SET FEEDBACK OFF >> tempproc_%DS_DBNAME%.sql
echo connect %DS_SCHEMA%/%DS_PASSWD%@%DS_DBNAME% >> tempproc_%DS_DBNAME%.sql
echo start CreateDataStoreDBLink.sql "%TX_SCHEMA%" "%TX_PASSWD%" "%TX_DBNAME%" >>tempproc_%DS_DBNAME%.sql
echo @VerifyDataStorePrerequisites.sql >> tempproc_%DS_DBNAME%.sql
sqlplus -s /nolog @tempproc_%DS_DBNAME%.sql
IF ERRORLEVEL 1 set errorstatus=99
IF %errorstatus% NEQ 0 goto exit
echo Prerequisites okay.

echo Installing data store tables into OLTP database %TX_DBNAME% ...
echo WHENEVER SQLERROR EXIT FAILURE  > tempproc_%DS_DBNAME%.sql
echo SET SERVEROUTPUT ON >> tempproc_%DS_DBNAME%.sql
echo SET verify OFF >> tempproc_%DS_DBNAME%.sql
echo SET FEEDBACK OFF >> tempproc_%DS_DBNAME%.sql
echo connect %TX_SCHEMA%/%TX_PASSWD%@%TX_DBNAME% >> tempproc_%DS_DBNAME%.sql
echo @CreateDataStoreInsertUpdateTables.sql >> tempproc_%DS_DBNAME%.sql
echo exit >> tempproc_%DS_DBNAME%.sql
sqlplus -s /nolog @tempproc_%DS_DBNAME%.sql
IF ERRORLEVEL 1 set errorstatus=99
IF %errorstatus% NEQ 0 goto exit
echo OLTP installation okay.

echo Installing tables and procedures into data store database %DS_DBNAME% ...
echo WHENEVER SQLERROR EXIT FAILURE  > tempproc_%DS_DBNAME%.sql
echo SET SERVEROUTPUT ON >> tempproc_%DS_DBNAME%.sql
echo SET verify OFF >> tempproc_%DS_DBNAME%.sql
echo SET FEEDBACK OFF >> tempproc_%DS_DBNAME%.sql
echo connect %DS_SCHEMA%/%DS_PASSWD%@%DS_DBNAME% >> tempproc_%DS_DBNAME%.sql

echo @CreateDataStoreLocalTables.sql >> tempproc_%DS_DBNAME%.sql
ECHO @UpdateDataStoreTables.sql >>  tempproc_%DS_DBNAME%.sql
ECHO @csiDataStorePackage.sql >> tempproc_%DS_DBNAME%.sql
ECHO @CreateDataStoreJobs.sql >> tempproc_%DS_DBNAME%.sql
echo exit >> tempproc_%DS_DBNAME%.sql
sqlplus -s /nolog @tempproc_%DS_DBNAME%.sql
IF ERRORLEVEL 1 set errorstatus=99
IF %errorstatus% NEQ 0 goto exit
del tempproc_%DS_DBNAME%.sql
echo Data store installation complete.
goto exit
:end

:SQLSERVER
IF (%DS_ACTUAL_DBNAME%)==() GOTO usage
ECHO ----------------------------------------------------
ECHO Database Vendor             : %DB_TYPE%
ECHO DataStore Database Host     : %DS_DBSERVERNAME%
ECHO DataStore Database Name     : %DS_DBNAME%
ECHO DataStore User Name         : %DS_SCHEMA%
ECHO Base Directory              : %SP_PATH%
ECHO OLTP Database Host          : %TX_DBSERVERNAME%
ECHO OLTP Database Name          : %TX_DBNAME%
ECHO OLTP User Name              : %TX_SCHEMA%
ECHO DataStore Actual DB Name    : %DS_ACTUAL_DBNAME%
ECHO ----------------------------------------------------
cd "%SP_PATH%\sql server"
echo Installing data store tables into OLTP database %TX_DBNAME% ...
SQLCMD -S %TX_DBSERVERNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -i CreateDataStoreInsertUpdateTables.sql
IF ERRORLEVEL 1 set errorstatus=99
IF %errorstatus% NEQ 0 goto exit
echo OLTP installation okay.

echo Installing tables and procedures into data store database %DS_DBNAME% ...

SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i CreateDataStoreLocalTables.sql
IF ERRORLEVEL 1 (
	set errorstatus=90
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i UpdateDataStoreTables.sql
IF ERRORLEVEL 1 (
	set errorstatus=91
	goto exit
)
SQLCMD -S %TX_DBSERVERNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "SET NOCOUNT ON; begin declare @tmp nvarchar(255) PRINT N'SET NOCOUNT ON; INSERT INTO DATASTORESETUP(PARAMETER,VALUE) '; PRINT N'VALUES (''TempRemoteServer'','; SELECT @tmp=N'''['+CONVERT(NVARCHAR(50),SERVERPROPERTY('MachineName'))+N'].['+DB_NAME()+N'].['+TABLE_SCHEMA+N']'')' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='InSiteSiteInfo' print @tmp end">TmpSetupInsert.sql
IF ERRORLEVEL 1 (
	set errorstatus=80
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -Q "SET NOCOUNT ON; DELETE FROM DATASTORESETUP WHERE PARAMETER='TempRemoteServer'"
IF ERRORLEVEL 1 (
	set errorstatus=81
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i TmpSetupInsert.sql
IF ERRORLEVEL 1 (
	set errorstatus=82
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i CreateDataStoreSynonyms.sql
IF ERRORLEVEL 1 (
	set errorstatus=83
	goto exit
)
del TmpSetupInsert.sql


SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i csiDataStoreInit.sql
IF ERRORLEVEL 1 (
	set errorstatus=92
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i csiDataStoreParseAndExecute.sql
IF ERRORLEVEL 1 (
	set errorstatus=93
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i csiDataStoreReplicator.sql
IF ERRORLEVEL 1 (
	set errorstatus=94
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i csiDataStoreCleanup.sql
IF ERRORLEVEL 1 (
	set errorstatus=95
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -i csiDataStoreVerifyJob.sql
IF ERRORLEVEL 1 (
	set errorstatus=96
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -Q "SET NOCOUNT ON; DELETE FROM DATASTORESETUP WHERE PARAMETER='TempJobDescr'"
IF ERRORLEVEL 1 (
	set errorstatus=97
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -Q "SET NOCOUNT ON; INSERT INTO DATASTORESETUP(PARAMETER,VALUE) VALUES('TempJobDescr','SQLCMD -S %DS_DBSERVERNAME% -d %DS_ACTUAL_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -Q ')"
IF ERRORLEVEL 1 (
	set errorstatus=98
	goto exit
)
SQLCMD -S %DS_DBSERVERNAME% -d %DS_DBNAME% -U %DS_SCHEMA% -P %DS_PASSWD% -b -m 20 -i CreateDataStoreJobs.sql
IF ERRORLEVEL 1 (
	set errorstatus=99
	goto exit
)

IF %errorstatus% NEQ 0 goto exit
echo Data store installation complete.
goto exit
:end

:DB2
echo Installing procedures into DB2 database %DS_DBNAME% ...
cd "%SP_PATH%\DB2"

echo DB2 CONNECT TO %TX_DBNAME% USER %TX_SCHEMA% USING %TX_PASSWD% > tempproc_%DS_DBNAME%.cmd
echo DB2 -td@ -f datastoreinsertupdatetables.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 CONNECT RESET >> tempproc_%DS_DBNAME%.cmd
db2cmd /c /w /i tempproc_%DS_DBNAME%.cmd

echo DB2 CONNECT TO %DS_DBNAME% USER %DS_SCHEMA% USING %DS_PASSWD% > tempproc_%DS_DBNAME%.cmd
ECHO db2 DROP FUNCTION delay  >> tempproc_%DS_DBNAME%.cmd
ECHO db2 CALL SQLJ.REMOVE_JAR('delayUDFjar')  >> tempproc_%DS_DBNAME%.cmd
ECHO db2 CALL SQLJ.INSTALL_JAR('file:.\delayUDF.jar','delayUDFjar')  >> tempproc_%DS_DBNAME%.cmd
ECHO db2 CREATE FUNCTION delay(INTEGER) RETURNS INTEGER EXTERNAL NAME 'delayUDFjar:delayUDF.DELAY(INTEGER)' LANGUAGE JAVA PARAMETER STYLE JAVA NOT DETERMINISTIC NO SQL FENCED RETURNS NULL ON NULL INPUT EXTERNAL ACTION  >> tempproc_%DS_DBNAME%.cmd

echo DB2 -td@ -f datastoresetuptables.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 CONNECT RESET >> tempproc_%DS_DBNAME%.cmd
db2cmd /c /w /i tempproc_%DS_DBNAME%.cmd

REM Note this block has to be run using "db2 -td; -f" instead of directly from db2cmd
ECHO UPDATE DBM CFG USING FEDERATED YES; > tempproc_%DS_DBNAME%.cmd
ECHO db2stop force; >> tempproc_%DS_DBNAME%.cmd
ECHO db2start; >> tempproc_%DS_DBNAME%.cmd
ECHO CONNECT TO %DS_DBNAME% USER %DS_SCHEMA% USING %DS_PASSWD%;  >> tempproc_%DS_DBNAME%.cmd
ECHO CREATE WRAPPER "DRDA"; >> tempproc_%DS_DBNAME%.cmd
ECHO CREATE SERVER %TX_DBNAME% TYPE DB2/UDB VERSION 8.2 WRAPPER DRDA AUTHORIZATION "%TX_SCHEMA%" PASSWORD "%TX_PASSWD%" OPTIONS (DBNAME '%TX_DBNAME%');  >> tempproc_%DS_DBNAME%.cmd
ECHO CREATE USER MAPPING FOR %DS_SCHEMA% SERVER %TX_DBNAME% OPTIONS (REMOTE_AUTHID '%TX_SCHEMA%', REMOTE_PASSWORD '%TX_PASSWD%');  >> tempproc_%DS_DBNAME%.cmd
ECHO UPDATE datastoresetup SET value = '%TX_DBNAME%' WHERE parameter = 'TX_DBNAME'; >> tempproc_%DS_DBNAME%.cmd
ECHO UPDATE datastoresetup SET value = '%TX_SCHEMA%' WHERE parameter = 'TX_SCHEMA'; >> tempproc_%DS_DBNAME%.cmd
ECHO CONNECT RESET;  >> tempproc_%DS_DBNAME%.cmd
db2cmd /c /w /i db2 -td; -f tempproc_%DS_DBNAME%.cmd

echo DB2 CONNECT TO %DS_DBNAME% USER %DS_SCHEMA% USING %DS_PASSWD% > tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f datastorelocaltables.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f updatedatastoretables.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f csidatastorecleanup.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f csidatastorereplicator.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f csidatastoremonitor.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 -td@  -f csidatastoresession.sql >> tempproc_%DS_DBNAME%.cmd
echo DB2 CONNECT RESET >> tempproc_%DS_DBNAME%.cmd
db2cmd /c /w /i tempproc_%DS_DBNAME%.cmd

DEL tempproc_%DS_DBNAME%.cmd

:end


:exit
REM endlocal
if %errorstatus% NEQ 0 (
	echo Error Installing DataStore Procedures - %errorstatus%
	pause
)
:end

