using System;
using System.ComponentModel.DataAnnotations;
using HMS.Models.Enums; // Assuming UserRole is here for UserId reference

namespace HMS.Models
{
    public class Exam : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; } // text type in DB, so no StringLength here

        public long ExamCode { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute.")]
        public int DurationMinutes { get; set; }

        public DateTime? StartTime { get; set; } // Nullable timestamp

        [Required]
        public long UserId { get; set; } // Exam coordinator ID

        public bool IsActive { get; set; } = false; // Whether exam is visible to students

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Batch must be a positive number.")]
        public int Batch { get; set; }

        // Navigation property
        public User? User { get; set; }
        public ICollection<ExamTopic>? ExamTopics { get; set; }
    }
}
