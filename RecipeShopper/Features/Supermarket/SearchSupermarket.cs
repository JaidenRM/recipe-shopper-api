using AutoMapper;
using RecipeShopper.Application.Enums;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Domain;
using RecipeShopper.Infrastructure.Services;

namespace RecipeShopper.Features.Supermarket
{
    public class SearchSupermarket
    {
        public record Query(string SearchTerm, SupermarketType[] Supermarkets) : IQuery<Response>;
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice);

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Product, Model>();
        }

        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly SupermarketService _supermarketService;
            private readonly IMapper _mapper;

            public Handler(SupermarketService supermarketService, IMapper mapper)
            {
                _supermarketService = supermarketService;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var productsBySupermarket = new Dictionary<SupermarketType, List<Model>>();
                var results = await _supermarketService.SearchSome(request.Supermarkets, request.SearchTerm);

                foreach(var (supermarket, products) in results)
                {
                    productsBySupermarket[supermarket] = _mapper.Map<List<Model>>(products);
                }

                return new Response(productsBySupermarket);
            }
        } 

        public record Response(Dictionary<SupermarketType, List<Model>> ProductsBySupermarketDict);
    }
}
