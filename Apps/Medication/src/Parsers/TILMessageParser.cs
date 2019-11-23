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
    using System.Linq;
    using HealthGateway.Medication.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public class TILMessageParser : BaseMessageParser<Pharmacy>
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TILMessageParser"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="config">The configuration provider.</param>
        public TILMessageParser(
            ILogger<TILMessageParser> logger,
            IConfiguration config)
            : base(config)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override HNMessage<string> CreateRequestMessage(HNMessageRequest request)
        {
            Contract.Requires(request != null);
            this.logger.LogTrace($"Creating TIL request message... {JsonConvert.SerializeObject(request)}");
            Message m = new Message();

            this.SetMessageHeader(m, request.UserId, request.IpAddress, request.TraceId);
            this.SetClaimsStandardSegment(m, string.Empty);
            this.SetProviderInfoSegment(m, request.TraceId);

            // ZPL - Location Information
            Segment zpl = new Segment(HNClientConfiguration.SEGMENT_ZPL, this.Encoding);
            zpl.AddNewField(request.PharmacyId); // Requested PharmaNet Location Identifier
            zpl.AddNewField(string.Empty); // PharmaNet Location Name
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
            zpl.AddNewField(this.ClientConfig.ZPL.TransactionReasonCode); // Transaction Reason Code
            m.AddNewSegment(zpl);

            this.SetTransactionControlSegment(m, HNClientConfiguration.PHARMACY_PROFILE_TRANSACTION_ID, request.TraceId, null);

            HNMessage<string> retVal = new HNMessage<string>(m.SerializeMessage(false));
            this.logger.LogDebug($"Finished creating TIL request message... {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc/>
        public override HNMessage<Pharmacy> ParseResponseMessage(string hl7Message)
        {
            if (string.IsNullOrEmpty(hl7Message))
            {
                throw new ArgumentNullException(nameof(hl7Message));
            }

            this.logger.LogTrace($"Parsing TIL response message... {hl7Message}");

            Message message = this.ParseRawMessage(hl7Message);

            // Checks the response status
            Segment zzz = message.Segments(HNClientConfiguration.SEGMENT_ZZZ).FirstOrDefault();
            Field status = zzz.Fields(2); // Status code
            Field statusMessage = zzz.Fields(7); // Status message

            if (status.Value != "0")
            {
                // The request was not processed
                return new HNMessage<Pharmacy>(Common.Constants.ResultType.Error, statusMessage.Value);
            }

            // ZPL location information
            Segment zpl = message.Segments(HNClientConfiguration.SEGMENT_ZPL).FirstOrDefault();
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

            pharmacy.PhoneType = zpl.Fields(10).Value; // Telecom Type Code

            // zpl.Fields(11).Value; // Effective Date
            pharmacy.PhoneNumber = $"{zpl.Fields(12).Value}{zpl.Fields(13).Value}"; // Area Code and Telephone Number

            // zpl.Fields(14).Value; // Termination Date
            // zpl.Fields(1)5.Value; // Transaction Reason Code
            HNMessage<Pharmacy> retVal = new HNMessage<Pharmacy>(pharmacy);
            this.logger.LogDebug($"Finished parsing TIL response message... {JsonConvert.SerializeObject(retVal)}");

            return retVal;
        }
    }
}
