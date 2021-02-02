using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KntScript;

namespace KNote.ClientWin.Core
{
    public class KNoteScriptLibrary: Library
    {
        private readonly Store _store;

        public KNoteScriptLibrary(Store store) 
        {
            _store = store;
        }

        #region Factory methods for KntScript 

        public FoldersSelectorComponent GetFoldersSelectorComponent()
        {
            return new FoldersSelectorComponent(_store);
        }

        public NotesSelectorComponent GetNotesSelectorComponent()
        {
            return new NotesSelectorComponent(_store);
        }

        public KNoteManagmentComponent GetKNoteManagmentComponent()
        {
            return new KNoteManagmentComponent(_store);
        }

        public MonitorComponent GetMonitorComponent()
        {
            return new MonitorComponent(_store);
        }

        public NoteEditorComponent GetNoteEditorComponent()
        {
            return new NoteEditorComponent(_store);
        }

        public KntScriptConsoleComponent GetKntScriptConsoleComponent()
        {
            return new KntScriptConsoleComponent(_store);
        }

        #endregion

        #region Utils methods

        public bool SendGMailMessage(string fromEmail, string fromName, string fromPwd,
            List<object> toUsers, string subject, string body)
        {
            return SendMailMessage(fromEmail, fromName, fromPwd, toUsers, subject, body, false, 587, "smtp.gmail.com", true);
        }

        public bool SendMailMessage(string fromEmail, string fromName, string fromPwd,
            List<object> toUsers, string subject, string body, bool isBodyHtml,
            int port, string host, bool enbleSsl)
        {
            var msg = new MailMessage();
            var client = new SmtpClient();

            try
            {
                foreach (string s in toUsers)
                    msg.To.Add(s.ToString());

                msg.From = new System.Net.Mail.MailAddress(fromEmail, fromName);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = isBodyHtml;

                client.Credentials = new System.Net.NetworkCredential(fromEmail, fromPwd);
                client.Port = port;
                client.Host = host;
                client.EnableSsl = enbleSsl;

                client.Send(msg);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public DbConnection GetSQLConnection(string connectionString)
        {
            var db =  new SqlConnection(connectionString);
            // db.Open();
            return db;                        
        }

        public void SetParameter(SqlCommand cmd, SqlParameter par)
        {
            cmd.Parameters.Add(par);
        }

        public void Exec(string fileName, string arguments, string userName, System.Security.SecureString password, string domain)
        {
            Exec(fileName, arguments, userName, password, domain, false);
        }

        public void Exec(string fileName, string arguments, string userName, System.Security.SecureString password, string domain, bool showError)
        {
            try
            {
                Process.Start(fileName, arguments, userName, password, domain);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error has occurred: " + ex.Message, "KntScript");
            }
        }

        public void Exec(string fileName, string arguments)
        {
            try
            {
                Process.Start(fileName, arguments);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error has occurred: " + ex.Message, "KntScript");
            }
        }

        #endregion 

        #region Tests and demos


        public float DemoSumNum(float x, float y)
        {
            return x + y;
        }

        public object TestNull()
        {
            return null;
        }

        public void TestMsg()
        {
            MessageBox.Show("TEST KNoteScriptLibrary Method");
        }

        public static void TestStatic()
        {
            MessageBox.Show("Static");
        }

        #endregion

    }

}
