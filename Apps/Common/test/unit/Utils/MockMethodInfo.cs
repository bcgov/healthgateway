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
namespace HealthGateway.CommonTests.Utils
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Mock Method Info.
    /// </summary>
    [Authorize]
    public static class MockMethodInfo
    {
        /// <summary>
        /// Mock Method.
        /// </summary>
        public static void MockMethod()
        {
            // Empty Swagger method.
        }

        /// <summary>
        /// Authorized Method.
        /// </summary>
        [Authorize]
        public static void AuthorizedMethod()
        {
            // Empty Swagger method.
        }
    }
}
