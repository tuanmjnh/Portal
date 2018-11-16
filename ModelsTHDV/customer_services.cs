namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class customer_services
    {
        [Key]
        public Guid customer_services_id { get; set; }

        [Required]
        [StringLength(255)]
        public string app_key { get; set; }

        public Guid? customer_id { get; set; }

        public Guid? services_id { get; set; }

        public string extras { get; set; }
    }
}
