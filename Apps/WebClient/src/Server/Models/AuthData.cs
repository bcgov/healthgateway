namespace HealthGateway.WebClient.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication Data Model.
    /// </summary>
    public class AuthData
    {
        /// <summary>
        /// Gets or sets a value indicating whether is the request Authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the request token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the authenticated user.
        /// </summary>
        public User User { get; set; }
    }
}
