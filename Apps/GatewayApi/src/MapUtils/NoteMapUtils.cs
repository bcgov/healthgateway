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
namespace HealthGateway.GatewayApi.MapUtils
{
    using AutoMapper;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class NoteMapUtils
    {
        /// <summary>
        /// Converts from UserNote to Note.
        /// </summary>
        /// <param name="userNote">The UserNote to convert.</param>
        /// <param name="cryptoDelegate">The Cryptographic delegate to use.</param>
        /// <param name="key">The encryption key to use.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A Note.</returns>
        public static Note ToDbModel(UserNote userNote, ICryptoDelegate cryptoDelegate, string key, IMapper autoMapper)
        {
            Note note = autoMapper.Map<UserNote, Note>(userNote, opts =>
                opts.AfterMap((src, dest) =>
                {
                    dest.Title = !string.IsNullOrEmpty(src.Title) ? cryptoDelegate.Encrypt(key, src.Title) : string.Empty;
                    dest.Text = !string.IsNullOrEmpty(src.Text) ? cryptoDelegate.Encrypt(key, src.Text) : string.Empty;
                }));
            return note;
        }

        /// <summary>
        /// Converts from Note to UserNote.
        /// </summary>
        /// <param name="note">The Note to convert.</param>
        /// <param name="cryptoDelegate">The Cryptographic delegate to use.</param>
        /// <param name="key">The encryption key to use.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A UserNote.</returns>
        public static UserNote CreateFromDbModel(Note note, ICryptoDelegate cryptoDelegate, string key, IMapper autoMapper)
        {
            UserNote userNote = autoMapper.Map<Note, UserNote>(note, opts =>
                opts.AfterMap((src, dest) =>
                {
                    dest.Title = !string.IsNullOrEmpty(src.Title) ? cryptoDelegate.Decrypt(key, src.Title) : string.Empty;
                    dest.Text = !string.IsNullOrEmpty(src.Text) ? cryptoDelegate.Decrypt(key, src.Text) : string.Empty;
                }));

            return userNote;
        }
    }
}
