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
namespace HealthGateway.Database.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthGateway.Common.Data.Models;

/// <summary>
/// The admin user profile model.
/// </summary>
public class AdminUserProfile : AuditableEntity
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AdminUserProfileId { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the users last login datetime.
    /// </summary>
    public DateTime LastLoginDateTime { get; set; }
}
