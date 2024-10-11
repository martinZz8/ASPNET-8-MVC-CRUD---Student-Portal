using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StudentPortal.Web.Models.Entities;
using StudentPortal.Web.Models.Entities.Interfaces;

namespace StudentPortal.Web.DbContexts
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options): base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -- Write some options of entity creations default SQL values --
            modelBuilder.Entity<Student>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .Property(u => u.Phone)
                .IsRequired(false);

            //modelBuilder.Entity<Student>()
            //    .Property(b => b.CreationDate)
            //    .HasDefaultValueSql("getutcdate()"); //getdate()

            modelBuilder.Entity<Photo>()
                .Property(u => u.OriginalFileName)
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }

        // Auto fill of entities "CreationDate" and "UpdateDate" (after creation or update of any entit)
        // based on: https://stackoverflow.com/questions/37285948/how-to-set-created-date-and-modified-date-to-enitites-in-db-first-approach
        public override int SaveChanges()
        {
            HandleCreationAndUpdateDateForEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            HandleCreationAndUpdateDateForEntities();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void HandleCreationAndUpdateDateForEntities()
        {
            DateTime nowDate = DateTime.UtcNow;

            foreach (EntityEntry changedEntity in ChangeTracker.Entries())
            {
                if (changedEntity.Entity is IEntityWithDates entity)
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            entity.CreateDate = nowDate;
                            entity.UpdateDate = nowDate;
                            break;
                        case EntityState.Modified:
                            Entry(entity).Property(x => x.CreateDate).IsModified = false;
                            entity.UpdateDate = nowDate;
                            break;
                    }
                }
            }
        }
    }
}
