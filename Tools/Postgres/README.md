# Patroni Postgres

## Installation

Ensure you have a Network Securiy Policy that permits pods to communicate with each other within the project.
The [BaseBuild NSP](../BaseBuild/nsp.yaml) should meet all of the requirements for this.

As a common image for Patroni does not exist, we need to create one based

```console
oc project 0bd5ad-tools
oc process -f ./openshift/build.yaml | oc apply -f -
oc start-build patroni-pg11 | oc apply -f -
```

You will need to tag patroni:v11-latest with the environment that you're deploying to (mock, dev, test, production)

```console
oc tag patroni:v11-latest patroni:dev
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
oc process -f ./openshift/deployment.yaml -p IMAGE_STREAM_TAG=dev | oc apply -f -
oc create -f ./openshift/pdb.yaml
```

NOTE: The provisioning of disk is fairly slow and you may not notice anything for a few minutes.

### Clean up

CAUTION: If you would like to remove all resources associated to Patroni, you can execute the following

```console
oc project obd5ad-dev
oc delete all -l cluster-name=patroni-postgres
oc delete pvc,secret,configmap,rolebinding,role,sa -l cluster-name=patroni-postgres
```

## Backup Container Deployment

In order to backup/restore the database, we utilize the [bcgov backup container](https://github.com/BCDevOps/backup-container)

On your local machine execute the following commands

```console
git clone git@github.com:BCDevOps/backup-container.git
cd backup-container/openshift/templates/backup
oc process -f backup-build.yaml | oc apply -f -
oc process -f backup-deploy.yaml -p IMAGE_NAMESPACE=0bd5ad-dev -p ENVIRONMENT_NAME=0bd5ad-dev -p ENVIRONMENT_FRIENDLY_NAME='HEALTH-GATEWAY (dev)'
oc create configmap backup-conf --from-file=[HG GIT PATH]/healthgateway/Tools/Postgres/backup.conf
```

## Troubleshooting

### Identifying the master node

Using the UI:
Navigate to Services > patroni-postgres-master
This service should be listing the current master node.

Using the command line:

```console
oc get pods -l role=master
```

or

```console
oc describe configmaps patroni-postgres-leader
```

### Connect to the database

The connection requires a port tunnel from the pod in openshift to the local computer, generally for a local port we use 5431.

use the following command to create a tunnel:

```bash
oc port-forward <pod-name> <local-port>:5432
```

example:

```bash
oc port-forward patroni-postgres-1 5431:5432
```

Use pgadmin or psql to connect to server: localhost:5431, username and password defined in the secrets.

### Patroni Maintenance

Read the Patroni [documentation](https://patroni.readthedocs.io/en/latest/).

Connect to the master DB pod using a terminal window

```console
oc project 0bd5ad-dev
oc rsh $(oc get pods -l role=master -o template --template '{{range .items}}{{.metadata.name}}{{end}}')
$
```

#### Initialize a broken cluster node

```console
$ patronictl reinit patroni-postgres patroni-postgres-[node#]
+ Cluster: patroni-postgres (6891020645577564276) -------+----+-----------+
| Member             | Host          | Role    | State   | TL | Lag in MB |
+--------------------+---------------+---------+---------+----+-----------+
| patroni-postgres-0 | 10.0.0.0      | Replica | running | 88 |         0 |
| patroni-postgres-1 | 10.0.0.0      | Leader  | running | 88 |           |
| patroni-postgres-2 | 10.0.0.0      | Replica | running | 88 |         0 |
+--------------------+---------------+---------+---------+----+-----------+
Are you sure you want to reinitialize members patroni-postgres-#? [y/N]:
```

#### Perform a switch over

```console
$ patronictl switchover
Master [patroni-postgres-1]:
Candidate ['patroni-postgres-0', 'patroni-postgres-2'] []: patroni-postgres-0
When should the switchover take place (e.g. 2022-03-03T07:03 )  [now]:
Current cluster topology
+ Cluster: patroni-postgres (6891020645577564276) -------+----+-----------+
| Member             | Host          | Role    | State   | TL | Lag in MB |
+--------------------+---------------+---------+---------+----+-----------+
| patroni-postgres-0 | 10.0.0.0      | Replica | running | 88 |         0 |
| patroni-postgres-1 | 10.0.0.0      | Leader  | running | 88 |           |
| patroni-postgres-2 | 10.0.0.0      | Replica | running | 88 |         0 |
+--------------------+---------------+---------+---------+----+-----------+
Are you sure you want to switchover cluster patroni-postgres, demoting current master patroni-postgres-1? [y/N]:
```

#### Execute queries

Connect to the DB

```console
psql -h localhost -p 5432 -U postgres
```

##### Terminate all connections

```console
select pg_terminate_backend(pid) from pg_stat_activity where datname='gateway';
```

#### Edit Patroni configuration using Rest API

1. Update config (e.g. postgresql.parameters.max_connections to 500):

    ```console
    curl -s -XPATCH -d '{"postgresql":{"parameters":{"max_prepared_transactions":500, "max_connections":500}}}' http://localhost:8008/config | jq .
    ```

1. Restart Cluster:

    ```console
    $ patronictl restart patroni-postgres
    + Cluster: patroni-postgres (6891020645577564276) -------+----+-----------+
    | Member             | Host          | Role    | State   | TL | Lag in MB |
    +--------------------+---------------+---------+---------+----+-----------+
    | patroni-postgres-0 | 10.0.0.0      | Replica | running | 88 |         0 |
    | patroni-postgres-1 | 10.0.0.0      | Leader  | running | 88 |           |
    | patroni-postgres-2 | 10.0.0.0      | Replica | running | 88 |         0 |
    +--------------------+---------------+---------+---------+----+-----------+
    When should the restart take place (e.g. 2022-03-03T07:09)  [now]:
    Are you sure you want to restart members patroni-postgres-2, patroni-postgres-1, patroni-postgres-0? [y/N]:
    ```

1. Get/View the current config:

    ```console
    $ curl -s localhost:8008/config | jq .
    {
        "postgresql": {
            "parameters": {
            "max_locks_per_transaction": 64,
            "max_prepared_transactions": 500,
            "max_connections": 500
            },
            "use_pg_rewind": true
        }
    }
    ```

## Restore Backup Script

Backups are generated daily by the dc "backup" in the production realm and are composed by a SQL script containing the database structure + data.

To restore the backup follow these steps:

NOTE:  This section is under review as a local to remote restore will take a very long time with our DB size.

1. Connect to openshift using the terminal/bash and set the project to the production one.
1. Get the name of the backup pod

    ```console
    oc get pods -l "name=backup-postgres"
    ```

1. Download the backup file to your local computer using the command below:

    ```console
    oc rsync <backup-pod-name>:/backups/daily/<date> <local-folder>
    ```

    This copies the folder from the pod to the local folder.

1. Extract backup script using gzip:

    ```console
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
