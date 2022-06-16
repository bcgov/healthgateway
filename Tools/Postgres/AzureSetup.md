# Azure DB Setup

## Create the HG Database

Using PG Admin, connect to the Azure DB using the Azure DB Admin credentials.

### Create the gateway role

```console
CREATE ROLE gateway WITH
  LOGIN
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  PASSWORD 'THE PASSWORD';
```

### Grant gateway access to Azure DB Admin User

```console
grant gateway to "Azure DB Admin UserId";
```

### Create the database

```console
CREATE DATABASE gateway WITH
  OWNER = gateway
  ENCODING = 'UTF8'
  LC_COLLATE = 'en_US.UTF-8'
  LC_CTYPE = 'en_US.UTF-8'
  CONNECTION LIMIT = -1
  TEMPLATE template0;
```

### Add requires extensions

If this command errors, it means that we need to ask the Azure Admin to add the extension in the server configuration.

```console
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

## Initial Data Migrations

After successfully creating the DB, disconnect from the database and re-connect using the gateway credentials and setting the maintenance database to gateway.

### Run Migrations

Open a query tool on the gateway db and run the [01_init-to-AddAllLaboratoryOrdersCommentEntryType.sql](/sql/01_init-to-AddAllLaboratoryOrdersCommentEntryType.sql) script.  You will see one warning

```console
NOTICE:  trigger "PushBannerChange" for relation "gateway.Communication" does not exist, skipping
```

The database is now ready for application configuration.  The JobScheduler should be run to update the database to match the current deployment.
