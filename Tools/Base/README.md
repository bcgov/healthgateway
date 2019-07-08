# Dot Net 2.2 Base Image

Creates a base image that can be utilized by other builds that require .Net Core 2.2

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/Base/build.yaml | oc apply -f -
```


# Webclient Deployment

Script to process a template file that creates:
	- Deployment Configuration (image based on "tools namespace/image stream:tag")
	- Horizontal Pod Autoscaler
	- Route	
	- Service

Replace <ENV> and <TAG> with the appropriates.
	
```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/Base/deployment.yaml -p NAMESPACE=q6qfzk -p APP_NAME=webclient -p ENV=<ENV> -p IMAGE="webclient:<TAG>" | oc apply -f -
```

