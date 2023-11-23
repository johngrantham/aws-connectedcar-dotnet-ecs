#!/bin/zsh

source config.zsh

echo " "
echo "*************************************************************"
echo "*           Building the container image locally            *"
echo "*************************************************************"
echo " "

cd ${workspacePath}

aws ecr get-login-password \
    --region ${region} | docker login \
    --username AWS \
    --password-stdin ${account}.dkr.ecr.${region}.amazonaws.com

docker build --build-arg TOKEN=${token} -t ${serviceLower}-${environmentLower} -f Dockerfile .

docker tag \
    ${serviceLower}-${environmentLower}\:latest \
    ${account}.dkr.ecr.${region}.amazonaws.com/${serviceLower}-${environmentLower}\:latest

echo " "
echo "*************************************************************"
echo "*            Pushing the image to the repository            *"
echo "*************************************************************"
echo " "

docker push ${account}.dkr.ecr.${region}.amazonaws.com/${serviceLower}-${environmentLower}\:latest

echo " "
echo "*************************************************************"
echo "*        Building and packaging the Lambda zip file         *"
echo "*************************************************************"
echo " "

dotnet lambda package \
    --framework net6.0 \
    -pl ${workspacePath}/src/ConnectedCar.Lambda \
    -o ${TMPDIR}/Lambda-${version}.zip

echo " "
echo "*************************************************************"
echo "*      Uploading the Lambda zip file to the S3 folder       *"
echo "*************************************************************"
echo " "

aws s3 cp \
    ${TMPDIR}/Lambda-${version}.zip \
    s3://${bucket}/${service}/${environment}/Lambda-${version}.zip

echo " "


