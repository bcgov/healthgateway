# Azure DevOps OpenShift Agent

Creates and deploys the Azure Agent in the OpenShift environment.  The running agent will accept one job and will exit after completion. 

Microsoft Azure Agent [documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/docker?view=azure-devops) was very helpful.

## Creating a Personal Access Token

A Personal Access Token (PAT) is an authentication method similar to using a password. Our Azure Agents use the PAT for authentication into Azure DevOps.

An Azure DevOps Administrator will have to:

* Open up a web browser and in the Azure DevOps portal, click user settings where the PAT settings exist.
* Click on New Token
* Give it a name and pick an expiry out the maximum
* In the scopes section Custom defined is selected
* Scroll to the bottom and click on Show all scopes
* Under Agent Pools enable Read & Manage
* Click create and take note of the PAT which will be used during deployment

## ToDo

After the Azure Agent exits OpenShift will sometimes delay restart due toCrash Loop Back Off.  This is true regardless of the exit code and the number of agents deployed needs to accomodate this downtime and the concurrency of builds.

## Deployment

To review the parameters execute:

```console
oc process -f ./openshift/AzureAgent.yaml --parameters
```

To create the Azure Agent, be in your tools project and minimally execute:

```console
oc process -f ./openshift/AzureAgent.yaml -p AZ_DEVOPS_ORG_URL=<URL> -p AZ_DEVOPS_TOKEN=<PAT> | oc apply -f -
```

Resulting in

```console
laws@Crius:.../Health/healthgateway/Tools/AzureAgent$ oc process -f ./openshift/AzureAgent.yaml -p AZ_DEVOPS_ORG_URL=<URL> -p AZ_DEVOPS_TOKEN=<PAT> | oc apply -f -
warning: error calculating patch from openapi spec: map: map[] does not contain declared merge key: name
rolebinding.authorization.openshift.io/azure-agent configured
configmap/azure-agent-config unchanged
secret/azure-agent-token configured
secret/azure-agent-hooksecret configured
imagestream.image.openshift.io/azure-agent unchanged
buildconfig.build.openshift.io/azure-agent-build configured
deploymentconfig.apps.openshift.io/azure-agent configured
error: map: map[] does not contain declared merge key: name
```

Note:  The error above is from the service account not being re-recreated.

## Updating Agent Image

If you need to update the base image that the Azure agent uses you would

* Update the docker/Dockerfile
* Commit to the dev branch
* Trigger a new build in OpenShift UI

This is only required if software version need to change, the Azure Agent itself will update on each start.
