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

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="noteDelegate">Injected Note delegate.</param>
        public NoteService(ILogger<UserFeedbackService> logger, INoteDelegate noteDelegate)
        {
            this.logger = logger;
            this.noteDelegate = noteDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Note> CreateNote(CreateNoteRequest note)
        {
            DBResult<Note> dbNote = this.noteDelegate.AddNote(new Note()
            {
                Text = note.Text,
                Title = note.Title,
                JournalDateTime = note.JournalDateTime,
                HdId = note.HdId,
            });
            RequestResult<Note> result = new RequestResult<Note>()
            {
                ResourcePayload = dbNote.Payload,
                ResultStatus = dbNote.Status == Database.Constant.DBStatusCode.Created ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
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
                ResultStatus = dbNotes.Status == Database.Constant.DBStatusCode.Read ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
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
                ResultStatus = dbResult.Status == Database.Constant.DBStatusCode.Updated ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }
    }
}
