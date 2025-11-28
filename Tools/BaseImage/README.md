# Base Image Creation

Creates a base image that can be utilized by other builds that require ASP .Net 8.0

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/BaseImage/build.yaml | oc apply -f -
```
## Certificates

This base image includes several trusted certificate files required by downstream services.
All `.crt` files are stored in the same directory as the Dockerfile and are copied into
`/usr/local/share/ca-certificates` during the build, followed by `update-ca-certificates`.

To add or update a certificate:

1. Place the `.crt` file in this directory using lowercase kebab-case naming.
2. Add a corresponding `COPY` line in the Dockerfile.
3. Rebuild the base image via the pipeline.
