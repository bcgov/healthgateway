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
namespace HealthGateway.PatientTests.Utils
{
#pragma warning disable SA1600 // Disable documentation for internal classes
#pragma warning disable SA1602 // Disable documentation for internal classes
    using System;
    using System.Threading;
    using HealthGateway.PatientDataAccess;
    using Moq;

    internal static class MockPatientDataRepositoryExtension
    {
        internal static Mock<IPatientDataRepository> GetMockPatientDataRepository()
        {
            Mock<IPatientDataRepository> repo = new();
            return repo;
        }

        internal static void AttachMockQuery<T>(this Mock<IPatientDataRepository> repo, Func<T, bool> predicate, params HealthData[] data)
            where T : PatientDataQuery
        {
            repo
                .Setup(
                    o => o.QueryAsync(
                        It.Is<T>(q => predicate(q)),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(data));
        }
    }
#pragma warning restore SA1602
#pragma warning restore SA1600
}
