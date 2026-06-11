# Copyright Siemens 2019

#region Params

param(
		
	[Parameter(Mandatory=$False,Position = 0)]
	[string]$databaseName = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").TxnDBName, 
	
	[Parameter(Mandatory=$False,Position = 1)]
	[string]$serverName = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").TxnDBHostName, 
	
	[Parameter(Mandatory=$False,Position = 2)]
	[string]$dbUsername = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").LoginName, 
	
	[Parameter(Mandatory=$False,Position = 3)]
	[string]$dbPassword = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").LoginPassword,        

	[Parameter(Mandatory=$False,Position = 4)]
	[string]$schema,

    [Parameter(Mandatory=$False,Position = 5)]
	[string]$port,

    [Parameter(Mandatory=$False,Position = 6)]
	[string]$dbtype = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").TxnDBType,
	
	[Parameter(Position = 7)]
	[bool]$isHosted,

    [Parameter(Mandatory=$False,Position = 8)]
	[string]$camstarPath = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Path")."(Default)",
    
    [Parameter(Mandatory=$False,Position = 9)]
    [string]$portSource = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").TxnDBConnectionParameters,
    
    [Parameter(Mandatory=$False,Position = 10)]
    [string]$ODACHintPath = "Oracle.ManagedDataAccess.dll"
) 

#endregion


function Select-FolderDialog {
    param(
        [string]$Description = "Select Folder",
        [string]$SelectedPath = "Desktop"
    )
       
    [System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null     

    $objForm = New-Object System.Windows.Forms.FolderBrowserDialog
    $objForm.SelectedPath = $SelectedPath
    $objForm.Description = $Description
    $Show = $objForm.ShowDialog()
    if ($Show -ne "OK") {
        return $null
    }
    $objForm.SelectedPath
}


function Get-OraclePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OraclePath,
        [Parameter(Mandatory = $false)]
        [bool]$NoPromptForPath = $false
    )       

    # Get Oracle path 
    if ($OraclePath -eq '' -or ($OraclePath -notmatch "Oracle.ManagedDataAccess.dll`$") -or !(Test-Path -Path $OraclePath) ) 
    {
        [bool] $finished = $false
        [string]$basePath = $null;

        if ( (![string]::IsNullOrWhiteSpace($OraclePath)) -and ((Test-Path -Path $OraclePath))) {

            $searchResults = get-childitem -path $OraclePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
            where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
            sort-object FullName -Descending | Select-Object FullName

            # if found
            if ($null -ne $searchResults) { 
                $OraclePath = $searchResults[0].FullName
                $finished = $true;
            }
            else {
                Write-Host -ForegroundColor Red "`nError: Oracle.ManagedDataAccess.dll not found in folder tree."
                $finished = $false;
            }
        }     

        if (!$finished)
        {
            # First try a reg search.
            try{
                $odac_home = Get-ItemProperty -Path "HKLM:\SOFTWARE\Oracle\*" -Name "ORACLE_HOME";
                $basePath = $odac_home.ORACLE_HOME;
                Write-Host "ORACLE_HOME basePath from reg search 1: " $basePath;
            }
            catch{
                $basePath = $null;
            }
            
            if ([string]::IsNullOrWhiteSpace($basePath)) {
                # Try known key/value.
                try{
                    $basePath = (Get-ItemProperty "HKLM:\SOFTWARE\Oracle\KEY_odac")."ORACLE_HOME";
                    Write-Host "ORACLE_HOME basePath from reg search 2: " $basePath;
                }
                catch{
                    $basePath = $null;
                }
            }
            
            if ([string]::IsNullOrWhiteSpace($basePath)) {
                # Try another known key/value.
                try{
                    $basePath = (Get-ItemProperty "HKLM:\SOFTWARE\Oracle\KEY_both")."ORACLE_HOME";
                    Write-Host "ORACLE_HOME basePath from reg search 3: " $basePath;
                }
                catch{
                    $basePath = $null;
                }
            }

            if (![string]::IsNullOrWhiteSpace($basePath)) {

                $searchResults = get-childitem -path $basePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
                where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
                sort-object FullName -Descending | Select-Object FullName

                # if found
                if ($null -ne $searchResults) { 
                    $OraclePath = $searchResults[0].FullName
                    $finished = $true;
                }
                else {
                    Write-Host -ForegroundColor Red "`nError: Oracle.ManagedDataAccess.dll not found in folder tree."
                    $finished = $false;
                    $basePath = $null;
                }
            }
        }

        #while (!$finished)
        #{
        #    $basePath = $null;
        #    if (!$NoPromptForPath) {
        #        # Prompt the user for a location to search.
        #        $basePath = Select-FolderDialog -Description "** Choose Oracle Managed Data Access folder **"  
        #        if (![string]::IsNullOrWhiteSpace($basePath)) {

        #            $searchResults = get-childitem -path $basePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
        #            where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
        #            sort-object FullName -Descending | Select-Object FullName

        #            # if found
        #            if ($null -ne $searchResults) { 
        #                $OraclePath = $searchResults[0].FullName
        #                $finished = $true;
        #            }
        #            else {
        #                Write-Host -ForegroundColor Red "`nError: Oracle.ManagedDataAccess.dll not found in folder tree."
        #                $finished = $false;
        #            }
        #        }
        #        else {
        #            $finished = $true;
        #        } 
        #    }
        #    else {
        #        Write-Host -ForegroundColor Red "`nError: Oracle.ManagedDataAccess.dll not found in folder tree."
        #        $finished = $true;
        #    }            
        #}
    }

    return $OraclePath
}

