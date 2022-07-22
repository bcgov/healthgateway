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
namespace HealthGateway.GatewayApi
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.AspNetConfiguration;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
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
            return ProgramConfiguration.CreateHostBuilder<Startup>(args);
        }
    }
}
