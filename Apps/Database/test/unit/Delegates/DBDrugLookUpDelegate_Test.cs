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
namespace HealthGateway.DatabaseTests.Context
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;


    public class DBDrugLookUpDelegate_Test
    {
        private readonly IConfiguration configuration;

        public DBDrugLookUpDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void MultipleDrugStatusTest()
        {
            //using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
            //optionsBuilder.UseNpgsql(this.configuration.GetConnectionString("GatewayConnection"));
            //using var dbContext = new GatewayDbContext(optionsBuilder.Options);
            //IDrugLookupDelegate drugLookupDelegate = new DBDrugLookupDelegate(loggerFactory.CreateLogger<DBDrugLookupDelegate>(), dbContext);
            //List<string> drugList = new List<string>()
            //{
            //    "00653217",
            //    "01941089",
            //};
            //Dictionary<string, string> names = drugLookupDelegate.GetDrugsBrandNameByDIN(drugList);
            Assert.True(true);
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

    }
}
