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
namespace HealthGateway.GatewayApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Utils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserEmailService userEmailService;
        private readonly IUserProfileService userProfileService;
        private readonly IUserSmsService userSmsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileController"/> class.
        /// </summary>
        /// <param name="userProfileService">The injected user profile service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="userSmsService">The injected user sms service.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        public UserProfileController(
            IUserProfileService userProfileService,
            IHttpContextAccessor httpContextAccessor,
            IUserEmailService userEmailService,
            IUserSmsService userSmsService,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.userProfileService = userProfileService;
            this.httpContextAccessor = httpContextAccessor;
            this.userEmailService = userEmailService;
            this.userSmsService = userSmsService;
            this.authenticationDelegate = authenticationDelegate;
        }

        /// <summary>
        /// Posts a user profile json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The resource hdid.</param>
        /// <param name="createUserRequest">The user profile request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The user profile record was saved.</response>
        /// <response code="400">The user profile was already inserted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserProfileModel>>> CreateUserProfile(string hdid, [FromBody] CreateUserRequest createUserRequest, CancellationToken ct)
        {
            // Validate that the query parameter matches the post body
            if (!hdid.Equals(createUserRequest.Profile.HdId, StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestResult();
            }

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                ClaimsPrincipal user = httpContext.User;
                DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);
                string? jwtEmailAddress = user.FindFirstValue(ClaimTypes.Email);
                return await this.userProfileService.CreateUserProfileAsync(createUserRequest, jwtAuthTime, jwtEmailAddress, ct);
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Gets a user profile json.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public async Task<RequestResult<UserProfileModel>> GetUserProfile(string hdid, CancellationToken ct)
        {
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);

            RequestResult<UserProfileModel> result = await this.userProfileService.GetUserProfileAsync(hdid, jwtAuthTime, ct);
            await this.AddUserPreferences(result.ResourcePayload, ct);

            return result;
        }

        /// <summary>
        /// Gets a result indicating the profile is valid.
        /// </summary>
        /// <returns>A boolean wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The request result is returned.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}/Validate")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public async Task<RequestResult<bool>> Validate(string hdid, CancellationToken ct)
        {
            return await this.userProfileService.ValidateMinimumAgeAsync(hdid, ct);
        }

        /// <summary>
        /// Closes a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Email template may not be present to send the closure email.</response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ExcludeFromCodeCoverage]
        public async Task<RequestResult<UserProfileModel>> CloseUserProfile(string hdid, CancellationToken ct)
        {
            // Retrieve the user identity id from the claims
            Guid userId = new(this.authenticationDelegate.FetchAuthenticatedUserId());

            return await this.userProfileService.CloseUserProfileAsync(hdid, userId, ct);
        }

        /// <summary>
        /// Restore a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Email template may not be present to send the recover email.</response>
        [HttpGet]
        [Route("{hdid}/recover")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ExcludeFromCodeCoverage]
        public async Task<RequestResult<UserProfileModel>> RecoverUserProfile(string hdid, CancellationToken ct)
        {
            return await this.userProfileService.RecoverUserProfileAsync(hdid, ct);
        }

        /// <summary>
        /// Gets the terms of service json.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The terms of service model wrapped in a request result.</returns>
        /// <response code="200">Returns the terms of service json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("termsofservice")]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 3600)]
        public async Task<RequestResult<TermsOfServiceModel>> GetLastTermsOfService(CancellationToken ct)
        {
            return await this.userProfileService.GetActiveTermsOfServiceAsync(ct);
        }

        /// <summary>
        /// Validates a user email verification.
        /// </summary>
        /// <returns>The an empty response.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="verificationKey">The email verification key.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The email was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The verification key was not found.</response>
        [HttpGet]
        [Route("{hdid}/email/validate/{verificationKey}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<bool>>> ValidateEmail(string hdid, Guid verificationKey, CancellationToken ct)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await this.authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct);

                if (accessToken != null)
                {
                    return await this.userEmailService.ValidateEmailAsync(hdid, verificationKey, ct);
                }
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Validates a sms verification.
        /// </summary>
        /// <returns>An empty response.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="validationCode">The sms validation code.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The sms was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The validation code was not found.</response>
        [HttpGet]
        [Route("{hdid}/sms/validate/{validationCode}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<bool>>> ValidateSms(string hdid, string validationCode, CancellationToken ct)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                RequestResult<bool> result = await this.userSmsService.ValidateSmsAsync(hdid, validationCode, ct);
                if (!result.ResourcePayload)
                {
                    await Task.Delay(5000, ct);
                }

                return result;
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">The new email.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        [HttpPut]
        [Route("{hdid}/email")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<bool> UpdateUserEmail(string hdid, [FromBody] string emailAddress, CancellationToken ct)
        {
            return await this.userEmailService.UpdateUserEmailAsync(hdid, emailAddress, ct);
        }

        /// <summary>
        /// Updates the user sms number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="smsNumber">The new sms number.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="400">the client must ensure sms number is valid.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/sms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<bool> UpdateUserSmsNumberAsync(string hdid, [FromBody] string smsNumber, CancellationToken ct)
        {
            return await this.userSmsService.UpdateUserSmsAsync(hdid, smsNumber, ct);
        }

        /// <summary>
        /// Puts a UserPreferenceModel json to be updated in the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The user preference record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserPreferenceModel>>> UpdateUserPreference(string hdid, [FromBody] UserPreferenceModel? userPreferenceModel, CancellationToken ct)
        {
            if (userPreferenceModel == null || string.IsNullOrEmpty(userPreferenceModel.Preference))
            {
                return new BadRequestResult();
            }

            if (userPreferenceModel.HdId != hdid)
            {
                return new ForbidResult();
            }

            userPreferenceModel.UpdatedBy = hdid;
            return await this.userProfileService.UpdateUserPreferenceAsync(userPreferenceModel, ct);
        }

        /// <summary>
        /// Posts a UserPreference json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The comment record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserPreferenceModel>>> CreateUserPreference(string hdid, [FromBody] UserPreferenceModel? userPreferenceModel, CancellationToken ct)
        {
            if (userPreferenceModel == null)
            {
                return new BadRequestResult();
            }

            userPreferenceModel.HdId = hdid;
            userPreferenceModel.CreatedBy = hdid;
            userPreferenceModel.UpdatedBy = hdid;
            return await this.userProfileService.CreateUserPreferenceAsync(userPreferenceModel, ct);
        }

        /// <summary>
        /// Updates the terms of service the user has agreed to.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="termsOfServiceId">The id of the terms of service to update for this user.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/acceptedterms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<RequestResult<UserProfileModel>> UpdateAcceptedTerms(string hdid, [FromBody] Guid termsOfServiceId, CancellationToken ct)
        {
            RequestResult<UserProfileModel> result = await this.userProfileService.UpdateAcceptedTermsAsync(hdid, termsOfServiceId, ct);
            await this.AddUserPreferences(result.ResourcePayload, ct);

            return result;
        }

        /// <summary>
        /// Test phone number validation required by the GatewayAPI and PHSA.
        /// </summary>
        /// <param name="phoneNumber">Phone number stripped of any masked characters.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the submitted phone number conforms to the appropriate standards.</returns>
        /// <response code="200">Returns the result of the validation attempt.</response>
        [HttpGet]
        [Route("IsValidPhoneNumber/{phoneNumber}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsValidPhoneNumber(string phoneNumber, CancellationToken ct)
        {
            return await this.userProfileService.IsPhoneNumberValidAsync(phoneNumber, ct);
        }

        private async Task AddUserPreferences(UserProfileModel? profile, CancellationToken ct)
        {
            if (profile != null)
            {
                RequestResult<Dictionary<string, UserPreferenceModel>> userPreferences = await this.userProfileService.GetUserPreferencesAsync(profile.HdId, ct);
                if (userPreferences.ResourcePayload != null)
                {
                    foreach (KeyValuePair<string, UserPreferenceModel> preference in userPreferences.ResourcePayload)
                    {
                        profile.Preferences.Add(preference);
                    }
                }
            }
        }
    }
}
