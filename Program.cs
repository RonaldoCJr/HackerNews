using HackerNews.Infrastructure.DependencyInjection;
using HackerNews.Infrastructure.Extensions;
using HackerNews.Infrastructure.Middleware;
using HackerNews.Infrastructure.Settings;


var builder = WebApplication.CreateBuilder(args);

var hnApiSettings = builder.Configuration.GetSection("HackerNewsApi").Get<HackerNewsApiSettings>() ?? throw new InvalidOperationException("HackerNewsApi settings are missing.");

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddCustomCacheSettings(builder.Configuration);
builder.Services.AddResiliencePolicies();
builder.Services.AddRefitSettings(builder.Configuration);
builder.Services.AddApiVersioningSettings();
builder.Services.AddSwaggerSettings();
builder.Services.AddApplicationSettings();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseSwaggerDocumentation(app.Environment);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();