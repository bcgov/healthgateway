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
namespace HealthGateway.DrugMaintainer.Database
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    /// <summary>
    /// The database context to be used for the Medication Service.
    /// </summary>
    public class MigrationDBFactory : IDesignTimeDbContextFactory<MigrationDBContext>
    {
        public MigrationDBFactory()
        {
        }

        public MigrationDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MigrationDBContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=gateway;User ID=gateway;Password=passw0rd;Integrated Security=true;Pooling=true;");
            return new MigrationDBContext(optionsBuilder.Options);
        }
    }
}
