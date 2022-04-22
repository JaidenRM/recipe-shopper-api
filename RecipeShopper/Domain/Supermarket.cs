using RecipeShopper.Domain.Enums;

namespace RecipeShopper.Domain
{
    public class Supermarket
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Supermarket(SupermarketType id, string name)
        {
            Id = (int)id;
            Name = name;
        }
    }
}
