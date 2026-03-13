using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using TechExpress.Application.Common;
using TechExpress.Application.Middlewares;
using TechExpress.Repository;
using TechExpress.Repository.Contexts;
using TechExpress.Service;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;
using TechExpress.Service.Initializers;
using TechExpress.Service.Services;
using TechExpress.Service.Utils;
using Anthropic;
using TechExpress.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    _ = options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Tech Express API",
            Version = "v1",
            Description = "API Document for Tech Express"
        };

        var securityScheme = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JSON Web Token",
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securityScheme;

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
        return Task.CompletedTask;
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// Anthropic AI configuration
var anthropicApiKey = builder.Configuration["AI:ApiKey"] ?? "";
var promptPath = Path.Combine(AppContext.BaseDirectory,
    builder.Configuration["AI:SystemPromptPath"] ?? "Prompts/chat-system-prompt.txt");
var systemPrompt = await File.ReadAllTextAsync(promptPath);
var anthropicClient = new AnthropicClient() { ApiKey = anthropicApiKey };
builder.Services.AddSingleton(anthropicClient);
builder.Services.AddSingleton(new ChatAiService(anthropicClient, systemPrompt));

// SQL Server configuration
var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(sqlConnectionString, sqlServerOptionsAction: sqlOpt =>
    {
        sqlOpt.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
});

builder.Services.AddScoped<ApplicationDbContext>();



builder.Services.AddScoped<UnitOfWork>();

builder.Services.AddScoped<SmtpEmailSender>();
builder.Services.AddScoped<PayOsClient>();
builder.Services.AddScoped<GoogleAuthUtils>();



builder.Services.AddScoped<ServiceProviders>();

// Jwt Authentication configuration
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/cartHub") || path.StartsWithSegments("/chatHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnForbidden = async context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "You are not allowed to perform this action" });
            }
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                // Surface the specific token failure reason if one exists
                var message = context.AuthenticateFailure switch
                {
                    SecurityTokenExpiredException => "Token expired",
                    SecurityTokenInvalidSignatureException => "Invalid token signature",
                    SecurityTokenMalformedException => "Malformed token",
                    not null => "Unauthenticated access",
                    null => "Unauthorized access"
                };
                await context.Response.WriteAsJsonAsync(new { error = message });
            }
        }
    };
});
builder.Services.AddSingleton<JwtUtils>();

// Binding validation configuration
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Error(s) at model has been found: " + string.Join(", ", errors)
        };
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContext>();

// Redis server configuration
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString! + ",abortConnect=false");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddScoped<RedisUtils>();
builder.Services.AddScoped<OtpUtils>();
builder.Services.AddScoped<SmtpEmailSender>();

builder.Services.AddHostedService<CleanOrderWorkerService>();
builder.Services.AddHostedService<SetOrderCompleteWorker>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.UseInlineDefinitionsForEnums();
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

builder.Services.AddHostedService<AdminInitializer>();
builder.Services.AddHostedService<ChangePromotionStatusWorker>();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new()
                {
                    Url = $"{httpReq.Scheme}://{httpReq.Host.Value}"
                }
            };
        });
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tech Express API v1");
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();

app.UseMiddleware<UserStatusMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await CategoriesInitializer.Init(context);
    await SpecDefinitionsInitializer.Init(context);
    await BrandsInitializer.Init(context);
    await ProductsInitializer.Init(context);
}   

app.MapHub<CartHub>("/cartHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();
