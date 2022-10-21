using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Application.Middlewares;
using RecipeShopper.Application.Pipelines;
using RecipeShopper.Data;
using RecipeShopper.Infrastructure.Services;
using RecipeShopper.Infrastructure.Supermarkets.Woolworths;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//Add `ConfigureServices` logic here
builder.Services
    .AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ApiVersionReader = new HeaderApiVersionReader("X-API-Header");
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorPipeline<,>));
builder.Services.AddTransient<ExceptionHandlerMiddleware>();
builder.Services.AddTransient<ISupermarketService, WoolworthsService>();
builder.Services.AddTransient<SupermarketService>();
builder.Services.AddHttpClient<ISupermarketService, WoolworthsService>();

builder.Services.AddDbContext<RecipeShopperContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Dev")));
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(opts =>
{
    opts.CustomSchemaIds(type => type.ToString().Replace('+', '.'));
    opts.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Recipe Shopper API",
        Description = "An API used to manage recipes for a user and provide helpful methods with these recipes like being able to view prices of products at supermarkets",
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

//==================================

var app = builder.Build();

// Add `Configure` logic here
app.UseHttpsRedirection();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors(c => {
    c.AllowAnyOrigin();
    c.AllowAnyHeader();
    c.AllowAnyMethod();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//============================

app.Run();
