# Self-elevate the script if required
if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {
    if ([int](Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty BuildNumber) -ge 6000) {
        $CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
        Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine
        Exit
    }
}

# Find Camstar
$camstarPath = $ENV:CamstarInstall
$SSODirPath = Join-Path $camstarPath "SSO"

# Update username, password in web.config and set sso to true
Write-Host "updating web.config..."
Write-Host
$username = Read-Host -Prompt "Please enter the value for the Opcenter admin username. This is the user that has all administrative rights"
$username = $username.Trim()
Write-Host

# Check if passwords match
do {
    $password = Read-Host -Prompt "Please enter password for '$username' User" -AsSecureString
	Write-Host
	$confirmPassword = Read-Host -Prompt "Retype password for confirmation" -AsSecureString
	Write-Host

	$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
	$plainConfirmPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirmPassword))

    if ($plainPassword.Equals($plainConfirmPassword)) {
        Write-Host "Passwords match."
		Write-Host
    }
    else {
        Write-Host "Passwords do not match. Please try again."
		Write-Host
        Start-Sleep -Seconds 1
    }
} until ($plainPassword.Equals($plainConfirmPassword))

# Encrypting password
$CamstarDll = "$camstarPath\InSite Administration\Camstar.Util.dll"
Add-Type -Path $CamstarDll
$encryptedPassword = [Camstar.Util.CryptUtil]::Encrypt($plainPassword)

$configFile = "$camstarPath\Camstar Portal\web.config"
$config = [xml](Get-Content -Path $configFile)
$appSettingsElement = $config.configuration.appSettings
$usernameElement = $appSettingsElement.add | Where-Object { $_.key -eq "username" }
$usernameElement.Attributes["value"].Value = $username
$passwordElement = $appSettingsElement.add | Where-Object { $_.key -eq "password" }
$passwordElement.Attributes["value"].Value = $encryptedPassword
$authFlags = $appSettingsElement.add | Where-Object { $_.key -eq "authFlags" }
$authFlags.value = '{"authType":"SSO","nameSource":"SANUpn"}'
$config.Save($configFile)

# Login Type - SAM/SAML
Write-Host
$LoginType = Read-Host -Prompt "Enter the login method (SAM/SAML)"
$LoginType = $LoginType.Trim()
Write-Host
# Check if LoginType is correct
$userInput = $null
do {
	$userInput = Read-Host "Is the login type '$LoginType' correct?(Y/N)"
	Write-Host
	if ($userInput -eq 'n') {
		$LoginType = Read-Host -Prompt "Enter the correct login type (SAM/SAML)"
		$LoginType = $LoginType.Trim()
		$LoginType = $LoginType.ToUpper()
		Write-Host
	}
} until ($userInput -eq 'y')

# Read SSOConfig or generate new
$filePath = "$camstarPath\InSite Administration\SSOConfig.json"
if (Test-Path -Path $filePath) {
        $jsonContent = Get-Content -Path $filePath -Raw | ConvertFrom-Json
}
else {
        $jsonContent = [PSCustomObject]@{
            AvailableServers = @()
            SelectedServer = ""
            AppServers = @()
        }
    $jsonContent | ConvertTo-Json -Depth 5 | Set-Content -Path $filePath
}

$server = [System.Net.Dns]::GetHostName()+"."+(Get-WmiObject win32_computersystem).Domain

# Check if server name is correct
Write-Host
$userInput = $null
do {
    $userInput = Read-Host "Is the server name (Server where the Camstar proxy and other resources are installed) '$server' correct? (Y/N)"
	Write-Host
    if ($userInput -eq 'n') {
        $server = Read-Host -Prompt "Enter the correct server name"
		$server = $server.Trim()
		Write-Host
    }
} until ($userInput -eq 'y')

# Callback URL
	$CallbackURL = "https://${server}/camstarportal/Default.aspx?mode=classic&theme=Horizon"
	Write-Host "The Callback URL is set by default to - '$CallbackURL'"
	Write-Host
	# Check if Callback URL is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the Callback URL '$CallbackURL' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$CallbackURL = Read-Host -Prompt "Enter the correct URL"
			$CallbackURL = $CallbackURL.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')

