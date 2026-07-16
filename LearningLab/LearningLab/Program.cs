using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearningLab.Data;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Repositories.AssetQueryRepository;
using LearningLab.Data.Repositories.AssetRepository;
using LearningLab.Data.Repositories.CampaignQueryRepository;
using LearningLab.Data.Repositories.CampaignMilestoneRepository;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSessionRepository;
using LearningLab.Data.Repositories.CampaignSettingsRepository;
using LearningLab.Data.Repositories.CharacterSheetRepository;
using LearningLab.Data.Repositories.NotificationCommandRepository;
using LearningLab.Data.Repositories.NotificationQueryRepository;
using LearningLab.Data.Repositories.RoleRepository;
using LearningLab.Data.Repositories.SessionNoteRepository;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.ErrorHandling;
using LearningLab.Infrastructure.Eventing;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Security;
using LearningLab.Security.AccessPermissions;
using LearningLab.Services.AssetService;
using LearningLab.Services.AuthService;
using LearningLab.Services.CampaignContentService;
using LearningLab.Services.CampaignParticipationInviteService;
using LearningLab.Services.CampaignSessionService;
using LearningLab.Services.CampaignSettingsService;
using LearningLab.Services.CampaignService;
using LearningLab.Services.CharacterSheetService;
using LearningLab.Services.NotificationService;
using LearningLab.Services.Security;
using LearningLab.Services.UserService;
using LearningLab.Sockets.Extensions;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicy = "DefaultCorsPolicy";

var builder = WebApplication.CreateBuilder(args);
var assetsRoot = Path.Combine(builder.Environment.ContentRootPath, "assets");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection connection string is required.");

builder.Services.AddDbContext<LearningLabContext>(options =>
    options.UseSqlServer(defaultConnectionString));
builder.Services.AddSingleton<IDbConnectionFactory>(
    _ => new SqlConnectionFactory(defaultConnectionString));

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
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken)
                    && path.StartsWithSegments(SocketEndpointPaths.Root))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
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
builder.Services.AddScoped<IAssetQueryRepository, AssetQueryRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<ICampaignQueryRepository, CampaignQueryRepository>();
builder.Services.AddScoped<ICampaignMilestoneRepository, CampaignMilestoneRepository>();
builder.Services.AddScoped<ICampaignParticipationInviteRepository, CampaignParticipationInviteRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignSessionRepository, CampaignSessionRepository>();
builder.Services.AddScoped<ICampaignSettingsRepository, CampaignSettingsRepository>();
builder.Services.AddScoped<ICharacterSheetRepository, CharacterSheetRepository>();
builder.Services.AddScoped<INotificationCommandRepository, NotificationCommandRepository>();
builder.Services.AddScoped<INotificationQueryRepository, NotificationQueryRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISessionNoteRepository, SessionNoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICampaignContentService, CampaignContentService>();
builder.Services.AddScoped<ICampaignParticipationInviteService, CampaignParticipationInviteService>();
builder.Services.AddScoped<ICampaignSessionService, CampaignSessionService>();
builder.Services.AddScoped<ICampaignSettingsService, CampaignSettingsService>();
builder.Services.AddScoped<ICharacterSheetService, CharacterSheetService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddLearningLabEventHub();
builder.Services.AddAccessPermissionAuthorization();
builder.Services.AddLearningLabAssetStorage(assetsRoot);
builder.Services.AddLearningLabSockets();

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
app.UseLearningLabStaticAssets(assetsRoot);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapLearningLabSockets();

app.Run();
