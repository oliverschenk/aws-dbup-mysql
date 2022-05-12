# AWS DbUp MySql

This repo contains the code for the Medium article [Read Secrets Manager secrets and perform RDS database migrations using Lambda](https://medium.com/@oliver.schenk/read-secrets-manager-secrets-and-perform-rds-database-migrations-using-lambda-7bbea31938b4).

It follows on from the article [Achieving RDS password rotation with Secrets Manager](https://medium.com/@oliver.schenk/achieving-rds-password-rotation-with-secrets-manager-3444fa30c94b).

Please note that the resources this project requires are NOT free. Deploy at your own risk and destroy when no longer needed.

## Prerequisites

- Dotnet SDK 6.0
- Serverless CLI
- AWS account with Administrator access
- aws-vault (only required if using deployment script `deploy.sh`)

This article depends on resources created in the article [Achieving RDS password rotation with Secrets Manager](https://medium.com/@oliver.schenk/achieving-rds-password-rotation-with-secrets-manager-3444fa30c94b).

## Getting Started

Use the deployment script proivided to build, deploy and run the migration Lambda function. This method assumes you have `aws-vault` configured with the necessary credentials.

You can configure the default region in the `deploy.sh` file.


```
./deploy.sh

DESCRIPTION:
  Script for deploying serverless lambda.

USAGE:
  deploy.sh -p credentials_profile [-r region] [-s stage] [-d destroy]

OPTIONS
  -p   the credentials profile to use (uses aws-vault)
  -r   region (default: ap-southeast-2)
  -s   the stage to deploy [dev, test, prod] (default: dev)
  -d   destroy
```

```
# to apply
./deploy.sh -p <aws_vault_profile>

# to destroy
./deploy.sh -p <aws_vault_profile> -d
```