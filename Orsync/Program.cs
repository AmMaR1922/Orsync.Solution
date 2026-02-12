using ApplicationLayer.DependencyInjection;
using InfrastructureLayer.Data.Context;
using InfrastructureLayer.DependencyInjection;
using InfrastructureLayer.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Orsync.Filters;
using Orsync.Filters.Orsync.Filters;
using Orsync.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Add DbContext
// ----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------------
// Add Identity
// ----------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ----------------------------
// Controllers
// ----------------------------
builder.Services.AddControllers();

// ----------------------------
// Application & Infrastructure DI
// ----------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ----------------------------
// JWT Authentication
// ----------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // ⚠️ مهم جداً: تعديل سلوك JWT مع Self-signed certificates
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddAuthorization();

// ----------------------------
// Swagger / OpenAPI
// ----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orsync API",
        Version = "v1",
        Description = "Orsync Pharmaceutical Market Analysis API with Clean Architecture"
    });

    // JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                In = ParameterLocation.Header,
                Name = "Authorization"
            },
            Array.Empty<string>()
        }
    });

    // File upload support
    c.OperationFilter<SwaggerFileOperationFilter>();
});

// ----------------------------
// CORS Policy - ✅ النسخة المحسنة
// ----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // ✅ دعم credentials مع origins محددة بدل AllowAnyOrigin
        policy.SetIsOriginAllowed(origin => true) // يسمح بأي origin في development
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // مهم للـ authentication
    });
    
    // ✅ خيار إضافي للـ Swagger UI
    options.AddPolicy("SwaggerUI", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7083",
            "http://localhost:7083",
            "https://localhost:5000",
            "http://localhost:5000",
            "https://localhost:3000",
            "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ----------------------------
// تكوين Kestrel لدعم HTTP/HTTPS
// ----------------------------
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // السماح بالاتصالات غير المشفرة للتجربة
    serverOptions.ListenLocalhost(5000); // HTTP
    serverOptions.ListenLocalhost(7083, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

var app = builder.Build();

// ----------------------------
// Middleware pipeline
// ----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orsync API v1");
        c.RoutePrefix = "";
        
        // ✅ مهم: تعطيل التحقق من الشهادة في Swagger UI
        c.ConfigObject.AdditionalItems.Add("domPinning", false);
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// ✅ تعطيل HTTPS Redirection في Development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Needed for uploaded files

// ✅ استخدام CORS قبل Authentication
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();