function Get-ODAC {
    param(
        [Parameter(Mandatory = $false)]
        [string]$OracleHintPath = "Oracle.ManagedDataAccess.dll",
        [Parameter(Mandatory = $false)]
        [bool]$NoPromptForPath = $false
    ) 

    [bool]$fLoaded = $false;

    # if it is in the path or GAC then it should load. try that first.
    try {
        [System.Reflection.Assembly]$asm = $null;
        try {
            #GAC first
            #$asm = [System.Reflection.Assembly]::Load("Oracle.ManagedDataAccess, Version=4.122.18.3, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=MSIL");
            $asm = [System.Reflection.Assembly]::LoadWithPartialName("Oracle.ManagedDataAccess")
            if (![string]::IsNullOrEmpty($asm.FullName)) {
                # make sure it actually worked by attempting to use it.
                $oracleConnection = [Oracle.ManagedDataAccess.Client.OracleConnection]::new();
                Write-Host "Loaded ODAC from: " $asm.CodeBase;
                $fLoaded = $true;
            }
        }
        catch {
            $fLoaded = $false;
        }

        if (!$fLoaded) {
            #Path second
            $asm = [System.Reflection.Assembly]::LoadFrom($OracleHintPath);
            if (![string]::IsNullOrEmpty($asm.FullName)) {
                # make sure it actually worked by attempting to use it.
                $oracleConnection = [Oracle.ManagedDataAccess.Client.OracleConnection]::new();
                Write-Host "Loaded ODAC from: " $asm.CodeBase;
                $fLoaded = $true;
            }
        }
    }
    catch {
        $fLoaded = $false;
    }

    if (!$fLoaded) {
        # Go looking for it.
        try {
            [string]$oraclePath = Get-OraclePath -OraclePath $OracleHintPath;
            Write-Host "Get-OraclePath returned: " $oraclePath
            Add-Type -Path $oraclePath
            # make sure it actually worked by attempting to use it.
            $oracleConnection = [Oracle.ManagedDataAccess.Client.OracleConnection]::new();
            $fLoaded = $true;
        }
        catch {
            $fLoaded = $false;
        }
    }

    if (!$fLoaded) {
        $message = "ERROR: Could not load Oracle.ManagedDataAccess assembly. Oracle ODAC must be installed."
        write-output $message
	if($isHosted) {
            Read-Host "$message"
         }
    }

    return $fLoaded;
}


