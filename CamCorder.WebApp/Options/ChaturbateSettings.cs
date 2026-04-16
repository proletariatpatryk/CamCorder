using System.ComponentModel.DataAnnotations;

namespace CamCorder.WebApp.Options;

public class ChaturbateSettings
{
    [Range(1, 100)]
    [Display(Name = "Max concurrent requests")]
    public int MaxConcurrentRequests { get; set; } = 5;
}
