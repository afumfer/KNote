using KNote.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Repository.EntityFramework
{
    public static class ModelBuilderExtensions
    {

        public static void KNoteDbConfigure(this ModelBuilder modelBuilder)
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

            modelBuilder.Entity<Folder>().HasIndex(_ => _.FolderNumber).IsUnique(true);
            modelBuilder.Entity<Folder>().HasIndex(_ => _.ParentId).IsUnique(false);
            modelBuilder.Entity<Folder>().HasIndex(_ => new { _.Order, _.Name}).IsUnique(false);
            modelBuilder.Entity<Note>().HasIndex(_ => _.NoteNumber).IsUnique(true);
            modelBuilder.Entity<Note>().HasIndex(_ => _.Topic).IsUnique(false);
            modelBuilder.Entity<Note>().HasIndex(_ => new {_.Priority, _.Topic }).IsUnique(false);
            modelBuilder.Entity<Note>().HasIndex(_ => _.InternalTags).IsUnique(false);
            modelBuilder.Entity<KAttribute>().HasIndex(_ => _.Name).IsUnique(true);
            modelBuilder.Entity<KAttribute>().HasIndex(_ => new { _.Order, _.Name }).IsUnique(false);
            modelBuilder.Entity<KAttributeTabulatedValue>().HasIndex(_ => new { _.KAttributeId, _.Value }).IsUnique(true);
            modelBuilder.Entity<NoteKAttribute>().HasIndex(_ => new { _.KAttributeId, _.NoteId }).IsUnique(true);
            modelBuilder.Entity<NoteType>().HasIndex(_ => _.Name).IsUnique(true);
            modelBuilder.Entity<SystemValue>().HasIndex(_ => new { _.Scope, _.Key }).IsUnique(true);
            modelBuilder.Entity<TraceNote>().HasIndex(_ => new { _.FromId, _.ToId }).IsUnique(true);
            modelBuilder.Entity<TraceNoteType>().HasIndex(_ => _.Name).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(_ => _.UserName).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(u => u.EMail).IsUnique(true);
            modelBuilder.Entity<Resource>().HasIndex(_ => new { _.NoteId, _.Name }).IsUnique(true);
        }

        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemValue>().HasData(
                new SystemValue { SystemValueId = Guid.NewGuid(), Scope = "SYSTEM", Key = "APP_VERSION", Value = "0.0.5.2" },
                new SystemValue { SystemValueId = Guid.NewGuid(), Scope = "SYSTEM", Key = "DB_VERSION", Value = "0.0.5.2" }
            );

            var idUser1 = Guid.NewGuid();
            var idUser2 = Guid.NewGuid();

            var passwordSaltDemo = Convert.FromBase64String("rS2A7TGIHC1wXYhvUIZYSAOa/AME+q77z2LMOfEAjw6oERZ3G0+LgrGA5ff+CbpjpwIrpMoyNmoVgTLlKl/KJ+BHMsd8ovMemsiEgS+FLGkPSzb/8kkOTcEgYDfDv9s1WTgAtduT5vgVWWz9XrsqbH6C4yE+I8rhBOc+i/Y3+B8=");
            var passwordHashDemo = Convert.FromBase64String("+OJpwQUcwmvI9gnmyqJO7L1TGzX6CpyniZgFC1zFnTmeRfbTJJ6vZBVm3eo84YclL5mlhaqh7iGPHF2fEDZZxw==");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = idUser1,
                    UserName = "adminKNote",
                    FullName = "Admin KNote",
                    EMail = "admin@mydomain.com",
                    RoleDefinition = "Admin",
                    PasswordSalt = passwordSaltDemo,
                    PasswordHash = passwordHashDemo
                },
                new User
                {
                    UserId = idUser2,
                    UserName = "user1",
                    FullName = "user1 KNote",
                    EMail = "user1@mydomain.com",
                    RoleDefinition = "Public",
                    PasswordSalt = passwordSaltDemo,
                    PasswordHash = passwordHashDemo
                }
            );

            // TODO: Añadir NoteTypes
            //Documentation
            //Work order
            //Reminder

            var idFolder1 = Guid.NewGuid();
            var idFolder2 = Guid.NewGuid();
            var idFolder3 = Guid.NewGuid();

            modelBuilder.Entity<Folder>().HasData(
                new Folder
                {
                    FolderId = idFolder1,
                    FolderNumber = 1,
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Name = "Home"
                },
                new Folder
                {
                    FolderId = idFolder2,
                    FolderNumber = 2,
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Name = "KaNote Documentation",
                    //ParentId = idFolder1
                },
                new Folder
                {
                    FolderId = idFolder3,
                    FolderNumber = 3,
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Name = "Temp",
                }
            );

            modelBuilder.Entity<Note>().HasData(
                new Note
                {
                    NoteId = Guid.NewGuid(),
                    NoteNumber = 3,
                    Topic = "Version History",
                    Script = "printline Version();",
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Description = "Version History .... TODO: ..... ",
                    FolderId = idFolder2,
                    Priority = 80
                },
                new Note
                {
                    NoteId = Guid.NewGuid(),
                    NoteNumber = 2,
                    Topic = "KaNote documentation",
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Description = "KaNote documentation .... TODO: ....",
                    FolderId = idFolder1,
                    Priority = 90
                },
                new Note
                {
                    NoteId = Guid.NewGuid(),
                    NoteNumber = 1,
                    Topic = "Wellcome to KNote",
                    CreationDateTime = DateTime.Now,
                    ModificationDateTime = DateTime.Now,
                    Description = "Wellcome to KNote .... TODO: ....",
                    FolderId = idFolder1,
                    Priority = 100
                }
            );
        }
    }
}
