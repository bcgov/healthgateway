namespace HealthGateway.CommonTests.Utils
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Mock Method Info.
    /// </summary>
    [AuthorizeAttribute]
    public class MockMethodInfo
    {
        /// <summary>
        /// Mock Method.
        /// </summary>
        public void MockMethod()
        {
        }

        /// <summary>
        /// Authorized Method.
        /// </summary>
        [Authorize]
        public void AuthorizedMethod()
        {
        }
    }
}
