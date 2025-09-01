namespace PersonalFinanceTracker.DTO
{
    public class Categorize
    {
        public int CategoryId { get; set; }
        public string? DescriptionPattern { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
