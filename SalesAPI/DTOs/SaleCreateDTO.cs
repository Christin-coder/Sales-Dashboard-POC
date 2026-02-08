using System.ComponentModel.DataAnnotations;

namespace SalesAPI.DTOs
{
    public class SaleCreateDTO
    {
        [Required]
        public int ProductID { get; set; }
        [Required]
        public int CustomerID { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}