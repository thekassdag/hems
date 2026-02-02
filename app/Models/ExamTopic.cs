using System;
using System.Collections.Generic; // Added for ICollection
using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class ExamTopic : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        public long ExamId { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total questions must be at least 1.")]
        public int TotalQuestions { get; set; }

        [Required]
        public string UserId { get; set; } // ID of the instructor who assigned to the exam

        [Required]
        public DateTime Deadline { get; set; } // Timestamp for submission deadline

        // Navigation properties
        public Exam? Exam { get; set; }
        public Topic? Topic { get; set; }
        public User? User { get; set; } // Instructor

        public ICollection<Question> Questions { get; set; } = new List<Question>(); // Collection of questions for this exam topic
    }
}
