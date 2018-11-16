namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class sub_item
    {
        public Guid id { get; set; }

        [StringLength(128)]
        public string app_key { get; set; }

        [StringLength(255)]
        public string id_key { get; set; }

        public Guid? item_id { get; set; }

        [StringLength(255)]
        public string main_key { get; set; }

        [StringLength(255)]
        public string value { get; set; }

        [StringLength(255)]
        public string sub_value { get; set; }

        public string images { get; set; }

        public long? quantity { get; set; }

        public long? quantity_total { get; set; }

        public decimal? price_old { get; set; }

        public decimal? price { get; set; }

        [Column(TypeName = "ntext")]
        public string desc { get; set; }

        public DateTime? started_at { get; set; }

        public DateTime? ended_at { get; set; }

        public int? orders { get; set; }

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
