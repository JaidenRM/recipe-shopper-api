using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Middlewares;
using RecipeShopper.Application.Pipelines;
using RecipeShopper.Data;

var builder = WebApplication.CreateBuilder(args);

//Add `ConfigureServices` logic here
builder.Services
    .AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorPipeline<,>));
builder.Services.AddTransient<ExceptionHandlerMiddleware>();
builder.Services.AddDbContext<RecipeShopperContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Dev")));
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program));

//==================================

var app = builder.Build();

// Add `Configure` logic here
app.UseHttpsRedirection();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
//============================

app.Run();
