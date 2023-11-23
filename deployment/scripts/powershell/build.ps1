
. "./config.ps1"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*               Building the container image                *"
Write-Host "*************************************************************"
Write-Host " "

Set-Location ${workspacePath}

(Get-ECRLoginCommand).Password | docker login `
    --username AWS `
    --password-stdin https://${account}.dkr.ecr.${region}.amazonaws.com

docker build --build-arg TOKEN=${token} -t "${serviceLower}-${environmentLower}" --pull -f Dockerfile .

docker tag `
    "${serviceLower}-${environmentLower}:latest" `
    "${account}.dkr.ecr.${region}.amazonaws.com/${serviceLower}-${environmentLower}:latest"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*            Pushing the image to the repository            *"
Write-Host "*************************************************************"
Write-Host " "

docker push "${account}.dkr.ecr.${region}.amazonaws.com/${serviceLower}-${environmentLower}:latest"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*       Packaging the authorization Lambda zip file         *"
Write-Host "*************************************************************"
Write-Host " "

dotnet lambda package `
    --framework net6.0 `
    -pl "${workspacePath}/src/ConnectedCar.Lambda" `
    -o "${env:TEMP}/Lambda-${version}.zip"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*              Uploading the Lambda zip file                *"
Write-Host "*************************************************************"
Write-Host " "

Write-S3Object -BucketName ${bucket} `
    -File "${env:TEMP}/Lambda-${version}.zip" `
    -Key "${service}/${environment}/Lambda-${version}.zip"

Write-Host " "
