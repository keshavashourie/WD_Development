# Copyright Siemens 2025

######################################
#####                            #####
#####          Methods           #####
#####                            #####
######################################

#region Global Parameters

param
(
	
	[Parameter(Position = 1)]
	[bool]$isHosted,

    [Parameter(Mandatory=$False,Position = 2)]
	[string]$selectedWorkspace,

    [Parameter(Mandatory=$False,Position = 3)]
	[string]$workspaceAbbreviation,

    [Parameter(Mandatory=$False,Position = 4)]
	[string[]]$workspacesParam,

    [Parameter(Mandatory=$False,Position = 5)]
	[string]$installType,

    [Parameter(Mandatory=$False,Position = 6)]
	[string]$camstarPath = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Path")."(Default)",

    [Parameter(Mandatory=$False,Position = 7)]
	[string]$insiteMDB = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar Insite Common").TemplateDatabaseFile,

    [Parameter(Mandatory=$False,Position = 8)]
    [xml] $xml,

    [Parameter(Mandatory=$False,Position = 9)]
    [xml] $xmlSettings,

    [Parameter(Mandatory=$False,Position = 10)]
    [System.Xml.XmlNode]$ReportHost,

    [Parameter(Mandatory=$False,Position = 11)]
    [System.Xml.XmlNode]$ReportHostsToAdd,

    [Parameter(Mandatory=$False,Position = 12)]
    [System.Xml.XmlNode]$docNodes,

    [Parameter(Mandatory=$False,Position = 13)]
	[string]$camstarServerPath = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Server\Path")."(Default)",

    [Parameter(Mandatory=$False,Position = 14)]
	[string]$dbtype = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar InSite Common").TxnDBType,

    [Parameter(Mandatory=$False,Position = 15)]
	[string]$dsDBPort,

    [Parameter(Mandatory=$False,Position = 16)]
	[string]$dsDBName,

    [Parameter(Mandatory=$False,Position = 17)]
	[string]$dsDBPassword,

    [Parameter(Mandatory=$False,Position = 18)]
	[string]$dsDBUserName,

    [Parameter(Mandatory=$False,Position = 19)]
	[string]$dsDBServer,

    [Parameter(Mandatory=$False,Position = 20)]
	[bool]$isOEE,

    [Parameter(Mandatory=$False,Position = 21)]
    [string]$oracleConnectionString,

    [Parameter(Mandatory=$False,Position = 22)]
    [string]$ODACHintPath = "Oracle.ManagedDataAccess.dll",

	[Parameter(Mandatory=$False,Position = 23)]
    [bool]$ReplaceMetaData=$true,

	[Parameter(Mandatory=$False,Position = 24)]
    [bool]$DBUpdate=$true,
    
    [Parameter(Mandatory=$False,Position = 25)]
    [string]$logFile,

    [Parameter(Mandatory=$False,Position = 26)]
    [bool]$isSRC,

    [Parameter(Mandatory=$False,Position = 27)]
    [string]$OPEXPassword,

    [Parameter(Mandatory=$False,Position = 28)]
    [string]$OPEXUserName,

    [Parameter(Mandatory=$False,Position = 29)]
    [string]$CNMOMPassword,

    [Parameter(Mandatory=$False,Position = 30)]
    [string]$CNMOMUserName,

    [Parameter(Mandatory=$False,Position = 31)]
    [string]$CNMOMGatewayHostHame,

    [Parameter(Mandatory=$False,Position = 32)]
    [string]$CNMOMGatewayName,

    [Parameter(Mandatory=$False,Position = 33)]
    [string]$CNMOMHostNames,

    [Parameter(Mandatory=$False,Position = 34)]
    [string]$CNMOMServer,

	[Parameter(Mandatory=$False,Position = 35)]
	[string]$PortNumber = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar Insite Common").AppServerPortOnInstall,

	[Parameter(Mandatory=$False,Position = 36)]
	[string]$ServerName = (Get-ItemProperty "HKLM:\Software\Wow6432Node\Camstar\Camstar Insite Common").AppServerNameOnInstall

) 

