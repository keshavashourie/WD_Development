@echo off

SET vNow=%DATE% %TIME%
echo ............................................................................................. > CompileSourcePrograms.log 
echo Installation started as at %vNow%  >> CompileSourcePrograms.log
echo Installation started as at %vNow%
echo ............................................................................................. >> CompileSourcePrograms.log 

SET vServerName=%1
SET vPortNumber=%2
SET vDatabaseName=%3
SET vDatabaseLoginUserName=%4
SET vDatabaseLoginUserPassword=%5

echo vServerName=%vServerName% >> CompileSourcePrograms.log
echo vDatabaseName=%vDatabaseName% >> CompileSourcePrograms.log
echo vDatabaseLoginUserName=%vDatabaseLoginUserName% >> CompileSourcePrograms.log
echo ............................................................................................. >> CompileSourcePrograms.log 

sqlcmd -S %vServerName%,%vPortNumber% -d %vDatabaseName% -U %vDatabaseLoginUserName% -P %vDatabaseLoginUserPassword% -b -i CompileSourcePrograms.sql >> CompileSourcePrograms.log
if errorlevel 1 (
    echo SQL execution failed >> CompileSourcePrograms.log
    exit /b 1
)
SET vNow=%DATE% %TIME%
echo Installation completed as at %vNow%  >> CompileSourcePrograms.log
echo Installation completed as at %vNow%