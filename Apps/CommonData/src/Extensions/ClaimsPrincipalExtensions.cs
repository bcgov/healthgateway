namespace HealthGateway.Common.Data.Extensions
{
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns a value that indicates whether the entity (user) represented by this claims principal is in any of the
        /// specified roles.
        /// </summary>
        /// <param name="user">The claims principal in question.</param>
        /// <param name="roles">The roles for which to check.</param>
        /// <returns>true if claims principal is in any of the specified roles; otherwise, false.</returns>
        public static bool IsInAnyRole(this ClaimsPrincipal? user, params string[] roles)
        {
            return user != null && roles.Any(user.IsInRole);
        }
    }
}
