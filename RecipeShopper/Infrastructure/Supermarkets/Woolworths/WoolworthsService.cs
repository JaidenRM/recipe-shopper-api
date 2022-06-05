using AutoMapper;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Domain;

namespace RecipeShopper.Infrastructure.Supermarkets.Woolworths
{
    public class WoolworthsService : ISupermarketService
    {
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        private readonly string _searchUrl = "https://www.woolworths.com.au/apis/ui/Search/products?searchTerm=";

        public WoolworthsService(HttpClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<List<Product>> Search(string term)
        {
            var resp = await _client.GetAsync(_searchUrl + term);
            resp.EnsureSuccessStatusCode();

            var jsonResp = await resp.Content.ReadFromJsonAsync<WoolworthsResponse>();

            return jsonResp
                .Products?
                .SelectMany(p => p.Products
                    .Select(p => _mapper.Map<Product>(p)))
                .ToList();
        }

        public SupermarketType GetSupermarketType() => SupermarketType.Woolworths;
    }
}
