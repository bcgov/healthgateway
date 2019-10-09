##environment and application are injected from the pipeline
$applicationPoolName = "hnc-$env:ENVIRONMENT"
$baseFolder = "E:\Applications\$env:APPLICATION"

#The variable below is based on the Azure DevOps drop artifact _HNClient 
$version = "$env:RELEASE_ARTIFACTS__HNCLIENT_BUILDNUMBER"
Write-Host "Starting deployment of release $version"

import-module WebAdministration
function Stop-AppPool ($webAppPoolName, [int]$secs) {
    $retvalue = $false
    $wsec = (get-date).AddSeconds($secs)
    Stop-WebAppPool -Name $webAppPoolName
    Write-Output "$(Get-Date) waiting up to $secs seconds for the WebAppPool '$webAppPoolName' to stop"
    $poolNotStopped = $true
    while (((get-date) -lt $wsec) -and $poolNotStopped) {
        $pstate = Get-WebAppPoolState -Name $webAppPoolName
        if ($pstate.Value -eq "Stopped") {
            Write-Output "$(Get-Date): WebAppPool '$webAppPoolName' is stopped"
            $poolNotStopped = $false
            $retvalue = $true
        }
    }
    return $retvalue
}

$releaseFolder = "$baseFolder\Release\$version"
Write-Host "Release will be deployed to $releaseFolder"

if (!(Test-Path $releaseFolder)) {
    Write-Host "Extracting $version.zip to release folder"
    Expand-Archive -Path _HNClient\drop\$version.zip -DestinationPath $releaseFolder
}
else {
    Write-Host "Release $version already exists on this server skipping extract"
}

Set-Location -Path $baseFolder\Environment\$applicationPoolName
$deployedFolder = Get-Item .\App | Select-Object -ExpandProperty Target
Write-Debug "Release Folder = $releaseFolder\"
Write-Debug "Deploy Folder = $deployedFolder"
if ($releaseFolder + "\" -ne $deployedFolder) {
    if ((Get-WebAppPoolState -Name $applicationPoolName).Value -ne 'Stopped') {
        Write-Host ('Stopping Application Pool: {0}' -f $applicationPoolName)
        $stopped = Stop-AppPool $applicationPoolName 30
        #Stop-WebAppPool -Name $applicationPoolName
    }
    if ($stopped) {
        Write-Host "Symlinking $version into environment $applicationPoolName"
        New-Item -Path .\App -ItemType SymbolicLink -Value ..\..\Release\$version\ -Force
    }
    else {
        Write-Error "IIS did not stop in a timely manner, skipping symlink will still try a restart..."
    }
    Write-Host ('Starting Application Pool: {0}' -f $applicationPoolName)
    Start-WebAppPool -Name $applicationPoolName
}
else
{
    Write-Host "Skipping deployment as release is already deployed in this environment."
}