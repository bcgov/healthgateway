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

using System.Threading.Tasks;
using Alba;
using HealthGateway.Common.Data.ViewModels;
using HealthGateway.Common.Models.Events;
using HealthGateway.GatewayApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

public class DependentTests : ScenarioContextBase<GatewayApi.Startup>
{
    private readonly DateTime dependentDob = DateTime.Parse("2014-Mar-15");
    private readonly string dependentPhn = "9874307168";
    private readonly string dependentFirstName = "Sam";
    private readonly string dependentLastName = "Testfive";
    private readonly string delegateHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

    public DependentTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, fixture)
    {
    }

    [Fact]
    public async Task CanAddDependent()
    {
        var scenarioResponse = await this.Host.Scenario(scenario =>
        {
            scenario.Post.Json(new AddDependentRequest
            {
                DateOfBirth = dependentDob,
                Phn = dependentPhn,
                FirstName = dependentFirstName,
                LastName = dependentLastName,
            }).ToUrl($"/UserProfile/{delegateHdid}/dependent");

            scenario.StatusCodeShouldBeOk();
        });

        var response = (await scenarioResponse.ReadAsJsonAsync<RequestResult<DependentModel>>()).ShouldNotBeNull();
        response.ResultStatus.ShouldNotBe(Common.Data.Constants.ResultType.Error);

        await this.Assert(async ctx =>
        {
            var outboxItems = await ctx.Outbox.Where(i => i.Metadata.SessionId == delegateHdid && i.Metadata.Type == typeof(DependentAddedEvent).Name).ToListAsync();
            outboxItems.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanRemoveDependent()
    {
        var dependentHdidToRemove = "232434345442257";
        var scenarioResponse = await this.Host.Scenario(scenario =>
        {
            scenario.Delete.Json(new DependentModel
            {
                DelegateId = delegateHdid,
                OwnerId = dependentHdidToRemove,
            }).ToUrl($"/UserProfile/{delegateHdid}/dependent/{dependentHdidToRemove}");

            scenario.StatusCodeShouldBeOk();
        });

        var response = (await scenarioResponse.ReadAsJsonAsync<RequestResult<DependentModel>>()).ShouldNotBeNull();
        response.ResultStatus.ShouldNotBe(Common.Data.Constants.ResultType.Error);

        await this.Assert(async ctx =>
        {
            var outboxItems = await ctx.Outbox.Where(i => i.Metadata.SessionId == delegateHdid && i.Metadata.Type == typeof(DependentRemovedEvent).Name).ToListAsync();
            outboxItems.Count.ShouldBe(1);
        });
    }
}
