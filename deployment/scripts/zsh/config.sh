#!/bin/zsh

set -e

workspacePath="/Users/Shared/Repos/aws-connectedcar-dotnet-ecs"
bucket="connectedcar-deployment-205412"
service="ConnectedCar"
serviceLower="connectedcar"
environment="Dev"
environmentLower="dev"
version="20220801"
stage="api"

number=$(date +"%H%M%S")
domain="connectedcar${number}"

account=$(aws sts get-caller-identity --query "Account" --output text)
region=$(aws configure get region)

cpu="ARM64"

token="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"

echo " "
echo "*************************************************************"
echo "*            Validating the config.sh variables             *"
echo "*************************************************************"
echo " "

if [ "${account}" = "" ] ; then
    echo "Error: default AWS account is not configured. Use the 'aws configure' command"
    exit 1
fi

if [ "${region}" = "" ] ; then
    echo "Error: default AWS region is not configured. Use the 'aws configure' command"
    exit 1
fi

if ! [ -d "${workspacePath}" ] ; then
  echo "Error: workspacePath is not valid"
  exit 1
fi

aws s3api head-bucket --bucket ${bucket}
