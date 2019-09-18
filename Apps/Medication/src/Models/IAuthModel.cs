
using System.Security.Claims;

namespace HealthGateway.MedicationService.Models
{

    public interface IAuthModel
    {
        #region Members
        int expiresInMinutes { get; set; }

        int refreshExpiresInMinutes { get; set; }

        string refreshToken { get; set; }

        string accessToken { get; set; }

        string tokenType { get; set; }

        string sessionState { get; set; }

        #endregion
    }
}