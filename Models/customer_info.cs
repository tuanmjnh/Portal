namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class customer_info
    {
        public Guid id { get; set; }

        [Required]
        [StringLength(255)]
        public string ma_cq { get; set; }

        public int? ma_dvi { get; set; }

        [StringLength(255)]
        public string acc_net { get; set; }

        [StringLength(255)]
        public string acc_tv { get; set; }

        [StringLength(255)]
        public string so_dd { get; set; }

        [StringLength(255)]
        public string so_cd { get; set; }

        [StringLength(255)]
        public string ten_tb { get; set; }

        public string dia_chi { get; set; }

        [StringLength(150)]
        public string ma_tuyen { get; set; }

        [StringLength(150)]
        public string ma_st { get; set; }

        public int? ma_dt { get; set; }

        public long? ma_cbt { get; set; }

        public string kieu { get; set; }

        public int? flag { get; set; }

        [Column(TypeName = "date")]
        public DateTime? created_at { get; set; }
    }
}
