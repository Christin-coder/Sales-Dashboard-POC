using System.ComponentModel.DataAnnotations.Schema;

namespace SalesAPI.Models
{
    public class Sale
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public int Quantity { get; set; } 
        public int CustomerID { get; set; }
        public int ProductID { get; set; }

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}