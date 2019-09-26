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
    using HealthGateway.MedicationService.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public static class TrpParser
    {
        private const string TRACE = "101010";

        /// <summary>
        /// Creates a TRP request message to HNClient.
        /// </summary>
        /// <param name="phn">The patient phn.</param>
        /// <param name="userId">The requester user id.</param>
        /// <param name="ipAddress">The requester ip address phn.</param>
        /// <param name="config">The configuration provider.</param>
        /// <returns>The HL7 message.</returns>
        public static HNMessage CreateRequestMessage(string phn, string userId, string ipAddress, IConfiguration config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            HNClientConfiguration hnClientConfig = new HNClientConfiguration();
            config.GetSection("HNClient").Bind(hnClientConfig);

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

        /// <summary>
        /// Parses a TRP response message from HNClient.
        /// </summary>
        /// <param name="hl7Message">The raw hl7 message.</param>
        /// <returns>The medication model list.</returns>
        public static List<MedicationStatement> ParseResponseMessage(string hl7Message)
        {
            Message m = new Message(hl7Message);
            return new List<MedicationStatement>();
        }
    }
}
