
. "./config.ps1"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*               Uploading the OpenAPI files                 *"
Write-Host "*************************************************************"
Write-Host " "

$admin = Get-Content `
    "${workspacePath}/deployment/ecs/specifications/admin.openapi.yaml",`
    "${workspacePath}/deployment/ecs/specifications/schemas.openapi.yaml" `
    | Out-String

$vehicle = Get-Content `
    "${workspacePath}/deployment/ecs/specifications/vehicle.openapi.yaml",`
    "${workspacePath}/deployment/ecs/specifications/schemas.openapi.yaml" `
    | Out-String

$customer = Get-Content `
    "${workspacePath}/deployment/ecs/specifications/customer.openapi.yaml",`
    "${workspacePath}/deployment/ecs/specifications/schemas.openapi.yaml" `
    | Out-String

Write-S3Object `
    -BucketName ${bucket} `
    -Content $admin `
    -Key "${service}/${environment}/admin.openapi.yaml"

Write-S3Object `
    -BucketName ${bucket} `
    -Content $vehicle `
    -Key "${service}/${environment}/vehicle.openapi.yaml"

Write-S3Object -BucketName ${bucket} `
    -Content $customer `
    -Key "${service}/${environment}/customer.openapi.yaml"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*       Uploading the CloudFormation template files         *"
Write-Host "*************************************************************"
Write-Host " "

Write-S3Object `
    -BucketName ${bucket} `
    -File "${workspacePath}/deployment/ecs/templates/network.yaml" `
    -Key "${service}/${environment}/network.yaml"

Write-S3Object `
    -BucketName ${bucket} `
    -File "${workspacePath}/deployment/ecs/templates/services.yaml" `
    -Key "${service}/${environment}/services.yaml"

Write-S3Object `
    -BucketName ${bucket} `
    -File "${workspacePath}/deployment/ecs/templates/roles.yaml" `
    -Key "${service}/${environment}/roles.yaml"

Write-S3Object `
    -BucketName ${bucket} `
    -File "${workspacePath}/deployment/ecs/templates/containers.yaml" `
    -Key "${service}/${environment}/containers.yaml"

Write-S3Object `
    -BucketName ${bucket} `
    -File "${workspacePath}/deployment/ecs/templates/apis.yaml" `
    -Key "${service}/${environment}/apis.yaml"

$templateBody = Get-Content -Path "${workspacePath}/deployment/ecs/templates/master.yaml" -raw

if (-Not (Test-CFNStack -Region ${region} -StackName "${service}${environment}"))
{
  Write-Host " "
  Write-Host "*************************************************************"
  Write-Host "*            Executing the create stack command             *"
  Write-Host "*************************************************************"
  Write-Host " "

  New-CFNStack `
      -StackName "${service}${environment}" `
      -TemplateBody $templateBody `
      -Parameter @( @{ ParameterKey="BucketName"; ParameterValue="${bucket}" }, `
                    @{ ParameterKey="ServiceName"; ParameterValue="${service}" }, `
                    @{ ParameterKey="ServiceNameLower"; ParameterValue="${serviceLower}" }, `
                    @{ ParameterKey="EnvironmentName"; ParameterValue="${environment}" }, `
                    @{ ParameterKey="EnvironmentNameLower"; ParameterValue="${environmentLower}" }, `
                    @{ ParameterKey="VersionNumber"; ParameterValue="${version}" }, `
                    @{ ParameterKey="StageName"; ParameterValue="${stage}" }, `
                    @{ ParameterKey="UserPoolDomainName"; ParameterValue="${domain}" }) `
                    @{ ParameterKey="CpuArchitectureName"; ParameterValue="${cpu}" }) `
      -Capability CAPABILITY_IAM,CAPABILITY_NAMED_IAM,CAPABILITY_AUTO_EXPAND

  Wait-CFNStack -Region ${region} `
      -StackName "${service}${environment}" `
      -Status CREATE_COMPLETE,ROLLBACK_COMPLETE `
      -Timeout 1200
}
else {
  Write-Host " "
  Write-Host "*************************************************************"
  Write-Host "*            Executing the update stack command             *"
  Write-Host "*************************************************************"
  Write-Host " "

  $domain = ((Get-CFNStack `
      -StackName "${service}${environment}").Outputs `
      | Where-Object {$_.OutputKey -EQ 'UserPoolDomainName'}).OutputValue

  Update-CFNStack `
      -StackName "${service}${environment}" `
      -TemplateBody $templateBody `
      -Parameter @( @{ ParameterKey="BucketName"; ParameterValue="${bucket}" }, `
                    @{ ParameterKey="ServiceName"; ParameterValue="${service}" }, `
                    @{ ParameterKey="ServiceNameLower"; ParameterValue="${serviceLower}" }, `
                    @{ ParameterKey="EnvironmentName"; ParameterValue="${environment}" }, `
                    @{ ParameterKey="VersionNumber"; ParameterValue="${version}" }, `
                    @{ ParameterKey="StageName"; ParameterValue="${stage}" }, `
                    @{ ParameterKey="UserPoolDomainName"; ParameterValue="${domain}" }) `
                    @{ ParameterKey="CpuArchitectureName"; ParameterValue="${cpu}" }) `
      -Capability CAPABILITY_IAM,CAPABILITY_NAMED_IAM,CAPABILITY_AUTO_EXPAND

  Wait-CFNStack `
      -StackName "${service}${environment}" `
      -Status UPDATE_COMPLETE,ROLLBACK_COMPLETE -Timeout 1200 
}

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*                Listing the stack outputs                  *"
Write-Host "*************************************************************"
Write-Host " "

$outputs = (Get-CFNStack -StackName "${service}${environment}").Outputs

Write-Output $outputs

Write-Host " "
