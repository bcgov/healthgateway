-- Runs once when the Postgres data volume is first initialized, against the POSTGRES_DB
-- database (hglocal). Mirrors Tools/Dev/Postgres/init/00_SetupDevDB.sql: provides
-- uuid_generate_v4(), which the audit trigger functions in the EF migrations depend on.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
