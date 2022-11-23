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
namespace HealthGateway.Database.Wrapper
{
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Class that represents the result of a request. Contains members for handling pagination and error resolution.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public class DbResult<T>
        where T : class?
    {
        /// <summary>
        /// Gets or sets the result payload.
        /// </summary>
        public T Payload { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Status of the request.
        /// </summary>
        public DbStatusCode Status { get; set; }

        /// <summary>
        /// Gets or sets the message depending on the result type.
        /// Will always be set when ResultType is Error.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
