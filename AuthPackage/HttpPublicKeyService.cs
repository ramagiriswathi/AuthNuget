using Microsoft.IdentityModel.Tokens;

namespace AuthPackage
{
    public class HttpPublicKeyService : IPublicKeyService
    {
        private readonly HttpClient _httpClient;

        public HttpPublicKeyService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<SecurityKey>> GetSigningKeysFromJwkAsync(string bffUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(bffUrl);
            response.EnsureSuccessStatusCode();
            var jwksJson = await response.Content.ReadAsStringAsync();
            var jwks = new JsonWebKeySet(jwksJson);
            return jwks.GetSigningKeys();
        }

        //public async Task<JsonWebKeySet> GetJwks()
        //{
        //    var response = await _httpClient.GetAsync(_jwksUri);
        //    response.EnsureSuccessStatusCode();

        //    var jwksJson = await response.Content.ReadAsStringAsync();
        //    return JsonWebKeySet.Create(jwksJson);
        //}
    }
}
