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

using System.Reflection;
using Alba;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"

public class WebAppFixture
{
    private readonly ServiceCollection services = new ServiceCollection();

    public IServiceCollection Services => this.services;

    public async Task<IAlbaHost> CreateHost<TStartup>(ITestOutputHelper output, IConfiguration configuration)
        where TStartup : class
    {
        IAlbaHost host = await AlbaHost.For<TStartup>(
            builder =>
            {
                builder.ConfigureServices((ctx, services) =>
                {
                    foreach (var service in this.services)
                    {
                        services.Add(service);
                    }
                    services.AddLogging(builder => builder.AddXUnit(output));
                });
            });

        return host;
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
    private readonly IConfiguration configuration;

    protected IAlbaHost Host { get; private set; } = null!;

    protected ITestOutputHelper Output { get; }

    protected ScenarioContextBase(ITestOutputHelper output, WebAppFixture fixture)
    {
        this.Output = output;
        this.fixture = fixture;
        this.configuration = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), false).Build();
    }

    public virtual async Task InitializeAsync()
    {
        this.Host = await this.fixture.CreateHost<TStartup>(this.Output, this.configuration);
    }

    public async Task DisposeAsync()
    {
        if (this.Host != null)
        {
            await this.Host.DisposeAsync();
        }
    }
}
