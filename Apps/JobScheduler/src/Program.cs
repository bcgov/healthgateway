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
namespace HealthGateway.JobScheduler
{
    using HealthGateway.Common.AspNetConfiguration;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// The program startup class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Default entry point.
        /// </summary>
        /// <param name="args">Inbound parms.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Initializes the Host Builder.
        /// </summary>
        /// <param name="args">Inbound parms to use.</param>
        /// <returns>The Host BUilder object.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            ProgramConfiguration.CreateWebHostBuilder<Startup>(args);
    }
}
