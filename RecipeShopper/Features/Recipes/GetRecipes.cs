using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
using RecipeShopper.Features.Products;

namespace RecipeShopper.Features.Recipes
{
    public class GetRecipes
    {
        public record Query(int[] Ids) : IQuery<Response>;

        public record ModelIngredient(int Id, string Name, decimal Quantity, string MeasurementUnit, GetProducts.Model LinkingProduct);
        public record ModelInstruction(int Id, int Order, string Description);
        public record Model(int Id, string Name, string Description, string Tags, int Servings, int DurationMinutes, DateTime LastModifiedUTC, DateTime CreatedOnUTC, List<ModelIngredient> Ingredients, List<ModelInstruction> Instructions);

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(r => r.Ids).NotNull();
            }
        }

        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            private readonly IMediator _mediator;

            public Handler(RecipeShopperContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Recipe> recipes = _db.Recipes;

                if (request.Ids.Any())
                    recipes = recipes.Where(r => request.Ids.Contains(r.Id));

                var dbRecipes = await recipes
                    .Include(r => r.Ingredients).ThenInclude(i => i.Product).ThenInclude(p => p.Supermarket)
                    .Include(r => r.Instructions)
                    .ToListAsync();

                var productIdsBySupermarketId = dbRecipes
                    .SelectMany(r => r.Ingredients)
                    .Where(i => i.Product != null)
                    .Select(i => i.Product)
                    .GroupBy(p => p.SupermarketId)
                    .ToDictionary(g => g.Key, g => g.Select(p => p.Id).ToArray());

                var productsQuery = new GetProducts.Query(productIdsBySupermarketId);
                var products = (await _mediator.Send(productsQuery)).Results;

                var mappedRecipes = recipes
                    .Select(r => MapToModel(r, products))
                    .ToList();

                return new Response(mappedRecipes);  
            }

            private static Model MapToModel(Recipe recipe, List<GetProducts.Model> products)
            {
                var instructions = recipe.Instructions
                    .Select(i => new ModelInstruction(i.Id, i.Order, i.Description))
                    .ToList();
                var ingredients = recipe.Ingredients
                    .Select(i =>
                    {
                        GetProducts.Model product = null;

                        if (i.Product != null)
                        {
                            var foundProduct = products.SingleOrDefault(p => p.Id == i.Product.Id && p.SupermarketId == i.Product.SupermarketId);
                            product = new GetProducts.Model(i.Product.Id, i.Product.Name, foundProduct.FullPrice, foundProduct.CurrentPrice, i.Product.SupermarketId, i.Product.Supermarket.Name);
                        }

                        return new ModelIngredient(i.Id, i.Name, i.Quantity, i.Unit.ToFriendlyString(), product);
                    })
                    .ToList();

                return new Model(recipe.Id, recipe.Name, recipe.Description, recipe.Tags, recipe.Servings, recipe.DurationMinutes, recipe.LastModifiedUTC, recipe.CreatedOnUTC, ingredients, instructions);
            }
        }

        public record Response(List<Model> Results);
    }
}
