using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CamCorder.Data.Entities
{
    public class Performer : IEntity
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(255)]
        public required string Url { get; set; }

        #region Navigation Properties

        public ICollection<Recording> Recordings { get; set; } = [];

        #endregion
    }

    public class Recording : IEntity
    {
        [Key]
        public int Id { get; set; }

        public required int PerformerId { get; set; }

        #region Navigation Properties

        public Performer? Performer { get; set; } = null;

        #endregion
    }
}
