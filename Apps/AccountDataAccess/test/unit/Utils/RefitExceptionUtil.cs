namespace AccountDataAccessTest.Utils
{
    using System.Net;
    using Refit;

    /// <summary>
    /// Exception utilities class to assist in mocking Refit exceptions.
    /// </summary>
    public static class RefitExceptionUtil
    {
        /// <summary>
        /// Creates a Refit exception with the given status code and reason phrase.
        /// </summary>
        /// <param name="statusCode">The desired exception status code.</param>
        /// <returns>Refit ApiException</returns>
        public static async Task<ApiException> CreateApiException(HttpStatusCode statusCode)
        {
            RefitSettings rfSettings = new();
            using HttpResponseMessage response = new(statusCode);
            return await ApiException.Create(
                null!,
                null!,
                response,
                rfSettings);
        }
    }
}
