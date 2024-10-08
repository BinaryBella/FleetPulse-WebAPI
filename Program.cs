using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;
using FleetPulse_BackEndDevelopment.Configuration;
using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Filters;
using FleetPulse_BackEndDevelopment.Helpers;
using FleetPulse_BackEndDevelopment.Quartz.Jobs;
using FleetPulse_BackEndDevelopment.Services;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using FirebaseAdmin.Messaging;
using AutoMapper;
using FleetPulse_BackEndDevelopment.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Initialize Firebase
FirebaseInitializer.InitializeFirebase();

// Register FirebaseMessaging
builder.Services.AddSingleton(provider => FirebaseMessaging.DefaultInstance);

// Add services to the container.
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
Configure(app, app.Environment);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Add Quartz.NET
    services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();

        var jobKey = new JobKey("SendMaintenanceNotificationJob");
        q.AddJob<SendMaintenanceNotificationJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("SendMaintenanceNotificationTrigger")
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(60)
                .RepeatForever()));
    });

    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    services.AddTransient<IMailService, FleetPulse_BackEndDevelopment.Services.MailService>();
    services.AddScoped<VehicleService>();

    // Add controllers with options
    services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    }).AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

    // Add Swagger
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FleetPulse API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please enter a valid JWT token",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        options.OperationFilter<AuthResponsesOperationFilter>();
        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        options.IgnoreObsoleteActions();
        options.IgnoreObsoleteProperties();
        options.CustomSchemaIds(type => type.FullName);
    });

    // Add authentication
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });

    services.AddAuthorization();

    // Add CORS
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    // Add DbContext
    services.AddDbContext<FleetPulseDbContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("SqlServerConnectionString"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
    });

    // Add AutoMapper
    services.AddAutoMapper(typeof(MappingProfiles));

    // Declared services
    services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
    services.AddScoped<DBSeeder>();
    services.AddScoped<IVehicleTypeService, VehicleTypeService>();
    services.AddScoped<IManufactureService, ManufactureService>();
    services.AddScoped<IVehicleService, VehicleService>();
    services.AddScoped<ITripService, TripService>();
    services.AddScoped<IDriverService, DriverService>();
    services.AddScoped<IHelperService, HelperService>();
    services.AddScoped<IStaffService, StaffService>();
    services.AddScoped<IAccidentService, AccidentService>();
    services.AddScoped<IPushNotificationService, PushNotificationService>();
    services.AddScoped<IMailService, MailService>();
    services.AddScoped<IVerificationCodeService, VerificationCodeService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<DBSeeder>();
    services.AddScoped<IVehicleMaintenanceService, VehicleMaintenanceService>();
    services.AddScoped<IVehicleMaintenanceTypeService, VehicleMaintenanceTypeService>();
    services.AddScoped<IFuelRefillService, FuelRefillService>();
    services.AddScoped<IPushNotificationService, PushNotificationService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IVehicleMaintenanceConfigurationService, VehicleMaintenanceConfigurationService>();
    services.AddScoped<SendMaintenanceNotificationJob>();
    services.AddScoped<IDriverService, DriverService>();
    services.AddScoped<IEmailUserCredentialService, EmailUserCredentialService>();

    // Add HttpContextAccessor
    services.AddHttpContextAccessor();

    // Add HttpClient
    services.AddHttpClient();

    // Add AutoMapper profiles
    services.AddAutoMapper(typeof(AccidentProfile));

    // Add logging (if needed)
    services.AddLogging();
}

void Configure(WebApplication app, IWebHostEnvironment env)
{
    app.UseHttpsRedirection();
    
    // Add static files middleware
    app.UseStaticFiles();
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FleetPulse API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger the root page
    });

    app.MapControllers();
}