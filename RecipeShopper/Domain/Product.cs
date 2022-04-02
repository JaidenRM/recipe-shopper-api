using RecipeShopper.Domain.Helpers;

namespace RecipeShopper.Domain
{
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public decimal FullPrice { get; private set; }
        public decimal CurrentPrice { get; private set; }

        public Product(int id, string name, decimal fullPrice, decimal currentPrice)
        {
            Id = id;
            Name = name;
            FullPrice = fullPrice;
            CurrentPrice = currentPrice;
        }
    }
}
