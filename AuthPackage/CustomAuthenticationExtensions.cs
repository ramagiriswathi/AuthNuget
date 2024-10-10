using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthPackage
{
    public static class CustomAuthenticationExtensions
    {
        public static AuthenticationBuilder AddFdcJwtBearer(this AuthenticationBuilder builder, Action<FdcAuthOptions> configureOptions)

        {

            builder.Services.Configure(configureOptions);

            builder.Services.AddHttpClient();

            builder.Services.AddMemoryCache();

            builder.Services.TryAddSingleton<IPublicKeyService, PublicKeyService>();



            return builder.Services.AddAuthentication(options =>

            {

                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(opt =>

            {

                opt.Events = new JwtBearerEvents()

                {

                    OnMessageReceived = async context =>

                    {

                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();

                        var logger = loggerFactory.CreateLogger(nameof(CustomAuthenticationExtensions));



                        try

                        {

                            if (context.Request.Headers.TryGetValue("Authorization", out var value))

                            {

                                var authHeader = value.ToString();



                                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))

                                {

                                    var token = authHeader.Substring("Bearer ".Length).Trim();



                                    if (!string.IsNullOrEmpty(token))

                                    {

                                        context.Token = token;

                                    }

                                    else

                                    {

                                        logger.LogWarning("Invalid Bearer token format: Token is null or empty.");

                                        context.NoResult();

                                        return;

                                    }

                                }

                                else

                                {

                                    logger.LogWarning("Invalid Authorization header format: Missing 'Bearer ' prefix.");

                                    context.NoResult();

                                    return;

                                }

                            }

                            else

                            {

                                logger.LogWarning("Authorization header is missing.");

                                context.NoResult();

                                return;

                            }



                            var publicKeyService =

                              context.HttpContext.RequestServices.GetRequiredService<IPublicKeyService>();

                            var fdcOptions = context.HttpContext.RequestServices

                              .GetRequiredService<IOptions<FdcAuthOptions>>();



                            var signingKeys = await publicKeyService.GetSigningKeysFromJwkAsync(fdcOptions.Value.SigningKeysUri);

                            var accessToken = context.Token;

                            var handler = new JwtSecurityTokenHandler();

                            var jwtToken = handler.ReadJwtToken(accessToken);

                            var issuer = jwtToken.Issuer;



                            if (signingKeys != null && signingKeys.TryGetValue(issuer, out var issuerSigningKeys))

                            {

                                context.Options.TokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;

                            }

                        }

                        catch (Exception ex)

                        {

                            logger.LogError(ex,

                              "An error occurred while fetching signing keys. Details: {ErrorMessage}, {ExceptionType}",

                              ex.Message, ex.GetType().Name);

                        }

                    },

                    OnTokenValidated = context =>

                    {

                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();

                        var logger = loggerFactory.CreateLogger(nameof(CustomAuthenticationExtensions));

                        try

                        {

                            var idToken = context.Request.Headers["X-Id-Token"].FirstOrDefault();

                            if (string.IsNullOrEmpty(idToken))

                            {

                                logger.LogInformation("X-Id-Token header is missing.");

                                return Task.CompletedTask;

                            }



                            var tokenHandler = new JwtSecurityTokenHandler();

                            var validationParameters = context.Options.TokenValidationParameters;



                            tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);



                            var existingIdentity = context.Principal?.Identity as ClaimsIdentity;

                            if (existingIdentity == null)

                            {

                                logger.LogInformation("Existing identity is null. Cannot add claims from ID token.");

                                return Task.CompletedTask;

                            }



                            var existingClaims = existingIdentity.Claims.Select(c => (c.Type, c.Value)).ToHashSet();



                            if (validatedToken is JwtSecurityToken jwtToken)

                            {

                                var idTokenClaims = jwtToken.Claims;

                                existingIdentity.AddClaims(

                                  idTokenClaims.Where(c => !existingClaims.Contains((c.Type, c.Value))));

                            }

                        }

                        catch (Exception ex)

                        {

                            logger.LogError(ex, "ID token validation error. Details: {ErrorMessage}, {ExceptionType}",

                              ex.Message, ex.GetType().Name);

                        }



                        return Task.CompletedTask;

                    },

                    OnAuthenticationFailed = context =>

                    {

                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();

                        var logger = loggerFactory.CreateLogger(nameof(CustomAuthenticationExtensions));



                        logger.LogError(context.Exception,

                          "Authentication failed. Details: {ErrorMessage}, {ExceptionType}", context.Exception.Message,

                          context.Exception.GetType().Name);



                        if (context.Exception is SecurityTokenExpiredException)

                        {

                            context.Response.Headers.Add("Token-Expired", "true");

                        }



                        return Task.CompletedTask;

                    }

                };

            });

        }
    }
}
