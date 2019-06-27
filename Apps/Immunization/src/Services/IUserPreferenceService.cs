using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;

namespace HealthGateway.Service
{
    // Service interface inspired/based on https://github.com/FirelyTeam/spark
    public interface IUserPreferenceService
    {
        List<string> GetDisplayNames();
    }
}