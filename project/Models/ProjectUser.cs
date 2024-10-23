namespace project.Models
{
    public class ProjectUser
    {
        public int UserId { get; set; }
        public int OrgId { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSurname { get; set; }
        public int PersonId { get; set; }
        public string Telephone { get; set; }
        public string OrgName { get; set; } 
        public string OrgEmail { get; set; } 
        public string OrgTelephone { get; set; }
        public bool IsActive { get; set; }
    }
}
