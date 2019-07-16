using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthGateway.Engine.Core;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.FhirResponseFactory;
using HealthGateway.Engine.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HealthGateway.Database;
using HealthGateway.Util;

namespace HealthGateway.Service
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly HealthGatewayContext db;

        public UserPreferenceService(DbContextOptions<HealthGatewayContext> dbOptions, IEnvironment env)
        {
            db = new HealthGatewayContext(dbOptions, env);
        }

        public List<string> GetDisplayNames()
        {            
            return db.UserPreferences.Select(up => up.DisplayName).ToList();
        }
   }
}