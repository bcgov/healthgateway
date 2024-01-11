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
namespace HealthGateway.IntegrationTests.AdminReports;

using Alba;
using HealthGateway.Admin.Common.Models.AdminReports;
using HealthGateway.Admin.Server;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public class AdminApiAdminReportsTests : ScenarioContextBase<Program>
{
    public AdminApiAdminReportsTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, TestConfiguration.AdminConfigSection, fixture)
    {
    }

    [Fact]
    public async Task RetrieveBlockedAccessReport()
    {
        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/BlockedAccess"); });

        IList<BlockedAccessRecord> blockedAccessRecords = (await scenarioResponse.ReadAsJsonAsync<IList<BlockedAccessRecord>>()).ShouldNotBeNull();

        await this.Assert(
            async ctx =>
            {
                List<BlockedAccess> allRecords = await ctx.BlockedAccess.ToListAsync();
                List<BlockedAccessRecord> expectedRecords = allRecords
                    .Select(a => new BlockedAccessRecord(a.Hdid, a.DataSources.ToList()))
                    .Where(e => e.BlockedSources.Count > 0)
                    .ToList();
                blockedAccessRecords.Count.ShouldBe(expectedRecords.Count);
                blockedAccessRecords.ShouldBeEquivalentTo(expectedRecords);
            });
    }

    [Fact]
    public async Task RetrieveProtectedDependentsReport()
    {
        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/ProtectedDependents"); });

        ProtectedDependentReport protectedDependents = (await scenarioResponse.ReadAsJsonAsync<ProtectedDependentReport>()).ShouldNotBeNull();
        await this.Assert(
            async ctx =>
            {
                List<string> protectedHdids = await ctx.Dependent
                    .Where(d => d.Protected == true)
                    .OrderBy(d => d.HdId)
                    .Select(d => d.HdId)
                    .ToListAsync();
                ReportMetadata expectedMetadata = new(protectedHdids.Count, 0, 25);
                protectedDependents.Records.Count.ShouldBe(protectedHdids.Count);
                protectedDependents.Records.Select(pd => pd.Hdid)
                    .ToList()
                    .ShouldBeEquivalentTo(protectedHdids);
                protectedDependents.Metadata.ShouldBeEquivalentTo(expectedMetadata);
            });
    }

    [Fact]
    public async Task RetrieveProtectedDependentsReportDescending()
    {
        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/ProtectedDependents?sortDirection=Descending"); });

        ProtectedDependentReport protectedDependents = (await scenarioResponse.ReadAsJsonAsync<ProtectedDependentReport>()).ShouldNotBeNull();
        await this.Assert(
            async ctx =>
            {
                List<string> protectedHdids = await ctx.Dependent
                    .Where(d => d.Protected == true)
                    .OrderByDescending(d => d.HdId)
                    .Select(d => d.HdId)
                    .ToListAsync();
                ReportMetadata expectedMetadata = new(protectedHdids.Count, 0, 25);
                protectedDependents.Records.Count.ShouldBe(protectedHdids.Count);
                protectedDependents.Records.Select(pd => pd.Hdid)
                    .ToList()
                    .ShouldBeEquivalentTo(protectedHdids);
                protectedDependents.Metadata.ShouldBeEquivalentTo(expectedMetadata);
            });
    }

    [Fact]
    public async Task RetrieveProtectedDependentsReportPaged()
    {
        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/ProtectedDependents?page=1&pageSize=1"); });

        ProtectedDependentReport protectedDependents = (await scenarioResponse.ReadAsJsonAsync<ProtectedDependentReport>()).ShouldNotBeNull();
        await this.Assert(
            async ctx =>
            {
                List<string> protectedHdids = await ctx.Dependent
                    .Where(d => d.Protected == true)
                    .OrderBy(d => d.HdId)
                    .Select(d => d.HdId)
                    .ToListAsync();
                ReportMetadata expectedMetadata = new(protectedHdids.Count, 1, 1);
                protectedDependents.Records.Count.ShouldBe(1);
                protectedDependents.Records[0].Hdid.ShouldBeEquivalentTo(protectedHdids[1]);
                protectedDependents.Metadata.ShouldBeEquivalentTo(expectedMetadata);
            });
    }
}
