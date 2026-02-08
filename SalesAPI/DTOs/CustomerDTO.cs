namespace SalesAPI.DTOs
{
    public class CustomerDTO
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}