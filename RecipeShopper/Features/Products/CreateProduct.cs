using FluentValidation;
using MediatR;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Products
{
    public class CreateProduct
    {
        public record Command(int Id, SupermarketType SupermarketType, string Name) : ICommand<Unit>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.Id).NotEmpty();
                RuleFor(m => m.Name).NotEmpty();
                RuleFor(m => m.SupermarketType).NotEmpty();
            }
        }

        public class Handler : ICommandHandler<Command, Unit>
        {
            private readonly RecipeShopperContext _db;

            public Handler(RecipeShopperContext db) => _db = db;

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = new Product
                {
                    Id = request.Id,
                    SupermarketId = (int)request.SupermarketType,
                    Name = request.Name
                };

                await _db.Products.AddAsync(product, cancellationToken);

                await _db.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}
