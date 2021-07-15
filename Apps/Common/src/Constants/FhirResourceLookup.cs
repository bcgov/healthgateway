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
    /// An enumeration representing the mechanism with which to lookup the fhir resource identifer.
    /// </summary>
    public enum FhirResourceLookup
    {
        /// <summary>
        /// Lookup the identifer via the http route.
        /// </summary>
        Route,

        /// <summary>
        /// Lookup the identifer via parameter.
        /// </summary>
        Parameter,
    }
}