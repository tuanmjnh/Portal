namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class bill_month
    {
        public Guid id { get; set; }

        public Guid? customer_id { get; set; }

        [StringLength(255)]
        public string ma_cq { get; set; }

        public int? ma_dvi { get; set; }

        [StringLength(150)]
        public string ma_tuyen { get; set; }

        public long? ma_cbt { get; set; }

        public int? ma_in { get; set; }

        public int? kieu_hd { get; set; }

        public decimal? tong_cd { get; set; }

        public decimal? tong_dd { get; set; }

        public decimal? tong_net { get; set; }

        public decimal? tong_tv { get; set; }

        public decimal? vat { get; set; }

        public decimal? tong { get; set; }

        public decimal? tongcong { get; set; }

        public int? flag { get; set; }

        [Column(TypeName = "date")]
        public DateTime? created_at { get; set; }
    }
}
