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
    using System.Linq;
    using AutoMapper;
    using DeepEqual.Syntax;
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
        [Fact]
        public void ShouldGetNotes()
        {
            (RequestResult<IEnumerable<UserNote>> actualResult, List<UserNote> userNoteList) = this.ExecuteGetNotes("abc");

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(userNoteList);
        }

        /// <summary>
        /// GetNotes - Database Error.
        /// </summary>
        [Fact]
        public void ShouldGetNotesWithDbError()
        {
            (RequestResult<IEnumerable<UserNote>> actualResult, _) = this.ExecuteGetNotes("abc", DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// GetNotes - Happy Path with No Existing Encryption Key.
        /// </summary>
        [Fact]
        public void ShouldGetNotesWithProfileKeyNotSet()
        {
            (RequestResult<IEnumerable<UserNote>> actualResult, List<UserNote> userNoteList) = this.ExecuteGetNotes();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(userNoteList);
        }

        /// <summary>
        /// InsertNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldInsertNote()
        {
            (RequestResult<UserNote> actualResult, UserNote userNote) = this.ExecuteCreateNote();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// InsertNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldInsertNoteWithDbError()
        {
            (RequestResult<UserNote> actualResult, _) = this.ExecuteCreateNote(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// UpdateNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateNote()
        {
            (RequestResult<UserNote> actualResult, UserNote userNote) = this.ExecuteUpdateNote();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateNoteWithDbError()
        {
            (RequestResult<UserNote> actualResult, _) = this.ExecuteUpdateNote(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// DeleteNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteNote()
        {
            (RequestResult<UserNote> actualResult, UserNote userNote) = this.ExecuteDeleteNote();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// DeleteNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteNoteWithDbError()
        {
            (RequestResult<UserNote> actualResult, _) = this.ExecuteDeleteNote(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// InsertNote - No Encryption Key Error.
        /// </summary>
        [Fact]
        public void ShouldInsertNoteWithNoKeyError()
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
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.CreateNote(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// UpdateNote - No Encryption Key Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateNoteWithNoKeyError()
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
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// DeleteNote - No Encryption Key Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteNoteWithNoKeyError()
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
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private (RequestResult<IEnumerable<UserNote>> ActualResult, List<UserNote> ExpectedPayload) ExecuteGetNotes(string? encryptionKey = null, DbStatusCode notesDbResultStatus = DbStatusCode.Read)
        {
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

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
                    CreatedDateTime = new DateTime(2020, 1, 1),
                },
                new UserNote
                {
                    HdId = this.hdid,
                    Title = "Second Note",
                    Text = "Second Note text",
                    CreatedDateTime = new DateTime(2020, 2, 2),
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
            noteDelegateMock.Setup(s => s.GetNotes(this.hdid, 0, 500)).Returns(notesDbResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                this.autoMapper);

            RequestResult<IEnumerable<UserNote>> actualResult = service.GetNotes(this.hdid);

            return (actualResult, expectedPayload);
        }

        private (RequestResult<UserNote> ActualResult, UserNote UserNote) ExecuteCreateNote(DbStatusCode dBStatusCode = DbStatusCode.Created)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Inserted Note",
                Text = "Inserted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> insertResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.AddNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(insertResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.CreateNote(userNote);
            return (actualResult, userNote);
        }

        private (RequestResult<UserNote> ActualResult, UserNote UserNote) ExecuteUpdateNote(DbStatusCode dBStatusCode = DbStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Updated Note",
                Text = "Updated Note text",
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> updateResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.UpdateNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(updateResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);
            return (actualResult, userNote);
        }

        private (RequestResult<UserNote> ActualResult, UserNote UserNote) ExecuteDeleteNote(DbStatusCode dBStatusCode = DbStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new()
            {
                HdId = this.hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            Note note = NoteMapUtils.ToDbModel(userNote, cryptoDelegateMock.Object, encryptionKey, this.autoMapper);

            DbResult<Note> deleteResult = new()
            {
                Payload = note,
                Status = dBStatusCode,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.DeleteNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(deleteResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                this.autoMapper);

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);
            return (actualResult, userNote);
        }
    }
}
