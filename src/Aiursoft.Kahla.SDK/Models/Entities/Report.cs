using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Aiursoft.Kahla.SDK.Models.Entities
{
    public enum ReportStatus
    {
        Pending = 0,
        Resolved = 1 // TODO: Allow admin resolve report.
    }
    
    public class Report
    {
        public int Id { get; init; }

        public string? TriggerId { get; init; }
        [ForeignKey(nameof(TriggerId))]
        [NotNull]
        public KahlaUser? Trigger { get; init; }

        public string? TargetId { get; init; }
        [ForeignKey(nameof(TargetId))]
        [NotNull]
        public KahlaUser? Target { get; init; }

        [StringLength(400)]
        public string? Reason { get; init; }
        public ReportStatus Status { get; init; } = ReportStatus.Pending;
        public DateTime ReportTime { get; init; } = DateTime.UtcNow;
    }
}
