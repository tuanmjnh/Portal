namespace Portal.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ContextORCHNIVNPTBACKAN1 : DbContext
    {
        public ContextORCHNIVNPTBACKAN1() : base("name=ORCHNIVNPTBACKAN1") { }


        //public virtual DbSet<Models.TABLE1> TABLE1 { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema("VNPTBACKAN1");
        }
    }
}
