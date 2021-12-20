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
namespace HealthGateway.Common.Data.Constants
{
    /// <summary>
    /// An enum representing the possible results for our transactions.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// Represents a system error condition.
        /// </summary>
        Error = 0,

        /// <summary>
        /// Represents a successful result.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Represents that the request needs an user action.
        /// </summary>
        ActionRequired = 2,
    }
}
