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
namespace HealthGateway.Common.Swagger
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Swagger filter to add the bearer into each request.
    /// </summary>
    public class AuthenticationRequirementsOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Adds the bearer token into each request.
        /// </summary>
        /// <param name="operation">The swagger operation.</param>
        /// <param name="context">The filter context.</param>
        [ExcludeFromCodeCoverage]
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security ??= [];

            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor cad)
            {
                bool controllerAuth = Array.Exists(cad.ControllerTypeInfo.GetCustomAttributes(true), t => t is AuthorizeAttribute);
                bool methodAuth = Array.Exists(cad.MethodInfo.GetCustomAttributes(false), t => t is AuthorizeAttribute);
                bool methodAnonymous = Array.Exists(cad.MethodInfo.GetCustomAttributes(false), t => t is AllowAnonymousAttribute);

                if ((controllerAuth && !methodAnonymous) || (!controllerAuth && methodAuth))
                {
                    operation.Security.Add(new() { { new("bearer", context.Document), [] } });
                }
            }
        }
    }
}
