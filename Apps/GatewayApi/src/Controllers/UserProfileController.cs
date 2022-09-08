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
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Utils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IUserProfileService userProfileService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserEmailService userEmailService;
        private readonly IUserSMSService userSMSService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileController"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="userProfileService">The injected user profile service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="userSMSService">The injected user sms service.</param>
        public UserProfileController(
            ILogger<UserProfileController> logger,
            IUserProfileService userProfileService,
            IHttpContextAccessor httpContextAccessor,
            IUserEmailService userEmailService,
            IUserSMSService userSMSService)
        {
            this.logger = logger;
            this.userProfileService = userProfileService;
            this.httpContextAccessor = httpContextAccessor;
            this.userEmailService = userEmailService;
            this.userSMSService = userSMSService;
        }

        /// <summary>
        /// Posts a user profile json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The resource hdid.</param>
        /// <param name="createUserRequest">The user profile request model.</param>
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
        public async Task<ActionResult<RequestResult<UserProfileModel>>> CreateUserProfile(string hdid, [FromBody] CreateUserRequest createUserRequest)
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
                return await this.userProfileService.CreateUserProfile(createUserRequest, jwtAuthTime, jwtEmailAddress).ConfigureAwait(true);
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Gets a user profile json.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public RequestResult<UserProfileModel> GetUserProfile(string hdid)
        {
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);

            RequestResult<UserProfileModel> result = this.userProfileService.GetUserProfile(hdid, jwtAuthTime);
            this.AddUserPreferences(result.ResourcePayload);

            return result;
        }

        /// <summary>
        /// Gets a result indicating the profile is valid.
        /// </summary>
        /// <returns>A boolean wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">The request result is returned.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}/Validate")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public async Task<PrimitiveRequestResult<bool>> Validate(string hdid)
        {
            return await this.userProfileService.ValidateMinimumAge(hdid).ConfigureAwait(true);
        }

        /// <summary>
        /// Closes a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ExcludeFromCodeCoverage]
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid)
        {
            // Retrieve the user identity id from the claims
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext!.User;
            Guid userId = new(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return this.userProfileService.CloseUserProfile(hdid, userId);
        }

        /// <summary>
        /// Restore a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}/recover")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ExcludeFromCodeCoverage]
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid)
        {
            return this.userProfileService.RecoverUserProfile(hdid);
        }

        /// <summary>
        /// Gets the terms of service json.
        /// </summary>
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
        public RequestResult<TermsOfServiceModel> GetLastTermsOfService()
        {
            return this.userProfileService.GetActiveTermsOfService();
        }

        /// <summary>
        /// Validates a user email verification.
        /// </summary>
        /// <returns>The an empty response.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="verificationKey">The email verification key.</param>
        /// <response code="200">The email was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The verification key was not found.</response>
        [HttpGet]
        [Route("{hdid}/email/validate/{verificationKey}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<PrimitiveRequestResult<bool>>> ValidateEmail(string hdid, Guid verificationKey)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    return this.userEmailService.ValidateEmail(hdid, verificationKey);
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
        /// <response code="200">The sms was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The validation code was not found.</response>
        [HttpGet]
        [Route("{hdid}/sms/validate/{validationCode}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<PrimitiveRequestResult<bool>>> ValidateSMS(string hdid, string validationCode)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                PrimitiveRequestResult<bool> result = this.userSMSService.ValidateSMS(hdid, validationCode);
                if (!result.ResourcePayload)
                {
                    await Task.Delay(5000).ConfigureAwait(true);
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
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/email")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public bool UpdateUserEmail(string hdid, [FromBody] string emailAddress)
        {
            return this.userEmailService.UpdateUserEmail(hdid, emailAddress);
        }

        /// <summary>
        /// Updates the user sms number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="smsNumber">The new sms number.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/sms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public bool UpdateUserSMSNumber(string hdid, [FromBody] string smsNumber)
        {
            return this.userSMSService.UpdateUserSMS(hdid, smsNumber);
        }

        /// <summary>
        /// Puts a UserPreferenceModel json to be updated in the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <response code="200">The user preference record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public ActionResult<RequestResult<UserPreferenceModel>> UpdateUserPreference(string hdid, [FromBody] UserPreferenceModel userPreferenceModel)
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
            return this.userProfileService.UpdateUserPreference(userPreferenceModel);
        }

        /// <summary>
        /// Posts a UserPreference json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <response code="200">The comment record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public ActionResult<RequestResult<UserPreferenceModel>> CreateUserPreference(string hdid, [FromBody] UserPreferenceModel userPreferenceModel)
        {
            if (userPreferenceModel == null)
            {
                return new BadRequestResult();
            }

            userPreferenceModel.HdId = hdid;
            userPreferenceModel.CreatedBy = hdid;
            userPreferenceModel.UpdatedBy = hdid;
            return this.userProfileService.CreateUserPreference(userPreferenceModel);
        }

        /// <summary>
        /// Updates the terms of service the user has agreed to.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="termsOfServiceId">The id of the terms of service to update for this user.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/acceptedterms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public RequestResult<UserProfileModel> UpdateAcceptedTerms(string hdid, [FromBody] Guid termsOfServiceId)
        {
            RequestResult<UserProfileModel> result = this.userProfileService.UpdateAcceptedTerms(hdid, termsOfServiceId);
            this.AddUserPreferences(result.ResourcePayload);

            return result;
        }

        private void AddUserPreferences(UserProfileModel profile)
        {
            if (profile != null)
            {
                RequestResult<Dictionary<string, UserPreferenceModel>> userPreferences = this.userProfileService.GetUserPreferences(profile.HdId);
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
