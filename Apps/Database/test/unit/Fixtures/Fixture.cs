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
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using Respawn;
    using Respawn.Graph;

    /// <summary>
    /// Helper methods to support fixture creation.
    /// </summary>
    public static class Fixture
    {
        private const string ConnectionStringKey = "GatewayConnection";
        private const string UserSecret = "84e2fe9a-a1f5-4de7-bef6-4518a33fa8b9";

        /// <summary>
        /// Deletes all data including dependent data from tables specified in parameter.
        /// </summary>
        /// <param name="tablesToInclude">An array of Tables to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<string?> ResetDatabase(Table[] tablesToInclude)
        {
            using NpgsqlConnection connection = new(GetConnectionString());

            await connection.OpenAsync().ConfigureAwait(true);
            Checkpoint checkpoint = new()
            {
                TablesToInclude = tablesToInclude,
                SchemasToInclude = new[]
                {
                    "gateway",
                },
                DbAdapter = DbAdapter.Postgres,
            };

            await checkpoint.Reset(connection).ConfigureAwait(true);
            return checkpoint.DeleteSql;
        }

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
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddUserSecrets(UserSecret)
                .Build();
            return config.GetConnectionString(ConnectionStringKey);
        }
    }
}
