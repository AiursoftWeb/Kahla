using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public enum ReportStatus : int
    {
        Pending = 0,
        Resolved = 1
    }
    public class Report
    {
        public int Id { get; set; }

        public string TriggerId { get; set; }
        public KahlaUser Trigger { get; set; }

        public string TargetId { get; set; }
        public KahlaUser Target { get; set; }

        public string Reason { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
    }
}
