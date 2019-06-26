using System.Net.Http.Headers;

namespace HealthGateway.Engine.Extensions
{
    public static class ETag
    {
        public static EntityTagHeaderValue Create(string value)
        {
            string tag = "\"" + value + "\"";
            return new EntityTagHeaderValue(tag, true);
        }
    }
}
