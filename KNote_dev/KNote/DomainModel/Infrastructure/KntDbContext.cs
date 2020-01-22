using Microsoft.EntityFrameworkCore;
using System.Data.Common;

using KNote.DomainModel.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using KNote.Shared;

namespace KNote.DomainModel.Infrastructure
{
    public class KntDbContext: DbContext
    {
        public DbSet<SystemValue> SystemValues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<KAttribute> KAttributes { get; set; }
        public DbSet<NoteKAttribute> NoteKAttributes { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<NoteTask> NoteTasks { get; set; }
        public DbSet<Window> Windows { get; set; }
        public DbSet<TraceNote> TraceNotes { get; set; }
        public DbSet<KMessage> KMessages { get; set; }
        public DbSet<KEvent> KEvents { get; set; }        
        public DbSet<KAttributeTabulatedValue> KAttributeTabulatedValues { get; set; }
        public DbSet<KLog> KLogs { get; set; }        
        public DbSet<NoteType> NoteTypes { get; set; }
        public DbSet<TraceNoteType> TraceNoteTypes { get; set; }

        public KntDbContext()
           : base()
        {
        }

        public KntDbContext(DbContextOptions<KntDbContext> options)
           : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        // TODO: !!! Pendiente de refactorizar o eliminar 
        public KntDbContext(DbConnection conn)
            //: base(conn, true)
        {
            //Database.SetInitializer<KntDbContext>(new DbInitializer());
        }


        // TODO: !!! Pendiente de eliminar, ver si con Sqlite hace falta algo similar a esto  (Se usaba en compact SQL) 
        public int? KntCommandTimeout 
        {
            // TODO: esta propiedad no es válida para EntityFramwork
            //       se debe disparar una excepción cuando se intente mofificar
            //get { return Database.CommandTimeout; }
            //set { Database.CommandTimeout = value; }

            get; set;
        }

        // TODO: !!! Pendiente de eliminar, ver si con Sqlite hace falta algo similar a esto  (Se usaba en compact SQL) 
        public bool KntLazyLoadingEnabled
        {
            //get { return this.Configuration.LazyLoadingEnabled; }
            //set { this.Configuration.LazyLoadingEnabled = value; }

            get; set;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Folder>()
                .HasMany(_ => _.Notes)
                .WithOne(_ => _.Folder)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KAttribute>()
                .HasMany(_ => _.NoteAttributes)
                .WithOne(_ => _.KAttribute)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            //modelBuilder.Entity<TraceNote>()
            //    .WithOne(_ => _.To)
            //    .HasMany(_ => _.From)                
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<TraceNote>()
            //   .HasRequired(_ => _.From)
            //   .WithMany(_ => _.To)
            //   .WillCascadeOnDelete(false);               
            modelBuilder.Entity<Note>()
                .HasMany(_ => _.To)
                .WithOne(_ => _.From)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Note>()
                .HasMany(_ => _.From)
                .WithOne(_ => _.To)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------
            //modelBuilder.Entity<Window>()
            //   .HasRequired(_ => _.User)
            //   .WithMany(_ => _.Windows)
            //   .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Window>()
            //   .HasRequired(_ => _.Note)
            //   .WithMany(_ => _.Windows)
            //   .WillCascadeOnDelete(false);

            modelBuilder.Entity<Window>()
                .HasOne(_ => _.User)
                .WithMany(_ => _.Windows)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Window>()
                .HasOne(_ => _.Note)
                .WithMany(_ => _.Windows)
                .OnDelete(DeleteBehavior.Cascade);

            // --------------------
            //modelBuilder.Entity<NoteTask>()
            //   .HasRequired(_ => _.Note)
            //   .WithMany(_ => _.Tasks)
            //   .WillCascadeOnDelete(false);

            //modelBuilder.Entity<NoteTask>()
            //   .HasRequired(_ => _.User)
            //   .WithMany(_ => _.Tasks)
            //   .WillCascadeOnDelete(false);

            modelBuilder.Entity<Note>()
               .HasMany(_ => _.NoteTasks)
               .WithOne(_ => _.Note)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
               .HasMany(_ => _.Tasks)
               .WithOne(_ => _.User)
               .OnDelete(DeleteBehavior.Restrict);


            // modelBuilder.Entity<GeneroPelicula>().HasKey(x => new { x.Field1, x.Field2 });

            modelBuilder.Entity<Folder>().HasIndex(_ => _.FolderNumber).IsUnique(true);
            modelBuilder.Entity<Folder>().HasIndex(_ => _.ParentId).IsUnique(false);
            modelBuilder.Entity<Note>().HasIndex(_ => _.NoteNumber).IsUnique(true);
            modelBuilder.Entity<Note>().HasIndex(_ => _.Topic).IsUnique(false);
            modelBuilder.Entity<Note>().HasIndex(_ => _.InternalTags).IsUnique(false);
            modelBuilder.Entity<KAttribute>().HasIndex(_ => _.Key).IsUnique(true);
            modelBuilder.Entity<KAttributeTabulatedValue>().HasIndex(_ => _.Key).IsUnique(true);
            modelBuilder.Entity<NoteKAttribute>().HasIndex(_ => new { _.KAttributeId, _.NoteId } ).IsUnique(true);
            modelBuilder.Entity<NoteType>().HasIndex(_ => _.Key).IsUnique(true);
            modelBuilder.Entity<SystemValue>().HasIndex(_ => _.Key).IsUnique(true);
            modelBuilder.Entity<TraceNote>().HasIndex(_ => new { _.FromId, _.ToId } ).IsUnique(true);
            modelBuilder.Entity<TraceNoteType>().HasIndex(_ => _.Key).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(_ => _.UserName).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(u => u.EMail).IsUnique(true);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var errors = GetKntEntityValidationsInfo();

            if (errors.Count > 0)
                throw new KntEntityValidationException(errors);
            else
                return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var errors = GetKntEntityValidationsInfo();

            if (errors.Count > 0)
                throw new KntEntityValidationException(errors);
            else
                return await base.SaveChangesAsync(cancellationToken);
            
            //return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private List<KntEntityValidationInfo> GetKntEntityValidationsInfo()
        {
            var errors = new List<KntEntityValidationInfo>();

            var changedEntities = ChangeTracker
                .Entries<IValidatableObject>()
                .Where(_ => _.State == EntityState.Added ||
                            _.State == EntityState.Modified);

            foreach (var errProp in changedEntities)
            {
                var errorsEntity = new List<ValidationResult>();

                var vc = new ValidationContext(errProp.Entity, null, null);
                Validator.TryValidateObject(
                    errProp.Entity, vc, errorsEntity, validateAllProperties: true);

                foreach (var errModel in errProp.Entity.Validate(null))
                    errorsEntity.Add(errModel);

                if(errorsEntity.Count > 0)
                    errors.Add(new KntEntityValidationInfo(errProp.Entity.ToString(), errorsEntity));
            }

            return errors;
        }

    }
}
