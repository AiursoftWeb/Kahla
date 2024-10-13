﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Aiursoft.Kahla.SDK.Models
{
    public enum ReportStatus
    {
        Pending = 0,
        Resolved = 1
    }
    public class Report
    {
        public int Id { get; set; }

        public string? TriggerId { get; set; }
        [ForeignKey(nameof(TriggerId))]
        [NotNull]
        public KahlaUser? Trigger { get; set; }

        public string? TargetId { get; set; }
        [ForeignKey(nameof(TargetId))]
        [NotNull]
        public KahlaUser? Target { get; set; }

        public string? Reason { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime ReportTime { get; set; } = DateTime.UtcNow;
    }
}