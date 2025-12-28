using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ScalarDotNetExample;
using ScalarDotNetExample.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var jwtKey = "0123456789abcdef0123456789abcdef";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
	options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

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
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
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

app.MapGet("/token", () =>
	{
		var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new(ClaimTypes.Name, "yogesh"),
			new(ClaimTypes.Email, "yogesh@gmail.com"),
			new(ClaimTypes.Role, "Admin"),
			new(JwtRegisteredClaimNames.Sub, "yogesh"),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var token = new JwtSecurityToken(
			null, // ValidateIssuer = false
			null, // ValidateAudience = false
			claims,
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(1),
			creds
		);

		var jwt = new JwtSecurityTokenHandler().WriteToken(token);

		return Results.Ok(new
		{
			access_token = jwt,
			token_type = "Bearer",
			expires_in = 3600
		});
	})
	.WithName("Token");

app.MapGet("/protected-endpoint", () => "This is Protected Endpoint.Won't work without valid JWT token").WithName("Protected Endpoint").RequireAuthorization();
app.MapGet("/test2", () => "This is test 2 endpoint").WithName("Test2");
app.MapGet("/test3", () => "This is test 3 endpoint").WithName("Test3");
app.MapGet("/test4", () => "This is test 4 endpoint").WithName("Test4");
app.MapGet("/test5", () => "This is test 5 endpoint").WithName("Test5");
app.MapGet("/test6", () => "This is test 6 endpoint").WithName("Test6");
app.MapGet("/test7", () => "This is test 7 endpoint").WithName("Test7");

#endregion

app.Run();