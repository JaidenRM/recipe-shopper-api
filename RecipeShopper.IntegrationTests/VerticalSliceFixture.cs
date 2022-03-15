using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RecipeShopper.Data;
using RecipeShopper.Entities;
using Respawn;
using Xunit;

namespace RecipeShopper.IntegrationTests;

// We will use this in our tests. This collection interface allows for contexts to be shared b/w all our tests in each class and method so we don't need to rerun this expensive ctor
[CollectionDefinition(nameof(VerticalSliceFixture))]
public class VerticalSliceFixtureCollection : ICollectionFixture<VerticalSliceFixture> { }

public class VerticalSliceFixture : IAsyncLifetime
{
    private readonly Checkpoint _checkpoint;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public VerticalSliceFixture()
    {
        _factory = new RecipeShopperApplicationFactory();

        _config = _factory.Services.GetRequiredService<IConfiguration>();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        _checkpoint = new Checkpoint {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres,
        };
    }

    class RecipeShopperApplicationFactory
        : WebApplicationFactory<Program>
    {
        private readonly string TestDbConnStr = "Server=127.0.0.1;Port=5432;Database=postgres-test;User Id = postgres; Password=local123;Include Error Detail=true;";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:Dev", TestDbConnStr }
                });
            });
        }
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipeShopperContext>();

        try
        {
            await dbContext.BeginTransactionAsync();

            await action(scope.ServiceProvider);

            await dbContext.CommitTransactionAsync();
        }
        catch (Exception)
        {
            dbContext.RollbackTransaction();
            throw;
        }
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipeShopperContext>();

        try
        {
            await dbContext.BeginTransactionAsync();

            var result = await action(scope.ServiceProvider);

            await dbContext.CommitTransactionAsync();

            return result;
        }
        catch (Exception)
        {
            dbContext.RollbackTransaction();
            throw;
        }
    }

    public Task ExecuteDbContextAsync(Func<RecipeShopperContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>()));

    public Task ExecuteDbContextAsync(Func<RecipeShopperContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>()).AsTask());

    public Task ExecuteDbContextAsync(Func<RecipeShopperContext, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>(), sp.GetService<IMediator>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<RecipeShopperContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<RecipeShopperContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>()).AsTask());

    public Task<T> ExecuteDbContextAsync<T>(Func<RecipeShopperContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipeShopperContext>(), sp.GetService<IMediator>()));

    public Task InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }
            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task<T> FindAsync<T>(int id)
        where T : class, IEntity
    {
        return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return mediator.Send(request);
        });
    }

    public Task SendAsync(IRequest request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();


            return mediator.Send(request);
        });
    }

    // Cleans database
    private async Task ResetCheckpoint()
    {
        using (var conn = new NpgsqlConnection(_config.GetConnectionString("DevTest")))
        {
            await conn.OpenAsync();

            await _checkpoint.Reset(conn);
        }
    }

    public Task InitializeAsync() => ResetCheckpoint();

    public Task DisposeAsync()
    {
        _factory?.Dispose();
        return Task.CompletedTask;
    }
}