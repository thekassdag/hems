using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Question : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        public long ExamId { get; set; }

        [Required]
        public long ExamTopicId { get; set; } // Foreign key to ExamTopic

        [Required]
        public string? Content { get; set; } // Question content

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Marks must be at least 1.")]
        public int Marks { get; set; }

        // Navigation properties
        public Exam? Exam { get; set; }
        public ExamTopic? ExamTopic { get; set; }
        public ICollection<Choice>? Choices { get; set; }
    }
}
