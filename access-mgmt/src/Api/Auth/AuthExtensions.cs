using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Api.Auth
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddStaticBearerAuthentication(this IServiceCollection services)
        {
            services.AddOptions<StaticBearerOptions>()
                    .Configure<IServiceProvider>((opts, sp) =>
                    {
                        // bound via configuration in Program.cs
                    });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = StaticBearerAuthenticationHandler.Scheme;
                options.DefaultChallengeScheme = StaticBearerAuthenticationHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, StaticBearerAuthenticationHandler>(
                StaticBearerAuthenticationHandler.Scheme, _ => { });

            return services;
        }
    }
}
