@ECHO OFF
REM --------------------------------------------------------------------------------------------------
REM Purpose: To gracefully terminate an InSite ExportImport purge process
REM --------------------------------------------------------------------------------------------------
REM Description: Since purge is a batch process, it will take long time to complete. This batch file 
REM is to stop the purge process gracefully and will be restartable from where it is stopped
REM --------------------------------------------------------------------------------------------------
REM File:              csiTerminateExpImpPurge.bat
REM Version:           1.0
REM Author:            Purushotham Neelakantachar
REM Create Date:       23-APR-2007
REM Database:          ORACLE/DB2/SQL
REM
REM Modification History:
REM Name			Date        	Action
REM --------------------------	----------	------------------------------------------------------
REM Purushotham Neelakantachar	04/23/2007	Initial version
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
SET PROCESS_NAME=%5
SET PROCESS_TYPE=%6
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET PROCESS_NAME=%6
SET PROCESS_TYPE=%7
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%PROCESS_NAME%)==() GOTO USAGE
IF (%PROCESS_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
GOTO USAGE
:END

:DB2
ECHO Terminating the purge process in database %DB_NAME% (DB2)
ECHO Sending terminate request to purge process...
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > TerminateExpImpPurge.cmd
ECHO UPDATE CSI_PURGE_CONFIG SET Parameter_Value = 'TERMINATE' WHERE Process_Name = '%PROCESS_NAME%' AND Process_Type = '%PROCESS_TYPE%' AND Parameter_Name = 'PROCESS-INTERRUPT-TYPE' AND Parameter_Status = 'ACTIVE'; >> TerminateExpImpPurge.cmd
ECHO DB2 COMMIT >> TerminateExpImpPurge.cmd
ECHO DB2 CONNECT RESET >> TerminateExpImpPurge.cmd
DB2CMD /c /w /i TerminateExpImpPurge.cmd
DEL TerminateExpImpPurge.cmd
ECHO The purge process will be terminated soon...
GOTO EXIT
:END

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
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% > TerminateExpImpPurge.or.sql
ECHO UPDATE CSI_PURGE_CONFIG SET Parameter_Value = 'TERMINATE' WHERE Process_Name = '%PROCESS_NAME%' AND Process_Type = '%PROCESS_TYPE%' AND Parameter_Name = 'PROCESS-INTERRUPT-TYPE' AND Parameter_Status = 'ACTIVE'; >> TerminateExpImpPurge.or.sql
ECHO COMMIT; >> TerminateExpImpPurge.or.sql
ECHO EXIT; >> TerminateExpImpPurge.or.sql
SQLPLUS /NOLOG @TerminateExpImpPurge.or.sql
IF ERRORLEVEL 1 SET ERRORSTATUS=99
DEL TerminateExpImpPurge.or.sql
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
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "UPDATE CSI_PURGE_CONFIG SET Parameter_Value = 'TERMINATE' WHERE Process_Name = '%PROCESS_NAME%' AND Process_Type = '%PROCESS_TYPE%' AND Parameter_Name = 'PROCESS-INTERRUPT-TYPE' AND Parameter_Status = 'ACTIVE'"
IF ERRORLEVEL 1 SET ERRORSTATUS=99
ECHO The purge process will be terminated soon...
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiTerminateExportImportPurge <ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password> <Process Name> <Process Type>"
ECHO Where
ECHO  "<ORACLE|SQLSERVER>"      - Specify database vendor
ECHO  "<Database Host>"         - For ORACLE and DB2: Not Required
ECHO                              For SQL SERVER: Database Host Name or IP Address - Required
ECHO  "<Database Name>"         - Native Database Name/Alias
ECHO  "<User>"                  - Insite database user/logon
ECHO  "<Password>"              - Insite database user password
ECHO  "<Process Name>"          - Process Name (UPPER - Case sensitive)
ECHO  "<Process Type>"          - Process Type (UPPER - Case sensitive)
ECHO  Examples: 
ECHO  "csiTerminateExportImportPurge ORACLE PROD insiteadmin admin EXPORT-IMPORT-PURGE CONTENT"
ECHO  "csiTerminateExportImportPurge SQLSERVER DBSERVERNAME PROD insiteadmin admin EXPORT-IMPORT-PURGE CONTENT"
ECHO  "csiTerminateExportImportPurge SQLSERVER 10.10.10.10 PROD insiteadmin admin EXPORT-IMPORT-PURGE CONTENT"
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
TIME /t
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error terminating ExportImportPurge in the %DB_NAME% database
	PAUSE
)
ENDLOCAL
:END
