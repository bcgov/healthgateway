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
    public class TILMessageParser : IHNMessageParser<Pharmacy>
    {
        private const string TRACE = "101010";
        private readonly IConfiguration configuration;
        private readonly string timeZoneId;
        private readonly HNClientConfiguration hnClientConfig = new HNClientConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="TILMessageParser"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public TILMessageParser(IConfiguration config)
        {
            this.configuration = config;
            this.configuration.GetSection("HNClient").Bind(this.hnClientConfig);
            this.timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this.hnClientConfig.WindowsTimeZoneId : this.hnClientConfig.UnixTimeZoneId;
        }

        /// <inheritdoc/>
        public HNMessage CreateRequestMessage(string pharmacyId, string userId, string ipAddress)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-CA");
            culture.DateTimeFormat.DateSeparator = "/";

            HL7Encoding encoding = new HL7Encoding();
            Message m = new Message();

            DateTime utc = DateTime.UtcNow;
            TimeZoneInfo localtz = TimeZoneInfo.FindSystemTimeZoneById(this.timeZoneId);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, localtz);

            // MSH - Message Header
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

            // ZCA - Claims Standard Message Header
            Segment zca = new Segment(HNClientConfiguration.SEGMENT_ZCA, encoding);
            zca.AddNewField(string.Empty); // BIN
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

            // ZPL - Location Information
            Segment zpl = new Segment(HNClientConfiguration.SEGMENT_ZPL, encoding);
            zpl.AddNewField(pharmacyId); // Requested PharmaNet Location Identifier
            zpl.AddNewField(string.Empty); // PharmaNet Location Nam
            zpl.AddNewField(string.Empty); // Location Type Code
            zpl.AddNewField(string.Empty); // Address Line 1
            zpl.AddNewField(string.Empty); // Address Line 2
            zpl.AddNewField(string.Empty); // City or Municipality
            zpl.AddNewField(string.Empty); // Province Code
            zpl.AddNewField(string.Empty); // Postal Code
            zpl.AddNewField(string.Empty); // Country Code
            zpl.AddNewField(string.Empty); // Telecom Type Code
            zpl.AddNewField(string.Empty); // Effective Date
            zpl.AddNewField(string.Empty); // Area Code
            zpl.AddNewField(string.Empty); // Telephone Number
            zpl.AddNewField(string.Empty); // Termination Date
            zpl.AddNewField(this.hnClientConfig.ZPL.TransactionReasonCode); // Transaction Reason Code
            m.AddNewSegment(zpl);

            // ZZZ - Transaction Control
            // ZZZ|TIL||1111|P1|nnnnnnnnnn|||||ZZZ1
            Segment zzz = new Segment(HNClientConfiguration.SEGMENT_ZZZ, encoding);
            zzz.AddNewField(HNClientConfiguration.PHARMACY_PROFILE_TRANSACTION_ID); // Transaction ID
            zzz.AddNewField(string.Empty); // Response Status (Empty)
            zzz.AddNewField(TRACE); // Trace Number
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerIdRef); // Practitioner ID Reference
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerId); // Practitioner ID
            m.AddNewSegment(zzz);

            HNMessage retMessage = new HNMessage();
            retMessage.Message = m.SerializeMessage(false);
            return retMessage;
        }

        /// <inheritdoc/>
        public List<Pharmacy> ParseResponseMessage(string hl7Message)
        {
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
                throw new Exception($"Failed to process the TIL request: {statusMessage}", new Exception(hl7Message));
            }

            // ZPL location information
            Segment zpl = m.Segments(HNClientConfiguration.SEGMENT_ZPL).FirstOrDefault();
            Pharmacy pharmacy = new Pharmacy();

            pharmacy.PharmacyId = zpl.Fields(1).Value; // Requested PharmaNet Location Identifier
            pharmacy.Name = zpl.Fields(2).Value; // PharmaNet Location Name

            // zpl.Fields(3).Value; // Location Type Code
            pharmacy.AddressLine1 = zpl.Fields(4).Value; // Address Line 1
            pharmacy.AddressLine2 = zpl.Fields(5).Value; // Address Line 2
            pharmacy.City = zpl.Fields(6).Value; // City or Municipality
            pharmacy.Province = zpl.Fields(7).Value; // Province Code
            pharmacy.PostalCode = zpl.Fields(8).Value; // Postal Code
            pharmacy.CountryCode = zpl.Fields(9).Value; // Country Code

            // zpl.Fields(10).Value; // Telecom Type Code
            // zpl.Fields(11).Value; // Effective Date
            pharmacy.PhoneNumber = $"{zpl.Fields(12).Value}{zpl.Fields(13).Value}"; // Area Code and Telephone Number

            // zpl.Fields(14).Value; // Termination Date
            // zpl.Fields(1)5.Value; // Transaction Reason Code
            return new List<Pharmacy>(new Pharmacy[] { pharmacy });
        }
    }
}
