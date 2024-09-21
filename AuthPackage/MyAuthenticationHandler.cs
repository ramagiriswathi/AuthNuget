/*

using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Security.Cryptography;

namespace AuthPackage
{
    public class MyAuthenticationHandler :  JwtBearerHandler  //AuthenticationHandler<JwtBearerOptions>
    {
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly string _bffUrl;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        

        public MyAuthenticationHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
            string bffUrl,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache)
            : base(options, logger, encoder, clock)
        {
            _configurationManager = configurationManager;
            _bffUrl = bffUrl;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Fetch public keys from BFF (with caching)
            var publicKey = await GetPublicKeysAsync();

            // Configure token validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
               ValidateIssuerSigningKey = true,
                IssuerSigningKeys = publicKey,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            // Update JwtBearerOptions with the new parameters
            Options.TokenValidationParameters = tokenValidationParameters;

            // Delegate the actual token validation to the base JwtBearerHandler
            return await base.HandleAuthenticateAsync();
        }

        private async Task<IEnumerable<SecurityKey>> GetPublicKeysAsync()
        {
            // Try to get the keys from the cache first
            if (!_cache.TryGetValue("PublicKeys", out IEnumerable<SecurityKey> cachedKeys))
            {
                // If not in cache, fetch them directly from the BFF
                using var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetAsync(_bffUrl + "/signing-keys"); // Assuming your BFF endpoint is at /get-signing-keys
                response.EnsureSuccessStatusCode();

                var jwksJson = await response.Content.ReadAsStringAsync();

                // Deserialize the response and extract the keys (adjust deserialization as needed)
                var keys = JsonSerializer.Deserialize<IEnumerable<JsonWebKey>>(jwksJson); // Or use Newtonsoft.Json if that's what your BFF returns

                // Convert JsonWebKeys to SecurityKeys
                var signingKeys = keys.Select(BuildSecurityKeyFromJwk);

                // Cache the keys
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Cache for 30 minutes   

                };
                _cache.Set("PublicKeys", signingKeys, cacheEntryOptions);

                cachedKeys = signingKeys;
            }

            return cachedKeys;
        }


        private SecurityKey BuildSecurityKeyFromJwk(JsonWebKey jwk)
        {
            if (jwk.Kty == "RSA" && jwk.Use == "sig")
            {
                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                    Exponent = Base64UrlEncoder.DecodeBytes(jwk.E)
                });
                return new RsaSecurityKey(rsa);
            }

            // Handle other key types or 'use' cases if necessary in the future
            throw new NotSupportedException("Unsupported key type or usage");
        }
    }
}
*/