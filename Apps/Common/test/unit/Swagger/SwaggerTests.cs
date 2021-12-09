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
namespace HealthGateway.CommonTests.Swagger
{
    using System.Collections.Generic;
    using System.Reflection;
    using HealthGateway.Common.Swagger;
    using HealthGateway.CommonTests.Utils;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Xunit;

    /// <summary>
    /// Unit Tests for Swagger.
    /// </summary>
    public class SwaggerTests
    {
        /// <summary>
        /// Should Apply.
        /// </summary>
        [Fact]
        public void ShouldApply()
        {
            AuthenticationRequirementsOperationFilter filter = new();
            OpenApiOperation openApiOperation = new();
            openApiOperation.Security = new List<OpenApiSecurityRequirement>();

            ControllerActionDescriptor actionDescriptor = new()
            {
                ActionName = "index",
                ControllerName = "swagger",
                ControllerTypeInfo = typeof(MockMethodInfo).GetTypeInfo(),
                MethodInfo = typeof(MockMethodInfo).GetMethod("MockMethod")!,
            };
            ApiDescription apiDescription = new()
            {
                ActionDescriptor = actionDescriptor,
            };
            OperationFilterContext filterContext = new(apiDescription, null, null, null);

            filter.Apply(openApiOperation, filterContext);

            Assert.Equal(1, openApiOperation.Security.Count);
        }

        /// <summary>
        /// ControllerActionDescriptor is null.
        /// </summary>
        [Fact]
        public void ShouldNullControllerActionDescriptor()
        {
            AuthenticationRequirementsOperationFilter filter = new();
            OpenApiOperation openApiOperation = new();
            ApiDescription apiDescription = new();
            OperationFilterContext filterContext = new(apiDescription, null, null, null);

            filter.Apply(openApiOperation, filterContext);

            Assert.Equal(0, openApiOperation.Security.Count);
        }
    }
}
