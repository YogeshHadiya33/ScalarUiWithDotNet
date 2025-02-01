using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ScalarDotNetExample;
using ScalarDotNetExample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

builder.Services
    .AddAuthorization()
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("<your_jwt_key>")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "This is my Scalar API";
    options.DarkMode = true;
    options.Favicon = "path";
    options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.RestSharp);
    options.HideModels = false;
    options.Layout = ScalarLayout.Modern;
    options.ShowSidebar = true;

    options.Authentication = new ScalarAuthenticationOptions
    {
        PreferredSecurityScheme = "Bearer"
    };
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

#region Endpoints

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/test1", () => "This is test 1 endpoint").WithName("Test1").RequireAuthorization();
app.MapGet("/test2", () => "This is test 2 endpoint").WithName("Test2");
app.MapGet("/test3", () => "This is test 3 endpoint").WithName("Test3");
app.MapGet("/test4", () => "This is test 4 endpoint").WithName("Test4");
app.MapGet("/test5", () => "This is test 5 endpoint").WithName("Test5");
app.MapGet("/test6", () => "This is test 6 endpoint").WithName("Test6");
app.MapGet("/test7", () => "This is test 7 endpoint").WithName("Test7");

#endregion

app.Run();