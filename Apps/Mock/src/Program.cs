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
    using System.Security.Cryptography.X509Certificates;
    using HealthGateway.Common.Models.ODR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private const string EnvironmentPrefix = "HealthGateway_";

        /// <summary>
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the IWebHostBuilder.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>Returns the configured webhost.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return CreateHostBuilder<Startup>(args);
        }

        private static IHostBuilder CreateHostBuilder<T>(string[] args)
            where T : class
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSimpleConsole(
                            options =>
                            {
                                options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss]";
                                options.IncludeScopes = true;
                            });
                        logging.AddOpenTelemetry();
                    })
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config.AddUserSecrets(typeof(Program).Assembly);
                        config.AddJsonFile("appsettings.local.json", true, true); // Loads local settings last to keep override
                        config.AddEnvironmentVariables(prefix: EnvironmentPrefix);
                    })
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<T>();
                        webBuilder.ConfigureKestrel(
                            options =>
                            {
                                IConfiguration configuration = options.ApplicationServices.GetService<IConfiguration>()!;
                                options.ConfigureHttpsDefaults(
                                    configureOptions =>
                                    {
                                        OdrConfig odrConfig = new();
                                        configuration.Bind(OdrConfig.OdrConfigSectionKey, odrConfig);
                                        OdrCertificateConfig? certConfig = odrConfig.ServerCertificate;

                                        if (certConfig?.Enabled is true)
                                        {
                                            configureOptions.ServerCertificate = new X509Certificate2(certConfig.Path, certConfig.Password);
                                            configureOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                        }
                                    });
                            });
                    });
        }
    }
}
