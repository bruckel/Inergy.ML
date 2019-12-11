using Inergy.ML.Model.Api;
using Microsoft.EntityFrameworkCore;

namespace Inergy.ML.Data.Entity
{
    public class ApiContext : DbContext
    {
        public DbSet<DataReadingMigrationConfig> DataReadingMigrationConfigs { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        { 
        
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataReadingMigrationConfig>(d =>
            {
                d.ToTable("t_DataReadingMigrationConfig").HasNoKey();
                d.Property(p => p.IdDatabase).HasColumnName("idDatabase").IsRequired();
                d.Property(p => p.IdProject).HasColumnName("idProject").IsRequired();
                d.Property(p => p.Cups).HasColumnName("cups").IsRequired();
                d.Property(p => p.BeginTimestamp).HasColumnName("beginTimestamp").IsRequired();
                d.Property(p => p.EndTimestamp).HasColumnName("endTimestamp").IsRequired();
                d.Property(p => p.IdDataReadingType).HasColumnName("idDataReadingType").IsRequired();
            }); 
        }
    }
}
