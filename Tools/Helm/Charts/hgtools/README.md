# Health Gateway Tools Helm Chart

This directory contains a Helm chart to deploy Health Gateway related tools and utilities in OpenShift Gold cluster.

## Usage

To install or upgrade, run the following command :

```sh
helm -n c8055e-tools upgrade hgtools . --install --set dr=[false/true] --set token=[azure DevOps PAT]
```

**Note: notice the Gold cluster you're currently logged in and set dr=true/false accordingly, i.e. dr=false for Gold and dr=true for Gold DR.**

If you're logged in to both, set the context by running the following command:

To set the context to Gold:

```sh
kubectl config use-context [namespace] /api-gold-devops-gov-bc-ca:6443/[user]@github
```

To set to context Gold DR:

```sh
kubectl config use-context [namespace] /api-golddr-devops-gov-bc-ca:6443/[user]@github
```

## Resources

The following resources are managed by this chart:

- Azure Agents for each environment (tools/dev/test/prod) configured as a statefulset
- Image streams for each Health Gateway component
