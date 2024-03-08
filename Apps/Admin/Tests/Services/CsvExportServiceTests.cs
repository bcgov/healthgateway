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

namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the CsvExportService class.
    /// </summary>
    public class CsvExportServiceTests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// Get comments download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCommentsAsync()
        {
            // Arrange
            Comment comment = new()
            {
                Id = Guid.NewGuid(),
                UserProfileId = "12345",
                EntryTypeCode = "Code",
                ParentEntryId = "12345",
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
            };
            IList<Comment> comments = [comment];
            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(comments);
            ICsvExportService service = GetCsvExportService(commentDelegateMock: commentDelegateMock);

            // Act
            Stream stream = await service.GetCommentsAsync();

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Get notes download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetNotesAsync()
        {
            // Arrange
            Note note = new()
            {
                Id = Guid.NewGuid(),
                HdId = "12345",
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
            };
            IList<Note> notes = [note];
            Mock<INoteDelegate> noteDelegateMock = new();
            noteDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(notes);
            ICsvExportService service = GetCsvExportService(noteDelegateMock: noteDelegateMock);

            // Act
            Stream stream = await service.GetNotesAsync();

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Get user profiles download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserProfilesAsync()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = "12345",
                Email = "test@healthgateway.com",
                SmsNumber = "2505550000",
                EncryptionKey = "key",
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
            };
            IList<UserProfile> userProfiles = [userProfile];
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            ICsvExportService service = GetCsvExportService(profileDelegateMock: profileDelegateMock);

            // Act
            Stream stream = await service.GetUserProfilesAsync();

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Get ratings download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetRatingsAsync()
        {
            // Arrange
            Rating rating = new()
            {
                Id = Guid.NewGuid(),
                RatingValue = 5,
            };
            IList<Rating> ratings = [rating];
            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ratings);
            ICsvExportService service = GetCsvExportService(ratingDelegateMock: ratingDelegateMock);

            // Act
            Stream stream = await service.GetRatingsAsync();

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Get inactive users download.
        /// </summary>
        /// <param name="inactiveUsersResultType">The result type to return for get inactive users service call.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Error)]
        public async Task ShouldGetInactiveUsersAsync(ResultType inactiveUsersResultType)
        {
            // Arrange
            RequestResult<List<AdminUserProfileView>> result = new()
            {
                ResultStatus = inactiveUsersResultType,
                ResourcePayload =
                [
                    new()
                    {
                        AdminUserProfileId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Doe",
                        Username = "JohnDoe",
                        Email = "email",
                        RealmRoles = "Admin",
                        LastLoginDateTime = DateTime.UtcNow,
                    },
                ],
            };

            Mock<IInactiveUserService> inactiveUserServiceMock = new();
            inactiveUserServiceMock.Setup(s => s.GetInactiveUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            ICsvExportService service = GetCsvExportService(inactiveUserServiceMock: inactiveUserServiceMock);

            // Act
            Stream stream = await service.GetInactiveUsersAsync(30);

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Get user feedback download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserFeedbackAsync()
        {
            // Arrange
            Guid adminTagId = Guid.NewGuid();
            UserFeedback userFeedback = new()
            {
                Id = Guid.NewGuid(),
                Comment = "Comment",
                CreatedDateTime = DateTime.UtcNow,
                Tags =
                [
                    new()
                    {
                        AdminTagId = adminTagId,
                        AdminTag = new()
                        {
                            AdminTagId = adminTagId,
                            Name = "Name",
                        },
                    },
                ],
            };

            IList<UserFeedback> userFeedbacks = [userFeedback];
            Mock<IFeedbackDelegate> feedbackDelegateMock = new();
            feedbackDelegateMock.Setup(s => s.GetAllUserFeedbackEntriesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(userFeedbacks);
            ICsvExportService service = GetCsvExportService(feedbackDelegateMock: feedbackDelegateMock);

            // Act
            Stream stream = await service.GetUserFeedbackAsync();

            // Assert
            Assert.NotNull(stream);
        }

        /// <summary>
        /// Happy path year of birth counts download.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetYearOfBirthCountsAsync()
        {
            // Arrange
            Dictionary<int, int> getResult = [];
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetLoggedInUserYearOfBirthCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(getResult);
            ICsvExportService service = GetCsvExportService(profileDelegateMock: profileDelegateMock);
            DateOnly startDateLocal = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
            DateOnly endDateLocal = DateOnly.FromDateTime(DateTime.Now);

            // Act
            Stream yobCounts = await service.GetYearOfBirthCountsAsync(startDateLocal, endDateLocal, CancellationToken.None);

            // Assert
            Assert.NotNull(yobCounts);
        }

        /// <summary>
        /// Tests the mapping from AdminUserProfile to AdminUserProfileView.
        /// </summary>
        [Fact]
        public void TestMapperAdminUserProfileViewFromAdminUserProfile()
        {
            AdminUserProfile adminUserProfile = new()
            {
                AdminUserProfileId = Guid.NewGuid(),
                LastLoginDateTime = DateTime.UtcNow,
                Username = "username",
            };

            AdminUserProfileView expected = new()
            {
                AdminUserProfileId = adminUserProfile.AdminUserProfileId,
                UserId = null,
                LastLoginDateTime = adminUserProfile.LastLoginDateTime,
                Username = adminUserProfile.Username,
                Email = null,
                FirstName = null,
                LastName = null,
                RealmRoles = null,
            };

            AdminUserProfileView actual = MappingService.MapToAdminUserProfileView(adminUserProfile);

            Assert.Equal(expected.AdminUserProfileId, actual.AdminUserProfileId);
            Assert.Equal(expected.UserId, actual.UserId);
            Assert.NotNull(actual.LastLoginDateTime);
            Assert.Equal(DateTimeKind.Unspecified, actual.LastLoginDateTime.Value.Kind);
            Assert.Equal(expected.LastLoginDateTime, DateFormatter.SpecifyLocal(actual.LastLoginDateTime.Value, Configuration));
            Assert.Equal(expected.Username, actual.Username);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.RealmRoles, actual.RealmRoles);
        }

        /// <summary>
        /// Tests the mapping from UserRepresentation to AdminUserProfileView.
        /// </summary>
        [Fact]
        public void TestMapperAdminUserProfileViewFromUserRepresentation()
        {
            UserRepresentation userRepresentation = new()
            {
                UserId = Guid.NewGuid(),
                Username = "username",
                Email = "email@email.com",
                FirstName = "firstname",
                LastName = "lastname",
            };

            AdminUserProfileView expected = new()
            {
                AdminUserProfileId = null,
                UserId = userRepresentation.UserId,
                LastLoginDateTime = null,
                Username = userRepresentation.Username,
                Email = userRepresentation.Email,
                FirstName = userRepresentation.FirstName,
                LastName = userRepresentation.LastName,
                RealmRoles = null,
            };

            AdminUserProfileView actual = MappingService.MapToAdminUserProfileView(userRepresentation);

            expected.ShouldDeepEqual(actual);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
                { "EnabledRoles", "[ \"AdminUser\", \"AdminReviewer\", \"SupportUser\" ]" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static ICsvExportService GetCsvExportService(
            Mock<IUserProfileDelegate>? profileDelegateMock = null,
            Mock<ICommentDelegate>? commentDelegateMock = null,
            Mock<INoteDelegate>? noteDelegateMock = null,
            Mock<IRatingDelegate>? ratingDelegateMock = null,
            Mock<IInactiveUserService>? inactiveUserServiceMock = null,
            Mock<IFeedbackDelegate>? feedbackDelegateMock = null)
        {
            profileDelegateMock = profileDelegateMock ?? new();
            commentDelegateMock = commentDelegateMock ?? new();
            noteDelegateMock = noteDelegateMock ?? new();
            ratingDelegateMock = ratingDelegateMock ?? new();
            inactiveUserServiceMock = inactiveUserServiceMock ?? new();
            feedbackDelegateMock = feedbackDelegateMock ?? new();

            return new CsvExportService(
                Configuration,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                commentDelegateMock.Object,
                ratingDelegateMock.Object,
                inactiveUserServiceMock.Object,
                feedbackDelegateMock.Object);
        }
    }
}
