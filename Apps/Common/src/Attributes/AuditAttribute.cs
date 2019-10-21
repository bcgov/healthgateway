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
namespace HealthGateway.Common.Attributes
{
    using System.Net;
    using System.Reflection;
    using HealthGateway.Common.Database.Models;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>
    /// The audit attribute.
    /// </summary>
    public class AuditAttribute : ActionFilterAttribute
    {
        private IAuditService auditService;
        public AuditAttribute(IAuditService auditSvc) 
        {
            this.auditService = auditSvc;
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            AuditEvent audit = new AuditEvent();

            AssemblyName assemblyName = context.Controller.GetType().Assembly.GetName();
            switch (assemblyName.Name)
            {
                case "Immunization":
                    audit.ApplicationName = Applications.Immunization;
                    break;
                case "Medication":
                    audit.ApplicationName = Applications.Medication;
                    break;
                case "Patient":
                    audit.ApplicationName = Applications.Patient;
                    break;
                case "WebClient":
                    audit.ApplicationName = Applications.WebClient;
                    break;
                default:
                    throw new System.Exception($"Invalid applicatio name: {assemblyName.Name}");
            }
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var controllerName = actionDescriptor.ControllerName;
            var actionName = actionDescriptor.ActionName;
            var parameters = actionDescriptor.Parameters;
            var fullName = actionDescriptor.DisplayName;

            IPAddress address = context.HttpContext.Connection.RemoteIpAddress;
            string ipv4Address = address.MapToIPv4().ToString();

            var statusCode = (context.Result as ObjectResult)?.StatusCode;
            switch ((HttpStatusCode)statusCode)
            {
                case HttpStatusCode.InternalServerError:
                    audit.TransactionResult = AuditTransactionResult.ServerError;
                    break;
                case HttpStatusCode.OK:
                    audit.TransactionResult = AuditTransactionResult.Ok;
                    break;
            }
            // this.auditService.WriteAuditEvent(new AuditEvent() {
            //     AuditEventDateTime = DateTime.UtcNow,
            //     ApplicationSubject = "",
            //     ClientIP = ipv4Address,
            //     ApplicationName = Applications.Medication,
            //     TransacationName = "",
            //     TransactionResult = statusCode
            // });
        }
    }
}