
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
namespace HealthGateway.Medication.Delegates
{
    using HealthGateway.Common.Database;
    using HealthGateway.Medication.Database;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Entity framework baed implementation of the sequence delegate.
    /// </summary>
    public class EntitySequenceDelegate : ISequenceDelegate
    {
        private IDBContextFactory dbContextFactory;

        /// <summary>
        /// Constructor that uses the dependency injection interfaces.
        /// </summary>
        /// <param name="dbContextFactory">The context factory to be used when accessing the databaase context.</param>
        public EntitySequenceDelegate(IDBContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Gets the next sequence number for the given sequence name.
        /// </summary>
        /// <param name="sequenceName">The sequence name</param>
        /// <returns>The next sequence value</returns>
        public long NextValueForSequence(string sequenceName)
        {
            using (MedicationDBContext ctx = (MedicationDBContext)this.dbContextFactory.CreateContext())
            {
                NpgsqlParameter result = new NpgsqlParameter("@result", NpgsqlDbType.Integer)
                {
                    Direction = System.Data.ParameterDirection.Output,
                };
                ctx.ExecuteSqlCommand($"SELECT nextval('{sequenceName}')", result);

                // code below is to be used when updating to EF 3
                // ctx.Database.ExecuteSqlRaw($"SELECT nextval('{seq}')", result);
                return (long)result.Value;
            }
        }
    }
}
