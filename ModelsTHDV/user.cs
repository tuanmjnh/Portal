namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class user
    {
        public Guid id { get; set; }

        [StringLength(255)]
        public string username { get; set; }

        [StringLength(255)]
        public string password { get; set; }

        [StringLength(36)]
        public string salt { get; set; }

        [StringLength(255)]
        public string full_name { get; set; }

        [Column(TypeName = "text")]
        public string address { get; set; }

        [StringLength(255)]
        public string roles { get; set; }

        public int? flag { get; set; }
    }
}
