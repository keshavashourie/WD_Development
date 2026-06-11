
# © 2018 Siemens Product Lifecycle Management Software Inc.

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
        
        #looks for header text and if not runs script command.
        if($sqlStatements.Contains("-------"))
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

    $sqlScript = $camstarPath + "Industry Solutions\Medical Device\Scripts\Mssql\mdPopulatePortalMenuUpdateData.sql"
    $oracleScript = $camstarPath + "Industry Solutions\Medical Device\Scripts\Oracle\mdPopulatePortalMenuUpdateData.or.sql"

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
# MIIpegYJKoZIhvcNAQcCoIIpazCCKWcCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCAUcenAmE/U44h4
# +pTIb2HH2UVM2/+OjyfWGspFVVcTTqCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
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
# sz+9VNMWTdxGMYIaoTCCGp0CAQEwfTBpMQswCQYDVQQGEwJVUzEXMBUGA1UEChMO
# RGlnaUNlcnQsIEluYy4xQTA/BgNVBAMTOERpZ2lDZXJ0IFRydXN0ZWQgRzQgQ29k
# ZSBTaWduaW5nIFJTQTQwOTYgU0hBMzg0IDIwMjEgQ0ExAhADlxGexb7h6t0SRpEj
# 2gIUMA0GCWCGSAFlAwQCAQUAoHwwEAYKKwYBBAGCNwIBDDECMAAwGQYJKoZIhvcN
# AQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUw
# LwYJKoZIhvcNAQkEMSIEICEqjsM8KarsDn5utTECvmneLhusD571TujN5RzQLtf7
# MA0GCSqGSIb3DQEBAQUABIICAJqu6pL9qUaNlrFhYYTeabXr2nLL6El30jqELVo8
# E3P1pyubK7kG0A3lNVH7c7CADoL2LhtcKWiYuu3biHVhslHitDf6aoQwyxUtq0GX
# m+e7CnXSkd201BMcic+v7gZIega9f+p/QT/KxuPz19lLRavSYZH7t7LrsSah6kAY
# ATAcrADPycvb3VjwgCRxcALDn5vS/xFJrmEopGd/ArsCvXC21T3hjiMt/qPjil7j
# epUyhonAf7TuhVBY9a9vIDGM+1afcgBoazIQ2rtvVZvMqUNkEWeGqN1ovEK7/cLI
# nH5+60gZk4rVAYxr/ijtPT18TIbkc3notQUfCdiIdpf53rZjXm9qgmOO0hqEvBQg
# WsTsxlqEeErAdIw8Vo2hE/MVZrACysdA99M9x3x2tbxqCRKkm80+J2TE2aZNalqh
# ATycG2i4fduAoPiiDjS0B2TcAgvWt2IJRqLQYkzr47XD3qrkjOsgCiocSJKBO12H
# AUb5WX7WrAKl9/jocxoH/+EERzASgZW0lJmk2CAgZPnqBPxavis4iRbdewbhh5/I
# gBQIxMC0I1gAvbN9vfQOU+rNtYo2F7jNysG7YNcfJvLpLja2pBX2sp2CX4wn9PTO
# yPF3/l9gO/hmaybQuo2+h7nRs7bBeDA/u3bYpZY57KUgWhcQzTGXdGa+iBnzoZcp
# Hei3oYIXdzCCF3MGCisGAQQBgjcDAwExghdjMIIXXwYJKoZIhvcNAQcCoIIXUDCC
# F0wCAQMxDzANBglghkgBZQMEAgEFADB4BgsqhkiG9w0BCRABBKBpBGcwZQIBAQYJ
# YIZIAYb9bAcBMDEwDQYJYIZIAWUDBAIBBQAEIOrCHAK/UlaetFqE1VohpI8377vy
# Ak7rq6rbvHHwyLEyAhEA0jDSM2pgHMBS8LrsuaJL0xgPMjAyNTEyMjkxODQ3NDNa
# oIITOjCCBu0wggTVoAMCAQICEAqA7xhLjfEFgtHEdqeVdGgwDQYJKoZIhvcNAQEL
# BQAwaTELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDkRpZ2lDZXJ0LCBJbmMuMUEwPwYD
# VQQDEzhEaWdpQ2VydCBUcnVzdGVkIEc0IFRpbWVTdGFtcGluZyBSU0E0MDk2IFNI
# QTI1NiAyMDI1IENBMTAeFw0yNTA2MDQwMDAwMDBaFw0zNjA5MDMyMzU5NTlaMGMx
# CzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwgSW5jLjE7MDkGA1UEAxMy
# RGlnaUNlcnQgU0hBMjU2IFJTQTQwOTYgVGltZXN0YW1wIFJlc3BvbmRlciAyMDI1
# IDEwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQDQRqwtEsae0OquYFaz
# K1e6b1H/hnAKAd/KN8wZQjBjMqiZ3xTWcfsLwOvRxUwXcGx8AUjni6bz52fGTfr6
# PHRNv6T7zsf1Y/E3IU8kgNkeECqVQ+3bzWYesFtkepErvUSbf+EIYLkrLKd6qJnu
# zK8Vcn0DvbDMemQFoxQ2Dsw4vEjoT1FpS54dNApZfKY61HAldytxNM89PZXUP/5w
# WWURK+IfxiOg8W9lKMqzdIo7VA1R0V3Zp3DjjANwqAf4lEkTlCDQ0/fKJLKLkzGB
# Tpx6EYevvOi7XOc4zyh1uSqgr6UnbksIcFJqLbkIXIPbcNmA98Oskkkrvt6lPAw/
# p4oDSRZreiwB7x9ykrjS6GS3NR39iTTFS+ENTqW8m6THuOmHHjQNC3zbJ6nJ6SXi
# LSvw4Smz8U07hqF+8CTXaETkVWz0dVVZw7knh1WZXOLHgDvundrAtuvz0D3T+dYa
# NcwafsVCGZKUhQPL1naFKBy1p6llN3QgshRta6Eq4B40h5avMcpi54wm0i2ePZD5
# pPIssoszQyF4//3DoK2O65Uck5Wggn8O2klETsJ7u8xEehGifgJYi+6I03UuT1j7
# FnrqVrOzaQoVJOeeStPeldYRNMmSF3voIgMFtNGh86w3ISHNm0IaadCKCkUe2Lnw
# JKa8TIlwCUNVwppwn4D3/Pt5pwIDAQABo4IBlTCCAZEwDAYDVR0TAQH/BAIwADAd
# BgNVHQ4EFgQU5Dv88jHt/f3X85FxYxlQQ89hjOgwHwYDVR0jBBgwFoAU729TSunk
# Bnx6yuKQVvYv1Ensy04wDgYDVR0PAQH/BAQDAgeAMBYGA1UdJQEB/wQMMAoGCCsG
# AQUFBwMIMIGVBggrBgEFBQcBAQSBiDCBhTAkBggrBgEFBQcwAYYYaHR0cDovL29j
# c3AuZGlnaWNlcnQuY29tMF0GCCsGAQUFBzAChlFodHRwOi8vY2FjZXJ0cy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRUcnVzdGVkRzRUaW1lU3RhbXBpbmdSU0E0MDk2U0hB
# MjU2MjAyNUNBMS5jcnQwXwYDVR0fBFgwVjBUoFKgUIZOaHR0cDovL2NybDMuZGln
# aWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZEc0VGltZVN0YW1waW5nUlNBNDA5NlNI
# QTI1NjIwMjVDQTEuY3JsMCAGA1UdIAQZMBcwCAYGZ4EMAQQCMAsGCWCGSAGG/WwH
# ATANBgkqhkiG9w0BAQsFAAOCAgEAZSqt8RwnBLmuYEHs0QhEnmNAciH45PYiT9s1
# i6UKtW+FERp8FgXRGQ/YAavXzWjZhY+hIfP2JkQ38U+wtJPBVBajYfrbIYG+Dui4
# I4PCvHpQuPqFgqp1PzC/ZRX4pvP/ciZmUnthfAEP1HShTrY+2DE5qjzvZs7JIIgt
# 0GCFD9ktx0LxxtRQ7vllKluHWiKk6FxRPyUPxAAYH2Vy1lNM4kzekd8oEARzFAWg
# eW3az2xejEWLNN4eKGxDJ8WDl/FQUSntbjZ80FU3i54tpx5F/0Kr15zW/mJAxZMV
# BrTE2oi0fcI8VMbtoRAmaaslNXdCG1+lqvP4FbrQ6IwSBXkZagHLhFU9HCrG/syT
# RLLhAezu/3Lr00GrJzPQFnCEH1Y58678IgmfORBPC1JKkYaEt2OdDh4GmO0/5cHe
# lAK2/gTlQJINqDr6JfwyYHXSd+V08X1JUPvB4ILfJdmL+66Gp3CSBXG6IwXMZUXB
# htCyIaehr0XkBoDIGMUG1dUtwq1qmcwbdUfcSYCn+OwncVUXf53VJUNOaMWMts0V
# lRYxe5nK+At+DI96HAlXHAL5SlfYxJ7La54i71McVWRP66bW+yERNpbJCjyCYG2j
# +bdpxo/1Cy4uPcU3AWVPGrbn5PhDBf3Froguzzhk++ami+r3Qrx5bIbY3TVzgiFI
# 7Gq3zWcwgga0MIIEnKADAgECAhANx6xXBf8hmS5AQyIMOkmGMA0GCSqGSIb3DQEB
# CwUAMGIxCzAJBgNVBAYTAlVTMRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNV
# BAsTEHd3dy5kaWdpY2VydC5jb20xITAfBgNVBAMTGERpZ2lDZXJ0IFRydXN0ZWQg
# Um9vdCBHNDAeFw0yNTA1MDcwMDAwMDBaFw0zODAxMTQyMzU5NTlaMGkxCzAJBgNV
# BAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwgSW5jLjFBMD8GA1UEAxM4RGlnaUNl
# cnQgVHJ1c3RlZCBHNCBUaW1lU3RhbXBpbmcgUlNBNDA5NiBTSEEyNTYgMjAyNSBD
# QTEwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQC0eDHTCphBcr48RsAc
# rHXbo0ZodLRRF51NrY0NlLWZloMsVO1DahGPNRcybEKq+RuwOnPhof6pvF4uGjwj
# qNjfEvUi6wuim5bap+0lgloM2zX4kftn5B1IpYzTqpyFQ/4Bt0mAxAHeHYNnQxqX
# mRinvuNgxVBdJkf77S2uPoCj7GH8BLuxBG5AvftBdsOECS1UkxBvMgEdgkFiDNYi
# OTx4OtiFcMSkqTtF2hfQz3zQSku2Ws3IfDReb6e3mmdglTcaarps0wjUjsZvkgFk
# riK9tUKJm/s80FiocSk1VYLZlDwFt+cVFBURJg6zMUjZa/zbCclF83bRVFLeGkuA
# hHiGPMvSGmhgaTzVyhYn4p0+8y9oHRaQT/aofEnS5xLrfxnGpTXiUOeSLsJygoLP
# p66bkDX1ZlAeSpQl92QOMeRxykvq6gbylsXQskBBBnGy3tW/AMOMCZIVNSaz7BX8
# VtYGqLt9MmeOreGPRdtBx3yGOP+rx3rKWDEJlIqLXvJWnY0v5ydPpOjL6s36czwz
# sucuoKs7Yk/ehb//Wx+5kMqIMRvUBDx6z1ev+7psNOdgJMoiwOrUG2ZdSoQbU2rM
# kpLiQ6bGRinZbI4OLu9BMIFm1UUl9VnePs6BaaeEWvjJSjNm2qA+sdFUeEY0qVjP
# KOWug/G6X5uAiynM7Bu2ayBjUwIDAQABo4IBXTCCAVkwEgYDVR0TAQH/BAgwBgEB
# /wIBADAdBgNVHQ4EFgQU729TSunkBnx6yuKQVvYv1Ensy04wHwYDVR0jBBgwFoAU
# 7NfjgtJxXWRM3y5nP+e6mK4cD08wDgYDVR0PAQH/BAQDAgGGMBMGA1UdJQQMMAoG
# CCsGAQUFBwMIMHcGCCsGAQUFBwEBBGswaTAkBggrBgEFBQcwAYYYaHR0cDovL29j
# c3AuZGlnaWNlcnQuY29tMEEGCCsGAQUFBzAChjVodHRwOi8vY2FjZXJ0cy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRUcnVzdGVkUm9vdEc0LmNydDBDBgNVHR8EPDA6MDig
# NqA0hjJodHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNlcnRUcnVzdGVkUm9v
# dEc0LmNybDAgBgNVHSAEGTAXMAgGBmeBDAEEAjALBglghkgBhv1sBwEwDQYJKoZI
# hvcNAQELBQADggIBABfO+xaAHP4HPRF2cTC9vgvItTSmf83Qh8WIGjB/T8ObXAZz
# 8OjuhUxjaaFdleMM0lBryPTQM2qEJPe36zwbSI/mS83afsl3YTj+IQhQE7jU/kXj
# jytJgnn0hvrV6hqWGd3rLAUt6vJy9lMDPjTLxLgXf9r5nWMQwr8Myb9rEVKChHyf
# pzee5kH0F8HABBgr0UdqirZ7bowe9Vj2AIMD8liyrukZ2iA/wdG2th9y1IsA0QF8
# dTXqvcnTmpfeQh35k5zOCPmSNq1UH410ANVko43+Cdmu4y81hjajV/gxdEkMx1NK
# U4uHQcKfZxAvBAKqMVuqte69M9J6A47OvgRaPs+2ykgcGV00TYr2Lr3ty9qIijan
# rUR3anzEwlvzZiiyfTPjLbnFRsjsYg39OlV8cipDoq7+qNNjqFzeGxcytL5TTLL4
# ZaoBdqbhOhZ3ZRDUphPvSRmMThi0vw9vODRzW6AxnJll38F0cuJG7uEBYTptMSbh
# dhGQDpOXgpIUsWTjd6xpR6oaQf/DJbg3s6KCLPAlZ66RzIg9sC+NJpud/v4+7RWs
# WCiKi9EOLLHfMR2ZyJ/+xhCx9yHbxtl5TPau1j/1MIDpMPx0LckTetiSuEtQvLsN
# z3Qbp7wGWqbIiOWCnb5WqxL3/BAPvIXKUjPSxyZsq8WhbaM2tszWkPZPubdcMIIF
# jTCCBHWgAwIBAgIQDpsYjvnQLefv21DiCEAYWjANBgkqhkiG9w0BAQwFADBlMQsw
# CQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cu
# ZGlnaWNlcnQuY29tMSQwIgYDVQQDExtEaWdpQ2VydCBBc3N1cmVkIElEIFJvb3Qg
# Q0EwHhcNMjIwODAxMDAwMDAwWhcNMzExMTA5MjM1OTU5WjBiMQswCQYDVQQGEwJV
# UzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQu
# Y29tMSEwHwYDVQQDExhEaWdpQ2VydCBUcnVzdGVkIFJvb3QgRzQwggIiMA0GCSqG
# SIb3DQEBAQUAA4ICDwAwggIKAoICAQC/5pBzaN675F1KPDAiMGkz7MKnJS7JIT3y
# ithZwuEppz1Yq3aaza57G4QNxDAf8xukOBbrVsaXbR2rsnnyyhHS5F/WBTxSD1If
# xp4VpX6+n6lXFllVcq9ok3DCsrp1mWpzMpTREEQQLt+C8weE5nQ7bXHiLQwb7iDV
# ySAdYyktzuxeTsiT+CFhmzTrBcZe7FsavOvJz82sNEBfsXpm7nfISKhmV1efVFiO
# DCu3T6cw2Vbuyntd463JT17lNecxy9qTXtyOj4DatpGYQJB5w3jHtrHEtWoYOAMQ
# jdjUN6QuBX2I9YI+EJFwq1WCQTLX2wRzKm6RAXwhTNS8rhsDdV14Ztk6MUSaM0C/
# CNdaSaTC5qmgZ92kJ7yhTzm1EVgX9yRcRo9k98FpiHaYdj1ZXUJ2h4mXaXpI8OCi
# EhtmmnTK3kse5w5jrubU75KSOp493ADkRSWJtppEGSt+wJS00mFt6zPZxd9LBADM
# fRyVw4/3IbKyEbe7f/LVjHAsQWCqsWMYRJUadmJ+9oCw++hkpjPRiQfhvbfmQ6QY
# uKZ3AeEPlAwhHbJUKSWJbOUOUlFHdL4mrLZBdd56rF+NP8m800ERElvlEFDrMcXK
# chYiCd98THU/Y+whX8QgUWtvsauGi0/C1kVfnSD8oR7FwI+isX4KJpn15GkvmB0t
# 9dmpsh3lGwIDAQABo4IBOjCCATYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQU
# 7NfjgtJxXWRM3y5nP+e6mK4cD08wHwYDVR0jBBgwFoAUReuir/SSy4IxLVGLp6ch
# nfNtyA8wDgYDVR0PAQH/BAQDAgGGMHkGCCsGAQUFBwEBBG0wazAkBggrBgEFBQcw
# AYYYaHR0cDovL29jc3AuZGlnaWNlcnQuY29tMEMGCCsGAQUFBzAChjdodHRwOi8v
# Y2FjZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3J0
# MEUGA1UdHwQ+MDwwOqA4oDaGNGh0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdp
# Q2VydEFzc3VyZWRJRFJvb3RDQS5jcmwwEQYDVR0gBAowCDAGBgRVHSAAMA0GCSqG
# SIb3DQEBDAUAA4IBAQBwoL9DXFXnOF+go3QbPbYW1/e/Vwe9mqyhhyzshV6pGrsi
# +IcaaVQi7aSId229GhT0E0p6Ly23OO/0/4C5+KH38nLeJLxSA8hO0Cre+i1Wz/n0
# 96wwepqLsl7Uz9FDRJtDIeuWcqFItJnLnU+nBgMTdydE1Od/6Fmo8L8vC6bp8jQ8
# 7PcDx4eo0kxAGTVGamlUsLihVo7spNU96LHc/RzY9HdaXFSMb++hUD38dglohJ9v
# ytsgjTVgHAIDyyCwrFigDkBjxZgiwbJZ9VVrzyerbHbObyMt9H5xaiNrIv8SuFQt
# J37YOtnwtoeW/VvRXKwYw02fc7cBqZ9Xql4o4rmUMYIDfDCCA3gCAQEwfTBpMQsw
# CQYDVQQGEwJVUzEXMBUGA1UEChMORGlnaUNlcnQsIEluYy4xQTA/BgNVBAMTOERp
# Z2lDZXJ0IFRydXN0ZWQgRzQgVGltZVN0YW1waW5nIFJTQTQwOTYgU0hBMjU2IDIw
# MjUgQ0ExAhAKgO8YS43xBYLRxHanlXRoMA0GCWCGSAFlAwQCAQUAoIHRMBoGCSqG
# SIb3DQEJAzENBgsqhkiG9w0BCRABBDAcBgkqhkiG9w0BCQUxDxcNMjUxMjI5MTg0
# NzQzWjArBgsqhkiG9w0BCRACDDEcMBowGDAWBBTdYjCshgotMGvaOLFoeVIwB/tB
# fjAvBgkqhkiG9w0BCQQxIgQgjGfJF6IojYioCVHUUkEBsQH2Luqk5J/J8ThTtysD
# jOUwNwYLKoZIhvcNAQkQAi8xKDAmMCQwIgQgSqA/oizXXITFXJOPgo5na5yuyrM/
# 420mmqM08UYRCjMwDQYJKoZIhvcNAQEBBQAEggIAela62gNCk0HHfxZWgNh5D1ag
# ejKUqr3QdaV6OdHdPOnMI+5kb3scPjeS+YJUnCuMjMhDiOd+SfYKcMr+w/Fae6Va
# cMIWsSPm31v2aFzF9DYh0JHN8hcN6LlH7UH/nYjkjOGrw+Y/ZfpT9ylL057WhNN1
# 0NN9Uux9jq6jKtmszv0+xYiAAvd3/NitE5B9eNVJW6DeAGcXsCluyDMSIrQcmjpi
# hHOh681K+OdlDZHsFONe7zlZRkJovEAgkZOrUx9LGIDl5n62jwhcx4U/iRdAIqEF
# 1vx0LlCstnrP0JW3+lkYazzeuNWTgYzf2Rx/gCUvITys8v4eWf2rvL9WlXYWgbyM
# InH+RzvFHWbqRJuSbkjB7eOBGBbiov11ApvxMff9KkBxHuNaTPHoIvuNCACgAE/C
# BshSYm1+wn33MupzmdTKso+2bblKNsetktZBkgkZgB8QjXhpKN4oJ6JlMKF5f7Uf
# BoOTaaenj5W+vMI5U+F/xLVen940bTg5KfUkWIpLEPwTdLCbZxbEU5cd1/PBbg59
# sxG5l2T2+A3AFNsu5ZyxHE4YUGLxnCdV0877nhFu13uLL09XBp43oKs4uadTFoAh
# zakMfBtwcE0+7WnzqAlp0bJb13z5MMZaFY8IPKEPU/6Q00CyAhLYY8prLdw46oBs
# S0nCII4pV9Kx/GcSpOc=
# SIG # End signature block
