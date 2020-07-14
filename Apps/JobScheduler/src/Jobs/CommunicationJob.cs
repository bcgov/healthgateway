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
namespace Healthgateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Hangfire;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class CommunicationJob : ICommunicationJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly IConfiguration configuration;
        private readonly ILogger<CommunicationJob> logger;
        private readonly ICommunicationDelegate communicationDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IEmailDelegate emailDelegate;
        private readonly string host;
        private readonly int port;
        private readonly int retryFetchSize;
        private readonly int maxRetries;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="communicationDelegate">The communication delegate to use.</param>
        /// <param name="userProfileDelegate">The user Profile delegate to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public CommunicationJob(IConfiguration configuration, ILogger<CommunicationJob> logger, ICommunicationDelegate communicationDelegate, IUserProfileDelegate userProfileDelegate, IEmailDelegate emailDelegate)
        {
            Contract.Requires((configuration != null) && (emailDelegate != null));
            this.configuration = configuration!;
            this.logger = logger;
            this.communicationDelegate = communicationDelegate!;
            this.userProfileDelegate = userProfileDelegate!;
            this.emailDelegate = emailDelegate!;
            IConfigurationSection section = configuration!.GetSection("Smtp");
            this.host = section.GetValue<string>("Host");
            this.port = section.GetValue<int>("Port");

            section = configuration.GetSection("EmailJob");
            this.maxRetries = section.GetValue<int>("MaxRetries", 9);
            this.retryFetchSize = section.GetValue<int>("MaxRetryFetchSize", 250);
        }

        /// <inheritdoc />
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void CreateCommunicationEmailsForNewCommunication()
        {
            this.logger.LogDebug($"Creating emails & communication emails...");
            List<Communication> communications = this.communicationDelegate.GetCommunicationsByTypeAndStatusCode(CommunicationType.Email, CommunicationStatus.New);

            if (communications.Count > 0)
            {
                this.logger.LogInformation($"Found {communications.Count} communications which are in New status.");
                foreach (Communication comm in communications)
                {
#pragma warning disable CA1031 //We want to catch exception.
                    try
                    {
                        // this.SendEmail(comm);
                    }
                    catch (Exception e)
                    {
                        // log the exception as a warning but we can continue
                        this.logger.LogWarning($"Error while sending - skipping for now\n{e.ToString()}");
                    }
#pragma warning restore CA1031 // Restore warnings.
                }
            }

            this.logger.LogDebug($"Finished sending low priority emails. {JsonConvert.SerializeObject(communications)}");
        }

        /// <inheritdoc />
        private void SendEmail(Guid emailId)
        {
            this.logger.LogTrace($"Sending email... {emailId}");
            Email email = this.emailDelegate.GetNewEmail(emailId);
            if (email != null)
            {
                // this.SendEmail(email);
                this.logger.LogInformation($"Todo Leo.");
            }
            else
            {
                this.logger.LogInformation($"Email {emailId} was not returned from DB, skipping.");
            }

            this.logger.LogDebug($"Finished sending email. {JsonConvert.SerializeObject(email)}");
        }

        private static MimeMessage PrepareMessage(Communication comm, List<MailboxAddress> emailBccList)
        {
            MimeMessage msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Health Gateway", EmailConsts.FromEmailAddressHGDonotreply));
            msg.To.Add(new MailboxAddress(EmailConsts.ToEmailAddressUndisclosedRecipients));
            msg.Bcc.AddRange(emailBccList);

            msg.Subject = comm.Subject;
            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = comm.Text,
            };
            return msg;
        }

    }
}
