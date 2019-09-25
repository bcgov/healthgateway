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

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public static class TRPParser
    {
        /// <summary>
        /// Creates a TRP request message to HNClient.
        /// </summary>
        /// <param name="phn">The patient phn.</param>
        /// <returns>The HL7 message.</returns>
        public static HNMessage CreateRequestMessage(string phn)
        {
            // A lot of these hardcoded values must be properly defined
            // as of now this is all the information we have.

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

            Message m = new Message();
            m.AddSegmentMSH("GATEWAY", "BC01001239", "PNP", "CPA", "1001:127.0.0.1", "ZPN^00", "0", "D", "2.1");
            m.SetValue("MSH.7", string.Format(culture, "{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); // HNClient specific date format
            m.SetValue("MSH.9", "ZPN"); // HNClient doesn't recognize event types (removes ^00 from message type)

            Segment zzz = new Segment("ZZZ", new HL7Encoding());
            zzz.AddNewField("TRP");
            zzz.AddNewField(string.Empty);
            zzz.AddNewField("248876");
            zzz.AddNewField("91");
            zzz.AddNewField("XXAPZ");
            m.AddNewSegment(zzz);

            Segment zca = new Segment("ZCA", new HL7Encoding());
            zca.AddNewField("1");
            zca.AddNewField("70");
            zca.AddNewField("00");
            zca.AddNewField("HG");
            zca.AddNewField("01");
            zca.AddNewField(string.Empty);
            m.AddNewSegment(zca);

            Segment zcb = new Segment("ZCB", new HL7Encoding());
            zcb.AddNewField("BCXX000034"); // Pharmacy ID?
            zcb.AddNewField("190819"); // Date ?
            zcb.AddNewField("248876");
            m.AddNewSegment(zcb);

            Segment zcc = new Segment("ZCC", new HL7Encoding());
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(string.Empty);
            zcc.AddNewField(phn); // PHN
            zcc.AddNewField(string.Empty);
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
            //m.ParseMessage();

            return new List<MedicationStatement>();
        }
    }
}
