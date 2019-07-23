namespace HealthGateway.WebClient.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// User model.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the users email.
        /// </summary>
        public string Email { get; set; }
    }
}
