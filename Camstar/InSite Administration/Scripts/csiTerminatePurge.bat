@ECHO OFF
REM --------------------------------------------------------------------------------------------------
REM File:              csiterminatepurge.bat
REM Version:           1.0
REM Author:            Stanly Manuel
REM Create Date:       27-AUG-2004
REM Database:          ORACLE/DB2/SQL
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------
REM Purpose: To gracefully terminate a purge process
REM --------------------------------------------------------------------------------------------------
REM Description: Since purge is a batch process, it will take long time to complete. This batch file 
REM is to stop the purge process gracefully and will be restartable from where it is stopped
REM --------------------------------------------------------------------------------------------------
REM Modification History:
REM Name			Date        	Action
REM -------------------------	----------	------------------------------------------------------
REM Stanly			07/14/2004	Initial Creation DB2
REM
REM Stanly			09/23/2004	oracle version added
REM
REM Stanly			10/05/2004	Sql Server version added
REM
REM Bill Lippard		07/25/2006	Fix for SPR S11030
REM
REM Bill Lippard		10/11/2006	Fix for SPR S11202
REM
REM Bill Lippard		12/01/2006	Updated copyright notice (SPR S1184)
REM
REM Bill Lippard		04/23/2007	Updated copyright notice (SPR S9984)
REM
REM Purushotham Neelakantachar	06/03/2008	Modified the code for SPR S12618:
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


:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiTerminatePurge <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"          - For ORACLE and DB2: Not Required
ECHO                               For SQL SERVER: Database Host Name or IP Address - Required
ECHO  "<Database Name>"          - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO
ECHO  Example:- "csiTerminatePurge ORACLE PROD insiteadmin admin"
ECHO		"csiTerminatePurge SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO		"csiTerminatePurge SQLSERVER 10.10.10.10 PROD insiteadmin admin"
ECHO
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:DB2
ECHO Terminating the purge process in database %DB_NAME% (DB2)
ECHO Sending terminate request to purge process...
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > tmprocTerminatePurge.cmd
ECHO DB2 UPDATE PURGERESTORESETUP SET Value='Y' WHERE Parameter='PURGE_TERMINATE' >> tmprocTerminatePurge.cmd
ECHO DB2 COMMIT >> tmprocTerminatePurge.cmd
ECHO DB2 CONNECT RESET >> tmprocTerminatePurge.cmd
DB2CMD /c /w /i tmprocTerminatePurge.cmd
DEL tmprocTerminatePurge.cmd
ECHO The purge process will be terminated soon...
GOTO exit
:end

:ORACLE
ECHO Terminating the purge process in database %DB_NAME% (ORACLE)
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Process Name               : %PROCESS_NAME%
ECHO Process Type               : %PROCESS_TYPE%
ECHO ----------------------------------------------------
ECHO Sending terminate request to purge process...
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% > tmprocTerminatePurge.sql
ECHO UPDATE PURGERESTORESETUP SET Value='Y' WHERE Parameter='PURGE_TERMINATE'; >> tmprocTerminatePurge.sql
ECHO COMMIT; >> tmprocTerminatePurge.sql
ECHO EXIT; >> tmprocTerminatePurge.sql
SQLPLUS /NOLOG @tmprocTerminatePurge.sql
DEL tmprocTerminatePurge.sql
ECHO The purge process will be terminated soon...
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
ECHO Terminating the purge process in database %DB_NAME% (SQL SERVER)
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Server Name       : %DB_SERVERNAME%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Process Name               : %PROCESS_NAME%
ECHO Process Type               : %PROCESS_TYPE%
ECHO ----------------------------------------------------
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "UPDATE PURGERESTORESETUP SET Value='Y' WHERE Parameter='PURGE_TERMINATE'"
ECHO The purge process will be terminated soon...
GOTO EXIT
:END

:EXIT
TIME /t
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error terminating the purge process in %DB_NAME%
	PAUSE
)
ENDLOCAL
:END
