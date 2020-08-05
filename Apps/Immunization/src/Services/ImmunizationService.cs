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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Factories;
    using HealthGateway.Immunization.Models;
    using Hl7.Fhir.Specification;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImmunizationFhirDelegate immunizationDelegate;
        private readonly IPatientDelegate patientDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="immunizationDelegateFactory">The factory to create immunization delegates.</param>
        /// <param name="patientDelegate">The injected patient delegate.</param>
        public ImmunizationService(
            ILogger<ImmunizationService> logger,
            IHttpContextAccessor httpAccessor,
            IImmunizationDelegateFactory immunizationDelegateFactory,
            IPatientDelegate patientDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.immunizationDelegate = immunizationDelegateFactory.CreateInstance();
            this.patientDelegate = patientDelegate;
    }

        /// <inheritdoc/>
        public async Task<IEnumerable<ImmunizationView>> GetImmunizations(string hdid)
        {
            this.logger.LogDebug($"Getting immunization from Immunization Service... {hdid}");
            List<ImmunizationView> immunizations = new List<ImmunizationView>();
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            RequestResult<Patient> patientResult = this.patientDelegate.GetPatient(hdid, jwtString);
            if (patientResult != null &&
                patientResult.ResultStatus == Common.Constants.ResultType.Success &&
                patientResult.ResourcePayload != null)
            {
                ImmunizationRequest request = new ImmunizationRequest()
                {
                    PersonalHealthNumber = patientResult.ResourcePayload.PersonalHealthNumber,
                    DateOfBirth = patientResult.ResourcePayload.Birthdate,
                };

                Hl7.Fhir.Model.Bundle fhirBundle = await this.immunizationDelegate.GetImmunizationBundle(request).ConfigureAwait(true);
                IEnumerable<Hl7.Fhir.Model.Immunization> immmsLiist = fhirBundle.Entry
                                                                                .Where(r => r.Resource is Hl7.Fhir.Model.Immunization)
                                                                                .Select(f => (Hl7.Fhir.Model.Immunization)f.Resource);
                foreach (Hl7.Fhir.Model.Immunization entry in immmsLiist)
                {
                    ImmunizationView immunizationView = new ImmunizationView
                    {
                        Id = entry.Id,
                        Name = entry.VaccineCode.Text,
                        Status = entry.Status.ToString()!,
                        OccurrenceDateTime = System.DateTime.Parse(entry.Occurrence.ToString()!, CultureInfo.InvariantCulture),
                    };
                    foreach (Hl7.Fhir.Model.Coding code in entry.VaccineCode.Coding)
                    {
                        ImmunizationAgent immunizationAgent = new ImmunizationAgent
                        {
                            Code = code.Code,
                            Name = code.Display,
                        };
                        immunizationView.ImmunizationAgents.Add(immunizationAgent);
                    }

                    immunizations.Add(immunizationView);
                }
            }
            else
            {
                if (patientResult != null)
                {
                    this.logger.LogError($"Unable to lookup Patient information {patientResult.ResultMessage}");
                }
                else
                {
                    this.logger.LogError($"Patient returned null Request");
                }
            }

            this.logger.LogDebug($"Finished getting immunization records {immunizations.Count}");
            return immunizations;
        }
    }
}
