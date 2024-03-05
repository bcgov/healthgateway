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
namespace HealthGateway.IntegrationTests.AdminSupport;

using Alba;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Admin.Server;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Models.Events;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public class PatientRepositoryBlockAccessTests : ScenarioContextBase<Program>
{
    private readonly string hdid = "e3d1b6ce-781d-4adc-ab40-a9e9ae847d93";

    public PatientRepositoryBlockAccessTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, TestConfiguration.AdminConfigSection, fixture)
    {
    }

    [Fact]
    public async Task BlockAccess()
    {
        await this.Host.Scenario(
            scenario =>
            {
                scenario.Put.Json(new BlockAccessRequest([DataSource.ClinicalDocument], "Block data source for integration test."))
                    .ToUrl($"/v1/api/Support/{this.hdid}/BlockAccess");
                scenario.StatusCodeShouldBeOk();
            });

        await this.Assert(
            async ctx =>
            {
                List<OutboxItem> outboxItems = await ctx.Outbox.Where(i => i.Metadata.SessionId == this.hdid && i.Metadata.Type == nameof(DataSourcesBlockedEvent)).ToListAsync();
                outboxItems.Count.ShouldBe(1);
            });
    }
}
