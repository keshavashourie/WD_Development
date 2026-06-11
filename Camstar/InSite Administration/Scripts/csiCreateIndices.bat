@ECHO OFF
REM setlocal
REM -----------------------------------------------------------------------------------------------------------------------------
REM  csiCreateIndices.bat
REM  Simple batch file used to run the csiCreateIndexes scripts.
REM  Assumptions:
REM     - Run from the same directory as the csiCreateIndices.bat file
REM     - The appropriate sql tool(ie sqlplus or osql) is in the users %PATH%
REM  Usage: 
REM    csiCreateIndices.bat <DB2/ORACLE/SQLSERVER> <Database> <user> <password> <DBType>
REM 
REM  Error Detection:  Since errors may result from running the create index process against 
REM                    an existing database, limit error checking to the connect statements.
REM
REM Modification History:
REM Name			Date		Action
REM --------------		----------	----------------
REM Bill Lippard		12/19/2006	Modified exit code to pause on error (S11268)
REM
REM Bill Lippard		04/23/2007      Added copyright notice (S9984)
REM
REM Bill Lippard		06/13/2007      Modified exit code to leave window open and removed pause after 
REM						usage display (S11478)
REM Bill Lippard		12/05/2007      SPR S12618. In SQL Server section, changed all calls to osql to
REM 						call sqlcmd.  Changed usage notes to replace Datasource with 
REM						Database and -D (DataSource) parameter with -d (Database).
REM						Removed -n parameter (obsolete).
REM
REM Purushotham Neelakantachar	05/27/2008	Modified the code for SPR S12757
REM						- Included logic to exit with a error code
REM
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S12618
REM						- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Included -S option to SQL Server's SQLCMD
REM
REM Purushotham Neelakantachar	02/04/2009 	Modified the code for SPR S14883
REM						- Included SQL Plus Formatting commands
REM
REM
REM Copyright Siemens 2023  
REM -----------------------------------------------------------------------------------------------------------------------------

TIME /t
SET ERRORSTATUS=0
SET INSITE_HOME=%CD%


REM Set local variables
REM -------------------
:SETVARIABLES
SET DB_VENDOR=%1
IF (%DB_VENDOR%)==() GOTO USAGE
IF /I (%DB_VENDOR%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_VENDOR%) EQU (SQLSERVER) GOTO SETSQLSERVER
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET DB_TYPE=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET DB_TYPE=%6
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_VENDOR%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_VENDOR%) EQU (SQLSERVER) GOTO SQLSERVER
IF /I (%DB_VENDOR%) EQU (DB2)       GOTO DB2
GOTO USAGE
:END

:DB2
ECHO db2 connect to %2 user %3 using %4 > tempind_%2.cmd
db2cmd /c /w /i tempind_%2.cmd
IF ERRORLEVEL 1 set errorstatus=99
IF %errorstatus% NEQ 0 goto exit
ECHO db2 connect to %2 user %3 using %4 > tempind_%2.cmd
ECHO db2 CALL csicreateindexes('NEW','EXECUTE','%5') >> tempind_%2.cmd
ECHO db2 connect reset >> tempind_%2.cmd
db2cmd /c /w /i tempind_%2.cmd
IF ERRORLEVEL 1 set errorstatus=99
del tempind_%2.cmd
goto exit
:END

:ORACLE
ECHO Creating indices into Oracle database %DB_NAME% ...
ECHO Creating indices into Oracle database %DB_NAME% ... > "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_VENDOR%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO Database Type		: %DB_TYPE%
ECHO ----------------------------------------------------
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempind_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> tempind_%DB_NAME%.sql
ECHO WHENEVER SQLERROR CONTINUE  >> tempind_%DB_NAME%.sql
ECHO SET SERVEROUTPUT ON SIZE 1000000 >> tempind_%DB_NAME%.sql
ECHO SET LINESIZE 300 >> tempind_%DB_NAME%.sql
ECHO SET PAGESIZE 1000 >> tempind_%DB_NAME%.sql
ECHO SET ECHO OFF >> tempind_%DB_NAME%.sql
ECHO SET HEADING OFF >> tempind_%DB_NAME%.sql
ECHO EXECUTE csiCreateIndexes('NEW','EXECUTE','%DB_TYPE%'); >> tempind_%DB_NAME%.sql
ECHO EXIT >> tempind_%DB_NAME%.sql
SQLPLUS /NOLOG  @tempind_%DB_NAME%.sql >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
DEL tempind_%DB_NAME%.sql
IF %ERRORSTATUS% NEQ 99 ECHO. >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
FINDSTR /I /C:"ORA-" ".\CreateIndices_%DB_NAME%.log" > NUL
IF %ERRORLEVEL%==0 SET ERRORSTATUS=99
IF %ERRORSTATUS%==0 ECHO Creating indices successful >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Creating indices failed >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
ECHO Creating indices into SQL Server database %DB_NAME% ...
ECHO Creating indices into SQL Server database %DB_NAME% ... > "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_VENDOR%
ECHO Database Host		: %DB_SERVERNAME%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO Database Type		: %DB_TYPE%
ECHO ----------------------------------------------------
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT DB_NAME()" >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
IF %ERRORSTATUS% NEQ 0 GOTO EXIT
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "EXECUTE csiCreateIndexes 'NEW','EXECUTE','%DB_TYPE%'" >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
ECHO. >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
IF %ERRORSTATUS%==0 ECHO Creating indices successful >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Creating indices failed >> "%INSITE_HOME%/CreateIndices_%DB_NAME%.log"

GOTO EXIT
:END

:USAGE
ECHO ------------------------------------------------------------------------------
ECHO USAGE: 
ECHO  "csiCreateIndices.bat <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password> <DBType>"
ECHO Where:
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"          - For ORACLE and DB2: Not Required
ECHO                               For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database Name>"          - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  "<DBtype>"                 - OLTP or DATASTORE
REM
ECHO  Example :- "csiCreateIndices ORACLE PROD InSiteAdmin admin OLTP"
ECHO		 "csiCreateIndices SQLSERVER DBSERVERNAME PROD InSiteAdmin admin OLTP"
ECHO		 "csiCreateIndices SQLSERVER 10.10.10.10 PROD InSiteAdmin admin DATASTORE"
ECHO		 "csiCreateIndices DB2 PROD InSiteAdmin admin OLTP"
ECHO 
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error Creating Indices
	ECHO Log file "CreateIndices_%DB_NAME%.log" generated in "%INSITE_HOME%" directory
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%
:END
