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
    /// A class with constants representing the various email template names.
    /// </summary>
    public static class FhirAccessType
    {
        /// <summary>
        /// Wildcard representing all available resources under the given context.
        /// </summary>
        public const string Wildcard = "*";

        /// <summary>
        /// Allows information to be read from the resource in a way that does not directly modify the resource.
        /// These interactions MAY include read, vread, history, search.
        /// See capabilities as defined at <a href="https://www.hl7.org/fhir/http.html"/>.
        /// </summary>
        public const string Read = "read";

        /// <summary>
        /// Allows information to be written to the resource.
        /// These interactions MAY include update, patch, delete, create.
        /// See capabilities as defined at <a href="https://www.hl7.org/fhir/http.html"/>.
        /// </summary>
        public const string Write = "write";
    }
}