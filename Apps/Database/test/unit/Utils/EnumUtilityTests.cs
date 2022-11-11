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
namespace HealthGateway.DatabaseTests.Utils
{
    using System;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Utils;
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
            string expected = "Success";

            string actual = EnumUtility.ToEnumString<AuditTransactionResult>(AuditTransactionResult.Success);

            Assert.True(actual == expected);
        }

        /// <summary>
        /// ToEnumString - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateToStringAnnotation()
        {
            string expected = "Ok";

            string actual = EnumUtility.ToEnumString<AuditTransactionResult>(AuditTransactionResult.Success, true);

            Assert.True(actual == expected);
        }

        /// <summary>
        /// ToEnum - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateToEnum()
        {
            AuditTransactionResult expected = AuditTransactionResult.Success;

            AuditTransactionResult actual = EnumUtility.ToEnum<AuditTransactionResult>("Success");

            Assert.True(actual == expected);
        }

        /// <summary>
        /// ToEnum - Happy Path (Annotation).
        /// </summary>
        [Fact]
        public void ValidateToEnumAnnotation()
        {
            AuditTransactionResult expected = AuditTransactionResult.Success;

            AuditTransactionResult actual = EnumUtility.ToEnum<AuditTransactionResult>("Ok", true);

            Assert.True(actual == expected);
        }

        /// <summary>
        /// ToEnum - Happy Path (Default Annotation).
        /// </summary>
        [Fact]
        public void ValidateToEnumAnnotationDefault()
        {
            AuditTransactionResult expected = AuditTransactionResult.Success;

            AuditTransactionResult actual = EnumUtility.ToEnum<AuditTransactionResult>("Success", true);

            Assert.True(actual == expected);
        }

        /// <summary>
        /// ToEnum - Not Found Error.
        /// </summary>
        [Fact]
        public void ValidateExceptionToEnum()
        {
            Assert.Throws<ArgumentException>(() => EnumUtility.ToEnum<AuditTransactionResult>("NOTAVALUE"));
        }
    }
}
