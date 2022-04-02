using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Entities;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace RecipeShopper.Features.Products
{
    public class GetProducts
    {
        //1. Query/Command - All the data we need to execute
        // Using record instead of class for ease, value-equality and immutability
        public record Query(int[] Ids) : IQuery<Response>;
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice);

        public class MappingProfile : Profile 
        {
            public MappingProfile()
            {
                CreateProjection<Product, Domain.Product>();
                CreateMap<Domain.Product, Model>();
            }
        }

        //2. Handler - Handle the data access stuff
        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            private readonly IConfigurationProvider _config;
            private readonly IMapper _mapper;

            public Handler(RecipeShopperContext dbContext, IConfigurationProvider config, IMapper mapper) {
                _db = dbContext;
                _config = config;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Product> products = _db.Products;

                if (request.Ids != null && request.Ids.Any())
                    products = products.Where(pro => request.Ids.Contains(pro.Id));

                var domainResults = await products
                    .ProjectTo<Domain.Product>(_config)
                    .ToListAsync();

                var results = _mapper
                    .Map<List<Domain.Product>, List<Model>>(domainResults)
                    .ToList();

                //3. Domain - If data is needed, convert to domain model which will handle all the behavioural stuff
                return new Response(results);
            }
        }

        //4. Response - The data we want to return
        public record Response(List<Model> Results);
    }
}
