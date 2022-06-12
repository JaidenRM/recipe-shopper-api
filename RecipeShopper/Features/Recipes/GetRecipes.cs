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
        /// <summary>Query for recipes with the passed in ids</summary>
        public record Query(int[] Ids) : IQuery<Response>
        {
            /// <example>[1, 44, 57]</example>
            public int[] Ids { get; init; } = Ids;
        }

        /// <summary>Contains details of a specific ingredient used in this recipe</summary>
        public record ModelIngredient(int Id, string Name, decimal Quantity, string MeasurementUnit, GetProducts.Model LinkingProduct)
        {
            /// <example>1</example>
            public int Id { get; init; } = Id;
            /// <example>Tomato Paste</example>
            public string Name { get; init; } = Name;
            /// <summary>The amount you want. The unit is determined by `MeasurementUnit`</summary>
            /// <example>2</example>
            public decimal Quantity { get; init; } = Quantity;
            /// <summary>
            ///     This is used to give meaning to `Quantity` so we know how we can measure it. 
            ///     Valid values for this include: `"none"`, `"each"`, `"teaspoon"`, `"tablespoon"`, `"grams"`, `"kilograms"`, `"millilitres"`, `"litres"`, `"cup"`, `"pinch"`
            /// </summary>
            /// <example>Tablespoon</example>
            public string MeasurementUnit { get; init; } = MeasurementUnit;
            /// <summary>This is the a specific version of the ingredient from a supermarket</summary>
            public GetProducts.Model LinkingProduct { get; init; } = LinkingProduct;
        };

        /// <summary>Contains details on one part of how to make this recipe</summary>
        public record ModelInstruction(int Id, int Order, string Description)
        {
            /// <example>1</example>
            public int Id { get; init; } = Id;
            /// <summary>Used to generate the order this instruction will appear. Lowest -> Highest</summary>
            /// <example>1</example>
            public int Order { get; init; } = Order;
            /// <summary>Describes what someone should be doing for this instruction</summary>
            /// <example>Finely chop the garlic</example>
            public string Description { get; init; } = Description;
        };

        /// <summary>Used to represent the components of a recipe</summary>
        public record Model(int Id, string Name, string Description, string Tags, int Servings, int DurationMinutes, DateTime LastModifiedUTC, DateTime CreatedOnUTC, List<ModelIngredient> Ingredients, List<ModelInstruction> Instructions)
        {
            /// <summary>Id of the recipe</summary>
            /// <example>1</example>
            public int Id { get; init; } = Id;
            /// <example>Recipe #1</example>
            public string Name { get; init; } = Name;
            /// <example>This is my very first recipe</example>
            public string Description { get; init; } = Description;
            /// <summary>Comma-separated list of ways to categorise your recipe</summary>
            /// <example>Quick,Easy,Healthy</example>
            public string Tags { get; init; } = Tags;
            /// <summary>The approximate number of people this recipe would serve</summary>
            /// <example>4</example>
            public int Servings { get; init; } = Servings;
            /// <summary>The total length of time it would take to make this recipe on average</summary>
            /// <example>30</example>
            public int DurationMinutes { get; init; } = DurationMinutes;
            /// <summary>The last time the recipe was modified in UTC format</summary>
            public DateTime LastModifiedUTC { get; init; } = LastModifiedUTC;
            /// <summary>When this recipe was first created in UTC format</summary>
            public DateTime CreatedOnUTC { get; init; } = CreatedOnUTC;
        };

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
