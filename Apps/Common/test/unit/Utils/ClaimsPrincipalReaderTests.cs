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
namespace HealthGateway.CommonTests.Utils;

using HealthGateway.Common.Utils;
using System;
using System.Globalization;
using System.Security.Claims;
using Xunit;

/// <summary>
/// Reading Claims Principal tests.
/// </summary>
public class ClaimsPrincipalReaderTests
{
    /// <summary>
    /// Get auth time from client principal when claim type is auth_time.
    /// </summary>
    [Fact]
    public void ShouldGetAuthDateTimeWhenClaimTypeIsAuthTime()
    {
        string expected = "12/22/2021 19:42:21";
        string actual = "1640202141";

        // Arrange
        ClaimsPrincipal principal = new();
        ClaimsIdentity identity = new();
        Claim claim = new(type: "auth_time", value: actual);
        identity.AddClaim(claim);
        principal.AddIdentity(identity);

        // Act
        DateTime jwtAuthDateTime = ClaimsPrincipalReader.GetAuthDateTime(principal);

        // Assert
        Assert.Equal(expected, jwtAuthDateTime.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Get auth time from client principal when claim type is iat.
    /// </summary>
    [Fact]
    public void ShouldGetAuthDateTimeWhenClaimTypeIsIat()
    {
        string expected = "12/22/2021 19:42:21";
        string actual = "1640202141";

        // Arrange
        ClaimsPrincipal principal = new();
        ClaimsIdentity identity = new();
        Claim claim = new(type: "iat", value: actual);
        identity.AddClaim(claim);
        principal.AddIdentity(identity);

        // Act
        DateTime jwtAuthDateTime = ClaimsPrincipalReader.GetAuthDateTime(principal);

        // Assert
        Assert.Equal(expected, jwtAuthDateTime.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Get base auth time from client principal when there is no claim.
    /// </summary>
    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenThereIsNoClaim()
    {
        // Arrange
        ClaimsPrincipal principal = new();
        ClaimsIdentity identity = new();
        principal.AddIdentity(identity);

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ClaimsPrincipalReader.GetAuthDateTime(principal));
    }
}
