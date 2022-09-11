namespace RecipeShopper.Domain
{
    public record ImageSet(string Small, string Medium, string Large);
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public decimal FullPrice { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public Supermarket Supermarket { get; private set; }
        public ImageSet ImageUrls { get; private set; }

        public Product(int id, string name, decimal fullPrice, decimal currentPrice, Supermarket supermarket, ImageSet imageUrls)
        {
            Id = id;
            Name = name;
            FullPrice = fullPrice;
            CurrentPrice = currentPrice;
            Supermarket = supermarket;
            ImageUrls = imageUrls;
        }
    }
}
