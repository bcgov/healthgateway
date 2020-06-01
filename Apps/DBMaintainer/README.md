# Database setup

## Create Migrations

`dotnet ef migrations add InitialCreate --project "../Database/src"`

## Run migrations

`dotnet ef database update --project "../Database/src"`

## Remove latest migration

`dotnet ef migrations remove --project "../Database/src"`