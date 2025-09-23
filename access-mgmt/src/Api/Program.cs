using Api.Filters;
using Api.Swagger;
using Application;
using Application.Common.Behaviors;
using FluentValidation;
using Infrastructure;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

public partial class Program {
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var authEnabled = builder.Configuration.GetValue("Auth:Enabled", false);

        builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApplicationAssemblyMarker>());
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddAutoMapper(typeof(IApplicationAssemblyMarker).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();

        builder.Services.AddControllers();
        builder.Services.AddApiVersioning(o => 
        { 
            o.AssumeDefaultVersionWhenUnspecified = true; 
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.ReportApiVersions = true;
        });
        builder.Services.AddVersionedApiExplorer(o =>
        {
            o.GroupNameFormat = "'v'VVV"; // v1, v1.1, etc.
            o.SubstituteApiVersionInUrl = true;
        });
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter **only** your access token. Swagger will add the `Bearer ` prefix.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var allowCreds = builder.Configuration.GetValue("Cors:AllowCredentials", false);
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            p.WithOrigins(corsOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod();
            if (allowCreds) p.AllowCredentials();
        }));

        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>(name: "db");

        builder.Services.Configure<StaticBearerOptions>(builder.Configuration.GetSection(StaticBearerOptions.SectionName));
        if (authEnabled)
        {
            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = builder.Configuration["Auth:Authority"];  // e.g. https://login.microsoftonline.com/<tenant>/v2.0
            //        options.Audience = builder.Configuration["Auth:Audience"];   // e.g. api://your-api-id
            //        options.RequireHttpsMetadata = false;  
            //    });

            // Simple static bearer auth 
            builder.Services.AddStaticBearerAuthentication();

            builder.Services.AddAuthorization(options =>
            {
                // policy
                options.AddPolicy("CanViewReports", p => p.RequireClaim("permission", "CanViewReports"));
                options.AddPolicy("CanManageUsers", p => p.RequireClaim("permission", "CanManageUsers"));
            });
        }

        var app = builder.Build();
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseCors();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHttpsRedirection();

        if (authEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                    options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());

                options.RoutePrefix = string.Empty;
            });
        }
        app.MapControllers();
        app.MapHealthChecks("/health");

        var migrate = builder.Configuration.GetValue("Database:MigrateOnStartup", false);
        var seed = builder.Configuration.GetValue("Database:SeedOnStartup", false);
        if (migrate || seed)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
            if (migrate) await db.Database.MigrateAsync();
            if (seed) await Infrastructure.Seed.DbSeeder.SeedAsync(db);
        }
        app.Run();
    }
}

public partial class Program { }


