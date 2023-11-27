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
using Alba.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

[CollectionDefinition("WebAppScenario")]
public class ScenarioCollection : ICollectionFixture<WebAppFixture>;

[Collection("WebAppScenario")]
public abstract class ScenarioContextBase<TProgram> : IAsyncLifetime
    where TProgram : class
{
    private readonly WebAppFixture fixture;
    private readonly TestConfiguration testConfiguration;
    private IServiceScope testServicesScope = null!;

    public IAlbaHost Host { get; private set; } = null!;

    public IServiceProvider TestServices => testServicesScope.ServiceProvider;

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
        this.Host = await this.fixture.CreateHost<TProgram>(this.Output, extensions: new[] { authentication });
        this.testServicesScope = Host.Services.CreateScope();
    }

    public virtual async Task DisposeAsync()
    {
        this.testServicesScope.Dispose();
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Change return type of method from 'IAlbaExtension' to 'OpenConnectUserPassword'", Justification = "Team decision")]
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
