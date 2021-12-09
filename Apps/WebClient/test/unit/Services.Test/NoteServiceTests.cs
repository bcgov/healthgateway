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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// NoteService's Unit Tests.
    /// </summary>
    public class NoteServiceTests
    {
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";

        /// <summary>
        /// GetNotes - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetNotes()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = this.ExecuteGetNotes("abc", DBStatusCode.Read);
            RequestResult<IEnumerable<UserNote>> actualResult = getNotesResult.Item1;
            List<UserNote> userNoteList = getNotesResult.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userNoteList.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetNotes - Database Error.
        /// </summary>
        [Fact]
        public void ShouldGetNotesWithDbError()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = this.ExecuteGetNotes("abc", DBStatusCode.Error);
            RequestResult<IEnumerable<UserNote>> actualResult = getNotesResult.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// GetNotes - No Encryption Key Error.
        /// </summary>
        [Fact]
        public void ShouldGetNotesWithProfileKeyNotSetError()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = this.ExecuteGetNotes(null);
            RequestResult<IEnumerable<UserNote>> actualResult = getNotesResult.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("Profile Key not set", actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// InsertNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldInsertNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = this.ExecuteCreateNote(DBStatusCode.Created);
            RequestResult<UserNote> actualResult = getNotesResult.Item1;
            UserNote userNote = getNotesResult.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// InsertNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldInsertNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> deleteNotesResult = this.ExecuteCreateNote(DBStatusCode.Error);
            RequestResult<UserNote> actualResult = deleteNotesResult.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// UpdateNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = this.ExecuteUpdateNote(DBStatusCode.Updated);
            RequestResult<UserNote> actualResult = getNotesResult.Item1;
            UserNote userNote = getNotesResult.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = this.ExecuteUpdateNote(DBStatusCode.Error);
            RequestResult<UserNote> actualResult = getNotesResult.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// DeleteNote - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> deleteNotesResult = this.ExecuteDeleteNote(DBStatusCode.Deleted);
            RequestResult<UserNote> actualResult = deleteNotesResult.Item1;
            UserNote userNote = deleteNotesResult.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            userNote.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// DeleteNote - Database Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = this.ExecuteDeleteNote(DBStatusCode.Error);
            RequestResult<UserNote> actualResult = getNotesResult.Item1;

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
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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
                new Mock<ICryptoDelegate>().Object);

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
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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
                new Mock<ICryptoDelegate>().Object);

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
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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
                new Mock<ICryptoDelegate>().Object);

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> ExecuteGetNotes(string? encryptionKey = null, DBStatusCode notesDBResultStatus = DBStatusCode.Read)
        {
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Note> noteList = new()
            {
                new Note
                {
                    HdId = this.hdid,
                    Title = "First Note",
                    Text = "First Note text",
                    CreatedDateTime = new DateTime(2020, 1, 1),
                },
                new Note
                {
                    HdId = this.hdid,
                    Title = "Second Note",
                    Text = "Second Note text",
                    CreatedDateTime = new DateTime(2020, 2, 2),
                },
            };
            List<UserNote>? userNoteList = null;
            if (encryptionKey != null)
            {
                userNoteList = UserNote.CreateListFromDbModel(noteList, cryptoDelegateMock.Object, encryptionKey).ToList();
            }

            DBResult<IEnumerable<Note>> notesDBResult = new()
            {
                Payload = noteList,
                Status = notesDBResultStatus,
            };

            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.GetNotes(this.hdid, 0, 500)).Returns(notesDBResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object);

            RequestResult<IEnumerable<UserNote>> userNoteResult = service.GetNotes(this.hdid, 0, 500);

            return new Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>>(userNoteResult, userNoteList);
        }

        private Tuple<RequestResult<UserNote>, UserNote> ExecuteCreateNote(DBStatusCode dBStatusCode = DBStatusCode.Created)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> insertResult = new()
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
                cryptoDelegateMock.Object);

            RequestResult<UserNote> actualResult = service.CreateNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }

        private Tuple<RequestResult<UserNote>, UserNote> ExecuteUpdateNote(DBStatusCode dBStatusCode = DBStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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

            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> updateResult = new()
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
                cryptoDelegateMock.Object);

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }

        private Tuple<RequestResult<UserNote>, UserNote> ExecuteDeleteNote(DBStatusCode dBStatusCode = DBStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

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
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> deleteResult = new()
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
                cryptoDelegateMock.Object);

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }
    }
}
