namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("item")]
    public partial class item
    {
        public Guid id { get; set; }

        [StringLength(255)]
        public string app_key { get; set; }

        [StringLength(255)]
        public string id_key { get; set; }

        [StringLength(255)]
        public string code_key { get; set; }

        [StringLength(255)]
        public string title { get; set; }

        [Column(TypeName = "ntext")]
        public string desc { get; set; }

        public long? quantity { get; set; }

        public long? quantity_total { get; set; }

        public decimal? price_old { get; set; }

        public decimal? price { get; set; }

        public string images { get; set; }

        public string url { get; set; }

        [StringLength(255)]
        public string author { get; set; }

        public string attach { get; set; }

        public int? orders { get; set; }

        public DateTime? started_at { get; set; }

        public DateTime? ended_at { get; set; }

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
