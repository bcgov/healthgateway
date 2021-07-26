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
namespace HealthGateway.Mock
{
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Dispatcher;
    using CoreWCF;
    using CoreWCF.Configuration;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Services;
    using HealthGateway.Mock.SOAP.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceReference;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureAccessControl(services);

            services.AddServiceModelServices();

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddTransient<IClientMessageInspector, LoggingMessageInspector>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseRest(app);

            app.UseServiceModel(builder =>
            {
                /*WSHttpBinding GetTransportWithMessageCredentialBinding()
                {
                    var serverBindingHttpsUserPassword = new WSHttpBinding(SecurityMode.TransportWithMessageCredential);
                    serverBindingHttpsUserPassword.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                    return serverBindingHttpsUserPassword;
                }*/

                //builder.ConfigureServiceHostBase<Controllers.ClientRegistriesController>(CustomUserNamePasswordValidatorCore.AddToHost);


                void ConfigureSoapService<TService, TContract>(string serviceprefix) where TService : class
                {
                    //Settings settings = new Settings().SetDefaults("localhost", serviceprefix);
                    builder.AddService<TService>()
                        .AddServiceEndpoint<TService, TContract>(new BasicHttpBinding(), "http://localhost:7000/v1/api/ClientRegistries/EchoWithGet");

                    /*.AddServiceEndpoint<TService, TContract>(new WSHttpBinding(SecurityMode.None),
                        settings.wsHttpAddress.LocalPath)
                    .AddServiceEndpoint<TService, TContract>(new WSHttpBinding(SecurityMode.Transport),
                        settings.wsHttpsAddress.LocalPath)
                    .AddServiceEndpoint<TService, TContract>(new NetTcpBinding(),
                        settings.netTcpAddress.LocalPath);*/
                    //Settings settings = new Settings().SetDefaults("localhost", serviceprefix);
                    /*builder.AddService<TService>()
                        .AddServiceEndpoint<TService, TContract>(
                            GetTransportWithMessageCredentialBinding(), settings.wsHttpAddressValidateUserPassword.LocalPath)
                        .AddServiceEndpoint<TService, TContract>(new BasicHttpBinding(),
                            settings.basicHttpAddress.LocalPath);
                        .AddServiceEndpoint<TService, TContract>(new WSHttpBinding(SecurityMode.None),
                            settings.wsHttpAddress.LocalPath)
                        .AddServiceEndpoint<TService, TContract>(new WSHttpBinding(SecurityMode.Transport),
                            settings.wsHttpsAddress.LocalPath)
                        .AddServiceEndpoint<TService, TContract>(new NetTcpBinding(),
                            settings.netTcpAddress.LocalPath);*/
                }

                ConfigureSoapService<ClientRegistries, QUPA_AR101102_PortType>(nameof(ClientRegistries));
            });
        }
    }
}
