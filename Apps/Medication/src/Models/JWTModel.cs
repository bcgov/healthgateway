using System.Security.Claims;

namespace HealthGateway.MedicationService.Models
{
    public class JWTModel : IAuthModel
    {
        #region Members
        public int expiresInMinutes { get; set; }

        public int refreshExpiresInMinutes { get; set; }

        public string refreshToken { get; set; }

        public string accessToken { get; set; }

        public string tokenType { get; set; }

        public string sessionState { get; set; }

        #endregion
    }
}