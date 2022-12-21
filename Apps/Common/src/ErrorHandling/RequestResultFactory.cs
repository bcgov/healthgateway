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

namespace HealthGateway.Common.ErrorHandling
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Factory for  <see cref="RequestResult{T}"/> instances.
    /// </summary>
    public static class RequestResultFactory
    {
        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New  <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, string errorMessage)
            where T : class? => new RequestResult<T>
            {
                ResultStatus = Data.Constants.ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = errorMessage,
                    ErrorCode = ErrorTranslator.InternalError(errorType),
                },
            };

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New  <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, IEnumerable<string> errorMessages)
            where T : class? =>
                Error<T>(errorType, string.Join(";", errorMessages));

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="validationResults">Fluent validation errors.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New  <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, IEnumerable<FluentValidation.Results.ValidationFailure> validationResults)
            where T : class? =>
                Error<T>(errorType, validationResults.Select(vr => vr.ErrorMessage));

        /// <summary>
        /// Factory method for service error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="serviceType">The service where the error is coming from.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with service error.</returns>
        public static RequestResult<T> ServiceError<T>(ErrorType errorType, ServiceType serviceType, string errorMessage)
            where T : class? => new RequestResult<T>
            {
                ResultStatus = Data.Constants.ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = errorMessage,
                    ErrorCode = ErrorTranslator.ServiceError(errorType, serviceType),
                },
            };

        /// <summary>
        /// Factory method for action required <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New  <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> ActionRequired<T>(ActionType actionType, string errorMessage)
            where T : class? => new RequestResult<T>
            {
                ResultStatus = Data.Constants.ResultType.ActionRequired,
                ResultError = ErrorTranslator.ActionRequired(errorMessage, actionType),
            };

        /// <summary>
        /// Factory method for successful <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <param name="payload">the payload to return.</param>
        /// <returns>New <see cref="RequestResult{T}"/> instance with success and payload.</returns>
        public static RequestResult<T> Success<T>(T payload)
            where T : class? => new RequestResult<T>
            {
                ResultStatus = Data.Constants.ResultType.Success,
                ResourcePayload = payload,
            };
    }
}
