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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OpenApi.Models;
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
            if (operation.Security == null)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }

            ControllerActionDescriptor? cad = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (cad != null)
            {
                bool controllerAuth = cad.ControllerTypeInfo.GetCustomAttributes(true).Any(t => t is AuthorizeAttribute);
                bool methodAuth = cad.MethodInfo.GetCustomAttributes(false).Any(t => t is AuthorizeAttribute);
                bool methodAnonymous = cad.MethodInfo.GetCustomAttributes(false).Any(t => t is AllowAnonymousAttribute);

                if ((controllerAuth && !methodAnonymous) || (!controllerAuth && methodAuth))
                {
                    bool controllerApiAuth = cad.ControllerTypeInfo.GetCustomAttributes(true).Any(t => t is AuthorizeAttribute attribute && attribute.Policy == ApiKeyPolicy.Write);
                    bool methodApiAuth = cad.MethodInfo.GetCustomAttributes(true).Any(t => t is AuthorizeAttribute attribute && attribute.Policy == ApiKeyPolicy.Write);
                    OpenApiSecurityScheme securityScheme;
                    if (controllerApiAuth || methodApiAuth)
                    {
                        securityScheme = new()
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apikey" },
                        };
                    }
                    else
                    {
                        securityScheme = new()
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" },
                        };
                    }

                    operation.Security.Add(
                        new()
                        {
                            { securityScheme, Array.Empty<string>() },
                        });
                }
            }
        }
    }
}
