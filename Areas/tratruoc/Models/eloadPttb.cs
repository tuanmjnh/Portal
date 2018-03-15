namespace Portal.Areas.tratruoc.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("eloadPttb")]
    public partial class eloadPttb
    {
        public Guid id { get; set; }

        public long? localID { get; set; }

        public long? stb { get; set; }

        [StringLength(255)]
        public string tentb { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ngaysinh { get; set; }

        public long? socmt { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ngaycap { get; set; }

        [StringLength(50)]
        public string noicap { get; set; }

        public string diachi { get; set; }

        [StringLength(50)]
        public string nguoidk { get; set; }

        public DateTime? ngaydk { get; set; }

        public DateTime? ngaysua { get; set; }

        public string diachidl { get; set; }

        [StringLength(50)]
        public string matinh { get; set; }

        public int? khuvuc { get; set; }

        public int? anhcmt { get; set; }

        [Column(TypeName = "ntext")]
        public string desc { get; set; }

        [StringLength(128)]
        public string createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        [StringLength(128)]
        public string updatedBy { get; set; }

        public DateTime? updatedAt { get; set; }

        public int? flag { get; set; }
    }
}
