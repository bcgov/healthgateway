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
namespace HealthGateway.Common.Data.Tests.Utils
{
    using System;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using Xunit;

    /// <summary>
    /// EnumUtility's Unit Tests.
    /// </summary>
    public class EnumUtilityTests
    {
        /// <summary>
        /// ToEnumString - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateToString()
        {
            const string expected = "TermsOfService";

            string actual = EnumUtility.ToEnumString<LegalAgreementType>(LegalAgreementType.TermsOfService);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumString - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateToStringAnnotation()
        {
            const string expected = "ToS";

            string actual = EnumUtility.ToEnumString<LegalAgreementType>(LegalAgreementType.TermsOfService, true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateToEnum()
        {
            const LegalAgreementType expected = LegalAgreementType.TermsOfService;

            LegalAgreementType actual = EnumUtility.ToEnum<LegalAgreementType>("TermsOfService");

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateToEnumAnnotation()
        {
            const LegalAgreementType expected = LegalAgreementType.TermsOfService;

            LegalAgreementType actual = EnumUtility.ToEnum<LegalAgreementType>("ToS", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Happy Path (Default Annotation).
        /// </summary>
        [Fact]
        public void ValidateToEnumAnnotationDefault()
        {
            const LegalAgreementType expected = LegalAgreementType.TermsOfService;

            LegalAgreementType actual = EnumUtility.ToEnum<LegalAgreementType>("TermsOfService", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Not Found Error.
        /// </summary>
        [Fact]
        public void ValidateExceptionToEnum()
        {
            Assert.Throws<ArgumentException>(() => EnumUtility.ToEnum<LegalAgreementType>("NOTAVALUE"));
        }
    }
}
