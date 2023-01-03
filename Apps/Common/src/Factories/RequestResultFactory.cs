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

namespace HealthGateway.Common.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation.Results;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;

    /// <summary>
    /// Factory for  <see cref="RequestResult{T}"/> instances.
    /// </summary>
    public static class RequestResultFactory
    {
        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="resultError">The error.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(RequestResultError resultError)
        {
            return Error<T>(default, resultError);
        }

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="payload">the payload.</param>
        /// <param name="resultError">The error.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(T? payload, RequestResultError resultError)
        {
            return new()
            {
                ResourcePayload = payload,
                ResultStatus = ResultType.Error,
                ResultError = resultError,
            };
        }

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, string errorMessage)
        {
            return Error<T>(
                new RequestResultError
                {
                    ResultMessage = errorMessage,
                    ErrorCode = ErrorTranslator.InternalError(errorType),
                });
        }

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, IEnumerable<string> errorMessages)
        {
            return Error<T>(errorType, string.Join(";", errorMessages));
        }

        /// <summary>
        /// Factory method for error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="validationResults">Fluent validation errors.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> Error<T>(ErrorType errorType, IEnumerable<ValidationFailure> validationResults)
        {
            return Error<T>(errorType, validationResults.Select(vr => vr.ErrorMessage));
        }

        /// <summary>
        /// Factory method for service error <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="serviceType">The service where the error is coming from.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with service error.</returns>
        public static RequestResult<T> ServiceError<T>(ErrorType errorType, ServiceType serviceType, string errorMessage)
        {
            return new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = errorMessage,
                    ErrorCode = ErrorTranslator.ServiceError(errorType, serviceType),
                },
            };
        }

        /// <summary>
        /// Factory method for action required <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <returns>New <see cref="RequestResult{T}"/> instance with error.</returns>
        public static RequestResult<T> ActionRequired<T>(ActionType actionType, string errorMessage)
        {
            return new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResultError = ErrorTranslator.ActionRequired(errorMessage, actionType),
            };
        }

        /// <summary>
        /// Factory method for successful <see cref="RequestResult{T}"/> instances.
        /// </summary>
        /// <typeparam name="T">The payload type.</typeparam>
        /// <param name="payload">the payload to return.</param>
        /// <param name="totalResultCount">the total result count, defaults to 0.</param>
        /// <param name="pageIndex">the page index.</param>
        /// <param name="pageSize">the page size.</param>
        /// <returns>New <see cref="RequestResult{T}"/> instance with success and payload.</returns>
        public static RequestResult<T> Success<T>(T payload, int totalResultCount = 0, int pageIndex = 0, int pageSize = 0)
        {
            return new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = payload,
                TotalResultCount = totalResultCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
        }
    }
}
