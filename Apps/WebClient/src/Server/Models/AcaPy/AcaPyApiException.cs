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
namespace HealthGateway.WebClient.Server.Models.AcaPy
{
    using System;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
    /// </summary>
    public class AcaPyApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        public AcaPyApiException()
        : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        /// <param name="message">Message of exception.</param>
        public AcaPyApiException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        /// <param name="message">Message of exception.</param>
        /// <param name="inner">Inner message of exception.</param>
        public AcaPyApiException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
