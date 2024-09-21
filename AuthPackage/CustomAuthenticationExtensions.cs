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

namespace AuthPackage
{
    public static class CustomAuthenticationExtensions
    {
        public static AuthenticationBuilder AddFdcAuthentication(this AuthenticationBuilder builder, 
            Action<JwtBearerOptions> configureOptions, 
            string bffUrl,
            IOptionsMonitor<JwtBearerOptions> jwtBearerOptionsMonitorinput)
        {
            // Register the IPublicKeyService as a singleton
            builder.Services.AddSingleton<IPublicKeyService, HttpPublicKeyService>();

            // Add the custom JwtBearerHandler scheme
            builder.AddScheme<JwtBearerOptions, CustomJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                configureOptions(options);

                // Configure the JwtBearerOptions and get the IOptionsMonitor
                var jwtBearerOptionsMonitor = builder.Services
                    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                    .Configure(configureOptions)
                    .PostConfigure<IHttpClientFactory>((jwtOptions, httpClientFactory) =>
                    {
                        // Use TokenHandlers instead of SecurityTokenValidators
                        options.TokenHandlers.Clear();
                        options.TokenHandlers.Add(new CustomJwtBearerHandler(
                            jwtBearerOptionsMonitorinput,
                            builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>(),
                            UrlEncoder.Default,
                            httpClientFactory.CreateClient(),
                            bffUrl,
                            builder.Services.BuildServiceProvider().GetRequiredService<IPublicKeyService>()
                        ));
                    });
            });

            return builder;
        }
    }
}
