namespace JRD_Projects.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // Matches column: hashed_password
        public string Hashed_Password { get; set; } = string.Empty;

        // Matches column: created_at
        public DateTime Created_At { get; set; }
    }
}
