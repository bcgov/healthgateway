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
namespace HealthGateway.PatientTests.Mappings
{
    using System;
    using AutoMapper;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientTests.Utils;
    using Xunit;
    using PatientImmunizationForecast = HealthGateway.Patient.Services.ImmunizationForecast;

    /// <summary>
    /// Immunization profile unit tests.
    /// </summary>
    public class ImmunizationProfileTests
    {
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Verifies populated forecast dates use the Patient Data API's ISO date format.
        /// </summary>
        [Fact]
        public void ShouldMapForecastDatesToIsoFormat()
        {
            // Arrange
            ImmunizationForecast forecast = new()
            {
                EligibleDate = new DateOnly(2024, 1, 2),
                DueDate = new DateOnly(2024, 2, 3),
                ForecastCreateDate = new DateOnly(2023, 12, 4),
            };

            // Act
            PatientImmunizationForecast actual = Mapper.Map<PatientImmunizationForecast>(forecast);

            // Assert
            Assert.Equal("2024-01-02", actual.EligibleDate);
            Assert.Equal("2024-02-03", actual.DueDate);
            Assert.Equal("2023-12-04", actual.ForecastCreateDate);
        }

        /// <summary>
        /// Verifies optional forecast dates remain absent in the Patient Data API response.
        /// </summary>
        [Fact]
        public void ShouldMapMissingForecastDatesToNull()
        {
            // Act
            PatientImmunizationForecast actual = Mapper.Map<PatientImmunizationForecast>(new ImmunizationForecast());

            // Assert
            Assert.Null(actual.EligibleDate);
            Assert.Null(actual.ForecastCreateDate);
            Assert.Equal("0001-01-01", actual.DueDate);
        }

        /// <summary>
        /// Verifies an absent vaccine code is represented as an empty string.
        /// </summary>
        [Fact]
        public void ShouldMapMissingForecastVaccineCodeToEmptyString()
        {
            // Act
            PatientImmunizationForecast actual = Mapper.Map<PatientImmunizationForecast>(new ImmunizationForecast());

            // Assert
            Assert.Equal(string.Empty, actual.VaccineCode);
        }
    }
}
