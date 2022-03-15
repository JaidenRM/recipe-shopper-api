using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Data;

var builder = WebApplication.CreateBuilder(args);

//Add `ConfigureServices` logic here
builder.Services.AddDbContext<RecipeShopperContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Dev")));
builder.Services.AddMediatR(typeof(Program));
//==================================

var app = builder.Build();

// Add `Configure` logic here

//============================

app.MapGet("/", () => "Hello World!");

app.Run();

public partial class Program { }