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
namespace HealthGateway.Medication.Test
{
    using DeepEqual.Syntax;
    using HealthGateway.Medication.Database;
    using HealthGateway.Medication.Models;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using Xunit;


    public class MedicationDbContext_Test
    {
        delegate void ExecuteSqlCommandDelegate(string sql, object[] parameters);
        [Fact]
        public void ShouldGetNextSequence()
        {
            Mock<IDbContext> dbMock = new Mock<IDbContext>();
            long expected = 1234L;
            dbMock
                .Setup(s => s.ExecuteSqlCommand(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback(new ExecuteSqlCommandDelegate((sql, parameters) => { 
                    ((NpgsqlParameter)parameters[0]).Value = expected;
                }));
            IMedicationDBContextExt ctx = new MedicationDBContextExt();
            long actual = ctx.NextValueForSequence(dbMock.Object, "test");

            Assert.Equal(expected, actual);
        }
    }
}
