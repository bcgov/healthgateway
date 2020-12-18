namespace HealthGateway.Patient.Test.Service
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
    public class SwaggerTest
    {
        /// <summary>
        /// Should Apply.
        /// </summary>
        [Fact]
        public void ShouldApply()
        {
            AuthenticationRequirementsOperationFilter filter = new AuthenticationRequirementsOperationFilter();
            OpenApiOperation openApiOperation = new OpenApiOperation();
            openApiOperation.Security = new List<OpenApiSecurityRequirement>();

            var actionDescriptor = new ControllerActionDescriptor()
            {
                ActionName = "index",
                ControllerName = "swagger",
                ControllerTypeInfo = typeof(MockMethodInfo).GetTypeInfo(),
                MethodInfo = typeof(MockMethodInfo).GetMethod("MockMethod"),
            };
            ApiDescription apiDescription = new ApiDescription()
            {
                ActionDescriptor = actionDescriptor,
            };
            OperationFilterContext filterContext = new OperationFilterContext(apiDescription, null, null, null);
            filter.Apply(openApiOperation, filterContext);
            Assert.Equal(1, openApiOperation.Security.Count);
        }

        /// <summary>
        /// ControllerActionDescriptor is null.
        /// </summary>
        [Fact]
        public void ShouldNullControllerActionDescriptor()
        {
            AuthenticationRequirementsOperationFilter filter = new AuthenticationRequirementsOperationFilter();
            OpenApiOperation openApiOperation = new OpenApiOperation();
            ApiDescription apiDescription = new ApiDescription();
            OperationFilterContext filterContext = new OperationFilterContext(apiDescription, null, null, null);
            filter.Apply(openApiOperation, filterContext);
            Assert.Equal(0, openApiOperation.Security.Count);
        }
    }
}
