# Patroni Postgres

## Installation

Process the template patroni.yaml in Openshift, the parameters are self explanatory, populate them as needed. 

The objects created include:
    1) Stateful Set Patroni-postgres containing 3 pods.
    2) Master service (eg patroni-postgres-master)
        Points to the elected master pod, which contains the transactional 
        database (selector is application=patroni-postgres, cluster-name=patroni-postgres, role=master).
    3) Replica service (eg patroni-postgres-replica)
        Points to the replica pods, which contains the read-only database, 
        we are currently configured for 2 replica pods (selector is application=patroni-postgres, cluster-name=patroni-postgres, role=replica).
    4) Service account "patroni" with a role binding to the "patroni" role.
    5) Volumes for each of the pods (5gb default).
    6) Config map that stores the current master pod (patroni-postgres-leader).
    7) Config map that stores the database configuration like "max_connections" (patroni-postgres-config).
    8) Secret containing username and password.

## Troubleshooting

### Identifying the master node

Using the UI:
Navigate to Services > patroni-postgres-master
This service should be listing the current master node.

Using the command line:

``` bash
oc get pods -l role=master
```
or

``` bash
oc describe configmaps patroni-postgres-leader;
```

### Connection to the database 

The connection requires a port tunnel from the pod in openshift to the local computer,
use the following command to create a tunnel:

``` bash
oc port-forward <pod-name> <local-port>:5432
```

example:

``` bash
oc port-forward patroni-postgres-1 5432:5432
```

Use pgadmin or psql to connect to server: localhost:5432, username and password defined in the secrets.


### Manual switch of master pod

Updates the configmap that stores the current leader node (master),
The script below sets the leader to node 2:

``` bash
oc annotate configmaps patroni-postgres-leader leader=patroni-postgres-2 --overwrite=true;
```

## Restore Backup

To download the backup to your local computer:

``` bash
oc rsync <backup-pod-name>:/backups/daily/<date> <local-folder>
```

Extract backup using gzip:

``` bash
gzip -d <file-name>
```

Connect to the master database pod using port-forward (See 'Connection to the Database').

Manually create the database:

``` bash
psql -h localhost -p 5432 -U postgres -c 'create database gateway;'
```

Execute the backup script:

``` bash
psql -h localhost -d gateway -U postgres -p 5432 -a -q -f <path-to-file>
```

