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

namespace HealthGateway.Common.ErrorHandling
{
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.ErrorHandling;

    /// <summary>
    /// Utilities for translating between error codes and its codification.
    /// </summary>
    public static class ErrorTranslator
    {
        /// <summary>
        /// Formats the error from the given error type and service.
        /// </summary>
        /// <param name="errorType">Error type that caused the issue.</param>
        /// <param name="service">Service that reported the error.</param>
        /// <returns>A codified string representing the error.</returns>
        public static string ServiceError(ErrorType errorType, ServiceType service)
        {
            var applicationName = System.AppDomain.CurrentDomain.FriendlyName.ToString();
            return applicationName + "Server-" + errorType.Value + "-" + service.Value;
        }

        /// <summary>
        /// Formats the error from the given error type.
        /// </summary>
        /// <param name="errorType">Error type that caused the issue.</param>
        /// <returns>A codified string representing the error.</returns>
        public static string InternalError(ErrorType errorType)
        {
            var applicationName = System.AppDomain.CurrentDomain.FriendlyName.ToString();
            return applicationName + "Server-" + errorType.Value;
        }

        /// <summary>
        /// Formats the error from the given error type.
        /// </summary>
        /// <param name="message">The user friendly message.</param>
        /// <param name="actionType">Action type that caused the issue.</param>
        /// <returns>A RequestResultError encapsulating the action required.</returns>
        public static RequestResultError ActionRequired(string message, ActionType actionType)
        {
            return new RequestResultError()
            {
                ResultMessage = message,
                ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                ActionCode = actionType,
            };
        }
    }
}