#endregion
if ($logFile -eq "")
{
    $logFile = Join-Path $PSScriptRoot IS_InstallLog.txt
}
. ($camstarPath + "Industry Solutions\Logging.ps1")
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
        [Parameter(Mandatory = $false)]
        [string]$OraclePath,
        [Parameter(Mandatory = $false)]
        [bool]$NoPromptForPath = $false
    )       

    # Get Oracle path 
    Write-LogInfo("Initial OraclePath: $OraclePath")
    $odac_home = "";

    if ($OraclePath -eq '' -or ($OraclePath -notmatch "Oracle.ManagedDataAccess.dll`$") -or !(Test-Path -Path $OraclePath) ) 
    {
        [bool] $finished = $false
        [string]$basePath = $null

        if ( $OraclePath -and ((Test-Path -Path $OraclePath))) {

            $searchResults = get-childitem -path $OraclePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
            where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
            sort-object FullName -Descending | Select-Object FullName

            # if found
            if ($null -ne $searchResults) { 
				Write-LogInfo("Search found Oracle.ManagedDataAccess.dll")
                $OraclePath = $searchResults[0].FullName
                $finished = $true
            }
            else {
                Write-LogError("`nError: Oracle.ManagedDataAccess.dll not found in folder tree.")
                $finished = $false
            }
        }     

        if (!$finished)
        {
			Write-LogInfo("Search in Registry for ORACLE_HOME")

            # First try a reg search.
            try{
                $odac_home = Get-ItemProperty -Path "HKLM:\SOFTWARE\Oracle\*" -Name "ORACLE_HOME" -ErrorAction SilentlyContinue
                $basePath = $odac_home.ORACLE_HOME
                Write-LogInfo("ORACLE_HOME basePath from reg search 1:  $basePath")
            }
            catch{
                $basePath = $null
            }
            
            if (!$basePath) {
				Write-LogInfo("Search in Registry for specific Oracle KEY")

                # Try known key/value.
                try{
                    $basePath = (Get-ItemProperty "HKLM:\SOFTWARE\Oracle\KEY_odac")."ORACLE_HOME" 
                    Write-LogInfo("ORACLE_HOME basePath from reg search 2:  $basePath")
                }
                catch{
                    $basePath = $null
                }
            }
            
            if (!$basePath) {
                # Try another known key/value.
                try{
                    $basePath = (Get-ItemProperty "HKLM:\SOFTWARE\Oracle\KEY_both")."ORACLE_HOME" 
                    Write-LogInfo("ORACLE_HOME basePath from reg search 3:  $basePath")
                }
                catch{
                    $basePath = $null
                }
            }

            if ($odac_home.GetType().BaseType.Name -eq "Array") {
				Write-LogInfo("Iterate through array of ORACLE_HOME entries")
                foreach($item in $odac_home)
                {
                    $basePath = $item.ORACLE_HOME
                    Write-LogInfo("Oracle Base Path: $basePath")

                    $searchResults = get-childitem -path $basePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
                    where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
                    sort-object FullName -Descending | Select-Object FullName

                    # if found
                    if ($null -ne $searchResults) { 
                        $OraclePath = $searchResults[0].FullName
                        $finished = $true
                        break
                    }
                    else {
                        Write-LogError("`nError: Oracle.ManagedDataAccess.dll not found in folder tree.")
                        $finished = $false
                        $basePath = $null
                    }
                }
            } else {
                if ($basePath) {
				    Write-LogInfo("Oracle Base Path: $basePath")

                    $searchResults = get-childitem -path $basePath -Filter 'Oracle.ManagedDataAccess.dll' -Recurse -ErrorAction SilentlyContinue | 
                    where-object { $_.FullName -match "\\Oracle.ManagedDataAccess.dll`$" } | 
                    sort-object FullName -Descending | Select-Object FullName

                    # if found
                    if ($null -ne $searchResults) { 
                        $OraclePath = $searchResults[0].FullName
                        $finished = $true
                    }
                    else {
                        Write-LogError("`nError: Oracle.ManagedDataAccess.dll not found in folder tree.")
                        $finished = $false
                        $basePath = $null
                    }
                }
            }
        }
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
                Write-LogInfo("Loaded ODAC from:  $asm.CodeBase;")
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
                Write-LogInfo("Loaded ODAC from: $asm.CodeBase;")
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
            Write-LogInfo("Get-OraclePath returned: $oraclePath")
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


#region Main

Function Main()
{
    $scriptPath = $PSScriptRoot
	#$logFile = ($scriptPath + "\IS_InstallLog.txt")

	start-transcript -path $scriptPath\IndustrySolutionsInstallation.log

	Write-LogInfo("`nBeginning Industry Solutions Installation...")
    Write-LogInfo("Mode: $installType, ReplaceMetadata: $ReplaceMetaData, DBUpdate: $DBUpdate")

    if($isOEE)
    {
		Write-LogInfo("`nInstalling OEE so test DB connection")
        testConnection $dsDBServer $dsDBPort $dsDBName $dsDBUserName $dsDBPassword
    }

    $wsCount = 0
    foreach ($element in $workspacesParam)
	{
		Write-LogInfo("`nWorkspace $element will be installed")
		$wsCount = $wsCount+1
	}
	if ($wsCount -eq 0)
    {
		$DBUpdate = $false
    }

	Cleanup($workspacesParam)
    if ($ReplaceMetaData){
        ReplaceMetaData($workspacesParam)
    }
	
    if($installType -eq 'Application Server')
    {
        MoveCodeBehindFiles($workspacesParam)
        UpdateMDLinks($workspacesParam)
        UpdateMDLinks($workspacesParam)
        MovePortalData($workspacesParam)
        if ($DBUpdate){
            GenerateWCF
        }
    }
    elseif($installType -eq 'Full Install')
    {
        MoveCodeBehindFiles($workspacesParam)
        UpdateMDLinks($workspacesParam)
        UpdateMDLinks($workspacesParam)
        MovePortalData($workspacesParam)
        if ($DBUpdate){
            UpdateDBTables
            GenerateWCF
            Run-DBScripts($workspacesParam)
        }
    }

    if($isOEE)
    {
        OEE $dsDBServer $dsDBPort $dsDBName $dsDBUserName $dsDBPassword
    }

    if (!($OPEXPassword -eq ''))
    {
        RunCTTScripts
    }

    if($isSRC)
    {
		if ($DBUpdate -eq $false) {
			$workspacesParam = @('Industry', 'SRCInstall')
			Run-DBScripts($workspacesParam)
		}

        InstallSrc
    }
}

#endregion

#region ReplaceMetaData

Function ReplaceMetaData([string[]]$workspace)
{
	Write-LogInfo("`nReplace MetaData")

    #region Back Up Insite Admin MDB

    $insiteAdminmdb = $insiteMDB
    [string[]]$getMDBName = $insiteAdminmdb.Split("\\")

    $mdbName = $getMDBName[-1]

    $toBackup = ($camstarPath + "InSite Administration\BackUpMDB\")
    $Backupmdb = $toBackup + "\" + $mdbName
    $mdbName2 = $mdbName.Replace(".mdb", "")
    $testPath = Test-Path -Path $toBackup
    $insiteAdminMDBPath = $insiteAdminmdb.Replace("\" + $mdbName, "")

    if($testPath -eq $False)
    {
        New-Folder -Path ($toBackup + "\") 
    }

    for($x = 0; $x -lt 1; $x++)
    {
        Copy-Item $insiteAdminmdb -Destination $toBackup -Force -Recurse
        $datetime = Get-Date -Format yyyyMMMd-hhmm
        Rename-Item $Backupmdb -NewName ($mdbName2 + "_" + $datetime + ".mdb")
    }

    #endregion

    #region Run Replacement Tool

    foreach ($element in $workspace)
    {        

            $selectedWorkspace = $element

			Write-LogInfo("`nReplace workspace: $workspace")

            #Replacement File Paths

            $toCurrent = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Current")
            $toBase = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Base")
            $toNew = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\New")
            $toTarget = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Target")
            $log = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\UpgradeLog.log")

            #Create New Files

            New-Folder -Path ($camstarPath + "InSite Administration\" + $selectedWorkspace)
            New-Folder -Path $toCurrent
            New-Folder -Path $toBase
            New-Folder -Path $toTarget
            New-Folder -Path $toNew

            #Source MDBs

            $basemetadata = ($camstarPath + "InSite Administration\Base Metadata\Insite.mdb")

            #Replacement MDBs

            $basemdb = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Base\InSite.mdb")
            $targetmdb = ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Target\Insite.mdb")
            $newmdb = Get-ChildItem -Path ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\MetaData\*.*") -include *.mdb
            $newTargetmdb = Get-ChildItem -Path ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Target\*.*") -include *.mdb

            #Copy/Paste mdb's into correct paths

            Copy-Item $basemetadata -Destination $toBase -Force -Recurse
            Copy-Item $basemetadata -Destination $toTarget -Force -Recurse
            Copy-Item $insiteAdminmdb -Destination $toCurrent -Force -Recurse
            Copy-Item $newmdb -Destination $toNew -Force -Recurse

            $currentmdb = Get-ChildItem -Path ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\Current\*.*") -include *.mdb
            $toNewmdb = Get-ChildItem -Path ($camstarPath + "InSite Administration\" + $selectedWorkspace + "\New\*.*") -include *.mdb

            Set-ItemProperty $currentmdb -name IsReadOnly -value $false
            Set-ItemProperty $toNewmdb -name IsReadOnly -value $false
            Set-ItemProperty $basemdb -name IsReadOnly -value $false
            Set-ItemProperty $targetmdb -name IsReadOnly -value $false

            #Execute Replacement Tool

            $replace = ($camstarPath + "InSite Conversion Tool\Merge Tool\ReplaceWorkspaceData.exe")
            $params = @("-log:""$log"" -base:""$basemdb"" -new:""$toNewmdb"" -current:""$currentmdb"" -target:""$targetmdb"" -replace_current_MDB:false -start_at_new_ws:true" )

            Start-Process $replace -ArgumentList $params -Verb runas -Wait -windowstyle hidden

            Rename-Item $targetmdb -NewName $mdbName
            Copy-Item ($toTarget + "\" + $mdbName) -Destination $insiteAdminMDBPath -Force -Recurse
            Copy-Item ($toTarget + "\" + $mdbName) -Destination $toBackup -Force -Recurse

            Rename-Item ($toBackup + "\" + $mdbName) -NewName ($mdbName2 + "_" + $datetime + "_" + $selectedWorkspace + ".mdb")

    }

	Write-LogInfo("`nReplace workspace comple")
    #endregion

}

#endregion

#region Move .cs files

Function MoveCodeBehindFiles([string[]]$workspace)
{
	Write-LogInfo("`Move Code Behind files")

    New-Folder -Path ($camstarServerPath + "\Plugins")
    $plugins = ($camstarServerPath + "\Plugins")

    foreach ($element in $workspace)
    {
        $selectedWorkspace = $element

		Write-LogInfo("`Installing workspace: $selectedWorkspace")

        #Move .cs files

        $shopfloor = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\App_Code\Shopfloor\*")
        $toShopfloor = ($camstarPath + "Camstar Portal\App_Code\WebPortlets\Shopfloor")
        $modeling = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\App_Code\Modeling\*")
        $toModeling = ($camstarPath + "Camstar Portal\App_Code\WebPortlets\Modeling")
        $compissue = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\App_Code\ComponentIssue\*")
        $toCompissue = ($camstarPath + "Camstar Portal\App_Code\WebPortlets\ComponentIssue")

        if($selectedWorkspace -eq "CIO")
        {
            $cioPluginPath = ($camstarPath + "Industry Solutions\CIO\Plugins\*")
            # Bug 52114 
            # TODO: Review to uncomment after US 50095.
            #if (Test-Path ($camstarPath + "Industry Solutions\CIO\Plugins\CTTPlugin.OutputToREST.dll")) {
            #    Remove-Item ($camstarPath + "Industry Solutions\CIO\Plugins\CTTPlugin.OutputToREST.dll") -Force
            #}

            
            # Workaround until bug is fixed in plugins.dll (MIO) to not 
            # throw an exception when it can't find a dir it assumes is
            # there.
            New-Folder -Path ("C:\Windows\system32\inetsrv\Plugins")

			Write-LogInfo("`Copy files from folder: $cioPluginPath")

			Write-LogInfo("`Copy to folder: $camstarServerPath")

			Try
			{
                Copy-Item $cioPluginPath -Destination $camstarServerPath -Force -Recurse -ErrorAction Continue
                # Bug 52114
                if (Test-Path ($camstarServerPath + "\CTTPlugin.OutputToREST.dll")) {
                    Remove-Item ($camstarServerPath + "\CTTPlugin.OutputToREST.dll") -Force
                }
			}
			Catch
			{
				$ErrorMessage = $_.Exception.Message
				$FailedItem = $_.Exception.ItemName
				Write-LogError("`Error: $ErrorMessage")
            }
            
			Write-LogInfo("`Register assembly and generate TLB")

            Try
			{
                $regsvr = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
                $CIOPluginHandler = ($camstarServerPath + "\CIOPluginHandler.dll")
                & cmd /c $regsvr /s $CIOPluginHandler /tlb /codebase
                if ($?) {
                Write-LogInfo("$CIOPluginHandler registered correctly.")
                } else {
                Write-LogError("`"Registration of $CIOPluginHandler failed. ($LASTEXITCODE)")
                }
			}
			Catch
			{
				$ErrorMessage = $_.Exception.Message
				$FailedItem = $_.Exception.ItemName
				Write-LogError("`Error: $ErrorMessage")
            }

            try {
                [string]$configFilePath = ($camstarServerPath + "\web.config");
                [System.Configuration.ExeConfigurationFileMap]$cfgFileMap = [System.Configuration.ExeConfigurationFileMap]::new();
                $cfgFileMap.ExeConfigFilename = $configFilePath;
                [System.Configuration.Configuration]$cfg = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration($cfgFileMap,0);
                #Write-Host "FilePath: " + $cfg.FilePath;
                #Write-Host "Has File: " + $cfg.HasFile;
                [System.Configuration.AppSettingsSection] $appSec = ([System.Configuration.AppSettingsSection]$cfg.GetSection("appSettings"));
                if (!$appSec.Settings.AllKeys.Contains("PlugInsDirectory"))
                {
                    $appSec.Settings.Add("PlugInsDirectory", ($camstarServerPath + "\Plugins"));
                    $cfg.Save();
                }
            }
            catch {
                $ErrorMessage = $_.Exception.Message
				$FailedItem = $_.Exception.ItemName
				Write-LogError("`Error: $ErrorMessage")
            }

            Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.applicationHost/applicationPools/add[@name='CamstarAppServer']" -name "CLRConfigFile" -value ($camstarServerPath + "\AspNet.config")
        }

        if($selectedWorkspace -eq "Batch Processing")
        {
			New-Folder -Path ($camstarPath + "Camstar Portal\Scripts\User\BatchProcessing")
			New-Folder -Path ($camstarPath + "Camstar Portal\Themes\User\BatchProcessing")

            $javaScript = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Scripts\User\BatchProcessing\*")
            $javaScriptPath = ($camstarPath + "Camstar Portal\Scripts\User\BatchProcessing")
            $css = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Themes\User\BatchProcessing\*")
            $cssPath = ($camstarPath + "Camstar Portal\Themes\User\BatchProcessing")

            Copy-Item $javaScript -Destination $javaScriptPath -Force -Recurse
            Copy-Item $css -Destination $cssPath -Force -Recurse
        }

        $file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Helpers\isDocumentService.cs")
		if (Test-Path -Path $file)
		{
			Write-LogInfo("`Remove file: $file")
			Remove-Item -Path $file -Force
		} else
		{
			Write-LogInfo("`NOT Removing file: $file")				
		}

        $file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Modeling\isImageMaint.cs")
		if (Test-Path -Path $file)
		{
			Write-LogInfo("`Remove file: $file")
			Remove-Item -Path $file -Force
		} else
		{
			Write-LogInfo("`NOT Removing file: $file")				
		}			

        $file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Modeling\isAttachImageHelper.cs")
		if (Test-Path -Path $file)
		{
			Write-LogInfo("`Remove file: $file")
			Remove-Item -Path $file -Force
		} else
		{
			Write-LogInfo("`NOT Removing file: $file")				
		}	
            
        $file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Shopfloor\isCurrentDefect.cs")
		if (Test-Path -Path $file)
		{
			Write-LogInfo("`Remove file: $file")
			Remove-Item -Path $file -Force
		} else
		{
			Write-LogInfo("`NOT Removing file: $file")				
		}

        $file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Shopfloor\isDefect.cs")
		if (Test-Path -Path $file)
		{
			Write-LogInfo("`Remove file: $file")
			Remove-Item -Path $file -Force
		} else
		{
			Write-LogInfo("`NOT Removing file: $file")				
		}	

        Copy-Item $shopfloor -Destination $toShopfloor -Force -Recurse
        Copy-Item $modeling -Destination $toModeling -Force -Recurse
		if (Test-Path -Path $compIssue)
		{
			Copy-Item $compissue -Destination $toCompissue -Force -Recurse
		}
        if($selectedWorkspace -eq "Industry")
        {
            New-Folder -Path ($camstarPath + "Camstar Portal\Scripts\User\Industry Solutions")
            New-Folder -Path ($camstarPath + "Camstar Portal\Themes\User\Industry Solutions")

            $javaScript = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Scripts\User\Industry Solution\*")
            $javaScriptPath = ($camstarPath + "Camstar Portal\Scripts\User\Industry Solutions")
            Copy-Item $javaScript -Destination $javaScriptPath -Force -Recurse
            
			$css = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Themes\User\Industry Solution\*")
            $cssPath = ($camstarPath + "Camstar Portal\Themes\User\Industry Solutions")
            Copy-Item $css -Destination $cssPath -Force -Recurse

            $css = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Themes\User\Camstar\*")
            $cssPath = ($camstarPath + "Camstar Portal\Themes\Camstar\User")
            Copy-Item $css -Destination $cssPath -Force -Recurse

            $css = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\Themes\User\*.css")
            $cssPath = ($camstarPath + "Camstar Portal\Themes\User")
            Copy-Item $css -Destination $cssPath -Force

            $html = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\User\*")
            $htmlPath = ($camstarPath + "Camstar Portal\User")
            Copy-Item $html -Destination $htmlPath -Force -Recurse

			$helpers = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\App_Code\Helpers\*")
			$toHelpers = ($camstarPath + "Camstar Portal\App_Code\Helpers")
			Copy-Item $helpers -Destination $toHelpers -Force -Recurse

			$file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\DocumentViewer.html")
			$toFile = ($camstarPath + "Camstar Portal\")
			Copy-Item $file -Destination $toFile -Force 

			$file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\isDocumentHandler.ashx")
			$toFile = ($camstarPath + "Camstar Portal\")
			Copy-Item $file -Destination $toFile -Force			
			
			$file = ($camstarPath + "Industry Solutions\Industry\Camstar Portal\isDefectService.svc")
			$toFile = ($camstarPath + "Camstar Portal\")
			Copy-Item $file -Destination $toFile -Force 

            # Remove code-behind files that no longer exist in current release
            $shopfloorFile = ($camstarPath + "Camstar Portal\App_Code\WebPortlets\Shopfloor\isCurrentDefect.cs")
		    if (Test-Path -Path $shopfloorFile)
		    {
				Write-LogInfo("`Remove file: $shopfloorFile")

			    Remove-Item -Path $shopfloorFile -Force
		    } else
			{
				Write-LogInfo("`NOT Removing file: $shopfloorFile")				
			}
            $shopfloorFile = ($camstarPath + "Camstar Portal\App_Code\WebPortlets\Shopfloor\isDefect.cs")
		    if (Test-Path -Path $shopfloorFile)
		    {
				Write-LogInfo("`Remove file: $shopfloorFile")
			    Remove-Item -Path $shopfloorFile -Force
		    } else
			{
				Write-LogInfo("`NOT Removing file: $shopfloorFile")				
			}

            $file = ($camstarPath + "Camstar Portal\App_Code\Helpers\isDocumentService.cs")
		    if (Test-Path -Path $file)
		    {
				Write-LogInfo("`Remove file: $file")
			    Remove-Item -Path $file -Force
		    } else
			{
				Write-LogInfo("`NOT Removing file: $file")				
			}					
        }
    }

	#  Attempt to copy isCustomProcPlugins.dll for all workspaces

    $isPlug = ($camstarPath + "Industry Solutions\Industry\Plugins\isCustomProcPlugins.dll")
        
	Write-LogInfo("`Copy file: $isPlug to $plugins")
		
	Try
	{
		Copy-Item $isPlug -Destination $plugins -Force -Recurse
	}
	Catch
	{
		$ErrorMessage = $_.Exception.Message
		$FailedItem = $_.Exception.ItemName
		Write-LogError("`Error: $ErrorMessage")
	}
}

#endregion

#region Move PortalData

Function MovePortalData([string[]]$workspace)
{
	Write-LogInfo("`Move Portal Data files")
    foreach ($element in $workspace)
    {        
        $selectedWorkspace = $element

        $newPortalData = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\PortalData\*")
        $originalPortalData = ($camstarPath + "InSite Administration\Portal Data")

        Copy-Item $newPortalData -Destination $originalPortalData -Force -Recurse
    }
}

#endregion

#region Update DB Tables

Function UpdateDBTables
{
	Write-LogInfo("`Update Database")

    $params = @("/dbupdate -ums -fds -csp -ci -cop -lco -ral")

    Set-Location ($camstarPath + "InSite Administration")

    Start-Process "CIMS.exe" -ArgumentList $params -Wait -windowstyle hidden
}

#endregion

#region Generate WCF Services

Function GenerateWCF
{
	Write-LogInfo("`Generate WCF Services")

    $params = @("/dbupdate -ums -gws -ssl -lco")

    Set-Location ($camstarPath + "InSite Administration")

    Start-Process "CIMS.exe" -ArgumentList $params -Wait -windowstyle hidden

}

#endregion

#region Update MD Links

function UpdateMDLinks([string[]]$workspace)
{

    foreach ($element in $workspace)
    {

        $selectedWorkspace = $element

        $fullPath = ($camstarPath + "Industry Solutions\" + $selectedWorkspace + "\Camstar Portal\CDOForms Settings\Settings.xml")
        $fullPathAppend = ($camstarPath + "Camstar Portal\User\Settings.xml")

        #Custom Settings

        $xdoc = New-Object System.Xml.XmlDocument
        $file = Resolve-Path($fullPath)
        $xdoc.Load($file)
        [xml] $xdoc = Get-Content $fullPath

        #Camstar Settings

        $xdoc2 = New-Object System.Xml.XmlDocument
        $file2 = Resolve-Path($fullPathAppend)
        $xdoc2.Load($file2)
        [xml] $xdoc2 = Get-Content $fullPathAppend

        #Move and replace text for web config and user setting for portal pages.

        $ReportHostsToAdd = $xdoc.PortalSettings.CDOFormsSettings.CDOForms.SelectNodes("CDOForm")   
        $docNodes = $xdoc2.PortalSettings.CDOFormsSettings.CDOForms.ChildNodes   

        foreach($ReportHost in $ReportHostsToAdd)
        {
                
            $i = 0
            $skip = 0
            $delta = 0
            $serviceNode = ""
            $pageNameNode = ""
            $service = ""
            $pageName = ""


            foreach ($attribute in $ReportHost.Attributes)
            {
                if($i -eq 0)
                {
                    $service = $attribute.Value
                }
                elseif($i -eq 1)
                {
                    $pageName = $attribute.Value
                }

                $i++
            }                    
                

            foreach ($xmlProperties in $docNodes)
            {
                if ($delta -eq 1) {
                    break
                }

                $x = 0

                foreach ($xmlProperty in $xmlProperties.Attributes) 
                {
                    if($x -eq 0)
                    {
                        $serviceNode = $xmlProperty.Value
                    }
                    elseif($x -eq 1)
                    {
                        $pageNameNode = $xmlProperty.Value

                        if (($serviceNode -eq $service) -and ($pageNameNode -eq $pageName))
                        {
                            $skip = 1
                            $delta = 1
                            break
                        }

                        if(($serviceNode -eq $service) -and ($pageNameNode -ne $pageName))
                        {
                            $xmlProperties.Attributes.RemoveAt(1);
                            $xmlProperties.SetAttribute("PageName", $pageName)

                            $xdoc2.Save($fullPathAppend)
                            $skip = 1
                            $delta =1

                            break
                        }
                    }

                    $x++
                }
            }

            if($skip -ne 1)
            {
                $Node = $xdoc2.CreateNode("element", "CDOForm","")
                $Node.SetAttribute("Service",$service)
                $Node.SetAttribute("PageName", $pageName)
                $xdoc2.PortalSettings.CDOFormsSettings.CDOForms.AppendChild($Node)               
                $xdoc2.Save($fullPathAppend)
                    
                $xdoc2 = [xml] $xdoc2.OuterXml.Replace(" xmlns=`"`"", "")
                $xdoc2.Save($fullPathAppend)
            }

        }

        if ($xdoc.PortalSettings.StudioSettings.DefaultModelingPages)
        {
            $pagesToAdd = $xdoc.PortalSettings.StudioSettings.DefaultModelingPages.SelectNodes("Page")   
            $pageNodes = $xdoc2.PortalSettings.StudioSettings.DefaultModelingPages.ChildNodes   

            foreach($page in $pagesToAdd)
            {
                $skip = 0
                $pageName = ""


                $pageName = $page.GetAttribute("Name")

                foreach ($pg in $pageNodes)
                {
                    $name = $pg.GetAttribute("Name")
                
                    if ($name -eq $pageName)
                    {
                    Write-Host $name
                        $skip = 1
                        break
                    }
                }

                if($skip -ne 1)
                {
                    $Node = $xdoc2.CreateNode("element", "Page","")
                    $Node.SetAttribute("Name", $pageName)
                    $xdoc2.PortalSettings.StudioSettings.DefaultModelingPages.AppendChild($Node)               
                    $xdoc2.Save($fullPathAppend)
                    
                    $xdoc2 = [xml] $xdoc2.OuterXml.Replace(" xmlns=`"`"", "")
                    $xdoc2.Save($fullPathAppend)
                }

            }
        }
    }
}

#endregion

#region RolesAndMenuDef

Function Run-DBScripts([string[]]$workspaces)
{
    foreach($workspace in $workspaces)
    {
        if($dbtype -eq "Oracle"){
            $scripts = Get-ChildItem -Path ($camstarPath + "Industry Solutions\" + $workspace + "\Scripts\Oracle\*.or.sql")
        }
        if($dbtype -eq "SQLServer"){
            $scripts = Get-ChildItem -Path ($camstarPath + "Industry Solutions\" + $workspace + "\Scripts\Mssql\*.sql")
        }
        foreach ($script in $scripts){
			if ($isSRC -Or !($script.Name -like 'srcPopulatePortalMenuUpdateData.sql' -Or $script.Name -like 'srcPopulatePortalMenuUpdateData.or.sql')) {
				[string[]]$scriptPaths += $script.FullName
			}
        }
        $dbPowershell = ($camstarPath + "Industry Solutions\Run-DBScripts.ps1")
        & $dbPowershell -ODACHintPath:$ODACHintPath -scriptPaths:$scriptPaths
    }
}

#endregion


#region SRC
Function InstallSrc()
{
    Write-LogInfo -message "Installing SRC";

    $srcFolder = Join-Path -Path $camstarPath -ChildPath "Industry Solutions\SRC"
    $srcZip = Join-Path -Path $srcFolder -ChildPath "SrcInstaller.zip"
	if (!(Test-Path -Path $srcZip))
	{
		$srcFolder = Join-Path -Path $camstarPath -ChildPath "Industry Solutions"
		$srcZip = Join-Path -Path $srcFolder -ChildPath "SRCInstaller.zip"
	}

	$destFolder = Join-Path -Path $camstarPath -ChildPath "Industry Solutions\SRCInstall"
	
    $installerFile = Join-Path -Path $destFolder -ChildPath "install.exe"
   
    Expand-Archive -LiteralPath $srcZip -DestinationPath $destFolder -Force

    if(Test-Path -Path $installerFile)
    {
        Write-LogInfo -message "Starting the SRC installer located at '$installerFile'.";
        Start-Process -FilePath $installerFile
    }
    else {
        Write-LogError "The SRC installer zip file extraction failed. You may manually extract the file located in the '$installerFile' file."
    }
}
#endregion

#region OEE

Function OEE([string] $dbServer, [string] $dbPort, [string] $dbName, [string] $user, [string] $pswrd)
{
    if($dbtype -eq "Oracle")
    {
        try
        { 
            $fLoadedODAC = Get-ODAC -OracleHintPath:$ODACHintPath
            if ($fLoadedODAC -ne $true){
                $message = "ERROR: Could not load Oracle assembly."
                write-output $message
                Exit
            }
        }
        Catch
        {   
            $message = "ERROR: Could not load Oracle assembly."
            write-output $message
            Exit
        }

        try
	    {
		    $oracleConnectionString = "Data Source= (DESCRIPTION =(ADDRESS =(PROTOCOL = TCP)(HOST = " + $dbServer + ")(PORT = " + $dbPort + "))(CONNECT_DATA =(SERVICE_NAME = " + $dbName + ")));User Id=" + $user + ";Password=" + $pswrd + ";"
		    $oracleConnection = New-Object Oracle.ManagedDataAccess.Client.OracleConnection($oracleConnectionString)
            $path = $camstarPath + "Industry Solutions\Industry\Scripts\Oracle"
            $pwrshlPath = $camstarPath + "Industry Solutions\IndustrySolutionsInstaller_PwrShl_Script.ps1"

			if($oracleConnection.State -ne 'Open')
			{
				$oracleConnection.Open()
			}

			$message = "Executing Additional Command to check for OEE Jobs"
			write-output $message

            $myOracle = $oracleConnection.CreateCommand()
            $myOracle.CommandText = "DECLARE 
isoee_job_exists NUMBER;

BEGIN

/* DROP/CREATE ISOEE_JOB */

SELECT count(*) INTO isoee_job_exists
FROM sys.user_scheduler_jobs
WHERE job_name = 'ISOEE_JOB';
IF isoee_job_exists = 1 THEN NULL; ELSE DBMS_SCHEDULER.CREATE_JOB( job_name => 'ISOEE_JOB', job_type => 'PLSQL_BLOCK', job_action => 'DECLARE PRUNDATE TIMESTAMP; BEGIN PRUNDATE := TO_CHAR(SYSDate,''dd-Mon-yy HH:MI:SS PM''); ISOEERESOURCETIMEINSTATUS.ISOEEUPDATEEQMTSTATUSBYSHIFT( PRUNDATE => PRUNDATE); END;', start_date => SYSTIMESTAMP,  repeat_interval => 'freq=minutely; INTERVAL=5', end_date => NULL, comments => 'Runs ISOEEUPDATEEQMTSTATUSBYSHIFT', auto_drop => FALSE, enabled => TRUE); END IF;

END;"
            $myTest = $myOracle.ExecuteReader()

			$message = "Result of query: " + $myTest
			write-output $message
	    }
	    catch
	    {
			$ErrorMessage = $_.Exception.Message                
            Write-LogInfo("Error is: $ErrorMessage")
            $message = "ERROR: Could not run SQL scripts, Error is: $ErrorMessage"
            write-output $message
	    }
        finally
        {
			$message = "Close Oracle connection"
			write-output $message

            $oracleConnection.Close()
           
            $oracleConnection.Dispose()
        }

    }


    if($dbtype -eq "SQLServer")
    {
        try
	    {
            $connectionString = "Data Source=$dbServer;Initial Catalog=$dbName;User Id=$user;Password=$pswrd;"
            $sqlConn = new-object Data.SqlClient.SqlConnection($connectionString)
            $path = $camstarPath + "Industry Solutions\Industry\Scripts\Mssql"

            if($sqlConn.State -ne 'Open')
			{
				$sqlConn.Open()
			}

            $JobName = $dbName + '.isOEEUpdateEquipmentStatusByShift_Job'
            $JobStepName = $dbName + '.isOEEUpdateEquipmentStatusByShift_Step_1'
            $JobScheduleName = $dbName + '.isOEEUpdateEquipmentStatusByShift_Schedule'            

            $sql = $sqlConn.CreateCommand()
            $sql.CommandText = "USE [msdb] IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'" + $JobName + "')
             EXEC msdb.dbo.sp_delete_job @job_name = N'" + $JobName + "', @delete_unused_schedule=1 BEGIN TRANSACTION DECLARE @ReturnCode INT SELECT @ReturnCode = 0 
             IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1) BEGIN EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', 
             @type=N'LOCAL', @name=N'[Uncategorized (Local)]' IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback END DECLARE @jobId BINARY(16) 
             EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'" + $JobName + "', @enabled=1, 
             @notify_level_eventlog=2, @notify_level_email=0, @notify_level_netsend=0, @notify_level_page=0, @delete_level=0, 
             @description=N'OEE Update Equipment refresh job.', @category_name=N'[Uncategorized (Local)]',
              @owner_login_name=N'" + $user + "', @job_id = @jobId OUTPUT IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback 
              EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'" + $JobStepName + "',
               @step_id=1, @cmdexec_success_code=0, @on_success_action=1, @on_success_step_id=0, @on_fail_action=2, @on_fail_step_id=0,
                @retry_attempts=0, @retry_interval=0, @os_run_priority=0, @subsystem=N'TSQL', @command=N'Declare @pRunDate datetime

Set @pRunDate = GetDate()

Exec isOEEUpdateEquipmentStatusByShift @pRunDate', @database_name=N'" + $dbName + "', @flags=0 IF (@@ERROR <> 0 OR @ReturnCode <> 0) 
GOTO QuitWithRollback EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1 
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback 
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'" + $JobScheduleName + "',
 @enabled=1, @freq_type=4, @freq_interval=1, @freq_subday_type=4, @freq_subday_interval=1, @freq_relative_interval=0, 
 @freq_recurrence_factor=0, @active_start_date=20161108, @active_end_date=99991231, @active_start_time=0, @active_end_time=235959 
 IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback 
 EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId,
  @server_name = N'(local)' IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback 
  COMMIT TRANSACTION 
  GOTO EndSave QuitWithRollback: IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION EndSave:";
            $myRun = $sql.ExecuteReader()
	    }
	    catch
	    {
			$ErrorMessage = $_.Exception.Message                
            Write-LogInfo("Error is: $ErrorMessage")
            $message = "ERROR: Could not run SQL scripts, Error is: $ErrorMessage"
            write-output $message
	    }
        finally
        {
            $sqlConn.Close()
           
            $sqlConn.Dispose()
        }
    }
}

#endregion

#region TestConnection

Function testConnection([string] $dbServer, [string] $dbPort, [string] $dbName, [string] $user, [string] $pswrd)
{
    if($dbtype -eq "Oracle")
    {
        try
        { 
            $fLoadedODAC = Get-ODAC -OracleHintPath:$ODACHintPath
            if ($fLoadedODAC -ne $true){
                $message = "ERROR: Could not load Oracle assembly."
                write-output $message
                Exit
            }
        }
        Catch
        {   
            $message = "ERROR: Could not load Oracle.ManagedDataAccess assembly. Oracle ODS must be installed."
            write-output $message
            Exit
        }

        try
	    {

		    $oracleConnectionString = "Data Source= (DESCRIPTION =(ADDRESS =(PROTOCOL = TCP)(HOST = " + $dbServer + ")(PORT = " + $dbPort + "))(CONNECT_DATA =(SERVICE_NAME = " + $dbName + ")));User Id=" + $user + ";Password=" + $pswrd + ";"
		    $oracleConnection = New-Object Oracle.ManagedDataAccess.Client.OracleConnection($oracleConnectionString)
		    $oracleConnection.Open()
	    }
	    catch
	    {
            $message = ("ERROR: Failed connection for the Oracle account " + $user +  " on Host " + $dbServer + ".")
            write-output $message
            Exit
	    }
        finally
        {
            if($oracleConnection.State -eq 'Open')
            {
                $oracleConnection.Close()
           
                $oracleConnection.Dispose()
            }
        }
    }

    if($dbtype -eq "SQLServer")
    {
        try
	    {
            $connectionString = "Data Source=$dbServer;Initial Catalog=$dbName;User Id=$user;Password=$pswrd;"
            $sqlConn = new-object Data.SqlClient.SqlConnection($connectionString)
            $sqlConn.Open()
	    }
	    catch
	    {
            $message = "ERROR: Failed connection for the SQLServer account '$user' on Host '$dbServer'."
            write-output $message
            Exit
	    }
        finally
        {
            if($sqlConn.State -eq 'Open')
            {
                $sqlConn.Close()
           
                $sqlConn.Dispose()
            }
        }
    }
}
        

#endregion

#region Cleanup

Function Cleanup([string[]]$workspace)
{
    foreach($element in $workspace)
    {
        $selectedWorkspace = $element
        $removePath = ($camstarPath + "InSite Administration\" + $selectedWorkspace)
        if(Test-Path -Path $removePath){
            Remove-Item -Path $removePath -Recurse
        }
    }
}

Function New-Folder([string]$path) {
    if(!(Test-Path -Path $path)){
        New-Item -Path $path -ItemType Directory
    }
}

#endregion

#region CTT
Function WriteErrorLog 
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $message,
        [Parameter(Mandatory = $false)]
        [ValidateSet("INFO","WARN","ERROR")]
        [string] $level = "INFO"
    )

    # Create timestamp
    $timestamp = (Get-Date).toString("yyyy/MM/dd HH:mm:ss")

    # Append content to log file
    Add-Content -Path $logFile -Encoding UTF8 -Value "$timestamp [$level] - $message"
}

Function RunCTTScripts()
{
	Write-LogInfo("OP EX CR User : $OPEXUserName")
	Write-LogInfo("OP CN MOM User: $CNMOMUserName")
	Write-LogInfo("OP CN MOM Gateway Host: $CNMOMGatewayHostHame")
	Write-LogInfo("OP CN MOM Gateway: $CNMOMGatewayName")
	Write-LogInfo("OP CN MOM HostNames: $CNMOMHostNames")
	Write-LogInfo("OP CN MOM Server: $CNMOMServer")
	
    #Opcenter Credentials
    $User = $OPEXUserName
    $PWord = ConvertTo-SecureString -String $OPEXPassword -AsPlainText -Force
    $script:OpcenterCredential = [PSCredential]::new($User, $PWord)

    RunCTTScript "OPEXCR_MaterialRequest" $true

    $User = $CNMOMUserName
    $PWord = ConvertTo-SecureString -String $CNMOMPassword -AsPlainText -Force
    $script:CNMOMCredential = [PSCredential]::new($User, $PWord)

    RunCTTScript "OPEXCON_MaterialRequest" $false
}

Function RunCTTScript([string] $fileName, [bool] $isOPEXCR)
{
        $CTTScriptPath = ($scriptPath + "\Industry\Scripts\Config\" + $fileName + ".cfg") 
		$RESULTS_TEMP = ($scriptPath + "\" + $fileName + "Temp.csv")
        $RESULTS = ($scriptPath + "\" + $fileName + ".csv")

        if(-not($PortNumber))
        {
            $Port = "443"
        }
        else
        {
            $Port = $PortNumber
        }

        if(-not($ServerName))
        {
            $HostName = "localhost"
        }
        else
        {
            $HostName = $ServerName
        }

        try
        {
             if (-not($isOPEXCR))
             {
				 Write-LogInfo("Update CN MOM CTT $fileName with Host $CNMOMServer and Port $Port")
				 UpdateCNMOMCTTScriptParameters -ScriptPath $CTTScriptPath -OpcenterCredential $script:OpcenterCredential -OpcenterHost $HostName -OpcenterPort $Port -CNMOMCredential $script:CNMOMCredential -ClientGatewayName $CNMOMGatewayName -HostNames $CNMOMHostNames 
				 
             } else
			 {
				 Write-LogInfo("Update EX CR CTT $fileName with Host $HostName and Port $Port")
				 UpdateEXCRCTTScriptParameters -ScriptPath $CTTScriptPath -OpcenterCredential $script:OpcenterCredential -OpcenterHost $HostName -OpcenterPort $Port
			 }
        
             try
             {
                  Write-LogInfo("Execute CTT: $CTTScriptPath")
                  ExecuteCTT -ScriptPath $CTTScriptPath -Result $RESULTS_TEMP -MID "" -CID "" -TID "" -RUNS "1"

                   FormatCTTOutput -TempFileName $RESULTS_TEMP -OutputFileName $RESULTS
                   $FailedScenarios = @( Import-Csv $RESULTS | Where-Object { $_.Status -eq "Failed" -and $_.TransactionName -ne "Breakpoint" } )

                    if ($FailedScenarios.count -gt 0) 
                    {
                        WriteErrorLog -level ERROR -message "'$($RESULTS)' contains $($FailedScenarios.count) failures"
                    }
             }
             Catch
             {
                  WriteErrorLog -level ERROR -message "Execute CTT Error : '$($_.Exception.Message)'"
             }
        }
        Catch
        {
             WriteErrorLog -level ERROR -message "Update CTT Error : '$($_.Exception.Message)'"
        }  
        Finally
        {
            Try
            {
                #Write-LogInfo("ResetCTT") 
                #ResetCTTScriptParameters -ScriptPath $CTTScriptPath 
            }
            Catch
            {
                 WriteErrorLog -level ERROR -message "Reset CTT Error : '$($_.Exception.Message)'"
            } 
        }    
} 

Function FormatCTTOutput 
{
    Param
    (
        [Parameter(Mandatory = $true)]
        [string]$TempFileName,
        [Parameter(Mandatory = $true)]
        [string]$OutputFileName
    )
    Set-Content $OutputFileName -Value "TransactionType, ScenarioName ,TransactionName, Iteration, Status, StatusContext, Executiontime, Starttime, Endtime"
    Add-Content -Path $OutputFileName -Encoding UTF8 -Value (Get-Content -Encoding UTF8 $TempFileName)
    Remove-Item -Path $TempFileName -Force
}

Function UpdateEXCRCTTScriptParameters {

    Param
	(
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath,
        [Parameter(Mandatory = $true)]
        [PSCredential]$OpcenterCredential,
        [Parameter(Mandatory = $true)]
        [string]$OpcenterHost,
        [Parameter(Mandatory = $true)]
        [string]$OpcenterPort
		
    )

    if (-Not (Test-Path $ScriptPath)) 
    {
        throw "Cannot find specified script path $ScriptPath"
    }
	else
	{
		try 
		{

			Write-LogInfo('Updating CTT {0}' -f $ScriptPath)

			$xml = [xml](Get-Content $ScriptPath)
			$nodes = $xml.CamstarLoadTester
			$nodes.Username = ($OpcenterCredential.GetNetworkCredential()).UserName
			$nodes.Password = ($OpcenterCredential.GetNetworkCredential()).Password
			$nodes.Host = $OpcenterHost
			$nodes.Port = $OpcenterPort

			
			$xml.Save($ScriptPath)

			Write-LogInfo('Updated CTT sucessfully {0}' -f $ScriptPath)
		}
		catch 
		{
			
             Write-LogInfo('Fail to Update CTT {0}' -f $ScriptPath)
             throw $_.Exception.Message
		}
	}
}

Function UpdateCNMOMCTTScriptParameters {

    Param
	(
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath,
        [Parameter(Mandatory = $true)]
        [PSCredential]$OpcenterCredential,
        [Parameter(Mandatory = $true)]
        [string]$OpcenterHost,
        [Parameter(Mandatory = $true)]
        [string]$OpcenterPort,
		[Parameter(Mandatory = $true)]
        [PSCredential]$CNMOMCredential,
		[Parameter(Mandatory = $true)]
        [string]$ClientGatewayName,
		[Parameter(Mandatory = $true)]
        [string]$HostNames
    )

    if (-Not (Test-Path $ScriptPath)) 
    {
        throw (New-ExceptionAsString -Message "Cannot find specified script path $ScriptPath")
    }
	else
	{
		try 
		{

			Write-LogInfo('Updating CTT {0}' -f $ScriptPath)

			$xml = [xml](Get-Content $ScriptPath)
			$nodes = $xml.CamstarLoadTester
			$nodes.Username = ($CNMOMCredential.GetNetworkCredential()).UserName
			$nodes.Password = ($CNMOMCredential.GetNetworkCredential()).Password
			$nodes.Host = $CNMOMServer
			$nodes.Port = $OpcenterPort

			$GlobalParameter_CNMOMUsername = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'UserName' }
			$GlobalParameter_CNMOMUsername.SubstitutionValue = ($OpcenterCredential.GetNetworkCredential()).UserName

			$GlobalParameter_CNMOMPassword = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'Password' }
			$GlobalParameter_CNMOMPassword.SubstitutionValue = ($OpcenterCredential.GetNetworkCredential()).Password

			$GlobalParameter_MIOClientGatewayName = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOClientGatewayName' }
			$GlobalParameter_MIOClientGatewayName.SubstitutionValue = $ClientGatewayName

			$GlobalParameter_MIOClientGatewayHostName = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOClientGatewayHostName' }
			$GlobalParameter_MIOClientGatewayHostName.SubstitutionValue = $OpcenterHost

			$GlobalParameter_MIOHostNames = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOHostNames' }
			$GlobalParameter_MIOHostNames.SubstitutionValue = $HostNames
			$xml.Save($ScriptPath)

			Write-LogInfo('Updated CTT sucessfully {0}' -f $ScriptPath)
		}
		catch 
		{
			
             Write-LogInfo('Fail to Update CTT {0}' -f $ScriptPath)
             throw $_.Exception.Message
		}
	}
}

Function ResetCTTScriptParameters 
{
    Param
	(
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath
    )

    if (-Not (Test-Path $ScriptPath)) 
    {
       throw "Cannot find specified script path $ScriptPath"
    }

    try 
	{
		Write-LogInfo('Resetting CTT parameters {0}' -f $ScriptPath)

        $xml = [xml](Get-Content $ScriptPath)
        $nodes = $xml.CamstarLoadTester
        $nodes.Username = ""
        $nodes.Password = ""
        $nodes.Host = ""
        $nodes.Port = ""

		$GlobalParameter_CNMOMUsername = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'UserName' }
        $GlobalParameter_CNMOMUsername.SubstitutionValue = ""

		$GlobalParameter_CNMOMPassword = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'Password' }
        $GlobalParameter_CNMOMPassword.SubstitutionValue = ""

		$GlobalParameter_MIOClientGatewayName = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOClientGatewayName' }
        $GlobalParameter_MIOClientGatewayName.SubstitutionValue = ""

		$GlobalParameter_MIOClientGatewayHostName = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOClientGatewayHostName' }
        $GlobalParameter_MIOClientGatewayHostName.SubstitutionValue = ""

		$GlobalParameter_MIOHostNames = $nodes.GlobalParameters.GlobalParameter | Where-Object { $_.Name -eq 'MIOHostNames' }
        $GlobalParameter_MIOHostNames.SubstitutionValue = ""
        $xml.Save($ScriptPath)

		Write-LogInfo('Succesfully Reset CTT parameters {0}' -f $ScriptPath)
    }
    catch 
    {
         Write-LogInfo('Fail to Reset CTT parameters {0}' -f $ScriptPath)
         throw $_.Exception.Message
    }
}

Function ExecuteCTT 
{
    param
	(
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath,
        [Parameter(Mandatory = $true)]
        [string]$Result,
        [Parameter(Mandatory = $false)]
        [string]$MID,
        [Parameter(Mandatory = $false)]
        [string]$CID,
        [Parameter(Mandatory = $false)]
        [string]$TID,
        [Parameter(Mandatory = $false)]
        [int]$RUNS = 1,
        [Parameter(Mandatory = $false)]
        [int]$ExecutionTimeoutInSeconds = 3600 
    )

    $TransactionTesterFolder = "C:\Program Files\Opcenter Execution Core\Transaction Tester\"

	if (Test-Path -Path $TransactionTesterFolder) 
	{
        Write-LogInfo('Transaction Tester Folder exist ' -f $TransactionTesterFolder)

		Try 
		{
			Push-Location $TransactionTesterFolder

			$Argument = ('CFG="{0}" MID="{1}" CID="{2}" TID="{3}" RUNS={4} RESULTFILE="{5}" CLOSEONCOMPLETE="true"' -f $ScriptPath, $MID, $CID, $TID, $RUNS, $Result)

			Write-LogInfo('Start Running CTT Script {0}' -f $ScriptPath)

			Start-Process CamstarTransactionTesterConsole.exe -ArgumentList $Argument -PassThru |
			Wait-Process -Timeout $ExecutionTimeoutInSeconds -ErrorVariable timedOut -ErrorAction Stop

			Write-LogInfo('CTT Script {0} run succesfully' -f $ScriptPath)
		}
		Catch 
        {
           Write-LogInfo('Fail to run Script {0}' -f $ScriptPath)
           throw $_.Exception.Message
        }
        Finally 
        {
            Pop-Location
        }
	}
}

#endregion

######################################
#####        Execution           #####
#####          Order             #####
######################################

Main
# SIG # Begin signature block
# MIIpeQYJKoZIhvcNAQcCoIIpajCCKWYCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCALb/grnec4xX8L
# X7DzOGRJ5Ai1NP70kIfviUS+JFEKUqCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
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
# LwYJKoZIhvcNAQkEMSIEIHchoXZuI08SHaGVrTQj4Ym7eOUc/6YlRyugS9m9oCm/
# MA0GCSqGSIb3DQEBAQUABIICABtWcxBX/YoRathf//9YAGlZThbRMQ7i+ooLu+ke
# YzM5WnOLK4osk261f/RXKky4JET4zWlnEOU2yYeD1uXSMB3QM1BQje/zskJzpGZP
# FgMBEglLYtnTJYkpBqLwOpzTZz7aMqUBzNf4nNPwa+lMJTY/RRnLJq+ZIeaLsDfi
# BCKbZ45LOF0NdjQgsFQht2NIWnChLjeJJ1vbWuRZ2r8/9iUOKF8WHuWyBJ7l/BdR
# rSYQDzH880VZK7/5f7JNf1ZqMFHKmN3q2XFbi68vUuvPPX5vZcGPlhoGh55UJGz+
# QyZYXVOlDOlr/FX/CjB21lFmXS9Omcgn2m5W1P9DEhpJt4hjhGQJ17gbxfpyOcF6
# hyOms/XEgYJcBH2AOQAROdn5Et3O1FL16UeDB7fz86MAy8oAY2IjJDJRZGsIgsa2
# hX+OPf0Drq2Qzt6PDg3vQ4t4xZBuCFt/uOl0cCHetmFKQiTnSbF59yokAMtD/8cS
# qnKnfQHuvS5PwCG8+xHx2OQdzHFICeNbbmCtNU9/A+DH9X9zpPX0zKkKxep64v6Q
# mWi88wIiSrtPQKrCFPJ7RIhq2xoKjVnqqh8OKULd9fM0I29qDQG11hkAQ0Xb8siZ
# dqXsQqM/SCEwiYoNIicIjjcYbOGmkRRBBuXShfo76PuER1e9nHoVlEL/qwW/C3zR
# wT4soYIXdjCCF3IGCisGAQQBgjcDAwExghdiMIIXXgYJKoZIhvcNAQcCoIIXTzCC
# F0sCAQMxDzANBglghkgBZQMEAgEFADB3BgsqhkiG9w0BCRABBKBoBGYwZAIBAQYJ
# YIZIAYb9bAcBMDEwDQYJYIZIAWUDBAIBBQAEIIiZvBx2kS/0qXkOVhyz6SsqOqTC
# WwrRvhpwYZvbauHlAhBVDhE6B84U6ZmCLADpRgKGGA8yMDI1MTIyOTE4MjI1N1qg
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
# hvcNAQkDMQ0GCyqGSIb3DQEJEAEEMBwGCSqGSIb3DQEJBTEPFw0yNTEyMjkxODIy
# NTdaMCsGCyqGSIb3DQEJEAIMMRwwGjAYMBYEFN1iMKyGCi0wa9o4sWh5UjAH+0F+
# MC8GCSqGSIb3DQEJBDEiBCBokdx5i3NxoLquPXyhBQ91a4gGR0A5ceRDUm+Dx3WA
# /jA3BgsqhkiG9w0BCRACLzEoMCYwJDAiBCBKoD+iLNdchMVck4+CjmdrnK7Ksz/j
# bSaaozTxRhEKMzANBgkqhkiG9w0BAQEFAASCAgCIzkQipu5/CnWPTFylXxIllacV
# CD8riiMqRDJKmFia2L4u0D3VRb5e4kfzSyZbiba6KGm2ZZO6hV8Tg0aflSeklH5c
# KfwR34iDu7NbshuCzQTdU/eatELIENgtdmsnSC29CiwxCJMr1JerA1szERKeyQDL
# wfwkK6hUJBtx3B0kmQnzo1GdzvP37VBpJ3YLpcKqVwif72aK1jBPvLpJBcsYo0U2
# pOCGEp0YbrC8sxlOkkvJgrC41eARR3UpTKZUbHtEELSJs6wg8MxcfjEVdoPfStOi
# GM/a3a/0umkx2jgZWIjZNHiikiXCCyhX2qyG9PAhghgE8ficwkNtyEiP/XnL8HNh
# 5aeLS5bLpOoY3Rkj9K8loII7AetV8hzNc8EtuGhpKmZtj8BIY9Oat6xzYIbmC2ME
# nYar9bDesIg5VszfwfW8cTC0VaBjtEneprlZuP6Jp0RTfSaeZXtuZDcqsTs67bAS
# VDVdmQEQT5HQ1iOr6So6yO3Uew90EbFN4vFNfKIi/R+IglKWPCcsGuFPR5ZEe+VJ
# +dTHwh7xYqj9riqOftUTbPmTz9AjBRuZLviUuWCgMYDKoqbyGxsB8gbEZ3mraSNn
# i5/d1S6Y/ngsLMyQSDlur4MYWa+pXO2+ueNnAfqbJKUgyO3yM+FCdMV7VYLp3Hl3
# gq6FmC3FZODNabqKkA==
# SIG # End signature block
