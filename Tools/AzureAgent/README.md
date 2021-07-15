# Azure DevOps OpenShift Agent

Creates and deploys the Azure Agent in the OpenShift environment.

Microsoft Azure Agent [documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/docker?view=azure-devops) was very helpful.

## Prerequise Image

As Docker Hub has recently put in place pull limits, a pull secret needs to be defined in the namespace that you're deploying to. We would typically do this in tools

```console
oc project 0bd5ad-tools
oc create secret docker-registry docker-secret --docker-server=docker.io --docker-username=healthopenshift --docker-password=[ASK TEAM] --docker-email=stephen.s.laws@gov.bc.ca
oc secrets link builder docker-secret --for=pull
oc secrets link deployer docker-secret --for=pull
```

## Creating a Personal Access Token

A Personal Access Token (PAT) is an authentication method similar to using a password. Our Azure Agents use the PAT for authentication into Azure DevOps.

An Azure DevOps Administrator will have to:

-   Open up a web browser and in the Azure DevOps portal, click user settings where the PAT settings exist.
-   Click on New Token
-   Give it a name and pick an expiry out the maximum
-   In the scopes section Custom defined is selected
-   Scroll to the bottom and click on Show all scopes
-   Under Agent Pools enable Read & Manage
-   Click create and take note of the PAT which will be used during deployment

## Deployment

You need to ensure that the Network Security Policy has been applied to the namespace. Our reference NSP is located at:
/Tools/BaseBuild

Please reference the [README.md](../BaseBuild/README.md) for detailed deployment instructions.

To create the AzureAgent image run the following in the tools project

```console
oc process -f ./openshift/AzureAgentBuild.yaml | oc apply -f -
```

verify that the build was successful before continuing on to deploy the agent.

To review the parameters for the deployment execute:

```console
oc process -f ./openshift/AzureAgent.yaml --parameters
```

run as 

```console
oc process -f ./openshift/AzureAgent.yaml -p AZ_DEVOPS_ORG_URL=<URL> -p AZ_DEVOPS_TOKEN=<PAT> -p INSTALL_NAMESPACE=0bd5ad-tools | oc apply -f -
```

Resulting in

```console
laws@Crius:.../Health/healthgateway/Tools/AzureAgent$ oc process -f ./openshift/AzureAgent.yaml -p AZ_DEVOPS_ORG_URL=<URL> -p AZ_DEVOPS_TOKEN=<PAT> -p INSTALL_NAMESPACE=0bd5ad-tools | oc apply -f -
serviceaccount/azure-agent created
rolebinding.authorization.openshift.io/azure-agent created
configmap/azure-agent-config created
secret/azure-agent-token created
secret/azure-agent-hooksecret created
imagestream.image.openshift.io/azure-agent created
buildconfig.build.openshift.io/azure-agent-build created
deploymentconfig.apps.openshift.io/azure-agent created
```

Note: if you run the script more than once you may see

```console
error: map: map[] does not contain declared merge key: name
```

This is simply the the Service Account not being re-recreated.

You then need to run two additional role bindings to allow tools to view Dev deployments and allow the other namespaces to pull images from tools.

```console
oc process -f ./openshift/rb-dev.yaml -p SRC_PROJECT=0bd5ad-tools -p DST_PROJECT=0bd5ad-dev | oc apply -f -
oc process -f ./openshift/rb-tools.yaml -p LICENSE=0bd5ad -p PULL_PROJECT=0bd5ad-tools | oc apply -f -
```

## Removing AzureAgent

List all resources created

```console
oc get serviceaccount,rolebinding,hpa,cm,secret,is,bc,dc --selector app=azure-agent -o name
```

Assuming that the above returns nothing unexpected, you can issue the delete:

```console
oc delete serviceaccount,rolebinding,hpa,cm,secret,is,bc,dc --selector app=azure-agent -o name
```

## Updating Agent Image

If you need to update the base image that the Azure agent uses you would

-   Update the docker/Dockerfile
-   Commit to the dev branch
-   Trigger a new build in OpenShift UI

This is only required if software version need to change, the Azure Agent itself will update on each start.
