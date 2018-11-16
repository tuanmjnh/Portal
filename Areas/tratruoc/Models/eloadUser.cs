namespace Portal.Areas.tratruoc.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class eloadUser
    {
        public Guid id { get; set; }

        public long? localID { get; set; }

        public Guid department { get; set; }

        [StringLength(255)]
        public string eloadNumber { get; set; }

        [StringLength(255)]
        public string fullName { get; set; }

        [StringLength(128)]
        public string manager { get; set; }

        [StringLength(50)]
        public string eloadType { get; set; }

        public DateTime? startedAt { get; set; }

        public DateTime? endedAt { get; set; }

        [Column(TypeName = "ntext")]
        public string desc { get; set; }

        [StringLength(128)]
        public string createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        [StringLength(128)]
        public string updatedBy { get; set; }

        public DateTime? updatedAt { get; set; }

        public int? isCCBS { get; set; }

        public int? isLock { get; set; }

        public int? flag { get; set; }
    }
}
