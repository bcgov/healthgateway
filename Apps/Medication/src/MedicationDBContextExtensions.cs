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

    /// <summary>
    /// Extensions to the Medications DB Context.
    /// </summary>
    public static class MedicationDBContextExtentions
    {

        private static IMedicationDBContextExt defaultImpl = new MedicationDBContextExt();
        public static IMedicationDBContextExt Implementation { get; set; } = defaultImpl;

        public static void RevertToDefaultImplementation()
        {
            Implementation = defaultImpl;
        }

        /// <inheritdoc/>
        public static long NextValueForSequence(this MedicationDBContext ctx, string seq)
        {
            return Implementation.NextValueForSequence(ctx, seq);
        }
    }
}
