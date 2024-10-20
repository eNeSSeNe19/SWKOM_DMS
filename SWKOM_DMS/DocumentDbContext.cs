using Microsoft.EntityFrameworkCore;
using SWKOM_DMS.Entities;

namespace SWKOM_DMS
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configuration for entity models can go here
        }
    }
}
