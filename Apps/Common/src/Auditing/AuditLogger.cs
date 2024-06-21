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
namespace HealthGateway.Common.Auditing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// The Abstract Audit Logger service.
    /// </summary>
    public abstract class AuditLogger : IAuditLogger
    {
        /// <inheritdoc/>
        public abstract Task WriteAuditEventAsync(AuditEvent auditEvent, CancellationToken ct = default);

        /// <inheritdoc/>
        public void PopulateWithHttpContext(HttpContext context, AuditEvent auditEvent)
        {
            ClaimsIdentity? claimsIdentity = context.User.Identity as ClaimsIdentity;
            Claim? hdidClaim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "hdid");
            string? hdid = hdidClaim?.Value;

            Claim? idirClaim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "preferred_username");
            string? idir = idirClaim?.Value;

            // Query parameters for Admin.Server actions
            context.Request.Query.TryGetValue("phn", out StringValues phnParameter);
            string? subjectPhn = phnParameter.FirstOrDefault();

            context.Request.Query.TryGetValue("queryString", out StringValues queryStringParameter);
            string? subjectQuery = queryStringParameter.FirstOrDefault();

            auditEvent.ApplicationType = GetApplicationType();
            auditEvent.TransactionResultCode = GetTransactionResultType(context.Response.StatusCode);

            auditEvent.ApplicationSubject = subjectQuery ?? hdid ?? subjectPhn;
            auditEvent.CreatedBy = hdid ?? idir ?? UserId.DefaultUser;

            RouteValueDictionary routeValues = context.Request.RouteValues;
            auditEvent.TransactionName = @$"{routeValues["controller"]}\{routeValues["action"]}";

            auditEvent.Trace = context.TraceIdentifier;
            auditEvent.ClientIp = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
            RouteData routeData = context.GetRouteData();

            // Some routes might not have the version
            auditEvent.TransactionVersion = routeData.Values["version"] != null ? routeData.Values["version"]?.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the transaction result from the http context status code.
        /// </summary>
        /// <param name="statusCode">The http context status code.</param>
        /// <returns>The mapped transaction result.</returns>
        private static AuditTransactionResult GetTransactionResultType(int statusCode)
        {
            return statusCode switch
            {
                < 400 => AuditTransactionResult.Success, // Success codes (1xx, 2xx, 3xx)
                401 or 403 => AuditTransactionResult.Unauthorized, // Unauthorized and forbidden (401, 403)
                < 500 => AuditTransactionResult.Failure, // Client/Request errors codes other than unauthorized and forbidden (4xx)
                _ => AuditTransactionResult.SystemError, // System error codes (5xx)
            };
        }

        /// <summary>
        /// Gets the current audit application type from the assembly name.
        /// </summary>
        /// <returns>The mapped application type.</returns>
        [ExcludeFromCodeCoverage]
        private static string GetApplicationType()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly()!.GetName();
            return assemblyName.Name switch
            {
                "Configuration" => ApplicationType.Configuration,
                "GatewayApi" or "WebClient" => ApplicationType.WebClient,
                "Immunization" => ApplicationType.Immunization,
                "Patient" => ApplicationType.Patient,
                "Medication" => ApplicationType.Medication,
                "Admin.Server" => ApplicationType.Admin,
                "Laboratory" => ApplicationType.Laboratory,
                "Encounter" => ApplicationType.Encounter,
                "ClinicalDocument" => ApplicationType.ClinicalDocument,
                "ReSharperTestRunner" or "ReSharperTestRunner64" or "ReSharperTestRunnerArm64" or "testhost" => ApplicationType.Configuration,
                _ => throw new NotSupportedException($"Audit Error: Invalid application name '{assemblyName.Name}'"),
            };
        }
    }
}
