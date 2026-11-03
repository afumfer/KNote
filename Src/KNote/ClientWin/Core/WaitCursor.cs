using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{
    // The wait cursor while it lives. Application.UseWaitCursor keeps it during an await (with only Cursor.Current, the
    // first message of the mouse after the await gave the arrow back); the previous value is restored when nested.
    public class WaitCursor : IDisposable
    {
        private readonly bool _previous;

        public WaitCursor()
        {
            _previous = Application.UseWaitCursor;
            Application.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            Application.UseWaitCursor = _previous;
            Cursor.Current = _previous ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
