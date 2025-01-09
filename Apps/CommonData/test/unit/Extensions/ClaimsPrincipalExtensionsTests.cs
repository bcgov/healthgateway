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
namespace HealthGateway.Common.Data.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using HealthGateway.Common.Data.Extensions;
    using Xunit;

    /// <summary>
    /// Tests for ClaimsPrincipalExtensions.
    /// </summary>
    public class ClaimsPrincipalExtensionsTests
    {
        public static TheoryData<string[], string[], bool> IsInAnyRoleTheoryData =>
            new()
            {
                { ["A"], ["A"], true },
                { ["A"], ["B"], false },
                { ["A", "B"], ["A"], true },
                { ["A", "B"], ["B"], true },
                { ["A", "B"], ["A", "B"], true },
                { ["A"], [], false },
                { [], ["A"], false },
            };

        [Theory]
        [MemberData(nameof(IsInAnyRoleTheoryData))]
        public void ValidateToEnumString(string[] userRoles, string[] targetRoles, bool expected)
        {
            IEnumerable<Claim> claims = userRoles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r));
            ClaimsPrincipal user = new([new ClaimsIdentity(claims)]);

            bool actual = user.IsInAnyRole(targetRoles);

            Assert.Equal(expected, actual);
        }
    }
}
