# General Setup

Once you have workstation configured, you will have to perform some configuration steps to get a working Health Gateway development environment.

* VS Code Extensions
* Acquiring the Health Gateway code
* Creating the Database

## VS Code Extensions

You may want to run VS Code and add one or more of the following extensions:

* C#
* Beautify
* Bracket Pair Colorizer 2
* Docker
* GitLens
* Material Theme
* Material Theme Icons
* markdownlint
* npm
* ESlint
* Remote - WSL
* Auto Rename Tag
* Auto Close Tag

## Acquiring the Health Gateway code

Open an Ubuntu 20.04 terminal or Windows command window or Mac Terminal/iTerm window.  
Navigate to a location where you want the application files to be created or create a new directory which will be referenced as $GATEWAYHOME going forward.

```bash
git clone https://github.com/bcgov/healthgateway.git
cd healthgateway
```

## Create the Database

From the healthgateway folder navigate to Tools/Dev/Postgres and start the DB:

```bash
cd $GATEWAYHOME/Tools/Dev/Postgres/
docker-compose up -d
```

Which should result in something similar to:

```bash
 ~  Projects  …  Tools  Dev  Postgres   dev  $    docker-compose up -d
Creating network "postgres_default" with the default driver
Pulling gatewaydb (postgres:latest)...
latest: Pulling from library/postgres
8559a31e96f4: Pull complete
04866763fec8: Pull complete
1705d51f48e5: Pull complete
e59f13162b50: Pull complete
f34bb6f66594: Pull complete
cbfb60b6801a: Pull complete
e8207269011b: Pull complete
89bccd0fcae0: Pull complete
d3be4c4d3a6e: Pull complete
6593b341f133: Pull complete
b63c7214eb05: Pull complete
a4594bc5ebc6: Pull complete
462172dd94a5: Pull complete
abac28c8c3a0: Pull complete
Digest: sha256:9ba6355d27ba9cd0acda1e28afaae4a5b7b2301bbbdc91794dcfca95ab08d2ef
Status: Downloaded newer image for postgres:latest
Creating gatewaydb ... donedocker-compose up -d
```

The Postgres docker image will be downloaded and started, approve any Windows prompt that may occur.  Confirm the DB is running:

```bash
docker ps
docker logs gatewaydb
```

With output looking like:

```bash
 ~  Projects  …  healthgateway  Apps  DBMaintainer   dev  2  $    docker ps
CONTAINER ID        IMAGE               COMMAND                  CREATED             STATUS              PORTS                    NAMES
61e2ea259e41        postgres:latest     "docker-entrypoint.s…"   18 minutes ago      Up 18 minutes       0.0.0.0:5432->5432/tcp   gatewaydb

 ~  Projects  …  Tools  Dev  Postgres   dev  2  $    docker logs gatewaydb
...
PostgreSQL init process complete; ready for start up.

2020-07-21 22:28:10.541 UTC [1] LOG:  starting PostgreSQL 12.3 (Debian 12.3-1.pgdg100+1) on x86_64-pc-linux-gnu, compiled by gcc (Debian 8.3.0-6) 8.3.0, 64-bit
2020-07-21 22:28:10.542 UTC [1] LOG:  listening on IPv4 address "0.0.0.0", port 5432
2020-07-21 22:28:10.542 UTC [1] LOG:  listening on IPv6 address "::", port 5432
2020-07-21 22:28:10.550 UTC [1] LOG:  listening on Unix socket "/var/run/postgresql/.s.PGSQL.5432"
2020-07-21 22:28:10.568 UTC [65] LOG:  database system was shut down at 2020-07-21 22:28:10 UTC
2020-07-21 22:28:10.572 UTC [1] LOG:  database system is ready to accept connections
```

Run the initial migrations to create the database tables and other objects:

```bash
cd $GATEWAYHOME/Apps/DBMaintainer
dotnet ef database update --project "../Database/src"
```

Ensure no errors have occurred.

Launch PGAdmin and if its the first time running it then set a master password of your choice.

Create a new configuration by:

* Right Clicking on Servers in the left pane
* Clicking on Create then Server
  * General
    * Name: HealthGateway (local)
  * Connection
    * Hostname/address: localhost
    * Maintenance database: postgres
    * Username: gateway
    * Password: passw0rd
    * Save password? checked
* Click Save

You should be able to connect to the database.  Confirm the Health Gateway tables exist by navigating to Servers/HealthGateway (local)/Databases (2)/gateway/Schemas/gateway/Tables  

if successful close PGAdmin.

## Configure the Application

If using VS Code open the workspace file located at $GATEWAYHOME/Apps/HealthGateway.code-workspace  
If using Visual Studio 2019, open the solution file located at $GATEWAYHOME/Apps/HealthGateway.sln  

Review each application in [section 3](../README.md) of the main README to configure individual applications.
