@ECHO off
REM setlocal
REM --------------------------------------------------------------------------------------------------
REM
REM  © 2018 Siemens Product Lifecycle Management Software Inc.
REM
REM  mdPopulateCMUpdate.bat
REM  Simple batch file used to run the populatereleasedata scripts.
REM  Assumptions:
REM     - Run from the Camstar\InSite Administration\Scripts folder
REM     - The appropriate sql tool(ie sqlplus or sqlcmd) is in the users %PATH%
REM  Usage: 
REM    mdPopulateCMUpdate.bat <ORACLE/SQLSERVER> <Database> <user> <password
REM
REM Modification History:
REM Name			Date        	Action
REM --------------------------	----------	------------------------------------------------------
REM Brandon Craig	04/06/2017     Created to reload Portal menu from the Load Menu checkbox in Management studio
REM --------------------------------------------------------------------------------------------------


TIME /t
SET ERRORSTATUS=0

REM Set local variables
REM -------------------
:SETVARIABLES
SET DB_TYPE=%1
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETSQLSERVER
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
GOTO USAGE
:END

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name		: %DB_USERNAME%
ECHO ----------------------------------------------------
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempdata_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> tempdata_%DB_NAME%.sql
ECHO @Scripts\oracle\mdPopulatePortalMenuUpdateData.or.sql >> tempdata_%DB_NAME%.sql
ECHO EXIT >> tempdata_%DB_NAME%.sql
SQLPLUS /NOLOG @tempdata_%DB_NAME%.sql
IF ERRORLEVEL 1 SET ERRORSTATUS=99
DEL tempdata_%DB_NAME%.sql
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Server Name	: %DB_SERVERNAME%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO ----------------------------------------------------
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT SUBSTRING(DB_NAME(),1,50)"
IF ERRORLEVEL 1 SET ERRORSTATUS=99
IF %ERRORSTATUS% NEQ 0 GOTO EXIT
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i "C:\Program Files (x86)\Camstar\InSite Administration\Scripts\Sql Server\mdPopulatePortalMenuUpdateData.sql"
IISRESET.exe /RESTART
IF ERRORLEVEL 1 set errorstatus=99
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "mdReloadPortalMenu.bat <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"	 	 - For ORACLE and DB2: Not Required
ECHO				   For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database | Datasource>"  - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  Example :- "mdReloadPortalMenu ORACLE PROD insiteadmin admin"
ECHO		 "mdReloadPortalMenu SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO		 "mdReloadPortalMenu SQLSERVER 10.10.10.10 PROD insiteadmin admin"
ECHO ------------------------------------------------------------------------------
ECHO © 2015 Siemens Product Lifecycle Management Software Inc.
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END


:EXIT
REM endlocal
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error Populating Data
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%

:END
