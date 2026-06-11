# -----------------------------------------------------------------------------------------------------------------------------
# 	Copyright Siemens 2024  
#
# 	DataStoreEmailSetUp.ps1
#
#  	Powershell script file used to run the DataStoreEmailSetUp.or.sql (Oracle) or DataStoreEmailSetUp.sql script (SQL Server).
#
# 	Assumptions:#
#     	%CamstarInstall%=C:\Program Files (x86)\Camstar
#     	Location of this .ps1 file (DataStoreEmailSetUp.ps1):
#      		%CamstarInstall%\InSite DataStore\Scripts
#     	Location of the SQL Scripts:
#          	DataStoreEmailSetUp.or.sql          (Oracle)        : %CamstarInstall%\InSite DataStore\Scripts\Oracle
#          	DataStoreEmailSetUp.sql             (SQL Server)    : %CamstarInstall%\InSite DataStore\Scripts\SQL Server
#     	Location of the log from the execution of this script:
#          	%CamstarInstall%\InSite DataStore\Scripts
#
#     	The appropriate Oracle tool (i.e. sqlplus) is in the user's %PATH% or SQL Server (i.e. sqlcmd) is in the user's %PATH%
#
# 	Execution:
#     	Run from Camstar DataStore directory (same as location of this .ps1 file): 
#       	%CamstarInstall%\InSite DataStore\Scripts
#     	When run with no parameters, the powershell script will prompt for the input values interactively. 
#     	Example:
#     		dataStoreEmailSetup.ps1  
#
#     	Run by passing in parameters from the command line:
#     	Examples:
#
#          	DataStoreEmailSetup.ps1 "<ORACLE>" "<Database Host>" "<DataStore Listening Port>" "<Database Name>" "<Admin User [defaults to SYS]>" "<Admin or SYS Password>" "<DataStore User Name>" "<Email Server>" "<Email Server port [Defaults to 25]>" "<Action [Defaults to ADD]>"
#          	DataStoreEmailSetup.ps1 "<SQLSERVER>" "<Database Host>" "<DataStore Listening Port>" "<Database Name>"  "<Admin User [defaults to SA]>" "<Admin or SA Password>" "<DataStore User Name>" "<Email Server>" "<Email Server port [Defaults to 25]>" "<Action [Defaults to ADD]>"
#
#          	(ORACLE):       .\DataStoreEmailSetUp.ps1 oracle APPDEVORA01.APPDEV.LOCAL 1521 appdev01 sys "Password" appdev01  mail_server.appdev.local 25 add
#          	(ORACLE):       .\DataStoreEmailSetUp.ps1 oracle APPDEVORA01.APPDEV.LOCAL 1521 appdev01 sys "Password" appdev01  mail_server.appdev.local 25 drop
#          	(SQLSERVER):    .\DataStoreEmailSetUp.ps1 sqlserver APPDEVSQL01.APPDEV.LOCAL 1433 appdev01 sa "Password" appdev01  mail_server.appdev.local 25 add
#          	(SQLSERVER):    .\DataStoreEmailSetUp.ps1 sqlserver APPDEVSQL01.APPDEV.LOCAL 1433 appdev01 sa "Password" appdev01  mail_server.appdev.local 25 drop
#			With named parameters (Number of input parameters and sequence can be changed):	
#			(ORACLE):       .\DataStoreEmailSetUp.ps1 -inputdatabasetype oracle -databaseserver APPDEVORA01.APPDEV.LOCAL -databaseport 1521 -databasename appdev01 -adminuser sys -adminpassword "password" -databaseuser appdev01 -emailserver mail_server.appdev.local -emailserverport 25 -inputaction add
#          	(ORACLE):       .\DataStoreEmailSetUp.ps1 -inputdatabasetype oracle -databaseserver APPDEVORA01.APPDEV.LOCAL -databaseport 1521 -databasename appdev01 -adminuser sys -adminpassword "password" -databaseuser appdev01 -emailserver mail_server.appdev.local -emailserverport 25 -inputaction drop
#          	(SQLSERVER):    .\DataStoreEmailSetUp.ps1 -inputdatabasetype sqlserver -databaseserver APPDEVSQL01.APPDEV.LOCAL -databaseport 1433 -databasename appdev01 -adminuser sa -adminpassword "password" -databaseuser appdev01 -emailserver mail_server.appdev.local -emailserverport 25 -inputaction add
#          	(SQLSERVER):    .\DataStoreEmailSetUp.ps1 -inputdatabasetype sqlserver -databaseserver APPDEVSQL01.APPDEV.LOCAL -databaseport 1433 -databasename appdev01 -adminuser sa -adminpassword "password" -databaseuser appdev01 -emailserver mail_server.appdev.local -emailserverport 25 -inputaction drop
#							
# 	Usage (Input Parameters): 
#
#     	Where:
#         	"<InputDatabaseType [ORACLE | oracle | SQLSERVER | sqlserver]>" - Is required.  The database vendor (i.e. ORACLE or SQLSERVER) 
#          	"<DatabaseServer>"                                              - Is required.  The Database Server Name or IP Address 
#          	"<DatabasePort>"                                     			- Is optional. 	The database Listening port. 
#                                                                         	- For Oracle, the default value is 1521.  
#                                                                             In interative mode, you may hit enter or type '' or "" to accept the default value.  
#                                                                             In command line mode you may enter '' or "" to accept the default value.
#                                                                         	- For SQL Server, the default value is 1433.  
#                                                                             In interactive mode, you may hit enter  or type '' or "" to accept the defualt value.   
#                                                                             In command line mode you may enter '' or "" to accept the default value.
#                                                                             For SQL Server, you may also enter the instance name for a Named SQL Server instance
#          	"<DatabaseName>"                                              	- Is required.  
#                                                                         	- For Oracle container databases instances, this is the service name for the pluggable database name for the DataStore
#                                                                         	- For Oracle non-container databases instances, this is the service name or the instance name
#                                                                         	- For SQL Server, this is the database name for the DataStore
#			"<AdminUser>"													- Is optional. Admin user for login.
#																			- For Oracle, the default value is SYS
#																			- For SQLSERVER, the default value is SA
#          	"<AdminPassword>"                                 				- Is required.  
#                                                                         	- For Oracle this is the SYS/Admin user password
#                                                                         	- For SQL Server this is the SA/Admin user password
#          	"<DatabaseUser>"                                        		- Is required.  
#                                                                         	- For Oracle this is the DataStore schema
#                                                                         	- For SQL Server this is the DataStore database user for SQL Server 
#          	"<EmailServer>"                                               	- Is required.  Email Server Name or IP Address. 
#			"<EmailServerPort>"												- Is optional. 	Email Server port defaults to 25.
#          	"<InputAction>"              									- Is optional.  Action to be performed defaults to ADD 
#
#     	ORACLE
#     	The input parameters "<DatabaseUser>", "<EmailServer>", "<EmailServerPort>" and "<InputAction>" will be passed into the DataStoreEmailSetUp.or.sql (Oracle) script
#     	These parameters will be used to set up an Access Control List and set permissions for the Oracle DataStore schema "<DataBaseUser>" to access the "<EmailServer>" with port "<EmailServerPort>"
#
#     	SQLSERVER
#     	The input parameters "<DatabaseUser>", "<EmailServer>", "<EmailServerPort>" and "<InputAction>" will be passed into the DataStoreEmailSetUp.sql (SQL Server) script
#     	These parameters will be used to set up an the Email Profile, Email Account with port "<EmailServerPort>" and set permissions for sending emails for the SQL Server DataStore User "<DatabaseUseR>" to access the "<Email Server>" 
#
# 	Error Detection:  
#
#     	Exit status of sqlplus or sqlcmd is checked. A comment indicating an error occurred is written to the log file
#
# 	Modification History:
#   Name                        Date            Action
#   ----------------------------------------------------------------------------------------------------------------------------------------------------
#	MBhandari					16/10/2023		New Powershell script	
#
#	----------------------------------------------------------------------------------------------------------------------------------------------------

