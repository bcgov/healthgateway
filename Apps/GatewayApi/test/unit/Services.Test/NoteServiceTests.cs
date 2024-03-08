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
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        private const string EncryptionKey = "abc";
        private const string Hdid = "1234567890123456789012345678901234567890123456789012";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(Mapper, GetCryptoDelegateMock().Object);

        /// <summary>
        /// GetNotes - Happy Path.
        /// </summary>
        /// <param name="notesDbStatusCode">The db status code for get notes.</param>
        /// <param name="updateProfileDbStatusCode">The db status code for update user profile.</param>
        /// <param name="updateNotesBatchDbStatusCode">The db status code for update notes batch.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="canAccessDataSource">The value indicates whether the health visit data source can be accessed or not.</param>
        /// <param name="exceptionThrown">The bool value indicating whether problem details exception was thrown or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, DbStatusCode.Deferred, null, EncryptionKey, true, false)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Deferred, DbStatusCode.Updated, null, true, false)]
        [InlineData(DbStatusCode.Error, DbStatusCode.Deferred, DbStatusCode.Updated, null, true, false)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Deferred, DbStatusCode.Error, null, true, true)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Error, null, null, true, true)]
        [InlineData(null, null, null, null, false, false)]
        public async Task ShouldGetNotes(
            DbStatusCode? notesDbStatusCode,
            DbStatusCode? updateProfileDbStatusCode,
            DbStatusCode? updateNotesBatchDbStatusCode,
            string? encryptionKey,
            bool canAccessDataSource,
            bool exceptionThrown)
        {
            // Arrange
            List<UserNote> expected =
            [
                new UserNote
                {
                    HdId = Hdid,
                    Title = "First Note",
                    Text = "First Note text",
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },

                new UserNote
                {
                    HdId = Hdid,
                    Title = "Second Note",
                    Text = "Second Note text",
                    CreatedDateTime = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture),
                },
            ];

            IList<Note> notes = expected.Select(n => Mapper.Map<UserNote, Note>(n)).ToList();
            if (encryptionKey != null)
            {
                foreach (Note note in notes)
                {
                    note.Title = GetCryptoDelegateMock().Object.Encrypt(encryptionKey, note.Title);
                    note.Text = GetCryptoDelegateMock().Object.Encrypt(encryptionKey, note.Text);
                }
            }

            DbResult<IList<Note>>? notesDbResult = notesDbStatusCode != null
                ? new()
                {
                    Payload = notes,
                    Status = notesDbStatusCode.Value,
                }
                : null;

            UserProfile userProfile = new()
            {
                EncryptionKey = encryptionKey,
            };

            DbResult<UserProfile>? updateProfileDbResult = updateProfileDbStatusCode != null
                ? new()
                {
                    Payload = userProfile,
                    Status = updateProfileDbStatusCode.Value,
                }
                : null;

            DbResult<IEnumerable<Note>>? notesBatchUpdateDbResult = updateNotesBatchDbStatusCode != null
                ? new()
                {
                    Payload = notes,
                    Status = updateNotesBatchDbStatusCode.Value,
                }
                : null;

            NoteService service = GetNoteService(
                userProfile: userProfile,
                updateProfileDbResult: updateProfileDbResult,
                notesDbResult: notesDbResult,
                notesBatchUpdateDbResult: notesBatchUpdateDbResult,
                canAccessDataSource: canAccessDataSource);

            if (exceptionThrown)
            {
                // Act and Assert
                await Assert.ThrowsAsync<DatabaseException>(() => service.GetNotesAsync(Hdid));
            }
            else
            {
                // Act
                RequestResult<IEnumerable<UserNote>> actual = await service.GetNotesAsync(Hdid);

                // Assert
                switch (canAccessDataSource, notesDbStatusCode)
                {
                    case (true, DbStatusCode.Read):
                        Assert.Equal(ResultType.Success, actual.ResultStatus);
                        expected.ShouldDeepEqual(actual.ResourcePayload);
                        break;

                    case (true, _):
                        Assert.Equal(ResultType.Error, actual.ResultStatus);
                        Assert.NotNull(actual.ResultError);
                        break;

                    case (false, _):
                        Assert.Equal(ResultType.Success, actual.ResultStatus);
                        Assert.Empty(actual.ResourcePayload);
                        break;
                }
            }
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

            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.CreateNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Created)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                expected.ShouldDeepEqual(actual.ResourcePayload);
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

            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.UpdateNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Updated)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                expected.ShouldDeepEqual(actual.ResourcePayload);
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

            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            NoteService service = GetNoteService(userProfile: userProfile, noteDbResult: noteDbResult);

            // Act
            RequestResult<UserNote> actual = await service.DeleteNoteAsync(expected);

            // Assert
            if (dbStatusCode == DbStatusCode.Deleted)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                Assert.Null(actual.ResultError);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
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
            DbResult<UserProfile>? updateProfileDbResult = null,
            Mock<INoteDelegate>? noteDelegateMock = null,
            DbResult<Note>? noteDbResult = null,
            DbResult<IList<Note>>? notesDbResult = null,
            DbResult<IEnumerable<Note>>? notesBatchUpdateDbResult = null,
            bool canAccessDataSource = true)
        {
            profileDelegateMock ??= new();

            if (updateProfileDbResult != null)
            {
                profileDelegateMock.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updateProfileDbResult);
            }

            profileDelegateMock.Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            noteDelegateMock ??= new();
            if (noteDbResult != null)
            {
                noteDelegateMock.Setup(s => s.AddNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
                noteDelegateMock.Setup(s => s.UpdateNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
                noteDelegateMock.Setup(s => s.DeleteNoteAsync(It.Is<Note>(x => x.Text == noteDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(noteDbResult);
            }

            if (notesDbResult != null)
            {
                noteDelegateMock.Setup(s => s.GetNotesAsync(Hdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(notesDbResult);
            }

            if (notesBatchUpdateDbResult != null)
            {
                noteDelegateMock.Setup(s => s.BatchUpdateAsync(It.IsAny<IEnumerable<Note>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(notesBatchUpdateDbResult);
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
