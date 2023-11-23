
. "./config.ps1"

Write-Host "*************************************************************"
Write-Host "*      Deleting existing files in the S3 target folder      *"
Write-Host "*************************************************************"

Remove-S3Object -BucketName ${bucket} -Key "${service}/${environment}" -Force

Write-Host " "
