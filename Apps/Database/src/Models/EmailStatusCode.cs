﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Database.Constant;

    #pragma warning disable CS1591 // self explanatory simple model
    #pragma warning disable SA1600 // self explanatory simple model
    public class EmailStatusCode : AuditableEntity
    {
        [Key]
        [Required]
        [MaxLength(10)]
        public string StatusCode { get; set; }

        [Required]
        [MaxLength(30)]
        public string Description { get; set; }
    }
}
