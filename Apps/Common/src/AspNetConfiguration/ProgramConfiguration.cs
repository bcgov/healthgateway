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
namespace HealthGateway.Common.AspNetConfiguration
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    /// <summary>
    /// The program configuration class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProgramConfiguration
    {
        private const string EnvironmentPrefix = "HealthGateway_";

        /// <summary>
        /// Creates a IHostBuilder with console logging and Configuration prefixing enabled.
        /// </summary>
        /// <typeparam name="T">The startup class.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Returns the configured WebHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder<T>(string[] args)
            where T : class
        {
            return Host.CreateDefaultBuilder(args)
                .UseDefaultLogging()
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config.AddJsonFile("appsettings.local.json", true, true); // Loads local settings last to keep override
                        config.AddEnvironmentVariables(prefix: EnvironmentPrefix);
                    })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<T>());
        }

        /// <summary>
        /// Creates a WebApplicationBuilder with configuration set and open telemetry.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Returns the configured WebApplicationBuilder.</returns>
        public static WebApplicationBuilder CreateWebAppBuilder(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.UseDefaultLogging();

            // Additional configuration sources
            builder.Configuration.AddJsonFile("appsettings.local.json", true, true);
            builder.Configuration.AddEnvironmentVariables(prefix: EnvironmentPrefix);
            return builder;
        }

        /// <summary>
        /// Create an initial logger to use during Program startup.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>An instance of a logger.</returns>
        public static ILogger GetInitialLogger(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateBootstrapLogger();

            using SerilogLoggerFactory factory = new(Log.Logger);
            return factory.CreateLogger("Startup");
        }
    }
}
