namespace RecipeShopper.Entities
{
    public class Recipe : IEntity, IUpdateable
    {
        public int Id { get; set; }
        public int Servings { get; set; }
        public int DurationMinutes { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime LastModifiedUTC { get; set; }

        public List<Ingredient> Ingredients { get; set; }
        public List<Instruction> Instructions { get; set; }
    }
}
