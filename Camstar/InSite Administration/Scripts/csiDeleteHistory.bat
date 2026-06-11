@ECHO OFF
SETLOCAL
REM --------------------------------------------------------------------------------------------------
REM  csiDeleteHistory.bat
REM  Simple batch file used to execute the csideleteMain stored procedure.
REM  The procedure will delete "old" container and history data. 
REM  
REM  Assumptions:
REM    The appropriate sql tool(ie sqlplus or osql) is in the users %PATH%
REM  Usage: 
REM    csiDeleteHistory.bat <DB2/ORACLE/SQLSERVER> <Database> <user> <password> <factory> 
REM                         <retentiondays> <limit> 
REM
REM Modification History:
REM Name			Date		Action
REM --------------		----------	----------------
REM Bill Lippard		12/05/2007	SPR S12618. In SQL Server section, changed all
REM						calls to osql to call sqlcmd.  Changed usage
REM						notes to replace Datasource with Database and
REM						-D (DataSource) parameter with -d (Database).
REM						Removed -n parameter (obsolete).
REM
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S12618
REM						- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Included -S and -b option to SQL Server's SQLCMD
REM
REM  Copyright Siemens 2023  
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
SET FACTORY_NAME=%5
SET RETENTION_DAYS=%6
SET LIMIT=%7
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET FACTORY_NAME=%6
SET RETENTION_DAYS=%7
SET LIMIT=%8
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
IF /I (%DB_TYPE%) EQU (DB2)       GOTO DB2
GOTO USAGE

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiDeleteHistory <DB2|ORACLE|SQLSERVER> <DatabaseHost> <Database Name> <User> <Password> <Factory> <Retention Days> <Limit>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"	 	 - For ORACLE and DB2: Not Required
ECHO				   For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database Name>"          - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  "<Factory>"                - Factory Name (Case sensitive)
ECHO  "<Retention Days>"         - Retention Days (will be checked against LastActivityDate)
ECHO  "<Limit>"                  - Limit
ECHO  Example :- "csiDeleteHistory ORACLE PROD insiteadmin admin HQ 30 1000"
ECHO		 "csiDeleteHistory SQLSERVER DBSERVERNAME PROD insiteadmin admin HQ 30"
ECHO		 "csiDeleteHistory SQLSERVER 10.10.10.10 PROD insiteadmin admin HQ 30"
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:DB2
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > tempdelete_%2.cmd
ECHO DB2 CALL csiDeleteMain('%FACTORY_NAME%',%RETENTION_DAYS%,%LIMIT%) >> tempdelete_%2.cmd
ECHO DB2 CONNECT RESET >> tempdelete_%2.cmd
DB2CMD /c /w /i tempdelete_%2.cmd
DEL tempdelete_%2.cmd
GOTO EXIT
:END

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Factory Name		: %FACTORY_NAME%
ECHO Retention Days		: %RETENTION_DAYS%
ECHO ----------------------------------------------------
ECHO SET SERVEROUTPUT ON SIZE 1000000 > _tempdelete.sql
ECHO EXEC csiDeleteMain('%FACTORY_NAME%',%RETENTION_DAYS%,%LIMIT%) >> _tempdelete.sql
ECHO EXIT >> _tempdelete.sql
SQLPLUS %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% @_tempdelete.sql
DEL _tempdelete.sql
GOTO EXIT
:END

:SQLSERVER
ECHO ----------------------------------------------------
ECHO Database Vendor		: %DB_TYPE%
ECHO Database Host 		: %DB_SERVERNAME%
ECHO Database Name		: %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Factory Name		: %FACTORY_NAME%
ECHO Retention Days		: %RETENTION_DAYS%
ECHO ----------------------------------------------------
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "EXECUTE csiDeleteMain '%FACTORY_NAME%',%RETENTION_DAYS%,%LIMIT%"
SET ERRORSTATUS=%ERRORLEVEL%
GOTO EXIT
:END

:exit
TIME /t
ENDLOCAL
REM exit
:END
