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
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    #pragma warning disable CA1303 // Do not pass literals as localized parameters

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private readonly ILogger<IAuditLogger> logger;

        private readonly IWriteAuditEventDelegate writeEventDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="writeEventDelegate">The audit event delegate.</param>
        public AuditLogger(ILogger<IAuditLogger> logger, IWriteAuditEventDelegate writeEventDelegate)
        {
            this.logger = logger;
            this.writeEventDelegate = writeEventDelegate;
        }

        /// <inheritdoc />
        public void WriteAuditEvent(AuditEvent auditEvent)
        {
            #pragma warning disable CA1031 // Modify 'WriteAuditEvent' to catch a more specific exception type, or rethrow the exception.
            this.logger.LogDebug(@"Begin WriteAuditEvent(auditEvent)");
            try
            {
                this.writeEventDelegate.WriteAuditEvent(auditEvent);
                this.logger.LogDebug(@"Saved AuditEvent");
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, @"In WriteAuditEvent");
            }
            #pragma warning restore CA1303
        }

        /// <inheritdoc />
        public void PopulateWithHttpContext(HttpContext context, AuditEvent auditEvent)
        {
            ClaimsIdentity? claimsIdentity = context.User.Identity as ClaimsIdentity;
            Claim? hdidClaim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "hdid");
            string? hdid = hdidClaim?.Value;

            Claim? idirClaim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "preferred_username");
            string? idir = idirClaim?.Value;

            // Query PHN header for Admin WebClient CovidCard actions
            context.Request.Headers.TryGetValue("phn", out var phnHeader);
            string? subjectPhn = phnHeader.FirstOrDefault();

            auditEvent.ApplicationType = GetApplicationType();
            auditEvent.TransactionResultCode = GetTransactionResultType(context.Response.StatusCode);

            auditEvent.ApplicationSubject = hdid ?? subjectPhn;
            auditEvent.CreatedBy = hdid ?? idir ?? UserId.DefaultUser;

            RouteValueDictionary routeValues = context.Request.RouteValues;
            auditEvent.TransactionName = @$"{routeValues["controller"]}\{routeValues["action"]}";

            auditEvent.Trace = context.TraceIdentifier;
            auditEvent.ClientIP = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
            RouteData routeData = context.GetRouteData();

            // Some routes might not have the version
            auditEvent.TransactionVersion = routeData?.Values["version"] != null ?
                routeData.Values["version"]?.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the transaction result from the http context status code.
        /// </summary>
        /// <param name="statusCode">The http context status code.</param>
        /// <returns>The mapped transaction result.</returns>
        private static AuditTransactionResult GetTransactionResultType(int statusCode)
        {
            // Success codes (1xx, 2xx, 3xx)
            if (statusCode < 400)
            {
                return AuditTransactionResult.Success;
            }

            // Unauthorized and forbidden (401, 403)
            if (statusCode == 401 || statusCode == 403)
            {
                return AuditTransactionResult.Unauthorized;
            }

            // Client/Request errors codes other than unauthorized and forbidden (4xx)
            if (statusCode >= 400 && statusCode < 500)
            {
                return AuditTransactionResult.Failure;
            }

            // System error codes (5xx)
            return AuditTransactionResult.SystemError;
        }

        /// <summary>
        /// Gets the current audit application type from the assembly name.
        /// </summary>
        /// <returns>The mapped application type.</returns>
        private static string GetApplicationType()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly()!.GetName();
            switch (assemblyName.Name)
            {
                case "Configuration":
                    return ApplicationType.Configuration;
                case "WebClient":
                    return ApplicationType.WebClient;
                case "Immunization":
                    return ApplicationType.Immunization;
                case "Patient":
                    return ApplicationType.Patient;
                case "Medication":
                    return ApplicationType.Medication;
                case "AdminWebClient":
                    return ApplicationType.AdminWebClient;
                case "Admin.Server":
                    return ApplicationType.AdminWebClient;
                case "Laboratory":
                    return ApplicationType.Laboratory;
                case "Encounter":
                    return ApplicationType.Encounter;
                case "ReSharperTestRunner":
                case "testhost":
                    return ApplicationType.Configuration;
                default:
                    throw new NotSupportedException($"Audit Error: Invalid application name '{assemblyName.Name}'");
            }
        }
    }

    #pragma warning restore CA1303 // Do not pass literals as localized parameters
}
