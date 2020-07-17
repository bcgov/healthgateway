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
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class UserProfileController
    {
        private readonly IUserProfileService userProfileService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserEmailService userEmailService;
        private readonly IUserSMSService userSMSService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileController"/> class.
        /// </summary>
        /// <param name="userProfileService">The injected user profile service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="userSMSService">The injected user sms service.</param>
        public UserProfileController(
            IUserProfileService userProfileService,
            IHttpContextAccessor httpContextAccessor,
            IUserEmailService userEmailService,
            IUserSMSService userSMSService)
        {
            this.userProfileService = userProfileService;
            this.httpContextAccessor = httpContextAccessor;
            this.userEmailService = userEmailService;
            this.userSMSService = userSMSService;
        }

        /// <summary>
        /// Posts a user profile json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The resource hdid.</param>
        /// <param name="createUserRequest">The user profile request model.</param>
        /// <response code="200">The user profile record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult CreateUserProfile([FromHeader] string authorization, string hdid, [FromBody] CreateUserRequest createUserRequest)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            RequestResult<UserProfileModel> result = this.userProfileService.CreateUserProfile(createUserRequest, new Uri(referer), authorization);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a user profile json.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Read)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetUserProfile([FromHeader] string authorization, string hdid)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string rowAuthTime = user.FindFirst(c => c.Type == "auth_time").Value;

            // Auth time at comes in the JWT as seconds after 1970-01-01
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
        /// Closes a user profile.
        /// </summary>
        /// <returns>The user profile model wrapped in a request result.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult CloseUserProfile([FromHeader] string authorization, string hdid)
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
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user profile json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/recover")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult RecoverUserProfile([FromHeader] string authorization, string hdid)
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
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <response code="200">Returns the terms of service json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("termsofservice")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetLastTermsOfService([FromHeader] string authorization)
        {
            RequestResult<TermsOfServiceModel> result = this.userProfileService.GetActiveTermsOfService();
            return new JsonResult(result);
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The an empty response.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <response code="200">The email was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The invite key was not found.</response>
        [HttpGet]
        [Route("{hdid}/email/validate/{inviteKey}")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult ValidateEmail([FromHeader] string authorization, string hdid, Guid inviteKey)
        {
            if (this.userEmailService.ValidateEmail(hdid, inviteKey, authorization))
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
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="validationCode">The sms invite validation code.</param>
        /// <response code="200">The sms was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The invite key was not found.</response>
        [HttpGet]
        [Route("{hdid}/sms/validate/{validationCode}")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult ValidateSMS([FromHeader] string authorization, string hdid, string validationCode)
        {
            if (this.userSMSService.ValidateSMS(hdid, validationCode, authorization))
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
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/email/invite")]
        [Authorize(Policy = UserPolicy.Read)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetUserEmailInvite([FromHeader] string authorization, string hdid)
        {
            MessagingVerification? emailInvite = this.userEmailService.RetrieveLastInvite(hdid);
            UserEmailInvite? result = UserEmailInvite.CreateFromDbModel(emailInvite);
            return new JsonResult(result);
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}/sms/invite")]
        [Authorize(Policy = UserPolicy.Read)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetUserSMSInvite([FromHeader] string authorization, string hdid)
        {
            MessagingVerification? smsInvite = this.userSMSService.RetrieveLastInvite(hdid);
            UserSMSInvite? result = UserSMSInvite.CreateFromDbModel(smsInvite);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">The new email.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/email")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult UpdateUserEmail([FromHeader] string authorization, string hdid, [FromBody] string emailAddress)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            bool result = this.userEmailService.UpdateUserEmail(hdid, emailAddress, new Uri(referer), authorization);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the user sms number.
        /// </summary>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="smsNumber">The new sms number.</param>
        /// <returns>True if the action was successful.</returns>
        /// <response code="200">Returns true if the call was successful.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/sms")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult UpdateUserSMSNumber([FromHeader] string authorization, string hdid, [FromBody] string smsNumber)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            bool result = this.userSMSService.UpdateUserSMS(hdid, smsNumber, new Uri(referer), authorization);
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates a user preference.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="name">The preference name.</param>
        /// <param name="value">The preference value.</param>
        /// <response code="200">The user preference record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}/preference/{name}")]
        [Authorize(Policy = UserPolicy.Write)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult UpdateUserPreference([FromHeader] string authorization, string hdid, string name, [FromBody] string value)
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
