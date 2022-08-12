using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Consumer;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace tests
{
    public class ConsumerWireMockPactTests
    {
        String
            provider =
                Environment.GetEnvironmentVariable("PACT_PROVIDER") != null
                    ? Environment.GetEnvironmentVariable("PACT_PROVIDER")
                    : "pactflow-example-bi-directional-provider-dotnet";

        String
            consumer =
                "pactflow-example-bi-directional-consumer-wiremock-dotnet";

        [Fact]
        public async Task GetProducts_WhenCalled_ReturnsAllProducts()
        {
            // Arrange
            var server = WireMockServer.Start();
            String serverUrl = server.Urls[0] + "/";
            server
                .WithConsumer(consumer)
                .WithProvider(provider)
                .Given(Request.Create().UsingGet().WithPath("/Products"))
                .WithTitle("a request to retrieve all products")
                .RespondWith(Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type",
                    "application/json; charset=utf-8")
                    .WithBodyAsJson(new []
                    { new { id = 27, name = "burger", type = "food" } }));

            // Act
            var client = new ProductClient();
            var products = await client.GetProducts(serverUrl, null);

            //Assert
            Assert.IsType<int>(products[0].id);
            Assert.IsType<string>(products[0].name);
            Assert.IsType<string>(products[0].type);

            // Save mapping to folder
            server
                .SaveStaticMappings(Path
                    .Combine("..", "..", "..", "wiremock-mappings"));

            // Save pact
            server
                .SavePact(Path.Combine("..", "..", "..", "pacts"),
                "get-products.json");
        }

        [Fact]
        public async Task GetProduct_WhenCalledWithExistingId_ReturnsProduct()
        {
            // Arrange
            var server = WireMockServer.Start();
            String serverUrl = server.Urls[0] + "/";
            server
                .WithConsumer(consumer)
                .WithProvider(provider)
                .Given(Request.Create().UsingGet().WithPath("/Products/27"))
                .WithTitle("a request to retrieve a product with existing id")
                .RespondWith(Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type",
                    "application/json; charset=utf-8")
                    .WithBodyAsJson(new {
                        id = 27,
                        name = "burger",
                        type = "food"
                    }));

            // Act
            var client = new ProductClient();
            var products = await client.GetProduct(serverUrl, 27, null);

            //Assert
            Assert.IsType<int>(products.id);
            Assert.IsType<string>(products.name);
            Assert.IsType<string>(products.type);

            // Save mapping to folder
            server
                .SaveStaticMappings(Path
                    .Combine("..", "..", "..", "wiremock-mappings"));

            // Save pact
            server
                .SavePact(Path.Combine("..", "..", "..", "pacts"),
                "get-product-by-id.json");
        }

        [Fact]
        public async Task GetProduct_WhenCalledWithInvalidID_ReturnsError()
        {
            // Arrange
            var server = WireMockServer.Start();
            String serverUrl = server.Urls[0] + "/";
            server
                .WithConsumer(consumer)
                .WithProvider(provider)
                .Given(Request.Create().UsingGet().WithPath("/Products/10"))
                .WithTitle("a request to retrieve a product id that does not exist")
                .RespondWith(Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.NotFound)
                    .WithHeader("Content-Type",
                    "application/json; charset=utf-8"));

            // Act
            var client = new ProductClient();
            var ex =
                await Assert
                    .ThrowsAsync<HttpRequestException>(() =>
                        client.GetProduct(serverUrl, 10, null));

            // Assert
            Assert
                .Equal("Response status code does not indicate success: 404 (Not Found).",
                ex.Message);

            server
                .SaveStaticMappings(Path
                    .Combine("..", "..", "..", "wiremock-mappings"));

            // Save pact
            server
                .SavePact(Path.Combine("..", "..", "..", "pacts"),
                "get-product-by-id-not-exist.json");
        }
    }
}
