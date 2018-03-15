namespace Portal.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MainContext : DbContext
    {
        public MainContext() : base("name=MainContext") { }


        public virtual DbSet<bill_month> bill_month { get; set; }
        public virtual DbSet<collected_staff> collected_staff { get; set; }
        public virtual DbSet<customer_info> customer_info { get; set; }
        public virtual DbSet<group> groups { get; set; }
        public virtual DbSet<group_item> group_item { get; set; }
        public virtual DbSet<item> items { get; set; }
        public virtual DbSet<sub_item> sub_items { get; set; }
        public virtual DbSet<local> locals { get; set; }
        public virtual DbSet<setting> settings { get; set; }
        public virtual DbSet<staff> staffs { get; set; }
        public virtual DbSet<subscriber_growth> subscriber_growth { get; set; }
        public virtual DbSet<Authentication.user> users { get; set; }
        public virtual DbSet<FileManager> FileManagers { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<DVCNTT> DVCNTTs { get; set; }
        public virtual DbSet<cuocTraTienTruoc> cuocTraTienTruocs { get; set; }
        public virtual DbSet<ManagerHD> ManagerHD { get; set; }
        public virtual DbSet<Areas.tratruoc.Models.eloadPttb> eloadPttbs { get; set; }
        public virtual DbSet<Areas.tratruoc.Models.eloadUser> eloadUsers { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder) { }
    }
    public abstract class RepositoryBase<T> where T : class
    {
        DbSet _dbSet;
        DbContext _context;
        public void Update(T entity, params System.Linq.Expressions.Expression<Func<T, object>>[] properties)
        {
            _dbSet.Attach(entity);
            System.Data.Entity.Infrastructure.DbEntityEntry<T> entry = _context.Entry(entity);
            foreach (var selector in properties)
                entry.Property(selector).IsModified = true;
        }
    }
}
