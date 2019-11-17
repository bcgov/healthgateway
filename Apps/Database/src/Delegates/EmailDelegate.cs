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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc />
    public class EmailDelegate : IEmailDelegate
    {
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public EmailDelegate(GatewayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public Email GetEmail(Guid emailId)
        {
            return this.dbContext.Find<Email>(emailId);
        }

        /// <inheritdoc />
        public Guid InsertEmail(Email email)
        {
            Contract.Requires(email != null);
            this.dbContext.Add<Email>(email);
            this.dbContext.SaveChanges();
            return email.Id;
        }

        /// <inheritdoc />
        public void UpdateEmail(Email email)
        {
            Contract.Requires(email != null);
            this.dbContext.Update<Email>(email);
            try
            {
                this.dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException e)
            {

            }

        }

        /// <inheritdoc />
        public EmailTemplate GetEmailTemplate(string templateName)
        {
            EmailTemplate emailTemplate = this.dbContext.EmailTemplate.Where(p => p.Name == templateName)
                             .FirstOrDefault<EmailTemplate>();
            return emailTemplate;
        }
    }
}
