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
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using System.Linq;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;

    public class NoteServiceTest
    {
        string hdid = "1234567890123456789012345678901234567890123456789012";

        [Fact]
        public void ShouldGetNotes()
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Note> noteList = new List<Note>();
            noteList.Add(new Note
            {
                HdId = hdid,
                Title = "First Note",
                Text = "First Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            });

            noteList.Add(new Note
            {
                HdId = hdid,
                Title = "Second Note",
                Text = "Second Note text",
                CreatedDateTime = new DateTime(2020, 2, 2)
            });
            List<UserNote> userNoteList = UserNote.CreateListFromDbModel(noteList, cryptoDelegateMock.Object, encryptionKey).ToList();

            DBResult<IEnumerable<Note>> notesDBResult = new DBResult<IEnumerable<Note>>
            {
                Payload = noteList,
                Status = Database.Constants.DBStatusCode.Read
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.GetNotes(hdid, 0, 500)).Returns(notesDBResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<IEnumerable<UserNote>> actualResult = service.GetNotes(hdid, 0, 500);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNoteList));
        }

        [Fact]
        public void ShouldInsertNote()
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Inserted Note",
                Text = "Inserted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> insertResult = new DBResult<Note>
            {
                Payload = note,
                Status = Database.Constants.DBStatusCode.Created
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.AddNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(insertResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserNote> actualResult = service.CreateNote(userNote);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldUpdateNote()
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Updated Note",
                Text = "Updated Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> updateResult = new DBResult<Note>
            {
                Payload = note,
                Status = Database.Constants.DBStatusCode.Updated
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.UpdateNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(updateResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldDeleteNote()
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> deleteResult = new DBResult<Note>
            {
                Payload = note,
                Status = Database.Constants.DBStatusCode.Deleted
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.DeleteNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(deleteResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldThrowIfNoKeyAdd()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            try
            {
                RequestResult<UserNote> actualResult = service.CreateNote(userNote);
                Assert.True(false); // If it gets to this line, no exception was thrown
            }
            catch (ApplicationException) { }
        }

        [Fact]
        public void ShouldThrowIfNoKeyUpdate()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            try
            {
                RequestResult<UserNote> actualResult = service.UpdateNote(userNote);
                Assert.True(false); // If it gets to this line, no exception was thrown
            }
            catch (ApplicationException) { }
        }

        [Fact]
        public void ShouldThrowIfNoKeyDelete()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            try
            {
                RequestResult<UserNote> actualResult = service.DeleteNote(userNote);
                Assert.True(false); // If it gets to this line, no exception was thrown
            }
            catch (ApplicationException) { }
        }
    }
}
