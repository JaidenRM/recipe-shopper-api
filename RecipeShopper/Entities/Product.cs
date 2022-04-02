namespace RecipeShopper.Entities
{
    public class Product : IEntity
    {
        //TODO: Change to Guid for better security against delete etc?
        public int Id { get; set; }
        public string Name { get; set; }
        //public int Quantity { get; set; }
        public decimal FullPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        //public bool IsOnSale { get; set; }

    }
}
