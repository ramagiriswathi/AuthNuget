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



        public async Task<Dictionary<string, IEnumerable<SecurityKey>>?> GetSigningKeysFromJwkAsync(Uri bffUrl)

        {

            if (_cache.TryGetValue(_cacheKey, out Dictionary<string, IEnumerable<SecurityKey>>? cachedKeys))

            {

                return cachedKeys;

            }



            await _semaphore.WaitAsync();



            try

            {

                if (_cache.TryGetValue(_cacheKey, out cachedKeys))

                {

                    return cachedKeys;

                }



                HttpResponseMessage response = await _httpClient.GetAsync(bffUrl);

                response.EnsureSuccessStatusCode();

                var jwksJson = await response.Content.ReadAsStringAsync();



                var issuerKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(jwksJson);



                var issuerSecurityKeySet = new Dictionary<string, IEnumerable<SecurityKey>>();

                if (issuerKeys != null)

                {

                    foreach (var ik in issuerKeys)

                    {

                        var jwks = new JsonWebKeySet(ik.Value);

                        issuerSecurityKeySet.Add(ik.Key, jwks.GetSigningKeys());

                    }

                }



                var cacheEntryOptions = new MemoryCacheEntryOptions()

                  .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(_cacheKey, issuerSecurityKeySet, cacheEntryOptions);



                return issuerSecurityKeySet;

            }

            finally

            {

                _semaphore.Release();

            }

        }

    }
}
