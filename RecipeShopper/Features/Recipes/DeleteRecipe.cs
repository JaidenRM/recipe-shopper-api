using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;

namespace RecipeShopper.Features.Recipes
{
    public class DeleteRecipe
    {
        public record Command(int Id) : ICommand<Unit>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id).NotEmpty().GreaterThanOrEqualTo(0);
            }
        }

        public class Handler : ICommandHandler<Command, Unit>
        {
            private readonly RecipeShopperContext _db;

            public Handler(RecipeShopperContext db) => _db = db;

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var toDelete = _db.Recipes
                    .Include(r => r.Ingredients).ThenInclude(i => i.Product)
                    .Include(r => r.Instructions)
                    .SingleOrDefault(r => r.Id == request.Id);

                if (toDelete != null)
                {
                    _db.RemoveRange(toDelete.Ingredients.Where(i => i.Product != null).Select(i => i.Product));
                    _db.Remove(toDelete);

                    await _db.SaveChangesAsync();
                }

                return Unit.Value;
            }
        }
    }
}
