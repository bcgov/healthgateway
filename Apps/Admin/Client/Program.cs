//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Client
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using Fluxor;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Services;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using MudBlazor.Services;
    using Refit;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    public static class Program
    {
        /// <summary>.
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>A task which represents the exit of the application.</returns>
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Configure HTTP Services
            string baseAddress = builder.HostEnvironment.BaseAddress;

            // Register Refit Clients
            RegisterRefitClients(builder, baseAddress);

            // Configure Logging
            IConfigurationSection loggerConfig = builder.Configuration.GetSection("Logging");
            builder.Services
                        .AddLogging(builder =>
                        {
                            builder.AddConfiguration(loggerConfig);
                        });

            // Enable Mud Blazor component services
            builder.Services.AddMudServices();

            // Configure Authentication and Authorization
            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Oidc", options.ProviderOptions);
                options.ProviderOptions.ResponseType = "code";
                options.UserOptions.RoleClaim = "role";
            }).AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();

            // Configure State Management
            var currentAssembly = typeof(Program).Assembly;
            builder.Services.AddFluxor(options => options
                                    .ScanAssemblies(currentAssembly)
                                    .UseReduxDevTools(rdt =>
                                    {
                                        rdt.Name = "Health Gateway Admin";
                                    }));

            // Register all the state facade
            AddStateFacadeScope(builder);

            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync().ConfigureAwait(true);
        }

        private static void RegisterRefitClients(WebAssemblyHostBuilder builder, string baseAddress)
        {
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

            string configAddress = GetConfigAddress(builder, "Configuration", baseAddress);
            builder.Services.AddRefitClient<IConfigurationApi>()
                      .ConfigureHttpClient(c => ConfigureHttpClient(c, configAddress));

            string supportAddress = GetConfigAddress(builder, "Support", baseAddress);
            builder.Services.AddRefitClient<ISupportApi>()
                           .ConfigureHttpClient(c => ConfigureHttpClient(c, supportAddress))
                           .AddHttpMessageHandler(sp => ConfigureAuthorization(sp, supportAddress));

            string exportCsvAddress = GetConfigAddress(builder, "CsvExport", baseAddress);
            builder.Services.AddRefitClient<ICsvExportApi>()
                    .ConfigureHttpClient(c => ConfigureHttpClient(c, exportCsvAddress))
                    .AddHttpMessageHandler(sp => ConfigureAuthorization(sp, exportCsvAddress));
        }

        private static void ConfigureHttpClient(HttpClient client, string address)
        {
            client.BaseAddress = new Uri(address);
        }

        private static string GetConfigAddress(WebAssemblyHostBuilder builder, string key, string baseAddress)
        {
            return builder.Configuration.GetSection("Services").GetValue<string>(key, baseAddress);
        }

        private static DelegatingHandler ConfigureAuthorization(IServiceProvider serviceProvider, string configAddress)
        {
            return serviceProvider.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(new[] { configAddress });
        }

        private static void AddStateFacadeScope(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<Admin.Client.Store.Configuration.StateFacade>();
            builder.Services.AddScoped<Admin.Client.Store.MessageVerification.StateFacade>();
        }
    }
}
