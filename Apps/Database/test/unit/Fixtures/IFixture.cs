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
    using HealthGateway.Database.Context;
    using Respawn.Graph;

    /// <summary>
    /// Methods to implement for Fixture classes.
    /// </summary>
    public interface IFixture
    {
        /// <summary>
        /// Returns database to state before text was executed.
        /// </summary>
        /// <returns>A list of Table objects containing tables to reset.</returns>
        Table[] TablesToReset();

        /// <summary>
        /// Prepare test data in database.
        /// </summary>
        /// <param name="context">Contains GatewayDbContext.</param>
        void SetupDatabase(GatewayDbContext context);
    }
}
