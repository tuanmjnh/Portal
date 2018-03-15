namespace Portal.ModelsHNI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TB_FSECURE_BKN")]
    public partial class TB_FSECURE_BKN
    {
        public int? DICHVUVT_ID { get; set; }
        public int? HDTB_CHA_ID { get; set; }
        public int? THUEBAO_CHA_ID { get; set; }
        public int? CAIDAT { get; set; }
        public string ACCOUNT { get; set; }
        public string SO_MEN { get; set; }
        public int HDTB_ID { get; set; }
        public string SO_DIDONG { get; set; }
        public int? GOICUOC_ID { get; set; }
        public string NGUOI_BANGIAO { get; set; }
        public DateTime NGAY_BANGIAO { get; set; }
        public string NGUOI_XTTT_BANGIAO { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public int? BANGIAO { get; set; }
        public string REQUEST_ID { get; set; }
        public string MA_FSECURE { get; set; }
        public string KIEU_BD { get; set; }
    }
}
