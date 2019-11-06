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
namespace HealthGateway.Database.Constant
{
    /// <summary>
    /// The set of programs.
    /// </summary>
    public enum ProgramType
    {
        /// <summary>
        /// The Federal Drug loading program for active drugs.
        /// </summary>
        FederalApproved = 105,

        /// <summary>
        /// The Federal Drug loading program for marketed drugs.
        /// </summary>
        FederalMarketed = 110,

        /// <summary>
        /// The Federal Drug loading program for Cancelled drugs.
        /// </summary>
        FederalCancelled = 115,

        /// <summary>
        /// The Federal Drug loading program for dorman drugs.
        /// </summary>
        FederalDormant = 120,

        /// <summary>
        /// The Provincial Drug loading program.
        /// </summary>
        Provincial = 200,
    }
}
