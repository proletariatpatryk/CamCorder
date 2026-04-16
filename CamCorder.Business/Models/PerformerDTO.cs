using System;

namespace CamCorder.Business.Models
{
    public class PerformerDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
    }
}
