namespace RecipeShopper.Entities
{
    public class Supermarket : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }
}
