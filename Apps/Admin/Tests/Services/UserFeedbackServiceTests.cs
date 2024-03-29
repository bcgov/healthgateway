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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the UserFeedbackService class.
    /// </summary>
    public class UserFeedbackServiceTests
    {
        private const string Hdid1 = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Hdid2 = "C3GOUHWRP3GTXR2BTY5HEC4YADEV4FPEGCXG2NB5K2USBL52S66S";
        private const string Email1 = "user@gmail.com";
        private const string Email2 = "user@outlook.com";
        private const string DbErrorMessage = "DB Error";
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// AssociateFeedbackTagsAsync.
        /// </summary>
        /// <param name="feedbackDbStatusCode">The db status code for getting user feedback with feedback tags.</param>
        /// <param name="updateFeedbackDbStatusCode">The db status code for updating user feedback with tag associations.</param>
        /// <param name="resultType">The result type returned for calling AssociateFeedbackTagsAsync.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, DbStatusCode.Updated, ResultType.Success)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Error, ResultType.Error)]
        [InlineData(DbStatusCode.NotFound, DbStatusCode.Error, ResultType.Error)]
        public async Task ShouldAssociateFeedbackTags(DbStatusCode feedbackDbStatusCode, DbStatusCode updateFeedbackDbStatusCode, ResultType resultType)
        {
            // Arrange
            const string tagName1 = "Poor";
            const string tagName2 = "Average";
            Guid userFeedbackId = Guid.NewGuid();
            Guid adminTagId1 = Guid.NewGuid();
            Guid adminTagId2 = Guid.NewGuid();

            IList<Tag> adminTags =
            [
                new(adminTagId1, tagName1),
                new(adminTagId2, tagName2),
            ];

            AssociateFeedbackTagMock mock = SetupAssociateFeedbackMock(
                userFeedbackId,
                adminTags,
                feedbackDbStatusCode,
                updateFeedbackDbStatusCode,
                resultType);

            // Act
            RequestResult<UserFeedbackView> actual = await mock.Service.AssociateFeedbackTagsAsync(userFeedbackId, adminTags.Select(x => x.AdminTagId).ToList());

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// CreateTagAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateTag()
        {
            // Arrange
            CreateUserFeedbackMock mock = SetupCreateUserFeedbackMock(false);

            // Act
            RequestResult<AdminTagView> actual = await mock.Service.CreateTagAsync(mock.TagName);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// CreateTagAsync handles error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateTagHandlesDbError()
        {
            // Arrange
            CreateUserFeedbackMock mock = SetupCreateUserFeedbackMock(true);

            // Act
            RequestResult<AdminTagView> actual = await mock.Service.CreateTagAsync(mock.TagName);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// DeleteTagAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteTag()
        {
            // Arrange
            DeleteUserFeedbackMock mock = SetupDeleteUserFeedbackMock(false);

            // Act
            RequestResult<AdminTagView> actual = await mock.Service.DeleteTagAsync(new() { Id = mock.AdminTagId, Name = mock.TagName });

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// DeleteTagAsync handles db error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteTagHandlesDbError()
        {
            // Arrange
            DeleteUserFeedbackMock mock = SetupDeleteUserFeedbackMock(true);

            // Act
            RequestResult<AdminTagView> actual = await mock.Service.DeleteTagAsync(new() { Id = mock.AdminTagId, Name = mock.TagName });

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetAllTagsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAllTags()
        {
            // Arrange
            GetAllFeedbackMock mock = SetupGetAllFeedbackMock();

            // Act
            RequestResult<IList<AdminTagView>> actual = await mock.Service.GetAllTagsAsync();

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetUserFeedbackAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserFeedback()
        {
            // Arrange
            GetUserFeedbackMock mock = SetupGetUserFeedbackMock();

            // Act
            RequestResult<IList<UserFeedbackView>> actual = await mock.Service.GetUserFeedbackAsync();

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// UpdateFeedbackReviewAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateFeedbackReview()
        {
            // Arrange
            Mock<IFeedbackDelegate> feedbackDelegateMock = new();
            UpdateFeedbackMock mock = SetupUpdateFeedbackMock(feedbackDelegateMock);

            // Act
            RequestResult<UserFeedbackView> actual = await mock.Service.UpdateFeedbackReviewAsync(mock.Update);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);

            feedbackDelegateMock.Verify(
                v => v.UpdateUserFeedbackAsync(
                    It.Is<UserFeedback>(x => x.UserProfileId == mock.Update.UserProfileId && x.Id == mock.Update.Id && x.Comment == mock.Update.Comment),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static AdminTag GenerateAdminTag(Guid adminTagId, string name)
        {
            return new()
            {
                AdminTagId = adminTagId,
                Name = name,
            };
        }

        private static AdminTagView GenerateAdminTagView(Guid id, string name)
        {
            return new()
            {
                Id = id,
                Name = name,
            };
        }

        private static UserFeedback GenerateUserFeedback(Guid id, string userProfileId, string comment, ICollection<UserFeedbackTag>? tags = null)
        {
            return new()
            {
                Id = id,
                UserProfileId = userProfileId,
                Comment = comment,
                ClientCode = UserLoginClientType.Web,
                Tags = tags ?? [],
            };
        }

        private static UserFeedbackView GenerateUserFeedbackView(Guid id, string userProfileId, string comment, string? email = null)
        {
            return new()
            {
                Id = id,
                UserProfileId = userProfileId,
                Comment = comment,
                ClientType = UserLoginClientType.Web,
                Email = email ?? string.Empty,
            };
        }

        private static UserFeedbackTag GenerateUserFeedbackTag(Guid userFeedbackId, Guid adminTagId)
        {
            return new()
            {
                UserFeedbackId = userFeedbackId,
                AdminTagId = adminTagId,
            };
        }

        private static UserFeedbackTagView GenerateUserFeedbackTagView(Guid userFeedbackId, Guid adminTagId)
        {
            return new()
            {
                Id = Guid.Empty,
                FeedbackId = userFeedbackId,
                TagId = adminTagId,
            };
        }

        private static UserProfile GenerateUserProfile(string hdid, string email)
        {
            return new()
            {
                HdId = hdid,
                Email = email,
            };
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IUserFeedbackService GetUserFeedbackService(
            Mock<IFeedbackDelegate>? feedbackDelegateMock = null,
            Mock<IAdminTagDelegate>? adminTagDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null)
        {
            feedbackDelegateMock = feedbackDelegateMock ?? new();
            adminTagDelegateMock = adminTagDelegateMock ?? new();
            userProfileDelegateMock = userProfileDelegateMock ?? new();

            return new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                feedbackDelegateMock.Object,
                adminTagDelegateMock.Object,
                userProfileDelegateMock.Object,
                MappingService);
        }

        private static AssociateFeedbackTagMock SetupAssociateFeedbackMock(
            Guid userFeedbackId,
            IList<Tag> tags,
            DbStatusCode feedbackDbStatusCode,
            DbStatusCode updateFeedbackDbStatusCode,
            ResultType resultType)
        {
            const string comment = "Great";

            IList<FeedbackTag> feedbackTags =
            [
                new(userFeedbackId, tags[0].AdminTagId),
                new(userFeedbackId, tags[1].AdminTagId),
            ];

            RequestResult<UserFeedbackView> expected = new()
            {
                ResultStatus = resultType,
                ResourcePayload = resultType == ResultType.Success
                    ? new()
                    {
                        Id = userFeedbackId,
                        UserProfileId = Hdid1,
                        Comment = comment,
                        ClientType = UserLoginClientType.Web,
                        Email = Email1,
                        Tags =
                        [
                            GenerateUserFeedbackTagView(userFeedbackId, feedbackTags[0].AdminTagId),
                            GenerateUserFeedbackTagView(userFeedbackId, feedbackTags[1].AdminTagId),
                        ],
                    }
                    : null,
                ResultError = resultType == ResultType.Error
                    ? new()
                    {
                        ResultMessage = DbErrorMessage,
                    }
                    : null,
            };

            IList<Guid> adminTagIds = tags.Select(x => x.AdminTagId).ToList();

            ICollection<UserFeedbackTag> userFeedbackTags =
            [
                GenerateUserFeedbackTag(feedbackTags[0].UserFeedbackId, feedbackTags[0].AdminTagId),
                GenerateUserFeedbackTag(feedbackTags[1].UserFeedbackId, feedbackTags[1].AdminTagId),
            ];

            DbResult<UserFeedback> userFeedbackDbResult = new()
            {
                Status = feedbackDbStatusCode,
                Payload = feedbackDbStatusCode == DbStatusCode.Read ? GenerateUserFeedback(userFeedbackId, Hdid1, comment) : null!,
                Message = feedbackDbStatusCode == DbStatusCode.NotFound ? DbErrorMessage : string.Empty,
            };

            DbResult<UserFeedback> updateUserFeedbackDbResult = new()
            {
                Status = updateFeedbackDbStatusCode,
                Payload = updateFeedbackDbStatusCode == DbStatusCode.Updated ? GenerateUserFeedback(userFeedbackId, Hdid1, comment, userFeedbackTags) : null!,
                Message = updateFeedbackDbStatusCode == DbStatusCode.Error ? DbErrorMessage : string.Empty,
            };

            IList<AdminTag> adminTags =
            [
                GenerateAdminTag(tags[0].AdminTagId, tags[0].TagName),
                GenerateAdminTag(tags[1].AdminTagId, tags[1].TagName),
            ];

            DbResult<IEnumerable<AdminTag>> adminTagDbResult = new()
            {
                Status = DbStatusCode.Read,
                Payload = adminTags,
            };

            UserProfile userProfile = GenerateUserProfile(Hdid1, Email1);

            Mock<IFeedbackDelegate> feedbackDelegateMock = new();
            feedbackDelegateMock.Setup(s => s.GetUserFeedbackWithFeedbackTagsAsync(userFeedbackId, It.IsAny<CancellationToken>())).ReturnsAsync(userFeedbackDbResult);
            feedbackDelegateMock
                .Setup(s => s.UpdateUserFeedbackWithTagAssociationsAsync(It.Is<UserFeedback>(x => x.Id == userFeedbackId && x.Tags.Count == adminTags.Count()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateUserFeedbackDbResult);

            Mock<IAdminTagDelegate> adminTagDelegateMock = new();
            adminTagDelegateMock.Setup(s => s.GetAdminTagsAsync(adminTagIds, It.IsAny<CancellationToken>())).ReturnsAsync(adminTagDbResult);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileAsync(Hdid1, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            IUserFeedbackService service = GetUserFeedbackService(
                adminTagDelegateMock: adminTagDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                feedbackDelegateMock: feedbackDelegateMock);

            return new(service, expected);
        }

        private static CreateUserFeedbackMock SetupCreateUserFeedbackMock(bool dbErrorExists)
        {
            const string tagName = "Great";

            RequestResult<AdminTagView> expected = new()
            {
                ResultStatus = dbErrorExists ? ResultType.Error : ResultType.Success,
                ResultError = dbErrorExists
                    ? new()
                    {
                        ResultMessage = DbErrorMessage,
                    }
                    : null,
                ResourcePayload = dbErrorExists ? null : new() { Name = tagName },
            };

            DbResult<AdminTag?> adminTagDbResult = new()
            {
                Status = dbErrorExists ? DbStatusCode.Error : DbStatusCode.Created,
                Payload = dbErrorExists ? null : new() { Name = tagName },
                Message = dbErrorExists ? DbErrorMessage : string.Empty,
            };

            Mock<IAdminTagDelegate> adminTagDelegateMock = new();
            adminTagDelegateMock.Setup(s => s.AddAsync(It.Is<AdminTag>(x => x.Name == tagName), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(adminTagDbResult!);

            IUserFeedbackService service = GetUserFeedbackService(adminTagDelegateMock: adminTagDelegateMock);
            return new(service, expected, tagName);
        }

        private static DeleteUserFeedbackMock SetupDeleteUserFeedbackMock(bool dbErrorExists)
        {
            Guid adminTagId = Guid.NewGuid();
            const string tagName = "Great";

            RequestResult<AdminTagView> expected = new()
            {
                ResourcePayload = dbErrorExists
                    ? null
                    : new()
                    {
                        Id = adminTagId,
                        Name = tagName,
                    },
                ResultError = dbErrorExists
                    ? new()
                    {
                        ResultMessage = DbErrorMessage,
                    }
                    : null,
                ResultStatus = dbErrorExists ? ResultType.Error : ResultType.Success,
            };

            DbResult<AdminTag?> adminTagDbResult = new()
            {
                Status = dbErrorExists ? DbStatusCode.Error : DbStatusCode.Deleted,
                Payload = dbErrorExists
                    ? null
                    : new()
                    {
                        AdminTagId = adminTagId,
                        Name = tagName,
                    },
                Message = dbErrorExists ? DbErrorMessage : string.Empty,
            };

            Mock<IAdminTagDelegate> adminTagDelegateMock = new();
            adminTagDelegateMock.Setup(
                    s => s.DeleteAsync(It.Is<AdminTag>(x => x.AdminTagId == adminTagId && x.Name == tagName), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(adminTagDbResult!);

            IUserFeedbackService service = GetUserFeedbackService(adminTagDelegateMock: adminTagDelegateMock);
            return new(service, expected, adminTagId, tagName);
        }

        private static GetUserFeedbackMock SetupGetUserFeedbackMock()
        {
            const string comment1 = "ABC";
            const string comment2 = "123";
            Guid feedbackId1 = Guid.NewGuid();
            Guid feedbackId2 = Guid.NewGuid();
            Guid feedbackId3 = Guid.NewGuid();

            IList<Profile> profiles =
            [
                new(Hdid1, Email1),
                new(Hdid2, Email2),
            ];

            IList<Feedback> feedbacks =
            [
                new(feedbackId1, Hdid1, comment1),
                new(feedbackId2, Hdid1, comment2),
                new(feedbackId3, Hdid2, comment1),
            ];

            RequestResult<IList<UserFeedbackView>> expected = new()
            {
                ResourcePayload =
                [
                    GenerateUserFeedbackView(feedbackId1, Hdid1, comment1, Email1),
                    GenerateUserFeedbackView(feedbackId2, Hdid1, comment2, Email1),
                    GenerateUserFeedbackView(feedbackId3, Hdid2, comment1, Email2),
                ],
                ResultStatus = ResultType.Success,
                TotalResultCount = feedbacks.Count,
            };

            IList<string> userProfileIds = profiles.Select(x => x.Hdid).ToList();

            IList<UserFeedback> userFeedbacks =
            [
                GenerateUserFeedback(feedbacks[0].FeedbackId, feedbacks[0].Hdid, feedbacks[0].Comment),
                GenerateUserFeedback(feedbacks[1].FeedbackId, feedbacks[1].Hdid, feedbacks[1].Comment),
                GenerateUserFeedback(feedbacks[2].FeedbackId, feedbacks[2].Hdid, feedbacks[2].Comment),
            ];

            IList<UserProfile> userProfiles =
            [
                GenerateUserProfile(profiles[0].Hdid, profiles[0].Email),
                GenerateUserProfile(profiles[1].Hdid, profiles[1].Email),
            ];

            Mock<IFeedbackDelegate> feedbackDelegateMock = new();
            feedbackDelegateMock.Setup(s => s.GetAllUserFeedbackEntriesAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(userFeedbacks);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfilesAsync(userProfileIds, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);

            IUserFeedbackService service = GetUserFeedbackService(feedbackDelegateMock, userProfileDelegateMock: userProfileDelegateMock);

            return new(service, expected);
        }

        private static UpdateFeedbackMock SetupUpdateFeedbackMock(Mock<IFeedbackDelegate> feedbackDelegateMock)
        {
            const string comment = "ABC";
            Guid feedbackId = Guid.NewGuid();

            UserFeedbackView update = GenerateUserFeedbackView(feedbackId, Hdid1, comment);

            RequestResult<UserFeedbackView> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = GenerateUserFeedbackView(feedbackId, Hdid1, comment, Email1),
            };
            DbResult<UserFeedback> dbResult = new()
            {
                Status = DbStatusCode.Read,
                Payload = GenerateUserFeedback(feedbackId, Hdid1, comment),
            };

            UserProfile userProfile = GenerateUserProfile(Hdid1, Email1);

            feedbackDelegateMock.Setup(s => s.GetUserFeedbackWithFeedbackTagsAsync(feedbackId, It.IsAny<CancellationToken>())).ReturnsAsync(dbResult);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileAsync(Hdid1, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            IUserFeedbackService service = GetUserFeedbackService(feedbackDelegateMock, userProfileDelegateMock: userProfileDelegateMock);

            return new(service, expected, update);
        }

        private static GetAllFeedbackMock SetupGetAllFeedbackMock()
        {
            Guid adminTagId1 = Guid.NewGuid();
            Guid adminTagId2 = Guid.NewGuid();
            const string tagName1 = "Test1";
            const string tagName2 = "Test2";

            IList<Dictionary<Guid, string>> tags =
            [
                new()
                {
                    { adminTagId1, tagName1 },
                },
                new()
                {
                    { adminTagId2, tagName2 },
                },
            ];

            RequestResult<IList<AdminTagView>> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload =
                [
                    GenerateAdminTagView(adminTagId1, tagName1),
                    GenerateAdminTagView(adminTagId2, tagName2),
                ],
                TotalResultCount = tags.Count,
            };

            IEnumerable<AdminTag> adminTags = tags
                .SelectMany(dict => dict.Select(kv => GenerateAdminTag(kv.Key, kv.Value)));

            Mock<IAdminTagDelegate> adminTagDelegateMock = new();
            adminTagDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(adminTags);

            IUserFeedbackService service = GetUserFeedbackService(adminTagDelegateMock: adminTagDelegateMock);
            return new(service, expected);
        }

        private record Tag(Guid AdminTagId, string TagName);

        private record Profile(string Hdid, string Email);

        private record Feedback(Guid FeedbackId, string Hdid, string Comment);

        private record FeedbackTag(Guid UserFeedbackId, Guid AdminTagId);

        private record AssociateFeedbackTagMock(IUserFeedbackService Service, RequestResult<UserFeedbackView> Expected);

        private record CreateUserFeedbackMock(IUserFeedbackService Service, RequestResult<AdminTagView> Expected, string TagName);

        private record DeleteUserFeedbackMock(IUserFeedbackService Service, RequestResult<AdminTagView> Expected, Guid AdminTagId, string TagName);

        private record GetAllFeedbackMock(IUserFeedbackService Service, RequestResult<IList<AdminTagView>> Expected);

        private record GetUserFeedbackMock(IUserFeedbackService Service, RequestResult<IList<UserFeedbackView>> Expected);

        private record UpdateFeedbackMock(IUserFeedbackService Service, RequestResult<UserFeedbackView> Expected, UserFeedbackView Update);
    }
}
