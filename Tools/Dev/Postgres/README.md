# Developer DB Setup

Install Docker

In the Tools/DB folder run

`docker-compose up -d`

Once the DB has started exec

`docker exec -it gatewaydb psql --username=postgres --dbname postgres -a -f /scripts/SetupDevDB.sql`

Start the JobScheduler application to run the migrations
