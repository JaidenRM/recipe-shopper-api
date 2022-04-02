using FluentValidation;
using MediatR;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Products
{
    public class CreateProduct
    {
        public record Command(int Id, string Name, decimal FullPrice, decimal CurrentPrice) : ICommand<Unit>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.Id).NotEmpty();
                RuleFor(m => m.Name).NotEmpty();
                RuleFor(m => m.FullPrice).GreaterThan(0);
                RuleFor(m => m.CurrentPrice).GreaterThan(0).LessThanOrEqualTo(m => m.FullPrice);
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
                    Name = request.Name,
                    FullPrice = request.FullPrice,
                    CurrentPrice = request.CurrentPrice
                };

                await _db.Products.AddAsync(product, cancellationToken);

                await _db.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}
