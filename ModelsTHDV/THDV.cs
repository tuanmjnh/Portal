namespace Portal.ModelsTHDV
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class THDV : DbContext
    {
        public THDV()
            : base("name=THDV")
        {
        }

        public virtual DbSet<customer_packet> customer_packet { get; set; }
        public virtual DbSet<customer_services> customer_services { get; set; }
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<packet> packets { get; set; }
        public virtual DbSet<setting> settings { get; set; }
        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<packet>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.salt)
                .IsFixedLength();

            modelBuilder.Entity<user>()
                .Property(e => e.address)
                .IsUnicode(false);
        }
    }
}
