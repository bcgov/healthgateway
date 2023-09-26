# Health Gateway Helm Chart

This directory contains a Helm chart to deploy Health Gateway services in OpenShift Gold clusters.

## Usage

Before using this chart, you'll need to obtain the latest copy of the environment configuration files and place them in `envs` folder.

To install or upgrade:

```sh
helm -n [license plate]-[env] upgrade [env] . --install -f envs/[env]/values[_dr].yaml
```

-   env can be dev, test or prod
-   \_dr is to be used when charting Gold DR.

To debug the generated yaml:

```sh
helm template . -f envs/[env]/values[_dr].yaml > debug.yaml
```

**Note: notice the Gold cluster you're currently logged in and use the correct values file**

If you're logged in to both, set the context by running the following command:

To set the context to Gold:

```sh
kubectl config use-context [namespace] /api-gold-devops-gov-bc-ca:6443/[user]@github
```

To set to context Gold DR:

```sh
kubectl config use-context [namespace] /api-golddr-devops-gov-bc-ca:6443/[user]@github
```

@github assumes you're logged in with a github account, use @idir if you're logged in with your IDIR account.

## Post install actions

If this is a fresh install, you'll need to configure the redis cluster for the first time, follow the instructions in the redis chart notes.

## Managed Resources

The following resources are managed by this chart:

-   Health Gateway services
-   Related dependencies
    -   redis cluster
    -   postgres cluster in Gold and standby cluster in Gold DR

## Dependencies

This chart depends on [BC GOV SSO Spilo postgres chart](https://github.com/bcgov/sso-helm-charts/tree/main/charts/patroni). Please see the charts docs for configuration options.
