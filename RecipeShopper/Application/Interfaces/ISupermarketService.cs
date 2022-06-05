using RecipeShopper.Domain.Enums;
using RecipeShopper.Domain;

namespace RecipeShopper.Application.Interfaces
{
    public interface ISupermarketService
    {
        Task<List<Product>> Search(string term);
        SupermarketType GetSupermarketType();
    }
}
