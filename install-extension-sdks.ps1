# paths
$adSdkUrl = "https://dvlup.blob.core.windows.net/general-app-files/MSIs/MicrosoftAdvertisingSDK.msi"
$servicesSdkUrl = "https://dvlup.blob.core.windows.net/general-app-files/MSIs/MicrosoftStoreServicesSDK.msi"

$adSdkPath = Join-Path $env:TEMP "MicrosoftAdvertisingSDK.msi"
$servicesSdkPath = Join-Path $env:TEMP "MicrosoftStoreServicesSDK.msi"

# Downloads the two MSI files to the environment temp folder

try
{
    Write-Output "downloading $adSdkUrl..."
    Invoke-WebRequest -Uri $adSdkUrl  -OutFile $adSDKPath
}
catch
{
    Write-Error "Failed to download $adSdkUrl"
}

try
{
    Write-Output "downloading $servicesSdkUrl..."
    Invoke-WebRequest -Uri $servicesSdkUrl -OutFile $servicesSdkPath
}
catch
{
    Write-Error "Failed to download $servicesSdkPath"
}

# Runs the downloaded MSI installers and waits for each to install

try
{
	Write-Output "installing $adSdkUrl..."
	Start-Process -Wait $adSdkPath
}
catch
{
    Write-Error "Failed to install $adSdkPath"
}

try
{
    Write-Output "installing $servicesSdkPath..."
	Start-Process -Wait $servicesSdkPath
}
catch
{
    Write-Error "Failed to install $servicesSdkPath"
}