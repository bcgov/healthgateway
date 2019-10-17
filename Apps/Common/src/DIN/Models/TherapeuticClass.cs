// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.DIN.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TherapeuticClass : AuditableEntity
    {
        public Guid TherapeuticClassId { get; set; }

        public DrugProduct Drug { get; set; }

        [MaxLength(8)]
        public string AtcNumber { get; set; }

        [MaxLength(120)]
        public string Atc { get; set; }

        [MaxLength(240)]
        public string AtcFrench { get; set; }

        [MaxLength(80)]
        public string Ahfs { get; set; }

        [MaxLength(160)]
        public string AhfsFrench { get; set; }
    }
}