<#
.SYNOPSIS
Execute of SQL scripts for MSS
#>
function Invoke-SQLScript([String]$HostName,[String]$ServiceName,[String]$SQLPort,[String]$UserName,[String]$CurrentPassword, [String] $ScriptText)
{  
       if($SQLPort -ne "")
       {
         $HostName = $HostName + "," + $SQLPort
       }

       $SQLText = Get-Content $ScriptText
              
       # built the connection string
        $connectionString = "Data Source=$HostName;Initial Catalog=$ServiceName;User Id=$UserName;Password=$CurrentPassword;"
            try {
                    # Try and connect to server
                    $sqlConn = new-object Data.SqlClient.SqlConnection($connectionString)
                    $delimiter = "GO"
                   
                   # Break up statements by delimiter
                      $SQLScript = Get-Content $ScriptText -Delimiter "GO"
                
                 foreach($text in $SQLScript)
                   {
                  
                        if($text -ne "")
                        {
                            if($sqlConn.State -ne 'Open')
                            {
                            $sqlConn.Open()
                            }
                                # If connection was made to the server
                            if ($sqlConn.State -eq 'Open')
                            {   

                                #Replace Delimiter with white space for execution of SQL statement
                                $_ = $text.Replace("GO","")

                               #Create the SQL Command object                  
                               $sqlcommand = new-object system.data.sqlclient.sqlcommand($_, $sqlConn) 

                               #Execute script
                               $test =  $sqlcommand.ExecuteNonQuery()
                            } 
                        }
                   }
                }

            catch  {     
                      # If connection failed write to verbose                                  
                       $excetion = $_.Exception.Message
                       WriteLog "Exception: On server - $HostName for script - $ScriptText error - $excetion"
                        if ($sqlConn.State -ne 'Open')
                        {
                         WriteLog "Exception: Not available Server: $HostName";
                        }
                       Exit    
                    } 
            finally 
                    {
                      $sqlConn.Close();
                      $sqlConn.Dispose()
                    } 
}

<#
.SYNOPSIS
Parses out the full script in to commands and reads the header text
#>
function OracleScriptParser([string] $file)
{
    $port = ($portSource -split "\=",2)[1]

    $dllsource = $camstarPath + "InSite Administration\Camstar.Util.dll"
    Add-Type -Path $dllsource
    $decryptedPassword = [Camstar.Util.CryptUtil]::Decrypt($dbPassword)

    $fullText = Get-Content $file -Delimiter "--#delimiter"

     foreach($text in $fullText)
    {

		#WriteLog -message $file
        $sqlStatements = $text.Replace("--#delimiter","")
        
        #looks for header text and if not runs script command. Looks also the end of the script
        if($sqlStatements.Contains("-------") -OR !$sqlStatements -OR $sqlStatements.trim().Length -eq 0)
            {
                #WriteLog -message $sqlStatements
                continue
            }
        else
            {    
                Invoke-OracleScript  -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText $sqlStatements
            }
    }

}

<#
.SYNOPSIS
Executes Qracle scripts using the Oracle Data Access Components.
#>
function Invoke-OracleScript([String]$HostName,[String]$ServiceName,[String]$SQLPort,[String]$UserName,[String]$CurrentPassword, [String] $ScriptText)
{
	try
	{

		$SQLConnectionString = "Data Source= (DESCRIPTION =(ADDRESS =(PROTOCOL = TCP)(HOST = " + $HostName + ")(PORT = " + $SQLPORT + "))(CONNECT_DATA =(SERVICE_NAME = " + $ServiceName + ")));User Id=" + $UserName + ";Password=" + $CurrentPassword + ";"
		$SQLConnection = New-Object Oracle.ManagedDataAccess.Client.OracleConnection($SQLConnectionString)
		$SQLConnection.Open()

        $SQLcommand = $SQLConnection.CreateCommand() #SET DEFINE OFF

        $SQLcommand.CommandText = $ScriptText
                                                                                                                                                                                                                                      
        $test = $SQLcommand.ExecuteNonQuery()

	}
	catch
	    {
          $ErrorMessage = $_.Exception.Message
          $message = "Exception: Failed for the Oracle account '$UserName' on Host '$HostName' and this script.`n*************** $ScriptText***************`nError = '$ErrorMessage'"
          WriteLog $message
          Exit
	    }
    finally
            {
                $SQLConnection.Close()
           
                $SQLConnection.Dispose()
            }
}

