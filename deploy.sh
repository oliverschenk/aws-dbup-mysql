#!/bin/bash
set -e

export NAME='my-project'

DEPLOY_ACTION='deploy'
DESTROY_ACTION='remove'
DEFAULT_RELEASE_TYPE='Debug'

DEFAULT_REGION='ap-southeast-2'
DEFAULT_STAGE='dev'

AWS_VAULT_PREFIX=''

REGION=$DEFAULT_REGION
STAGE=$DEFAULT_STAGE
RELEASE_TYPE=$DEFAULT_RELEASE_TYPE
ACTION=$DEPLOY_ACTION

function usage {
    echo "DESCRIPTION:"
    echo "  Script for deploying serverless lambda."
    echo ""
    echo "USAGE:"
    echo "  deploy.sh [-p credentials_profile] [-r region] [-s stage] [-d destroy]"
    echo ""
    echo "OPTIONS"
    echo "  -p   the credentials profile to use (uses aws-vault)"
    echo "  -r   region (default: ap-southeast-2)"
    echo "  -s   the stage to deploy [dev, test, prod] (default: dev)"
    echo "  -d   destroy"
}

function aws_exec {
    ${AWS_VAULT_PREFIX}$1
}

while getopts "p:r:s:d" option; do
    case ${option} in
        p ) AWS_VAULT_PROFILE=$OPTARG;;
        r ) REGION=$OPTARG;;
        s ) STAGE=$OPTARG;;
        d ) ACTION=$DESTROY_ACTION;;
        \? )
            echo "Invalid option: -$OPTARG" 1>&2
            usage
            exit 1
            ;;
    esac
done

if [[ -n "${VALIDATION_ERROR}" ]]; then
    usage
    exit 1
fi

if [[ -n "${AWS_VAULT_PROFILE}" ]]; then
    AWS_VAULT_PREFIX="aws-vault exec ${AWS_VAULT_PROFILE} -- "
fi

case ${STAGE,,} in

  dev)
    RELEASE_TYPE='Debug'
    ;;

  test | prod)
    RELEASE_TYPE='Release'
    ;;

  *)
    RELEASE_TYPE='Release'
    ;;
esac

echo "=== Using the following parameters ==="
echo "Region: ${REGION}"
echo "Stage: ${STAGE}"
echo "Release: ${RELEASE_TYPE}"
echo "Action: ${ACTION}"

if [[ "${ACTION}" = "${DEPLOY_ACTION}" ]]; then

  echo ""
  echo "=== Restoring dependencies ==="
  dotnet restore

  echo ""
  echo "=== Building dotnet project ==="
  dotnet build -c ${RELEASE_TYPE}

  echo ""
  echo "=== Creating lambda package ==="
  dotnet lambda package -c ${RELEASE_TYPE}

fi

echo ""
echo "=== Applying action: ${ACTION} ==="
aws_exec "serverless ${ACTION} --region ${REGION} --stage ${STAGE}"

if [[ "${ACTION}" = "${DEPLOY_ACTION}" ]]; then

  echo ""
  echo "=== Seeding database ==="
  aws_exec "aws lambda invoke --function-name ${STAGE}-${NAME}-aws-dbup-mysql response.json"
  rm response.json

fi

echo ""
echo "Completed."