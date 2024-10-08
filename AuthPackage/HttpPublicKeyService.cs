using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace AuthPackage
{
    public class PublicKeyService : IPublicKeyService
    {

        private readonly HttpClient _httpClient;

        private readonly IMemoryCache _cache;

        private readonly string _cacheKey = "PublicKeyCache";

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public PublicKeyService(IHttpClientFactory httpClientFactory, IMemoryCache cache)

        {

            _httpClient = httpClientFactory.CreateClient();

            _cache = cache;

        }



        public async Task<IEnumerable<SecurityKey>> GetSigningKeysFromJwkAsync(Uri bffUrl)

        {

            await _semaphore.WaitAsync();

            try

            {

                if (!_cache.TryGetValue(_cacheKey, out IEnumerable<SecurityKey> cachedKeys))

                {

                    HttpResponseMessage response = await _httpClient.GetAsync(bffUrl);

                    response.EnsureSuccessStatusCode();

                    var jwksJson = await response.Content.ReadAsStringAsync();

                    var jwks = new JsonWebKeySet(jwksJson);

                    cachedKeys = jwks.GetSigningKeys();



                    var cacheEntryOptions = new MemoryCacheEntryOptions()

                      .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                    _cache.Set(_cacheKey, cachedKeys, cacheEntryOptions);

                }



                return cachedKeys;

            }

            finally

            {

                _semaphore.Release();

            }

        }



        public async Task<Dictionary<string, IEnumerable<SecurityKey>>> GetSigningKeysFromJwkAsyncV2(Uri bffUrl)

        {

            await _semaphore.WaitAsync();

            try

            {

                if (!_cache.TryGetValue(_cacheKey, out Dictionary<string, IEnumerable<SecurityKey>> cachedKeys))

                {

                    HttpResponseMessage response = await _httpClient.GetAsync(bffUrl);

                    response.EnsureSuccessStatusCode();

                    var jwksJson = await response.Content.ReadAsStringAsync();



                    cachedKeys = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<SecurityKey>>>(jwksJson);



                    var cacheEntryOptions = new MemoryCacheEntryOptions()

                      .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                    _cache.Set(_cacheKey, cachedKeys, cacheEntryOptions);

                }



                return cachedKeys;

            }

            finally

            {

                _semaphore.Release();

            }

        }

    }
}
