# Developer Tooling Setup

## Postgres

### DB Initialization

To initialize the Health Gateway DB start the Docker image

```console
docker compose up -d
docker logs --follow gatewaydb
```

Once the DB has started and you see a message like

```console
database system is ready to accept connections
```

you can stop following the logs CTRL-C and start the JobScheduler application to run any new migrations and run the drug loader tasks.  You can also view the DBMaintainer README to run them manually.

### Upgrading init scripts

If you would like to update the initialization script in the DBMaintainer folder

```bash
dotnet ef migrations script --output 01_init-to-XXX.sql --idempotent
```

Where XXX is the name of the last migration included.  You then need to add the following to the top of the file

```console
\c hglocal
SET ROLE hglocal;
```

### Clean up

Remove the pgdata volume by executing

```console
docker volume rm gatewaydb.local
```

## Redis

### Clean up

Remove the Redis volume by executing

```console
docker volume rm gatewaycache.local
```

## Seq

### Verify

```console
curl -XPOST "http://localhost:5341/api/events/raw?clef" \
  -d "{'@t':'2018-06-07T03:44:57.8532799Z','@mt':'Hello, {User}','User':'alice'}"
```
### Password

```console
echo $(echo 'NewPassword' | docker run --rm -i datalust/seq config hash)
```

### Cleanup

Remove the Seq log volume by executing

```console
docker volume rm gatewaylogs.local
```
