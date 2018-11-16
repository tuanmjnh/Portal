namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ManagerHD")]
    public partial class ManagerHD
    {
        public Guid id { get; set; }

        [StringLength(128)]
        public string app_key { get; set; }

        public int localID { get; set; }

        [StringLength(256)]
        public string localName { get; set; }

        [StringLength(128)]
        public string contractID { get; set; }

        [StringLength(512)]
        public string customerName { get; set; }

        [StringLength(1024)]
        public string customerAddress { get; set; }

        [StringLength(50)]
        public string customerPhone { get; set; }

        [StringLength(256)]
        public string accounts { get; set; }

        [StringLength(2000)]
        public string attach { get; set; }

        [StringLength(256)]
        public string createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        [StringLength(256)]
        public string updatedBy { get; set; }

        public DateTime? updatedAt { get; set; }

        [StringLength(1000)]
        public string details { get; set; }

        [StringLength(1000)]
        public string cfmNotes { get; set; }

        [StringLength(256)]
        public string cfmBy { get; set; }

        public DateTime? cfmAt { get; set; }

        public int? accountNumber { get; set; }

        public int? flag { get; set; }
    }
}
