﻿using AutoMapper;
using FluentValidation;
using MediatR;
using RecipeShopper.Application.Exceptions;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Products
{
    public class UpdateProduct
    {

        public record Command(int Id, string Name, decimal FullPrice, decimal CurrentPrice) : ICommand<Unit>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.Id).GreaterThan(0);
                RuleFor(m => m.Name).NotEmpty();
                RuleFor(m => m.FullPrice).GreaterThanOrEqualTo(0);
                RuleFor(m => m.CurrentPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(m => m.FullPrice);
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Command, Product>();
        }

        public class Handler : ICommandHandler<Command, Unit>
        {
            private readonly RecipeShopperContext _db;
            private readonly IMapper _mapper;

            public Handler(RecipeShopperContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var toUpdate = await _db.Products.FindAsync(request.Id);

                if (toUpdate == null) throw new RecordNotFoundException("Could not find the record to update");

                toUpdate = _mapper.Map<Product>(request);
                await _db.SaveChangesAsync();

                return default;
            }
        }
    }
}