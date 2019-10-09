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
    public class TRPMessageParser : BaseMessageParser<List<MedicationStatement>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TRPMessageParser"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public TRPMessageParser(IConfiguration config) : base(config)
        {
        }

        /// <inheritdoc/>
        public override HNMessage<string> CreateRequestMessage(string id, string userId, string ipAddress)
        {
            id = id.PadLeft(13, '0');
            Message m = new Message();

            this.SetMessageHeader(m, userId, ipAddress);
            this.SetTransactionControlSegment(m, HNClientConfiguration.PATIENT_PROFILE_TRANSACTION_ID);
            this.SetClaimsStandardSegment(m, this.hnClientConfig.ZCA.BIN);
            this.SetProviderInfoSegment(m);

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
            zcc.AddNewField(id); // Provincial Health Care ID
            zcc.AddNewField(string.Empty); // Patient Gender
            m.AddNewSegment(zcc);

            return new HNMessage<string>(m.SerializeMessage(false));
        }

        /// <inheritdoc/>
        public override HNMessage<List<MedicationStatement>> ParseResponseMessage(string hl7Message)
        {
            List<MedicationStatement> medicationStatements = new List<MedicationStatement>();
            if (string.IsNullOrEmpty(hl7Message))
            {
                throw new ArgumentNullException(nameof(hl7Message));
            }

            Message m = this.ParseRawMessage(hl7Message);

            // Checks the response status
            Segment zzz = m.Segments(HNClientConfiguration.SEGMENT_ZZZ).FirstOrDefault();
            Field status = zzz.Fields(2); // Status code
            Field statusMessage = zzz.Fields(7); // Status message

            if (status.Value != "0")
            {
                // The request was not processed
                return new HNMessage<List<MedicationStatement>>(true, statusMessage.Value);
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
                    MedicationStatement medicationStatement = new MedicationStatement();
                    string[] fields = record.Split('^');
                    medicationStatement.DIN = fields[1]; // DIN
                    medicationStatement.GenericName = fields[2]; // Generic Name

                    // fields[3]; // Same Store Indicator
                    medicationStatement.Quantity = float.Parse(fields[4], CultureInfo.CurrentCulture) / 10; // Quantity
                    medicationStatement.Dosage = float.Parse(fields[5], CultureInfo.CurrentCulture) / 1000; // Max Daily Dosage

                    // fields[6]; // Ingredient Code
                    // fields[7]; // Ingredient Name
                    medicationStatement.PrescriptionStatus = fields[8][0]; // RX Status
                    medicationStatement.DispensedDate = this.ParseDate(fields[9]).Value; // Date Dispensed

                    // fields[10]; // Intervention Code
                    // fields[11]; // Practitioner ID Reference
                    // fields[12]; // Practitioner ID
                    medicationStatement.PractitionerSurname = fields[13]; // Practitioner Family Name
                    medicationStatement.DrugDiscontinuedDate = this.ParseDate(fields[14]); // Drug Discontinued Date

                    // fields[15]; // Drug Discontinued Source
                    medicationStatement.Directions = fields[16]; // Directions

                    // fields[17]; // Comment Text
                    // fields[18]; // Practitioner ID Reference (Duplicated ?)
                    // fields[19]; // Practitioner ID (Duplicated ?)
                    medicationStatement.DateEntered = this.ParseDate(fields[20]); // Date Entered

                    medicationStatement.PharmacyId = fields[21]; // Pharmacy ID

                    // fields[22].ToString(); // Adaptation Indicator

                    // fields[23].ToString(); // PharmaNet Prescription Identifier
                    // fields[24].ToString(); // MMI Codes
                    // fields[25].ToString(); // Clinical Service Codes
                    medicationStatements.Add(medicationStatement);
                }
            }
            return new HNMessage<List<MedicationStatement>>(medicationStatements);
        }
    }
}
