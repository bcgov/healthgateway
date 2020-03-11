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
namespace HealthGateway.Medication.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public interface IRestMedStatementDelegate
    {
        /// <summary>
        /// Returns a set of Medication Statements.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="protectiveWord">The protective word to validate.</param>
        /// <param name="userId">The user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <returns></returns>
        public Task<IEnumerable<MedicationHistoryResponse>> GetMedicationStatementsAsync(string phn, string protectiveWord, string userId, string ipAddress);
    }
}