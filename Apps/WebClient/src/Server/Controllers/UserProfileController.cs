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
namespace HealthGateway.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class UserProfileController
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
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Write)]
        public async Task<IActionResult> CreateUserProfile(string hdid, [FromBody] CreateUserRequest createUserRequest)
        {
            // Validate that the query parameter matches the post body
            if (!hdid.Equals(createUserRequest.Profile.HdId, StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestResult();
            }

            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            string bearerToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);

            RequestResult<UserProfileModel> result = await this.userProfileService.CreateUserProfile(createUserRequest, new Uri(referer), bearerToken).ConfigureAwait(true);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a user profile json.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Read)]
        public IActionResult GetUserProfile(string hdid)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            var jsonSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() },
            };

            this.logger.LogTrace($"HTTP context user: {JsonConvert.SerializeObject(user, jsonSettings)}");

            string rowAuthTime = user.FindFirstValue("auth_time"); // auth_time is not mandatory in a Bearer token.

            if (rowAuthTime == null)
            {
                rowAuthTime = user.FindFirstValue("iat"); // get token  "issued at time", which *is* mandatory in JWT bearer token.
            }

            // Auth time comes in the JWT as seconds after 1970-01-01
            DateTime jwtAuthTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(int.Parse(rowAuthTime, CultureInfo.CurrentCulture));

            RequestResult<UserProfileModel> result = this.userProfileService.GetUserProfile(hdid, jwtAuthTime);

            if (result.ResourcePayload != null)
            {
                RequestResult<Dictionary<string, string>> userPreferences = this.userProfileService.GetUserPreferences(hdid);
                result.ResourcePayload.Preferences = userPreferences.ResourcePayload != null ? userPreferences.ResourcePayload : new Dictionary<string, string>();
            }

            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a result indicating the profile is valid.
        /// </summary>
        /// <returns>A boolean wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">The request result is returned.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/Validate")]
        [Authorize(Policy = UserPolicy.Read)]
        public async Task<IActionResult> Validate(string hdid)
        {
            PrimitiveRequestResult<bool> result = await this.userProfileService.ValidateMinimumAge(hdid).ConfigureAwait(true);
            return new JsonResult(result);
        }

        /// <summary>
        /// Closes a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Write)]
        public IActionResult CloseUserProfile(string hdid)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            // Retrieve the user identity id from the claims
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            Guid userId = new Guid(user.FindFirst(ClaimTypes.NameIdentifier).Value);

            RequestResult<UserProfileModel> result = this.userProfileService.CloseUserProfile(hdid, userId, referer);
            return new JsonResult(result);
        }

        /// <summary>
        /// Restore a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/recover")]
        [Authorize(Policy = UserPolicy.Write)]
        public IActionResult RecoverUserProfile(string hdid)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            RequestResult<UserProfileModel> result = this.userProfileService.RecoverUserProfile(hdid, referer);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets the terms of service json.
        /// </summary>
        /// <returns>The terms of service model wrapped in a request result.</returns>
        /// <response code="200">Returns the terms of service json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("termsofservice")]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 3600)]
        public IActionResult GetLastTermsOfService()
        {
            RequestResult<TermsOfServiceModel> result = this.userProfileService.GetActiveTermsOfService();
            return new JsonResult(result);
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The an empty response.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <response code="200">The email was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The invite key was not found.</response>
        [HttpGet]
        [Route("{hdid}/email/validate/{inviteKey}")]
        [Authorize(Policy = UserPolicy.Write)]
        public async Task<IActionResult> ValidateEmail(string hdid, Guid inviteKey)
        {
            string bearerToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);

            if (this.userEmailService.ValidateEmail(hdid, inviteKey, bearerToken))
            {
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }
        }

        /// <summary>
        /// Validates a sms invite.
        /// </summary>
        /// <returns>An empty response.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="validationCode">The sms invite validation code.</param>
        /// <response code="200">The sms was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The invite key was not found.</response>
        [HttpGet]
        [Route("{hdid}/sms/validate/{validationCode}")]
        [Authorize(Policy = UserPolicy.Write)]
        public async Task<IActionResult> ValidateSMS(string hdid, string validationCode)
        {
            string bearerToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);

            if (this.userSMSService.ValidateSMS(hdid, validationCode, bearerToken))
            {
                return new OkResult();
            }
            else
            {
                System.Threading.Thread.Sleep(5000);
                return new NotFoundResult();
            }
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/email/invite")]
        [Authorize(Policy = UserPolicy.Read)]
        public IActionResult GetUserEmailInvite(string hdid)
        {
            MessagingVerification? emailInvite = this.userEmailService.RetrieveLastInvite(hdid);
            UserEmailInvite? result = UserEmailInvite.CreateFromDbModel(emailInvite);
            return new JsonResult(result);
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/sms/invite")]
        [Authorize(Policy = UserPolicy.Read)]
        public IActionResult GetUserSMSInvite(string hdid)
        {
            MessagingVerification? smsInvite = this.userSMSService.RetrieveLastInvite(hdid);
            UserSMSInvite? result = UserSMSInvite.CreateFromDbModel(smsInvite);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">The new email.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/email")]
        [Authorize(Policy = UserPolicy.Write)]
        public async Task<IActionResult> UpdateUserEmail(string hdid, [FromBody] string emailAddress)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            string bearerToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);

            bool result = this.userEmailService.UpdateUserEmail(hdid, emailAddress, new Uri(referer), bearerToken);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the user sms number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="smsNumber">The new sms number.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/sms")]
        [Authorize(Policy = UserPolicy.Write)]
        public async Task<IActionResult> UpdateUserSMSNumber(string hdid, [FromBody] string smsNumber)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            string bearerToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);

            bool result = this.userSMSService.UpdateUserSMS(hdid, smsNumber, new Uri(referer), bearerToken);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates a user preference.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="name">The preference name.</param>
        /// <param name="value">The preference value.</param>
        /// <response code="200">The user preference record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/preference/{name}")]
        [Authorize(Policy = UserPolicy.Write)]
        public IActionResult UpdateUserPreference(string hdid, string name, [FromBody] string value)
        {
            if (name == null)
            {
                return new BadRequestResult();
            }
            else
            {
                bool result = this.userProfileService.UpdateUserPreference(hdid, name, value);
                return new JsonResult(result);
            }
        }
    }
}
