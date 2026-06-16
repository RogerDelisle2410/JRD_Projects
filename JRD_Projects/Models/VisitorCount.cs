namespace JRD_Projects.Models
{
    public class VisitorCount
    {
        public int Id { get; set; }

        // Matches your "count" column
        public int count { get; set; }

        // Matches your "today_visits" column
        public int today_visits { get; set; }

        public DateTime? last_visit { get; set; }
        public string? last_ip { get; set; }
        public string? last_user_agent { get; set; }
    }
}
