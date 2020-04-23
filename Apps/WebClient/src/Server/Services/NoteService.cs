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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
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
        public NoteService(ILogger<UserFeedbackService> logger, INoteDelegate noteDelegate, IProfileDelegate profileDelegate, ICryptoDelegate cryptoDelegate)
        {
            this.logger = logger;
            this.noteDelegate = noteDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Note> CreateNote(UserNote userNote)
        {
            //string key = this.profileDelegate.GetUserProfile(note.HdId).Payload.code;
            string key = "somekey";
            string encryptedTitle = this.cryptoDelegate.Encrypt(key, userNote.Title);
            string encryptedText = this.cryptoDelegate.Encrypt(key, userNote.Text);            

            Note note = userNote.ToDbModel();
            note.Title = encryptedTitle;
            note.Text = encryptedText;

            DBResult<Note> dbNote = this.noteDelegate.AddNote(note);
            RequestResult<Note> result = new RequestResult<Note>()
            {
                ResourcePayload = dbNote.Payload,
                ResultStatus = dbNote.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultMessage = dbNote.Message,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<Note>> GetNotes(string hdId, int page = 0, int pageSize = 500)
        {
            int offset = page * pageSize;
            DBResult<List<Note>> dbNotes = this.noteDelegate.GetNotes(hdId, offset, pageSize);
            RequestResult<IEnumerable<Note>> result = new RequestResult<IEnumerable<Note>>()
            {
                ResourcePayload = dbNotes.Payload,
                PageIndex = page,
                PageSize = pageSize,
                TotalResultCount = dbNotes.Payload.Count,
                ResultStatus = dbNotes.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultMessage = dbNotes.Message,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<Note> UpdateNote(Note note)
        {
            DBResult<Note> dbResult = this.noteDelegate.UpdateNote(note);
            RequestResult<Note> result = new RequestResult<Note>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<Note> DeleteNote(Note note)
        {
            DBResult<Note> dbResult = this.noteDelegate.DeleteNote(note);
            RequestResult<Note> result = new RequestResult<Note>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }
    }
}
