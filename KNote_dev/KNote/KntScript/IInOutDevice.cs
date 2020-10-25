using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KntScript
{
    public interface IInOutDevice
    {
        void Print(string str, bool newLine = false);
        bool ReadVars(List<ReadVarItem> readVarItmes);
        void Clear();
        void Show();
        void Hide();
        void SetEmbeddedMode();
        string GetOutContent();
        void LockForm(bool lockFrm);
        void Close();
    }
}
