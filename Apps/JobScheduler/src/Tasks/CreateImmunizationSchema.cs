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
namespace Healthgateway.JobScheduler.Tasks
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Queries the Health Gateway DB for all current users with emails.
    /// Queues NotificationSettings job for each.
    /// </summary>
    public class CreateImmunizationSchema : IOneTimeTask
    {
        private readonly ILogger<CreateImmunizationSchema> logger;
        private readonly IWalletIssuerDelegate walletIssuerDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateImmunizationSchema"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="walletIssuerDelegate">The injected wallet issuer service.</param>
        public CreateImmunizationSchema(
                ILogger<CreateImmunizationSchema> logger,
                IWalletIssuerDelegate walletIssuerDelegate)
        {
            this.logger = logger;
            this.walletIssuerDelegate = walletIssuerDelegate;
        }

        /// <summary>
        /// Runs the task that needs to be done for the IOneTimeTask.
        /// </summary>
        public void Run()
        {
            SchemaRequest schema = new ()
            {
                SchemaName = this.walletIssuerDelegate.WalletIssuerConfig.SchemaName,
                SchemaVersion = this.walletIssuerDelegate.WalletIssuerConfig.SchemaVersion,
            };

            foreach (var property in typeof(ImmunizationCredentialPayload).GetProperties())
            {
                schema.Attributes?.Add(property.Name);
            }

            this.logger.LogInformation($"Performing Task {this.GetType().Name}");
            RequestResult<SchemaResponse> schemaResponse = Task.Run(async () => await this.walletIssuerDelegate.CreateSchemaAsync(schema).ConfigureAwait(true)).Result;
            if (schemaResponse.ResourcePayload != null)
            {
                Task.Run(async () => await this.walletIssuerDelegate.CreateCredentialDefinitionAsync(schemaResponse.ResourcePayload.SchemaId).ConfigureAwait(true));
            }

            this.logger.LogInformation($"Task {this.GetType().Name} has completed");
        }
    }
}
