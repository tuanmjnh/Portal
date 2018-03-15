namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TABLE1")]
    public partial class TABLE1
    {
        public int ID { get; set; }

        public string NAME { get; set; }
    }
}
