using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGateway.AdminWebClient.Models
{
    public class UserProfile
    {
        public string username;
        public string firstName;
        public string lastName;

        public UserProfile(string username, string firstName, string lastName)
        {
            this.username = username;
            this.firstName = firstName;
            this.lastName = lastName;
        }
    }
}
