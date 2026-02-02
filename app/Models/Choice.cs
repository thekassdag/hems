using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Choice : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        public long QuestionId { get; set; } // Foreign key to Question

        [Required]
        public string? ChoiceText { get; set; } // Choice content

        public bool IsCorrect { get; set; } = false; // Marks if this is the correct answer

        // Navigation property
        public Question? Question { get; set; }
    }
}
