#FROM microsoft/dotnet:sdk AS build-env
FROM microsoft/aspnetcore-build AS builder
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
#FROM microsoft/dotnet:aspnetcore-runtime
FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "WebClient.dll"]