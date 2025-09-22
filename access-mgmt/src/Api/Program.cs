using Application;
using FluentValidation;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);

// MediatR v12 registration
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<IApplicationAssemblyMarker>());

// AutoMapper and FluentValidation
builder.Services.AddAutoMapper(typeof(IApplicationAssemblyMarker).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();

builder.Services.AddControllers();

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.Run();

public partial class Program { }
