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
namespace HealthGateway.LaboratoryTests.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.LaboratoryTests.Utils;
    using Xunit;

    /// <summary>
    /// Laboratory Test Profile Unit Tests.
    /// </summary>
    public class LaboratoryTestProfileTests
    {
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Should map phsa laboratory test to laboratory test.
        /// </summary>
        /// <param name="plsTestStatus">The pls test status to map to order status.</param>
        /// <param name="outOfRange">bool indicating whether lab test is out of range or not.</param>
        /// <param name="expectedResult">The expected result based on plsTestStatus and outOfRange.</param>
        [Theory]
        [InlineData("Completed", true, "Out of Range")]
        [InlineData("Completed", false, "In Range")]
        [InlineData("Corrected", true, "Out of Range")]
        [InlineData("Corrected", false, "In Range")]
        [InlineData("Cancelled", true, "Cancelled")]
        [InlineData("Cancelled", false, "Cancelled")]
        [InlineData("Active", true, "Pending")]
        [InlineData("Active", false, "Pending")]
        [InlineData("Pending", true, "Pending")]
        [InlineData("Pending", false, "Pending")]
        public void ShouldMapLaboratoryTest(string plsTestStatus, bool outOfRange, string expectedResult)
        {
            // Arrange
            LaboratoryTest expected = new()
            {
                TestStatus = plsTestStatus == "Active" ? "Pending" : plsTestStatus,
                Result = expectedResult,
            };

            PhsaLaboratoryTest phsaLaboratoryTest = new()
            {
                OutOfRange = outOfRange,
                PlisTestStatus = plsTestStatus,
            };

            // Act
            LaboratoryTest actual = Mapper.Map<LaboratoryTest>(phsaLaboratoryTest);

            // Verify
            Assert.Equal(expected.TestStatus, actual.TestStatus);
            // Assert.Equal(expected.OutOfRange, actual.OutOfRange);
            Assert.Equal(expected.Result, actual.Result);
        }
    }
}
