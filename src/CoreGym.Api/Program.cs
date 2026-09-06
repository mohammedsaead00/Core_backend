using System.Text;
using CoreGym.Api;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Authorization;
using CoreGym.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CoreGymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoreGym")));

builder.Services.AddCoreGymAuthorization();
builder.Services.AddCoreGymApplicationServices();

ConfigureJwt(builder);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Entities carry enums (profiles.role); serialize as camelCase strings like Supabase ('client').
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
});
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CoreGymExceptionHandler>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CoreGymDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

// Two interchangeable JWT modes until the IdP decision is final (open question 7):
//   Jwt:SigningKey → locally-signed HS256 tokens (dev/test, or any HS256 issuer)
//   Jwt:Authority  → any OpenID Connect provider (Supabase Auth, Auth0, Keycloak, ...)
static void ConfigureJwt(WebApplicationBuilder builder)
{
    var jwt = builder.Configuration.GetSection("Jwt");
    var signingKey = jwt["SigningKey"];
    var authority = jwt["Authority"];
    var issuer = jwt["Issuer"];
    var audience = jwt["Audience"];

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            if (!string.IsNullOrWhiteSpace(signingKey))
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = issuer is not null,
                    ValidIssuer = issuer,
                    ValidateAudience = audience is not null,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    NameClaimType = "sub",
                };
            }
            else if (!string.IsNullOrWhiteSpace(authority))
            {
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = audience is not null,
                    ValidAudience = audience,
                    NameClaimType = "sub",
                };
            }
            else
            {
                throw new InvalidOperationException("Configure either Jwt:SigningKey or Jwt:Authority.");
            }
        });
}

public partial class Program
{
}
