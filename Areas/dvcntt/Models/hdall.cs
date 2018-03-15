namespace Portal.Areas.dvcntt.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    public partial class hdall
    {
        public decimal Ma_dvi { get; set; }
        [StringLength(50)]
        public string ma_cq { get; set; }
        [StringLength(50)]
        public string acc_net { get; set; }
        [StringLength(50)]
        public string acc_tv { get; set; }
        [StringLength(50)]
        public string so_dd { get; set; }
        [StringLength(50)]
        public string so_cd { get; set; }
        [StringLength(100)]
        public string ten_tb { get; set; }
        [StringLength(100)]
        public string dia_chi { get; set; }
        [StringLength(50)]
        public string ma_tuyen { get; set; }
        [StringLength(15)]
        public string ma_st { get; set; }
        public decimal ma_dt { get; set; }
        public decimal ma_cbt { get; set; }
        public decimal tong_cd { get; set; }
        public decimal tong_dd { get; set; }
        public decimal tong_net { get; set; }
        public decimal tong_tv { get; set; }
        public decimal vat { get; set; }
        public decimal tong { get; set; }
        public decimal kthue { get; set; }
        public decimal gtru { get; set; }
        public decimal tongcong { get; set; }
        [StringLength(10)]
        public string Kieu { get; set; }
        public Decimal ghep { get; set; }
        public Decimal ma_in { get; set; }
        public Decimal flag { get; set; }
        public Decimal app_id { get; set; }
    }
}
