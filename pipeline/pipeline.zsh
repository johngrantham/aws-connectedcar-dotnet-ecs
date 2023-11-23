#!/bin/zsh

workspacePath="/Users/Shared/Repos/aws-connectedcar-dotnet-ecs"
commonPath="/Users/Shared/Repos/aws-connectedcar-common"
bucket="connectedcar-deployment-205412"
service="ConnectedCar"
serviceLower="connectedcar"
environment="Dev"
environmentLower="dev"
stage="api"

number=$(date +"%H%M%S")
domain="connectedcar${number}"

buildFile="buildspec/build.buildspec.yml"
deployFile="deployment/templates/master.yaml"
testFile="buildspec/test.buildspec.yml"

repoOwner="johngrantham"
sourceRepoName="aws-connectedcar-dotnet-ecs"
commonRepoName="aws-connectedcar-common"

echo " "
echo "*************************************************************"
echo "*      Executing create stack command for the pipeline      *"
echo "*************************************************************"
echo " "

aws cloudformation create-stack \
    --stack-name ${service}Pipeline${environment} \
    --template-body file://${workspacePath}/pipeline/pipeline.yaml \
    --parameters ParameterKey=BucketName,ParameterValue=${bucket} \
                 ParameterKey=ServiceName,ParameterValue=${service} \
                 ParameterKey=ServiceNameLower,ParameterValue=${serviceLower} \
                 ParameterKey=EnvironmentName,ParameterValue=${environment} \
                 ParameterKey=EnvironmentNameLower,ParameterValue=${environmentLower} \
                 ParameterKey=StageName,ParameterValue=${stage}  \
                 ParameterKey=UserPoolDomainName,ParameterValue=${domain} \
                 ParameterKey=BuildFile,ParameterValue=${buildFile} \
                 ParameterKey=TestFile,ParameterValue=${testFile} \
                 ParameterKey=DeployFile,ParameterValue=${deployFile} \
                 ParameterKey=RepoOwner,ParameterValue=${repoOwner} \
                 ParameterKey=SourceRepoName,ParameterValue=${sourceRepoName} \
                 ParameterKey=CommonRepoName,ParameterValue=${commonRepoName} \
    --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND

echo " "
