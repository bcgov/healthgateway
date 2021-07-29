# Patroni Postgres

## Installation

### OpenShift 4

In OpenShift 4, we don't have a common image for Postgres and have to create our own

```console
oc project 0bd5ad-tools
oc process -f ./openshift/build.yaml | oc apply -f -
oc start-build patroni-pg11 | oc apply -f -
```

Once that has completed, we can then deploy a standard single cluster per namespace by

1. Creating the secrets, service account, and role bindings
1. Allowing the service accounts to pull from the tools namespace
1. Creating the statefulset

Via the command line this looks like

```console
oc project 0bd5ad-dev
oc process -f ./openshift/deployment-prereq.yaml -p PATRONI_SUPERUSER_PASSWORD='[The Password]' -p PATRONI_REPLICATION_PASSWORD='[The Password]' -p APP_DB_PASSWORD='[The Password]' | oc apply -f -
oc process -f ./openshift/rb-pullers.yaml | oc apply -f -
oc process -f ./openshift/deployment.yaml | oc apply -f -
```

NOTE: The provisioning of disk is fairly slow and you may not notice anything for a few minutes.

#### Clean up

If you would like to cleanup, you can execute the following which will remove all data associated to Patroni

```console
# Clean everthing
oc delete all -l cluster-name=patroni-postgres
oc delete pvc,secret,configmap,rolebinding,role,sa -l cluster-name=patroni-postgres
```

Volumes left over to cleanup
Service account

## Troubleshooting

### Identifying the master node

Using the UI:
Navigate to Services > patroni-postgres-master
This service should be listing the current master node.

Using the command line:

```bash
oc get pods -l role=master
```

or

```bash
oc describe configmaps patroni-postgres-leader;
```

### Connection to the database

The connection requires a port tunnel from the pod in openshift to the local computer,
use the following command to create a tunnel:

```bash
oc port-forward <pod-name> <local-port>:5432
```

example:

```bash
oc port-forward patroni-postgres-1 5432:5432
```

Use pgadmin or psql to connect to server: localhost:5432, username and password defined in the secrets.

### Manual switch of master pod

Updates the configmap that stores the current leader node (master),
The script below sets the leader to node 2:

```bash
oc annotate configmaps patroni-postgres-leader leader=patroni-postgres-2 --overwrite=true;
```

## Restore Backup Script

Backups are generated daily by the dc "backup" in the production realm and are composed by a SQL script containing the database structure + data.

To restore the backup follow these steps:

1. Connect to openshift using the terminal/bash and set the project to the production one.
1. Download the backup file to your local computer using the command below:

    ```bash
    oc rsync <backup-pod-name>:/backups/daily/<date> <local-folder>
    ```

    This copies the folder from the pod to the local folder.

1. Extract backup script using gzip:

    ```bash
    gzip -d <file-name>
    ```

1. Connect to the master database pod using port-forward (See 'Connection to the Database').

1. Manually create the database:

    ```bash
    psql -h localhost -p 5432 -U postgres -c 'create database gateway;'
    ```

1. Execute the script to restore the database:

    ```bash
    psql -h localhost -d gateway -U postgres -p 5432 -a -q -f <path-to-file>
    ```

## Delete Database (Cleanup)

1. Drop all current connections:

    ```bash
    psql -h localhost -p 5432 -U postgres -c "select pg_terminate_backend(pid) from pg_stat_activity where datname='gateway';"
    ```

1. Drop Database:

    ```bash
    psql -h localhost -p 5432 -U postgres -c 'drop database gateway;'
    ```

1. Create Database:

    ```bash
    psql -h localhost -p 5432 -U postgres -c 'create database gateway;'
    ```

1. Run Migrations Scripts

    ?

## Edit Patroni configuration using Rest API

1. Connect the terminal to any of the patroni pods running using remote shell:

    ```bash
    oc rsh patroni-postgres-1
    ```

1. Update config (e.g. postgresql.parameters.max_connections to 500):

    ```bash
    curl -s -XPATCH -d '{"postgresql":{"parameters":{"max_prepared_transactions":500, "max_connections":500}}}' http://localhost:8008/config | jq .
    ```

1. Restart Cluster:

    ```bash
    patronictl restart patroni-postgres
    ```

1. Get/View the current config:

    ```bash
    curl -s localhost:8008/config | jq .
    ```
