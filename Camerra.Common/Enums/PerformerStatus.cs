using System.ComponentModel.DataAnnotations;

namespace CamCorder.Common.Enums
{
    public enum RoomStatus
    {
        [Display(Name = "Online")]
        Online,

        [Display(Name = "Offline")]
        Offline,

        [Display(Name = "Private")]
        Private,

        [Display(Name = "Away")]
        Away,

        [Display(Name = "Unknown")]
        Unknown
    }
    
}
