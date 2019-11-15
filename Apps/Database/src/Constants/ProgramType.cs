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
    public static class ProgramType
    {
        /// <summary>
        /// The Federal Drug loading program for active drugs.
        /// </summary>
        public const string FederalApproved = "FED-DRUG-A";

        /// <summary>
        /// The Federal Drug loading program for marketed drugs.
        /// </summary>
        public const string FederalMarketed = "FED-DRUG-M";

        /// <summary>
        /// The Federal Drug loading program for Cancelled drugs.
        /// </summary>
        public const string FederalCancelled = "FED-DRUG-C";

        /// <summary>
        /// The Federal Drug loading program for dorman drugs.
        /// </summary>
        public const string FederalDormant = "FED-DRUG-D";

        /// <summary>
        /// The Provincial Drug loading program.
        /// </summary>
        public const string Provincial = "PROV-DRUG";
    }
}
