# Azure DevOps OpenShift Agent

Creates and deploys the Azure Agent in the OpenShift environment.

Microsoft Azure Agent [documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/docker?view=azure-devops) was very helpful.

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
