using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    // Dummy classes for tests

    public class DocumentDummy
    {
        public float IdDocument { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public DateTime CreationDateTime { get; set; }
        public FolderDummy Folder { get; set; }
        public myEnum Type { get; set; }

        public DocumentDummy()
        {
            IdDocument = 1;
            Topic = string.Empty;
            CreationDateTime = DateTime.Now;
            Folder = new FolderDummy();
            Folder.Name = "Default Folder ";
            Description = "___ () " + IdDocument.ToString();
            Type = myEnum.TypeOne;
        }

        public DocumentDummy(int id)
        {
            IdDocument = id;
            Topic = string.Empty;
            CreationDateTime = DateTime.Now;
            Folder = new FolderDummy();
            Folder.Name = "Default Folder - constructor id";
            Description = "___  (id) " + IdDocument.ToString();
            Type = myEnum.TypeTwo;
        }

        public DocumentDummy(FolderDummy folder)
        {
            IdDocument = 1;
            Topic = string.Empty;
            Description = string.Empty;
            CreationDateTime = DateTime.Now;
            Folder = folder;
            Description = "___ (folder)";
            Type = myEnum.TypeThree;
        }

        public override string ToString()
        {
            return IdDocument.ToString() + " : " + Topic.ToString() + " : " + Description.ToString();
        }

        public float DocumentTestMethodA(string param)
        {
            MessageBox.Show("param = " + param);
            return 9;
        }

        public void DocumentTestMethodB(object param)
        {
            Form a = new Form();
            a.Show();
        }

        public string DocumentTestMethodB(string param)
        {
            return ">> " + param;
        }
    }

    public class FolderDummy
    {
        public int IdFolder { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }        

        public FolderDummy()
        {
            IdFolder = 1;
            Name = "Folder name: (default)";
            Comments = string.Empty;
        }

        public string FolderTestMethodB(string param)
        {
            return ">>> " + param;
        }

        public override string ToString()
        {
            return IdFolder.ToString() + " : " + Name.ToString() + " : " + Comments.ToString();
        }
    }

    public static class TestStaticClass
    {
        public static string TestPropStatic2 { get; set; }

        public static void TestStatic2()
        {
            MessageBox.Show("Static");
        }

        public static void TestStatic2(string msg)
        {
            MessageBox.Show("Static: " + msg);
        }      
    }

    public enum myEnum
    { 
        TypeOne,
        TypeTwo,
        TypeThree,
        TypeFour,
        TypeFive
    }
}
