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
namespace HealthGateway.Mock.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Utils;
    using HealthGateway.Mock.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The PHSA mock controller.
    /// </summary>
    [Route("[controller]")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Method signatures match mocked endpoints.")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Mocked Controller methods.")]
    public class PhsaController : ControllerBase
    {
        /// <summary>
        /// Gets mock data for laboratory orders.
        /// </summary>
        /// <param name="subjectHdid">The HDID of the patient.</param>
        /// <returns>The mocked laboratory order json.</returns>
        [HttpGet]
        [Route("LaboratoryOrders/LabSummary")]
        [Produces("application/json")]
        public ContentResult LaboratoryOrders([FromQuery] string subjectHdid)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.LaboratoryOrders.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for COVID-19 orders.
        /// </summary>
        /// <param name="subjectHdid">The HDID of the patient.</param>
        /// <param name="limit">The limit on the number of records returned.</param>
        /// <returns>The mocked COVID-19 order json.</returns>
        [HttpGet]
        [Route("Covid19Orders")]
        [Produces("application/json")]
        public ContentResult Covid19Orders([FromQuery] string subjectHdid, [FromQuery] string limit)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.Covid19Orders.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for COVID-19 reports.
        /// </summary>
        /// <param name="id">The laboratory order ID.</param>
        /// <param name="subjectHdid">The HDID of the patient.</param>
        /// <returns>The mocked COVID-19 report json.</returns>
        [HttpGet]
        [Route("Covid19Orders/{id}/LabReportDocument")]
        [Produces("application/json")]
        public ContentResult Covid19Report(string id, [FromQuery] string subjectHdid)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.LabReport.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for laboratory reports.
        /// </summary>
        /// <param name="id">The laboratory order ID.</param>
        /// <param name="subjectHdid">The HDID of the patient.</param>
        /// <returns>The mocked laboratory report json.</returns>
        [HttpGet]
        [Route("LaboratoryOrders/{id}/LabReportDocument")]
        [Produces("application/json")]
        public ContentResult LabReport(string id, [FromQuery] string subjectHdid)
        {
            string? labReportPayload = AssetReader.Read("HealthGateway.Mock.Assets.LabReport.json");
            return new ContentResult { Content = labReportPayload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for immunizations.
        /// </summary>
        /// <returns>The mocked immunization json.</returns>
        [HttpGet]
        [Route("immunizations")]
        [Produces("application/json")]
        public ContentResult Immunizations()
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.Immunizations.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for a particular immunization.
        /// </summary>
        /// <param name="immunizationId">The immunization ID.</param>
        /// <returns>The mocked immunization json.</returns>
        [HttpGet]
        [Route("immunizations/{immunizationId}")]
        [Produces("application/json")]
        public ContentResult Immunization(string immunizationId)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.Immunization.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for a particular vaccine status.
        /// </summary>
        /// <returns>The mocked vaccine status json.</returns>
        [HttpPost]
        [Route("vaccineStatus")]
        [Produces("application/json")]
        public ContentResult VaccineStatus()
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.VaccineStatus.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Mocks the endpoint for updating notification settings.
        /// </summary>
        /// <param name="request">The new notification settings.</param>
        /// <returns>The mocked notification settings json.</returns>
        [HttpPut]
        [Route("notificationSettings")]
        [Produces("application/json")]
        public ContentResult UpdateNotificationSettings([FromBody] NotificationSettingRequest request)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.NotificationSettings-Put.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }

        /// <summary>
        /// Mocks the endpoint for retrieving covid immunization events.
        /// </summary>
        /// <param name="phn">The phn of the patient.</param>
        /// <param name="dob">The date of birth of the patient.</param>
        /// <returns>The mocked covid immunization response json.</returns>
        [HttpPost]
        [Route("Support/Immunizations")]
        [Produces("application/json")]
        public ContentResult GetCovidImmunizations([FromHeader] string phn, [FromHeader] string dob)
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.CovidImmunizations.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }
    }
}
