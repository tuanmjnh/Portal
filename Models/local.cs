namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("local")]
    public partial class local
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        [StringLength(50)]
        public string key_id { get; set; }

        public long? parent_id { get; set; }

        public string parents_id { get; set; }

        public int? total_item { get; set; }

        [StringLength(50)]
        public string key_name { get; set; }

        [StringLength(150)]
        public string title { get; set; }

        [StringLength(128)]
        public string created_by { get; set; }

        public DateTime? created_at { get; set; }

        [StringLength(128)]
        public string updated_by { get; set; }

        public DateTime? updated_at { get; set; }

        public int? flag { get; set; }
    }
}
