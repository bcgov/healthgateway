# Base Image Creation

Creates a base image that can be utilized by other builds that require ASP .Net 9.0

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/BaseImage/build.yaml | oc apply -f -
```
## Overview

Some projects using this base image may require the installation of the following certificates: we need to install the following certificates into the base image:

1. TLSChain.crt
2. TLSRoot.crt
3. TrustedRoot.crt

To avoid storing these certificates in our repository, we utilize Azure Secrets. The certificates are first converted to Base64 format before being stored in Azure. During the build process, they are decoded and included in the Docker image.

## Preparing Certificates for Azure

Before storing the certificates in Azure, they must be converted to Base64 format using the following commands:

```console
base64 -i tls-chain.crt -o tls-chain.b64
base64 -i trusted-root.crt -o trusted-root.b64
base64 -i tls-root.crt -o tls-root.b64
```
The resulting .b64 files can then be stored as secrets in Azure DevOps.

## Creating Secrets in Azure

Follow these steps to create secrets in Azure DevOps:

1. Navigate to Azure DevOps:
    - Open your Azure DevOps project.
    - Go to Pipelines > Library.

2. In Variable Groups:
   - Click on Secrets.

3. Add the Base64 Encoded Certificates as Secrets:
   - Click + Add to create a new variable.
   - Enter the following names and their corresponding Base64-encoded values from the .b64 files:
      - tls-chain
      - tls-root
      - trusted-root
   - Click the lock icon to mark them as secrets.
   - Save the changes.

4. Reference the Secrets in the Azure Pipeline:
   - Ensure your pipeline YAML file includes the variable group: Secrets
   - The secrets will be accessible as '\$(tls-chain)', '\$(tls-root)', and '\$(trusted-root)' in the pipeline.

    ```console
    variables:
       - group: Secrets
    ```

## Summary

1. Convert certificates to Base64 before storing them in Azure Secrets.
2. Create a Variable Group in Azure DevOps and store the secrets securely.
3. Reference the secrets in the Azure Pipeline YAML file.

