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
    public static class CommentMapUtils
    {
        /// <summary>
        /// Converts from UserComment to Comment.
        /// </summary>
        /// <param name="userComment">The UserComment to convert.</param>
        /// <param name="cryptoDelegate">The Cryptographic delegate to use.</param>
        /// <param name="key">The encryption key to use.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A Comment.</returns>
        public static Comment ToDbModel(UserComment userComment, ICryptoDelegate cryptoDelegate, string key, IMapper autoMapper)
        {
            Comment comment = autoMapper.Map<UserComment, Comment>(
                userComment,
                opts =>
                    opts.AfterMap(
                        (src, dest) =>
                            dest.Text = !string.IsNullOrEmpty(src.Text) ? cryptoDelegate.Encrypt(key, src.Text) : string.Empty));
            return comment;
        }

        /// <summary>
        /// Converts from Comment to UserComment.
        /// </summary>
        /// <param name="comment">The Comment to convert.</param>
        /// <param name="cryptoDelegate">The Cryptographic delegate to use.</param>
        /// <param name="key">The encryption key to use.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A UserComment.</returns>
        public static UserComment CreateFromDbModel(Comment comment, ICryptoDelegate cryptoDelegate, string key, IMapper autoMapper)
        {
            UserComment userComment = autoMapper.Map<Comment, UserComment>(
                comment,
                opts =>
                    opts.AfterMap(
                        (src, dest) =>
                            dest.Text = !string.IsNullOrEmpty(src.Text) ? cryptoDelegate.Decrypt(key, src.Text) : string.Empty));

            return userComment;
        }
    }
}
