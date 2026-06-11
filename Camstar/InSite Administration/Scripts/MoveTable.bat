@echo off
REM setlocal
REM  MoveTable.bat
REM -----------------------------------------------------------------------------------------------------------------------------
REM  Batch file used to move the specified table to the specified tablespace/filegroup.
REM  Assumptions:
REM     - Run from the Camstar\InSite Administration\scripts directory
REM     - The MoveTable.sql script must be in the Camstar\InSite Administration\scripts\SQL Server directory
REM     - The appropriate sql tool(ie sqlplus or sqlcmd) is in the users %PATH%
REM  Usage: 
REM    MoveTable.bat <DB2/ORACLE/SQLSERVER> <Database | DataSource> <user> <password> <TableName> <TablespaceName>
REM 
REM
REM Modification History:
REM Name			Date            Action
REM -------------------------	----------	----------------------------------------------------------------
REM Bill Lippard		09/04/2008	Initial Release (S13641) - Bill Lippard
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S14004
REM						- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Included -S option to SQL Server's SQLCMD
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
SET TABLE_NAME=%5
SET TABLESPACE_NAME=%6

GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET TABLE_NAME=%6
SET FILEGROUP=%7
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%TABLE_NAME%)==() GOTO USAGE
IF /I (%DB_VENDOR%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_VENDOR%) EQU (SQLSERVER) GOTO SQLSERVER
IF /I (%DB_VENDOR%) EQU (DB2)       GOTO DB2
GOTO USAGE
:END

:DB2
echo This feature not implemented for DB2!
pause
goto exit
:end

:ORACLE
ECHO Moving table in Oracle database %DB_NAME% ...
ECHO Moving table in Oracle database %DB_NAME% ... > "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_VENDOR%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO Table Name			: %TABLE_NAME%
ECHO Tablespace Name		: %TABLESPACE_NAME%
ECHO ----------------------------------------------------
ECHO WHENEVER SQLERROR EXIT FAILURE  > TempMoveTable_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >> TempMoveTable_%DB_NAME%.sql
ECHO WHENEVER SQLERROR CONTINUE  >> TempMoveTable_%DB_NAME%.sql
ECHO ALTER TABLE %DB_USERNAME%.%TABLE_NAME% MOVE TABLESPACE %TABLESPACE_NAME; >> TempMoveTable_%DB_NAME%.sql
ECHO EXIT >> TempMoveTable_%DB_NAME%.sql
SQLPLUS /NOLOG  @TempMoveTable_%DB_NAME%.sql >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
DEL TempMoveTable_%DB_NAME%.sql
IF %ERRORSTATUS% NEQ 99 ECHO. >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
FINDSTR /I /C:"ORA-" ".\MoveTable_%DB_NAME%.log" > NUL
IF %ERRORLEVEL%==0 SET ERRORSTATUS=99
IF %ERRORSTATUS%==0 ECHO Move Table successful >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
CD ..
GOTO EXIT
:END


:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
IF (%FILEGROUP%)==() GOTO USAGE
CD SQL Server
ECHO Moving table in SQL Server database %DB_NAME% ...
ECHO Moving table in SQL Server database %DB_NAME% ... > "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_VENDOR%
ECHO Database Host		: %DB_SERVERNAME%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name 	: %DB_USERNAME%
ECHO Table Name			: %TABLE_NAME%
ECHO File Group			: %FILEGROUP%
ECHO ----------------------------------------------------
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT DB_NAME()" >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
IF %ERRORSTATUS% NEQ 0 GOTO EXIT
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiMoveTable.sql -v TableName=%TABLE_NAME% FileGroup=%FILEGROUP% >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
ECHO. >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
IF %ERRORSTATUS%==0 ECHO Move Table successful >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed >> "%INSITE_HOME%/MoveTable_%DB_NAME%.log"
CD ..
GOTO EXIT
:END



:USAGE
ECHO ------------------------------------------------------------------------------
ECHO USAGE: 
ECHO  "csiMoveTable.bat <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password> <DBType>"
ECHO Where:
ECHO  "<ORACLE|SQLSERVER>"          - Specify database vendor
ECHO  "<Database Host>"             - For ORACLE: Not Required
ECHO                                  For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database Name>"             - Native database name/alias
ECHO  "<User>"                      - Insite database user/logon
ECHO  "<Password>"                  - Insite database user password
ECHO  "<TableName>"                 - Name of the Table to be moved
ECHO  "<Tablespace Name/FileGroup>" - Name of the tablespace or filegroup to which the table is moved
ECHO.
ECHO  Example :- "csiMoveTable ORACLE PROD InSiteAdmin admin CONTAINER TS_INSITEADMIN_DATA"
ECHO		 "csiMoveTable SQLSERVER DBSERVERNAME PROD InSiteAdmin admin CONTAINER FG_INSITEADMIN_DATA"
ECHO		 "csiMoveTable SQLSERVER 10.10.10.10 PROD InSiteAdmin admin CONTAINER FG_INSITEADMIN_DATA"
ECHO. 
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error Moving table %TABLE_NAME%
	ECHO Log file "MoveTable_%DB_NAME%.log" generated in "%INSITE_HOME%" directory
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%
:END

