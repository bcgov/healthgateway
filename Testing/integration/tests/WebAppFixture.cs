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

using System.Data;
using System.Data.Common;
using Alba;
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
    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder().WithImage("docker.io/postgres:15").Build();
    private readonly RedisContainer redisContainer = new RedisBuilder().WithImage("docker.io/redis:7.0").Build();

    public IServiceCollection Services => this.testRelatedServices;

    public virtual async Task<IAlbaHost> CreateHost<TProgram>(ITestOutputHelper output, KeyValuePair<string, string?>[]? configurationSettings = null, IEnumerable<IAlbaExtension>? extensions = null)
        where TProgram : class
    {
        IAlbaHost host = await AlbaHost.For<TProgram>(
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
                builder.ConfigureAppConfiguration(
                    (_, configBuilder) =>
                    {
                        string? secretsPath = Environment.GetEnvironmentVariable("SECRETS_PATH");
                        configBuilder.AddInMemoryCollection(configOverrides);
                        if (!string.IsNullOrEmpty(secretsPath))
                        {
                            configBuilder.AddJsonFile(secretsPath);
                        }
                    });
            },
            extensions?.ToArray() ?? Array.Empty<IAlbaExtension>());
        return host;
    }

    public async Task InitializeAsync()
    {
        if (this.postgreSqlContainer.State != TestcontainersStates.Running)
        {
            await this.postgreSqlContainer.StartAsync();

            DbContextOptionsBuilder<GatewayDbContext> builder = new();
            builder.UseNpgsql(this.postgreSqlContainer.GetConnectionString(), optionsBuilder => optionsBuilder.MigrationsHistoryTable("__EFMigrationsHistory", "gateway"));
            using GatewayDbContext dbCtx = new(builder.Options);
            await MigrateDatabase(dbCtx);
            await SeedData(dbCtx);
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

    private static async Task MigrateDatabase(GatewayDbContext ctx)
    {
        await ctx.Database.MigrateAsync();
    }

    private static async Task SeedData(GatewayDbContext ctx)
    {
        DbConnection conn = ctx.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
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
}
