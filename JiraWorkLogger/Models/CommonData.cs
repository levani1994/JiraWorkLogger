namespace ScrinToJiraSync.Models
{
    public class CommonData
    {
        public List<Company> Companies { get; set; }
        public long EmploymentId { get; set; }
    }

    public class Company
    {
        public List<Employment> Employments { get; set; }
    }

    public class Employment
    {
        public long Id { get; set; }
    }
}
