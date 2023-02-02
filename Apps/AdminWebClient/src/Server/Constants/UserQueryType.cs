﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Admin.Constants
{
    /// <summary>
    /// Represents the type of query being performed.
    /// </summary>
    public enum UserQueryType
    {
        /// <summary>
        /// Query by Personal Health Number.
        /// </summary>
        Phn,

        /// <summary>
        /// Query by email.
        /// </summary>
        Email,

        /// <summary>
        /// Query by SMS phone number.
        /// </summary>
        Sms,

        /// <summary>
        /// Query by Health-Directed identifier.
        /// </summary>
        Hdid,

        /// <summary>
        /// Query by dependent Personal Health Number.
        /// </summary>
        Dependent,
    }
}
