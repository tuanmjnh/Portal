namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cuocTraTienTruoc")]
    public partial class cuocTraTienTruoc
    {
        public long id { get; set; }

        [StringLength(256)]
        public string account { get; set; }

        public int thangNop { get; set; }

        public int? thangThem { get; set; }

        public decimal tien { get; set; }

        public decimal tientruthang { get; set; }

        public DateTime thang_dk { get; set; }

        public DateTime? thang_bd { get; set; }

        public DateTime? thang_kt { get; set; }

        [StringLength(512)]
        public string ghiChu { get; set; }

        public int type { get; set; }

        public int flag { get; set; }
    }
}
