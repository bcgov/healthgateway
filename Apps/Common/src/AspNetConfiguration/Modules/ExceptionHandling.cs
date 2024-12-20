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
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.ErrorHandling.ExceptionHandlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides ASP.Net Services for exception handling.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ExceptionHandling
    {
        /// <summary>
        /// Adds and configures the services required to use problem details.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public static void ConfigureProblemDetails(IServiceCollection services)
        {
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<HealthGatewayExceptionHandler>();
            services.AddExceptionHandler<DefaultExceptionHandler>();
            services.AddProblemDetails();
        }

        /// <summary>
        /// Configures the app to use problem details middleware.
        /// </summary>
        /// <param name="app">The application builder to use.</param>
        public static void UseProblemDetails(IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages(); // replaces empty HTTP error responses with problem details responses
        }
    }
}
