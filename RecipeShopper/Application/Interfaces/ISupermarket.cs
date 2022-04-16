using RecipeShopper.Application.Enums;
using RecipeShopper.Domain;

namespace RecipeShopper.Application.Interfaces
{
    public interface ISupermarket
    {
        Task<List<Product>> Search(string term);
        SupermarketType GetSupermarketType();
    }
}
