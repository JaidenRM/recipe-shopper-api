using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RecipeShopper.Entities;
using System.Data;

namespace RecipeShopper.Data
{
    public class RecipeShopperContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;
        private readonly IConfiguration _config;

        public RecipeShopperContext(DbContextOptions<RecipeShopperContext> options, IConfiguration config) : base(options) 
        {
            _config = config;
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Supermarket> Supermarket => Set<Supermarket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasKey(k => new { k.Id, k.SupermarketId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(_config.GetConnectionString("Dev"));

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();

                await (_currentTransaction?.CommitAsync() ?? Task.CompletedTask);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
