# Azure DevOps OpenShift Agent

Creates and deploys the Azure Agent in the OpenShift environment.  The running agent will accept one job and will exit after completion. 

## ToDo
After the Azure Agent exits OpenShift will sometimes delay based on Crash Loop Back Off.  This is true regardless of the exit code and a solution still needs to be investigated.

## Web Hooks

The template will create a Web Hook that can be used to monitor the source GIT repository and update automatically.  

## Usage

To review the parameters execute:

```console
oc process -f ./openshift/AzureAgent.yaml --parameters
```

To create the Azure Agent, be in your tools project and minimally execute:

```console
oc process -f ./openshift/AzureAgent.yaml -p AZ_DEVOPS_ORG_URL=<URL> -p AZ_DEVOPS_TOKEN=<PAT>
```
