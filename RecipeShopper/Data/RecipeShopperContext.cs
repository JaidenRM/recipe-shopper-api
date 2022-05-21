using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            // datetime auto updaters
            ChangeTracker.Tracked += OnEntityTracked;
            ChangeTracker.StateChanged += OnEntityStateChanged;
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Supermarket> Supermarket => Set<Supermarket>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<Instruction> Instructions => Set<Instruction>();

        void OnEntityTracked(object sender, EntityTrackedEventArgs e)
        {
            if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is IUpdateable entity)
            {
                var utcNow = DateTime.UtcNow;

                entity.LastModifiedUTC = utcNow;
                entity.CreatedOnUTC = utcNow;
            }
        }

        void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is IUpdateable entity)
                entity.LastModifiedUTC = DateTime.UtcNow;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasKey(k => new { k.Id, k.SupermarketId });
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supermarket)
                .WithMany(s => s.Products);

            modelBuilder.Entity<Instruction>()
                .HasOne(i => i.Recipe)
                .WithMany(r => r.Instructions);
            modelBuilder.Entity<Instruction>()
                .HasIndex(i => new { i.RecipeId, i.Order })
                .IsUnique();

            modelBuilder.Entity<Ingredient>()
                .HasOne(i => i.Recipe)
                .WithMany(r => r.Ingredients);
            modelBuilder.Entity<Ingredient>()
                .HasOne(i => i.Product)
                .WithOne()
                .HasForeignKey<Ingredient>(i => new { i.ProductId, i.SupermarketId });

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
