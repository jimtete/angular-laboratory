using System.IdentityModel.Tokens.Jwt;
using System.Text;
using LearningLab.Data;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Repositories.CharacterSheetRepository;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.ErrorHandling;
using LearningLab.Security;
using LearningLab.Services.AuthService;
using LearningLab.Services.CharacterSheetService;
using LearningLab.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicy = "DefaultCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LearningLabContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "JWT signing key is required.")
    .Validate(options => options.SigningKey.Length >= 32, "JWT signing key must be at least 32 characters.")
    .Validate(options => options.ExpirationMinutes > 0, "JWT expiration must be greater than zero.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "A valid access token is required.",
                    Data = null
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "You are not authorized to access this resource.",
                    Data = null
                });
            }
        };
    });

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICharacterSheetRepository, CharacterSheetRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICharacterSheetService, CharacterSheetService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
