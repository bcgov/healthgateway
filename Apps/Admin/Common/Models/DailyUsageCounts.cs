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
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Model containing daily usage counts for the admin dashboard.
    /// </summary>
    public record DailyUsageCounts
    {
        /// <summary>Gets the daily counts of user registrations.</summary>
        public IDictionary<DateOnly, int> UserRegistrations { get; init; } = ImmutableDictionary<DateOnly, int>.Empty;

        /// <summary>Gets daily counts of user logins.</summary>
        public IDictionary<DateOnly, int> UserLogins { get; init; } = ImmutableDictionary<DateOnly, int>.Empty;

        /// <summary>Gets daily counts of dependent registrations.</summary>
        public IDictionary<DateOnly, int> DependentRegistrations { get; init; } = ImmutableDictionary<DateOnly, int>.Empty;
    }
}
