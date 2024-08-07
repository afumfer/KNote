﻿using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using KNote.Repository.EntityFramework.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using KNote.Model;

namespace KNote.Repository.EntityFramework;

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

    public KntDbContext(DbContextOptions<KntDbContext> options, bool ensureCreated = true)
       : base(options)
    {            
        if(ensureCreated)
            Database.EnsureCreated();            
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.KNoteDbConfigure();
        modelBuilder.Seed();
        base.OnModelCreating(modelBuilder);
    }

    #region Override SaveChanges 

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

    #endregion
}
