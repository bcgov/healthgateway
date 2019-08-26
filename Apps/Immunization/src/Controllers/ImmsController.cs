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
namespace Immunization.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using HealthGateway.Models;
    /// <summary>
    /// The Immunization controller.
    /// </summary>
    [Authorize]
    [Route("v1/api/[controller]")]
    [ApiController]
    public class ImmsController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmsController"/> class.
        /// </summary>
        /// <param name="svc">The immunization data service.</param>
        public ImmsController(IImmsService svc)
        {
            this.service = svc;
        }

        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private IImmsService service { get; set; }

        /// <summary>
        /// Gets a json list of immunization records.
        /// </summary>
        /// <returns>a list of immunization records.</returns>
        [HttpGet]
        [Route("items")]
        public IEnumerable<ImmsDataModel> GetItems() {
            return this.service.GetImmunizations(); // For now.
        }
    }
}