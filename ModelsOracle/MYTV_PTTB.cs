namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MYTV_PTTB")]
    public partial class MYTV_PTTB
    {
        [Key]
        public string ID { get; set; }
        public string MYTV_ID { get; set; }
        public string FULLNAME { get; set; }
        public string CUSTCATE { get; set; }
        public string ADDRESS { get; set; }
        public string MOBILE { get; set; }
        public string TELEPHONE { get; set; }
        public string MA_DVI { get; set; }
        public string MA_CBT { get; set; }
        public string MA_TUYEN { get; set; }
        public string MA_KH { get; set; }
        public string MA_CQ { get; set; }
        public string MA_TT_HNI { get; set; }
        public int MA_DT { get; set; }
        public string MA_ST { get; set; }
        public int NGAY_SD { get; set; }
        public DateTime SIGNDATE { get; set; }
        public DateTime REGISTDATE { get; set; }
        public int GOICUOCID { get; set; }
        public int TH_THANG { get; set; }
        public int TH_SD { get; set; }
        public int TH_HUY { get; set; }
        public decimal CUOC_SD { get; set; }
        public decimal CUOC_TB { get; set; }
        public decimal TONG { get; set; }
        public decimal VAT { get; set; }
        public decimal TONGCONG { get; set; }
        public int DUPECOUNT { get; set; }
        public int STATUS { get; set; }
        public int ISDATMOI { get; set; }
        public int ISNULLDB { get; set; }
        public int ISNULLMT { get; set; }
        public int ISHUY { get; set; }
    }
}
