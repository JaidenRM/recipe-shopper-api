namespace RecipeShopper.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public int Quantity { get; set; }
        public decimal FullPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        //public bool IsOnSale { get; set; }

    }
}
