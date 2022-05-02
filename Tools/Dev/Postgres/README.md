# Developer DB Setup

To initialize the Health Gateway DB start Docker image

```console
docker compose up -d
docker logs --follow gatewaydb
```

Once that DB has started, and you see a message like

```console
database system is ready to accept connections
```

At this point you can stop following the logs CTRL-C and start the JobScheduler application to run the migrations or view the DBMainter README to run them manually.
