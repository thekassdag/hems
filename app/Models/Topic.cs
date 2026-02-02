using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Topic : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
