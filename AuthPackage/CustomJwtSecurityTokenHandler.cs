
/*

namespace AuthPackage
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;

    public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        private readonly
     IPublicKeyService _publicKeyService;

        public CustomJwtSecurityTokenHandler(IPublicKeyService publicKeyService)
        {
            _publicKeyService = publicKeyService;
        }

        public override ClaimsPrincipal ValidateToken(string tokenIdentifier, TokenValidationParameters validationParameters, out SecurityToken validatedToken)

        {
            // Fetch the JWK set from your public key service
            var jwks = _publicKeyService.GetSigningKeysFromJwkAsync(tokenIdentifier);

            // Configure validation parameters with the fetched JWKs
            validationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
            {
                // Find the matching JWK from the set
                var jwk = jwks.Keys.FirstOrDefault(key => key.Kid == kid);

                if (jwk == null)
                    return null;

                // Convert the JWK to a SecurityKey
                return new JsonWebKey(jwk.ToString());
            };

            // Use the base class to validate the token
            return base.ValidateToken(token, validationParameters, out validatedToken);
        }
    }
}
*/