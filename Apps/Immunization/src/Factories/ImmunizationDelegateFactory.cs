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
namespace HealthGateway.Immunization.Factories
{
    using System;
    using HealthGateway.Immunization.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Creates the Immunization Delegate dynamically.
    /// </summary>
    public class ImmunizationDelegateFactory : IImmunizationDelegateFactory
    {
        private const string DefaultInstance = "HealthGateway.Immunization.Delegates.ImmunizationFhirDelegate";
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationDelegateFactory"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="serviceProvider">The injected service provider.</param>
        public ImmunizationDelegateFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates an instance of IImunizationFhirDelegate via configuration.
        /// </summary>
        /// <returns>The IImunizationFhirDelegate.</returns>
        public IImmunizationFhirDelegate CreateInstance()
        {
            string typeStr = this.configuration.GetValue<string>("ImmunizationDelegate", DefaultInstance);
            Type? type = Type.GetType(typeStr);
            IImmunizationFhirDelegate instance = (IImmunizationFhirDelegate)ActivatorUtilities.CreateInstance(this.serviceProvider, type);
            return instance;
        }
    }
}
