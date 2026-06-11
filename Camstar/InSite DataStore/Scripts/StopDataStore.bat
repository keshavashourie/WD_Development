@ECHO OFF
REM --------------------------------------------------------------------------------------------------
REM setlocal
REM  StopDataStore.bat
REM  Simple batch file used to stop the database stored procedures
REM
REM Modification History:
REM Name			Date        	Action
REM -------------------------	----------	------------------------------------------------------
REM Purushotham Neelakantachar	07/15/2008	Modified the code for SPR S12618:
REM                 				- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						This parameter will be used only for SQL SERVER
REM						- Replaced SQL Server's OSQL commands with SQLCMD
REM
REM --------------------------------------------------------------------------------------------------
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------

TIME /t
SET ERRORSTATUS=0
SET INSITE_HOME=%CD%

REM Set local variables
REM -------------------
:SETVARIABLES
SET DB_TYPE=%1
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETSQLSERVER
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETDB2
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET BASE_DIR=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET BASE_DIR=%6
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set DB2 related data
REM -----------------------
:SETDB2
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET BASE_DIR=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
SET INSITE_HOME=%CD%
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%BASE_DIR%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
IF /I (%DB_TYPE%) EQU (DB2) GOTO DB2
GOTO USAGE
:END


:USAGE
ECHO ------------------------------------------------------------------------------
ECHO USAGE:  
ECHO   "StopDataStore <ORACLE|SQLSERVER|DB2> <ODS Database Host> <ODS Database Name> <ODS User> <ODS Password> <Base dir>"
ECHO  "<ORACLE|SQLSERVER>"      - Specify ODS database vendor
ECHO  "<ODS Database Host>"     - For ORACLE and DB2: Not Required
ECHO                              For SQL SERVER: ODS Database Host Name or IP Address - Required
ECHO  "<ODS Database Name>"     - Native ODS Database Name/Alias
ECHO  "<ODS Database User>"     - Insite ODS database user/logon
ECHO  "<ODS Database Password>" - Insite ODS database user password
ECHO  "<Base Dir>"              - Insite ODS database scripts directory. 
ECHO				  Ex: "c:\program files\camstar\insite datastore\scripts"
ECHO				      "." To use the current directory
ECHO  Examples: 
ECHO  StopDataStore ORACLE PROD insiteadmin admin "c:\program files\camstar\insite datastore\scripts"
ECHO  StopDataStore SQLSERVER DBSERVERNAME PROD insiteadmin admin .
ECHO  StopDataStore SQLSERVER 10.10.10.10 PROD insiteadmin admin .
ECHO ------------------------------------------------------------------------------
ECHO Copyright Siemens 2023  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:ORACLE
ECHO Stopping DataStore in Oracle database %DB_NAME% ...
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Base Directory             : %BASE_DIR%
ECHO ----------------------------------------------------
CD "%BASE_DIR%\oracle"
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempproc_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> tempproc_%DB_NAME%.sql
ECHO @StopDataStore.sql >> tempproc_%DB_NAME%.sql
ECHO EXIT >> tempproc_%DB_NAME%.sql
SQLPLUS /NOLOG @tempproc_%DB_NAME%.sql
IF ERRORLEVEL 1 SET ERRORSTATUS=99
DEL tempproc_%DB_NAME%.sql
GOTO EXIT
:END

:SQLSERVER
ECHO Stopping DataStore in SQL Server database %DB_NAME% ...
CD "%BASE_DIR%\sql server"
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i StopDataStore.sql
IF ERRORLEVEL 1 SET ERRORSTATUS=99
GOTO EXIT
:END


:DB2
ECHO Stopping DataStore in DB2 database %DB_NAME% ...
CD "%BASE_DIR%\DB2"
DB2CMD /c /w /i TerminateDataStore.bat %DB_NAME% %DB_USERNAME% %DB_PASSWORD% ALL
:END


:EXIT
REM endlocal
IF %ERRORSTATUS% NEQ 0 ECHO Error stopping datastore
REM IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%
:END

