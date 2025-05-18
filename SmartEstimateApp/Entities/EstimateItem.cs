namespace Entities
{
    public class EstimateItem
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string Category { get; set; }

        public int DisplayOrder { get; set; }

        public Estimate Estimate { get; set; }
    }
}