<#This is the main function for processing.#>
function Main
{
    $port = ($portSource -split "\=",2)[1]

    $sqlScript = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql\isPopulatePortalMenuUpdateData.sql"
    $sqlScript2 = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql\isResourcesResolvedCalendar.sql"
    $sqlScript3 = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql\is_OEECalculation.sql"
    $sqlScript4 = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql\isPopulateDefaultUserQuery.sql"
    $sqlScript5 = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql\isFunctionsAndProcedures.sql"
	
    $oracleScript = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle\isPopulatePortalMenuUpdateData.or.sql"
    $oracleScript2 = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle\isResourcesResolvedCalendar.or.sql"
    $oracleScript3 = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle\is_OEECalculation.or.sql"
    $oracleScript4 = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle\isPopulateDefaultUserQuery.or.sql"
    $oracleScript5 = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle\isFunctionsAndProcedures.or.sql"

    $RelativeDirectory = get-location;

    $dllsource = $camstarPath + "InSite Administration\Camstar.Util.dll"
    Add-Type -Path $dllsource
    $decryptedPassword = [Camstar.Util.CryptUtil]::Decrypt($dbPassword)  

    if ($dbtype -eq "SQLServer")
    {
        try{
            #This is where we are excuting the Script  
                  
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript"
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript2"
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript3"
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript4"
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript5"
           }
        Catch{   
                #WriteLog( $ErrorMessage = $_.Exception.Message)

	            if($isHosted)
                {
                    Read-Host "There was an error  : $ErrorMessage . Please rerun the script!"
                }

             }
    }
    else
    {
        try{ 
            #This is where we are excuting the Script  

            try
            { 
                #[System.Reflection.Assembly]::LoadWithPartialName("Oracle.ManagedDataAccess")
                $fLoadedODAC = Get-ODAC -OracleHintPath:$ODACHintPath
                if ($fLoadedODAC -ne $true){
                    $message = "ERROR: Could not load Oracle assembly."
                    write-output $message
                    if($isHosted)
                    {
                        Read-Host "There was an error  : $message . Please rerun the script!"
                    }
                    Exit
                }
            }
            Catch
            {   
                $message = "ERROR: Could not load Oracle assembly."
                write-output $message
                if($isHosted)
                {
                    Read-Host "There was an error  : $message . Please rerun the script!"
                }
                Exit
            }  
                
            OracleScriptParser -file $oracleScript
            OracleScriptParser -file $oracleScript2
            OracleScriptParser -file $oracleScript3
            OracleScriptParser -file $oracleScript4
            OracleScriptParser -file $oracleScript5
           }
        Catch{   
                #WriteLog( $ErrorMessage = $_.Exception.Message)

	            if($isHosted)
                {
                    Read-Host "There was an error  : $ErrorMessage . Please rerun the script!"
                }

             }
    }
}

######################################
#####        Execution           #####
#####          Order             #####
######################################

$isHosted = $false

