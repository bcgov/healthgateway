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
        private readonly ICommunicationEmailDelegate commEmailDelegate;
        private readonly IEmailDelegate emailDelegate;
        private readonly int retryFetchSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="communicationDelegate">The Communication delegate to use.</param>
        /// <param name="commEmailDelegate">The Communication Email delegate to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public CommunicationJob(IConfiguration configuration, ILogger<CommunicationJob> logger, ICommunicationDelegate communicationDelegate, ICommunicationEmailDelegate commEmailDelegate, IEmailDelegate emailDelegate)
        {
            Contract.Requires((configuration != null) && (emailDelegate != null));
            this.configuration = configuration!;
            this.logger = logger;
            this.communicationDelegate = communicationDelegate!;
            this.commEmailDelegate = commEmailDelegate!;
            this.emailDelegate = emailDelegate!;

            IConfigurationSection section = this.configuration.GetSection("EmailJob");
            this.retryFetchSize = section.GetValue<int>("MaxRetryFetchSize", 250);
        }

        /// <inheritdoc />
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void CreateCommunicationEmailsForNewCommunications()
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
                        DateTime? createdOnOrAfterFilter = null;
                        List<UserProfile> usersToSendCommEmails;
                        do
                        {
                            usersToSendCommEmails = this.commEmailDelegate.GetActiveUserProfilesWithoutCommEmailByCommunicationIdAndByCreatedOnOrAfter(comm.Id, createdOnOrAfterFilter, this.retryFetchSize);
                            foreach (UserProfile profile in usersToSendCommEmails)
                            {
                                // Idea for Improvement: we can look into a lib for Bulk Insert / Update using .NET Core for Postgres (if needed).
                                // Insert a new Email record into db.
                                Email email = new Email()
                                {
                                    From = EmailConsts.FromEmailAddressHGDonotreply,
                                    To = profile.Email,
                                    Subject = comm.Subject,
                                    Body = comm.Text,
                                    FormatCode = EmailFormat.HTML,
                                    Priority = comm.Priority,
                                };
                                Guid createdEmailId = this.emailDelegate.InsertEmail(email, true);

                                // Inser a new CommunicationEmail record into 
                                CommunicationEmail commEmail = new CommunicationEmail()
                                {
                                    EmailId = createdEmailId,
                                    UserProfileHdId = profile.HdId,
                                    CommunicationId = comm.Id,
                                };
                                this.commEmailDelegate.Add(commEmail, true);
                            }

                            // Set the createdOnOrAfterFilter for the next query retrieving the next chunk of user profiles.
                            createdOnOrAfterFilter = usersToSendCommEmails[usersToSendCommEmails.Count - 1].CreatedDateTime;
                        }
                        while (usersToSendCommEmails.Count > 0 && usersToSendCommEmails.Count == this.retryFetchSize); // keep looping when the above query returns the max return rows (or user profiles).
                    }
                    catch (Exception e)
                    {
                        // log the exception as a warning but we can continue
                        this.logger.LogWarning($"Error while creating new emails and communication email records for Email Communication - skipping for now\n{e.ToString()}");
                    }
#pragma warning restore CA1031 // Restore warnings.
                }
            }

            this.logger.LogDebug($"Finished sending low priority emails. {JsonConvert.SerializeObject(communications)}");
        }

        private void TestCreateNewComm()
        {
            Communication comm = new Communication()
            {
                CommunicationStatusCode = CommunicationStatus.New,
                CommunicationTypeCode = CommunicationType.Email,
                Subject = "Testing Communication Job 01",
                Text = @"<html>
                            <body>
                            <h1>Enter the main heading, usually the same as the title.</h1>
                            <p>Be <b>bold</b> in stating your key points. Put them in a list: </p>
                            <ul>
                            <li>The first item in your list</li>
                            <li>The second item; <i>italicize</i> key words</li>
                            </ul>
                            <p>Improve your image by including an image. </p>
                            </body>
                            </html>",
                Priority = EmailPriority.Standard,
            };
            this.communicationDelegate.Add(comm, true);
        }
    }
}
