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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Xunit;


    public class TRPMessageParser_Test
    {
        private readonly IHNMessageParser<List<MedicationStatement>> parser;
        private readonly string phn = "0000123456789";
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
            string dateTime = this.getDateTime().ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = this.getDateTime().ToString("yyMMdd", this.culture);

            HNMessage<string> request = this.parser.CreateRequestMessage(phn, userId, ipAddress, 101010);

            Assert.False(request.IsError);
            Assert.StartsWith($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId.ToUpper()}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r", request.Message);
            Assert.Contains($"ZCA|{hnClientConfig.ZCA.BIN}|{hnClientConfig.ZCA.CPHAVersionNumber}|{hnClientConfig.ZCA.TransactionCode}|{hnClientConfig.ZCA.SoftwareId}|{hnClientConfig.ZCA.SoftwareVersion}", request.Message);
            Assert.Contains($"ZZZ|TRP||{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}", request.Message);
            Assert.Contains($"ZCB|{hnClientConfig.ZCB.PharmacyId}|{date}|{traceNumber}", request.Message);
            Assert.EndsWith($"ZCC||||||||||{phn}|\r", request.Message);
        }

        [Fact]
        public void ShouldParseInvalidMessage()
        {
            string expectedErrorMessage = "SOME ERROR";
            string dateTime = this.getDateTime().ToString("yyyy/MM/dd HH:mm:ss", this.culture);
            string date = this.getDateTime().ToString("yyMMdd", this.culture);
            StringBuilder sb = new StringBuilder();
            sb.Append($"MSH|^~\\&|{hnClientConfig.SendingApplication}|{hnClientConfig.SendingFacility}|{hnClientConfig.ReceivingApplication}|{hnClientConfig.ReceivingFacility}|{dateTime}|{userId}:{ipAddress}|ZPN|{traceNumber}|{hnClientConfig.ProcessingID}|{hnClientConfig.MessageVersion}\r");
            sb.Append($"ZCB|BCXXZZZYYY|{date}|{traceNumber}\r");
            sb.Append($"ZZZ|TRP|1|{traceNumber}|{hnClientConfig.ZZZ.PractitionerIdRef}|{hnClientConfig.ZZZ.PractitionerId}||{expectedErrorMessage}\r");
            sb.Append($"ZCC||||||||||{phn}\r");

            HNMessage<List<MedicationStatement>> actual = this.parser.ParseResponseMessage(sb.ToString());

            Assert.True(actual.IsError);
            Assert.Equal(expectedErrorMessage, actual.Error);
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
            MedicationStatement expectedMedicationStatement = new MedicationStatement()
            {
                DateEntered = DateTime.Today,
                Directions = "DIRECTIONS",
                DispensedDate = DateTime.Today.AddDays(-1),                
                PharmacyId = "BC123456",
                PractitionerSurname = "DR.GATEWAY",
                PrescriptionStatus = 'F',
                PrescriptionIdentifier = "5790",
                Medication = new Medication(){
                    BrandName = null,
                    DIN = "123456",
                    MaxDailyDosage = 1.555f,
                    DrugDiscontinuedDate = null,
                    GenericName = "CLARITHROMYCIN",
                    Manufacturer = "BGP PHARMA ULC",
                    DosageUnit = "MG",
                    Dosage = 500.0f,
                    Form = "TABLET",
                    Quantity = 55.5f,
                }
            };

            string genericName = "CLARITHROMYCIN                BGP PHARMA ULC 500 MG    TABLET";

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

            sb.Append($"{expectedMedicationStatement.Medication.DIN}^");
            sb.Append($"{genericName}^N^");
            sb.Append($"{expectedMedicationStatement.Medication.Quantity.ToString("F1").Replace(".", string.Empty)}^");
            sb.Append($"{expectedMedicationStatement.Medication.MaxDailyDosage.ToString("F3").Replace(".", string.Empty)}^^^");
            sb.Append($"{expectedMedicationStatement.PrescriptionStatus}^");
            sb.Append($"{expectedMedicationStatement.DispensedDate.ToString("yyyyMMdd")}^CACI^P1^XXALE^");
            sb.Append($"{expectedMedicationStatement.PractitionerSurname}^");
            sb.Append($"{expectedMedicationStatement.Medication.DrugDiscontinuedDate?.ToString("yyyyMMdd")}^^");
            sb.Append($"{expectedMedicationStatement.Directions}^^^^");
            sb.Append($"{expectedMedicationStatement.DateEntered?.ToString("yyyyMMdd")}^");
            sb.Append($"{expectedMedicationStatement.PharmacyId}^Y^");
            sb.Append($"{expectedMedicationStatement.PrescriptionIdentifier}^");

            // Other prescriptions
            sb.Append("~ZPB3^572349^COLCHICINE                    ODAN LABS      0.6 MG    TABLET^N^70^1000^^^D^20190129^NI^P1^XXALE^PHARMACISTWITHLONGCHARACTERNAME#035^20190129^PR^ADAPTED RX AND DISCONTINUED^REASON FOR DISCONTINUATION^P1^XXALE^20190129^QAERXPP^Y^5788^HIGH^CHGD");
            sb.Append("~ZPB3^294322^ALLOPURINOL                   APOTEX INC     300 MG    TABLET^N^1200^4000^^^D^20190116^^91^XXALT^ABLEBODIED^20190116^PH^PRESCRIPTION # 16^REASON FOR DISCONTINUATION^P1^XXAKZ^20190116^BC000000QA^N");

            HNMessage<List<MedicationStatement>> actual = this.parser.ParseResponseMessage(sb.ToString());

            Assert.False(actual.IsError);
            Assert.Equal(3, actual.Message.Count);
            Assert.True(expectedMedicationStatement.IsDeepEqual(actual.Message.First()));
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
