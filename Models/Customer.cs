namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Customer")]
    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Customer()
        {
            DVCNTTs = new HashSet<DVCNTT>();
        }

        public long id { get; set; }

        [StringLength(256)]
        public string app_key { get; set; }

        [StringLength(256)]
        public string id_key { get; set; }

        [StringLength(256)]
        public string name { get; set; }

        [StringLength(256)]
        public string author { get; set; }

        [StringLength(256)]
        public string code { get; set; }

        [StringLength(1000)]
        public string phone { get; set; }

        [StringLength(256)]
        public string email { get; set; }

        [StringLength(1000)]
        public string address { get; set; }

        [StringLength(1000)]
        public string details { get; set; }

        [StringLength(256)]
        public string createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        [StringLength(256)]
        public string updatedBy { get; set; }

        public DateTime? updatedAt { get; set; }

        public int? flag { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DVCNTT> DVCNTTs { get; set; }
    }
}
