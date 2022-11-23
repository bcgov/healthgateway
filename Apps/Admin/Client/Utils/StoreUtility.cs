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

namespace HealthGateway.Admin.Client.Utils
{
    using System;
    using HealthGateway.Admin.Client.Store;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Utilities for interacting with the application store.
    /// </summary>
    public static class StoreUtility
    {
        /// <summary>
        /// Formats errors returned from external requests.
        /// </summary>
        /// <param name="resultError">An error returned from the server.</param>
        /// <returns>The generated <see cref="RequestError"/>.</returns>
        public static RequestError FormatRequestError(RequestResultError? resultError)
        {
            return FormatRequestError(null, resultError);
        }

    /// <summary>
        /// Formats errors returned from external requests.
        /// </summary>
        /// <param name="exception">An exception returned by Refit.</param>
        /// <param name="resultError">An error returned from the server.</param>
        /// <returns>The generated <see cref="RequestError"/>.</returns>
        public static RequestError FormatRequestError(Exception? exception, RequestResultError? resultError = null)
        {
            if (resultError is not null)
            {
                return new()
                {
                    Message = resultError.ResultMessage,
                    Details = new()
                    {
                        { "errorCode", resultError.ErrorCode },
                        { "traceId", resultError.TraceId },
                        { "actionCode", resultError.ActionCodeValue ?? string.Empty },
                    },
                };
            }

            if (exception is not null)
            {
                return new()
                {
                    Message = exception.Message,
                };
            }

            return new()
            {
                Message = "Unknown error",
            };
        }
    }
}
