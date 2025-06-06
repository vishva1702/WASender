﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WASender.Contracts;
using WASender.Contracts.AdminSide;
using WASender.Helpers;
using WASender.Models;
using WASender.Services;
using WASender.Services.AdminSide;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure MySQL Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Enable localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// JWT Configuration
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.Cookies";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
    options.SlidingExpiration = true;

})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Disable default 5-minute clock skew
    };
});


// Enable CORS (adjust origins as necessary)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://localhost:5001")
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Enable session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Services (Dependency Injection)
// 🔹 Register Services (Dependency Injection)
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<ISeoService, SeoService>();
builder.Services.AddScoped<UploaderHelper>();
builder.Services.AddScoped<BlogHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IGlobalDataService, GlobalDataService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IAboutService, AboutService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<WASender.Contracts.IEmailSender, WASender.Services.EmailSender>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<NotificationHelper>();
builder.Services.AddScoped<IUserTemplateService, UserTemplateService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IEnvService, EnvService>();
builder.Services.AddScoped<IAdminTemplateService, AdminTemplateService>();
builder.Services.AddScoped<IAdminScheduleService, AdminScheduleService>();
builder.Services.AddScoped<UploaderHelper>();
builder.Services.AddScoped<BlogHelper>();



var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Apply CORS before Authentication
app.UseCors("AllowAll");

// Enable session, authentication, and authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Middleware to redirect unauthorized users AFTER authentication runs
app.Use(async (context, next) =>
{
    if ((context.Request.Path.StartsWithSegments("/Admin") || context.Request.Path.StartsWithSegments("/User"))
        && !context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Login");
        return;
    }
    await next();
});

// Configure MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();