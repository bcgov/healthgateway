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
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The program configuration class.
    /// </summary>
    public static class ProgramConfiguration
    {
        private const string EnvironmentPrefix = "HealthGateway_";

        /// <summary>
        /// Builds the webhost object with console logging and Configuration prefixing enabled.
        /// </summary>
        /// <typeparam name="T">The startup class.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Returns the configured webhost.</returns>
        [Obsolete("Invoke static CreateWebHostBuilder - See Medication Program.cs")]
        public static IWebHost BuildWebHost<T>(string[] args)
            where T : class
        {
            return CreateWebHostBuilder<T>(args).Build();
        }

        /// <summary>
        /// Creates a WebHostBuild with console logging and Configuration prefixing enabled.
        /// </summary>
        /// <typeparam name="T">The startup class.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Returns the configured WebHostBuilder</returns>
        public static IWebHostBuilder CreateWebHostBuilder<T>(string[] args)
             where T : class
        {
            return WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddEnvironmentVariables(prefix: EnvironmentPrefix);
            })
            .UseStartup<T>()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
        }
    }
}
