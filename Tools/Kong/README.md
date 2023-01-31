# Kong Configuration

## Prerequisites

Read the [GWA API documentation](https://bcgov.github.io/aps-infra-platform/guides/owner-journey/).

Based on reading above documentation you should have the `gwa` command-line application installed locally.

Grab the environment files from Sharepoint (`hg-dev.env`, `hg-test.env`, and `hg-test.env`) and place them in this folder.

## Enabling Access to OpenShift

To allow Kong to access the OpenShift environments, network policies should be added by running these commands for each environment:

```shell
oc project [environment]
oc apply -f network-policies.yaml
```

## Publish Kong Configurations

Running `generate.sh` will generate configuration files for each of the environments by replacing the variables in `config.tmpl` with values from the appropriate `.env` file and iterating over each service.

```shell
./generate.sh
```

> The variable substitutions are performed using `envsubst`, which is part of `gettext`. If it's not preinstalled on your system, you can use a package manager to install `gettext`.

After the configurations for the environments have been generated, they can be published by running the following script. This will override any previously published configurations.

```shell
./publish.sh
```

## Publish to Kong using the pipeline

The actual publishing to Cong dev, test and prod is done from a Azure DevOps pipeline using the preconfigured `Kong - [env]` library variable groups.
