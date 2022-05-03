//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.GatewayApiTests.Services.Test.Mock
{
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Moq;

    /// <summary>
    /// LegalAgreementDelegateMock.
    /// </summary>
    public class LegalAgreementDelegateMock : Mock<ILegalAgreementDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegalAgreementDelegateMock"/> class.
        /// </summary>
        /// <param name="termsOfService">terms of service.</param>
        public LegalAgreementDelegateMock(LegalAgreement termsOfService)
        {
            this.Setup(s => s.GetActiveByAgreementType(LegalAgreementType.TermsofService))
                .Returns(new DBResult<LegalAgreement>() { Payload = termsOfService });
        }
    }
}
