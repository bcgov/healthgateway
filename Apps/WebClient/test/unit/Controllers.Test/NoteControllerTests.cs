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
namespace HealthGateway.WebClient.Test.Controllers
{
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// NoteController's Unit Tests.
    /// </summary>
    public class NoteControllerTests
    {
        private const string Hdid = "mockedHdId";

        /// <summary>
        /// CreateNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    CreatedBy = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.CreateNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            IActionResult actualResult = controller.CreateNote(Hdid, expectedResult.ResourcePayload);

            expectedResult.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// UpdateNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldUpdateNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.UpdateNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            IActionResult actualResult = controller.UpdateNote(Hdid, expectedResult.ResourcePayload);

            expectedResult.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// DeleteNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldDeleteNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.DeleteNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            IActionResult actualResult = controller.DeleteNote(expectedResult.ResourcePayload);

            expectedResult.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// GetAll Notes - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetAll()
        {
            List<UserNote> mockedNotes = new();
            for (int i = 0; i < 10; i++)
            {
                mockedNotes.Add(new UserNote()
                {
                    Text = "note " + i,
                    HdId = Hdid,
                });
            }

            RequestResult<IEnumerable<UserNote>> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = mockedNotes,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.GetNotes(It.IsAny<string>(), 0, 500)).Returns(expectedResult);

            NoteController service = new(noteServiceMock.Object);

            IActionResult actualResult = service.GetAll(Hdid);

            JsonResult? jsonResult = actualResult as JsonResult;
            RequestResult<IEnumerable<UserNote>>? actualRequestResult = jsonResult?.Value as RequestResult<IEnumerable<UserNote>>;
            Assert.NotNull(actualRequestResult);
            Assert.Equal(ResultType.Success, actualRequestResult?.ResultStatus);
        }
    }
}
