using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

using KntScript;

namespace KNote.ClientWin.Views
{
    public class MyLibrary: Library
    {        
        public List<DocumentDummy> ColecDocDemo()
        {
            List<DocumentDummy> lisRes = new List<DocumentDummy>();

            lisRes.Add(new DocumentDummy(1));
            lisRes.Add(new DocumentDummy(2));
            lisRes.Add(new DocumentDummy(3));
            lisRes.Add(new DocumentDummy(4));
            lisRes.Add(new DocumentDummy(5));
            return lisRes;
        }

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
            MessageBox.Show("TEST MyLibrary Method");
        }

        public static void TestStatic()
        {
            MessageBox.Show("Static");
        }

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
                if (showError == true)
                    MessageBox.Show("The following error has occurred: " + ex.Message, "KntScript");
                else
                    throw ex;
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
                throw ex;
            }
        }

    }

}
