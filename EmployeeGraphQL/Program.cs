using Api.GraphQL;
using Api.GraphQL.Auth;
using Api.GraphQL.Inputs;
using EmployeeGraphQL.Application.Services;
using EmployeeGraphQL.Application.Settings;
using EmployeeGraphQL.GraphQL.Errors;
using EmployeeGraphQL.Infrastructure.Data;
using EmployeeGraphQL.Infrastructure.Middleware;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// Database Configuration (PostgreSQL)
// -----------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddHttpClient();

// -----------------------------------------
// REDIS (WSL Redis Server)
// -----------------------------------------
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>();

if (redisSettings?.IsEnabled == true)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { redisSettings.Connection ?? "localhost:6379" },
            AbortOnConnectFail = false,
            ConnectRetry = 5,
            ConnectTimeout = 5000
        };

        return ConnectionMultiplexer.Connect(config);
    });
}
else
{
    // 👇 IMPORTANT: register dummy redis so DI does not fail
    builder.Services.AddSingleton<IConnectionMultiplexer?>(sp => null);
}
// builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
// {
//     var config = new ConfigurationOptions
//     {
//         EndPoints = { "localhost:6379" },
//         AbortOnConnectFail = false,
//         ConnectRetry = 10,
//         ConnectTimeout = 10000,
//         SyncTimeout = 10000
//     };

//     return ConnectionMultiplexer.Connect(config);
// });

// 1️⃣ Register FluentValidation
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<RedisStreamKaryakarProducer>();
builder.Services.AddSingleton<RedisStreamProducer>();
builder.Services.AddSingleton<RedisCacheService>();

builder.Services.AddHostedService<DepartmentImportWorker>();
builder.Services.AddHostedService<CsvImportValidationWorker>();
builder.Services.AddHostedService<CsvImportExecuteWorker>();

//Import Services (MUST ADD)
builder.Services.AddScoped<ImportJobService>();
builder.Services.AddScoped<KaryakarValidationService>();

builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<AsmPermissionService>();
builder.Services.AddScoped<DepartmentScheduledJobService>();
builder.Services.AddScoped<IGraphQLPermissionProvider, GraphQLPermissionProvider>();
builder.Services.Configure<MisModel>(builder.Configuration.GetSection("MisApi"));
builder.Services.Configure<ASMModel>(builder.Configuration.GetSection("ASMService"));
builder.Services.Configure<HangfireDepartmentImportSettings>(builder.Configuration.GetSection("HangfireJobs:DepartmentImport"));
builder.Services.Configure<SsoModel>(builder.Configuration.GetSection("SsoService"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.AddScoped<IMisApiService, MisApiService>();
builder.Services.AddScoped<IAsmApiService, AsmApiService>();
builder.Services.AddHttpClient<ISsoService, SsoService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<CsvParserService>();
builder.Services.AddScoped<SyncKaryakarValidationService>();
builder.Services.AddScoped<KaryakarImportService>();


// -----------------------------------------
// GraphQL Configuration
// -----------------------------------------
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
   .UseField<ModuleAuthorizationMiddleware>()
     .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    })
   .ModifyCostOptions(opt =>
    {
        opt.EnforceCostLimits = true;
        opt.MaxFieldCost = 100;
    })
    .AddQueryType<Query>()
    .AddType<TemplateQuery>()
    .AddMutationType<Mutation>()
    .AddHttpRequestInterceptor<AuthRequestInterceptor>()
    .AddTypeExtension<DepartmentImportMutation>()
    .AddTypeExtension<KaryakarImportMutation>()
    .AddTypeExtension<TemplateMutation>()
    .AddTypeExtension<EmployeeMutation>()
    .AddTypeExtension<DepartmentMutation>()
    .AddTypeExtension<ProjectMutation>()
    .AddUploadType()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddErrorFilter<ValidationExceptionErrorFilter>()
    .AddMaxExecutionDepthRule(5);

// Validators
builder.Services.AddScoped<IValidator<TemplateInput>, TemplateInputValidator>();
builder.Services.AddScoped<IValidator<DepartmentInput>, DepartmentInputValidator>();
builder.Services.AddScoped<IValidator<EmployeeInput>, EmployeeValidator>();
builder.Services.AddScoped<IValidator<ProjectInput>, ProjectInputValidator>();

//Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 50;        // allow only 50
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;         // disable queue
    });
});


builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

var app = builder.Build();

//Hangfire
// 1️⃣ Initialize Hangfire JobStorage (MUST be before AddOrUpdate)
app.UseHangfireDashboard("/hangfire");

// 2️⃣ Load settings from appsettings.json
var depJobConfig = app.Services.GetRequiredService<IOptions<HangfireDepartmentImportSettings>>().Value;

// 3️⃣ Register recurring job (NOW storage is initialized)
RecurringJob.AddOrUpdate<DepartmentScheduledJobService>(
    depJobConfig.JobName,
    job => job.PublishDepartmentJob(),
    depJobConfig.Cron
);


app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GraphQLExceptionStatusCodeMiddleware>();
app.UseAuthorization();

//header
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("departmentId", out var depId))
    {
        context.Items["departmentId"] = depId.ToString();
    }

    await next();
});

// GraphQL Endpoint
app.MapGraphQL("/graphql").RequireRateLimiting("api");

app.Run();