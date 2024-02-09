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
namespace AccountDataAccessTest.Utils
{
    using System.Net;
    using Refit;

    /// <summary>
    /// Exception utilities class to assist in mocking Refit exceptions.
    /// </summary>
    public static class RefitExceptionUtil
    {
        /// <summary>
        /// Creates a Refit exception with the given status code and reason phrase.
        /// </summary>
        /// <param name="statusCode">The desired exception status code.</param>
        /// <returns>Refit ApiException</returns>
        public static async Task<ApiException> CreateApiException(HttpStatusCode statusCode)
        {
            RefitSettings rfSettings = new();
            using HttpResponseMessage response = new(statusCode);
            return await ApiException.Create(
                null!,
                null!,
                response,
                rfSettings);
        }
    }
}
