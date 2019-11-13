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
namespace HealthGateway.Common.Constants
{
    /// <summary>
    /// A class with constants representing the various prescription statues.
    /// </summary>
    public static class PrescriptionStatus
    {
        /// <summary>
        /// The status code for Filled prescriptions.
        /// </summary>
        public const char Filled = 'F';

        /// <summary>
<<<<<<< HEAD
        /// The status code for Discontinued prescriptions.
        /// </summary>
        public const char Discontinued = 'D';

        /// <summary>
        /// The status code for refused to fill.
        /// </summary>
        public const char NotDispensed = 'N';

        /// <summary>
        /// Teh status code for reversed prescriptions.
        /// </summary>
        public const char Reversed = 'R';
=======
        /// The status code for Dispensed prescriptions.
        /// </summary>
        public const char Dispensed = 'D';
>>>>>>> dev
    }
}
