using Xunit;
using RestSharp;

namespace HealthGateway.WebClient.Test.Functional.Api
{
    public class Configuration_Test
    {
        private string baseUrl = "https://dev-immsservice-gateway.pathfinder.gov.bc.ca/";

        [Fact]
        public void NonExistentApiTest()
        {
            // arrange
            RestClient client = new RestClient(baseUrl);
            RestRequest request = new RestRequest("v1/api/Imms/InvalidAPI", Method.GET);

            // act
            IRestResponse response = client.Execute(request);

            // assert
            Assert.False(response.IsSuccessful);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void UnauthorizedAccessTest()
        {
            // arrange
            RestClient client = new RestClient(baseUrl);
            RestRequest request = new RestRequest("/v1/api/Imms/items", Method.GET);

            // act
            IRestResponse response = client.Execute(request);

            // assert
            Assert.False(response.IsSuccessful);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
