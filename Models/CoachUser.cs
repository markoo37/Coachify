namespace CoachCRM.Models
{
    public class CoachUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
    }
}