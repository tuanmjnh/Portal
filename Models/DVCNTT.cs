namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DVCNTT")]
    public partial class DVCNTT
    {
        public long id { get; set; }

        [StringLength(128)]
        public string app_key { get; set; }

        public long? customerID { get; set; }

        [StringLength(512)]
        public string groupID { get; set; }

        [StringLength(128)]
        public string extraID { get; set; }

        public decimal? price { get; set; }

        public decimal? priceExtra { get; set; }

        public decimal? total { get; set; }

        public decimal? vat { get; set; }

        [StringLength(256)]
        public string quantity { get; set; }

        [StringLength(256)]
        public string createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        [StringLength(256)]
        public string updatedBy { get; set; }

        public DateTime? updatedAt { get; set; }

        public int? flag { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
