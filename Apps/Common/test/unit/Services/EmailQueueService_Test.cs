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
namespace HealthGateway.CommonTests.Services
{
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class Email_Test
    {
        [Fact]
        public void ShouldProcessTemplate()
        {
            string emailTo = "test@test.com";
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("SUBJECT", "Test Subject");
            d.Add("PARM1", "PARM1");
            d.Add("PARM2", "PARM2");
            d.Add("PARM3", "PARM3");
            EmailTemplate template = new EmailTemplate()
            {
                Subject = "Subject=${SUBJECT}",
                Body = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}",
                FormatCode = EmailFormat.Text,
                Priority = EmailPriority.Standard,
            };
            Email expected = new Email()
            {
                To = emailTo,
                Subject = "Subject=Test Subject",
                Body = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3"
            };
            IEmailQueueService emailService = new EmailQueueService(
                new Mock<ILogger<EmailQueueService>>().Object,
                new Mock<IEmailDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<IWebHostEnvironment>().Object);
            Email actual = emailService.ProcessTemplate(emailTo, template, d);
            expected.Id = actual.Id;
            Assert.True(expected.IsDeepEqual(actual));
        }
    }
}
