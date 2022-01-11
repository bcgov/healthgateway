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
namespace HealthGateway.Common.Utils;

using System;
using System.Globalization;
using System.Security.Claims;

/// <summary>
/// Utilities for retrieving data from security objects.
/// </summary>
public static class ClaimsPrincipalReader
{
    /// <summary>
    /// Looks up authenticated logged in date time from claims principal.
    /// </summary>
    /// <param name="claimsPrincipal">Claims Principal to retrieve date time from.</param>
    /// <returns>Authenticated login date time.</returns>
    public static DateTime GetAuthDateTime(ClaimsPrincipal claimsPrincipal)
    {
        // auth_time is not mandatory in a Bearer token.
        string rowAuthTime = claimsPrincipal.FindFirstValue("auth_time");

        // get token  "issued at time", which *is* mandatory in JWT bearer token.
        rowAuthTime ??= claimsPrincipal.FindFirstValue("iat");

        // Auth time comes in the JWT as seconds after 1970-01-01
        DateTime jwtAuthTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(int.Parse(rowAuthTime, CultureInfo.CurrentCulture));

        return jwtAuthTime;
    }
}
