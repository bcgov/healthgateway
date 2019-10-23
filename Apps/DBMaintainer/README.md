# Datbase setup

## Create Migrations

`dotnet ef migrations add InitialCreate --project "../Common/src" --output-dir "../../DBMaintainer/Migrations" -c DrugDBContext`
`dotnet ef migrations add InitialCreate --project "../Common/src" -c DrugDBContext`

## Run migrations

`dotnet ef database update --project "../Common/src" -c DrugDBContext`