if($LoginType -eq "SAM"){
	
	$IsPKCEEnabled = $false
	# Check for PKCE
	do {
		$pkceInput = Read-Host "Do you want to enable PKCE? (y/n)"
		$pkceInput = $pkceInput.ToLower()
	} while ($pkceInput -ne 'y' -and $pkceInput -ne 'n')

	$IsPKCEEnabled = $pkceInput -eq 'y'

	# Clientid
	Write-Host
	$ClientId = Read-Host -Prompt "Enter ClientId"
	$ClientId = $ClientId.Trim()
	Write-Host
	# Check if ClientSecret is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the ClientId '$ClientId' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$ClientId = Read-Host -Prompt "Enter the correct ClientId"
			$ClientId = $ClientId.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')

	if ($IsPKCEEnabled) {
		# PKCE enabled - Client Secret is optional
		do {
			$secretInput = Read-Host "Do you want to enter a Client Secret?(Y/N)"
			$secretInput = $secretInput.ToLower()
		} while ($secretInput -ne 'y' -and $secretInput -ne 'n')
		
		if ($secretInput -eq 'y') {
			Write-Host
			$ClientSecret = Read-Host -Prompt "Enter ClientSecret"
			$ClientSecret = $ClientSecret.Trim()
			Write-Host
			# Check if ClientSecret is correct
			$userInput = $null
			do {
				$userInput = Read-Host "Is the ClientSecret '$ClientSecret' correct?(Y/N)"
				Write-Host
				if ($userInput -eq 'n') {
					$ClientSecret = Read-Host -Prompt "Enter the correct ClientSecret"
					$ClientSecret = $ClientSecret.Trim()
					Write-Host
				}
			} until ($userInput -eq 'y')
		} else {
			Write-Host
			$ClientSecret = ""
		}
	} else {
		# PKCE disabled - Client Secret is mandatory
		Write-Host
		$ClientSecret = Read-Host -Prompt "Enter ClientSecret"
		$ClientSecret = $ClientSecret.Trim()
		Write-Host
		# Check if ClientSecret is correct
		$userInput = $null
		do {
			$userInput = Read-Host "Is the ClientSecret '$ClientSecret' correct?(Y/N)"
			Write-Host
			if ($userInput -eq 'n') {
				$ClientSecret = Read-Host -Prompt "Enter the correct ClientSecret"
				$ClientSecret = $ClientSecret.Trim()
				Write-Host
			}
		} until ($userInput -eq 'y')
	}

	# AuthHandlder URL
	$AuthHandlerURL = Read-Host -Prompt "Enter Auth Handler URL"
	$AuthHandlerURL = $AuthHandlerURL.Trim()
	Write-Host
	# Check if AuthHandler URL is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the AuthHandler URL '$AuthHandlerURL' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$AuthHandlerURL = Read-Host -Prompt "Enter the correct URL"
			$AuthHandlerURL = $AuthHandlerURL.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')
	
	# PostLogOutURL
	$PostLogOutURL = Read-Host -Prompt "Enter PostLogOut URL"
	$PostLogOutURL = $PostLogOutURL.Trim()
	Write-Host
	# Check if PostLogOutURL URL is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the PostLogOut URL '$PostLogOutURL' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$PostLogOutURL = Read-Host -Prompt "Enter the correct URL"
			$PostLogOutURL = $PostLogOutURL.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')
	
	$urlProtocol = "https"

	# Update the JSON content
	$jsonContent.AvailableServers = @($server)
	$jsonContent.SelectedServer = $server

	$appServerObject = [PSCustomObject]@{
		ServerName = $server
		LoginMethod = "SAM"
		ProtocolOption = $urlProtocol
		SAMConfig = [PSCustomObject]@{
			IsPKCEEnabled = $IsPKCEEnabled
			ClientId = $ClientId
			CallBackURL = $CallbackURL
			ClientSecret = $ClientSecret
			AuthHandler = $AuthHandlerURL
			PostLogOutURL = $PostLogOutURL
			RedirectURL = "https://${server}/IDPAgent/api/v1/sso-service-provider/sam/provider/callback"
		}
		DefaultConfiguration = [PSCustomObject]@{
			ServerURL = ""
		}
		SamlServiceProvider = [PSCustomObject]@{
			SamlExtIdpPublicKey = ""
			SamlEndpoint = ""
			SamlEntityID = ""
			SamlAssertUrl = ""
			SamlStartSLOUrl = ""
			SamlLogoutRequestUrl = ""
			SamlLogoutResponseUrl = ""
			SamlCallBackUrl = ""
			SamlPrivateKey = ""
			SamlPassword = ""
		}
	}
}
elseif ($LoginType -eq "SAML"){
	# Create the folder structure
	try {
		New-Item -ItemType Directory -Path "$SSODirPath\IDPAgent\app_data" -ErrorAction Stop
		New-Item -ItemType Directory -Path "$SSODirPath\IDPAgent\app_data\Certificates" -ErrorAction Stop
		New-Item -ItemType Directory -Path "$SSODirPath\IDPAgent\app_data\jwt" -ErrorAction Stop
	}
	catch {
		if ($_.Exception.Message -like "*already exists*") {
			Write-Host "The 'app_data' directory already exists. Skipping folder creation."
			Write-Host
		}
		else {
			Write-Host "An error occurred while creating the folder structure: $($_.Exception.Message)"
			Exit
		}
	}

	# Copy SAML Certificate
	Write-Host
	Write-Host
	$validPaths = $false
	while (-not $validPaths) {
		$certificatePath = Read-Host -Prompt "Enter directory location for the certificate(downloaded from the SAML application created)"
		$certificatePath = $certificatePath.Trim()
		
		# Check if source folder exists
		if (-not (Test-Path -Path $certificatePath)) {
			Write-Host "Error: Source directory does not exist: $certificatePath" -ForegroundColor Red
			Write-Host "Please try again with a valid directory path" -ForegroundColor Yellow
			continue
		}

		Write-Host
		$certificate = Read-Host -Prompt "Enter the name of the certificate(downloaded from the SAML application created). Enter full name with extension"
		$certificate = $certificate.Trim()
		$fullCertificatePath = "$certificatePath\$certificate"
		
		# Check if source certificate file exists
		if (-not (Test-Path -Path $fullCertificatePath -PathType Leaf)) {
			Write-Host "Error: Certificate file does not exist: $fullCertificatePath" -ForegroundColor Red
			Write-Host "Please try again with a valid certificate file" -ForegroundColor Yellow
			continue
		}

		$validPaths = $true
	}

	try {
		Copy-Item -Path $fullCertificatePath -Destination $SSODirPath\IDPAgent\app_data\Certificates -Force -ErrorAction Stop
	}
	catch {
		Write-Host "`nError copying certificate: $_" -ForegroundColor Red
		Write-Host "Please ensure you have sufficient permissions to write to $destinationFolder." -ForegroundColor Red
	}
	
	# Protocol(https/http)
	Write-Host
	$protocol = Read-Host "Which protocol needs to be used? (http or https)"
	$protocol = $protocol.Trim()
	# Check if protocol is correct
	Write-Host
	$userInput = $null
	do {
		$userInput = Read-Host "Is the protocol '$protocol' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$protocol = Read-Host -Prompt "Enter the correct protocol"
			Write-Host
		}
	} until ($userInput -eq 'y')
	$protocol = $protocol.Trim().ToUpper()
	$urlProtocol = $protocol.ToLower()

	# Password for pfx file
	$password = "localhost"
	$passwordBytes = [System.Text.Encoding]::UTF8.GetBytes($password)
	$passwordEncoded = [System.Convert]::ToBase64String($passwordBytes)
	
	Write-Host "The following URLs will be updated to login via SAML" -ForegroundColor Yellow
	Write-Host

	# Input for URLs
	Write-Host "Please make sure you are entering the details correctly"
	Write-Host
	Write-Host "EndpointURL" -ForegroundColor Yellow
	Write-Host "This is the endpoint where the service provider sends the login request to the identity provider. For example-if Azure AD is used,the Endpoint URL will be the Login URL provided by the IDP."
	Write-Host
	Write-Host "EntityID URL" -ForegroundColor Yellow
	Write-Host "This is the Unique identifier for the SAML entity created on the Azure AD portal. For example: https://opcenter-core-ex-azad.com"
	Write-Host
	Write-Host "Logout URL" -ForegroundColor Yellow
	Write-Host "This is the URL to the IDP logout url."
	Write-Host

	# SAML Endpoint URL
	Write-Host
	$SAMLEndpoint = Read-Host -Prompt "Enter Endpoint URL(Endpoint where the service provider sends the login request to the identity provider). Enter the entire URL"
	$SAMLEndpoint = $SAMLEndpoint.Trim()
	Write-Host
	# Check if Endpoint URL is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the SAMLEndpoint URL '$SAMLEndpoint' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$SAMLEndpoint = Read-Host -Prompt "Enter the correct URL"
			$SAMLEndpoint = $SAMLEndpoint.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')

	# SAML EntityID URL
	Write-Host
	$SamlEntityID = Read-Host -Prompt "Enter EntityID URL(Unique identifier for the SAML entity created on the Azure AD portal)"
	$SamlEntityID = $SamlEntityID.Trim()
	Write-Host
	# Check if SAML EntityID is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the SamlEntityID URL '$SamlEntityID' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$SamlEntityID = Read-Host -Prompt "Enter the correct URL"
			$SamlEntityID = $SamlEntityID.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')

	# SAML Logout URL
	Write-Host
	$SAMLLogoutURL = Read-Host -Prompt "Enter Logout URL(URL to the IDP logout url)"
	$SAMLLogoutURL = $SAMLLogoutURL.Trim()
	Write-Host
	# Check if SAML Logout URL is correct
	$userInput = $null
	do {
		$userInput = Read-Host "Is the SAMLLogoutURL URL '$SAMLLogoutURL' correct?(Y/N)"
		Write-Host
		if ($userInput -eq 'n') {
			$SAMLLogoutURL = Read-Host -Prompt "Enter the correct URL"
			$SAMLLogoutURL = $SAMLLogoutURL.Trim()
			Write-Host
		}
	} until ($userInput -eq 'y')

	# Update the JSON content
	$jsonContent.AvailableServers = @($server)
	$jsonContent.SelectedServer = $server

	$appServerObject = [PSCustomObject]@{
		ServerName = $server
		LoginMethod = "SAML"
		ProtocolOption = $protocol
		SAMConfig = [PSCustomObject]@{
			ClientId = ""
			CallBackURL = ""
			ClientSecret = ""
			AuthHandler = ""
			PostLogOutURL = ""
			RedirectURL = ""
		}
		DefaultConfiguration = [PSCustomObject]@{
			ServerURL = ""
		}
		SamlServiceProvider = [PSCustomObject]@{
			SamlExtIdpPublicKey = "\app_data\Certificates\$certificate"
			SamlEndpoint = $SAMLEndpoint
			SamlEntityID = $SamlEntityID
			SamlAssertUrl = "${urlProtocol}://${server}/IDPAgent/api/v1/sso-service-provider/saml-assert-consumer-service"
			SamlStartSLOUrl = $SAMLLogoutURL
			SamlLogoutRequestUrl = "${urlProtocol}://${server}/IDPAgent/api/v1/sso-service-provider/saml-logout-request"
			SamlLogoutResponseUrl = $null
			SamlCallBackUrl = $CallbackURL
			SamlPrivateKey = "\app_data\jwt\cert.pfx"
			SamlPassword = $passwordEncoded
		}
	}
}
$jsonContent.AppServers = @($appServerObject)

