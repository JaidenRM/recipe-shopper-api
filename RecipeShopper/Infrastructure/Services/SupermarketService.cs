using RecipeShopper.Application.Enums;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Domain;

namespace RecipeShopper.Infrastructure.Services
{
    public class SupermarketService
    {
        private readonly Dictionary<SupermarketType, ISupermarket> _availableSupermarketServicesDict = new Dictionary<SupermarketType, ISupermarket>();

        public SupermarketService(IEnumerable<ISupermarket> supermarketServices)
        {
            foreach(var service in supermarketServices)
            {
                _availableSupermarketServicesDict[service.GetSupermarketType()] = service;
            }
        }

        public async Task<List<Product>> Search(SupermarketType supermarketType, string term)
        {
            if (!_availableSupermarketServicesDict.ContainsKey(supermarketType))
                throw new NotImplementedException("This supermarket has not been setup");

            return await _availableSupermarketServicesDict[supermarketType].Search(term);
        }

        public async Task<Dictionary<SupermarketType, List<Product>>> SearchSome(SupermarketType[] supermarkets, string term)
        {
            var productsBySupermarket = new Dictionary<SupermarketType, List<Product>>();

            foreach (var supermarket in supermarkets)
            {
                productsBySupermarket[supermarket] = await Search(supermarket, term);
            }

            return productsBySupermarket;
        }

        public async Task<Dictionary<SupermarketType, List<Product>>> SearchAll(string term)
        {
            var productsBySupermarket = new Dictionary<SupermarketType, List<Product>>();

            foreach(var (supermarket, service) in _availableSupermarketServicesDict)
            {
                productsBySupermarket[supermarket] = await service.Search(term);
            }

            return productsBySupermarket;
        }
    }
}
