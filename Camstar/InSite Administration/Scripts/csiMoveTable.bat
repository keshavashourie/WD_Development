@ECHO OFF
REM SETLOCAL
REM csiMoveTable.bat
REM -----------------------------------------------------------------------------------------------------------
REM  Batch file used to move the specified table to the specified tablespace/filegroup.
REM  Assumptions:
REM     - Run from Camstar Home directory
REM	  - Ex: CAMSTAR_HOME=C:\Program Files\Camstar\InSite Administration
REM	        $CAMSTAR_HOME\Scripts
REM     - Location of the SQL Scripts:
REM	  - csiMoveTable.or.sql(ORACLE): $CAMSTAR_HOME\Scripts\Oracle
REM	  - csiMoveTable.sql(SQLSERVER): $CAMSTAR_HOME\Scripts\SQL Server
REM     - The appropriate sql tool(ie sqlplus or sqlcmd) is in the users %PATH%
REM
REM -------------------------------------------------------------------
REM CAUTION  ****  CAUTION  ****  CAUTION  ****  CAUTION  ****  CAUTION
REM
REM Before running this script, please ensure to back up the database
REM and verify the back ups are good.
REM
REM CAUTION  ****  CAUTION  ****  CAUTION  ****  CAUTION  ****  CAUTION
REM -------------------------------------------------------------------
REM  Usage: 
REM    csiMoveTable.bat <DB2/ORACLE/SQLSERVER> <Database | DataSource> <user> <password> <TableOwner> <TableName> <TablespaceName>
REM 
REM
REM Modification History:
REM Name			Date            Action
REM -------------------------	----------	---------------------------------------------------------------
REM Bill Lippard		09/04/2008	Initial Release (S13641) - Bill Lippard
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S14004
REM						- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Included -S option to SQL Server's SQLCMD
REM Purushotham Neelakantachar	10/12/2009 	Modified the code as below
REM						- Included two new input parameters
REM							- TABLE_OWNER
REM							- TABLESPACE_NAME
REM						- Added new logic for new input parameters
REM						- Included code to execute stored procedures
REM						- Included code to log more messages
REM
REM Copyright Siemens 2023  
REM -----------------------------------------------------------------------------------------------------------

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
SET TABLE_OWNER=%5
SET TABLE_NAME=%6
SET TABLESPACE_NAME=%7

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
SET FILE_GROUP=%7
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
CD Oracle
ECHO Moving table in Oracle database %DB_NAME% ...
ECHO Moving table in Oracle database %DB_NAME% ... > "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ---------------------------------------------------- >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Current Directory          : %INSITE_HOME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database Vendor            : %DB_VENDOR% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database Name              : %DB_NAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database User Name         : %DB_USERNAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Table Owner                : %TABLE_OWNER% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Table Name                 : %TABLE_NAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Tablespace Name            : %TABLESPACE_NAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ---------------------------------------------------- >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Current Directory          : %INSITE_HOME%
ECHO Database Vendor            : %DB_VENDOR%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Table Owner                : %TABLE_OWNER%
ECHO Table Name                 : %TABLE_NAME%
ECHO Tablespace Name            : %TABLESPACE_NAME%
ECHO ----------------------------------------------------
IF (%DB_USERNAME%)==(SYS) (SET CONNECT_STRING=%DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% AS SYSDBA) ELSE (SET CONNECT_STRING=%DB_USERNAME%/%DB_PASSWORD%@%DB_NAME%)
ECHO WHENEVER SQLERROR EXIT FAILURE  > "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO CONNECT %CONNECT_STRING% >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO SET SERVEROUTPUT ON SIZE 1000000 >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO SET LINESIZE 200 >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO SET PAGES 1000 >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO WHENEVER SQLERROR CONTINUE  >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO SHOW USER  >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO @csiMoveTable.or.sql >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO EXECUTE MOVE_TABLE ( '%TABLE_OWNER%','%TABLE_NAME%','%TABLESPACE_NAME%'); >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO DROP PROCEDURE MOVE_TABLE; >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
ECHO EXIT >> "%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql"
SQLPLUS /NOLOG  @"%INSITE_HOME%/TempMoveTable_%DB_NAME%.sql" >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
DEL "%INSITE_HOME%\TempMoveTable_%DB_NAME%.sql"
IF %ERRORSTATUS% NEQ 99 ECHO. >> "%INSITE_HOME%\csiMoveTable_%DB_NAME%.log"
FINDSTR /I /C:"ORA-" "%INSITE_HOME%\csiMoveTable_%DB_NAME%.log" > NUL
IF %ERRORLEVEL%==0 SET ERRORSTATUS=99
IF %ERRORSTATUS%==0 ECHO Move Table successful
IF %ERRORSTATUS%==0 ECHO Move Table successful >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
CD ..
GOTO EXIT
:END


:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
IF (%FILE_GROUP%)==() GOTO USAGE
CD SQL Server
ECHO Moving table in SQL Server database %DB_NAME% ...
ECHO Moving table in SQL Server database %DB_NAME% ... > "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ---------------------------------------------------- >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Current Directory          : %INSITE_HOME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database Vendor            : %DB_VENDOR% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database Host              : %DB_SERVERNAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database Name              : %DB_NAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Database User Name         : %DB_USERNAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO Table Name                 : %TABLE_NAME% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO File Group                 : %FILE_GROUP% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ---------------------------------------------------- >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Current Directory          : %INSITE_HOME%
ECHO Database Vendor            : %DB_VENDOR%
ECHO Database Host              : %DB_SERVERNAME%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Table Name                 : %TABLE_NAME%
ECHO File Group                 : %FILE_GROUP%
ECHO ----------------------------------------------------
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT DB_NAME()" >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
IF %ERRORSTATUS% NEQ 0 GOTO EXIT
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiMoveTable.sql -v TableName=%TABLE_NAME% FileGroup=%FILE_GROUP% >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
SET ERRORSTATUS=%ERRORLEVEL%
ECHO. >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
IF %ERRORSTATUS%==0 ECHO Move Table successful
IF %ERRORSTATUS%==0 ECHO Move Table successful >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed
IF %ERRORSTATUS% NEQ 0 ECHO Move Table failed >> "%INSITE_HOME%/csiMoveTable_%DB_NAME%.log"
CD ..
GOTO EXIT
:END



:USAGE
ECHO ------------------------------------------------------------------------------
ECHO USAGE: 
ECHO "csiMoveTable.bat <DB2/ORACLE/SQLSERVER> <Database | DataSource> <user> <password> <TableOwner> <TableName> <TablespaceName>"
ECHO.
ECHO Where:
ECHO  "<ORACLE|SQLSERVER>"          - Specify database vendor
ECHO  "<Database Host>"             - For ORACLE: N/A
ECHO                                  For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database Name>"             - Native database name/alias
ECHO  "<User>"                      - Insite database user/logon
ECHO                                  - ORACLE: User should have privileges to move tablespace
ECHO                                  - SQL SERVER: User should have privileges to move filegroups
ECHO  "<Password>"                  - Insite database user password
ECHO  "<TableOwner>"                - For Oracle: Owner of the Table to be moved
ECHO				      For SQL Server: N/A
ECHO  "<TableName>"                 - Name of the Table to be moved
ECHO  "<Tablespace Name/FileGroup>" - Name of the tablespace or filegroup to which the table is moved
ECHO.
ECHO  Example :- "csiMoveTable ORACLE PROD InSiteAdmin admin InSiteAdmin CONTAINER TS_INSITEADMIN_DATA"
ECHO		 "csiMoveTable ORACLE PROD SYS secret InSiteAdmin CONTAINER TS_INSITEADMIN_DATA"
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
	ECHO Log file "csiMoveTable_%DB_NAME%.log" generated in "%INSITE_HOME%" directory
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%
:END

