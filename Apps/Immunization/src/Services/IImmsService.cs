using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;
using HealthGateway.Models;

namespace HealthGateway.Service
{
    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public interface IImmsService
    {
        /// <summary>
        /// Gets a list of mock immunization records.
        /// </summary>
        /// <returns>A list of ImmsDataModel object</returns>
        IEnumerable<ImmsDataModel> GetMockData();
    }
}