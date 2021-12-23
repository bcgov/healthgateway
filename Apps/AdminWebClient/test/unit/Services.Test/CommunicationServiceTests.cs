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
namespace HealthGateway.AdminWebClientTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommunicationService's Unit Tests.
    /// </summary>
    public class CommunicationServiceTests
    {
        /// <summary>
        /// AddCommunication - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAddCommunication()
        {
            ShouldAddCommunicationWithSpecifiedDBStatusCode(DBStatusCode.Created, ResultType.Success);
        }

        /// <summary>
        /// AddCommunication - DB Error.
        /// </summary>
        [Fact]
        public void AddCommunicationShouldReturnDBError()
        {
            ShouldAddCommunicationWithSpecifiedDBStatusCode(DBStatusCode.Error, ResultType.Error);
        }

        /// <summary>
        /// GetCommunications - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetCommunications()
        {
            ShouldGetCommunicationsWithSpecifiedDBStatusCode(DBStatusCode.Read, ResultType.Success);
        }

        /// <summary>
        /// GetCommunications - DB Error.
        /// </summary>
        [Fact]
        public void GetCommunicationsShouldReturnDBError()
        {
            ShouldGetCommunicationsWithSpecifiedDBStatusCode(DBStatusCode.Error, ResultType.Error);
        }

        /// <summary>
        /// UpdateCommunication - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommunication()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> actualResult = UpdateCommunication(comm, DBStatusCode.Updated);

            // Check result
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            comm.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateCommunication - DB Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommunicationWithDBError()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> actualResult = UpdateCommunication(comm, DBStatusCode.Error);

            // Check result
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            comm.ShouldDeepEqual(actualResult?.ResourcePayload);
        }

        /// <summary>
        /// UpdateCommunication - Invalid Date Range.
        /// </summary>
        [Fact]
        public void ShouldEffectiveDateBeforeExpiryDateWhenUpdateCommunication()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 03), // Effective Date is after Expiry Date.
            };

            RequestResult<Communication> actualResult = UpdateCommunication(comm, DBStatusCode.Updated);

            // Check result
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("Effective Date should be before Expiry Date.", actualResult?.ResultError?.ResultMessage);
        }

        /// <summary>
        /// DeleteCommunication - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommunication()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> actualResult = DeleteCommunication(comm, DBStatusCode.Deleted);

            // Check result
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            comm.ShouldDeepEqual(actualResult?.ResourcePayload);
        }

        /// <summary>
        /// DeleteCommunication - Already Processed Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteProcessedCommunicationReturnError()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                CommunicationStatusCode = CommunicationStatus.Processed,
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> expectedResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Processed communication can't be deleted.", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
            };

            RequestResult<Communication> actualResult = DeleteCommunication(comm, DBStatusCode.Error);

            // Check result
            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// DeleteCommunication - DB Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommunicationWithDBError()
        {
            // Sample communication to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> actualResult = DeleteCommunication(comm, DBStatusCode.Error);

            // Check result
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            comm.ShouldDeepEqual(actualResult?.ResourcePayload);
        }

        private static RequestResult<Communication> UpdateCommunication(Communication comm, DBStatusCode dbStatusCode)
        {
            // Set up delegate
            DBResult<Database.Models.Communication> insertResult = new()
            {
                Payload = comm.ToDbModel(),
                Status = dbStatusCode,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.Update(It.Is<Database.Models.Communication>(x => x.Text == comm.Text), true)).Returns(insertResult);

            // Set up service
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object);

            return service.Update(comm);
        }

        private static RequestResult<Communication> DeleteCommunication(Communication comm, DBStatusCode dbStatusCode)
        {
            // Set up delegate
            DBResult<Database.Models.Communication> deleteResult = new()
            {
                Payload = comm.ToDbModel(),
                Status = dbStatusCode,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.Delete(It.Is<Database.Models.Communication>(x => x.Text == comm.Text), true)).Returns(deleteResult);

            // Set up service
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object);

            return service.Delete(comm);
        }

        private static void ShouldAddCommunicationWithSpecifiedDBStatusCode(DBStatusCode dBStatusCode, ResultType expectedResultType)
        {
            // Sample communication to test
            Communication comm = new()
            {
                Text = "Test communication",
                Subject = "Testing communication",
                EffectiveDateTime = new DateTime(2020, 04, 04),
                ExpiryDateTime = new DateTime(2020, 05, 13),
            };

            // Set up delegate
            DBResult<Database.Models.Communication> insertResult = new()
            {
                Payload = comm.ToDbModel(),
                Status = dBStatusCode,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.Add(It.Is<Database.Models.Communication>(x => x.Text == comm.Text), true)).Returns(insertResult);

            // Set up service
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object);

            RequestResult<Communication> actualResult = service.Add(comm);

            // Check result
            Assert.Equal(expectedResultType, actualResult.ResultStatus);
            comm.ShouldDeepEqual(actualResult?.ResourcePayload);
        }

        private static void ShouldGetCommunicationsWithSpecifiedDBStatusCode(DBStatusCode dBStatusCode, ResultType expectedResultType)
        {
            // Sample communication to test
            List<Database.Models.Communication> commsList = new()
            {
                new Database.Models.Communication()
                {
                    Text = "Test communication",
                    Subject = "Testing communication",
                    EffectiveDateTime = new DateTime(2020, 04, 04),
                    ExpiryDateTime = new DateTime(2020, 05, 13),
                },
                new Database.Models.Communication()
                {
                    Text = "Test communication 2",
                    Subject = "Testing communication 2",
                    EffectiveDateTime = new DateTime(2021, 04, 04),
                    ExpiryDateTime = new DateTime(2021, 05, 13),
                },
            };

            List<Database.Models.Communication> refCommsList = commsList;

            DBResult<IEnumerable<Database.Models.Communication>> commsDBResult = new()
            {
                Payload = commsList,
                Status = dBStatusCode,
            };

            Mock<ICommunicationDelegate> commsDelegateMock = new();
            commsDelegateMock.Setup(s => s.GetAll()).Returns(commsDBResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                commsDelegateMock.Object);

            RequestResult<IEnumerable<Communication>> actualResult = service.GetAll();

            Assert.Equal(expectedResultType, actualResult.ResultStatus);
            refCommsList.ShouldDeepEqual(actualResult?.ResourcePayload);
        }
    }
}
