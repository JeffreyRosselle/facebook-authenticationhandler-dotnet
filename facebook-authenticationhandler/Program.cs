using FacebookAuthenticationhandler.Facebook;
using Microsoft.AspNetCore.Authentication.Facebook;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(FacebookDefaults.AuthenticationScheme)
    .AddScheme<FacebookOptions, FacebookAuthHanlder>(FacebookDefaults.AuthenticationScheme, options =>
    {
        options.AppId = "[APPID]";
        options.AppSecret = "[APPSECRET]";
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpClient(nameof(FacebookClient), (provider, client) =>
{
    client.BaseAddress = new Uri("https://graph.facebook.com/v13.0");
})
    .AddTypedClient<IFacebookClient, FacebookClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


