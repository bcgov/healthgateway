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
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class NoteService : INoteService
    {
        private readonly ILogger logger;
        private readonly INoteDelegate noteDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="noteDelegate">Injected Note delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        public NoteService(ILogger<NoteService> logger, INoteDelegate noteDelegate, IUserProfileDelegate profileDelegate, ICryptoDelegate cryptoDelegate, IMapper autoMapper)
        {
            this.logger = logger;
            this.noteDelegate = noteDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc />
        public RequestResult<UserNote> CreateNote(UserNote userNote)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userNote.HdId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userNote.HdId}");
                return new RequestResult<UserNote>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Note note = NoteMapUtils.ToDbModel(userNote, this.cryptoDelegate, key, this.autoMapper);

            DBResult<Note> dbNote = this.noteDelegate.AddNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = NoteMapUtils.CreateFromDbModel(dbNote.Payload, this.cryptoDelegate, key, this.autoMapper),
                ResultStatus = dbNote.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbNote.Status == DBStatusCode.Created ? null : new RequestResultError() { ResultMessage = dbNote.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<UserNote>> GetNotes(string hdId, int page = 0, int pageSize = 500)
        {
            int offset = page * pageSize;
            DBResult<IEnumerable<Note>> dbNotes = this.noteDelegate.GetNotes(hdId, offset, pageSize);

            UserProfile profile = this.profileDelegate.GetUserProfile(hdId).Payload;
            string? key = profile.EncryptionKey;

            // If there is no key yet, generate one and store it in the profile. Only valid while not all profiles have a encryption key.
            if (key == null)
            {
                this.logger.LogInformation($"First time note retrieval with key for user ${hdId}");
                key = this.EncryptFirstTime(profile, dbNotes.Payload);
                dbNotes = this.noteDelegate.GetNotes(hdId, offset, pageSize);
            }

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${hdId}");
                return new RequestResult<IEnumerable<UserNote>>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            RequestResult<IEnumerable<UserNote>> result = new RequestResult<IEnumerable<UserNote>>()
            {
                ResourcePayload = dbNotes.Payload.Select(c => NoteMapUtils.CreateFromDbModel(c, this.cryptoDelegate, key, this.autoMapper)),
                PageIndex = page,
                PageSize = pageSize,
                TotalResultCount = dbNotes.Payload.ToList().Count,
                ResultStatus = dbNotes.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultError = dbNotes.Status == DBStatusCode.Read ? null : new RequestResultError() { ResultMessage = dbNotes.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<UserNote> UpdateNote(UserNote userNote)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userNote.HdId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userNote.HdId}");
                return new RequestResult<UserNote>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Note note = NoteMapUtils.ToDbModel(userNote, this.cryptoDelegate, key, this.autoMapper);

            DBResult<Note> dbResult = this.noteDelegate.UpdateNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = NoteMapUtils.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key, this.autoMapper),
                ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Updated ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<UserNote> DeleteNote(UserNote userNote)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userNote.HdId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userNote.HdId}");
                return new RequestResult<UserNote>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Note note = NoteMapUtils.ToDbModel(userNote, this.cryptoDelegate, key, this.autoMapper);
            DBResult<Note> dbResult = this.noteDelegate.DeleteNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = NoteMapUtils.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key, this.autoMapper),
                ResultStatus = dbResult.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Deleted ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        private string EncryptFirstTime(UserProfile profile, IEnumerable<Note> dbNotes)
        {
            string key = this.cryptoDelegate.GenerateKey();
            profile.EncryptionKey = key;
            this.profileDelegate.Update(profile, false);
            foreach (Note note in dbNotes.ToList())
            {
                note.Title = this.cryptoDelegate.Encrypt(key, note.Title ?? string.Empty);
                note.Text = this.cryptoDelegate.Encrypt(key, note.Text ?? string.Empty);
            }

            this.noteDelegate.BatchUpdate(dbNotes, true);
            return key;
        }
    }
}
