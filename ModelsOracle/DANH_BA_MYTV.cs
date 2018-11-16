namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DANH_BA_MYTV")]
    public partial class DANH_BA_MYTV
    {
        public string LOGINNAME { get; set; }
        public string FULLNAME { get; set; }
        public string CUSTCATE { get; set; }
        public string ADDRESS1 { get; set; }
        public string MOBILE { get; set; }
        public string CONTRACTCD { get; set; }
        public DateTime SIGNDATE { get; set; }
        public int PACKAGE { get; set; }
        public DateTime REGISTDATE { get; set; }
        public string STATUS { get; set; }
        public string DISTRICT { get; set; }
        public string CARDID { get; set; }
        public DateTime L_SUS_DATE { get; set; }
        public string MA_DVI { get; set; }
        public string MA_CQ { get; set; }
        public int MA_DT { get; set; }
        public string MA_ST { get; set; }
        public int SOLUONGMAY { get; set; }
        public string PHUONG_ID { get; set; }
        public int NGAY_SUDUNG { get; set; }
        public string MA_CBT { get; set; }
        public string MA_TUYENTHU { get; set; }
        public string MA_TT_HNI { get; set; }
        public string MA_KH { get; set; }
        public int DATMOI_TRONGTHANG { get; set; }
        public DateTime NGAY_CHUYEN { get; set; }
    }
}
