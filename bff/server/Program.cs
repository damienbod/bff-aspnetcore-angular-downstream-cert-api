﻿using BffOpenIddict.Server;
using BffOpenIddict.Server.ApiClient;
using BffOpenIddict.Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

var services = builder.Services;
var configuration = builder.Configuration;

var stsServer = configuration["OpenIDConnectSettings:Authority"];

services.AddSecurityHeaderPolicies()
  .SetPolicySelector((PolicySelectorContext ctx) =>
  {
      return SecurityHeadersDefinitions.GetHeaderPolicyCollection(
          builder.Environment.IsDevelopment(), stsServer);
  });

services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "__Host-X-XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services.AddHttpClient();
services.AddOptions();

services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    configuration.GetSection("OpenIDConnectSettings").Bind(options);
    //options.Authority = configuration["OpenIDConnectSettings:Authority"];
    //options.ClientId = configuration["OpenIDConnectSettings:ClientId"];
    //options.ClientSecret = configuration["OpenIDConnectSettings:ClientSecret"];

    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name"
    };
});

services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services.AddRazorPages().AddMvcOptions(options =>
{
    //var policy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
    //options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddSingleton<ApiTokenCacheClient>();

// For OAuth2 JWT Bearer token validation
//builder.Services.AddReverseProxy()
//   .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
//   .AddTransforms<JwtTransformProvider>();

// Create an authorization policy used by YARP when forwarding requests
builder.Services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", builder =>
{
    builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    builder.RequireAuthenticatedUser();
}));

var cert = new X509Certificate2("client.pfx", "1234");

services.AddReverseProxy()
    .ConfigureHttpClient((context, handler) =>
    {
        handler.SslOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = [cert]
        };
    })
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Do not add to deployments, for debug reasons
IdentityModelEventSource.ShowPII = true;

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseNoUnauthorizedRedirect("/api");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapNotFound("/api/{**segment}");

app.MapReverseProxy();

app.MapFallbackToPage("/_Host");

app.Run();