# Azure DB Setup

## Create the HG Database

Using PG Admin, connect to the Azure DB using the Azure DB Admin credentials.

### Create the gateway role

```console
CREATE ROLE [ROLE NAME] WITH
  LOGIN
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  PASSWORD '[THE PASSWORD]';
```

### Grant gateway access to Azure DB Admin User

```console
grant [ROLE NAME] to "Azure DB Admin UserId";
```

### Create the database

```console
CREATE DATABASE [DB NAME] WITH
  OWNER = [ROLE NAME]
  ENCODING = 'UTF8'
  LC_COLLATE = 'en_US.UTF-8'
  LC_CTYPE = 'en_US.UTF-8'
  CONNECTION LIMIT = -1
  TEMPLATE template0;

REVOKE connect ON DATABASE [DB NAME] FROM PUBLIC;
```

### Add requires extensions

If this command errors, it means that we need to ask the Azure Admin to add the extension in the server configuration.

```console
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

## Initial Data Migrations

JobScheduler will need to connect to the DB and run in order to create the DB tables and schemas.

