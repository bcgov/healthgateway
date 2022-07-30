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
namespace HealthGateway.DatabaseTests.Fixtures
{
    using System;
    using System.IO;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Helper methods to support fixture creation.
    /// </summary>
    public static class Fixture
    {
        private const string UserSecret = "84e2fe9a-a1f5-4de7-bef6-4518a33fa8b9";

        /// <summary>
        /// Creates DB Context for Gateway Database.
        /// </summary>
        /// <returns>An instance of GatewayDbContext.</returns>
        public static GatewayDbContext CreateContext()
        {
            return new(
                new DbContextOptionsBuilder<GatewayDbContext>()
                    .UseNpgsql(GetConnectionString())
                    .Options);
        }

        /// <summary>
        /// Gets database connection string from settings.
        /// </summary>
        /// <returns>A database connection string.</returns>
        public static string? GetConnectionString()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine($"Directory: {Directory.GetCurrentDirectory()}");
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddUserSecrets(UserSecret)
                .Build();
            string? connectionString = config.GetConnectionString("GatewayConnection");
            Console.WriteLine($"Connection String: {connectionString}");
            return connectionString;
        }
    }
}
