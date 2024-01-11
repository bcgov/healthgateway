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
namespace AccountDataAccessTest
{
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the AuditRepository class.
    /// </summary>
    public class AuditRepositoryTests
    {
        private const string Hdid = "abc123";

        /// <summary>
        /// GetDemographics by PHN - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByPhn()
        {
            // Arrange
            AgentAudit audit = new()
            {
                Hdid = Hdid,
                Reason = "Unit Test Audit",
                OperationCode = AuditOperation.ChangeDataSourceAccess,
                GroupCode = AuditGroup.BlockedAccess,
            };
            IEnumerable<AgentAudit> agentAudits = new List<AgentAudit>
            {
                audit,
            };

            AgentAuditQuery query = new(Hdid, AuditGroup.BlockedAccess);
            AuditRepository auditRepository = GetAuditRepository(agentAudits);

            // Act
            IEnumerable<AgentAudit> actual = await auditRepository.HandleAsync(query);

            // Verify
            IEnumerable<AgentAudit> collection = actual.ToList();
            Assert.Single(collection);
            agentAudits.Single().ShouldDeepEqual(collection.Single());
        }

        private static AuditRepository GetAuditRepository(IEnumerable<AgentAudit> agentAudits)
        {
            Mock<IAgentAuditDelegate> agentAuditDelegate = new();
            agentAuditDelegate.Setup(s => s.GetAgentAuditsAsync(It.IsAny<string>(), It.IsAny<AuditGroup>(), It.IsAny<CancellationToken>())).ReturnsAsync(agentAudits);

            return new(agentAuditDelegate.Object);
        }
    }
}
