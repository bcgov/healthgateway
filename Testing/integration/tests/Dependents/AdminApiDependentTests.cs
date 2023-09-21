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

namespace HealthGateway.IntegrationTests.Dependents;

using Alba;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Admin.Server;
using HealthGateway.Common.Models.Events;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public class AdminApiDependentTests : ScenarioContextBase<Program>
{
    public AdminApiDependentTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, TestConfiguration.AdminConfigSection, fixture)
    {
    }

    [Fact]
    public async Task ProtectDependent()
    {
        string dependentHdid = "9874307168";
        string[] allowedDelegates = new[] { "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A" };

        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario =>
            {
                scenario
                    .Put
                    .Json(new ProtectDependentRequest(allowedDelegates, "test"))
                    .ToUrl($"/v1/api/delegation/{dependentHdid}/ProtectDependent");
            });

        AgentAction response = (await scenarioResponse.ReadAsJsonAsync<AgentAction>()).ShouldNotBeNull();
        response.Hdid.ShouldBe(dependentHdid);

        await this.Assert(
            async ctx =>
            {
                List<OutboxItem> outboxItems = await ctx.Outbox.Where(i => i.Metadata.SessionId == dependentHdid && i.Metadata.Type == typeof(DependentProtectionAddedEvent).Name).ToListAsync();
                outboxItems.Count.ShouldBe(1);
            });
    }

    [Fact]
    public async Task UnprotectDependent()
    {
        string dependentHdid = "35224807075386271";
        IScenarioResult scenarioResponse = await this.Host.Scenario(
            scenario =>
            {
                scenario
                    .Put
                    .Json(new UnprotectDependentRequest("test"))
                    .ToUrl($"/v1/api/delegation/{dependentHdid}/UnprotectDependent");
            });

        AgentAction response = (await scenarioResponse.ReadAsJsonAsync<AgentAction>()).ShouldNotBeNull();
        response.Hdid.ShouldBe(dependentHdid);

        await this.Assert(
            async ctx =>
            {
                List<OutboxItem> outboxItems = await ctx.Outbox.Where(i => i.Metadata.SessionId == dependentHdid && i.Metadata.Type == typeof(DependentProtectionRemovedEvent).Name).ToListAsync();
                outboxItems.Count.ShouldBe(1);
            });
    }
}
