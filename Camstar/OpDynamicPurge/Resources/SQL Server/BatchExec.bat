@echo off

:: Check if arguments were provided (silent mode) or prompt for input (interactive mode)
if "%~1"=="" goto interactive_mode

:: Silent mode - use command line arguments
set "DatabaseHost=%~1"
set "PortNumber=%~2"
set "ConfigDBUsername=%~3"
set "ConfigDBPassword=%~4"
set "TxnDBName=%~5"
set "TxnDBSchema=%~6"
set "ArchiveDBName=%~7"
set "ArchiveDBSchema=%~8"
set "DesignerMetadataDatabase=%~9"
shift
set "SiteInfoDatabase=%~9"

echo Running in silent mode...
goto process_inputs

:interactive_mode
:: Interactive mode - prompt for user input

set /p DatabaseHost=Enter Database Host: 
set /p PortNumber=Enter Database Port Number: 
set /p ConfigDBUsername=Enter Config Database Username: 

:: Get the current directory for PowerShell password encryption
set "current_dir=%cd%"
cd ..\..
set "config_folder=%cd%"
cd "%current_dir%"

:: Use PowerShell to securely prompt for the password, encrypt it, and output both values
for /f "tokens=1,2 delims=," %%A in ('powershell -Command "$dllPath = '%config_folder%\Camstar.Util.dll'; Add-Type -Path $dllPath; $password = Read-Host 'Enter Config Database Password' -AsSecureString; $plainTextPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)); $encryptedPassword = [Camstar.Util.CryptUtil]::Encrypt($plainTextPassword); Write-Output ($plainTextPassword + ',' + $encryptedPassword)"') do (
    set ConfigDBPassword=%%A
    set encrypted_password=%%B
)

set /p TxnDBName=Enter Transactional Database Name: 
set /p TxnDBSchema=Enter Transactional Database Schema: 
set /p ArchiveDBName=Enter Archive Database Name: 
set /p ArchiveDBSchema=Enter Archive Database Schema: 
set /p DesignerMetadataDatabase=Enter InSite.mdb file path. [ENTER] to accept C:\Program Files (x86)\Camstar\InSite Administration: 
set /p SiteInfoDatabase=Enter SiteInfo.mdb file path.[ENTER] to accept C:\Program Files (x86)\Camstar\InSite Administration: 

:process_inputs
:: Get the current directory
set "current_dir=%cd%"

:: Set insite/siteinfo default path
set "default_path=C:\Program Files (x86)\Camstar\InSite Administration"

:: Move two folders up
cd ..\..
set "config_folder=%cd%"

:: For silent mode, encrypt the password here since it wasn't done during input
if not "%~1"=="" (
    for /f "tokens=1 delims=," %%A in ('powershell -Command "$dllPath = '%config_folder%\Camstar.Util.dll'; Add-Type -Path $dllPath; $encryptedPassword = [Camstar.Util.CryptUtil]::Encrypt('%ConfigDBPassword%'); Write-Output $encryptedPassword"') do (
        set encrypted_password=%%A
    )
)

:: Handle default paths
if "%DesignerMetadataDatabase%"=="" set "DesignerMetadataDatabase=%default_path%"
if "%SiteInfoDatabase%"=="" set "SiteInfoDatabase=%default_path%"

set "DesignerMetadataDatabase=%DesignerMetadataDatabase%\Insite.mdb"
set "SiteInfoDatabase=%SiteInfoDatabase%\SiteInfo.mdb"

:: Move back to script directory
cd .\Resources\SQL Server

:: Executing CreatePurgingTables.bat asynchronously
call :log Starting CreatePurgingTables.bat
call CreatePurgingTables.bat %DatabaseHost% %PortNumber% "DPT" %ConfigDBUsername% %ConfigDBPassword% >> BatchExec.log
if errorlevel 1 (
    echo Error: CreatePurgingTables.bat failed.
    call :log Error: CreatePurgingTables.bat failed.
    if "%~1"=="" pause
    exit /b 1
)
call :log Completed CreatePurgingTables.bat

