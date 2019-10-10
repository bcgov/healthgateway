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
namespace HealthGateway.Medication.Models
{
    /// <summary>
    /// The HN Client message.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public class HNMessage<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        public HNMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        /// <param name="response">The response message object.</param>
        public HNMessage(T response)
        {
            this.Message = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNMessage{T}"/> class.
        /// </summary>
        /// <param name="isError">The error indicator.</param>
        /// <param name="errorMessage">The error message.</param>
        public HNMessage(bool isError, string errorMessage)
        {
            this.IsErr = isError;
            this.Error = errorMessage;
        }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public T Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error occurred.
        /// </summary>
        public bool IsErr { get; set; }

        /// <summary>
        /// Gets or sets the error message associated to this Message.
        /// </summary>
        public string Error { get; set; }
    }
}
