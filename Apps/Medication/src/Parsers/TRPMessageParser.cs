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
namespace HealthGateway.Medication.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using HealthGateway.Medication.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public class TRPMessageParser : IHNMessageParser<MedicationStatement>
    {
        private const string TRACE = "101010";
        private readonly IConfiguration configuration;
        private readonly string timeZoneId;
        private readonly HNClientConfiguration hnClientConfig = new HNClientConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="TRPMessageParser"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public TRPMessageParser(IConfiguration config)
        {
            this.configuration = config;
            this.configuration.GetSection("HNClient").Bind(this.hnClientConfig);

            this.timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this.hnClientConfig.WindowsTimeZoneId : this.hnClientConfig.UnixTimeZoneId;
        }

        /// <inheritdoc/>
        public HNMessage CreateRequestMessage(string phn, string userId, string ipAddress)
        {
            if (phn.Length < 13)
            {
                phn = phn.PadLeft(13, '0');
            }

            HNClientConfiguration hnClientConfig = new HNClientConfiguration();
            this.configuration.GetSection("HNClient").Bind(hnClientConfig);

            // OUR FACILITY 'BC01001249'
            // Raw HL7 sample message.
            // MSH|^~\\&|GATEWAY|BC01001249|PNP|CPA|2019/09/24 13:49:29|1001:127.0.0.1|ZPN|248875|D|2.1||
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
            DateTime utc = DateTime.UtcNow;
            TimeZoneInfo localtz = TimeZoneInfo.FindSystemTimeZoneById(this.timeZoneId);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, localtz);
            m.AddSegmentMSH(
                this.hnClientConfig.SendingApplication,
                this.hnClientConfig.SendingFacility,
                this.hnClientConfig.ReceivingApplication,
                this.hnClientConfig.ReceivingFacility,
                $"{userId}:{ipAddress}",
                $"{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00",
                TRACE,
                this.hnClientConfig.ProcessingID,
                this.hnClientConfig.MessageVersion);
            m.SetValue("MSH.7", string.Format(culture, "{0:yyyy/MM/dd HH:mm:ss}", localDateTime)); // HNClient specific date format
            m.SetValue("MSH.9", HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE); // HNClient doesn't recognize event types (removes ^00 from message type)

            // ZZZ - Transaction Control
            Segment zzz = new Segment(HNClientConfiguration.SEGMENT_ZZZ, encoding);
            zzz.AddNewField(HNClientConfiguration.PATIENT_PROFILE_TRANSACTION_ID); // Transaction ID
            zzz.AddNewField(string.Empty); // Response Status (Empty)
            zzz.AddNewField(TRACE); // Trace Number
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerIdRef); // Practitioner ID Reference
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerId); // Practitioner ID
            m.AddNewSegment(zzz);

            // ZCA - Claims Standard Message Header
            Segment zca = new Segment(HNClientConfiguration.SEGMENT_ZCA, encoding);
            zca.AddNewField(this.hnClientConfig.ZCA.BIN); // BIN
            zca.AddNewField(this.hnClientConfig.ZCA.CPHAVersionNumber); // CPHA Version Number
            zca.AddNewField(this.hnClientConfig.ZCA.TransactionCode); // Transaction Code
            zca.AddNewField(this.hnClientConfig.ZCA.SoftwareId); // Provider Software ID
            zca.AddNewField(this.hnClientConfig.ZCA.SoftwareVersion); // Provider Software Version
            m.AddNewSegment(zca);

            // ZCB - Provider Information
            Segment zcb = new Segment(HNClientConfiguration.SEGMENT_ZCB, encoding);
            zcb.AddNewField(this.hnClientConfig.ZCB.PharmacyId); // Pharmacy ID Code
            zcb.AddNewField(localDateTime.ToString("yyMMdd", culture)); // Provider Transaction Date
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
            List<MedicationStatement> medicationStatements = new List<MedicationStatement>();
            if (string.IsNullOrEmpty(hl7Message))
            {
                throw new ArgumentNullException(nameof(hl7Message));
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

            if (zpb3 != null)
            {
                // Split value into multiple records
                string[] records = zpb3.Value.Split('~');
                foreach (string record in records)
                {
                    string[] fields = record.Split('^');
                    MedicationStatement medicationStatement = new MedicationStatement();
                    Medication medication = new Medication();
                    medication.DIN = fields[1]; // DIN
                    medication.ParseHL7V2Name(fields[2]); // Generic Name

                    // fields[3]; // Same Store Indicator
                    medication.Quantity = float.Parse(fields[4], CultureInfo.CurrentCulture) / 10; // Quantity
                    medication.Dosage = float.Parse(fields[5], CultureInfo.CurrentCulture) / 1000; // Max Daily Dosage

                    // fields[6]; // Ingredient Code
                    // fields[7]; // Ingredient Name
                    medicationStatement.PrescriptionStatus = fields[8].ToCharArray()[0]; // RX Status
                    medicationStatement.DispensedDate = DateTime.ParseExact(fields[9], "yyyyMMdd", CultureInfo.CurrentCulture); // Date Dispensed

                    // fields[10]; // Intervention Code
                    // fields[11]; // Practitioner ID Reference
                    // fields[12]; // Practitioner ID
                    medicationStatement.PractitionerSurname = fields[13]; // Practitioner Family Name
                    medication.DrugDiscontinuedDate = string.IsNullOrEmpty(fields[14]) ?
                        (DateTime?)null : DateTime.ParseExact(fields[14], "yyyyMMdd", CultureInfo.CurrentCulture); // Drug Discontinued Date

                    // fields[15]; // Drug Discontinued Source
                    medicationStatement.Directions = fields[16]; // Directions

                    // fields[17]; // Comment Text
                    // fields[18]; // Practitioner ID Reference (Duplicated ?)
                    // fields[19]; // Practitioner ID (Duplicated ?)
                    medicationStatement.DateEntered = string.IsNullOrEmpty(fields[20]) ?
                        (DateTime?)null : DateTime.ParseExact(fields[20], "yyyyMMdd", CultureInfo.CurrentCulture); // Date Entered

                    medicationStatement.PharmacyId = fields[21]; // Pharmacy ID

                    if (fields.Length > 23)
                    {
                        // fields[22].ToString(); // Adaptation Indicator                    
                        medicationStatement.PrescriptionIdentifier = fields[23].ToString(); // PharmaNet Prescription Identifier
                        // fields[24].ToString(); // MMI Codes
                        // fields[25].ToString(); // Clinical Service Codes
                    }
                    medicationStatements.Add(medicationStatement);
                }
            }

            return medicationStatements;
        }
    }
}
