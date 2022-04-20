namespace RecipeShopper.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int SupermarketId { get; set; }
        public Supermarket Supermarket { get; set; }
    }
}
