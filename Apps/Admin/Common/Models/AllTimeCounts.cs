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
namespace HealthGateway.Admin.Common.Models
{
    /// <summary>
    /// Model containing all-time counts for the admin dashboard.
    /// </summary>
    public record AllTimeCounts
    {
        /// <summary>Gets the number of registered Health Gateway users.</summary>
        public int RegisteredUsers { get; init; }

        /// <summary>Gets the number of dependent registrations in Health Gateway.</summary>
        public int Dependents { get; init; }

        /// <summary>Gets the number of closed Health Gateway accounts.</summary>
        public int ClosedAccounts { get; init; }
    }
}
