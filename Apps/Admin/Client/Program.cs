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
    using System.Reflection;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using Fluxor;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Services;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using MudBlazor;
    using MudBlazor.Services;
    using Refit;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    public static class Program
    {
        /// <summary>
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>A task which represents the exit of the application.</returns>
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Register Refit Clients
            RegisterRefitClients(builder);

            // Configure Logging
            IConfigurationSection loggerConfig = builder.Configuration.GetSection("Logging");
            builder.Services
                .AddLogging(loggingBuilder => loggingBuilder.AddConfiguration(loggerConfig));

            // Enable Mud Blazor component services
            builder.Services.AddMudServices(config => config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight);

            WebAssemblyHost[] app = new WebAssemblyHost[1];

            // Configure Authentication and Authorization
            builder.Services.AddOidcAuthentication(
                    options =>
                    {
                        NavigationManager navigationManager = app[0].Services.GetRequiredService<NavigationManager>();
                        Uri uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

                        // If there is an authProvider query string value then set the hint.
                        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("authProvider", out StringValues authProvider))
                        {
                            options.ProviderOptions.AdditionalProviderParameters.Add("kc_idp_hint", authProvider.ToString());
                        }

                        builder.Configuration.Bind("Oidc", options.ProviderOptions);
                        options.ProviderOptions.ResponseType = "code";
                        options.UserOptions.RoleClaim = "role";
                    })
                .AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();

            // Configure State Management
            Assembly currentAssembly = typeof(Program).Assembly;
            builder.Services.AddFluxor(
                options => options
                    .ScanAssemblies(currentAssembly)
                    .UseReduxDevTools(rdt => rdt.Name = "Health Gateway Admin"));

            builder.Services.AddBlazoredLocalStorage();

            app[0] = builder.Build();
            await app[0].RunAsync().ConfigureAwait(true);
        }

        private static void RegisterRefitClients(this WebAssemblyHostBuilder builder)
        {
            RegisterRefitClient<IAnalyticsApi>(builder, "v1/api/CsvExport", true);
            RegisterRefitClient<IBroadcastsApi>(builder, "v1/api/Broadcast", true);
            RegisterRefitClient<ICommunicationsApi>(builder, "v1/api/Communication", true);
            RegisterRefitClient<IConfigurationApi>(builder, "v1/api/Configuration", false);
            RegisterRefitClient<IDashboardApi>(builder, "v1/api/Dashboard", true);
            RegisterRefitClient<ISupportApi>(builder, "v1/api/Support", true);
            RegisterRefitClient<ITagApi>(builder, "v1/api/Tag", true);
            RegisterRefitClient<IUserFeedbackApi>(builder, "v1/api/UserFeedback", true);
        }

        private static void RegisterRefitClient<T>(WebAssemblyHostBuilder builder, string servicePath, bool isAuthorized)
            where T : class
        {
            Uri baseAddress = new(builder.HostEnvironment.BaseAddress);
            builder.Services.AddTransient(_ => new HttpClient { BaseAddress = baseAddress });

            Uri address = new(baseAddress, servicePath);

            if (isAuthorized)
            {
                builder.Services.AddRefitClient<T>()
                    .ConfigureHttpClient(c => c.BaseAddress = address)
                    .AddHttpMessageHandler(sp => ConfigureAuthorization(sp, address.AbsoluteUri));
                return;
            }

            builder.Services.AddRefitClient<T>()
                .ConfigureHttpClient(c => c.BaseAddress = address);
        }

        private static DelegatingHandler ConfigureAuthorization(IServiceProvider serviceProvider, string address)
        {
            return serviceProvider.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(new[] { address });
        }
    }
}
