@ECHO off
setlocal
REM --------------------------------------------------------------------------------------------------
REM  csiresettxnid.bat
REM  Simple batch file used to execute the csiresettxnid stored procedure.
REM  The procedure will reset the start value used to generate transactionids
REM  SQL Server - transactionidtable.idval identity column
REM  Oracle and DB2 - TxnIDSeq sequence
REM  
REM  Assumptions:
REM    The appropriate sql tool(ie sqlplus or osql) is in the users %PATH%
REM  Usage: 
REM    csiresettxnid.bat <DB2/ORACLE/SQLSERVER> <Database> <user> <password>
REM
REM Modification History:
REM Name			Date        	Action
REM -------------------------	----------	------------------------------------------------------
REM Bill Lippard		12/05/2007      SPR S12618. In SQL Server section, changed all calls to 
REM 						osql to call sqlcmd. Changed usage notes to replace 
REM 						Datasource with Database and -D (DataSource) parameter 
REM 						with -d (Database).
REM
REM Purushotham Neelakantachar	05/27/2008	Modified the code for SPR S12618:
REM						- Included a new input parameter: DB_SERVERNAME
REM						This parameter will be used only for SQL SERVER
REM						- Replaced SQL Server's OSQL commands with SQLCMD
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
ECHO DB2 connect to %DB_NAME% user %DB_USERNAME% using %DB_PASSWORD% > temp_%2.cmd
ECHO DB2 call csiresettxnid() >> temp_%2.cmd
ECHO DB2 connect reset >> temp_%2.cmd
DB2CMD /c /w /i temp_%2.cmd
DEL temp_%2.cmd
GOT EXIT
:END

:ORACLE
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO ----------------------------------------------------
ECHO EXEC csiResetTxnId() > _temp.sql
ECHO EXIT >> _temp.sql
SQLPLUS %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% @_temp.sql
DEL _temp.sql
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Server Name       : %DB_SERVERNAME%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO ----------------------------------------------------
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "EXECUTE csiResetTxnid"
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiResetTxnId <ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<ORACLE|SQLSERVER>"      - Specify database vendor
ECHO  "<Database Host>"         - For ORACLE and DB2: Not Required
ECHO                              For SQL SERVER: Database Host Name or IP Address - Required
ECHO  "<Database Name>"         - Native Database Name/Alias
ECHO  "<User>"                  - Insite database user/logon
ECHO  "<Password>"              - Insite database user password
ECHO  Examples: 
ECHO  "csiResetTxnId ORACLE PROD insiteadmin admin"
ECHO  "csiResetTxnId SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO  "csiResetTxnId SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO  "csiResetTxnId SQLSERVER 10.10.10.10 PROD insiteadmin admin"
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
ENDLOCAL
REM exit
:END
