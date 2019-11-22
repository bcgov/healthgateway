﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.Medication.Models
{
    using HealthGateway.Common.Constants;

    /// <summary>
    /// The HN Client message.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public class HNMessage<T>
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        public HNMessage()
            : this(null, ResultType.Success, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        /// <param name="response">The response message object.</param>
        public HNMessage(T response)
            : this(response, ResultType.Success, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        /// <param name="result">The result of the transaction.</param>
        /// <param name="resultMessage">A message based on the result.</param>
        public HNMessage(ResultType result, string resultMessage)
            : this(null, result, resultMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        /// <param name="response">The response message object.</param>
        /// <param name="result">The result of the transaction.</param>
        /// <param name="resultMessage">A message based on the result.</param>
        public HNMessage(T response, ResultType result, string resultMessage)
        {
            this.Message = response;
            this.Result = result;
            this.ResultMessage = resultMessage;
        }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public T Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the result.
        /// </summary>
        public ResultType Result { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the result.
        /// </summary>
        public string ResultMessage { get; set; }
    }
}
