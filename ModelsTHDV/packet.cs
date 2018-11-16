namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class packet
    {
        public long id { get; set; }

        [Required]
        public string name { get; set; }

        public decimal price_total { get; set; }

        public decimal price_vnn { get; set; }

        public decimal price_mytv { get; set; }

        public string services { get; set; }

        [Column(TypeName = "text")]
        public string description { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? started_at { get; set; }

        public DateTime? end_at { get; set; }

        public int flag { get; set; }
    }
}
