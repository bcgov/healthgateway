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
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using HealthGateway.Medication.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Base class of HNClient message parser.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public abstract class BaseMessageParser<T> : IHNMessageParser<T>
        where T : class
    {
        private readonly TimeZoneInfo localTimeZone;
        private readonly IConfiguration configuration;
        private readonly CultureInfo culture;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMessageParser{T}"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        protected BaseMessageParser(IConfiguration config)
        {
            if (config is null || config.GetSection("HNClient") is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.configuration = config;
            this.ClientConfig = new HNClientConfiguration();
            this.configuration.GetSection("HNClient").Bind(this.ClientConfig);
            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                this.ClientConfig.WindowsTimeZoneId : this.ClientConfig.UnixTimeZoneId;
            this.localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            this.culture = CultureInfo.CreateSpecificCulture("en-CA");
            this.culture.DateTimeFormat.DateSeparator = "/";

            this.Encoding = new HL7Encoding();
        }

        /// <summary>
        /// Gets or sets the HNClient configuration.
        /// </summary>
        protected HNClientConfiguration ClientConfig { get; set; }

        /// <summary>
        /// Gets or sets the Hl7 encoding.
        /// </summary>
        protected HL7Encoding Encoding { get; set; }

        /// <inheritdoc/>
        public abstract HNMessage<string> CreateRequestMessage(HNMessageRequest request);

        /// <inheritdoc/>
        public abstract HNMessage<T> ParseResponseMessage(string hl7Message);

        /// <summary>
        /// Sets the MSH segment into the message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="userId">The request user id.</param>
        /// <param name="ipAddress">The request user ip address.</param>
        /// <param name="traceId">The trace ID of the Pharmanet message.</param>
        protected void SetMessageHeader(Message message, string userId, string ipAddress, long traceId)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string formattedTraceId = traceId.ToString(CultureInfo.InvariantCulture).PadLeft(6, '0');

            // MSH - Message Header
            message.AddSegmentMSH(
                this.ClientConfig.SendingApplication,
                this.ClientConfig.SendingFacility,
                this.ClientConfig.ReceivingApplication,
                this.ClientConfig.ReceivingFacility,
                $"{userId?.ToUpper(this.culture)}:{ipAddress}",
                $"{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00",
                formattedTraceId,
                this.ClientConfig.ProcessingID,
                this.ClientConfig.MessageVersion);
            message.SetValue("MSH.7", this.GetLocalDateTime()); // HNClient specific date format
            message.SetValue("MSH.9", HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE); // HNClient doesn't recognize event types (removes ^00 from message type)
        }

        /// <summary>
        /// Sets the ZZZ segment into the message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="transactionId">The message transaction id.</param>
        /// <param name="traceId">The trace ID of the Pharmanet message.</param>
        /// <param name="protectiveWord">The protecitve word securing certain HL7 messages.</param>
        protected void SetTransactionControlSegment(Message message, string transactionId, long traceId, string protectiveWord)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string formattedTraceId = traceId.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6, '0');

            // ZZZ - Transaction Control
            Segment zzz = new Segment(HNClientConfiguration.SEGMENT_ZZZ, this.Encoding);
            zzz.AddNewField(transactionId); // Transaction ID
            zzz.AddNewField(string.Empty); // Response Status (Empty)
            zzz.AddNewField(formattedTraceId); // Trace Number
            zzz.AddNewField(this.ClientConfig.ZZZ.PractitionerIdRef); // Practitioner ID Reference
            zzz.AddNewField(this.ClientConfig.ZZZ.PractitionerId); // Practitioner ID
            zzz.AddNewField(string.Empty); // Transaction Segment Count
            zzz.AddNewField(string.Empty); // Transaction Text
            zzz.AddNewField(string.IsNullOrEmpty(protectiveWord) ? string.Empty : protectiveWord); // Current Patient Keyword
            zzz.AddNewField(string.Empty); // New Patient Keyword
            zzz.AddNewField(string.Empty); // Additional Transaction Text

            message.AddNewSegment(zzz);
        }

        /// <summary>
        /// Sets the ZCA segment into the message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="id">The first field in the ZCA segment.</param>
        protected void SetClaimsStandardSegment(Message message, string id)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // ZCA - Claims Standard Message Header
            Segment zca = new Segment(HNClientConfiguration.SEGMENT_ZCA, this.Encoding);
            zca.AddNewField(id); // BIN
            zca.AddNewField(this.ClientConfig.ZCA.CPHAVersionNumber); // CPHA Version Number
            zca.AddNewField(this.ClientConfig.ZCA.TransactionCode); // Transaction Code
            zca.AddNewField(this.ClientConfig.ZCA.SoftwareId); // Provider Software ID
            zca.AddNewField(this.ClientConfig.ZCA.SoftwareVersion); // Provider Software Version
            message.AddNewSegment(zca);
        }

        /// <summary>
        /// Sets the ZCB segment into the message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="traceId">The trace ID of the Pharmanet message.</param>
        protected void SetProviderInfoSegment(Message message, long traceId)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string formattedTraceId = traceId.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6, '0');

            // ZCB - Provider Information
            Segment zcb = new Segment(HNClientConfiguration.SEGMENT_ZCB, this.Encoding);
            zcb.AddNewField(this.ClientConfig.ZCB.PharmacyId); // Pharmacy ID Code
            zcb.AddNewField(this.GetLocalDate()); // Provider Transaction Date
            zcb.AddNewField(formattedTraceId); // Trace Number
            message.AddNewSegment(zcb);
        }

        /// <summary>
        /// Parses the raw hl7 message into a message object.
        /// </summary>
        /// <param name="rawMessage">The raw hl7 message.</param>
        /// <returns>The parsed message object.</returns>
        protected Message ParseRawMessage(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage))
            {
                throw new ArgumentNullException(nameof(rawMessage));
            }

            // Replaces the message type with message type + event so it can correcly parse the message.
            rawMessage = rawMessage.Replace(
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}|",
                $"|{HNClientConfiguration.PATIENT_PROFILE_MESSAGE_TYPE}^00|",
                StringComparison.CurrentCulture);
            Message message = new Message(rawMessage);
            message.ParseMessage();

            return message;
        }

        /// <summary>
        /// Gets the local date and time.
        /// </summary>
        /// <returns>The local date and time in yyyy/MM/dd HH:mm:ss format.</returns>
        protected string GetLocalDateTime()
        {
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.localTimeZone);
            return string.Format(this.culture, "{0:yyyy/MM/dd HH:mm:ss}", localDate);
        }

        /// <summary>
        /// Gets the local date.
        /// </summary>
        /// <returns>The local date in yyMMdd format.</returns>
        protected string GetLocalDate()
        {
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.localTimeZone);
            return string.Format(this.culture, "{0:yyMMdd}", localDate);
        }

        /// <summary>
        /// Parses the string value to DateTime.
        /// </summary>
        /// <param name="value">The date value in string format yyyyMMdd.</param>
        /// <returns>The parsed datetime.</returns>
        protected DateTime? ParseDate(string value)
        {
            return string.IsNullOrEmpty(value) ?
                (DateTime?)null : DateTime.ParseExact(value, "yyyyMMdd", this.culture);
        }
    }
}
