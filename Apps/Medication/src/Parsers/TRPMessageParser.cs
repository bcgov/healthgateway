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
namespace HealthGateway.MedicationService.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using HealthGateway.MedicationService.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public class TRPMessageParser : IHNMessageParser<MedicationStatement>
    {
        private const string TRACE = "101010";
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TRPMessageParser"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public TRPMessageParser(IConfiguration config)
        {
            this.configuration = config;
        }

        /// <inheritdoc/>
        public HNMessage CreateRequestMessage(string phn, string userId, string ipAddress)
        {
            HNClientConfiguration hnClientConfig = new HNClientConfiguration();
            this.configuration.GetSection("HNClient").Bind(hnClientConfig);

            // OUR FACILITY 'BC01001239'
            // Raw HL7 sample message.
            // MSH|^~\\&|GATEWAY|BC01001239|PNP|CPA|2019/09/24 13:49:29|1001:127.0.0.1|ZPN|248875|D|2.1||
            // \r
            // ZZZ|TRP||248876|91|XXAPZ
            // \r
            // ZCA|1|70|00|HG|01|
            // \r
            // ZCB|BCXX000034|190819|248876
            // \r
            // ZCC||||||||||0009735353315|
            // \r
            // \r
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-CA");
            culture.DateTimeFormat.DateSeparator = "/";

            HL7Encoding encoding = new HL7Encoding();
            Message m = new Message();

            // MSH - Message Header
            m.AddSegmentMSH(hnClientConfig.SendingApplication, hnClientConfig.SendingFacility, hnClientConfig.ReceivingApplication, hnClientConfig.ReceivingFacility, $"{userId}:{ipAddress}", $"{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00", TRACE, hnClientConfig.ProcessingID, hnClientConfig.MessageVersion);
            m.SetValue("MSH.7", string.Format(culture, "{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); // HNClient specific date format
            m.SetValue("MSH.9", HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE); // HNClient doesn't recognize event types (removes ^00 from message type)

            // ZZZ - Transaction Control
            Segment zzz = new Segment(HNClientConfiguration.SEGMENT_ZZZ, encoding);
            zzz.AddNewField(HNClientConfiguration.PATIENT_PROFILE_TRANSACTION_ID); // Transaction ID
            zzz.AddNewField(string.Empty); // Response Status (Empty)
            zzz.AddNewField(TRACE); // Trace Number
            zzz.AddNewField(hnClientConfig.ZZZ.PractitionerIdRef); // Practitioner ID Reference
            zzz.AddNewField(hnClientConfig.ZZZ.PractitionerId); // Practitioner ID
            m.AddNewSegment(zzz);

            // ZCA - Claims Standard Message Header
            Segment zca = new Segment(HNClientConfiguration.SEGMENT_ZCA, encoding);
            zca.AddNewField(hnClientConfig.ZCA.BIN); // BIN
            zca.AddNewField(hnClientConfig.ZCA.CPHAVersionNumber); // CPHA Version Number
            zca.AddNewField(hnClientConfig.ZCA.TransactionCode); // Transaction Code
            zca.AddNewField(hnClientConfig.ZCA.SoftwareId); // Provider Software ID
            zca.AddNewField(hnClientConfig.ZCA.SoftwareVersion); // Provider Software Version
            m.AddNewSegment(zca);

            // ZCB - Provider Information
            Segment zcb = new Segment(HNClientConfiguration.SEGMENT_ZCB, encoding);
            zcb.AddNewField(hnClientConfig.ZCB.PharmacyId); // Pharmacy ID Code
            zcb.AddNewField(DateTime.Now.ToString("yyMMdd", culture)); // Provider Transaction Date
            zcb.AddNewField(TRACE); // Trace Number
            m.AddNewSegment(zcb);

            // ZCC - Beneficiary Information
            Segment zcc = new Segment(HNClientConfiguration.SEGMENT_ZCC, encoding);
            zcc.AddNewField(string.Empty); // Carrier ID
            zcc.AddNewField(string.Empty); // Group Number or Code
            zcc.AddNewField(string.Empty); // Client ID Number or Code
            zcc.AddNewField(string.Empty); // Patient Code
            zcc.AddNewField(string.Empty); // Patient Date of Birth
            zcc.AddNewField(string.Empty); // Cardholder Identity
            zcc.AddNewField(string.Empty); // Relationship
            zcc.AddNewField(string.Empty); // Patient First Name
            zcc.AddNewField(string.Empty); // Patient Last Name
            zcc.AddNewField(phn); // Provincial Health Care ID
            zcc.AddNewField(string.Empty); // Patient Gender
            m.AddNewSegment(zcc);

            HNMessage retMessage = new HNMessage();
            retMessage.Message = m.SerializeMessage(false);
            return retMessage;
        }

        /// <inheritdoc/>
        public List<MedicationStatement> ParseResponseMessage(string hl7Message)
        {
            List<MedicationStatement> ret = new List<MedicationStatement>();
            if (string.IsNullOrEmpty(hl7Message))
            {
                return ret;
            }

            // Replaces the message type with message type + event so it can correcly parse the message.
            hl7Message = hl7Message.Replace(
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}|",
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00|",
                StringComparison.CurrentCulture);
            Message m = new Message(hl7Message);
            m.ParseMessage();

            // Checks the response status
            Segment zzz = m.Segments(HNClientConfiguration.SEGMENT_ZZZ).FirstOrDefault();
            Field status = zzz.Fields(2); // Status code
            Field statusMessage = zzz.Fields(7); // Status message

            if (status.Value != "0")
            {
                throw new Exception($"Failed to process the TRP request: {statusMessage}", new Exception(hl7Message));
            }

            // ZPB patient history response
            Segment zpb = m.Segments(HNClientConfiguration.SEGMENT_ZPB).FirstOrDefault();

            // ZPB sub segments (fields)
            // ZPB1 clinical information block (clinical condition)
            // ZPB2 reaction information block
            // ZPB3 RX information block

            // Fields index start from 1 not 0.
            Field zpb3 = zpb.Fields(3);

            // Split value into multiple records
            string[] records = zpb3.Value.Split('~');
            foreach (string record in records)
            {
                string[] fields = record.Split('^');
                fields[1].ToString(); // DIN
                fields[2].ToString(); // Generic Name
                fields[3].ToString(); // Same Store Indicator
                fields[4].ToString(); // Quantity
                fields[5].ToString(); // Max Daily Dosage
                fields[6].ToString(); // Ingredient Code
                fields[7].ToString(); // Ingredient Name
                fields[8].ToString(); // RX Status
                fields[9].ToString(); // Date Dispensed
                fields[10].ToString(); // Intervention Code
                fields[11].ToString(); // Practitioner ID Reference
                fields[12].ToString(); // Practitioner ID
                fields[13].ToString(); // Practitioner Family Name
                fields[14].ToString(); // Drug Discontinued Date
                fields[15].ToString(); // Drug Discontinued Source
                fields[16].ToString(); // Directions
                fields[17].ToString(); // Comment Text

                fields[18].ToString(); // Practitioner ID Reference (Duplicated ?)
                fields[19].ToString(); // Practitioner ID (Duplicated ?)

                fields[20].ToString(); // Date Entered
                fields[21].ToString(); // Pharmacy ID
                fields[22].ToString(); // Adaptation Indicator
                if (fields.Length > 23)
                {
                    fields[23].ToString(); // PharmaNet Prescription Identifier
                }

                if (fields.Length > 24)
                {
                    fields[24].ToString(); // MMI Codes
                }

                if (fields.Length > 25)
                {
                    fields[25].ToString(); // Clinical Service Codes
                }
            }

            return new List<MedicationStatement>();
        }
    }
}
