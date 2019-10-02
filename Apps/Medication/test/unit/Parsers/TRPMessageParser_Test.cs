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
    using HealthGateway.MedicationService.Models;
    using HealthGateway.MedicationService.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Xunit;


    public class TRPMessageParser_Test
    {
        private readonly IHNMessageParser<Prescription> parser;
        private readonly string phn = "123456789";
        private readonly string userId = "test";
        private readonly string ipAddress = "127.0.0.1";
        private readonly string traceNumber = "101010";
        private readonly CultureInfo culture;
        private readonly HNClientConfiguration hnClientConfig = new HNClientConfiguration();

        public TRPMessageParser_Test()
        {
            this.culture = CultureInfo.CreateSpecificCulture("en-CA");
            this.culture.DateTimeFormat.DateSeparator = "/";

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
            configuration.GetSection("HNClient").Bind(hnClientConfig);
            this.parser = new TRPMessageParser(configuration);
        }

        [Fact]
        public void ShouldCreateRequestMessage()
        {
            string dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = DateTime.Now.ToString("yyMMdd", this.culture);

            HNMessage request = this.parser.CreateRequestMessage(phn, userId, ipAddress);

            Assert.False(request.IsErr);
            Assert.StartsWith($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r", request.Message);
            Assert.Contains($"ZCA|{hnClientConfig.ZCA.BIN}|{hnClientConfig.ZCA.CPHAVersionNumber}|{hnClientConfig.ZCA.TransactionCode}|{hnClientConfig.ZCA.SoftwareId}|{hnClientConfig.ZCA.SoftwareVersion}", request.Message);
            Assert.Contains($"ZZZ|TRP||{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}", request.Message);
            Assert.Contains($"ZCB|{hnClientConfig.ZCB.PharmacyId}|{date}|{traceNumber}", request.Message);
            Assert.EndsWith($"ZCC||||||||||{phn}|\r", request.Message);
        }

        [Fact]
        public void ShouldParseResponseMessage()
        {
            Prescription expectedPrescription = new Prescription()
            {
                BrandName = null,
                DateEntered = DateTime.Today,
                DIN = "123456",
                Directions = "DIRECTIONS",
                DispensedDate = DateTime.Today.AddDays(-1),
                Dosage = 1.555F,
                DrugDiscontinuedDate = null,
                GenericName = "GENERICNAME   LABNAME   STRENGHT   TYPE",
                PharmacyId = "BC123456",
                PractitionerSurname = "DR.GATEWAY",
                PrescriptionStatus = 'F',
                Quantity = 55.5F
            };

            string dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = DateTime.Now.ToString("yyMMdd", this.culture);
            StringBuilder sb = new StringBuilder();
            sb.Append($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r");
            sb.Append($"ZCB|BCXXZZZYYY|{date}|{traceNumber}\r");
            sb.Append($"ZZZ|TRP|0|{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}||0 Operation successful\r");
            sb.Append($"ZCC||||||||||{phn}\r");
            sb.Append("ZPB");
            
            // ZPB1 medical condition
            sb.Append("|ZPB1^THIS IS A CLINICAL CONDITION*AND IT HAS EXACTLY 56 BYTES^Y^DP^20170330^^^");
            
            // ZPB2 reactions
            sb.Append("|ZPB2^294322^ALLOPURINOL                   APOTEX INC     300 MG    TABLET^^^AE^20190815^*ADE_0427_Adverse Drug Reaction_Possible_Hospitalization^91^XXANR^20190815");

            // ZPB3 prescriptions
            sb.Append("|ZPB3^");

            sb.Append($"{expectedPrescription.DIN}^");
            sb.Append($"{expectedPrescription.GenericName}^N^");
            sb.Append($"{expectedPrescription.Quantity.ToString("F1").Replace(".", string.Empty)}^");
            sb.Append($"{expectedPrescription.Dosage.ToString("F3").Replace(".", string.Empty)}^^^");
            sb.Append($"{expectedPrescription.PrescriptionStatus}^");
            sb.Append($"{expectedPrescription.DispensedDate.ToString("yyyyMMdd")}^CACI^P1^XXALE^");
            sb.Append($"{expectedPrescription.PractitionerSurname}^");
            sb.Append($"{expectedPrescription.DrugDiscontinuedDate?.ToString("yyyyMMdd")}^^");
            sb.Append($"{expectedPrescription.Directions}^^^^");
            sb.Append($"{expectedPrescription.DateEntered?.ToString("yyyyMMdd")}^");
            sb.Append($"{expectedPrescription.PharmacyId}^Y^5790");

            // Other prescriptions
            sb.Append("~ZPB3^572349^COLCHICINE                    ODAN LABS      0.6 MG    TABLET^N^70^1000^^^D^20190129^NI^P1^XXALE^PHARMACISTWITHLONGCHARACTERNAME#035^20190129^PR^ADAPTED RX AND DISCONTINUED^REASON FOR DISCONTINUATION^P1^XXALE^20190129^QAERXPP^Y^5788^HIGH^CHGD");
            sb.Append("~ZPB3^294322^ALLOPURINOL                   APOTEX INC     300 MG    TABLET^N^1200^4000^^^D^20190116^^91^XXALT^ABLEBODIED^20190116^PH^PRESCRIPTION # 16^REASON FOR DISCONTINUATION^P1^XXAKZ^20190116^BC000000QA^N");

            List<Prescription> prescriptions = this.parser.ParseResponseMessage(sb.ToString());

            Assert.Equal(3, prescriptions.Count);
            Assert.True(expectedPrescription.IsDeepEqual(prescriptions.First()));
        }
    }
}
