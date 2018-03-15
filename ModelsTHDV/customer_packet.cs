namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class customer_packet
    {
        [Key]
        public long customer_packet_id { get; set; }

        public Guid customer_id { get; set; }

        public long packet_id { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? started_at { get; set; }

        public DateTime? end_at { get; set; }

        public int? flag { get; set; }
    }
}
