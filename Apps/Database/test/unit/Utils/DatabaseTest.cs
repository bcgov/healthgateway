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
namespace HealthGateway.DatabaseTests.Utils
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Npgsql;
    using Xunit;

    /// <summary>
    /// Abstract class to perform a set of DB Tests.
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "IAsyncLifetime takes care of it")]
    public abstract class DatabaseTest : IAsyncLifetime
    {
        private readonly TestcontainerDatabase dbContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(
                new PostgreSqlTestcontainerConfiguration
                {
                    Database = "gateway",
                    Username = "gateway",
                    Password = "gateway",
                })
            .Build();

        /// <summary>
        /// Gets or sets the health gateway DB Context.
        /// </summary>
        protected GatewayDbContext? DbContext { get; set; }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            await this.dbContainer.StartAsync().ConfigureAwait(true);
            DbContextOptions<GatewayDbContext> options = new DbContextOptionsBuilder<GatewayDbContext>()
                .UseNpgsql(this.dbContainer.ConnectionString)
                .Options;
            this.DbContext = new GatewayDbContext(options);
            await this.DbContext.Database.MigrateAsync().ConfigureAwait(true);
            await using NpgsqlConnection conn = new(this.dbContainer.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(true);
            await using NpgsqlCommand command = new();
            command.Connection = conn;
            command.CommandText = this.SeedSql();
            await command.ExecuteReaderAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            if (this.DbContext != null)
            {
                await this.DbContext.DisposeAsync().ConfigureAwait(true);
            }

            await this.dbContainer.DisposeAsync().AsTask().ConfigureAwait(true);
        }

        /// <summary>
        /// Reads a resource file into a string.
        /// </summary>
        /// <param name="resource">
        /// The fully qualified resource to read ie
        /// "HealthGateway.Database.Assets.Legal.TermsOfService.txt".
        /// </param>
        /// <returns>The contents of the file read.</returns>
        protected static string ReadSeedData(string resource)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(DatabaseTest));
            Stream? resourceStream = assembly!.GetManifestResourceStream(resource);
            using StreamReader reader = new(resourceStream!, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// SQL script that will be executed on initialization.
        /// </summary>
        /// <returns>The SQL to run on the DB prior to test execution.</returns>
        protected abstract string SeedSql();
    }
}
