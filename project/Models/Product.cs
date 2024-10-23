namespace project.Models
{
    public class Product
    {
        public int ProductId { get; set; } 
        public string? ProductName { get; set; }
        public int quantity { get; set; }
        public string? barcode { get; set; }
        public int userId { get; set; }
        public string? Role { get; set; }
        public string? WarehouseName { get; set; }
        public string? location { get; set; }

    }
}
