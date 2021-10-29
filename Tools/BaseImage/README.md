# Base Image Creation

Creates a base image that can be utilized by other builds that require ASP .Net 5.0

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/BaseImage/build.yaml | oc apply -f -
```
