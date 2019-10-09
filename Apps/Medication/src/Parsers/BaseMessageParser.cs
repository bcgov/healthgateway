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
    /// Base class of HNClient message parser.
    /// </summary>
    public abstract class BaseMessageParser<T> : IHNMessageParser<T>
    {
        protected const string TRACE = "101010";
        protected readonly IConfiguration configuration;
        protected readonly HNClientConfiguration hnClientConfig;
        protected readonly HL7Encoding encoding;
        private readonly TimeZoneInfo localTimeZone;
        private readonly CultureInfo culture;

        protected BaseMessageParser(IConfiguration config)
        {
            this.configuration = config;
            this.hnClientConfig = new HNClientConfiguration();
            this.configuration.GetSection("HNClient").Bind(this.hnClientConfig);

            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                this.hnClientConfig.WindowsTimeZoneId : this.hnClientConfig.UnixTimeZoneId;
            localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            culture = CultureInfo.CreateSpecificCulture("en-CA");
            culture.DateTimeFormat.DateSeparator = "/";

            encoding = new HL7Encoding();
        }

        /// <inheritdoc/>
        public abstract HNMessage<string> CreateRequestMessage(string id, string userId, string ipAddress);

        /// <inheritdoc/>
        public abstract HNMessage<T> ParseResponseMessage(string hl7Message);

        protected void SetMessageHeader(Message m, string userId, string ipAddress)
        {
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
            m.SetValue("MSH.7", this.GetLocalDateTime()); // HNClient specific date format
            m.SetValue("MSH.9", HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE); // HNClient doesn't recognize event types (removes ^00 from message type)
        }

        protected void SetTransactionControlSegment(Message m, string transactionId)
        {
            // ZZZ - Transaction Control
            Segment zzz = new Segment(HNClientConfiguration.SEGMENT_ZZZ, encoding);
            zzz.AddNewField(transactionId); // Transaction ID
            zzz.AddNewField(string.Empty); // Response Status (Empty)
            zzz.AddNewField(TRACE); // Trace Number
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerIdRef); // Practitioner ID Reference
            zzz.AddNewField(this.hnClientConfig.ZZZ.PractitionerId); // Practitioner ID
            m.AddNewSegment(zzz);
        }

        protected void SetClaimsStandardSegment(Message m, string id)
        {
            // ZCA - Claims Standard Message Header
            Segment zca = new Segment(HNClientConfiguration.SEGMENT_ZCA, this.encoding);
            zca.AddNewField(id); // BIN
            zca.AddNewField(this.hnClientConfig.ZCA.CPHAVersionNumber); // CPHA Version Number
            zca.AddNewField(this.hnClientConfig.ZCA.TransactionCode); // Transaction Code
            zca.AddNewField(this.hnClientConfig.ZCA.SoftwareId); // Provider Software ID
            zca.AddNewField(this.hnClientConfig.ZCA.SoftwareVersion); // Provider Software Version
            m.AddNewSegment(zca);
        }

        protected void SetProviderInfoSegment(Message m)
        {
            // ZCB - Provider Information
            Segment zcb = new Segment(HNClientConfiguration.SEGMENT_ZCB, this.encoding);
            zcb.AddNewField(this.hnClientConfig.ZCB.PharmacyId); // Pharmacy ID Code
            zcb.AddNewField(this.GetLocalDate()); // Provider Transaction Date
            zcb.AddNewField(TRACE); // Trace Number
            m.AddNewSegment(zcb);
        }

        protected Message ParseRawMessage(string message)
        {
            // Replaces the message type with message type + event so it can correcly parse the message.
            message = message.Replace(
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}|",
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00|",
                StringComparison.CurrentCulture);
            Message m = new Message(message);
            m.ParseMessage();

            return m;
        }

        protected string GetLocalDateTime()
        {
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
            return string.Format(culture, "{0:yyyy/MM/dd HH:mm:ss}", localDate);
        }

        protected string GetLocalDate()
        {
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
            return string.Format(culture, "{0:yyMMdd}", localDate);
        }

        /// <summary>
        /// Parses the string value to DateTime.
        /// </summary>
        /// <param name="value">The date value in string format yyyyMMdd.</param>
        /// <returns>The parsed datetime.</returns>
        protected DateTime? ParseDate(string value)
        {
            return string.IsNullOrEmpty(value) ?
                (DateTime?)null : DateTime.ParseExact(value, "yyyyMMdd", culture);
        }
    }
}
