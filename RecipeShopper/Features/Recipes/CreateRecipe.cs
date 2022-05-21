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
        public record Command(string Name, string Description, int Servings, int DurationMinutes, string Tags, List<CreateIngredient> Ingredients, List<CreateInstruction> Instructions) : ICommand<int>;
        public record CreateProduct(int Id, int SupermarketId, string Name);
        public record CreateIngredient(string Name, decimal Quantity, string MeasurementUnit, CreateProduct LinkingProduct);
        public record CreateInstruction(int Order, string Description);

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
