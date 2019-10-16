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
namespace HealthGateway.Medication
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Npgsql;
    using NpgsqlTypes;

    /// <inheritdoc/>
    public class MedicationDBContextExt : IMedicationDBContextExt
    {
        /// <inheritdoc/>
        public long NextValueForSequence(MedicationDBContext ctx, string seq)
        {
            NpgsqlParameter result = new NpgsqlParameter("@result", NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            ctx.Database.ExecuteSqlCommand($"SELECT nextval('{seq}')", result);
            // code below is to be used when updating to EF 3
            // ctx.Database.ExecuteSqlRaw($"SELECT nextval('{seq}')", result);
            return (long)result.Value;
        }
    }
}
