namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DIA_CHI")]
    public partial class DIA_CHI
    {
        public int ID { get; set; }
        public int DUONGPHO_ID { get; set; }
        public int MAPHO_ID { get; set; }
        public int PHUONGXA_ID { get; set; }
        public int QUANHUYEN_ID { get; set; }
        public string TEN_DUONGPHO { get; set; }
        public string DAUTEN { get; set; }
        public string TEN_PHUONGXA { get; set; }
        public string TEN_QUANHUYEN { get; set; }
    }
}
