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

    public class Status : AuditableEntity
    {
        public Guid StatusId { get; set; }

        [Required]
        public DrugProduct Drug { get; set; }

        [MaxLength(1)]
        public string CurrentStatusFlag { get; set; }

        [MaxLength(40)]
        public string StatusDesc { get; set; }

        [MaxLength(80)]
        public string StatusDescFrench { get; set; }

        public DateTime? HistoryDate { get; set; }

        [MaxLength(80)]
        public string LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
