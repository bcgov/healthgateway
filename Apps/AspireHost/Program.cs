using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Same Serilog defaults as the apps (see Observability.UseDefaultLogging), minus the
// enrichers that only apply to HTTP request pipelines.
builder.Services.AddSerilog(config => config
    .Enrich.WithMachineName()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AspireHost")
    .Enrich.WithEnvironmentName()
    .ReadFrom.Configuration(builder.Configuration));

// Connection strings come from the user secrets shared with the apps (UserSecretsId in the csproj).
string gatewayConnection = builder.Configuration.GetConnectionString("GatewayConnection")
    ?? throw new InvalidOperationException("The ConnectionStrings:GatewayConnection user secret is required.");
string redisConnection = builder.Configuration["RedisConnection"]
    ?? throw new InvalidOperationException("The RedisConnection user secret is required.");

NpgsqlConnectionStringBuilder gatewayDb = new(gatewayConnection);

IResourceBuilder<ParameterResource> postgresUser = builder.AddParameter("postgres-user", gatewayDb.Username!);
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("postgres-password", gatewayDb.Password!, secret: true);
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("GatewayDb", userName: postgresUser, password: postgresPassword, port: gatewayDb.Port)
    .WithImageTag("15")
    .WithContainerName("GatewayDB")
    .WithArgs("-c", "max_connections=500")
    .WithEnvironment("POSTGRES_DB", gatewayDb.Database)
    .WithInitFiles("postgres-init")
    .WithDataVolume("gatewaydb.local");

string redisEndpoint = redisConnection.Split(',')[0];
int redisPort = redisEndpoint.Contains(':', StringComparison.Ordinal) ? int.Parse(redisEndpoint.Split(':')[^1], CultureInfo.InvariantCulture) : 6379;
string? redisPasswordValue = redisConnection.Split(',')
    .Select(part => part.Trim())
    .FirstOrDefault(part => part.StartsWith("password=", StringComparison.OrdinalIgnoreCase))?["password=".Length..];
IResourceBuilder<RedisResource> redis = builder.AddRedis("GatewayCache", port: redisPort)
    .WithPassword(redisPasswordValue is null ? null : builder.AddParameter("redis-password", redisPasswordValue, secret: true))
    .WithImageTag("6.2")
    .WithContainerName("GatewayCache")
    .WithDataVolume("gatewaycache.local")
    .WithPersistence(TimeSpan.FromSeconds(60));

// Secret parameters so the dashboard's Environment view masks the injected values.
IResourceBuilder<ParameterResource> gatewayConnectionParam = builder.AddParameter("gateway-connection", gatewayConnection, secret: true);
IResourceBuilder<ParameterResource> redisConnectionParam = builder.AddParameter("redis-connection", redisConnection, secret: true);

// Requires the dotnet-ef global tool. The connection string is not passed on the command
// line; DBMaintainer resolves GatewayConnection from the same shared user secrets.
IResourceBuilder<ExecutableResource> migrations = builder
    .AddExecutable("DbMigrations", "dotnet", "../DBMaintainer")
    .WithArgs("ef", "database", "update", "--project", "../Database/src")
    .WaitFor(postgres);

AddApp<Projects.Patient>("Patient");
AddApp<Projects.GatewayApi>("GatewayApi");
AddApp<Projects.Immunization>("Immunization");
AddApp<Projects.Medication>("Medication");
AddApp<Projects.Laboratory>("Laboratory");
AddApp<Projects.Encounter>("Encounter");
AddApp<Projects.ClinicalDocument>("ClinicalDocument");
AddApp<Projects.JobScheduler>("JobScheduler");
AddApp<Projects.WebClient>("WebClient");
AddApp<Projects.Admin_Server>("Admin");

builder.Build().Run();

IResourceBuilder<ProjectResource> AddApp<TProject>(string name)
    where TProject : IProjectMetadata, new()
{
    // HealthGateway_-prefixed environment variables are loaded last by ProgramConfiguration,
    // so these override user secrets and appsettings.local.json in every app.
    return builder.AddProject<TProject>(name)
        .WithEnvironment("HealthGateway_ConnectionStrings__GatewayConnection", gatewayConnectionParam)
        .WithEnvironment("HealthGateway_RedisConnection", redisConnectionParam)
        .WaitFor(postgres)
        .WaitFor(redis)
        .WaitForCompletion(migrations);
}
