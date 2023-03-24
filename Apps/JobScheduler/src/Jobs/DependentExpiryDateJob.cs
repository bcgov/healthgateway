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
namespace HealthGateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Updates expiry date for dependents in resource delegate table.
    /// </summary>
    public class DependentExpiryDateJob
    {
        private const string JobKey = "DependentExpiryDate";
        private const string MaxDependentAgeKey = "MaxDependentAge";
        private const string MaxRowsToProcessKey = "MaxRowsToProcess";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private readonly int maxDependentAge;
        private readonly int maxRowsToProcess;

        private readonly ILogger<DependentExpiryDateJob> logger;
        private readonly GatewayDbContext dbContext;
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentExpiryDateJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="patientService">The patient service to use.</param>
        public DependentExpiryDateJob(
            IConfiguration configuration,
            ILogger<DependentExpiryDateJob> logger,
            GatewayDbContext dbContext,
            IPatientService patientService)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.patientService = patientService;
            this.maxDependentAge = configuration.GetValue<int>($"{JobKey}:{MaxDependentAgeKey}");
            this.maxRowsToProcess = configuration.GetValue<int>($"{JobKey}:{MaxRowsToProcessKey}");
        }

        /// <summary>
        /// Updates resource delegate's expiry date based on date of birth returned from patient service.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            this.logger.LogInformation("Max rows to process: {MaxRowsToProcess}", this.maxRowsToProcess);
            List<ResourceDelegate> resourceDelegates = this.dbContext.ResourceDelegate.Where(rd => rd.ExpiryDate == null).Take(this.maxRowsToProcess).ToList();

            foreach (ResourceDelegate resourceDelegate in resourceDelegates)
            {
                Task<RequestResult<PatientModel>> patientResult = this.patientService.GetPatient(resourceDelegate.ResourceOwnerHdid);
                if (patientResult.Result.ResultStatus != ResultType.Error && patientResult.Result.ResourcePayload != null)
                {
                    DateOnly expiryDate = DateOnly.FromDateTime(patientResult.Result.ResourcePayload.Birthdate.AddYears(this.maxDependentAge));
                    resourceDelegate.ExpiryDate = expiryDate;
                }
                else
                {
                    this.logger.LogError("Unable to find patient record for dependent Hdid: {ResourceOwnerHdid}", resourceDelegate.ResourceOwnerHdid);
                }
            }

            foreach (ResourceDelegate resourceDelegate in resourceDelegates)
            {
                this.dbContext.Update(resourceDelegate);
            }

            this.dbContext.SaveChanges();

            this.logger.LogInformation("Completed updating {ResourceDelegatesCount}", resourceDelegates.Count);
        }
    }
}
