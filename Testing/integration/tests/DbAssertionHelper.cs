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

namespace HealthGateway.IntegrationTests;

using HealthGateway.Database.Context;
using Microsoft.Extensions.DependencyInjection;

public static class DbAssertionHelper
{
    public static async Task Assert<TStartup>(this ScenarioContextBase<TStartup> context, Func<GatewayDbContext, Task> assertion)
        where TStartup : class
    {
        // ensure the assertion uses a newly created db context
        await using var scope = context.Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        await assertion(dbContext);
    }
}
