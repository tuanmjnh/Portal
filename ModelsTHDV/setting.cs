namespace Portal.ModelsTHDV
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class setting
    {
        [Key]
        [StringLength(250)]
        public string app_key { get; set; }

        public string sub_key { get; set; }

        public string value { get; set; }

        public string sub_value { get; set; }

        public string extras { get; set; }
    }
}
