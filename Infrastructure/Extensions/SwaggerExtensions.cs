using HackerNews.Infrastructure.Settings;
using Microsoft.OpenApi;

namespace HackerNews.Infrastructure.DependencyInjection
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerSettings(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersioningSettings.Current, new OpenApiInfo
                {
                    Title = "Hacker News API - Santander Test",
                    Version = ApiVersioningSettings.Current
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(
                        $"/swagger/{ApiVersioningSettings.Current}/swagger.json",
                        $"Hacker News {ApiVersioningSettings.Current}"
                    );

                    c.RoutePrefix = "swagger";
                });
            }

            return app;
        }
    }
}
