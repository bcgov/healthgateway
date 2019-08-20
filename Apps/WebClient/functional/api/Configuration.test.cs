using Xunit;
using System;
using RestSharp;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace HealthGateway.WebClient.Test.Functional.Api
{
    public class Configuration_Test
    {
        private string baseUrl = "https://dev-gateway.pathfinder.gov.bc.ca";

        public Configuration_Test()
        {
            var config = GetIConfigurationRoot();
            var clientId = config["TestRun"];
            Console.WriteLine(config.GetSection("TestRun").ToString());
            Console.WriteLine("-------");
            Console.WriteLine(clientId);
            Console.WriteLine("-------");
        }

        [Fact]
        public void RetrieveConfigurationTest()
        {
            // arrange
            RestClient client = new RestClient(baseUrl);
            RestRequest request = new RestRequest("api/Configuration", Method.GET);

            // act
            IRestResponse response = client.Execute(request);
            JObject json = JObject.Parse(response.Content);

            // assert
            Assert.Equal(4, json.Count);
            Assert.NotNull(json["openIdConnect"]);

            // Verify that the authentication authority is a valid URL
            Uri uriResult;
            bool result = Uri.TryCreate(json["openIdConnect"]["authority"].ToString(), UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp;

            Assert.True(result);
            Assert.NotNull(json["identityProviders"]);
            Assert.NotNull(json["webClient"]);
            Assert.NotNull(json["serviceEndpoints"]);
        }

        public class TestRunConfiguration
        {
            public Dictionary<string, string> URLs;
        }

        public static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static TestRunConfiguration GetApplicationConfiguration()
        {
            var configuration = new TestRunConfiguration();

            var iConfig = GetIConfigurationRoot();

            iConfig
                .GetSection("TestRun")
                .Bind(configuration);

            return configuration;
        }
    }
}