Main
# SIG # Begin signature block
# MIIpeQYJKoZIhvcNAQcCoIIpajCCKWYCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDdD5BKN/nWOikk
# pl50xt0SGGjkWxEfgiz5bOtqmxVFYKCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
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
# LwYJKoZIhvcNAQkEMSIEIFbQ0yqihmdomg7yDAZ2dpamoMAfQFCGxFrC0EfsNDEW
# MA0GCSqGSIb3DQEBAQUABIICAF34bGB9XfF+XIKq9TOWeg4w+JuAHhOeWTNUDpEN
# zHcoZlrmSBa27mYG9r0hFW/GzXmt+c0luuSzr+34WgH/DJjYbprw7MscThyDfwQc
# KlykWtsinJcP6HDi45B21ULREP8F9jXOwvtW79lwFqvyJwNullRebtfBdZipZhfn
# SyD8lVJDaXM1XDQaTfSL4esLfeOkoagtIT6dLqI4ywLRTkNGOUN5x541FhG0cMG3
# +U5CdMOGImzaP16EDRMSTu7rLMNhsUNJtz2M2gNTOfyaKO8UsT9obaxNk5Gb1Fb3
# Gbe+hogMbju/1iWOr43l4nALRUB8Eev5A4FDBZch0iD3dPzk4J19u0ETXRlpmbrV
# tcC6QHKc+vCo6bSosInpTrKDDe73yJdLcQl1CekEWpHGEJ+MySiQCr7TSTwmF3d7
# k06jsls8B47D46Zt2pjtt4L4N1PXxynPUFUpxtcxXZkWJqC5Q9IphCymkpPPWe9F
# CoQfmIkPDxsdRH7Vg9Uh1dcirQ6TM2iw+Fd2c7UrnlbHTgyHqRkrdExNXMObzqzI
# ZroSDIUYL6mTyg/zO6n5IU7KpYlelao4p9IyE5FgqM+gVc5z1NJ9NIET0a+a9TyU
# cf2dX3xMLFZsxQrj+nKB+REfU+cYDppY3jJBHQ+JZS7xfk+XufazmvcBfSUmA90H
# HyNSoYIXdjCCF3IGCisGAQQBgjcDAwExghdiMIIXXgYJKoZIhvcNAQcCoIIXTzCC
# F0sCAQMxDzANBglghkgBZQMEAgEFADB3BgsqhkiG9w0BCRABBKBoBGYwZAIBAQYJ
# YIZIAYb9bAcBMDEwDQYJYIZIAWUDBAIBBQAEIKITR8+u0fEYg7QD3WRD9DoK5tL/
# jDLN/QGv+VNmpy62AhBBT/0ywEsfrW4mcU4ZLAfIGA8yMDI1MTIyOTE4MTcyNVqg
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
# hvcNAQkDMQ0GCyqGSIb3DQEJEAEEMBwGCSqGSIb3DQEJBTEPFw0yNTEyMjkxODE3
# MjVaMCsGCyqGSIb3DQEJEAIMMRwwGjAYMBYEFN1iMKyGCi0wa9o4sWh5UjAH+0F+
# MC8GCSqGSIb3DQEJBDEiBCCUSB0zPLxP4bOkkSRPtBvmYSd07yXiMZnYP8G/bp9P
# HDA3BgsqhkiG9w0BCRACLzEoMCYwJDAiBCBKoD+iLNdchMVck4+CjmdrnK7Ksz/j
# bSaaozTxRhEKMzANBgkqhkiG9w0BAQEFAASCAgCAPaPp7TEh90Vc1adLPBkoPnSR
# 8lKEvqrqMShqBffd4kdNeJ2jdtkgiw2FVWeDBtwjjuXlKxap2sw79NNk6F5vjNtE
# 6xTyJ+ZShwNECNkCu3CSKTQb2dJJtH1XJ8qRfRnRTafHEQxG/PDh3P6jbae7hV/Q
# 9onSKa5ihNZ4Q0jXzrRfcBquGd9HuT+vZPzCfZ22xi9ZlX96SAI7Cu4IByTn6CFZ
# S78j8WffrTzRQGXpi+OTt9z1O+zyXyQSV9pHy9OFEclDKCUb3+D7rDTh53br5Sd3
# 83O5jylwibKZZSM6Ccxgs+0yhTfx2SKgYJu5vHM9wJO8bibFR0eSaKyfZFgggW27
# wJtwu8DtmuwKFlDRA6HJgdWuZ/c6/LUtcqnouqwpMmmYjjkpsUjO9w4c1CjkrhJ+
# vcB1TmEzqLy4h36/4vXJVjxHXekXzK63F6uWRpPKgHOHI5qgOePoJrWEztaHHHaf
# 2dj0YXOSKtUnx3ucTLOdqESRJrKNz1xajlccdZmTll3idyPDmoG5Up1e9NPizbOD
# iPi5atVaFUi3UOFtsi1tE61ngrEBVk5yHUcLX36pA/Rk7ds8rNqqnLGgUaCWR53C
# CvFyhf3uOWxjFdtWqJ75Dtfh/yu/LhxqUKSKcoqq8yy2jnPgwKIgzCGdUMcnylPF
# p6BSWden63n3Zzaugw==
# SIG # End signature block