Param ([string]$inputDatabaseType,[string]$databaseServer, [int32]$databasePort, [string]$databaseName, [string]$adminUser, [string]$adminPassword, [string]$databaseUser, [string]$emailServer, [int32]$emailServerPort, [string]$inputAction)

Function ValidateDBType {
Param(
[Parameter(Mandatory=$true)]
[ValidateNotNullOrEmpty()]
[ValidateSet("ORACLE", "SQLSERVER")]
[String[]]$databaseType
)  
	$databaseType	
}
do {	
	try {		
			# If input is given, verify for valid values and revert.
			# Different variable is used to ensure that this validation works only once and validation for the prompted value is not done here.
			if ($inputDatabaseType)
			{
				$databaseType = $inputDatabaseType
				if (-not($inputDatabaseType.toUpper() -eq "ORACLE" -or $inputDatabaseType.toUpper() -eq "SQLSERVER"))
				{
					Write-Warning 'Invalid database type entered, allowed values are Oracle or SQLServer.'
					$inputDatabaseType = ""
				}
			}
			# Prompt the user for database type if input value is not given or the given value is not valid.
			if (-not($inputDatabaseType) -or (-not($databaseType.toUpper() -eq "ORACLE" -or $databaseType.toUpper() -eq "SQLSERVER")))
			{
				$databaseType = Read-Host 'Please enter Datastore Database Type'
			}			
		$databaseType = validateDBType($databaseType)
		}
		Catch {				
				Write-Warning 'Invalid database type entered, allowed values are Oracle or SQLServer.'
		}
	} while (-not($databaseType.toUpper() -eq "ORACLE" -or $databaseType.toUpper() -eq "SQLSERVER"))

if (-not($databaseServer))
{
	$databaseServer= Read-Host -Prompt  "Enter DataStore Database Server Name or IP Address" 
}
if (-not($databasePort))
{
	if ( $databaseType.toUpper() -eq "ORACLE")
	{
		$databasePort= Read-Host -Prompt  "Enter DataStore Listening Port (default 1521) "
		if (-not($databasePort))
		{
			$databasePort = 1521
		}
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		$databasePort= Read-Host -Prompt  "Enter DataStore Listening Port or Named Instance (default 1433) "
		if (-not($databasePort))
		{
			$databasePort = 1433
		}
	}
}
if (-not($databaseName))
{
	$databaseName= Read-Host -Prompt  "Enter DataStore Database Name " 
}
if (-not($adminUser))
{
	if ( $databaseType.toUpper() -eq "ORACLE")
	{
		$adminUser= Read-Host -Prompt  "Enter Oracle Admin User (default SYS) "
		if (-not($adminUser))
		{
			$adminUser = "SYS"
		}
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		$adminUser= Read-Host -Prompt  "Enter SQL Server Admin User (default SA) "
		if (-not($adminUser))
		{
			$adminUser = "SA"
		}
	}
}
if (-not($adminPassword))
{
	if ( $databaseType.toUpper() -eq "ORACLE")
	{
		$SysPassword= Read-Host -Prompt  "Enter Oracle Admin password " -AsSecureString
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		$SysPassword= Read-Host -Prompt  "Enter SQL Server Admin password " -AsSecureString
	}
}
if (-not($databaseUser))
{
	if ( $databaseType.toUpper() -eq "ORACLE")
	{
		$databaseUser= Read-Host -Prompt  "Enter Datastore Schema "
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		$databaseUser= Read-Host -Prompt  "Enter Datastore User "
	}
}
if (-not($emailServer))
{
	$emailServer= Read-Host -Prompt  "Enter email server "
}
if (-not($emailServerPort))
{
	$emailServerPort= Read-Host -Prompt  "Enter email server port (default 25)"
	if (-not($emailServerPort))
	{
		$emailServerPort = 25
	}
}
Function ValidateAction {
Param(
#[Parameter(Mandatory=$true)]
[ValidateSet("ADD", "DROP", "")]
[String[]]$action
)    
    $action
}

do {	
	try {		
			# If input is given, verify for valid values and revert.
			# Different variable is used to ensure that this validation works only once and validation for the prompted value is not done here.
			if ($inputAction)
			{
				$action = $inputAction
				if (-not($inputAction.toUpper() -eq "ADD" -or $inputAction.toUpper() -eq "DROP"))
				{
					Write-Warning 'Invalid action entered, allowed values are "Add" or "Drop". Re-enter a value'
					$inputAction = ""
				}
			}
			# Prompt the user for database type if input value is not given or the given value is not valid.
			if (-not($inputAction) -OR (-not($action.toUpper() -eq "ADD" -or $action.toUpper() -eq "DROP")))
			{
				$action = Read-Host 'Please enter action  (default Add)'
				if (-not($action))
				{
					$action = "ADD"
				}
			}			
		$action = validateAction($action)
		}
		Catch {				
				Write-Warning 'Invalid action entered, allowed values are "Add" or "Drop". Re-enter a value'
		}
	} while (-not($action.toUpper() -eq "ADD" -or $action.toUpper() -eq "DROP"))

