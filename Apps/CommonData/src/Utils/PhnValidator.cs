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
namespace HealthGateway.Common.Data.Utils
{
    /// <summary>
    /// Validates a Personal Health Number.
    /// </summary>
    public static class PhnValidator
    {
        /// <summary>
        /// Validates the supplied value is a proper Personal Health Number.
        /// </summary>
        /// <param name="phn">The Personal Health Number to validate.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValid(string? phn) => new Validations.PhnValidator().Validate(phn).IsValid;
    }
}
