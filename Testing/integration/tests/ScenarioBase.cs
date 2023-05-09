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
    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder().Build();
    private readonly RedisContainer redisContainer = new RedisBuilder().WithImage("docker.io/redis:7.0").Build();

    public IServiceCollection Services => this.testRelatedServices;

    public virtual async Task<IAlbaHost> CreateHost<TStartup>(ITestOutputHelper output, params KeyValuePair<string, string?>[] configurationSettings)
        where TStartup : class
    {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
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
                var configOverrides = new Dictionary<string, string?>(configurationSettings)
                {
                    { "ConnectionStrings:GatewayConnection", this.postgreSqlContainer.GetConnectionString() },
                    { "RedisConnection", this.redisContainer.GetConnectionString() },
                };
                // set or override configuration settings
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(configOverrides);
                });
            });
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

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

    protected IAlbaHost Host { get; private set; } = null!;

    protected ITestOutputHelper Output { get; }

    protected ScenarioContextBase(ITestOutputHelper output, WebAppFixture fixture)
    {
        this.Output = output;
        this.fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        this.Host = await this.fixture.CreateHost<TStartup>(this.Output);

        using var initScope = this.Host.Services.CreateScope();
        await MigrateDatabase(initScope.ServiceProvider.GetRequiredService<GatewayDbContext>());
    }

    private static async Task MigrateDatabase(GatewayDbContext ctx)
    {
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await this.Host.DisposeAsync();
    }
}
