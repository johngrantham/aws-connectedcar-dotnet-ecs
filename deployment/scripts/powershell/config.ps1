
Import-Module AWSPowerShell.NetCore

$ErrorActionPreference = "Stop"

$workspacePath="[enter base path]/aws-connectedcar-dotnet-container"
$bucket="[enter bucket name]"
$service="ConnectedCar"
$serviceLower="connectedcar"
$environment="Dev"
$environmentLower="dev"
$version="20220801"
$stage="api"

$number=(Get-Date -UFormat "%H%M%S")
$domain="connectedcar${number}"

$account=(Get-STSCallerIdentity).Account
$region="[enter region code]"

$cpu="X86_64"

token="[enter token value]"

Write-Host "*** Validating the config.ps1 variables ***"
Write-Host " "

if ("${account}" -eq "") {
    Write-Host "Error: account is not valid"
    return
}

if ("${region}" -eq "") {
    Write-Host "Error: region is not valid"
    return
}

if (!(Test-S3Bucket -BucketName ${bucket})) {
    Write-Host "Error: bucket is not valid"
    return
}

if (!(Test-Path -path ${workspacePath})) {
    Write-Host "Error: workspacePath is not valid"
    return
}  
