using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Products
{
    public class DeleteProduct
    {
        public record Command(int Id) : ICommand<Unit>;

        public class Validation : AbstractValidator<Command>
        {
            public Validation()
            {
                RuleFor(m => m.Id).NotEmpty();
                RuleFor(m => m.Id).GreaterThan(0);
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Product, Command>();
        }

        public class CommandHandler : ICommandHandler<Command, Unit>
        {
            private readonly RecipeShopperContext _db;

            public CommandHandler(RecipeShopperContext db) => _db = db;
            

            public async Task<Unit> Handle(Command message, CancellationToken token)
            {
                var toDelete = await _db.Products.FindAsync(message.Id);
                
                if (toDelete != null) _db.Products.Remove(toDelete);

                return default;
            }
        }
    }
}
