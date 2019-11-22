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
namespace HealthGateway.Medication.Test
{
    using DeepEqual.Syntax;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using Xunit;


    public class TILMessageParser_Test
    {
        private readonly IHNMessageParser<Pharmacy> parser;
        private readonly string pharmacyId = "123456789";
        private readonly string userId = "test";
        private readonly string ipAddress = "127.0.0.1";
        private readonly string traceNumber = "101010";
        private readonly CultureInfo culture;
        private readonly HNClientConfiguration hnClientConfig = new HNClientConfiguration();

        public TILMessageParser_Test()
        {
            this.culture = CultureInfo.CreateSpecificCulture("en-CA");
            this.culture.DateTimeFormat.DateSeparator = "/";

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json").Build();
            configuration.GetSection("HNClient").Bind(hnClientConfig);
            this.parser = new TILMessageParser(
                new Mock<ILogger<TILMessageParser>>().Object,
                configuration);
        }

        [Fact]
        public void ShouldCreateTILRequestMessage()
        {
            string dateTime = this.getDateTime().ToString("yyyy/MM/dd HH:mm:", this.culture); // Skips seconds to avoid time mismatching
            string date = this.getDateTime().ToString("yyMMdd", this.culture);

            HNMessage<string> request = this.parser.CreateRequestMessage(new HNMessageRequest
            {
                PharmacyId = pharmacyId,
                UserId = userId,
                IpAddress = ipAddress,
                TraceId = 101010
            });

            Assert.True(request.Result == HealthGateway.Common.Constants.ResultType.Sucess);
            Assert.StartsWith($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}", request.Message);
            Assert.Contains($"|{userId.ToUpper()}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r", request.Message);
            Assert.Contains($"ZCA||{hnClientConfig.ZCA.CPHAVersionNumber}|{hnClientConfig.ZCA.TransactionCode}|{hnClientConfig.ZCA.SoftwareId}|{hnClientConfig.ZCA.SoftwareVersion}", request.Message);
            Assert.Contains($"ZCB|{hnClientConfig.ZCB.PharmacyId}|{date}|{traceNumber}", request.Message);
            Assert.Contains($"ZPL|{pharmacyId}||||||||||||||{hnClientConfig.ZPL.TransactionReasonCode}\r", request.Message);
            Assert.EndsWith($"ZZZ|TIL||{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}|||||\r", request.Message);
        }

        [Fact]
        public void ShouldParseInvalidTILMessage()
        {
            string expectedErrorMessage = "SOME ERROR";
            string dateTime = this.getDateTime().ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = this.getDateTime().ToString("yyMMdd", this.culture);
            StringBuilder sb = new StringBuilder();
            sb.Append($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r");
            sb.Append($"ZCB|BCXXZZZYYY|{date}|{traceNumber}\r");
            sb.Append($"ZPL|{pharmacyId}||||||||||||||{hnClientConfig.ZPL.TransactionReasonCode}|\r");
            sb.Append($"ZZZ|TIL|1|{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}||{expectedErrorMessage}\r");
            HNMessage<Pharmacy> actual = this.parser.ParseResponseMessage(sb.ToString());

            Assert.True(actual.Result == Common.Constants.ResultType.Error);
            Assert.Equal(expectedErrorMessage, actual.ResultMessage);
            Assert.Null(actual.Message);
        }

        [Fact]
        public void ShouldParseEmptyMessage()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => this.parser.ParseResponseMessage(""));
        }

        [Fact]
        public void ShouldParseResponseMessage()
        {
            Pharmacy expectedPharmacy = new Pharmacy()
            {
                AddressLine1 = "TEST Street",
                AddressLine2 = "ETC",
                City = "Victoria",
                CountryCode = "CA",
                Name = "Gateway",
                PharmacyId = "123456",
                PhoneType = "F",
                PhoneNumber = "2500008888",
                PostalCode = "V0V0X0",
                Province = "BC"
            };

            string dateTime = this.getDateTime().ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = this.getDateTime().ToString("yyMMdd", this.culture);
            StringBuilder sb = new StringBuilder();
            sb.Append($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r");
            sb.Append($"ZCB|BCXXZZZYYY|{date}|{traceNumber}\r");
           
            // ZPL pharmacy info
            sb.Append("ZPL|");
            sb.Append($"{expectedPharmacy.PharmacyId}|");
            sb.Append($"{expectedPharmacy.Name}||");
            sb.Append($"{expectedPharmacy.AddressLine1}|");
            sb.Append($"{expectedPharmacy.AddressLine2}|");
            sb.Append($"{expectedPharmacy.City}|");
            sb.Append($"{expectedPharmacy.Province}|");
            sb.Append($"{expectedPharmacy.PostalCode}|");
            sb.Append($"{expectedPharmacy.CountryCode}|");
            sb.Append($"{expectedPharmacy.PhoneType}||");
            sb.Append($"{expectedPharmacy.PhoneNumber.Substring(0, 3)}|");
            sb.Append($"{expectedPharmacy.PhoneNumber.Substring(3, 7)}|||");
            sb.Append($"{expectedPharmacy.PharmacyId}|\r");

            sb.Append($"ZZZ|TRP|0|{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}||0 Operation successful\r");

            HNMessage<Pharmacy> actual = this.parser.ParseResponseMessage(sb.ToString());

            Assert.True(actual.Result == Common.Constants.ResultType.Sucess);
            Assert.True(expectedPharmacy.IsDeepEqual(actual.Message));
        }

        private DateTime getDateTime()
        {
            DateTime utc = DateTime.UtcNow;
            TimeZoneInfo localtz = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this.hnClientConfig.WindowsTimeZoneId : this.hnClientConfig.UnixTimeZoneId);
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, localtz);

            return local;
        }
    }
}
