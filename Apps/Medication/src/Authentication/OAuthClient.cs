
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Web;

namespace HealthGateway.MedicationService.Controllers
{
    public class OAuthClient 
    {
        static async Task<AccessToken> GetToken()
        {
            string clientId = "medication-service";
            string clientSecret = "";
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeader.Accept.Clear();

            }
        }
    }
}