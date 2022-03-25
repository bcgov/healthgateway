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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

/// <summary>
/// The result model for laboratory orders.
/// </summary>
public class LaboratoryOrderResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the orders have been retrieved.
    /// Will be set to true if the object has been fully loaded.
    /// When false, only Loaded, and RetryIn will be populated.
    /// </summary>
    [JsonPropertyName("loaded")]
    public bool Loaded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the orders have been queued.
    /// </summary>
    [JsonPropertyName("queued")]
    public bool Queued { get; set; }

    /// <summary>
    /// Gets or sets the minimal amount of time that should be waited before another request.
    /// The unit of measurement is in milliseconds.
    /// </summary>
    [JsonPropertyName("retryin")]
    public int RetryIn { get; set; }

    /// <summary>
    /// Gets or sets the collection of laboratory orders.
    /// </summary>
    [JsonPropertyName("orders")]
    public IEnumerable<LaboratoryOrder> LaboratoryOrders { get; set; } = Enumerable.Empty<LaboratoryOrder>();
}
