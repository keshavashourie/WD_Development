@echo off

REM SET vNow=%date:~10%%date:~4,2%%date:~7,2%%time:~0,2%%time:~3,5%
SET vNow=%DATE% %TIME%
echo ............................................................................................. > CreateDBViews.log 
echo Installation started as at %vNow%  >> CreateDBViews.log
echo Installation started as at %vNow%  
echo ............................................................................................. >> CreateDBViews.log 

SET vServerName=%1
SET vPortNumber=%2
:: THIS IS THE CONFIG DB NAME AND SCHEMA
SET vDatabaseName=%3
SET vDatabaseSchema=%4
::
SET vDatabaseLoginUserName=%5
SET vDatabaseLoginUserPassword=%6
:: THIS IS THE TXN DB NAME AND SCHEMA
SET vTxnDatabaseName=%7
SET vTxnDatabaseSchema=%8

echo vServerName=%vServerName% >> CreateDBViews.log
echo vDatabaseName=%vDatabaseName% >> CreateDBViews.log
echo vDatabaseLoginUserName=%vDatabaseLoginUserName% >> CreateDBViews.log
echo ............................................................................................. >> CreateDBViews.log 
sqlcmd -S %vServerName%,%vPortNumber% -d %vDatabaseName% -U %vDatabaseLoginUserName% -P %vDatabaseLoginUserPassword% -v DatabaseName=%vTxnDatabaseName% SchemaName=%vTxnDatabaseSchema% -b -i CreateDBViews.sql >> CreateDBViews.log
if errorlevel 1 (
    echo SQL execution failed >> CreateDBViews.log
    exit /b 1
)

SET vNow=%DATE% %TIME%
echo Installation completed as at %vNow%  >> CreateDBViews.log
echo Installation completed as at %vNow%  