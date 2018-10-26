using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models
{
    public class FileRecord
    {
        public int Id { get; set; }
        public int FileKey { get; set; }
        public string SourceName { get; set; }
        public DateTime UploadTime { get; set; } = DateTime.UtcNow;

        public string UploaderId { get; set; }
        [ForeignKey(nameof(UploaderId))]
        public KahlaUser Uploader { get; set; }
    }
}
