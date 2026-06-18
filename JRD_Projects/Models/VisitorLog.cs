namespace JRD_Projects.Models
{
    public class VisitorLog
    {
        public int Id { get; set; }
        public DateTime? Timestamp { get; set; }
        //public string? Ip { get; set; }
        //public string? UserAgent { get; set; } 
        public string? Location { get; set; }
    }
}
