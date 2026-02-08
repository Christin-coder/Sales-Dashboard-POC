namespace SalesAPI.DTOs
{
    public class SaleDTO
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Total {get; set; }
    
    }
}