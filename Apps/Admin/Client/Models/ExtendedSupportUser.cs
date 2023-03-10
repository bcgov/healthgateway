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

namespace HealthGateway.Admin.Client.Models;

using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// A system communication with additional state information.
/// </summary>
public class ExtendedSupportUser : SupportUser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedSupportUser"/> class.
    /// </summary>
    /// <param name="model">The support user model.</param>
    public ExtendedSupportUser(SupportUser model)
    {
        this.PopulateFromModel(model);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the support user details have been expanded.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Populates all properties from a support user model.
    /// </summary>
    /// <param name="model">The support user model.</param>
    public void PopulateFromModel(SupportUser model)
    {
        this.PersonalHealthNumber = model.PersonalHealthNumber;
        this.Hdid = model.Hdid;
        this.CreatedDateTime = model.CreatedDateTime;
        this.LastLoginDateTime = model.LastLoginDateTime;
        this.PhysicalAddress = model.PhysicalAddress;
        this.PostalAddress = model.PostalAddress;
    }
}
