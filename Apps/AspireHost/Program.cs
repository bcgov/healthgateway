using System.Globalization;
using Aspire.Hosting.JavaScript;
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
// Aspire 13.5 enables TLS on the Redis endpoint by default when a trusted ASP.NET developer
// certificate exists. The apps connect with the plaintext RedisConnection user secret, so opt out.
// WithoutHttpsCertificate is still marked experimental (ASPIRECERTIFICATES001).
#pragma warning disable ASPIRECERTIFICATES001
IResourceBuilder<RedisResource> redis = builder.AddRedis("GatewayCache", port: redisPort)
    .WithoutHttpsCertificate()
    .WithPassword(redisPasswordValue is null ? null : builder.AddParameter("redis-password", redisPasswordValue, secret: true))
    .WithImageTag("6.2")
    .WithContainerName("GatewayCache")
    .WithDataVolume("gatewaycache.local")
    .WithPersistence(TimeSpan.FromSeconds(60));
#pragma warning restore ASPIRECERTIFICATES001

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
IResourceBuilder<ProjectResource> webClient = AddApp<Projects.WebClient>("WebClient");
AddApp<Projects.Admin_Server>("Admin");

// Metro, the Expo dev server for the ClientApp (npm run dev = expo start --port 5002). The simulator's dev build and the
// web app (http://localhost:5002) load from it, so each start serves the latest code and app.config.ts, with hot reload.
// It starts straight away, without waiting for the database, so it holds 5002 before WebClient's SpaProxy would start
// its own copy. CI=false: Expo turns reloading off when CI is set.
IResourceBuilder<JavaScriptAppResource> clientApp = builder.AddJavaScriptApp("ClientApp", "../WebClient/src/ClientApp", "dev")
    .WithNpm(install: false)
    .WithHttpEndpoint(port: 5002, isProxied: false)
    .WithEnvironment("CI", "false");

// The dev app on an emulator or simulator, started from the dashboard (Start), so a run without mobile work starts
// none. HG_MOBILE_PLATFORM: android (default) or ios (macOS only). HG_MOBILE_DEVICE: the AVD or simulator by name, e.g.
// Pixel_10_Pro_XL or "iPhone 18 Pro Max" (without one: a running one, else the first). Both come from the AppHost's
// configuration: environment variables, user secrets or appsettings. One that isn't running is started, and shut down
// when Aspire stops. See
// Apps/WebClient/src/ClientApp/scripts/mobile-dev.js.
string mobilePlatform = builder.Configuration["HG_MOBILE_PLATFORM"] ?? "android";
string mobileDevice = builder.Configuration["HG_MOBILE_DEVICE"] ?? string.Empty;
// An emulator or simulator the script starts is shut down when this host exits (Aspire stops).
string aspirePid = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);

// MobileApp opens the installed dev app on Metro; it waits for WebClient, whose /configuration the app reads first.
builder.AddExecutable("MobileApp", "node", "../WebClient/src/ClientApp", "scripts/mobile-dev.js", "open")
    .WithEnvironment("HG_MOBILE_PLATFORM", mobilePlatform)
    .WithEnvironment("HG_MOBILE_DEVICE", mobileDevice)
    .WithEnvironment("HG_ASPIRE_PID", aspirePid)
    .WithExplicitStart()
    .WaitFor(clientApp)
    .WaitFor(webClient);

// MobileBuild builds and installs the dev app (needed once per emulator or simulator, then after native changes: a
// native package, a plugin, native fields in app.config.ts), then opens it. Android needs JAVA_HOME for Gradle.
builder.AddExecutable("MobileBuild", "node", "../WebClient/src/ClientApp", "scripts/mobile-dev.js", "build")
    .WithEnvironment("HG_MOBILE_PLATFORM", mobilePlatform)
    .WithEnvironment("HG_MOBILE_DEVICE", mobileDevice)
    .WithEnvironment("HG_ASPIRE_PID", aspirePid)
    .WithExplicitStart()
    .WaitFor(clientApp);

await builder.Build().RunAsync();

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
