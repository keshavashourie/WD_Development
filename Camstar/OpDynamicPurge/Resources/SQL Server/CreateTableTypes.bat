@echo off

SET vNow=%DATE% %TIME%
echo ............................................................................................. > CreateTableTypes.log 
echo Installation started as at %vNow%  >> CreateTableTypes.log
echo Installation started as at %vNow%
echo ............................................................................................. >> CreateTableTypes.log 

SET vServerName=%1
SET vPortNumber=%2
SET vDatabaseName=%3
SET vDatabaseLoginUserName=%4
SET vDatabaseLoginUserPassword=%5

echo vServerName=%vServerName% >> CreateTableTypes.log
echo vDatabaseName=%vDatabaseName% >> CreateTableTypes.log
echo vDatabaseLoginUserName=%vDatabaseLoginUserName% >> CreateTableTypes.log
echo ............................................................................................. >> CreateTableTypes.log 

sqlcmd -S %vServerName%,%vPortNumber% -d %vDatabaseName% -U %vDatabaseLoginUserName% -P %vDatabaseLoginUserPassword% -b -i CreateTableTypes.sql >> CreateTableTypes.log
if errorlevel 1 (
    echo SQL execution failed >> CreateTableTypes.log
    exit /b 1
)
SET vNow=%DATE% %TIME%
echo Installation completed as at %vNow%  >> CreateTableTypes.log
echo Installation completed as at %vNow%