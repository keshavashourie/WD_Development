@ECHO off
REM setlocal
REM --------------------------------------------------------------------------------------------------
REM  csiRunDatabaseScript.bat
REM  Simple batch file used to run database scripts.
REM  Assumptions:
REM     - Run from the same directory as the batch file
REM     - The appropriate sql tool(ie sqlplus or sqlcmd) is in the users %PATH%
REM  Usage: 
REM    csiRunDatabaseScript.bat <DB2/ORACLE/SQLSERVER> <Database> <user> <password> <scripttorun> <callingscript>
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
REM Ramesh Nagamalli	02/11/2009	Copied PopulateReleaseData batch file and generalized it to run any script file that is passed
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
IF /I (%DB_TYPE%) EQU (DB2) GOTO SETDB2
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET DB_SCRIPT=%5
SET CALLING_SCRIPT=%6
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET DB_SCRIPT=%6
SET CALLING_SCRIPT=%7
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set DB2 related data
REM --------------------------
:SETDB2
ECHO ----------------------------------------------------
ECHO DB2 is currently not supported...
ECHO ----------------------------------------------------
SET ERRORSTATUS=99
GOTO EXIT
:END


REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%DB_SCRIPT%)==() GOTO USAGE
IF (%CALLING_SCRIPT%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
GOTO USAGE
:END

:DB2
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > tempdata_%2.cmd
ECHO DB2 -td@ -s -f %DB_SCRIPT% >> tempdata_%2.cmd
DB2CMD /c /w /i tempdata_%2.cmd
IF ERRORLEVEL 1 SET ERRORSTATUS=99
DEL tempdata_%2.cmd
GOTO EXIT
:END

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Name			: %DB_NAME%
ECHO Database User Name		: %DB_USERNAME%
ECHO ----------------------------------------------------
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempdata_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> tempdata_%DB_NAME%.sql
ECHO @%DB_SCRIPT% >> tempdata_%DB_NAME%.sql
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
ECHO Database Name			: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO ----------------------------------------------------
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT SUBSTRING(DB_NAME(),1,50)"
IF ERRORLEVEL 1 SET ERRORSTATUS=99
IF %ERRORSTATUS% NEQ 0 GOTO EXIT
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i %DB_SCRIPT%
IF ERRORLEVEL 1 set errorstatus=99
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO	"csiRunDatabaseScript <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password> <ScriptToRun> <CallingScript>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"	 		 - For ORACLE and DB2: Not Required
ECHO				   For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database | Datasource>"  - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  "<ScriptToRun>"            - Script file that has to be executed
ECHO  "<CallingScript>"          - Calling Script file that has invoked this script
ECHO  Example :-	csiRunDatabaseScript ORACLE PROD insiteadmin admin ".\oracle\CleanupOrphanedPermissions.or.sql" csiCleaupOrphanedPermissions.bat
ECHO				csiRunDatabaseScript SQLSERVER DBSERVERNAME PROD insiteadmin admin ".\sql server\CleanupOrphanedPermissions.sql" csiCleaupOrphanedPermissions.bat
ECHO				csiRunDatabaseScript SQLSERVER 10.10.10.10 PROD insiteadmin admin ".\sql server\CleanupOrphanedPermissions.sql" csiCleaupOrphanedPermissions.bat
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END


:EXIT
REM endlocal
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error running script %CALLING_SCRIPT%
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%

:END
