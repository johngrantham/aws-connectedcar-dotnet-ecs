#!/bin/zsh

source config.sh

echo " "
echo "*************************************************************"
echo "*         Deleting existing files in the S3 folder          *"
echo "*************************************************************"
echo " "

aws s3 rm --recursive s3://${bucket}/${service}/${environment}

echo " "

