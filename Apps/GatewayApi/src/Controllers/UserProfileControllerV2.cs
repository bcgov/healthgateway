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
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using FluentValidation;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Utils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("2.0")]
    [Route("UserProfile")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class UserProfileControllerV2 : ControllerBase
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserEmailServiceV2 userEmailService;
        private readonly IUserProfileServiceV2 userProfileService;
        private readonly IUserSmsServiceV2 userSmsService;
        private readonly IUserPreferenceServiceV2 userPreferenceService;
        private readonly ILegalAgreementServiceV2 legalAgreementService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileControllerV2"/> class.
        /// </summary>
        /// <param name="userProfileService">The injected user profile service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="userSmsService">The injected user sms service.</param>
        /// <param name="userPreferenceService">The injected user preference service.</param>
        /// <param name="legalAgreementService">The injected legal agreement service.</param>
        public UserProfileControllerV2(
            IUserProfileServiceV2 userProfileService,
            IHttpContextAccessor httpContextAccessor,
            IUserEmailServiceV2 userEmailService,
            IUserSmsServiceV2 userSmsService,
            IUserPreferenceServiceV2 userPreferenceService,
            ILegalAgreementServiceV2 legalAgreementService)
        {
            this.userProfileService = userProfileService;
            this.httpContextAccessor = httpContextAccessor;
            this.userEmailService = userEmailService;
            this.userSmsService = userSmsService;
            this.userPreferenceService = userPreferenceService;
            this.legalAgreementService = legalAgreementService;
        }

        /// <summary>
        /// Creates a new user profile.
        /// </summary>
        /// <param name="hdid">The resource hdid.</param>
        /// <param name="createUserRequest">The user profile request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The user profile.</returns>
        /// <response code="200">The user's profile has been created.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">The patient could not be found.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="502">Upstream error.</response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType<UserProfileModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<UserProfileModel>> CreateUserProfile(string hdid, [FromBody] CreateUserRequest createUserRequest, CancellationToken ct)
        {
            if (!hdid.Equals(createUserRequest.Profile.HdId, StringComparison.OrdinalIgnoreCase))
            {
                return new ForbidResult();
            }

            ClaimsPrincipal user = this.httpContextAccessor.HttpContext!.User;
            DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);
            string? jwtEmailAddress = user.FindFirstValue(ClaimTypes.Email);
            return await this.userProfileService.CreateUserProfileAsync(createUserRequest, jwtAuthTime, jwtEmailAddress, ct);
        }

        /// <summary>
        /// Gets a user profile.
        /// </summary>
        /// <returns>The requested user profile, or an empty profile if the requested profile doesn't exist.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the requested user profile, or an empty profile if the requested profile doesn't exists.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Internal server error.</response>
        /// <response code="502">Upstream error.</response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [Produces("application/json")]
        [ProducesResponseType<UserProfileModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<UserProfileModel> GetUserProfile(string hdid, CancellationToken ct)
        {
            DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(this.httpContextAccessor.HttpContext!.User);
            return await this.userProfileService.GetUserProfileAsync(hdid, jwtAuthTime, ct);
        }

        /// <summary>
        /// Validates a user is eligible to create a Health Gateway account.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean value indicating whether the user is eligible to create a Health Gateway account.</returns>
        /// <response code="200">
        /// Returns a boolean value indicating whether the user is eligible to create a Health Gateway
        /// account.
        /// </response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="502">Upstream error.</response>
        [HttpGet]
        [Route("{hdid}/Validate")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [Produces("application/json")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<bool> Validate(string hdid, CancellationToken ct)
        {
            return await this.userProfileService.ValidateEligibilityAsync(hdid, ct);
        }

        /// <summary>
        /// Closes a user profile.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The user's profile has been closed.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseUserProfile(string hdid, CancellationToken ct)
        {
            await this.userProfileService.CloseUserProfileAsync(hdid, ct);
            return this.Ok();
        }

        /// <summary>
        /// Restores a user profile.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The user's profile has been restored.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Route("{hdid}/recover")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecoverUserProfile(string hdid, CancellationToken ct)
        {
            await this.userProfileService.RecoverUserProfileAsync(hdid, ct);
            return this.Ok();
        }

        /// <summary>
        /// Gets the terms of service.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The terms of service model.</returns>
        /// <response code="200">Returns the terms of service.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Route("termsofservice")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 3600)]
        public async Task<TermsOfServiceModel> GetLastTermsOfService(CancellationToken ct)
        {
            return await this.legalAgreementService.GetActiveTermsOfServiceAsync(ct);
        }

        /// <summary>
        /// Validates an email address.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="verificationKey">The email verification key.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean value indicating whether the validation was successful.</returns>
        /// <response code="200">Returns a boolean value indicating whether the validation was successful.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The verification key was not found.</response>
        /// <response code="409">The validation has already been performed.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Route("{hdid}/email/validate/{verificationKey}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<bool> ValidateEmail(string hdid, Guid verificationKey, CancellationToken ct)
        {
            return await this.userEmailService.ValidateEmailAsync(hdid, verificationKey, ct);
        }

        /// <summary>
        /// Validates an SMS number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="validationCode">The SMS validation code.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean value indicating whether the validation was successful.</returns>
        /// <response code="200">Returns a boolean value indicating whether the validation was successful.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="502">Upstream error.</response>
        [HttpGet]
        [Route("{hdid}/sms/validate/{validationCode}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<bool> ValidateSms(string hdid, string validationCode, CancellationToken ct)
        {
            return await this.userSmsService.ValidateSmsAsync(hdid, validationCode, ct);
        }

        /// <summary>
        /// Updates a user's email address.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="emailAddress">The new email.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The user's email address has been successfully updated.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut]
        [Route("{hdid}/email")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserEmail(string hdid, [FromBody] string emailAddress = "", CancellationToken ct = default)
        {
            await this.userEmailService.UpdateUserEmailAsync(hdid, emailAddress, ct);
            return this.Ok();
        }

        /// <summary>
        /// Updates a user's SMS number.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="smsNumber">The new sms number.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The user's SMS number has been updated.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut]
        [Route("{hdid}/sms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserSmsNumberAsync(string hdid, [FromBody] string smsNumber, CancellationToken ct)
        {
            await this.userSmsService.UpdateUserSmsAsync(hdid, smsNumber, ct);
            return this.Ok();
        }

        /// <summary>
        /// Updates an existing user preference.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated user preference.</returns>
        /// <response code="200">The user preference record was saved.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Internal server error.</response>
        [HttpPut]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType<UserPreferenceModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserPreferenceModel>> UpdateUserPreference(string hdid, [FromBody] UserPreferenceModel? userPreferenceModel, CancellationToken ct)
        {
            await new UserPreferenceModelValidator().ValidateAndThrowAsync(userPreferenceModel, ct);
            if (!hdid.Equals(userPreferenceModel?.HdId, StringComparison.OrdinalIgnoreCase))
            {
                return new ForbidResult();
            }

            return await this.userPreferenceService.UpdateUserPreferenceAsync(hdid, userPreferenceModel, ct);
        }

        /// <summary>
        /// Creates a new user preference.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="userPreferenceModel">The user preference request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The created user preference.</returns>
        /// <response code="200">The comment record was saved.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Route("{hdid}/preference")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType<UserPreferenceModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<UserPreferenceModel> CreateUserPreference(string hdid, [FromBody] UserPreferenceModel userPreferenceModel, CancellationToken ct)
        {
            await new UserPreferenceModelValidator().ValidateAndThrowAsync(userPreferenceModel, ct);
            return await this.userPreferenceService.CreateUserPreferenceAsync(hdid, userPreferenceModel, ct);
        }

        /// <summary>
        /// Updates a user's profile to capture approval of the terms of service.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="termsOfServiceId">The ID of the terms of service to approve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The user's acceptance of the terms of service was captured successfully.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">User profile was not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut]
        [Route("{hdid}/acceptedterms")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAcceptedTerms(string hdid, [FromBody] Guid termsOfServiceId, CancellationToken ct)
        {
            await this.userProfileService.UpdateAcceptedTermsAsync(hdid, termsOfServiceId, ct);
            return this.Ok();
        }

        /// <summary>
        /// Determines whether a phone number is valid.
        /// </summary>
        /// <param name="phoneNumber">Phone number stripped of any mask characters.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean value indicating whether the phone number is valid.</returns>
        /// <response code="200">Returns a boolean value indicating whether the phone number is valid.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        [HttpGet]
        [Route("IsValidPhoneNumber/{phoneNumber}")]
        [Produces("application/json")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<bool> IsValidPhoneNumber(string phoneNumber, CancellationToken ct)
        {
            return await this.userProfileService.IsPhoneNumberValidAsync(phoneNumber, ct);
        }
    }
}
