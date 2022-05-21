using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace RecipeShopper.Features.Recipes
{
    public class GetRecipes
    {
        public record Query(int[] Ids) : IQuery<Response>;

        public record ModelProduct(int Id, int SupermarketId, string Name);
        public record ModelIngredient(int Id, string Name, decimal Quantity, string MeasurementUnit, ModelProduct LinkingProduct);
        public record ModelInstruction(int Id, int Order, string Description);
        public record Model(int Id, string Name, string Description, string Tags, int Servings, int DurationMinutes, DateTime LastModifiedUTC, DateTime CreatedOnUTC, List<ModelIngredient> Ingredients, List<ModelInstruction> Instructions);

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(r => r.Ids).NotNull();
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Product, ModelProduct>();
                CreateProjection<Ingredient, ModelIngredient>()
                    .ForCtorParam(
                        "MeasurementUnit",
                        opt => opt.MapFrom(src => src.Unit.ToFriendlyString()))
                    .ForCtorParam(
                        "LinkingProduct",
                        opt => opt.MapFrom(src => src.Product));
                CreateProjection<Instruction, ModelInstruction>();
                CreateProjection<Recipe, Model>();
            }
        }

        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            private readonly IConfigurationProvider _config;

            public Handler(RecipeShopperContext db, IConfigurationProvider config)
            {
                _db = db;
                _config = config;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Recipe> recipes = _db.Recipes;

                if (request.Ids.Any())
                    recipes = recipes.Where(r => request.Ids.Contains(r.Id));

                var results = await recipes
                    .ProjectTo<Model>(_config)
                    .ToListAsync();

                return new Response(results);
            }
        }

        public record Response(List<Model> Results);
    }
}
