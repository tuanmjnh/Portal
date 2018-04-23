namespace Portal.Areas.ND49.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ND49")]
    public partial class ND49
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid ID { get; set; }
        [StringLength(128)]
        public string STB { get; set; }
        public int SO_ANH { get; set; }
        public int TD_TKC { get; set; }
        public int TB_TD3THANG { get; set; }
        [StringLength(256)]
        public string BTS { get; set; }
        public int? MA_DVI { get; set; }
        [StringLength(128)]
        public string DVIQL { get; set; }
        [StringLength(128)]
        public string NVQL { get; set; }
        [StringLength(128)]
        public string CREATEDBY { get; set; }
        public DateTime CREATEDAT { get; set; }
        [StringLength(128)]
        public string UPDATEDBY { get; set; }
        public DateTime? UPDATEDAT { get; set; }
        public int FLAG { get; set; }
    }
    public partial class ND49Export:ND49
    {
        public string TEN_DVI { get; set; }
        public string TEN_NVQL { get; set; }
    }
}
