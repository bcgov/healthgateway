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
namespace HealthGateway.Mock.AccessManagement.Authentication;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HealthGateway.Common.Models.ODR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Authentication handler to verify valid credentials for ODR access are provided using Basic Authorization.
/// </summary>
public class OdrBasicAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly string expectedCredentials;
    private readonly bool enabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="OdrBasicAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="options">The injected monitor for the options instance.</param>
    /// <param name="logger">The injected logger.</param>
    /// <param name="encoder">The injected UrlEncoder.</param>
    /// <param name="clock">The injected ISystemClock.</param>
    public OdrBasicAuthorizationHandler(
        IConfiguration configuration,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        OdrConfig odrConfig = new();
        configuration.Bind(OdrConfig.OdrConfigSectionKey, odrConfig);

        this.expectedCredentials = odrConfig.Authorization?.Credentials ?? string.Empty;
        this.enabled = odrConfig.Authorization?.Enabled ?? false;
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All exceptions result in authentication failure")]
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // skip authentication if endpoint has [AllowAnonymous] attribute
        Endpoint? endpoint = this.Context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // skip authentication if it's been disabled via configuration
        if (!this.enabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!this.Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        try
        {
            AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(this.Request.Headers["Authorization"]);
            byte[] credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            string[] credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            string username = credentials[0];
            string password = credentials[1];

            if ($"{username}:{password}" != this.expectedCredentials)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
            }
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }

        Claim[] claims = { new(ClaimTypes.AuthenticationMethod, this.Scheme.Name) };
        ClaimsIdentity identity = new(claims, this.Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, this.Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
