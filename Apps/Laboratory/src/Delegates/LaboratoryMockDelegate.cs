//-------------------------------------------------------------------------
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
namespace HealthGateway.Laboratory.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Laboratory.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Implementation that uses HTTP to retrieve laboratory information.
    /// </summary>
    public class LaboratoryMockDelegate : ILaboratoryDelegate
    {
        /// <inheritdoc/>
        public IEnumerable<LaboratoryResult> GetLaboratoryData()
        {
            LaboratoryResult[] mockData =
            {
                new LaboratoryResult()
                {
                    Title = "Test",
                    TestDate = DateTime.Today,
                },
            };
            return mockData.AsEnumerable();
        }
    }
}
