// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

namespace HealthGateway.IntegrationTests;

using System.Data.Common;
using System.Reflection;
using Alba;
using Alba.Security;
using DotNet.Testcontainers.Containers;
using HealthGateway.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

public class WebAppFixture : IAsyncLifetime
{
    private readonly ServiceCollection testRelatedServices = new();
    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder().WithImage("docker.io/postgres:11").Build();
    private readonly RedisContainer redisContainer = new RedisBuilder().WithImage("docker.io/redis:7.0").Build();

    public IServiceCollection Services => this.testRelatedServices;

    public virtual async Task<IAlbaHost> CreateHost<TStartup>(ITestOutputHelper output, KeyValuePair<string, string?>[]? configurationSettings = null, IEnumerable<IAlbaExtension>? extensions = null)
        where TStartup : class
    {
        IAlbaHost host = await AlbaHost.For<TStartup>(
            builder =>
            {
                builder.ConfigureServices(
                    (_, services) =>
                    {
                        // add test specific dependencies
                        foreach (ServiceDescriptor service in this.testRelatedServices)
                        {
                            services.Add(service);
                        }

                        // set logging to xunit output
                        services.AddLogging(loggingBuilder => loggingBuilder.AddXUnit(output));
                    });

                // override the db connection string
                Dictionary<string, string?> configOverrides = new(configurationSettings ?? Enumerable.Empty<KeyValuePair<string, string?>>())
                {
                    { "ConnectionStrings:GatewayConnection", this.postgreSqlContainer.GetConnectionString() },
                    { "RedisConnection", this.redisContainer.GetConnectionString() },
                };
                // set or override configuration settings
                builder.ConfigureAppConfiguration((context, configBuilder) => { configBuilder.AddInMemoryCollection(configOverrides); });
            },
            extensions?.ToArray() ?? Array.Empty<IAlbaExtension>());
        return host;
    }

    public async Task InitializeAsync()
    {
        if (this.postgreSqlContainer.State != TestcontainersStates.Running)
        {
            await this.postgreSqlContainer.StartAsync();
        }

        if (this.redisContainer.State != TestcontainersStates.Running)
        {
            await this.redisContainer.StartAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await this.postgreSqlContainer.DisposeAsync();
        await this.redisContainer.DisposeAsync();
    }
}

[CollectionDefinition("WebAppScenario")]
public class ScenarioCollection : ICollectionFixture<WebAppFixture>
{
}

[Collection("WebAppScenario")]
public abstract class ScenarioContextBase<TStartup> : IAsyncLifetime, IClassFixture<WebAppFixture>
    where TStartup : class
{
    private readonly WebAppFixture fixture;
    private readonly TestConfiguration testConfiguration;

    public IAlbaHost Host { get; private set; } = null!;

    protected ITestOutputHelper Output { get; }

    protected ScenarioContextBase(ITestOutputHelper output, string configSectionName, WebAppFixture fixture)
    {
        this.Output = output;
        this.fixture = fixture;
        string? secretsPath = Environment.GetEnvironmentVariable("SECRETS_PATH");
        IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly());
        if (!string.IsNullOrEmpty(secretsPath))
        {
            configBuilder.AddJsonFile(secretsPath);
        }

        this.testConfiguration = configBuilder.Build().GetSection(configSectionName).Get<TestConfiguration>()!;
    }

    public virtual async Task InitializeAsync()
    {
        IAlbaExtension authentication = this.CreateClientCredentials(this.testConfiguration.DefaultUserName);
        this.Host = await this.fixture.CreateHost<TStartup>(this.Output, extensions: new[] { authentication });

        using IServiceScope migrationScope = this.Host.Services.CreateScope();
        GatewayDbContext dbCtx = migrationScope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        await MigrateDatabase(dbCtx);
        await SeedData(dbCtx);
    }

    private static async Task MigrateDatabase(GatewayDbContext ctx)
    {
        await ctx.Database.MigrateAsync();
    }

    private static async Task SeedData(GatewayDbContext ctx)
    {
        DbConnection conn = ctx.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        await using DbCommand command = conn.CreateCommand();
        string seedScript = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";\n" + await File.ReadAllTextAsync("../../../../../functional/tests/cypress/db/seed.sql");
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        command.CommandText = seedScript;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await this.Host.DisposeAsync();
    }

    public TestUser GetTestUser(string userName)
    {
        TestUser? user = this.testConfiguration.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userName} not found in the test configuration");
        }

        return user;
    }

    private IAlbaExtension CreateClientCredentials(string userName)
    {
        string clientId = this.testConfiguration.ClientId;
        string clientSecret = this.testConfiguration.ClientSecret;
        TestUser user = this.GetTestUser(userName);
        OpenConnectUserPassword userAuth = new()
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            UserName = user.UserName,
            Password = user.Password,
        };

        return userAuth;
    }
}

public record TestUser(string UserName, string Password);

public record TestConfiguration
{
    public const string AdminConfigSection = "Admin";
    public const string WebClientConfigSection = "WebClient";

    public string Authority { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string DefaultUserName { get; set; } = null!;
    public IEnumerable<TestUser> Users { get; set; } = Array.Empty<TestUser>();
}
