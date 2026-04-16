using System.ComponentModel.DataAnnotations;

namespace CamCorder.WebApp.Options;

public class CamCorderOptions
{
    public const string SectionName = "CamCorder";

    [Required]
    [Display(Name = "Recordings path")]
    public string RecordingsPath { get; set; } = "App_Data/Recordings";

    [Range(1, 86400)]
    [Display(Name = "Polling interval (seconds)")]
    public int PollingIntervalSeconds { get; set; } = 15;

    [Display(Name = "Download enabled")]
    public bool DownloadEnabled { get; set; } = true;

    [Display(Name = "Max concurrent recordings")]
    public int MaxConcurrentRecordings { get; set; } = 0;

    public ChaturbateSettings ChaturbateSettings { get; set; } = new();
}
