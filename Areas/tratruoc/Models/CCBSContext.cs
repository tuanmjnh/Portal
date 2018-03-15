namespace Portal.Areas.tratruoc.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CCBSContext : DbContext
    {
        public CCBSContext()
            : base("name=MainContext")
        {
        }

        public virtual DbSet<eloadPttb> eloadPttbs { get; set; }
        public virtual DbSet<eloadUser> eloadUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
    }
}
