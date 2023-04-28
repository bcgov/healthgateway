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
namespace HealthGateway.AccountDataAccess.Patient.Api
{
#pragma warning disable SA1600 // Disables documentation for internal class.
#pragma warning disable SA1602 // Disables documentation for internal class.
    using System;

    internal abstract record PersonalAccount
    {
        public Guid Id { get; set; }

        public bool Active { get; set; }

        public string? DisplayName { get; set; }

        public DateTime CreationTimeStampUtc { get; set; }

        public DateTime ModifyTimeStampUtc { get; set; }

        public Patient PatientIdentity { get; set; } = null!;
    }

    internal abstract record Patient
    {
        public Guid Pid { get; set; }

        public string? Phn { get; set; }

        public string? HdId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime BirthDate { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; }

        public DateTime CreationTimeStampUtc { get; set; }

        public DateTime ModifyTimeStampUtc { get; set; }
    }
}
#pragma warning restore SA1600
#pragma warning restore SA1602
