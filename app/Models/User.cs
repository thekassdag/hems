using HMS.Models.Enums;

namespace HMS.Models
{
    public class User : BaseEntity
    {
        public long Id { get; set; }
        public string? IdNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public char Gender { get; set; }
        public UserRole Role { get; set; } = UserRole.Student;
        public bool IsActive { get; set; } = true;
    }
}
