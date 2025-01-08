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
    using System.Globalization;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using Xunit;

    /// <summary>
    /// Patient Mappings Unit Tests.
    /// </summary>
    public class PatientMappingsTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private const string Gender = "Male";

        // PHSA Preferred Names
        private const string PhsaPreferredFirstName = "Ted";

        private const string? PhsaPreferredSecondName = null;
        private const string PhsaPreferredThirdName = "Shaw";
        private const string PhsaPreferredLastName = "Rogers";

        // PHSA Legal Names
        private const string PhsaLegalFirstName = "TSN";
        private const string PhsaLegalSecondName = "CTV";
        private const string PhsaLegalThirdName = "City";
        private const string PhsaLegalLastName = "Bell";

        // PHSA Home Address
        private const string PhsaHomeAddressStreetOne = "200 Main Street";
        private const string PhsaHomeAddressCity = "Victoria";
        private const string PhsaHomeAddressProvState = "BC";
        private const string PhsaHomeAddressPostal = "V8V 2L9";
        private const string PhsaHomeAddressCountry = "Canada";

        // PHSA Mail Address
        private const string PhsaMailAddressStreetOne = "200 Sutlej Street";
        private const string PhsaMailAddressStreetTwo = "Suite 303";
        private const string PhsaMailAddressStreetThree = "Buzz 303";
        private const string PhsaMailAddressCity = "Victoria";
        private const string PhsaMailAddressProvState = "BC";
        private const string PhsaMailAddressPostal = "V8V 2L9";
        private const string PhsaMailAddressCountry = "Canada";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly DateTime BirthDate = DateTime.Parse("1990-01-01", CultureInfo.InvariantCulture);

        /// <summary>
        /// Should map patient identity to patient model.
        /// </summary>
        [Fact]
        public void ShouldGetPatientModel()
        {
            const string delimiter = " ";

            // Arrange
            const string expectedCommonGivenName = $"{PhsaPreferredFirstName}{delimiter}{PhsaPreferredThirdName}";

            const string expectedLegalGivenName = $"{PhsaLegalFirstName}{delimiter}{PhsaLegalSecondName}{delimiter}{PhsaLegalThirdName}";

            PatientModel expectedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
                Gender = Gender,
                Birthdate = BirthDate,
                ResponseCode = string.Empty,
                IsDeceased = true,
                CommonName = new Name
                {
                    GivenName = expectedCommonGivenName,
                    Surname = PhsaPreferredLastName,
                },
                LegalName = new Name
                {
                    GivenName = expectedLegalGivenName,
                    Surname = PhsaLegalLastName,
                },
                PhysicalAddress = new Address
                {
                    StreetLines =
                    [
                        PhsaHomeAddressStreetOne,
                    ],
                    City = PhsaHomeAddressCity,
                    State = PhsaHomeAddressProvState,
                    PostalCode = PhsaHomeAddressPostal,
                    Country = PhsaHomeAddressCountry,
                },
                PostalAddress = new Address
                {
                    StreetLines =
                    [
                        PhsaMailAddressStreetOne,
                        PhsaMailAddressStreetTwo,
                        PhsaMailAddressStreetThree,
                    ],
                    City = PhsaMailAddressCity,
                    State = PhsaMailAddressProvState,
                    PostalCode = PhsaMailAddressPostal,
                    Country = PhsaMailAddressCountry,
                },
            };

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
                Gender = Gender,
                Dob = BirthDate,
                HasDeathIndicator = true,
                PreferredFirstName = PhsaPreferredFirstName,
                PreferredSecondName = PhsaPreferredSecondName,
                PreferredThirdName = PhsaPreferredThirdName,
                PreferredLastName = PhsaPreferredLastName,
                LegalFirstName = PhsaLegalFirstName,
                LegalSecondName = PhsaLegalSecondName,
                LegalThirdName = PhsaLegalThirdName,
                LegalLastName = PhsaLegalLastName,
                HomeAddressStreetOne = PhsaHomeAddressStreetOne,
                HomeAddressCity = PhsaHomeAddressCity,
                HomeAddressProvState = PhsaHomeAddressProvState,
                HomeAddressPostal = PhsaHomeAddressPostal,
                HomeAddressCountry = PhsaHomeAddressCountry,
                MailAddressStreetOne = PhsaMailAddressStreetOne,
                MailAddressStreetTwo = PhsaMailAddressStreetTwo,
                MailAddressStreetThree = PhsaMailAddressStreetThree,
                MailAddressCity = PhsaMailAddressCity,
                MailAddressProvState = PhsaMailAddressProvState,
                MailAddressPostal = PhsaMailAddressPostal,
                MailAddressCountry = PhsaMailAddressCountry,
            };

            // Act
            PatientModel actual = Mapper.Map<PatientModel>(patientIdentity);

            // Assert
            actual.ShouldDeepEqual(expectedPatient);
        }

        /// <summary>
        /// Should map patient identity to patient model.
        /// </summary>
        [Fact]
        public void ShouldGetPatientModelGivenNoNamesAndAddresses()
        {
            // Arrange
            string expectedCommonGivenName = string.Empty;
            string expectedCommonSurname = string.Empty;

            string expectedLegalGivenName = string.Empty;
            string expectedLegalSurname = string.Empty;

            PatientModel expectedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
                Gender = Gender,
                Birthdate = BirthDate,
                ResponseCode = string.Empty,
                IsDeceased = true,
                CommonName = new Name
                {
                    GivenName = expectedCommonGivenName,
                    Surname = expectedCommonSurname,
                },
                LegalName = new Name
                {
                    GivenName = expectedLegalGivenName,
                    Surname = expectedLegalSurname,
                },
                PhysicalAddress = null,
                PostalAddress = null,
            };

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
                Gender = Gender,
                Dob = BirthDate,
                HasDeathIndicator = true,
            };

            // Act
            PatientModel actual = Mapper.Map<PatientModel>(patientIdentity);

            // Assert
            actual.ShouldDeepEqual(expectedPatient);
        }
    }
}
