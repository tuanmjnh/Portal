namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class customer
    {
        public Guid id { get; set; }

        [Required]
        public string name { get; set; }

        public string mobile { get; set; }

        [StringLength(255)]
        public string email { get; set; }

        public string address { get; set; }

        public string local { get; set; }

        public string mytv { get; set; }

        public string packet_id { get; set; }

        public string created_by { get; set; }

        public DateTime? created_at { get; set; }

        public string updated_by { get; set; }

        public DateTime? updated_at { get; set; }

        public int flag { get; set; }
    }
}
