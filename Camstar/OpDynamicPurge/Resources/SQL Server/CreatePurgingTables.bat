@echo off

SET vNow=%DATE% %TIME%
echo ............................................................................................. > CreatePurgingTables.log 
echo Installation started as at %vNow%  >> CreatePurgingTables.log
echo Installation started as at %vNow%
echo ............................................................................................. >> CreatePurgingTables.log 

SET vServerName=%1
SET vPortNumber=%2
SET vDatabaseName=%3
SET vDatabaseLoginUserName=%4
SET vDatabaseLoginUserPassword=%5

echo vServerName=%vServerName% >> CreatePurgingTables.log
echo vDatabaseName=%vDatabaseName% >> CreatePurgingTables.log
echo vDatabaseLoginUserName=%vDatabaseLoginUserName% >> CreatePurgingTables.log
echo ............................................................................................. >> CreatePurgingTables.log 

sqlcmd -S %vServerName%,%vPortNumber% -d %vDatabaseName% -U %vDatabaseLoginUserName% -P %vDatabaseLoginUserPassword% -b -i CreatePurgingTables.sql >> CreatePurgingTables.log
if errorlevel 1 (
    echo SQL execution failed >> CreatePurgingTables.log
    exit /b 1
)
SET vNow=%DATE% %TIME%
echo Installation completed as at %vNow%  >> CreatePurgingTables.log
echo Installation completed as at %vNow%