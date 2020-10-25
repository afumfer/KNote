using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace KntScript
{
    public class Library
    {
        public static string AnTS_Version = "0.0.1.5";

        internal IInOutDevice InOutDevice { get; set; }
        
        public int RandomInt(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue);
        }

        public int RandomInt()
        {
            Random random = new Random();
            return random.Next();
        }

        public DateTime DateTimeNow()
        {
            return DateTime.Now;
        }

        public DateTime DateTimeToday()
        {
            return DateTime.Today;
        }

        public Guid NewGuid()
        {
            Guid a = Guid.NewGuid();
            return a;
        }

        public List<object> NewCollectionObjects()
        {
            return new List<object>();            
        }

        public List<string> SplitText(string str, string sepString)
        {            
            string[] sep = { sepString };
            return Split(str, sep);
        }

        public List<string> SplitWords(string str)
        {            
            string[] sep = { " ", "\n", "\r" };
            return Split(str, sep);
        }

        public List<string> SplitLines(string str)
        {
            string[] sep = { "\n", "\r" };
            return Split(str, sep);
        }

        private List<string> Split(string str, string[] sep)
        {
            List<string> outWords = new List<string>();            
            string[] a = str.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in a)
                outWords.Add(s);

            return outWords;
        }

        public string GetOutContent()
        {
            if (InOutDevice != null)
                return InOutDevice.GetOutContent();
            else
                return "ERROR: the output device is not linked to the function library.";
        }

        public void ShowOutWindow()
        {
            InOutDevice.Show();
        }

        public void HideOutWindow()
        {
            InOutDevice.Hide();
        }

        public void CloseOutWindow()
        {
            InOutDevice.Close();
        }
    }
}
