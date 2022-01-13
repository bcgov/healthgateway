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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HealthGateway.Admin.Server.Mappers;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CsvExportService : ICsvExportService
    {
        private const int PageSize = 100000;
        private const int Page = 0;
        private const string AuthConfigSectionName = "KeycloakAdmin:Authentication";
        private readonly INoteDelegate noteDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly ICommentDelegate commentDelegate;
        private readonly IRatingDelegate ratingDelegate;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IAdminUserProfileDelegate adminUserProfileDelegate;
        private readonly IUserAdminDelegate userAdminDelegate;
        private readonly ILogger logger;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportService"/> class.
        /// </summary>
        /// <param name="noteDelegate">The note delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="commentDelegate">The comment delegate to interact with the DB.</param>
        /// <param name="ratingDelegate">The rating delegate to interact with the DB.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="adminUserProfileDelegate">The admin user profile delegate to interact with the DB.</param>
        /// <param name="userAdminDelegate">The user admin delegate to access identity access.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public CsvExportService(
            INoteDelegate noteDelegate,
            IUserProfileDelegate userProfileDelegate,
            ICommentDelegate commentDelegate,
            IRatingDelegate ratingDelegate,
            IAuthenticationDelegate authDelegate,
            IAdminUserProfileDelegate adminUserProfileDelegate,
            IUserAdminDelegate userAdminDelegate,
            ILogger<CsvExportService> logger,
            IConfiguration configuration)
        {
            this.noteDelegate = noteDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.commentDelegate = commentDelegate;
            this.ratingDelegate = ratingDelegate;
            this.authDelegate = authDelegate;
            this.adminUserProfileDelegate = adminUserProfileDelegate;
            this.userAdminDelegate = userAdminDelegate;
            this.logger = logger;

            IConfigurationSection configSection = configuration.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");

            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest);
        }

        /// <inheritdoc />
        public Stream GetComments(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Comment>> comments = this.commentDelegate.GetAll(Page, PageSize);
            return GetStream<Comment, CommentCsvMap>(comments.Payload);
        }

        /// <inheritdoc />
        public Stream GetNotes(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Note>> notes = this.noteDelegate.GetAll(Page, PageSize);
            return GetStream<Note, NoteCsvMap>(notes.Payload);
        }

        /// <inheritdoc />
        public Stream GetUserProfiles(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<UserProfile>> profiles = this.userProfileDelegate.GetAll(Page, PageSize);
            return GetStream<UserProfile, UserProfileCsvMap>(profiles.Payload);
        }

        /// <inheritdoc />
        public Stream GetRatings(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Rating>> profiles = this.ratingDelegate.GetAll(Page, PageSize);
            return GetStream<Rating, UserProfileCsvMap>(profiles.Payload);
        }

        /// <inheritdoc />
        public async Task<Stream> GetInactiveUsers(int inactiveDays)
        {
            this.logger.LogDebug("Getting inactive users since {InactiveDays} day(s) from last login....", inactiveDays);

            List<AdminUserProfileView> inactiveUsers = new List<AdminUserProfileView>();

            // Inactive admin user profiles from DB
            DBResult<IEnumerable<AdminUserProfile>> profilesResult = this.adminUserProfileDelegate.GetInactiveAdminUserProfiles(inactiveDays);

            if (profilesResult.Status == DBStatusCode.Read && profilesResult.Payload.Any())
            {
                inactiveUsers.AddRange(profilesResult.Payload.Select(AdminUserProfileView.FromModel).ToList());
                this.logger.LogDebug("Inactive db admin user profile count: {Count} since {InactiveDays} day(s)...", inactiveUsers.Count, inactiveDays);

                // Get admin and support users from keycloak
                JWTModel jwtModel = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest);
                RequestResult<IEnumerable<UserRepresentation>> adminUsersResult = await this.userAdminDelegate.GetUsers(IdentityAccessRole.AdminUser, jwtModel).ConfigureAwait(true);
                RequestResult<IEnumerable<UserRepresentation>> supportUsersResult = await this.userAdminDelegate.GetUsers(IdentityAccessRole.SupportUser, jwtModel).ConfigureAwait(true);

                if (adminUsersResult.ResultStatus == ResultType.Success && adminUsersResult.ResourcePayload != null)
                {
                    this.SetUserDetails(inactiveUsers, adminUsersResult.ResourcePayload.ToList(), IdentityAccessRole.AdminUser);
                }

                if (supportUsersResult.ResultStatus == ResultType.Success && supportUsersResult.ResourcePayload != null)
                {
                    this.SetUserDetails(inactiveUsers, supportUsersResult.ResourcePayload.ToList(), IdentityAccessRole.SupportUser);
                }

                this.logger.LogDebug("Inactive user with no keycloak match count: {Count}...", inactiveUsers.FindAll(x => x.UserId == null).Count);
                inactiveUsers.RemoveAll(x => x.UserId == null);
            }

            this.logger.LogDebug("Inactive user count: {Count}...", inactiveUsers.Count);
            return GetStream<AdminUserProfileView, AdminUserProfileViewCsvMap>(inactiveUsers);
        }

        private static Stream GetStream<TModel, TMap>(IEnumerable<TModel> obj)
            where TMap : ClassMap
        {
            MemoryStream stream = new();
            using (StreamWriter writeFile = new(stream, leaveOpen: true))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                CsvWriter csv = new(writeFile, CultureInfo.CurrentCulture, leaveOpen: true);
#pragma warning restore CA2000 // Dispose objects before losing scope
                csv.Context.RegisterClassMap<TMap>();
                csv.WriteRecords(obj);
            }

            return stream;
        }

        private void SetUserDetails(List<AdminUserProfileView> inactiveUsers, List<UserRepresentation> identityAccessUsers, IdentityAccessRole role)
        {
            this.logger.LogDebug("{Role} count: {Count}...", role.ToString(), identityAccessUsers.Count);
            List<UserRepresentation> users = identityAccessUsers.FindAll(x1 => inactiveUsers.Exists(x2 => x1.Username == x2.Username));
            this.logger.LogDebug("Filtered {Role} count: {Count}...", role.ToString(), users.Count);

            foreach (AdminUserProfileView inactiveUser in inactiveUsers)
            {
                UserRepresentation? user = users.Find(x => x.Username == inactiveUser.Username);

                if (user != null)
                {
                    inactiveUser.FirstName = inactiveUser.FirstName ??= user.FirstName;
                    inactiveUser.LastName = inactiveUser.LastName ??= user.LastName;
                    inactiveUser.UserId = inactiveUser.UserId ??= user.UserId;
                    inactiveUser.RealmRoles = inactiveUser.RealmRoles != null ? (inactiveUser.RealmRoles + ", " + role.ToString()) : role.ToString();
                }
            }
        }
    }
}
