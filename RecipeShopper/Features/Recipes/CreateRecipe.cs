using AutoMapper;
using FluentValidation;
using MediatR;
using RecipeShopper.Application.Extensions;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Recipes
{
    public class CreateRecipe
    {
        /// <summary>Properties used to create a new recipe</summary>
        public record Command(string Name, string Description, int Servings, int DurationMinutes, string Tags, List<CreateIngredient> Ingredients, List<CreateInstruction> Instructions) : ICommand<int>
        {
            /// <summary>What to call the recipe</summary>
            /// <example>Recipe #2</example>
            public string Name { get; init; } = Name;
            /// <example>This is my recipe on how to make...</example>
            public string Description { get; init; } = Description;
            /// <summary>The approximate number of people this recipe would serve</summary>
            /// <example>2</example>
            public int Servings { get; init; } = Servings;
            /// <summary>The total length of time it would take to make this recipe on average</summary>
            /// <example>10</example>
            public int DurationMinutes { get; init; } = DurationMinutes;
            /// <summary>Comma-separated list of ways to categorise your recipe</summary>
            /// <example>Simple,Popular,Indulgent</example>
            public string Tags { get; init; } = Tags;
        };

        /// <summary>The supermarket representation of the mention ingredient</summary>
        public record CreateProduct(int Id, int SupermarketId, string Name)
        {
            /// <summary>Represents the id of this product from a specific store</summary>
            /// <example>345679</example>
            public int Id { get; init; } = Id;
            /// <summary>Name of the product from the store</summary>
            /// <example>Martin's Milk (1L)</example>
            public string Name { get; init; } = Name;
            /// <summary>The internal id used to track which supermarket this product is from</summary>
            /// <example>2</example>
            public int SupermarketId { get; init; } = SupermarketId;
        };

        /// <summary>Contains details of a specific ingredient used in this recipe</summary>
        public record CreateIngredient(string Name, decimal Quantity, string MeasurementUnit, CreateProduct LinkingProduct)
        {
            /// <example>Milk</example>
            public string Name { get; init; } = Name;
            /// <summary>The amount you want. The unit is determined by `MeasurementUnit`</summary>
            /// <example>300</example>
            public decimal Quantity { get; init; } = Quantity;
            /// <summary>
            ///     This is used to give meaning to `Quantity` so we know how we can measure it. 
            ///     Valid values for this include: `"none"`, `"each"`, `"teaspoon"`, `"tablespoon"`, `"grams"`, `"kilograms"`, `"millilitres"`, `"litres"`, `"cup"`, `"pinch"`
            /// </summary>
            /// <example>Millilitres</example>
            public string MeasurementUnit { get; init; } = MeasurementUnit;
            /// <summary>This is the a specific version of the ingredient from a supermarket</summary>
            public CreateProduct LinkingProduct { get; init; } = LinkingProduct;
        };

        /// <summary>Contains details on one part of how to make this recipe</summary>
        public record CreateInstruction(int Order, string Description)
        {
            /// <summary>Used to generate the order this instruction will appear. Lowest -> Highest</summary>
            /// <example>3</example>
            public int Order { get; init; } = Order;
            /// <summary>Describes what someone should be doing for this instruction</summary>
            /// <example>Blend all the ingredients together for at least 30 seconds.</example>
            public string Description { get; init; } = Description;
        };

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
                RuleFor(c => c.Servings).GreaterThan(0);
                RuleFor(c => c.DurationMinutes).GreaterThan(0);
                // Shouldn't be any identical `Order` values
                RuleFor(c => c.Instructions.DistinctBy(i => i.Order).Count())
                    .Equal(c => c.Instructions.Count)
                    .When(c => c.Instructions != null);
                RuleForEach(c => c.Ingredients)
                    .ChildRules(cr => {
                        cr.RuleFor(i => i.Name).NotEmpty();
                        cr.RuleFor(i => i.Quantity).GreaterThan(0);
                        cr.RuleFor(i => i.MeasurementUnit.ToEnum<MeasurementUnit>()).IsInEnum();
                        cr.RuleFor(i => i.LinkingProduct.Id).GreaterThan(0).When(i => i.LinkingProduct != null);
                        cr.RuleFor(i => i.LinkingProduct.SupermarketId).GreaterThan(0).When(i => i.LinkingProduct != null);
                        cr.RuleFor(i => i.LinkingProduct.Name).NotEmpty().When(i => i.LinkingProduct != null);
                    })
                    .When(c => c.Ingredients != null);
                RuleForEach(c => c.Instructions)
                    .ChildRules(cr =>
                    {
                        cr.RuleFor(i => i.Order).GreaterThanOrEqualTo(0);
                        cr.RuleFor(i => i.Description).NotEmpty();
                    })
                    .When(c => c.Instructions != null);
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<CreateProduct, Product>();
                CreateMap<CreateIngredient, Ingredient>()
                    .ForMember(
                        dest => dest.Unit,
                        opt => opt.MapFrom(src => src.MeasurementUnit.ToEnum<MeasurementUnit>()))
                    .ForMember(
                        dest => dest.Product,
                        opt => opt.MapFrom(src => src.LinkingProduct));
                CreateMap<CreateInstruction, Instruction>();
                CreateMap<Command, Recipe>();
            }
        }

        public class Handler : ICommandHandler<Command, int>
        {
            private readonly RecipeShopperContext _db;
            private readonly IMapper _mapper;

            public Handler(RecipeShopperContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<int> Handle(Command request, CancellationToken cancellationToken)
            {
                var recipe = _mapper.Map<Recipe>(request);

                await _db.Recipes.AddAsync(recipe);

                await _db.SaveChangesAsync();

                return recipe.Id;
            }
        }
    }
}
