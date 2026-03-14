// using InfrastructureLayer.Data.Context;
//using InfrastructureLayer.DependencyInjection;
//using InfrastructureLayer.Identity;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Orsync.Filters;
//using Orsync.Filters.Orsync.Filters;
//using Orsync.Middleware;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// ----------------------------
//// Add DbContext
//// ----------------------------
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// ----------------------------
//// Add Identity
//// ----------------------------
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.User.AllowedUserNameCharacters =
//        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
//    options.User.RequireUniqueEmail = true;

//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequiredLength = 6;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//// ----------------------------
//// Controllers
//// ----------------------------
//builder.Services.AddControllers();

//// ----------------------------
//// Application & Infrastructure DI
//// ----------------------------
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

//// ----------------------------
//// JWT Authentication
//// ----------------------------
//var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings["Issuer"],
//        ValidAudience = jwtSettings["Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//        ClockSkew = TimeSpan.Zero
//    };
//});

//builder.Services.AddAuthorization();

//// ----------------------------
//// Swagger / OpenAPI
//// ----------------------------
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Orsync API",
//        Version = "v1",
//        Description = "Orsync Pharmaceutical Market Analysis API with Clean Architecture"
//    });

//    // JWT Bearer
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.Http,
//        Scheme = "Bearer"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
//                In = ParameterLocation.Header,
//                Name = "Authorization"
//            },
//            Array.Empty<string>()
//        }
//    });

//    // File upload support
//    c.OperationFilter<SwaggerFileOperationFilter>();
//});

//// ----------------------------
//// CORS Policy
//// ----------------------------
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//var app = builder.Build();

//// ----------------------------
//// Middleware pipeline (تم تعديل الترتيب هنا)
//// ----------------------------

//// 1. Swagger in Development
//if (true)
//{
//    app.UseDeveloperExceptionPage();


//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orsync API v1");
//        c.RoutePrefix = "";
//    });
//}

//// 2. CORS (يجب أن يكون هنا قبل أي Middleware قد يولد خطأ)
//app.UseCors("AllowAll");

//// 3. Exception Handling (الآن سيرجع الـ Headers صح لو حصل خطأ)
//app.UseMiddleware<ExceptionHandlingMiddleware>();

//// 4. HTTPS Redirection
//app.UseHttpsRedirection();

//// 5. Static Files
//app.UseStaticFiles();

//// 6. Authentication & Authorization
//app.UseAuthentication();
//app.UseAuthorization();

//// 7. Map Controllers
//app.MapControllers();

//app.Run();

// #region old 
//using InfrastructureLayer.DependencyInjection;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http.Features; // لازم لـ FormOptions
//using Microsoft.Extensions.FileProviders;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//var builder = WebApplication.CreateBuilder(args);

//// Services

//builder.Services
//    .AddControllers()
//    .AddNewtonsoftJson(options =>
//    {
//        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
//    });

//builder.Services.AddEndpointsApiExplorer();

//// إضافة البنية التحتية
//builder.Services.AddInfrastructure(builder.Configuration);

//// إعداد JWT
//var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings["Issuer"],
//        ValidAudience = jwtSettings["Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
//    };
//});

//// إعداد CORS مرة واحدة فقط
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//// زيادة الحد الأقصى لحجم الملفات المرفوعة
//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = 100_000_000; // 100 MB
//});

//// إعداد Swagger مع JWT + Authorize تلقائي
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

//    // إعداد JWT Bearer
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "ضع رمز الـ JWT هنا (سيتم تعبئته تلقائيًا بعد تسجيل الدخول)"
//    });

//    // ربط الـ Security Requirement بكل الـ endpoints
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                },
//                Scheme = "bearer",
//                Name = "Authorization",
//                In = ParameterLocation.Header
//            },
//            Array.Empty<string>()
//        }
//    });
//});

//var app = builder.Build();

//// Middleware
//if (true)
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//        c.RoutePrefix = string.Empty;
//    });
//}



//app.UseHttpsRedirection();
//app.UseStaticFiles();


//app.UseAuthentication();
//app.UseAuthorization();


//app.MapControllers();

//app.Run();

//#endregion





using InfrastructureLayer.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------
// Services
// -------------------------------------------------
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orsync API", Version = "v1" });
});

// Infrastructure & Services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// File upload limit
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000; // 100 MB
});

var app = builder.Build();

// -------------------------------------------------
// Middleware
// -------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orsync API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// Static files for uploads
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
