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

using HealthGateway.Admin.Common.Models;

/// <summary>
/// Patient support details with additional state information.
/// </summary>
public class ExtendedPatientSupportDetails : PatientSupportDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedPatientSupportDetails"/> class.
    /// </summary>
    /// <param name="model">The patient support details model.</param>
    public ExtendedPatientSupportDetails(PatientSupportDetails model)
    {
        this.PopulateFromModel(model);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the details have been expanded.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Populates all properties from a patient support details model.
    /// </summary>
    /// <param name="model">The patient support details model.</param>
    public void PopulateFromModel(PatientSupportDetails model)
    {
        this.PersonalHealthNumber = model.PersonalHealthNumber;
        this.Hdid = model.Hdid;
        this.ProfileCreatedDateTime = model.ProfileCreatedDateTime;
        this.ProfileLastLoginDateTime = model.ProfileLastLoginDateTime;
        this.PhysicalAddress = model.PhysicalAddress;
        this.PostalAddress = model.PostalAddress;
    }
}
