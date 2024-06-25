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
namespace HealthGateway.Common.Utils
{
    using HealthGateway.Common.Constants;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// HttpContextHelper class.
    /// </summary>
    public static class HttpContextHelper
    {
        private const string RouteResourceIdentifier = "hdid";

        /// <summary>
        /// Gets the subject identifier for requested health data resource(s) from the request context.
        /// </summary>
        /// <param name="context">The HTTP context containing information about the current request.</param>
        /// <param name="lookupMethod">The mechanism with which to retrieve the subject identifier.</param>
        /// <returns>The subject identifier for requested health data resource(s).</returns>
        public static string? GetResourceHdid(HttpContext? context, FhirSubjectLookupMethod lookupMethod)
        {
            string? retVal = lookupMethod switch
            {
                FhirSubjectLookupMethod.Route => context?.Request.RouteValues[RouteResourceIdentifier] as string,
                FhirSubjectLookupMethod.Parameter => context?.Request.Query[RouteResourceIdentifier],
                _ => null,
            };

            return retVal;
        }
    }
}