:: Executing CreateTableTypes.bat asynchronously
call :log Starting CreateTableTypes.bat
call CreateTableTypes.bat %DatabaseHost% %PortNumber% "DPT" %ConfigDBUsername% %ConfigDBPassword% >> BatchExec.log
if errorlevel 1 (
    echo Error: CreateTableTypes.bat failed.
    call :log Error: CreateTableTypes.bat failed.
    if "%~1"=="" pause
    exit /b 1
)
call :log Completed CreateTableTypes.bat

:: Executing CompileSourcePrograms.bat asynchronously
call :log Starting CompileSourcePrograms.bat
call CompileSourcePrograms.bat %DatabaseHost% %PortNumber% "DPT" %ConfigDBUsername% %ConfigDBPassword% >> BatchExec.log
if errorlevel 1 (
    echo Error: CompileSourcePrograms.bat failed.
	call :log Error: CompileSourcePrograms.bat failed.
	if "%~1"=="" pause
	exit /b 1
)
call :log Completed CompileSourcePrograms.bat

:: Executing CreateDBViews.bat asynchronously
call :log Starting CreateDBViews.bat
call CreateDBViews.bat %DatabaseHost% %PortNumber% "DPT" "dbo" %ConfigDBUsername% %ConfigDBPassword% %TxnDBName% %TxnDBSchema% >> BatchExec.log
if errorlevel 1 (
    echo Error: CreateDBViews.bat failed.
	call :log Error: CreateDBViews.bat failed.
	if "%~1"=="" pause
	exit /b 1
)
call :log Completed CreateDBViews.bat

:: Executes the SQL file to insert Txn and Archive records into the CSI_PURGEUTIL_CONFIG table
call :log Starting A500_CSI_PurgeUtil_InsertTxnArchiveDetails
SQLCMD -S %DatabaseHost%,%PortNumber% -d "DPT" -U %ConfigDBUsername% -P %ConfigDBPassword% -b -i "Scripts\A500_CSI_PurgeUtil_InsertTxnArchiveDetails.sql" -v TransactionDBName=%TxnDBName% TransactionDBSchema=%TxnDBSchema% ArchiveDBName=%ArchiveDBName% ArchiveDBSchema=%ArchiveDBSchema% >> BatchExec.log
if errorlevel 1 (
    echo Error: A500_CSI_PurgeUtil_InsertTxnArchiveDetails.sql failed.
	call :log Error: A500_CSI_PurgeUtil_InsertTxnArchiveDetails.sql failed.
	if "%~1"=="" pause
	exit /b 1
)
call :log Completed A500_CSI_PurgeUtil_InsertTxnArchiveDetails

echo Installation Completed.

:: Create XML config file
cd ..\..
set xml_file=%cd%\config.xml
(
  echo ^<?xml version="1.0" encoding="UTF-8"?^>
  echo ^<Credential^>
  echo   ^<DbType^>SQLServer^</DbType^>
  echo   ^<Host^>%DatabaseHost%^</Host^>
  echo   ^<Port^>%PortNumber%^</Port^>
  echo   ^<Username^>%ConfigDBUsername%^</Username^>
  echo   ^<Password^>%encrypted_password%^</Password^>
  echo ^</Credential^>
) > "%xml_file%"

echo Database configuration saved to %xml_file%
echo Installation Completed.

:: Only pause in interactive mode
if "%~1"=="" pause

:: Function to log with timestamps
cd .\Resources\SQL Server
:log
set "logfile=BatchExec.log"
set "timestamp=%date% %time:~0,8%"
set "args=%*"

:: Disable echo arguments with sensitve info during silent mode
echo %args% | findstr """" >nul 2>&1
if errorlevel 1 (
    echo [%timestamp%] %* >> "%logfile%"
)
exit /b