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

    $sqlScript = $camstarPath + "Industry Solutions\CIO\Scripts\Mssql\CIOUpdateRoles.sql"
	
    $oracleScript = $camstarPath + "Industry Solutions\CIO\Scripts\Oracle\CIOUpdateRoles.or.sql"

    $RelativeDirectory = get-location;

    $dllsource = $camstarPath + "InSite Administration\Camstar.Util.dll"
    Add-Type -Path $dllsource
    $decryptedPassword = [Camstar.Util.CryptUtil]::Decrypt($dbPassword)  

    if ($dbtype -eq "SQLServer")
    {
        try{
            #This is where we are excuting the Script  
                  
            Invoke-SQLScript -HostName $serverName -ServiceName $databaseName -SQLPort $port -UserName $dbUsername -CurrentPassword $decryptedPassword -ScriptText "$sqlScript"
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
# MIIgPwYJKoZIhvcNAQcCoIIgMDCCICwCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCAnf+vm8Q8dlL1O
# Q9vtdrGfpq0W9Pz/VHwzFE8PEje9eaCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
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
# yK+p/pQd52MbOoZWeE4wggd3MIIFX6ADAgECAhAIvho4B7Q0chUBikxdv+V8MA0G
# CSqGSIb3DQEBCwUAMGkxCzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwg
# SW5jLjFBMD8GA1UEAxM4RGlnaUNlcnQgVHJ1c3RlZCBHNCBDb2RlIFNpZ25pbmcg
# UlNBNDA5NiBTSEEzODQgMjAyMSBDQTEwHhcNMjQwMTIyMDAwMDAwWhcNMjUwMTIx
# MjM1OTU5WjB/MQswCQYDVQQGEwJVUzEOMAwGA1UECBMFVGV4YXMxDjAMBgNVBAcT
# BVBsYW5vMScwJQYDVQQKEx5TaWVtZW5zIEluZHVzdHJ5IFNvZnR3YXJlIEluYy4x
# JzAlBgNVBAMTHlNpZW1lbnMgSW5kdXN0cnkgU29mdHdhcmUgSW5jLjCCAiIwDQYJ
# KoZIhvcNAQEBBQADggIPADCCAgoCggIBAMAk7AUSNBuguoy+KeddFMeIC57s95BH
# fgMfQjsBhDWLtNjFUqfdIridDHpsS0+fdGCGGqdn30q0st1NoDiRbOATsatJiLcf
# vQNAF/ET5toQvIvz9wfgHaz6E4A9wBfKsWK7vGHvf/L1OotDexw0E0IyYCeaU4V0
# JNXKiW4m8bvqInfNMkLessMrMyjDzXRolCZRI+38sUA0T4MZEUxdgk5tTLO4XLaX
# Yy5EtFmyG+ZZ8l6Nn86ce9yEIBijniCeHZR9qzfiEwbwWVgBsaA5XbndtX8t1idD
# rmFVHJJ7p8SVwRlzPTtgvKBDRKvJ6KahJts00eaWGlQP5dtdO7NriXqdcPymyM6v
# hXNIptBaJUgTj/msyk4xXY4DpX+Ue2ZYVIYHJPybzkax94X9tpEUWf6pgjO99jW/
# uEeqDnK3FOTYhnTCVpAYKLiItvSfkjAj6sAEcETL7a6egtXqSX+teRbUbB2vi/AM
# kfnkzVTG6+vU1STD875qYca8mT8gYdeYPgYMwXdDD8Z8ZktFSsWV2WmoH+3z7oLG
# 4bHjvbO4o90cfiUl9et35809j4+ZZ64QfOxurdnPM+ahdwvOVrAixgZQiCEU5aym
# 3sOoxyabSIjfCXr8vWXHfoz+VH7aW4cR3o4dQsigBuIt4IZR/ooZjmfJaqaLAZBe
# HS99GH3B/QbtAgMBAAGjggIDMIIB/zAfBgNVHSMEGDAWgBRoN+Drtjv4XxGG+/5h
# ewiIZfROQjAdBgNVHQ4EFgQUjPruSm8P/Qko4b2muVTy8mF0bsgwPgYDVR0gBDcw
# NTAzBgZngQwBBAEwKTAnBggrBgEFBQcCARYbaHR0cDovL3d3dy5kaWdpY2VydC5j
# b20vQ1BTMA4GA1UdDwEB/wQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAzCBtQYD
# VR0fBIGtMIGqMFOgUaBPhk1odHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNl
# cnRUcnVzdGVkRzRDb2RlU2lnbmluZ1JTQTQwOTZTSEEzODQyMDIxQ0ExLmNybDBT
# oFGgT4ZNaHR0cDovL2NybDQuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZEc0
# Q29kZVNpZ25pbmdSU0E0MDk2U0hBMzg0MjAyMUNBMS5jcmwwgZQGCCsGAQUFBwEB
# BIGHMIGEMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20wXAYI
# KwYBBQUHMAKGUGh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRy
# dXN0ZWRHNENvZGVTaWduaW5nUlNBNDA5NlNIQTM4NDIwMjFDQTEuY3J0MAkGA1Ud
# EwQCMAAwDQYJKoZIhvcNAQELBQADggIBAMuPpBxJa5HRUGrlKljtz6HuSCjm+tkL
# CCREM8cYiYg6VReF2S8Kw7Zc2Kas1f7gb4obQ1F1FFAEI1CgWGNtgYrvYDTVifsc
# 02kKoFa4Nvs3NoKnAAD8WnvP89tzxqtL3EQmracdaO4aYg/gRpO7q2RfrM6X8Nd0
# GVDnscupPyQTrg4ACAm+xSeoUpRMxGDZRGord3YEQZH0bQmuW9dwazAOFJyI9k/B
# P4RN0XDFldbCFqA+d9y3VMxMd50rgVuIi4eAtaxhIXrwo/0dfk2ykxdR0qrKv5ta
# gRiGbeNCnUVamWuIQ+us4aFQsbzpOLMporEb//torRko+R4V59I1OvN0qeGRyT2h
# WJ1hh59HSNuj6KGETYkSzJWbxczZ/+jicRLJ7dUfSeFC6/aK2vvhn0oS+f6U7JJN
# crmwIcEh+h0jfjVLZVrsxfI0yG8InK3Z1of+lJrrHg/XFPAt+AqAUrAKMgOcOg6g
# YAx6vG1K98LdOCypuSzqzhrRQBDHAqUXpAU23wdVyxBlseh9Vwrm3o5Js8YrrBvd
# zga/6Po4+ikVljm7zMDvag/S7VmYid1rIe2vn1Xo26ic/+x3amuKt9Sttx9C0+IA
# r+MhW+6afnRBWN3INXm4gDdmu82LcvOmik3tNvEVdWx/MEsfBPDbRd512eLyMPOC
# vrrL7YT6AKrnMYIRZjCCEWICAQEwfTBpMQswCQYDVQQGEwJVUzEXMBUGA1UEChMO
# RGlnaUNlcnQsIEluYy4xQTA/BgNVBAMTOERpZ2lDZXJ0IFRydXN0ZWQgRzQgQ29k
# ZSBTaWduaW5nIFJTQTQwOTYgU0hBMzg0IDIwMjEgQ0ExAhAIvho4B7Q0chUBikxd
# v+V8MA0GCWCGSAFlAwQCAQUAoHwwEAYKKwYBBAGCNwIBDDECMAAwGQYJKoZIhvcN
# AQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUw
# LwYJKoZIhvcNAQkEMSIEIMj9bf06vScfnk7sg62GuL5YOqpTrSs7d6gicaYB8kc1
# MA0GCSqGSIb3DQEBAQUABIICAC65RpVSKIzYuzjLVFt7dhXgSvxlmx8xydMg91rs
# /oLr0w5ja65AskrOmhQ0QivTVYwvkm4jXyxuw6wkEja19oY4Y1PnjqmjGKkt3MRr
# tvG9b4hHEE6m/VriGSeK54CpursGx79LoPc8I9C4r2mYu07diqmSKkgIECC4HTDF
# 00Vt12TaKba9yoQLxByFaEJR7HYgRqQ9QU2SLl58/2dAI1Xe8OZwZMVumOx22wGP
# A+893HsQvAo4n+0P8BrLMwjqr4kBgDS8ym3CJQuM3KPRPm4PxglBDlqc3eflO0gW
# 93HHUtcsurvWTedTRi/u68TP1EYFWhzFHt5oJo2GlAJ1JuAK/WrlTgLPc/pGs5mY
# bk/1/yHe158DZwGXFQ7vvqHPSj6wR6IlheHhwkkA6iL1IPEpG4BjVaDW26fHyPJT
# XTpTLIj1VWcU2octBuJ1fyE4ZCslYrUx79ojU28C/e91/KeT4vlhEU4ojhhqad4I
# t0d27gJXu9+WOHLQVgtrfff5QiHui6IjTbn0LAgFtxO33eWG5+vVu4Wol+mJ50GK
# TBsWMR80uMmumHLlUM7Ld+YDX4cZuunBl2l+QGgJ1Z4DMq71oSuetePmYyZPj1DP
# 9q1inCgyWN6SU21BqHfy6J9JeNYWbN081SQauKm1grElMQridG4Ixdd+ejGhxO2o
# l3fOoYIOPDCCDjgGCisGAQQBgjcDAwExgg4oMIIOJAYJKoZIhvcNAQcCoIIOFTCC
# DhECAQMxDTALBglghkgBZQMEAgEwggEOBgsqhkiG9w0BCRABBKCB/gSB+zCB+AIB
# AQYLYIZIAYb4RQEHFwMwMTANBglghkgBZQMEAgEFAAQg/QRW61b0UYaE8Rhpcv5j
# 7TIS/WBaD6HCQXu3oLN3zikCFGKae7oWHb0Yzc0hcZCPfFfuzVBkGA8yMDI0MDUy
# MTE0MjE1MVowAwIBHqCBhqSBgzCBgDELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5
# bWFudGVjIENvcnBvcmF0aW9uMR8wHQYDVQQLExZTeW1hbnRlYyBUcnVzdCBOZXR3
# b3JrMTEwLwYDVQQDEyhTeW1hbnRlYyBTSEEyNTYgVGltZVN0YW1waW5nIFNpZ25l
# ciAtIEczoIIKizCCBTgwggQgoAMCAQICEHsFsdRJaFFE98mJ0pwZnRIwDQYJKoZI
# hvcNAQELBQAwgb0xCzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5WZXJpU2lnbiwgSW5j
# LjEfMB0GA1UECxMWVmVyaVNpZ24gVHJ1c3QgTmV0d29yazE6MDgGA1UECxMxKGMp
# IDIwMDggVmVyaVNpZ24sIEluYy4gLSBGb3IgYXV0aG9yaXplZCB1c2Ugb25seTE4
# MDYGA1UEAxMvVmVyaVNpZ24gVW5pdmVyc2FsIFJvb3QgQ2VydGlmaWNhdGlvbiBB
# dXRob3JpdHkwHhcNMTYwMTEyMDAwMDAwWhcNMzEwMTExMjM1OTU5WjB3MQswCQYD
# VQQGEwJVUzEdMBsGA1UEChMUU3ltYW50ZWMgQ29ycG9yYXRpb24xHzAdBgNVBAsT
# FlN5bWFudGVjIFRydXN0IE5ldHdvcmsxKDAmBgNVBAMTH1N5bWFudGVjIFNIQTI1
# NiBUaW1lU3RhbXBpbmcgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQC7WZ1ZVU+djHJdGoGi61XzsAGtPHGsMo8Fa4aaJwAyl2pNyWQUSym7wtkpuS7s
# Y7Phzz8LVpD4Yht+66YH4t5/Xm1AONSRBudBfHkcy8utG7/YlZHz8O5s+K2WOS5/
# wSe4eDnFhKXt7a+Hjs6Nx23q0pi1Oh8eOZ3D9Jqo9IThxNF8ccYGKbQ/5IMNJsN7
# CD5N+Qq3M0n/yjvU9bKbS+GImRr1wOkzFNbfx4Dbke7+vJJXcnf0zajM/gn1kze+
# lYhqxdz0sUvUzugJkV+1hHk1inisGTKPI8EyQRtZDqk+scz51ivvt9jk1R1tETqS
# 9pPJnONI7rtTDtQ2l4Z4xaE3AgMBAAGjggF3MIIBczAOBgNVHQ8BAf8EBAMCAQYw
# EgYDVR0TAQH/BAgwBgEB/wIBADBmBgNVHSAEXzBdMFsGC2CGSAGG+EUBBxcDMEww
# IwYIKwYBBQUHAgEWF2h0dHBzOi8vZC5zeW1jYi5jb20vY3BzMCUGCCsGAQUFBwIC
# MBkaF2h0dHBzOi8vZC5zeW1jYi5jb20vcnBhMC4GCCsGAQUFBwEBBCIwIDAeBggr
# BgEFBQcwAYYSaHR0cDovL3Muc3ltY2QuY29tMDYGA1UdHwQvMC0wK6ApoCeGJWh0
# dHA6Ly9zLnN5bWNiLmNvbS91bml2ZXJzYWwtcm9vdC5jcmwwEwYDVR0lBAwwCgYI
# KwYBBQUHAwgwKAYDVR0RBCEwH6QdMBsxGTAXBgNVBAMTEFRpbWVTdGFtcC0yMDQ4
# LTMwHQYDVR0OBBYEFK9j1sqjToVy4Ke8QfMpojh/gHViMB8GA1UdIwQYMBaAFLZ3
# +mlIR59TEtXC6gcydgfRlwcZMA0GCSqGSIb3DQEBCwUAA4IBAQB16rAt1TQZXDJF
# /g7h1E+meMFv1+rd3E/zociBiPenjxXmQCmt5l30otlWZIRxMCrdHmEXZiBWBpgZ
# jV1x8viXvAn9HJFHyeLojQP7zJAv1gpsTjPs1rSTyEyQY0g5QCHE3dZuiZg8tZiX
# 6KkGtwnJj1NXQZAv4R5NTtzKEHhsQm7wtsX4YVxS9U72a433Snq+8839A9fZ9gOo
# D+NT9wp17MZ1LqpmhQSZt/gGV+HGDvbor9rsmxgfqrnjOgC/zoqUywHbnsc4uw9S
# q9HjlANgCk2g/idtFDL8P5dA4b+ZidvkORS92uTTw+orWrOVWFUEfcea7CMDjYUq
# 0v+uqWGBMIIFSzCCBDOgAwIBAgIQe9Tlr7rMBz+hASMEIkFNEjANBgkqhkiG9w0B
# AQsFADB3MQswCQYDVQQGEwJVUzEdMBsGA1UEChMUU3ltYW50ZWMgQ29ycG9yYXRp
# b24xHzAdBgNVBAsTFlN5bWFudGVjIFRydXN0IE5ldHdvcmsxKDAmBgNVBAMTH1N5
# bWFudGVjIFNIQTI1NiBUaW1lU3RhbXBpbmcgQ0EwHhcNMTcxMjIzMDAwMDAwWhcN
# MjkwMzIyMjM1OTU5WjCBgDELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5bWFudGVj
# IENvcnBvcmF0aW9uMR8wHQYDVQQLExZTeW1hbnRlYyBUcnVzdCBOZXR3b3JrMTEw
# LwYDVQQDEyhTeW1hbnRlYyBTSEEyNTYgVGltZVN0YW1waW5nIFNpZ25lciAtIEcz
# MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArw6Kqvjcv2l7VBdxRwm9
# jTyB+HQVd2eQnP3eTgKeS3b25TY+ZdUkIG0w+d0dg+k/J0ozTm0WiuSNQI0iqr6n
# CxvSB7Y8tRokKPgbclE9yAmIJgg6+fpDI3VHcAyzX1uPCB1ySFdlTa8CPED39N0y
# OJM/5Sym81kjy4DeE035EMmqChhsVWFX0fECLMS1q/JsI9KfDQ8ZbK2FYmn9ToXB
# ilIxq1vYyXRS41dsIr9Vf2/KBqs/SrcidmXs7DbylpWBJiz9u5iqATjTryVAmwlT
# 8ClXhVhe6oVIQSGH5d600yaye0BTWHmOUjEGTZQDRcTOPAPstwDyOiLFtG/l77CK
# mwIDAQABo4IBxzCCAcMwDAYDVR0TAQH/BAIwADBmBgNVHSAEXzBdMFsGC2CGSAGG
# +EUBBxcDMEwwIwYIKwYBBQUHAgEWF2h0dHBzOi8vZC5zeW1jYi5jb20vY3BzMCUG
# CCsGAQUFBwICMBkaF2h0dHBzOi8vZC5zeW1jYi5jb20vcnBhMEAGA1UdHwQ5MDcw
# NaAzoDGGL2h0dHA6Ly90cy1jcmwud3Muc3ltYW50ZWMuY29tL3NoYTI1Ni10c3Mt
# Y2EuY3JsMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMIMA4GA1UdDwEB/wQEAwIHgDB3
# BggrBgEFBQcBAQRrMGkwKgYIKwYBBQUHMAGGHmh0dHA6Ly90cy1vY3NwLndzLnN5
# bWFudGVjLmNvbTA7BggrBgEFBQcwAoYvaHR0cDovL3RzLWFpYS53cy5zeW1hbnRl
# Yy5jb20vc2hhMjU2LXRzcy1jYS5jZXIwKAYDVR0RBCEwH6QdMBsxGTAXBgNVBAMT
# EFRpbWVTdGFtcC0yMDQ4LTYwHQYDVR0OBBYEFKUTAamfhcwbbhYeXzsxqnk2AHsd
# MB8GA1UdIwQYMBaAFK9j1sqjToVy4Ke8QfMpojh/gHViMA0GCSqGSIb3DQEBCwUA
# A4IBAQBGnq/wuKJfoplIz6gnSyHNsrmmcnBjL+NVKXs5Rk7nfmUGWIu8V4qSDQjY
# ELo2JPoKe/s702K/SpQV5oLbilRt/yj+Z89xP+YzCdmiWRD0Hkr+Zcze1GvjUil1
# AEorpczLm+ipTfe0F1mSQcO3P4bm9sB/RDxGXBda46Q71Wkm1SF94YBnfmKst04u
# FZrlnCOvWxHqcalB+Q15OKmhDc+0sdo+mnrHIsV0zd9HCYbE/JElshuW6YUI6N3q
# dGBuYKVWeg3IRFjc5vlIFJ7lv94AvXexmBRyFCTfxxEsHwA/w0sUxmcczB4Go5Bf
# XFSLPuMzW4IPxbeGAk5xn+lmRT92MYICWjCCAlYCAQEwgYswdzELMAkGA1UEBhMC
# VVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMR8wHQYDVQQLExZTeW1h
# bnRlYyBUcnVzdCBOZXR3b3JrMSgwJgYDVQQDEx9TeW1hbnRlYyBTSEEyNTYgVGlt
# ZVN0YW1waW5nIENBAhB71OWvuswHP6EBIwQiQU0SMAsGCWCGSAFlAwQCAaCBpDAa
# BgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwHAYJKoZIhvcNAQkFMQ8XDTI0MDUy
# MTE0MjE1MVowLwYJKoZIhvcNAQkEMSIEIGuY8uqn1wLrk5qVbtWzT1HEYIAZXo8k
# 72pxY6Am17fAMDcGCyqGSIb3DQEJEAIvMSgwJjAkMCIEIMR0znYAfQI5Tg2l5N58
# FMaA+eKCATz+9lPvXbcf32H4MAsGCSqGSIb3DQEBAQSCAQBg5FpJ1ahJ9SphwyCo
# VVIbhA2sDz77VgTqoNkLBXLQhEdhuRIpDR2DfRPImiVXjSifC9KvQUgLTB5RHQUw
# T2LfxwFDnQZ4KTU5Ew13+AmtBiAx3U/2gc0nd5JrlVdzMhMQ1pl+4XLEurqKCQyU
# M1AlzJUpo4RkSbS3TV4yVQfW+H5MGUP1PdZuk6EUnpTCGIB4xdw6+XoAeJeSbMEe
# Altu58xG6sUmd/81RJcL3P4amJWTanl2NOEXQc4/g25idXR1CdyR0Q4+tyw8bbtQ
# 3KGjeabtBTpyHJZWRUktfhpjXmAP4Uhuu9uR82QYm0gRkjL+yqBaEhV8uVdyblr5
# Ld85
# SIG # End signature block
