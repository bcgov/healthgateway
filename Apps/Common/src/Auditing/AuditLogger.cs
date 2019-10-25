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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

#pragma warning disable CA1303 // Do not pass literals as localized parameters

    /// <summary>
    /// The Authorization service
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private const string ANONYMOUS = "anonymous";

        private readonly ILogger<IAuditLogger> logger;

        private readonly IConfiguration configuration;

        private readonly AuditDbContext dbContext;

        public AuditLogger(ILogger<IAuditLogger> logger, AuditDbContext dbContext, IConfiguration config)
        {
            this.logger = logger;
            this.configuration = config;
            this.dbContext = dbContext;
        }

        public void WriteAuditEvent(AuditEvent auditEvent)
        {
            // An audit event catches all types of exceptions.
#pragma warning disable CA1031 // Modify 'WriteAuditEvent' to catch a more specific exception type, or rethrow the exception.
            this.logger.LogDebug(@"Begin AuditLogger.WriteAuditEvent(auditEvent)");
            try
            {
                this.dbContext.AuditEvent.Add(auditEvent);
                this.dbContext.SaveChanges();
                this.logger.LogInformation(@"Saved AuditEvent");
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
            Contract.Requires(context != null);
            Contract.Requires(auditEvent != null);

            ClaimsIdentity claimsIdentity = (ClaimsIdentity)context.User.Identity;
            Claim hdidClaim = claimsIdentity.Claims.Where(c => c.Type == "hdid").FirstOrDefault();
            string hdid = hdidClaim?.Value;

            auditEvent.ApplicationType = this.GetApplicationType();
            auditEvent.TransactionResultType = this.GetTransactionResultType(context.Response.StatusCode);
            auditEvent.ApplicationSubject = hdid;
            auditEvent.CreatedBy = string.IsNullOrEmpty(context.User.Identity.Name) ?
                ANONYMOUS : context.User.Identity.Name;
            auditEvent.TransacationName = context.Request.Path;
            auditEvent.Trace = context.TraceIdentifier;
            auditEvent.ClientIP = context.Connection.RemoteIpAddress.MapToIPv4().ToString();

            RouteData routeData = context.GetRouteData();

            // Some routes might not have the version
            auditEvent.TransactionVersion = routeData?.Values["version"] != null ?
                routeData.Values["version"].ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the transaction result from the http context status code.
        /// </summary>
        /// <param name="statusCode">The http context status code.</param>
        /// <returns>The mapped transaction result.</returns>
        private AuditTransactionResultType GetTransactionResultType(int statusCode)
        {
            // Success codes (1xx, 2xx, 3xx)
            if (statusCode < 400)
            {
                return AuditTransactionResultType.Success;
            }

            // Unauthorized and forbidden (401, 403)
            if (statusCode == 401 || statusCode == 403)
            {
                return AuditTransactionResultType.Unauthorized;
            }

            // Client/Request errors codes other than unauthorized and forbidden (4xx)
            if (statusCode >= 400 && statusCode < 500)
            {
                return AuditTransactionResultType.Failure;
            }

            // System error codes (5xx)
            return AuditTransactionResultType.SystemError;
        }

        /// <summary>
        /// Gets the current audit application type from the assembly name.
        /// </summary>
        /// <returns>The mapped application type.</returns>
        private AuditApplicationType GetApplicationType()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
            object returnValue;
            if (Enum.TryParse(typeof(AuditApplicationType), assemblyName.Name, true, out returnValue))
            {
                return (AuditApplicationType)returnValue;
            }
            else
            {
                throw new NotSupportedException($"Audit Error: Invalid application name '{assemblyName.Name}'");
            }
        }
    }

#pragma warning restore CA1303 // Do not pass literals as localized parameters
}