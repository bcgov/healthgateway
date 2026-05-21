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
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// NoteService's Unit Tests.
    /// </summary>
    public class NoteServiceTests
    {
        private const string EncryptionKey = "abc";
        private const string Hdid = "1234567890123456789012345678901234567890123456789012";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(Mapper, GetCryptoDelegateMock().Object);

        /// <summary>
        /// GetNotes - returns decrypted notes when the profile already has an encryption key.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldReturnNotes()
        {
            // Arrange
            List<UserNote> expected = GetUserNotes();
            IList<Note> encryptedNotes = GetEncryptedNotes(expected, EncryptionKey);

            DbResult<IList<Note>> notesDbResult = GetNotesDbResult(encryptedNotes);

            UserProfile userProfile = new()
            {
                EncryptionKey = EncryptionKey,
            };

            NoteService service = GetNoteService(
                userProfile: userProfile,
                notesDbResult: notesDbResult);

            // Act
            RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            actual.ResourcePayload.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetNotes - generates an encryption key and encrypts existing notes when the profile does not have an encryption key.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldEncryptNotesWhenProfileDoesNotHaveEncryptionKey()
        {
            // Arrange
            IList<Note> notes =
            [
                new()
                {
                    HdId = Hdid,
                    Title = "First Note",
                    Text = "First Note text",
                },
            ];

            DbResult<IList<Note>> notesDbResult = GetNotesDbResult(notes);

            UserProfile userProfile = new()
            {
                EncryptionKey = null,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            Mock<INoteDelegate> noteDelegateMock = new();

            NoteService service = GetNoteService(
                profileDelegateMock,
                userProfile,
                noteDelegateMock,
                notesDbResult: notesDbResult);

            // Act
            RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actual.ResultStatus);

            noteDelegateMock.Verify(
                x => x.BatchUpdateAsync(It.IsAny<IEnumerable<Note>>(), true, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// GetNotes - returns an error when retrieving notes from the database fails.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldReturnErrorWhenGetNotesFails()
        {
            // Arrange
            DbResult<IList<Note>> notesDbResult = new()
            {
                Payload = [],
                Status = DbStatusCode.Error,
            };

            UserProfile userProfile = new()
            {
                EncryptionKey = EncryptionKey,
            };

            NoteService service = GetNoteService(
                userProfile: userProfile,
                notesDbResult: notesDbResult);

            // Act
            RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.NotNull(actual.ResultError);
        }

        /// <summary>
        /// GetNotes - returns an empty result when the user cannot access the Note data source.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldReturnEmptyNotesWhenDataSourceCannotBeAccessed()
        {
            // Arrange
            NoteService service = GetNoteService(
                userProfile: new UserProfile { EncryptionKey = EncryptionKey },
                canAccessDataSource: false);

            // Act
            RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Empty(actual.ResourcePayload);
        }

        /// <summary>
        /// InsertNote.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created, EncryptionKey)]
        [InlineData(DbStatusCode.Error, EncryptionKey)]
        [InlineData(DbStatusCode.Error, null)]
        public async Task ShouldInsertNote(DbStatusCode? dbStatusCode, string? encryptionKey)
        {
            // Arrange
            UserNote expected = new()
            {
                HdId = Hdid,
                Title = "Inserted Note",
                Text = "Inserted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = MappingService.MapToNote(expected, encryptionKey);

            DbResult<Note>? noteDbResult = dbStatusCode != null
                ? new()
                {
                    Payload = note,
                    Status = dbStatusCode.Value,
                }
                : null;

            UserProfile userProfile = new() { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.CreateNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Created)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                actual.ResourcePayload.ShouldDeepEqual(expected);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// UpdateNote.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, EncryptionKey)]
        [InlineData(DbStatusCode.Error, EncryptionKey)]
        [InlineData(DbStatusCode.Error, null)]
        public async Task ShouldUpdateNote(DbStatusCode? dbStatusCode, string? encryptionKey)
        {
            // Arrange
            UserNote expected = new()
            {
                HdId = Hdid,
                Title = "Updated Note",
                Text = "Updated Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = MappingService.MapToNote(expected, encryptionKey);

            DbResult<Note>? noteDbResult = dbStatusCode != null
                ? new()
                {
                    Payload = note,
                    Status = dbStatusCode.Value,
                }
                : null;

            UserProfile userProfile = new() { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.UpdateNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Updated)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                actual.ResourcePayload.ShouldDeepEqual(expected);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// DeleteNote.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Deleted, EncryptionKey)]
        [InlineData(DbStatusCode.Error, EncryptionKey)]
        [InlineData(DbStatusCode.Error, null)]
        public async Task ShouldDeleteNote(DbStatusCode? dbStatusCode, string? encryptionKey)
        {
            // Arrange
            UserNote expected = new()
            {
                HdId = Hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Note note = MappingService.MapToNote(expected, encryptionKey);

            DbResult<Note>? noteDbResult = dbStatusCode != null
                ? new()
                {
                    Payload = note,
                    Status = dbStatusCode.Value,
                }
                : null;

            UserProfile userProfile = new() { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.DeleteNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Deleted)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                actual.ResourcePayload.ShouldDeepEqual(expected);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        [Theory]
        [InlineData(nameof(NoteService.CreateNoteAsync))]
        [InlineData(nameof(NoteService.UpdateNoteAsync))]
        [InlineData(nameof(NoteService.DeleteNoteAsync))]
        public async Task ShouldReturnErrorWhenProfileNotFound(string methodName)
        {
            // Arrange
            UserNote userNote = new()
            {
                HdId = Hdid,
                Title = "Test Note",
                Text = "Test Note text",
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            NoteService service = GetNoteService(userProfile: null);

            // Act
            RequestResult<UserNote> actual = methodName switch
            {
                nameof(NoteService.CreateNoteAsync) => await service.CreateNoteAsync(userNote),
                nameof(NoteService.UpdateNoteAsync) => await service.UpdateNoteAsync(userNote),
                nameof(NoteService.DeleteNoteAsync) => await service.DeleteNoteAsync(userNote),
                _ => throw new InvalidOperationException(),
            };

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.NotNull(actual.ResultError);
        }

        [Fact]
        public async Task ShouldReturnErrorWhenProfileNotFoundForGetNotes()
        {
            // Arrange
            List<Note> notes = [];

            DbResult<IList<Note>> notesDbResult = new()
            {
                Payload = notes,
                Status = DbStatusCode.Read,
            };

            NoteService service = GetNoteService(
                userProfile: null,
                notesDbResult: notesDbResult);

            // Act
            RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.NotNull(actual.ResultError);
        }

        private static List<UserNote> GetUserNotes()
        {
            return
            [
                new()
                {
                    HdId = Hdid,
                    Title = "First Note",
                    Text = "First Note text",
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },
                new()
                {
                    HdId = Hdid,
                    Title = "Second Note",
                    Text = "Second Note text",
                    CreatedDateTime = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture),
                },
            ];
        }

        private static IList<Note> GetEncryptedNotes(IEnumerable<UserNote> userNotes, string encryptionKey)
        {
            ICryptoDelegate cryptoDelegate = GetCryptoDelegateMock().Object;

            IList<Note> notes = userNotes
                .Select(Mapper.Map<UserNote, Note>)
                .ToList();

            foreach (Note note in notes)
            {
                note.Title = cryptoDelegate.Encrypt(encryptionKey, note.Title);
                note.Text = cryptoDelegate.Encrypt(encryptionKey, note.Text);
            }

            return notes;
        }

        private static DbResult<IList<Note>> GetNotesDbResult(IList<Note> notes, DbStatusCode status = DbStatusCode.Read)
        {
            return new()
            {
                Payload = notes,
                Status = status,
            };
        }

        private static Mock<ICryptoDelegate> GetCryptoDelegateMock()
        {
            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns(() => "Y1FmVGpXblpxNHQ3dyF6JUMqRi1KYU5kUmdVa1hwMnM=");
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));
            return cryptoDelegateMock;
        }

        private static NoteService GetNoteService(
            Mock<IUserProfileDelegate>? profileDelegateMock = null,
            UserProfile? userProfile = null,
            Mock<INoteDelegate>? noteDelegateMock = null,
            DbResult<Note>? noteDbResult = null,
            DbResult<IList<Note>>? notesDbResult = null,
            bool canAccessDataSource = true)
        {
            profileDelegateMock ??= new();

            profileDelegateMock.Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            profileDelegateMock
                .Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile profile, bool _, CancellationToken _) => new DbResult<UserProfile>
                {
                    Payload = profile,
                    Status = DbStatusCode.Deferred,
                });

            noteDelegateMock ??= new();
            if (noteDbResult != null)
            {
                noteDelegateMock.Setup(s => s.AddNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
                noteDelegateMock.Setup(s => s.UpdateNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
                noteDelegateMock.Setup(s => s.DeleteNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
            }

            noteDelegateMock
                .Setup(s => s.BatchUpdateAsync(It.IsAny<IEnumerable<Note>>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Note> notes, bool _, CancellationToken _) => new DbResult<IEnumerable<Note>>
                {
                    Payload = notes,
                    Status = DbStatusCode.Updated,
                });

            if (notesDbResult != null)
            {
                noteDelegateMock.Setup(s => s.GetNotesAsync(Hdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(notesDbResult);
            }

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            return new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                GetCryptoDelegateMock().Object,
                patientRepository.Object,
                MappingService);
        }
    }
}
