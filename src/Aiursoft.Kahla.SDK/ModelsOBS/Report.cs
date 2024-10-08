﻿using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS
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
        public KahlaUser? Trigger { get; set; }

        public string? TargetId { get; set; }
        public KahlaUser? Target { get; set; }

        public string? Reason { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime ReportTime { get; set; } = DateTime.UtcNow;
    }
}
