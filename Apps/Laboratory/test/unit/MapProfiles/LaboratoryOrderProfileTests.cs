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
    /// Laboratory Order Profile Unit Tests.
    /// </summary>
    public class LaboratoryOrderProfileTests
    {
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Should map phsa laboratory order to laboratory order.
        /// </summary>
        /// <param name="plsTestStatus">The pls test status to map to order status.</param>
        /// <param name="orderStatus">The order status that has been mapped from pls test status.</param>
        [Theory]
        [InlineData("Held", "Pending")]
        [InlineData("Partial", "Pending")]
        [InlineData("Pending", "Pending")]
        [InlineData("Final", "Final")]
        public void ShouldMapLaboratoryOrder(string plsTestStatus, string orderStatus)
        {
            const bool isReportAvailable = true;

            // Arrange
            LaboratoryOrder expected = new()
            {
                TestStatus = plsTestStatus,
                OrderStatus = orderStatus,
                ReportAvailable = isReportAvailable,
            };

            PhsaLaboratoryOrder phsaLaboratoryOrder = new()
            {
                PdfReportAvailable = isReportAvailable,
                PlisTestStatus = plsTestStatus,
            };

            // Act
            LaboratoryOrder actual = Mapper.Map<LaboratoryOrder>(phsaLaboratoryOrder);

            // Verify
            Assert.Equal(expected.TestStatus, actual.TestStatus);
            Assert.Equal(expected.OrderStatus, actual.OrderStatus);
            Assert.Equal(expected.ReportAvailable, actual.ReportAvailable);
        }
    }
}
