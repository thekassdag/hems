using System;
using System.ComponentModel.DataAnnotations;
using HMS.Models.Enums;

namespace HMS.Models
{
    public class LabMachine : BaseEntity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? LabCode { get; set; }

        [StringLength(100)]
        public string? MachineName { get; set; }

        [Required]
        [StringLength(255)] // Assuming deviceId can be a long string from FingerprintJS
        public string? DeviceId { get; set; }

        public LabMachineStatus Status { get; set; } = LabMachineStatus.Inactive;

        public DateTime? LastUsedAt { get; set; }
    }
}
