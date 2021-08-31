//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Utils;
    using HealthGateway.Immunization.Constants;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public class VaccineStatusService : IVaccineStatusService
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private const string AuthConfigSectionName = "ClientAuthentication";
        private const string BackgroundBlue = "#38598a";
        private const string BackgroundGreen = "#2e8540";
        private const string BorderDashed = "dashed";
        private const string BorderSolid = "solid";
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly ICaptchaDelegate captchaDelegate;
        private readonly IIronPDFDelegate ironPdfDelegate;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly PHSAConfig phsaConfig;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="vaccineStatusDelegate">The injected vaccine status delegate.</param>
        /// <param name="captchaDelegate">The injected captcha delegate.</param>
        /// <param name="ironPdfDelegate">The injected ironpdf delegate.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            ICaptchaDelegate captchaDelegate,
            IIronPDFDelegate ironPdfDelegate)
        {
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.authDelegate = authDelegate;
            this.captchaDelegate = captchaDelegate;
            this.ironPdfDelegate = ironPdfDelegate;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password

            this.phsaConfig = new PHSAConfig();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus(string phn, string dateOfBirth, string token)
        {
            RequestResult<VaccineStatus> retVal = new ()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            DateTime dob;
            try
            {
                dob = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing date of birth",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            if (!PHNValidator.IsValid(phn))
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing phn",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            bool isTokenValid = await this.captchaDelegate.IsCaptchaValid(token).ConfigureAwait(true);
            if (!isTokenValid)
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Invalid captcha token",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            VaccineStatusQuery query = new ()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = dob,
            };

            string? accessToken = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest).AccessToken;
            RequestResult<PHSAResult<VaccineStatusResult>> result =
                await this.vaccineStatusDelegate.GetVaccineStatus(query, accessToken).ConfigureAwait(true);
            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload == null)
            {
                retVal.ResourcePayload = new VaccineStatus();
                retVal.ResourcePayload.State = Constants.VaccineState.NotFound;
            }
            else
            {
                retVal.ResourcePayload = new VaccineStatus()
                {
                    Birthdate = payload.Birthdate,
                    PersonalHealthNumber = phn,
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    Doses = payload.DoseCount,
                    State = Enum.Parse<VaccineState>(payload.StatusIndicator),
                    QRCode = payload.QRCode,
                };
            }

            if (result.ResourcePayload != null)
            {
                retVal.ResourcePayload.Loaded = !result.ResourcePayload.LoadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(result.ResourcePayload.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GetVaccineStatusPDF(string phn, string dateOfBirth, string token)
        {
            RequestResult<VaccineStatus> requestResult = await this.GetVaccineStatus(phn, dateOfBirth, token).ConfigureAwait(true);
            IronPDFRequestModel pdfRequest = new ();
            pdfRequest.FileName = "BCVaccineCard";
            pdfRequest.Data.Add("bcLogoImageSrc", AssetReader.Read("HealthGateway.Immunization.Assets.Images.bcid-logo-rev-en.png", true));
            pdfRequest.HtmlTemplate = AssetReader.Read("HealthGateway.Immunization.Assets.Templates.VaccineStatusCard.html") !;

            if (requestResult.ResultStatus != ResultType.Success || requestResult.ResourcePayload == null)
            {
                return new RequestResult<ReportModel>()
                {
                    ResultStatus = requestResult.ResultStatus,
                    ResultError = requestResult.ResultError,
                };
            }

            pdfRequest.Data.Add("birthdate", requestResult.ResourcePayload.Birthdate!.Value.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture));
            pdfRequest.Data.Add("name", $"{requestResult.ResourcePayload.FirstName} {requestResult.ResourcePayload.LastName}");
            pdfRequest.Data.Add("qrCodeImageSrc", requestResult.ResourcePayload.QRCode.Data);
            switch (requestResult.ResourcePayload.State)
            {
                case VaccineState.AllDosesReceived:
                    pdfRequest.Data.Add("resultText", "Fully Vaccinated");
                    pdfRequest.Data.Add("resultColor", BackgroundGreen);
                    pdfRequest.Data.Add("resultBorder", BorderSolid);
                    pdfRequest.Data.Add("resultImageSrc", AssetReader.Read("HealthGateway.Immunization.Assets.Images.fully-vaccinated.svg", true));
                    break;
                case VaccineState.PartialDosesReceived:
                    pdfRequest.Data.Add("resultText", "Partially Vaccinated");
                    pdfRequest.Data.Add("resultColor", BackgroundBlue);
                    pdfRequest.Data.Add("resultBorder", BorderDashed);
                    string dosesImage = requestResult.ResourcePayload.Doses > 1 ? "2" : "1";
                    pdfRequest.Data.Add("resultImageSrc", AssetReader.Read($"HealthGateway.Immunization.Assets.Images.dose-{dosesImage}.svg", true));
                    break;
                case VaccineState.Exempt:
                    pdfRequest.Data.Add("resultText", "Exempt");
                    pdfRequest.Data.Add("resultColor", BackgroundBlue);
                    pdfRequest.Data.Add("resultBorder", BorderDashed);
                    pdfRequest.Data.Add("resultImageSrc", AssetReader.Read("HealthGateway.Immunization.Assets.Images.no-doses.svg", true));
                    break;
                default:
                    pdfRequest.Data.Add("resultText", "No Records Found");
                    pdfRequest.Data.Add("resultColor", BackgroundBlue);
                    pdfRequest.Data.Add("resultBorder", BorderDashed);
                    pdfRequest.Data.Add("resultImageSrc", AssetReader.Read("HealthGateway.Immunization.Assets.Images.no-doses.svg", true));
                    break;
            }

            return this.ironPdfDelegate.GeneratePDF(pdfRequest);
        }
    }
}
