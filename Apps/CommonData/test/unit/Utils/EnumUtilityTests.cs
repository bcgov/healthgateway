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
        public void ValidateToEnumString()
        {
            const string expected = "TermsOfService";

            string actual = EnumUtility.ToEnumString<LegalAgreementType>(LegalAgreementType.TermsOfService);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumString - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumString()
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
        public void ValidateAnnotationToEnum()
        {
            const LegalAgreementType expected = LegalAgreementType.TermsOfService;

            LegalAgreementType actual = EnumUtility.ToEnum<LegalAgreementType>("ToS", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Happy Path (Annotation fallback to named value).
        /// </summary>
        [Fact]
        public void ValidateToEnumFallback()
        {
            const LegalAgreementType expected = LegalAgreementType.TermsOfService;

            LegalAgreementType actual = EnumUtility.ToEnum<LegalAgreementType>("TermsOfService", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnum - Exception when not found.
        /// </summary>
        [Fact]
        public void ValidateToEnumException()
        {
            Assert.Throws<ArgumentException>(() => EnumUtility.ToEnum<LegalAgreementType>("NOTAVALUE"));
        }

        /// <summary>
        /// ToEnum - Exception when casing doesn't match.
        /// </summary>
        [Fact]
        public void ValidateToEnumCaseSensitiveException()
        {
            Assert.Throws<ArgumentException>(() => EnumUtility.ToEnum<LegalAgreementType>("termsofservice"));
        }

        /// <summary>
        /// ToEnum - Exception when casing doesn't match (Annotation).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumCaseSensitiveException()
        {
            Assert.Throws<ArgumentException>(() => EnumUtility.ToEnum<LegalAgreementType>("tos", true));
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateToEnumOrDefault()
        {
            const EmailStatus expected = EmailStatus.Processed;

            EmailStatus actual = EnumUtility.ToEnumOrDefault<EmailStatus>("Processed");

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Not Found).
        /// </summary>
        [Fact]
        public void ValidateToEnumOrDefaultNotFound()
        {
            const EmailStatus expected = EmailStatus.New;

            EmailStatus actual = EnumUtility.ToEnumOrDefault<EmailStatus>("invalid");

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Specified Not Found).
        /// </summary>
        [Fact]
        public void ValidateToEnumOrDefaultSpecifiedNotFound()
        {
            const EmailStatus expected = EmailStatus.Pending;

            EmailStatus actual = EnumUtility.ToEnumOrDefault(string.Empty, defaultValue: EmailStatus.Pending);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumOrDefault()
        {
            const EmailFormat expected = EmailFormat.Html;

            EmailFormat actual = EnumUtility.ToEnumOrDefault<EmailFormat>("HTML", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Annotation, Not Found).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumOrDefaultNotFound()
        {
            const EmailFormat expected = EmailFormat.Text;

            EmailFormat actual = EnumUtility.ToEnumOrDefault<EmailFormat>(string.Empty, true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Annotation, Specified Not Found).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumOrDefaultSpecifiedNotFound()
        {
            const EmailFormat expected = EmailFormat.Html;

            EmailFormat actual = EnumUtility.ToEnumOrDefault(string.Empty, true, EmailFormat.Html);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Happy Path (Annotation fallback to named value).
        /// </summary>
        [Fact]
        public void ValidateToEnumOrDefaultFallback()
        {
            const EmailFormat expected = EmailFormat.Html;

            EmailFormat actual = EnumUtility.ToEnumOrDefault<EmailFormat>("Html", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Not found when casing doesn't match.
        /// </summary>
        [Fact]
        public void ValidateToEnumOrDefaultCaseSensitiveNotFound()
        {
            const EmailStatus expected = EmailStatus.New;

            EmailStatus actual = EnumUtility.ToEnumOrDefault<EmailStatus>("processed", true);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ToEnumOrDefault - Not found when casing doesn't match (Annotation).
        /// </summary>
        [Fact]
        public void ValidateAnnotationToEnumOrDefaultCaseSensitiveNotFound()
        {
            const EmailFormat expected = EmailFormat.Text;

            EmailFormat actual = EnumUtility.ToEnumOrDefault<EmailFormat>("html", true);

            Assert.Equal(expected, actual);
        }
    }
}
