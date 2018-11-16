namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("group")]
    public partial class group
    {
        public Guid id { get; set; }

        [StringLength(255)]
        public string app_key { get; set; }

        [StringLength(255)]
        public string id_key { get; set; }

        [StringLength(255)]
        public string title { get; set; }

        public string parent_id { get; set; }

        public string parents_id { get; set; }

        public int? level { get; set; }

        public int? orders { get; set; }

        [Column(TypeName = "ntext")]
        public string desc { get; set; }

        [StringLength(128)]
        public string created_by { get; set; }

        public DateTime? created_at { get; set; }

        [StringLength(128)]
        public string updated_by { get; set; }

        public DateTime? updated_at { get; set; }

        public int? flag { get; set; }

        public string extras { get; set; }
    }
}
