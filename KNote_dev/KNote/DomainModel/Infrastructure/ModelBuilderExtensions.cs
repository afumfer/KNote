using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.DomainModel.Infrastructure
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Author>().HasData(
            //    new Author
            //    {
            //        AuthorId = 1,
            //        FirstName = "William",
            //        LastName = "Shakespeare"
            //    }
            //);
            //modelBuilder.Entity<Book>().HasData(
            //    new Book { BookId = 1, AuthorId = 1, Title = "Hamlet" },
            //    new Book { BookId = 2, AuthorId = 1, Title = "King Lear" },
            //    new Book { BookId = 3, AuthorId = 1, Title = "Othello" }
            //);


            //var personas = new List<Persona>();
            //for (int i = 5; i < 100; i++)
            //{
            //    personas.Add(new Persona()
            //    {
            //        Id = i,
            //        Nombre = $"Persona {i}",
            //        FechaNacimiento = DateTime.Today
            //    });
            //}
            //modelBuilder.Entity<Persona>().HasData(personas);

            //var roleAdmin = new IdentityRole()
            ////{ Id = Guid.NewGuid().ToString(), Name = "admin", NormalizedName = "admin" };
            //{ Id = "89086180-b978-4f90-9dbd-a7040bc93f41", Name = "admin", NormalizedName = "admin" };
            //modelBuilder.Entity<IdentityRole>().HasData(roleAdmin);

        }


        //public string TestCreateEntitiesWithContext(string conection, string provider)
        //{
        //    string res = "";

        //    res += TestCreateSystemValuesWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateUsersWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateAttributesWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateFoldersWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateNotesWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateMessagesWithContext(conection, provider) + Environment.NewLine;
        //    res += TestCreateEventsWithContext(conection, provider) + Environment.NewLine;

        //    return res;
        //}

        //public string TestCreateSystemValuesWithContext(string conection, string provider)
        //{
        //    string res = "Create System Value OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            SystemValue s1, s2;

        //            s1 = new SystemValue
        //            {
        //                SystemValueId = NewGuid(),
        //                Key = "OWNER",
        //                Value = "afumfer"
        //            };

        //            s2 = new SystemValue
        //            {
        //                SystemValueId = NewGuid(),
        //                Key = "VERSIONDB",
        //                Value = "0.01"
        //            };

        //            context.SystemValues.Add(s1);
        //            context.SystemValues.Add(s2);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateUsersWithContext(string conection, string provider)
        //{
        //    string res = "Create User OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            User u1 = null, u2 = null;

        //            u1 = new User
        //            {
        //                UserId = NewGuid(),
        //                UserName = "afumfer",
        //                FullName = "afumfer",
        //                EMail = "afumfer@gmail.com"
        //            };

        //            u2 = new User
        //            {
        //                UserId = NewGuid(),
        //                UserName = "afumfer2",
        //                FullName = "afumfer2",
        //                EMail = "afumfer2@gmail.com"
        //            };

        //            context.Users.Add(u1);
        //            context.Users.Add(u2);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateAttributesWithContext(string conection, string provider)
        //{
        //    string res = "Create KAttributes OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            KAttribute a1, a2;
        //            KAttributeTabulatedValue v1, v2, v3;

        //            a1 = new KAttribute
        //            {
        //                KAttributeId = NewGuid(),
        //                Key = "SUBSISTEMA",
        //                Name = "Subistema",
        //                KAttributeDataType = EnumKAttributeDataType.dtTabulate,
        //                Order = 1,
        //            };

        //            a2 = new KAttribute
        //            {
        //                KAttributeId = NewGuid(),
        //                Key = "COMENTARIO",
        //                Name = "Comentario",
        //                KAttributeDataType = EnumKAttributeDataType.dtString,
        //                Order = 2,
        //            };

        //            v1 = new KAttributeTabulatedValue
        //            {
        //                KAttributeTabulatedValueId = NewGuid(),
        //                Key = "KEY1",
        //                Value = "User managment",
        //                Order = 1
        //            };

        //            v2 = new KAttributeTabulatedValue
        //            {
        //                KAttributeTabulatedValueId = NewGuid(),
        //                Key = "KEY2",
        //                Value = "Foders managment",
        //                Order = 2
        //            };

        //            v3 = new KAttributeTabulatedValue
        //            {
        //                KAttributeTabulatedValueId = NewGuid(),
        //                Key = "KEY3",
        //                Value = "Notes managment",
        //                Order = 3
        //            };

        //            a1.KAttributeTabulatedValues = new List<KAttributeTabulatedValue>();

        //            a1.KAttributeTabulatedValues.Add(v1);
        //            a1.KAttributeTabulatedValues.Add(v2);
        //            a1.KAttributeTabulatedValues.Add(v3);

        //            context.KAttributes.Add(a1);
        //            context.KAttributes.Add(a2);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateFoldersWithContext(string conection, string provider)
        //{
        //    string res = "Create Folder OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            Folder f1, f2, f3;
        //            KAttribute a1;
        //            User u1;

        //            a1 = context.KAttributes.Where(a => a.Key == "SUBSISTEMA").First();
        //            u1 = context.Users.Where(u => u.UserName == "afumfer").First();

        //            f1 = new Folder
        //            {
        //                FolderId = NewGuid(),
        //                FolderNumber = 1,
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Name = "Parent Folder 1"
        //            };

        //            f2 = new Folder
        //            {
        //                FolderId = NewGuid(),
        //                FolderNumber = 2,
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Name = "Child Folder 1",
        //                ParentFolder = f1,
        //            };

        //            f3 = new Folder
        //            {
        //                FolderId = NewGuid(),
        //                FolderNumber = 3,
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Name = "Child Folder 1",
        //            };

        //            context.Folders.Add(f1);
        //            context.Folders.Add(f2);
        //            context.Folders.Add(f3);

        //            f1.ChildsFolders.Add(f3);

        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateNotesWithContext(string conection, string provider)
        //{
        //    string res = "Create Notes OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            Note n1, n2, n3;
        //            User u1;
        //            Folder f1, f2;
        //            KAttribute a1, a2;


        //            u1 = context.Users.Where(u => u.UserName == "afumfer").First();
        //            f1 = context.Folders.Where(f => f.FolderNumber == 2).First();
        //            f2 = context.Folders.Where(f => f.FolderNumber == 3).First();
        //            a1 = context.KAttributes.Where(a => a.Key == "SUBSISTEMA").First();
        //            a2 = context.KAttributes.Where(a => a.Key == "COMENTARIO").First();

        //            n1 = new Note
        //            {
        //                NoteId = NewGuid(),
        //                NoteNumber = 1,
        //                Topic = "Note number 1",
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Description = "Description 111, note 1",
        //                Folder = f1,
        //                KAttributes = new List<NoteKAttribute>(),
        //                Resources = new List<Resource>(),
        //                Tasks = new List<NoteTask>(),
        //                Windows = new List<Window>(),
        //                KMessages = new List<KMessage>(),
        //                From = new List<TraceNote>(),
        //                To = new List<TraceNote>()
        //            };
        //            context.Notes.Add(n1);

        //            n2 = new Note
        //            {
        //                NoteId = NewGuid(),
        //                NoteNumber = 2,
        //                Topic = "Note number 2",
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Description = "Description 222, note 2",
        //                Folder = f1,
        //                KAttributes = new List<NoteKAttribute>(),
        //                Resources = new List<Resource>(),
        //                Tasks = new List<NoteTask>(),
        //                Windows = new List<Window>(),
        //                KMessages = new List<KMessage>(),
        //                From = new List<TraceNote>(),
        //                To = new List<TraceNote>()
        //            };
        //            context.Notes.Add(n2);

        //            n3 = new Note
        //            {
        //                NoteId = NewGuid(),
        //                NoteNumber = 3,
        //                Topic = "Note number 3",
        //                Script = "printline Version();",
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Description = "Description 333, note 3",
        //                Folder = f2,
        //                KAttributes = new List<NoteKAttribute>(),
        //                Resources = new List<Resource>(),
        //                Tasks = new List<NoteTask>(),
        //                Windows = new List<Window>(),
        //                KMessages = new List<KMessage>(),
        //                From = new List<TraceNote>(),
        //                To = new List<TraceNote>()
        //            };
        //            context.Notes.Add(n3);

        //            #region Colecs 

        //            n1.KAttributes.Add(new NoteKAttribute
        //            {
        //                NoteKAttributeId = NewGuid(),
        //                KAttribute = a1,
        //                Value = "Valuer for atribute 1"
        //            });

        //            n1.KAttributes.Add(new NoteKAttribute
        //            {
        //                NoteKAttributeId = NewGuid(),
        //                KAttribute = a2,
        //                Value = "This is a comment"
        //            });

        //            n1.Resources.Add(new Resource
        //            {
        //                ResourceId = NewGuid(),
        //                Description = "Resource 1 for note 1",
        //                Path = @"C:\Tmp",
        //                FileMimeType = "text"
        //            });

        //            n1.Tasks.Add(new NoteTask
        //            {
        //                NoteTaskId = NewGuid(),
        //                CreationDateTime = DateTime.Now,
        //                ModificationDateTime = DateTime.Now,
        //                Description = "Task descripcion for note 1",
        //                DifficultyLevel = 1,
        //                User = u1,
        //            });

        //            n1.Windows.Add(new Window
        //            {
        //                WindowId = NewGuid(),
        //                User = u1,
        //                Width = 2000,
        //                Height = 2000,
        //                FontSize = 11,
        //                Visible = true,
        //                ForeColor = 12648447,
        //                TitleColor = 8454143
        //            });

        //            #endregion

        //            n1.To.Add(new TraceNote
        //            {
        //                TraceNoteId = NewGuid(),
        //                To = n2,
        //                Order = 1,
        //                Weight = 0.1
        //            });

        //            n1.To.Add(new TraceNote
        //            {
        //                TraceNoteId = NewGuid(),
        //                To = n3,
        //                Order = 1,
        //                Weight = 0.1
        //            });

        //            n2.To.Add(new TraceNote
        //            {
        //                TraceNoteId = NewGuid(),
        //                To = n3,
        //                Order = 1,
        //                Weight = 0.1
        //            });

        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateMessagesWithContext(string conection, string provider)
        //{
        //    string res = "Create KMessages OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            KMessage m1;
        //            Note n1, n3;
        //            User u1;

        //            n1 = context.Notes.Where(n => n.NoteNumber == 1).First();
        //            n3 = context.Notes.Where(n => n.NoteNumber == 3).First();
        //            u1 = context.Users.Where(u => u.UserName == "afumfer").First();

        //            m1 = new KMessage
        //            {
        //                KMessageId = NewGuid(),
        //                AlarmDateTime = DateTime.Now.AddDays(20),
        //                NotificationType = EnumNotificationType.PostIt,
        //                AlarmType = EnumAlarmType.Standard,
        //                Content = "This is a message for user 1",
        //                AlarmActivated = true,
        //                User = u1,
        //                Note = n1
        //            };

        //            context.KMessages.Add(m1);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}

        //public string TestCreateEventsWithContext(string conection, string provider)
        //{
        //    string res = "Create KEvents OK (Context)";
        //    var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
        //    conn.ConnectionString = conection;

        //    try
        //    {
        //        using (KntDbContext context = new KntDbContext(conn))
        //        {
        //            KEvent e1;
        //            Note n1, n3;
        //            User u1;

        //            n1 = context.Notes.Where(n => n.NoteNumber == 1).First();
        //            n3 = context.Notes.Where(n => n.NoteNumber == 3).First();
        //            u1 = context.Users.Where(u => u.UserName == "afumfer").First();

        //            e1 = new KEvent
        //            {
        //                KEventId = NewGuid(),
        //                EntityId = n1.NoteId,
        //                EntityName = "Note",
        //                EventType = EnumEventType.OnSaveActionDefault,
        //                NoteScriptId = n3.NoteId,
        //                PropertyName = "",
        //                PropertyValue = "",
        //            };

        //            context.KEvents.Add(e1);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res = ex.Message;
        //    }
        //    return res;
        //}




    }
}