# Save the updated JSON content back to the file
$jsonContent | ConvertTo-Json -Depth 5 | Out-String | ForEach-Object { $_.Replace('\u0026', '&') } | Set-Content -Path $filePath 

# Perform iisreset
Write-Host
Write-Host "Performing 'iisreset'..."
iisreset

#Restart CIMS
$cimsProcesses = Get-Process -Name "CIMS*" -ErrorAction SilentlyContinue
if ($cimsProcesses) {
    foreach ($process in $cimsProcesses) {
        Write-Host "Closing CIMS process: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Cyan
        $process.CloseMainWindow()
        Start-Sleep -Seconds 2
        
        # If process still running, force close it
        if (!$process.HasExited) {
            Write-Host "Force closing process..." -ForegroundColor Red
            $process.Kill()
        }
    }
}

$cimsPath = "$camstarPath\InSite Administration\CIMS.exe"

if (Test-Path $cimsPath) {
    Start-Process -FilePath $cimsPath
}

Write-Host
Write-Host
Write-Host
Write-Host "Information:" -ForegroundColor Yellow

# Check hosting bundle version
Write-Host
$hostingBundleVersion = $env:WEBSITE_NODE_DEFAULT_VERSION
if ($hostingBundleVersion) {
	Write-Host "Hosting bundle version: $hostingBundleVersion"
} else {
	Write-Host "Hosting bundle version not found. Please check your hosting environment and install hosting bundle if not already installed."
}