Function Write-Log {
    [CmdletBinding()]
    Param(
    [Parameter(Mandatory=$False)]
    [ValidateSet("INFO","WARN","ERROR")]
    [String]
    $Level = "INFO",
    [Parameter(Mandatory=$True)]
    [string]
    $Message,
    [Parameter(Mandatory=$False)]
    [string]
    $logfile)

	# $Stamp = (Get-Date).toString("yyyy/MM/dd HH:mm:ss")
	# $Line = "$Stamp $Level $Message"
	$Line = "$Message"
    If($logfile) {
        Add-Content $logfile -Value $Line
    }
    Else {
        Write-Output $Line
    }
}
$datastorePath = $pwd.path
echo $datastorePath

$logFile = $datastorePath + "\DataStoreEmailSetUp_" + $databaseType + "_" + $databaseName+"_"+$databaseUser+".log"
echo $logFile

$orclDBScript = $datastorePath+"\Oracle\DataStoreEmailSetUp.or.sql"
$sqlDBScript = $datastorePath+"\SQL Server\DataStoreEmailSetUp.sql"
$orclInitDBScript = $datastorePath+"\Oracle\initiate_DataStoreEmailSetUp.or.sql"
$sqlInitDBScript = $datastorePath+"\Oracle\initiate_DataStoreEmailSetUp.or.sql"
$sqlplusLogFile = $datastorePath+"\DataStoreEmailSetUp.log"
$sqlcmdLogFile = $datastorePath+"\DataStoreEmailSetUp.log"

if (Test-Path $logFile) {
        Remove-Item $logFile -Force -Recurse
    }
	
If (!(Test-Path $logFile)) {
        New-Item -Path $logFile | Out-Null
    }
	
Write-Log -level "INFO" -message "© Siemens 2023" -logfile $logFile

$Stamp = (Get-Date).toString("yyyy/MM/dd HH:mm:ss")
Write-Log -level "INFO" -message "Start Time : $Stamp" -logfile $logFile

Write-Log -level "INFO" -message "Datastore home : $datastorePath" -logfile $logFile

Write-Log -level "INFO" -message "[DB Script information ]" -logfile $logFile

if ( $databaseType.toUpper() -eq "ORACLE")
{
	Write-Log -level "INFO" -message " $orclDBScript " -logfile $logFile
}
elseif ( $databaseType.toUpper() -eq "SQLSERVER")
{
	Write-Log -level "INFO" -message " $sqlDBScript " -logfile $logFile
}
Write-Log -level "INFO" -message " [Log file information] " -logfile $logFile
Write-Log -level "INFO" -message " $logFile " -logfile $logFile
Write-Log -level "INFO" -message "Generated in directory: ""$datastorePath""" -logfile $logFile

if ($databaseType.toUpper() -eq "ORACLE")
{
	Write-Log -level "INFO" -message "Sqlplus Log file : ""$sqlplusLogFile""" -logfile $logFile
}
elseif ($databaseType.toUpper() -eq "SQLSERVER")
{
	Write-Log -level "INFO" -message "Sqlcmd Log file: ""$sqlcmdLogFile""" -logfile $logFile
}
Write-Log -level "INFO" -message "Generated in directory: ""$datastorePath""" -logfile $logFile

if (-not($SysPassword) -and -not($adminPassword))
{
	$SysPassword="unspecified"
}
if (-not($databaseServer))
{
	$databaseServer="unspecified"
}
if (-not($databaseUser))
{
	$databaseUser="unspecified"
}	
if (-not($emailServer))
{
	$emailServer = "unspecified"
}

function validateInput
{
	Param(
    [Parameter(Mandatory=$False)]
    [decimal]
	$ErrCase)

	if ($SysPassword -eq "unspecified")
	{
		$ErrCase = 1
		Write-Log -level "ERROR" -message "Password not provided." -logfile $logFile
	}
	if ($databaseServer -eq "unspecified")
	{
		$ErrCase = 1
		Write-Log -level "ERROR" -message "Database Server not provided." -logfile $logFile
	}
	if ($databaseUser -eq "unspecified")
	{
		$ErrCase = 1
		Write-Log -level "ERROR" -message "Database User not provided." -logfile $logFile
	}	
	if ($emailServer -eq "unspecified")
	{		
		$ErrCase = 1
		Write-Log -level "ERROR" -message "Email Server not provided." -logfile $logFile
	}
	return $ErrCase
}
$ErrCase = 0
$ErrCase = validateInput ($ErrCase)

