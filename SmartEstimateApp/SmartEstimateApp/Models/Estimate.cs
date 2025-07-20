namespace SmartEstimateApp.Models
{
    public class Estimate
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ValidUntil { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountRate { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        public int ProjectId { get; set; }

        public List<EstimateItem> Items { get; set; } = new List<EstimateItem>();
    }
}
