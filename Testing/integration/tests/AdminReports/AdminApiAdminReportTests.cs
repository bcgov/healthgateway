﻿// -------------------------------------------------------------------------
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
using HealthGateway.Admin.Common.Models;
using HealthGateway.Admin.Server;
using HealthGateway.Common.Data.Constants;
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
        IList<BlockedAccessRecord> expectedRecords = new List<BlockedAccessRecord>
        {
            new(
                "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ",
                new List<DataSource> { DataSource.ClinicalDocument, DataSource.Covid19TestResult }),
        };

        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/BlockedAccess"); });

        IList<BlockedAccessRecord> blockedAccessRecords = (await scenarioResponse.ReadAsJsonAsync<IList<BlockedAccessRecord>>()).ShouldNotBeNull();
        blockedAccessRecords.Count.ShouldBe(1);
        blockedAccessRecords.ShouldBeEquivalentTo(expectedRecords);
    }

    [Fact]
    public async Task RetrieveProtectedDependentsReport()
    {
        IEnumerable<string> expectedHdids = new List<string>
        {
            "508820774378599978",
            "35224807075386271",
        };

        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario => { scenario.Get.Url("/v1/api/AdminReport/ProtectedDependents"); });

        IEnumerable<string> protectedHdids = (await scenarioResponse.ReadAsJsonAsync<IEnumerable<string>>()).ShouldNotBeNull();
        protectedHdids.Count().ShouldBe(2);
        protectedHdids.ShouldBeEquivalentTo(expectedHdids);
    }
}
