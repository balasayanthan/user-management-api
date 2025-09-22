using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests
{
    public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public ApiSmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Swagger_available()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/swagger/v1/swagger.json");
            resp.EnsureSuccessStatusCode();
        }
    }
}
