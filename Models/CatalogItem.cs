namespace FlowMediaWebMVC.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string FilterKey { get; set; }
        public double Price { get; set; }
    }
}