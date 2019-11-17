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
namespace Healthgateway.Hangfire.Delegates
{
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;

    public class SmtpDelegate : ISmtpDelegate
    {
        private string host { get; set; }
        private int port { get; set; }

        public int Ok
        {
            get => (int)SmtpStatusCode.Ok;
        }

        private readonly ILogger<SmtpDelegate> logger;
        private readonly IConfiguration configuration;

        public SmtpDelegate(ILogger<SmtpDelegate> logger, IConfiguration configuration)
        {
            this.configuration = configuration;
            IConfigurationSection section = configuration.GetSection("Smtp");
            this.host = section.GetValue<string>("Host");
            this.port = section.GetValue<int>("Port");
        }

        public int SendEmail(Email email)
        {
            int retCode = (int)SmtpStatusCode.Ok;
            Contract.Requires(email != null);
            MimeMessage msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Health Gateway", email.From));
            msg.To.Add(new MailboxAddress(email.To));
            msg.Subject = email.Subject;
            msg.Body = new TextPart(email.FormatCode == EmailFormat.HTML ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
            {
                Text = email.Body,
            };
            using (SmtpClient smtpClient = new SmtpClient())
            {
                try
                {
                    smtpClient.Connect(this.host, this.port);
                    try
                    {
                        smtpClient.Send(msg);
                    }
                    catch(SmtpCommandException e)
                    {
                        retCode = (int)e.StatusCode;
                    }
                    smtpClient.Disconnect(true);
                }
                catch(SmtpCommandException e)
                {
                    retCode = (int)e.StatusCode;
                }
            }
            return retCode;
        }
    }
}
