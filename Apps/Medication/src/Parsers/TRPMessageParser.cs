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
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Models;
    using HL7.Dotnetcore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Parser of TRP (Patient Profile) messages.
    /// </summary>
    public class TRPMessageParser : BaseMessageParser<List<MedicationStatement>>
    {
        /// <summary>
        /// The minimun size of the expected TRF field length.
        /// </summary>
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable SA1310 // Field names should not contain underscore
        public const int MIN_TRF_FIELD_LENGTH = 23;
        #pragma warning restore SA1310 // Field names should not contain underscore
        #pragma warning restore CA1707 // Identifiers should not contain underscores

        private const string PROTECTEDWORD = "17 Field KEYWORD contains invalid value";
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TRPMessageParser"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="config">The injected configuration provider.</param>
        public TRPMessageParser(
            ILogger<TRPMessageParser> logger,
            IConfiguration config)
            : base(config)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override HNMessage<string> CreateRequestMessage(HNMessageRequest request)
        {
            this.logger.LogTrace($"Creating TRP request message... {JsonConvert.SerializeObject(request)}");

            // HNClient only accepts a 13 digit phn
            request.Phn = request.Phn.PadLeft(13, '0');
            Message message = new Message();

            this.SetMessageHeader(message, request.UserId, request.IpAddress, request.TraceId);
            this.SetTransactionControlSegment(message, HNClientConfiguration.PATIENT_PROFILE_TRANSACTION_ID, request.TraceId, request.ProtectiveWord);
            this.SetClaimsStandardSegment(message, this.ClientConfig.ZCA!.BIN);
            this.SetProviderInfoSegment(message, request.TraceId);

            // ZCC - Beneficiary Information
            Segment zcc = new Segment(HNClientConfiguration.SEGMENT_ZCC, this.Encoding);
            zcc.AddNewField(string.Empty); // Carrier ID
            zcc.AddNewField(string.Empty); // Group Number or Code
            zcc.AddNewField(string.Empty); // Client ID Number or Code
            zcc.AddNewField(string.Empty); // Patient Code
            zcc.AddNewField(string.Empty); // Patient Date of Birth
            zcc.AddNewField(string.Empty); // Cardholder Identity
            zcc.AddNewField(string.Empty); // Relationship
            zcc.AddNewField(string.Empty); // Patient First Name
            zcc.AddNewField(string.Empty); // Patient Last Name
            zcc.AddNewField(request.Phn); // Provincial Health Care ID
            zcc.AddNewField(string.Empty); // Patient Gender
            message.AddNewSegment(zcc);

            HNMessage<string> retVal = new HNMessage<string>(message.SerializeMessage(false));
            this.logger.LogDebug($"Finished creating TRP request message. {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc/>
        public override HNMessage<List<MedicationStatement>> ParseResponseMessage(string hl7Message)
        {
            if (string.IsNullOrEmpty(hl7Message))
            {
                throw new ArgumentNullException(nameof(hl7Message));
            }

            this.logger.LogTrace($"Parsing TRP response message... {hl7Message}");
            List<MedicationStatement> medicationStatements = new List<MedicationStatement>();

            Message message = this.ParseRawMessage(hl7Message);

            // Checks the response status
            Segment zzz = message.Segments(HNClientConfiguration.SEGMENT_ZZZ).FirstOrDefault();
            Field status = zzz.Fields(2); // Status code
            Field statusMessage = zzz.Fields(7); // Status message

            if (status.Value != "0")
            {
                if (statusMessage.Value == PROTECTEDWORD)
                {
                    // If the protected word - 17 Field Keyword contains invalid value
                    return new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Protected, ErrorMessages.ProtectiveWordErrorMessage);
                }
                else
                {
                    // The request was not processed
                    return new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Error, statusMessage.Value);
                }
            }

            // ZPB patient history response
            Segment zpb = message.Segments(HNClientConfiguration.SEGMENT_ZPB).FirstOrDefault();

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
                    MedicationSumary medicationSumary = new MedicationSumary();
                    medicationSumary.DIN = fields[1]; // DIN
                    medicationSumary.GenericName = fields[2]; // Generic Name

                    // fields[3]; // Same Store Indicator
                    medicationSumary.Quantity = float.Parse(fields[4], CultureInfo.CurrentCulture) / 10; // Quantity
                    medicationSumary.MaxDailyDosage = float.Parse(fields[5], CultureInfo.CurrentCulture) / 1000; // Max Daily Dosage

                    // fields[6]; // Ingredient Code
                    // fields[7]; // Ingredient Name
                    medicationStatement.PrescriptionStatus = fields[8][0]; // RX Status
                    medicationStatement.DispensedDate = this.ParseDate(fields[9]) !.Value; // Date Dispensed

                    // fields[10]; // Intervention Code
                    // fields[11]; // Practitioner ID Reference
                    // fields[12]; // Practitioner ID
                    medicationStatement.PractitionerSurname = fields[13]; // Practitioner Family Name
                    medicationSumary.DrugDiscontinuedDate = this.ParseDate(fields[14]); // Drug Discontinued Date

                    // fields[15]; // Drug Discontinued Source
                    medicationStatement.Directions = fields[16]; // Directions

                    // fields[17]; // Comment Text
                    // fields[18]; // Practitioner ID Reference (Duplicated ?)
                    // fields[19]; // Practitioner ID (Duplicated ?)
                    medicationStatement.DateEntered = this.ParseDate(fields[20]); // Date Entered

                    medicationStatement.PharmacyId = fields[21]; // Pharmacy ID

                    if (fields.Length > MIN_TRF_FIELD_LENGTH)
                    {
                        // fields[22].ToString(); // Adaptation Indicator.
                        medicationStatement.PrescriptionIdentifier = fields[23]; // PharmaNet Prescription Identifier

                        // fields[24].ToString(); // MMI Codes
                        // fields[25].ToString(); // Clinical Service Codes
                    }

                    medicationStatement.MedicationSumary = medicationSumary;
                    medicationStatements.Add(medicationStatement);
                }
            }

            HNMessage<List<MedicationStatement>> retVal = new HNMessage<List<MedicationStatement>>(medicationStatements);

            this.logger.LogDebug($"Finished parsing TRP response message. {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }
    }
}
