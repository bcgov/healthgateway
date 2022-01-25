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
namespace HealthGateway.Laboratory.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HealthGateway.Laboratory.Models.PHSA;

/// <summary>
/// An instance of a Laboratory Summary.
/// </summary>
public class LaboratorySummary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LaboratorySummary"/> class.
    /// </summary>
    public LaboratorySummary()
    {
        this.LabOrders = new List<LaboratoryOrder>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LaboratorySummary"/> class.
    /// </summary>
    /// <param name="laboratoryOrders">The list of Laboratory Order records.</param>
    [JsonConstructor]
    public LaboratorySummary(IList<LaboratoryOrder> laboratoryOrders)
    {
        this.LabOrders = laboratoryOrders;
    }

    /// <summary>
    /// Gets or sets the last refresh date of the Laboratory Summary.
    /// </summary>
    [JsonPropertyName("lastRefreshDate")]
    public DateTime LastRefreshDate { get; set; }

    /// <summary>
    /// Gets or sets the hash of the Laboratory Summary.
    /// </summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the laboratory battery count of the Laboratory Summary.
    /// </summary>
    [JsonPropertyName("batteryCount")]
    public int BatteryCount { get; set; }

    /// <summary>
    /// Gets or sets the laboratory order count of the Laboratory Summary.
    /// </summary>
    [JsonPropertyName("LabOrderCount")]
    public int LabOrderCount { get; set; }

    /// <summary>
    /// Gets the list of Laboratory Orders.
    /// </summary>
    [JsonPropertyName("labOrders")]
    public IList<LaboratoryOrder>? LabOrders { get; }

    /// <summary>
    /// Creates a LaboratorySummary object from a PHSA model.
    /// </summary>
    /// <param name="model">The laboratory summary result to convert.</param>
    /// <returns>The newly created laboratory summary object.</returns>
    public static LaboratorySummary FromPhsaModel(PhsaLaboratorySummary model)
    {
        IList<LaboratoryOrder> laboratoryOrders =
            model.LabOrders != null ? model.LabOrders.Select(LaboratoryOrder.FromPhsaModel).ToList() : new List<LaboratoryOrder>();

        return new LaboratorySummary(laboratoryOrders)
        {
            LastRefreshDate = model.LastRefreshDate,
            Hash = model.Hash,
            BatteryCount = model.BatteryCount,
            LabOrderCount = model.LabOrderCount,
        };
    }
}
