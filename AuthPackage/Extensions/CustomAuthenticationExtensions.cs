using Customer.JwtBearerAuthentication.Lib.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuthPackage.Extensions
{
    public static class CustomAuthenticationExtensions
    {
        public static AuthenticationBuilder AddFdcJwtBearer(this AuthenticationBuilder builder, Action<FdcAuthOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            builder.Services.AddHttpClient();

            builder.Services.AddMemoryCache();

            builder.Services.TryAddSingleton<IPublicKeyService, PublicKeyService>();

            builder.Services.AddTransient<JwtBearerEventsProvider>();



            return builder.Services.AddAuthentication(options =>

            {

                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(opt =>

            {

                var eventsProvider = builder.Services.BuildServiceProvider().GetRequiredService<JwtBearerEventsProvider>();

                opt.Events = eventsProvider.GetJwtBearerEvents();

            });

        }

    }
}
