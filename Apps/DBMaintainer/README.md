# Datbase setup

## Create Migrations

`dotnet ef migrations add InitialCreate --project "../Common/src" -c DrugDBContext`

## Run migrations

`dotnet ef database update --project "../Common/src" -c DrugDBContext`
