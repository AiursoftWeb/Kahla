﻿namespace Kahla.Home.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public string AppLatestVersion { get; set; }
        public string CLILatestVersion { get; set; }
        public string DownloadRoot { get; internal set; }
        public string CliDownloadRoot { get; internal set; }
        public string SDKLatestVersion { get; set; }
        public string KahlaWebPath { get; set; }
    }
}
