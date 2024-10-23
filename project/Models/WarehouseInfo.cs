namespace project.Models
{
    public class WarehouseInfo
    {
        public string WarehouseName { get; set; }
        public int TotalQuantity { get; set; }
        public int FreeSeats { get; set; }
        public string? Location { get; set; }
        public int UserId { get; set; }
    }
}
