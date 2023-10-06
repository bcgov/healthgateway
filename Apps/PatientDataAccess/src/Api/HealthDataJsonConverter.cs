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
namespace HealthGateway.PatientDataAccess.Api
{
#pragma warning disable SA1600 // Disables documentation for internal class.
#pragma warning disable SA1602 // Disables documentation for internal class.
    using System;
    using HealthGateway.Common.Utils;

    internal class HealthDataJsonConverter : PolymorphicJsonConverter<HealthDataEntry>
    {
        protected override string Discriminator => "healthDataType";

        protected override Type? ResolveType(string? discriminatorValue)
        {
            return discriminatorValue switch
            {
                "Laboratory" => typeof(LaboratoryOrder),
                "COVID19Laboratory" => typeof(LaboratoryOrder),
                "ClinicalDocument" => typeof(ClinicalDocument),
                "DiagnosticImaging" => typeof(DiagnosticImagingExam),
                "BcCancerScreening" => typeof(BcCancerScreening),
                _ => null,
            };
        }
    }
}
#pragma warning restore SA1600
#pragma warning restore SA1602
