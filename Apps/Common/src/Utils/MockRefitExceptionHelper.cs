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
namespace HealthGateway.Common.Utils
{
    using System;
    using System.Net;
    using System.Net.Http;
    using Refit;

    /// <summary>
    /// Utility to mock exceptions.
    /// </summary>
    public static class MockRefitExceptionHelper
    {
        /// <summary>
        /// Build the api exception that the mock will throw.
        /// </summary>
        /// <param name="statusCode">The http status code to return.</param>
        /// <param name="method">The http method used to send the request.</param>
        /// <param name="response">The response content associated with the http response message.</param>
        /// <param name="innerException">Add an inner exception to the <see cref="ApiException"/>.</param>
        /// <returns>A <see cref="ApiException"/> representing a custom api exception creation.</returns>
        public static ApiException CreateApiException(HttpStatusCode statusCode, HttpMethod method, HttpContent? response = null, Exception? innerException = null)
        {
            RefitSettings refitSettings = new();

            using HttpRequestMessage requestMessage = new()
            {
                Method = method,
            };

            using HttpResponseMessage responseMessage = new()
            {
                StatusCode = statusCode,
                Content = response,
            };

            return ApiException.Create(
                    requestMessage,
                    requestMessage.Method,
                    responseMessage,
                    refitSettings,
                    innerException)
                .Result;
        }

        /// <summary>
        /// Build the http request exception that the mock will throw.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="statusCode">The http status code associated with the exception.</param>
        /// <param name="innerException">Add an inner exception to the <see cref="HttpRequestException"/>.</param>
        /// <returns>A <see cref="HttpRequestException"/> representing a custom api exception creation.</returns>
        public static HttpRequestException CreateHttpRequestException(string message, HttpStatusCode? statusCode = HttpStatusCode.InternalServerError, Exception? innerException = null)
        {
            return new HttpRequestException(message, innerException, statusCode);
        }
    }
}
