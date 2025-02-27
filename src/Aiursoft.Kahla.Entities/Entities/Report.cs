using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Aiursoft.Kahla.Entities.Entities
{
    public class Report
    {
        public int Id { get; init; }

        [StringLength(64)]
        public string? TriggerId { get; init; }
        [ForeignKey(nameof(TriggerId))]
        [NotNull]
        public KahlaUser? Trigger { get; init; }

        [StringLength(64)]
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