Write-Host
Write-Host "Server URL to access application via Single Sign-On is: " ${urlProtocol}://${server}/CamstarPortal/Default.aspx

Write-Host
Write-Host "Important Note:" -ForegroundColor Yellow
Write-Host "Email Id of the user needs to be added in the portal in the Modelling > Employee page to login via SSO."
Write-Host

# Exit
Write-Host "Script completed. Press Enter to exit."
Read-Host
# SIG # Begin signature block
# MIIpeQYJKoZIhvcNAQcCoIIpajCCKWYCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDxK2GsjB9vbtfL
# JIrNZ5XWpnCjz2mhd/Y/fLuksVmFyaCCDi8wggawMIIEmKADAgECAhAIrUCyYNKc
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
# LwYJKoZIhvcNAQkEMSIEIG86Vl5ApatQE2HHfei3BKV3M5AHt+mtvsU5gpVYsbBf
# MA0GCSqGSIb3DQEBAQUABIICAHX/n/JKbj3CX+YLzTV7B3IIGzSzSCqvHI2xMqud
# 6ToTEwrL/5Wzr/jBsyYm+pyfTG44XYmlB4TqluTSBs+B6PyNQK5UpGxvljso1V5B
# VyidUTK4jVgXHWWx7YA3BbEnMTZdGUt2bJGSIFm9syShikAQ2ulSGMx2JzYIf2yG
# ktaMTNilNVdw/KnMMZP3B//svrvuyo82EK68MU3Eybq/yy483E1vQslP9sgWbVqn
# Od3imheoNkruPqroTadDphmukaY1pUtDgmCsAqsmpJCB5hb6s7O0Vjigcf8f7W2h
# XrvdPB4fHfMqTUaU5RSFjROFQFRHHt0z5Xkzc+WYF6SBS9QS8za8myYQ0zqeakqc
# bZF7kjO8L0X37QzvfIgg/nOzbA/u6NwmQcH9qBSl1/0IhAUDPDmZH6ILw1Aguakc
# bSAqHQLkDRIbDsrSYhpGxIW+lpUVRcESvJxfFslvMvllrF4IxvOoriEsea06oF6f
# K7GmlA/wJD59VNiG08MubmmtZfUeV6yWjUixswg8YX/PndXi0Ofj2NxzfOXWrvS7
# D1SrbdBwKryj+b3unHLrHh2xNUr2WshjXFsNEnUWLeybQ34w50LfPXWDeWwQEeer
# Epljmxaf/yzhY4Hx0WN8Dok2feEZW0sz8xyGARRpHcMWPgHp50LIt7kz4En4KeDC
# A4ESoYIXdjCCF3IGCisGAQQBgjcDAwExghdiMIIXXgYJKoZIhvcNAQcCoIIXTzCC
# F0sCAQMxDzANBglghkgBZQMEAgEFADB3BgsqhkiG9w0BCRABBKBoBGYwZAIBAQYJ
# YIZIAYb9bAcBMDEwDQYJYIZIAWUDBAIBBQAEILXVEhqIh1C4DG7JDUsnDmAtbiDm
# gNBRf7VU0Uz3c1WiAhAKNUqdAXW26BYOpjKnPAa0GA8yMDI1MTIyOTE1MzgzMlqg
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
# hvcNAQkDMQ0GCyqGSIb3DQEJEAEEMBwGCSqGSIb3DQEJBTEPFw0yNTEyMjkxNTM4
# MzJaMCsGCyqGSIb3DQEJEAIMMRwwGjAYMBYEFN1iMKyGCi0wa9o4sWh5UjAH+0F+
# MC8GCSqGSIb3DQEJBDEiBCCP9yUFWhbI8VU4c1Meu0zY2IHFQCdEndX72CjaHape
# 5TA3BgsqhkiG9w0BCRACLzEoMCYwJDAiBCBKoD+iLNdchMVck4+CjmdrnK7Ksz/j
# bSaaozTxRhEKMzANBgkqhkiG9w0BAQEFAASCAgDD29JPS/QWGv4Gp5xAYSsI/tBB
# iRS/YCSftiwuYyVy86bupTCHJumjfYSAh6pygw7QyzCxYnzb2dtD43mcws+T4yIE
# rF8IC8adBRj6QnGmzCGlzEbK0lR6K6zrSZo0OAvXnjriYkNNdvChcYpP6fsFI0bA
# yP8gF58puC+phXoP46L+dSj8oUUXtztdfmV4q+UjfNHlOEt+dzn+2YZFkq7xW9j1
# 1EVcL0cXk5FZz/McJZsv4b3Nq3v5lOn07R44q3Ro+pAtcuRLIMGLCMgN94OXsWRz
# O5eIMq73bcLk5gVZMCmrNCCroGtafHQRoztjJWAWf68c+RCt54S4oaJMSQz3MsCv
# riNefbkg7T7b3zA/IvSEJg8EQDVJOnkKSqPzzAsa6pkK6vomLqSkbIEaFJ5MWkfh
# VObSjAIYNsPc06XAceU5Iy5FeosCxc8Xmso4YUj2IB6sxeb8DRZEo5RAqVJt6aUr
# kpHBYYSLbpEt7Tj338l8z7J4qitm8i9fdH5MbkHf92bp9PHFGBsvuJsnF4fK1VxF
# n06B9faiLxLAAllTgbmPAYAFC7e0psHx56+4ygoyQez82p4cx7eeWqqQev3WPhLb
# YRKAIXrrDt9AEi0wejHqBJ+fR1eMuO13vxJxRCfnTJ5U/+H8tpMo2EmFezhqqRVb
# 8liBeZPAvGQUrWj8+g==
# SIG # End signature block
