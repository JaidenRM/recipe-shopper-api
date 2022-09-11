namespace RecipeShopper.Infrastructure.Supermarkets.Woolworths
{
    public class WoolworthsProduct
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? WasPrice { get; set; }
        public decimal? SavingsAmount { get; set; }
        public BundleSpecial CentreTag { get; set; }
        public bool IsInStock { get; set; }
        public bool IsPurchasable { get; set; }
        public bool IsBundle { get; set; }
        public bool IsOnSpecial { get; set; }
        public string Unit { get; set; }
        public string SmallImageFile { get; set; }
        public string MediumImageFile { get; set; }
        public string LargeImageFile { get; set; }
        // Is ID
        public int Stockcode { get; set; }
    }

    // JSON Name is "CentreTag"
    public class BundleSpecial
    {
        public MultibuyData multibuyData { get; set; }
    }

    public class MultibuyData
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // better name would be something like PricePerUnitTag ??
        public string CupTag { get; set; }
    }
}
