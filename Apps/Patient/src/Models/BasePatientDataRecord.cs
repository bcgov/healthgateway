// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Patient.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Utils;

    /// <summary>
    /// abstract record that contains patient data.
    /// </summary>
    [JsonConverter(typeof(PatientDataJsonConverter))]
    [KnownType(typeof(DiagnosticImagingExamData))]
    public abstract record BasePatientDataRecord
    {
        /// <summary>
        /// Gets the type of the patient data.
        /// </summary>
        public abstract string Type { get; }
    }

    // Disable documentation for internal class.
#pragma warning disable SA1600
    internal class PatientDataJsonConverter : PolymorphicJsonConverter<BasePatientDataRecord>
    {
        protected override string ResolveDiscriminatorValue(BasePatientDataRecord value)
        {
            return value.Type;
        }

        protected override Type? ResolveType(string discriminatorValue)
        {
            return discriminatorValue switch
            {
                nameof(DiagnosticImagingExamData) => typeof(DiagnosticImagingExamData),
                _ => null,
            };
        }
    }
#pragma warning restore SA1600
}
