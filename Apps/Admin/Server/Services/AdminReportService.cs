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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.AdminReports;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Serilog;

    /// <inheritdoc/>
    /// <param name="delegationDelegate">The ResourceDelegate delegate to communicate with DB.</param>
    /// <param name="blockedAccessDelegate">The BlockedAccess delegate to communicate with DB.</param>
    /// <param name="patientRepository">The patient repository used to retrieve patient details.</param>
    /// <param name="logger">Injected Logger.</param>
    public class AdminReportService(IDelegationDelegate delegationDelegate, IBlockedAccessDelegate blockedAccessDelegate, IPatientRepository patientRepository, ILogger logger) : IAdminReportService
    {
        /// <inheritdoc/>
        public async Task<ProtectedDependentReport> GetProtectedDependentsReportAsync(int page, int pageSize, SortDirection sortDirection, CancellationToken ct = default)
        {
            (IList<string> hdids, int totalHdids) = await delegationDelegate.GetProtectedDependentHdidsAsync(page, pageSize, sortDirection, ct);

            IEnumerable<Task<ProtectedDependentRecord>> tasks = hdids
                .Select(
                    async hdid =>
                    {
                        PatientQuery query = new PatientDetailsQuery(Hdid: hdid, Source: PatientDetailSource.All);
                        PatientQueryResult? patient = null;
                        try
                        {
                            patient = await patientRepository.Query(query, ct);
                        }
                        catch (ProblemDetailsException e)
                        {
                            logger.Error($"Error retrieving patient details for hdid {hdid}: {e.Message}");
                        }

                        return new ProtectedDependentRecord(hdid, patient?.Items.SingleOrDefault()?.Phn);
                    });

            return new(await Task.WhenAll(tasks), new(totalHdids, page, pageSize));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlockedAccessRecord>> GetBlockedAccessReportAsync(CancellationToken ct = default)
        {
            IList<BlockedAccess> records = await blockedAccessDelegate.GetAllAsync(ct);
            return records
                .Where(r => r.DataSources.Count > 0)
                .Select(r => new BlockedAccessRecord(r.Hdid, r.DataSources.ToList()));
        }
    }
}
