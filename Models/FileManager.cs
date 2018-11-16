namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FileManager")]
    public partial class FileManager
    {
        public Guid ID { get; set; }

        public Guid? ParentID { get; set; }

        [StringLength(255)]
        public string Parent { get; set; }

        public string Root { get; set; }

        public string Subdirectory { get; set; }

        public int? Level { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string Extension { get; set; }

        [StringLength(50)]
        public string ExtensionIcon { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(50)]
        public string Attributes { get; set; }

        public int? AttributesEnum { get; set; }

        public long? Length { get; set; }

        public bool? IsReadOnly { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? CreationTimeUtc { get; set; }

        public DateTime? LastAccessTime { get; set; }

        public DateTime? LastAccessTimeUtc { get; set; }

        public DateTime? LastWriteTime { get; set; }

        public DateTime? LastWriteTimeUtc { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; }

        [StringLength(128)]
        public string LastAccessBy { get; set; }

        [StringLength(128)]
        public string LastWriteBy { get; set; }

        public bool? Exists { get; set; }

        public int? Flag { get; set; }

        public string Extras { get; set; }
    }
}
