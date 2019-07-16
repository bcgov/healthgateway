using System;
using System.Collections.Generic;

namespace HealthGateway.Database.Models
{
    public partial class UserPreferences
    {
        public string DisplayName { get; set; }
        public Guid Id { get; set; }
    }
}
