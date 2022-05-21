namespace RecipeShopper.Entities
{
    public interface IUpdateable
    {
        public DateTime CreatedOnUTC { get; set; }
        public DateTime LastModifiedUTC { get; set; }
    }
}
