using RecipeShopper.Domain.Enums;

namespace RecipeShopper.Entities
{
    public class Ingredient : IEntity
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int? ProductId { get; set; }
        public int? SupermarketId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public MeasurementUnit Unit { get; set; }

        public Recipe Recipe { get; set; }
        public Product Product { get; set; }
        public Supermarket Supermarket { get; set; }
    }
}
