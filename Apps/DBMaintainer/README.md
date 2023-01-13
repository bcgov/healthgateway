# DBMaintainer

The DBMaintainer project provides CLI tools to load Federal and Provincial drug files.  
The project also is used to perform EF migration tasks from the command line.

## Database utilities

dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

### Create Migrations

`dotnet ef migrations add InitialCreate --project "../Database/src"`

### Run migrations

`dotnet ef database update --project "../Database/src"`

### Revert migrations

`dotnet ef database update [MigrationName] --project "../Database/src"`

### Remove latest migration

`dotnet ef migrations remove --project "../Database/src"`

### Generate Production scripts

`dotnet ef migrations script --idempotent --project "../Database/src" --output db.sql`
