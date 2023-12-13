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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// NoteService's Unit Tests.
    /// </summary>
    public class NoteServiceTests
    {
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";

        /// <summary>
        /// GetNotes - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetNotes()
        {
            (Task<RequestResult<IEnumerable<UserNote>>> task, List<UserNote> userNoteList) = this.ExecuteGetNotes("abc");
            RequestResult<IEnumerable<UserNote>> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(userNoteList);
        }

        /// <summary>
        /// GetNotes - Blocked Access.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetNotesGivenBlockedAccess()
        {
            (Task<RequestResult<IEnumerable<UserNote>>> task, _) = this.ExecuteGetNotes("abc", DbStatusCode.Error, false);
            RequestResult<IEnumerable<UserNote>> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetNotes - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetNotesWithDbError()
        {
            (Task<RequestResult<IEnumerable<UserNote>>> task, _) = this.ExecuteGetNotes("abc", DbStatusCode.Error);
            RequestResult<IEnumerable<UserNote>> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// GetNotes - Happy Path with No Existing Encryption Key.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetNotesWithProfileKeyNotSet()
        {
            (Task<RequestResult<IEnumerable<UserNote>>> task, List<UserNote> userNoteList) = this.ExecuteGetNotes();
            RequestResult<IEnumerable<UserNote>> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(userNoteList);
        }

        /// <summary>
        /// InsertNote - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertNote()
        {
            (Task<RequestResult<UserNote>> task, UserNote userNote) = this.ExecuteCreateNote();

            RequestResult<UserNote> actualResult = await task;
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// InsertNote - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertNoteWithDbError()
        {
            (Task<RequestResult<UserNote>> task, _) = this.ExecuteCreateNote(DbStatusCode.Error);
            RequestResult<UserNote> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// UpdateNote - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateNote()
        {
            (Task<RequestResult<UserNote>> task, UserNote userNote) = this.ExecuteUpdateNote();
            RequestResult<UserNote> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateNote - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateNoteWithDbError()
        {
            (Task<RequestResult<UserNote>> task, _) = this.ExecuteUpdateNote(DbStatusCode.Error);
            RequestResult<UserNote> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// DeleteNote - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteNote()
        {
            (Task<RequestResult<UserNote>> task, UserNote userNote) = this.ExecuteDeleteNote();
            RequestResult<UserNote> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// DeleteNote - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteNoteWithDbError()
        {
            (Task<RequestResult<UserNote>> task, _) = this.ExecuteDeleteNote(DbStatusCode.Error);
            RequestResult<UserNote> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// InsertNote - No Encryption Key Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertNoteWithNoKeyError()
        {
            string? encryptionKey = null;
            UserProfile? userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid)).ReturnsAsync(userProfile);

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = await service.CreateNoteAsync(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// UpdateNote - No Encryption Key Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateNoteWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = await service.UpdateNoteAsync(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// DeleteNote - No Encryption Key Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteNoteWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = await service.DeleteNoteAsync(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private (Task<RequestResult<IEnumerable<UserNote>>> ActualResult, List<UserNote> ExpectedPayload) ExecuteGetNotes(
            string? encryptionKey = null,
            DbStatusCode notesDbResultStatus = DbStatusCode.Read,
            bool canAccessDataSource = true)
        {
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid)).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns(() => "Y1FmVGpXblpxNHQ3dyF6JUMqRi1KYU5kUmdVa1hwMnM=");
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string key, string text) => text + key);

            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string key, string text) => text.Remove(text.Length - key.Length));

            List<UserNote> expectedPayload = new()
            {
                new UserNote
                {
                    HdId = this.hdid,
                    Title = "First Note",
                    Text = "First Note text",
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },
                new UserNote
                {
                    HdId = this.hdid,
                    Title = "Second Note",
                    Text = "Second Note text",
                    CreatedDateTime = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture),
                },
            };

            IList<Note> dbNotes = expectedPayload.Select(n => this.autoMapper.Map<UserNote, Note>(n)).ToList();
            if (encryptionKey != null)
            {
                foreach (Note note in dbNotes)
                {
                    note.Title = cryptoDelegateMock.Object.Encrypt(encryptionKey, note.Title);
                    note.Text = cryptoDelegateMock.Object.Encrypt(encryptionKey, note.Text);
                }
            }

            DbResult<IList<Note>> notesDbResult = new()
            {
                Payload = dbNotes,
                Status = notesDbResultStatus,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.GetNotesAsync(this.hdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(notesDbResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                patientRepository.Object,
                this.autoMapper);

            Task<RequestResult<IEnumerable<UserNote>>> actualResult = service.GetNotesAsync(this.hdid);

            return (actualResult, expectedPayload);
        }

        private (Task<RequestResult<UserNote>> ActualResult, UserNote UserNote) ExecuteCreateNote(DbStatusCode dBStatusCode = DbStatusCode.Created)
        {
            string encryptionKey = "abc";
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid)).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Inserted Note",
                Text = "Inserted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> insertResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.AddNoteAsync(It.Is<Note>(x => x.Text == note.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(insertResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            Task<RequestResult<UserNote>> actualResult = service.CreateNoteAsync(userNote);
            return (actualResult, userNote);
        }

        private (Task<RequestResult<UserNote>> ActualResult, UserNote UserNote) ExecuteUpdateNote(DbStatusCode dBStatusCode = DbStatusCode.Updated)
        {
            string encryptionKey = "abc";
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid)).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Updated Note",
                Text = "Updated Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> updateResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.UpdateNoteAsync(It.Is<Note>(x => x.Text == note.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(updateResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            Task<RequestResult<UserNote>> actualResult = service.UpdateNoteAsync(userNote);
            return (actualResult, userNote);
        }

        private (Task<RequestResult<UserNote>> ActualResult, UserNote UserNote) ExecuteDeleteNote(DbStatusCode dBStatusCode = DbStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            UserProfile? userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid)).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> deleteResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.DeleteNoteAsync(It.Is<Note>(x => x.Text == note.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(deleteResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                this.autoMapper);

            Task<RequestResult<UserNote>> actualResult = service.DeleteNoteAsync(userNote);
            return (actualResult, userNote);
        }
    }
}
