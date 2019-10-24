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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using HealthGateway.Common.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Authorization service
    /// </summary>
    public class AuditService : IAuditService
    {
        private const string ANONYMOUS = "anonymous";

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public AuditService(IConfiguration config)
        {
        }

        /// <inheritdoc />
        public Task WriteAuditEvent(AuditEvent auditEvent)
        {
            Task t = Task.Factory.StartNew(() => 
            {
                // Execute audit logging into database context

            } );
            
            return t;
        }

        /// <inheritdoc />
        public AuditEvent ParseHttpContext(HttpContext context, AuditEvent audit)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (audit is null)
            {
                audit = new AuditEvent();
            }

            audit.ApplicationName = this.GetApplication();
            audit.TransactionResult = this.GetTransactionResult(context.Response.StatusCode);
            audit.ApplicationSubject = context.User.Identity.Name;
            audit.CreatedBy = string.IsNullOrEmpty(context.User.Identity.Name) ?
                ANONYMOUS : context.User.Identity.Name;
            audit.TransacationName = context.Request.Path;
            audit.Trace = context.TraceIdentifier;
            audit.ClientIP = context.Connection.RemoteIpAddress.MapToIPv4().ToString();

            RouteData routeData = context.GetRouteData();

            // Some routes might not have the version
            audit.TransactionVersion = routeData != null ?
                routeData.Values["version"].ToString() : string.Empty;
            return audit;
        }

        /// <summary>
        /// Gets the transaction result from the http context status code.
        /// </summary>
        /// <param name="statusCode">The http context status code.</param>
        /// <returns>The mapped transaction result.</returns>
        private AuditTransactionResult GetTransactionResult(int statusCode)
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
        /// Gets the current application.
        /// </summary>
        /// <returns>The mapped application.</returns>
        private Application GetApplication()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
            object returnValue;
            if (Enum.TryParse(typeof(Application), assemblyName.Name, true, out returnValue))
            {
                return (Application)returnValue;
            }
            else
            {
                throw new NotSupportedException($"Audit Error: Invalid application name '{assemblyName.Name}'");
            }
        }
    }
}