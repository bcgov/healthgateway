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
namespace HealthGateway.WebClient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class NoteService : INoteService
    {
        private readonly ILogger logger;
        private readonly INoteDelegate noteDelegate;
        private readonly IProfileDelegate profileDelegate;
        private readonly ICryptoDelegate cryptoDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="noteDelegate">Injected Note delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        public NoteService(ILogger<NoteService> logger, INoteDelegate noteDelegate, IProfileDelegate profileDelegate, ICryptoDelegate cryptoDelegate)
        {
            this.logger = logger;
            this.noteDelegate = noteDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
        }

        /// <inheritdoc />
        public RequestResult<UserNote> CreateNote(UserNote userNote)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userNote.HdId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogInformation($"User does not have a key: ${userNote.HdId}");
                throw new ApplicationException("Profile key not set");
            }

            Note note = userNote.ToDbModel(this.cryptoDelegate, key);

            DBResult<Note> dbNote = this.noteDelegate.AddNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = UserNote.CreateFromDbModel(dbNote.Payload, this.cryptoDelegate, key),
                ResultStatus = dbNote.Status == Database.Constant.DBStatusCode.Created ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbNote.Message,
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
                this.logger.LogInformation($"User does not have a key: ${hdId}");
                throw new ApplicationException("Profile key not set");
            }

            RequestResult<IEnumerable<UserNote>> result = new RequestResult<IEnumerable<UserNote>>()
            {
                ResourcePayload = UserNote.CreateListFromDbModel(dbNotes.Payload, this.cryptoDelegate, key),
                PageIndex = page,
                PageSize = pageSize,
                TotalResultCount = dbNotes.Payload.ToList().Count,
                ResultStatus = dbNotes.Status == Database.Constant.DBStatusCode.Read ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbNotes.Message,
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
                this.logger.LogInformation($"User does not have a key: ${userNote.HdId}");
                throw new ApplicationException("Profile key not set");
            }

            Note note = userNote.ToDbModel(this.cryptoDelegate, key);

            DBResult<Note> dbResult = this.noteDelegate.UpdateNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = UserNote.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key),
                ResultStatus = dbResult.Status == Database.Constant.DBStatusCode.Updated ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbResult.Message,
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
                this.logger.LogInformation($"User does not have a key: ${userNote.HdId}");
                throw new ApplicationException("Profile key not set");
            }
            
            Note note = userNote.ToDbModel(this.cryptoDelegate, key);
            DBResult<Note> dbResult = this.noteDelegate.DeleteNote(note);
            RequestResult<UserNote> result = new RequestResult<UserNote>()
            {
                ResourcePayload = UserNote.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key),
                ResultStatus = dbResult.Status == Database.Constant.DBStatusCode.Deleted ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }

        private string EncryptFirstTime(UserProfile profile, IEnumerable<Note> dbNotes)
        {
            string key = this.cryptoDelegate.GenerateKey();
            profile.EncryptionKey = key;
            this.profileDelegate.Update(profile, false);

            foreach (Note note in dbNotes)
            {
                string encryptedTitle = this.cryptoDelegate.Encrypt(key, note.Title);
                string encryptedText = this.cryptoDelegate.Encrypt(key, note.Text);
                note.Title = encryptedTitle;
                note.Text = encryptedText;
            }

            this.noteDelegate.BatchUpdate(dbNotes, true);

            return key;
        }
    }
}
