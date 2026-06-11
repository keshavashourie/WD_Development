@ECHO off
REM setlocal
REM --------------------------------------------------------------------------------------------------
REM  csiPopulateRelease.bat
REM  Simple batch file used to run the populatereleasedata scripts.
REM  Assumptions:
REM     - Run from the same directory as the populaterelease.bat file
REM     - The appropriate sql tool(ie sqlplus or sqlcmd) is in the users %PATH%
REM  Usage: 
REM    csipopulaterelease.bat <DB2/ORACLE/SQLSERVER> <Database> <user> <password
REM
REM Modification History:
REM Name			Date        	Action
REM --------------------------	----------	------------------------------------------------------
REM Bill Lippard		06/13/2007      Modified exit code to leave window open and removed 
REM						pause after usage display (S11478)
REM
REM Bill Lippard		12/05/2007      SPR S12618. In SQL Server section, changed all
REM						calls to osql to call sqlcmd.  Changed usage
REM						notes to replace Datasource with Database and
REM						-D (DataSource) parameter with -d (Database).
REM						Removed -n parameter (obsolete).
REM
REM Purusohtham Neelakantachar	05/27/2008	Modified the code for SPR S12618
REM						- Included a new input parameter: DB_SERVERNAME
REM						This parameter will be used only for SQL SERVER
REM						- Replaced SQL Server's OSQL commands with SQLCMD
REM
REM Barry Etter			06/13/2008	Modified the code to populate to RBAC Starter Data
REM
REM Copyright Siemens 2023  
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

:DB2
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > tempdata_%2.cmd
ECHO DB2 -td@ -s -f .\db2\PopulateReleaseData.db2 >> tempdata_%2.cmd
DB2CMD /c /w /i tempdata_%2.cmd
IF ERRORLEVEL 1 SET ERRORSTATUS=99
DEL tempdata_%2.cmd
GOTO EXIT
:END

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name		: %DB_USERNAME%
ECHO ----------------------------------------------------
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempdata_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> tempdata_%DB_NAME%.sql
ECHO @.\oracle\PopulateReleaseData.or.sql >> tempdata_%DB_NAME%.sql
echo @.\oracle\PopulateRBACDefaultData.or.sql >> tempdata_%DB_NAME%.sql
echo @.\oracle\PopulatePortalMenuDefaultData.or.sql >> tempdata_%DB_NAME%.sql
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
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i ".\sql server\PopulateReleaseData.sql"
IF ERRORLEVEL 1 set errorstatus=99
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i ".\sql server\PopulateRBACDefaultData.sql"
IF ERRORLEVEL 1 set errorstatus=99
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i ".\sql server\PopulatePortalMenuDefaultData.sql"
IF ERRORLEVEL 1 set errorstatus=99
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i ".\sql Server\CMSpecDefaultData.sql"
IF ERRORLEVEL 1 set errorstatus=99
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiPopulateRelease.bat <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"	 	 - For ORACLE and DB2: Not Required
ECHO				   For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database | Datasource>"  - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  Example :- "csiPopulateRelease ORACLE PROD insiteadmin admin"
ECHO		 "csiPopulateRelease SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO		 "csiPopulateRelease SQLSERVER 10.10.10.10 PROD insiteadmin admin"
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
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
