@ECHO OFF
SET ERRORSTATUS=0
SET DB_TYPE=%1
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETSQLSERVER
IF /I (%DB_TYPE%) EQU (DB2) GOTO SETORACLE
GOTO USAGE

:SETORACLE
@csiRunDatabaseScript.bat %1 %2 %3 %4 ".\oracle\CleanupOrphanedPermissions.or.sql" csiCleaupOrphanedPermissions.bat
GOTO EXIT
:END

:SETSQLSERVER
@csiRunDatabaseScript.bat %1 %2 %3 %4 %5 ".\sql server\CleanupOrphanedPermissions.sql" csiCleaupOrphanedPermissions.bat 
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO    "csiCleaupOrphanedPermissions <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"   - Specify database vendor
ECHO  "<Database Host>"          - For ORACLE and DB2: Not Required
ECHO                               For SQL SERVER: Database Server Name or IP Address - Required
ECHO  "<Database | Datasource>"  - Native database name/alias
ECHO  "<User>"                   - Insite database user/logon
ECHO  "<Password>"               - Insite database user password
ECHO  Example :-    "csiCleaupOrphanedPermissions ORACLE PROD insiteadmin admin"
ECHO                "csiCleaupOrphanedPermissions SQLSERVER DBSERVERNAME PROD insiteadmin admin"
ECHO                "csiCleaupOrphanedPermissions SQLSERVER 10.10.10.10 PROD insiteadmin admin"
ECHO ------------------------------------------------------------------------------
ECHO Copyright Siemens 2023  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END


:EXIT
REM endlocal
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%

:END
