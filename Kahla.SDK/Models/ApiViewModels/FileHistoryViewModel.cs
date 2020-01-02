using Aiursoft.SDK.Models.Probe;
using Aiursoft.XelNaga.Models;
using System;
using System.Collections.Generic;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class FileHistoryViewModel : AiurCollection<Folder>
    {
        [Obsolete("This method is only for framework", true)]
        public FileHistoryViewModel() { }

        public FileHistoryViewModel(List<Folder> folders) : base(folders) { }

        public string SiteName { get; set; }
        public string RootPath { get; set; }
    }
}
