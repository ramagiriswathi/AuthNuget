//using Customer.JwtBearerAuthentication.Lib.Services;

//using Customer.JwtBearerAuthentication.Lib.Tests.Services;

//using Microsoft.Extensions.Caching.Memory;

//using Microsoft.IdentityModel.Tokens;

using AuthPackage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;

using Moq.Protected;

using System.Net;

using System.Text.Json;

namespace AuthTests
{
    public class PublicKeyServiceTests

    {

        [Fact]

        public async Task GetSigningKeysFromJwkAsync_CacheMiss_FetchesFromBff()

        {

            // Arrange

            var memoryCache = MemoryCache(out var mockMemoryCache);



            var httpClient = HttpClient();

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory

              => factory.CreateClient("fdc-client")).Returns(httpClient);



            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, memoryCache);

            var bffUrl = new Uri("https://dummy-bff/jwks");



            // Act

            var keys = await publicKeyService.GetSigningKeysFromJwkAsync(bffUrl);



            // Assert

            Assert.NotNull(keys);

            Assert.Equal(2, keys.Count());



            mockMemoryCache.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Once);

            mockMemoryCache.Verify(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny), Times.Exactly(2));

        }



        [Fact]

        public async Task GetSigningKeysFromJwkAsync_EmptyJwks_ReturnsEmptyDictionary()

        {

            // Arrange

            var memoryCache = MemoryCache(out var mockMemoryCache);



            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();



            mockHttpMessageHandler.Protected()

              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())



              .ReturnsAsync(new HttpResponseMessage()

              {

                  StatusCode = HttpStatusCode.OK,

                  Content = new StringContent(@"{}")

              });





            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory

              => factory.CreateClient("fdc-client")).Returns(httpClient);



            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, memoryCache);

            var bffUrl = new Uri("https://your-bff/jwks");



            // Act

            var keys = await publicKeyService.GetSigningKeysFromJwkAsync(bffUrl);



            // Assert

            Assert.NotNull(keys);

            Assert.Empty(keys);



            mockMemoryCache.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Once);

            mockMemoryCache.Verify(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny), Times.Exactly(2));

        }



        [Fact]

        public async Task GetSigningKeysFromJwkAsync_CacheHit_ReturnsCachedKeys()

        {

            // Arrange

            var expectedKeys = new Dictionary<string, IEnumerable<SecurityKey>>

    {

      { "issuer1", new List<SecurityKey>() },

      { "issuer2", new List<SecurityKey>() }

    };



            object? expected = expectedKeys;



            var memoryCache = MockMemoryCacheService.GetMemoryCache(expected);



            var mockHttpClientFactory = new Mock<IHttpClientFactory>();



            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, memoryCache);

            var bffUrl = new Uri("https://dummy-bff/jwks");



            // Act

            var keys = await publicKeyService.GetSigningKeysFromJwkAsync(bffUrl);



            // Assert

            Assert.Same(expectedKeys, keys);

        }





        [Fact]

        public async Task GetSigningKeysFromJwkAsync_HandlesJsonSerializationException_AndThrows()

        {

            // Arrange

            var mockMemoryCache = new Mock<IMemoryCache>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();



            // Invalid Json

            var jwksData = @"

            {

                ""issuer1"": ""{ \""keys\"": [{ \""kid\"": \""key1\"", \""kty\"": \""RSA\"", \""use\"": \""sig\"", \""n\"": \""some-modulus-value\"", \""e\"": \""some-exponent-value\"" }] }"",

                ""issuer2"": ""{ \""keys\"": [{ \""kid\"": \""key2\"", \""kty\"": \""RSA\"", \""use\"": \""sig\"", \""n\"": \""another-modulus-value\"", \""e\"": \""another-exponent-value\"" }] }""

            ";

            mockHttpMessageHandler.Protected()

              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())



              .ReturnsAsync(new HttpResponseMessage()

              {

                  StatusCode = HttpStatusCode.OK,

                  Content = new StringContent("invalid-json")

              });



            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory

              => factory.CreateClient("fdc-client")).Returns(httpClient);



            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, mockMemoryCache.Object);

            var bffUrl = new Uri("https://dummy-bff/jwks");



            // Act & Assert

            await Assert.ThrowsAsync<JsonException>(() => publicKeyService.GetSigningKeysFromJwkAsync(bffUrl));

        }





        [Fact]

        public async Task GetSigningKeysFromJwkAsync_HandlesException_AndThrows()

        {

            // Arrange

            var mockMemoryCache = new Mock<IMemoryCache>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()

              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())

              .ThrowsAsync(new

                HttpRequestException("Network error"));



            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory

              => factory.CreateClient("fdc-client")).Returns(httpClient);



            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, mockMemoryCache.Object);

            var bffUrl = new Uri("https://dummy-bff/jwks");



            // Act & Assert

            await Assert.ThrowsAsync<HttpRequestException>(() => publicKeyService.GetSigningKeysFromJwkAsync(bffUrl));

        }





        [Fact]

        public async Task GetSigningKeysFromJwkAsync_ConcurrentRequests_OnlyOneFetchAndOthersUseCache()

        {

            // Arrange

            var httpClient = HttpClient();

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory

              => factory.CreateClient("fdc-client")).Returns(httpClient);

            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var publicKeyService = new PublicKeyService(mockHttpClientFactory.Object, memoryCache);

            var bffUrl = new Uri("https://dummy-bff/jwks");





            // Act

            var tasks = new List<Task<Dictionary<string, IEnumerable<SecurityKey>>>>();

            for (int i = 0; i < 10; i++) // Simulate 10 concurrent requests

            {

                tasks.Add(publicKeyService.GetSigningKeysFromJwkAsync(bffUrl));

            }

            await Task.WhenAll(tasks);



            // Assert



            var firstResult = tasks[0].Result;

            foreach (var task in tasks)

            {

                Assert.Equal(firstResult, task.Result);

            }

        }



        private static HttpClient HttpClient()

        {

            var jwksData = @"

            {

                ""issuer1"": ""{ \""keys\"": [{ \""kid\"": \""key1\"", \""kty\"": \""RSA\"", \""use\"": \""sig\"", \""n\"": \""some-modulus-value\"", \""e\"": \""some-exponent-value\"" }] }"",

                ""issuer2"": ""{ \""keys\"": [{ \""kid\"": \""key2\"", \""kty\"": \""RSA\"", \""use\"": \""sig\"", \""n\"": \""another-modulus-value\"", \""e\"": \""another-exponent-value\"" }] }""

            }";



            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()

              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())

              .ReturnsAsync(new HttpResponseMessage

              {

                  StatusCode = HttpStatusCode.OK,

                  Content = new StringContent(jwksData)

              });



            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            return httpClient;

        }



        private static IMemoryCache MemoryCache(out Mock<IMemoryCache> mockMemoryCache)

        {

            object? expectedCacheValue = null;



            var memoryCache = MockMemoryCacheService.GetMemoryCache(expectedCacheValue);



            var cachEntry = Mock.Of<ICacheEntry>();



            mockMemoryCache = Mock.Get(memoryCache);

            mockMemoryCache

              .Setup(m => m.CreateEntry(It.IsAny<object>()))

              .Returns(cachEntry);

            return memoryCache;

        }

    }
}