if ($ErrCase -eq 1)
{
	Write-Log -level "ERROR" -message "Email setup script aborted." -logfile $logFile
}
else
{
	Write-Log -level "INFO" -message "[Validated input ] " -logfile $logFile
	Write-Log -level "INFO" -message "Database Vendor 										: $databaseType" -logfile $logFile
	Write-Log -level "INFO" -message "DataStore Database Server (Host Name or IP Address) 	: $databaseServer" -logfile $logFile
	Write-Log -level "INFO" -message "DataStore Listening Port 								: $databasePort" -logfile $logFile
	Write-Log -level "INFO" -message "DataStore Database Name 								: $databaseName" -logfile $logFile

	if ( $databaseType.toUpper() -eq "ORACLE")
	{
		Write-Log -level "INFO" -message "DataStore Schema 										: $databaseUser" -logfile $logFile
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		Write-Log -level "INFO" -message "DataStore Database User 								: $databaseUser" -logfile $logFile
	}
	
	Write-Log -level "INFO" -message "Email Server 											: $emailServer" -logfile $logFile	
	Write-Log -level "INFO" -message "Email Server Port										: $emailServerPort" -logfile $logFile
	Write-Log -level "INFO" -message "Action 													: $action" -logfile $logFile
	
	if (-not($adminPassword))
	{
		$adminPassword =[Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SysPassword))
	}
	Write-Log -level "INFO" -message "continued." -logfile $logFile	
	Write-Log -level "INFO" -message "[ Database script execution ] " -logfile $logFile
	
	if ( $databaseType.toUpper() -eq "ORACLE")
	{	
		try 
		{
			Write-Log -level "INFO" -message "Executing Script: ""$orclDBScript"" " -logfile $logFile
			$sqlConn = $adminUser +"/"+ $adminPassword+ "@" +$databaseServer + ":" + $databasePort + "/" + $databaseName
			if ($adminUser.toUpper() -eq "SYS")
			{
				$sqlConn = $sqlConn + " as sysdba"
			}
			
			$sqlfile = "@`"" + $orclDBScript + "`"" + " '$databaseUser' '$emailServer' '$action' '$emailServerPort' "
			$sqlfile | sqlplus $sqlConn | add-content $logFile;
			Write-Log -level "INFO" -message "Sqlplus Log file : ""$sqlplusLogFile""" -logfile $logFile
		}
		catch
		{
			$ErrorMessage = $_.Exception.Message
			echo $ErrorMessage
			Write-Log -level "ERROR" -message $ErrorMessage -logfile $logFile
		}
	}
	elseif ( $databaseType.toUpper() -eq "SQLSERVER")
	{
		try 
		{
			Write-Log -level "INFO" -message "Executing Script: ""$sqlDBScript"" " -logfile $logFile
			sqlcmd -S $databaseServer,$databasePort -U $adminUser -P $adminPassword -i $sqlDBScript -v DATASTORE_USERNAME="$databaseUser" EMAIL_SERVER="$emailServer" ACTION=$action  EMAIL_SERVER_PORT=$emailServerPort -o $sqlcmdLogFile			
			
			$From = Get-Content -Path $sqlcmdLogFile
			Add-Content -Path $logFile -Value $From
			
			Write-Log -level "INFO" -message "SqlCmd Log file : ""$sqlcmdLogFile""" -logfile $logFile
		}
		catch
		{
			$ErrorMessage = $_.Exception.Message
			Write-Log -level "ERROR" -message $ErrorMessage -logfile $logFile			
		}
	}
}
# SIG # Begin signature block
# MIIpeQYJKoZIhvcNAQcCoIIpajCCKWYCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCCPixoWaytHHhHB
# 4yNZnOCMtOAyR5jhdYe3j11F2FFRSqCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
# TJ9ezam9k67ZMA0GCSqGSIb3DQEBDAUAMGIxCzAJBgNVBAYTAlVTMRUwEwYDVQQK
# EwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xITAfBgNV
# BAMTGERpZ2lDZXJ0IFRydXN0ZWQgUm9vdCBHNDAeFw0yMTA0MjkwMDAwMDBaFw0z
# NjA0MjgyMzU5NTlaMGkxCzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwg
# SW5jLjFBMD8GA1UEAxM4RGlnaUNlcnQgVHJ1c3RlZCBHNCBDb2RlIFNpZ25pbmcg
# UlNBNDA5NiBTSEEzODQgMjAyMSBDQTEwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAw
# ggIKAoICAQDVtC9C0CiteLdd1TlZG7GIQvUzjOs9gZdwxbvEhSYwn6SOaNhc9es0
# JAfhS0/TeEP0F9ce2vnS1WcaUk8OoVf8iJnBkcyBAz5NcCRks43iCH00fUyAVxJr
# Q5qZ8sU7H/Lvy0daE6ZMswEgJfMQ04uy+wjwiuCdCcBlp/qYgEk1hz1RGeiQIXhF
# LqGfLOEYwhrMxe6TSXBCMo/7xuoc82VokaJNTIIRSFJo3hC9FFdd6BgTZcV/sk+F
# LEikVoQ11vkunKoAFdE3/hoGlMJ8yOobMubKwvSnowMOdKWvObarYBLj6Na59zHh
# 3K3kGKDYwSNHR7OhD26jq22YBoMbt2pnLdK9RBqSEIGPsDsJ18ebMlrC/2pgVItJ
# wZPt4bRc4G/rJvmM1bL5OBDm6s6R9b7T+2+TYTRcvJNFKIM2KmYoX7BzzosmJQay
# g9Rc9hUZTO1i4F4z8ujo7AqnsAMrkbI2eb73rQgedaZlzLvjSFDzd5Ea/ttQokbI
# YViY9XwCFjyDKK05huzUtw1T0PhH5nUwjewwk3YUpltLXXRhTT8SkXbev1jLchAp
# QfDVxW0mdmgRQRNYmtwmKwH0iU1Z23jPgUo+QEdfyYFQc4UQIyFZYIpkVMHMIRro
# OBl8ZhzNeDhFMJlP/2NPTLuqDQhTQXxYPUez+rbsjDIJAsxsPAxWEQIDAQABo4IB
# WTCCAVUwEgYDVR0TAQH/BAgwBgEB/wIBADAdBgNVHQ4EFgQUaDfg67Y7+F8Rhvv+
# YXsIiGX0TkIwHwYDVR0jBBgwFoAU7NfjgtJxXWRM3y5nP+e6mK4cD08wDgYDVR0P
# AQH/BAQDAgGGMBMGA1UdJQQMMAoGCCsGAQUFBwMDMHcGCCsGAQUFBwEBBGswaTAk
# BggrBgEFBQcwAYYYaHR0cDovL29jc3AuZGlnaWNlcnQuY29tMEEGCCsGAQUFBzAC
# hjVodHRwOi8vY2FjZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRUcnVzdGVkUm9v
# dEc0LmNydDBDBgNVHR8EPDA6MDigNqA0hjJodHRwOi8vY3JsMy5kaWdpY2VydC5j
# b20vRGlnaUNlcnRUcnVzdGVkUm9vdEc0LmNybDAcBgNVHSAEFTATMAcGBWeBDAED
# MAgGBmeBDAEEATANBgkqhkiG9w0BAQwFAAOCAgEAOiNEPY0Idu6PvDqZ01bgAhql
# +Eg08yy25nRm95RysQDKr2wwJxMSnpBEn0v9nqN8JtU3vDpdSG2V1T9J9Ce7FoFF
# UP2cvbaF4HZ+N3HLIvdaqpDP9ZNq4+sg0dVQeYiaiorBtr2hSBh+3NiAGhEZGM1h
# mYFW9snjdufE5BtfQ/g+lP92OT2e1JnPSt0o618moZVYSNUa/tcnP/2Q0XaG3Ryw
# YFzzDaju4ImhvTnhOE7abrs2nfvlIVNaw8rpavGiPttDuDPITzgUkpn13c5Ubdld
# AhQfQDN8A+KVssIhdXNSy0bYxDQcoqVLjc1vdjcshT8azibpGL6QB7BDf5WIIIJw
# 8MzK7/0pNVwfiThV9zeKiwmhywvpMRr/LhlcOXHhvpynCgbWJme3kuZOX956rEnP
# LqR0kq3bPKSchh/jwVYbKyP/j7XqiHtwa+aguv06P0WmxOgWkVKLQcBIhEuWTatE
# QOON8BUozu3xGFYHKi8QxAwIZDwzj64ojDzLj4gLDb879M4ee47vtevLt/B3E+bn
# KD+sEq6lLyJsQfmCXBVmzGwOysWGw/YmMwwHS6DTBwJqakAwSEs0qFEgu60bhQji
# WQ1tygVQK+pKHJ6l/aCnHwZ05/LWUpD9r4VIIflXO7ScA+2GRfS0YW6/aOImYIbq
# yK+p/pQd52MbOoZWeE4wggd3MIIFX6ADAgECAhADlxGexb7h6t0SRpEj2gIUMA0G
# CSqGSIb3DQEBCwUAMGkxCzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwg
# SW5jLjFBMD8GA1UEAxM4RGlnaUNlcnQgVHJ1c3RlZCBHNCBDb2RlIFNpZ25pbmcg
# UlNBNDA5NiBTSEEzODQgMjAyMSBDQTEwHhcNMjUwMTE0MDAwMDAwWhcNMjYwMTI4
# MjM1OTU5WjB/MQswCQYDVQQGEwJVUzEOMAwGA1UECBMFVGV4YXMxDjAMBgNVBAcT
# BVBsYW5vMScwJQYDVQQKEx5TaWVtZW5zIEluZHVzdHJ5IFNvZnR3YXJlIEluYy4x
# JzAlBgNVBAMTHlNpZW1lbnMgSW5kdXN0cnkgU29mdHdhcmUgSW5jLjCCAiIwDQYJ
# KoZIhvcNAQEBBQADggIPADCCAgoCggIBAMm25s/HWC0nPtAs+OR/1sR6Hu+esYcZ
# w3DfWlkNvnLH50SRLa2TOsUTGKFcbM4DjzGrkZXUb7RypjznkX81i0nqcAtLI9pg
# xPuR3xkj3T2k4Bg7NSrHdchTckIdV/LhtMWfBS/wFHhRZVC3q3aFbzl+fFuVLYtU
# trZa2QkgnGWnwTlkxfLaSlfJ2Gi+H3diPd9X0P51wKUW6C9zQy4XaGShUZ/bPUUJ
# 1MbEqqURtyx7uWuFrVQkOT8/ccy7CerSMCyoQkiR/PY+hPo91jbggk72k75PuTQt
# bbJSkwEin7HYy5VClHVwuTgzGqvQijoIouNwYyibD3DIuQeMw5Lf4n75qUHh6thn
# urRk30OF+LrluS0NNjuKwPoHTqfjkqZLk2Fz2ygwHtHw3te4zXMRfsn8gsIjnJ5e
# wxCkLtZXbVH+WRefjrQlA5VjGeTrMEzrRTGUW4yctURIIH2h8BWseGsIVR1S5aq6
# r6Zqnzkve/be5BF6MoiOzZ+2IcYrC5IbeON9vGItqioiweBx/7oJuKr5tiCK9dyw
# iWs8YL/WazIo3eFSBdR2WcASF4ifcTVTh8LT+tCsIwtGvmE29GrknuvvIoojaQ+M
# NWunTa/A9EzU4ClIlW2GTCnXTy751xMu5xugoHZCOJfcHWAKNZJgQruJy6XtvTiA
# 8lfDh3dBw7TlAgMBAAGjggIDMIIB/zAfBgNVHSMEGDAWgBRoN+Drtjv4XxGG+/5h
# ewiIZfROQjAdBgNVHQ4EFgQUngvoXIKVPEphUu8+FM9P/Gy4O4wwPgYDVR0gBDcw
# NTAzBgZngQwBBAEwKTAnBggrBgEFBQcCARYbaHR0cDovL3d3dy5kaWdpY2VydC5j
# b20vQ1BTMA4GA1UdDwEB/wQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAzCBtQYD
# VR0fBIGtMIGqMFOgUaBPhk1odHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNl
# cnRUcnVzdGVkRzRDb2RlU2lnbmluZ1JTQTQwOTZTSEEzODQyMDIxQ0ExLmNybDBT
# oFGgT4ZNaHR0cDovL2NybDQuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZEc0
# Q29kZVNpZ25pbmdSU0E0MDk2U0hBMzg0MjAyMUNBMS5jcmwwgZQGCCsGAQUFBwEB
# BIGHMIGEMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20wXAYI
# KwYBBQUHMAKGUGh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRy
# dXN0ZWRHNENvZGVTaWduaW5nUlNBNDA5NlNIQTM4NDIwMjFDQTEuY3J0MAkGA1Ud
# EwQCMAAwDQYJKoZIhvcNAQELBQADggIBALIuMtkUuvimRZk4WvbiIAOyaGz7IWDx
# SB8EypgxtF1osjB4RVCNyKgBggC0bEDoq/Q5UVI3lEZe4Ps/iJIFQ9aWg6lWSZRB
# mYw8mhmit1Qq+QcqgPX7KZTFe3ccaLKMvIfv97s8R4fxAPiPkV6QgnYW4kIVZ0l/
# dwg68YaTJ0/gMD2A92n2i+szxeaFNch9Z1XiQtvG9E346DqqUo5fbuqvMQhhsOaK
# TbGU15b7UKEnY2aoLSzXEvIaDO7rK9zoCrvALAGE8L+HHcfKAE2Xyg6Un/flZhC0
# ylE5xCsfkmUEOMn7a3joTnVodVG1aqjNL221MxrSNk8xTGelDURdMiLn/+FTbifv
# 9+jE3G6Y5xE/79K3YuGTHGWLK2cZwBl4wAHEaZBahgFoSHNZerfwNKcImOWXNa/y
# 9MOJntkKcccz+NzNO8R15M4sBAPdfNYMS3rfL85Ek01bU8torFpfhIr8hRxQOzRv
# mdzfAEFcHmToUjgLVP/lB96zohlvuqzRRumTAQBipDEH6RN95nfJgT6ys0b/EtOE
# To3UnBWa6uofrSjjC63wqFHpjf8o7cqxI/zxnVfFR86TdvV6XF8pJXrlbVoS4R9y
# Bam4kHHh82v3i++AW5AqA5Jm4eczClm6slPYwgmzTMeP5VBCHJYxhCjEoXhQMj8Q
# sz+9VNMWTdxGMYIaoDCCGpwCAQEwfTBpMQswCQYDVQQGEwJVUzEXMBUGA1UEChMO
# RGlnaUNlcnQsIEluYy4xQTA/BgNVBAMTOERpZ2lDZXJ0IFRydXN0ZWQgRzQgQ29k
# ZSBTaWduaW5nIFJTQTQwOTYgU0hBMzg0IDIwMjEgQ0ExAhADlxGexb7h6t0SRpEj
# 2gIUMA0GCWCGSAFlAwQCAQUAoHwwEAYKKwYBBAGCNwIBDDECMAAwGQYJKoZIhvcN
# AQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUw
# LwYJKoZIhvcNAQkEMSIEIIsn8GVbs4stEe7Iz0ket17hfp5CPQPzKvlUKE/+rerE
# MA0GCSqGSIb3DQEBAQUABIICAAQE6rVx7PgFUJTcdmnqoBJ/RcO6YGYYtd1D+HWZ
# tXC0bJwgTgEhZQpAyRm6a6UpHSEaPL0XIEGEVIZVuARUjwQrXi/mlVrHMVoqW7hB
# jgdrVJL611FG3jxeg6jx8JhoIZHFArnDnp5I1slugpXr3LLc+dK0NjAkdLnyQ6k4
# w8ULrzuZpbpoyKl2tFr+EW6tKwCiwvyDPwlx/1rvObwiPQc8/Yx2GCgc29bQJ1vR
# fsCy2MhYvrlP4wFZj+0YMmi4gCcxrYEfbiEuAYong4nXs6fHw8FgKxWbVByeZn1t
# TcMCZIneJrxs3gy7vHF3iEuWma8y78MlvTZ7HazOtdZpT9H94KDA18DMplQiAKbf
# WlnGNQGPOD+i10C8UImI4g7woZjJZrgtS8bTj6AB+m1CD6ggOqy0YqEq4GUNfIPK
# G9y3quT/caGSiH5fHhBzP+wrzMlBPqmQzqULkGsyA8bIMsuLuOWeCn/idYQV0BnX
# MdajyPh9PNKCML0QS1rZrIu/Qof77KqjEs5FENWC7YHINhwDtRMN70zH7bG3CRKz
# oi7XULn3RiXL3Zce6tsm3/zKuF15p517q6aZOR6Xnb/V5bSx4Z2FJnAo/UhtwV1e
# A0rRm0aE5YfLnmvXbgn1IJj/WM29KVI0kUzXIP2vbuO6CdW5egNT9n6vQOZhd33K
# +6nboYIXdjCCF3IGCisGAQQBgjcDAwExghdiMIIXXgYJKoZIhvcNAQcCoIIXTzCC
# F0sCAQMxDzANBglghkgBZQMEAgEFADB3BgsqhkiG9w0BCRABBKBoBGYwZAIBAQYJ
# YIZIAYb9bAcBMDEwDQYJYIZIAWUDBAIBBQAEIMFzXJX26l0De08VNGgdyedAnruo
# KWZ/7dMWoW8vke3KAhB1H5KyV8bY+CAl8i5UIVL1GA8yMDI1MTIyNDAzMDk1M1qg
# ghM6MIIG7TCCBNWgAwIBAgIQCoDvGEuN8QWC0cR2p5V0aDANBgkqhkiG9w0BAQsF
# ADBpMQswCQYDVQQGEwJVUzEXMBUGA1UEChMORGlnaUNlcnQsIEluYy4xQTA/BgNV
# BAMTOERpZ2lDZXJ0IFRydXN0ZWQgRzQgVGltZVN0YW1waW5nIFJTQTQwOTYgU0hB
# MjU2IDIwMjUgQ0ExMB4XDTI1MDYwNDAwMDAwMFoXDTM2MDkwMzIzNTk1OVowYzEL
# MAkGA1UEBhMCVVMxFzAVBgNVBAoTDkRpZ2lDZXJ0LCBJbmMuMTswOQYDVQQDEzJE
# aWdpQ2VydCBTSEEyNTYgUlNBNDA5NiBUaW1lc3RhbXAgUmVzcG9uZGVyIDIwMjUg
# MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBANBGrC0Sxp7Q6q5gVrMr
# V7pvUf+GcAoB38o3zBlCMGMyqJnfFNZx+wvA69HFTBdwbHwBSOeLpvPnZ8ZN+vo8
# dE2/pPvOx/Vj8TchTySA2R4QKpVD7dvNZh6wW2R6kSu9RJt/4QhguSssp3qome7M
# rxVyfQO9sMx6ZAWjFDYOzDi8SOhPUWlLnh00Cll8pjrUcCV3K3E0zz09ldQ//nBZ
# ZREr4h/GI6Dxb2UoyrN0ijtUDVHRXdmncOOMA3CoB/iUSROUINDT98oksouTMYFO
# nHoRh6+86Ltc5zjPKHW5KqCvpSduSwhwUmotuQhcg9tw2YD3w6ySSSu+3qU8DD+n
# igNJFmt6LAHvH3KSuNLoZLc1Hf2JNMVL4Q1OpbybpMe46YceNA0LfNsnqcnpJeIt
# K/DhKbPxTTuGoX7wJNdoRORVbPR1VVnDuSeHVZlc4seAO+6d2sC26/PQPdP51ho1
# zBp+xUIZkpSFA8vWdoUoHLWnqWU3dCCyFG1roSrgHjSHlq8xymLnjCbSLZ49kPmk
# 8iyyizNDIXj//cOgrY7rlRyTlaCCfw7aSUROwnu7zER6EaJ+AliL7ojTdS5PWPsW
# eupWs7NpChUk555K096V1hE0yZIXe+giAwW00aHzrDchIc2bQhpp0IoKRR7YufAk
# prxMiXAJQ1XCmnCfgPf8+3mnAgMBAAGjggGVMIIBkTAMBgNVHRMBAf8EAjAAMB0G
# A1UdDgQWBBTkO/zyMe39/dfzkXFjGVBDz2GM6DAfBgNVHSMEGDAWgBTvb1NK6eQG
# fHrK4pBW9i/USezLTjAOBgNVHQ8BAf8EBAMCB4AwFgYDVR0lAQH/BAwwCgYIKwYB
# BQUHAwgwgZUGCCsGAQUFBwEBBIGIMIGFMCQGCCsGAQUFBzABhhhodHRwOi8vb2Nz
# cC5kaWdpY2VydC5jb20wXQYIKwYBBQUHMAKGUWh0dHA6Ly9jYWNlcnRzLmRpZ2lj
# ZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRHNFRpbWVTdGFtcGluZ1JTQTQwOTZTSEEy
# NTYyMDI1Q0ExLmNydDBfBgNVHR8EWDBWMFSgUqBQhk5odHRwOi8vY3JsMy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRUcnVzdGVkRzRUaW1lU3RhbXBpbmdSU0E0MDk2U0hB
# MjU2MjAyNUNBMS5jcmwwIAYDVR0gBBkwFzAIBgZngQwBBAIwCwYJYIZIAYb9bAcB
# MA0GCSqGSIb3DQEBCwUAA4ICAQBlKq3xHCcEua5gQezRCESeY0ByIfjk9iJP2zWL
# pQq1b4URGnwWBdEZD9gBq9fNaNmFj6Eh8/YmRDfxT7C0k8FUFqNh+tshgb4O6Lgj
# g8K8elC4+oWCqnU/ML9lFfim8/9yJmZSe2F8AQ/UdKFOtj7YMTmqPO9mzskgiC3Q
# YIUP2S3HQvHG1FDu+WUqW4daIqToXFE/JQ/EABgfZXLWU0ziTN6R3ygQBHMUBaB5
# bdrPbF6MRYs03h4obEMnxYOX8VBRKe1uNnzQVTeLni2nHkX/QqvXnNb+YkDFkxUG
# tMTaiLR9wjxUxu2hECZpqyU1d0IbX6Wq8/gVutDojBIFeRlqAcuEVT0cKsb+zJNE
# suEB7O7/cuvTQasnM9AWcIQfVjnzrvwiCZ85EE8LUkqRhoS3Y50OHgaY7T/lwd6U
# Arb+BOVAkg2oOvol/DJgddJ35XTxfUlQ+8Hggt8l2Yv7roancJIFcbojBcxlRcGG
# 0LIhp6GvReQGgMgYxQbV1S3CrWqZzBt1R9xJgKf47CdxVRd/ndUlQ05oxYy2zRWV
# FjF7mcr4C34Mj3ocCVccAvlKV9jEnstrniLvUxxVZE/rptb7IRE2lskKPIJgbaP5
# t2nGj/ULLi49xTcBZU8atufk+EMF/cWuiC7POGT75qaL6vdCvHlshtjdNXOCIUjs
# arfNZzCCBrQwggScoAMCAQICEA3HrFcF/yGZLkBDIgw6SYYwDQYJKoZIhvcNAQEL
# BQAwYjELMAkGA1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0IEluYzEZMBcGA1UE
# CxMQd3d3LmRpZ2ljZXJ0LmNvbTEhMB8GA1UEAxMYRGlnaUNlcnQgVHJ1c3RlZCBS
# b290IEc0MB4XDTI1MDUwNzAwMDAwMFoXDTM4MDExNDIzNTk1OVowaTELMAkGA1UE
# BhMCVVMxFzAVBgNVBAoTDkRpZ2lDZXJ0LCBJbmMuMUEwPwYDVQQDEzhEaWdpQ2Vy
# dCBUcnVzdGVkIEc0IFRpbWVTdGFtcGluZyBSU0E0MDk2IFNIQTI1NiAyMDI1IENB
# MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBALR4MdMKmEFyvjxGwBys
# ddujRmh0tFEXnU2tjQ2UtZmWgyxU7UNqEY81FzJsQqr5G7A6c+Gh/qm8Xi4aPCOo
# 2N8S9SLrC6Kbltqn7SWCWgzbNfiR+2fkHUiljNOqnIVD/gG3SYDEAd4dg2dDGpeZ
# GKe+42DFUF0mR/vtLa4+gKPsYfwEu7EEbkC9+0F2w4QJLVSTEG8yAR2CQWIM1iI5
# PHg62IVwxKSpO0XaF9DPfNBKS7Zazch8NF5vp7eaZ2CVNxpqumzTCNSOxm+SAWSu
# Ir21Qomb+zzQWKhxKTVVgtmUPAW35xUUFREmDrMxSNlr/NsJyUXzdtFUUt4aS4CE
# eIY8y9IaaGBpPNXKFifinT7zL2gdFpBP9qh8SdLnEut/GcalNeJQ55IuwnKCgs+n
# rpuQNfVmUB5KlCX3ZA4x5HHKS+rqBvKWxdCyQEEGcbLe1b8Aw4wJkhU1JrPsFfxW
# 1gaou30yZ46t4Y9F20HHfIY4/6vHespYMQmUiote8ladjS/nJ0+k6MvqzfpzPDOy
# 5y6gqztiT96Fv/9bH7mQyogxG9QEPHrPV6/7umw052AkyiLA6tQbZl1KhBtTasyS
# kuJDpsZGKdlsjg4u70EwgWbVRSX1Wd4+zoFpp4Ra+MlKM2baoD6x0VR4RjSpWM8o
# 5a6D8bpfm4CLKczsG7ZrIGNTAgMBAAGjggFdMIIBWTASBgNVHRMBAf8ECDAGAQH/
# AgEAMB0GA1UdDgQWBBTvb1NK6eQGfHrK4pBW9i/USezLTjAfBgNVHSMEGDAWgBTs
# 1+OC0nFdZEzfLmc/57qYrhwPTzAOBgNVHQ8BAf8EBAMCAYYwEwYDVR0lBAwwCgYI
# KwYBBQUHAwgwdwYIKwYBBQUHAQEEazBpMCQGCCsGAQUFBzABhhhodHRwOi8vb2Nz
# cC5kaWdpY2VydC5jb20wQQYIKwYBBQUHMAKGNWh0dHA6Ly9jYWNlcnRzLmRpZ2lj
# ZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRSb290RzQuY3J0MEMGA1UdHwQ8MDowOKA2
# oDSGMmh0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRSb290
# RzQuY3JsMCAGA1UdIAQZMBcwCAYGZ4EMAQQCMAsGCWCGSAGG/WwHATANBgkqhkiG
# 9w0BAQsFAAOCAgEAF877FoAc/gc9EXZxML2+C8i1NKZ/zdCHxYgaMH9Pw5tcBnPw
# 6O6FTGNpoV2V4wzSUGvI9NAzaoQk97frPBtIj+ZLzdp+yXdhOP4hCFATuNT+ReOP
# K0mCefSG+tXqGpYZ3essBS3q8nL2UwM+NMvEuBd/2vmdYxDCvwzJv2sRUoKEfJ+n
# N57mQfQXwcAEGCvRR2qKtntujB71WPYAgwPyWLKu6RnaID/B0ba2H3LUiwDRAXx1
# Neq9ydOal95CHfmTnM4I+ZI2rVQfjXQA1WSjjf4J2a7jLzWGNqNX+DF0SQzHU0pT
# i4dBwp9nEC8EAqoxW6q17r0z0noDjs6+BFo+z7bKSBwZXTRNivYuve3L2oiKNqet
# RHdqfMTCW/NmKLJ9M+MtucVGyOxiDf06VXxyKkOirv6o02OoXN4bFzK0vlNMsvhl
# qgF2puE6FndlENSmE+9JGYxOGLS/D284NHNboDGcmWXfwXRy4kbu4QFhOm0xJuF2
# EZAOk5eCkhSxZON3rGlHqhpB/8MluDezooIs8CVnrpHMiD2wL40mm53+/j7tFaxY
# KIqL0Q4ssd8xHZnIn/7GELH3IdvG2XlM9q7WP/UwgOkw/HQtyRN62JK4S1C8uw3P
# dBunvAZapsiI5YKdvlarEvf8EA+8hcpSM9LHJmyrxaFtoza2zNaQ9k+5t1wwggWN
# MIIEdaADAgECAhAOmxiO+dAt5+/bUOIIQBhaMA0GCSqGSIb3DQEBDAUAMGUxCzAJ
# BgNVBAYTAlVTMRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5k
# aWdpY2VydC5jb20xJDAiBgNVBAMTG0RpZ2lDZXJ0IEFzc3VyZWQgSUQgUm9vdCBD
# QTAeFw0yMjA4MDEwMDAwMDBaFw0zMTExMDkyMzU5NTlaMGIxCzAJBgNVBAYTAlVT
# MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
# b20xITAfBgNVBAMTGERpZ2lDZXJ0IFRydXN0ZWQgUm9vdCBHNDCCAiIwDQYJKoZI
# hvcNAQEBBQADggIPADCCAgoCggIBAL/mkHNo3rvkXUo8MCIwaTPswqclLskhPfKK
# 2FnC4SmnPVirdprNrnsbhA3EMB/zG6Q4FutWxpdtHauyefLKEdLkX9YFPFIPUh/G
# nhWlfr6fqVcWWVVyr2iTcMKyunWZanMylNEQRBAu34LzB4TmdDttceItDBvuINXJ
# IB1jKS3O7F5OyJP4IWGbNOsFxl7sWxq868nPzaw0QF+xembud8hIqGZXV59UWI4M
# K7dPpzDZVu7Ke13jrclPXuU15zHL2pNe3I6PgNq2kZhAkHnDeMe2scS1ahg4AxCN
# 2NQ3pC4FfYj1gj4QkXCrVYJBMtfbBHMqbpEBfCFM1LyuGwN1XXhm2ToxRJozQL8I
# 11pJpMLmqaBn3aQnvKFPObURWBf3JFxGj2T3wWmIdph2PVldQnaHiZdpekjw4KIS
# G2aadMreSx7nDmOu5tTvkpI6nj3cAORFJYm2mkQZK37AlLTSYW3rM9nF30sEAMx9
# HJXDj/chsrIRt7t/8tWMcCxBYKqxYxhElRp2Yn72gLD76GSmM9GJB+G9t+ZDpBi4
# pncB4Q+UDCEdslQpJYls5Q5SUUd0viastkF13nqsX40/ybzTQRESW+UQUOsxxcpy
# FiIJ33xMdT9j7CFfxCBRa2+xq4aLT8LWRV+dIPyhHsXAj6KxfgommfXkaS+YHS31
# 2amyHeUbAgMBAAGjggE6MIIBNjAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBTs
# 1+OC0nFdZEzfLmc/57qYrhwPTzAfBgNVHSMEGDAWgBRF66Kv9JLLgjEtUYunpyGd
# 823IDzAOBgNVHQ8BAf8EBAMCAYYweQYIKwYBBQUHAQEEbTBrMCQGCCsGAQUFBzAB
# hhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20wQwYIKwYBBQUHMAKGN2h0dHA6Ly9j
# YWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RDQS5jcnQw
# RQYDVR0fBD4wPDA6oDigNoY0aHR0cDovL2NybDMuZGlnaWNlcnQuY29tL0RpZ2lD
# ZXJ0QXNzdXJlZElEUm9vdENBLmNybDARBgNVHSAECjAIMAYGBFUdIAAwDQYJKoZI
# hvcNAQEMBQADggEBAHCgv0NcVec4X6CjdBs9thbX979XB72arKGHLOyFXqkauyL4
# hxppVCLtpIh3bb0aFPQTSnovLbc47/T/gLn4offyct4kvFIDyE7QKt76LVbP+fT3
# rDB6mouyXtTP0UNEm0Mh65ZyoUi0mcudT6cGAxN3J0TU53/oWajwvy8LpunyNDzs
# 9wPHh6jSTEAZNUZqaVSwuKFWjuyk1T3osdz9HNj0d1pcVIxv76FQPfx2CWiEn2/K
# 2yCNNWAcAgPLILCsWKAOQGPFmCLBsln1VWvPJ6tsds5vIy30fnFqI2si/xK4VC0n
# ftg62fC2h5b9W9FcrBjDTZ9ztwGpn1eqXijiuZQxggN8MIIDeAIBATB9MGkxCzAJ
# BgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwgSW5jLjFBMD8GA1UEAxM4RGln
# aUNlcnQgVHJ1c3RlZCBHNCBUaW1lU3RhbXBpbmcgUlNBNDA5NiBTSEEyNTYgMjAy
# NSBDQTECEAqA7xhLjfEFgtHEdqeVdGgwDQYJYIZIAWUDBAIBBQCggdEwGgYJKoZI
# hvcNAQkDMQ0GCyqGSIb3DQEJEAEEMBwGCSqGSIb3DQEJBTEPFw0yNTEyMjQwMzA5
# NTNaMCsGCyqGSIb3DQEJEAIMMRwwGjAYMBYEFN1iMKyGCi0wa9o4sWh5UjAH+0F+
# MC8GCSqGSIb3DQEJBDEiBCB9x9vMcHC82AFaVJsGxZNt7cs6p9GfJORlfq2/TA72
# pTA3BgsqhkiG9w0BCRACLzEoMCYwJDAiBCBKoD+iLNdchMVck4+CjmdrnK7Ksz/j
# bSaaozTxRhEKMzANBgkqhkiG9w0BAQEFAASCAgBy8ATZuKg9MBNCfsiaOvUbsi4Q
# ZvHkaa7JWhGG66th/2zTT7HFslqJK+Ubu+WFOkehz/X7tfoJhB8IfDQEw+ckXp2X
# PRMfqanWhqmaVqr82q8MX4NPH6LJ+5Y8TcgLjMbwhD3OH5mQe3C3MXYisSFn8n4N
# sKIwj5b5/PMXc7ux1rnre2u6klA3F8ZfErrYUfAXSvYPEAEoSw4elJ+egcb4VlXF
# EmZto/mGNYjIFIqb4r6RKTwaXBgW0WkK6AaCLJyIrfAPo7eB/Ol5V2yFCiR7dgdx
# jBIwHIzFwSo3CBE1Fp06SvsvPodqPx3L7L9+tZ8dEf3aCrXEiONxUKBtI358hldZ
# 8TdDoxdoc24LP3XSB/+tgd6L/xmlx09PRxAHQ58ZqczQh/j4/gbV9MgUgcktd81m
# 2Qfttn9bx9BBOFWK6TMbu5I3o/abppFMHqcmpLegAwxOxY4uqdjrijX8rI2j9yUV
# SrDiV2g4cThui515ofATQzd/HcwkxqxQjPLH7EPbKZPV63kqvKa4CJNPgbUynEk2
# wiDu0dgsXkH0FemQ9xALEQ21Q+eY7FRjEcnYludXG8x4jLi+Y3dIZinIy5pBSd94
# JusrOWyDF50q1WpTvBbHf89bVS3JLkkKBvUpFntvY8lalDhCMYUJbeOESGQcj9Jf
# nX0yEfiKrywnOxOP5w==
# SIG # End signature block
