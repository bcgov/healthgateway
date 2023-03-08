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
namespace HealthGateway.GatewayApiTests.Controllers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// PhsaController's Unit Tests.
    /// </summary>
    public class PhsaControllerTests
    {
        private readonly string hdid = "mockedHdId";
        private readonly DateTime fromDate = DateTime.UtcNow.AddDays(-1);
        private readonly DateTime toDate = DateTime.UtcNow.AddDays(1);

        /// <summary>
        /// GetDependentsAsync by hdid - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDependentsByHdid()
        {
            Mock<IDependentService> dependentServiceMock = new();
            IEnumerable<DependentModel> expectedDependents = GetMockDependentModels();
            RequestResult<IEnumerable<DependentModel>> expectedResult = new()
            {
                ResourcePayload = expectedDependents,
                ResultStatus = ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.GetDependentsAsync(this.hdid, 0, 500)).ReturnsAsync(expectedResult);

            PhsaController phsaController = new(
                dependentServiceMock.Object);
            ActionResult<RequestResult<IEnumerable<DependentModel>>> actualResult = await phsaController.GetAll(this.hdid).ConfigureAwait(true);

            RequestResult<IEnumerable<DependentModel>>? actualRequestResult = actualResult.Value;
            expectedResult.ShouldDeepEqual(actualRequestResult);
        }

        /// <summary>
        /// GetDependents by date - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetDependentsByDate()
        {
            Mock<IDependentService> dependentServiceMock = new();
            IEnumerable<GetDependentResponse> expectedDependents = GetMockDependentResponses();
            RequestResult<IEnumerable<GetDependentResponse>> expectedResult = new()
            {
                ResourcePayload = expectedDependents,
                ResultStatus = ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.GetDependents(this.fromDate, this.toDate, 0, 5000)).Returns(expectedResult);

            PhsaController phsaController = new(
                dependentServiceMock.Object);
            ActionResult<RequestResult<IEnumerable<GetDependentResponse>>> actualResult = phsaController.GetAll(this.fromDate, this.toDate);

            RequestResult<IEnumerable<GetDependentResponse>>? actualRequestResult = actualResult.Value;
            expectedResult.ShouldDeepEqual(actualRequestResult);
        }

        private static IEnumerable<GetDependentResponse> GetMockDependentResponses()
        {
            List<GetDependentResponse> dependentResponses = new();

            for (int i = 0; i < 10; i++)
            {
                dependentResponses.Add(
                    new GetDependentResponse
                    {
                        OwnerId = $"OWNER00{i}",
                        DelegateId = $"DELEGATE00{i}",
                        CreationDateTime = DateTime.UtcNow.AddMinutes(i),
                    });
            }

            return dependentResponses;
        }

        private static IEnumerable<DependentModel> GetMockDependentModels()
        {
            List<DependentModel> dependentModels = new();

            for (int i = 0; i < 10; i++)
            {
                dependentModels.Add(
                    new DependentModel
                    {
                        OwnerId = $"OWNER00{i}",
                        DelegateId = $"DELEGATE00{i}",
                        Version = (uint)i,
                        DependentInformation = new DependentInformation
                        {
                            Phn = $"{dependentModels}-{i}",
                            DateOfBirth = new DateTime(1980 + i, 1, 1),
                            Gender = "Female",
                            FirstName = "first",
                            LastName = "last-{i}",
                        },
                    });
            }

            return dependentModels;
        }
    }
}
