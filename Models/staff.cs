namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("staff")]
    public partial class staff
    {
        public Guid id { get; set; }

        public Guid? user_id { get; set; }

        [StringLength(255)]
        public string title { get; set; }
    }
}
