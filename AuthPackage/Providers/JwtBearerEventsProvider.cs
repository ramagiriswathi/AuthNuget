using AuthPackage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;



namespace Customer.JwtBearerAuthentication.Lib.Providers;



public class JwtBearerEventsProvider

{

    private readonly ILogger<JwtBearerEventsProvider> _logger;

    private readonly IPublicKeyService _publicKeyService;

    private readonly FdcAuthOptions _fdcOptions;



    public JwtBearerEventsProvider(ILogger<JwtBearerEventsProvider> logger,

                   IPublicKeyService publicKeyService,

                   IOptions<FdcAuthOptions> fdcOptions)

    {

        _logger = logger;

        _publicKeyService = publicKeyService;

        _fdcOptions = fdcOptions.Value;

    }



    public JwtBearerEvents GetJwtBearerEvents()

    {

        return new JwtBearerEvents

        {

            OnMessageReceived = async context =>

            {

                try

                {

                    if (context.Request.Headers.ContainsKey("Authorization"))

                    {

                        var authHeader = context.Request.Headers["Authorization"].ToString();



                        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))

                        {

                            var token = authHeader.Substring("Bearer ".Length).Trim();



                            if (!string.IsNullOrEmpty(token))

                            {

                                context.Token = token;

                            }

                            else

                            {

                                _logger.LogWarning("Invalid Bearer token format: Token is null or empty.");

                                context.NoResult();

                                return;

                            }

                        }

                        else

                        {

                            _logger.LogWarning("Invalid Authorization header format: Missing 'Bearer ' prefix.");

                            context.NoResult();

                            return;

                        }

                    }

                    else

                    {

                        _logger.LogWarning("Authorization header is missing.");

                        context.NoResult();

                        return;

                    }



                    var signingKeys = await _publicKeyService.GetSigningKeysFromJwkAsync(_fdcOptions.SigningKeysUri);

                    context.Options.TokenValidationParameters.IssuerSigningKeys = signingKeys;

                }

                catch (Exception ex)

                {

                    _logger.LogError(ex,

                      "An error occurred while fetching signing keys. Details: {ErrorMessage}, {ExceptionType}",

                      ex.Message, ex.GetType().Name);

                }

            },

            OnTokenValidated = context =>

            {

                try

                {

                    var idToken = context.Request.Headers["X-Id-Token"].FirstOrDefault();

                    if (string.IsNullOrEmpty(idToken))

                    {

                        _logger.LogInformation("X-Id-Token header is missing.");

                        return Task.CompletedTask;

                    }



                    var tokenHandler = new JwtSecurityTokenHandler();

                    var validationParameters = context.Options.TokenValidationParameters;



                    tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);



                    var idTokenClaims = ((JwtSecurityToken)validatedToken).Claims;

                    var existingIdentity = context.Principal?.Identity as ClaimsIdentity;

                    if (existingIdentity == null)

                    {

                        _logger.LogInformation("Existing identity is null. Cannot add claims from ID token.");

                        return Task.CompletedTask;

                    }



                    var existingClaims = existingIdentity.Claims.Select(c => (c.Type, c.Value)).ToHashSet();



                    existingIdentity.AddClaims(idTokenClaims.Where(c => !existingClaims.Contains((c.Type, c.Value))));

                }

                catch (Exception ex)

                {

                    _logger.LogError(ex, "ID token validation error. Details: {ErrorMessage}, {ExceptionType}", ex.Message, ex.GetType().Name);

                }



                return Task.CompletedTask;

            },

            OnAuthenticationFailed = context =>

            {

                _logger.LogError(context.Exception, "Authentication failed. Details: {ErrorMessage}, {ExceptionType}", context.Exception.Message, context.Exception.GetType().Name);



                if (context.Exception is SecurityTokenExpiredException)

                {

                    context.Response.Headers.Add("Token-Expired", "true");

                }



                return Task.CompletedTask;

            }

        };

    }

}