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
namespace HealthGateway.GatewayApiTests.Controllers.Test
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote
                {
                    HdId = Hdid,
                    CreatedBy = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.CreateNoteAsync(It.IsAny<UserNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            RequestResult<UserNote> actualResult = await controller.CreateNote(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// UpdateNote - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.UpdateNoteAsync(It.IsAny<UserNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            RequestResult<UserNote> actualResult = await controller.UpdateNote(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// DeleteNote - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteNote()
        {
            RequestResult<UserNote> expectedResult = new()
            {
                ResourcePayload = new UserNote
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<INoteService> noteServiceMock = new();
            noteServiceMock.Setup(s => s.DeleteNoteAsync(It.IsAny<UserNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            NoteController controller = new(noteServiceMock.Object);

            RequestResult<UserNote> actualResult = await controller.DeleteNote(expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetAll Notes - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAll()
        {
            List<UserNote> mockedNotes = new();
            for (int i = 0; i < 10; i++)
            {
                mockedNotes.Add(
                    new UserNote
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
            noteServiceMock.Setup(s => s.GetNotesAsync(It.IsAny<string>(), 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            NoteController service = new(noteServiceMock.Object);

            RequestResult<IEnumerable<UserNote>> actualResult = await service.GetAll(Hdid, It.IsAny<CancellationToken>());

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
        }
    }
}
