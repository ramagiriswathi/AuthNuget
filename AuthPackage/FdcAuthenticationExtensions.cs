
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;

namespace AuthPackage
{
    public static class FdcAuthenticationExtensions
    {
        public static AuthenticationBuilder AddFdcAuth(
        this AuthenticationBuilder builder,
        string bffUrl,
        Action<AuthenticationSchemeOptions> configureOptions = null)
        {
            // Default scheme name if none is provided
            const string defaultScheme = "FdcAuth";

            // Register the custom authentication scheme and handler
            builder.AddJwtBearer(defaultScheme, configureOptions);


            // Register the MyAuthenticationHandler as a scoped service, passing the BFF URL
            builder.Services.AddScoped<MyAuthenticationHandler>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var encoder = sp.GetRequiredService<UrlEncoder>();
                var clock = sp.GetRequiredService<ISystemClock>();
                var configurationManager = sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>();

                //return new MyAuthenticationHandler(
                //    sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>(),
                //    loggerFactory,
                //    encoder,
                //    clock,
                //    configurationManager,
                //    bffUrl, // Pass the BFF URL here
                //    sp.GetRequiredService<IHttpClientFactory>(),
                //    sp.GetRequiredService<IMemoryCache>());

                var handler = new MyAuthenticationHandler(
                sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>(),
                loggerFactory,
                encoder,
                clock,
                configurationManager,
                bffUrl,
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<IMemoryCache>());

                // Apply configureOptions to the handler's Options
                configureOptions?.Invoke(handler.Options);

                return handler;
            });

            return builder;
        }
    }
}
