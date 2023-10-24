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
    public class AdminReportService : IAdminReportService
    {
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IBlockedAccessDelegate blockedAccessDelegate;
        private readonly IPatientRepository patientRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminReportService"/> class.
        /// </summary>
        /// <param name="delegationDelegate">The ResourceDelegate delegate to communicate with DB.</param>
        /// <param name="blockedAccessDelegate">The BlockedAccess delegate to communicate with DB.</param>
        /// <param name="patientRepository">The patient repository used to retrieve patient details.</param>
        /// <param name="logger">Injected Logger.</param>
        public AdminReportService(IDelegationDelegate delegationDelegate, IBlockedAccessDelegate blockedAccessDelegate, IPatientRepository patientRepository, ILogger logger)
        {
            this.delegationDelegate = delegationDelegate;
            this.blockedAccessDelegate = blockedAccessDelegate;
            this.patientRepository = patientRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ProtectedDependentReport> GetProtectedDependentsReportAsync(int page, int pageSize, SortDirection sortDirection, CancellationToken ct)
        {
            (IList<string> Hdids, int TotalHdids) protectedHdids = await this.delegationDelegate.GetProtectedDependentHdidsAsync(page, pageSize, sortDirection, ct);
            IEnumerable<Task<ProtectedDependentRecord>> tasks = protectedHdids.Hdids
                .Select(
                    async hdid =>
                    {
                        PatientQuery query = new PatientDetailsQuery(Hdid: hdid, Source: PatientDetailSource.All);
                        PatientQueryResult? patient = null;
                        try
                        {
                            patient = await this.patientRepository.Query(query, ct);
                        }
                        catch (ProblemDetailsException e)
                        {
                            this.logger.Error($"Error retrieving patient details for hdid {hdid}: {e.Message}");
                        }

                        return new ProtectedDependentRecord(hdid, patient?.Items.SingleOrDefault()?.Phn);
                    });
            return new ProtectedDependentReport(await Task.WhenAll(tasks), new ReportMetaData(protectedHdids.TotalHdids, page, pageSize));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlockedAccessRecord>> GetBlockedAccessReportAsync(CancellationToken ct)
        {
            IList<BlockedAccess> records = await this.blockedAccessDelegate.GetAllAsync(ct);
            return records
                .Where(r => r.DataSources.Count > 0)
                .Select(r => new BlockedAccessRecord(r.Hdid, r.DataSources.ToList()));
        }
    }
}
