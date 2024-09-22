using AuthPackage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

// Correct one
public class CustomJwtBearerHandler : JwtBearerHandler
{
    private readonly string _bffUrl;
    private readonly IPublicKeyService _publicKeyService;
    private readonly FdcAuthOptions _fdcOptions;

    public CustomJwtBearerHandler(IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, HttpClient httpClient, IOptions<FdcAuthOptions> fdcAuthOptions,
        IPublicKeyService publicKeyService)
        : base(options, logger, encoder)
    {
      //  _bffUrl = bffUrl ?? throw new ArgumentNullException(nameof(bffUrl));
        _publicKeyService = publicKeyService;
    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = GetTokenFromRequestHeader();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }
        try
        {
            // Fetch the JWS key set from the public key service
            var signingKeys = await _publicKeyService.GetSigningKeysFromJwkAsync(_fdcOptions.SigningKeysUri);

            // Configure token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,

                // Set the fetched signing keys
                ValidIssuer = Options.Authority,
                ValidAudience = Options.Audience
            };

            // Validate the token
            //var tokenHandler = new JwtSecurityTokenHandler();
            //ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            //return AuthenticateResult.Success(new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme));


            // Update JwtBearerOptions with the new parameters
            Options.TokenValidationParameters = validationParameters;

            // Delegate the actual token validation to the base JwtBearerHandler
            return await base.HandleAuthenticateAsync();
        }
        catch (SecurityTokenException ex)
        {
            Logger.LogError($"Token validation failed: {ex.Message}");
            return AuthenticateResult.Fail("Invalid token.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Authentication error: {ex.Message}");
            return AuthenticateResult.Fail("Invalid token.");
        }
    }

    private string? GetTokenFromRequestHeader()
    {
        if (Request.Headers.ContainsKey("Authorization"))
        {
            string authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        return null;
    }
}
